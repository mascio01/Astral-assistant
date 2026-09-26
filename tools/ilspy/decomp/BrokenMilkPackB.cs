public class BrokenMilkPackB : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenMilkPackB", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenMilkPackB;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
