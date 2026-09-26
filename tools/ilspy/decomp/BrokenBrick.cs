public class BrokenBrick : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenBrick", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenBrick;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
