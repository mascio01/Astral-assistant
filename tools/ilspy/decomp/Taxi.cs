internal class Taxi : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Old Car_Taxi");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Taxi;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
