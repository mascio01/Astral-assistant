using System;
using System.Collections.Generic;
using UnityEngine;

public class SpeakToTarget : Goal, ISpeechGoal
{
	public Speech Speech;

	public BaseObject SpeechObject;

	public Speech ReplyTo;

	public BaseObject ReplyToReferringTo;

	public MemoryParam ReplyToParam;

	public List<SpeechParamResult> SpeechParamResults = new List<SpeechParamResult>();

	public string SpeechText_DEPRECATED;

	public MemoryParam MemoryParam;

	public bool Success;

	public Speech SpecialBehaviourTriggered;

	public TimeSpan GoalStartedTime;

	protected bool _started;

	public static int MaxFaceTargetDist = 32;

	public SpeakToTarget()
	{
	}

	public SpeakToTarget(Speech speech, BaseObject speechObject, MemoryParam memoryParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyToParam)
	{
		Speech = speech;
		SpeechObject = speechObject;
		MemoryParam = memoryParam;
		ReplyTo = replyTo;
		ReplyToReferringTo = replyReferringTo;
		ReplyToParam = replyToParam;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Speech);
		reflector.Add(ref SpeechObject);
		reflector.AddAfter(ref ReplyTo, 56);
		reflector.AddAfter(ref ReplyToReferringTo, 56);
		if (reflector.Version >= 266)
		{
			ReplyToParam.Reflect(reflector);
		}
		if (reflector.Version < 4)
		{
			reflector.Add(ref SpeechText_DEPRECATED);
		}
		else
		{
			reflector.Add(ref SpeechParamResults);
		}
		if (reflector.Version < 144)
		{
			Character obj = null;
			int value = 0;
			reflector.AddAfter(ref obj, 143);
			reflector.Add(ref value);
		}
		else
		{
			MemoryParam.Reflect(reflector);
		}
		reflector.Add(ref _started);
		reflector.Add(ref Success);
		reflector.AddAfter(ref SpecialBehaviourTriggered, 256);
		reflector.AddAfter(ref GoalStartedTime, 366);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SpeakToTarget;
	}

	public Speech GetSpeech()
	{
		return Speech;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Target != null && (Target.Object == null || Target.Object.Deleted))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		GoalStartedTime = Session.Instance.PlayTime;
		if (Speech.MustFinish)
		{
			character.PushQueuedSpeech(Speech, GetTargetCharacter(), SpeechObject, MemoryParam);
		}
		if (Speech.Situation == SpeechSituation.OpenerToSelf && character.Speak(Speech, null, SpeechObject, MemoryParam))
		{
			character.SpeakingParamResults.CopyToList(SpeechParamResults);
			_started = true;
		}
		if (Speech.Situation == SpeechSituation.PayRespects && Target != null)
		{
			Conversation.SetPaidRespectsFlag(character, Target);
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (_started && !Finished && character.Speaking == Speech)
		{
			Character targetCharacter = GetTargetCharacter();
			MemoryParam param = MemoryParam;
			MemoryParam param2 = MemoryParam;
			BaseObject resultObj;
			BaseObject resultObj2;
			bool interrupted = (Session.Instance.PlayTime - character.SpeechStartTime).TotalSeconds < (double)StoryManager.GetSpeechLipsMoveTime(character.SpeakingTextEnglish) || StoryManager.Instance.GetContinue(character, targetCharacter, SpeechObject, Speech, out resultObj, ref param, Session.Instance.DeterministicRand) != null || StoryManager.Instance.GetNoReply(character, targetCharacter, SpeechObject, Speech, out resultObj2, ref param2, Session.Instance.DeterministicRand) != null;
			character.OnSpeechFinished(interrupted);
		}
		else if (!_started)
		{
			Character targetCharacter2 = GetTargetCharacter();
			if (targetCharacter2 != null && targetCharacter2.GetGoal() != null)
			{
				targetCharacter2.GetGoal().OnSpokenToFinished(targetCharacter2, null, character, Speech, interrupted: true, null, null);
			}
		}
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	private bool CanStartSpeaking(Character character)
	{
		if (Speech.Importance >= Importance.Normal && Speech.Importance != Importance.EndFisticuffs)
		{
			Character targetCharacter = GetTargetCharacter();
			Character mostInterestingSpeaker = StoryManager.Instance.GetMostInterestingSpeaker(skipIfLowerPriorityThanLocalPlayerConversation: false, skipIfLowerPriorityThanAnyPlayerConversation: true);
			if (mostInterestingSpeaker != null && mostInterestingSpeaker != character && mostInterestingSpeaker != targetCharacter && mostInterestingSpeaker.Speaking != null && mostInterestingSpeaker.Speaking.Importance > Importance.Cheer)
			{
				return false;
			}
			Session instance = Session.Instance;
			if (instance.IsEveryoneInInfoScreen() && instance.PlayTime - GoalStartedTime < TimeSpan.FromSeconds(20.0))
			{
				return false;
			}
		}
		if (character.GoalTarget != null && !character.Sitting)
		{
			return MathUtil.AngleDiff(character.FacingAngle, character.CalcDesiredFacingAngleFromGoalTarget()) < MathF.PI / 100f;
		}
		return true;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && (!Speech.CanTalkOutOfRange || (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= (float)MathUtil.Squared(MaxFaceTargetDist)))
		{
			character.GoalTarget = Target;
		}
		if (!_started && CanStartSpeaking(character) && character.Speak(Speech, targetCharacter, SpeechObject, MemoryParam))
		{
			character.SpeakingParamResults.CopyToList(SpeechParamResults);
			targetCharacter?.OnSpokenToStarted(character, Speech, SpeechObject, MemoryParam, ReplyTo, ReplyToReferringTo, ReplyToParam);
			_started = true;
		}
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
		if (!_started || Finished)
		{
			return;
		}
		if (speech != Speech)
		{
			Debug.LogWarning("Unexpected speech in OnSpeechFinished, expected: " + Speech.UniqueID + ", got: " + speech.UniqueID);
			return;
		}
		SpeechWithParams continueOverride = GetContinueOverride(character, parent, interrupted);
		if (continueOverride.IsValid())
		{
			cont = continueOverride.Speech;
			continueObject = continueOverride.Obj;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !targetCharacter.OnSpokenToFinished(character, Speech, interrupted, cont, SpecialBehaviourTriggered) && !interrupted)
		{
			bool multipleOptions = false;
			MemoryParam param = MemoryParam;
			BaseObject resultObj;
			Speech reply = StoryManager.Instance.GetReply(targetCharacter, character, SpeechObject, Speech, out resultObj, ref param, out multipleOptions, Session.Instance.DeterministicRand);
			if (reply != null && reply.MustFinish && !Conversation.WantReplyChoice(multipleOptions, Speech, targetCharacter, Conversation.CanBeControlledByPlayerStatic(targetCharacter, character)))
			{
				targetCharacter.PushQueuedSpeech(reply, character, resultObj, param);
			}
		}
		if (cont != null)
		{
			if (continueOverride.IsValid())
			{
				SetSpeechWithParams(continueOverride);
			}
			else
			{
				Speech = cont;
			}
			character.Speak(Speech, GetTargetCharacter(), continueObject, MemoryParam);
			character.SpeakingParamResults.CopyToList(SpeechParamResults);
		}
		else
		{
			Finished = true;
			Success = !interrupted;
		}
	}

	public virtual SpeechWithParams GetContinueOverride(Character character, Goal parent, bool interrupted)
	{
		switch (Speech.SpecialBehaviour)
		{
		case SpecialSpeechBehaviour.DrinkingGameTruth:
		{
			if (SpecialBehaviourTriggered != null)
			{
				break;
			}
			SpecialBehaviourTriggered = Speech;
			Character targetCharacter2 = GetTargetCharacter();
			List<int> list4 = new List<int>();
			for (int i = 0; i < character.Memories.Count; i++)
			{
				list4.Add(i);
			}
			CustomRandom deterministicRand2 = Session.Instance.DeterministicRand;
			while (list4.Count > 0)
			{
				int index = deterministicRand2.Next() % list4.Count;
				int index2 = list4[index];
				list4.RemoveAt(index);
				MemoryParam memoryParam2 = new MemoryParam(character, index2);
				if (memoryParam2.GetParamType() != ParamType.Memory)
				{
					continue;
				}
				Speech speechForMemory2 = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter2, memoryParam2);
				if (speechForMemory2 == null)
				{
					continue;
				}
				Memory memory2 = memoryParam2.GetMemory();
				bool flag2 = memory2.Actor != null && memory2.Actor.NameKnown;
				if (!flag2)
				{
					flag2 |= (memory2.Object as Character)?.NameKnown ?? false;
					if (!flag2)
					{
						flag2 |= (memory2.Object as Community)?.CommunityNameKnown ?? false;
					}
				}
				if (flag2)
				{
					return SpeechWithParams.Create(speechForMemory2, memory2.Object, memoryParam2);
				}
			}
			List<Speech> list5 = GameImpl.Instance.SpeechesForSituation[80];
			Speech speech = ((list5.Count > 0) ? list5[0] : null);
			if (speech != null)
			{
				MemoryParam param = default(MemoryParam);
				BaseObject resultObj;
				bool multipleOptions;
				Speech reply = StoryManager.Instance.GetReply(character, targetCharacter2, null, speech, out resultObj, ref param, out multipleOptions, deterministicRand2);
				if (reply != null)
				{
					return SpeechWithParams.Create(reply, resultObj, param);
				}
			}
			break;
		}
		case SpecialSpeechBehaviour.DrinkingGameLie:
		{
			if (SpecialBehaviourTriggered != null)
			{
				break;
			}
			SpecialBehaviourTriggered = Speech;
			Character targetCharacter = GetTargetCharacter();
			CustomRandom deterministicRand = Session.Instance.DeterministicRand;
			MemoryParam memoryParam = default(MemoryParam);
			List<Character> list = new List<Character>();
			List<Community> list2 = new List<Community>();
			foreach (Character member in character.Community.Members)
			{
				if (member != character && member.GetBaseObjectType() == BaseObjectType.Human)
				{
					list.Add(member);
				}
			}
			if (list.Count < 2)
			{
				foreach (Character character6 in Session.Instance.CharacterManager.Characters)
				{
					if (character6 != character && character6.AliveAndNotZombie && character6.NameKnown && !character6.Disappeared && character6.GetBaseObjectType() == BaseObjectType.Human && !string.IsNullOrEmpty(character6.FirstName) && !string.IsNullOrEmpty(character6.Surname) && !list.Contains(character6))
					{
						list.Add(character6);
					}
				}
			}
			if (list.Count < 2)
			{
				foreach (Character character7 in Session.Instance.CharacterManager.Characters)
				{
					if (character7 != character && character7.GetBaseObjectType() == BaseObjectType.Human && !string.IsNullOrEmpty(character7.FirstName) && !string.IsNullOrEmpty(character7.Surname) && !list.Contains(character7))
					{
						list.Add(character7);
					}
				}
			}
			foreach (Community community2 in Session.Instance.CommunityManager.Communities)
			{
				if (character.Community != community2 && !community2.IsAmbientCommunity() && Session.Instance.CommunityManager.GetRelationship(character.Community, community2) >= CommunityRelationshipType.Known && community2.HasAnyActiveMembers())
				{
					list2.Add(community2);
				}
			}
			if (list2.Count < 1)
			{
				foreach (Community community3 in Session.Instance.CommunityManager.Communities)
				{
					if (character.Community != community3 && !community3.IsAmbientCommunity() && Session.Instance.CommunityManager.GetRelationship(character.Community, community3) >= CommunityRelationshipType.Known && !list2.Contains(community3))
					{
						list2.Add(community3);
					}
				}
			}
			if (list2.Count < 1)
			{
				foreach (Community community4 in Session.Instance.CommunityManager.Communities)
				{
					if (character.Community != community4 && !community4.IsAmbientCommunity() && !list2.Contains(community4))
					{
						list2.Add(community4);
					}
				}
			}
			if (list.Count <= 0)
			{
				break;
			}
			Character character2 = ((character.Community != null) ? character.Community.Leader : null);
			if (!list.Contains(character2))
			{
				character2 = null;
			}
			Character character3 = list[deterministicRand.Next() % list.Count];
			list.Remove(character3);
			Character character4 = ((list.Count > 0) ? list[deterministicRand.Next() % list.Count] : null);
			Community community = ((list2.Count > 0) ? list2[deterministicRand.Next() % list2.Count] : null);
			List<MemoryPrototype> list3 = new List<MemoryPrototype>();
			foreach (KeyValuePair<string, MemoryPrototype> item in GameImpl.Instance.CurrentMemoryPrototypesDeterministic)
			{
				MemoryPrototype value = item.Value;
				if (!value.CanBeARumour)
				{
					continue;
				}
				Character character5 = (value.ActorMustBeLeader ? character2 : (value.ActorMustBeSelf ? character : character3));
				if (character5 == null)
				{
					continue;
				}
				BaseObject obj;
				switch (value.ObjectType)
				{
				case MemoryObjectType.None:
					obj = null;
					break;
				case MemoryObjectType.Character:
				{
					if (character4 == null)
					{
						continue;
					}
					bool flag = !character4.AliveAndNotZombie;
					if (value.ObjectMustBeDead != flag || (value.MustBeACouple && Relationship.GetPartner(character5) != character4) || (value.MustBeAttractedTo && (!character5.IsAttractedTo(character4.GetGender()) || !character4.IsAttractedTo(character5.GetGender()))))
					{
						continue;
					}
					obj = character4;
					break;
				}
				case MemoryObjectType.Community:
					if (community == null)
					{
						continue;
					}
					obj = community;
					break;
				default:
					continue;
				}
				if (!character.HasMemory(item.Value, character5, obj))
				{
					list3.Add(item.Value);
				}
			}
			if (list3.Count > 0)
			{
				MemoryPrototype memoryPrototype = list3[deterministicRand.Next() % list3.Count];
				Character actor = (memoryPrototype.ActorMustBeLeader ? character2 : (memoryPrototype.ActorMustBeSelf ? character : character3));
				BaseObject obj2 = null;
				switch (memoryPrototype.ObjectType)
				{
				case MemoryObjectType.None:
					obj2 = null;
					break;
				case MemoryObjectType.Character:
					obj2 = character4;
					break;
				case MemoryObjectType.Community:
					obj2 = community;
					break;
				}
				Memory memory = Memory.Create(memoryPrototype, actor, obj2, null, 1f, fakeNews: true);
				memoryParam = new MemoryParam(memory);
				Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, memoryParam);
				if (speechForMemory != null)
				{
					return SpeechWithParams.Create(speechForMemory, obj2, memoryParam);
				}
				Debug.Log("Couldn't find speech for " + memoryPrototype.UniqueID);
			}
			break;
		}
		}
		return default(SpeechWithParams);
	}

	protected void SetSpeechWithParams(SpeechWithParams speechWithParams)
	{
		Speech = speechWithParams.Speech;
		SpeechObject = speechWithParams.Obj;
		MemoryParam = speechWithParams.Params;
	}

	public void Skip(Character character)
	{
		if (character.IsSpeechSkippable())
		{
			TimeSpan timeSpan = Session.Instance.PlayTime - character.SpeechStartTime;
			if (_started && !Finished && character.Speaking == Speech && timeSpan >= TimeSpan.FromSeconds(0.10000000149011612))
			{
				character.SkipSpeech(onlySkipLipsMoving: false);
			}
		}
	}
}
