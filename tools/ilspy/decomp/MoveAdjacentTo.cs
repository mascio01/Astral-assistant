public class MoveAdjacentTo : MoveTo
{
	private bool CanBeOnTile = true;

	public MoveAdjacentTo()
	{
	}

	public MoveAdjacentTo(MovementType movementType, TerrainCoord destTile, bool canBeOnTile)
		: base(movementType, destTile)
	{
		MoveToCentreOfTile = true;
		CanBeOnTile = canBeOnTile;
	}

	public MoveAdjacentTo(MovementType movementType, TerrainCoord destTile, bool canBeOnTile, bool dontOpenOurGates)
		: base(movementType, destTile)
	{
		MoveToCentreOfTile = true;
		CanBeOnTile = canBeOnTile;
		_dontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveAdjacentTo;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref CanBeOnTile, 102);
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		_requester.StartAdjacentToRequest(GetStartTile(character), DestTile, character, null, AStarPriority, _dontOpenOurGates, CanBeOnTile, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (!character.Tile.IsAdjacent(DestTile))
		{
			return MoveToResult.Fail;
		}
		return MoveToResult.Success;
	}

	public override void UpdateDestination(Character character, Goal parent)
	{
	}

	public override bool ShouldReRequestPath(Character character, Goal parent)
	{
		if (character.Route.Count == 0)
		{
			return false;
		}
		if (_requester != null)
		{
			return false;
		}
		TerrainCoord terrainCoord = character.Route[0];
		return GameTerrain.Instance.IsImpassable(terrainCoord.x, terrainCoord.y, 2049, character, null);
	}
}
