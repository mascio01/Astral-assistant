public class Maize : PlantableCrop
{
	private static PrefabResource[] UnityModel = new PrefabResource[5]
	{
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Soil", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage1", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage2", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage3", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn", 20)
	};

	private static PrefabResource[] UnityModelDead = new PrefabResource[5]
	{
		UnityModel[0],
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage1_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage2_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Stage3_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Corn\\Corn_Dead", 10)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Maize;
	}
}
