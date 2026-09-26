public class GasPumps : Prop
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/GasStation/GasPumps1"),
		new PrefabResource("Prefabs/Props/GasStation/GasPumps2")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GasPumps;
	}
}
