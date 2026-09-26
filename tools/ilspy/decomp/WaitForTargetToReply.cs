using System;
using System.Collections.Generic;

public class WaitForTargetToReply : WaitAndFaceTarget, ISpeechGoal
{
	public Speech MyLastSpeech;

	public Speech MyLastSpecialBehaviourTriggeredFromSpeech;

	public BaseObject MyLastSpeechObject;

	public MemoryParam MyLastSpeechParam;

	public List<SpeechParamResult> MyLastSpeechParamResults = new List<SpeechParamResult>();

	public string MyLastSpeechText_DEPRECATED;

	public WaitForTargetToReply()
	{
	}

	public WaitForTargetToReply(Speech myLastSpeech, BaseObject myLastSpeechObject, MemoryParam myLastSpeechParam, List<SpeechParamResult> myLastSpeechParamResults, string myLastSpeechText_DEPRECATED, Speech myLastSpecialBehaviourTriggeredFromSpeech, float timeout)
		: base(TimeSpan.FromSeconds(timeout))
	{
		MyLastSpeech = myLastSpeech;
		MyLastSpeechObject = myLastSpeechObject;
		MyLastSpeechParam = myLastSpeechParam;
		myLastSpeechParamResults.CopyToList(MyLastSpeechParamResults);
		MyLastSpeechText_DEPRECATED = myLastSpeechText_DEPRECATED;
		MyLastSpecialBehaviourTriggeredFromSpeech = myLastSpecialBehaviourTriggeredFromSpeech;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MyLastSpeech);
		reflector.AddAfter(ref MyLastSpeechObject, 24);
		if (reflector.Version >= 266)
		{
			MyLastSpeechParam.Reflect(reflector);
		}
		if (reflector.Version < 4)
		{
			reflector.Add(ref MyLastSpeechText_DEPRECATED);
		}
		else
		{
			reflector.Add(ref MyLastSpeechParamResults);
		}
		reflector.AddAfter(ref MyLastSpecialBehaviourTriggeredFromSpeech, 257);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (!MyLastSpeech.CanTalkOutOfRange || (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= (float)MathUtil.Squared(SpeakToTarget.MaxFaceTargetDist))
		{
			character.GoalTarget = Target;
		}
		base.Update(character, parent);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.WaitForTargetToReply;
	}

	public Speech GetSpeech()
	{
		return MyLastSpeech;
	}

	public string GetMyLastSpeechText(Character character)
	{
		if (MyLastSpeechText_DEPRECATED != null)
		{
			return MyLastSpeechText_DEPRECATED;
		}
		Speech.BuildSpeechText(MyLastSpeech.TextHash, MyLastSpeech.Params, character, GetTargetObject(), MyLastSpeechObject, MyLastSpeechParamResults, out var speechText, null, englishOnly: false, isQuest: false);
		return speechText;
	}
}
