public class RedStrainSpawnPoint : ZombieSpawnPoint
{
	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/RedStrainSpawnPoint", 10);

	public override InfectionType Infection => InfectionType.Red;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RedStrainSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
