using System.Collections.Generic;

public class CharacterCreationSettings : CrewMemberSettings, IReflectable
{
	public GangName GangName = new GangName();

	public int RandomSeed;

	public int StartDayOfYear;

	public float StartHourOfDay;

	public MapSize MapSize = MapSize.Large;

	public DifficultySettings DifficultySettings = new DifficultySettings();

	public int HitTheRoadCount;

	public bool AchievementsEnabled = true;

	public List<CrewMemberSettings> Crew = new List<CrewMemberSettings>();

	public List<Equipment> VehicleInventory = new List<Equipment>();

	public PropPrototype VehicleProto;

	public float VehicleDamageFraction;

	public int VehicleMaterialVariation = -1;

	public int VehicleColorVariation = -1;

	public int VehicleColorVariation2 = -1;

	public int VehicleColorVariation3 = -1;

	public int VehicleColorVariation4 = -1;

	public List<KeyValuePair<string, float>> Variables = new List<KeyValuePair<string, float>>();

	public List<EquipmentPolicy> CommunityEquipmentPolicies = new List<EquipmentPolicy>();

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref FirstName);
		reflector.Add(ref Surname);
		reflector.Add(Appearance);
		int value = Inventory.Count;
		reflector.Add(ref value);
		for (int i = 0; i < value; i++)
		{
			Equipment equipment = null;
			BaseObjectType value2 = BaseObjectType.Invalid;
			if (reflector.IsDeserialising)
			{
				reflector.Add(ref value2);
				equipment = BaseObjectManager.Create(value2) as Equipment;
				Inventory.Add(equipment);
			}
			else
			{
				equipment = Inventory[i];
				value2 = equipment.GetBaseObjectType();
				reflector.Add(ref value2);
			}
			reflector.Add(equipment);
		}
		for (int j = 0; j < 10; j++)
		{
			reflector.Add(ref Skills[j]);
		}
		reflector.AddStringList(ref Personality);
	}

	public int GetAmountOfEquipmentByName(string name)
	{
		foreach (Equipment item in Inventory)
		{
			if (item.GetPrototype().Name == name)
			{
				return item.GetAmount();
			}
		}
		return 0;
	}
}
