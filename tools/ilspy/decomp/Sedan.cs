internal class Sedan : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Old Car_1");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Sedan;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
