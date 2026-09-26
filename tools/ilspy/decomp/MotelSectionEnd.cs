public class MotelSectionEnd : Building
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Buildings/MotelSectionEnd");

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MotelSectionEnd;
	}

	protected override int GetOldInhabitantsCount()
	{
		return 4;
	}
}
