using System;
using System.Collections.Generic;
using UMA;
using UMA.PoseTools;
using UnityEngine;
using UnityEngine.Rendering;

public class Human : Character
{
	private enum BodyPart
	{
		Head,
		Torso,
		Hands,
		Legs,
		Feet,
		Count
	}

	public bool Pregnant;

	public float PregnancyProgression;

	private static Resource<GameObject> UMACharacter;

	public static Resource<GameObject> UMAMale;

	public static Resource<GameObject> UMAFemale;

	private static Resource<GameObject>[] FemalePussZombieBody = new Resource<GameObject>[5];

	private static Resource<GameObject>[] MaleSkeletalZombieBody = new Resource<GameObject>[5];

	private static Resource<UmaTPose> MaleUmaTPose;

	private static Resource<UmaTPose> FemaleUmaTPose;

	private static Resource<UMAExpressionSet> MaleExpressionSet;

	private static Resource<UMAExpressionSet> FemaleExpressionSet;

	private static Resource<RagdollSettings> MaleRagdollSettings;

	private static Resource<RagdollSettings> FemaleRagdollSettings;

	private static Resource<Material>[] FaceMaterial = new Resource<Material>[21];

	private static Resource<Material> MaleBodyMaterial;

	private static Resource<Material> FemaleBodyMaterial;

	private static Resource<Material> MaleZombieBody1Material;

	private static Resource<Material> MaleZombieBody3Material;

	private static Resource<Material> FemalePussZombieBodyMaterial;

	private static Resource<Material> MaleSkeletalZombieFaceMaterial;

	private static Resource<Material>[] EyesMaterial = new Resource<Material>[6];

	private static Resource<Material> InnerMouthMaterial;

	public static Resource<Material> EyelashesMaterial;

	public static Resource<Material>[] HairMaterial = new Resource<Material>[27];

	private static Resource<Material> HairBandMaterial;

	private static Resource<Material>[] BeardMaterial = new Resource<Material>[10];

	private static Resource<Texture>[] UMAHairTex = new Resource<Texture>[27];

	private static Resource<Texture>[] UMAHairBump = new Resource<Texture>[27];

	private static Resource<Texture>[] UMAHairMetallic = new Resource<Texture>[27];

	private static Resource<Texture>[] UMABeardTex = new Resource<Texture>[10];

	private static Resource<Texture>[] UMABeardBump = new Resource<Texture>[10];

	private static Resource<Texture>[] UMABeardMetallic = new Resource<Texture>[10];

	private static Resource<GameObject>[] UMAHair = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMAHairBand = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMABeanieHair = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMABeanieHairBand = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMAHatHair = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMAHatHairBand = new Resource<GameObject>[27];

	private static Resource<GameObject>[] UMABeard = new Resource<GameObject>[10];

	private static Resource<GameObject>[] UMABeardBand = new Resource<GameObject>[10];

	private static List<PrefabResource> ClothingMeshes = new List<PrefabResource>();

	public static Dictionary<string, GameObject> ClothingMeshByName = new Dictionary<string, GameObject>();

	private static List<Resource<Material>> ClothingMaterials = new List<Resource<Material>>();

	public static Dictionary<string, Material> ClothingMaterialsByName = new Dictionary<string, Material>();

	public static HitBoxSettings HitBoxSettings;

	private static List<SkeletonBone> newBones = new List<SkeletonBone>();

	public static string UMA_Male_Rig = "UMA_Male_Rig";

	public static string UMA_Female_Rig = "UMA_Female_Rig";

	public static string NoneTag = "None";

	public static string ArmorTag = "Armor";

	public static string HelmetTag = "Helmet";

	public static string LegArmorTag = "LegArmor";

	public static string EyelashTag = "Eyelash";

	public static string HairTag = "Hair";

	public static bool JustUnifiedBody = false;

	private static float MoveAnimSpeedScaleMin = 0.5f;

	private static float MoveAnimSpeedScaleMax = 1.5f;

	public float UnityCrouchingTransition;

	public float UnitySittingTransition;

	public static bool WantAimAngleOverride = false;

	public static float AimAngleOverride = 0f;

	private static string UnityUpdateStr = "Human.UnityUpdate";

	private int LastUpdateFacialExpressionFrame = int.MinValue;

	private static float ExpressionUpdateDistFromCamera = 8f;

	public const string Root = "Root";

	public const string GlobalBone = "Root/Global";

	public const string PositionBone = "Root/Global/Position";

	public const string Hips = "Root/Global/Position/Hips";

	public const string LowerBack = "Root/Global/Position/Hips/LowerBack";

	public const string Spine = "Root/Global/Position/Hips/LowerBack/Spine";

	public const string Spine1 = "Root/Global/Position/Hips/LowerBack/Spine/Spine1";

	public const string Neck = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck";

	public const string Head = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head";

	public const string LeftUpLeg = "Root/Global/Position/Hips/LeftUpLeg";

	public const string LeftLeg = "Root/Global/Position/Hips/LeftUpLeg/LeftLeg";

	public const string LeftFoot = "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot";

	public const string LeftShoulder = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder";

	public const string LeftArm = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm";

	public const string LeftForearm = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm";

	public const string LeftHand = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand";

	public const string RightUpLeg = "Root/Global/Position/Hips/RightUpLeg";

	public const string RightLeg = "Root/Global/Position/Hips/RightUpLeg/RightLeg";

	public const string RightFoot = "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot";

	public const string RightShoulder = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder";

	public const string RightArm = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm";

	public const string RightForearm = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm";

	public const string RightHand = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand";

	public const string UnityWeaponName = "Weapon";

	public const string UnityLoadedAmmoName = "LoadedAmmo";

	public const string UnityWeaponFullName = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/Weapon";

	public const string UnityLeftHandItemName = "LeftHandItem";

	public const string UnityLeftHandItemFullName = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHandItem";

	public const string UnityBowStringFullName = "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Weapon/Bow_Group/Bow_Base/Bow_String";

	private static float MinTimeBetweenBushRustles = 0.25f;

	private static float MaxTimeBetweenBushRustles = 0.5f;

	private static float BushRustleVolume = 1f;

	public static float PregnancyMonths = 9f;

	public static float InLaborTime = Sun.DayLengthSecs * 0.25f;

	public static float UnaccompaniedDieInChildbirthTime = InLaborTime * 0.25f;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref Pregnant, 482);
		reflector.AddAfter(ref PregnancyProgression, 482);
	}

	public static Human Spawn(TerrainCoord tile, float facingAngle, HumanAppearance appearance, InfectionType infection)
	{
		Human human = new Human();
		human.Position = GameTerrain.Instance.GetTileCentrePos(tile);
		human.FacingAngle = facingAngle;
		human.Appearance = appearance;
		human.Infection = infection;
		human.InfectionProgression = ((infection != InfectionType.None) ? 1f : 0f);
		human.OnSpawn();
		return human;
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Human;
	}

	public Human GetAuthoritativeOrElseThisHuman()
	{
		if (!IsAuthoritative())
		{
			return (Human)Authoritative;
		}
		return this;
	}

	public Human GetPredictedOrElseThisHuman()
	{
		if (Predicted == null)
		{
			return this;
		}
		return (Human)Predicted;
	}

	public HumanAppearance GetAppearance()
	{
		return (HumanAppearance)Appearance;
	}

	public override CharacterAppearance CreateAppearance()
	{
		return new HumanAppearance();
	}

	public override float GetPregnancyProgression()
	{
		return PregnancyProgression;
	}

	public override bool IsPregnant()
	{
		return Pregnant;
	}

	public override EquipmentPrototype GetMeatType()
	{
		if (!Rotten)
		{
			return EquipmentPrototype.HumanMeat;
		}
		return EquipmentPrototype.RancidHumanMeat;
	}

	public override int GetMeatAmount(int cookingSkill)
	{
		if (!(SkinnedAmount < 1f))
		{
			return 0;
		}
		return Math.Max(1, (int)((float)(2 + cookingSkill) * (1f - SkinnedAmount)));
	}

	public override float GetShoutVoiceRadius()
	{
		return 32f;
	}

	public override float GetMaxSoundVisibilityRange()
	{
		if (!IsCrouching())
		{
			return 32f;
		}
		return 16f;
	}

	public override void OnNewGame()
	{
		base.OnNewGame();
		if (base.Zombie)
		{
			SetGoal(new ZombieGoal());
		}
		else
		{
			SetGoal(new SurvivorGoal());
		}
	}

	public void SaveFavouriteCharacter(OnSavedFunction onSavedFunction)
	{
		new SavedCharacter(this).SaveWithConfirmation(onSavedFunction);
	}

	public void RandomizeAppearance(CustomRandom rand)
	{
		Appearance.Age = HumanAppearance.PickRandomAge(rand);
		GetAppearance().Randomize(Infection, rand);
		UnityOnChangedBones();
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null && !CanWear(Clothes[i].GetPrototype()))
			{
				Clothes[i].Strip(this);
			}
		}
	}

	public override bool CanWear(EquipmentPrototype proto)
	{
		if (proto.ClothingType != ClothingType.Invalid)
		{
			if (proto.ClothingType == ClothingType.Top && GetAppearance().BodyType == BodyType.FemalePussZombie)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override string GetLootLocation()
	{
		if (!string.IsNullOrEmpty(LootLocation))
		{
			return LootLocation;
		}
		if (Community != null && !string.IsNullOrEmpty(Community.LootLocation))
		{
			return Community.LootLocation;
		}
		if (!base.Zombie)
		{
			return LootLocationDef.Survivor;
		}
		return LootLocationDef.Zombie;
	}

	public override bool CanOpenGates()
	{
		return !base.Zombie;
	}

	public override Texture2D GetOverheadActionIcon()
	{
		Texture2D overheadActionIcon = base.GetOverheadActionIcon();
		if (overheadActionIcon == null && IsCrouching() && GetPredictedOrElseThisHuman().UnityCrouchingTransition > 0.99f)
		{
			return GameCursor.CamouflageIcon;
		}
		return overheadActionIcon;
	}

	public static void LoadHumanContent()
	{
		UMACharacter = new Resource<GameObject>("Prefabs/Character/UMA_Blender_Separated");
		UMAMale = new Resource<GameObject>("Prefabs/Character/Male_Unified");
		UMAFemale = new Resource<GameObject>("Prefabs/Character/Female_Unified");
		FemalePussZombieBody[1] = new Resource<GameObject>("Prefabs/Character/FemaleZombie2/F_Zombie 2_torso");
		FemalePussZombieBody[2] = new Resource<GameObject>("Prefabs/Character/FemaleZombie2/F_Zombie 2_hands");
		FemalePussZombieBody[3] = new Resource<GameObject>("Prefabs/Character/FemaleZombie2/F_Zombie 2_lowerbody");
		FemalePussZombieBody[4] = new Resource<GameObject>("Prefabs/Character/FemaleZombie2/F_Zombie 2_feet");
		MaleSkeletalZombieBody[0] = new Resource<GameObject>("Prefabs/Character/MaleZombie1/M_Zombie 1_head");
		MaleSkeletalZombieBody[1] = new Resource<GameObject>("Prefabs/Character/MaleZombie1/M_Zombie 1_Torso");
		MaleSkeletalZombieBody[2] = new Resource<GameObject>("Prefabs/Character/MaleZombie1/M_Zombie 1_hands");
		MaleSkeletalZombieBody[3] = new Resource<GameObject>("Prefabs/Character/MaleZombie1/M_Zombie 1_lower");
		MaleSkeletalZombieBody[4] = new Resource<GameObject>("Prefabs/Character/MaleZombie1/M_Zombie 1_feet");
		MaleUmaTPose = new Resource<UmaTPose>("TPoses/Male_Unified_TPose");
		FemaleUmaTPose = new Resource<UmaTPose>("TPoses/Female_Unified_TPose");
		MaleExpressionSet = new Resource<UMAExpressionSet>("Expressions/Male Expression Set");
		FemaleExpressionSet = new Resource<UMAExpressionSet>("Expressions/Male Expression Set");
		MaleRagdollSettings = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_Male");
		FemaleRagdollSettings = new Resource<RagdollSettings>("RagdollSettings/Ragdoll_Female");
		FaceMaterial[0] = new Resource<Material>("Materials/Character/Faces/MaleFace01");
		FaceMaterial[1] = new Resource<Material>("Materials/Character/Faces/MaleFace02");
		FaceMaterial[2] = new Resource<Material>("Materials/Character/Faces/M_Face1");
		FaceMaterial[3] = new Resource<Material>("Materials/Character/Faces/M_Face2");
		FaceMaterial[4] = new Resource<Material>("Materials/Character/Faces/M_Face3");
		FaceMaterial[5] = new Resource<Material>("Materials/Character/Faces/M_Face4");
		FaceMaterial[6] = new Resource<Material>("Materials/Character/Faces/M_Face5");
		FaceMaterial[7] = new Resource<Material>("Materials/Character/Faces/M_Face6");
		FaceMaterial[8] = new Resource<Material>("Materials/Character/Faces/M_Face7");
		FaceMaterial[9] = new Resource<Material>("Materials/Character/Faces/M_Face8");
		FaceMaterial[10] = new Resource<Material>("Materials/Character/Faces/MaleZombieFace3");
		FaceMaterial[11] = new Resource<Material>("Materials/Character/Faces/MaleZombieFace4");
		FaceMaterial[12] = new Resource<Material>("Materials/Character/Faces/FemaleFace01");
		FaceMaterial[13] = new Resource<Material>("Materials/Character/Faces/F_Face1");
		FaceMaterial[14] = new Resource<Material>("Materials/Character/Faces/F_Face2");
		FaceMaterial[15] = new Resource<Material>("Materials/Character/Faces/F_Face3");
		FaceMaterial[16] = new Resource<Material>("Materials/Character/Faces/F_Face4");
		FaceMaterial[17] = new Resource<Material>("Materials/Character/Faces/F_Face5");
		FaceMaterial[18] = new Resource<Material>("Materials/Character/Faces/F_Face6");
		FaceMaterial[19] = new Resource<Material>("Materials/Character/Faces/F_Face7");
		FaceMaterial[20] = new Resource<Material>("Materials/Character/Faces/F_Face8");
		MaleBodyMaterial = new Resource<Material>("Materials/Character/Body/MaleBody02");
		FemaleBodyMaterial = new Resource<Material>("Materials/Character/Body/FemaleBody01");
		MaleZombieBody1Material = new Resource<Material>("Materials/Character/Body/MaleZombieBody1");
		MaleZombieBody3Material = new Resource<Material>("Materials/Character/Body/MaleZombieBody3");
		FemalePussZombieBodyMaterial = new Resource<Material>("Materials/Character/Body/FemaleZombieBody2");
		MaleSkeletalZombieFaceMaterial = new Resource<Material>("Materials/Character/Faces/MaleZombieFace1");
		EyesMaterial[0] = (EyesMaterial[5] = new Resource<Material>("Materials/Character/Eyes/Eye"));
		EyesMaterial[1] = new Resource<Material>("Materials/Character/Eyes/ZombieEye_Green");
		EyesMaterial[2] = new Resource<Material>("Materials/Character/Eyes/ZombieEye_Blue");
		EyesMaterial[3] = new Resource<Material>("Materials/Character/Eyes/ZombieEye_Red");
		EyesMaterial[4] = new Resource<Material>("Materials/Character/Eyes/ZombieEye_White");
		InnerMouthMaterial = new Resource<Material>("Materials/Character/InnerMouth");
		EyelashesMaterial = new Resource<Material>("Materials/Character/Eyelash");
		HairBandMaterial = new Resource<Material>("Materials/Character/Hair/HairBand");
		UMAHairTex[25] = new Resource<Texture>("Textures/Hair/hair 10");
		UMAHairBump[25] = new Resource<Texture>("Textures/Hair/hair 10_n");
		UMAHairMetallic[25] = new Resource<Texture>("Textures/Hair/hair 10_s");
		UMAHairTex[12] = new Resource<Texture>("Textures/Hair/MaleHair01_Albedo");
		UMAHairBump[12] = new Resource<Texture>("Textures/Hair/MaleHair01_Normal");
		UMAHairMetallic[12] = new Resource<Texture>("Textures/Hair/MaleHair01_Roughness");
		UMAHairTex[13] = new Resource<Texture>("Textures/Hair/Cap_Hair");
		UMAHairBump[13] = new Resource<Texture>("Textures/Hair/Cap_Hair Heightmap");
		UMAHairTex[14] = new Resource<Texture>("Textures/Hair/Beard head stubble_d");
		UMAHairBump[14] = new Resource<Texture>("Textures/Hair/Beard head stubble_n");
		UMABeardTex[3] = new Resource<Texture>("Textures/Hair/MaleBeard01_Albedo");
		UMABeardBump[3] = new Resource<Texture>("Textures/Hair/MaleBeard01_Normal");
		UMABeardMetallic[3] = new Resource<Texture>("Textures/Hair/MaleBeard01_Roughness");
		UMABeardTex[2] = new Resource<Texture>("Textures/Hair/MaleBeard02_Albedo");
		UMABeardBump[2] = new Resource<Texture>("Textures/Hair/MaleBeard02_Normal");
		UMABeardMetallic[2] = new Resource<Texture>("Textures/Hair/MaleBeard02_Roughness");
		UMABeardTex[4] = new Resource<Texture>("Textures/Hair/MaleBeard03_Albedo");
		UMABeardBump[4] = new Resource<Texture>("Textures/Hair/MaleBeard03_Normal");
		UMABeardMetallic[4] = new Resource<Texture>("Textures/Hair/MaleBeard03_Roughness");
		UMABeardTex[1] = new Resource<Texture>("Textures/Hair/stubble_d");
		UMABeardBump[1] = new Resource<Texture>("Textures/Hair/stubble_n");
		UMAHair[15] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair1");
		UMABeanieHair[15] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair1_Beanie");
		UMAHatHair[15] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair1_Cap");
		UMAHair[16] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair2");
		UMABeanieHair[16] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair2_Beanie");
		UMAHatHair[16] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair2_Cap");
		UMAHair[17] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair3");
		UMABeanieHair[17] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair3_Beanie");
		UMAHatHair[17] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair3_Cap");
		UMAHair[18] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair4");
		UMABeanieHair[18] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair4_Beanie");
		UMAHatHair[18] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair4_Cap");
		UMAHair[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5");
		UMAHairBand[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5BAND");
		UMABeanieHair[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5_Beanie");
		UMABeanieHairBand[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5BAND_Beanie");
		UMAHatHair[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5_Cap");
		UMAHatHairBand[19] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair5BAND");
		UMAHair[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6");
		UMAHairBand[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6band");
		UMABeanieHair[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6_Beanie");
		UMABeanieHairBand[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6band");
		UMAHatHair[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6_Cap");
		UMAHatHairBand[20] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair6band");
		UMAHair[21] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair7");
		UMAHairBand[21] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair7bands");
		UMABeanieHair[21] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair7_Beanie");
		UMAHatHair[21] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair7_Cap");
		UMAHair[22] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair8");
		UMABeanieHair[22] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair8_Beanie");
		UMAHatHair[22] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair8_Cap");
		UMAHair[23] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair9");
		UMABeanieHair[23] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair9_Beanie");
		UMAHatHair[23] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair9_Cap");
		UMAHair[24] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair11");
		UMABeanieHair[24] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair11_Beanie");
		UMAHatHair[24] = new Resource<GameObject>("Prefabs/Character/Hair/F_Hair11_Cap");
		UMAHair[26] = new Resource<GameObject>("Prefabs/Character/Hair/F_ZOMBIE 1_hair");
		UMAHair[1] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair1");
		UMABeanieHair[1] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair1_Beanie");
		UMAHatHair[1] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair1_Cap");
		UMAHair[2] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair2");
		UMABeanieHair[2] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair2_Beanie");
		UMAHatHair[2] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair2_Cap");
		UMAHair[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3");
		UMAHairBand[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3 band");
		UMABeanieHair[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3_Beanie");
		UMABeanieHairBand[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3 band_Beanie");
		UMAHatHair[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3_Cap");
		UMAHatHairBand[3] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair3 band");
		UMAHair[4] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair4");
		UMABeanieHair[4] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair1_Beanie");
		UMAHatHair[4] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair1_Cap");
		UMAHair[5] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair 5");
		UMABeanieHair[5] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair5_Beanie");
		UMAHatHair[5] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair5_Cap");
		UMAHair[6] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair6");
		UMABeanieHair[6] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair5_Beanie");
		UMAHatHair[6] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair5_Cap");
		UMAHair[7] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair7");
		UMAHairBand[7] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair7band");
		UMABeanieHair[7] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair5_Beanie");
		UMAHatHair[7] = new Resource<GameObject>("Prefabs/Character/Hair/M_Hair7_Cap");
		UMAHair[8] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair8");
		UMAHair[9] = new Resource<GameObject>("Prefabs/Character/Hair/M_hair9");
		UMAHair[10] = new Resource<GameObject>("Prefabs/Character/Hair/MaleShortHair");
		UMABeanieHair[10] = new Resource<GameObject>("Prefabs/Character/Hair/MaleShortHair_Beanie");
		UMAHatHair[10] = new Resource<GameObject>("Prefabs/Character/Hair/MaleShortHair_Cap");
		UMAHair[11] = new Resource<GameObject>("Prefabs/Character/Hair/MaleHair_Military");
		UMAHair[12] = null;
		UMAHair[13] = null;
		UMAHair[14] = null;
		Resource<Material> resource = new Resource<Material>("Materials/Character/Hair/Hair");
		Resource<Material> resource2 = new Resource<Material>("Materials/Character/Hair/IsbitMaleHair");
		for (int i = 0; i < HairMaterial.Length; i++)
		{
			if (UMAHair[i] != null)
			{
				HairMaterial[i] = resource;
			}
		}
		HairMaterial[10] = resource2;
		HairMaterial[11] = resource2;
		HairMaterial[26] = new Resource<Material>("Materials/Character/Hair/FemaleZombieHair");
		UMABeard[1] = null;
		UMABeard[3] = null;
		UMABeard[2] = null;
		UMABeard[4] = null;
		UMABeard[5] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard1");
		UMABeard[6] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard2");
		UMABeard[7] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard 3");
		UMABeard[8] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard4");
		UMABeard[9] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard 5");
		UMABeardBand[9] = new Resource<GameObject>("Prefabs/Character/Hair/M_beard 5band");
		BeardMaterial[5] = new Resource<Material>("Materials/Character/Hair/MaleBeard1");
		BeardMaterial[6] = new Resource<Material>("Materials/Character/Hair/MaleBeard2");
		BeardMaterial[7] = new Resource<Material>("Materials/Character/Hair/MaleBeard3");
		BeardMaterial[8] = new Resource<Material>("Materials/Character/Hair/MaleBeard4");
		BeardMaterial[9] = new Resource<Material>("Materials/Character/Hair/MaleBeard5");
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Female_Mill_All"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Male_Mill_All"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/F_Long coat_boots"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/F_Long coat_glasses"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/F_Long coat_pants"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/F_Long coat_top"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/long coat"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/long coat boots"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/long coat glasses"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/long coat pants"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/long coat top"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_balaklava_fix01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_boots_fix01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_hat_cap"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_hat_cowboy"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_hat_fix_01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_pants_fix01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_sport"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_t-shirt_fix01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mill_vest"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/Mll_Hat_cap_var01"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/M_Helmet6"));
		ClothingMeshes.Add(new PrefabResource("Prefabs/Character/Clothes/F_Helmet6"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Regular"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Detective"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Sheriff"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Camo"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Poor"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/CowboyHat/Woven"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/Red"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/TigerStripe"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MercJacket/Woodland"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Digital"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Woodland1"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Woodland2"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Woodland3"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Woodland4"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillVest/Woodland5"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Digital"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Woodland1"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Woodland2"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillPants/Woodland3"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Digital"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Woodland1"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Woodland2"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Woodland3"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Woodland4"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/BaseballCap/Woodland5"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Digital"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Woodland1"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Woodland2"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Woodland3"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Woodland4"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillCap/Woodland5"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Blank"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Desert"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Snow"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Digital"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Woodland1"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Woodland2"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/MillHat/Woodland3"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Backpack/Backpack4"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Backpack/Backpack4_Camo"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 2-a"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 2-b"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 6-a"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 6-b"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 7-a"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 7-c"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 8-a"));
		ClothingMaterials.Add(new Resource<Material>("Materials/Character/Clothes/Army Headgear Pack/helmet 8-b"));
		HitBoxSettings = GameObject.Find("HitBoxes/HumanHitBoxSettings").GetComponent<HitBoxSettings>();
	}

	public static void OnAllContentLoaded()
	{
		using (new StopWatchMarker("Human.OnAllContentLoaded"))
		{
			CheckVertexPositionsAreInUVChannels(UMACharacter);
			for (int i = 0; i < FemalePussZombieBody.Length; i++)
			{
				if (FemalePussZombieBody[i] != null)
				{
					CheckVertexPositionsAreInUVChannels(FemalePussZombieBody[i]);
				}
			}
			for (int j = 0; j < MaleSkeletalZombieBody.Length; j++)
			{
				if (MaleSkeletalZombieBody[j] != null)
				{
					CheckVertexPositionsAreInUVChannels(MaleSkeletalZombieBody[j]);
				}
			}
			foreach (PrefabResource clothingMesh in ClothingMeshes)
			{
				GameObject gameObject = clothingMesh;
				for (int k = 0; k < gameObject.transform.childCount; k++)
				{
					GameObject gameObject2 = gameObject.transform.GetChild(k).gameObject;
					SkinnedMeshRenderer component = gameObject2.GetComponent<SkinnedMeshRenderer>();
					if (!gameObject2.name.StartsWith("UMA") && component != null)
					{
						string key = gameObject.name + ":" + gameObject2.name;
						ClothingMeshByName[key] = gameObject2;
					}
				}
				CheckVertexPositionsAreInUVChannels(gameObject);
			}
			foreach (Resource<Material> clothingMaterial in ClothingMaterials)
			{
				string path = clothingMaterial.GetPath();
				path = path.Replace("Materials/Character/Clothes/", "");
				ClothingMaterialsByName[path] = clothingMaterial;
			}
		}
	}

	public static void CheckVertexPositionsAreInUVChannels(Mesh mesh)
	{
		if (mesh.uv2 == null || mesh.uv2.Length != mesh.vertices.Length || mesh.uv3 == null || mesh.uv3.Length != mesh.vertices.Length)
		{
			Debug.LogError("Mesh doesn't have vertex positions in UV channels: " + mesh.name + " (add it to MeshPostProcessing.SkinnedMeshNames)");
		}
	}

	public static void CheckVertexPositionsAreInUVChannels(GameObject obj)
	{
	}

	private GameObject CopyBones(GameObject sourceObj, GameObject targetParentObj)
	{
		GameObject gameObject = new GameObject();
		gameObject.name = sourceObj.name;
		gameObject.transform.parent = targetParentObj.transform;
		gameObject.transform.localPosition = sourceObj.transform.localPosition;
		gameObject.transform.localRotation = sourceObj.transform.localRotation;
		gameObject.transform.localScale = sourceObj.transform.localScale;
		for (int i = 0; i < sourceObj.transform.childCount; i++)
		{
			GameObject gameObject2 = sourceObj.transform.GetChild(i).gameObject;
			CopyBones(gameObject2, gameObject);
		}
		return gameObject;
	}

	private Transform UnityFindBoneByName(Transform obj, string name)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			Transform child = obj.transform.GetChild(i);
			if (child.name == name)
			{
				return child;
			}
			Transform transform = UnityFindBoneByName(child, name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public override void UnityInit()
	{
		if (Disappeared)
		{
			return;
		}
		Unity.Obj = new GameObject(GetDisplayNameString());
		switch (Usage)
		{
		case CharacterUsage.Normal:
			Unity.Obj.layer = Character.CharactersLayer;
			break;
		case CharacterUsage.IconGimp:
			Unity.Obj.layer = Character.IconGimpLayer;
			break;
		case CharacterUsage.PreviewGimp:
			Unity.Obj.layer = Character.PreviewGimpLayer;
			break;
		}
		Unity.Obj.AddComponent<CharacterBehaviour>().Owner = this;
		UnitySetupAudioSource();
		Unity.Animator = Unity.Obj.AddComponent<Animator>();
		Unity.Animator.runtimeAnimatorController = (base.Zombie ? AnimationManager.Instance.UnityZombieAnimatorController : AnimationManager.Instance.UnityAnimatorController);
		Unity.Animator.applyRootMotion = false;
		Unity.Prefab = ((Appearance.Gender == GenderType.Male) ? UMAMale : UMAFemale);
		GameObject obj = Unity.Prefab;
		GameObject sourceObj = obj.FindChild((Appearance.Gender == GenderType.Male) ? UMA_Male_Rig : UMA_Female_Rig);
		GameObject gameObject = CopyBones(sourceObj, Unity.Obj);
		gameObject.name = "Root";
		gameObject.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
		gameObject.transform.GetChild(0).gameObject.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
		SkinnedMeshRenderer component = obj.FindChild((Appearance.Gender == GenderType.Male) ? "UMA_Human_Male" : "UMA_Human_Female").GetComponent<SkinnedMeshRenderer>();
		Unity.Bones = new Transform[component.bones.Length];
		for (int i = 0; i < Unity.Bones.Length; i++)
		{
			Unity.Bones[i] = UnityFindBoneByName(Unity.Obj.transform, component.bones[i].name);
		}
		UmaTPose umaTPose = ((Appearance.Gender == GenderType.Male) ? MaleUmaTPose : FemaleUmaTPose);
		umaTPose.DeSerialize();
		UMASkeleton uMASkeleton = new UMASkeleton(gameObject.transform);
		for (int j = 0; j < umaTPose.boneInfo.Length; j++)
		{
			SkeletonBone skeletonBone = umaTPose.boneInfo[j];
			int nameHash = Animator.StringToHash(skeletonBone.name);
			if (uMASkeleton.HasBone(nameHash))
			{
				uMASkeleton.Set(nameHash, skeletonBone.position, skeletonBone.scale, skeletonBone.rotation);
			}
		}
		if (Appearance.Gender == GenderType.Male)
		{
			HumanMaleSkeletonSetup.UpdateUMAMaleDNABones(GetAppearance().UmaDnaHumanoid, uMASkeleton);
		}
		else
		{
			HumanFemaleSkeletonSetup.UpdateUMAFemaleDNABones(GetAppearance().UmaDnaHumanoid, uMASkeleton);
		}
		HumanDescription humanDescription = CreateHumanDescription(gameObject, uMASkeleton, umaTPose);
		Avatar avatar = AvatarBuilder.BuildHumanAvatar(Unity.Obj, humanDescription);
		Unity.Animator.avatar = avatar;
		RagdollSettings ragdollSettings = ((Appearance.Gender == GenderType.Female) ? FemaleRagdollSettings : MaleRagdollSettings);
		UnitySetupRagdollBones(ragdollSettings, "Root/");
		Unity.SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
		UnitySetupAppearance();
		Unity.Obj.SetActive(value: false);
		Unity.Obj.FindChild("Root").layer = Unity.Obj.layer;
		Unity.UmaExpressionPlayer = Unity.Obj.AddComponent<UMAExpressionPlayer>();
		Unity.UmaExpressionPlayer.expressionSet = ((Appearance.Gender == GenderType.Male) ? MaleExpressionSet : FemaleExpressionSet);
		Unity.UmaExpressionPlayer.Initialize(uMASkeleton, GetAppearance().UmaDnaHumanoid.headSize);
		Unity.UmaExpressionPlayer.enableBlinking = true;
		Unity.UmaExpressionPlayer.enableSaccades = true;
		UnitySetupCollisionShape();
		UnityPlayInitialAnim();
		base.UnityInit();
	}

	public override void UnityDelete()
	{
		Avatar avatar = ((Unity.Animator != null) ? Unity.Animator.avatar : null);
		base.UnityDelete();
		if (avatar != null)
		{
			UnityEngine.Object.Destroy(avatar);
		}
	}

	public HumanDescription CreateHumanDescription(GameObject rootBone, UMASkeleton umaSkeleton, UmaTPose umaTPose)
	{
		HumanDescription result = new HumanDescription
		{
			armStretch = 0f,
			feetSpacing = 0f,
			legStretch = 0f,
			lowerArmTwist = 0.2f,
			lowerLegTwist = 1f,
			upperArmTwist = 0.5f,
			upperLegTwist = 0.1f,
			skeleton = umaTPose.boneInfo,
			human = umaTPose.humanInfo
		};
		SkeletonModifier(rootBone, umaSkeleton, ref result.skeleton, result.human);
		return result;
	}

	private void SkeletonModifier(GameObject rootBone, UMASkeleton umaSkeleton, ref SkeletonBone[] bones, HumanBone[] human)
	{
		int i = 0;
		newBones.Clear();
		for (; !umaSkeleton.HasBone(Animator.StringToHash(bones[i].name)); i++)
		{
		}
		if (i > 0)
		{
			Transform transform = Unity.Obj.transform;
			SkeletonBone item = bones[i - 2];
			item.position = transform.localPosition;
			item.rotation = transform.localRotation;
			item.scale = transform.localScale;
			item.name = transform.name;
			newBones.Add(item);
			Transform transform2 = rootBone.transform;
			item = bones[i - 1];
			item.position = transform2.localPosition;
			item.rotation = transform2.localRotation;
			item.scale = transform2.localScale;
			item.name = transform2.name;
			newBones.Add(item);
		}
		for (int j = i; j < bones.Length; j++)
		{
			SkeletonBone item2 = bones[j];
			int nameHash = Animator.StringToHash(item2.name);
			GameObject boneGameObject = umaSkeleton.GetBoneGameObject(nameHash);
			if (boneGameObject != null)
			{
				item2.position = boneGameObject.transform.localPosition;
				item2.scale = boneGameObject.transform.localScale;
				item2.rotation = umaSkeleton.GetTPoseCorrectedRotation(nameHash, item2.rotation);
				newBones.Add(item2);
			}
		}
		bones = newBones.ToArray();
	}

	private SkinnedMeshRenderer UnitySetupSkinnedMeshRenderer(SkinnedMeshRenderer prefabSkinnedMeshRenderer, string tag)
	{
		string name = prefabSkinnedMeshRenderer.transform.parent.name + ":" + prefabSkinnedMeshRenderer.name;
		GameObject gameObject = Unity.Obj.FindChild(name);
		SkinnedMeshRenderer skinnedMeshRenderer;
		if (gameObject == null)
		{
			gameObject = new GameObject(name);
			gameObject.transform.parent = Unity.Obj.transform;
			gameObject.layer = Unity.Obj.layer;
			skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
			skinnedMeshRenderer.rootBone = GetUnityBone(Bone.Hips).transform;
			skinnedMeshRenderer.bones = Unity.Bones;
		}
		else
		{
			skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
		}
		skinnedMeshRenderer.sharedMesh = prefabSkinnedMeshRenderer.sharedMesh;
		gameObject.tag = tag;
		skinnedMeshRenderer.localBounds = MathUtil.CreateBoundsCentreExtents(prefabSkinnedMeshRenderer.localBounds.center, prefabSkinnedMeshRenderer.localBounds.extents + Vector3.one);
		skinnedMeshRenderer.updateWhenOffscreen = Unity.WantUpdateWhenOffscreen;
		Unity.SkinnedMeshRenderers.Add(skinnedMeshRenderer);
		RendererDataBehaviour component = skinnedMeshRenderer.GetComponent<RendererDataBehaviour>();
		if (component != null)
		{
			component.RendererDataDirty = true;
		}
		return skinnedMeshRenderer;
	}

	private SkinnedMeshRenderer UnitySetupBodyPart(string name, Material mat, string tag)
	{
		GameObject gameObject = UMACharacter.GetAsset().transform.Find(name).gameObject;
		SkinnedMeshRenderer skinnedMeshRenderer = UnitySetupSkinnedMeshRenderer(gameObject.GetComponent<SkinnedMeshRenderer>(), tag);
		skinnedMeshRenderer.sharedMaterial = mat;
		return skinnedMeshRenderer;
	}

	private SkinnedMeshRenderer UnitySetupBodyPart(GameObject prefab, Material mat, string tag)
	{
		for (int i = 0; i < prefab.transform.childCount; i++)
		{
			GameObject gameObject = prefab.transform.GetChild(i).gameObject;
			if (gameObject.GetComponent<SkinnedMeshRenderer>() != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = UnitySetupSkinnedMeshRenderer(gameObject.GetComponent<SkinnedMeshRenderer>(), tag);
				skinnedMeshRenderer.sharedMaterial = mat;
				return skinnedMeshRenderer;
			}
		}
		return null;
	}

	private static GameObject FindClothingPrefab(string name)
	{
		for (int num = GameImpl.Instance.CurrentStories.Count - 1; num >= 0; num--)
		{
			foreach (PrefabResource prefab in GameImpl.Instance.CurrentStories[num].Prefabs)
			{
				if (!prefab.IsSkinned() || !(prefab.GetPath() == name))
				{
					continue;
				}
				GameObject asset = prefab.GetAsset();
				if (!(asset != null))
				{
					continue;
				}
				for (int i = 0; i < asset.transform.childCount; i++)
				{
					GameObject gameObject = asset.transform.GetChild(i).gameObject;
					if (gameObject.GetComponent<SkinnedMeshRenderer>() != null)
					{
						return gameObject;
					}
				}
			}
		}
		if (ClothingMeshByName.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	private SkinnedMeshRenderer UnitySetupClothingPart(Equipment item)
	{
		if (item != null)
		{
			string meshName = item.GetMeshName(Appearance.Gender);
			if (meshName != null)
			{
				GameObject gameObject = FindClothingPrefab(meshName);
				if (gameObject != null)
				{
					SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
					if (component != null)
					{
						string tag = ((!(item is Armor)) ? NoneTag : ((item.GetClothingType() == ClothingType.Hat) ? HelmetTag : ((item.GetClothingType() == ClothingType.LegArmor) ? LegArmorTag : ArmorTag)));
						SkinnedMeshRenderer skinnedMeshRenderer = UnitySetupSkinnedMeshRenderer(component, tag);
						string materialName = item.GetMaterialName();
						if (materialName != null)
						{
							skinnedMeshRenderer.sharedMaterial = ClothingMaterialsByName[materialName];
						}
						else
						{
							skinnedMeshRenderer.sharedMaterial = component.sharedMaterial;
						}
						skinnedMeshRenderer.material.color = item.GetColor();
						skinnedMeshRenderer.material.SetColor(ShaderHash._Color2, item.GetColor2());
						skinnedMeshRenderer.material.SetColor(ShaderHash._Color3, item.GetColor3());
						return skinnedMeshRenderer;
					}
				}
			}
		}
		return null;
	}

	public override void UnitySetupAppearance()
	{
		if (Disappeared)
		{
			return;
		}
		HumanAppearance appearance = GetAppearance();
		bool flag = Usage == CharacterUsage.IconGimp && Character.GeneratingIconType == IconType.Clothing;
		Unity.SkinnedMeshRenderers.Clear();
		Unity.BackpackSkinnedMeshRenderer = null;
		Equipment equipment = Clothes[0];
		Equipment item = Clothes[1];
		Equipment item2 = Clothes[2];
		Equipment item3 = Clothes[3];
		Equipment item4 = Clothes[4];
		Equipment item5 = Clothes[5];
		Equipment item6 = Clothes[6];
		Equipment item7 = Clothes[7];
		Equipment item8 = Clothes[8];
		Color32 skinColor = appearance.GetSkinColor(this);
		Material mat = ((appearance.Gender == GenderType.Male) ? MaleBodyMaterial : FemaleBodyMaterial);
		if (appearance.FaceType == FaceType.MaleZombieFace3 || appearance.FaceType == FaceType.MaleZombieFace4)
		{
			mat = MaleZombieBody3Material;
		}
		if (JustUnifiedBody)
		{
			UnitySetupBodyPart(Unity.Prefab, mat, NoneTag);
		}
		else
		{
			if (!flag)
			{
				switch (appearance.BodyType)
				{
				case BodyType.Normal:
				{
					SkinnedMeshRenderer skinnedMeshRenderer = UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Face" : "UMA_Human_Female_Face", FaceMaterial[(int)appearance.FaceType], NoneTag);
					skinnedMeshRenderer.material.color = skinColor;
					skinnedMeshRenderer.material.SetColor(ShaderHash._HairColor, appearance.HairColor);
					skinnedMeshRenderer.material.SetColor(ShaderHash._FrecklesColor, appearance.FrecklesColor);
					skinnedMeshRenderer.material.SetFloat(ShaderHash._WrinklesAmount, appearance.WrinklesAmount);
					skinnedMeshRenderer.material.SetFloat(ShaderHash._ZombieAmount, base.Zombie ? appearance.ZombieDecayAmount : 0f);
					if (UMAHairTex[(int)appearance.HairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairTex, UMAHairTex[(int)appearance.HairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairTex, null);
					}
					if (UMAHairBump[(int)appearance.HairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairBumpMap, UMAHairBump[(int)appearance.HairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairBumpMap, null);
					}
					if (UMAHairMetallic[(int)appearance.HairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairMetallicGlossMap, UMAHairMetallic[(int)appearance.HairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._HairMetallicGlossMap, null);
					}
					if (UMABeardTex[(int)appearance.FacialHairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardTex, UMABeardTex[(int)appearance.FacialHairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardTex, null);
					}
					if (UMABeardBump[(int)appearance.FacialHairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardBumpMap, UMABeardBump[(int)appearance.FacialHairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardBumpMap, null);
					}
					if (UMABeardMetallic[(int)appearance.FacialHairType] != null)
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardMetallicGlossMap, UMABeardMetallic[(int)appearance.FacialHairType]);
					}
					else
					{
						skinnedMeshRenderer.material.SetTexture(ShaderHash._BeardMetallicGlossMap, null);
					}
					Vector4 value = ((appearance.FacialHairType == FacialHairType.ShortGoateeAndMoustache || appearance.FacialHairType == FacialHairType.LightMuttonChopsAndMoustache || appearance.FacialHairType == FacialHairType.MuttonChopsAndMoustache) ? new Vector4(2f, 2f, -0.5f, 0f) : new Vector4(1f, 1f, 0f, 0f));
					skinnedMeshRenderer.material.SetVector(ShaderHash._BeardTex_ST, value);
					skinnedMeshRenderer.material.SetVector(ShaderHash._BeardBumpMap_ST, value);
					skinnedMeshRenderer.material.SetVector(ShaderHash._BeardMetallicGlossMap_ST, value);
					SkinnedMeshRenderer skinnedMeshRenderer2 = UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Hands" : "UMA_Human_Female_Hands", mat, NoneTag);
					skinnedMeshRenderer2.material.color = skinColor;
					skinnedMeshRenderer2.material.SetFloat(ShaderHash._ZombieAmount, base.Zombie ? appearance.ZombieDecayAmount : 0f);
					if (appearance.Gender == GenderType.Female)
					{
						UnitySetupBodyPart("UMA_Human_Female_Eyelash", EyelashesMaterial, EyelashTag);
					}
					break;
				}
				case BodyType.FemalePussZombie:
					UnitySetupBodyPart(FemalePussZombieBody[2], FemalePussZombieBodyMaterial, NoneTag).material.color = skinColor;
					break;
				case BodyType.MaleSkeletalZombie:
					UnitySetupBodyPart(MaleSkeletalZombieBody[0], MaleSkeletalZombieFaceMaterial, NoneTag).material.color = skinColor;
					UnitySetupBodyPart(MaleSkeletalZombieBody[2], MaleZombieBody1Material, NoneTag).material.color = skinColor;
					break;
				}
				UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Eyes" : "UMA_Human_Female_Eyes", EyesMaterial[(int)Infection], NoneTag).material.color = appearance.EyeColor;
				UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_InnerMouth" : "UMA_Human_Female_InnerMouth", InnerMouthMaterial, NoneTag);
				Resource<GameObject> resource = UMAHair[(int)appearance.HairType];
				Resource<GameObject> resource2 = UMAHairBand[(int)appearance.HairType];
				Resource<GameObject> resource3 = UMABeard[(int)appearance.FacialHairType];
				Resource<GameObject> resource4 = UMABeardBand[(int)appearance.FacialHairType];
				if (equipment is Balaklava)
				{
					resource = null;
					resource2 = null;
					resource3 = null;
					resource4 = null;
				}
				else if (equipment is Beanie)
				{
					resource = UMABeanieHair[(int)appearance.HairType];
					resource2 = UMABeanieHairBand[(int)appearance.HairType];
				}
				else if (equipment != null && !(equipment is Crown))
				{
					resource = UMAHatHair[(int)appearance.HairType];
					resource2 = UMAHatHairBand[(int)appearance.HairType];
				}
				if (resource != null)
				{
					UnitySetupBodyPart(resource, HairMaterial[(int)appearance.HairType], HairTag).material.color = appearance.HairColor;
				}
				if (resource2 != null)
				{
					UnitySetupBodyPart(resource2, HairBandMaterial, NoneTag).material.color = appearance.HairBandColor;
				}
				if (resource3 != null)
				{
					UnitySetupBodyPart(resource3, BeardMaterial[(int)appearance.FacialHairType], HairTag).material.color = appearance.HairColor;
				}
				if (resource4 != null)
				{
					UnitySetupBodyPart(resource4, HairBandMaterial, NoneTag).material.color = appearance.HairBandColor;
				}
			}
			SkinnedMeshRenderer skinnedMeshRenderer3 = UnitySetupClothingPart(equipment);
			if (skinnedMeshRenderer3 != null && (Usage == CharacterUsage.IconGimp || Usage == CharacterUsage.PreviewGimp))
			{
				skinnedMeshRenderer3.shadowCastingMode = ShadowCastingMode.Off;
			}
			if ((UnitySetupClothingPart(item2) == null || appearance.BodyType == BodyType.FemalePussZombie) && !flag)
			{
				SkinnedMeshRenderer skinnedMeshRenderer4 = null;
				switch (appearance.BodyType)
				{
				case BodyType.Normal:
					skinnedMeshRenderer4 = UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Torso" : "UMA_Human_Female_Torso", mat, NoneTag);
					break;
				case BodyType.FemalePussZombie:
					skinnedMeshRenderer4 = UnitySetupBodyPart(FemalePussZombieBody[1], FemalePussZombieBodyMaterial, NoneTag);
					break;
				case BodyType.MaleSkeletalZombie:
					skinnedMeshRenderer4 = UnitySetupBodyPart(MaleSkeletalZombieBody[1], MaleZombieBody1Material, NoneTag);
					break;
				}
				skinnedMeshRenderer4.material.color = skinColor;
				skinnedMeshRenderer4.material.SetColor(ShaderHash._UnderwearColor, appearance.UnderwearColor);
				skinnedMeshRenderer4.material.SetFloat(ShaderHash._ZombieAmount, base.Zombie ? appearance.ZombieDecayAmount : 0f);
			}
			if (UnitySetupClothingPart(item3) == null && !flag)
			{
				SkinnedMeshRenderer skinnedMeshRenderer5 = null;
				switch (appearance.BodyType)
				{
				case BodyType.Normal:
					skinnedMeshRenderer5 = UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Legs" : "UMA_Human_Female_Legs", mat, NoneTag);
					break;
				case BodyType.FemalePussZombie:
					skinnedMeshRenderer5 = UnitySetupBodyPart(FemalePussZombieBody[3], FemalePussZombieBodyMaterial, NoneTag);
					break;
				case BodyType.MaleSkeletalZombie:
					skinnedMeshRenderer5 = UnitySetupBodyPart(MaleSkeletalZombieBody[3], MaleZombieBody1Material, NoneTag);
					break;
				}
				skinnedMeshRenderer5.material.color = skinColor;
				skinnedMeshRenderer5.material.SetColor(ShaderHash._UnderwearColor, appearance.UnderwearColor);
				skinnedMeshRenderer5.material.SetFloat(ShaderHash._ZombieAmount, base.Zombie ? appearance.ZombieDecayAmount : 0f);
			}
			if (UnitySetupClothingPart(item4) == null && !flag)
			{
				SkinnedMeshRenderer skinnedMeshRenderer6 = null;
				switch (appearance.BodyType)
				{
				case BodyType.Normal:
					skinnedMeshRenderer6 = UnitySetupBodyPart((appearance.Gender == GenderType.Male) ? "UMA_Human_Male_Feet" : "UMA_Human_Female_Feet", mat, NoneTag);
					break;
				case BodyType.FemalePussZombie:
					skinnedMeshRenderer6 = UnitySetupBodyPart(FemalePussZombieBody[4], FemalePussZombieBodyMaterial, NoneTag);
					break;
				case BodyType.MaleSkeletalZombie:
					skinnedMeshRenderer6 = UnitySetupBodyPart(MaleSkeletalZombieBody[4], MaleZombieBody1Material, NoneTag);
					break;
				}
				skinnedMeshRenderer6.material.color = skinColor;
				skinnedMeshRenderer6.material.SetFloat(ShaderHash._ZombieAmount, base.Zombie ? appearance.ZombieDecayAmount : 0f);
			}
			UnitySetupClothingPart(item);
			Unity.BackpackSkinnedMeshRenderer = UnitySetupClothingPart(item5);
			UnitySetupClothingPart(item6);
			UnitySetupClothingPart(item7);
			UnitySetupClothingPart(item8);
		}
		for (int num = Unity.Obj.transform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = Unity.Obj.transform.GetChild(num).gameObject;
			SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component != null && Unity.SkinnedMeshRenderers.IndexOf(component) == -1)
			{
				for (int i = 0; i < gameObject.transform.childCount; i++)
				{
					InjuryBehaviour component2 = gameObject.transform.GetChild(i).gameObject.GetComponent<InjuryBehaviour>();
					if (component2 != null && Unity.InjuryObjects != null)
					{
						Unity.InjuryObjects.Remove(component2);
					}
				}
				UnityEngine.Object.DestroyImmediate(component.material);
				UnityEngine.Object.DestroyImmediate(gameObject);
			}
		}
		UnityUpdateDecals();
		UnityUpdateInjuries();
		UnityUpdateMeshList();
	}

	public override void LinkMuscleToStrengthSkill()
	{
		float num = (float)Skillset.Strength / 5f;
		if (num != GetAppearance().Bones.Muscle)
		{
			GetAppearance().Bones.Muscle = num;
			GetAppearance().SetupDNA();
			if (Unity.Obj != null && Usage != CharacterUsage.PreviewGimp)
			{
				UnityOnChangedBones();
			}
		}
	}

	public override void UnityUpdateSkinColor()
	{
		HumanAppearance appearance = GetAppearance();
		Color32 skinColor = appearance.GetSkinColor(this);
		for (int i = 0; i < Unity.SkinnedMeshRenderers.Count; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = Unity.SkinnedMeshRenderers[i];
			if (skinnedMeshRenderer.material.shader.name == "Custom/Character/Skin" || skinnedMeshRenderer.material.shader.name == "Custom/Character/Face")
			{
				skinnedMeshRenderer.material.color = skinColor;
				skinnedMeshRenderer.material.SetFloat(ShaderHash._ZombieAmount, appearance.ZombieDecayAmount);
			}
		}
	}

	public override void UnityActivate()
	{
		base.UnityActivate();
		UnityCrouchingTransition = (IsCrouching() ? 1f : 0f);
		UnitySittingTransition = (IsSitting() ? 1f : 0f);
	}

	public override void UnityDeactivate()
	{
		UnityCrouchingTransition = 0f;
		UnitySittingTransition = 0f;
		base.UnityDeactivate();
	}

	private void UnityStripCollisionObjects(GameObject obj)
	{
		List<Collider> list = new List<Collider>();
		obj.GetComponentsInChildren(includeInactive: true, list);
		foreach (Collider item in list)
		{
			UnityEngine.Object.Destroy(item);
		}
	}

	private void UnityUpdateEquippedItems(out EquippedModelProperties equippedModelProperties)
	{
		equippedModelProperties = ((EquippedItem != null) ? EquippedItem.GetPrototype().EquippedModelProperties : null);
		EquippedModelProperties equippedModelProperties2 = ((LeftHandEquippedItem != null) ? LeftHandEquippedItem.GetPrototype().EquippedModelProperties : null);
		GameObject gameObject = ((EquippedItem != null && EquippedItem.ShowLoadedAmmo()) ? EquippedItem.GetLoadedAmmoModel() : null);
		switch (CurrentActionAnim)
		{
		case ActionAnim.ForgeStart:
		case ActionAnim.ForgeLoop:
		case ActionAnim.ForgeEnd:
			equippedModelProperties = Equipment.GetEquippedModel(Equipment.HammerProperties);
			equippedModelProperties2 = null;
			break;
		case ActionAnim.PotStart:
		case ActionAnim.PotLoop:
		case ActionAnim.PotEnd:
			equippedModelProperties = Equipment.GetEquippedModel(Equipment.SpoonProperties);
			break;
		}
		if (CarryingObject != null)
		{
			equippedModelProperties = null;
			equippedModelProperties2 = null;
			gameObject = null;
		}
		GameObject gameObject2 = ((equippedModelProperties != null && equippedModelProperties.Prefab != null) ? equippedModelProperties.Prefab.GetAsset() : null);
		if (Unity.WeaponModel != gameObject2)
		{
			if (Unity.WeaponModel != null)
			{
				UnityEngine.Object.DestroyImmediate(Unity.WeaponObj);
				Unity.WeaponObj = null;
				Unity.WeaponAnimator = null;
				Unity.WeaponModel = null;
				UnityUpdateMeshList();
			}
			if (gameObject2 != null)
			{
				Transform transform = GetUnityBone(equippedModelProperties.EquippedBone).transform;
				Unity.WeaponObj = UnityEngine.Object.Instantiate(gameObject2, transform);
				if (!(EquippedItem is Throwable))
				{
					UnityStripCollisionObjects(Unity.WeaponObj);
				}
				Unity.WeaponAnimator = Unity.WeaponObj.GetComponent<Animator>();
				if (Unity.WeaponAnimator == null && EquippedItem is Bow && EquipmentPrototype.Bow != null && EquipmentPrototype.Bow.EquippedModelProperties != null && EquipmentPrototype.Bow.EquippedModelProperties.Prefab != null && EquipmentPrototype.Bow.EquippedModelProperties.Prefab.GetAsset() != null && EquipmentPrototype.Bow.EquippedModelProperties.Prefab.GetAsset().GetComponent<Animator>() != null)
				{
					Unity.WeaponAnimator = Unity.WeaponObj.AddComponent<Animator>();
					Unity.WeaponAnimator.runtimeAnimatorController = AnimationManager.Instance.UnityBowController;
					Unity.WeaponAnimator.avatar = EquipmentPrototype.Bow.EquippedModelProperties.Prefab.GetAsset().GetComponent<Animator>().avatar;
					Unity.WeaponAnimator.applyRootMotion = true;
					Unity.WeaponAnimator.updateMode = AnimatorUpdateMode.Normal;
					Unity.WeaponAnimator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
				}
				Unity.WeaponObj.name = "Weapon";
				Prop.SetLayerRecursively(Unity.WeaponObj, Unity.Obj.layer);
				UnityUpdateMeshList();
				Unity.WeaponModel = gameObject2;
			}
		}
		if (Unity.LoadedAmmoModel != gameObject)
		{
			if (Unity.LoadedAmmoModel != null)
			{
				UnityEngine.Object.DestroyImmediate(Unity.LoadedAmmoObj);
				Unity.LoadedAmmoObj = null;
				Unity.LoadedAmmoModel = null;
				UnityUpdateMeshList();
			}
			if (gameObject != null)
			{
				Transform transform2 = GetUnityBone(EquippedItem.GetLoadedAmmoBone()).transform;
				Unity.LoadedAmmoObj = UnityEngine.Object.Instantiate(gameObject, transform2);
				Unity.LoadedAmmoObj.name = "LoadedAmmo";
				Prop.SetLayerRecursively(Unity.LoadedAmmoObj, Unity.Obj.layer);
				UnityUpdateMeshList();
				Unity.LoadedAmmoModel = gameObject;
			}
		}
		if (Unity.LoadedAmmoObj != null && EquippedItem != null)
		{
			Unity.LoadedAmmoObj.transform.localPosition = EquippedItem.GetLoadedAmmoLocalPos();
			Unity.LoadedAmmoObj.transform.localEulerAngles = EquippedItem.GetLoadedAmmoLocalRotation();
			Unity.LoadedAmmoObj.transform.localScale = EquippedItem.GetLoadedAmmoLocalScale();
		}
		GameObject gameObject3 = ((equippedModelProperties2 != null && equippedModelProperties2.Prefab != null) ? equippedModelProperties2.Prefab.GetAsset() : null);
		if (Unity.LeftHandItemModel != gameObject3)
		{
			if (Unity.LeftHandItemModel != null)
			{
				UnityEngine.Object.DestroyImmediate(Unity.LeftHandItemObj);
				Unity.LeftHandItemObj = null;
				Unity.LeftHandItemModel = null;
				UnityUpdateMeshList();
			}
			if (gameObject3 != null)
			{
				Transform transform3 = GetUnityBone(Bone.LeftHand).transform;
				Unity.LeftHandItemObj = UnityEngine.Object.Instantiate(gameObject3, transform3);
				Unity.LeftHandItemObj.name = "LeftHandItem";
				Prop.SetLayerRecursively(Unity.LeftHandItemObj, Unity.Obj.layer);
				UnityUpdateMeshList();
				Unity.LeftHandItemModel = gameObject3;
			}
		}
		if (Unity.LeftHandItemObj != null && equippedModelProperties2 != null)
		{
			Unity.LeftHandItemObj.transform.localPosition = equippedModelProperties2.LocalPos;
			Unity.LeftHandItemObj.transform.localEulerAngles = equippedModelProperties2.LocalRotation;
			Unity.LeftHandItemObj.transform.localScale = equippedModelProperties2.LocalScale * Vector3.one;
		}
	}

	public override void UnityUpdate()
	{
		if (IsBeingPredicted())
		{
			return;
		}
		using (new UnityProfileMarker(UnityUpdateStr))
		{
			UnityUpdateEquippedItems(out var equippedModelProperties);
			bool flag = Consciousness == Consciousness.Dead && IsProne() && Unity.CurrentVoiceSoundType == VoiceSoundType.None;
			if (CanSkipUnityUpdate && flag)
			{
				return;
			}
			CanSkipUnityUpdate = flag;
			if (!IsUnityObjectActive())
			{
				Debug.Log("Calling UnityUpdate but Unity Object is not active!");
			}
			UnityEnsureAnimStateIsCorrect();
			bool flag2 = IsAiming() || (AnimWrapper != null && AnimWrapper.Aiming);
			bool flag3 = IsCrouching();
			bool flag4 = IsSitting();
			TargettableBodyLocation currentTargetBodyLocation = GetCurrentTargetBodyLocation();
			if (flag2)
			{
				if (WantAimAngleOverride)
				{
					Unity.AimingUpDownAngle = AimAngleOverride * (MathF.PI / 180f);
				}
				else
				{
					Unity.AimingUpDownAngle = MathUtil.Delt(Unity.AimingUpDownAngle, CalcAimUpDownAngle(), Time.deltaTime * (MathF.PI * 2f));
				}
			}
			else
			{
				Unity.AimingUpDownAngle = 0f;
			}
			UnityUpdatePosition();
			if (Unity.Animator != null && Unity.Animator.isInitialized)
			{
				float num = MovementSpeed;
				if (num < MoveAnimSpeedScaleMax)
				{
					num = Mathf.Lerp(MoveAnimSpeedScaleMin, MoveAnimSpeedScaleMax, num / MoveAnimSpeedScaleMax);
				}
				float num2 = Unity.Animator.GetFloat(AnimHash.Speed);
				if (MovementSpeed > 0f && num2 == 0f && AnimWrapper != null && AnimWrapper.UpperBodyStateNameHash != 0)
				{
					TimeSpan actionAnimTime = GetActionAnimTime();
					Unity.Animator.CrossFadeInFixedTime(AnimWrapper.UpperBodyStateNameHash, AnimWrapper.TransitionInTime, (!AnimWrapper.UpperBodyIsAdditive) ? 1 : 2, (float)actionAnimTime.TotalSeconds * AnimWrapper.Speed);
				}
				Unity.Animator.SetFloat(AnimHash.Speed, (MovementSpeed > 0f) ? num : 0f);
				Unity.Animator.SetBool(AnimHash.Aiming, flag2);
				Unity.Animator.SetBool(AnimHash.Sitting, Sitting);
				Vector2 vector = MathUtil.GetDirFromAngle(MathUtil.SignedAngleDiff(MovementAngle, FacingAngle)) * num;
				Unity.Animator.SetFloat(AnimHash.VelX, vector.x);
				Unity.Animator.SetFloat(AnimHash.VelZ, vector.y);
				Unity.Animator.SetBool(AnimHash.Female, Appearance.Gender == GenderType.Female);
				if (base.Zombie)
				{
					UnityCrouchingTransition = 0f;
					UnitySittingTransition = 0f;
					bool rightLeg;
					bool flag5 = ShouldLimp(out rightLeg);
					Unity.Animator.SetBool(AnimHash.LimpLeftLeg, flag5 && !rightLeg);
					Unity.Animator.SetBool(AnimHash.LimpRightLeg, flag5 && rightLeg);
				}
				else
				{
					UnityCrouchingTransition = Unity.Animator.GetFloat(AnimHash.CrouchingAmount);
					UnitySittingTransition = Unity.Animator.GetFloat(AnimHash.SittingAmount);
					bool flag6 = (EquippedItem is MeleeWeapon || EquippedItem == null) && CurrentActionAnim != ActionAnim.Attack;
					Unity.Animator.SetBool(AnimHash.InActionAnim, CurrentActionAnim != ActionAnim.None);
					Unity.Animator.SetBool(AnimHash.InAimingAnim, UnityIsInAimingAnim());
					Unity.Animator.SetFloat(AnimHash.AimUpDownAngle, Unity.AimingUpDownAngle * 57.29578f);
					Unity.Animator.SetBool(AnimHash.MeleeAimHigh, flag6 && currentTargetBodyLocation == TargettableBodyLocation.Head);
					Unity.Animator.SetBool(AnimHash.MeleeAimMiddle, flag6 && currentTargetBodyLocation == TargettableBodyLocation.Torso);
					Unity.Animator.SetBool(AnimHash.MeleeAimLow, flag6 && currentTargetBodyLocation == TargettableBodyLocation.Legs);
					Unity.Animator.SetBool(AnimHash.Sliding, CurrentActionAnim == ActionAnim.Slide);
					Unity.Animator.SetBool(AnimHash.Carrying, CarryingObject != null);
					Unity.Animator.SetBool(AnimHash.Crouching, flag3);
					Unity.Animator.SetBool(AnimHash.Limping, ShouldLimp());
					Unity.Animator.SetBool(AnimHash.DisableFistsAiming, DisableFistsAiming);
					EquippedAnim equippedAnim = equippedModelProperties?.EquippedAnim ?? EquippedAnim.None;
					bool flag7 = equippedAnim == EquippedAnim.OneHanded;
					bool flag8 = equippedAnim == EquippedAnim.Pistol;
					bool flag9 = equippedAnim == EquippedAnim.Rifle;
					bool flag10 = equippedAnim == EquippedAnim.Bow;
					bool flag11 = EquippedItem != null && EquippedItem is Throwable;
					bool value = !flag7 && !flag8 && !flag9 && !flag10 && !flag11;
					Unity.Animator.SetBool(AnimHash.Unarmed, value);
					Unity.Animator.SetBool(AnimHash.OneHanded, flag7);
					Unity.Animator.SetBool(AnimHash.Pistol, flag8);
					Unity.Animator.SetBool(AnimHash.Rifle, flag9);
					Unity.Animator.SetBool(AnimHash.Bow, flag10);
					Unity.Animator.SetBool(AnimHash.Throwable, flag11);
					Unity.Animator.SetBool(AnimHash.Syringe, equippedAnim == EquippedAnim.Syringe);
					Unity.Animator.SetBool(AnimHash.Spoon, equippedAnim == EquippedAnim.Spoon);
					Unity.Animator.SetBool(AnimHash.Carrot, equippedAnim == EquippedAnim.Carrot);
					Unity.Animator.SetBool(AnimHash.WaterBottle, equippedAnim == EquippedAnim.WaterBottle);
					Unity.Animator.SetBool(AnimHash.WateringCan, equippedAnim == EquippedAnim.WateringCan);
					Unity.Animator.SetBool(AnimHash.Flint, equippedAnim == EquippedAnim.Flint);
				}
				if (Unity.WeaponObj != null && equippedModelProperties != null)
				{
					Unity.WeaponObj.transform.localPosition = equippedModelProperties.LocalPos;
					Unity.WeaponObj.transform.localEulerAngles = equippedModelProperties.LocalRotation;
					Unity.WeaponObj.transform.localScale = equippedModelProperties.LocalScale * Vector3.one;
					if (Unity.WeaponAnimator != null && Unity.WeaponAnimator.runtimeAnimatorController != null)
					{
						Unity.WeaponAnimator.SetBool(AnimHash.Aiming, flag2);
						Unity.WeaponAnimator.SetBool(AnimHash.Crouching, flag3);
					}
				}
				if (Unity.BackpackSkinnedMeshRenderer != null)
				{
					Unity.BackpackSkinnedMeshRenderer.enabled = CurrentActionAnim != ActionAnim.ChokeHoldStruggle;
				}
				UpdateFacialExpression();
				UpdateRecoveringFromRagdoll();
				bool flag12 = Consciousness == Consciousness.Unconscious;
				if (Unity.UnconsciousEffect != null && !flag12 && Unity.UnconsciousEffect.WantToBeDeleted())
				{
					UnityEngine.Object.DestroyImmediate(Unity.UnconsciousEffect.gameObject);
					Unity.UnconsciousEffect = null;
					UnityUpdateCanvasList();
				}
				if (Unity.UnconsciousEffect == null && flag12)
				{
					Transform transform = GetUnityBone(Bone.Head).transform;
					GameObject gameObject = new GameObject();
					gameObject.transform.parent = transform;
					gameObject.transform.localPosition = new Vector3(-0.15f, 0f, 0f);
					Unity.UnconsciousEffect = gameObject.AddComponent<UnconsciousBehaviour>();
				}
				if (Unity.UnconsciousEffect != null)
				{
					int num3 = 0;
					if (flag12)
					{
						num3 = Math.Max(num3, (int)(SedativeEffect / 5f) + 1);
						if (BloodLoss > 1f)
						{
							int num4 = ((!base.Zombie) ? GetSkillLevelWithEffects(SkillType.Constitution) : 0);
							if (num4 > 0)
							{
								num3 = Math.Max(num3, Mathf.CeilToInt((BloodLoss - 1f) / (float)num4 * 6f));
							}
						}
						num3 = Math.Min(6, num3);
					}
					Unity.UnconsciousEffect.SetWantNumStars(this, num3);
				}
			}
			else
			{
				UnityCrouchingTransition = Mathf.Clamp01(UnityCrouchingTransition + (flag3 ? 1f : (-1f)) * Time.deltaTime / 0.25f);
				UnitySittingTransition = Mathf.Clamp01(UnitySittingTransition + (flag4 ? 1f : (-1f)) * Time.deltaTime / 0.25f);
			}
			UnityUpdateOverheadIcons();
			UpdateLoopingSound();
			if (CurrentActionAnim == ActionAnim.Slide && Unity.SlidingEffect == null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate((GameObject)SpecialEffectManager.Instance.SlidingDust, Unity.Obj.transform);
				GameObject gameObject3 = UnityEngine.Object.Instantiate((GameObject)SpecialEffectManager.Instance.SlidingDebris, Unity.Obj.transform);
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject3.transform.localPosition = Vector3.zero;
				Unity.SlidingEffect = gameObject2.GetComponent<ParticleSystem>();
				Unity.SlidingDebris = gameObject3.GetComponent<ParticleSystem>();
			}
			if (Unity.SlidingEffect != null)
			{
				if (CurrentActionAnim == ActionAnim.Slide && !Unity.SlidingEffect.isPlaying)
				{
					Unity.SlidingEffect.Play();
					Unity.SlidingDebris.Play();
				}
				if (CurrentActionAnim == ActionAnim.Slide && !Unity.SlidingEffect.emission.enabled)
				{
					ParticleSystem.EmissionModule emission = Unity.SlidingEffect.emission;
					ParticleSystem.EmissionModule emission2 = Unity.SlidingDebris.emission;
					emission.enabled = true;
					emission2.enabled = true;
				}
				if (CurrentActionAnim != ActionAnim.Slide && Unity.SlidingEffect.emission.enabled)
				{
					ParticleSystem.EmissionModule emission3 = Unity.SlidingEffect.emission;
					ParticleSystem.EmissionModule emission4 = Unity.SlidingDebris.emission;
					emission3.enabled = false;
					emission4.enabled = false;
				}
				if (CurrentActionAnim != ActionAnim.Slide && !Unity.SlidingEffect.IsAlive())
				{
					Unity.SlidingEffect.Stop();
					Unity.SlidingDebris.Stop();
				}
				if (CurrentActionAnim == ActionAnim.Slide)
				{
					ParticleSystem.MainModule main = Unity.SlidingEffect.main;
					main.startColor = Color.Lerp(new Color(0.5f, 0.5f, 0.5f, 0.5f), new Color(1f, 1f, 1f, 0.5f), MathUtil.Squared(Session.Instance.Weather.SnowOnGroundAmount));
				}
			}
			if (CarriedBy != null)
			{
				UpdateCarriedBy();
			}
		}
	}

	public void UnityUpdateIKPoint(int animHash, AvatarIKGoal ikGoal, IKPoint ikPoint)
	{
		float num = Unity.Animator.GetFloat(animHash);
		if (animHash == AnimHash.PistolAimingIK || animHash == AnimHash.RifleLeftHandIK)
		{
			num = num * num * num * num;
		}
		UnityUpdateIKPoint(ikGoal, ikPoint, num);
	}

	public void UnityUpdateIKPoint(AvatarIKGoal ikGoal, IKPoint ikPoint, float weight)
	{
		if (weight > 0f)
		{
			GameObject unityBone = GetUnityBone(ikPoint.Bone);
			if (unityBone != null)
			{
				Matrix4x4 localToWorldMatrix = unityBone.transform.localToWorldMatrix;
				Matrix4x4 matrix4x = Matrix4x4.TRS(ikPoint.Pos, Quaternion.Euler(ikPoint.Rot), Vector3.one);
				Matrix4x4 mat = localToWorldMatrix * matrix4x;
				Unity.Animator.SetIKPosition(ikGoal, mat.Translation());
				Unity.Animator.SetIKPositionWeight(ikGoal, weight * ikPoint.PosWeight);
				Unity.Animator.SetIKRotation(ikGoal, mat.rotation);
				Unity.Animator.SetIKRotationWeight(ikGoal, weight * ikPoint.RotWeight);
			}
		}
	}

	private void UnityUpdateIKPointLerped(AvatarIKGoal ikGoal, IKPoint ikPoint1, IKPoint ikPoint2, float t, float weight)
	{
		if (weight > 0f)
		{
			GameObject unityBone = GetUnityBone(ikPoint1.Bone);
			GameObject unityBone2 = GetUnityBone(ikPoint2.Bone);
			if (unityBone != null && unityBone2 != null)
			{
				Matrix4x4 localToWorldMatrix = unityBone.transform.localToWorldMatrix;
				Matrix4x4 localToWorldMatrix2 = unityBone2.transform.localToWorldMatrix;
				Matrix4x4 matrix4x = Matrix4x4.TRS(ikPoint1.Pos, Quaternion.Euler(ikPoint1.Rot), Vector3.one);
				Matrix4x4 matrix4x2 = Matrix4x4.TRS(ikPoint2.Pos, Quaternion.Euler(ikPoint2.Rot), Vector3.one);
				Matrix4x4 mat = localToWorldMatrix * matrix4x;
				Matrix4x4 mat2 = localToWorldMatrix2 * matrix4x2;
				Unity.Animator.SetIKPosition(ikGoal, Vector3.Lerp(mat.Translation(), mat2.Translation(), t));
				Unity.Animator.SetIKPositionWeight(ikGoal, weight * Mathf.Lerp(ikPoint1.PosWeight, ikPoint2.PosWeight, t));
				Unity.Animator.SetIKRotation(ikGoal, Quaternion.Slerp(mat.rotation, mat2.rotation, t));
				Unity.Animator.SetIKRotationWeight(ikGoal, weight * Mathf.Lerp(ikPoint1.RotWeight, ikPoint2.RotWeight, t));
			}
		}
	}

	public void UnityUpdateIK()
	{
		if (Unity.Animator == null)
		{
			Debug.LogWarning("UnityUpdateIK called on " + GetDisplayNameString() + " (" + (IsPredicted() ? "Predicted" : "Authoritative") + ") with no unity object");
			return;
		}
		Unity.Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
		Unity.Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
		float num = Unity.Animator.GetFloat(AnimHash.AimingAmount);
		float num2 = Unity.Animator.GetFloat(AnimHash.CrouchingAmount);
		EquippedModelProperties equippedModelProperties = ((EquippedItem != null) ? EquippedItem.GetPrototype().EquippedModelProperties : null);
		if (equippedModelProperties != null)
		{
			AvatarIKGoal ikGoal = ((equippedModelProperties.EquippedBone == Bone.RightHand) ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand);
			AvatarIKGoal ikGoal2 = ((equippedModelProperties.EquippedBone == Bone.RightHand) ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand);
			UnityUpdateIKPointLerped(ikGoal, equippedModelProperties.RightHandIKPoint, equippedModelProperties.CrouchingRightHandIKPoint, num2, num);
			UnityUpdateIKPoint(AnimHash.EquipmentLeftHandIK, ikGoal2, equippedModelProperties.LeftHandIKPoint);
			UnityUpdateIKPoint(AnimHash.PistolAimingIK, ikGoal2, equippedModelProperties.LeftHandIKPoint);
			UnityUpdateIKPoint(AnimHash.RifleLeftHandIK, ikGoal2, equippedModelProperties.LeftHandIKPoint);
			UnityUpdateIKPoint(AnimHash.BowRightHandIK, ikGoal2, equippedModelProperties.LeftHandIKPoint);
			UnityUpdateIKPoint(AnimHash.DiggingIK, ikGoal, equippedModelProperties.RightHandIKPoint);
			if (!IsAiming() && CurrentActionAnim != ActionAnim.Reload)
			{
				UnityUpdateIKPointLerped(ikGoal, equippedModelProperties.RunningRightHandIKPoint, equippedModelProperties.CrouchIdleRightHandIKPoint, num2, Math.Max(num2, Unity.Animator.GetFloat(AnimHash.RunningRightHandIK)));
			}
		}
		UnityUpdateIKPoint(AnimHash.DrinkingIK, AvatarIKGoal.RightHand, Equipment.EquipmentSettings.DrinkingIKPoint);
		UnityUpdateIKPoint(AnimHash.EatingIK, AvatarIKGoal.RightHand, Equipment.EquipmentSettings.EatingRightHandIKPoint);
		UnityUpdateIKPoint(AnimHash.EatingLeftHandIK, AvatarIKGoal.LeftHand, Equipment.EquipmentSettings.EatingLeftHandIKPoint);
		UnityUpdateIKPoint(AnimHash.WateringLeftHandIK, AvatarIKGoal.LeftHand, Equipment.EquipmentSettings.WateringLeftHandIKPoint);
		Unity.Animator.SetLookAtWeight((EquippedItem == null || EquippedItem is Throwable) ? num : 0f, 0.25f, 0.75f, 0f, 0.5f);
		Unity.Animator.SetLookAtPosition(Pos + Vector3.up * Height * Mathf.Lerp(0.75f, 0.4f, num2) + base.Forward);
	}

	private void UpdateFacialExpression()
	{
		if (Usage == CharacterUsage.IconGimp || Usage == CharacterUsage.PreviewGimp)
		{
			return;
		}
		TimeSpan playTime = Session.Instance.PlayTime;
		float num = (float)playTime.TotalSeconds;
		float num2 = 1f / 60f * (float)Session.Instance.GetFramesPerFrame();
		int frameCount = Time.frameCount;
		int lastUpdateFacialExpressionFrame = LastUpdateFacialExpressionFrame;
		LastUpdateFacialExpressionFrame = frameCount;
		bool flag = lastUpdateFacialExpressionFrame < frameCount - 1;
		Unity.UmaExpressionPlayer.WantExpensiveUpdate |= (base.EyePosition - Session.Instance.GameCamera.GetPos()).sqrMagnitude < ExpressionUpdateDistFromCamera * ExpressionUpdateDistFromCamera;
		if (UnityIsInAimingAnim() && EquippedItem is Pistol)
		{
			Unity.UmaExpressionPlayer.enableSaccades = false;
			Unity.UmaExpressionPlayer.leftEyeUp_Down = (Unity.UmaExpressionPlayer.rightEyeUp_Down = 0.65f);
		}
		else
		{
			Unity.UmaExpressionPlayer.enableSaccades = true;
		}
		FacialExpression facialExpression = FacialExpression.Neutral;
		float num3 = 0f;
		bool flag2 = false;
		if (Speaking != null && SpeakingTextEnglish != null)
		{
			float num4 = (float)(playTime - SpeechStartTime).TotalSeconds;
			float speechLipsMoveTime = StoryManager.GetSpeechLipsMoveTime(SpeakingTextEnglish);
			for (int i = 0; i < SpeakingEmoticons.Count && num4 / speechLipsMoveTime >= (float)SpeakingEmoticons[i].Pos / (float)SpeakingText.Length; i++)
			{
				facialExpression = SpeakingEmoticons[i].FacialExpression;
				num3 = 1f;
				flag2 = true;
			}
		}
		if (!flag2)
		{
			if (!base.Zombie)
			{
				float num5 = CalcMorale();
				if (num5 > 0f)
				{
					facialExpression = FacialExpression.Happy;
					num3 = 0.5f * num5 / 100f;
					if (InCombat)
					{
						facialExpression = FacialExpression.Aggressive;
					}
					else if (Hud.Instance.Pip.FocusObject == this && GetCurrentTarget() is Character character)
					{
						CalcApprovalRating(character, out var approval, out var _);
						if (approval < 0f)
						{
							facialExpression = FacialExpression.Suspicious;
						}
					}
				}
				else if (num5 < 0f)
				{
					facialExpression = FacialExpression.Sad;
					num3 = (0f - num5) / 100f;
					if (InCombat)
					{
						facialExpression = FacialExpression.Angry;
					}
					else if (Hud.Instance.Pip.FocusObject == this && GetCurrentTarget() is Character character2)
					{
						CalcApprovalRating(character2, out var approval2, out var _);
						facialExpression = ((!(approval2 < 0f)) ? FacialExpression.Hurt : FacialExpression.Resentful);
					}
				}
			}
			if (base.Zombie && InCombat && IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl))
			{
				facialExpression = FacialExpression.Snarl;
				num3 = 1f;
			}
			else if (!base.Zombie && IsVoiceSoundPlaying(VoiceSoundType.Exhausted))
			{
				facialExpression = FacialExpression.Exhausted;
				num3 = 1f;
			}
			if (base.Zombie && MovementType == MovementType.Run)
			{
				facialExpression = FacialExpression.Snarl;
				num3 = 1f;
			}
			switch (CurrentActionAnim)
			{
			case ActionAnim.ZombieBiteStart:
			case ActionAnim.ZombieBitePrepare:
			case ActionAnim.ZombieBiteFail:
			case ActionAnim.ZombieBiteFinish:
			case ActionAnim.ZombieJump:
			case ActionAnim.ZombieJumpBiteStart:
			case ActionAnim.ZombieJumpBitePrepare:
			case ActionAnim.ZombieJumpBiteFail:
			case ActionAnim.ZombieJumpBiteFinish:
				facialExpression = FacialExpression.Snarl;
				num3 = 1f;
				break;
			case ActionAnim.ZombieBiteLoop:
			case ActionAnim.ZombieJumpBiteLoop:
				facialExpression = FacialExpression.Snarl;
				num3 = (float)Math.Cos(GetActionAnimPlayedFrac() * (MathF.PI * 2f)) * 0.5f + 0.5f;
				break;
			case ActionAnim.RestrainedStruggle:
			case ActionAnim.RestrainedFree:
				facialExpression = FacialExpression.Angry;
				num3 = 1f;
				break;
			default:
				if (GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel && Speaking == null)
				{
					facialExpression = FacialExpression.Exhausted;
					num3 = 1f;
				}
				break;
			}
		}
		if (InvisibleStrain != InvisibleStrainType.None && IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl))
		{
			facialExpression = FacialExpression.Snarl;
			num3 = 1f;
		}
		float num6 = 0f;
		float num7 = 0f;
		float target = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float target2 = 0f;
		float target3 = 0f;
		float target4 = 0f;
		float num10 = 0f;
		switch (facialExpression)
		{
		case FacialExpression.Happy:
			target = num3;
			break;
		case FacialExpression.Threatening:
			num8 = num3;
			target = 0f - num8;
			break;
		case FacialExpression.Unhappy:
			target = 0f - num3;
			break;
		case FacialExpression.Suspicious:
			target2 = (0f - num3) * 0.75f;
			target3 = (target4 = num3 * 0.5f);
			break;
		case FacialExpression.Angry:
			num8 = num3;
			target = 0f - num8;
			target2 = 0f - num3;
			target3 = (target4 = num3 * 0.5f);
			break;
		case FacialExpression.Aggressive:
			target = num3;
			target2 = 0f - num3;
			target3 = (target4 = num3 * 0.5f);
			break;
		case FacialExpression.Resentful:
			target = 0f - num3;
			target2 = 0f - num3;
			target3 = (target4 = num3 * 0.5f);
			break;
		case FacialExpression.Wistful:
			target2 = num3;
			target3 = (target4 = 0f - num3);
			break;
		case FacialExpression.Sad:
			target = 0f - num3;
			target2 = num3;
			target3 = (target4 = 0f - num3);
			break;
		case FacialExpression.Moved:
			target = num3;
			target2 = num3;
			target3 = (target4 = 0f - num3);
			break;
		case FacialExpression.Hurt:
			num8 = num3;
			target = 0f - num8;
			target2 = num3;
			target3 = (target4 = 0f - num3);
			break;
		case FacialExpression.Surprised:
			target3 = (target4 = num3);
			target2 = num3;
			num10 = num3;
			num7 = 0.5f * num3;
			num6 = 0.25f * num3;
			break;
		case FacialExpression.Scared:
			target3 = (target4 = num3 * 0.5f);
			target2 = num3 * 0.5f;
			num10 = num3;
			break;
		case FacialExpression.Dismayed:
			target = 0f - num3;
			num10 = num3;
			target2 = num3;
			target3 = (target4 = (0f - num3) * 0.75f);
			break;
		case FacialExpression.Delighted:
			target = num3;
			num10 = num3;
			target3 = (target4 = num3 * 0.75f);
			target2 = num3 * 0.75f;
			break;
		case FacialExpression.Shocked:
			num8 = num3;
			target = 0f - num8;
			num10 = num3;
			target3 = (target4 = num3 * 0.75f);
			break;
		case FacialExpression.Exhausted:
			num9 = num3 * (0.5f + (float)Math.Sin(num * (MathF.PI * 2f)) * 0.125f);
			target2 = num3;
			target3 = (target4 = 0f - num3);
			num6 = 0.333f * num3;
			break;
		case FacialExpression.Snarl:
			num8 = num3;
			num10 = num3;
			target = 0f - num3;
			target2 = 0f - num3;
			target3 = (target4 = num3);
			num6 = num3 * 0.65f;
			break;
		case FacialExpression.Quizical:
			target4 = num3;
			break;
		case FacialExpression.Amused:
			target = num3;
			target4 = num3;
			break;
		case FacialExpression.Unimpressed:
			target = 0f - num3;
			target4 = num3;
			break;
		case FacialExpression.Incredulous:
			num8 = num3;
			target = 0f - num8;
			target4 = num3;
			break;
		}
		bool flag3 = false;
		if (Speaking != null)
		{
			float num11 = (float)(playTime - SpeechStartTime).TotalSeconds;
			float speechLipsMoveTime2 = StoryManager.GetSpeechLipsMoveTime(SpeakingTextEnglish);
			if (num11 <= speechLipsMoveTime2)
			{
				string speakingText = SpeakingText;
				float num12 = num11 / speechLipsMoveTime2;
				int num13 = (int)(num12 * (float)speakingText.Length);
				for (int j = Math.Max(0, num13 - 3); j <= Math.Min(speakingText.Length - 1, num13 + 3); j++)
				{
					float num14 = (1f - Math.Abs((float)j / (float)speakingText.Length - num12)) / 2f;
					float num15 = 0f;
					float num16 = 0f;
					switch (speakingText[j])
					{
					case 'A':
					case 'a':
					case 'Á':
					case 'á':
					case 'Ą':
					case 'ą':
					case 'А':
					case 'Я':
					case 'а':
					case 'я':
						num15 = 0.8f;
						num16 = 0f;
						break;
					case 'E':
					case 'e':
					case 'É':
					case 'é':
					case 'Ę':
					case 'ę':
					case 'Ё':
					case 'Е':
					case 'Э':
					case 'е':
					case 'э':
					case 'ё':
						num15 = 0.6f;
						num16 = 0.2f;
						break;
					case 'I':
					case 'i':
					case 'Í':
					case 'í':
					case 'И':
					case 'и':
						num15 = 0.5f;
						num16 = 0.1f;
						break;
					case 'O':
					case 'o':
					case 'Ó':
					case 'Ö':
					case 'ó':
					case 'ö':
					case 'Ő':
					case 'ő':
						num15 = 0f;
						num16 = 0.8f;
						break;
					case 'U':
					case 'u':
					case 'Ú':
					case 'Ü':
					case 'ú':
					case 'ü':
					case 'Ű':
					case 'ű':
					case 'У':
					case 'Ы':
					case 'Ю':
					case 'у':
					case 'ы':
					case 'ю':
						num15 = 0.1f;
						num16 = 0.5f;
						break;
					default:
						if (speakingText[j] >= '一')
						{
							num15 = ((j % 2 == 1) ? 0f : Mathf.Sqrt(MathUtil.RandomFloat((int)speakingText[j])));
							num16 = MathUtil.Squared(MathUtil.RandomFloat(speakingText[j] + 45645));
						}
						break;
					}
					num6 = Math.Min(num6 + num15 * num14, 1f) * 0.5f;
					num7 = Math.Min(num7 + num16 * num14, 1f);
					flag3 = true;
				}
			}
		}
		else if (base.Zombie && IsVoiceSoundPlaying(VoiceSoundType.ZombieSnarl))
		{
			num6 = (float)Math.Sin(num) * 0.5f + 0.5f;
			flag3 = true;
		}
		float num17 = ((facialExpression == FacialExpression.Neutral) ? 1f : 0.25f);
		float num18 = num2 / num17;
		float speed = (flag3 ? (num2 / 0.05f) : num18);
		if (flag)
		{
			num18 = (speed = 1000f);
		}
		Unity.UmaExpressionPlayer.midBrowUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.midBrowUp_Down, target2, num18);
		Unity.UmaExpressionPlayer.leftBrowUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.leftBrowUp_Down, target3, num18);
		Unity.UmaExpressionPlayer.rightBrowUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.rightBrowUp_Down, target4, num18);
		Unity.UmaExpressionPlayer.leftMouthSmile_Frown = MathUtil.Delt(Unity.UmaExpressionPlayer.leftMouthSmile_Frown, target, num18);
		Unity.UmaExpressionPlayer.rightMouthSmile_Frown = MathUtil.Delt(Unity.UmaExpressionPlayer.rightMouthSmile_Frown, target, num18);
		Unity.UmaExpressionPlayer.leftUpperLipUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.leftUpperLipUp_Down, num8, num18);
		Unity.UmaExpressionPlayer.rightUpperLipUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.rightUpperLipUp_Down, num8, num18);
		Unity.UmaExpressionPlayer.noseSneer = MathUtil.Delt(Unity.UmaExpressionPlayer.noseSneer, num8, num18);
		Unity.UmaExpressionPlayer.leftLowerLipUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.leftLowerLipUp_Down, num9, num18);
		Unity.UmaExpressionPlayer.rightLowerLipUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.rightLowerLipUp_Down, num9, num18);
		Unity.UmaExpressionPlayer.tongueOut = MathUtil.Delt(Unity.UmaExpressionPlayer.tongueOut, num9 * 0.35f, num18);
		Unity.UmaExpressionPlayer.tongueUp_Down = MathUtil.Delt(Unity.UmaExpressionPlayer.tongueUp_Down, num9 * 0.25f, num18);
		if (num10 > 0f || Unity.UmaExpressionPlayer.leftEyeOpen_Close > 0f || Unity.UmaExpressionPlayer.rightEyeOpen_Close > 0f)
		{
			Unity.UmaExpressionPlayer.leftEyeOpen_Close = MathUtil.Delt(Unity.UmaExpressionPlayer.leftEyeOpen_Close, num10, num18);
			Unity.UmaExpressionPlayer.rightEyeOpen_Close = MathUtil.Delt(Unity.UmaExpressionPlayer.rightEyeOpen_Close, num10, num18);
		}
		Unity.UmaExpressionPlayer.jawOpen_Close = MathUtil.Delt(Unity.UmaExpressionPlayer.jawOpen_Close, num6, speed);
		Unity.UmaExpressionPlayer.mouthNarrow_Pucker = MathUtil.Delt(Unity.UmaExpressionPlayer.mouthNarrow_Pucker, num7, speed);
	}

	public override string GetUnityBoneName(Bone bone)
	{
		string text = null;
		switch (bone)
		{
		case Bone.WorldPos:
			return string.Empty;
		case Bone.Root:
			return "Root";
		case Bone.Global:
			return "Root/Global";
		case Bone.Position:
			return "Root/Global/Position";
		case Bone.Hips:
			return "Root/Global/Position/Hips";
		case Bone.LowerBack:
			return "Root/Global/Position/Hips/LowerBack";
		case Bone.Spine:
			return "Root/Global/Position/Hips/LowerBack/Spine";
		case Bone.Spine1:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1";
		case Bone.Neck:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck";
		case Bone.Head:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/Neck/Head";
		case Bone.LeftUpLeg:
			return "Root/Global/Position/Hips/LeftUpLeg";
		case Bone.LeftLeg:
			return "Root/Global/Position/Hips/LeftUpLeg/LeftLeg";
		case Bone.LeftFoot:
			return "Root/Global/Position/Hips/LeftUpLeg/LeftLeg/LeftFoot";
		case Bone.LeftShoulder:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder";
		case Bone.LeftArm:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm";
		case Bone.LeftForearm:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm";
		case Bone.LeftHand:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand";
		case Bone.RightUpLeg:
			return "Root/Global/Position/Hips/RightUpLeg";
		case Bone.RightLeg:
			return "Root/Global/Position/Hips/RightUpLeg/RightLeg";
		case Bone.RightFoot:
			return "Root/Global/Position/Hips/RightUpLeg/RightLeg/RightFoot";
		case Bone.RightShoulder:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder";
		case Bone.RightArm:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm";
		case Bone.RightForearm:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm";
		case Bone.RightHand:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand";
		case Bone.MeleeWeapon:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/RightShoulder/RightArm/RightForeArm/RightHand/Weapon";
		case Bone.BowString:
			return "Root/Global/Position/Hips/LowerBack/Spine/Spine1/LeftShoulder/LeftArm/LeftForeArm/LeftHand/Weapon/Bow_Group/Bow_Base/Bow_String";
		default:
			Debug.LogWarning(GetDisplayNameString() + " trying to get invalid bone name: " + bone);
			return "Root";
		}
	}

	public override HitBox[] GetHitBoxes()
	{
		return HitBoxSettings.HitBoxes;
	}

	public override float GetUnityHeadHeight()
	{
		return Mathf.Lerp(Mathf.Lerp(Appearance.Height, Appearance.CrouchingHeight + ((EquippedItem is Bow) ? 0.2f : 0f), UnityCrouchingTransition), Appearance.CrouchingHeight, UnitySittingTransition);
	}

	public override int GetUnityIdleAnimHash()
	{
		if (!base.IsConscious && CurrentAnimState == AnimState.Animation)
		{
			switch (MathUtil.RandomInt(Id, 10))
			{
			case 0:
				return AnimHash.PlayDead1;
			case 1:
				return AnimHash.PlayDead2;
			case 2:
				return AnimHash.PlayDead3;
			case 3:
				return AnimHash.PlayDead4;
			case 4:
				return AnimHash.PlayDead5;
			case 5:
				return AnimHash.PlayDead6;
			case 6:
				return AnimHash.PlayDead7;
			case 7:
				return AnimHash.PlayDead8;
			case 8:
				return AnimHash.PlayDead9;
			case 9:
				return AnimHash.PlayDead10;
			}
		}
		bool flag = CrouchingTransition >= 0.5f;
		bool flag2 = AimingTransition >= 0.5f;
		if (!Sitting)
		{
			if (!base.Zombie)
			{
				if (!(EquippedItem is Throwable))
				{
					if (!(EquippedItem is Bow))
					{
						if (!(EquippedItem is LongGun))
						{
							if (!(EquippedItem is Pistol))
							{
								if (!(EquippedItem is MeleeWeapon))
								{
									if (!flag)
									{
										if (!flag2)
										{
											return AnimHash.Idle;
										}
										return AnimHash.UnarmedIdleAiming;
									}
									if (!flag2)
									{
										return AnimHash.CrouchIdle;
									}
									return AnimHash.UnarmedCrouchIdleAiming;
								}
								if (!flag)
								{
									if (!flag2)
									{
										return AnimHash.OneHandedIdle;
									}
									return AnimHash.OneHandedIdleAiming;
								}
								if (!flag2)
								{
									return AnimHash.OneHandedCrouchIdle;
								}
								return AnimHash.OneHandedCrouchIdleAiming;
							}
							if (!flag)
							{
								if (!flag2)
								{
									return AnimHash.PistolIdle;
								}
								return AnimHash.PistolIdleAiming;
							}
							if (!flag2)
							{
								return AnimHash.PistolCrouchIdle;
							}
							return AnimHash.PistolCrouchIdleAiming;
						}
						if (!flag)
						{
							if (!flag2)
							{
								return AnimHash.RifleIdle;
							}
							return AnimHash.RifleIdleAiming;
						}
						if (!flag2)
						{
							return AnimHash.RifleCrouchIdle;
						}
						return AnimHash.RifleCrouchIdleAiming;
					}
					if (!flag)
					{
						if (!flag2)
						{
							return AnimHash.BowIdle;
						}
						return AnimHash.BowIdleAiming;
					}
					if (!flag2)
					{
						return AnimHash.BowCrouchIdle;
					}
					return AnimHash.BowCrouchIdleAiming;
				}
				if (!flag)
				{
					if (!flag2)
					{
						return AnimHash.ThrowableIdle;
					}
					return AnimHash.ThrowableIdleAiming;
				}
				if (!flag2)
				{
					return AnimHash.ThrowableCrouchIdle;
				}
				return AnimHash.ThrowableCrouchIdleAiming;
			}
			return AnimHash.Idle;
		}
		return AnimHash.Sitting;
	}

	public override void OnMovedByCollisionManager()
	{
		Session instance = Session.Instance;
		if ((instance.GameCamera.Focus - Position).sqrMagnitude >= Character.FootstepSoundDist * Character.FootstepSoundDist)
		{
			return;
		}
		float num = Mathf.Lerp(0.75f, 1f, Mathf.Clamp01(MovementSpeed / Character.RunSpeed)) * BushRustleVolume;
		if (num == 0f)
		{
			return;
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (currentTime - instance.LastBushRustleTime < TimeSpan.FromSeconds(Mathf.Lerp(MinTimeBetweenBushRustles, MaxTimeBetweenBushRustles, MathUtil.RandomFloat((float)instance.LastBushRustleTime.TotalSeconds))) || !(GameTerrain.Instance.GetClosestObjectInRange(base.Tile, 1, (TileObject obj) => obj.GetBaseObjectType() == BaseObjectType.Bush) is Bush bush) || (bush.Pos - Position).sqrMagnitude >= Bush.RustleRadius * Bush.RustleRadius)
		{
			return;
		}
		Vector2 rhs = MathUtil.ToXZ(Position - OldPosition);
		if (!(Vector2.Dot(MathUtil.ToXZ(bush.Pos - OldPosition), rhs) <= 0f) && CheckFrontmostPrediction(PredictedEventType.BushRustle))
		{
			if (IsCrouching())
			{
				num *= 0.75f;
			}
			SoundManager.PlaySound3DFromList(SoundManager.BushRustleSounds, bush.Pos + Vector3.up, num * Character.FootstepSoundVolume, isBackground: false, isBush: true);
			instance.LastBushRustleTime = currentTime;
		}
	}

	public int CalcAccompaniedAmountInLabor()
	{
		int num = 0;
		if (InsideBuilding != null && !InTerrain)
		{
			Character[] inhabitants = InsideBuilding.Inhabitants;
			foreach (Character character in inhabitants)
			{
				if (character != null && character != this)
				{
					num += character.GetSkillLevelWithEffects(SkillType.Medicine);
				}
			}
		}
		return num;
	}

	protected override void SimulateSurvivalFactors(TimeSpan curTime, float dts)
	{
		base.SimulateSurvivalFactors(curTime, dts);
		if (Pregnant)
		{
			SetPregnancyProgression(Math.Min(1f, PregnancyProgression + dts / (Sun.DayLengthSecs * (float)Weather.DaysInAMonth * PregnancyMonths)));
			if (PregnancyProgression >= 1f)
			{
				float num = (float)CalcAccompaniedAmountInLabor() / 5f;
				float bloodLoss = (1f - num) * dts / UnaccompaniedDieInChildbirthTime;
				int constitution = 0;
				ApplyBloodLoss2(bloodLoss, 0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, constitution, SecrecyMode.Public);
				if (!base.Alive && IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.DiedInChildbirth, null, this, 1f, secret: false);
				}
			}
		}
		else if (PregnancyProgression > 0f)
		{
			SetPregnancyProgression(Math.Max(0f, PregnancyProgression - dts / Sun.DayLengthSecs));
		}
	}

	public void SetPregnancyProgression(float v)
	{
		PregnancyProgression = v;
		if (Mathf.Abs(PregnancyProgression - GetAppearance().Bones.Pregnancy) > 0.01f)
		{
			GetAppearance().Bones.Pregnancy = PregnancyProgression;
			GetAppearance().SetupDNA();
			if (Unity.Obj != null && Usage != CharacterUsage.PreviewGimp)
			{
				UnityOnChangedBones();
			}
		}
	}

	public void SetPregnant(bool v)
	{
		Pregnant = v;
		if (!Pregnant)
		{
			SetPregnancyProgression(PregnancyProgression * 0.5f);
		}
	}
}
