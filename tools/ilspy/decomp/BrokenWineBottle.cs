public class BrokenWineBottle : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenWineBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenWineBottle;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
