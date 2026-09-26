using System;

public class MeleeAttackAnim : AnimationGoal
{
	public AttackType AttackType;

	public MeleeAttackAnim()
	{
	}

	public MeleeAttackAnim(AttackType attackType)
	{
		AttackType = attackType;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MeleeAttackAnim;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref AttackType);
	}

	public override bool CanStartAnimation(Character character, Goal parent)
	{
		if (character.IsParrying() || character.IsDodging())
		{
			return false;
		}
		return base.CanStartAnimation(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		switch (AttackType)
		{
		case AttackType.Invalid:
			switch (MathUtil.RandomInt(((int)currentTime.Ticks ^ (int)(currentTime.Ticks >> 32)) * 4 + character.Id * 3000 + 890, 5))
			{
			case 0:
			case 1:
				Anim = ActionAnim.PunchLeft;
				break;
			case 2:
			case 3:
				Anim = ActionAnim.PunchRight;
				break;
			case 4:
				Anim = ActionAnim.Kick;
				break;
			}
			break;
		case AttackType.Punch:
			Anim = (MathUtil.RandomChoice((float)currentTime.TotalMilliseconds, 0.5f) ? ActionAnim.PunchLeft : ActionAnim.PunchRight);
			break;
		case AttackType.Snap:
			Anim = ActionAnim.SnapAttack;
			break;
		case AttackType.AttackJumpingZombie:
			Anim = ActionAnim.AttackJumpingZombie;
			break;
		case AttackType.Kick:
			Anim = ActionAnim.Kick;
			break;
		default:
			Anim = ActionAnim.Attack;
			break;
		}
		base.OnActivate(character, parent);
		Target.ClearInaccessible();
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !Finished)
		{
			character.SetRecentActivity(RecentActivityType.Combat, targetCharacter);
		}
		if (character.IsTargetSurrendering(targetCharacter) || MeleeAttack.TargetIsBeingRestrained(character, targetCharacter))
		{
			Finished = true;
		}
		if (character.CurrentActionAnim != Anim && !character.CanMeleeAttack(GetTargetCharacter(), AttackType))
		{
			Finished = true;
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		Character targetCharacter = GetTargetCharacter();
		switch (animEvent.EventType)
		{
		case AnimationEventType.PunchLeft:
			character.OnMeleeAttack(targetCharacter, Character.GetUnarmedAttackTypeFromStance(character.GetCurrentTargetBodyLocation()), InjuryType.Punch, Bone.LeftHand, Character.PunchDamage * character.GetUnarmedDamageModifier(), assassinate: false, stealthy: false);
			return true;
		case AnimationEventType.PunchRight:
			character.OnMeleeAttack(targetCharacter, Character.GetUnarmedAttackTypeFromStance(character.GetCurrentTargetBodyLocation()), InjuryType.Punch, Bone.RightHand, Character.PunchDamage * character.GetUnarmedDamageModifier(), assassinate: false, stealthy: false);
			return true;
		case AnimationEventType.KickLeft:
			character.OnMeleeAttack(targetCharacter, AttackType.Kick, InjuryType.Punch, Bone.LeftFoot, Character.KickDamage * character.GetUnarmedDamageModifier(), assassinate: false, stealthy: false);
			return true;
		case AnimationEventType.KickRight:
			character.OnMeleeAttack(targetCharacter, AttackType.Kick, InjuryType.Punch, Bone.RightFoot, Character.KickDamage * character.GetUnarmedDamageModifier(), assassinate: false, stealthy: false);
			return true;
		case AnimationEventType.MeleeWeaponQuickAttack:
			if (character.EquippedItem != null)
			{
				character.OnMeleeAttack(targetCharacter, Character.GetMeleeWeaponAttackTypeFromStance(character.GetCurrentTargetBodyLocation()), character.EquippedItem.GetInjuryType(), Bone.MeleeWeapon, character.EquippedItem.GetDamageIncludingEffects(character) * Character.QuickAttackDamageModifier, assassinate: false, stealthy: false);
			}
			return true;
		case AnimationEventType.MeleeWeaponHeavyAttack:
			if (character.EquippedItem != null)
			{
				character.OnMeleeAttack(targetCharacter, Character.GetMeleeWeaponAttackTypeFromStance(character.GetCurrentTargetBodyLocation()), character.EquippedItem.GetInjuryType(), Bone.MeleeWeapon, character.EquippedItem.GetDamageIncludingEffects(character), assassinate: false, stealthy: false);
			}
			return true;
		case AnimationEventType.ChanceToParryHigh:
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, AttackType.High);
			}
			return true;
		case AnimationEventType.ChanceToParryMiddle:
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, AttackType.Middle);
			}
			return true;
		case AnimationEventType.ChanceToParryLow:
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, AttackType.Low);
			}
			return true;
		case AnimationEventType.ChanceToDodgeKick:
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, AttackType.Kick);
			}
			return true;
		case AnimationEventType.ChanceToBlockPunch:
			if (targetCharacter != null && targetCharacter.IsPredicted() == character.IsPredicted())
			{
				targetCharacter.OnChanceToParry(character, Character.GetUnarmedAttackTypeFromStance(character.GetCurrentTargetBodyLocation()));
			}
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}
}
