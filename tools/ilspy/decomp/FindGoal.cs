using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FindGoal : StateMachineGoal
{
	public struct FindData
	{
		public InfectionType WorstInfectionType;

		public bool HasFlint;

		public bool HasKnife;

		public bool HasAxe;

		public bool HasPickaxe;

		public float NeedWater;

		public float NeedUrine;

		public float NeedSnow;

		public bool NeedWood;

		public EquipmentPrototype NeedMiningResource;

		public float CurrentInventoryWeight;

		public Equipment BestWaterBottle;

		public List<EquipmentPolicy> CarryPolicies;

		public bool HasCarryPolicyForItemType(EquipmentPrototype proto)
		{
			if (CarryPolicies != null)
			{
				for (int i = 0; i < CarryPolicies.Count; i++)
				{
					if (CarryPolicies[i].Matches(proto, null, InfectionType.None))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public static string[] FindTypeNames = StringUtil.GetEnumNames<FindType>();

	public bool Critical;

	public bool CanTravelFar;

	public bool OnlyInEnclosedAreas;

	public bool CheckEquipmentPolicyForCraftingResources;

	public bool HasStartedChoppingOrMining;

	public bool ChoseToFindTool;

	public float MaxDistToTravel = float.MaxValue;

	public FindResult Result;

	public TerrainCoord FilledFromTile = TerrainCoord.Invalid;

	public int FilledFromPathIndex = -1;

	public MovementType MovementType;

	public Equipment FoundItem;

	public EquipmentPrototype FoundResource;

	public FindType FindType;

	public EquipmentPrototype ProtoToFind;

	public LiquidPrototype Liquid;

	public float LiquidAmount;

	public Recipe FollowingRecipe;

	public Equipment UsingItem;

	public int DesiredAmountOfRecipe = 1;

	public TileObject Building;

	public float ToCarryWeight;

	public List<PropPrototype> CropTypesToFindSeedsFor;

	public RoleInfo CraftingRole;

	public static GameProfiler _Timer = new GameProfiler("Update.FindGoalSearch");

	public static TimeSpan RememberFailedAttemptTime = Sun.DayLength;

	public static float CostOfFailure = 10f;

	private static List<Target> EscapedFromTargets = new List<Target>();

	public static float MinDistSqFromSomeoneWeRanAwayFrom = MathUtil.Squared(64f);

	public static float MinDotFromSomeoneWeRanAwayFrom = Mathf.Cos(MathF.PI / 4f);

	public static int DefaultFindFoodAmount = 3;

	private static float TreeChopCost = 30f;

	public bool Success => Result == FindResult.Success;

	public FindGoal()
	{
	}

	public FindGoal(FindType findType, MovementType movementType, bool critical)
	{
		Critical = critical;
		MovementType = movementType;
		FindType = findType;
	}

	public FindGoal(FindType findType, EquipmentPrototype proto, MovementType movementType, bool critical)
	{
		Critical = critical;
		MovementType = movementType;
		FindType = findType;
		ProtoToFind = proto;
	}

	public FindGoal(FindType findType, LiquidPrototype liquid, float liquidAmount, MovementType movementType, bool critical)
	{
		Critical = critical;
		MovementType = movementType;
		FindType = findType;
		Liquid = liquid;
		LiquidAmount = liquidAmount;
	}

	public FindGoal(FindType findType, Recipe recipe, TileObject building, int desiredAmount, MovementType movementType, bool critical)
	{
		Critical = critical;
		MovementType = movementType;
		FindType = findType;
		FollowingRecipe = recipe;
		Building = building;
		DesiredAmountOfRecipe = desiredAmount;
	}

	public FindGoal(FindType findType, Recipe recipe, Equipment usingItem, int desiredAmount, MovementType movementType, bool critical)
	{
		Critical = critical;
		MovementType = movementType;
		FindType = findType;
		FollowingRecipe = recipe;
		UsingItem = usingItem;
		DesiredAmountOfRecipe = desiredAmount;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FindGoal;
	}

	public override void BuildDebugExtraInfoString(Character character, Goal parent, StringBuilder str)
	{
		base.BuildDebugExtraInfoString(character, parent, str);
		str.Append(' ');
		str.Append('(');
		str.Append(FindTypeNames[(int)FindType]);
		str.Append(')');
	}

	public override bool WasSuccessful()
	{
		return Success;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Critical);
		reflector.AddAfter(ref CanTravelFar, 259);
		reflector.AddAfter(ref OnlyInEnclosedAreas, 536);
		reflector.AddAfter(ref MaxDistToTravel, 376);
		reflector.AddAfter(ref CheckEquipmentPolicyForCraftingResources, 206);
		reflector.AddAfter(ref Result, 41);
		reflector.AddAfter(ref FilledFromTile, 386);
		reflector.AddAfter(ref FilledFromPathIndex, 386);
		reflector.AddAfter(ref Building, 383);
		reflector.AddAfter(ref HasStartedChoppingOrMining, 332);
		reflector.AddAfter(ref ChoseToFindTool, 357);
		if (reflector.Version >= 444)
		{
			CraftingRole.Reflect(reflector);
		}
		if (reflector.Version < 41)
		{
			bool value = false;
			reflector.Add(ref value);
			Result = (value ? FindResult.Success : FindResult.NotFound);
		}
		reflector.Add(ref MovementType);
		reflector.Add(ref FoundItem);
		reflector.AddAfter(ref FoundResource, 387);
		reflector.Add(ref FindType);
		reflector.Add(ref ProtoToFind);
		reflector.AddAfter(ref Liquid, 65);
		reflector.AddAfter(ref LiquidAmount, 296);
		reflector.Add(ref FollowingRecipe);
		reflector.AddAfter(ref UsingItem, 298);
		reflector.Add(ref DesiredAmountOfRecipe);
		reflector.Add(ref ToCarryWeight);
		if (reflector.Version < 218)
		{
			List<BaseObjectType> list = new List<BaseObjectType>();
			reflector.AddBaseObjectTypeList(ref list);
			{
				foreach (BaseObjectType item in list)
				{
					PropPrototype propPrototype = GameImpl.Instance.FindPropPrototypeByName(item.ToString());
					if (propPrototype != null)
					{
						if (CropTypesToFindSeedsFor == null)
						{
							CropTypesToFindSeedsFor = new List<PropPrototype>();
						}
						CropTypesToFindSeedsFor.Add(propPrototype);
					}
				}
				return;
			}
		}
		reflector.AddPropPrototypeList(ref CropTypesToFindSeedsFor);
	}

	private bool CanIgnoreWeight(Character character)
	{
		switch (FindType)
		{
		case FindType.Food:
		case FindType.Drink:
		case FindType.Bandage:
		case FindType.Antigen:
		case FindType.Lighter:
		case FindType.MaterialForFire:
		case FindType.Backpack:
		case FindType.WaterBottle:
		case FindType.Clothing:
		case FindType.SealedContainer:
		case FindType.WaterForCrops:
		case FindType.FoodForAnimals:
		case FindType.WaterForAnimals:
		case FindType.GoodBandage:
			return true;
		case FindType.Prototype:
		case FindType.CraftingResources:
		case FindType.BuildingResources:
			return !character.IsControllableByPlayer();
		default:
			return false;
		}
	}

	public Equipment FindItemInInventory(Character character, TileObject carrier, FindData findData)
	{
		bool tooHeavy = false;
		float score = 0f;
		int amountToTransfer = 0;
		Equipment result = FindItemInInventory(character, carrier, findData, ref tooHeavy, ref score, ref amountToTransfer);
		if (!tooHeavy)
		{
			return result;
		}
		return null;
	}

	public Equipment FindItemInInventory(Character character, TileObject carrier, FindData findData, ref bool tooHeavy, ref float score, ref int amountToTransfer)
	{
		EquipmentContainer inventory = carrier.GetInventory();
		if (inventory == null)
		{
			return null;
		}
		Equipment equipment = null;
		switch (FindType)
		{
		case FindType.Prototype:
			equipment = inventory.FindItemOfType(ProtoToFind);
			break;
		case FindType.Food:
		{
			bool forSharing = character.IsAlly(carrier);
			equipment = inventory.GetFood(carrier, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing, out score);
			break;
		}
		case FindType.FoodForJourney:
			equipment = inventory.GetFood(carrier, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false, out score);
			break;
		case FindType.Drink:
			equipment = inventory.GetBestWaterBottleToDrink(carrier, character, forSharing: false);
			break;
		case FindType.WaterForCrops:
			equipment = inventory.GetBestWateringCan(mustHaveWater: true);
			break;
		case FindType.WaterForAnimals:
			equipment = inventory.FindBestItemWithLiquid(LiquidPrototype.Water);
			break;
		case FindType.WaterForJourney:
			equipment = inventory.GetBestWaterBottle();
			break;
		case FindType.FoodForAnimals:
			equipment = inventory.GetFoodForAnimal(BaseObjectType.Chicken, character);
			break;
		case FindType.Alcohol:
			equipment = inventory.GetBestAlcoholToDrink(carrier, character, ignorePolicy: false);
			break;
		case FindType.GoodBandage:
			equipment = inventory.FindItemWithHighestBandageLevel(1);
			break;
		case FindType.Bandage:
		{
			equipment = inventory.FindItemWithHighestBandageLevel();
			if (equipment != null)
			{
				break;
			}
			Character character3 = carrier as Character;
			if (character3 != null && character3.Alive && character3 != character)
			{
				break;
			}
			for (int j = 0; j < inventory.Count; j++)
			{
				Equipment item2 = inventory.GetItem(j);
				if (item2.GetPrototype().SkillBonus > 0 || (character.IsWearing(item2) && !character.IsActionAllowedForItem(item2, EquipmentPolicyAction.CanStrip)))
				{
					continue;
				}
				foreach (Recipe oneItemRecipesForBandage in GameImpl.Instance.OneItemRecipesForBandages)
				{
					if (!oneItemRecipesForBandage.Ingredients[0].MatchesItem(item2, oneItemRecipesForBandage))
					{
						continue;
					}
					equipment = item2;
					score = oneItemRecipesForBandage.CraftingTime;
					if (character3 == character && character.IsWearing(item2))
					{
						if (character.IsControllableByPlayer())
						{
							score += 100f;
						}
						else
						{
							score += 10f;
						}
					}
					break;
				}
			}
			break;
		}
		case FindType.Antigen:
			equipment = inventory.GetAntigen(findData.WorstInfectionType);
			break;
		case FindType.Lighter:
		{
			Equipment equipment2 = inventory.FindItemOfType(EquipmentPrototype.Match);
			Equipment equipment3 = inventory.FindItemOfType(EquipmentPrototype.Flint);
			Equipment equipment4 = inventory.FindItemOfClass(typeof(HuntingKnife));
			if (carrier == character)
			{
				if (equipment4 != null && equipment3 != null)
				{
					equipment = equipment4;
				}
				else if (equipment2 != null)
				{
					equipment = equipment2;
				}
			}
			else if (equipment2 != null)
			{
				equipment = equipment2;
			}
			else if (!findData.HasFlint && equipment3 != null)
			{
				equipment = equipment3;
			}
			else if (!findData.HasKnife && equipment4 != null)
			{
				equipment = equipment4;
			}
			break;
		}
		case FindType.MaterialForFire:
			return inventory.GetBestMaterialToAddToFire(carrier, character);
		case FindType.Backpack:
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				Equipment item = inventory.GetItem(i);
				if (item.GetClothingType() == ClothingType.Backpack && (!(carrier is Character { AliveAndNotZombie: not false }) || !item.IsWornOrRemovedForSparring(carrier)) && character.GetMaxInventoryWeightIncludingBackpack(item) >= findData.CurrentInventoryWeight + item.GetWeight() + ToCarryWeight)
				{
					return item;
				}
			}
			break;
		}
		case FindType.Axe:
			equipment = inventory.GetAxe();
			break;
		case FindType.Pickaxe:
			equipment = inventory.GetPickaxe();
			break;
		case FindType.WateringCan:
			equipment = inventory.GetBestWateringCan();
			break;
		case FindType.Toolbox:
			equipment = inventory.GetToolbox();
			break;
		case FindType.HuntingKnife:
			equipment = inventory.GetHuntingKnife();
			break;
		case FindType.Shovel:
			equipment = inventory.GetShovel();
			break;
		case FindType.Seeds:
		{
			if (CropTypesToFindSeedsFor == null)
			{
				break;
			}
			for (int l = 0; l < CropTypesToFindSeedsFor.Count; l++)
			{
				equipment = inventory.GetSeedsForCropType(carrier, character, CropTypesToFindSeedsFor[l]);
				if (equipment != null)
				{
					break;
				}
			}
			break;
		}
		case FindType.WaterBottle:
			equipment = inventory.GetBestLiquidContainerToFill(LiquidPrototype.Water, out score);
			break;
		case FindType.SealedContainer:
			equipment = inventory.GetBestLiquidContainerToFill(Liquid, LiquidAmount, out score);
			break;
		case FindType.Ammo:
		{
			if (findData.CarryPolicies == null)
			{
				break;
			}
			for (int k = 0; k < findData.CarryPolicies.Count; k++)
			{
				if (findData.CarryPolicies[k].Proto != null)
				{
					equipment = inventory.FindItemOfType(findData.CarryPolicies[k].Proto, findData.CarryPolicies[k].InfectedWith);
				}
				if (findData.CarryPolicies[k].Liquid != null)
				{
					equipment = inventory.FindBestItemWithLiquid(findData.CarryPolicies[k].Liquid, findData.CarryPolicies[k].InfectedWith);
				}
				if (equipment != null)
				{
					if (!(carrier is Character otherCharacter) || CanTransferFromCharacter(otherCharacter, equipment, FindType, out amountToTransfer))
					{
						break;
					}
					equipment = null;
				}
			}
			break;
		}
		case FindType.CraftingResources:
			if (character == carrier)
			{
				if (FollowingRecipe.HasAllIngredients(character, carrier, null, CheckEquipmentPolicyForCraftingResources ? character : null) && FollowingRecipe.Ingredients.Count > 0)
				{
					equipment = inventory.FindIngredient(character, carrier, FollowingRecipe.Ingredients[0], FollowingRecipe, UsingItem, character, CheckEquipmentPolicyForCraftingResources ? character : null);
				}
				break;
			}
			foreach (Ingredient ingredient in FollowingRecipe.Ingredients)
			{
				if (!ingredient.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null))
				{
					equipment = inventory.FindIngredient(character, carrier, ingredient, FollowingRecipe, UsingItem, null, CheckEquipmentPolicyForCraftingResources ? character : null);
					if (equipment != null)
					{
						break;
					}
				}
			}
			break;
		case FindType.BuildingResources:
		{
			if (Building == null)
			{
				break;
			}
			UnderConstructionInfo underConstructionInfo = Building.GetUnderConstructionInfo();
			if (underConstructionInfo == null)
			{
				break;
			}
			foreach (Ingredient ingredient2 in FollowingRecipe.Ingredients)
			{
				if (!underConstructionInfo.HasUsedEnoughOfIngredient(ingredient2))
				{
					equipment = inventory.FindIngredient(character, carrier, ingredient2, FollowingRecipe, UsingItem, (character == carrier) ? character : null, null);
					if (equipment != null)
					{
						break;
					}
				}
			}
			break;
		}
		case FindType.Clothing:
			equipment = inventory.GetBestClothingForCharacter(carrier, character, Critical, null);
			break;
		}
		if (!CanIgnoreWeight(character) && equipment != null && character != carrier && GetPourIntoContainer(character, equipment) == null)
		{
			tooHeavy = !character.HasInventorySpaceFor(equipment.GetWeight());
		}
		return equipment;
	}

	public bool IsLookingInObject(TileObject obj)
	{
		if (SubGoal != null)
		{
			return SubGoal.GetTargetObject() == obj;
		}
		return false;
	}

	public bool IsLookingForType(Character character, EquipmentPrototype proto, out float amountNeeded)
	{
		switch (FindType)
		{
		case FindType.MaterialForFire:
			amountNeeded = 1f;
			return proto == EquipmentPrototype.Wood;
		case FindType.Prototype:
			amountNeeded = 1f;
			return ProtoToFind == proto;
		case FindType.CraftingResources:
			foreach (Ingredient ingredient in FollowingRecipe.Ingredients)
			{
				if (ingredient.Prototypes != null && ingredient.Prototypes.Contains(proto))
				{
					return !ingredient.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null, out amountNeeded);
				}
			}
			amountNeeded = 0f;
			return false;
		case FindType.BuildingResources:
			if (Building != null)
			{
				UnderConstructionInfo underConstructionInfo = Building.GetUnderConstructionInfo();
				if (underConstructionInfo != null)
				{
					foreach (Ingredient ingredient2 in FollowingRecipe.Ingredients)
					{
						if (ingredient2.Prototypes != null && ingredient2.Prototypes.Contains(proto))
						{
							return !underConstructionInfo.HasUsedEnoughOfIngredient(ingredient2, out amountNeeded);
						}
					}
				}
			}
			amountNeeded = 0f;
			return false;
		default:
			amountNeeded = 0f;
			return false;
		}
	}

	private bool IsLookingForWood(Character character)
	{
		if (FindType == FindType.MaterialForFire)
		{
			return true;
		}
		float amountNeeded;
		return IsLookingForType(character, EquipmentPrototype.Wood, out amountNeeded);
	}

	private EquipmentPrototype IsLookingForMiningResources(Character character)
	{
		EquipmentPrototype[] miningResources = EquipmentPrototype.MiningResources;
		foreach (EquipmentPrototype equipmentPrototype in miningResources)
		{
			if (IsLookingForType(character, equipmentPrototype, out var _))
			{
				return equipmentPrototype;
			}
		}
		return null;
	}

	public static bool CanTransferToolFromCrafter(Character otherCharacter, Equipment item, Recipe recipe, TerrainCoord craftingPropLocation)
	{
		if (recipe.RequiredPropToWorkOn() == BaseObjectType.Campfire)
		{
			if (item.GetPrototype() == EquipmentPrototype.Flint && item.GetAmount() <= 1)
			{
				return false;
			}
			if (item.GetPrototype() == EquipmentPrototype.Match && item.GetAmount() <= 1)
			{
				return false;
			}
			if (item is HuntingKnife && otherCharacter.Inventory.CountItemsOfClass(typeof(HuntingKnife)) <= 1)
			{
				return false;
			}
		}
		switch (recipe.RecipeType)
		{
		case RecipeType.Toolbox:
			if (item is Toolbox && otherCharacter.Inventory.CountItemsOfClass(typeof(Toolbox)) <= 1)
			{
				return false;
			}
			break;
		case RecipeType.Shovel:
			if (item is Shovel && otherCharacter.Inventory.CountItemsOfClass(typeof(Shovel)) <= 1)
			{
				return false;
			}
			break;
		case RecipeType.HuntingKnife:
			if (item is HuntingKnife && otherCharacter.Inventory.CountItemsOfClass(typeof(HuntingKnife)) <= 1)
			{
				return false;
			}
			break;
		case RecipeType.Campfire_Pot:
			if (item.GetPrototype() == EquipmentPrototype.Pot && otherCharacter.Inventory.CountItemsOfType(EquipmentPrototype.Pot) <= 1 && GameTerrain.Instance.GetCraftingPropOnTile(craftingPropLocation.x, craftingPropLocation.y) is Campfire campfire2 && campfire2.Inventory.CountItemsOfType(EquipmentPrototype.Pot) == 0)
			{
				return false;
			}
			break;
		case RecipeType.Campfire_FryingPan:
			if (item.GetPrototype() == EquipmentPrototype.FryingPan && otherCharacter.Inventory.CountItemsOfType(EquipmentPrototype.FryingPan) <= 1 && GameTerrain.Instance.GetCraftingPropOnTile(craftingPropLocation.x, craftingPropLocation.y) is Campfire campfire && campfire.Inventory.CountItemsOfType(EquipmentPrototype.FryingPan) == 0)
			{
				return false;
			}
			break;
		}
		if (recipe.IsIngredient(EquipmentPrototype.Wood) && item is Axe && otherCharacter.Inventory.CountItemsOfClass(typeof(Axe)) <= 1)
		{
			return false;
		}
		if (recipe.IsAnyIngredientAMiningResource() && item is Pickaxe && otherCharacter.Inventory.CountItemsOfClass(typeof(Pickaxe)) <= 1)
		{
			return false;
		}
		return true;
	}

	public static bool CanTransferFromCharacter(Character otherCharacter, Equipment item, FindType findType, out int amountToTransfer)
	{
		int num = (amountToTransfer = (item.CanBeCombined() ? item.GetAmount() : otherCharacter.Inventory.CountItemsOfType(item.GetPrototype(), item.InfectedWith)));
		if (otherCharacter.IsWearing(item))
		{
			return false;
		}
		bool flag = otherCharacter.MostRecentRoleIndex >= 0 && otherCharacter.Roles[otherCharacter.MostRecentRoleIndex].Role == Role.Organizer;
		for (int i = 0; i < otherCharacter.Roles.Count; i++)
		{
			if (i != otherCharacter.MostRecentRoleIndex && !flag && findType != FindType.AutoDeposit)
			{
				continue;
			}
			switch (otherCharacter.Roles[i].Role)
			{
			case Role.Builder:
			{
				TileObject tileObject = otherCharacter.IsBuildingSomething();
				if (tileObject == null || tileObject.GetUnderConstructionInfo() == null)
				{
					break;
				}
				if (tileObject.GetUnderConstructionInfo().IsRemainingIngredient(item))
				{
					return false;
				}
				if (!CanTransferToolFromCrafter(otherCharacter, item, tileObject.GetUnderConstructionInfo().Recipe, tileObject.GetTile()))
				{
					return false;
				}
				if (item is Axe && tileObject.GetUnderConstructionInfo().Recipe.IsIngredient(EquipmentPrototype.Wood))
				{
					int num14 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num14 <= 1)
					{
						return false;
					}
					amountToTransfer = num14 - 1;
				}
				if (item is Pickaxe && tileObject.GetUnderConstructionInfo().Recipe.IsAnyIngredientAMiningResource())
				{
					int num15 = otherCharacter.Inventory.CountItemsOfClass(typeof(Pickaxe));
					if (num15 <= 1)
					{
						return false;
					}
					amountToTransfer = num15 - 1;
				}
				break;
			}
			case Role.Repairing:
			{
				RepairGoal repairGoal = otherCharacter.GetRepairGoal();
				if (repairGoal.CurrentBuildingToRepair == null)
				{
					break;
				}
				if (repairGoal.CurrentBuildingToRepair.GetRepairResourceType() == item.GetPrototype())
				{
					amountToTransfer = Math.Min(amountToTransfer, num - Mathf.CeilToInt(repairGoal.CurrentBuildingToRepair.GetRepairResourceNeeded() * repairGoal.CurrentBuildingToRepair.GetDamageFraction()));
					if (amountToTransfer <= 0)
					{
						return false;
					}
				}
				if (item is Axe && repairGoal.CurrentBuildingToRepair.GetRepairResourceType() == EquipmentPrototype.Wood)
				{
					int num6 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num6 <= 1)
					{
						return false;
					}
					amountToTransfer = num6 - 1;
				}
				if (item is Pickaxe && Array.IndexOf(EquipmentPrototype.MiningResources, repairGoal.CurrentBuildingToRepair.GetRepairResourceType()) != -1)
				{
					int num7 = otherCharacter.Inventory.CountItemsOfClass(typeof(Pickaxe));
					if (num7 <= 1)
					{
						return false;
					}
					amountToTransfer = num7 - 1;
				}
				if (item is Toolbox)
				{
					int num8 = otherCharacter.Inventory.CountItemsOfClass(typeof(Toolbox));
					if (num8 <= 1)
					{
						return false;
					}
					amountToTransfer = num8 - 1;
				}
				break;
			}
			case Role.Capturing:
			{
				TerrainCoord targetLocation4 = otherCharacter.Roles[i].TargetLocation;
				TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(targetLocation4.x, targetLocation4.y);
				if (fixedObjectOnTile == null)
				{
					break;
				}
				if (fixedObjectOnTile.GetCaptureResourceType() == item.GetPrototype())
				{
					amountToTransfer = Math.Min(amountToTransfer, num - Mathf.CeilToInt(fixedObjectOnTile.GetCaptureResourceNeeded() * (1f - fixedObjectOnTile.GetCaptureFraction())));
					if (amountToTransfer <= 0)
					{
						return false;
					}
				}
				if (item is Axe && fixedObjectOnTile.GetCaptureResourceType() == EquipmentPrototype.Wood)
				{
					int num18 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num18 <= 1)
					{
						return false;
					}
					amountToTransfer = num18 - 1;
				}
				if (item is Pickaxe && Array.IndexOf(EquipmentPrototype.MiningResources, fixedObjectOnTile.GetCaptureResourceType()) != -1)
				{
					int num19 = otherCharacter.Inventory.CountItemsOfClass(typeof(Pickaxe));
					if (num19 <= 1)
					{
						return false;
					}
					amountToTransfer = num19 - 1;
				}
				if (item is Toolbox)
				{
					int num20 = otherCharacter.Inventory.CountItemsOfClass(typeof(Toolbox));
					if (num20 <= 1)
					{
						return false;
					}
					amountToTransfer = num20 - 1;
				}
				break;
			}
			case Role.Lumberjack:
				if (item is Axe)
				{
					int num3 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num3 <= 1)
					{
						return false;
					}
					amountToTransfer = num3 - 1;
				}
				break;
			case Role.Trapper:
			{
				if (item is Axe)
				{
					int num11 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num11 <= 1)
					{
						return false;
					}
					amountToTransfer = num11 - 1;
					break;
				}
				int num12 = 0;
				foreach (Prop building in otherCharacter.Community.Buildings)
				{
					if (building is ITrap trap && trap.GetEquipmentNeededForReset() == item.GetPrototype() && trap.CanResetTrap())
					{
						num12++;
					}
				}
				foreach (PitTrap pitTrap in otherCharacter.Community.PitTraps)
				{
					if (pitTrap.GetEquipmentNeededForReset() == item.GetPrototype() && pitTrap.CanResetTrap())
					{
						num12++;
					}
				}
				if (num12 >= num)
				{
					return false;
				}
				amountToTransfer = num - num12;
				break;
			}
			case Role.Miner:
				if (item is Pickaxe)
				{
					int num13 = otherCharacter.Inventory.CountItemsOfClass(typeof(Pickaxe));
					if (num13 <= 1)
					{
						return false;
					}
					amountToTransfer = num13 - 1;
				}
				break;
			case Role.Farmer:
				if (findType == FindType.Seeds)
				{
					return false;
				}
				if (item.GetSeedForPlantType() != null && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanPlant))
				{
					if (num < FarmingGoal.MaxSeedsToCarry)
					{
						return false;
					}
					int num4 = FarmingGoal.MaxSeedsToCarry / 2;
					amountToTransfer = Math.Min(amountToTransfer, num - num4);
				}
				if (item is WateringCan)
				{
					int num5 = otherCharacter.Inventory.CountItemsOfClass(typeof(WateringCan));
					if (num5 <= 1)
					{
						return false;
					}
					amountToTransfer = num5 - 1;
				}
				if (item.GetLiquidCapacity() > 0f && (item.GetLiquidContentsType() == null || item.GetLiquidContentsType() == LiquidPrototype.Water) && otherCharacter.Inventory.CountContainersWithLiquidOrEmpty(LiquidPrototype.Water) <= 1)
				{
					return false;
				}
				break;
			case Role.Cook:
			{
				if (item.GetPrototype() == EquipmentPrototype.Pot && otherCharacter.Inventory.CountItemsOfType(EquipmentPrototype.Pot) <= 1)
				{
					TerrainCoord targetLocation = otherCharacter.Roles[i].TargetLocation;
					if (!(GameTerrain.Instance.GetCraftingPropOnTile(targetLocation.x, targetLocation.y) is Campfire campfire2) || campfire2.Inventory.CountItemsOfType(EquipmentPrototype.Pot) == 0)
					{
						return false;
					}
				}
				if (item.GetPrototype() == EquipmentPrototype.FryingPan && otherCharacter.Inventory.CountItemsOfType(EquipmentPrototype.FryingPan) <= 1)
				{
					TerrainCoord targetLocation2 = otherCharacter.Roles[i].TargetLocation;
					if (!(GameTerrain.Instance.GetCraftingPropOnTile(targetLocation2.x, targetLocation2.y) is Campfire campfire3) || campfire3.Inventory.CountItemsOfType(EquipmentPrototype.FryingPan) == 0)
					{
						return false;
					}
				}
				if (GameCursor.CanAddMaterialToFire(item.GetPrototype()))
				{
					TerrainCoord targetLocation3 = otherCharacter.Roles[i].TargetLocation;
					if (GameTerrain.Instance.GetCraftingPropOnTile(targetLocation3.x, targetLocation3.y) is Campfire campfire4 && campfire4.WoodRemaining < WarmGoal.ReplenishWoodAmount && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
					{
						if (num <= 1)
						{
							return false;
						}
						amountToTransfer = num - 1;
					}
				}
				if (item is HuntingKnife)
				{
					int num16 = otherCharacter.Inventory.CountItemsOfClass(typeof(HuntingKnife));
					if (num16 <= 1)
					{
						return false;
					}
					amountToTransfer = num16 - 1;
				}
				if (item is Axe)
				{
					int num17 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num17 <= 1)
					{
						return false;
					}
					amountToTransfer = num17 - 1;
				}
				if (item.GetPrototype() == EquipmentPrototype.Flint)
				{
					if (num <= 1)
					{
						return false;
					}
					amountToTransfer = num - 1;
				}
				if (item.GetPrototype() == EquipmentPrototype.Match)
				{
					if (num <= 1)
					{
						return false;
					}
					amountToTransfer = num - 1;
				}
				CraftGoal craftGoal = otherCharacter.GetCraftGoal();
				if (craftGoal == null || craftGoal.FollowingRecipe == null)
				{
					break;
				}
				if (craftGoal.FollowingRecipe.ProductLiquidPrototype != null && craftGoal.FollowingRecipe.RecipeType != RecipeType.Campfire_Pot && craftGoal.FollowingRecipe.IsSuitableContainer(item, dontReplaceContents: true) && craftGoal.GetRoleBeingPerformed(otherCharacter) == Role.Cook && craftGoal.FollowingRecipe.WouldBeUsedToFillWithProduct(otherCharacter, null, item))
				{
					return false;
				}
				if (item.GetLiquidCapacity() > 0f && item.GetLiquidContentsType() == null)
				{
					foreach (Ingredient ingredient in craftGoal.FollowingRecipe.Ingredients)
					{
						if (ingredient.LiquidTypes == null)
						{
							continue;
						}
						foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
						{
							if (otherCharacter.Inventory.GetBestLiquidContainerToFill(liquidType) == item)
							{
								return false;
							}
						}
					}
				}
				if (craftGoal.FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Campfire)
				{
					break;
				}
				CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(otherCharacter.Roles[i].TargetLocation.x, otherCharacter.Roles[i].TargetLocation.y);
				if (craftingPropOnTile == null)
				{
					break;
				}
				Equipment equipment2 = craftingPropOnTile.Inventory.FindItemOfType(EquipmentPrototype.Pot);
				if (equipment2 == null)
				{
					equipment2 = otherCharacter.Inventory.GetBestCookingPot();
				}
				if (equipment2 != null)
				{
					LiquidPrototype liquidContentsType2 = equipment2.GetLiquidContentsType();
					if (liquidContentsType2 != null && otherCharacter.Inventory.GetBestLiquidContainerToFill(liquidContentsType2, equipment2.GetLiquidContentsAmount()) == item)
					{
						return false;
					}
				}
				break;
			}
			case Role.Crafter:
			{
				if (otherCharacter.Roles[i].Recipe == null)
				{
					break;
				}
				if (!CanTransferToolFromCrafter(otherCharacter, item, otherCharacter.Roles[i].Recipe, otherCharacter.Roles[i].TargetLocation))
				{
					return false;
				}
				if (otherCharacter.Roles[i].Recipe.ProductLiquidPrototype != null && otherCharacter.Roles[i].Recipe.IsSuitableContainer(item, dontReplaceContents: true) && otherCharacter.Roles[i].Recipe.WouldBeUsedToFillWithProduct(otherCharacter, null, item))
				{
					return false;
				}
				if (item.GetLiquidCapacity() > 0f && item.GetLiquidContentsType() == null)
				{
					foreach (Ingredient ingredient2 in otherCharacter.Roles[i].Recipe.Ingredients)
					{
						if (ingredient2.LiquidTypes == null)
						{
							continue;
						}
						foreach (LiquidPrototype liquidType2 in ingredient2.LiquidTypes)
						{
							if (otherCharacter.Inventory.GetBestLiquidContainerToFill(liquidType2) == item)
							{
								return false;
							}
						}
					}
				}
				if (otherCharacter.Roles[i].Recipe.RequiredPropToWorkOn() != BaseObjectType.Campfire)
				{
					break;
				}
				Campfire campfire = GameTerrain.Instance.GetCraftingPropOnTile(otherCharacter.Roles[i].TargetLocation.x, otherCharacter.Roles[i].TargetLocation.y) as Campfire;
				if (campfire != null)
				{
					Equipment equipment = campfire.Inventory.FindItemOfType(EquipmentPrototype.Pot);
					if (equipment == null)
					{
						equipment = otherCharacter.Inventory.GetBestCookingPot();
					}
					if (equipment != null)
					{
						LiquidPrototype liquidContentsType = equipment.GetLiquidContentsType();
						if (liquidContentsType != null && otherCharacter.Inventory.GetBestLiquidContainerToFill(liquidContentsType, equipment.GetLiquidContentsAmount()) == item)
						{
							return false;
						}
					}
				}
				if (item is HuntingKnife)
				{
					int num9 = otherCharacter.Inventory.CountItemsOfClass(typeof(HuntingKnife));
					if (num9 <= 1)
					{
						return false;
					}
					amountToTransfer = num9 - 1;
				}
				if (item is Axe)
				{
					int num10 = otherCharacter.Inventory.CountItemsOfClass(typeof(Axe));
					if (num10 <= 1)
					{
						return false;
					}
					amountToTransfer = num10 - 1;
				}
				if (item.GetPrototype() == EquipmentPrototype.Flint)
				{
					if (num <= 1)
					{
						return false;
					}
					amountToTransfer = num - 1;
				}
				if (item.GetPrototype() == EquipmentPrototype.Match)
				{
					if (num <= 1)
					{
						return false;
					}
					amountToTransfer = num - 1;
				}
				if (GameCursor.CanAddMaterialToFire(item.GetPrototype()) && campfire != null && campfire.WoodRemaining < WarmGoal.ReplenishWoodAmount && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
				{
					if (num <= 1)
					{
						return false;
					}
					amountToTransfer = num - 1;
				}
				break;
			}
			case Role.AnimalFeeder:
				if (otherCharacter.Inventory.GetFoodForAnimal(BaseObjectType.Chicken, otherCharacter) == item)
				{
					int num2 = CalcDesiredFoodAmountForAnimals(otherCharacter, item);
					if (num <= num2)
					{
						return false;
					}
					amountToTransfer = num - num2;
				}
				break;
			}
		}
		float num21 = 0f;
		if ((uint)(findType - 3) > 1u && findType != FindType.GoodBandage)
		{
			num21 = otherCharacter.GetTargetAmountToCarryIncludingAmmo(item.GetPrototype(), null, item.InfectedWith);
		}
		if (findType != FindType.Food && findType != FindType.FoodForJourney)
		{
			if (otherCharacter.IsUsingForCrafting(item, willTransferContainers: true, findType, out var needed))
			{
				return false;
			}
			amountToTransfer -= needed + (int)num21;
		}
		if (item.GetBandageLevel() > -1)
		{
			int numUnbandagedInjuries = otherCharacter.GetNumUnbandagedInjuries(Math.Min(item.GetBandageLevel(), otherCharacter.GetSkillLevelWithEffects(SkillType.Medicine)));
			if (numUnbandagedInjuries > 0 && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
			{
				amountToTransfer -= numUnbandagedInjuries;
			}
		}
		if (item.GetAntigenType() != InfectionType.None && otherCharacter.HasInjuryWithInfectionType(item.GetAntigenType()) && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
		{
			amountToTransfer--;
		}
		float nutrition = item.GetNutrition();
		if (nutrition > 0f && otherCharacter.GetHunger() >= Character.HungryTime && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse) && (item.GetLiquidContentsType() == null || item.GetLiquidContentsType().GetNutritionPerFlOz() >= EquipmentContainer.MinNutritionPerFlOzToEat) && otherCharacter.Inventory.GetFood(otherCharacter, otherCharacter, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false, out var _) == item)
		{
			float num22 = nutrition * (float)num - otherCharacter.GetHunger();
			if (!(num22 > 0f))
			{
				return false;
			}
			amountToTransfer = Math.Min(amountToTransfer, Mathf.FloorToInt(num22 / item.GetNutrition()));
		}
		if (otherCharacter.GetThirst() / Sun.DayLengthSecs >= otherCharacter.Inventory.GetTotalWater() / Character.WaterNeededPerDayInFlOz && (item.GetLiquidContentsType() == LiquidPrototype.Water || item == otherCharacter.Inventory.GetBestWaterBottle()) && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
		{
			return false;
		}
		if (item.GetLiquidContentsType() != null && item.GetLiquidContentsType().AlcoholContent > 0f && otherCharacter.IsTooDepressedToFollowOrders() && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
		{
			return false;
		}
		if (otherCharacter.CanTransferEquipmentAway(item) != CantTransferReason.CanTransfer)
		{
			return false;
		}
		if (num21 > 0f)
		{
			amountToTransfer = Math.Min(amountToTransfer, num - (int)num21);
		}
		if (item.GetLiquidContentsType() != null)
		{
			float targetAmountToCarryIncludingAmmo = otherCharacter.GetTargetAmountToCarryIncludingAmmo(null, item.GetLiquidContentsType(), item.InfectedWith);
			if (targetAmountToCarryIncludingAmmo > 0f)
			{
				float amountOfLiquidType = otherCharacter.Inventory.GetAmountOfLiquidType(item.GetLiquidContentsType());
				if (item.GetLiquidContentsAmount() > amountOfLiquidType - targetAmountToCarryIncludingAmmo)
				{
					return false;
				}
			}
		}
		if (item.GetLiquidCapacity() > 0f && item.GetLiquidContentsType() == null && (flag || findType == FindType.AutoDeposit))
		{
			List<EquipmentPolicy> allCarryAmountPolicies = otherCharacter.GetAllCarryAmountPolicies();
			if (allCarryAmountPolicies != null)
			{
				for (int j = 0; j < allCarryAmountPolicies.Count; j++)
				{
					if (allCarryAmountPolicies[j].Liquid != null && otherCharacter.Inventory.GetAmountOfLiquidType(allCarryAmountPolicies[j].Liquid, allCarryAmountPolicies[j].InfectedWith) < allCarryAmountPolicies[j].TargetAmount && item == otherCharacter.Inventory.GetBestLiquidContainerToFill(allCarryAmountPolicies[j].Liquid))
					{
						return false;
					}
				}
			}
		}
		if (GameCursor.CanAddMaterialToFire(item.GetPrototype()))
		{
			WarmGoal warmGoal = otherCharacter.GetWarmGoal();
			if (warmGoal != null && warmGoal.ReplenishCampfire != null && !warmGoal.ReplenishCampfire.Deleted && warmGoal.ReplenishCampfire.WoodRemaining < WarmGoal.ReplenishWoodAmount && otherCharacter.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
			{
				amountToTransfer--;
			}
		}
		return amountToTransfer > 0;
	}

	public bool HasInvestigatedCharacter(Character otherCharacter, Equipment item)
	{
		if (otherCharacter.Investigated)
		{
			return true;
		}
		if (item is Arrow && otherCharacter.HasArrowStuckInjury() && GameTerrain.Instance.FogOfWar.IsTileExplored(otherCharacter.Tile.x, otherCharacter.Tile.y))
		{
			return true;
		}
		if (otherCharacter.IsWearing(item) && GameTerrain.Instance.FogOfWar.IsTileExplored(otherCharacter.Tile.x, otherCharacter.Tile.y))
		{
			return true;
		}
		if (otherCharacter.EquippedItem == item && GameTerrain.Instance.FogOfWar.IsTileExplored(otherCharacter.Tile.x, otherCharacter.Tile.y))
		{
			return true;
		}
		return false;
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Goal firstSubGoal = GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
		if (firstSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, firstSubGoal);
			return;
		}
		if (Result != FindResult.Success)
		{
			Result = FindResult.NotFound;
		}
		Finished = true;
	}

	public FindData SetupFindData(Character character)
	{
		FindData result = default(FindData);
		switch (FindType)
		{
		case FindType.Drink:
			result.NeedWater = character.GetThirstInFlOz();
			break;
		case FindType.WaterForCrops:
		case FindType.WaterForAnimals:
		case FindType.WaterForJourney:
			result.NeedWater = 50f;
			break;
		case FindType.CraftingResources:
		case FindType.BuildingResources:
		{
			Ingredient liquidIngredient = FollowingRecipe.GetLiquidIngredient(LiquidPrototype.Water);
			float amountNeeded = 0f;
			if (FindType == FindType.CraftingResources && liquidIngredient != null && !liquidIngredient.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null, out amountNeeded))
			{
				result.NeedWater = amountNeeded;
			}
			if (FindType == FindType.BuildingResources && liquidIngredient != null && Building != null && Building.GetUnderConstructionInfo() != null && !Building.GetUnderConstructionInfo().HasUsedEnoughOfIngredient(liquidIngredient, out amountNeeded))
			{
				result.NeedWater = amountNeeded;
			}
			Ingredient liquidIngredient2 = FollowingRecipe.GetLiquidIngredient(LiquidPrototype.Snow);
			float amountNeeded2 = 0f;
			if (FindType == FindType.CraftingResources && liquidIngredient2 != null && !liquidIngredient2.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null, out amountNeeded2))
			{
				result.NeedSnow = amountNeeded2;
			}
			if (FindType == FindType.BuildingResources && liquidIngredient2 != null && Building.GetUnderConstructionInfo() != null && !Building.GetUnderConstructionInfo().HasUsedEnoughOfIngredient(liquidIngredient2, out amountNeeded2))
			{
				result.NeedSnow = amountNeeded2;
			}
			Ingredient liquidIngredient3 = FollowingRecipe.GetLiquidIngredient(LiquidPrototype.Urine);
			float amountNeeded3 = 0f;
			if (FindType == FindType.CraftingResources && liquidIngredient3 != null && !liquidIngredient3.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null, out amountNeeded3))
			{
				result.NeedUrine = amountNeeded3;
			}
			if (FindType == FindType.BuildingResources && liquidIngredient3 != null && Building.GetUnderConstructionInfo() != null && !Building.GetUnderConstructionInfo().HasUsedEnoughOfIngredient(liquidIngredient3, out amountNeeded3))
			{
				result.NeedUrine = amountNeeded3;
			}
			break;
		}
		case FindType.Antigen:
			result.WorstInfectionType = character.GetWorstInfectionTypeInProgression();
			break;
		case FindType.Lighter:
			result.HasFlint = character.Inventory.FindItemOfType(EquipmentPrototype.Flint) != null;
			result.HasKnife = character.Inventory.FindItemOfClass(typeof(HuntingKnife)) != null;
			break;
		case FindType.Backpack:
			result.CurrentInventoryWeight = character.Inventory.GetWeight(character);
			break;
		case FindType.Ammo:
		{
			List<EquipmentPolicy> allCarryAmountPolicies = character.GetAllCarryAmountPolicies();
			if (allCarryAmountPolicies == null)
			{
				break;
			}
			result.CarryPolicies = new List<EquipmentPolicy>();
			for (int i = 0; i < allCarryAmountPolicies.Count; i++)
			{
				if (!allCarryAmountPolicies[i].HasTargetCarryAmount(character))
				{
					result.CarryPolicies.Add(allCarryAmountPolicies[i]);
					if (allCarryAmountPolicies[i].Liquid == LiquidPrototype.Water)
					{
						float amountOfLiquidType = character.Inventory.GetAmountOfLiquidType(allCarryAmountPolicies[i].Liquid, allCarryAmountPolicies[i].InfectedWith);
						result.NeedWater = allCarryAmountPolicies[i].TargetAmount - amountOfLiquidType;
					}
					else if (allCarryAmountPolicies[i].Liquid == LiquidPrototype.Snow)
					{
						float amountOfLiquidType2 = character.Inventory.GetAmountOfLiquidType(allCarryAmountPolicies[i].Liquid, allCarryAmountPolicies[i].InfectedWith);
						result.NeedSnow = allCarryAmountPolicies[i].TargetAmount - amountOfLiquidType2;
					}
					else if (allCarryAmountPolicies[i].Liquid == LiquidPrototype.Urine)
					{
						float amountOfLiquidType3 = character.Inventory.GetAmountOfLiquidType(allCarryAmountPolicies[i].Liquid, allCarryAmountPolicies[i].InfectedWith);
						result.NeedUrine = allCarryAmountPolicies[i].TargetAmount - amountOfLiquidType3;
					}
				}
			}
			break;
		}
		}
		if (result.NeedWater > 0f)
		{
			if (FindType == FindType.WaterForCrops)
			{
				result.BestWaterBottle = character.Inventory.GetBestWateringCan();
			}
			else
			{
				result.BestWaterBottle = character.Inventory.GetBestLiquidContainerToFill(LiquidPrototype.Water, result.NeedWater);
			}
		}
		if (result.NeedSnow > 0f)
		{
			result.BestWaterBottle = character.Inventory.GetBestLiquidContainerToFill(LiquidPrototype.Snow, result.NeedSnow);
		}
		if (result.NeedUrine > 0f)
		{
			result.BestWaterBottle = character.Inventory.GetBestLiquidContainerToFill(LiquidPrototype.Urine, result.NeedUrine);
		}
		result.NeedWood = IsLookingForWood(character);
		if (result.NeedWood)
		{
			result.HasAxe = character.Inventory.GetAxe() != null;
		}
		result.NeedMiningResource = IsLookingForMiningResources(character);
		if (result.NeedMiningResource != null)
		{
			result.HasPickaxe = character.Inventory.GetPickaxe() != null;
		}
		return result;
	}

	private float CalcCostForFailedAttempts(Character character, Prop prop, int entranceIndex)
	{
		TimeSpan maxTimeSinceLastAttempt = RememberFailedAttemptTime;
		if (prop.Community == character.Community && prop.Community != null && FindType == FindType.Food)
		{
			maxTimeSinceLastAttempt = TimeSpan.FromSeconds(60.0);
		}
		if (character.HasFailedFindAttempt(prop, this, maxTimeSinceLastAttempt, out var timeSinceAttempt, out var failCount))
		{
			return (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
		}
		return 0f;
	}

	public static void BuildListOfEscapedFromTargets(Character character)
	{
		EscapedFromTargets.Clear();
		if (character.IsControllableByPlayer())
		{
			return;
		}
		foreach (Target target in character.Targets)
		{
			if (target.GetFlag(TargetFlags.EscapedFromTarget) && !target.HasAnyFlag((TargetFlags)8388672) && target.Object != null && character.IsEnemy(target.Object))
			{
				EscapedFromTargets.Add(target);
			}
		}
	}

	public static void ClearListOfEscapedFromTargets()
	{
		EscapedFromTargets.Clear();
	}

	public static bool IsNearSomeoneWeJustRanAwayFrom(Character character, Vector3 pos)
	{
		Vector2 posXZ = character.PosXZ;
		Vector2 vector = MathUtil.ToXZ(pos);
		foreach (Target escapedFromTarget in EscapedFromTargets)
		{
			Vector2 vector2 = MathUtil.ToXZ(escapedFromTarget.Object.Pos);
			if (Vector2.Dot(MathUtil.SafeNormalize(vector - posXZ, Vector2.zero), MathUtil.SafeNormalize(vector2 - posXZ, Vector2.zero)) >= MinDotFromSomeoneWeRanAwayFrom && MathUtil.GetDistSqFromSegmentToPoint(posXZ, vector, vector2) <= MinDistSqFromSomeoneWeRanAwayFrom)
			{
				return true;
			}
		}
		return false;
	}

	private Goal GetFirstSubGoal(Character character, Goal parent, bool findToolFailed, bool failedToDepositStuff)
	{
		using (new ProfileMarker(_Timer))
		{
			GameTerrain terrain = GameTerrain.Instance;
			Session instance = Session.Instance;
			CustomRandom deterministicRand = instance.DeterministicRand;
			FindData findData = SetupFindData(character);
			ChoseToFindTool = false;
			Vector2 posXZ = character.PosXZ;
			TerrainCoord tile = character.Tile;
			bool controllableByPlayer = character.IsControllableByPlayer();
			bool flag = Critical && (!character.HasPersonality(CachedPersonalityType.Moral) || character.IsLooter());
			float num = ((!character.HasPersonality(CachedPersonalityType.Immoral)) ? (controllableByPlayer ? 1080f : 240f) : (controllableByPlayer ? 120f : 60f));
			float num2 = (controllableByPlayer ? 120f : 60f);
			TileObject best = null;
			Equipment bestItem = null;
			EquipmentPrototype bestResourceProto = null;
			LiquidPrototype bestResourceLiquid = null;
			SpeechSituation bestSpeechSituation = SpeechSituation.None;
			TerrainCoord bestTile = TerrainCoord.Invalid;
			int bestEntranceIndex = 0;
			int bestTerrainPathIndex = -1;
			float bestCost = (CanTravelFar ? float.MaxValue : ((FindType == FindType.Ammo || FindType == FindType.Clothing) ? 40f : (Critical ? float.MaxValue : (controllableByPlayer ? 80f : 200f))));
			if (MaxDistToTravel < float.MaxValue)
			{
				bestCost = Math.Min(bestCost, MaxDistToTravel / Character.WalkSpeed);
				if (findData.NeedWood)
				{
					bestCost += TreeChopCost;
				}
			}
			float bestTreeCost = float.MaxValue;
			float bestMiningCost = float.MaxValue;
			float num3 = float.MaxValue;
			float num4 = float.MaxValue;
			float num5 = float.MaxValue;
			int bestMaxAmount = int.MaxValue;
			bool bestIsTooHeavy = false;
			if (findData.NeedWater > 0f && CheckEquipmentPolicyForCraftingResources && !character.IsActionAllowedForItem(null, LiquidPrototype.Water, InfectionType.None, EquipmentPolicyAction.CanCraftWith, includeCommunityPolicy: true))
			{
				return null;
			}
			if (findData.NeedSnow > 0f && CheckEquipmentPolicyForCraftingResources && !character.IsActionAllowedForItem(null, LiquidPrototype.Snow, InfectionType.None, EquipmentPolicyAction.CanCraftWith, includeCommunityPolicy: true))
			{
				return null;
			}
			if (findData.NeedUrine > 0f && CheckEquipmentPolicyForCraftingResources && !character.IsActionAllowedForItem(null, LiquidPrototype.Urine, InfectionType.None, EquipmentPolicyAction.CanCraftWith, includeCommunityPolicy: true))
			{
				return null;
			}
			if (FindType != FindType.Ammo && FindType != FindType.SealedContainer && FindType != FindType.FoodForJourney && FindType != FindType.WaterBottle && FindType != FindType.WaterForCrops && FindType != FindType.Backpack)
			{
				float score = 0f;
				bool tooHeavy = false;
				int amountToTransfer = 0;
				Equipment equipment = FindItemInInventory(character, character, findData, ref tooHeavy, ref score, ref amountToTransfer);
				if (equipment != null && (FindType != FindType.WaterForJourney || !(equipment.GetLiquidContentsAmount() < equipment.GetLiquidCapacity() - 0.001f)) && (FindType != FindType.Prototype || equipment.GetAmount() >= DesiredAmountOfRecipe))
				{
					if (FindType == FindType.Food && character.SquadLeader == null && character.SquadId == 0)
					{
						best = character;
						bestItem = equipment;
						bestCost = 0f - score;
					}
					else if (FindType == FindType.Bandage)
					{
						best = character;
						bestItem = equipment;
						bestCost = score;
					}
					else
					{
						if (FindType != FindType.SealedContainer && FindType != FindType.WaterBottle)
						{
							FoundItem = equipment;
							Result = FindResult.Success;
							Finished = true;
							return null;
						}
						best = character;
						bestItem = equipment;
						bestCost = EquipmentContainer.CalcNormalizedContainerScore(score);
					}
				}
			}
			BuildListOfEscapedFromTargets(character);
			CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
			foreach (Character character3 in Session.Instance.CharacterManager.Characters)
			{
				if (character3 == character)
				{
					continue;
				}
				float num6 = (MathUtil.ToXZ(character3.Position) - posXZ).magnitude / Character.WalkSpeed;
				if (character3.Alive)
				{
					if (character3.Community == character.Community || character3.CanFollowPlayer)
					{
						if (character3.IsDoingSomethingTerriblyImportant())
						{
							continue;
						}
						num6 *= 0.5f;
					}
					else if (character.IsAlly(character3) && !controllableByPlayer)
					{
						if (character3.IsDoingSomethingTerriblyImportant())
						{
							continue;
						}
						num6 += 30f;
					}
					else
					{
						num6 = ((character.Community == null || character.Community.CommunityType != CommunityType.RovingTrader) ? (num6 + 120f) : (num6 + 10f));
						if (character3.Consciousness == Consciousness.Unconscious)
						{
							if (!flag)
							{
								continue;
							}
							num6 += num;
						}
						if (FindType == FindType.BuildingResources || FindType == FindType.CraftingResources || (FindType == FindType.Prototype && controllableByPlayer) || FindType == FindType.Alcohol)
						{
							continue;
						}
					}
					if ((FindType == FindType.Clothing && character3.Community != character.Community && !character.IsControllableByOrFollowingPlayer()) || FindType == FindType.WaterForCrops)
					{
						continue;
					}
				}
				else
				{
					if (character3.Community != null && character3.Community != character.Community && character3.Community.HasAnyActiveMembers() && (!character.IsAlly(character3) || controllableByPlayer) && Session.Instance.CommunityManager.GetRelationship(character.Community, character3.Community) != CommunityRelationshipType.Hostile)
					{
						if (!flag)
						{
							continue;
						}
						num6 += num;
					}
					if (character3.Community == character.Community && instance.PlayTime - character3.TimeOfDeath < TimeSpan.FromSeconds(20.0) && !Critical)
					{
						continue;
					}
					if (FindType == FindType.WaterForCrops)
					{
						num6 += 120f;
					}
				}
				num6 += 10f;
				if (findData.NeedWater > 0f)
				{
					num6 += 10f;
					if (character3.Alive)
					{
						num6 += 20f;
					}
					if (findData.BestWaterBottle != null)
					{
						num6 += 30f;
					}
				}
				if ((FindType != FindType.Food && FindType != FindType.FoodForJourney && FindType != FindType.SealedContainer && FindType != FindType.WaterBottle && num6 >= bestCost) || (character.HasMovementZone() && !character.MovementZone.Contains(character3.Tile)) || (OnlyInEnclosedAreas && !Attack.IsInsideEnclosedBase(character3)))
				{
					continue;
				}
				int amountToTransfer2 = int.MaxValue;
				bool tooHeavy2 = false;
				float score2 = 0f;
				Equipment equipment2 = FindItemInInventory(character, character3, findData, ref tooHeavy2, ref score2, ref amountToTransfer2);
				if (equipment2 == null || (controllableByPlayer && !HasInvestigatedCharacter(character3, equipment2)))
				{
					continue;
				}
				if (FindType == FindType.Food || FindType == FindType.FoodForJourney)
				{
					num6 -= score2;
					if (num6 >= bestCost)
					{
						continue;
					}
				}
				else if (FindType == FindType.Bandage)
				{
					num6 += score2;
				}
				else if (FindType == FindType.SealedContainer || FindType == FindType.WaterBottle)
				{
					num6 -= EquipmentContainer.CalcNormalizedContainerScore(score2);
					if (num6 >= bestCost)
					{
						continue;
					}
				}
				if (FindType == FindType.Bandage && character.Community != null && character.Community.FindThreatByLocation(character3.Tile, 32f) != null)
				{
					continue;
				}
				if (character3.IsConscious)
				{
					if (character3.Zombie || character3.GetBaseObjectType() != BaseObjectType.Human || character.WantToAvoidCommunity(character3.Community))
					{
						continue;
					}
					switch (FindType)
					{
					case FindType.Ammo:
					case FindType.Clothing:
						if (character.Community != null && character.Community.IsAnyMemberFindingFrom(FindType, character3, character))
						{
							continue;
						}
						break;
					case FindType.Prototype:
						if (character.Community != null && character.Community.AreTooManyMembersFindingFrom(ProtoToFind, character3, character))
						{
							continue;
						}
						break;
					}
					if (((FindType == FindType.FoodForJourney || FindType == FindType.WaterForJourney || (FindType == FindType.SealedContainer && parent is FindGoal && (((FindGoal)parent).FindType == FindType.FoodForJourney || ((FindGoal)parent).FindType == FindType.WaterForJourney))) && character.SquadId != 0 && character3.SquadId == character.SquadId) || (FindType != FindType.Ammo && !CanTransferFromCharacter(character3, equipment2, FindType, out amountToTransfer2)))
					{
						continue;
					}
					if (character.Community != character3.Community && ((FindType == FindType.Bandage) ? ((float)character3.GetPriceToBandage(character)) : Math.Max(1f, character3.GetPriceToSell(equipment2, character))) > (float)character.Inventory.GetGoldAmount())
					{
						if (!Critical || character3.HasMemoryAfter(MemoryPrototype.Begged, character, character3, Session.Instance.PlayTime - Sun.DayLength))
						{
							continue;
						}
						num6 += num2;
					}
					if (character3.FindActiveGoal(GoalType.Conversation) is Conversation conversation && conversation.GetTargetCharacter() != character)
					{
						continue;
					}
				}
				else if (character.WantToAvoidCommunity(character3.GetCommunityThatOwnsThisArea()))
				{
					continue;
				}
				if (!character.HasFailedFindAttempt(character3, this, RememberFailedAttemptTime, out var _, out var _) && !IsNearSomeoneWeJustRanAwayFrom(character, character3.Pos))
				{
					bestCost = num6;
					best = character3;
					bestItem = equipment2;
					bestResourceProto = null;
					bestResourceLiquid = null;
					bestTile = TerrainCoord.Invalid;
					bestEntranceIndex = 0;
					bestTerrainPathIndex = -1;
					bestIsTooHeavy = tooHeavy2;
					bestMaxAmount = amountToTransfer2;
					bestSpeechSituation = ((character3.Community == character.Community && character3.IsAwake) ? SpeechSituation.FindingItem : (Critical ? SpeechSituation.ScavengingItemCritical : SpeechSituation.ScavengingItem));
				}
			}
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				Equipment equipment3 = null;
				float score3 = 0f;
				bool tooHeavy3 = false;
				int amountToTransfer3 = 0;
				Building building = allProp as Building;
				int num7 = ((building == null) ? 1 : building.GetEntranceDefs().Length);
				for (int i = 0; i < num7; i++)
				{
					Vector2 vector = ((building != null) ? MathUtil.ToXZ(building.GetEntrancePos(i)) : allProp.PosXZ);
					float num8 = (vector - posXZ).magnitude / Character.WalkSpeed;
					if (allProp.Community == character.Community)
					{
						num8 *= 0.5f;
						if ((FindType == FindType.Food || FindType == FindType.FoodForJourney) && allProp.GetBaseObjectType() == BaseObjectType.Campfire)
						{
							num8 -= 10f;
						}
						if (findData.NeedWater > 0f && allProp.GetBaseObjectType() == BaseObjectType.Well)
						{
							num8 -= 20f;
							if (terrain.IsTileEnclosed(tile))
							{
								TerrainCoord nearestPassableTileTo = allProp.GetNearestPassableTileTo(tile, 0, character, null);
								if (terrain.IsTileEnclosed(nearestPassableTileTo))
								{
									num8 -= 20f;
								}
							}
						}
					}
					else
					{
						if ((FindType == FindType.Clothing || FindType == FindType.Alcohol) && !controllableByPlayer)
						{
							i = num7;
							continue;
						}
						if (allProp.Community != null && allProp.Community.HasAnyActiveMembers() && allProp.GetBaseObjectType() != BaseObjectType.Well && (!character.IsAlly(allProp) || controllableByPlayer))
						{
							if (!flag)
							{
								i = num7;
								continue;
							}
							num8 += num;
						}
					}
					if (findData.NeedWater > 0f && findData.BestWaterBottle != null && allProp.GetBaseObjectType() != BaseObjectType.Well)
					{
						num8 += 40f;
					}
					if (FindType != FindType.Food && FindType != FindType.FoodForJourney && num8 >= bestCost)
					{
						continue;
					}
					if (building != null && building.GetPropPrototype().IsEnterableBySpecies(character.GetBaseObjectType()))
					{
						if (!building.CouldEnterIfNotFull(character))
						{
							i = num7;
							continue;
						}
						if (character.HasMovementZone() && !character.MovementZone.Contains(terrain.GetTileCoordForPosXZ(vector)))
						{
							i = num7;
							continue;
						}
						if (OnlyInEnclosedAreas && !terrain.IsTileEnclosed(terrain.GetTileCoordForPosXZ(vector)))
						{
							i = num7;
							continue;
						}
					}
					else
					{
						if (allProp.IsLockable() && allProp.Community != null && character.Community != allProp.Community)
						{
							i = num7;
							continue;
						}
						if (character.HasMovementZone() && !character.MovementZone.Overlaps(allProp.GetTileRect()))
						{
							i = num7;
							continue;
						}
						if (OnlyInEnclosedAreas && !terrain.IsTileEnclosed(allProp.GetNearestPassableTileTo(tile, 0, character, null)))
						{
							i = num7;
							continue;
						}
						if (allProp is AnimalFeederProp)
						{
							i = num7;
							continue;
						}
					}
					if (controllableByPlayer)
					{
						if (allProp.GetMaxInventoryWeight() == 0f)
						{
							if (!GameTerrain.Instance.FogOfWar.IsAnyTileInRectCornersExplored(allProp.MinTile, allProp.MaxTile))
							{
								i = num7;
								continue;
							}
						}
						else if (!allProp.Investigated)
						{
							i = num7;
							continue;
						}
					}
					if (allProp.GetUnderConstructionInfo() != null)
					{
						i = num7;
						continue;
					}
					if (character.WantToAvoidCommunity(allProp.GetCommunityThatOwnsThisArea()))
					{
						i = num7;
						continue;
					}
					if ((FindType == FindType.WaterBottle || FindType == FindType.SealedContainer) && allProp.GetBaseObjectType() == BaseObjectType.Campfire)
					{
						i = num7;
						continue;
					}
					if (FindType == FindType.Bandage && character.Community != null && character.Community.FindThreatByLocation(allProp.Tile, MathUtil.ToXZ(allProp.GetBoundingBox().extents).magnitude + 32f) != null)
					{
						i = num7;
						continue;
					}
					if (FindType == FindType.WaterForCrops && allProp.GetBaseObjectType() != BaseObjectType.Well)
					{
						num8 += 120f;
					}
					if ((findData.NeedWater > 0f || findData.NeedUrine > 0f || findData.NeedSnow > 0f) && findData.BestWaterBottle == null)
					{
						LiquidPrototype liquid = null;
						float neededAmount = 0f;
						if (findData.NeedWater > 0f)
						{
							liquid = LiquidPrototype.Water;
							neededAmount = findData.NeedWater;
						}
						else if (findData.NeedUrine > 0f)
						{
							liquid = LiquidPrototype.Urine;
							neededAmount = findData.NeedUrine;
						}
						else if (findData.NeedSnow > 0f)
						{
							liquid = LiquidPrototype.Snow;
							neededAmount = findData.NeedSnow;
						}
						if (allProp.Inventory.GetBestLiquidContainerToFill(liquid, neededAmount) != null)
						{
							num4 = Math.Min(num4, num8 + CalcCostForFailedAttempts(character, allProp, i));
						}
					}
					if (i == 0)
					{
						equipment3 = FindItemInInventory(character, allProp, findData, ref tooHeavy3, ref score3, ref amountToTransfer3);
					}
					if (equipment3 != null)
					{
						if (allProp.CanTransferEquipmentAway(equipment3) != CantTransferReason.CanTransfer)
						{
							equipment3 = null;
						}
						else if (FindType == FindType.Food || FindType == FindType.FoodForJourney)
						{
							num8 -= score3;
						}
						else if (FindType == FindType.Bandage)
						{
							num8 += score3;
						}
						else if (FindType == FindType.SealedContainer || FindType == FindType.WaterBottle)
						{
							num8 -= EquipmentContainer.CalcNormalizedContainerScore(score3);
						}
					}
					EquipmentPrototype equipmentPrototype = null;
					LiquidPrototype liquidPrototype = null;
					if (equipment3 == null)
					{
						if (findData.NeedWater > 0f && allProp is Well)
						{
							liquidPrototype = LiquidPrototype.Water;
							num3 = Math.Min(num3, num8 + CalcCostForFailedAttempts(character, allProp, i));
							if (findData.BestWaterBottle == null)
							{
								continue;
							}
						}
						else if (findData.NeedUrine > 0f && allProp.GetLiquidType() == LiquidPrototype.Urine && allProp.GetLiquidAmount() > 0f)
						{
							liquidPrototype = allProp.GetLiquidType();
							num5 = Math.Min(num5, num8);
							if (findData.BestWaterBottle == null)
							{
								continue;
							}
						}
						else if (findData.NeedWood && allProp.GetBaseObjectType() == BaseObjectType.FallenTreeProp && LumberjackGoal.IsChoppableTree(allProp))
						{
							Community communityThatOwnsThisArea = allProp.GetCommunityThatOwnsThisArea();
							if (communityThatOwnsThisArea != null && communityThatOwnsThisArea != character.Community && communityThatOwnsThisArea.HasAnyLivingNonZombieMembers() && !communityThatOwnsThisArea.CachedAllies.Contains(character.Community))
							{
								continue;
							}
							bestTreeCost = Math.Min(bestTreeCost, num8);
							if (!findData.HasAxe)
							{
								continue;
							}
							equipmentPrototype = EquipmentPrototype.Wood;
							tooHeavy3 = !CanIgnoreWeight(character) && !character.HasInventorySpaceFor(EquipmentPrototype.Wood.Weight);
						}
						else if (findData.NeedMiningResource != null && (findData.NeedMiningResource == allProp.GetMiningResourceType() || (allProp.GetBaseObjectType() == BaseObjectType.Mine && ((Mine)allProp).CanEnter(character))))
						{
							if (allProp is Mine mine && !mine.HasRichDeposits(findData.NeedMiningResource.GetMineralType()))
							{
								continue;
							}
							Community communityThatOwnsThisArea2 = allProp.GetCommunityThatOwnsThisArea();
							if (communityThatOwnsThisArea2 != null && communityThatOwnsThisArea2 != character.Community && communityThatOwnsThisArea2.HasAnyLivingNonZombieMembers() && !communityThatOwnsThisArea2.CachedAllies.Contains(character.Community))
							{
								continue;
							}
							num8 += AnimationManager.Instance.Anims[141][0].Clip.length * (float)allProp.GetMiningProgressNeededToExtract(findData.NeedMiningResource.GetMineralType());
							bestMiningCost = Math.Min(bestMiningCost, num8);
							if (!findData.HasPickaxe)
							{
								continue;
							}
							equipmentPrototype = findData.NeedMiningResource;
							tooHeavy3 = !CanIgnoreWeight(character) && !character.HasInventorySpaceFor(findData.NeedMiningResource.Weight);
						}
						else if (FindType == FindType.Food || FindType == FindType.FoodForJourney)
						{
							if (!(allProp is CraftingProp craftingProp) || !craftingProp.IsCrafting() || !craftingProp.CraftingRecipe.IsProductEdible(includeSugar: false, includeHumanMeat: true))
							{
								continue;
							}
							equipmentPrototype = craftingProp.CraftingRecipe.ProductPrototype;
							liquidPrototype = craftingProp.CraftingRecipe.ProductLiquidPrototype;
							num8 += 40f;
						}
						else
						{
							if (FindType != FindType.Drink || !(allProp is CraftingProp craftingProp2) || !craftingProp2.IsCrafting() || craftingProp2.CraftingRecipe.ProductLiquidPrototype != LiquidPrototype.Water)
							{
								continue;
							}
							equipmentPrototype = craftingProp2.CraftingRecipe.ProductPrototype;
							liquidPrototype = craftingProp2.CraftingRecipe.ProductLiquidPrototype;
							num8 += 40f;
						}
					}
					if (building != null && num7 > 0)
					{
						TerrainCoord entranceTile = building.GetEntranceTile(i);
						if (terrain.IsImpassable(entranceTile.x, entranceTile.y, 1024, character, null))
						{
							continue;
						}
					}
					num8 += CalcCostForFailedAttempts(character, allProp, i);
					if (!(num8 >= bestCost) && !IsNearSomeoneWeJustRanAwayFrom(character, allProp.Pos))
					{
						bestCost = num8;
						best = allProp;
						bestEntranceIndex = i;
						bestItem = equipment3;
						bestResourceProto = equipmentPrototype;
						bestResourceLiquid = liquidPrototype;
						bestIsTooHeavy = tooHeavy3;
						bestMaxAmount = int.MaxValue;
						bestTile = TerrainCoord.Invalid;
						bestTerrainPathIndex = -1;
						bestSpeechSituation = ((allProp is FallenTreeProp) ? SpeechSituation.ChoppingFirewood : ((allProp is Boulder) ? SpeechSituation.Mining : ((allProp is Well) ? SpeechSituation.GettingWaterFromWell : ((allProp.Community == character.Community) ? SpeechSituation.FindingItem : (Critical ? SpeechSituation.ScavengingItemCritical : SpeechSituation.ScavengingItem)))));
					}
				}
			}
			if (findData.NeedWater > 0f && instance.Weather.TemperatureInCelsius >= Weather.RiversDrinkableTemperatureInCelsius)
			{
				foreach (TerrainPath path in terrain.Paths)
				{
					if (!path.IsRiver)
					{
						continue;
					}
					float closestDistSq = float.MaxValue;
					Vector3 closestPointOnPath = Vector3.zero;
					Vector2 closestDirXZOnPath = Vector2.zero;
					float closestPathIndex = 0f;
					if (!path.GetClosestPointOnPathToPos(terrain, posXZ, ref closestDistSq, ref closestPointOnPath, ref closestDirXZOnPath, ref closestPathIndex, mustBeInWater: true))
					{
						continue;
					}
					float num9 = Mathf.Sqrt(closestDistSq) / Character.WalkSpeed;
					if (!(num9 < bestCost))
					{
						continue;
					}
					TerrainCoord tileCoordForPos = terrain.GetTileCoordForPos(closestPointOnPath);
					if ((character.HasMovementZone() && !character.MovementZone.Contains(tileCoordForPos)) || (OnlyInEnclosedAreas && !terrain.IsTileEnclosed(tileCoordForPos)))
					{
						continue;
					}
					if (terrain.IsTileEnclosed(tile) && terrain.IsTileEnclosed(tileCoordForPos))
					{
						num9 -= 20f;
					}
					if (character.HasFailedFindAttemptForTerrainPath(terrain, path.Index, this, RememberFailedAttemptTime, out var timeSinceAttempt2, out var failCount2))
					{
						num9 += (float)MathUtil.Squared(failCount2) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt2.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
						if (num9 >= bestCost)
						{
							continue;
						}
					}
					if (!IsNearSomeoneWeJustRanAwayFrom(character, closestPointOnPath))
					{
						bestCost = num9;
						best = null;
						bestItem = null;
						bestResourceProto = null;
						bestResourceLiquid = LiquidPrototype.Water;
						bestIsTooHeavy = false;
						bestMaxAmount = int.MaxValue;
						bestTile = tileCoordForPos;
						bestEntranceIndex = 0;
						bestTerrainPathIndex = path.Index;
						bestSpeechSituation = SpeechSituation.GettingWaterFromRiver;
					}
				}
			}
			if (findData.NeedSnow > 0f && instance.Weather.SnowOnGroundAmount >= Weather.ScoopableSnowOnGroundAmount)
			{
				TerrainCoord terrainCoord = TerrainCoord.Invalid;
				for (int j = 1; j < 64; j++)
				{
					if (!(terrainCoord == TerrainCoord.Invalid))
					{
						break;
					}
					for (int k = 0; k < j; k++)
					{
						if (!(terrainCoord == TerrainCoord.Invalid))
						{
							break;
						}
						TerrainCoord terrainCoord2 = character.Tile + deterministicRand.RandomTile(new TerrainCoord(-j, -j), new TerrainCoord(j, j));
						if ((!character.HasMovementZone() || character.MovementZone.Contains(terrainCoord2)) && (!OnlyInEnclosedAreas || terrain.IsTileEnclosed(terrainCoord2)) && !GameTerrain.Instance.IsImpassable(terrainCoord2.x, terrainCoord2.y, 5, character, null))
						{
							terrainCoord = terrainCoord2;
						}
					}
				}
				if (terrainCoord != TerrainCoord.Invalid)
				{
					float num10 = terrainCoord.GetDist(character.Tile) / Character.WalkSpeed;
					if (num10 < bestCost)
					{
						bestCost = num10;
						best = null;
						bestItem = null;
						bestResourceProto = null;
						bestResourceLiquid = LiquidPrototype.Snow;
						bestIsTooHeavy = false;
						bestMaxAmount = int.MaxValue;
						bestTile = terrainCoord;
						bestEntranceIndex = 0;
						bestTerrainPathIndex = -1;
						bestSpeechSituation = SpeechSituation.None;
					}
				}
			}
			if (FindType == FindType.Food || FindType == FindType.FoodForAnimals || FindType == FindType.FoodForJourney || FindType == FindType.Seeds || FindType == FindType.CraftingResources || FindType == FindType.Prototype)
			{
				int num11 = ((character.Community != null) ? character.Community.Id : 0);
				foreach (CropPatch patch in instance.CropsManager.Patches)
				{
					if (patch.CropsInPatch.Count == 0)
					{
						continue;
					}
					float num12 = 0f;
					bool flag2 = false;
					bool flag3 = false;
					switch (FindType)
					{
					case FindType.Food:
					case FindType.FoodForJourney:
					{
						EquipmentPrototype harvestPrototype3 = patch.CropsInPatch[0].GetHarvestPrototype();
						flag2 = harvestPrototype3 != null && character.IsActionAllowedForItem(harvestPrototype3, null, InfectionType.None, EquipmentPolicyAction.CanUse);
						break;
					}
					case FindType.FoodForAnimals:
					{
						EquipmentPrototype harvestPrototype2 = patch.CropsInPatch[0].GetHarvestPrototype();
						flag2 = harvestPrototype2 != null && harvestPrototype2.FoodForAnimal != null && harvestPrototype2.FoodForAnimal.Contains(BaseObjectType.Chicken) && character.IsActionAllowedForItem(harvestPrototype2, null, InfectionType.None, EquipmentPolicyAction.CanFeedToAnimals);
						break;
					}
					case FindType.Seeds:
					{
						EquipmentPrototype harvestPrototype = patch.CropsInPatch[0].GetHarvestPrototype();
						EquipmentPrototype harvestSeedsPrototype = patch.CropsInPatch[0].GetHarvestSeedsPrototype();
						if (harvestSeedsPrototype != null)
						{
							flag3 = CropTypesToFindSeedsFor.Contains(harvestSeedsPrototype.GetSeedForPlantType()) && character.IsActionAllowedForItem(harvestSeedsPrototype, null, InfectionType.None, EquipmentPolicyAction.CanPlant);
						}
						else
						{
							flag2 = CropTypesToFindSeedsFor.Contains(harvestPrototype.GetSeedForPlantType()) && character.IsActionAllowedForItem(harvestPrototype, null, InfectionType.None, EquipmentPolicyAction.CanPlant);
						}
						break;
					}
					case FindType.CraftingResources:
						foreach (Ingredient ingredient in FollowingRecipe.Ingredients)
						{
							if (ingredient.Prototypes != null)
							{
								if (ingredient.Prototypes.Contains(patch.CropsInPatch[0].GetHarvestPrototype()) && (!CheckEquipmentPolicyForCraftingResources || character.IsActionAllowedForItem(patch.CropsInPatch[0].GetHarvestPrototype(), null, InfectionType.None, EquipmentPolicyAction.CanCraftWith)) && !ingredient.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null))
								{
									flag2 = true;
									break;
								}
								if (patch.CropsInPatch[0].GetHarvestSeedsPrototype() != null && ingredient.Prototypes.Contains(patch.CropsInPatch[0].GetHarvestSeedsPrototype()) && (!CheckEquipmentPolicyForCraftingResources || character.IsActionAllowedForItem(patch.CropsInPatch[0].GetHarvestSeedsPrototype(), null, InfectionType.None, EquipmentPolicyAction.CanCraftWith)) && !ingredient.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, CheckEquipmentPolicyForCraftingResources ? character : null))
								{
									flag3 = true;
									break;
								}
							}
						}
						break;
					case FindType.Prototype:
						flag2 = patch.CropType != null && patch.CropType.HarvestPrototype == ProtoToFind;
						flag3 = patch.CropType != null && patch.CropType.HarvestSeedsPrototype == ProtoToFind;
						break;
					}
					if (!flag2 && !flag3)
					{
						continue;
					}
					if (patch.CommunityId != 0 && patch.CommunityId != num11)
					{
						Community community = patch.GetCommunity();
						if (community != null && community.HasAnyLivingNonZombieMembers() && (!character.IsAlly(community) || controllableByPlayer))
						{
							if (!Critical || character.WantToAvoidCommunity(patch.GetCommunity()) || !flag)
							{
								continue;
							}
							num12 += num;
						}
					}
					foreach (PlantableCrop item in patch.CropsInPatch)
					{
						if (item.IsDead())
						{
							continue;
						}
						float num13 = num12;
						if (!item.IsRipe() || ((!flag2 || item.HarvestableAmount <= 0) && (!flag3 || item.HarvestableSeedsAmount <= 0)) || (controllableByPlayer && !terrain.FogOfWar.IsTileExplored(item.GetTileX(), item.GetTileY())) || (character.HasMovementZone() && !character.MovementZone.Contains(item.Tile)) || (OnlyInEnclosedAreas && !terrain.IsTileEnclosed(item.Tile)) || character.HasRoleWithTargetLocation(Role.Capturing, item.Tile))
						{
							continue;
						}
						num13 += (item.PosXZ - posXZ).magnitude / Character.WalkSpeed;
						if (character.HasFailedFindAttempt(item, this, RememberFailedAttemptTime, out var timeSinceAttempt3, out var failCount3))
						{
							num13 += (float)MathUtil.Squared(failCount3) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt3.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
						}
						if (!(num13 >= bestCost) && !IsNearSomeoneWeJustRanAwayFrom(character, item.Pos))
						{
							bool flag4 = false;
							if (!CanIgnoreWeight(character))
							{
								flag4 = flag2 && !character.HasInventorySpaceFor(item.GetHarvestWeight()) && flag3 && !character.HasInventorySpaceFor(item.GetHarvestSeedsWeight());
							}
							bestCost = num13;
							best = item;
							bestItem = null;
							bestResourceProto = (flag2 ? item.GetHarvestPrototype() : (flag3 ? item.GetHarvestSeedsPrototype() : null));
							bestResourceLiquid = null;
							bestIsTooHeavy = flag4;
							bestMaxAmount = int.MaxValue;
							bestTile = TerrainCoord.Invalid;
							bestEntranceIndex = 0;
							bestTerrainPathIndex = -1;
							bestSpeechSituation = ((item.CommunityId == 0 || item.CommunityId == num11) ? SpeechSituation.PickingCrops : SpeechSituation.StealingCrops);
						}
					}
				}
			}
			if (findData.NeedWood)
			{
				GameTerrain.Instance.TreeMapWho.GetNearestObject(character.Tile, (bestCost == float.MaxValue) ? int.MaxValue : Mathf.CeilToInt(bestCost * Character.WalkSpeed), (TreeProp tree) => CheckTree(character, controllableByPlayer, findData.HasAxe, tree, ref bestCost, ref bestTreeCost, ref best, ref bestItem, ref bestResourceProto, ref bestResourceLiquid, ref bestIsTooHeavy, ref bestMaxAmount, ref bestTile, ref bestEntranceIndex, ref bestTerrainPathIndex, ref bestSpeechSituation));
			}
			if (findData.NeedMiningResource != null)
			{
				GameTerrain.Instance.RockMapWho.GetNearestObject(character.Tile, (bestCost == float.MaxValue) ? int.MaxValue : Mathf.CeilToInt(bestCost * Character.WalkSpeed), (SingleTileObject rock) => (rock.GetMiningResourceType() == findData.NeedMiningResource || rock.GetGrabbableEquipmentType() == findData.NeedMiningResource) && CheckTree(character, controllableByPlayer, findData.HasPickaxe, rock, ref bestCost, ref bestMiningCost, ref best, ref bestItem, ref bestResourceProto, ref bestResourceLiquid, ref bestIsTooHeavy, ref bestMaxAmount, ref bestTile, ref bestEntranceIndex, ref bestTerrainPathIndex, ref bestSpeechSituation));
			}
			if (FindType == FindType.Ammo && findData.CarryPolicies != null)
			{
				bool flag5 = false;
				for (int num14 = 0; num14 < findData.CarryPolicies.Count; num14++)
				{
					if (findData.CarryPolicies[num14].Proto != null && findData.CarryPolicies[num14].Proto.TypeName == BaseObjectType.Arrow)
					{
						flag5 = true;
						break;
					}
				}
				if (flag5)
				{
					GameTerrain.Instance.ArrowMapWho.GetNearestObject(character.Tile, (bestCost == float.MaxValue) ? int.MaxValue : Mathf.CeilToInt(bestCost * Character.WalkSpeed), delegate(ArrowProp arrow)
					{
						bool flag8 = false;
						for (int l = 0; l < findData.CarryPolicies.Count; l++)
						{
							if (findData.CarryPolicies[l].Proto == arrow.GetGrabbableEquipmentType() && findData.CarryPolicies[l].InfectedWith == InfectionType.None)
							{
								flag8 = true;
								break;
							}
						}
						if (!flag8)
						{
							return false;
						}
						float num21 = (arrow.PosXZ - character.PosXZ).magnitude / Character.WalkSpeed;
						if (num21 >= bestCost)
						{
							return false;
						}
						if (controllableByPlayer && !GameTerrain.Instance.FogOfWar.IsTileExplored(arrow.Tile.x, arrow.Tile.y))
						{
							return false;
						}
						if (character.HasMovementZone() && !character.MovementZone.Contains(arrow.Tile))
						{
							return false;
						}
						if (OnlyInEnclosedAreas && !terrain.IsTileEnclosed(arrow.Tile))
						{
							return false;
						}
						if (character.WantToAvoidCommunity(arrow.GetCommunityThatOwnsThisArea()))
						{
							return false;
						}
						if (character.HasFailedFindAttempt(arrow, this, RememberFailedAttemptTime, out var timeSinceAttempt4, out var failCount4))
						{
							num21 += (float)MathUtil.Squared(failCount4) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt4.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
							if (num21 >= bestCost)
							{
								return false;
							}
						}
						if (IsNearSomeoneWeJustRanAwayFrom(character, arrow.Pos))
						{
							return false;
						}
						if (arrow.GetGrabbableEquipmentType() == null)
						{
							return false;
						}
						bool flag9 = !CanIgnoreWeight(character) && !character.HasInventorySpaceFor(arrow.GetGrabbableEquipmentType().Weight);
						bestCost = num21;
						best = arrow;
						bestItem = null;
						bestResourceProto = arrow.GetGrabbableEquipmentType();
						bestResourceLiquid = null;
						bestIsTooHeavy = flag9;
						bestMaxAmount = int.MaxValue;
						bestTile = TerrainCoord.Invalid;
						bestEntranceIndex = 0;
						bestTerrainPathIndex = -1;
						bestSpeechSituation = SpeechSituation.FindingItem;
						return true;
					});
				}
			}
			ClearListOfEscapedFromTargets();
			if (best == character)
			{
				if (FindType == FindType.Bandage && bestItem.GetPrototype().BandageLevel == -1)
				{
					return GetCraftBandageGoal(character, bestItem);
				}
				FoundItem = bestItem;
				Result = FindResult.Success;
				Finished = true;
				return null;
			}
			int num15 = 1;
			switch (FindType)
			{
			case FindType.Food:
				num15 = DefaultFindFoodAmount;
				if (bestItem != null && bestItem.GetWeight() > 1f)
				{
					num15 = Math.Min(num15, Math.Max(1, Mathf.CeilToInt((float)num15 / bestItem.GetWeight())));
				}
				if (bestItem != null && character.IsActionAllowedForItem(bestItem, EquipmentPolicyAction.AutoDeposit))
				{
					num15 = Math.Min(num15, Math.Max(1, Mathf.CeilToInt(character.GetHunger() / bestItem.GetNutrition())));
				}
				break;
			case FindType.FoodForJourney:
				num15 = DefaultFindFoodAmount;
				break;
			case FindType.FoodForAnimals:
				if (bestItem != null)
				{
					num15 = CalcDesiredFoodAmountForAnimals(character, bestItem);
				}
				break;
			case FindType.Bandage:
				num15 = 6;
				break;
			case FindType.Ammo:
				if (bestItem != null)
				{
					int num16 = (int)character.GetTargetAmountToCarryIncludingAmmo(bestItem.GetPrototype(), null, bestItem.InfectedWith);
					int num17 = character.Inventory.CountItemsOfType(bestItem.GetPrototype(), bestItem.InfectedWith);
					num15 = Math.Max(num15, num16 - num17);
					if (bestItem.GetPrototype().IsAmmo() && character.Inventory.IsOutOfAmmo(character))
					{
						MovementType = MovementType.Run;
					}
					else
					{
						MovementType = MovementType.Walk;
					}
				}
				break;
			case FindType.Seeds:
				if (bestItem != null)
				{
					num15 = bestItem.GetAmount();
					num15 = Math.Max(1, Math.Min(num15, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / bestItem.GetWeight())));
					num15 = Math.Min(num15, FarmingGoal.MaxSeedsToCarry);
				}
				break;
			case FindType.Prototype:
				num15 = Math.Max(num15, DesiredAmountOfRecipe);
				break;
			}
			num15 = Math.Min(num15, bestMaxAmount);
			bool flag6 = best != null || bestTile != TerrainCoord.Invalid;
			EquipmentPrototype equipmentPrototype2 = ((best != null) ? best.GetGrabbableEquipmentType() : null);
			Prop prop = best as Prop;
			CraftingProp craftingProp3 = best as CraftingProp;
			Well well = best as Well;
			PlantableCrop plantableCrop = best as PlantableCrop;
			Campfire campfire = best as Campfire;
			Character character2 = best as Character;
			int num18 = -1;
			Recipe recipe = null;
			if (FindType == FindType.Food || FindType == FindType.Drink)
			{
				CraftGoal craftGoal = character.GetCraftGoal();
				if (craftGoal != null && Session.Instance.PlayTime - craftGoal._lastAttemptedTime >= CraftGoal.MinTimeBetweenAttempts && (!flag6 || (best != null && best.GetCommunity() != character.Community) || (craftingProp3 != null && craftingProp3.IsCrafting())))
				{
					for (int num19 = 0; num19 < character.Roles.Count; num19++)
					{
						RoleInfo roleInfo = character.Roles[num19];
						if (roleInfo.Paused || roleInfo.FailedRecently || Session.Instance.PlayTime - character.Roles[num19].LastFailedTime < CraftGoal.MinTimeBetweenAttempts)
						{
							continue;
						}
						bool flag7 = false;
						if (roleInfo.Role == Role.Cook)
						{
							flag7 = true;
						}
						else if (roleInfo.Role == Role.Crafter && roleInfo.Recipe != null)
						{
							if (FindType == FindType.Food && roleInfo.Recipe.IsProductEdible(includeSugar: false, includeHumanMeat: true) && character.IsActionAllowedForItem(roleInfo.Recipe.ProductPrototype, null, InfectionType.None, EquipmentPolicyAction.CanUse))
							{
								flag7 = true;
							}
							else if (FindType == FindType.Drink && roleInfo.Recipe.ProductLiquidPrototype != null && roleInfo.Recipe.ProductLiquidPrototype.WaterContent > 0f && character.IsActionAllowedForItem(null, roleInfo.Recipe.ProductLiquidPrototype, InfectionType.None, EquipmentPolicyAction.CanUse))
							{
								flag7 = true;
							}
						}
						if (!flag7)
						{
							continue;
						}
						CraftingProp craftingProp4 = null;
						if (roleInfo.TargetLocation != TerrainCoord.Invalid)
						{
							craftingProp4 = GameTerrain.Instance.GetCraftingPropOnTile(roleInfo.TargetLocation.x, roleInfo.TargetLocation.y);
							if (craftingProp4 == null || craftingProp4.IsCrafting())
							{
								continue;
							}
						}
						if (roleInfo.Role == Role.Cook)
						{
							recipe = CraftGoal.PickCookRecipe(character, craftingProp4, checkIfAllowedToEatIt: true, prioritiseSkinning: true, Session.Instance.DeterministicRand);
							if (recipe == null)
							{
								continue;
							}
						}
						else
						{
							if (!roleInfo.Recipe.CommunityHasEnoughSpareIngredientsWithinMovementZone(character))
							{
								continue;
							}
							recipe = roleInfo.Recipe;
						}
						num18 = num19;
						break;
					}
				}
			}
			if ((bestSpeechSituation == SpeechSituation.ScavengingItem || bestSpeechSituation == SpeechSituation.ScavengingItemCritical) && character.IsControllableByPlayer() && HintManager.Instance.Hints[21].CanShowHint() && instance.FollowerCommandsEnabled && instance.HitTheRoadCount == 0 && GameImpl.Instance.Settings.HintsEnabled && character.Community.GetConsciousNonZombieMemberCount() > 1 && character.SquadLeader == null && !character.DirectControlled)
			{
				TerrainCoord other = ((best != null) ? best.GetTile() : bestTile);
				if (character.Tile.GetDistSquared(other) >= MathUtil.Squared(48f))
				{
					HintManager.Instance.Hints[21].StartShowing(GameImpl.Translate(HintManager.HINT_SetZone));
				}
			}
			SpeechSituation sit = SpeechSituation.AskForItem;
			switch (FindType)
			{
			case FindType.Food:
			case FindType.FoodForJourney:
				sit = SpeechSituation.AskForFood;
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingFood;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem)
				{
					bestSpeechSituation = SpeechSituation.ScavengingFood;
				}
				break;
			case FindType.Drink:
				sit = SpeechSituation.AskForDrink;
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingWater;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem)
				{
					bestSpeechSituation = SpeechSituation.ScavengingWater;
				}
				break;
			case FindType.Bandage:
				sit = SpeechSituation.AskForBandage;
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingBandages;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem)
				{
					bestSpeechSituation = SpeechSituation.ScavengingBandages;
				}
				break;
			case FindType.Antigen:
				sit = SpeechSituation.AskForAntigen;
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingAntigen;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem)
				{
					bestSpeechSituation = SpeechSituation.ScavengingAntigen;
				}
				break;
			case FindType.Ammo:
				if (bestItem != null && bestItem.GetPrototype().IsAmmo())
				{
					if (bestSpeechSituation == SpeechSituation.FindingItem)
					{
						bestSpeechSituation = SpeechSituation.FindingAmmo;
					}
					if (bestSpeechSituation == SpeechSituation.ScavengingItem)
					{
						bestSpeechSituation = SpeechSituation.ScavengingAmmo;
					}
				}
				break;
			case FindType.Lighter:
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingLighter;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem)
				{
					bestSpeechSituation = SpeechSituation.ScavengingLighter;
				}
				break;
			case FindType.Clothing:
				if (bestSpeechSituation == SpeechSituation.FindingItem)
				{
					bestSpeechSituation = SpeechSituation.FindingClothes;
				}
				if (bestSpeechSituation == SpeechSituation.ScavengingItem || bestSpeechSituation == SpeechSituation.ScavengingItemCritical)
				{
					bestSpeechSituation = SpeechSituation.ScavengingClothes;
				}
				break;
			}
			FoundResource = bestResourceProto;
			Goal goal = null;
			if (num18 != -1)
			{
				CraftGoal craftGoal2 = new CraftGoal();
				craftGoal2.StartRecipe(character, recipe, null, 1, character.Roles[num18].TargetLocation, Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: false, urgent: false);
				goal = craftGoal2;
				CraftingRole = character.Roles[num18];
			}
			else if (bestIsTooHeavy && FindType != FindType.Backpack)
			{
				float num20 = float.MaxValue;
				if (bestItem != null)
				{
					num20 = bestItem.GetWeight() * (float)num15;
				}
				else if (bestResourceProto != null)
				{
					num20 = bestResourceProto.Weight * (float)num15;
				}
				else if (bestResourceLiquid != null && EquipmentPrototype.PlasticBottle != null && EquipmentPrototype.SealedContainer != null)
				{
					num20 = (bestResourceLiquid.CanPourIntoBottles ? EquipmentPrototype.PlasticBottle.Weight : EquipmentPrototype.SealedContainer.Weight);
				}
				bool failed;
				Goal goal2 = (failedToDepositStuff ? null : GatherGoal.GetMoveToAndDepositGoal(character, num20, num20, MovementType, out failed));
				goal = ((goal2 == null) ? new FindGoal(FindType.Backpack, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel,
					ToCarryWeight = num20
				} : goal2);
			}
			else if (findData.NeedWater > 0f && findData.BestWaterBottle == null && num3 != float.MaxValue && num4 != float.MaxValue && (!flag6 || num3 + num4 < bestCost - 1f) && !findToolFailed)
			{
				goal = new FindGoal(FindType.WaterBottle, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel
				};
				ChoseToFindTool = best != null;
			}
			else if (findData.NeedUrine > 0f && findData.BestWaterBottle == null && num5 != float.MaxValue && num4 != float.MaxValue && (!flag6 || num5 + num4 < bestCost - 1f) && !findToolFailed)
			{
				goal = new FindGoal(FindType.SealedContainer, LiquidPrototype.Urine, findData.NeedUrine, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel
				};
				ChoseToFindTool = best != null;
			}
			else if (findData.NeedWood && !findData.HasAxe && bestTreeCost != float.MaxValue && (!flag6 || bestTreeCost < bestCost - 50f) && !findToolFailed && MakeRoomForTool(character, EquipmentPrototype.Axe, EquipmentPrototype.Wood, failedToDepositStuff, out goal))
			{
				if (goal == null)
				{
					goal = new FindGoal(FindType.Axe, MovementType, Critical)
					{
						MaxDistToTravel = MaxDistToTravel
					};
					ChoseToFindTool = best != null;
				}
			}
			else if (findData.NeedMiningResource != null && !findData.HasPickaxe && bestMiningCost != float.MaxValue && equipmentPrototype2 != findData.NeedMiningResource && (!flag6 || bestMiningCost < bestCost - 50f) && !findToolFailed && MakeRoomForTool(character, EquipmentPrototype.Pickaxe, findData.NeedMiningResource, failedToDepositStuff, out goal))
			{
				if (goal == null)
				{
					goal = new FindGoal(FindType.Pickaxe, MovementType, Critical)
					{
						MaxDistToTravel = MaxDistToTravel
					};
					ChoseToFindTool = best != null;
				}
			}
			else if (plantableCrop != null)
			{
				HarvestCrops harvestCrops = new HarvestCrops(plantableCrop.Tile, ignoreWeight: true, CalcDontOpenOurGates(character));
				harvestCrops.SetMovementType(character, MovementType);
				harvestCrops.IsGathering = true;
				goal = harvestCrops;
			}
			else if ((FindType == FindType.Food || FindType == FindType.FoodForJourney) && craftingProp3 != null && craftingProp3.IsCrafting())
			{
				goal = ((!(craftingProp3 is Campfire campfire2) || campfire2.IsBurning() || character.Community == null || character.Community.IsAnyMemberLightingFire(campfire2)) ? ((StateMachineGoal)new SitAroundFireGoal(character, craftingProp3, MovementType)) : ((StateMachineGoal)new LightFireGoal(character, null, campfire2, MovementType, canAddMaterialToFire: true)));
			}
			else if (FindType == FindType.Drink && craftingProp3 != null && craftingProp3.IsCrafting())
			{
				goal = ((!(craftingProp3 is Campfire campfire3) || campfire3.IsBurning() || character.Community == null || character.Community.IsAnyMemberLightingFire(campfire3)) ? ((StateMachineGoal)new SitAroundFireGoal(character, craftingProp3, MovementType)) : ((StateMachineGoal)new LightFireGoal(character, null, campfire3, MovementType, canAddMaterialToFire: true)));
			}
			else if ((FindType == FindType.Drink || FindType == FindType.WaterForCrops || FindType == FindType.WaterForAnimals || FindType == FindType.WaterForJourney) && bestTile != TerrainCoord.Invalid)
			{
				goal = ((findData.BestWaterBottle != null) ? new FillLiquidContainer(character, bestTile, bestTerrainPathIndex, findData.BestWaterBottle, MovementType, CalcDontOpenOurGates(character)) : ((FindType != FindType.WaterForCrops && FindType != FindType.WaterForAnimals && FindType != FindType.WaterForJourney) ? ((StateMachineGoal)new MoveToAndDrinkFromRiver(character, bestTile, bestTerrainPathIndex, MovementType, CalcDontOpenOurGates(character))) : ((StateMachineGoal)new FindGoal(FindType.WaterBottle, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel
				})));
			}
			else if ((FindType == FindType.CraftingResources || FindType == FindType.BuildingResources || FindType == FindType.Ammo) && bestTile != TerrainCoord.Invalid)
			{
				goal = ((findData.BestWaterBottle != null) ? ((StateMachineGoal)new FillLiquidContainer(character, bestTile, bestTerrainPathIndex, findData.BestWaterBottle, MovementType, CalcDontOpenOurGates(character))) : ((StateMachineGoal)((!(findData.NeedSnow > 0f)) ? new FindGoal(FindType.WaterBottle, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel
				} : new FindGoal(FindType.SealedContainer, LiquidPrototype.Snow, findData.NeedSnow, MovementType, Critical)
				{
					MaxDistToTravel = MaxDistToTravel
				})));
			}
			else if (findData.NeedWater > 0f && well != null)
			{
				goal = new FillLiquidContainer(character, well.Tile, bestTerrainPathIndex, findData.BestWaterBottle, MovementType, CalcDontOpenOurGates(character));
			}
			else if (findData.NeedUrine > 0f && prop != null && prop.GetLiquidType() == LiquidPrototype.Urine && prop.GetLiquidAmount() > 0f)
			{
				goal = new FillLiquidContainer(character, prop.Tile, bestTerrainPathIndex, findData.BestWaterBottle, MovementType, CalcDontOpenOurGates(character));
			}
			else if ((best is TreeProp || best is FallenTreeProp) && findData.HasAxe)
			{
				goal = new MoveToAndChop(character, best, MovementType, CalcDontOpenOurGates(character));
			}
			else if (findData.NeedMiningResource != null && best != null && (best.GetMiningResourceType() == findData.NeedMiningResource || (best is Mine && bestItem == null)))
			{
				goal = new MoveToAndMine(character, best, findData.NeedMiningResource.GetMineralType(), MovementType, CalcDontOpenOurGates(character));
			}
			else if (equipmentPrototype2 != null)
			{
				goal = new MoveToAndGrab(character, best, MovementType, CalcDontOpenOurGates(character));
			}
			else if (FindType == FindType.Food && campfire != null && campfire.CraftingRecipe != null && campfire.CraftingRecipe.RecipeType == RecipeType.Campfire_Pot && bestItem != null && bestItem.GetPrototype() == EquipmentPrototype.Pot)
			{
				goal = new MoveToAndInteractGoal(character, campfire, InteractionType.EatFromPot, bestItem, MovementType)
				{
					DontOpenOurGates = CalcDontOpenOurGates(character)
				};
			}
			else if (FindType == FindType.Food && prop != null && bestItem != null && bestItem.GetLiquidContentsAmount() > 0f)
			{
				goal = new MoveToAndInteractGoal(character, prop, InteractionType.EatFromPot, bestItem, MovementType)
				{
					DontOpenOurGates = CalcDontOpenOurGates(character)
				};
			}
			else if (campfire != null && campfire.CraftingRecipe != null && campfire.CraftingRecipe.RecipeType == RecipeType.Campfire_Pot && bestItem != null && bestItem.GetPrototype() == EquipmentPrototype.Pot && bestItem.GetLiquidContentsAmount() > 0f && findData.BestWaterBottle != null)
			{
				goal = new MoveToAndInteractGoal(character, campfire, InteractionType.FillFromPot, bestItem, MovementType)
				{
					DontOpenOurGates = CalcDontOpenOurGates(character)
				};
			}
			else if (prop != null)
			{
				Equipment pourIntoContainer = GetPourIntoContainer(character, bestItem);
				if (pourIntoContainer == null)
				{
					goal = ((campfire == null || bestItem.GetPrototype() != EquipmentPrototype.Pot || (FindType == FindType.Prototype && ProtoToFind == EquipmentPrototype.Pot) || (FindType == FindType.Ammo && findData.HasCarryPolicyForItemType(EquipmentPrototype.Pot)) || (FollowingRecipe != null && FollowingRecipe.GetIngredient(EquipmentPrototype.Pot) != null)) ? ((StateMachineGoal)new MoveToAndTake(character, prop, bestEntranceIndex, bestItem, num15, CanIgnoreWeight(character), MovementType)
					{
						DontOpenOurGates = CalcDontOpenOurGates(character)
					}) : ((StateMachineGoal)new FindGoal(FindType.SealedContainer, bestItem.GetLiquidContentsType(), bestItem.GetLiquidContentsAmount(), MovementType, Critical)
					{
						MaxDistToTravel = MaxDistToTravel
					}));
				}
				else
				{
					float desiredAmount = Math.Min(bestItem.GetLiquidContentsAmount(), pourIntoContainer.GetLiquidCapacity() - pourIntoContainer.GetLiquidContentsAmount());
					goal = new MoveToAndTake(character, prop, bestEntranceIndex, bestItem, desiredAmount, CanIgnoreWeight(character), MovementType, pourIntoContainer)
					{
						DontOpenOurGates = CalcDontOpenOurGates(character)
					};
				}
			}
			else if (character2 != null)
			{
				if (character2.Consciousness >= Consciousness.Unconscious)
				{
					Equipment pourIntoContainer2 = GetPourIntoContainer(character, bestItem);
					if (pourIntoContainer2 != null)
					{
						float desiredAmount2 = Math.Min(bestItem.GetLiquidContentsAmount(), pourIntoContainer2.GetLiquidCapacity() - pourIntoContainer2.GetLiquidContentsAmount());
						goal = new MoveToAndTake(character, character2, bestItem, desiredAmount2, CanIgnoreWeight(character), MovementType, pourIntoContainer2)
						{
							DontOpenOurGates = CalcDontOpenOurGates(character)
						};
					}
					else
					{
						goal = new MoveToAndTake(character, character2, bestItem, num15, CanIgnoreWeight(character), MovementType)
						{
							DontOpenOurGates = CalcDontOpenOurGates(character)
						};
					}
				}
				else if (character.IsEnemy(character2))
				{
					Attack attack = new Attack(character, character2, dontOpenOurGates: false, default(StayInRangeParams));
					attack.SetMovementTypeWhenNotInDanger(character, MovementType);
					goal = attack;
				}
				else
				{
					MemoryParam param = new MemoryParam(num15);
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, character2, bestItem, sit, param);
					if (speechForSituation != null)
					{
						Conversation conversation2 = new Conversation(character, character2, bestItem, param, speechForSituation, controlledByPlayer: false, null, null, default(MemoryParam));
						conversation2.DontOpenOurGates = CalcDontOpenOurGates(character);
						conversation2.SetMovementType(character, MovementType);
						goal = conversation2;
					}
					else
					{
						goal = null;
					}
				}
			}
			else
			{
				goal = null;
			}
			if (goal != null && bestSpeechSituation != SpeechSituation.None && character.IsControllableByPlayer() && StoryManager.Instance.GetMostInterestingSpeaker() == null && FindType != FindType.WaterForCrops)
			{
				BaseObject obj = bestItem;
				if (bestItem == null)
				{
					obj = best;
				}
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, obj, bestSpeechSituation);
				if (speechForSituation2 != null)
				{
					character.Speak(speechForSituation2, null, obj);
				}
			}
			if (goal is FindGoal findGoal)
			{
				findGoal.CanTravelFar = CanTravelFar;
			}
			FoundItem = bestItem;
			return goal;
		}
	}

	private bool MakeRoomForTool(Character character, EquipmentPrototype toolProto, EquipmentPrototype resourceProto, bool failedToDepositStuff, out Goal goal)
	{
		goal = null;
		if (toolProto == null || resourceProto == null)
		{
			return false;
		}
		float num = toolProto.Weight + resourceProto.Weight;
		if (character.HasInventorySpaceFor(num))
		{
			return true;
		}
		bool failed;
		Goal goal2 = (failedToDepositStuff ? null : GatherGoal.GetMoveToAndDepositGoal(character, num, num, MovementType, out failed));
		if (goal2 != null)
		{
			goal = goal2;
			return true;
		}
		return false;
	}

	private static int CalcDesiredFoodAmountForAnimals(Character character, Equipment item)
	{
		if (character.Community == null)
		{
			return 1;
		}
		return MathUtil.Clamp(Mathf.CeilToInt((float)(2 + character.Community.GetLivingNonZombieMemberCountBySpecies(BaseObjectType.Chicken)) / (item.GetNutrition() * Chicken.NutritionFactor / Sun.DayLengthSecs)), 3, 10);
	}

	public static bool CalcDontOpenOurGates(Character character)
	{
		bool result = false;
		if (character.IsInEnclosedArea())
		{
			foreach (Target target in character.Targets)
			{
				if (target.GetFlag(TargetFlags.Inaccessible) && target.Object != null && !target.Object.Deleted && !target.HasAnyFlag((TargetFlags)2084) && !target.Object.IsDestroyed() && character.IsEnemy(target.Object))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private Equipment GetPourIntoContainer(Character character, Equipment bestItem)
	{
		Equipment result = null;
		if (bestItem.GetLiquidContentsType() != null)
		{
			result = ((FindType != FindType.WaterForCrops) ? character.Inventory.GetBestLiquidContainerToFill(bestItem.GetLiquidContentsType(), bestItem, bestItem.GetLiquidContentsAmount()) : character.Inventory.GetBestWateringCan());
		}
		return result;
	}

	private Goal GetCraftBandageGoal(Character character, Equipment item)
	{
		foreach (Recipe oneItemRecipesForBandage in GameImpl.Instance.OneItemRecipesForBandages)
		{
			if (oneItemRecipesForBandage.Ingredients[0].MatchesItem(item, oneItemRecipesForBandage))
			{
				CraftGoal craftGoal = new CraftGoal();
				craftGoal.StartRecipe(character, oneItemRecipesForBandage, item, 1, character.Tile, Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: false, urgent: false);
				return craftGoal;
			}
		}
		return null;
	}

	private bool CheckTree(Character character, bool controllableByPlayer, bool hasAxe, TileObject tree, ref float bestCost, ref float bestTreeCost, ref TileObject best, ref Equipment bestItem, ref EquipmentPrototype bestResourceProto, ref LiquidPrototype bestResourceLiquid, ref bool bestIsTooHeavy, ref int bestMaxAmount, ref TerrainCoord bestTile, ref int bestEntranceIndex, ref int bestTerrainPathIndex, ref SpeechSituation bestSpeechSituation)
	{
		if (GameTerrain.Instance.IsSlopeOrImpassableRaw(tree.GetTileX(), tree.GetTileY()))
		{
			return false;
		}
		float num = (tree.PosXZ - character.PosXZ).magnitude / Character.WalkSpeed;
		num += TreeChopCost;
		if (num >= bestCost)
		{
			return false;
		}
		TerrainCoord centreTile = tree.GetCentreTile();
		if (controllableByPlayer && !GameTerrain.Instance.FogOfWar.IsTileExplored(centreTile.x, centreTile.y))
		{
			return false;
		}
		if (character.HasMovementZone() && !character.MovementZone.Contains(centreTile))
		{
			return false;
		}
		if (OnlyInEnclosedAreas && !GameTerrain.Instance.IsTileEnclosed(tree.GetNearestPassableTileTo(character.Tile, 0, character, null)))
		{
			return false;
		}
		if (tree.GetBaseObjectType() == BaseObjectType.TreeProp && ((TreeProp)tree).Growth < 1f)
		{
			return false;
		}
		Community communityThatOwnsThisArea = tree.GetCommunityThatOwnsThisArea();
		if (communityThatOwnsThisArea != null && communityThatOwnsThisArea != character.Community && communityThatOwnsThisArea.HasAnyLivingNonZombieMembers() && !communityThatOwnsThisArea.CachedAllies.Contains(character.Community))
		{
			return false;
		}
		bool flag = false;
		if (character.HasFailedFindAttempt(tree, this, RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
		{
			num += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
			flag = true;
			if (num >= bestCost)
			{
				return false;
			}
		}
		if (IsNearSomeoneWeJustRanAwayFrom(character, tree.Pos))
		{
			return false;
		}
		bestTreeCost = Math.Min(bestTreeCost, num);
		if (!hasAxe && tree.GetGrabbableEquipmentType() == null)
		{
			return false;
		}
		EquipmentPrototype equipmentPrototype = ((tree.GetMiningResourceType() != null) ? tree.GetMiningResourceType() : ((tree.GetGrabbableEquipmentType() != null) ? tree.GetGrabbableEquipmentType() : EquipmentPrototype.Wood));
		bool flag2 = !CanIgnoreWeight(character) && !character.HasInventorySpaceFor(equipmentPrototype.Weight);
		bestCost = num;
		best = tree;
		bestItem = null;
		bestResourceProto = equipmentPrototype;
		bestResourceLiquid = null;
		bestIsTooHeavy = flag2;
		bestMaxAmount = int.MaxValue;
		bestTile = TerrainCoord.Invalid;
		bestEntranceIndex = 0;
		bestTerrainPathIndex = -1;
		bestSpeechSituation = ((tree is TreeProp) ? SpeechSituation.ChoppingFirewood : ((tree.GetMiningResourceType() != null) ? SpeechSituation.Mining : SpeechSituation.None));
		return !flag;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (FindType == FindType.SealedContainer && FoundItem != null && FoundItem.GetLiquidContentsType() != null && FoundItem.GetLiquidContentsType() != Liquid)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: not false })
		{
			return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: true);
		}
		if (SubGoal is CraftGoal craftGoal)
		{
			if (FindType != FindType.Bandage)
			{
				if (craftGoal.DesiredAmount > 0 && character.GetCraftGoal() != null)
				{
					character.GetCraftGoal().OnFailed(character, CraftingRole);
				}
				return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
			}
			Result = ((character.Inventory.FindItemWithHighestBandageLevel() != null) ? FindResult.Success : FindResult.NotFound);
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.Result == FindResult.Success)
			{
				return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
			}
			if (findGoal.FindType == FindType.Backpack)
			{
				Result = FindResult.TooHeavy;
			}
			else
			{
				if (ChoseToFindTool)
				{
					return GetFirstSubGoal(character, parent, findToolFailed: true, failedToDepositStuff: false);
				}
				if (findGoal.FindType == FindType.WaterBottle || findGoal.FindType == FindType.SealedContainer || findGoal.FindType == FindType.WateringCan)
				{
					Result = FindResult.NeedContainer;
				}
				else if (findGoal.FindType == FindType.Axe)
				{
					Result = FindResult.NeedAxe;
				}
				else if (findGoal.FindType == FindType.Pickaxe)
				{
					Result = FindResult.NeedPickaxe;
				}
				else
				{
					Result = FindResult.NotFound;
				}
			}
		}
		if (SubGoal is LightFireGoal { Success: not false })
		{
			return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
		}
		if (SubGoal is SitAroundFireGoal { Success: not false })
		{
			return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
		}
		if (SubGoal is MoveToAndChop moveToAndChop)
		{
			if (moveToAndChop.Success)
			{
				if (character.Inventory.FindItemOfType(EquipmentPrototype.Wood) == null)
				{
					FallenTreeProp targetFallenTree = moveToAndChop.GetTargetFallenTree();
					if (targetFallenTree != null && !targetFallenTree.IsStump())
					{
						return new MoveToAndChop(character, targetFallenTree, MovementType);
					}
					return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
				}
				int num = 0;
				if (FindType == FindType.BuildingResources && FollowingRecipe != null && Building != null && Building.GetUnderConstructionInfo() != null)
				{
					Ingredient ingredient = FollowingRecipe.GetIngredient(EquipmentPrototype.Wood);
					if (ingredient != null && ingredient.IsItem())
					{
						int amount = ingredient.Amount;
						int num2 = (int)Building.GetUnderConstructionInfo().GetAmountUsedOfIngredient(ingredient);
						int num3 = (int)character.Inventory.GetIngredientAmount(character, character, ingredient, FollowingRecipe, UsingItem, character, character);
						num = DesiredAmountOfRecipe * amount - num2 - num3;
						num = Math.Min(num, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / EquipmentPrototype.Wood.Weight));
					}
				}
				else if (FindType == FindType.Prototype)
				{
					int num4 = character.Inventory.CountItemsOfType(ProtoToFind);
					num = DesiredAmountOfRecipe - num4;
					num = Math.Min(num, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / EquipmentPrototype.Wood.Weight));
				}
				if (num > 0)
				{
					TileObject tileObject = moveToAndChop.GetTargetFallenTree();
					if (tileObject == null || !LumberjackGoal.IsChoppableTree(tileObject))
					{
						tileObject = GameTerrain.Instance.GetClosestObjectInRange(character.Tile, 16, delegate(TileObject obj)
						{
							if (!LumberjackGoal.IsChoppableTree(obj))
							{
								return false;
							}
							if (!LumberjackGoal.CheckTree(character, obj, character.IsControllableByPlayer()))
							{
								return false;
							}
							if (GameTerrain.Instance.IsSlopeOrImpassableRaw(obj.GetTileX(), obj.GetTileY()))
							{
								return false;
							}
							TimeSpan timeSinceAttempt;
							int failCount;
							return !character.HasFailedFindAttempt(obj, 0, 0, FindType.Prototype, EquipmentPrototype.Wood, null, null, RememberFailedAttemptTime, out timeSinceAttempt, out failCount);
						});
					}
					if (tileObject != null)
					{
						HasStartedChoppingOrMining = true;
						return new MoveToAndChop(character, tileObject, MovementType, CalcDontOpenOurGates(character));
					}
				}
				Result = FindResult.Success;
			}
			else
			{
				FallenTreeProp targetFallenTree2 = moveToAndChop.GetTargetFallenTree();
				if (targetFallenTree2 != null && targetFallenTree2.IsStump())
				{
					return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
				}
				Result = FindResult.NotAccessible;
				character.AddFailedFindAttempt(moveToAndChop.GetTargetObject(), this);
			}
		}
		if (SubGoal is MoveToAndMine moveToAndMine)
		{
			TileObject targetObject = moveToAndMine.GetTargetObject();
			if (moveToAndMine.Success)
			{
				int num5 = 0;
				if (FindType == FindType.BuildingResources && targetObject != null && FollowingRecipe != null && Building != null && Building.GetUnderConstructionInfo() != null)
				{
					EquipmentPrototype equipmentPrototype = EquipmentPrototype.MiningResources[(int)moveToAndMine.MineralType];
					Ingredient ingredient2 = FollowingRecipe.GetIngredient(equipmentPrototype);
					if (ingredient2 != null && ingredient2.IsItem())
					{
						int amount2 = ingredient2.Amount;
						int num6 = (int)Building.GetUnderConstructionInfo().GetAmountUsedOfIngredient(ingredient2);
						int num7 = (int)character.Inventory.GetIngredientAmount(character, character, ingredient2, FollowingRecipe, UsingItem, character, character);
						num5 = DesiredAmountOfRecipe * amount2 - num6 - num7;
						num5 = Math.Min(num5, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / equipmentPrototype.Weight));
					}
				}
				else if (FindType == FindType.Prototype)
				{
					int num8 = character.Inventory.CountItemsOfType(ProtoToFind);
					num5 = DesiredAmountOfRecipe - num8;
					num5 = Math.Min(num5, Mathf.FloorToInt((character.GetAvailableInventorySpace() - FarmingGoal.RequiredFreeInventorySpace) / ProtoToFind.Weight));
				}
				if (num5 > 0)
				{
					HasStartedChoppingOrMining = true;
					return new MoveToAndMine(character, targetObject, moveToAndMine.MineralType, MovementType, CalcDontOpenOurGates(character));
				}
				Result = FindResult.Success;
			}
			else
			{
				Result = FindResult.NotAccessible;
				character.AddFailedFindAttemptForEntrance(targetObject, moveToAndMine.EntranceIndex, this);
			}
		}
		if (SubGoal is MoveToAndGrab moveToAndGrab)
		{
			if (moveToAndGrab.Success)
			{
				Result = FindResult.Success;
			}
			else
			{
				Result = FindResult.NotAccessible;
				if (moveToAndGrab.GetTargetObject() != null)
				{
					character.AddFailedFindAttempt(moveToAndGrab.GetTargetObject(), this);
				}
			}
		}
		if (SubGoal is MoveToAndTake moveToAndTake)
		{
			if (moveToAndTake.TooHeavy)
			{
				Result = FindResult.TooHeavy;
			}
			else if (moveToAndTake.AmountRetrieved > 0f)
			{
				Result = FindResult.Success;
			}
			else
			{
				if (moveToAndTake.IsTargetDeleted() || moveToAndTake.GetTargetEquipment() == null || !moveToAndTake.Target.Object.InventoryContains(moveToAndTake.GetTargetEquipment()))
				{
					return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
				}
				Result = FindResult.NotAccessible;
				character.AddFailedFindAttemptForEntrance(moveToAndTake.Target.Object, moveToAndTake.EntranceIndex, this);
			}
		}
		if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal)
		{
			Result = (moveToAndInteractGoal.Success ? FindResult.Success : FindResult.NotAccessible);
			if (!moveToAndInteractGoal.Success && !moveToAndInteractGoal.IsTargetDeleted() && moveToAndInteractGoal.Item != null && moveToAndInteractGoal.Target.Object.InventoryContains(moveToAndInteractGoal.Item) && moveToAndInteractGoal.Item.GetLiquidContentsAmount() > 0f)
			{
				character.AddFailedFindAttempt(moveToAndInteractGoal.Target.Object, this);
			}
			return null;
		}
		if (SubGoal is FillLiquidContainer fillLiquidContainer)
		{
			if (fillLiquidContainer.Success)
			{
				Result = FindResult.Success;
				FilledFromTile = fillLiquidContainer.Tile;
				FilledFromPathIndex = fillLiquidContainer.TerrainPathIndex;
				if (fillLiquidContainer.GetLiquidContainer() != null && fillLiquidContainer.GetLiquidContainer().GetLiquidContentsType() == LiquidPrototype.Water)
				{
					Goal fillExtraWaterContainerGoal = GetFillExtraWaterContainerGoal(character, FilledFromTile, FilledFromPathIndex, MovementType);
					if (fillExtraWaterContainerGoal != null)
					{
						return fillExtraWaterContainerGoal;
					}
				}
			}
			else if (Result != FindResult.Success)
			{
				Result = FindResult.NotAccessible;
				if (GameTerrain.Instance.GetFixedObjectOnTile(fillLiquidContainer.Tile.x, fillLiquidContainer.Tile.y) is Prop searchObject)
				{
					character.AddFailedFindAttempt(searchObject, this);
				}
				else if (fillLiquidContainer.TerrainPathIndex != -1)
				{
					character.AddFailedFindAttemptForTerrainPath(GameTerrain.Instance, fillLiquidContainer.TerrainPathIndex, this);
				}
			}
		}
		if (SubGoal is HarvestCrops harvestCrops)
		{
			if (harvestCrops.Success)
			{
				Result = FindResult.Success;
			}
			else
			{
				Result = FindResult.NotAccessible;
				TileObject tileObject2 = GameTerrain.Instance.GetFixedObjectOnTile(harvestCrops.Tile.x, harvestCrops.Tile.y) as PlantableCrop;
				if (tileObject2 != null)
				{
					character.AddFailedFindAttempt(tileObject2, this);
				}
			}
		}
		if (SubGoal is MoveToAndDrinkFromRiver moveToAndDrinkFromRiver)
		{
			if (moveToAndDrinkFromRiver.Success)
			{
				Result = FindResult.Success;
				FilledFromTile = moveToAndDrinkFromRiver.Tile;
				FilledFromPathIndex = moveToAndDrinkFromRiver.TerrainPathIndex;
				Goal fillExtraWaterContainerGoal2 = GetFillExtraWaterContainerGoal(character, FilledFromTile, FilledFromPathIndex, MovementType);
				if (fillExtraWaterContainerGoal2 != null)
				{
					return fillExtraWaterContainerGoal2;
				}
			}
			else
			{
				Result = FindResult.NotAccessible;
				if (moveToAndDrinkFromRiver.TerrainPathIndex != -1)
				{
					character.AddFailedFindAttemptForTerrainPath(GameTerrain.Instance, moveToAndDrinkFromRiver.TerrainPathIndex, this);
				}
			}
		}
		if (SubGoal is Conversation conversation)
		{
			if (conversation.Success && conversation.GotSuccessfulResponse)
			{
				return new Wait(TimeSpan.FromSeconds(1.0));
			}
			Result = FindResult.NotAccessible;
			character.AddFailedFindAttempt(conversation.Target.Object, this);
			return null;
		}
		if (SubGoal is Wait)
		{
			Result = FindResult.Success;
		}
		if (Result == FindResult.Success)
		{
			if (FindType != FindType.CraftingResources)
			{
				FoundItem = FindItemInInventory(character, character, SetupFindData(character));
				Result = ((FoundItem != null) ? FindResult.Success : FindResult.NotFound);
			}
			if (FindType == FindType.Bandage && FoundItem != null && FoundItem.GetPrototype().BandageLevel == -1)
			{
				return GetCraftBandageGoal(character, FoundItem);
			}
			if (FoundItem == null && FindType == FindType.Lighter)
			{
				return GetFirstSubGoal(character, parent, findToolFailed: false, failedToDepositStuff: false);
			}
			if (FindType == FindType.Backpack)
			{
				Result = (character.HasInventorySpaceFor(ToCarryWeight) ? FindResult.Success : FindResult.NotFound);
			}
		}
		return base.GetNextSubGoal(character, parent);
	}

	public static Goal GetFillExtraWaterContainerGoal(Character character, TerrainCoord filledFromTile, int filledFromPathIndex, MovementType movementType)
	{
		Equipment bestWaterBottle = character.Inventory.GetBestWaterBottle();
		if (bestWaterBottle != null && bestWaterBottle.GetLiquidContentsAmount() < bestWaterBottle.GetLiquidCapacity())
		{
			return new FillLiquidContainer(character, filledFromTile, filledFromPathIndex, bestWaterBottle, movementType);
		}
		if (character.HasRole(Role.Farmer))
		{
			Equipment bestWateringCan = character.Inventory.GetBestWateringCan();
			if (bestWateringCan != null && bestWateringCan.GetLiquidContentsAmount() < bestWateringCan.GetLiquidCapacity())
			{
				return new FillLiquidContainer(character, filledFromTile, filledFromPathIndex, bestWateringCan, movementType);
			}
		}
		return null;
	}

	public int TakeNeededResourcesFromBuilding(Character character, TileObject obj, out bool _tooHeavy, bool ignoreWeight, Equipment pourIntoContainer)
	{
		_tooHeavy = false;
		int num = 0;
		if (obj == null)
		{
			return num;
		}
		EquipmentContainer inventory = obj.GetInventory();
		if (inventory == null)
		{
			return 0;
		}
		switch (FindType)
		{
		case FindType.CraftingResources:
		{
			float maxInventoryWeight = character.GetMaxInventoryWeight();
			foreach (Ingredient ingredient2 in FollowingRecipe.Ingredients)
			{
				float ingredientAmount = character.Inventory.GetIngredientAmount(character, character, ingredient2, FollowingRecipe, UsingItem, null, CheckEquipmentPolicyForCraftingResources ? character : null);
				float num6 = ((ingredient2.Prototypes != null) ? ((float)ingredient2.Amount) : ((ingredient2.LiquidTypes != null) ? ingredient2.LiquidAmount : 0f)) - ingredientAmount;
				while (num6 > 0f)
				{
					Equipment equipment3 = inventory.FindIngredient(character, obj, ingredient2, FollowingRecipe, UsingItem, null, CheckEquipmentPolicyForCraftingResources ? character : null);
					if (equipment3 == null)
					{
						break;
					}
					int num7 = (int)((maxInventoryWeight - character.Inventory.GetWeight(character)) / equipment3.GetWeight());
					if (ignoreWeight)
					{
						num7 = Math.Max(num7, 1);
					}
					if (ingredient2.Prototypes != null)
					{
						int num8 = Math.Min(num7, Math.Max(1, (int)num6));
						if (!character.IsControllableByPlayer())
						{
							num8 = Math.Max(num8, (int)ingredientAmount - ingredient2.Amount);
						}
						if (num8 > 0)
						{
							TakeAnim.TakeAllItemsFromPot(character, obj, equipment3);
							Equipment equipment4 = inventory.Take(obj, equipment3, num8);
							if (equipment4 != null)
							{
								NotificationManager.Instance.AddEquipmentNotification(obj, character, equipment4, equipment4.GetAmount());
							}
							character.Inventory.Add(character, equipment4);
							num += num8;
							num6 -= (float)num8;
							continue;
						}
						_tooHeavy = true;
						break;
					}
					if (ingredient2.LiquidTypes == null)
					{
						break;
					}
					bool flag2 = pourIntoContainer != null && (pourIntoContainer.GetLiquidContentsType() == null || pourIntoContainer.GetLiquidContentsType() == equipment3.GetLiquidContentsType());
					if (num7 > 0 || flag2 || (!character.IsControllableByPlayer() && ingredientAmount < ingredient2.LiquidAmount))
					{
						TakeAnim.TakeAllItemsFromPot(character, obj, equipment3);
						if (flag2)
						{
							pourIntoContainer.FillLiquid(equipment3.GetLiquidContentsType(), equipment3.DrainLiquid(pourIntoContainer.GetLiquidCapacity() - pourIntoContainer.GetLiquidContentsAmount()), equipment3.InfectedWith);
						}
						else
						{
							Equipment equipment5 = inventory.Take(obj, equipment3, 1);
							if (equipment5 != null)
							{
								NotificationManager.Instance.AddEquipmentNotification(obj, character, equipment5, equipment5.GetAmount());
							}
							character.Inventory.Add(character, equipment5);
						}
						num++;
						num6 -= equipment3.GetLiquidContentsAmount();
						continue;
					}
					_tooHeavy = true;
					break;
				}
			}
			break;
		}
		case FindType.BuildingResources:
		{
			if (Building == null)
			{
				break;
			}
			UnderConstructionInfo underConstructionInfo = Building.GetUnderConstructionInfo();
			if (underConstructionInfo == null)
			{
				break;
			}
			int num2 = 0;
			bool flag = false;
			for (int i = 0; i < underConstructionInfo.Recipe.Ingredients.Count; i++)
			{
				if (underConstructionInfo.HasUsedEnoughOfIngredient(underConstructionInfo.Recipe.Ingredients[i]))
				{
					continue;
				}
				Ingredient ingredient = underConstructionInfo.Recipe.Ingredients[i];
				Equipment equipment = inventory.FindIngredient(character, obj, ingredient, underConstructionInfo.Recipe, UsingItem, null, null);
				if (equipment == null)
				{
					continue;
				}
				int num3 = (int)((character.GetMaxInventoryWeight() - character.Inventory.GetWeight(character)) / equipment.GetWeight());
				if (ignoreWeight)
				{
					num3 = Math.Max(num3, 1);
				}
				if (num3 > 0)
				{
					if (!ingredient.IsLiquid())
					{
						int num4 = (int)underConstructionInfo.GetAmountUsedOfIngredient(ingredient);
						int num5 = (int)character.Inventory.GetIngredientAmount(character, character, ingredient, underConstructionInfo.Recipe, UsingItem, character, null);
						int val = DesiredAmountOfRecipe * ingredient.Amount - num4 - num5;
						num3 = Math.Min(num3, val);
						if (num3 <= 0)
						{
							Debug.LogWarning("Getting an " + equipment.GetDisplayNameString() + " we already have? amountUsed: " + num4 + ", amountInInventory: " + num5 + ", amountNeeded: " + val);
							num3 = 1;
						}
					}
					TakeAnim.TakeAllItemsFromPot(character, obj, equipment);
					Equipment equipment2 = inventory.Take(obj, equipment, num3);
					if (equipment2 != null)
					{
						NotificationManager.Instance.AddEquipmentNotification(obj, character, equipment2, equipment2.GetAmount());
					}
					character.Inventory.Add(character, equipment2);
					num += num3;
					num2 += num3;
				}
				else
				{
					flag = true;
				}
			}
			if (num2 == 0 && flag)
			{
				_tooHeavy = true;
			}
			break;
		}
		case FindType.Clothing:
			num = ClothingGoal.ExchangeClothing(obj, character, Critical);
			break;
		}
		return num;
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		MovementType = movementType;
		base.SetMovementType(character, movementType);
	}
}
