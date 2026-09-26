public class PlantCrops : StateMachineGoal
{
	private TerrainCoord _tile;

	private MovementType _movementType = MovementType.Walk;

	public bool MustBeWithinPlantingZone;

	private Equipment Seeds;

	public bool Success;

	public TerrainCoord Tile => _tile;

	public override GoalType GetGoalType()
	{
		return GoalType.PlantCrops;
	}

	public PlantCrops()
	{
	}

	public PlantCrops(TerrainCoord tile, Equipment seeds, MovementType movementType)
	{
		_tile = tile;
		Seeds = seeds;
		_movementType = movementType;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _tile);
		reflector.Add(ref Seeds);
		reflector.Add(ref _movementType);
		reflector.AddAfter(ref MustBeWithinPlantingZone, 133);
		reflector.Add(ref Success);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.EquippedItem != Seeds)
		{
			SetSubGoal(character, parent, new Equip(Seeds));
			return;
		}
		if (!MustBeWithinPlantingZone && character.Tile.IsWithinBounds(_tile - new TerrainCoord(1, 1), _tile + new TerrainCoord(1, 1)) && GameTerrain.Instance.CharacterMapWho.AreAnyObjectsInRect(_tile, _tile, character))
		{
			SetSubGoal(character, parent, new PlantCropsAnim(_tile, Seeds));
			return;
		}
		MoveTo moveTo = new MoveTo(_movementType, _tile);
		if (MustBeWithinPlantingZone && Seeds != null)
		{
			moveTo.MustBeWithinPlantingZone = Seeds.GetSeedForPlantType();
		}
		SetSubGoal(character, parent, moveTo);
	}

	public override void Update(Character character, Goal parent)
	{
		if (!character.PlantNewCrops && parent is FarmingGoal)
		{
			Success = true;
			Finished = true;
		}
		base.Update(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip)
		{
			if (character.EquippedItem != Seeds)
			{
				return null;
			}
			return new MoveTo(_movementType, _tile);
		}
		if (SubGoal is MoveTo { Success: not false } && Seeds != null && (!MustBeWithinPlantingZone || Session.Instance.CropsManager.IsInPatchOfType(character.Tile, character.GetCommunityId(), Seeds.GetSeedForPlantType())))
		{
			return new PlantCropsAnim(character.Tile, Seeds);
		}
		if (SubGoal is PlantCropsAnim { Success: not false })
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
