public class MediumCanister : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropMediumCanister", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MediumCanister;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
