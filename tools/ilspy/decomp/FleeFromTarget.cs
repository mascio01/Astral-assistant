using UnityEngine;

public class FleeFromTarget : MoveToTarget
{
	public float MinRange;

	public float MaxRange;

	public FleeFromTarget()
	{
	}

	public FleeFromTarget(MovementType movementType, float minRange, float maxRange, bool dontOpenOurGates)
		: base(movementType)
	{
		MinRange = minRange;
		MaxRange = maxRange;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public FleeFromTarget(Character character, Character targetCharacter, MovementType movementType, float minRange, float maxRange)
		: base(movementType)
	{
		MinRange = minRange;
		MaxRange = maxRange;
		SetTarget(character, null, character.GetOrCreateTarget(targetCharacter));
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MinRange);
		reflector.AddAfter(ref MaxRange, 87);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FleeFromTarget;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartFleeRequest(GetStartTile(character), DestTile, MinRange, MaxRange, character, Target.Object, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public virtual bool IsOutOfRangeOfTarget(Character character)
	{
		if (Vector2.SqrMagnitude(GameTerrain.Instance.GetTileCentreXZ(DestTile) - MathUtil.ToXZ(character.Position)) >= MaxRange * MaxRange)
		{
			return true;
		}
		return false;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished && SubGoal == null && IsOutOfRangeOfTarget(character))
		{
			Finished = true;
			Success = true;
		}
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (IsOutOfRangeOfTarget(character))
		{
			return MoveToResult.Success;
		}
		if (_requestedDestTile != DestTile)
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Success;
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		if (_requester != null)
		{
			return false;
		}
		TerrainCoord routeDestination = character.GetRouteDestination();
		return GameTerrain.Instance.IsImpassable(routeDestination.x, routeDestination.y, 2049, character, null);
	}

	public override int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetFurthestEntranceFrom(Target.Object.GetCentreTile(), character);
	}
}
