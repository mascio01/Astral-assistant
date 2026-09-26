public class Basket : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBasket", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Basket;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
