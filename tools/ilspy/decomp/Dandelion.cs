public class Dandelion : Flower
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Trees/Dandelion1", 50),
		new PrefabResource("Prefabs/Trees/Dandelion2", 50)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Dandelion;
	}
}
