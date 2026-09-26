public class BigPack : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBigPack", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BigPack;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
