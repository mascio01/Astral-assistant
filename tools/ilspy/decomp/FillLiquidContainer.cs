public class FillLiquidContainer : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	private Equipment LiquidContainer;

	public TerrainCoord Tile = TerrainCoord.Invalid;

	public int TerrainPathIndex = -1;

	public bool Success;

	public bool DontOpenOurGates;

	public Equipment PreviouslyEquipped;

	public override GoalType GetGoalType()
	{
		return GoalType.FillLiquidContainer;
	}

	public Equipment GetLiquidContainer()
	{
		return LiquidContainer;
	}

	public FillLiquidContainer()
	{
	}

	public FillLiquidContainer(Character character, TerrainCoord tile, int terrainPathIndex, Equipment liquidContainer, MovementType movementType)
	{
		LiquidContainer = liquidContainer;
		Tile = tile;
		TerrainPathIndex = terrainPathIndex;
		_movementType = movementType;
	}

	public FillLiquidContainer(Character character, TerrainCoord tile, int terrainPathIndex, Equipment liquidContainer, MovementType movementType, bool dontOpenOurGates)
	{
		LiquidContainer = liquidContainer;
		Tile = tile;
		TerrainPathIndex = terrainPathIndex;
		_movementType = movementType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref LiquidContainer);
		reflector.Add(ref Tile);
		reflector.AddAfter(ref TerrainPathIndex, 212);
		reflector.Add(ref Success);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.EquippedItem != LiquidContainer)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(LiquidContainer));
			return;
		}
		TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(Tile.x, Tile.y);
		if (fixedObjectOnTile is Well || fixedObjectOnTile is Outhouse)
		{
			SetSubGoal(character, parent, new MoveAdjacentTo(_movementType, Tile, canBeOnTile: false, DontOpenOurGates));
		}
		else
		{
			SetSubGoal(character, parent, new MoveTo(_movementType, Tile, DontOpenOurGates));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip)
		{
			if (character.EquippedItem != LiquidContainer || Success)
			{
				return null;
			}
			TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(Tile.x, Tile.y);
			if (fixedObjectOnTile is Well || fixedObjectOnTile is Outhouse)
			{
				return new MoveAdjacentTo(_movementType, Tile, canBeOnTile: false, DontOpenOurGates);
			}
			return new MoveTo(_movementType, Tile, DontOpenOurGates);
		}
		MoveTo moveTo = SubGoal as MoveTo;
		MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = SubGoal as MoveAsCloseAsPossibleTo;
		if (SubGoal is MoveAdjacentTo moveAdjacentTo)
		{
			if (moveAdjacentTo.Success)
			{
				return new TurnTo(Tile);
			}
			return new MoveAsCloseAsPossibleTo(_movementType, Tile, 3.1f);
		}
		if (moveAsCloseAsPossibleTo != null)
		{
			if (moveAsCloseAsPossibleTo.Success)
			{
				return new TurnTo(Tile);
			}
		}
		else if (moveTo != null && moveTo.Success)
		{
			return new FillLiquidContainerFromRiverAnim(Tile);
		}
		if (SubGoal is TurnTo)
		{
			TileObject fixedObjectOnTile2 = GameTerrain.Instance.GetFixedObjectOnTile(Tile.x, Tile.y);
			if (fixedObjectOnTile2 is Well || fixedObjectOnTile2 is Outhouse)
			{
				return new FillLiquidContainerFromWellAnim(character, fixedObjectOnTile2);
			}
		}
		if (SubGoal is FillLiquidContainerFromWellAnim fillLiquidContainerFromWellAnim)
		{
			Success = fillLiquidContainerFromWellAnim.Success;
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
			{
				return new Equip(PreviouslyEquipped);
			}
		}
		if (SubGoal is FillLiquidContainerFromRiverAnim fillLiquidContainerFromRiverAnim)
		{
			Success = fillLiquidContainerFromRiverAnim.Success;
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
			{
				return new Equip(PreviouslyEquipped);
			}
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
