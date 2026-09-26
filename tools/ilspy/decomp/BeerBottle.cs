public class BeerBottle : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBeerBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BeerBottle;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
