public class MotelSectionStraight : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/MotelSectionStraight");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MotelSectionStraight;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}
}
