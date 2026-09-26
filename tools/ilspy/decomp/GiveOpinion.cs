using System;
using System.Collections.Generic;

public class GiveOpinion : SpeakToTarget
{
	private Character OpinionOf;

	private List<SpeechWithParams> _speeches = new List<SpeechWithParams>();

	public GiveOpinion()
	{
	}

	public GiveOpinion(Character opinionOf)
		: base(null, null, default(MemoryParam), null, null, default(MemoryParam))
	{
		OpinionOf = opinionOf;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _speeches);
		reflector.AddAfter(ref OpinionOf, 546);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GiveOpinion;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Character targetCharacter = GetTargetCharacter();
		if (OpinionOf == null)
		{
			OpinionOf = targetCharacter;
		}
		MemoryIndexScore.MemoryIndices.Clear();
		float num = 0f;
		MemoryIndexScore item = default(MemoryIndexScore);
		for (int i = 0; i < character.Memories.Count; i++)
		{
			if (((character.Memories[i].Object == OpinionOf && character.Memories[i].Prototype.RuleSet == MemoryRuleSet.Envy) || character.Memories[i].Actor == OpinionOf) && !character.Memories[i].IsTrivial())
			{
				float priority = character.Memories[i].Priority;
				num = Math.Max(num, priority);
				item.Index = i;
				item.Score = priority;
				MemoryIndexScore.MemoryIndices.Add(item);
			}
		}
		for (int j = 0; j < MemoryIndexScore.MemoryIndices.Count; j++)
		{
			MemoryIndexScore value = MemoryIndexScore.MemoryIndices[j];
			value.Score += deterministicRand.RandomFloat() * num;
			MemoryIndexScore.MemoryIndices[j] = value;
		}
		MemoryIndexScore.MemoryIndices.InsertionSort(MemoryIndexScore.Sorter);
		character.CalcApprovalRating(OpinionOf, out var approval, out var respect);
		float num2 = 0f;
		float num3 = 0f;
		int num4 = 0;
		for (int k = 0; k < 2; k++)
		{
			if (num4 >= 3)
			{
				break;
			}
			for (int l = 0; l < MemoryIndexScore.MemoryIndices.Count; l++)
			{
				if (num4 >= 3)
				{
					break;
				}
				int index = MemoryIndexScore.MemoryIndices[l].Index;
				BaseObject obj = character.Memories[index].Object;
				float approvalContribution = character.Memories[index].ApprovalContribution;
				float respectContribution = character.Memories[index].RespectContribution;
				if (k != 0 || ((!(num2 * approval < 0f) || !(approvalContribution * approval < 0f)) && (!(num3 * respect < 0f) || !(respectContribution * respect < 0f))))
				{
					Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, new MemoryParam(character, index));
					if (speechForMemory != null)
					{
						num2 += approvalContribution;
						num3 += respectContribution;
						_speeches.Add(SpeechWithParams.Create(speechForMemory, obj, new MemoryParam(character, index)));
						num4++;
						MemoryIndexScore.MemoryIndices.RemoveAt(l);
						l--;
					}
				}
			}
		}
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, OpinionOf, SpeechSituation.Opinion);
		if (speechForSituation != null)
		{
			_speeches.Add(SpeechWithParams.Create(speechForSituation, OpinionOf, default(MemoryParam)));
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
}
