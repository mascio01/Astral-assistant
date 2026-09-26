internal class Pickup : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\VisionGames_Rusty_Car_3");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Pickup;
	}
}
