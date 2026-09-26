using System;
using System.Collections.Generic;
using UnityEngine;

public class UnderConstructionInfo
{
	public Recipe Recipe;

	public List<UsedIngredient> IngredientsUsed = new List<UsedIngredient>();

	public float TimeTakenOnResource;

	public static float UnderConstructionHeight = 2f;

	public UnderConstructionInfo()
	{
	}

	public UnderConstructionInfo(Recipe recipe)
	{
		Recipe = recipe;
	}

	public void Reflect(Reflector reflector, TileObject owner)
	{
		if (reflector.Version < 382)
		{
			Character obj = null;
			reflector.Add(ref obj);
		}
		reflector.Add(ref Recipe);
		reflector.Add(ref IngredientsUsed);
		reflector.AddAfter(ref TimeTakenOnResource, 581);
	}

	public bool HasUsedEnoughOfIngredient(Ingredient ingredient)
	{
		float amountNeeded;
		return HasUsedEnoughOfIngredient(ingredient, out amountNeeded);
	}

	public bool HasUsedEnoughOfIngredient(Ingredient ingredient, out float amountNeeded)
	{
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < IngredientsUsed.Count; i++)
		{
			if (IngredientsUsed[i].Prototype != null && ingredient.Prototypes != null && ingredient.Prototypes.Contains(IngredientsUsed[i].Prototype))
			{
				num += IngredientsUsed[i].Amount;
			}
			if (IngredientsUsed[i].LiquidPrototype != null && ingredient.LiquidTypes != null && ingredient.LiquidTypes.Contains(IngredientsUsed[i].LiquidPrototype))
			{
				num2 += IngredientsUsed[i].LiquidAmount;
			}
		}
		amountNeeded = (float)(ingredient.Amount - num) + (ingredient.LiquidAmount - num2);
		if (num >= ingredient.Amount)
		{
			return num2 >= ingredient.LiquidAmount;
		}
		return false;
	}

	public void GetNeededIngredientsWeight(out float minRequiredWeight, out float desiredWeight, float carryCapacity)
	{
		minRequiredWeight = 0f;
		desiredWeight = 0f;
		foreach (Ingredient ingredient in Recipe.Ingredients)
		{
			float amountUsedOfIngredient = GetAmountUsedOfIngredient(ingredient);
			if (ingredient.IsLiquid())
			{
				if (!(amountUsedOfIngredient >= ingredient.LiquidAmount) && EquipmentPrototype.PlasticBottle != null && EquipmentPrototype.SealedContainer != null)
				{
					minRequiredWeight = Math.Min(minRequiredWeight, EquipmentPrototype.PlasticBottle.Weight);
					desiredWeight = Math.Max(desiredWeight, EquipmentPrototype.SealedContainer.Weight);
				}
			}
			else
			{
				if (amountUsedOfIngredient >= (float)ingredient.Amount)
				{
					continue;
				}
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					if (minRequiredWeight == 0f)
					{
						minRequiredWeight = prototype.Weight;
					}
					else
					{
						minRequiredWeight = Math.Min(minRequiredWeight, prototype.Weight);
					}
					int num = Math.Max(1, Math.Min(Mathf.CeilToInt((float)ingredient.Amount - amountUsedOfIngredient), Mathf.CeilToInt(carryCapacity * 0.5f / prototype.Weight)));
					desiredWeight = Math.Max(desiredWeight, prototype.Weight * (float)num);
				}
			}
		}
	}

	public bool IsRemainingIngredient(Equipment item)
	{
		Ingredient ingredient = Recipe.GetIngredient(item);
		if (ingredient != null)
		{
			float amountUsedOfIngredient = GetAmountUsedOfIngredient(ingredient);
			if (ingredient.IsLiquid())
			{
				return amountUsedOfIngredient < ingredient.LiquidAmount;
			}
			return amountUsedOfIngredient < (float)ingredient.Amount;
		}
		return false;
	}

	public float GetAmountUsedOfIngredient(Ingredient ingredient)
	{
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < IngredientsUsed.Count; i++)
		{
			if (IngredientsUsed[i].Prototype != null && ingredient.Prototypes != null && ingredient.Prototypes.Contains(IngredientsUsed[i].Prototype))
			{
				num += IngredientsUsed[i].Amount;
			}
			if (IngredientsUsed[i].LiquidPrototype != null && ingredient.LiquidTypes != null && ingredient.LiquidTypes.Contains(IngredientsUsed[i].LiquidPrototype))
			{
				num2 += IngredientsUsed[i].LiquidAmount;
			}
		}
		return Math.Max(num2, num);
	}

	public void UseIngredient(EquipmentPrototype proto, int amount)
	{
		for (int i = 0; i < IngredientsUsed.Count; i++)
		{
			if (IngredientsUsed[i].Prototype != null && IngredientsUsed[i].Prototype == proto)
			{
				IngredientsUsed[i].Amount += amount;
				return;
			}
		}
		IngredientsUsed.Add(UsedIngredient.CreateItem(proto, amount));
	}

	public void UseLiquidIngredient(LiquidPrototype liquidType, float amount)
	{
		for (int i = 0; i < IngredientsUsed.Count; i++)
		{
			if (IngredientsUsed[i].LiquidPrototype != null && IngredientsUsed[i].LiquidPrototype == liquidType)
			{
				IngredientsUsed[i].LiquidAmount += amount;
				return;
			}
		}
		IngredientsUsed.Add(UsedIngredient.CreateLiquid(liquidType, amount));
	}

	public bool IsBuiltEnoughToBeAnObstacle()
	{
		return IngredientsUsed.Count > 0;
	}

	public bool IsCompleted()
	{
		if (Recipe.Ingredients.Count == 0)
		{
			return TimeTakenOnResource >= Recipe.CraftingTime;
		}
		GetProgress(out var used, out var total);
		return used >= total - 0.0001f;
	}

	public void GetProgress(out float used, out float total)
	{
		used = 0f;
		total = 0f;
		for (int i = 0; i < Recipe.Ingredients.Count; i++)
		{
			float amountUsedOfIngredient = GetAmountUsedOfIngredient(Recipe.Ingredients[i]);
			if (Recipe.Ingredients[i].IsLiquid())
			{
				float liquidCapacity = EquipmentPrototype.PlasticBottle.LiquidCapacity;
				used += amountUsedOfIngredient / liquidCapacity;
				total += Recipe.Ingredients[i].LiquidAmount / liquidCapacity;
			}
			else
			{
				used += amountUsedOfIngredient;
				total += Recipe.Ingredients[i].Amount;
			}
		}
	}

	public float GetProgress()
	{
		GetProgress(out var used, out var total);
		float num = Recipe.CraftingTime / (float)Math.Max(1, Recipe.CountIngredients());
		used += TimeTakenOnResource / num;
		return Math.Min((total > 0f) ? (used / total) : 1f, 1f);
	}

	public bool ShowHalfBuiltModel()
	{
		if (IngredientsUsed.Count <= 0)
		{
			return TimeTakenOnResource > 0f;
		}
		return true;
	}

	public GameObject UnityInitCordon(TileObject owner)
	{
		GameTerrain instance = GameTerrain.Instance;
		float halfSize = instance.HalfSize;
		float y = owner.Pos.y;
		TerrainCoord minTile = owner.GetMinTile();
		TerrainCoord maxTile = owner.GetMaxTile();
		GameObject gameObject = new GameObject();
		gameObject.transform.position = owner.Pos;
		if (owner is PitTrap)
		{
			gameObject.transform.position = instance.GetOriginalTileCentrePos(owner.GetCentreTile());
			y = gameObject.transform.position.y;
		}
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			UnityEngine.Object.Instantiate(Prop.ConstructionSiteCordonModel.GetAsset(), new Vector3((float)i - halfSize, y, (float)minTile.y - halfSize), Quaternion.Euler(-90f, 0f, 0f), gameObject.transform);
			UnityEngine.Object.Instantiate(Prop.ConstructionSiteCordonModel.GetAsset(), new Vector3((float)i - halfSize + 1f, y, (float)maxTile.y - halfSize + 1f), Quaternion.Euler(-90f, 180f, 0f), gameObject.transform);
		}
		for (int j = minTile.y; j <= maxTile.y; j++)
		{
			UnityEngine.Object.Instantiate(Prop.ConstructionSiteCordonModel.GetAsset(), new Vector3((float)minTile.x - halfSize, y, (float)j - halfSize + 1f), Quaternion.Euler(-90f, 90f, 0f), gameObject.transform);
			UnityEngine.Object.Instantiate(Prop.ConstructionSiteCordonModel.GetAsset(), new Vector3((float)maxTile.x - halfSize + 1f, y, (float)j - halfSize), Quaternion.Euler(-90f, -90f, 0f), gameObject.transform);
		}
		return gameObject;
	}
}
