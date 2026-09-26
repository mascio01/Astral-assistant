using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public class Ingredient
{
	[XmlIgnore]
	public List<EquipmentPrototype> Prototypes;

	public List<string> PrototypeNames;

	[DefaultValue(0)]
	public int Amount;

	[XmlIgnore]
	public List<LiquidPrototype> LiquidTypes;

	public List<string> LiquidTypeNames;

	[DefaultValue(0f)]
	public float LiquidAmount;

	[DefaultValue(true)]
	public bool Interchangeable = true;

	[DefaultValue(IngredientInfectionState.Any)]
	public IngredientInfectionState IngredientInfectionState;

	[DefaultValue(IngredientReturnType.Half)]
	public IngredientReturnType ReturnType;

	public bool IsItem()
	{
		if (Prototypes != null)
		{
			return Prototypes.Count > 0;
		}
		return false;
	}

	public bool IsLiquid()
	{
		if (LiquidTypes != null)
		{
			return LiquidTypes.Count > 0;
		}
		return false;
	}

	public bool MatchesItem(Equipment item, Recipe recipe)
	{
		if (Prototypes != null && Prototypes.Contains(item.GetPrototype()) && item.MatchesIngredientInfectionState(IngredientInfectionState) && (item.GetLiquidContentsAmount() == 0f || recipe.IsLiquidIngredient(item.GetLiquidContentsType())))
		{
			return true;
		}
		if (LiquidTypes != null && LiquidTypes.Contains(item.GetLiquidContentsType()) && item.MatchesIngredientInfectionState(IngredientInfectionState))
		{
			return true;
		}
		return false;
	}

	public bool HasAnyOfIngredient(Character crafter, TileObject carrier, Recipe recipe, Equipment usingItem, Character checkEquipmentPolicyForUser)
	{
		if (carrier.GetInventory() == null)
		{
			return false;
		}
		return carrier.GetInventory().FindIngredient(crafter, carrier, this, recipe, usingItem, null, checkEquipmentPolicyForUser) != null;
	}

	public bool HasEnoughOfIngredient(Character crafter, TileObject carrier, Recipe recipe, Equipment usingItem, Character checkEquipmentPolicyForUser)
	{
		float amountNeeded;
		return HasEnoughOfIngredient(crafter, carrier, recipe, usingItem, checkEquipmentPolicyForUser, out amountNeeded);
	}

	public bool HasEnoughOfIngredient(Character crafter, TileObject carrier, Recipe recipe, Equipment usingItem, Character checkEquipmentPolicyForUser, out float amountNeeded)
	{
		if (carrier.GetInventory() == null)
		{
			amountNeeded = (float)Amount + LiquidAmount;
			return false;
		}
		if (Prototypes != null)
		{
			int num = 0;
			foreach (EquipmentPrototype prototype in Prototypes)
			{
				for (int i = 0; i < carrier.GetInventory().Count; i++)
				{
					Equipment item = carrier.GetInventory().GetItem(i);
					if (item.GetPrototype() != prototype || !item.MatchesIngredientInfectionState(IngredientInfectionState) || !crafter.CanUseItemForCrafting(carrier, item, usingItem, recipe) || (item.GetLiquidContentsAmount() != 0f && !recipe.IsLiquidIngredient(item.GetLiquidContentsType())))
					{
						continue;
					}
					if (checkEquipmentPolicyForUser != null)
					{
						if (!checkEquipmentPolicyForUser.IsActionAllowedForItem(item, EquipmentPolicyAction.CanCraftWith))
						{
							continue;
						}
						int val = ((carrier is Character character) ? ((int)character.GetTargetAmountToCarryIncludingAmmo(item.GetPrototype(), null, item.InfectedWith)) : 0);
						num -= Math.Min(item.GetAmount(), val);
					}
					num += item.GetAmount();
				}
			}
			if (num < Amount)
			{
				amountNeeded = Amount - num;
				return false;
			}
		}
		if (LiquidTypes != null)
		{
			float num2 = 0f;
			for (int j = 0; j < LiquidTypes.Count; j++)
			{
				num2 += carrier.GetInventory().GetAmountOfLiquidType(carrier, LiquidTypes[j], out var _, IngredientInfectionState, checkEquipmentPolicyForUser);
			}
			if (num2 < LiquidAmount)
			{
				amountNeeded = LiquidAmount - num2;
				return false;
			}
		}
		amountNeeded = 0f;
		return true;
	}

	public float GetAveragePrice()
	{
		float num = 0f;
		if (Prototypes != null)
		{
			float num2 = float.MaxValue;
			foreach (EquipmentPrototype prototype in Prototypes)
			{
				num2 = Math.Min(num2, prototype.BasePrice);
			}
			num += num2 * (float)Amount;
		}
		if (LiquidTypes != null)
		{
			float num3 = float.MaxValue;
			foreach (LiquidPrototype liquidType in LiquidTypes)
			{
				num3 = Math.Min(num3, liquidType.BasePricePerFlOz);
			}
			num += num3 * LiquidAmount;
		}
		return num;
	}
}
