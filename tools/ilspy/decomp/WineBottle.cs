public class WineBottle : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropWineBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.WineBottle;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
