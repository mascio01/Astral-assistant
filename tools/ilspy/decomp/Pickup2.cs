internal class Pickup2 : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Old Car 3");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Pickup2;
	}
}
