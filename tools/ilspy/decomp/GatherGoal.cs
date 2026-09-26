using System;
using System.Collections.Generic;
using UnityEngine;

public class GatherGoal : RoleGoal
{
	private struct EquipmentScore
	{
		public Equipment Item;

		public float Score;

		public EquipmentScore(Equipment item, float score)
		{
			Item = item;
			Score = score;
		}
	}

	public class SortEquipmentByScoreDescending : IComparer<EquipmentScore>
	{
		int IComparer<EquipmentScore>.Compare(EquipmentScore a, EquipmentScore b)
		{
			if (a.Score < b.Score)
			{
				return 1;
			}
			if (a.Score > b.Score)
			{
				return -1;
			}
			if (a.Item.Id < b.Item.Id)
			{
				return 1;
			}
			if (a.Item.Id > b.Item.Id)
			{
				return -1;
			}
			return 0;
		}
	}

	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private TerrainCoord CurrentTargetLocation;

	private EquipmentPrototype CurrentResourceType;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	private static SortEquipmentByScoreDescending EquipmentSorter = new SortEquipmentByScoreDescending();

	private static List<EquipmentScore> DumpItems = new List<EquipmentScore>();

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorGather;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastAttemptedTime);
		if (reflector.Version < 522)
		{
			List<GatheredItem> list = new List<GatheredItem>();
			reflector.Add(ref list);
			for (int i = 0; i < list.Count; i++)
			{
				int num = list[i].Amount;
				foreach (Equipment content in character.Inventory.Contents)
				{
					if (content.GetPrototype() == list[i].Type && content.GetLiquidContentsType() == list[i].Liquid && content.InfectedWith == list[i].InfectedWith && !content.WasGifted() && !content.IsWorn(character))
					{
						content.ForceSetGatheredAmount(Math.Min(content.GetAmount(), num));
						num -= content.GetGatheredAmount();
						if (num <= 0)
						{
							break;
						}
					}
				}
			}
		}
		reflector.AddAfter(ref MovementType, 183);
		reflector.AddAfter(ref CurrentTargetLocation, 353);
		reflector.AddAfter(ref CurrentResourceType, 353);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.GatherGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.Community == null)
		{
			return false;
		}
		if (!character.HasRunningRole(Role.Gatherer))
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Gatherer))
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
		return (GoalPriority)(210 - character.GetFirstRunningRoleIndex(Role.Gatherer));
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	private bool CanGatherSuppliesFrom(Character character, Prop prop)
	{
		if (prop.Inventory.Count == 0)
		{
			return false;
		}
		if (prop is Building building && !building.CouldEnterIfNotFull(character))
		{
			return false;
		}
		int amount = 0;
		if (GetSuppliesToGather(character, prop, out amount) == null)
		{
			return false;
		}
		return true;
	}

	public static bool CanStoreSuppliesIn(Character character, Prop prop, float lightestItemWeight)
	{
		if (prop.Community != character.Community)
		{
			return false;
		}
		if (prop.GetUnderConstructionInfo() != null)
		{
			return false;
		}
		if (prop is CraftingProp)
		{
			return false;
		}
		if (prop is Building building && !building.CouldEnterIfNotFull(character))
		{
			return false;
		}
		if (character.HasFailedFindAttempt(prop, 0, 0, FindType.Deposit, null, null, null, TimeSpan.FromSeconds(60.0), out var _, out var _))
		{
			return false;
		}
		if (lightestItemWeight <= 0f)
		{
			return true;
		}
		return prop.HasInventorySpaceFor(lightestItemWeight);
	}

	public static Prop GetBuildingToStoreSuppliesIn(Character character, GatheredItem gatheredItem)
	{
		return GetBuildingToStoreSuppliesIn(character, gatheredItem, null);
	}

	public static Prop GetBuildingToStoreSuppliesIn(Character character, GatheredItem gatheredItem, Prop alreadyInProp)
	{
		TerrainCoord other = alreadyInProp?.Tile ?? character.Tile;
		float num = float.MaxValue;
		Prop result = null;
		if (character.Community != null)
		{
			foreach (Prop building in character.Community.Buildings)
			{
				float num2 = building.Tile.GetDist(other);
				if (building.IsGuardPost())
				{
					num2 += 200f;
				}
				else if (building.GetPropPrototype().IsLooterProp)
				{
					num2 += 300f;
				}
				BaseObjectType baseObjectType = building.GetBaseObjectType();
				if (baseObjectType <= BaseObjectType.RabbitTrap)
				{
					if (baseObjectType != BaseObjectType.Outhouse)
					{
						if (baseObjectType == BaseObjectType.Grave || baseObjectType == BaseObjectType.RabbitTrap)
						{
							continue;
						}
					}
					else
					{
						num2 += 100f;
					}
				}
				else if (baseObjectType != BaseObjectType.EnterableVehicle)
				{
					if (baseObjectType == BaseObjectType.AnimalFeederProp || baseObjectType == BaseObjectType.AnimalDrinkerProp)
					{
						continue;
					}
				}
				else
				{
					num2 += 30f;
				}
				if (building.StoragePolicies != null)
				{
					for (int i = 0; i < building.StoragePolicies.Count; i++)
					{
						if (building.StoragePolicies[i].Matches(gatheredItem))
						{
							num2 -= 100000f;
							break;
						}
					}
				}
				if (num2 < num && CanStoreSuppliesIn(character, building, (alreadyInProp == building) ? 0f : gatheredItem.Type.Weight))
				{
					result = building;
					num = num2;
				}
			}
		}
		return result;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Goal firstSubGoal = GetFirstSubGoal(character, parent);
		if (firstSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, firstSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	private Equipment GetSuppliesToGather(Character character, Prop prop, out int amount)
	{
		Equipment equipment = null;
		int num = int.MaxValue;
		if (prop != null)
		{
			for (int i = 0; i < prop.Inventory.Count; i++)
			{
				Equipment item = prop.Inventory.GetItem(i);
				if (character.HasInventorySpaceFor(item.GetWeight()))
				{
					int num2 = character.Community.CountInventoryItemsOfType(item.GetPrototype());
					if (num2 < num)
					{
						equipment = item;
						num = num2;
					}
				}
			}
		}
		amount = ((equipment != null) ? character.GetAmountOfEquipmentThatCanBeStored(equipment) : 0);
		return equipment;
	}

	private void OnFailed(Character character, TerrainCoord targetLocation, EquipmentPrototype resourceType)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Gatherer, targetLocation, resourceType));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	private Goal GetFirstSubGoal(Character character, Goal parent)
	{
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role != Role.Gatherer || character.Roles[i].Paused || character.Roles[i].FailedRecently)
			{
				continue;
			}
			EquipmentPrototype resourceType = character.Roles[i].ResourceType;
			TerrainCoord targetLocation = character.Roles[i].TargetLocation;
			Prop prop = GameTerrain.Instance.GetFixedObjectOnTile(targetLocation.x, targetLocation.y) as Prop;
			Equipment equipment = null;
			float availableInventorySpace = character.GetAvailableInventorySpace();
			bool flag = false;
			if (prop != null)
			{
				if (resourceType == null)
				{
					float num = float.MinValue;
					foreach (Equipment content in prop.Inventory.Contents)
					{
						float num2 = content.GetWeight() + content.GetBasePrice();
						if (content.GetWeight() <= availableInventorySpace)
						{
							num2 += 100000f;
						}
						if (character.Community != null)
						{
							if (content.GetLiquidContentsType() != null)
							{
								int craftingLimit = character.Community.GetCraftingLimit(content.GetLiquidContentsType());
								if (craftingLimit < int.MaxValue && character.Community.GetTotalLiquid(content.GetLiquidContentsType()) >= (float)craftingLimit)
								{
									flag = true;
									continue;
								}
							}
							else
							{
								int craftingLimit2 = character.Community.GetCraftingLimit(content.GetPrototype());
								if (craftingLimit2 < int.MaxValue && character.Community.CountInventoryItemsOfType(content.GetPrototype()) >= craftingLimit2)
								{
									flag = true;
									continue;
								}
							}
						}
						if (num2 > num)
						{
							num = num2;
							equipment = content;
						}
					}
				}
				else
				{
					if (character.Community != null)
					{
						int craftingLimit3 = character.Community.GetCraftingLimit(resourceType);
						if (craftingLimit3 < int.MaxValue && character.Community.CountInventoryItemsOfType(resourceType) >= craftingLimit3)
						{
							flag = true;
						}
					}
					equipment = prop.Inventory.FindItemOfType(resourceType);
				}
			}
			MovementType = ((!character.Roles[i].Urgent) ? MovementType.Walk : MovementType.Run);
			character.SetRoleInProgress(i, inProgress: true);
			Equipment bestItem = null;
			Prop prop2 = null;
			int amountToDeposit = 0;
			if (equipment != null)
			{
				float maxInventoryWeight = character.GetMaxInventoryWeight();
				float weight = equipment.GetWeight();
				int num3 = Math.Max(1, Math.Min(equipment.GetAmount(), Mathf.CeilToInt((weight > 0f) ? (maxInventoryWeight * 0.5f / weight) : 0f)));
				prop2 = GetMoveToAndDepositProp(character, weight, weight * (float)num3, out bestItem, out amountToDeposit, out var _);
			}
			else
			{
				bestItem = character.Inventory.FindGatheredItemOfType(resourceType);
				if (bestItem != null)
				{
					amountToDeposit = bestItem.GetGatheredAmount();
					prop2 = GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(bestItem));
				}
			}
			if (prop2 != null)
			{
				TerrainCoord tile = character.Tile;
				if (prop2 != null && prop2.Tile.GetDistSquared(tile) < targetLocation.GetDistSquared(tile))
				{
					return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
			}
			if (prop == null)
			{
				if (prop2 != null)
				{
					return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
				character.CancelRole(character.Roles[i]);
				character.OnRoleSucceeded();
				return null;
			}
			if (prop is Building building)
			{
				if (building.CanEnterReason(character) == CursorActionDisabledReason.OccupiedByOtherCommunity)
				{
					if (prop2 != null)
					{
						return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
						{
							WasGathered = (bestItem.GetGatheredAmount() > 0)
						};
					}
					character.CancelRole(character.Roles[i]);
					character.OnRoleSucceeded();
					return null;
				}
			}
			else if (prop.Community != character.Community && prop.Community != null && prop.Community.HasAnyActiveMembers())
			{
				if (prop2 != null)
				{
					return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
				character.CancelRole(character.Roles[i]);
				character.OnRoleSucceeded();
				return null;
			}
			if (equipment == null)
			{
				if (prop2 != null)
				{
					return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
				if (flag)
				{
					character.SetRoleFailedRecently(character.Roles[i]);
					continue;
				}
				character.CancelRole(character.Roles[i]);
				character.OnRoleSucceeded();
				if (!character.HasRunningRole(Role.Gatherer))
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.GatherDepleted);
					character.Speak(speechForSituation);
					if (character.IsControllableByPlayer() && character.Community.IsCloseToBase(character.Tile, 8f))
					{
						character.SetHangoutLocation(character.Tile);
					}
				}
				return null;
			}
			if (equipment.GetWeight() > availableInventorySpace)
			{
				if (prop2 != null)
				{
					return new MoveToAndDeposit(character, prop2, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
				if (character.IsControllableByPlayer())
				{
					MemoryParam param = new MemoryParam(equipment.GetPrototype());
					Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
					character.Speak(speechForSituation2, null, null, param);
					character.PauseRole(character.Roles[i]);
				}
				else
				{
					OnFailed(character, targetLocation, resourceType);
				}
				return null;
			}
			MoveToAndTake result = new MoveToAndTake(character, prop, equipment, equipment.GetAmount(), ignoreWeight: false, MovementType)
			{
				IsGathering = true,
				DontOpenOurGates = FindGoal.CalcDontOpenOurGates(character)
			};
			CurrentTargetLocation = targetLocation;
			CurrentResourceType = resourceType;
			return result;
		}
		_lastAttemptedTime = Session.Instance.PlayTime;
		return null;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndTake { SuccessfullyReachedDestination: not false } moveToAndTake)
		{
			if (moveToAndTake.AmountRetrieved > 0f && moveToAndTake.GetTargetEquipment() != null && !moveToAndTake.IsTargetDeleted())
			{
				character.IssueCommandToFollowers(FollowCommand.GatherItems, moveToAndTake.Target.Object.GetCentreTile(), moveToAndTake.GetTargetEquipment().GetPrototype());
			}
			return GetFirstSubGoal(character, parent);
		}
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: not false })
		{
			return GetFirstSubGoal(character, parent);
		}
		OnFailed(character, CurrentTargetLocation, CurrentResourceType);
		return base.GetNextSubGoal(character, parent);
	}

	public static float GetItemDumpScore(Character character, Equipment item)
	{
		float num = item.GetWeight() * (float)item.GetAmount();
		BaseObjectType baseObjectType = item.GetBaseObjectType();
		if (baseObjectType == BaseObjectType.MolotovCocktail || baseObjectType == BaseObjectType.PipeBomb)
		{
			num *= 0.1f;
		}
		if (item.GetBandageLevel() > 0 || item.GetAntigenType() != InfectionType.None)
		{
			num *= 0.1f;
		}
		if (item.GetPrototype().SkillBonus > 0 && item.GetClothingType() == ClothingType.Invalid)
		{
			num -= 10f;
		}
		if (item.GetGatheredAmount() > 0 && !character.IsAnyActiveRoleUsingForCrafting(item))
		{
			num += 100000f;
		}
		return num;
	}

	public static Equipment GetBestAmmoWeaponOfType(Character character, EquipmentPrototype proto)
	{
		if (character.EquippedItem != null && character.EquippedItem.GetPrototype() == proto)
		{
			return character.EquippedItem;
		}
		int num = -1;
		Equipment result = null;
		foreach (Equipment content in character.Inventory.Contents)
		{
			if (content.GetPrototype() == proto && content is AmmoWeapon ammoWeapon && content.GetGatheredAmount() == 0)
			{
				int currentAmmo = ammoWeapon.GetCurrentAmmo();
				if (currentAmmo > num)
				{
					num = currentAmmo;
					result = ammoWeapon;
				}
			}
		}
		return result;
	}

	public static Equipment GetBestItemToDump(Character character, bool wantToFreeSpace, bool needToFreeSpace, ref Prop resultProp, out int resultAmountToDeposit)
	{
		Equipment result = null;
		resultAmountToDeposit = 0;
		if (!character.IsControllableByPlayer() && character.HasRole(Role.Organizer))
		{
			foreach (Equipment content in character.Inventory.Contents)
			{
				if (content.GetGatheredAmount() > 0)
				{
					resultProp = GetBuildingToStoreSuppliesIn(character, GatheredItem.Create(content));
					if (resultProp != null)
					{
						resultAmountToDeposit = content.GetGatheredAmount();
						return content;
					}
				}
			}
		}
		Equipment bestWeapon = character.Inventory.GetBestWeapon(character, null, wantRanged: false, bluntOnly: false, ignoreGathered: true);
		Equipment bestWaterBottle = character.Inventory.GetBestWaterBottle(ignoreGathered: true);
		float num = (needToFreeSpace ? 0f : 0.1f);
		DumpItems.Clear();
		foreach (Equipment content2 in character.Inventory.Contents)
		{
			if ((content2.GetPrototype().CanBeAutoDeposited || content2.GetGatheredAmount() != 0) && (wantToFreeSpace || content2.GetGatheredAmount() != 0))
			{
				float itemDumpScore = GetItemDumpScore(character, content2);
				if (itemDumpScore > num)
				{
					DumpItems.Add(new EquipmentScore(content2, itemDumpScore));
				}
			}
		}
		DumpItems.Sort(EquipmentSorter);
		for (int i = 0; i < DumpItems.Count; i++)
		{
			Equipment item = DumpItems[i].Item;
			if (item == bestWaterBottle || !FindGoal.CanTransferFromCharacter(character, item, FindType.DumpGatheredItems, out var amountToTransfer))
			{
				continue;
			}
			if (item.GetGatheredAmount() < item.GetAmount())
			{
				if (item == bestWeapon)
				{
					amountToTransfer = Math.Min(amountToTransfer, item.GetAmount() - 1);
					if (amountToTransfer <= 0)
					{
						continue;
					}
				}
				if (item is AmmoWeapon && GetBestAmmoWeaponOfType(character, item.GetPrototype()) == item)
				{
					continue;
				}
				BaseObjectType baseObjectType = item.GetBaseObjectType();
				if (baseObjectType == BaseObjectType.HuntingKnife || baseObjectType == BaseObjectType.BrainScanner)
				{
					amountToTransfer = Math.Min(amountToTransfer, item.GetAmount() - 1);
					if (amountToTransfer <= 0)
					{
						continue;
					}
				}
			}
			GatheredItem gatheredItem = GatheredItem.Create(item);
			if (resultProp == null)
			{
				resultProp = GetBuildingToStoreSuppliesIn(character, gatheredItem);
			}
			else
			{
				bool flag = true;
				if (!OrganizerGoal.IsGatheredItemInStoragePolicies(resultProp.StoragePolicies, gatheredItem))
				{
					foreach (Prop building in character.Community.Buildings)
					{
						if (OrganizerGoal.IsGatheredItemInStoragePolicies(building.StoragePolicies, gatheredItem))
						{
							flag = false;
							break;
						}
					}
				}
				if (!flag)
				{
					continue;
				}
			}
			if (resultProp != null)
			{
				resultAmountToDeposit = amountToTransfer;
				result = item;
				break;
			}
		}
		DumpItems.Clear();
		return result;
	}

	public static Prop GetMoveToAndDepositProp(Character character, float requiredWeightToFree, float idealWeightToFree, out Equipment bestItem, out int amountToDeposit, out bool failed)
	{
		bestItem = null;
		amountToDeposit = 0;
		float availableInventorySpace = character.GetAvailableInventorySpace();
		bool flag = availableInventorySpace < requiredWeightToFree;
		bool flag2 = availableInventorySpace < idealWeightToFree;
		failed = flag;
		bool flag3 = false;
		float num = 0f;
		int num2 = 0;
		foreach (Equipment content in character.Inventory.Contents)
		{
			if (content.GetGatheredAmount() > 0 && !character.IsAnyActiveRoleUsingForCrafting(content))
			{
				float num3 = content.GetNutrition() * (float)content.GetAmount();
				num += num3;
				if (content.GetSeedForPlantType() != null)
				{
					num2 += content.GetAmount();
				}
				else if (num3 == 0f)
				{
					flag3 = true;
				}
			}
		}
		if (num >= CraftGoal.StoreCraftedNutritionThreshold || num2 >= FarmingGoal.MaxSeedsToCarry || flag || flag2 || flag3)
		{
			Prop resultProp = null;
			bestItem = GetBestItemToDump(character, flag2, flag, ref resultProp, out amountToDeposit);
			if (resultProp != null)
			{
				if (num < CraftGoal.StoreCraftedNutritionThreshold && num2 < FarmingGoal.MaxSeedsToCarry && !flag && !flag2 && resultProp.GetNearestTileTo(character.Tile).GetDistSquared(character.Tile) >= MathUtil.Squared(64f))
				{
					return null;
				}
				failed = false;
				return resultProp;
			}
		}
		return null;
	}

	public static Goal GetMoveToAndDepositGoal(Character character, float requiredWeightToFree, float idealWeightToFree, MovementType movementType, out bool failed)
	{
		Equipment bestItem;
		int amountToDeposit;
		Prop moveToAndDepositProp = GetMoveToAndDepositProp(character, requiredWeightToFree, idealWeightToFree, out bestItem, out amountToDeposit, out failed);
		if (moveToAndDepositProp != null)
		{
			return new MoveToAndDeposit(character, moveToAndDepositProp, bestItem, amountToDeposit, movementType)
			{
				WasGathered = (bestItem.GetGatheredAmount() > 0)
			};
		}
		return null;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Gatherer, CurrentTargetLocation, CurrentResourceType);
	}
}
