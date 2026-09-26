public class WoodBarn : Building
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_03"),
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_08")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WoodBarn;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
