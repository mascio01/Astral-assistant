using System;
using UnityEngine;

public class StartBiteTarget : AnimationGoal
{
	public bool FromJumping;

	public StartBiteTarget()
	{
	}

	public StartBiteTarget(bool fromJumping)
		: base(fromJumping ? ActionAnim.ZombieJumpBiteStart : ActionAnim.ZombieBiteStart)
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
		return GoalType.StartBiteTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (AnimState != SpeechAnimState.NotStarted)
		{
			Character targetCharacter = GetTargetCharacter();
			if (character.GetActionAnimPlayedFrac() >= 0.5f && targetCharacter.CurrentActionAnim == ActionAnim.SnapAttack && targetCharacter.IsFacing(character.PosXZ, MathF.PI / 4f) && character.IsPredicted() == targetCharacter.IsPredicted() && targetCharacter.CanAttackGrabbingZombie != CanAttackState.HasAttacked)
			{
				targetCharacter.CanAttackGrabbingZombie = CanAttackState.CanAttack;
			}
		}
	}

	public override bool WantPosInterpolation(Character character, out Vector3 destPos)
	{
		Character targetCharacter = GetTargetCharacter();
		destPos = BiteTarget.GetPosForBitingAnimation(character, targetCharacter, FromJumping, PosWhenAnimStarted);
		return true;
	}

	public override bool WantFaceTarget(Character character)
	{
		if (FromJumping)
		{
			return false;
		}
		return base.WantFaceTarget(character);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.ChanceToBlockZombieBite:
		{
			Character targetCharacter2 = GetTargetCharacter();
			if (targetCharacter2 != null && targetCharacter2.IsPredicted() == character.IsPredicted())
			{
				targetCharacter2.OnChanceToParry(character, AttackType.ZombieBite);
			}
			break;
		}
		case AnimationEventType.ChanceToBlockZombieJump:
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, AttackType.ZombieJump);
			}
			break;
		}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
