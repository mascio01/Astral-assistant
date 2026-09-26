public class FoodPack : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropFoodPack", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.FoodPack;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
