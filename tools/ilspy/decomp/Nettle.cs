public class Nettle : Flower
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Trees/Nettle1", 50),
		new PrefabResource("Prefabs/Trees/Nettle2", 50)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Nettle;
	}
}
