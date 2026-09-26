using System;
using System.Collections.Generic;
using System.Text;
using BeautifyEffect;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class HudBehaviour : MonoBehaviour
{
	public static HudBehaviour Instance;

	public RenderTexture MainViewRenderTexture;

	public RenderTexture SmallMainViewRenderTexture;

	public RenderTexture PipViewRenderTexture;

	public RenderTexture MinimapViewRenderTexture;

	public RenderTexture DecalLayerRenderTexture;

	public RectTransform HudPanelRectTransform;

	public RectTransform MainPanelRectTransform;

	public RectTransform PipPanelRectTransform;

	public RectTransform NamesPanelRectTransform;

	public RectTransform MinimapPanelRectTransform;

	public RectTransform OpinionGraphRectTransform;

	public RectTransform AdmireRectTransform;

	public RectTransform PityRectTransform;

	public RectTransform DespiseRectTransform;

	public RectTransform FearRectTransform;

	public RectTransform SidebarRectTransform;

	public RawImage UnityMainImage;

	public RawImage UnityPipImage;

	public RawImage UnityMinimapImage;

	public OpinionGraphBehaviour UnityOpinionGraph;

	public GameObject UnityGameCameraObj;

	public GameObject UnityDecalLayersCameraObj;

	public GameObject UnityPipCameraObj;

	public MinimapCameraBehaviour UnityMinimapCameraBehaviour;

	public GameObject UnityMapCameraObj;

	public Camera UnityGameCamera;

	public Camera UnityDecalLayersCamera;

	public Camera UnityPipCamera;

	public Camera UnityMinimapCamera;

	public Camera UnityMapCamera;

	public GrassCameraBehaviour UnityGrassCameraBehaviour;

	public GrassCameraBehaviour UnityPipGrassCameraBehaviour;

	public DepthOfFieldDeprecated UnityPipDoFCameraBehaviour;

	public PostProcessLayer UnityPostProcessingLayer;

	public GameCameraBehaviour UnityGameCameraBehaviour;

	public PipCameraBehaviour UnityPipCameraBehaviour;

	public OutlineCameraBehaviour UnityFocusedOutlineBehaviour;

	public OutlineCameraBehaviour UnityTargetOutlineBehaviour;

	public OutlineCameraBehaviour UnitySelectedOutlineBehaviour;

	public Beautify UnityBeautifyBehaviour;

	public FrostEffect UnityGameCameraFrostEffect;

	public int GameCameraCullingMask;

	public GameObject UnityPauseIcon;

	public GameObject UnityFastForwardIcon;

	public TextMeshProUGUI UnityPlaySpeedText;

	public RawImage UnityBiohazardImage;

	public ProgressBarBehaviour UnityInfectionBar;

	public RawImage UnitySavingIcon;

	public RawImage UnityMicrophoneIcon;

	public TextMeshProUGUI UnityWIPText;

	public GameObject UnityChatInputFieldBG;

	public TMP_InputField UnityChatInputField;

	public ChatState CurrentChatState;

	public RawImage UnityVignette;

	public EquipmentIconBehaviour UnityEquippedIcon;

	public RawImage UnityFullBodyIconImage;

	public RawImage UnityBodyArmorIcon;

	public RawImage UnityHelmetIcon;

	public RawImage UnityLegArmorIcon;

	public GameObject UnityPipCaption;

	public TextMeshProUGUI UnityPipCaptionText;

	public TextMeshProUGUI UnityGoToPipObjectPrompt;

	public TextMeshProUGUI UnitySelectionInfo;

	public GameObject UnityInhabitantsPanel;

	public GameObject UnityStarsPanel;

	public GameObject UnityShortcutsPanel;

	public List<CharacterIconBehaviour> UnityInhabitants = new List<CharacterIconBehaviour>();

	public GameObject UnityPipPanel;

	public HalftoneBehaviour UnityNamesPanel;

	public TextMeshProUGUI UnityDisplayName;

	public TextMeshProUGUI UnitySubDisplayName;

	public TextMeshProUGUI UnityControlledCharacterName;

	public TextMeshProUGUI UnityBloodLossTitle;

	public TextMeshProUGUI UnityVehicleDamageTitle;

	public TextMeshProUGUI UnityFatigueTitle;

	public TextMeshProUGUI UnityVehicleGasTitle;

	public TextMeshProUGUI UnityAdrenalineTitle;

	public ProgressBarBehaviour UnityBloodLoss;

	public ProgressBarBehaviour UnityBloodLossUnconscious;

	public ProgressBarBehaviour UnityFatigue;

	public ProgressBarBehaviour UnityAdrenaline;

	public TextMeshProUGUI UnityLoadedAmmo;

	public TextMeshProUGUI UnityTotalAmmo;

	public TextMeshProUGUI UnityParryPromptText;

	public TextMeshProUGUI UnityParryPromptSuccessText;

	public RawImage UnityAmmoTypeIcon;

	public RawImage UnityParryPromptImage;

	public RawImage UnityParryPromptBang;

	public RawImage UnityParryPromptForbidden;

	public TextMeshProUGUI UnityTemperatureText;

	public RawImage UnityMiniThermometer;

	public RawImage UnityMiniThermometerMercury;

	public RawImage UnityWeatherVaneImage;

	public RawImage UnityCompassImage;

	public RawImage[] UnityInjury = new RawImage[6];

	public RawImage[] UnityInjuryLine = new RawImage[6];

	public Vector3[] UnityInjuryOriginalPos = new Vector3[6];

	public Vector3[] UnityInjuryLineOriginalPos = new Vector3[6];

	public GameObject UnityStatusBar;

	public GameObject UnityStatusBarSpacer;

	public GameObject UnityNewNotifications;

	public TextMeshProUGUI UnityNewNotificationsAmount;

	public GameObject UnityNotification;

	public GameObject UnityCommunityStatNotification;

	public GameObject UnityEquipmentNotification;

	public RawImage UnityEquipmentNotificationBG;

	public CharacterSwitchBarBehaviour UnityCharacterSwitchBar;

	public GameObject UnityCharacterSwitchBarContents;

	public GameObject UnityCharacterSwitchBarLeftArrow;

	public GameObject UnityCharacterSwitchBarRightArrow;

	public float CharacterSwitchBarTimeout;

	public float CharacterSwitchBarTransition;

	public SkillsChangeBehaviour UnitySkillsChangePopupPanel;

	public float SkillsChangePopupTransition;

	public TextMeshProUGUI UnityNotificationTitle;

	public TextMeshProUGUI UnityNotificationQuestGroupName;

	public TextMeshProUGUI UnityNotificationPrompt;

	public TextMeshProUGUI UnityNotificationMessage;

	public Image UnityNotificationCloseButton;

	public RawImage UnityNotificationIcon;

	public EventSystem UnityNotificationEventSystem;

	public TextMeshProUGUI UnityEquipmentNotificationName;

	public TextMeshProUGUI UnityEquipmentNotificationAmount;

	public EquipmentIconBehaviour UnityEquipmentNotificationIcon;

	public CommunityStatBehaviour UnityCommunityStatNotificationStat;

	public ActionMenuBehaviour UnityActionMenu;

	public ActionMenuBehaviour UnityActionMenuLeftAligned;

	public ActionMenuBehaviour UnitySpeechMenu;

	public SpeechBubbleBehaviour UnityScreenSpeechBubble;

	public GameObject UnityAudioListener;

	public GameObject UnitySelectionCircle;

	public GameObject UnitySelectionRect;

	public GameObject UnityReticule;

	public Animator UnityReticuleAnimator;

	public GameObject UnityTargetBodyLocation;

	public GameObject[] UnityTargetIndicator = new GameObject[2];

	public GameObject UnityHealthBar;

	public RawImage UnityCraftingProgressIndicator;

	public RawImage UnityOverheadActionIcon;

	public GameObject UnityParryPrompt;

	public GameObject UnityParryPromptTextDisplay;

	public GameObject UnityParryPromptSuccessDisplay;

	public GameObject UnityGateInsideText;

	public GameObject UnityGateOutsideText;

	public List<RawImage> UnityQuestDestinationArrows = new List<RawImage>();

	public Vector2 Size;

	public Vector2 Centre;

	public int MainPanelWidthOnScreen;

	public int PipPanelSizeOnScreen;

	public int MinimapPanelWidthOnScreen;

	public int MinimapPanelHeightOnScreen;

	public Rect MainPanelRect;

	public float GameCameraNearClipDist;

	public float GameCameraFarClipDist;

	public float GameCameraFOV;

	public float GameCameraAspect;

	private StringBuilder DisplayName = new StringBuilder();

	private StringBuilder SubDisplayName = new StringBuilder();

	private StringBuilder PipCaptionText = new StringBuilder();

	private StringBuilder GoToPipObjectText = new StringBuilder();

	private StringBuilder TemperatureText = new StringBuilder();

	private StringBuilder LoadedAmmoText = new StringBuilder();

	private StringBuilder TotalAmmoText = new StringBuilder();

	private StringBuilder ControlledCharacterNameText = new StringBuilder();

	private StringBuilder PlaySpeedText = new StringBuilder();

	private StringBuilder ShortcutText = new StringBuilder();

	private StringBuilder SelectionInfoText = new StringBuilder();

	public static int HUD_Meanwhile = StringUtil.JenkinsHash("HUD_Meanwhile");

	public static int HUD_Paused = StringUtil.JenkinsHash("HUD_Paused");

	public static int HUD_SelectedCount = StringUtil.JenkinsHash("HUD_SelectedCount");

	public static int HUD_VehicleGas = StringUtil.JenkinsHash("HUD_VehicleGas");

	private static float SmallMainPanelWidthOnScreen = 418f;

	private float SmallMainPanelHeightOnScreen = 360f;

	private static float SmallMainPanelSpacingStuff = 8f;

	private static float SidebarWidth = 408f;

	private static float MercuryTopPixel = 13f;

	private static float MercuryBottomPixel = 97f;

	private static float HeightInPixels = 128f;

	private static int HUD_DegreesC = StringUtil.JenkinsHash("HUD_DegreesC");

	private static int HUD_DegreesF = StringUtil.JenkinsHash("HUD_DegreesF");

	public static float MinimapSize = 256f;

	public static float SteamDeckMinimapSize = 384f;

	public static float SidebarMinimapWidth = 384f;

	private static string Bonus = "Bonus";

	private static string Skill = "Skill";

	private static string Star = "Star";

	private static string[] Stars = new string[5] { "Star1", "Star2", "Star3", "Star4", "Star5" };

	private static string Damp = "Damp";

	private static string Icon = "Icon";

	private static string Name = "Name";

	private static string BuildingIcon = "BuildingIcon";

	private static string EquipmentIcon = "EquipmentIcon";

	private static string LiquidIcon = "EquipmentIcon/LiquidIcon";

	private static string InfectedWithIcon = "EquipmentIcon/InfectedWithIcon";

	private static string Shortcut = "EquipmentIcon/Shortcut";

	private static string EquipmentName = "Equipment/Name";

	private static string EquipmentLoadedAmmo = "Equipment/AmmoRow/LoadedAmmo";

	private static string AmmoIcon = "Equipment/AmmoRow/AmmoIcon";

	private static string Recipe = "Recipe";

	private static string RecipeName = "Recipe/Name";

	private static string RecipeItem = "RecipeItem";

	private static string RecipeText = "RecipeText";

	private static string Plus = "+";

	private static string Equal = "=";

	private static string Damage = "Damage";

	private static string Range = "Range";

	private static string Fatigue = "Fatigue";

	private static string FatigueTitle = "FatigueText";

	private static string DamageCompare = "DamageCompare";

	private static string RangeCompare = "RangeCompare";

	private static string FatigueCompare = "FatigueCompare";

	private static string Text = "Text";

	private static string Bar = "Bar";

	private static string BarFill = "Bar/Fill";

	public static int HUD_lbs = StringUtil.JenkinsHash("HUD_lbs");

	public static int HUD_kg = StringUtil.JenkinsHash("HUD_kg");

	public static int HUD_each = StringUtil.JenkinsHash("HUD_each");

	public static int HUD_LoadedAmmo = StringUtil.JenkinsHash("HUD_LoadedAmmo");

	public static int HUD_SightBonus = StringUtil.JenkinsHash("HUD_SightBonus");

	public static int HUD_Point = StringUtil.JenkinsHash("HUD_Point");

	public static int HUD_Points = StringUtil.JenkinsHash("HUD_Points");

	public static int HUD_None = StringUtil.JenkinsHash("HUD_None");

	public static int HUD_Metres = StringUtil.JenkinsHash("HUD_Metres");

	public static int HUD_DontUse = StringUtil.JenkinsHash("HUD_DontUse");

	public static int HUD_DontUse_Female = StringUtil.JenkinsHash("HUD_DontUse_Female");

	public static int HUD_DontUse_Plural = StringUtil.JenkinsHash("HUD_DontUse_Plural");

	public static int HUD_DontUse_Female_Plural = StringUtil.JenkinsHash("HUD_DontUse_Female_Plural");

	public static int HUD_DontEat = StringUtil.JenkinsHash("HUD_DontEat");

	public static int HUD_DontEat_Female = StringUtil.JenkinsHash("HUD_DontEat_Female");

	public static int HUD_DontEat_Plural = StringUtil.JenkinsHash("HUD_DontEat_Plural");

	public static int HUD_DontEat_Female_Plural = StringUtil.JenkinsHash("HUD_DontEat_Female_Plural");

	public static int HUD_DontDrink = StringUtil.JenkinsHash("HUD_DontDrink");

	public static int HUD_DontDrink_Female = StringUtil.JenkinsHash("HUD_DontDrink_Female");

	public static int HUD_DontDrink_Plural = StringUtil.JenkinsHash("HUD_DontDrink_Plural");

	public static int HUD_DontDrink_Female_Plural = StringUtil.JenkinsHash("HUD_DontDrink_Female_Plural");

	public static int HUD_DontCraftWith = StringUtil.JenkinsHash("HUD_DontCraftWith");

	public static int HUD_DontCraftWith_Female = StringUtil.JenkinsHash("HUD_DontCraftWith_Female");

	public static int HUD_DontCraftWith_Plural = StringUtil.JenkinsHash("HUD_DontCraftWith_Plural");

	public static int HUD_DontCraftWith_Female_Plural = StringUtil.JenkinsHash("HUD_DontCraftWith_Female_Plural");

	public static int HUD_DontCookWith = StringUtil.JenkinsHash("HUD_DontCookWith");

	public static int HUD_DontCookWith_Female = StringUtil.JenkinsHash("HUD_DontCookWith_Female");

	public static int HUD_DontCookWith_Plural = StringUtil.JenkinsHash("HUD_DontCookWith_Plural");

	public static int HUD_DontCookWith_Female_Plural = StringUtil.JenkinsHash("HUD_DontCookWith_Female_Plural");

	public static int HUD_DontPlant = StringUtil.JenkinsHash("HUD_DontPlant");

	public static int HUD_DontPlant_Female = StringUtil.JenkinsHash("HUD_DontPlant_Female");

	public static int HUD_DontPlant_Plural = StringUtil.JenkinsHash("HUD_DontPlant_Plural");

	public static int HUD_DontPlant_Female_Plural = StringUtil.JenkinsHash("HUD_DontPlant_Female_Plural");

	public static int HUD_DontShare = StringUtil.JenkinsHash("HUD_DontShare");

	public static int HUD_DontShare_Female = StringUtil.JenkinsHash("HUD_DontShare_Female");

	public static int HUD_DontShare_Plural = StringUtil.JenkinsHash("HUD_DontShare_Plural");

	public static int HUD_DontShare_Female_Plural = StringUtil.JenkinsHash("HUD_DontShare_Female_Plural");

	public static int HUD_DontFeedToAnimals = StringUtil.JenkinsHash("HUD_DontFeedToAnimals");

	public static int HUD_DontFeedToAnimals_Female = StringUtil.JenkinsHash("HUD_DontFeedToAnimals_Female");

	public static int HUD_DontFeedToAnimals_Plural = StringUtil.JenkinsHash("HUD_DontFeedToAnimals_Plural");

	public static int HUD_DontFeedToAnimals_Female_Plural = StringUtil.JenkinsHash("HUD_DontFeedToAnimals_Female_Plural");

	public static int HUD_DontStrip = StringUtil.JenkinsHash("HUD_DontStrip");

	public static int HUD_DontStrip_Female = StringUtil.JenkinsHash("HUD_DontStrip_Female");

	public static int HUD_DontStrip_Plural = StringUtil.JenkinsHash("HUD_DontStrip_Plural");

	public static int HUD_DontStrip_Female_Plural = StringUtil.JenkinsHash("HUD_DontStrip_Female_Plural");

	public static int HUD_AutoCollect = StringUtil.JenkinsHash("HUD_AutoCollect");

	public static int HUD_AutoDeposit = StringUtil.JenkinsHash("HUD_AutoDeposit");

	public static int HUD_Target = StringUtil.JenkinsHash("HUD_Target");

	public static int HUD_NoRestrictions = StringUtil.JenkinsHash("HUD_NoRestrictions");

	public static int HUD_Detectable = StringUtil.JenkinsHash("HUD_Detectable");

	public static int HUD_Hidden = StringUtil.JenkinsHash("HUD_Hidden");

	public static int HUD_Fatigue = StringUtil.JenkinsHash("HUD_Fatigue");

	public static int HUD_SimilarArea = StringUtil.JenkinsHash("HUD_SimilarArea");

	public static int HUD_HarderArea = StringUtil.JenkinsHash("HUD_HarderArea");

	public static Color TraitLowCol = Color.blue;

	public static Color TraitMediumCol = Color.white;

	public static Color TraitHighCol = Color.red;

	private StringBuilder sb = new StringBuilder(50);

	private float ActionMenuWidth = 708f;

	private float MaxLineLength = 648f;

	private const float RecipeSpacing = 4f;

	private static GameProfiler AddRecipeTextTimer = new GameProfiler("AddRecipeText");

	private static GameProfiler AddRecipeIconTimer = new GameProfiler("AddRecipeIcon");

	private static GameProfiler AddRecipeLiquidContainerTimer = new GameProfiler("AddRecipeLiquidContainer");

	private static GameProfiler AddRecipeSpacerTimer = new GameProfiler("AddRecipeSpacerTimer");

	private static GameProfiler AddRecipeTextIconPairTimer = new GameProfiler("AddRecipeTextIconPair");

	public static Color32 InsulationJacketCol = new Color32(byte.MaxValue, 151, 224, byte.MaxValue);

	public static Color32 InsulationJacketDampCol = new Color32(164, 147, 164, byte.MaxValue);

	public static int HUD_Empty = StringUtil.JenkinsHash("HUD_Empty");

	public static int HUD_Empty_Female = StringUtil.JenkinsHash("HUD_Empty_Female");

	public static int HUD_Ammo = StringUtil.JenkinsHash("HUD_Ammo");

	public static int HUD_Workers = StringUtil.JenkinsHash("HUD_Workers");

	public static int HUD_DesiredAmount = StringUtil.JenkinsHash("HUD_DesiredAmount");

	public static int HUD_SetDesiredAmount = StringUtil.JenkinsHash("HUD_SetDesiredAmount");

	public static int HUD_Bleeding = StringUtil.JenkinsHash("HUD_Bleeding");

	public static int HUD_Infected = StringUtil.JenkinsHash("HUD_Infected");

	public static int HUD_Thirsty = StringUtil.JenkinsHash("HUD_Thirsty");

	public static int HUD_Hungry = StringUtil.JenkinsHash("HUD_Hungry");

	public static int HUD_Tired = StringUtil.JenkinsHash("HUD_Tired");

	public static int HUD_Cold = StringUtil.JenkinsHash("HUD_Cold");

	public static int HUD_Depressed = StringUtil.JenkinsHash("HUD_Depressed");

	public static int HUD_Bleeding_Female = StringUtil.JenkinsHash("HUD_Bleeding_Female");

	public static int HUD_Infected_Female = StringUtil.JenkinsHash("HUD_Infected_Female");

	public static int HUD_Thirsty_Female = StringUtil.JenkinsHash("HUD_Thirsty_Female");

	public static int HUD_Hungry_Female = StringUtil.JenkinsHash("HUD_Hungry_Female");

	public static int HUD_Tired_Female = StringUtil.JenkinsHash("HUD_Tired_Female");

	public static int HUD_Cold_Female = StringUtil.JenkinsHash("HUD_Cold_Female");

	public static int HUD_Depressed_Female = StringUtil.JenkinsHash("HUD_Depressed_Female");

	public static int HUD_Hostile = StringUtil.JenkinsHash("HUD_Hostile");

	public static int HUD_Defeated = StringUtil.JenkinsHash("HUD_Defeated");

	public static int HUD_Allies = StringUtil.JenkinsHash("HUD_Allies");

	public static int HUD_UnknownCommunity = StringUtil.JenkinsHash("HUD_UnknownCommunity");

	public static int HUD_MineralRich = StringUtil.JenkinsHash("HUD_MineralRich");

	public static int HUD_MineralPoor = StringUtil.JenkinsHash("HUD_MineralPoor");

	public static int HUD_OutsideZone = StringUtil.JenkinsHash("HUD_OutsideZone");

	public static int HUD_NorthEast = StringUtil.JenkinsHash("HUD_NorthEast");

	public static int HUD_SouthEast = StringUtil.JenkinsHash("HUD_SouthEast");

	public static int HUD_NorthWest = StringUtil.JenkinsHash("HUD_NorthWest");

	public static int HUD_SouthWest = StringUtil.JenkinsHash("HUD_SouthWest");

	public static int HUD_More = StringUtil.JenkinsHash("HUD_More");

	public static int HUD_XP = StringUtil.JenkinsHash("HUD_XP");

	public static int HUD_XP_Singular = StringUtil.JenkinsHash("HUD_XP_Singular");

	public static int HUD_Tastiness = StringUtil.JenkinsHash("HUD_Tastiness");

	public static int HUD_Nutrition = StringUtil.JenkinsHash("HUD_Nutrition");

	public static int HUD_Alcohol = StringUtil.JenkinsHash("HUD_Alcohol");

	public static int HUD_Days = StringUtil.JenkinsHash("HUD_Days");

	public static int HUD_Day = StringUtil.JenkinsHash("HUD_Day");

	public static int HUD_Yes = StringUtil.JenkinsHash("HUD_Yes");

	public static int HUD_No = StringUtil.JenkinsHash("HUD_No");

	private static float MoreTextLength = 0f;

	private static string MoreText;

	private static int RecipeTextSplitLength = 36;

	public static float HoldTimerBtnPromptScale = 0.75f;

	private static string CalculateRelativeRectTransformBounds = "CalculateRelativeRectTransformBounds";

	private int ActionMenuVisibleFrames;

	private static List<LayoutElement> Separators = new List<LayoutElement>();

	private static float ActionMenuXOffsetFlyMode = 100f;

	private static float ActionMenuXOffsetBuildingMode = 200f;

	private static float ActionMenuXOffset = 100f;

	public const string UnityStatusTextObjName = "Text";

	public string StatusBarMsg;

	public float StatusBarMsgTimeout;

	public bool StatusBarMsgShowForOneFrame;

	public AvailableAction StatusBarMsgAction;

	public static float MsgTimeout = 8f;

	public float StatusBarTransition;

	private StringBuilder HintText = new StringBuilder();

	private static int HINT_Unconscious = StringUtil.JenkinsHash("HINT_Unconscious");

	private static int HINT_Depressed = StringUtil.JenkinsHash("HINT_Depressed");

	private static int HINT_Feuding = StringUtil.JenkinsHash("HINT_Feuding");

	private static int HINT_InLabor = StringUtil.JenkinsHash("HINT_InLabor");

	private static int HINT_Encumbered = StringUtil.JenkinsHash("HINT_Encumbered");

	private static int HINT_BrainScanReadout = StringUtil.JenkinsHash("HINT_BrainScanReadout");

	private static int HINT_DemoTimeout = StringUtil.JenkinsHash("HINT_DemoTimeout");

	private static int HINT_Seconds = StringUtil.JenkinsHash("HINT_Seconds");

	private static float CharacterSwitchBarTime = 2f;

	private static float SkillsPopupTransitionTime = 0.25f;

	private void Awake()
	{
		Instance = this;
		base.gameObject.transform.localPosition = Vector3.zero;
		base.gameObject.transform.localRotation = Quaternion.identity;
		base.gameObject.transform.localScale = Vector3.one;
		GameObject gameObject = base.transform.Find("MainView").gameObject;
		GameObject gameObject2 = base.transform.Find("NamesPanel/PipView").gameObject;
		GameObject gameObject3 = base.transform.Find("MinimapView").gameObject;
		GameObject gameObject4 = base.transform.Find("OpinionGraph").gameObject;
		GameObject gameObject5 = base.transform.Find("Sidebar").gameObject;
		MainPanelRectTransform = (RectTransform)gameObject.transform;
		PipPanelRectTransform = (RectTransform)gameObject2.transform;
		MinimapPanelRectTransform = (RectTransform)gameObject3.transform;
		OpinionGraphRectTransform = (RectTransform)gameObject4.transform;
		HudPanelRectTransform = (RectTransform)base.transform;
		SidebarRectTransform = (RectTransform)gameObject5.transform;
		NamesPanelRectTransform = (RectTransform)base.transform.Find("NamesPanel");
		AdmireRectTransform = (RectTransform)base.transform.Find("OpinionGraph/Admire").transform;
		PityRectTransform = (RectTransform)base.transform.Find("OpinionGraph/Pity").transform;
		DespiseRectTransform = (RectTransform)base.transform.Find("OpinionGraph/Despise").transform;
		FearRectTransform = (RectTransform)base.transform.Find("OpinionGraph/Fear").transform;
		UnityMainImage = gameObject.GetComponent<RawImage>();
		UnityPipImage = gameObject2.GetComponent<RawImage>();
		UnityMinimapImage = gameObject3.GetComponent<RawImage>();
		UnityOpinionGraph = base.transform.Find("OpinionGraph").GetComponent<OpinionGraphBehaviour>();
		Size = HudPanelRectTransform.rect.size;
		Centre = HudPanelRectTransform.rect.size * 0.5f;
		UnityStatusBar = base.transform.Find("StatusBar").gameObject;
		UnityStatusBarSpacer = base.transform.Find("StatusBar/Spacer").gameObject;
		UnityNewNotifications = base.transform.Find("NewNotificationsPanel").gameObject;
		UnityNewNotificationsAmount = base.transform.Find("NewNotificationsPanel/Amount").gameObject.GetComponent<TextMeshProUGUI>();
		UnityNotification = base.transform.Find("NotificationPanel").gameObject;
		UnityCommunityStatNotification = base.transform.Find("CommunityStatNotification").gameObject;
		UnityCommunityStatNotificationStat = base.transform.Find("CommunityStatNotification/CommunityStatDisplay").GetComponent<CommunityStatBehaviour>();
		UnityEquipmentNotification = base.transform.Find("EquipmentNotificationPanel").gameObject;
		UnityEquipmentNotificationBG = base.transform.Find("EquipmentNotificationPanel/Background").GetComponent<RawImage>();
		UnityCharacterSwitchBar = base.transform.Find("CharacterSwitchBar").gameObject.GetComponent<CharacterSwitchBarBehaviour>();
		UnityCharacterSwitchBarContents = base.transform.Find("CharacterSwitchBar/Scroll View/Viewport/Content").gameObject;
		UnityCharacterSwitchBarLeftArrow = base.transform.Find("CharacterSwitchBar/LeftArrow").gameObject;
		UnityCharacterSwitchBarRightArrow = base.transform.Find("CharacterSwitchBar/RightArrow").gameObject;
		UnitySkillsChangePopupPanel = GameObject.Find("Launcher/SkillsChangePopupPanel").GetComponent<SkillsChangeBehaviour>();
		UnityNotificationTitle = base.transform.Find("NotificationPanel/TitlePanel/Title").gameObject.GetComponent<TextMeshProUGUI>();
		UnityNotificationQuestGroupName = base.transform.Find("NotificationPanel/QuestGroupName").gameObject.GetComponent<TextMeshProUGUI>();
		UnityNotificationPrompt = base.transform.Find("NotificationPanel/TitlePanel/Prompt").gameObject.GetComponent<TextMeshProUGUI>();
		UnityNotificationMessage = base.transform.Find("NotificationPanel/MessagePanel/MessageText").gameObject.GetComponent<TextMeshProUGUI>();
		UnityNotificationCloseButton = base.transform.Find("NotificationPanel/TitlePanel/CloseButton").gameObject.GetComponent<Image>();
		UnityNotificationIcon = base.transform.Find("NotificationPanel/MessagePanel/Icon").gameObject.GetComponent<RawImage>();
		UnityNotificationEventSystem = base.transform.Find("NotificationPanel/EventSystem").gameObject.GetComponent<EventSystem>();
		UnityEquipmentNotificationName = base.transform.Find("EquipmentNotificationPanel/MessagePanel/Name").gameObject.GetComponent<TextMeshProUGUI>();
		UnityEquipmentNotificationAmount = base.transform.Find("EquipmentNotificationPanel/MessagePanel/Amount").gameObject.GetComponent<TextMeshProUGUI>();
		UnityEquipmentNotificationIcon = base.transform.Find("EquipmentNotificationPanel/Icon").gameObject.GetComponent<EquipmentIconBehaviour>();
		UnityActionMenu = GameObject.Find("Launcher/ActionMenu").GetComponent<ActionMenuBehaviour>();
		UnityActionMenuLeftAligned = GameObject.Find("Launcher/ActionMenuLeftAligned").GetComponent<ActionMenuBehaviour>();
		UnitySpeechMenu = GameObject.Find("Launcher/SpeechMenu").GetComponent<ActionMenuBehaviour>();
		UnityScreenSpeechBubble = base.transform.Find("ScreenSpeechBubble").GetComponent<SpeechBubbleBehaviour>();
		UnityScreenSpeechBubble.IsPipBubble = true;
		UnityPauseIcon = base.transform.Find("PauseIcon").gameObject;
		UnityFastForwardIcon = base.transform.Find("FastForwardIcon").gameObject;
		UnityPlaySpeedText = base.transform.Find("PlaySpeedText").GetComponent<TextMeshProUGUI>();
		UnityBiohazardImage = base.transform.Find("Biohazard").gameObject.GetComponent<RawImage>();
		UnityInfectionBar = base.transform.Find("Infection").gameObject.GetComponent<ProgressBarBehaviour>();
		UnitySavingIcon = base.transform.Find("SavingIcon").gameObject.GetComponent<RawImage>();
		UnityMicrophoneIcon = base.transform.Find("MicrophoneIcon").gameObject.GetComponent<RawImage>();
		UnityWIPText = base.transform.Find("WIP").gameObject.GetComponent<TextMeshProUGUI>();
		UnityChatInputFieldBG = base.transform.Find("ChatInputField").gameObject;
		UnityChatInputFieldBG.SetActive(value: false);
		UnityChatInputField = UnityChatInputFieldBG.FindChild("InputField").GetComponent<TMP_InputField>();
		UnityVignette = base.transform.Find("MainView/Vignette").gameObject.GetComponent<RawImage>();
		UnityEquippedIcon = base.transform.Find("Sidebar/EquippedIcon").gameObject.GetComponent<EquipmentIconBehaviour>();
		UnityFullBodyIconImage = base.transform.Find("FullBodyIcon").gameObject.GetComponent<RawImage>();
		UnityBodyArmorIcon = base.transform.Find("Sidebar/BodyArmorIcon").gameObject.GetComponent<RawImage>();
		UnityHelmetIcon = base.transform.Find("Sidebar/HelmetIcon").gameObject.GetComponent<RawImage>();
		UnityLegArmorIcon = base.transform.Find("Sidebar/LegArmorIcon").gameObject.GetComponent<RawImage>();
		UnityPipCaption = base.transform.Find("NamesPanel/PipView/Caption").gameObject;
		UnityPipCaptionText = base.transform.Find("NamesPanel/PipView/Caption/Text").gameObject.GetComponent<TextMeshProUGUI>();
		UnityGoToPipObjectPrompt = base.transform.Find("NamesPanel/PipView/GoToPipObjectPrompt").gameObject.GetComponent<TextMeshProUGUI>();
		UnitySelectionInfo = base.transform.Find("SelectionInfo").gameObject.GetComponent<TextMeshProUGUI>();
		UnityInhabitantsPanel = base.transform.Find("NamesPanel/PipView/InhabitantsPanel").gameObject;
		UnityStarsPanel = base.transform.Find("NamesPanel/PipView/StarsPanel").gameObject;
		UnityPipPanel = base.transform.Find("NamesPanel").gameObject;
		UnityNamesPanel = base.transform.Find("NamesPanel/Background").gameObject.GetComponent<HalftoneBehaviour>();
		UnityNamesPanel.material = new Material(UnityNamesPanel.material);
		UnityDisplayName = base.transform.Find("NamesPanel/DisplayName").gameObject.GetComponent<TextMeshProUGUI>();
		UnitySubDisplayName = base.transform.Find("NamesPanel/SubDisplayName").gameObject.GetComponent<TextMeshProUGUI>();
		UnityControlledCharacterName = base.transform.Find("Sidebar/FullName").gameObject.GetComponent<TextMeshProUGUI>();
		UnityBloodLossTitle = base.transform.Find("Sidebar/BloodLossTitle").gameObject.GetComponent<TextMeshProUGUI>();
		UnityVehicleDamageTitle = base.transform.Find("Sidebar/VehicleDamageTitle").gameObject.GetComponent<TextMeshProUGUI>();
		UnityBloodLoss = base.transform.Find("Sidebar/BloodLoss").gameObject.GetComponent<ProgressBarBehaviour>();
		UnityBloodLossUnconscious = base.transform.Find("Sidebar/BloodLossUnconscious").gameObject.GetComponent<ProgressBarBehaviour>();
		UnityFatigueTitle = base.transform.Find("Sidebar/FatigueTitle").gameObject.GetComponent<TextMeshProUGUI>();
		UnityVehicleGasTitle = base.transform.Find("Sidebar/VehicleGasTitle").gameObject.GetComponent<TextMeshProUGUI>();
		UnityFatigue = base.transform.Find("Sidebar/Fatigue").gameObject.GetComponent<ProgressBarBehaviour>();
		UnityAdrenalineTitle = base.transform.Find("Sidebar/AdrenalineTitle").gameObject.GetComponent<TextMeshProUGUI>();
		UnityAdrenaline = base.transform.Find("Sidebar/Adrenaline").gameObject.GetComponent<ProgressBarBehaviour>();
		UnityLoadedAmmo = base.transform.Find("Sidebar/LoadedAmmo").gameObject.GetComponent<TextMeshProUGUI>();
		UnityTotalAmmo = base.transform.Find("Sidebar/TotalAmmo").gameObject.GetComponent<TextMeshProUGUI>();
		UnityAmmoTypeIcon = base.transform.Find("Sidebar/AmmoTypeIcon").gameObject.GetComponent<RawImage>();
		UnityShortcutsPanel = base.transform.Find("Sidebar/Shortcuts").gameObject;
		UnityTemperatureText = base.transform.Find("MinimapView/Temperature").gameObject.GetComponent<TextMeshProUGUI>();
		UnityMiniThermometer = base.transform.Find("MinimapView/MiniThermometer").gameObject.GetComponent<RawImage>();
		UnityMiniThermometerMercury = base.transform.Find("MinimapView/MiniThermometerMercury").gameObject.GetComponent<RawImage>();
		UnityWeatherVaneImage = base.transform.Find("MinimapView/WeatherVane").gameObject.GetComponent<RawImage>();
		UnityCompassImage = base.transform.Find("MinimapView/Compass").gameObject.GetComponent<RawImage>();
		for (int i = 0; i < 6; i++)
		{
			RawImage[] unityInjury = UnityInjury;
			int num = i;
			Transform obj = base.transform;
			InjuryLocation injuryLocation = (InjuryLocation)i;
			unityInjury[num] = obj.Find("FullBodyIcon/Injury" + injuryLocation).gameObject.GetComponent<RawImage>();
			UnityInjuryOriginalPos[i] = UnityInjury[i].rectTransform.anchoredPosition3D;
			Transform obj2 = base.transform;
			injuryLocation = (InjuryLocation)i;
			Transform transform = obj2.Find("FullBodyIcon/InjuryLine" + injuryLocation);
			if (transform != null)
			{
				UnityInjuryLine[i] = transform.GetComponent<RawImage>();
				UnityInjuryLineOriginalPos[i] = UnityInjuryLine[i].rectTransform.anchoredPosition3D;
			}
		}
		UnityAudioListener = GameObject.Find("AudioListener");
		UnitySelectionCircle = GameObject.Find("SelectionCircle");
		UnitySelectionRect = GameObject.Find("SelectionRect");
		UnityReticule = GameObject.Find("Reticule");
		UnityReticuleAnimator = UnityReticule.GetComponent<Animator>();
		UnityTargetBodyLocation = GameObject.Find("TargetBodyLocation");
		UnityTargetIndicator[0] = GameObject.Find("LeftTargetIndicator");
		UnityTargetIndicator[1] = GameObject.Find("RightTargetIndicator");
		UnityHealthBar = GameObject.Find("BloodLossBar");
		UnityCraftingProgressIndicator = GameObject.Find("CraftingProgressIndicator").GetComponent<RawImage>();
		UnityCraftingProgressIndicator.material = new Material(UnityCraftingProgressIndicator.material);
		UnityOverheadActionIcon = GameObject.Find("OverheadActionIcon").GetComponent<RawImage>();
		UnityOverheadActionIcon.material = new Material(UnityOverheadActionIcon.material);
		UnityParryPrompt = base.transform.Find("MainView/ParryPrompt").gameObject;
		UnityParryPromptTextDisplay = UnityParryPrompt.FindChild("TextDisplay");
		UnityParryPromptSuccessDisplay = UnityParryPrompt.FindChild("SuccessDisplay");
		UnityParryPromptText = UnityParryPrompt.FindChild("TextDisplay/ParryPromptText").GetComponent<TextMeshProUGUI>();
		UnityParryPromptSuccessText = UnityParryPrompt.FindChild("SuccessDisplay/SuccessText").GetComponent<TextMeshProUGUI>();
		UnityParryPromptImage = UnityParryPrompt.FindChild("TimeoutDisplay").GetComponent<RawImage>();
		UnityParryPromptImage.material = new Material(UnityParryPromptImage.material);
		UnityParryPromptBang = UnityParryPrompt.FindChild("SuccessDisplay/Bang").GetComponent<RawImage>();
		UnityParryPromptForbidden = UnityParryPrompt.FindChild("Forbidden").GetComponent<RawImage>();
		UnityGateInsideText = GameObject.Find("GateInsideText");
		UnityGateOutsideText = GameObject.Find("GateOutsideText");
		UnityGameCameraObj = GameObject.Find("GameCamera");
		UnityDecalLayersCameraObj = GameObject.Find("DecalLayersCamera");
		UnityPipCameraObj = GameObject.Find("PipCamera");
		UnityMinimapCameraBehaviour = GameObject.Find("MinimapCamera").GetComponent<MinimapCameraBehaviour>();
		UnityMapCameraObj = GameObject.Find("MapCamera");
		UnityGameCamera = UnityGameCameraObj.GetComponent<Camera>();
		UnityPostProcessingLayer = UnityGameCameraObj.GetComponent<PostProcessLayer>();
		UnityDecalLayersCamera = UnityDecalLayersCameraObj.GetComponent<Camera>();
		GameCameraNearClipDist = UnityGameCamera.nearClipPlane;
		GameCameraFarClipDist = UnityGameCamera.farClipPlane;
		SetFOV(PlayerPrefs.GetFloat("FOV", UnityGameCamera.fieldOfView));
		GameCameraAspect = UnityGameCamera.aspect;
		UnityPipCamera = UnityPipCameraObj.GetComponent<Camera>();
		UnityMinimapCamera = UnityMinimapCameraBehaviour.GetComponent<Camera>();
		UnityMapCamera = UnityMapCameraObj.GetComponent<Camera>();
		UnityGameCameraBehaviour = UnityGameCameraObj.GetComponent<GameCameraBehaviour>();
		UnityBeautifyBehaviour = UnityGameCameraObj.GetComponent<Beautify>();
		UnityBeautifyBehaviour.enabled = Sun.UseBeautify();
		UnityPipCameraBehaviour = UnityPipCameraObj.GetComponent<PipCameraBehaviour>();
		UnityGrassCameraBehaviour = UnityGameCameraObj.GetComponent<GrassCameraBehaviour>();
		UnityGrassCameraBehaviour.GrassRangeMax = PlayerPrefs.GetFloat("GrassRange", UnityGrassCameraBehaviour.GrassRangeMax);
		UnityPipGrassCameraBehaviour = UnityPipCameraObj.GetComponent<GrassCameraBehaviour>();
		UnityPipDoFCameraBehaviour = UnityPipCameraObj.GetComponent<DepthOfFieldDeprecated>();
		UnityFocusedOutlineBehaviour = new OutlineCameraBehaviour("FocusedOutline");
		UnityTargetOutlineBehaviour = new OutlineCameraBehaviour("TargetOutline");
		UnitySelectedOutlineBehaviour = new OutlineCameraBehaviour("SelectedOutline");
		UnityGameCameraFrostEffect = UnityGameCameraObj.GetComponent<FrostEffect>();
		GameCameraCullingMask = UnityGameCamera.cullingMask;
	}

	private void Start()
	{
		base.gameObject.SetActive(value: false);
		UnityNewNotifications.SetActive(value: false);
		UnityNotification.SetActive(value: false);
		UnityCommunityStatNotification.SetActive(value: false);
		UnityEquipmentNotification.SetActive(value: false);
		UnityCharacterSwitchBar.gameObject.SetActive(value: false);
		UnitySkillsChangePopupPanel.gameObject.SetActive(value: false);
		UnityGameCameraObj.SetActive(value: false);
		UnityPipCameraObj.SetActive(value: false);
		UnityMinimapCameraBehaviour.gameObject.SetActive(value: false);
		UnityMapCameraObj.SetActive(value: false);
		UnityAudioListener.SetActive(value: false);
		UnityInfectionBar.UnityFill.material = new Material(UnityInfectionBar.UnityFill.material);
	}

	public bool IsActive()
	{
		return base.gameObject.activeSelf;
	}

	public void OnActivate()
	{
		_ = Session.Instance;
		base.gameObject.SetActive(value: true);
		UnityPipCameraObj.SetActive(value: true);
		UnityGameCameraObj.SetActive(value: true);
		UnityAudioListener.SetActive(value: true);
		UnityWIPText.SetUnityText(string.Empty);
		Update();
	}

	public void OnDeactivate()
	{
		UnityAudioListener.SetActive(value: false);
		UnityGameCameraObj.SetActive(value: false);
		UnityPipCameraObj.SetActive(value: false);
		UnityChatInputFieldBG.SetActive(value: false);
		base.gameObject.SetActive(value: false);
	}

	public void SetFOV(float v)
	{
		Camera unityDecalLayersCamera = UnityDecalLayersCamera;
		float fieldOfView = (UnityGameCamera.fieldOfView = (GameCameraFOV = v));
		unityDecalLayersCamera.fieldOfView = fieldOfView;
	}

	private void UpdateMainPanelSizeAndPos()
	{
		InfoScreen instance = InfoScreen.Instance;
		if (GameImpl.Instance.Settings.SidebarLayoutEnabled)
		{
			MainPanelRectTransform.sizeDelta = new Vector2(Size.x - SidebarWidth, Size.y);
			MainPanelRectTransform.anchoredPosition = new Vector2(Mathf.Lerp(0f - SidebarWidth, Size.x - SidebarWidth, instance.Transition), 0f);
		}
		else
		{
			SmallMainPanelHeightOnScreen = Size.y - NamesPanelRectTransform.rect.height - SidebarRectTransform.rect.height - SmallMainPanelSpacingStuff;
			MainPanelRectTransform.sizeDelta = Vector2.Lerp(Size, new Vector2(SmallMainPanelWidthOnScreen, SmallMainPanelHeightOnScreen), instance.Transition);
			MainPanelRectTransform.anchoredPosition = Vector2.Lerp(Vector2.zero, new Vector2(0f, -484f), instance.Transition);
		}
		MainPanelRect = MainPanelRectTransform.rect;
		Camera unityDecalLayersCamera = UnityDecalLayersCamera;
		float gameCameraAspect = (UnityGameCamera.aspect = MainPanelRectTransform.sizeDelta.x / MainPanelRectTransform.sizeDelta.y);
		unityDecalLayersCamera.aspect = (GameCameraAspect = gameCameraAspect);
	}

	private void UpdateMainRenderTexture()
	{
		bool flag = !GameImpl.Instance.Settings.SidebarLayoutEnabled && InfoScreen.Instance.Transition >= 0.5f;
		UnityMainImage.texture = (flag ? SmallMainViewRenderTexture : MainViewRenderTexture);
		UnityGameCamera.targetTexture = (flag ? SmallMainViewRenderTexture : MainViewRenderTexture);
	}

	public void Update()
	{
		switch (CurrentChatState)
		{
		case ChatState.WantOpen:
			UnityChatInputFieldBG.SetActive(value: true);
			CurrentChatState = ChatState.Open;
			break;
		case ChatState.WantClosed:
			UnityChatInputFieldBG.SetActive(value: false);
			UnityChatInputField.SetUnityText("");
			CurrentChatState = ChatState.Closed;
			break;
		}
		GameImpl instance = GameImpl.Instance;
		Session instance2 = Session.Instance;
		Hud instance3 = Hud.Instance;
		InfoScreen instance4 = InfoScreen.Instance;
		if (instance2 == null || instance2.State != SessionState.Started)
		{
			return;
		}
		bool flag = instance.Settings.SidebarLayoutEnabled && instance4.ActiveAndFullyTransitionedIn;
		UnityGameCamera.cullingMask = ((!flag) ? GameCameraCullingMask : 0);
		UnityDecalLayersCamera.gameObject.SetActive(!instance4.ActiveAndFullyTransitionedIn && instance.Settings.HighQualityEffects && instance2.GameCamera.FlyCamTransition < 0.5f && BFX_BloodSettings.BloodDecalsActive > 0);
		UpdateMainPanelSizeAndPos();
		UpdateMainRenderTexture();
		Character localControlledCharacter = instance3.LocalControlledCharacter;
		EnterableVehicle enterableVehicle = ((localControlledCharacter != null) ? (localControlledCharacter.InsideBuilding as EnterableVehicle) : null);
		UnityAudioListener.transform.position = ((localControlledCharacter == null) ? instance2.GameCamera.Focus : (enterableVehicle?.CentreOfMass ?? Vector3.Lerp(localControlledCharacter.Pos + Mathf.Lerp(localControlledCharacter.Appearance.CrouchingEyeHeight, localControlledCharacter.Appearance.EyeHeight, localControlledCharacter.CrouchingTransition) * Vector3.up, instance2.GameCamera.Focus, instance2.GameCamera.FlyCamTransition)));
		UnityAudioListener.transform.rotation = UnityGameCamera.transform.rotation;
		UnityVignette.gameObject.SetActive(instance3.VignetteAmount > 0f);
		UnityVignette.color = new Color(0.5f, 0f, 0f, instance3.VignetteAmount);
		if (localControlledCharacter != null)
		{
			float t = 1f - Mathf.Clamp01((localControlledCharacter.GetBodyTemperatureInCelsius() - Character.BodyTemperatureInCelsiusUnconscious) / (Character.BodyTemperatureInCelsiusNormal - Character.BodyTemperatureInCelsiusUnconscious));
			UnityGameCameraFrostEffect.enabled = localControlledCharacter.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusShivering;
			UnityGameCameraFrostEffect.FrostAmount = Mathf.Lerp(0.2f, 0.5f, t);
		}
		else
		{
			UnityGameCameraFrostEffect.enabled = false;
		}
		UnityPauseIcon.SetActive(instance2.PlaySpeed == PlaySpeed.Paused && !NotificationManager.Instance.IsDisplayingNotification());
		UnityFastForwardIcon.SetActive(instance2.PlaySpeed >= PlaySpeed.FastForwardx2 && !NotificationManager.Instance.IsDisplayingNotification());
		UnityPlaySpeedText.gameObject.SetActive(instance2.PlaySpeed >= PlaySpeed.FastForwardx2 && !NotificationManager.Instance.IsDisplayingNotification());
		if (UnityPlaySpeedText.gameObject.activeSelf)
		{
			PlaySpeedText.Length = 0;
			PlaySpeedText.Append('x');
			PlaySpeedText.AppendWithoutGarbage((instance2.PlaySpeed == PlaySpeed.FastForwardx4) ? 4 : 2);
			UnityPlaySpeedText.SetUnityTextIfDifferent(PlaySpeedText);
		}
		UnitySavingIcon.gameObject.SetActive(SaveGameManager.Instance.WantSavingIcon());
		UnitySavingIcon.transform.localEulerAngles = new Vector3(0f, 0f, (0f - Time.unscaledTime) * 360f);
		UnityMicrophoneIcon.gameObject.SetActive(OnlineParty.Instance.PushToTalkButtonHeld);
		bool flag2 = GraphicsDebugMenu.ShowHUD || instance.Settings.SidebarLayoutEnabled;
		SidebarRectTransform.gameObject.SetActive(flag2);
		Equipment equipment = localControlledCharacter?.DesiredEquippedItem;
		Armor armor = ((localControlledCharacter != null) ? (localControlledCharacter.Clothes[6] as Armor) : null);
		Armor armor2 = ((localControlledCharacter != null) ? (localControlledCharacter.Clothes[0] as Armor) : null);
		Armor armor3 = ((localControlledCharacter != null) ? (localControlledCharacter.Clothes[7] as Armor) : null);
		UnityEquippedIcon.gameObject.SetActive(equipment != null);
		UnityEquippedIcon.Initialize(equipment, equipment?.InfectedWith ?? InfectionType.None);
		bool flag3 = localControlledCharacter != null && localControlledCharacter.GetInfectionProgression() > 0f;
		bool flag4 = localControlledCharacter != null && localControlledCharacter.GetAdrenaline() <= 0f;
		bool flag5 = localControlledCharacter != null && localControlledCharacter.GetAdrenaline() > 0f;
		int num = localControlledCharacter?.GetSkillLevelWithEffects(SkillType.Constitution) ?? 0;
		bool flag6 = enterableVehicle?.IsDriveable ?? false;
		UnityControlledCharacterName.gameObject.SetActive(localControlledCharacter != null);
		UnityBloodLossTitle.gameObject.SetActive(localControlledCharacter != null && !flag6);
		UnityVehicleDamageTitle.gameObject.SetActive(flag6);
		UnityBloodLoss.gameObject.SetActive(localControlledCharacter != null);
		UnityBloodLossUnconscious.gameObject.SetActive(num > 0);
		UnityFatigueTitle.gameObject.SetActive(flag4 && !flag6);
		UnityVehicleGasTitle.gameObject.SetActive(flag6);
		UnityFatigue.gameObject.SetActive(flag4 || flag6);
		UnityAdrenalineTitle.gameObject.SetActive(flag5 && !flag6);
		UnityAdrenaline.gameObject.SetActive(flag5 && !flag6);
		UnityBiohazardImage.gameObject.SetActive(flag3);
		UnityInfectionBar.gameObject.SetActive(flag3);
		UnityBodyArmorIcon.gameObject.SetActive(armor != null);
		UnityHelmetIcon.gameObject.SetActive(armor2 != null);
		UnityLegArmorIcon.gameObject.SetActive(armor3 != null);
		LoadedAmmoText.Length = 0;
		TotalAmmoText.Length = 0;
		ControlledCharacterNameText.Length = 0;
		if (localControlledCharacter != null)
		{
			if (flag6)
			{
				float damageFraction = enterableVehicle.GetDamageFraction();
				float num2 = enterableVehicle.GetLiquidAmount() / enterableVehicle.GetLiquidCapacity();
				UnityBloodLoss.SetValue(damageFraction);
				UnityBloodLoss.SetFlashing(damageFraction >= 0.9f);
				UnityFatigue.SetValue(num2);
				UnityFatigue.SetFlashing(num2 < EnterableVehicle.DefaultGasFracMin);
				LiquidPrototype liquidType = enterableVehicle.GetLiquidType();
				UnityVehicleGasTitle.SetUnityTextIfDifferent(GameImpl.Translate(liquidType?.NameHash ?? HUD_VehicleGas));
			}
			else
			{
				UnityBloodLoss.SetValue(Mathf.Clamp01(localControlledCharacter.GetBloodLoss()));
				UnityBloodLoss.SetFlashing(localControlledCharacter.HasUnbandagedInjury(0));
				UnityFatigue.SetValue(localControlledCharacter.GetFatigue());
				UnityFatigue.SetFlashing(localControlledCharacter.GetFatigue() >= Character.ExhaustedFatigueLevel);
				UnityAdrenaline.SetValue(localControlledCharacter.GetAdrenaline());
			}
			if (num > 0)
			{
				UnityBloodLossUnconscious.SetValue(Mathf.Clamp01((localControlledCharacter.GetBloodLoss() - 1f) / (float)num));
				UnityBloodLossUnconscious.SetWidth(num * 40);
				UnityBloodLossUnconscious.SetTicks(num - 1);
			}
			AmmoWeapon ammoWeapon = equipment as AmmoWeapon;
			if (ammoWeapon != null)
			{
				LoadedAmmoText.AppendWithoutGarbage(ammoWeapon.CurrentAmmo);
				LoadedAmmoText.Append('/');
				LoadedAmmoText.AppendWithoutGarbage(ammoWeapon.GetMaxAmmo());
			}
			else if (equipment != null && equipment.CanBeCombined())
			{
				LoadedAmmoText.AppendWithoutGarbage(equipment.GetAmount());
			}
			TotalAmmoText.Length = 0;
			if (ammoWeapon != null)
			{
				int number = ((ammoWeapon.CurrentAmmoType != null) ? localControlledCharacter.Inventory.GetAmmoCountOfType(ammoWeapon.CurrentAmmoType, ammoWeapon.InfectedWith, ammoWeapon) : localControlledCharacter.Inventory.GetAmmoCountForWeaponNotIncludingLoadedAmmo(ammoWeapon));
				TotalAmmoText.AppendWithoutGarbage(number);
			}
			if (flag3)
			{
				InfectionType worstInfectionTypeInProgression = localControlledCharacter.GetWorstInfectionTypeInProgression();
				Color32 infectionCol = GameTerrain.MinimapSettings.GetInfectionCol(worstInfectionTypeInProgression);
				Color32 infectionCol2 = GameTerrain.MinimapSettings.GetInfectionCol2(worstInfectionTypeInProgression);
				UnityBiohazardImage.color = infectionCol;
				UnityInfectionBar.SetValue(localControlledCharacter.GetInfectionProgression());
				UnityInfectionBar.UnityFill.material.color = infectionCol;
				UnityInfectionBar.UnityFill.material.SetColor(ShaderHash._DotColor, infectionCol2);
			}
			if (armor != null)
			{
				UnityBodyArmorIcon.color = armor.GetArmorIconCol();
			}
			if (armor2 != null)
			{
				UnityHelmetIcon.color = armor2.GetArmorIconCol();
			}
			if (armor3 != null)
			{
				UnityLegArmorIcon.color = armor3.GetArmorIconCol();
			}
			localControlledCharacter.BuildDisplayName(ControlledCharacterNameText, noStrangers: false, englishOnly: false);
		}
		UnityLoadedAmmo.SetUnityTextIfDifferent(LoadedAmmoText);
		UnityTotalAmmo.SetUnityTextIfDifferent(TotalAmmoText);
		UnityControlledCharacterName.SetUnityTextIfDifferent(ControlledCharacterNameText);
		EquipmentPrototype equipmentPrototype = equipment?.GetCurrentAmmoType();
		UnityAmmoTypeIcon.gameObject.SetActive(equipmentPrototype != null && equipmentPrototype.Tex != null);
		if (UnityAmmoTypeIcon.gameObject.activeSelf)
		{
			UnityAmmoTypeIcon.texture = equipmentPrototype.Tex.GetAsset();
			UnityAmmoTypeIcon.material = Hud.OutlineBlackMat;
		}
		UnityFullBodyIconImage.texture = localControlledCharacter?.GetFullIcon();
		UnityFullBodyIconImage.color = Color.white;
		UnityFullBodyIconImage.gameObject.SetActive(UnityFullBodyIconImage.texture != null && flag2);
		for (int i = 0; i < 6; i++)
		{
			bool flag7 = localControlledCharacter?.HasInjury((InjuryLocation)i) ?? false;
			UnityInjury[i].gameObject.SetActive(flag7);
			if (UnityInjuryLine[i] != null)
			{
				UnityInjuryLine[i].gameObject.SetActive(flag7);
			}
			if (flag7)
			{
				switch (localControlledCharacter.GetInjuryBandageLevel((InjuryLocation)i))
				{
				case -1:
					UnityInjury[i].uvRect = new Rect(0f + (float)((localControlledCharacter.Id + i) % 3) * (1f / 3f), 2f / 3f, 1f / 3f, 1f / 3f);
					break;
				case 0:
					UnityInjury[i].uvRect = new Rect(0f, 1f / 3f, 1f / 3f, 1f / 3f);
					break;
				case 1:
					UnityInjury[i].uvRect = new Rect(1f / 3f, 1f / 3f, 1f / 3f, 1f / 3f);
					break;
				case 2:
					UnityInjury[i].uvRect = new Rect(2f / 3f, 1f / 3f, 1f / 3f, 1f / 3f);
					break;
				case 3:
					UnityInjury[i].uvRect = new Rect(0f, 0f, 1f / 3f, 1f / 3f);
					break;
				case 4:
					UnityInjury[i].uvRect = new Rect(1f / 3f, 0f, 1f / 3f, 1f / 3f);
					break;
				case 5:
					UnityInjury[i].uvRect = new Rect(2f / 3f, 0f, 1f / 3f, 1f / 3f);
					break;
				}
				UnityInjury[i].rectTransform.anchoredPosition3D = UnityInjuryOriginalPos[i] * localControlledCharacter.Appearance.OverallScale;
				if (UnityInjuryLine[i] != null)
				{
					UnityInjuryLine[i].rectTransform.anchoredPosition3D = UnityInjuryLineOriginalPos[i] * localControlledCharacter.Appearance.OverallScale;
				}
			}
		}
		DisplayName.Length = 0;
		SubDisplayName.Length = 0;
		TileObject focusObject = instance3.Pip.FocusObject;
		if (focusObject != null)
		{
			focusObject.BuildDisplayName(DisplayName, InfoScreen.AllowViewInfoOnAnyone, englishOnly: false);
			focusObject.BuildSubDisplayName(SubDisplayName);
		}
		UnityDisplayName.SetUnityTextIfDifferent(DisplayName);
		UnitySubDisplayName.SetUnityTextIfDifferent(SubDisplayName);
		Color col = Color.white;
		Color col2 = Color.white;
		focusObject?.GetSubDisplayCol(out col, out col2);
		UnityNamesPanel.material.color = col;
		UnityNamesPanel.material.SetColor(ShaderHash._DotColor, col2);
		PipCaptionText.Length = 0;
		if (instance2.IsPaused(includeTransition: false))
		{
			PipCaptionText.Append(GameImpl.Translate(HUD_Paused));
		}
		else if (instance3.Pip.FocusObject != null && instance3.Pip.FocusObject != InfoScreen.Instance.CurrentObject && instance3.Pip.FocusObject != instance3.LocalTargetObject && instance3.Pip.FocusObject != instance3.CursorTargetObject && instance3.Pip.FocusObject != instance3.LocalControlledCharacterOrBuildingTheyAreIn && instance3.LocalControlledCharacterOrBuildingTheyAreIn != null && (instance3.Pip.FocusObject.PosXZ - instance3.LocalControlledCharacterOrBuildingTheyAreIn.PosXZ).magnitude >= 32f)
		{
			PipCaptionText.Append(GameImpl.Translate(HUD_Meanwhile));
		}
		else if (instance2.PlaySpeed == PlaySpeed.FastForwardx2)
		{
			PipCaptionText.Append('x');
			PipCaptionText.Append('2');
		}
		else if (instance2.PlaySpeed == PlaySpeed.FastForwardx4)
		{
			PipCaptionText.Append('x');
			PipCaptionText.Append('4');
		}
		UnityPipCaption.SetActive(PipCaptionText.Length > 0);
		if (UnityPipCaption.activeSelf)
		{
			UnityPipCaptionText.SetUnityTextIfDifferent(PipCaptionText);
		}
		GoToPipObjectText.Length = 0;
		if (instance3.Pip.CanUseRightStickToGoToPipObject)
		{
			GoToPipObjectText.AppendButtonPromptString(InputFunction.ControlPipCharacter);
		}
		UnityGoToPipObjectPrompt.gameObject.SetActive(GoToPipObjectText.Length > 0);
		if (UnityGoToPipObjectPrompt.gameObject.activeSelf)
		{
			UnityGoToPipObjectPrompt.SetUnityTextIfDifferent(GoToPipObjectText);
		}
		int num3 = 0;
		if (focusObject is Building building)
		{
			for (int j = 0; j < building.Inhabitants.Length; j++)
			{
				if (building.Inhabitants[j] != null)
				{
					CharacterIconBehaviour characterIconBehaviour;
					if (num3 < UnityInhabitants.Count)
					{
						characterIconBehaviour = UnityInhabitants[num3];
					}
					else
					{
						characterIconBehaviour = UnityEngine.Object.Instantiate(InfoScreen.CharacterIcon.GetAsset(), UnityInhabitantsPanel.transform, worldPositionStays: false).GetComponent<CharacterIconBehaviour>();
						characterIconBehaviour.interactable = false;
						characterIconBehaviour.IsHudIcon = true;
						UnityInhabitants.Add(characterIconBehaviour);
					}
					characterIconBehaviour.Initialize(building.Inhabitants[j]);
					num3++;
				}
			}
		}
		while (UnityInhabitants.Count > num3)
		{
			UnityEngine.Object.Destroy(UnityInhabitants[num3].gameObject);
			UnityInhabitants.RemoveAt(num3);
		}
		PlantableCrop plantableCrop = focusObject as PlantableCrop;
		UnityStarsPanel.SetActive(plantableCrop != null);
		if (plantableCrop != null)
		{
			for (int k = 0; k < UnityStarsPanel.transform.childCount; k++)
			{
				RawImage component = UnityStarsPanel.transform.GetChild(k).GetComponent<RawImage>();
				if (k < plantableCrop.FarmerSkillLevel)
				{
					component.color = new Color32(111, 147, byte.MaxValue, byte.MaxValue);
				}
				else
				{
					component.color = Color.gray;
				}
			}
		}
		if (localControlledCharacter != null)
		{
			InputFunctionManager instance5 = InputFunctionManager.Instance;
			Equipment bestMeleeWeaponForShortcut = localControlledCharacter.GetBestMeleeWeaponForShortcut();
			EquipmentPrototype ammoType;
			InfectionType infectedWith;
			Equipment bestAmmoWeaponForShortcut = localControlledCharacter.GetBestAmmoWeaponForShortcut(out ammoType, out infectedWith);
			Equipment bestThrowableForShortcut = localControlledCharacter.GetBestThrowableForShortcut();
			for (int l = 0; l < 3; l++)
			{
				Equipment equipment2 = l switch
				{
					1 => bestAmmoWeaponForShortcut, 
					0 => bestMeleeWeaponForShortcut, 
					_ => bestThrowableForShortcut, 
				};
				InputFunction inputFunction = l switch
				{
					1 => InputFunction.SelectRangedWeapon, 
					0 => InputFunction.SelectMeleeWeapon, 
					_ => InputFunction.SelectThrowingWeapon, 
				};
				bool flag8 = equipment2 != null && instance5.IsMapped(inputFunction);
				UnityShortcutsPanel.transform.GetChild(l).gameObject.SetActive(flag8);
				if (flag8)
				{
					RawImage component2 = UnityShortcutsPanel.transform.GetChild(l).GetChild(0).GetComponent<RawImage>();
					TextMeshProUGUI component3 = UnityShortcutsPanel.transform.GetChild(l).GetChild(1).GetComponent<TextMeshProUGUI>();
					ShortcutText.Length = 0;
					ShortcutText.AppendButtonPromptString(inputFunction);
					component3.SetUnityTextIfDifferent(ShortcutText);
					if (equipment2.GetPrototype().Tex != null && (bool)equipment2.GetPrototype().Tex.GetAsset())
					{
						component2.texture = (Texture2D)equipment2.GetPrototype().Tex;
					}
				}
			}
		}
		UnitySelectionInfo.gameObject.SetActive(instance3.SelectedCharacters.Count > 0 && instance2.GameCamera.FlyCam);
		if (UnitySelectionInfo.gameObject.activeSelf)
		{
			SelectionInfoText.Length = 0;
			SelectionInfoText.Append(GameImpl.Translate(HUD_SelectedCount));
			SelectionInfoText.Append(' ');
			SelectionInfoText.Append(instance3.SelectedCharacters.Count);
			UnitySelectionInfo.SetUnityTextIfDifferent(SelectionInfoText);
		}
		bool flag9 = instance3.Pip.FocusObject != null;
		UnityPipPanel.SetActive(flag9);
		UnityPipCamera.gameObject.SetActive(flag9 && (instance.Settings.PiPBackgroundEnabled || !GraphicsDebugMenu.PipCommandBuffers));
		if (!instance.Settings.HighQualityEffects && !instance.Settings.PiPBackgroundEnabled)
		{
			instance.UnityReflectionCameraObj.SetActive(!flag9);
			instance.UnityRefractionCameraObj.SetActive(!flag9);
			instance.Sun.UnityReflectionProbe.refreshMode = ((!flag9) ? ReflectionProbeRefreshMode.EveryFrame : ReflectionProbeRefreshMode.ViaScripting);
		}
		else
		{
			instance.UnityReflectionCameraObj.SetActive(value: true);
			instance.UnityRefractionCameraObj.SetActive(value: true);
			instance.Sun.UnityReflectionProbe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
		}
		UpdateMinimapPanel();
		UpdateUnityPipSpeechBubble();
		UpdateUnityStatusBar();
		UpdateCharacterSwitchBar();
		UpdateSkillsChangePopupPanel();
		UpdateUnityQuestDestinationArrows();
		Color color = Color.Lerp(Color.white, new Color(0.25f, 0.25f, 0.25f, 1f), NotificationManager.Instance.DesiredScreenDarkenAmount);
		UnityMainImage.color = color;
	}

	public void UpdateUnityQuestDestinationArrow(int i, Vector3 pos, Color col)
	{
		RawImage rawImage = null;
		if (i < UnityQuestDestinationArrows.Count)
		{
			rawImage = UnityQuestDestinationArrows[i];
		}
		else
		{
			rawImage = UnityEngine.Object.Instantiate(Hud.QuestDestinationArrow.GetAsset()).GetComponent<RawImage>();
			UnityQuestDestinationArrows.Add(rawImage);
		}
		rawImage.transform.position = pos;
		rawImage.transform.rotation = UnityGameCamera.transform.rotation;
		float magnitude = MathUtil.ToXZ(rawImage.transform.position - UnityGameCamera.transform.position).magnitude;
		rawImage.color = col * new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((magnitude - UnityGameCameraBehaviour.FogOfWarBehaviour.FogStart) / (UnityGameCameraBehaviour.FogOfWarBehaviour.FogEnd - UnityGameCameraBehaviour.FogOfWarBehaviour.FogStart)));
	}

	public void UpdateUnityQuestDestinationArrows()
	{
		StoryManager instance = StoryManager.Instance;
		PlayerRecord localPlayerRecord = Session.Instance.GetLocalPlayerRecord();
		int num = 0;
		foreach (TileObject questDestination in instance.QuestDestinations)
		{
			TileObject tileObject = questDestination;
			Character character = tileObject as Character;
			if (character != null)
			{
				character = character.GetPredictedOrElseThisCharacter();
			}
			if (character != null && character.InsideBuilding != null)
			{
				tileObject = character.InsideBuilding;
				character = null;
			}
			Vector3 pos = ((character == null) ? tileObject.GetCentreTop() : (character.Pos + new Vector3(0f, character.Height + character.Unity.OverheadIconsTopY, 0f)));
			UpdateUnityQuestDestinationArrow(num, pos, Color.white);
			num++;
		}
		if (localPlayerRecord != null)
		{
			for (int i = 0; i < localPlayerRecord.MapMarkerLocations.Count; i++)
			{
				Vector3 tileCentrePos = GameTerrain.Instance.GetTileCentrePos(localPlayerRecord.MapMarkerLocations[i].Tile);
				UpdateUnityQuestDestinationArrow(num, tileCentrePos, MapPage.GetMapMarkerColor(localPlayerRecord.MapMarkerLocations[i].Type));
				num++;
			}
		}
		while (UnityQuestDestinationArrows.Count > num)
		{
			UnityEngine.Object.Destroy(UnityQuestDestinationArrows[num].gameObject);
			UnityQuestDestinationArrows.RemoveAt(num);
		}
	}

	private void UpdateMinimapPanel()
	{
		GameImpl instance = GameImpl.Instance;
		Hud instance2 = Hud.Instance;
		Weather weather = Session.Instance.Weather;
		bool flag = GraphicsDebugMenu.ShowHUD || instance.Settings.SidebarLayoutEnabled;
		bool flag2 = instance2.Cursor.WantDisplayBrainScanInMinimap() && flag;
		bool flag3 = !flag2 && flag;
		UnityOpinionGraph.gameObject.SetActive(flag2);
		UnityMinimapImage.gameObject.SetActive(flag3);
		UnityTemperatureText.gameObject.SetActive(flag3);
		UnityMiniThermometer.gameObject.SetActive(flag3);
		UnityWeatherVaneImage.gameObject.SetActive(flag3);
		UnityCompassImage.gameObject.SetActive(flag3);
		if (flag2)
		{
			Character localControlledCharacter = instance2.LocalControlledCharacter;
			Character character = instance2.LocalTargetObject as Character;
			Speech cursorSpeech = instance2.Cursor.GetCursorSpeech();
			BaseObject target = instance2.Cursor.GetAvailableAction().Target;
			UnityOpinionGraph.Init(character, localControlledCharacter, cursorSpeech, target);
		}
		else if (flag3)
		{
			bool useCelsius = instance.Settings.UseCelsius;
			TemperatureText.Length = 0;
			TemperatureText.AppendWithoutGarbage(Mathf.RoundToInt(useCelsius ? weather.TemperatureInCelsius : MathUtil.CelsiusToFahrenheit(weather.TemperatureInCelsius)));
			TemperatureText.Append(GameImpl.Translate(useCelsius ? HUD_DegreesC : HUD_DegreesF));
			UnityTemperatureText.SetUnityTextIfDifferent(TemperatureText);
			RectTransform rectTransform = (RectTransform)UnityMiniThermometer.transform;
			float num = Weather.AverageDaytimeWinterTemperatureInCelsius - Weather.DiurnalTemperatureVariationInCelsius - Weather.TemperatureRangeInCelsius;
			float num2 = Weather.AverageDaytimeSummerTemperatureInCelsius + Weather.DiurnalTemperatureVariationInCelsius + Weather.TemperatureRangeInCelsius;
			float t = (weather.TemperatureInCelsius - num) / (num2 - num);
			float num3 = Mathf.Lerp(MercuryBottomPixel, MercuryTopPixel, t) / HeightInPixels;
			UnityMiniThermometerMercury.rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, rectTransform.rect.height * (1f - num3));
			UnityMiniThermometerMercury.uvRect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f - num3));
			if (instance.Settings.MinimapRotationEnabled)
			{
				UnityWeatherVaneImage.transform.eulerAngles = new Vector3(0f, 0f, UnityGameCamera.transform.eulerAngles.y - weather.GetWindAngleDeg());
				UnityCompassImage.transform.eulerAngles = new Vector3(0f, 0f, UnityGameCamera.transform.eulerAngles.y - 180f);
			}
			else
			{
				UnityWeatherVaneImage.transform.eulerAngles = new Vector3(0f, 0f, 180f - weather.GetWindAngleDeg());
				UnityCompassImage.transform.eulerAngles = new Vector3(0f, 0f, 0f);
			}
		}
	}

	public static float GetMinimapSize()
	{
		if (!GameImpl.Instance.IsSteamDeck())
		{
			return MinimapSize;
		}
		return SteamDeckMinimapSize;
	}

	public void OnScreenResized()
	{
		GameImpl instance = GameImpl.Instance;
		HudPanelRectTransform.sizeDelta = Size;
		bool sidebarLayoutEnabled = GameImpl.Instance.Settings.SidebarLayoutEnabled;
		bool piPBackgroundEnabled = GameImpl.Instance.Settings.PiPBackgroundEnabled;
		UnityPipCamera.cullingMask = (piPBackgroundEnabled ? ((1 << Character.DefaultLayer) | (1 << Character.TransparentFXLayer) | (1 << Character.IgnoreRaycastLayer) | (1 << Character.WaterLayer) | (1 << Character.CharactersLayer)) : (1 << Character.PipLayer));
		UnityPipCameraBehaviour.FogOfWarBehaviour.FogEnabled = piPBackgroundEnabled;
		UnityPipDoFCameraBehaviour.enabled = piPBackgroundEnabled;
		UnityPipGrassCameraBehaviour.enabled = piPBackgroundEnabled;
		UnityPostProcessingLayer.enabled = GameImpl.TAA;
		UnityPipCameraObj.GetComponent<FlareLayer>().enabled = piPBackgroundEnabled;
		SidebarRectTransform.anchoredPosition = (sidebarLayoutEnabled ? Vector2.zero : new Vector2(-8f, 8f));
		SidebarRectTransform.sizeDelta = (sidebarLayoutEnabled ? new Vector2(SidebarWidth, Size.y) : new Vector2(396f, 232f));
		UnityFullBodyIconImage.rectTransform.anchoredPosition = SidebarRectTransform.anchoredPosition + new Vector2(-92f, 118f);
		UnitySavingIcon.rectTransform.anchoredPosition = (GameImpl.Instance.IsSteamDeck() ? new Vector2(469f, -398f) : new Vector2(330f, -485f));
		MinimapPanelRectTransform.sizeDelta = (sidebarLayoutEnabled ? new Vector2(SidebarMinimapWidth, SidebarMinimapWidth + Math.Max(0f, Size.y - GameImpl.DefaultReferenceHeight)) : new Vector2(GetMinimapSize(), GetMinimapSize()));
		MinimapPanelRectTransform.anchoredPosition = (sidebarLayoutEnabled ? new Vector2(Size.x - SidebarWidth + 12f, 206f) : new Vector2(8f, 8f));
		MinimapPanelRectTransform.gameObject.GetShadowComponent().enabled = !sidebarLayoutEnabled;
		OpinionGraphRectTransform.sizeDelta = MinimapPanelRectTransform.sizeDelta;
		OpinionGraphRectTransform.anchoredPosition = MinimapPanelRectTransform.anchoredPosition;
		OpinionGraphRectTransform.gameObject.GetShadowComponent().enabled = !sidebarLayoutEnabled;
		AdmireRectTransform.localScale = Vector3.one * (sidebarLayoutEnabled ? 1f : (2f / 3f));
		PityRectTransform.localScale = Vector3.one * (sidebarLayoutEnabled ? 1f : (2f / 3f));
		DespiseRectTransform.localScale = Vector3.one * (sidebarLayoutEnabled ? 1f : (2f / 3f));
		FearRectTransform.localScale = Vector3.one * (sidebarLayoutEnabled ? 1f : (2f / 3f));
		AdmireRectTransform.anchoredPosition = OpinionGraphRectTransform.sizeDelta * new Vector2(1f, 1f) * 0.25f;
		PityRectTransform.anchoredPosition = OpinionGraphRectTransform.sizeDelta * new Vector2(-1f, 1f) * 0.25f;
		DespiseRectTransform.anchoredPosition = OpinionGraphRectTransform.sizeDelta * new Vector2(-1f, -1f) * 0.25f;
		FearRectTransform.anchoredPosition = OpinionGraphRectTransform.sizeDelta * new Vector2(1f, -1f) * 0.25f;
		UnityControlledCharacterName.enabled = !sidebarLayoutEnabled;
		UpdateMainPanelSizeAndPos();
		if (Hud.Opaque != null && Hud.Opaque.GetAsset() != null)
		{
			UnityPipImage.material = (piPBackgroundEnabled ? Hud.Opaque : Hud.HalftonePip);
			UnityMinimapImage.material = (sidebarLayoutEnabled ? Hud.Opaque.GetAsset() : null);
		}
		MainPanelWidthOnScreen = (int)(MainPanelRectTransform.rect.width * instance.ScreenScale);
		PipPanelSizeOnScreen = (int)(PipPanelRectTransform.rect.width * instance.ScreenScale);
		MinimapPanelWidthOnScreen = (int)(MinimapPanelRectTransform.rect.width * instance.ScreenScale);
		MinimapPanelHeightOnScreen = (int)(MinimapPanelRectTransform.rect.height * instance.ScreenScale);
		float num = ((!GameImpl.SuperSample) ? 1 : 2);
		Vector2Int mainPanelSizeWithoutSuperSample = instance.MainPanelSizeWithoutSuperSample;
		mainPanelSizeWithoutSuperSample.x = Mathf.RoundToInt((float)mainPanelSizeWithoutSuperSample.x * num);
		mainPanelSizeWithoutSuperSample.y = Mathf.RoundToInt((float)mainPanelSizeWithoutSuperSample.y * num);
		if (MainViewRenderTexture == null || MainViewRenderTexture.width != mainPanelSizeWithoutSuperSample.x || MainViewRenderTexture.height != mainPanelSizeWithoutSuperSample.y)
		{
			if (MainViewRenderTexture != null)
			{
				MainViewRenderTexture.Release();
			}
			MainViewRenderTexture = new RenderTexture(mainPanelSizeWithoutSuperSample.x, mainPanelSizeWithoutSuperSample.y, 24, RenderTextureFormat.ARGB32);
			MainViewRenderTexture.Create();
		}
		int num2 = (int)(SmallMainPanelWidthOnScreen * instance.ScreenScale * num);
		int num3 = (int)(SmallMainPanelHeightOnScreen * instance.ScreenScale * num);
		if (SmallMainViewRenderTexture == null || SmallMainViewRenderTexture.width != num2 || SmallMainViewRenderTexture.height != num3)
		{
			if (SmallMainViewRenderTexture != null)
			{
				SmallMainViewRenderTexture.Release();
			}
			SmallMainViewRenderTexture = new RenderTexture(num2, num3, 24, RenderTextureFormat.ARGB32);
			SmallMainViewRenderTexture.Create();
		}
		int num4 = (int)((float)MainViewRenderTexture.width * 0.25f);
		int num5 = (int)((float)MainViewRenderTexture.height * 0.25f);
		if (DecalLayerRenderTexture == null || DecalLayerRenderTexture.width != num4 || DecalLayerRenderTexture.height != num5)
		{
			if (DecalLayerRenderTexture != null)
			{
				DecalLayerRenderTexture.Release();
			}
			DecalLayerRenderTexture = new RenderTexture(num4, num5, 24, RenderTextureFormat.Depth);
			DecalLayerRenderTexture.Create();
		}
		UnityDecalLayersCamera.targetTexture = DecalLayerRenderTexture;
		Shader.SetGlobalTexture("_LayerDecalDepthTexture", DecalLayerRenderTexture);
		Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS");
		Shader.EnableKeyword("USE_CUSTOM_DECAL_LAYERS_IGNORE_MODE");
		UpdateMainRenderTexture();
		UnityDecalLayersCamera.fieldOfView = UnityGameCamera.fieldOfView;
		int num6 = Mathf.RoundToInt((float)PipPanelSizeOnScreen * num);
		int num7 = Mathf.RoundToInt((float)PipPanelSizeOnScreen * num);
		if (PipViewRenderTexture == null || PipViewRenderTexture.width != num6 || PipViewRenderTexture.height != num7)
		{
			if (PipViewRenderTexture != null)
			{
				PipViewRenderTexture.Release();
			}
			PipViewRenderTexture = new RenderTexture(num6, num7, 24, RenderTextureFormat.ARGB32);
			PipViewRenderTexture.Create();
		}
		UnityPipImage.texture = PipViewRenderTexture;
		UnityPipCamera.targetTexture = PipViewRenderTexture;
		UnityPipCamera.fieldOfView = 45f;
		UnityPipCamera.aspect = (float)PipViewRenderTexture.width / (float)PipViewRenderTexture.height;
		int minimapPanelWidthOnScreen = MinimapPanelWidthOnScreen;
		int minimapPanelHeightOnScreen = MinimapPanelHeightOnScreen;
		if (MinimapViewRenderTexture == null || MinimapViewRenderTexture.width != minimapPanelWidthOnScreen || MinimapViewRenderTexture.height != minimapPanelHeightOnScreen)
		{
			if (MinimapViewRenderTexture != null)
			{
				MinimapViewRenderTexture.Release();
			}
			MinimapViewRenderTexture = new RenderTexture(minimapPanelWidthOnScreen, minimapPanelHeightOnScreen, 24, RenderTextureFormat.ARGB32);
			MinimapViewRenderTexture.Create();
		}
		UnityMinimapImage.texture = MinimapViewRenderTexture;
		UnityMinimapCamera.targetTexture = MinimapViewRenderTexture;
		UnityMinimapCamera.fieldOfView = 90f;
		UnityMinimapCamera.aspect = (float)MinimapViewRenderTexture.width / (float)MinimapViewRenderTexture.height;
	}

	public static Color GetTraitColor(float trait)
	{
		if (!(trait < 0.5f))
		{
			return Color.Lerp(TraitMediumCol, TraitHighCol, trait * 2f - 1f);
		}
		return Color.Lerp(TraitLowCol, TraitMediumCol, trait * 2f);
	}

	private void EndRecipe(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		EndRecipeLine(unityActionObj, ref lineLength, ref lineIdx, ref idx2);
		while (lineIdx < unityActionObj.transform.childCount)
		{
			UnityEngine.Object.Destroy(unityActionObj.transform.GetChild(lineIdx).gameObject);
			lineIdx++;
		}
	}

	private void EndRecipeLine(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		if (lineIdx < unityActionObj.transform.childCount)
		{
			GameObject gameObject = unityActionObj.transform.GetChild(lineIdx).gameObject;
			while (idx2 < gameObject.transform.childCount)
			{
				UnityEngine.Object.Destroy(gameObject.transform.GetChild(idx2).gameObject);
				idx2++;
			}
			lineIdx++;
			idx2 = 0;
			lineLength = 0f;
		}
	}

	private GameObject GetRecipeLine(GameObject unityActionObj, float expectedWidth, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		if (lineLength + expectedWidth > MaxLineLength)
		{
			EndRecipeLine(unityActionObj, ref lineLength, ref lineIdx, ref idx2);
		}
		GameObject gameObject = ((lineIdx < unityActionObj.transform.childCount) ? unityActionObj.transform.GetChild(lineIdx).gameObject : null);
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.CraftActionLine, unityActionObj.transform, worldPositionStays: false);
			gameObject.transform.SetSiblingIndex(lineIdx);
		}
		return gameObject;
	}

	private TextMeshProUGUI AddRecipeText(GameObject unityActionObj, string text, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeTextTimer))
		{
			TextMeshProUGUI textMeshProUGUI = null;
			float num = 0f;
			if (lineIdx < unityActionObj.transform.childCount)
			{
				GameObject gameObject = unityActionObj.transform.GetChild(lineIdx).gameObject;
				if (idx2 < gameObject.transform.childCount)
				{
					TextMeshProUGUI component = gameObject.transform.GetChild(idx2).GetComponent<TextMeshProUGUI>();
					if (component != null && component.text == text)
					{
						textMeshProUGUI = component;
					}
				}
				else if (lineIdx + 1 < unityActionObj.transform.childCount)
				{
					GameObject gameObject2 = unityActionObj.transform.GetChild(lineIdx + 1).gameObject;
					if (gameObject2.transform.childCount > 0)
					{
						TextMeshProUGUI component2 = gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
						if (component2 != null && component2.text == text)
						{
							textMeshProUGUI = component2;
						}
					}
				}
			}
			num = ((!(textMeshProUGUI != null)) ? Hud.RecipeText.GetAsset().GetComponent<TextMeshProUGUI>().GetPreferredValues(text)
				.x : textMeshProUGUI.rectTransform.rect.width);
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			if (textMeshProUGUI == null)
			{
				GameObject obj = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeText, recipeLine.transform, worldPositionStays: false);
				obj.transform.SetSiblingIndex(idx2);
				textMeshProUGUI = obj.GetComponent<TextMeshProUGUI>();
				textMeshProUGUI.SetUnityText(text);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return textMeshProUGUI;
		}
	}

	private TextMeshProUGUI AddRecipeText(GameObject unityActionObj, StringBuilder sb, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeTextTimer))
		{
			TextMeshProUGUI textMeshProUGUI = null;
			float num = 0f;
			if (lineIdx < unityActionObj.transform.childCount)
			{
				GameObject gameObject = unityActionObj.transform.GetChild(lineIdx).gameObject;
				if (idx2 < gameObject.transform.childCount)
				{
					TextMeshProUGUI component = gameObject.transform.GetChild(idx2).GetComponent<TextMeshProUGUI>();
					if (component != null && sb.AreContentsIdentical(component.text))
					{
						textMeshProUGUI = component;
					}
				}
				else if (lineIdx + 1 < unityActionObj.transform.childCount)
				{
					GameObject gameObject2 = unityActionObj.transform.GetChild(lineIdx + 1).gameObject;
					if (gameObject2.transform.childCount > 0)
					{
						TextMeshProUGUI component2 = gameObject2.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
						if (component2 != null && sb.AreContentsIdentical(component2.text))
						{
							textMeshProUGUI = component2;
						}
					}
				}
			}
			string text = null;
			if (textMeshProUGUI != null)
			{
				text = textMeshProUGUI.text;
				num = textMeshProUGUI.rectTransform.rect.width;
			}
			else
			{
				text = sb.ToString();
				num = Hud.RecipeText.GetAsset().GetComponent<TextMeshProUGUI>().GetPreferredValues(text)
					.x;
			}
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			if (textMeshProUGUI == null)
			{
				GameObject obj = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeText, recipeLine.transform, worldPositionStays: false);
				obj.transform.SetSiblingIndex(idx2);
				textMeshProUGUI = obj.GetComponent<TextMeshProUGUI>();
				textMeshProUGUI.SetUnityText(text);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return textMeshProUGUI;
		}
	}

	private RawImage AddRecipeIcon(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeIconTimer))
		{
			float num = 36f;
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			GameObject gameObject = ((idx2 < recipeLine.transform.childCount) ? recipeLine.transform.GetChild(idx2).gameObject : null);
			if (gameObject == null || gameObject.GetComponent<RawImage>() == null || gameObject.transform.childCount != 0)
			{
				gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeItem, recipeLine.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(idx2);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return gameObject.GetComponent<RawImage>();
		}
	}

	private RawImage AddRecipeLiquidContainer(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeLiquidContainerTimer))
		{
			float num = 36f;
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			GameObject gameObject = ((idx2 < recipeLine.transform.childCount) ? recipeLine.transform.GetChild(idx2).gameObject : null);
			if (gameObject == null || gameObject.GetComponent<RawImage>() == null || gameObject.transform.childCount != 1)
			{
				gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeLiquidContainer, recipeLine.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(idx2);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return gameObject.GetComponent<RawImage>();
		}
	}

	private RawImage AddRecipeInfectedLiquidContainer(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeLiquidContainerTimer))
		{
			float num = 36f;
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			GameObject gameObject = ((idx2 < recipeLine.transform.childCount) ? recipeLine.transform.GetChild(idx2).gameObject : null);
			if (gameObject == null || gameObject.GetComponent<RawImage>() == null || gameObject.transform.childCount != 2)
			{
				gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeInfectedLiquidContainer, recipeLine.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(idx2);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return gameObject.GetComponent<RawImage>();
		}
	}

	private void AddRecipeSpacer(GameObject unityActionObj, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeSpacerTimer))
		{
			float num = 16f;
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			GameObject gameObject = ((idx2 < recipeLine.transform.childCount) ? recipeLine.transform.GetChild(idx2).gameObject : null);
			if (gameObject == null || gameObject.transform.childCount != 0 || gameObject.GetComponent<RawImage>() != null || gameObject.GetComponent<TextMeshProUGUI>() != null)
			{
				gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeSpacer, recipeLine.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(idx2);
			}
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
		}
	}

	private TextMeshProUGUI AddRecipeTextIconPair(GameObject unityActionObj, string text, Texture2D icon, bool selected, ref float lineLength, ref int lineIdx, ref int idx2)
	{
		using (new ProfileMarker(AddRecipeTextIconPairTimer))
		{
			float num = Hud.RecipeTextIconPair.GetAsset().transform.GetChild(1).GetComponent<TextMeshProUGUI>().GetPreferredValues(text)
				.x + 36f + 4f;
			GameObject recipeLine = GetRecipeLine(unityActionObj, num, ref lineLength, ref lineIdx, ref idx2);
			GameObject gameObject = ((idx2 < recipeLine.transform.childCount) ? recipeLine.transform.GetChild(idx2).gameObject : null);
			TextMeshProUGUI textMeshProUGUI = ((gameObject != null && gameObject.transform.childCount == 2) ? gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>() : null);
			RawImage rawImage = ((gameObject != null && gameObject.transform.childCount == 2) ? gameObject.transform.GetChild(0).GetComponent<RawImage>() : null);
			if (gameObject == null || textMeshProUGUI == null || rawImage == null)
			{
				gameObject = UnityEngine.Object.Instantiate((GameObject)Hud.RecipeTextIconPair, recipeLine.transform, worldPositionStays: false);
				gameObject.transform.SetSiblingIndex(idx2);
				textMeshProUGUI = gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
				rawImage = gameObject.transform.GetChild(0).GetComponent<RawImage>();
			}
			textMeshProUGUI.SetUnityText(text);
			rawImage.texture = icon;
			rawImage.material = (selected ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
			lineLength += ((idx2 == 0) ? 0f : 4f) + num;
			idx2++;
			return textMeshProUGUI;
		}
	}

	private void BuildEquipmentNameString(Equipment equipment, int amount, StringBuilder sb)
	{
		sb.Length = 0;
		equipment.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
		if (equipment.GetLiquidContentsType() != null)
		{
			sb.Append(' ');
			sb.Append('(');
			sb.Append(GameImpl.Translate(equipment.GetLiquidContentsType().NameHash));
			sb.Append(')');
		}
		else if (equipment.GetLiquidCapacity() > 0f)
		{
			sb.Append(' ');
			sb.Append('(');
			sb.Append(GameImpl.Translate(HUD_Empty, HUD_Empty_Female, equipment.GetPrototype().GetGenderInLanguage(GameImpl.Instance.Settings.Language)));
			sb.Append(')');
		}
		if (amount > 1)
		{
			sb.Append(' ');
			sb.Append('(');
			sb.Append('x');
			sb.AppendWithoutGarbage(amount);
			sb.Append(')');
		}
	}

	public Vector2 GetUIObjectCentreOnScreen(GameObject obj)
	{
		using (new UnityProfileMarker(CalculateRelativeRectTransformBounds))
		{
			RectTransform rectTransform = (RectTransform)obj.transform;
			return Centre + MathUtil.ToXY(HudPanelRectTransform.InverseTransformPoint(rectTransform.TransformPoint(MathUtil.ToXY0(rectTransform.rect.center))));
		}
	}

	public Rect GetUIObjectRectOnScreen(GameObject obj)
	{
		using (new UnityProfileMarker(CalculateRelativeRectTransformBounds))
		{
			RectTransform child = (RectTransform)obj.transform;
			Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(HudPanelRectTransform, child);
			return MathUtil.CreateRectCentreExtents(Centre + MathUtil.ToXY(bounds.center), bounds.extents);
		}
	}

	public void UpdateUnityActionMenu()
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		GameImpl instance2 = GameImpl.Instance;
		Session instance3 = Session.Instance;
		Hud instance4 = Hud.Instance;
		ActionMenu actionMenu = instance2.GetActionMenu();
		GameCamera gameCamera = instance3?.GameCamera;
		Vector2 vector = instance.GetCursorPos();
		if (actionMenu != null && actionMenu.FocusUnityObj != null && SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor)
		{
			vector = GetUIObjectCentreOnScreen(actionMenu.FocusUnityObj);
		}
		BaseTabPage currentPage = InfoScreen.Instance.GetCurrentPage();
		MapPage mapPage = (mapPage = currentPage as MapPage);
		if (InfoScreen.Instance.Active && SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor && mapPage != null)
		{
			vector = mapPage.CalcCursorPosOnScreen();
		}
		bool flag = gameCamera != null && !gameCamera.FlyCam && !InfoScreen.Instance.Active && !GameImpl.Instance.IsMenuOpen() && !GameImpl.Instance.IsDialogOpen();
		if (flag && instance4 != null && instance4.LocalControlledCharacter != null && instance4.LocalControlledCharacter.GetPredictedOrElseThisCharacter().InsideBuilding is EnterableVehicle enterableVehicle && enterableVehicle.GetPredictedOrElseThisVehicle().IsDrivingTooFastToShowMenu())
		{
			actionMenu = null;
		}
		bool flag2 = gameCamera?.FlyCam ?? false;
		bool flag3 = actionMenu != null && actionMenu.WantSpeechBubbleMenu() && !flag2;
		bool flag4 = instance2.LastCursorLockMode != CursorLockMode.Locked && vector.x < Size.x * 0.49f;
		if (actionMenu != null && actionMenu.FocusUnityObj == null && !flag2 && mapPage == null && !instance2.IsDialogOpen())
		{
			flag4 = false;
		}
		if (actionMenu != null && actionMenu.FocusUnityObj != null && actionMenu.IsTooltipMenu(out var offsetX, out var leftAligned))
		{
			flag4 = leftAligned;
			vector = GetUIObjectCentreOnScreen(actionMenu.FocusUnityObj) + new Vector2(offsetX, 0f);
		}
		UnityActionMenu.gameObject.SetActive(actionMenu != null && !flag3 && actionMenu.HasAnyVisibleActions() && !flag4);
		UnityActionMenuLeftAligned.gameObject.SetActive(actionMenu != null && !flag3 && actionMenu.HasAnyVisibleActions() && flag4);
		UnitySpeechMenu.gameObject.SetActive(actionMenu != null && flag3 && actionMenu.HasAnyVisibleActions());
		if (actionMenu == null)
		{
			ActionMenuVisibleFrames = 0;
			return;
		}
		ActionMenuWidth = ((actionMenu.DesiredWidth != 0f) ? actionMenu.DesiredWidth : ((float)((GameImpl.Instance.Settings.SidebarLayoutEnabled || currentPage is CommunityPage) ? 608 : 708)));
		MaxLineLength = ActionMenuWidth - 40f - 16f - 4f;
		ActionMenuBehaviour actionMenuBehaviour = (flag3 ? UnitySpeechMenu : (flag4 ? UnityActionMenuLeftAligned : UnityActionMenu));
		actionMenuBehaviour.SetWidth(ActionMenuWidth);
		actionMenuBehaviour.SelectedAction = actionMenu.SelectedAction;
		Separators.Clear();
		if (actionMenu.HasAnyVisibleActions())
		{
			float num = 0f;
			for (int i = 0; i < 2; i++)
			{
				List<AvailableAction> list = ((i == 0) ? actionMenu.HeaderActions : actionMenu.AvailableActions);
				GameObject gameObject = ((i == 0) ? actionMenuBehaviour.GetHeaderContent() : actionMenuBehaviour.GetContent());
				int j;
				for (j = 0; j < list.Count && (i != 1 || j < actionMenu.SelectedAction + 12); j++)
				{
					GameObject gameObject2 = null;
					CursorAction actionType = list[j].ActionType;
					GameObject gameObject3 = Hud.AvailableActionPrefab;
					switch (actionType)
					{
					case CursorAction.Separator:
						gameObject3 = Hud.ActionSeparator;
						break;
					case CursorAction.SkillBonus:
						gameObject3 = Hud.SkillBonus;
						break;
					case CursorAction.CropRating:
						gameObject3 = Hud.CropRating;
						break;
					case CursorAction.Insulation:
						gameObject3 = Hud.Insulation;
						break;
					case CursorAction.CharacterRole:
					case CursorAction.CharacterStatus:
					case CursorAction.SelectCharacterWithRoleOnProp:
					case CursorAction.BasePriceAndWeight:
					case CursorAction.TradePriceAndWeight:
					case CursorAction.SalePriceAndWeight:
					case CursorAction.MarkedUpSalePriceAndWeight:
					case CursorAction.LoadNewMap:
					case CursorAction.ChokeHold:
					case CursorAction.SlitThroat:
					case CursorAction.KillUnconscious:
					case CursorAction.Slaughter:
					case CursorAction.OpenGiftMenu:
					case CursorAction.Feed:
					case CursorAction.GiveWater:
					case CursorAction.Bandage:
					case CursorAction.Inject:
					case CursorAction.TakeAll:
					case CursorAction.StealAll:
					case CursorAction.Repair:
					case CursorAction.TakeOver:
					case CursorAction.WaterCrops:
					case CursorAction.FillFromWell:
					case CursorAction.FillFromRiver:
					case CursorAction.FillFromSnow:
					case CursorAction.ChopTree:
					case CursorAction.ChopLog:
					case CursorAction.ChopTreeStealing:
					case CursorAction.ChopLogStealing:
					case CursorAction.ClearBush:
					case CursorAction.Mine:
					case CursorAction.MineStealing:
					case CursorAction.LightFireWithMatch:
					case CursorAction.LightFireWithFlint:
					case CursorAction.AddMaterialToFire:
					case CursorAction.PourFuel:
					case CursorAction.PutOutFire:
					case CursorAction.PourWater:
					case CursorAction.Refuel:
					case CursorAction.Skin:
					case CursorAction.CancelGathering:
					case CursorAction.CancelGuarding:
					case CursorAction.CancelFarming:
					case CursorAction.CancelLumberjack:
					case CursorAction.CancelCook:
					case CursorAction.CancelMining:
					case CursorAction.CancelTrapper:
					case CursorAction.CancelBuildingRole:
					case CursorAction.CancelCraftingRole:
					case CursorAction.CancelCapturing:
					case CursorAction.CancelRepairing:
					case CursorAction.CancelAnimalFeeding:
					case CursorAction.CancelOrganizing:
					case CursorAction.ResumeGathering:
					case CursorAction.ResumeGuarding:
					case CursorAction.ResumeFarming:
					case CursorAction.ResumeLumberjack:
					case CursorAction.ResumeCook:
					case CursorAction.ResumeMining:
					case CursorAction.ResumeTrapper:
					case CursorAction.ResumeBuildingRole:
					case CursorAction.ResumeCraftingRole:
					case CursorAction.ResumeCapturing:
					case CursorAction.ResumeRepairing:
					case CursorAction.ResumeAnimalFeeding:
					case CursorAction.ResumeOrganizing:
					case CursorAction.PauseGathering:
					case CursorAction.PauseGuarding:
					case CursorAction.PauseFarming:
					case CursorAction.PauseLumberjack:
					case CursorAction.PauseCook:
					case CursorAction.PauseMining:
					case CursorAction.PauseTrapper:
					case CursorAction.PauseBuildingRole:
					case CursorAction.PauseCraftingRole:
					case CursorAction.PauseCapturing:
					case CursorAction.PauseRepairing:
					case CursorAction.PauseAnimalFeeding:
					case CursorAction.PauseOrganizing:
					case CursorAction.EatFromPot:
					case CursorAction.StealFromPot:
					case CursorAction.LoadAmmo:
					case CursorAction.Craft:
					case CursorAction.CancelMedic:
					case CursorAction.ResumeMedic:
					case CursorAction.PauseMedic:
					case CursorAction.ModName:
						gameObject3 = Hud.CraftAction;
						break;
					case CursorAction.BuildHere:
						gameObject3 = Hud.BuildHere;
						break;
					case CursorAction.DisplayNameAndIcon:
					case CursorAction.EquipmentSelect:
					case CursorAction.Gift:
					case CursorAction.SetCropsPatchHere:
						gameObject3 = Hud.EquipmentSelect;
						break;
					case CursorAction.TownInfectionType:
					case CursorAction.CharacterNameAndIcon:
					case CursorAction.EquipmentNameAndIcon:
					case CursorAction.LoadedAmmo:
					case CursorAction.SetMarker:
					case CursorAction.ClearMarker:
					case CursorAction.TalkToInhabitant:
						gameObject3 = Hud.NameAndIcon;
						break;
					case CursorAction.WeaponStats:
						gameObject3 = Hud.WeaponStats;
						break;
					case CursorAction.TastinessStat:
						gameObject3 = Hud.TastinessStat;
						break;
					}
					if (i == 1 && actionType != CursorAction.Separator)
					{
						float num2 = instance4?.HoldTime ?? 0f;
						if (j < gameObject.transform.childCount && 1 < gameObject.transform.GetChild(j).childCount && gameObject.transform.GetChild(j).GetChild(1).name.StartsWith(gameObject3.name))
						{
							gameObject2 = gameObject.transform.GetChild(j).GetChild(1).gameObject;
						}
						else
						{
							GameObject gameObject4 = UnityEngine.Object.Instantiate(Hud.ActionWrapper.GetAsset(), gameObject.transform, worldPositionStays: false);
							gameObject4.transform.SetSiblingIndex(j);
							gameObject2 = UnityEngine.Object.Instantiate(gameObject3, gameObject4.transform, worldPositionStays: false);
							gameObject2.transform.SetSiblingIndex(1);
						}
						RawImage component = gameObject.transform.GetChild(j).GetChild(0).GetComponent<RawImage>();
						TextMeshProUGUI component2 = gameObject.transform.GetChild(j).GetChild(0).GetChild(0)
							.GetComponent<TextMeshProUGUI>();
						StringUtil.SetUnityTextButtonPrompt(component2, InputFunction.MainAction);
						component2.alpha = ((actionMenu.SelectedAction == j) ? 1f : 0f);
						component2.transform.localScale = Vector3.one * ((num2 > 0f) ? HoldTimerBtnPromptScale : 1f);
						component.gameObject.SetActive(mapPage == null);
						component.enabled = actionMenu.SelectedAction == j && num2 > 0f;
						if (component.enabled)
						{
							component.materialForRendering.color = Color.black;
							component.materialForRendering.SetFloat(ShaderHash._FillAmount, Mathf.Clamp01(num2 / Hud.HoldTimeout));
						}
					}
					else if (j < gameObject.transform.childCount && gameObject.transform.GetChild(j).name.StartsWith(gameObject3.name))
					{
						gameObject2 = gameObject.transform.GetChild(j).gameObject;
					}
					else
					{
						gameObject2 = UnityEngine.Object.Instantiate(gameObject3, gameObject.transform, worldPositionStays: false);
						gameObject2.transform.SetSiblingIndex(j);
					}
					switch (actionType)
					{
					case CursorAction.Separator:
						Separators.Add(gameObject2.GetComponent<LayoutElement>());
						break;
					case CursorAction.TownName:
					{
						TextMeshProUGUI component25 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						list[j].Target.BuildDisplayName(sb, InfoScreen.GetAllowViewInfoOnAnyone(), englishOnly: false);
						component25.SetUnityTextIfDifferent(sb);
						component25.fontSize = 32f;
						component25.color = Color.black;
						break;
					}
					case CursorAction.TownInfectionType:
					{
						RawImage component92 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component93 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						Town town = list[j].Target as Town;
						component92.texture = (Texture2D)GameCursor.BiohazardIcon;
						component92.color = GameTerrain.MinimapSettings.GetInfectionCol(town.Infection);
						sb.Length = 0;
						sb.Append(GameImpl.Translate(Character.GetInfectionTypeStringHash(town.Infection)));
						component93.SetUnityTextIfDifferent(sb);
						component93.fontSize = 32f;
						component93.color = Color.black;
						break;
					}
					case CursorAction.DisplayNameAndIcon:
					{
						RawImage component71 = gameObject2.FindChild(EquipmentIcon).GetComponent<RawImage>();
						RawImage component72 = gameObject2.FindChild(LiquidIcon).GetComponent<RawImage>();
						RawImage component73 = gameObject2.FindChild(InfectedWithIcon).GetComponent<RawImage>();
						RawImage component74 = gameObject2.FindChild(AmmoIcon).GetComponent<RawImage>();
						TextMeshProUGUI component75 = gameObject2.FindChild(Shortcut).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component76 = gameObject2.FindChild(EquipmentName).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component77 = gameObject2.FindChild(EquipmentLoadedAmmo).GetComponent<TextMeshProUGUI>();
						BaseObject target = list[j].Target;
						component71.texture = target.GetIcon(out var mat14, out var col12, highlighted: false);
						component71.material = mat14;
						component71.color = col12;
						component72.gameObject.SetActive(value: false);
						component73.gameObject.SetActive(value: false);
						component75.gameObject.SetActive(value: false);
						component74.gameObject.SetActive(value: false);
						sb.Length = 0;
						target.BuildDisplayName(sb, InfoScreen.AllowViewInfoOnAnyone, englishOnly: false);
						component76.SetUnityText(sb);
						component76.color = Color.black;
						sb.Length = 0;
						target.BuildSubDisplayName(sb);
						Community community = target.GetCommunity();
						if (community != null && !community.IsZombieCommunity() && !community.IsAnimalCommunity() && community.CommunityType != CommunityType.Player && community.CommunityStatusShown)
						{
							if (sb.Length == 0)
							{
								sb.Append(GameImpl.Translate(HUD_UnknownCommunity));
							}
							Character character4 = target as Character;
							CommunityRelationshipType communityRelationshipType = ((instance3.CommunityManager.PlayerCommunity != null) ? instance3.CommunityManager.GetRelationship(instance3.CommunityManager.PlayerCommunity, community) : CommunityRelationshipType.Unknown);
							bool flag14 = (character4 == null || !character4.Alive || (!character4.Zombie && character4.GetBaseObjectType() == BaseObjectType.Human)) && !community.HasAnyActiveMembers();
							bool flag15 = !flag14 && communityRelationshipType == CommunityRelationshipType.Allied;
							bool flag16 = !flag14 && communityRelationshipType == CommunityRelationshipType.Hostile;
							bool flag17 = !flag15 && community.CommunityType == CommunityType.Looter && communityRelationshipType != CommunityRelationshipType.Unknown;
							if (flag17 || flag16 || flag14 || flag15)
							{
								bool flag18 = false;
								sb.Append(' ');
								sb.Append('(');
								if (flag17)
								{
									if (flag18)
									{
										sb.Append(',');
										sb.Append(' ');
									}
									sb.Append(GameImpl.Translate(Community.COMMUNITY_Looters));
									flag18 = true;
								}
								if (flag16)
								{
									if (flag18)
									{
										sb.Append(',');
										sb.Append(' ');
									}
									sb.Append(GameImpl.Translate(HUD_Hostile));
									flag18 = true;
								}
								if (flag14)
								{
									if (flag18)
									{
										sb.Append(',');
										sb.Append(' ');
									}
									sb.Append(GameImpl.Translate(HUD_Defeated));
									flag18 = true;
								}
								if (flag15)
								{
									if (flag18)
									{
										sb.Append(',');
										sb.Append(' ');
									}
									sb.Append(GameImpl.Translate(HUD_Allies));
									flag18 = true;
								}
								sb.Append(')');
							}
						}
						component77.SetUnityText(sb);
						component77.color = Color.black;
						break;
					}
					case CursorAction.CharacterName:
					{
						TextMeshProUGUI component32 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						list[j].Actor.BuildDisplayName(sb, InfoScreen.GetAllowViewInfoOnAnyone(), englishOnly: false);
						component32.SetUnityTextIfDifferent(sb);
						component32.fontSize = 32f;
						component32.color = Color.black;
						break;
					}
					case CursorAction.CharacterRole:
					{
						float lineLength2 = 0f;
						int lineIdx2 = 0;
						int idx2 = 0;
						Character actor2 = list[j].Actor;
						for (int l = 0; l < actor2.Roles.Count; l++)
						{
							if (l > 0)
							{
								AddRecipeSpacer(gameObject2, ref lineLength2, ref lineIdx2, ref idx2);
							}
							Texture2D roleDisplayIcon2 = RoleDisplayBehaviour.GetRoleDisplayIcon(actor2, actor2.Roles[l]);
							sb.Length = 0;
							RoleDisplayBehaviour.GetRoleDisplayText(actor2, actor2.Roles[l], RoleDisplayTextMode.Medium, sb);
							RawImage rawImage4 = AddRecipeIcon(gameObject2, ref lineLength2, ref lineIdx2, ref idx2);
							rawImage4.texture = roleDisplayIcon2;
							rawImage4.color = actor2.Roles[l].GetRoleIconCol();
							AddRecipeText(gameObject2, sb, ref lineLength2, ref lineIdx2, ref idx2);
						}
						EndRecipe(gameObject2, ref lineLength2, ref lineIdx2, ref idx2);
						break;
					}
					case CursorAction.SelectCharacterWithRoleOnProp:
					{
						float lineLength4 = 0f;
						int lineIdx4 = 0;
						int idx4 = 0;
						Character actor3 = list[j].Actor;
						RoleInfo roleInfo2 = actor3.Roles[list[j].Amount];
						bool flag6 = actionMenu.SelectedAction == j;
						RawImage rawImage6 = AddRecipeIcon(gameObject2, ref lineLength4, ref lineIdx4, ref idx4);
						rawImage6.texture = actor3.GetIcon(out var mat4, out var col3, flag6);
						rawImage6.material = mat4;
						rawImage6.color = col3 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						sb.Length = 0;
						actor3.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
						sb.Append(':');
						AddRecipeText(gameObject2, sb, ref lineLength4, ref lineIdx4, ref idx4).color = (flag6 ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						RawImage rawImage7 = AddRecipeIcon(gameObject2, ref lineLength4, ref lineIdx4, ref idx4);
						rawImage7.texture = RoleDisplayBehaviour.GetRoleDisplayIcon(actor3, roleInfo2);
						rawImage7.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						rawImage7.color = roleInfo2.GetRoleIconCol() * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						sb.Length = 0;
						RoleDisplayBehaviour.GetRoleDisplayText(actor3, roleInfo2, RoleDisplayTextMode.Medium, sb);
						AddRecipeText(gameObject2, sb, ref lineLength4, ref lineIdx4, ref idx4).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						EndRecipe(gameObject2, ref lineLength4, ref lineIdx4, ref idx4);
						break;
					}
					case CursorAction.CharacterNameAndIcon:
					{
						RawImage component56 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component57 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						component56.texture = list[j].Actor.GetIcon(out var mat11, out var col9, highlighted: false);
						component56.material = mat11;
						component56.color = col9;
						sb.Length = 0;
						list[j].Actor.BuildDisplayName(sb, InfoScreen.GetAllowViewInfoOnAnyone(), englishOnly: false);
						component57.SetUnityText(sb);
						component57.fontSize = 32f;
						component57.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black);
						break;
					}
					case CursorAction.TalkToInhabitant:
					{
						RawImage component4 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component5 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						component4.texture = list[j].Object.GetIcon(out var mat, out var col, highlighted: false);
						component4.material = mat;
						component4.color = col;
						sb.Length = 0;
						list[j].Object.BuildDisplayName(sb, InfoScreen.GetAllowViewInfoOnAnyone(), englishOnly: false);
						component5.SetUnityText(sb);
						component5.fontSize = 32f;
						component5.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.EquipmentName:
					{
						TextMeshProUGUI component67 = gameObject2.GetComponent<TextMeshProUGUI>();
						Equipment equipment13 = list[j].Target as Equipment;
						sb.Length = 0;
						if (equipment13 != null)
						{
							int amount2 = equipment13.GetAmount();
							Character actor9 = list[j].Actor;
							TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
							if (tradePage != null && actor9 != null)
							{
								amount2 = tradePage.GetAmountAfterPendingTrade(equipment13, actor9);
							}
							BuildEquipmentNameString(equipment13, amount2, sb);
						}
						component67.SetUnityTextIfDifferent(sb);
						component67.fontSize = 32f;
						component67.color = Color.black;
						break;
					}
					case CursorAction.EquipmentNameAndIcon:
					{
						RawImage component47 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component48 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						component47.texture = list[j].Target.GetIcon(out var mat6, out var col4, highlighted: false);
						component47.material = mat6;
						component47.color = col4;
						Equipment equipment10 = list[j].Target as Equipment;
						sb.Length = 0;
						BuildEquipmentNameString(equipment10, list[j].Amount, sb);
						component48.SetUnityText(sb);
						component48.fontSize = 32f;
						component48.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black);
						break;
					}
					case CursorAction.WeaponStats:
					{
						TextMeshProUGUI component17 = gameObject2.FindChild(Damage).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component18 = gameObject2.FindChild(Range).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component19 = gameObject2.FindChild(Fatigue).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component20 = gameObject2.FindChild(FatigueTitle).GetComponent<TextMeshProUGUI>();
						RawImage component21 = gameObject2.FindChild(DamageCompare).GetComponent<RawImage>();
						RawImage component22 = gameObject2.FindChild(RangeCompare).GetComponent<RawImage>();
						RawImage component23 = gameObject2.FindChild(FatigueCompare).GetComponent<RawImage>();
						Character actor4 = list[j].Actor;
						Equipment equipment3 = list[j].Target as Equipment;
						float num5 = actor4?.GetFatiguePenaltySkillFactor() ?? 1f;
						float accurateRange;
						float rangeIncludingEffects = equipment3.GetRangeIncludingEffects(actor4, null, out accurateRange);
						float num6 = equipment3.GetDamageIncludingEffects(actor4) * (float)equipment3.GetPrototype().NumPelletsPerShot;
						float num7 = (equipment3.GetFatiguePenaltyWhenAttacking() + equipment3.GetFatiguePenaltyWhenAiming()) * num5;
						sb.Length = 0;
						sb.AppendWithoutGarbage(Mathf.RoundToInt(num6 * 100f));
						sb.Append('%');
						component17.SetUnityTextIfDifferent(sb);
						if (num7 > 0f)
						{
							sb.Length = 0;
							sb.AppendWithoutGarbage(Mathf.RoundToInt(num7 * 100f));
							sb.Append('%');
							component19.SetUnityTextIfDifferent(sb);
							sb.Length = 0;
							sb.Append(GameImpl.Translate(HUD_Fatigue));
							sb.Append(':');
							component20.SetUnityTextIfDifferent(sb);
						}
						component19.gameObject.SetActive(num7 > 0f);
						component20.gameObject.SetActive(num7 > 0f);
						sb.Length = 0;
						if (equipment3 is RangedWeapon && accurateRange < rangeIncludingEffects)
						{
							sb.AppendWithoutGarbage(accurateRange, 1);
							sb.Append('-');
						}
						sb.AppendWithoutGarbage(rangeIncludingEffects, 1);
						sb.Append(GameImpl.Translate(HUD_Metres));
						component18.SetUnityTextIfDifferent(sb);
						if (actor4 != null && actor4.EquippedItem != null && actor4.EquippedItem != equipment3)
						{
							float accurateRange2;
							float rangeIncludingEffects2 = actor4.EquippedItem.GetRangeIncludingEffects(actor4, null, out accurateRange2);
							float num8 = actor4.EquippedItem.GetDamageIncludingEffects(actor4) * (float)actor4.EquippedItem.GetPrototype().NumPelletsPerShot;
							float num9 = (actor4.EquippedItem.GetFatiguePenaltyWhenAttacking() + actor4.EquippedItem.GetFatiguePenaltyWhenAiming()) * num5;
							component21.color = ((num6 > num8) ? Color.green : ((num6 < num8) ? Color.red : MathUtil.TransparentBlackCol));
							component23.color = ((num7 > num9) ? Color.red : ((num7 < num9) ? Color.green : MathUtil.TransparentBlackCol));
							component22.color = ((rangeIncludingEffects > rangeIncludingEffects2) ? Color.green : ((rangeIncludingEffects < rangeIncludingEffects2) ? Color.red : MathUtil.TransparentBlackCol));
							component21.rectTransform.localEulerAngles = ((num6 > num8) ? new Vector3(180f, 0f, 0f) : Vector3.zero);
							component23.rectTransform.localEulerAngles = ((num7 > num9) ? new Vector3(180f, 0f, 0f) : Vector3.zero);
							component22.rectTransform.localEulerAngles = ((rangeIncludingEffects > rangeIncludingEffects2) ? new Vector3(180f, 0f, 0f) : Vector3.zero);
							component21.gameObject.SetActive(num6 != num8);
							component23.gameObject.SetActive(num7 != num9 && num7 > 0f);
							component22.gameObject.SetActive(rangeIncludingEffects != rangeIncludingEffects2);
						}
						else
						{
							component21.gameObject.SetActive(value: false);
							component23.gameObject.SetActive(value: false);
							component22.gameObject.SetActive(value: false);
						}
						break;
					}
					case CursorAction.TastinessStat:
					{
						TextMeshProUGUI component8 = gameObject2.FindChild(Text).GetComponent<TextMeshProUGUI>();
						RawImage component9 = gameObject2.FindChild(Bar).GetComponent<RawImage>();
						RawImage component10 = gameObject2.FindChild(BarFill).GetComponent<RawImage>();
						Character actor = list[j].Actor;
						Equipment equipment = list[j].Target as Equipment;
						float num3 = actor?.CalcTastiness(equipment) ?? ((equipment.GetLiquidContentsType() != null) ? equipment.GetLiquidContentsType().Tastiness : equipment.GetPrototype().Tastiness);
						float num4 = equipment.GetNutrition() / Sun.DayLengthSecs;
						float alcoholContent = equipment.GetAlcoholContent();
						Vector2 sizeDelta = new Vector2(component9.rectTransform.rect.width * 0.5f * Mathf.Clamp01(Mathf.Abs(num3) / 100f), 0f);
						component10.rectTransform.sizeDelta = sizeDelta;
						component10.rectTransform.anchoredPosition = new Vector2(component9.rectTransform.rect.width * 0.5f - ((num3 < 0f) ? sizeDelta.x : 0f), 0f);
						component10.color = ((num3 > 0f) ? new Color32(0, 160, 0, byte.MaxValue) : new Color32(160, 0, 0, byte.MaxValue));
						sb.Length = 0;
						if (num4 != 0f)
						{
							sb.Append(GameImpl.Translate(HUD_Nutrition));
							sb.Append(' ');
							sb.AppendWithoutGarbage(num4, (!((double)num4 < 0.1)) ? 1 : 2);
							sb.Append(' ');
							sb.Append(GameImpl.Translate(StringUtil.IsPlural(num4, GameImpl.Instance.Settings.Language) ? HUD_Days : HUD_Day));
							sb.Append(',');
							sb.Append(' ');
						}
						if (alcoholContent != 0f)
						{
							sb.Append(GameImpl.Translate(HUD_Alcohol));
							sb.Append(' ');
							sb.AppendWithoutGarbage(alcoholContent, 1);
							sb.Append('%');
							sb.Append(',');
							sb.Append(' ');
						}
						sb.Append(GameImpl.Translate(HUD_Tastiness));
						component8.SetUnityTextIfDifferent(sb);
						break;
					}
					case CursorAction.EquipmentTotalName:
					{
						TextMeshProUGUI component83 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						if (list[j].Proto != null)
						{
							sb.Append(GameImpl.Translate(list[j].Proto.NameHash));
						}
						else if (list[j].Liquid != null)
						{
							sb.Append(GameImpl.Translate(list[j].Liquid.NameHash));
						}
						component83.SetUnityTextIfDifferent(sb);
						component83.fontSize = 32f;
						component83.color = Color.black;
						break;
					}
					case CursorAction.NumberOfCrafters:
					{
						TextMeshProUGUI component7 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(GameImpl.Translate(HUD_Workers));
						sb.Append(' ');
						sb.AppendWithoutGarbage(list[j].Amount);
						component7.SetUnityTextIfDifferent(sb);
						component7.fontSize = 32f;
						component7.color = Color.black;
						break;
					}
					case CursorAction.CraftingLimit:
					{
						BuildCraftingLimitString(list[j]);
						TextMeshProUGUI component46 = gameObject2.GetComponent<TextMeshProUGUI>();
						component46.SetUnityTextIfDifferent(sb);
						component46.fontSize = 32f;
						component46.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black);
						break;
					}
					case CursorAction.EquipmentDescription:
					{
						TextMeshProUGUI component45 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						if (list[j].Proto != null)
						{
							sb.Append(GameImpl.Translate(list[j].Proto.DescriptionHash));
						}
						else if (list[j].Liquid != null)
						{
							sb.Append(GameImpl.Translate(list[j].Liquid.DescriptionHash));
						}
						else
						{
							Equipment equipment9 = list[j].Target as Equipment;
							sb.Append(GameImpl.Translate(equipment9.GetPrototype().DescriptionHash));
						}
						component45.SetUnityTextIfDifferent(sb);
						component45.fontSize = 28f;
						component45.color = Color.black;
						break;
					}
					case CursorAction.MapQuadrants:
					{
						TextMeshProUGUI component31 = gameObject2.GetComponent<TextMeshProUGUI>();
						int num14 = ((list[j].Target is Equipment equipment6) ? equipment6.GetMapQuadrant() : 0);
						sb.Length = 0;
						sb.Append('(');
						for (int num15 = 0; num15 < MathUtil.Squared(GameTerrain.Instance.Size >> 9); num15++)
						{
							if ((num14 & (1 << num15)) == 0)
							{
								continue;
							}
							if (sb.Length > 1)
							{
								sb.Append(',');
								sb.Append(' ');
							}
							if (GameTerrain.Instance.Size > 1024)
							{
								int num16 = GameTerrain.Instance.Size >> 9;
								int num17 = num16 - 1 - num15 % num16;
								int num18 = num15 / num16;
								sb.AppendWithoutGarbage(num17 + 1);
								sb.Append(',');
								sb.Append(' ');
								sb.AppendWithoutGarbage(num18 + 1);
								continue;
							}
							switch (num15)
							{
							case 0:
								sb.Append(GameImpl.Translate(HUD_NorthEast));
								break;
							case 1:
								sb.Append(GameImpl.Translate(HUD_NorthWest));
								break;
							case 2:
								sb.Append(GameImpl.Translate(HUD_SouthEast));
								break;
							case 3:
								sb.Append(GameImpl.Translate(HUD_SouthWest));
								break;
							}
						}
						sb.Append(')');
						component31.SetUnityTextIfDifferent(sb);
						component31.fontSize = 28f;
						component31.color = Color.black;
						break;
					}
					case CursorAction.BasePriceAndWeight:
					case CursorAction.TradePriceAndWeight:
					case CursorAction.SalePriceAndWeight:
					case CursorAction.MarkedUpSalePriceAndWeight:
					{
						Equipment equipment18 = list[j].Target as Equipment;
						Character character5 = ((actionType != CursorAction.BasePriceAndWeight) ? (list[j].Object as Character) : null);
						float num33 = equipment18.GetBasePrice();
						int num34 = equipment18.GetAmount();
						if (character5 != null)
						{
							Character actor11 = list[j].Actor;
							num33 = actor11.GetInventoryItemPriceAdjustedForTrade(equipment18, character5, actionType == CursorAction.TradePriceAndWeight || actionType == CursorAction.SalePriceAndWeight || actionType == CursorAction.MarkedUpSalePriceAndWeight);
							if (actionType == CursorAction.MarkedUpSalePriceAndWeight)
							{
								num33 *= Session.FoodMarkup;
							}
							TradePage tradePage2 = InfoScreen.Instance.GetCurrentPage() as TradePage;
							if (tradePage2 != null)
							{
								num34 = tradePage2.GetAmountAfterPendingTrade(equipment18, actor11);
							}
						}
						float lineLength14 = 0f;
						int lineIdx14 = 0;
						int idx14 = 0;
						sb.Length = 0;
						sb.AppendWithoutGarbage((character5 != null && character5.IsControllableByPlayer()) ? ((int)Math.Ceiling((float)num34 * num33)) : ((int)Math.Floor((float)num34 * num33)));
						if (num34 > 1)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.AppendWithoutGarbage(num33, 2);
							sb.Append(' ');
							sb.Append(GameImpl.Translate(HUD_each));
							sb.Append(')');
						}
						AddRecipeTextIconPair(gameObject2, sb.ToString(), Hud.GoldIcon, selected: false, ref lineLength14, ref lineIdx14, ref idx14).color = Color.black;
						bool useMetricWeights2 = GameImpl.Instance.Settings.UseMetricWeights;
						sb.Length = 0;
						sb.AppendWithoutGarbage((float)num34 * equipment18.GetWeight() * (useMetricWeights2 ? 0.45359236f : 1f), 2);
						sb.Append(' ');
						sb.Append(GameImpl.Translate(useMetricWeights2 ? HUD_kg : HUD_lbs));
						if (num34 > 1)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.AppendWithoutGarbage(equipment18.GetWeight() * (useMetricWeights2 ? 0.45359236f : 1f), 2);
							sb.Append(' ');
							sb.Append(GameImpl.Translate(useMetricWeights2 ? HUD_kg : HUD_lbs));
							sb.Append(' ');
							sb.Append(GameImpl.Translate(HUD_each));
							sb.Append(')');
						}
						AddRecipeSpacer(gameObject2, ref lineLength14, ref lineIdx14, ref idx14);
						AddRecipeTextIconPair(gameObject2, sb.ToString(), Hud.WeightIcon, selected: false, ref lineLength14, ref lineIdx14, ref idx14).color = Color.black;
						if (equipment18.GetLiquidCapacity() > 0f)
						{
							bool useMetricWeights3 = GameImpl.Instance.Settings.UseMetricWeights;
							sb.Length = 0;
							sb.AppendWithoutGarbage(equipment18.GetLiquidContentsAmount() * (useMetricWeights3 ? 0.0295735f : 1f), 1);
							sb.Append('/');
							sb.AppendWithoutGarbage(equipment18.GetLiquidCapacity() * (useMetricWeights3 ? 0.0295735f : 1f), 1);
							sb.Append(' ');
							sb.Append(GameImpl.Translate(useMetricWeights3 ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
							AddRecipeSpacer(gameObject2, ref lineLength14, ref lineIdx14, ref idx14);
							AddRecipeTextIconPair(gameObject2, sb.ToString(), GameCursor.CursorWaterBottle, selected: false, ref lineLength14, ref lineIdx14, ref idx14);
						}
						EndRecipe(gameObject2, ref lineLength14, ref lineIdx14, ref idx14);
						break;
					}
					case CursorAction.Insulation:
					{
						Equipment obj2 = list[j].Target as Equipment;
						int insulation = obj2.GetInsulation();
						int insulationIncludingDampness = obj2.GetInsulationIncludingDampness();
						GenderType gender2 = obj2.GetGender(GameImpl.Instance.Settings.Language);
						bool plural2 = obj2.IsPlural();
						for (int num35 = 1; num35 <= 4; num35++)
						{
							RawImage component90 = gameObject2.transform.GetChild(num35).GetComponent<RawImage>();
							component90.color = ((num35 <= insulationIncludingDampness) ? InsulationJacketCol : InsulationJacketDampCol);
							component90.gameObject.SetActive(num35 <= insulation);
						}
						TextMeshProUGUI component91 = gameObject2.FindChild(Damp).GetComponent<TextMeshProUGUI>();
						component91.gameObject.SetActive(insulationIncludingDampness < insulation);
						component91.SetUnityTextIfDifferent(GameImpl.Translate(InsulationDisplayBehaviour.HUD_Damp, InsulationDisplayBehaviour.HUD_Damp_Female, InsulationDisplayBehaviour.HUD_Damp_Plural, InsulationDisplayBehaviour.HUD_Damp_Female_Plural, gender2, plural2));
						break;
					}
					case CursorAction.LoadedAmmo:
					{
						RawImage component84 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component85 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						AmmoWeapon ammoWeapon = list[j].Target as AmmoWeapon;
						EquipmentPrototype equipmentPrototype3 = ((ammoWeapon.CurrentAmmoType != null) ? ammoWeapon.CurrentAmmoType : ammoWeapon.GetPrototype().GetDefaultAmmoPrototype());
						component84.texture = ((equipmentPrototype3 != null && equipmentPrototype3.Tex != null) ? equipmentPrototype3.Tex.GetAsset() : null);
						component84.material = Hud.OutlineBlackMat;
						sb.Length = 0;
						sb.Append(GameImpl.Translate(HUD_LoadedAmmo));
						sb.Append(' ');
						sb.AppendWithoutGarbage(ammoWeapon.CurrentAmmo);
						sb.Append('/');
						sb.AppendWithoutGarbage(ammoWeapon.GetMaxAmmo() * ammoWeapon.GetAmount());
						component85.SetUnityTextIfDifferent(sb);
						component85.fontSize = 32f;
						component85.color = Color.black;
						break;
					}
					case CursorAction.SightBonus:
					{
						TextMeshProUGUI component78 = gameObject2.GetComponent<TextMeshProUGUI>();
						Equipment equipment14 = list[j].Target as Equipment;
						sb.Length = 0;
						if (equipment14.GetSightRangeModifierWhenEquipped() > 0)
						{
							sb.Append('+');
						}
						sb.AppendWithoutGarbage(equipment14.GetSightRangeModifierWhenEquipped());
						sb.Append(' ');
						sb.Append(GameImpl.Translate(HUD_SightBonus));
						component78.SetUnityTextIfDifferent(sb);
						component78.fontSize = 32f;
						component78.color = ((equipment14.GetSightRangeModifierWhenEquipped() > 0) ? new Color32(0, 160, 0, byte.MaxValue) : new Color32(160, 0, 0, byte.MaxValue));
						break;
					}
					case CursorAction.SkillBonus:
					{
						bool num19 = instance4 != null && instance4.LocalControlledCharacter != null && instance4.LocalControlledCharacter != list[j].Actor;
						TextMeshProUGUI component33 = gameObject2.FindChild(Bonus).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component34 = gameObject2.FindChild(Skill).GetComponent<TextMeshProUGUI>();
						Equipment equipment7 = list[j].Target as Equipment;
						SkillType skillBonusType = equipment7.GetPrototype().SkillBonusType;
						Color color2 = ((equipment7.GetPrototype().SkillBonus > 0) ? new Color32(0, 160, 0, byte.MaxValue) : new Color32(160, 0, 0, byte.MaxValue));
						sb.Length = 0;
						if (equipment7.GetPrototype().SkillBonus > 0)
						{
							sb.Append('+');
						}
						sb.AppendWithoutGarbage(equipment7.GetPrototype().SkillBonus);
						component33.SetUnityTextIfDifferent(sb);
						component33.color = color2;
						gameObject2.FindChild(Star).GetComponent<RawImage>().color = color2;
						sb.Length = 0;
						sb.Append(GameImpl.Translate(Skillset.GetSkillNameHash(skillBonusType)));
						component34.SetUnityTextIfDifferent(sb);
						component34.color = color2;
						if (num19)
						{
							Character localControlledCharacter = instance4.LocalControlledCharacter;
							int level = localControlledCharacter.Skillset.GetLevel(skillBonusType);
							int cap = localControlledCharacter.Skillset.GetCap(skillBonusType);
							int num20 = 0;
							int num21 = 0;
							localControlledCharacter.GetSkillEffects(SkillDisplayBehaviour._skillEffects);
							for (int num22 = 0; num22 < SkillDisplayBehaviour._skillEffects.Count; num22++)
							{
								if (SkillDisplayBehaviour._skillEffects[num22].SkillType == skillBonusType)
								{
									if (SkillDisplayBehaviour._skillEffects[num22].Effect > 0)
									{
										num20 += SkillDisplayBehaviour._skillEffects[num22].Effect;
									}
									else if (SkillDisplayBehaviour._skillEffects[num22].Effect < 0)
									{
										num21 += SkillDisplayBehaviour._skillEffects[num22].Effect;
									}
								}
							}
							for (int num23 = 0; num23 < 5; num23++)
							{
								RawImage component35 = gameObject2.FindChild(Stars[num23]).GetComponent<RawImage>();
								Color color3 = SkillDisplayBehaviour.StarColActive;
								int num24 = num23 + 1;
								if (num24 > level)
								{
									color3 = SkillDisplayBehaviour.StarColBonus;
								}
								if (num24 > level + num20)
								{
									color3 = SkillDisplayBehaviour.StarColInactive;
								}
								else if (num24 > level + num20 + num21)
								{
									color3 = SkillDisplayBehaviour.StarColInjured;
								}
								component35.gameObject.SetActive(num23 < cap);
								component35.color = color3;
							}
						}
						else
						{
							for (int num25 = 5; num25 < gameObject2.transform.childCount; num25++)
							{
								gameObject2.transform.GetChild(num25).gameObject.SetActive(value: false);
							}
						}
						break;
					}
					case CursorAction.SkillOnConsumption:
					{
						TextMeshProUGUI component36 = gameObject2.GetComponent<TextMeshProUGUI>();
						Equipment equipment8 = list[j].Target as Equipment;
						SkillType type = ((equipment8.GetLiquidContentsType() != null) ? equipment8.GetLiquidContentsType().SkillOnConsumptionType : equipment8.GetPrototype().SkillOnConsumptionType);
						float num26 = ((equipment8.GetLiquidContentsType() != null) ? (equipment8.GetLiquidContentsType().SkillOnConsumptionProgressionPerFlOz * equipment8.GetLiquidContentsAmount()) : equipment8.GetPrototype().SkillOnConsumptionProgression);
						float num27 = equipment8.GetNutrition() / Sun.DayLengthSecs;
						sb.Length = 0;
						if (num26 > 0f)
						{
							sb.Append('+');
						}
						sb.AppendWithoutGarbage(num26, 0);
						sb.Append(' ');
						sb.Append(GameImpl.Translate(StringUtil.IsPlural(num26, GameImpl.Instance.Settings.Language) ? HUD_XP : HUD_XP_Singular));
						sb.Append(' ');
						sb.Append(GameImpl.Translate(Skillset.GetSkillNameHash(type)));
						if (num27 > 0f)
						{
							float num28 = Mathf.RoundToInt(num26 / num27);
							sb.Append(' ');
							sb.Append('(');
							sb.AppendWithoutGarbage(num28, (num28 < 1f) ? 1 : 0);
							sb.Append('/');
							sb.Append(GameImpl.Translate(HUD_Day));
							sb.Append(')');
						}
						component36.SetUnityTextIfDifferent(sb);
						component36.fontSize = 32f;
						component36.color = ((num26 > 0f) ? new Color32(0, 160, 0, byte.MaxValue) : new Color32(160, 0, 0, byte.MaxValue));
						break;
					}
					case CursorAction.CropRating:
					{
						int num31 = ((list[j].Target is PlantableCrop plantableCrop) ? plantableCrop.FarmerSkillLevel : 0);
						for (int num32 = 0; num32 < 5; num32++)
						{
							RawImage component87 = gameObject2.FindChild(Stars[num32]).GetComponent<RawImage>();
							Color color11 = SkillDisplayBehaviour.StarColActive;
							if (num32 + 1 > num31)
							{
								color11 = SkillDisplayBehaviour.StarColInactive;
							}
							component87.gameObject.SetActive(value: true);
							component87.color = color11;
						}
						break;
					}
					case CursorAction.TakeAll:
					case CursorAction.StealAll:
					{
						TileObject tileObject = list[j].Target as TileObject;
						float lineLength7 = 0f;
						int lineIdx7 = 0;
						int idx7 = 0;
						AddRecipeText(gameObject2, list[j].GetCaption(), ref lineLength7, ref lineIdx7, ref idx7).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						string text2 = GameImpl.Translate(HUD_More);
						if (text2 != MoreText)
						{
							MoreText = text2;
							MoreTextLength = Hud.RecipeText.GetAsset().GetComponent<TextMeshProUGUI>().GetPreferredValues(MoreText)
								.x;
						}
						int selectedIndex;
						List<EquipmentContainer.Equippable> list2 = tileObject.GetInventory().BuildEquippableList(null, null, wantLockOn: false, takeAll: true, out selectedIndex);
						for (int num29 = 0; num29 < list2.Count; num29++)
						{
							Equipment item2 = list2[num29].Item;
							if (!item2.IsIncludedInTakeAll(tileObject))
							{
								continue;
							}
							if (lineIdx7 >= 1 && lineLength7 >= MaxLineLength - MoreTextLength - 200f)
							{
								AddRecipeText(gameObject2, MoreText, ref lineLength7, ref lineIdx7, ref idx7).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
								break;
							}
							LiquidPrototype liquidPrototype2 = item2.GetLiquidContentsType();
							if (liquidPrototype2 == null)
							{
								liquidPrototype2 = item2.DesignatedLiquid;
							}
							EquipmentPrototype prototype = item2.GetPrototype();
							InfectionType infectedWith2 = list2[num29].InfectedWith;
							int amount = list2[num29].Amount;
							if (liquidPrototype2 != null)
							{
								RawImage rawImage16 = AddRecipeLiquidContainer(gameObject2, ref lineLength7, ref lineIdx7, ref idx7);
								rawImage16.texture = ((prototype.Tex != null) ? prototype.Tex.GetAsset() : null);
								rawImage16.color = Color.white;
								RawImage component50 = rawImage16.transform.GetChild(0).GetComponent<RawImage>();
								component50.texture = ((liquidPrototype2.Tex != null) ? liquidPrototype2.Tex.GetAsset() : null);
								Color color5 = liquidPrototype2.Col;
								if (item2.GetLiquidContentsAmount() == 0f)
								{
									color5.a = 0.5f;
								}
								component50.color = color5;
							}
							else if (infectedWith2 != InfectionType.None)
							{
								RawImage rawImage17 = AddRecipeLiquidContainer(gameObject2, ref lineLength7, ref lineIdx7, ref idx7);
								rawImage17.texture = item2.GetIcon(out var _, out var col5, highlighted: false);
								rawImage17.color = col5;
								RawImage component51 = rawImage17.transform.GetChild(0).GetComponent<RawImage>();
								component51.texture = (Texture2D)GameCursor.BiohazardIcon;
								component51.color = GameTerrain.MinimapSettings.GetInfectionCol(infectedWith2);
							}
							else if (prototype.Special)
							{
								RawImage rawImage18 = AddRecipeLiquidContainer(gameObject2, ref lineLength7, ref lineIdx7, ref idx7);
								rawImage18.texture = item2.GetIcon(out var _, out var col6, highlighted: false);
								rawImage18.color = col6;
								RawImage component52 = rawImage18.transform.GetChild(0).GetComponent<RawImage>();
								component52.texture = (Texture2D)GameCursor.SkillIconSmall;
								component52.color = prototype.GetSpecialIconColor();
							}
							else
							{
								RawImage rawImage19 = AddRecipeIcon(gameObject2, ref lineLength7, ref lineIdx7, ref idx7);
								rawImage19.texture = item2.GetIcon(out var _, out var col7, highlighted: false);
								rawImage19.color = col7;
							}
							if (amount > 1)
							{
								sb.Length = 0;
								sb.Append('(');
								sb.AppendWithoutGarbage(amount);
								sb.Append(')');
								AddRecipeText(gameObject2, sb.ToString(), ref lineLength7, ref lineIdx7, ref idx7).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							}
						}
						EndRecipe(gameObject2, ref lineLength7, ref lineIdx7, ref idx7);
						break;
					}
					case CursorAction.OpenGiftMenu:
					{
						float lineLength8 = 0f;
						int lineIdx8 = 0;
						int idx8 = 0;
						AddRecipeText(gameObject2, list[j].GetCaption(), ref lineLength8, ref lineIdx8, ref idx8).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						Character actor7 = list[j].Actor;
						for (int num30 = 0; num30 < actor7.Inventory.Count; num30++)
						{
							Equipment item3 = actor7.Inventory.GetItem(num30);
							if (item3.WasGifted())
							{
								continue;
							}
							EquipmentPrototype prototype2 = item3.GetPrototype();
							LiquidPrototype liquidContentsType = item3.GetLiquidContentsType();
							if (prototype2.CanBeGift() && !GameCursor.GiftProtos.Contains(prototype2))
							{
								RawImage rawImage20;
								if (prototype2.Special)
								{
									rawImage20 = AddRecipeLiquidContainer(gameObject2, ref lineLength8, ref lineIdx8, ref idx8);
									RawImage component53 = rawImage20.transform.GetChild(0).GetComponent<RawImage>();
									component53.texture = (Texture2D)GameCursor.SkillIconSmall;
									component53.color = prototype2.GetSpecialIconColor();
								}
								else
								{
									rawImage20 = AddRecipeIcon(gameObject2, ref lineLength8, ref lineIdx8, ref idx8);
								}
								rawImage20.texture = item3.GetIcon(out var _, out var col8, highlighted: false);
								rawImage20.color = col8;
								GameCursor.GiftProtos.Add(prototype2);
							}
							else if (liquidContentsType != null && liquidContentsType.CanBeGift() && !GameCursor.GiftLiquids.Contains(liquidContentsType))
							{
								RawImage rawImage21 = AddRecipeLiquidContainer(gameObject2, ref lineLength8, ref lineIdx8, ref idx8);
								rawImage21.texture = ((prototype2.Tex != null) ? prototype2.Tex.GetAsset() : null);
								rawImage21.color = Color.white;
								RawImage component54 = rawImage21.transform.GetChild(0).GetComponent<RawImage>();
								component54.texture = ((liquidContentsType.Tex != null) ? liquidContentsType.Tex.GetAsset() : null);
								component54.color = liquidContentsType.Col;
								GameCursor.GiftLiquids.Add(liquidContentsType);
							}
						}
						GameCursor.GiftProtos.Clear();
						GameCursor.GiftLiquids.Clear();
						EndRecipe(gameObject2, ref lineLength8, ref lineIdx8, ref idx8);
						break;
					}
					case CursorAction.ModName:
					{
						float lineLength3 = 0f;
						int lineIdx3 = 0;
						int idx3 = 0;
						AddRecipeText(gameObject2, list[j].SpeechText, ref lineLength3, ref lineIdx3, ref idx3).color = Color.black;
						RawImage rawImage5 = AddRecipeIcon(gameObject2, ref lineLength3, ref lineIdx3, ref idx3);
						rawImage5.texture = (Texture2D)((list[j].Amount < 0) ? GameCursor.CompletedQuestIcon : GameCursor.FailedQuestIcon);
						rawImage5.color = Color.white;
						EndRecipe(gameObject2, ref lineLength3, ref lineIdx3, ref idx3);
						break;
					}
					case CursorAction.CharacterStatus:
					{
						float lineLength13 = 0f;
						int lineIdx13 = 0;
						int idx13 = 0;
						Character actor10 = list[j].Actor;
						if (actor10.HasUnbandagedInjury(0))
						{
							RawImage rawImage29 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage29.texture = (Texture2D)GameCursor.BloodLossIcon;
							rawImage29.color = Color.red;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Bleeding, HUD_Bleeding_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.GetInfectionProgression() > 0f)
						{
							RawImage rawImage30 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage30.texture = (Texture2D)GameCursor.BiohazardIcon;
							rawImage30.color = GameTerrain.MinimapSettings.GetInfectionCol(actor10.GetWorstInfectionTypeInProgression());
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Infected, HUD_Infected_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.GetThirst() >= Character.ThirstCriticalTime)
						{
							RawImage rawImage31 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage31.texture = (Texture2D)GameCursor.CursorWaterBottle;
							rawImage31.color = Color.white;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Thirsty, HUD_Thirsty_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.GetHunger() >= Character.HungerCriticalTime)
						{
							RawImage rawImage32 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage32.texture = (Texture2D)GameCursor.CursorEat;
							rawImage32.color = Color.white;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Hungry, HUD_Hungry_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime)
						{
							RawImage rawImage33 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage33.texture = (Texture2D)GameCursor.CursorSleep;
							rawImage33.color = Color.white;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Tired, HUD_Tired_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia)
						{
							RawImage rawImage34 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage34.texture = (Texture2D)GameCursor.FrostIcon;
							rawImage34.color = Color.white;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Cold, HUD_Cold_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						if (actor10.IsTooDepressedToFollowOrders())
						{
							RawImage rawImage35 = AddRecipeIcon(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
							rawImage35.texture = (Texture2D)GameCursor.DepressedIcon;
							rawImage35.color = Color.white;
							AddRecipeText(gameObject2, GameImpl.Translate(HUD_Depressed, HUD_Depressed_Female, actor10.GetGender()), ref lineLength13, ref lineIdx13, ref idx13).color = Color.black;
						}
						EndRecipe(gameObject2, ref lineLength13, ref lineIdx13, ref idx13);
						break;
					}
					case CursorAction.Craft:
					{
						Recipe recipe3 = list[j].Recipe;
						Character actor5 = list[j].Actor;
						Character character = actor5;
						Equipment equipment4 = list[j].Target as Equipment;
						float lineLength5 = 0f;
						int lineIdx5 = 0;
						int idx5 = 0;
						bool flag7 = list[j].Enabled == CursorActionDisabledReason.Enabled && recipe3.HasAnyIngredients(character, actor5, equipment4, null);
						if (recipe3.NameHash != 0)
						{
							string text = GameImpl.Translate(recipe3.NameHash);
							if (text.Length >= RecipeTextSplitLength)
							{
								int num10 = 0;
								int n = RecipeTextSplitLength;
								while (n > 0)
								{
									n--;
									if (text[n] == ' ')
									{
										break;
									}
								}
								while (true)
								{
									sb.Clear();
									sb.Append(text, num10, n - num10);
									AddRecipeText(gameObject2, sb, ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag7 ? 1f : 0.5f);
									num10 = ++n;
									if (n >= text.Length)
									{
										break;
									}
									for (; n < text.Length && text[n] != ' '; n++)
									{
									}
								}
							}
							else
							{
								AddRecipeText(gameObject2, text, ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag7 ? 1f : 0.5f);
							}
						}
						InfectionType infectionType = InfectionType.None;
						bool flag8 = true;
						foreach (Ingredient ingredient in recipe3.Ingredients)
						{
							bool flag9 = ingredient.HasEnoughOfIngredient(character, actor5, recipe3, equipment4, null);
							if (!flag8)
							{
								AddRecipeText(gameObject2, Plus, ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag7 ? 1f : 0.5f);
							}
							flag8 = false;
							InfectionType infectionType2 = InfectionType.None;
							if (ingredient.Prototypes != null)
							{
								EquipmentPrototype equipmentPrototype = null;
								Color color = Color.white;
								if (equipment4 != null && ingredient.MatchesItem(equipment4, recipe3))
								{
									equipmentPrototype = equipment4.GetPrototype();
									infectionType2 = equipment4.InfectedWith;
									color = equipment4.GetColor();
								}
								else
								{
									float num11 = float.MinValue;
									foreach (EquipmentPrototype prototype3 in ingredient.Prototypes)
									{
										if (prototype3 != null && prototype3.Tex != null)
										{
											Equipment equipment5 = actor5.Inventory.FindItemOfTypePreferringUnused(character, actor5, prototype3, recipe3, checkCraftingPolicy: false, ingredient.IngredientInfectionState);
											float num12 = equipment5?.GetAmount() ?? 0;
											if (num12 > num11)
											{
												equipmentPrototype = prototype3;
												infectionType2 = equipment5?.InfectedWith ?? InfectionType.None;
												color = ((equipment5 != null) ? ((Color)equipment5.GetColor()) : Color.white);
												num11 = num12;
											}
										}
									}
								}
								if (equipmentPrototype != null)
								{
									RawImage rawImage8 = ((infectionType2 == InfectionType.None) ? AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5) : AddRecipeLiquidContainer(gameObject2, ref lineLength5, ref lineIdx5, ref idx5));
									rawImage8.texture = equipmentPrototype.Tex.GetAsset();
									rawImage8.color = color * (flag9 ? 1f : 0.5f);
									if (infectionType2 != InfectionType.None)
									{
										RawImage component27 = rawImage8.transform.GetChild(0).GetComponent<RawImage>();
										component27.texture = (Texture2D)GameCursor.BiohazardIcon;
										component27.color = GameTerrain.MinimapSettings.GetInfectionCol(infectionType2);
									}
								}
							}
							else if (ingredient.LiquidTypes != null)
							{
								LiquidPrototype liquidPrototype = null;
								if (equipment4 != null && ingredient.MatchesItem(equipment4, recipe3))
								{
									liquidPrototype = equipment4.GetLiquidContentsType();
									infectionType2 = equipment4.InfectedWith;
									infectionType = (InfectionType)Math.Max((int)infectionType, (int)equipment4.InfectedWith);
								}
								else
								{
									float num13 = float.MinValue;
									foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
									{
										if (liquidType != null)
										{
											InfectionType ingredientsInfectedWith;
											float amountOfLiquidType = actor5.Inventory.GetAmountOfLiquidType(actor5, liquidType, out ingredientsInfectedWith, ingredient.IngredientInfectionState);
											if (amountOfLiquidType > num13)
											{
												liquidPrototype = liquidType;
												infectionType2 = ingredientsInfectedWith;
												num13 = amountOfLiquidType;
											}
											break;
										}
									}
								}
								RawImage rawImage9 = ((infectionType2 == InfectionType.None) ? AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5) : AddRecipeLiquidContainer(gameObject2, ref lineLength5, ref lineIdx5, ref idx5));
								rawImage9.texture = ((liquidPrototype != null && liquidPrototype.Tex != null) ? liquidPrototype.Tex.GetAsset() : null);
								rawImage9.color = ((liquidPrototype != null) ? ((Color)liquidPrototype.Col) : Color.white) * (flag9 ? 1f : 0.5f);
								if (infectionType2 != InfectionType.None)
								{
									RawImage component28 = rawImage9.transform.GetChild(0).GetComponent<RawImage>();
									component28.texture = (Texture2D)GameCursor.BiohazardIcon;
									component28.color = GameTerrain.MinimapSettings.GetInfectionCol(infectionType2);
								}
							}
							else
							{
								RawImage rawImage10 = AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
								rawImage10.texture = null;
								rawImage10.color = Color.white * (flag9 ? 1f : 0.5f);
							}
							infectionType = (InfectionType)Math.Max((int)infectionType, (int)infectionType2);
							if (ingredient.Amount > 1)
							{
								sb.Length = 0;
								sb.Append('x');
								sb.AppendWithoutGarbage(ingredient.Amount);
								AddRecipeText(gameObject2, sb.ToString(), ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag9 ? 1f : 0.5f);
							}
						}
						if (idx5 > 0 && (recipe3.ProductPrototype != null || recipe3.ProductLiquidPrototype != null || recipe3.ProductPropPrototype != null))
						{
							AddRecipeText(gameObject2, Equal, ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag7 ? 1f : 0.5f);
						}
						InfectionType infectionType3 = (recipe3.CanPassInfectionToProduct ? infectionType : InfectionType.None);
						if (recipe3.ProductPrototype != null)
						{
							if (infectionType3 == InfectionType.None)
							{
								RawImage rawImage11 = AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
								rawImage11.texture = ((recipe3.ProductPrototype.Tex != null) ? recipe3.ProductPrototype.Tex.GetAsset() : null);
								rawImage11.color = Color.white * (flag7 ? 1f : 0.5f);
							}
							else
							{
								RawImage rawImage12 = AddRecipeLiquidContainer(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
								rawImage12.texture = ((recipe3.ProductPrototype.Tex != null) ? recipe3.ProductPrototype.Tex.GetAsset() : null);
								rawImage12.color = Color.white * (flag7 ? 1f : 0.5f);
								RawImage component29 = rawImage12.transform.GetChild(0).GetComponent<RawImage>();
								component29.texture = (Texture2D)GameCursor.BiohazardIcon;
								component29.color = GameTerrain.MinimapSettings.GetInfectionCol(infectionType3);
							}
							if (recipe3.ProductAmount > 1)
							{
								sb.Length = 0;
								sb.Append('x');
								sb.AppendWithoutGarbage(recipe3.ProductAmount);
								AddRecipeText(gameObject2, sb.ToString(), ref lineLength5, ref lineIdx5, ref idx5).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * (flag7 ? 1f : 0.5f);
							}
						}
						if (recipe3.ProductLiquidPrototype != null)
						{
							if (infectionType3 == InfectionType.None)
							{
								RawImage rawImage13 = AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
								rawImage13.texture = ((recipe3.ProductLiquidPrototype.Tex != null) ? recipe3.ProductLiquidPrototype.Tex.GetAsset() : null);
								rawImage13.color = (Color)recipe3.ProductLiquidPrototype.Col * (flag7 ? 1f : 0.5f);
							}
							else
							{
								RawImage rawImage14 = AddRecipeLiquidContainer(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
								rawImage14.texture = ((recipe3.ProductLiquidPrototype.Tex != null) ? recipe3.ProductLiquidPrototype.Tex.GetAsset() : null);
								rawImage14.color = (Color)recipe3.ProductLiquidPrototype.Col * (flag7 ? 1f : 0.5f);
								RawImage component30 = rawImage14.transform.GetChild(0).GetComponent<RawImage>();
								component30.texture = (Texture2D)GameCursor.BiohazardIcon;
								component30.color = GameTerrain.MinimapSettings.GetInfectionCol(infectionType3);
							}
						}
						if (recipe3.ProductPropPrototype != null)
						{
							RawImage rawImage15 = AddRecipeIcon(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
							rawImage15.texture = IconGenerator.Instance.GetIconForObjectType(recipe3.ProductPropPrototype, out var mat5, actionMenu.SelectedAction == j);
							rawImage15.material = mat5;
							rawImage15.color = Color.white * (flag7 ? 1f : 0.5f);
						}
						EndRecipe(gameObject2, ref lineLength5, ref lineIdx5, ref idx5);
						break;
					}
					case CursorAction.BuildHere:
					{
						RawImage component11 = gameObject2.FindChild(BuildingIcon).GetComponent<RawImage>();
						TextMeshProUGUI component12 = gameObject2.FindChild(RecipeName).GetComponent<TextMeshProUGUI>();
						GameObject gameObject5 = gameObject2.FindChild(Recipe).gameObject;
						Recipe recipe = list[j].Recipe;
						Equipment equipment2 = list[j].Target as Equipment;
						if (recipe.ProductPropPrototype != null && recipe.ProductPropPrototype.TypeName == BaseObjectType.Mine && !GameTerrain.Instance.IsTileOutsideBounds(list[j].Tile.x, list[j].Tile.y) && GameTerrain.Instance.FogOfWar.IsTileVisible(list[j].Tile.x, list[j].Tile.y))
						{
							sb.Length = 0;
							sb.Append(GameImpl.Translate(recipe.NameHash));
							Mine.BuildMineralDepositsString(sb, list[j].Tile);
							component12.SetUnityTextIfDifferent(sb);
						}
						else
						{
							component12.SetUnityText(GameImpl.Translate(recipe.NameHash));
						}
						component12.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						component11.texture = IconGenerator.Instance.GetIconForObjectType(recipe.ProductPropPrototype, out var mat2, actionMenu.SelectedAction == j);
						component11.material = mat2;
						component11.color = Color.white * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						int m = 1;
						foreach (Ingredient ingredient2 in recipe.Ingredients)
						{
							if (ingredient2.Prototypes != null && ingredient2.Prototypes.Count > 0)
							{
								GameObject gameObject6 = ((m < gameObject5.transform.childCount) ? gameObject5.transform.GetChild(m).gameObject : null);
								if (gameObject6 == null)
								{
									gameObject6 = UnityEngine.Object.Instantiate((GameObject)Hud.BuildingIngredient, gameObject5.transform, worldPositionStays: false);
								}
								m++;
								RawImage component13 = gameObject6.FindChild(RecipeItem).GetComponent<RawImage>();
								TextMeshProUGUI component14 = gameObject6.FindChild(RecipeText).GetComponent<TextMeshProUGUI>();
								if (equipment2 != null && ingredient2.MatchesItem(equipment2, recipe))
								{
									component13.texture = equipment2.GetIcon(out var _, out var col2, actionMenu.SelectedAction == j);
									component13.color = col2 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
								}
								else
								{
									component13.texture = ((ingredient2.Prototypes[0].Tex != null) ? ingredient2.Prototypes[0].Tex.GetAsset() : null);
									component13.color = Color.white * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
								}
								component14.SetUnityText(ingredient2.Amount.ToString());
								component14.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							}
						}
						for (; m < gameObject5.transform.childCount; m++)
						{
							UnityEngine.Object.Destroy(gameObject5.transform.GetChild(m).gameObject);
						}
						break;
					}
					case CursorAction.Repair:
					case CursorAction.TakeOver:
					{
						TileObject tileObject2 = list[j].Target as TileObject;
						EquipmentPrototype equipmentPrototype2 = ((list[j].ActionType == CursorAction.TakeOver) ? tileObject2.GetCaptureResourceType() : tileObject2.GetRepairResourceType());
						int number = Mathf.CeilToInt((list[j].ActionType == CursorAction.TakeOver) ? (tileObject2.GetCaptureResourceNeeded() * (1f - tileObject2.GetCaptureFraction())) : (tileObject2.GetRepairResourceNeeded() * tileObject2.GetDamageFraction()));
						float lineLength12 = 0f;
						int lineIdx12 = 0;
						int idx12 = 0;
						AddRecipeText(gameObject2, list[j].GetCaption(), ref lineLength12, ref lineIdx12, ref idx12).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						sb.Length = 0;
						sb.Append(' ');
						sb.Append('(');
						AddRecipeText(gameObject2, sb.ToString(), ref lineLength12, ref lineIdx12, ref idx12).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						RawImage rawImage28 = AddRecipeIcon(gameObject2, ref lineLength12, ref lineIdx12, ref idx12);
						rawImage28.texture = ((equipmentPrototype2.Tex != null) ? equipmentPrototype2.Tex.GetAsset() : null);
						rawImage28.color = Color.white;
						sb.Length = 0;
						sb.Append('x');
						sb.AppendWithoutGarbage(number);
						sb.Append(')');
						AddRecipeText(gameObject2, sb.ToString(), ref lineLength12, ref lineIdx12, ref idx12).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						EndRecipe(gameObject2, ref lineLength12, ref lineIdx12, ref idx12);
						break;
					}
					case CursorAction.KillUnconscious:
					case CursorAction.Slaughter:
					case CursorAction.Feed:
					case CursorAction.GiveWater:
					case CursorAction.Bandage:
					case CursorAction.Inject:
					case CursorAction.WaterCrops:
					case CursorAction.FillFromWell:
					case CursorAction.FillFromRiver:
					case CursorAction.FillFromSnow:
					case CursorAction.ChopTree:
					case CursorAction.ChopLog:
					case CursorAction.ChopTreeStealing:
					case CursorAction.ChopLogStealing:
					case CursorAction.ClearBush:
					case CursorAction.LightFireWithMatch:
					case CursorAction.LightFireWithFlint:
					case CursorAction.AddMaterialToFire:
					case CursorAction.PourFuel:
					case CursorAction.PutOutFire:
					case CursorAction.PourWater:
					case CursorAction.Refuel:
					case CursorAction.Skin:
					case CursorAction.EatFromPot:
					case CursorAction.StealFromPot:
					{
						float lineLength10 = 0f;
						int lineIdx10 = 0;
						int idx10 = 0;
						Equipment equipment15 = list[j].Target as Equipment;
						AddRecipeText(gameObject2, list[j].GetCaption(), ref lineLength10, ref lineIdx10, ref idx10).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						if (equipment15 != null)
						{
							LiquidPrototype liquidPrototype4 = equipment15.GetLiquidContentsType();
							if (liquidPrototype4 == null)
							{
								liquidPrototype4 = equipment15.DesignatedLiquid;
							}
							if (liquidPrototype4 != null)
							{
								if (equipment15.InfectedWith != InfectionType.None)
								{
									RawImage rawImage23 = AddRecipeInfectedLiquidContainer(gameObject2, ref lineLength10, ref lineIdx10, ref idx10);
									rawImage23.texture = equipment15.GetIcon(out var mat15, out var col13, actionMenu.SelectedAction == j);
									rawImage23.material = mat15;
									rawImage23.color = col13 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
									RawImage component79 = rawImage23.transform.GetChild(0).GetComponent<RawImage>();
									component79.texture = ((liquidPrototype4.Tex != null) ? liquidPrototype4.Tex.GetAsset() : null);
									Color color9 = liquidPrototype4.Col;
									if (equipment15.GetLiquidContentsAmount() == 0f)
									{
										color9.a = 0.5f;
									}
									component79.color = color9 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
									RawImage component80 = rawImage23.transform.GetChild(1).GetComponent<RawImage>();
									component80.texture = (Texture2D)GameCursor.BiohazardIcon;
									component80.color = GameTerrain.MinimapSettings.GetInfectionCol(equipment15.InfectedWith);
								}
								else
								{
									RawImage rawImage24 = AddRecipeLiquidContainer(gameObject2, ref lineLength10, ref lineIdx10, ref idx10);
									rawImage24.texture = equipment15.GetIcon(out var mat16, out var col14, actionMenu.SelectedAction == j);
									rawImage24.material = mat16;
									rawImage24.color = col14 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
									RawImage component81 = rawImage24.transform.GetChild(0).GetComponent<RawImage>();
									component81.texture = ((liquidPrototype4.Tex != null) ? liquidPrototype4.Tex.GetAsset() : null);
									Color color10 = liquidPrototype4.Col;
									if (equipment15.GetLiquidContentsAmount() == 0f)
									{
										color10.a = 0.5f;
									}
									component81.color = color10 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
								}
							}
							else if (equipment15.InfectedWith != InfectionType.None)
							{
								RawImage rawImage25 = AddRecipeLiquidContainer(gameObject2, ref lineLength10, ref lineIdx10, ref idx10);
								rawImage25.texture = equipment15.GetIcon(out var mat17, out var col15, actionMenu.SelectedAction == j);
								rawImage25.material = mat17;
								rawImage25.color = col15 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
								RawImage component82 = rawImage25.transform.GetChild(0).GetComponent<RawImage>();
								component82.texture = (Texture2D)GameCursor.BiohazardIcon;
								component82.color = GameTerrain.MinimapSettings.GetInfectionCol(equipment15.InfectedWith);
							}
							else
							{
								RawImage rawImage26 = AddRecipeIcon(gameObject2, ref lineLength10, ref lineIdx10, ref idx10);
								rawImage26.texture = equipment15.GetIcon(out var mat18, out var col16, actionMenu.SelectedAction == j);
								rawImage26.material = mat18;
								rawImage26.color = col16 * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							}
							if (actionType == CursorAction.AddMaterialToFire || actionType == CursorAction.LightFireWithMatch || actionType == CursorAction.Feed)
							{
								sb.Length = 0;
								sb.Append(' ');
								sb.Append('(');
								sb.AppendWithoutGarbage(equipment15.GetAmount());
								sb.Append(')');
								AddRecipeText(gameObject2, sb.ToString(), ref lineLength10, ref lineIdx10, ref idx10).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							}
						}
						else if ((actionType == CursorAction.PourWater || actionType == CursorAction.Refuel) && list[j].Object is Prop prop && prop.GetLiquidType() != null)
						{
							sb.Length = 0;
							sb.Append(' ');
							sb.Append('(');
							sb.Append(GameImpl.Translate(prop.GetLiquidType().GetNameKey()));
							sb.Append(')');
							AddRecipeText(gameObject2, sb.ToString(), ref lineLength10, ref lineIdx10, ref idx10).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						}
						EndRecipe(gameObject2, ref lineLength10, ref lineIdx10, ref idx10);
						break;
					}
					case CursorAction.LoadAmmo:
					{
						float lineLength6 = 0f;
						int lineIdx6 = 0;
						int idx6 = 0;
						_ = (Equipment)list[j].Target;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (list[j].Proto != null)
						{
							sb.Append(':');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(list[j].Proto.NameHash));
						}
						Texture2D icon = ((list[j].Proto.Tex != null) ? list[j].Proto.Tex.GetAsset() : null);
						bool flag11 = actionMenu.SelectedAction == j;
						AddRecipeTextIconPair(gameObject2, sb.ToString(), icon, flag11, ref lineLength6, ref lineIdx6, ref idx6).color = (flag11 ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						EndRecipe(gameObject2, ref lineLength6, ref lineIdx6, ref idx6);
						break;
					}
					case CursorAction.LoadNewMap:
					{
						Vector2 posXZ = ((EnterableVehicle)list[j].Target).GetPredictedOrElseThisVehicle().PosXZ;
						GameTerrain.Instance.GetNearestHitTheRoadPoint(posXZ, GameCursor.HitTheRoadRange, out var traits);
						float lineLength15 = 0f;
						int lineIdx15 = 0;
						int idx15 = 0;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						AddRecipeText(gameObject2, sb.ToString(), ref lineLength15, ref lineIdx15, ref idx15).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						AddRecipeSpacer(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
						switch (traits.Type)
						{
						case RoadDestinationType.Random:
						{
							RawImage rawImage36 = AddRecipeIcon(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
							rawImage36.texture = (Texture2D)GameCursor.TownIcon;
							rawImage36.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage36.color = GetTraitColor(traits.Urbanized);
							RawImage rawImage37 = AddRecipeIcon(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
							rawImage37.texture = (Texture2D)GameCursor.PopulatedIcon;
							rawImage37.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage37.color = GetTraitColor(traits.Populated);
							RawImage rawImage38 = AddRecipeIcon(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
							rawImage38.texture = (Texture2D)GameCursor.BiohazardIcon;
							rawImage38.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage38.color = GetTraitColor(traits.Infected);
							RawImage rawImage39 = AddRecipeIcon(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
							rawImage39.texture = (Texture2D)GameCursor.InvisibleStrainIcon;
							rawImage39.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage39.color = GetTraitColor(traits.InvisibleStrain);
							break;
						}
						case RoadDestinationType.KeepCurrentSettings:
							sb.Length = 0;
							sb.Append(GameImpl.Translate(HUD_SimilarArea));
							AddRecipeText(gameObject2, sb, ref lineLength15, ref lineIdx15, ref idx15).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							break;
						case RoadDestinationType.HarderSettings:
							sb.Length = 0;
							sb.Append(GameImpl.Translate(HUD_HarderArea));
							AddRecipeText(gameObject2, sb, ref lineLength15, ref lineIdx15, ref idx15).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							break;
						}
						AddRecipeSpacer(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
						sb.Length = 0;
						sb.Append('(');
						AddRecipeText(gameObject2, sb, ref lineLength15, ref lineIdx15, ref idx15).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						for (int num36 = 0; num36 < traits.SkillCapBonus; num36++)
						{
							RawImage rawImage40 = AddRecipeIcon(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
							rawImage40.texture = (Texture2D)GameCursor.SkillIcon;
							rawImage40.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage40.color = Color.white;
						}
						sb.Length = 0;
						sb.Append(')');
						AddRecipeText(gameObject2, sb, ref lineLength15, ref lineIdx15, ref idx15).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						EndRecipe(gameObject2, ref lineLength15, ref lineIdx15, ref idx15);
						break;
					}
					case CursorAction.Mine:
					case CursorAction.MineStealing:
					{
						float lineLength11 = 0f;
						int lineIdx11 = 0;
						int idx11 = 0;
						Equipment equipment16 = (Equipment)list[j].Target;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (list[j].Proto != null)
						{
							sb.Append(':');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(list[j].Proto.NameHash));
						}
						AddRecipeText(gameObject2, sb.ToString(), ref lineLength11, ref lineIdx11, ref idx11).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						RawImage rawImage27 = AddRecipeIcon(gameObject2, ref lineLength11, ref lineIdx11, ref idx11);
						rawImage27.texture = equipment16.GetIcon(out var mat19, out var col17, actionMenu.SelectedAction == j);
						rawImage27.material = mat19;
						rawImage27.color = col17;
						EndRecipe(gameObject2, ref lineLength11, ref lineIdx11, ref idx11);
						break;
					}
					case CursorAction.SetMiner:
					{
						TextMeshProUGUI component55 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (list[j].Proto != null)
						{
							sb.Append(':');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(list[j].Proto.NameHash));
						}
						component55.SetUnityTextIfDifferent(sb);
						component55.fontSize = 32f;
						component55.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.StartingPoints:
					{
						sb.Length = 0;
						sb.AppendWithoutGarbage(list[j].Amount);
						sb.Append(' ');
						sb.Append(GameImpl.Translate((list[j].Amount == 1) ? HUD_Point : HUD_Points));
						TextMeshProUGUI component26 = gameObject2.GetComponent<TextMeshProUGUI>();
						component26.SetUnityTextIfDifferent(sb);
						component26.fontSize = 32f;
						component26.color = Color.black * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.EquipmentSelect:
					case CursorAction.Gift:
					{
						RawImage component60 = gameObject2.FindChild(EquipmentIcon).GetComponent<RawImage>();
						RawImage component61 = gameObject2.FindChild(LiquidIcon).GetComponent<RawImage>();
						RawImage component62 = gameObject2.FindChild(InfectedWithIcon).GetComponent<RawImage>();
						RawImage component63 = gameObject2.FindChild(AmmoIcon).GetComponent<RawImage>();
						TextMeshProUGUI component64 = gameObject2.FindChild(Shortcut).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component65 = gameObject2.FindChild(EquipmentName).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component66 = gameObject2.FindChild(EquipmentLoadedAmmo).GetComponent<TextMeshProUGUI>();
						Color color6 = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						Equipment equipment11 = list[j].Target as Equipment;
						EquipmentPrototype proto3 = list[j].Proto;
						LiquidPrototype liquidPrototype3 = equipment11?.GetLiquidContentsType();
						if (liquidPrototype3 == null && equipment11 != null)
						{
							liquidPrototype3 = equipment11.DesignatedLiquid;
						}
						InfectionType infectedWith3 = list[j].InfectedWith;
						bool flag12 = equipment11?.GetPrototype().Special ?? false;
						Color32 color7 = equipment11?.GetPrototype().GetSpecialIconColor() ?? EquipmentIconBehaviour.SpecialIconCol;
						Material mat12 = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						Color col10 = Color.white;
						component60.texture = ((equipment11 != null) ? equipment11.GetIcon(out mat12, out col10, actionMenu.SelectedAction == j) : ((Texture2D)GameCursor.CrossIcon));
						component60.material = mat12;
						component60.color = col10;
						component61.gameObject.SetActive(liquidPrototype3 != null);
						component61.texture = ((liquidPrototype3 != null && liquidPrototype3.Tex != null) ? liquidPrototype3.Tex.GetAsset() : null);
						Color color8 = liquidPrototype3?.Col ?? MathUtil.TransparentBlack;
						color8.a = ((equipment11 != null && equipment11.GetLiquidContentsAmount() == 0f) ? 0.5f : 1f);
						component61.color = color8;
						component62.gameObject.SetActive(infectedWith3 != InfectionType.None || flag12);
						component62.texture = (Texture2D)((infectedWith3 != InfectionType.None) ? GameCursor.BiohazardIcon : GameCursor.SkillIconSmall);
						component62.color = ((infectedWith3 != InfectionType.None) ? GameTerrain.MinimapSettings.GetInfectionCol(infectedWith3) : color7);
						component63.gameObject.SetActive(proto3 != null);
						component63.texture = ((proto3 != null && proto3.Tex != null) ? proto3.Tex.GetAsset() : null);
						component63.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						bool flag13 = list[j].Shortcut != InputFunction.Invalid && InputFunctionManager.Instance.IsMapped(list[j].Shortcut);
						component64.gameObject.SetActive(flag13);
						if (flag13)
						{
							sb.Length = 0;
							sb.AppendButtonPromptString(list[j].Shortcut);
							component64.SetUnityTextIfDifferent(sb);
						}
						sb.Length = 0;
						if (equipment11 != null)
						{
							BuildEquipmentNameString(equipment11, list[j].Amount, sb);
						}
						else
						{
							sb.Append(GameImpl.Translate(HUD_None));
						}
						component65.SetUnityText(sb);
						component65.color = color6;
						sb.Length = 0;
						if (actionType == CursorAction.Gift)
						{
							if (equipment11 is Book)
							{
								equipment11.BuildDescriptionString(sb);
							}
						}
						else if (equipment11 is AmmoWeapon && proto3 != null)
						{
							sb.Append(GameImpl.Translate(proto3.NameHash));
							sb.Append(':');
							sb.Append(' ');
							sb.AppendWithoutGarbage(list[j].Actor.Inventory.GetAmmoCountOfType(proto3, infectedWith3));
						}
						component66.SetUnityText(sb);
						component66.color = color6;
						break;
					}
					case CursorAction.SetCropsPatchHere:
					{
						RawImage component38 = gameObject2.FindChild(EquipmentIcon).GetComponent<RawImage>();
						RawImage component39 = gameObject2.FindChild(LiquidIcon).GetComponent<RawImage>();
						RawImage component40 = gameObject2.FindChild(InfectedWithIcon).GetComponent<RawImage>();
						RawImage component41 = gameObject2.FindChild(AmmoIcon).GetComponent<RawImage>();
						TextMeshProUGUI component42 = gameObject2.FindChild(Shortcut).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component43 = gameObject2.FindChild(EquipmentName).GetComponent<TextMeshProUGUI>();
						TextMeshProUGUI component44 = gameObject2.FindChild(EquipmentLoadedAmmo).GetComponent<TextMeshProUGUI>();
						Color color4 = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						EquipmentPrototype proto2 = list[j].Proto;
						_ = (Material)((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						component38.texture = ((proto2.Tex != null) ? proto2.Tex.GetAsset() : null);
						component38.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						component39.gameObject.SetActive(value: false);
						component44.gameObject.SetActive(value: false);
						component40.gameObject.SetActive(value: false);
						component42.gameObject.SetActive(value: false);
						component41.gameObject.SetActive(value: false);
						sb.Length = 0;
						if (proto2.NameHash != 0)
						{
							sb.Append(GameImpl.Translate(proto2.NameHash, englishOnly: false));
						}
						component43.SetUnityText(sb);
						component43.color = color4;
						break;
					}
					case CursorAction.Tooltip:
					{
						TextMeshProUGUI component49 = gameObject2.GetComponent<TextMeshProUGUI>();
						component49.SetUnityText(list[j].GetCaption());
						component49.fontSize = 32f;
						component49.color = Color.black;
						break;
					}
					case CursorAction.ExitBuilding:
					{
						TextMeshProUGUI component89 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						int amount4 = list[j].Amount;
						if (list[j].Target is Building building2 && building2.GetEntranceDefs().Length > 1)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.Append(GameImpl.Translate(building2.GetEntranceDefs()[amount4].NameHash));
							sb.Append(')');
						}
						component89.SetUnityTextIfDifferent(sb);
						component89.fontSize = 32f;
						component89.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.ChangeBuildingSlot:
					{
						TextMeshProUGUI component68 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						int amount3 = list[j].Amount;
						if (list[j].Target is Building building && amount3 < building.GetInhabitantSlotDefs().Length)
						{
							sb.Append(':');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(building.GetInhabitantSlotDefs()[amount3].NameHash));
						}
						component68.SetUnityTextIfDifferent(sb);
						component68.fontSize = 32f;
						component68.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.LocateEquipment:
					{
						BuildLocateEquipmentString(list[j]);
						TextMeshProUGUI component58 = gameObject2.GetComponent<TextMeshProUGUI>();
						component58.SetUnityTextIfDifferent(sb);
						component58.fontSize = 32f;
						component58.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.SetMarker:
					case CursorAction.ClearMarker:
					{
						RawImage component69 = gameObject2.FindChild(Icon).GetComponent<RawImage>();
						TextMeshProUGUI component70 = gameObject2.FindChild(Name).GetComponent<TextMeshProUGUI>();
						component69.texture = (Texture2D)GameCursor.CurrentQuestIcon;
						component69.color = MapPage.GetMapMarkerColor(list[j].MarkerType);
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						component70.SetUnityTextIfDifferent(sb);
						component70.fontSize = 32f;
						component70.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.ResumeCrafting:
					{
						TextMeshProUGUI component24 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						sb.Append(' ');
						sb.Append('(');
						Recipe recipe2 = list[j].Recipe;
						CraftingProp craftingProp = list[j].Target as CraftingProp;
						if (recipe2.ProductPrototype != null)
						{
							sb.Append(GameImpl.Translate(recipe2.ProductPrototype.NameHash));
						}
						else if (recipe2.ProductLiquidPrototype != null)
						{
							sb.Append(GameImpl.Translate(recipe2.ProductLiquidPrototype.NameHash));
						}
						else if (recipe2.ProductPropPrototype != null)
						{
							sb.Append(GameImpl.Translate(recipe2.ProductPropPrototype.NameHash));
						}
						if (craftingProp.CurrentCrafter != null)
						{
							sb.Append(' ');
							sb.Append('-');
							sb.Append(' ');
							craftingProp.CurrentCrafter.BuildDisplayName(sb, noStrangers: false, englishOnly: false);
						}
						sb.Append(')');
						component24.SetUnityTextIfDifferent(sb);
						component24.fontSize = 32f;
						component24.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.CancelGathering:
					case CursorAction.CancelGuarding:
					case CursorAction.CancelFarming:
					case CursorAction.CancelLumberjack:
					case CursorAction.CancelCook:
					case CursorAction.CancelMining:
					case CursorAction.CancelTrapper:
					case CursorAction.CancelBuildingRole:
					case CursorAction.CancelCraftingRole:
					case CursorAction.CancelCapturing:
					case CursorAction.CancelRepairing:
					case CursorAction.CancelAnimalFeeding:
					case CursorAction.CancelOrganizing:
					case CursorAction.ResumeGathering:
					case CursorAction.ResumeGuarding:
					case CursorAction.ResumeFarming:
					case CursorAction.ResumeLumberjack:
					case CursorAction.ResumeCook:
					case CursorAction.ResumeMining:
					case CursorAction.ResumeTrapper:
					case CursorAction.ResumeBuildingRole:
					case CursorAction.ResumeCraftingRole:
					case CursorAction.ResumeCapturing:
					case CursorAction.ResumeRepairing:
					case CursorAction.ResumeAnimalFeeding:
					case CursorAction.ResumeOrganizing:
					case CursorAction.PauseGathering:
					case CursorAction.PauseGuarding:
					case CursorAction.PauseFarming:
					case CursorAction.PauseLumberjack:
					case CursorAction.PauseCook:
					case CursorAction.PauseMining:
					case CursorAction.PauseTrapper:
					case CursorAction.PauseBuildingRole:
					case CursorAction.PauseCraftingRole:
					case CursorAction.PauseCapturing:
					case CursorAction.PauseRepairing:
					case CursorAction.PauseAnimalFeeding:
					case CursorAction.PauseOrganizing:
					case CursorAction.CancelMedic:
					case CursorAction.ResumeMedic:
					case CursorAction.PauseMedic:
					{
						float lineLength = 0f;
						int lineIdx = 0;
						int idx = 0;
						RoleInfo roleInfo = default(RoleInfo);
						switch (list[j].ActionType)
						{
						case CursorAction.CancelGathering:
						case CursorAction.ResumeGathering:
						case CursorAction.PauseGathering:
							roleInfo = new RoleInfo(Role.Gatherer, list[j].Tile, list[j].Proto);
							break;
						case CursorAction.CancelMining:
						case CursorAction.ResumeMining:
						case CursorAction.PauseMining:
							roleInfo = new RoleInfo(Role.Miner, list[j].Proto);
							break;
						case CursorAction.CancelGuarding:
						case CursorAction.ResumeGuarding:
						case CursorAction.PauseGuarding:
							roleInfo = new RoleInfo(Role.Guard);
							break;
						case CursorAction.CancelFarming:
						case CursorAction.ResumeFarming:
						case CursorAction.PauseFarming:
							roleInfo = new RoleInfo(Role.Farmer);
							break;
						case CursorAction.CancelLumberjack:
						case CursorAction.ResumeLumberjack:
						case CursorAction.PauseLumberjack:
							roleInfo = new RoleInfo(Role.Lumberjack);
							break;
						case CursorAction.CancelCook:
						case CursorAction.ResumeCook:
						case CursorAction.PauseCook:
							roleInfo = new RoleInfo(Role.Cook);
							break;
						case CursorAction.CancelTrapper:
						case CursorAction.ResumeTrapper:
						case CursorAction.PauseTrapper:
							roleInfo = new RoleInfo(Role.Trapper);
							break;
						case CursorAction.CancelBuildingRole:
						case CursorAction.ResumeBuildingRole:
						case CursorAction.PauseBuildingRole:
							roleInfo = new RoleInfo(Role.Builder);
							break;
						case CursorAction.CancelCapturing:
						case CursorAction.ResumeCapturing:
						case CursorAction.PauseCapturing:
							roleInfo = new RoleInfo(Role.Capturing, list[j].Tile);
							break;
						case CursorAction.CancelRepairing:
						case CursorAction.ResumeRepairing:
						case CursorAction.PauseRepairing:
							roleInfo = new RoleInfo(Role.Repairing, list[j].Tile);
							break;
						case CursorAction.CancelAnimalFeeding:
						case CursorAction.ResumeAnimalFeeding:
						case CursorAction.PauseAnimalFeeding:
							roleInfo = new RoleInfo(Role.AnimalFeeder);
							break;
						case CursorAction.CancelCraftingRole:
						case CursorAction.ResumeCraftingRole:
						case CursorAction.PauseCraftingRole:
							roleInfo = new RoleInfo(Role.Crafter, list[j].Recipe, list[j].Tile);
							break;
						case CursorAction.CancelOrganizing:
						case CursorAction.ResumeOrganizing:
						case CursorAction.PauseOrganizing:
							roleInfo = new RoleInfo(Role.Organizer);
							break;
						case CursorAction.CancelMedic:
						case CursorAction.ResumeMedic:
						case CursorAction.PauseMedic:
							roleInfo = new RoleInfo(Role.Medic);
							break;
						}
						roleInfo = list[j].Actor.GetRoleInfo(roleInfo);
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						sb.Append(':');
						sb.Append(' ');
						AddRecipeText(gameObject2, sb, ref lineLength, ref lineIdx, ref idx).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						Texture2D roleDisplayIcon = RoleDisplayBehaviour.GetRoleDisplayIcon(list[j].Actor, roleInfo);
						RawImage rawImage = AddRecipeIcon(gameObject2, ref lineLength, ref lineIdx, ref idx);
						rawImage.texture = roleDisplayIcon;
						rawImage.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						rawImage.color = roleInfo.GetRoleIconCol() * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						sb.Length = 0;
						RoleDisplayBehaviour.GetRoleDisplayText(list[j].Actor, roleInfo, RoleDisplayTextMode.Medium, sb);
						AddRecipeText(gameObject2, sb, ref lineLength, ref lineIdx, ref idx).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						if (roleInfo.ResourceType != null)
						{
							RawImage rawImage2 = AddRecipeIcon(gameObject2, ref lineLength, ref lineIdx, ref idx);
							rawImage2.texture = ((roleInfo.ResourceType.Tex != null) ? roleInfo.ResourceType.Tex.GetAsset() : null);
							rawImage2.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
							rawImage2.color = Color.white * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						}
						if (roleInfo.Recipe != null && (roleInfo.Recipe.ProductPrototype != null || roleInfo.Recipe.ProductLiquidPrototype != null))
						{
							RawImage rawImage3 = AddRecipeIcon(gameObject2, ref lineLength, ref lineIdx, ref idx);
							rawImage3.texture = ((roleInfo.Recipe.ProductLiquidPrototype != null && roleInfo.Recipe.ProductLiquidPrototype.Tex != null) ? roleInfo.Recipe.ProductLiquidPrototype.Tex.GetAsset() : ((roleInfo.Recipe.ProductPrototype != null && roleInfo.Recipe.ProductPrototype.Tex != null) ? roleInfo.Recipe.ProductPrototype.Tex.GetAsset() : null));
							rawImage3.color = ((roleInfo.Recipe.ProductLiquidPrototype != null) ? ((Color)roleInfo.Recipe.ProductLiquidPrototype.Col) : Color.white) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
							rawImage3.material = ((actionMenu.SelectedAction == j) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
						}
						EndRecipe(gameObject2, ref lineLength, ref lineIdx, ref idx);
						break;
					}
					case CursorAction.SetBlockAnimals:
					{
						TextMeshProUGUI component86 = gameObject2.GetComponent<TextMeshProUGUI>();
						Gate gate2 = list[j].Target as Gate;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (gate2 != null)
						{
							sb.Append(':');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(gate2.BlockAnimals ? HUD_Yes : HUD_No));
						}
						component86.SetUnityTextIfDifferent(sb);
						component86.fontSize = 32f;
						component86.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.SetGatePolicy:
					{
						TextMeshProUGUI component59 = gameObject2.GetComponent<TextMeshProUGUI>();
						Gate gate = list[j].Target as Gate;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (gate != null)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.Append(GameImpl.Translate(Gate.GetGatePolicyStringHash(gate.GatePolicy)));
							sb.Append(')');
						}
						component59.SetUnityTextIfDifferent(sb);
						component59.fontSize = 32f;
						component59.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.SetFoodPolicy:
					case CursorAction.SetDrinkPolicy:
					case CursorAction.SetAmmoPolicy:
					case CursorAction.SetWeaponAmmoPolicy:
					case CursorAction.SetEquipmentPolicy:
					{
						TextMeshProUGUI component6 = gameObject2.GetComponent<TextMeshProUGUI>();
						Character obj = list[j].Object as Character;
						EquipmentPrototype proto = list[j].Proto;
						LiquidPrototype liquid = list[j].Liquid;
						InfectionType infectedWith = list[j].InfectedWith;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (actionType == CursorAction.SetWeaponAmmoPolicy)
						{
							sb.Append(' ');
							sb.Append('-');
							sb.Append(' ');
							sb.Append(GameImpl.Translate(proto.NameHash));
						}
						sb.Append(' ');
						sb.Append('(');
						bool sameAsCommunity;
						int equipmentPolicyMask = obj.GetEquipmentPolicyMask(proto, liquid, infectedWith, includeCommunityPolicy: true, out sameAsCommunity);
						float targetAmountToCarryIncludingAmmo = obj.GetTargetAmountToCarryIncludingAmmo(proto, liquid, infectedWith);
						int noRestrictionsPolicyMask = EquipmentPolicy.NoRestrictionsPolicyMask;
						if (equipmentPolicyMask == noRestrictionsPolicyMask && targetAmountToCarryIncludingAmmo == 0f)
						{
							sb.Append(GameImpl.Translate(HUD_NoRestrictions));
						}
						else
						{
							bool flag5 = false;
							if (targetAmountToCarryIncludingAmmo != 0f)
							{
								sb.Append(GameImpl.Translate(HUD_Target));
								sb.Append(':');
								sb.Append(' ');
								if (liquid != null)
								{
									bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
									sb.AppendWithoutGarbage(targetAmountToCarryIncludingAmmo * (useMetricWeights ? 0.0295735f : 1f), 1);
									sb.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
								}
								else
								{
									sb.AppendWithoutGarbage((int)targetAmountToCarryIncludingAmmo);
								}
								flag5 = true;
							}
							for (int k = 0; k < 8; k++)
							{
								if ((equipmentPolicyMask & (1 << k)) == (noRestrictionsPolicyMask & (1 << k)) || !EquipmentPolicy.IsActionPossibleForItem((EquipmentPolicyAction)k, proto, liquid))
								{
									continue;
								}
								if (flag5)
								{
									sb.Append(',');
									sb.Append(' ');
								}
								Language language = GameImpl.Instance.Settings.Language;
								GenderType gender = liquid?.GetGenderInLanguage(language) ?? proto?.GetGenderInLanguage(language) ?? GenderType.Count;
								bool plural = liquid == null && (proto?.UsePluralIndefiniteArticle ?? false);
								switch ((EquipmentPolicyAction)k)
								{
								case EquipmentPolicyAction.CanUse:
									if (list[j].ActionType == CursorAction.SetFoodPolicy)
									{
										sb.Append(GameImpl.Translate(HUD_DontEat, HUD_DontEat_Female, HUD_DontEat_Plural, HUD_DontEat_Female_Plural, gender, plural));
									}
									else if (list[j].ActionType == CursorAction.SetDrinkPolicy)
									{
										sb.Append(GameImpl.Translate(HUD_DontDrink, HUD_DontDrink_Female, HUD_DontDrink_Plural, HUD_DontDrink_Female_Plural, gender, plural));
									}
									else
									{
										sb.Append(GameImpl.Translate(HUD_DontUse, HUD_DontUse_Female, HUD_DontUse_Plural, HUD_DontUse_Female_Plural, gender, plural));
									}
									break;
								case EquipmentPolicyAction.CanCraftWith:
									if (list[j].ActionType == CursorAction.SetFoodPolicy)
									{
										sb.Append(GameImpl.Translate(HUD_DontCookWith, HUD_DontCookWith_Female, HUD_DontCookWith_Plural, HUD_DontCookWith_Female_Plural, gender, plural));
									}
									else
									{
										sb.Append(GameImpl.Translate(HUD_DontCraftWith, HUD_DontCraftWith_Female, HUD_DontCraftWith_Plural, HUD_DontCraftWith_Female_Plural, gender, plural));
									}
									break;
								case EquipmentPolicyAction.CanPlant:
									sb.Append(GameImpl.Translate(HUD_DontPlant, HUD_DontPlant_Female, HUD_DontPlant_Plural, HUD_DontPlant_Female_Plural, gender, plural));
									break;
								case EquipmentPolicyAction.CanFeedToAnimals:
									sb.Append(GameImpl.Translate(HUD_DontFeedToAnimals, HUD_DontFeedToAnimals_Female, HUD_DontFeedToAnimals_Plural, HUD_DontFeedToAnimals_Female_Plural, gender, plural));
									break;
								case EquipmentPolicyAction.CanShare:
									sb.Append(GameImpl.Translate(HUD_DontShare, HUD_DontShare_Female, HUD_DontShare_Plural, HUD_DontShare_Female_Plural, gender, plural));
									break;
								case EquipmentPolicyAction.CanStrip:
									sb.Append(GameImpl.Translate(HUD_DontStrip, HUD_DontStrip_Female, HUD_DontStrip_Plural, HUD_DontStrip_Female_Plural, gender, plural));
									break;
								case EquipmentPolicyAction.AutoCollect:
									sb.Append(GameImpl.Translate(HUD_AutoCollect));
									break;
								case EquipmentPolicyAction.AutoDeposit:
									sb.Append(GameImpl.Translate(HUD_AutoDeposit));
									break;
								}
								flag5 = true;
							}
							if (!flag5)
							{
								sb.Append(GameImpl.Translate(HUD_NoRestrictions));
							}
						}
						sb.Append(')');
						component6.SetUnityTextIfDifferent(sb);
						component6.fontSize = 32f;
						component6.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.SetDesignatedLiquid:
					{
						TextMeshProUGUI component88 = gameObject2.GetComponent<TextMeshProUGUI>();
						Equipment equipment17 = list[j].Target as Equipment;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						sb.Append(' ');
						sb.Append('(');
						if (equipment17.DesignatedLiquid == null)
						{
							sb.Append(GameImpl.Translate(HUD_NoRestrictions));
						}
						else
						{
							sb.Append(GameImpl.Translate(equipment17.DesignatedLiquid.NameHash));
						}
						sb.Append(')');
						component88.SetUnityTextIfDifferent(sb);
						component88.fontSize = 32f;
						component88.color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.ChokeHold:
					case CursorAction.SlitThroat:
					{
						float lineLength9 = 0f;
						int lineIdx9 = 0;
						int idx9 = 0;
						Character actor8 = list[j].Actor;
						Character character3 = list[j].Object as Character;
						Equipment equipment12 = (Equipment)list[j].Target;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (character3 != null)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.Append(GameImpl.Translate(actor8.WouldChokeBeDetectable(character3) ? HUD_Detectable : HUD_Hidden));
							sb.Append(')');
						}
						AddRecipeText(gameObject2, sb.ToString(), ref lineLength9, ref lineIdx9, ref idx9).color = ((actionMenu.SelectedAction == j) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						if (equipment12 != null)
						{
							RawImage rawImage22 = AddRecipeIcon(gameObject2, ref lineLength9, ref lineIdx9, ref idx9);
							rawImage22.texture = equipment12.GetIcon(out var mat13, out var col11, actionMenu.SelectedAction == j);
							rawImage22.material = mat13;
							rawImage22.color = col11;
						}
						EndRecipe(gameObject2, ref lineLength9, ref lineIdx9, ref idx9);
						break;
					}
					case CursorAction.Pickpocket:
					{
						Character actor6 = list[j].Actor;
						Character character2 = list[j].Target as Character;
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						if (character2 != null && character2.Community != null)
						{
							Character closestMember = null;
							bool flag10 = character2.Community.IsCharacterVisibleToAnyMember(actor6, out closestMember, notIncludingCaptives: true);
							sb.Append(' ');
							sb.Append('(');
							sb.Append(GameImpl.Translate(flag10 ? HUD_Detectable : HUD_Hidden));
							sb.Append(')');
						}
						TextMeshProUGUI component37 = gameObject2.GetComponent<TextMeshProUGUI>();
						component37.SetUnityText(sb);
						component37.fontSize = 32f;
						component37.color = ((actionMenu.SelectedAction == j && i != 0) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.Take:
					case CursorAction.Give:
					case CursorAction.Store:
					case CursorAction.Sell:
					case CursorAction.Buy:
					{
						TextMeshProUGUI component16 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append(list[j].GetCaption());
						list[j].GetTransferVars(out var _, out var _, out var _, out var _, out var minTransferrable, out var maxTransferrable);
						if (minTransferrable > 0 && minTransferrable < maxTransferrable && maxTransferrable > 1)
						{
							sb.Append(' ');
							sb.Append('(');
							sb.AppendButtonPromptString(InputFunction.AltAction);
							sb.Append('x');
							sb.AppendWithoutGarbage(minTransferrable);
							sb.Append(')');
						}
						component16.SetUnityTextIfDifferent(sb);
						component16.fontSize = 32f;
						component16.color = ((actionMenu.SelectedAction == j && i != 0) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					case CursorAction.SpeechToReplyTo:
					{
						TextMeshProUGUI component15 = gameObject2.GetComponent<TextMeshProUGUI>();
						sb.Length = 0;
						sb.Append('"');
						sb.Append(list[j].GetCaption());
						sb.Append('"');
						component15.SetUnityTextIfDifferent(sb);
						component15.fontSize = 32f;
						component15.color = Color.gray;
						break;
					}
					default:
					{
						TextMeshProUGUI component3 = gameObject2.GetComponent<TextMeshProUGUI>();
						component3.SetUnityText(list[j].GetCaption());
						component3.fontSize = 32f;
						component3.color = ((actionMenu.SelectedAction == j && i != 0) ? Color.red : Color.black) * ((list[j].Enabled == CursorActionDisabledReason.Enabled) ? 1f : 0.5f);
						break;
					}
					}
					if (actionType != CursorAction.Separator)
					{
						num = Mathf.Max(num, ((RectTransform)gameObject2.transform).rect.width);
					}
				}
				for (; j < gameObject.transform.childCount; j++)
				{
					UnityEngine.Object.Destroy(gameObject.transform.GetChild(j).gameObject);
				}
			}
			for (int num37 = 0; num37 < Separators.Count; num37++)
			{
				Separators[num37].preferredWidth = num;
			}
			RectTransform rectTransform = (RectTransform)actionMenuBehaviour.transform;
			Vector2 vector2 = vector - Centre + new Vector2((instance4 != null && instance4.Cursor.IsPlacingBuilding) ? ActionMenuXOffsetBuildingMode : ActionMenuXOffsetFlyMode, 0f) * (flag4 ? 1f : (-1f));
			if (flag)
			{
				vector2 = Vector2.zero - new Vector2(ActionMenuXOffset, 0f);
				if (instance4 != null && instance4.LocalControlledCharacter != null)
				{
					Character predictedOrElseThisCharacter = instance4.LocalControlledCharacter.GetPredictedOrElseThisCharacter();
					vector2 = gameCamera.GetScreenPosFromPointInWorld((predictedOrElseThisCharacter.InsideBuilding as EnterableVehicle)?.GetPredictedOrElseThisVehicle().CentreOfMass ?? predictedOrElseThisCharacter.EyePosition) - Centre - new Vector2(ActionMenuXOffset, 0f);
					vector2.x = Mathf.Clamp(vector2.x, (0f - Size.x) * 0.5f, MainPanelRectTransform.rect.width - Size.x * 0.5f);
					vector2.y = Mathf.Clamp(vector2.y, rectTransform.rect.height * 0.5f - Size.y * 0.5f, Size.y * 0.5f - rectTransform.rect.height * 0.5f);
				}
			}
			float num38 = (0f - Size.y) * 0.5f;
			float num39 = Size.y * 0.5f;
			float num40 = vector2.y - rectTransform.rect.height * 0.5f;
			float num41 = vector2.y + rectTransform.rect.height * 0.5f;
			if (num40 < num38)
			{
				vector2.y += num38 - num40;
				actionMenuBehaviour.Tail.rectTransform.anchoredPosition = new Vector2(actionMenuBehaviour.Tail.rectTransform.anchoredPosition.x, num40 - num38);
			}
			else if (num41 > num39)
			{
				vector2.y -= num41 - num39;
				actionMenuBehaviour.Tail.rectTransform.anchoredPosition = new Vector2(actionMenuBehaviour.Tail.rectTransform.anchoredPosition.x, num41 - num39);
			}
			else
			{
				actionMenuBehaviour.Tail.rectTransform.anchoredPosition = new Vector2(actionMenuBehaviour.Tail.rectTransform.anchoredPosition.x, 0f);
			}
			if (ActionMenuVisibleFrames < 2)
			{
				vector2 += new Vector2(0f, MainPanelRectTransform.rect.height + 10000f);
			}
			actionMenuBehaviour.transform.localPosition = vector2;
			ActionMenuVisibleFrames++;
		}
		else
		{
			ActionMenuVisibleFrames = 0;
		}
	}

	private void UpdateUnityPipSpeechBubble()
	{
		Character character = ((Hud.Instance.Pip.FocusObject is Character character2) ? character2.GetPredictedOrElseThisCharacter() : null);
		bool flag = character != null && character.Speaking != null && !NotificationManager.Instance.IsDisplayingNotification();
		UnityScreenSpeechBubble.gameObject.SetActive(flag || !UnityScreenSpeechBubble.IsFinished());
		if (flag)
		{
			UnityScreenSpeechBubble.SetSpeaking(character.Speaking, character.SpeechStartTime, character.SpeakingText, character.SpeakingTextEnglish, forceUpdate: false);
		}
	}

	public bool IsShowingPipSpeechBubble()
	{
		return UnityScreenSpeechBubble.gameObject.activeSelf;
	}

	public bool IsChatting()
	{
		return CurrentChatState != ChatState.Closed;
	}

	public StringBuilder BuildCraftingLimitString(AvailableAction action)
	{
		sb.Length = 0;
		if (action.Amount < int.MaxValue)
		{
			sb.Append(GameImpl.Translate(HUD_DesiredAmount));
			sb.Append(' ');
			if (action.Proto != null)
			{
				sb.AppendWithoutGarbage(action.Amount);
			}
			else if (action.Liquid != null)
			{
				bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
				sb.AppendWithoutGarbage((float)action.Amount * (useMetricWeights ? 0.0295735f : 1f), 1);
				sb.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
			}
		}
		else
		{
			sb.Append(GameImpl.Translate(HUD_SetDesiredAmount));
		}
		return sb;
	}

	public StringBuilder BuildLocateEquipmentString(AvailableAction action)
	{
		sb.Length = 0;
		action.Object.BuildDisplayName(sb, InfoScreen.GetAllowViewInfoOnAnyone(), englishOnly: false);
		sb.Append(' ');
		sb.Append('(');
		if (action.Proto != null)
		{
			sb.AppendWithoutGarbage((int)action.FloatAmount);
		}
		else if (action.Liquid != null)
		{
			bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
			sb.AppendWithoutGarbage(action.FloatAmount * (useMetricWeights ? 0.0295735f : 1f), 1);
			sb.Append(GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz));
		}
		sb.Append(')');
		return sb;
	}

	public void OnCloseNotification()
	{
		NotificationManager.Instance.CloseCurrentNotification();
	}

	public void SetStatusBarMsg(AvailableAction action)
	{
		StringBuilder stringBuilder = new StringBuilder();
		action.BuildDisabledReasonString(stringBuilder);
		StatusBarMsg = stringBuilder.ToString();
		StatusBarMsgTimeout = MsgTimeout;
		StatusBarMsgShowForOneFrame = false;
		StatusBarMsgAction = action;
	}

	public void SetStatusBarMsg(string msg)
	{
		StatusBarMsg = msg;
		StatusBarMsgTimeout = MsgTimeout;
		StatusBarMsgShowForOneFrame = false;
		StatusBarMsgAction = default(AvailableAction);
	}

	public void SetStatusBarMsgThisFrame(string msg)
	{
		StatusBarMsg = msg;
		StatusBarMsgTimeout = 0.001f;
		StatusBarMsgShowForOneFrame = true;
		StatusBarMsgAction = default(AvailableAction);
	}

	public void ClearStatusBarMsg()
	{
		StatusBarMsg = null;
		StatusBarMsgTimeout = 0f;
		StatusBarMsgShowForOneFrame = false;
		StatusBarMsgAction = default(AvailableAction);
	}

	public void ShowRefuseToAttackMsg(Character character, TileObject targetObj)
	{
		string str = GameImpl.Translate("HINT_RefuseToAttack").Replace("%1", character.GetDisplayNameString()).Replace("%2", targetObj.GetDisplayNameString(noStrangers: false, englishOnly: false));
		str = StringUtil.ApplyFormulaeWithHashes(str, character, character, character.GetDisplayNameHashIfGeneric(noStrangers: false), targetObj, targetObj.GetDisplayNameHashIfGeneric(noStrangers: false));
		Instance.SetStatusBarMsg(str);
	}

	public void ShowOutsideZoneMsg(Character character)
	{
		string str = GameImpl.Translate("HUD_OutsideZone").Replace("%1", character.GetDisplayNameString());
		str = StringUtil.ApplyFormulae(str, character, character);
		Instance.SetStatusBarMsg(str);
	}

	public void BuildInLaborMsg(Character character, int accompanied)
	{
		HintText.Length = 0;
		HintText.Append(GameImpl.Translate(HINT_InLabor).Replace("%1", character.GetDisplayNameString()).Replace("%2", accompanied.ToString()));
		StringUtil.ApplyFormulae(HintText, character, character);
	}

	public bool IsShowingStatusBarMsg()
	{
		return StatusBarMsgTimeout > 0f;
	}

	public bool IsShowingRecentStatusBarMsg()
	{
		return StatusBarMsgTimeout > MsgTimeout * 0.5f;
	}

	public void UpdateUnityStatusBar()
	{
		Session instance = Session.Instance;
		Hud instance2 = Hud.Instance;
		if (StatusBarMsgTimeout > 0f && !StatusBarMsgShowForOneFrame)
		{
			StatusBarMsgTimeout -= Time.unscaledDeltaTime;
			if (StatusBarMsgTimeout <= 0f || (StatusBarMsgAction.ActionType != CursorAction.None && !StatusBarMsgAction.IsEqual(instance2.Cursor.GetAvailableAction())))
			{
				ClearStatusBarMsg();
			}
		}
		StatusBarMsgShowForOneFrame = false;
		string text = null;
		bool flag = false;
		if (!NotificationManager.Instance.IsDisplayingNotification())
		{
			text = StatusBarMsg;
			if (string.IsNullOrEmpty(text))
			{
				text = OnlineParty.Instance.GetCurrentStatus();
			}
			if (string.IsNullOrEmpty(text) && instance2.LocalControlledCharacter != null && instance.CountdownToTimeJump == 0 && !instance.WantSlowerTransitionIn)
			{
				if (instance2.LocalControlledCharacter.Consciousness == Consciousness.Unconscious)
				{
					HintText.Length = 0;
					HintText.Append(GameImpl.Translate(HINT_Unconscious).Replace("%1", instance2.LocalControlledCharacter.GetDisplayNameString()));
					StringUtil.ApplyFormulae(HintText, instance2.LocalControlledCharacter, instance2.LocalControlledCharacter);
					text = HintText.ToString();
					flag = true;
				}
				else
				{
					Character character = instance2.LocalTargetObject as Character;
					AIOverridesControlReason aIOverridesControlReason = instance2.LocalControlledCharacter.AIOverridesControl();
					if (aIOverridesControlReason == AIOverridesControlReason.Depressed)
					{
						HintText.Length = 0;
						HintText.Append(GameImpl.Translate(HINT_Depressed).Replace("%1", instance2.LocalControlledCharacter.GetDisplayNameString()));
						StringUtil.ApplyFormulae(HintText, instance2.LocalControlledCharacter, instance2.LocalControlledCharacter);
						text = HintText.ToString();
						flag = true;
					}
					else if (aIOverridesControlReason == AIOverridesControlReason.Feuding)
					{
						HintText.Length = 0;
						HintText.Append(GameImpl.Translate(HINT_Feuding).Replace("%1", instance2.LocalControlledCharacter.GetDisplayNameString()));
						StringUtil.ApplyFormulae(HintText, instance2.LocalControlledCharacter, instance2.LocalControlledCharacter);
						text = HintText.ToString();
						flag = true;
					}
					else if (aIOverridesControlReason == AIOverridesControlReason.InLabor && instance2.LocalControlledCharacter.IsPregnant())
					{
						BuildInLaborMsg(instance2.LocalControlledCharacter, ((Human)instance2.LocalControlledCharacter).CalcAccompaniedAmountInLabor());
						text = HintText.ToString();
					}
					else if (character != null && character.AliveAndNotZombie && character.GetBaseObjectType() == BaseObjectType.Human && instance.DifficultySettings.InvisibleStrainPercentage > 0f && !instance2.LocalWantLockOnTarget && instance2.LocalControlledCharacter.Inventory.FindItemOfClass(typeof(BrainScanner)) != null)
					{
						int num = ((character.OriginalId != 0) ? character.OriginalId : character.Id);
						float num2 = 0f;
						int val = 0;
						switch (character.InvisibleStrain)
						{
						case InvisibleStrainType.None:
							if (!character.IsPlayerAvatar())
							{
								num2 = Mathf.Pow(MathUtil.RandomFloat(num), 8f);
								val = 0;
							}
							break;
						case InvisibleStrainType.Excitable:
							num2 = 1f - MathUtil.Squared(MathUtil.RandomFloat(num));
							val = 1;
							break;
						case InvisibleStrainType.Subtle:
							num2 = MathUtil.GetNormallyDistributedRand(num, 4);
							val = 1;
							break;
						}
						int num3 = Math.Max((int)(num2 * 100f), val);
						HintText.Length = 0;
						HintText.Append(GameImpl.Translate(HINT_BrainScanReadout).Replace("%1", num3.ToString()));
						StringUtil.ApplyFormulae(HintText, instance2.LocalControlledCharacter, instance2.LocalControlledCharacter);
						text = HintText.ToString();
						flag = true;
					}
					else if (instance2.LocalControlledCharacter.Encumbered)
					{
						HintText.Length = 0;
						HintText.Append(GameImpl.Translate(HINT_Encumbered).Replace("%1", instance2.LocalControlledCharacter.GetDisplayNameString()));
						StringUtil.ApplyFormulae(HintText, instance2.LocalControlledCharacter, instance2.LocalControlledCharacter);
						text = HintText.ToString();
						flag = true;
					}
					else if (GameImpl.Instance.IsDemo() && instance.PlayTime >= Session.DemoTimeoutWarning)
					{
						HintText.Length = 0;
						HintText.Append(GameImpl.Translate(HINT_DemoTimeout));
						HintText.Append(' ');
						TimeSpan timeSpan = TimeSpan.FromTicks(Math.Max(0L, (Session.DemoTimeout - instance.PlayTime).Ticks));
						if (timeSpan.Minutes > 0)
						{
							HintText.AppendWithoutGarbage(timeSpan.Minutes);
							HintText.Append(':');
						}
						if (timeSpan.Seconds < 10)
						{
							HintText.Append('0');
						}
						HintText.AppendWithoutGarbage(timeSpan.Seconds);
						HintText.Append(GameImpl.Translate(HINT_Seconds));
						text = HintText.ToString();
					}
					else if (instance.CommunityManager.PlayerCommunity != null && instance.CommunityManager.PlayerCommunity.GivingBirth.Count > 0)
					{
						Character character2 = instance.CommunityManager.PlayerCommunity.GivingBirth[0];
						if (character2.IsPregnant())
						{
							int num4 = ((Human)character2).CalcAccompaniedAmountInLabor();
							if (num4 < 5)
							{
								BuildInLaborMsg(character2, num4);
								text = HintText.ToString();
							}
						}
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			UnityStatusBar.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>().SetUnityText(text);
		}
		StatusBarTransition = Mathf.Clamp01(StatusBarTransition + ((!string.IsNullOrEmpty(text)) ? 1f : (-1f)) * 8f * Time.unscaledDeltaTime);
		float num5 = Mathf.Clamp01(StatusBarTransition - InfoScreen.Instance.Transition);
		RectTransform rectTransform = (RectTransform)UnityStatusBar.transform;
		rectTransform.anchoredPosition = new Vector2(0f, Mathf.SmoothStep(0f, 1f, 1f - num5) * rectTransform.rect.height);
		if (StatusBarTransition > 0f && !UnityStatusBar.activeSelf && !flag)
		{
			SoundManager.PlayMenuSound(SoundManager.StatusNotificationSound);
		}
		UnityStatusBar.SetActive(StatusBarTransition > 0f);
		UnityStatusBarSpacer.SetActive(UnityNewNotifications.gameObject.activeSelf);
	}

	public void OnChat(string txt)
	{
		if (Session.Instance != null && IsChatting())
		{
			Session.Instance.IncomingChatText = txt;
		}
		CurrentChatState = ChatState.WantClosed;
	}

	public void ShowCharacterSwitchBar()
	{
		CharacterSwitchBarTimeout = CharacterSwitchBarTime;
	}

	public void UpdateCharacterSwitchBar()
	{
		if (IsChatting() || InfoScreen.Instance.Active)
		{
			CharacterSwitchBarTimeout = 0f;
		}
		CharacterSwitchBarTimeout = Math.Max(0f, CharacterSwitchBarTimeout - Time.unscaledDeltaTime);
		CharacterSwitchBarTransition = Mathf.Clamp(CharacterSwitchBarTransition + ((CharacterSwitchBarTimeout > 0f) ? 1f : (-1f)) * Time.unscaledDeltaTime * 8f, 0f, 1f);
		UnityCharacterSwitchBar.gameObject.SetActive(CharacterSwitchBarTransition > 0f);
		if (!(CharacterSwitchBarTransition > 0f))
		{
			return;
		}
		Session instance = Session.Instance;
		Community community = instance.CommunityManager.PlayerCommunity;
		if (instance.Editor && Hud.Instance.EditorSelectedObject != null)
		{
			community = Hud.Instance.EditorSelectedObject.GetCommunity();
		}
		CommunityPage.BuildSortedListOfCommunityMembers(community);
		List<CommunityPage.CharacterScore> temp = CommunityPage.Temp;
		bool flag = false;
		UnityCharacterSwitchBar.CharacterIcons.Clear();
		int i = 0;
		for (int j = 0; j < temp.Count; j++)
		{
			Character character = temp[j].Character;
			if (character.AliveAndNotZombie)
			{
				CharacterIconBehaviour component;
				if (i < UnityCharacterSwitchBarContents.transform.childCount)
				{
					component = UnityCharacterSwitchBarContents.transform.GetChild(i).GetComponent<CharacterIconBehaviour>();
					component.SetCharacter(character);
				}
				else
				{
					component = UnityEngine.Object.Instantiate(InfoScreen.CharacterIcon.GetAsset(), UnityCharacterSwitchBarContents.transform, worldPositionStays: false).GetComponent<CharacterIconBehaviour>();
					component.IsHudIcon = true;
					LayoutElement layoutElement = component.gameObject.AddComponent<LayoutElement>();
					float num = (layoutElement.flexibleHeight = 132f);
					float num3 = (layoutElement.flexibleWidth = num);
					float minWidth = (layoutElement.minHeight = num3);
					layoutElement.minWidth = minWidth;
					component.Initialize(character, checkOnScreen: true);
					flag = true;
				}
				component.ForceSelected = character == Hud.Instance.LocalControlledCharacter;
				UnityCharacterSwitchBar.CharacterIcons.Add(component);
				if (character == Hud.Instance.LocalControlledCharacter)
				{
					UnityCharacterSwitchBar.SelectedCharacterIndex = i;
				}
				i++;
			}
		}
		for (; i < UnityCharacterSwitchBarContents.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(UnityCharacterSwitchBarContents.transform.GetChild(i).gameObject);
		}
		RectTransform rectTransform = (RectTransform)UnityCharacterSwitchBar.transform;
		rectTransform.anchoredPosition = new Vector2(MainPanelRectTransform.sizeDelta.x * 0.5f, Mathf.Lerp(-128f, 108f, CharacterSwitchBarTransition));
		RectTransform rectTransform2 = (RectTransform)UnityCharacterSwitchBarContents.transform;
		UnityCharacterSwitchBarLeftArrow.SetActive(rectTransform2.rect.width > rectTransform.rect.width && rectTransform2.localPosition.x < 0f);
		UnityCharacterSwitchBarRightArrow.SetActive(rectTransform2.rect.width > rectTransform.rect.width && rectTransform2.localPosition.x > rectTransform.rect.width - rectTransform2.rect.width);
		if (flag)
		{
			UnityCharacterSwitchBar.Update();
		}
	}

	public void UpdateSkillsChangePopupPanel()
	{
		Session instance = Session.Instance;
		bool flag = instance.SkillsChangePopups.Count > 0 && instance.WantFinish == WantFinishState.None;
		float num = Mathf.Clamp01(SkillsChangePopupTransition + (flag ? 1f : (-1f)) * GameImpl.UnscaledDeltaTime / SkillsPopupTransitionTime);
		if (num > 0f && SkillsChangePopupTransition <= 0f)
		{
			UnitySkillsChangePopupPanel.gameObject.SetActive(value: true);
			UnitySkillsChangePopupPanel.Initialize();
		}
		else if (num <= 0f && SkillsChangePopupTransition > 0f)
		{
			UnitySkillsChangePopupPanel.gameObject.SetActive(value: false);
		}
		SkillsChangePopupTransition = num;
		if (num > 0f)
		{
			UnitySkillsChangePopupPanel.transform.localScale = new Vector3(num, num, 1f);
		}
	}

	public void KillSkillsChangePopupPanel()
	{
		UnitySkillsChangePopupPanel.gameObject.SetActive(value: false);
		SkillsChangePopupTransition = 0f;
	}
}
