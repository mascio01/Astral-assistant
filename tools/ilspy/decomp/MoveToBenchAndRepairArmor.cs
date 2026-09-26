public class MoveToBenchAndRepairArmor : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public Equipment PreviouslyEquipped;

	public MoveToBenchAndRepairArmor()
	{
	}

	public MoveToBenchAndRepairArmor(Character character, TileObject workBench, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(workBench));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToBenchAndRepairArmor;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.AddAfter(ref PreviouslyEquipped, 524);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (GetTargetProp() is WorkBench workBench)
		{
			TerrainCoord tileToStandOn = workBench.GetTileToStandOn(character);
			SetSubGoal(character, parent, new MoveTo(MovementType, tileToStandOn));
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
	}

	public bool IsAnimating()
	{
		return !(SubGoal is MoveTo);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false })
		{
			if (character.EquippedItem != null)
			{
				PreviouslyEquipped = character.EquippedItem;
				return new Equip(null);
			}
			return new AnimationGoal(ActionAnim.CraftTableStart);
		}
		if (SubGoal is Equip { Equipment: null })
		{
			return new AnimationGoal(ActionAnim.CraftTableStart);
		}
		if (SubGoal is AnimationGoal animationGoal)
		{
			switch (animationGoal.GetAnim())
			{
			case ActionAnim.CraftTableStart:
				return new RepairArmorAnim();
			case ActionAnim.CraftTableLoop:
				foreach (Equipment content in character.Inventory.Contents)
				{
					if (content is Armor armor)
					{
						armor.Repair(character);
					}
				}
				Success = true;
				return new AnimationGoal(ActionAnim.CraftTableEnd);
			case ActionAnim.CraftTableEnd:
				if (PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
				{
					return new Equip(PreviouslyEquipped);
				}
				break;
			}
		}
		return base.GetNextSubGoal(character, parent);
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
