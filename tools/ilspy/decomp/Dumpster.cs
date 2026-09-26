public class Dumpster : TiltedProp
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/GarbageAndTrashProps/Dumpster");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Dumpster;
	}
}
