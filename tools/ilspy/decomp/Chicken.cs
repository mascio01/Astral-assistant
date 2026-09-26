using System;
using System.Text;
using UnityEngine;

public class Chicken : Animal
{
	public static Resource<GameObject>[] ChickenPrefab = new Resource<GameObject>[3];

	public static Resource<RagdollSettings>[] ChickenRagdollSettings = new Resource<RagdollSettings>[3];

	public static Resource<Texture2D>[] ChickenIcon = new Resource<Texture2D>[3];

	public static Resource<Texture2D>[] ChickenIconWhite = new Resource<Texture2D>[3];

	public static HitBoxSettings[] HitBoxSettings = new HitBoxSettings[3];

	public static Resource<Material>[] ChickenMaterial = new Resource<Material>[3];

	public TimeSpan LastCrowTime = Target.Never;

	public TimeSpan LastFertilizedTime = Target.Never;

	public TimeSpan BroodyStartTime = Target.Never;

	public TimeSpan LastAteFromGround = Target.Never;

	public float EggProduction;

	public static float ChickenAdultAge = 1f / 12f;

	public static float NutritionFactor = 4f;

	public static float RoosterDistFromFocus = 1f;

	public static float RoosterZOffset = 0f;

	public static float RoosterYOffset = -0.25f;

	public static float RoosterYOffsetResting = -0.35f;

	public static float HenDistFromFocus = 1f;

	public static float HenZOffset = 0.05f;

	public static float HenYOffset = -0.2f;

	public static float HenYOffsetResting = -0.3f;

	public static float ChickDistFromFocus = 0.4f;

	public static float ChickZOffset = 0.02f;

	public static float ChickYOffset = 0f;

	public static float ChickenCarryRotX = -65.1f;

	public static float ChickenCarryRotY = 135.95f;

	public static float ChickenCarryRotZ = -128.26f;

	public static float ChickenCarryOffsetX = -0.21f;

	public static float ChickenCarryOffsetY = 0.11f;

	public static float ChickenCarryOffsetZ = 0.17f;

	public static float ChickenAwakeCarryRotX = -63.19f;

	public static float ChickenAwakeCarryRotY = -180f;

	public static float ChickenAwakeCarryRotZ = -72.77f;

	public static float ChickenAwakeCarryOffsetX = -0.09f;

	public static float ChickenAwakeCarryOffsetY = 0.04f;

	public static float ChickenAwakeCarryOffsetZ = 0.17f;

	private static int NAME_Rooster = StringUtil.JenkinsHash("NAME_Rooster");

	private static int NAME_Hen = StringUtil.JenkinsHash("NAME_Hen");

	private static int NAME_Chick = StringUtil.JenkinsHash("NAME_Chick");

	private static int MENU_Male = StringUtil.JenkinsHash("MENU_Male");

	private static int MENU_Female = StringUtil.JenkinsHash("MENU_Female");

	public const string ChickenRoot = "Arm_cock";

	public const string ChickenGlobalBone = "Arm_cock/Basic";

	public const string ChickenPositionBone = null;

	public const string ChickenHips = "Arm_cock/Basic/body";

	public const string ChickenLowerBack = null;

	public const string ChickenSpine = "Arm_cock/Basic/body/spine_01";

	public const string ChickenSpine1 = "Arm_cock/Basic/body/spine_01/spine_02";

	public const string ChickenNeck = "Arm_cock/Basic/body/spine_01/spine_02/neck/neck_end";

	public const string ChickenHead = "Arm_cock/Basic/body/head";

	public const string ChickenLeftUpLeg = "Arm_cock/Basic/body/leg1_L";

	public const string ChickenLeftLeg = "Arm_cock/Basic/body/leg1_L/leg2_L";

	public const string ChickenLeftFoot = "Arm_cock/Basic/finger3_L_002";

	public const string ChickenLeftShoulder = "Arm_cock/Basic/body/spine_01/arm_L";

	public const string ChickenLeftArm = null;

	public const string ChickenLeftForearm = "Arm_cock/Basic/body/spine_01/arm_L/hand_L";

	public const string ChickenLeftHand = "Arm_cock/Basic/body/spine_01/arm_L/hand_L/hand_L_end";

	public const string ChickenRightUpLeg = "Arm_cock/Basic/body/leg1_R";

	public const string ChickenRightLeg = "Arm_cock/Basic/body/leg1_R/leg2_R";

	public const string ChickenRightFoot = "Arm_cock/Basic/foot_R";

	public const string ChickenRightShoulder = "Arm_cock/Basic/body/spine_01/arm_R";

	public const string ChickenRightArm = null;

	public const string ChickenRightForearm = "Arm_cock/Basic/body/spine_01/arm_R/hand_R";

	public const string ChickenRightHand = "Arm_cock/Basic/body/spine_01/arm_R/hand_R/hand_R_end";

	public const string ChickRoot = "Arm_chick";

	public const string ChickGlobalBone = "Arm_chick/base2";

	public const string ChickPositionBone = null;

	public const string ChickHips = "Arm_chick/base2/base1";

	public const string ChickLowerBack = null;

	public const string ChickSpine = "Arm_chick/base2/base1/spine1";

	public const string ChickSpine1 = "Arm_chick/base2/base1/spine1";

	public const string ChickNeck = "Arm_chick/base2/base1/Neck";

	public const string ChickHead = "Arm_chick/base2/base1/Neck/head";

	public const string ChickLeftUpLeg = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L";

	public const string ChickLeftLeg = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L/Leg3_L";

	public const string ChickLeftFoot = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L/Leg3_L/Leg3_L_end";

	public const string ChickLeftShoulder = "Arm_chick/base2/base1/wing_L";

	public const string ChickLeftArm = null;

	public const string ChickLeftForearm = null;

	public const string ChickLeftHand = "Arm_chick/base2/base1/wing_L/wing_L_end";

	public const string ChickRightUpLeg = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R";

	public const string ChickRightLeg = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R/Leg3_R";

	public const string ChickRightFoot = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R/Leg3_R/Leg3_R_end";

	public const string ChickRightShoulder = "Arm_chick/base2/base1/wing_R";

	public const string ChickRightArm = null;

	public const string ChickRightForearm = null;

	public const string ChickRightHand = "Arm_chick/base2/base1/wing_R/wing_R_end";

	private static float ChickArrowScale = 25f;

	private static float ChickenArrowScale = 1f;

	public override float MapDotSize => 1.5f;

	public override float Radius => 0.15f;

	public override float VisionConeDeg => 150f;

	public static void LoadChickenContent()
	{
		ChickenPrefab[0] = new Resource<GameObject>("Prefabs/Animals/Chicken/Rooster");
		ChickenPrefab[1] = new Resource<GameObject>("Prefabs/Animals/Chicken/Hen");
		ChickenPrefab[2] = new Resource<GameObject>("Prefabs/Animals/Chicken/Chick");
		ChickenRagdollSettings[0] = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_ChickenRooster");
		ChickenRagdollSettings[1] = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_ChickenHen");
		ChickenRagdollSettings[2] = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_ChickenChick");
		ChickenIcon[0] = new Resource<Texture2D>("Textures/BuildingIcons/ChickenRooster");
		ChickenIcon[1] = new Resource<Texture2D>("Textures/BuildingIcons/ChickenHen");
		ChickenIcon[2] = new Resource<Texture2D>("Textures/BuildingIcons/ChickenChick");
		ChickenIconWhite[0] = new Resource<Texture2D>("Textures/BuildingIcons/ChickenRoosterWhite");
		ChickenIconWhite[1] = new Resource<Texture2D>("Textures/BuildingIcons/ChickenHenWhite");
		ChickenIconWhite[2] = ChickenIcon[2];
		HitBoxSettings[0] = GameObject.Find("HitBoxes/ChickenRoosterHitBoxSettings").GetComponent<HitBoxSettings>();
		HitBoxSettings[1] = GameObject.Find("HitBoxes/ChickenHenHitBoxSettings").GetComponent<HitBoxSettings>();
		HitBoxSettings[2] = GameObject.Find("HitBoxes/ChickenChickHitBoxSettings").GetComponent<HitBoxSettings>();
		ChickenMaterial[0] = new Resource<Material>("Materials/Animals/Chicken/Chicken_color");
		ChickenMaterial[1] = new Resource<Material>("Materials/Animals/Chicken/Chicken_color2");
		ChickenMaterial[2] = new Resource<Material>("Materials/Animals/Chicken/Chicken_color3");
	}

	public static Chicken Spawn(TerrainCoord tile, float facingAngle, ChickenAppearance appearance)
	{
		Chicken chicken = new Chicken();
		chicken.Appearance = appearance;
		chicken.Position = GameTerrain.Instance.GetTileCentrePos(tile);
		chicken.FacingAngle = facingAngle;
		chicken.OnSpawn();
		return chicken;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Chicken;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref LastCrowTime, 306);
		reflector.AddAfter(ref LastFertilizedTime, 309);
		reflector.AddAfter(ref BroodyStartTime, 316);
		reflector.AddAfter(ref LastAteFromGround, 319);
		reflector.AddAfter(ref EggProduction, 309);
	}

	public ChickenModel GetChickenModel()
	{
		if (Appearance.Age < ChickenAdultAge)
		{
			return ChickenModel.Chick;
		}
		if (Appearance.Gender != GenderType.Male)
		{
			return ChickenModel.Hen;
		}
		return ChickenModel.Rooster;
	}

	public override bool IsYouth()
	{
		return GetChickenModel() == ChickenModel.Chick;
	}

	public override void SetAge(float age)
	{
		ChickenModel chickenModel = GetChickenModel();
		Appearance.Age = age;
		ChickenModel chickenModel2 = GetChickenModel();
		if (chickenModel != chickenModel2)
		{
			bool num = IsUnityObjectActive();
			if (num)
			{
				UnityDeactivate();
			}
			UnityDelete();
			UnityInit();
			if (num)
			{
				UnityActivate();
			}
		}
	}

	public bool IsReadyToLayEgg()
	{
		return EggProduction >= 1f;
	}

	public bool IsFertilized()
	{
		return Session.Instance.PlayTime - LastFertilizedTime <= TimeSpan.FromSeconds(Sun.DayLengthSecs * 3f);
	}

	public bool IsBroody()
	{
		return Session.Instance.PlayTime - BroodyStartTime <= TimeSpan.FromSeconds(Sun.DayLengthSecs * FertilizedEgg.DaysToHatch * 3f);
	}

	protected override void SimulateSurvivalFactors(TimeSpan curTime, float dts)
	{
		base.SimulateSurvivalFactors(curTime, dts);
		if (GetChickenModel() == ChickenModel.Hen && Hunger < Character.HungryTime && !IsBroody())
		{
			EggProduction = Math.Min(1f, EggProduction + dts / Sun.DayLengthSecs * 0.9230769f);
		}
		SetAge(Appearance.Age + dts / (Sun.DayLengthSecs * (float)Weather.DaysInAYear));
	}

	public override Texture2D GetIconResource()
	{
		return (GetAppearance().ChickenColor == ChickenColor.White) ? ChickenIconWhite[(int)GetChickenModel()] : ChickenIcon[(int)GetChickenModel()];
	}

	public override CharacterAppearance CreateAppearance()
	{
		return new ChickenAppearance();
	}

	public ChickenAppearance GetAppearance()
	{
		return (ChickenAppearance)Appearance;
	}

	public override bool DeleteWhenSkinned()
	{
		return true;
	}

	public override EquipmentPrototype GetMeatType()
	{
		return EquipmentPrototype.SkinnedChicken;
	}

	public override int GetMeatAmount(int cookingSkill)
	{
		if (!(SkinnedAmount < 1f) || GetChickenModel() == ChickenModel.Chick)
		{
			return 0;
		}
		return 1;
	}

	public override void Consume(Character character, float amount, InfectionType infectionType)
	{
		base.Consume(character, amount * 10f, infectionType);
	}

	public override float GetMaxInventoryWeightWithoutBackpack()
	{
		return 4f;
	}

	public override bool IsSmallAnimal()
	{
		return true;
	}

	public override float GetNutritionFactor()
	{
		return NutritionFactor;
	}

	public override Bone GetPipBoneToFocusOnWhenDead()
	{
		return Bone.Spine1;
	}

	public override float GetWeaponRangeFactorWhenAimedAtMe()
	{
		return 0.75f;
	}

	public override float GetTargetableRadiusFactor(bool isUsingRangedWeapon)
	{
		if (!isUsingRangedWeapon)
		{
			return 0.5f;
		}
		return 1f;
	}

	public override float GetTargetableAngle()
	{
		return MathF.PI / 4f;
	}

	public override float GetMaxSoundVisibilityRange()
	{
		return 8f;
	}

	public override float GetMaxVisibilityRangeToZombies()
	{
		return 8f;
	}

	public override void GetSightRange(out int fogStart, out int fogEnd)
	{
		fogEnd = 12;
		fogStart = 8;
	}

	public override bool HasSenseOfSmell()
	{
		return false;
	}

	public override float GetWalkSpeed()
	{
		if (GetChickenModel() != ChickenModel.Chick)
		{
			return 1f;
		}
		return 0.5f;
	}

	public override float GetRunSpeed()
	{
		if (GetChickenModel() != ChickenModel.Chick)
		{
			return 6f;
		}
		return 3f;
	}

	public override float GetMaxLimbDamage()
	{
		return 0.01f;
	}

	public override bool CanWalkInRivers()
	{
		return false;
	}

	public override bool CanEatGrass()
	{
		return true;
	}

	public override bool IsAffectedByTemperature()
	{
		return true;
	}

	public override int GetFurInsulation()
	{
		return 6;
	}

	public override float GetPipFocusDist()
	{
		return GetChickenModel() switch
		{
			ChickenModel.Rooster => RoosterDistFromFocus, 
			ChickenModel.Hen => HenDistFromFocus, 
			ChickenModel.Chick => ChickDistFromFocus, 
			_ => base.GetPipFocusDist(), 
		};
	}

	public override float GetPipYOffset()
	{
		switch (GetChickenModel())
		{
		case ChickenModel.Rooster:
			if (CurrentPose != AnimalPose.Resting)
			{
				return RoosterYOffset;
			}
			return RoosterYOffsetResting;
		case ChickenModel.Hen:
			if (CurrentPose != AnimalPose.Resting)
			{
				return HenYOffset;
			}
			return HenYOffsetResting;
		case ChickenModel.Chick:
			return ChickYOffset;
		default:
			return base.GetPipYOffset();
		}
	}

	public override float GetPipZOffset()
	{
		return GetChickenModel() switch
		{
			ChickenModel.Rooster => RoosterZOffset, 
			ChickenModel.Hen => HenZOffset, 
			ChickenModel.Chick => ChickZOffset, 
			_ => base.GetPipZOffset(), 
		};
	}

	public override float GetCarryRotX()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryRotX;
		}
		return ChickenAwakeCarryRotX;
	}

	public override float GetCarryRotY()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryRotY;
		}
		return ChickenAwakeCarryRotY;
	}

	public override float GetCarryRotZ()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryRotZ;
		}
		return ChickenAwakeCarryRotZ;
	}

	public override float GetCarryOffsetX()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryOffsetX;
		}
		return ChickenAwakeCarryOffsetX;
	}

	public override float GetCarryOffsetY()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryOffsetY;
		}
		return ChickenAwakeCarryOffsetY;
	}

	public override float GetCarryOffsetZ()
	{
		if (!base.IsAwake)
		{
			return ChickenCarryOffsetZ;
		}
		return ChickenAwakeCarryOffsetZ;
	}

	public override void PlayIdleSound()
	{
		PlayVoiceSoundFromList((GetChickenModel() == ChickenModel.Chick) ? SoundManager.ChickPeepSounds : SoundManager.ChickenIdleSounds, VoiceSoundType.AnimalIdle);
	}

	public override void PlayDeathSound()
	{
		PlayVoiceSoundFromList((GetChickenModel() == ChickenModel.Chick) ? SoundManager.ChickPeepSounds : SoundManager.ChickenDeathSounds, VoiceSoundType.AnimalDeath);
	}

	public override void PlayAlertSound()
	{
		PlayVoiceSoundFromList((GetChickenModel() == ChickenModel.Chick) ? SoundManager.ChickPeepSounds : SoundManager.ChickenAlertSounds, VoiceSoundType.AnimalAlert);
	}

	public override void PlayFleeSound()
	{
		PlayVoiceSoundFromList((GetChickenModel() == ChickenModel.Chick) ? SoundManager.ChickPeepSounds : SoundManager.ChickenFleeSounds, VoiceSoundType.AnimalFleeing);
	}

	public override bool OnAnimationEvent(AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.RoosterCrow:
			LastCrowTime = Session.Instance.PlayTime;
			PlayVoiceSoundFromList(SoundManager.RoosterCrowSounds, VoiceSoundType.AnimalIdle);
			return true;
		case AnimationEventType.MatingSound:
			PlayVoiceSoundFromList(SoundManager.ChickenMatingSounds, VoiceSoundType.AnimalMating);
			return true;
		default:
			return base.OnAnimationEvent(animEvent);
		}
	}

	public override void OnPickedUpBy(Character character, bool startTransition)
	{
		base.OnPickedUpBy(character, startTransition);
		if (base.IsAwake && character.CheckFrontmostPrediction(PredictedEventType.AlertSound))
		{
			PlayAlertSound();
		}
	}

	public override void OnDroppedBy(Character character)
	{
		base.OnDroppedBy(character);
		if (base.IsConscious && character.CheckFrontmostPrediction(PredictedEventType.FleeSound))
		{
			PlayFleeSound();
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (string.IsNullOrEmpty(FirstName))
		{
			switch (GetChickenModel())
			{
			case ChickenModel.Rooster:
				sb.Append(GameImpl.Translate(NAME_Rooster, englishOnly));
				break;
			case ChickenModel.Hen:
				sb.Append(GameImpl.Translate(NAME_Hen, englishOnly));
				break;
			case ChickenModel.Chick:
				sb.Append(GameImpl.Translate(NAME_Chick, englishOnly));
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate((Appearance.Gender == GenderType.Male) ? MENU_Male : MENU_Female, englishOnly));
				sb.Append(')');
				break;
			}
		}
		else if (GameImpl.WantNamesReversed(Surname, englishOnly))
		{
			sb.Append(GameImpl.TranslateSurname(Surname, Appearance.Gender, englishOnly, SurnameVerified));
			sb.Append(GameImpl.TranslateName(FirstName, englishOnly, FirstNameVerified));
		}
		else
		{
			sb.Append(GameImpl.TranslateName(FirstName, englishOnly, FirstNameVerified));
			if (Surname.Length > 0)
			{
				sb.Append(' ');
				sb.Append(GameImpl.TranslateSurname(Surname, Appearance.Gender, englishOnly, SurnameVerified));
			}
		}
	}

	public override bool IsEnemy(TileObject obj, bool includeJustActivatedInvisibleStrain = false)
	{
		if (obj is Character { Zombie: not false, Alive: not false })
		{
			return true;
		}
		return false;
	}

	public override void UnityInit()
	{
		if (Disappeared)
		{
			return;
		}
		ChickenModel chickenModel = GetChickenModel();
		Resource<RagdollSettings> resource = ChickenRagdollSettings[(int)chickenModel];
		RagdollSettings ragdollSettings = null;
		if (resource != null)
		{
			ragdollSettings = resource;
		}
		UnityInitAnimal(ChickenPrefab[(int)chickenModel], ragdollSettings, (chickenModel == ChickenModel.Chick) ? "Arm_chick/" : "Arm_cock/");
		if (chickenModel != ChickenModel.Chick)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in Unity.SkinnedMeshRenderers)
			{
				skinnedMeshRenderer.gameObject.tag = Human.HairTag;
			}
		}
		base.UnityInit();
	}

	public override void UnitySetupAppearance()
	{
		if (Disappeared)
		{
			return;
		}
		ChickenAppearance appearance = GetAppearance();
		if (!(appearance.Age >= ChickenAdultAge))
		{
			return;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in Unity.SkinnedMeshRenderers)
		{
			skinnedMeshRenderer.sharedMaterial = ChickenMaterial[(int)appearance.ChickenColor];
		}
	}

	public override string GetUnityBoneName(Bone bone)
	{
		string result = null;
		if (GetChickenModel() == ChickenModel.Chick)
		{
			switch (bone)
			{
			case Bone.WorldPos:
				result = string.Empty;
				break;
			case Bone.Root:
				result = "Arm_chick";
				break;
			case Bone.Global:
				result = "Arm_chick/base2";
				break;
			case Bone.Position:
				result = null;
				break;
			case Bone.Hips:
				result = "Arm_chick/base2/base1";
				break;
			case Bone.LowerBack:
				result = null;
				break;
			case Bone.Spine:
				result = "Arm_chick/base2/base1/spine1";
				break;
			case Bone.Spine1:
				result = "Arm_chick/base2/base1/spine1";
				break;
			case Bone.Neck:
				result = "Arm_chick/base2/base1/Neck";
				break;
			case Bone.Head:
				result = "Arm_chick/base2/base1/Neck/head";
				break;
			case Bone.LeftUpLeg:
				result = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L";
				break;
			case Bone.LeftLeg:
				result = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L/Leg3_L";
				break;
			case Bone.LeftFoot:
				result = "Arm_chick/base2/base1/spine1/Leg1_L/Leg2_L/Leg3_L/Leg3_L_end";
				break;
			case Bone.LeftShoulder:
				result = "Arm_chick/base2/base1/wing_L";
				break;
			case Bone.LeftArm:
				result = null;
				break;
			case Bone.LeftForearm:
				result = null;
				break;
			case Bone.LeftHand:
				result = "Arm_chick/base2/base1/wing_L/wing_L_end";
				break;
			case Bone.RightUpLeg:
				result = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R";
				break;
			case Bone.RightLeg:
				result = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R/Leg3_R";
				break;
			case Bone.RightFoot:
				result = "Arm_chick/base2/base1/spine1/Leg1_R/Leg2_R/Leg3_R/Leg3_R_end";
				break;
			case Bone.RightShoulder:
				result = "Arm_chick/base2/base1/wing_R";
				break;
			case Bone.RightArm:
				result = null;
				break;
			case Bone.RightForearm:
				result = null;
				break;
			case Bone.RightHand:
				result = "Arm_chick/base2/base1/wing_R/wing_R_end";
				break;
			case Bone.MeleeWeapon:
				result = null;
				break;
			}
		}
		else
		{
			switch (bone)
			{
			case Bone.WorldPos:
				result = string.Empty;
				break;
			case Bone.Root:
				result = "Arm_cock";
				break;
			case Bone.Global:
				result = "Arm_cock/Basic";
				break;
			case Bone.Position:
				result = null;
				break;
			case Bone.Hips:
				result = "Arm_cock/Basic/body";
				break;
			case Bone.LowerBack:
				result = null;
				break;
			case Bone.Spine:
				result = "Arm_cock/Basic/body/spine_01";
				break;
			case Bone.Spine1:
				result = "Arm_cock/Basic/body/spine_01/spine_02";
				break;
			case Bone.Neck:
				result = "Arm_cock/Basic/body/spine_01/spine_02/neck/neck_end";
				break;
			case Bone.Head:
				result = "Arm_cock/Basic/body/head";
				break;
			case Bone.LeftUpLeg:
				result = "Arm_cock/Basic/body/leg1_L";
				break;
			case Bone.LeftLeg:
				result = "Arm_cock/Basic/body/leg1_L/leg2_L";
				break;
			case Bone.LeftFoot:
				result = "Arm_cock/Basic/finger3_L_002";
				break;
			case Bone.LeftShoulder:
				result = "Arm_cock/Basic/body/spine_01/arm_L";
				break;
			case Bone.LeftArm:
				result = null;
				break;
			case Bone.LeftForearm:
				result = "Arm_cock/Basic/body/spine_01/arm_L/hand_L";
				break;
			case Bone.LeftHand:
				result = "Arm_cock/Basic/body/spine_01/arm_L/hand_L/hand_L_end";
				break;
			case Bone.RightUpLeg:
				result = "Arm_cock/Basic/body/leg1_R";
				break;
			case Bone.RightLeg:
				result = "Arm_cock/Basic/body/leg1_R/leg2_R";
				break;
			case Bone.RightFoot:
				result = "Arm_cock/Basic/foot_R";
				break;
			case Bone.RightShoulder:
				result = "Arm_cock/Basic/body/spine_01/arm_R";
				break;
			case Bone.RightArm:
				result = null;
				break;
			case Bone.RightForearm:
				result = "Arm_cock/Basic/body/spine_01/arm_R/hand_R";
				break;
			case Bone.RightHand:
				result = "Arm_cock/Basic/body/spine_01/arm_R/hand_R/hand_R_end";
				break;
			case Bone.MeleeWeapon:
				result = null;
				break;
			}
		}
		return result;
	}

	public override HitBox[] GetHitBoxes()
	{
		return HitBoxSettings[(int)GetChickenModel()].HitBoxes;
	}

	public override Bone GetPickedUpByBone()
	{
		if (GetChickenModel() != ChickenModel.Chick)
		{
			return Bone.Hips;
		}
		return Bone.Global;
	}

	public override float GetArrowScale()
	{
		if (GetChickenModel() != ChickenModel.Chick)
		{
			return ChickenArrowScale;
		}
		return ChickArrowScale;
	}
}
