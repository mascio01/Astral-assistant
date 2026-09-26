public class PepperPlant : PlantableCrop
{
	private static PrefabResource[] UnityModel = new PrefabResource[5]
	{
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Soil", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage1", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage2", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage3", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant", 20)
	};

	private static PrefabResource[] UnityModelDead = new PrefabResource[5]
	{
		UnityModel[0],
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage1_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage2_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Stage3_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pepper\\PepperPlant_Dead", 10)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PepperPlant;
	}
}
