using System;
using UnityEngine;

public class TakeCoverFromTargetWhileFiring : TakeCoverFromTarget
{
	public TakeCoverFromTargetWhileFiring()
	{
	}

	public TakeCoverFromTargetWhileFiring(bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
		: base(MovementType.Jog, dontOpenOurGates)
	{
		Aiming = true;
		StayInRangeParams = stayInRangeParams;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeCoverFromTargetWhileFiring;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished && (character.GetCurrentActionPriority() < ActionPriority.Attack || character.IsActionAnimFinished()) && character.IsFacing(MathUtil.ToXZ(Target.LastKnownPosition), MathF.PI / 10f) && character.IsLongEnoughSinceWeLastFired() && RangedAttack.CanAttack(character, Target, Crouching, canDestroyNearbyBuildings: false))
		{
			character.TryStartActionAnim(character.GetFireAction(out var animSpeed), GetTargetObject(), animSpeed);
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Fire)
		{
			if (character.EquippedItem is RangedWeapon rangedWeapon)
			{
				rangedWeapon.OnFired(character, Target, TargettableBodyLocation.Torso, Vector3.zero, 0f, 0f, assassinate: false, SecrecyMode.Public, fromAI: true);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
