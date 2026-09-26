public class Swing : Prop
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Swing_A"),
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Swing_B")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Swing;
	}
}
