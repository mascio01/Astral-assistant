public class WaterPlant : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	private Equipment LiquidContainer;

	public bool PourOnto;

	public bool Success;

	public override GoalType GetGoalType()
	{
		return GoalType.WaterPlant;
	}

	public WaterPlant()
	{
	}

	public WaterPlant(Equipment liquidContainer)
	{
		LiquidContainer = liquidContainer;
	}

	public WaterPlant(Character character, TileObject obj, Equipment liquidContainer, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(obj));
		HasUserTarget = true;
		LiquidContainer = liquidContainer;
		_movementType = movementType;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref LiquidContainer);
		reflector.AddAfter(ref PourOnto, 468);
		reflector.Add(ref Success);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (character.EquippedItem != LiquidContainer)
		{
			SetSubGoal(character, parent, new Equip(LiquidContainer));
		}
		else
		{
			SetSubGoal(character, parent, GetMoveToGoal());
		}
	}

	public MoveTo GetMoveToGoal()
	{
		Prop targetProp = GetTargetProp();
		if (targetProp != null)
		{
			return new MoveWithinBounds(_movementType, targetProp.GetMinTile() - new TerrainCoord(1, 1), targetProp.GetMaxTile() + new TerrainCoord(1, 1));
		}
		return new MoveAdjacentToTarget(_movementType, canBeOnTile: false);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip)
		{
			if (character.EquippedItem != LiquidContainer)
			{
				return null;
			}
			return GetMoveToGoal();
		}
		if (SubGoal is MoveTo { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			if (character.EquippedItem != LiquidContainer)
			{
				return null;
			}
			return new WaterPlantAnim
			{
				PourOnto = PourOnto
			};
		}
		if (SubGoal is WaterPlantAnim { Success: not false })
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
		TileObject targetObject = GetTargetObject();
		if (targetObject is PlantableCrop && LiquidContainer != null && LiquidContainer.GetLiquidContentsType() == LiquidPrototype.Water)
		{
			return targetObject.GetCentreTile().GetDist(plant.Tile);
		}
		return float.MaxValue;
	}
}
