using System;

public class ListenToTarget : AnimationGoal, ISpeechGoal
{
	public Speech Speech;

	public BaseObject SpeechObject;

	public MemoryParam SpeechParam;

	public Speech ReplyTo;

	public BaseObject ReplyToReferringTo;

	public MemoryParam ReplyToParam;

	public bool Success;

	public Speech MyReplyOverride;

	public BaseObject MyReplyOverrideReferringTo;

	public MemoryParam MyReplyOverrideMemoryParam;

	public ListenToTarget()
	{
	}

	public ListenToTarget(Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyParam)
	{
		Speech = speech;
		SpeechObject = speechObject;
		SpeechParam = speechParam;
		ReplyTo = replyTo;
		ReplyToReferringTo = replyReferringTo;
		ReplyToParam = replyParam;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Speech);
		reflector.AddAfter(ref SpeechObject, 24);
		if (reflector.Version >= 266)
		{
			SpeechParam.Reflect(reflector);
		}
		reflector.AddAfter(ref ReplyTo, 56);
		reflector.AddAfter(ref ReplyToReferringTo, 56);
		if (reflector.Version >= 266)
		{
			ReplyToParam.Reflect(reflector);
		}
		reflector.Add(ref Success);
		reflector.AddAfter(ref MyReplyOverride, 143);
		reflector.AddAfter(ref MyReplyOverrideReferringTo, 143);
		if (reflector.Version >= 144)
		{
			MyReplyOverrideMemoryParam.Reflect(reflector);
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ListenToTarget;
	}

	public Speech GetSpeech()
	{
		return Speech;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (GetTargetCharacter() == null || IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override int OnlyFaceTargetIfInRange()
	{
		if (!Speech.CanTalkOutOfRange)
		{
			return 0;
		}
		return SpeakToTarget.MaxFaceTargetDist;
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

	public override bool OnSpokenToFinished(Character character, Goal parent, Character speaker, Speech speech, bool interrupted, Speech cont, Speech specialBehaviourTriggered)
	{
		if (Target.Object == speaker && Speech == speech)
		{
			if (cont != null)
			{
				Speech = cont;
			}
			else
			{
				if (specialBehaviourTriggered != null)
				{
					Speech = specialBehaviourTriggered;
				}
				Finished = true;
				Success = !interrupted;
				if (speaker.FindActiveGoal(GoalType.GreetingResponse) is GreetingResponse greetingResponse)
				{
					MyReplyOverride = greetingResponse.TheirReply;
					MyReplyOverrideReferringTo = greetingResponse.TheirReplyReferringTo;
					MyReplyOverrideMemoryParam = greetingResponse.TheirReplyMemoryParam;
				}
			}
		}
		return true;
	}

	public void Skip(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.IsSpeechSkippable())
		{
			TimeSpan timeSpan = Session.Instance.PlayTime - targetCharacter.SpeechStartTime;
			if (!Finished && targetCharacter.Speaking == Speech && timeSpan >= TimeSpan.FromSeconds(0.10000000149011612))
			{
				targetCharacter.SkipSpeech(onlySkipLipsMoving: false);
			}
		}
	}
}
