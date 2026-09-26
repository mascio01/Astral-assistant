using System.Collections.Generic;

public class Loadout
{
	public string LoadoutName;

	public string NativeName;

	public string NativeDescription;

	public List<LoadoutEquipment> Equipment = new List<LoadoutEquipment>();

	public int[] SkillPoints = new int[10];

	public string GetNameKey()
	{
		return "LOADOUT_NAME_" + LoadoutName;
	}

	public string GetDescriptionKey()
	{
		return "LOADOUT_DESC_" + LoadoutName;
	}
}
