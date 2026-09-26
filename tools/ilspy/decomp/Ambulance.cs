internal class Ambulance : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\ambulance_Model1_LD");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Ambulance;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
