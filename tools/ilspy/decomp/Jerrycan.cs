public class Jerrycan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropJerrycan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Jerrycan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
