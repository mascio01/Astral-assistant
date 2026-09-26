public class TakeawayCoffee : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropTakewayCoffee", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.TakeawayCoffee;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
