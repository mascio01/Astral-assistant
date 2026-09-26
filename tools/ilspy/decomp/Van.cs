internal class Van : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\Van");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Van;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
