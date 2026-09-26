public class BoulderFlint : Boulder
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Nature Package/Rock1_flint"),
		new PrefabResource("Prefabs/Nature Package/Rock2_flint")
	};

	public BoulderFlint()
	{
		ResourceRemaining = 80;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BoulderFlint;
	}
}
