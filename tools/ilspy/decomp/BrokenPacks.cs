public class BrokenPacks : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenPacks", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenPacks;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
