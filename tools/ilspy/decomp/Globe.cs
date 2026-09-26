public class Globe : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Old School Playground Gear/Globe");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Globe;
	}
}
