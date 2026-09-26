public class PumpkinPlant : PlantableCrop
{
	private static PrefabResource[] UnityModel = new PrefabResource[6]
	{
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Soil", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage1", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage2", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage3", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage4", 20),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin", 20)
	};

	private static PrefabResource[] UnityModelDead = new PrefabResource[6]
	{
		UnityModel[0],
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage1_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage2_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage3_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Stage4_Dead", 10),
		new PrefabResource("Prefabs\\Props\\Farm Plants\\Plants\\Pumpkin\\Pumpkin_Dead", 10)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PumpkinPlant;
	}
}
