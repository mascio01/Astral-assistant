public class MoveWithinBounds : MoveTo
{
	private TerrainCoord BoundsMin;

	private TerrainCoord BoundsMax;

	public MoveWithinBounds()
	{
	}

	public MoveWithinBounds(MovementType movementType, TerrainCoord boundsMin, TerrainCoord boundsMax)
		: base(movementType, (boundsMin + boundsMax) / 2)
	{
		BoundsMin = boundsMin;
		BoundsMax = boundsMax;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref BoundsMin);
		reflector.Add(ref BoundsMax);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveWithinBounds;
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartWithinBoundsRequest(GetStartTile(character), BoundsMin, BoundsMax, character, null, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (!character.Tile.IsWithinBounds(BoundsMin, BoundsMax))
		{
			return MoveToResult.Fail;
		}
		return MoveToResult.Success;
	}

	public override int GetBuildingExitIndex(Character character)
	{
		return character.InsideBuilding.GetClosestEntranceTo((BoundsMin + BoundsMax) / 2, character, mustBeUnblocked: true);
	}
}
