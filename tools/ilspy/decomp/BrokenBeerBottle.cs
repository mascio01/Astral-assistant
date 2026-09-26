public class BrokenBeerBottle : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenBeerBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenBeerBottle;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
