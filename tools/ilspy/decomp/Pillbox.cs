public class Pillbox : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/Bunker");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Pillbox;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 1;
	}
}
