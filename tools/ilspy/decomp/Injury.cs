using System;
using UnityEngine;

public struct Injury : IReflectable
{
	public static string[] InfectionTypeNames = StringUtil.GetEnumNames<InfectionType>();

	public static string[] InvisibleStrainTypeNames = StringUtil.GetEnumNames<InvisibleStrainType>();

	public static string[] InjuryLocationNames = StringUtil.GetEnumNames<InjuryLocation>();

	public static string[] InjuryTypeNames = StringUtil.GetEnumNames<InjuryType>("Invalid");

	public int InjuryId;

	public InjuryType Type;

	public InjuryLocation Location;

	public InfectionType InfectionType;

	public InfectionType OriginalInfectionType;

	public Bone Bone;

	public Vector3 PosInBoneSpace;

	public Vector3 DirInBoneSpace;

	public Matrix4x4 InvDecalTransform;

	public int DecalTexIndex;

	public float DamageAmount;

	public bool Ragdolled;

	public bool Burning;

	public bool ArrowStuck;

	public bool AbsorbedByVest;

	public TimeSpan InjuryTime;

	public TimeSpan BandagedHealTime;

	public int BandagedSkillLevel;

	public Character Attacker;

	public bool AssailantUnknown;

	public bool Intentional;

	public SecrecyMode Secrecy;

	public SparringType FromSparring;

	public static float DecalScale = 0.25f;

	public static int NUM_DECALS_IN_TEX = 4;

	private static string RootSlash = "Root/";

	public static float InfectionProgressionRateGreenMin = 1f / (Sun.DayLengthSecs * 7f);

	public static float InfectionProgressionRateGreenMax = 1f / (Sun.DayLengthSecs * 14f);

	public static float InfectionProgressionRateBlueMin = 1f / (Sun.DayLengthSecs * 1f);

	public static float InfectionProgressionRateBlueMax = 1f / (Sun.DayLengthSecs * 3f);

	public static float InfectionProgressionRateRedMin = 1f / 60f;

	public static float InfectionProgressionRateRedMax = 1f / Sun.DayLengthSecs;

	public static float InfectionProgressionRateWhiteMin = 1f / 3f;

	public static float InfectionProgressionRateWhiteMax = 0.0033333334f;

	public bool Bandaged => BandagedSkillLevel != -1;

	public Injury(Character character, InjuryType type, bool absorbedByVest, InjuryLocation location, InfectionType infectionType, Bone bone, Vector3 posInBoneSpace, float damageAmount, TimeSpan injuryTime, Character attacker, bool assailantUnknown, bool intentional)
	{
		if (character.IsPredicted())
		{
			InjuryId = PredictedObjectManager.Instance.PredictedNextFreeInjuryId++;
		}
		else
		{
			InjuryId = Session.Instance.NextFreeInjuryId++;
		}
		Type = type;
		Location = location;
		Bone = bone;
		PosInBoneSpace = posInBoneSpace;
		OriginalInfectionType = (InfectionType = infectionType);
		DamageAmount = damageAmount;
		Ragdolled = false;
		Burning = type == InjuryType.Fire;
		ArrowStuck = type == InjuryType.Arrow;
		AbsorbedByVest = absorbedByVest;
		InjuryTime = injuryTime;
		BandagedHealTime = TimeSpan.Zero;
		BandagedSkillLevel = -1;
		Attacker = attacker;
		AssailantUnknown = assailantUnknown;
		Intentional = intentional;
		Secrecy = SecrecyMode.Public;
		FromSparring = SparringType.None;
		InvDecalTransform = Matrix4x4.identity;
		DirInBoneSpace = Vector3.forward;
		DecalTexIndex = 0;
		SetupDecalEffect(character);
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref InjuryId);
		reflector.Add(ref Type);
		reflector.Add(ref Location);
		reflector.Add(ref InfectionType);
		if (reflector.Version >= 396)
		{
			reflector.Add(ref OriginalInfectionType);
		}
		else
		{
			OriginalInfectionType = InfectionType;
		}
		if (!reflector.IsDoingNetworkChecksum)
		{
			reflector.Add(ref Bone);
			reflector.Add(ref PosInBoneSpace);
		}
		reflector.Add(ref DamageAmount);
		reflector.Add(ref Burning);
		reflector.Add(ref ArrowStuck);
		reflector.AddAfter(ref AbsorbedByVest, 28);
		reflector.Add(ref Ragdolled);
		reflector.Add(ref InjuryTime);
		reflector.Add(ref BandagedHealTime);
		reflector.Add(ref BandagedSkillLevel);
		reflector.AddAfter(ref Attacker, 230);
		reflector.AddAfter(ref AssailantUnknown, 310);
		reflector.AddAfter(ref Intentional, 557);
		reflector.AddAfter(ref Secrecy, 338);
		reflector.AddAfter(ref FromSparring, 394);
		if (reflector.IsDeserialising)
		{
			InvDecalTransform = Matrix4x4.identity;
			DirInBoneSpace = Vector3.forward;
			DecalTexIndex = 0;
		}
	}

	public void SetupDecalEffect(Character character)
	{
		if (GetDecalType() != DecalType.None)
		{
			float angle = MathUtil.RandomFloat((float)InjuryTime.TotalSeconds) * (MathF.PI * 2f);
			Matrix4x4 m = BuildCharacterDecalMatrix(character, Bone, PosInBoneSpace, DecalScale, angle, out DirInBoneSpace);
			InvDecalTransform = Matrix4x4.Inverse(m);
			DecalTexIndex = MathUtil.RandomInt((int)InjuryTime.Ticks, NUM_DECALS_IN_TEX);
		}
	}

	public static Matrix4x4 BuildCharacterDecalMatrix(Character character, Bone bone, Vector3 posInBoneSpace, float scale, float angle, out Vector3 dirInBoneSpace)
	{
		Character predictedOrElseThisCharacter = character.GetPredictedOrElseThisCharacter();
		if (predictedOrElseThisCharacter.Unity.Prefab == null || predictedOrElseThisCharacter.Unity.Prefab.GetAsset() == null)
		{
			dirInBoneSpace = Vector3.forward;
			return Matrix4x4.identity;
		}
		GameObject obj = predictedOrElseThisCharacter.Unity.Prefab;
		string text = character.GetUnityBoneName(bone);
		string text2 = null;
		for (Bone bone2 = bone + 1; bone2 < Bone.Count; bone2++)
		{
			if (text2 != null)
			{
				break;
			}
			text2 = character.GetUnityBoneName(bone2);
		}
		if (character.GetBaseObjectType() == BaseObjectType.Human)
		{
			text = text.Replace(RootSlash, string.Empty);
			text2 = text2.Replace(RootSlash, string.Empty);
			obj = obj.FindChild((character.Appearance.Gender == GenderType.Male) ? Human.UMA_Male_Rig : Human.UMA_Female_Rig);
		}
		GameObject gameObject = obj.FindChild(text);
		GameObject gameObject2 = obj.FindChild(text2);
		if (gameObject == null)
		{
			dirInBoneSpace = Vector3.forward;
			return Matrix4x4.identity;
		}
		Matrix4x4 localToWorldMatrix = gameObject.transform.localToWorldMatrix;
		Vector3 vector = localToWorldMatrix.MultiplyPoint(posInBoneSpace);
		if (bone == Bone.Spine1 && vector.z > 1.4f)
		{
			bone = ((!(vector.x > 0f)) ? Bone.LeftShoulder : Bone.RightShoulder);
		}
		Vector3 vector2;
		if (bone == Bone.Head)
		{
			vector2 = new Vector3(0f, -0.05f, 1.62f);
		}
		else
		{
			Vector3 dir = localToWorldMatrix.Up();
			if (gameObject2 != null)
			{
				dir = MathUtil.SafeNormalize(gameObject2.transform.localToWorldMatrix.Translation() - localToWorldMatrix.Translation(), localToWorldMatrix.Up());
			}
			vector2 = MathUtil.GetClosestPointOnRayToPoint(localToWorldMatrix.Translation(), dir, vector);
		}
		Vector3 vector3 = MathUtil.SafeNormalize(vector2 - vector, Vector3.up);
		dirInBoneSpace = gameObject.transform.worldToLocalMatrix.MultiplyVector(vector3);
		Vector3 rhs = Vector3.forward;
		if (Math.Abs(Vector3.Dot(vector3, rhs)) >= 0.99f)
		{
			rhs = Vector3.right;
		}
		Vector3 vector4 = Vector3.Normalize(Vector3.Cross(vector3, rhs));
		rhs = Vector3.Normalize(Vector3.Cross(vector4, vector3));
		Matrix4x4 matrix4x = MathUtil.CreateFromAxisAngle(vector3, angle);
		vector4 = matrix4x.MultiplyVector(vector4);
		rhs = matrix4x.MultiplyVector(rhs);
		vector4 *= scale;
		rhs *= scale;
		vector3 *= scale;
		Matrix4x4 mat = Matrix4x4.identity;
		MathUtil.SetTranslation(ref mat, vector - vector4 * 0.5f - rhs * 0.5f);
		MathUtil.SetRight(ref mat, vector4);
		MathUtil.SetForward(ref mat, vector3);
		MathUtil.SetUp(ref mat, rhs);
		return mat;
	}

	public DecalType GetDecalType()
	{
		switch (Type)
		{
		case InjuryType.Bullet:
		case InjuryType.Arrow:
		case InjuryType.ZombieBite:
		case InjuryType.SharpObject:
		case InjuryType.BluntObject:
		case InjuryType.Punch:
			if (!AbsorbedByVest)
			{
				if (!Bandaged)
				{
					return DecalType.Blood;
				}
				return DecalType.None;
			}
			return DecalType.ArmorCrack;
		default:
			return DecalType.None;
		}
	}

	public bool ShouldBleed()
	{
		switch (Type)
		{
		case InjuryType.Fire:
		case InjuryType.BluntObject:
		case InjuryType.Swallowed:
		case InjuryType.Punch:
			return false;
		default:
			return true;
		}
	}

	public Injury ApplyAntigen(InfectionType infectionType)
	{
		if (InfectionType == infectionType)
		{
			InfectionType = InfectionType.None;
		}
		return this;
	}

	public static TimeSpan GetHealTimeAtSkillLevel(int skillLevel)
	{
		return new TimeSpan(Sun.DayLength.Ticks * (6 - skillLevel));
	}

	public Injury ApplyBandage(int skillLevel, TimeSpan curTime, InfectionType infectedWith)
	{
		float num = 0f;
		if (Bandaged)
		{
			num = 1f - (float)((BandagedHealTime - curTime).TotalSeconds / GetHealTimeAtSkillLevel(BandagedSkillLevel).TotalSeconds);
		}
		BandagedSkillLevel = skillLevel;
		BandagedHealTime = curTime + TimeSpan.FromSeconds((double)(1f - num) * GetHealTimeAtSkillLevel(BandagedSkillLevel).TotalSeconds);
		InfectionType = (InfectionType)Math.Max((int)InfectionType, (int)infectedWith);
		ArrowStuck = false;
		Burning = false;
		return this;
	}

	public Injury Burnout()
	{
		Burning = false;
		return this;
	}

	public Injury ApplyRagdoll()
	{
		Ragdolled = true;
		return this;
	}

	public float GetInfectionProgressionRate(Character character, ref int constitution)
	{
		if (InfectionType == InfectionType.None || character.GetBaseObjectType() != BaseObjectType.Human)
		{
			return 0f;
		}
		if (constitution == -1)
		{
			constitution = character.GetSkillLevelWithEffects(SkillType.Constitution);
		}
		return InfectionType switch
		{
			InfectionType.Green => Mathf.Lerp(InfectionProgressionRateGreenMin, InfectionProgressionRateGreenMax, (float)constitution / 5f), 
			InfectionType.Blue => Mathf.Lerp(InfectionProgressionRateBlueMin, InfectionProgressionRateBlueMax, (float)constitution / 5f), 
			InfectionType.Red => Mathf.Lerp(InfectionProgressionRateRedMin, InfectionProgressionRateRedMax, (float)constitution / 5f), 
			InfectionType.White => Mathf.Lerp(InfectionProgressionRateWhiteMin, InfectionProgressionRateWhiteMax, (float)constitution / 5f), 
			InfectionType.Invisible => Mathf.Lerp(InfectionProgressionRateWhiteMin, InfectionProgressionRateWhiteMax, (float)constitution / 5f), 
			_ => 0f, 
		};
	}

	public static InjuryLocation GetInjuryLocationFromBone(Bone bone)
	{
		switch (bone)
		{
		case Bone.Head:
			return InjuryLocation.Head;
		case Bone.LeftShoulder:
		case Bone.LeftArm:
		case Bone.LeftForearm:
		case Bone.LeftHand:
			return InjuryLocation.LeftArm;
		case Bone.RightShoulder:
		case Bone.RightArm:
		case Bone.RightForearm:
		case Bone.RightHand:
			return InjuryLocation.RightArm;
		case Bone.LeftFoot:
			return InjuryLocation.LeftLeg;
		case Bone.RightFoot:
			return InjuryLocation.RightLeg;
		default:
			return InjuryLocation.Torso;
		}
	}

	public bool IsLegs()
	{
		InjuryLocation location = Location;
		if ((uint)(location - 4) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsArms()
	{
		InjuryLocation location = Location;
		if ((uint)(location - 2) <= 1u)
		{
			return true;
		}
		return false;
	}
}
