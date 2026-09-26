using System.Collections.Generic;

public class LiquidSpawner : DebugMenu
{
	public LiquidSpawner()
		: base(GameImpl.Translate("DEBUG_LiquidSpawner"))
	{
		foreach (KeyValuePair<string, LiquidPrototype> item in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			LiquidPrototype proto = item.Value;
			Items.Add(new DebugMenuFloatAdjuster(proto.Name, 0f, GetContainerCapacity, () => GetAmount(proto), delegate(float v)
			{
				SetAmount(proto, v);
			}));
		}
	}

	public float GetAmount(LiquidPrototype proto)
	{
		Equipment equipment = GetEquipment();
		if (equipment == null || equipment.GetLiquidContentsType() != proto)
		{
			return 0f;
		}
		return equipment.GetLiquidContentsAmount();
	}

	public void SetAmount(LiquidPrototype proto, float v)
	{
		Equipment equipment = GetEquipment();
		if (equipment != null)
		{
			equipment.SetLiquid(proto, v);
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public float GetContainerCapacity()
	{
		return GetEquipment()?.GetLiquidCapacity() ?? 0f;
	}

	public Equipment GetEquipment()
	{
		Character currentCharacter = CharacterEditor.GetCurrentCharacter();
		if (currentCharacter != null)
		{
			return currentCharacter.EquippedItem;
		}
		TileObject currentObject = CharacterEditor.GetCurrentObject();
		if (currentObject != null && currentObject.GetInventory() != null)
		{
			for (int num = currentObject.GetInventory().Count - 1; num >= 0; num--)
			{
				Equipment item = currentObject.GetInventory().GetItem(num);
				if (item.GetLiquidCapacity() > 0f)
				{
					return item;
				}
			}
		}
		return null;
	}
}
