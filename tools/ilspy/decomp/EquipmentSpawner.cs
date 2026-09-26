using System.Collections.Generic;
using UnityEngine;

public class EquipmentSpawner : DebugMenu
{
	public EquipmentSpawner()
		: base(GameImpl.Translate("DEBUG_EquipmentSpawner"))
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			EquipmentPrototype proto = item.Value;
			Items.Add(new DebugMenuItemEquipment(proto, delegate(int colorVariation, int colorVariation2, int colorVariation3, int materialVariation, int amount)
			{
				bool locked = Session.Instance.DeterministicRand.Locked;
				Session.Instance.DeterministicRand.Locked = false;
				SpawnEquipment(proto, colorVariation, colorVariation2, colorVariation3, materialVariation, amount);
				Session.Instance.DeterministicRand.Locked = locked;
			}));
		}
	}

	public void SpawnEquipment(EquipmentPrototype proto, int colorVariation, int colorVariation2, int colorVariation3, int materialVariation, int amount)
	{
		TileObject currentObject = CharacterEditor.GetCurrentObject();
		if (currentObject != null && currentObject.GetInventory() != null && amount > 0)
		{
			Equipment equipment = Equipment.Spawn(proto, amount);
			equipment.ColorVariation = colorVariation;
			equipment.ColorVariation2 = colorVariation2;
			equipment.ColorVariation3 = colorVariation3;
			equipment.MaterialVariation = materialVariation;
			if (proto.DefaultLiquidPrototype != null && proto.LiquidCapacity > 0f)
			{
				float num = Mathf.Lerp(proto.DefaultLiquidFilledMin, proto.DefaultLiquidFilledMax, MathUtil.RandomFloat(equipment.Id)) / 100f;
				equipment.SetLiquid(proto.DefaultLiquidPrototype, proto.LiquidCapacity * num);
			}
			if (proto.DefaultInfectionTypeMin > InfectionType.None)
			{
				equipment.InfectedWith = (InfectionType)MathUtil.RandomInt(equipment.Id, (int)proto.DefaultInfectionTypeMin, (int)(proto.DefaultInfectionTypeMax + 1));
			}
			currentObject.GetInventory().Add(currentObject, equipment);
			Session.Instance.AchievementsEnabled = false;
		}
	}
}
