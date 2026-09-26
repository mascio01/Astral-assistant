using System;
using UnityEngine;

public class GuardGoal : RoleGoal
{
	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastAttemptedTime);
		reflector.AddAfter(ref MovementType, 333);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GuardGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorGuard;
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Guard));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Guard))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Guard))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - _lastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (!character.GuardDuty)
		{
			return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Guard));
		}
		return GoalPriority.Survivor_GuardDuty;
	}

	private bool IsValidBuilding(Character character, Prop prop)
	{
		if (prop.IsGuardPost())
		{
			if (character.InsideBuilding == prop)
			{
				return true;
			}
			if (character.HasMovementZone() && !character.MovementZone.Overlaps(prop.GetTileRect()))
			{
				return false;
			}
			return (prop as Building).CanEnter(character);
		}
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DirectControlledCrouching = false;
		TerrainCoord roleTargetLocation = character.GetRoleTargetLocation(Role.Guard);
		if (roleTargetLocation != TerrainCoord.Invalid)
		{
			Building building = GameTerrain.Instance.GetBuilding(roleTargetLocation.x, roleTargetLocation.y);
			if (building != null && IsValidBuilding(character, building))
			{
				SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, MovementType, FindGoal.CalcDontOpenOurGates(character)));
				character.SetRoleInProgress(new RoleInfo(Role.Guard), inProgress: true);
				return;
			}
		}
		Building building2 = null;
		float num = 10000f;
		foreach (Prop building3 in character.Community.Buildings)
		{
			if (IsValidBuilding(character, building3))
			{
				float sqrMagnitude = MathUtil.ToXZ(building3.Pos - character.Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					building2 = building3 as Building;
					num = sqrMagnitude;
				}
			}
		}
		if (building2 != null)
		{
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building2, MovementType, FindGoal.CalcDontOpenOurGates(character)));
			character.SetRoleInProgress(new RoleInfo(Role.Guard), inProgress: true);
			return;
		}
		int num2 = 5;
		if (roleTargetLocation != TerrainCoord.Invalid)
		{
			if (character.Tile.IsWithinBounds(roleTargetLocation - new TerrainCoord(num2, num2), roleTargetLocation + new TerrainCoord(num2, num2)))
			{
				SetSubGoal(character, parent, new Wait(TimeSpan.FromSeconds(10.0)));
			}
			else
			{
				SetSubGoal(character, parent, new MoveWithinBounds(MovementType.Walk, roleTargetLocation - new TerrainCoord(num2, num2), roleTargetLocation + new TerrainCoord(num2, num2)));
			}
			character.SetRoleInProgress(new RoleInfo(Role.Guard), inProgress: true);
		}
		else
		{
			OnFailed(character);
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndEnterBuilding { Success: not false } && character.InsideBuilding != null)
		{
			character.SetRoleTargetLocation(Role.Guard, character.InsideBuilding.Tile);
			ResetLastAttemptedTime();
			if (character.EquippedItem == null)
			{
				Equipment bestWeaponForIdle = character.Inventory.GetBestWeaponForIdle(character, rangedOnly: true, allowMolotovs: false);
				if (bestWeaponForIdle != null)
				{
					return new Equip(bestWeaponForIdle);
				}
			}
			return new TurnToAngle(character.InsideBuilding.GetInhabitantSlotAngleRad(character));
		}
		if (SubGoal is Equip && character.InsideBuilding != null)
		{
			return new TurnToAngle(character.InsideBuilding.GetInhabitantSlotAngleRad(character));
		}
		if (SubGoal is TurnToAngle)
		{
			return new Wait(TimeSpan.FromSeconds(10.0));
		}
		if (SubGoal is MoveWithinBounds { Success: not false })
		{
			return new Wait(TimeSpan.FromSeconds(10.0));
		}
		if (SubGoal is Wait)
		{
			character.OnRoleSucceeded();
			if (character.IsGuarding())
			{
				return new Wait(TimeSpan.FromSeconds(10.0));
			}
			return null;
		}
		OnFailed(character);
		return base.GetNextSubGoal(character, parent);
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Guard);
	}
}
