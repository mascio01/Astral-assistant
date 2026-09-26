public class GreenStrainSpawnPoint : ZombieSpawnPoint
{
	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/GreenStrainSpawnPoint", 10);

	public override InfectionType Infection => InfectionType.Green;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GreenStrainSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
