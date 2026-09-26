public class GasStation : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/GasStation");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GasStation;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
