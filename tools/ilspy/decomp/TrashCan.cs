public class TrashCan : Prop
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/GarbageAndTrashProps/Trashcan_A"),
		new PrefabResource("Prefabs/Props/GarbageAndTrashProps/Trashcan_B")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TrashCan;
	}
}
