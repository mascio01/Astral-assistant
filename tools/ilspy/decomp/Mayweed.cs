public class Mayweed : Flower
{
	private static PrefabResource[] Models = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Trees/Mayweed1", 50),
		new PrefabResource("Prefabs/Trees/Mayweed2", 50),
		new PrefabResource("Prefabs/Trees/Mayweed3", 50)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Mayweed;
	}
}
