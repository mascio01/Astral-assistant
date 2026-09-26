using System;
using System.Collections.Generic;
using UnityEngine;

public class GreetingResponse : SpeakToTarget
{
	private List<SpeechWithParams> _speeches = new List<SpeechWithParams>();

	public Speech TheirReply;

	public BaseObject TheirReplyReferringTo;

	public MemoryParam TheirReplyMemoryParam;

	public GreetingResponse()
		: base(null, null, default(MemoryParam), null, null, default(MemoryParam))
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _speeches);
		reflector.Add(ref TheirReply);
		reflector.Add(ref TheirReplyReferringTo);
		if (reflector.Version >= 144)
		{
			TheirReplyMemoryParam.Reflect(reflector);
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GreetingResponse;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Morale);
		if (speechForSituation != null)
		{
			_speeches.Add(SpeechWithParams.Create(speechForSituation, null, default(MemoryParam)));
		}
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		MemoryParam memoryParam = default(MemoryParam);
		if (character.InvisibleStrain != InvisibleStrainType.None && character.Community != null && targetCharacter != null && targetCharacter.InvisibleStrain == InvisibleStrainType.None)
		{
			List<Character> list = new List<Character>();
			List<Community> list2 = new List<Community>();
			foreach (Character member in character.Community.Members)
			{
				if (member != character && member != targetCharacter && member.GetBaseObjectType() == BaseObjectType.Human)
				{
					list.Add(member);
				}
			}
			foreach (Community community2 in Session.Instance.CommunityManager.Communities)
			{
				if (character.Community != community2 && !community2.IsAmbientCommunity() && Session.Instance.CommunityManager.GetRelationship(character.Community, community2) >= CommunityRelationshipType.Known && community2.HasAnyActiveMembers())
				{
					list2.Add(community2);
				}
			}
			if (list.Count > 0)
			{
				Character character2 = list[deterministicRand.Next() % list.Count];
				list.Remove(character2);
				Character character3 = ((list.Count > 0) ? list[deterministicRand.Next() % list.Count] : null);
				Community community = ((list2.Count > 0) ? list2[deterministicRand.Next() % list2.Count] : null);
				List<MemoryPrototype> list3 = new List<MemoryPrototype>();
				foreach (KeyValuePair<string, MemoryPrototype> item2 in GameImpl.Instance.CurrentMemoryPrototypesDeterministic)
				{
					MemoryPrototype value = item2.Value;
					if (!value.CanBeARumour || (value.ActorMustBeLeader && character2.Rank != Rank.Leader))
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
						if (character3 == null)
						{
							continue;
						}
						bool flag = !character3.AliveAndNotZombie;
						if (value.ObjectMustBeDead != flag || (value.MustBeACouple && Relationship.GetPartner(character2) != character3) || (value.MustBeAttractedTo && (!character2.IsAttractedTo(character3.GetGender()) || !character3.IsAttractedTo(character2.GetGender()))))
						{
							continue;
						}
						obj = character3;
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
					Memory memory = Memory.Create(value, character2, obj, null, 1f, fakeNews: true);
					memory.CalcApprovalRespectContribution(targetCharacter);
					if (memory.ApprovalContribution < 0f)
					{
						list3.Add(item2.Value);
					}
				}
				if (list3.Count > 0)
				{
					MemoryPrototype memoryPrototype = list3[deterministicRand.Next() % list3.Count];
					BaseObject obj2 = null;
					switch (memoryPrototype.ObjectType)
					{
					case MemoryObjectType.None:
						obj2 = null;
						break;
					case MemoryObjectType.Character:
						obj2 = character3;
						break;
					case MemoryObjectType.Community:
						obj2 = community;
						break;
					}
					Memory memory2 = Memory.Create(memoryPrototype, character2, obj2, null, 1f, fakeNews: true);
					memory2.CalcApprovalRespectContribution(targetCharacter);
					memoryParam = new MemoryParam(memory2);
				}
			}
		}
		if (memoryParam.GetParamType() != ParamType.Memory)
		{
			MemoryIndexScore.MemoryIndices.Clear();
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			float num = character.CalcMorale();
			MemoryIndexScore item = default(MemoryIndexScore);
			for (int i = 0; i < character.Memories.Count; i++)
			{
				if (character.Memories[i].Actor != character && character.Memories[i].Actor != targetCharacter && !character.Memories[i].IsTrivial())
				{
					float num2 = (float)(currentTime - character.Memories[i].Time).TotalSeconds;
					float moraleContribution = character.Memories[i].MoraleContribution;
					if (moraleContribution * num < 0f)
					{
						num2 += 3f * Sun.DayLengthSecs;
					}
					num2 += Mathf.Abs(moraleContribution) * 3f * Sun.DayLengthSecs / 100f;
					if (targetCharacter.FindMemory(character.Memories[i].Prototype, character.Memories[i].Actor, character.Memories[i].Object) != -1)
					{
						num2 += 3f * Sun.DayLengthSecs;
					}
					item.Index = i;
					item.Score = num2;
					MemoryIndexScore.MemoryIndices.Add(item);
				}
			}
			if (MemoryIndexScore.MemoryIndices.Count > 0)
			{
				MemoryIndexScore.MemoryIndices.InsertionSort(MemoryIndexScore.Sorter);
				int index = MemoryIndexScore.MemoryIndices[MemoryIndexScore.MemoryIndices.Count - 1].Index;
				memoryParam = new MemoryParam(character, index);
				MemoryIndexScore.MemoryIndices.Clear();
			}
		}
		if (memoryParam.GetParamType() == ParamType.Memory)
		{
			BaseObject obj3 = memoryParam.GetMemory().Object;
			Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, memoryParam);
			if (speechForMemory != null)
			{
				_speeches.Add(SpeechWithParams.Create(speechForMemory, obj3, memoryParam));
			}
		}
		SpeechWithParams continueOverride = GetContinueOverride(character, parent, interrupted: false);
		if (continueOverride.IsValid())
		{
			SetSpeechWithParams(continueOverride);
		}
		base.OnActivate(character, parent);
	}

	public override SpeechWithParams GetContinueOverride(Character character, Goal parent, bool interrupted)
	{
		if (_speeches.Count > 0)
		{
			SpeechWithParams result = _speeches[0];
			_speeches.RemoveAt(0);
			return result;
		}
		return default(SpeechWithParams);
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		if (Speech == speech && MemoryParam.GetParamType() == ParamType.Memory)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null)
			{
				Memory memory = MemoryParam.GetMemory();
				bool flag = false;
				if (memory.FakeNews)
				{
					character.CalcApprovalRating(memory.Actor, out var approval, out var _);
					float num = approval;
					num += (float)character.GetPersonality(CachedPersonalityType.Skeptical, CachedPersonalityType.Credulous) * 50f;
					if (memory.Actor.Community == character.Community)
					{
						num += (float)character.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle) * 50f;
					}
					flag = num >= 50f;
				}
				if (flag)
				{
					TheirReply = StoryManager.Instance.GetSpeechForSituation(targetCharacter, character, memory.Object, SpeechSituation.MemoryDisbelief, new MemoryParam(memory));
					TheirReplyReferringTo = memory.Object;
					TheirReplyMemoryParam = new MemoryParam(memory);
				}
				else
				{
					int num2 = targetCharacter.FindMemory(memory.Prototype, memory.Actor, memory.Object);
					if (num2 == -1 || targetCharacter.Memories[num2].Time < memory.Time)
					{
						targetCharacter.AddMemory(memory.Prototype, memory.Actor, memory.Object, memory.LastQuantity, memory.FakeNews, forceAdd: false);
						num2 = targetCharacter.FindMemory(memory.Prototype, memory.Actor, memory.Object);
					}
					TheirReply = StoryManager.Instance.GetSpeechForSituation(targetCharacter, character, memory.Object, SpeechSituation.MemoryResponse, new MemoryParam(targetCharacter, num2));
					TheirReplyReferringTo = memory.Object;
					TheirReplyMemoryParam = new MemoryParam(targetCharacter, num2);
				}
			}
		}
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
	}
}
