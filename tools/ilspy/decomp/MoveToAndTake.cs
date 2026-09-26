using System;
using System.Text;

public class MoveToAndTake : StateMachineGoal
{
	private MovementType _movementType = MovementType.Walk;

	public Equipment _targetEquipment;

	public Equipment PourIntoContainer;

	private float _desiredAmount;

	private bool _ignoreWeight;

	public bool IsGathering;

	public bool DontOpenOurGates;

	private bool IsControlledByPlayer;

	public bool Pickpocketing;

	private PlayerID ControllingPlayerID;

	public float AmountRetrieved;

	public bool TooHeavy;

	public bool SuccessfullyReachedDestination;

	public int EntranceIndex = -1;

	public Equipment GetTargetEquipment()
	{
		return _targetEquipment;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndTake;
	}

	public override bool IsCollectingSomethingFrom(TileObject obj)
	{
		GetTargetObject();
		return GetTargetObject() == obj;
	}

	public MoveToAndTake()
	{
	}

	public MoveToAndTake(PlayerID controllingPlayerID)
	{
		IsControlledByPlayer = true;
		ControllingPlayerID = controllingPlayerID;
	}

	public MoveToAndTake(Character character, TileObject targetObject, MovementType movementType)
	{
		_movementType = movementType;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public MoveToAndTake(Character character, TileObject targetObject, Equipment targetEquipment, int desiredAmount, bool ignoreWeight)
	{
		_targetEquipment = targetEquipment;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public MoveToAndTake(Character character, TileObject targetObject, Equipment targetEquipment, int desiredAmount, bool ignoreWeight, MovementType movementType)
	{
		_targetEquipment = targetEquipment;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		_movementType = movementType;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public MoveToAndTake(Character character, TileObject targetObject, int entranceIndex, Equipment targetEquipment, int desiredAmount, bool ignoreWeight, MovementType movementType)
	{
		_targetEquipment = targetEquipment;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		_movementType = movementType;
		EntranceIndex = entranceIndex;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public MoveToAndTake(Character character, TileObject targetObject, Equipment targetEquipment, float desiredAmount, bool ignoreWeight, MovementType movementType, Equipment pourIntoContainer)
	{
		_targetEquipment = targetEquipment;
		PourIntoContainer = pourIntoContainer;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		_movementType = movementType;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public MoveToAndTake(Character character, TileObject targetObject, int entranceIndex, Equipment targetEquipment, float desiredAmount, bool ignoreWeight, MovementType movementType, Equipment pourIntoContainer)
	{
		_targetEquipment = targetEquipment;
		PourIntoContainer = pourIntoContainer;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		_movementType = movementType;
		EntranceIndex = entranceIndex;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _movementType);
		reflector.Add(ref _targetEquipment);
		reflector.AddAfter(ref PourIntoContainer, 72);
		if (reflector.Version < 72)
		{
			int value = 0;
			int value2 = 0;
			reflector.Add(ref value);
			reflector.Add(ref value2);
			_desiredAmount = value;
			AmountRetrieved = value2;
		}
		else
		{
			reflector.Add(ref _desiredAmount);
			reflector.Add(ref AmountRetrieved);
		}
		reflector.Add(ref AmountRetrieved);
		reflector.Add(ref SuccessfullyReachedDestination);
		reflector.AddAfter(ref TooHeavy, 41);
		reflector.AddAfter(ref IsGathering, 358);
		reflector.Add(ref _ignoreWeight);
		reflector.AddAfter(ref DontOpenOurGates, 331);
		reflector.AddAfter(ref IsControlledByPlayer, 4);
		reflector.AddAfter(ref ControllingPlayerID, 4);
		reflector.AddAfter(ref Pickpocketing, 624);
		reflector.AddAfter(ref EntranceIndex, 90);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (_targetEquipment != null && Target.Object.CanTransferEquipmentAway(_targetEquipment) != CantTransferReason.CanTransfer)
		{
			Finished = true;
		}
		else if (GetTargetBuilding() != null && GetTargetBuilding().GetPropPrototype().IsEnterableBySpecies(character.GetBaseObjectType()) && !IsControlledByPlayer)
		{
			MoveToAndEnterBuilding moveToAndEnterBuilding = new MoveToAndEnterBuilding();
			moveToAndEnterBuilding.EntranceIndex = EntranceIndex;
			moveToAndEnterBuilding.SetMovementType(character, _movementType);
			moveToAndEnterBuilding._dontOpenOurGates = DontOpenOurGates;
			SetSubGoal(character, parent, moveToAndEnterBuilding);
		}
		else if (Pickpocketing)
		{
			SetSubGoal(character, parent, new MoveWithinRangeOfTarget(_movementType, aiming: false, 0f, 1.25f, DontOpenOurGates));
		}
		else
		{
			SetSubGoal(character, parent, new MoveAdjacentToTarget(_movementType, DontOpenOurGates, Target.Object is Character));
		}
	}

	public override void BuildDebugExtraInfoString(Character character, Goal parent, StringBuilder str)
	{
		base.BuildDebugExtraInfoString(character, parent, str);
		if (_targetEquipment != null)
		{
			str.Append(' ');
			str.Append('(');
			_targetEquipment.BuildDisplayName(str, noStrangers: true, englishOnly: true);
			str.Append(')');
		}
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (PourIntoContainer != null && !character.Inventory.Contains(PourIntoContainer))
		{
			return false;
		}
		if (_targetEquipment != null)
		{
			EquipmentContainer inventory = Target.Object.GetInventory();
			if ((inventory == null || !inventory.Contains(_targetEquipment)) && !(SubGoal is LeaveBuilding) && !(SubGoal is TakeAnim))
			{
				return false;
			}
			if (Target.Object.CanTransferEquipmentAway(_targetEquipment) != CantTransferReason.CanTransfer && !(SubGoal is LeaveBuilding) && !(SubGoal is TakeAnim))
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveTo { Success: not false })
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget)
		{
			if (IsControlledByPlayer)
			{
				return new TakeUIGoal(ControllingPlayerID, Pickpocketing);
			}
			return new TakeAnim(_targetEquipment, _desiredAmount, _ignoreWeight, PourIntoContainer, IsGathering);
		}
		if (SubGoal is TakeAnim takeAnim)
		{
			AmountRetrieved += takeAnim.AmountRetrieved;
			SuccessfullyReachedDestination = true;
			return null;
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			EntranceIndex = moveToAndEnterBuilding.EntranceIndex;
			if (moveToAndEnterBuilding.Success)
			{
				return new Wait(TimeSpan.FromSeconds(1.0), disableSleep: true);
			}
			Building targetBuilding = GetTargetBuilding();
			if (targetBuilding.CouldEnterIfNotFull(character) && targetBuilding.IsFull() && character.Tile.IsWithinBounds(targetBuilding.GetMinTile() - new TerrainCoord(1, 1), targetBuilding.GetMaxTile() + new TerrainCoord(1, 1)))
			{
				return new MoveAdjacentToTarget(_movementType, DontOpenOurGates, canBeOnTile: false);
			}
		}
		if (SubGoal is Wait)
		{
			if (parent is FindGoal findGoal && (findGoal.FindType == FindType.BuildingResources || findGoal.FindType == FindType.CraftingResources || findGoal.FindType == FindType.Clothing))
			{
				AmountRetrieved = findGoal.TakeNeededResourcesFromBuilding(character, GetTargetObject(), out TooHeavy, _ignoreWeight, PourIntoContainer);
			}
			else
			{
				TakeAnim.Take(character, GetTargetObject(), _targetEquipment, _desiredAmount, _ignoreWeight, PourIntoContainer, IsGathering, out AmountRetrieved);
				if (parent is OrganizerGoal organizerGoal)
				{
					organizerGoal.OnFirstItemTaken(character, GetTargetProp());
				}
			}
			return new LeaveBuilding(EntranceIndex);
		}
		if (SubGoal is LeaveBuilding)
		{
			SuccessfullyReachedDestination = true;
			return null;
		}
		return null;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take && PourIntoContainer == null)
		{
			character.RemoveFailedFindAttempt(GetTargetObject());
			if (parent is FindGoal findGoal && (findGoal.FindType == FindType.BuildingResources || findGoal.FindType == FindType.CraftingResources || findGoal.FindType == FindType.Clothing))
			{
				AmountRetrieved = findGoal.TakeNeededResourcesFromBuilding(character, GetTargetObject(), out TooHeavy, _ignoreWeight, PourIntoContainer);
				return true;
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is LeaveBuilding)
		{
			character.GoalTarget = null;
		}
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

	public override bool IsSubstantiallyFinished(Character character)
	{
		if (SubGoal is TakeUIGoal takeUIGoal && takeUIGoal.IsSubstantiallyFinished(character))
		{
			return true;
		}
		return base.IsSubstantiallyFinished(character);
	}
}
