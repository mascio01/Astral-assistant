public class BrokenTinCan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenTinCan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenTinCan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
