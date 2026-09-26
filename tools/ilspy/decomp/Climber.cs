public class Climber : Prop
{
	private static PrefabResource[] Model = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Climber_A"),
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Climber_B"),
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Climber_C")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Climber;
	}
}
