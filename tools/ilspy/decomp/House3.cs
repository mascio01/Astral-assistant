public class House3 : Building
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.House3;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 8;
	}
}
