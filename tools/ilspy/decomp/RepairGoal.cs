using System;
using UnityEngine;

public class RepairGoal : RoleGoal
{
	public TileObject CurrentBuildingToRepair;

	private TerrainCoord CurrentBuildingTile;

	private TimeSpan LastAttemptedTime = TimeSpan.FromDays(-365.0);

	private bool HasSaidNoResources;

	private bool HasSaidTooHeavy;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.RepairGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorRepair;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref CurrentBuildingToRepair);
		reflector.AddAfter(ref CurrentBuildingTile, 507);
		reflector.Add(ref MovementType);
		reflector.Add(ref LastAttemptedTime);
		reflector.AddAfter(ref HasSaidNoResources, 472);
		reflector.AddAfter(ref HasSaidTooHeavy, 472);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.Community == null)
		{
			return false;
		}
		if (!character.HasRunningRole(Role.Repairing))
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Repairing))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active)
		{
			if (SubGoal is AnimationGoal)
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			int roleIndex = character.GetRoleIndex(new RoleInfo(Role.Repairing));
			if (roleIndex != -1)
			{
				return (GoalPriority)(210 - roleIndex);
			}
		}
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role == Role.Repairing && !character.Roles[i].Paused && !character.Roles[i].FailedRecently)
			{
				return (GoalPriority)(210 - i);
			}
		}
		return GoalPriority.Impossible;
	}

	public void ResetLastAttemptedTime()
	{
		LastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	public void SetCurrentBuildingToRepair(Character character, Goal parent, TileObject building)
	{
		if (CurrentBuildingToRepair != building)
		{
			if (Active)
			{
				OnDeactivate(character, parent);
				CurrentBuildingToRepair = building;
				OnActivate(character, parent);
			}
			else
			{
				CurrentBuildingToRepair = building;
			}
			HasSaidNoResources = false;
			HasSaidTooHeavy = false;
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.Repairing), inProgress: true);
		Goal firstSubGoal = GetFirstSubGoal(character, parent);
		if (firstSubGoal != null)
		{
			SetSubGoal(character, parent, firstSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	private TileObject GetBestBuildingToRepair(Character character, bool fast)
	{
		if (CurrentBuildingToRepair != null && CurrentBuildingToRepair.GetDamageFraction() > 0f && !CurrentBuildingToRepair.IsDestroyed() && CurrentBuildingToRepair.GetCommunityId() == character.Community.Id && character.GetSkillLevelWithEffects(SkillType.Construction) >= CurrentBuildingToRepair.GetRepairSkillNeeded() && (!character.HasMovementZone() || character.MovementZone.Overlaps(CurrentBuildingToRepair.GetTileRect())))
		{
			return CurrentBuildingToRepair;
		}
		float num = float.MaxValue;
		TileObject result = null;
		foreach (TileObject item in character.Community.NeedsRepair)
		{
			if ((character.HasMovementZone() && !character.MovementZone.Overlaps(item.GetTileRect())) || character.Community.IsAnyMemberRepairingBuilding(item) || item.IsDestroyed() || character.GetSkillLevelWithEffects(SkillType.Construction) < item.GetRepairSkillNeeded() || item is EnterableVehicle { GearState: not GearState.Off })
			{
				continue;
			}
			float dist = item.GetTile().GetDist(character.Tile);
			if (dist < num)
			{
				if (fast)
				{
					return item;
				}
				num = dist;
				result = item;
			}
		}
		return result;
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Repairing));
		LastAttemptedTime = Session.Instance.PlayTime;
	}

	public Goal GetFirstSubGoal(Character character, Goal parent)
	{
		TileObject currentBuildingToRepair = CurrentBuildingToRepair;
		CurrentBuildingToRepair = GetBestBuildingToRepair(character, fast: false);
		if (CurrentBuildingToRepair != currentBuildingToRepair)
		{
			HasSaidNoResources = false;
			HasSaidTooHeavy = false;
		}
		if (CurrentBuildingToRepair != null)
		{
			Type type = (CurrentBuildingToRepair.GetPropPrototype().NeedToolboxToRepair() ? typeof(Toolbox) : null);
			if (type != null && character.Inventory.FindItemOfClass(type) == null)
			{
				character.DirectControlledCrouching = false;
				SetCrouching(character, parent, character.DirectControlledCrouching);
				return new FindGoal(FindType.Toolbox, MovementType, critical: false);
			}
			EquipmentPrototype repairResourceType = CurrentBuildingToRepair.GetRepairResourceType();
			if (character.Inventory.FindItemOfType(repairResourceType) != null || character.GetCaptureGoal().GetLeftOverIngredientAmount(repairResourceType) > 0f)
			{
				TerrainCoord destTile = FindPositionToRepair(character);
				if (destTile.GetDistSquared(character.Tile) > MathUtil.Squared(8f))
				{
					character.DirectControlledCrouching = false;
				}
				SetCrouching(character, parent, character.DirectControlledCrouching);
				CurrentBuildingTile = CurrentBuildingToRepair.GetCentreTile();
				return new MoveAsCloseAsPossibleTo(MovementType, destTile, CaptureGoal.MaxDistFromBuilding, FindGoal.CalcDontOpenOurGates(character));
			}
			character.DirectControlledCrouching = false;
			SetCrouching(character, parent, character.DirectControlledCrouching);
			return new FindGoal(FindType.Prototype, repairResourceType, MovementType, critical: false)
			{
				CanTravelFar = character.IsControllableByPlayer()
			};
		}
		OnFailed(character);
		return null;
	}

	private TerrainCoord FindPositionToRepair(Character character)
	{
		Vector2 posXZ = CurrentBuildingToRepair.PosXZ;
		if (CurrentBuildingToRepair is Prop prop)
		{
			int num = prop.FindDamagePointNearestPos(character.EyePosition);
			if (num != -1)
			{
				posXZ = MathUtil.ToXZ(prop.DamagePoints[num].Pos);
			}
		}
		return CaptureGoal.FindPositionToHammer(character, CurrentBuildingToRepair, posXZ);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!(SubGoal is MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo) || CurrentBuildingToRepair == null || !(CurrentBuildingTile != CurrentBuildingToRepair.GetCentreTile()))
		{
			return;
		}
		TerrainCoord terrainCoord = FindPositionToRepair(character);
		if (terrainCoord != moveAsCloseAsPossibleTo.OriginalDestTile && terrainCoord.GetDist(moveAsCloseAsPossibleTo.OriginalDestTile) > terrainCoord.GetDist(character.Tile) - 1f)
		{
			if (terrainCoord.GetDistSquared(character.Tile) > MathUtil.Squared(8f))
			{
				character.DirectControlledCrouching = false;
			}
			SetCrouching(character, parent, character.DirectControlledCrouching);
			CurrentBuildingTile = CurrentBuildingToRepair.GetCentreTile();
			SetSubGoal(character, parent, new MoveAsCloseAsPossibleTo(MovementType, terrainCoord, CaptureGoal.MaxDistFromBuilding, FindGoal.CalcDontOpenOurGates(character)));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo)
		{
			if (moveAsCloseAsPossibleTo.Success && CurrentBuildingToRepair != null)
			{
				return new TurnTo(CurrentBuildingToRepair.GetNearestTileTo(character.Tile));
			}
			OnFailed(character);
			return null;
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.FindType == FindType.Prototype)
			{
				if (findGoal.Success)
				{
					return GetFirstSubGoal(character, parent);
				}
				if (findGoal.Result == FindResult.TooHeavy && character.IsControllableByPlayer() && CurrentBuildingToRepair != null)
				{
					if (!HasSaidTooHeavy)
					{
						MemoryParam param = new MemoryParam(findGoal.ProtoToFind);
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
						character.Speak(speechForSituation, null, null, param);
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
						HasSaidTooHeavy = true;
					}
				}
				else if (findGoal.Result != FindResult.NotAccessible && character.IsControllableByPlayer() && CurrentBuildingToRepair != null && !HasSaidNoResources)
				{
					MemoryParam param2 = new MemoryParam(findGoal.ProtoToFind);
					Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, CurrentBuildingToRepair, SpeechSituation.InsufficientRepairingResources, param2);
					character.Speak(speechForSituation2, null, CurrentBuildingToRepair, param2);
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					HasSaidNoResources = true;
				}
				OnFailed(character);
				return null;
			}
			if (findGoal.Success)
			{
				return GetFirstSubGoal(character, parent);
			}
			if (character.IsControllableByPlayer() && CurrentBuildingToRepair != null)
			{
				if (findGoal.FindType == FindType.Toolbox || findGoal.FindType == FindType.Shovel)
				{
					Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, (findGoal.FindType == FindType.Toolbox) ? SpeechSituation.NeedToolbox : SpeechSituation.NeedShovel);
					character.Speak(speechForSituation3);
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				}
				OnFailed(character);
				return null;
			}
		}
		if (SubGoal is TurnTo && CurrentBuildingToRepair != null)
		{
			Equipment equipment = (CurrentBuildingToRepair.GetPropPrototype().NeedToolboxToRepair() ? character.Inventory.GetToolbox() : character.Inventory.FindItemOfType(CurrentBuildingToRepair.GetRepairResourceType()));
			if (character.EquippedItem != equipment)
			{
				if (equipment != null)
				{
					return new Equip(equipment);
				}
				return GetFirstSubGoal(character, parent);
			}
			return new AnimationGoal((CurrentBuildingToRepair.GetPropPrototype().RepairAnim == RepairAnimType.Hammering) ? ActionAnim.RepairStart : ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (SubGoal is Equip && CurrentBuildingToRepair != null)
		{
			return new AnimationGoal((CurrentBuildingToRepair.GetPropPrototype().RepairAnim == RepairAnimType.Hammering) ? ActionAnim.RepairStart : ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		AnimationGoal animationGoal = SubGoal as AnimationGoal;
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.RepairStart || animationGoal.GetAnim() == ActionAnim.CraftStart) && CurrentBuildingToRepair != null)
		{
			return new RepairAnim(CurrentBuildingToRepair, (animationGoal.GetAnim() == ActionAnim.RepairStart) ? ActionAnim.Repair : ActionAnim.CraftNonLooped, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.Repair || animationGoal.GetAnim() == ActionAnim.CraftNonLooped))
		{
			return new AnimationGoal((animationGoal.GetAnim() == ActionAnim.Repair) ? ActionAnim.RepairFinish : ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.RepairFinish || animationGoal.GetAnim() == ActionAnim.CraftEnd) && CurrentBuildingToRepair != null)
		{
			EquipmentPrototype repairResourceType = CurrentBuildingToRepair.GetRepairResourceType();
			float num = Math.Min(1f, CurrentBuildingToRepair.GetRepairResourceNeeded() * CurrentBuildingToRepair.GetDamageFraction());
			if (character.GetCaptureGoal().GetLeftOverIngredientAmount(repairResourceType) < num)
			{
				Equipment equipment2 = character.Inventory.FindItemOfType(repairResourceType);
				if (equipment2 != null)
				{
					character.Inventory.UseItemOfType(character, character, equipment2.GetPrototype(), 1, null, out var _);
					character.GetCaptureGoal().AddLeftOverIngredient(repairResourceType, 1f);
					character.Skillset.AddProgress(character, SkillType.Construction, 10f);
				}
			}
			float num2 = Math.Min(num, character.GetCaptureGoal().GetLeftOverIngredientAmount(repairResourceType));
			character.GetCaptureGoal().AddLeftOverIngredient(repairResourceType, 0f - num2);
			CurrentBuildingToRepair.RepairDamage(num2 / CurrentBuildingToRepair.GetRepairResourceNeeded(), character.EyePosition);
			if (CurrentBuildingToRepair.GetDamageFraction() <= 0f)
			{
				character.OnRoleSucceeded();
			}
			Goal firstSubGoal = GetFirstSubGoal(character, parent);
			if (firstSubGoal == null)
			{
				Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.RepairsComplete);
				character.Speak(speechForSituation4);
				character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				character.SetRoleInProgress(new RoleInfo(Role.Repairing), inProgress: false);
			}
			return firstSubGoal;
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Repairing);
	}
}
