public class Fireweed : Flower
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Trees/Fireweed1", 50),
		new PrefabResource("Prefabs/Trees/Fireweed2", 50)
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Fireweed;
	}
}
