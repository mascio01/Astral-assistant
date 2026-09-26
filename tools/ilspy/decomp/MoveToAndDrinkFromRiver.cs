public class MoveToAndDrinkFromRiver : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	public TerrainCoord Tile = TerrainCoord.Invalid;

	public int TerrainPathIndex = -1;

	public bool DontOpenOurGates;

	public bool Success;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndDrinkFromRiver;
	}

	public MoveToAndDrinkFromRiver()
	{
	}

	public MoveToAndDrinkFromRiver(Character character, TerrainCoord tile, int terrainPathIndex, MovementType movementType)
	{
		Tile = tile;
		TerrainPathIndex = terrainPathIndex;
		_movementType = movementType;
	}

	public MoveToAndDrinkFromRiver(Character character, TerrainCoord tile, int terrainPathIndex, MovementType movementType, bool dontOpenOurGates)
	{
		Tile = tile;
		TerrainPathIndex = terrainPathIndex;
		_movementType = movementType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref Tile);
		reflector.AddAfter(ref TerrainPathIndex, 212);
		reflector.Add(ref Success);
		reflector.AddAfter(ref DontOpenOurGates, 331);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new MoveAdjacentTo(_movementType, Tile, canBeOnTile: false, DontOpenOurGates));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentTo { Success: not false })
		{
			return new TurnTo(Tile);
		}
		if (SubGoal is TurnTo)
		{
			return new DrinkFromRiver();
		}
		if (SubGoal is DrinkFromRiver)
		{
			Success = true;
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
