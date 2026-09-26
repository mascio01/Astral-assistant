public class MoveAdjacentToTarget : MoveToTarget
{
	public bool CanBeOnTile = true;

	public bool CanBeDiagonallyAdjacent;

	public MoveAdjacentToTarget()
	{
	}

	public MoveAdjacentToTarget(MovementType movementType, bool canBeOnTile)
		: base(movementType)
	{
		CanBeOnTile = canBeOnTile;
	}

	public MoveAdjacentToTarget(MovementType movementType, bool dontOpenOurGates, bool canBeOnTile)
		: base(movementType)
	{
		_dontOpenOurGates = dontOpenOurGates;
		MoveToCentreOfTile = true;
		CanBeOnTile = canBeOnTile;
	}

	public MoveAdjacentToTarget(Character character, TileObject targetObj, MovementType movementType, bool canBeOnTile)
		: base(movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MoveToCentreOfTile = true;
		CanBeOnTile = canBeOnTile;
	}

	public MoveAdjacentToTarget(Character character, TileObject targetObj, MovementType movementType, bool dontOpenOurGates, bool canBeOnTile)
		: base(movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		_dontOpenOurGates = dontOpenOurGates;
		MoveToCentreOfTile = true;
		CanBeOnTile = canBeOnTile;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveAdjacentToTarget;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref CanBeOnTile, 102);
		reflector.AddAfter(ref CanBeDiagonallyAdjacent, 618);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (IsAdjacentToTarget(character, Target.Object, CanBeDiagonallyAdjacent))
		{
			Success = true;
			Finished = true;
		}
	}

	public override void StartAStarRequest(Character character, Goal parent)
	{
		TileObject targetObject = GetTargetObject();
		if (targetObject is Prop prop && prop.GetMaxTile() != prop.GetMinTile())
		{
			_requester.StartWithinBoundsRequest(GetStartTile(character), prop.GetMinTile() - new TerrainCoord(1, 1), prop.GetMaxTile() + new TerrainCoord(1, 1), character, null, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
		}
		else if (CanBeDiagonallyAdjacent && targetObject != null)
		{
			_requester.StartWithinBoundsRequest(GetStartTile(character), targetObject.GetTile() - new TerrainCoord(1, 1), targetObject.GetTile() + new TerrainCoord(1, 1), character, null, AStarPriority, _dontOpenOurGates, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
		}
		else
		{
			_requester.StartAdjacentToRequest(GetStartTile(character), DestTile, character, null, AStarPriority, _dontOpenOurGates, CanBeOnTile, RespectMovementZone, AvoidHostileBases, DontAvoidCommunityId);
		}
	}

	public static bool IsAdjacentToTarget(Character character, TileObject targetObj)
	{
		return IsAdjacentToTarget(character, targetObj, canBeDiagonallyAdjacent: false);
	}

	public static bool IsAdjacentToTarget(Character character, TileObject targetObj, bool canBeDiagonallyAdjacent)
	{
		TerrainCoord tile = character.Tile;
		if (targetObj is Prop prop && prop.GetMaxTile() != prop.GetMinTile())
		{
			if (tile.IsWithinBounds(prop.GetMinTile() - new TerrainCoord(1, 1), prop.GetMaxTile() + new TerrainCoord(1, 1)))
			{
				return true;
			}
		}
		else if (canBeDiagonallyAdjacent)
		{
			if (tile.IsWithinBounds(targetObj.GetTile() - new TerrainCoord(1, 1), targetObj.GetTile() + new TerrainCoord(1, 1)))
			{
				return true;
			}
		}
		else if (tile.IsAdjacent(targetObj.GetNearestTileTo(tile)))
		{
			return true;
		}
		return false;
	}

	public override MoveToResult HasReachedDestination(Character character, Goal parent)
	{
		if (IsAdjacentToTarget(character, Target.Object, CanBeDiagonallyAdjacent))
		{
			return MoveToResult.Success;
		}
		if (_requestedDestTile != DestTile)
		{
			return MoveToResult.Working;
		}
		return MoveToResult.Fail;
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
		TerrainCoord routeDestination = character.GetRouteDestination();
		return GameTerrain.Instance.IsImpassable(routeDestination.x, routeDestination.y, 2049, character, null);
	}
}
