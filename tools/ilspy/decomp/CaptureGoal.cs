using System;
using System.Collections.Generic;
using UnityEngine;

public class CaptureGoal : RoleGoal
{
	private TileObject Building;

	private TerrainCoord Tile = TerrainCoord.Invalid;

	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private List<LeftOverIngredient> LeftOverIngredients = new List<LeftOverIngredient>();

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public static float MaxDistFromBuilding = 8f;

	public override GoalType GetGoalType()
	{
		return GoalType.CaptureGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorFlag;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Building);
		reflector.AddAfter(ref Tile, 374);
		if (reflector.Version < 388)
		{
			bool value = false;
			reflector.Add(ref value);
		}
		reflector.Add(ref MovementType);
		reflector.Add(ref _lastAttemptedTime);
		reflector.Add(ref LeftOverIngredients);
	}

	public void Restart()
	{
		if (Active)
		{
			Finished = true;
		}
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	private void OnFailed(Character character)
	{
		if (Tile != TerrainCoord.Invalid)
		{
			character.SetRoleFailedRecently(new RoleInfo(Role.Capturing, Tile));
		}
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.Community == null)
		{
			return false;
		}
		if (!character.HasRunningRole(Role.Capturing))
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Capturing))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - _lastAttemptedTime < MinTimeBetweenAttempts)
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
			if (character.IsChoppingWood())
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (character.IsMining())
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (Tile != TerrainCoord.Invalid)
			{
				int roleIndex = character.GetRoleIndex(new RoleInfo(Role.Capturing, Tile));
				if (roleIndex != -1)
				{
					return (GoalPriority)(210 - roleIndex);
				}
			}
			return GoalPriority.Impossible;
		}
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role == Role.Capturing && !character.Roles[i].Paused && !character.Roles[i].FailedRecently)
			{
				return (GoalPriority)(210 - i);
			}
		}
		return GoalPriority.Impossible;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
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

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		Building = null;
		Tile = TerrainCoord.Invalid;
	}

	public float GetLeftOverIngredientAmount(EquipmentPrototype proto)
	{
		for (int i = 0; i < LeftOverIngredients.Count; i++)
		{
			if (LeftOverIngredients[i].Prototype == proto)
			{
				return LeftOverIngredients[i].Amount;
			}
		}
		return 0f;
	}

	public void AddLeftOverIngredient(EquipmentPrototype proto, float amount)
	{
		for (int i = 0; i < LeftOverIngredients.Count; i++)
		{
			if (LeftOverIngredients[i].Prototype == proto)
			{
				LeftOverIngredients[i] = LeftOverIngredient.CreateItem(proto, LeftOverIngredients[i].Amount + amount);
				if (LeftOverIngredients[i].Amount < 0f)
				{
					LeftOverIngredients.RemoveAt(i);
				}
				return;
			}
		}
		if (amount > 0f)
		{
			LeftOverIngredients.Add(LeftOverIngredient.CreateItem(proto, amount));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		EquipmentPrototype neededResourceType = GetNeededResourceType();
		if (SubGoal is MoveAsCloseAsPossibleTo { Success: not false } && Building != null)
		{
			return new TurnTo(Building.GetNearestTileTo(character.Tile));
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.FindType == FindType.Prototype)
			{
				if (findGoal.Success)
				{
					return GetFirstSubGoal(character, parent);
				}
				if (findGoal.Result == FindResult.TooHeavy && character.IsControllableByPlayer() && Building != null)
				{
					MemoryParam param = new MemoryParam(findGoal.ProtoToFind);
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
					character.Speak(speechForSituation, null, null, param);
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					character.PauseRole(new RoleInfo(Role.Capturing, Tile));
					return null;
				}
				if (findGoal.Result != FindResult.NotAccessible && character.IsControllableByPlayer() && Building != null)
				{
					MemoryParam param2 = new MemoryParam(findGoal.ProtoToFind);
					Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, Building, SpeechSituation.InsufficientCapturingResources, param2);
					character.Speak(speechForSituation2, null, Building, param2);
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					character.PauseRole(new RoleInfo(Role.Capturing, Tile));
					return null;
				}
			}
			else
			{
				if (findGoal.Success)
				{
					return GetFirstSubGoal(character, parent);
				}
				if (character.IsControllableByPlayer() && Building != null)
				{
					if (findGoal.FindType == FindType.Toolbox || findGoal.FindType == FindType.Shovel)
					{
						Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, (findGoal.FindType == FindType.Toolbox) ? SpeechSituation.NeedToolbox : SpeechSituation.NeedShovel);
						character.Speak(speechForSituation3);
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					}
					character.PauseRole(new RoleInfo(Role.Capturing, Tile));
					return null;
				}
			}
		}
		if (SubGoal is TurnTo && Building != null)
		{
			PropPrototype propPrototype = Building.GetPropPrototype();
			Equipment equipment = (propPrototype.NeedToolboxToRepair() ? character.Inventory.GetToolbox() : character.Inventory.FindItemOfType(neededResourceType));
			if (character.EquippedItem != equipment)
			{
				if (equipment != null)
				{
					return new Equip(equipment);
				}
				return GetFirstSubGoal(character, parent);
			}
			return new AnimationGoal((propPrototype.RepairAnim == RepairAnimType.Hammering) ? ActionAnim.RepairStart : ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (SubGoal is Equip && Building != null)
		{
			return new AnimationGoal((Building.GetPropPrototype().RepairAnim == RepairAnimType.Hammering) ? ActionAnim.RepairStart : ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		AnimationGoal animationGoal = SubGoal as AnimationGoal;
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.RepairStart || animationGoal.GetAnim() == ActionAnim.CraftStart) && Building != null)
		{
			return new RepairAnim(Building, (animationGoal.GetAnim() == ActionAnim.RepairStart) ? ActionAnim.Repair : ActionAnim.CraftNonLooped, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.Repair || animationGoal.GetAnim() == ActionAnim.CraftNonLooped))
		{
			return new AnimationGoal((animationGoal.GetAnim() == ActionAnim.Repair) ? ActionAnim.RepairFinish : ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
		}
		if (animationGoal != null && (animationGoal.GetAnim() == ActionAnim.RepairFinish || animationGoal.GetAnim() == ActionAnim.CraftEnd) && Building != null)
		{
			float num = Math.Min(1f, Building.GetCaptureResourceNeeded() * (1f - Building.GetCaptureFraction()));
			if (GetLeftOverIngredientAmount(neededResourceType) < num)
			{
				Equipment equipment2 = character.Inventory.FindItemOfType(neededResourceType);
				if (equipment2 != null)
				{
					character.Inventory.UseItemOfType(character, character, equipment2.GetPrototype(), 1, null, out var _);
					AddLeftOverIngredient(neededResourceType, 1f);
					character.Skillset.AddProgress(character, SkillType.Construction, 10f);
				}
			}
			float num2 = Math.Min(num, GetLeftOverIngredientAmount(neededResourceType));
			AddLeftOverIngredient(neededResourceType, 0f - num2);
			if (Building.CaptureBuilding(num2 / Building.GetCaptureResourceNeeded(), character))
			{
				character.OnRoleSucceeded();
				Prop prop = Building as Prop;
				prop?.SetCommunity(character.Community);
				if (prop != null)
				{
					Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, null, prop, SpeechSituation.CaptureComplete);
					character.Speak(speechForSituation4, null, prop);
				}
				character.CancelRole(new RoleInfo(Role.Capturing, Tile));
				return null;
			}
			return GetFirstSubGoal(character, parent);
		}
		OnFailed(character);
		return null;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (Building != null)
		{
			Community community = Building.GetCommunity();
			if (!GameCursor.CanTakeOverCommunityBuildings(character, community))
			{
				character.OnRoleSucceeded();
				character.CancelRole(new RoleInfo(Role.Capturing, Tile));
				Finished = true;
			}
		}
	}

	public EquipmentPrototype GetNeededResourceType()
	{
		if (Building == null)
		{
			return null;
		}
		return Building.GetCaptureResourceType();
	}

	public float GetNeededResourceAmount()
	{
		if (Building == null)
		{
			return 0f;
		}
		return Building.GetCaptureResourceNeeded() * (1f - Building.GetCaptureFraction());
	}

	public static TerrainCoord FindPositionToHammer(Character character, TileObject building, Vector2 posXZ)
	{
		TerrainCoord tileCoordForPosXZ = GameTerrain.Instance.GetTileCoordForPosXZ(posXZ);
		tileCoordForPosXZ = building.GetNearestTileTo(tileCoordForPosXZ);
		TerrainCoord terrainCoord = building.GetMaxTile() - building.GetMinTile();
		if (terrainCoord.x > 0 && terrainCoord.y > 0)
		{
			Bounds boundingBox = building.GetBoundingBox();
			Vector3 center = boundingBox.center;
			if (Math.Abs((posXZ.x - center.x) / (boundingBox.max.x - center.x)) >= Math.Abs((posXZ.y - center.z) / (boundingBox.max.z - center.z)))
			{
				if (posXZ.x >= center.x)
				{
					tileCoordForPosXZ.x = building.GetMaxTile().x + 1;
				}
				else
				{
					tileCoordForPosXZ.x = building.GetMinTile().x - 1;
				}
			}
			else if (posXZ.y >= center.z)
			{
				tileCoordForPosXZ.y = building.GetMaxTile().y + 1;
			}
			else
			{
				tileCoordForPosXZ.y = building.GetMinTile().y - 1;
			}
		}
		return building.GetNearestPassableTileTo(tileCoordForPosXZ, 2053, character, null);
	}

	public Goal GetFirstSubGoal(Character character, Goal parent)
	{
		Building = null;
		Tile = TerrainCoord.Invalid;
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role != Role.Capturing)
			{
				continue;
			}
			TerrainCoord targetLocation = character.Roles[i].TargetLocation;
			TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(targetLocation.x, targetLocation.y);
			if (fixedObjectOnTile != null && fixedObjectOnTile.GetTile() == targetLocation && fixedObjectOnTile.GetCommunity() != character.Community && !fixedObjectOnTile.IsDestroyed() && character.GetSkillLevelWithEffects(SkillType.Construction) >= fixedObjectOnTile.GetCaptureSkillNeeded())
			{
				if (!character.Roles[i].Paused && !character.Roles[i].FailedRecently)
				{
					if (!character.HasMovementZone() || character.MovementZone.Overlaps(fixedObjectOnTile.GetTileRect()))
					{
						Building = fixedObjectOnTile;
						Tile = targetLocation;
						MovementType = ((!character.Roles[i].Urgent) ? MovementType.Walk : MovementType.Run);
						character.SetRoleInProgress(i, inProgress: true);
						break;
					}
					character.SetRoleFailedRecently(character.Roles[i]);
				}
			}
			else
			{
				_ = character.Roles.Count;
				character.CancelRole(character.Roles[i]);
				i = -1;
			}
		}
		if (Building == null)
		{
			_lastAttemptedTime = Session.Instance.PlayTime;
			return null;
		}
		return GetFirstSubGoalForBuilding(character, parent);
	}

	private Goal GetFirstSubGoalForBuilding(Character character, Goal parent)
	{
		EquipmentPrototype neededResourceType = GetNeededResourceType();
		if (neededResourceType == null)
		{
			return null;
		}
		Type type = (Building.GetPropPrototype().NeedToolboxToRepair() ? typeof(Toolbox) : null);
		if (type != null && character.Inventory.FindItemOfClass(type) == null)
		{
			character.DirectControlledCrouching = false;
			return new FindGoal(FindType.Toolbox, MovementType, critical: false);
		}
		float neededResourceAmount = GetNeededResourceAmount();
		float leftOverIngredientAmount = GetLeftOverIngredientAmount(neededResourceType);
		int num = character.Inventory.CountItemsOfType(neededResourceType);
		int val = Mathf.CeilToInt(neededResourceAmount - leftOverIngredientAmount);
		val = Math.Min(val, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / neededResourceType.Weight) + num);
		if (val > num && Building != null && Building.GetTileRect().GetClosestDistSqTo(character.Tile) > MathUtil.Squared(8f))
		{
			character.DirectControlledCrouching = false;
			SetCrouching(character, parent, character.DirectControlledCrouching);
			return new FindGoal(FindType.Prototype, neededResourceType, MovementType, critical: false)
			{
				CanTravelFar = character.IsControllableByPlayer(),
				DesiredAmountOfRecipe = val
			};
		}
		if ((num > 0 || leftOverIngredientAmount > 0f) && Building != null)
		{
			Vector2 posXZ = Building.PosXZ;
			if (Building is Prop prop)
			{
				PrefabResource unityModel = prop.GetUnityModel();
				int num2 = (int)(prop.GetCaptureFraction() * (float)unityModel.Windows.Count);
				if (num2 < unityModel.Windows.Count)
				{
					posXZ = MathUtil.ToXZ(prop.World.MultiplyPoint(unityModel.Windows[num2].Pos));
				}
			}
			TerrainCoord destTile = FindPositionToHammer(character, Building, posXZ);
			if (destTile.GetDistSquared(character.Tile) > MathUtil.Squared(16f))
			{
				character.DirectControlledCrouching = false;
			}
			SetCrouching(character, parent, character.DirectControlledCrouching);
			return new MoveAsCloseAsPossibleTo(MovementType, destTile, MaxDistFromBuilding, FindGoal.CalcDontOpenOurGates(character));
		}
		character.DirectControlledCrouching = false;
		SetCrouching(character, parent, character.DirectControlledCrouching);
		return new FindGoal(FindType.Prototype, neededResourceType, MovementType, critical: false)
		{
			CanTravelFar = character.IsControllableByPlayer(),
			DesiredAmountOfRecipe = Math.Max(1, val)
		};
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Capturing, Tile);
	}
}
