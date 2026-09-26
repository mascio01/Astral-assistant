public class PlasticBottle : Equipment
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PlasticBottle;
	}

	public override bool IsBottle()
	{
		return true;
	}
}
