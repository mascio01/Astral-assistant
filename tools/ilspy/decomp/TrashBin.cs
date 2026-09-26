public class TrashBin : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/GarbageAndTrashProps/Trashbin");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TrashBin;
	}
}
