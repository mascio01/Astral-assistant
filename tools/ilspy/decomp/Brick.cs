public class Brick : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrick", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Brick;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
