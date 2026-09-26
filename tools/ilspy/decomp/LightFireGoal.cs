public class LightFireGoal : StateMachineGoal
{
	public bool Success;

	public bool Critical;

	public bool CanAddMaterialToFire;

	public bool IsPlayerCommand;

	public MovementType MovementType = MovementType.Walk;

	public Equipment Lighter;

	public Equipment PreviouslyEquipped;

	public LightFireGoal()
	{
	}

	public LightFireGoal(Equipment lighter, MovementType movementType, bool canAddMaterialToFire)
	{
		MovementType = movementType;
		Lighter = lighter;
		CanAddMaterialToFire = canAddMaterialToFire;
	}

	public LightFireGoal(Character character, Equipment lighter, TileObject obj, MovementType movementType, bool canAddMaterialToFire)
	{
		SetTarget(character, null, character.GetOrCreateTarget(obj));
		HasUserTarget = true;
		MovementType = movementType;
		Lighter = lighter;
		CanAddMaterialToFire = canAddMaterialToFire;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LightFireGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		TileObject targetObject = GetTargetObject();
		if (targetObject == null)
		{
			return false;
		}
		if (targetObject.IsBurning() && !(SubGoal is LightFireWithFlint) && !(SubGoal is LightFireWithMatch) && (!(SubGoal is Equip) || !Success))
		{
			return false;
		}
		if (!Success && Lighter != null)
		{
			if (!character.Inventory.Contains(Lighter))
			{
				return false;
			}
			if (Lighter is HuntingKnife && character.Inventory.FindItemOfType(EquipmentPrototype.Flint) == null)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		Campfire campfire = GetTargetObject() as Campfire;
		if (campfire != null && campfire.IsBurning())
		{
			Success = true;
			Finished = true;
		}
		else if (CanAddMaterialToFire && campfire != null && campfire.WoodRemaining <= 0f)
		{
			SetSubGoal(character, parent, new MoveToAndAddMaterialToFire(character, campfire.Tile, null, MovementType, Critical));
		}
		else if (Lighter == null)
		{
			SetSubGoal(character, parent, new FindGoal(FindType.Lighter, MovementType, Critical));
		}
		else if (character.EquippedItem != Lighter)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(Lighter));
		}
		else
		{
			Goal moveToTargetGoal = GetMoveToTargetGoal();
			SetSubGoal(character, parent, moveToTargetGoal);
		}
	}

	private Goal GetMoveToTargetGoal()
	{
		Prop targetProp = GetTargetProp();
		if (targetProp != null && !(targetProp is Campfire))
		{
			return new MoveWithinBounds(MovementType, targetProp.GetMinTile() - new TerrainCoord(1, 1), targetProp.GetMaxTile() + new TerrainCoord(1, 1));
		}
		return new MoveAdjacentToTarget(MovementType, canBeOnTile: false);
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

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref Critical);
		reflector.Add(ref MovementType);
		reflector.Add(ref Lighter);
		reflector.AddAfter(ref CanAddMaterialToFire, 10);
		reflector.AddAfter(ref PreviouslyEquipped, 328);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndAddMaterialToFire moveToAndAddMaterialToFire)
		{
			if (moveToAndAddMaterialToFire.Success)
			{
				if (Lighter == null)
				{
					return new FindGoal(FindType.Lighter, MovementType, Critical);
				}
				PreviouslyEquipped = character.EquippedItem;
				return new Equip(Lighter);
			}
			if (parent is ObeyLeaderGoal)
			{
				character.SpeakForSituation(null, null, SpeechSituation.NeedFuelForFire, default(MemoryParam));
			}
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.Success)
			{
				Lighter = findGoal.FoundItem;
				PreviouslyEquipped = character.EquippedItem;
				return new Equip(Lighter);
			}
			if (parent is ObeyLeaderGoal)
			{
				character.SpeakForSituation(null, null, SpeechSituation.NeedLighter, default(MemoryParam));
			}
		}
		if (SubGoal is Equip && character.EquippedItem == Lighter && !Success)
		{
			return GetMoveToTargetGoal();
		}
		if (SubGoal is MoveTo { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			if (Lighter != null && Lighter.GetPrototype() == EquipmentPrototype.Match)
			{
				return new LightFireWithMatch();
			}
			if (Lighter is HuntingKnife && character.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null)
			{
				return new LightFireWithFlint();
			}
		}
		if (SubGoal is AnimationGoal && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			return new Equip(PreviouslyEquipped);
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

	public void LightFire(Character character)
	{
		GetTargetObject()?.LightFire(character);
		Success = true;
	}
}
