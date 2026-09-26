using System;
using UnityEngine;

public class MoveWithinRangeOfTarget : MoveToTarget
{
	public float MinRange;

	public float MaxRange;

	public StayInRangeParams StayInRangeParams;

	public float MinDistFromStart;

	protected Vector3 _requestedDestPos;

	public MoveWithinRangeOfTarget()
	{
	}

	public MoveWithinRangeOfTarget(MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates)
		: base(movementType)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		MinRange = minRange;
		MaxRange = maxRange;
		Aiming = aiming;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public MoveWithinRangeOfTarget(MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
		: base(movementType)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		MinRange = minRange;
		MaxRange = maxRange;
		Aiming = aiming;
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public MoveWithinRangeOfTarget(MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates, StayInRangeParams stayInRangeParams, float minDistFromStart)
		: base(movementType)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		MinRange = minRange;
		MaxRange = maxRange;
		Aiming = aiming;
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
		MinDistFromStart = minDistFromStart;
	}

	public MoveWithinRangeOfTarget(Character character, Character targetCharacter, MovementType movementType, bool aiming, float minRange, float maxRange, bool dontOpenOurGates)
		: base(movementType)
	{
		maxRange = Math.Max(minRange + 0.1f, maxRange);
		MinRange = minRange;
		MaxRange = maxRange;
		Aiming = aiming;
		_dontOpenOurGates = dontOpenOurGates;
		SetTarget(character, null, character.GetOrCreateTarget(targetCharacter));
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MinRange);
		reflector.Add(ref MaxRange);
		StayInRangeParams.Reflect(reflector);
		if (reflector.Version < 175)
		{
			float value = float.MaxValue;
			reflector.AddAfter(ref value, 58);
			if (value != float.MaxValue)
			{
				StayInRangeParams.StayInRangeOf = StayInRangeOf.SquadLeader;
			}
		}
		reflector.Add(ref _requestedDestPos);
		if (reflector.Version < 62)
		{
			reflector.Add(ref _dontOpenOurGates);
		}
		reflector.AddAfter(ref MinDistFromStart, 297);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveWithinRangeOfTarget;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		_requestedDestPos = Target.LastKnownPosition;
		_requester.StartWithinRangeRequest(GetStartTile(character), _requestedDestPos, MinRange, MaxRange, character, Target.Object, AStarPriority, _dontOpenOurGates, resultTile, resultDist, MinDistFromStart, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId, CanUnlockGatesFromInsideWithKey);
	}

	public virtual bool IsInRangeOfTarget(Character character, Goal parent)
	{
		float num = Vector2.SqrMagnitude(Target.Object.PosXZ - MathUtil.ToXZ(character.Position));
		if (num >= MinRange * MinRange && num <= MaxRange * MaxRange)
		{
			return true;
		}
		return false;
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		return DestTile.GetDist(_requestedDestTile) >= Math.Max(character.Tile.GetDist(_requestedDestTile) - MaxRange, 1f);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished && SubGoal == null && IsInRangeOfTarget(character, parent))
		{
			Finished = true;
			Success = true;
		}
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (IsInRangeOfTarget(character, parent))
		{
			return MoveToResult.Success;
		}
		if (_requestedDestTile != DestTile)
		{
			return MoveToResult.Working;
		}
		if (Vector2.SqrMagnitude(MathUtil.ToXZ(Target.LastKnownPosition) - character.PosXZ) > Vector2.SqrMagnitude(MathUtil.ToXZ(_requestedDestPos) - character.PosXZ))
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Fail;
	}
}
