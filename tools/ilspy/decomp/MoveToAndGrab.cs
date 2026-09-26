public class MoveToAndGrab : StateMachineGoal
{
	public bool Success;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool IsGathering;

	public MoveToAndGrab()
	{
	}

	public MoveToAndGrab(Character character, TileObject targetObj, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public MoveToAndGrab(Character character, TileObject targetObj, MovementType movementType, bool dontOpenOurGates)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndGrab;
	}

	public override bool IsCollectingSomethingFrom(TileObject obj)
	{
		GetTargetObject();
		return GetTargetObject() == obj;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref IsGathering, 527);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (GetTargetObject() == null)
		{
			Finished = true;
			Success = true;
		}
		else
		{
			MoveTo goal = new MoveWithinRangeOfTarget(MovementType, aiming: false, 0f, 1.25f, DontOpenOurGates);
			SetSubGoal(character, parent, goal);
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false } && GetTargetObject() != null)
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			TileObject targetObject = GetTargetObject();
			if (targetObject != null && !IsTargetDeleted())
			{
				return new AnimationGoal((targetObject.Pos.y >= character.Pos.y + 1f) ? ActionAnim.Scavenge : ActionAnim.ScavengeCorpse);
			}
		}
		return null;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take)
		{
			TileObject targetObject = GetTargetObject();
			if (targetObject != null && targetObject.GetGrabbableEquipmentType() != null)
			{
				EquipmentContainer inventory = targetObject.GetInventory();
				Equipment equipment = Equipment.Spawn(targetObject.GetGrabbableEquipmentType());
				equipment.InfectedWith = targetObject.GetGrabbableEquipmentInfectedWith();
				if (equipment != null && character.HasInventorySpaceFor(equipment.GetWeight() + (inventory?.GetWeightIncludingWornItems() ?? 0f)))
				{
					if (targetObject.GetCommunity() != null)
					{
						character.OnStoleSomething(targetObject.GetCommunity(), null, equipment.GetBasePrice() * (float)equipment.GetAmount());
					}
					StoryManager.Instance.RestoreEquipmentInQuestInstances(equipment);
					int amount = equipment.GetAmount();
					NotificationManager.Instance.AddEquipmentNotification(null, character, equipment, amount);
					equipment = character.Inventory.Add(character, equipment);
					if (IsGathering)
					{
						equipment.IncrementGatheredAmount(amount);
					}
					if (inventory != null)
					{
						while (inventory.Count > 0)
						{
							Equipment item = inventory.GetItem(0);
							int amount2 = item.GetAmount();
							NotificationManager.Instance.AddEquipmentNotification(null, character, item, amount2);
							item = inventory.Take(targetObject, item, item.GetAmount());
							item = character.Inventory.Add(character, item);
							if (IsGathering)
							{
								item.IncrementGatheredAmount(amount2);
							}
						}
					}
					targetObject.Delete();
					Success = true;
				}
				else
				{
					equipment.Delete();
				}
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
