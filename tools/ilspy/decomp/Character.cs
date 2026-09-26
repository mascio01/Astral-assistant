using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UMA.PoseTools;
using UnityEngine;
using UnityEngine.UI;

public abstract class Character : MultiTileObject
{
	public struct UnityState
	{
		public GameObject Obj;

		public Resource<GameObject> Prefab;

		public List<MeshRenderer> MeshRenderers;

		public List<SkinnedMeshRenderer> SkinnedMeshRenderers;

		public List<Canvas> Canvases;

		public SkinnedMeshRenderer BackpackSkinnedMeshRenderer;

		public Transform[] Bones;

		public GameObject SelectionCircleObj;

		public GameObject WeaponObj;

		public GameObject WeaponModel;

		public GameObject LeftHandItemObj;

		public GameObject LeftHandItemModel;

		public GameObject LoadedAmmoObj;

		public GameObject LoadedAmmoModel;

		public GameObject CollisionShape;

		public UnconsciousBehaviour UnconsciousEffect;

		public List<InjuryBehaviour> InjuryObjects;

		public ParticleSystem SlidingEffect;

		public ParticleSystem SlidingDebris;

		public ParticleSystem MuzzleFlash;

		public AudioSource VoiceAudioSource;

		public AudioSource OneShotAudioSource;

		public AudioSource FootstepAudioSource;

		public VoiceSoundType CurrentVoiceSoundType;

		public Animator Animator;

		public Animator WeaponAnimator;

		public RawImage FocusedArrow;

		public RawImage FocusedArrowLarge;

		public RawImage OverheadActionIcon;

		public RawImage CraftingProgressIndicator;

		public GameObject PlayerNames;

		public GameObject BloodLossBar;

		public SpeechBubbleBehaviour SpeechBubble;

		public UMAExpressionPlayer UmaExpressionPlayer;

		public AnimState UnityAnimState;

		public ActionAnim UnityActionAnim;

		public float OverheadIconsTopY;

		public float AimingUpDownAngle;

		public bool WantUpdateWhenOffscreen;

		public bool IsValid()
		{
			return Obj != null;
		}

		public bool IsWaitingForUMACharacterCreation()
		{
			if (Obj != null)
			{
				return Animator == null;
			}
			return false;
		}

		public void SetWantUpdateWhenOffscreen(bool v)
		{
			if (WantUpdateWhenOffscreen == v || SkinnedMeshRenderers == null)
			{
				return;
			}
			WantUpdateWhenOffscreen = v;
			Animator.cullingMode = ((!WantUpdateWhenOffscreen) ? AnimatorCullingMode.CullUpdateTransforms : AnimatorCullingMode.AlwaysAnimate);
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in SkinnedMeshRenderers)
			{
				skinnedMeshRenderer.updateWhenOffscreen = WantUpdateWhenOffscreen;
			}
		}
	}

	public struct RagdollBoneInfo
	{
		public string Path;

		public string ConnectedPath;
	}

	public class SortMemoriesByPriorityDescending : IComparer<Memory>
	{
		int IComparer<Memory>.Compare(Memory a, Memory b)
		{
			if (a.Priority < b.Priority)
			{
				return 1;
			}
			if (a.Priority > b.Priority)
			{
				return -1;
			}
			return 0;
		}
	}

	private class SortCarryPolicies : IComparer<EquipmentPolicy>
	{
		int IComparer<EquipmentPolicy>.Compare(EquipmentPolicy a, EquipmentPolicy b)
		{
			bool flag = a.Proto != null && a.Proto.IsAmmo();
			bool flag2 = b.Proto != null && b.Proto.IsAmmo();
			if (!flag && flag2)
			{
				return 1;
			}
			if (flag && !flag2)
			{
				return -1;
			}
			float num = ((a.Proto != null) ? a.Proto.BasePrice : ((a.Liquid != null) ? (a.Liquid.BasePricePerFlOz * 50f) : 0f));
			float num2 = ((b.Proto != null) ? b.Proto.BasePrice : ((b.Liquid != null) ? (b.Liquid.BasePricePerFlOz * 50f) : 0f));
			if (num < num2)
			{
				return 1;
			}
			if (num > num2)
			{
				return -1;
			}
			return 0;
		}
	}

	public enum MysteriousAttackType
	{
		None,
		Hurt,
		KnockedOut,
		Killed,
		PropertyDamage
	}

	public static string[] RankNames = StringUtil.GetEnumNames<Rank>();

	public static string[] RoleNames = StringUtil.GetEnumNames<Role>();

	public static string[] MovementTypeNames = StringUtil.GetEnumNames<MovementType>();

	public static string[] CauseOfDeathNames = StringUtil.GetEnumNames<CauseOfDeath>();

	public static string[] ClothingTypeNames = StringUtil.GetEnumNames<ClothingType>("Invalid");

	public static string[] SparringTypeNames = StringUtil.GetEnumNames<SparringType>();

	public static string[] CanOpenGatesNames = StringUtil.GetEnumNames<CanOpenGates>();

	public static string[] SpeechSituationNames = StringUtil.GetEnumNames<SpeechSituation>();

	public static string[] SurrenderModeNames = StringUtil.GetEnumNames<SurrenderMode>();

	public UnityState Unity;

	public bool CanSkipUnityUpdate;

	public CharacterUsage Usage;

	public static float HumanRadius = 0.35f;

	public const float MaxAnimRadius = 0.95f;

	public const float MaxAnimHeight = 2.5f;

	public static float WalkSpeed = 1.5f;

	public static float WalkInjuredSpeed = 1f;

	public static float CrouchWalkSpeed = 1.5f;

	public static float CrouchOutOfBreathRunSpeed = 2f;

	public static float CrouchRunSpeed = 3f;

	public static float ZombieWalkSpeed = 0.579f;

	public static float ZombieWalkInjuredSpeed = 1f;

	public static float ZombieRunSpeed = 6f;

	public static float JogSpeed = 3f;

	public static float OutOfBreathRunSpeed = 4f;

	public static float RunSpeed = 6f;

	public static float RunInjuredSpeed = 4.5f;

	public static float OutOfBreathRunInjuredSpeed = 3.5f;

	public static float RotSpeed = MathF.PI * 4f;

	public static float SlipperySlopeSpeedFactor = 10f;

	public static float AimPenaltyWhenTargetIsMovingLaterally = -2f;

	public static float AimPenaltyWhenTargetIsMovingAway = -1f;

	public static int BaseSightRange = 15;

	public static int BaseUnconsciousSightRange = 4;

	public static int ZombieSightRange = 16;

	public static int ZombiePlayDeadSightRange = 4;

	public static float FatigueRecoverRate = 0.05f;

	public static float RunningFatigueRate = 0.0375f;

	public static float CarryingFatigueRatePerLb = 0.0002f;

	public static float DodgingFatiguePenalty = 0.1f;

	public static float MeleeAttackFatiguePenalty = 0.1f;

	public static float BloodLossRate = 0.002f;

	public static float BloodLossRecoverRate = 1f / (Sun.DayLengthSecs * 2.5f);

	public static float BloodLossFactorMinConstitution = 1f;

	public static float BloodLossFactorMaxConstitution = 0.5f;

	public static float BurningDamageRate = 0.1f;

	public static float ZombieBurningDamageRate = 0.2f;

	public static float ZombieBiteDamage = 0.2f;

	public static float CriticalBloodLossLevel = 0.9f;

	public static float ExhaustedFatigueLevel = 0.9f;

	public static float HungryTime = Sun.DayLengthSecs * 1f;

	public static float HungerCriticalTime = Sun.DayLengthSecs * 3f;

	public static float HungerExtraCriticalTime = Sun.DayLengthSecs * 6f;

	public static float HungerDieTime = Sun.DayLengthSecs * 7f;

	public static float NeedToiletTime = Sun.DayLengthSecs;

	public static float WetPantsTime = Sun.DayLengthSecs * 2f;

	public static float ThirstyTime = Sun.DayLengthSecs * 0.5f;

	public static float ThirstCriticalTime = Sun.DayLengthSecs * 1f;

	public static float ThirstExtraCriticalTime = Sun.DayLengthSecs * 1.75f;

	public static float ThirstDieTime = Sun.DayLengthSecs * 2f;

	public static float WaterNeededPerDayInFlOz = 50f;

	public static float SleepyTime = Sun.DayLengthSecs * 0.75f;

	public static float SleepDeprivationCriticalTime = Sun.DayLengthSecs * 2f;

	public static float SleepDeprivationExtraCriticalTime = Sun.DayLengthSecs * 6f;

	public static float SleepDeprivationDieTime = Sun.DayLengthSecs * 7f;

	public static float SleepRecoverRate = 3f;

	public static float BodyTemperatureInCelsiusHyperpyrexia = 40f;

	public static float BodyTemperatureInCelsiusFever = 38f;

	public static float BodyTemperatureInCelsiusNormal = 37f;

	public static float BodyTemperatureInCelsiusShivering = 36f;

	public static float BodyTemperatureInCelsiusMildHypothermia = 35f;

	public static float BodyTemperatureInCelsiusModerateHypothermia = 32f;

	public static float BodyTemperatureInCelsiusWakeUp = 30f;

	public static float BodyTemperatureInCelsiusUnconscious = 28f;

	public static float BodyTemperatureInCelsiusDeath = 20f;

	public static float BACEuphoria = 0.1f;

	public static float BACExcitement = 0.2f;

	public static float BACConfusion = 0.25f;

	public static float BACStupor = 0.3f;

	public static float BACComa = 0.4f;

	public static float BACDeath = 0.45f;

	public static float GathererCloseToHomeDist = 50f;

	public static float InfectionProgressionCriticalLevel = 0.75f;

	public static float ZombificationRate = 0.1f;

	public static TimeSpan MinZombificationTime = TimeSpan.FromSeconds(10.0);

	public static float CremationTime = 60f;

	public static float AdrenalineTimeout = 10f;

	public static float PickpocketForgetSpeed = 100f / Sun.DayLengthSecs;

	public static float PunchDamage = 0.05f;

	public static float KickDamage = 0.08f;

	public static float QuickAttackDamageModifier = 0.5f;

	public static float FisticuffsWinBloodLoss = 0.5f;

	public static float FeudingMaxBloodLoss = 0.95f;

	public static float FisticuffsCancelDist = 30f;

	public static int SnowballFightWinHits = 8;

	public Character Predicted;

	public Character Authoritative;

	public int PredictedFreshFrame = -1;

	public byte[] PredictedActionCounter = new byte[34];

	public List<PredictedEvent> PredictedEvents;

	public Vector3 Position;

	public Vector3 OldPosition;

	public Vector2 VelocityXZ;

	public Vector2 VelocityXZBeforeCollisionDetection;

	public float FacingAngle;

	public float DesiredFacingAngle;

	public bool IsInMapWho;

	public bool IsInBurningMapWho;

	public bool WantKilledByPitTrap;

	public CharacterAppearance Appearance;

	public bool PlayDead;

	public bool Disappeared;

	public bool HiddenOnMap;

	public bool KeptAroundForReferences;

	public bool HasTravelledFromAnotherMap;

	public int OriginalId;

	public Consciousness Consciousness;

	public float SkinnedAmount;

	public CauseOfDeath CauseOfDeath;

	public bool Rotten;

	public bool WasAttackedBeforeLastCeaseFire;

	public Vector3 PlaceOfDeath;

	public TimeSpan TimeOfDeath = Target.Never;

	public Character Killer;

	public TimeSpan LastKnockedOutTime = Target.Never;

	public TimeSpan LastRevivedTime = Target.Never;

	public Character LastPersonChokedMe;

	public Grave BuriedInGrave;

	public float PickpocketDetection;

	public List<GatheredItem> PickpocketedItems;

	public InfectionType Infection;

	public InvisibleStrainType InvisibleStrain;

	public TimeSpan InvisibleStrainJustActivated;

	public TimeSpan InvisibleStrainNextSpread;

	public TimeSpan InvisibleStrainSnarlStartTime;

	public bool HadInvisibleStrainFromStart;

	public Character DontInfectWithInvisibleStrain;

	public List<Injury> Injuries = new List<Injury>();

	public float Fuel;

	public float CremationProgression;

	public List<QueuedSpeech> QueuedSpeeches = new List<QueuedSpeech>();

	public List<SpeechMemory> SpokenSpeeches = new List<SpeechMemory>();

	public List<Timer> Timers = new List<Timer>();

	public Speech Speaking;

	public Character Listener;

	public BaseObject SpeechObject;

	public MemoryParam SpeechParam;

	public TimeSpan SpeechStartTime;

	public string SpeakingText;

	public string SpeakingTextEnglish;

	public List<SpeechEmoticon> SpeakingEmoticons = new List<SpeechEmoticon>();

	public List<SpeechParamResult> SpeakingParamResults = new List<SpeechParamResult>();

	private SpeechAnimState CurrentSpeechAnimState;

	public AnimState CurrentAnimState;

	public Trans[] ProneBoneTransforms = new Trans[24];

	public ActionAnim CurrentActionAnim;

	public AnimWrapper AnimWrapper;

	public TimeSpan AnimStartTime;

	public float AnimSpeed = 1f;

	public float AnimHeight;

	public TileObject InteractionObject;

	public TileObject CarryingObject;

	public int CommunityNoticedBodySnatching;

	public TimeSpan LastRescuedByPlayer = Target.Never;

	public Character CarriedBy;

	public Character ParryingAttacker;

	public TileObject DodgingProjectile;

	public CanAttackState CanAttackJumpingZombie;

	public CanAttackState CanAttackGrabbingZombie;

	public bool IsCurrentActionAnimFromGoal;

	public TimeSpan LastFireTime;

	public TimeSpan LastAttackedSomeoneTime;

	public TimeSpan DangerousToRescueMeTime = Target.Never;

	public Character DangerousToRescueMeBecause;

	public Character BeingRescuedBy;

	public SparringType SparringType;

	public Character SparringPartner;

	public Character SparringInstigator;

	public float SparringPartnerInitialBloodLoss;

	public int SparringSnowballHits;

	public List<Target> Targets = new List<Target>();

	public List<Character> CharactersTargetingMe = new List<Character>();

	public Target GoalTarget;

	public Target DirectControlledTarget;

	public bool DirectControlledAiming;

	public TimeSpan DirectControlledAimingStartTime;

	public int GoalAimingRefCount;

	public bool DisableFistsAiming;

	public bool DirectControlledCrouching;

	public int GoalCrouchingRefCount;

	public bool Sitting;

	public TerrainCoord SittingAround;

	public Community Community;

	public Community InitialCommunity;

	public TimeSpan JoinedCommunityTime = Target.Never;

	public int SquadId;

	public Skillset Skillset;

	public Rank Rank;

	public Character VotedFor;

	public SurrenderMode SurrenderMode;

	public List<RoleInfo> Roles = new List<RoleInfo>();

	public int MostRecentRoleIndex = -1;

	public bool GuardDuty;

	public bool PlantNewCrops = true;

	public float DownTime;

	public TerrainCoord HangOutLocation = TerrainCoord.Invalid;

	public TerrainCoord InitialHangOutLocation = TerrainCoord.Invalid;

	public bool CanFollowPlayer;

	public Character SquadLeader;

	public List<Character> Followers;

	public int FollowMeMoveFrame;

	public Vector2 FollowMePosXZ;

	public float FollowMeDirAngle;

	public TileObject ScriptedGoalMarker;

	public RecentActivityType RecentActivityType;

	public TimeSpan RecentActivityTime;

	public Character RecentActivityWith;

	public Vector3 RecentActivityPos;

	private Goal Goal;

	private bool IsDirectControlled;

	public bool DirectControlledMajorAIDisabled;

	private TimeSpan LastDirectControlInputActionTime = TimeSpan.FromDays(-365.0);

	private TimeSpan LastPlayerCommandTime = TimeSpan.FromDays(-365.0);

	public PlayerID AvatarForPlayer;

	public List<TerrainCoord> Route = new List<TerrainCoord>();

	public MovementType MovementType;

	public MovementMode MovementMode;

	public float FlankingDesiredRange;

	public bool WantAvoidance;

	public bool Tired;

	public float MovementSpeed;

	public float MovementAngle;

	public int SprintCountdown;

	public TerrainRect MovementZone = TerrainRect.Invalid;

	public bool MovementZonePaused;

	public Building InsideBuilding;

	public int InsideBuildingSlotIndex = -1;

	public bool WasOrderedInsideBuilding;

	public TimeSpan LastEnteredOrExitedBuildingTime;

	public int DisableSleep;

	public Building RagdollIgnoreBuilding;

	private float Fade;

	private float FadeTarget;

	private float FadeTime;

	public Equipment EquippedItem;

	public Equipment DesiredEquippedItem;

	public Equipment LeftHandEquippedItem;

	public Equipment[] Clothes = new Equipment[9];

	public EquipmentContainer Inventory = new EquipmentContainer();

	public bool Encumbered;

	public bool HalfDemolishLoot;

	public float CachedInventoryWeight;

	public EquipmentPrototype DontSellItemTypeToPlayer;

	public bool NonDeterministicSelected;

	public bool CachedShortcutMeleeWeaponValid;

	public bool CachedShortcutAmmoWeaponValid;

	public bool CachedShortcutThrowableValid;

	public Equipment CachedShortcutMeleeWeapon;

	public Equipment CachedShortcutAmmoWeapon;

	public Equipment CachedShortcutThrowable;

	public EquipmentPrototype CachedShortcutAmmoType;

	public InfectionType CachedShortcutAmmoInfectedWith;

	public ThinkPriority ThinkPriorityBucket = ThinkPriority.Dead;

	public TimeSpan LastUpdateTime;

	public TimeSpan LastThinkTime;

	public TimeSpan LastMemoryUpdateTime;

	public TimeSpan LastSkillAtrophyTime;

	public bool DisableInputsUntilNextThink;

	public int NumUpdateFrames;

	public int FramesPerUpdate = 1;

	public bool WasCrouching;

	public bool WasAiming;

	public float CrouchingTransition;

	public float AimingTransition;

	public bool InCombat;

	public bool AboutToBeInCombat;

	public bool OrderedToAttack;

	public bool Surrendering;

	public int UnderAttackRefCount;

	private int LastVisibleInputFrame;

	private bool NeedsUpdateEveryFrame;

	public VisibleTilesCache VisibleTilesCache;

	public string FirstName = string.Empty;

	public string Surname = string.Empty;

	public StringStatus FirstNameVerified;

	public StringStatus SurnameVerified;

	public string LootLocation = string.Empty;

	public List<string> Personality = new List<string>();

	private int CachedPersonality;

	public List<Memory> Memories = new List<Memory>();

	public List<Memory> SparringMemories = new List<Memory>();

	public List<int> SparringRemovedArmorIds;

	public List<EmpathyOverride> EmpathyOverrides = new List<EmpathyOverride>();

	public List<FindAttempt> FindAttempts = new List<FindAttempt>();

	public List<ExitBuildingAttempt> ExitBuildingAttempts = new List<ExitBuildingAttempt>();

	public List<EquipmentPolicy> EquipmentPolicies = new List<EquipmentPolicy>();

	public List<EquipmentPrototype> BooksRead;

	public List<Relationship> Relationships = new List<Relationship>();

	public bool NameKnown;

	public bool BackgroundKnown;

	public bool BrainScanned;

	public uint PersonalityKnown;

	public bool InjuryRemarkedOn;

	public bool InfectionRemarkedOn;

	public bool EulogyGiven;

	public bool Investigated;

	public bool God;

	public bool DisablePrediction;

	public bool DontSimulateSurvivalFactorsUntilDiscovered;

	public bool DontSimulateSurvivalFactorsUntilJoinCommunity;

	public bool DontLeaveCommunity;

	public bool DontAbandonPlayer;

	public bool AlwaysActivateInvisibleStrain;

	public float LimitBloodLoss;

	public bool DontBury;

	public bool Boxer;

	public bool KnownBoxer;

	public float Fatigue;

	public float Adrenaline;

	public float BloodLoss;

	public float Hunger;

	public float Thirst;

	public float Toilet;

	public float SleepDeprivation;

	public float BodyTemperatureInCelsius = BodyTemperatureInCelsiusNormal;

	public float TemperatureInsulationDelta;

	public float BloodAlcoholConcentration;

	public float SedativeEffect;

	public float Excitement;

	public float LungCancer;

	public float InfectionProgression;

	public float AimingAccuracy;

	public float ZombieEscapePower;

	public float ChokePower;

	public TimeSpan LastChokeTime;

	public int ReservedGoldAmount;

	public TimeSpan LastDrankAlcoholTime;

	public TimeSpan EncouragementTimeout;

	public TimeSpan LastUsedMolotovTime;

	public TimeSpan LastUsedPipeBombTime;

	public LogEventFeed Feed;

	public static string JoeWheeler = "JoeWheeler";

	public static string CooperMcClure = "CooperMcClure";

	public static int NAME_Stranger = StringUtil.JenkinsHash("NAME_Stranger");

	public static int NAME_Zombie = StringUtil.JenkinsHash("NAME_Zombie");

	public static int NAME_Looter = StringUtil.JenkinsHash("NAME_Looter");

	public static int NAME_Stranger_Female = StringUtil.JenkinsHash("NAME_Stranger_Female");

	public static int NAME_Zombie_Female = StringUtil.JenkinsHash("NAME_Zombie_Female");

	public static int NAME_Looter_Female = StringUtil.JenkinsHash("NAME_Looter_Female");

	public static int HUD_GreenStrain = StringUtil.JenkinsHash("HUD_GreenStrain");

	public static int HUD_BlueStrain = StringUtil.JenkinsHash("HUD_BlueStrain");

	public static int HUD_RedStrain = StringUtil.JenkinsHash("HUD_RedStrain");

	public static int HUD_WhiteStrain = StringUtil.JenkinsHash("HUD_WhiteStrain");

	public static int HUD_InvisibleStrain = StringUtil.JenkinsHash("HUD_InvisibleStrain");

	public const string SkinShader = "Custom/Character/Skin";

	public const string FaceShader = "Custom/Character/Face";

	public const string ClothesShader = "Custom/Character/Clothes";

	private const int MAX_DECALS = 16;

	private Matrix4x4[] TmpInvDecalTransform = new Matrix4x4[16];

	private float[] TmpDecalTexIndex = new float[16];

	private static List<InjuryBehaviour> FoundInjuriesList = new List<InjuryBehaviour>();

	public static float OverheadIconsYOffset = 0.15f;

	public static float FocusArrowScale = 0.5f;

	public static float FocusArrowScaleFlyMode = 4f;

	public static float FocusArrowYOffset = 0.1f;

	public static float OverheadActionIconYOffset = 0.2f;

	public static float OverheadIconExtraOffsetWhenRabbitTargetted = 0.25f;

	public static float BloodLossBarYOffset = 0.05f;

	public static float BloodLossBarScale = 0.005f;

	public static float SpeechBubbleYOffset = 0.2f;

	public static float CraftingProgressIndicatorScale = 0.75f;

	public static float CraftingProgressIndicatorYOffset = 0.2f;

	public static float PlayerNameYOffset = 0.05f;

	public static float PlayerNameScale = 0.005f;

	private static StringBuilder ingredientStringBuilder = new StringBuilder();

	private static string CalcCamouflageIconFillAmountStr = "CalcCamouflageIconFillAmount";

	private static string CollisionShapeStr = "CollisionShape";

	public static bool DebugShowCharacters = true;

	public bool WantUnityUpdateAppearance;

	public bool WantUnityUpdateInjuries;

	public bool WantUnityUpdateDecals;

	protected DeletionState CharacterDeleted;

	private static float MinChokePower = 0.1f;

	private static float MaxChokePower = 0.2f;

	private static float MinAssassinatePower = 0.15f;

	private static float MaxAssassinatePower = 0.3f;

	private static float MinRestrainPower = 0.1f;

	private static float MaxRestrainPower = 0.2f;

	private static float RestraintFatiguePenalty = 0.1f;

	private static float ChokeFatiguePenalty = 0.1f;

	private static float ChokeFakePressesPerSecondWhenHeld = 8f;

	private static float EscapeFakePressesPerSecondWhenHeld = 4f;

	private static float ChokeSoundRadius = 4f;

	public static int HUD_Parry = StringUtil.JenkinsHash("HUD_Parry");

	public static int HUD_Dodge = StringUtil.JenkinsHash("HUD_Dodge");

	public static int HUD_Escape = StringUtil.JenkinsHash("HUD_Escape");

	public static int HUD_Knockout = StringUtil.JenkinsHash("HUD_Knockout");

	public static int HUD_Assassinated = StringUtil.JenkinsHash("HUD_Assassinated");

	public static int HINT_TooTiredToFight = StringUtil.JenkinsHash("HINT_TooTiredToFight");

	public static int HINT_TooTiredToShoot = StringUtil.JenkinsHash("HINT_TooTiredToShoot");

	public static int HINT_LegShotDamageLimit = StringUtil.JenkinsHash("HINT_LegShotDamageLimit");

	private static StringBuilder sb = new StringBuilder();

	private static List<Character> TargetedCharacters = new List<Character>();

	private static float StandardDrinkAlcoholFlOz = 0.6f;

	public static float BaseMarkup = 1.5f;

	public static float ExitBuildingHitForce = 100f;

	private static float OutnumberedRange = 64f;

	public static bool DrawLookRaycasts = false;

	private static List<TileObject> NearbyPlants = new List<TileObject>();

	private static float FootstepsRangeFactor = 4f;

	private static float FootstepsRangeFactorCrouching = 2f;

	private static float VisRaycastCrouchingHeight = 0.6f;

	private static string GetVisibilityStr = "GetVisibility";

	public int IconIndex = -1;

	public int FullIconIndex = -1;

	public static Human[] IconGimp = new Human[2];

	public static BaseObject IconGimpMaster = null;

	public static GenderType IconGimpGender;

	public static IconType GeneratingIconType = IconType.None;

	public static PortraitPose GeneratingPortraitPose = PortraitPose.None;

	public static bool GeneratingIconFirstTimeHack = true;

	private bool CachedIsStationary = true;

	private static int PushingAgainstWaistHighWallFrame;

	private static Character PushingAgainstWaistHighWallCharacter;

	private static TileObject PushingAgainstWaistHighWallObj;

	private static List<Zone> _oldTriggerZones = new List<Zone>();

	private static List<Zone> _newTriggerZones = new List<Zone>();

	public static Resource<Texture2D> BloodDecalTex;

	public static Resource<Texture2D> BloodDecalNormals;

	public static Resource<Texture2D> CrackDecalTex;

	public static Resource<Texture2D> CrackDecalNormals;

	public static Resource<Texture2D> OverlayTex;

	public static Resource<Texture2D> OverlayNormal;

	public static Resource<GameObject> InjuryArrow;

	public static Resource<GameObject> InjuryBurning;

	public static Resource<GameObject> UnconsciousStar;

	public static int DefaultLayer;

	public static int CharacterCapsuleLayer;

	public static int TransparentFXLayer;

	public static int IgnoreRaycastLayer;

	public static int IconGimpLayer;

	public static int NonPhysicalLayer;

	public static int PreviewGimpLayer;

	public static int CharactersLayer;

	public static int WorldUILayer;

	public static int WaterLayer;

	public static int RagdollsLayer;

	public static int OutlinedLayer;

	public static int InvisibleLayer;

	public static int PipLayer;

	public static int GibLayer;

	public static int JuggernautLayer;

	public static int SunLayer;

	private static float TrapMaxRagdollVel = 0.5f;

	private static float PushOutOfWatchTowerForce = 3000f;

	public static TimeSpan RecoverFromRagdollTime = TimeSpan.FromSeconds(0.5);

	private static float RagdollStopMovingTolerance = 0.015f;

	private static TimeSpan AliveMinRagdollTime = TimeSpan.FromSeconds(0.5);

	private static TimeSpan DeadMinRagdollTime = TimeSpan.FromSeconds(1.5);

	private static TimeSpan AliveMaxRagdollTime = TimeSpan.FromSeconds(1.0);

	private static TimeSpan DeadMaxRagdollTime = TimeSpan.FromSeconds(5.0);

	private static TimeSpan DeadOnSlopeMaxRagdollTime = TimeSpan.FromSeconds(15.0);

	private bool SentRagdollStopMoving;

	private bool RagdollFromStumble;

	public static string EmmaOConnor = "EmmaOConnor";

	public static string KellySalas = "KellySalas";

	public static bool AlwaysActivateInvisibleStrainWhenNearDeath = false;

	private static float BluntDamageAbsorption = 0.75f;

	private static float SharpDamageAbsorption = 0.95f;

	public static float MaxLimbDamage = 0.4f;

	private static float LastZombieBiteSoundPlayedTime;

	public static bool DrawExplosionRaycasts = false;

	public static float ExplosionHitRadius = 1f;

	public static float BurnedHitRadius = 0.25f;

	public static float MeleeHitRadius = 0.25f;

	public static float BottleHitRadius = 0.25f;

	public static float ShotgunHitRadius = 0.5f;

	public static float BulletHitRadius = 0.25f;

	public static float ChokeHoldHitRadius = 0.25f;

	public static float MeleeHitForce = 2000f;

	public static float BurnedHitForce = 1000f;

	public static float BottleHitForce = 2000f;

	public static float ExplosionMaxHitForce = 10000f;

	public static float ExplosionMinHitForce = 6000f;

	public static float ChokeHoldHitForce = 2500f;

	public static float TrapDownForce = 8000f;

	private static float VehicleImpactFactor = 0.5f;

	private static float AccelerationLimit = 100f;

	private const float MaxInventoryWeightAtMinStrength = 30f;

	private const float MaxInventoryWeightAtMaxStrength = 40f;

	public static string Anyone = "Anyone";

	public static List<SkillEffect> SkillEffects = new List<SkillEffect>();

	public static List<SkillEffect> SkillEffectsOnThread = new List<SkillEffect>();

	private int[] CachedSkillLevelsWithEffects = new int[10];

	public TimeSpan RestockTime;

	public float ValueOfGoodsTradedWithPlayerAndSoldOn;

	public int TraderLevel;

	public int InitialBandageCount;

	public static float[] ProgressionToTraderLevel = new float[6] { 0f, 10f, 50f, 150f, 500f, 1000f };

	public static float TraderStockClothingProbability = 0.25f;

	private static List<Equipment> TradedItems = new List<Equipment>();

	public static int TraderSaveTokenMin = 0;

	public static int TraderSaveTokenMax = 2;

	public static float MoraleSad = -25f;

	public static float MoraleMiserable = -50f;

	public static float MoraleDepressed = -75f;

	public static float MoraleSuicidal = -100f;

	public const float MaxApprovalRating = 100f;

	public const float MinApprovalRating = -100f;

	public const float MaxRespectRating = 100f;

	public const float MinRespectRating = -100f;

	private float CachedMorale;

	private bool CachedMoraleDirty = true;

	private Dictionary<BaseObject, Vector2> CachedOpinion = new Dictionary<BaseObject, Vector2>();

	public static int MaxMemories = 128;

	public static SortMemoriesByPriorityDescending MemorySorter = new SortMemoriesByPriorityDescending();

	private bool CachedMysteriousAttackDirty = true;

	private MysteriousAttackType CachedMysteriousAttackType;

	private float CachedMysteriousAttackQuantity;

	private static TimeSpan TimeBetweenMemoryUpdates = TimeSpan.FromSeconds(10.0);

	private static float DefaultMoraleFadeSpeed = 2f;

	public static string TentCity = "TentCity";

	public static string WhiteHillsGang = "WhiteHillsGang";

	private static float Ln2 = 0.6931472f;

	private static float ForgetThreshold = 0.01f;

	public List<EquipmentPolicy> CachedCarryAmountPolicies;

	public List<EquipmentPrototype> TempDesiredAmmoTypes = new List<EquipmentPrototype>();

	public bool CachedCarryAmountPoliciesDirty = true;

	private static SortCarryPolicies CarryPolicySorter = new SortCarryPolicies();

	private static float ForgivingRadius = 0.95f;

	private static float ForgivingHeight = 2f;

	private static float BulletBoundsRadius = 0.2f;

	private static HitBox[] NoHitBoxes = new HitBox[0];

	public static bool DrawLastKnownTargetPositions = false;

	public static bool DrawBoundingBoxes;

	public static bool DrawHitBoxes;

	public static bool DrawInjuries;

	public static bool DrawBones;

	private static Bone ZombieBiteBone = Bone.Spine1;

	private static Vector3 ZombieBitePosInBoneSpace = new Vector3(-0.15f, 0.2f, 0.1f);

	private static float DirectControlledZombieEscapePower = 0.1f;

	public static TimeSpan MinTimeBetweenPlayerSurrender = TimeSpan.FromSeconds(60.0);

	private static TimeSpan RevivedInCaptivityTime = TimeSpan.FromSeconds(120.0);

	private static float RevivedInCaptivityFriendDist = 48f;

	private static float RevivedInCaptivityEnemyDist = 24f;

	public static float CarryRotX = -5.74f;

	public static float CarryRotY = 90f;

	public static float CarryRotZ = 56.49f;

	public static float CarryOffsetX = -0.26f;

	public static float CarryOffsetY = 0.12f;

	public static float CarryOffsetZ = 0.14f;

	public float CarriedTransition;

	public Quaternion CarriedRotStart;

	public Vector3 CarriedOffsetStart;

	public int CarriedInitHackTimer;

	public static int CarriedInitHackFrames = 2;

	private static float DropAwakeOffset = 0.5f;

	public static bool DrawMeleeAttackRaycasts = false;

	private static List<TileObject> PotentialTargets = new List<TileObject>();

	private const float MinParryDot = 0.5f;

	public static int RangedHeadshotSkillLevel = 2;

	public static int RangedLegshotSkillLevel = 1;

	public static int MeleeHeadSkillLevel = 1;

	public static int MeleeLegSkillLevel = 0;

	public static int UnarmedHeadSkillLevel = 0;

	public static int UnarmedLegSkillLevel = 0;

	public static float FootstepSoundDist = 6f;

	public static float FootstepSoundVolume = 1f;

	public static float VoiceSoundVolume = 1f;

	private static float DeepWaterDepth = 0.5f;

	private static float LightFootstepSpeed = 1.6f;

	private static float HeavyFootstepSpeed = 4.9f;

	private static List<string> TempPersonalities = new List<string>();

	private static float VehicleSoundRange = 16f;

	private static List<Character> NearbyCharacters = new List<Character>();

	private List<TileObject> NearbyBurningObjects = new List<TileObject>();

	private List<SingleTileProp> NearbyTrapSigns = new List<SingleTileProp>();

	private List<Prop> NearbyFood = new List<Prop>();

	private static float MaxAimUpAngle = MathF.PI / 2f;

	private static float MaxAimDownAngle = -MathF.PI / 2f;

	public const float InvisibleStrainBloodLossThreshold = 0.95f;

	private static int MinClothingInsulation = 5;

	public static float FireTemperatureInCelsius = 80f;

	public static float NakedComfortableTemperatureInCelsius = 24f;

	public static float ClothingInsulationTemperature = 3f;

	public static float MinIndoorTempDelta = 1f;

	public static float MinLooterIndoorTempDelta = 10f;

	public static float MinChickenCoopTempDelta = 5f;

	public static float HypothermiaRate = 0.00055555557f;

	public static float RainDampeningRate = 1f / 30f;

	public static float SnowDampeningRate = 0.00033333333f;

	public static float EvaporationRate = 1f / 120f;

	public static float FireEvaporationRate = 1f / 30f;

	public static float IndoorEvaporationRate = 0.0011111111f;

	public static float OverdressedFatiguePenalty = 1f / 30f;

	public static float WarmthFromExercising = 0.1f;

	public static float HungerFromExercising = 30f;

	public static float ThirstFromExercising = 30f;

	private static float[] MaxHeightForClothingType = new float[8] { 2f, 1.86f, 1.7f, 1.1f, 0.1f, 1.6f, 1.6f, 0.7f };

	private static float[] MinHeightForClothingType = new float[8] { 1.9f, 1.84f, 1.1f, 0.1f, 0f, 1.2f, 1.2f, 0.1f };

	private AStarRequester TeleportOffSlopeRequest;

	private const float SlidingStuckSpeed = 0.001f;

	public static float ChokeStrugglePowerMin = 0.5f;

	public static float ChokeStrugglePowerMax = 1f;

	public static float RestrainedStrugglePowerMin = 0.25f;

	public static float RestrainedStrugglePowerMax = 0.5f;

	public static float JogIfTiredDist = 20f;

	public static float InvisibleStrainSnarlProbabilityPerSecondExcitable = 0.004f;

	public static float InvisibleStrainSnarlProbabilityPerSecondExcitableExcited = 0.1f;

	public static float InvisibleStrainSnarlProbabilityPerSecondSubtle = 0.001f;

	public static float InvisibleStrainSnarlProbabilityPerSecondSubtleExcited = 0.01f;

	public static float CrouchingTransitionTime = 0.5f;

	public static float AimingTransitionTime = 0.5f;

	public static TimeSpan LoseFogOfWarAfterDeathTime = TimeSpan.FromSeconds(15.0);

	public static string UpdateVisibleTilesCacheStr = "UpdateVisibleTilesCache";

	private static GameProfiler LookTimer = new GameProfiler("Update.Character.Look");

	private static GameProfiler ThinkTimer = new GameProfiler("Update.Character.Think");

	private static List<Target> DeleteList = new List<Target>();

	private static Character CurrentlyThinking;

	private TerrainCoord LastGoodTile = TerrainCoord.Invalid;

	private float TrappedTimer;

	private const float IgnoreIfNotDetectedTime = 10f;

	private static List<Goal> GoalsToProcess = new List<Goal>();

	public virtual float Radius => HumanRadius;

	public bool IsAwake => Consciousness == Consciousness.Conscious;

	public bool IsConscious => Consciousness < Consciousness.Unconscious;

	public bool Alive => Consciousness < Consciousness.Dead;

	public bool AliveAndNotZombie
	{
		get
		{
			if (Alive)
			{
				return !Zombie;
			}
			return false;
		}
	}

	public bool ConsciousAndNotZombie
	{
		get
		{
			if (IsConscious)
			{
				return !Zombie;
			}
			return false;
		}
	}

	public bool AwakeAndNotZombie
	{
		get
		{
			if (IsAwake)
			{
				return !Zombie;
			}
			return false;
		}
	}

	public bool Zombie => Infection != InfectionType.None;

	public CanOpenGates CanOpenPlayerGates
	{
		get
		{
			if (CanFollowPlayer)
			{
				return global::CanOpenGates.Yes;
			}
			if (Community != null)
			{
				if (Community.CachedAllies.Contains(Session.Instance.CommunityManager.PlayerCommunity))
				{
					return global::CanOpenGates.Yes;
				}
				if (Community.CanOpenPlayerGates == global::CanOpenGates.No)
				{
					Squad squad = GetSquad();
					if (squad != null && squad.CanForceOpenPlayerGates())
					{
						return global::CanOpenGates.Unknown;
					}
				}
				return Community.CanOpenPlayerGates;
			}
			return global::CanOpenGates.Unknown;
		}
	}

	public override Vector3 Pos => Position;

	public override Vector2 PosXZ => new Vector2(Position.x, Position.z);

	public TerrainCoord Tile => GameTerrain.Instance.GetTileCoordForPos(Position);

	public Vector3 EyePosition => Position + new Vector3(0f, EyeHeight, 0f);

	public Vector3 GunPosition => Position + new Vector3(0f, GunHeight, 0f);

	public Vector3 ThrowPosition => Position + new Vector3(0f, ThrowHeight, 0f);

	public override float Height
	{
		get
		{
			if (!IsCrouching())
			{
				return Appearance.Height;
			}
			return Appearance.CrouchingHeight;
		}
	}

	public float EyeHeight
	{
		get
		{
			if (!IsCrouching())
			{
				return Appearance.EyeHeight;
			}
			return Appearance.CrouchingEyeHeight;
		}
	}

	public float GunHeight
	{
		get
		{
			if (!IsCrouching())
			{
				return Appearance.GunHeight;
			}
			return Appearance.CrouchingGunHeight;
		}
	}

	public float ThrowHeight
	{
		get
		{
			if (!IsCrouching())
			{
				return Appearance.ThrowHeight;
			}
			return Appearance.CrouchingThrowHeight;
		}
	}

	public override bool Deleted => CharacterDeleted == DeletionState.Deleted;

	public virtual float VisionConeDeg => 60f;

	public virtual float MapDotSize => 2.5f;

	public virtual MapIconType MapIconType
	{
		get
		{
			if (!Alive || CurrentActionAnim == ActionAnim.PlayDead)
			{
				return MapIconType.Dead;
			}
			if (Zombie)
			{
				return MapIconType.Zombie;
			}
			if (Consciousness == Consciousness.Unconscious)
			{
				return MapIconType.Unconscious;
			}
			Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
			if (this == localControlledCharacter || (SquadLeader == localControlledCharacter && SquadLeader != null))
			{
				return MapIconType.Pointer;
			}
			if ((NameKnown || InfoScreen.AllowViewInfoOnAnyone || Session.Instance.Editor) && Community != null && (Community.CommunityType == CommunityType.Normal || Community.CommunityType == CommunityType.Looter))
			{
				if (Rank == Rank.Leader)
				{
					return MapIconType.Leader;
				}
				if (HasRole(Role.Trader))
				{
					return MapIconType.Trader;
				}
			}
			return MapIconType.Normal;
		}
	}

	public override Color32 MapColor
	{
		get
		{
			TerrainCoord tile = Tile;
			if (!NameKnown && (Consciousness < Consciousness.Unconscious || !Investigated) && !GameTerrain.Instance.FogOfWar.IsTileVisible(tile.x, tile.y) && FogOfWar.DebugFogOfWarEnabled)
			{
				return MathUtil.TransparentBlack;
			}
			if (Disappeared || HiddenOnMap)
			{
				return MathUtil.TransparentBlack;
			}
			Color color = GameTerrain.MinimapSettings.NeutralCol;
			if (Infection > InfectionType.None)
			{
				color = GameTerrain.MinimapSettings.GetInfectionCol(Infection);
			}
			else if (Community != null)
			{
				CommunityManager communityManager = Session.Instance.CommunityManager;
				if (Community == communityManager.PlayerCommunity)
				{
					return GameTerrain.MinimapSettings.FriendCol;
				}
				switch (communityManager.GetRelationship(Community, communityManager.PlayerCommunity))
				{
				case CommunityRelationshipType.Hostile:
					if (!GameTerrain.Instance.FogOfWar.IsTileVisible(tile.x, tile.y) && (Consciousness < Consciousness.Unconscious || !Investigated) && FogOfWar.DebugFogOfWarEnabled)
					{
						return MathUtil.TransparentBlack;
					}
					color = GameTerrain.MinimapSettings.EnemyCol;
					break;
				case CommunityRelationshipType.Allied:
					color = GameTerrain.MinimapSettings.AllyCol;
					break;
				}
			}
			if (!Alive || CurrentActionAnim == ActionAnim.PlayDead)
			{
				color *= (Color)GameTerrain.MinimapSettings.DeadCol;
				if (GetBaseObjectType() == BaseObjectType.Human)
				{
					if (Inventory.IsEmptyExceptForWornItems(this))
					{
						color.a = 0.5f;
					}
				}
				else if (SkinnedAmount >= 1f && Inventory.IsEmptyExceptForWornItems(this))
				{
					color.a = 0.5f;
				}
			}
			return color;
		}
	}

	public bool DirectControlled
	{
		get
		{
			return IsDirectControlled;
		}
		set
		{
			SetDirectControlled(value, wantSetLastInputTime: true, wantClearLeaderCommand: true);
		}
	}

	public virtual float GetWalkSpeed()
	{
		if (!Zombie)
		{
			return WalkSpeed;
		}
		return ZombieWalkSpeed;
	}

	public virtual float GetRunSpeed()
	{
		if (!Zombie)
		{
			return RunSpeed;
		}
		return ZombieRunSpeed;
	}

	public virtual float GetRotSpeed()
	{
		return RotSpeed;
	}

	public override byte[] GetPredictedActionCounter()
	{
		return PredictedActionCounter;
	}

	public override List<PredictedEvent> GetPredictedEvents()
	{
		if (PredictedEvents == null)
		{
			PredictedEvents = new List<PredictedEvent>();
		}
		return PredictedEvents;
	}

	public override void Consume(Character character, float amount, InfectionType infectionType)
	{
		if (Alive)
		{
			Vector3 nonDeterministicHitPos = (IsUnityObjectActive() ? GetUnityBoneTransform(ZombieBiteBone).MultiplyPoint(ZombieBitePosInBoneSpace) : GetBoundingBoxCentre());
			InjuryLocation injuryLocation = PickRandomInjuryLocation(character.PosXZ, TargettableBodyLocation.Torso);
			float damage = ZombieBiteDamage * amount;
			OnDamaged(character, this, InjuryType.ZombieBite, injuryLocation, infectionType, null, SkillType.Strength, ref damage, 0f, nonDeterministicHitPos, Vector3.zero, ZombieBiteBone, ZombieBitePosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
		}
		else
		{
			SkinnedAmount = Math.Min(1f, SkinnedAmount + amount);
		}
	}

	public override float GetConsumedAmount()
	{
		return SkinnedAmount;
	}

	public override bool IsDestroyed()
	{
		return !Alive;
	}

	public override bool IsDisappeared()
	{
		return Disappeared;
	}

	public override Flammability GetFlammability()
	{
		return Flammability.Medium_RequiresFuel;
	}

	public override ImpactSusceptibility GetImpactSusceptibility()
	{
		return ImpactSusceptibility.Medium_BulletHits;
	}

	public bool IsZombifying()
	{
		if (!Alive && InfectionProgression > 0f)
		{
			return !Zombie;
		}
		return false;
	}

	public override string GetLootLocation()
	{
		return LootLocation;
	}

	public override void SetLootLocation(string lootLocation)
	{
		LootLocation = lootLocation;
	}

	public bool IsAttractedTo(GenderType gender)
	{
		if (HasPersonality(CachedPersonalityType.Homosexual))
		{
			return gender == Appearance.Gender;
		}
		if (HasPersonality(CachedPersonalityType.Bisexual))
		{
			return true;
		}
		return gender != Appearance.Gender;
	}

	public bool IsInDatingAgeRange(Character other)
	{
		return Mathf.Abs(Appearance.Age - other.Appearance.Age) <= 20f;
	}

	public virtual CharacterAppearance CreateAppearance()
	{
		return null;
	}

	public virtual EquipmentPrototype GetMeatType()
	{
		return null;
	}

	public virtual int GetMeatAmount(int cookingSkill)
	{
		return 0;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref Position);
		reflector.Add(ref OldPosition);
		if (reflector.Version < 560)
		{
			int value = 0;
			reflector.Add(ref value);
		}
		reflector.Add(ref VelocityXZ);
		reflector.Add(ref FacingAngle);
		reflector.Add(ref DesiredFacingAngle);
		reflector.AddAfter(ref WantKilledByPitTrap, 181);
		if (reflector.IsDeserialising && reflector.Version < 117)
		{
			LastGoodTile = Tile;
		}
		else
		{
			reflector.AddAfter(ref LastGoodTile, 117);
		}
		reflector.AddAfter(ref TrappedTimer, 571);
		if (reflector.IsDeserialising)
		{
			Appearance = CreateAppearance();
		}
		reflector.Add(Appearance);
		reflector.Add(ref FirstName);
		reflector.Add(ref Surname);
		if (!reflector.IsDoingNetworkChecksum)
		{
			reflector.AddAfter(ref FirstNameVerified, 595);
			reflector.AddAfter(ref SurnameVerified, 595);
		}
		reflector.Add(ref NameKnown);
		reflector.Add(ref BackgroundKnown);
		reflector.AddAfter(ref BrainScanned, 398);
		reflector.AddAfter(ref PersonalityKnown, 398);
		reflector.Add(ref InjuryRemarkedOn);
		reflector.Add(ref InfectionRemarkedOn);
		reflector.AddAfter(ref EulogyGiven, 27);
		reflector.AddAfter(ref Investigated, 16);
		reflector.Add(ref God);
		reflector.AddAfter(ref DisablePrediction, 204);
		reflector.Add(ref DontSimulateSurvivalFactorsUntilDiscovered);
		reflector.Add(ref DontSimulateSurvivalFactorsUntilJoinCommunity);
		reflector.AddAfter(ref DontLeaveCommunity, 558);
		reflector.AddAfter(ref DontAbandonPlayer, 621);
		reflector.AddAfter(ref AlwaysActivateInvisibleStrain, 619);
		reflector.AddAfter(ref LimitBloodLoss, 456);
		reflector.AddAfter(ref DontBury, 453);
		reflector.Add(ref Encumbered);
		reflector.Add(ref HalfDemolishLoot);
		reflector.Add(ref CachedInventoryWeight);
		reflector.AddAfter(ref DontSellItemTypeToPlayer, 552);
		reflector.Add(ref Boxer);
		reflector.Add(ref KnownBoxer);
		reflector.AddAfter(ref SparringType, 125);
		reflector.AddAfter(ref SparringPartner, 105);
		reflector.AddAfter(ref SparringInstigator, 244);
		reflector.AddAfter(ref SparringPartnerInitialBloodLoss, 125);
		reflector.AddAfter(ref SparringSnowballHits, 243);
		reflector.Add(ref BloodLoss);
		reflector.Add(ref Fatigue);
		reflector.Add(ref Adrenaline);
		reflector.Add(ref Hunger);
		reflector.Add(ref Thirst);
		reflector.AddAfter(ref Toilet, 75);
		reflector.Add(ref SleepDeprivation);
		reflector.Add(ref BodyTemperatureInCelsius);
		reflector.AddAfter(ref TemperatureInsulationDelta, 42);
		if (reflector.Version < 50 && GetBaseObjectType() != BaseObjectType.Human)
		{
			BodyTemperatureInCelsius = BodyTemperatureInCelsiusNormal;
			TemperatureInsulationDelta = 0f;
		}
		reflector.Add(ref BloodAlcoholConcentration);
		reflector.Add(ref SedativeEffect);
		reflector.AddAfter(ref Excitement, 142);
		reflector.Add(ref LungCancer);
		reflector.Add(ref InfectionProgression);
		reflector.Add(ref AimingAccuracy);
		reflector.Add(ref ZombieEscapePower);
		reflector.Add(ref ChokePower);
		reflector.Add(ref LastChokeTime);
		reflector.Add(ref ReservedGoldAmount);
		reflector.AddAfter(ref LastDrankAlcoholTime, 13);
		reflector.AddAfter(ref EncouragementTimeout, 25);
		reflector.AddAfter(ref LastUsedMolotovTime, 170);
		reflector.AddAfter(ref LastUsedPipeBombTime, 170);
		reflector.Add(ref Infection);
		reflector.AddAfter(ref InvisibleStrain, 142);
		if (reflector.Version < 157)
		{
			bool value2 = false;
			reflector.AddAfter(ref value2, 142);
		}
		else
		{
			reflector.AddAfter(ref InvisibleStrainJustActivated, 157);
		}
		reflector.AddAfter(ref InvisibleStrainNextSpread, 200);
		reflector.AddAfter(ref InvisibleStrainSnarlStartTime, 198);
		reflector.AddAfter(ref HadInvisibleStrainFromStart, 200);
		reflector.AddAfter(ref DontInfectWithInvisibleStrain, 295);
		reflector.Add(ref Consciousness);
		if (reflector.Version < 151)
		{
			bool value3 = false;
			reflector.Add(ref value3);
			SkinnedAmount = (value3 ? 1f : 0f);
		}
		else
		{
			reflector.Add(ref SkinnedAmount);
		}
		if (reflector.Version < 167)
		{
			bool value4 = false;
			reflector.AddAfter(ref value4, 134);
			CauseOfDeath = (value4 ? CauseOfDeath.Zombie : CauseOfDeath.Other);
		}
		else
		{
			reflector.AddAfter(ref CauseOfDeath, 187);
		}
		reflector.AddAfter(ref Rotten, 151);
		reflector.AddAfter(ref WasAttackedBeforeLastCeaseFire, 505);
		reflector.AddAfter(ref PlaceOfDeath, 167);
		reflector.Add(ref TimeOfDeath);
		reflector.AddAfter(ref Killer, 230);
		reflector.Add(ref LastKnockedOutTime);
		reflector.AddAfter(ref LastRevivedTime, 400);
		reflector.Add(ref LastPersonChokedMe);
		reflector.AddAfter(ref PickpocketDetection, 626);
		if (reflector.Version >= 627)
		{
			reflector.AddListThatCanBeNull(ref PickpocketedItems);
		}
		reflector.Add(ref PlayDead);
		reflector.Add(ref Disappeared);
		reflector.AddAfter(ref HiddenOnMap, 533);
		reflector.AddAfter(ref KeptAroundForReferences, 130);
		reflector.AddAfter(ref HasTravelledFromAnotherMap, 479);
		reflector.AddAfter(ref OriginalId, 608);
		reflector.AddAfter(ref CharacterDeleted, 160);
		int value5 = Targets.Count;
		reflector.Add(ref value5);
		if (reflector.IsDeserialising)
		{
			List<Target> list = new List<Target>();
			foreach (Target target in Targets)
			{
				list.Add(target);
			}
			for (int i = 0; i < value5; i++)
			{
				TileObject obj = null;
				reflector.Add(ref obj);
				if (obj == null)
				{
					new Target().Reflect(reflector);
					continue;
				}
				Target orCreateTarget = GetOrCreateTarget(obj);
				orCreateTarget.Object = obj;
				orCreateTarget.Reflect(reflector);
				list.Remove(orCreateTarget);
			}
			foreach (Target item3 in list)
			{
				RemoveTarget(item3);
			}
			list.Clear();
		}
		else
		{
			foreach (Target target2 in Targets)
			{
				reflector.Add(ref target2.Object);
				target2.Reflect(reflector);
			}
		}
		reflector.Add(ref GoalTarget, this);
		reflector.Add(ref DirectControlledTarget, this);
		if (reflector.IsDeserialising && DirectControlledTarget != null)
		{
			DirectControlledTarget.RefCount++;
		}
		reflector.Add(ref GoalAimingRefCount);
		reflector.Add(ref DirectControlledAiming);
		reflector.Add(ref DirectControlledAimingStartTime);
		reflector.AddAfter(ref DisableFistsAiming, 37);
		reflector.Add(ref GoalCrouchingRefCount);
		reflector.Add(ref DirectControlledCrouching);
		reflector.Add(ref Sitting);
		reflector.AddAfter(ref SittingAround, 71);
		reflector.AddGameObjectRefList(ref NearbyBurningObjects);
		if (!reflector.IsDoingPrediction)
		{
			reflector.AddAfter(ref Memories, 22);
			reflector.AddAfter(ref SparringMemories, 268);
			if (reflector.Version >= 582)
			{
				reflector.AddIntList(ref SparringRemovedArmorIds);
			}
			if (reflector.Version == 575 && (reflector.IsDoingNetworkChecksum || reflector.IsTextDumping))
			{
				int value6 = CachedOpinion.Count;
				reflector.Add(ref value6);
				if (reflector.IsSerialising)
				{
					foreach (KeyValuePair<BaseObject, Vector2> item4 in CachedOpinion)
					{
						BaseObject value7 = item4.Key;
						Vector2 value8 = item4.Value;
						reflector.Add(ref value7);
						reflector.Add(ref value8);
					}
				}
				else
				{
					for (int j = 0; j < value6; j++)
					{
						BaseObject value9 = null;
						Vector2 value10 = Vector2.zero;
						reflector.Add(ref value9);
						reflector.Add(ref value10);
						CachedOpinion[value9] = value10;
					}
				}
			}
			if (reflector.IsDeserialising && !reflector.IsDoingNetworkChecksum)
			{
				for (int num = Memories.Count - 1; num >= 0; num--)
				{
					if (Memories[num].Prototype == null)
					{
						Memories.RemoveAt(num);
					}
				}
			}
			if (reflector.Version < 95)
			{
				for (int num2 = Memories.Count - 1; num2 >= 0; num2--)
				{
					if (Memories[num2].Prototype == MemoryPrototype.Killed && Memories[num2].Object == null)
					{
						Memories.RemoveAt(num2);
					}
				}
			}
			if (reflector.Version < 418)
			{
				for (int k = 0; k < Memories.Count; k++)
				{
					Memory value11 = Memories[k];
					value11.MoraleContribution = value11.CalcMemoryMoraleContribution(this);
					Memories[k] = value11;
				}
			}
			if (reflector.Version >= 564)
			{
				reflector.AddSmallList(ref EmpathyOverrides);
			}
			else if (reflector.Version >= 145)
			{
				List<BaseObject> list2 = null;
				reflector.AddGameObjectRefList(ref list2);
				if (list2 != null)
				{
					for (int l = 0; l < list2.Count; l++)
					{
						EmpathyOverride item = new EmpathyOverride
						{
							OverrideObject = list2[l],
							OverrideValue = -100f
						};
						EmpathyOverrides.Add(item);
					}
				}
			}
		}
		reflector.AddStringList(ref Personality);
		if (reflector.Version < 569)
		{
			CachePersonality();
		}
		else
		{
			reflector.Add(ref CachedPersonality);
			if (reflector.Version < 592)
			{
				CachePersonality();
			}
		}
		if (!reflector.IsDoingPrediction)
		{
			reflector.AddAfter(ref FindAttempts, 64);
			if (reflector.Version < 362)
			{
				for (int num3 = FindAttempts.Count - 1; num3 > 0; num3--)
				{
					for (int m = 0; m < num3; m++)
					{
						if (FindAttempts[num3].Matches(FindAttempts[m].SearchObjectId, FindAttempts[m].EntranceIndex, FindAttempts[m].TerrainPathIndex, FindAttempts[m].FindType, FindAttempts[m].ProtoToFind, FindAttempts[m].Liquid, FindAttempts[m].FollowingRecipe))
						{
							FindAttempt value12 = FindAttempts[m];
							value12.FailCount += FindAttempts[num3].FailCount;
							FindAttempts[m] = value12;
							FindAttempts.RemoveAt(num3);
							break;
						}
					}
				}
			}
		}
		reflector.AddAfter(ref ExitBuildingAttempts, 138);
		if (reflector.Version >= 206)
		{
			reflector.Add(ref EquipmentPolicies);
		}
		if (reflector.Version >= 207)
		{
			MovementZone.Reflect(reflector);
		}
		reflector.AddAfter(ref MovementZonePaused, 209);
		if (reflector.Version >= 22)
		{
			reflector.AddEquipmentPrototypeList(ref BooksRead);
		}
		if (reflector.Version >= 24)
		{
			reflector.Add(ref Relationships);
			if (reflector.IsDeserialising)
			{
				for (int num4 = Relationships.Count - 1; num4 >= 0; num4--)
				{
					if (Relationships[num4].RelationshipTarget == null)
					{
						Relationships.RemoveAt(num4);
					}
				}
			}
		}
		if (reflector.Version < 139)
		{
			reflector.AddAfter(ref Memories, 23);
		}
		Skillset.Reflect(reflector);
		reflector.Add(ref Community);
		reflector.Add(ref InitialCommunity);
		reflector.AddAfter(ref JoinedCommunityTime, 530);
		if (reflector.IsDeserialising && reflector.Version < 120 && InitialCommunity == null && Community != null && Community.CommunityType != CommunityType.Player)
		{
			InitialCommunity = Community;
		}
		if (reflector.IsDeserialising && reflector.Version < 151)
		{
			Rotten = Zombie && IsAmbient();
		}
		reflector.Add(ref SquadId);
		if (reflector.Version < 352)
		{
			RoleInfo item2 = default(RoleInfo);
			reflector.Add(ref item2.Role);
			reflector.AddAfter(ref item2.Paused, 126);
			if (item2.Role != Role.None)
			{
				Roles.Add(item2);
			}
		}
		else
		{
			reflector.Add(ref Roles);
			if (reflector.IsDeserialising && reflector.Version < 392)
			{
				for (int num5 = Roles.Count - 1; num5 >= 0; num5--)
				{
					if (Roles[num5].Role == Role.None)
					{
						Roles.RemoveAt(num5);
					}
				}
				if (Community != null && Community.IsAISettlement())
				{
					int num6 = Roles.Count - 1;
					while (num6 >= 0 && Roles.Count > 1)
					{
						if (Roles[num6].Role != Role.Trader)
						{
							Roles.RemoveAt(num6);
						}
						num6--;
					}
				}
			}
		}
		reflector.AddAfter(ref MostRecentRoleIndex, 523);
		reflector.AddAfter(ref GuardDuty, 52);
		reflector.Add(ref PlantNewCrops);
		reflector.AddAfter(ref DownTime, 532);
		reflector.Add(ref HangOutLocation);
		reflector.Add(ref InitialHangOutLocation);
		if (reflector.Version < 529 && HangOutLocation == TerrainCoord.Invalid && UniqueID == JoeWheeler)
		{
			HangOutLocation = InitialHangOutLocation;
		}
		if (reflector.Version < 352)
		{
			TerrainCoord value13 = TerrainCoord.Zero;
			TerrainCoord value14 = TerrainCoord.Zero;
			TerrainCoord value15 = TerrainCoord.Zero;
			TerrainCoord value16 = TerrainCoord.Zero;
			TerrainCoord value17 = TerrainCoord.Zero;
			TerrainCoord value18 = TerrainCoord.Zero;
			TerrainCoord value19 = TerrainCoord.Zero;
			TerrainCoord value20 = TerrainCoord.Zero;
			reflector.Add(ref value13);
			reflector.AddAfter(ref value14, 31);
			reflector.AddAfter(ref value15, 66);
			reflector.AddAfter(ref value16, 162);
			reflector.AddAfter(ref value17, 31);
			reflector.Add(ref value18);
			reflector.Add(ref value19);
			reflector.Add(ref value20);
			if (Roles.Count > 0)
			{
				RoleInfo value21 = Roles[0];
				switch (value21.Role)
				{
				case Role.Farmer:
					value21.TargetLocation = value13;
					break;
				case Role.Lumberjack:
					value21.TargetLocation = value14;
					break;
				case Role.Miner:
					value21.TargetLocation = value15;
					break;
				case Role.Trapper:
					value21.TargetLocation = value16;
					break;
				case Role.Cook:
					value21.TargetLocation = value17;
					break;
				case Role.Guard:
					value21.TargetLocation = value18;
					break;
				case Role.Gatherer:
					value21.TargetLocation = value19;
					break;
				}
				Roles[0] = value21;
			}
		}
		reflector.Add(ref RestockTime);
		reflector.AddAfter(ref ValueOfGoodsTradedWithPlayerAndSoldOn, 184);
		reflector.AddAfter(ref TraderLevel, 184);
		reflector.AddAfter(ref InitialBandageCount, 587);
		if (reflector.Version < 352)
		{
			EquipmentPrototype value22 = null;
			EquipmentPrototype value23 = null;
			Recipe value24 = null;
			TerrainCoord value25 = TerrainCoord.Zero;
			reflector.Add(ref value22);
			reflector.AddAfter(ref value23, 66);
			reflector.AddAfter(ref value24, 29);
			reflector.AddAfter(ref value25, 29);
			if (Roles.Count > 0)
			{
				RoleInfo value26 = Roles[0];
				switch (value26.Role)
				{
				case Role.Gatherer:
					value26.ResourceType = value22;
					break;
				case Role.Miner:
					value26.ResourceType = value23;
					break;
				case Role.Crafter:
					value26.Recipe = value24;
					value26.TargetLocation = value25;
					break;
				}
				Roles[0] = value26;
			}
		}
		reflector.Add(ref Rank);
		reflector.AddAfter(ref VotedFor, 355);
		reflector.AddAfter(ref SurrenderMode, 498);
		reflector.Add(ref CanFollowPlayer);
		reflector.Add(ref SquadLeader);
		reflector.AddGameObjectRefList(ref Followers);
		reflector.Add(ref FollowMeMoveFrame);
		reflector.Add(ref FollowMePosXZ);
		reflector.Add(ref FollowMeDirAngle);
		if (reflector.Version < 364)
		{
			CanOpenGates value27 = global::CanOpenGates.Unknown;
			reflector.AddAfter(ref value27, 283);
			if (value27 != global::CanOpenGates.Unknown && Community != null)
			{
				Community.CanOpenPlayerGates = value27;
			}
		}
		reflector.Add(ref ScriptedGoalMarker);
		reflector.Add(ref RecentActivityType);
		reflector.Add(ref RecentActivityTime);
		reflector.Add(ref RecentActivityWith);
		reflector.AddAfter(ref RecentActivityPos, 397);
		reflector.Add(ref Route);
		reflector.Add(ref MovementType);
		reflector.Add(ref MovementMode);
		reflector.Add(ref FlankingDesiredRange);
		reflector.Add(ref WantAvoidance);
		reflector.AddAfter(ref Tired, 526);
		reflector.Add(ref ThinkPriorityBucket);
		reflector.Add(ref LastUpdateTime);
		reflector.Add(ref LastThinkTime);
		if (reflector.Version < 107)
		{
			LastMemoryUpdateTime = Session.Instance.PlayTime;
		}
		else
		{
			reflector.AddAfter(ref LastMemoryUpdateTime, 107);
		}
		if (reflector.Version < 347)
		{
			LastSkillAtrophyTime = Session.Instance.PlayTime;
		}
		else
		{
			reflector.AddAfter(ref LastSkillAtrophyTime, 347);
		}
		reflector.AddAfter(ref WasCrouching, 63);
		reflector.AddAfter(ref WasAiming, 63);
		reflector.AddAfter(ref CrouchingTransition, 77);
		reflector.AddAfter(ref AimingTransition, 77);
		reflector.AddAfter(ref DisableInputsUntilNextThink, 97);
		reflector.Add(ref NumUpdateFrames);
		reflector.AddAfter(ref FramesPerUpdate, 560);
		if (reflector.IsDoingNetworkChecksum && reflector.Version >= 220)
		{
			reflector.Add(ref LastVisibleInputFrame);
		}
		reflector.AddAfter(ref NeedsUpdateEveryFrame, 554);
		reflector.Add(ref InCombat);
		reflector.AddAfter(ref AboutToBeInCombat, 122);
		reflector.AddAfter(ref OrderedToAttack, 103);
		reflector.AddAfter(ref Surrendering, 401);
		reflector.Add(ref UnderAttackRefCount);
		reflector.Add(ref Injuries);
		reflector.Add(ref Fuel);
		reflector.Add(ref CremationProgression);
		reflector.Add(ref QueuedSpeeches);
		if (reflector.Version < 573)
		{
			for (int num7 = QueuedSpeeches.Count - 1; num7 >= 0; num7--)
			{
				if (QueuedSpeeches[num7].Speech == null)
				{
					QueuedSpeeches.RemoveAt(num7);
				}
			}
		}
		if (!reflector.IsDoingPrediction)
		{
			reflector.Add(ref SpokenSpeeches);
			if (reflector.IsDeserialising)
			{
				for (int num8 = SpokenSpeeches.Count - 1; num8 >= 0; num8--)
				{
					if (SpokenSpeeches[num8].Speech == null)
					{
						SpokenSpeeches.RemoveAt(num8);
					}
				}
			}
		}
		if (reflector.Version >= 156)
		{
			reflector.Add(ref Timers);
		}
		reflector.Add(ref Speaking);
		reflector.Add(ref SpeechStartTime);
		reflector.Add(ref SpeakingParamResults);
		reflector.Add(ref Listener);
		reflector.Add(ref SpeechObject);
		if (reflector.Version >= 245)
		{
			SpeechParam.Reflect(reflector);
		}
		reflector.Add(ref CurrentSpeechAnimState);
		reflector.Add(ref EquippedItem);
		reflector.Add(ref DesiredEquippedItem);
		reflector.Add(ref LeftHandEquippedItem);
		int num9 = ((reflector.Version < 273) ? 7 : ((reflector.Version < 579) ? 8 : 9));
		for (int n = 0; n < num9; n++)
		{
			reflector.Add(ref Clothes[n]);
		}
		reflector.Add(Inventory);
		if (reflector.IsDeserialising && reflector.Version < 147)
		{
			foreach (Equipment content in Inventory.Contents)
			{
				content.InventoryOwner = this;
			}
		}
		if (reflector.IsDoingPrediction)
		{
			reflector.Add(ref SprintCountdown);
		}
		reflector.Add(ref Goal, this);
		reflector.Add(ref IsDirectControlled);
		reflector.Add(ref DirectControlledMajorAIDisabled);
		reflector.Add(ref LastDirectControlInputActionTime);
		reflector.Add(ref LastPlayerCommandTime);
		reflector.AddAfter(ref AvatarForPlayer, 193);
		reflector.Add(ref MovementSpeed);
		reflector.Add(ref MovementAngle);
		reflector.Add(ref InsideBuilding);
		reflector.Add(ref InsideBuildingSlotIndex);
		reflector.Add(ref RagdollIgnoreBuilding);
		reflector.AddAfter(ref WasOrderedInsideBuilding, 174);
		reflector.AddAfter(ref LastEnteredOrExitedBuildingTime, 562);
		if (reflector.Version < 408)
		{
			bool value28 = false;
			reflector.AddAfter(ref value28, 391);
			DisableSleep = (value28 ? 1 : 0);
		}
		else
		{
			reflector.Add(ref DisableSleep);
		}
		reflector.Add(ref Fade);
		reflector.Add(ref FadeTarget);
		reflector.Add(ref FadeTime);
		reflector.Add(ref CurrentAnimState);
		if (!reflector.IsDoingNetworkChecksum && !reflector.IsDoingPrediction)
		{
			if (CurrentAnimState == AnimState.Ragdoll && reflector.IsSerialising && GetPredictedOrElseThisCharacter().IsUnityObjectActive())
			{
				GetPredictedOrElseThisCharacter().UnitySaveProneBoneTransforms();
			}
			if (CurrentAnimState != AnimState.Animation)
			{
				if (reflector.Version < 55)
				{
					for (int num10 = 0; num10 < 23; num10++)
					{
						ProneBoneTransforms[num10].Reflect(reflector);
					}
					ProneBoneTransforms[23].Pos = Vector3.zero;
					ProneBoneTransforms[23].Orientation = Quaternion.identity;
					ProneBoneTransforms[23].Scale = 1f;
				}
				else
				{
					for (int num11 = 0; num11 < 24; num11++)
					{
						ProneBoneTransforms[num11].Reflect(reflector);
						MathUtil.CheckNaNorInfinity(ProneBoneTransforms[num11]);
					}
				}
			}
		}
		string value29 = ((AnimWrapper != null) ? AnimWrapper.BaseStateName : string.Empty);
		reflector.Add(ref value29);
		reflector.Add(ref AnimStartTime);
		reflector.AddAfter(ref AnimSpeed, 600);
		reflector.Add(ref AnimHeight);
		reflector.Add(ref InteractionObject);
		reflector.AddAfter(ref CarryingObject, 54);
		reflector.AddAfter(ref CommunityNoticedBodySnatching, 81);
		reflector.AddAfter(ref LastRescuedByPlayer, 351);
		reflector.AddAfter(ref CarriedBy, 54);
		reflector.Add(ref ParryingAttacker);
		reflector.Add(ref CurrentActionAnim);
		if (reflector.IsDeserialising)
		{
			AnimWrapper = AnimationManager.Instance.GetAnimByName(CurrentActionAnim, value29);
		}
		reflector.Add(ref CanAttackJumpingZombie);
		reflector.Add(ref CanAttackGrabbingZombie);
		if (reflector.Version < 51)
		{
			bool value30 = false;
			reflector.Add(ref value30);
		}
		reflector.AddAfter(ref IsCurrentActionAnimFromGoal, 51);
		reflector.Add(ref LastFireTime);
		reflector.AddAfter(ref LastAttackedSomeoneTime, 45);
		reflector.AddAfter(ref DangerousToRescueMeTime, 378);
		reflector.AddAfter(ref DangerousToRescueMeBecause, 378);
		reflector.AddAfter(ref BeingRescuedBy, 424);
		if (reflector.Version == 239)
		{
			int value31 = 0;
			reflector.Add(ref value31);
		}
		if (reflector.IsDoingPrediction)
		{
			reflector.AddByteArray(ref PredictedActionCounter);
		}
		if (reflector.Version < 134 && !AliveAndNotZombie && HasAnyInfectedInjuries())
		{
			CauseOfDeath = CauseOfDeath.Zombie;
		}
		if (reflector.Version < 155 && InfectionProgression > 0f && AliveAndNotZombie && GetWorstInfectionTypeInProgression() == InfectionType.None)
		{
			InfectionProgression = 0f;
		}
		if (reflector.Version < 405 && Community != null && Community.CommunityType == CommunityType.Player)
		{
			SetAllSkillsKnown();
			PersonalityKnown = 0u;
		}
		if (reflector.Version < 406 && IsPlayerAvatar())
		{
			SetAllPersonalitiesKnown();
			Relationship.SetAllRelationshipsKnown(this);
		}
	}

	public override void PredictedFixup()
	{
		base.PredictedFixup();
		if (InsideBuilding != null)
		{
			InsideBuilding = InsideBuilding.GetPredictedOrElseThis() as Building;
		}
		if (InteractionObject != null)
		{
			InteractionObject = InteractionObject.GetPredictedOrElseThis();
		}
		if (SparringPartner != null)
		{
			SparringPartner = SparringPartner.GetPredictedOrElseThisCharacter();
		}
		if (CarryingObject != null)
		{
			CarryingObject = CarryingObject.GetPredictedOrElseThis();
		}
		if (CarriedBy != null)
		{
			CarriedBy = CarriedBy.GetPredictedOrElseThisCharacter();
		}
		if (ParryingAttacker != null)
		{
			ParryingAttacker = ParryingAttacker.GetPredictedOrElseThisCharacter();
		}
		if (DodgingProjectile != null)
		{
			DodgingProjectile = DodgingProjectile.GetPredictedOrElseThis();
		}
		if (SquadLeader != null)
		{
			SquadLeader = SquadLeader.Predicted;
		}
		if (Followers != null)
		{
			for (int num = Followers.Count - 1; num >= 0; num--)
			{
				Followers[num] = Followers[num].Predicted;
				if (Followers[num] == null)
				{
					Followers.RemoveAt(num);
				}
			}
		}
		foreach (Target target in Targets)
		{
			target.Object = target.Object.GetPredictedOrElseThis();
			target.Predicted = true;
		}
		if (Goal != null)
		{
			Goal.PredictedFixup(this, null);
		}
	}

	public bool IsSprintEnabled()
	{
		return SprintCountdown > 0;
	}

	public void EnableSprint()
	{
		SprintCountdown = 2;
	}

	public void DisableSprint()
	{
		SprintCountdown = 0;
	}

	public override float GetHeightIgnoringCrouching()
	{
		return Appearance.Height;
	}

	public virtual float GetUnityHeadHeight()
	{
		return Height;
	}

	public virtual bool CanEatGrass()
	{
		return false;
	}

	public virtual bool CanGraze()
	{
		return false;
	}

	public override float GetPipFocusDist()
	{
		return Pip.DefaultDistFromFocus;
	}

	public virtual float GetPipYaw()
	{
		if (CarryingObject != null)
		{
			if (!IsCrouching())
			{
				return Pip.CarryingYaw;
			}
			return Pip.CarryingCrouchingYaw;
		}
		return 0f;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		return Appearance.Gender;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		switch (Usage)
		{
		case CharacterUsage.IconGimp:
			sb.Append("IconGimp");
			return;
		case CharacterUsage.PreviewGimp:
			sb.Append("PreviewGimp");
			return;
		}
		string result3;
		if (FirstName.Length > 0)
		{
			string result;
			if (NameKnown || noStrangers || Session.Instance.Editor)
			{
				if (GameImpl.WantNamesReversed(Surname, englishOnly))
				{
					sb.Append(GameImpl.TranslateSurname(Surname, Appearance.Gender, englishOnly, SurnameVerified));
					sb.Append(GameImpl.TranslateName(FirstName, englishOnly, FirstNameVerified));
					return;
				}
				sb.Append(GameImpl.TranslateName(FirstName, englishOnly, FirstNameVerified));
				if (Surname.Length > 0)
				{
					sb.Append(' ');
					sb.Append(GameImpl.TranslateSurname(Surname, Appearance.Gender, englishOnly, SurnameVerified));
				}
			}
			else if (Appearance.Gender == GenderType.Female && GameImpl.Instance.TryTranslate(NAME_Stranger_Female, out result, englishOnly))
			{
				sb.Append(result);
			}
			else
			{
				sb.Append(GameImpl.Translate(NAME_Stranger, englishOnly));
			}
		}
		else if (Zombie)
		{
			if (Appearance.Gender == GenderType.Female && GameImpl.Instance.TryTranslate(NAME_Zombie_Female, out var result2, englishOnly))
			{
				sb.Append(result2);
			}
			else
			{
				sb.Append(GameImpl.Translate(NAME_Zombie, englishOnly));
			}
		}
		else if (Appearance.Gender == GenderType.Female && GameImpl.Instance.TryTranslate(NAME_Looter_Female, out result3, englishOnly))
		{
			sb.Append(result3);
		}
		else
		{
			sb.Append(GameImpl.Translate(NAME_Looter, englishOnly));
		}
	}

	public override int GetDisplayNameHashIfGeneric(bool noStrangers)
	{
		if (FirstName.Length > 0)
		{
			if (NameKnown || noStrangers || Session.Instance.Editor)
			{
				return 0;
			}
			return NAME_Stranger;
		}
		if (Zombie)
		{
			return NAME_Zombie;
		}
		return NAME_Looter;
	}

	public static int GetInfectionTypeStringHash(InfectionType infection)
	{
		return infection switch
		{
			InfectionType.Green => HUD_GreenStrain, 
			InfectionType.Blue => HUD_BlueStrain, 
			InfectionType.Red => HUD_RedStrain, 
			InfectionType.White => HUD_WhiteStrain, 
			InfectionType.Invisible => HUD_InvisibleStrain, 
			_ => 0, 
		};
	}

	public override void BuildSubDisplayName(StringBuilder sb)
	{
		if (Infection > InfectionType.None && Infection < InfectionType.Invisible && (Alive || Community == null || Community.IsZombieCommunity()))
		{
			int infectionTypeStringHash = GetInfectionTypeStringHash(Infection);
			if (infectionTypeStringHash != 0)
			{
				sb.Append(GameImpl.Translate(infectionTypeStringHash));
			}
			return;
		}
		if (Session.Instance.IsInMultiplayerGame())
		{
			PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(this);
			if (playerControllingCharacter != null)
			{
				PartyMember partyMemberByID = OnlineParty.Instance.GetPartyMemberByID(playerControllingCharacter.PlayerID);
				if (partyMemberByID != null)
				{
					sb.Append(partyMemberByID.GetPlayerName());
					return;
				}
			}
		}
		if (Community != null && Community.CommunityType == CommunityType.Psycho && Community.CommunityName.Type == GangNameType.TranslatedString && Appearance.Gender == GenderType.Female && Community.CommunityName.TranslatedStringKey == Community.COMMUNITY_Turned)
		{
			string value = GameImpl.Translate(Community.COMMUNITY_Turned_Female);
			if (!string.IsNullOrEmpty(value))
			{
				sb.Append(value);
				return;
			}
		}
		base.BuildSubDisplayName(sb);
	}

	public override void GetSubDisplayCol(out Color col, out Color col2)
	{
		if (Infection != InfectionType.None)
		{
			col = GameTerrain.MinimapSettings.GetInfectionCol(Infection);
			col2 = GameTerrain.MinimapSettings.GetInfectionCol2(Infection);
		}
		else
		{
			base.GetSubDisplayCol(out col, out col2);
		}
		if (!Alive)
		{
			col *= (Color)GameTerrain.MinimapSettings.DeadCol;
			col2 *= (Color)GameTerrain.MinimapSettings.DeadCol2;
		}
	}

	public bool KnowsName(Character other)
	{
		if (other == this)
		{
			return true;
		}
		if (Community == null)
		{
			return false;
		}
		if (Community == other.Community)
		{
			return true;
		}
		if (Community.CommunityType == CommunityType.Player)
		{
			if (other.NameKnown)
			{
				return true;
			}
		}
		else if (Community.GetRelationship(other.Community) >= CommunityRelationshipType.Known && !Community.IsAlwaysHostileCommunity() && !other.Community.IsAlwaysHostileCommunity())
		{
			return true;
		}
		if (InitialCommunity != null && InitialCommunity.CommunityType != CommunityType.Player)
		{
			if (Community == other.InitialCommunity)
			{
				return true;
			}
			if (InitialCommunity == other.Community)
			{
				return true;
			}
			if (InitialCommunity == other.InitialCommunity)
			{
				return true;
			}
			if (!InitialCommunity.IsAlwaysHostileCommunity())
			{
				if (InitialCommunity.GetRelationship(other.Community) >= CommunityRelationshipType.Known && !other.Community.IsAlwaysHostileCommunity())
				{
					return true;
				}
				if (InitialCommunity.GetRelationship(other.InitialCommunity) >= CommunityRelationshipType.Known && !other.InitialCommunity.IsAlwaysHostileCommunity())
				{
					return true;
				}
			}
		}
		if (Relationship.GetRelationship(this, other) != RelationshipType.None)
		{
			return true;
		}
		return false;
	}

	public void UnitySetupAudioSource()
	{
		Unity.VoiceAudioSource = Unity.Obj.AddComponent<AudioSource>();
		Unity.VoiceAudioSource.spatialBlend = 1f;
		Unity.VoiceAudioSource.minDistance = SoundManager.AudioRolloffMinDist;
		Unity.VoiceAudioSource.maxDistance = SoundManager.AudioRolloffMaxDist;
		Unity.VoiceAudioSource.RealisticRolloff();
		Unity.VoiceAudioSource.playOnAwake = false;
		Unity.OneShotAudioSource = Unity.Obj.AddComponent<AudioSource>();
		Unity.OneShotAudioSource.spatialBlend = 1f;
		Unity.OneShotAudioSource.minDistance = SoundManager.AudioRolloffMinDist;
		Unity.OneShotAudioSource.maxDistance = SoundManager.AudioRolloffMaxDist;
		Unity.OneShotAudioSource.RealisticRolloff();
		Unity.OneShotAudioSource.playOnAwake = false;
		Unity.FootstepAudioSource = Unity.Obj.AddComponent<AudioSource>();
		Unity.FootstepAudioSource.spatialBlend = 1f;
		Unity.FootstepAudioSource.minDistance = SoundManager.AudioRolloffMinDist;
		Unity.FootstepAudioSource.maxDistance = FootstepSoundDist;
		Unity.FootstepAudioSource.rolloffMode = AudioRolloffMode.Linear;
		Unity.FootstepAudioSource.playOnAwake = false;
	}

	public void UnitySetupRagdollBones(RagdollSettings ragdollSettings, string prefix)
	{
		for (int i = 0; i < ragdollSettings.Bones.Length; i++)
		{
			GameObject gameObject = Unity.Obj.FindChild(prefix + ragdollSettings.Bones[i].Name);
			GameObject gameObject2 = Unity.Obj.FindChild(prefix + ragdollSettings.Bones[i].ConnectedName);
			if (gameObject == null)
			{
				Debug.LogError("Couldn't find bone " + ragdollSettings.Bones[i].Name);
			}
			if (gameObject2 == null)
			{
				Debug.LogError("Couldn't find connected bone " + ragdollSettings.Bones[i].ConnectedName);
			}
			gameObject.layer = NonPhysicalLayer;
			ragdollSettings.Bones[i].CopyTo(gameObject, gameObject2, null);
		}
	}

	public virtual void UnitySetupAppearance()
	{
	}

	public virtual void UnityUpdateSkinColor()
	{
	}

	public void UnityUpdatePosition()
	{
		if (!(Unity.Obj != null))
		{
			return;
		}
		if (CurrentAnimState != AnimState.Ragdoll)
		{
			if (CarriedBy != null)
			{
				string unityBoneName = CarriedBy.GetUnityBoneName(Bone.RightForearm);
				GameObject gameObject = CarriedBy.GetPredictedOrElseThisCharacter().Unity.Obj.FindChild(unityBoneName).gameObject;
				Quaternion localRotation = gameObject.transform.rotation * Quaternion.Slerp(CarriedRotStart, Quaternion.Euler(GetCarryRotX(), GetCarryRotY(), GetCarryRotZ()), CarriedTransition);
				Vector3 localPosition = gameObject.transform.localToWorldMatrix.MultiplyPoint(Vector3.Lerp(CarriedOffsetStart, new Vector3(GetCarryOffsetX(), GetCarryOffsetY(), GetCarryOffsetZ()), CarriedTransition));
				Unity.Obj.transform.localPosition = localPosition;
				Unity.Obj.transform.localRotation = localRotation;
			}
			else
			{
				Unity.Obj.transform.localPosition = Position + new Vector3(0f, AnimHeight, 0f);
				Unity.Obj.transform.localRotation = Quaternion.Euler(0f, FacingAngle * 57.29578f, 0f);
			}
		}
		else
		{
			Unity.Obj.transform.localPosition = GameTerrain.Instance.ClampPosWithinBounds(Unity.Obj.transform.localPosition, 1f);
		}
	}

	public override void UnityUpdateLayer(bool pip, Vector3 camPos, float farClipPlane)
	{
		if (pip)
		{
			if (GameImpl.Instance.Settings.PiPBackgroundEnabled)
			{
				float radius = Radius;
				float magnitude = (PosXZ - MathUtil.ToXZ(camPos)).magnitude;
				SetVisibleForRendering(pip, this == Hud.Instance.Pip.FocusObject || CarriedBy == Hud.Instance.Pip.FocusObject || magnitude >= radius);
			}
			else
			{
				SetVisibleForRendering(pip, visible: true);
			}
		}
		else
		{
			SetVisibleForRendering(pip, IsVisibleInFogOfWar());
		}
	}

	public void SetVisibleForRendering(bool pip, bool visible)
	{
		int unityLayer = ((!visible) ? InvisibleLayer : ((pip && !GameImpl.Instance.Settings.PiPBackgroundEnabled) ? PipLayer : CharactersLayer));
		SetUnityLayer(unityLayer);
		if (pip || Unity.Canvases == null)
		{
			return;
		}
		int layer = (visible ? WorldUILayer : InvisibleLayer);
		foreach (Canvas canvase in Unity.Canvases)
		{
			canvase.gameObject.layer = layer;
		}
	}

	public void SetUnityLayer(int desiredLayer)
	{
		if (Unity.SkinnedMeshRenderers != null)
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in Unity.SkinnedMeshRenderers)
			{
				skinnedMeshRenderer.gameObject.layer = desiredLayer;
			}
		}
		if (Unity.MeshRenderers != null)
		{
			foreach (MeshRenderer meshRenderer in Unity.MeshRenderers)
			{
				meshRenderer.gameObject.layer = desiredLayer;
			}
		}
		if (Unity.WeaponObj != null)
		{
			Prop.SetLayerRecursively(Unity.WeaponObj, desiredLayer);
		}
		if (Unity.LoadedAmmoObj != null)
		{
			Prop.SetLayerRecursively(Unity.LoadedAmmoObj, desiredLayer);
		}
		if (Unity.LeftHandItemObj != null)
		{
			Prop.SetLayerRecursively(Unity.LeftHandItemObj, desiredLayer);
		}
		if (Unity.UnconsciousEffect != null)
		{
			Prop.SetLayerRecursively(Unity.UnconsciousEffect.gameObject, desiredLayer);
		}
		if (Unity.MuzzleFlash != null)
		{
			Prop.SetLayerRecursively(Unity.MuzzleFlash.gameObject, desiredLayer);
		}
	}

	private void UnityAddInjuryDecals(Armor armor, List<Injury> injuries, DecalType decalType, SkinnedMeshRenderer renderer)
	{
		int num = 0;
		for (int i = 0; i < injuries.Count; i++)
		{
			if (injuries[i].GetDecalType() == decalType)
			{
				TmpInvDecalTransform[num] = injuries[i].InvDecalTransform;
				TmpDecalTexIndex[num] = injuries[i].DecalTexIndex;
				num++;
				if (num >= 16)
				{
					break;
				}
			}
		}
		if (decalType == DecalType.ArmorCrack)
		{
			bool flag = armor != null && armor.Protection == 0f;
			Vector2 vector = MathUtil.RandomVec2(new Vector2(Id, Id + 34543));
			renderer.material.SetTexture(ShaderHash._OverlayTex, (Texture2D)OverlayTex);
			renderer.material.SetTexture(ShaderHash._OverlayNormal, (Texture2D)OverlayNormal);
			renderer.material.SetVector(ShaderHash._OverlayOffsetAndAmount, new Vector3(vector.x, vector.y, flag ? 1f : 0f));
			renderer.material.SetTexture(ShaderHash._DecalTex, (Texture2D)CrackDecalTex);
			renderer.material.SetTexture(ShaderHash._DecalNormals, (Texture2D)CrackDecalNormals);
		}
		else
		{
			renderer.material.SetTexture(ShaderHash._DecalTex, (Texture2D)BloodDecalTex);
			renderer.material.SetTexture(ShaderHash._DecalNormals, (Texture2D)BloodDecalNormals);
		}
		renderer.material.SetInt(ShaderHash._NumDecals, num);
		renderer.material.SetMatrixArray(ShaderHash._DecalInvTransform, TmpInvDecalTransform);
		renderer.material.SetFloatArray(ShaderHash._DecalTexIndex, TmpDecalTexIndex);
	}

	public void UnityUpdateDecals()
	{
		if (!HasUnityObject())
		{
			return;
		}
		WantUnityUpdateDecals = false;
		for (int i = 0; i < Unity.SkinnedMeshRenderers.Count; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = Unity.SkinnedMeshRenderers[i];
			if (skinnedMeshRenderer.material.shader.name == "Custom/Character/Skin" || skinnedMeshRenderer.material.shader.name == "Custom/Character/Face" || skinnedMeshRenderer.material.shader.name == "Custom/Character/Clothes")
			{
				if (skinnedMeshRenderer.gameObject.tag == Human.ArmorTag)
				{
					if (Clothes[6] is Armor armor)
					{
						UnityAddInjuryDecals(armor, armor.ArmorDamagePoints, DecalType.ArmorCrack, skinnedMeshRenderer);
					}
				}
				else if (skinnedMeshRenderer.gameObject.tag == Human.LegArmorTag)
				{
					if (Clothes[7] is Armor armor2)
					{
						UnityAddInjuryDecals(armor2, armor2.ArmorDamagePoints, DecalType.ArmorCrack, skinnedMeshRenderer);
					}
				}
				else if (skinnedMeshRenderer.gameObject.tag == Human.HelmetTag)
				{
					if (Clothes[0] is Armor armor3)
					{
						UnityAddInjuryDecals(armor3, armor3.ArmorDamagePoints, DecalType.ArmorCrack, skinnedMeshRenderer);
					}
				}
				else
				{
					UnityAddInjuryDecals(null, Injuries, DecalType.Blood, skinnedMeshRenderer);
				}
			}
			if (skinnedMeshRenderer.material.shader.name == "Custom/Character/Clothes")
			{
				Vector2 vector = MathUtil.RandomVec2(new Vector2(Id + 82364, Id + 74389));
				skinnedMeshRenderer.material.SetVector(ShaderHash._GrungeOffset, vector);
			}
		}
	}

	public virtual float GetArrowScale()
	{
		return 1f;
	}

	public virtual Bone GetPipBoneToFocusOnWhenDead()
	{
		return Bone.Head;
	}

	private void UnityAddStuckArrows(List<Injury> injuries)
	{
		for (int i = 0; i < injuries.Count; i++)
		{
			if (!injuries[i].ArrowStuck)
			{
				continue;
			}
			InjuryBehaviour injuryBehaviour = FindUnityInjuryObject(injuries[i].InjuryId);
			if (injuryBehaviour == null)
			{
				GameObject unityBone = GetUnityBone(injuries[i].Bone);
				if (unityBone != null)
				{
					if (Unity.InjuryObjects == null)
					{
						Unity.InjuryObjects = new List<InjuryBehaviour>();
					}
					float arrowScale = GetArrowScale();
					injuryBehaviour = UnityEngine.Object.Instantiate(InjuryArrow.GetAsset(), unityBone.transform, worldPositionStays: false).GetComponent<InjuryBehaviour>();
					injuryBehaviour.InjuryId = injuries[i].InjuryId;
					injuryBehaviour.transform.localPosition = injuries[i].PosInBoneSpace - injuries[i].DirInBoneSpace * 0.75f;
					injuryBehaviour.transform.localRotation = Quaternion.LookRotation(injuries[i].DirInBoneSpace);
					injuryBehaviour.transform.localScale = Vector3.one * arrowScale;
					Unity.InjuryObjects.Add(injuryBehaviour);
					UnityUpdateMeshList();
				}
			}
			if (injuryBehaviour != null)
			{
				FoundInjuriesList.Add(injuryBehaviour);
			}
		}
	}

	public void UnityUpdateInjuries()
	{
		if (!HasUnityObject())
		{
			return;
		}
		WantUnityUpdateInjuries = false;
		FoundInjuriesList.Clear();
		UnityAddStuckArrows(Injuries);
		Equipment[] clothes = Clothes;
		for (int i = 0; i < clothes.Length; i++)
		{
			if (clothes[i] is Armor armor)
			{
				UnityAddStuckArrows(armor.ArmorDamagePoints);
			}
		}
		if (IsBurning())
		{
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in Unity.SkinnedMeshRenderers)
			{
				InjuryBehaviour injuryBehaviour = FindUnityChildInjuryObject(skinnedMeshRenderer.gameObject, 0);
				if (injuryBehaviour == null)
				{
					if (Unity.InjuryObjects == null)
					{
						Unity.InjuryObjects = new List<InjuryBehaviour>();
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(InjuryBurning.GetAsset(), skinnedMeshRenderer.transform, worldPositionStays: false);
					injuryBehaviour = gameObject.GetComponent<InjuryBehaviour>();
					injuryBehaviour.InjuryId = 0;
					Unity.InjuryObjects.Add(injuryBehaviour);
					UnityUpdateMeshList();
					ParticleSystem.ShapeModule shape = gameObject.GetComponent<ParticleSystem>().shape;
					shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
					shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
					shape.skinnedMeshRenderer = skinnedMeshRenderer;
				}
				if (injuryBehaviour != null)
				{
					FoundInjuriesList.Add(injuryBehaviour);
				}
			}
		}
		if (Unity.InjuryObjects != null)
		{
			for (int num = Unity.InjuryObjects.Count - 1; num >= 0; num--)
			{
				if (!FoundInjuriesList.Contains(Unity.InjuryObjects[num]))
				{
					UnityEngine.Object.DestroyImmediate(Unity.InjuryObjects[num].gameObject);
					Unity.InjuryObjects.RemoveAt(num);
					UnityUpdateMeshList();
				}
			}
		}
		FoundInjuriesList.Clear();
	}

	public InjuryBehaviour FindUnityInjuryObject(int injuryId)
	{
		if (Unity.InjuryObjects != null)
		{
			foreach (InjuryBehaviour injuryObject in Unity.InjuryObjects)
			{
				if (injuryObject.InjuryId == injuryId)
				{
					return injuryObject;
				}
			}
		}
		return null;
	}

	public InjuryBehaviour FindUnityChildInjuryObject(GameObject obj, int injuryId)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			InjuryBehaviour component = obj.transform.GetChild(i).GetComponent<InjuryBehaviour>();
			if (component != null && component.InjuryId == injuryId)
			{
				return component;
			}
		}
		return null;
	}

	public void UnityUpdateOverheadIcons()
	{
		Hud instance = Hud.Instance;
		if (instance == null || Usage != CharacterUsage.Normal)
		{
			return;
		}
		Session instance2 = Session.Instance;
		float unityHeadHeight = GetUnityHeadHeight();
		Transform transform = HudBehaviour.Instance.UnityGameCamera.transform;
		Character authoritativeOrElseThisCharacter = GetAuthoritativeOrElseThisCharacter();
		bool flag = false;
		bool flag2 = false;
		Color color = (IsInPlayerCommunity() ? GameTerrain.MinimapSettings.FriendCol : GameTerrain.MinimapSettings.AllyCol);
		bool flag3 = authoritativeOrElseThisCharacter == instance.LocalTargetObject && Alive;
		if ((instance.FocusedCharacterArrowTimeout > 0f || instance2.PlaySpeed == PlaySpeed.Paused) && authoritativeOrElseThisCharacter.IsInSameSquad(instance.LocalControlledCharacter))
		{
			bool num = authoritativeOrElseThisCharacter == instance.LocalControlledCharacter;
			flag = !num;
			flag2 = num;
			flag3 = true;
		}
		else if (instance2.PlaySpeed == PlaySpeed.Paused && instance.LocalControlledCharacter != null && instance.LocalControlledCharacter.FindActiveGoal(GoalType.Attack) is Attack attack && attack.Target.Object == this)
		{
			flag2 = true;
			color = GameTerrain.MinimapSettings.EnemyCol;
		}
		bool flag4 = false;
		float craftingProgress = 0f;
		Recipe recipe = null;
		Equipment usingItem = null;
		UnderConstructionInfo underConstructionInfo = null;
		bool wantCheckEquipmentPolicy = true;
		if (authoritativeOrElseThisCharacter == instance.LocalControlledCharacter || authoritativeOrElseThisCharacter == instance.LocalTargetObject)
		{
			flag4 = GetCraftingProgressForDisplay(out craftingProgress, out recipe, out usingItem, out underConstructionInfo, out wantCheckEquipmentPolicy);
		}
		if (flag && Unity.FocusedArrow == null)
		{
			Unity.FocusedArrow = UnityEngine.Object.Instantiate((GameObject)Hud.FocusedArrow, Unity.Obj.transform, worldPositionStays: false).GetComponent<RawImage>();
			UnityUpdateCanvasList();
		}
		if (flag2 && Unity.FocusedArrowLarge == null)
		{
			Unity.FocusedArrowLarge = UnityEngine.Object.Instantiate((GameObject)Hud.FocusedArrowLarge, Unity.Obj.transform, worldPositionStays: false).GetComponent<RawImage>();
			UnityUpdateCanvasList();
		}
		if (flag3 && Unity.BloodLossBar == null)
		{
			Unity.BloodLossBar = UnityEngine.Object.Instantiate((GameObject)Hud.BloodLossBar, Unity.Obj.transform, worldPositionStays: false);
			UnityUpdateCanvasList();
		}
		if (flag4 && Unity.CraftingProgressIndicator == null)
		{
			Unity.CraftingProgressIndicator = UnityEngine.Object.Instantiate((GameObject)Hud.CraftingProgressIndicator, Unity.Obj.transform, worldPositionStays: false).GetComponent<RawImage>();
			Unity.CraftingProgressIndicator.material = new Material(Unity.CraftingProgressIndicator.material);
			UnityUpdateCanvasList();
		}
		if (Unity.FocusedArrow != null)
		{
			Unity.FocusedArrow.gameObject.SetActive(flag);
		}
		if (Unity.FocusedArrowLarge != null)
		{
			Unity.FocusedArrowLarge.gameObject.SetActive(flag2);
		}
		if (Unity.BloodLossBar != null)
		{
			Unity.BloodLossBar.gameObject.SetActive(flag3);
		}
		if (Unity.CraftingProgressIndicator != null)
		{
			Unity.CraftingProgressIndicator.gameObject.SetActive(flag4);
		}
		float num2 = Mathf.Lerp(1f, FocusArrowScaleFlyMode, instance2.GameCamera.FlyCamTransition);
		float num3 = OverheadIconsYOffset;
		if (instance.LocalControlledCharacter != null)
		{
			num3 = Mathf.Lerp(-0.05f, OverheadIconsYOffset, Mathf.Clamp01(Math.Max(instance2.GameCamera.ZoomDist - 2f, (instance.LocalControlledCharacter.PosXZ - PosXZ).magnitude - 2f)));
		}
		if (instance.LocalWantLockOnTarget && authoritativeOrElseThisCharacter == instance.LocalTargetObject && this is Rabbit)
		{
			num3 += OverheadIconExtraOffsetWhenRabbitTargetted;
		}
		RawImage rawImage = (flag ? Unity.FocusedArrow : (flag2 ? Unity.FocusedArrowLarge : null));
		if (rawImage != null)
		{
			num3 += FocusArrowYOffset;
			rawImage.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
			rawImage.transform.rotation = transform.rotation;
			rawImage.transform.localScale = Vector3.one * FocusArrowScale * num2;
			rawImage.color = color;
			num3 += FocusArrowYOffset;
		}
		if (flag3)
		{
			num3 += BloodLossBarYOffset;
			Unity.BloodLossBar.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
			Unity.BloodLossBar.transform.rotation = transform.rotation;
			Unity.BloodLossBar.transform.localScale = Vector3.one * BloodLossBarScale * num2;
			ProgressBarBehaviour component = Unity.BloodLossBar.transform.GetChild(0).GetComponent<ProgressBarBehaviour>();
			ProgressBarBehaviour component2 = Unity.BloodLossBar.transform.GetChild(2).GetComponent<ProgressBarBehaviour>();
			int num4 = ((!Zombie) ? GetSkillLevelWithEffects(SkillType.Constitution) : 0);
			component.SetValue(BloodLoss);
			component2.gameObject.SetActive(num4 > 0);
			if (num4 > 0)
			{
				component2.SetValue(Mathf.Clamp01((BloodLoss - 1f) / (float)num4));
				component2.SetWidth((float)num4 * 10f);
				component2.SetTicks(num4 - 1);
				component2.SetFillOutlineEnabled(enabled: false);
			}
			Armor armor = Clothes[0] as Armor;
			Armor armor2 = Clothes[6] as Armor;
			Armor armor3 = Clothes[7] as Armor;
			Transform child = Unity.BloodLossBar.transform.GetChild(1);
			child.GetChild(0).gameObject.SetActive(armor != null);
			child.GetChild(1).gameObject.SetActive(armor2 != null);
			child.GetChild(2).gameObject.SetActive(armor3 != null);
			if (armor != null)
			{
				child.GetChild(0).gameObject.GetComponent<RawImage>().color = armor.GetArmorIconCol();
			}
			if (armor2 != null)
			{
				child.GetChild(1).gameObject.GetComponent<RawImage>().color = armor2.GetArmorIconCol();
			}
			if (armor3 != null)
			{
				child.GetChild(2).gameObject.GetComponent<RawImage>().color = armor3.GetArmorIconCol();
			}
			num3 += BloodLossBarYOffset;
		}
		if (flag4)
		{
			num3 += CraftingProgressIndicatorYOffset;
			Unity.CraftingProgressIndicator.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
			Unity.CraftingProgressIndicator.transform.rotation = transform.rotation;
			Unity.CraftingProgressIndicator.transform.localScale = Vector3.one * CraftingProgressIndicatorScale * num2;
			Unity.CraftingProgressIndicator.material.SetFloat(ShaderHash._FillAmount, craftingProgress);
			num3 += CraftingProgressIndicatorYOffset;
			PopulateCraftingProgressDisplay(Unity.CraftingProgressIndicator.gameObject, this, craftingProgress, recipe, usingItem, underConstructionInfo, underConstructionInfo?.IngredientsUsed, wantCheckEquipmentPolicy);
		}
		bool flag5 = Speaking != null && GetAuthoritativeOrElseThis() != Hud.Instance.Pip.FocusObject && Hud.Instance.Pip.FocusObjectLastUpdatedTime > SpeechStartTime;
		if (flag5 && Unity.SpeechBubble == null)
		{
			Unity.SpeechBubble = UnityEngine.Object.Instantiate((GameObject)Hud.WorldSpeechBubble, Unity.Obj.transform, worldPositionStays: false).GetComponent<SpeechBubbleBehaviour>();
			UnityUpdateCanvasList();
		}
		if (Unity.SpeechBubble != null)
		{
			Unity.SpeechBubble.gameObject.SetActive(flag5 || !Unity.SpeechBubble.IsFinished());
			if (flag5)
			{
				Unity.SpeechBubble.SetSpeaking(Speaking, SpeechStartTime, SpeakingText, SpeakingTextEnglish, forceUpdate: false);
			}
			if (Unity.SpeechBubble.gameObject.activeSelf)
			{
				num3 += SpeechBubbleYOffset;
				Unity.SpeechBubble.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
				Unity.SpeechBubble.transform.rotation = transform.rotation;
				Unity.SpeechBubble.Scale = num2;
				num3 += ((RectTransform)Unity.SpeechBubble.transform).rect.height * Unity.SpeechBubble.transform.localScale.y / num2 - SpeechBubbleYOffset;
			}
		}
		Texture2D texture2D = null;
		if (IsOutdoors())
		{
			texture2D = authoritativeOrElseThisCharacter.GetOverheadActionIcon();
			if (texture2D != (Texture2D)GameCursor.AlertIcon && texture2D != (Texture2D)GameCursor.NoseIcon && texture2D != (Texture2D)GameCursor.EarIcon && texture2D != (Texture2D)GameCursor.EyeIcon && (Community == null || (Community.CommunityType != CommunityType.Player && !CanFollowPlayer)))
			{
				texture2D = null;
			}
		}
		if (texture2D != null && Unity.OverheadActionIcon == null)
		{
			Unity.OverheadActionIcon = UnityEngine.Object.Instantiate((GameObject)Hud.OverheadActionIcon, Unity.Obj.transform, worldPositionStays: false).GetComponent<RawImage>();
			Unity.OverheadActionIcon.material = new Material(Unity.OverheadActionIcon.material);
			UnityUpdateCanvasList();
		}
		if (Unity.OverheadActionIcon != null)
		{
			Unity.OverheadActionIcon.gameObject.SetActive(texture2D != null);
		}
		if (texture2D != null)
		{
			float value = 0f;
			Color color2 = Color.white;
			if (texture2D == (Texture2D)GameCursor.CamouflageIcon)
			{
				value = authoritativeOrElseThisCharacter.CalcCamouflageIconFillAmount();
				color2 = new Color(0f, 0.5f, 0f);
			}
			Unity.OverheadActionIcon.material.SetFloat(ShaderHash._FillAmount, value);
			Unity.OverheadActionIcon.material.color = color2;
			Unity.OverheadActionIcon.texture = texture2D;
			num3 += OverheadActionIconYOffset;
			Unity.OverheadActionIcon.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
			Unity.OverheadActionIcon.transform.rotation = transform.rotation;
			Unity.OverheadActionIcon.transform.localScale = Vector3.one * FocusArrowScale * num2;
			num3 += OverheadActionIconYOffset;
		}
		Unity.OverheadIconsTopY = num3;
		int i = 0;
		if (instance2.IsInMultiplayerGame())
		{
			bool flag6 = instance2.IsLocalPlayerControllingCharacter(authoritativeOrElseThisCharacter);
			for (PlayerMode playerMode = PlayerMode.Controlling; playerMode <= PlayerMode.Observing; playerMode++)
			{
				foreach (PlayerRecord playerRecord in instance2.PlayerRecords)
				{
					if (playerRecord.PlayerCharacter != authoritativeOrElseThisCharacter || playerRecord.PlayerMode != playerMode)
					{
						continue;
					}
					PartyMember partyMemberByID = GameImpl.Instance.OnlineParty.GetPartyMemberByID(playerRecord.PlayerID);
					if (partyMemberByID != null)
					{
						if (Unity.PlayerNames == null)
						{
							Unity.PlayerNames = UnityEngine.Object.Instantiate((GameObject)Hud.PlayerNamePrefab, Unity.Obj.transform, worldPositionStays: false);
							UnityUpdateCanvasList();
						}
						if (i < Unity.PlayerNames.transform.childCount)
						{
							TextMeshProUGUI component3 = Unity.PlayerNames.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
							component3.gameObject.SetActive(value: true);
							string playerName = partyMemberByID.GetPlayerName();
							component3.SetUnityTextIfDifferent(playerName);
							float num5 = ((i == 0) ? 1f : 0.5f);
							component3.color = new Color(num5, num5, num5, Mathf.Clamp01((instance.LocalControlledCharacter == authoritativeOrElseThisCharacter && flag6) ? instance.PlayerNameTimeout : 1f));
						}
						i++;
					}
				}
			}
		}
		if (!(Unity.PlayerNames != null))
		{
			return;
		}
		Unity.PlayerNames.SetActive(i > 0);
		if (i > 0)
		{
			num3 += PlayerNameYOffset;
			Unity.PlayerNames.transform.position = GetUnityPos() + new Vector3(0f, unityHeadHeight, 0f) + transform.up * num2 * num3;
			Unity.PlayerNames.transform.rotation = transform.rotation;
			Unity.PlayerNames.transform.localScale = Vector3.one * PlayerNameScale * num2;
			num3 += ((RectTransform)Unity.PlayerNames.transform).rect.height * Unity.PlayerNames.transform.localScale.y;
			for (; i < Unity.PlayerNames.transform.childCount; i++)
			{
				Unity.PlayerNames.transform.GetChild(i).gameObject.SetActive(value: false);
			}
		}
	}

	public static GameObject PopulateCraftingProgressDisplay(GameObject craftingProgressIndicator, Character crafter, float craftingProgress, Recipe recipe, Equipment usingItem, UnderConstructionInfo underConstructionInfo, List<UsedIngredient> usedIngredients, bool wantCheckEquipmentPolicy)
	{
		GameObject gameObject = craftingProgressIndicator.transform.GetChild(0).gameObject;
		gameObject.transform.localScale = Vector3.one * Mathf.Lerp(Mathf.Lerp(0.1f, 1f, Mathf.Clamp01(Session.Instance.GameCamera.ZoomDist / 8f)), 1f, Session.Instance.GameCamera.FlyCamTransition);
		int i;
		for (i = 0; i < recipe.Ingredients.Count; i++)
		{
			Ingredient ingredient = recipe.Ingredients[i];
			GameObject gameObject2 = null;
			if (i < gameObject.transform.childCount)
			{
				gameObject2 = gameObject.transform.GetChild(i).gameObject;
			}
			else
			{
				gameObject2 = UnityEngine.Object.Instantiate(Hud.CraftingProgressResource.GetAsset(), gameObject.transform);
				gameObject2.transform.SetSiblingIndex(i);
			}
			RawImage component = gameObject2.transform.GetChild(0).GetComponent<RawImage>();
			RawImage component2 = gameObject2.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
			TextMeshProUGUI component3 = gameObject2.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
			Texture2D texture = null;
			Material mat = null;
			Color col = Color.white;
			bool flag = false;
			bool flag2 = false;
			InfectionType infectionType = InfectionType.None;
			if (!flag2 && ingredient.LiquidTypes != null && usingItem != null)
			{
				foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
				{
					if (usingItem.GetLiquidContentsType() == liquidType)
					{
						texture = ((usingItem.GetLiquidContentsType().Tex != null) ? usingItem.GetLiquidContentsType().Tex.GetAsset() : null);
						col = usingItem.GetLiquidContentsType().Col;
						infectionType = usingItem.InfectedWith;
						mat = null;
						flag = true;
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2 && ingredient.Prototypes != null && usingItem != null)
			{
				foreach (EquipmentPrototype prototype in ingredient.Prototypes)
				{
					if (usingItem.GetPrototype() == prototype)
					{
						texture = usingItem.GetIcon(out mat, out col, highlighted: false);
						infectionType = usingItem.InfectedWith;
						flag2 = true;
						break;
					}
				}
			}
			if (usedIngredients != null)
			{
				if (!flag2 && ingredient.LiquidTypes != null)
				{
					foreach (LiquidPrototype liquidType2 in ingredient.LiquidTypes)
					{
						LiquidPrototype liquidPrototype = null;
						foreach (UsedIngredient usedIngredient in usedIngredients)
						{
							if (usedIngredient.LiquidPrototype == liquidType2)
							{
								liquidPrototype = usedIngredient.LiquidPrototype;
								break;
							}
						}
						if (liquidPrototype != null)
						{
							texture = ((liquidPrototype.Tex != null) ? liquidPrototype.Tex.GetAsset() : null);
							col = liquidPrototype.Col;
							mat = null;
							flag = true;
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2 && ingredient.Prototypes != null)
				{
					foreach (EquipmentPrototype prototype2 in ingredient.Prototypes)
					{
						EquipmentPrototype equipmentPrototype = null;
						foreach (UsedIngredient usedIngredient2 in usedIngredients)
						{
							if (usedIngredient2.Prototype == prototype2)
							{
								equipmentPrototype = usedIngredient2.Prototype;
								break;
							}
						}
						if (equipmentPrototype != null)
						{
							texture = ((equipmentPrototype.Tex != null) ? equipmentPrototype.Tex.GetAsset() : null);
							mat = Hud.OutlineBlackMat;
							col = Color.white;
							flag2 = true;
							break;
						}
					}
				}
			}
			else
			{
				if (!flag2 && ingredient.LiquidTypes != null)
				{
					foreach (LiquidPrototype liquidType3 in ingredient.LiquidTypes)
					{
						if (crafter != null && crafter.Inventory.GetAmountOfLiquidType(crafter, liquidType3, out var ingredientsInfectedWith, ingredient.IngredientInfectionState, wantCheckEquipmentPolicy ? crafter : null) > 0f)
						{
							texture = ((liquidType3.Tex != null) ? liquidType3.Tex.GetAsset() : null);
							infectionType = ingredientsInfectedWith;
							col = liquidType3.Col;
							mat = null;
							flag = true;
							flag2 = true;
							break;
						}
					}
				}
				if (!flag2 && ingredient.Prototypes != null)
				{
					foreach (EquipmentPrototype prototype3 in ingredient.Prototypes)
					{
						Equipment equipment = crafter?.Inventory.FindItemOfTypePreferringUnused(crafter, crafter, prototype3, recipe, wantCheckEquipmentPolicy, ingredient.IngredientInfectionState);
						if (equipment != null)
						{
							texture = equipment.GetIcon(out mat, out col, highlighted: false);
							infectionType = equipment.InfectedWith;
							flag2 = true;
							break;
						}
					}
				}
			}
			if (!flag2)
			{
				if (ingredient.LiquidTypes != null && ingredient.LiquidTypes.Count > 0)
				{
					texture = ((ingredient.LiquidTypes[0].Tex != null) ? ingredient.LiquidTypes[0].Tex : null);
					col = ingredient.LiquidTypes[0].Col;
					mat = null;
					flag = true;
					flag2 = true;
				}
				else if (ingredient.Prototypes != null && ingredient.Prototypes.Count > 0)
				{
					texture = ((ingredient.Prototypes[0].Tex != null) ? ingredient.Prototypes[0].Tex : null);
					col = Color.white;
					mat = Hud.OutlineBlackMat;
					flag2 = true;
				}
			}
			component.texture = texture;
			component.material = mat;
			component.color = col;
			component2.gameObject.SetActive(infectionType != InfectionType.None);
			component2.color = GameTerrain.MinimapSettings.GetInfectionCol(infectionType);
			ingredientStringBuilder.Clear();
			if (underConstructionInfo != null)
			{
				float amountUsedOfIngredient = underConstructionInfo.GetAmountUsedOfIngredient(ingredient);
				if (flag)
				{
					ingredientStringBuilder.AppendWithoutGarbage(amountUsedOfIngredient, 1);
				}
				else
				{
					ingredientStringBuilder.Append((int)amountUsedOfIngredient);
				}
				ingredientStringBuilder.Append('/');
			}
			if (flag)
			{
				bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
				ingredientStringBuilder.AppendWithoutGarbage(ingredient.LiquidAmount * (useMetricWeights ? 0.0295735f : 1f), 1);
				ingredientStringBuilder.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
			}
			else
			{
				ingredientStringBuilder.Append(ingredient.Amount);
			}
			if (!ingredientStringBuilder.AreContentsIdentical(component3.text))
			{
				string text = ingredientStringBuilder.ToString();
				((RectTransform)gameObject2.transform).sizeDelta = new Vector2(component3.GetPreferredValues(text).x * component3.transform.localScale.x + component.rectTransform.rect.width + 0.025f, component.rectTransform.rect.height + 0.05f);
				component3.text = text;
			}
		}
		for (; i < gameObject.transform.childCount; i++)
		{
			UnityEngine.Object.DestroyImmediate(gameObject.transform.GetChild(i).gameObject);
		}
		return gameObject;
	}

	public bool GetCraftingProgressForDisplay(out float craftingProgress, out Recipe recipe, out Equipment usingItem, out UnderConstructionInfo underConstructionInfo, out bool wantCheckEquipmentPolicy)
	{
		Goal parent;
		if (CurrentActionAnim == ActionAnim.CraftStart)
		{
			if (GetAuthoritativeOrElseThisCharacter().FindActiveGoal(GoalType.CraftGoal) is CraftGoal { FollowingRecipe: not null } craftGoal)
			{
				craftingProgress = 0f;
				recipe = craftGoal.FollowingRecipe;
				usingItem = craftGoal.UsingItem;
				wantCheckEquipmentPolicy = craftGoal.WantCheckEquipmentPolicy(this);
				underConstructionInfo = null;
				return true;
			}
		}
		else if (CurrentActionAnim == ActionAnim.CraftLoop && GetAuthoritativeOrElseThisCharacter().FindActiveGoal(GoalType.CraftAnim, out parent) is CraftAnim { FollowingRecipe: not null } craftAnim)
		{
			craftingProgress = Mathf.Clamp01(craftAnim.TimeTaken / craftAnim.FollowingRecipe.CraftingTime);
			recipe = craftAnim.FollowingRecipe;
			usingItem = craftAnim.UsingItem;
			wantCheckEquipmentPolicy = !(parent is CraftGoal) || ((CraftGoal)parent).WantCheckEquipmentPolicy(this);
			underConstructionInfo = null;
			return true;
		}
		craftingProgress = 0f;
		recipe = null;
		underConstructionInfo = null;
		usingItem = null;
		wantCheckEquipmentPolicy = false;
		return false;
	}

	public float CalcCamouflageAmount()
	{
		float num = 1f;
		foreach (Character item in CharactersTargetingMe)
		{
			if (item != null && item.IsConscious && !item.Disappeared && item.GetBaseObjectType() != BaseObjectType.Chicken && (item.Community != Community || item.Zombie) && !item.CanFollowPlayer)
			{
				Target target = item.GetTarget(this);
				if (target != null)
				{
					num = Math.Min(num, target.Camouflage);
				}
			}
		}
		return num;
	}

	public float CalcCamouflageIconFillAmount()
	{
		using (new UnityProfileMarker(CalcCamouflageIconFillAmountStr))
		{
			return (CalcCamouflageAmount() * 112f + 8f) / 128f;
		}
	}

	public Character GetNearestAttacker()
	{
		Character result = null;
		float num = float.MaxValue;
		foreach (Character item in CharactersTargetingMe)
		{
			Goal goal = item.FindActiveGoal(item.Zombie ? GoalType.FeedOnLiving : GoalType.Attack);
			if (goal != null && goal.GetTargetObject() == this)
			{
				float sqrMagnitude = MathUtil.ToXZ(item.Position - Position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = item;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public bool IsAnyoneInConversationWithMe()
	{
		if (FindActiveGoal(GoalType.Conversation) is Conversation)
		{
			return true;
		}
		foreach (Character item in CharactersTargetingMe)
		{
			if (item.FindActiveGoal(GoalType.Conversation) is Conversation conversation2 && conversation2.GetTargetCharacter() == this)
			{
				return true;
			}
		}
		return false;
	}

	public void UnityUpdateAppearance()
	{
		if (HasUnityObject())
		{
			WantUnityUpdateAppearance = false;
			UnitySetupAppearance();
			if (CurrentAnimState == AnimState.Ragdoll)
			{
				UnitySaveProneBoneTransforms();
			}
			UnityAnimSetup();
		}
	}

	public void UnityOnChangedAppearance()
	{
		WantUnityUpdateAppearance = true;
		CanSkipUnityUpdate = false;
		if (IsAuthoritative())
		{
			IconIndex = -1;
			FullIconIndex = -1;
			if (IconGenerator.Instance.Queue.Processing.Equals(IconGenerationRequest.Full(this)))
			{
				IconGenerator.Instance.Queue.FinishedProcessing();
				IconGenerator.Instance.Queue.Push(IconGenerationRequest.Full(this));
				IconGimpMaster = null;
			}
		}
	}

	public void UnityOnChangedBones()
	{
		if (Predicted != null)
		{
			Predicted.UnityOnChangedBones();
			return;
		}
		AnimatorUpdateMode updateMode = AnimatorUpdateMode.Normal;
		AnimatorStateInfo[] array = new AnimatorStateInfo[6];
		bool flag = IsUnityObjectActive();
		if (flag)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Unity.Animator.GetCurrentAnimatorStateInfo(i);
			}
			updateMode = Unity.Animator.updateMode;
			UnityDeactivate();
		}
		UnityDelete();
		UnityInit();
		if (flag)
		{
			UnityActivate();
			Unity.Animator.updateMode = updateMode;
			for (int j = 0; j < array.Length; j++)
			{
				Unity.Animator.Play(array[j].shortNameHash, j, array[j].normalizedTime);
			}
		}
	}

	public override void UnityDelete()
	{
		if (Predicted != null)
		{
			PredictedObjectManager.Instance.RemovePredictedObject(Predicted);
		}
		base.UnityDelete();
		StopVoiceSound();
		if (!Unity.IsValid())
		{
			return;
		}
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in Unity.SkinnedMeshRenderers)
		{
			UnityEngine.Object.Destroy(skinnedMeshRenderer.material);
		}
		UnityEngine.Object.Destroy(Unity.Obj);
		Unity = default(UnityState);
	}

	public void UnityPlayInitialAnim()
	{
		if (Unity.Animator != null && Unity.Animator.isInitialized)
		{
			Unity.Animator.Play(GetUnityIdleAnimHash());
		}
		UnityAnimSetup();
		if (AnimWrapper != null)
		{
			Unity.Animator.SetFloat(AnimHash.WorkSpeed, AnimSpeed);
			if (AnimWrapper.UpperBodyStateNameHash != 0 && MovementSpeed > 0f)
			{
				Unity.Animator.PlayInFixedTime(AnimWrapper.UpperBodyStateNameHash, (!AnimWrapper.UpperBodyIsAdditive) ? 1 : 2, (float)GetActionAnimTime().TotalSeconds);
			}
			else
			{
				Unity.Animator.PlayInFixedTime(AnimWrapper.BaseStateNameHash, 0, (float)GetActionAnimTime().TotalSeconds);
			}
			if (AnimWrapper.EquipmentStateNameHash != 0 && Unity.WeaponAnimator != null)
			{
				Unity.WeaponAnimator.PlayInFixedTime(AnimWrapper.EquipmentStateNameHash, 0, (float)GetActionAnimTime().TotalSeconds);
			}
		}
		Unity.UnityActionAnim = CurrentActionAnim;
	}

	public virtual int GetUnityIdleAnimHash()
	{
		return AnimHash.Idle;
	}

	public void UnityAnimSetup()
	{
		if (Unity.Animator != null)
		{
			AnimState currentAnimState = CurrentAnimState;
			if ((uint)(currentAnimState - 1) <= 2u)
			{
				UnityLoadProneBoneTransforms();
			}
			UnityUpdateRagdollEnabled(retainVelocity: true);
		}
	}

	public void UnityEnsureAnimStateIsCorrect()
	{
		if (Unity.UnityAnimState != CurrentAnimState)
		{
			UnityUpdateRagdollEnabled(retainVelocity: true);
		}
		if (Unity.UnityActionAnim != CurrentActionAnim)
		{
			if (CurrentActionAnim == ActionAnim.None)
			{
				Unity.Animator.Play(GetUnityIdleAnimHash(), 0);
			}
			else if (AnimWrapper != null)
			{
				Unity.Animator.SetFloat(AnimHash.WorkSpeed, AnimSpeed);
				if (AnimWrapper.UpperBodyStateNameHash != 0 && MovementSpeed > 0f)
				{
					Unity.Animator.PlayInFixedTime(AnimWrapper.UpperBodyStateNameHash, (!AnimWrapper.UpperBodyIsAdditive) ? 1 : 2, (float)GetActionAnimTime().TotalSeconds);
				}
				else
				{
					Unity.Animator.PlayInFixedTime(AnimWrapper.BaseStateNameHash, 0, (float)GetActionAnimTime().TotalSeconds);
				}
				if (AnimWrapper.EquipmentStateNameHash != 0 && Unity.WeaponAnimator != null)
				{
					Unity.WeaponAnimator.PlayInFixedTime(AnimWrapper.EquipmentStateNameHash, 0, (float)GetActionAnimTime().TotalSeconds);
				}
			}
			Unity.UnityActionAnim = CurrentActionAnim;
		}
		if (WantUnityUpdateAppearance)
		{
			UnityUpdateAppearance();
		}
		if (WantUnityUpdateDecals)
		{
			UnityUpdateDecals();
		}
		if (WantUnityUpdateInjuries)
		{
			UnityUpdateInjuries();
		}
	}

	public override void UnityInit()
	{
		base.UnityInit();
		UnitySetupIdBehaviour();
		for (int i = 0; i < Injuries.Count; i++)
		{
			Injury value = Injuries[i];
			value.SetupDecalEffect(this);
			Injuries[i] = value;
		}
		UnityUpdateInjuries();
	}

	public void UnitySetupCollisionShape()
	{
		if (Unity.Obj != null)
		{
			Unity.CollisionShape = new GameObject();
			Unity.CollisionShape.name = CollisionShapeStr;
			Unity.CollisionShape.transform.parent = Unity.Obj.transform;
			Unity.CollisionShape.layer = CharacterCapsuleLayer;
			CapsuleCollider capsuleCollider = Unity.CollisionShape.AddComponent<CapsuleCollider>();
			capsuleCollider.transform.localPosition = new Vector3(0f, Height * 0.5f, 0f);
			capsuleCollider.radius = Radius;
			capsuleCollider.height = Height;
			capsuleCollider.isTrigger = true;
		}
	}

	public override void UnityActivate()
	{
		if (Unity.Obj == null)
		{
			Debug.LogError("Trying to activate a unity object that hasn't been inited? " + GetDisplayNameString() + ", Alive: " + Alive + ", Zombie: " + Zombie + ", Disappeared: " + Disappeared);
		}
		else if (!IsUnityObjectActive())
		{
			base.UnityActivate();
			Unity.Obj.SetActive(value: true);
			UnityPlayInitialAnim();
			if (IsAiming() || (AnimWrapper != null && AnimWrapper.Aiming))
			{
				Unity.AimingUpDownAngle = CalcAimUpDownAngle();
			}
			else
			{
				Unity.AimingUpDownAngle = 0f;
			}
			if (CarryingObject != null)
			{
				CarryingObject.OnPickedUpBy(this, startTransition: false);
			}
			UnityUpdate();
		}
	}

	public override void UnityDeactivate()
	{
		if (Predicted != null)
		{
			PredictedObjectManager.Instance.RemovePredictedObject(Predicted);
		}
		if (Unity.Obj != null)
		{
			Unity.Obj.SetActive(value: false);
			Unity.OverheadIconsTopY = 0f;
		}
		base.UnityDeactivate();
	}

	public void UnityUpdateMeshList()
	{
		if (Unity.MeshRenderers == null)
		{
			Unity.MeshRenderers = new List<MeshRenderer>();
		}
		Unity.MeshRenderers.Clear();
		Unity.Obj.GetComponentsInChildren(Unity.MeshRenderers);
	}

	public void UnityUpdateCanvasList()
	{
		if (Unity.Canvases == null)
		{
			Unity.Canvases = new List<Canvas>();
		}
		Unity.Canvases.Clear();
		Unity.Obj.GetComponentsInChildren(Unity.Canvases);
	}

	public override bool HasUnityObject()
	{
		return Unity.Obj != null;
	}

	public override bool IsUnityObjectActive()
	{
		if (Unity.Obj != null)
		{
			return Unity.Obj.activeSelf;
		}
		return false;
	}

	public override GameObject GetUnityObject()
	{
		return Unity.Obj;
	}

	public override bool IsUnityObjectStatic()
	{
		return false;
	}

	public override bool WantUnityObjectToStayActive()
	{
		if (CurrentAnimState == AnimState.Ragdoll || CarryingObject != null)
		{
			return true;
		}
		return base.WantUnityObjectToStayActive();
	}

	public override bool WantUnityObjectVisible()
	{
		return DebugShowCharacters;
	}

	public static bool GetDebugShowCharacters()
	{
		return DebugShowCharacters;
	}

	public static void SetDebugShowCharacters(bool v)
	{
		DebugShowCharacters = v;
	}

	public override Vector3 GetUnityPos()
	{
		if (CurrentAnimState == AnimState.Ragdoll && IsUnityObjectActive())
		{
			Vector3 pos = GetUnityBoneTransform(Bone.Hips).Translation();
			return GameTerrain.Instance.ClampPosToSurface(pos);
		}
		return Position;
	}

	public float GetUnityPosY()
	{
		if (IsUnityObjectActive())
		{
			Vector3 vector = GetUnityBoneTransform(Bone.Hips).Translation();
			return Math.Max(vector.y - Appearance.OverallScale * HumanAppearance.UMAUnscaledHipsHeight, GameTerrain.Instance.GetTileHeightAtPos(vector.x, vector.z));
		}
		return Position.y;
	}

	public void OnEditorSave()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			RoleInfo value = Roles[i];
			switch (value.Role)
			{
			case Role.Guard:
				value.TargetLocation = Tile;
				if (Community != null)
				{
					Building nearestGuardPost = Community.GetNearestGuardPost(Tile, float.MaxValue);
					if (nearestGuardPost != null)
					{
						value.TargetLocation = nearestGuardPost.Tile;
					}
				}
				break;
			case Role.Cook:
				value.TargetLocation = Tile;
				if (Community != null && Community.GetNearestBuildingOfType(Tile, typeof(Campfire), float.MaxValue) is Campfire campfire)
				{
					value.TargetLocation = campfire.Tile;
				}
				break;
			case Role.Farmer:
				value.TargetLocation = Tile;
				if (Community != null)
				{
					CropPatch nearestPatch = Session.Instance.CropsManager.GetNearestPatch(Tile, Community.Id);
					if (nearestPatch != null)
					{
						value.TargetLocation = nearestPatch.CropsInPatch[0].Tile;
					}
				}
				break;
			case Role.Lumberjack:
			case Role.Miner:
				if (value.TargetLocation == TerrainCoord.Zero)
				{
					value.TargetLocation = TerrainCoord.Invalid;
				}
				break;
			}
			Roles[i] = value;
		}
	}

	public override void OnSpawn()
	{
		OnSpawnOrTimeJump();
		OldPosition = Position;
		DesiredFacingAngle = FacingAngle;
		LinkMuscleToStrengthSkill();
		base.OnSpawn();
		RegisterWithTriggerZones();
		FollowMePosXZ = PosXZ;
	}

	public void OnSpawnOrTimeJump()
	{
		LastMemoryUpdateTime = (LastSkillAtrophyTime = (LastUpdateTime = (LastThinkTime = Session.Instance.PlayTime)));
		FramesPerUpdate = 1;
	}

	public virtual void OnNewGame()
	{
		if (Rank != Rank.Captive && InitialCommunity == null)
		{
			InitialCommunity = Community;
		}
		InitialHangOutLocation = (HangOutLocation = Tile);
		InitialBandageCount = Inventory.CountItemsOfType(EquipmentPrototype.Bandage);
		int firstRoleIndex = GetFirstRoleIndex(Role.Lumberjack);
		if (firstRoleIndex != -1)
		{
			TreeProp nearestTree = GameTerrain.Instance.GetNearestTree(Tile);
			if (nearestTree != null)
			{
				RoleInfo value = Roles[firstRoleIndex];
				value.TargetLocation = nearestTree.GetCentreTile();
				Roles[firstRoleIndex] = value;
			}
		}
		UpdateVisibleTilesCache();
	}

	public override void Init()
	{
		base.Init();
		Session instance = Session.Instance;
		instance.CharacterManager.OnCharacterCreated(this);
		if (ThinkPriorityBucket != ThinkPriority.Dead)
		{
			instance.CharacterManager.ThinkBuckets[(int)ThinkPriorityBucket].Add(this);
		}
		if (instance.Editor && CurrentAnimState == AnimState.Ragdoll)
		{
			CurrentAnimState = AnimState.Animation;
		}
		UpdateWorldTransformAndBounds();
		UpdateCachedAStarInfo(force: true);
		if (InsideBuilding != null && InsideBuilding.Prototype == null)
		{
			Debug.LogWarning(GetDisplayNameString() + " was in a building with no prototype, leaving");
			InsideBuilding = null;
			InsideBuildingSlotIndex = -1;
			WasOrderedInsideBuilding = false;
		}
		if ((InsideBuilding == null || (InsideBuilding.Prototype != null && InsideBuilding.GetInhabitantSlotDefs()[InsideBuildingSlotIndex].External)) && !Disappeared)
		{
			AddToTerrain();
		}
		if (VelocityXZ.sqrMagnitude != 0f)
		{
			instance.CharacterManager.CollisionManager.AddToMovers(this);
		}
	}

	public override void AddToTerrain()
	{
		base.AddToTerrain();
		AddToCharacterMapWho();
		if (IsBurning())
		{
			AddToBurningMapWho();
		}
	}

	public override void RemoveFromTerrain()
	{
		RemoveFromBurningMapWho();
		RemoveFromCharacterMapWho();
		base.RemoveFromTerrain();
	}

	private void AddToCharacterMapWho()
	{
		if (!IsInMapWho)
		{
			GameTerrain.Instance.CharacterMapWho.AddToMapWho(this, Tile);
			IsInMapWho = true;
		}
	}

	private void RemoveFromCharacterMapWho()
	{
		if (IsInMapWho)
		{
			IsInMapWho = false;
			GameTerrain.Instance.CharacterMapWho.RemoveFromMapWho(this, Tile);
		}
	}

	private void AddToBurningMapWho()
	{
		if (!IsInBurningMapWho)
		{
			GameTerrain.Instance.BurningMapWho.AddToMapWho(this, Tile);
			IsInBurningMapWho = true;
		}
	}

	private void RemoveFromBurningMapWho()
	{
		if (IsInBurningMapWho)
		{
			IsInBurningMapWho = false;
			GameTerrain.Instance.BurningMapWho.RemoveFromMapWho(this, Tile);
		}
	}

	public override void InitPredicted()
	{
		base.InitPredicted();
		if (Authoritative.Unity.IsValid())
		{
			Unity = Authoritative.Unity;
			Authoritative.Unity = default(UnityState);
			Array.Copy(Authoritative.ProneBoneTransforms, ProneBoneTransforms, ProneBoneTransforms.Length);
			int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(Authoritative);
			if (num != -1)
			{
				Session.Instance.ActiveMovingUnityObjects[num] = this;
			}
		}
		else if (!Unity.IsValid())
		{
			Debug.LogError(GetDisplayNameString() + " is predicted but has no Unity Object.  Disappeared: " + Disappeared + ", Deleted: " + Deleted + ", Alive: " + Alive);
		}
		WantUnityUpdateAppearance |= Authoritative.WantUnityUpdateAppearance;
		WantUnityUpdateInjuries |= Authoritative.WantUnityUpdateInjuries;
		WantUnityUpdateDecals |= Authoritative.WantUnityUpdateDecals;
		Authoritative.WantUnityUpdateAppearance = false;
		Authoritative.WantUnityUpdateInjuries = false;
		Authoritative.WantUnityUpdateDecals = false;
		SpeakingText = Authoritative.SpeakingText;
		SpeakingTextEnglish = Authoritative.SpeakingTextEnglish;
		Authoritative.SpeakingEmoticons.CopyToList(SpeakingEmoticons);
		Array.Copy(Authoritative.CachedSkillLevelsWithEffects, CachedSkillLevelsWithEffects, CachedSkillLevelsWithEffects.Length);
		UpdateWorldTransformAndBounds();
		if (VelocityXZ.sqrMagnitude != 0f)
		{
			Session.Instance.PredictedObjectManager.PredictedCollisionManager.AddToMovers(this);
		}
	}

	public override void DeletePredicted()
	{
		SetGoal(null);
		SetDirectControlledTarget(null);
		ClearVelocityXZ();
		if (Authoritative.Deleted)
		{
			UnityDelete();
		}
		else
		{
			Authoritative.Unity = Unity;
			Unity = default(UnityState);
			int num = Session.Instance.ActiveMovingUnityObjects.IndexOf(this);
			if (num != -1)
			{
				Session.Instance.ActiveMovingUnityObjects[num] = Authoritative;
			}
		}
		base.DeletePredicted();
	}

	public override bool PropWantDelete()
	{
		return CharacterDeleted == DeletionState.WantDelete;
	}

	public void MarkForDeletion()
	{
		if (CharacterDeleted == DeletionState.None)
		{
			CharacterDeleted = DeletionState.WantDelete;
		}
	}

	public override void Delete()
	{
		if (InsideBuilding != null)
		{
			InsideBuilding.OnCharacterLeave(this, 0, fromBuildingDestroyed: false, fromRagdolled: false);
		}
		SetGoal(null);
		SetDirectControlledTarget(null);
		RemoveAllTargets();
		ClearVelocityXZ();
		if (Community != null)
		{
			Community.RemoveMember(this);
		}
		GameTerrain.Instance.AStar.AddChange(AStarChange.RemoveCharacter(this));
		UnregisterFromTriggerZones();
		RemoveFromTerrain();
		if (ThinkPriorityBucket != ThinkPriority.Dead)
		{
			Session.Instance.CharacterManager.ThinkBuckets[(int)ThinkPriorityBucket].Remove(this);
		}
		if (VisibleTilesCache != null)
		{
			VisibleTilesCache.RemoveRefs(this);
			VisibleTilesCache = null;
		}
		Session.Instance.CharacterManager.OnCharacterDeleted(this);
		Session.Instance.CommunityManager.OnCharacterDeletedOrDisappeared(this);
		StoryManager.Instance.OnCharacterDeleted(this);
		Session.Instance.OnCharacterDeleted(this);
		Inventory.DeleteAll(this, carrierBeingDeleted: true);
		base.Delete();
		CharacterDeleted = DeletionState.Deleted;
	}

	public void OnLoad(int version)
	{
		int num = ((Goal != null) ? Goal.CheckDisableSleepRefCount() : 0);
		if (DisableSleep != num)
		{
			Debug.Log("Incorrect DisableSleep refcount for " + GetDisplayNameString() + ", was: " + DisableSleep + ", should be: " + num + " - " + GetGoalDebugString());
			DisableSleep = num;
		}
		if (version < 587)
		{
			InitialBandageCount = Inventory.CountItemsOfType(EquipmentPrototype.Bandage);
		}
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role != Role.Miner)
			{
				continue;
			}
			MineralType mineralType = Roles[i].ResourceType?.GetMineralType() ?? MineralType.None;
			MineralType mineralType2 = mineralType;
			TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(Roles[i].TargetLocation.x, Roles[i].TargetLocation.y);
			if (fixedObjectOnTile != null)
			{
				if (fixedObjectOnTile is Mine mine)
				{
					if (!mine.HasRichDeposits(mineralType))
					{
						for (int j = 0; j < 4; j++)
						{
							if (mine.HasRichDeposits((MineralType)j))
							{
								mineralType2 = (MineralType)j;
								break;
							}
						}
					}
				}
				else if (fixedObjectOnTile.GetMineralType() != mineralType)
				{
					mineralType2 = fixedObjectOnTile.GetMineralType();
				}
			}
			if (mineralType2 != mineralType && mineralType2 > MineralType.None && mineralType2 < MineralType.Count)
			{
				RoleInfo value = Roles[i];
				value.ResourceType = EquipmentPrototype.MiningResources[(int)mineralType2];
				Roles[i] = value;
				if (FindActiveGoal(GoalType.MinerGoal) is MinerGoal minerGoal)
				{
					minerGoal.CurrentMiningResourceType = value.ResourceType;
				}
				if (FindActiveGoal(GoalType.MoveToAndMine) is MoveToAndMine moveToAndMine && moveToAndMine.GetTargetObject() == fixedObjectOnTile)
				{
					moveToAndMine.MineralType = mineralType2;
				}
				Debug.Log("Updated miner role for " + GetDisplayNameString() + " to " + mineralType2);
			}
		}
		UpdateVisibleTilesCache();
		if (!Disappeared)
		{
			RegisterWithTriggerZones();
		}
		if (Speaking != null)
		{
			Speech.BuildSpeechText(Speaking.TextHash, Speaking.Params, this, Listener, SpeechObject, SpeakingParamResults, out SpeakingText, SpeakingEmoticons, englishOnly: false, isQuest: false);
			Speech.BuildSpeechText(Speaking.TextHash, Speaking.Params, this, Listener, SpeechObject, SpeakingParamResults, out SpeakingTextEnglish, null, englishOnly: true, isQuest: false);
		}
		SanityCheckTargetRefCounts(onLoadingThread: true, null);
	}

	public void UpdatePreProcessInputFrame(PlayerRecord playerRecord)
	{
		if (DirectControlled)
		{
			MovementSpeed = 0f;
		}
		if (AIOverridesControl() != AIOverridesControlReason.None)
		{
			DirectControlled = false;
		}
	}

	public void UpdatePostProcessInputFrame(PlayerRecord playerRecord)
	{
		if (!CanReceiveUserInputs() || IsChokingSomeone())
		{
			return;
		}
		if (playerRecord.WantLockOnTarget && CurrentActionAnim != ActionAnim.HandsUp)
		{
			ClearSitting();
			DirectControlled = true;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			if (playerRecord.TargetObject != null)
			{
				DesiredFacingAngle = MathUtil.GetAngleFromNormalizedDir(MathUtil.SafeNormalize(MathUtil.ToXZ(playerRecord.TargetObject.GetBoundingBoxCentre() - Position), MathUtil.ToXZ(base.Forward)));
			}
			else
			{
				DesiredFacingAngle = MathUtil.GetAngleFromNormalizedDir(MathUtil.SafeNormalize(MathUtil.ToXZ(playerRecord.TargetPos - Position), MathUtil.ToXZ(base.Forward)));
			}
			if (EquippedItem is AmmoWeapon { CurrentAmmo: 0 } ammoWeapon && CurrentActionAnim == ActionAnim.None && Inventory.FindAmmoForWeapon(ammoWeapon) != null)
			{
				OnProcessReloadAction();
			}
		}
		if (DirectControlled && DesiredEquippedItem != EquippedItem)
		{
			TryStartActionAnim(ActionAnim.Unequip, null, canInterruptEqualPriority: false);
		}
	}

	public void OnProcessMoveAction(PlayerRecord playerRecord, Vector2 moveDir)
	{
		if (!CanReceiveUserInputs(out var _, out var reason))
		{
			if (reason != AIOverridesControlReason.Vaulting)
			{
				return;
			}
			Vector2 vector = MathUtil.ToXZ(base.Forward);
			float angleFromNormalizedDir = MathUtil.GetAngleFromNormalizedDir(MathUtil.SafeNormalize(moveDir, vector));
			angleFromNormalizedDir = Prop.GetAngleFromOrientationType(Prop.GetClosestOrientationTypeToAngle(angleFromNormalizedDir));
			moveDir = MathUtil.GetDirFromAngle(angleFromNormalizedDir);
			if (Mathf.Abs(Vector2.Dot(moveDir, vector)) >= Mathf.Cos(MathF.PI / 4f))
			{
				return;
			}
			TerrainCoord tile = Tile;
			if (!GameTerrain.Instance.IsImpassable(tile.x, tile.y, 0, this, null))
			{
				return;
			}
			TerrainCoord terrainCoord = new TerrainCoord(Mathf.RoundToInt(moveDir.x), Mathf.RoundToInt(moveDir.y));
			TerrainCoord terrainCoord2 = tile + terrainCoord;
			if (GameTerrain.Instance.IsImpassable(terrainCoord2.x, terrainCoord2.y, 1, this, null))
			{
				return;
			}
			TerrainCoord terrainCoord3 = GameTerrain.Instance.GetTileCoordForPosXZ(PosXZ + vector * Radius) + terrainCoord;
			if (!GameTerrain.Instance.IsImpassable(terrainCoord3.x, terrainCoord3.y, 1, this, null))
			{
				TerrainCoord terrainCoord4 = GameTerrain.Instance.GetTileCoordForPosXZ(PosXZ - vector * Radius) + terrainCoord;
				if (!GameTerrain.Instance.IsImpassable(terrainCoord4.x, terrainCoord4.y, 1, this, null))
				{
					SetFacingAngle(angleFromNormalizedDir);
				}
			}
			return;
		}
		if (InsideBuilding is EnterableVehicle enterableVehicle && enterableVehicle.GetDriverAuthoritative() == GetAuthoritativeOrElseThisCharacter())
		{
			if (enterableVehicle.IsAuthoritative() == IsAuthoritative())
			{
				enterableVehicle.OnProcessMoveAction(moveDir);
			}
			DirectControlled = true;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			return;
		}
		float num = 1f / 60f * (float)Session.Instance.GetFramesPerFrame();
		if (CurrentActionAnim == ActionAnim.Slide)
		{
			DirectControlled = true;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			Vector2 vector2 = MathUtil.RightNormal(MathUtil.SafeNormalize(MathUtil.ToXZ(GameTerrain.Instance.GetNormalAtPos(Position.x, Position.z)), Vector2.zero));
			float runSpeed = GetRunSpeed();
			Vector2 vector3 = Vector2.Dot(moveDir, vector2) * vector2;
			AddVelocityXZ(vector3 * num * runSpeed, setFollowMeDir: true);
			return;
		}
		CancelSurrendering();
		if (!IsCurrentActionAnimFromGoal && !CanMoveDuringActionAnim() && !IsActionAnimInterruptible())
		{
			return;
		}
		bool flag = IsCrouching();
		float num2 = (flag ? CrouchRunSpeed : GetRunSpeed());
		float num3 = (flag ? CrouchWalkSpeed : JogSpeed);
		if (!playerRecord.WantLockOnTarget && IsSprintEnabled())
		{
			num3 = ((!flag) ? ((GetFatigueMinusAdrenaline() >= 1f) ? OutOfBreathRunSpeed : GetRunSpeed()) : ((GetFatigueMinusAdrenaline() >= 1f) ? CrouchOutOfBreathRunSpeed : CrouchRunSpeed));
		}
		moveDir *= num2;
		float num4 = moveDir.magnitude;
		if (num4 > num3)
		{
			moveDir *= num3 / num4;
			num4 = num3;
		}
		DirectControlled = true;
		MarkPlayerControlled();
		CancelOneOffCrafting();
		if (IsSprintEnabled())
		{
			EnableSprint();
		}
		DesiredFacingAngle = MathUtil.GetAngleFromNormalizedDir(MathUtil.SafeNormalize(moveDir, MathUtil.ToXZ(base.Forward)));
		if (InsideBuilding == null)
		{
			AddVelocityXZ(moveDir * num, setFollowMeDir: true);
			MovementSpeed = num4;
			MovementAngle = MathUtil.GetAngleFromDir(moveDir, FacingAngle);
			if (!CanMoveDuringActionAnim())
			{
				ClearActionAnim();
			}
		}
	}

	public void OnProcessSprintAction(bool enable, bool automatic)
	{
		if (!CanReceiveUserInputs(out var constructing) && !constructing)
		{
			return;
		}
		if (DirectControlled)
		{
			if (enable)
			{
				EnableSprint();
			}
			else
			{
				DisableSprint();
			}
			MarkPlayerControlled();
			CancelOneOffCrafting();
		}
		else if (!automatic && Goal != null)
		{
			if (enable)
			{
				Goal.SetMovementType(this, MovementType.Run);
			}
			else
			{
				Goal.SetMovementType(this, MovementType.Walk);
			}
		}
	}

	public ActionAnim GetFireAction()
	{
		float animSpeed;
		return GetFireAction(out animSpeed);
	}

	public ActionAnim GetFireAction(out float animSpeed)
	{
		ActionAnim result = ActionAnim.Fire;
		animSpeed = 1f;
		if (EquippedItem is Bow bow)
		{
			if (!HasInfiniteAmmo(bow) && !Inventory.HasAmmoForWeapon(bow))
			{
				result = ActionAnim.FireLastArrow;
			}
			animSpeed = bow.GetReloadSpeed(this);
		}
		return result;
	}

	public bool IsLongEnoughSinceWeLastFired()
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (EquippedItem != null)
		{
			return currentTime - LastFireTime >= TimeSpan.FromSeconds(EquippedItem.GetMinTimeBetweenFiring());
		}
		return true;
	}

	public void OnProcessFireAction(PlayerRecord playerRecord)
	{
		if (!CanReceiveUserInputs())
		{
			return;
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (EquippedItem is RangedWeapon rangedWeapon && rangedWeapon.CanFire())
		{
			bool flag = IsLongEnoughSinceWeLastFired() && !playerRecord.MustLetGoToFireAgain;
			if (rangedWeapon is Bow)
			{
				flag &= currentTime - DirectControlledAimingStartTime >= TimeSpan.FromSeconds(0.5);
			}
			if (flag && TryStartActionAnim(GetFireAction(out var animSpeed), playerRecord.TargetObject, animSpeed))
			{
				DirectControlled = true;
				MarkPlayerControlled();
				CancelOneOffCrafting();
				if (!rangedWeapon.CanHoldTriggerToFire())
				{
					playerRecord.MustLetGoToFireAgain = true;
				}
			}
		}
		else
		{
			if (EquippedItem != null && !(EquippedItem is MeleeWeapon))
			{
				return;
			}
			Character character = playerRecord.TargetObject as Character;
			ActionAnim actionAnim = ActionAnim.None;
			actionAnim = ((EquippedItem != null) ? ((character != null && character.IsZombieJumping() && character.GetActionAnimTimeSpeedIndependant() < TimeSpan.FromSeconds(ZombieJumpGoal.DodgeTime)) ? ActionAnim.AttackJumpingZombie : ((character == null || character.CurrentActionAnim != ActionAnim.ZombieBiteStart) ? ActionAnim.Attack : ActionAnim.SnapAttack)) : ((character == null || !character.IsRagdollOrProneOrRecovering()) ? (MathUtil.RandomChoice((float)currentTime.TotalSeconds, 0.5f) ? ActionAnim.PunchLeft : ActionAnim.PunchRight) : ActionAnim.Kick));
			if (!playerRecord.MustLetGoToFireAgain && TryStartActionAnim(actionAnim, playerRecord.TargetObject, canInterruptEqualPriority: false))
			{
				DirectControlled = true;
				DirectControlledCrouching = false;
				MarkPlayerControlled();
				CancelOneOffCrafting();
				if ((actionAnim == ActionAnim.AttackJumpingZombie || actionAnim == ActionAnim.SnapAttack) && playerRecord.IsLocal && IsFrontmostPrediction() && Hud.Instance.ParryPromptAttacker == playerRecord.TargetObject && playerRecord.TargetObject is Character)
				{
					Hud.Instance.OnParrySucceeded(playerRecord.TargetObject as Character, HUD_Parry, -1f);
				}
				playerRecord.MustLetGoToFireAgain = true;
			}
		}
	}

	public void OnProcessKickAction(PlayerRecord playerRecord)
	{
		if (CanReceiveUserInputs() && !playerRecord.MustLetGoToFireAgain)
		{
			CancelSurrendering();
			if (TryStartActionAnim(ActionAnim.Kick, playerRecord.TargetObject, canInterruptEqualPriority: false))
			{
				DirectControlled = true;
				DirectControlledCrouching = false;
				MarkPlayerControlled();
				CancelOneOffCrafting();
				playerRecord.MustLetGoToFireAgain = true;
			}
		}
	}

	public void OnProcessParryAction(PlayerRecord playerRecord, Character attacker, ActionAnim action)
	{
		if (!CanReceiveUserInputs() || playerRecord.MustLetGoToFireAgain)
		{
			return;
		}
		bool flag = action == ActionAnim.Dodge_Backwards || action == ActionAnim.Dodge_Forwards || action == ActionAnim.Dodge_Left || action == ActionAnim.Dodge_Right;
		if (flag && IsTooTiredFor(DodgingFatiguePenalty))
		{
			return;
		}
		CancelSurrendering();
		if (TryStartActionAnim(action, attacker, canInterruptEqualPriority: false))
		{
			if (flag)
			{
				ApplyFatiguePenalty(DodgingFatiguePenalty);
			}
			ParryingAttacker = attacker;
			DodgingProjectile = null;
			DirectControlled = true;
			DirectControlledCrouching = false;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			if (playerRecord.IsLocal && IsFrontmostPrediction() && Hud.Instance.ParryPromptAttacker == attacker && attacker != null)
			{
				Hud.Instance.OnParrySucceeded(attacker, flag ? HUD_Dodge : HUD_Parry, -1f);
			}
			playerRecord.MustLetGoToFireAgain = true;
		}
	}

	public void OnProcessSurrenderAction(PlayerRecord playerRecord)
	{
		if (CanReceiveUserInputs() && !playerRecord.MustLetGoToFireAgain && playerRecord.TargetObject is Character targetCharacter)
		{
			StartPlayerSurrendering(targetCharacter, playerRecord);
		}
	}

	public void StartPlayerSurrendering(Character targetCharacter, PlayerRecord playerRecord)
	{
		if (!TryStartActionAnim(ActionAnim.HandsUp, targetCharacter, canInterruptEqualPriority: false))
		{
			return;
		}
		BaseObject obj = null;
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, targetCharacter, ref obj, SpeechSituation.Surrender, default(MemoryParam));
		if (speechForSituation != null)
		{
			CommandSpeakTo(playerRecord, targetCharacter, obj, default(MemoryParam), speechForSituation, null, null, default(MemoryParam), isDoubleClick: true);
		}
		foreach (PlayerRecord playerRecord2 in Session.Instance.PlayerRecords)
		{
			if (playerRecord2 != playerRecord && playerRecord2.PlayerMode == PlayerMode.Controlling && playerRecord2.PlayerCharacter != this && playerRecord2.PlayerCharacter != null)
			{
				playerRecord2.PlayerCharacter.TryStartActionAnim(ActionAnim.HandsUp, targetCharacter, canInterruptEqualPriority: false);
				playerRecord2.PlayerCharacter.DirectControlled = false;
			}
		}
	}

	public void CancelSurrendering()
	{
		if (Community == null || Community.CommunityType != CommunityType.Player)
		{
			return;
		}
		if (IsAuthoritative())
		{
			foreach (Character member in Community.Members)
			{
				member.StopActionAnim(ActionAnim.HandsUp);
			}
			return;
		}
		StopActionAnim(ActionAnim.HandsUp);
	}

	public void OnProcessEscapeAction(PlayerRecord playerRecord, bool held, int heldFrames)
	{
		DirectControlled = true;
		DirectControlledCrouching = false;
		CancelOneOffCrafting();
		if (!(InteractionObject is Character attacker) || playerRecord.MustLetGoToFireAgain)
		{
			return;
		}
		float zombieEscapePower = ZombieEscapePower;
		if (TryEscapeZombieBite(DirectControlledZombieEscapePower * (held ? ((float)heldFrames * (1f / 60f) * ChokeFakePressesPerSecondWhenHeld) : 1f)))
		{
			if (playerRecord.IsLocal && IsFrontmostPrediction())
			{
				Hud.Instance.OnParrySucceeded(attacker, HUD_Escape, 1f);
			}
			playerRecord.MustLetGoToFireAgain = true;
		}
		else if (ZombieEscapePower > zombieEscapePower)
		{
			if (playerRecord.IsLocal && IsFrontmostPrediction())
			{
				Hud.Instance.OnZombieEscapeButtonMashed(attacker);
			}
			playerRecord.MustLetGoToFireAgain = true;
		}
	}

	public void OnProcessChokeAction(PlayerRecord playerRecord, bool held, int heldFrames)
	{
		SetDirectControlled(value: true, wantSetLastInputTime: true, wantClearLeaderCommand: false);
		MarkPlayerControlled();
		CancelOneOffCrafting();
		if (!playerRecord.MustLetGoToFireAgain && ApplyChokeAction(playerRecord, held, heldFrames))
		{
			playerRecord.MustLetGoToFireAgain = true;
		}
	}

	private HoldType GetHoldTypeFromAnim()
	{
		if (CurrentActionAnim != ActionAnim.RestrainLoop)
		{
			if (CurrentActionAnim != ActionAnim.SlitThroatLoop)
			{
				return HoldType.ChokeHold;
			}
			return HoldType.SlitThroat;
		}
		return HoldType.Restrain;
	}

	private bool CouldChokeSuccessfully(Character victim, HoldType holdType)
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.HandToHand);
		float num = Mathf.Lerp(holdType switch
		{
			HoldType.SlitThroat => MinAssassinatePower, 
			HoldType.ChokeHold => MinChokePower, 
			_ => MinRestrainPower, 
		}, holdType switch
		{
			HoldType.SlitThroat => MaxAssassinatePower, 
			HoldType.ChokeHold => MaxChokePower, 
			_ => MaxRestrainPower, 
		}, (float)skillLevelWithEffects / 5f) * ChokeFakePressesPerSecondWhenHeld;
		float num2 = 0f;
		float num3 = Mathf.Lerp(t: (!victim.Zombie) ? ((float)victim.GetSkillLevelWithEffects(SkillType.Strength) / 5f) : ((float)victim.Infection / 5f), a: (holdType == HoldType.Restrain) ? RestrainedStrugglePowerMin : ChokeStrugglePowerMin, b: (holdType == HoldType.Restrain) ? RestrainedStrugglePowerMax : ChokeStrugglePowerMax);
		if (num3 >= num)
		{
			return false;
		}
		float num4 = 0.5f / (num - num3);
		float num5 = ((holdType == HoldType.Restrain) ? RestraintFatiguePenalty : ChokeFatiguePenalty) * num4 * ChokeFakePressesPerSecondWhenHeld * GetFatiguePenaltySkillFactor();
		float num6 = 1f - GetFatigueMinusAdrenaline();
		return num5 <= num6;
	}

	private bool ApplyChokeAction(PlayerRecord playerRecord, bool held, int heldFrames)
	{
		if (!(InteractionObject is Character character))
		{
			return false;
		}
		if (DirectControlled && this == Hud.Instance.LocalControlledCharacter && character.Zombie)
		{
			HintManager.Instance.Hints[33].MarkPerformed();
		}
		float num = (held ? ((float)heldFrames * (1f / 60f) * ChokeFakePressesPerSecondWhenHeld) : 1f);
		float num2 = ((character.CurrentActionAnim == ActionAnim.RestrainedStruggle) ? RestraintFatiguePenalty : ChokeFatiguePenalty) * num;
		float num3 = num2 * GetFatiguePenaltySkillFactor();
		float num4 = 1f - GetFatigueMinusAdrenaline();
		float num5 = ((num3 == 0f) ? 1f : Math.Min(1f, num4 / num3));
		num2 *= num5;
		if (num2 > 0f && (character.CurrentActionAnim == ActionAnim.ChokeHoldStruggle || character.CurrentActionAnim == ActionAnim.RestrainedStruggle))
		{
			HoldType holdTypeFromAnim = GetHoldTypeFromAnim();
			int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.HandToHand);
			float num6 = Mathf.Lerp(holdTypeFromAnim switch
			{
				HoldType.SlitThroat => MinAssassinatePower, 
				HoldType.ChokeHold => MinChokePower, 
				_ => MinRestrainPower, 
			}, holdTypeFromAnim switch
			{
				HoldType.SlitThroat => MaxAssassinatePower, 
				HoldType.ChokeHold => MaxChokePower, 
				_ => MaxRestrainPower, 
			}, (float)skillLevelWithEffects / 5f) * num5;
			ChokePower += num6 * num;
			LastChokeTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			ApplyFatiguePenalty(num2);
			if (CurrentActionAnim != ActionAnim.RestrainLoop && CheckFrontmostPrediction(PredictedEventType.ChokingSound) && !IsVoiceSoundPlaying(VoiceSoundType.Choking))
			{
				PlayVoiceSoundFromList(SoundManager.ChokeSounds, VoiceSoundType.Choking);
			}
			if (ChokePower >= 1f)
			{
				if (IsPredicted() == character.IsPredicted())
				{
					MoveToAndChokeHold moveToAndChokeHold = FindActiveGoal(GoalType.MoveToAndChokeHold) as MoveToAndChokeHold;
					bool flag = moveToAndChokeHold?.Assassinate ?? false;
					SecrecyMode secrecy = moveToAndChokeHold?.Secrecy ?? SecrecyMode.IgnoredByObjectCommunity;
					if (CurrentActionAnim == ActionAnim.SlitThroatLoop || flag)
					{
						Bone bone = Bone.Neck;
						Vector3 nonDeterministicAttackPos = (IsUnityObjectActive() ? GetUnityBoneTransform(bone).Translation() : GetBoundingBoxCentre());
						InfectionType infectionType = ((EquippedItem != null) ? EquippedItem.InfectedWith : InfectionType.None);
						bool absorbedByArmor = false;
						character.OnMeleeAttacked2(this, character, InjuryType.SharpObject, InjuryLocation.Head, infectionType, nonDeterministicAttackPos, base.Forward, bone, Vector3.zero, 1f, out absorbedByArmor, dontReact: false, assassinate: true, secrecy, stealthy: true, TargettableBodyLocation.Head, small: true);
						if (character.Alive)
						{
							DirectControlledCrouching = false;
							character.OnChokeHoldCancelled(this);
						}
						if (CheckFrontmostPrediction(PredictedEventType.SlitThroatSound))
						{
							PlaySoundOneShotFromList(SoundManager.SkinningSounds);
						}
						Skillset.AddProgress(this, SkillType.Stealth, 10f);
					}
					else if (CurrentActionAnim == ActionAnim.ChokeHoldLoop)
					{
						Bone bone2 = Bone.Spine1;
						Vector3 hitPos = (IsUnityObjectActive() ? GetUnityBoneTransform(bone2).Translation() : GetBoundingBoxCentre());
						character.Ragdollify(ChokeHoldHitRadius, hitPos, ChokeHoldHitForce * base.Forward, bone2, Vector3.zero, fromStumble: false, retainVelocity: true);
						character.SedativeEffect += 30f;
						character.LastPersonChokedMe = this;
						character.SetConsciousness(Consciousness.Unconscious);
						Skillset.AddProgress(this, SkillType.Stealth, 10f);
					}
					else if (CurrentActionAnim == ActionAnim.RestrainLoop)
					{
						if (IsAuthoritative())
						{
							Character sparringPartner = character.SparringPartner;
							if (sparringPartner != null)
							{
								if (Speaking != null && Speaking.Situation == SpeechSituation.Restrain)
								{
									OnSpeechFinished(interrupted: true);
								}
								Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, sparringPartner, this, SpeechSituation.FeudingGiveUp);
								if (speechForSituation != null)
								{
									character.Speak(speechForSituation, character.SparringPartner, this, default(MemoryParam));
								}
								character.SetSparringPartner(SparringType.None, null, null);
								sparringPartner.SetSparringPartner(SparringType.None, null, null);
								Memory.OnMemorableEvent(MemoryPrototype.RestrainedSuccessfully, this, character, sparringPartner, 1f, SecrecyMode.Public, null);
								character.SetRecentActivity(RecentActivityType.Feuding, sparringPartner);
								sparringPartner.SetRecentActivity(RecentActivityType.Feuding, character);
							}
						}
						character.StopActionAnim(ActionAnim.RestrainedStruggle);
					}
				}
				Goal.OnChokeSucceeded(this, null, character);
				if (IsAuthoritative() && holdTypeFromAnim == HoldType.ChokeHold && !character.Zombie && this is Human)
				{
					if (character.IsAssailantUnknown(this))
					{
						Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, null, character, 1f, SecrecyMode.OnlyKnownToObject);
					}
					else
					{
						Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, this, character, 1f, SecrecyMode.OnlyKnownToObject);
					}
					Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, this, character, 1f, SecrecyMode.IgnoredByObjectCommunity);
				}
				if (playerRecord != null && playerRecord.IsLocal && holdTypeFromAnim != HoldType.Restrain && CheckFrontmostPrediction(PredictedEventType.Choke))
				{
					Hud.Instance.OnParrySucceeded(character, (holdTypeFromAnim == HoldType.ChokeHold) ? HUD_Knockout : HUD_Assassinated, 1f);
				}
			}
			if (IsAuthoritative() && holdTypeFromAnim != HoldType.Restrain)
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Choke, character.Pos, ChokeSoundRadius, GetMaxSoundVisibilityRange(), this, this, character, character));
			}
			return true;
		}
		return false;
	}

	public bool WouldChokeBeDetectable(Character victim, List<Character> ignoreList = null)
	{
		if (CalcCamouflageAmount() == 0f)
		{
			return true;
		}
		AISound sound = new AISound(AISoundType.Choke, victim.Pos, ChokeSoundRadius, GetMaxSoundVisibilityRange(), this, this, victim, victim);
		foreach (Character item in victim.CharactersTargetingMe)
		{
			if (item.IsAwake && item != this && (ignoreList == null || !ignoreList.Contains(item)) && (item.Community == victim.Community || IsEnemy(item)))
			{
				item.CheckIfSoundCanBeSeenOrHeard(sound, out var seen, out var heard);
				if (seen || heard)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnProcessDropAction()
	{
		if (CanReceiveUserInputs())
		{
			CancelSurrendering();
			if (CurrentActionAnim != ActionAnim.Drop && TryStartActionAnim(ActionAnim.Drop, null))
			{
				DirectControlled = true;
				MarkPlayerControlled();
				CancelOneOffCrafting();
			}
		}
	}

	public void OnProcessReloadAction()
	{
		if (CanReceiveUserInputs())
		{
			CancelSurrendering();
			float animSpeed = ((EquippedItem is AmmoWeapon ammoWeapon) ? ammoWeapon.GetReloadSpeed(this) : 1f);
			if (CurrentActionAnim != ActionAnim.Reload && TryStartActionAnim(ActionAnim.Reload, null, animSpeed))
			{
				DirectControlled = true;
				MarkPlayerControlled();
				CancelOneOffCrafting();
			}
		}
	}

	public void OnProcessCrouchAction()
	{
		if (CanReceiveUserInputs())
		{
			CancelSurrendering();
			DirectControlled = true;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			DirectControlledCrouching = !DirectControlledCrouching;
			ClearSitting();
			DisableSprint();
		}
	}

	public void OnProcessSetDesiredWeaponAction(PlayerRecord playerRecord, Equipment weapon, EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		if (!CanReceiveUserInputs())
		{
			return;
		}
		CancelSurrendering();
		if (weapon != null && !Inventory.Contains(weapon))
		{
			return;
		}
		if (ammoType != null && weapon != null && (ammoType != weapon.GetCurrentAmmoType() || infectedWith != weapon.InfectedWith) && weapon is AmmoWeapon ammoWeapon)
		{
			if (IsAuthoritative())
			{
				ammoWeapon.Unload(this);
				ammoWeapon.CurrentAmmoType = ammoType;
				ammoWeapon.InfectedWith = infectedWith;
			}
			if (weapon == EquippedItem && GetCurrentActionPriority() < GetActionPriority(ActionAnim.Reload))
			{
				OnProcessReloadAction();
			}
			else if (IsAuthoritative())
			{
				Equipment equipment = Inventory.TakeAmmoOfType(this, ammoType, infectedWith, ammoWeapon.GetMaxAmmo());
				if (equipment != null)
				{
					ammoWeapon.OnReload(this, equipment.GetAmount(), ammoType, equipment.InfectedWith);
					equipment.Delete();
				}
			}
		}
		if (playerRecord.IsLocal && weapon is Weapon)
		{
			HintManager.Instance.Hints[22].MarkPerformed();
		}
		DesiredEquippedItem = weapon;
		SetDirectControlled(value: true, wantSetLastInputTime: true, wantClearLeaderCommand: false);
		MarkPlayerControlled();
		CancelOneOffCrafting();
	}

	public bool CanReceiveUserInputs()
	{
		bool constructing;
		AIOverridesControlReason reason;
		return CanReceiveUserInputs(out constructing, out reason);
	}

	public bool CanReceiveUserInputs(out bool constructing)
	{
		AIOverridesControlReason reason;
		return CanReceiveUserInputs(out constructing, out reason);
	}

	public bool CanReceiveUserInputs(out bool constructing, out AIOverridesControlReason reason)
	{
		constructing = false;
		reason = AIOverridesControlReason.None;
		if (Consciousness >= Consciousness.Unconscious)
		{
			return false;
		}
		if (Zombie)
		{
			return false;
		}
		if (GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (CurrentAnimState != AnimState.Animation)
		{
			return false;
		}
		reason = AIOverridesControl();
		if (reason != AIOverridesControlReason.None)
		{
			return false;
		}
		if (DisableInputsUntilNextThink)
		{
			return false;
		}
		if (IsInsideBuildingUnderConstruction())
		{
			constructing = true;
			return false;
		}
		return true;
	}

	public bool HasMolotovCocktail()
	{
		return Inventory.FindItemOfClass(typeof(MolotovCocktail)) != null;
	}

	public float GetMolotovCocktailRange(Character character, TileObject targetObj)
	{
		if (!(Inventory.FindItemOfClass(typeof(MolotovCocktail)) is MolotovCocktail molotovCocktail))
		{
			return 0f;
		}
		return molotovCocktail.GetRangeIncludingEffects(character, targetObj);
	}

	public bool HasExplosives()
	{
		if (Inventory.FindItemOfClass(typeof(RPG)) is RPG rPG)
		{
			if (!HasInfiniteAmmo(rPG) && rPG.CurrentAmmo <= 0)
			{
				return Inventory.HasAmmoForWeapon(rPG);
			}
			return true;
		}
		return Inventory.FindItemOfClass(typeof(PipeBomb)) != null;
	}

	public bool HasGun()
	{
		foreach (Equipment content in Inventory.Contents)
		{
			if (content is Gun gun)
			{
				return HasInfiniteAmmo(gun) || gun.CurrentAmmo > 0 || Inventory.HasAmmoForWeapon(gun);
			}
		}
		return false;
	}

	public float GetExplosivesRange(Character character, TileObject targetObj)
	{
		float num = 0f;
		if (Inventory.FindItemOfClass(typeof(RPG)) is RPG rPG && (HasInfiniteAmmo(rPG) || rPG.CurrentAmmo > 0 || Inventory.HasAmmoForWeapon(rPG)))
		{
			num = Math.Max(num, rPG.GetRangeIncludingEffects(character, targetObj));
		}
		if (Inventory.FindItemOfClass(typeof(PipeBomb)) is PipeBomb pipeBomb)
		{
			num = Math.Max(num, pipeBomb.GetRangeIncludingEffects(character, targetObj));
		}
		return num;
	}

	public bool HasEquippedRangedWeaponAndAmmo(bool previouslyDid)
	{
		if (Zombie)
		{
			return false;
		}
		if (EquippedItem is AmmoWeapon ammoWeapon)
		{
			if (!HasInfiniteAmmo(ammoWeapon) && ammoWeapon.CurrentAmmo <= 0)
			{
				return Inventory.HasAmmoForWeapon(ammoWeapon);
			}
			return true;
		}
		if (EquippedItem is MolotovCocktail || EquippedItem is PipeBomb)
		{
			return true;
		}
		if (previouslyDid && Community != Session.Instance.CommunityManager.PlayerCommunity && IsAwake)
		{
			return true;
		}
		return false;
	}

	public bool HasInfiniteAmmo(Equipment equipment)
	{
		if (equipment != null && equipment.GetPrototype() == EquipmentPrototype.Snowball)
		{
			return false;
		}
		return !IsControllableByPlayer();
	}

	public bool HasInfiniteBandages(Equipment equipment, Character targetCharacter)
	{
		if (Community != null && Community.IsAISettlement())
		{
			return equipment.GetBandageLevel() > 0;
		}
		return false;
	}

	public bool RefuseToAttackTarget(TileObject targetObj)
	{
		if (Rank == Rank.Leader)
		{
			return false;
		}
		if (SparringPartner == targetObj)
		{
			return false;
		}
		if (!(targetObj is Human { AliveAndNotZombie: not false } human))
		{
			return false;
		}
		if (IsEnemy(targetObj))
		{
			return false;
		}
		CalcApprovalRating(human, out var approval, out var _);
		float num = 40f;
		num += (float)GetPersonality(CachedPersonalityType.Fickle, CachedPersonalityType.Loyal) * 30f;
		num += (float)GetPersonality(CachedPersonalityType.Detached, CachedPersonalityType.Compassionate) * 30f;
		if (HasPersonality(CachedPersonalityType.Sociopath))
		{
			num += 30f;
		}
		if (HasPersonality(CachedPersonalityType.Moral))
		{
			num -= 30f;
		}
		return approval >= num;
	}

	public void HandleInput(PlayerRecord localPlayerRecord, InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (!CanReceiveUserInputs(out var constructing, out var reason))
		{
			if (constructing && Goal != null)
			{
				if (Goal.GetMovementType() != MovementType.Run)
				{
					if (instance.IsJustPressed(InputFunction.Sprint, capture: true, ButtonPromptBarBehaviour.PROMPT_Sprint))
					{
						inputFrame.AddAction(new InputAction(InputActionType.EnableSprint));
					}
				}
				else if (instance.IsJustPressed(InputFunction.Sprint))
				{
					inputFrame.AddAction(new InputAction(InputActionType.DisableSprint));
				}
			}
			else
			{
				if (reason != AIOverridesControlReason.Vaulting)
				{
					return;
				}
				Vector2 vector = instance.GetVector2(InputFunction.MoveHoriz, InputFunction.MoveVert);
				if (vector.sqrMagnitude > 0f && IsOutdoors())
				{
					if (vector.sqrMagnitude > 1f)
					{
						vector.Normalize();
					}
					Vector2 vector2 = MathUtil.SafeNormalize(MathUtil.ToXZ(Session.Instance.GameCamera.GetForward()), Vector2.zero);
					Vector2 moveDir = MathUtil.SafeNormalize(MathUtil.ToXZ(Session.Instance.GameCamera.GetRight()), Vector2.zero) * vector.x + vector2 * vector.y;
					inputFrame.AddAction(InputAction.Move(moveDir));
				}
			}
			return;
		}
		Hud instance2 = Hud.Instance;
		Character predictedOrElseThisCharacter = GetPredictedOrElseThisCharacter();
		if (predictedOrElseThisCharacter.IsBeingBitten())
		{
			if (predictedOrElseThisCharacter.GetCurrentActionPriority() != ActionPriority.Struggle || !(predictedOrElseThisCharacter.InteractionObject is Character))
			{
				return;
			}
			bool flag = instance.IsJustPressed(InputFunction.Parry, capture: false) || instance.IsJustPressed(InputFunction.Attack, capture: false);
			bool flag2 = instance.IsPressed(InputFunction.Parry, capture: false) || instance.IsPressed(InputFunction.Attack, capture: false);
			Character character = (Character)predictedOrElseThisCharacter.InteractionObject.GetAuthoritativeOrElseThis();
			instance2.ShowParryPrompt(character, predictedOrElseThisCharacter.CanEscapeZombieBite(DirectControlledZombieEscapePower * ((flag2 && !flag) ? (1f / 60f * EscapeFakePressesPerSecondWhenHeld) : 1f)), predictedOrElseThisCharacter.ZombieEscapePower / predictedOrElseThisCharacter.GetStruggleFreePenalty(character.Infection));
			if (instance2.ParryPromptEnabled)
			{
				if (flag)
				{
					inputFrame.AddAction(new InputAction(InputActionType.Escape));
				}
				else if (flag2)
				{
					inputFrame.AddAction(InputAction.EscapeHeld(1));
				}
				instance.Capture(InputFunction.Parry, untilReleased: false);
				instance.Capture(InputFunction.Attack, untilReleased: false);
			}
			return;
		}
		if ((predictedOrElseThisCharacter.CurrentActionAnim == ActionAnim.ChokeHoldLoop || predictedOrElseThisCharacter.CurrentActionAnim == ActionAnim.SlitThroatLoop || predictedOrElseThisCharacter.CurrentActionAnim == ActionAnim.RestrainLoop) && predictedOrElseThisCharacter.InteractionObject is Character && FindActiveGoal(GoalType.InvisibleStrainSpreadGoal) == null)
		{
			bool flag3 = false;
			if (instance.IsJustPressed(InputFunction.Parry, capture: false) || instance.IsJustPressed(InputFunction.Attack, capture: false))
			{
				inputFrame.AddAction(new InputAction(InputActionType.Choke));
			}
			else if (instance.IsPressed(InputFunction.Parry, capture: false) || instance.IsPressed(InputFunction.Attack, capture: false))
			{
				inputFrame.AddAction(InputAction.ChokeHeld(1));
				flag3 = true;
			}
			instance.Capture(InputFunction.Parry, untilReleased: false);
			instance.Capture(InputFunction.Attack, untilReleased: false);
			Character victim = (Character)predictedOrElseThisCharacter.InteractionObject.GetAuthoritativeOrElseThis();
			float num = ((predictedOrElseThisCharacter.CurrentActionAnim == ActionAnim.RestrainLoop) ? RestraintFatiguePenalty : ChokeFatiguePenalty);
			instance2.ShowChokePrompt(victim, !predictedOrElseThisCharacter.IsTooTiredFor(num * (flag3 ? (1f / 60f * ChokeFakePressesPerSecondWhenHeld * 8f) : 1f)), predictedOrElseThisCharacter.ChokePower);
			return;
		}
		if (IsPushingAgainstWaistHighWall() && instance.IsJustPressed(InputFunction.MainAction, capture: true, ButtonPromptBarBehaviour.PROMPT_Vault))
		{
			inputFrame.AddAction(InputAction.Vault(PushingAgainstWaistHighWallObj, isDoubleClick: true));
		}
		Vector2 vector3 = instance.GetVector2(InputFunction.MoveHoriz, InputFunction.MoveVert);
		AmmoWeapon ammoWeapon = EquippedItem as AmmoWeapon;
		bool flag4 = false;
		if (localPlayerRecord.WantLockOnTarget)
		{
			bool flag5 = EquippedItem is HuntingKnife;
			if ((EquippedItem == null || flag5) && GameCursor.CanChokeHold(this, instance2.LocalTargetObject))
			{
				sb.Length = 0;
				sb.Append(GameImpl.Translate(AvailableAction.Caption[flag5 ? 75 : 74]));
				sb.Append(' ');
				sb.Append('(');
				sb.Append(GameImpl.Translate(WouldChokeBeDetectable(instance2.LocalTargetObject as Character) ? HudBehaviour.HUD_Detectable : HudBehaviour.HUD_Hidden));
				sb.Append(')');
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt(InputFunction.Attack, sb.ToString());
				if (!flag5)
				{
					HintManager.Instance.ShowNeedKnifeToAssassinateHint();
				}
				if (instance.IsJustPressed(InputFunction.Attack))
				{
					if (RefuseToAttackTarget(localPlayerRecord.TargetObject))
					{
						HudBehaviour.Instance.ShowRefuseToAttackMsg(this, localPlayerRecord.TargetObject);
					}
					else if (flag5)
					{
						HintManager.Instance.Hints[16].MarkPerformed();
						inputFrame.AddAction(InputAction.SlitThroat(instance2.LocalTargetObject as Character, isDoubleClick: true));
					}
					else
					{
						inputFrame.AddAction(InputAction.ChokeHold(instance2.LocalTargetObject as Character, isDoubleClick: true));
					}
				}
			}
			else if (RefuseToAttackTarget(localPlayerRecord.TargetObject))
			{
				if (instance.IsJustPressed(InputFunction.Attack))
				{
					HudBehaviour.Instance.ShowRefuseToAttackMsg(this, localPlayerRecord.TargetObject);
				}
			}
			else
			{
				flag4 = instance.IsPressed(InputFunction.Attack);
			}
		}
		Vector2 vector4 = Vector2.zero;
		if (vector3.sqrMagnitude > 0f && IsOutdoors())
		{
			if (vector3.sqrMagnitude > 1f)
			{
				vector3.Normalize();
			}
			Vector2 vector5 = MathUtil.SafeNormalize(MathUtil.ToXZ(Session.Instance.GameCamera.GetForward()), Vector2.zero);
			vector4 = MathUtil.SafeNormalize(MathUtil.ToXZ(Session.Instance.GameCamera.GetRight()), Vector2.zero) * vector3.x + vector5 * vector3.y;
			inputFrame.AddAction(InputAction.Move(vector4));
		}
		if (InsideBuilding is EnterableVehicle enterableVehicle && enterableVehicle.GetDriverAuthoritative() == GetAuthoritativeOrElseThisCharacter() && (enterableVehicle.IsDriveable || enterableVehicle.MossCleanedAway))
		{
			Vector2 moveDir2 = new Vector2(vector3.x, Mathf.Clamp(vector3.y + instance.GetAxis(InputFunction.VehicleAccelerate) - instance.GetAxis(InputFunction.VehicleBrake), -1f, 1f));
			if (moveDir2.sqrMagnitude > 0f)
			{
				inputFrame.AddAction(InputAction.Move(moveDir2));
			}
			if (enterableVehicle.IsMoving && instance.IsPressed(InputFunction.VehicleHandBrake, capture: true, ButtonPromptBarBehaviour.PROMPT_HandBrake))
			{
				inputFrame.AddAction(new InputAction(InputActionType.HandBrake));
			}
			if (instance.IsJustPressed(InputFunction.VehicleHorn, capture: true, ButtonPromptBarBehaviour.PROMPT_Horn))
			{
				inputFrame.AddAction(new InputAction(InputActionType.Horn));
			}
		}
		int num2;
		if (!DirectControlled)
		{
			if (Goal == null)
			{
				num2 = 0;
				goto IL_06b3;
			}
			num2 = ((Goal.GetMovementType() == MovementType.Run) ? 1 : 0);
		}
		else
		{
			num2 = (IsSprintEnabled() ? 1 : 0);
		}
		if (num2 == 0)
		{
			goto IL_06b3;
		}
		goto IL_0744;
		IL_06b3:
		if (GetFatigueMinusAdrenaline() < 1f && !instance2.LocalWantLockOnTarget)
		{
			int buttonPromptHash = 0;
			if (MovementSpeed >= JogSpeed - 0.1f && !IsPushingAgainstWaistHighWall())
			{
				buttonPromptHash = ButtonPromptBarBehaviour.PROMPT_Sprint;
			}
			else if (!DirectControlled && Goal != null && MovementType == MovementType.Walk)
			{
				buttonPromptHash = ButtonPromptBarBehaviour.PROMPT_Sprint;
			}
			if (instance.IsJustPressed(InputFunction.Sprint, capture: true, buttonPromptHash))
			{
				inputFrame.AddAction(new InputAction(InputActionType.EnableSprint));
			}
			else if (GameCursor.CachedIsAnyoneOnScreenInCombat)
			{
				inputFrame.AddAction(new InputAction(InputActionType.EnableSprintAutomatic));
			}
		}
		goto IL_0744;
		IL_0744:
		if (num2 != 0 && !DirectControlled && instance.IsJustPressed(InputFunction.Sprint))
		{
			inputFrame.AddAction(new InputAction(InputActionType.DisableSprint));
		}
		if (flag4 && (EquippedItem == null || EquippedItem is Weapon) && InTerrain && EquippedItem == DesiredEquippedItem)
		{
			if (ammoWeapon != null && ammoWeapon.CurrentAmmo == 0)
			{
				if (ammoWeapon.CanBeReloaded(this, checkIfAllowedToUseAmmo: false))
				{
					inputFrame.AddAction(new InputAction(InputActionType.Reload));
				}
				else
				{
					Equipment bestWeaponForIdle = Inventory.GetBestWeaponForIdle(this, rangedOnly: false, allowMolotovs: true);
					if (bestWeaponForIdle != null)
					{
						inputFrame.AddAction(InputAction.SetDesiredWeapon(bestWeaponForIdle, bestWeaponForIdle.GetCurrentAmmoType(), bestWeaponForIdle.InfectedWith));
					}
					else
					{
						if (ammoWeapon is Gun)
						{
							SoundManager.PlayMenuSound(SoundManager.GunDryFireSound);
						}
						instance.Capture(InputFunction.Attack, untilReleased: true);
					}
				}
			}
			else
			{
				inputFrame.AddAction(new InputAction(InputActionType.Fire));
			}
		}
		CursorAction cursorAction = instance2.Cursor.GetCursorAction();
		if (cursorAction != CursorAction.TalkTo && cursorAction != CursorAction.OpenGiftMenu && cursorAction != CursorAction.Back && cursorAction != CursorAction.FollowMe && cursorAction != CursorAction.StopFollowingMe)
		{
			bool flag6 = instance.IsJustPressed(InputFunction.Parry, capture: false);
			bool flag7 = instance.IsPressed(InputFunction.Parry);
			ActionAnim resultAction = ActionAnim.None;
			bool resultEnabled;
			Character character2 = predictedOrElseThisCharacter.FindBestAttackerForDirectControlledParry(out resultAction, out resultEnabled);
			if (character2 != null)
			{
				character2 = character2.GetAuthoritativeOrElseThisCharacter();
				instance2.ShowParryPrompt(character2, resultEnabled, -1f);
			}
			else if (instance2.Cursor.CanSkipConversationWith == null)
			{
				if (EquippedItem is MeleeWeapon)
				{
					switch (GetCurrentTargetBodyLocation())
					{
					case TargettableBodyLocation.Head:
						resultAction = ActionAnim.Parry_High;
						break;
					case TargettableBodyLocation.Torso:
						resultAction = ActionAnim.Parry_Middle;
						break;
					case TargettableBodyLocation.Legs:
						resultAction = ActionAnim.Parry_Low;
						break;
					}
				}
				else if (EquippedItem == null)
				{
					resultAction = ActionAnim.Block_Punch;
				}
				resultEnabled = true;
			}
			if (flag7 && vector3.sqrMagnitude > 0f)
			{
				if (instance2.LocalTargetObject == null)
				{
					resultAction = ((character2 == null) ? ActionAnim.Dodge_Forwards : ((Vector2.Dot(MathUtil.SafeNormalize(vector4, Vector2.zero), MathUtil.SafeNormalize(character2.GetPredictedOrElseThisCharacter().PosXZ - PosXZ, Vector2.zero)) >= 0.707f && !character2.GetPredictedOrElseThisCharacter().IsZombieJumping()) ? ActionAnim.Kick : ActionAnim.Dodge_Forwards));
				}
				else if (!(Math.Abs(vector3.y) > Math.Abs(vector3.x)))
				{
					resultAction = ((!(vector3.x > 0f)) ? ActionAnim.Dodge_Left : ActionAnim.Dodge_Right);
				}
				else if (vector3.y > 0f)
				{
					character2 = instance2.LocalTargetObject as Character;
					resultAction = ((character2 != null && character2.GetPredictedOrElseThisCharacter().IsZombieJumping()) ? ActionAnim.Dodge_Forwards : ActionAnim.Kick);
				}
				else
				{
					resultAction = ActionAnim.Dodge_Backwards;
				}
				resultEnabled = true;
			}
			if (flag7 && resultAction != ActionAnim.None)
			{
				if ((resultAction == ActionAnim.Dodge_Backwards || resultAction == ActionAnim.Dodge_Forwards || resultAction == ActionAnim.Dodge_Left || resultAction == ActionAnim.Dodge_Right) && IsTooTiredFor(DodgingFatiguePenalty))
				{
					if (flag6)
					{
						switch (Appearance.Gender)
						{
						case GenderType.Male:
							SoundManager.PlayMenuSound(SoundManager.MaleCatchingBreathSound);
							break;
						case GenderType.Female:
							SoundManager.PlayMenuSound(SoundManager.FemaleCatchingBreathSound);
							break;
						}
					}
				}
				else if (resultEnabled)
				{
					inputFrame.AddAction(InputAction.Parry(character2, resultAction));
				}
				else if (flag6)
				{
					SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
				}
			}
		}
		if (instance.IsJustPressed(InputFunction.Crouch))
		{
			inputFrame.AddAction(new InputAction(InputActionType.Crouch));
			HintManager.Instance.Hints[6].MarkPerformed();
		}
		if (ammoWeapon != null)
		{
			if (ammoWeapon.CanBeReloaded(this, checkIfAllowedToUseAmmo: false))
			{
				if (instance.IsPressed(InputFunction.Reload, capture: true, ButtonPromptBarBehaviour.PROMPT_Reload))
				{
					inputFrame.AddAction(new InputAction(InputActionType.Reload));
				}
			}
			else if (ammoWeapon.GetMaxAmmo() > 1)
			{
				instance.Capture(InputFunction.Reload, untilReleased: false);
			}
		}
		if (!localPlayerRecord.WantLockOnTarget)
		{
			return;
		}
		Character character3 = localPlayerRecord.TargetObject as Human;
		if (character3 == null || !character3.AwakeAndNotZombie || CurrentActionAnim == ActionAnim.HandsUp)
		{
			return;
		}
		Community community = localPlayerRecord.TargetObject.GetCommunity();
		if (community != null && community.CanPlayerSurrenderToMe(this, character3))
		{
			HintManager.Instance.ShowSurrenderHint(character3);
			if (instance.IsPressed(InputFunction.Surrender, capture: true, ButtonPromptBarBehaviour.PROMPT_Surrender))
			{
				HintManager.Instance.MarkSurrenderPerformed();
				inputFrame.AddAction(new InputAction(InputActionType.Surrender));
			}
		}
	}

	public void HandleInputEvenWhilePaused(PlayerRecord localPlayerRecord, InputFrame inputFrame)
	{
		if (CanReceiveUserInputs() && !IsUsingEquippedItem() && !IsCraftingAnim())
		{
			InputFunctionManager instance = InputFunctionManager.Instance;
			Hud instance2 = Hud.Instance;
			float axis = instance.GetAxis(InputFunction.SelectAction);
			if (axis != 0f)
			{
				instance.Capture(InputFunction.SelectAction, untilReleased: true);
			}
			if (axis < 0f)
			{
				EquipmentPrototype ammoType;
				InfectionType infectedWith;
				Equipment nextEquippable = Inventory.GetNextEquippable(this, DesiredEquippedItem, 1, instance2.LocalPressingAim, out ammoType, out infectedWith);
				inputFrame.AddAction(InputAction.SetDesiredWeapon(nextEquippable, ammoType, infectedWith));
				Hud.Instance.Cursor.ShowEquipmentSelect();
			}
			if (axis > 0f)
			{
				EquipmentPrototype ammoType2;
				InfectionType infectedWith2;
				Equipment nextEquippable2 = Inventory.GetNextEquippable(this, DesiredEquippedItem, -1, instance2.LocalPressingAim, out ammoType2, out infectedWith2);
				inputFrame.AddAction(InputAction.SetDesiredWeapon(nextEquippable2, ammoType2, infectedWith2));
				Hud.Instance.Cursor.ShowEquipmentSelect();
			}
		}
	}

	public void CommandMoveTo(PlayerRecord playerRecord, TerrainCoord dest, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = survivorGoal.GetLeaderCommand() as MoveAsCloseAsPossibleTo;
			if (moveAsCloseAsPossibleTo == null || moveAsCloseAsPossibleTo.DestTile != dest)
			{
				moveAsCloseAsPossibleTo = new MoveAsCloseAsPossibleTo(MovementType.Walk, dest);
			}
			moveAsCloseAsPossibleTo.SetCrouching(this, null, DirectControlledCrouching);
			moveAsCloseAsPossibleTo.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveAsCloseAsPossibleTo, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandSpeakTo(PlayerRecord playerRecord, Character target, BaseObject speechObject, MemoryParam speechParam, Speech speech, Speech replyTo, BaseObject replyToReferringTo, MemoryParam replyToParam, bool isDoubleClick)
	{
		if (target == this)
		{
			return;
		}
		if (speech.Situation == SpeechSituation.Swap)
		{
			TakeUIGoal.StartSwappingSupplies(playerRecord, this, target, SwappingSuppliesMode.Swapping);
		}
		else if (Goal is SurvivorGoal survivorGoal)
		{
			Conversation conversation = survivorGoal.GetLeaderCommand() as Conversation;
			if (conversation == null || conversation.GetTargetObject() != target)
			{
				conversation = new Conversation(this, target, speechObject, speechParam, speech, controlledByPlayer: true, replyTo, replyToReferringTo, replyToParam);
			}
			else
			{
				conversation.Speak(this, speechObject, default(MemoryParam), speech, replyTo, replyToReferringTo);
			}
			conversation.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, conversation, target, ObeyLeaderGoal.SourceType.SpeakTo, playerRecord);
		}
	}

	public void CommandGiveGift(PlayerRecord playerRecord, Character target, Equipment gift, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			if (target.FindActiveGoal(GoalType.Conversation) is Conversation conversation)
			{
				conversation.Finished = true;
			}
			MoveToAndInteractGoal moveToAndInteractGoal = survivorGoal.GetLeaderCommand() as MoveToAndInteractGoal;
			if (moveToAndInteractGoal == null || moveToAndInteractGoal.GetTargetObject() != target || moveToAndInteractGoal.GetInteractionType() != InteractionType.GiveGift || moveToAndInteractGoal.Item != gift)
			{
				moveToAndInteractGoal = new MoveToAndInteractGoal(this, target, InteractionType.GiveGift, gift, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			moveToAndInteractGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndInteractGoal, target, ObeyLeaderGoal.SourceType.SpeakTo, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandInteract(PlayerRecord playerRecord, TileObject target, InteractionType interactionType, Equipment item, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndInteractGoal moveToAndInteractGoal = survivorGoal.GetLeaderCommand() as MoveToAndInteractGoal;
			if (moveToAndInteractGoal == null || moveToAndInteractGoal.GetTargetObject() != target || moveToAndInteractGoal.GetInteractionType() != interactionType || moveToAndInteractGoal.Item != item)
			{
				moveToAndInteractGoal = new MoveToAndInteractGoal(this, target, interactionType, item, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			moveToAndInteractGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndInteractGoal, target, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandBury(PlayerRecord playerRecord, Character body, Prop grave, bool isDoubleClick)
	{
		if (body != null && CarryingObject == body && Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndInteractGoal moveToAndInteractGoal = survivorGoal.GetLeaderCommand() as MoveToAndInteractGoal;
			if (moveToAndInteractGoal == null || moveToAndInteractGoal.GetTargetObject() != grave || moveToAndInteractGoal.GetInteractionType() != InteractionType.Bury)
			{
				moveToAndInteractGoal = new MoveToAndInteractGoal(this, grave, InteractionType.Bury, body);
			}
			moveToAndInteractGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndInteractGoal, grave, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandChokeHold(PlayerRecord playerRecord, Character target, bool isDoubleClick, HoldType holdType)
	{
		if (!(Goal is SurvivorGoal survivorGoal) || target == this)
		{
			return;
		}
		MoveToAndChokeHold moveToAndChokeHold = survivorGoal.GetLeaderCommand() as MoveToAndChokeHold;
		if (moveToAndChokeHold == null || moveToAndChokeHold.GetTargetObject() != target || moveToAndChokeHold.HoldType != holdType)
		{
			moveToAndChokeHold = new MoveToAndChokeHold(holdType);
		}
		moveToAndChokeHold.SetMovementType(this, MovementType.Run);
		survivorGoal.SetLeaderCommand(this, null, moveToAndChokeHold, target, ObeyLeaderGoal.SourceType.Player, playerRecord);
		DirectControlled = false;
		DisableInputsUntilNextThink = true;
		if (holdType == HoldType.Restrain || Followers == null || Followers.Count <= 0)
		{
			return;
		}
		TargetedCharacters.Clear();
		TargetedCharacters.Add(target);
		foreach (Target target2 in Targets)
		{
			if (target2.Object is Character character && character != target && character.IsConscious && character.Tile.GetDist(target.Tile) <= 16f && (character.Zombie == target.Zombie || character.Community == target.Community))
			{
				Character nearestFollowerWhoCanAssassinateTarget = GetNearestFollowerWhoCanAssassinateTarget(character, holdType, TargetedCharacters);
				if (nearestFollowerWhoCanAssassinateTarget != null)
				{
					nearestFollowerWhoCanAssassinateTarget.SetFollowCommand((holdType == HoldType.SlitThroat) ? FollowCommand.Assassinate : FollowCommand.ChokdHold, TerrainCoord.Invalid, null, null, 0f, character);
					TargetedCharacters.Add(target);
				}
			}
		}
		TargetedCharacters.Clear();
	}

	public void CommandAttackUnconscious(PlayerRecord playerRecord, Character target, bool isDoubleClick, bool kill)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			InteractionType interactionType = (kill ? InteractionType.KillUnconscious : InteractionType.BludgeonUnconscious);
			MoveToAndInteractGoal moveToAndInteractGoal = survivorGoal.GetLeaderCommand() as MoveToAndInteractGoal;
			if (moveToAndInteractGoal == null || moveToAndInteractGoal.GetTargetObject() != target || moveToAndInteractGoal.GetInteractionType() != interactionType)
			{
				moveToAndInteractGoal = new MoveToAndInteractGoal(interactionType, 0, null, null, null);
			}
			moveToAndInteractGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndInteractGoal, target, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandEnterBuilding(PlayerRecord playerRecord, Building target, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndEnterBuilding moveToAndEnterBuilding = survivorGoal.GetLeaderCommand() as MoveToAndEnterBuilding;
			if (moveToAndEnterBuilding == null || moveToAndEnterBuilding.GetTargetObject() != target)
			{
				moveToAndEnterBuilding = new MoveToAndEnterBuilding();
			}
			moveToAndEnterBuilding.SetCrouching(this, null, DirectControlledCrouching);
			moveToAndEnterBuilding.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndEnterBuilding, target, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandLeaveBuildingAndGoTo(PlayerRecord playerRecord, TerrainCoord dest, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			LeaveBuildingAndMoveTo leaveBuildingAndMoveTo = survivorGoal.GetLeaderCommand() as LeaveBuildingAndMoveTo;
			if (leaveBuildingAndMoveTo == null || leaveBuildingAndMoveTo.DestTile != dest)
			{
				leaveBuildingAndMoveTo = new LeaveBuildingAndMoveTo(dest);
			}
			leaveBuildingAndMoveTo.SetCrouching(this, null, DirectControlledCrouching);
			leaveBuildingAndMoveTo.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, leaveBuildingAndMoveTo, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandLeaveBuilding(PlayerRecord playerRecord, int entranceIndex, bool isDoubleClick)
	{
		if (!(Goal is SurvivorGoal survivorGoal) || InsideBuilding == null || entranceIndex >= InsideBuilding.GetEntranceDefs().Length)
		{
			return;
		}
		TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(InsideBuilding.GetEntrancePos(entranceIndex));
		Building building = GameTerrain.Instance.GetBuilding(tileCoordForPos.x, tileCoordForPos.y);
		if (building != null && building != InsideBuilding)
		{
			if (building.CanEnter(this))
			{
				InsideBuilding.OnCharacterLeave(this, entranceIndex, fromBuildingDestroyed: false, fromRagdolled: false);
				building.OnCharacterEnter(this, wasOrderedInsideBuilding: true);
				building.PlayEnterSound(this);
				MakeMeGroupLeader();
				CancelOneOffCrafting();
				MarkPlayerControlled();
			}
			return;
		}
		LeaveBuilding leaveBuilding = survivorGoal.GetLeaderCommand() as LeaveBuilding;
		if (leaveBuilding == null || leaveBuilding.EntranceIndex != entranceIndex)
		{
			leaveBuilding = new LeaveBuilding(entranceIndex);
		}
		leaveBuilding.SetCrouching(this, null, DirectControlledCrouching);
		survivorGoal.SetLeaderCommand(this, null, leaveBuilding, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
		DirectControlled = false;
		DirectControlledMajorAIDisabled = true;
		LastDirectControlInputActionTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		MarkPlayerControlled();
	}

	public void CommandAttack(PlayerRecord playerRecord, TileObject target, bool isDoubleClick)
	{
		if (target != this && Goal is SurvivorGoal survivorGoal)
		{
			Attack attack = survivorGoal.GetLeaderCommand() as Attack;
			if (attack == null || attack.PreferredTarget != target)
			{
				attack = new Attack(this, target, dontOpenOurGates: false, StayInRangeParams.OfSquadLeader());
			}
			Target orCreateTarget = GetOrCreateTarget(target);
			orCreateTarget.ForceVisible(this);
			orCreateTarget.ClearInaccessible();
			attack.SetMovementTypeWhenNotInDanger(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, attack, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			OrderedToAttack = true;
		}
	}

	public void CommandVaultWaistHighWall(PlayerRecord playerRecord, TileObject prop, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndVaultWaistHighWall moveToAndVaultWaistHighWall = survivorGoal.GetLeaderCommand() as MoveToAndVaultWaistHighWall;
			if (moveToAndVaultWaistHighWall == null || moveToAndVaultWaistHighWall.GetTargetObject() != prop)
			{
				moveToAndVaultWaistHighWall = new MoveToAndVaultWaistHighWall();
			}
			moveToAndVaultWaistHighWall.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			if (EquippedItem != null && (EquippedItem.GetEquippedAnim() == EquippedAnim.Pistol || EquippedItem.GetEquippedAnim() == EquippedAnim.Rifle))
			{
				moveToAndVaultWaistHighWall.SetAiming(this, null, aiming: true);
			}
			survivorGoal.SetLeaderCommand(this, null, moveToAndVaultWaistHighWall, prop, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
			DisableInputsUntilNextThink = true;
		}
	}

	public void CommandPickUp(PlayerRecord playerRecord, TileObject prop, bool isDoubleClick)
	{
		if (prop != this && Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndPickUp moveToAndPickUp = survivorGoal.GetLeaderCommand() as MoveToAndPickUp;
			if (moveToAndPickUp == null || moveToAndPickUp.GetTargetObject() != prop)
			{
				moveToAndPickUp = new MoveToAndPickUp();
			}
			moveToAndPickUp.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndPickUp, prop, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandGrab(PlayerRecord playerRecord, TileObject prop, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndGrab moveToAndGrab = survivorGoal.GetLeaderCommand() as MoveToAndGrab;
			if (moveToAndGrab == null || moveToAndGrab.GetTargetObject() != prop)
			{
				moveToAndGrab = new MoveToAndGrab();
			}
			moveToAndGrab.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndGrab, prop, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandChop(PlayerRecord playerRecord, TileObject prop, bool isDoubleClick)
	{
		RemoveFailedFindAttempt(prop);
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndChop moveToAndChop = survivorGoal.GetLeaderCommand() as MoveToAndChop;
			if (moveToAndChop == null || moveToAndChop.GetTargetObject() != prop)
			{
				moveToAndChop = new MoveToAndChop();
			}
			moveToAndChop.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndChop, prop, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandMine(PlayerRecord playerRecord, TileObject prop, MineralType mineralType, bool isDoubleClick)
	{
		RemoveFailedFindAttempt(prop);
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndMine moveToAndMine = survivorGoal.GetLeaderCommand() as MoveToAndMine;
			if (moveToAndMine == null || moveToAndMine.GetTargetObject() != prop || moveToAndMine.MineralType != mineralType)
			{
				moveToAndMine = new MoveToAndMine();
				moveToAndMine.MineralType = mineralType;
			}
			moveToAndMine.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndMine, prop, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandLightFire(PlayerRecord playerRecord, Equipment lighter, TileObject obj, bool isDoubleClick)
	{
		if (obj != this && Goal is SurvivorGoal survivorGoal)
		{
			LightFireGoal lightFireGoal = survivorGoal.GetLeaderCommand() as LightFireGoal;
			if (lightFireGoal == null || lightFireGoal.GetTargetObject() != obj)
			{
				lightFireGoal = new LightFireGoal(lighter, (!isDoubleClick) ? MovementType.Walk : MovementType.Run, canAddMaterialToFire: true);
			}
			lightFireGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, lightFireGoal, obj, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandAddMaterialToFire(PlayerRecord playerRecord, Equipment item, TerrainCoord tile, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndAddMaterialToFire moveToAndAddMaterialToFire = survivorGoal.GetLeaderCommand() as MoveToAndAddMaterialToFire;
			if (moveToAndAddMaterialToFire == null || moveToAndAddMaterialToFire.Tile != tile || moveToAndAddMaterialToFire.Item != item)
			{
				moveToAndAddMaterialToFire = new MoveToAndAddMaterialToFire(this, tile, item, (!isDoubleClick) ? MovementType.Walk : MovementType.Run, critical: false);
			}
			moveToAndAddMaterialToFire.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndAddMaterialToFire, GameTerrain.Instance.GetFixedObjectOnTile(tile.x, tile.y), ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandSitByFire(PlayerRecord playerRecord, Campfire campfire, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			SitAroundFireGoal sitAroundFireGoal = survivorGoal.GetLeaderCommand() as SitAroundFireGoal;
			if (sitAroundFireGoal == null || sitAroundFireGoal.GetTargetObject() != campfire)
			{
				sitAroundFireGoal = new SitAroundFireGoal();
			}
			sitAroundFireGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, sitAroundFireGoal, campfire, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandRepairArmor(PlayerRecord playerRecord, WorkBench workBench, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToBenchAndRepairArmor moveToBenchAndRepairArmor = survivorGoal.GetLeaderCommand() as MoveToBenchAndRepairArmor;
			if (moveToBenchAndRepairArmor == null || moveToBenchAndRepairArmor.GetTargetObject() != workBench)
			{
				moveToBenchAndRepairArmor = new MoveToBenchAndRepairArmor(this, workBench, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			moveToBenchAndRepairArmor.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToBenchAndRepairArmor, workBench, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandSkin(PlayerRecord playerRecord, Character character, bool isDoubleClick)
	{
		if (character != this && Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndSkin moveToAndSkin = survivorGoal.GetLeaderCommand() as MoveToAndSkin;
			if (moveToAndSkin == null || moveToAndSkin.GetTargetObject() != character)
			{
				moveToAndSkin = new MoveToAndSkin();
			}
			moveToAndSkin.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndSkin, character, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandWaterPlant(PlayerRecord playerRecord, Equipment liquidContainer, TileObject obj, bool pourOnto, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			WaterPlant waterPlant = survivorGoal.GetLeaderCommand() as WaterPlant;
			if (waterPlant == null || waterPlant.GetTargetObject() != obj || waterPlant.PourOnto != pourOnto)
			{
				waterPlant = new WaterPlant(liquidContainer);
				waterPlant.PourOnto = pourOnto;
			}
			waterPlant.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, waterPlant, obj, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandFillLiquidContainer(PlayerRecord playerRecord, Equipment liquidContainer, TerrainCoord tile, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			FillLiquidContainer fillLiquidContainer = survivorGoal.GetLeaderCommand() as FillLiquidContainer;
			if (fillLiquidContainer == null || fillLiquidContainer.Tile != tile)
			{
				fillLiquidContainer = new FillLiquidContainer(this, tile, -1, liquidContainer, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			fillLiquidContainer.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, fillLiquidContainer, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			IssueCommandToFollowers(FollowCommand.FillWaterBottle, tile, null);
			DirectControlled = false;
		}
	}

	public void CommandPlantCrops(PlayerRecord playerRecord, Equipment seeds, TerrainCoord tile, bool isDoubleClick)
	{
		Session.Instance.CropsManager.SetPlantableCropType(GetCommunityId(), tile, seeds.GetSeedForPlantType());
		if (Goal is SurvivorGoal survivorGoal)
		{
			PlantCrops plantCrops = survivorGoal.GetLeaderCommand() as PlantCrops;
			if (plantCrops == null || plantCrops.Tile != tile)
			{
				plantCrops = new PlantCrops(tile, seeds, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			plantCrops.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, plantCrops, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandHarvestCrops(PlayerRecord playerRecord, TerrainCoord tile, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			HarvestCrops harvestCrops = survivorGoal.GetLeaderCommand() as HarvestCrops;
			if (harvestCrops == null || harvestCrops.Tile != tile)
			{
				harvestCrops = new HarvestCrops(tile, ignoreWeight: false);
			}
			harvestCrops.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, harvestCrops, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandResetTrap(PlayerRecord playerRecord, TileObject trap, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndResetTrap moveToAndResetTrap = survivorGoal.GetLeaderCommand() as MoveToAndResetTrap;
			if (moveToAndResetTrap == null || moveToAndResetTrap.GetTargetObject() != trap)
			{
				moveToAndResetTrap = new MoveToAndResetTrap();
			}
			moveToAndResetTrap.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndResetTrap, trap, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandBuild(TileObject building, bool isDoubleClick, bool justPlaced)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Builder);
		if (CanAddRole(roleInfo) && Goal is SurvivorGoal { BuildGoal: { } buildGoal } survivorGoal && building.GetUnderConstructionInfo() != null && buildGoal.SetCurrentBuilding(this, survivorGoal, building))
		{
			if (isDoubleClick)
			{
				buildGoal.SetMovementType(this, MovementType.Run);
			}
			else if (!justPlaced)
			{
				buildGoal.SetMovementType(this, MovementType.Walk);
			}
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			CancelOneOffCrafting();
			ResetBuildGoalLastAttemptedTimeAndFinish();
		}
	}

	public void CommandCraft(Recipe recipe, Equipment item, bool recurring, int amount, TerrainCoord tile, Prop.OrientationType orientation, bool isDoubleClick)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Crafter, recipe, tile);
		if ((recurring && !CanAddRole(roleInfo)) || !(Goal is SurvivorGoal { CraftGoal: { } craftGoal }))
		{
			return;
		}
		craftGoal.StartRecipe(this, recipe, item, recurring ? int.MaxValue : amount, tile, orientation, wasTriggeredFromDirectControl: true, isDoubleClick);
		craftGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
		if (recurring)
		{
			AddRole(roleInfo);
			if (Community != null && recipe.ProductPrototype != null)
			{
				Community.SetCraftingLimit(recipe.ProductPrototype, amount);
			}
			if (Community != null && recipe.ProductLiquidPrototype != null)
			{
				Community.SetCraftingLimit(recipe.ProductLiquidPrototype, amount);
			}
		}
		else
		{
			ClearLeaderCommand();
		}
		DirectControlled = false;
		MarkPlayerControlled();
		ResetCraftGoalLastAttemptedTimeAndFinish(recurring);
		if (recurring)
		{
			ResetCraftGoalHasSaidInsufficientResources();
		}
	}

	public void CommandResumeCrafting(CraftingProp craftingProp, bool isDoubleClick)
	{
		if (!craftingProp.IsCrafting() || !(Goal is SurvivorGoal { CraftGoal: { } craftGoal }))
		{
			return;
		}
		bool flag = craftingProp.CraftingDesiredAmount == int.MaxValue;
		int craftingDesiredAmount = craftingProp.CraftingDesiredAmount;
		if (craftingProp.CurrentCrafter != null && craftingProp.CurrentCrafter != this)
		{
			craftingProp.CurrentCrafter.GetCraftGoal()?.StopRecipe(craftingProp.CurrentCrafter);
			craftingProp.SetCrafter(this);
		}
		craftGoal.StartRecipe(this, craftingProp.CraftingRecipe, null, flag ? int.MaxValue : craftingDesiredAmount, craftingProp.GetCentreTile(), Prop.OrientationType.Deg0, wasTriggeredFromDirectControl: true, isDoubleClick);
		craftGoal.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
		if (flag)
		{
			if (craftGoal.GetRoleBeingPerformed(this) == Role.Cook)
			{
				ResumeRole(new RoleInfo(Role.Cook));
			}
			else
			{
				AddRole(new RoleInfo(Role.Crafter, craftingProp.CraftingRecipe, craftingProp.GetCentreTile()));
			}
		}
		DirectControlled = false;
		MarkPlayerControlled();
		ResetCraftGoalLastAttemptedTimeAndFinish(flag);
		if (flag)
		{
			ResetCraftGoalHasSaidInsufficientResources();
		}
	}

	public void CommandTake(PlayerRecord playerRecord, TileObject hoverObject, bool isDoubleClick, PlayerID controllingPlayerID)
	{
		if (hoverObject != this && Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndTake moveToAndTake = survivorGoal.GetLeaderCommand() as MoveToAndTake;
			if (moveToAndTake == null || moveToAndTake.Target.Object != hoverObject)
			{
				moveToAndTake = new MoveToAndTake(controllingPlayerID);
			}
			moveToAndTake.Pickpocketing = (hoverObject as Character)?.IsAwake ?? false;
			moveToAndTake.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndTake, hoverObject, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandTakeAll(PlayerRecord playerRecord, TileObject hoverObject, bool isDoubleClick, PlayerID controllingPlayerID)
	{
		if (hoverObject != this && Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndTakeAll moveToAndTakeAll = survivorGoal.GetLeaderCommand() as MoveToAndTakeAll;
			if (moveToAndTakeAll == null || moveToAndTakeAll.Target.Object != hoverObject)
			{
				moveToAndTakeAll = new MoveToAndTakeAll();
			}
			moveToAndTakeAll.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndTakeAll, hoverObject, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void Knock(Gate gate)
	{
		Session instance = Session.Instance;
		if (gate != null && gate.Community != null && Community != null && instance.CommunityManager.GetRelationship(Community, gate.Community) == CommunityRelationshipType.Introducing && !gate.Community.IsAnyoneIntroducing(Community))
		{
			instance.CommunityManager.SetRelationship(Community, gate.Community, CommunityRelationshipType.Unknown);
		}
		PlaySoundOneShot(SoundManager.KnockSound);
		instance.AISoundManager.AddSound(new AISound(AISoundType.Knock, Pos, 64f, 64f, this, this, null, null));
	}

	public void CommandRepair(TileObject target)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Repairing);
		if (CanAddRole(roleInfo) && Goal is SurvivorGoal { RepairGoal: { } repairGoal } survivorGoal)
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
			repairGoal.SetCurrentBuildingToRepair(this, survivorGoal, target);
			repairGoal.ResetLastAttemptedTime();
		}
	}

	public void CommandCapture(TileObject target)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Capturing, target.GetTile());
		if (CanAddRole(roleInfo) && Goal is SurvivorGoal { CaptureGoal: { } captureGoal })
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
			captureGoal.Restart();
			captureGoal.ResetLastAttemptedTime();
		}
	}

	public void CommandEat(PlayerRecord playerRecord, Equipment item)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			EatGoal eatGoal = survivorGoal.GetLeaderCommand() as EatGoal;
			if (eatGoal == null || eatGoal._food != item)
			{
				eatGoal = new EatGoal(item);
				eatGoal.Crouching = DirectControlledCrouching;
			}
			survivorGoal.SetLeaderCommand(this, null, eatGoal, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandDrink(PlayerRecord playerRecord, Equipment item)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			DrinkGoal drinkGoal = survivorGoal.GetLeaderCommand() as DrinkGoal;
			if (drinkGoal == null || drinkGoal._drink != item)
			{
				drinkGoal = new DrinkGoal(item);
				drinkGoal.Crouching = DirectControlledCrouching;
			}
			survivorGoal.SetLeaderCommand(this, null, drinkGoal, null, (playerRecord == null) ? ObeyLeaderGoal.SourceType.Scripted : ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void CommandUse(PlayerRecord playerRecord, Equipment item)
	{
		if (!(Goal is SurvivorGoal survivorGoal))
		{
			return;
		}
		if (item.GetPrototype().HasCustomUseAction)
		{
			BaseObject obj = item;
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, null, ref obj, SpeechSituation.CustomUseAction, default(MemoryParam));
			if (speechForSituation != null)
			{
				CommandSpeakTo(playerRecord, null, obj, default(MemoryParam), speechForSituation, null, null, default(MemoryParam), isDoubleClick: false);
			}
		}
		else if (item.IsEdible())
		{
			CommandEat(playerRecord, item);
		}
		else if (item.IsDrinkable())
		{
			CommandDrink(playerRecord, item);
		}
		else if (item.GetBandageLevel() > -1)
		{
			BandageSelfAnim bandageSelfAnim = survivorGoal.GetLeaderCommand() as BandageSelfAnim;
			if (bandageSelfAnim == null || bandageSelfAnim.Bandage != item)
			{
				bandageSelfAnim = new BandageSelfAnim(item);
				bandageSelfAnim.Crouching = DirectControlledCrouching;
			}
			survivorGoal.SetLeaderCommand(this, null, bandageSelfAnim, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
		else if (item.GetAntigenType() != InfectionType.None)
		{
			MedicateSelfAnim medicateSelfAnim = survivorGoal.GetLeaderCommand() as MedicateSelfAnim;
			if (medicateSelfAnim == null || medicateSelfAnim._syringe != item)
			{
				medicateSelfAnim = new MedicateSelfAnim(item);
				medicateSelfAnim.Crouching = DirectControlledCrouching;
			}
			survivorGoal.SetLeaderCommand(this, null, medicateSelfAnim, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
		else
		{
			item.OnUsedFromInfoScreen(playerRecord, this, this);
		}
	}

	public void CommandDrinkFromRiver(PlayerRecord playerRecord, TerrainCoord tile, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			TerrainCoord nearestTileOnPath = GameTerrain.Instance.GetNearestTileOnPath(GameTerrain.Instance.GetTileCentreXZ(tile), river: true, 32f);
			if (nearestTileOnPath != TerrainCoord.Invalid)
			{
				tile = nearestTileOnPath;
			}
			MoveToAndDrinkFromRiver moveToAndDrinkFromRiver = survivorGoal.GetLeaderCommand() as MoveToAndDrinkFromRiver;
			if (moveToAndDrinkFromRiver == null || moveToAndDrinkFromRiver.Tile != tile)
			{
				moveToAndDrinkFromRiver = new MoveToAndDrinkFromRiver(this, tile, -1, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			moveToAndDrinkFromRiver.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndDrinkFromRiver, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			IssueCommandToFollowers(FollowCommand.FillWaterBottle, tile, null);
			DirectControlled = false;
		}
	}

	public void CommandScoopSnow(PlayerRecord playerRecord, TerrainCoord tile, bool isDoubleClick)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			MoveToAndScoopSnow moveToAndScoopSnow = survivorGoal.GetLeaderCommand() as MoveToAndScoopSnow;
			if (moveToAndScoopSnow == null || moveToAndScoopSnow.Tile != tile)
			{
				moveToAndScoopSnow = new MoveToAndScoopSnow(this, tile, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			}
			moveToAndScoopSnow.SetMovementType(this, (!isDoubleClick) ? MovementType.Walk : MovementType.Run);
			survivorGoal.SetLeaderCommand(this, null, moveToAndScoopSnow, null, ObeyLeaderGoal.SourceType.Player, playerRecord);
			DirectControlled = false;
		}
	}

	public void PauseAllRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			PauseRole(i);
		}
	}

	public void PauseRole(RoleInfo roleInfo)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex != -1)
		{
			PauseRole(roleIndex);
		}
	}

	public void PauseRole(int i)
	{
		if (!Roles[i].Paused)
		{
			RoleInfo value = Roles[i];
			value.Paused = true;
			Roles[i] = value;
			if (MostRecentRoleIndex == i)
			{
				MostRecentRoleIndex = -1;
			}
			if (HaveAllRunningRolesFailedRecently())
			{
				ClearFailedRecentlyOnAllRoles();
			}
			if (InfoScreen.Instance.IsShowingInventoryFor(this))
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
		}
	}

	public void AddDownTime(float downTime)
	{
		DownTime += downTime;
		PauseAllRoles();
	}

	public void ResumeAllRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			ResumeRole(i);
		}
	}

	public void ResumeRole(RoleInfo roleInfo)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex != -1)
		{
			ResumeRole(roleIndex);
		}
	}

	public void ResumeRole(int i)
	{
		if (!(DownTime > 0f) && Roles[i].Paused)
		{
			RoleInfo value = Roles[i];
			value.Paused = false;
			value.FailedRecently = false;
			value.LastFailedTime = Target.Never;
			Roles[i] = value;
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			switch (Roles[i].Role)
			{
			case Role.Guard:
				ResetGuardGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Gatherer:
				ResetGatherGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Farmer:
				ResetFarmingGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Lumberjack:
				ResetLumberjackGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Crafter:
				ResetCraftGoalLastAttemptedTimeAndFinish(notIfDoingOneOffCrafting: true);
				ResetCraftGoalHasSaidInsufficientResources();
				break;
			case Role.Miner:
				ResetMinerGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Trapper:
				ResetTrapperGoalLastAttemptedTimeAndFinish();
				break;
			case Role.AnimalFeeder:
				ResetAnimalFeederGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Organizer:
				ResetOrganizerGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Builder:
				ResetBuildGoalLastAttemptedTimeAndFinish();
				break;
			case Role.Medic:
				ResetMedicGoalLastAttemptedTimeAndFinish();
				break;
			}
			if (InfoScreen.Instance.IsShowingInventoryFor(this))
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
		}
	}

	public void ChangeRolePriority(RoleInfo roleInfo, int dir)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex == -1)
		{
			return;
		}
		int num = roleIndex + dir;
		if (num >= 0 && num < Roles.Count)
		{
			roleInfo = Roles[roleIndex];
			Roles.RemoveAt(roleIndex);
			Roles.Insert(num, roleInfo);
			if (MostRecentRoleIndex == roleIndex)
			{
				MostRecentRoleIndex = num;
			}
			if (InfoScreen.Instance.IsShowingInventoryFor(this))
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
		}
	}

	public void SetRoleUrgent(RoleInfo roleInfo, bool urgent, PlayerRecord playerRecord)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex == -1 || Roles[roleIndex].Urgent == urgent)
		{
			return;
		}
		roleInfo = Roles[roleIndex];
		roleInfo.Urgent = urgent;
		roleInfo.FailedRecently = false;
		if (urgent && IsSatisfyingNeeds())
		{
			roleInfo.LastFailedTime = Target.Never;
		}
		Roles[roleIndex] = roleInfo;
		if (InfoScreen.Instance.IsShowingInventoryFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		if (!urgent && !HintManager.Instance.Hints[31].Performed && Hud.Instance.LocalControlledCharacter == this && !IsPlayerAvatar())
		{
			HintManager.Instance.Hints[31].MarkPerformed();
		}
		if (playerRecord == null)
		{
			sb.Length = 0;
			RoleDisplayBehaviour.GetRoleDisplayText(this, roleInfo, RoleDisplayTextMode.Medium, sb);
			HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate(urgent ? "HUD_SetRoleUrgent" : "HUD_SetRoleNotUrgent").Replace("%1", sb.ToString()));
		}
		else if (FindActiveGoalOfClass(typeof(RoleGoal)) is RoleGoal roleGoal && roleGoal.GetRoleInfoBeingPerformed(this).Equals(roleInfo))
		{
			roleGoal.SetMovementType(this, (!urgent) ? MovementType.Walk : ((roleInfo.Role == Role.Gatherer || roleInfo.Role == Role.Miner) ? MovementType.Jog : MovementType.Run));
		}
		if (urgent && IsSatisfyingNeeds() && Goal is SurvivorGoal survivorGoal)
		{
			survivorGoal.SubGoal.Finished = true;
			if (playerRecord != null)
			{
				playerRecord.JustSetRoleToUrgent = true;
			}
			MostRecentRoleIndex = roleIndex;
			switch (roleInfo.Role)
			{
			case Role.Lumberjack:
				survivorGoal.LumberjackGoal.ResetLastAttemptedTime();
				break;
			case Role.Guard:
				survivorGoal.GuardGoal.ResetLastAttemptedTime();
				break;
			case Role.Gatherer:
				survivorGoal.GatherGoal.ResetLastAttemptedTime();
				break;
			case Role.Farmer:
				survivorGoal.FarmingGoal.ResetLastAttemptedTime();
				break;
			case Role.Organizer:
				survivorGoal.OrganizerGoal.ResetLastAttemptedTime();
				break;
			case Role.Trapper:
				survivorGoal.TrapperGoal.ResetLastAttemptedTime();
				break;
			case Role.Crafter:
				survivorGoal.CraftGoal.ResetLastAttemptedTime();
				break;
			case Role.Cook:
				survivorGoal.CraftGoal.ResetLastAttemptedTime();
				break;
			case Role.AnimalFeeder:
				survivorGoal.AnimalFeederGoal.ResetLastAttemptedTime();
				break;
			case Role.Builder:
				survivorGoal.BuildGoal.ResetLastAttemptedTime();
				break;
			case Role.Miner:
				survivorGoal.MinerGoal.ResetLastAttemptedTime();
				break;
			case Role.Capturing:
				survivorGoal.CaptureGoal.ResetLastAttemptedTime();
				break;
			case Role.Repairing:
				survivorGoal.RepairGoal.ResetLastAttemptedTime();
				break;
			case Role.Medic:
				survivorGoal.MedicGoal.ResetLastAttemptedTime();
				break;
			case Role.Trader:
			case Role.NoRole:
			case Role.Enforcer:
				break;
			}
		}
	}

	public bool IsRoleUrgent(RoleInfo roleInfo)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex == -1)
		{
			return false;
		}
		return Roles[roleIndex].Urgent;
	}

	public void TryToEnsureHangoutLocationIsSet(bool forceOverride = false)
	{
		if (HangOutLocation != TerrainCoord.Invalid && !forceOverride)
		{
			return;
		}
		if ((InitialHangOutLocation == TerrainCoord.Invalid || forceOverride) && Community != null && Community.BaseRect.VerticesArea > 0 && Community.IsAISettlement())
		{
			for (int i = 0; i < 100; i++)
			{
				InitialHangOutLocation = Session.Instance.DeterministicRand.RandomTile(Community.BaseRect.min, Community.BaseRect.max);
				if (Community.Perimeter != null && Community.Perimeter.Count > 0)
				{
					if (MathUtil.IsTileInPolygon(InitialHangOutLocation, Community.Perimeter))
					{
						break;
					}
				}
				else if (GameTerrain.Instance.IsTileEnclosed(InitialHangOutLocation.x, InitialHangOutLocation.y))
				{
					break;
				}
			}
		}
		SetHangoutLocation(InitialHangOutLocation);
	}

	public void SetHangoutLocation(TerrainCoord tile)
	{
		HangOutLocation = ((tile != TerrainCoord.Invalid && HasMovementZone()) ? MovementZone.ClampTileWithinBounds(tile) : tile);
	}

	public void Guard(Building target)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Guard, target.Tile);
		if (CanAddRole(roleInfo))
		{
			RemoveFailedFindAttempt(target);
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetGuardGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void Gather(Prop target, EquipmentPrototype gatherType)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Gatherer, target.Tile, gatherType);
		if (CanAddRole(roleInfo))
		{
			RemoveFailedFindAttempt(target);
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetGatherGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void Farm(TerrainCoord tile, bool resetFarmingGoal)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Farmer, tile);
		if (CanAddRole(roleInfo))
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			if (resetFarmingGoal)
			{
				ResetFarmingGoalLastAttemptedTimeAndFinish();
			}
			MakeMeGroupLeader();
			CancelOneOffCrafting();
			ShowFarmerWarningIfTooCold();
		}
	}

	public void ShowFarmerWarningIfTooCold()
	{
		if (!Session.Instance.Weather.IsSafeForPlantingCrops())
		{
			string str = GameImpl.Translate("HINT_WontPlantNewCropsTooCold").Replace("%1", GetDisplayNameString());
			str = StringUtil.ApplyFormulae(str, null, this);
			HudBehaviour.Instance.SetStatusBarMsg(str);
		}
	}

	public void Lumberjack(TileObject target)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Lumberjack, target.GetCentreTile());
		if (CanAddRole(roleInfo))
		{
			RemoveFailedFindAttempt(target);
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetLumberjackGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void Cook(Campfire campfire)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Cook, campfire.GetCentreTile());
		if (CanAddRole(roleInfo))
		{
			RemoveFailedFindAttempt(campfire);
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetCraftGoalLastAttemptedTimeAndFinish(notIfDoingOneOffCrafting: true);
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void SetMiner(TileObject target, MineralType mineralType)
	{
		if ((target.GetMiningResourceType() != null || target.GetBaseObjectType() == BaseObjectType.Mine) && mineralType != MineralType.None)
		{
			RoleInfo roleInfo = new RoleInfo(Role.Miner, target.GetCentreTile(), EquipmentPrototype.MiningResources[(int)mineralType]);
			if (CanAddRole(roleInfo))
			{
				RemoveFailedFindAttempt(target);
				AddRole(roleInfo);
				DirectControlled = false;
				DirectControlledMajorAIDisabled = false;
				MarkPlayerControlled();
				ResetMinerGoalLastAttemptedTimeAndFinish();
				MakeMeGroupLeader();
				CancelOneOffCrafting();
			}
		}
	}

	public void SetTrapper(TileObject target)
	{
		RoleInfo roleInfo = new RoleInfo(Role.Trapper, target.GetCentreTile());
		if (CanAddRole(roleInfo))
		{
			RemoveFailedFindAttempt(target);
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetTrapperGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void SetAnimalFeeder()
	{
		RoleInfo roleInfo = new RoleInfo(Role.AnimalFeeder);
		if (CanAddRole(roleInfo))
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetAnimalFeederGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void SetOrganizer()
	{
		RoleInfo roleInfo = new RoleInfo(Role.Organizer);
		if (CanAddRole(roleInfo))
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetOrganizerGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public void SetMedic()
	{
		RoleInfo roleInfo = new RoleInfo(Role.Medic);
		if (CanAddRole(roleInfo))
		{
			AddRole(roleInfo);
			DirectControlled = false;
			DirectControlledMajorAIDisabled = false;
			MarkPlayerControlled();
			ResetOrganizerGoalLastAttemptedTimeAndFinish();
			MakeMeGroupLeader();
			CancelOneOffCrafting();
		}
	}

	public bool GetDontSimulateSurvivalFactorsUntilDiscovered()
	{
		return DontSimulateSurvivalFactorsUntilDiscovered;
	}

	public bool GetDontSimulateSurvivalFactorsUntilJoinCommunity()
	{
		return DontSimulateSurvivalFactorsUntilJoinCommunity;
	}

	public bool GetDontSimulateSurvivalFactors()
	{
		if (!DontSimulateSurvivalFactorsUntilDiscovered)
		{
			return DontSimulateSurvivalFactorsUntilJoinCommunity;
		}
		return true;
	}

	public bool IsSleeping()
	{
		return Consciousness == Consciousness.Sleeping;
	}

	public bool IsSleepingOrUnconscious()
	{
		if (Consciousness != Consciousness.Sleeping)
		{
			return Consciousness == Consciousness.Unconscious;
		}
		return true;
	}

	public float GetBloodLoss()
	{
		return BloodLoss;
	}

	public float GetFatigue()
	{
		return Fatigue;
	}

	public float GetAdrenaline()
	{
		return Adrenaline;
	}

	public float GetFatigueMinusAdrenaline()
	{
		if (!(Adrenaline > 0f))
		{
			return Fatigue;
		}
		return 0f;
	}

	public float GetHunger()
	{
		return Hunger;
	}

	public float GetThirst()
	{
		return Thirst;
	}

	public float GetThirstInFlOz()
	{
		return Thirst * WaterNeededPerDayInFlOz / Sun.DayLengthSecs;
	}

	public float GetToilet()
	{
		return Toilet;
	}

	public float GetSleepDeprivation()
	{
		return SleepDeprivation;
	}

	public float GetBodyTemperatureInCelsius()
	{
		return BodyTemperatureInCelsius;
	}

	public float GetBodyTemperatureIncludingFeverInCelsius()
	{
		if (AliveAndNotZombie)
		{
			return BodyTemperatureInCelsius + (BodyTemperatureInCelsiusHyperpyrexia - BodyTemperatureInCelsiusNormal) * InfectionProgression;
		}
		return Session.Instance.Weather.TemperatureInCelsius;
	}

	public float GetBloodAlcoholConcentration()
	{
		return BloodAlcoholConcentration;
	}

	public float GetLungCancer()
	{
		return LungCancer;
	}

	public float GetInfectionProgression()
	{
		return InfectionProgression;
	}

	public float GetExcitement()
	{
		return Excitement;
	}

	public int GetReservedGoldAmount()
	{
		return ReservedGoldAmount;
	}

	public bool GetPlayDead()
	{
		return PlayDead;
	}

	public void SetDontSimulateSurvivalFactorsUntilDiscovered(bool v)
	{
		DontSimulateSurvivalFactorsUntilDiscovered = v;
	}

	public void SetDontSimulateSurvivalFactorsUntilJoinCommunity(bool v)
	{
		DontSimulateSurvivalFactorsUntilJoinCommunity = v;
	}

	public void SetBloodLoss(float v)
	{
		BloodLoss = v;
	}

	public void SetFatigue(float v)
	{
		Fatigue = v;
	}

	public void SetHunger(float v)
	{
		Hunger = v;
	}

	public void SetThirst(float v)
	{
		Thirst = v;
	}

	public void SetToilet(float v)
	{
		Toilet = v;
	}

	public void SetSleepDeprivation(float v)
	{
		SleepDeprivation = v;
	}

	public void SetBodyTemperatureInCelsius(float v)
	{
		BodyTemperatureInCelsius = v;
	}

	public void SetBloodAlcoholConcentration(float v)
	{
		BloodAlcoholConcentration = v;
	}

	public void SetLungCancer(float v)
	{
		LungCancer = v;
	}

	public void SetInfectionProgression(float v)
	{
		InfectionProgression = v;
	}

	public void SetExcitement(float v)
	{
		Excitement = v;
	}

	public void SetFacingAngle(float val)
	{
		FacingAngle = (DesiredFacingAngle = val);
		UpdateWorldTransformAndBounds();
	}

	public void SetReservedGoldAmount(int v)
	{
		ReservedGoldAmount = v;
	}

	public void SetPlayDead(bool on)
	{
		PlayDead = on;
		if (PlayDead)
		{
			if (Goal is SurvivorGoal survivorGoal && survivorGoal.FindSubGoalByType(GoalType.PlayDead) == null)
			{
				survivorGoal.AddSubGoal(new PlayDead());
			}
			if (Goal is ZombieGoal zombieGoal && zombieGoal.FindSubGoalByType(GoalType.PlayDead) == null)
			{
				zombieGoal.AddSubGoal(new PlayDead());
			}
		}
	}

	public virtual void SetSitting(TerrainCoord sittingAroundTile)
	{
		Sitting = true;
		SittingAround = sittingAroundTile;
	}

	public virtual void ClearSitting()
	{
		Sitting = false;
		SittingAround = TerrainCoord.Invalid;
	}

	public float GetMinFatigue()
	{
		if (Zombie)
		{
			return 0f;
		}
		return Math.Min(1f, Math.Max(SleepDeprivation - SleepDeprivationCriticalTime, 0f) / (SleepDeprivationDieTime - SleepDeprivationCriticalTime) + Math.Max(Hunger - HungerCriticalTime, 0f) / (HungerDieTime - HungerCriticalTime) + Math.Max(Thirst - ThirstCriticalTime, 0f) / (ThirstDieTime - ThirstCriticalTime) + (1f - Mathf.Clamp01((BodyTemperatureInCelsius - BodyTemperatureInCelsiusUnconscious) / (BodyTemperatureInCelsiusModerateHypothermia - BodyTemperatureInCelsiusUnconscious))) + Math.Max(InfectionProgression - InfectionProgressionCriticalLevel, 0f) / (1f - InfectionProgressionCriticalLevel) + LungCancer + ((Encumbered && IsControllableByPlayer()) ? 1f : 0f));
	}

	public float GetFatiguePenaltySkillFactor()
	{
		return Mathf.Lerp(1f, 0.5f, (float)GetSkillLevelWithEffects(SkillType.Strength) / 5f);
	}

	public bool IsTooTiredFor(float fatigue)
	{
		if (Zombie)
		{
			return false;
		}
		float fatiguePenaltySkillFactor = GetFatiguePenaltySkillFactor();
		return GetFatigueMinusAdrenaline() > 1f - fatigue * fatiguePenaltySkillFactor;
	}

	public void ApplyFatiguePenalty(float fatigue)
	{
		if (!Zombie && GetBaseObjectType() == BaseObjectType.Human && !God)
		{
			Skillset.AddProgress(this, SkillType.Strength, fatigue);
			float fatiguePenaltySkillFactor = GetFatiguePenaltySkillFactor();
			if (TemperatureInsulationDelta > 0f)
			{
				fatigue *= 1f + TemperatureInsulationDelta * OverdressedFatiguePenalty;
			}
			BodyTemperatureInCelsius += fatigue * WarmthFromExercising;
			Hunger += fatigue * HungerFromExercising;
			Thirst += fatigue * ThirstFromExercising;
			Fatigue = Math.Min(Fatigue + fatigue * fatiguePenaltySkillFactor, 1f);
		}
	}

	public bool CanSleep()
	{
		if (InsideBuilding != null && !IsOutdoors())
		{
			if (DisableSleep > 0 || IsCraftingAnim() || IsMining() || CurrentActionAnim == ActionAnim.Pee)
			{
				return false;
			}
			if (InsideBuilding is EnterableVehicle { IsMoving: not false })
			{
				return false;
			}
			if (InsideBuilding != null && InsideBuilding.IsBurning())
			{
				return false;
			}
			if (GetPregnancyProgression() >= 1f)
			{
				return false;
			}
			if (Sun.GetSunIntensity(Session.Instance.DaysSinceStart) <= 0.5f)
			{
				return true;
			}
			if (IsSleepingOrUnconscious())
			{
				if (InsideBuilding.Community == Community)
				{
					return SleepDeprivation > 0f;
				}
				return SleepDeprivation >= SleepyTime;
			}
			if (InsideBuilding.Community == Community)
			{
				return SleepDeprivation >= SleepyTime;
			}
			return SleepDeprivation >= SleepDeprivationCriticalTime;
		}
		return false;
	}

	public bool ShouldBeUnconscious()
	{
		if (!(BloodLoss >= 1f) && !(BodyTemperatureInCelsius <= ((Consciousness == Consciousness.Unconscious) ? BodyTemperatureInCelsiusWakeUp : BodyTemperatureInCelsiusUnconscious)) && !(BloodAlcoholConcentration >= BACStupor))
		{
			return SedativeEffect > 0f;
		}
		return true;
	}

	public void SetConsciousness(Consciousness consciousness, bool canSpeak = true)
	{
		if (Consciousness == consciousness)
		{
			return;
		}
		if (Consciousness == Consciousness.Unconscious && consciousness < Consciousness.Unconscious)
		{
			if (IsAuthoritative() && InsideBuilding != null)
			{
				InsideBuilding.OnCharacterLeave(this, 0, fromBuildingDestroyed: false, fromRagdolled: false);
			}
			WasAttackedBeforeLastCeaseFire = false;
			if (IsControllableByPlayer())
			{
				ClearLeaderCommand();
			}
			if (Zombie && CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				PlayVoiceSoundFromList(SoundManager.ZombiePainSounds, VoiceSoundType.ZombieSnarl);
			}
			if (LastPersonChokedMe != null)
			{
				if (!Zombie && this is Human && IsAuthoritative())
				{
					Character character = LastPersonChokedMe;
					if (Community != null)
					{
						float num = MathUtil.Squared(32f);
						foreach (Character member in Community.Members)
						{
							if (!member.ConsciousAndNotZombie || member.GetBaseObjectType() != BaseObjectType.Human)
							{
								continue;
							}
							float sqrMagnitude = (member.PosXZ - PosXZ).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								int num2 = member.FindMemoryOfObjectAfter(MemoryPrototype.KnockedOut, this, LastKnockedOutTime);
								if (num2 != -1 && member.Memories[num2].Actor != null)
								{
									character = member.Memories[num2].Actor;
									num = sqrMagnitude;
								}
							}
						}
					}
					if (character != LastPersonChokedMe)
					{
						AddMemory(MemoryPrototype.KnockedOut, character, this, 1f);
						GetOrCreateTarget(character).TakeBlameForAttacks(LastKnockedOutTime);
					}
					Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.WokeUpAngry, Position, GetShoutVoiceRadius(), GetMaxSoundVisibilityRange(), this, character, this, this));
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, character, SpeechSituation.RevivedAngry);
					if (speechForSituation != null)
					{
						Speak(speechForSituation, character);
					}
				}
				LastPersonChokedMe = null;
			}
			else if (!Zombie && this is Human && IsAuthoritative() && canSpeak)
			{
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(this, null, SpeechSituation.Revived);
				if (speechForSituation2 != null)
				{
					Speak(speechForSituation2);
				}
			}
			if (!HasUnbandagedInjury(0))
			{
				DangerousToRescueMeTime = Target.Never;
				DangerousToRescueMeBecause = null;
			}
		}
		Consciousness consciousness2 = Consciousness;
		Consciousness = consciousness;
		if (IsAuthoritative())
		{
			IconIndex = -1;
		}
		if (Consciousness == Consciousness.Unconscious)
		{
			LastKnockedOutTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		}
		else if (Consciousness == Consciousness.Conscious && consciousness2 == Consciousness.Unconscious)
		{
			LastRevivedTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		}
		if (IsAuthoritative())
		{
			if (IsOutdoors())
			{
				UpdateCachedAStarInfo(force: true);
			}
			StoryManager.Instance.SetConditionsDirty();
			if (VisibleTilesCache != null && Alive)
			{
				GameTerrain.Instance.FogOfWar.MarkVisibleTilesCacheDirty(this);
			}
			if (Community != null)
			{
				if (Consciousness == Consciousness.Unconscious)
				{
					Community.OnMemberKnockedUnconscious(this);
				}
				else if (Consciousness == Consciousness.Conscious && consciousness2 == Consciousness.Unconscious)
				{
					Community.OnMemberRevived(this);
				}
			}
		}
		if (!IsOutdoors())
		{
			return;
		}
		if (Consciousness != Consciousness.Conscious)
		{
			if (!IsRagdoll() && !IsProne())
			{
				if (Session.Instance.Editor)
				{
					UnityPlayInitialAnim();
				}
				else
				{
					Ragdollify(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: true);
				}
			}
		}
		else
		{
			if (!IsRagdoll() && !IsProne())
			{
				return;
			}
			if (CarriedBy != null)
			{
				if (IsAuthoritative() == CarriedBy.IsAuthoritative())
				{
					CarriedBy.Drop();
				}
			}
			else
			{
				RecoverFromRagdoll(CurrentAnimState == AnimState.Prone_Back);
			}
		}
	}

	public bool OnStoleSomething(Community stoleSomethingFromCommunity, Character stoleSomethingFromCharacter, float value, bool seenByThiefCommunity = true)
	{
		if (Community != null && stoleSomethingFromCommunity != null && stoleSomethingFromCommunity != Community && stoleSomethingFromCommunity.HasAnyLivingNonZombieMembers() && !stoleSomethingFromCommunity.IsAnimalCommunity() && (!IsAlly(stoleSomethingFromCommunity) || IsControllableByPlayer()) && (!CanFollowPlayer || stoleSomethingFromCommunity.CommunityType != CommunityType.Player))
		{
			Character closestMember = null;
			bool flag = stoleSomethingFromCommunity.IsCharacterVisibleToAnyMember(this, out closestMember, notIncludingCaptives: true);
			if (!flag && (stoleSomethingFromCommunity.GetRelationship(Community) == CommunityRelationshipType.Allied || (stoleSomethingFromCharacter != null && stoleSomethingFromCharacter.CanFollowPlayer && IsControllableByPlayer())))
			{
				closestMember = stoleSomethingFromCommunity.GetNearestLivingNonZombieMember(Tile, null, BaseObjectType.Human, float.MaxValue);
				flag = flag || closestMember != null;
			}
			if (!flag && InsideBuilding != null && InsideBuilding.Community == stoleSomethingFromCommunity)
			{
				closestMember = stoleSomethingFromCommunity.GetNearestLivingNonZombieMember(Tile, null, BaseObjectType.Human, MathUtil.Squared(64f));
				flag = closestMember != null;
			}
			float quantityFactor = Math.Max(1f, value * 0.01f);
			if (flag)
			{
				Memory.OnMemorableEvent(MemoryPrototype.StoleFrom, this, (stoleSomethingFromCharacter != null) ? ((BaseObject)stoleSomethingFromCharacter) : ((BaseObject)stoleSomethingFromCommunity), quantityFactor, secret: false, closestMember);
				bool flag2 = Session.Instance.CommunityManager.GetRelationship(Community, stoleSomethingFromCommunity) != CommunityRelationshipType.Allied && stoleSomethingFromCommunity.CommunityType != CommunityType.Player;
				if (stoleSomethingFromCharacter != null && stoleSomethingFromCharacter.CanFollowPlayer && IsControllableByPlayer())
				{
					flag2 = false;
				}
				if (flag2 && closestMember.HasMemoryOfAnyoneInCommunityAfter(MemoryPrototype.Surrendered, stoleSomethingFromCommunity, Community, Session.Instance.PlayTime - TimeSpan.FromSeconds(Sun.DayLengthSecs * 21f)))
				{
					flag2 = false;
				}
				if (closestMember.CurrentActionAnim == ActionAnim.HandsUp)
				{
					flag2 = false;
				}
				if (closestMember.InsideBuilding != null)
				{
					closestMember.InsideBuilding.OnCharacterLeave(closestMember, closestMember.InsideBuilding.GetClosestEntranceTo(Tile), fromBuildingDestroyed: false, fromRagdolled: false);
				}
				if (flag2)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(closestMember, this, SpeechSituation.StopThief);
					if (speechForSituation != null)
					{
						closestMember.Speak(speechForSituation);
					}
					Session.Instance.CommunityManager.SetRelationship(Community, stoleSomethingFromCommunity, CommunityRelationshipType.Hostile);
				}
				else
				{
					Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(closestMember, this, SpeechSituation.StopThiefSurrendered);
					if (speechForSituation2 != null)
					{
						closestMember.Speak(speechForSituation2);
					}
					closestMember.SetRecentActivity(RecentActivityType.Conversation, this);
				}
			}
			else if (seenByThiefCommunity)
			{
				Memory.OnMemorableEvent(MemoryPrototype.StoleFrom, this, (stoleSomethingFromCharacter != null) ? ((BaseObject)stoleSomethingFromCharacter) : ((BaseObject)stoleSomethingFromCommunity), quantityFactor, SecrecyMode.IgnoredByObjectCommunity);
			}
			return flag;
		}
		return false;
	}

	public void OnPickpocketedFrom(Character pickpocket, Equipment item, bool taken, float noticeability)
	{
		PickpocketDetection += noticeability;
		if (taken)
		{
			GatheredItem gatheredItem = GatheredItem.CreateIncludingLiquidAmount(item);
			bool flag = false;
			if (PickpocketedItems == null)
			{
				PickpocketedItems = new List<GatheredItem>();
			}
			for (int i = 0; i < PickpocketedItems.Count; i++)
			{
				if (PickpocketedItems[i].Matches(gatheredItem))
				{
					PickpocketedItems[i] = PickpocketedItems[i].Combine(gatheredItem);
					flag = true;
				}
			}
			if (!flag)
			{
				PickpocketedItems.Add(gatheredItem);
			}
		}
		if (PickpocketDetection >= TakePage.MaxPickpocketDetection)
		{
			GetOrCreateTarget(pickpocket).ForceVisible(this);
			pickpocket.OnStoleSomething(Community, this, item.GetBasePrice());
		}
		else if (taken)
		{
			pickpocket.Skillset.AddProgress(pickpocket, SkillType.Stealth, noticeability * 0.1f);
		}
	}

	public void AssignBlameForPickpocketedItem(GatheredItem pickpocketedItem, Character suspect)
	{
		if (PickpocketedItems == null)
		{
			return;
		}
		for (int i = 0; i < PickpocketedItems.Count; i++)
		{
			if (PickpocketedItems[i].Matches(pickpocketedItem))
			{
				float num = ((PickpocketedItems[i].Liquid != null) ? PickpocketedItems[i].Liquid.BasePricePerFlOz : PickpocketedItems[i].Type.BasePrice);
				float quantityFactor = Math.Max(1f, (float)PickpocketedItems[i].Amount * num);
				Memory.OnMemorableEvent(MemoryPrototype.TheftSuspect, suspect, this, quantityFactor, secret: false);
				PickpocketedItems.RemoveAt(i);
				if (PickpocketedItems.Count == 0)
				{
					PickpocketedItems = null;
				}
				break;
			}
		}
	}

	public virtual bool CanOpenGates()
	{
		return false;
	}

	public bool WantToAvoidCommunity(Community community)
	{
		if (community == null || Community == null || community == Community)
		{
			return false;
		}
		if (Session.Instance.CommunityManager.GetRelationship(Community, community) == CommunityRelationshipType.Hostile)
		{
			return community.HasAnyLivingNonZombieMembers();
		}
		return false;
	}

	public virtual float GetNutritionFactor()
	{
		return 1f;
	}

	public void Eat(Equipment food, bool playSound, bool fromInfoScreen, Character fedBy)
	{
		if (food.GetLiquidCapacity() > 0f)
		{
			ConsumeLiquid(food, playSound, fromInfoScreen);
			return;
		}
		float num = food.GetPrototype().GetNutrition() * GetNutritionFactor();
		float num2 = food.GetPrototype().GetWater() * GetNutritionFactor();
		float num3 = food.GetPrototype().GetCaffeine() * GetNutritionFactor();
		float num4 = food.GetPrototype().Alcohol * GetNutritionFactor();
		float num5 = num4 / CalcStandardDrinksToBACFactor();
		Hunger = Math.Max(0f, Hunger - num);
		Thirst = Math.Max(0f, Thirst - num2);
		SleepDeprivation = Math.Max(0f, SleepDeprivation - num3);
		Toilet += num + Mathf.Abs(num3);
		BloodAlcoholConcentration = Math.Max(0f, BloodAlcoholConcentration + num4);
		if (playSound)
		{
			if (fromInfoScreen)
			{
				SoundManager.PlayMenuSoundFromList((Math.Abs(num) >= Math.Abs(num2)) ? SoundManager.EatSounds : SoundManager.DrinkSounds);
			}
			else
			{
				PlaySoundUsingFootstepAudioSourceFromList((Math.Abs(num) >= Math.Abs(num2)) ? SoundManager.EatSounds : SoundManager.DrinkSounds, 1f);
			}
		}
		if (food.InfectedWith != InfectionType.None)
		{
			SwallowInfectedFood(food.InfectedWith);
		}
		float num6 = CalcTastiness(food.GetPrototype()) / 100f;
		if (num6 != 0f)
		{
			if (num4 > 0f)
			{
				AddMemory((num6 >= 0f) ? MemoryPrototype.DrankTastyAlcohol : MemoryPrototype.DrankDisgustingAlcohol, this, null, Mathf.Abs(num6) * num5);
			}
			else if (Math.Abs(num2) > Math.Abs(num))
			{
				AddMemory((num6 >= 0f) ? MemoryPrototype.DrankTastyDrink : MemoryPrototype.DrankDisgustingDrink, this, null, Mathf.Abs(num6) * num2 / Sun.DayLengthSecs);
			}
			else
			{
				AddMemory((num6 >= 0f) ? MemoryPrototype.AteTastyFood : MemoryPrototype.AteDisgustingFood, this, null, Mathf.Abs(num6) * num / Sun.DayLengthSecs);
			}
		}
		if (food.GetPrototype().ContainsHumanMeat)
		{
			if (fedBy != null)
			{
				Memory.OnMemorableEvent(MemoryPrototype.FedHumanMeatTo, fedBy, this, 1f, secret: false);
			}
			else
			{
				Memory.OnMemorableEvent(MemoryPrototype.Cannibal, this, null, 1f, secret: false);
			}
		}
		if (num4 > 0f)
		{
			LastDrankAlcoholTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		}
		if (food.GetPrototype().SkillOnConsumptionType != SkillType.Invalid)
		{
			Skillset.AddProgress(this, food.GetPrototype().SkillOnConsumptionType, food.GetPrototype().SkillOnConsumptionProgression);
		}
		if (IsControllableByPlayer())
		{
			food.GetPrototype().MarkDiscovered();
		}
		StoryManager.Instance.SetConditionsDirty();
	}

	public float ConsumeLiquid(Equipment container, bool playSound, bool fromInfoScreen, float limitAmount = float.MaxValue)
	{
		LiquidPrototype liquidContentsType = container.GetLiquidContentsType();
		if (liquidContentsType == null)
		{
			return 0f;
		}
		float val = 0f;
		if (liquidContentsType.WaterContent > 0f)
		{
			val = Math.Max(val, GetThirstInFlOz() / (liquidContentsType.WaterContent / 100f));
		}
		if (liquidContentsType.GetNutritionPerFlOz() > 0f)
		{
			val = Math.Max(val, GetHunger() / liquidContentsType.GetNutritionPerFlOz());
		}
		if (liquidContentsType.AlcoholContent > 0f)
		{
			val = StandardDrinkAlcoholFlOz / (liquidContentsType.AlcoholContent / 100f);
		}
		val = Math.Min(val, limitAmount);
		if (val == 0f)
		{
			return 0f;
		}
		InfectionType infectedWith = container.InfectedWith;
		float num = container.DrainLiquid(val);
		ConsumeLiquid(liquidContentsType, num, playSound, fromInfoScreen, infectedWith);
		return num;
	}

	private float CalcStandardDrinksToBACFactor()
	{
		float weightInKg = GetWeightInKg();
		float num = ((Appearance.Gender == GenderType.Male) ? 0.58f : 0.49f);
		return 0.96720004f / (num * weightInKg);
	}

	public void ConsumeLiquid(LiquidPrototype liquid, float amount, bool playSound, bool fromInfoScreen, InfectionType infectedWith)
	{
		amount *= GetNutritionFactor();
		float num = amount * liquid.WaterContent / 100f * Sun.DayLengthSecs / WaterNeededPerDayInFlOz;
		float num2 = amount * liquid.GetNutritionPerFlOz();
		float num3 = amount * liquid.GetCaffeinePerFlOz();
		float num4 = amount * liquid.AlcoholContent / 100f;
		float num5 = num4 / StandardDrinkAlcoholFlOz;
		float num6 = num5 * CalcStandardDrinksToBACFactor();
		Thirst = Math.Max(0f, Thirst - num);
		Hunger = Math.Max(0f, Hunger - num2);
		SleepDeprivation = Math.Max(0f, SleepDeprivation - num3);
		Toilet += num2 + Math.Max(num, 0f) + num4 + Mathf.Abs(num3);
		BloodAlcoholConcentration = Math.Max(0f, BloodAlcoholConcentration + num6);
		if (infectedWith != InfectionType.None && amount > 0f)
		{
			SwallowInfectedFood(infectedWith);
		}
		float num7 = CalcTastiness(liquid) / 100f;
		if (num7 != 0f)
		{
			if (liquid.AlcoholContent > 0f)
			{
				AddMemory((num7 >= 0f) ? MemoryPrototype.DrankTastyAlcohol : MemoryPrototype.DrankDisgustingAlcohol, this, null, Mathf.Abs(num7) * num5);
			}
			else if (liquid.WaterContent > 0f)
			{
				AddMemory((num7 >= 0f) ? MemoryPrototype.DrankTastyDrink : MemoryPrototype.DrankDisgustingDrink, this, null, Mathf.Abs(num7) * num / Sun.DayLengthSecs);
			}
			else
			{
				AddMemory((num7 >= 0f) ? MemoryPrototype.AteTastyFood : MemoryPrototype.AteDisgustingFood, this, null, Mathf.Abs(num7) * num2 / Sun.DayLengthSecs);
			}
		}
		if (liquid.SkillOnConsumptionType != SkillType.Invalid)
		{
			Skillset.AddProgress(this, liquid.SkillOnConsumptionType, liquid.SkillOnConsumptionProgressionPerFlOz * amount);
		}
		if (liquid.AlcoholContent > 0f)
		{
			LastDrankAlcoholTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		}
		if (playSound)
		{
			if (fromInfoScreen)
			{
				SoundManager.PlayMenuSoundFromList((liquid.NutritionPerFlOz > 0f) ? SoundManager.EatSounds : SoundManager.DrinkSounds);
			}
			else
			{
				PlaySoundUsingFootstepAudioSourceFromList((liquid.NutritionPerFlOz > 0f) ? SoundManager.EatSounds : SoundManager.DrinkSounds, 1f);
			}
		}
		StoryManager.Instance.SetConditionsDirty();
		if (IsControllableByPlayer())
		{
			liquid.MarkDiscovered();
		}
	}

	public bool WantDrinkAlcohol()
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		bool flag = HasPersonality(CachedPersonalityType.Alcoholic);
		if (currentTime - LastDrankAlcoholTime >= TimeSpan.FromSeconds(flag ? 10f : 30f))
		{
			return GetBloodAlcoholConcentration() < (flag ? BACStupor : BACExcitement);
		}
		return false;
	}

	public float CalcTastiness(EquipmentPrototype proto)
	{
		float num = proto.Tastiness;
		if (proto.Spicy)
		{
			if (HasPersonality(CachedPersonalityType.LikesSpicyFood))
			{
				num = Math.Min(num + 50f, 100f);
			}
			else if (HasPersonality(CachedPersonalityType.LikesBlandFood))
			{
				num = Math.Max(num - 50f, -100f);
			}
		}
		return num;
	}

	public float CalcTastiness(LiquidPrototype liquid)
	{
		float num = liquid.Tastiness;
		if (liquid.Spicy)
		{
			if (HasPersonality(CachedPersonalityType.LikesSpicyFood))
			{
				num = Math.Min(num + 50f, 100f);
			}
			else if (HasPersonality(CachedPersonalityType.LikesBlandFood))
			{
				num = Math.Max(num - 50f, -100f);
			}
		}
		return num;
	}

	public float CalcTastiness(Equipment item)
	{
		if (item.GetLiquidContentsType() == null)
		{
			return CalcTastiness(item.GetPrototype());
		}
		return CalcTastiness(item.GetLiquidContentsType());
	}

	public void ApplyBandage(Character applier, Equipment bandage, bool playSound, bool callingCodeSupportsPrediction)
	{
		int bandageLevel = bandage.GetBandageLevel();
		InfectionType infectedWith = bandage.InfectedWith;
		ApplyBandage(applier, playSound, bandageLevel, infectedWith, callingCodeSupportsPrediction);
	}

	public void ApplyBandage(Character applier, bool playSound, int bandageLevel, InfectionType infectedWith, bool callingCodeSupportsPrediction)
	{
		int num = Math.Min(applier.GetSkillLevelWithEffects(SkillType.Medicine), bandageLevel);
		InjuryLocation injuryLocation = InjuryLocation.Count;
		int num2 = num;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (!Injuries[i].AbsorbedByVest && Injuries[i].BandagedSkillLevel < num2)
			{
				injuryLocation = Injuries[i].Location;
				num2 = Injuries[i].BandagedSkillLevel;
			}
		}
		if (injuryLocation != InjuryLocation.Count)
		{
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			float num3 = 0f;
			int num4 = -1;
			for (int j = 0; j < Injuries.Count; j++)
			{
				if (Injuries[j].Location == injuryLocation && Injuries[j].BandagedSkillLevel < num && !Injuries[j].AbsorbedByVest)
				{
					if ((Injuries[j].Burning || Injuries[j].ShouldBleed()) && Injuries[j].Attacker != null && !Injuries[j].Attacker.Zombie && !Injuries[j].Bandaged)
					{
						float num5 = (float)(currentTime - Injuries[j].InjuryTime).TotalSeconds;
						float num6 = (Injuries[j].Burning ? BurningDamageRate : BloodLossRate);
						num3 += num5 * num6;
						num4 = j;
					}
					Injuries[j] = Injuries[j].ApplyBandage(num, currentTime, infectedWith);
				}
			}
			if (num3 > 0f && num4 != -1 && !Zombie && IsAuthoritative())
			{
				int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Constitution);
				num3 *= Mathf.Lerp(BloodLossFactorMinConstitution, BloodLossFactorMaxConstitution, (float)skillLevelWithEffects / 5f);
				TriggerHurtMemory(Injuries[num4].Attacker, Injuries[num4].AssailantUnknown, num3, Injuries[num4].Secrecy, Injuries[num4].FromSparring, Injuries[num4].Intentional);
			}
			GetPredictedOrElseThisCharacter().UnityUpdateDecals();
			GetPredictedOrElseThisCharacter().UnityUpdateInjuries();
			if (playSound && (!callingCodeSupportsPrediction || CheckFrontmostPrediction(PredictedEventType.BandageSound)))
			{
				GetPredictedOrElseThisCharacter().PlaySoundOneShotFromList(SoundManager.BandageSounds);
			}
			if (!applier.IsPredicted())
			{
				applier.Skillset.AddProgress(applier, SkillType.Medicine, 10f);
			}
		}
		if (!HasUnbandagedInjury(0))
		{
			InjuryRemarkedOn = false;
			Adrenaline = 0f;
			if (Consciousness < Consciousness.Unconscious)
			{
				DangerousToRescueMeTime = Target.Never;
				DangerousToRescueMeBecause = null;
			}
			if (GetAuthoritativeOrElseThis() == Hud.Instance.LocalControlledCharacter)
			{
				Hud.Instance.VignetteAmount = 0f;
			}
		}
		if (IsControllableByPlayer() && playSound)
		{
			if (bandageLevel > 0)
			{
				HintManager.Instance.Hints[15].MarkPerformed();
			}
			if (Consciousness == Consciousness.Unconscious)
			{
				HintManager.Instance.Hints[23].MarkPerformed();
				if (bandageLevel > 0)
				{
					HintManager.Instance.Hints[24].MarkPerformed();
				}
			}
		}
		if (IsAuthoritative())
		{
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	private bool ApplyAntigen(InfectionType infectionType)
	{
		bool flag = false;
		bool result = false;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].InfectionType == infectionType)
			{
				Injuries[i] = Injuries[i].ApplyAntigen(infectionType);
				result = true;
			}
			else if (Injuries[i].InfectionType != InfectionType.None)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			InfectionProgression = 0f;
			InfectionRemarkedOn = false;
		}
		return result;
	}

	public void InjectAntigen(Character injector, InfectionType infectionType, InfectionType infectedWith, Equipment syringe, bool callingCodeSupportsPrediction)
	{
		if (syringe != null && syringe.IsFake())
		{
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.UsedFakeAntigen, injector, syringe);
			return;
		}
		if (ApplyAntigen(infectionType))
		{
			injector.Skillset.AddProgress(injector, SkillType.Medicine, 10f);
		}
		if (IsAuthoritative() && Predicted != null)
		{
			Predicted.ApplyAntigen(infectionType);
		}
		if (infectedWith != InfectionType.None)
		{
			Zombify(infectedWith);
		}
		if (GetPredictedOrElseThisCharacter().HasUnityObject())
		{
			GetPredictedOrElseThisCharacter().UnityUpdateSkinColor();
		}
		if (IsAuthoritative())
		{
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public float GetPriceToSell(Equipment item, Character buyer)
	{
		float basePrice = item.GetBasePrice();
		return GetPriceToSell(basePrice, buyer);
	}

	public float GetPriceToSell(EquipmentPrototype proto, Character buyer)
	{
		return GetPriceToSell(proto.BasePrice, buyer);
	}

	public float GetPriceToSell(float basePrice, Character buyer)
	{
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(buyer, out approval, out respect);
		float num = Mathf.Clamp(BaseMarkup - approval / 100f * 0.5f - respect / 100f * 0.5f, 1f, 2f);
		return basePrice * num;
	}

	public float GetPriceToBuy(Equipment item, Character seller)
	{
		float basePrice = item.GetBasePrice();
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(seller, out approval, out respect);
		float num = Mathf.Clamp(0.5f + approval / 100f * 0.5f + respect / 100f * 0.5f, 0f, 1f);
		return basePrice * num;
	}

	public float GetInventoryItemPriceAdjustedForTrade(Equipment item, Character tradingWith, bool fromTradingScreen)
	{
		if (tradingWith != null)
		{
			if (fromTradingScreen && IsControllableByPlayer())
			{
				return tradingWith.GetPriceToBuy(item, this);
			}
			return GetPriceToSell(item, tradingWith);
		}
		return item.GetBasePrice();
	}

	public virtual void LinkMuscleToStrengthSkill()
	{
	}

	public virtual void SetGender(GenderType gender)
	{
		Appearance.Gender = gender;
		UnityOnChangedBones();
	}

	public virtual void SetAge(float age)
	{
		Appearance.Age = age;
		UnityOnChangedAppearance();
	}

	public void SetFirstName(string name)
	{
		if (Id != 0)
		{
			Session.Instance.CharacterManager.OnFirstNameAdded(FirstName);
		}
		FirstName = name;
		if (Id != 0)
		{
			Session.Instance.CharacterManager.OnFirstNameRemoved(FirstName);
		}
	}

	public void RandomizeName(CustomRandom rand)
	{
		string firstName = GameImpl.Instance.PickRandomName((Appearance.Gender == GenderType.Female) ? NameType.FemaleFirstName : NameType.MaleFirstName, rand);
		if (Id != 0)
		{
			for (int i = 0; i < 10; i++)
			{
				if (!Session.Instance.CharacterManager.DoesAnyCharacterHaveFirstName(firstName))
				{
					break;
				}
				firstName = GameImpl.Instance.PickRandomName((Appearance.Gender == GenderType.Female) ? NameType.FemaleFirstName : NameType.MaleFirstName, rand);
			}
		}
		SetFirstName(firstName);
		FirstNameVerified = StringStatus.Verified;
		if (GetBaseObjectType() == BaseObjectType.Human)
		{
			Surname = GameImpl.Instance.PickRandomName(NameType.Surname, rand);
			SurnameVerified = StringStatus.Verified;
		}
	}

	private void AddClothingToList(EquipmentPrototype proto, List<EquipmentPrototype> newClothes)
	{
		if (proto == null || !CanWear(proto))
		{
			return;
		}
		foreach (EquipmentPrototype newClothe in newClothes)
		{
			if (!proto.IsClothingCompatible(newClothe))
			{
				return;
			}
		}
		newClothes.Add(proto);
	}

	public void RandomizeClothing(CustomRandom rand, bool seasonallyAppropriate)
	{
		RandomizeClothing(rand, seasonallyAppropriate, isPortrait: false, mustHaveBackpack: false, mustHaveBodyArmor: false, mustHaveHelmet: false, mustHaveLegArmor: false);
	}

	public void RandomizeClothing(CustomRandom rand, bool seasonallyAppropriate, bool isPortrait, bool mustHaveBackpack, bool mustHaveBodyArmor, bool mustHaveHelmet, bool mustHaveLegArmor)
	{
		GameImpl instance = GameImpl.Instance;
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null)
			{
				Clothes[i].TransferFrom(this, int.MaxValue, triggeredByLocalPlayer: false).Delete();
			}
		}
		string lootLocation = GetLootLocation();
		EquipmentPrototype proto = instance.PickRandomClothing(lootLocation, ClothingType.Shoes, mustPickSomething: true, rand);
		EquipmentPrototype proto2 = instance.PickRandomClothing(lootLocation, ClothingType.Bottom, mustPickSomething: true, rand);
		EquipmentPrototype proto3 = instance.PickRandomClothing(lootLocation, ClothingType.Top, mustPickSomething: true, rand);
		EquipmentPrototype proto4 = instance.PickRandomClothing(lootLocation, ClothingType.Hat, mustHaveHelmet, mustHaveHelmet ? typeof(Armor) : null, rand);
		EquipmentPrototype proto5 = instance.PickRandomClothing(lootLocation, ClothingType.Glasses, mustPickSomething: false, rand);
		EquipmentPrototype proto6 = instance.PickRandomClothing(lootLocation, ClothingType.BodyArmor, mustHaveBodyArmor, rand);
		EquipmentPrototype proto7 = instance.PickRandomClothing(lootLocation, ClothingType.LegArmor, mustHaveLegArmor, rand);
		EquipmentPrototype equipmentPrototype = null;
		if (HasRole(Role.Trader) && mustHaveBackpack)
		{
			equipmentPrototype = instance.PickLargeBackpack(rand);
		}
		if (equipmentPrototype == null && !isPortrait)
		{
			equipmentPrototype = instance.PickRandomClothing(lootLocation, ClothingType.Backpack, mustHaveBackpack, rand);
		}
		if (Zombie && rand.RandomChoice(0.1f))
		{
			proto = null;
		}
		if (Zombie && rand.RandomChoice(0.1f))
		{
			proto2 = null;
		}
		if (Zombie && rand.RandomChoice(0.1f))
		{
			proto3 = null;
		}
		if (!mustHaveHelmet && (Zombie || rand.RandomChoice(0.5f)))
		{
			proto4 = null;
		}
		if (rand.RandomChoice(0.9f))
		{
			proto5 = null;
		}
		List<EquipmentPrototype> list = new List<EquipmentPrototype>();
		AddClothingToList(proto, list);
		AddClothingToList(proto2, list);
		AddClothingToList(proto3, list);
		AddClothingToList(proto4, list);
		AddClothingToList(proto5, list);
		AddClothingToList(equipmentPrototype, list);
		AddClothingToList(proto6, list);
		AddClothingToList(proto7, list);
		if (seasonallyAppropriate)
		{
			float outsideTemp = Session.Instance.Weather.CalcAverageTemperatureInCelsius();
			int num = CalcRequiredInsulationForTemperature(outsideTemp);
			int num2 = 0;
			while (num2 < 100)
			{
				num2++;
				int num3 = 0;
				foreach (EquipmentPrototype item in list)
				{
					num3 += item.Insulation;
				}
				if (num3 == num || Mathf.Abs(CalcAverageTemperatureInsulationDelta(outsideTemp, num3)) < ClothingGoal.MinAverageTempDeltaToChangeClothes * 0.5f)
				{
					break;
				}
				ClothingType clothingType = ClothingType.Invalid;
				switch (rand.Next(mustHaveHelmet ? 3 : 4))
				{
				case 0:
					clothingType = ClothingType.Shoes;
					break;
				case 1:
					clothingType = ClothingType.Bottom;
					break;
				case 2:
					clothingType = ClothingType.Top;
					break;
				case 3:
					clothingType = ClothingType.Hat;
					break;
				}
				EquipmentPrototype equipmentPrototype2 = instance.PickRandomClothing(lootLocation, clothingType, mustPickSomething: true, rand);
				if (clothingType == ClothingType.Hat && rand.RandomChoice(0.5f))
				{
					equipmentPrototype2 = null;
				}
				EquipmentPrototype equipmentPrototype3 = null;
				foreach (EquipmentPrototype item2 in list)
				{
					if (item2.ClothingType == clothingType)
					{
						equipmentPrototype3 = item2;
						break;
					}
				}
				int num4 = equipmentPrototype3?.Insulation ?? 0;
				int num5 = equipmentPrototype2?.Insulation ?? 0;
				if ((num3 >= num || num5 > num4) && (num3 <= num || num5 < num4))
				{
					if (equipmentPrototype3 != null)
					{
						list.Remove(equipmentPrototype3);
					}
					AddClothingToList(equipmentPrototype2, list);
				}
			}
		}
		if (isPortrait)
		{
			foreach (EquipmentPrototype item3 in list)
			{
				Clothes[(int)item3.ClothingType] = Equipment.Create(item3);
				Clothes[(int)item3.ClothingType].RandomiseVariations(rand);
			}
			return;
		}
		foreach (EquipmentPrototype item4 in list)
		{
			Clothes[(int)item4.ClothingType] = Inventory.Add(this, Equipment.SpawnRandomVariation(item4, rand));
		}
		UnityOnChangedAppearance();
	}

	public virtual bool CanWear(EquipmentPrototype proto)
	{
		return false;
	}

	public virtual float GetPregnancyProgression()
	{
		return 0f;
	}

	public virtual bool IsPregnant()
	{
		return false;
	}

	public void OnEnterBuilding(Building building, bool wasOrderedInsideBuilding)
	{
		if (InteractionObject != null || CurrentActionAnim == ActionAnim.HandsUp)
		{
			ClearActionAnim();
		}
		HaltMovement();
		if (VelocityXZ.sqrMagnitude > 0f)
		{
			ClearVelocityXZ();
		}
		InsideBuilding = building;
		InsideBuildingSlotIndex = building.GetInhabitantIndex(this);
		WasOrderedInsideBuilding = wasOrderedInsideBuilding;
		LastEnteredOrExitedBuildingTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (!building.GetInhabitantSlotDefs()[InsideBuildingSlotIndex].External)
		{
			RemoveFromTerrain();
		}
		SetPosition(building.GetSlotPos(InsideBuildingSlotIndex));
		SetFacingAngle(building.GetSlotAngleRad(InsideBuildingSlotIndex));
		if (Consciousness == Consciousness.Conscious && CanSleep())
		{
			SetConsciousness(Consciousness.Sleeping);
		}
		if (InfoScreen.Instance.IsShowingInventoryFor(building) || InfoScreen.Instance.IsShowingInventoryFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		if (Hud.Instance.LocalControlledCharacter == this && !Session.Instance.GameCamera.FlyCam)
		{
			Session.Instance.GameCamera.WantSnapZoom = true;
		}
		RemoveFailedFindAttempt(building);
	}

	public TerrainCoord GetBuildingExitPos(int entranceIndex)
	{
		GameTerrain instance = GameTerrain.Instance;
		Vector3 entrancePos = InsideBuilding.GetEntrancePos(entranceIndex);
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(entrancePos);
		if (instance.IsImpassable(tileCoordForPos.x, tileCoordForPos.y, 2051, this, InsideBuilding))
		{
			int num = 1;
			TerrainCoord bestTile = TerrainCoord.Invalid;
			do
			{
				TerrainCoord terrainCoord = InsideBuilding.MinTile - new TerrainCoord(num, num);
				TerrainCoord terrainCoord2 = InsideBuilding.MaxTile + new TerrainCoord(num, num);
				float bestDistSq = float.MaxValue;
				for (int i = terrainCoord.x; i <= terrainCoord2.x; i++)
				{
					TestTileForExitPos(tileCoordForPos, new TerrainCoord(i, terrainCoord.y), ref bestTile, ref bestDistSq);
				}
				for (int j = terrainCoord.x; j <= terrainCoord2.x; j++)
				{
					TestTileForExitPos(tileCoordForPos, new TerrainCoord(j, terrainCoord2.y), ref bestTile, ref bestDistSq);
				}
				for (int k = terrainCoord.y; k <= terrainCoord2.y; k++)
				{
					TestTileForExitPos(tileCoordForPos, new TerrainCoord(terrainCoord.x, k), ref bestTile, ref bestDistSq);
				}
				for (int l = terrainCoord.y; l <= terrainCoord2.y; l++)
				{
					TestTileForExitPos(tileCoordForPos, new TerrainCoord(terrainCoord2.x, l), ref bestTile, ref bestDistSq);
				}
				num++;
			}
			while (bestTile == TerrainCoord.Invalid);
			return bestTile;
		}
		return tileCoordForPos;
	}

	public void OnLeaveBuilding(int entranceIndex, bool fromBuildingDestroyed, bool fromRagdolled)
	{
		bool inTerrain = InTerrain;
		if (IsAuthoritative())
		{
			if (InfoScreen.Instance.IsShowingInventoryFor(InsideBuilding) || InfoScreen.Instance.IsShowingInventoryFor(this))
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
			if (!fromRagdolled && !fromBuildingDestroyed && Util.AmIOnMainThread())
			{
				if (InsideBuilding is EnterableVehicle)
				{
					SoundManager.PlaySound3D(SoundManager.ExitVehicleSound, Pos, 0.5f);
				}
				else if (!InsideBuilding.GetInhabitantSlotDef(GetAuthoritativeOrElseThisCharacter()).External && !(InsideBuilding is TiltedBuilding))
				{
					SoundManager.PlaySound3D(SoundManager.ExitBuildingSound, Pos, 0.1f);
				}
			}
			if (Hud.Instance.LocalControlledCharacter == this && InfoScreen.Instance.CurrentObject == InsideBuilding)
			{
				InfoScreen.Instance.CloseInfoScreen();
			}
		}
		Vector3 pos = InsideBuilding.GetEntrancePos(entranceIndex);
		float facingAngle = InsideBuilding.GetEntranceAngle(entranceIndex);
		if (!fromRagdolled)
		{
			GameTerrain instance = GameTerrain.Instance;
			bool flag = false;
			if (fromBuildingDestroyed)
			{
				TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
				for (int i = 0; i < 10; i++)
				{
					Bounds boundingBox = InsideBuilding.GetBoundingBox();
					float x = boundingBox.min.x + (boundingBox.max.x - boundingBox.min.x) * MathUtil.RandomFloat((float)(i * 1000) + (float)currentTime.TotalSeconds + (float)Id * 0.54645f);
					float z = boundingBox.min.z + (boundingBox.max.z - boundingBox.min.z) * MathUtil.RandomFloat((float)(i * 2000) + (float)currentTime.TotalSeconds + (float)Id * 0.2138f);
					float tileHeightAtPos = instance.GetTileHeightAtPos(x, z);
					pos = new Vector3(x, tileHeightAtPos, z);
					facingAngle = MathUtil.RandomFloat((float)(i * 3000) + (float)currentTime.TotalSeconds + (float)Id * 0.92673f) * (MathF.PI * 2f);
					TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(pos);
					if (!instance.IsImpassable(tileCoordForPos.x, tileCoordForPos.y, 2051, this, InsideBuilding))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				TerrainCoord buildingExitPos = GetBuildingExitPos(entranceIndex);
				pos = instance.GetTileCentrePos(buildingExitPos);
			}
		}
		if (IsAuthoritative() && !InsideBuilding.GetInhabitantSlotDef(this).External)
		{
			AddToTerrain();
		}
		InsideBuilding = null;
		InsideBuildingSlotIndex = -1;
		WasOrderedInsideBuilding = false;
		LastEnteredOrExitedBuildingTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (Consciousness == Consciousness.Sleeping)
		{
			SetConsciousness(Consciousness.Conscious);
		}
		if (fromRagdolled)
		{
			return;
		}
		SetPosition(pos.x, pos.z);
		SetFacingAngle(facingAngle);
		if (Consciousness != Consciousness.Conscious && !IsRagdoll())
		{
			if (!inTerrain && IsAuthoritative())
			{
				UnityDelete();
				UnityInit();
				UnityActivate();
				Unity.Animator.Play(AnimHash.Idle, 0);
				Unity.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
				Unity.Animator.Update(1f);
				Unity.Animator.updateMode = AnimatorUpdateMode.Normal;
			}
			Vector3 hitPos = (IsUnityObjectActive() ? GetUnityBoneTransform(Bone.Spine1).GetPosition() : Vector3.zero);
			Ragdollify(0f, hitPos, base.Forward * ExitBuildingHitForce, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: false);
		}
	}

	private void TestTileForExitPos(TerrainCoord startTile, TerrainCoord tile, ref TerrainCoord bestTile, ref float bestDistSq)
	{
		float distSquared = tile.GetDistSquared(startTile);
		if (distSquared < bestDistSq && !GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, this, InsideBuilding))
		{
			bestDistSq = distSquared;
			bestTile = tile;
		}
	}

	public void OnChangeBuildingSlot(int oldSlot, int newSlot)
	{
		if (InsideBuilding.GetInhabitantSlotDefs()[oldSlot].External)
		{
			RemoveFromTerrain();
		}
		InsideBuildingSlotIndex = newSlot;
		SetPosition(InsideBuilding.GetSlotPos(newSlot));
		SetFacingAngle(InsideBuilding.GetSlotAngleRad(newSlot));
		if (InsideBuilding.GetInhabitantSlotDefs()[newSlot].External)
		{
			AddToTerrain();
		}
	}

	public void StartFade(float target, float time)
	{
		FadeTarget = target;
		FadeTime = time;
	}

	public void SetFade(float fade)
	{
		Fade = (FadeTarget = fade);
		FadeTime = 0f;
	}

	public bool IsFadeComplete()
	{
		return Fade == FadeTarget;
	}

	public float GetFade()
	{
		return Fade;
	}

	public float GetFadeTarget()
	{
		return FadeTarget;
	}

	public float GetFadeTime()
	{
		return FadeTime;
	}

	public Target GetTarget(TileObject obj)
	{
		if (obj == this)
		{
			Debug.LogError("Trying to target self! " + GetDisplayNameString());
			return null;
		}
		if (obj == null)
		{
			return null;
		}
		foreach (Target target in Targets)
		{
			if (target.Object != null && target.Object.GetAuthoritativeOrElseThis() == obj.GetAuthoritativeOrElseThis())
			{
				return target;
			}
		}
		return null;
	}

	public Target GetOrCreateTarget(TileObject obj)
	{
		return GetOrCreateTarget(obj, obj.GetBoundingBoxBottom());
	}

	public Target GetOrCreateTarget(TileObject obj, Vector3 pos)
	{
		foreach (Target target2 in Targets)
		{
			if (target2.Object != null && target2.Object.GetAuthoritativeOrElseThis() == obj.GetAuthoritativeOrElseThis())
			{
				return target2;
			}
		}
		Target target = new Target(obj);
		target.Predicted = IsPredicted();
		target.UpdateLastKnownInfo(this, pos);
		AddTarget(target);
		return target;
	}

	private void AddTarget(Target newTarget)
	{
		Targets.Add(newTarget);
		if (IsAuthoritative() && newTarget.Object is Character character)
		{
			if (character == this)
			{
				Debug.LogWarning(character.GetDisplayNameString() + " targeting self? " + GetDisplayNameString());
			}
			else if (character.CharactersTargetingMe.Contains(this))
			{
				Debug.LogWarning(character.GetDisplayNameString() + "CharactersTargetingMe already contains " + GetDisplayNameString());
			}
			else
			{
				character.CharactersTargetingMe.Add(this);
			}
		}
	}

	private void RemoveTarget(Target target)
	{
		Targets.Remove(target);
		if (IsAuthoritative())
		{
			if (target.Object is Character character)
			{
				character.CharactersTargetingMe.Remove(this);
			}
			CachedMysteriousAttackDirty = true;
		}
	}

	private void RemoveAllTargets()
	{
		while (Targets.Count > 0)
		{
			RemoveTarget(Targets[0]);
		}
	}

	public void RefreshTarget(TileObject obj)
	{
		if (obj != this)
		{
			Target orCreateTarget = GetOrCreateTarget(obj);
			orCreateTarget.ForceVisible(this);
			orCreateTarget.ClearInaccessible();
		}
	}

	private bool AreAnyLivingZombiesNearby()
	{
		foreach (Target target in Targets)
		{
			if (target.Object is Character { Zombie: not false } && !target.GetFlag(TargetFlags.Dead) && MathUtil.ToXZ(target.LastKnownPosition - Pos).magnitude <= 48f)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOutnumberedBy(Community enemyCommunity, bool orEqual, Character include)
	{
		if (enemyCommunity != null)
		{
			int num = 0;
			if (Community != null)
			{
				num += Community.GetConsciousNonZombieMemberCountInRange(Tile, OutnumberedRange, null);
				foreach (Community cachedAlly in Community.CachedAllies)
				{
					num += cachedAlly.GetConsciousNonZombieMemberCountInRange(Tile, OutnumberedRange, null);
				}
			}
			else
			{
				num++;
			}
			int num2 = 0;
			foreach (Target target in Targets)
			{
				if (target.Object is Character character && (character.Community == enemyCommunity || enemyCommunity.CachedAllies.Contains(character.Community)) && target.TimeSinceLastDetected < TimeSpan.FromSeconds(120.0) && !target.HasAnyFlag((TargetFlags)8390656))
				{
					if (character == include)
					{
						include = null;
					}
					num2++;
				}
			}
			if (include != null && include.IsConscious)
			{
				num2++;
			}
			if (num >= num2)
			{
				if (orEqual)
				{
					return num == num2;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void MarkVisibleDeterministic()
	{
		LastVisibleInputFrame = Session.Instance.InputFrame;
	}

	public bool IsVisibleForUpdate()
	{
		if (LastVisibleInputFrame != Session.Instance.InputFrame && CarryingObject == null)
		{
			return NeedsUpdateEveryFrame;
		}
		return true;
	}

	public override float GetVisibility(Character from, Vector3 eyePos, Vector3 eyeDir, float sightRange, out bool hasLineOfSightIgnoringPlantCover, out bool canHearFootsteps)
	{
		using (new UnityProfileMarker(GetVisibilityStr))
		{
			if (from.Zombie)
			{
				sightRange = Math.Min(sightRange, GetMaxVisibilityRangeToZombies());
			}
			bool flag = IsCrouching();
			GameTerrain instance = GameTerrain.Instance;
			Vector3 vector = (flag ? (Position + new Vector3(0f, VisRaycastCrouchingHeight, 0f)) : EyePosition) - eyePos;
			Vector2 rhs = MathUtil.ToXZ(vector);
			float magnitude = rhs.magnitude;
			if (magnitude > sightRange)
			{
				hasLineOfSightIgnoringPlantCover = false;
				canHearFootsteps = false;
				return 0f;
			}
			rhs /= magnitude;
			if (from.IsControllableByOrFollowingPlayer())
			{
				hasLineOfSightIgnoringPlantCover = true;
				canHearFootsteps = false;
				return 1f;
			}
			float num = 0f;
			if (flag)
			{
				num = (float)GetSkillLevelWithEffects(SkillType.Stealth) / 5f;
				if (IsSuperStealthyFollower())
				{
					hasLineOfSightIgnoringPlantCover = false;
					canHearFootsteps = false;
					return 0f;
				}
			}
			float num2 = MovementSpeed * (1f - num) * ((!flag) ? FootstepsRangeFactor : ((MovementSpeed <= CrouchWalkSpeed + 0.01f) ? 0f : FootstepsRangeFactorCrouching));
			canHearFootsteps = magnitude <= num2;
			if (canHearFootsteps && IsControllableByPlayer() && IsSneaking())
			{
				canHearFootsteps = false;
			}
			if (from.IsOutdoors() && !from.IsPlayingDead() && Vector2.Dot(MathUtil.ToXZ(eyeDir), rhs) < Mathf.Cos(MathF.PI / 180f * from.VisionConeDeg))
			{
				hasLineOfSightIgnoringPlantCover = false;
				return 0f;
			}
			if (from.Community != null && from.Community == Community && !from.Zombie)
			{
				bool flag2 = IsConscious;
				if (!flag2)
				{
					Target target = from.GetTarget(this);
					if (target != null)
					{
						flag2 |= target.GetFlag((TargetFlags)8390656);
					}
				}
				if (flag2)
				{
					hasLineOfSightIgnoringPlantCover = true;
					canHearFootsteps = false;
					return 1f;
				}
			}
			else if (Community != Session.Instance.CommunityManager.PlayerCommunity && !CanFollowPlayer)
			{
				hasLineOfSightIgnoringPlantCover = true;
				canHearFootsteps = false;
				return 1f;
			}
			float num3 = Mathf.Sqrt(Mathf.Clamp01((magnitude - 1f) / Mathf.Lerp(5f, 1f, num)));
			float num4 = 1f;
			if (flag)
			{
				TerrainCoord tile = Tile;
				float num5 = 0f;
				for (int i = 0; i < 8; i++)
				{
					GrassType grassType = (GrassType)i;
					float num6 = (float)instance.GrassMap.GetGrassAmount(tile.x, tile.y, grassType) / 15f;
					num6 = ((grassType != GrassType.Grass3) ? (num6 * 0.5f) : (num6 * 0.25f));
					num5 += num6;
				}
				instance.GetObjectsInRect(tile - new TerrainCoord(1, 1), tile + new TerrainCoord(1, 1), NearbyPlants);
				foreach (TileObject nearbyPlant in NearbyPlants)
				{
					float magnitude2 = (nearbyPlant.PosXZ - PosXZ).magnitude;
					num5 += nearbyPlant.GetPlantCoverForCamouflage() * (1f - Mathf.Clamp01((magnitude2 - 1f) / Radius));
				}
				NearbyPlants.Clear();
				float num7 = Math.Min(1f, num * num5 * num3);
				num4 *= 1f - num7 * magnitude / sightRange;
			}
			RaycastResult raycastResult = instance.RayCast(new Ray(eyePos, vector), magnitude, 0x28 | (flag ? 32768 : 0), from.InsideBuilding);
			CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
			hasLineOfSightIgnoringPlantCover = raycastResult.HitObject == null || raycastResult.HitObject == this || raycastResult.HitObject == InsideBuilding;
			num4 *= (hasLineOfSightIgnoringPlantCover ? (1f - Math.Min(1f, raycastResult.PlantCover * num3 * (0.25f + num * 0.75f))) : 0f);
			if (DrawLookRaycasts)
			{
				DebugGraphics.AddPersistentLine(eyePos, raycastResult.GetHitPosition(), Color.Lerp(Color.red, Color.green, num4));
			}
			return num4;
		}
	}

	public override bool IsTargetable()
	{
		if (Disappeared)
		{
			return false;
		}
		if (IsBeingCarried())
		{
			return false;
		}
		if (PlayDead && !Zombie && !Session.Instance.Editor)
		{
			return false;
		}
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		if (Hud.Instance.LocalTargetObject == this && Hud.Instance.LocalWantLockOnTarget)
		{
			return true;
		}
		TerrainCoord tile = Tile;
		return GameTerrain.Instance.FogOfWar.IsTileVisible(tile.x, tile.y);
	}

	public override bool IsVisibleInFogOfWar()
	{
		if (!FogOfWar.DebugFogOfWarEnabled)
		{
			return true;
		}
		TerrainCoord tile = Tile;
		return GameTerrain.Instance.FogOfWar.IsTileVisible(tile.x, tile.y);
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 3) == 0)
		{
			return false;
		}
		if ((options & 0x800) != 0 && Consciousness >= Consciousness.Sleeping)
		{
			return false;
		}
		if (tile != Tile)
		{
			return false;
		}
		bool flag = IsStationary();
		if ((options & 1) != 0 && flag)
		{
			return true;
		}
		if ((options & 2) != 0 && !flag)
		{
			return true;
		}
		return false;
	}

	public override bool IsRound()
	{
		return true;
	}

	public override Texture2D GetIcon(out Material mat, out Color col, bool highlighted)
	{
		Texture2D iconResource = GetIconResource();
		if (iconResource != null)
		{
			mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			col = Color.white;
			return iconResource;
		}
		iconResource = IconGenerator.Instance.FaceIconPool.GetTexture(IconIndex, Id);
		if (iconResource == null)
		{
			IconGenerator.Instance.Queue.Push(IconGenerationRequest.Face(this));
		}
		mat = (highlighted ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		col = ((iconResource != null) ? Color.white : MathUtil.TransparentBlackCol);
		return iconResource;
	}

	public Texture2D GetFullIcon()
	{
		Texture2D texture = IconGenerator.Instance.FullIconPool.GetTexture(FullIconIndex, Id);
		if (texture == null)
		{
			IconGenerator.Instance.Queue.Push(IconGenerationRequest.Full(this));
		}
		return texture;
	}

	public static void DrawIcon()
	{
		if (IconGimpMaster == null)
		{
			return;
		}
		Character character = IconGimp[(int)IconGimpGender];
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		IconGenerator instance3 = IconGenerator.Instance;
		Character character2 = IconGimpMaster as Character;
		Equipment equipment = IconGimpMaster as Equipment;
		if (!IconGimpMaster.Deleted || (IconGimpMaster is Equipment && CharacterCreationMenu.Instance != null && CharacterCreationMenu.Instance.PreviewEquipment.Contains((Equipment)IconGimpMaster)))
		{
			int stateNameHash = AnimHash.Idle;
			switch (GeneratingIconType)
			{
			case IconType.Full:
				stateNameHash = AnimHash.HudPose;
				break;
			case IconType.Portrait:
				switch (GeneratingPortraitPose)
				{
				case PortraitPose.None:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.SpikedBatOverShoulder:
					stateNameHash = AnimHash.PortraitPoseOneHanded;
					break;
				case PortraitPose.SpikedBatOverShoulderCloseUp:
					stateNameHash = AnimHash.PortraitPoseOneHanded;
					break;
				case PortraitPose.ZombieCharging:
					stateNameHash = AnimHash.PortraitPoseZombie;
					break;
				case PortraitPose.ZombieCharging2:
					stateNameHash = AnimHash.PortraitPoseZombie2;
					break;
				case PortraitPose.ZombieCharging3:
					stateNameHash = AnimHash.PortraitPoseZombie3;
					break;
				case PortraitPose.ZombieCharging4:
					stateNameHash = AnimHash.PortraitPoseZombie4;
					break;
				case PortraitPose.RifleIdle:
					stateNameHash = AnimHash.RifleIdle;
					break;
				case PortraitPose.RifleIdleCloseUp:
					stateNameHash = AnimHash.RifleIdle;
					break;
				case PortraitPose.InWatchtower:
					stateNameHash = AnimHash.RifleIdle;
					break;
				case PortraitPose.ShotgunAiming:
					stateNameHash = AnimHash.RifleIdleAiming;
					break;
				case PortraitPose.PistolAiming:
					stateNameHash = AnimHash.PistolIdleAiming;
					break;
				case PortraitPose.SitByFire:
					stateNameHash = AnimHash.Sitting;
					break;
				case PortraitPose.SitByFireCloseUp:
					stateNameHash = AnimHash.Sitting;
					break;
				case PortraitPose.BowAiming:
					stateNameHash = AnimHash.BowIdleAiming;
					break;
				case PortraitPose.CrouchingWithKnife:
					stateNameHash = AnimHash.OneHandedCrouchIdle;
					break;
				case PortraitPose.ChoppingWithAxe:
					stateNameHash = AnimHash.ChopTreeLoop;
					break;
				case PortraitPose.PickaxeOverShoulder:
					stateNameHash = AnimHash.PortraitPoseOneHanded2;
					break;
				case PortraitPose.PickaxeOverShoulderCloseUp:
					stateNameHash = AnimHash.PortraitPoseOneHanded2;
					break;
				case PortraitPose.InHelicopter:
					stateNameHash = AnimHash.PortraitPoseSit;
					break;
				case PortraitPose.OneHandedAttack1:
					stateNameHash = AnimHash.PortraitPoseOneHandedAttack1;
					break;
				case PortraitPose.OneHandedAttack1CloseUp:
					stateNameHash = AnimHash.PortraitPoseOneHandedAttack1;
					break;
				case PortraitPose.OneHandedAttack2:
					stateNameHash = AnimHash.PortraitPoseOneHandedAttack2;
					break;
				case PortraitPose.Running:
					stateNameHash = AnimHash.PortraitPoseRun;
					break;
				case PortraitPose.Eating:
					stateNameHash = AnimHash.PortraitPoseEat;
					break;
				case PortraitPose.Drinking:
					stateNameHash = AnimHash.PortraitPoseDrink;
					break;
				case PortraitPose.Reading:
					stateNameHash = AnimHash.ReadLoop;
					break;
				case PortraitPose.Hugging:
					stateNameHash = AnimHash.HugLoop;
					break;
				case PortraitPose.HandsUp:
					stateNameHash = AnimHash.HandsUp;
					break;
				case PortraitPose.MugShot:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotHappy:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotAngry:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotAggressive:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotWistful:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotSuspicious:
					stateNameHash = AnimHash.Idle;
					break;
				case PortraitPose.MugShotMoved:
					stateNameHash = AnimHash.Idle;
					break;
				}
				break;
			}
			character.Unity.UmaExpressionPlayer.WantExpensiveUpdate = true;
			character.Unity.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			character.Unity.Obj.SetActive(value: true);
			character.Unity.Animator.Play(stateNameHash);
			character.EquippedItem = ((GeneratingIconType == IconType.Portrait) ? character2.EquippedItem : null);
			character.SetFacingAngle(0f);
			character.Position = Sun.IconGenerationPos;
			float daysSinceStart = 0.5f;
			float num = 0f;
			Character character3 = null;
			Camera component = ((GeneratingIconType == IconType.Portrait) ? instance.UnityPortraitCameraObj : instance.UnityIconCameraObj).GetComponent<Camera>();
			RenderTexture renderTexture = null;
			switch (GeneratingIconType)
			{
			case IconType.Full:
				renderTexture = instance3.FullIconPool.RenderTex;
				break;
			case IconType.Face:
				renderTexture = instance3.FaceIconPool.RenderTex;
				break;
			case IconType.Portrait:
				renderTexture = PortraitGallery.Instance.RenderTex;
				break;
			case IconType.Clothing:
				renderTexture = instance3.ClothingIconPool.RenderTex;
				break;
			}
			component.targetTexture = renderTexture;
			component.gameObject.SetActive(value: true);
			RenderTexture.active = renderTexture;
			List<GameObject> list = new List<GameObject>();
			switch (GeneratingIconType)
			{
			case IconType.Full:
			{
				Vector3 vector3 = character.Pos + new Vector3(0f, HumanAppearance.MaleDefaultHeight * 0.5f, 0f);
				component.transform.position = vector3 + character.Forward * 1.5f;
				component.orthographicSize = 1f;
				component.transform.LookAt(vector3, Vector3.up);
				break;
			}
			case IconType.Face:
			{
				Vector3 vector2 = character.Pos + new Vector3(0f, character.Appearance.EyeHeight, 0f);
				component.transform.position = vector2 + character.Forward * 0.45f;
				component.orthographicSize = 0.15f;
				component.transform.LookAt(vector2, Vector3.up);
				if (character2.IsSleepingOrUnconscious())
				{
					character.Unity.UmaExpressionPlayer.leftEyeOpen_Close = -1f;
					character.Unity.UmaExpressionPlayer.rightEyeOpen_Close = -1f;
				}
				break;
			}
			case IconType.Portrait:
			{
				bool flag = PortraitGeneratorMenu.Instance.IsGeneratingAchievementIcons();
				CustomRandom customRandom = (flag ? PortraitGeneratorMenu.Instance.Rand : MathUtil.NonDeterministicRand);
				float num2 = 1f;
				float y = character.Appearance.EyeHeight;
				float x = 0f;
				float angle = character.FacingAngle;
				float z = 0f;
				float z2 = 0f;
				switch (GeneratingPortraitPose)
				{
				case PortraitPose.MugShot:
				case PortraitPose.MugShotHappy:
				case PortraitPose.MugShotAngry:
				case PortraitPose.MugShotAggressive:
				case PortraitPose.MugShotWistful:
				case PortraitPose.MugShotSuspicious:
				case PortraitPose.MugShotMoved:
					num2 = 0.45f;
					y = character.Appearance.EyeHeight * 0.98f;
					switch (GeneratingPortraitPose)
					{
					case PortraitPose.MugShotHappy:
						character.Unity.UmaExpressionPlayer.leftMouthSmile_Frown = 0.5f;
						character.Unity.UmaExpressionPlayer.rightMouthSmile_Frown = 0.5f;
						break;
					case PortraitPose.MugShotSuspicious:
						character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.midBrowUp_Down = -0.75f;
						break;
					case PortraitPose.MugShotAngry:
						character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.leftMouthSmile_Frown = -1f;
						character.Unity.UmaExpressionPlayer.rightMouthSmile_Frown = -1f;
						character.Unity.UmaExpressionPlayer.leftUpperLipUp_Down = 1f;
						character.Unity.UmaExpressionPlayer.rightUpperLipUp_Down = 1f;
						character.Unity.UmaExpressionPlayer.noseSneer = 1f;
						break;
					case PortraitPose.MugShotAggressive:
						character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 0.5f;
						character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.leftMouthSmile_Frown = 1f;
						character.Unity.UmaExpressionPlayer.rightMouthSmile_Frown = 1f;
						break;
					case PortraitPose.MugShotWistful:
						character.Unity.UmaExpressionPlayer.leftBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.rightBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.midBrowUp_Down = 1f;
						break;
					case PortraitPose.MugShotMoved:
						character.Unity.UmaExpressionPlayer.leftBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.rightBrowUp_Down = -1f;
						character.Unity.UmaExpressionPlayer.midBrowUp_Down = 1f;
						character.Unity.UmaExpressionPlayer.leftMouthSmile_Frown = 0.5f;
						character.Unity.UmaExpressionPlayer.rightMouthSmile_Frown = 0.5f;
						break;
					}
					break;
				case PortraitPose.SpikedBatOverShoulder:
					num2 = 0.45f;
					y = character.Appearance.EyeHeight;
					x = 0.05f;
					break;
				case PortraitPose.SpikedBatOverShoulderCloseUp:
					num2 = 0.3f;
					y = character.Appearance.EyeHeight;
					x = -0.02f;
					break;
				case PortraitPose.ZombieCharging:
					num2 = 0.6f;
					y = character.Appearance.Height * 0.9f;
					break;
				case PortraitPose.ZombieCharging2:
					num2 = 0.6f;
					y = character.Appearance.Height * 0.85f;
					break;
				case PortraitPose.ZombieCharging3:
					num2 = 0.6f;
					y = character.Appearance.Height * 0.85f;
					break;
				case PortraitPose.ZombieCharging4:
					num2 = 0.6f;
					y = character.Appearance.Height * 0.85f;
					break;
				case PortraitPose.RifleIdle:
					num2 = 1f;
					y = character.Appearance.EyeHeight * 0.85f;
					angle = 0.25f;
					break;
				case PortraitPose.RifleIdleCloseUp:
					num2 = 0.5f;
					y = character.Appearance.EyeHeight * 0.95f;
					angle = 0.25f;
					break;
				case PortraitPose.ShotgunAiming:
					num2 = 0.9f;
					y = character.Appearance.EyeHeight * 0.85f;
					angle = -MathF.PI / 5f;
					x = 0.4f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					break;
				case PortraitPose.PistolAiming:
					num2 = 0.8f;
					y = character.Appearance.EyeHeight * 0.85f;
					angle = -MathF.PI / 5f;
					x = 0.25f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					break;
				case PortraitPose.SitByFire:
					num2 = character.Appearance.OverallScale * 1.1f;
					y = 0.5f;
					angle = 0f;
					break;
				case PortraitPose.SitByFireCloseUp:
					num2 = character.Appearance.OverallScale * 0.5f;
					y = character.Appearance.OverallScale * 0.75f;
					angle = 0f;
					break;
				case PortraitPose.BowAiming:
					num2 = 1.4f;
					y = character.Appearance.EyeHeight * 0.9f;
					break;
				case PortraitPose.CrouchingWithKnife:
					num2 = character.Appearance.OverallScale * 1.4f;
					y = 0.6f;
					angle = 0f;
					break;
				case PortraitPose.ChoppingWithAxe:
					num2 = 1.2f;
					y = character.Appearance.EyeHeight * 0.75f;
					angle = MathF.PI / 2f;
					break;
				case PortraitPose.PickaxeOverShoulder:
					num2 = 0.6f;
					y = character.Appearance.EyeHeight;
					x = 0.15f;
					z = 0.4f;
					break;
				case PortraitPose.PickaxeOverShoulderCloseUp:
					num2 = 0.4f;
					y = character.Appearance.EyeHeight;
					x = 0.1f;
					z = 0.4f;
					break;
				case PortraitPose.Running:
					num2 = 1.5f;
					y = character.Appearance.EyeHeight * 0.7f;
					angle = MathF.PI / 2f;
					z2 = 0.1f;
					break;
				case PortraitPose.HandsUp:
					num2 = character.Appearance.OverallScale * 1.5f;
					y = character.Appearance.EyeHeight * 0.96f;
					break;
				case PortraitPose.Eating:
					num2 = 0.5f;
					y = character.Appearance.EyeHeight * 0.96f;
					x = 0.05f;
					break;
				case PortraitPose.Drinking:
					num2 = 0.6f;
					y = character.Appearance.EyeHeight;
					angle = -MathF.PI / 4f;
					x = 0.1f;
					break;
				case PortraitPose.Reading:
					num2 = character.Appearance.OverallScale * 0.7f;
					y = character.Appearance.EyeHeight * 0.9f;
					angle = MathF.PI / 8f;
					z2 = 0.1f;
					x = -0.1f;
					break;
				case PortraitPose.OneHandedAttack1:
					num2 = 0.8f;
					y = character.Appearance.EyeHeight * 0.9f;
					angle = 0f;
					x = 0f;
					z = -0.6f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					break;
				case PortraitPose.OneHandedAttack1CloseUp:
					num2 = 0.7f;
					y = character.Appearance.EyeHeight * 0.9f;
					angle = -MathF.PI / 4f;
					x = 0.3f;
					z2 = 0.1f;
					z = -0.3f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					break;
				case PortraitPose.OneHandedAttack2:
					num2 = 0.8f;
					y = character.Appearance.EyeHeight * 0.9f;
					x = 0f;
					z = -0.6f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					break;
				case PortraitPose.InHelicopter:
				{
					num2 = Mathf.Lerp(0.3f, 0.4f, flag ? 0f : customRandom.RandomFloat());
					z2 = -0.1f;
					int num3 = ((!flag) ? customRandom.Next(2) : 0);
					angle = ((num3 == 0) ? (MathF.PI / 4f) : (-MathF.PI / 4f)) + ((flag ? 0f : customRandom.RandomFloat()) * 2f - 1f) * (MathF.PI / 8f);
					y = character.Appearance.EyeHeight * 0.75f;
					daysSinceStart = ((num3 == 0) ? (-0.72f) : 0.72f);
					Color color = ((HumanAppearance)character.Appearance).GetSkinColor(character);
					float num4 = 1f - (color.r + color.g + color.b) / 3f;
					num += Mathf.Max(0f, (num4 - 0.5f) * 2f);
					PropPrototype propPrototype2 = instance.FindPropPrototypeByName("Helicopter3");
					if (propPrototype2 != null && propPrototype2.Prefabs.Count > 0)
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate((GameObject)propPrototype2.Prefabs[0]);
						if (gameObject2 != null)
						{
							gameObject2.transform.position = Sun.IconGenerationPos + new Vector3((num3 == 0) ? 0.5f : (-0.5f), -0.25f, 0f);
							gameObject2.transform.rotation = Quaternion.Euler(0f, 270f, 0f);
							Prop.ReplaceLayerRecursively(gameObject2, DefaultLayer, IconGimpLayer, skipParticleEffects: false);
							list.Add(gameObject2);
						}
					}
					break;
				}
				case PortraitPose.InWatchtower:
				{
					num2 = 3f;
					y = character.Appearance.EyeHeight * 0.9f;
					angle = MathF.PI / 8f;
					PropPrototype propPrototype = instance.FindPropPrototypeByName("WatchTower");
					if (propPrototype != null && propPrototype.Prefabs.Count > 0)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)propPrototype.Prefabs[0]);
						if (gameObject != null)
						{
							gameObject.transform.position = character.Position - propPrototype.Inhabitants[0].Pos;
							Prop.ReplaceLayerRecursively(gameObject, DefaultLayer, IconGimpLayer, skipParticleEffects: false);
							list.Add(gameObject);
						}
					}
					break;
				}
				case PortraitPose.Hugging:
					num2 = 1.5f;
					y = character.Appearance.EyeHeight * 0.85f;
					angle = MathF.PI / 2f;
					z2 = -0.75f;
					character3 = IconGimp[(int)(IconGimpGender + 1) % IconGimp.Length];
					if (character3 == null)
					{
						break;
					}
					if (!flag || PortraitGeneratorMenu.Instance.CurrentIconType == PortraitGeneratorMenu.AchievementIconType.Steam_256x256_jpg)
					{
						((HumanAppearance)character3.Appearance).Randomize(InfectionType.None, customRandom);
						for (int i = 0; i < 9; i++)
						{
							character3.Clothes[i] = null;
						}
						character3.RandomizeClothing(customRandom, seasonallyAppropriate: false, isPortrait: true, mustHaveBackpack: false, mustHaveBodyArmor: false, mustHaveHelmet: false, mustHaveLegArmor: false);
						character3.UnityOnChangedBones();
						for (int j = 0; j < 9; j++)
						{
							character3.Clothes[j] = null;
						}
					}
					character3.Unity.UmaExpressionPlayer.WantExpensiveUpdate = true;
					character3.Unity.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
					character3.Unity.Obj.SetActive(value: true);
					character3.Unity.Animator.Play(AnimHash.Hugged);
					character.SetFacingAngle(MathF.PI);
					character3.SetFacingAngle(0f);
					character3.Position = character.Pos + character.Forward * HugTarget.MaxStartHugDist;
					break;
				}
				if (character.Infection != InfectionType.None)
				{
					character.Unity.UmaExpressionPlayer.jawOpen_Close = 0.5f;
					character.Unity.UmaExpressionPlayer.leftEyeOpen_Close = 1f;
					character.Unity.UmaExpressionPlayer.rightEyeOpen_Close = 1f;
					character.Unity.UmaExpressionPlayer.leftBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightBrowUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.midBrowUp_Down = -1f;
					character.Unity.UmaExpressionPlayer.leftUpperLipUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.rightUpperLipUp_Down = 1f;
					character.Unity.UmaExpressionPlayer.noseSneer = 1f;
					character.Unity.UmaExpressionPlayer.leftMouthSmile_Frown = -1f;
					character.Unity.UmaExpressionPlayer.rightMouthSmile_Frown = -1f;
				}
				Vector3 vector = character.Pos + new Vector3(x, y, z2);
				component.transform.position = vector + MathUtil.ToXZY(MathUtil.GetDirFromAngle(angle), z) * num2;
				component.transform.LookAt(vector, Vector3.up);
				break;
			}
			case IconType.Clothing:
				equipment.SetupClothingIconCam(component, character);
				break;
			}
			instance.Sun.SetupLightForTime(daysSinceStart, useMiddayReflectionTex: true);
			if (num > 0f)
			{
				Color ambientLight = RenderSettings.ambientLight;
				ambientLight += new Color(ambientLight.r * num, ambientLight.g * num, ambientLight.b * num, 0f);
				RenderSettings.ambientLight += ambientLight;
			}
			character.UnityUpdate();
			character.Unity.Animator.Update(1f);
			character.Unity.UmaExpressionPlayer.enableSaccades = false;
			character.Unity.UmaExpressionPlayer.Update();
			character.Unity.UmaExpressionPlayer.LateUpdate();
			if (character3 != null)
			{
				character3.UnityUpdate();
				character3.Unity.Animator.Update(1f);
				character3.Unity.UmaExpressionPlayer.enableSaccades = false;
				character3.Unity.UmaExpressionPlayer.Update();
				character3.Unity.UmaExpressionPlayer.LateUpdate();
			}
			component.Render();
			if (GeneratingIconFirstTimeHack)
			{
				component.Render();
				GeneratingIconFirstTimeHack = false;
			}
			switch (GeneratingIconType)
			{
			case IconType.Full:
			{
				character2.FullIconIndex = instance3.FullIconPool.AddTexture(IconGimpMaster.Id);
				Texture2D texture2 = instance3.FullIconPool.GetTexture(character2.FullIconIndex, IconGimpMaster.Id);
				if (texture2 != null)
				{
					texture2.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
					texture2.Apply();
				}
				break;
			}
			case IconType.Face:
			{
				character2.IconIndex = instance3.FaceIconPool.AddTexture(IconGimpMaster.Id);
				Texture2D texture3 = instance3.FaceIconPool.GetTexture(character2.IconIndex, IconGimpMaster.Id);
				if (texture3 != null)
				{
					texture3.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
					texture3.Apply();
				}
				break;
			}
			case IconType.Clothing:
			{
				int clothingVariationHash = equipment.GetClothingVariationHash();
				equipment.ClothingIconIndex = instance3.ClothingIconPool.AddTexture(clothingVariationHash);
				Texture2D texture = instance3.ClothingIconPool.GetTexture(equipment.ClothingIconIndex, clothingVariationHash);
				if (texture != null)
				{
					texture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
					texture.Apply();
				}
				break;
			}
			case IconType.Portrait:
				PortraitGallery.Instance.OnPortraitGenerated(character2, GeneratingPortraitPose);
				break;
			}
			RenderTexture.active = null;
			component.gameObject.SetActive(value: false);
			component.targetTexture = null;
			if (instance2 != null)
			{
				instance.Sun.SetupLightForTime(instance2.DaysSinceStart, useMiddayReflectionTex: false);
			}
			character.Unity.Obj.SetActive(value: false);
			character.EquippedItem = null;
			if (character3 != null)
			{
				character3.Unity.Obj.SetActive(value: false);
				character3.EquippedItem = null;
			}
			foreach (GameObject item in list)
			{
				UnityEngine.Object.DestroyImmediate(item);
			}
		}
		IconType generatingIconType = GeneratingIconType;
		if ((uint)(generatingIconType - 1) <= 2u)
		{
			instance3.Queue.FinishedProcessing();
		}
		IconGimpMaster = null;
	}

	public void TryStartGeneratingIcon(IconType iconType, PortraitPose pose)
	{
		if (IconGimpMaster == null)
		{
			IconGimpMaster = this;
			IconGimpGender = Appearance.Gender;
			GeneratingIconType = iconType;
			GeneratingPortraitPose = pose;
			if (IconGimp[(int)IconGimpGender] == null)
			{
				IconGimp[(int)IconGimpGender] = new Human();
			}
			Character character = IconGimp[(int)IconGimpGender];
			character.Position = Sun.IconGenerationPos;
			character.Usage = CharacterUsage.IconGimp;
			character.Infection = Infection;
			character.InfectionProgression = InfectionProgression;
			character.Appearance = Appearance;
			for (int i = 0; i < 9; i++)
			{
				character.Clothes[i] = Clothes[i];
			}
			character.UnityOnChangedBones();
			for (int j = 0; j < 9; j++)
			{
				character.Clothes[j] = null;
			}
		}
	}

	public static void TryStartGeneratingClothingIcon(Equipment clothing)
	{
		if (IconGimpMaster == null)
		{
			IconGimpMaster = clothing;
			IconGimpGender = GenderType.Male;
			GeneratingIconType = IconType.Clothing;
			GeneratingPortraitPose = PortraitPose.None;
			if (IconGimp[(int)IconGimpGender] == null)
			{
				IconGimp[(int)IconGimpGender] = new Human();
			}
			Character character = IconGimp[(int)IconGimpGender];
			character.Usage = CharacterUsage.IconGimp;
			character.Appearance = new HumanAppearance(IconGimpGender, 30f);
			for (int i = 0; i < 9; i++)
			{
				character.Clothes[i] = ((i == (int)clothing.GetClothingType()) ? clothing : null);
			}
			character.UnityOnChangedBones();
			for (int j = 0; j < 9; j++)
			{
				character.Clothes[j] = null;
			}
		}
	}

	public static void CancelGeneratingClothingIcon(Equipment clothing)
	{
		if (IconGimpMaster == clothing)
		{
			IconGimpMaster = null;
		}
	}

	public bool IsStationary()
	{
		if (MovementType != MovementType.None && Route.Count != 0)
		{
			if (Route.Count == 1)
			{
				return Route[0] == Tile;
			}
			return false;
		}
		return true;
	}

	private void UpdateCachedAStarInfo(bool force)
	{
		if (!IsPredicted() && !Disappeared)
		{
			bool flag = IsStationary();
			bool isAwake = IsAwake && CarriedBy == null;
			if (flag != CachedIsStationary || force)
			{
				GameTerrain.Instance.AStar.AddChange(AStarChange.AddCharacter(this, isAwake, flag));
			}
			CachedIsStationary = flag;
		}
	}

	public void HaltMovement()
	{
		MovementType = MovementType.None;
		MovementSpeed = 0f;
		WantAvoidance = false;
		Route.Clear();
		UpdateCachedAStarInfo(force: false);
	}

	public void PopNextRouteNode()
	{
		Route.RemoveAt(0);
		if (Route.Count == 0)
		{
			MovementType = MovementType.None;
			MovementSpeed = 0f;
			WantAvoidance = false;
		}
		UpdateCachedAStarInfo(force: false);
	}

	public void StartMovingDirectlyToTarget(MovementType movementType)
	{
		MovementSpeed = 0f;
		WantAvoidance = false;
		Route.Clear();
		MovementMode = MovementMode.MoveDirectlyToTarget;
		MovementType = movementType;
		UpdateCachedAStarInfo(force: false);
	}

	public void StopMovingDirectlyToTarget()
	{
		MovementMode = MovementMode.Pathfinding;
		MovementType = MovementType.None;
		MovementSpeed = 0f;
		UpdateCachedAStarInfo(force: false);
	}

	public void StartFlankingTarget(bool left, MovementType movementType, float desiredRange)
	{
		MovementSpeed = 0f;
		WantAvoidance = false;
		Route.Clear();
		MovementMode = (left ? MovementMode.FlankLeft : MovementMode.FlankRight);
		MovementType = movementType;
		FlankingDesiredRange = desiredRange;
		UpdateCachedAStarInfo(force: false);
	}

	public void StopFlankingTarget()
	{
		MovementMode = MovementMode.Pathfinding;
		MovementType = MovementType.None;
		MovementSpeed = 0f;
		UpdateCachedAStarInfo(force: false);
	}

	public void MoveToCurrentTile(MovementType movementType)
	{
		TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(Position);
		if ((Position - GameTerrain.Instance.GetTileCentrePos(tileCoordForPos)).sqrMagnitude > 0.0001f)
		{
			MovementType = movementType;
			Route.Clear();
			Route.Add(tileCoordForPos);
			UpdateCachedAStarInfo(force: false);
		}
		else
		{
			HaltMovement();
		}
	}

	public void MoveTo(MovementType movementType, List<TerrainCoord> route)
	{
		MovementType = movementType;
		WantAvoidance = false;
		Route.Clear();
		Route.InsertRange(0, route);
		UpdateCachedAStarInfo(force: false);
	}

	public void MoveToTile(MovementType movementType, TerrainCoord tile)
	{
		MovementType = movementType;
		WantAvoidance = false;
		Route.Clear();
		Route.Add(tile);
		UpdateCachedAStarInfo(force: false);
	}

	public bool IsNearRoute(TerrainCoord tile, float range)
	{
		GameTerrain instance = GameTerrain.Instance;
		Vector2 tileCentreXZ = instance.GetTileCentreXZ(tile);
		Vector2 tileCentreXZ2 = instance.GetTileCentreXZ(Tile);
		for (int i = 0; i < Route.Count; i++)
		{
			Vector2 tileCentreXZ3 = instance.GetTileCentreXZ(Route[i]);
			if (MathUtil.GetDistSqFromLineToPoint(tileCentreXZ2, tileCentreXZ3, tileCentreXZ) <= range * range)
			{
				return true;
			}
		}
		return false;
	}

	public void SetMovementType(MovementType movementType)
	{
		if (Route.Count > 0)
		{
			MovementType = movementType;
			UpdateCachedAStarInfo(force: false);
		}
	}

	public MovementType GetMovementType()
	{
		return MovementType;
	}

	public bool IsFinishedRoute()
	{
		return Route.Count == 0;
	}

	public int GetRouteCount()
	{
		return Route.Count;
	}

	public TerrainCoord GetRouteNode(int i)
	{
		return Route[i];
	}

	public TerrainCoord GetRouteNextNode()
	{
		if (Route.Count == 0)
		{
			return Tile;
		}
		return Route[0];
	}

	public TerrainCoord GetRouteDestination()
	{
		if (Route.Count == 0)
		{
			return Tile;
		}
		return Route[Route.Count - 1];
	}

	public void OnCollisionWithCharacter(Character other)
	{
		if (Goal != null)
		{
			Goal.OnCollisionWithCharacter(this, null, other);
		}
	}

	public bool IsPushingAgainstWaistHighWall()
	{
		if (DirectControlled && PushingAgainstWaistHighWallCharacter == this && PushingAgainstWaistHighWallFrame >= GameImpl.FrameCount - 1)
		{
			return !PushingAgainstWaistHighWallObj.Deleted;
		}
		return false;
	}

	public void OnCollisionWithProp(TileObject prop)
	{
		if (GetMovementType() != MovementType.None && GetRouteCount() > 0 && prop is Gate { UnderConstructionInfo: null } gate && (gate.GateState != GateState.Open || (gate.Community != null && gate.Community.CommunityType == CommunityType.Player && Community != null && Community.CommunityType != CommunityType.Player && CanOpenPlayerGates != global::CanOpenGates.Yes && gate.IsInFrontOfGate(PosXZ))))
		{
			Bounds boundingBox = gate.GetBoundingBox();
			if (MathUtil.DoesSegmentIntersectRect(PosXZ, GameTerrain.Instance.GetTileCentreXZ(GetRouteNextNode()), MathUtil.ToXZ(boundingBox.min), MathUtil.ToXZ(boundingBox.max)))
			{
				OnEncounterGate(gate);
			}
		}
		if (prop.CoverType != CoverType.WaistHigh || IsInDamageReactionAnim() || IsRagdollOrProneOrRecovering() || (prop.GetUnderConstructionInfo() != null && !prop.GetUnderConstructionInfo().IsBuiltEnoughToBeAnObstacle()) || !IsMovingTowards(prop, MathF.PI / 4f))
		{
			return;
		}
		if (CurrentActionAnim == ActionAnim.Slide)
		{
			TryStartActionAnim(ActionAnim.Vault, prop);
		}
		if (DirectControlled)
		{
			if (IsFrontmostPrediction())
			{
				PushingAgainstWaistHighWallFrame = Time.frameCount;
				PushingAgainstWaistHighWallCharacter = GetAuthoritativeOrElseThisCharacter();
				PushingAgainstWaistHighWallObj = prop;
			}
		}
		else if (GetMovementType() != MovementType.None && GetRouteCount() > 0)
		{
			Bounds boundingBox2 = prop.GetBoundingBox();
			if (MathUtil.DoesSegmentIntersectRect(PosXZ, GameTerrain.Instance.GetTileCentreXZ(GetRouteNextNode()), MathUtil.ToXZ(boundingBox2.min), MathUtil.ToXZ(boundingBox2.max)))
			{
				OnEncounterWaistHighWall(prop);
			}
		}
	}

	public void OnEncounterGate(Gate gate)
	{
		if (Goal != null)
		{
			Goal.OnEncounterGate(this, null, gate);
		}
	}

	public void OnEncounterWaistHighWall(TileObject prop)
	{
		if (Goal != null)
		{
			Goal.OnEncounterWaistHighWall(this, null, prop);
		}
	}

	public float CalcYPos(float x, float z)
	{
		return GameTerrain.Instance.GetTileHeightAtPos(x, z);
	}

	public override void SetTile(TerrainCoord tile)
	{
		Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(tile);
		SetPosition(tileCentreXZ.x, tileCentreXZ.y);
	}

	public void SetPosition(float x, float z)
	{
		float num = GameTerrain.Instance.HalfSize - 1f;
		x = Mathf.Clamp(x, 0f - num, num);
		z = Mathf.Clamp(z, 0f - num, num);
		float y = CalcYPos(x, z);
		SetPosition(new Vector3(x, y, z));
	}

	public void SetPosition(Vector3 pos)
	{
		float num = GameTerrain.Instance.HalfSize - 1f;
		if (MathUtil.IsNaNorInfinity(pos))
		{
			string displayNameString = GetDisplayNameString();
			Vector3 vector = pos;
			Debug.LogWarning("NaN in SetPosition for " + displayNameString + ": " + vector.ToString());
			return;
		}
		if (pos.x < 0f - num || pos.x > num || pos.z < 0f - num || pos.z > num || pos.y < -1f || pos.y > 64f)
		{
			string displayNameString2 = GetDisplayNameString();
			Vector3 vector = pos;
			Debug.LogWarning("Dodgy pos being set for " + displayNameString2 + ": " + vector.ToString());
		}
		pos.x = Mathf.Clamp(pos.x, 0f - num, num);
		pos.z = Mathf.Clamp(pos.z, 0f - num, num);
		if (VisibleTilesCache != null && Tile != GameTerrain.Instance.GetTileCoordForPos(pos))
		{
			GameTerrain.Instance.FogOfWar.MarkVisibleTilesCacheDirty(this);
		}
		TerrainCoord tile = Tile;
		Position = pos;
		UpdateWorldTransformAndBounds();
		OnMoved(tile, Tile);
	}

	private void UpdateWorldTransformAndBounds()
	{
		World = Matrix4x4.TRS(Position, Quaternion.Euler(0f, FacingAngle * 57.29578f, 0f), Vector3.one);
		SetBoundingBox(MathUtil.CreateBoundsMinMax(Position - new Vector3(0.95f, 0f, 0.95f), Position + new Vector3(0.95f, 2.5f, 0.95f)));
	}

	public override void OnTerrainHeightChanged()
	{
		if (InsideBuilding != null && InsideBuilding.GetInhabitantSlotDefs()[InsideBuildingSlotIndex].External)
		{
			InsideBuilding.OnTerrainHeightChanged();
			Position.y = InsideBuilding.GetSlotPos(InsideBuildingSlotIndex).y;
		}
		else
		{
			Position.y = CalcYPos(Position.x, Position.z);
		}
		UpdateWorldTransformAndBounds();
	}

	public bool IsAwareOfTrap(TileObject obj)
	{
		if (Community == null)
		{
			return false;
		}
		if (Infection > InfectionType.None && Infection < InfectionType.Invisible)
		{
			return false;
		}
		if (obj.GetCommunity() == Community)
		{
			return true;
		}
		if (obj is Tripwire)
		{
			if (Community.WarnedAboutTripwires)
			{
				return true;
			}
		}
		else if (Community.WarnedAboutTraps)
		{
			return true;
		}
		return false;
	}

	public void OnMoved(TerrainCoord oldTile, TerrainCoord newTile)
	{
		if (oldTile != newTile)
		{
			if (IsAuthoritative())
			{
				_oldTriggerZones.Clear();
				_newTriggerZones.Clear();
				GameTerrain.Instance.GetTriggerZonesOnTile(oldTile, _oldTriggerZones);
				GameTerrain.Instance.GetTriggerZonesOnTile(newTile, _newTriggerZones);
				for (int i = 0; i < _oldTriggerZones.Count; i++)
				{
					if (_newTriggerZones.IndexOf(_oldTriggerZones[i]) == -1)
					{
						_oldTriggerZones[i].OnCharacterLeft(this);
					}
				}
				for (int j = 0; j < _newTriggerZones.Count; j++)
				{
					if (_oldTriggerZones.IndexOf(_newTriggerZones[j]) == -1)
					{
						_newTriggerZones[j].OnCharacterEntered(this);
					}
				}
				if (Alive && CarriedBy == null && InsideBuilding == null)
				{
					TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(newTile.x, newTile.y);
					if (fixedObjectOnTile is ITrap trap && trap.CanTriggerTrap(this) && !IsAwareOfTrap(fixedObjectOnTile))
					{
						Session.Instance.PropManager.AddTriggeredTrap(this, fixedObjectOnTile);
					}
				}
			}
			if (DirectControlled && InsideBuilding == null)
			{
				SetHangoutLocation(newTile);
			}
			UpdateCachedAStarInfo(force: true);
			if (IsInMapWho)
			{
				GameTerrain.Instance.CharacterMapWho.OnMoved(this, oldTile, newTile);
			}
			if (IsInBurningMapWho)
			{
				GameTerrain.Instance.BurningMapWho.OnMoved(this, oldTile, newTile);
			}
		}
		if (CarryingObject != null && CarryingObject is Character character)
		{
			character.SetPosition(Position);
		}
		UnityUpdatePosition();
	}

	public virtual void OnMovedByCollisionManager()
	{
	}

	public void UnregisterFromTriggerZones()
	{
		_oldTriggerZones.Clear();
		GameTerrain.Instance.GetTriggerZonesOnTile(Tile, _oldTriggerZones);
		for (int i = 0; i < _oldTriggerZones.Count; i++)
		{
			_oldTriggerZones[i].OnCharacterLeft(this);
		}
	}

	public void RegisterWithTriggerZones()
	{
		_newTriggerZones.Clear();
		GameTerrain.Instance.GetTriggerZonesOnTile(Tile, _newTriggerZones);
		for (int i = 0; i < _newTriggerZones.Count; i++)
		{
			_newTriggerZones[i].OnCharacterEntered(this);
		}
	}

	public void AddVelocityXZ(Vector2 velXZ, bool setFollowMeDir)
	{
		if (InsideBuilding != null)
		{
			return;
		}
		if (MathUtil.IsNaNorInfinity(velXZ))
		{
			string displayNameString = GetDisplayNameString();
			Vector2 vector = velXZ;
			Debug.LogWarning("NaN in AddVelocityXZ for " + displayNameString + ": " + vector.ToString());
			return;
		}
		VelocityXZ += velXZ;
		if (CurrentActionAnim != ActionAnim.SitByFire)
		{
			ClearSitting();
		}
		Session instance = Session.Instance;
		(IsPredicted() ? instance.PredictedObjectManager.PredictedCollisionManager : instance.CharacterManager.CollisionManager).AddToMovers(this);
		if (setFollowMeDir)
		{
			SetFollowMePos(velXZ);
		}
	}

	public void SetFollowMePos(Vector2 velXZ)
	{
		int currentFrame = PredictedObjectManager.Instance.GetCurrentFrame(IsPredicted());
		if (FollowMeMoveFrame >= currentFrame - 10 || (FollowMePosXZ - PosXZ).sqrMagnitude >= MathUtil.Squared(FollowGoal.CalcMaxFollowerDist(this)))
		{
			FollowMeMoveFrame = currentFrame;
			FollowMePosXZ = PosXZ + velXZ;
			FollowMeDirAngle = MathUtil.GetAngleFromDir(velXZ, DesiredFacingAngle);
		}
	}

	public void ClearVelocityXZ()
	{
		VelocityXZ = Vector2.zero;
		Session instance = Session.Instance;
		(IsPredicted() ? instance.PredictedObjectManager.PredictedCollisionManager : instance.CharacterManager.CollisionManager).RemoveFromMovers(this);
	}

	public Vector3 GetOldVelocity()
	{
		return (Position - OldPosition) / FramesPerUpdate;
	}

	public void SetSparringPartner(SparringType sparringType, Character sparringPartner, Character instigator)
	{
		SparringType sparringType2 = SparringType;
		if (SparringType == SparringType.Feuding && SparringPartner != null && SparringPartner.SparringPartner == this && IsAuthoritative())
		{
			Character leader = Session.Instance.CommunityManager.PlayerCommunity.Leader;
			if (leader != null && leader != this && leader != SparringPartner)
			{
				bool num = Relationship.IsRomanticRelationshipType(Relationship.GetRelationship(this, leader));
				bool flag = Relationship.IsRomanticRelationshipType(Relationship.GetRelationship(SparringPartner, leader));
				bool num2 = num & HasJealousRomanticMemoryOf(SparringPartner, leader);
				flag &= SparringPartner.HasJealousRomanticMemoryOf(this, leader);
				if (num2 && flag)
				{
					AchievementsManager.Instance.UnlockAchievement(Achievement.PartnersFightOverYou);
				}
			}
		}
		if (sparringPartner != null && SparringPartner != null && sparringPartner != SparringPartner)
		{
			SparringPartner.SetSparringPartner(SparringType.None, null, null);
			SetSparringPartner(SparringType.None, null, null);
		}
		if (sparringPartner != SparringPartner && SparringPartner != null && FindActiveGoal(GoalType.Attack) is Attack attack && attack.GetTargetCharacter() == SparringPartner)
		{
			attack.Finished = true;
		}
		SparringType = sparringType;
		SparringPartner = sparringPartner;
		SparringInstigator = instigator;
		SparringPartnerInitialBloodLoss = sparringPartner?.GetBloodLoss() ?? 0f;
		SparringSnowballHits = 0;
		SparringMemories.Clear();
		if (SparringType == SparringType.Feuding)
		{
			for (int i = 0; i < Memories.Count; i++)
			{
				if ((Memories[i].Actor == SparringPartner || (Memories[i].Prototype.RuleSet == MemoryRuleSet.Envy && Memories[i].Object == SparringPartner)) && !Memories[i].IsTrivial() && Memories[i].ApprovalContribution <= -10f)
				{
					SparringMemories.Add(Memories[i]);
				}
			}
		}
		if (SparringType == SparringType.Boxing)
		{
			for (int j = 0; j < 9; j++)
			{
				if (Clothes[j] is Armor)
				{
					if (SparringRemovedArmorIds == null)
					{
						SparringRemovedArmorIds = new List<int>();
					}
					SparringRemovedArmorIds.Add(Clothes[j].Id);
					Clothes[j].Strip(this);
				}
			}
		}
		else
		{
			if (sparringType2 != SparringType.Boxing || SparringRemovedArmorIds == null)
			{
				return;
			}
			for (int k = 0; k < Inventory.Count; k++)
			{
				Equipment item = Inventory.GetItem(k);
				if (SparringRemovedArmorIds.Contains(item.Id))
				{
					item.Wear(this);
				}
			}
			SparringRemovedArmorIds.Clear();
		}
	}

	public virtual void OnDie(float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, CauseOfDeath causeOfDeath, Character killer, SecrecyMode secret)
	{
		DirectControlled = false;
		DirectControlledCrouching = false;
		if (SparringPartner != null)
		{
			if (SparringPartner.IsPredicted() == IsPredicted())
			{
				SparringPartner.SetSparringPartner(SparringType.None, null, null);
			}
			SparringPartner = null;
		}
		ClearVelocityXZ();
		Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, causeOfDeath != CauseOfDeath.Trap && causeOfDeath != CauseOfDeath.Tripwire);
		CancelAllRoles();
		SetGoal(null);
		RemoveAllTargets();
		LeaveAllSquads();
		SetConsciousness(Consciousness.Dead);
		TimeOfDeath = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		Killer = killer;
		if (!Zombie || CauseOfDeath == CauseOfDeath.Other)
		{
			CauseOfDeath = causeOfDeath;
			PlaceOfDeath = Pos;
		}
		Fatigue = 0f;
		Adrenaline = 0f;
		if (Zombie && CheckFrontmostPrediction(PredictedEventType.ZombieSound))
		{
			PlayVoiceSoundFromList(SoundManager.ZombieDieSounds, VoiceSoundType.ZombieSnarl);
		}
		if (!IsAuthoritative())
		{
			return;
		}
		Feed.ClearFeed();
		foreach (Equipment content in Inventory.Contents)
		{
			content.Concealed = false;
		}
		if (Community != null)
		{
			Community.OnMemberDied(this, killer, secret);
		}
		if (!Zombie && this is Human)
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, null, SpeechSituation.Death);
			if (speechForSituation != null && Speak(speechForSituation) && SpeakingTextEnglish.Length <= 16)
			{
				SkipSpeech(onlySkipLipsMoving: true);
			}
		}
		UpdateThinkBucket();
		StoryManager.Instance.SetConditionsDirty();
		if (SquadId != 0)
		{
			SanitiseSquad();
		}
		InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(this);
		if (invaderInstanceThatCreatedHunter != null)
		{
			StoryManager.QueueEvents(invaderInstanceThatCreatedHunter.Invader.OnKilledEvents, null, null, this, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Killed, killer, this);
	}

	public static void LoadContent()
	{
		BloodDecalTex = new Resource<Texture2D>("Textures/Decals/BloodDecal");
		BloodDecalNormals = new Resource<Texture2D>("Textures/Decals/BloodDecal_H");
		CrackDecalTex = new Resource<Texture2D>("Textures/Decals/ArmorCrackDecal");
		CrackDecalNormals = new Resource<Texture2D>("Textures/Decals/ArmorCrackDecal_N");
		OverlayTex = new Resource<Texture2D>("Textures/Decals/ArmorCrackOverlay");
		OverlayNormal = new Resource<Texture2D>("Textures/Decals/ArmorCrackOverlay_N");
		InjuryArrow = new Resource<GameObject>("Prefabs/SpecialEffects/InjuryArrow");
		InjuryBurning = new Resource<GameObject>("Prefabs/SpecialEffects/InjuryBurning");
		UnconsciousStar = new Resource<GameObject>("Prefabs/HUD/UnconsciousStar");
	}

	public static void InitLayerMasks()
	{
		DefaultLayer = LayerMask.NameToLayer("Default");
		CharacterCapsuleLayer = LayerMask.NameToLayer("CharacterCapsule");
		TransparentFXLayer = LayerMask.NameToLayer("TransparentFX");
		IgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		NonPhysicalLayer = LayerMask.NameToLayer("NonPhysical");
		IconGimpLayer = LayerMask.NameToLayer("IconGimp");
		PreviewGimpLayer = LayerMask.NameToLayer("PreviewGimp");
		CharactersLayer = LayerMask.NameToLayer("Characters");
		WorldUILayer = LayerMask.NameToLayer("WorldUI");
		WaterLayer = LayerMask.NameToLayer("Water");
		RagdollsLayer = LayerMask.NameToLayer("Ragdolls");
		OutlinedLayer = LayerMask.NameToLayer("Outlined");
		InvisibleLayer = LayerMask.NameToLayer("Invisible");
		PipLayer = LayerMask.NameToLayer("Pip");
		GibLayer = LayerMask.NameToLayer("Gib");
		JuggernautLayer = LayerMask.NameToLayer("Juggernaut");
		SunLayer = LayerMask.NameToLayer("Sun");
	}

	private void UnitySetRigidBodiesEnabled(GameObject obj, bool enabled, bool retainVelocity, Vector3 vel)
	{
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = !enabled;
			obj.layer = (enabled ? RagdollsLayer : NonPhysicalLayer);
			if (enabled && retainVelocity)
			{
				if (!retainVelocity && vel.magnitude >= TrapMaxRagdollVel)
				{
					vel *= TrapMaxRagdollVel / vel.magnitude;
				}
				component.linearVelocity = vel * 60f;
			}
		}
		_ = obj.transform.localScale.sqrMagnitude;
		_ = 0f;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			UnitySetRigidBodiesEnabled(gameObject, enabled, retainVelocity, vel);
		}
	}

	public void UnityUpdateRagdollEnabled(bool retainVelocity)
	{
		UnityUpdateRagdollEnabled(retainVelocity, GetOldVelocity());
	}

	public void UnityUpdateRagdollEnabled(bool retainVelocity, Vector3 vel)
	{
		Unity.Animator.enabled = CurrentAnimState == AnimState.Animation;
		Unity.CollisionShape.SetActive(CurrentAnimState == AnimState.Animation || CurrentAnimState == AnimState.RecoverFromRagdoll_Back || CurrentAnimState == AnimState.RecoverFromRagdoll_Front);
		if (Unity.UmaExpressionPlayer != null)
		{
			Unity.UmaExpressionPlayer.enabled = CurrentAnimState == AnimState.Animation;
		}
		UnitySetRigidBodiesEnabled(Unity.Obj, CurrentAnimState == AnimState.Ragdoll, retainVelocity, vel);
		Unity.UnityAnimState = CurrentAnimState;
	}

	public void Ragdollify(float hitRadius, Vector3 hitPos, Vector3 hitForce, Bone bone, Vector3 posInBoneSpace, bool fromStumble, bool retainVelocity)
	{
		Ragdollify(hitRadius, hitPos, hitForce, bone, posInBoneSpace, fromStumble, retainVelocity, GetOldVelocity());
	}

	public void Ragdollify(float hitRadius, Vector3 hitPos, Vector3 hitForce, Bone bone, Vector3 posInBoneSpace, bool fromStumble, bool retainVelocity, Vector3 vel)
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if (InsideBuilding != null)
		{
			InhabitantSlotDef inhabitantSlotDef = InsideBuilding.GetInhabitantSlotDef(GetAuthoritativeOrElseThisCharacter());
			if (inhabitantSlotDef.External)
			{
				if (new TerrainRect(InsideBuilding.ExtentsMin, InsideBuilding.ExtentsMax).TilesArea > 1)
				{
					RagdollIgnoreBuilding = InsideBuilding;
				}
				else if (inhabitantSlotDef.Pos.y > 1f)
				{
					hitForce = MathUtil.ToX0Y(MathUtil.GetDirFromAngle(InsideBuilding.GetEntranceAngle(0))) * PushOutOfWatchTowerForce;
					bone = Bone.Spine;
					posInBoneSpace = Vector3.zero;
				}
			}
			InsideBuilding.OnCharacterLeave(this, 0, fromBuildingDestroyed: false, IsOutdoors());
		}
		Drop();
		bool num = CheckFrontmostPrediction(PredictedEventType.Ragdollify);
		if (num && !Disappeared && !IsUnityObjectActive())
		{
			UnityActivate();
		}
		CurrentAnimState = AnimState.Ragdoll;
		SentRagdollStopMoving = false;
		RagdollFromStumble = fromStumble;
		ClearActionAnim();
		if (num && !Disappeared && Unity.Animator != null)
		{
			UnityUpdateRagdollEnabled(retainVelocity, vel);
			if (hitForce.sqrMagnitude > 0f)
			{
				while (bone != Bone.Invalid)
				{
					string unityBoneName = GetUnityBoneName(bone);
					GameObject gameObject = null;
					if (unityBoneName != null)
					{
						if (unityBoneName.Length == 0)
						{
							gameObject = Unity.Obj;
						}
						else
						{
							GameObject gameObject2 = Unity.Obj.FindChild(unityBoneName);
							if (gameObject2 == null)
							{
								Debug.LogError("Incorrect bone name: " + unityBoneName);
							}
							gameObject = gameObject2.gameObject;
						}
					}
					Rigidbody rigidbody = ((gameObject != null) ? gameObject.GetComponent<Rigidbody>() : null);
					if (rigidbody != null)
					{
						rigidbody.AddForceAtPosition(hitForce, hitPos);
						break;
					}
					bone--;
				}
			}
		}
		if (currentActionAnim == ActionAnim.HandsUp && Community != null && IsAuthoritative())
		{
			Community.UpdateCachedAStarInfo();
		}
	}

	public void RecoverFromRagdoll(bool back)
	{
		CurrentAnimState = (back ? AnimState.RecoverFromRagdoll_Back : AnimState.RecoverFromRagdoll_Front);
		RagdollIgnoreBuilding = null;
		ClearActionAnim();
		if (Unity.IsValid() && IsUnityObjectActive())
		{
			UnitySaveProneBoneTransforms();
		}
	}

	public void SetProne(bool back)
	{
		CurrentAnimState = (back ? AnimState.Prone_Back : AnimState.Prone_Front);
		RagdollIgnoreBuilding = null;
		ClearActionAnim();
		if (Unity.IsValid())
		{
			UnitySaveProneBoneTransforms();
		}
	}

	public void SetStanding()
	{
		CurrentAnimState = AnimState.Animation;
		RagdollIgnoreBuilding = null;
		ClearActionAnim();
		TryStartActionAnim(ActionAnim.None, null);
	}

	private void UnitySaveProneBoneTransforms()
	{
		for (int i = 0; i < 24; i++)
		{
			GameObject unityBone = GetUnityBone((Bone)i);
			ProneBoneTransforms[i] = ((unityBone != null) ? new Trans(unityBone.transform.localPosition, unityBone.transform.localScale.x, unityBone.transform.localRotation) : Trans.identity);
			MathUtil.CheckNaNorInfinity(ProneBoneTransforms[i]);
		}
		if (IsPredicted())
		{
			Array.Copy(ProneBoneTransforms, Authoritative.ProneBoneTransforms, ProneBoneTransforms.Length);
		}
	}

	private void UnityLoadProneBoneTransforms()
	{
		for (int i = 0; i < 24; i++)
		{
			GameObject unityBone = GetUnityBone((Bone)i);
			if (unityBone != null)
			{
				MathUtil.CheckNaNorInfinity(ProneBoneTransforms[i]);
				if (ProneBoneTransforms[i].Scale == 0f)
				{
					Debug.LogWarning("ProneBoneTransforms scale is 0");
					ProneBoneTransforms[i].Scale = 1f;
				}
				unityBone.transform.localPosition = ProneBoneTransforms[i].Pos;
				unityBone.transform.localScale = Vector3.one * ProneBoneTransforms[i].Scale;
				unityBone.transform.localRotation = ProneBoneTransforms[i].Orientation;
			}
		}
	}

	public bool IsRagdoll()
	{
		return CurrentAnimState == AnimState.Ragdoll;
	}

	public bool IsRagdollOrProneOrRecovering()
	{
		return CurrentAnimState != AnimState.Animation;
	}

	public bool IsRecoveringFromRagdoll()
	{
		if (CurrentAnimState != AnimState.RecoverFromRagdoll_Back)
		{
			return CurrentAnimState == AnimState.RecoverFromRagdoll_Front;
		}
		return true;
	}

	public bool IsProne()
	{
		if (CurrentAnimState != AnimState.Prone_Back)
		{
			return CurrentAnimState == AnimState.Prone_Front;
		}
		return true;
	}

	public bool IsGettingUp()
	{
		if (CurrentActionAnim != ActionAnim.GetUp_Back)
		{
			return CurrentActionAnim == ActionAnim.GetUp_Front;
		}
		return true;
	}

	private bool IsRagdollMoving(GameObject obj, float tolerance)
	{
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null && component.linearVelocity.sqrMagnitude >= tolerance * tolerance)
		{
			return true;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			if (IsRagdollMoving(gameObject, tolerance))
			{
				return true;
			}
		}
		return false;
	}

	public void GetPosFromRagdoll(out Vector3 pos, out bool back, out float facingAngle)
	{
		Character predictedOrElseThisCharacter = GetPredictedOrElseThisCharacter();
		pos = Pos;
		back = true;
		facingAngle = FacingAngle;
		if (predictedOrElseThisCharacter.IsUnityObjectActive())
		{
			Matrix4x4 unityBoneTransform = predictedOrElseThisCharacter.GetUnityBoneTransform(Bone.Hips);
			pos = unityBoneTransform.Translation();
			back = unityBoneTransform.Forward().y >= 0f;
			facingAngle = ((unityBoneTransform.Right().y >= 0f - Hud.Cos45) ? MathUtil.GetAngleFromDir(MathUtil.ToXZ(unityBoneTransform.Right()) * (back ? 1f : (-1f)), FacingAngle) : MathUtil.GetAngleFromDir(MathUtil.ToXZ(unityBoneTransform.Forward()), FacingAngle));
		}
		pos = GameTerrain.Instance.ClampPosWithinBounds(pos, 1f);
	}

	public override void UnitySendPhysicsStateToClients(InputFrame inputFrame)
	{
		if (CurrentAnimState == AnimState.Ragdoll && !SentRagdollStopMoving && Session.Instance.IsPartyLeader())
		{
			GameTerrain instance = GameTerrain.Instance;
			TimeSpan timeSpan = Session.Instance.PlayTime - AnimStartTime;
			Character predictedOrElseThisCharacter = GetPredictedOrElseThisCharacter();
			GetPosFromRagdoll(out var pos, out var back, out var facingAngle);
			if (timeSpan >= (IsAwake ? AliveMinRagdollTime : DeadMinRagdollTime) && !IsBeingCarried() && (RagdollFromStumble || Infection == InfectionType.Invisible || timeSpan >= (IsAwake ? AliveMaxRagdollTime : (instance.IsSlopeWithinInBounds(instance.GetTileCoordForPos(pos)) ? DeadOnSlopeMaxRagdollTime : DeadMaxRagdollTime)) || !IsRagdollMoving(predictedOrElseThisCharacter.Unity.Obj, RagdollStopMovingTolerance)))
			{
				inputFrame.AddAction(InputAction.RagdollStopMoving(this, MathUtil.ToXZ(pos), back, facingAngle));
				SentRagdollStopMoving = true;
			}
			else
			{
				inputFrame.AddAction(InputAction.RagdollMove(this, MathUtil.ToXZ(pos), facingAngle));
			}
		}
	}

	public void OnRagdollMove(Vector2 posXZ, float facingAngle)
	{
		SetPosition(posXZ.x, posXZ.y);
		SetFacingAngle(facingAngle);
		SetFollowMePos(Vector2.zero);
	}

	public void OnRagdollStopMoving(Vector2 posXZ, bool back, float facingAngle)
	{
		OnRagdollMove(posXZ, facingAngle);
		if (Unity.IsValid())
		{
			Transform transform = GetUnityBone((this is Chicken) ? Bone.Global : Bone.Hips).transform;
			Vector3 position = transform.position;
			Quaternion rotation = transform.rotation;
			Unity.Obj.transform.localPosition = Position + new Vector3(0f, AnimHeight, 0f);
			Unity.Obj.transform.localRotation = Quaternion.Euler(0f, FacingAngle * 57.29578f, 0f);
			transform.position = position;
			transform.rotation = rotation;
		}
		if (Consciousness == Consciousness.Conscious)
		{
			RecoverFromRagdoll(back);
		}
		else
		{
			SetProne(back);
		}
		if (Predicted != null)
		{
			Predicted.OnRagdollStopMoving(posXZ, back, facingAngle);
			Array.Copy(Predicted.ProneBoneTransforms, ProneBoneTransforms, 24);
			for (int i = 0; i < ProneBoneTransforms.Length; i++)
			{
				MathUtil.CheckNaNorInfinity(ProneBoneTransforms[i]);
			}
		}
	}

	public Squad GetSquad()
	{
		if (SquadId == 0 || Community == null)
		{
			return null;
		}
		return Community.GetSquad(SquadId);
	}

	public void SanitiseSquad()
	{
		if (SquadId != 0 && (!Alive || (Zombie && Community != null && !Community.IsZombieCommunity())))
		{
			Debug.LogWarning((Disappeared ? "Disappeared" : "Dead") + " character still in squad? " + GetDisplayNameString() + ", Alive: " + Alive + ", Zombie: " + Zombie + ", Community: " + ((Community != null) ? Community.GetDisplayNameString() : "None") + ", Type: " + ((Community != null) ? Community.CommunityType.ToString() : "None"));
			if (Community != null)
			{
				Community.RemoveFromSquad(this);
			}
			SquadId = 0;
		}
	}

	public void Zombify(InfectionType infectionType)
	{
		if (infectionType == InfectionType.None)
		{
			return;
		}
		if (IsAuthoritative() && SquadId != 0 && Community != null)
		{
			Community.RemoveFromSquad(this);
		}
		InfectionProgression = 1f;
		Infection = infectionType;
		SetConsciousness(Consciousness.Conscious);
		BloodLoss = 0f;
		BodyTemperatureInCelsius = BodyTemperatureInCelsiusNormal;
		ClearActionAnim();
		Character predictedOrElseThisCharacter = GetPredictedOrElseThisCharacter();
		if (predictedOrElseThisCharacter.Unity.Animator != null)
		{
			predictedOrElseThisCharacter.Unity.Animator.runtimeAnimatorController = AnimationManager.Instance.UnityZombieAnimatorController;
			if (predictedOrElseThisCharacter.CurrentAnimState == AnimState.Animation)
			{
				predictedOrElseThisCharacter.UnityPlayInitialAnim();
			}
		}
		if (HasUnityObject())
		{
			UnitySetupAppearance();
		}
		if (IsAuthoritative())
		{
			SetGoal(new ZombieGoal());
			Feed.ClearFeed();
			UpdateCachedAStarInfo(force: true);
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public void ActivateInvisibleStrain()
	{
		bool flag = IsPredicted();
		if (SparringPartner != null)
		{
			if (SparringPartner.IsPredicted() == IsPredicted())
			{
				SparringPartner.SetSparringPartner(SparringType.None, null, null);
			}
			SparringPartner = null;
		}
		if (!flag && SquadId != 0)
		{
			Community.RemoveFromSquad(this);
		}
		if (InsideBuilding != null)
		{
			InsideBuilding.OnCharacterLeave(this, 0, fromBuildingDestroyed: false, fromRagdolled: false);
		}
		if (!flag)
		{
			CancelAllRoles();
		}
		string goalDebugString = GetGoalDebugString();
		SetGoal(null);
		SanityCheckTargetRefCounts(onLoadingThread: false, goalDebugString);
		CauseOfDeath = CauseOfDeath.Zombie;
		TimeOfDeath = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		Zombify(InfectionType.Invisible);
		InvisibleStrainJustActivated = PredictedObjectManager.Instance.GetCurrentTime(flag);
		SedativeEffect = 0f;
		if (flag)
		{
			return;
		}
		StoryManager.Instance.DramaticDeathCharacter = this;
		StoryManager.Instance.SetConditionsDirty();
		if (Community != null && Community.CommunityType != CommunityType.Psycho)
		{
			if (HadInvisibleStrainFromStart)
			{
				Memory.OnMemorableEvent(MemoryPrototype.HasInvisibleStrain, this, null, 1f, SecrecyMode.OnlyKnownToSubjectCommunity);
			}
			else
			{
				Memory.OnMemorableEvent(MemoryPrototype.ContractedInvisibleStrain, null, this, 1f, SecrecyMode.OnlyKnownToObjectCommunity);
			}
		}
		List<Character> list = new List<Character>();
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == MemoryPrototype.AccusedOfInvisibleStrain && Memories[i].Object == this)
			{
				list.Add(Memories[i].Actor);
			}
		}
		foreach (Character item in list)
		{
			Memory.OnMemorableEvent(MemoryPrototype.AccusedOfInvisibleStrainCorrectly, item, this, 1f, SecrecyMode.Public);
		}
		Feed.ClearFeed();
		if (Community != null)
		{
			Community.OnMemberDied(this, null, SecrecyMode.Public, justActivatedInvisibleStrain: true);
		}
	}

	public bool WillActivateDoomStrain(bool checkEnough = true)
	{
		if (!AliveAndNotZombie)
		{
			return false;
		}
		if (GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (Disappeared)
		{
			return false;
		}
		if (IsPlayerAvatar())
		{
			return false;
		}
		if (InvisibleStrain != InvisibleStrainType.None)
		{
			return true;
		}
		if (UniqueID == EmmaOConnor)
		{
			return false;
		}
		Session instance = Session.Instance;
		if (MathUtil.RandomChoice((float)(Id ^ instance.GameUniqueId) * 6.66f, IsInPlayerCommunity() ? 0.3f : 0.7f))
		{
			return true;
		}
		if (Community == null)
		{
			Debug.Log(GetDisplayNameString());
		}
		if (checkEnough && Community != null)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			foreach (Character member in Community.Members)
			{
				if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
				{
					num3++;
					if (member.WillActivateDoomStrain(checkEnough: false))
					{
						num++;
					}
					if (!member.IsPlayerAvatar() && MathUtil.JenkinsHash((uint)(member.Id ^ instance.GameUniqueId)) > MathUtil.JenkinsHash((uint)(Id ^ instance.GameUniqueId)))
					{
						num2++;
					}
				}
			}
			int num4 = Mathf.CeilToInt((float)num3 * 0.3f);
			if (num < num4 && num2 < num4)
			{
				return true;
			}
		}
		return false;
	}

	public bool WillActivateInvisibleStrain()
	{
		if (!Zombie && InvisibleStrain != InvisibleStrainType.None && GetBaseObjectType() == BaseObjectType.Human)
		{
			if (AlwaysActivateInvisibleStrainWhenNearDeath)
			{
				return true;
			}
			if (Community != null && Community.CommunityType == CommunityType.Psycho)
			{
				return true;
			}
			if (AlwaysActivateInvisibleStrain)
			{
				return true;
			}
			return MathUtil.RandomChoice((int)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).TotalSeconds / MathUtil.RandomInt(Id, 10, 600) + Id * 238, 0.5f);
		}
		return false;
	}

	public float ApplyBloodLoss(float bloodLoss, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, CauseOfDeath causeOfDeath, Character attacker)
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Constitution);
		return ApplyBloodLoss2(bloodLoss, hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, causeOfDeath, attacker, skillLevelWithEffects, SecrecyMode.Public);
	}

	public float ApplyBloodLoss2(float bloodLoss, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, CauseOfDeath causeOfDeath, Character attacker, int constitution, SecrecyMode secret)
	{
		if (BloodLoss + bloodLoss >= 0.95f && WillActivateInvisibleStrain())
		{
			bloodLoss = Math.Max(0f, 0.95f - BloodLoss + 0.0001f);
		}
		if (LimitBloodLoss > 0f && BloodLoss + bloodLoss >= LimitBloodLoss)
		{
			bloodLoss = Math.Max(0f, LimitBloodLoss - BloodLoss);
		}
		float num = 1f + (float)((!Zombie) ? constitution : 0);
		if (BloodLoss + bloodLoss >= num)
		{
			bloodLoss = num - BloodLoss;
			BloodLoss = num;
			OnDie(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, causeOfDeath, attacker, secret);
		}
		else
		{
			BloodLoss += bloodLoss;
		}
		return bloodLoss;
	}

	public bool IsDeathSignificant(Character killer)
	{
		if (Zombie)
		{
			return false;
		}
		if (GetBaseObjectType() == BaseObjectType.Human)
		{
			return true;
		}
		if (Community == null || Community.IsAnimalCommunity())
		{
			return false;
		}
		if (killer != null && killer.Community != Community && !killer.Zombie)
		{
			return true;
		}
		return false;
	}

	public void TriggerHurtMemory(Character source, bool assailantUnknown, float damage, SecrecyMode secret, SparringType sparringType, bool intentional)
	{
		if (!IsDeathSignificant(source) || source.Zombie)
		{
			return;
		}
		bool flag = IsLooter() && !source.IsLooter() && GetBaseObjectType() == BaseObjectType.Human && Memory.IsKnownToSubjectCommunity(secret) && Community != null && Community.GetRelationship(source.Community) == CommunityRelationshipType.Hostile;
		if (source.SparringType == SparringType.FightToTheDeath && source.SparringPartner == this)
		{
			Memory.OnMemorableEvent(MemoryPrototype.HurtPossibleInvisibleStrain, source, this, damage, SecrecyMode.Public);
		}
		else if (source.Community == Community && Community != null && sparringType == SparringType.None && secret == SecrecyMode.Public && !intentional)
		{
			Memory.OnMemorableEvent(MemoryPrototype.HurtAccidentally, source, this, damage, SecrecyMode.Public);
		}
		else if (assailantUnknown && secret == SecrecyMode.Public)
		{
			Memory.OnMemorableEvent(MemoryPrototype.Hurt, null, this, damage, SecrecyMode.OnlyKnownToObjectCommunity);
			if (!flag)
			{
				Memory.OnMemorableEvent(MemoryPrototype.Hurt, source, this, damage, SecrecyMode.IgnoredByObjectCommunity);
			}
		}
		else
		{
			Memory.OnMemorableEvent(MemoryPrototype.Hurt, source, this, damage, (flag && Memory.IsKnownToObjectCommunity(secret)) ? SecrecyMode.OnlyKnownToObjectCommunity : secret);
		}
	}

	public void TriggerKilledMemory(Character source, bool assailantUnknown, SecrecyMode secret, SparringType sparringType, TileObject intendedTarget, bool assassinate, bool intentional)
	{
		if (secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.IgnoredByObjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToSubject && secret != SecrecyMode.OnlyKnownToObjectCommunity && secret != SecrecyMode.Private)
		{
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Death, Position, 0f, GetMaxSoundVisibilityRange(), this, source, this, intendedTarget));
		}
		if (Zombie)
		{
			Memory.OnMemorableEvent(MemoryPrototype.KilledZombie, source, null, 1f, secret);
		}
		else
		{
			bool flag = IsLooter() && !source.IsLooter() && GetBaseObjectType() == BaseObjectType.Human && Memory.IsKnownToSubjectCommunity(secret) && Community != null && Community.GetRelationship(source.Community) == CommunityRelationshipType.Hostile;
			if (flag)
			{
				Memory.OnMemorableEvent(MemoryPrototype.KilledLooter, source, null, null, 1f, secret, null, fakeNews: false, null, Community);
			}
			if (IsDeathSignificant(source))
			{
				if (sparringType == SparringType.FightToTheDeath || (assassinate && IsInMyCommunityOrAlly(source) && HasMemoryAfter(MemoryPrototype.AccusedOfInvisibleStrain, source, this, Session.Instance.PlayTime - Sun.DayLength)))
				{
					Memory.OnMemorableEvent(MemoryPrototype.KilledPossibleInvisibleStrain, source, this, 1f, SecrecyMode.Public);
				}
				else if (source.Community == Community && Community != null && sparringType == SparringType.None && secret == SecrecyMode.Public && !intentional)
				{
					Memory.OnMemorableEvent(MemoryPrototype.KilledAccidentally, source, this, 1f, SecrecyMode.Public);
				}
				else if (assailantUnknown && secret == SecrecyMode.Public)
				{
					Memory.OnMemorableEvent(MemoryPrototype.Killed, null, this, 1f, SecrecyMode.OnlyKnownToObjectCommunity);
					if (!flag)
					{
						Memory.OnMemorableEvent(MemoryPrototype.Killed, source, this, 1f, SecrecyMode.IgnoredByObjectCommunity);
					}
				}
				else
				{
					Memory.OnMemorableEvent(MemoryPrototype.Killed, source, this, 1f, (flag && Memory.IsKnownToObjectCommunity(secret)) ? SecrecyMode.OnlyKnownToObjectCommunity : secret);
				}
			}
		}
		source.AddExcitement(Zombie ? 0.25f : 0.5f);
	}

	public void Heal(float amount)
	{
		if (Alive)
		{
			BloodLoss = Math.Max(BloodLoss - amount, 0f);
		}
	}

	private void AbsorbDamage(Character source, ref InjuryType injuryType, InjuryLocation injuryLocation, EquipmentPrototype ammoType, Bone bone, Vector3 posInBoneSpace, ClothingType clothingType, ref float damage, ref bool absorbedByArmor)
	{
		if (!(Clothes[(int)clothingType] is Armor armor))
		{
			return;
		}
		InjuryType injuryType2 = injuryType;
		float damageAbsorption = armor.GetDamageAbsorption();
		float num = damage / damageAbsorption;
		float num2 = ((num > armor.Protection) ? (armor.Protection * damageAbsorption) : damage);
		if (injuryType == InjuryType.BluntObject || injuryType == InjuryType.Punch)
		{
			num2 *= BluntDamageAbsorption;
			num *= BluntDamageAbsorption;
		}
		else if (injuryType == InjuryType.SharpObject)
		{
			num2 *= SharpDamageAbsorption;
			num *= SharpDamageAbsorption;
			if (num < armor.Protection)
			{
				injuryType = InjuryType.BluntObject;
			}
		}
		if (ammoType != null)
		{
			num *= 1f + ammoType.ArmorPiercing;
			num2 *= 1f - ammoType.ArmorPiercing;
		}
		if (IsAuthoritative())
		{
			armor.Protection = Math.Max(0f, armor.Protection - num);
		}
		damage -= num2;
		absorbedByArmor = damage <= 0f;
		if (IsAuthoritative())
		{
			int num3 = ((injuryLocation == InjuryLocation.Torso) ? 8 : 4);
			float num4 = 0f;
			if (armor.ArmorDamagePoints.Count >= num3)
			{
				num4 += armor.ArmorDamagePoints[0].DamageAmount;
				armor.ArmorDamagePoints.RemoveAt(0);
			}
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			armor.ArmorDamagePoints.Add(new Injury(this, injuryType, absorbedByArmor, injuryLocation, InfectionType.None, bone, posInBoneSpace, damage + num4, currentTime, source, assailantUnknown: false, intentional: true));
		}
		WantUnityUpdateDecals = true;
		WantUnityUpdateInjuries = true;
		CanSkipUnityUpdate = false;
		if (source == null || !source.DirectControlled || source != Hud.Instance.LocalControlledCharacter)
		{
			return;
		}
		if (injuryType2 == InjuryType.SharpObject && num2 > 0f)
		{
			if (HintManager.Instance.Hints[38].CanShowHint())
			{
				HintManager.Instance.Hints[38].StartShowing(GameImpl.Translate(HintManager.HINT_UseBluntWeaponsAgainstArmor));
			}
		}
		else if (injuryType2 == InjuryType.BluntObject && !absorbedByArmor && HintManager.Instance.Hints[38].LastShown != TimeSpan.Zero)
		{
			HintManager.Instance.Hints[38].Hide();
			HintManager.Instance.Hints[38].MarkPerformed();
		}
		if (injuryType2 != InjuryType.Arrow && injuryType2 != InjuryType.Bullet)
		{
			return;
		}
		if ((ammoType != null && ammoType.ArmorPiercing == 0f) & absorbedByArmor)
		{
			if (HintManager.Instance.Hints[37].CanShowHint())
			{
				HintManager.Instance.Hints[37].StartShowing(GameImpl.Translate(HintManager.HINT_UseArmorPiercingAmmo));
			}
		}
		else if (ammoType != null && ammoType.ArmorPiercing > 0f && !absorbedByArmor && HintManager.Instance.Hints[37].LastShown != TimeSpan.Zero)
		{
			HintManager.Instance.Hints[37].Hide();
			HintManager.Instance.Hints[37].MarkPerformed();
		}
	}

	public virtual float GetMaxLimbDamage()
	{
		return Infection switch
		{
			InfectionType.Green => 0.001f, 
			InfectionType.Blue => 0.05f, 
			InfectionType.Red => 0.1f, 
			InfectionType.White => 0.1f, 
			InfectionType.Invisible => 0.5f, 
			_ => MaxLimbDamage, 
		};
	}

	private void GetModifiedDamageToMe(Character source, ref float damage, InjuryType injuryType, ref InjuryLocation injuryLocation, ref bool isStealthAttack)
	{
	}

	private float GetModifiedDamageToMe2(Character source, float damage, ref InjuryType injuryType, InjuryLocation injuryLocation, EquipmentPrototype ammoType, TargettableBodyLocation targetBodyLocation, Bone bone, Vector3 posInBoneSpace, bool isStealthAttack, out bool absorbedByVest)
	{
		absorbedByVest = false;
		if (IsSmallAnimal())
		{
			return 1f;
		}
		switch (Infection)
		{
		case InfectionType.Green:
			damage *= 1f;
			break;
		case InfectionType.Blue:
			damage *= 0.5f;
			break;
		case InfectionType.Red:
			damage *= 0.25f;
			break;
		case InfectionType.White:
			damage *= 0.175f;
			break;
		case InfectionType.Invisible:
			damage *= 0.15f;
			break;
		case InfectionType.None:
			if (injuryLocation != InjuryLocation.Head)
			{
				float t = (float)GetSkillLevelWithEffects(SkillType.Constitution) / 5f;
				damage *= Mathf.Lerp(1f, (injuryType == InjuryType.BluntObject || injuryType == InjuryType.Punch) ? 0.25f : 0.5f, t);
			}
			break;
		}
		if (injuryType != InjuryType.Explosion && injuryType != InjuryType.TrapExplosion && injuryType != InjuryType.Fire)
		{
			switch (injuryLocation)
			{
			case InjuryLocation.Head:
				if (targetBodyLocation == TargettableBodyLocation.Head)
				{
					damage = ((injuryType != InjuryType.Punch) ? (damage * 2f) : (damage * 1.25f));
				}
				break;
			default:
				if (injuryType != InjuryType.Punch)
				{
					damage *= 0.5f;
				}
				break;
			case InjuryLocation.Torso:
				break;
			}
		}
		if (isStealthAttack)
		{
			float num = Mathf.Lerp(1f, 3f, (float)source.GetSkillLevelWithEffects(SkillType.Stealth) / 5f);
			damage *= num;
		}
		if (injuryType != InjuryType.Fire && injuryType != InjuryType.ZombieBite && injuryType != InjuryType.VehicleImpact)
		{
			switch (injuryLocation)
			{
			case InjuryLocation.Torso:
			case InjuryLocation.LeftArm:
			case InjuryLocation.RightArm:
				AbsorbDamage(source, ref injuryType, injuryLocation, ammoType, bone, posInBoneSpace, ClothingType.BodyArmor, ref damage, ref absorbedByVest);
				break;
			case InjuryLocation.LeftLeg:
			case InjuryLocation.RightLeg:
				AbsorbDamage(source, ref injuryType, injuryLocation, ammoType, bone, posInBoneSpace, ClothingType.LegArmor, ref damage, ref absorbedByVest);
				break;
			case InjuryLocation.Head:
				AbsorbDamage(source, ref injuryType, injuryLocation, ammoType, bone, posInBoneSpace, ClothingType.Hat, ref damage, ref absorbedByVest);
				break;
			}
		}
		if (injuryType != InjuryType.Explosion && injuryType != InjuryType.TrapExplosion && injuryType != InjuryType.Fire && (injuryLocation == InjuryLocation.LeftLeg || injuryLocation == InjuryLocation.RightLeg) && (source != SparringPartner || SparringType != SparringType.Boxing))
		{
			float legDamage = GetLegDamage();
			float num2 = Math.Max(0f, GetMaxLimbDamage() - legDamage);
			if (num2 == 0f && source != null && source.DirectControlled && GameImpl.Instance.Settings.HintsEnabled)
			{
				HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate(HINT_LegShotDamageLimit));
			}
			damage = Math.Min(damage, num2);
		}
		damage = Math.Min(damage, 1.01f);
		return damage;
	}

	public void OnMeleeAttacked(Character source, TileObject intendedTarget, InjuryType injuryType, InjuryLocation injuryLocation, InfectionType infectionType, Vector3 nonDeterministicAttackPos, Vector3 attackDir, Bone bone, Vector3 hitPosInBoneSpace, float damage, out bool absorbedByArmor, bool dontReact, bool assassinate, bool stealthy, TargettableBodyLocation targetBodyLocation, bool small)
	{
		SecrecyMode secrecy = (stealthy ? SecrecyMode.IgnoredByObjectCommunity : SecrecyMode.Public);
		OnMeleeAttacked2(source, intendedTarget, injuryType, injuryLocation, infectionType, nonDeterministicAttackPos, attackDir, bone, hitPosInBoneSpace, damage, out absorbedByArmor, dontReact, assassinate, secrecy, stealthy, targetBodyLocation, small);
	}

	public void OnMeleeAttacked2(Character source, TileObject intendedTarget, InjuryType injuryType, InjuryLocation injuryLocation, InfectionType infectionType, Vector3 nonDeterministicAttackPos, Vector3 attackDir, Bone bone, Vector3 hitPosInBoneSpace, float damage, out bool absorbedByArmor, bool dontReact, bool assassinate, SecrecyMode secrecy, bool stealthy, TargettableBodyLocation targetBodyLocation, bool small)
	{
		if (!Alive)
		{
			absorbedByArmor = false;
			return;
		}
		GetTarget(source)?.ClearInaccessible();
		OnDamaged(source, intendedTarget, ref injuryType, injuryLocation, infectionType, null, SkillType.HandToHand, ref damage, MeleeHitRadius, nonDeterministicAttackPos, attackDir * MeleeHitForce, bone, hitPosInBoneSpace, dontReact, assassinate, secrecy, targetBodyLocation, 0f, out absorbedByArmor);
		if (CheckFrontmostPrediction(PredictedEventType.OnMeleeAttacked) && IsUnityObjectActive() && injuryType == InjuryType.SharpObject && !absorbedByArmor)
		{
			small = small || damage <= 0f;
			GameObject unityBone = GetUnityBone(bone);
			Vector3 vector = GetUnityBoneTransform(bone).MultiplyPoint(hitPosInBoneSpace);
			SpecialEffectManager.Instance.SpawnBloodEffect(vector, (nonDeterministicAttackPos - vector).normalized, unityBone, small);
			GameObject unityBone2 = source.GetUnityBone(Bone.MeleeWeapon);
			if (unityBone2 != null)
			{
				SpecialEffectManager.Instance.SpawnBloodEffect(vector, attackDir, unityBone2, small);
			}
		}
	}

	private bool ApplyRagdoll(bool legs, bool moving, Character source, InjuryType injuryType, TargettableBodyLocation targetBodyLocation)
	{
		if (injuryType == InjuryType.Punch)
		{
			return false;
		}
		float num = 0f;
		switch (Infection)
		{
		case InfectionType.None:
		case InfectionType.Green:
			num = (moving ? 0.001f : 0.25f);
			break;
		case InfectionType.Blue:
			num = (moving ? 0.125f : 0.25f);
			break;
		case InfectionType.Red:
		case InfectionType.White:
		case InfectionType.Invisible:
			num = ((moving && legs) ? 0.125f : 0.25f);
			break;
		}
		if (moving && ShouldLimp())
		{
			num = 0.001f;
		}
		if (targetBodyLocation == TargettableBodyLocation.Legs)
		{
			num = 0f;
		}
		float num2 = 0f;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (!Injuries[i].Ragdolled && (!legs || Injuries[i].IsLegs()))
			{
				num2 += Injuries[i].DamageAmount;
			}
		}
		if (num2 >= num)
		{
			for (int j = 0; j < Injuries.Count; j++)
			{
				if (!Injuries[j].Ragdolled && (!legs || Injuries[j].IsLegs()))
				{
					Injuries[j] = Injuries[j].ApplyRagdoll();
				}
			}
			return true;
		}
		return false;
	}

	public void OnDamaged(Character source, TileObject intendedTarget, InjuryType injuryType, InjuryLocation injuryLocation, SkillType skillType, ref float damage, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, bool dontReact, bool assassinate, SecrecyMode secret, TargettableBodyLocation targetBodyLocation, float fuel, out bool absorbedByVest)
	{
		absorbedByVest = false;
	}

	public void OnDamaged(Character source, TileObject intendedTarget, InjuryType injuryType, InjuryLocation injuryLocation, InfectionType infectionType, EquipmentPrototype ammoType, SkillType skillType, ref float damage, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, bool dontReact, bool assassinate, SecrecyMode secret)
	{
		TargettableBodyLocation targetBodyLocation = TargettableBodyLocation.Torso;
		OnDamaged(source, intendedTarget, ref injuryType, injuryLocation, infectionType, ammoType, skillType, ref damage, hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, dontReact, assassinate, secret, targetBodyLocation, 0f, out var _);
	}

	public virtual float GetShoutVoiceRadius()
	{
		return 0f;
	}

	public virtual float GetMaxSoundVisibilityRange()
	{
		return 0f;
	}

	public virtual float GetMaxVisibilityRangeToZombies()
	{
		return 32f;
	}

	public void OnDamaged(Character source, TileObject intendedTarget, ref InjuryType injuryType, InjuryLocation injuryLocation, InfectionType infectionType, EquipmentPrototype ammoType, SkillType skillType, ref float damage, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, bool dontReact, bool assassinate, SecrecyMode secret, TargettableBodyLocation targetBodyLocation, float fuel, out bool absorbedByVest)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (source != null)
		{
			if (IsPredicted() && source.IsAuthoritative())
			{
				source = null;
			}
			if (source == this)
			{
				Debug.LogWarning(GetDisplayNameString() + " damaged themselves: " + GetGoalDebugString());
				source = null;
			}
		}
		bool isStealthAttack = source != null && !IsControllableByPlayer() && !IsHighAlert() && !IsBeingBittenOrChoked() && GetBaseObjectType() != BaseObjectType.Chicken;
		if (isStealthAttack && source.DirectControlled && source == Hud.Instance.LocalControlledCharacter && (Zombie || GetBaseObjectType() != BaseObjectType.Human))
		{
			HintManager.Instance.Hints[33].MarkPerformed();
		}
		if (source != null && !isStealthAttack && !assassinate)
		{
			source.LastAttackedSomeoneTime = currentTime;
		}
		absorbedByVest = false;
		if (!Alive || God)
		{
			return;
		}
		if (source != null && source.DirectControlled && targetBodyLocation == TargettableBodyLocation.Legs)
		{
			PlayerRecord playerControllingMe = source.GetPlayerControllingMe();
			if (playerControllingMe != null && playerControllingMe.IsLocal)
			{
				HintManager.Instance.Hints[1].MarkPerformed();
			}
		}
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Constitution);
		if (assassinate)
		{
			damage = Math.Max(0.01f, 1.01f + (float)((!Zombie) ? skillLevelWithEffects : 0) - BloodLoss);
			if (IsAuthoritative() && Clothes[6] is Armor armor)
			{
				armor.Protection = 0f;
			}
		}
		else
		{
			GetModifiedDamageToMe(source, ref damage, injuryType, ref injuryLocation, ref isStealthAttack);
			damage = GetModifiedDamageToMe2(source, damage, ref injuryType, injuryLocation, ammoType, targetBodyLocation, bone, posInBoneSpace, isStealthAttack, out absorbedByVest);
		}
		bool flag = infectionType == InfectionType.None && injuryType == InjuryType.ZombieBite && source != null && source.InvisibleStrain != InvisibleStrainType.None && GetBaseObjectType() == BaseObjectType.Human;
		if (infectionType != InfectionType.None && source != null && IsAuthoritative() && source.EquippedItem is MeleeWeapon meleeWeapon)
		{
			meleeWeapon.DosesRemaining = Math.Max(0, meleeWeapon.DosesRemaining - 1);
			if (meleeWeapon.DosesRemaining <= 0)
			{
				meleeWeapon.InfectedWith = InfectionType.None;
			}
		}
		if (GetBaseObjectType() != BaseObjectType.Human)
		{
			infectionType = InfectionType.None;
		}
		if (absorbedByVest || injuryType == InjuryType.BluntObject)
		{
			infectionType = InfectionType.None;
		}
		if (flag)
		{
			damage = 0f;
			secret = SecrecyMode.Private;
		}
		bool flag2 = source != null && SparringPartner != null && source.GetAuthoritativeOrElseThisCharacter() == SparringPartner.GetAuthoritativeOrElseThisCharacter() && (injuryType == InjuryType.BluntObject || injuryType == InjuryType.Punch || injuryType == InjuryType.SharpObject || SparringType == SparringType.FightToTheDeath);
		if (flag2 && SparringType != SparringType.FightToTheDeath)
		{
			if (injuryType == InjuryType.Punch && SparringType == SparringType.Boxing)
			{
				damage = Math.Min(Math.Max(0f, FisticuffsWinBloodLoss + 0.01f - BloodLoss), damage);
			}
		}
		else if (!absorbedByVest && damage > 0f && !Zombie && !flag)
		{
			if (IsAwake)
			{
				Adrenaline = 1f;
			}
			if (this == Hud.Instance.LocalControlledCharacter)
			{
				Hud.Instance.TriggerDamageVignette();
				if (injuryType == InjuryType.ZombieBite && Time.unscaledTime - LastZombieBiteSoundPlayedTime >= 1f)
				{
					SoundManager.PlayMenuSoundFromList(SoundManager.ZombieBiteSounds);
					LastZombieBiteSoundPlayedTime = Time.unscaledTime;
				}
			}
		}
		int num = -1;
		SparringType sparringType = (flag2 ? source.SparringType : SparringType.None);
		bool intentional = sparringType != SparringType.None || IsAttackIntentional(source, intendedTarget);
		bool flag3 = IsAssailantUnknown(source);
		if (!absorbedByVest && (damage > 0f || flag || injuryType == InjuryType.Arrow))
		{
			int num2 = 0;
			int num3 = -1;
			float num4 = 0f;
			for (int i = 0; i < Injuries.Count; i++)
			{
				if (Injuries[i].Location == injuryLocation)
				{
					num2++;
					if (Injuries[i].Type == injuryType && Injuries[i].InfectionType == infectionType && (num3 == -1 || (Injuries[i].Bandaged && !Injuries[num3].Bandaged)))
					{
						num3 = i;
					}
				}
			}
			int num5 = ((injuryLocation == InjuryLocation.Torso) ? 8 : 4);
			if (num2 >= num5 && num3 != -1)
			{
				num4 = Injuries[num3].DamageAmount;
				RemoveInjuryAt(num3);
			}
			num = Injuries.Count;
			Injury injury = new Injury(this, injuryType, absorbedByVest: false, injuryLocation, infectionType, bone, posInBoneSpace, damage + num4, currentTime, source, flag3, intentional);
			injury.Secrecy = secret;
			injury.FromSparring = sparringType;
			AddInjury(injury);
			Fuel += fuel;
			if (InTerrain)
			{
				if (IsBurning())
				{
					AddToBurningMapWho();
				}
				else
				{
					RemoveFromBurningMapWho();
				}
			}
			WantUnityUpdateDecals = true;
			WantUnityUpdateInjuries = true;
			CanSkipUnityUpdate = false;
			CauseOfDeath causeOfDeath = ((injuryType == InjuryType.TrapExplosion) ? CauseOfDeath.Tripwire : ((injuryType == InjuryType.ZombieBite) ? CauseOfDeath.Zombie : ((assassinate && (secret == SecrecyMode.Private || secret == SecrecyMode.IgnoredByObjectCommunity) && source.InvisibleStrain != InvisibleStrainType.None) ? CauseOfDeath.InvisibleStrainAssassination : CauseOfDeath.Other)));
			damage = ApplyBloodLoss2(damage, hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, causeOfDeath, source, skillLevelWithEffects, secret);
		}
		if (flag)
		{
			InvisibleStrain = source.InvisibleStrain;
		}
		if (injuryType == InjuryType.Arrow && IsAuthoritative() && MathUtil.RandomChoice((float)(Id * 23) + (float)currentTime.TotalMilliseconds + 7656.1235f, 1f - ammoType.ChanceOfArrowBreaking / 100f))
		{
			Equipment equipment = Equipment.Spawn(ammoType);
			Inventory.Add(this, equipment);
		}
		if (Alive)
		{
			if (IsAuthoritative())
			{
				StoryManager.Instance.SetConditionsDirty();
			}
			if (injuryLocation == InjuryLocation.Head && (injuryType == InjuryType.BluntObject || (injuryType == InjuryType.Punch && !IsAwake)) && (!IsAwake || isStealthAttack) && (SparringPartner != source || SparringType != SparringType.Boxing))
			{
				SedativeEffect += 30f;
				Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
				if (IsAwake)
				{
					LastPersonChokedMe = source;
					SetConsciousness(Consciousness.Unconscious);
				}
				if (IsAuthoritative() && this is Human && !Zombie)
				{
					if (flag3 && secret == SecrecyMode.Public)
					{
						Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, null, this, 1f, SecrecyMode.OnlyKnownToObjectCommunity);
						Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, source, this, 1f, SecrecyMode.IgnoredByObjectCommunity);
					}
					else
					{
						Memory.OnMemorableEvent(MemoryPrototype.KnockedOut, source, this, 1f, secret);
					}
				}
			}
			else if (IsAwake)
			{
				if (source != null && !flag2 && secret != SecrecyMode.Private)
				{
					GetOrCreateTarget(source).OnAttackedMe(this, source.Position, forceVisible: true);
				}
				if (Zombie && CheckFrontmostPrediction(PredictedEventType.ZombieSound))
				{
					PlayVoiceSoundFromList(SoundManager.ZombiePainSounds, VoiceSoundType.ZombieSnarl);
				}
				else if (this is Animal animal && !IsVoiceSoundPlaying(VoiceSoundType.AnimalFleeing) && CheckFrontmostPrediction(PredictedEventType.PainSound))
				{
					animal.PlayFleeSound();
				}
				if (secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.IgnoredByObjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToObjectCommunity && secret != SecrecyMode.OnlyKnownToSubject && secret != SecrecyMode.Private && !assassinate && IsAuthoritative())
				{
					Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Pain, Position, GetShoutVoiceRadius(), GetMaxSoundVisibilityRange(), this, source, this, intendedTarget));
				}
				if (flag3 && num != -1 && !IsAssailantUnknown(source))
				{
					Injury value = Injuries[num];
					value.AssailantUnknown = false;
					Injuries[num] = value;
				}
				if (IsAuthoritative() && source != null && !flag2 && secret != SecrecyMode.Private && !assassinate)
				{
					TriggerHurtMemory(source, flag3, damage, secret, sparringType, intentional);
					if (!Zombie && GetBaseObjectType() == BaseObjectType.Human)
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, source, SpeechSituation.Pain);
						if (speechForSituation != null && Speak(speechForSituation))
						{
							SkipSpeech(onlySkipLipsMoving: true);
						}
					}
				}
				Vector2 lhs = -MathUtil.ToXZ((source != null) ? (Pos - source.Pos) : (-base.Forward)).normalized;
				float num6 = Vector2.Dot(lhs, MathUtil.ToXZ(base.Forward));
				float num7 = Vector2.Dot(lhs, MathUtil.ToXZ(base.Right));
				ActionAnim anim = ActionAnim.Damaged_Torso_FromCentre;
				switch (injuryLocation)
				{
				case InjuryLocation.Head:
					anim = ((!(num7 >= 0.707f)) ? ((!(num7 <= -0.707f)) ? ((!(num6 >= 0f)) ? ActionAnim.Damaged_Head_FromBehind : ActionAnim.Damaged_Head_FromCentre) : ActionAnim.Damaged_Head_FromLeft) : ActionAnim.Damaged_Head_FromRight);
					break;
				case InjuryLocation.Torso:
					anim = ((!(num6 >= 0f)) ? ActionAnim.Damaged_Torso_FromBehind : ActionAnim.Damaged_Torso_FromCentre);
					break;
				case InjuryLocation.LeftArm:
					anim = ActionAnim.Damaged_Torso_FromLeft;
					break;
				case InjuryLocation.LeftLeg:
					anim = ActionAnim.Damaged_LeftLeg;
					break;
				case InjuryLocation.RightArm:
					anim = ActionAnim.Damaged_Torso_FromRight;
					break;
				case InjuryLocation.RightLeg:
					anim = ActionAnim.Damaged_RightLeg;
					break;
				}
				if (!IsInDamageReactionAnim() && !dontReact && IsOutdoors())
				{
					if (DirectControlled)
					{
						if (!flag2)
						{
							TryStartActionAnim(anim, source);
							DirectControlledCrouching = false;
						}
					}
					else
					{
						Character character = InteractionObject as Character;
						if (Zombie && source != null && source != InteractionObject && character != null && character.AliveAndNotZombie && !character.HasAnyInfectedInjuries() && IsAuthoritative())
						{
							Memory.OnMemorableEvent(MemoryPrototype.SavedLife, source, InteractionObject, 1f, secret: false);
						}
						if (CarryingObject == null || IsControllableByPlayer() || (injuryType != InjuryType.Arrow && injuryType != InjuryType.Bullet) || (source != null && source.DirectControlled))
						{
							Character character2 = source;
							if (character2 == null)
							{
								character2 = this;
							}
							if (injuryType == InjuryType.VehicleImpact)
							{
								Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
							}
							else if (MovementSpeed >= JogSpeed)
							{
								if ((injuryLocation == InjuryLocation.LeftLeg || injuryLocation == InjuryLocation.RightLeg) && !absorbedByVest && ApplyRagdoll(legs: true, moving: true, character2, injuryType, targetBodyLocation))
								{
									Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
								}
								else if (injuryType == InjuryType.Arrow)
								{
									TryStartActionAnim(anim, source);
								}
							}
							else if (IsZombieJumping() || IsPlayingDead() || (IsRagdollOrProneOrRecovering() && injuryType != InjuryType.Bullet && injuryType != InjuryType.Arrow))
							{
								if (ApplyRagdoll(legs: false, moving: true, character2, injuryType, targetBodyLocation))
								{
									Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
								}
							}
							else if ((injuryLocation == InjuryLocation.LeftLeg || injuryLocation == InjuryLocation.RightLeg) && !absorbedByVest && ApplyRagdoll(legs: true, moving: false, character2, injuryType, targetBodyLocation))
							{
								Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
							}
							else
							{
								TryStartActionAnim(anim, source);
							}
						}
					}
				}
				if (Goal != null)
				{
					Goal.OnDamaged(this, null, source, injuryLocation, absorbedByVest);
				}
			}
			else if (!dontReact && IsOutdoors() && injuryType != InjuryType.ZombieBite)
			{
				Ragdollify(hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, fromStumble: false, retainVelocity: true);
			}
		}
		else if (source != null && IsAuthoritative())
		{
			if (secret != SecrecyMode.Private)
			{
				TriggerKilledMemory(source, flag3, secret, sparringType, intendedTarget, assassinate, intentional);
			}
			if (injuryType == InjuryType.VehicleImpact)
			{
				AchievementsManager.Instance.IncrementAchievementStat(Achievement.VehicleKills, 1);
				if (source.InsideBuilding is EnterableVehicle enterableVehicle && enterableVehicle.IsJuggernaut())
				{
					AchievementsManager.Instance.IncrementAchievementStat(Achievement.Story_DumpTruckKills, 1);
				}
			}
		}
		AddExcitement(damage);
		if (source != null && IsAuthoritative())
		{
			source.AddExcitement(damage);
			if (skillType != SkillType.Invalid)
			{
				source.Skillset.AddProgress(source, skillType, 5f * damage);
			}
			if (isStealthAttack)
			{
				source.Skillset.AddProgress(source, SkillType.Stealth, 10f * damage);
			}
		}
		OnDamaged(source, intendedTarget, injuryType, injuryLocation, skillType, ref damage, hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, dontReact, assassinate, secret, targetBodyLocation, fuel, out var _);
	}

	public void AddExcitement(float amount)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		float num = 2 + GetPersonality(CachedPersonalityType.Emotional, CachedPersonalityType.Unflappable) + (HasPersonality(CachedPersonalityType.Bipolar) ? 2 : 0);
		Excitement += Mathf.Lerp(0.05f, 0.1f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + (float)Id * 1000f)) * num * amount;
	}

	public void OnProjectileHit(Character shooter, TileObject intendedTarget, InjuryType injuryType, InjuryLocation injuryLocation, InfectionType infectionType, EquipmentPrototype ammoType, SkillType skillType, float damage, float hitRadius, Vector3 nonDeterministicHitPos, Vector3 nonDeterministicHitForce, Bone bone, Vector3 posInBoneSpace, bool assassinate, SecrecyMode secret, TargettableBodyLocation targetBodyLocation, out bool absorbedByVest)
	{
		absorbedByVest = false;
		if (Alive)
		{
			OnDamaged(shooter, intendedTarget, ref injuryType, injuryLocation, infectionType, ammoType, skillType, ref damage, hitRadius, nonDeterministicHitPos, nonDeterministicHitForce, bone, posInBoneSpace, dontReact: false, assassinate, secret, targetBodyLocation, 0f, out absorbedByVest);
		}
	}

	private bool GetExplosionHitInfo(Vector3 centre, TileObject bomb, out Vector3 hitPos, out Vector3 dir, ref float damage, float damageRadius, out InjuryLocation injuryLocation, out Bone hitBone, out Vector3 hitPosInBoneSpace)
	{
		Vector3 boundingBoxCentre = GetBoundingBoxCentre();
		dir = boundingBoxCentre - centre;
		float magnitude = dir.magnitude;
		hitPos = boundingBoxCentre;
		hitBone = Bone.Spine;
		hitPosInBoneSpace = Vector3.zero;
		injuryLocation = InjuryLocation.Torso;
		if (magnitude > 0.001f)
		{
			dir /= magnitude;
			Ray ray = new Ray(centre, dir);
			RaycastResult raycastResult = GameTerrain.Instance.RayCast(ray, damageRadius, 40, bomb);
			if (DrawExplosionRaycasts)
			{
				if ((raycastResult.HitObject != null && raycastResult.HitDist < magnitude) || damageRadius <= magnitude)
				{
					DebugGraphics.AddPersistentLine(ray.origin, raycastResult.GetHitPosition(), Color.yellow);
					DebugGraphics.AddPersistentLine(raycastResult.GetHitPosition(), ray.GetPoint(damageRadius), Color.red);
				}
				else
				{
					DebugGraphics.AddPersistentLine(ray.origin, ray.GetPoint(magnitude), Color.green);
				}
			}
			if (raycastResult.HitObject != null && raycastResult.HitDist < magnitude)
			{
				return false;
			}
			float num = 1f - magnitude / damageRadius;
			if (num <= 0f)
			{
				return false;
			}
			damage *= (float)Math.Sqrt(num);
		}
		else
		{
			dir = Vector3.up;
		}
		int num2 = (int)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).Ticks + 67890;
		injuryLocation = PickRandomInjuryLocation(MathUtil.ToXZ(centre), TargettableBodyLocation.Torso);
		PickRandomHitPos(injuryLocation, num2 + 1, centre, out hitBone, out hitPosInBoneSpace);
		return true;
	}

	public override float OnExplosionImpact(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, bool fromFoundations, SkillType skillType, InfectionType infectionType, bool itsATrap, TileObject bomb)
	{
		if (!Alive)
		{
			return 0f;
		}
		if (IsDodging() && IsFriendlyFire(source, intendedTarget, itsATrap: false))
		{
			return 0f;
		}
		if (!GetExplosionHitInfo(centre, bomb, out var hitPos, out var dir, ref damage, damageRadius, out var injuryLocation, out var hitBone, out var hitPosInBoneSpace))
		{
			return 0f;
		}
		Vector3 nonDeterministicHitForce = dir * Mathf.Lerp(ExplosionMaxHitForce, ExplosionMinHitForce, Mathf.Clamp((centre - hitPos).magnitude / damageRadius, 0f, 1f));
		OnDamaged(source, intendedTarget, itsATrap ? InjuryType.TrapExplosion : InjuryType.Explosion, injuryLocation, infectionType, null, skillType, ref damage, ExplosionHitRadius, hitPos, nonDeterministicHitForce, hitBone, hitPosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
		return damage;
	}

	public override float OnBurned(Character source, TileObject intendedTarget, Vector3 centre, float damage, float damageRadius, float fuel, SkillType skillType, InfectionType infectionType, bool assassinate, SecrecyMode secret)
	{
		if (IsDodging() && IsFriendlyFire(source, intendedTarget, itsATrap: false))
		{
			return 0f;
		}
		if (!GetExplosionHitInfo(centre, null, out var hitPos, out var dir, ref damage, damageRadius, out var injuryLocation, out var hitBone, out var hitPosInBoneSpace))
		{
			return 0f;
		}
		if (!Alive)
		{
			if (CremationProgression < 1f)
			{
				Fuel += fuel;
				if (!IsBurning())
				{
					AddInjury(new Injury(this, InjuryType.Fire, absorbedByVest: false, injuryLocation, InfectionType.None, hitBone, hitPosInBoneSpace, 0f, PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()), source, IsAssailantUnknown(source), IsAttackIntentional(source, intendedTarget)));
					if (InTerrain)
					{
						AddToBurningMapWho();
					}
				}
				UnityUpdateInjuries();
			}
			return 0f;
		}
		InjuryType injuryType = InjuryType.Fire;
		OnDamaged(source, intendedTarget, ref injuryType, injuryLocation, InfectionType.None, null, skillType, ref damage, BurnedHitRadius, hitPos, dir * BurnedHitForce, hitBone, hitPosInBoneSpace, dontReact: false, assassinate, secret, TargettableBodyLocation.Torso, fuel, out var _);
		return damage;
	}

	public override float OnVehicleCollision(BaseObject other, Vector3 relativeVelocity, Vector3 contactPoint, bool predicted)
	{
		EnterableVehicle enterableVehicle = other as EnterableVehicle;
		Character character = enterableVehicle?.GetDriver();
		float num = enterableVehicle?.GetMass() ?? 0f;
		Vector3 vector = enterableVehicle?.Pos ?? contactPoint;
		Vector3 nonDeterministicHitForce = (relativeVelocity + Vector3.up) * num * VehicleImpactFactor;
		float massEstimate = GetMassEstimate();
		float magnitude = nonDeterministicHitForce.magnitude;
		if (magnitude > 0f && massEstimate > 0f)
		{
			float b = magnitude / massEstimate;
			b = Mathf.Min(AccelerationLimit, b);
			nonDeterministicHitForce *= massEstimate * b / magnitude;
		}
		float magnitude2 = relativeVelocity.magnitude;
		float num2 = EnterableVehicle.DrivingHideMenuSpeedMph / 2.2369418f;
		float damage = Mathf.Max(0f, magnitude2 - num2) * num * Prop.VehicleCollisionDamageFactor;
		if (damage > 0f)
		{
			int currentFrame = PredictedObjectManager.Instance.GetCurrentFrame(IsPredicted());
			InjuryLocation injuryLocation = PickRandomInjuryLocation(MathUtil.ToXZ(vector), TargettableBodyLocation.Torso);
			PickRandomHitPos(injuryLocation, other.Id * 274 + Id * 65 + currentFrame, vector, out var bone, out var hitPosInBoneSpace);
			Character intendedTarget = this;
			if (character != null && !character.IsControllableByPlayer() && IsControllableByPlayer())
			{
				intendedTarget = null;
			}
			InjuryType injuryType = InjuryType.VehicleImpact;
			OnDamaged(character, intendedTarget, ref injuryType, injuryLocation, InfectionType.None, null, SkillType.Construction, ref damage, BurnedHitRadius, contactPoint, nonDeterministicHitForce, bone, hitPosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public, TargettableBodyLocation.Torso, 0f, out var _);
		}
		return damage;
	}

	public override float GetMassEstimate()
	{
		return GetWeightInKg();
	}

	public bool HasAnyInfectedInjuries()
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].InfectionType != InfectionType.None)
			{
				return true;
			}
		}
		return false;
	}

	public InfectionType GetWorstInfectionTypeInProgression()
	{
		InfectionType infectionType = InfectionType.None;
		for (int i = 0; i < Injuries.Count; i++)
		{
			infectionType = (InfectionType)Math.Max((int)infectionType, (int)Injuries[i].InfectionType);
		}
		return infectionType;
	}

	public bool HasInjuryWithInfectionType(InfectionType infectionType)
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].InfectionType == infectionType)
			{
				return true;
			}
		}
		return false;
	}

	public InfectionType GetStrainFromMostRecentInfectedInjury()
	{
		InfectionType result = InfectionType.None;
		TimeSpan timeSpan = Target.Never;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].OriginalInfectionType != InfectionType.None && Injuries[i].InjuryTime > timeSpan)
			{
				timeSpan = Injuries[i].InjuryTime;
				result = Injuries[i].OriginalInfectionType;
			}
		}
		return result;
	}

	public TimeSpan GetTimeOfMostRecentInfectedInjury()
	{
		TimeSpan timeSpan = Target.Never;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].OriginalInfectionType != InfectionType.None && Injuries[i].InjuryTime > timeSpan)
			{
				timeSpan = Injuries[i].InjuryTime;
			}
		}
		if (timeSpan == Target.Never)
		{
			for (int j = 0; j < Injuries.Count; j++)
			{
				if (Injuries[j].Type == InjuryType.ZombieBite && Injuries[j].InjuryTime > timeSpan)
				{
					timeSpan = Injuries[j].InjuryTime;
				}
			}
		}
		return timeSpan;
	}

	public bool IsHealable()
	{
		if (!AliveAndNotZombie)
		{
			return false;
		}
		for (int i = 1; i < 6; i++)
		{
			InfectionType infectionType = (InfectionType)i;
			if (HasInjuryWithInfectionType(infectionType) && GameImpl.Instance.FindAntigenPrototypeForInfectionType(infectionType) == null)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasUnbandagedInjury(int skillLevel)
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (!Injuries[i].AbsorbedByVest && Injuries[i].BandagedSkillLevel < skillLevel)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumUnbandagedInjuries(int skillLevel)
	{
		int num = 0;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < Injuries.Count; j++)
			{
				if (Injuries[j].Location == (InjuryLocation)i && !Injuries[j].Bandaged && !Injuries[j].AbsorbedByVest)
				{
					num++;
					break;
				}
			}
		}
		return num;
	}

	private static bool HasArrowStuckInjury(List<Injury> injuries)
	{
		for (int i = 0; i < injuries.Count; i++)
		{
			if (injuries[i].ArrowStuck)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasArrowStuckInjury()
	{
		if (HasArrowStuckInjury(Injuries))
		{
			return true;
		}
		Equipment[] clothes = Clothes;
		for (int i = 0; i < clothes.Length; i++)
		{
			if (clothes[i] is Armor armor && HasArrowStuckInjury(armor.ArmorDamagePoints))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInjury(InjuryLocation location)
	{
		PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Location == location && !Injuries[i].AbsorbedByVest)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInjuryOfType(InjuryType injuryType)
	{
		PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Type == injuryType && !Injuries[i].AbsorbedByVest)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInfectedInjuryOfType(InjuryType injuryType)
	{
		PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Type == injuryType && Injuries[i].InfectionType != InfectionType.None && !Injuries[i].AbsorbedByVest)
			{
				return true;
			}
		}
		return false;
	}

	public TimeSpan GetNewestUnbandagedInjuryAge()
	{
		TimeSpan timeSpan = TimeSpan.Zero;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].BandagedSkillLevel == -1 && !Injuries[i].AbsorbedByVest)
			{
				timeSpan = TimeSpan.FromTicks(Math.Max(Injuries[i].InjuryTime.Ticks, timeSpan.Ticks));
			}
		}
		return PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - timeSpan;
	}

	public TimeSpan GetNewestInfectedInjuryAge()
	{
		TimeSpan timeSpan = TimeSpan.Zero;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].InfectionType != InfectionType.None)
			{
				timeSpan = TimeSpan.FromTicks(Math.Max(Injuries[i].InjuryTime.Ticks, timeSpan.Ticks));
			}
		}
		return PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - timeSpan;
	}

	public int GetInjuryBandageLevel(InjuryLocation location = InjuryLocation.Count)
	{
		int num = int.MaxValue;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if ((Injuries[i].Location == location || location == InjuryLocation.Count) && Injuries[i].BandagedSkillLevel < num && !Injuries[i].AbsorbedByVest)
			{
				num = Injuries[i].BandagedSkillLevel;
			}
		}
		return num;
	}

	public bool HasArmInjury()
	{
		if (!HasInjury(InjuryLocation.LeftArm))
		{
			return HasInjury(InjuryLocation.RightArm);
		}
		return true;
	}

	public bool HasLegInjury()
	{
		if (!HasInjury(InjuryLocation.LeftLeg))
		{
			return HasInjury(InjuryLocation.RightLeg);
		}
		return true;
	}

	public float GetLegDamage()
	{
		float num = 0f;
		for (int i = 0; i < Injuries.Count; i++)
		{
			InjuryType type = Injuries[i].Type;
			if ((uint)type <= 1u || (uint)(type - 5) <= 1u || type == InjuryType.Punch)
			{
				InjuryLocation location = Injuries[i].Location;
				if ((uint)(location - 4) <= 1u)
				{
					num += Injuries[i].DamageAmount;
				}
			}
		}
		return num;
	}

	public bool ShouldLimp()
	{
		bool rightLeg;
		return ShouldLimp(out rightLeg);
	}

	public bool ShouldLimp(out bool rightLeg)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		for (int i = 0; i < Injuries.Count; i++)
		{
			InjuryType type = Injuries[i].Type;
			if ((uint)type <= 1u || (uint)(type - 5) <= 1u || type == InjuryType.Punch)
			{
				switch (Injuries[i].Location)
				{
				case InjuryLocation.LeftLeg:
					num2 += Injuries[i].DamageAmount;
					num3 += Injuries[i].DamageAmount;
					break;
				case InjuryLocation.RightLeg:
					num += Injuries[i].DamageAmount;
					num3 += Injuries[i].DamageAmount;
					break;
				}
			}
		}
		float maxLimbDamage = GetMaxLimbDamage();
		if (num3 >= maxLimbDamage)
		{
			rightLeg = num >= num2;
			return true;
		}
		rightLeg = false;
		return false;
	}

	public bool IsBoxerReadyToFight()
	{
		if (!Boxer)
		{
			return false;
		}
		if (Consciousness >= Consciousness.Unconscious || Zombie)
		{
			return false;
		}
		if (BloodLoss > 0f)
		{
			return false;
		}
		if (Injuries.Count > 0)
		{
			return false;
		}
		return true;
	}

	public float PickAIMovementSpeed(MovementType movementType, bool tired)
	{
		if (tired)
		{
			movementType = MovementType.Jog;
		}
		switch (movementType)
		{
		case MovementType.Walk:
			if (IsCrouching())
			{
				return CrouchWalkSpeed;
			}
			if (ShouldLimp())
			{
				if (!Zombie)
				{
					return WalkInjuredSpeed;
				}
				return ZombieWalkInjuredSpeed;
			}
			return GetWalkSpeed();
		case MovementType.Jog:
			if (!IsCrouching())
			{
				if (!Zombie || !ShouldLimp())
				{
					return JogSpeed;
				}
				return ZombieWalkInjuredSpeed;
			}
			return CrouchWalkSpeed;
		case MovementType.Run:
			if (IsCrouching())
			{
				if (!(GetFatigueMinusAdrenaline() < 1f))
				{
					return CrouchOutOfBreathRunSpeed;
				}
				return CrouchRunSpeed;
			}
			if (ShouldLimp())
			{
				if (!Zombie)
				{
					if (!(GetFatigueMinusAdrenaline() < 1f))
					{
						return OutOfBreathRunInjuredSpeed;
					}
					return RunInjuredSpeed;
				}
				return ZombieWalkInjuredSpeed;
			}
			if (!(GetFatigueMinusAdrenaline() < 1f))
			{
				return OutOfBreathRunSpeed;
			}
			return GetRunSpeed();
		default:
			return 0f;
		}
	}

	public override EquipmentContainer GetInventory()
	{
		return Inventory;
	}

	public override ActionAnim GetTakeAnim()
	{
		if (!IsAwake)
		{
			return ActionAnim.ScavengeCorpse;
		}
		return ActionAnim.Scavenge;
	}

	public override void MarkInvestigated(Character investigator)
	{
		if (!Investigated)
		{
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Investigated, investigator, this);
		}
		Investigated = true;
	}

	public override bool IsInvestigated()
	{
		return Investigated;
	}

	public static float GetMaxInventoryWeightWithoutBackpackForFitness(int fitness)
	{
		return Mathf.Lerp(30f, 40f, (float)fitness / 5f);
	}

	public virtual float GetMaxInventoryWeightWithoutBackpack()
	{
		return GetMaxInventoryWeightWithoutBackpackForFitness(GetSkillLevelWithEffects(SkillType.Strength));
	}

	public override float GetMaxInventoryWeight()
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Strength);
		float num = GetMaxInventoryWeightWithoutBackpackForFitness(skillLevelWithEffects);
		for (int i = 0; i < 9; i++)
		{
			if (Clothes[i] != null)
			{
				num += Clothes[i].GetCarryWeightEffect(skillLevelWithEffects);
			}
		}
		return num;
	}

	public float GetMaxInventoryWeightIgnoringBackpack(Equipment ignoreBackpack)
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Strength);
		float num = GetMaxInventoryWeightWithoutBackpackForFitness(skillLevelWithEffects);
		for (int i = 0; i < 9; i++)
		{
			if (Clothes[i] == ignoreBackpack)
			{
				Equipment clothingOfTypeWithBestCarryEffect = Inventory.GetClothingOfTypeWithBestCarryEffect((ClothingType)i, skillLevelWithEffects, ignoreBackpack);
				if (clothingOfTypeWithBestCarryEffect != null)
				{
					num += clothingOfTypeWithBestCarryEffect.GetCarryWeightEffect(skillLevelWithEffects);
				}
			}
			else if (Clothes[i] != null)
			{
				num += Clothes[i].GetCarryWeightEffect(skillLevelWithEffects);
			}
		}
		return num;
	}

	public override float GetMaxInventoryWeightIncludingBackpack(Equipment includeItem)
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Strength, includeItem);
		float num = GetMaxInventoryWeightWithoutBackpackForFitness(skillLevelWithEffects);
		float carryWeightEffect = includeItem.GetCarryWeightEffect(skillLevelWithEffects);
		for (int i = 0; i < 9; i++)
		{
			if (i == (int)includeItem.GetClothingType() && (Clothes[i] == null || Clothes[i].GetCarryWeightEffect(skillLevelWithEffects) < carryWeightEffect))
			{
				num += carryWeightEffect;
			}
			else if (Clothes[i] != null)
			{
				num += Clothes[i].GetCarryWeightEffect(skillLevelWithEffects);
			}
		}
		return num;
	}

	public float GetMaxInventoryWeightAfterPendingTrades(List<InventoryBehaviour.EquipmentScore> contentsAfterPendingTrades)
	{
		SkillEffects.Clear();
		GetSkillEffectsForInjuries(SkillEffects);
		for (int i = 0; i < contentsAfterPendingTrades.Count; i++)
		{
			contentsAfterPendingTrades[i].Item.GetSkillEffects(this, SkillEffects, contentsAfterPendingTrades[i].AmountAfterPendingTrades);
		}
		int fitness = CalcSkillLevelFromEffects(SkillType.Strength, SkillEffects);
		float num = GetMaxInventoryWeightWithoutBackpackForFitness(fitness);
		for (int j = 0; j < 9; j++)
		{
			Equipment equipment = Clothes[j];
			bool flag = false;
			for (int k = 0; k < contentsAfterPendingTrades.Count; k++)
			{
				Equipment item = contentsAfterPendingTrades[k].Item;
				if (equipment == item)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				equipment = null;
			}
			if (j == 5)
			{
				float num2 = 0f;
				for (int l = 0; l < contentsAfterPendingTrades.Count; l++)
				{
					Equipment item2 = contentsAfterPendingTrades[l].Item;
					if (item2.GetClothingType() == ClothingType.Backpack)
					{
						float carryWeightEffect = item2.GetCarryWeightEffect(fitness);
						if (carryWeightEffect > num2)
						{
							equipment = item2;
							num2 = carryWeightEffect;
						}
					}
				}
			}
			if (equipment != null)
			{
				num += equipment.GetCarryWeightEffect(fitness);
			}
		}
		return num;
	}

	public Equipment GetBestMeleeWeaponForShortcut()
	{
		if (CachedShortcutMeleeWeaponValid)
		{
			return CachedShortcutMeleeWeapon;
		}
		CachedShortcutMeleeWeapon = Inventory.GetBestWeaponForShortcut(this, typeof(MeleeWeapon), 0, out var _, out var _);
		CachedShortcutMeleeWeaponValid = true;
		return CachedShortcutMeleeWeapon;
	}

	public Equipment GetBestAmmoWeaponForShortcut(out EquipmentPrototype ammoType, out InfectionType infectedWith)
	{
		if (CachedShortcutAmmoWeaponValid)
		{
			ammoType = CachedShortcutAmmoType;
			infectedWith = CachedShortcutAmmoInfectedWith;
			return CachedShortcutAmmoWeapon;
		}
		CachedShortcutAmmoWeapon = Inventory.GetBestWeaponForShortcut(this, typeof(AmmoWeapon), 0, out CachedShortcutAmmoType, out CachedShortcutAmmoInfectedWith);
		CachedShortcutAmmoWeaponValid = true;
		ammoType = CachedShortcutAmmoType;
		infectedWith = CachedShortcutAmmoInfectedWith;
		return CachedShortcutAmmoWeapon;
	}

	public Equipment GetBestThrowableForShortcut()
	{
		if (CachedShortcutThrowableValid)
		{
			return CachedShortcutThrowable;
		}
		CachedShortcutThrowable = Inventory.GetBestWeaponForShortcut(this, typeof(Throwable), 0, out var _, out var _);
		CachedShortcutThrowableValid = true;
		return CachedShortcutThrowable;
	}

	public bool IsWearing(Equipment item)
	{
		if (item.GetClothingType() != ClothingType.Invalid)
		{
			return Clothes[(int)item.GetClothingType()] == item;
		}
		return false;
	}

	public bool IsArmoredOnBodyLocation(TargettableBodyLocation bodyLocation)
	{
		ClothingType clothingType;
		switch (bodyLocation)
		{
		case TargettableBodyLocation.Head:
			clothingType = ClothingType.Hat;
			break;
		case TargettableBodyLocation.Torso:
			clothingType = ClothingType.BodyArmor;
			break;
		case TargettableBodyLocation.Legs:
			clothingType = ClothingType.LegArmor;
			break;
		default:
			return false;
		}
		if (Clothes[(int)clothingType] is Armor armor)
		{
			return armor.Protection > 0f;
		}
		return false;
	}

	public bool IsWearingArmor()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] is Armor)
			{
				return true;
			}
		}
		return false;
	}

	public bool LikesGiftType(EquipmentPrototype proto, List<string> becauseOfPersonality)
	{
		if (proto.GiftFor != null && proto.GiftFor.Count > 0)
		{
			foreach (string item in proto.GiftFor)
			{
				if (item == Anyone)
				{
					return true;
				}
				if (!Personality.Contains(item))
				{
					return false;
				}
			}
			if (becauseOfPersonality != null)
			{
				foreach (string item2 in proto.GiftFor)
				{
					becauseOfPersonality.Add(item2);
				}
			}
			return true;
		}
		return false;
	}

	public bool LikesGiftLiquid(LiquidPrototype liquid, List<string> becauseOfPersonality)
	{
		if (liquid.GiftFor != null && liquid.GiftFor.Count > 0)
		{
			foreach (string item in liquid.GiftFor)
			{
				if (item == Anyone)
				{
					return true;
				}
				if (!Personality.Contains(item))
				{
					return false;
				}
			}
			if (becauseOfPersonality != null)
			{
				foreach (string item2 in liquid.GiftFor)
				{
					becauseOfPersonality.Add(item2);
				}
			}
			return true;
		}
		return false;
	}

	public bool DislikesGiftType(EquipmentPrototype item, List<string> becauseOfPersonality)
	{
		if (LikesGiftType(item, null))
		{
			return false;
		}
		if (item.BadGiftFor != null)
		{
			foreach (string item2 in item.BadGiftFor)
			{
				if (Personality.Contains(item2))
				{
					becauseOfPersonality?.Add(item2);
					return true;
				}
			}
		}
		return false;
	}

	public bool DislikesGiftLiquid(LiquidPrototype liquid, List<string> becauseOfPersonality)
	{
		if (LikesGiftLiquid(liquid, null))
		{
			return false;
		}
		if (liquid.BadGiftFor != null)
		{
			foreach (string item in liquid.BadGiftFor)
			{
				if (Personality.Contains(item))
				{
					becauseOfPersonality?.Add(item);
					return true;
				}
			}
		}
		return false;
	}

	public bool LikesGift(Equipment item, List<string> becauseOfPersonality)
	{
		EquipmentPrototype prototype = item.GetPrototype();
		if (LikesGiftType(prototype, becauseOfPersonality))
		{
			return true;
		}
		LiquidPrototype liquidContentsType = item.GetLiquidContentsType();
		if (liquidContentsType != null && LikesGiftLiquid(liquidContentsType, becauseOfPersonality))
		{
			return true;
		}
		return false;
	}

	public bool DislikesGift(Equipment item, List<string> becauseOfPersonality)
	{
		EquipmentPrototype prototype = item.GetPrototype();
		if (DislikesGiftType(prototype, becauseOfPersonality))
		{
			return true;
		}
		LiquidPrototype liquidContentsType = item.GetLiquidContentsType();
		if (liquidContentsType != null && DislikesGiftLiquid(liquidContentsType, becauseOfPersonality))
		{
			return true;
		}
		return false;
	}

	public int CanSellItemToPlayerCommunity(Equipment item)
	{
		if (DontSellItemTypeToPlayer == item.GetPrototype())
		{
			return 0;
		}
		switch (item.GetPrototype().TradeBehaviour)
		{
		case TradeBehaviour.CanSellToPlayer:
			return item.GetAmount();
		case TradeBehaviour.CantSellToPlayer:
			return 0;
		case TradeBehaviour.CanSellToAnyone:
			return item.GetAmount();
		case TradeBehaviour.CantSellToAnyone:
			return 0;
		default:
		{
			if (item.IsWornOrRemovedForSparring(this))
			{
				return 0;
			}
			if (item is MolotovCocktail)
			{
				return item.GetAmount() - 1;
			}
			if (!item.GetPrototype().Special)
			{
				if (item is Gun)
				{
					Gun gun = null;
					for (int i = 0; i < Inventory.Count; i++)
					{
						if (Inventory.GetItem(i) is Gun gun2 && !gun2.GetPrototype().Special && (gun == null || gun2.GetBasePrice() > gun.GetBasePrice()))
						{
							gun = gun2;
						}
					}
					if (item == gun)
					{
						return 0;
					}
				}
				if (item is MeleeWeapon)
				{
					MeleeWeapon meleeWeapon = null;
					for (int j = 0; j < Inventory.Count; j++)
					{
						if (Inventory.GetItem(j) is MeleeWeapon meleeWeapon2 && !meleeWeapon2.GetPrototype().Special && (meleeWeapon == null || meleeWeapon2.GetBaseDamage() > meleeWeapon.GetBaseDamage()))
						{
							meleeWeapon = meleeWeapon2;
						}
					}
					if (item == meleeWeapon)
					{
						return item.GetAmount() - 1;
					}
				}
				if (item.IsEdible() && Hunger > HungryTime)
				{
					return 0;
				}
			}
			if (item.GetGatheredAmount() > 0 && HasRole(Role.Organizer))
			{
				return item.GetAmount() - item.GetGatheredAmount();
			}
			if (item is WateringCan && HasRole(Role.Farmer) && item == Inventory.GetBestWateringCan())
			{
				return item.GetAmount() - 1;
			}
			if (item is Axe && (HasRole(Role.Lumberjack) || HasRole(Role.Trapper) || HasRole(Role.Builder)) && item == Inventory.GetAxe())
			{
				return item.GetAmount() - 1;
			}
			if (item is Pickaxe && HasRole(Role.Miner) && item == Inventory.GetPickaxe())
			{
				return item.GetAmount() - 1;
			}
			if (item.GetPrototype() == EquipmentPrototype.Pot && HasRole(Role.Cook) && item == Inventory.GetBestCookingPot())
			{
				return item.GetAmount() - 1;
			}
			if (item.GetPrototype() == EquipmentPrototype.FryingPan && HasRole(Role.Cook) && item == Inventory.GetFryingPan())
			{
				return item.GetAmount() - 1;
			}
			if (HasRole(Role.Builder) && (item is Toolbox || item is Shovel))
			{
				BuildGoal buildGoal = GetBuildGoal();
				if (buildGoal != null)
				{
					Recipe followingRecipe = buildGoal.GetFollowingRecipe();
					if (followingRecipe != null && followingRecipe.IsRecipeTool(this, item))
					{
						return item.GetAmount() - 1;
					}
				}
			}
			if (HasCraftingRoleWithIngredient(item))
			{
				return 0;
			}
			CraftGoal craftGoal = GetCraftGoal();
			if (craftGoal != null && craftGoal.FollowingRecipe != null && craftGoal.FollowingRecipe.IsIngredient(item))
			{
				return 0;
			}
			if (item == Inventory.GetBestWaterBottle())
			{
				return 0;
			}
			if (item.GetLiquidContentsType() == LiquidPrototype.Water && item.GetLiquidContentsAmount() > 0f && Thirst > ThirstyTime)
			{
				return 0;
			}
			if (item.GetBandageLevel() > -1 && HasUnbandagedInjury(0))
			{
				return 0;
			}
			if (item.GetAntigenType() != InfectionType.None && HasInjuryWithInfectionType(item.GetAntigenType()))
			{
				return 0;
			}
			return item.GetAmount();
		}
		}
	}

	public bool CanSellItemToAICommunity(Equipment item)
	{
		return item.GetPrototype().TradeBehaviour switch
		{
			TradeBehaviour.CanSellToAI => true, 
			TradeBehaviour.CantSellToAI => false, 
			TradeBehaviour.CanSellToAnyone => true, 
			TradeBehaviour.CantSellToAnyone => false, 
			_ => true, 
		};
	}

	public override bool WantToKeepGift(Equipment item)
	{
		if (item.Gifted)
		{
			if (!IsControllableByPlayer())
			{
				return IsAwake;
			}
			return Alive;
		}
		return false;
	}

	public override CantTransferReason CanTransferEquipmentAway(Equipment item, bool onTradePage)
	{
		if (WantToKeepGift(item))
		{
			return CantTransferReason.WantToKeepMe;
		}
		if (Alive && !onTradePage && item.GetClothingType() != ClothingType.Invalid && Clothes[(int)item.GetClothingType()] == item && (item.GetPrototype().CarryWeight > 0f || item.GetPrototype().CarryWeightBonusPerSkillLevel > 0f))
		{
			if (!(GetMaxInventoryWeightIgnoringBackpack(item) >= Inventory.GetWeightIgnoringItem(this, item)))
			{
				return CantTransferReason.SourceTooHeavyWithoutMe;
			}
			return CantTransferReason.CanTransfer;
		}
		return CantTransferReason.CanTransfer;
	}

	public void GetSkillEffectsForInjuries(List<SkillEffect> skillEffects)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < Injuries.Count; i++)
		{
			switch (Injuries[i].Location)
			{
			case InjuryLocation.LeftArm:
				if (!flag)
				{
					skillEffects.Add(new SkillEffect(SkillType.Firearms, -1));
					skillEffects.Add(new SkillEffect(SkillType.Archery, -1));
					skillEffects.Add(new SkillEffect(SkillType.HandToHand, -1));
					flag = true;
				}
				break;
			case InjuryLocation.RightArm:
				if (!flag2)
				{
					skillEffects.Add(new SkillEffect(SkillType.Firearms, -1));
					skillEffects.Add(new SkillEffect(SkillType.Archery, -1));
					skillEffects.Add(new SkillEffect(SkillType.HandToHand, -1));
					flag2 = true;
				}
				break;
			case InjuryLocation.LeftLeg:
				if (!flag3)
				{
					skillEffects.Add(new SkillEffect(SkillType.Strength, -1));
					flag3 = true;
				}
				break;
			case InjuryLocation.RightLeg:
				if (!flag4)
				{
					skillEffects.Add(new SkillEffect(SkillType.Strength, -1));
					flag4 = true;
				}
				break;
			}
		}
	}

	public void AddInjury(Injury injury)
	{
		Injuries.Add(injury);
		OnInjuriesChanged(injury.Location);
		if (injury.InfectionType != InfectionType.None)
		{
			InfectionProgression += 0.0001f;
		}
	}

	public void RemoveInjuryAt(int i)
	{
		OnInjuriesChanged(Injuries[i].Location);
		Injuries.RemoveAt(i);
	}

	public void OnInjuriesChanged(InjuryLocation injuryLocation)
	{
		switch (injuryLocation)
		{
		case InjuryLocation.LeftArm:
		case InjuryLocation.RightArm:
			ClearCachedSkillLevelWithEffects(SkillType.Firearms);
			ClearCachedSkillLevelWithEffects(SkillType.Archery);
			ClearCachedSkillLevelWithEffects(SkillType.HandToHand);
			break;
		case InjuryLocation.LeftLeg:
		case InjuryLocation.RightLeg:
			ClearCachedSkillLevelWithEffects(SkillType.Strength);
			break;
		}
	}

	public void GetSkillEffects(List<SkillEffect> skillEffects)
	{
		skillEffects.Clear();
		GetSkillEffectsForInjuries(skillEffects);
		for (int i = 0; i < Inventory.Count; i++)
		{
			Equipment item = Inventory.GetItem(i);
			item.GetSkillEffects(this, skillEffects, item.GetAmount());
		}
	}

	public int CalcSkillLevelFromEffects(SkillType skillType, List<SkillEffect> skillEffects)
	{
		int num = Skillset.GetLevel(skillType);
		int cap = Skillset.GetCap(skillType);
		for (int i = 0; i < skillEffects.Count; i++)
		{
			if (skillEffects[i].SkillType == skillType)
			{
				num += skillEffects[i].Effect;
			}
		}
		return Math.Max(0, Math.Min(num, cap));
	}

	public Character()
	{
		for (int i = 0; i < CachedSkillLevelsWithEffects.Length; i++)
		{
			CachedSkillLevelsWithEffects[i] = -1;
		}
	}

	public void ClearCachedSkillLevelWithEffects(SkillType skillType)
	{
		CachedSkillLevelsWithEffects[(int)skillType] = -1;
	}

	public int GetSkillLevelWithEffects(SkillType skillType)
	{
		if (skillType < SkillType.Strength || skillType >= SkillType.Count)
		{
			return 0;
		}
		if (CachedSkillLevelsWithEffects[(int)skillType] == -1)
		{
			CachedSkillLevelsWithEffects[(int)skillType] = GetSkillLevelWithEffects(skillType, null);
		}
		return CachedSkillLevelsWithEffects[(int)skillType];
	}

	public int GetSkillLevelWithEffects(SkillType skillType, Equipment includeItem)
	{
		List<SkillEffect> skillEffects = (Util.AmIOnMainThread() ? SkillEffects : SkillEffectsOnThread);
		GetSkillEffects(skillEffects);
		includeItem?.GetSkillEffects(this, skillEffects, includeItem.GetAmount());
		return CalcSkillLevelFromEffects(skillType, skillEffects);
	}

	public bool CanUseRecipe(Recipe recipe)
	{
		return GetSkillLevelWithEffects(recipe.SkillType) >= recipe.SkillLevel;
	}

	public void IncrementSkillLevel(SkillType skillType)
	{
		int level = Skillset.GetLevel(skillType);
		if (level < 5)
		{
			float progress = Skillset.ProgressionToLevel[level + 1];
			Skillset.AddProgress(this, skillType, progress);
		}
	}

	public void ApplyAimingPenalty(float penalty)
	{
		if (EquippedItem != null)
		{
			penalty *= 1f - (float)GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType()) / 6f;
			AimingAccuracy = Math.Max(AimingAccuracy - penalty, 0f);
		}
	}

	public float GetAccuracy()
	{
		return AimingAccuracy;
	}

	public float GetUnarmedDamageModifier()
	{
		float num = 1f + (float)GetSkillLevelWithEffects(SkillType.HandToHand) / 5f;
		if (GetFatigueMinusAdrenaline() >= ExhaustedFatigueLevel)
		{
			num *= 0.1f;
		}
		return num;
	}

	public int GetBoxerWagerAmount()
	{
		int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.HandToHand);
		return StoryManager.BoxerWagerAmount[skillLevelWithEffects];
	}

	public int GetPriceToBandage(Character character)
	{
		if (Community != null && character != null && Community == character.Community)
		{
			return 0;
		}
		Equipment equipment = Inventory.FindItemWithHighestBandageLevel();
		int val = equipment?.GetBandageLevel() ?? (-1);
		float num = equipment?.GetBasePrice() ?? 0f;
		float num2 = Math.Min(GetSkillLevelWithEffects(SkillType.Medicine), val);
		num2 += num;
		float priceToSell = GetPriceToSell(num2, character);
		return Math.Max(1, (int)priceToSell);
	}

	public void SetupRestockTime()
	{
		if (RestockTime == TimeSpan.Zero)
		{
			RestockTime = Session.Instance.PlayTime + Sun.DayLength;
		}
	}

	private int GetAmountToTransferLimit(Equipment item, Equipment bestMeleeWeapon, Equipment bestRangedWeapon, Equipment bestWaterBottle)
	{
		if (!FindGoal.CanTransferFromCharacter(this, item, FindType.DumpGatheredItems, out var amountToTransfer))
		{
			return 0;
		}
		if (item == bestMeleeWeapon || item == bestRangedWeapon || item == bestWaterBottle)
		{
			return item.GetAmount() - 1;
		}
		if (item.GetSeedForPlantType() != null && Community != null && Community.IsAISettlement())
		{
			return Math.Max(0, Community.CountInventoryItemsOfType(item.GetPrototype()) - Community.GetReservedCropAmount(item.GetSeedForPlantType()));
		}
		if (item.IsEdible() && Community != null && Community.IsAISettlement() && Community.CalcCommunityNutritionLevel(item.GetNutrition() * (float)item.GetAmount()) < 1f)
		{
			return 0;
		}
		return amountToTransfer;
	}

	public void Restock()
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		Equipment bestWeapon = Inventory.GetBestWeapon(this, null, wantRanged: false, bluntOnly: false, ignoreGathered: true);
		Equipment bestWeapon2 = Inventory.GetBestWeapon(this, null, wantRanged: true, bluntOnly: false, ignoreGathered: true);
		Equipment bestWaterBottle = Inventory.GetBestWaterBottle(ignoreGathered: true);
		float num = 0f;
		while (Inventory.GetWeight(this) > GetMaxInventoryWeight() * 0.5f)
		{
			TradedItems.Clear();
			for (int i = 0; i < Inventory.Count; i++)
			{
				Equipment item = Inventory.GetItem(i);
				if (item.GetTradedAmount() > 0 && item.CanBeDestroyed())
				{
					int amountToTransferLimit = GetAmountToTransferLimit(item, bestWeapon, bestWeapon2, bestWaterBottle);
					item.ForceSetTradedAmount(Math.Min(item.GetTradedAmount(), amountToTransferLimit));
					if (item.GetTradedAmount() > 0)
					{
						TradedItems.Add(item);
					}
				}
			}
			if (TradedItems.Count == 0)
			{
				break;
			}
			Equipment equipment = TradedItems[deterministicRand.Next(TradedItems.Count)];
			num += equipment.GetBasePrice() * (float)equipment.GetTradedAmount();
			if (equipment.GetTradedAmount() < equipment.GetAmount())
			{
				equipment.IncrementAmount(-equipment.GetTradedAmount());
				Inventory.CacheEncumbered(this);
			}
			else
			{
				Inventory.Remove(this, equipment);
				equipment.Delete();
			}
		}
		TradedItems.Clear();
		if (HasRole(Role.Trader))
		{
			int num2 = 0;
			while (Inventory.GetWeight(this) > GetMaxInventoryWeight() * 0.65f && num2 < 5)
			{
				Equipment equipment2 = null;
				float num3 = float.MaxValue;
				int num4 = 0;
				for (int j = 0; j < Inventory.Count; j++)
				{
					Equipment item2 = Inventory.GetItem(j);
					if (item2.GetTradedAmount() != 0 || !item2.CanBeDestroyed() || item2.GetPrototype().Scarcity <= LootScarcity.Rare || !(item2.GetBasePrice() < 10f) || item2.GetPrototype() == EquipmentPrototype.Gold)
					{
						continue;
					}
					float num5 = item2.GetBasePrice() / Math.Max(item2.GetWeight(), 0.0001f);
					if (num5 < num3)
					{
						int amountToTransferLimit2 = GetAmountToTransferLimit(item2, bestWeapon, bestWeapon2, bestWaterBottle);
						if (amountToTransferLimit2 > 0)
						{
							equipment2 = item2;
							num3 = num5;
							num4 = amountToTransferLimit2;
						}
					}
				}
				if (equipment2 == null || Community == null)
				{
					break;
				}
				TileObject tileObject = null;
				float num6 = float.MaxValue;
				foreach (Prop building in Community.Buildings)
				{
					if (building.GetUnderConstructionInfo() == null && building.HasInventorySpaceFor(equipment2.GetWeight() * (float)num4))
					{
						float magnitude = (building.PosXZ - PosXZ).magnitude;
						if (magnitude < num6)
						{
							tileObject = building;
							num6 = magnitude;
						}
					}
				}
				foreach (Character member in Community.Members)
				{
					if (member != this && member.GetBaseObjectType() == BaseObjectType.Human && member.IsConscious && !member.HasRole(Role.Trader) && member.HasInventorySpaceFor(equipment2.GetWeight() * (float)num4 + member.GetMaxInventoryWeight() * (1f - GameTerrain.MaxLootFilledAmount)))
					{
						float num7 = (member.PosXZ - PosXZ).magnitude - member.GetAvailableInventorySpace();
						if (num7 < num6)
						{
							tileObject = member;
							num6 = num7;
						}
					}
				}
				if (tileObject == null)
				{
					break;
				}
				Equipment equipment3 = Inventory.Take(this, equipment2, num4);
				tileObject.GetInventory().Add(tileObject, equipment3);
				num2++;
			}
			ValueOfGoodsTradedWithPlayerAndSoldOn += num;
			while (TraderLevel < ProgressionToTraderLevel.Length - 1 && ValueOfGoodsTradedWithPlayerAndSoldOn >= ProgressionToTraderLevel[TraderLevel])
			{
				TraderLevel++;
			}
		}
		while (Community != null && Community.StolenGoodsRecords.Count > 0)
		{
			int index = deterministicRand.Next(Community.StolenGoodsRecords.Count);
			bool flag = false;
			foreach (Prop building2 in Community.Buildings)
			{
				Equipment equipment4 = building2.Inventory.FindFullestItemOfTypeWithLiquid(building2, Community.StolenGoodsRecords[index].Type, Community.StolenGoodsRecords[index].Liquid, Community.StolenGoodsRecords[index].InfectedWith);
				if (equipment4 != null)
				{
					int amount = Community.StolenGoodsRecords[index].Amount;
					amount = Math.Min(amount, Mathf.FloorToInt(GetAvailableInventorySpace() / equipment4.GetWeight()));
					if (amount > 0)
					{
						Inventory.Add(this, building2.Inventory.Take(building2, equipment4, amount));
						Community.RemoveStolenGoodsRecord(Community.StolenGoodsRecords[index].Type, Community.StolenGoodsRecords[index].Liquid, Community.StolenGoodsRecords[index].InfectedWith, amount);
						flag = true;
						break;
					}
					goto end_IL_05a5;
				}
			}
			if (!flag)
			{
				Community.StolenGoodsRecords.RemoveAt(index);
			}
			continue;
			end_IL_05a5:
			break;
		}
		bool flag2 = HasRole(Role.Trader);
		if (EquipmentPrototype.Gold != null)
		{
			int amount2 = deterministicRand.Next(flag2 ? 50 : 5) + Mathf.CeilToInt(num);
			SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.Gold, amount2, fillLiquidContainers: true);
		}
		if (EquipmentPrototype.Bandage != null)
		{
			int num8 = InitialBandageCount - Inventory.CountItemsOfType(EquipmentPrototype.Bandage);
			if (num8 > 0)
			{
				SpawnEquipmentIfSpaceIsAvailable(EquipmentPrototype.Bandage, num8, fillLiquidContainers: true);
			}
		}
		int lootCount = (flag2 ? 10 : 2);
		bool includeClothing = flag2 && deterministicRand.RandomChoice(TraderStockClothingProbability);
		GameTerrain.GenerateLoot(this, lootCount, deterministicRand, includeClothing, fillLiquidContainers: true);
		if (flag2 && Session.Instance.DifficultySettings.SaveTokensRequired && Session.Instance.DifficultySettings.TradersHaveSaveTokens)
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomItemOfClass(typeof(SavegameToken), deterministicRand);
			if (equipmentPrototype != null)
			{
				int num9 = deterministicRand.Next(TraderSaveTokenMin, TraderSaveTokenMax + 1);
				int num10 = Inventory.CountItemsOfClass(typeof(SavegameToken));
				if (num10 < num9)
				{
					SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype, num9 - num10, fillLiquidContainers: true);
				}
			}
		}
		RestockTime = TimeSpan.Zero;
	}

	public override bool WantPrediction()
	{
		if (!IsProne() && InTerrain && !Deleted)
		{
			return !Unity.IsWaitingForUMACharacterCreation();
		}
		return false;
	}

	public override bool IsPredicted()
	{
		return PredictedFreshFrame != -1;
	}

	public override bool IsAuthoritative()
	{
		return PredictedFreshFrame == -1;
	}

	public override int GetPredictedFreshFrame()
	{
		return PredictedFreshFrame;
	}

	public override TileObject GetPredicted()
	{
		return Predicted;
	}

	public override TileObject GetAuthoritative()
	{
		return Authoritative;
	}

	public override void SetPredictedFreshFrame(int frame)
	{
		PredictedFreshFrame = frame;
	}

	public override void SetPredicted(TileObject obj)
	{
		Predicted = (Character)obj;
	}

	public override void SetAuthoritative(TileObject obj)
	{
		Authoritative = (Character)obj;
	}

	public Character GetAuthoritativeOrElseThisCharacter()
	{
		if (!IsAuthoritative())
		{
			return Authoritative;
		}
		return this;
	}

	public Character GetPredictedOrElseThisCharacter()
	{
		if (Predicted == null)
		{
			return this;
		}
		return Predicted;
	}

	public override int GetCommunityId()
	{
		if (Community == null)
		{
			return 0;
		}
		return Community.Id;
	}

	public override Community GetCommunity()
	{
		return Community;
	}

	public override void SetCommunity(Community community)
	{
		if (IsPredicted())
		{
			Community = community;
		}
		else if (community != null)
		{
			community.AddMember(this);
		}
		else if (Community != null)
		{
			Community.RemoveMember(this);
		}
	}

	public override bool IsPlural()
	{
		if (Community != null)
		{
			return Community.GetLivingNonZombieMemberCount() > 1;
		}
		return false;
	}

	public override bool IsMany()
	{
		if (Community != null)
		{
			return Community.GetLivingNonZombieMemberCount() >= 5;
		}
		return false;
	}

	public override bool IsZero()
	{
		if (Community != null)
		{
			return Community.GetLivingNonZombieMemberCount() == 0;
		}
		return false;
	}

	public override Character GetAsCharacter()
	{
		return this;
	}

	public override Community GetCommunityThatOwnsThisArea()
	{
		TerrainCoord tile = GetTile();
		int ownerCommunityIdForTile = GameTerrain.Instance.GetOwnerCommunityIdForTile(tile.x, tile.y);
		if (ownerCommunityIdForTile != 0)
		{
			return BaseObjectManager.Instance.FindBaseObjectByID(ownerCommunityIdForTile) as Community;
		}
		return null;
	}

	public bool IsInMyCommunityOrAlly(TileObject obj)
	{
		if (Community == null)
		{
			return false;
		}
		int communityId = obj.GetCommunityId();
		if (communityId == Community.Id)
		{
			return true;
		}
		foreach (Community cachedAlly in Community.CachedAllies)
		{
			if (cachedAlly.Id == communityId)
			{
				return true;
			}
		}
		if (Community.IsFEMA)
		{
			Community community = obj.GetCommunity();
			if (community != null && community.IsFEMA)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInPlayerCommunity()
	{
		if (Community != null)
		{
			return Community.CommunityType == CommunityType.Player;
		}
		return false;
	}

	public bool IsInHiddenCommunity()
	{
		if (Community != null)
		{
			return Community.HiddenCommunity;
		}
		return false;
	}

	public bool IsAlly(TileObject obj)
	{
		if (Community == null)
		{
			return false;
		}
		int communityId = obj.GetCommunityId();
		foreach (Community cachedAlly in Community.CachedAllies)
		{
			if (cachedAlly.Id == communityId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsMyCommunityOrAlly(Community community)
	{
		if (Community == null)
		{
			return false;
		}
		if (Community == community)
		{
			return true;
		}
		return Community.CachedAllies.Contains(community);
	}

	public bool IsAlly(Community community)
	{
		if (Community == null)
		{
			return false;
		}
		return Community.CachedAllies.Contains(community);
	}

	public bool IsControllableByPlayer()
	{
		if (!AliveAndNotZombie)
		{
			return false;
		}
		if (Community != null && Community.CommunityType == CommunityType.Player)
		{
			return GetBaseObjectType() == BaseObjectType.Human;
		}
		return false;
	}

	public bool CanFollowPlayerIncludeAllies()
	{
		if (!AliveAndNotZombie)
		{
			return false;
		}
		if (GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (Community != null)
		{
			if (Community.CommunityType == CommunityType.Player)
			{
				return true;
			}
			if (Community.CachedAllies.Contains(Session.Instance.CommunityManager.PlayerCommunity))
			{
				return true;
			}
		}
		if (CanFollowPlayer)
		{
			return true;
		}
		return false;
	}

	public bool CanFollowPlayerIncludeNearbyAllies()
	{
		if (IsControllableByPlayer())
		{
			return true;
		}
		if (CanFollowPlayer || (Community != null && Community.CachedAllies.Contains(Session.Instance.CommunityManager.PlayerCommunity)))
		{
			return Session.Instance.IsWithinRangeOfMainCameraDeterministic(PosXZ, 128f);
		}
		return false;
	}

	public bool IsOnPlayersTeam()
	{
		if (!AliveAndNotZombie)
		{
			return false;
		}
		CommunityManager communityManager = Session.Instance.CommunityManager;
		if (Community != null)
		{
			return Community == communityManager.PlayerCommunity;
		}
		return false;
	}

	public bool IsControllableByOrFollowingPlayer()
	{
		if (!IsControllableByPlayer())
		{
			if (SquadLeader != null)
			{
				return SquadLeader.IsControllableByPlayer();
			}
			return false;
		}
		return true;
	}

	public bool IsSelectable()
	{
		return CanFollowPlayerIncludeAllies();
	}

	public void MarkPlayerControlled()
	{
		LastPlayerCommandTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
	}

	public void MarkFollowingPlayerControlled(Character leader)
	{
		LastPlayerCommandTime = leader.LastPlayerCommandTime;
	}

	public bool HasBeenPlayerControlledRecently(bool critical, bool extraCritical)
	{
		if (DirectControlledMajorAIDisabled)
		{
			return true;
		}
		PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(this);
		if (playerControllingCharacter != null && playerControllingCharacter.SyncedIsNavigatingMenus)
		{
			return true;
		}
		if (extraCritical)
		{
			return false;
		}
		return PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - LastPlayerCommandTime <= TimeSpan.FromSeconds(critical ? 10f : 60f);
	}

	public void FollowPlayer(Character controlledCharacter)
	{
		Follow(controlledCharacter);
		MarkPlayerControlled();
		CancelOneOffCrafting();
		if (Goal is SurvivorGoal survivorGoal)
		{
			survivorGoal.GetFollowGoal()?.OnStartFollowingPlayer(this);
		}
	}

	public void MakeMeGroupLeader()
	{
		if (SquadLeader == null)
		{
			return;
		}
		Character squadLeader = SquadLeader;
		Follow(null, canSetHangOutLocation: false);
		if (squadLeader.FindActiveGoal(GoalType.TreatGoal) is TreatGoal treatGoal && treatGoal.Recipient == this)
		{
			squadLeader.Follow(null);
			return;
		}
		if (squadLeader.DirectControlled)
		{
			IsAuthoritative();
			return;
		}
		squadLeader.Follow(this);
		while (squadLeader.Followers != null && squadLeader.Followers.Count > 0)
		{
			squadLeader.Followers[0].Follow(this, canSetHangOutLocation: false);
		}
	}

	public void SetDirectControlled(bool value, bool wantSetLastInputTime, bool wantClearLeaderCommand)
	{
		if (value)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe == null || playerControllingMe.FlyMode)
			{
				return;
			}
		}
		IsDirectControlled = value;
		if (IsDirectControlled)
		{
			MakeMeGroupLeader();
			if (wantSetLastInputTime)
			{
				LastDirectControlInputActionTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			}
			if (wantClearLeaderCommand)
			{
				ClearLeaderCommand();
			}
			DirectControlledMajorAIDisabled = true;
		}
		else
		{
			DisableSprint();
			DirectControlledAiming = false;
			SetDirectControlledTarget(null);
		}
	}

	public void SetDirectControlledTarget(Target target)
	{
		if (DirectControlledTarget != target)
		{
			if (DirectControlledTarget != null)
			{
				DirectControlledTarget.RefCount--;
			}
			DirectControlledTarget = target;
			if (DirectControlledTarget != null)
			{
				DirectControlledTarget.RefCount++;
			}
		}
	}

	public bool CanBreakOutOfDirectControlForMinorAI(bool alreadyActive, Role isRole)
	{
		return CanBreakOutOfDirectControlForMinorAI(alreadyActive, isRole, wasTriggeredFromDirectControl: false, isSpeechGoal: false);
	}

	public bool CanBreakOutOfDirectControlForMinorAI(bool alreadyActive, Role isRole, bool wasTriggeredFromDirectControl, bool isSpeechGoal)
	{
		PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(this);
		if (playerControllingCharacter != null && playerControllingCharacter.SyncedIsNavigatingMenus)
		{
			return false;
		}
		if (!alreadyActive && isRole != Role.None && InsideBuilding != null)
		{
			if (playerControllingCharacter != null)
			{
				if (RecentActivityType == RecentActivityType.EnteredBuilding && Session.Instance.PlayTime - RecentActivityTime <= AftermathGoal.MaxTimeSinceEnteredBuilding)
				{
					return false;
				}
				if (playerControllingCharacter.SyncedIsInInfoScreen && !playerControllingCharacter.JustSetRoleToUrgent && (InsideBuilding.GetBaseObjectType() != BaseObjectType.Mine || isRole != Role.Miner))
				{
					return false;
				}
			}
			if (InsideBuilding is EnterableVehicle { IsMoving: not false })
			{
				return false;
			}
		}
		if (!DirectControlledMajorAIDisabled)
		{
			return true;
		}
		if (!wasTriggeredFromDirectControl && UnderAttackRefCount > 0)
		{
			return false;
		}
		foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
		{
			if (playerRecord.SyncedSwappingSupplies == this)
			{
				return false;
			}
			if (playerRecord.SyncedSwappingSuppliesWith == this)
			{
				return false;
			}
		}
		if (!isSpeechGoal && !alreadyActive && playerControllingCharacter != null && playerControllingCharacter.TargetObject is Character && ((Character)playerControllingCharacter.TargetObject).IsAwake)
		{
			return false;
		}
		if (PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - LastDirectControlInputActionTime <= TimeSpan.FromSeconds(wasTriggeredFromDirectControl ? 0.1f : 3f))
		{
			return false;
		}
		if (!wasTriggeredFromDirectControl && playerControllingCharacter != null && !alreadyActive)
		{
			int num = MathUtil.Squared(GetSightRange());
			foreach (Character deterministicVisibleCharacter in playerControllingCharacter.DeterministicVisibleCharacters)
			{
				if (deterministicVisibleCharacter != this && deterministicVisibleCharacter.Alive && (deterministicVisibleCharacter.PosXZ - PosXZ).sqrMagnitude <= (float)num && IsEnemy(deterministicVisibleCharacter))
				{
					return false;
				}
				if (deterministicVisibleCharacter.GetBaseObjectType() != BaseObjectType.Human && IsCrouching() && EquippedItem != null && deterministicVisibleCharacter.LikesFood(EquippedItem.GetPrototype()))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void OnStopDirectControl()
	{
		DirectControlled = false;
		DirectControlledMajorAIDisabled = false;
		CancelSurrendering();
	}

	public bool IsAiming()
	{
		if (!DirectControlledAiming)
		{
			return GoalAimingRefCount > 0;
		}
		return true;
	}

	public bool IsCrouching()
	{
		if ((!DirectControlled || !DirectControlledCrouching) && GoalCrouchingRefCount <= 0)
		{
			if (!DirectControlled && DirectControlledCrouching && IsControllableByPlayer())
			{
				return IsInSurvivorGoalWithNoSubGoal();
			}
			return false;
		}
		return true;
	}

	public bool IsHiding()
	{
		if (IsOutdoors())
		{
			return IsCrouching();
		}
		if (InsideBuilding != null)
		{
			return !InsideBuilding.IsMovingVehicle();
		}
		return true;
	}

	public bool IsInSurvivorGoalWithNoSubGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.SubGoal == null;
		}
		return false;
	}

	public bool IsSitting()
	{
		return Sitting;
	}

	public virtual bool IsYouth()
	{
		return false;
	}

	public bool UnityIsInAimingAnim()
	{
		if (Unity.Animator != null && Unity.Animator.isInitialized)
		{
			if (Unity.Animator.GetCurrentAnimatorStateInfo(0).tagHash != AnimHash.Aiming)
			{
				return Unity.Animator.GetNextAnimatorStateInfo(0).tagHash == AnimHash.Aiming;
			}
			return true;
		}
		return IsAiming();
	}

	public PlayerRecord GetPlayerControllingMe()
	{
		if (!IsAuthoritative())
		{
			return PredictedObjectManager.Instance.GetPredictedPlayerControllingCharacter(this);
		}
		return Session.Instance.GetPlayerControllingCharacter(this);
	}

	public void UpdateDirectControlled(TimeSpan dt)
	{
		SprintCountdown = Math.Max(0, SprintCountdown - 1);
		PlayerRecord playerControllingMe = GetPlayerControllingMe();
		if (playerControllingMe == null)
		{
			return;
		}
		if (playerControllingMe.WantLockOnTarget && !DirectControlledAiming)
		{
			DirectControlledAimingStartTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		}
		DirectControlledAiming = playerControllingMe.WantLockOnTarget;
		SetDirectControlledTarget(null);
		if (playerControllingMe.WantLockOnTarget)
		{
			TileObject targetObject = playerControllingMe.TargetObject;
			if (targetObject != null)
			{
				SetDirectControlledTarget(GetOrCreateTarget(targetObject));
				DirectControlledTarget.ForceVisible(this);
			}
		}
	}

	public void SetGoal(Goal goal)
	{
		if (CurrentlyThinking == this)
		{
			Debug.LogError("Setting goal on currently thinking character: " + GetDisplayNameString() + ", " + GetGoalDebugString());
		}
		if (Goal != null)
		{
			Goal.OnDeactivate(this, null);
		}
		Goal = goal;
		if (Goal != null)
		{
			Goal.OnActivate(this, null);
		}
	}

	public Goal GetGoal()
	{
		return Goal;
	}

	public Goal FindActiveGoal(GoalType goalType)
	{
		Goal parent;
		return FindActiveGoal(goalType, out parent);
	}

	public Goal FindActiveGoal(GoalType goalType, out Goal parent)
	{
		parent = null;
		Goal goal = Goal;
		while (goal != null)
		{
			if (goal.GetGoalType() == goalType)
			{
				return goal;
			}
			StateMachineGoal stateMachineGoal = goal as StateMachineGoal;
			parent = goal;
			goal = stateMachineGoal?.SubGoal;
		}
		return null;
	}

	public Goal FindActiveGoalOfClass(Type type)
	{
		Goal parent;
		return FindActiveGoalOfClass(type, out parent);
	}

	public Goal FindActiveGoalOfClass(Type type, out Goal parent)
	{
		parent = null;
		Goal goal = Goal;
		while (goal != null)
		{
			if (type.IsInstanceOfType(goal))
			{
				return goal;
			}
			StateMachineGoal stateMachineGoal = goal as StateMachineGoal;
			parent = goal;
			goal = stateMachineGoal?.SubGoal;
		}
		return null;
	}

	public bool CanShowDialogOptions()
	{
		if (Goal != null)
		{
			return Goal.CanShowDialogOptions(this);
		}
		return false;
	}

	public virtual Texture2D GetOverheadActionIcon()
	{
		if (Goal == null)
		{
			return null;
		}
		return Goal.GetOverheadActionIcon(this);
	}

	public AIOverridesControlReason AIOverridesControl()
	{
		if (IsTooDepressedToFollowOrders() && IsLeaderConsciousAndNotZombie())
		{
			return AIOverridesControlReason.Depressed;
		}
		foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
		{
			if (playerRecord.PlayerMode == PlayerMode.Controlling && playerRecord.PlayerCharacter != null && playerRecord.PlayerCharacter != this && playerRecord.PlayerCharacter.CurrentActionAnim == ActionAnim.HandsUp && playerRecord.PlayerCharacter.FindActiveGoal(GoalType.Conversation) is Conversation conversation && conversation.AIOverridesControl(playerRecord.PlayerCharacter, null) == AIOverridesControlReason.Scripted)
			{
				return AIOverridesControlReason.Scripted;
			}
		}
		if (Goal == null)
		{
			return AIOverridesControlReason.None;
		}
		return Goal.AIOverridesControl(this, null);
	}

	public bool IsLeaderAliveAndNotZombie()
	{
		if (Community != null && Community.Leader != null)
		{
			return Community.Leader.AliveAndNotZombie;
		}
		return false;
	}

	public bool IsLeaderConsciousAndNotZombie()
	{
		if (Community != null && Community.Leader != null)
		{
			return Community.Leader.ConsciousAndNotZombie;
		}
		return false;
	}

	public bool IsTooDepressedToFollowOrders()
	{
		if (Community != null && Community.Leader == GetAuthoritativeOrElseThisCharacter())
		{
			return false;
		}
		if (AvatarForPlayer.IsValid())
		{
			return false;
		}
		if (Session.Instance.PlayTime < EncouragementTimeout)
		{
			return false;
		}
		if (DownTime > 0f)
		{
			return false;
		}
		if (!Session.Instance.FollowerCommandsEnabled && IsControllableByPlayer())
		{
			return false;
		}
		if (CurrentActionAnim == ActionAnim.HandsUp && IsControllableByPlayer())
		{
			return false;
		}
		if (Community != null && (Community.IsAmbientCommunity() || Community.CommunityType == CommunityType.Psycho))
		{
			return false;
		}
		if (Zombie || GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		Squad squad = GetSquad();
		if (squad != null && squad.Behaviour == SquadBehaviour.Ambush)
		{
			return false;
		}
		float num = (HasPersonality(CachedPersonalityType.Loyal) ? MoraleSuicidal : (HasPersonality(CachedPersonalityType.Fickle) ? MoraleMiserable : MoraleDepressed));
		return CalcMorale() <= num;
	}

	public void CancelOneOffCrafting()
	{
		CraftGoal craftGoal = GetCraftGoal();
		if (craftGoal != null && craftGoal.DesiredAmount != int.MaxValue)
		{
			craftGoal.StopRecipe(this);
			craftGoal.Finished = true;
		}
	}

	public void SetLeaderCommand(Goal goal, TileObject target, ObeyLeaderGoal.SourceType source)
	{
		if (Goal != null)
		{
			Goal.SetLeaderCommand(this, null, goal, target, source, null);
		}
	}

	public void ClearLeaderCommand()
	{
		if (Goal != null)
		{
			Goal.SetLeaderCommand(this, null, null, null, ObeyLeaderGoal.SourceType.Player, null);
		}
	}

	public Goal GetLeaderCommand()
	{
		if (Goal != null)
		{
			return Goal.GetLeaderCommand();
		}
		return null;
	}

	public Goal GetLeaderCommandEvenIfItIsInactive()
	{
		if (Goal != null)
		{
			return Goal.GetLeaderCommandEvenIfItIsInactive();
		}
		return null;
	}

	public bool IsLeaderCommandFinished()
	{
		if (Goal != null)
		{
			return Goal.IsLeaderCommandFinished(this);
		}
		return true;
	}

	public bool WasLeaderCommandSuccessful()
	{
		if (Goal != null)
		{
			return Goal.WasLeaderCommandSuccessful();
		}
		return true;
	}

	public bool DoIOrAnyoneInSquadHaveEquipmentOfType(EquipmentPrototype proto)
	{
		Squad squad = GetSquad();
		if (squad != null)
		{
			foreach (Character member in squad.Members)
			{
				if (member.Inventory.FindItemOfType(proto) != null)
				{
					return true;
				}
			}
			return false;
		}
		return Inventory.FindItemOfType(proto) != null;
	}

	public bool DoIOrAnyoneInSquadHaveEquipmentOfClass(Type type)
	{
		Squad squad = GetSquad();
		if (squad != null)
		{
			foreach (Character member in squad.Members)
			{
				if (member.Inventory.FindItemOfClass(type) != null)
				{
					return true;
				}
			}
		}
		return Inventory.FindItemOfClass(type) != null;
	}

	public bool DoIOrAnyoneInSquadHaveAllIngredients(Recipe recipe)
	{
		Squad squad = GetSquad();
		if (squad != null)
		{
			foreach (Character member in squad.Members)
			{
				if (recipe.HasAllIngredients(member, member, null, member))
				{
					return true;
				}
			}
		}
		return recipe.HasAllIngredients(this, this, null, this);
	}

	public void Follow(Character leader)
	{
		Follow(leader, canSetHangOutLocation: true);
	}

	public void Follow(Character leader, bool canSetHangOutLocation)
	{
		if (leader == this)
		{
			return;
		}
		if (leader != null)
		{
			ClearLeaderCommand();
			if (leader.Community == Session.Instance.CommunityManager.PlayerCommunity && SquadId != 0 && IsAuthoritative())
			{
				Community.RemoveFromSquad(this);
			}
		}
		if (SquadLeader != null && SquadLeader.Followers != null)
		{
			SquadLeader.Followers.Remove(this);
		}
		SquadLeader = leader;
		if (SquadLeader != null)
		{
			if (SquadLeader.Followers == null)
			{
				SquadLeader.Followers = new List<Character>();
			}
			SquadLeader.Followers.Add(this);
			if (Followers != null)
			{
				while (Followers.Count > 0)
				{
					if (Followers[0] == leader)
					{
						Followers[0].Follow(null);
					}
					else
					{
						Followers[0].Follow(leader);
					}
				}
			}
			DirectControlledCrouching = SquadLeader.DirectControlledCrouching;
		}
		if (Goal != null)
		{
			Goal.GetFollowGoal()?.OnSquadLeaderChanged(this, Goal);
		}
		if (canSetHangOutLocation && IsControllableByPlayer())
		{
			SetHangoutLocation((leader == null) ? Tile : TerrainCoord.Invalid);
		}
		if (IsAuthoritative())
		{
			if (IsControllableByPlayer() || CanFollowPlayer)
			{
				Hud.Instance.ShowFocusedCharacterHud();
			}
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public Character GetNearestFollower()
	{
		Character result = null;
		float num = float.MaxValue;
		if (Followers != null)
		{
			foreach (Character follower in Followers)
			{
				float sqrMagnitude = (follower.PosXZ - PosXZ).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = follower;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public Character GetNearestFollowerWhoCanAssassinateTarget(Character targetCharacter, HoldType holdType, List<Character> ignoreList)
	{
		FollowCommand followCommand = ((holdType == HoldType.SlitThroat) ? FollowCommand.Assassinate : FollowCommand.ChokdHold);
		Character result = null;
		float num = float.MaxValue;
		if (Followers != null)
		{
			foreach (Character follower in Followers)
			{
				float sqrMagnitude = (follower.PosXZ - targetCharacter.PosXZ).sqrMagnitude;
				if (sqrMagnitude < num && follower.IsConscious && (holdType != HoldType.SlitThroat || follower.Inventory.GetHuntingKnife() != null) && follower.GetFollowCommand() != followCommand && FollowGoal.CanFollowCommandBeIssuedTo(this, follower, followCommand) && follower.CouldChokeSuccessfully(targetCharacter, holdType) && !follower.WouldChokeBeDetectable(targetCharacter, ignoreList))
				{
					result = follower;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public void LeaveSquad()
	{
		while (Followers != null && Followers.Count > 0)
		{
			Followers[0].Follow(null);
		}
		Follow(null);
	}

	public void LeaveAllSquads()
	{
		if (SquadLeader != null)
		{
			Follow(null, canSetHangOutLocation: false);
		}
		if (Followers != null)
		{
			while (Followers.Count > 0)
			{
				Followers[0].Follow(null);
			}
		}
	}

	public void IssueCommandToFollowers(FollowCommand followCommand, TerrainCoord tile, EquipmentPrototype equipmentType, LiquidPrototype liquid = null, float amount = 0f)
	{
		IssueCommandToFollowers(followCommand, tile, equipmentType, liquid, amount, null);
	}

	public void IssueCommandToFollowers(FollowCommand followCommand, TerrainCoord tile, EquipmentPrototype equipmentType, LiquidPrototype liquid, float amount, TileObject target)
	{
		if (Followers == null)
		{
			return;
		}
		foreach (Character follower in Followers)
		{
			if (FollowGoal.CanFollowCommandBeIssuedTo(this, follower, followCommand))
			{
				follower.SetFollowCommand(followCommand, tile, equipmentType, liquid, amount, target);
			}
		}
	}

	public void SetFollowCommand(FollowCommand followCommand, TerrainCoord tile, EquipmentPrototype equipmentType, LiquidPrototype liquid = null, float amount = 0f, TileObject target = null)
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			survivorGoal.GetFollowGoal()?.SetFollowCommand(this, survivorGoal, followCommand, tile, equipmentType, liquid, amount, target);
		}
	}

	public void ClearFollowCommand()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			survivorGoal.GetFollowGoal()?.ClearFollowCommand(this, survivorGoal);
		}
	}

	public FollowCommand GetFollowCommand()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			FollowGoal followGoal = survivorGoal.GetFollowGoal();
			if (followGoal != null)
			{
				return followGoal.FollowCommand;
			}
		}
		return FollowCommand.Normal;
	}

	public bool IsSuperStealthyFollower()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			FollowGoal followGoal = survivorGoal.GetFollowGoal();
			if (followGoal != null)
			{
				if (followGoal.FollowCommand == FollowCommand.Assassinate || followGoal.FollowCommand == FollowCommand.ChokdHold)
				{
					return true;
				}
				if (followGoal.ExtraSuperStealthyTime > Session.Instance.PlayTime)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasFollowCommandActive()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			FollowGoal followGoal = survivorGoal.GetFollowGoal();
			if (followGoal != null)
			{
				return followGoal.FollowCommand != FollowCommand.Normal;
			}
		}
		return false;
	}

	public void LeaveCommunity(bool rejoinInitialCommunity, SecrecyMode secrecy, bool kicked, bool kickedDueToInfection)
	{
		LeaveCommunity(rejoinInitialCommunity, secrecy, kicked, kickedDueToInfection, null);
	}

	public void LeaveCommunity(bool rejoinInitialCommunity, SecrecyMode secrecy, bool kicked, bool kickedDueToInfection, Character kickedBy)
	{
		if (IsPlayerAvatar())
		{
			return;
		}
		CommunityManager communityManager = Session.Instance.CommunityManager;
		Community community = Community;
		Community community2;
		if (rejoinInitialCommunity && InitialCommunity != null && InitialCommunity != Community && (!InitialCommunity.IsAmbientCommunity() || !InitialCommunity.KeptAroundForReferences))
		{
			community2 = InitialCommunity;
			InitialCommunity.AddMemberWithNotifications(this, secrecy, kicked, kickedDueToInfection, kickedBy);
			SetHangoutLocation(InitialHangOutLocation);
		}
		else
		{
			community2 = Community.Spawn((Community != null && Community.IsLooterCommunity()) ? CommunityType.TemporaryLooter : CommunityType.Temporary);
			community2.CommunityName.Randomise(Session.Instance.DeterministicRand, unique: true, community2);
			community2.AddMemberWithNotifications(this, secrecy, kicked, kickedDueToInfection, kickedBy);
			InitialCommunity = community2;
			SetHangoutLocation(TerrainCoord.Invalid);
			if (community != null)
			{
				communityManager.SetRelationship(community, community2, CommunityRelationshipType.Known);
				if (community.CommunityType == CommunityType.Player)
				{
					community2.CommunityNameKnown = true;
					community2.CanOpenPlayerGates = global::CanOpenGates.No;
				}
				else
				{
					community2.CommunityNameKnown = NameKnown;
				}
			}
			if (AliveAndNotZombie)
			{
				Squad squad = community2.AddSquad(SquadBehaviour.Trade, 0);
				community2.AddToSquad(this, squad);
				community2.GoToNextTradeDestination(squad, Session.Instance.DeterministicRand, mustMove: false, canOccupyBases: false);
			}
		}
		if (community != null && community2 != null && community2.Members.Count == 1)
		{
			for (int i = 0; i < community.CommunityRelationships.Count; i++)
			{
				CommunityRelationshipType communityRelationshipType = community.CommunityRelationships[i].RelationshipType;
				if (communityRelationshipType == CommunityRelationshipType.Hostile)
				{
					communityRelationshipType = CommunityRelationshipType.Known;
				}
				if (communityRelationshipType == CommunityRelationshipType.Known || communityRelationshipType == CommunityRelationshipType.Ceasefire)
				{
					Community otherCommunity = community.CommunityRelationships[i].GetOtherCommunity();
					if (otherCommunity != null)
					{
						communityManager.SetRelationship(community2, otherCommunity, communityRelationshipType);
					}
				}
			}
		}
		if (community != null && community2 != null && community.CommunityType == CommunityType.Player)
		{
			community2.WarnAboutTraps(pitTraps: true, tripwires: true);
		}
	}

	public void SetScriptedGoalMarker(TileObject marker, MovementType movementType, bool disableWhenReachedMarker, ScriptedMoveImportance importance, bool teleportIfMoveFailed, bool sit = false, bool avoidHostileBases = false)
	{
		ScriptedGoalMarker = marker;
		if (Goal is SurvivorGoal survivorGoal)
		{
			survivorGoal.SetScriptedGoalMarker(this, null, marker, movementType, disableWhenReachedMarker, importance, teleportIfMoveFailed, sit, avoidHostileBases);
		}
	}

	public TileObject GetScriptedGoalMarker()
	{
		return ScriptedGoalMarker;
	}

	public ScriptedGoal GetScriptedGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.ScriptedGoal;
		}
		return null;
	}

	public GatherGoal GetGatherGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.GatherGoal;
		}
		return null;
	}

	public CaptureGoal GetCaptureGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.CaptureGoal;
		}
		return null;
	}

	public RepairGoal GetRepairGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.RepairGoal;
		}
		return null;
	}

	public CraftGoal GetCraftGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.CraftGoal;
		}
		return null;
	}

	public BuildGoal GetBuildGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.BuildGoal;
		}
		return null;
	}

	public WarmGoal GetWarmGoal()
	{
		if (Goal is SurvivorGoal survivorGoal)
		{
			return survivorGoal.WarmGoal;
		}
		return null;
	}

	public bool IsCollectingSomethingFrom(TileObject obj)
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsCollectingSomethingFrom(obj);
	}

	public bool IsSatisfyingNeeds()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsSatisfyingNeeds();
	}

	public bool IsCraftingWithProp(CraftingProp craftingProp)
	{
		CraftGoal craftGoal = GetCraftGoal();
		if (craftGoal != null)
		{
			if (craftGoal.Active && craftGoal.FollowingRecipe == craftingProp.CraftingRecipe)
			{
				return craftingProp.GetTileRect().Contains(craftGoal.DestTile);
			}
			return false;
		}
		return false;
	}

	private bool IsUsingIngredientForCrafting(Equipment item, bool willTransferContainers, Ingredient ingredient, ref int needed)
	{
		if (ingredient == null)
		{
			return false;
		}
		if (ingredient.Prototypes != null)
		{
			needed = Math.Max(needed, ingredient.Amount);
			if (item.GetAmount() <= ingredient.Amount)
			{
				return true;
			}
		}
		if (ingredient.LiquidTypes != null && item.GetLiquidContentsType() != null)
		{
			if (willTransferContainers)
			{
				float targetAmountToCarryIncludingAmmo = GetTargetAmountToCarryIncludingAmmo(null, item.GetLiquidContentsType(), item.InfectedWith);
				float num = 0f;
				foreach (Equipment content in Inventory.Contents)
				{
					if (content.GetLiquidContentsType() == item.GetLiquidContentsType() && content.MatchesIngredientInfectionState(ingredient.IngredientInfectionState))
					{
						if (item == content)
						{
							return true;
						}
						num += content.GetLiquidContentsAmount();
						if (num >= ingredient.LiquidAmount + targetAmountToCarryIncludingAmmo)
						{
							break;
						}
					}
				}
			}
			else if (Inventory.GetTotalLiquid(item.GetLiquidContentsType()) <= ingredient.LiquidAmount * 2f)
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsUsingForCrafting(Equipment item, bool willTransferContainers, FindType findType, out int needed)
	{
		needed = 0;
		CraftGoal craftGoal = GetCraftGoal();
		if (craftGoal != null)
		{
			if (craftGoal.FollowingRecipe != null && craftGoal.DesiredAmount > 0 && craftGoal.DesiredAmount != int.MaxValue)
			{
				Ingredient ingredient = craftGoal.FollowingRecipe.GetIngredient(item);
				if (IsUsingIngredientForCrafting(item, willTransferContainers, ingredient, ref needed))
				{
					return true;
				}
			}
			bool flag = MostRecentRoleIndex >= 0 && Roles[MostRecentRoleIndex].Role == Role.Organizer;
			for (int i = 0; i < Roles.Count; i++)
			{
				if (i != MostRecentRoleIndex && !flag && findType != FindType.AutoDeposit)
				{
					continue;
				}
				if (Roles[i].Role == Role.Crafter && Roles[i].Recipe != null)
				{
					Ingredient ingredient2 = Roles[i].Recipe.GetIngredient(item);
					if (IsUsingIngredientForCrafting(item, willTransferContainers, ingredient2, ref needed))
					{
						return true;
					}
				}
				else if (Roles[i].Role == Role.Cook && craftGoal.FollowingRecipe != null && craftGoal.DesiredAmount == int.MaxValue && craftGoal.FollowingRecipe.IsValidRecipeForCook())
				{
					Ingredient ingredient3 = craftGoal.FollowingRecipe.GetIngredient(item);
					if (IsUsingIngredientForCrafting(item, willTransferContainers, ingredient3, ref needed))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsAnyActiveRoleUsingForCrafting(Equipment item)
	{
		CraftGoal craftGoal = GetCraftGoal();
		if (craftGoal != null)
		{
			if (craftGoal.FollowingRecipe != null && craftGoal.DesiredAmount > 0 && craftGoal.DesiredAmount != int.MaxValue && craftGoal.FollowingRecipe.GetIngredient(item) != null)
			{
				return true;
			}
			for (int i = 0; i < Roles.Count; i++)
			{
				if (Roles[i].Paused)
				{
					continue;
				}
				if (Roles[i].Role == Role.Crafter && Roles[i].Recipe != null)
				{
					if (Roles[i].Recipe.GetIngredient(item) != null)
					{
						return true;
					}
				}
				else if (Roles[i].Role == Role.Cook && craftGoal.FollowingRecipe != null && craftGoal.DesiredAmount == int.MaxValue && craftGoal.FollowingRecipe.IsValidRecipeForCook() && craftGoal.FollowingRecipe.GetIngredient(item) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public TileObject IsBuildingSomething()
	{
		if (Goal is SurvivorGoal { BuildGoal: { CurrentBuilding: not null } buildGoal })
		{
			return buildGoal.CurrentBuilding;
		}
		return null;
	}

	public bool IsInsideBuildingUnderConstruction()
	{
		TileObject building;
		return IsInsideBuildingUnderConstruction(out building);
	}

	public bool IsInsideBuildingUnderConstruction(out TileObject building)
	{
		building = null;
		if (FindActiveGoal(GoalType.BuildGoal) is BuildGoal buildGoal)
		{
			return buildGoal.IsInsideBuilding(this, out building);
		}
		return false;
	}

	public bool IsInsideBuildingUnderConstruction(TileObject building)
	{
		if (IsInsideBuildingUnderConstruction(out var building2))
		{
			return building2 == building;
		}
		return false;
	}

	public bool IsDoingSomethingTerriblyImportant()
	{
		if (InCombat || IsLowAlert())
		{
			return true;
		}
		if (Goal != null)
		{
			return Goal.IsDoingSomethingTerriblyImportant();
		}
		return false;
	}

	public void ResetGatherGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { GatherGoal: { } gatherGoal })
		{
			gatherGoal.ResetLastAttemptedTime();
			gatherGoal.Finished = true;
		}
	}

	public void ResetFarmingGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { FarmingGoal: { } farmingGoal })
		{
			farmingGoal.ResetLastAttemptedTime();
			farmingGoal.Finished = true;
		}
	}

	public void ResetGuardGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { GuardGoal: { } guardGoal })
		{
			guardGoal.ResetLastAttemptedTime();
			guardGoal.Finished = true;
		}
	}

	public void ResetMedicGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { MedicGoal: { } medicGoal })
		{
			medicGoal.ResetLastAttemptedTime();
			medicGoal.Finished = true;
		}
	}

	public void ResetBuildGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { BuildGoal: { } buildGoal })
		{
			buildGoal.ResetLastAttemptedTime();
			buildGoal.Finished = true;
		}
	}

	public void SetBuildGoalLastAttemptedTimeToNow()
	{
		if (Goal is SurvivorGoal { BuildGoal: { } buildGoal })
		{
			buildGoal.SetLastAttemptedTimeToNow();
		}
	}

	public void ResetCraftGoalLastAttemptedTimeAndFinish(bool notIfDoingOneOffCrafting)
	{
		if (Goal is SurvivorGoal { CraftGoal: { } craftGoal } && (!notIfDoingOneOffCrafting || craftGoal.DesiredAmount == int.MaxValue))
		{
			craftGoal.ResetLastAttemptedTime();
			craftGoal.Finished = true;
		}
	}

	public void ResetCraftGoalHasSaidInsufficientResources()
	{
		if (Goal is SurvivorGoal { CraftGoal: { } craftGoal })
		{
			craftGoal.HasSaidInsufficientResources = false;
		}
	}

	public void ResetLumberjackGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { LumberjackGoal: { } lumberjackGoal })
		{
			lumberjackGoal.ResetLastAttemptedTime();
			lumberjackGoal.Finished = true;
		}
	}

	public void ResetMinerGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { MinerGoal: { } minerGoal })
		{
			minerGoal.ResetLastAttemptedTime();
			minerGoal.Finished = true;
		}
	}

	public void ResetTrapperGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { TrapperGoal: { } trapperGoal })
		{
			trapperGoal.ResetLastAttemptedTime();
			trapperGoal.Finished = true;
		}
	}

	public void ResetAnimalFeederGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { AnimalFeederGoal: { } animalFeederGoal })
		{
			animalFeederGoal.ResetLastAttemptedTime();
			animalFeederGoal.Finished = true;
		}
	}

	public void ResetOrganizerGoalLastAttemptedTimeAndFinish()
	{
		if (Goal is SurvivorGoal { OrganizerGoal: { } organizerGoal })
		{
			organizerGoal.ResetLastAttemptedTime();
			organizerGoal.Finished = true;
		}
	}

	public void SetPersonality(string personalityType, bool val)
	{
		if (val)
		{
			if (!Personality.Contains(personalityType))
			{
				Personality.Add(personalityType);
			}
		}
		else if (!val)
		{
			Personality.Remove(personalityType);
		}
		CachePersonality();
	}

	public void SetPersonality(CachedPersonalityType personalityType, bool val)
	{
		string item = PersonalitiesList.CachedType[(int)personalityType];
		if (val)
		{
			if (!Personality.Contains(item))
			{
				Personality.Add(item);
			}
			CachedPersonality |= 1 << (int)personalityType;
		}
		else if (!val)
		{
			Personality.Remove(item);
			CachedPersonality &= ~(1 << (int)personalityType);
		}
	}

	public void RandomizePersonality(CustomRandom rand, string faction)
	{
		Personality.Clear();
		foreach (PersonalityGroup allPersonalityGroup in GameImpl.Instance.GetAllPersonalityGroups(faction))
		{
			float num = rand.RandomFloat() * 100f;
			float num2 = 0f;
			for (int i = 0; i < allPersonalityGroup.Personalities.Count; i++)
			{
				num2 += allPersonalityGroup.Personalities[i].Probability;
				if (num <= num2)
				{
					Personality.Add(allPersonalityGroup.Personalities[i].Personality);
					break;
				}
			}
		}
		CachePersonality();
	}

	public void CachePersonality()
	{
		CachedPersonality = 0;
		for (int i = 0; i < 28; i++)
		{
			if (HasPersonality(PersonalitiesList.CachedType[i]))
			{
				CachedPersonality |= 1 << i;
			}
		}
	}

	public bool HasPersonality(string personalityType)
	{
		return Personality.Contains(personalityType);
	}

	public bool HasPersonality(CachedPersonalityType personalityType)
	{
		return (CachedPersonality & (1 << (int)personalityType)) != 0;
	}

	public int GetPersonality(string positivePersonalityType, string negativePersonalityType)
	{
		int num = 0;
		if (HasPersonality(positivePersonalityType))
		{
			num++;
		}
		if (HasPersonality(negativePersonalityType))
		{
			num--;
		}
		return num;
	}

	public int GetPersonality(CachedPersonalityType positivePersonalityType, CachedPersonalityType negativePersonalityType)
	{
		int num = 0;
		if (HasPersonality(positivePersonalityType))
		{
			num++;
		}
		if (HasPersonality(negativePersonalityType))
		{
			num--;
		}
		return num;
	}

	public bool IsPersonalityKnown(int i)
	{
		return (PersonalityKnown & (1 << i)) != 0;
	}

	public bool HasAnyUnknownPersonality()
	{
		for (int i = 0; i < Personality.Count; i++)
		{
			if (!IsPersonalityKnown(i))
			{
				return true;
			}
		}
		return false;
	}

	public void SetPersonalityKnown(int i)
	{
		PersonalityKnown |= (uint)(1 << i);
		if (InfoScreen.Instance.IsShowingBrainScanFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void SetAllPersonalitiesKnown()
	{
		PersonalityKnown = uint.MaxValue;
		if (InfoScreen.Instance.IsShowingBrainScanFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public bool IsPersonalityKnown(string personalityType)
	{
		int num = Personality.IndexOf(personalityType);
		if (num >= 0 && num < 16)
		{
			return IsPersonalityKnown(num);
		}
		return false;
	}

	public void SetPersonalityKnown(string personalityType)
	{
		int num = Personality.IndexOf(personalityType);
		if (num >= 0 && num < 16)
		{
			SetPersonalityKnown(num);
		}
	}

	public void SetSkillKnown(SkillType skillType)
	{
		Skillset.SetSkillKnown(skillType);
		if (InfoScreen.Instance != null && InfoScreen.Instance.IsShowingBrainScanFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void SetAllSkillsKnown()
	{
		Skillset.SetAllSkillsKnown();
		if (InfoScreen.Instance != null && InfoScreen.Instance.IsShowingBrainScanFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void RandomizeSurvivalFactors(CustomRandom rand)
	{
		SetSleepDeprivation(rand.RandomFloat() * SleepyTime);
		SetHunger(rand.RandomFloat() * HungryTime);
		SetThirst(rand.RandomFloat() * ThirstyTime);
	}

	public void RandomizeInvisibleStrain(TemplateHasInvisibleStrain hasInvisibleStrain, CustomRandom rand)
	{
		InvisibleStrainType invisibleStrainType = InvisibleStrainType.None;
		switch (hasInvisibleStrain)
		{
		case TemplateHasInvisibleStrain.Default:
			if (rand.RandomChoice(Session.Instance.DifficultySettings.InvisibleStrainPercentage / 100f))
			{
				invisibleStrainType = (rand.RandomChoice(0.5f) ? InvisibleStrainType.Excitable : InvisibleStrainType.Subtle);
			}
			break;
		case TemplateHasInvisibleStrain.Yes:
			invisibleStrainType = (rand.RandomChoice(0.5f) ? InvisibleStrainType.Excitable : InvisibleStrainType.Subtle);
			break;
		case TemplateHasInvisibleStrain.Excitable:
			invisibleStrainType = InvisibleStrainType.Excitable;
			break;
		case TemplateHasInvisibleStrain.Subtle:
			invisibleStrainType = InvisibleStrainType.Subtle;
			break;
		}
		if (invisibleStrainType != InvisibleStrainType.None)
		{
			HadInvisibleStrainFromStart = true;
			InvisibleStrain = invisibleStrainType;
		}
	}

	public bool WantToDeclareWar(TileObject obj)
	{
		if (obj is Character character)
		{
			if (!character.Alive)
			{
				return false;
			}
			if (Community != null && character.Community != null)
			{
				if (character.Community == Community)
				{
					return false;
				}
				CommunityRelationshipType relationship = Session.Instance.CommunityManager.GetRelationship(Community, character.Community);
				if (relationship == CommunityRelationshipType.Ceasefire || relationship == CommunityRelationshipType.Allied)
				{
					return false;
				}
			}
			if (Rank == Rank.Captive)
			{
				return false;
			}
			if (Rank != Rank.Leader && IsLeaderAliveAndNotZombie())
			{
				return false;
			}
			float approval = 0f;
			float respect = 0f;
			CalcApprovalRatingSinceLastFight(character, out approval, out respect);
			GetAttackOnSightApprovalThreshold(out var withMinRespect, out var withMaxRespect);
			Vector2 vector = new Vector2(respect, approval);
			Vector2 vector2 = new Vector2(-100f, withMinRespect);
			Vector2 vector3 = new Vector2(100f, withMaxRespect) - vector2;
			return Vector2.Dot(rhs: new Vector2(0f - vector3.y, vector3.x), lhs: vector - vector2) <= 0f;
		}
		return false;
	}

	public static bool RespectExceeds(float approval, float respect, float requiredRespectAt0Approval, float approvalRange)
	{
		Vector2 vector = new Vector2(respect, approval);
		Vector2 vector2 = new Vector2(requiredRespectAt0Approval + approvalRange, -100f);
		Vector2 vector3 = new Vector2(requiredRespectAt0Approval - approvalRange, 100f) - vector2;
		return Vector2.Dot(rhs: new Vector2(0f - vector3.y, vector3.x), lhs: vector - vector2) <= 0f;
	}

	public static bool ApprovalExceeds(float approval, float respect, float requiredApprovalAt0Respect, float respectRange)
	{
		Vector2 vector = new Vector2(respect, approval);
		Vector2 vector2 = new Vector2(-100f, requiredApprovalAt0Respect + respectRange);
		Vector2 vector3 = new Vector2(100f, requiredApprovalAt0Respect - respectRange) - vector2;
		return Vector2.Dot(rhs: new Vector2(0f - vector3.y, vector3.x), lhs: vector - vector2) >= 0f;
	}

	public bool WantJoinCommunity(Character asker, bool threatened, bool checkRelationships, List<Character> checkedCharacters = null)
	{
		if (DontLeaveCommunity)
		{
			return false;
		}
		if (asker.Community == null)
		{
			return false;
		}
		if (WantLeaveCommunity(asker.Community, checkRelationships: false))
		{
			return false;
		}
		if (asker.Community == Community)
		{
			return true;
		}
		if (asker.Community.CommunityType != CommunityType.Player && asker.Community.Leader != null && asker.Community.Leader.AliveAndNotZombie && asker.Community.Leader.WantKickFromCommunity(asker, checkRelationships: true))
		{
			return false;
		}
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(asker, out approval, out respect);
		GetJoinCommunityRespectThreshold(out var withMinApproval, out var withMaxApproval, threatened);
		float num = Mathf.Lerp(withMinApproval, withMaxApproval, 0.5f);
		float approvalRange = withMinApproval - num;
		if (!RespectExceeds(approval, respect, num, approvalRange))
		{
			return false;
		}
		if (checkRelationships)
		{
			if (checkedCharacters == null)
			{
				checkedCharacters = new List<Character>();
			}
			checkedCharacters.Add(this);
			for (int i = 0; i < Relationships.Count; i++)
			{
				if (Relationships[i].RelationshipTarget != null && Relationships[i].RelationshipTarget.Community == Community && !checkedCharacters.Contains(Relationships[i].RelationshipTarget) && HasGoodRelationship(Relationships[i].RelationshipTarget) && !Relationships[i].RelationshipTarget.WantJoinCommunity(asker, threatened, checkRelationships: true, checkedCharacters))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool WantLeaveCommunity(Community community, bool checkRelationships, List<Character> checkedCharacters = null)
	{
		if (community.Leader == null || !community.Leader.AliveAndNotZombie)
		{
			return false;
		}
		if (DontLeaveCommunity && community == Community)
		{
			return false;
		}
		if (community.CommunityType == CommunityType.Player)
		{
			if (IsPlayerAvatar())
			{
				return false;
			}
			if (DontAbandonPlayer)
			{
				return false;
			}
			if (checkRelationships)
			{
				if (checkedCharacters == null)
				{
					checkedCharacters = new List<Character>();
				}
				checkedCharacters.Add(this);
				for (int i = 0; i < Relationships.Count; i++)
				{
					if (Relationships[i].RelationshipTarget != null && Relationships[i].RelationshipTarget.Community == community && !checkedCharacters.Contains(Relationships[i].RelationshipTarget) && HasGoodRelationship(Relationships[i].RelationshipTarget) && !Relationships[i].RelationshipTarget.WantLeaveCommunity(community, checkRelationships: true, checkedCharacters))
					{
						return false;
					}
				}
			}
		}
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(community.Leader, out approval, out respect);
		GetLeaveCommunityRespectThreshold(out var withMinApproval, out var withMaxApproval);
		float num = Mathf.Lerp(withMinApproval, withMaxApproval, 0.5f);
		float approvalRange = withMinApproval - num;
		return !RespectExceeds(approval, respect, num, approvalRange);
	}

	public bool WantKickFromCommunity(Character character, bool checkRelationships, List<Character> checkedCharacters = null)
	{
		if (character.DontLeaveCommunity)
		{
			return false;
		}
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(character, out approval, out respect);
		GetKickFromCommunityApprovalThreshold(out var withMinRespect, out var withMaxRespect);
		float num = Mathf.Lerp(withMinRespect, withMaxRespect, 0.5f);
		float respectRange = withMinRespect - num;
		if (ApprovalExceeds(approval, respect, num, respectRange))
		{
			return false;
		}
		if (checkRelationships)
		{
			if (checkedCharacters == null)
			{
				checkedCharacters = new List<Character>();
			}
			checkedCharacters.Add(character);
			for (int i = 0; i < character.Relationships.Count; i++)
			{
				if (character.Relationships[i].RelationshipTarget != null && character.Relationships[i].RelationshipTarget.Community == Community && !checkedCharacters.Contains(character.Relationships[i].RelationshipTarget) && character.HasGoodRelationship(character.Relationships[i].RelationshipTarget) && !WantKickFromCommunity(character.Relationships[i].RelationshipTarget, checkRelationships: true, checkedCharacters))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void GetAttackOnSightApprovalThreshold(out float withMinRespect, out float withMaxRespect)
	{
		withMinRespect = -70f;
		withMaxRespect = -110f;
		if (HasPersonality(CachedPersonalityType.Sociopath))
		{
			withMinRespect -= 20f;
		}
		float num = GetPersonality(CachedPersonalityType.Bold, CachedPersonalityType.Nervous);
		withMinRespect -= num * 10f;
		withMaxRespect -= num * 20f;
		float num2 = GetPersonality(CachedPersonalityType.Aggressive, CachedPersonalityType.Passive);
		num2 -= (HasPersonality(CachedPersonalityType.PassiveAggressive) ? 1f : 0f);
		withMinRespect -= num2 * 10f;
		withMaxRespect -= num2 * 20f;
	}

	public void GetJoinCommunityRespectThreshold(out float withMinApproval, out float withMaxApproval, bool threatened)
	{
		withMaxApproval = 20f;
		withMinApproval = 120f;
		bool flag = Community != null && Community.CommunityType == CommunityType.RovingRefugee;
		if (flag)
		{
			withMinApproval -= 60f;
			withMaxApproval -= 70f;
		}
		if (threatened)
		{
			float num = GetPersonality(CachedPersonalityType.Bold, CachedPersonalityType.Nervous);
			withMinApproval += num * ((flag && num > 0f) ? 10f : 40f);
		}
		float num2 = GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		if (Community == null || Community.GetLivingNonZombieMemberCount() == 1)
		{
			num2 = -1f;
		}
		withMaxApproval += num2 * ((flag && num2 > 0f) ? 10f : 40f);
	}

	public void GetLeaveCommunityRespectThreshold(out float withMinApproval, out float withMaxApproval)
	{
		withMaxApproval = -60f;
		withMinApproval = 0f;
		float num = GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		withMaxApproval -= num * 40f;
	}

	public void GetKickFromCommunityApprovalThreshold(out float withMinRespect, out float withMaxRespect)
	{
		withMinRespect = -50f;
		withMaxRespect = -90f;
		float num = GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		withMinRespect -= num * 40f;
		withMaxRespect -= num * 40f;
	}

	public bool HasGoodRelationship(Character other)
	{
		if (!other.AliveAndNotZombie)
		{
			return false;
		}
		RelationshipType relationship = Relationship.GetRelationship(this, other);
		if ((uint)(relationship - 1) <= 5u)
		{
			return HasEnoughApprovalForGoodRelationship(other);
		}
		return false;
	}

	public int GetNumGoodRelationshipsInCommunity(bool recurse, Community community, List<Character> checkedCharacters = null)
	{
		int num = 0;
		if (checkedCharacters == null)
		{
			checkedCharacters = new List<Character>();
		}
		checkedCharacters.Add(this);
		for (int i = 0; i < Relationships.Count; i++)
		{
			if (Relationships[i].RelationshipTarget != null && Relationships[i].RelationshipTarget.Community == community && !checkedCharacters.Contains(Relationships[i].RelationshipTarget) && HasGoodRelationship(Relationships[i].RelationshipTarget))
			{
				num++;
				if (recurse)
				{
					num += Relationships[i].RelationshipTarget.GetNumGoodRelationshipsInCommunity(recurse: true, community, checkedCharacters);
				}
			}
		}
		return num;
	}

	public bool HasEnoughApprovalForGoodRelationship(Character other)
	{
		float approval = 0f;
		float respect = 0f;
		CalcApprovalRating(other, out approval, out respect);
		float num = GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		float num2 = -40f - num * 40f;
		return approval >= num2;
	}

	public void SetMoraleCacheDirty()
	{
		CachedMoraleDirty = true;
	}

	public float CalcMorale()
	{
		if (IsPredicted())
		{
			return Authoritative.CalcMorale();
		}
		if (CachedMoraleDirty)
		{
			float num = 0f;
			for (int i = 0; i < Memories.Count; i++)
			{
				num += Memories[i].MoraleContribution;
			}
			CachedMorale = Mathf.Clamp(num, -100f, 100f);
			CachedMoraleDirty = false;
		}
		return CachedMorale;
	}

	public override void CalcApprovalRatingForCharacterOrCommunity(BaseObject obj, out float approval, out float respect, TimeSpan afterTime, TimeSpan beforeTime)
	{
		if (obj is Character character)
		{
			CalcApprovalRating(character, out approval, out respect, afterTime, beforeTime);
		}
		else if (obj is Community community)
		{
			CalcApprovalRating(community, out approval, out respect, afterTime, beforeTime);
		}
		else
		{
			base.CalcApprovalRatingForCharacterOrCommunity(obj, out approval, out respect, afterTime, beforeTime);
		}
	}

	public void ClearOpinionCacheForMemory(ref Memory memory)
	{
		if (memory.Prototype.RuleSet == MemoryRuleSet.OpinionOfGroup)
		{
			CachedOpinion.Clear();
			return;
		}
		ClearOpinionCacheForObject(memory.Actor);
		if (memory.Prototype.RuleSet == MemoryRuleSet.Envy)
		{
			ClearOpinionCacheForObject(memory.Object);
		}
	}

	public void ClearOpinionCacheForObject(BaseObject obj)
	{
		if (obj != null)
		{
			CachedOpinion.Remove(obj);
			if (obj is Character { Community: not null } character)
			{
				CachedOpinion.Remove(character.Community);
			}
		}
	}

	private bool GetCachedOpinion(BaseObject obj, out float approval, out float respect)
	{
		Vector2 value2;
		if (StoryManager.IsEvaluatingSpeechOptionsOnThread)
		{
			lock (CachedOpinion)
			{
				if (CachedOpinion.TryGetValue(obj, out var value))
				{
					respect = value.x;
					approval = value.y;
					return true;
				}
			}
		}
		else if (CachedOpinion.TryGetValue(obj, out value2))
		{
			respect = value2.x;
			approval = value2.y;
			return true;
		}
		respect = 0f;
		approval = 0f;
		return false;
	}

	private void CacheOpinion(BaseObject obj, float approval, float respect)
	{
		if (StoryManager.IsEvaluatingSpeechOptionsOnThread)
		{
			lock (CachedOpinion)
			{
				CachedOpinion[obj] = new Vector2(respect, approval);
				return;
			}
		}
		CachedOpinion[obj] = new Vector2(respect, approval);
	}

	public void CalcApprovalRating(Character character, out float approval, out float respect)
	{
		CalcApprovalRating(character, out approval, out respect, Target.Never, TimeSpan.MaxValue);
	}

	public void CalcApprovalRating(Character character, out float approval, out float respect, TimeSpan afterTime, TimeSpan beforeTime, bool fromPredicted = false)
	{
		if (IsPredicted())
		{
			Authoritative.CalcApprovalRating(character, out approval, out respect, afterTime, beforeTime, fromPredicted: true);
			return;
		}
		if (character == this)
		{
			approval = 100f;
			respect = 0f;
			return;
		}
		if (character.Surname.Length == 0 && (GetBaseObjectType() != BaseObjectType.Human || Zombie))
		{
			approval = 0f;
			respect = 0f;
			return;
		}
		bool flag = afterTime <= Target.Never && beforeTime >= TimeSpan.MaxValue;
		if (flag && GetCachedOpinion(character, out approval, out respect))
		{
			return;
		}
		approval = 0f;
		respect = 0f;
		if (afterTime <= TimeSpan.Zero)
		{
			for (int i = 0; i < Relationships.Count; i++)
			{
				if (Relationships[i].RelationshipTarget == character)
				{
					approval += Relationships[i].ApprovalContribution;
					respect += Relationships[i].RespectContribution;
				}
			}
		}
		for (int j = 0; j < Memories.Count; j++)
		{
			if (Memories[j].Prototype.RuleSet == MemoryRuleSet.OpinionOfGroup)
			{
				if (Memories[j].Object == character.Community && Memories[j].Actor == this && Memories[j].Time > afterTime && Memories[j].Time < beforeTime)
				{
					approval += Memories[j].ApprovalContribution;
					respect += Memories[j].RespectContribution;
				}
			}
			else if (Memories[j].Actor == character && Memories[j].Time > afterTime && Memories[j].Time < beforeTime)
			{
				approval += Memories[j].ApprovalContribution;
				respect += Memories[j].RespectContribution;
			}
			else if (Memories[j].Prototype.RuleSet == MemoryRuleSet.Envy && Memories[j].Object == character && Memories[j].Time > afterTime && Memories[j].Time < beforeTime && Memories[j].ApprovalContribution < 0f)
			{
				approval += Memories[j].ApprovalContribution;
			}
		}
		approval = Mathf.Clamp(approval, -100f, 100f);
		respect = Mathf.Clamp(respect, -100f, 100f);
		if (flag && !fromPredicted)
		{
			CacheOpinion(character, approval, respect);
		}
	}

	public void CalcApprovalRating(Community community, out float approval, out float respect)
	{
		CalcApprovalRating(community, out approval, out respect, Target.Never, TimeSpan.MaxValue);
	}

	public void CalcApprovalRating(Community community, out float approval, out float respect, TimeSpan afterTime, TimeSpan beforeTime, bool fromPredicted = false)
	{
		if (IsPredicted())
		{
			Authoritative.CalcApprovalRating(community, out approval, out respect, afterTime, beforeTime, fromPredicted: true);
			return;
		}
		if (community.IsZombieCommunity() || community.IsAnimalCommunity())
		{
			approval = 0f;
			respect = 0f;
			return;
		}
		bool flag = afterTime <= Target.Never && beforeTime >= TimeSpan.MaxValue;
		if (flag && GetCachedOpinion(community, out approval, out respect))
		{
			return;
		}
		approval = 0f;
		respect = 0f;
		if (afterTime <= TimeSpan.Zero)
		{
			for (int i = 0; i < Relationships.Count; i++)
			{
				if (Relationships[i].RelationshipTarget != null && Relationships[i].RelationshipTarget.Community == community)
				{
					approval += Relationships[i].ApprovalContribution;
					respect += Relationships[i].RespectContribution;
				}
			}
		}
		for (int j = 0; j < Memories.Count; j++)
		{
			if (Memories[j].Actor != null && Memories[j].Actor.Community == community && Memories[j].Time > afterTime && Memories[j].Time < beforeTime)
			{
				approval += Memories[j].ApprovalContribution;
				respect += Memories[j].RespectContribution;
			}
			else if (Memories[j].Prototype.RuleSet == MemoryRuleSet.Envy && (Memories[j].Object == community || (Memories[j].Object != null && Memories[j].Object.GetCommunity() == community)) && Memories[j].Time > afterTime && Memories[j].Time < beforeTime && Memories[j].ApprovalContribution < 0f)
			{
				approval += Memories[j].ApprovalContribution;
			}
		}
		approval = Mathf.Clamp(approval, -100f, 100f);
		respect = Mathf.Clamp(respect, -100f, 100f);
		if (flag && !fromPredicted)
		{
			CacheOpinion(community, approval, respect);
		}
	}

	public void CalcApprovalRatingSinceLastFight(Character character, out float approval, out float respect)
	{
		TimeSpan timeSpan = Target.Never;
		for (int i = 0; i < Memories.Count; i++)
		{
			if ((Memories[i].Prototype == MemoryPrototype.BeatUp || Memories[i].Prototype == MemoryPrototype.FoughtOff || Memories[i].Prototype == MemoryPrototype.Coup) && ((Memories[i].Actor == this && Memories[i].Object == character) || (Memories[i].Actor == character && Memories[i].Object == this)))
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
			if (Memories[i].Prototype == MemoryPrototype.RestrainedSuccessfully && ((Memories[i].Object == this && Memories[i].ThirdParty == character) || (Memories[i].Object == character && Memories[i].ThirdParty == this)))
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
			if ((Memories[i].Prototype == MemoryPrototype.Rejected || Memories[i].Prototype == MemoryPrototype.KickedFromCommunity || Memories[i].Prototype == MemoryPrototype.KickedPossibleInvisibleStrain) && Memories[i].Actor == this && Memories[i].Object == character)
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
			if (Memories[i].Prototype == MemoryPrototype.LeadershipDefeated && Community != null && character.Community != null && (Memories[i].Actor == Community.Leader || Memories[i].Actor == character.Community.Leader) && ((InitialCommunity != null && Memories[i].Object == InitialCommunity) || (character.InitialCommunity != null && Memories[i].Object == character.InitialCommunity)))
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
			if (Memories[i].Prototype == MemoryPrototype.Surrendered && Community != null && character.Community != null && Memories[i].Actor != null && Memories[i].Object != null && ((Memories[i].Actor.Community == character.Community && Memories[i].Object.GetCommunity() == Community) || (Memories[i].Actor.Community == Community && Memories[i].Object.GetCommunity() == character.Community)))
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
			if ((Memories[i].Prototype == MemoryPrototype.Rescued || Memories[i].Prototype == MemoryPrototype.NeededRescue) && Memories[i].Actor == character && Memories[i].Object == this)
			{
				timeSpan = MathUtil.Max(timeSpan, Memories[i].Time);
			}
		}
		CalcApprovalRating(character, out var approval2, out var respect2, Target.Never, timeSpan);
		CalcApprovalRating(character, out var approval3, out var respect3, timeSpan, TimeSpan.MaxValue);
		approval = approval3 + Math.Max(0f, approval2);
		respect = respect3 + respect2;
	}

	public void CalcAbsApprovalRating(Character character, out float approval, out float respect)
	{
		if (IsPredicted())
		{
			Authoritative.CalcApprovalRating(character, out approval, out respect);
			return;
		}
		approval = 0f;
		respect = 0f;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Actor == character)
			{
				approval += Math.Abs(Memories[i].ApprovalContribution);
				respect += Math.Abs(Memories[i].RespectContribution);
			}
		}
	}

	public void CalcCommunityApprovalRating(Community community, out float approval, out float respect)
	{
		if (IsPredicted())
		{
			Authoritative.CalcCommunityApprovalRating(community, out approval, out respect);
			return;
		}
		approval = 0f;
		respect = 0f;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Actor != null && Memories[i].Actor.Community == community)
			{
				float num = ((Memories[i].Actor == community.Leader) ? 1f : 0.5f);
				approval += Memories[i].ApprovalContribution * num;
				respect += Memories[i].RespectContribution * num;
			}
		}
		approval = Mathf.Clamp(approval, -100f, 100f);
		respect = Mathf.Clamp(respect, -100f, 100f);
	}

	public void SortMemories()
	{
		Memories.InsertionSort(MemorySorter);
		ClearOpinionCache();
		OnMemoriesChanged();
	}

	public void ClearOpinionCache()
	{
		CachedOpinion.Clear();
	}

	public void OnMemoriesChanged()
	{
		CachedMoraleDirty = true;
		CachedMysteriousAttackDirty = true;
		if (CharacterMemoryEditor.CharacterToEdit == this)
		{
			CharacterMemoryEditor.Refresh = true;
		}
	}

	public void AddMemory(MemoryPrototype proto, Character actor, BaseObject obj, float quantityFactor, bool forceAdd = false)
	{
		AddMemory(proto, actor, obj, null, quantityFactor, fakeNews: false, null, forceAdd);
	}

	public void AddMemory(MemoryPrototype proto, Character actor, BaseObject obj, BaseObject thirdParty, float quantityFactor, bool forceAdd = false)
	{
		AddMemory(proto, actor, obj, thirdParty, quantityFactor, fakeNews: false, null, forceAdd);
	}

	public void AddMemory(MemoryPrototype proto, Character actor, BaseObject obj, float quantityFactor, bool fakeNews, bool forceAdd = false)
	{
		AddMemory(proto, actor, obj, null, quantityFactor, fakeNews, null, forceAdd);
	}

	public void AddMemory(MemoryPrototype proto, Character actor, BaseObject obj, BaseObject thirdParty, float quantityFactor, bool fakeNews, TileObject propertyThatWasDamaged = null, bool forceAdd = false)
	{
		if (!IsAuthoritative() || !Alive || (Zombie && Infection != InfectionType.Invisible) || (actor != null && actor.Zombie && actor.Infection != InfectionType.Invisible))
		{
			return;
		}
		Memory memory = Memory.Create(proto, actor, obj, thirdParty, quantityFactor, fakeNews);
		memory.CalcApprovalRespectContribution(this);
		if ((proto == MemoryPrototype.KnockedOut || proto == MemoryPrototype.Hurt) && actor != null)
		{
			int num = FindMemory(proto, null, obj);
			if (num != -1 && Memories[num].Time >= memory.Time - TimeSpan.FromSeconds(600.0) && !Memories[num].FakeNews)
			{
				Memory value = Memories[num];
				if (value.QuantityFactor <= 1f)
				{
					Memories.RemoveAt(num);
				}
				else
				{
					float num2 = Math.Max(0f, value.QuantityFactor - 1f) / value.QuantityFactor;
					value.QuantityFactor *= num2;
					value.ApprovalContribution *= num2;
					value.RespectContribution *= num2;
					Memories[num] = value;
				}
			}
		}
		if (memory.Prototype.Replace != null)
		{
			foreach (string item in memory.Prototype.Replace)
			{
				MemoryPrototype memoryPrototype = GameImpl.Instance.FindMemoryPrototypeByUniqueID(item);
				if (memoryPrototype == null || string.IsNullOrEmpty(memoryPrototype.ReciprocalMemoryID))
				{
					continue;
				}
				int num3 = 0;
				while (num3 < Memories.Count)
				{
					if (Memories[num3].Prototype.UniqueID == memoryPrototype.ReciprocalMemoryID && memory.Actor == Memories[num3].Object && memory.Object == Memories[num3].Actor)
					{
						Memories.RemoveAt(num3);
					}
					else
					{
						num3++;
					}
				}
			}
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (!memory.ShouldMerge(Memories[i]))
			{
				continue;
			}
			memory.Merge(this, Memories[i]);
			memory.CalcPriority(this);
			Memories.RemoveAt(i);
			int index = FindPositionToInsertMemoryWithPriority(memory.Priority);
			Memories.Insert(index, memory);
			if (memory.Prototype.Replace != null && memory.Prototype.Replace.Count > 0)
			{
				i++;
				while (i < Memories.Count)
				{
					if (memory.ShouldMerge(Memories[i]))
					{
						Memories.RemoveAt(i);
					}
					else
					{
						i++;
					}
				}
			}
			OnMemoriesChanged();
			ClearOpinionCacheForMemory(ref memory);
			return;
		}
		if (memory.ApprovalContribution == 0f && memory.RespectContribution == 0f && memory.MoraleContribution == 0f && memory.Prototype.RuleSet != MemoryRuleSet.Envy && !forceAdd)
		{
			return;
		}
		memory.CalcPriority(this);
		int index2 = FindPositionToInsertMemoryWithPriority(memory.Priority);
		Memories.Insert(index2, memory);
		OnMemoriesChanged();
		ClearOpinionCacheForMemory(ref memory);
		int num4 = Memories.Count - 1;
		while (num4 >= 0 && Memories.Count > MaxMemories)
		{
			if (Memories[num4].CanBeForgotten(this))
			{
				Memory memory2 = Memories[num4];
				ClearOpinionCacheForMemory(ref memory2);
				Memories.RemoveAt(num4);
			}
			num4--;
		}
		if (memory.Prototype == MemoryPrototype.MurderSuspect || memory.Prototype == MemoryPrototype.AttackSuspect || memory.Prototype == MemoryPrototype.ArsonSuspect || memory.Prototype == MemoryPrototype.Killed || memory.Prototype == MemoryPrototype.KnockedOut)
		{
			if (memory.Actor != null && memory.Object is TileObject && memory.Object != this)
			{
				GetOrCreateTarget((TileObject)memory.Object).SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
			}
		}
		else if (memory.Prototype == MemoryPrototype.DestroyedProperty && memory.Actor != null && memory.Object == Community && memory.Object != this && propertyThatWasDamaged != null)
		{
			GetOrCreateTarget(propertyThatWasDamaged).SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
		}
		if (InfoScreen.Instance.IsShowingBrainScanFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	private int FindPositionToInsertMemoryWithPriority(float priority)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Priority <= priority)
			{
				return i;
			}
		}
		return 0;
	}

	public void DeleteAllMemoriesWithProto(MemoryPrototype proto)
	{
		bool flag = false;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto)
			{
				Memories.RemoveAt(i);
				flag = true;
			}
		}
		if (flag)
		{
			OnMemoriesChanged();
			ClearOpinionCache();
		}
	}

	public void DeleteMemory(MemoryPrototype proto, string uniqueID)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object != null && Memories[i].Object.GetUniqueID() == uniqueID)
			{
				Memories.RemoveAt(i);
				OnMemoriesChanged();
				ClearOpinionCache();
				break;
			}
		}
	}

	public void DeleteMemory(MemoryPrototype proto, Character actor, BaseObject obj)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Object == obj)
			{
				Memories.RemoveAt(i);
				OnMemoriesChanged();
				ClearOpinionCache();
				break;
			}
		}
	}

	public void RemoveDeletedObjFromMemories(BaseObject deletedObj)
	{
		if (CarriedBy == deletedObj)
		{
			CarriedBy.DropAuthoritative();
		}
		if (CarryingObject == deletedObj)
		{
			DropAuthoritative();
		}
		for (int num = Memories.Count - 1; num >= 0; num--)
		{
			if (Memories[num].Actor == deletedObj)
			{
				Memories.RemoveAt(num);
				OnMemoriesChanged();
				ClearOpinionCache();
			}
			else if (Memories[num].Object == deletedObj)
			{
				Memory value = Memories[num];
				value.Object = null;
				Memories[num] = value;
				OnMemoriesChanged();
				ClearOpinionCache();
			}
		}
		for (int num2 = QueuedSpeeches.Count - 1; num2 >= 0; num2--)
		{
			if (QueuedSpeeches[num2].Target == deletedObj || QueuedSpeeches[num2].SpeechObject == deletedObj)
			{
				QueuedSpeeches.RemoveAt(num2);
			}
		}
	}

	public void OnCommunityDeleted(Community deletedCommunity)
	{
		if (InitialCommunity == deletedCommunity)
		{
			InitialCommunity = null;
		}
		if (Community == deletedCommunity)
		{
			if (SquadId != 0)
			{
				Community.RemoveFromSquad(this);
			}
			Community = null;
		}
		RemoveDeletedObjFromMemories(deletedCommunity);
	}

	public bool HasMemoryOfObject(BaseObject obj)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Actor == obj || Memories[i].Object == obj || Memories[i].ThirdParty == obj)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryOfObjectAfter(MemoryPrototype proto, BaseObject obj, TimeSpan time)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object == obj && Memories[i].Time >= time)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryOfHelpingAgainstZombies(Community community, TimeSpan time)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == MemoryPrototype.KilledZombie && Memories[i].Actor != null && Memories[i].Actor.Community == community && Memories[i].Time >= time)
			{
				return true;
			}
			if (Memories[i].Prototype == MemoryPrototype.SavedLife && Memories[i].Actor != null && Memories[i].Actor.Community == community && Memories[i].Time >= time && Memories[i].Object != null && Memories[i].Object.GetCommunity() == Community)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasJealousRomanticMemoryOf(Character rival, Character myLoveInterest)
	{
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype.RuleSet == MemoryRuleSet.Romantic && Memories[i].ApprovalContribution < 0f && Memories[i].Actor == rival && Memories[i].Object == myLoveInterest)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemory(MemoryPrototype proto, Character actor, BaseObject obj)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemory(proto, actor, obj);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Object == obj)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryOfThirdParty(MemoryPrototype proto, BaseObject ob, BaseObject thirdParty)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemoryOfThirdParty(proto, ob, thirdParty);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object == ob && Memories[i].ThirdParty == thirdParty)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryWithRuleset(MemoryRuleSet ruleSet, Character actor, BaseObject obj, TimeSpan after)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemoryWithRuleset(ruleSet, actor, obj, after);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype.RuleSet == ruleSet && Memories[i].Actor == actor && Memories[i].Object == obj && Memories[i].Time >= after)
			{
				return true;
			}
		}
		return false;
	}

	public float GetMemoryQuantity(MemoryPrototype proto, Character actor, BaseObject obj)
	{
		if (IsPredicted())
		{
			return Authoritative.GetMemoryQuantity(proto, actor, obj);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Object == obj)
			{
				return Memories[i].QuantityFactor;
			}
		}
		return 0f;
	}

	public float GetMemoryQuantityCountForCommunity(MemoryPrototype proto, Community actorCommunity, Community objCommunity)
	{
		if (IsPredicted())
		{
			return Authoritative.GetMemoryQuantityCountForCommunity(proto, actorCommunity, objCommunity);
		}
		float num = 0f;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor != null && Memories[i].Actor.Community == actorCommunity && Memories[i].Object != null && Memories[i].Object.GetCommunity() == objCommunity)
			{
				num += Memories[i].QuantityFactor;
			}
		}
		return num;
	}

	public bool HasMemoryAfter(MemoryPrototype proto, Character actor, BaseObject obj, TimeSpan time)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemoryAfter(proto, actor, obj, time);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Object == obj && Memories[i].Time >= time)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryOfAnyoneInCommunityAfter(MemoryPrototype proto, Community community, BaseObject obj, TimeSpan time)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemoryOfAnyoneInCommunityAfter(proto, community, obj, time);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor != null && Memories[i].Actor.Community == community && Memories[i].Object == obj && Memories[i].Time >= time)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasMemoryWhereObjectIsAnyoneInCommunityAfter(MemoryPrototype proto, Character actor, BaseObject obj, TimeSpan time)
	{
		if (IsPredicted())
		{
			return Authoritative.HasMemoryWhereObjectIsAnyoneInCommunityAfter(proto, actor, obj, time);
		}
		Community community = obj.GetCommunity();
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Time >= time)
			{
				if (Memories[i].Object == obj)
				{
					return true;
				}
				if (Memories[i].Object != null && Memories[i].Object.GetCommunity() == community)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int FindMemory(MemoryPrototype proto, Character actor, BaseObject obj)
	{
		if (IsPredicted())
		{
			return Authoritative.FindMemory(proto, actor, obj);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Actor == actor && Memories[i].Object == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindMemoryOfObject(MemoryPrototype proto, BaseObject obj)
	{
		if (IsPredicted())
		{
			return Authoritative.FindMemoryOfObject(proto, obj);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public int FindMemoryOfObjectAfter(MemoryPrototype proto, BaseObject obj, TimeSpan time)
	{
		if (IsPredicted())
		{
			return Authoritative.FindMemoryOfObject(proto, obj);
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object == obj && Memories[i].Time >= time)
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasAnyFakeMemories()
	{
		if (IsPredicted())
		{
			return Authoritative.HasAnyFakeMemories();
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].FakeNews)
			{
				return true;
			}
		}
		return false;
	}

	public TimeSpan GetTimeOfOldestMemory(BaseObject obj)
	{
		if (IsPredicted())
		{
			return Authoritative.GetTimeOfOldestMemory(obj);
		}
		TimeSpan timeSpan = Session.Instance.PlayTime;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Object == obj)
			{
				timeSpan = MathUtil.Min(timeSpan, Memories[i].Time);
			}
		}
		return timeSpan;
	}

	public MysteriousAttackType HasRecentlyBeenAttackedByUnknownAssailant(out float bestQuantity)
	{
		if (!CachedMysteriousAttackDirty)
		{
			bestQuantity = CachedMysteriousAttackQuantity;
			return CachedMysteriousAttackType;
		}
		MysteriousAttackType mysteriousAttackType = MysteriousAttackType.None;
		bestQuantity = 0f;
		TimeSpan timeSpan = Session.Instance.PlayTime - TimeSpan.FromSeconds(AlertGoal.HiddenAttackerAlertTime);
		foreach (Target target in Targets)
		{
			if (target.LastAttackedUsTime != Target.Never && !target.FullyTracked)
			{
				timeSpan = MathUtil.Min(timeSpan, target.LastAttackedUsTime);
			}
		}
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Actor == null && Memories[i].Object == this && (Memories[i].Prototype == MemoryPrototype.KnockedOut || Memories[i].Prototype == MemoryPrototype.Hurt) && Memories[i].Time >= timeSpan && !Memories[i].Actioned)
			{
				MysteriousAttackType mysteriousAttackType2 = ((Memories[i].Prototype != MemoryPrototype.KnockedOut) ? MysteriousAttackType.Hurt : MysteriousAttackType.KnockedOut);
				if (mysteriousAttackType2 > mysteriousAttackType || (mysteriousAttackType2 == mysteriousAttackType && Memories[i].QuantityFactor > bestQuantity))
				{
					bestQuantity = Memories[i].QuantityFactor;
					mysteriousAttackType = mysteriousAttackType2;
				}
			}
		}
		CachedMysteriousAttackType = mysteriousAttackType;
		CachedMysteriousAttackQuantity = bestQuantity;
		CachedMysteriousAttackDirty = false;
		return mysteriousAttackType;
	}

	public void MarkMysteriousAttackMemoriesAsActioned()
	{
		TimeSpan timeSpan = Session.Instance.PlayTime - TimeSpan.FromSeconds(AlertGoal.HiddenAttackerAlertTime);
		foreach (Target target in Targets)
		{
			if (target.LastAttackedUsTime != Target.Never && !target.FullyTracked)
			{
				timeSpan = MathUtil.Min(timeSpan, target.LastAttackedUsTime);
			}
		}
		for (int num = Memories.Count - 1; num >= 0; num--)
		{
			if (Memories[num].Actor == null && Memories[num].Object == this && (Memories[num].Prototype == MemoryPrototype.KnockedOut || Memories[num].Prototype == MemoryPrototype.Hurt) && Memories[num].Time >= timeSpan && !Memories[num].Actioned)
			{
				Memory value = Memories[num];
				value.Actioned = true;
				Memories[num] = value;
				OnMemoriesChanged();
			}
		}
	}

	public bool HasAnyTargetsWhoAttackedUs()
	{
		foreach (Target target in Targets)
		{
			if (target.LastAttackedUsTime != Target.Never)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAttackIntentional(Character source, TileObject intendedTarget)
	{
		if (intendedTarget != this && (intendedTarget == null || intendedTarget.GetCommunity() != Community))
		{
			return IsEnemy(source);
		}
		return true;
	}

	public bool IsAssailantUnknown(Character character)
	{
		if (character == null || character == this)
		{
			return false;
		}
		return !IsFullyTracked(character);
	}

	public bool IsFullyTracked(Character character)
	{
		return GetTarget(character)?.FullyTracked ?? false;
	}

	public bool HasReadBook(EquipmentPrototype proto)
	{
		if (BooksRead == null)
		{
			return false;
		}
		return BooksRead.Contains(proto);
	}

	public void OnReadBook(EquipmentPrototype proto)
	{
		if (BooksRead == null)
		{
			BooksRead = new List<EquipmentPrototype>();
		}
		if (!BooksRead.Contains(proto))
		{
			BooksRead.Add(proto);
			if (IsAuthoritative() && HasReadAllBooksInSet(proto))
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.GiftAllBooksInSet);
			}
		}
	}

	public bool HasReadAllBooksInSet(EquipmentPrototype proto)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			if (proto.IsBookInSameSet(item.Value) && !BooksRead.Contains(item.Value))
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateMemories(bool force)
	{
		TimeSpan playTime = Session.Instance.PlayTime;
		TimeSpan timeSpan = playTime - LastMemoryUpdateTime;
		if (timeSpan < TimeBetweenMemoryUpdates && !force)
		{
			return;
		}
		LastMemoryUpdateTime = playTime;
		if (!AliveAndNotZombie || DontSimulateSurvivalFactorsUntilDiscovered || DontSimulateSurvivalFactorsUntilJoinCommunity)
		{
			return;
		}
		bool flag = Community != null && !Community.IsAmbientCommunity() && !Community.IsTemporaryCommunity() && Community.IsUnhygienic() && Community.UniqueID != TentCity && Community.UniqueID != WhiteHillsGang;
		bool flag2 = Community != null && !Community.IsAmbientCommunity() && !Community.IsTemporaryCommunity() && Community.LacksSpace();
		float num = CalcMorale();
		Character character = ((Community != null) ? Community.Leader : null);
		if (character != null && character.AliveAndNotZombie && character != this)
		{
			float num2 = (float)timeSpan.TotalSeconds / Sun.DayLengthSecs;
			if (GetHunger() >= HungerCriticalTime)
			{
				AddMemory(MemoryPrototype.LeadershipCausedHunger, character, this, num2);
			}
			if (GetThirst() >= ThirstCriticalTime)
			{
				AddMemory(MemoryPrototype.LeadershipCausedThirst, character, this, num2);
			}
			if (GetSleepDeprivation() >= SleepDeprivationCriticalTime)
			{
				AddMemory(MemoryPrototype.LeadershipCausedSleepDeprivation, character, this, num2);
			}
			if (GetBodyTemperatureInCelsius() <= BodyTemperatureInCelsiusModerateHypothermia)
			{
				AddMemory(MemoryPrototype.LeadershipCausedHypothermia, character, this, num2);
			}
			if (num < 0f)
			{
				AddMemory(MemoryPrototype.LeadershipCausedLowMorale, character, this, num2 * (num / -100f));
			}
			if (flag)
			{
				AddMemory(MemoryPrototype.LeadershipCausedLackOfHygiene, character, this, num2);
			}
			if (flag2)
			{
				AddMemory(MemoryPrototype.LeadershipCausedLackOfSpace, character, this, num2);
			}
		}
		float moraleFadeSpeed = Memory.CalcMoraleContribution(this, DefaultMoraleFadeSpeed);
		float step = (float)timeSpan.TotalSeconds;
		float halfLife = Sun.DayLengthSecs * 7f;
		bool changed = false;
		for (int num3 = Memories.Count - 1; num3 >= 0; num3--)
		{
			MemoryPrototype prototype = Memories[num3].Prototype;
			if (Memories[num3].Object == this)
			{
				if (prototype == MemoryPrototype.LeadershipCausedHunger)
				{
					if (GetHunger() < HungryTime)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedThirst)
				{
					if (GetThirst() < ThirstyTime)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedSleepDeprivation)
				{
					if (GetSleepDeprivation() < SleepyTime)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedHypothermia)
				{
					if (GetBodyTemperatureInCelsius() > BodyTemperatureInCelsiusMildHypothermia)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedLowMorale)
				{
					if (num >= 0f)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
					changed = true;
				}
				else if (prototype == MemoryPrototype.LeadershipCausedLackOfHygiene)
				{
					if (!flag)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedLackOfSpace)
				{
					if (!flag2)
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else if (prototype == MemoryPrototype.LeadershipCausedOverwork)
				{
					if (!HasAnyUrgentRoles())
					{
						FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
					}
				}
				else
				{
					FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
				}
			}
			else
			{
				FadeMemory(this, num3, step, moraleFadeSpeed, halfLife, ref changed);
			}
		}
		if (changed)
		{
			SortMemories();
		}
		if (Community != null && Community.CommunityType == CommunityType.Player && !IsPlayerAvatar() && WantLeaveCommunity(Community, checkRelationships: true) && !HasQueuedSpeechOfType(SpeechSituation.LeaveCommunity))
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, Community.Leader, SpeechSituation.LeaveCommunity);
			if (speechForSituation != null)
			{
				PushQueuedSpeech(speechForSituation, Community.Leader, null, default(MemoryParam));
			}
		}
		if (Community == null || Rank != Rank.Leader)
		{
			return;
		}
		Community playerCommunity = Session.Instance.CommunityManager.PlayerCommunity;
		if (!Community.CachedAllies.Contains(playerCommunity) || playerCommunity.Leader == null || HasQueuedSpeechOfType(SpeechSituation.LeaveAlliance))
		{
			return;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		CalcApprovalRating(playerCommunity.Leader, out var approval, out var respect);
		if (approval + respect < -75f)
		{
			Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(this, playerCommunity.Leader, SpeechSituation.LeaveAlliance);
			if (speechForSituation2 != null)
			{
				PushQueuedSpeech(speechForSituation2, playerCommunity.Leader, null, default(MemoryParam));
			}
			return;
		}
		Community.CalcAverageApprovalRating(playerCommunity, out approval, out respect);
		if (approval + respect < -75f)
		{
			Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(this, playerCommunity.Leader, SpeechSituation.LeaveAlliance);
			if (speechForSituation3 != null)
			{
				PushQueuedSpeech(speechForSituation3, playerCommunity.Leader, null, default(MemoryParam));
			}
		}
	}

	public static void FadeMemory(Character character, int i, float step, float moraleFadeSpeed, float halfLife, ref bool changed)
	{
		Memory value = character.Memories[i];
		if (value.Prototype.FadeSpeed != 0f)
		{
			float num = Mathf.Exp((0f - step) * value.Prototype.FadeSpeed * Ln2 / halfLife);
			float num2 = Mathf.Exp((0f - step) * value.Prototype.FadeSpeed * moraleFadeSpeed * Ln2 / halfLife);
			value.ApprovalContribution *= num;
			value.RespectContribution *= num;
			value.MoraleContribution *= num2;
			changed = true;
			if (value.CanBeForgotten(character) && Mathf.Abs(value.RespectContribution) < ForgetThreshold && Mathf.Abs(value.ApprovalContribution) < ForgetThreshold && Mathf.Abs(value.MoraleContribution) < ForgetThreshold)
			{
				character.Memories.RemoveAt(i);
				return;
			}
			value.CalcPriority(character);
			character.Memories[i] = value;
		}
	}

	public void RemoveFailedExitBuildingAttempt(Building building, int exitIndex)
	{
		if (building == null || IsPredicted())
		{
			return;
		}
		for (int num = ExitBuildingAttempts.Count - 1; num >= 0; num--)
		{
			if (ExitBuildingAttempts[num].Matches(building, exitIndex))
			{
				ExitBuildingAttempts.RemoveAt(num);
			}
		}
	}

	public bool HasFailedExitBuildingAttempt(Building building, int exitIndex)
	{
		if (IsPredicted())
		{
			return Authoritative.HasFailedExitBuildingAttempt(building, exitIndex);
		}
		if (building != null)
		{
			for (int i = 0; i < ExitBuildingAttempts.Count; i++)
			{
				if (ExitBuildingAttempts[i].Matches(building, exitIndex))
				{
					TimeSpan timeSpan = Session.Instance.PlayTime - ExitBuildingAttempts[i].FailTime;
					TimeSpan timeSpan2 = TimeSpan.FromSeconds(10.0);
					if (ExitBuildingAttempts[i].FailCount > 2)
					{
						timeSpan2 = TimeSpan.FromSeconds(300.0);
					}
					else if (ExitBuildingAttempts[i].FailCount > 1)
					{
						timeSpan2 = TimeSpan.FromSeconds(60.0);
					}
					return timeSpan <= timeSpan2;
				}
			}
		}
		return false;
	}

	public void AddFailedExitBuildingAttempt(Building building, int exitIndex)
	{
		if (building == null || IsPredicted())
		{
			return;
		}
		ExitBuildingAttempt exitBuildingAttempt = new ExitBuildingAttempt
		{
			BuildingId = building.Id,
			ExitIndex = exitIndex,
			FailTime = Session.Instance.PlayTime,
			FailCount = 1
		};
		for (int i = 0; i < ExitBuildingAttempts.Count; i++)
		{
			if (ExitBuildingAttempts[i].Matches(building, exitIndex))
			{
				exitBuildingAttempt.FailCount += ExitBuildingAttempts[i].FailCount;
				ExitBuildingAttempts[i] = exitBuildingAttempt;
				return;
			}
		}
		while (ExitBuildingAttempts.Count > 16)
		{
			ExitBuildingAttempts.RemoveAt(0);
		}
		ExitBuildingAttempts.Add(exitBuildingAttempt);
	}

	public void ClearFailedFindAttempts()
	{
		FindAttempts.Clear();
	}

	public void RemoveFailedFindAttempt(TileObject obj)
	{
		if (obj == null)
		{
			return;
		}
		for (int num = FindAttempts.Count - 1; num >= 0; num--)
		{
			if (FindAttempts[num].SearchObjectId == obj.Id)
			{
				FindAttempts.RemoveAt(num);
			}
		}
	}

	public bool HasFailedFindAttempt(BaseObject searchObject, FindGoal findGoal, TimeSpan maxTimeSinceLastAttempt, out TimeSpan timeSinceAttempt, out int failCount)
	{
		return HasFailedFindAttempt(searchObject, 0, 0, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe, maxTimeSinceLastAttempt, out timeSinceAttempt, out failCount);
	}

	public bool HasFailedFindAttemptForEntrance(BaseObject searchObject, int entranceIndex, FindGoal findGoal, TimeSpan maxTimeSinceLastAttempt, out TimeSpan timeSinceAttempt, out int failCount)
	{
		return HasFailedFindAttempt(searchObject, entranceIndex, 0, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe, maxTimeSinceLastAttempt, out timeSinceAttempt, out failCount);
	}

	public bool HasFailedFindAttemptForTerrainPath(BaseObject searchObject, int terrainPathIndex, FindGoal findGoal, TimeSpan maxTimeSinceLastAttempt, out TimeSpan timeSinceAttempt, out int failCount)
	{
		return HasFailedFindAttempt(searchObject, 0, terrainPathIndex, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe, maxTimeSinceLastAttempt, out timeSinceAttempt, out failCount);
	}

	public bool HasFailedFindAttempt(BaseObject searchObject, int entranceIndex, int terrainPathIndex, FindType findType, EquipmentPrototype proto, LiquidPrototype liquid, Recipe recipe, TimeSpan maxTimeSinceLastAttempt, out TimeSpan timeSinceAttempt, out int failCount)
	{
		if (searchObject != null)
		{
			for (int i = 0; i < FindAttempts.Count; i++)
			{
				if (!FindAttempts[i].Matches(searchObject, entranceIndex, terrainPathIndex, findType, proto, liquid, recipe))
				{
					continue;
				}
				float h;
				if (searchObject is TileObject targetObj)
				{
					if (MoveAdjacentToTarget.IsAdjacentToTarget(this, targetObj) && !(searchObject is Character { Alive: not false }))
					{
						break;
					}
				}
				else if (terrainPathIndex < GameTerrain.Instance.Paths.Count && GameTerrain.Instance.Paths[terrainPathIndex].GetAmountOnPath(GameTerrain.Instance, PosXZ, out h) < 1f)
				{
					break;
				}
				timeSinceAttempt = Session.Instance.PlayTime - FindAttempts[i].SearchTime;
				failCount = FindAttempts[i].FailCount;
				return timeSinceAttempt <= maxTimeSinceLastAttempt;
			}
		}
		timeSinceAttempt = TimeSpan.Zero;
		failCount = 0;
		return false;
	}

	public void AddFailedFindAttempt(BaseObject searchObject, FindGoal findGoal)
	{
		AddFailedFindAttempt(searchObject, 0, 0, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe);
	}

	public void AddFailedFindAttemptForTerrainPath(BaseObject searchObject, int terrainPathIndex, FindGoal findGoal)
	{
		AddFailedFindAttempt(searchObject, 0, terrainPathIndex, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe);
	}

	public void AddFailedFindAttemptForEntrance(BaseObject searchObject, int entranceIndex, FindGoal findGoal)
	{
		AddFailedFindAttempt(searchObject, Math.Max(0, entranceIndex), 0, findGoal.FindType, findGoal.ProtoToFind, findGoal.Liquid, findGoal.FollowingRecipe);
	}

	public void AddFailedFindAttempt(BaseObject searchObject, int entranceIndex, int terrainPathIndex, FindType findType, EquipmentPrototype proto, LiquidPrototype liquid, Recipe recipe)
	{
		if (searchObject == null)
		{
			return;
		}
		FindAttempt findAttempt = new FindAttempt
		{
			SearchObjectId = searchObject.Id,
			EntranceIndex = entranceIndex,
			FindType = findType,
			ProtoToFind = proto,
			Liquid = liquid,
			FollowingRecipe = recipe,
			TerrainPathIndex = terrainPathIndex,
			SearchTime = Session.Instance.PlayTime,
			FailCount = 1
		};
		for (int i = 0; i < FindAttempts.Count; i++)
		{
			if (FindAttempts[i].Matches(searchObject, entranceIndex, terrainPathIndex, findType, proto, liquid, recipe))
			{
				findAttempt.FailCount += FindAttempts[i].FailCount;
				FindAttempts[i] = findAttempt;
				return;
			}
		}
		while (FindAttempts.Count > 128)
		{
			FindAttempts.RemoveAt(0);
		}
		FindAttempts.Add(findAttempt);
	}

	public void ForgetPropertyDamage()
	{
		if (!ConsciousAndNotZombie)
		{
			WasAttackedBeforeLastCeaseFire = true;
		}
		foreach (Target target in Targets)
		{
			target.LastAttackedMeTime = Target.Never;
			target.LastAttackedUsTime = Target.Never;
			target.LastHeardAttackTime = Target.Never;
			if (target.GetFlag((TargetFlags)80) || target.GetFlag((TargetFlags)48))
			{
				target.SetFlag(TargetFlags.HaveInvestigatedBody, on: true);
				target.SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
			}
		}
		for (int num = QueuedSpeeches.Count - 1; num >= 0; num--)
		{
			if (QueuedSpeeches[num].Speech != null)
			{
				SpeechSituation situation = QueuedSpeeches[num].Speech.Situation;
				if ((uint)(situation - 148) <= 2u || (uint)(situation - 164) <= 2u)
				{
					QueuedSpeeches.RemoveAt(num);
				}
			}
		}
		if (FindActiveGoal(GoalType.Conversation) is Conversation { OpeningSpeech: not null } conversation)
		{
			SpeechSituation situation = conversation.OpeningSpeech.Situation;
			if ((uint)(situation - 148) <= 2u || (uint)(situation - 164) <= 2u)
			{
				conversation.Finished = true;
			}
		}
	}

	public bool CanUseItemForCrafting(TileObject carrier, Equipment item, Equipment usingItem, Recipe recipe)
	{
		if (usingItem == item)
		{
			return true;
		}
		if (carrier is Character character && character.IsWearing(item))
		{
			if (character.Alive)
			{
				return false;
			}
			if (character.Community == Community)
			{
				return false;
			}
			if (character.Community != null && Session.Instance.CommunityManager.GetRelationship(Community, character.Community) != CommunityRelationshipType.Hostile)
			{
				return false;
			}
		}
		if (item.GetPrototype().ContainsHumanMeat && recipe.IsProductDrinkableOrEdible() && ShouldRefuseToEatHumanMeat())
		{
			return false;
		}
		return true;
	}

	public bool ShouldRefuseToEatHumanMeat()
	{
		if (HasPersonality(CachedPersonalityType.Moral))
		{
			return Hunger < HungerExtraCriticalTime;
		}
		return false;
	}

	public bool IsActionAllowedForItem(Equipment item, EquipmentPolicyAction action)
	{
		return IsActionAllowedForItem(item, action, includeCommunityPolicy: true);
	}

	public bool IsActionAllowedForItem(Equipment item, EquipmentPolicyAction action, bool includeCommunityPolicy)
	{
		if (action == EquipmentPolicyAction.CanShare && item.GetLiquidContentsType() != null && !IsActionAllowedForItem(item.GetPrototype(), null, InfectionType.None, action))
		{
			return false;
		}
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(item))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
		}
		if (includeCommunityPolicy)
		{
			if (Community == null)
			{
				return true;
			}
			return Community.IsActionAllowedForItem(item, action);
		}
		return true;
	}

	public bool IsActionAllowedForItem(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, EquipmentPolicyAction action)
	{
		return IsActionAllowedForItem(proto, liquid, infectionType, action, includeCommunityPolicy: true);
	}

	public bool IsActionAllowedForItem(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, EquipmentPolicyAction action, bool includeCommunityPolicy)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
		}
		if (includeCommunityPolicy)
		{
			if (Community == null)
			{
				return true;
			}
			return Community.IsActionAllowedForItem(proto, liquid, infectionType, action);
		}
		return (EquipmentPolicy.GetDefaultActionsMask(proto, liquid) & (1 << (int)action)) != 0;
	}

	public bool IsActionAllowedForItemIgnoringInfection(EquipmentPrototype proto, LiquidPrototype liquid, EquipmentPolicyAction action, bool includeCommunityPolicy)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].MatchesIgnoringInfection(proto, liquid))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
		}
		if (includeCommunityPolicy)
		{
			if (Community == null)
			{
				return true;
			}
			return Community.IsActionAllowedForItemIgnoringInfection(proto, liquid, action);
		}
		return (EquipmentPolicy.GetDefaultActionsMask(proto, liquid) & (1 << (int)action)) != 0;
	}

	public bool IsActionAllowedForType(BaseObjectType baseObjectType, EquipmentPolicyAction action)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Proto != null && EquipmentPolicies[i].Proto.TypeName == baseObjectType && EquipmentPolicies[i].GetPolicy(action))
			{
				return true;
			}
		}
		if (Community == null)
		{
			return false;
		}
		return Community.IsActionAllowedForType(baseObjectType, action);
	}

	public float GetTargetAmountToCarry(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, bool includeCommunityPolicy = true)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				return EquipmentPolicies[i].TargetAmount;
			}
		}
		if (includeCommunityPolicy && Community != null)
		{
			return Community.GetTargetAmountToCarry(proto, liquid, infectionType);
		}
		if (proto == null)
		{
			return 0f;
		}
		return proto.DefaultCarryAmount;
	}

	public float GetTargetAmountToCarryIncludingAmmo(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		List<EquipmentPolicy> allCarryAmountPolicies = GetAllCarryAmountPolicies();
		if (allCarryAmountPolicies != null)
		{
			for (int i = 0; i < allCarryAmountPolicies.Count; i++)
			{
				if (allCarryAmountPolicies[i].Matches(proto, liquid, infectionType))
				{
					return allCarryAmountPolicies[i].TargetAmount;
				}
			}
		}
		return 0f;
	}

	public int GetEquipmentPolicyMask(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, bool includeCommunityPolicy, out bool sameAsCommunity)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				sameAsCommunity = false;
				return EquipmentPolicies[i].GetPolicyMask();
			}
		}
		sameAsCommunity = true;
		if (includeCommunityPolicy && Community != null)
		{
			return Community.GetEquipmentPolicyMask(proto, liquid, infectionType);
		}
		return EquipmentPolicy.GetDefaultActionsMask(proto, liquid);
	}

	public void SetEquipmentPolicy(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, int policyMask, float targetAmount, bool sameAsCommunity)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				EquipmentPolicy value = EquipmentPolicies[i];
				value.SetPolicyMask(policyMask, targetAmount);
				if (sameAsCommunity)
				{
					EquipmentPolicies.RemoveAt(i);
				}
				else
				{
					EquipmentPolicies[i] = value;
				}
				OnEquipmentPolicyChanged();
				return;
			}
		}
		if (!sameAsCommunity)
		{
			EquipmentPolicy item = new EquipmentPolicy(proto, liquid, infectionType);
			item.SetPolicyMask(policyMask, targetAmount);
			EquipmentPolicies.Add(item);
			OnEquipmentPolicyChanged();
		}
	}

	public void ClearEquipmentPolicies()
	{
		EquipmentPolicies.Clear();
	}

	public static bool ContainsEquipmentPolicy(List<EquipmentPolicy> policies, int count, EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		for (int i = 0; i < count; i++)
		{
			if (policies[i].Matches(proto, liquid, infectionType))
			{
				return true;
			}
		}
		return false;
	}

	public List<EquipmentPolicy> GetAllCarryAmountPolicies()
	{
		if (Authoritative != null)
		{
			return Authoritative.CachedCarryAmountPolicies;
		}
		if (CachedCarryAmountPoliciesDirty)
		{
			CachedCarryAmountPoliciesDirty = false;
			if (CachedCarryAmountPolicies == null)
			{
				CachedCarryAmountPolicies = new List<EquipmentPolicy>();
			}
			else
			{
				CachedCarryAmountPolicies.Clear();
			}
			for (int i = 0; i < EquipmentPolicies.Count; i++)
			{
				CachedCarryAmountPolicies.Add(EquipmentPolicies[i]);
			}
			TempDesiredAmmoTypes.Clear();
			bool flag = IsControllableByPlayer();
			if (flag)
			{
				bool flag2 = HasRunningRoleWhichWillDepositGatheredItems();
				foreach (Equipment content in Inventory.Contents)
				{
					if (!(content is AmmoWeapon ammoWeapon) || ammoWeapon.GetAmmoTypes() == null || (flag2 && ammoWeapon.GetGatheredAmount() > 0))
					{
						continue;
					}
					foreach (EquipmentPrototype ammoType in ammoWeapon.GetAmmoTypes())
					{
						TempDesiredAmmoTypes.Add(ammoType);
					}
				}
			}
			if (Community != null)
			{
				int count = CachedCarryAmountPolicies.Count;
				for (int j = 0; j < Community.EquipmentPolicies.Count; j++)
				{
					if (!ContainsEquipmentPolicy(CachedCarryAmountPolicies, count, Community.EquipmentPolicies[j].Proto, Community.EquipmentPolicies[j].Liquid, Community.EquipmentPolicies[j].InfectedWith) && (Community.EquipmentPolicies[j].Proto == null || !Community.EquipmentPolicies[j].Proto.IsAmmo() || TempDesiredAmmoTypes.Contains(Community.EquipmentPolicies[j].Proto)))
					{
						CachedCarryAmountPolicies.Add(Community.EquipmentPolicies[j]);
					}
				}
			}
			if (flag)
			{
				foreach (EquipmentPrototype tempDesiredAmmoType in TempDesiredAmmoTypes)
				{
					if (tempDesiredAmmoType.DefaultCarryAmount > 0 && !ContainsEquipmentPolicy(CachedCarryAmountPolicies, CachedCarryAmountPolicies.Count, tempDesiredAmmoType, null, InfectionType.None))
					{
						EquipmentPolicy item = new EquipmentPolicy(tempDesiredAmmoType, null, InfectionType.None);
						item.TargetAmount = tempDesiredAmmoType.DefaultCarryAmount;
						CachedCarryAmountPolicies.Add(item);
					}
				}
				TempDesiredAmmoTypes.Clear();
			}
			for (int num = CachedCarryAmountPolicies.Count - 1; num >= 0; num--)
			{
				if (CachedCarryAmountPolicies[num].TargetAmount == 0f)
				{
					CachedCarryAmountPolicies.RemoveAt(num);
				}
			}
			CachedCarryAmountPolicies.Sort(CarryPolicySorter);
		}
		return CachedCarryAmountPolicies;
	}

	public void OnEquipmentPolicyChanged()
	{
		CachedCarryAmountPoliciesDirty = true;
		if (Goal != null)
		{
			Goal.OnEquipmentPolicyChanged(this, null);
		}
	}

	public void SetMovementZone(TerrainRect rect, bool paused = false)
	{
		MovementZone = rect;
		MovementZonePaused = paused;
		SetHangoutLocation(HangOutLocation);
		ClearFailedFindAttempts();
	}

	public void PauseMovementZone()
	{
		if (MovementZone != TerrainRect.Invalid)
		{
			MovementZonePaused = true;
			ClearFailedFindAttempts();
		}
	}

	public void ResumeMovementZone()
	{
		MovementZonePaused = false;
		ClearFailedFindAttempts();
	}

	public bool HasMovementZone()
	{
		if (MovementZone != TerrainRect.Invalid)
		{
			return !MovementZonePaused;
		}
		return false;
	}

	public Character GetActorForMostRecentMemoryOfType(MemoryPrototype proto, Character obj)
	{
		TimeSpan timeSpan = TimeSpan.Zero;
		Character result = null;
		for (int i = 0; i < Memories.Count; i++)
		{
			if (Memories[i].Prototype == proto && Memories[i].Object == obj && Memories[i].Time > timeSpan)
			{
				timeSpan = Memories[i].Time;
				result = Memories[i].Actor;
			}
		}
		return result;
	}

	public bool ShouldIgnoreInvisibleStrainCharacter(Character character)
	{
		if ((Infection == InfectionType.Invisible && character.InvisibleStrain != InvisibleStrainType.None) || (character.Infection == InfectionType.Invisible && InvisibleStrain != InvisibleStrainType.None))
		{
			return true;
		}
		return false;
	}

	public bool ShouldIgnoreInvisibleStrainEnemy(TileObject obj)
	{
		if (obj is Character character)
		{
			return ShouldIgnoreInvisibleStrainCharacter(character);
		}
		return false;
	}

	public bool IsEnemy(TileObject obj)
	{
		return IsEnemy(obj, includeJustActivatedInvisibleStrain: false);
	}

	public virtual bool IsEnemy(TileObject obj, bool includeJustActivatedInvisibleStrain = false)
	{
		if (obj is Character character)
		{
			if (!character.Alive)
			{
				return false;
			}
			if (character.Zombie != Zombie)
			{
				if (character.InvisibleStrainJustActivated != TimeSpan.Zero && PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - character.InvisibleStrainJustActivated <= TimeSpan.FromSeconds(2.0) && !includeJustActivatedInvisibleStrain && Session.Instance.CommunityManager.GetRelationship(Community, character.Community) != CommunityRelationshipType.Hostile)
				{
					return false;
				}
				if (ShouldIgnoreInvisibleStrainCharacter(character))
				{
					return false;
				}
				return true;
			}
			if (character.Community != Community && Community != null && character.Community != null)
			{
				return Session.Instance.CommunityManager.GetRelationship(Community, character.Community) == CommunityRelationshipType.Hostile;
			}
			return false;
		}
		if (obj is Building { Inhabitants: var inhabitants })
		{
			foreach (Character character2 in inhabitants)
			{
				if (character2 != null && IsEnemy(character2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAttackableEnemy(TileObject obj)
	{
		if (!(obj is Character) && Community != null)
		{
			Community community = obj.GetCommunity();
			if (Community.GetRelationship(community) == CommunityRelationshipType.Hostile)
			{
				return true;
			}
		}
		return IsEnemy(obj);
	}

	private static bool IsUnarmedAttackType(AttackType attackType)
	{
		if ((uint)(attackType - 6) <= 1u || (uint)(attackType - 12) <= 1u)
		{
			return true;
		}
		return false;
	}

	public override bool IsFriendlyFire(Character source, TileObject target, bool itsATrap, AttackType meleeAttackType)
	{
		if (AliveAndNotZombie && source != null)
		{
			if (SparringPartner == source)
			{
				switch (SparringType)
				{
				case SparringType.Boxing:
					if (IsUnarmedAttackType(meleeAttackType))
					{
						return false;
					}
					break;
				default:
					return false;
				case SparringType.SnowballFight:
					break;
				}
			}
			if (Community != null)
			{
				if (Community == source.Community)
				{
					return true;
				}
				if (Community.CachedAllies.Contains(source.Community))
				{
					return true;
				}
			}
			if (itsATrap)
			{
				return false;
			}
			if (CanFollowPlayer && source.Community == Session.Instance.CommunityManager.PlayerCommunity)
			{
				return true;
			}
			if (source.CanFollowPlayer && Community == Session.Instance.CommunityManager.PlayerCommunity)
			{
				return true;
			}
			if (target != this && GetBaseObjectType() == BaseObjectType.Human && !source.IsEnemy(this))
			{
				bool flag = target is Character character && character.Zombie;
				if ((target == null || Community == null || target.GetCommunity() != Community || flag) && (target == null || target.IsDestroyed() || source.IsEnemy(target)))
				{
					return true;
				}
			}
			if (source.IsTargetSurrendering(this) && source.Community == Session.Instance.CommunityManager.PlayerCommunity)
			{
				return true;
			}
		}
		return false;
	}

	private Bounds GetForgivingBoundingBoxForProjectiles()
	{
		if (GetBaseObjectType() == BaseObjectType.Human)
		{
			return MathUtil.CreateBoundsMinMax(Position - new Vector3(ForgivingRadius, 0f, ForgivingRadius), Position + new Vector3(ForgivingRadius, ForgivingHeight, ForgivingRadius));
		}
		return GetRaycastBoundingBox(bullet: false);
	}

	private Bounds GetRaycastBoundingBox(bool bullet)
	{
		float num = Radius;
		if (bullet)
		{
			num = Math.Min(num, BulletBoundsRadius);
		}
		return MathUtil.CreateBoundsMinMax(Position - new Vector3(num, 0f, num), Position + new Vector3(num, Height, num));
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if (InsideBuilding != null && (flags & 0x200) == 0)
		{
			return null;
		}
		if ((flags & 0x400) != 0 && IsFriendlyFire(source, target, itsATrap: false))
		{
			return null;
		}
		if ((flags & 0x800) != 0 && this != target && !IsAwake)
		{
			return null;
		}
		if ((flags & 1) != 0)
		{
			float distance;
			if ((flags & 0x2000) != 0 && target == this)
			{
				if (GetForgivingBoundingBoxForProjectiles().IntersectRay(ray, out distance) && distance < length)
				{
					return distance;
				}
			}
			else if (GetRaycastBoundingBox((flags & 0x20000) != 0).IntersectRay(ray, out distance) && distance < length)
			{
				return distance;
			}
		}
		if ((flags & 2) != 0 && GetBoundingBox().IntersectRay(ray, out var distance2) && distance2 < length)
		{
			float? num = RaycastAgainstNonDeterministicHitBoxes(ray, ref bone, ref hitPosInBoneSpace);
			if (num.HasValue && num < length)
			{
				return num;
			}
		}
		return null;
	}

	public virtual HitBox[] GetHitBoxes()
	{
		return NoHitBoxes;
	}

	public float? RaycastAgainstNonDeterministicHitBoxes(Ray ray, ref Bone bone, ref Vector3 hitPosInBoneSpace)
	{
		float distance;
		if (IsUnityObjectActive())
		{
			HitBox[] hitBoxes = GetHitBoxes();
			float num = float.MaxValue;
			for (int i = 0; i < hitBoxes.Length; i++)
			{
				Matrix4x4 transform = GetUnityBoneTransform(hitBoxes[i].Bone);
				if (ray.IntersectsOBB(ref transform, hitBoxes[i].Box, out var dist) && dist < num)
				{
					num = dist;
					bone = hitBoxes[i].Bone;
				}
			}
			if (num != float.MaxValue)
			{
				Vector3 position = ray.origin + ray.direction * num;
				Matrix4x4 matrix = GetUnityBoneTransform(bone);
				hitPosInBoneSpace = MathUtil.InverseTransform(position, ref matrix);
				return num;
			}
		}
		else if (GetRaycastBoundingBox(bullet: true).IntersectRay(ray, out distance))
		{
			return distance;
		}
		return null;
	}

	public InjuryLocation PickRandomInjuryLocation(Vector2 attackerPosXZ, TargettableBodyLocation targetBodyLocation)
	{
		int seed = (int)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).Ticks + Id * 6789;
		InjuryLocation result = InjuryLocation.Torso;
		switch (targetBodyLocation)
		{
		case TargettableBodyLocation.Torso:
			result = MathUtil.RandomInt(seed, 6) switch
			{
				0 => InjuryLocation.Head, 
				1 => (Vector2.Dot(attackerPosXZ - PosXZ, MathUtil.ToXZ(World.Right())) >= 0f) ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg, 
				2 => (Vector2.Dot(attackerPosXZ - PosXZ, MathUtil.ToXZ(World.Right())) >= 0f) ? InjuryLocation.RightArm : InjuryLocation.LeftArm, 
				_ => InjuryLocation.Torso, 
			};
			break;
		case TargettableBodyLocation.Head:
			result = MathUtil.RandomInt(seed, 6) switch
			{
				0 => InjuryLocation.Torso, 
				1 => (Vector2.Dot(attackerPosXZ - PosXZ, MathUtil.ToXZ(World.Right())) >= 0f) ? InjuryLocation.RightArm : InjuryLocation.LeftArm, 
				_ => InjuryLocation.Head, 
			};
			break;
		case TargettableBodyLocation.Legs:
			result = MathUtil.RandomInt(seed, 6) switch
			{
				0 => InjuryLocation.Torso, 
				1 => (Vector2.Dot(attackerPosXZ - PosXZ, MathUtil.ToXZ(World.Right())) >= 0f) ? InjuryLocation.RightArm : InjuryLocation.LeftArm, 
				_ => (Vector2.Dot(attackerPosXZ - PosXZ, MathUtil.ToXZ(World.Right())) >= 0f) ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg, 
			};
			break;
		}
		return result;
	}

	public void PickRandomHitPos(InjuryLocation injuryLocation, int seed, Vector3 attackPos, out Bone bone, out Vector3 hitPosInBoneSpace)
	{
		int num = 0;
		HitBox[] hitBoxes = GetHitBoxes();
		for (int i = 0; i < hitBoxes.Length; i++)
		{
			if (hitBoxes[i].InjuryLocation == injuryLocation)
			{
				num++;
			}
		}
		int num2 = ((GetBaseObjectType() == BaseObjectType.Human) ? 1 : 0);
		int num3 = MathUtil.RandomInt(seed, num);
		for (int j = 0; j < hitBoxes.Length; j++)
		{
			if (hitBoxes[j].InjuryLocation == injuryLocation)
			{
				if (num3 == 0)
				{
					num2 = j;
					break;
				}
				num3--;
			}
		}
		Vector3 vector;
		Vector3 vector2;
		if (num2 < hitBoxes.Length)
		{
			bone = hitBoxes[num2].Bone;
			vector = hitBoxes[num2].Box.min / GetArrowScale();
			vector2 = hitBoxes[num2].Box.max / GetArrowScale();
		}
		else
		{
			bone = Bone.Spine1;
			vector = Vector3.zero;
			vector2 = Vector3.zero;
		}
		hitPosInBoneSpace = new Vector3(Mathf.Lerp(vector.x, vector2.x, MathUtil.RandomFloat(seed + 1)), Mathf.Lerp(vector.y, vector2.y, MathUtil.RandomFloat(seed + 2)), Mathf.Lerp(vector.z, vector2.z, MathUtil.RandomFloat(seed + 3)));
		if (!IsUnityObjectActive())
		{
			return;
		}
		Matrix4x4 matrix = GetUnityBoneTransform(bone);
		Vector3 vector3 = MathUtil.InverseTransform(attackPos, ref matrix);
		Bounds bounds = MathUtil.CreateBoundsMinMax(vector, vector2);
		if (!bounds.Contains(vector3))
		{
			Vector3 vector4 = MathUtil.SafeNormalize(hitPosInBoneSpace - vector3, Vector3.forward);
			Vector3 origin = vector3 - vector4 * (vector2 - vector).magnitude * 2f;
			Ray ray = new Ray(origin, vector4);
			if (bounds.IntersectRay(ray, out var distance))
			{
				hitPosInBoneSpace = ray.GetPoint(distance);
			}
		}
	}

	public void PickRandomNearbyHitPos(float seed, Vector3 attackPos, Bone bone, ref Vector3 hitPosInBoneSpace, float spread)
	{
		if (!IsUnityObjectActive())
		{
			return;
		}
		Matrix4x4 matrix = GetUnityBoneTransform(bone);
		Vector3 vector = MathUtil.InverseTransform(attackPos, ref matrix);
		float magnitude = (hitPosInBoneSpace - vector).magnitude;
		float num = Mathf.Tan(spread) * magnitude;
		hitPosInBoneSpace += MathUtil.RandomVec3(new Vector3(seed + 1f, seed + 2f, seed + 3f)) * num;
		int num2 = 0;
		HitBox[] hitBoxes = GetHitBoxes();
		for (int i = 0; i < hitBoxes.Length; i++)
		{
			if (hitBoxes[i].Bone == bone)
			{
				num2 = i;
				break;
			}
		}
		Vector3 vector2;
		Vector3 vector3;
		if (num2 < hitBoxes.Length)
		{
			vector2 = hitBoxes[num2].Box.min / GetArrowScale();
			vector3 = hitBoxes[num2].Box.max / GetArrowScale();
		}
		else
		{
			vector2 = Vector3.zero;
			vector3 = Vector3.zero;
		}
		hitPosInBoneSpace = MathUtil.Clamp(hitPosInBoneSpace, vector2, vector3);
		Bounds bounds = MathUtil.CreateBoundsMinMax(vector2, vector3);
		if (!bounds.Contains(vector))
		{
			Vector3 vector4 = MathUtil.SafeNormalize(hitPosInBoneSpace - vector, Vector3.forward);
			Vector3 origin = vector - vector4 * (vector3 - vector2).magnitude * 2f;
			Ray ray = new Ray(origin, vector4);
			if (bounds.IntersectRay(ray, out var distance))
			{
				hitPosInBoneSpace = ray.GetPoint(distance);
			}
		}
	}

	public static void SetDebugDrawBoundingBoxes(bool on)
	{
		DrawBoundingBoxes = on;
	}

	public static bool GetDebugDrawBoundingBoxes()
	{
		return DrawBoundingBoxes;
	}

	public static void SetDebugDrawHitBoxes(bool on)
	{
		DrawHitBoxes = on;
	}

	public static bool GetDebugDrawHitBoxes()
	{
		return DrawHitBoxes;
	}

	public static void SetDebugDrawInjuries(bool on)
	{
		DrawInjuries = on;
	}

	public static bool GetDebugDrawInjuries()
	{
		return DrawInjuries;
	}

	public static void SetDebugDrawBones(bool on)
	{
		DrawBones = on;
	}

	public static bool GetDebugDrawBones()
	{
		return DrawBones;
	}

	public void DebugDrawBoundingBoxes()
	{
		Bounds forgivingBoundingBoxForProjectiles = GetForgivingBoundingBoxForProjectiles();
		Bounds raycastBoundingBox = GetRaycastBoundingBox(bullet: true);
		Bounds raycastBoundingBox2 = GetRaycastBoundingBox(bullet: false);
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		DebugGraphics.DrawBox(forgivingBoundingBoxForProjectiles.center, forgivingBoundingBoxForProjectiles.extents, Color.blue);
		DebugGraphics.DrawBox(raycastBoundingBox.center, raycastBoundingBox.extents, Color.blue);
		DebugGraphics.DrawBox(raycastBoundingBox2.center, raycastBoundingBox2.extents, Color.red);
		DebugGraphics.EndDrawLines();
	}

	public void DebugDrawHitBoxes()
	{
		DebugGraphics.StartDrawLines(World);
		DebugGraphics.DrawAxes(Matrix4x4.identity, 1f);
		DebugGraphics.EndDrawLines();
		HitBox[] hitBoxes = GetHitBoxes();
		for (int i = 0; i < hitBoxes.Length; i++)
		{
			HitBox hitBox = hitBoxes[i];
			DebugGraphics.StartDrawLines(GetUnityBoneTransform(hitBox.Bone));
			DebugGraphics.DrawBox(hitBox.Box.center, hitBox.Box.extents, Color.red);
			DebugGraphics.DrawAxes(Matrix4x4.identity, 0.1f);
			DebugGraphics.EndDrawLines();
		}
	}

	public void DebugDrawInjuries()
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			DebugGraphics.StartDrawLines(GetUnityBoneTransform(Injuries[i].Bone));
			DebugGraphics.DrawBox(Injuries[i].PosInBoneSpace, Vector3.one * 0.01f, Color.red);
			DebugGraphics.EndDrawLines();
		}
	}

	private void DebugDrawBone(GameObject bone)
	{
		for (int i = 0; i < bone.transform.childCount; i++)
		{
			GameObject gameObject = bone.transform.GetChild(i).gameObject;
			DebugGraphics.DrawLine(bone.transform.position, gameObject.transform.position, Color.red);
			DebugDrawBone(gameObject);
		}
	}

	public void DebugDrawBones()
	{
		DebugGraphics.StartDrawLines(Matrix4x4.identity);
		DebugDrawBone(Unity.Obj);
		DebugGraphics.EndDrawLines();
	}

	public override void OnPostRender()
	{
		if (DrawLastKnownTargetPositions && IsUnityObjectActive() && Alive && !DirectControlled)
		{
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			foreach (Target target in Targets)
			{
				Color col = ((target.Object.GetCommunity() == Community) ? Color.green : (IsEnemy(target.Object) ? Color.red : Color.yellow));
				if (!target.FullyTracked)
				{
					col *= 0.25f;
				}
				col.a = 1f;
				DebugGraphics.DrawLine(EyePosition, target.LastKnownPosition, col);
			}
			DebugGraphics.EndDrawLines();
		}
		if (DrawBoundingBoxes && IsUnityObjectActive())
		{
			DebugDrawBoundingBoxes();
		}
		if (DrawHitBoxes && IsUnityObjectActive())
		{
			DebugDrawHitBoxes();
		}
		if (DrawInjuries && IsUnityObjectActive())
		{
			DebugDrawInjuries();
		}
		if (DrawBones && IsUnityObjectActive())
		{
			DebugDrawBones();
		}
	}

	public Role GetRole()
	{
		return GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: true).Role;
	}

	public bool HasRole(Role role)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRunningRole(Role role)
	{
		return GetFirstRunningRoleIndex(role) != -1;
	}

	public bool HasRole(RoleInfo roleInfo)
	{
		return GetRoleIndex(roleInfo) != -1;
	}

	public bool HaveAllRunningRolesFailedRecently()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (!Roles[i].Paused && !Roles[i].FailedRecently)
			{
				return false;
			}
		}
		return true;
	}

	public void SetRoleFailedRecently(RoleInfo roleInfo)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Equals(roleInfo))
			{
				roleInfo = Roles[i];
				roleInfo.FailedRecently = true;
				roleInfo.LastFailedTime = Session.Instance.PlayTime;
				Roles[i] = roleInfo;
				if (i == MostRecentRoleIndex)
				{
					MostRecentRoleIndex = -1;
				}
				break;
			}
		}
		if (HaveAllRunningRolesFailedRecently())
		{
			ClearFailedRecentlyOnAllRoles();
		}
	}

	public void SetRoleInProgress(RoleInfo roleInfo, bool inProgress)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex != -1)
		{
			SetRoleInProgress(roleIndex, inProgress);
		}
	}

	public void SetRoleInProgress(int roleIndex, bool inProgress)
	{
		MostRecentRoleIndex = (inProgress ? roleIndex : (-1));
	}

	public bool HasAnyUrgentRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Urgent && !Roles[i].Paused)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyUnpausedRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (!Roles[i].Paused)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyPausedRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Paused)
			{
				return true;
			}
		}
		return false;
	}

	public void OnRoleSucceeded()
	{
		ClearFailedRecentlyOnAllRoles();
	}

	public void ClearFailedRecentlyOnAllRoles()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].FailedRecently)
			{
				RoleInfo value = Roles[i];
				value.FailedRecently = false;
				Roles[i] = value;
			}
		}
	}

	public bool HasRunningRoleWhichWillDepositGatheredItems()
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			switch (Roles[i].Role)
			{
			case Role.Farmer:
			case Role.Gatherer:
			case Role.Crafter:
			case Role.Lumberjack:
			case Role.Miner:
			case Role.Trapper:
			case Role.Organizer:
				if (!Roles[i].Paused && !Roles[i].FailedRecently && !Roles[i].IsInFailedCooldown())
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	public bool HasCraftingRoleWithIngredient(Equipment item)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == Role.Crafter && Roles[i].Recipe != null && Roles[i].Recipe.IsIngredient(item))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRoleWithTargetLocation(Role role, TerrainCoord tile)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role && Roles[i].TargetLocation == tile)
			{
				return true;
			}
		}
		return false;
	}

	public TerrainCoord GetRoleTargetLocation(Role role)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role)
			{
				return Roles[i].TargetLocation;
			}
		}
		return TerrainCoord.Invalid;
	}

	public void SetRoleTargetLocation(Role role, TerrainCoord targetLocation)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role)
			{
				RoleInfo value = Roles[i];
				value.TargetLocation = targetLocation;
				Roles[i] = value;
			}
		}
	}

	public int GetRoleIndex(RoleInfo roleInfo)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Equals(roleInfo))
			{
				return i;
			}
		}
		return -1;
	}

	public int GetFirstRoleIndex(Role role)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetFirstRunningRoleIndex(Role role)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role && !Roles[i].Paused && !Roles[i].FailedRecently && !Roles[i].IsInFailedCooldown())
			{
				return i;
			}
		}
		return -1;
	}

	public RoleInfo GetTopRunningRoleInfo(bool canShowPausedIfNoneAreUnpaused)
	{
		if (MostRecentRoleIndex != -1)
		{
			return Roles[MostRecentRoleIndex];
		}
		for (int i = 0; i < Roles.Count; i++)
		{
			if (!Roles[i].Paused && !Roles[i].FailedRecently && !Roles[i].IsInFailedCooldown())
			{
				return Roles[i];
			}
		}
		if (!(Roles.Count > 0 && canShowPausedIfNoneAreUnpaused))
		{
			return default(RoleInfo);
		}
		return Roles[0];
	}

	public RoleInfo GetRoleInfo(Role role)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Role == role)
			{
				return Roles[i];
			}
		}
		return default(RoleInfo);
	}

	public RoleInfo GetRoleInfo(RoleInfo roleInfo)
	{
		for (int i = 0; i < Roles.Count; i++)
		{
			if (Roles[i].Equals(roleInfo))
			{
				return Roles[i];
			}
		}
		return default(RoleInfo);
	}

	private bool CanAddRole(RoleInfo roleInfo)
	{
		if (GetRoleIndex(roleInfo) != -1)
		{
			return true;
		}
		return Roles.Count < 10;
	}

	public void AddRole(RoleInfo roleInfo)
	{
		AddRole(roleInfo, fromBuildGoal: false);
	}

	public void AddRole(RoleInfo roleInfo, bool fromBuildGoal)
	{
		roleInfo.Paused = DownTime > 0f;
		if (DownTime > 0f && IsControllableByPlayer())
		{
			string str = GameImpl.Translate("HINT_TakingABreak").Replace("%1", GetDisplayNameString());
			str = StringUtil.ApplyFormulae(str, null, this);
			HudBehaviour.Instance.SetStatusBarMsg(str);
		}
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex != -1)
		{
			roleInfo.Urgent = Roles[roleIndex].Urgent;
			Roles[roleIndex] = roleInfo;
			return;
		}
		Roles.Insert(0, roleInfo);
		if (MostRecentRoleIndex >= 0)
		{
			MostRecentRoleIndex++;
		}
		GuardDuty = false;
		if (InfoScreen.Instance.IsShowingInventoryFor(this))
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
		if (IsControllableByPlayer() && GameImpl.Instance.Settings.HintsEnabled && !HudBehaviour.Instance.IsShowingStatusBarMsg())
		{
			sb.Length = 0;
			RoleDisplayBehaviour.GetRoleDisplayText(this, roleInfo, RoleDisplayTextMode.Medium, sb);
			string str2 = GameImpl.Translate("HINT_NewRole").Replace("%1", GetDisplayNameString()).Replace("%2", sb.ToString());
			str2 = StringUtil.ApplyFormulae(str2, null, this);
			HudBehaviour.Instance.SetStatusBarMsg(str2);
		}
	}

	public void RemoveRole(int i)
	{
		if (MostRecentRoleIndex == i)
		{
			MostRecentRoleIndex = -1;
		}
		else if (MostRecentRoleIndex > i)
		{
			MostRecentRoleIndex--;
		}
		Roles.RemoveAt(i);
	}

	public void CancelRole(RoleInfo roleInfo)
	{
		CancelRole(roleInfo, fromBuildGoal: false);
	}

	public void CancelRole(RoleInfo roleInfo, bool fromBuildGoal)
	{
		int roleIndex = GetRoleIndex(roleInfo);
		if (roleIndex != -1)
		{
			RemoveRole(roleIndex);
			if (IsControllableByPlayer())
			{
				SetHangoutLocation(Tile);
			}
			if (HaveAllRunningRolesFailedRecently())
			{
				ClearFailedRecentlyOnAllRoles();
			}
			if (InfoScreen.Instance.IsShowingInventoryFor(this))
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
			if (roleInfo.Role == Role.Builder && Goal is SurvivorGoal { BuildGoal: { Active: not false } } survivorGoal && !fromBuildGoal)
			{
				survivorGoal.SetSubGoal(this, null, survivorGoal.FindSubGoalByType(GoalType.BoredGoal));
			}
		}
	}

	public void CancelAllRoles()
	{
		while (Roles.Count > 0)
		{
			CancelRole(Roles[0]);
		}
		DownTime = 0f;
	}

	public bool HasCurrentRoleOutsideBase()
	{
		if (MostRecentRoleIndex != -1 && Community != null)
		{
			Role role = Roles[MostRecentRoleIndex].Role;
			if (role == Role.Gatherer || role == Role.Lumberjack || role == Role.Miner)
			{
				TerrainCoord targetLocation = Roles[MostRecentRoleIndex].TargetLocation;
				if (GameTerrain.Instance.GetOwnerCommunityIdForTile(targetLocation.x, targetLocation.y) != Community.Id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool ShouldRoleTakePriorityOverNeeds()
	{
		for (int i = 0; i <= MostRecentRoleIndex; i++)
		{
			if (Roles[i].Paused || Roles[i].FailedRecently || Roles[i].IsInFailedCooldown())
			{
				continue;
			}
			if (Roles[i].Urgent)
			{
				return true;
			}
			switch (Roles[i].Role)
			{
			case Role.Gatherer:
				if (Roles[i].ResourceType != null)
				{
					GatheredItem gatheredItem2 = GatheredItem.Create(Roles[i].ResourceType, null, InfectionType.None, 1);
					Prop buildingToStoreSuppliesIn2 = GatherGoal.GetBuildingToStoreSuppliesIn(this, gatheredItem2);
					if (buildingToStoreSuppliesIn2 != null && Tile.GetDistSquared(buildingToStoreSuppliesIn2.Tile) > GathererCloseToHomeDist * GathererCloseToHomeDist)
					{
						return true;
					}
				}
				foreach (Equipment content in Inventory.Contents)
				{
					if (content.GetGatheredAmount() > 0 && GatherGoal.GetBuildingToStoreSuppliesIn(this, GatheredItem.Create(content)) != null)
					{
						return true;
					}
				}
				break;
			case Role.Builder:
			{
				BuildGoal buildGoal = GetBuildGoal();
				if (buildGoal != null && buildGoal.CurrentBuilding != null && (Tile.GetDistSquared(buildGoal.CurrentBuilding.GetTile()) > GathererCloseToHomeDist * GathererCloseToHomeDist || buildGoal.DoesBuilderHaveNeededResources(this)))
				{
					return true;
				}
				break;
			}
			case Role.Repairing:
			{
				RepairGoal repairGoal = GetRepairGoal();
				if (repairGoal != null && repairGoal.CurrentBuildingToRepair != null && (Tile.GetDistSquared(repairGoal.CurrentBuildingToRepair.GetTile()) > GathererCloseToHomeDist * GathererCloseToHomeDist || Inventory.FindItemOfType(repairGoal.CurrentBuildingToRepair.GetRepairResourceType()) != null))
				{
					return true;
				}
				break;
			}
			case Role.Capturing:
			{
				if (Tile.GetDistSquared(Roles[i].TargetLocation) > GathererCloseToHomeDist * GathererCloseToHomeDist)
				{
					return true;
				}
				TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(Roles[i].TargetLocation.x, Roles[i].TargetLocation.y);
				if (fixedObjectOnTile != null && Inventory.FindItemOfType(fixedObjectOnTile.GetCaptureResourceType()) != null)
				{
					return true;
				}
				break;
			}
			case Role.Crafter:
				if (Roles[i].Recipe != null && Tile.GetDistSquared(Roles[i].TargetLocation) > GathererCloseToHomeDist * GathererCloseToHomeDist)
				{
					return true;
				}
				break;
			case Role.Miner:
				if (Roles[i].ResourceType != null)
				{
					GatheredItem gatheredItem3 = GatheredItem.Create(Roles[i].ResourceType, null, InfectionType.None, 1);
					Prop buildingToStoreSuppliesIn3 = GatherGoal.GetBuildingToStoreSuppliesIn(this, gatheredItem3);
					if (buildingToStoreSuppliesIn3 != null && (Tile.GetDistSquared(buildingToStoreSuppliesIn3.Tile) > GathererCloseToHomeDist * GathererCloseToHomeDist || Inventory.FindItemOfType(Roles[i].ResourceType) != null))
					{
						return true;
					}
				}
				break;
			case Role.Lumberjack:
				if (EquipmentPrototype.Wood != null)
				{
					GatheredItem gatheredItem = GatheredItem.Create(EquipmentPrototype.Wood, null, InfectionType.None, 1);
					Prop buildingToStoreSuppliesIn = GatherGoal.GetBuildingToStoreSuppliesIn(this, gatheredItem);
					if (buildingToStoreSuppliesIn != null && (Tile.GetDistSquared(buildingToStoreSuppliesIn.Tile) > GathererCloseToHomeDist * GathererCloseToHomeDist || Inventory.FindItemOfType(EquipmentPrototype.Wood) != null))
					{
						return true;
					}
				}
				break;
			}
		}
		return false;
	}

	public void OnThingIWasBuildingGotDeleted(TileObject obj)
	{
		if (Goal is SurvivorGoal { BuildGoal: { } buildGoal } survivorGoal)
		{
			buildGoal.OnBuildingGotDeleted(this, survivorGoal, obj);
		}
	}

	public Rank GetRank()
	{
		return Rank;
	}

	public void SetRank(Rank rank)
	{
		if (Rank == rank)
		{
			return;
		}
		if (Community == null)
		{
			Debug.LogWarning("Trying to set rank on a character with no community!");
			return;
		}
		Rank = rank;
		if (Rank == Rank.Leader)
		{
			foreach (Character member in Community.Members)
			{
				if (member != this && member.Rank == Rank.Leader)
				{
					member.SetRank(Rank.None);
				}
			}
		}
		Community.UpdateLeader();
	}

	public override bool IsPlayerAvatar()
	{
		if (Community != null && Community.CommunityType == CommunityType.Player && Rank == Rank.Leader)
		{
			return true;
		}
		return AvatarForPlayer.IsValid();
	}

	public Character GetGroupLeader()
	{
		Character character = this;
		if (character.SquadLeader != null)
		{
			character = character.SquadLeader;
		}
		return character;
	}

	public bool IsInAnySquad()
	{
		if (SquadLeader == null)
		{
			if (Followers != null)
			{
				return Followers.Count > 0;
			}
			return false;
		}
		return true;
	}

	public bool IsInSameSquad(Character other)
	{
		if (this != other && (Followers == null || !Followers.Contains(other)) && (SquadLeader == null || SquadLeader != other))
		{
			if (SquadLeader != null && SquadLeader.Followers != null)
			{
				return SquadLeader.Followers.Contains(other);
			}
			return false;
		}
		return true;
	}

	public bool IsInSameCommunity(TileObject other)
	{
		if (Community != null)
		{
			return Community.Id == other.GetCommunityId();
		}
		return false;
	}

	public bool HaveIOrAnyoneInMySquadBeenOrderedToAttackSomeone()
	{
		if (OrderedToAttack)
		{
			return true;
		}
		if (SquadLeader != null)
		{
			if (SquadLeader.OrderedToAttack)
			{
				return true;
			}
			if (SquadLeader.Followers != null)
			{
				foreach (Character follower in SquadLeader.Followers)
				{
					if (follower != this && follower.OrderedToAttack)
					{
						return true;
					}
				}
			}
		}
		if (Followers != null)
		{
			foreach (Character follower2 in Followers)
			{
				if (follower2 != this && follower2.OrderedToAttack)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsInDamageReactionAnim()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 26) <= 7u)
		{
			return true;
		}
		return false;
	}

	public bool OnChokeHoldStarted(Character attacker, HoldType holdType)
	{
		if (InteractionObject != null)
		{
			if (!IsCraftingAnim())
			{
				return false;
			}
			StopActionAnim(CurrentActionAnim);
		}
		if (this is Human)
		{
			if (!IsAwake)
			{
				return true;
			}
			Vector3 forward = base.Forward;
			Vector3 vector = attacker.Position - Position;
			forward.y = 0f;
			vector.y = 0f;
			ActionAnim anim = ((holdType == HoldType.Restrain && !Zombie) ? ActionAnim.RestrainedStruggle : ActionAnim.ChokeHoldStruggle);
			if (TryStartActionAnim(anim, attacker))
			{
				DirectControlledCrouching = false;
				return true;
			}
		}
		return false;
	}

	public void OnCancelChokeHold()
	{
		if (CurrentActionAnim == ActionAnim.ChokeHoldStart || CurrentActionAnim == ActionAnim.ChokeHoldLoop)
		{
			DirectControlledCrouching = false;
			TryStartActionAnim(ActionAnim.ChokeHoldFail, InteractionObject);
		}
		else if (CurrentActionAnim == ActionAnim.SlitThroatStart || CurrentActionAnim == ActionAnim.SlitThroatLoop)
		{
			DirectControlledCrouching = false;
			TryStartActionAnim(ActionAnim.SlitThroatFail, InteractionObject);
		}
		else if (CurrentActionAnim == ActionAnim.RestrainStart || CurrentActionAnim == ActionAnim.RestrainLoop)
		{
			DirectControlledCrouching = false;
			TryStartActionAnim(ActionAnim.RestrainFail, InteractionObject);
		}
	}

	public void OnChokeHoldCancelled(Character attacker)
	{
		if (InteractionObject != attacker || !IsAwake || (CurrentActionAnim != ActionAnim.ChokeHoldStruggle && CurrentActionAnim != ActionAnim.RestrainedStruggle))
		{
			return;
		}
		HoldType holdTypeFromAnim = attacker.GetHoldTypeFromAnim();
		if (holdTypeFromAnim == HoldType.Restrain)
		{
			if (IsAuthoritative() && SparringPartner != null && SparringType == SparringType.Feuding)
			{
				if (attacker.Speaking != null && attacker.Speaking.Situation == SpeechSituation.Restrain)
				{
					attacker.OnSpeechFinished(interrupted: true);
				}
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, attacker, SparringPartner, SpeechSituation.BreakFreeOfRestraint);
				if (speechForSituation != null)
				{
					Speak(speechForSituation, attacker, SparringPartner);
				}
				Memory.OnMemorableEvent(MemoryPrototype.RestrainedUnsuccessfully, attacker, this, SparringPartner, 1f, SecrecyMode.Public, null);
			}
		}
		else
		{
			GetOrCreateTarget(attacker).OnGrabbedMe(this);
			if (Zombie && CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				PlayVoiceSoundFromList(SoundManager.ZombiePainSounds, VoiceSoundType.ZombieSnarl);
			}
			if (IsAuthoritative() && (!(BloodLoss >= 0.95f) || !WillActivateInvisibleStrain()))
			{
				Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Pain, Position, GetShoutVoiceRadius(), GetMaxSoundVisibilityRange(), this, attacker, this, this));
			}
			if (!Zombie && this is Human && IsAuthoritative())
			{
				Memory.OnMemorableEvent(MemoryPrototype.FailedAttack, attacker, this, 1f, secret: false);
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(this, attacker, SpeechSituation.Pain);
				if (speechForSituation2 != null && Speak(speechForSituation2, attacker))
				{
					SkipSpeech(onlySkipLipsMoving: true);
				}
			}
		}
		TryStartActionAnim((holdTypeFromAnim == HoldType.Restrain && !Zombie) ? ActionAnim.RestrainedFree : ActionAnim.ChokeHoldFree, attacker);
	}

	public void OnZombieBite(Character zombie)
	{
		zombie.InvisibleStrainJustActivated = TimeSpan.Zero;
		GetTarget(zombie)?.ClearInaccessible();
		if (GetCurrentActionPriority() != ActionPriority.StruggleFree)
		{
			Vector3 nonDeterministicHitPos = (IsUnityObjectActive() ? GetUnityBoneTransform(ZombieBiteBone).MultiplyPoint(ZombieBitePosInBoneSpace) : GetBoundingBoxCentre());
			float damage = ZombieBiteDamage;
			OnDamaged(zombie, this, InjuryType.ZombieBite, InjuryLocation.Torso, zombie.Infection, null, SkillType.Strength, ref damage, 0f, nonDeterministicHitPos, Vector3.zero, ZombieBiteBone, ZombieBitePosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
			if (zombie.Infection == InfectionType.White && IsControllableByPlayer())
			{
				HintManager.Instance.Hints[17].MarkPerformed();
			}
		}
	}

	public void OnZombieBiteLoopStart(Character zombie)
	{
		if (IsAwake && !DirectControlledMajorAIDisabled && zombie.Zombie)
		{
			TryEscapeZombieBite(float.MaxValue);
		}
	}

	public bool CanEscapeZombieBite(float power)
	{
		if (God)
		{
			return true;
		}
		if (!(InteractionObject is Character character))
		{
			return false;
		}
		power = Math.Max(0f, Math.Min(power, GetStruggleFreePenalty(character.Infection) - ZombieEscapePower));
		return !IsTooTiredFor(power);
	}

	public bool TryEscapeZombieBite(float power)
	{
		if (GetCurrentActionPriority() == ActionPriority.Struggle)
		{
			if (!(InteractionObject is Character character))
			{
				return false;
			}
			power = Math.Max(0f, Math.Min(power, GetStruggleFreePenalty(character.Infection) - ZombieEscapePower));
			if (!IsTooTiredFor(power))
			{
				ZombieEscapePower += power;
				ApplyFatiguePenalty(power);
				if (ZombieEscapePower >= GetStruggleFreePenalty(character.Infection) - 0.001f)
				{
					ActionAnim anim = ((CurrentActionAnim == ActionAnim.BittenPinnedStruggle) ? ActionAnim.BittenPinnedFree : ((CurrentActionAnim == ActionAnim.BittenFrontStruggle) ? ActionAnim.BittenFrontFree : ActionAnim.BittenRearFree));
					return TryStartActionAnim(anim, character);
				}
			}
		}
		return false;
	}

	public bool OnZombieBiteStarted(Character zombie, bool fromJumping)
	{
		if (InteractionObject != null && InteractionObject != zombie)
		{
			return false;
		}
		if (CurrentActionAnim == ActionAnim.Vault)
		{
			return false;
		}
		if (this is Human)
		{
			if (!IsAwake)
			{
				return true;
			}
			Vector3 forward = base.Forward;
			Vector3 rhs = zombie.Position - Position;
			forward.y = 0f;
			rhs.y = 0f;
			bool flag = Vector3.Dot(forward, rhs) >= 0f;
			ActionAnim anim = (fromJumping ? ActionAnim.BittenPinnedStruggle : (flag ? ActionAnim.BittenFrontStruggle : ActionAnim.BittenRearStruggle));
			if (TryStartActionAnim(anim, zombie))
			{
				DirectControlledCrouching = false;
				ZombieEscapePower = 0f;
				return true;
			}
			return false;
		}
		Vector3 nonDeterministicHitPos = (IsUnityObjectActive() ? GetUnityBoneTransform(ZombieBiteBone).MultiplyPoint(ZombieBitePosInBoneSpace) : GetBoundingBoxCentre());
		float damage = ZombieBiteDamage;
		OnDamaged(zombie, this, InjuryType.ZombieBite, InjuryLocation.Torso, zombie.Infection, null, SkillType.Strength, ref damage, 0f, nonDeterministicHitPos, Vector3.zero, ZombieBiteBone, ZombieBitePosInBoneSpace, dontReact: false, assassinate: false, SecrecyMode.Public);
		return true;
	}

	public void OnZombieBiteCancelled(Character zombie)
	{
		if (InteractionObject == zombie && IsAwake && !Zombie)
		{
			ActionAnim anim = ((CurrentActionAnim == ActionAnim.BittenPinnedStruggle) ? ActionAnim.BittenPinnedFree : ((CurrentActionAnim == ActionAnim.BittenFrontStruggle) ? ActionAnim.BittenFrontFree : ActionAnim.BittenRearFree));
			TryStartActionAnim(anim, zombie);
		}
	}

	public void OnTargetStruggledFree(Character target)
	{
		InvisibleStrainJustActivated = TimeSpan.Zero;
		if (Goal != null)
		{
			Goal.OnTargetStruggledFree(this, null, target);
		}
	}

	public float GetStruggleFreePenalty(InfectionType infection)
	{
		if (IsControllableByPlayer())
		{
			switch (infection)
			{
			case InfectionType.Green:
				return 0.3f;
			case InfectionType.Blue:
				return 0.4f;
			case InfectionType.Red:
				return 0.5f;
			case InfectionType.White:
				return 0.6f;
			case InfectionType.Invisible:
				return 0.8f;
			}
		}
		else
		{
			switch (infection)
			{
			case InfectionType.Green:
				return 0.1f;
			case InfectionType.Blue:
				return 0.15f;
			case InfectionType.Red:
				return 0.2f;
			case InfectionType.White:
				return 0.25f;
			case InfectionType.Invisible:
				return 0.3f;
			}
		}
		return 0f;
	}

	public void OnStruggledFree()
	{
		if (InteractionObject is Character character)
		{
			if (character.IsPredicted() == IsPredicted() && !character.Deleted)
			{
				character.OnTargetStruggledFree(this);
			}
			if (character.Zombie)
			{
				GetOrCreateTarget(character).OnGrabbedMe(this);
			}
			InteractionObject = null;
		}
	}

	public bool OnHugStarted(Character hugger)
	{
		if (InteractionObject != null)
		{
			return false;
		}
		if (Zombie)
		{
			return false;
		}
		return TryStartActionAnim(ActionAnim.Hugged, hugger, canInterruptEqualPriority: false);
	}

	public void OnHugFinished(Character hugger)
	{
		if (InteractionObject == hugger)
		{
			TryStartActionAnim(ActionAnim.HuggedFree, hugger);
		}
	}

	public bool OnMatingStarted(Character partner)
	{
		if (InteractionObject != null)
		{
			return false;
		}
		return TryStartActionAnim(ActionAnim.FemaleMating, partner, canInterruptEqualPriority: false);
	}

	public bool GetTimeTillAttack(Character targetCharacter, out TimeSpan remainingTime, out TimeSpan totalTime)
	{
		switch (CurrentActionAnim)
		{
		case ActionAnim.ZombieBitePrepare:
		{
			TimeSpan timeOfEvent4 = AnimationManager.Instance.Anims[38][0].GetTimeOfEvent(AnimationEventType.Bite);
			remainingTime = GetActionAnimTimeRemaining() + timeOfEvent4;
			totalTime = GetActionAnimDuration() + timeOfEvent4;
			return true;
		}
		case ActionAnim.ZombieBiteLoop:
		{
			TimeSpan timeOfEvent3 = AnimationManager.Instance.Anims[38][0].GetTimeOfEvent(AnimationEventType.Bite);
			remainingTime = GetActionAnimTimeTillEvent(AnimationEventType.Bite);
			totalTime = ((GetActionAnimTime() < timeOfEvent3) ? (AnimationManager.Instance.Anims[37][0].GetDuration() + timeOfEvent3) : AnimationManager.Instance.Anims[38][0].GetDuration());
			return true;
		}
		case ActionAnim.ZombieJumpBiteStart:
		{
			TimeSpan timeSpan3 = AnimationManager.Instance.Anims[45][0].GetTimeOfEvent(AnimationEventType.Bite) + AnimationManager.Instance.Anims[44][0].GetDuration();
			remainingTime = GetActionAnimTimeRemaining() + timeSpan3;
			totalTime = GetActionAnimDuration() + timeSpan3;
			return true;
		}
		case ActionAnim.ZombieJumpBitePrepare:
		{
			TimeSpan timeOfEvent = AnimationManager.Instance.Anims[45][0].GetTimeOfEvent(AnimationEventType.Bite);
			remainingTime = GetActionAnimTimeRemaining() + timeOfEvent;
			totalTime = GetActionAnimDuration() + timeOfEvent;
			return true;
		}
		case ActionAnim.ZombieJumpBiteLoop:
		{
			TimeSpan timeOfEvent2 = AnimationManager.Instance.Anims[45][0].GetTimeOfEvent(AnimationEventType.Bite);
			remainingTime = GetActionAnimTimeTillEvent(AnimationEventType.Bite);
			totalTime = ((GetActionAnimTime() < timeOfEvent2) ? (AnimationManager.Instance.Anims[44][0].GetDuration() + timeOfEvent2) : AnimationManager.Instance.Anims[45][0].GetDuration());
			return true;
		}
		case ActionAnim.ZombieBiteStart:
			remainingTime = GetActionAnimTimeRemaining();
			totalTime = GetActionAnimDuration();
			return true;
		case ActionAnim.ZombieJump:
		{
			float num = GetOldVelocity().magnitude * 60f;
			if (!(num < 1E-05f))
			{
				float magnitude = (PosXZ - targetCharacter.PosXZ).magnitude;
				totalTime = TimeSpan.FromSeconds(magnitude / num);
				remainingTime = MathUtil.Max(totalTime - GetActionAnimTime(), TimeSpan.Zero);
				return GetActionAnimTimeSpeedIndependant() <= TimeSpan.FromSeconds(ZombieJumpGoal.DodgeTime);
			}
			break;
		}
		case ActionAnim.PunchLeft:
		case ActionAnim.PunchRight:
		case ActionAnim.Attack:
		case ActionAnim.Kick:
		{
			TimeSpan actionAnimTime = GetActionAnimTime();
			TimeSpan timeSpan = TimeSpan.Zero;
			foreach (AnimEvent @event in AnimWrapper.Events)
			{
				TimeSpan timeSpan2 = TimeSpan.FromSeconds(@event.Time.TotalSeconds / (double)(AnimWrapper.Speed * AnimSpeed));
				AnimationEventType eventType = @event.EventType;
				if ((uint)(eventType - 4) <= 5u)
				{
					if (actionAnimTime <= timeSpan2)
					{
						remainingTime = timeSpan2 - actionAnimTime;
						totalTime = timeSpan2 - timeSpan;
						return true;
					}
					timeSpan = timeSpan2;
				}
			}
			break;
		}
		}
		remainingTime = TimeSpan.Zero;
		totalTime = TimeSpan.Zero;
		return false;
	}

	public bool IsPlayingDead()
	{
		return CurrentActionAnim == ActionAnim.PlayDead;
	}

	public bool IsZombieJumping()
	{
		return CurrentActionAnim == ActionAnim.ZombieJump;
	}

	public bool IsFiring()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if (currentActionAnim == ActionAnim.Fire || currentActionAnim == ActionAnim.FireLastArrow)
		{
			return true;
		}
		return false;
	}

	public bool IsBeingBitten()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 48) <= 5u)
		{
			return true;
		}
		return false;
	}

	public bool IsBeingBittenOrChoked()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 48) <= 5u || (uint)(currentActionAnim - 123) <= 1u || (uint)(currentActionAnim - 160) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsBeingPinnedOrPinning()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 43) <= 4u || (uint)(currentActionAnim - 52) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsZombieAttacking()
	{
		switch (CurrentActionAnim)
		{
		case ActionAnim.ZombieBiteStart:
		case ActionAnim.ZombieBitePrepare:
		case ActionAnim.ZombieBiteLoop:
		case ActionAnim.ZombieBiteFail:
		case ActionAnim.ZombieBiteFinish:
		case ActionAnim.ZombieJumpBiteStart:
		case ActionAnim.ZombieJumpBitePrepare:
		case ActionAnim.ZombieJumpBiteLoop:
		case ActionAnim.ZombieJumpBiteFail:
		case ActionAnim.ZombieJumpBiteFinish:
			return true;
		default:
			return false;
		}
	}

	public bool IsBeingChoked()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 123) <= 1u || (uint)(currentActionAnim - 160) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsChokingSomeone()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 119) <= 3u || (uint)(currentActionAnim - 125) <= 3u || (uint)(currentActionAnim - 156) <= 3u)
		{
			return true;
		}
		return false;
	}

	public bool IsParrying()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 17) <= 4u)
		{
			return true;
		}
		return false;
	}

	public bool IsDodging()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 22) <= 3u)
		{
			return !IsActionAnimInterruptible();
		}
		return false;
	}

	public bool IsCraftingStandaloneAnim()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 90) <= 2u || currentActionAnim == ActionAnim.CraftNonLooped)
		{
			return true;
		}
		return false;
	}

	public bool IsCraftingAnim()
	{
		switch (CurrentActionAnim)
		{
		case ActionAnim.RepairStart:
		case ActionAnim.Repair:
		case ActionAnim.RepairFinish:
		case ActionAnim.CraftStart:
		case ActionAnim.CraftLoop:
		case ActionAnim.CraftEnd:
		case ActionAnim.PotStart:
		case ActionAnim.PotLoop:
		case ActionAnim.PotEnd:
		case ActionAnim.ForgeStart:
		case ActionAnim.ForgeLoop:
		case ActionAnim.ForgeEnd:
		case ActionAnim.CraftTableStart:
		case ActionAnim.CraftTableLoop:
		case ActionAnim.CraftTableEnd:
		case ActionAnim.CraftNonLooped:
			return true;
		default:
			return false;
		}
	}

	public bool IsMining()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 137) <= 5u)
		{
			return true;
		}
		return false;
	}

	public bool IsChoppingWood()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 95) <= 5u)
		{
			return true;
		}
		return false;
	}

	public bool IsLightingFire()
	{
		ActionAnim currentActionAnim = CurrentActionAnim;
		if ((uint)(currentActionAnim - 102) <= 1u)
		{
			return true;
		}
		return false;
	}

	public bool IsUsingEquippedItem()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsUsingEquippedItem();
	}

	public bool IsFacing(Vector2 targetPosXZ, float tolerance)
	{
		return Vector2.Dot(MathUtil.SafeNormalize(targetPosXZ - PosXZ, MathUtil.ToXZ(base.Forward)), MathUtil.ToXZ(base.Forward)) >= Mathf.Cos(tolerance);
	}

	public bool IsMovingTowards(TileObject prop, float tolerance)
	{
		Vector2 nearestPosXZTo = prop.GetNearestPosXZTo(PosXZ);
		Vector2 dirFromAngle = MathUtil.GetDirFromAngle(MovementAngle);
		return Vector2.Dot(MathUtil.SafeNormalize(nearestPosXZTo - PosXZ, dirFromAngle), dirFromAngle) >= Mathf.Cos(tolerance);
	}

	public bool IsSurrendering()
	{
		if (CurrentActionAnim == ActionAnim.HandsUp)
		{
			return true;
		}
		if (Surrendering)
		{
			return true;
		}
		if (Zombie || GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - LastRevivedTime < TimeSpan.FromSeconds(2.0) && !InCombat)
		{
			return true;
		}
		if (CanFollowPlayerIncludeNearbyAllies())
		{
			return Session.Instance.IsPlayerSurrendering();
		}
		if (Community != null)
		{
			return Community.IsSurrendering();
		}
		return false;
	}

	public bool IsTargetObjectSurrendering(TileObject targetObj)
	{
		if (targetObj is Character targetCharacter)
		{
			return IsTargetSurrendering(targetCharacter);
		}
		Community community = targetObj.GetCommunity();
		if (community != null)
		{
			Character nearestAwakeNonZombieMember = community.GetNearestAwakeNonZombieMember(targetObj.GetTile(), null, BaseObjectType.Human, float.MaxValue);
			if (nearestAwakeNonZombieMember != null)
			{
				return IsTargetSurrendering(nearestAwakeNonZombieMember);
			}
			return false;
		}
		return false;
	}

	public bool IsTargetSurrendering(Character targetCharacter)
	{
		if (targetCharacter != null && targetCharacter.IsSurrendering() && Community != null)
		{
			if (targetCharacter.Community != null && targetCharacter.Community.CommunityType == CommunityType.Player)
			{
				return Session.Instance.PlayTime - Community.LastRefusedPlayerSurrender >= MinTimeBetweenPlayerSurrender;
			}
			return true;
		}
		return false;
	}

	public bool WasRevivedInCaptivity(bool hysteresis)
	{
		if (IsControllableByPlayer())
		{
			return false;
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (currentTime - LastRevivedTime < RevivedInCaptivityTime)
		{
			bool flag = Inventory.HasAnyDecentWeapons();
			int num = 0;
			int num2 = 0;
			foreach (Character member in Community.Members)
			{
				if (member != null && member != this && member.IsAwake && member.GetBaseObjectType() == BaseObjectType.Human && !member.Zombie && MathUtil.ToXZ(member.Pos - Pos).sqrMagnitude <= RevivedInCaptivityFriendDist * RevivedInCaptivityFriendDist)
				{
					if (!(currentTime - member.LastRevivedTime < RevivedInCaptivityTime) || member.Inventory.HasAnyDecentWeapons())
					{
						return false;
					}
					num2++;
				}
			}
			foreach (Target target in Targets)
			{
				if (!(target.Object is Character { Community: not null } character) || character.Community == Community || !character.IsAwake || character.GetBaseObjectType() != BaseObjectType.Human || character.Zombie || !target.FullyTracked || !(MathUtil.ToXZ(target.LastKnownPosition - Pos).sqrMagnitude <= RevivedInCaptivityEnemyDist * RevivedInCaptivityEnemyDist) || !IsEnemy(character) || (character.Community.CommunityType != CommunityType.Player && !Session.Instance.CommunityManager.PlayerCommunity.CachedAllies.Contains(character.Community)) || !character.Inventory.HasAnyDecentWeapons())
				{
					continue;
				}
				num++;
				if (flag && !hysteresis)
				{
					if (num >= num2 + 2)
					{
						return true;
					}
				}
				else if (num >= num2 + 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsSneakingUpOn(Character character)
	{
		if (FindActiveGoal(GoalType.MoveToAndChokeHold) is MoveToAndChokeHold moveToAndChokeHold)
		{
			return moveToAndChokeHold.GetTargetCharacter() == character;
		}
		return false;
	}

	public bool IsSneaking()
	{
		if (FindActiveGoal(GoalType.MoveToAndChokeHold) != null)
		{
			return true;
		}
		if (SquadLeader != null && IsCrouching() && SquadLeader.IsCrouching())
		{
			return true;
		}
		return false;
	}

	public bool IsLooter()
	{
		if (Community != null)
		{
			return Community.IsLooterCommunity();
		}
		return false;
	}

	public bool IsAmbient()
	{
		if (Community != null)
		{
			return Community.IsAmbientCommunity();
		}
		return false;
	}

	public bool IsHunter()
	{
		if (Community != null)
		{
			return Community.IsHunterCommunity();
		}
		return false;
	}

	public bool IsFEMA()
	{
		if (Community != null)
		{
			return Community.IsFEMA;
		}
		return false;
	}

	public static ActionPriority GetActionPriority(ActionAnim anim)
	{
		switch (anim)
		{
		case ActionAnim.None:
			return ActionPriority.None;
		case ActionAnim.Drunk:
		case ActionAnim.VeryDrunk:
			return ActionPriority.Idle;
		case ActionAnim.MaleMating:
		case ActionAnim.FemaleMating:
			return ActionPriority.Mating;
		case ActionAnim.Fire:
		case ActionAnim.PunchLeft:
		case ActionAnim.PunchRight:
		case ActionAnim.Attack:
		case ActionAnim.SnapAttack:
		case ActionAnim.AttackJumpingZombie:
		case ActionAnim.Kick:
		case ActionAnim.ZombieBiteStart:
		case ActionAnim.ZombieBitePrepare:
		case ActionAnim.ZombieBiteLoop:
		case ActionAnim.ZombieBiteStumble:
		case ActionAnim.ZombieBiteFail:
		case ActionAnim.ZombieBiteFinish:
		case ActionAnim.ZombieJump:
		case ActionAnim.FireLastArrow:
			return ActionPriority.Attack;
		case ActionAnim.Parry_High:
		case ActionAnim.Parry_Middle:
		case ActionAnim.Parry_Low:
		case ActionAnim.Block_Punch:
		case ActionAnim.Block_Kick:
			return ActionPriority.Parry;
		case ActionAnim.Dodge_Backwards:
		case ActionAnim.Dodge_Forwards:
		case ActionAnim.Dodge_Left:
		case ActionAnim.Dodge_Right:
			return ActionPriority.Dodge;
		case ActionAnim.ZombieJumpBiteStart:
		case ActionAnim.ZombieJumpBitePrepare:
		case ActionAnim.ZombieJumpBiteLoop:
		case ActionAnim.ZombieJumpBiteFail:
		case ActionAnim.ZombieJumpBiteFinish:
			return ActionPriority.JumpAttack;
		case ActionAnim.ChokeHoldStart:
		case ActionAnim.ChokeHoldLoop:
		case ActionAnim.ChokeHoldFail:
		case ActionAnim.ChokeHoldFinish:
		case ActionAnim.SlitThroatStart:
		case ActionAnim.SlitThroatLoop:
		case ActionAnim.SlitThroatFail:
		case ActionAnim.SlitThroatFinish:
		case ActionAnim.RestrainStart:
		case ActionAnim.RestrainLoop:
		case ActionAnim.RestrainFail:
		case ActionAnim.RestrainFinish:
			return ActionPriority.ChokeHold;
		case ActionAnim.HandsUp:
			return ActionPriority.HandsUp;
		case ActionAnim.Damaged_Torso_FromCentre:
		case ActionAnim.Damaged_Torso_FromRight:
		case ActionAnim.Damaged_Torso_FromLeft:
		case ActionAnim.Damaged_Head_FromCentre:
		case ActionAnim.Damaged_Head_FromRight:
		case ActionAnim.Damaged_Head_FromLeft:
		case ActionAnim.Damaged_LeftLeg:
		case ActionAnim.Damaged_RightLeg:
			return ActionPriority.Damaged;
		case ActionAnim.GetUp_Front:
		case ActionAnim.GetUp_Back:
			return ActionPriority.GetUp;
		case ActionAnim.BittenFrontStruggle:
		case ActionAnim.BittenRearStruggle:
		case ActionAnim.BittenPinnedStruggle:
		case ActionAnim.ChokeHoldStruggle:
		case ActionAnim.RestrainedStruggle:
			return ActionPriority.Struggle;
		case ActionAnim.BittenFrontFree:
		case ActionAnim.BittenRearFree:
		case ActionAnim.BittenPinnedFree:
		case ActionAnim.ChokeHoldFree:
		case ActionAnim.RestrainedFree:
		case ActionAnim.HuggedFree:
			return ActionPriority.StruggleFree;
		case ActionAnim.Slide:
			return ActionPriority.Sliding;
		case ActionAnim.SlideRecover:
			return ActionPriority.SlideRecovering;
		case ActionAnim.Vault:
			return ActionPriority.Vaulting;
		case ActionAnim.Panic:
			return ActionPriority.Panicking;
		case ActionAnim.Carried:
			return ActionPriority.Carried;
		default:
			return ActionPriority.Normal;
		}
	}

	public TimeSpan GetActionAnimTime()
	{
		return PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - AnimStartTime;
	}

	public TimeSpan GetActionAnimTimeSpeedIndependant()
	{
		if (AnimWrapper == null)
		{
			return TimeSpan.Zero;
		}
		return TimeSpan.FromSeconds((float)GetActionAnimTime().TotalSeconds * AnimWrapper.Speed * AnimSpeed);
	}

	public TimeSpan GetActionAnimDuration()
	{
		if (CurrentActionAnim == ActionAnim.None)
		{
			return TimeSpan.Zero;
		}
		return AnimWrapper.GetDuration() / AnimSpeed;
	}

	public TimeSpan GetActionAnimDurationSpeedIndependant()
	{
		if (CurrentActionAnim == ActionAnim.None)
		{
			return TimeSpan.Zero;
		}
		return AnimWrapper.GetDurationSpeedIndependant();
	}

	public TimeSpan GetActionAnimTimeRemaining()
	{
		return GetActionAnimDuration() - GetActionAnimTime();
	}

	public float GetActionAnimPlayedFrac()
	{
		if (CurrentActionAnim == ActionAnim.None)
		{
			return 0f;
		}
		return (float)GetActionAnimTime().TotalSeconds / (float)GetActionAnimDuration().TotalSeconds;
	}

	public ActionPriority GetCurrentActionPriority()
	{
		if (!IsActionAnimFinished())
		{
			return GetActionPriority(CurrentActionAnim);
		}
		return ActionPriority.None;
	}

	public bool IsActionAnimLooped()
	{
		if (AnimWrapper != null)
		{
			return AnimWrapper.Looped;
		}
		return false;
	}

	public bool IsActionAnimFinished()
	{
		if (!IsActionAnimLooped())
		{
			return GetActionAnimTime() >= GetActionAnimDuration();
		}
		return false;
	}

	public bool IsActionAnimInterruptible()
	{
		if (AnimWrapper != null)
		{
			if (AnimWrapper.Looped)
			{
				return GetCurrentActionPriority() <= ActionPriority.Idle;
			}
			return GetActionAnimTime().TotalSeconds >= GetActionAnimDuration().TotalSeconds * (double)AnimWrapper.InterruptibleFrac;
		}
		return false;
	}

	public bool IsActionAnimAfterLastEventTime()
	{
		return GetActionAnimTime() * AnimSpeed > AnimWrapper.GetTimeOfLastEvent();
	}

	public bool CanMoveDuringActionAnim()
	{
		if (AnimWrapper != null)
		{
			return AnimWrapper.UpperBodyStateNameHash != 0;
		}
		return true;
	}

	public bool CanTurnDuringActionAnim()
	{
		if (AnimWrapper != null && !AnimWrapper.CanTurnDuringAnim)
		{
			return IsActionAnimInterruptible();
		}
		return true;
	}

	public PipAnimView GetActionAnimPipView()
	{
		if (AnimWrapper == null)
		{
			return PipAnimView.Normal;
		}
		return AnimWrapper.PipView;
	}

	public TimeSpan GetAnimTimeInCurrentLoop()
	{
		if (!AnimWrapper.Looped)
		{
			return TimeSpan.FromTicks(Math.Min(GetActionAnimTime().Ticks, GetActionAnimDuration().Ticks));
		}
		return TimeSpan.FromTicks(GetActionAnimTime().Ticks % GetActionAnimDuration().Ticks);
	}

	public TimeSpan GetActionAnimTimeTillEvent(AnimationEventType eventType)
	{
		TimeSpan timeSpan = AnimWrapper.GetTimeOfEvent(eventType) / AnimSpeed;
		TimeSpan animTimeInCurrentLoop = GetAnimTimeInCurrentLoop();
		if (animTimeInCurrentLoop < timeSpan)
		{
			return timeSpan - animTimeInCurrentLoop;
		}
		return timeSpan + GetActionAnimDuration() - animTimeInCurrentLoop;
	}

	public bool TryStartActionAnimIfNotAlreadyPlaying(ActionAnim anim)
	{
		if (CurrentActionAnim == anim)
		{
			return true;
		}
		return TryStartActionAnim(anim, null);
	}

	public bool TryStartActionAnimFromGoal(ActionAnim anim, TileObject targetObj, float animSpeed)
	{
		return TryStartActionAnim(anim, targetObj, canInterruptEqualPriority: true, TimeSpan.Zero, fromGoal: true, animSpeed);
	}

	public bool TryStartActionAnim(ActionAnim anim, TileObject targetObj)
	{
		return TryStartActionAnim(anim, targetObj, canInterruptEqualPriority: true, TimeSpan.Zero, fromGoal: false);
	}

	public bool TryStartActionAnim(ActionAnim anim, TileObject targetObj, float animSpeed = 1f)
	{
		return TryStartActionAnim(anim, targetObj, canInterruptEqualPriority: true, TimeSpan.Zero, fromGoal: false, animSpeed);
	}

	public bool TryStartActionAnim(ActionAnim anim, TileObject targetObj, bool canInterruptEqualPriority)
	{
		return TryStartActionAnim(anim, targetObj, canInterruptEqualPriority, TimeSpan.Zero, fromGoal: false);
	}

	public bool TryStartActionAnim(ActionAnim anim, TileObject targetObj, bool canInterruptEqualPriority, TimeSpan startTime, bool fromGoal, float animSpeed = 1f)
	{
		if (CurrentAnimState == AnimState.Animation && ((canInterruptEqualPriority ? (GetActionPriority(anim) >= GetCurrentActionPriority()) : (GetActionPriority(anim) > GetCurrentActionPriority())) || IsActionAnimFinished() || IsActionAnimInterruptible()))
		{
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			AnimWrapper anim2 = AnimationManager.Instance.GetAnim(this, targetObj, anim, currentTime);
			if (anim2 != null || anim == ActionAnim.None)
			{
				if (anim2 != null && anim2.IsInteractionWithObject && targetObj == null)
				{
					Debug.LogWarning("No target specified for interaction anim: " + anim);
					return false;
				}
				if (CarryingObject != null && anim != ActionAnim.None && anim != ActionAnim.Drunk && anim != ActionAnim.VeryDrunk && anim != ActionAnim.Drop && anim != ActionAnim.Vault)
				{
					if (IsAuthoritative())
					{
						DropAuthoritative();
					}
					else
					{
						Drop();
					}
				}
				CurrentActionAnim = anim;
				AnimWrapper = anim2;
				AnimStartTime = currentTime - startTime;
				AnimSpeed = animSpeed;
				AnimHeight = 0f;
				InteractionObject = ((anim2 != null && anim2.IsInteractionWithObject) ? targetObj : null);
				ParryingAttacker = null;
				DodgingProjectile = null;
				CanAttackJumpingZombie = CanAttackState.None;
				CanAttackGrabbingZombie = CanAttackState.None;
				IsCurrentActionAnimFromGoal = fromGoal;
				NeedsUpdateEveryFrame = anim == ActionAnim.Vault || anim == ActionAnim.Slide;
				if (anim == ActionAnim.Fire || anim == ActionAnim.FireLastArrow)
				{
					LastFireTime = currentTime;
				}
				if (anim2 != null && Sitting != anim2.Sitting)
				{
					if (anim2.Sitting)
					{
						SetSitting(SittingAround);
					}
					else
					{
						ClearSitting();
					}
				}
				if (CheckFrontmostPrediction(PredictedEventType.TryStartAnimation, anim2?.BaseStateNameHash ?? 0) && Unity.Animator != null && Unity.Animator.isInitialized)
				{
					if (anim2 != null && anim2.BaseStateNameHash != 0)
					{
						if (CurrentAnimState != Unity.UnityAnimState)
						{
							UnityUpdateRagdollEnabled(retainVelocity: true);
						}
						Unity.Animator.SetFloat(AnimHash.WorkSpeed, AnimSpeed);
						if (anim2.UpperBodyStateNameHash != 0 && MovementSpeed > 0f)
						{
							AnimLayer animLayer = ((!anim2.UpperBodyIsAdditive) ? AnimLayer.UpperBody : AnimLayer.UpperBodyAdditive);
							if (!anim2.DontRestartIfAlreadyPLaying || !Unity.Animator.GetCurrentAnimatorStateInfo((int)animLayer).IsName(anim2.UpperBodyStateName))
							{
								Unity.Animator.CrossFadeInFixedTime(anim2.UpperBodyStateNameHash, anim2.TransitionInTime, (int)animLayer, (float)startTime.TotalSeconds);
							}
						}
						else if (!anim2.DontRestartIfAlreadyPLaying || !Unity.Animator.GetCurrentAnimatorStateInfo(0).IsName(anim2.BaseStateName))
						{
							Unity.Animator.CrossFadeInFixedTime(anim2.BaseStateNameHash, anim2.TransitionInTime, 0, (float)startTime.TotalSeconds);
						}
						if (anim2.EquipmentStateNameHash != 0 && Unity.WeaponAnimator != null)
						{
							Unity.WeaponAnimator.SetFloat(AnimHash.WorkSpeed, AnimSpeed);
							Unity.WeaponAnimator.CrossFadeInFixedTime(anim2.EquipmentStateNameHash, anim2.TransitionInTime, 0, (float)startTime.TotalSeconds);
						}
					}
					Unity.UnityActionAnim = anim;
				}
				return true;
			}
			Debug.LogWarning("Animation not found for action: " + anim);
		}
		return false;
	}

	public void StopActionAnim(ActionAnim anim)
	{
		if (CurrentActionAnim != anim)
		{
			return;
		}
		if (AnimWrapper != null && Unity.Animator != null && Unity.Animator.isInitialized && CheckFrontmostPrediction(PredictedEventType.StopAnimation, AnimWrapper.BaseStateNameHash))
		{
			if (CurrentAnimState != Unity.UnityAnimState)
			{
				UnityUpdateRagdollEnabled(retainVelocity: true);
			}
			Unity.Animator.CrossFadeInFixedTime(GetUnityIdleAnimHash(), 0.1f, 0);
			Unity.Animator.SetFloat(AnimHash.WorkSpeed, 1f);
			Unity.UnityActionAnim = anim;
		}
		CurrentActionAnim = ActionAnim.None;
		AnimWrapper = null;
		AnimStartTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		AnimSpeed = 1f;
		AnimHeight = 0f;
		NeedsUpdateEveryFrame = false;
		InteractionObject = null;
		ParryingAttacker = null;
		DodgingProjectile = null;
	}

	private void ClearActionAnim()
	{
		CurrentActionAnim = ActionAnim.None;
		AnimWrapper = null;
		AnimStartTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		AnimSpeed = 1f;
		AnimHeight = 0f;
		NeedsUpdateEveryFrame = false;
		InteractionObject = null;
		ParryingAttacker = null;
		DodgingProjectile = null;
		Unity.UnityActionAnim = ActionAnim.None;
	}

	public void SetAnimSpeed(float animSpeed)
	{
		AnimSpeed = animSpeed;
		if (Unity.Animator != null && Unity.Animator.isInitialized)
		{
			Unity.Animator.SetFloat(AnimHash.WorkSpeed, AnimSpeed);
		}
	}

	public void TriggerAnimEvents(TimeSpan dt)
	{
		if (CurrentActionAnim != ActionAnim.None)
		{
			TimeSpan start = (PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - dt - AnimStartTime) * AnimSpeed;
			AnimWrapper.TriggerEvents(this, start, dt * AnimSpeed);
		}
	}

	public virtual bool OnAnimationEvent(AnimEvent animEvent)
	{
		if (Goal != null && Goal.OnAnimationEvent(this, null, animEvent))
		{
			return true;
		}
		switch (animEvent.EventType)
		{
		case AnimationEventType.Equip:
			EquippedItem = DesiredEquippedItem;
			if (EquippedItem is Gun && CheckFrontmostPrediction(PredictedEventType.GunCockSound))
			{
				PlaySoundOneShot(SoundManager.GunCockSound);
			}
			return true;
		case AnimationEventType.Unequip:
			EquippedItem = null;
			return true;
		case AnimationEventType.Fire:
		{
			RangedWeapon rangedWeapon = EquippedItem as RangedWeapon;
			Target target = null;
			TargettableBodyLocation targetBodyLocation = TargettableBodyLocation.Torso;
			Vector3 targetPos = Vector3.zero;
			float throwAngle = 0f;
			float throwSpeed = 0f;
			PlayerRecord playerRecord = null;
			if (DirectControlled)
			{
				playerRecord = GetPlayerControllingMe();
				if (playerRecord != null && playerRecord.WantLockOnTarget)
				{
					if (playerRecord.TargetObject != null)
					{
						target = GetOrCreateTarget(playerRecord.TargetObject);
						targetBodyLocation = playerRecord.TargetBodyLocationToAimFor;
					}
					else
					{
						targetPos = playerRecord.TargetPos;
					}
					throwAngle = playerRecord.ThrowAngle;
					throwSpeed = playerRecord.ThrowSpeed;
				}
			}
			else
			{
				targetBodyLocation = GetCurrentTargetBodyLocation();
			}
			if (rangedWeapon != null)
			{
				float aimingAccuracy = AimingAccuracy;
				bool flag = rangedWeapon.OnFired(this, target, targetBodyLocation, targetPos, throwAngle, throwSpeed, assassinate: false, SecrecyMode.Public, IsCurrentActionAnimFromGoal);
				if (DirectControlled && playerRecord != null && playerRecord.IsLocal && target != null && target.Object is Character character)
				{
					float magnitude = (character.PosXZ - PosXZ).magnitude;
					rangedWeapon.GetRangeIncludingEffects(this, character, out var accurateRange);
					if (magnitude <= accurateRange)
					{
						if (flag)
						{
							if (aimingAccuracy >= 1f)
							{
								if (HintManager.Instance.Hints[35].LastShown != TimeSpan.Zero)
								{
									HintManager.Instance.Hints[35].Hide();
									HintManager.Instance.Hints[35].MarkPerformed();
								}
								if (HintManager.Instance.Hints[36].LastShown != TimeSpan.Zero)
								{
									HintManager.Instance.Hints[36].Hide();
									HintManager.Instance.Hints[36].MarkPerformed();
								}
							}
						}
						else if (HintManager.Instance.Hints[35].CanShowHint())
						{
							HintManager.Instance.Hints[35].StartShowing(GameImpl.Translate(HintManager.HINT_WaitForCrosshairToGoRed));
						}
					}
					else if (magnitude > accurateRange + 1f && aimingAccuracy < 1f && !flag && HintManager.Instance.Hints[36].CanShowHint())
					{
						HintManager.Instance.Hints[36].StartShowing(GameImpl.Translate(HintManager.HINT_GetCloserForAccurateShot));
					}
				}
			}
			return true;
		}
		case AnimationEventType.Reload:
			if (IsAuthoritative() && EquippedItem is AmmoWeapon ammoWeapon)
			{
				int requiredAmount = ammoWeapon.GetMaxAmmo() - ammoWeapon.CurrentAmmo;
				if (requiredAmount > 0)
				{
					InfectionType infectionType = InfectionType.None;
					EquipmentPrototype takenAmmoType;
					int num;
					if (HasInfiniteAmmo(ammoWeapon))
					{
						takenAmmoType = ((ammoWeapon.CurrentAmmoType != null) ? ammoWeapon.CurrentAmmoType : ammoWeapon.GetPrototype().GetDefaultAmmoPrototype());
						infectionType = ammoWeapon.InfectedWith;
						num = requiredAmount;
					}
					else
					{
						takenAmmoType = ammoWeapon.CurrentAmmoType;
						infectionType = ammoWeapon.InfectedWith;
						num = Inventory.TakeBestAmmoForWeapon(this, DirectControlled ? null : this, ammoWeapon, ref requiredAmount, ref takenAmmoType, ref infectionType);
						if (num == 0 && ammoWeapon.CurrentAmmoType != null)
						{
							takenAmmoType = null;
							infectionType = InfectionType.Count;
							num = Inventory.TakeBestAmmoForWeapon(this, DirectControlled ? null : this, ammoWeapon, ref requiredAmount, ref takenAmmoType, ref infectionType);
						}
					}
					if (takenAmmoType != null)
					{
						ammoWeapon.OnReload(this, num, takenAmmoType, infectionType);
					}
				}
			}
			return true;
		case AnimationEventType.ChanceToParryHigh:
			InformPotentialTargetsOfChanceToParry(AttackType.High);
			return true;
		case AnimationEventType.ChanceToParryMiddle:
			InformPotentialTargetsOfChanceToParry(AttackType.Middle);
			return true;
		case AnimationEventType.ChanceToParryLow:
			InformPotentialTargetsOfChanceToParry(AttackType.Low);
			return true;
		case AnimationEventType.ChanceToDodgeKick:
			InformPotentialTargetsOfChanceToParry(AttackType.Kick);
			return true;
		case AnimationEventType.ChanceToBlockPunch:
			InformPotentialTargetsOfChanceToParry(GetUnarmedAttackTypeFromStance(GetCurrentTargetBodyLocation()));
			return true;
		case AnimationEventType.PunchLeft:
			OnMeleeAttack(GetUnarmedAttackTypeFromStance(GetCurrentTargetBodyLocation()), InjuryType.Punch, Bone.LeftHand, PunchDamage * GetUnarmedDamageModifier());
			return true;
		case AnimationEventType.PunchRight:
			OnMeleeAttack(GetUnarmedAttackTypeFromStance(GetCurrentTargetBodyLocation()), InjuryType.Punch, Bone.RightHand, PunchDamage * GetUnarmedDamageModifier());
			return true;
		case AnimationEventType.KickLeft:
			OnMeleeAttack(AttackType.Kick, InjuryType.Punch, Bone.LeftFoot, KickDamage * GetUnarmedDamageModifier());
			return true;
		case AnimationEventType.KickRight:
			OnMeleeAttack(AttackType.Kick, InjuryType.Punch, Bone.RightFoot, KickDamage * GetUnarmedDamageModifier());
			return true;
		case AnimationEventType.MeleeWeaponQuickAttack:
			if (EquippedItem != null)
			{
				OnMeleeAttack(GetMeleeWeaponAttackTypeFromStance(GetCurrentTargetBodyLocation()), EquippedItem.GetInjuryType(), Bone.MeleeWeapon, EquippedItem.GetDamageIncludingEffects(this) * QuickAttackDamageModifier);
			}
			return true;
		case AnimationEventType.MeleeWeaponHeavyAttack:
			if (EquippedItem != null)
			{
				OnMeleeAttack(GetMeleeWeaponAttackTypeFromStance(GetCurrentTargetBodyLocation()), EquippedItem.GetInjuryType(), Bone.MeleeWeapon, EquippedItem.GetDamageIncludingEffects(this));
			}
			return true;
		case AnimationEventType.StruggleFree:
			OnStruggledFree();
			if (CheckFrontmostPrediction(PredictedEventType.StruggleFree))
			{
				PlaySoundOneShotFromList(SoundManager.PunchSounds);
			}
			return true;
		case AnimationEventType.Drop:
			Drop();
			return true;
		case AnimationEventType.BreakRockSound:
			if (!IsOutdoors())
			{
				PlaySoundOneShotFromList(SoundManager.BreakRockSounds);
			}
			return true;
		default:
			return false;
		}
	}

	public override void PlaySoundOneShotFromList(List<Resource<AudioClip>> clips, float volume)
	{
		if (!InTerrain)
		{
			SoundManager.PlaySound3DFromList(clips, Pos, volume);
		}
		else
		{
			base.PlaySoundOneShotFromList(clips, volume);
		}
	}

	public override void PlaySoundOneShot(AudioClip clip, float volume)
	{
		if (!InTerrain)
		{
			SoundManager.PlaySound3D(clip, Pos, volume);
		}
		else
		{
			base.PlaySoundOneShot(clip, volume);
		}
	}

	public void OnCraftingFinished(TileObject obj)
	{
		if (Goal != null)
		{
			Goal.OnCraftingFinished(this, null, obj);
		}
	}

	public void PickUp(TileObject obj)
	{
		if (obj.IsBeingCarried() || obj.IsDisappeared())
		{
			return;
		}
		if (!IsBeingPredicted() && !IsUnityObjectActive())
		{
			UnityActivate();
		}
		CarryingObject = obj;
		obj.OnPickedUpBy(this, startTransition: true);
		if (obj.GetCommunityId() != CommunityNoticedBodySnatching)
		{
			CommunityNoticedBodySnatching = 0;
		}
		if (!IsAuthoritative())
		{
			return;
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.PickedUp, this, obj);
		if (IsStealingToPickUp(CarryingObject))
		{
			Community community = CarryingObject.GetCommunity();
			if (OnStoleSomething(community, null, 0f, seenByThiefCommunity: false))
			{
				CommunityNoticedBodySnatching = community.Id;
			}
		}
	}

	public static bool IsStealingToPickUp(TileObject obj)
	{
		Community community = obj.GetCommunity();
		Character character = obj as Character;
		bool flag = false;
		if (community != null)
		{
			flag = character == null || obj.GetBaseObjectType() != BaseObjectType.Human;
			if (character != null && (!character.AliveAndNotZombie || character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) != null))
			{
				flag = ((!community.IsAISettlement()) ? (flag | (community.GetNearestLivingNonZombieMember(character.Tile, obj as Character, BaseObjectType.Human, MathUtil.Squared(12f)) != null)) : (flag | community.IsTileInsidePerimeter(character.Tile)));
			}
		}
		return flag;
	}

	public void Drop(Building stashedInBuilding = null)
	{
		if (CarryingObject == null)
		{
			return;
		}
		Animal animal = CarryingObject as Animal;
		if (!IsPredicted() && animal != null && animal.Alive && animal.Community != Community)
		{
			bool flag = true;
			if (animal.Community != null)
			{
				foreach (Character member in animal.Community.Members)
				{
					if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member != this)
					{
						Target target = member.GetTarget(this);
						if (target != null && target.Camouflage < 1f)
						{
							flag = false;
							break;
						}
					}
				}
				if (stashedInBuilding != null && stashedInBuilding.Community == Community)
				{
					flag = true;
				}
			}
			if (flag && !Animal.CanReturnAnimalToOldCommunity(animal, animal.Community, Community))
			{
				bool stolen = false;
				if (animal.Community != null && animal.Community.HasAnyLivingNonZombieMembers())
				{
					Memory.OnMemorableEvent(MemoryPrototype.StoleFrom, this, animal.Community, 1f, SecrecyMode.OnlyKnownToSubjectCommunity);
					stolen = true;
				}
				Community.AddMemberWithNotifications(animal);
				animal.Stolen = stolen;
				StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.StoleAnimal, this, animal);
			}
		}
		CarryingObject.OnDroppedBy(this);
		CarryingObject = null;
	}

	public void DropAuthoritative(Building stashedInBuilding = null)
	{
		if (Predicted != null)
		{
			Predicted.Drop(stashedInBuilding);
		}
		Drop(stashedInBuilding);
	}

	public virtual float GetCarryRotX()
	{
		return CarryRotX;
	}

	public virtual float GetCarryRotY()
	{
		return CarryRotY;
	}

	public virtual float GetCarryRotZ()
	{
		return CarryRotZ;
	}

	public virtual float GetCarryOffsetX()
	{
		return CarryOffsetX;
	}

	public virtual float GetCarryOffsetY()
	{
		return CarryOffsetY;
	}

	public virtual float GetCarryOffsetZ()
	{
		return CarryOffsetZ;
	}

	public virtual Bone GetPickedUpByBone()
	{
		return Bone.Spine;
	}

	public override void OnPickedUpBy(Character character, bool startTransition)
	{
		bool flag = !IsAwake || !IsSmallAnimal();
		if (flag)
		{
			if (CurrentAnimState != AnimState.Ragdoll)
			{
				Ragdollify(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: true);
			}
		}
		else
		{
			TryStartActionAnim(ActionAnim.Carried, null);
		}
		CarriedBy = character;
		UpdateCachedAStarInfo(force: false);
		if (CheckFrontmostPrediction(PredictedEventType.OnPickedUpBy) && IsUnityObjectActive() && character.IsUnityObjectActive())
		{
			string unityBoneName = GetUnityBoneName(GetPickedUpByBone());
			string unityBoneName2 = character.GetUnityBoneName(Bone.RightForearm);
			GameObject gameObject = (flag ? Unity.Obj.FindChild(unityBoneName).gameObject : Unity.Obj);
			GameObject gameObject2 = character.Unity.Obj.FindChild(unityBoneName2).gameObject;
			if (startTransition)
			{
				CarriedTransition = 0f;
				CarriedRotStart = Quaternion.Inverse(gameObject2.transform.rotation) * gameObject.transform.rotation;
				CarriedOffsetStart = gameObject2.transform.worldToLocalMatrix.MultiplyPoint(gameObject.transform.position);
			}
			else
			{
				CarriedInitHackTimer = CarriedInitHackFrames;
				UnitySetKinematic(Unity.Obj, kinematic: true);
			}
			if (flag)
			{
				FixedJoint fixedJoint = gameObject2.GetComponent<FixedJoint>();
				if (gameObject2.GetComponent<FixedJoint>() != null)
				{
					Debug.LogWarning("Didn't expect this to be called when we already have a fixed joint");
				}
				else
				{
					fixedJoint = gameObject2.AddComponent<FixedJoint>();
				}
				fixedJoint.connectedBody = gameObject.GetComponent<Rigidbody>();
			}
		}
		else
		{
			CarriedTransition = 1f;
		}
	}

	public override void OnDroppedBy(Character character)
	{
		bool num = IsRagdoll();
		if (num && IsUnityObjectActive() && character.IsUnityObjectActive())
		{
			FixedJoint component = Util.FindChild(name: character.GetUnityBoneName(Bone.RightForearm), obj: character.Unity.Obj).gameObject.GetComponent<FixedJoint>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
		if (CarriedBy == character)
		{
			CarriedBy = null;
			UpdateCachedAStarInfo(force: false);
		}
		if (num)
		{
			Vector3 zero = Vector3.zero;
			Ragdollify(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, fromStumble: false, retainVelocity: true, zero);
			return;
		}
		Vector2 vector = MathUtil.ToXZ(character.Pos + character.Forward * DropAwakeOffset);
		SetPosition(vector.x, vector.y);
		SetFacingAngle(character.FacingAngle);
		ClearActionAnim();
		TryStartActionAnim(ActionAnim.GetUp_Front, null);
	}

	public void UpdateCarriedBy()
	{
		if (!IsUnityObjectActive() || !CarriedBy.IsUnityObjectActive())
		{
			return;
		}
		bool num = IsRagdoll();
		string unityBoneName = GetUnityBoneName(GetPickedUpByBone());
		string unityBoneName2 = CarriedBy.GetUnityBoneName(Bone.RightForearm);
		GameObject gameObject = (num ? Unity.Obj.FindChild(unityBoneName).gameObject : Unity.Obj);
		GameObject gameObject2 = CarriedBy.Unity.Obj.FindChild(unityBoneName2).gameObject;
		FixedJoint fixedJoint = null;
		if (num)
		{
			fixedJoint = gameObject2.GetComponent<FixedJoint>();
			if (fixedJoint != null)
			{
				fixedJoint.connectedBody = null;
			}
		}
		CarriedTransition = Math.Min(CarriedTransition + Time.deltaTime * 4f, 1f);
		CarriedInitHackTimer = Math.Max(CarriedInitHackTimer - 1, 0);
		Quaternion rotation = gameObject2.transform.rotation * Quaternion.Slerp(CarriedRotStart, Quaternion.Euler(GetCarryRotX(), GetCarryRotY(), GetCarryRotZ()), CarriedTransition);
		Vector3 position = gameObject2.transform.localToWorldMatrix.MultiplyPoint(Vector3.Lerp(CarriedOffsetStart, new Vector3(GetCarryOffsetX(), GetCarryOffsetY(), GetCarryOffsetZ()), CarriedTransition));
		gameObject.transform.SetPositionAndRotation(position, rotation);
		if (num)
		{
			if (fixedJoint == null)
			{
				fixedJoint = gameObject2.AddComponent<FixedJoint>();
			}
			fixedJoint.connectedBody = gameObject.GetComponent<Rigidbody>();
			UnitySetKinematic(Unity.Obj, CarriedInitHackTimer > 0);
		}
	}

	private void UnitySetKinematic(GameObject obj, bool kinematic)
	{
		Rigidbody component = obj.GetComponent<Rigidbody>();
		if (component != null)
		{
			component.isKinematic = kinematic;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			UnitySetKinematic(gameObject, kinematic);
		}
	}

	public override bool IsBeingCarried()
	{
		return CarriedBy != null;
	}

	public bool CanMeleeAttack(TileObject target, AttackType attackType)
	{
		float score;
		return CanMeleeAttack(target, attackType, includeExtraDistForMovement: false, out score);
	}

	public bool CanMeleeAttack(TileObject target, AttackType attackType, out float score)
	{
		return CanMeleeAttack(target, attackType, includeExtraDistForMovement: false, out score);
	}

	public bool CanMeleeAttack(TileObject target, AttackType attackType, bool includeExtraDistForMovement, out float score)
	{
		score = float.MinValue;
		if (InsideBuilding != null)
		{
			return false;
		}
		if (target is Character { Alive: not false } character)
		{
			if (attackType == AttackType.AttackJumpingZombie && !character.IsZombieJumping())
			{
				return false;
			}
			Vector2 vector = character.PosXZ - PosXZ;
			float magnitude = vector.magnitude;
			float meleeAttackRange = GetMeleeAttackRange(attackType, includeExtraDistForMovement);
			float num = ((magnitude > 0.0001f) ? Vector2.Dot(vector / magnitude, MathUtil.ToXZ(base.Forward)) : 1f);
			if (magnitude <= meleeAttackRange && num >= 0f)
			{
				score = (1f - magnitude / meleeAttackRange) * num;
				Vector3 gunPosition = GunPosition;
				Vector3 gunPosition2 = character.GunPosition;
				RaycastResult raycastResult = GameTerrain.Instance.RayCast(gunPosition, gunPosition2, 4136);
				if (DrawMeleeAttackRaycasts)
				{
					if (raycastResult.HitObject != null)
					{
						DebugGraphics.AddPersistentLine(gunPosition, raycastResult.GetHitPosition(), Color.yellow);
						DebugGraphics.AddPersistentLine(raycastResult.GetHitPosition(), gunPosition2, Color.red);
					}
					else
					{
						DebugGraphics.AddPersistentLine(gunPosition, gunPosition2, Color.green);
					}
				}
				return raycastResult.HitObject == null;
			}
		}
		return false;
	}

	public List<TileObject> GetPotentialTargets(AttackType attackType, bool includeExtraDistForMovement)
	{
		TerrainCoord tile = Tile;
		int num = (int)GetMeleeAttackRange(attackType, includeExtraDistForMovement);
		if (IsAuthoritative())
		{
			GameTerrain.Instance.GetObjectsOfTypeInRect(tile - new TerrainCoord(num, num), tile + new TerrainCoord(num, num), PotentialTargets, typeof(Character));
			return PotentialTargets;
		}
		return PredictedObjectManager.Instance.PredictedObjects;
	}

	public float GetMeleeAttackRange(AttackType attackType, bool includeExtraDistForMovement)
	{
		switch (attackType)
		{
		case AttackType.Low:
		case AttackType.Middle:
		case AttackType.High:
		case AttackType.Snap:
		case AttackType.AttackJumpingZombie:
			if (EquippedItem is RangedWeapon)
			{
				return 0f;
			}
			return (float)Math.Ceiling((EquippedItem != null) ? EquippedItem.GetRangeIncludingEffects(this, null) : Hud.DirectControlTargetableRadius) + (includeExtraDistForMovement ? 0.5f : 0f);
		case AttackType.Kick:
			return Hud.DirectControlTargetableRadius + (includeExtraDistForMovement ? 1.5f : 0f);
		default:
			return Hud.DirectControlTargetableRadius + (includeExtraDistForMovement ? 0.5f : 0f);
		}
	}

	public void InformPotentialTargetsOfChanceToParry(AttackType attackType)
	{
		List<TileObject> potentialTargets = GetPotentialTargets(attackType, includeExtraDistForMovement: true);
		foreach (TileObject item in potentialTargets)
		{
			if (item != this && CanMeleeAttack(item, attackType, includeExtraDistForMovement: true, out var _))
			{
				Character character = item as Character;
				if (!character.IsFriendlyFire(this, item, itsATrap: false, attackType))
				{
					character.OnChanceToParry(this, attackType);
				}
			}
		}
		if (potentialTargets == PotentialTargets)
		{
			potentialTargets.Clear();
		}
	}

	public void OnMeleeAttack(AttackType attackType, InjuryType injuryType, Bone bone, float damage)
	{
		Character targetCharacter = null;
		List<TileObject> potentialTargets = GetPotentialTargets(attackType, includeExtraDistForMovement: false);
		TileObject tileObject = null;
		if (DirectControlled)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe != null && playerControllingMe.WantLockOnTarget)
			{
				tileObject = playerControllingMe.TargetObject;
				if (playerControllingMe.IsLocal)
				{
					HintManager.Instance.Hints[0].MarkPerformed();
				}
			}
		}
		float num = float.MinValue;
		foreach (TileObject item in potentialTargets)
		{
			if (item == this || !CanMeleeAttack(item, attackType, out var score))
			{
				continue;
			}
			Character character = item as Character;
			if (!character.IsFriendlyFire(this, tileObject, itsATrap: false, attackType))
			{
				if (IsEnemy(item))
				{
					score += 10f;
				}
				else if (SparringPartner != null && (SparringType == SparringType.Boxing || SparringType == SparringType.Feuding) && tileObject == SparringPartner && character != tileObject)
				{
					continue;
				}
				if (DirectControlled && item == tileObject)
				{
					score += 100f;
				}
				if (score > num)
				{
					targetCharacter = character;
					num = score;
				}
			}
		}
		if (potentialTargets == PotentialTargets)
		{
			potentialTargets.Clear();
		}
		OnMeleeAttack(targetCharacter, attackType, injuryType, bone, damage, assassinate: false, stealthy: false);
	}

	public void OnMeleeAttack(Character targetCharacter, AttackType attackType, InjuryType injuryType, Bone bone, float damage, bool assassinate, bool stealthy)
	{
		ApplyFatiguePenalty((EquippedItem != null) ? EquippedItem.GetFatiguePenaltyWhenAttacking() : MeleeAttackFatiguePenalty);
		bool flag = targetCharacter != null && !assassinate && targetCharacter.IsFriendlyFire(this, targetCharacter, itsATrap: false, attackType);
		bool wasFightToTheDeath = targetCharacter != null && SparringPartner == targetCharacter && SparringType == SparringType.FightToTheDeath;
		if (!flag && targetCharacter != null && CanMeleeAttack(targetCharacter, attackType))
		{
			if (targetCharacter.IsParryingAttack(this, attackType, damage))
			{
				if (targetCharacter.IsParrying())
				{
					MeleeWeapon meleeWeapon = targetCharacter.EquippedItem as MeleeWeapon;
					if ((attackType == AttackType.Punch || attackType == AttackType.PunchLow || attackType == AttackType.PunchHigh) && meleeWeapon != null)
					{
						InjuryLocation injuryLocation = ((bone == Bone.LeftHand) ? InjuryLocation.LeftArm : InjuryLocation.RightArm);
						Vector3 vector = (IsUnityObjectActive() ? GetUnityBoneTransform(bone).Translation() : GetBoundingBoxCentre());
						Vector3 hitPosInBoneSpace = Vector3.zero;
						PickRandomHitPos(injuryLocation, (int)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).Ticks, vector, out bone, out hitPosInBoneSpace);
						float damage2 = meleeWeapon.GetDamageIncludingEffects(targetCharacter) * QuickAttackDamageModifier;
						OnMeleeAttacked(targetCharacter, this, meleeWeapon.GetInjuryType(), injuryLocation, meleeWeapon.InfectedWith, vector, targetCharacter.Forward, bone, hitPosInBoneSpace, damage2, out var _, dontReact: false, assassinate, stealthy, TargettableBodyLocation.Torso, small: true);
					}
				}
				if (CheckFrontmostPrediction(PredictedEventType.ParrySound))
				{
					if (targetCharacter.IsDodging())
					{
						PlaySoundOneShotFromList(SoundManager.PunchMissSounds, 0.5f);
					}
					else if (targetCharacter.IsParrying() && targetCharacter.EquippedItem is MeleeWeapon && (attackType == AttackType.High || attackType == AttackType.Middle || attackType == AttackType.Low))
					{
						PlaySoundOneShotFromList(SoundManager.ParrySounds, 0.5f);
					}
					else if (targetCharacter.IsParrying() && (attackType == AttackType.Punch || attackType == AttackType.PunchHigh || attackType == AttackType.PunchLow || attackType == AttackType.Kick))
					{
						PlaySoundOneShotFromList(SoundManager.PunchBlockedSounds);
					}
				}
			}
			else
			{
				Vector3 vector2 = (IsUnityObjectActive() ? GetUnityBoneTransform(bone).Translation() : GetBoundingBoxCentre());
				Bone bone2 = Bone.Spine;
				Vector3 hitPosInBoneSpace2 = Vector3.zero;
				TargettableBodyLocation targetBodyLocation = TargettableBodyLocation.Torso;
				InjuryLocation injuryLocation2 = InjuryLocation.Torso;
				Vector2 normalized = (PosXZ - targetCharacter.PosXZ).normalized;
				Vector3 attackDir = base.Forward;
				switch (attackType)
				{
				case AttackType.High:
				case AttackType.BludgeonUnconscious:
				case AttackType.KillUnconscious:
				case AttackType.PunchHigh:
					injuryLocation2 = InjuryLocation.Head;
					targetBodyLocation = TargettableBodyLocation.Head;
					break;
				case AttackType.Low:
				case AttackType.PunchLow:
					injuryLocation2 = ((Vector2.Dot(normalized, MathUtil.ToXZ(targetCharacter.Right)) >= 0f) ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg);
					targetBodyLocation = TargettableBodyLocation.Legs;
					break;
				case AttackType.Middle:
				case AttackType.Snap:
				{
					float num = Vector2.Dot(normalized, MathUtil.ToXZ(targetCharacter.Right));
					injuryLocation2 = ((num >= 0.707f) ? InjuryLocation.RightArm : ((num <= -0.707f) ? InjuryLocation.LeftArm : InjuryLocation.Torso));
					targetBodyLocation = TargettableBodyLocation.Torso;
					break;
				}
				case AttackType.AttackJumpingZombie:
					injuryLocation2 = InjuryLocation.Torso;
					targetBodyLocation = TargettableBodyLocation.Torso;
					attackDir = base.Up;
					break;
				}
				int num2 = (int)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).Ticks + 12345;
				targetCharacter.PickRandomHitPos(injuryLocation2, num2 + 1, vector2, out bone2, out hitPosInBoneSpace2);
				if (DirectControlled && GetFatigueMinusAdrenaline() >= ExhaustedFatigueLevel && IsAuthoritative() && GameImpl.Instance.Settings.HintsEnabled && Session.Instance.IsLocalPlayerControllingCharacter(this))
				{
					HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate(HINT_TooTiredToFight));
				}
				bool absorbedByArmor2 = false;
				if (targetCharacter.IsPredicted() == IsPredicted())
				{
					bool small = EquippedItem == null || EquippedItem is HuntingKnife;
					InfectionType infectionType = ((EquippedItem != null) ? EquippedItem.InfectedWith : InfectionType.None);
					targetCharacter.OnMeleeAttacked(this, targetCharacter, injuryType, injuryLocation2, infectionType, vector2, attackDir, bone2, hitPosInBoneSpace2, damage, out absorbedByArmor2, dontReact: false, assassinate, stealthy, targetBodyLocation, small);
				}
				if (CheckFrontmostPrediction(PredictedEventType.MeleeAttackHitSound))
				{
					if (absorbedByArmor2)
					{
						PlaySoundOneShotFromList(SoundManager.HitArmorSounds, 0.5f);
					}
					else if (attackType == AttackType.AttackJumpingZombie)
					{
						PlaySoundOneShotFromList(SoundManager.GoreSplatSounds);
					}
					else if (injuryType == InjuryType.SharpObject)
					{
						PlaySoundOneShotFromList(SoundManager.BladeHitSounds);
					}
					else
					{
						PlaySoundOneShotFromList(SoundManager.PunchSounds);
					}
				}
			}
		}
		else if (CheckFrontmostPrediction(PredictedEventType.MeleeAttackMissSound))
		{
			PlaySoundOneShotFromList(SoundManager.PunchMissSounds, 0.5f);
		}
		if (SparringPartner != targetCharacter && !flag && IsAuthoritative())
		{
			Equipment equipment = null;
			if ((uint)(attackType - 1) <= 4u)
			{
				equipment = EquippedItem;
			}
			AISound sound = new AISound((!stealthy) ? AISoundType.Attack : AISoundType.StealthAttack, Position, equipment?.GetAttackSoundRadius() ?? 0f, GetMaxSoundVisibilityRange(), this, this, targetCharacter, targetCharacter);
			sound.WasFightToTheDeath = wasFightToTheDeath;
			Session.Instance.AISoundManager.AddSound(sound);
		}
	}

	public static AttackType GetMeleeWeaponAttackTypeFromStance(TargettableBodyLocation attackerStance)
	{
		return attackerStance switch
		{
			TargettableBodyLocation.Head => AttackType.High, 
			TargettableBodyLocation.Torso => AttackType.Middle, 
			TargettableBodyLocation.Legs => AttackType.Low, 
			_ => AttackType.Invalid, 
		};
	}

	public static AttackType GetUnarmedAttackTypeFromStance(TargettableBodyLocation attackerStance)
	{
		return attackerStance switch
		{
			TargettableBodyLocation.Head => AttackType.PunchHigh, 
			TargettableBodyLocation.Torso => AttackType.Punch, 
			TargettableBodyLocation.Legs => AttackType.PunchLow, 
			_ => AttackType.Invalid, 
		};
	}

	public static bool CanParryForBodyLocation(TargettableBodyLocation attackerStance, TargettableBodyLocation defenderStance)
	{
		return attackerStance switch
		{
			TargettableBodyLocation.Head => defenderStance != TargettableBodyLocation.Legs, 
			TargettableBodyLocation.Torso => true, 
			TargettableBodyLocation.Legs => defenderStance != TargettableBodyLocation.Head, 
			_ => false, 
		};
	}

	public bool CanParry(Character attacker, AttackType attackType, out bool enabled, out ActionAnim parryAction, out bool isFacingAttacker)
	{
		float score;
		return CanParry(attacker, attackType, out enabled, out parryAction, out isFacingAttacker, out score);
	}

	public virtual bool CanParry(Character attacker, AttackType attackType, out bool enabled, out ActionAnim parryAction, out bool isFacingAttacker, out float score)
	{
		Vector2 lhs = MathUtil.SafeNormalize(attacker.PosXZ - PosXZ, MathUtil.ToXZ(base.Forward));
		float num = Vector2.Dot(lhs, MathUtil.ToXZ(base.Forward));
		isFacingAttacker = num >= 0.5f;
		enabled = false;
		parryAction = ActionAnim.None;
		score = num;
		if (attacker != null && attacker.FindActiveGoal(GoalType.InvisibleStrainSpreadGoal) != null)
		{
			return false;
		}
		if (Zombie)
		{
			enabled = isFacingAttacker && !ShouldLimp();
			parryAction = ActionAnim.Dodge_Backwards;
		}
		else if (isFacingAttacker)
		{
			switch (attackType)
			{
			case AttackType.Low:
			case AttackType.PunchLow:
				if (EquippedItem == null)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Legs, GetCurrentTargetBodyLocation());
					parryAction = ((attackType == AttackType.PunchLow) ? ActionAnim.Block_Punch : ActionAnim.Dodge_Backwards);
				}
				else if (EquippedItem is MeleeWeapon)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Legs, GetCurrentTargetBodyLocation());
					parryAction = ActionAnim.Parry_Low;
				}
				else
				{
					enabled = true;
					parryAction = ActionAnim.Dodge_Backwards;
				}
				break;
			case AttackType.Middle:
			case AttackType.Punch:
				if (EquippedItem == null)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Torso, GetCurrentTargetBodyLocation());
					parryAction = ((attackType == AttackType.Punch) ? ActionAnim.Block_Punch : ActionAnim.Dodge_Backwards);
				}
				else if (EquippedItem is MeleeWeapon)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Torso, GetCurrentTargetBodyLocation());
					parryAction = ActionAnim.Parry_Middle;
				}
				else
				{
					enabled = true;
					parryAction = ActionAnim.Dodge_Backwards;
				}
				break;
			case AttackType.High:
			case AttackType.PunchHigh:
				if (EquippedItem == null)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Head, GetCurrentTargetBodyLocation());
					parryAction = ((attackType == AttackType.PunchHigh) ? ActionAnim.Block_Punch : ActionAnim.Dodge_Backwards);
				}
				else if (EquippedItem is MeleeWeapon)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Head, GetCurrentTargetBodyLocation());
					parryAction = ActionAnim.Parry_High;
				}
				else
				{
					enabled = true;
					parryAction = ActionAnim.Dodge_Backwards;
				}
				break;
			case AttackType.Kick:
				if (EquippedItem == null || EquippedItem is MeleeWeapon)
				{
					enabled = CanParryForBodyLocation(TargettableBodyLocation.Legs, GetCurrentTargetBodyLocation());
				}
				else
				{
					enabled = true;
				}
				parryAction = ((EquippedItem is MeleeWeapon) ? ActionAnim.Parry_Low : ActionAnim.Block_Kick);
				break;
			case AttackType.ZombieBite:
				enabled = true;
				parryAction = ActionAnim.Kick;
				break;
			case AttackType.ZombieJump:
				enabled = true;
				parryAction = ActionAnim.Dodge_Forwards;
				break;
			}
		}
		else
		{
			enabled = !IsTooTiredFor(DodgingFatiguePenalty);
			float magnitude = (attacker.PosXZ - PosXZ).magnitude;
			float meleeAttackRange = GetMeleeAttackRange(attackType, includeExtraDistForMovement: true);
			if (magnitude < meleeAttackRange)
			{
				score = 1f + (meleeAttackRange - magnitude) / meleeAttackRange;
			}
			if (attackType == AttackType.ZombieJump)
			{
				parryAction = ((Vector2.Dot(lhs, MathUtil.ToXZ(base.Right)) >= 0f) ? ActionAnim.Dodge_Right : ActionAnim.Dodge_Left);
			}
			else
			{
				parryAction = ((Vector2.Dot(lhs, MathUtil.ToXZ(base.Right)) >= 0f) ? ActionAnim.Dodge_Left : ActionAnim.Dodge_Right);
				if (num < -0.707f)
				{
					TimeSpan combatStartTime;
					TileObject combatTarget = GetCombatTarget(out combatStartTime);
					if (combatTarget == null || combatTarget == attacker || !IsFacing(combatTarget.PosXZ, MathF.PI / 4f) || !((combatTarget.PosXZ - PosXZ).sqrMagnitude < 9f))
					{
						parryAction = ActionAnim.Dodge_Forwards;
					}
				}
			}
		}
		ActionPriority actionPriority = GetActionPriority(parryAction);
		if (GetCurrentActionPriority() == actionPriority)
		{
			return IsActionAnimInterruptible();
		}
		return GetCurrentActionPriority() < actionPriority;
	}

	private Vector2 GetDodgingDir()
	{
		return CurrentActionAnim switch
		{
			ActionAnim.Dodge_Backwards => -MathUtil.ToXZ(base.Forward), 
			ActionAnim.Dodge_Forwards => MathUtil.ToXZ(base.Forward), 
			ActionAnim.Dodge_Left => -MathUtil.ToXZ(base.Right), 
			ActionAnim.Dodge_Right => MathUtil.ToXZ(base.Right), 
			_ => Vector2.zero, 
		};
	}

	public bool IsParryingAttack(Character attacker, AttackType attackType, float damage)
	{
		if (IsDodging())
		{
			if (attacker.IsZombieJumping())
			{
				return true;
			}
			return Vector2.Dot(GetDodgingDir(), attacker.Forward) > -0.707f;
		}
		if (Vector2.Dot(MathUtil.SafeNormalize(attacker.PosXZ - PosXZ, MathUtil.ToXZ(base.Forward)), MathUtil.ToXZ(base.Forward)) >= 0.5f)
		{
			switch (attackType)
			{
			case AttackType.Low:
				if (CurrentActionAnim != ActionAnim.Parry_Low)
				{
					return CurrentActionAnim == ActionAnim.Parry_Middle;
				}
				return true;
			case AttackType.Middle:
				if (CurrentActionAnim != ActionAnim.Parry_Low && CurrentActionAnim != ActionAnim.Parry_Middle)
				{
					return CurrentActionAnim == ActionAnim.Parry_High;
				}
				return true;
			case AttackType.High:
				if (CurrentActionAnim != ActionAnim.Parry_Middle)
				{
					return CurrentActionAnim == ActionAnim.Parry_High;
				}
				return true;
			case AttackType.Punch:
				if (CurrentActionAnim != ActionAnim.Block_Punch && CurrentActionAnim != ActionAnim.Parry_Low && CurrentActionAnim != ActionAnim.Parry_Middle)
				{
					return CurrentActionAnim == ActionAnim.Parry_High;
				}
				return true;
			case AttackType.PunchHigh:
				if ((CurrentActionAnim != ActionAnim.Block_Punch || GetCurrentTargetBodyLocation() == TargettableBodyLocation.Legs) && CurrentActionAnim != ActionAnim.Parry_Middle)
				{
					return CurrentActionAnim == ActionAnim.Parry_High;
				}
				return true;
			case AttackType.PunchLow:
				if ((CurrentActionAnim != ActionAnim.Block_Punch || GetCurrentTargetBodyLocation() == TargettableBodyLocation.Head) && CurrentActionAnim != ActionAnim.Parry_Low)
				{
					return CurrentActionAnim == ActionAnim.Parry_Middle;
				}
				return true;
			case AttackType.Kick:
				if (CurrentActionAnim != ActionAnim.Block_Kick)
				{
					return CurrentActionAnim == ActionAnim.Parry_Low;
				}
				return true;
			case AttackType.ZombieBite:
				return CurrentActionAnim == ActionAnim.Kick;
			case AttackType.ZombieJump:
				return CurrentActionAnim == ActionAnim.AttackJumpingZombie;
			}
		}
		return false;
	}

	public Character FindBestAttackerForDirectControlledParry(out ActionAnim resultAction, out bool resultEnabled)
	{
		TerrainCoord tile = Tile;
		int num = (int)Math.Ceiling(Hud.DirectControlTargetableRadius + 2f);
		List<TileObject> list;
		if (IsAuthoritative())
		{
			GameTerrain.Instance.GetObjectsOfTypeInRect(tile - new TerrainCoord(num, num), tile + new TerrainCoord(num, num), PotentialTargets, typeof(Character));
			list = PotentialTargets;
		}
		else
		{
			list = PredictedObjectManager.Instance.PredictedObjects;
		}
		float num2 = float.MinValue;
		Character result = null;
		resultEnabled = false;
		resultAction = ActionAnim.None;
		foreach (TileObject item in list)
		{
			if (item is Character character && character != this && character.IsAwake && character.GetCombatTarget(out var _) == this && (IsEnemy(character) || character == SparringPartner) && character.AnimWrapper != null && character.AnimWrapper.CanBeParried(character, this, character.AnimSpeed, out var parryAction, out var enabled, out var score) && score > num2)
			{
				result = character;
				num2 = score;
				resultEnabled = enabled;
				resultAction = parryAction;
			}
		}
		if (list == PotentialTargets)
		{
			list.Clear();
		}
		return result;
	}

	public TileObject GetCombatTarget(out TimeSpan combatStartTime)
	{
		if (DirectControlled)
		{
			combatStartTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
			if (!DirectControlledAiming || DirectControlledTarget == null)
			{
				return null;
			}
			return DirectControlledTarget.Object;
		}
		if (Goal != null)
		{
			return Goal.GetCombatTarget(out combatStartTime);
		}
		combatStartTime = TimeSpan.Zero;
		return null;
	}

	public bool IsFleeing()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsFleeing();
	}

	public bool WantSquadToStayInRange()
	{
		Squad squad = GetSquad();
		if (squad != null && squad.Action == SquadAction.Retreat)
		{
			return true;
		}
		if (Goal == null)
		{
			return false;
		}
		return Goal.WantSquadToStayInRange(this);
	}

	public bool IsHighAlert()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsHighAlert(this);
	}

	public bool IsLowAlert()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsLowAlert(this);
	}

	public bool IsBored()
	{
		if (Goal == null)
		{
			return false;
		}
		return Goal.IsBored(this);
	}

	private float CalcHand2HandSkillFrac()
	{
		if (Zombie)
		{
			return (float)(Infection - 1) / 4f;
		}
		return (float)GetSkillLevelWithEffects(SkillType.HandToHand) / 5f;
	}

	public bool OnChanceToParry(Character attacker, AttackType attackType)
	{
		ActionAnim parryAction = ActionAnim.None;
		bool enabled = default(bool);
		bool isFacingAttacker = default(bool);
		if (!DirectControlled && CanParry(attacker, attackType, out enabled, out parryAction, out isFacingAttacker) && enabled)
		{
			float num = CalcHand2HandSkillFrac();
			float num2 = attacker.CalcHand2HandSkillFrac();
			float num3 = num * (1f - num2 * 0.5f);
			float num4 = 0f;
			if (!Zombie)
			{
				num4 = ((EquippedItem != null || !IsUnarmedAttackType(attackType)) ? ((isFacingAttacker || IsControllableByPlayer()) ? Mathf.Lerp(0.5f, 1f, num3) : Mathf.Lerp(0f, 0.5f, num3)) : ((isFacingAttacker || IsControllableByPlayer()) ? Mathf.Lerp(0.5f, 1f, Mathf.Sqrt(num3)) : Mathf.Lerp(0f, 0.5f, num3)));
			}
			else
			{
				float a;
				switch (attackType)
				{
				default:
					a = 0f;
					break;
				case AttackType.Low:
				case AttackType.PunchLow:
					a = 0.2f;
					break;
				case AttackType.High:
				case AttackType.PunchHigh:
					a = 0.4f;
					break;
				}
				float num5;
				switch (attackType)
				{
				default:
					num5 = 0.2f;
					break;
				case AttackType.Low:
				case AttackType.PunchLow:
					num5 = 0.6f;
					break;
				case AttackType.High:
				case AttackType.PunchHigh:
					num5 = 1f;
					break;
				}
				float b = num5;
				num4 = Mathf.Lerp(a, b, num3);
			}
			if (MathUtil.RandomChoice((float)PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()).TotalMilliseconds, num4) && parryAction != ActionAnim.None && TryStartActionAnim(parryAction, attacker, canInterruptEqualPriority: false))
			{
				if (parryAction == ActionAnim.Dodge_Backwards || parryAction == ActionAnim.Dodge_Forwards || parryAction == ActionAnim.Dodge_Left || parryAction == ActionAnim.Dodge_Right)
				{
					ApplyFatiguePenalty(DodgingFatiguePenalty);
				}
				ParryingAttacker = attacker;
				DodgingProjectile = null;
				if (Zombie && attacker != null && attacker.DirectControlled && Hud.Instance.LocalControlledCharacter == attacker)
				{
					HintManager instance = HintManager.Instance;
					if ((attackType == AttackType.Low || attackType == AttackType.PunchLow) && instance.Hints[2].CanShowHint())
					{
						instance.Hints[2].ShowAndMarkPerformed(GameImpl.Translate(HintManager.HINT_LegAttackDodged));
					}
					if ((attackType == AttackType.High || attackType == AttackType.PunchHigh) && instance.Hints[3].CanShowHint())
					{
						instance.Hints[3].ShowAndMarkPerformed(GameImpl.Translate(HintManager.HINT_HeadAttackDodged));
					}
				}
				return true;
			}
		}
		return false;
	}

	public void OnChanceToDodgeSplashDamage(Character attacker, Vector3 startPos, Vector3 centre, TileObject projectile)
	{
		ActionAnim actionAnim = ActionAnim.None;
		Vector2 v = MathUtil.SafeNormalize(MathUtil.ToXZ(centre - startPos), Vector2.zero);
		Vector2 lhs = MathUtil.SafeNormalize(MathUtil.ToXZ(centre - MathUtil.ToX0Y(v) - Pos), MathUtil.ToXZ(base.Forward));
		float num = Vector2.Dot(lhs, MathUtil.ToXZ(base.Forward));
		actionAnim = ((num >= Mathf.Cos(MathF.PI / 4f)) ? ActionAnim.Dodge_Backwards : ((num <= 0f - Mathf.Cos(MathF.PI / 4f)) ? ActionAnim.Dodge_Forwards : ((!(Vector2.Dot(lhs, MathUtil.ToXZ(base.Right)) >= 0f)) ? ActionAnim.Dodge_Right : ActionAnim.Dodge_Left)));
		if (TryStartActionAnim(actionAnim, attacker, canInterruptEqualPriority: false))
		{
			ApplyFatiguePenalty(DodgingFatiguePenalty);
			ParryingAttacker = attacker;
			DodgingProjectile = projectile;
		}
	}

	public void OnChanceToDodgeVehicle(EnterableVehicle vehicle, Vector2 closestPointXZ)
	{
		ActionAnim actionAnim = ActionAnim.None;
		Vector2 lhs = MathUtil.SafeNormalize(closestPointXZ - PosXZ, MathUtil.ToXZ(base.Forward));
		float num = Vector2.Dot(lhs, MathUtil.ToXZ(base.Forward));
		actionAnim = ((num >= Mathf.Cos(MathF.PI / 4f)) ? ActionAnim.Dodge_Backwards : ((num <= 0f - Mathf.Cos(MathF.PI / 4f)) ? ActionAnim.Dodge_Forwards : ((!(Vector2.Dot(lhs, MathUtil.ToXZ(base.Right)) >= 0f)) ? ActionAnim.Dodge_Right : ActionAnim.Dodge_Left)));
		if (TryStartActionAnim(actionAnim, vehicle, canInterruptEqualPriority: false))
		{
			ApplyFatiguePenalty(DodgingFatiguePenalty);
			ParryingAttacker = vehicle.GetDriver();
			DodgingProjectile = vehicle;
		}
	}

	public bool CanTargetBodyLocation(TargettableBodyLocation location, TileObject targetObj)
	{
		if (targetObj != null && targetObj.IsSmallAnimal())
		{
			return location == TargettableBodyLocation.Torso;
		}
		if (EquippedItem is AmmoWeapon)
		{
			int skillLevelWithEffects = GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType());
			return location switch
			{
				TargettableBodyLocation.Head => skillLevelWithEffects >= RangedHeadshotSkillLevel, 
				TargettableBodyLocation.Legs => skillLevelWithEffects >= RangedLegshotSkillLevel, 
				_ => true, 
			};
		}
		if (EquippedItem is MeleeWeapon)
		{
			int skillLevelWithEffects2 = GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType());
			return location switch
			{
				TargettableBodyLocation.Head => skillLevelWithEffects2 >= MeleeHeadSkillLevel, 
				TargettableBodyLocation.Legs => skillLevelWithEffects2 >= MeleeLegSkillLevel, 
				_ => true, 
			};
		}
		if (EquippedItem == null)
		{
			int skillLevelWithEffects3 = GetSkillLevelWithEffects(SkillType.HandToHand);
			return location switch
			{
				TargettableBodyLocation.Head => skillLevelWithEffects3 >= UnarmedHeadSkillLevel, 
				TargettableBodyLocation.Legs => skillLevelWithEffects3 >= UnarmedLegSkillLevel, 
				_ => true, 
			};
		}
		return location == TargettableBodyLocation.Torso;
	}

	public virtual string GetUnityBoneName(Bone bone)
	{
		return null;
	}

	public Matrix4x4 GetUnityBoneTransform(Bone bone)
	{
		if (Unity.Obj != null && Unity.Obj.activeSelf)
		{
			string unityBoneName = GetUnityBoneName(bone);
			Transform transform = ((unityBoneName.Length == 0) ? Unity.Obj.transform : Unity.Obj.transform.Find(unityBoneName));
			if (transform != null)
			{
				return transform.localToWorldMatrix;
			}
			string text = unityBoneName;
			while (text.Length > 0)
			{
				int num = text.LastIndexOf('/');
				if (num == -1)
				{
					text = "";
					continue;
				}
				text = text.Substring(0, num);
				transform = Unity.Obj.transform.Find(text);
				if (!(transform != null))
				{
					continue;
				}
				return transform.localToWorldMatrix;
			}
			Debug.LogError(GetDisplayNameString() + ": GetUnityBoneTransform could not find bone: " + unityBoneName + ", last parent bone we did find was: " + text + ", CurrentAnimState: " + CurrentAnimState.ToString() + ", CurrentActionAnim: " + CurrentActionAnim);
		}
		else
		{
			Debug.LogWarning("Called GetUnityBoneTransform when Unity object was not active!");
		}
		return Matrix4x4.identity;
	}

	public GameObject GetUnityBone(Bone bone)
	{
		if (Unity.Obj != null)
		{
			string unityBoneName = GetUnityBoneName(bone);
			Transform transform = ((unityBoneName == null) ? null : ((unityBoneName.Length == 0) ? Unity.Obj.transform : Unity.Obj.transform.Find(unityBoneName)));
			if (!(transform != null))
			{
				return null;
			}
			return transform.gameObject;
		}
		return null;
	}

	public GameObject StealUnityWeapon()
	{
		GameObject weaponObj = Unity.WeaponObj;
		if (weaponObj != null)
		{
			weaponObj.transform.parent = null;
		}
		Unity.WeaponObj = null;
		Unity.WeaponAnimator = null;
		Unity.WeaponModel = null;
		if (Unity.Obj != null)
		{
			UnityUpdateMeshList();
		}
		return weaponObj;
	}

	public override AudioSource GetUnityAudioSource()
	{
		return Unity.OneShotAudioSource;
	}

	public void PlayFoostepSound(bool attacking)
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		if ((instance.GameCamera.Focus - Position).sqrMagnitude >= FootstepSoundDist * FootstepSoundDist)
		{
			return;
		}
		TerrainCoord tile = Tile;
		TerrainTex tileTerrainTex = instance2.GetTileTerrainTex(tile);
		FootstepSurfaceType footstepSurfaceType = FootstepSurfaceType.Count;
		switch (tileTerrainTex)
		{
		case TerrainTex.Grass:
			footstepSurfaceType = FootstepSurfaceType.Grass;
			if (instance.Weather.GroundMuddiness >= 0.5f)
			{
				footstepSurfaceType = ((instance.Weather.TemperatureInCelsius > 0f) ? FootstepSurfaceType.Mud : FootstepSurfaceType.IcyGround);
			}
			break;
		case TerrainTex.Road:
		case TerrainTex.Rock:
		case TerrainTex.RockCliff:
			footstepSurfaceType = FootstepSurfaceType.Road;
			break;
		case TerrainTex.Mud:
			footstepSurfaceType = FootstepSurfaceType.Dirt;
			if (instance.Weather.GroundMuddiness >= 0.5f)
			{
				footstepSurfaceType = ((instance.Weather.TemperatureInCelsius > 0f) ? FootstepSurfaceType.Mud : FootstepSurfaceType.IcyGround);
			}
			break;
		case TerrainTex.ConiferFloor:
		case TerrainTex.DeciduousFloor:
			footstepSurfaceType = FootstepSurfaceType.Bush;
			break;
		case TerrainTex.Debris:
			footstepSurfaceType = FootstepSurfaceType.Debris;
			break;
		}
		if (instance.Weather.SnowOnGroundAmount >= 0.75f)
		{
			footstepSurfaceType = FootstepSurfaceType.Snow;
		}
		if (instance2.IsTileRoad(tile.x, tile.y))
		{
			if (footstepSurfaceType != FootstepSurfaceType.Snow && instance2.GetAmountOnPath(PosXZ, out var _, wantRiver: false) < 1f)
			{
				footstepSurfaceType = FootstepSurfaceType.Road;
			}
		}
		else if (instance2.IsTileRiver(tile.x, tile.y))
		{
			Vector2 posXZ = PosXZ;
			if (instance2.GetAmountOnPath(posXZ, out var H2, wantRiver: true) < 1f)
			{
				float tileHeightAtPos = instance2.GetTileHeightAtPos(posXZ, ignoreIce: false, ignoreRoadCamber: true);
				float num = H2 + GameTerrain.RiverHeightOffset - tileHeightAtPos;
				footstepSurfaceType = ((num < 0.01f && Session.Instance.Weather.TemperatureInCelsius < 0f) ? FootstepSurfaceType.Ice : ((!(num < DeepWaterDepth)) ? FootstepSurfaceType.DeepWater : FootstepSurfaceType.ShallowWater));
			}
		}
		if (footstepSurfaceType != FootstepSurfaceType.Count)
		{
			float num2 = (attacking ? 0.5f : Mathf.Clamp01((MovementSpeed - LightFootstepSpeed) / (HeavyFootstepSpeed - LightFootstepSpeed)));
			float num3 = 1f - num2;
			if (IsCrouching())
			{
				num2 *= 0.5f;
				num3 *= 0.5f;
			}
			if (num2 > 0f)
			{
				PlaySoundUsingFootstepAudioSourceFromList(SoundManager.RunningFootstepSounds[(int)footstepSurfaceType], num2 * SoundManager.RunningFootstepVolume[(int)footstepSurfaceType]);
			}
			if (num3 > 0f)
			{
				PlaySoundUsingFootstepAudioSourceFromList(SoundManager.WalkingFootstepSounds[(int)footstepSurfaceType], num3 * SoundManager.WalkingFootstepVolume[(int)footstepSurfaceType]);
			}
		}
	}

	public void PlaySoundUsingFootstepAudioSourceFromList(List<Resource<AudioClip>> clips, float volume)
	{
		if (clips.Count != 0)
		{
			PlaySoundUsingFootstepAudioSource(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], volume);
		}
	}

	public void PlaySoundUsingFootstepAudioSource(Resource<AudioClip> clip, float volume)
	{
		if (!((Session.Instance.GameCamera.Focus - Position).sqrMagnitude >= FootstepSoundDist * FootstepSoundDist))
		{
			AudioSource footstepAudioSource = Unity.FootstepAudioSource;
			if (!(footstepAudioSource == null) && IsUnityObjectActive())
			{
				footstepAudioSource.volume = SoundManager.WorldSoundVolume;
				footstepAudioSource.PlayOneShot(clip, volume * FootstepSoundVolume);
			}
		}
	}

	public override bool ShouldPlaySoundInPip()
	{
		if (!base.ShouldPlaySoundInPip())
		{
			if (InteractionObject != null)
			{
				return Hud.Instance.Pip.FocusObject == InteractionObject;
			}
			return false;
		}
		return true;
	}

	public void PlayVoiceSoundFromList(List<Resource<AudioClip>> clips, VoiceSoundType type)
	{
		if (clips.Count > 0)
		{
			PlayVoiceSound(clips[MathUtil.NonDeterministicRand.Next(clips.Count)], looping: false, type);
		}
	}

	public void PlayVoiceSound(AudioClip clip, bool looping, VoiceSoundType type)
	{
		if (clip == null || Unity.VoiceAudioSource == null || !IsUnityObjectActive())
		{
			return;
		}
		float spatialBlend = 1f;
		if (SoundManager.Instance.IsTooFarAwayToHearSound(Position))
		{
			if (!ShouldPlaySoundInPip())
			{
				return;
			}
			spatialBlend = 0f;
		}
		if (Predicted != null)
		{
			Predicted.StopVoiceSound();
		}
		if (Authoritative != null)
		{
			Authoritative.StopVoiceSound();
		}
		StopVoiceSound();
		Unity.CurrentVoiceSoundType = type;
		Unity.VoiceAudioSource.clip = clip;
		Unity.VoiceAudioSource.volume = SoundManager.WorldSoundVolume * VoiceSoundVolume;
		Unity.VoiceAudioSource.loop = looping;
		Unity.VoiceAudioSource.time = 0f;
		Unity.VoiceAudioSource.spatialBlend = spatialBlend;
		Unity.VoiceAudioSource.Play();
		if (looping)
		{
			Unity.VoiceAudioSource.time = MathUtil.NonDeterministicRand.RandomFloat() * (Unity.VoiceAudioSource.clip.length * 0.9f);
		}
	}

	public void StopVoiceSound()
	{
		if (Unity.VoiceAudioSource != null && Unity.VoiceAudioSource.isPlaying)
		{
			Unity.VoiceAudioSource.Stop();
			Unity.VoiceAudioSource.clip = null;
			Unity.CurrentVoiceSoundType = VoiceSoundType.None;
		}
	}

	public bool IsAnyVoiceSoundPlaying()
	{
		if (Predicted != null && Predicted.Unity.VoiceAudioSource != null && Predicted.Unity.VoiceAudioSource.isPlaying)
		{
			return true;
		}
		if (Authoritative != null && Authoritative.Unity.VoiceAudioSource != null && Authoritative.Unity.VoiceAudioSource.isPlaying)
		{
			return true;
		}
		if (Unity.VoiceAudioSource != null)
		{
			return Unity.VoiceAudioSource.isPlaying;
		}
		return false;
	}

	public bool IsVoiceSoundPlaying(VoiceSoundType type)
	{
		if (Predicted != null && Predicted.Unity.VoiceAudioSource != null && Predicted.Unity.VoiceAudioSource.isPlaying && Unity.CurrentVoiceSoundType == type)
		{
			return true;
		}
		if (Authoritative != null && Authoritative.Unity.VoiceAudioSource != null && Authoritative.Unity.VoiceAudioSource.isPlaying && Unity.CurrentVoiceSoundType == type)
		{
			return true;
		}
		if (Unity.VoiceAudioSource != null && Unity.VoiceAudioSource.isPlaying)
		{
			return Unity.CurrentVoiceSoundType == type;
		}
		return false;
	}

	public void UpdateLoopingSound()
	{
		VoiceSoundType voiceSoundType = ((Unity.VoiceAudioSource != null && Unity.VoiceAudioSource.isPlaying && Unity.VoiceAudioSource.loop) ? Unity.CurrentVoiceSoundType : VoiceSoundType.None);
		VoiceSoundType voiceSoundType2 = ((CurrentActionAnim == ActionAnim.Slide) ? VoiceSoundType.Sliding : ((IsBurning() || EquippedItem is MolotovCocktail) ? VoiceSoundType.Burning : VoiceSoundType.None));
		if (IsBeingPredicted() || Session.Instance == null || Session.Instance.IsPaused())
		{
			voiceSoundType2 = VoiceSoundType.None;
		}
		if (voiceSoundType2 == voiceSoundType)
		{
			return;
		}
		if (voiceSoundType2 != VoiceSoundType.None)
		{
			Resource<AudioClip> resource = null;
			switch (voiceSoundType2)
			{
			case VoiceSoundType.Sliding:
				resource = SoundManager.SlidingSound;
				break;
			case VoiceSoundType.Burning:
				resource = SoundManager.BurningSounds[MathUtil.NonDeterministicRand.Next(SoundManager.BurningSounds.Count)];
				break;
			}
			if (resource != null)
			{
				PlayVoiceSound(resource, looping: true, voiceSoundType2);
			}
		}
		else
		{
			Unity.VoiceAudioSource.volume = Math.Max(0f, Unity.VoiceAudioSource.volume - 1f * Time.unscaledDeltaTime);
			if (Unity.VoiceAudioSource.volume <= 0f)
			{
				Unity.VoiceAudioSource.Stop();
				Unity.VoiceAudioSource.clip = null;
			}
		}
	}

	public bool CanDiscoverStuffFromSpeech()
	{
		if (DirectControlled)
		{
			return true;
		}
		if (GetPlayerControllingMe() != null)
		{
			return true;
		}
		return false;
	}

	public void OnSpeechFinished(bool interrupted)
	{
		Character listener = Listener;
		BaseObject speechObject = SpeechObject;
		MemoryParam speechParam = SpeechParam;
		Speech speaking = Speaking;
		Speaking = null;
		Listener = null;
		SpeechObject = null;
		SpeechParam = default(MemoryParam);
		SpeakingText = null;
		SpeakingTextEnglish = null;
		SpeakingParamResults.Clear();
		SpeakingEmoticons.Clear();
		CurrentSpeechAnimState = SpeechAnimState.NotStarted;
		if (!IsAuthoritative())
		{
			return;
		}
		StoryManager.Instance.OnInterestingSpeechFinished(this);
		if ((!interrupted || (speaking.UninterruptibleEvents != null && speaking.UninterruptibleEvents.Count > 0) || (speaking.NoReply != null && speaking.NoReply.Count > 0)) && (CanDiscoverStuffFromSpeech() || (listener != null && listener.CanDiscoverStuffFromSpeech()) || speaking.Importance >= Importance.Quest))
		{
			StoryManager.DiscoverPersonalityFromConditions(speaking.Conditions, speaking.PriorityTerms, not: false, this, listener, speechObject, speechParam);
		}
		if (!interrupted)
		{
			if (speaking.Once)
			{
				StoryManager.Instance.RememberSpokenOnce(speaking);
			}
			if (speaking.RepeatTime != 0f)
			{
				RememberSpokenSpeech(speaking, listener);
			}
			StoryManager.QueueEvents(speaking.Events, this, listener, speechObject, speechParam);
		}
		StoryManager.QueueEvents(speaking.UninterruptibleEvents, this, listener, speechObject, speechParam);
		if (!interrupted)
		{
			PopQueuedSpeech(speaking, listener, speechObject, speechParam);
		}
		BaseObject resultObj = null;
		MemoryParam param = speechParam;
		Speech cont = (interrupted ? null : StoryManager.Instance.GetContinue(this, listener, speechObject, speaking, out resultObj, ref param, Session.Instance.DeterministicRand));
		if (Goal != null)
		{
			Goal.OnSpeechFinished(this, null, speaking, listener, interrupted, cont, resultObj, param);
		}
	}

	public void OnSpokenToStarted(Character character, Speech speech, BaseObject speechObject, MemoryParam speechParam, Speech replyTo, BaseObject replyReferringTo, MemoryParam replyParam)
	{
		GetOrCreateTarget(character).UpdateLastKnownInfo(this);
		if (Goal != null)
		{
			Goal.OnSpokenToStarted(this, null, character, speech, speechObject, speechParam, replyTo, replyReferringTo, replyParam);
		}
	}

	public bool OnSpokenToFinished(Character speaker, Speech speech, bool interrupted, Speech cont, Speech specialBehaviourTriggered)
	{
		if (Goal != null)
		{
			return Goal.OnSpokenToFinished(this, null, speaker, speech, interrupted, cont, specialBehaviourTriggered);
		}
		return false;
	}

	public Speech SpeakForSituation(Character listener, BaseObject obj, SpeechSituation sit, MemoryParam param)
	{
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, listener, obj, sit, param);
		if (speechForSituation != null)
		{
			Speak(speechForSituation, listener, obj, param);
		}
		return speechForSituation;
	}

	private bool ShouldLogSpeech()
	{
		switch (Speaking.Importance)
		{
		case Importance.Cheer:
		case Importance.FriendComplaint:
		case Importance.Encouragement:
			return false;
		case Importance.Low:
			if (GetPlayerControllingMe() == null)
			{
				if (Listener != null)
				{
					return Listener.GetPlayerControllingMe() != null;
				}
				return false;
			}
			return true;
		case Importance.FriendUnderAttack:
			return false;
		case Importance.Memory:
			if (Speaking.Situation == SpeechSituation.MemoryDisbelief || Speaking.Situation == SpeechSituation.MemoryResponse)
			{
				if (GetPlayerControllingMe() == null)
				{
					if (Listener != null)
					{
						return Listener.GetPlayerControllingMe() != null;
					}
					return false;
				}
				return true;
			}
			if (FindActiveGoal(GoalType.GreetingResponse) == null)
			{
				break;
			}
			if (GetPlayerControllingMe() == null)
			{
				if (Listener != null)
				{
					return Listener.GetPlayerControllingMe() != null;
				}
				return false;
			}
			return true;
		case Importance.Quest:
		case Importance.Critical:
		case Importance.Uninterruptible:
			return true;
		}
		if (!IsControllableByOrFollowingPlayer())
		{
			if (Listener != null)
			{
				return Listener.IsControllableByOrFollowingPlayer();
			}
			return false;
		}
		return true;
	}

	public bool Speak(Speech speech)
	{
		return Speak(speech, null, null, default(MemoryParam), null, null);
	}

	public bool Speak(Speech speech, Character listener)
	{
		return Speak(speech, listener, null, default(MemoryParam), null, null);
	}

	public bool Speak(Speech speech, Character listener, MemoryParam param)
	{
		return Speak(speech, listener, null, param, null, null);
	}

	public bool Speak(Speech speech, Character listener, BaseObject obj)
	{
		return Speak(speech, listener, obj, default(MemoryParam), null, null);
	}

	public bool Speak(Speech speech, Character listener, BaseObject obj, MemoryParam param)
	{
		return Speak(speech, listener, obj, param, null, null);
	}

	public bool Speak(Speech speech, Character listener, BaseObject obj, MemoryParam param, string chatSpeech, string chatSpeaker)
	{
		if (speech == null)
		{
			return false;
		}
		if (speech.MustFinish)
		{
			PushQueuedSpeech(speech, listener, obj, param);
		}
		if (Speaking != null)
		{
			if (Speaking.Situation > speech.Situation)
			{
				return false;
			}
			if (Speaking.Situation == speech.Situation && Speaking.Situation == SpeechSituation.Pain)
			{
				return false;
			}
			OnSpeechFinished(interrupted: true);
		}
		Speaking = speech;
		Listener = listener;
		SpeechObject = obj;
		SpeechParam = param;
		SpeechStartTime = Session.Instance.PlayTime;
		Speech.EvaluateSpeechParams(speech.Params, this, listener, obj, SpeakingParamResults, param, chatSpeech, chatSpeaker, Session.Instance.DeterministicRand, null, speech.UniqueID);
		Speech.BuildSpeechText(speech.TextHash, speech.Params, this, listener, obj, SpeakingParamResults, out SpeakingText, SpeakingEmoticons, englishOnly: false, isQuest: false);
		Speech.BuildSpeechText(speech.TextHash, speech.Params, this, listener, obj, SpeakingParamResults, out SpeakingTextEnglish, null, englishOnly: true, isQuest: false);
		if (speech.Anim != ActionAnim.None)
		{
			CurrentSpeechAnimState = (TryStartActionAnim(speech.Anim, listener, canInterruptEqualPriority: true) ? SpeechAnimState.Started : SpeechAnimState.NotStarted);
		}
		if (IsAuthoritative())
		{
			StoryManager.Instance.OnInterestingSpeechStarted(this, listener, speech.Importance);
			if (ShouldLogSpeech())
			{
				LogEvent logEvent = new LogEvent(LogEventType.Speech);
				logEvent.Character = this;
				logEvent.Listener = listener;
				logEvent.ReferringTo = obj;
				logEvent.Speech = Speaking;
				logEvent.SpeakingParamResults = new List<SpeechParamResult>();
				SpeakingParamResults.CopyToList(logEvent.SpeakingParamResults);
				logEvent.SpeakerWasDirectControlled = Session.Instance.GetPlayerControllingCharacter(this) != null;
				Session.Instance.AddLogEvent(logEvent);
			}
		}
		if (Zombie && IsAuthoritative())
		{
			PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)Appearance.Gender], VoiceSoundType.ZombieSnarl);
		}
		if (!speech.MustFinish)
		{
			PopQueuedSpeech(speech, listener, obj, param);
		}
		return true;
	}

	public void SkipSpeech(bool onlySkipLipsMoving)
	{
		if (Speaking != null)
		{
			TimeSpan timeSpan = Session.Instance.PlayTime - SpeechStartTime;
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(StoryManager.GetSpeechLipsMoveTime(SpeakingTextEnglish));
			if (timeSpan < timeSpan2)
			{
				SpeechStartTime = Session.Instance.PlayTime - timeSpan2;
			}
			else if (!onlySkipLipsMoving)
			{
				OnSpeechFinished(interrupted: false);
			}
		}
	}

	public bool IsSpeechSkippable()
	{
		if (Speaking == null)
		{
			return false;
		}
		SpeechSituation situation = Speaking.Situation;
		if (situation == SpeechSituation.Pain || situation == SpeechSituation.Death)
		{
			return false;
		}
		if (Speaking.Importance >= Importance.Critical && Speaking.Anim != ActionAnim.None)
		{
			return false;
		}
		return true;
	}

	public bool CanNarrate()
	{
		if (Zombie)
		{
			return false;
		}
		if (Consciousness >= Consciousness.Unconscious)
		{
			return false;
		}
		if (GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (InCombat)
		{
			return false;
		}
		if (UnderAttackRefCount > 0)
		{
			return false;
		}
		if (Speaking != null)
		{
			return false;
		}
		if (Disappeared)
		{
			return false;
		}
		if (!IsOutdoors())
		{
			return false;
		}
		if (FindActiveGoal(GoalType.Conversation) != null)
		{
			return false;
		}
		return true;
	}

	public int GetQueuedSpeechIndex(Speech speech, Character target, BaseObject speechObject, MemoryParam param)
	{
		for (int i = 0; i < QueuedSpeeches.Count; i++)
		{
			if (QueuedSpeeches[i].Speech != null && QueuedSpeeches[i].Speech.UniqueID == speech.UniqueID && QueuedSpeeches[i].Target == target && QueuedSpeeches[i].SpeechObject == speechObject && QueuedSpeeches[i].Param == param)
			{
				return i;
			}
		}
		return -1;
	}

	public void PushQueuedSpeech(Speech speech, Character target, BaseObject speechObject, MemoryParam param)
	{
		if (GetQueuedSpeechIndex(speech, target, speechObject, param) == -1)
		{
			QueuedSpeeches.Add(QueuedSpeech.Create(speech, target, speechObject, param, TimeSpan.Zero));
		}
		UpdateThinkBucket();
	}

	public void PopQueuedSpeech(Speech speech, Character target, BaseObject speechObject, MemoryParam param)
	{
		int queuedSpeechIndex = GetQueuedSpeechIndex(speech, target, speechObject, param);
		if (queuedSpeechIndex != -1)
		{
			QueuedSpeeches.RemoveAt(queuedSpeechIndex);
		}
	}

	public bool HasQueuedSpeechOfType(SpeechSituation sit)
	{
		if (Speaking != null && Speaking.Situation == sit)
		{
			return true;
		}
		for (int i = 0; i < QueuedSpeeches.Count; i++)
		{
			if (QueuedSpeeches[i].Speech.Situation == sit)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasQueuedSpeechOfImportance(Importance importance)
	{
		if (Speaking != null && Speaking.Importance == importance)
		{
			return true;
		}
		for (int i = 0; i < QueuedSpeeches.Count; i++)
		{
			if (QueuedSpeeches[i].Speech.Importance == importance)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasQueuedSpeechWith(Character otherCharacter)
	{
		if (Speaking != null && Listener == otherCharacter)
		{
			return true;
		}
		for (int i = 0; i < QueuedSpeeches.Count; i++)
		{
			if (QueuedSpeeches[i].Target == otherCharacter)
			{
				return true;
			}
		}
		return false;
	}

	public void SetQueuedSpeechLastFailedAttemptTime(Speech speech, Character target, BaseObject speechObject, MemoryParam param, TimeSpan time)
	{
		int queuedSpeechIndex = GetQueuedSpeechIndex(speech, target, speechObject, param);
		if (queuedSpeechIndex != -1)
		{
			QueuedSpeeches[queuedSpeechIndex] = QueuedSpeech.Create(speech, target, speechObject, param, time);
		}
	}

	public bool HasSpokenSpeechRecently(Speech speech, Character target, float time)
	{
		int spokenSpeechIndex = GetSpokenSpeechIndex(speech, target);
		if (spokenSpeechIndex != -1)
		{
			return Session.Instance.PlayTime < SpokenSpeeches[spokenSpeechIndex].SpokenTime + TimeSpan.FromSeconds(time);
		}
		return false;
	}

	public bool HasSpokenSpeechToAnyoneRecently(Speech speech, float time)
	{
		for (int i = 0; i < SpokenSpeeches.Count; i++)
		{
			if (SpokenSpeeches[i].Speech.UniqueID == speech.UniqueID && Session.Instance.PlayTime < SpokenSpeeches[i].SpokenTime + TimeSpan.FromSeconds(time))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSpokenSpeechForSituationRecently(SpeechSituation situation, Character target, TimeSpan time, out TimeSpan spokenTime)
	{
		bool result = false;
		spokenTime = Session.Instance.PlayTime - time;
		int num = target?.Id ?? 0;
		for (int i = 0; i < SpokenSpeeches.Count; i++)
		{
			if (SpokenSpeeches[i].Speech.Situation == situation && SpokenSpeeches[i].TargetId == num && SpokenSpeeches[i].SpokenTime > spokenTime)
			{
				spokenTime = SpokenSpeeches[i].SpokenTime;
				result = true;
			}
		}
		return result;
	}

	public bool HasSpokenSpeechForSituationToAnyoneRecently(SpeechSituation situation, TimeSpan time, out TimeSpan spokenTime)
	{
		bool result = false;
		spokenTime = Session.Instance.PlayTime - time;
		for (int i = 0; i < SpokenSpeeches.Count; i++)
		{
			if (SpokenSpeeches[i].Speech.Situation == situation && SpokenSpeeches[i].SpokenTime > spokenTime)
			{
				spokenTime = SpokenSpeeches[i].SpokenTime;
				result = true;
			}
		}
		return result;
	}

	public int GetSpokenSpeechIndex(Speech speech, Character target)
	{
		int num = target?.Id ?? 0;
		for (int i = 0; i < SpokenSpeeches.Count; i++)
		{
			if (SpokenSpeeches[i].Speech.UniqueID == speech.UniqueID && SpokenSpeeches[i].TargetId == num)
			{
				return i;
			}
		}
		return -1;
	}

	public void RememberSpokenSpeech(Speech speech, Character target, bool force = false)
	{
		if (speech.RepeatTime != 0f || force)
		{
			int spokenSpeechIndex = GetSpokenSpeechIndex(speech, target);
			if (spokenSpeechIndex != -1)
			{
				SpokenSpeeches[spokenSpeechIndex] = SpeechMemory.Create(speech, target);
			}
			else
			{
				SpokenSpeeches.Add(SpeechMemory.Create(speech, target));
			}
			StoryManager.Instance.SetConditionsDirty();
		}
	}

	public void SetTimer(string name, TimeSpan time)
	{
		for (int i = 0; i < Timers.Count; i++)
		{
			if (Timers[i].Name == name)
			{
				Timers.RemoveAt(i);
				break;
			}
		}
		Timers.Add(Timer.Create(name, time));
	}

	public bool IsTimerRunning(string name)
	{
		for (int i = 0; i < Timers.Count; i++)
		{
			if (Timers[i].Name == name && Session.Instance.PlayTime < Timers[i].FinishTime)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsTimerAtLeast(string name, float seconds)
	{
		for (int i = 0; i < Timers.Count; i++)
		{
			if (Timers[i].Name == name)
			{
				return Session.Instance.PlayTime >= Timers[i].StartTime + TimeSpan.FromSeconds(seconds);
			}
		}
		return true;
	}

	public Character GetListener()
	{
		if (FindActiveGoal(GoalType.SpeakToTarget) is SpeakToTarget speakToTarget)
		{
			return speakToTarget.GetTargetCharacter();
		}
		return null;
	}

	public void SetRecentActivity(RecentActivityType type, Character with)
	{
		RecentActivityType = type;
		RecentActivityTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		RecentActivityWith = with;
	}

	public void SetRecentActivityAtPos(RecentActivityType type, Vector3 pos)
	{
		RecentActivityType = type;
		RecentActivityTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		RecentActivityWith = null;
		RecentActivityPos = pos;
	}

	public void ClearRecentActivity()
	{
		RecentActivityType = RecentActivityType.None;
		RecentActivityTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		RecentActivityWith = null;
	}

	public bool HasRecentActivityOfType(RecentActivityType type, TimeSpan withinTime)
	{
		if (RecentActivityType == type)
		{
			return PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - RecentActivityTime < withinTime;
		}
		return false;
	}

	public bool IsOutdoors()
	{
		if (!InTerrain)
		{
			return IsPredicted();
		}
		return true;
	}

	public bool IsGuarding()
	{
		if (InsideBuilding != null)
		{
			return IsOutdoors();
		}
		return false;
	}

	public bool IsInBuildingSusceptibleToBulletHits()
	{
		if (InsideBuilding != null)
		{
			return InsideBuilding.IsSusceptibleToBulletHits();
		}
		return false;
	}

	public bool IsInEnclosedArea()
	{
		TerrainCoord terrainCoord = ((InsideBuilding == null) ? Tile : InsideBuilding.GetEntranceTile(0));
		return GameTerrain.Instance.IsTileEnclosed(terrainCoord.x, terrainCoord.y);
	}

	private bool ShouldReactToAttackSound(AISoundType soundType, Character source, TileObject targetObj, TileObject intendedTargetObj, Target target, bool seen, bool heard, bool wasFightToTheDeath)
	{
		if (targetObj == null)
		{
			return false;
		}
		if (Community == null)
		{
			return false;
		}
		if (source.Community == null)
		{
			return false;
		}
		if (source.Community == Community)
		{
			return false;
		}
		Community community = targetObj.GetCommunity();
		if (source.Community == community)
		{
			return false;
		}
		Character character = targetObj as Character;
		if (character != null && character.Zombie)
		{
			return false;
		}
		if (source.SparringPartner == character || wasFightToTheDeath)
		{
			return false;
		}
		if (!target.FullyTracked)
		{
			return false;
		}
		if (IsEmpathyDisabled(targetObj))
		{
			return false;
		}
		if (!source.IsControllableByOrFollowingPlayer() && intendedTargetObj != targetObj && intendedTargetObj != null && (intendedTargetObj.GetCommunity() != Community || (intendedTargetObj is Character && ((Character)intendedTargetObj).Zombie)))
		{
			return false;
		}
		if (community == Community)
		{
			return true;
		}
		if (SquadLeader != null && community != null && community == SquadLeader.Community && source.Community.IsLooterCommunity())
		{
			return true;
		}
		if (CanFollowPlayer && community != null && community.CommunityType == CommunityType.Player && source.Community.IsLooterCommunity())
		{
			return true;
		}
		return false;
	}

	public void CheckIfSoundCanBeSeenOrHeard(AISound sound, out bool seen, out bool heard)
	{
		heard = MathUtil.ToXZ(sound.Pos - Position).sqrMagnitude <= sound.SoundRadius * sound.SoundRadius;
		if (sound.Type == AISoundType.Choke && Zombie)
		{
			heard = false;
		}
		seen = IsOutdoors() && MathUtil.ToXZ(sound.Pos - Position).sqrMagnitude <= sound.SightRadius * sound.SightRadius && IsFacing(MathUtil.ToXZ(sound.Pos), MathF.PI / 180f * VisionConeDeg);
		if (sound.Type == AISoundType.StealthAttack || sound.Type == AISoundType.Radio)
		{
			bool num = seen;
			Target target = GetTarget(sound.Source);
			seen &= target != null && target.Camouflage < 0.5f;
			if (num && target != null && sound.Type != AISoundType.Radio)
			{
				target.Camouflage -= 0.25f;
			}
		}
		else
		{
			if (!seen)
			{
				return;
			}
			if (sound.Type == AISoundType.Choke)
			{
				bool flag = false;
				float num2 = GetSightRange();
				if (MathUtil.ToXZ(sound.Pos - EyePosition).sqrMagnitude <= num2 * num2 && sound.Target is Character)
				{
					RaycastResult raycastResult = GameTerrain.Instance.RayCast(EyePosition, sound.Pos + new Vector3(0f, ((Character)sound.Target).EyeHeight, 0f), 40);
					if (raycastResult.HitObject == null || raycastResult.HitObject == sound.Target)
					{
						flag = true;
					}
				}
				Target target2 = GetTarget(sound.Source);
				seen &= (target2?.FullyTracked ?? false) || flag;
			}
			else
			{
				if (sound.Type == AISoundType.Attack)
				{
					return;
				}
				TileObject tileObject = (AISound.SoundComesFromSource(sound.Type) ? sound.Source : sound.Target);
				if (tileObject is Character)
				{
					if (tileObject != this)
					{
						Target target3 = GetTarget(tileObject);
						seen &= target3?.FullyTracked ?? false;
					}
					return;
				}
				seen = false;
				float num3 = GetSightRange();
				if (MathUtil.ToXZ(sound.Pos - EyePosition).sqrMagnitude <= num3 * num3)
				{
					RaycastResult raycastResult2 = GameTerrain.Instance.RayCast(EyePosition, sound.Pos, 4);
					if (raycastResult2.HitObject == null || raycastResult2.HitObject == tileObject)
					{
						seen = true;
					}
				}
			}
		}
	}

	public override void OnHearSound(AISound sound)
	{
		if (Consciousness >= Consciousness.Unconscious || ((sound.Type == AISoundType.WokeUpAngry || sound.Type == AISoundType.Choke) && sound.Target == this))
		{
			return;
		}
		CheckIfSoundCanBeSeenOrHeard(sound, out var seen, out var heard);
		if ((!seen && !heard) || (!seen && sound.Source.IsSneakingUpOn(this)))
		{
			return;
		}
		if (sound.Type == AISoundType.Assassination)
		{
			if (sound.Target != this && GetBaseObjectType() == BaseObjectType.Human && !Zombie)
			{
				SetRecentActivity(RecentActivityType.Assassination, sound.Target as Character);
			}
			return;
		}
		Target orCreateTarget = GetOrCreateTarget(sound.Source, sound.Pos);
		orCreateTarget.OnSound(this, sound, seen, heard);
		if (Zombie || GetBaseObjectType() != BaseObjectType.Human)
		{
			return;
		}
		AISoundType type = sound.Type;
		if (((uint)(type - 3) <= 1u || type == AISoundType.FoundBody) && sound.Target != this && sound.Target != null)
		{
			GetOrCreateTarget(sound.Target).OnHeardAboutBody(this);
		}
		if (AISound.IsAttackSound(sound.Source, this, sound.IntendedTarget, sound.Type, sound.EquipmentProto))
		{
			if (!ShouldReactToAttackSound(sound.Type, sound.Source, sound.Target, sound.IntendedTarget, orCreateTarget, seen, heard, sound.WasFightToTheDeath))
			{
				return;
			}
			if (!sound.Source.Zombie && Session.Instance.CommunityManager.GetRelationship(Community, sound.Source.GetCommunity()) != CommunityRelationshipType.Hostile)
			{
				Session.Instance.CommunityManager.SetRelationship(Community, sound.Source.GetCommunity(), CommunityRelationshipType.Hostile);
				if (!DirectControlled)
				{
					Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, sound.Source, orCreateTarget.FullyTracked ? SpeechSituation.Attacking : SpeechSituation.HighAlert);
					if (speechForSituation != null)
					{
						Speak(speechForSituation, sound.Source);
					}
				}
			}
			if (!IsControllableByPlayer())
			{
				Community.OnEncounteredThreat(this, sound.Source, orCreateTarget);
			}
		}
		else if (sound.Type == AISoundType.Interesting)
		{
			bool flag = sound.Source != null && sound.Source.SparringType == SparringType.SnowballFight && !sound.Source.DirectControlled;
			if (!HasRecentActivityOfType(RecentActivityType.ThrownAt, AftermathGoal.MaxTimeSinceActivity) && !flag)
			{
				SetRecentActivityAtPos(RecentActivityType.Interesting, sound.Pos);
			}
		}
	}

	public int GetSightRange()
	{
		GetSightRange(out var _, out var fogEnd);
		return fogEnd;
	}

	public virtual void GetSightRange(out int fogStart, out int fogEnd)
	{
		if (Zombie)
		{
			fogStart = (fogEnd = ZombieSightRange);
			if (IsPlayingDead())
			{
				fogStart = (fogEnd = ZombiePlayDeadSightRange);
			}
			return;
		}
		if (Consciousness == Consciousness.Unconscious)
		{
			fogStart = BaseUnconsciousSightRange;
		}
		else
		{
			fogStart = BaseSightRange;
			if (EquippedItem != null)
			{
				fogStart += EquippedItem.GetSightRangeModifierWhenEquipped();
			}
			for (int i = 0; i < Clothes.Length; i++)
			{
				if (Clothes[i] != null)
				{
					fogStart += Clothes[i].GetSightRangeModifierWhenEquipped();
				}
			}
			if (InsideBuilding != null)
			{
				fogStart += InsideBuilding.GetInhabitantSlotDef(this).SightRangeModifier;
			}
		}
		fogStart = Math.Min(31, fogStart);
		fogEnd = Math.Min(31, fogStart + 8);
	}

	public virtual bool CanWalkInRivers()
	{
		return true;
	}

	public virtual bool LikesFood(EquipmentPrototype proto)
	{
		return proto.Nutrition > 0f;
	}

	public virtual bool HasSenseOfSmell()
	{
		return Zombie;
	}

	public virtual bool IsAffectedByTemperature()
	{
		return true;
	}

	public bool IsInSmellRange(float smellRangef, TileObject obj)
	{
		if (smellRangef > 0f)
		{
			Vector2 vector = PosXZ - obj.PosXZ;
			float magnitude = vector.magnitude;
			if (magnitude <= smellRangef)
			{
				Vector2 dirFromAngle = MathUtil.GetDirFromAngle(Session.Instance.Weather.WindAngleDeg * (MathF.PI / 180f));
				if (magnitude < 1f || Vector2.Dot(vector / magnitude, dirFromAngle) >= Mathf.Cos(MathF.PI / 3f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasAnyEnemiesNearby()
	{
		foreach (Target target in Targets)
		{
			if (!target.Object.Deleted && target.Camouflage < 1f && !target.HasAnyFlag((TargetFlags)8390656) && target.Object is Human && IsEnemy(target.Object))
			{
				return true;
			}
		}
		return false;
	}

	private void OnTrapDetected(Character corpse, TileObject trap, bool pitTraps, bool tripwires)
	{
		Community.WarnAboutTraps(pitTraps, tripwires);
		Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, corpse, trap, SpeechSituation.NoticedTraps);
		if (speechForSituation != null)
		{
			Speak(speechForSituation, corpse, trap);
		}
	}

	public void Look(out int sightRange)
	{
		GameTerrain instance = GameTerrain.Instance;
		bool flag = Community != null && Community.CommunityType == CommunityType.Player;
		TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(Position);
		GetSightRange(out var fogStart, out sightRange);
		int num = (HasSenseOfSmell() ? sightRange : 0);
		instance.CharacterMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(sightRange, sightRange), tileCoordForPos + new TerrainCoord(sightRange, sightRange), NearbyCharacters);
		Vector3 eyePos = Position + Appearance.EyeHeight * Vector3.up;
		Vector3 forward = base.Forward;
		float num2 = sightRange;
		float smellRangef = num;
		if ((int)Consciousness < ((!flag) ? 1 : 2))
		{
			foreach (Character nearbyCharacter in NearbyCharacters)
			{
				if (nearbyCharacter == this)
				{
					continue;
				}
				bool hasLineOfSightIgnoringPlantCover;
				bool canHearFootsteps;
				float visibility = nearbyCharacter.GetVisibility(this, eyePos, forward, num2, out hasLineOfSightIgnoringPlantCover, out canHearFootsteps);
				if (visibility > 0f || hasLineOfSightIgnoringPlantCover)
				{
					Target orCreateTarget = GetOrCreateTarget(nearbyCharacter);
					orCreateTarget.MarkVisible(visibility, hasLineOfSightIgnoringPlantCover);
					if (Community != null && Zombie == Community.IsZombieCommunity() && visibility >= 1f && IsEnemy(nearbyCharacter))
					{
						Community.OnEncounteredThreat(this, nearbyCharacter, orCreateTarget);
					}
					if (!Zombie && GetBaseObjectType() == BaseObjectType.Human && !nearbyCharacter.Zombie && Community != null && Community.CommunityType != CommunityType.Player && MathUtil.ToXZ(nearbyCharacter.Pos - PlaceOfDeath).magnitude < 32f)
					{
						if (nearbyCharacter.CauseOfDeath == CauseOfDeath.Trap && !Community.WarnedAboutTraps)
						{
							OnTrapDetected(nearbyCharacter, null, pitTraps: true, tripwires: false);
						}
						if (nearbyCharacter.CauseOfDeath == CauseOfDeath.Tripwire && !Community.WarnedAboutTripwires)
						{
							OnTrapDetected(nearbyCharacter, null, pitTraps: false, tripwires: true);
						}
					}
					if (nearbyCharacter.GetBaseObjectType() == BaseObjectType.Deer && nearbyCharacter.Community != null && nearbyCharacter.Community.SpawnPoint != null && IsControllableByPlayer() && nearbyCharacter.Community.SpawnPoint is DeerSpawnPoint { Discovered: false } deerSpawnPoint && (nearbyCharacter.PosXZ - deerSpawnPoint.PosXZ).sqrMagnitude < 4096f && MathUtil.ToXZ(orCreateTarget.LastKnownPosition - Pos).sqrMagnitude <= (float)(fogStart * fogStart) && CanNarrate())
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, nearbyCharacter, null, SpeechSituation.DiscoveredDeer);
						if (speechForSituation != null)
						{
							Speak(speechForSituation, nearbyCharacter);
						}
						deerSpawnPoint.Discovered = true;
					}
				}
				if (canHearFootsteps)
				{
					GetOrCreateTarget(nearbyCharacter).OnHeardFootsteps(this);
				}
				if (IsInSmellRange(smellRangef, nearbyCharacter) && !nearbyCharacter.IsSneaking())
				{
					GetOrCreateTarget(nearbyCharacter).OnSmelled(this);
					if (HintManager.Instance.Hints[33].CanShowHint() && Hud.Instance.LocalControlledCharacter == nearbyCharacter && nearbyCharacter.DirectControlled && nearbyCharacter.IsCrouching() && nearbyCharacter.IsConscious && nearbyCharacter.IsFacing(PosXZ, MathF.PI / 4f) && nearbyCharacter.FindActiveGoal(GoalType.Conversation) == null && !nearbyCharacter.Community.AreAnyMembersInCombat())
					{
						HintManager.Instance.Hints[33].StartShowing(GameImpl.Translate(HintManager.HINT_StayDownwind));
					}
				}
			}
			foreach (EnterableVehicle movingVehicle in Session.Instance.PropManager.MovingVehicles)
			{
				if (InsideBuilding == movingVehicle)
				{
					continue;
				}
				float num3 = Math.Max(num2, VehicleSoundRange);
				Vector2 vector = MathUtil.ToXZ(movingVehicle.CentreOfMass);
				if (!((vector - PosXZ).sqrMagnitude <= num3 * num3))
				{
					continue;
				}
				bool flag2 = IsFacing(vector, VisionConeDeg * (MathF.PI / 180f));
				Target orCreateTarget2 = GetOrCreateTarget(movingVehicle);
				orCreateTarget2.OnHeardFootsteps(this);
				if (flag2)
				{
					orCreateTarget2.MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
				}
				Character[] inhabitants = movingVehicle.Inhabitants;
				foreach (Character character in inhabitants)
				{
					if (character != null)
					{
						Target orCreateTarget3 = GetOrCreateTarget(character);
						orCreateTarget3.OnHeardFootsteps(this);
						if (flag2)
						{
							orCreateTarget3.MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
						}
					}
				}
			}
		}
		NearbyCharacters.Clear();
		int num4 = Mathf.CeilToInt(TileObject.MaxFireHeatRange) + 8;
		instance.BurningMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num4, num4), tileCoordForPos + new TerrainCoord(num4, num4), NearbyBurningObjects);
		if (Zombie || GetBaseObjectType() != BaseObjectType.Human)
		{
			instance.FoodMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num, num), tileCoordForPos + new TerrainCoord(num, num), NearbyFood);
			foreach (Prop item in NearbyFood)
			{
				if (item != null && LikesFoodProp(item))
				{
					bool hasLineOfSightIgnoringPlantCover2;
					bool canHearFootsteps2;
					float visibility2 = item.GetVisibility(this, eyePos, forward, num2, out hasLineOfSightIgnoringPlantCover2, out canHearFootsteps2);
					if (visibility2 > 0f || hasLineOfSightIgnoringPlantCover2)
					{
						GetOrCreateTarget(item).MarkVisible(visibility2, hasLineOfSightIgnoringPlantCover2);
					}
					if (IsInSmellRange(smellRangef, item))
					{
						GetOrCreateTarget(item).OnSmelled(this);
					}
				}
			}
		}
		NearbyTrapSigns.Clear();
		if (Community == null || (Community.WarnedAboutTraps && Community.WarnedAboutTripwires) || Community.CommunityType == CommunityType.Player || Zombie || GetBaseObjectType() != BaseObjectType.Human || Consciousness >= Consciousness.Unconscious || IsPlayingDead())
		{
			return;
		}
		instance.TrapSignMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(sightRange, sightRange), tileCoordForPos + new TerrainCoord(sightRange, sightRange), NearbyTrapSigns);
		foreach (SingleTileProp nearbyTrapSign in NearbyTrapSigns)
		{
			BaseObjectType baseObjectType = nearbyTrapSign.GetBaseObjectType();
			bool flag3 = baseObjectType == BaseObjectType.PitTrap;
			if (!flag3 || !Community.WarnedAboutTraps)
			{
				bool flag4 = baseObjectType == BaseObjectType.Tripwire;
				if ((!flag4 || !Community.WarnedAboutTripwires) && (nearbyTrapSign.GetVisibility(this, eyePos, forward, num2, out var hasLineOfSightIgnoringPlantCover3, out var _) > 0f || hasLineOfSightIgnoringPlantCover3))
				{
					bool flag5 = baseObjectType == BaseObjectType.TrapsSign;
					OnTrapDetected(null, nearbyTrapSign, flag3 || flag5, flag4 || flag5);
					break;
				}
			}
		}
	}

	public bool LikesFoodProp(TileObject prop)
	{
		if (prop is FoodProp foodProp)
		{
			if (Zombie)
			{
				return Array.IndexOf(EquipmentPrototype.Meat, foodProp.FoodProto) != -1;
			}
			return LikesFood(foodProp.FoodProto);
		}
		if (prop is RabbitTrap rabbitTrap)
		{
			if (GetBaseObjectType() == BaseObjectType.Rabbit)
			{
				return rabbitTrap.IsOpen;
			}
			return false;
		}
		return false;
	}

	public override bool IsBurning()
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Burning)
			{
				return true;
			}
		}
		return false;
	}

	public override void LightFire(Character character)
	{
		if (!IsBurning())
		{
			for (int i = 0; i < Injuries.Count; i++)
			{
				if (Injuries[i].Type == InjuryType.Fire)
				{
					Injury value = Injuries[i];
					value.Burning = true;
					Injuries[i] = value;
				}
			}
			if (!IsBurning())
			{
				AddInjury(new Injury(this, InjuryType.Fire, absorbedByVest: false, InjuryLocation.Torso, InfectionType.None, Bone.Spine1, Vector3.zero, 0f, PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()), character, IsAssailantUnknown(character), intentional: true));
			}
			if (InTerrain)
			{
				AddToBurningMapWho();
			}
		}
		UnityUpdateInjuries();
	}

	public override void AddFuel(Character character, float amount)
	{
		Fuel += amount;
	}

	public override float GetFuel()
	{
		return Fuel;
	}

	public void SwallowInfectedFood(InfectionType infectionType)
	{
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Type == InjuryType.Swallowed)
			{
				Injury value = Injuries[i];
				value.InfectionType = (InfectionType)Math.Max((int)value.InfectionType, (int)infectionType);
				Injuries[i] = value;
				return;
			}
		}
		Injury injury = new Injury(this, InjuryType.Swallowed, absorbedByVest: false, InjuryLocation.Torso, infectionType, Bone.Spine, Vector3.zero, 0f, PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()), null, assailantUnknown: true, intentional: true);
		injury.BandagedSkillLevel = 5;
		AddInjury(injury);
	}

	public override void PredictedUpdate(TimeSpan dt)
	{
		PredictedObjectManager instance = PredictedObjectManager.Instance;
		if (dt.Ticks != 0L)
		{
			Update(instance.PredictedTime);
			instance.PredictedCollisionManager.UpdatedThisFrame.Add(this);
		}
		if (Goal != null)
		{
			Goal.PreUpdate(this, null);
			Goal.Update(this, null);
			Goal.PostUpdate(this, null);
		}
		LastThinkTime = instance.PredictedTime;
	}

	public void Disappear(bool fromGoal)
	{
		if (InsideBuilding != null)
		{
			InsideBuilding.OnCharacterLeave(this, 0, fromBuildingDestroyed: false, fromRagdolled: false);
		}
		StoryManager.Instance.RemoveInterestingSpeaker(this);
		if (CarriedBy != null)
		{
			CarriedBy.DropAuthoritative();
		}
		if (CarryingObject != null)
		{
			DropAuthoritative();
		}
		bool num = Community != null && Community.HasAnyActiveMembers();
		GameTerrain.Instance.AStar.AddChange(AStarChange.RemoveCharacter(this));
		UnregisterFromTriggerZones();
		RemoveFromTerrain();
		Disappeared = true;
		Inventory.DeleteAll(this, carrierBeingDeleted: false);
		if (fromGoal)
		{
			if (GetGoal() != null)
			{
				GetGoal().Finished = true;
			}
		}
		else
		{
			SetGoal(null);
			RemoveAllTargets();
			LeaveAllSquads();
			SanitiseSquad();
		}
		UpdateThinkBucket();
		UnityDelete();
		Session.Instance.CharacterManager.OnCharacterDisappeared(this);
		Session.Instance.CommunityManager.OnCharacterDeletedOrDisappeared(this);
		if (num && !Community.HasAnyActiveMembers())
		{
			Community.OnCommunityHasNoActiveMembers(SecrecyMode.Private);
		}
		UpdateVisibleTilesCache();
	}

	public override void DeleteOrDisappear(bool fromGoal)
	{
		if (!IsAmbient() || CommunityManager.IsAnyoneImportantReferencingObject(this))
		{
			Disappear(fromGoal);
			KeptAroundForReferences = true;
		}
		else
		{
			Delete();
		}
	}

	public void Update(TimeSpan curTime)
	{
		TimeSpan dt = curTime - LastUpdateTime;
		if (dt.Ticks == 0L && NumUpdateFrames > 0 && IsAuthoritative())
		{
			int num = Session.Instance.CharacterManager.Characters.IndexOf(this);
			Debug.Log("Update called twice in a frame for " + GetDisplayNameString() + "? (" + num + "): " + GetGoalDebugString());
		}
		NumUpdateFrames++;
		if (Fade != FadeTarget)
		{
			Fade = ((FadeTime > 0f) ? MathUtil.Delt(Fade, FadeTarget, (float)dt.TotalSeconds / FadeTime) : FadeTarget);
		}
		SimulateClothesDampness(dt);
		if (IsBurning())
		{
			if (!Alive)
			{
				CremationProgression += (float)dt.TotalSeconds / CremationTime;
			}
			Fuel = Math.Max(0f, Fuel - TileObject.FuelBurningRateFlOzPerSecond * (float)dt.TotalSeconds);
			if (CremationProgression >= 1f)
			{
				CremationProgression = 1f;
				Disappear(fromGoal: false);
			}
			if ((Fuel <= 0f && GetFlammability() < Flammability.High) || CremationProgression >= 1f)
			{
				float num2 = 0f;
				int num3 = -1;
				for (int i = 0; i < Injuries.Count; i++)
				{
					if (Injuries[i].Burning)
					{
						if (Injuries[i].Attacker != null)
						{
							float num4 = (float)(curTime - Injuries[i].InjuryTime).TotalSeconds;
							num2 += num4 * BurningDamageRate;
							num3 = i;
						}
						Injuries[i] = Injuries[i].Burnout();
					}
				}
				if (num3 != -1 && !Zombie && IsAuthoritative())
				{
					int skillLevelWithEffects = GetSkillLevelWithEffects(SkillType.Constitution);
					num2 *= Mathf.Lerp(BloodLossFactorMinConstitution, BloodLossFactorMaxConstitution, (float)skillLevelWithEffects / 5f);
					TriggerHurtMemory(Injuries[num3].Attacker, Injuries[num3].AssailantUnknown, num2, Injuries[num3].Secrecy, Injuries[num3].FromSparring, Injuries[num3].Intentional);
				}
				RemoveFromBurningMapWho();
				UnityUpdateInjuries();
			}
		}
		if (Alive)
		{
			UpdateAlive(curTime, dt);
		}
		else
		{
			if (SquadId != 0 && IsAuthoritative())
			{
				SanitiseSquad();
			}
			if (InfectionProgression > 0f && !Zombie && SkinnedAmount < 1f && GetBaseObjectType() == BaseObjectType.Human)
			{
				InfectionProgression += ZombificationRate * (float)dt.TotalSeconds;
				if (InfectionProgression >= 1f && curTime - TimeOfDeath >= MinZombificationTime)
				{
					bool flag = false;
					bool flag2 = false;
					InfectionType infectionType = InfectionType.None;
					for (int j = 0; j < Injuries.Count; j++)
					{
						infectionType = (InfectionType)Math.Max((int)Injuries[j].InfectionType, (int)infectionType);
						if (Injuries[j].InfectionType == InfectionType.White || Injuries[j].InfectionType == InfectionType.Invisible)
						{
							if (Injuries[j].Type == InjuryType.Swallowed)
							{
								flag2 = true;
							}
							else if (Injuries[j].Type != InjuryType.ZombieBite && Injuries[j].Attacker != null && Injuries[j].Attacker.Community != null && Injuries[j].Attacker.Community.CommunityType == CommunityType.Player)
							{
								flag = true;
							}
						}
					}
					Zombify(infectionType);
					if (flag2 && IsAuthoritative())
					{
						AchievementsManager.Instance.UnlockAchievement(Achievement.InfectWithFood);
					}
					else if (flag && IsAuthoritative())
					{
						AchievementsManager.Instance.IncrementAchievementStat(Achievement.InfectWithWeapon, 1);
					}
				}
			}
		}
		if (Speaking != null)
		{
			if (Speaking.Anim != ActionAnim.None)
			{
				if (CurrentSpeechAnimState == SpeechAnimState.Started && CurrentActionAnim != Speaking.Anim)
				{
					CurrentSpeechAnimState = SpeechAnimState.NotStarted;
				}
				if (CurrentSpeechAnimState == SpeechAnimState.NotStarted)
				{
					CurrentSpeechAnimState = (TryStartActionAnim(Speaking.Anim, null, canInterruptEqualPriority: true) ? SpeechAnimState.Started : SpeechAnimState.NotStarted);
				}
				if (CurrentSpeechAnimState == SpeechAnimState.Started && IsActionAnimFinished())
				{
					CurrentSpeechAnimState = SpeechAnimState.Finished;
				}
			}
			if (curTime - SpeechStartTime >= StoryManager.GetSpeechTime(SpeakingTextEnglish) && (Speaking.Anim == ActionAnim.None || CurrentSpeechAnimState == SpeechAnimState.Finished))
			{
				OnSpeechFinished(interrupted: false);
			}
		}
		if (TeleportOffSlopeRequest != null && CurrentActionAnim != ActionAnim.Slide)
		{
			TeleportOffSlopeRequest.CancelRequest();
			TeleportOffSlopeRequest = null;
		}
		FramesPerUpdate = Math.Max(1, Mathf.RoundToInt((float)dt.TotalSeconds / (1f / 60f)));
		LastUpdateTime = curTime;
		if (IsAuthoritative())
		{
			UpdateVisibleTilesCache();
			UpdateThinkBucket();
		}
	}

	public TileObject GetCurrentTarget()
	{
		if (DirectControlled)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe != null)
			{
				return playerControllingMe.TargetObject;
			}
		}
		else if (Goal != null)
		{
			return Goal.GetCurrentTarget(this);
		}
		return null;
	}

	public TargettableBodyLocation GetCurrentTargetBodyLocation()
	{
		if (DirectControlled)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe != null)
			{
				return playerControllingMe.TargetBodyLocationToAimFor;
			}
		}
		else if (Goal != null)
		{
			return Goal.GetCurrentTargetBodyLocation(this);
		}
		return TargettableBodyLocation.Torso;
	}

	public Vector3 GetTargetAimPos(Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, bool deterministic)
	{
		InjuryLocation injuryLocation;
		return GetTargetAimPos(target, targetBodyLocation, targetPos, deterministic, out injuryLocation);
	}

	public Vector3 GetTargetAimPos(Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, bool deterministic, out InjuryLocation injuryLocation)
	{
		if (target == null && targetPos != Vector3.zero)
		{
			injuryLocation = InjuryLocation.Torso;
			return targetPos;
		}
		if (target == null || target.Object == null || target.Object.Deleted)
		{
			injuryLocation = InjuryLocation.Torso;
			return Position + base.Forward * 6f + new Vector3(0f, GunHeight, 0f);
		}
		return GetTargetAimPos(target.Object, target.LastKnownPosition, target.Visible, targetBodyLocation, deterministic, out injuryLocation);
	}

	public Vector3 GetTargetAimPos(TileObject targetObj, Vector3 lastKnownPosition, bool visible, TargettableBodyLocation targetBodyLocation, bool deterministic, out InjuryLocation injuryLocation)
	{
		injuryLocation = InjuryLocation.Torso;
		if (targetObj is Character character)
		{
			Bone bone = Bone.Spine;
			float y = character.GunHeight;
			if (EquippedItem is Throwable throwable && throwable.GetDamageRadius() > 0f)
			{
				if (character.IsGuarding())
				{
					return lastKnownPosition + new Vector3(0f, y, 0f);
				}
				return lastKnownPosition;
			}
			switch (targetBodyLocation)
			{
			case TargettableBodyLocation.Head:
				injuryLocation = InjuryLocation.Head;
				break;
			case TargettableBodyLocation.Legs:
				injuryLocation = ((Vector2.Dot(PosXZ - character.PosXZ, MathUtil.ToXZ(character.World.Right())) >= 0f) ? InjuryLocation.RightLeg : InjuryLocation.LeftLeg);
				break;
			case TargettableBodyLocation.Count:
				injuryLocation = InjuryLocation.Count;
				break;
			}
			switch (injuryLocation)
			{
			case InjuryLocation.Head:
				y = character.EyeHeight;
				bone = Bone.Head;
				break;
			case InjuryLocation.LeftLeg:
				y = character.Appearance.KneeHeight;
				bone = Bone.LeftLeg;
				break;
			case InjuryLocation.RightLeg:
				y = character.Appearance.KneeHeight;
				bone = Bone.RightLeg;
				break;
			case InjuryLocation.Torso:
				y = character.GunHeight;
				bone = Bone.Spine;
				break;
			case InjuryLocation.LeftArm:
				y = character.GunHeight;
				bone = Bone.LeftForearm;
				break;
			case InjuryLocation.RightArm:
				y = character.GunHeight;
				bone = Bone.RightForearm;
				break;
			case InjuryLocation.Count:
				y = 0f;
				bone = Bone.Root;
				break;
			}
			if (!deterministic && visible && character.IsUnityObjectActive())
			{
				return character.GetUnityBoneTransform(bone).Translation();
			}
			return lastKnownPosition + new Vector3(0f, y, 0f);
		}
		Bounds boundingBox = targetObj.GetBoundingBox();
		if (targetObj.CoverType == CoverType.WaistHigh && !deterministic)
		{
			TerrainCoord nearestTileTo = targetObj.GetNearestTileTo(Tile);
			return GameTerrain.Instance.GetTileCentrePos(nearestTileTo) + new Vector3(0f, Appearance.GunHeight, 0f);
		}
		if (targetObj is FoodProp foodProp)
		{
			return foodProp.Position;
		}
		if (targetObj is Prop)
		{
			TerrainCoord nearestTileTo2 = targetObj.GetNearestTileTo(Tile);
			Vector3 result = GameTerrain.Instance.GetTileCentrePos(nearestTileTo2) + new Vector3(0f, GunHeight, 0f);
			float num = Math.Min(0.25f, (boundingBox.max.y - boundingBox.min.y) * 0.5f);
			result.y = Mathf.Clamp(result.y, boundingBox.min.y + num, boundingBox.max.y - num);
			return result;
		}
		return lastKnownPosition + new Vector3(0f, (boundingBox.max.y - boundingBox.min.y) * 0.5f, 0f);
	}

	public bool IsTargetBlocked(Target target, Vector3 targetPos, float throwAngle, float throwSpeed)
	{
		Character hitCharacter;
		return IsTargetBlocked(target, targetPos, throwAngle, throwSpeed, out hitCharacter);
	}

	public bool IsTargetBlocked(Target target, Vector3 targetPos, float throwAngle, float throwSpeed, out Character hitCharacter)
	{
		TileObject tileObject = target?.Object;
		InjuryLocation injuryLocation;
		Vector3 targetAimPos = GetTargetAimPos(target, TargettableBodyLocation.Torso, targetPos, deterministic: true, out injuryLocation);
		int num = 134697;
		RaycastResult raycastResult;
		if (EquippedItem is Throwable)
		{
			Vector3 throwPosition = ThrowPosition;
			Vector2 horizDir = MathUtil.SafeNormalize(MathUtil.ToXZ(targetAimPos - throwPosition), MathUtil.ToXZ(base.Forward));
			raycastResult = BaseThrownProjectile.ParabolicRayCast(this, tileObject, throwPosition, horizDir, throwAngle, throwSpeed, num, IsPredicted(), debugDraw: false, out var _);
			hitCharacter = raycastResult.HitObject as Character;
			if (target == null && targetPos != Vector3.zero && (raycastResult.GetHitPosition() - targetAimPos).magnitude < 0.1f)
			{
				return false;
			}
		}
		else
		{
			Vector3 gunPosition = GunPosition;
			if (InsideBuilding != null && InsideBuilding.IsGuardPost())
			{
				gunPosition += base.Forward * RangedAttack.WatchTowerOffsetHack;
			}
			raycastResult = GameTerrain.Instance.RayCast(gunPosition, targetAimPos, num, InsideBuilding, this, tileObject, IsPredicted());
			hitCharacter = raycastResult.HitObject as Character;
			if (target == null && targetPos != Vector3.zero && raycastResult.HitDist >= (targetAimPos - gunPosition).magnitude - 0.1f)
			{
				return false;
			}
		}
		if (raycastResult.HitObject != null)
		{
			return raycastResult.HitObject != tileObject;
		}
		return false;
	}

	public float Get2DDistSqToTargetAimPos(Target target, Vector3 targetPos)
	{
		return MathUtil.ToXZ(GetTargetAimPos(target, TargettableBodyLocation.Torso, targetPos, deterministic: true) - Position).sqrMagnitude;
	}

	public float Get2DDistToTargetAimPos(Target target, Vector3 targetPos)
	{
		return (float)Math.Sqrt(Get2DDistSqToTargetAimPos(target, targetPos));
	}

	public float Get2DDistToTargetLastKnownPos(Target target)
	{
		return MathUtil.ToXZ(target.LastKnownPosition - Pos).magnitude;
	}

	public float CalcAimUpDownAngle()
	{
		Target target = ((DirectControlledTarget != null) ? DirectControlledTarget : GoalTarget);
		TargettableBodyLocation currentTargetBodyLocation = GetCurrentTargetBodyLocation();
		Vector3 targetPos = Vector3.zero;
		if (DirectControlled)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe != null)
			{
				targetPos = playerControllingMe.TargetPos;
			}
		}
		Vector3 vector = Position + new Vector3(0f, Appearance.GunHeight, 0f);
		targetPos = GetTargetAimPos(target, currentTargetBodyLocation, targetPos, deterministic: false);
		Vector3 vector2 = targetPos - vector;
		float magnitude = vector2.magnitude;
		return Mathf.Clamp((magnitude > 0.001f) ? ((float)Math.Asin(vector2.y / magnitude)) : 0f, MaxAimDownAngle, MaxAimUpAngle);
	}

	public float CalcDesiredFacingAngleFromGoalTarget()
	{
		Vector3 targetPos = Vector3.zero;
		if (DirectControlled)
		{
			PlayerRecord playerControllingMe = GetPlayerControllingMe();
			if (playerControllingMe != null)
			{
				targetPos = playerControllingMe.TargetPos;
			}
		}
		if (GoalTarget == null)
		{
			return DesiredFacingAngle;
		}
		return MathUtil.GetAngleTo(Position, GetTargetAimPos(GoalTarget, TargettableBodyLocation.Torso, targetPos, deterministic: true), DesiredFacingAngle);
	}

	private bool IsTouchingSlope()
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord minTile = GetMinTile();
		TerrainCoord maxTile = GetMaxTile();
		for (int i = minTile.x; i <= maxTile.x; i++)
		{
			for (int j = minTile.y; j <= maxTile.y; j++)
			{
				if (instance.IsSlope(i, j))
				{
					TerrainCoord tile = new TerrainCoord(i, j);
					Vector2 tileCentreXZ = instance.GetTileCentreXZ(tile);
					if (MathUtil.DoesCircleIntersectRect(PosXZ, Radius, tileCentreXZ - new Vector2(0.5f, 0.5f), tileCentreXZ + new Vector2(0.5f, 0.5f), out var _, out var _))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private float CalcInfectionProgressionRate()
	{
		float num = 0f;
		int constitution = -1;
		for (int i = 0; i < Injuries.Count; i++)
		{
			num = Math.Max(num, Injuries[i].GetInfectionProgressionRate(this, ref constitution));
		}
		return num;
	}

	public void PowerNap()
	{
		float num = Math.Min(SleepDeprivation, Sun.DayLengthSecs * 2f / SleepRecoverRate);
		Thirst = Math.Min(Math.Max(Thirst, ThirstCriticalTime) - num, Thirst);
		Hunger = Math.Min(Math.Max(Hunger, HungerExtraCriticalTime) - num, Hunger);
		InfectionProgression = Math.Min(Math.Max(InfectionProgression, InfectionProgressionCriticalLevel) - num * CalcInfectionProgressionRate(), InfectionProgression);
		SimulateSurvivalFactors(Session.Instance.PlayTime, num);
	}

	protected virtual void SimulateSurvivalFactors(TimeSpan curTime, float dts)
	{
		if (BloodLoss >= 0.95f && WillActivateInvisibleStrain())
		{
			ActivateInvisibleStrain();
		}
		float num = 0f;
		float num2 = 0f;
		float num3 = 5f;
		bool flag = false;
		bool flag2 = false;
		Character character = null;
		bool assailantUnknown = false;
		bool intentional = true;
		SecrecyMode secret = SecrecyMode.Public;
		SparringType sparringType = SparringType.None;
		int constitution = -1;
		for (int i = 0; i < Injuries.Count; i++)
		{
			if (Injuries[i].Burning)
			{
				num += (Zombie ? ZombieBurningDamageRate : BurningDamageRate);
				if (Injuries[i].Attacker != null && character == null)
				{
					character = Injuries[i].Attacker;
					assailantUnknown = Injuries[i].AssailantUnknown;
					intentional = Injuries[i].Intentional;
				}
				flag = true;
			}
			else
			{
				if (Zombie)
				{
					continue;
				}
				num2 = Math.Max(num2, Injuries[i].GetInfectionProgressionRate(this, ref constitution));
				num3 = Math.Min(num3, Injuries[i].BandagedSkillLevel);
				if (Injuries[i].Bandaged)
				{
					if (curTime >= Injuries[i].BandagedHealTime && Injuries[i].InfectionType == InfectionType.None && BloodLoss <= 0f)
					{
						RemoveInjuryAt(i);
						i--;
						UnityUpdateDecals();
						UnityUpdateInjuries();
					}
				}
				else
				{
					if (Injuries[i].AbsorbedByVest)
					{
						continue;
					}
					flag = true;
					if (Injuries[i].ShouldBleed())
					{
						num += BloodLossRate;
						flag2 |= Injuries[i].Type == InjuryType.ZombieBite;
						if (Injuries[i].Attacker != null && character == null)
						{
							character = Injuries[i].Attacker;
							assailantUnknown = Injuries[i].AssailantUnknown;
							intentional = Injuries[i].Intentional;
							secret = Injuries[i].Secrecy;
							sparringType = Injuries[i].FromSparring;
						}
					}
				}
			}
		}
		if (!flag)
		{
			if (BloodLoss > 0f && GetPregnancyProgression() < 1f)
			{
				if (!IsControllableByPlayer())
				{
					num3 = Math.Max(num3, 0.1f);
				}
				BloodLoss = Math.Max(BloodLoss - BloodLossRecoverRate * num3 * dts, 0f);
			}
		}
		else
		{
			if (this == Hud.Instance.LocalControlledCharacter)
			{
				Hud.Instance.VignetteAmount = Math.Max(Hud.Instance.VignetteAmount, Hud.DamageVignetteAmountWhenLosingBlood);
			}
			if (num > 0f)
			{
				if (constitution == -1)
				{
					constitution = GetSkillLevelWithEffects(SkillType.Constitution);
				}
				float bloodLoss = num * Mathf.Lerp(BloodLossFactorMinConstitution, BloodLossFactorMaxConstitution, (float)constitution / 5f) * dts;
				ApplyBloodLoss2(bloodLoss, 0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, flag2 ? CauseOfDeath.Zombie : CauseOfDeath.Other, character, constitution, secret);
				if (!Alive)
				{
					if (character != null && IsAuthoritative())
					{
						TriggerKilledMemory(character, assailantUnknown, SecrecyMode.Public, sparringType, null, assassinate: false, intentional);
					}
					return;
				}
			}
		}
		if (!Zombie)
		{
			InfectionProgression += num2 * dts;
			if (InfectionProgression >= 1f)
			{
				if (WillActivateInvisibleStrain())
				{
					ActivateInvisibleStrain();
					return;
				}
				InfectionProgression = 1f;
				OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Zombie, null, SecrecyMode.Public);
				if (IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, this, 1f, secret: false);
				}
				return;
			}
			if (InfectionProgression > 0f && HasUnityObject())
			{
				UnityUpdateSkinColor();
			}
			float num4 = ((Appearance.Gender == GenderType.Male) ? 0.015f : 0.017f);
			float num5 = dts / (Sun.DayLengthSecs / 24f);
			BloodAlcoholConcentration = Math.Max(0f, BloodAlcoholConcentration - num5 * num4);
		}
		SedativeEffect = Math.Max(0f, SedativeEffect - dts);
		if (!InCombat)
		{
			Excitement = Math.Max(0f, Excitement - dts / 600f);
		}
		bool flag3 = !Zombie && !DontSimulateSurvivalFactorsUntilJoinCommunity && !DontSimulateSurvivalFactorsUntilDiscovered && ScriptedGoalMarker == null && (!CanFollowPlayer || IsControllableByPlayer() || SquadLeader == null || !SquadLeader.IsControllableByPlayer());
		if (flag3)
		{
			Hunger += dts;
			if (Hunger >= HungerDieTime)
			{
				Hunger = HungerDieTime;
				OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
				if (IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, this, 1f, secret: false);
				}
				return;
			}
			Thirst += dts;
			if (Thirst >= ThirstDieTime)
			{
				Thirst = ThirstDieTime;
				OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
				if (IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, this, 1f, secret: false);
				}
				return;
			}
		}
		if (flag3 && IsAffectedByTemperature())
		{
			float outsideTemperature = GetOutsideTemperature();
			float num6 = GetTotalClothingInsulationIncludingDampness();
			TemperatureInsulationDelta = outsideTemperature + num6 * ClothingInsulationTemperature - NakedComfortableTemperatureInCelsius;
			if (!IsOutdoors())
			{
				float val = ((InsideBuilding.Prototype.EnterableBySpecies == BaseObjectType.Chicken) ? MinChickenCoopTempDelta : ((Community != null && Community.IsAmbientCommunity()) ? MinLooterIndoorTempDelta : MinIndoorTempDelta));
				TemperatureInsulationDelta = Math.Max(val, TemperatureInsulationDelta);
			}
			BodyTemperatureInCelsius = Mathf.Clamp(BodyTemperatureInCelsius + TemperatureInsulationDelta * HypothermiaRate * dts, BodyTemperatureInCelsiusDeath, BodyTemperatureInCelsiusNormal);
			if (GetBodyTemperatureIncludingFeverInCelsius() <= BodyTemperatureInCelsiusDeath)
			{
				OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
				if (IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, this, 1f, secret: false);
				}
				return;
			}
		}
		Consciousness consciousness = (ShouldBeUnconscious() ? Consciousness.Unconscious : (CanSleep() ? Consciousness.Sleeping : Consciousness.Conscious));
		SetConsciousness(consciousness);
		if (IsSleepingOrUnconscious())
		{
			SleepDeprivation = Math.Max(0f, SleepDeprivation - dts * SleepRecoverRate);
		}
		else if (flag3 && !IsAmbient())
		{
			SleepDeprivation += dts;
			if (SleepDeprivation >= SleepDeprivationDieTime)
			{
				SleepDeprivation = SleepDeprivationDieTime;
				OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
				if (IsAuthoritative())
				{
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, this, 1f, secret: false);
				}
				return;
			}
		}
		if (DownTime > 0f && IsTakingDownTime())
		{
			DownTime = Math.Max(0f, DownTime - dts);
			if (DownTime == 0f)
			{
				ResumeAllRoles();
				EncouragementTimeout = Session.Instance.PlayTime + TimeSpan.FromSeconds(7f * Sun.DayLengthSecs);
			}
		}
		if (Community == Session.Instance.CommunityManager.PlayerCommunity && curTime - LastSkillAtrophyTime >= Sun.DayLength)
		{
			LastSkillAtrophyTime += Sun.DayLength;
		}
	}

	private bool IsTakingDownTime()
	{
		if (InCombat || UnderAttackRefCount > 0)
		{
			return false;
		}
		if (Goal is SurvivorGoal survivorGoal && survivorGoal.ObeyLeaderGoal == survivorGoal.SubGoal && survivorGoal.ObeyLeaderGoal != null && survivorGoal.ObeyLeaderGoal.GetSource() == ObeyLeaderGoal.SourceType.Player)
		{
			return false;
		}
		return true;
	}

	public float CalcAverageTemperatureInsulationDelta()
	{
		float outsideTemp = Session.Instance.Weather.CalcAverageTemperatureInCelsius();
		float clothingInsulation = GetTotalClothingInsulation();
		return CalcAverageTemperatureInsulationDelta(outsideTemp, clothingInsulation);
	}

	public float CalcAverageTemperatureInsulationDelta(float outsideTemp, float clothingInsulation)
	{
		float num = outsideTemp + clothingInsulation * ClothingInsulationTemperature;
		if (num > NakedComfortableTemperatureInCelsius)
		{
			return Math.Max(0f, num - (float)MinClothingInsulation * ClothingInsulationTemperature - NakedComfortableTemperatureInCelsius);
		}
		return num - NakedComfortableTemperatureInCelsius;
	}

	public static int CalcRequiredInsulationForTemperature(float outsideTemp)
	{
		return Mathf.CeilToInt(Math.Max(0f, (NakedComfortableTemperatureInCelsius - outsideTemp) / ClothingInsulationTemperature));
	}

	public float GetOutsideTemperature()
	{
		float temperatureInCelsius = Session.Instance.Weather.TemperatureInCelsius;
		float num = 0f;
		foreach (TileObject nearbyBurningObject in NearbyBurningObjects)
		{
			num = Math.Max(num, nearbyBurningObject.GetFireEffectOnCharacter(this));
		}
		return Mathf.Lerp(temperatureInCelsius, FireTemperatureInCelsius, num);
	}

	public Campfire GetNearestBurningCampfire(float maxRange)
	{
		Campfire result = null;
		float num = maxRange;
		foreach (TileObject nearbyBurningObject in NearbyBurningObjects)
		{
			if (nearbyBurningObject is Campfire campfire)
			{
				float magnitude = (nearbyBurningObject.PosXZ - PosXZ).magnitude;
				if (magnitude < num)
				{
					num = magnitude;
					result = campfire;
				}
			}
		}
		return result;
	}

	public bool IsNaked()
	{
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	public virtual int GetFurInsulation()
	{
		return 0;
	}

	public int GetTotalClothingInsulation()
	{
		int num = GetFurInsulation();
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null)
			{
				num += Clothes[i].GetInsulation();
			}
		}
		return num;
	}

	public int GetTotalClothingInsulationIncludingDampness()
	{
		int num = GetFurInsulation();
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null)
			{
				num += Clothes[i].GetInsulationIncludingDampness();
			}
		}
		return num;
	}

	public GenderType GetClothingCountAndGender(Language language, out int count)
	{
		bool flag = false;
		count = 0;
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null && Clothes[i].GetInsulation() > 0)
			{
				GenderType gender = Clothes[i].GetGender(language);
				flag = flag || gender == GenderType.Male;
				count++;
			}
		}
		if (!flag && count != 0)
		{
			return GenderType.Female;
		}
		return GenderType.Male;
	}

	private void SimulateClothesDampness(TimeSpan dt)
	{
		if (GetBaseObjectType() != BaseObjectType.Human || IsPredicted())
		{
			return;
		}
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		Weather weather = instance.Weather;
		float num = (float)dt.TotalSeconds;
		float num2 = 0f;
		float num3 = 0f;
		if (IsOutdoors())
		{
			float num4 = weather.PrecipitationAmount;
			if (num4 < 0.1f)
			{
				num4 = 0f;
			}
			if (IsGuarding())
			{
				num4 = 0f;
			}
			num2 = num4 * Mathf.Lerp(SnowDampeningRate, RainDampeningRate, Mathf.Clamp01((weather.TemperatureInCelsius - 0f) / 5f));
			if (num2 <= 0f)
			{
				num2 = (0f - (1f - weather.Cloudiness) * Sun.GetSunIntensity(instance.DaysSinceStart)) * EvaporationRate;
				float num5 = 0f;
				foreach (TileObject nearbyBurningObject in NearbyBurningObjects)
				{
					num5 = Math.Max(num5, nearbyBurningObject.GetFireEffectOnCharacter(this));
				}
				num2 = Mathf.Lerp(num2, 0f - FireEvaporationRate, num5);
			}
			TerrainCoord tile = Tile;
			if (instance2.IsTileRiver(tile.x, tile.y))
			{
				float H;
				if (IsProne())
				{
					num3 = float.MaxValue;
				}
				else if (instance2.GetAmountOnPath(PosXZ, out H, wantRiver: true) < 1f)
				{
					num3 = H + GameTerrain.RiverHeightOffset - Pos.y;
				}
			}
		}
		else
		{
			num2 = 0f - IndoorEvaporationRate;
		}
		float overallScale = Appearance.OverallScale;
		for (int i = 0; i < Clothes.Length; i++)
		{
			if (Clothes[i] != null && Clothes[i].GetInsulation() > 0)
			{
				float val = Mathf.Clamp01((num3 - MinHeightForClothingType[i] * overallScale) / (MaxHeightForClothingType[i] * overallScale - MinHeightForClothingType[i] * overallScale));
				Clothes[i].SetDampness(Math.Max(val, Mathf.Clamp01(Clothes[i].GetDampness() + num2 * num)));
			}
		}
	}

	private TerrainCoord FindSafePlaceToTeleportToFromSlope(TerrainCoord startTile)
	{
		if (IsPredicted())
		{
			return TerrainCoord.Invalid;
		}
		if (TeleportOffSlopeRequest == null)
		{
			TeleportOffSlopeRequest = new AStarRequester();
			TeleportOffSlopeRequest.StartTeleportOffSlopeRequest(startTile, this, AStarPriority.Unknown, respectMovementZone: false);
		}
		if (TeleportOffSlopeRequest != null)
		{
			switch (TeleportOffSlopeRequest.Result)
			{
			case AStarResult.Success:
			{
				TerrainCoord result = ((TeleportOffSlopeRequest.Route.Count > 0) ? TeleportOffSlopeRequest.Route[TeleportOffSlopeRequest.Route.Count - 1] : Tile);
				TeleportOffSlopeRequest = null;
				return result;
			}
			case AStarResult.Fail:
				TeleportOffSlopeRequest = null;
				return TerrainCoord.Invalid;
			}
		}
		return TerrainCoord.Invalid;
	}

	public bool ShouldDropCarriedObject()
	{
		if (IsAiming())
		{
			return true;
		}
		if (CarryingObject is Character { IsAwake: not false } character && character.GetBaseObjectType() == BaseObjectType.Human)
		{
			return true;
		}
		return false;
	}

	private void UpdateAlive(TimeSpan curTime, TimeSpan dt)
	{
		if (WantKilledByPitTrap)
		{
			OnDie(0f, Vector3.zero, -Vector3.up * TrapDownForce, Bone.Spine, Vector3.zero, CauseOfDeath.Trap, null, SecrecyMode.Public);
			if (IsAuthoritative() && IsDeathSignificant(null))
			{
				Memory.OnMemorableEvent(MemoryPrototype.Killed, null, this, 1f, secret: false);
			}
			WantKilledByPitTrap = false;
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		float num = (float)dt.TotalSeconds;
		TerrainCoord tile = Tile;
		if (DontSimulateSurvivalFactorsUntilDiscovered && instance.FogOfWar.IsTileVisible(tile.x, tile.y))
		{
			DontSimulateSurvivalFactorsUntilDiscovered = false;
		}
		if (!Disappeared)
		{
			SimulateSurvivalFactors(curTime, num);
		}
		if (!Alive)
		{
			return;
		}
		if (PickpocketDetection > 0f)
		{
			PickpocketDetection -= num * PickpocketForgetSpeed;
			if (PickpocketDetection <= 0f)
			{
				PickpocketDetection = 0f;
				if (PickpocketedItems != null)
				{
					PickpocketedItems.Clear();
					PickpocketedItems = null;
				}
			}
		}
		if (GetActionPriority(CurrentActionAnim) <= ActionPriority.Idle)
		{
			if (CurrentAnimState == AnimState.Animation && MovementSpeed == 0f && MovementType == MovementType.None && !IsAiming() && !IsCrouching() && !IsSitting() && !Zombie)
			{
				if (BloodAlcoholConcentration >= BACEuphoria && BloodAlcoholConcentration < BACConfusion)
				{
					TryStartActionAnimIfNotAlreadyPlaying(ActionAnim.Drunk);
				}
				else if (BloodAlcoholConcentration >= BACConfusion)
				{
					TryStartActionAnimIfNotAlreadyPlaying(ActionAnim.VeryDrunk);
				}
				else if (CurrentActionAnim == ActionAnim.Drunk || CurrentActionAnim == ActionAnim.VeryDrunk)
				{
					StopActionAnim(CurrentActionAnim);
				}
			}
			else if (CurrentActionAnim != ActionAnim.None)
			{
				StopActionAnim(CurrentActionAnim);
			}
		}
		if (CurrentActionAnim != ActionAnim.Slide && InsideBuilding == null && instance.IsSlope(tile.x, tile.y) && GetBaseObjectType() == BaseObjectType.Human)
		{
			TryStartActionAnim(ActionAnim.Slide, null, canInterruptEqualPriority: false);
		}
		switch (CurrentActionAnim)
		{
		case ActionAnim.BittenFrontStruggle:
		case ActionAnim.BittenFrontFree:
		case ActionAnim.BittenPinnedStruggle:
		case ActionAnim.BittenPinnedFree:
			if (InteractionObject is Character character2)
			{
				DesiredFacingAngle = character2.DesiredFacingAngle + MathF.PI;
			}
			break;
		case ActionAnim.BittenRearStruggle:
		case ActionAnim.BittenRearFree:
		case ActionAnim.ChokeHoldStruggle:
		case ActionAnim.ChokeHoldFree:
		case ActionAnim.FemaleMating:
		case ActionAnim.RestrainedFree:
			if (InteractionObject is Character character3)
			{
				DesiredFacingAngle = character3.DesiredFacingAngle;
			}
			break;
		case ActionAnim.RestrainedStruggle:
			if (SparringPartner != null)
			{
				SetFacingAngle(MathUtil.GetAngleFromDir(SparringPartner.PosXZ - PosXZ, DesiredFacingAngle));
			}
			break;
		case ActionAnim.ChokeHoldLoop:
		case ActionAnim.SlitThroatLoop:
		case ActionAnim.RestrainLoop:
			if (InteractionObject is Character character)
			{
				if (CurrentActionAnim == ActionAnim.RestrainLoop && character.SparringPartner != null)
				{
					SetFacingAngle(MathUtil.GetAngleTo(character.Pos, character.SparringPartner.Pos, character.FacingAngle));
					Vector2 vector = character.PosXZ + MathUtil.SafeNormalize(character.PosXZ - character.SparringPartner.PosXZ, MathUtil.ToXZ(-character.Forward)) * LoopChokeHold.ChokeDist;
					SetPosition(vector.x, vector.y);
				}
				PlayerRecord playerControllingMe = GetPlayerControllingMe();
				if (playerControllingMe == null || playerControllingMe.FlyMode || FindActiveGoal(GoalType.InvisibleStrainSpreadGoal) != null)
				{
					int heldFrames = Mathf.CeilToInt(num / (1f / 60f));
					ApplyChokeAction(null, held: true, heldFrames);
				}
				float num2 = 0f;
				float num3 = Mathf.Lerp(t: (!character.Zombie) ? ((float)character.GetSkillLevelWithEffects(SkillType.Strength) / 5f) : ((float)character.Infection / 5f), a: (CurrentActionAnim == ActionAnim.RestrainLoop) ? RestrainedStrugglePowerMin : ChokeStrugglePowerMin, b: (CurrentActionAnim == ActionAnim.RestrainLoop) ? RestrainedStrugglePowerMax : ChokeStrugglePowerMax) * num;
				ChokePower = Math.Max(0f, ChokePower - num3);
				if (ChokePower <= 0f && IsPredicted() == character.IsPredicted())
				{
					character.OnChokeHoldCancelled(this);
				}
			}
			break;
		case ActionAnim.Slide:
		{
			Vector2 vector2 = MathUtil.ToXZ(instance.GetNormalAtPos(Position.x, Position.z));
			if (!IsTouchingSlope() || InsideBuilding != null || vector2.sqrMagnitude < 1E-06f)
			{
				TryStartActionAnim(ActionAnim.SlideRecover, null);
				break;
			}
			AddVelocityXZ(vector2 * SlipperySlopeSpeedFactor * num, setFollowMeDir: true);
			if (num > 0.001f)
			{
				VelocityXZ = Vector2.ClampMagnitude(VelocityXZ, GetRunSpeed() * num);
			}
			MovementAngle = (DesiredFacingAngle = MathUtil.GetAngleFromDir(vector2, FacingAngle));
			if (Mathf.Abs(OldPosition.y - Position.y) <= 0.001f)
			{
				TerrainCoord terrainCoord = FindSafePlaceToTeleportToFromSlope(Tile);
				if (terrainCoord != TerrainCoord.Invalid)
				{
					Vector2 tileCentreXZ = instance.GetTileCentreXZ(terrainCoord);
					SetPosition(tileCentreXZ.x, tileCentreXZ.y);
					TryStartActionAnim(ActionAnim.SlideRecover, null);
				}
			}
			break;
		}
		case ActionAnim.Vault:
			if (MovementType != MovementType.None && MovementMode == MovementMode.Pathfinding)
			{
				Vector2 lhs = instance.GetTileCentreXZ(Route[0]) - PosXZ;
				if (Vector2.Dot(lhs, MathUtil.ToXZ(base.Forward)) < 0f)
				{
					PopNextRouteNode();
					if (Route.Count > 0)
					{
						lhs = instance.GetTileCentreXZ(Route[0]) - PosXZ;
						FacingAngle = (DesiredFacingAngle = MathUtil.GetAngleFromDir(lhs, DesiredFacingAngle));
					}
					else
					{
						TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(tile.x, tile.y);
						if (fixedObjectOnTile == null || fixedObjectOnTile.CoverType != CoverType.WaistHigh)
						{
							StopActionAnim(ActionAnim.Vault);
							break;
						}
					}
				}
			}
			if (GetActionAnimPlayedFrac() >= AnimWrapper.VaultLoopEndFrac)
			{
				Vector2 dirFromAngle = MathUtil.GetDirFromAngle(DesiredFacingAngle);
				TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(PosXZ + dirFromAngle * 0.5f);
				TileObject fixedObjectOnTile2 = instance.GetFixedObjectOnTile(tileCoordForPosXZ.x, tileCoordForPosXZ.y);
				if (fixedObjectOnTile2 != null && fixedObjectOnTile2.CoverType == CoverType.WaistHigh && !instance.IsSlopeOrImpassableRaw(tileCoordForPosXZ.x, tileCoordForPosXZ.y))
				{
					TryStartActionAnim(ActionAnim.Vault, fixedObjectOnTile2, canInterruptEqualPriority: true, TimeSpan.FromSeconds(AnimWrapper.VaultLoopStartFrac * AnimWrapper.Clip.length / AnimWrapper.Speed), IsCurrentActionAnimFromGoal);
				}
			}
			break;
		default:
			if (GoalTarget != null && (IsAiming() || MovementSpeed == 0f))
			{
				DesiredFacingAngle = CalcDesiredFacingAngleFromGoalTarget();
			}
			break;
		}
		if (CurrentActionAnim == ActionAnim.AttackJumpingZombie && CanAttackJumpingZombie == CanAttackState.CanAttack && EquippedItem != null)
		{
			float damageIncludingEffects = EquippedItem.GetDamageIncludingEffects(this);
			InjuryType injuryType = EquippedItem.GetInjuryType();
			OnMeleeAttack(AttackType.AttackJumpingZombie, injuryType, Bone.MeleeWeapon, damageIncludingEffects);
			CanAttackJumpingZombie = CanAttackState.HasAttacked;
		}
		if (CurrentActionAnim == ActionAnim.SnapAttack && CanAttackGrabbingZombie == CanAttackState.CanAttack && EquippedItem != null)
		{
			float damageIncludingEffects2 = EquippedItem.GetDamageIncludingEffects(this);
			InjuryType injuryType2 = EquippedItem.GetInjuryType();
			OnMeleeAttack(AttackType.Snap, injuryType2, Bone.MeleeWeapon, damageIncludingEffects2);
			CanAttackGrabbingZombie = CanAttackState.HasAttacked;
		}
		if (DirectControlled)
		{
			UpdateDirectControlled(dt);
		}
		else
		{
			MovementSpeed = 0f;
		}
		if (CarryingObject != null && ShouldDropCarriedObject())
		{
			Drop();
		}
		if (MovementType != MovementType.None && CurrentAnimState == AnimState.Animation && InsideBuilding == null && (CanMoveDuringActionAnim() || IsActionAnimInterruptible()))
		{
			if (!CanMoveDuringActionAnim())
			{
				bool num4 = IsActionAnimFinished();
				ActionAnim currentActionAnim = CurrentActionAnim;
				ClearActionAnim();
				if (num4)
				{
					if (CurrentSpeechAnimState == SpeechAnimState.Started)
					{
						CurrentSpeechAnimState = SpeechAnimState.Finished;
					}
					if (Goal != null)
					{
						Goal.OnActionAnimFinished(this, null, currentActionAnim);
					}
				}
			}
			if (MovementMode == MovementMode.Pathfinding && MovementType == MovementType.Run && !Tired && !InCombat && UnderAttackRefCount == 0 && GetFatigueMinusAdrenaline() >= 0.6f && GetRouteDestination().GetDistSquared(Tile) >= JogIfTiredDist * JogIfTiredDist)
			{
				Tired = true;
			}
			else if (Tired && (GetFatigueMinusAdrenaline() < 0.5f || MovementMode != MovementMode.Pathfinding || MovementType != MovementType.Run || InCombat || UnderAttackRefCount > 0 || GetRouteDestination().GetDistSquared(Tile) < JogIfTiredDist * JogIfTiredDist))
			{
				Tired = false;
			}
			MovementSpeed = PickAIMovementSpeed(MovementType, Tired);
			float num5 = MovementSpeed * num;
			do
			{
				MovementMode movementMode = MovementMode;
				Vector2 vector3;
				if ((uint)(movementMode - 1) <= 2u)
				{
					vector3 = ((GoalTarget != null && GoalTarget.Object != null) ? GoalTarget.Object.PosXZ : PosXZ);
					num5 = Math.Min(num5, MovementSpeed * (1f / 60f) * 4f);
				}
				else
				{
					vector3 = instance.GetTileCentreXZ(Route[0]);
				}
				Vector2 vector4 = new Vector2(vector3.x - Position.x, vector3.y - Position.z);
				float magnitude = vector4.magnitude;
				Vector2 vector5 = MathUtil.ToXZ(base.Forward);
				if (magnitude > 0.01f)
				{
					vector5 = vector4 / magnitude;
				}
				switch (MovementMode)
				{
				case MovementMode.FlankLeft:
					vector5 = MathUtil.SafeNormalize((magnitude - FlankingDesiredRange) * vector5 - MathUtil.RightNormal(vector5), -MathUtil.ToXZ(base.Right));
					break;
				case MovementMode.FlankRight:
					vector5 = MathUtil.SafeNormalize((magnitude - FlankingDesiredRange) * vector5 + MathUtil.RightNormal(vector5), MathUtil.ToXZ(base.Right));
					break;
				case MovementMode.Pathfinding:
					if (Tile != Route[0])
					{
						Vector2 pos = PosXZ + vector5 * 0.5f;
						TerrainCoord tileCoordForPosXZ2 = instance.GetTileCoordForPosXZ(pos);
						TileObject fixedObjectOnTile3 = instance.GetFixedObjectOnTile(tileCoordForPosXZ2.x, tileCoordForPosXZ2.y);
						if (fixedObjectOnTile3 != null && fixedObjectOnTile3.CoverType == CoverType.WaistHigh && (fixedObjectOnTile3.GetUnderConstructionInfo() == null || fixedObjectOnTile3.GetUnderConstructionInfo().IsBuiltEnoughToBeAnObstacle()))
						{
							OnEncounterWaistHighWall(fixedObjectOnTile3);
						}
					}
					break;
				}
				float desiredFacingAngle = (MovementAngle = (float)Math.Atan2(vector5.x, vector5.y));
				if ((MovementMode == MovementMode.FlankLeft || MovementMode == MovementMode.FlankRight) && HasMovementZone() && MovementZone.Contains(Tile) && !MovementZone.Contains(instance.GetTileCoordForPosXZ(PosXZ + vector5)))
				{
					MovementSpeed = 0f;
					break;
				}
				if (MovementMode == MovementMode.MoveDirectlyToTarget && magnitude <= Radius + ((GoalTarget != null && GoalTarget.Object is Character) ? ((Character)GoalTarget.Object).Radius : 0f) * 1.01f)
				{
					DesiredFacingAngle = desiredFacingAngle;
					MovementSpeed = 0f;
					break;
				}
				if (magnitude > num5)
				{
					if (!IsAiming() || GoalTarget == null)
					{
						DesiredFacingAngle = desiredFacingAngle;
					}
					if (WantAvoidance)
					{
						vector5 += new Vector2(vector5.y, 0f - vector5.x);
						vector5.Normalize();
						WantAvoidance = false;
					}
					AddVelocityXZ(num5 * vector5, setFollowMeDir: true);
					break;
				}
				if (magnitude <= 0.01f || (MovementMode == MovementMode.Pathfinding && Route.Count > 1 && !IsVisibleForUpdate()))
				{
					if (!IsAiming() || GoalTarget == null)
					{
						DesiredFacingAngle = desiredFacingAngle;
					}
					if (magnitude > 0.01f && (CollisionManager.TraceCollisionsIfLargeStep(this, Position, vector4, out var _) - vector3).sqrMagnitude > 0.0001f)
					{
						AddVelocityXZ(vector4, setFollowMeDir: true);
						break;
					}
					SetPosition(vector3.x, vector3.y);
					SetFollowMePos(Vector2.zero);
					num5 -= magnitude;
					if (MovementMode == MovementMode.Pathfinding)
					{
						PopNextRouteNode();
					}
					continue;
				}
				if (!IsAiming() || GoalTarget == null)
				{
					DesiredFacingAngle = desiredFacingAngle;
				}
				AddVelocityXZ(vector4, setFollowMeDir: true);
				break;
			}
			while (!Session.Instance.PropManager.HasTriggeredTrap(this) && num5 >= 0.01f && MovementType != MovementType.None && MovementMode == MovementMode.Pathfinding);
		}
		Adrenaline = Math.Max(Adrenaline - num / AdrenalineTimeout, 0f);
		float minFatigue = GetMinFatigue();
		Fatigue = Math.Max(Fatigue, minFatigue);
		if (MovementSpeed < JogSpeed + 0.001f)
		{
			if (!IsBeingBittenOrChoked())
			{
				Fatigue = Math.Max(Fatigue - FatigueRecoverRate * num, minFatigue);
			}
		}
		else
		{
			ApplyFatiguePenalty(RunningFatigueRate * num);
		}
		if (CarryingObject != null && MovementSpeed > 0f)
		{
			ApplyFatiguePenalty(CarryingObject.GetWeightWhenCarried() * CarryingFatigueRatePerLb * MovementSpeed * num);
		}
		if (IsAiming() && EquippedItem != null)
		{
			ApplyFatiguePenalty(EquippedItem.GetFatiguePenaltyWhenAiming() * num);
			if (EquippedItem is AmmoWeapon)
			{
				PlayerRecord playerRecord = (DirectControlled ? GetPlayerControllingMe() : null);
				AmmoWeapon ammoWeapon = EquippedItem as AmmoWeapon;
				if (MovementSpeed == 0f && (CrouchingTransition <= 0f || CrouchingTransition >= 1f))
				{
					TileObject tileObject = null;
					TargettableBodyLocation targettableBodyLocation = TargettableBodyLocation.Torso;
					Vector2 vector6 = Vector2.zero;
					Vector2 vector7 = Vector3.zero;
					bool flag = false;
					bool flag2 = false;
					if (DirectControlled)
					{
						if (playerRecord != null && playerRecord.WantLockOnTarget)
						{
							tileObject = playerRecord.TargetObject;
							targettableBodyLocation = playerRecord.TargetBodyLocationToAimFor;
							vector6 = tileObject?.PosXZ ?? MathUtil.ToXZ(playerRecord.TargetPos);
							vector7 = ((tileObject is Character) ? (MathUtil.ToXZ(((Character)tileObject).GetOldVelocity()) / num) : Vector2.zero);
							flag2 = IsTargetBlocked(DirectControlledTarget, playerRecord.TargetPos, playerRecord.ThrowAngle, playerRecord.ThrowSpeed);
							flag = true;
						}
					}
					else
					{
						tileObject = ((GoalTarget != null) ? GoalTarget.Object : null);
						if (tileObject != null)
						{
							vector6 = tileObject.PosXZ;
							vector7 = ((tileObject is Character) ? (MathUtil.ToXZ(((Character)tileObject).GetOldVelocity()) / num) : Vector2.zero);
							flag = true;
							flag2 = !GoalTarget.HasLineOfSightIgnoringPlantCover;
						}
					}
					float accurateRange;
					float accurateRangeAtLowestSkill;
					float rangeIncludingEffects = ammoWeapon.GetRangeIncludingEffects(this, tileObject, out accurateRange, includingBuildingEffects: true, out accurateRangeAtLowestSkill);
					if (flag && (vector6 - PosXZ).sqrMagnitude <= rangeIncludingEffects * rangeIncludingEffects && !flag2)
					{
						float magnitude2 = (vector6 - PosXZ).magnitude;
						float num8;
						if (targettableBodyLocation == TargettableBodyLocation.Head || tileObject is Rabbit)
						{
							int num6 = ((targettableBodyLocation == TargettableBodyLocation.Head) ? RangedHeadshotSkillLevel : 0);
							float num7 = Mathf.Clamp01((float)(GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType()) - num6) / (5f - (float)num6));
							num8 = Mathf.Lerp(ammoWeapon.GetMaxHeadAimTime(), ammoWeapon.GetMinHeadAimTime(), num7 * (1f - Mathf.Clamp01(magnitude2 / rangeIncludingEffects)));
						}
						else if (targettableBodyLocation == TargettableBodyLocation.Legs)
						{
							float num9 = Mathf.Clamp01((float)(GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType()) - RangedLegshotSkillLevel) / (5f - (float)RangedLegshotSkillLevel));
							num8 = Mathf.Lerp(ammoWeapon.GetMaxLegsAimTime(), ammoWeapon.GetMinLegsAimTime(), num9 * (1f - Mathf.Clamp01(magnitude2 / rangeIncludingEffects)));
						}
						else
						{
							float num10 = (float)GetSkillLevelWithEffects(EquippedItem.GetRangeSkillType()) / 5f;
							num8 = Mathf.Lerp(ammoWeapon.GetMaxAimTime(), ammoWeapon.GetMinAimTime(), num10 * (1f - Mathf.Clamp01(magnitude2 / rangeIncludingEffects)));
						}
						if (tileObject is Character { InsideBuilding: not null })
						{
							num8 *= 2f;
						}
						num8 += MathUtil.Squared(BloodAlcoholConcentration / BACComa) * 10f;
						float max = 1f - Mathf.Clamp01((magnitude2 - accurateRange) / (rangeIncludingEffects - accurateRangeAtLowestSkill));
						float magnitude3 = vector7.magnitude;
						float num11 = ((magnitude3 > 0.01f) ? (Mathf.Abs(Vector2.Dot(MathUtil.ToXZ(base.Right), vector7 / magnitude3)) * magnitude3) : 0f);
						float num12 = ((magnitude3 > 0.01f) ? (Mathf.Max(0f, Vector2.Dot(MathUtil.ToXZ(base.Forward), vector7 / magnitude3)) * magnitude3) : 0f);
						float num13 = AimPenaltyWhenTargetIsMovingLaterally * num11 / RunSpeed + AimPenaltyWhenTargetIsMovingAway * num12 / GetRunSpeed();
						AimingAccuracy = Mathf.Clamp(AimingAccuracy + num / num8 + num * num13, 0f, max);
					}
					else
					{
						AimingAccuracy = 0f;
					}
				}
				else
				{
					ApplyAimingPenalty(ammoWeapon.GetAimPenaltyWhenMoving() * num);
				}
			}
			else
			{
				AimingAccuracy = 0f;
			}
		}
		else
		{
			AimingAccuracy = 0f;
		}
		if (SparringPartner != null && IsAuthoritative() && !HasQueuedSpeechOfImportance(Importance.EndFisticuffs) && !StoryManager.Instance.HasQueuedBoxingMatchEvents(this))
		{
			if (!SparringPartner.Alive || IsEnemy(SparringPartner) || SparringPartner.SparringPartner != this)
			{
				SetSparringPartner(SparringType.None, null, null);
			}
			else if (SparringType == SparringType.SnowballFight)
			{
				if (SparringInstigator != this)
				{
					if (((SparringPartner.PosXZ - PosXZ).sqrMagnitude >= FisticuffsCancelDist * FisticuffsCancelDist && !IsFleeing()) || SparringPartner.Consciousness >= Consciousness.Unconscious || Session.Instance.Weather.TemperatureInCelsius >= Weather.ScoopableSnowOnGroundAmount || SparringPartner.InsideBuilding != null)
					{
						Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.SnowballFightYouRanAway);
						if (speechForSituation != null)
						{
							PushQueuedSpeech(speechForSituation, SparringPartner, null, default(MemoryParam));
						}
					}
					else if (SparringPartner.SparringSnowballHits >= SnowballFightWinHits)
					{
						Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.SnowballFightYouWin);
						if (speechForSituation2 != null)
						{
							PushQueuedSpeech(speechForSituation2, SparringPartner, null, default(MemoryParam));
						}
					}
					else if (SparringSnowballHits >= SnowballFightWinHits)
					{
						Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.SnowballFightIWin);
						if (speechForSituation3 != null)
						{
							PushQueuedSpeech(speechForSituation3, SparringPartner, null, default(MemoryParam));
						}
					}
				}
			}
			else if (SparringType == SparringType.Feuding)
			{
				if (((SparringPartner.PosXZ - PosXZ).sqrMagnitude >= FisticuffsCancelDist * FisticuffsCancelDist && SparringInstigator != this && !IsPlayerAvatar()) || ((SparringPartner.PosXZ - PosXZ).sqrMagnitude >= FisticuffsCancelDist * FisticuffsCancelDist && SparringInstigator == this && SparringPartner.IsPlayerAvatar()) || SparringPartner.Consciousness >= Consciousness.Unconscious || SparringPartner.IsFleeing() || SparringPartner.InsideBuilding != null)
				{
					Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FeudingYouRanAway);
					if (speechForSituation4 != null)
					{
						PushQueuedSpeech(speechForSituation4, SparringPartner, null, default(MemoryParam));
					}
				}
				else if ((BloodLoss - SparringPartner.SparringPartnerInitialBloodLoss >= FisticuffsWinBloodLoss || BloodLoss >= FeudingMaxBloodLoss) && BloodLoss >= SparringPartner.GetBloodLoss())
				{
					Speech speechForSituation5 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FeudingYouWin);
					if (speechForSituation5 != null)
					{
						PushQueuedSpeech(speechForSituation5, SparringPartner, null, default(MemoryParam));
					}
				}
				else if (Speaking == null && SparringPartner.Speaking == null && Session.Instance.PlayTime - SpeechStartTime >= TimeSpan.FromSeconds(Mathf.Lerp(5f, 8f, (float)(Id * 1000) + (float)SpeechStartTime.TotalSeconds % 1000f)) && SparringMemories.Count > 0)
				{
					int index = Session.Instance.DeterministicRand.Next(SparringMemories.Count);
					Memory memory = SparringMemories[index];
					SparringMemories.RemoveAt(index);
					Speech speechForMemory = StoryManager.Instance.GetSpeechForMemory(this, SparringPartner, new MemoryParam(memory));
					if (speechForMemory != null)
					{
						Speak(speechForMemory, SparringPartner, memory.Object, new MemoryParam(memory));
					}
				}
			}
			else if (SparringType == SparringType.FightToTheDeath)
			{
				if (SparringInstigator != this && ((SparringPartner.PosXZ - PosXZ).sqrMagnitude >= FisticuffsCancelDist * FisticuffsCancelDist || SparringPartner.Consciousness >= Consciousness.Unconscious || SparringPartner.IsFleeing() || SparringPartner.InsideBuilding != null))
				{
					if (IsConscious)
					{
						Speech speechForSituation6 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FeudingYouRanAway);
						if (speechForSituation6 != null)
						{
							PushQueuedSpeech(speechForSituation6, SparringPartner, null, default(MemoryParam));
						}
					}
					else
					{
						SparringPartner.SetSparringPartner(SparringType.None, null, null);
						SetSparringPartner(SparringType.None, null, null);
					}
				}
			}
			else if (SparringInstigator != this)
			{
				if ((SparringPartner.PosXZ - PosXZ).sqrMagnitude >= FisticuffsCancelDist * FisticuffsCancelDist || SparringPartner.Consciousness >= Consciousness.Unconscious || SparringPartner.InsideBuilding != null)
				{
					Speech speechForSituation7 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FisticuffsYouRanAway);
					if (speechForSituation7 != null)
					{
						PushQueuedSpeech(speechForSituation7, SparringPartner, null, default(MemoryParam));
					}
				}
				else if (BloodLoss >= FisticuffsWinBloodLoss && BloodLoss >= SparringPartner.GetBloodLoss())
				{
					Speech speechForSituation8 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FisticuffsYouWin);
					if (speechForSituation8 != null)
					{
						PushQueuedSpeech(speechForSituation8, SparringPartner, null, default(MemoryParam));
					}
				}
				else if ((SparringPartner.BloodLoss >= FisticuffsWinBloodLoss && SparringPartner.GetBloodLoss() >= BloodLoss) || SparringPartner.Consciousness >= Consciousness.Unconscious)
				{
					Speech speechForSituation9 = StoryManager.Instance.GetSpeechForSituation(this, SparringPartner, SpeechSituation.FisticuffsIWin);
					if (speechForSituation9 != null)
					{
						PushQueuedSpeech(speechForSituation9, SparringPartner, null, default(MemoryParam));
					}
				}
			}
		}
		if (FacingAngle != DesiredFacingAngle && CurrentAnimState == AnimState.Animation && !Sitting && CanTurnDuringActionAnim())
		{
			FacingAngle = MathUtil.AngleStep(FacingAngle, DesiredFacingAngle, GetRotSpeed() * num);
			UpdateWorldTransformAndBounds();
		}
		TriggerAnimEvents(dt);
		if (CurrentActionAnim != ActionAnim.None && IsActionAnimFinished())
		{
			if (DirectControlled && CurrentActionAnim == ActionAnim.Unequip && !IsCurrentActionAnimFromGoal)
			{
				TryStartActionAnim((DesiredEquippedItem != null) ? DesiredEquippedItem.GetEquipAction() : ActionAnim.EquipUnarmed, null, canInterruptEqualPriority: true);
			}
			else
			{
				if (CurrentSpeechAnimState == SpeechAnimState.Started)
				{
					CurrentSpeechAnimState = SpeechAnimState.Finished;
				}
				ActionAnim currentActionAnim2 = CurrentActionAnim;
				TryStartActionAnim(ActionAnim.None, null);
				if (Goal != null)
				{
					Goal.OnActionAnimFinished(this, null, currentActionAnim2);
				}
			}
		}
		if (IsRecoveringFromRagdoll() && (curTime - AnimStartTime) * AnimSpeed >= RecoverFromRagdollTime)
		{
			ActionAnim anim = ((CurrentAnimState == AnimState.RecoverFromRagdoll_Back) ? ActionAnim.GetUp_Back : ActionAnim.GetUp_Front);
			CurrentAnimState = AnimState.Animation;
			TryStartActionAnim(anim, null);
		}
		CrouchingTransition = Mathf.Clamp01(CrouchingTransition + (IsCrouching() ? 1f : (-1f)) * num / CrouchingTransitionTime);
		AimingTransition = Mathf.Clamp01(AimingTransition + (IsAiming() ? 1f : (-1f)) * num / AimingTransitionTime);
		if (InvisibleStrain == InvisibleStrainType.None || Zombie)
		{
			return;
		}
		float a = ((InvisibleStrain == InvisibleStrainType.Subtle) ? InvisibleStrainSnarlProbabilityPerSecondSubtle : InvisibleStrainSnarlProbabilityPerSecondExcitable);
		float b = ((InvisibleStrain == InvisibleStrainType.Subtle) ? InvisibleStrainSnarlProbabilityPerSecondSubtleExcited : InvisibleStrainSnarlProbabilityPerSecondExcitableExcited);
		float probability = Mathf.Lerp(a, b, Excitement) * num;
		if (MathUtil.RandomChoice((float)curTime.TotalSeconds + (float)Id * 0.001f, probability))
		{
			InvisibleStrainSnarlStartTime = curTime;
			if (!IsAnyVoiceSoundPlaying() && CheckFrontmostPrediction(PredictedEventType.ZombieSound))
			{
				PlayVoiceSoundFromList(SoundManager.ZombieAlertSounds, VoiceSoundType.ZombieSnarl);
			}
		}
	}

	public void UpdateRecoveringFromRagdoll()
	{
		if (!IsRecoveringFromRagdoll())
		{
			return;
		}
		AnimationManager.Instance.GetAnim(this, null, (CurrentAnimState == AnimState.RecoverFromRagdoll_Back) ? ActionAnim.GetUp_Back : ActionAnim.GetUp_Front, TimeSpan.Zero).Clip.SampleAnimation(Unity.Obj, 0f);
		float t = (float)(PredictedObjectManager.Instance.GetCurrentTime(IsPredicted()) - AnimStartTime).TotalSeconds * AnimSpeed / (float)RecoverFromRagdollTime.TotalSeconds;
		for (int i = 0; i < 23; i++)
		{
			GameObject unityBone = GetUnityBone((Bone)i);
			if (unityBone != null)
			{
				unityBone.transform.localPosition = Vector3.Lerp(ProneBoneTransforms[i].Pos, unityBone.transform.localPosition, t);
				unityBone.transform.localScale = Vector3.Lerp(Vector3.one * ProneBoneTransforms[i].Scale, unityBone.transform.localScale, t);
				unityBone.transform.localRotation = Quaternion.Slerp(ProneBoneTransforms[i].Orientation, unityBone.transform.localRotation, t);
				MathUtil.CheckNaNorInfinity(unityBone.transform.localPosition);
				MathUtil.CheckNaNorInfinity(unityBone.transform.localScale);
				MathUtil.CheckNaNorInfinity(unityBone.transform.localRotation);
			}
		}
	}

	public override float GetWeightWhenCarried()
	{
		return Appearance.GetWeight();
	}

	public float GetWeightInKg()
	{
		return GetWeightWhenCarried() * 0.453592f;
	}

	public void UpdateVisibleTilesCache()
	{
		using (new UnityProfileMarker(UpdateVisibleTilesCacheStr))
		{
			bool flag = false;
			if (Community != null && (Community.CommunityType == CommunityType.Player || CanFollowPlayer || Community.CachedAllies.Contains(Session.Instance.CommunityManager.PlayerCommunity)) && !Zombie && !Disappeared && (Alive || Session.Instance.PlayTime - TimeOfDeath < LoseFogOfWarAfterDeathTime))
			{
				flag = true;
			}
			if (VisibleTilesCache == null && flag)
			{
				VisibleTilesCache = new VisibleTilesCache();
				GameTerrain.Instance.FogOfWar.MarkVisibleTilesCacheDirty(this);
			}
			else if (VisibleTilesCache != null && !flag && GameTerrain.Instance.FogOfWar.ThreadCalcInProgress != VisibleTilesCache)
			{
				VisibleTilesCache.RemoveRefs(this);
				VisibleTilesCache = null;
			}
		}
	}

	private bool IsAwareOfCrouchingPlayer()
	{
		if (IsConscious && Community != null && Community.CommunityType != CommunityType.Player && GetBaseObjectType() != BaseObjectType.Chicken && !CanFollowPlayer && !Disappeared)
		{
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling && playerRecord.PlayerCharacter != null && playerRecord.PlayerCharacter.IsCrouching())
				{
					Target target = GetTarget(playerRecord.PlayerCharacter);
					if (target != null && target.Camouflage < 1f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void UpdateThinkBucket()
	{
		ThinkPriority thinkPriority = ThinkPriority.Dead;
		if (Alive && !Disappeared)
		{
			thinkPriority = (IsVisibleForUpdate() ? ThinkPriority.Visible : ((InCombat || UnderAttackRefCount > 0 || QueuedSpeeches.Count > 0 || ScriptedGoalMarker != null || IsAwareOfCrouchingPlayer()) ? ThinkPriority.InCombat : ThinkPriority.Idle));
		}
		if (thinkPriority != ThinkPriorityBucket)
		{
			Session instance = Session.Instance;
			if (ThinkPriorityBucket >= ThinkPriority.Idle)
			{
				instance.CharacterManager.ThinkBuckets[(int)ThinkPriorityBucket].Remove(this);
			}
			ThinkPriorityBucket = thinkPriority;
			if (ThinkPriorityBucket >= ThinkPriority.Idle)
			{
				instance.CharacterManager.ThinkBuckets[(int)ThinkPriorityBucket].Add(this);
			}
		}
	}

	public void Think()
	{
		bool flag = IsControllableByPlayer();
		DisableInputsUntilNextThink = false;
		if (InTerrain && InsideBuilding == null && !IsInsideBuildingUnderConstruction() && CurrentActionAnim != ActionAnim.Slide && CurrentActionAnim != ActionAnim.Vault && CurrentActionAnim != ActionAnim.LightFireWithFlint && CurrentActionAnim != ActionAnim.LightFireWithMatch)
		{
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord tile = Tile;
			if (instance.IsImpassable(tile.x, tile.y, 8192, null, null, out var isRound) && !isRound)
			{
				if (!IsRagdoll())
				{
					TerrainCoord nearestPassableTileTo = GetNearestPassableTileTo(LastGoodTile, 2051, this, null, mustBeSpawnable: false, checkPath: true);
					SetPosition(instance.GetTileCentrePos(nearestPassableTileTo));
				}
			}
			else if (!flag && !isRound && !instance.IsTileSpawnable(tile.x, tile.y) && !instance.IsTileEnclosedOrBuiltOn(tile))
			{
				if (!IsInHiddenCommunity() || !GetDontSimulateSurvivalFactors())
				{
					TrappedTimer += (float)(Session.Instance.PlayTime - LastThinkTime).TotalSeconds;
					if (TrappedTimer >= 300f && !IsVisibleForUpdate())
					{
						TerrainCoord nearestPassableTileTo2 = GetNearestPassableTileTo(LastGoodTile, 2051, null, null, mustBeSpawnable: true);
						SetPosition(instance.GetTileCentrePos(nearestPassableTileTo2));
					}
				}
			}
			else if (flag && instance.IsTileIllegal(tile.x, tile.y))
			{
				TerrainCoord nearestPassableTileTo3 = GetNearestPassableTileTo(LastGoodTile, 2051, null, null, mustBeSpawnable: true);
				SetPosition(instance.GetTileCentrePos(nearestPassableTileTo3));
			}
			else
			{
				LastGoodTile = Tile;
				TrappedTimer = 0f;
			}
		}
		for (int i = 0; i < Timers.Count; i++)
		{
			if (Session.Instance.PlayTime >= Timers[i].FinishTime)
			{
				Timers.RemoveAt(i);
			}
		}
		UpdateMemories(force: false);
		int sightRange;
		using (new ProfileMarker(LookTimer))
		{
			Look(out sightRange);
		}
		using (new ProfileMarker(ThinkTimer))
		{
			foreach (Target target in Targets)
			{
				if (target.RefCount == 0)
				{
					if (target.AllowDeletion())
					{
						DeleteList.Add(target);
					}
					target.Update(this);
					continue;
				}
				if (flag)
				{
					if (target.LastVisibleTime != Session.Instance.PlayTime)
					{
						if (target.Object is Character { Tile: var tile2 })
						{
							if (GameTerrain.Instance.FogOfWar.IsTileVisible(tile2.x, tile2.y))
							{
								target.MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
							}
						}
						else if (target.Object != null && GameTerrain.Instance.FogOfWar.IsAnyTileInRectVisible(target.Object.GetMinTile(), target.Object.GetMaxTile()))
						{
							target.MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
						}
					}
				}
				else if (!(target.Object is Character))
				{
					target.MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
				}
				target.Update(this);
			}
			if (Community != null && !Zombie && this is Human && IsAwake && Rank != Rank.Captive)
			{
				if (!flag)
				{
					float bestQuantity;
					MysteriousAttackType mysteriousAttackType = HasRecentlyBeenAttackedByUnknownAssailant(out bestQuantity);
					if (mysteriousAttackType != MysteriousAttackType.None && !HasQueuedSpeechOfType(SpeechSituation.RevivedAngry) && !StoryManager.Instance.HasQueuedEventsOfType(StoryEventType.RegisterAttack))
					{
						bool investigated = !HasAnyTargetsWhoAttackedUs();
						if (FindSomeoneToBlameForAttack(this, mysteriousAttackType, bestQuantity, investigated))
						{
							MarkMysteriousAttackMemoriesAsActioned();
						}
					}
				}
				foreach (Target target2 in Targets)
				{
					Character character2 = target2.Object as Character;
					if (target2.GetFlag(TargetFlags.CarryingFriend) && target2.FullyTracked && !flag && character2.CarryingObject is Character character3 && (character3.GetBaseObjectType() != BaseObjectType.Human || Session.Instance.PlayTime - character3.LastRescuedByPlayer < RescueGoal.MinTimeBetweenAssumingPlayerIsRescuingSomeone) && character2.CommunityNoticedBodySnatching != Community.Id && character2.OnStoleSomething(Community, this, 0f, seenByThiefCommunity: false))
					{
						character2.CommunityNoticedBodySnatching = Community.Id;
					}
					if (!target2.GetFlag((TargetFlags)80) && !target2.GetFlag((TargetFlags)48))
					{
						continue;
					}
					bool flag2 = target2.GetFlag(TargetFlags.KnockedOut);
					if (target2.GetFlag(TargetFlags.HaveAssignedBlameForAttack))
					{
						continue;
					}
					if (flag && (character2 == null || character2.CauseOfDeath != CauseOfDeath.InvisibleStrainAssassination))
					{
						AssignBlameForAttack(target2);
						continue;
					}
					if (character2 != null && character2.Infection == InfectionType.Invisible)
					{
						AssignBlameForAttack(target2);
						continue;
					}
					if (flag2)
					{
						if (HasMemoryOfObjectAfter(MemoryPrototype.KnockedOut, target2.Object, ((Character)target2.Object).LastKnockedOutTime))
						{
							AssignBlameForAttack(target2);
							continue;
						}
					}
					else
					{
						if (character2 != null && (character2.Hunger >= HungerDieTime || character2.Thirst >= ThirstDieTime || character2.SleepDeprivation >= SleepDeprivationDieTime || character2.BodyTemperatureInCelsius <= BodyTemperatureInCelsiusDeath))
						{
							AssignBlameForAttack(target2);
							continue;
						}
						int num = FindMemoryOfObject(MemoryPrototype.Killed, character2);
						if (num == -1)
						{
							num = FindMemoryOfObject(MemoryPrototype.KilledAccidentally, character2);
						}
						if (num == -1)
						{
							num = FindMemoryOfObject(MemoryPrototype.KilledPossibleInvisibleStrain, character2);
						}
						if (num == -1)
						{
							num = FindMemoryOfObject(MemoryPrototype.Died, character2);
						}
						if (num != -1)
						{
							if (Memories[num].Actor != null)
							{
								AssignBlameForAttack(target2);
								continue;
							}
						}
						else
						{
							Memory.OnMemorableEvent(MemoryPrototype.Killed, null, character2, null, 1f, SecrecyMode.OnlyKnownToObjectCommunity, this);
						}
					}
					if (character2 != null && character2.CauseOfDeath == CauseOfDeath.Zombie)
					{
						if (AreAnyLivingZombiesNearby())
						{
							continue;
						}
						if (Session.Instance.PlayTime - character2.TimeOfDeath >= TimeSpan.FromSeconds(120.0))
						{
							AssignBlameForAttack(target2);
							continue;
						}
					}
					if (FindSomeoneToBlameForAttack(target2.Object, flag2 ? MysteriousAttackType.KnockedOut : MysteriousAttackType.Killed, 1f, target2.GetFlag(TargetFlags.HaveInvestigatedBody)))
					{
						AssignBlameForAttack(target2);
					}
				}
				if (!flag)
				{
					for (int j = 0; j < Community.PropertyDamageRecords.Count; j++)
					{
						if (!Community.PropertyDamageRecords[j].HasAssignedBlame)
						{
							float visibleFromDist = Community.PropertyDamageRecords[j].GetVisibleFromDist(sightRange);
							if (Community.PropertyDamageRecords[j].Region.GetClosestDistSqTo(Tile) < visibleFromDist * visibleFromDist && FindSomeoneToBlameForAttack(null, MysteriousAttackType.PropertyDamage, 1f, Community.PropertyDamageRecords[j].Investigated))
							{
								PropertyDamageRecord value = Community.PropertyDamageRecords[j];
								value.HasAssignedBlame = true;
								Community.PropertyDamageRecords[j] = value;
							}
						}
					}
				}
			}
			if (!flag && RestockTime != TimeSpan.Zero && Session.Instance.PlayTime >= RestockTime && !Zombie && IsAwake)
			{
				Restock();
			}
			if (Community != null && Community.CommunityType == CommunityType.RovingTrader && Hunger >= HungryTime && !Zombie && IsAwake && Inventory.GetFood(this, this, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) == null)
			{
				EquipmentPrototype equipmentPrototype = GameImpl.Instance.PickRandomFood(Session.Instance.DeterministicRand, this);
				if (equipmentPrototype != null)
				{
					SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype, 1, fillLiquidContainers: true);
				}
			}
			if (IsControllableByOrFollowingPlayer() && (DirectControlledMajorAIDisabled || IsInAnySquad()) && IsTooDepressedToFollowOrders() && IsLeaderConsciousAndNotZombie())
			{
				LeaveSquad();
				DirectControlledMajorAIDisabled = false;
			}
			foreach (Target delete in DeleteList)
			{
				RemoveTarget(delete);
			}
			DeleteList.Clear();
			WasCrouching = IsCrouching();
			WasAiming = IsAiming();
			if (Goal != null)
			{
				CurrentlyThinking = this;
				Goal.PreUpdate(this, null);
				Goal.Update(this, null);
				Goal.PostUpdate(this, null);
				CurrentlyThinking = null;
				if (Goal.Finished)
				{
					SetGoal(null);
					if (Disappeared)
					{
						RemoveAllTargets();
						LeaveAllSquads();
						SanitiseSquad();
					}
				}
				SanityCheckTargetRefCounts();
			}
			if (OrderedToAttack && !InCombat)
			{
				OrderedToAttack = false;
			}
			LastThinkTime = Session.Instance.PlayTime;
		}
	}

	public bool FindSomeoneToBlameForAttack(TileObject victim, MysteriousAttackType mysteriousAttackType, float mysteriousAttackQuantity, bool investigated)
	{
		Character character = victim as Character;
		float num = 50f;
		Human human = null;
		foreach (Target target2 in Targets)
		{
			if (target2.Object == victim || !(target2.Object is Human human2) || (human2.Zombie && human2.Rotten) || (Community != null && Community.IsFEMA && human2.Community != null && human2.Community.IsFEMA))
			{
				continue;
			}
			if (character != null && character.CauseOfDeath == CauseOfDeath.Zombie && human2.Community != null)
			{
				if (human2.Community.CommunityType != CommunityType.Player || MathUtil.ToXZ(target2.LastKnownPosition - character.Pos).magnitude >= 64f)
				{
					continue;
				}
				TimeSpan time = character.TimeOfDeath - TimeSpan.FromSeconds(180.0);
				if ((Community != null) ? Community.HasMemoryOfHelpingAgainstZombies(human2.Community, time) : HasMemoryOfHelpingAgainstZombies(human2.Community, time))
				{
					human = null;
					return true;
				}
			}
			CalcApprovalRating(human2, out var approval, out var _);
			float num2 = approval;
			if (!target2.FullyTracked || target2.TimeSinceLastDetected > TimeSpan.FromSeconds(10.0))
			{
				num2 += 50f;
			}
			if (HasMemory(MemoryPrototype.Hurt, human2, character) || HasMemory(MemoryPrototype.Hurt, character, human2) || HasMemory(MemoryPrototype.BeatUp, human2, character) || HasMemory(MemoryPrototype.BeatUp, character, human2) || HasMemory(MemoryPrototype.FoughtOff, human2, character) || HasMemory(MemoryPrototype.FoughtOff, character, human2) || HasMemoryOfThirdParty(MemoryPrototype.RestrainedSuccessfully, human2, character) || HasMemoryOfThirdParty(MemoryPrototype.RestrainedSuccessfully, character, human2) || target2.LastAttackedMeTime != Target.Never)
			{
				num2 -= 50f;
			}
			if (HasMemoryOfObjectAfter(MemoryPrototype.AccusedOfInvisibleStrain, human2, Target.Never))
			{
				num2 -= (HasPersonality(CachedPersonalityType.Skeptical) ? 25f : (HasPersonality(CachedPersonalityType.Credulous) ? 75f : 50f));
			}
			if (human2.Zombie)
			{
				num2 -= 10000f;
			}
			else if (human2.Community == Community)
			{
				num2 += 75f;
			}
			else
			{
				CommunityRelationshipType relationship = Session.Instance.CommunityManager.GetRelationship(Community, human2.Community);
				if (relationship == CommunityRelationshipType.Allied)
				{
					num2 += 50f;
				}
				else
				{
					if (relationship == CommunityRelationshipType.Hostile)
					{
						num2 -= 10000f;
					}
					if (target2.GetFlag(TargetFlags.KnownAssailant) || target2.LastAttackedUsTime != Target.Never)
					{
						num2 -= 2000f;
					}
					if (!investigated)
					{
						num2 -= 1000f;
					}
				}
			}
			if (num2 < num)
			{
				num = num2;
				human = human2;
			}
		}
		if (human != null)
		{
			Target target = GetTarget(human);
			if (!target.FullyTracked || target.TimeSinceLastDetected > TimeSpan.FromSeconds(10.0))
			{
				return false;
			}
			SpeechSituation speechSituation = SpeechSituation.None;
			bool flag = num < -10f;
			if (human.Community == Community)
			{
				if (human.Rank == Rank.Leader && DontLeaveCommunity)
				{
					flag = false;
				}
				if (character != null && character.DontLeaveCommunity)
				{
					flag = false;
				}
			}
			if (flag)
			{
				if (character != null)
				{
					MemoryPrototype proto = mysteriousAttackType switch
					{
						MysteriousAttackType.Hurt => MemoryPrototype.Hurt, 
						MysteriousAttackType.Killed => MemoryPrototype.Killed, 
						_ => MemoryPrototype.KnockedOut, 
					};
					if (IsEmpathyDisabled(victim))
					{
						AddMemory(proto, human, victim, mysteriousAttackQuantity);
					}
					else
					{
						Memory.OnMemorableEvent(proto, human, victim, null, mysteriousAttackQuantity, SecrecyMode.OnlyKnownToObjectCommunity, this);
					}
					if (!character.WasAttackedBeforeLastCeaseFire)
					{
						speechSituation = ((mysteriousAttackType == MysteriousAttackType.Killed) ? SpeechSituation.BlameForMurder : SpeechSituation.BlameForAttack);
					}
				}
				else
				{
					Debug.Log("Blaming " + human.GetDisplayNameString() + " for destroying " + ((victim != null) ? victim.GetDisplayNameString() : "something"));
					Memory.OnMemorableEvent(MemoryPrototype.DestroyedProperty, human, Community, null, mysteriousAttackQuantity, SecrecyMode.OnlyKnownToObjectCommunity, this);
					speechSituation = SpeechSituation.BlameForArson;
				}
			}
			else if (character != null)
			{
				MemoryPrototype proto2 = ((mysteriousAttackType == MysteriousAttackType.Killed) ? MemoryPrototype.MurderSuspect : MemoryPrototype.AttackSuspect);
				if (IsEmpathyDisabled(victim))
				{
					AddMemory(proto2, human, victim, mysteriousAttackQuantity);
				}
				else
				{
					Memory.OnMemorableEvent(proto2, human, victim, null, mysteriousAttackQuantity, SecrecyMode.OnlyKnownToObjectCommunity, this);
				}
				speechSituation = ((mysteriousAttackType == MysteriousAttackType.Killed) ? SpeechSituation.SuspectOfMurder : SpeechSituation.SuspectOfAttack);
			}
			else
			{
				Memory.OnMemorableEvent(MemoryPrototype.ArsonSuspect, human, Community, null, mysteriousAttackQuantity, SecrecyMode.OnlyKnownToObjectCommunity, this);
				speechSituation = SpeechSituation.SuspectOfArson;
			}
			foreach (Character member in Community.Members)
			{
				foreach (Target target3 in member.Targets)
				{
					if (target3.Object == human)
					{
						if (!flag)
						{
						}
					}
					else
					{
						target3.ForgetUnprovenAttacks();
					}
				}
			}
			if (speechSituation != SpeechSituation.None && human.AliveAndNotZombie && !IsEmpathyDisabled(victim))
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(this, human, victim, speechSituation);
				if (speechForSituation != null)
				{
					PushQueuedSpeech(speechForSituation, human, victim, default(MemoryParam));
				}
				SetRecentActivity(RecentActivityType.Blame, human);
			}
			if (victim == this)
			{
				for (int i = 0; i < Injuries.Count; i++)
				{
					if (Injuries[i].AssailantUnknown)
					{
						Injury value = Injuries[i];
						value.AssailantUnknown = false;
						value.Attacker = human;
						Injuries[i] = value;
					}
				}
			}
			return true;
		}
		return false;
	}

	public bool IsEmpathyDisabled(TileObject targetObj)
	{
		for (int i = 0; i < EmpathyOverrides.Count; i++)
		{
			if (EmpathyOverrides[i].OverrideObject == targetObj && EmpathyOverrides[i].OverrideValue <= 0f)
			{
				return true;
			}
		}
		if (Rank == Rank.Captive && targetObj != this && targetObj != null && targetObj.GetCommunity() == Community)
		{
			return true;
		}
		return false;
	}

	public void AssignBlameForAttack(Target target)
	{
		target.SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
		if (IsEmpathyDisabled(target.Object))
		{
			return;
		}
		foreach (Character member in Community.Members)
		{
			if (member.ConsciousAndNotZombie && member != target.Object)
			{
				member.GetOrCreateTarget(target.Object).SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
			}
		}
	}

	public string GetGoalDebugString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Goal != null)
		{
			Goal.BuildDebugString(this, null, stringBuilder);
		}
		return stringBuilder.ToString();
	}

	public void SanityCheckTargetRefCounts()
	{
		SanityCheckTargetRefCounts(onLoadingThread: false, null);
	}

	public void SanityCheckTargetRefCounts(bool onLoadingThread, string prevGoalStr)
	{
		if (IsPredicted())
		{
			return;
		}
		foreach (Target target in Targets)
		{
			target.SanityCheckRefCount = 0;
		}
		GoalsToProcess.Clear();
		if (DirectControlledTarget != null)
		{
			DirectControlledTarget.SanityCheckRefCount++;
		}
		if (Goal != null)
		{
			GoalsToProcess.Add(Goal);
		}
		for (int i = 0; i < GoalsToProcess.Count; i++)
		{
			Goal goal = GoalsToProcess[i];
			if (goal.Target != null)
			{
				goal.Target.SanityCheckRefCount++;
			}
			if (!(goal is StateMachineGoal stateMachineGoal))
			{
				continue;
			}
			if (stateMachineGoal.SubGoal != null)
			{
				GoalsToProcess.Add(stateMachineGoal.SubGoal);
			}
			if (goal is ObeyLeaderGoal obeyLeaderGoal && obeyLeaderGoal.GetCommand() != null && obeyLeaderGoal.GetCommand() != obeyLeaderGoal.SubGoal)
			{
				GoalsToProcess.Add(obeyLeaderGoal.GetCommand());
			}
			if (!(goal is PrioritiserGoal prioritiserGoal))
			{
				continue;
			}
			foreach (Goal subGoal in prioritiserGoal.SubGoals)
			{
				if (subGoal != prioritiserGoal.SubGoal)
				{
					GoalsToProcess.Add(subGoal);
				}
			}
		}
		foreach (Target target2 in Targets)
		{
			if (target2.SanityCheckRefCount != target2.RefCount)
			{
				Debug.LogWarning("Sanity Check RefCount Failed! " + GetDisplayNameString() + ": " + ((prevGoalStr != null) ? prevGoalStr : GetGoalDebugString()));
			}
		}
		GoalsToProcess.Clear();
	}
}
