internal class Sedan2 : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Old Rusty Car");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Sedan2;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
