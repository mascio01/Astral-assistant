public class RabbitAppearance : CharacterAppearance
{
	public override float Height => 0.25f;

	public override float EyeHeight => 0.2f;

	public override float GunHeight => 0.15f;

	public override float ThrowHeight => 0.2f;

	public override float KneeHeight => 0.05f;

	public RabbitAppearance()
	{
	}

	public RabbitAppearance(GenderType gender, float age)
		: base(gender, age)
	{
	}

	public override float GetWeight()
	{
		return 2f;
	}

	public static float PickRandomAge(CustomRandom rand)
	{
		return 1f;
	}
}
