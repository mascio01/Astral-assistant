public class WhiteStrainSpawnPoint : ZombieSpawnPoint
{
	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/WhiteStrainSpawnPoint", 10);

	public override InfectionType Infection => InfectionType.White;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WhiteStrainSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
