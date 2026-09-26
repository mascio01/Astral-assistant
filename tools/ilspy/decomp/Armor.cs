using System;
using System.Collections.Generic;
using UnityEngine;

public class Armor : Equipment
{
	public List<Injury> ArmorDamagePoints = new List<Injury>();

	public float Protection = 1f;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Armor;
	}

	public float GetDamageAbsorption()
	{
		return Math.Max(0.001f, Prototype.DamageAbsorption);
	}

	public override float GetArmorProtection()
	{
		return Protection;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Protection);
		if (reflector.Version >= 279)
		{
			reflector.Add(ref ArmorDamagePoints);
		}
	}

	public Color GetArmorIconCol()
	{
		if (!(Protection <= 0f))
		{
			if (!(Protection <= 0.25f))
			{
				if (!(Protection <= 0.5f))
				{
					if (!(Protection <= 0.75f))
					{
						return Color.white;
					}
					return new Color(0.75f, 0.75f, 0.75f, 1f);
				}
				return new Color(0.5f, 0.5f, 0.5f, 1f);
			}
			return new Color(0.25f, 0.25f, 0.25f, 1f);
		}
		return new Color(0.5f, 0f, 0f, 1f);
	}

	public void Repair(Character character)
	{
		Protection = 1f;
		ArmorDamagePoints.Clear();
		if (character.IsWearing(this))
		{
			character.WantUnityUpdateDecals = true;
			character.WantUnityUpdateInjuries = true;
			character.CanSkipUnityUpdate = false;
		}
	}

	public override void Strip(Character character)
	{
		base.Strip(character);
		for (int i = 0; i < ArmorDamagePoints.Count; i++)
		{
			Injury value = ArmorDamagePoints[i];
			value.ArrowStuck = false;
			ArmorDamagePoints[i] = value;
			character.WantUnityUpdateDecals = true;
			character.CanSkipUnityUpdate = false;
		}
	}
}
