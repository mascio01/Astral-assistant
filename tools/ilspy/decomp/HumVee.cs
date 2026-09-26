internal class HumVee : EnterableVehicle
{
	public static PrefabResource Model = new PrefabResource("Prefabs\\Vehicles\\VisionGames_Rusty_Car_1");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.HumVee;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 2;
	}
}
