using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class Recipe
{
	public string UniqueID = "";

	public string NativeName = "";

	[XmlIgnore]
	public int NameHash;

	[XmlIgnore]
	public Texture2D Icon;

	[DefaultValue(RecipeType.Normal)]
	public RecipeType RecipeType;

	public ShowRecipe ShowRecipe;

	[DefaultValue("")]
	public string CraftingStationType = "";

	[XmlIgnore]
	public PropPrototype CraftingStationPrototype;

	public SkillType SkillType = SkillType.Construction;

	[DefaultValue(0)]
	public int SkillLevel;

	public int SkillProgression = 10;

	[DefaultValue("")]
	public string ProductType = "";

	[XmlIgnore]
	public PropPrototype ProductPropPrototype;

	[XmlIgnore]
	public EquipmentPrototype ProductPrototype;

	[DefaultValue("")]
	public string ProductPrototypeName = "";

	[DefaultValue(1)]
	public int ProductAmount = 1;

	[XmlIgnore]
	public LiquidPrototype ProductLiquidPrototype;

	[DefaultValue("")]
	public string ProductLiquidPrototypeName = "";

	[DefaultValue(0f)]
	public float ProductLiquidAmount;

	[DefaultValue(0f)]
	public float ProductLiquidNutritionMultiplier;

	[DefaultValue(true)]
	public bool CanPassInfectionToProduct = true;

	public List<RecipeExtraOutput> ExtraOutputs;

	[DefaultValue(false)]
	public bool OnlyUseManually;

	[DefaultValue(false)]
	public bool Deprecated;

	[OnlyVisibleForRecipeType(RecipeType.Workbench, RecipeType.Forge, RecipeType.Kiln)]
	[DefaultValue(false)]
	public bool IsDisassembly;

	public float CraftingTime;

	public List<Ingredient> Ingredients = new List<Ingredient>();

	[XmlIgnore]
	public bool ShownDiscoveredNotification;

	public void CopyFrom(Recipe other)
	{
		FieldInfo[] fields = typeof(Recipe).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}

	public bool IsValidRecipeForCook()
	{
		return IsProductEdible(includeSugar: false, includeHumanMeat: true);
	}

	public bool IsProductDrinkableOrEdible()
	{
		if (ProductPrototype != null && ProductPrototype.GetNutrition() > 0f)
		{
			return true;
		}
		if (ProductLiquidPrototype != null && ProductLiquidPrototype.DrinkableOrEdible)
		{
			return true;
		}
		return false;
	}

	public bool IsProductEdible(bool includeSugar, bool includeHumanMeat)
	{
		if (!includeHumanMeat)
		{
			foreach (Ingredient ingredient in Ingredients)
			{
				if (ingredient.Prototypes == null)
				{
					continue;
				}
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					if (prototype.ContainsHumanMeat)
					{
						return false;
					}
				}
			}
		}
		if (ProductPrototype != null && ProductPrototype.GetNutrition() > 0f)
		{
			if (!includeHumanMeat && ProductPrototype.ContainsHumanMeat)
			{
				return false;
			}
			return true;
		}
		if (ProductLiquidPrototype != null && ProductLiquidPrototype.Edible && (includeSugar || ProductLiquidPrototype.NutritionPerFlOz >= EquipmentContainer.MinNutritionPerFlOzToEat))
		{
			return true;
		}
		return false;
	}

	public bool IsProductDrinkable()
	{
		if (ProductLiquidPrototype != null && ProductLiquidPrototype.Drinkable)
		{
			return true;
		}
		return false;
	}

	public bool IsProductAlcoholic()
	{
		if (ProductLiquidPrototype != null && ProductLiquidPrototype.Drinkable && ProductLiquidPrototype.AlcoholContent > 0f)
		{
			return true;
		}
		return false;
	}

	public BaseObjectType RequiredPropToWorkOn()
	{
		switch (RecipeType)
		{
		case RecipeType.Campfire_SpitRoast_Rabbit:
		case RecipeType.Campfire_SpitRoast_Chicken:
		case RecipeType.Campfire_SpitRoast_Venison:
		case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
		case RecipeType.Campfire_Pot:
		case RecipeType.Campfire_FryingPan:
			return BaseObjectType.Campfire;
		case RecipeType.Kiln:
			return BaseObjectType.Kiln;
		case RecipeType.Workbench:
			return BaseObjectType.WorkBench;
		case RecipeType.Forge:
			return BaseObjectType.Forge;
		case RecipeType.Still:
			return BaseObjectType.Still;
		case RecipeType.Nitrary:
			return BaseObjectType.Nitrary;
		default:
			return BaseObjectType.Invalid;
		}
	}

	public bool IsCraftingPropForRecipe(Prop prop)
	{
		if (CraftingStationPrototype != null && CraftingStationPrototype != prop.Prototype)
		{
			return false;
		}
		if (RequiredPropToWorkOn() != prop.GetBaseObjectType())
		{
			return false;
		}
		return true;
	}

	public bool IsRecipeTool(Character creator, Equipment item)
	{
		switch (RecipeType)
		{
		case RecipeType.HuntingKnife:
			if (item == creator.Inventory.GetHuntingKnife())
			{
				return true;
			}
			break;
		case RecipeType.Toolbox:
			if (item == creator.Inventory.GetToolbox())
			{
				return true;
			}
			break;
		case RecipeType.Shovel:
			if (item == creator.Inventory.GetShovel())
			{
				return true;
			}
			break;
		case RecipeType.Campfire_Pot:
			if (item == creator.Inventory.GetBestCookingPot())
			{
				return true;
			}
			break;
		case RecipeType.Campfire_FryingPan:
			if (item == creator.Inventory.GetFryingPan())
			{
				return true;
			}
			break;
		}
		return false;
	}

	public bool IsRecipeToolType(Equipment item)
	{
		return RecipeType switch
		{
			RecipeType.HuntingKnife => item is HuntingKnife, 
			RecipeType.Toolbox => item is Toolbox, 
			RecipeType.Shovel => item is Shovel, 
			RecipeType.Campfire_Pot => item.GetPrototype() == EquipmentPrototype.Pot, 
			RecipeType.Campfire_FryingPan => item.GetPrototype() == EquipmentPrototype.FryingPan, 
			_ => true, 
		};
	}

	public bool HasToolTypeForRecipe(TileObject obj)
	{
		EquipmentContainer inventory = obj.GetInventory();
		if (inventory == null)
		{
			return false;
		}
		return RecipeType switch
		{
			RecipeType.HuntingKnife => inventory.GetHuntingKnife() != null, 
			RecipeType.Toolbox => inventory.GetToolbox() != null, 
			RecipeType.Shovel => inventory.GetShovel() != null, 
			RecipeType.Campfire_Pot => inventory.GetBestCookingPot() != null, 
			RecipeType.Campfire_FryingPan => inventory.GetFryingPan() != null, 
			_ => true, 
		};
	}

	public FindGoal GetFindGoalForRecipeTool(MovementType movementType)
	{
		return RecipeType switch
		{
			RecipeType.HuntingKnife => new FindGoal(FindType.HuntingKnife, movementType, critical: false), 
			RecipeType.Toolbox => new FindGoal(FindType.Toolbox, movementType, critical: false), 
			RecipeType.Shovel => new FindGoal(FindType.Shovel, movementType, critical: false), 
			_ => null, 
		};
	}

	public void GetRecipeToolWeight(out float minWeight, out float maxWeight)
	{
		minWeight = 0f;
		maxWeight = 0f;
		switch (RecipeType)
		{
		case RecipeType.HuntingKnife:
		{
			foreach (EquipmentPrototype huntingKnife in EquipmentPrototype.HuntingKnives)
			{
				minWeight = Math.Min(huntingKnife.Weight, minWeight);
				maxWeight = Math.Max(huntingKnife.Weight, maxWeight);
			}
			break;
		}
		case RecipeType.Toolbox:
		{
			foreach (EquipmentPrototype toolbox in EquipmentPrototype.Toolboxes)
			{
				minWeight = Math.Min(toolbox.Weight, minWeight);
				maxWeight = Math.Max(toolbox.Weight, maxWeight);
			}
			break;
		}
		case RecipeType.Shovel:
		{
			foreach (EquipmentPrototype shovel in EquipmentPrototype.Shovels)
			{
				minWeight = Math.Min(shovel.Weight, minWeight);
				maxWeight = Math.Max(shovel.Weight, maxWeight);
			}
			break;
		}
		case RecipeType.Campfire_Pot:
			if (EquipmentPrototype.Pot != null)
			{
				minWeight = Math.Min(EquipmentPrototype.Pot.Weight, minWeight);
				maxWeight = Math.Max(EquipmentPrototype.Pot.Weight, maxWeight);
			}
			break;
		case RecipeType.Campfire_FryingPan:
			if (EquipmentPrototype.FryingPan != null)
			{
				minWeight = Math.Min(EquipmentPrototype.FryingPan.Weight, minWeight);
				maxWeight = Math.Max(EquipmentPrototype.FryingPan.Weight, maxWeight);
			}
			break;
		case RecipeType.Campfire_SpitRoast_Rabbit:
		case RecipeType.Campfire_SpitRoast_Chicken:
		case RecipeType.Campfire_SpitRoast_Venison:
		case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
			break;
		}
	}

	public bool IsIngredient(Equipment item)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.MatchesItem(item, this))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsIngredient(EquipmentPrototype proto)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.Prototypes != null && ingredient.Prototypes.Contains(proto))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyIngredientAMiningResource()
	{
		for (int i = 0; i < EquipmentPrototype.MiningResources.Length; i++)
		{
			if (IsIngredient(EquipmentPrototype.MiningResources[i]))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsLiquidIngredient(LiquidPrototype liquid)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.LiquidTypes != null && ingredient.LiquidTypes.Contains(liquid))
			{
				return true;
			}
		}
		return false;
	}

	public Ingredient GetIngredient(Equipment item)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.MatchesItem(item, this))
			{
				return ingredient;
			}
		}
		return null;
	}

	public Ingredient GetIngredient(EquipmentPrototype proto)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.Prototypes != null && ingredient.Prototypes.Contains(proto))
			{
				return ingredient;
			}
		}
		return null;
	}

	public bool AreAllIngredientsReturnedInFull()
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.ReturnType == IngredientReturnType.Full)
			{
				return true;
			}
		}
		return false;
	}

	public Ingredient GetLiquidIngredient(LiquidPrototype liquid)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.LiquidTypes != null && ingredient.LiquidTypes.Contains(liquid))
			{
				return ingredient;
			}
		}
		return null;
	}

	public float GetMaxWeightOfIngredientsWeHaventGotYet(Character character, Equipment usingItem)
	{
		float num = 0f;
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.HasEnoughOfIngredient(character, character, this, usingItem, character, out var amountNeeded))
			{
				continue;
			}
			if (ingredient.Prototypes != null)
			{
				float num2 = 0f;
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					num2 = Math.Max(num2, prototype.Weight);
				}
				num += num2 * amountNeeded;
			}
			if (ingredient.LiquidTypes != null && EquipmentPrototype.PlasticBottle != null)
			{
				num += (float)Mathf.CeilToInt(amountNeeded / EquipmentPrototype.PlasticBottle.LiquidCapacity) * EquipmentPrototype.PlasticBottle.Weight;
			}
		}
		return num;
	}

	public bool HasAnyIngredients(Character crafter, TileObject carrier, Equipment usingItem, Character checkEquipmentPolicyForUser)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (usingItem != null && !ingredient.Interchangeable && ingredient.MatchesItem(usingItem, this))
			{
				if (carrier.InventoryContains(usingItem))
				{
					return true;
				}
			}
			else if (ingredient.HasAnyOfIngredient(crafter, carrier, this, usingItem, checkEquipmentPolicyForUser))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAllIngredients(Character crafter, TileObject carrier, Equipment usingItem, Character checkEquipmentPolicyForUser)
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (usingItem != null && !ingredient.Interchangeable && ingredient.MatchesItem(usingItem, this) && !carrier.InventoryContains(usingItem))
			{
				return false;
			}
			if (!ingredient.HasEnoughOfIngredient(crafter, carrier, this, usingItem, checkEquipmentPolicyForUser))
			{
				return false;
			}
		}
		return true;
	}

	public float GetProductAmount(Character carrierCharacter, Equipment usingItem)
	{
		float num = 0f;
		if (ProductLiquidPrototype != null)
		{
			num = ProductLiquidAmount;
			if (ProductLiquidPrototype.GetNutritionPerFlOz() > 0f)
			{
				if (!CalcNutritionFromIngredients(carrierCharacter, usingItem, out var ingredientsNutrition))
				{
					foreach (Ingredient ingredient in Ingredients)
					{
						float num2 = float.MaxValue;
						if (ingredient.Prototypes != null)
						{
							foreach (EquipmentPrototype prototype in ingredient.Prototypes)
							{
								num2 = Math.Min(prototype.GetNutrition() * (float)ingredient.Amount, num2);
							}
						}
						if (ingredient.LiquidTypes != null)
						{
							foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
							{
								num2 = Math.Min(liquidType.GetNutritionPerFlOz() * ingredient.LiquidAmount, num2);
							}
						}
						if (num2 != float.MaxValue)
						{
							ingredientsNutrition += num2;
						}
					}
				}
				num += ProductLiquidNutritionMultiplier * ingredientsNutrition / ProductLiquidPrototype.GetNutritionPerFlOz();
			}
		}
		return num;
	}

	public bool HasSuitableContainer(Character carrierCharacter, Equipment usingItem)
	{
		bool hasSomeButNotEnough;
		float productAmount;
		float freeCapacity;
		return HasSuitableContainer(carrierCharacter, usingItem, out hasSomeButNotEnough, out productAmount, out freeCapacity);
	}

	public bool HasSuitableContainer(Character carrierCharacter, Equipment usingItem, out bool hasSomeButNotEnough, out float productAmount, out float freeCapacity)
	{
		hasSomeButNotEnough = false;
		productAmount = GetProductAmount(carrierCharacter, usingItem);
		freeCapacity = 0f;
		if (ProductLiquidPrototype != null)
		{
			for (int i = 0; i < carrierCharacter.Inventory.Count; i++)
			{
				Equipment item = carrierCharacter.Inventory.GetItem(i);
				if (item.GetLiquidCapacity() > 0f && IsSuitableContainer(item, dontReplaceContents: true))
				{
					freeCapacity += item.GetLiquidCapacity() - item.GetLiquidContentsAmount();
				}
			}
			if (ProductPrototype != null && ProductPrototype.IsSuitableContainer(ProductLiquidPrototype))
			{
				freeCapacity += ProductPrototype.LiquidCapacity * (float)ProductAmount;
			}
			if (ExtraOutputs != null)
			{
				foreach (RecipeExtraOutput extraOutput in ExtraOutputs)
				{
					if (extraOutput.ProductPrototype != null && extraOutput.ProductPrototype.IsSuitableContainer(ProductLiquidPrototype))
					{
						freeCapacity += extraOutput.ProductPrototype.LiquidCapacity * (float)extraOutput.MinAmount;
					}
				}
			}
			hasSomeButNotEnough = freeCapacity > 0f;
			return freeCapacity >= productAmount - 0.01f;
		}
		return true;
	}

	public bool IsSuitableContainer(Equipment item, bool dontReplaceContents = false)
	{
		if (ProductLiquidPrototype != null && item.GetLiquidCapacity() > 0f)
		{
			if (dontReplaceContents)
			{
				if (item.GetLiquidContentsType() != null && item.GetLiquidContentsType() != ProductLiquidPrototype)
				{
					return false;
				}
				if (item.GetLiquidContentsAmount() >= item.GetLiquidCapacity())
				{
					return false;
				}
			}
			if (item.DesignatedLiquid != null && item.DesignatedLiquid != ProductLiquidPrototype)
			{
				return false;
			}
			if (item is WateringCan)
			{
				return false;
			}
			if (item.GetPrototype() == EquipmentPrototype.Pot && !(item.InventoryOwner is Campfire))
			{
				return false;
			}
			if (!ProductLiquidPrototype.CanPourIntoBottles && item.IsBottle())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool WouldBeUsedToFillWithProduct(Character carrierCharacter, Equipment usingItem, Equipment container)
	{
		float productAmount = GetProductAmount(carrierCharacter, usingItem);
		float num = 0f;
		foreach (Equipment content in carrierCharacter.Inventory.Contents)
		{
			if (content == container)
			{
				return num < productAmount;
			}
			if (IsSuitableContainer(content, dontReplaceContents: true))
			{
				num += content.GetLiquidCapacity() - content.GetLiquidContentsAmount();
			}
		}
		return false;
	}

	public bool CommunityHasEnoughSpareIngredientsWithinMovementZone(Character character)
	{
		Community community = character.Community;
		if (community == null)
		{
			return false;
		}
		foreach (Ingredient ingredient in Ingredients)
		{
			bool flag = false;
			if (ingredient.Prototypes != null)
			{
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					if ((prototype.GetSeedForPlantType() == null || community.IsAllowedToEatCrops(prototype.GetSeedForPlantType())) && character.IsActionAllowedForItem(prototype, null, InfectionType.None, EquipmentPolicyAction.CanCraftWith) && (!prototype.ContainsHumanMeat || !character.ShouldRefuseToEatHumanMeat() || !IsProductDrinkableOrEdible()) && community.HasUsableInventoryItemsOfTypeWithinMovementZone(character, prototype, ingredient.Amount))
					{
						flag = true;
					}
				}
			}
			if (ingredient.LiquidTypes != null)
			{
				foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
				{
					if (character.IsActionAllowedForItem(null, liquidType, InfectionType.None, EquipmentPolicyAction.CanCraftWith) && community.GetTotalLiquidWithinMovementZone(character, liquidType) >= ingredient.LiquidAmount)
					{
						flag = true;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public int GetMaxCraftableUsingItemsInInventory(Character crafter, Equipment usingItem)
	{
		EquipmentContainer inventory = crafter.Inventory;
		int num = int.MaxValue;
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.Prototypes != null && ingredient.Amount > 0)
			{
				int num2 = 0;
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					for (int i = 0; i < inventory.Count; i++)
					{
						Equipment item = inventory.GetItem(i);
						if (item.GetPrototype() == prototype && crafter.CanUseItemForCrafting(crafter, item, usingItem, this) && item.MatchesIngredientInfectionState(ingredient.IngredientInfectionState))
						{
							num2 += item.GetAmount();
						}
					}
				}
				num = Math.Min(num, num2 / ingredient.Amount);
			}
			if (ingredient.LiquidTypes == null || !(ingredient.LiquidAmount > 0f))
			{
				continue;
			}
			float num3 = 0f;
			foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
			{
				for (int j = 0; j < inventory.Count; j++)
				{
					Equipment item2 = inventory.GetItem(j);
					if (item2.GetLiquidContentsType() == liquidType && item2.MatchesIngredientInfectionState(ingredient.IngredientInfectionState))
					{
						num3 += item2.GetLiquidContentsAmount();
					}
				}
			}
			num = Math.Min(num, (int)(num3 / ingredient.LiquidAmount));
		}
		return num;
	}

	public int CountIngredients()
	{
		int num = 0;
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.LiquidTypes != null)
			{
				num++;
			}
			if (ingredient.Prototypes != null)
			{
				num += ingredient.Amount;
			}
		}
		return num;
	}

	public bool HasNonInterchangeableIngredients()
	{
		foreach (Ingredient ingredient in Ingredients)
		{
			if (!ingredient.Interchangeable)
			{
				return true;
			}
		}
		return false;
	}

	public float UseIngredientsAndCreateProduct(Character carrier, Community community, Equipment usingItem, TerrainCoord destTile, Prop.OrientationType orientation, bool addToGatheredItems, bool checkEquipmentPolicy)
	{
		float numProduced = 0f;
		if (HasAllIngredients(carrier, carrier, usingItem, checkEquipmentPolicy ? carrier : null))
		{
			float ingredientsNutrition = 0f;
			InfectionType ingredientsInfectedWith = InfectionType.None;
			bool usedHumanIngredients = false;
			UseIngredients(carrier, usingItem, null, out ingredientsNutrition, out ingredientsInfectedWith, out usedHumanIngredients, checkEquipmentPolicy);
			CreateProduct(carrier, community, usingItem, destTile, orientation, ingredientsNutrition, ingredientsInfectedWith, out numProduced, addToGatheredItems);
		}
		return numProduced;
	}

	public static TileObject PlaceProp(PropPrototype propPrototype, TileObject carrier, Community community, TerrainCoord destTile, Prop.OrientationType orientation, Equipment usingItem)
	{
		TileObject tileObject = TileObject.CreateProp(propPrototype);
		if (tileObject != null)
		{
			Character character = carrier as Character;
			Session instance = Session.Instance;
			GameTerrain instance2 = GameTerrain.Instance;
			TerrainCoord tileGhost = destTile;
			tileObject.SetTileGhost(tileGhost);
			if (tileObject is Prop)
			{
				(tileObject as Prop).SetOrientationType(orientation);
				(tileObject as Prop).Investigated = true;
			}
			bool flag = character?.IsControllableByPlayer() ?? false;
			int num = 0;
			while (GameCursor.CanBuildHere(character, tileObject, checkOtherCharacters: false, !flag && num <= 50, community, num > 50) != CursorActionDisabledReason.Enabled)
			{
				if (num == 0 && character != null)
				{
					tileGhost = character.GetCentreTile();
				}
				else if (num <= 100)
				{
					tileGhost += instance.DeterministicRand.RandomTile(new TerrainCoord(-1, -1), new TerrainCoord(1, 1));
				}
				else
				{
					int num2 = num - 100;
					tileGhost = destTile + instance.DeterministicRand.RandomTile(new TerrainCoord(-num2, -num2), new TerrainCoord(num2, num2));
				}
				tileObject.SetTileGhost(tileGhost);
				num++;
				if (num >= 300)
				{
					return null;
				}
			}
			if (community != null)
			{
				tileObject.SetCommunity(community);
			}
			if (usingItem != null && tileObject is Prop { Prototype: not null } prop)
			{
				if (prop.Prototype.GetNumMaterialVariations() > 0)
				{
					prop.MaterialVariation = usingItem.MaterialVariation % prop.Prototype.GetNumMaterialVariations();
				}
				if (prop.Prototype.GetNumColorVariations() > 0)
				{
					prop.ColorVariation = usingItem.ColorVariation % prop.Prototype.GetNumColorVariations();
				}
				if (prop.Prototype.GetNumColorVariations2() > 0)
				{
					prop.ColorVariation2 = usingItem.ColorVariation2 % prop.Prototype.GetNumColorVariations2();
				}
				if (prop.Prototype.GetNumColorVariations3() > 0)
				{
					prop.ColorVariation3 = usingItem.ColorVariation3 % prop.Prototype.GetNumColorVariations3();
				}
			}
			Session.Instance.ClearAreaForBuilding(community, tileObject);
			tileObject.OnSpawn();
			community?.OnCompletedBuildingAddedToCommunity(character, tileObject, null);
			instance2.BuildMinimap(tileObject.GetMinTile(), tileObject.GetMaxTile());
			List<TileObject> list = new List<TileObject>();
			instance2.GetObjectsOfTypeInRect(tileObject.GetMinTile(), tileObject.GetMaxTile(), list, typeof(Character));
			foreach (Character item in list)
			{
				if (item.Tile.IsWithinBounds(tileObject.GetMinTile(), tileObject.GetMaxTile()) && tileObject.IsImpassable(item, 0, item.Tile))
				{
					TerrainCoord tile = BuildGoal.FindSuitableExitTileStatic(item, tileObject);
					item.SetPosition(instance2.GetTileCentrePos(tile));
				}
			}
			list.Clear();
		}
		return tileObject;
	}

	public bool CreateProduct(TileObject carrier, Community community, Equipment usingItem, TerrainCoord destTile, Prop.OrientationType orientation, float ingredientsNutrition, InfectionType ingredientsInfectedWith, out float numProduced, bool addToGatheredItems, List<Equipment> craftedItems = null)
	{
		Character character = carrier as Character;
		numProduced = 0f;
		if (ProductPropPrototype != null)
		{
			TileObject tileObject = PlaceProp(ProductPropPrototype, carrier, community, destTile, orientation, usingItem);
			if (tileObject != null)
			{
				numProduced += 1f;
				StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.BuiltSomething, character, tileObject);
			}
		}
		Equipment equipment = null;
		Equipment equipment2 = null;
		EquipmentPrototype productPrototype = ProductPrototype;
		if (productPrototype != null)
		{
			Equipment equipment3 = null;
			if (productPrototype.CanBeCombined)
			{
				Equipment equipment4 = Equipment.Spawn(productPrototype, ProductAmount);
				if (CanPassInfectionToProduct)
				{
					equipment4.Infect(ingredientsInfectedWith);
				}
				equipment3 = carrier.GetInventory().Add(carrier, equipment4);
				if (addToGatheredItems && character != null)
				{
					equipment3.IncrementGatheredAmount(ProductAmount);
				}
				numProduced += ProductAmount;
			}
			else
			{
				for (int i = 0; i < ProductAmount; i++)
				{
					Equipment equipment5 = Equipment.Spawn(productPrototype);
					equipment5.RandomiseVariations(Session.Instance.DeterministicRand);
					if (CanPassInfectionToProduct)
					{
						equipment5.Infect(ingredientsInfectedWith);
					}
					equipment3 = carrier.GetInventory().Add(carrier, equipment5);
					if (addToGatheredItems && character != null)
					{
						equipment3.IncrementGatheredAmount(1);
					}
					numProduced += 1f;
				}
			}
			if (character != null && equipment3 != null)
			{
				NotificationManager.Instance.AddEquipmentNotification(null, character, equipment3, ProductAmount);
			}
			if (equipment == null)
			{
				equipment = equipment3;
			}
			else
			{
				equipment2 = equipment3;
			}
			if (community != null && community.CommunityType == CommunityType.Player)
			{
				if (productPrototype == EquipmentPrototype.CornCookie)
				{
					AchievementsManager.Instance.IncrementAchievementStat(Achievement.Craft_Cookies, ProductAmount);
				}
				else if (productPrototype == EquipmentPrototype.ArmorPiercingARAmmo || productPrototype == EquipmentPrototype.ArmorPiercingSniperAmmo)
				{
					AchievementsManager.Instance.IncrementAchievementStat(Achievement.Craft_ArmorPiercingAmmo, ProductAmount);
				}
			}
		}
		List<Equipment> list = null;
		if (ExtraOutputs != null)
		{
			foreach (RecipeExtraOutput extraOutput in ExtraOutputs)
			{
				if (extraOutput.ProductPrototype == null || extraOutput.MaxAmount < extraOutput.MinAmount || !Session.Instance.DeterministicRand.RandomChoice((float)extraOutput.Probability / 100f))
				{
					continue;
				}
				int num = Session.Instance.DeterministicRand.Next(extraOutput.MinAmount, extraOutput.MaxAmount + 1);
				if (num <= 0)
				{
					continue;
				}
				Equipment equipment6 = null;
				for (int j = 0; j < (extraOutput.ProductPrototype.CanBeCombined ? 1 : num); j++)
				{
					int num2 = ((!extraOutput.ProductPrototype.CanBeCombined) ? 1 : num);
					equipment6 = Equipment.Spawn(extraOutput.ProductPrototype, num2);
					if (CanPassInfectionToProduct)
					{
						equipment6.Infect(ingredientsInfectedWith);
					}
					equipment6 = carrier.GetInventory().Add(carrier, equipment6);
					if (addToGatheredItems && character != null)
					{
						equipment6.IncrementGatheredAmount(num2);
					}
					numProduced += 1f;
					if (list == null)
					{
						list = new List<Equipment>();
					}
					list.Add(equipment6);
				}
				if (character != null && equipment6 != null)
				{
					NotificationManager.Instance.AddEquipmentNotification(null, character, equipment6, num);
				}
				if (community != null && community.CommunityType == CommunityType.Player)
				{
					if (extraOutput.ProductPrototype == EquipmentPrototype.CornCookie)
					{
						AchievementsManager.Instance.IncrementAchievementStat(Achievement.Craft_Cookies, num);
					}
					else if (extraOutput.ProductPrototype == EquipmentPrototype.ArmorPiercingARAmmo || extraOutput.ProductPrototype == EquipmentPrototype.ArmorPiercingSniperAmmo)
					{
						AchievementsManager.Instance.IncrementAchievementStat(Achievement.Craft_ArmorPiercingAmmo, num);
					}
				}
			}
		}
		if (ProductLiquidPrototype != null)
		{
			float num3 = ProductLiquidAmount;
			if (ProductLiquidPrototype.GetNutritionPerFlOz() > 0f)
			{
				num3 += ProductLiquidNutritionMultiplier * ingredientsNutrition / ProductLiquidPrototype.GetNutritionPerFlOz();
			}
			numProduced += num3;
			while (num3 > 0.0001f)
			{
				Equipment equipment7 = null;
				float num4 = 0f;
				for (int k = 0; k < carrier.GetInventory().Count; k++)
				{
					Equipment item = carrier.GetInventory().GetItem(k);
					float num5 = 0f;
					if (item.GetLiquidCapacity() > 0f)
					{
						if (!IsSuitableContainer(item))
						{
							continue;
						}
						num5 += 1f;
						if (item == usingItem)
						{
							num5 += 1f;
						}
						if (item == equipment)
						{
							num5 += 1f;
						}
						if (item == equipment2)
						{
							num5 += 1f;
						}
						if (list != null && list.Contains(item))
						{
							num5 += 1f;
						}
						if (item.GetLiquidContentsType() == ProductLiquidPrototype)
						{
							if (item.GetLiquidContentsAmount() >= item.GetLiquidCapacity() - 0.0001f)
							{
								continue;
							}
							num5 += 100f;
						}
						else if (item.GetLiquidContentsType() == null)
						{
							num5 += 10f;
						}
						if (item.GetPrototype().DefaultLiquidPrototype == ProductLiquidPrototype)
						{
							num5 += 5f;
						}
					}
					if (num5 > num4)
					{
						num4 = num5;
						equipment7 = item;
					}
				}
				if (equipment7 == null)
				{
					break;
				}
				if (equipment7.GetLiquidContentsAmount() > 0f && equipment7.GetLiquidContentsType() != ProductLiquidPrototype)
				{
					equipment7.DrainLiquid(equipment7.GetLiquidContentsAmount());
				}
				float num6 = Math.Min(num3, equipment7.GetLiquidCapacity() - equipment7.GetLiquidContentsAmount());
				equipment7.FillLiquid(ProductLiquidPrototype, num6, CanPassInfectionToProduct ? ingredientsInfectedWith : InfectionType.None);
				if (addToGatheredItems && character != null && equipment7.GetLiquidContentsAmount() >= equipment7.GetLiquidCapacity() - 0.01f)
				{
					equipment7.SetGathered();
				}
				num3 -= num6;
			}
			if (ProductLiquidPrototype == LiquidPrototype.Vodka && community != null && community.CommunityType == CommunityType.Player)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.Craft_Vodka);
			}
		}
		return true;
	}

	public void UseIngredients(Character carrier, Equipment usingItem, out float ingredientsNutrition, out InfectionType ingredientsInfectedWith, bool checkEquipmentPolicy)
	{
		UseIngredients(carrier, usingItem, null, out ingredientsNutrition, out ingredientsInfectedWith, out var _, checkEquipmentPolicy);
	}

	public void UseIngredients(Character carrier, Equipment usingItem, out float ingredientsNutrition, out InfectionType ingredientsInfectedWith, out bool usedHumanIngredients, bool checkEquipmentPolicy)
	{
		UseIngredients(carrier, usingItem, null, out ingredientsNutrition, out ingredientsInfectedWith, out usedHumanIngredients, checkEquipmentPolicy);
	}

	public void UseIngredients(Character carrier, Equipment usingItem, List<UsedIngredient> usedIngredients, out float ingredientsNutrition, out InfectionType ingredientsInfectedWith, out bool usedHumanIngredients, bool checkEquipmentPolicy)
	{
		ingredientsNutrition = 0f;
		ingredientsInfectedWith = InfectionType.None;
		usedHumanIngredients = false;
		usedIngredients?.Clear();
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.LiquidTypes == null)
			{
				continue;
			}
			float num = 0f;
			if (usingItem != null && usingItem.GetLiquidContentsType() != null && ingredient.LiquidTypes.Contains(usingItem.GetLiquidContentsType()))
			{
				LiquidPrototype liquidContentsType = usingItem.GetLiquidContentsType();
				float num2 = usingItem.DrainLiquid(ingredient.LiquidAmount);
				ingredientsNutrition += liquidContentsType.GetNutritionPerFlOz() * num2;
				ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)usingItem.InfectedWith);
				num += num2;
				usedIngredients?.Add(UsedIngredient.CreateLiquid(liquidContentsType, num2));
			}
			for (int i = 0; i < ingredient.LiquidTypes.Count; i++)
			{
				if (!(ingredient.LiquidAmount > num))
				{
					break;
				}
				LiquidPrototype liquidPrototype = ingredient.LiquidTypes[i];
				InfectionType infectedWith;
				float num3 = carrier.GetInventory().DrainLiquidOfType(carrier, liquidPrototype, ingredient.LiquidAmount - num, this, out infectedWith, checkEquipmentPolicy ? carrier : null);
				ingredientsNutrition += liquidPrototype.GetNutritionPerFlOz() * num3;
				ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)infectedWith);
				num += num3;
				usedIngredients?.Add(UsedIngredient.CreateLiquid(liquidPrototype, num3));
			}
		}
		foreach (Ingredient ingredient2 in Ingredients)
		{
			if (ingredient2.Prototypes == null)
			{
				continue;
			}
			int num4 = 0;
			if (usingItem != null && ingredient2.Prototypes.Contains(usingItem.GetPrototype()))
			{
				EquipmentPrototype prototype = usingItem.GetPrototype();
				int num5 = carrier.GetInventory().UseItem(carrier, usingItem, ingredient2.Amount);
				ingredientsNutrition += prototype.GetNutrition() * (float)num5;
				ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)usingItem.InfectedWith);
				num4 += num5;
				usedHumanIngredients |= num5 > 0 && prototype.ContainsHumanMeat;
				usedIngredients?.Add(UsedIngredient.CreateItem(prototype, num5));
			}
			for (int j = 0; j < ingredient2.Prototypes.Count; j++)
			{
				if (ingredient2.Amount <= num4)
				{
					break;
				}
				EquipmentPrototype equipmentPrototype = ingredient2.Prototypes[j];
				InfectionType infectionType;
				int num6 = carrier.GetInventory().UseItemOfType(carrier, carrier, equipmentPrototype, ingredient2.Amount - num4, this, out infectionType, ingredient2.IngredientInfectionState, checkEquipmentPolicy);
				ingredientsNutrition += equipmentPrototype.GetNutrition() * (float)num6;
				ingredientsInfectedWith = (InfectionType)Math.Max((int)ingredientsInfectedWith, (int)infectionType);
				num4 += num6;
				usedHumanIngredients |= num6 > 0 && equipmentPrototype.ContainsHumanMeat;
				usedIngredients?.Add(UsedIngredient.CreateItem(equipmentPrototype, num6));
			}
		}
		carrier.Skillset.AddProgress(carrier, SkillType, SkillProgression);
		if (usedHumanIngredients && IsProductDrinkableOrEdible())
		{
			Memory.OnMemorableEvent(MemoryPrototype.UsedHumanIngredients, carrier, null, 1f, secret: false);
		}
	}

	public bool CalcNutritionFromIngredients(TileObject carrier, Equipment usingItem, out float ingredientsNutrition)
	{
		ingredientsNutrition = 0f;
		bool result = true;
		foreach (Ingredient ingredient in Ingredients)
		{
			if (ingredient.LiquidTypes == null)
			{
				continue;
			}
			float num = 0f;
			if (usingItem != null && usingItem.GetLiquidContentsType() != null && ingredient.LiquidTypes.Contains(usingItem.GetLiquidContentsType()))
			{
				float num2 = Math.Min(usingItem.GetLiquidContentsAmount(), ingredient.LiquidAmount);
				ingredientsNutrition += usingItem.GetLiquidContentsType().GetNutritionPerFlOz() * num2;
				num += num2;
			}
			for (int i = 0; i < ingredient.LiquidTypes.Count; i++)
			{
				if (!(ingredient.LiquidAmount > num))
				{
					break;
				}
				LiquidPrototype liquidPrototype = ingredient.LiquidTypes[i];
				foreach (Equipment content in carrier.GetInventory().Contents)
				{
					if (content.GetLiquidContentsType() == liquidPrototype)
					{
						float num3 = Math.Min(content.GetLiquidContentsAmount(), ingredient.LiquidAmount - num);
						ingredientsNutrition += liquidPrototype.GetNutritionPerFlOz() * num3;
						num += num3;
						if (num >= ingredient.LiquidAmount)
						{
							break;
						}
					}
				}
			}
			if (num < ingredient.LiquidAmount)
			{
				result = false;
			}
		}
		foreach (Ingredient ingredient2 in Ingredients)
		{
			if (ingredient2.Prototypes == null)
			{
				continue;
			}
			int num4 = 0;
			if (usingItem != null && ingredient2.Prototypes.Contains(usingItem.GetPrototype()))
			{
				int num5 = Math.Min(usingItem.GetAmount(), ingredient2.Amount);
				ingredientsNutrition += usingItem.GetPrototype().GetNutrition() * (float)num5;
				num4 += num5;
			}
			for (int j = 0; j < ingredient2.Prototypes.Count; j++)
			{
				if (ingredient2.Amount <= num4)
				{
					break;
				}
				Equipment equipment = carrier.GetInventory().FindItemOfType(ingredient2.Prototypes[j]);
				if (equipment != null)
				{
					int num6 = Math.Min(equipment.GetAmount(), ingredient2.Amount - num4);
					ingredientsNutrition += ingredient2.Prototypes[j].GetNutrition() * (float)num6;
					num4 += num6;
					if (num4 >= ingredient2.Amount)
					{
						break;
					}
				}
			}
			if (num4 < ingredient2.Amount)
			{
				result = false;
			}
		}
		return result;
	}

	public string GetNameKey()
	{
		return "RECIPE_" + UniqueID;
	}

	public void CacheStuff()
	{
		List<EquipmentPrototype> list = new List<EquipmentPrototype>();
		List<LiquidPrototype> list2 = new List<LiquidPrototype>();
		ProductPrototype = ((!string.IsNullOrEmpty(ProductPrototypeName)) ? GameImpl.Instance.FindEquipmentPrototypeByName(ProductPrototypeName) : null);
		ProductLiquidPrototype = ((!string.IsNullOrEmpty(ProductLiquidPrototypeName)) ? GameImpl.Instance.FindLiquidPrototypeByName(ProductLiquidPrototypeName) : null);
		ProductPropPrototype = ((!string.IsNullOrEmpty(ProductType)) ? GameImpl.Instance.FindPropPrototypeByName(ProductType) : null);
		CraftingStationPrototype = ((!string.IsNullOrEmpty(CraftingStationType)) ? GameImpl.Instance.FindPropPrototypeByName(CraftingStationType) : null);
		foreach (Ingredient ingredient in Ingredients)
		{
			ingredient.Prototypes = null;
			ingredient.LiquidTypes = null;
		}
		foreach (Ingredient ingredient2 in Ingredients)
		{
			if (ingredient2.PrototypeNames != null && ingredient2.PrototypeNames.Count > 0)
			{
				for (int i = 0; i < ingredient2.PrototypeNames.Count; i++)
				{
					EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(ingredient2.PrototypeNames[i]);
					if (equipmentPrototype != null)
					{
						if (ingredient2.Prototypes == null)
						{
							ingredient2.Prototypes = new List<EquipmentPrototype>();
						}
						ingredient2.Prototypes.Add(equipmentPrototype);
						if (list.Contains(equipmentPrototype))
						{
							Debug.LogError("Can't have the same ingredient type twice in a recipe: " + UniqueID + ": " + equipmentPrototype.Name);
						}
						else
						{
							list.Add(equipmentPrototype);
						}
					}
				}
			}
			else
			{
				ingredient2.PrototypeNames = null;
			}
			if (ingredient2.LiquidTypeNames != null && ingredient2.LiquidTypeNames.Count > 0)
			{
				for (int j = 0; j < ingredient2.LiquidTypeNames.Count; j++)
				{
					LiquidPrototype liquidPrototype = GameImpl.Instance.FindLiquidPrototypeByName(ingredient2.LiquidTypeNames[j]);
					if (liquidPrototype != null)
					{
						if (ingredient2.LiquidTypes == null)
						{
							ingredient2.LiquidTypes = new List<LiquidPrototype>();
						}
						ingredient2.LiquidTypes.Add(liquidPrototype);
						if (list2.Contains(liquidPrototype))
						{
							Debug.LogError("Can't have the same liquid type twice in a recipe: " + UniqueID + ": " + liquidPrototype);
						}
						else
						{
							list2.Add(liquidPrototype);
						}
					}
				}
			}
			else
			{
				ingredient2.LiquidTypeNames = null;
			}
			if (ingredient2.IsLiquid())
			{
				ingredient2.Amount = 0;
				ingredient2.LiquidAmount = Math.Max(ingredient2.LiquidAmount, 1f);
			}
			else
			{
				ingredient2.Amount = Math.Max(ingredient2.Amount, 1);
				ingredient2.LiquidAmount = 0f;
			}
		}
		if (ExtraOutputs == null)
		{
			return;
		}
		foreach (RecipeExtraOutput extraOutput in ExtraOutputs)
		{
			extraOutput.ProductPrototype = ((!string.IsNullOrEmpty(extraOutput.ProductPrototypeName)) ? GameImpl.Instance.FindEquipmentPrototypeByName(extraOutput.ProductPrototypeName) : null);
		}
		if (ExtraOutputs.Count == 0)
		{
			ExtraOutputs = null;
		}
	}

	public bool IsDiscovered()
	{
		ShowRecipe showRecipe = ShowRecipe;
		if ((uint)(showRecipe - 4) <= 2u && SkillType > SkillType.Invalid && SkillType < SkillType.Count && Session.Instance.HighestSkillLevel[(int)SkillType] < SkillLevel)
		{
			return false;
		}
		switch (ShowRecipe)
		{
		case ShowRecipe.IfAllIngredientsFound:
		case ShowRecipe.IfAllIngredientsFoundAndHasSkill:
			foreach (Ingredient ingredient in Ingredients)
			{
				if (ingredient.Prototypes != null)
				{
					bool flag = false;
					foreach (EquipmentPrototype prototype in ingredient.Prototypes)
					{
						flag |= prototype.Discovered;
					}
					if (!flag)
					{
						return false;
					}
				}
				if (ingredient.LiquidTypes == null)
				{
					continue;
				}
				bool flag2 = false;
				foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
				{
					flag2 |= liquidType.Discovered;
				}
				if (!flag2)
				{
					return false;
				}
			}
			switch (RecipeType)
			{
			case RecipeType.Toolbox:
			{
				bool flag4 = false;
				foreach (EquipmentPrototype toolbox in EquipmentPrototype.Toolboxes)
				{
					flag4 |= toolbox.Discovered;
				}
				if (!flag4)
				{
					return false;
				}
				break;
			}
			case RecipeType.Shovel:
			{
				bool flag3 = false;
				foreach (EquipmentPrototype shovel in EquipmentPrototype.Shovels)
				{
					flag3 |= shovel.Discovered;
				}
				if (!flag3)
				{
					return false;
				}
				break;
			}
			default:
				if (CraftingStationPrototype != null && !CraftingStationPrototype.Discovered)
				{
					return false;
				}
				break;
			}
			return true;
		case ShowRecipe.IfAnyIngredientsFound:
		case ShowRecipe.IfAnyIngredientsFoundAndHasSkill:
			foreach (Ingredient ingredient2 in Ingredients)
			{
				if (ingredient2.Prototypes != null)
				{
					foreach (EquipmentPrototype prototype2 in ingredient2.Prototypes)
					{
						if (prototype2.Discovered)
						{
							return true;
						}
					}
				}
				if (ingredient2.LiquidTypes == null)
				{
					continue;
				}
				foreach (LiquidPrototype liquidType2 in ingredient2.LiquidTypes)
				{
					if (liquidType2.Discovered)
					{
						return true;
					}
				}
			}
			switch (RecipeType)
			{
			case RecipeType.Toolbox:
				foreach (EquipmentPrototype toolbox2 in EquipmentPrototype.Toolboxes)
				{
					if (toolbox2.Discovered)
					{
						return true;
					}
				}
				break;
			case RecipeType.Shovel:
				foreach (EquipmentPrototype shovel2 in EquipmentPrototype.Shovels)
				{
					if (shovel2.Discovered)
					{
						return true;
					}
				}
				break;
			default:
				if (CraftingStationPrototype != null && CraftingStationPrototype.Discovered)
				{
					return true;
				}
				break;
			}
			return false;
		case ShowRecipe.IfImplementFound:
		case ShowRecipe.IfImplementFoundAndHasSkill:
			switch (RecipeType)
			{
			case RecipeType.Toolbox:
				foreach (EquipmentPrototype toolbox3 in EquipmentPrototype.Toolboxes)
				{
					if (toolbox3.Discovered)
					{
						return true;
					}
				}
				break;
			case RecipeType.Shovel:
				foreach (EquipmentPrototype shovel3 in EquipmentPrototype.Shovels)
				{
					if (shovel3.Discovered)
					{
						return true;
					}
				}
				break;
			case RecipeType.Normal:
			case RecipeType.HuntingKnife:
				return true;
			default:
				if (CraftingStationPrototype == null || CraftingStationPrototype.Discovered)
				{
					return true;
				}
				break;
			}
			return false;
		case ShowRecipe.Always:
			return true;
		default:
			return false;
		}
	}
}
