public class BrokenCoffeeMug : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenCoffeeMug", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenCoffeeMug;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
