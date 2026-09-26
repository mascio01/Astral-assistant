using System.Collections.Generic;

public class GiveEulogy : SpeakToTarget
{
	private List<SpeechWithParams> _speeches = new List<SpeechWithParams>();

	public GiveEulogy()
	{
	}

	public GiveEulogy(Speech speech, BaseObject speechObject)
		: base(speech, speechObject, default(MemoryParam), null, null, default(MemoryParam))
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GiveEulogy;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _speeches);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		_speeches.Add(SpeechWithParams.Create(Speech, null, default(MemoryParam)));
		int num = 0;
		for (int i = 0; i < character.Memories.Count && num < 3; i++)
		{
			if (character.Memories[i].Prototype == MemoryPrototype.ContractedInvisibleStrain)
			{
				if (character.Memories[i].Object != targetCharacter)
				{
					continue;
				}
			}
			else if (character.Memories[i].Actor != targetCharacter)
			{
				continue;
			}
			if (!character.Memories[i].IsTrivial())
			{
				Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(character, targetCharacter, new MemoryParam(character, i));
				if (speechForMemory != null)
				{
					_speeches.Insert(1, SpeechWithParams.Create(speechForMemory, character.Memories[i].Object, new MemoryParam(character, i)));
					num++;
				}
			}
		}
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Sendoff);
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
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			targetCharacter.EulogyGiven = true;
			if (targetCharacter.BuriedInGrave != null)
			{
				targetCharacter.BuriedInGrave.VisitGrave();
			}
		}
		return default(SpeechWithParams);
	}
}
