public abstract class CharacterAppearance : IReflectable
{
	public const string Male = "Male";

	public const string Female = "Female";

	public static string[] GenderTypeNames = StringUtil.GetEnumNames<GenderType>();

	public GenderType Gender;

	public float Age;

	public virtual float OverallScale => 1f;

	public abstract float Height { get; }

	public abstract float EyeHeight { get; }

	public abstract float GunHeight { get; }

	public abstract float ThrowHeight { get; }

	public abstract float KneeHeight { get; }

	public virtual float CrouchingHeight => Height;

	public virtual float CrouchingEyeHeight => EyeHeight;

	public virtual float CrouchingGunHeight => GunHeight;

	public virtual float CrouchingThrowHeight => ThrowHeight;

	public CharacterAppearance()
	{
	}

	public CharacterAppearance(GenderType gender, float age)
	{
		Gender = gender;
		Age = age;
	}

	public virtual void Reflect(Reflector reflector)
	{
		reflector.Add(ref Gender);
		reflector.Add(ref Age);
	}

	public abstract float GetWeight();
}
