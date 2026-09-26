public class BoulderLeadOre : Boulder
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Nature Package/Rock1_lead_ore"),
		new PrefabResource("Prefabs/Nature Package/Rock2_lead_ore")
	};

	public BoulderLeadOre()
	{
		ResourceRemaining = 80;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BoulderLeadOre;
	}
}
