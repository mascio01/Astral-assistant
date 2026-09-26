using System;
using UnityEngine;

public struct GatheredItem : IReflectable, IComparable<GatheredItem>
{
	public EquipmentPrototype Type;

	public LiquidPrototype Liquid;

	public InfectionType InfectedWith;

	public int Amount;

	public static GatheredItem Create(EquipmentPrototype type, LiquidPrototype liquid, InfectionType infectionType, int amount)
	{
		return new GatheredItem
		{
			Type = type,
			Liquid = liquid,
			InfectedWith = infectionType,
			Amount = amount
		};
	}

	public static GatheredItem Create(Equipment item)
	{
		return new GatheredItem
		{
			Type = item.GetPrototype(),
			Liquid = ((item.GetLiquidContentsType() != null) ? item.GetLiquidContentsType() : item.DesignatedLiquid),
			InfectedWith = item.InfectedWith,
			Amount = item.GetAmount()
		};
	}

	public static GatheredItem CreateIncludingLiquidAmount(Equipment item)
	{
		return new GatheredItem
		{
			Type = item.GetPrototype(),
			Liquid = ((item.GetLiquidContentsType() != null) ? item.GetLiquidContentsType() : item.DesignatedLiquid),
			InfectedWith = item.InfectedWith,
			Amount = ((item.GetLiquidContentsType() != null) ? Mathf.CeilToInt(item.GetLiquidContentsAmount()) : item.GetAmount())
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.AddAfter(ref Liquid, 340);
		reflector.AddAfter(ref InfectedWith, 448);
		reflector.Add(ref Amount);
	}

	public int CompareTo(GatheredItem other)
	{
		if (Type.Weight > other.Type.Weight)
		{
			return -1;
		}
		if (Type.Weight < other.Type.Weight)
		{
			return 1;
		}
		if (Type.NameHash < other.Type.NameHash)
		{
			return -1;
		}
		if (Type.NameHash > other.Type.NameHash)
		{
			return 1;
		}
		return 0;
	}

	public bool Matches(GatheredItem other)
	{
		if (InfectedWith != other.InfectedWith)
		{
			return false;
		}
		if (Liquid == null)
		{
			return Type == other.Type;
		}
		return other.Liquid == Liquid;
	}

	public GatheredItem Combine(GatheredItem other)
	{
		return Create(Type, Liquid, InfectedWith, Amount + other.Amount);
	}
}
