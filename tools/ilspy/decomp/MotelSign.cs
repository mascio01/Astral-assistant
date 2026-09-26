public class MotelSign : Prop
{
	private static PrefabResource[] Models = new PrefabResource[3]
	{
		new PrefabResource("Prefabs/Props/MotelSign"),
		new PrefabResource("Prefabs/Props/MotelSign1"),
		new PrefabResource("Prefabs/Props/MallSign")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MotelSign;
	}
}
