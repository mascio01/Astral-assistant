using System.Collections.Generic;

public class MoveToAndResetTrap : StateMachineGoal
{
	public bool Success;

	public bool CouldNotFindResources;

	public MovementType MovementType = MovementType.Walk;

	public bool DontOpenOurGates;

	public bool IsGathering;

	private static List<TileObject> NearbyTraps = new List<TileObject>();

	public MoveToAndResetTrap()
	{
	}

	public MoveToAndResetTrap(Character character, TileObject targetObj, MovementType movementType)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
	}

	public MoveToAndResetTrap(Character character, TileObject targetObj, MovementType movementType, bool dontOpenOurGates)
	{
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
		MovementType = movementType;
		DontOpenOurGates = dontOpenOurGates;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MoveToAndResetTrap;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
		reflector.Add(ref CouldNotFindResources);
		reflector.Add(ref MovementType);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref IsGathering, 353);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (!(GetTargetObject() is ITrap trap))
		{
			Finished = true;
			Success = false;
			return;
		}
		EquipmentPrototype equipmentNeededForReset = trap.GetEquipmentNeededForReset();
		if (equipmentNeededForReset != null && character.Inventory.FindItemOfType(equipmentNeededForReset) == null)
		{
			FindGoal findGoal = new FindGoal(FindType.Prototype, equipmentNeededForReset, MovementType, critical: false);
			findGoal.CanTravelFar = character.IsControllableByPlayer();
			SetSubGoal(character, parent, findGoal);
		}
		else
		{
			TellFollowersToResetNearbyTraps(character);
			MoveTo goal = new MoveWithinRangeOfTarget(MovementType, aiming: false, 0f, 1.25f, DontOpenOurGates);
			SetSubGoal(character, parent, goal);
		}
	}

	private void TellFollowersToResetNearbyTraps(Character character)
	{
		TileObject targetObject = GetTargetObject();
		if (targetObject == null || character.Followers == null || character.Followers.Count <= 0)
		{
			return;
		}
		int num = 16;
		GameTerrain.Instance.GetObjectsInRect(targetObject.GetCentreTile() - new TerrainCoord(num, num), targetObject.GetCentreTile() + new TerrainCoord(num, num), NearbyTraps);
		float num2 = float.MaxValue;
		TileObject tileObject = null;
		foreach (TileObject nearbyTrap in NearbyTraps)
		{
			if (!(nearbyTrap is ITrap trap))
			{
				continue;
			}
			float magnitude = (nearbyTrap.PosXZ - character.PosXZ).magnitude;
			if (magnitude < num2)
			{
				EquipmentPrototype equipmentNeededForReset = trap.GetEquipmentNeededForReset();
				if ((equipmentNeededForReset == null || character.Inventory.FindItemOfType(equipmentNeededForReset) != null) && !character.Community.IsAnyMemberResettingTrap(trap))
				{
					tileObject = nearbyTrap;
					num2 = magnitude;
				}
			}
		}
		if (tileObject != null)
		{
			character.IssueCommandToFollowers(FollowCommand.ResetTrap, tileObject.GetCentreTile(), null);
		}
		NearbyTraps.Clear();
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FindGoal findGoal)
		{
			if (!findGoal.Success)
			{
				CouldNotFindResources = true;
				return null;
			}
			if (!IsTargetDeleted())
			{
				TellFollowersToResetNearbyTraps(character);
				return new MoveWithinRangeOfTarget(MovementType, aiming: false, 0f, 1.25f, DontOpenOurGates);
			}
		}
		if (SubGoal is MoveTo { Success: not false } && GetTargetObject() != null)
		{
			return new TurnToTarget();
		}
		if (SubGoal is TurnToTarget && GetTargetObject() != null && !IsTargetDeleted())
		{
			return new AnimationGoal(ActionAnim.ScavengeCorpse);
		}
		return null;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take && SubGoal is AnimationGoal)
		{
			if (GetTargetObject() is ITrap trap)
			{
				EquipmentPrototype equipmentNeededForReset = trap.GetEquipmentNeededForReset();
				if (equipmentNeededForReset != null && character.Inventory.UseItemOfType(character, character, equipmentNeededForReset, 1, null, out var _) == 0)
				{
					return false;
				}
				trap.ResetTrap(character, IsGathering);
				Success = true;
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
