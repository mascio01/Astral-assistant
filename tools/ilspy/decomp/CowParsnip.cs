public class CowParsnip : Flower
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Trees/CowParsnip1", 50),
		new PrefabResource("Prefabs/Trees/CowParsnip2", 50)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.CowParsnip;
	}
}
