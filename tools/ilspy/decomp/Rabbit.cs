using System;
using System.Text;
using UnityEngine;

public class Rabbit : Animal
{
	public static Resource<GameObject> RabbitPrefab;

	public static Resource<RagdollSettings> RabbitRagdollSettings;

	public static Resource<Texture2D> RabbitIcon;

	public static HitBoxSettings HitBoxSettings;

	private static int NAME_Rabbit = StringUtil.JenkinsHash("NAME_Rabbit");

	public const string Root = "root";

	public const string GlobalBone = "root/RABBIT_";

	public const string PositionBone = null;

	public const string Hips = "root/RABBIT_/RABBIT_ Pelvis";

	public const string LowerBack = null;

	public const string Spine = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine";

	public const string Spine1 = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1";

	public const string Neck = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ Neck";

	public const string Head = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ Neck/RABBIT_ Neck1/RABBIT_ Head";

	public const string LeftUpLeg = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh";

	public const string LeftLeg = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh/RABBIT_ L Calf";

	public const string LeftFoot = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh/RABBIT_ L Calf/RABBIT_ L Foot";

	public const string LeftShoulder = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle";

	public const string LeftArm = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm";

	public const string LeftForearm = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm/RABBIT_ L Forearm";

	public const string LeftHand = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm/RABBIT_ L Forearm/RABBIT_ L Hand";

	public const string RightUpLeg = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh";

	public const string RightLeg = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh/RABBIT_ R Calf";

	public const string RightFoot = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh/RABBIT_ R Calf/RABBIT_ R Foot";

	public const string RightShoulder = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle";

	public const string RightArm = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm";

	public const string RightForearm = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm/RABBIT_ R Forearm";

	public const string RightHand = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm/RABBIT_ R Forearm/RABBIT_ R Hand";

	public override float MapDotSize => 1.5f;

	public override float Radius => 0.15f;

	public override float VisionConeDeg => 150f;

	public static void LoadRabbitContent()
	{
		RabbitPrefab = new Resource<GameObject>("Prefabs/Animals/Rabbit_PBR");
		RabbitRagdollSettings = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_Rabbit");
		RabbitIcon = new Resource<Texture2D>("Textures/BuildingIcons/Rabbit");
		HitBoxSettings = GameObject.Find("HitBoxes/RabbitHitBoxSettings").GetComponent<HitBoxSettings>();
	}

	public static Rabbit Spawn(TerrainCoord tile, float facingAngle, RabbitAppearance appearance)
	{
		Rabbit rabbit = new Rabbit();
		rabbit.Appearance = appearance;
		rabbit.Position = GameTerrain.Instance.GetTileCentrePos(tile);
		rabbit.FacingAngle = facingAngle;
		rabbit.OnSpawn();
		return rabbit;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Rabbit;
	}

	public override Texture2D GetIconResource()
	{
		return RabbitIcon;
	}

	public override EquipmentPrototype GetGrabbableEquipmentType()
	{
		if (!base.Alive)
		{
			return EquipmentPrototype.DeadRabbit;
		}
		return null;
	}

	public override InfectionType GetGrabbableEquipmentInfectedWith()
	{
		return Infection;
	}

	public override CharacterAppearance CreateAppearance()
	{
		return new RabbitAppearance();
	}

	public override bool DeleteWhenSkinned()
	{
		return true;
	}

	public override EquipmentPrototype GetMeatType()
	{
		return EquipmentPrototype.SkinnedRabbit;
	}

	public override int GetMeatAmount(int cookingSkill)
	{
		if (!(SkinnedAmount < 1f))
		{
			return 0;
		}
		return 1;
	}

	public override void Consume(Character character, float amount, InfectionType infectionType)
	{
		base.Consume(character, amount * 10f, infectionType);
	}

	public override float GetPipYOffset()
	{
		if (CurrentPose == AnimalPose.Alert)
		{
			return Pip.BunnyAlertYOffset;
		}
		return 0f;
	}

	public override float GetPipZOffset()
	{
		if (CurrentPose != AnimalPose.Alert)
		{
			return Pip.BunnyZOffset;
		}
		return 0f;
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
		return 4f;
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

	public override float GetPipFocusDist()
	{
		return Pip.BunnyDistFromFocus;
	}

	public override float GetMaxSoundVisibilityRange()
	{
		return 8f;
	}

	public override float GetMaxVisibilityRangeToZombies()
	{
		return 8f;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 274)
		{
			bool value = false;
			reflector.Add(ref value);
			CurrentPose = (value ? AnimalPose.Alert : AnimalPose.Normal);
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (string.IsNullOrEmpty(FirstName) || !NameKnown)
		{
			sb.Append(GameImpl.Translate(NAME_Rabbit, englishOnly));
		}
		else
		{
			base.BuildDisplayName(sb, noStrangers, englishOnly);
		}
	}

	public override void GetSightRange(out int fogStart, out int fogEnd)
	{
		fogEnd = 16;
		fogStart = fogEnd / 2;
	}

	public override bool HasSenseOfSmell()
	{
		return true;
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
		return false;
	}

	public override void UnityInit()
	{
		if (!Disappeared)
		{
			UnityInitAnimal(RabbitPrefab, RabbitRagdollSettings, "root/");
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

	public override void PlayDeathSound()
	{
		PlayVoiceSoundFromList(SoundManager.RabbitDeathSounds, VoiceSoundType.AnimalDeath);
	}

	public override string GetUnityBoneName(Bone bone)
	{
		string result = null;
		switch (bone)
		{
		case Bone.WorldPos:
			result = string.Empty;
			break;
		case Bone.Root:
			result = "root";
			break;
		case Bone.Global:
			result = "root/RABBIT_";
			break;
		case Bone.Position:
			result = null;
			break;
		case Bone.Hips:
			result = "root/RABBIT_/RABBIT_ Pelvis";
			break;
		case Bone.LowerBack:
			result = null;
			break;
		case Bone.Spine:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine";
			break;
		case Bone.Spine1:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1";
			break;
		case Bone.Neck:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ Neck";
			break;
		case Bone.Head:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ Neck/RABBIT_ Neck1/RABBIT_ Head";
			break;
		case Bone.LeftUpLeg:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh";
			break;
		case Bone.LeftLeg:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh/RABBIT_ L Calf";
			break;
		case Bone.LeftFoot:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ L Thigh/RABBIT_ L Calf/RABBIT_ L Foot";
			break;
		case Bone.LeftShoulder:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle";
			break;
		case Bone.LeftArm:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm";
			break;
		case Bone.LeftForearm:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm/RABBIT_ L Forearm";
			break;
		case Bone.LeftHand:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ L Clavicle/RABBIT_ L UpperArm/RABBIT_ L Forearm/RABBIT_ L Hand";
			break;
		case Bone.RightUpLeg:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh";
			break;
		case Bone.RightLeg:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh/RABBIT_ R Calf";
			break;
		case Bone.RightFoot:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ R Thigh/RABBIT_ R Calf/RABBIT_ R Foot";
			break;
		case Bone.RightShoulder:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle";
			break;
		case Bone.RightArm:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm";
			break;
		case Bone.RightForearm:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm/RABBIT_ R Forearm";
			break;
		case Bone.RightHand:
			result = "root/RABBIT_/RABBIT_ Pelvis/RABBIT_ Spine/RABBIT_ Spine1/RABBIT_ R Clavicle/RABBIT_ R UpperArm/RABBIT_ R Forearm/RABBIT_ R Hand";
			break;
		case Bone.MeleeWeapon:
			result = null;
			break;
		}
		return result;
	}

	public override HitBox[] GetHitBoxes()
	{
		return HitBoxSettings.HitBoxes;
	}
}
