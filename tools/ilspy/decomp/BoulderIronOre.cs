public class BoulderIronOre : Boulder
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Nature Package/Rock1_iron_ore"),
		new PrefabResource("Prefabs/Nature Package/Rock2_iron_ore")
	};

	public BoulderIronOre()
	{
		ResourceRemaining = 80;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BoulderIronOre;
	}
}
