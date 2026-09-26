using System;
using System.Collections.Generic;

public class TalkToGrave : SpeakToTarget
{
	private List<SpeechWithParams> _speeches = new List<SpeechWithParams>();

	public TalkToGrave()
	{
	}

	public TalkToGrave(Speech speech, BaseObject speechObject)
		: base(speech, speechObject, default(MemoryParam), null, null, default(MemoryParam))
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TalkToGrave;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _speeches);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Character targetCharacter = GetTargetCharacter();
		float num = character.CalcMorale();
		_speeches.Add(SpeechWithParams.Create(Speech, null, default(MemoryParam)));
		MemoryIndexScore.MemoryIndices.Clear();
		if (targetCharacter.BuriedInGrave != null)
		{
			float num2 = 0f;
			MemoryIndexScore item = default(MemoryIndexScore);
			for (int i = 0; i < character.Memories.Count; i++)
			{
				if (!(character.Memories[i].Time < targetCharacter.BuriedInGrave.LastVisitedTime) && character.Memories[i].Actor != character)
				{
					float priority = character.Memories[i].Priority;
					num2 = Math.Max(num2, priority);
					item.Index = i;
					item.Score = priority;
					MemoryIndexScore.MemoryIndices.Add(item);
				}
			}
			for (int j = 0; j < MemoryIndexScore.MemoryIndices.Count; j++)
			{
				MemoryIndexScore value = MemoryIndexScore.MemoryIndices[j];
				value.Score += deterministicRand.RandomFloat() * num2;
				MemoryIndexScore.MemoryIndices[j] = value;
			}
			MemoryIndexScore.MemoryIndices.InsertionSort(MemoryIndexScore.Sorter);
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
					if (!(num3 * num < 0f) || !(approvalContribution * num < 0f) || k != 0)
					{
						Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, new MemoryParam(character, index));
						if (speechForMemory != null)
						{
							num3 += approvalContribution;
							_speeches.Add(SpeechWithParams.Create(speechForMemory, obj, new MemoryParam(character, index)));
							num4++;
							MemoryIndexScore.MemoryIndices.RemoveAt(l);
							l--;
						}
					}
				}
			}
		}
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Morale);
		if (speechForSituation != null)
		{
			_speeches.Add(SpeechWithParams.Create(speechForSituation, null, default(MemoryParam)));
		}
		Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.GoodbyeGrave);
		if (speechForSituation2 != null)
		{
			_speeches.Add(SpeechWithParams.Create(speechForSituation2, null, default(MemoryParam)));
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
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.BuriedInGrave != null)
		{
			targetCharacter.BuriedInGrave.VisitGrave();
		}
		return default(SpeechWithParams);
	}
}
