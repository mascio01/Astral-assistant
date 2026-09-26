public class MoveToAndAddMaterialToFire : StateMachineGoal
{
	public bool Success;

	public bool Critical;

	public MovementType MovementType = MovementType.Walk;

	public TerrainCoord Tile;

	public Equipment Item;

	public Equipment PreviouslyEquipped;

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndAddMaterialToFire;
	}

	public MoveToAndAddMaterialToFire()
	{
	}

	public MoveToAndAddMaterialToFire(Character character, TerrainCoord tile, Equipment item, MovementType movementType, bool critical)
	{
		MovementType = movementType;
		Tile = tile;
		Item = item;
		Critical = critical;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref Tile);
		reflector.Add(ref Item);
		reflector.Add(ref Critical);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!(GameTerrain.Instance.GetCraftingPropOnTile(Tile.x, Tile.y) is Campfire campfire))
		{
			return false;
		}
		if (campfire.WoodRemaining >= 1f)
		{
			if (SubGoal is AddMaterialToFireAnim || (Success && SubGoal is Equip))
			{
				return true;
			}
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, GetNextSubGoal(character, parent));
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
		if (!(GameTerrain.Instance.GetCraftingPropOnTile(Tile.x, Tile.y) is Campfire targetObj))
		{
			return null;
		}
		if (SubGoal is Equip && Success)
		{
			return null;
		}
		if (SubGoal is AddMaterialToFireAnim)
		{
			Success = true;
			if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
			{
				return new Equip(PreviouslyEquipped);
			}
			return null;
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (!findGoal.Success)
			{
				return null;
			}
			Item = findGoal.FoundItem;
		}
		if (SubGoal is MoveAdjacentTo moveAdjacentTo)
		{
			if (moveAdjacentTo.Success)
			{
				return new AddMaterialToFireAnim(character, targetObj, Item);
			}
			return null;
		}
		if (Item != null && !character.InventoryContains(Item))
		{
			Item = null;
		}
		if (Item == null)
		{
			Item = character.Inventory.GetBestMaterialToAddToFire(character, character);
			if (Item == null)
			{
				return new FindGoal(FindType.MaterialForFire, MovementType, Critical);
			}
		}
		if (Item != character.EquippedItem)
		{
			PreviouslyEquipped = character.EquippedItem;
			return new Equip(Item);
		}
		if (character.Sitting && character.Tile.GetDistSquared(Tile) <= TileObject.MaxFireHeatRange * TileObject.MaxFireHeatRange)
		{
			return new AddMaterialToFireAnim(character, targetObj, Item);
		}
		return new MoveAdjacentTo(MovementType, Tile, canBeOnTile: false);
	}
}
