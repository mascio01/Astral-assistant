public class HarvestCrops : StateMachineGoal
{
	private TerrainCoord _tile;

	private MovementType _movementType = MovementType.Walk;

	private bool _ignoreWeight;

	public bool IsGathering;

	public bool DontOpenOurGates;

	public bool Success;

	public TerrainCoord Tile => _tile;

	public override GoalType GetGoalType()
	{
		return GoalType.HarvestCrops;
	}

	public HarvestCrops()
	{
	}

	public HarvestCrops(TerrainCoord tile, bool ignoreWeight)
	{
		_tile = tile;
		_ignoreWeight = ignoreWeight;
	}

	public HarvestCrops(TerrainCoord tile, bool ignoreWeight, bool dontOpenOurGates)
	{
		_tile = tile;
		_ignoreWeight = ignoreWeight;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _tile);
		reflector.Add(ref _movementType);
		reflector.AddAfter(ref _ignoreWeight, 4);
		reflector.AddAfter(ref IsGathering, 353);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.AddAfter(ref Success, 74);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new MoveAdjacentTo(_movementType, _tile, canBeOnTile: false, DontOpenOurGates));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (GameTerrain.Instance.GetPlant(_tile.x, _tile.y) == null && !(SubGoal is HarvestCropsAnim { Success: not false }))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentTo { Success: not false })
		{
			return new TurnTo(_tile);
		}
		if (SubGoal is TurnTo)
		{
			return new HarvestCropsAnim(_tile, _ignoreWeight, IsGathering);
		}
		if (SubGoal is HarvestCropsAnim { Success: not false })
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

	public override float IsWateringOrHarvestingPlant(PlantableCrop plant)
	{
		return _tile.GetDist(plant.Tile);
	}
}
