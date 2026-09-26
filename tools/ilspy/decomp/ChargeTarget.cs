using System;

public class ChargeTarget : StateMachineGoal
{
	private TimeSpan _voiceSoundTime;

	public static float JumpDist = 3f;

	public static float RunupDist = 2.5f;

	public static float StandingBiteDist = 0.75f;

	private static TimeSpan MinTimeBetweenVoiceSound = TimeSpan.FromSeconds(2.0);

	private static TimeSpan MaxTimeBetweenVoiceSound = TimeSpan.FromSeconds(4.0);

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.IsDoingNetworkChecksum)
		{
			TimeSpan value = TimeSpan.Zero;
			reflector.Add(ref value);
		}
		else
		{
			reflector.Add(ref _voiceSoundTime);
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ChargeTarget;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		return true;
	}

	private void PlayAttackSound(Character character)
	{
		character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		_voiceSoundTime = currentTime + TimeSpan.FromTicks(MathUtil.RandomInt((int)currentTime.Ticks, (int)MinTimeBetweenVoiceSound.Ticks, (int)MaxTimeBetweenVoiceSound.Ticks));
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (MathUtil.ToXZ(Target.Object.Pos - character.Position).sqrMagnitude < RunupDist * RunupDist || GetTargetCharacter().IsBeingBittenOrChoked() || character.ShouldLimp() || MathUtil.RandomChoice((float)currentTime.TotalMilliseconds + (float)character.Id, 0.5f) || character.InvisibleStrainJustActivated != TimeSpan.Zero)
		{
			SetSubGoal(character, parent, new MoveDirectlyToTarget(character, character.ShouldLimp() ? MovementType.Walk : MovementType.Run, StandingBiteDist));
		}
		else
		{
			SetSubGoal(character, parent, new MoveDirectlyToTarget(character, MovementType.Run, JumpDist));
		}
		if (character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			PlayAttackSound(character);
		}
		if (character.IsAuthoritative() && !character.IsBeingChoked())
		{
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, GetTargetCharacter(), character, character));
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished)
		{
			if (character.CurrentActionAnim == ActionAnim.Dodge_Backwards && character.IsActionAnimInterruptible())
			{
				character.StopActionAnim(ActionAnim.Dodge_Backwards);
			}
			if (!character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && !Session.Instance.IsPaused() && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound) && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) >= _voiceSoundTime && MathUtil.ToXZ(Target.Object.Pos - character.Position).sqrMagnitude <= FeedOnLiving.MaxChargeDist * FeedOnLiving.MaxChargeDist)
			{
				PlayAttackSound(character);
			}
			if (character.IsRagdollOrProneOrRecovering() || character.IsInDamageReactionAnim())
			{
				Finished = true;
			}
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (!targetCharacter.IsAwake)
		{
			return null;
		}
		if (SubGoal is MoveDirectlyToTarget moveDirectlyToTarget)
		{
			if (moveDirectlyToTarget.Success)
			{
				if (moveDirectlyToTarget.DesiredRange == StandingBiteDist && targetCharacter is Human && targetCharacter.IsAwake)
				{
					if (FeedOnLiving.CanAttackTarget(character, targetCharacter))
					{
						return new BiteTarget(fromJumping: false);
					}
					return null;
				}
				if (FeedOnLiving.CanAttackTarget(character, targetCharacter))
				{
					return new ZombieJumpGoal(MathUtil.SafeNormalize(MathUtil.ToXZ(Target.Object.Pos - character.Position), MathUtil.ToXZ(character.Forward)));
				}
				return null;
			}
			return null;
		}
		if (SubGoal is ZombieJumpGoal zombieJumpGoal)
		{
			if (zombieJumpGoal.Success)
			{
				return new BiteTarget(fromJumping: true);
			}
			return new TurnToTarget(MathF.PI / 10f);
		}
		return base.GetNextSubGoal(character, parent);
	}
}
