using System;
using UnityEngine;

public class MoveWithinRangeAndSightOfTarget : MoveWithinRangeOfTarget
{
	private TerrainCoord StartTile;

	public MoveWithinRangeAndSightOfTarget()
	{
	}

	public MoveWithinRangeAndSightOfTarget(MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates, StayInRangeParams stayInRangeParams, float minDistFromStart)
		: base(movementType, aiming, minRange, maxRange, dontOpenOurGates, stayInRangeParams, minDistFromStart)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
	}

	public MoveWithinRangeAndSightOfTarget(Character character, Character targetCharacter, MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates, StayInRangeParams stayInRangeParams, float minDistFromStart)
		: base(movementType, aiming, minRange, maxRange, dontOpenOurGates, stayInRangeParams, minDistFromStart)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		SetTarget(character, null, character.GetOrCreateTarget(targetCharacter));
		HasUserTarget = true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTile = character.Tile;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.Version < 297)
		{
			reflector.AddAfter(ref MinDistFromStart, 62);
		}
		reflector.AddAfter(ref StartTile, 62);
		if (reflector.Version < 62)
		{
			reflector.Add(ref _dontOpenOurGates);
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveWithinRangeAndSightOfTarget;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		bool flag = parent is MeleeAttack;
		if (parent is RangedAttack)
		{
			flag |= character.EquippedItem is Throwable || character.EquippedItem is RPG;
		}
		Vector3 targetAimPos = character.GetTargetAimPos(Target, TargettableBodyLocation.Count, Target.LastKnownPosition, deterministic: true);
		bool flag2 = GetTargetCharacter()?.IsGuarding() ?? false;
		bool targetIsCrouching = Target.GetFlag(TargetFlags.Crouching) && !flag2;
		_requestedDestPos = Target.LastKnownPosition;
		_requester.StartVisibleAndWithinRangeRequest(GetStartTile(character), targetAimPos, targetIsCrouching, MinRange, MaxRange, character, Target.Object, AStarPriority, ignoreFlammableDefences: false, ignoreExplodableDefences: false, _dontOpenOurGates, resultTile, resultDist, MinDistFromStart, flag, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override bool IsInRangeOfTarget(Character character, Goal parent)
	{
		if (!base.IsInRangeOfTarget(character, parent))
		{
			return false;
		}
		if (MinDistFromStart > 0f && character.Tile.GetDistSquared(StartTile) < MinDistFromStart * MinDistFromStart)
		{
			return false;
		}
		if (parent is RangedAttack)
		{
			return RangedAttack.CanAttack(character, Target, character.IsCrouching(), canDestroyNearbyBuildings: false);
		}
		if (parent is MeleeAttack)
		{
			return character.CanMeleeAttack(Target.Object, AttackType.Middle);
		}
		return true;
	}
}
