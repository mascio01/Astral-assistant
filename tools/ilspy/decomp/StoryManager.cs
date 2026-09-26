using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class StoryManager : IReflectable
{
	public interface IPerObjectFunc
	{
		void Call(BaseObject obj, MemoryParam param, ref int intParam, ref float floatParam);
	}

	private struct EvaluateReplyPerObjectFunc : IPerObjectFunc
	{
		public Character Speaker;

		public Character Target;

		public List<SpeechWithCachedPriority> Results;

		public Speech Reply;

		public bool IgnoreDisabled;

		public void Call(BaseObject obj2, MemoryParam param2, ref int intParam, ref float floatParam)
		{
			Instance.EvaluateReply(Speaker, Target, obj2, Results, Reply, param2, IgnoreDisabled, ref intParam, recursing: true);
		}
	}

	private struct EvaluateSpeechOptionPerObjectFunc : IPerObjectFunc
	{
		public Character Speaker;

		public Character Target;

		public List<SpeechWithCachedPriority> Results;

		public Speech Speech;

		public bool IgnoreIfHasNoReplies;

		public bool IgnoreDisabled;

		public void Call(BaseObject obj2, MemoryParam param2, ref int intParam, ref float floatParam)
		{
			Instance.EvaluateSpeechOption(Speaker, Target, obj2, Results, Speech, param2, IgnoreIfHasNoReplies, IgnoreDisabled, recursing: true);
		}
	}

	public class EvaluateSpeechOptionsTaskData : BaseTaskData
	{
		public int StartIndex;

		public List<SpeechWithCachedPriority> Results = new List<SpeechWithCachedPriority>();

		public ManualResetEvent FinishedEvent = new ManualResetEvent(initialState: true);

		public static Character Speaker;

		public static Character Target;

		public static BaseObject Obj;

		public static SpeechSituation Situation;

		public static MemoryParam Param;

		public static bool IgnoreIfHasNoReplies;

		public static bool IgnoreDisabled;
	}

	private class SortSpeechOptionsByPriorityDescending : IComparer<SpeechWithCachedPriority>
	{
		int IComparer<SpeechWithCachedPriority>.Compare(SpeechWithCachedPriority a, SpeechWithCachedPriority b)
		{
			if (a.CachedPriority < b.CachedPriority)
			{
				return 1;
			}
			if (a.CachedPriority > b.CachedPriority)
			{
				return -1;
			}
			if (a.BaseOrder < b.BaseOrder)
			{
				return -1;
			}
			if (a.BaseOrder > b.BaseOrder)
			{
				return 1;
			}
			return 0;
		}
	}

	private class SortInterestingSpeakersDescending : IComparer<InterestingSpeaker>
	{
		int IComparer<InterestingSpeaker>.Compare(InterestingSpeaker a, InterestingSpeaker b)
		{
			if (a.Importance < b.Importance)
			{
				return 1;
			}
			if (a.Importance > b.Importance)
			{
				return -1;
			}
			if (a.FinishedCountdown != -1 && b.FinishedCountdown == -1)
			{
				return 1;
			}
			if (a.FinishedCountdown == -1 && b.FinishedCountdown != -1)
			{
				return -1;
			}
			return 0;
		}
	}

	public static int[] BoxerWagerAmount = new int[6] { 1, 5, 10, 25, 50, 100 };

	public static StoryManager Instance;

	public Dictionary<int, QuestInstance> QuestInstances = new Dictionary<int, QuestInstance>();

	public List<QuestInstance> ActiveQuests = new List<QuestInstance>();

	public List<InvaderInstance> ActiveInvaders = new List<InvaderInstance>();

	public List<Zone> Zones = new List<Zone>();

	public List<EnabledTrigger>[] EnabledTriggers = new List<EnabledTrigger>[16];

	public List<EnabledTrigger> OneShotTriggersUsed = new List<EnabledTrigger>();

	public List<PlayerStartPoint> PlayerStartPoints = new List<PlayerStartPoint>();

	public List<TileObject> QuestDestinations = new List<TileObject>();

	public List<QueuedEvent> QueuedEvents = new List<QueuedEvent>();

	public MineralType SentGeologicalMap = MineralType.None;

	public Vector3 SentMapCamPosition = Vector3.zero;

	public List<Variable> Variables = new List<Variable>();

	private List<string> SpokenOnce = new List<string>();

	private List<InterestingSpeaker> InterestingSpeakers = new List<InterestingSpeaker>();

	public Character DramaticDeathCharacter;

	public TimeSpan DramaticDeathLastRagdollTime;

	private bool WantEvaluateConditions;

	private TimeSpan LastEverySecondTriggerTime;

	private QuestInstance _currentQuest;

	private QuestInstance _sentCurrentQuest;

	private static TimeSpan OneSecond = TimeSpan.FromSeconds(1.0);

	private static string EvaluateRepliesStr = "EvaluateReplies";

	private static List<Character> TempMembers = new List<Character>();

	private static string EvaluateReplyStr = "EvaluateReply";

	private static string EvaluateSpeechOptionStr = "EvaluateSpeechOptionStr";

	private static List<EvaluateSpeechOptionsTaskData> TaskData = new List<EvaluateSpeechOptionsTaskData>();

	private static List<Speech> SpeechesToEvaluate;

	private static int CurTaskDataIndex;

	private static int MaxPerThread = 20;

	public static TaskFunc EvaluateSpeechOptionsOnThreadFunc = EvaluateSpeechOptionsOnThread;

	public static bool EvaluateSpeechOptionsOnThreadEnabled = true;

	public static bool IsEvaluatingSpeechOptionsOnThread = false;

	private static List<string> UsedSpeechIDs = new List<string>();

	private SortSpeechOptionsByPriorityDescending SpeechOptionSorter = new SortSpeechOptionsByPriorityDescending();

	public static List<SpeechWithCachedPriority> _tmpSpeechOptions = new List<SpeechWithCachedPriority>();

	private static string GetSpeechForSituationStr = "GetSpeechForSituation";

	private static List<InvaderInstance> TempInvaderInstances = new List<InvaderInstance>();

	private static SortInterestingSpeakersDescending InterestingSpeakerSorter = new SortInterestingSpeakersDescending();

	public QuestInstance CurrentQuest
	{
		get
		{
			return _currentQuest;
		}
		set
		{
			if (_currentQuest != value)
			{
				_currentQuest = value;
				RefreshQuestMarkers();
				if (InfoScreen.Instance.IsShowingQuests())
				{
					InfoScreen.Instance.WantRepopulate = true;
				}
			}
		}
	}

	public StoryManager()
	{
		Instance = this;
		for (int i = 0; i < EnabledTriggers.Length; i++)
		{
			EnabledTriggers[i] = new List<EnabledTrigger>();
		}
	}

	public void Unload()
	{
		Instance = null;
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 246)
		{
			List<Trigger> list = new List<Trigger>();
			reflector.AddTriggerList(ref list);
			for (int i = 0; i < list.Count; i++)
			{
				EnabledTrigger item = new EnabledTrigger
				{
					Trigger = list[i]
				};
				EnabledTriggers[(int)item.Trigger.Type].Add(item);
			}
		}
		else
		{
			List<EnabledTrigger> list2 = new List<EnabledTrigger>();
			if (reflector.IsSerialising)
			{
				for (int j = 0; j < EnabledTriggers.Length; j++)
				{
					list2.AddRange(EnabledTriggers[j]);
				}
			}
			reflector.Add(ref list2);
			if (reflector.IsDeserialising)
			{
				for (int k = 0; k < list2.Count; k++)
				{
					if ((reflector.Version >= 294 || list2[k].QuestInstance == null) && list2[k].Trigger != null)
					{
						EnabledTriggers[(int)list2[k].Trigger.Type].Add(list2[k]);
					}
				}
				foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
				{
					if (questInstance.Value.State == QuestInstance.EState.Active)
					{
						EnableTriggersForQuest(questInstance.Value);
					}
				}
			}
		}
		reflector.AddAfter(ref OneShotTriggersUsed, 470);
		reflector.AddAfter(ref LastEverySecondTriggerTime, 293);
		if (reflector.Version >= 248)
		{
			reflector.Add(ref ActiveInvaders);
		}
		reflector.Add(ref QueuedEvents);
		reflector.Add(ref Variables);
		reflector.AddStringList(ref SpokenOnce);
		reflector.Add(ref InterestingSpeakers);
		reflector.Add(ref DramaticDeathCharacter);
		reflector.Add(ref DramaticDeathLastRagdollTime);
		if (reflector.Version < 136)
		{
			TerrainCoord value = TerrainCoord.Invalid;
			reflector.AddAfter(ref value, 17);
		}
	}

	public void FixupOnLoad(Session session, Reflector reflector)
	{
		PlayerRecord localPlayerRecord = session.GetLocalPlayerRecord();
		_currentQuest = localPlayerRecord.SyncedCurrentQuest;
		MapPage.Instance.GeologicalMap = localPlayerRecord.SyncedGeologicalMap;
		MapPage.Instance.CamPosition = localPlayerRecord.SyncedMapCamPosition;
		RefreshQuestMarkers();
	}

	public void OnLoad(int version)
	{
		for (int num = ActiveInvaders.Count - 1; num >= 0; num--)
		{
			InvaderInstance invaderInstance = ActiveInvaders[num];
			if (invaderInstance.CreatedObjects.Count == 0 && !invaderInstance.Active)
			{
				ActiveInvaders.RemoveAt(num);
			}
			else
			{
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					if (ActiveInvaders[num].Invader != null && ActiveInvaders[num2].Invader != null && ActiveInvaders[num].Invader.UniqueID == ActiveInvaders[num2].Invader.UniqueID && ActiveInvaders[num].SourceObject == ActiveInvaders[num2].SourceObject)
					{
						ActiveInvaders.RemoveAt(num);
						break;
					}
				}
			}
		}
		foreach (Story currentStory in GameImpl.Instance.CurrentStories)
		{
			foreach (KeyValuePair<string, Script> script in currentStory.Scripts)
			{
				foreach (Trigger trigger in script.Value.Triggers)
				{
					if (trigger.StartEnabled && version < 617)
					{
						EnableTrigger(trigger.UniqueID, null, null, null);
					}
					if (trigger.Type == TriggerType.GameLoad && AreAllConditionsSatisfied(trigger.Conditions, null, null, null, default(MemoryParam)))
					{
						QueueEvents(trigger.Events, null, null, null, default(MemoryParam));
					}
				}
			}
		}
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		if (_sentCurrentQuest != _currentQuest)
		{
			inputFrame.AddAction(InputAction.SetCurrentQuest(_currentQuest));
			_sentCurrentQuest = _currentQuest;
		}
		if (SentGeologicalMap != MapPage.Instance.GeologicalMap)
		{
			inputFrame.AddAction(InputAction.SetGeologicalMap(MapPage.Instance.GeologicalMap));
			SentGeologicalMap = MapPage.Instance.GeologicalMap;
		}
		if ((SentMapCamPosition - MapPage.Instance.CamPosition).sqrMagnitude > 1f)
		{
			inputFrame.AddAction(InputAction.SetMapCamPos(MapPage.Instance.CamPosition));
			SentMapCamPosition = MapPage.Instance.CamPosition;
		}
	}

	public void SetConditionsDirty()
	{
		WantEvaluateConditions = true;
		Hud.Instance.Cursor.WantRefreshSpeechOptions = true;
	}

	public void OnNewGame()
	{
		foreach (Story currentStory in GameImpl.Instance.CurrentStories)
		{
			foreach (KeyValuePair<string, Script> script in currentStory.Scripts)
			{
				foreach (Trigger trigger in script.Value.Triggers)
				{
					if (trigger.StartEnabled)
					{
						EnableTrigger(trigger.UniqueID, null, null, null);
					}
					if (trigger.Type == TriggerType.GameStart && AreAllConditionsSatisfied(trigger.Conditions, null, null, null, default(MemoryParam)))
					{
						QueueEvents(trigger.Events, null, null, null, default(MemoryParam));
					}
				}
			}
		}
	}

	public bool HasQueuedEventsForThisFrame()
	{
		for (int i = 0; i < QueuedEvents.Count; i++)
		{
			if (Session.Instance.PlayTime - QueuedEvents[i].QueuedTime >= TimeSpan.FromSeconds(QueuedEvents[i].Event.Delay))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasQueuedBoxingMatchEvents(Character actor)
	{
		for (int i = 0; i < QueuedEvents.Count; i++)
		{
			if (QueuedEvents[i].Actor == actor)
			{
				StoryEventType type = QueuedEvents[i].Event.Type;
				if ((uint)(type - 73) <= 1u)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasQueuedShakedownEvents()
	{
		for (int i = 0; i < QueuedEvents.Count; i++)
		{
			StoryEventType type = QueuedEvents[i].Event.Type;
			if ((uint)(type - 49) <= 2u)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasQueuedEventsOfType(StoryEventType storyEventType)
	{
		for (int i = 0; i < QueuedEvents.Count; i++)
		{
			if (QueuedEvents[i].Event.Type == storyEventType)
			{
				return true;
			}
		}
		return false;
	}

	public void Update()
	{
		Session instance = Session.Instance;
		for (int i = 0; i < InterestingSpeakers.Count; i++)
		{
			if (InterestingSpeakers[i].FinishedCountdown >= 0)
			{
				if (InterestingSpeakers[i].FinishedCountdown >= 30)
				{
					InterestingSpeakers.RemoveAt(i);
					i--;
				}
				else
				{
					InterestingSpeakers[i] = InterestingSpeaker.Create(InterestingSpeakers[i].Speaker, InterestingSpeakers[i].Listener, InterestingSpeakers[i].FinishedCountdown + 1, InterestingSpeakers[i].Importance);
				}
			}
		}
		if (WantEvaluateConditions)
		{
			WantEvaluateConditions = false;
			foreach (Zone zone in Zones)
			{
				zone.Evaluate();
			}
			for (int j = 0; j < ActiveQuests.Count; j++)
			{
				QuestInstance questInstance = ActiveQuests[j];
				if (questInstance.Quest != null)
				{
					if (questInstance.EvaluateIsFailed())
					{
						questInstance.Fail();
						j--;
					}
					else if (questInstance.EvaluateIsCompleted())
					{
						questInstance.Complete();
						j--;
					}
				}
			}
			foreach (Story currentStory in GameImpl.Instance.CurrentStories)
			{
				foreach (Quest questsWithStartCondition in currentStory.QuestsWithStartConditions)
				{
					if (!IsQuestDiscovered(questsWithStartCondition.UniqueID, null, null, null) && AreAllConditionsSatisfied(questsWithStartCondition.StartConditions, null, null, null, default(MemoryParam)))
					{
						DiscoverQuest(questsWithStartCondition.UniqueID, null, null, null, canRediscover: false);
					}
				}
				foreach (Invader invadersWithStartCondition in currentStory.InvadersWithStartConditions)
				{
					if (GameImpl.Instance.FindInvaderByUniqueID(invadersWithStartCondition.UniqueID) == invadersWithStartCondition)
					{
						InvaderInstance invaderInstance = GetInvaderInstance(invadersWithStartCondition, null);
						if (invaderInstance == null && AreAllConditionsSatisfied(invadersWithStartCondition.StartConditions, null, null, null, default(MemoryParam)))
						{
							ActivateInvader(invadersWithStartCondition, null, out var _);
						}
						else if (invaderInstance != null && !AreAllConditionsSatisfied(invadersWithStartCondition.StartConditions, null, null, null, default(MemoryParam)))
						{
							DeactivateInvader(invaderInstance);
						}
					}
				}
			}
			TriggerEnabledTriggersOfType(TriggerType.Normal, null, null);
			RefreshQuestMarkers();
		}
		if (instance.PlayTime - LastEverySecondTriggerTime >= OneSecond)
		{
			TriggerEnabledTriggersOfType(TriggerType.EverySecond, null, null);
			LastEverySecondTriggerTime = instance.PlayTime;
		}
		TriggerQueuedEvents();
		for (int num = ActiveInvaders.Count - 1; num >= 0; num--)
		{
			InvaderInstance invaderInstance2 = ActiveInvaders[num];
			if (invaderInstance2.Invader != null && invaderInstance2.Invader.MaxTimeoutDays > 0f && Session.Instance.PlayTime >= invaderInstance2.TriggeredTime + invaderInstance2.CalcTimeout())
			{
				DeactivateInvader(invaderInstance2);
			}
		}
		if (DramaticDeathCharacter != null)
		{
			if (!DramaticDeathCharacter.Deleted && DramaticDeathCharacter.CurrentAnimState == AnimState.Ragdoll)
			{
				DramaticDeathLastRagdollTime = Session.Instance.PlayTime;
			}
			if (Session.Instance.PlayTime - DramaticDeathLastRagdollTime >= TimeSpan.FromSeconds(2.0) || DramaticDeathCharacter.Disappeared)
			{
				DramaticDeathCharacter = null;
			}
		}
	}

	public void TriggerQueuedEvents()
	{
		Session instance = Session.Instance;
		for (int i = 0; i < QueuedEvents.Count; i++)
		{
			if (instance.PlayTime - QueuedEvents[i].QueuedTime >= TimeSpan.FromSeconds(QueuedEvents[i].Event.Delay))
			{
				QueuedEvents[i].Event.TriggerEvent(QueuedEvents[i].Actor, QueuedEvents[i].Target, QueuedEvents[i].Obj, QueuedEvents[i].Param);
				QueuedEvents.RemoveAt(i);
				i--;
			}
		}
	}

	public static void QueueEvents(List<StoryEvent> events, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		if (events != null)
		{
			for (int i = 0; i < events.Count; i++)
			{
				Instance.QueuedEvents.Add(QueuedEvent.Create(events[i], actor, target, obj, param, Session.Instance.PlayTime));
			}
		}
	}

	public static bool AreAnyConditionsSatisfied(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AreAllConditionsSatisfied(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (!conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static float GetConditionsSum(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		float num = 0f;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					num += conditions[i].EvaluatePriority(actor, target, obj, param);
				}
			}
		}
		return num;
	}

	public static float GetConditionsMultiply(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		float num = 1f;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					num *= conditions[i].EvaluatePriority(actor, target, obj, param);
				}
			}
		}
		return num;
	}

	public static float GetConditionsMin(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		float num = float.MaxValue;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					num = Math.Min(num, conditions[i].EvaluatePriority(actor, target, obj, param));
				}
			}
		}
		return num;
	}

	public static float GetConditionsMax(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		float num = float.MinValue;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					num = Math.Max(num, conditions[i].EvaluatePriority(actor, target, obj, param));
				}
			}
		}
		return num;
	}

	public static bool DiscoverPersonalityFromConditions(List<Condition> conditions, List<Condition> priorityTerms, bool not, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		bool flag = false;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				switch (conditions[i].Type)
				{
				case ConditionType.HasPersonality:
				case ConditionType.HasUndiscoveredPersonality:
					if (!(conditions[i].Not ^ not))
					{
						conditions[i].GetSubject<Character>(actor, target, obj, param)?.SetPersonalityKnown(conditions[i].StringData);
						flag = true;
					}
					break;
				case ConditionType.And:
				case ConditionType.Or:
				case ConditionType.IsConditionBlockSatisfied:
				{
					if (conditions[i].ConditionBlocks == null)
					{
						break;
					}
					for (int j = 0; j < conditions[i].ConditionBlocks.Count; j++)
					{
						ConditionBlock conditionBlock = conditions[i].ConditionBlocks[j];
						if (conditionBlock != null)
						{
							flag |= DiscoverPersonalityFromConditions(conditionBlock.Conditions, null, conditions[i].Not ^ not, actor, target, obj, param);
						}
					}
					break;
				}
				}
			}
		}
		if (priorityTerms != null)
		{
			for (int k = 0; k < priorityTerms.Count; k++)
			{
				switch (priorityTerms[k].Type)
				{
				case ConditionType.HasPersonality:
				case ConditionType.HasUndiscoveredPersonality:
					if (!(priorityTerms[k].Not ^ (priorityTerms[k].PriorityFactor < 0f) ^ not))
					{
						priorityTerms[k].GetSubject<Character>(actor, target, obj, param)?.SetPersonalityKnown(priorityTerms[k].StringData);
						flag = true;
					}
					break;
				case ConditionType.And:
				case ConditionType.Or:
				case ConditionType.IsConditionBlockSatisfied:
				{
					if (priorityTerms[k].ConditionBlocks == null)
					{
						break;
					}
					for (int l = 0; l < priorityTerms[k].ConditionBlocks.Count; l++)
					{
						ConditionBlock conditionBlock2 = priorityTerms[k].ConditionBlocks[l];
						if (conditionBlock2 != null)
						{
							flag |= DiscoverPersonalityFromConditions(conditionBlock2.Conditions, null, priorityTerms[k].Not ^ (priorityTerms[k].PriorityFactor < 0f) ^ not, actor, target, obj, param);
						}
					}
					break;
				}
				}
			}
		}
		return flag;
	}

	public static bool IsPersonalityCondition(List<Condition> conditions, int index, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		switch (conditions[index].Type)
		{
		case ConditionType.HasPersonality:
		case ConditionType.HasUndiscoveredPersonality:
			return conditions[index].GetSubject<Character>(actor, target, obj, param) == actor;
		case ConditionType.And:
		case ConditionType.Or:
		case ConditionType.IsConditionBlockSatisfied:
		{
			if (conditions[index].ConditionBlocks == null)
			{
				break;
			}
			for (int i = 0; i < conditions[index].ConditionBlocks.Count; i++)
			{
				ConditionBlock conditionBlock = conditions[index].ConditionBlocks[i];
				if (conditionBlock == null || conditionBlock.Conditions == null)
				{
					continue;
				}
				for (int j = 0; j < conditionBlock.Conditions.Count; j++)
				{
					if (IsPersonalityCondition(conditionBlock.Conditions, j, actor, target, obj, param))
					{
						return true;
					}
				}
			}
			break;
		}
		}
		return false;
	}

	public static bool AreAllSpeechConditionsSatisfied(List<Condition> conditions, Character actor, Character target, BaseObject obj, MemoryParam param, out bool lacksPersonality)
	{
		lacksPersonality = false;
		if (conditions != null)
		{
			for (int i = 0; i < conditions.Count; i++)
			{
				if (!conditions[i].IsConditionSatisfied(actor, target, obj, param))
				{
					if (!IsPersonalityCondition(conditions, i, actor, target, obj, param))
					{
						return false;
					}
					lacksPersonality = true;
				}
			}
		}
		return true;
	}

	public static float CalcSpeechPriority(Character speaker, Character target, BaseObject obj, Speech speech, MemoryParam param, out bool visibleButDisabled, bool forOpinionGraph)
	{
		float num = 0f;
		visibleButDisabled = false;
		if (speech.Once && Instance.HasSpokenOnce(speech))
		{
			return float.MinValue;
		}
		if (speech.RepeatTime != 0f && speaker.HasSpokenSpeechRecently(speech, target, speech.RepeatTime))
		{
			return float.MinValue;
		}
		if (speech.Conditions != null && (!AreAllSpeechConditionsSatisfied(speech.Conditions, speaker, target, obj, param, out visibleButDisabled) || (visibleButDisabled && forOpinionGraph)))
		{
			return float.MinValue;
		}
		bool flag = false;
		if (speech.PriorityTerms != null)
		{
			for (int i = 0; i < speech.PriorityTerms.Count; i++)
			{
				if (!speech.PriorityTerms[i].IsConditionSatisfied(speaker, target, obj, param))
				{
					if (speech.PriorityTerms[i].Type == ConditionType.HasPersonality && speech.PriorityTerms[i].GetSubject<Character>(speaker, target, obj, param) == speaker)
					{
						flag = true;
					}
				}
				else
				{
					num += speech.PriorityTerms[i].EvaluatePriority(speaker, target, obj, param);
				}
			}
		}
		if (num < 0f && flag)
		{
			visibleButDisabled = true;
		}
		return num;
	}

	private void EvaluateReplies(Character speaker, Character target, BaseObject obj, MemoryParam param, List<SpeechWithCachedPriority> results, List<SpeechRef> replies, bool ignoreDisabled, string includeExtraRepliesTo)
	{
		using (new UnityProfileMarker(EvaluateRepliesStr))
		{
			int order = 0;
			if (replies != null)
			{
				for (int i = 0; i < replies.Count; i++)
				{
					EvaluateReply(speaker, target, obj, results, replies[i], param, ignoreDisabled, ref order, recursing: false);
				}
			}
			if (!string.IsNullOrEmpty(includeExtraRepliesTo))
			{
				foreach (Story currentStory in GameImpl.Instance.CurrentStories)
				{
					List<Speech> value = null;
					currentStory.ExtraRepliesTo.TryGetValue(includeExtraRepliesTo, out value);
					if (value == null)
					{
						continue;
					}
					foreach (Speech item in value)
					{
						EvaluateReply(speaker, target, obj, results, item, param, ignoreDisabled, ref order, recursing: false);
					}
				}
			}
			results.InsertionSort(SpeechOptionSorter);
		}
	}

	private List<Character> BuildListOfCandidateMembers(Character speaker, Character target, Community community, bool includeListener)
	{
		List<Character> list = (Util.AmIOnMainThread() ? TempMembers : new List<Character>());
		list.Clear();
		foreach (Character member in community.Members)
		{
			if (member != speaker && (member != target || includeListener) && member.AliveAndNotZombie && !member.Disappeared)
			{
				list.Add(member);
			}
		}
		return list;
	}

	public bool EvaluatePerObjectType(Character speaker, Character target, BaseObject obj, MemoryParam param, Speech reply, IPerObjectFunc func, ref int intParam, ref float floatParam)
	{
		switch (reply.PerObjectType)
		{
		case SpeechPerObjectType.PerMemory:
			if (!string.IsNullOrEmpty(reply.PerMemory))
			{
				for (int n = 0; n < speaker.Memories.Count; n++)
				{
					if (speaker.Memories[n].Prototype.UniqueID == reply.PerMemory)
					{
						func.Call(speaker.Memories[n].Object, new MemoryParam(speaker, n), ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerRelationship:
		{
			for (int num2 = 0; num2 < speaker.Relationships.Count; num2++)
			{
				if ((speaker.Relationships[num2].RelationshipType == reply.PerRelationship || (reply.PerRelationship == RelationshipType.None && speaker.Relationships[num2].RelationshipType != RelationshipType.Ex && speaker.Relationships[num2].RelationshipType != RelationshipType.InLoveWith && speaker.Relationships[num2].RelationshipType != RelationshipType.NotInLoveWith)) && (speaker.Relationships[num2].RelationshipTarget != target || reply.IncludeListener) && speaker.Relationships[num2].RelationshipTarget != null && speaker.Relationships[num2].RelationshipTarget.AliveAndNotZombie && !speaker.Relationships[num2].RelationshipTarget.Disappeared)
				{
					func.Call(speaker.Relationships[num2].RelationshipTarget, param, ref intParam, ref floatParam);
				}
			}
			return true;
		}
		case SpeechPerObjectType.PerRomanticRelationship:
		{
			for (int m = 0; m < speaker.Relationships.Count; m++)
			{
				if ((speaker.Relationships[m].RelationshipType == RelationshipType.SleepingWith || speaker.Relationships[m].RelationshipType == RelationshipType.MarriedTo) && (speaker.Relationships[m].RelationshipTarget != target || reply.IncludeListener) && speaker.Relationships[m].RelationshipTarget != null && speaker.Relationships[m].RelationshipTarget.AliveAndNotZombie && !speaker.Relationships[m].RelationshipTarget.Disappeared)
				{
					func.Call(speaker.Relationships[m].RelationshipTarget, param, ref intParam, ref floatParam);
				}
			}
			return true;
		}
		case SpeechPerObjectType.PerCommunityMember:
			if (speaker.Community != null)
			{
				foreach (Character member in speaker.Community.Members)
				{
					if (member != speaker && (member != target || reply.IncludeListener) && member.AliveAndNotZombie && !member.Disappeared)
					{
						func.Call(member, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerCommunityMemberSample:
			if (speaker.Community != null)
			{
				List<Character> list3 = BuildListOfCandidateMembers(speaker, target, speaker.Community, reply.IncludeListener);
				if (list3.Count > 0)
				{
					Character obj4 = list3[MathUtil.RandomInt(target.Id * 123456 + speaker.Id * 789 + Session.Instance.Frame, list3.Count)];
					func.Call(obj4, param, ref intParam, ref floatParam);
				}
				list3.Clear();
				list3 = null;
			}
			return true;
		case SpeechPerObjectType.PerListenerCommunityMember:
			if (target != null && target.Community != null)
			{
				foreach (Character member2 in target.Community.Members)
				{
					if (member2 != speaker && (member2 != target || reply.IncludeListener) && member2.AliveAndNotZombie && !member2.Disappeared)
					{
						func.Call(member2, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerListenerCommunityMemberSample:
			if (target != null && target.Community != null)
			{
				List<Character> list2 = BuildListOfCandidateMembers(speaker, target, target.Community, reply.IncludeListener);
				if (list2.Count > 0)
				{
					Character obj3 = list2[MathUtil.RandomInt(target.Id * 123456 + speaker.Id * 789 + Session.Instance.Frame, list2.Count)];
					func.Call(obj3, param, ref intParam, ref floatParam);
				}
				list2.Clear();
				list2 = null;
			}
			return true;
		case SpeechPerObjectType.PerReferringToCommunityMember:
			if (obj != null)
			{
				Community community = obj.GetCommunity();
				if (community != null)
				{
					foreach (Character member3 in community.Members)
					{
						if (member3 != speaker && (member3 != target || reply.IncludeListener) && member3.AliveAndNotZombie && !member3.Disappeared)
						{
							func.Call(member3, param, ref intParam, ref floatParam);
						}
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerCarriedBody:
			if (speaker.CarryingObject != null)
			{
				func.Call(speaker.CarryingObject, param, ref intParam, ref floatParam);
			}
			return true;
		case SpeechPerObjectType.PerKnownCommunity:
			if (speaker.Community != null)
			{
				for (int k = 0; k < speaker.Community.CommunityRelationships.Count; k++)
				{
					Community otherCommunity2 = speaker.Community.CommunityRelationships[k].GetOtherCommunity();
					if (otherCommunity2 != null && speaker.Community.CommunityRelationships[k].RelationshipType >= CommunityRelationshipType.Known && otherCommunity2.HasAnyLivingNonZombieMembers())
					{
						func.Call(otherCommunity2, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerListenerAlliedCommunity:
			if (target != null && target.Community != null)
			{
				foreach (Community cachedAlly in target.Community.CachedAllies)
				{
					if (cachedAlly.HasAnyLivingNonZombieMembers())
					{
						func.Call(cachedAlly, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerEnemyCommunity:
			if (speaker.Community != null)
			{
				for (int l = 0; l < speaker.Community.CommunityRelationships.Count; l++)
				{
					Community otherCommunity3 = speaker.Community.CommunityRelationships[l].GetOtherCommunity();
					if (otherCommunity3 != null && speaker.Community.CommunityRelationships[l].RelationshipType == CommunityRelationshipType.Hostile && otherCommunity3.HasAnyLivingNonZombieMembers())
					{
						func.Call(otherCommunity3, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerEnemyCommunityMemberSample:
			if (speaker.Community != null)
			{
				for (int j = 0; j < speaker.Community.CommunityRelationships.Count; j++)
				{
					Community otherCommunity = speaker.Community.CommunityRelationships[j].GetOtherCommunity();
					if (otherCommunity != null && speaker.Community.CommunityRelationships[j].RelationshipType == CommunityRelationshipType.Hostile && otherCommunity.HasAnyLivingNonZombieMembers())
					{
						List<Character> list = BuildListOfCandidateMembers(speaker, target, otherCommunity, reply.IncludeListener);
						if (list.Count > 0)
						{
							int num = target?.Id ?? 0;
							Character obj2 = list[MathUtil.RandomInt(num * 123456 + speaker.Id * 789 + Session.Instance.Frame, list.Count)];
							func.Call(obj2, param, ref intParam, ref floatParam);
						}
						list.Clear();
						list = null;
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerLooterCommunityMemberSample:
			if (speaker.Community != null)
			{
				foreach (Community community2 in Session.Instance.CommunityManager.Communities)
				{
					if (community2.CommunityType == CommunityType.Looter && community2.HasAnyLivingNonZombieMembers())
					{
						List<Character> list4 = BuildListOfCandidateMembers(speaker, target, community2, reply.IncludeListener);
						if (list4.Count > 0)
						{
							int num3 = target?.Id ?? 0;
							Character obj5 = list4[MathUtil.RandomInt(num3 * 123456 + speaker.Id * 789 + Session.Instance.Frame, list4.Count)];
							func.Call(obj5, param, ref intParam, ref floatParam);
						}
						list4.Clear();
						list4 = null;
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuest:
			foreach (QuestInstance activeQuest in ActiveQuests)
			{
				if (activeQuest.Quest != null && (string.IsNullOrEmpty(reply.PerStringData) || activeQuest.Quest.UniqueID == reply.PerStringData))
				{
					func.Call(activeQuest, param, ref intParam, ref floatParam);
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuestGiver:
			foreach (QuestInstance activeQuest2 in ActiveQuests)
			{
				if (activeQuest2.Quest != null && (string.IsNullOrEmpty(reply.PerStringData) || activeQuest2.Quest.UniqueID == reply.PerStringData))
				{
					func.Call(activeQuest2.QuestGiver, param, ref intParam, ref floatParam);
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuestSeeker:
			foreach (QuestInstance activeQuest3 in ActiveQuests)
			{
				if (activeQuest3.Quest != null && (string.IsNullOrEmpty(reply.PerStringData) || activeQuest3.Quest.UniqueID == reply.PerStringData))
				{
					func.Call(activeQuest3.QuestSeeker, param, ref intParam, ref floatParam);
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuestObject:
			foreach (QuestInstance activeQuest4 in ActiveQuests)
			{
				if (activeQuest4.Quest != null && (string.IsNullOrEmpty(reply.PerStringData) || activeQuest4.Quest.UniqueID == reply.PerStringData))
				{
					func.Call(activeQuest4.QuestObject, param, ref intParam, ref floatParam);
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuestReferringToTarget:
			if (target != null)
			{
				foreach (QuestInstance activeQuest5 in ActiveQuests)
				{
					if (activeQuest5.Quest != null && (activeQuest5.QuestObject == target || (target.GetCommunity() != null && activeQuest5.QuestObject == target.GetCommunity()) || (activeQuest5.QuestObject is Equipment && target.InventoryContains((Equipment)activeQuest5.QuestObject))))
					{
						func.Call(activeQuest5.QuestGiver, param, ref intParam, ref floatParam);
					}
				}
			}
			return true;
		case SpeechPerObjectType.PerActiveQuestGivenByTarget:
			foreach (QuestInstance activeQuest6 in ActiveQuests)
			{
				if (activeQuest6.Quest != null && activeQuest6.QuestGiver == target)
				{
					func.Call(activeQuest6.QuestObject, param, ref intParam, ref floatParam);
				}
			}
			return true;
		case SpeechPerObjectType.PerUniqueID:
			if (!string.IsNullOrEmpty(reply.PerStringData))
			{
				BaseObject objectByUniqueID = BaseObjectManager.Instance.GetObjectByUniqueID(reply.PerStringData);
				func.Call(objectByUniqueID, param, ref intParam, ref floatParam);
			}
			return true;
		case SpeechPerObjectType.PerSpeakerPickpocketedItem:
			if (speaker != null && speaker.PickpocketedItems != null)
			{
				for (int i = 0; i < speaker.PickpocketedItems.Count; i++)
				{
					if (speaker.PickpocketedItems[i].Liquid != null)
					{
						func.Call(obj, new MemoryParam(speaker.PickpocketedItems[i].Liquid), ref intParam, ref floatParam);
					}
					else
					{
						func.Call(obj, new MemoryParam(speaker.PickpocketedItems[i].Type), ref intParam, ref floatParam);
					}
				}
			}
			return true;
		default:
			return false;
		}
	}

	private void EvaluateReply(Character speaker, Character target, BaseObject obj, List<SpeechWithCachedPriority> results, Speech reply, MemoryParam param, bool ignoreDisabled, ref int order, bool recursing)
	{
		using (new UnityProfileMarker(EvaluateReplyStr))
		{
			if (reply == null)
			{
				return;
			}
			if (!recursing && reply.PerObjectType != SpeechPerObjectType.None)
			{
				EvaluateReplyPerObjectFunc evaluateReplyPerObjectFunc = default(EvaluateReplyPerObjectFunc);
				evaluateReplyPerObjectFunc.Speaker = speaker;
				evaluateReplyPerObjectFunc.Target = target;
				evaluateReplyPerObjectFunc.Results = results;
				evaluateReplyPerObjectFunc.Reply = reply;
				evaluateReplyPerObjectFunc.IgnoreDisabled = ignoreDisabled;
				float floatParam = 0f;
				EvaluatePerObjectType(speaker, target, obj, param, reply, evaluateReplyPerObjectFunc, ref order, ref floatParam);
			}
			else
			{
				bool visibleButDisabled;
				float priority = CalcSpeechPriority(speaker, target, obj, reply, param, out visibleButDisabled, forOpinionGraph: false);
				SpeechWithCachedPriority item = new SpeechWithCachedPriority(reply, priority, order++, speaker, obj, param, visibleButDisabled);
				if ((!ignoreDisabled || item.Enabled) && item.Visible)
				{
					results.Add(item);
				}
			}
		}
	}

	private void EvaluateSpeechOption(Character speaker, Character target, BaseObject obj, List<SpeechWithCachedPriority> results, Speech speech, MemoryParam param, bool ignoreIfHasNoReplies, bool ignoreDisabled, bool recursing)
	{
		using (new UnityProfileMarker(EvaluateSpeechOptionStr))
		{
			if (speech == null)
			{
				return;
			}
			if (!recursing && speech.PerObjectType != SpeechPerObjectType.None)
			{
				EvaluateSpeechOptionPerObjectFunc evaluateSpeechOptionPerObjectFunc = default(EvaluateSpeechOptionPerObjectFunc);
				evaluateSpeechOptionPerObjectFunc.Speaker = speaker;
				evaluateSpeechOptionPerObjectFunc.Target = target;
				evaluateSpeechOptionPerObjectFunc.Results = results;
				evaluateSpeechOptionPerObjectFunc.Speech = speech;
				evaluateSpeechOptionPerObjectFunc.IgnoreIfHasNoReplies = ignoreIfHasNoReplies;
				evaluateSpeechOptionPerObjectFunc.IgnoreDisabled = ignoreDisabled;
				int intParam = 0;
				float floatParam = 0f;
				EvaluatePerObjectType(speaker, target, obj, param, speech, evaluateSpeechOptionPerObjectFunc, ref intParam, ref floatParam);
				return;
			}
			bool visibleButDisabled;
			float priority = CalcSpeechPriority(speaker, target, obj, speech, param, out visibleButDisabled, forOpinionGraph: false);
			SpeechWithCachedPriority item = new SpeechWithCachedPriority(speech, priority, results.Count, speaker, obj, param, visibleButDisabled);
			if (!item.Visible)
			{
				return;
			}
			if (ignoreIfHasNoReplies && item.Speech.Events == null && item.Speech.UninterruptibleEvents == null)
			{
				MemoryParam param2 = param;
				MemoryParam param3 = param;
				if (GetContinue(speaker, target, obj, item.Speech, out var _, ref param2, MathUtil.NonDeterministicRand) == null && GetReply(target, speaker, obj, item.Speech, out var _, ref param3, out var _, MathUtil.NonDeterministicRand) == null)
				{
					return;
				}
			}
			if (!ignoreDisabled || item.Enabled)
			{
				results.Add(item);
			}
		}
	}

	public static void GetSpeechesToEvaluate(SpeechSituation situation, List<Speech> speechesToEvaluate)
	{
		int num = -1;
		UsedSpeechIDs.Clear();
		speechesToEvaluate.Clear();
		for (int num2 = GameImpl.Instance.CurrentStories.Count - 1; num2 >= 0; num2--)
		{
			List<Speech> list = GameImpl.Instance.CurrentStories[num2].SpeechesBySituation[(int)situation];
			if (list != null)
			{
				foreach (Speech item in list)
				{
					if (num < 0 || UsedSpeechIDs.LastIndexOf(item.UniqueID, num) == -1)
					{
						UsedSpeechIDs.Add(item.UniqueID);
						speechesToEvaluate.Add(item);
					}
				}
			}
			num = UsedSpeechIDs.Count - 1;
		}
	}

	private void EvaluateSpeechOptions(Character speaker, Character target, BaseObject obj, List<SpeechWithCachedPriority> results, SpeechSituation situation, MemoryParam param, bool ignoreIfHasNoReplies)
	{
		EvaluateSpeechOptions(speaker, target, obj, results, situation, param, ignoreIfHasNoReplies, ignoreDisabled: false);
	}

	private void EvaluateSpeechOptions(Character speaker, Character target, BaseObject obj, List<SpeechWithCachedPriority> results, SpeechSituation situation, MemoryParam param, bool ignoreIfHasNoReplies, bool ignoreDisabled)
	{
		int count = results.Count;
		SpeechesToEvaluate = GameImpl.Instance.SpeechesForSituation[(int)situation];
		if (SpeechesToEvaluate.Count >= MaxPerThread && EvaluateSpeechOptionsOnThreadEnabled)
		{
			IsEvaluatingSpeechOptionsOnThread = true;
			EvaluateSpeechOptionsTaskData.Speaker = speaker;
			EvaluateSpeechOptionsTaskData.Target = target;
			EvaluateSpeechOptionsTaskData.Obj = obj;
			EvaluateSpeechOptionsTaskData.Situation = situation;
			EvaluateSpeechOptionsTaskData.Param = param;
			EvaluateSpeechOptionsTaskData.IgnoreIfHasNoReplies = ignoreIfHasNoReplies;
			EvaluateSpeechOptionsTaskData.IgnoreDisabled = ignoreDisabled;
			CurTaskDataIndex = 0;
			for (int i = 0; i < SpeechesToEvaluate.Count; i += MaxPerThread)
			{
				if (CurTaskDataIndex >= TaskData.Count)
				{
					EvaluateSpeechOptionsTaskData evaluateSpeechOptionsTaskData = new EvaluateSpeechOptionsTaskData();
					evaluateSpeechOptionsTaskData.StartIndex = i;
					TaskData.Add(evaluateSpeechOptionsTaskData);
				}
				EvaluateSpeechOptionsTaskData evaluateSpeechOptionsTaskData2 = TaskData[CurTaskDataIndex];
				evaluateSpeechOptionsTaskData2.FinishedEvent.Reset();
				GameImpl.Instance.UpdateThreadPool.AddTask(EvaluateSpeechOptionsOnThreadFunc, null, evaluateSpeechOptionsTaskData2, TaskPriority.High);
				CurTaskDataIndex++;
			}
			for (int j = 0; j < CurTaskDataIndex; j++)
			{
				if (!TaskData[j].FinishedEvent.WaitOne(1000))
				{
					EvaluateSpeechOptionsOnThreadEnabled = false;
					Debug.LogWarning("Error in threaded evaluate speech options? " + j + "/" + SpeechesToEvaluate.Count);
				}
				for (int k = 0; k < TaskData[j].Results.Count; k++)
				{
					SpeechWithCachedPriority item = TaskData[j].Results[k];
					item.BaseOrder = results.Count;
					results.Add(item);
				}
				TaskData[j].Results.Clear();
			}
			EvaluateSpeechOptionsTaskData.Speaker = null;
			EvaluateSpeechOptionsTaskData.Target = null;
			EvaluateSpeechOptionsTaskData.Obj = null;
			EvaluateSpeechOptionsTaskData.Param = default(MemoryParam);
			IsEvaluatingSpeechOptionsOnThread = false;
		}
		else
		{
			foreach (Speech item2 in SpeechesToEvaluate)
			{
				EvaluateSpeechOption(speaker, target, obj, results, item2, param, ignoreIfHasNoReplies, ignoreDisabled, recursing: false);
			}
		}
		if (results.Count > count)
		{
			results.InsertionSort(count, results.Count - count, SpeechOptionSorter);
		}
	}

	private static void EvaluateSpeechOptionsOnThread(BaseTaskData data)
	{
		EvaluateSpeechOptionsTaskData evaluateSpeechOptionsTaskData = (EvaluateSpeechOptionsTaskData)data;
		try
		{
			int num = Math.Min(evaluateSpeechOptionsTaskData.StartIndex + MaxPerThread, SpeechesToEvaluate.Count);
			for (int i = evaluateSpeechOptionsTaskData.StartIndex; i < num; i++)
			{
				Speech speech = SpeechesToEvaluate[i];
				Instance.EvaluateSpeechOption(EvaluateSpeechOptionsTaskData.Speaker, EvaluateSpeechOptionsTaskData.Target, EvaluateSpeechOptionsTaskData.Obj, evaluateSpeechOptionsTaskData.Results, speech, EvaluateSpeechOptionsTaskData.Param, EvaluateSpeechOptionsTaskData.IgnoreIfHasNoReplies, EvaluateSpeechOptionsTaskData.IgnoreDisabled, recursing: false);
			}
			evaluateSpeechOptionsTaskData.Results.InsertionSort(Instance.SpeechOptionSorter);
		}
		catch (Exception ex)
		{
			Debug.Log("Error in EvaluateSpeechOptionsOnThread: " + ex.Message + ex.StackTrace);
		}
		evaluateSpeechOptionsTaskData.FinishedEvent.Set();
	}

	public bool GetReferringToAndMemoryParamForSpeech(Character speaker, Character listener, Speech speech, out BaseObject resultObj, out MemoryParam param, CustomRandom rand)
	{
		resultObj = null;
		param = default(MemoryParam);
		if (speech.PerObjectType == SpeechPerObjectType.None)
		{
			return true;
		}
		EvaluateSpeechOption(speaker, listener, null, _tmpSpeechOptions, speech, param, ignoreIfHasNoReplies: false, ignoreDisabled: false, recursing: false);
		int i;
		for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority; i++)
		{
		}
		if (i > 0)
		{
			int index = rand.Next(i);
			resultObj = _tmpSpeechOptions[index].CachedObject;
			param = _tmpSpeechOptions[index].CachedMemoryParam;
		}
		_tmpSpeechOptions.Clear();
		return i > 0;
	}

	public Speech GetReply(Character replier, Character originalSpeaker, BaseObject obj, Speech speech, out BaseObject resultObj, ref MemoryParam param, out bool multipleOptions, CustomRandom rand)
	{
		Speech result = null;
		resultObj = null;
		EvaluateReplies(replier, originalSpeaker, obj, param, _tmpSpeechOptions, speech.Replies, ignoreDisabled: false, speech.UniqueID);
		multipleOptions = _tmpSpeechOptions.Count > 1;
		for (int num = _tmpSpeechOptions.Count - 1; num >= 0; num--)
		{
			if (!_tmpSpeechOptions[num].Enabled)
			{
				_tmpSpeechOptions.RemoveAt(num);
			}
		}
		int i;
		for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority; i++)
		{
		}
		if (i > 0)
		{
			int index = rand.Next(i);
			result = _tmpSpeechOptions[index].Speech;
			resultObj = _tmpSpeechOptions[index].CachedObject;
			param = _tmpSpeechOptions[index].CachedMemoryParam;
		}
		_tmpSpeechOptions.Clear();
		return result;
	}

	public Speech GetContinue(Character speaker, Character target, BaseObject obj, Speech speech, out BaseObject resultObj, ref MemoryParam param, CustomRandom rand)
	{
		Speech result = null;
		resultObj = null;
		EvaluateReplies(speaker, target, obj, param, _tmpSpeechOptions, speech.Continues, ignoreDisabled: true, null);
		int i;
		for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority; i++)
		{
		}
		if (i > 0)
		{
			int index = rand.Next(i);
			result = _tmpSpeechOptions[index].Speech;
			resultObj = _tmpSpeechOptions[index].CachedObject;
			param = _tmpSpeechOptions[index].CachedMemoryParam;
		}
		_tmpSpeechOptions.Clear();
		return result;
	}

	public Speech GetNoReply(Character speaker, Character target, BaseObject obj, Speech speech, out BaseObject resultObj, ref MemoryParam param, CustomRandom rand)
	{
		Speech result = null;
		resultObj = null;
		EvaluateReplies(speaker, target, obj, param, _tmpSpeechOptions, speech.NoReply, ignoreDisabled: true, null);
		int i;
		for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority; i++)
		{
		}
		if (i > 0)
		{
			int index = rand.Next(i);
			result = _tmpSpeechOptions[index].Speech;
			resultObj = _tmpSpeechOptions[index].CachedObject;
			param = _tmpSpeechOptions[index].CachedMemoryParam;
		}
		_tmpSpeechOptions.Clear();
		return result;
	}

	public Speech PickSpeech(Character speaker, Character target, BaseObject obj, List<SpeechRef> speeches, out BaseObject resultObj, ref MemoryParam param, CustomRandom rand)
	{
		Speech result = null;
		resultObj = null;
		EvaluateReplies(speaker, target, obj, param, _tmpSpeechOptions, speeches, ignoreDisabled: true, null);
		int i;
		for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority; i++)
		{
		}
		if (i > 0)
		{
			int index = rand.Next(i);
			result = _tmpSpeechOptions[index].Speech;
			resultObj = _tmpSpeechOptions[index].CachedObject;
			param = _tmpSpeechOptions[index].CachedMemoryParam;
		}
		_tmpSpeechOptions.Clear();
		return result;
	}

	public bool DoesSpeechHaveReplies(Speech speech)
	{
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.NoReplyChoice)
		{
			return false;
		}
		if (speech.SpecialBehaviour == SpecialSpeechBehaviour.Gossip)
		{
			return false;
		}
		if (speech.Replies != null && speech.Replies.Count > 0)
		{
			return true;
		}
		foreach (Story currentStory in GameImpl.Instance.CurrentStories)
		{
			List<Speech> value = null;
			currentStory.ExtraRepliesTo.TryGetValue(speech.UniqueID, out value);
			if (value != null && value.Count > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool GetSpeechOptions(Character speaker, Character target, List<SpeechWithCachedPriority> speechOptions, Speech lastSpeechToReplyTo, BaseObject lastSpeechObj, MemoryParam lastSpeechParam)
	{
		if (speaker == null)
		{
			return false;
		}
		if (lastSpeechToReplyTo != null && DoesSpeechHaveReplies(lastSpeechToReplyTo))
		{
			EvaluateReplies(speaker, target, lastSpeechObj, lastSpeechParam, speechOptions, lastSpeechToReplyTo.Replies, ignoreDisabled: false, lastSpeechToReplyTo.UniqueID);
			return true;
		}
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskForBandage, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskForAntigen, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.OfferBandage, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.OfferAntigen, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.OfferFood, default(MemoryParam), ignoreIfHasNoReplies: false);
		if (target.IsControllableByPlayer())
		{
			if (speaker.IsPlayerAvatar())
			{
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Swap, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestTopic, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestInfo, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Gossip, default(MemoryParam), ignoreIfHasNoReplies: true);
				if (target.IsTooDepressedToFollowOrders())
				{
					EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Encourage, default(MemoryParam), ignoreIfHasNoReplies: false);
				}
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskBackground, default(MemoryParam), ignoreIfHasNoReplies: true);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskMorale, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskOpinion, default(MemoryParam), ignoreIfHasNoReplies: false);
				if (CanOfferGifts(speaker, target, checkIfLeader: true))
				{
					EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.OfferGift, default(MemoryParam), ignoreIfHasNoReplies: false);
				}
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Rumor, default(MemoryParam), ignoreIfHasNoReplies: false);
				GetLoveTriangleSpeechOptions(speaker, target, speechOptions);
				if (Session.Instance.FollowerCommandsEnabled && !target.DontLeaveCommunity)
				{
					EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.KickFromCommunity, default(MemoryParam), ignoreIfHasNoReplies: false);
				}
			}
			else
			{
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Swap, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestTopic, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestInfo, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskMorale, default(MemoryParam), ignoreIfHasNoReplies: false);
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskOpinion, default(MemoryParam), ignoreIfHasNoReplies: false);
			}
			return false;
		}
		bool num = target.CanFollowPlayerIncludeAllies();
		if (!num && target.Community != null && target.Community.CanOpenPlayerGates == CanOpenGates.Unknown)
		{
			Squad squad = target.GetSquad();
			if (squad == null || !squad.CanForceOpenPlayerGates())
			{
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.GateKeeping, default(MemoryParam), ignoreIfHasNoReplies: false);
			}
		}
		bool flag = false;
		int num2 = 0 | ((speaker.GetHunger() >= Character.HungerCriticalTime) ? 1 : 0);
		flag |= speaker.GetThirst() >= Character.ThirstCriticalTime;
		if (num2 != 0)
		{
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskForFood, default(MemoryParam), ignoreIfHasNoReplies: false);
		}
		if (flag)
		{
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskForDrink, default(MemoryParam), ignoreIfHasNoReplies: false);
		}
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestTopic, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.QuestInfo, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskName, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Gossip, default(MemoryParam), ignoreIfHasNoReplies: true);
		if (target.Rank != Rank.Captive)
		{
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.LookingForWork, default(MemoryParam), ignoreIfHasNoReplies: true);
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Fisticuffs, default(MemoryParam), ignoreIfHasNoReplies: false);
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Trade, default(MemoryParam), ignoreIfHasNoReplies: false);
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.WarnAboutTraps, default(MemoryParam), ignoreIfHasNoReplies: true);
		}
		if (num && target.IsTooDepressedToFollowOrders())
		{
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Encourage, default(MemoryParam), ignoreIfHasNoReplies: false);
		}
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskBackground, default(MemoryParam), ignoreIfHasNoReplies: true);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskMorale, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskOpinion, default(MemoryParam), ignoreIfHasNoReplies: false);
		if (CanOfferGifts(speaker, target, checkIfLeader: true))
		{
			EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.OfferGift, default(MemoryParam), ignoreIfHasNoReplies: false);
		}
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.AskToJoinCommunity, default(MemoryParam), ignoreIfHasNoReplies: false);
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Rumor, default(MemoryParam), ignoreIfHasNoReplies: false);
		GetLoveTriangleSpeechOptions(speaker, target, speechOptions);
		if (!num && target.Community != null && target.Community.CanOpenPlayerGates != CanOpenGates.Unknown)
		{
			Squad squad2 = target.GetSquad();
			if (squad2 == null || !squad2.CanForceOpenPlayerGates())
			{
				EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.GateKeeping, default(MemoryParam), ignoreIfHasNoReplies: false);
			}
		}
		EvaluateSpeechOptions(speaker, target, null, speechOptions, SpeechSituation.Threat, default(MemoryParam), ignoreIfHasNoReplies: false);
		return false;
	}

	private void GetLoveTriangleSpeechOptions(Character speaker, Character target, List<SpeechWithCachedPriority> speechOptions)
	{
		Character partner = Relationship.GetPartner(target);
		if (partner == null || !partner.AliveAndNotZombie || !Relationship.IsPartnerKnownToPlayer(target) || target.Community == speaker.Community)
		{
			return;
		}
		for (int i = 0; i < partner.Relationships.Count; i++)
		{
			RelationshipType relationshipType = partner.Relationships[i].RelationshipType;
			if ((uint)(relationshipType - 7) <= 1u)
			{
				Character relationshipTarget = partner.Relationships[i].RelationshipTarget;
				if (relationshipTarget != null && relationshipTarget.AliveAndNotZombie && Relationship.IsKnownToPlayer(relationshipTarget, partner))
				{
					EvaluateSpeechOptions(speaker, target, relationshipTarget, speechOptions, SpeechSituation.LoveTriangle, default(MemoryParam), ignoreIfHasNoReplies: false);
				}
			}
		}
	}

	public static bool CanOfferGifts(Character speaker, Character target, bool checkIfLeader)
	{
		if (target == null)
		{
			return false;
		}
		if (!target.Alive)
		{
			return false;
		}
		if (target.IsControllableByPlayer() && checkIfLeader && !speaker.IsPlayerAvatar())
		{
			return false;
		}
		if (speaker.Inventory.GetNumGiftableItems() == 0)
		{
			return false;
		}
		return true;
	}

	public static TimeSpan GetSpeechTime(string text)
	{
		return TimeSpan.FromSeconds(1f + (float)text.Length * 0.05f);
	}

	public static float GetSpeechLipsMoveTime(string text)
	{
		return (float)GetSpeechTime(text).TotalSeconds / 2f;
	}

	public Speech GetSpeechForMemory(Character speaker, Character target, MemoryParam param)
	{
		if (param.GetMemory().Actor == target)
		{
			Speech speechForSituation = GetSpeechForSituation(speaker, target, param.GetMemory().Object, SpeechSituation.MemoryOfYou, param);
			if (speechForSituation != null)
			{
				return speechForSituation;
			}
		}
		return GetSpeechForSituation(speaker, target, param.GetMemory().Object, SpeechSituation.Memory, param);
	}

	public Speech GetSpeechForSituation(Character speaker, Character target, SpeechSituation sit)
	{
		return GetSpeechForSituation(speaker, target, null, sit, default(MemoryParam));
	}

	public Speech GetSpeechForSituation(Character speaker, Character target, BaseObject obj, SpeechSituation sit)
	{
		return GetSpeechForSituation(speaker, target, ref obj, sit, default(MemoryParam));
	}

	public Speech GetSpeechForSituation(Character speaker, Character target, BaseObject obj, SpeechSituation sit, MemoryParam param)
	{
		return GetSpeechForSituation(speaker, target, ref obj, sit, param);
	}

	public Speech GetSpeechForSituation(Character speaker, Character target, ref BaseObject obj, SpeechSituation sit, MemoryParam param)
	{
		return GetSpeechForSituation(speaker, target, ref obj, sit, ref param);
	}

	public Speech GetSpeechForSituation(Character speaker, Character target, ref BaseObject obj, SpeechSituation sit, ref MemoryParam param)
	{
		using (new UnityProfileMarker(GetSpeechForSituationStr))
		{
			EvaluateSpeechOptions(speaker, target, obj, _tmpSpeechOptions, sit, param, ignoreIfHasNoReplies: false, ignoreDisabled: true);
			Speech result = null;
			int i;
			for (i = 0; i < _tmpSpeechOptions.Count && _tmpSpeechOptions[i].CachedPriority == _tmpSpeechOptions[0].CachedPriority && _tmpSpeechOptions[i].Enabled; i++)
			{
			}
			if (i > 0)
			{
				int index = Session.Instance.DeterministicRand.Next(i);
				result = _tmpSpeechOptions[index].Speech;
				obj = _tmpSpeechOptions[index].CachedObject;
				param = _tmpSpeechOptions[index].CachedMemoryParam;
			}
			else
			{
				obj = null;
			}
			_tmpSpeechOptions.Clear();
			return result;
		}
	}

	public void RebuildQuestInstances()
	{
		Dictionary<int, QuestInstance> dictionary = new Dictionary<int, QuestInstance>();
		foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
		{
			QuestInstance value = questInstance.Value;
			value.Hash = QuestInstance.CalcHash(value.Quest, value.QuestGiver, value.QuestSeeker, value.QuestObject);
			dictionary[questInstance.Value.Hash] = value;
		}
		QuestInstances = dictionary;
	}

	public QuestInstance GetQuestInstance(Quest quest, Character giver, Character seeker, BaseObject ob)
	{
		int key = QuestInstance.CalcHash(quest, giver, seeker, ob);
		QuestInstance value = null;
		QuestInstances.TryGetValue(key, out value);
		return value;
	}

	public QuestInstance GetOrCreateQuestInstance(Quest quest, Character giver, Character seeker, BaseObject ob, bool canSkip)
	{
		QuestInstance questInstance = GetQuestInstance(quest, giver, seeker, ob);
		if (questInstance != null)
		{
			return questInstance;
		}
		SetConditionsDirty();
		return QuestInstance.Spawn(quest, giver, seeker, ob, canSkip);
	}

	public QuestInstance FindQuestInstanceGivenByWithSeeker(Quest quest, Character giver, Character seeker)
	{
		foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
		{
			if (questInstance.Value.Quest == quest && questInstance.Value.QuestGiver == giver && questInstance.Value.QuestSeeker == seeker)
			{
				return questInstance.Value;
			}
		}
		return null;
	}

	public QuestInstance GetMostRecentActiveQuest()
	{
		if (ActiveQuests.Count > 0)
		{
			return ActiveQuests[ActiveQuests.Count - 1];
		}
		return null;
	}

	public void DiscoverQuest(string uniqueID, Character giver, Character seeker, BaseObject ob, bool canRediscover)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			QuestInstance orCreateQuestInstance = GetOrCreateQuestInstance(quest, giver, seeker, ob, canSkip: true);
			if (orCreateQuestInstance != null && orCreateQuestInstance.State != QuestInstance.EState.Active && canRediscover)
			{
				orCreateQuestInstance.Reactivate();
			}
		}
		else
		{
			Debug.LogWarning("Quest not found: " + uniqueID);
		}
	}

	public void CompleteQuest(string uniqueID, Character giver, Character seeker, BaseObject ob, bool skipCompletionEvents)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			Instance.GetOrCreateQuestInstance(quest, giver, seeker, ob, canSkip: false).Complete(skipCompletionEvents);
		}
		else
		{
			Debug.LogWarning("Quest not found: " + uniqueID);
		}
	}

	public void CompleteQuestIfItIsDiscovered(string uniqueID, Character giver, Character seeker, BaseObject ob, bool skipCompletionEvents)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			Instance.GetQuestInstance(quest, giver, seeker, ob)?.Complete(skipCompletionEvents);
		}
		else
		{
			Debug.LogWarning("Quest not found: " + uniqueID);
		}
	}

	public bool IsQuestDiscovered(string uniqueID, Character giver, Character seeker, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			return Instance.GetQuestInstance(quest, giver, seeker, ob) != null;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestDiscoveredGivenBy(string uniqueID, Character giver)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestGiver == giver)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestDiscoveredWithObject(string uniqueID, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestObject == ob)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestDiscoveredGivenByWithSeeker(string uniqueID, Character giver, Character seeker)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestGiver == giver && questInstance.Value.QuestSeeker == seeker)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestDiscoveredWithAnyParams(string uniqueID)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestActive(string uniqueID, Character giver, Character seeker, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			QuestInstance questInstance = Instance.GetQuestInstance(quest, giver, seeker, ob);
			if (questInstance != null)
			{
				return questInstance.State == QuestInstance.EState.Active;
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestActiveGivenBy(string uniqueID, Character giver)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestGiver == giver && questInstance.Value.State == QuestInstance.EState.Active)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestActiveWithObject(string uniqueID, BaseObject questObject)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestObject == questObject && questInstance.Value.State == QuestInstance.EState.Active)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestActiveWithSeeker(string uniqueID, BaseObject questSeeker)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.QuestSeeker == questSeeker && questInstance.Value.State == QuestInstance.EState.Active)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestActiveWithAnyParams(string uniqueID)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
			{
				if (questInstance.Value.Quest == quest && questInstance.Value.State == QuestInstance.EState.Active)
				{
					return true;
				}
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestCompleted(string uniqueID, Character giver, Character seeker, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			QuestInstance questInstance = Instance.GetQuestInstance(quest, giver, seeker, ob);
			if (questInstance != null)
			{
				return questInstance.State == QuestInstance.EState.Completed;
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public bool IsQuestFailed(string uniqueID, Character giver, Character seeker, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			QuestInstance questInstance = Instance.GetQuestInstance(quest, giver, seeker, ob);
			if (questInstance != null)
			{
				return questInstance.State == QuestInstance.EState.Failed;
			}
			return false;
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return false;
	}

	public TimeSpan GetTimeSinceQuestStarted(string uniqueID, Character giver, Character seeker, BaseObject ob)
	{
		Quest quest = GameImpl.Instance.FindQuestByUniqueID(uniqueID);
		if (quest != null)
		{
			QuestInstance questInstance = Instance.GetQuestInstance(quest, giver, seeker, ob);
			if (questInstance != null)
			{
				return questInstance.DiscoveredTime;
			}
		}
		Debug.LogWarning("Quest not found: " + uniqueID);
		return TimeSpan.Zero;
	}

	public void ReplaceEquipmentInQuestInstances(Equipment from, Equipment to)
	{
		bool flag = false;
		foreach (KeyValuePair<int, QuestInstance> questInstance in QuestInstances)
		{
			if (questInstance.Value.QuestObject == from)
			{
				questInstance.Value.SetQuestObject(to);
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		Instance.RebuildQuestInstances();
		for (int i = 0; i < EnabledTriggers.Length; i++)
		{
			for (int j = 0; j < EnabledTriggers[i].Count; j++)
			{
				if (EnabledTriggers[i][j].Object == from)
				{
					EnabledTrigger value = EnabledTriggers[i][j];
					value.Object = to;
					EnabledTriggers[i][j] = value;
				}
			}
		}
		for (int k = 0; k < QueuedEvents.Count; k++)
		{
			if (QueuedEvents[k].Obj == from)
			{
				QueuedEvent value2 = QueuedEvents[k];
				value2.Obj = to;
				QueuedEvents[k] = value2;
			}
		}
	}

	public void RestoreEquipmentInQuestInstances(Equipment item)
	{
		QuestInstance questInstance = null;
		foreach (KeyValuePair<int, QuestInstance> questInstance2 in Instance.QuestInstances)
		{
			if (questInstance2.Value.QuestObjectEquipmentType != item.GetPrototype() || (questInstance2.Value.QuestObject != null && !questInstance2.Value.QuestObject.Deleted))
			{
				continue;
			}
			if (questInstance != null && (questInstance2.Value.QuestGiver != questInstance.QuestGiver || questInstance2.Value.QuestSeeker != questInstance.QuestSeeker))
			{
				QuestGroup questGroup = GameImpl.Instance.FindQuestGroupByUniqueID(questInstance.Quest.GroupID);
				if (questGroup == null || !questGroup.MergeInstances)
				{
					continue;
				}
			}
			questInstance2.Value.SetQuestObject(item);
			questInstance = questInstance2.Value;
		}
		RebuildQuestInstances();
	}

	public bool IsTriggerEnabled(string uniqueID, Character actor, BaseObject obj)
	{
		for (int i = 0; i < EnabledTriggers.Length; i++)
		{
			for (int j = 0; j < EnabledTriggers[i].Count; j++)
			{
				if (EnabledTriggers[i][j].Trigger.UniqueID == uniqueID && EnabledTriggers[i][j].Actor == actor && EnabledTriggers[i][j].Object == obj)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void EnableTrigger(string uniqueID, Character actor, Character target, BaseObject obj)
	{
		if (!IsTriggerEnabled(uniqueID, actor, obj))
		{
			Trigger trigger = GameImpl.Instance.FindTriggerByUniqueID(uniqueID);
			if (trigger != null)
			{
				EnabledTrigger item = new EnabledTrigger
				{
					Trigger = trigger,
					Actor = actor,
					Target = target,
					Object = obj
				};
				EnabledTriggers[(int)trigger.Type].Add(item);
			}
		}
	}

	public void DisableTrigger(string uniqueID, Character actor, Character target, BaseObject obj)
	{
		for (int i = 0; i < EnabledTriggers.Length; i++)
		{
			for (int j = 0; j < EnabledTriggers[i].Count; j++)
			{
				if (EnabledTriggers[i][j].Trigger.UniqueID == uniqueID && EnabledTriggers[i][j].Actor == actor && EnabledTriggers[i][j].Target == target && EnabledTriggers[i][j].Object == obj)
				{
					EnabledTriggers[i].RemoveAt(j);
					return;
				}
			}
		}
	}

	public int FindEnabledTriggerForQuest(QuestInstance questInstance, Trigger trigger)
	{
		List<EnabledTrigger> list = EnabledTriggers[(int)trigger.Type];
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].QuestInstance == questInstance && list[i].Trigger == trigger)
			{
				return i;
			}
		}
		return -1;
	}

	public void EnableTriggersForQuest(QuestInstance questInstance)
	{
		for (int i = 0; i < EnabledTriggers.Length; i++)
		{
			for (int num = EnabledTriggers[i].Count - 1; num >= 0; num--)
			{
				if (EnabledTriggers[i][num].QuestInstance == questInstance && questInstance.Quest.FindTrigger(EnabledTriggers[i][num].Trigger) == -1)
				{
					EnabledTriggers[i].RemoveAt(num);
				}
			}
		}
		if (questInstance.Quest == null || questInstance.Quest.Triggers == null)
		{
			return;
		}
		for (int j = 0; j < questInstance.Quest.Triggers.Count; j++)
		{
			if (FindEnabledTriggerForQuest(questInstance, questInstance.Quest.Triggers[j]) == -1)
			{
				EnabledTrigger item = new EnabledTrigger
				{
					Trigger = questInstance.Quest.Triggers[j],
					QuestInstance = questInstance
				};
				Instance.EnabledTriggers[(int)item.Trigger.Type].Add(item);
			}
		}
	}

	public void DisableTriggersForQuest(QuestInstance questInstance)
	{
		if (questInstance.Quest == null || questInstance.Quest.Triggers == null)
		{
			return;
		}
		for (int i = 0; i < questInstance.Quest.Triggers.Count; i++)
		{
			Trigger trigger = questInstance.Quest.Triggers[i];
			List<EnabledTrigger> list = EnabledTriggers[(int)trigger.Type];
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].QuestInstance == questInstance)
				{
					list.RemoveAt(num);
				}
			}
		}
	}

	public void OneShotTrigger(string uniqueID, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		Trigger trigger = GameImpl.Instance.FindTriggerByUniqueID(uniqueID);
		if (trigger == null)
		{
			return;
		}
		int num = FindUsedOneShotTriggerIndex(trigger, actor, target, obj, param);
		if ((num != -1 && !OneShotTriggersUsed[num].CanTrigger()) || !AreAllConditionsSatisfied(trigger.Conditions, actor, target, obj, param))
		{
			return;
		}
		QueueEvents(trigger.Events, actor, target, obj, param);
		if (trigger.OnceOnly || trigger.TriggerRepeatTime != 0f)
		{
			if (num == -1)
			{
				EnabledTrigger item = new EnabledTrigger
				{
					Trigger = trigger,
					Actor = actor,
					Target = target,
					Object = obj,
					QuestInstance = param.GetBaseObject(),
					LastTriggeredTime = Session.Instance.PlayTime
				};
				OneShotTriggersUsed.Add(item);
			}
			else
			{
				EnabledTrigger value = OneShotTriggersUsed[num];
				value.LastTriggeredTime = Session.Instance.PlayTime;
				OneShotTriggersUsed[num] = value;
			}
		}
	}

	public int FindUsedOneShotTriggerIndex(Trigger trigger, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		if (trigger.OnceOnly || trigger.TriggerRepeatTime != 0f)
		{
			for (int i = 0; i < OneShotTriggersUsed.Count; i++)
			{
				if (OneShotTriggersUsed[i].Trigger == trigger && OneShotTriggersUsed[i].Actor == actor && OneShotTriggersUsed[i].Target == target && OneShotTriggersUsed[i].Object == obj && OneShotTriggersUsed[i].QuestInstance == param.GetBaseObject())
				{
					return i;
				}
			}
		}
		return -1;
	}

	public bool IsCharacterInTriggerZone(Character character, string triggerZoneNameUniqueID)
	{
		if (BaseObjectManager.Instance.GetObjectByUniqueID(triggerZoneNameUniqueID) is Zone zone)
		{
			return zone.IsCharacterInZone(character);
		}
		return false;
	}

	public void TriggerEnabledTriggersOfType(TriggerType triggerType, Character triggerer, BaseObject triggeree)
	{
		List<EnabledTrigger> list = EnabledTriggers[(int)triggerType];
		for (int i = 0; i < list.Count; i++)
		{
			Trigger trigger = list[i].Trigger;
			if (list[i].QuestInstance != null)
			{
				if (list[i].CanTrigger() && AreAllConditionsSatisfied(trigger.Conditions, triggerer, null, triggeree, new MemoryParam(list[i].QuestInstance)))
				{
					QueueEvents(trigger.Events, triggerer, null, triggeree, new MemoryParam(list[i].QuestInstance));
					EnabledTrigger value = list[i];
					value.LastTriggeredTime = Session.Instance.PlayTime;
					list[i] = value;
				}
				continue;
			}
			Character actor = list[i].Actor;
			BaseObject obj = list[i].Object;
			if (list[i].CanTrigger())
			{
				Character target = triggerer;
				if (triggerType == TriggerType.Normal || triggerType == TriggerType.EverySecond)
				{
					target = list[i].Target;
				}
				if (AreAllConditionsSatisfied(trigger.Conditions, actor, target, obj, new MemoryParam(triggeree)))
				{
					QueueEvents(trigger.Events, actor, target, obj, new MemoryParam(triggeree));
					EnabledTrigger value2 = list[i];
					value2.LastTriggeredTime = Session.Instance.PlayTime;
					list[i] = value2;
				}
			}
		}
	}

	public void RemoveFromInvaderInstance(BaseObject hunter)
	{
		foreach (InvaderInstance activeInvader in ActiveInvaders)
		{
			if (activeInvader.CreatedObjects.Contains(hunter))
			{
				activeInvader.CreatedObjects.Remove(hunter);
				if (activeInvader.CreatedObjects.Count == 0 && !activeInvader.Active)
				{
					ActiveInvaders.Remove(activeInvader);
				}
				break;
			}
		}
	}

	public InvaderInstance GetInvaderInstanceThatCreatedHunter(BaseObject hunter)
	{
		foreach (InvaderInstance activeInvader in ActiveInvaders)
		{
			if (activeInvader.CreatedObjects.Contains(hunter))
			{
				return activeInvader;
			}
		}
		return null;
	}

	public InvaderInstance GetInvaderInstance(Invader invader, BaseObject sourceObject)
	{
		foreach (InvaderInstance activeInvader in ActiveInvaders)
		{
			if (activeInvader.Invader == invader && activeInvader.SourceObject == sourceObject)
			{
				return activeInvader;
			}
		}
		return null;
	}

	public InvaderInstance GetInvaderInstanceByUniqueID(string invaderUniqueID, BaseObject sourceObject)
	{
		foreach (InvaderInstance activeInvader in ActiveInvaders)
		{
			if (activeInvader.Invader != null && activeInvader.Invader.UniqueID == invaderUniqueID && activeInvader.SourceObject == sourceObject)
			{
				return activeInvader;
			}
		}
		return null;
	}

	public InvaderInstance ActivateInvader(Invader invader, BaseObject sourceObject, out bool newlyActivated)
	{
		InvaderInstance invaderInstance = GetInvaderInstance(invader, sourceObject);
		if (invaderInstance == null)
		{
			invaderInstance = new InvaderInstance();
			invaderInstance.Invader = invader;
			invaderInstance.SourceObject = sourceObject;
		}
		else if (invaderInstance.Active)
		{
			invaderInstance.TriggeredTime = Session.Instance.PlayTime;
			newlyActivated = false;
			return invaderInstance;
		}
		SetConditionsDirty();
		invaderInstance.LastSpawnedTime = Session.Instance.PlayTime - TimeSpan.FromSeconds(invader.MinCooldownDays * Sun.DayLengthSecs * Session.Instance.DeterministicRand.RandomFloat());
		invaderInstance.TriggeredTime = Session.Instance.PlayTime;
		invaderInstance.TimeoutLerp = Session.Instance.DeterministicRand.RandomFloat();
		invaderInstance.Active = true;
		ActiveInvaders.Add(invaderInstance);
		if (InfoScreen.Instance.IsShowingMap())
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		newlyActivated = true;
		return invaderInstance;
	}

	public void ActivateInvader(string uniqueID, BaseObject sourceObject, out bool newlyActivated)
	{
		Invader invader = GameImpl.Instance.FindInvaderByUniqueID(uniqueID);
		if (invader != null)
		{
			ActivateInvader(invader, sourceObject, out newlyActivated);
			return;
		}
		newlyActivated = false;
		Debug.LogWarning("Invader not found: " + uniqueID);
	}

	public void DeactivateInvader(InvaderInstance invaderInstance)
	{
		if (invaderInstance.CreatedObjects.Count > 0)
		{
			invaderInstance.Active = false;
		}
		else
		{
			ActiveInvaders.Remove(invaderInstance);
		}
		if (InfoScreen.Instance.IsShowingMap())
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void DeactivateInvader(string uniqueID, BaseObject sourceObject)
	{
		Invader invader = GameImpl.Instance.FindInvaderByUniqueID(uniqueID);
		if (invader != null)
		{
			InvaderInstance invaderInstance = GetInvaderInstance(invader, sourceObject);
			if (invaderInstance != null)
			{
				DeactivateInvader(invaderInstance);
			}
		}
		else
		{
			Debug.LogWarning("Invader not found: " + uniqueID);
		}
	}

	public InvaderInstance PickInvader(CustomRandom rand, bool freebie)
	{
		TempInvaderInstances.Clear();
		float num = 0f;
		foreach (InvaderInstance activeInvader in ActiveInvaders)
		{
			if (activeInvader.Active && activeInvader.Invader != null)
			{
				int num2 = activeInvader.Invader.CalcMaxActive();
				if (activeInvader.CreatedObjects.Count < num2 && (freebie || !(Session.Instance.PlayTime - activeInvader.LastSpawnedTime <= activeInvader.CalcCooldown())))
				{
					TempInvaderInstances.Add(activeInvader);
					num += activeInvader.Invader.CalcDensity();
				}
			}
		}
		if (num <= 0f)
		{
			TempInvaderInstances.Clear();
			return null;
		}
		float num3 = rand.RandomFloat() * num;
		foreach (InvaderInstance tempInvaderInstance in TempInvaderInstances)
		{
			num3 -= tempInvaderInstance.Invader.CalcDensity();
			if (num3 < 0f)
			{
				TempInvaderInstances.Clear();
				return tempInvaderInstance;
			}
		}
		TempInvaderInstances.Clear();
		return null;
	}

	public void RefreshQuestMarkers()
	{
		QuestDestinations.Clear();
		QuestMarkerType questMarkerType = QuestMarkerType.Normal;
		if (_currentQuest != null && _currentQuest.Quest != null)
		{
			if (_currentQuest.Quest.QuestMarkers != null)
			{
				for (int i = 0; i < _currentQuest.Quest.QuestMarkers.Count; i++)
				{
					BaseObject subject = _currentQuest.Quest.QuestMarkers[i].GetSubject<BaseObject>(_currentQuest.QuestGiver, _currentQuest.QuestSeeker, _currentQuest.QuestObject, _currentQuest.QuestParam);
					TileObject tileObject = subject as TileObject;
					Community community = subject as Community;
					if (subject is Equipment equipment)
					{
						tileObject = equipment.InventoryOwner;
					}
					if (community != null)
					{
						questMarkerType = _currentQuest.Quest.QuestMarkerType;
						if (questMarkerType == QuestMarkerType.UnAlliedLeader || questMarkerType == QuestMarkerType.Leader)
						{
							if (community.Leader != null)
							{
								QuestDestinations.Add(community.Leader);
							}
							continue;
						}
						foreach (Character member in community.Members)
						{
							QuestDestinations.Add(member);
						}
						foreach (Prop building in community.Buildings)
						{
							QuestDestinations.Add(building);
						}
					}
					else if (tileObject != null)
					{
						QuestDestinations.Add(tileObject);
						questMarkerType = _currentQuest.Quest.QuestMarkerType;
					}
				}
			}
			if (_currentQuest.Quest.Metric != QuestMetric.None)
			{
				_currentQuest.CalcQuestMetric(addQuestMarkers: true);
			}
		}
		for (int num = QuestDestinations.Count - 1; num >= 0; num--)
		{
			TileObject tileObject2 = QuestDestinations[num];
			if (tileObject2.Deleted)
			{
				QuestDestinations.RemoveAt(num);
			}
			else if (_currentQuest != null && _currentQuest.Quest.QuestMarkerConditions != null && _currentQuest.Quest.QuestMarkerConditions.Count > 0 && !AreAllConditionsSatisfied(_currentQuest.Quest.QuestMarkerConditions, _currentQuest.QuestGiver, _currentQuest.QuestSeeker, _currentQuest.QuestObject, new MemoryParam(tileObject2)))
			{
				QuestDestinations.RemoveAt(num);
			}
			else
			{
				switch (questMarkerType)
				{
				case QuestMarkerType.Alive:
					if (!(tileObject2 is Human { Alive: not false }))
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.AliveAndNotZombie:
					if (!(tileObject2 is Human { AliveAndNotZombie: not false }))
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.DeadButUnburied:
					if (!(tileObject2 is Human { Alive: false, Disappeared: false }))
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.UnderConstruction:
					if (tileObject2.GetUnderConstructionInfo() == null)
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.LivingQuarters:
					if (!tileObject2.IsAccommodation())
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.UnAlliedLeader:
					if (tileObject2 is Human { AliveAndNotZombie: not false } human3)
					{
						if (Session.Instance.CommunityManager.PlayerCommunity.CachedAllies.Contains(human3.Community))
						{
							QuestDestinations.RemoveAt(num);
						}
					}
					else
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				case QuestMarkerType.Leader:
					if (!(tileObject2 is Human { AliveAndNotZombie: not false }))
					{
						QuestDestinations.RemoveAt(num);
					}
					break;
				}
			}
		}
	}

	public void SetVariable(Variable var)
	{
		for (int i = 0; i < Variables.Count; i++)
		{
			if (Variables[i].UniqueID == var.UniqueID && Variables[i].SubjectID == var.SubjectID && Variables[i].ObjectID == var.ObjectID)
			{
				Variables[i] = var;
				SetConditionsDirty();
				return;
			}
		}
		Variables.Add(var);
		SetConditionsDirty();
	}

	public void IncrementVariable(string uniqueID, BaseObject sub, BaseObject ob, float amount, float cap = 0f)
	{
		float variable = GetVariable(uniqueID, sub, ob);
		variable += amount;
		if (cap != 0f)
		{
			variable = Math.Min(variable, cap);
		}
		SetVariable(Variable.Create(uniqueID, sub, ob, variable));
	}

	public float GetVariable(string uniqueID, BaseObject sub, BaseObject ob)
	{
		int num = sub?.Id ?? 0;
		int num2 = ob?.Id ?? 0;
		for (int i = 0; i < Variables.Count; i++)
		{
			if (Variables[i].UniqueID == uniqueID && Variables[i].SubjectID == num && Variables[i].ObjectID == num2)
			{
				return Variables[i].Value;
			}
		}
		return 0f;
	}

	public bool HasSpokenOnce(Speech speech)
	{
		return SpokenOnce.Contains(speech.UniqueID);
	}

	public void RememberSpokenOnce(Speech speech)
	{
		SpokenOnce.Add(speech.UniqueID);
	}

	public void OnInterestingSpeechStarted(Character speaker, Character listener, Importance importance)
	{
		int num = FindInterestingSpeaker(speaker);
		if (num != -1)
		{
			InterestingSpeakers[num] = InterestingSpeaker.Create(speaker, listener, -1, importance);
		}
		else
		{
			InterestingSpeakers.Add(InterestingSpeaker.Create(speaker, listener, -1, importance));
		}
		InterestingSpeakers.InsertionSort(InterestingSpeakerSorter);
	}

	public void OnCharacterDeleted(Character character)
	{
		RemoveInterestingSpeaker(character);
		OnObjectDeleted(character);
	}

	public void RemoveInterestingSpeaker(Character character)
	{
		int num = FindInterestingSpeaker(character);
		if (num != -1)
		{
			InterestingSpeakers.RemoveAt(num);
		}
		if (DramaticDeathCharacter == character)
		{
			DramaticDeathCharacter = null;
		}
	}

	public void OnObjectDeleted(TileObject obj)
	{
		for (int num = QueuedEvents.Count - 1; num >= 0; num--)
		{
			if (QueuedEvents[num].Actor == obj)
			{
				QueuedEvent value = QueuedEvents[num];
				value.Actor = null;
				QueuedEvents[num] = value;
			}
			if (QueuedEvents[num].Target == obj)
			{
				QueuedEvent value2 = QueuedEvents[num];
				value2.Target = null;
				QueuedEvents[num] = value2;
			}
			if (QueuedEvents[num].Obj == obj)
			{
				QueuedEvent value3 = QueuedEvents[num];
				value3.Obj = null;
				QueuedEvents[num] = value3;
			}
		}
	}

	public void OnInterestingSpeechFinished(Character speaker)
	{
		int num = FindInterestingSpeaker(speaker);
		if (num != -1)
		{
			InterestingSpeakers[num] = InterestingSpeaker.Create(InterestingSpeakers[num].Speaker, InterestingSpeakers[num].Listener, 0, InterestingSpeakers[num].Importance);
		}
		InterestingSpeakers.InsertionSort(InterestingSpeakerSorter);
	}

	private int FindInterestingSpeaker(Character speaker)
	{
		for (int i = 0; i < InterestingSpeakers.Count; i++)
		{
			if (InterestingSpeakers[i].Speaker == speaker)
			{
				return i;
			}
		}
		return -1;
	}

	public Character GetMostInterestingSpeaker()
	{
		return GetMostInterestingSpeaker(skipIfLowerPriorityThanLocalPlayerConversation: false, skipIfLowerPriorityThanAnyPlayerConversation: false);
	}

	public Character GetMostInterestingSpeaker(bool skipIfLowerPriorityThanLocalPlayerConversation, bool skipIfLowerPriorityThanAnyPlayerConversation)
	{
		if (skipIfLowerPriorityThanLocalPlayerConversation)
		{
			bool flag = false;
			if (Hud.Instance.LocalTargetObject is Character { AliveAndNotZombie: not false } character && !Hud.Instance.LocalWantLockOnTarget)
			{
				flag = true;
				if (character.FindActiveGoal(GoalType.Conversation) is Conversation conversation)
				{
					conversation.GetCurrentSpeaker(character);
				}
			}
			else if (Hud.Instance.LocalControlledCharacter != null && Hud.Instance.LocalControlledCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation2)
			{
				flag = true;
				conversation2.GetCurrentSpeaker(Hud.Instance.LocalControlledCharacter);
			}
			if (!flag)
			{
				skipIfLowerPriorityThanLocalPlayerConversation = false;
			}
		}
		if (skipIfLowerPriorityThanAnyPlayerConversation)
		{
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode != PlayerMode.Controlling)
				{
					continue;
				}
				bool flag2 = false;
				if (playerRecord.TargetObject is Character { AliveAndNotZombie: not false } character2 && !playerRecord.WantLockOnTarget)
				{
					flag2 = true;
					if (character2.FindActiveGoal(GoalType.Conversation) is Conversation conversation3)
					{
						conversation3.GetCurrentSpeaker(character2);
					}
				}
				else if (playerRecord.PlayerCharacter != null && playerRecord.PlayerCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation4)
				{
					flag2 = true;
					conversation4.GetCurrentSpeaker(playerRecord.PlayerCharacter);
				}
				if (!flag2)
				{
					skipIfLowerPriorityThanAnyPlayerConversation = false;
				}
			}
		}
		for (int i = 0; i < InterestingSpeakers.Count; i++)
		{
			InterestingSpeaker interestingSpeaker = InterestingSpeakers[i];
			if (interestingSpeaker.Importance == Importance.Cheer)
			{
				if (skipIfLowerPriorityThanLocalPlayerConversation || skipIfLowerPriorityThanAnyPlayerConversation || (!interestingSpeaker.IsEitherDirectControlled() && !interestingSpeaker.IsEitherSparringPartnerDirectControlled()))
				{
					continue;
				}
			}
			else if (interestingSpeaker.Importance <= Importance.Low)
			{
				if (skipIfLowerPriorityThanLocalPlayerConversation || skipIfLowerPriorityThanAnyPlayerConversation || !interestingSpeaker.IsEitherDirectControlled())
				{
					continue;
				}
			}
			else if (interestingSpeaker.Importance <= Importance.Encouragement || interestingSpeaker.Importance == Importance.FriendUnderAttack)
			{
				if (skipIfLowerPriorityThanLocalPlayerConversation || skipIfLowerPriorityThanAnyPlayerConversation || !interestingSpeaker.IsEitherPlayerControlled())
				{
					continue;
				}
			}
			else if (interestingSpeaker.Importance == Importance.Memory)
			{
				if ((!interestingSpeaker.IsEitherDirectControlled() && !interestingSpeaker.IsEitherSparring() && (interestingSpeaker.Listener == null || interestingSpeaker.Listener.Alive)) || !interestingSpeaker.IsEitherKnown(!interestingSpeaker.IsEitherSparring()))
				{
					continue;
				}
			}
			else if ((interestingSpeaker.Importance <= Importance.Normal || interestingSpeaker.Importance == Importance.BlameForAttack || interestingSpeaker.Importance == Importance.EndFisticuffs) && !interestingSpeaker.IsEitherKnown(interestingSpeaker.Importance >= Importance.Normal && interestingSpeaker.Importance != Importance.EndFisticuffs))
			{
				continue;
			}
			return InterestingSpeakers[i].Speaker;
		}
		return null;
	}
}
