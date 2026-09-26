internal class FillWateringCanAndWaterPlant : StateMachineGoal
{
	private TerrainCoord _tile;

	public bool Success;

	private MovementType MovementType = MovementType.Walk;

	public override GoalType GetGoalType()
	{
		return GoalType.FillWateringCanAndWaterPlant;
	}

	public FillWateringCanAndWaterPlant()
	{
	}

	public FillWateringCanAndWaterPlant(TerrainCoord tile, MovementType movementType)
	{
		_tile = tile;
		MovementType = movementType;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _tile);
		reflector.Add(ref Success);
		reflector.AddAfter(ref MovementType, 183);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		PlantableCrop plant = GameTerrain.Instance.GetPlant(_tile.x, _tile.y);
		if (plant != null)
		{
			_ = character.PosXZ;
			Equipment bestWateringCan = character.Inventory.GetBestWateringCan();
			if (bestWateringCan.GetLiquidContentsType() != LiquidPrototype.Water || (bestWateringCan.GetLiquidContentsAmount() < 1f && bestWateringCan.GetLiquidContentsAmount() + plant.GetMoisture() < 1f))
			{
				SetSubGoal(character, parent, new FindGoal(FindType.WaterForCrops, MovementType, critical: false));
			}
			else
			{
				SetSubGoal(character, parent, new WaterPlant(character, plant, bestWateringCan, MovementType));
			}
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FillLiquidContainer { Success: not false } fillLiquidContainer)
		{
			PlantableCrop plant = GameTerrain.Instance.GetPlant(_tile.x, _tile.y);
			if (plant != null && plant.GetMoisture() < 1f)
			{
				return new WaterPlant(character, plant, fillLiquidContainer.GetLiquidContainer(), MovementType);
			}
		}
		if (SubGoal is FindGoal { Success: not false })
		{
			Equipment bestWateringCan = character.Inventory.GetBestWateringCan();
			if (bestWateringCan != null)
			{
				PlantableCrop plant2 = GameTerrain.Instance.GetPlant(_tile.x, _tile.y);
				if (plant2 != null && plant2.GetMoisture() < 1f)
				{
					return new WaterPlant(character, plant2, bestWateringCan, MovementType);
				}
			}
		}
		if (SubGoal is WaterPlant { Success: not false })
		{
			Success = true;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override float IsWateringOrHarvestingPlant(PlantableCrop plant)
	{
		return _tile.GetDist(plant.Tile);
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		base.SetMovementType(character, movementType);
	}

	public override MovementType GetMovementType()
	{
		return MovementType;
	}
}
