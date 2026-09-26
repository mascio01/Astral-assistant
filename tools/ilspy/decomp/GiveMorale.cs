using System;
using System.Collections.Generic;
using UnityEngine;

public class GiveMorale : SpeakToTarget
{
	private List<SpeechWithParams> _speeches = new List<SpeechWithParams>();

	public GiveMorale()
		: base(null, null, default(MemoryParam), null, null, default(MemoryParam))
	{
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _speeches);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GiveMorale;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Character targetCharacter = GetTargetCharacter();
		float num = character.CalcMorale();
		MemoryIndexScore.MemoryIndices.Clear();
		float num2 = 0f;
		MemoryIndexScore item = default(MemoryIndexScore);
		for (int i = 0; i < character.Memories.Count; i++)
		{
			if (character.Memories[i].IsTrivial() || character.Memories[i].MoraleContribution == 0f || character.Memories[i].Prototype.RuleSet == MemoryRuleSet.Past)
			{
				continue;
			}
			if (!string.IsNullOrEmpty(character.Memories[i].Prototype.ReciprocalMemoryID) && character.Memories[i].Prototype.ReciprocalMemoryID == character.Memories[i].Prototype.UniqueID)
			{
				bool flag = false;
				for (int j = 0; j < character.Memories.Count; j++)
				{
					if (character.Memories[j].Prototype == character.Memories[i].Prototype && character.Memories[j].Actor == character.Memories[i].Object && character.Memories[j].Object == character.Memories[i].Actor)
					{
						flag = Mathf.Abs(character.Memories[j].MoraleContribution) > Mathf.Abs(character.Memories[i].MoraleContribution);
						if (character.Memories[j].MoraleContribution == character.Memories[i].MoraleContribution)
						{
							flag = character.Memories[i].Actor == character || (character.Memories[i].Object != character && character.Memories[j].Actor.Id > character.Memories[i].Actor.Id);
						}
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			float num3 = Mathf.Abs(character.Memories[i].MoraleContribution);
			num2 = Math.Max(num2, num3);
			item.Index = i;
			item.Score = num3;
			MemoryIndexScore.MemoryIndices.Add(item);
		}
		for (int k = 0; k < MemoryIndexScore.MemoryIndices.Count; k++)
		{
			MemoryIndexScore value = MemoryIndexScore.MemoryIndices[k];
			value.Score += deterministicRand.RandomFloat() * num2;
			MemoryIndexScore.MemoryIndices[k] = value;
		}
		MemoryIndexScore.MemoryIndices.InsertionSort(MemoryIndexScore.Sorter);
		float num4 = 0f;
		int num5 = 0;
		for (int l = 0; l < 2; l++)
		{
			if (num5 >= 3)
			{
				break;
			}
			for (int m = 0; m < MemoryIndexScore.MemoryIndices.Count; m++)
			{
				if (num5 >= 3)
				{
					break;
				}
				int index = MemoryIndexScore.MemoryIndices[m].Index;
				BaseObject obj = character.Memories[index].Object;
				float moraleContribution = character.Memories[index].MoraleContribution;
				if (!(num4 * num < 0f) || !(moraleContribution * num < 0f) || l != 0)
				{
					Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, new MemoryParam(character, index));
					if (speechForMemory != null)
					{
						num4 += moraleContribution;
						_speeches.Add(SpeechWithParams.Create(speechForMemory, obj, new MemoryParam(character, index)));
						num5++;
						MemoryIndexScore.MemoryIndices.RemoveAt(m);
						m--;
					}
				}
			}
		}
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Morale);
		if (speechForSituation != null)
		{
			_speeches.Add(SpeechWithParams.Create(speechForSituation, null, default(MemoryParam)));
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
