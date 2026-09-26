public class BrokenPlate : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenPlate", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenPlate;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
