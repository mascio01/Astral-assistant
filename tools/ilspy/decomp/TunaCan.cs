public class TunaCan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropTunaCan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TunaCan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
