public class CoffeeMug : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropCoffeeMug", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.CoffeeMug;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
