public struct StoragePolicy : IReflectable
{
	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public StoragePolicy(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		Proto = proto;
		Liquid = liquid;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Proto);
		reflector.Add(ref Liquid);
	}

	public bool Matches(GatheredItem gatheredItem)
	{
		if (gatheredItem.Liquid == null)
		{
			return gatheredItem.Type == Proto;
		}
		return gatheredItem.Liquid == Liquid;
	}
}
