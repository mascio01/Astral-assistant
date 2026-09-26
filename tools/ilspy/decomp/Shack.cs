public class Shack : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/Shack");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Shack;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
