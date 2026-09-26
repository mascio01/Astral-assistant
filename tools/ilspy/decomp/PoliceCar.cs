internal class PoliceCar : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\carPoliceOriginal");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PoliceCar;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
