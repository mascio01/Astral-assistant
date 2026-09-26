public class Carousel : Prop
{
	private static PrefabResource[] Model = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Carousel_A"),
		new PrefabResource("Prefabs/Props/Old School Playground Gear/Carousel_B")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Carousel;
	}
}
