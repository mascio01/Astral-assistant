public class GasStationSign : Prop
{
	private static PrefabResource[] Models = new PrefabResource[2]
	{
		new PrefabResource("Prefabs/Props/GasStationSign"),
		new PrefabResource("Prefabs/Props/GasStation/GasStationSign")
	};

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.GasStationSign;
	}
}
