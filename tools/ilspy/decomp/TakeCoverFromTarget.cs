public class TakeCoverFromTarget : MoveToTarget
{
	public StayInRangeParams StayInRangeParams;

	public static int TakeCoverRange = 12;

	public TakeCoverFromTarget()
	{
	}

	public TakeCoverFromTarget(MovementType movementType, bool dontOpenOurGates)
		: base(movementType)
	{
		MoveToCentreOfTile = true;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeCoverFromTarget;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
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
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		_requester.StartTakeCoverRequest(GetStartTile(character), Target.LastKnownPosition, TakeCoverRange, character, Target.Object, AStarPriority, _dontOpenOurGates, resultTile, resultDist, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		return MoveToResult.Success;
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		if (_requester != null)
		{
			return false;
		}
		TerrainCoord routeDestination = character.GetRouteDestination();
		if (character.Tile != routeDestination)
		{
			return GameTerrain.Instance.IsImpassable(routeDestination.x, routeDestination.y, 2049, character, null);
		}
		return false;
	}

	public override int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetFurthestEntranceFrom(Target.Object.GetCentreTile(), character);
	}
}
