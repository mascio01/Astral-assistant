public class Billboard : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Billboard");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Billboard;
	}
}
