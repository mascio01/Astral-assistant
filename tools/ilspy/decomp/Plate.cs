public class Plate : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropPlate", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Plate;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
