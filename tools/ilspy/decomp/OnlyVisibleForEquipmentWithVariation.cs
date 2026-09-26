using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForEquipmentWithVariation : Attribute
{
	public enum VariationType
	{
		Color1,
		Color2,
		Color3,
		Material
	}

	public VariationType Type;

	public OnlyVisibleForEquipmentWithVariation(VariationType variationType)
	{
		Type = variationType;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (!(obj is Template { Type: TemplateType.Equipment } template))
		{
			return false;
		}
		EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(template.EquipmentPrototypeName);
		if (equipmentPrototype == null)
		{
			return false;
		}
		switch (Type)
		{
		case VariationType.Color1:
			if (equipmentPrototype.ColorVariations != null)
			{
				return equipmentPrototype.ColorVariations.Length != 0;
			}
			return false;
		case VariationType.Color2:
			if (equipmentPrototype.ColorVariations2 != null)
			{
				return equipmentPrototype.ColorVariations2.Length != 0;
			}
			return false;
		case VariationType.Color3:
			if (equipmentPrototype.ColorVariations3 != null)
			{
				return equipmentPrototype.ColorVariations3.Length != 0;
			}
			return false;
		case VariationType.Material:
			if (equipmentPrototype.MaterialVariations != null)
			{
				return equipmentPrototype.MaterialVariations.Length != 0;
			}
			return false;
		default:
			return false;
		}
	}
}
