public class TrashBag : Prop
{
	private static PrefabResource[] Model = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Props/GarbageAndTrashProps/Trashbag"),
		new PrefabResource("Prefabs/Props/GarbageAndTrashProps/GargabeBag_A"),
		new PrefabResource("Prefabs/Props/GarbageAndTrashProps/GargabeBag_B")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TrashBag;
	}
}
