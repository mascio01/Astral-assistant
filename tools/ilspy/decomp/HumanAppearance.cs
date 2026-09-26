using System;
using System.Collections.Generic;
using UMA;
using UnityEngine;

public class HumanAppearance : CharacterAppearance
{
	public class BoneSettings : IReflectable
	{
		public float Fatness = 0.5f;

		public float Muscle = 0.5f;

		public float Tallness = 0.5f;

		public float BreastSize = 0.5f;

		public float Pregnancy;

		public float EarsSize = 0.5f;

		public float EarsRotation = 0.5f;

		public float NoseSize = 0.5f;

		public float NoseCurve = 0.5f;

		public float NoseWidth = 0.5f;

		public float NoseInclination = 0.5f;

		public float NosePosition = 0.5f;

		public float NosePronounced = 0.5f;

		public float NoseFlatten = 0.5f;

		public float ChinSize = 0.5f;

		public float ChinPronounced = 0.5f;

		public float ChinPosition = 0.5f;

		public float JawsSize = 0.5f;

		public float JawsPosition = 0.5f;

		public float CheekSize = 0.5f;

		public float CheekPosition = 0.5f;

		public float LowCheekPronounced = 0.5f;

		public float LowCheekPosition = 0.5f;

		public float ForeheadSize = 0.5f;

		public float ForeheadPosition = 0.5f;

		public float LipsSize = 0.5f;

		public float MouthSize = 0.5f;

		public float EyeRotation = 0.5f;

		public float EyeSize = 0.5f;

		public float EyeSpacing = 0.5f;

		public void Reflect(Reflector reflector)
		{
			reflector.Add(ref Fatness);
			reflector.Add(ref Muscle);
			reflector.Add(ref Tallness);
			reflector.Add(ref BreastSize);
			reflector.AddAfter(ref Pregnancy, 483);
			reflector.Add(ref EarsSize);
			reflector.Add(ref EarsRotation);
			reflector.Add(ref NoseSize);
			reflector.Add(ref NoseCurve);
			reflector.Add(ref NoseWidth);
			reflector.Add(ref NoseInclination);
			reflector.Add(ref NosePosition);
			reflector.Add(ref NosePronounced);
			reflector.Add(ref NoseFlatten);
			reflector.Add(ref ChinSize);
			reflector.Add(ref ChinPronounced);
			reflector.Add(ref ChinPosition);
			reflector.Add(ref JawsSize);
			reflector.Add(ref JawsPosition);
			reflector.Add(ref CheekSize);
			reflector.Add(ref CheekPosition);
			reflector.Add(ref LowCheekPronounced);
			reflector.Add(ref LowCheekPosition);
			reflector.Add(ref ForeheadSize);
			reflector.Add(ref ForeheadPosition);
			reflector.Add(ref LipsSize);
			reflector.Add(ref MouthSize);
			reflector.Add(ref EyeRotation);
			reflector.Add(ref EyeSize);
			reflector.Add(ref EyeSpacing);
		}
	}

	public static string[] BodyTypeNames = StringUtil.GetEnumNames<BodyType>();

	public static string[] FaceTypeNames = StringUtil.GetEnumNames<FaceType>();

	public static string[] HairTypeNames = StringUtil.GetEnumNames<HairType>();

	public static string[] FacialHairTypeNames = StringUtil.GetEnumNames<FacialHairType>();

	public BodyType BodyType;

	public FaceType FaceType;

	public HairType HairType;

	public FacialHairType FacialHairType;

	public float SkinColorIndex;

	public float EyeColorIndex;

	public float HairColorIndex;

	public float ZombieDecayAmount;

	public Color32 FrecklesColor;

	public Color32 HairBandColor;

	public Color32 UnderwearColor;

	public BoneSettings Bones = new BoneSettings();

	public UMADnaCustom UmaDnaHumanoid = new UMADnaCustom();

	public static float UMAUnscaledHeight = 2f;

	public static float UMAUnscaledEyeHeight = 1.86f;

	public static float UMAUnscaledGunHeight = 1.6f;

	public static float UMAUnscaledThrowHeight = 2f;

	public static float UMAUnscaledKneeHeight = 0.6f;

	public static float UMAUnscaledHipsHeight = 1f;

	public static float UMAUnscaledCrouchingHeight = 1.2f;

	public static float UMAUnscaledCrouchingEyeHeight = 1.1f;

	public static float UMAUnscaledCrouchingGunHeight = 1f;

	public static float UMAUnscaledCrouchingThrowHeight = 1.6f;

	public static float MinAge = 18f;

	public static float MaxAge = 80f;

	private static List<BodyType> TempBodyTypes = new List<BodyType>();

	private static List<FaceType> TempFaceTypes = new List<FaceType>();

	private static List<HairType> TempHairTypes = new List<HairType>();

	public static Color32[] HairColorSpectrum = new Color32[4]
	{
		new Color32(0, 0, 0, byte.MaxValue),
		new Color32(130, 92, 52, byte.MaxValue),
		new Color32(192, 72, 0, byte.MaxValue),
		new Color32(228, 199, 122, byte.MaxValue)
	};

	private const float NonEuropeanSkinToneStartIndex = 5f;

	private const float AfricanSkinToneStartIndex = 9f;

	public static Color32[] HairBandColors = new Color32[12]
	{
		new Color32(0, 0, 0, byte.MaxValue),
		new Color32(219, 64, 140, byte.MaxValue),
		new Color32(64, 144, 219, byte.MaxValue),
		new Color32(230, 104, 49, byte.MaxValue),
		new Color32(32, 49, 97, byte.MaxValue),
		new Color32(192, 210, 95, byte.MaxValue),
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(249, 59, 59, byte.MaxValue),
		new Color32(byte.MaxValue, 154, 211, byte.MaxValue),
		new Color32(194, 121, byte.MaxValue, byte.MaxValue),
		new Color32(byte.MaxValue, 248, 128, byte.MaxValue),
		new Color32(151, 114, 56, byte.MaxValue)
	};

	public static Color32[] SkinColorSpectrum = new Color32[11]
	{
		new Color32(224, 159, 154, byte.MaxValue),
		new Color32(221, 175, 146, byte.MaxValue),
		new Color32(219, 165, 119, byte.MaxValue),
		new Color32(210, 155, 125, byte.MaxValue),
		new Color32(222, 146, 120, byte.MaxValue),
		new Color32(179, 113, 63, byte.MaxValue),
		new Color32(170, 116, 82, byte.MaxValue),
		new Color32(168, 95, 62, byte.MaxValue),
		new Color32(115, 66, 34, byte.MaxValue),
		new Color32(83, 45, 32, byte.MaxValue),
		new Color32(63, 32, 20, byte.MaxValue)
	};

	public static Color32[] ZombieSkinColorSpectrum = new Color32[11]
	{
		new Color32(216, 205, 154, byte.MaxValue),
		new Color32(157, 174, 104, byte.MaxValue),
		new Color32(178, 192, 101, byte.MaxValue),
		new Color32(83, 113, 117, byte.MaxValue),
		new Color32(104, 117, 83, byte.MaxValue),
		new Color32(92, 121, 121, byte.MaxValue),
		new Color32(83, 94, 73, byte.MaxValue),
		new Color32(73, 75, 59, byte.MaxValue),
		new Color32(37, 49, 41, byte.MaxValue),
		new Color32(44, 49, 52, byte.MaxValue),
		new Color32(26, 26, 29, byte.MaxValue)
	};

	public static Color32[] EyeColorSpectrum = new Color32[8]
	{
		new Color32(146, 165, byte.MaxValue, byte.MaxValue),
		new Color32(73, 104, byte.MaxValue, byte.MaxValue),
		new Color32(70, 141, 36, byte.MaxValue),
		new Color32(236, 212, 109, byte.MaxValue),
		new Color32(211, 158, 101, byte.MaxValue),
		new Color32(214, 102, 42, byte.MaxValue),
		new Color32(128, 101, 64, byte.MaxValue),
		new Color32(94, 52, 39, byte.MaxValue)
	};

	public static Color32[] MaleUnderwearColors = new Color32[5]
	{
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(0, 0, 0, byte.MaxValue),
		new Color32(128, 128, 128, byte.MaxValue),
		new Color32(190, 49, 49, byte.MaxValue),
		new Color32(157, 166, 217, byte.MaxValue)
	};

	public static Color32[] FemaleUnderwearColors = new Color32[5]
	{
		new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue),
		new Color32(0, 0, 0, byte.MaxValue),
		new Color32(190, 49, 49, byte.MaxValue),
		new Color32(225, 180, 156, byte.MaxValue),
		new Color32(byte.MaxValue, 154, 199, byte.MaxValue)
	};

	public Color32 EyeColor => Color32.Lerp(EyeColorSpectrum[Math.Min(EyeColorSpectrum.Length - 1, (int)EyeColorIndex)], EyeColorSpectrum[Math.Min(EyeColorSpectrum.Length - 1, (int)EyeColorIndex + 1)], EyeColorIndex - Mathf.Floor(EyeColorIndex));

	public Color32 HairColor => Color32.Lerp(Color32.Lerp(HairColorSpectrum[Math.Min(HairColorSpectrum.Length - 1, (int)HairColorIndex)], HairColorSpectrum[Math.Min(HairColorSpectrum.Length - 1, (int)HairColorIndex + 1)], HairColorIndex - Mathf.Floor(HairColorIndex)), new Color32(192, 192, 192, byte.MaxValue), Math.Min(1f, ZombieDecayAmount * 0.5f + WrinklesAmount));

	public float WrinklesAmount => Mathf.Clamp01((Age - 45f) / 25f);

	public static float MaleDefaultHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledHeight;

	public static float MaleDefaultEyeHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledEyeHeight;

	public static float MaleDefaultGunHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledGunHeight;

	public static float MaleDefaultCrouchingGunHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledCrouchingGunHeight;

	public static float MaleDefaultThrowHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledThrowHeight;

	public static float MaleDefaultKneeHeight => HumanMaleSkeletonSetup.DefaultOverallScale * UMAUnscaledKneeHeight;

	public static float FemaleDefaultEyeHeight => HumanFemaleSkeletonSetup.DefaultOverallScale * UMAUnscaledEyeHeight;

	public override float OverallScale
	{
		get
		{
			if (Gender != GenderType.Male)
			{
				return HumanFemaleSkeletonSetup.CalcOverallScale(UmaDnaHumanoid);
			}
			return HumanMaleSkeletonSetup.CalcOverallScale(UmaDnaHumanoid);
		}
	}

	public override float Height => OverallScale * UMAUnscaledHeight;

	public override float EyeHeight => OverallScale * UMAUnscaledEyeHeight;

	public override float GunHeight => OverallScale * UMAUnscaledGunHeight;

	public override float ThrowHeight => OverallScale * UMAUnscaledThrowHeight;

	public override float KneeHeight => OverallScale * UMAUnscaledKneeHeight;

	public override float CrouchingHeight => OverallScale * UMAUnscaledCrouchingHeight;

	public override float CrouchingEyeHeight => OverallScale * UMAUnscaledCrouchingEyeHeight;

	public override float CrouchingGunHeight => OverallScale * UMAUnscaledCrouchingGunHeight;

	public override float CrouchingThrowHeight => OverallScale * UMAUnscaledCrouchingThrowHeight;

	public override float GetWeight()
	{
		return ((Gender == GenderType.Female) ? Mathf.Lerp(115f, 159f, (Height - 1.4732f) / 0.3556f) : Mathf.Lerp(134f, 179f, (Height - 1.5494f) / 0.3556f)) + (Bones.Fatness - 0.5f + (Bones.Muscle - 0.5f)) * 50f + Bones.Pregnancy * 30f;
	}

	public HumanAppearance()
	{
	}

	public HumanAppearance(GenderType gender, float age)
		: base(gender, age)
	{
		HairBandColor.a = byte.MaxValue;
		UnderwearColor.a = byte.MaxValue;
	}

	public void SetupDNA()
	{
		float t = Mathf.Clamp01(Age / 18f);
		float num = Mathf.Lerp(0.25f, 0.5f, t);
		UmaDnaHumanoid.height = Mathf.Lerp(num * 0.9f, num * 1.1f, Bones.Tallness);
		UmaDnaHumanoid.headSize = Mathf.Lerp(num * 1.05f, num * 0.95f, Bones.Tallness);
		UmaDnaHumanoid.upperWeight = Mathf.Lerp(0.25f, 0.75f, Bones.Fatness);
		UmaDnaHumanoid.lowerWeight = Mathf.Lerp(0.4f, 0.6f, Bones.Fatness + Bones.Pregnancy * 0.25f);
		UmaDnaHumanoid.gluteusSize = Bones.Fatness;
		UmaDnaHumanoid.belly = Mathf.Lerp(Mathf.Lerp(0.25f, 0.75f, Bones.Fatness), 1.25f, Bones.Pregnancy);
		float t2 = (Bones.Muscle + Bones.Fatness) * 0.5f;
		UmaDnaHumanoid.neckThickness = Mathf.Lerp(0.25f, 0.75f, t2);
		UmaDnaHumanoid.waist = Mathf.Lerp(0.25f, 0.75f, t2);
		UmaDnaHumanoid.armWidth = Mathf.Clamp(Bones.Muscle + Bones.Fatness, 0.25f, 1f);
		UmaDnaHumanoid.forearmWidth = Mathf.Clamp(Bones.Muscle + Bones.Fatness, 0.25f, 1f);
		UmaDnaHumanoid.upperMuscle = Mathf.Lerp(0.25f, 0.6f, Bones.Muscle);
		UmaDnaHumanoid.lowerMuscle = Mathf.Lerp(0.4f, 0.55f, Bones.Muscle);
		UmaDnaHumanoid.breastSize = ((Gender == GenderType.Male) ? Bones.Fatness : (Bones.BreastSize * Mathf.Clamp01((Age - 10f) / 6f) + Bones.Pregnancy * 0.5f));
		UmaDnaHumanoid.earsSize = Mathf.Lerp(0.4f, 0.6f, Bones.EarsSize);
		UmaDnaHumanoid.earsRotation = ((Bones.EarsRotation <= 0.5f) ? (0.4f + Bones.EarsRotation * 2f * 0.1f) : (0.5f + MathUtil.Squared((Bones.EarsRotation - 0.5f) * 2f) * 0.5f));
		UmaDnaHumanoid.noseSize = Mathf.Lerp(0.25f, 0.75f, Bones.NoseSize);
		UmaDnaHumanoid.noseCurve = Mathf.Lerp(0.25f, 0.75f, Bones.NoseCurve);
		UmaDnaHumanoid.noseWidth = Mathf.Lerp(0.25f, 0.75f, Bones.NoseWidth);
		UmaDnaHumanoid.noseInclination = Mathf.Lerp(0.25f, 0.75f, Bones.NoseInclination);
		UmaDnaHumanoid.nosePosition = Mathf.Lerp(0.25f, 0.75f, Bones.NosePosition);
		UmaDnaHumanoid.nosePronounced = Mathf.Lerp(0.25f, 0.75f, Bones.NosePronounced);
		UmaDnaHumanoid.noseFlatten = Mathf.Lerp(0.25f, 0.75f, Bones.NoseFlatten);
		UmaDnaHumanoid.chinSize = Mathf.Lerp(0.4f, 0.6f, Bones.ChinSize);
		UmaDnaHumanoid.chinPronounced = Mathf.Lerp(0.4f, 0.6f, Bones.ChinPronounced);
		UmaDnaHumanoid.chinPosition = Bones.ChinPosition;
		UmaDnaHumanoid.jawsSize = Mathf.Lerp(0.25f, 0.75f, Bones.JawsSize);
		UmaDnaHumanoid.jawsPosition = Mathf.Lerp(0.25f, 0.75f, Bones.JawsPosition);
		UmaDnaHumanoid.cheekSize = Bones.CheekSize * 0.75f;
		UmaDnaHumanoid.cheekPosition = Bones.CheekPosition;
		UmaDnaHumanoid.lowCheekPronounced = Mathf.Lerp(0.4f, 0.6f, Bones.LowCheekPronounced);
		UmaDnaHumanoid.lowCheekPosition = Bones.LowCheekPosition;
		UmaDnaHumanoid.foreheadSize = Mathf.Lerp(0.15f, 0.85f, Bones.ForeheadSize);
		UmaDnaHumanoid.foreheadPosition = Mathf.Lerp(0.25f, 0.75f, Bones.ForeheadPosition);
		UmaDnaHumanoid.lipsSize = Bones.LipsSize;
		UmaDnaHumanoid.mouthSize = Bones.MouthSize;
		UmaDnaHumanoid.eyeRotation = Bones.EyeRotation;
		UmaDnaHumanoid.eyeSize = Mathf.Lerp(0.4f, 0.6f, Bones.EyeSize);
		UmaDnaHumanoid.eyeSpacing = Mathf.Lerp(0.45f, 0.55f, Bones.EyeSpacing);
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref BodyType);
		reflector.Add(ref FaceType);
		reflector.Add(ref HairType);
		reflector.Add(ref FacialHairType);
		reflector.Add(ref SkinColorIndex);
		reflector.Add(ref EyeColorIndex);
		reflector.Add(ref HairColorIndex);
		reflector.AddAfter(ref FrecklesColor, 534);
		reflector.Add(ref HairBandColor);
		reflector.Add(ref UnderwearColor);
		Bones.Reflect(reflector);
		if (reflector.IsDeserialising)
		{
			SetupDNA();
		}
	}

	public static float GetNormallyDistributedRand(CustomRandom rand)
	{
		double num = 0.0;
		for (int i = 0; i < 12; i++)
		{
			num += rand.NextDouble();
		}
		num /= 12.0;
		return (float)num;
	}

	public static float PickRandomAge(CustomRandom rand)
	{
		double num = -2.0;
		for (int i = 0; i < 4; i++)
		{
			num += rand.NextDouble();
		}
		num /= 2.0;
		return Mathf.Lerp(MinAge, MaxAge, Mathf.Abs((float)num));
	}

	public void Randomize(InfectionType infectionType, CustomRandom rand)
	{
		Randomize(infectionType, rand, faceOnly: false);
	}

	public void Randomize(InfectionType infectionType, CustomRandom rand, bool faceOnly)
	{
		if (!faceOnly)
		{
			BodyType = PickRandomBodyType(infectionType, Gender, rand);
			SkinColorIndex = PickRandomSkinColor(rand);
			EyeColorIndex = PickRandomEyeColor(rand, SkinColorIndex);
			HairColorIndex = PickRandomHairColorIndex(rand, SkinColorIndex);
			HairBandColor = PickRandomHairBandColor(rand);
			HairType = PickRandomHairType(Gender, Age, rand);
			FacialHairType = PickRandomFacialHairType(Gender, Age, rand);
			FrecklesColor.a = (byte)((!(SkinColorIndex >= 5f)) ? ((byte)(rand.RandomFloat() * 255f)) : 0);
			UnderwearColor = PickRandomUnderwearColor(Gender, rand);
			ZombieDecayAmount = ((infectionType != InfectionType.None) ? rand.RandomFloat() : 0f);
			Bones.Fatness = GetNormallyDistributedRand(rand);
			Bones.Muscle = GetNormallyDistributedRand(rand);
			Bones.Tallness = GetNormallyDistributedRand(rand);
			Bones.BreastSize = rand.RandomFloat();
		}
		FaceType = PickRandomFaceType(infectionType, Gender, Age, rand);
		Bones.EarsSize = rand.RandomFloat();
		Bones.EarsRotation = rand.RandomFloat();
		Bones.NoseSize = rand.RandomFloat();
		Bones.NoseCurve = rand.RandomFloat();
		Bones.NoseWidth = rand.RandomFloat();
		Bones.NoseInclination = rand.RandomFloat();
		Bones.NosePosition = rand.RandomFloat();
		Bones.NosePronounced = rand.RandomFloat();
		Bones.NoseFlatten = rand.RandomFloat();
		Bones.ChinSize = rand.RandomFloat();
		Bones.ChinPronounced = rand.RandomFloat();
		Bones.ChinPosition = rand.RandomFloat();
		Bones.JawsSize = rand.RandomFloat();
		Bones.JawsPosition = rand.RandomFloat();
		Bones.CheekSize = rand.RandomFloat();
		Bones.CheekPosition = rand.RandomFloat();
		Bones.LowCheekPronounced = rand.RandomFloat();
		Bones.LowCheekPosition = rand.RandomFloat();
		Bones.ForeheadSize = rand.RandomFloat();
		Bones.ForeheadPosition = rand.RandomFloat();
		Bones.LipsSize = ((SkinColorIndex < 9f) ? Mathf.Lerp(0f, 0.75f, rand.RandomFloat()) : Mathf.Lerp(0.25f, 1f, rand.RandomFloat()));
		Bones.MouthSize = rand.RandomFloat();
		Bones.EyeRotation = rand.RandomFloat();
		Bones.EyeSize = rand.RandomFloat();
		Bones.EyeSpacing = rand.RandomFloat();
		SetupDNA();
	}

	public static BodyType PickRandomBodyType(InfectionType infection, GenderType gender, CustomRandom rand)
	{
		if (infection != InfectionType.None && rand.RandomChoice(0.1f))
		{
			TempBodyTypes.Clear();
			for (int i = 0; i < BodyTypeNames.Length; i++)
			{
				switch (gender)
				{
				case GenderType.Male:
					if (BodyTypeNames[i].StartsWith("Male"))
					{
						TempBodyTypes.Add((BodyType)i);
					}
					break;
				case GenderType.Female:
					if (BodyTypeNames[i].StartsWith("Female"))
					{
						TempBodyTypes.Add((BodyType)i);
					}
					break;
				}
			}
			if (TempBodyTypes.Count <= 0)
			{
				return BodyType.Normal;
			}
			return TempBodyTypes[rand.Next(TempBodyTypes.Count)];
		}
		return BodyType.Normal;
	}

	public static FaceType PickRandomFaceType(InfectionType infection, GenderType gender, float age, CustomRandom rand)
	{
		TempFaceTypes.Clear();
		for (int i = 0; i < FaceTypeNames.Length; i++)
		{
			switch ((FaceType)i)
			{
			case FaceType.MaleFace01:
				if (age < 45f)
				{
					continue;
				}
				break;
			case FaceType.MaleFace1:
				if (age < 40f)
				{
					continue;
				}
				break;
			case FaceType.MaleFace2:
				if (age < 25f)
				{
					continue;
				}
				break;
			case FaceType.MaleFace4:
				if (age < 30f)
				{
					continue;
				}
				break;
			case FaceType.MaleFace6:
				if (age < 35f)
				{
					continue;
				}
				break;
			case FaceType.MaleFace8:
				if (age < 40f)
				{
					continue;
				}
				break;
			case FaceType.MaleZombieFace3:
			case FaceType.MaleZombieFace4:
				if (infection == InfectionType.None)
				{
					continue;
				}
				break;
			}
			switch (gender)
			{
			case GenderType.Male:
				if (FaceTypeNames[i].StartsWith("Male"))
				{
					TempFaceTypes.Add((FaceType)i);
				}
				break;
			case GenderType.Female:
				if (FaceTypeNames[i].StartsWith("Female"))
				{
					TempFaceTypes.Add((FaceType)i);
				}
				break;
			}
		}
		if (TempFaceTypes.Count <= 0)
		{
			if (gender != GenderType.Male)
			{
				return FaceType.FemaleFace01;
			}
			return FaceType.MaleFace02;
		}
		return TempFaceTypes[rand.Next(TempFaceTypes.Count)];
	}

	public static HairType PickRandomHairType(GenderType gender, float age, CustomRandom rand)
	{
		TempHairTypes.Clear();
		for (int i = 0; i < HairTypeNames.Length; i++)
		{
			HairType hairType = (HairType)i;
			if (hairType != HairType.MaleBalding)
			{
				if (hairType != HairType.FemaleShaved)
				{
					if (hairType == HairType.FemaleZombieHair)
					{
						continue;
					}
				}
				else if (age >= 40f)
				{
					continue;
				}
			}
			else if (age < 40f)
			{
				continue;
			}
			switch (gender)
			{
			case GenderType.Male:
				if (HairTypeNames[i].StartsWith("Male"))
				{
					TempHairTypes.Add((HairType)i);
				}
				break;
			case GenderType.Female:
				if (HairTypeNames[i].StartsWith("Female"))
				{
					TempHairTypes.Add((HairType)i);
				}
				break;
			}
		}
		if (TempHairTypes.Count <= 0)
		{
			return HairType.None;
		}
		return TempHairTypes[rand.Next(TempHairTypes.Count)];
	}

	public static FacialHairType PickRandomFacialHairType(GenderType gender, float age, CustomRandom rand)
	{
		if (gender == GenderType.Male)
		{
			if (age < 16f)
			{
				return FacialHairType.None;
			}
			if (age < 18f)
			{
				return (FacialHairType)rand.Next(2);
			}
			if (age < 21f)
			{
				return (FacialHairType)rand.Next(3);
			}
			if (age < 24f)
			{
				return (FacialHairType)rand.Next(5);
			}
			return (FacialHairType)rand.Next(10);
		}
		return FacialHairType.None;
	}

	public static float PickRandomHairColorIndex(CustomRandom rand, float skinColorIndex)
	{
		if (skinColorIndex >= 5f)
		{
			return 0f;
		}
		return rand.RandomFloat() * (float)(HairColorSpectrum.Length - 1);
	}

	public static Color32 PickRandomHairBandColor(CustomRandom rand)
	{
		return HairBandColors[rand.Next(HairBandColors.Length)];
	}

	public static float PickRandomSkinColor(CustomRandom rand)
	{
		return rand.RandomFloat() * (float)(SkinColorSpectrum.Length - 1);
	}

	public static float PickRandomEyeColor(CustomRandom rand, float skinColorIndex)
	{
		if (skinColorIndex >= 5f)
		{
			return Mathf.Lerp(4f, 7f, rand.RandomFloat());
		}
		if (skinColorIndex >= 9f)
		{
			return Mathf.Lerp(6f, 7f, rand.RandomFloat());
		}
		return rand.RandomFloat() * (float)(EyeColorSpectrum.Length - 1);
	}

	public static Color32 PickRandomUnderwearColor(GenderType gender, CustomRandom rand)
	{
		return gender switch
		{
			GenderType.Male => MaleUnderwearColors[rand.Next() % MaleUnderwearColors.Length], 
			GenderType.Female => FemaleUnderwearColors[rand.Next() % FemaleUnderwearColors.Length], 
			_ => new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), 
		};
	}

	public Color32 GetSkinColor(Character character)
	{
		Color32 a = Color32.Lerp(SkinColorSpectrum[Math.Min(SkinColorSpectrum.Length - 1, (int)SkinColorIndex)], SkinColorSpectrum[Math.Min(SkinColorSpectrum.Length - 1, (int)SkinColorIndex + 1)], SkinColorIndex - Mathf.Floor(SkinColorIndex));
		Color32 b = Color32.Lerp(ZombieSkinColorSpectrum[Math.Min(ZombieSkinColorSpectrum.Length - 1, (int)SkinColorIndex)], ZombieSkinColorSpectrum[Math.Min(ZombieSkinColorSpectrum.Length - 1, (int)SkinColorIndex + 1)], SkinColorIndex - Mathf.Floor(SkinColorIndex));
		float t = Mathf.Clamp01((character.GetInfectionProgression() - Character.InfectionProgressionCriticalLevel) / (1f - Character.InfectionProgressionCriticalLevel));
		return Color32.Lerp(a, b, t);
	}
}
