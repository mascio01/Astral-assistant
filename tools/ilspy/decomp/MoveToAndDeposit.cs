using System;
using System.Collections.Generic;

public class MoveToAndDeposit : StateMachineGoal
{
	public class SortEquipmentByIndividualWeightDescending : IComparer<Equipment>
	{
		int IComparer<Equipment>.Compare(Equipment a, Equipment b)
		{
			if (a.GetWeight() > b.GetWeight())
			{
				return -1;
			}
			if (a.GetWeight() < b.GetWeight())
			{
				return 1;
			}
			if (a.Id > b.Id)
			{
				return -1;
			}
			if (a.Id < b.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	public MovementType MovementType = MovementType.Walk;

	public bool SuccessfullyReachedDestination;

	public int EntranceIndex = -1;

	public float WeightToFree;

	public Equipment StuffToDeposit;

	public int AmountToDeposit;

	public int AmountDeposited;

	public bool WasGathered;

	private static SortEquipmentByIndividualWeightDescending EquipmentSorterByIndividualWeightDescending = new SortEquipmentByIndividualWeightDescending();

	private static List<Equipment> Temp = new List<Equipment>();

	public MoveToAndDeposit()
	{
	}

	public MoveToAndDeposit(Character character, TileObject targetObject, Equipment stuffToDeposit, int amountToDeposit, MovementType movementType)
	{
		MovementType = movementType;
		StuffToDeposit = stuffToDeposit;
		AmountToDeposit = amountToDeposit;
		SetTarget(character, null, character.GetOrCreateTarget(targetObject));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndDeposit;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref MovementType);
		reflector.Add(ref SuccessfullyReachedDestination);
		reflector.AddAfter(ref EntranceIndex, 90);
		reflector.AddAfter(ref WeightToFree, 116);
		reflector.AddAfter(ref StuffToDeposit, 324);
		reflector.AddAfter(ref AmountToDeposit, 353);
		reflector.AddAfter(ref AmountDeposited, 353);
		reflector.AddAfter(ref WasGathered, 353);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (WasGathered && StuffToDeposit != null && character.IsWearing(StuffToDeposit))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (WasGathered && StuffToDeposit != null && character.IsWearing(StuffToDeposit))
		{
			StuffToDeposit.ClearGathered();
			SuccessfullyReachedDestination = true;
			Finished = true;
		}
		else
		{
			SetSubGoal(character, parent, new MoveAdjacentToTarget(MovementType, canBeOnTile: false));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAdjacentToTarget moveAdjacentToTarget)
		{
			if (moveAdjacentToTarget.Success)
			{
				return new TurnToTarget();
			}
			character.AddFailedFindAttempt(moveAdjacentToTarget.GetTargetObject(), 0, 0, FindType.Deposit, null, null, null);
		}
		if (SubGoal is TurnToTarget)
		{
			return new DepositAnim();
		}
		if (SubGoal is DepositAnim)
		{
			SuccessfullyReachedDestination = true;
			return null;
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			EntranceIndex = moveToAndEnterBuilding.EntranceIndex;
			if (moveToAndEnterBuilding.Success)
			{
				if (parent is LumberjackGoal && GetTargetProp() != null)
				{
					character.IssueCommandToFollowers(FollowCommand.StoreChoppedWood, GetTargetProp().GetCentreTile(), null);
				}
				else if (parent is MinerGoal && GetTargetProp() != null)
				{
					MinerGoal minerGoal = (MinerGoal)parent;
					if (minerGoal.CurrentMiningResourceType != null)
					{
						character.IssueCommandToFollowers(FollowCommand.StoreMinedRock, GetTargetProp().GetCentreTile(), minerGoal.CurrentMiningResourceType);
					}
				}
				else if (parent is TrapperGoal && GetTargetProp() != null)
				{
					character.IssueCommandToFollowers(FollowCommand.StoreTrappedAnimals, GetTargetProp().GetCentreTile(), EquipmentPrototype.DeadRabbit);
				}
				else
				{
					character.IssueCommandToFollowers(FollowCommand.StoreItems, GetTargetProp().GetCentreTile(), null);
				}
				return new Wait(TimeSpan.FromSeconds(1.0));
			}
			Building targetBuilding = GetTargetBuilding();
			if (targetBuilding != null && targetBuilding.CouldEnterIfNotFull(character) && targetBuilding.IsFull() && character.Tile.IsWithinBounds(targetBuilding.GetMinTile() - new TerrainCoord(1, 1), targetBuilding.GetMaxTile() + new TerrainCoord(1, 1)))
			{
				return new MoveAdjacentToTarget(MovementType, canBeOnTile: false);
			}
			character.AddFailedFindAttempt(targetBuilding, EntranceIndex, 0, FindType.Deposit, null, null, null);
		}
		if (SubGoal is Wait)
		{
			Deposit(character, parent);
			return new LeaveBuilding(EntranceIndex);
		}
		if (SubGoal is LeaveBuilding)
		{
			SuccessfullyReachedDestination = true;
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take)
		{
			Deposit(character, parent);
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public void Deposit(Character character, Goal parent)
	{
		Prop targetProp = GetTargetProp();
		if (targetProp == null || StuffToDeposit == null || !character.InventoryContains(StuffToDeposit))
		{
			return;
		}
		character.IssueCommandToFollowers(FollowCommand.StoreItems, targetProp.GetCentreTile(), null);
		int num = Math.Min(AmountToDeposit, (int)(targetProp.GetAvailableInventorySpace() / StuffToDeposit.GetWeight()));
		if (num <= 0)
		{
			return;
		}
		Equipment equipment = character.Inventory.Take(character, StuffToDeposit, num);
		if (equipment != null)
		{
			NotificationManager.Instance.AddEquipmentNotification(character, targetProp, equipment, equipment.GetAmount());
			AmountDeposited = equipment.GetAmount();
			if (WasGathered)
			{
				character.OnRoleSucceeded();
			}
			targetProp.Inventory.Add(targetProp, equipment, character);
		}
	}
}
