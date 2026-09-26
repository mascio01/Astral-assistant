internal class Van2 : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Old Car 2");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Van2;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
