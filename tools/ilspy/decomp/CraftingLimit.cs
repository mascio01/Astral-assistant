public struct CraftingLimit : IReflectable
{
	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public int Limit;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Proto);
		reflector.Add(ref Liquid);
		reflector.Add(ref Limit);
	}
}
