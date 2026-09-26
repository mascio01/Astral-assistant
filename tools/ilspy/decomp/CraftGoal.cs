using System;
using System.Collections.Generic;
using UnityEngine;

public class CraftGoal : RoleGoal
{
	public Recipe FollowingRecipe;

	public Recipe LastPickedCookingRecipe;

	public Equipment UsingItem;

	public int DesiredAmount = int.MaxValue;

	public float MaxDistToTravel = float.MaxValue;

	public bool WasTriggeredFromDirectControl;

	public TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	public TimeSpan LastFailedToReplenishFireTime = TimeSpan.FromDays(-365.0);

	private TimeSpan LastCheckedBestRoleTime = Target.Never;

	public bool HasSaidInsufficientResources;

	public bool HasSaidNeedContainer;

	public bool HasSaidTooHeavy;

	public bool UsedHumanIngredients;

	public TerrainCoord StandOnTile;

	public TerrainCoord DestTile;

	public Prop.OrientationType DestOrientationType;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	private static List<Recipe> TempRecipes = new List<Recipe>();

	private static float MaxCraftingPropDist = float.MaxValue;

	public static float StoreCraftedNutritionThreshold = Sun.DayLengthSecs * 8f;

	public override GoalType GetGoalType()
	{
		return GoalType.CraftGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is AnimationGoal)
		{
			return null;
		}
		return (FollowingRecipe != null && FollowingRecipe.IsProductDrinkableOrEdible()) ? GameCursor.CursorCook : GameCursor.CursorCraft;
	}

	public void SubtractDesiredAmount()
	{
		if (DesiredAmount != int.MaxValue)
		{
			DesiredAmount = Math.Max(0, DesiredAmount - 1);
		}
	}

	public bool WantCheckEquipmentPolicy(Character character)
	{
		return DesiredAmount == int.MaxValue;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref FollowingRecipe);
		reflector.AddAfter(ref LastPickedCookingRecipe, 538);
		reflector.Add(ref UsingItem);
		reflector.Add(ref DesiredAmount);
		reflector.AddAfter(ref MaxDistToTravel, 376);
		reflector.AddAfter(ref WasTriggeredFromDirectControl, 11);
		reflector.Add(ref _lastAttemptedTime);
		reflector.AddAfter(ref LastFailedToReplenishFireTime, 572);
		reflector.AddAfter(ref LastCheckedBestRoleTime, 441);
		reflector.Add(ref MovementType);
		if (reflector.Version < 41)
		{
			bool value = false;
			reflector.Add(ref value);
		}
		if (reflector.Version < 69)
		{
			bool value2 = false;
			reflector.Add(ref value2);
		}
		if (reflector.Version != 367)
		{
			reflector.Add(ref HasSaidInsufficientResources);
		}
		reflector.AddAfter(ref HasSaidNeedContainer, 73);
		reflector.AddAfter(ref HasSaidTooHeavy, 503);
		reflector.AddAfter(ref UsedHumanIngredients, 574);
		reflector.Add(ref StandOnTile);
		reflector.Add(ref DestTile);
		reflector.Add(ref DestOrientationType);
		if (reflector.Version >= 70 && reflector.Version < 72)
		{
			List<EquipmentPrototype> list = null;
			reflector.AddEquipmentPrototypeList(ref list);
		}
		else
		{
			if (reflector.Version < 72 || reflector.Version >= 353)
			{
				return;
			}
			List<Equipment> list2 = new List<Equipment>();
			if (reflector.Version < 233)
			{
				reflector.Add(ref list2);
			}
			else
			{
				reflector.AddGameObjectRefList(ref list2);
			}
			if (character.GetGatherGoal() == null)
			{
				return;
			}
			foreach (Equipment item in list2)
			{
				item.SetGathered();
			}
		}
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	public void SetLastAttemptedTimeToNow()
	{
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	private void OnFailed(Character character)
	{
		Role roleBeingPerformed = GetRoleBeingPerformed(character);
		if (roleBeingPerformed != Role.None)
		{
			RoleInfo roleInfo = new RoleInfo(roleBeingPerformed, FollowingRecipe, DestTile);
			OnFailed(character, roleInfo);
		}
		else
		{
			_lastAttemptedTime = Session.Instance.PlayTime;
		}
	}

	public void OnFailed(Character character, RoleInfo roleInfo)
	{
		bool flag = false;
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (!character.Roles[i].Paused && !character.Roles[i].FailedRecently && (character.Roles[i].Role == Role.Crafter || character.Roles[i].Role == Role.Cook) && !character.Roles[i].Equals(roleInfo))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			_lastAttemptedTime = Session.Instance.PlayTime;
		}
		character.SetRoleFailedRecently(roleInfo);
	}

	private bool IsFinishingUp()
	{
		if (SubGoal is AnimationGoal animationGoal && (animationGoal.GetAnim() == ActionAnim.CraftEnd || animationGoal.GetAnim() == ActionAnim.CraftTableEnd || animationGoal.GetAnim() == ActionAnim.ForgeEnd || animationGoal.GetAnim() == ActionAnim.PotEnd))
		{
			return true;
		}
		if (SubGoal is Wait)
		{
			return true;
		}
		if (SubGoal is MoveToAndTake)
		{
			return true;
		}
		return false;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!Active || !IsFinishingUp())
		{
			if (DesiredAmount == int.MaxValue)
			{
				if (character.Community == null)
				{
					return false;
				}
				bool flag = false;
				for (int i = 0; i < character.Roles.Count; i++)
				{
					if (!character.Roles[i].Paused && !character.Roles[i].FailedRecently && !(Session.Instance.PlayTime - character.Roles[i].LastFailedTime < MinTimeBetweenAttempts))
					{
						if (character.Roles[i].Role == Role.Crafter && character.Roles[i].Recipe != null)
						{
							flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
						}
						else if (character.Roles[i].Role == Role.Cook)
						{
							flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
						}
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else if (DesiredAmount <= 0 || FollowingRecipe == null)
			{
				return false;
			}
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, (DesiredAmount == int.MaxValue) ? Role.Crafter : Role.None, WasTriggeredFromDirectControl, isSpeechGoal: false) && parent is SurvivorGoal)
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
		if (DesiredAmount != int.MaxValue)
		{
			if (!WasTriggeredFromDirectControl)
			{
				return GoalPriority.Survivor_Crafting;
			}
			return GoalPriority.Survivor_ObeyLeader_Craft;
		}
		if (Active)
		{
			if (character.IsLightingFire())
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
			if (character.IsCraftingStandaloneAnim())
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (character.SquadLeader == null)
			{
				bool flag = SubGoal is LightFireGoal || SubGoal is MoveToAndAddMaterialToFire;
				if (flag && character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusShivering)
				{
					return GoalPriority.Survivor_RoleByFire_Critical;
				}
				if (FollowingRecipe != null)
				{
					BaseObjectType baseObjectType = FollowingRecipe.RequiredPropToWorkOn();
					if (baseObjectType == BaseObjectType.Campfire || (uint)(baseObjectType - 161) <= 1u)
					{
						CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
						if (craftingPropOnTile != null && FollowingRecipe.IsCraftingPropForRecipe(craftingPropOnTile) && (craftingPropOnTile.CurrentCrafter == character || !craftingPropOnTile.IsCrafting()))
						{
							if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusShivering && (craftingPropOnTile.CurrentCrafter == character || GameCursor.CanStartCraftingWithProp(character, craftingPropOnTile, FollowingRecipe, UsingItem, ingredientsMustBeOnMe: true, WantCheckEquipmentPolicy(character), checkIfCrafting: true) == CursorActionDisabledReason.Enabled))
							{
								return GoalPriority.Survivor_RoleByFire_Critical;
							}
							if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusNormal)
							{
								return GoalPriority.Survivor_RoleByFire;
							}
						}
					}
				}
				if (flag)
				{
					return GoalPriority.Survivor_RoleByFire;
				}
			}
			Role roleBeingPerformed = GetRoleBeingPerformed(character);
			if (roleBeingPerformed != Role.None)
			{
				int roleIndex = character.GetRoleIndex(new RoleInfo(roleBeingPerformed, FollowingRecipe, DestTile));
				if (roleIndex != -1)
				{
					if (FollowingRecipe != null)
					{
						BaseObjectType baseObjectType = FollowingRecipe.RequiredPropToWorkOn();
						if (baseObjectType == BaseObjectType.Kiln || baseObjectType == BaseObjectType.Still || baseObjectType == BaseObjectType.Nitrary)
						{
							CraftingProp craftingPropOnTile2 = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
							if (craftingPropOnTile2 != null && FollowingRecipe.IsCraftingPropForRecipe(craftingPropOnTile2) && craftingPropOnTile2.IsCrafting() && craftingPropOnTile2.CurrentCrafter == character)
							{
								return (GoalPriority)(150 - roleIndex);
							}
						}
					}
					return (GoalPriority)(210 - roleIndex);
				}
			}
			return GoalPriority.Impossible;
		}
		GetHighestPriorityRoleIndex(character, out var highestPriority);
		return highestPriority;
	}

	private void CheckPriority(ref int index, ref GoalPriority highestPriority, int i, GoalPriority priority)
	{
		if (priority > highestPriority)
		{
			index = i;
			highestPriority = priority;
		}
	}

	private int GetHighestPriorityRoleIndex(Character character, out GoalPriority highestPriority)
	{
		int index = -1;
		highestPriority = GoalPriority.Impossible;
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if ((character.Roles[i].Role != Role.Crafter && character.Roles[i].Role != Role.Cook) || character.Roles[i].Paused || character.Roles[i].FailedRecently || Session.Instance.PlayTime - character.Roles[i].LastFailedTime < MinTimeBetweenAttempts)
			{
				continue;
			}
			Recipe recipe = character.Roles[i].Recipe;
			if (recipe != null && !character.CanUseRecipe(recipe))
			{
				continue;
			}
			bool flag = false;
			if (character.Roles[i].Role == Role.Cook)
			{
				flag = true;
				if (LastPickedCookingRecipe != null)
				{
					BaseObjectType baseObjectType = LastPickedCookingRecipe.RequiredPropToWorkOn();
					if (baseObjectType != BaseObjectType.Campfire && (uint)(baseObjectType - 161) > 1u)
					{
						flag = false;
					}
				}
			}
			CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(character.Roles[i].TargetLocation.x, character.Roles[i].TargetLocation.y);
			if (craftingPropOnTile != null && (flag || (recipe != null && recipe.IsCraftingPropForRecipe(craftingPropOnTile))))
			{
				BaseObjectType baseObjectType = craftingPropOnTile.GetBaseObjectType();
				if ((baseObjectType == BaseObjectType.Campfire || (uint)(baseObjectType - 161) <= 1u) && character.SquadLeader == null && (craftingPropOnTile.CurrentCrafter == character || !craftingPropOnTile.IsCrafting()))
				{
					if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusMildHypothermia && (craftingPropOnTile.CurrentCrafter == character || (recipe != null && GameCursor.CanStartCraftingWithProp(character, craftingPropOnTile, recipe, null, ingredientsMustBeOnMe: true, WantCheckEquipmentPolicy(character), checkIfCrafting: true) == CursorActionDisabledReason.Enabled)))
					{
						CheckPriority(ref index, ref highestPriority, i, GoalPriority.Survivor_RoleByFire_Critical);
					}
					if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusShivering)
					{
						CheckPriority(ref index, ref highestPriority, i, GoalPriority.Survivor_RoleByFire);
					}
				}
				baseObjectType = craftingPropOnTile.GetBaseObjectType();
				if ((baseObjectType == BaseObjectType.Kiln || baseObjectType == BaseObjectType.Still || baseObjectType == BaseObjectType.Nitrary) && craftingPropOnTile.IsCrafting() && craftingPropOnTile.CurrentCrafter == character)
				{
					CheckPriority(ref index, ref highestPriority, i, (GoalPriority)(150 - i));
					continue;
				}
			}
			CheckPriority(ref index, ref highestPriority, i, (GoalPriority)(210 - i));
		}
		return index;
	}

	public static Recipe PickCookRecipe(Character character, CraftingProp craftingProp, bool checkIfAllowedToEatIt, bool prioritiseSkinning, CustomRandom rand)
	{
		Campfire campfire = craftingProp as Campfire;
		TempRecipes.Clear();
		float num = float.MinValue;
		float num2 = float.MinValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		Recipe recipe = null;
		Recipe recipe2 = null;
		foreach (KeyValuePair<string, Recipe> item in GameImpl.Instance.CurrentRecipesDeterministic)
		{
			Recipe value = item.Value;
			if (!value.IsValidRecipeForCook() || value.OnlyUseManually || value.Deprecated || !character.CanUseRecipe(value) || character.Community.HasReachedCraftingLimitForProduct(value) || (!value.HasAllIngredients(character, character, null, character) && !value.CommunityHasEnoughSpareIngredientsWithinMovementZone(character)))
			{
				continue;
			}
			bool flag = false;
			switch (value.RecipeType)
			{
			case RecipeType.Campfire_Pot:
			{
				Campfire campfire2 = campfire;
				if (campfire2 == null || !value.IsCraftingPropForRecipe(campfire2))
				{
					campfire2 = character.Community.GetNearestCraftingPropForRecipe(value, character, MaxCraftingPropDist) as Campfire;
				}
				if (campfire2 != null)
				{
					flag = campfire2.Inventory.FindItemOfType(EquipmentPrototype.Pot) != null || character.Inventory.FindItemOfType(EquipmentPrototype.Pot) != null || character.Community.HasUsableInventoryItemsOfTypeWithinMovementZone(character, EquipmentPrototype.Pot, 1);
				}
				break;
			}
			case RecipeType.Campfire_FryingPan:
			{
				Campfire campfire3 = campfire;
				if (campfire3 == null || !value.IsCraftingPropForRecipe(campfire3))
				{
					campfire3 = character.Community.GetNearestCraftingPropForRecipe(value, character, MaxCraftingPropDist) as Campfire;
				}
				if (campfire3 != null)
				{
					flag = campfire3.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) != null || character.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) != null || character.Community.HasUsableInventoryItemsOfTypeWithinMovementZone(character, EquipmentPrototype.FryingPan, 1);
				}
				break;
			}
			case RecipeType.Campfire_SpitRoast_Rabbit:
			case RecipeType.Campfire_SpitRoast_Chicken:
			case RecipeType.Campfire_SpitRoast_Venison:
			case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
				flag = (campfire != null && value.IsCraftingPropForRecipe(campfire)) || character.Community.GetNearestCraftingPropForRecipe(value, character, MaxCraftingPropDist) != null;
				break;
			case RecipeType.Toolbox:
				flag = character.Inventory.GetToolbox() != null || character.Community.HasUsableInventoryItemsOfClassWithinMovementZone(character, typeof(Toolbox), 1);
				flag &= value.HasSuitableContainer(character, null);
				break;
			case RecipeType.Shovel:
				flag = character.Inventory.GetShovel() != null || character.Community.HasUsableInventoryItemsOfClassWithinMovementZone(character, typeof(Shovel), 1);
				flag &= value.HasSuitableContainer(character, null);
				break;
			case RecipeType.HuntingKnife:
				flag = character.Inventory.GetHuntingKnife() != null || character.Community.HasUsableInventoryItemsOfClassWithinMovementZone(character, typeof(HuntingKnife), 1);
				flag &= value.HasSuitableContainer(character, null);
				break;
			default:
				flag = value.HasSuitableContainer(character, null);
				flag &= (craftingProp != null && value.IsCraftingPropForRecipe(craftingProp)) || character.Community.GetNearestCraftingPropForRecipe(value, character, MaxCraftingPropDist) != null;
				break;
			}
			if (flag && (!checkIfAllowedToEatIt || character.IsActionAllowedForItem(value.ProductPrototype, value.ProductLiquidPrototype, InfectionType.None, EquipmentPolicyAction.CanUse)) && (value.ProductPrototype == null || !value.ProductPrototype.ContainsHumanMeat || !character.ShouldRefuseToEatHumanMeat() || !character.IsControllableByPlayer()))
			{
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				if (value.RecipeType == RecipeType.HuntingKnife && value.ProductPrototype != null && !value.ProductPrototype.ContainsHumanMeat && prioritiseSkinning && !character.Community.HasUsableInventoryItemsOfTypeWithinMovementZone(character, value.ProductPrototype, 1))
				{
					num5 += 1000f;
					num6 += 1000f;
				}
				if (value.ProductLiquidPrototype != null && value.ProductLiquidPrototype.NutritionPerFlOz > 0f)
				{
					num5 += value.ProductLiquidPrototype.SkillOnConsumptionProgressionPerFlOz / value.ProductLiquidPrototype.NutritionPerFlOz;
					num6 += character.CalcTastiness(value.ProductLiquidPrototype);
					num7 += value.ProductLiquidAmount * value.ProductLiquidPrototype.NutritionPerFlOz;
				}
				if (value.ProductPrototype != null && value.ProductPrototype.Nutrition > 0f)
				{
					num5 += value.ProductPrototype.SkillOnConsumptionProgression / value.ProductPrototype.Nutrition;
					num6 += character.CalcTastiness(value.ProductPrototype);
					num7 += (float)value.ProductAmount * value.ProductPrototype.Nutrition;
				}
				if (num5 > num2 || (num5 == num2 && num7 > num4))
				{
					recipe2 = value;
					num4 = num7;
					num2 = num5;
				}
				if (num6 > num || (num6 == num && num7 > num3))
				{
					recipe = value;
					num3 = num7;
					num = num6;
				}
			}
		}
		if (rand != null)
		{
			if (recipe2 != null)
			{
				TempRecipes.Add(recipe2);
			}
			if (recipe != null)
			{
				TempRecipes.Add(recipe);
			}
			if (TempRecipes.Count > 0)
			{
				return TempRecipes[rand.Next(TempRecipes.Count)];
			}
			return null;
		}
		return recipe;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching && DesiredAmount != int.MaxValue && FollowingRecipe != null && FollowingRecipe.RecipeType == RecipeType.Normal;
		base.OnActivate(character, parent);
		if (DesiredAmount == int.MaxValue && character.Community != null)
		{
			GoalPriority highestPriority;
			int highestPriorityRoleIndex = GetHighestPriorityRoleIndex(character, out highestPriority);
			if (highestPriorityRoleIndex == -1)
			{
				OnFailed(character);
				Finished = true;
				return;
			}
			if (character.Roles[highestPriorityRoleIndex].Role == Role.Cook)
			{
				TerrainCoord terrainCoord = character.Roles[highestPriorityRoleIndex].TargetLocation;
				CraftingProp craftingProp = ((terrainCoord != TerrainCoord.Invalid) ? GameTerrain.Instance.GetCraftingPropOnTile(terrainCoord.x, terrainCoord.y) : null);
				if (craftingProp == null)
				{
					float num = float.MaxValue;
					Campfire campfire = null;
					foreach (Prop building in character.Community.Buildings)
					{
						if (building is Campfire campfire2)
						{
							float distSquared = campfire2.Tile.GetDistSquared(character.Tile);
							if (distSquared < num && !character.Community.IsAnyMemberCookingWithFire(campfire2) && (!character.Community.IsAISettlement() || character.Community.IsTileInsidePerimeter(campfire2.Tile)))
							{
								num = distSquared;
								campfire = campfire2;
							}
						}
					}
					if (campfire != null)
					{
						RoleInfo value = character.Roles[highestPriorityRoleIndex];
						value.TargetLocation = campfire.Tile;
						character.Roles[highestPriorityRoleIndex] = value;
						terrainCoord = campfire.Tile;
						craftingProp = campfire;
					}
				}
				Campfire campfire3 = craftingProp as Campfire;
				if (campfire3 != null && campfire3.Inventory.GetTotalNutrition() > 0f)
				{
					OnFailed(character, new RoleInfo(Role.Cook));
					Finished = true;
					return;
				}
				if (campfire3 != null && !campfire3.IsBurning() && character.Community != null && !character.Community.IsAnyMemberLightingFire(campfire3))
				{
					DestTile = campfire3.GetCentreTile();
					SetSubGoal(character, parent, new LightFireGoal(character, null, campfire3, MovementType, canAddMaterialToFire: true));
					return;
				}
				if (campfire3 != null && campfire3.IsBurning() && campfire3.WoodRemaining < WarmGoal.ReplenishWoodAmount && Session.Instance.PlayTime - LastFailedToReplenishFireTime > TimeSpan.FromSeconds(120.0) && character.Community != null && !character.Community.IsAnyMemberLightingFire(campfire3))
				{
					DestTile = campfire3.GetCentreTile();
					SetSubGoal(character, parent, new MoveToAndAddMaterialToFire(character, campfire3.Tile, null, MovementType, critical: false));
					return;
				}
				if (craftingProp != null && craftingProp.IsCrafting())
				{
					if (craftingProp.CurrentCrafter != character)
					{
						OnFailed(character, new RoleInfo(Role.Cook));
						Finished = true;
						return;
					}
					if (FollowingRecipe != craftingProp.CraftingRecipe)
					{
						StartRecipe(character, craftingProp.CraftingRecipe, null, int.MaxValue, craftingProp.GetCentreTile(), Prop.OrientationType.Count, WasTriggeredFromDirectControl, character.Roles[highestPriorityRoleIndex].Urgent);
					}
				}
				else
				{
					CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
					Recipe recipe = (LastPickedCookingRecipe = PickCookRecipe(character, craftingProp, checkIfAllowedToEatIt: false, prioritiseSkinning: true, Session.Instance.DeterministicRand));
					if (recipe == null)
					{
						OnFailed(character, new RoleInfo(Role.Cook));
						Finished = true;
						return;
					}
					if (recipe.RequiredPropToWorkOn() != BaseObjectType.Invalid)
					{
						if (craftingProp != null && recipe.IsCraftingPropForRecipe(craftingProp))
						{
							terrainCoord = craftingProp.GetCentreTile();
						}
						else if (character.Community.GetNearestCraftingPropForRecipe(recipe, character, MaxCraftingPropDist) is CraftingProp craftingProp2)
						{
							terrainCoord = craftingProp2.GetCentreTile();
						}
					}
					StartRecipe(character, recipe, null, int.MaxValue, terrainCoord, Prop.OrientationType.Deg0, WasTriggeredFromDirectControl, character.Roles[highestPriorityRoleIndex].Urgent);
				}
			}
			else if (character.Roles[highestPriorityRoleIndex].Role == Role.Crafter && character.Roles[highestPriorityRoleIndex].Recipe != null)
			{
				if (FollowingRecipe != character.Roles[highestPriorityRoleIndex].Recipe)
				{
					StartRecipe(character, character.Roles[highestPriorityRoleIndex].Recipe, null, int.MaxValue, character.Roles[highestPriorityRoleIndex].TargetLocation, Prop.OrientationType.Count, WasTriggeredFromDirectControl, character.Roles[highestPriorityRoleIndex].Urgent);
				}
				else
				{
					DestTile = character.Roles[highestPriorityRoleIndex].TargetLocation;
				}
			}
			character.SetRoleInProgress(highestPriorityRoleIndex, inProgress: true);
		}
		bool fail = false;
		StandOnTile = GetTileToStandOn(character, out fail);
		if (fail)
		{
			OnFailed(character);
			Finished = true;
			return;
		}
		Goal nextSubGoal = GetNextSubGoal(character, parent);
		if (nextSubGoal != null)
		{
			SetSubGoal(character, parent, nextSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		if (DesiredAmount == 0 && !HasUsedIngredientsWaitingForProduct(character))
		{
			StopRecipe(character);
		}
		WasTriggeredFromDirectControl = false;
	}

	private TerrainCoord GetTileToStandOn(Character character, out bool fail)
	{
		fail = false;
		if (FollowingRecipe == null)
		{
			return TerrainCoord.Invalid;
		}
		if (FollowingRecipe.ProductPropPrototype != null && FollowingRecipe.ProductPropPrototype.ProtoInstance is Prop prop)
		{
			prop.CalcMinMaxTile(DestTile, DestOrientationType, out var minTile, out var maxTile);
			return new TerrainRect(minTile, maxTile).GetNearestPassableTileTo(character.Tile, 2049, character, null);
		}
		switch (FollowingRecipe.RequiredPropToWorkOn())
		{
		case BaseObjectType.Campfire:
			if (GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire campfire)
			{
				return campfire.GetTileToStandOn(character);
			}
			fail = true;
			break;
		case BaseObjectType.WorkBench:
		case BaseObjectType.Kiln:
		case BaseObjectType.Forge:
		case BaseObjectType.Still:
		case BaseObjectType.Nitrary:
		{
			CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
			if (craftingPropOnTile != null && FollowingRecipe.IsCraftingPropForRecipe(craftingPropOnTile))
			{
				TerrainCoord tileToStandOn = craftingPropOnTile.GetTileToStandOn(character);
				fail = tileToStandOn == TerrainCoord.Invalid;
				return tileToStandOn;
			}
			fail = true;
			break;
		}
		}
		return TerrainCoord.Invalid;
	}

	private bool HasUsedIngredientsWaitingForProduct(Character character)
	{
		if (FollowingRecipe != null && FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Invalid)
		{
			CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
			if (craftingPropOnTile != null && craftingPropOnTile.IsCrafting())
			{
				return craftingPropOnTile.CurrentCrafter == character;
			}
			return false;
		}
		return false;
	}

	public void StartRecipe(Character character, Recipe recipe, Equipment item, int amount, TerrainCoord tile, Prop.OrientationType orientation, bool wasTriggeredFromDirectControl, bool urgent)
	{
		FollowingRecipe = recipe;
		UsingItem = item;
		DesiredAmount = amount;
		WasTriggeredFromDirectControl = wasTriggeredFromDirectControl;
		HasSaidNeedContainer = false;
		if (wasTriggeredFromDirectControl)
		{
			HasSaidTooHeavy = false;
			HasSaidInsufficientResources = false;
		}
		DestTile = tile;
		DestOrientationType = orientation;
		MovementType = ((!urgent) ? MovementType.Walk : MovementType.Run);
	}

	public void StopRecipe(Character character)
	{
		FollowingRecipe = null;
		UsingItem = null;
		DesiredAmount = int.MaxValue;
		WasTriggeredFromDirectControl = false;
		int firstRunningRoleIndex = character.GetFirstRunningRoleIndex(Role.Crafter);
		if (firstRunningRoleIndex != -1)
		{
			StartRecipe(character, character.Roles[firstRunningRoleIndex].Recipe, null, int.MaxValue, character.Roles[firstRunningRoleIndex].TargetLocation, Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: false, character.Roles[firstRunningRoleIndex].Urgent);
		}
	}

	public void GiveUp(Character character)
	{
		if (DesiredAmount == int.MaxValue && FollowingRecipe != null)
		{
			character.CancelRole(new RoleInfo(Role.Crafter, FollowingRecipe, DestTile));
		}
		StopRecipe(character);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (SubGoal is MoveToAndTake moveToAndTake)
		{
			if (!(moveToAndTake.AmountRetrieved > 0f))
			{
				OnFailed(character);
				return null;
			}
			if (DesiredAmount == int.MaxValue && character.Community != null)
			{
				RoleInfo roleInfoBeingPerformed = GetRoleInfoBeingPerformed(character);
				GoalPriority highestPriority;
				int highestPriorityRoleIndex = GetHighestPriorityRoleIndex(character, out highestPriority);
				if (highestPriorityRoleIndex != -1 && !character.Roles[highestPriorityRoleIndex].Equals(roleInfoBeingPerformed))
				{
					return null;
				}
			}
		}
		if (SubGoal is AnimationGoal animationGoal)
		{
			switch (animationGoal.GetAnim())
			{
			case ActionAnim.CraftStart:
				if (FollowingRecipe == null)
				{
					if (!Crouching)
					{
						return new AnimationGoal(ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
					}
					break;
				}
				return new CraftAnim(ActionAnim.CraftLoop, FollowingRecipe, UsingItem, MovementType == MovementType.Run);
			case ActionAnim.CraftLoop:
				if (!Crouching)
				{
					return new AnimationGoal(ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				break;
			case ActionAnim.PotStart:
				if (FollowingRecipe == null || !HasUsedIngredientsWaitingForProduct(character))
				{
					return new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.PotEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new CraftAnim(character, animationGoal.GetTargetObject(), ActionAnim.PotLoop, FollowingRecipe, UsingItem, MovementType == MovementType.Run);
			case ActionAnim.PotLoop:
				return new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.PotEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			case ActionAnim.CraftTableStart:
				if (FollowingRecipe == null || !HasUsedIngredientsWaitingForProduct(character))
				{
					return new AnimationGoal(ActionAnim.CraftTableEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new CraftAnim(character, animationGoal.GetTargetObject(), ActionAnim.CraftTableLoop, FollowingRecipe, UsingItem, MovementType == MovementType.Run);
			case ActionAnim.CraftTableLoop:
				return new AnimationGoal(ActionAnim.CraftTableEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			case ActionAnim.ForgeStart:
				if (FollowingRecipe == null || !HasUsedIngredientsWaitingForProduct(character))
				{
					return new AnimationGoal(ActionAnim.ForgeEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
				}
				return new CraftAnim(character, animationGoal.GetTargetObject(), ActionAnim.ForgeLoop, FollowingRecipe, UsingItem, MovementType == MovementType.Run);
			case ActionAnim.ForgeLoop:
				return new AnimationGoal(ActionAnim.ForgeEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		if (SubGoal is LightFireGoal { Success: false })
		{
			OnFailed(character);
			return null;
		}
		if (SubGoal is MoveToAndAddMaterialToFire { Success: false })
		{
			LastFailedToReplenishFireTime = Session.Instance.PlayTime;
			OnFailed(character);
			return null;
		}
		if (FollowingRecipe == null)
		{
			return null;
		}
		if (SubGoal is SitAroundFireGoal)
		{
			if (HasUsedIngredientsWaitingForProduct(character))
			{
				LastCheckedBestRoleTime = Session.Instance.PlayTime;
				return new Idle();
			}
			return new Wait(TimeSpan.FromSeconds(2.0));
		}
		if (DesiredAmount == int.MaxValue && !HasUsedIngredientsWaitingForProduct(character) && character.Community != null && character.Community.HasReachedCraftingLimitForProduct(FollowingRecipe))
		{
			OnFailed(character);
			return null;
		}
		if (DesiredAmount == 0 && !HasUsedIngredientsWaitingForProduct(character))
		{
			if (!(parent is TreatGoal))
			{
				if (FollowingRecipe.RequiredPropToWorkOn() == BaseObjectType.Invalid)
				{
					if (!(parent is FindGoal) && !(parent is WarmGoal))
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.CraftingComplete);
						if (speechForSituation != null)
						{
							character.Speak(speechForSituation);
						}
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					}
				}
				else
				{
					CraftingProp craftingPropOnTile = instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
					if (craftingPropOnTile != null && craftingPropOnTile.Inventory.Count > 0 && GetRetrieveFinishedItemGoal(character, craftingPropOnTile, out var goal, out var _))
					{
						return goal;
					}
				}
			}
			if (FollowingRecipe != null)
			{
				StopRecipe(character);
			}
			return null;
		}
		if (UsingItem != null && !character.Inventory.Contains(UsingItem))
		{
			Ingredient ingredient = FollowingRecipe.GetIngredient(UsingItem);
			if (ingredient != null && !ingredient.Interchangeable)
			{
				StopRecipe(character);
				return null;
			}
			UsingItem = null;
		}
		if (SubGoal is MoveTo moveTo)
		{
			if (moveTo.Success)
			{
				return new TurnTo(DestTile);
			}
			bool fail;
			TerrainCoord tileToStandOn = GetTileToStandOn(character, out fail);
			if (tileToStandOn != TerrainCoord.Invalid && tileToStandOn != StandOnTile && !fail)
			{
				character.DirectControlledCrouching = false;
				StandOnTile = tileToStandOn;
				MoveTo moveTo2 = new MoveTo(MovementType, StandOnTile);
				moveTo2.MoveToCentreOfTile = FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Invalid;
				return moveTo2;
			}
			StandOnTile = tileToStandOn;
			OnFailed(character);
			return null;
		}
		if (SubGoal is FindGoal { Result: not FindResult.Success } findGoal)
		{
			if (character.IsControllableByPlayer())
			{
				Role roleBeingPerformed = GetRoleBeingPerformed(character);
				SpeechSituation speechSituation = SpeechSituation.None;
				MemoryParam param = default(MemoryParam);
				if (findGoal.FindType == FindType.SealedContainer)
				{
					if (!HasSaidNeedContainer)
					{
						speechSituation = (findGoal.Liquid.CanPourIntoBottles ? SpeechSituation.NeedBottle : SpeechSituation.NeedSealedContainer);
						param = new MemoryParam(findGoal.Liquid);
						HasSaidNeedContainer = true;
					}
				}
				else if (findGoal.Result == FindResult.NeedContainer)
				{
					speechSituation = SpeechSituation.NeedSealedContainer;
					if (findGoal.FindType == FindType.CraftingResources)
					{
						foreach (Ingredient ingredient2 in FollowingRecipe.Ingredients)
						{
							if (!ingredient2.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, character) && ingredient2.LiquidTypes != null && ingredient2.LiquidTypes.Count > 0)
							{
								param = new MemoryParam(ingredient2.LiquidTypes[0]);
								break;
							}
						}
					}
				}
				else if (findGoal.Result == FindResult.TooHeavy)
				{
					if (!HasSaidTooHeavy || roleBeingPerformed != Role.Cook)
					{
						speechSituation = SpeechSituation.GatherTooHeavy;
						param = ((findGoal.FoundItem != null) ? new MemoryParam(findGoal.FoundItem) : new MemoryParam(findGoal.FoundResource));
						if (roleBeingPerformed == Role.Cook)
						{
							HasSaidTooHeavy = true;
						}
					}
				}
				else if (roleBeingPerformed == Role.Crafter && findGoal.Result != FindResult.NotAccessible && !HasSaidInsufficientResources)
				{
					foreach (Ingredient ingredient3 in FollowingRecipe.Ingredients)
					{
						if (!ingredient3.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, character))
						{
							if (ingredient3.Prototypes != null && ingredient3.Prototypes.Count > 0)
							{
								param = new MemoryParam(ingredient3.Prototypes[0]);
								break;
							}
							if (ingredient3.LiquidTypes != null && ingredient3.LiquidTypes.Count > 0)
							{
								param = new MemoryParam(ingredient3.LiquidTypes[0]);
								break;
							}
						}
					}
					speechSituation = (FollowingRecipe.IsProductEdible(includeSugar: false, includeHumanMeat: true) ? SpeechSituation.InsufficientCookingResources : SpeechSituation.InsufficientCraftingResources);
					HasSaidInsufficientResources = true;
				}
				if (speechSituation != SpeechSituation.None)
				{
					Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, null, speechSituation, param);
					if (speechForSituation2 != null)
					{
						character.Speak(speechForSituation2, null, null, param);
					}
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				}
				if (DesiredAmount < int.MaxValue)
				{
					GiveUp(character);
				}
				else if (roleBeingPerformed == Role.Cook)
				{
					StopRecipe(character);
				}
				else if (findGoal.Result == FindResult.TooHeavy && character.IsControllableByPlayer())
				{
					character.PauseRole(new RoleInfo(Role.Crafter, FollowingRecipe, DestTile));
				}
			}
			else if (GetRoleBeingPerformed(character) == Role.Cook)
			{
				StopRecipe(character);
			}
			OnFailed(character);
			return null;
		}
		if (!HasUsedIngredientsWaitingForProduct(character) && FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Invalid)
		{
			CraftingProp craftingPropOnTile2 = instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
			if (craftingPropOnTile2 != null && (craftingPropOnTile2.CraftingRecipe == null || craftingPropOnTile2.CraftingFinished) && craftingPropOnTile2.Inventory.Count > 0)
			{
				if (DesiredAmount == int.MaxValue && GetRoleBeingPerformed(character) == Role.Cook && craftingPropOnTile2.Inventory.GetTotalNutrition() > 0f)
				{
					OnFailed(character);
					return null;
				}
				if (GetRetrieveFinishedItemGoal(character, craftingPropOnTile2, out var goal2, out var hasSpace2))
				{
					return goal2;
				}
				if (!hasSpace2)
				{
					OnFailed(character);
					return null;
				}
			}
		}
		if (DesiredAmount == int.MaxValue)
		{
			float maxWeightOfIngredientsWeHaventGotYet = FollowingRecipe.GetMaxWeightOfIngredientsWeHaventGotYet(character, UsingItem);
			bool failed;
			Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, maxWeightOfIngredientsWeHaventGotYet, maxWeightOfIngredientsWeHaventGotYet, MovementType, out failed);
			if (moveToAndDepositGoal != null)
			{
				return moveToAndDepositGoal;
			}
		}
		bool flag = false;
		if (FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Invalid)
		{
			CraftingProp craftingPropOnTile3 = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
			if (craftingPropOnTile3 == null)
			{
				GiveUp(character);
				return null;
			}
			if (HasUsedIngredientsWaitingForProduct(character))
			{
				flag = true;
			}
			else if (craftingPropOnTile3.IsCrafting() && craftingPropOnTile3.CurrentCrafter != character)
			{
				if (craftingPropOnTile3.CraftingRecipe != FollowingRecipe || (craftingPropOnTile3.CurrentCrafter != null && craftingPropOnTile3.CurrentCrafter.FindActiveGoal(GoalType.CraftGoal) != null))
				{
					OnFailed(character);
					return null;
				}
				craftingPropOnTile3.SetCrafter(character);
				flag = true;
			}
			else
			{
				while (true)
				{
					Equipment bestCookingPot;
					Equipment bestLiquidContainerToFill;
					switch (GameCursor.CanStartCraftingWithProp(character, craftingPropOnTile3, FollowingRecipe, UsingItem, ingredientsMustBeOnMe: true, WantCheckEquipmentPolicy(character), checkIfCrafting: true))
					{
					case CursorActionDisabledReason.Disabled:
					case CursorActionDisabledReason.CampfireInUse:
						OnFailed(character);
						return null;
					case CursorActionDisabledReason.NeedEmptyPot:
						bestCookingPot = character.Inventory.GetBestCookingPot();
						if (bestCookingPot != null && bestCookingPot.GetLiquidContentsAmount() > 0f)
						{
							bestLiquidContainerToFill = character.Inventory.GetBestLiquidContainerToFill(bestCookingPot.GetLiquidContentsType(), bestCookingPot, bestCookingPot.GetLiquidContentsAmount());
							if (bestLiquidContainerToFill != null)
							{
								goto IL_0a8b;
							}
							if (!((double)bestCookingPot.GetLiquidContentsType().BasePricePerFlOz < 0.05))
							{
								return new FindGoal(FindType.SealedContainer, bestCookingPot.GetLiquidContentsType(), bestCookingPot.GetLiquidContentsAmount(), MovementType, critical: false)
								{
									MaxDistToTravel = MaxDistToTravel
								};
							}
							flag = true;
						}
						else
						{
							flag = true;
						}
						break;
					case CursorActionDisabledReason.DontHaveSuitableContainer:
					case CursorActionDisabledReason.DontEnoughSuitableContainers:
					case CursorActionDisabledReason.DontHaveSealedContainer:
					case CursorActionDisabledReason.DontEnoughSealedContainers:
						return new FindGoal(FindType.SealedContainer, FollowingRecipe.ProductLiquidPrototype, FollowingRecipe.ProductLiquidAmount, MovementType, critical: false)
						{
							MaxDistToTravel = MaxDistToTravel
						};
					case CursorActionDisabledReason.Enabled:
						flag = true;
						break;
					}
					break;
					IL_0a8b:
					bestLiquidContainerToFill.FillLiquid(bestCookingPot.GetLiquidContentsType(), bestCookingPot.DrainLiquid(bestLiquidContainerToFill.GetLiquidCapacity() - bestLiquidContainerToFill.GetLiquidContentsAmount()), bestCookingPot.InfectedWith);
				}
			}
		}
		else
		{
			flag = FollowingRecipe.HasAllIngredients(character, character, UsingItem, WantCheckEquipmentPolicy(character) ? character : null);
			if (flag && !FollowingRecipe.HasSuitableContainer(character, UsingItem))
			{
				return new FindGoal(FindType.SealedContainer, FollowingRecipe.ProductLiquidPrototype, FollowingRecipe.ProductLiquidAmount, MovementType, critical: false)
				{
					MaxDistToTravel = MaxDistToTravel
				};
			}
		}
		if (flag)
		{
			RecipeType recipeType = FollowingRecipe.RecipeType;
			if ((uint)(recipeType - 4) <= 5u && instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire campfire && !campfire.IsBurning())
			{
				character.DirectControlledCrouching = false;
				return new LightFireGoal(character, null, campfire, MovementType, canAddMaterialToFire: true);
			}
			if (StandOnTile != TerrainCoord.Invalid && character.Tile != StandOnTile && (!character.IsSitting() || !character.Tile.IsAdjacent(StandOnTile) || !HasUsedIngredientsWaitingForProduct(character)))
			{
				MoveTo moveTo2 = new MoveTo(MovementType, StandOnTile);
				moveTo2.MoveToCentreOfTile = FollowingRecipe.RequiredPropToWorkOn() != BaseObjectType.Invalid;
				return moveTo2;
			}
			if (character.EquippedItem != null)
			{
				return new Equip(null);
			}
			switch (FollowingRecipe.RecipeType)
			{
			case RecipeType.Campfire_SpitRoast_Rabbit:
			case RecipeType.Campfire_SpitRoast_Chicken:
			case RecipeType.Campfire_SpitRoast_Venison:
			case RecipeType.Campfire_SpitRoast_SomeKindOfMeat:
				if (instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire campfire2)
				{
					if (!HasUsedIngredientsWaitingForProduct(character) && campfire2.CraftingRecipe == null)
					{
						if (character.IsFacing(campfire2.PosXZ, 0.001f))
						{
							return new AnimationGoal(ActionAnim.SpitRoast);
						}
						return new TurnTo(DestTile);
					}
					return new SitAroundFireGoal(character, campfire2, MovementType)
					{
						MinRange = 0f
					};
				}
				GiveUp(character);
				return null;
			case RecipeType.Campfire_Pot:
			case RecipeType.Campfire_FryingPan:
				if (instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire campfire3)
				{
					if (HasUsedIngredientsWaitingForProduct(character) || campfire3.CraftingRecipe == null || (campfire3.CraftingFinished && campfire3.Inventory.Count == 1 && campfire3.Inventory.GetItem(0).GetPrototype() == ((FollowingRecipe.RecipeType == RecipeType.Campfire_FryingPan) ? EquipmentPrototype.FryingPan : EquipmentPrototype.Pot) && campfire3.Inventory.GetItem(0).GetLiquidContentsAmount() == 0f))
					{
						if (character.IsFacing(campfire3.PosXZ, 0.001f))
						{
							character.IssueCommandToFollowers(FollowCommand.SitAroundFire, campfire3.Tile, null);
							return new CraftWithPropStartAnim(character, campfire3, ActionAnim.PotStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
						}
						return new TurnTo(DestTile);
					}
					return new SitAroundFireGoal(character, campfire3, MovementType)
					{
						MinRange = 0f
					};
				}
				GiveUp(character);
				return null;
			case RecipeType.Forge:
			case RecipeType.Workbench:
			{
				Prop craftingPropOnTile5 = instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
				if (craftingPropOnTile5 != null && FollowingRecipe.IsCraftingPropForRecipe(craftingPropOnTile5))
				{
					TerrainCoord nearestTileTo2 = craftingPropOnTile5.GetNearestTileTo(StandOnTile);
					if (character.IsFacing(instance.GetTileCentreXZ(nearestTileTo2), 0.001f))
					{
						character.DirectControlledCrouching = false;
						return new AnimationGoal(character, craftingPropOnTile5, (craftingPropOnTile5 is Forge) ? ActionAnim.ForgeStart : ActionAnim.CraftTableStart, enableFaceTarget: false, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
					}
					return new TurnTo(nearestTileTo2);
				}
				GiveUp(character);
				return null;
			}
			case RecipeType.Still:
			case RecipeType.Kiln:
			case RecipeType.Nitrary:
			{
				Prop craftingPropOnTile4 = instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
				if (craftingPropOnTile4 != null && FollowingRecipe.IsCraftingPropForRecipe(craftingPropOnTile4))
				{
					if (!HasUsedIngredientsWaitingForProduct(character))
					{
						TerrainCoord nearestTileTo = craftingPropOnTile4.GetNearestTileTo(StandOnTile);
						if (character.IsFacing(instance.GetTileCentreXZ(nearestTileTo), 0.001f))
						{
							character.DirectControlledCrouching = false;
							return new AnimationGoal((craftingPropOnTile4 is Kiln) ? ActionAnim.ScavengeCorpse : ActionAnim.Scavenge, enableFaceTarget: false);
						}
						return new TurnTo(nearestTileTo);
					}
					character.OnRoleSucceeded();
					if (DesiredAmount == int.MaxValue && character.Community != null)
					{
						RoleInfo roleInfoBeingPerformed2 = GetRoleInfoBeingPerformed(character);
						GoalPriority highestPriority2;
						int highestPriorityRoleIndex2 = GetHighestPriorityRoleIndex(character, out highestPriority2);
						if (highestPriorityRoleIndex2 != -1 && !character.Roles[highestPriorityRoleIndex2].Equals(roleInfoBeingPerformed2))
						{
							return null;
						}
					}
					return new SitAroundFireGoal(character, craftingPropOnTile4, MovementType);
				}
				GiveUp(character);
				return null;
			}
			default:
				if (Crouching)
				{
					return new CraftAnim(ActionAnim.CraftLoop, FollowingRecipe, UsingItem, MovementType == MovementType.Run);
				}
				return new AnimationGoal(ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		if (DesiredAmount == int.MaxValue || (FollowingRecipe.ProductPropPrototype != null && !character.IsControllableByPlayer()) || !(parent is PrioritiserGoal))
		{
			switch (FollowingRecipe.RecipeType)
			{
			case RecipeType.Toolbox:
				if (character.Inventory.GetToolbox() == null)
				{
					return new FindGoal(FindType.Toolbox, MovementType, critical: false)
					{
						MaxDistToTravel = MaxDistToTravel
					};
				}
				break;
			case RecipeType.Shovel:
				if (character.Inventory.GetShovel() == null)
				{
					return new FindGoal(FindType.Shovel, MovementType, critical: false)
					{
						MaxDistToTravel = MaxDistToTravel
					};
				}
				break;
			case RecipeType.HuntingKnife:
				if (character.Inventory.GetHuntingKnife() == null)
				{
					return new FindGoal(FindType.HuntingKnife, MovementType, critical: false)
					{
						MaxDistToTravel = MaxDistToTravel
					};
				}
				break;
			case RecipeType.Campfire_Pot:
			case RecipeType.Campfire_FryingPan:
				if (instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire prop)
				{
					CursorActionDisabledReason cursorActionDisabledReason = GameCursor.CanStartCraftingWithProp(character, prop, FollowingRecipe, UsingItem, ingredientsMustBeOnMe: true, WantCheckEquipmentPolicy(character), checkIfCrafting: true);
					if (cursorActionDisabledReason == CursorActionDisabledReason.NeedPot || cursorActionDisabledReason == CursorActionDisabledReason.NeedFryingPan)
					{
						return new FindGoal(FindType.Prototype, (cursorActionDisabledReason == CursorActionDisabledReason.NeedFryingPan) ? EquipmentPrototype.FryingPan : EquipmentPrototype.Pot, MovementType, critical: false)
						{
							MaxDistToTravel = MaxDistToTravel
						};
					}
				}
				break;
			}
			return new FindGoal(FindType.CraftingResources, FollowingRecipe, UsingItem, DesiredAmount, MovementType, critical: false)
			{
				CanTravelFar = character.IsControllableByPlayer(),
				MaxDistToTravel = MaxDistToTravel,
				CheckEquipmentPolicyForCraftingResources = WantCheckEquipmentPolicy(character)
			};
		}
		MemoryParam param2 = default(MemoryParam);
		foreach (Ingredient ingredient4 in FollowingRecipe.Ingredients)
		{
			if (!ingredient4.HasEnoughOfIngredient(character, character, FollowingRecipe, UsingItem, character))
			{
				if (ingredient4.Prototypes != null && ingredient4.Prototypes.Count > 0)
				{
					param2 = new MemoryParam(ingredient4.Prototypes[0]);
					break;
				}
				if (ingredient4.LiquidTypes != null && ingredient4.LiquidTypes.Count > 0)
				{
					param2 = new MemoryParam(ingredient4.LiquidTypes[0]);
					break;
				}
			}
		}
		Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, null, FollowingRecipe.IsProductDrinkableOrEdible() ? SpeechSituation.InsufficientCookingResources : SpeechSituation.InsufficientCraftingResources, param2);
		if (speechForSituation3 != null)
		{
			character.Speak(speechForSituation3, null, null, param2);
		}
		character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
		StopRecipe(character);
		return null;
	}

	private bool GetRetrieveFinishedItemGoal(Character character, CraftingProp craftingProp, out Goal goal, out bool hasSpace)
	{
		Equipment equipment = ((craftingProp is Campfire && FollowingRecipe.RecipeType == RecipeType.Campfire_Pot) ? craftingProp.Inventory.FindItemOfType(EquipmentPrototype.Pot) : ((craftingProp is Campfire && FollowingRecipe.RecipeType == RecipeType.Campfire_FryingPan) ? craftingProp.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) : null));
		float weightIgnoringItem = craftingProp.Inventory.GetWeightIgnoringItem(craftingProp, equipment);
		hasSpace = weightIgnoringItem == 0f || character.HasInventorySpaceFor(weightIgnoringItem);
		if (!hasSpace)
		{
			goal = GatherGoal.GetMoveToAndDepositGoal(character, weightIgnoringItem, weightIgnoringItem, MovementType, out var _);
			if (goal != null)
			{
				return true;
			}
		}
		if (hasSpace || !character.IsControllableByPlayer())
		{
			character.DirectControlledCrouching = false;
			for (int i = 0; i < craftingProp.Inventory.Count; i++)
			{
				Equipment item = craftingProp.Inventory.GetItem(i);
				if (item.GetPrototype() == EquipmentPrototype.Pot && craftingProp is Campfire)
				{
					if (item.GetLiquidContentsAmount() > 0f)
					{
						Equipment bestLiquidContainerToFill = character.Inventory.GetBestLiquidContainerToFill(item.GetLiquidContentsType(), item.GetLiquidContentsAmount());
						if (bestLiquidContainerToFill == null)
						{
							FindGoal findGoal = new FindGoal(FindType.SealedContainer, item.GetLiquidContentsType(), item.GetLiquidContentsAmount(), MovementType, critical: false);
							findGoal.MaxDistToTravel = MaxDistToTravel;
							goal = findGoal;
							return true;
						}
						HasSaidNeedContainer = false;
						MoveToAndTake moveToAndTake = new MoveToAndTake(character, craftingProp, item, item.GetLiquidContentsAmount(), !character.IsControllableByPlayer(), MovementType, bestLiquidContainerToFill);
						moveToAndTake.IsGathering = DesiredAmount == int.MaxValue;
						goal = moveToAndTake;
						return true;
					}
					if (FollowingRecipe.RecipeType != RecipeType.Campfire_Pot)
					{
						MoveToAndTake moveToAndTake2 = new MoveToAndTake(character, craftingProp, item, item.GetAmount(), !character.IsControllableByPlayer(), MovementType);
						moveToAndTake2.IsGathering = DesiredAmount == int.MaxValue;
						goal = moveToAndTake2;
						return true;
					}
				}
				else
				{
					if (item.GetPrototype() != EquipmentPrototype.FryingPan || !(craftingProp is Campfire))
					{
						MoveToAndTake moveToAndTake3 = new MoveToAndTake(character, craftingProp, item, item.GetAmount(), !character.IsControllableByPlayer(), MovementType);
						moveToAndTake3.IsGathering = DesiredAmount == int.MaxValue;
						goal = moveToAndTake3;
						return true;
					}
					if (FollowingRecipe.RecipeType != RecipeType.Campfire_FryingPan)
					{
						MoveToAndTake moveToAndTake4 = new MoveToAndTake(character, craftingProp, item, item.GetAmount(), !character.IsControllableByPlayer(), MovementType);
						moveToAndTake4.IsGathering = DesiredAmount == int.MaxValue;
						goal = moveToAndTake4;
						return true;
					}
				}
			}
		}
		else if (GetRoleBeingPerformed(character) != Role.Cook)
		{
			Equipment obj = null;
			foreach (Equipment content in craftingProp.Inventory.Contents)
			{
				if (content != equipment)
				{
					obj = content;
				}
			}
			MemoryParam param = new MemoryParam(obj);
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, null, null, param);
			}
			character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
			if (DesiredAmount == int.MaxValue)
			{
				character.PauseRole(new RoleInfo(Role.Crafter, FollowingRecipe, DestTile));
			}
			else
			{
				GiveUp(character);
			}
			goal = null;
			return true;
		}
		goal = null;
		return false;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		AnimationEventType eventType = animEvent.EventType;
		if ((eventType == AnimationEventType.Take || eventType == AnimationEventType.StartCrafting) && (animEvent.EventType != AnimationEventType.Take || SubGoal is AnimationGoal))
		{
			if (FollowingRecipe != null)
			{
				if (UsingItem != null)
				{
					if (character.IsWearing(UsingItem))
					{
						UsingItem.Strip(character);
					}
					if (character.EquippedItem == UsingItem)
					{
						character.EquippedItem = (character.DesiredEquippedItem = null);
					}
				}
				RecipeType recipeType = FollowingRecipe.RecipeType;
				if ((uint)(recipeType - 4) <= 10u)
				{
					bool hasSomeButNotEnough = false;
					float productAmount = 0f;
					float freeCapacity = 0f;
					if (FollowingRecipe.RecipeType == RecipeType.Campfire_Pot || FollowingRecipe.HasSuitableContainer(character, UsingItem, out hasSomeButNotEnough, out productAmount, out freeCapacity))
					{
						CraftingProp craftingPropOnTile = GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y);
						if (craftingPropOnTile != null && GameCursor.CanStartCraftingWithProp(character, craftingPropOnTile, FollowingRecipe, UsingItem, ingredientsMustBeOnMe: true, WantCheckEquipmentPolicy(character), checkIfCrafting: true) == CursorActionDisabledReason.Enabled)
						{
							float ingredientsNutrition = 0f;
							InfectionType ingredientsInfectedWith = InfectionType.None;
							FollowingRecipe.UseIngredients(character, UsingItem, craftingPropOnTile.UsedIngredients, out ingredientsNutrition, out ingredientsInfectedWith, out UsedHumanIngredients, WantCheckEquipmentPolicy(character));
							craftingPropOnTile.SetCraftingRecipe(FollowingRecipe, character, ingredientsNutrition, ingredientsInfectedWith, DesiredAmount);
							if (FollowingRecipe.RecipeType == RecipeType.Campfire_Pot && craftingPropOnTile.Inventory.FindItemOfType(EquipmentPrototype.Pot) == null)
							{
								Equipment bestCookingPot = character.Inventory.GetBestCookingPot();
								bestCookingPot.DrainLiquid(bestCookingPot.GetLiquidContentsAmount());
								bestCookingPot = character.Inventory.Take(character, bestCookingPot, 1);
								craftingPropOnTile.Inventory.Add(craftingPropOnTile, bestCookingPot);
							}
							if (FollowingRecipe.RecipeType == RecipeType.Campfire_FryingPan && craftingPropOnTile.Inventory.FindItemOfType(EquipmentPrototype.FryingPan) == null)
							{
								Equipment fryingPan = character.Inventory.GetFryingPan();
								fryingPan = character.Inventory.Take(character, fryingPan, 1);
								craftingPropOnTile.Inventory.Add(craftingPropOnTile, fryingPan);
							}
							if (FollowingRecipe.ProductLiquidPrototype != null)
							{
								while (productAmount > 0.001f)
								{
									Equipment bestLiquidContainerToFill = character.Inventory.GetBestLiquidContainerToFill(FollowingRecipe.ProductLiquidPrototype, FollowingRecipe.ProductLiquidAmount);
									if (bestLiquidContainerToFill == null)
									{
										break;
									}
									productAmount -= bestLiquidContainerToFill.GetLiquidCapacity() - bestLiquidContainerToFill.GetLiquidContentsAmount();
									bestLiquidContainerToFill = character.Inventory.Take(character, bestLiquidContainerToFill, 1);
									craftingPropOnTile.Inventory.Add(craftingPropOnTile, bestLiquidContainerToFill);
								}
							}
						}
					}
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public Role GetRoleBeingPerformed(Character character)
	{
		if (DesiredAmount == int.MaxValue)
		{
			for (int i = 0; i < character.Roles.Count; i++)
			{
				if (character.Roles[i].Equals(new RoleInfo(Role.Crafter, DestTile, null, FollowingRecipe)))
				{
					return Role.Crafter;
				}
				if (character.Roles[i].Role == Role.Cook && (FollowingRecipe == null || FollowingRecipe.IsValidRecipeForCook()))
				{
					return Role.Cook;
				}
			}
		}
		return Role.None;
	}

	public override bool OnCraftingFinished(Character character, Goal parent, TileObject obj)
	{
		if (obj.GetCentreTile() == DestTile)
		{
			SubtractDesiredAmount();
			if (Active && FollowingRecipe != null && DesiredAmount != int.MaxValue)
			{
				SpeechSituation sit = (FollowingRecipe.IsProductEdible(includeSugar: true, includeHumanMeat: true) ? SpeechSituation.CookingComplete : (FollowingRecipe.IsProductAlcoholic() ? SpeechSituation.BoozeComplete : SpeechSituation.CraftingComplete));
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, sit);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation);
				}
			}
			RoleInfo roleInfoBeingPerformed = GetRoleInfoBeingPerformed(character);
			if (roleInfoBeingPerformed.Role == Role.Cook)
			{
				StopRecipe(character);
			}
			if (roleInfoBeingPerformed.Role != Role.None)
			{
				if (roleInfoBeingPerformed.Role == Role.Cook || FollowingRecipe == null || character.Community.HasReachedCraftingLimitForProduct(FollowingRecipe))
				{
					character.SetRoleInProgress(roleInfoBeingPerformed, inProgress: false);
				}
				character.OnRoleSucceeded();
			}
			if (Active)
			{
				AnimationGoal animationGoal = SubGoal as AnimationGoal;
				if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.PotLoop)
				{
					SetSubGoal(character, parent, new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.PotEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f));
				}
				else if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.CraftLoop)
				{
					SetSubGoal(character, parent, new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f));
				}
				else if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.CraftTableLoop)
				{
					SetSubGoal(character, parent, new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.CraftTableEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f));
				}
				else if (animationGoal != null && animationGoal.GetAnim() == ActionAnim.ForgeLoop)
				{
					SetSubGoal(character, parent, new AnimationGoal(character, animationGoal.GetTargetObject(), ActionAnim.ForgeEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f));
				}
				else if (SubGoal is Idle)
				{
					SetSubGoal(character, parent, new Wait(TimeSpan.FromSeconds(2.0)));
				}
			}
			return true;
		}
		return false;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is Idle)
		{
			if (Session.Instance.PlayTime - LastCheckedBestRoleTime >= TimeSpan.FromSeconds(10.0))
			{
				character.OnRoleSucceeded();
				LastCheckedBestRoleTime = Session.Instance.PlayTime;
				if (DesiredAmount == int.MaxValue && character.Community != null)
				{
					RoleInfo roleInfoBeingPerformed = GetRoleInfoBeingPerformed(character);
					GoalPriority highestPriority;
					int highestPriorityRoleIndex = GetHighestPriorityRoleIndex(character, out highestPriority);
					if (highestPriorityRoleIndex != -1 && !character.Roles[highestPriorityRoleIndex].Equals(roleInfoBeingPerformed))
					{
						Finished = true;
					}
				}
			}
			if (!HasUsedIngredientsWaitingForProduct(character))
			{
				Finished = true;
			}
			if (GameTerrain.Instance.GetCraftingPropOnTile(DestTile.x, DestTile.y) is Campfire campfire)
			{
				if (campfire != null && !campfire.IsBurning() && character.Community != null && !character.Community.IsAnyMemberLightingFire(campfire))
				{
					SetSubGoal(character, parent, new LightFireGoal(character, null, campfire, MovementType, canAddMaterialToFire: true));
				}
				else if (campfire != null && campfire.IsBurning() && campfire.WoodRemaining < WarmGoal.ReplenishWoodAmount && Session.Instance.PlayTime - LastFailedToReplenishFireTime > TimeSpan.FromSeconds(120.0) && character.Community != null && !character.Community.IsAnyMemberLightingFire(campfire))
				{
					SetSubGoal(character, parent, new MoveToAndAddMaterialToFire(character, campfire.Tile, null, MovementType, critical: false));
				}
			}
		}
		if (FollowingRecipe != null && !character.CanUseRecipe(FollowingRecipe))
		{
			Finished = true;
		}
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return GetRoleBeingPerformed(character) switch
		{
			Role.Cook => new RoleInfo(Role.Cook), 
			Role.Crafter => new RoleInfo(Role.Crafter, FollowingRecipe, DestTile), 
			_ => default(RoleInfo), 
		};
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		base.SetMovementType(character, movementType);
		if (SubGoal is CraftAnim craftAnim)
		{
			craftAnim.Urgent = movementType == MovementType.Run;
		}
	}
}
