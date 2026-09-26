using System;

public class PlayerChooseReply : FaceTarget, ISpeechGoal
{
	public Speech SpeechToReplyTo;

	public TimeSpan LastTimeTheyWereTalkingToMe;

	public PlayerChooseReply()
	{
	}

	public PlayerChooseReply(Speech speechToReplyTo)
	{
		SpeechToReplyTo = speechToReplyTo;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref SpeechToReplyTo);
		reflector.AddAfter(ref LastTimeTheyWereTalkingToMe, 443);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.PlayerChooseReply;
	}

	public Speech GetSpeech()
	{
		return SpeechToReplyTo;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (!SpeechToReplyTo.CanTalkOutOfRange || (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= (float)MathUtil.Squared(SpeakToTarget.MaxFaceTargetDist))
		{
			character.GoalTarget = Target;
		}
		if (targetCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation && conversation.GetTargetCharacter() == character)
		{
			LastTimeTheyWereTalkingToMe = Session.Instance.PlayTime;
		}
		else if (Session.Instance.PlayTime - LastTimeTheyWereTalkingToMe >= TimeSpan.FromSeconds(10.0))
		{
			Finished = true;
		}
		base.Update(character, parent);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}
}
