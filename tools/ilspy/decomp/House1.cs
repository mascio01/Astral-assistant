public class House1 : Building
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.House1;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
