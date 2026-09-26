using UnityEngine;

public class LoopBiteTarget : AnimationGoal
{
	public Vector3 StartPos;

	public bool FromJumping;

	public LoopBiteTarget()
	{
	}

	public LoopBiteTarget(Vector3 startPos, bool fromJumping)
		: base(fromJumping ? ActionAnim.ZombieJumpBiteLoop : ActionAnim.ZombieBiteLoop)
	{
		StartPos = startPos;
		FromJumping = fromJumping;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FromJumping);
		reflector.Add(ref StartPos);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LoopBiteTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		character.SetPosition(BiteTarget.GetPosForBitingAnimation(character, GetTargetCharacter(), FromJumping, StartPos));
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		Character targetCharacter = GetTargetCharacter();
		switch (animEvent.EventType)
		{
		case AnimationEventType.BiteLoopStart:
			if (!character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
			}
			if (character.IsAuthoritative() && !character.IsBeingChoked())
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, targetCharacter, character, character));
			}
			if (targetCharacter != null && character.IsPredicted() == targetCharacter.IsPredicted())
			{
				targetCharacter.OnZombieBiteLoopStart(character);
			}
			return true;
		case AnimationEventType.Bite:
			if (targetCharacter != null && character.IsPredicted() == targetCharacter.IsPredicted())
			{
				targetCharacter.OnZombieBite(character);
			}
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}

	public override bool WantFaceTarget(Character character)
	{
		if (FromJumping)
		{
			return false;
		}
		return base.WantFaceTarget(character);
	}
}
