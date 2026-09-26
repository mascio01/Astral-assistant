using UnityEngine;

public class ChickenAppearance : CharacterAppearance
{
	public ChickenColor ChickenColor;

	public override float Height
	{
		get
		{
			if (!(Age < Chicken.ChickenAdultAge))
			{
				if (Gender != GenderType.Male)
				{
					return 0.6f;
				}
				return 0.75f;
			}
			return 0.16f;
		}
	}

	public override float EyeHeight
	{
		get
		{
			if (!(Age < Chicken.ChickenAdultAge))
			{
				if (Gender != GenderType.Male)
				{
					return 0.54f;
				}
				return 0.65f;
			}
			return 0.14f;
		}
	}

	public override float GunHeight
	{
		get
		{
			if (!(Age < Chicken.ChickenAdultAge))
			{
				if (Gender != GenderType.Male)
				{
					return 0.36f;
				}
				return 0.4f;
			}
			return 0.1f;
		}
	}

	public override float ThrowHeight
	{
		get
		{
			if (!(Age < Chicken.ChickenAdultAge))
			{
				if (Gender != GenderType.Male)
				{
					return 0.54f;
				}
				return 0.65f;
			}
			return 0.14f;
		}
	}

	public override float KneeHeight
	{
		get
		{
			if (!(Age < Chicken.ChickenAdultAge))
			{
				if (Gender != GenderType.Male)
				{
					return 0.1f;
				}
				return 0.1f;
			}
			return 0.03f;
		}
	}

	public ChickenAppearance()
	{
	}

	public ChickenAppearance(GenderType gender, float age)
		: base(gender, age)
	{
	}

	public override float GetWeight()
	{
		if (!(Age < Chicken.ChickenAdultAge))
		{
			if (Gender != GenderType.Male)
			{
				return 8f;
			}
			return 14f;
		}
		return 1f;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref ChickenColor, 307);
	}

	public void Randomize(CustomRandom rand)
	{
		ChickenColor = PickRandomChickenColor(rand);
	}

	public static float PickRandomAge(CustomRandom rand)
	{
		return Mathf.Lerp(0f, 6f, rand.RandomFloat());
	}

	public static ChickenColor PickRandomChickenColor(CustomRandom rand)
	{
		return (ChickenColor)rand.Next(3);
	}
}
