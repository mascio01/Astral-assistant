using System;
using System.Collections.Generic;
using UnityEngine;

public class FarmingGoal : RoleGoal
{
	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private TimeSpan LastFindSeedsFailedTime = TimeSpan.FromDays(-365.0);

	private TimeSpan LastFindWateringCanFailedTime = TimeSpan.FromDays(-365.0);

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public static float MaxFarmingDist = 2048f;

	public static float NeedWateringMoistureLevel = 0.25f;

	public static float NeedWateringPrecipitationLevel = 0.25f;

	private static float FarmerSpreadDist = 20f;

	public static float RequiredFreeInventorySpace = 2f;

	public static float ReservedNutritionDays = 2f;

	public static int MaxSeedsToCarry = 20;

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is FillWateringCanAndWaterPlant)
		{
			return GameCursor.CursorUseWateringCan;
		}
		if (SubGoal is PlantCrops)
		{
			return GameCursor.CursorPlant;
		}
		if (SubGoal is HarvestCrops)
		{
			return GameCursor.CursorPlant;
		}
		if (SubGoal is Conversation)
		{
			return GameCursor.CursorPlant;
		}
		FindGoal findGoal = SubGoal as FindGoal;
		if (findGoal != null && findGoal.FindType == FindType.WateringCan)
		{
			return GameCursor.CursorUseWateringCan;
		}
		if (findGoal != null && findGoal.FindType == FindType.Seeds)
		{
			return GameCursor.CursorPlant;
		}
		return null;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastAttemptedTime);
		reflector.AddAfter(ref LastFindSeedsFailedTime, 18);
		reflector.AddAfter(ref LastFindWateringCanFailedTime, 18);
		reflector.AddAfter(ref MovementType, 183);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FarmingGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Farmer))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Farmer))
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
		if (Active)
		{
			ActionAnim currentActionAnim = character.CurrentActionAnim;
			if ((uint)(currentActionAnim - 68) <= 1u || (uint)(currentActionAnim - 74) <= 2u)
			{
				return GoalPriority.Survivor_Role_Animation;
			}
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Farmer));
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Farmer));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	private float IsAnyoneElseWateringOrHarvestingThisPlant(Character character, PlantableCrop plant)
	{
		float num = float.MaxValue;
		if (character.Community != null)
		{
			foreach (Character member in character.Community.Members)
			{
				if (member != character && member.AliveAndNotZombie && member.GetGoal() != null)
				{
					num = Math.Min(num, member.GetGoal().IsWateringOrHarvestingPlant(plant));
				}
			}
		}
		return num;
	}

	private PlantableCrop FindCropToWaterOrHarvest(Character character)
	{
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		int num = ((character.Community != null) ? character.Community.Id : 0);
		TerrainCoord invalid = TerrainCoord.Invalid;
		PlantableCrop result = null;
		float num2 = float.MaxValue;
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId != num || ((invalid == TerrainCoord.Invalid || patch.Rect.GetClosestDistSqTo(invalid) >= MathUtil.Squared(MaxFarmingDist)) && patch.Rect.GetClosestDistSqTo(character.Tile) >= MathUtil.Squared(MaxFarmingDist)) || (character.HasMovementZone() && !character.MovementZone.Overlaps(patch.Rect)))
			{
				continue;
			}
			foreach (PlantableCrop item in patch.CropsInPatch)
			{
				if (item.IsDead() || (character.HasMovementZone() && !character.MovementZone.Contains(item.Tile)))
				{
					continue;
				}
				float moisture = item.GetMoisture();
				float dist = item.Tile.GetDist((invalid == TerrainCoord.Invalid) ? character.Tile : invalid);
				float num3 = moisture * 100f + dist;
				if (moisture < -0.5f)
				{
					num3 -= 100f;
				}
				if (item.IsRipe())
				{
					num3 -= 10f;
					if (Session.Instance.Weather.TemperatureInCelsius <= 0f)
					{
						num3 -= 10000f;
					}
				}
				else if (moisture >= NeedWateringMoistureLevel || Session.Instance.Weather.PrecipitationAmount > NeedWateringPrecipitationLevel)
				{
					continue;
				}
				if (!(num3 < num2))
				{
					continue;
				}
				float num4 = IsAnyoneElseWateringOrHarvestingThisPlant(character, item);
				if (!(num4 < 0.1f))
				{
					if (num4 < FarmerSpreadDist)
					{
						num3 += FarmerSpreadDist - num4;
					}
					if (num3 < num2)
					{
						num2 = num3;
						result = item;
					}
				}
			}
		}
		return result;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.Farmer), inProgress: true);
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

	private Goal GetStoreHarvestedCropsSubGoal(Character character, float weightToFree, out bool failed)
	{
		failed = false;
		if (weightToFree == float.MaxValue || !character.HasInventorySpaceFor(weightToFree))
		{
			Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, weightToFree, weightToFree, MovementType, out failed);
			if (moveToAndDepositGoal != null)
			{
				return moveToAndDepositGoal;
			}
		}
		return null;
	}

	private Goal GetWaterOrHarvestCropsSubGoal(Character character, Goal parent, PlantableCrop crop)
	{
		if (crop.IsRipe())
		{
			bool failed;
			Goal storeHarvestedCropsSubGoal = GetStoreHarvestedCropsSubGoal(character, crop.GetHarvestWeight(), out failed);
			if (storeHarvestedCropsSubGoal != null)
			{
				return storeHarvestedCropsSubGoal;
			}
			if (!failed)
			{
				return new HarvestCrops(crop.Tile, ignoreWeight: true)
				{
					IsGathering = true
				};
			}
		}
		if (crop.GetMoisture() >= NeedWateringMoistureLevel || Session.Instance.Weather.PrecipitationAmount > NeedWateringPrecipitationLevel)
		{
			return null;
		}
		if (character.Inventory.GetBestWateringCan() != null)
		{
			character.SetRoleTargetLocation(Role.Farmer, crop.Tile);
			return new FillWateringCanAndWaterPlant(crop.Tile, MovementType);
		}
		if (Session.Instance.PlayTime - LastFindWateringCanFailedTime >= MinTimeBetweenAttempts)
		{
			float weightToFree = ((EquipmentPrototype.WateringCan != null) ? EquipmentPrototype.WateringCan.Weight : 4f);
			bool failed2;
			Goal storeHarvestedCropsSubGoal2 = GetStoreHarvestedCropsSubGoal(character, weightToFree, out failed2);
			if (storeHarvestedCropsSubGoal2 != null)
			{
				return storeHarvestedCropsSubGoal2;
			}
			if (!failed2)
			{
				return new FindGoal(FindType.WateringCan, MovementType, critical: false);
			}
		}
		return null;
	}

	private bool CanPlantNewCrops(Character character)
	{
		if (character.IsControllableByPlayer())
		{
			if (!Session.Instance.Weather.IsSafeForPlantingCrops())
			{
				return false;
			}
		}
		else if (!Session.Instance.Weather.IsSafeForPlantingCropsAI())
		{
			return false;
		}
		if (!character.IsControllableByPlayer() && character.Community != null)
		{
			CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
			float nutritionAmountNeededToReplaceCurrentStocks = character.Community.GetNutritionAmountNeededToReplaceCurrentStocks();
			if (character.Community.GetPlantedNutritionAmount() < nutritionAmountNeededToReplaceCurrentStocks)
			{
				return true;
			}
			float num = Weather.CalcNutritionNeededToStoreForWinterPerPerson(Session.Instance.DayOfYear, rampUpOverPlantingSeason: true) * character.Community.GetMemberCountWhenConsideringFoodNeeds();
			if (character.Community.GetHarvestedNutritionAmount() < num)
			{
				return true;
			}
			foreach (PropPrototype plantableCropType in PropPrototype.PlantableCropTypes)
			{
				if (plantableCropType.HarvestSeedsPrototype != null)
				{
					int reservedCropAmount = character.Community.GetReservedCropAmount(plantableCropType);
					int num2 = character.Community.CountInventoryItemsOfType(plantableCropType.HarvestSeedsPrototype);
					if (num2 < reservedCropAmount && num2 > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		return true;
	}

	private bool WouldExceedLimitsForCropType(Character character, PropPrototype cropType)
	{
		if (character.Community != null)
		{
			EquipmentPrototype harvestPrototype = cropType.HarvestPrototype;
			EquipmentPrototype harvestSeedsPrototype = cropType.HarvestSeedsPrototype;
			int num = ((harvestPrototype != null) ? character.Community.GetCraftingLimit(harvestPrototype) : int.MaxValue);
			int num2 = ((harvestSeedsPrototype != null) ? character.Community.GetCraftingLimit(harvestSeedsPrototype) : int.MaxValue);
			if (num < int.MaxValue && character.Community.CountInventoryItemsOfType(harvestPrototype) >= num && num2 < int.MaxValue && character.Community.CountInventoryItemsOfType(harvestSeedsPrototype) >= num2)
			{
				return true;
			}
		}
		return false;
	}

	private Goal GetPlantNewCropsSubGoal(Character character, Goal parent)
	{
		if (!CanPlantNewCrops(character))
		{
			return null;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		int num = 0;
		Span<TerrainCoord> span = stackalloc TerrainCoord[character.Community.Members.Count];
		if (character.Community != null)
		{
			foreach (Character member in character.Community.Members)
			{
				if (member.FindActiveGoal(GoalType.PlantCrops) is PlantCrops plantCrops)
				{
					span[num] = plantCrops.Tile;
					num++;
				}
			}
		}
		int num2 = ((character.Community != null) ? character.Community.Id : 0);
		TerrainCoord invalid = TerrainCoord.Invalid;
		float num3 = MathUtil.Squared(MaxFarmingDist);
		TerrainCoord terrainCoord = TerrainCoord.Invalid;
		TerrainCoord terrainCoord2 = ((invalid == TerrainCoord.Invalid) ? character.Tile : invalid);
		Equipment equipment = null;
		PlantableCrop plantableCrop = null;
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId != num2 || patch.Rect.GetClosestDistSqTo(terrainCoord2) > num3 || (character.HasMovementZone() && !character.MovementZone.Overlaps(patch.Rect)))
			{
				continue;
			}
			Equipment seedsForCropType = character.Inventory.GetSeedsForCropType(character, character, patch.CropType);
			if (seedsForCropType == null || WouldExceedLimitsForCropType(character, patch.CropType))
			{
				continue;
			}
			for (int i = 0; i < patch.Tiles.Count; i++)
			{
				TerrainCoord terrainCoord3 = patch.Tiles[i];
				float distSquared = terrainCoord3.GetDistSquared(terrainCoord2);
				if (!(distSquared < num3))
				{
					continue;
				}
				PlantableCrop plantableCrop2 = null;
				if (GameCursor.CanPlantHere(character, terrainCoord3, checkCharacters: true) != CursorActionDisabledReason.Enabled)
				{
					PlantableCrop plant = GameTerrain.Instance.GetPlant(terrainCoord3.x, terrainCoord3.y);
					if (plant == null || !plant.IsDead())
					{
						continue;
					}
					plantableCrop2 = plant;
				}
				if ((!character.HasMovementZone() || character.MovementZone.Contains(terrainCoord3)) && span.IndexOf(terrainCoord3, 0, num) == -1)
				{
					num3 = distSquared;
					terrainCoord = patch.Tiles[i];
					equipment = seedsForCropType;
					plantableCrop = plantableCrop2;
				}
			}
		}
		if (terrainCoord != TerrainCoord.Invalid && equipment != null)
		{
			if (plantableCrop != null)
			{
				return new HarvestCrops(terrainCoord, ignoreWeight: true);
			}
			character.SetRoleTargetLocation(Role.Farmer, terrainCoord);
			return new PlantCrops(terrainCoord, equipment, MovementType)
			{
				MustBeWithinPlantingZone = true
			};
		}
		if (Session.Instance.PlayTime - LastFindSeedsFailedTime >= MinTimeBetweenAttempts)
		{
			List<PropPrototype> list = new List<PropPrototype>();
			float num4 = 1f;
			foreach (CropPatch patch2 in Session.Instance.CropsManager.Patches)
			{
				if (patch2.CommunityId != num2 || list.Contains(patch2.CropType) || !patch2.IsWithinRangeOf(terrainCoord2, MaxFarmingDist) || (character.HasMovementZone() && !character.MovementZone.Overlaps(patch2.Rect)) || WouldExceedLimitsForCropType(character, patch2.CropType))
				{
					continue;
				}
				for (int j = 0; j < patch2.Tiles.Count; j++)
				{
					TerrainCoord terrainCoord4 = patch2.Tiles[j];
					if (terrainCoord4.GetDistSquared(terrainCoord2) < MaxFarmingDist * MaxFarmingDist && GameCursor.CanPlantHere(character, terrainCoord4, checkCharacters: true) == CursorActionDisabledReason.Enabled && (!character.HasMovementZone() || character.MovementZone.Contains(terrainCoord4)) && span.IndexOf(terrainCoord4, 0, num) == -1)
					{
						list.Add(patch2.CropType);
						num4 = Math.Max(num4, patch2.CropType.GetHarvestSeedsWeight());
						break;
					}
				}
			}
			bool failed;
			Goal storeHarvestedCropsSubGoal = GetStoreHarvestedCropsSubGoal(character, num4, out failed);
			if (storeHarvestedCropsSubGoal != null)
			{
				return storeHarvestedCropsSubGoal;
			}
			if (!failed && list.Count > 0)
			{
				return new FindGoal(FindType.Seeds, MovementType, critical: false)
				{
					CropTypesToFindSeedsFor = list
				};
			}
		}
		return null;
	}

	private Goal GetFirstSubGoal(Character character, Goal parent)
	{
		PlantableCrop plantableCrop = FindCropToWaterOrHarvest(character);
		if (plantableCrop != null && (plantableCrop.GetMoisture() <= PlantableCrop.CriticalDehydrationLevel * 0.5f || (plantableCrop.IsRipe() && Session.Instance.Weather.TemperatureInCelsius <= 0f)))
		{
			Goal waterOrHarvestCropsSubGoal = GetWaterOrHarvestCropsSubGoal(character, parent, plantableCrop);
			if (waterOrHarvestCropsSubGoal != null)
			{
				return waterOrHarvestCropsSubGoal;
			}
		}
		Goal plantNewCropsSubGoal = GetPlantNewCropsSubGoal(character, parent);
		if (plantNewCropsSubGoal != null)
		{
			return plantNewCropsSubGoal;
		}
		if (plantableCrop != null)
		{
			Goal waterOrHarvestCropsSubGoal2 = GetWaterOrHarvestCropsSubGoal(character, parent, plantableCrop);
			if (waterOrHarvestCropsSubGoal2 != null)
			{
				return waterOrHarvestCropsSubGoal2;
			}
		}
		float num = Sun.DayLengthSecs * 8f;
		int num2 = MaxSeedsToCarry;
		if (character.GetAvailableInventorySpace() < RequiredFreeInventorySpace)
		{
			num = 0f;
			num2 = 0;
		}
		if (character.Inventory.GetTotalHarvestedNutrition() > num || character.Inventory.CountSeeds() > num2)
		{
			bool failed;
			Goal storeHarvestedCropsSubGoal = GetStoreHarvestedCropsSubGoal(character, float.MaxValue, out failed);
			if (storeHarvestedCropsSubGoal != null)
			{
				return storeHarvestedCropsSubGoal;
			}
		}
		if (character.IsControllableByPlayer() && Session.Instance.Weather.TemperatureInCelsius <= 0f && Session.Instance.Weather.CalcAverageTemperatureInCelsius() <= 0f && Session.Instance.CropsManager.GetCommunityCropsCount(character.GetCommunityId()) == 0)
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.TooColdToFarm);
			character.Speak(speechForSituation);
			character.PauseRole(new RoleInfo(Role.Farmer));
		}
		OnFailed(character);
		return null;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndDeposit moveToAndDeposit)
		{
			if (moveToAndDeposit.SuccessfullyReachedDestination)
			{
				return GetFirstSubGoal(character, parent);
			}
			OnFailed(character);
			return null;
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.Success)
			{
				return GetFirstSubGoal(character, parent);
			}
			switch (findGoal.FindType)
			{
			case FindType.Seeds:
				LastFindSeedsFailedTime = Session.Instance.PlayTime;
				break;
			case FindType.WateringCan:
				LastFindWateringCanFailedTime = Session.Instance.PlayTime;
				break;
			}
			return GetFirstSubGoal(character, parent);
		}
		if (SubGoal is FillWateringCanAndWaterPlant { Success: not false })
		{
			character.OnRoleSucceeded();
			return GetFirstSubGoal(character, parent);
		}
		if (SubGoal is PlantCrops { Success: not false })
		{
			character.OnRoleSucceeded();
			return GetFirstSubGoal(character, parent);
		}
		if (SubGoal is HarvestCrops { Success: not false })
		{
			character.OnRoleSucceeded();
			return GetFirstSubGoal(character, parent);
		}
		OnFailed(character);
		return base.GetNextSubGoal(character, parent);
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Farmer);
	}
}
