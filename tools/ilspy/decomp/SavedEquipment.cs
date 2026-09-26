public struct SavedEquipment : IReflectable
{
	public EquipmentPrototype Proto;

	public int Amount;

	public int ColorVariation;

	public int ColorVariation2;

	public int ColorVariation3;

	public int MaterialVariation;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Proto);
		reflector.Add(ref Amount);
		reflector.AddAfter(ref ColorVariation, 189);
		reflector.AddAfter(ref ColorVariation2, 189);
		reflector.AddAfter(ref ColorVariation3, 189);
		reflector.AddAfter(ref MaterialVariation, 189);
	}
}
