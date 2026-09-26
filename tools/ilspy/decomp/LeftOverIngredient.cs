public class LeftOverIngredient : IReflectable
{
	public EquipmentPrototype Prototype;

	public float Amount;

	public static LeftOverIngredient CreateItem(EquipmentPrototype proto, float amount)
	{
		return new LeftOverIngredient
		{
			Prototype = proto,
			Amount = amount
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Prototype);
		reflector.Add(ref Amount);
	}
}
