using System;
using UnityEngine;

public class BiteTarget : StateMachineGoal
{
	private bool FromJumping;

	public const float BiteDist = 0.25f;

	public const float MaxStartBiteDist = 0.75f;

	public BiteTarget()
	{
	}

	public BiteTarget(bool fromJumping)
	{
		FromJumping = fromJumping;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FromJumping);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BiteTarget;
	}

	public static Vector3 GetPosForBitingAnimation(Character character, Character targetCharacter, bool fromJumping, Vector3 startPos)
	{
		Vector3 vector = MathUtil.ToX0Y(MathUtil.SafeNormalize(MathUtil.ToXZ(startPos) - targetCharacter.PosXZ, -MathUtil.GetDirFromAngle(character.DesiredFacingAngle)));
		return targetCharacter.Position + vector * (fromJumping ? 0f : 0.25f);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.InsideBuilding != null)
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || IsTargetDeleted())
		{
			return false;
		}
		if (targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (!Active)
		{
			if ((MathUtil.ToXZ(targetCharacter.Position) - MathUtil.ToXZ(character.Position)).sqrMagnitude > MathUtil.Squared(0.75f))
			{
				return false;
			}
			if (!targetCharacter.IsAwake)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (character.IsParrying() || character.IsDodging() || character.IsRagdollOrProneOrRecovering() || character.IsInDamageReactionAnim() || character.IsGettingUp())
		{
			Finished = true;
		}
		else if ((SubGoal is StartBiteTarget && FromJumping) || SubGoal is PrepareBiteTarget || SubGoal is LoopBiteTarget)
		{
			if (!targetCharacter.IsAwake || targetCharacter.GetBaseObjectType() != BaseObjectType.Human)
			{
				SetSubGoal(character, parent, new FinishBiteTarget(FromJumping));
			}
			else if (!character.Zombie && character.InvisibleStrain != InvisibleStrainType.None && targetCharacter.InvisibleStrain != InvisibleStrainType.None && (character.CurrentActionAnim != ActionAnim.ZombieBiteLoop || character.GetActionAnimTime() >= character.GetActionAnimDuration()))
			{
				SetSubGoal(character, parent, new FinishBiteTarget(FromJumping));
			}
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.HaltMovement();
		Character targetCharacter = GetTargetCharacter();
		if (FromJumping && character.IsPredicted() == targetCharacter.IsPredicted() && !targetCharacter.OnZombieBiteStarted(character, FromJumping))
		{
			Finished = true;
			return;
		}
		SetSubGoal(character, parent, new StartBiteTarget(FromJumping));
		if (!character.IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl) && character.CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
		}
		if (character.IsAuthoritative() && !character.IsBeingChoked() && targetCharacter != null)
		{
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, targetCharacter, character, character));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsPredicted() == targetCharacter.IsPredicted())
		{
			targetCharacter.OnZombieBiteCancelled(character);
		}
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is StartBiteTarget startBiteTarget)
		{
			Character targetCharacter = GetTargetCharacter();
			if (!FromJumping && (targetCharacter.IsParryingAttack(character, AttackType.ZombieBite, Character.ZombieBiteDamage) || !character.IsFacing(targetCharacter.PosXZ, MathF.PI / 4f) || (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude >= 0.5625f))
			{
				return new ZombieStumble();
			}
			if (!FromJumping && character.IsPredicted() == targetCharacter.IsPredicted() && !targetCharacter.OnZombieBiteStarted(character, FromJumping))
			{
				return null;
			}
			return new PrepareBiteTarget(startBiteTarget.PosWhenAnimStarted, startBiteTarget.FromJumping);
		}
		if (SubGoal is PrepareBiteTarget prepareBiteTarget)
		{
			return new LoopBiteTarget(prepareBiteTarget.StartPos, prepareBiteTarget.FromJumping);
		}
		return null;
	}

	public override void OnTargetStruggledFree(Character character, Goal parent, Character target)
	{
		if (Target.Object == target && (SubGoal is PrepareBiteTarget || SubGoal is LoopBiteTarget || SubGoal is StartBiteTarget))
		{
			if (!FromJumping && SubGoal is StartBiteTarget)
			{
				Finished = true;
			}
			else
			{
				SetSubGoal(character, parent, new FailBiteTarget(FromJumping));
			}
		}
	}
}
