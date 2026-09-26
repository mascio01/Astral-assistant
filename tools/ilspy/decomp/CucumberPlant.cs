public class CucumberPlant : PlantableCrop
{
	private static PrefabResource[] UnityModel = new PrefabResource[4]
	{
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Soil", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber_Stage1", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber_Stage2", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber", 20)
	};

	private static PrefabResource[] UnityModelDead = new PrefabResource[4]
	{
		UnityModel[0],
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber_Stage1_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber_Stage2_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Cucumber\\Cucumber_Dead", 10)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.CucumberPlant;
	}
}
