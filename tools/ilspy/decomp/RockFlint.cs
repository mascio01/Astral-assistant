public class RockFlint : Rock
{
	private static PrefabResource[] UnityRocks = new PrefabResource[5]
	{
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_01_Snow_flint", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_02_Snow_flint", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_03_Snow_flint", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_04_Snow_flint", 20),
		new PrefabResource("Prefabs\\Dynamic Nature\\Rocks and Stones\\prefab_rock_05_Snow_flint", 20)
	};

	public RockFlint()
	{
		ResourceRemaining = 10;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RockFlint;
	}
}
