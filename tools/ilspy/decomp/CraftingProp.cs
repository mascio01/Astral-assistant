using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class CraftingProp : Prop
{
	public Recipe CraftingRecipe;

	public Character CurrentCrafter;

	public bool CraftingFinished;

	public float CraftingTimeSpent;

	public float CraftingIngredientsNutrition;

	public InfectionType CraftingIngredientsInfectedWith;

	public int CraftingDesiredAmount;

	public List<UsedIngredient> UsedIngredients = new List<UsedIngredient>();

	public override Color32 MapColor => GameTerrain.MinimapSettings.PropCol;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref CraftingRecipe, 68);
		reflector.AddAfter(ref CurrentCrafter, 68);
		reflector.AddAfter(ref CraftingFinished, 68);
		reflector.AddAfter(ref CraftingTimeSpent, 68);
		reflector.AddAfter(ref CraftingIngredientsNutrition, 68);
		reflector.AddAfter(ref CraftingIngredientsInfectedWith, 153);
		reflector.AddAfter(ref CraftingDesiredAmount, 238);
		reflector.AddAfter(ref UsedIngredients, 594);
	}

	public bool IsCrafting()
	{
		if (CraftingRecipe != null)
		{
			return !CraftingFinished;
		}
		return false;
	}

	public float GetCraftingProgress()
	{
		if (CraftingRecipe == null)
		{
			return 0f;
		}
		return Mathf.Clamp01(CraftingTimeSpent / CraftingRecipe.CraftingTime);
	}

	public void Craft(float time)
	{
		CraftingTimeSpent += time;
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override float GetMaxInventoryWeight()
	{
		if (CraftingRecipe == null)
		{
			return 0f;
		}
		return base.GetMaxInventoryWeight();
	}

	public override bool CanSetPropName()
	{
		return true;
	}

	public override bool CanSetStoragePolicy()
	{
		return false;
	}

	public override void Init()
	{
		base.Init();
		if (IsCrafting())
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		}
	}

	public virtual void SetCraftingRecipe(Recipe recipe, Character crafter, float ingredientsNutrition, InfectionType ingredientsInfectedWith, int craftingDesiredAmount)
	{
		CraftingRecipe = recipe;
		CraftingTimeSpent = 0f;
		CraftingFinished = false;
		CurrentCrafter = crafter;
		CraftingIngredientsNutrition = ingredientsNutrition;
		CraftingIngredientsInfectedWith = ingredientsInfectedWith;
		CraftingDesiredAmount = craftingDesiredAmount;
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this);
		NotificationManager.Instance.CheckCommunityStats = true;
		if (crafter == null || crafter.GetPlayerControllingMe() == null)
		{
			NotificationManager.Instance.SuppressNextAggroNotification = true;
		}
	}

	public virtual void SetCrafter(Character crafter)
	{
		CurrentCrafter = crafter;
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (IsCrafting())
		{
			if (CraftingTimeSpent >= CraftingRecipe.CraftingTime)
			{
				CraftingTimeSpent = CraftingRecipe.CraftingTime;
				CraftingFinished = true;
				OnCraftingFinished();
			}
			else
			{
				stillNeedUpdating = true;
			}
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}

	public virtual void OnCraftingFinished()
	{
		if (CraftingDesiredAmount != int.MaxValue)
		{
			CraftingDesiredAmount--;
		}
		float numProduced = 0f;
		CraftingRecipe.CreateProduct(this, (CurrentCrafter != null) ? CurrentCrafter.Community : Community, null, Tile, Orientation, CraftingIngredientsNutrition, CraftingIngredientsInfectedWith, out numProduced, addToGatheredItems: false);
		if (CurrentCrafter != null)
		{
			CurrentCrafter.OnCraftingFinished(this);
		}
	}

	public override CantTransferReason CanTransferEquipmentAway(Equipment item, bool onTradePage)
	{
		if (IsCrafting() && CraftingRecipe.ProductLiquidPrototype != null && item.GetLiquidCapacity() > 0f)
		{
			return CantTransferReason.LiquidProductReceptacle;
		}
		return base.CanTransferEquipmentAway(item, onTradePage);
	}

	public virtual TerrainCoord GetTileToStandOn(Character character)
	{
		TerrainCoord bestTile = TerrainCoord.Invalid;
		float bestDistSq = float.MaxValue;
		TerrainCoord lhs = Prop.GetDirFromOrientationType((OrientationType)((int)(Orientation + 1) % 4));
		TerrainCoord rhs = Prop.GetDirFromOrientationType(Orientation);
		TerrainCoord centreTile = GetCentreTile();
		if (GetBaseObjectType() == BaseObjectType.Forge)
		{
			centreTile += rhs;
		}
		if (GetBaseObjectType() == BaseObjectType.Nitrary)
		{
			MathUtil.Swap(ref lhs, ref rhs);
			centreTile += lhs;
			lhs *= 2;
		}
		TerrainRect.TestTile(centreTile, GetCentreTile() + lhs, 2049, character, null, ref bestTile, ref bestDistSq);
		TerrainRect.TestTile(centreTile, GetCentreTile() + lhs + rhs, 2049, character, null, ref bestTile, ref bestDistSq);
		TerrainRect.TestTile(centreTile, GetCentreTile() + lhs - rhs, 2049, character, null, ref bestTile, ref bestDistSq);
		return bestTile;
	}

	public virtual TerrainRect GetStandingArea()
	{
		TerrainCoord lhs = Prop.GetDirFromOrientationType((OrientationType)((int)(Orientation + 1) % 4));
		TerrainCoord rhs = Prop.GetDirFromOrientationType(Orientation);
		TerrainCoord centreTile = GetCentreTile();
		if (GetBaseObjectType() == BaseObjectType.Forge)
		{
			centreTile += rhs;
		}
		if (GetBaseObjectType() == BaseObjectType.Nitrary)
		{
			MathUtil.Swap(ref lhs, ref rhs);
			centreTile += lhs;
			lhs *= 2;
		}
		TerrainCoord terrainCoord = GetCentreTile() + lhs;
		return new TerrainRect(terrainCoord, terrainCoord).Include(terrainCoord + rhs).Include(terrainCoord - rhs);
	}
}
