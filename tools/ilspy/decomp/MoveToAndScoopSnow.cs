public class MoveToAndScoopSnow : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public TerrainCoord Tile;

	public MoveToAndScoopSnow()
	{
	}

	public MoveToAndScoopSnow(Character character, TerrainCoord tile, MovementType movementType)
	{
		MovementType = movementType;
		Tile = tile;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndScoopSnow;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.Add(ref Tile);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (character.Tile == Tile)
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.PickUp, enableFaceTarget: false));
		}
		else
		{
			SetSubGoal(character, parent, new MoveAdjacentTo(MovementType, Tile, canBeOnTile: false, DontOpenOurGates));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentTo { Success: not false })
		{
			return new TurnTo(Tile);
		}
		if (SubGoal is TurnTo)
		{
			return new AnimationGoal(ActionAnim.PickUp, enableFaceTarget: false);
		}
		if (SubGoal is AnimationGoal)
		{
			Success = true;
		}
		return null;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.PickUp)
		{
			Equipment equipment = Equipment.Spawn(EquipmentPrototype.Snowball);
			if (equipment != null)
			{
				character.DesiredEquippedItem = (character.EquippedItem = character.Inventory.Add(character, equipment));
				GameTerrain.Instance.AddRecentSnowScoop(Tile);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
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
