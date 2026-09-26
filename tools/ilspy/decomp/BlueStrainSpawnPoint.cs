public class BlueStrainSpawnPoint : ZombieSpawnPoint
{
	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/BlueStrainSpawnPoint", 10);

	public override InfectionType Infection => InfectionType.Blue;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BlueStrainSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
