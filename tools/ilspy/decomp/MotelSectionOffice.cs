public class MotelSectionOffice : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/MotelSectionOffice");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MotelSectionOffice;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}
}
