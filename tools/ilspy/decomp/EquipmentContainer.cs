using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentContainer : IReflectable
{
	public class SortEquipmentByPriceDescending : IComparer<Equipment>
	{
		int IComparer<Equipment>.Compare(Equipment a, Equipment b)
		{
			float basePrice = a.GetBasePrice();
			float basePrice2 = b.GetBasePrice();
			if (basePrice < basePrice2)
			{
				return 1;
			}
			if (basePrice > basePrice2)
			{
				return -1;
			}
			return 0;
		}
	}

	public struct Equippable : IEquatable<Equippable>
	{
		public Equipment Item;

		public EquipmentPrototype AmmoType;

		public InfectionType InfectedWith;

		public int Amount;

		public bool Equals(Equippable other)
		{
			if (Item == other.Item && AmmoType == other.AmmoType)
			{
				return InfectedWith == other.InfectedWith;
			}
			return false;
		}
	}

	public class SortEquippablesByInfectionTypeAscending : IComparer<Equippable>
	{
		public static SortEquippablesByInfectionTypeAscending Instance = new SortEquippablesByInfectionTypeAscending();

		int IComparer<Equippable>.Compare(Equippable a, Equippable b)
		{
			if (a.InfectedWith > b.InfectedWith)
			{
				return 1;
			}
			if (a.InfectedWith < b.InfectedWith)
			{
				return -1;
			}
			return 0;
		}
	}

	private struct DrainCandidate : IComparable<DrainCandidate>
	{
		public Equipment Item;

		public float Score;

		public int CompareTo(DrainCandidate other)
		{
			if (Score < other.Score)
			{
				return -1;
			}
			if (Score > other.Score)
			{
				return 1;
			}
			if (Item.Id < other.Item.Id)
			{
				return -1;
			}
			if (Item.Id > other.Item.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	private struct EquipmentScore : IEquatable<EquipmentScore>
	{
		public Equipment Item;

		public EquipmentPrototype AmmoType;

		public InfectionType InfectedWith;

		public float Score;

		public bool Equals(EquipmentScore other)
		{
			if (Item == other.Item && AmmoType == other.AmmoType)
			{
				return InfectedWith == other.InfectedWith;
			}
			return false;
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
			if (a.InfectedWith > b.InfectedWith)
			{
				return -1;
			}
			if (a.InfectedWith < b.InfectedWith)
			{
				return 1;
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

	public List<Equipment> Contents = new List<Equipment>();

	public bool GeneratedLoot;

	public static SortEquipmentByPriceDescending PriceSorter = new SortEquipmentByPriceDescending();

	private static List<Equipment> TempBestItems = new List<Equipment>();

	public const float HighContainerScore = 1000000f;

	public static float MinNutritionToEat = 0.11f * Sun.DayLengthSecs;

	public static float MinNutritionPerFlOzToEat = 0.005f;

	public static float MinTastinessToEatUnlessReallyHungry = -50f;

	private static List<Equippable> Equippables = new List<Equippable>();

	private static List<DrainCandidate> DrainCandidates = new List<DrainCandidate>();

	private static List<EquipmentScore> WeaponScores = new List<EquipmentScore>();

	private static SortEquipmentByScoreDescending WeaponSorter = new SortEquipmentByScoreDescending();

	private static GameProfiler GetBestWeaponForShortcutTimer = new GameProfiler("GetBestWeaponForShortcut");

	private static List<EquipmentPrototype> TempAmmoTypes = new List<EquipmentPrototype>();

	public int Count => Contents.Count;

	public void CopyToList(List<Equipment> results, bool stripItemsThatCantTravel)
	{
		CustomBinaryWriterToMemory customBinaryWriterToMemory = new CustomBinaryWriterToMemory();
		CustomBinaryReaderFromMemory customBinaryReaderFromMemory = new CustomBinaryReaderFromMemory(customBinaryWriterToMemory._buffer, customBinaryWriterToMemory._buffer.Length);
		foreach (Equipment content in Contents)
		{
			if (!stripItemsThatCantTravel || content.GetPrototype().CanHitTheRoad)
			{
				Equipment equipment = Equipment.CreateCopy(content);
				customBinaryWriterToMemory.ResetIndex();
				content.Reflect(customBinaryWriterToMemory);
				customBinaryReaderFromMemory.SetBuffer(customBinaryWriterToMemory._buffer, customBinaryWriterToMemory._index);
				equipment.Reflect(customBinaryReaderFromMemory);
				results.Add(equipment);
			}
		}
	}

	public void DeleteAll(TileObject carrier, bool carrierBeingDeleted)
	{
		if (carrierBeingDeleted)
		{
			foreach (Equipment content in Contents)
			{
				content.SetInventoryOwner(null);
				content.Delete();
			}
			Contents.Clear();
		}
		else
		{
			while (Contents.Count > 0)
			{
				Take(carrier, Contents[0], Contents[0].GetAmount()).Delete();
			}
		}
	}

	public void ClearGatheredItems()
	{
		foreach (Equipment content in Contents)
		{
			content.ClearGathered();
		}
	}

	public bool HasAnyGatheredItems()
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetGatheredAmount() > 0)
			{
				return true;
			}
		}
		return false;
	}

	public float GetWeightIncludingWornItems()
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			num += (float)content.GetAmount() * content.GetWeight();
		}
		return num;
	}

	public float GetWeight(TileObject carrier)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (!content.IsWornOrRemovedForSparring(carrier))
			{
				num += (float)content.GetAmount() * content.GetWeight();
			}
		}
		return num;
	}

	public float GetWeightIgnoringItem(TileObject carrier, Equipment ignore)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (!content.IsWornOrRemovedForSparring(carrier) && content != ignore)
			{
				num += (float)content.GetAmount() * content.GetWeight();
			}
		}
		return num;
	}

	public Equipment FindLightestTakeableItem(TileObject carrier)
	{
		Equipment result = null;
		float num = float.MaxValue;
		foreach (Equipment content in Contents)
		{
			if (content.IsIncludedInTakeAll(carrier) && carrier.CanTransferEquipmentAway(content) == CantTransferReason.CanTransfer && content.GetWeight() < num)
			{
				result = content;
				num = content.GetWeight();
			}
		}
		return result;
	}

	public bool HasAnyArmorThatNeedsRepairing()
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetBaseObjectType() == BaseObjectType.Armor && ((Armor)content).Protection <= 0.75f)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanBeDestroyed()
	{
		foreach (Equipment content in Contents)
		{
			if (!content.CanBeDestroyed())
			{
				return false;
			}
		}
		return true;
	}

	public bool Contains(Equipment item)
	{
		return Contents.Contains(item);
	}

	public List<Equipment> GetEquipmentSortedByPriceDescending()
	{
		List<Equipment> list = new List<Equipment>();
		foreach (Equipment content in Contents)
		{
			list.Add(content);
		}
		list.InsertionSort(PriceSorter);
		return list;
	}

	public int GetAmmoCountOfType(EquipmentPrototype ammoType, InfectionType infectedWith, Equipment ignore = null)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content != ignore)
			{
				num += content.GetAmmoCountOfType(ammoType, infectedWith);
			}
		}
		return num;
	}

	public bool HasAmmoOfType(EquipmentPrototype ammoType, InfectionType infectedWith, Equipment ignore = null, Character checkIfCanBeUsedBy = null)
	{
		foreach (Equipment content in Contents)
		{
			if (content != ignore && content.HasAmmoOfType(ammoType, infectedWith) && (checkIfCanBeUsedBy == null || checkIfCanBeUsedBy.IsActionAllowedForItem(ammoType, null, content.InfectedWith, EquipmentPolicyAction.CanUse)))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAmmoOfType(EquipmentPrototype ammoType, out InfectionType infectedWith, Equipment ignore = null, Character checkIfCanBeUsedBy = null)
	{
		bool result = false;
		infectedWith = InfectionType.None;
		foreach (Equipment content in Contents)
		{
			if (content != ignore && content.HasAmmoOfType(ammoType) && (checkIfCanBeUsedBy == null || checkIfCanBeUsedBy.IsActionAllowedForItem(ammoType, null, content.InfectedWith, EquipmentPolicyAction.CanUse)))
			{
				infectedWith = (InfectionType)Math.Max((int)infectedWith, (int)content.InfectedWith);
				result = true;
			}
		}
		return result;
	}

	public int GetAmmoCountForWeaponNotIncludingLoadedAmmo(AmmoWeapon weapon)
	{
		int num = 0;
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				num += CountItemsOfType(item);
			}
		}
		return num;
	}

	public bool HasAmmoForWeapon(AmmoWeapon weapon, Equipment ignore = null, Character checkIfCharacterAllowedToUseIt = null)
	{
		foreach (Equipment content in Contents)
		{
			if (content != ignore && content.HasAmmoForWeapon(weapon, checkIfCharacterAllowedToUseIt))
			{
				return true;
			}
		}
		return false;
	}

	public AmmoWeapon GetWeaponForAmmoType(EquipmentPrototype ammoType)
	{
		if (ammoType.AmmoForWeaponPrototypes == null)
		{
			return null;
		}
		foreach (Equipment content in Contents)
		{
			if (content is AmmoWeapon result && ammoType.AmmoForWeaponPrototypes.Contains(content.GetPrototype()))
			{
				return result;
			}
		}
		return null;
	}

	public float GetAmountOfLiquidType(LiquidPrototype liquidType)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquidType)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public float GetAmountOfLiquidType(LiquidPrototype liquidType, InfectionType infectionType)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquidType && content.InfectedWith == infectionType)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public float GetAmountOfLiquidType(TileObject carrier, LiquidPrototype liquidType, out InfectionType ingredientsInfectedWith, IngredientInfectionState ingredientInfectionState, Character checkIfCanBeCraftedBy = null)
	{
		ingredientsInfectedWith = InfectionType.None;
		float num = 0f;
		if (checkIfCanBeCraftedBy != null)
		{
			for (InfectionType infectionType = InfectionType.None; infectionType < InfectionType.Count; infectionType++)
			{
				switch (ingredientInfectionState)
				{
				case IngredientInfectionState.MustBeInfected:
					if (infectionType == InfectionType.None)
					{
						continue;
					}
					goto default;
				case IngredientInfectionState.CantBeInfected:
					if (infectionType != InfectionType.None)
					{
						continue;
					}
					goto default;
				default:
				{
					if (!checkIfCanBeCraftedBy.IsActionAllowedForItem(null, liquidType, infectionType, EquipmentPolicyAction.CanCraftWith))
					{
						continue;
					}
					float num2 = 0f;
					bool flag = false;
					foreach (Equipment content in Contents)
					{
						if (content.GetLiquidContentsType() == liquidType)
						{
							if (content.InfectedWith == infectionType)
							{
								num2 += content.GetLiquidContentsAmount();
							}
							else
							{
								flag = true;
							}
						}
					}
					if (num2 > 0f)
					{
						float num3 = ((carrier is Character character) ? character.GetTargetAmountToCarryIncludingAmmo(null, liquidType, infectionType) : 0f);
						num2 -= num3;
					}
					if (num2 > 0f)
					{
						num += num2;
						ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)infectionType);
					}
					if (flag)
					{
						continue;
					}
					break;
				}
				}
				break;
			}
		}
		else
		{
			foreach (Equipment content2 in Contents)
			{
				if (content2.GetLiquidContentsType() == liquidType && content2.MatchesIngredientInfectionState(ingredientInfectionState))
				{
					num += content2.GetLiquidContentsAmount();
					ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)content2.InfectedWith);
				}
			}
		}
		return num;
	}

	public Equipment FindBestItemWithLiquid(LiquidPrototype liquidType)
	{
		Equipment equipment = null;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquidType && (equipment == null || equipment.GetLiquidContentsAmount() < content.GetLiquidContentsAmount()))
			{
				equipment = content;
			}
		}
		return equipment;
	}

	public Equipment FindBestItemWithLiquid(LiquidPrototype liquidType, InfectionType infectionType)
	{
		Equipment equipment = null;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquidType && content.InfectedWith == infectionType && (equipment == null || equipment.GetLiquidContentsAmount() < content.GetLiquidContentsAmount()))
			{
				equipment = content;
			}
		}
		return equipment;
	}

	public Equipment FindItemWithHighestBandageLevel()
	{
		return FindItemWithHighestBandageLevel(0);
	}

	public Equipment FindItemWithHighestBandageLevel(int requiredLevel)
	{
		Equipment result = null;
		int num = requiredLevel;
		foreach (Equipment content in Contents)
		{
			if (content.GetBandageLevel() >= num)
			{
				result = content;
				num = content.GetBandageLevel();
			}
		}
		return result;
	}

	public int FindHighestBandageLevel()
	{
		return FindItemWithHighestBandageLevel()?.GetBandageLevel() ?? (-1);
	}

	public FertilizedEgg FindFertilizedEgg(Chicken mother)
	{
		foreach (Equipment content in Contents)
		{
			if (content is FertilizedEgg fertilizedEgg && fertilizedEgg.Mother == mother)
			{
				return fertilizedEgg;
			}
		}
		return null;
	}

	public Equipment GetBestClothingForCharacter(TileObject carrier, Character character, bool critical, List<Equipment> ignore)
	{
		TempBestItems.Clear();
		float num = character.CalcAverageTemperatureInsulationDelta();
		float num2 = 0f;
		foreach (Equipment content in Contents)
		{
			ClothingType clothingType = content.GetClothingType();
			if (clothingType == ClothingType.Invalid || clothingType == ClothingType.Backpack || (ignore != null && ignore.Contains(content)) || (content.IsWornOrRemovedForSparring(carrier) && (character == carrier || !critical || ((Character)carrier).Alive)))
			{
				continue;
			}
			float num3 = content.GetInsulation();
			float num4 = -1f;
			Equipment equipment = character.Clothes[(int)clothingType];
			if (equipment == null)
			{
				if ((uint)(clothingType - 2) <= 2u)
				{
					num4 += 100f;
				}
			}
			else if (character.Community != null && character.Community.CommunityType == CommunityType.Player)
			{
				if (!character.IsActionAllowedForItem(equipment, EquipmentPolicyAction.CanStrip))
				{
					continue;
				}
			}
			else if (equipment is Armor)
			{
				continue;
			}
			if (num3 > 0f)
			{
				float num5 = ((equipment != null) ? ((float)equipment.GetInsulation()) : 0f);
				if (num3 > num5 && num < 0f)
				{
					float num6 = (num3 - num5) * Character.ClothingInsulationTemperature;
					num4 += num6;
					if (num6 > 0f - num)
					{
						num4 -= num6 + num;
					}
				}
				else if (num3 < num5 && num > 0f)
				{
					float num7 = (num5 - num3) * Character.ClothingInsulationTemperature;
					num4 += num7;
					if (num7 > num)
					{
						num4 -= num7 - num;
					}
				}
			}
			if (num4 == num2)
			{
				TempBestItems.Add(content);
			}
			else if (num4 > num2)
			{
				num2 = num4;
				TempBestItems.Clear();
				TempBestItems.Add(content);
			}
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (TempBestItems.Count <= 0)
		{
			return null;
		}
		return TempBestItems[MathUtil.RandomInt(character.Id + currentTime.Milliseconds, TempBestItems.Count)];
	}

	public Equipment GetBestWateringCan()
	{
		return GetBestWateringCan(mustHaveWater: false);
	}

	public Equipment GetBestWateringCan(bool mustHaveWater)
	{
		Equipment result = null;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (!(content.GetLiquidCapacity() > 0f) || (content.GetLiquidContentsType() != null && content.GetLiquidContentsType() != LiquidPrototype.Water))
			{
				continue;
			}
			if (mustHaveWater)
			{
				if (content.GetLiquidContentsAmount() == 0f)
				{
					continue;
				}
			}
			else if (content.DesignatedLiquid != null && content.DesignatedLiquid != LiquidPrototype.Water)
			{
				continue;
			}
			float num2 = content.GetLiquidCapacity() + content.GetLiquidContentsAmount() * 0.01f;
			if (content is WateringCan)
			{
				num2 += 1000f;
			}
			if (num2 > num)
			{
				result = content;
				num = num2;
			}
		}
		return result;
	}

	public Equipment GetBestWateringToPutOutFire(float waterNeeded)
	{
		Equipment result = null;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidCapacity() > 0f && (content.GetLiquidContentsType() == null || content.GetLiquidContentsType() == LiquidPrototype.Water) && !(content.GetLiquidContentsAmount() < waterNeeded))
			{
				float num2 = content.GetLiquidCapacity() + content.GetLiquidContentsAmount() * 0.01f;
				if (num2 > num)
				{
					result = content;
					num = num2;
				}
			}
		}
		return result;
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid)
	{
		float bestScore = 0f;
		return GetBestLiquidContainerToFill(liquid, null, 0f, out bestScore);
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid, float neededAmount)
	{
		float bestScore = 0f;
		return GetBestLiquidContainerToFill(liquid, null, neededAmount, out bestScore);
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid, Equipment existingContainer, float neededAmount)
	{
		float bestScore = 0f;
		return GetBestLiquidContainerToFill(liquid, null, neededAmount, out bestScore);
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid, out float score)
	{
		return GetBestLiquidContainerToFill(liquid, null, 0f, out score);
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid, float neededAmount, out float score)
	{
		return GetBestLiquidContainerToFill(liquid, null, neededAmount, out score);
	}

	public Equipment GetBestLiquidContainerToFill(LiquidPrototype liquid, Equipment existingContainer, float neededAmount, out float bestScore)
	{
		Equipment result = null;
		bestScore = float.MinValue;
		foreach (Equipment content in Contents)
		{
			if (!(content.GetLiquidCapacity() > 0f) || content == existingContainer || (content.DesignatedLiquid != null && content.DesignatedLiquid != liquid) || (content.GetLiquidContentsType() != null && content.GetLiquidContentsType() != liquid) || content.GetLiquidContentsAmount() >= content.GetLiquidCapacity() - 0.001f || (content is WateringCan && liquid != LiquidPrototype.Water) || content.GetPrototype() == EquipmentPrototype.Pot || (liquid != null && !liquid.CanPourIntoBottles && content.IsBottle()))
			{
				continue;
			}
			float num = content.GetLiquidContentsAmount();
			if (liquid != null && liquid.CanPourIntoBottles && !content.IsBottle() && content.GetLiquidContentsAmount() == 0f)
			{
				if (existingContainer != null && existingContainer.IsBottle())
				{
					continue;
				}
				num += -1000f;
			}
			if (content is WateringCan)
			{
				num += -100000f;
			}
			if (content.GetPrototype().DefaultLiquidPrototype != liquid)
			{
				num += -0.01f;
			}
			if (neededAmount > content.GetLiquidCapacity())
			{
				num += -10000f;
			}
			if (content.DesignatedLiquid == liquid)
			{
				num += 1000000f;
			}
			if ((existingContainer == null || existingContainer.GetPrototype().DefaultLiquidPrototype != liquid || content.GetPrototype().DefaultLiquidPrototype == liquid) && num > bestScore)
			{
				result = content;
				bestScore = num;
			}
		}
		return result;
	}

	public static float CalcNormalizedContainerScore(float score)
	{
		return Mathf.Sign(score) * Mathf.Pow(Mathf.Abs(score) / 1000000f, 0.1f) * 64f / Character.WalkSpeed;
	}

	public Equipment GetBestCookingPot()
	{
		Equipment result = null;
		float num = float.MaxValue;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == EquipmentPrototype.Pot && content.GetLiquidCapacity() > 0f)
			{
				float liquidContentsPrice = content.GetLiquidContentsPrice();
				if (liquidContentsPrice < num)
				{
					result = content;
					num = liquidContentsPrice;
				}
			}
		}
		return result;
	}

	public Equipment GetFryingPan()
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == EquipmentPrototype.FryingPan)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment GetBestWaterBottleToDrink(TileObject owner, Character drinker, bool forSharing)
	{
		bool flag = drinker.GetThirst() >= Character.ThirstCriticalTime;
		Equipment result = null;
		float num = float.MinValue;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == null || !(content.GetLiquidContentsType().WaterContent > 0f))
			{
				continue;
			}
			float num2 = drinker.CalcTastiness(content.GetLiquidContentsType());
			if (num2 < -50f && drinker.GetThirst() < Character.ThirstDieTime - Sun.DayLengthSecs * 0.1f)
			{
				continue;
			}
			if (!(content is WateringCan))
			{
				num2 += 1000f;
			}
			else if (!flag)
			{
				continue;
			}
			if (content.InfectedWith != InfectionType.None && !content.WasGifted() && drinker.IsControllableByPlayer())
			{
				continue;
			}
			if (forSharing)
			{
				if (owner is Character character)
				{
					if (character.IsInSameCommunity(drinker))
					{
						if (!drinker.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse))
						{
							continue;
						}
					}
					else if (!character.IsActionAllowedForItem(content, EquipmentPolicyAction.CanShare))
					{
						continue;
					}
				}
			}
			else if (!drinker.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse))
			{
				continue;
			}
			if (num2 > num)
			{
				result = content;
				num = num2;
			}
		}
		return result;
	}

	public Equipment GetBestAlcoholToDrink(TileObject owner, Character drinker, bool ignorePolicy)
	{
		Equipment result = null;
		float num = float.MinValue;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() != null && content.GetLiquidContentsType().AlcoholContent > 0f)
			{
				float num2 = drinker.CalcTastiness(content.GetLiquidContentsType());
				if (!(content is WateringCan))
				{
					num2 += 1000f;
				}
				if ((content.InfectedWith == InfectionType.None || content.WasGifted() || !drinker.IsControllableByPlayer()) && (ignorePolicy || drinker.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse)) && num2 > num)
				{
					result = content;
					num = num2;
				}
			}
		}
		return result;
	}

	public float GetDrinkableAlcoholAmount(TileObject owner, Character drinker, bool ignorePolicy)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() != null && content.GetLiquidContentsType().AlcoholContent > 0f && (content.InfectedWith == InfectionType.None || content.WasGifted() || !drinker.IsControllableByPlayer()) && (ignorePolicy || drinker.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse)))
			{
				num += content.GetLiquidContentsType().AlcoholContent / 100f * content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public Equipment GetBestWaterBottle(bool ignoreGathered = false)
	{
		Equipment result = null;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidCapacity() > 0f && (content.GetLiquidContentsType() == null || content.GetLiquidContentsType() == LiquidPrototype.Water) && (content.DesignatedLiquid == LiquidPrototype.Water || content.DesignatedLiquid == null || content.GetLiquidContentsType() == LiquidPrototype.Water) && (!ignoreGathered || content.GetGatheredAmount() == 0))
			{
				float num2 = content.GetLiquidContentsAmount();
				if (!(content is WateringCan))
				{
					num2 += 1000f;
				}
				if (content.GetLiquidContentsType() == LiquidPrototype.Water)
				{
					num2 += 100f;
				}
				if (content.GetPrototype().DefaultLiquidPrototype == LiquidPrototype.Water)
				{
					num2 += 100f;
				}
				if (content.DesignatedLiquid == LiquidPrototype.Water)
				{
					num2 += 100f;
				}
				if (num2 > num)
				{
					result = content;
					num = num2;
				}
			}
		}
		return result;
	}

	public Equipment GetBestSeeds()
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetSeedForPlantType() != null)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment GetSeedsForCropType(TileObject carrier, Character planter, PropPrototype cropType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetSeedForPlantType() == cropType && (carrier.GetCommunityId() == planter.GetCommunityId() || !(carrier is Character { Alive: not false } character) || character.IsActionAllowedForItem(content, EquipmentPolicyAction.CanShare)) && planter.IsActionAllowedForItem(content, EquipmentPolicyAction.CanPlant))
			{
				return content;
			}
		}
		return null;
	}

	public Equipment GetLightestInedibleSeeds()
	{
		float num = float.MaxValue;
		Equipment result = null;
		foreach (Equipment content in Contents)
		{
			if (content.GetSeedForPlantType() != null && content.GetNutrition() == 0f && content.GetWeight() < num)
			{
				num = content.GetWeight();
				result = content;
			}
		}
		return result;
	}

	public Equipment GetFood(TileObject owner, Character eater, bool includeGifts, bool ignoreIfUsingForCrafting, bool forSharing)
	{
		float bestScore;
		return GetFood(owner, eater, includeGifts, ignoreIfUsingForCrafting, forSharing, out bestScore);
	}

	public Equipment GetFood(TileObject owner, Character eater, bool includeGifts, bool ignoreIfUsingForCrafting, bool forSharing, out float bestScore)
	{
		Equipment result = null;
		bestScore = float.MinValue;
		foreach (Equipment content in Contents)
		{
			float num = float.MinValue;
			EquipmentPrototype prototype = content.GetPrototype();
			if (prototype.GetNutrition() > 0f)
			{
				num = eater.CalcTastiness(prototype);
				if (!prototype.CanBeDestroyed)
				{
					continue;
				}
				if (prototype.GetNutrition() <= MinNutritionToEat)
				{
					num -= 50f;
				}
				if (prototype.ContainsHumanMeat)
				{
					if (eater.HasPersonality(CachedPersonalityType.Moral))
					{
						num -= 100f;
					}
					else if (eater.HasPersonality(CachedPersonalityType.Immoral))
					{
						num += 20f;
					}
				}
			}
			if (content.GetLiquidContentsType() != null && content.GetLiquidContentsType().Edible)
			{
				if (content.GetLiquidContentsType().NutritionPerFlOz < MinNutritionPerFlOzToEat)
				{
					continue;
				}
				num = eater.CalcTastiness(content.GetLiquidContentsType());
			}
			if (!(num > bestScore) || (!includeGifts && content.Gifted) || (num < MinTastinessToEatUnlessReallyHungry && eater.GetHunger() < Character.HungerDieTime - Sun.DayLengthSecs) || (prototype.ContainsHumanMeat && eater.ShouldRefuseToEatHumanMeat()) || (eater.IsControllableByPlayer() && !eater.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse)))
			{
				continue;
			}
			if (forSharing)
			{
				if (owner is Character character)
				{
					if ((prototype.ContainsHumanMeat && character.ShouldRefuseToEatHumanMeat()) || (!character.IsInSameCommunity(eater) && !character.IsActionAllowedForItem(content, EquipmentPolicyAction.CanShare)))
					{
						continue;
					}
				}
				else if (!eater.IsInSameCommunity(owner))
				{
					Community community = owner.GetCommunity();
					if (community != null && !community.IsActionAllowedForItem(content, EquipmentPolicyAction.CanShare))
					{
						continue;
					}
				}
			}
			if ((!ignoreIfUsingForCrafting || !owner.IsUsingForCrafting(content, willTransferContainers: false, FindType.Food, out var _)) && (content.GetSeedForPlantType() == null || eater.Community == null || eater.Community != owner.GetCommunity() || eater.Community.IsAllowedToEatCrops(content.GetSeedForPlantType())) && (content.InfectedWith == InfectionType.None || content.WasGifted() || !eater.IsControllableByPlayer()))
			{
				result = content;
				bestScore = num;
			}
		}
		return result;
	}

	public Equipment GetFoodForAnimal(BaseObjectType baseObjectType, Character checkPolicyFor)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype().FoodForAnimal != null && content.GetPrototype().FoodForAnimal.Contains(baseObjectType) && (checkPolicyFor == null || checkPolicyFor.IsActionAllowedForItem(content, EquipmentPolicyAction.CanFeedToAnimals)))
			{
				return content;
			}
		}
		return null;
	}

	public Equipment GetAntigen(InfectionType infectionType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetAntigenType() == infectionType)
			{
				return content;
			}
		}
		return null;
	}

	public bool HasAntigenForInjuries(Character character)
	{
		for (int i = 0; i < character.Injuries.Count; i++)
		{
			if (character.Injuries[i].InfectionType != InfectionType.None && GetAntigen(character.Injuries[i].InfectionType) != null)
			{
				return true;
			}
		}
		return false;
	}

	public int CountFoodItems()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.IsEdible())
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public int CountSeeds()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetSeedForPlantType() != null)
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public float GetTotalNutrition()
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			num += content.GetNutrition() * (float)content.GetAmount();
		}
		return num;
	}

	public bool HasAnythingToShowOnSellingFoodScreen(Character carrierCharacter)
	{
		foreach (Equipment content in Contents)
		{
			if (InventoryBehaviour.CanShowItemOnSellingFoodScreen(carrierCharacter, content))
			{
				return true;
			}
		}
		return false;
	}

	public float GetTotalHarvestedNutrition()
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetHarvestedFromPlantProto() != null)
			{
				num += content.GetNutrition() * (float)content.GetAmount();
			}
		}
		return num;
	}

	public float GetTotalNutritionThatImAllowedToEat(Character character)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			float nutrition = content.GetNutrition();
			if (nutrition > 0f && character.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse))
			{
				num += nutrition * (float)content.GetAmount();
			}
		}
		return num;
	}

	public float GetTotalWater()
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == LiquidPrototype.Water)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public float GetTotalLiquid(LiquidPrototype liquid)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquid)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public float GetTotalLiquid(LiquidPrototype liquid, InfectionType infectionType)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquid && content.InfectedWith == infectionType)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public float GetTotalGatheredLiquid(LiquidPrototype liquid, InfectionType infectionType)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquid && content.InfectedWith == infectionType && content.GetGatheredAmount() > 0)
			{
				num += content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public int CountAllItems()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			num += content.GetAmount();
		}
		return num;
	}

	public int CountBackpacks()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == ClothingType.Backpack)
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public void AddPrototypeCounters()
	{
		foreach (Equipment content in Contents)
		{
			content.AddPrototypeCounters();
		}
	}

	public int CountItemsOfType(EquipmentPrototype prototype)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype)
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public int CountItemsOfType(EquipmentPrototype prototype, InfectionType infectionType)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.InfectedWith == infectionType)
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public int CountGatheredItemsOfType(EquipmentPrototype prototype, InfectionType infectionType, Equipment ignore)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.InfectedWith == infectionType && content != ignore)
			{
				num += content.GetGatheredAmount();
			}
		}
		return num;
	}

	public int CountTradedItemsOfType(EquipmentPrototype prototype)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype)
			{
				num += content.GetTradedAmount();
			}
		}
		return num;
	}

	public int CountGiftedItemsOfType(EquipmentPrototype prototype)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.WasGifted())
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public int CountItemsOfClass(Type type)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (type.IsInstanceOfType(content))
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public int CountItemsOfClothingType(ClothingType clothingType)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == clothingType)
			{
				num += content.GetAmount();
			}
		}
		return num;
	}

	public Equipment FindAntigenForInfectionType(InfectionType infectionType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetAntigenType() == infectionType)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment FindItemOfType(EquipmentPrototype prototype)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment FindItemOfType(EquipmentPrototype prototype, InfectionType infectionType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.InfectedWith == infectionType)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment FindGatheredItemOfType(EquipmentPrototype prototype)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.GetGatheredAmount() > 0)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment FindAmmoForWeapon(AmmoWeapon weapon)
	{
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				Equipment equipment = FindItemOfType(item);
				if (equipment != null)
				{
					return equipment;
				}
			}
		}
		return null;
	}

	public Equipment FindItemWithLiquid(LiquidPrototype liquid, InfectionType infectionType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquid && content.InfectedWith == infectionType)
			{
				return content;
			}
		}
		return null;
	}

	public Equipment FindFullestItemOfTypeWithLiquid(TileObject carrier, EquipmentPrototype prototype, LiquidPrototype liquid, InfectionType infectionType, bool includeGifts = false)
	{
		int count;
		return FindFullestItemOfTypeWithLiquid(carrier, prototype, liquid, infectionType, out count, includeGifts);
	}

	public Equipment FindFullestItemOfTypeWithLiquid(TileObject carrier, EquipmentPrototype prototype, LiquidPrototype liquid, InfectionType infectionType, out int count, bool includeGifts = false)
	{
		count = 0;
		float num = float.MinValue;
		Equipment result = null;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.GetLiquidContentsType() == liquid && content.InfectedWith == infectionType && (includeGifts || !content.WasGifted()) && !content.IsWornOrRemovedForSparring(carrier))
			{
				count += content.GetAmount();
				if (content.GetLiquidContentsAmount() > num)
				{
					result = content;
					num = content.GetLiquidContentsAmount();
				}
			}
		}
		return result;
	}

	public Equipment FindItemOfTypePreferringUnused(Character user, TileObject carrier, EquipmentPrototype prototype, Recipe crafting, bool checkCraftingPolicy, IngredientInfectionState ingredientInfectionState = IngredientInfectionState.Any)
	{
		Equipment result = null;
		float num = float.MaxValue;
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype() == prototype && content.MatchesIngredientInfectionState(ingredientInfectionState) && (crafting == null || user.CanUseItemForCrafting(carrier, content, null, crafting)) && (!checkCraftingPolicy || user.IsActionAllowedForItem(content, EquipmentPolicyAction.CanCraftWith)))
			{
				float num2 = 0f;
				if (content.IsWornOrRemovedForSparring(carrier))
				{
					num2 += 1000f;
				}
				if (content.GetLiquidContentsType() != null)
				{
					num2 += content.GetLiquidContentsAmount() * content.GetLiquidContentsType().BasePricePerFlOz;
				}
				num2 -= (float)content.InfectedWith * 1000000f;
				if (num2 < num)
				{
					result = content;
					num = num2;
				}
			}
		}
		return result;
	}

	public Equipment FindItemOfClass(Type type, int stopAt)
	{
		foreach (Equipment content in Contents)
		{
			if (type.IsInstanceOfType(content))
			{
				if (stopAt == 0)
				{
					return content;
				}
				stopAt--;
			}
		}
		return null;
	}

	public Equipment FindItemOfClass(Type type)
	{
		foreach (Equipment content in Contents)
		{
			if (type.IsInstanceOfType(content))
			{
				return content;
			}
		}
		return null;
	}

	public int GetWorldMapQuadrants()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content is WorldMap worldMap)
			{
				num |= worldMap.MapQuadrant;
			}
		}
		return num;
	}

	public int GetGeologicalMapQuadrants()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content is GeologicalMap geologicalMap)
			{
				num |= geologicalMap.MapQuadrant;
			}
		}
		return num;
	}

	public Armor FindArmor(ClothingType clothingType, int stopAt)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == clothingType && content is Armor result)
			{
				if (stopAt == 0)
				{
					return result;
				}
				stopAt--;
			}
		}
		return null;
	}

	public Equipment FindContainerOfLiquidType(LiquidPrototype liquidType)
	{
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == liquidType)
			{
				return content;
			}
		}
		return null;
	}

	public float GetUnusedCapacityForLiquid(LiquidPrototype liquidType)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidCapacity() > 0f && (content.GetLiquidContentsType() == liquidType || content.GetLiquidContentsType() == null))
			{
				num += content.GetLiquidCapacity() - content.GetLiquidContentsAmount();
			}
		}
		return num;
	}

	public int CountContainersWithLiquidOrEmpty(LiquidPrototype liquidType)
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidCapacity() > 0f && (content.GetLiquidContentsType() == liquidType || content.GetLiquidContentsType() == null))
			{
				num++;
			}
		}
		return num;
	}

	public Equipment GetBestMaterialToAddToFire(TileObject carrier, Character characterToCarryIt)
	{
		Equipment equipment = null;
		bool flag = false;
		float num = float.MaxValue;
		foreach (Equipment content in Contents)
		{
			if (GameCursor.CanAddMaterialToFire(content.GetPrototype()) && (characterToCarryIt == null || characterToCarryIt.IsActionAllowedForItem(content, EquipmentPolicyAction.CanUse)))
			{
				bool flag2 = characterToCarryIt != null && characterToCarryIt != carrier && !characterToCarryIt.HasInventorySpaceFor(content.GetWeight());
				if (equipment == null || ((flag2 == flag) ? (content.GetBasePrice() < num) : flag))
				{
					equipment = content;
					num = content.GetBasePrice();
					flag = flag2;
				}
			}
		}
		return equipment;
	}

	public Equipment FindIngredient(Character crafter, TileObject carrier, Ingredient ingredient, Recipe recipe, Equipment usingItem, Character ignoreToolOwnedByCharacter, Character checkEquipmentPolicyForCharacter)
	{
		foreach (Equipment content in Contents)
		{
			if (!ingredient.MatchesItem(content, recipe) || (ignoreToolOwnedByCharacter != null && recipe.IsRecipeTool(ignoreToolOwnedByCharacter, content)) || !crafter.CanUseItemForCrafting(carrier, content, usingItem, recipe))
			{
				continue;
			}
			if (checkEquipmentPolicyForCharacter != null)
			{
				if (!content.CanBeCombined())
				{
					if (content.GetLiquidContentsType() != null)
					{
						if (carrier.GetInventory().FindItemWithLiquid(content.GetLiquidContentsType(), content.InfectedWith) != content)
						{
							continue;
						}
					}
					else if (carrier.GetInventory().FindItemOfType(content.GetPrototype(), content.InfectedWith) != content)
					{
						continue;
					}
				}
				if (!checkEquipmentPolicyForCharacter.IsActionAllowedForItem(content, EquipmentPolicyAction.CanCraftWith))
				{
					continue;
				}
				int num = ((carrier is Character character) ? ((int)character.GetTargetAmountToCarryIncludingAmmo(content.GetPrototype(), content.GetLiquidContentsType(), content.InfectedWith)) : 0);
				if (content.GetLiquidContentsType() != null)
				{
					if (carrier.GetInventory().GetAmountOfLiquidType(content.GetLiquidContentsType(), content.InfectedWith) <= (float)num)
					{
						continue;
					}
				}
				else if ((content.CanBeCombined() ? content.GetAmount() : carrier.GetInventory().CountItemsOfType(content.GetPrototype(), content.InfectedWith)) <= num)
				{
					continue;
				}
			}
			if (content.MatchesIngredientInfectionState(ingredient.IngredientInfectionState))
			{
				return content;
			}
		}
		return null;
	}

	public float GetIngredientAmount(Character crafter, TileObject carrier, Ingredient ingredient, Recipe recipe, Equipment usingItem, Character ignoreToolOwnedByCharacter, Character checkEquipmentPolicyForCharacter)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (!ingredient.MatchesItem(content, recipe) || !crafter.CanUseItemForCrafting(carrier, content, usingItem, recipe) || (ignoreToolOwnedByCharacter != null && recipe.IsRecipeTool(ignoreToolOwnedByCharacter, content)))
			{
				continue;
			}
			int num2 = content.GetAmount();
			float num3 = content.GetLiquidContentsAmount();
			if (checkEquipmentPolicyForCharacter != null)
			{
				if (!content.CanBeCombined())
				{
					if (content.GetLiquidContentsType() != null)
					{
						if (carrier.GetInventory().FindItemWithLiquid(content.GetLiquidContentsType(), content.InfectedWith) != content)
						{
							continue;
						}
					}
					else if (carrier.GetInventory().FindItemOfType(content.GetPrototype(), content.InfectedWith) != content)
					{
						continue;
					}
					if (content.GetLiquidContentsType() != null)
					{
						num3 = carrier.GetInventory().GetAmountOfLiquidType(content.GetLiquidContentsType(), content.InfectedWith);
					}
					else
					{
						num2 = carrier.GetInventory().CountItemsOfType(content.GetPrototype(), content.InfectedWith);
					}
				}
				if (!checkEquipmentPolicyForCharacter.IsActionAllowedForItem(content, EquipmentPolicyAction.CanCraftWith))
				{
					continue;
				}
				int num4 = ((carrier is Character character) ? ((int)character.GetTargetAmountToCarryIncludingAmmo(content.GetPrototype(), content.GetLiquidContentsType(), content.InfectedWith)) : 0);
				if (content.GetLiquidContentsType() != null)
				{
					num3 -= (float)num4;
					if (num3 <= 0f)
					{
						continue;
					}
				}
				else
				{
					num2 -= num4;
					if (num2 <= 0)
					{
						continue;
					}
				}
			}
			if (ingredient.Prototypes != null)
			{
				num += (float)num2;
			}
			if (ingredient.LiquidTypes != null)
			{
				num += num3;
			}
		}
		return num;
	}

	public List<Equippable> BuildEquippableList(Character character, Equipment selected, bool wantLockOn, bool takeAll, out int selectedIndex)
	{
		selectedIndex = -1;
		InventoryBehaviour.Temp.Clear();
		Equippables.Clear();
		if (selected == null)
		{
			selectedIndex = Equippables.Count;
		}
		if (!takeAll)
		{
			Equippables.Add(default(Equippable));
		}
		for (int i = 0; i < Contents.Count; i++)
		{
			Equipment equipment = Contents[i];
			if ((takeAll || (equipment.CanBeEquipped() && (!equipment.WasGifted() || !(equipment is Throwable)))) && (!wantLockOn || (equipment.AllowLockOn(character) && (!(equipment is GlassBottle) || !(equipment.GetLiquidContentsAmount() > 0f)))))
			{
				InventoryBehaviour.Temp.Add(new InventoryBehaviour.EquipmentScore(equipment, 0, transferred: false, equipment.GetAmount(), i));
			}
		}
		InventoryBehaviour.SortInventory(InventoryBehaviour.Temp);
		for (int j = 0; j < InventoryBehaviour.Temp.Count; j++)
		{
			Equipment item = InventoryBehaviour.Temp[j].Item;
			int amount = item.GetAmount();
			if (!item.CanBeCombined())
			{
				bool flag = false;
				int num = 0;
				for (int k = 0; k < InventoryBehaviour.Temp.Count; k++)
				{
					Equipment item2 = InventoryBehaviour.Temp[k].Item;
					if (item2.GetPrototype() == item.GetPrototype() && item2.GetLiquidContentsType() == item.GetLiquidContentsType() && (item2.InfectedWith == item.InfectedWith || item is AmmoWeapon) && (item2.DesignatedLiquid == item.DesignatedLiquid || item.GetLiquidContentsType() != null))
					{
						if (k < j)
						{
							flag = true;
							break;
						}
						num++;
					}
				}
				if (flag)
				{
					continue;
				}
				amount = num;
			}
			if (selected == item)
			{
				selectedIndex = Equippables.Count;
			}
			if (item is AmmoWeapon ammoWeapon && character != null)
			{
				bool flag2 = false;
				List<EquipmentPrototype> ammoTypes = ammoWeapon.GetAmmoTypes();
				if (ammoTypes != null)
				{
					foreach (EquipmentPrototype item6 in ammoTypes)
					{
						int count = Equippables.Count;
						foreach (Equipment content in character.Inventory.Contents)
						{
							if (content.HasAmmoOfType(item6))
							{
								Equippable item3 = new Equippable
								{
									Item = item,
									AmmoType = item6,
									Amount = amount,
									InfectedWith = content.InfectedWith
								};
								if (Equippables.IndexOf(item3) == -1)
								{
									Equippables.Add(item3);
									flag2 = true;
								}
							}
						}
						int num2 = Equippables.Count - count;
						if (num2 > 1)
						{
							Equippables.Sort(count, num2, SortEquippablesByInfectionTypeAscending.Instance);
						}
						for (int l = count; l < Equippables.Count; l++)
						{
							if (ammoWeapon == selected && ammoWeapon.CurrentAmmoType == item6 && ammoWeapon.InfectedWith == Equippables[l].InfectedWith)
							{
								selectedIndex = l;
								break;
							}
						}
					}
				}
				if (!flag2)
				{
					Equippable item4 = new Equippable
					{
						Item = item,
						AmmoType = ammoWeapon.CurrentAmmoType,
						Amount = amount
					};
					Equippables.Add(item4);
				}
			}
			else
			{
				Equippable item5 = new Equippable
				{
					Item = item,
					Amount = amount,
					InfectedWith = item.InfectedWith
				};
				Equippables.Add(item5);
			}
		}
		InventoryBehaviour.Temp.Clear();
		selectedIndex = Math.Max(0, Math.Min(selectedIndex, Equippables.Count - 1));
		return Equippables;
	}

	public Equipment GetNextEquippable(Character character, Equipment cur, int dir, bool onlyAimable, out EquipmentPrototype ammoType, out InfectionType infectedWith)
	{
		int selectedIndex;
		List<Equippable> list = BuildEquippableList(character, cur, onlyAimable, takeAll: false, out selectedIndex);
		if (list.Count > 0)
		{
			int index = MathUtil.Clamp(selectedIndex + dir, 0, list.Count - 1);
			ammoType = list[index].AmmoType;
			infectedWith = list[index].InfectedWith;
			return list[index].Item;
		}
		ammoType = null;
		infectedWith = InfectionType.None;
		return null;
	}

	private static void RemoveArrows(List<Injury> injuries, ref int numArrows, ref bool removedArrows)
	{
		for (int i = 0; i < injuries.Count; i++)
		{
			if (injuries[i].ArrowStuck)
			{
				if (numArrows > 0)
				{
					numArrows--;
					continue;
				}
				Injury value = injuries[i];
				value.ArrowStuck = false;
				injuries[i] = value;
				removedArrows = true;
			}
		}
	}

	public void CacheEncumbered(TileObject carrier)
	{
		if (carrier is Character character)
		{
			character.CachedInventoryWeight = GetWeight(carrier);
			character.Encumbered = character.CachedInventoryWeight > character.GetMaxInventoryWeight();
			character.CachedShortcutMeleeWeaponValid = false;
			character.CachedShortcutAmmoWeaponValid = false;
			character.CachedShortcutThrowableValid = false;
			int numArrows = CountItemsOfClass(typeof(Arrow));
			bool removedArrows = false;
			RemoveArrows(character.Injuries, ref numArrows, ref removedArrows);
			foreach (Equipment content in character.Inventory.Contents)
			{
				if (content is Armor armor)
				{
					RemoveArrows(armor.ArmorDamagePoints, ref numArrows, ref removedArrows);
				}
			}
			if (removedArrows)
			{
				character.UnityUpdateInjuries();
			}
			if (Session.Instance != null)
			{
				Session.Instance.UpdateHighestSkillLevel(character);
			}
		}
		if (InfoScreen.Instance.IsShowingInventoryFor(carrier))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public Equipment Take(TileObject carrier, Equipment item, int amount)
	{
		if (Contents.IndexOf(item) == -1)
		{
			return null;
		}
		Equipment equipment = item.TakeMe(amount);
		CacheEncumbered(carrier);
		if (equipment == item)
		{
			Remove(carrier, item);
		}
		StoryManager.Instance.SetConditionsDirty();
		return equipment;
	}

	public int UseItemOfType(Character user, TileObject carrier, EquipmentPrototype type, int amount, bool isCrafting, out InfectionType infectionType, IngredientInfectionState ingredientInfectionState = IngredientInfectionState.Any, bool checkCraftingPolicy = false)
	{
		return UseItemOfType(user, carrier, type, amount, null, out infectionType, ingredientInfectionState, checkCraftingPolicy);
	}

	public int UseItemOfType(Character user, TileObject carrier, EquipmentPrototype type, int amount, Recipe crafting, out InfectionType infectionType, IngredientInfectionState ingredientInfectionState = IngredientInfectionState.Any, bool checkCraftingPolicy = false)
	{
		int num = 0;
		infectionType = InfectionType.None;
		while (num < amount)
		{
			Equipment equipment = FindItemOfTypePreferringUnused(user, carrier, type, crafting, checkCraftingPolicy, ingredientInfectionState);
			if (equipment == null)
			{
				break;
			}
			infectionType = (InfectionType)Math.Max((int)infectionType, (int)equipment.InfectedWith);
			int num2 = UseItem(carrier, equipment, amount - num);
			num += num2;
			if (num2 == 0 || carrier.IsPredicted())
			{
				break;
			}
		}
		return num;
	}

	public int UseItem(TileObject carrier, Equipment item, int amount)
	{
		if (Contents.IndexOf(item) == -1)
		{
			return 0;
		}
		amount = Math.Min(amount, item.GetAmount());
		if (carrier.IsPredicted())
		{
			return amount;
		}
		if (!InfoScreen.Instance.Active && carrier is Character)
		{
			NotificationManager.Instance.AddEquipmentNotification(carrier, null, item, amount);
		}
		if (item.GetAmount() > amount)
		{
			item.IncrementAmount(-amount);
			CacheEncumbered(carrier);
			return amount;
		}
		int num = 0;
		while (item.GetLiquidContentsAmount() > 0.001f)
		{
			Equipment equipment = null;
			float num2 = 0f;
			for (int i = 0; i < Count; i++)
			{
				Equipment item2 = GetItem(i);
				if (item2 != item && !(item2.GetLiquidCapacity() <= 0f) && !(item2.GetLiquidContentsAmount() >= item2.GetLiquidCapacity() - 0.01f) && (item2.GetLiquidContentsType() == null || item2.GetLiquidContentsType() == item.GetLiquidContentsType()))
				{
					float num3 = 1f;
					if (item2.GetLiquidContentsType() == item.GetLiquidContentsType())
					{
						num3 += 10f;
					}
					if (item2.GetPrototype().DefaultLiquidPrototype == item.GetLiquidContentsType())
					{
						num3 += 1f;
					}
					if (num3 > num2)
					{
						num2 = num3;
						equipment = item2;
					}
				}
			}
			if (equipment == null)
			{
				break;
			}
			float maxAmount = equipment.GetLiquidCapacity() - equipment.GetLiquidContentsAmount() + 0.01f;
			equipment.FillLiquid(item.GetLiquidContentsType(), item.DrainLiquid(maxAmount), item.InfectedWith);
			num++;
			if (num >= 1000)
			{
				Debug.LogError("Infinite loop in UseItem! " + item.GetDisplayNameString() + ", " + item.GetLiquidContentsAmount());
				break;
			}
		}
		Remove(carrier, item);
		item.Delete();
		return item.GetAmount();
	}

	public float DrainLiquidOfType(TileObject carrier, LiquidPrototype type, float amount, Recipe recipe, out InfectionType infectedWith, Character checkEquipmentPolicy)
	{
		infectedWith = InfectionType.None;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetLiquidContentsType() == type && (checkEquipmentPolicy == null || checkEquipmentPolicy.IsActionAllowedForItem(content, EquipmentPolicyAction.CanCraftWith)))
			{
				DrainCandidate item = new DrainCandidate
				{
					Item = content,
					Score = content.GetLiquidContentsAmount()
				};
				item.Score -= (float)content.InfectedWith * 1000f;
				if (recipe.IsIngredient(content))
				{
					item.Score -= 100000000f;
				}
				DrainCandidates.Add(item);
			}
		}
		DrainCandidates.Sort();
		for (int i = 0; i < DrainCandidates.Count; i++)
		{
			num += DrainCandidates[i].Item.DrainLiquid(amount - num);
			infectedWith = (InfectionType)Math.Max((int)infectedWith, (int)DrainCandidates[i].Item.InfectedWith);
			if (num >= amount)
			{
				break;
			}
		}
		DrainCandidates.Clear();
		return num;
	}

	public Equipment TakeAmmoOfType(TileObject owner, EquipmentPrototype ammoType, InfectionType infectedWith, int desiredAmount)
	{
		Equipment equipment = FindItemOfType(ammoType, infectedWith);
		if (equipment != null)
		{
			equipment = Take(owner, equipment, desiredAmount);
			desiredAmount -= equipment.GetAmount();
		}
		if (desiredAmount > 0)
		{
			for (int i = 0; i < Contents.Count; i++)
			{
				if (Contents[i] is AmmoWeapon ammoWeapon && ammoWeapon.CurrentAmmoType == ammoType && ammoWeapon.InfectedWith == infectedWith && ammoWeapon.CurrentAmmo > 0)
				{
					int num = Math.Min(desiredAmount, ammoWeapon.CurrentAmmo);
					if (equipment == null)
					{
						equipment = Equipment.Spawn(ammoType, num);
						equipment.InfectedWith = infectedWith;
					}
					else
					{
						equipment.SetNewAmount(equipment.GetAmount() + num);
					}
					ammoWeapon.ConsumeAmmo(num);
				}
			}
		}
		return equipment;
	}

	public int TakeBestAmmoForWeapon(TileObject owner, Character checkIfCharacterIsAllowed, AmmoWeapon weapon, ref int requiredAmount, ref EquipmentPrototype takenAmmoType, ref InfectionType infectedWith)
	{
		int num = 0;
		for (int num2 = Contents.Count - 1; num2 >= 0; num2--)
		{
			Equipment equipment = Contents[num2];
			if (equipment != weapon)
			{
				bool deleteMe = false;
				num += equipment.TakeBestAmmoForWeapon(checkIfCharacterIsAllowed, weapon, ref requiredAmount, ref takenAmmoType, ref infectedWith, out deleteMe);
				if (deleteMe)
				{
					Remove(owner, equipment);
					equipment.Delete();
				}
			}
		}
		CacheEncumbered(owner);
		return num;
	}

	public Weapon GetBestWeapon(Character character, TileObject target, bool wantRanged, bool bluntOnly, bool ignoreGathered = false)
	{
		EquipmentPrototype bestAmmoType;
		InfectionType bestInfectedWith;
		return GetBestWeapon(character, target, wantRanged, bluntOnly, out bestAmmoType, out bestInfectedWith, ignoreGathered);
	}

	public Weapon GetBestWeapon(Character character, TileObject target, bool wantRanged, bool bluntOnly, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith, bool ignoreGathered = false)
	{
		float num = 0f;
		Weapon result = null;
		bestAmmoType = null;
		bestInfectedWith = InfectionType.None;
		foreach (Equipment content in Contents)
		{
			if (content is Weapon weapon && !(weapon is GlassBottle) && weapon is RangedWeapon == wantRanged && (!ignoreGathered || content.GetGatheredAmount() < content.GetAmount()) && (!bluntOnly || weapon.GetInjuryType() == InjuryType.BluntObject) && (!(content is Throwable item) || character.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse)))
			{
				EquipmentPrototype bestAmmoType2;
				InfectionType bestInfectedWith2;
				float score = weapon.GetScore(character, target, out bestAmmoType2, out bestInfectedWith2);
				if (score > num)
				{
					num = score;
					result = weapon;
					bestAmmoType = bestAmmoType2;
					bestInfectedWith = bestInfectedWith2;
				}
			}
		}
		return result;
	}

	public Equipment GetBestWeaponForShortcut(Character character, Type type, int count, out EquipmentPrototype resultAmmoType, out InfectionType resultInfectedWith)
	{
		using (new ProfileMarker(GetBestWeaponForShortcutTimer))
		{
			foreach (Equipment content in Contents)
			{
				if (!type.IsInstanceOfType(content))
				{
					continue;
				}
				if (!content.CanBeCombined())
				{
					bool flag = false;
					for (int i = 0; i < WeaponScores.Count; i++)
					{
						if (WeaponScores[i].Item.GetPrototype() == content.GetPrototype())
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				if (content is AmmoWeapon ammoWeapon && ammoWeapon.GetAmmoTypes() != null)
				{
					foreach (EquipmentPrototype ammoType in ammoWeapon.GetAmmoTypes())
					{
						foreach (Equipment content2 in character.Inventory.Contents)
						{
							if (content2.HasAmmoOfType(ammoType))
							{
								EquipmentScore item = new EquipmentScore
								{
									Item = content,
									AmmoType = ammoType,
									InfectedWith = content2.InfectedWith
								};
								if (!WeaponScores.Contains(item))
								{
									item.Score = content.GetDamageIncludingEffects(character, ammoType) * (float)content.GetNumPelletsPerShot();
									WeaponScores.Add(item);
								}
							}
						}
					}
				}
				else
				{
					EquipmentScore item2 = new EquipmentScore
					{
						Item = content,
						Score = content.GetDamageIncludingEffects(character) * (float)content.GetNumPelletsPerShot()
					};
					WeaponScores.Add(item2);
				}
			}
			if (WeaponScores.Count == 0)
			{
				resultInfectedWith = InfectionType.None;
				resultAmmoType = null;
				return null;
			}
			WeaponScores.Sort(WeaponSorter);
			int index = count % WeaponScores.Count;
			Equipment item3 = WeaponScores[index].Item;
			resultAmmoType = WeaponScores[index].AmmoType;
			resultInfectedWith = WeaponScores[index].InfectedWith;
			WeaponScores.Clear();
			return item3;
		}
	}

	public Weapon GetBestWeaponForIdle(Character character, bool rangedOnly, bool allowMolotovs)
	{
		float num = 0f;
		Weapon result = null;
		foreach (Equipment content in Contents)
		{
			if (content is Weapon weapon && (!(weapon is AmmoWeapon ammoWeapon) || character.HasInfiniteAmmo(ammoWeapon) || ammoWeapon.CurrentAmmo != 0 || HasAmmoForWeapon(ammoWeapon)) && (!rangedOnly || weapon is RangedWeapon) && (allowMolotovs || !(weapon is MolotovCocktail)) && !(weapon is GlassBottle))
			{
				EquipmentPrototype bestAmmoType;
				InfectionType bestInfectedWith;
				float score = weapon.GetScore(character, null, out bestAmmoType, out bestInfectedWith);
				if (score > num)
				{
					num = score;
					result = weapon;
				}
			}
		}
		return result;
	}

	public bool HasAnyGuns(Character owner, bool withAmmo)
	{
		foreach (Equipment content in Contents)
		{
			if (content is Gun gun && (!withAmmo || owner.HasInfiniteAmmo(gun) || gun.CurrentAmmo != 0 || owner.Inventory.HasAmmoForWeapon(gun)))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyDecentWeapons()
	{
		foreach (Equipment content in Contents)
		{
			if (content is Weapon weapon && (!(weapon is Throwable) || weapon is MolotovCocktail || weapon is PipeBomb))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOutOfAmmo(Character character)
	{
		TempAmmoTypes.Clear();
		foreach (Equipment content in Contents)
		{
			if (content.GetPrototype().AmmoPrototypes == null)
			{
				continue;
			}
			foreach (EquipmentPrototype ammoPrototype in content.GetPrototype().AmmoPrototypes)
			{
				if (!TempAmmoTypes.Contains(ammoPrototype) && character.IsActionAllowedForItem(ammoPrototype, null, InfectionType.None, EquipmentPolicyAction.CanUse))
				{
					if (FindItemOfType(ammoPrototype) != null)
					{
						TempAmmoTypes.Clear();
						return false;
					}
					TempAmmoTypes.Add(ammoPrototype);
				}
			}
		}
		bool result = TempAmmoTypes.Count > 0;
		TempAmmoTypes.Clear();
		return result;
	}

	public Toolbox GetToolbox()
	{
		foreach (Equipment content in Contents)
		{
			if (content is Toolbox result)
			{
				return result;
			}
		}
		return null;
	}

	public Shovel GetShovel()
	{
		foreach (Equipment content in Contents)
		{
			if (content is Shovel result)
			{
				return result;
			}
		}
		return null;
	}

	public Axe GetAxe()
	{
		foreach (Equipment content in Contents)
		{
			if (content is Axe result)
			{
				return result;
			}
		}
		return null;
	}

	public Pickaxe GetPickaxe()
	{
		foreach (Equipment content in Contents)
		{
			if (content is Pickaxe result)
			{
				return result;
			}
		}
		return null;
	}

	public HuntingKnife GetHuntingKnife()
	{
		foreach (Equipment content in Contents)
		{
			if (content is HuntingKnife result)
			{
				return result;
			}
		}
		return null;
	}

	public int CountHuntingKnives()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content is HuntingKnife)
			{
				num++;
			}
		}
		return num;
	}

	public Equipment GetGold()
	{
		return FindItemOfType(EquipmentPrototype.Gold);
	}

	public int GetGoldAmount()
	{
		return GetGold()?.GetAmount() ?? 0;
	}

	public int GetNumGiftableItems()
	{
		int num = 0;
		foreach (Equipment content in Contents)
		{
			if (content.CanBeGiftItem())
			{
				num++;
			}
		}
		return num;
	}

	public float CalcLootableSuppliesValue(TileObject owner)
	{
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (StoryEvent.CanLootItem(owner, content))
			{
				num += (float)content.GetAmount() * content.GetBasePrice();
			}
		}
		return num;
	}

	public Equipment GetBestBackpack(int fitness)
	{
		Equipment result = null;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == ClothingType.Backpack)
			{
				float carryWeightEffect = content.GetCarryWeightEffect(fitness);
				if (carryWeightEffect > num)
				{
					result = content;
					num = carryWeightEffect;
				}
			}
		}
		return result;
	}

	public Equipment GetClothingOfTypeWithBestCarryEffect(ClothingType clothingType, int fitness, Equipment ignoreMe)
	{
		Equipment result = null;
		float num = 0f;
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == clothingType && content != ignoreMe)
			{
				float carryWeightEffect = content.GetCarryWeightEffect(fitness);
				if (carryWeightEffect > num)
				{
					result = content;
					num = carryWeightEffect;
				}
			}
		}
		return result;
	}

	public Equipment GetCheapestClothingOfType(ClothingType clothingType)
	{
		Equipment result = null;
		float num = float.MaxValue;
		foreach (Equipment content in Contents)
		{
			if (content.GetClothingType() == clothingType && content.GetBasePrice() < num)
			{
				result = content;
				num = content.GetBasePrice();
			}
		}
		return result;
	}

	public Equipment GetItem(int i)
	{
		return Contents[i];
	}

	public bool IsEmptyExceptForWornItems(TileObject carrier)
	{
		return IsEmptyExceptForWornItems(carrier, showCampfirePot: false);
	}

	public bool IsEmptyExceptForWornItems(TileObject carrier, bool showCampfirePot)
	{
		foreach (Equipment content in Contents)
		{
			if (content.IsIncludedInTakeAll(carrier, showCampfirePot))
			{
				return false;
			}
		}
		return true;
	}

	public Equipment Add(TileObject carrier, Equipment equipment)
	{
		return Add(carrier, equipment, null);
	}

	public Equipment Add(TileObject carrier, Equipment equipment, TileObject from)
	{
		if (equipment == null)
		{
			return null;
		}
		Character character = carrier as Character;
		Community community = carrier.GetCommunity();
		if (community != null)
		{
			if (equipment.GetNutrition() > 0f)
			{
				community.CachedHarvestedNutritionAmount = -1f;
			}
			if (community.CommunityType == CommunityType.Player)
			{
				equipment.MarkDiscovered();
				if (character != null && equipment is AmmoWeapon)
				{
					character.CachedCarryAmountPoliciesDirty = true;
				}
			}
			else
			{
				equipment.DesignatedLiquid = null;
			}
		}
		if (character != null)
		{
			SkillType skillBonusType = equipment.GetPrototype().SkillBonusType;
			if (skillBonusType != SkillType.Invalid)
			{
				character.ClearCachedSkillLevelWithEffects(skillBonusType);
			}
		}
		if (equipment.CanBeCombined())
		{
			for (int i = 0; i < Contents.Count; i++)
			{
				Equipment equipment2 = Contents[i];
				if (equipment2.CanBeCombinedWith(equipment))
				{
					equipment2.CombineAmount(equipment);
					if (equipment.Id != 0)
					{
						StoryManager.Instance.ReplaceEquipmentInQuestInstances(equipment, equipment2);
						equipment.Delete();
					}
					CacheEncumbered(carrier);
					if (StoryManager.Instance != null && carrier.Id != 0)
					{
						StoryManager.Instance.SetConditionsDirty();
					}
					return equipment2;
				}
			}
		}
		Contents.Add(equipment);
		equipment.SetInventoryOwner(carrier);
		if (equipment.Id != 0 && carrier.Id != 0 && BaseObjectManager.Instance != null && BaseObjectManager.Instance.FindBaseObjectByID(equipment.Id) == null)
		{
			Debug.LogWarning("Adding invalid " + equipment.GetDisplayNameString() + " (" + equipment.Id + ") to inventory of " + carrier.GetDisplayNameString() + " (" + carrier.Id + ")");
		}
		if (equipment.GetClothingType() == ClothingType.Backpack && character != null)
		{
			int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Strength);
			if (GetBestBackpack(skillLevelWithEffects) == equipment)
			{
				equipment.Wear(character);
			}
		}
		else if (character != null && equipment is Armor && character.AliveAndNotZombie && character.Clothes[(int)equipment.GetClothingType()] == null && Session.Instance != null && !character.IsControllableByPlayer())
		{
			equipment.Wear(character);
		}
		CacheEncumbered(carrier);
		if (equipment.GetAntigenType() != InfectionType.None && character != null && character.Community != null)
		{
			character.Community.ClearNoAntigenKnown(equipment.GetAntigenType());
		}
		if (character != null && character.IsControllableByPlayer())
		{
			if (EquipmentPrototype.AllAmmoTypes.Contains(equipment.GetPrototype()) || equipment is RangedWeapon)
			{
				foreach (Target target in character.Targets)
				{
					target.ClearInaccessible();
				}
			}
			if (equipment.GetBaseObjectType() == BaseObjectType.BrainScanner && BaseObjectManager.Instance.GetObjectByUniqueID(Character.CooperMcClure) is Character character2 && character2.Inventory.FindItemOfClass(typeof(BrainScanner)) == null)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.Story_FindBrainScanner);
			}
		}
		if (from != null && carrier is AnimalFeederProp animalFeederProp)
		{
			animalFeederProp.LastFilledBy = from as Character;
		}
		if (StoryManager.Instance != null && carrier.Id != 0)
		{
			StoryManager.Instance.SetConditionsDirty();
		}
		if (InfoScreen.Instance.IsShowingInventoryFor(carrier))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		if ((equipment is WorldMap || equipment is Radio) && carrier.GetCommunity() != null && carrier.GetCommunity().CommunityType == CommunityType.Player)
		{
			GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
		}
		return equipment;
	}

	public void RemoveEquipmentWithNoPrototype(TileObject carrier, Equipment equipment)
	{
		equipment.SetInventoryOwner(null);
		Contents.Remove(equipment);
		if (!(carrier is Character character))
		{
			return;
		}
		if (character.EquippedItem == equipment)
		{
			character.EquippedItem = null;
		}
		if (character.DesiredEquippedItem == equipment)
		{
			character.DesiredEquippedItem = null;
		}
		for (int i = 0; i < character.Clothes.Length; i++)
		{
			if (character.Clothes[i] == equipment)
			{
				character.Clothes[i] = null;
			}
		}
	}

	public void Remove(TileObject carrier, Equipment equipment)
	{
		equipment.Gifted = false;
		equipment.Concealed = false;
		equipment.ClearGathered();
		equipment.SetInventoryOwner(null);
		Contents.Remove(equipment);
		if (equipment.GetNutrition() > 0f)
		{
			Community community = carrier.GetCommunity();
			if (community != null)
			{
				community.CachedHarvestedNutritionAmount = -1f;
			}
		}
		Character character = carrier as Character;
		if (character != null)
		{
			SkillType skillBonusType = equipment.GetPrototype().SkillBonusType;
			if (skillBonusType != SkillType.Invalid)
			{
				character.ClearCachedSkillLevelWithEffects(skillBonusType);
			}
			if (character.EquippedItem == equipment)
			{
				character.EquippedItem = null;
			}
			if (character.DesiredEquippedItem == equipment)
			{
				character.DesiredEquippedItem = null;
			}
			if (character.IsWearing(equipment))
			{
				equipment.Strip(character);
				if (equipment.GetClothingType() == ClothingType.Backpack)
				{
					int skillLevelWithEffects = character.GetSkillLevelWithEffects(SkillType.Strength);
					GetBestBackpack(skillLevelWithEffects)?.Wear(character);
				}
			}
		}
		if (carrier is CraftingProp { CraftingRecipe: not null } craftingProp)
		{
			if ((craftingProp.CraftingRecipe.RecipeType == RecipeType.Campfire_Pot && equipment.GetPrototype() == EquipmentPrototype.Pot && craftingProp.Inventory.FindItemOfType(EquipmentPrototype.Pot) == null) || (craftingProp.CraftingRecipe.RecipeType != RecipeType.Campfire_Pot && Contents.Count == 0))
			{
				if (Contents.Count > 0)
				{
					Debug.Log("Destroying " + Contents.Count + " items in campfire! " + Contents[0].GetDisplayNameString());
					DeleteAll(carrier, carrierBeingDeleted: false);
				}
				craftingProp.SetCraftingRecipe(null, null, 0f, InfectionType.None, 0);
			}
			else if ((craftingProp.CraftingRecipe.RecipeType == RecipeType.Campfire_FryingPan && equipment.GetPrototype() == EquipmentPrototype.FryingPan && craftingProp.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) == null) || (craftingProp.CraftingRecipe.RecipeType != RecipeType.Campfire_FryingPan && Contents.Count == 0))
			{
				if (Contents.Count > 0)
				{
					Debug.Log("Destroying " + Contents.Count + " items in campfire! " + Contents[0].GetDisplayNameString());
					DeleteAll(carrier, carrierBeingDeleted: false);
				}
				craftingProp.SetCraftingRecipe(null, null, 0f, InfectionType.None, 0);
			}
		}
		CacheEncumbered(carrier);
		if (StoryManager.Instance != null && carrier.Id != 0)
		{
			StoryManager.Instance.SetConditionsDirty();
		}
		if (InfoScreen.Instance.IsShowingInventoryFor(carrier))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		if (carrier.GetCommunity() != null && carrier.GetCommunity().CommunityType == CommunityType.Player)
		{
			if (equipment is WorldMap || equipment is Radio)
			{
				GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
			}
			if (character != null && equipment is AmmoWeapon)
			{
				character.CachedCarryAmountPoliciesDirty = true;
			}
		}
	}

	public void RemoveAll(TileObject carrier)
	{
		while (Count > 0)
		{
			Equipment item = GetItem(0);
			Remove(carrier, item);
		}
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.IsSerialising)
		{
			foreach (Equipment content in Contents)
			{
				if (content.Id == 0 || BaseObjectManager.Instance.FindBaseObjectByID(content.Id) == null)
				{
					Debug.LogError("Trying to serialise invalid " + content.GetDisplayNameString() + ", id: " + content.Id + ", amount: " + content.GetAmount());
				}
			}
		}
		reflector.AddGameObjectRefList(ref Contents);
		reflector.AddAfter(ref GeneratedLoot, 469);
	}
}
