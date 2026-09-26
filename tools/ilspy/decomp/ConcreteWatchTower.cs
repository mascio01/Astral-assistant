public class ConcreteWatchTower : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/ConcreteTower");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.ConcreteWatchTower;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 1;
	}
}
