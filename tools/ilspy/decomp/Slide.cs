public class Slide : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Old School Playground Gear/Slide");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Slide;
	}
}
