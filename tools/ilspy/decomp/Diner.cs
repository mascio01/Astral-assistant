public class Diner : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/Diner");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Diner;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
