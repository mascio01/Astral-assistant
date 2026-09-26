public class MiniMart : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/MiniMart");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MiniMart;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
