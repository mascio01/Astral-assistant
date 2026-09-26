public class MoveToAndPickUp : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool CanUnlockGatesFromInsideWithKey;

	public MoveToAndPickUp()
	{
	}

	public MoveToAndPickUp(MovementType movementType)
	{
		MovementType = movementType;
	}

	public MoveToAndPickUp(Character character, TileObject targetObj, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndPickUp;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref CanUnlockGatesFromInsideWithKey, 549);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		TileObject targetObject = GetTargetObject();
		if (targetObject == null || character.CarryingObject == targetObject)
		{
			Finished = true;
			Success = true;
			return;
		}
		if (!GameCursor.CanPickUp(character, targetObject))
		{
			Finished = true;
			Success = false;
			return;
		}
		MoveAdjacentToTarget moveAdjacentToTarget = new MoveAdjacentToTarget(MovementType, DontOpenOurGates, canBeOnTile: false);
		moveAdjacentToTarget.CanUnlockGatesFromInsideWithKey = CanUnlockGatesFromInsideWithKey;
		moveAdjacentToTarget.CanBeDiagonallyAdjacent = true;
		SetSubGoal(character, parent, moveAdjacentToTarget);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false } && GetTargetObject() != null)
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget && GetTargetObject() != null && !IsTargetDeleted())
		{
			return new PickUpAnim();
		}
		if (SubGoal is PickUpAnim)
		{
			Success = true;
		}
		return null;
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
