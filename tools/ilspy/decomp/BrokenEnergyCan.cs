public class BrokenEnergyCan : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropBrokenEnergyCan", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BrokenEnergyCan;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
