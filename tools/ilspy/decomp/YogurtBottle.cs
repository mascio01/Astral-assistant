public class YogurtBottle : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropYogurtBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.YogurtBottle;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
