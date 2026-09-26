public class LeaveBuildingAndMoveTo : StateMachineGoal
{
	private TerrainCoord _destTile;

	private MovementType _movementType = MovementType.Walk;

	public TerrainCoord DestTile => _destTile;

	public override GoalType GetGoalType()
	{
		return GoalType.LeaveBuildingAndMoveTo;
	}

	public LeaveBuildingAndMoveTo()
	{
	}

	public LeaveBuildingAndMoveTo(TerrainCoord dest)
	{
		_destTile = dest;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref _destTile);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.InsideBuilding != null)
		{
			int closestEntranceTo = character.InsideBuilding.GetClosestEntranceTo(_destTile);
			SetSubGoal(character, parent, new LeaveBuilding(closestEntranceTo));
		}
		else
		{
			SetSubGoal(character, parent, new MoveTo(_movementType, _destTile));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is LeaveBuilding)
		{
			return new MoveTo(_movementType, _destTile);
		}
		return null;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		_movementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return _movementType;
	}
}
