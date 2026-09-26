public class WatchTower : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/WatchTower");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WatchTower;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 1;
	}
}
