public class BrokenMilkPackA : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenMilkPackA", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenMilkPackA;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
