using System;
using UnityEngine;

public class TrapperGoal : RoleGoal
{
	private TimeSpan PitTrapLastAttemptedTime = TimeSpan.FromDays(-365.0);

	private TimeSpan RabbitTrapLastAttemptedTime = TimeSpan.FromDays(-365.0);

	private TimeSpan LastAttemptedTime = TimeSpan.FromDays(-365.0);

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.TrapperGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.TrapperIcon;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && SubGoal is MoveToAndResetTrap moveToAndResetTrap && moveToAndResetTrap.SubGoal is AnimationGoal)
		{
			return GoalPriority.Survivor_Role_Animation;
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Trapper));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Trapper))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Trapper))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - PitTrapLastAttemptedTime < MinTimeBetweenAttempts && Session.Instance.PlayTime - RabbitTrapLastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref PitTrapLastAttemptedTime);
		reflector.Add(ref RabbitTrapLastAttemptedTime);
		reflector.Add(ref LastAttemptedTime);
		reflector.Add(ref MovementType);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role == Role.Trapper && !character.Roles[i].Paused && EquipmentPrototype.DeadRabbit != null)
			{
				character.Inventory.FindItemOfType(EquipmentPrototype.DeadRabbit)?.SetGathered();
				break;
			}
		}
		character.SetRoleInProgress(new RoleInfo(Role.Trapper), inProgress: true);
		Goal nextSubGoal = GetNextSubGoal(character, parent);
		if (nextSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, nextSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndResetTrap moveToAndResetTrap)
		{
			if (moveToAndResetTrap.Success)
			{
				character.OnRoleSucceeded();
			}
			else
			{
				OnFailed(character);
				if (moveToAndResetTrap.GetTargetObject() is PitTrap)
				{
					PitTrapLastAttemptedTime = Session.Instance.PlayTime;
				}
				if (moveToAndResetTrap.GetTargetObject() is RabbitTrap)
				{
					RabbitTrapLastAttemptedTime = Session.Instance.PlayTime;
				}
			}
		}
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: false })
		{
			OnFailed(character);
		}
		TileObject tileObject = null;
		float num = float.MaxValue;
		if (Session.Instance.PlayTime - PitTrapLastAttemptedTime >= MinTimeBetweenAttempts)
		{
			foreach (PitTrap pitTrap in character.Community.PitTraps)
			{
				if (pitTrap.CanResetTrap())
				{
					float magnitude = (pitTrap.PosXZ - character.PosXZ).magnitude;
					if (magnitude < num && (!character.HasMovementZone() || character.MovementZone.Contains(pitTrap.Tile)) && !character.Community.IsAnyMemberResettingTrap(pitTrap))
					{
						tileObject = pitTrap;
						num = magnitude;
					}
				}
			}
		}
		foreach (Prop building in character.Community.Buildings)
		{
			if (building is ITrap trap && (!(building is RabbitTrap) || !(Session.Instance.PlayTime - RabbitTrapLastAttemptedTime < MinTimeBetweenAttempts)) && trap.CanResetTrap())
			{
				float magnitude2 = (building.PosXZ - character.PosXZ).magnitude;
				if (magnitude2 < num && (!character.HasMovementZone() || character.MovementZone.Overlaps(building.GetTileRect())) && !character.Community.IsAnyMemberResettingTrap(trap))
				{
					tileObject = building;
					num = magnitude2;
				}
			}
		}
		if (tileObject != null)
		{
			if (tileObject is RabbitTrap && !character.HasInventorySpaceFor(EquipmentPrototype.DeadRabbit.Weight))
			{
				bool failed;
				Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, EquipmentPrototype.DeadRabbit.Weight, EquipmentPrototype.DeadRabbit.Weight, MovementType, out failed);
				if (moveToAndDepositGoal != null)
				{
					return moveToAndDepositGoal;
				}
				if (failed)
				{
					OnFailed(character);
					return null;
				}
			}
			return new MoveToAndResetTrap(character, tileObject, MovementType, FindGoal.CalcDontOpenOurGates(character))
			{
				IsGathering = true
			};
		}
		bool failed2;
		Goal moveToAndDepositGoal2 = GatherGoal.GetMoveToAndDepositGoal(character, 0f, 0f, MovementType, out failed2);
		if (moveToAndDepositGoal2 != null)
		{
			return moveToAndDepositGoal2;
		}
		if (failed2)
		{
			OnFailed(character);
			return null;
		}
		OnFailed(character);
		return base.GetNextSubGoal(character, parent);
	}

	public void ResetLastAttemptedTime()
	{
		LastAttemptedTime = (PitTrapLastAttemptedTime = (RabbitTrapLastAttemptedTime = TimeSpan.FromDays(-365.0)));
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Trapper));
		LastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Trapper);
	}
}
