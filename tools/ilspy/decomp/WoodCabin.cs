public class WoodCabin : Building
{
	private static PrefabResource[] Model = new PrefabResource[4]
	{
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_01"),
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_04"),
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_06"),
		new PrefabResource("Prefabs/Buildings/Wooden_Houses_Set/Houses_Wood_09")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WoodCabin;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}
}
