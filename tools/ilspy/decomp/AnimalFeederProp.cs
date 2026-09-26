public class AnimalFeederProp : Prop
{
	private static PrefabResource Model = new PrefabResource("Prefabs/Props/Bowl");

	public Character LastFilledBy;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.AnimalFeederProp;
	}

	public override bool CanSetStoragePolicy()
	{
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref LastFilledBy, 360);
	}
}
