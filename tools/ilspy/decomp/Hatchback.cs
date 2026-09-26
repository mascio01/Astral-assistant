internal class Hatchback : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\VisionGames_Rusty_Car_5_v2");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Hatchback;
	}
}
