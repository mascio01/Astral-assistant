public class EnergyCan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropEnergyCan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.EnergyCan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
