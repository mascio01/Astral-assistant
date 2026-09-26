public class PlasticBottleOnGround : Trash
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Abandoned Props Pack PBR/PropPlasticBottle", 10);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PlasticBottleOnGround;
	}

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}
}
