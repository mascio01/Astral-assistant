public class TinCan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropTinCan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TinCan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
