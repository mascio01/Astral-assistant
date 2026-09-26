public class BrokenTakeawayCoffee : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenTakewayCoffee", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenTakeawayCoffee;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
