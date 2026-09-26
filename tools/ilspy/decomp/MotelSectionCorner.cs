public class MotelSectionCorner : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/MotelSectionCorner");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MotelSectionCorner;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}
}
