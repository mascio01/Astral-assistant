using UnityEngine;

public class DeerAppearance : CharacterAppearance
{
	public override float Height
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return 1.5f;
			}
			return 2.1f;
		}
	}

	public override float EyeHeight
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return 1.3f;
			}
			return 1.4f;
		}
	}

	public override float GunHeight
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return 0.9f;
			}
			return 1f;
		}
	}

	public override float ThrowHeight
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return 1.3f;
			}
			return 1.4f;
		}
	}

	public override float KneeHeight
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return 0.35f;
			}
			return 0.4f;
		}
	}

	public DeerAppearance()
	{
	}

	public DeerAppearance(GenderType gender, float age)
		: base(gender, age)
	{
	}

	public override float GetWeight()
	{
		if (Gender != GenderType.Male)
		{
			return 144f;
		}
		return 225f;
	}

	public static float PickRandomAge(CustomRandom rand)
	{
		return Mathf.Lerp(1.5f, 5f, rand.RandomFloat());
	}
}
