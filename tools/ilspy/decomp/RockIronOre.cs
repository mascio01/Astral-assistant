public class RockIronOre : Rock
{
	private static PrefabResource[] UnityRocks = new PrefabResource[5]
	{
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_01_Snow_iron_ore", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_02_Snow_iron_ore", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_03_Snow_iron_ore", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_04_Snow_iron_ore", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_05_Snow_iron_ore", 20)
	};

	public RockIronOre()
	{
		ResourceRemaining = 10;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RockIronOre;
	}
}
