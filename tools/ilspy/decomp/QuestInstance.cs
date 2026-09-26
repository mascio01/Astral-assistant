using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class QuestInstance : BaseObject
{
	public enum EState
	{
		Active,
		Completed,
		Failed
	}

	public int Hash;

	public Quest Quest;

	public Character QuestGiver;

	public Character QuestSeeker;

	public BaseObject QuestObject;

	public EquipmentPrototype QuestObjectEquipmentType;

	public MemoryParam QuestParam;

	public EState State;

	public TimeSpan DiscoveredTime;

	protected string UniqueID;

	private string CachedDescription;

	private Language CachedDescriptionLanguage = Language.Invalid;

	private InputType CachedDescriptionInputType;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.QuestInstance;
	}

	public override string GetUniqueID()
	{
		return UniqueID;
	}

	public override void SetUniqueID(string id)
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		UniqueID = id;
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public static int CalcHash(Quest quest, Character giver, Character seeker, BaseObject ob)
	{
		return (int)((uint)(StringUtil.JenkinsHash(quest.UniqueID) ^ (giver?.Id ?? 0)) ^ ((seeker != null) ? MathUtil.WangHash((uint)(seeker.Id + 100000000)) : 0) ^ ((ob != null) ? MathUtil.WangHash((uint)ob.Id) : 0));
	}

	public static QuestInstance Spawn(Quest quest, Character giver, Character seeker, BaseObject ob, bool canSkip)
	{
		QuestInstance questInstance = new QuestInstance();
		questInstance.Hash = CalcHash(quest, giver, seeker, ob);
		questInstance.Quest = quest;
		questInstance.QuestGiver = giver;
		questInstance.QuestSeeker = seeker;
		questInstance.SetQuestObject(ob);
		questInstance.UniqueID = quest.UniqueID + ((giver != null) ? ("_" + giver.Id) : "") + ((seeker != null) ? ("_" + seeker.Id) : "") + ((ob != null) ? ("_" + ob.Id) : "");
		questInstance.Init();
		questInstance.OnQuestActivated(canSkip);
		return questInstance;
	}

	public void SetQuestObject(BaseObject ob)
	{
		QuestObject = ob;
		if (ob is Equipment equipment)
		{
			QuestObjectEquipmentType = equipment.GetPrototype();
		}
		else
		{
			QuestObjectEquipmentType = null;
		}
	}

	public void OnQuestActivated(bool canSkip)
	{
		DiscoveredTime = Session.Instance.PlayTime;
		StoryManager.QueueEvents(Quest.StartEvents, QuestGiver, QuestSeeker, QuestObject, QuestParam);
		if (canSkip && Quest.SkipNotificationIfAlreadyCompleted && EvaluateIsCompleted() && !EvaluateIsFailed())
		{
			Complete();
			return;
		}
		if (StoryManager.Instance.CurrentQuest == null || StoryManager.Instance.CurrentQuest.Quest == null || !StoryManager.Instance.CurrentQuest.Quest.HighPriority)
		{
			StoryManager.Instance.CurrentQuest = this;
		}
		NotificationManager.Instance.AddNewQuestNotification(this);
		LogEvent logEvent = new LogEvent(LogEventType.QuestDiscovered);
		logEvent.Quest = this;
		logEvent.Character = QuestGiver;
		logEvent.Listener = QuestSeeker;
		logEvent.ReferringTo = QuestObject;
		logEvent.SpeakingParamResults = new List<SpeechParamResult>();
		Speech.EvaluateSpeechParams(Quest.Params, QuestGiver, QuestSeeker, QuestObject, logEvent.SpeakingParamResults, QuestParam, null, null, MathUtil.NonDeterministicRand, this, Quest.UniqueID);
		Session.Instance.AddLogEvent(logEvent);
	}

	public override void Init()
	{
		base.Init();
		QuestInstance value = null;
		if (!StoryManager.Instance.QuestInstances.TryGetValue(Hash, out value))
		{
			StoryManager.Instance.QuestInstances.Add(Hash, this);
		}
		else
		{
			Debug.LogWarning("Duplicate quest hash " + Hash + " for " + GetDebugString() + ", existing: " + value.GetDebugString());
		}
		BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		if (State == EState.Active)
		{
			StoryManager.Instance.ActiveQuests.Add(this);
			if (Session.Instance.State >= SessionState.Loaded)
			{
				StoryManager.Instance.EnableTriggersForQuest(this);
			}
		}
	}

	public string GetDebugString()
	{
		return UniqueID + ((QuestSeeker != null) ? (" with seeker " + QuestSeeker.GetDisplayNameString()) : string.Empty) + ((QuestGiver != null) ? (" with giver " + QuestGiver.GetDisplayNameString()) : string.Empty) + ((QuestObject != null) ? (" with object " + QuestObject.GetDisplayNameString()) : string.Empty);
	}

	public override void Delete()
	{
		if (State == EState.Active)
		{
			StoryManager.Instance.DisableTriggersForQuest(this);
			StoryManager.Instance.ActiveQuests.Remove(this);
		}
		BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		StoryManager.Instance.QuestInstances.Remove(Hash);
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref UniqueID);
		reflector.Add(ref Quest);
		if (Quest == null && reflector.IsDeserialising && reflector is CustomBinaryReader customBinaryReader)
		{
			Debug.LogWarning("Got a quest instance with a null quest: " + customBinaryReader.LastReadName);
		}
		reflector.Add(ref State);
		reflector.Add(ref DiscoveredTime);
		reflector.AddAfter(ref QuestGiver, 43);
		reflector.AddAfter(ref QuestSeeker, 287);
		reflector.AddAfter(ref QuestObject, 43);
		if (reflector.Version >= 567)
		{
			reflector.Add(ref QuestObjectEquipmentType);
		}
		else
		{
			SetQuestObject(QuestObject);
		}
		if (reflector.Version >= 535)
		{
			QuestParam.Reflect(reflector);
		}
		reflector.AddAfter(ref Hash, 43);
		if (reflector.IsDeserialising && reflector.Version < 255)
		{
			Hash = CalcHash(Quest, QuestGiver, QuestSeeker, QuestObject);
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (CachedDescriptionLanguage != GameImpl.Instance.Settings.Language || CachedDescriptionInputType != InputFunctionManager.Instance.CurrentInputType || englishOnly)
		{
			List<SpeechParamResult> paramResults = new List<SpeechParamResult>();
			List<SpeechEmoticon> emoticons = new List<SpeechEmoticon>();
			Speech.EvaluateSpeechParams(Quest.Params, QuestGiver, QuestSeeker, QuestObject, paramResults, QuestParam, null, null, MathUtil.NonDeterministicRand, this, Quest.UniqueID);
			Speech.BuildSpeechText(Quest.DescriptionHash, Quest.Params, QuestGiver, QuestSeeker, QuestObject, paramResults, out var speechText, emoticons, englishOnly, isQuest: true);
			if (englishOnly)
			{
				sb.Append(speechText);
				return;
			}
			CachedDescription = speechText;
			CachedDescriptionLanguage = GameImpl.Instance.Settings.Language;
			CachedDescriptionInputType = InputFunctionManager.Instance.CurrentInputType;
		}
		sb.Append(CachedDescription);
	}

	private void FinishQuest()
	{
		if (StoryManager.Instance.CurrentQuest == this)
		{
			StoryManager.Instance.CurrentQuest = StoryManager.Instance.GetMostRecentActiveQuest();
		}
		StoryManager.Instance.DisableTriggersForQuest(this);
	}

	public void Reactivate()
	{
		if (State != EState.Active)
		{
			State = EState.Active;
			OnQuestActivated(canSkip: false);
			StoryManager.Instance.ActiveQuests.Add(this);
			StoryManager.Instance.SetConditionsDirty();
			StoryManager.Instance.EnableTriggersForQuest(this);
		}
	}

	public void Complete()
	{
		Complete(skipCompletionEvents: false);
	}

	public void Complete(bool skipCompletionEvents)
	{
		if (State == EState.Active)
		{
			StoryManager.Instance.ActiveQuests.Remove(this);
			State = EState.Completed;
			StoryManager.Instance.SetConditionsDirty();
			if (!skipCompletionEvents)
			{
				StoryManager.QueueEvents(Quest.CompletionEvents, QuestGiver, QuestSeeker, QuestObject, QuestParam);
			}
			FinishQuest();
		}
	}

	public void Fail()
	{
		Fail(skipCompletionEvents: false);
	}

	public void Fail(bool skipCompletionEvents)
	{
		if (State == EState.Active)
		{
			StoryManager.Instance.ActiveQuests.Remove(this);
			State = EState.Failed;
			StoryManager.Instance.SetConditionsDirty();
			if (!skipCompletionEvents)
			{
				StoryManager.QueueEvents(Quest.FailureEvents, QuestGiver, QuestSeeker, QuestObject, QuestParam);
			}
			FinishQuest();
			NotificationManager.Instance.AddQuestFailedNotification(this);
			LogEvent logEvent = new LogEvent(LogEventType.QuestFailed);
			logEvent.Quest = this;
			logEvent.Character = QuestGiver;
			logEvent.Listener = QuestSeeker;
			logEvent.ReferringTo = QuestObject;
			logEvent.SpeakingParamResults = new List<SpeechParamResult>();
			Speech.EvaluateSpeechParams(Quest.Params, QuestGiver, QuestSeeker, QuestObject, logEvent.SpeakingParamResults, QuestParam, null, null, MathUtil.NonDeterministicRand, this, Quest.UniqueID);
			Session.Instance.AddLogEvent(logEvent);
		}
	}

	public bool EvaluateIsCompleted()
	{
		if ((Quest.MetricIsCompletionCondition || Quest.Metric == QuestMetric.PercentageOfAllNPCsRecruited) && CalcQuestMetric(addQuestMarkers: false) >= Quest.MetricCompletedAmount)
		{
			return true;
		}
		if (Quest.CompletionConditions == null || Quest.CompletionConditions.Count == 0)
		{
			return false;
		}
		return StoryManager.AreAllConditionsSatisfied(Quest.CompletionConditions, QuestGiver, QuestSeeker, QuestObject, QuestParam);
	}

	public bool EvaluateIsFailed()
	{
		return StoryManager.AreAnyConditionsSatisfied(Quest.FailureConditions, QuestGiver, QuestSeeker, QuestObject, QuestParam);
	}

	public float CalcQuestMetric(bool addQuestMarkers)
	{
		switch (Quest.Metric)
		{
		case QuestMetric.Formula:
		{
			if (Quest.MetricLimit == 0f)
			{
				break;
			}
			float num = 0f;
			if (Quest.MetricFormulas != null)
			{
				for (int i = 0; i < Quest.MetricFormulas.Count; i++)
				{
					if (Quest.MetricFormulas[i].GetConditionBlock() != null)
					{
						num += Quest.MetricFormulas[i].GetConditionBlock().Evaluate(QuestGiver, QuestSeeker, QuestObject, QuestParam);
					}
				}
			}
			return num / Quest.MetricLimit * 100f;
		}
		case QuestMetric.PercentageOfAllNPCsRecruited:
		{
			CommunityManager communityManager2 = Session.Instance.CommunityManager;
			float num4 = communityManager2.PlayerCommunity.GetLivingNonZombieMemberCountIncludingAllies();
			float num5 = 0f;
			foreach (Community community in communityManager2.Communities)
			{
				if (!community.IsAmbientCommunity())
				{
					num5 += (float)community.GetLivingNonZombieMemberCount();
				}
			}
			return num4 / num5 * 100f;
		}
		case QuestMetric.IncreaseQuestGiverMorale:
			if (QuestGiver != null)
			{
				return (QuestGiver.CalcMorale() + 100f) / 2f;
			}
			break;
		case QuestMetric.ReduceQuestObjectMorale:
			if (QuestObject is Character)
			{
				return (((Character)QuestObject).CalcMorale() + 100f) / 2f;
			}
			break;
		case QuestMetric.TimeSinceStarted:
			if (Quest.MetricLimit == 0f)
			{
				return 100f;
			}
			return 100f * (float)(Session.Instance.PlayTime - DiscoveredTime).TotalSeconds / Quest.MetricLimit;
		case QuestMetric.InvaderTimeout:
		{
			Invader invader = GameImpl.Instance.FindInvaderByUniqueID(Quest.MetricStringData);
			if (invader == null)
			{
				break;
			}
			InvaderInstance invaderInstance = StoryManager.Instance.GetInvaderInstance(invader, QuestObject);
			if (invaderInstance != null)
			{
				float num6 = (float)(Session.Instance.PlayTime - invaderInstance.TriggeredTime).TotalSeconds;
				float num7 = (float)invaderInstance.CalcTimeout().TotalSeconds;
				if (num7 > 0f)
				{
					return num6 / num7 * 100f;
				}
			}
			break;
		}
		case QuestMetric.RelationshipProgress:
		{
			MemoryPrototype memoryPrototype = ((Quest.MetricStringData != null) ? GameImpl.Instance.FindMemoryPrototypeByUniqueID(Quest.MetricStringData) : null);
			MemoryPrototype memoryPrototype2 = ((Quest.MetricStringData2 != null) ? GameImpl.Instance.FindMemoryPrototypeByUniqueID(Quest.MetricStringData2) : null);
			float num2 = 0f;
			float num3 = 0f;
			CommunityManager communityManager = Session.Instance.CommunityManager;
			if (communityManager.PlayerCommunity.Leader != null)
			{
				foreach (Character member in communityManager.PlayerCommunity.Members)
				{
					if (member.AliveAndNotZombie && !member.IsPlayerAvatar())
					{
						if (memoryPrototype != null)
						{
							num2 = Math.Max(num2, member.GetMemoryQuantity(memoryPrototype, member, communityManager.PlayerCommunity.Leader));
						}
						if (memoryPrototype2 != null)
						{
							num3 = Math.Max(num3, member.GetMemoryQuantity(memoryPrototype2, member, communityManager.PlayerCommunity.Leader));
						}
						switch (Relationship.GetRelationship(communityManager.PlayerCommunity.Leader, member))
						{
						case RelationshipType.SleepingWith:
						case RelationshipType.MarriedTo:
							num2 = Quest.MetricLimit;
							break;
						case RelationshipType.FriendsWith:
							num3 = Quest.MetricLimit2;
							break;
						}
					}
				}
			}
			return Mathf.Clamp01(Math.Max((Quest.MetricLimit != 0f) ? (num2 / Quest.MetricLimit) : 0f, (Quest.MetricLimit2 != 0f) ? (num3 / Quest.MetricLimit2) : 0f)) * 100f;
		}
		case QuestMetric.Drunkenness:
		{
			float val = ((QuestGiver != null && Quest.MetricLimit != 0f) ? (100f * QuestGiver.GetBloodAlcoholConcentration() / Quest.MetricLimit) : 0f);
			float val2 = ((QuestObject is Character && Quest.MetricLimit != 0f) ? (100f * ((Character)QuestObject).GetBloodAlcoholConcentration() / Quest.MetricLimit) : 0f);
			return Math.Min(val, val2);
		}
		}
		return 0f;
	}

	public bool HasReferenceTo(BaseObject obj)
	{
		if (QuestGiver == obj)
		{
			return true;
		}
		if (QuestSeeker == obj)
		{
			return true;
		}
		if (QuestObject == obj)
		{
			return true;
		}
		return false;
	}
}
