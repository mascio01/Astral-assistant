public class SunflowerPlant : PlantableCrop
{
	private static PrefabResource[] UnityModel = new PrefabResource[6]
	{
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Soil", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage1", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage2", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage3", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage4", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower", 20)
	};

	private static PrefabResource[] UnityModelDead = new PrefabResource[6]
	{
		UnityModel[0],
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage1_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage2_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage3_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Stage4_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Sunflower\\Sunflower_Dead", 10)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.SunflowerPlant;
	}
}
