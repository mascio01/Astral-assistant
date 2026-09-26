using System;
using System.Collections.Generic;
using System.Text;

public class DebugMenuLootLocationAdjuster : DebugMenuItemAdjuster
{
	private BaseObject Obj;

	public DebugMenuLootLocationAdjuster(string name, BaseObject obj)
		: base(name)
	{
		Obj = obj;
		DecrementEvent = OnDecrement;
		IncrementEvent = OnIncrement;
		BuildValueStringEvent = BuildLootLocationDisplayString;
		BuildDisplayString();
	}

	public void OnDecrement()
	{
		if (Obj != null)
		{
			List<string> lootLocationOptions = BaseMenu.GetLootLocationOptions();
			int num = Math.Max(0, lootLocationOptions.IndexOf(Obj.GetLootLocation()));
			Obj.SetLootLocation(lootLocationOptions[(num + lootLocationOptions.Count - 1) % lootLocationOptions.Count]);
			BuildDisplayString();
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public void OnIncrement()
	{
		if (Obj != null)
		{
			List<string> lootLocationOptions = BaseMenu.GetLootLocationOptions();
			int num = Math.Max(0, lootLocationOptions.IndexOf(Obj.GetLootLocation()));
			Obj.SetLootLocation(lootLocationOptions[(num + lootLocationOptions.Count + 1) % lootLocationOptions.Count]);
			BuildDisplayString();
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public void BuildLootLocationDisplayString(ref StringBuilder value)
	{
		value.Length = 0;
		value.Append(Name);
		value.Append(" < ");
		if (Obj != null)
		{
			value.Append(Obj.GetLootLocation());
		}
		value.Append(" >");
	}
}
