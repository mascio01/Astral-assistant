public class MilkPack : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropMilkPack", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MilkPack;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
