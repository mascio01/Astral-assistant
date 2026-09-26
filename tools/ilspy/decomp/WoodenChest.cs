public class WoodenChest : Prop
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/chest"),
		new PrefabResource("Prefabs/Props/Gold_Chest 1")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WoodenChest;
	}
}
