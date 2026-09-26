public class UsedIngredient : IReflectable
{
	public EquipmentPrototype Prototype;

	public int Amount;

	public LiquidPrototype LiquidPrototype;

	public float LiquidAmount;

	public static UsedIngredient CreateItem(EquipmentPrototype proto, int amount)
	{
		return new UsedIngredient
		{
			Prototype = proto,
			Amount = amount
		};
	}

	public static UsedIngredient CreateLiquid(LiquidPrototype liquidType, float amount)
	{
		UsedIngredient obj = new UsedIngredient
		{
			LiquidPrototype = liquidType
		};
		obj.LiquidAmount += amount;
		return obj;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Prototype);
		reflector.Add(ref Amount);
		reflector.Add(ref LiquidPrototype);
		reflector.Add(ref LiquidAmount);
	}
}
