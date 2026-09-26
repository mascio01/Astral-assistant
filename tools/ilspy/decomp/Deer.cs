using System;
using System.Text;
using UnityEngine;

public class Deer : Animal
{
	public static Resource<GameObject>[] DeerPrefab = new Resource<GameObject>[2];

	public static Resource<RagdollSettings>[] DeerRagdollSettings = new Resource<RagdollSettings>[2];

	public static Resource<Texture2D>[] DeerIcon = new Resource<Texture2D>[2];

	public static HitBoxSettings[] HitBoxSettings = new HitBoxSettings[2];

	public bool Alert;

	public static float DeerCarryRotX = 67.03f;

	public static float DeerCarryRotY = 70.85f;

	public static float DeerCarryRotZ = -16.28f;

	public static float DeerCarryOffsetX = -0.25f;

	public static float DeerCarryOffsetY = -0.08f;

	public static float DeerCarryOffsetZ = 0.24f;

	public static float DeerRotSpeed = MathF.PI * 2f;

	private static float DeerStagDistFromFocus = 3f;

	private static float DeerDoeDistFromFocus = 3f;

	private static float DeerPipYOffset = -0.5f;

	private static float DeerStagPipZOffset = 0.25f;

	private static float DeerDoePipZOffset = 0.2f;

	private static int NAME_DeerStag = StringUtil.JenkinsHash("NAME_DeerStag");

	private static int NAME_DeerDoe = StringUtil.JenkinsHash("NAME_DeerDoe");

	public const string StagRoot = "root";

	public const string StagGlobalBone = "root/STAG_";

	public const string StagPositionBone = null;

	public const string StagHips = "root/STAG_/STAG_ Pelvis";

	public const string StagLowerBack = null;

	public const string StagSpine = "root/STAG_/STAG_ Pelvis/STAG_ Spine";

	public const string StagSpine1 = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1";

	public const string StagNeck = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ Neck/STAG_ Neck1";

	public const string StagHead = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ Neck/STAG_ Neck1/STAG_ Neck2/STAG_ Head";

	public const string StagLeftUpLeg = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf";

	public const string StagLeftLeg = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf/STAG_ L HorseLink";

	public const string StagLeftFoot = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf/STAG_ L HorseLink/STAG_ L Foot";

	public const string StagLeftShoulder = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle";

	public const string StagLeftArm = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm";

	public const string StagLeftForearm = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm/STAG_ L Forearm";

	public const string StagLeftHand = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm/STAG_ L Forearm/STAG_ L Hand";

	public const string StagRightUpLeg = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf";

	public const string StagRightLeg = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf/STAG_ R HorseLink";

	public const string StagRightFoot = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf/STAG_ R HorseLink/STAG_ R Foot";

	public const string StagRightShoulder = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle";

	public const string StagRightArm = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm";

	public const string StagRightForearm = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm/STAG_ R Forearm";

	public const string StagRightHand = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm/STAG_ R Forearm/STAG_ R Hand";

	public const string DoeRoot = "root";

	public const string DoeGlobalBone = "root/DeerDoe_";

	public const string DoePositionBone = null;

	public const string DoeHips = "root/DeerDoe_/DeerDoe_ Pelvis";

	public const string DoeLowerBack = null;

	public const string DoeSpine = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine";

	public const string DoeSpine1 = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1";

	public const string DoeNeck = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ Neck/DeerDoe_ Neck1";

	public const string DoeHead = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ Neck/DeerDoe_ Neck1/DeerDoe_ Neck2/DeerDoe_ Head";

	public const string DoeLeftUpLeg = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf";

	public const string DoeLeftLeg = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf/DeerDoe_ L HorseLink";

	public const string DoeLeftFoot = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf/DeerDoe_ L HorseLink/DeerDoe_ L Foot";

	public const string DoeLeftShoulder = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle";

	public const string DoeLeftArm = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm";

	public const string DoeLeftForearm = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm/DeerDoe_ L Forearm";

	public const string DoeLeftHand = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm/DeerDoe_ L Forearm/DeerDoe_ L Hand";

	public const string DoeRightUpLeg = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf";

	public const string DoeRightLeg = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf/DeerDoe_ R HorseLink";

	public const string DoeRightFoot = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf/DeerDoe_ R HorseLink/DeerDoe_ R Foot";

	public const string DoeRightShoulder = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle";

	public const string DoeRightArm = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm";

	public const string DoeRightForearm = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm/DeerDoe_ R Forearm";

	public const string DoeRightHand = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm/DeerDoe_ R Forearm/DeerDoe_ R Hand";

	public override float VisionConeDeg => 150f;

	public static void LoadDeerContent()
	{
		DeerPrefab[0] = new Resource<GameObject>("Prefabs/Animals/Deer/DeerStag_PBR");
		DeerPrefab[1] = new Resource<GameObject>("Prefabs/Animals/Deer/DeerDoe_PBR");
		DeerRagdollSettings[0] = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_DeerStag");
		DeerRagdollSettings[1] = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_DeerDoe");
		DeerIcon[0] = new Resource<Texture2D>("Textures/BuildingIcons/DeerStag");
		DeerIcon[1] = new Resource<Texture2D>("Textures/BuildingIcons/DeerDoe");
		HitBoxSettings[0] = GameObject.Find("HitBoxes/DeerStagHitBoxSettings").GetComponent<HitBoxSettings>();
		HitBoxSettings[1] = GameObject.Find("HitBoxes/DeerDoeHitBoxSettings").GetComponent<HitBoxSettings>();
	}

	public static Deer Spawn(TerrainCoord tile, float facingAngle, DeerAppearance appearance)
	{
		Deer deer = new Deer();
		deer.Appearance = appearance;
		deer.Position = GameTerrain.Instance.GetTileCentrePos(tile);
		deer.FacingAngle = facingAngle;
		deer.OnSpawn();
		return deer;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Deer;
	}

	public override Texture2D GetIconResource()
	{
		return DeerIcon[(int)Appearance.Gender];
	}

	public override CharacterAppearance CreateAppearance()
	{
		return new DeerAppearance();
	}

	public override EquipmentPrototype GetMeatType()
	{
		return EquipmentPrototype.Venison;
	}

	public override int GetMeatAmount(int cookingSkill)
	{
		if (!(SkinnedAmount < 1f))
		{
			return 0;
		}
		return Math.Max(1, (int)((float)(3 + cookingSkill / 2 + ((Appearance.Gender == GenderType.Male) ? 1 : 0)) * (1f - SkinnedAmount)));
	}

	public override void Consume(Character character, float amount, InfectionType infectionType)
	{
		base.Consume(character, amount * 0.1f, infectionType);
	}

	public override float GetMaxInventoryWeightWithoutBackpack()
	{
		return 4f;
	}

	public override void GetSightRange(out int fogStart, out int fogEnd)
	{
		fogEnd = ((CurrentPose == AnimalPose.Sleeping) ? 8 : ((CurrentPose == AnimalPose.Grazing || CurrentPose == AnimalPose.Resting) ? 12 : 16));
		fogStart = fogEnd / 2;
	}

	public override bool HasSenseOfSmell()
	{
		return true;
	}

	public override float GetRunSpeed()
	{
		return 15f;
	}

	public override float GetMaxLimbDamage()
	{
		return 0.01f;
	}

	public override float GetPipFocusDist()
	{
		if (Appearance.Gender != GenderType.Male)
		{
			return DeerDoeDistFromFocus;
		}
		return DeerStagDistFromFocus;
	}

	public override float GetPipYOffset()
	{
		return DeerPipYOffset;
	}

	public override float GetPipZOffset()
	{
		if (Appearance.Gender != GenderType.Male)
		{
			return DeerDoePipZOffset;
		}
		return DeerStagPipZOffset;
	}

	public override float GetCarryRotX()
	{
		return DeerCarryRotX;
	}

	public override float GetCarryRotY()
	{
		return DeerCarryRotY;
	}

	public override float GetCarryRotZ()
	{
		return DeerCarryRotZ;
	}

	public override float GetCarryOffsetX()
	{
		return DeerCarryOffsetX;
	}

	public override float GetCarryOffsetY()
	{
		return DeerCarryOffsetY;
	}

	public override float GetCarryOffsetZ()
	{
		return DeerCarryOffsetZ;
	}

	public override Bone GetPickedUpByBone()
	{
		return Bone.Spine1;
	}

	public override float GetRotSpeed()
	{
		return DeerRotSpeed;
	}

	public override bool CanGraze()
	{
		return true;
	}

	public override bool CanEatGrass()
	{
		return true;
	}

	public override bool IsAffectedByTemperature()
	{
		return false;
	}

	public override void PlayDeathSound()
	{
		PlayVoiceSoundFromList((Appearance.Gender == GenderType.Male) ? SoundManager.DeerStagDeathSounds : SoundManager.DeerDoeDeathSounds, VoiceSoundType.AnimalDeath);
	}

	public override void PlayAlertSound()
	{
		PlayVoiceSoundFromList((Appearance.Gender == GenderType.Male) ? SoundManager.DeerStagAlertSounds : SoundManager.DeerDoeAlertSounds, VoiceSoundType.AnimalAlert);
	}

	public override void PlayFleeSound()
	{
		PlayVoiceSoundFromList((Appearance.Gender == GenderType.Male) ? SoundManager.DeerStagFleeSounds : SoundManager.DeerDoeFleeSounds, VoiceSoundType.AnimalFleeing);
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (string.IsNullOrEmpty(FirstName) || !NameKnown)
		{
			sb.Append(GameImpl.Translate((Appearance.Gender == GenderType.Male) ? NAME_DeerStag : NAME_DeerDoe, englishOnly));
		}
		else
		{
			base.BuildDisplayName(sb, noStrangers, englishOnly);
		}
	}

	public override void UnityInit()
	{
		if (!Disappeared)
		{
			UnityInitAnimal(DeerPrefab[(int)Appearance.Gender], DeerRagdollSettings[(int)Appearance.Gender], "root/");
			base.UnityInit();
		}
	}

	public override bool IsEnemy(TileObject obj, bool includeJustActivatedInvisibleStrain = false)
	{
		if (obj != null)
		{
			switch (obj.GetBaseObjectType())
			{
			case BaseObjectType.Human:
				return true;
			case BaseObjectType.EnterableVehicle:
				return ((EnterableVehicle)obj).IsMoving;
			}
		}
		return false;
	}

	public override void OnDie(float hitRadius, Vector3 nonDeterministicHitPos, Vector3 force, Bone bone, Vector3 posInBoneSpace, CauseOfDeath causeOfDeath, Character killer, SecrecyMode secret)
	{
		base.OnDie(hitRadius, nonDeterministicHitPos, force, bone, posInBoneSpace, causeOfDeath, killer, secret);
		if (CheckFrontmostPrediction(PredictedEventType.DeathSound))
		{
			PlaySoundOneShotFromList((Appearance.Gender == GenderType.Male) ? SoundManager.DeerStagDeathSounds : SoundManager.DeerDoeDeathSounds);
		}
	}

	public override string GetUnityBoneName(Bone bone)
	{
		string result = null;
		if (Appearance.Gender == GenderType.Male)
		{
			switch (bone)
			{
			case Bone.WorldPos:
				result = string.Empty;
				break;
			case Bone.Root:
				result = "root";
				break;
			case Bone.Global:
				result = "root/STAG_";
				break;
			case Bone.Position:
				result = null;
				break;
			case Bone.Hips:
				result = "root/STAG_/STAG_ Pelvis";
				break;
			case Bone.LowerBack:
				result = null;
				break;
			case Bone.Spine:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine";
				break;
			case Bone.Spine1:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1";
				break;
			case Bone.Neck:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ Neck/STAG_ Neck1";
				break;
			case Bone.Head:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ Neck/STAG_ Neck1/STAG_ Neck2/STAG_ Head";
				break;
			case Bone.LeftUpLeg:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf";
				break;
			case Bone.LeftLeg:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf/STAG_ L HorseLink";
				break;
			case Bone.LeftFoot:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ L Thigh/STAG_ L Calf/STAG_ L HorseLink/STAG_ L Foot";
				break;
			case Bone.LeftShoulder:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle";
				break;
			case Bone.LeftArm:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm";
				break;
			case Bone.LeftForearm:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm/STAG_ L Forearm";
				break;
			case Bone.LeftHand:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ L Clavicle/STAG_ L UpperArm/STAG_ L Forearm/STAG_ L Hand";
				break;
			case Bone.RightUpLeg:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf";
				break;
			case Bone.RightLeg:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf/STAG_ R HorseLink";
				break;
			case Bone.RightFoot:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ R Thigh/STAG_ R Calf/STAG_ R HorseLink/STAG_ R Foot";
				break;
			case Bone.RightShoulder:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle";
				break;
			case Bone.RightArm:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm";
				break;
			case Bone.RightForearm:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm/STAG_ R Forearm";
				break;
			case Bone.RightHand:
				result = "root/STAG_/STAG_ Pelvis/STAG_ Spine/STAG_ Spine1/STAG_ R Clavicle/STAG_ R UpperArm/STAG_ R Forearm/STAG_ R Hand";
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
				result = "root";
				break;
			case Bone.Global:
				result = "root/DeerDoe_";
				break;
			case Bone.Position:
				result = null;
				break;
			case Bone.Hips:
				result = "root/DeerDoe_/DeerDoe_ Pelvis";
				break;
			case Bone.LowerBack:
				result = null;
				break;
			case Bone.Spine:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine";
				break;
			case Bone.Spine1:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1";
				break;
			case Bone.Neck:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ Neck/DeerDoe_ Neck1";
				break;
			case Bone.Head:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ Neck/DeerDoe_ Neck1/DeerDoe_ Neck2/DeerDoe_ Head";
				break;
			case Bone.LeftUpLeg:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf";
				break;
			case Bone.LeftLeg:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf/DeerDoe_ L HorseLink";
				break;
			case Bone.LeftFoot:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ L Thigh/DeerDoe_ L Calf/DeerDoe_ L HorseLink/DeerDoe_ L Foot";
				break;
			case Bone.LeftShoulder:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle";
				break;
			case Bone.LeftArm:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm";
				break;
			case Bone.LeftForearm:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm/DeerDoe_ L Forearm";
				break;
			case Bone.LeftHand:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ L Clavicle/DeerDoe_ L UpperArm/DeerDoe_ L Forearm/DeerDoe_ L Hand";
				break;
			case Bone.RightUpLeg:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf";
				break;
			case Bone.RightLeg:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf/DeerDoe_ R HorseLink";
				break;
			case Bone.RightFoot:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ R Thigh/DeerDoe_ R Calf/DeerDoe_ R HorseLink/DeerDoe_ R Foot";
				break;
			case Bone.RightShoulder:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle";
				break;
			case Bone.RightArm:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm";
				break;
			case Bone.RightForearm:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm/DeerDoe_ R Forearm";
				break;
			case Bone.RightHand:
				result = "root/DeerDoe_/DeerDoe_ Pelvis/DeerDoe_ Spine/DeerDoe_ Spine1/DeerDoe_ R Clavicle/DeerDoe_ R UpperArm/DeerDoe_ R Forearm/DeerDoe_ R Hand";
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
		return HitBoxSettings[(int)Appearance.Gender].HitBoxes;
	}
}
