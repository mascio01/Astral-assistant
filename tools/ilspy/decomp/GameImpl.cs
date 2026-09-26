using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class GameImpl
{
	public struct LoadSessionInfo
	{
		public enum Type
		{
			Invalid,
			NewGame,
			SavedGame,
			LatestSavedGame,
			NetworkSession,
			TextDump
		}

		public Type LoadType;

		public bool ForceProcedurallyGenerated;

		public bool IsGenerating;

		public int WantLoadGameUniqueId;

		public CharacterCreationSettings CharacterSettings;

		public SaveGame SaveGame;

		public byte[] Bytes;

		public int Len;

		public string LoadingPath;

		public CustomBinaryReader CurrentSessionReader;

		public CustomBinaryReader CurrentTerrainReader;

		public bool SessionReaderFinished;

		public bool TerrainReaderFinished;

		public bool Success;

		public string ErrorMessage;
	}

	private enum DownloadingStoriesState
	{
		Ready,
		NeedsDownload,
		Error
	}

	public class SortRecipesBySkillAscending : IComparer<Recipe>
	{
		int IComparer<Recipe>.Compare(Recipe a, Recipe b)
		{
			if (a.SkillLevel < b.SkillLevel)
			{
				return -1;
			}
			if (a.SkillLevel > b.SkillLevel)
			{
				return 1;
			}
			return string.CompareOrdinal(a.UniqueID, b.UniqueID);
		}
	}

	public static uint AppId = 1054510u;

	public static int ReleaseVersion = 286;

	public static string ReleaseVersionAsString = ReleaseVersion.ToString();

	public static GameImpl Instance;

	private GameState State;

	private BaseMenu Menu;

	private List<BaseDialog> DialogStack = new List<BaseDialog>();

	private Session Session;

	public InputFunctionManager InputFunctionManager = new InputFunctionManager();

	public OnlineParty OnlineParty = new OnlineParty();

	public WorkshopManager WorkshopManager = new WorkshopManager();

	public AchievementsManager AchievementsManager = new AchievementsManager();

	public SpecialEffectManager SpecialEffectManager = new SpecialEffectManager();

	public MusicManager MusicManager = new MusicManager();

	public AnimationManager AnimManager = new AnimationManager();

	public SaveGameManager SaveGameManager = new SaveGameManager();

	public NotificationManager NotificationManager = new NotificationManager();

	public PortraitGallery PortraitGallery = new PortraitGallery();

	public GrassRenderer MainGrassRenderer = new GrassRenderer();

	public GrassRenderer PipGrassRenderer = new GrassRenderer();

	public Sun Sun = new Sun();

	public WeatherParticleSystem WeatherParticleSystem = new WeatherParticleSystem();

	public IconGenerator IconGenerator = new IconGenerator();

	public bool SteamInitialized;

	public Language DefaultLanguage;

	public Translation UIEnglishTranslation;

	public Translation UIForeignTranslation;

	public int LastChangedLanguageFrame;

	public Story BaseStory;

	public Story CurrentStory;

	public List<Story> CurrentStories = new List<Story>();

	public int ScreenWidth;

	public int ScreenHeight;

	public float ScreenScale = 1f;

	public int WidthOnScreen;

	public int HeightOnScreen;

	public int LeftOnScreen;

	public int TopOnScreen;

	public static bool SuperSample = false;

	public static bool TAA = false;

	public GameObject UnityLauncherObj;

	public GameObject UnityMenuMaskObj;

	public GameObject UnityFaderPanelObj;

	public GameObject UnityCursorObj;

	public RectTransform UnityCursorRectTransform;

	public RawImage UnityCursorRawImage;

	public RawImage UnityCursorForbidden;

	public Animator UnityCursorAnimator;

	public GameObject UnityHudPanelObj;

	public HudBehaviour UnityHudBehaviour;

	public GameObject UnityUICameraObj;

	public GameObject UnityPortraitCameraObj;

	public GameObject UnityIconCameraObj;

	public GameObject UnityCharacterCreationCameraObj;

	public GameObject UnityScreenshotCameraObj;

	public GameObject UnityScreenshotCanvasObj;

	public RenderTexture UnityScreenshotRenderTexture;

	public GameObject UnityReflectionCameraObj;

	public GameObject UnityRefractionCameraObj;

	public RenderTexture UnityReflectionRenderTexture;

	public RenderTexture UnityRefractionRenderTexture;

	public GameObject UnityPortraitGeneratorCameraObj;

	public GameObject UnityPortraitGeneratorCanvasObj;

	public AudioSource UnityRainSound;

	public AudioSource UnityCricketsSound;

	public AudioSource UnityRiverSound;

	public AudioSource UnityWaterfallSound;

	public AudioSource UnityWindInLeavesSound;

	public AudioSource UnityWindInGrassSound;

	public AudioSource UnityParryPromptSound;

	public int NumInputFramesToAdd;

	public int NumTimesUpdateWasCalledBetweenFixedUpdates;

	public InputFrame HandleInputFrame = new InputFrame();

	public InputFrame PrevHandleInputFrame = new InputFrame();

	public int FixedFrameCount;

	public const int FixedFramesPerSecond = 60;

	public const float FixedFramesPerSecondf = 60f;

	public const float FixedFrameTime = 1f / 60f;

	public static TimeSpan FixedFrameTimeSpan = TimeSpan.FromSeconds(0.01666666753590107);

	public SettingsData Settings = new SettingsData();

	private Thread LoadingThread;

	private Thread LoadingSessionThread;

	public LoadSessionInfo LoadingSessionInfo;

	private DownloadingStoriesState DownloadingStories;

	public MyThreadPool UpdateThreadPool = new MyThreadPool();

	public readonly Thread MainThread;

	public readonly string StreamingAssetsPath;

	public string SaveGamePath;

	private Callback<FloatingGamepadTextInputDismissed_t> FloatingGamepadTextInputDismissedCallback;

	private SteamAPIWarningMessageHook_t SteamAPIWarningMessageHook;

	public static bool IsShowingOSK;

	public static int FinishedShowingOSKCountdown;

	public static bool Unloading;

	public Dictionary<string, EquipmentPrototype> CurrentEquipmentPrototypes;

	public Dictionary<string, LiquidPrototype> CurrentLiquidPrototypes;

	public Dictionary<int, PropPrototype> CurrentPropPrototypes;

	public Dictionary<string, MemoryPrototype> CurrentMemoryPrototypes;

	public Dictionary<string, Recipe> CurrentRecipes;

	public SortedDictionary<string, EquipmentPrototype> CurrentEquipmentPrototypesDeterministic;

	public SortedDictionary<string, LiquidPrototype> CurrentLiquidPrototypesDeterministic;

	public SortedDictionary<int, PropPrototype> CurrentPropPrototypesDeterministic;

	public SortedDictionary<string, MemoryPrototype> CurrentMemoryPrototypesDeterministic;

	public SortedDictionary<string, Recipe> CurrentRecipesDeterministic;

	public List<Recipe> CurrentRecipesSortedBySkill;

	public List<Recipe> OneItemRecipesForBandages;

	public List<EquipmentPrototype> EquipmentPrototypesThatCanBeAddedToFire = new List<EquipmentPrototype>();

	public List<EquipmentPrototype> AllIngredientsForRecipesWhichCanBeRecurring;

	public List<LiquidPrototype> AllLiquidIngredientsForRecipesWhichCanBeRecurring;

	public List<Speech>[] SpeechesForSituation = new List<Speech>[172];

	public Resource<Texture2D> TraderSquadMapIcon;

	public Resource<Texture2D> ExtortionSquadMapIcon;

	private static SortRecipesBySkillAscending RecipeSorter = new SortRecipesBySkillAscending();

	public static float DefaultReferenceWidth = 1920f;

	public static float DefaultReferenceHeight = 1080f;

	private static GameProfiler OnlineUpdateTimer = new GameProfiler("Update.Online");

	private static GameProfiler ActionMenuUpdateTimer = new GameProfiler("Update.ActionMenu");

	private static string HandleInputStr = "HandleInput";

	private static string InputFunctionManagerUpdateStr = "InputFunctionManagerUpdate";

	private static string FinishHandleInputOnThreadStr = "FinishHandleInputOnThread";

	private int FrameCounter;

	private float TimeCounter;

	private float LastAvgFrameTime;

	private TaskFunc HandleInputTaskFunc;

	public ManualResetEvent HandleInputFinishedEvent = new ManualResetEvent(initialState: true);

	public Thread HandleInputThread;

	public SaveGameType RequestedAutoSaveFromThread = SaveGameType.Invalid;

	public static float DeltaTime;

	public static float UnscaledDeltaTime;

	public static float UnscaledTime;

	public static float RealTimeSinceStartup;

	public static int FrameCount;

	private Vector2 MouseOriginalScreenPos;

	private List<InputAction> mergeableActions = new List<InputAction>();

	private bool GUIHasControl;

	private static StringBuilder FPSString = new StringBuilder(100);

	private static string ms = "ms ";

	private static string fps = " fps ";

	private static string Frame = "Frame: ";

	private static string Avg = "Average: ";

	public GUISkin OverrideGUISkin;

	public GUISkin OriginalGUISkin;

	private static int HUD_VehicleGear = StringUtil.JenkinsHash("HUD_VehicleGear");

	private static int HUD_VehicleRPM = StringUtil.JenkinsHash("HUD_VehicleRPM");

	private static int HUD_VehicleSpeed = StringUtil.JenkinsHash("HUD_VehicleSpeed");

	private static int HUD_Kmh = StringUtil.JenkinsHash("HUD_Kmh");

	private static int HUD_Mph = StringUtil.JenkinsHash("HUD_Mph");

	public CursorLockMode LastCursorLockMode;

	public static float CursorXFracWhenPlacingBuilding = 0.65f;

	private MessageBox UnityMessageBox;

	private ConfirmationBox UnityConfirmationBox;

	private HitTheRoadDialog UnityHitTheRoadDialog;

	private YesNoCancelBox UnityYesNoCancelBox;

	private InputBox UnityInputBox;

	private InputBox UnityInputBoxMultiline;

	private DifficultyDialog UnityDifficultyDialog;

	private AllowJoinDialog UnityAllowJoinDialog;

	private EquipmentTransferAmountBox UnityEquipmentTransferAmountBox;

	private CraftAmountBox UnityCraftAmountBox;

	private CraftLimitBox UnityCraftLimitBox;

	private GatePolicyDialog UnityGatePolicyDialog;

	private StoragePolicyDialog UnityStoragePolicyDialog;

	private DesignatedLiquidDialog UnityDesignatedLiquidDialog;

	private EquipmentPolicyDialog UnityEquipmentPolicyDialog;

	private List<BaseDialog> QueuedDialogs = new List<BaseDialog>();

	private const string EmptyStr = "";

	public static string Censored = "****";

	private TMP_FontAsset KomikaAxis = Resources.Load<TMP_FontAsset>("Fonts & Materials/KomikaTitle-Axis SDF");

	private TMP_FontAsset KomikaKaps = Resources.Load<TMP_FontAsset>("Fonts & Materials/KOMTXTKB SDF");

	private TMP_FontAsset Komika = Resources.Load<TMP_FontAsset>("Fonts & Materials/KOMTXTB_ SDF");

	private TMP_FontAsset LuckiestGuy = Resources.Load<TMP_FontAsset>("Fonts & Materials/luckiestguy SDF");

	private TMP_FontAsset PoetsenOne = Resources.Load<TMP_FontAsset>("Fonts & Materials/PoetsenOne-Regular SDF");

	private TMP_FontAsset Mitr = Resources.Load<TMP_FontAsset>("Fonts & Materials/Mitr-SemiBold SDF");

	private static int[] ProbabilityRatio = new int[6] { 0, 1, 9, 90, 300, 500 };

	private static int[] ProbabilityRatioTraderLevel1 = new int[6] { 0, 1, 4, 25, 30, 40 };

	private static int[] ProbabilityRatioTraderLevel2 = new int[6] { 0, 4, 16, 25, 25, 30 };

	private static int[] ProbabilityRatioTraderLevel3 = new int[6] { 0, 10, 20, 30, 20, 20 };

	private static int[] ProbabilityRatioTraderLevel4 = new int[6] { 0, 20, 30, 25, 15, 10 };

	private static int[] ProbabilityRatioTraderLevel5 = new int[6] { 0, 40, 30, 20, 10, 0 };

	private static int[] TotalProbabilityRatio = new int[7];

	private static int[] TotalProbabilityRatioTraderLevel1 = new int[7];

	private static int[] TotalProbabilityRatioTraderLevel2 = new int[7];

	private static int[] TotalProbabilityRatioTraderLevel3 = new int[7];

	private static int[] TotalProbabilityRatioTraderLevel4 = new int[7];

	private static int[] TotalProbabilityRatioTraderLevel5 = new int[7];

	private static List<EquipmentPrototype>[] ScarcityBuckets = new List<EquipmentPrototype>[6];

	private int SessionSize;

	private int TerrainSize;

	public Vector2Int MainPanelSizeWithoutSuperSample => new Vector2Int(UnityHudBehaviour.MainPanelWidthOnScreen, HeightOnScreen);

	public GameImpl()
	{
		Instance = this;
		MainThread = Thread.CurrentThread;
		StreamingAssetsPath = Application.streamingAssetsPath;
		SaveGamePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/SavedGames/Survivalist2";
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		CSteamID steamID = default(CSteamID);
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			Debug.Log("Arg " + i + ": " + commandLineArgs[i]);
			if (commandLineArgs[i].Contains("connect_lobby") && i + 1 < commandLineArgs.Length)
			{
				ulong result = 0uL;
				if (ulong.TryParse(commandLineArgs[i + 1], out result))
				{
					steamID = new CSteamID(result);
				}
			}
			if (commandLineArgs[i].Contains("oos_debug"))
			{
				Session.OutOfSyncDebugEnabled = true;
			}
			if (commandLineArgs[i].Contains("no_fog_of_war"))
			{
				FogOfWar.NoFogOfWar = true;
			}
			if (commandLineArgs[i].Contains("ignore_gamepads"))
			{
				InputFunctionManager.IgnoreGamepads = true;
			}
			if (commandLineArgs[i].Contains("party_size_limit") && i + 1 < commandLineArgs.Length)
			{
				OnlineParty.PartySizeLimit = StringUtil.ParseInt(commandLineArgs[i + 1]);
			}
			if (commandLineArgs[i].Contains("savedir") && i + 1 < commandLineArgs.Length)
			{
				SaveGamePath = commandLineArgs[i + 1];
			}
		}
		OnlineParty.Instance.RequestedLobbyIDToJoin = new LobbyID(steamID);
		for (int j = 0; j < ScarcityBuckets.Length; j++)
		{
			ScarcityBuckets[j] = new List<EquipmentPrototype>();
		}
		for (int num = 5; num >= 0; num--)
		{
			TotalProbabilityRatio[num] = TotalProbabilityRatio[num + 1] + ProbabilityRatio[num];
			TotalProbabilityRatioTraderLevel1[num] = TotalProbabilityRatioTraderLevel1[num + 1] + ProbabilityRatioTraderLevel1[num];
			TotalProbabilityRatioTraderLevel2[num] = TotalProbabilityRatioTraderLevel2[num + 1] + ProbabilityRatioTraderLevel2[num];
			TotalProbabilityRatioTraderLevel3[num] = TotalProbabilityRatioTraderLevel3[num + 1] + ProbabilityRatioTraderLevel3[num];
			TotalProbabilityRatioTraderLevel4[num] = TotalProbabilityRatioTraderLevel4[num + 1] + ProbabilityRatioTraderLevel4[num];
			TotalProbabilityRatioTraderLevel5[num] = TotalProbabilityRatioTraderLevel5[num + 1] + ProbabilityRatioTraderLevel5[num];
		}
		GC.AddMemoryPressure(1073741824L);
		HandleInputTaskFunc = HandleInputOnThread;
	}

	private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
	{
		Debug.LogError(pchDebugText);
	}

	private void OnFloatingGamepadTextInputDismissed(FloatingGamepadTextInputDismissed_t callback)
	{
		IsShowingOSK = false;
		FinishedShowingOSKCountdown = 10;
	}

	public void Start(GameObject unityLauncherObj)
	{
		CultureInfo currentUICulture = CultureInfo.CurrentUICulture;
		Language languageFromCode = LanguageMenu.GetLanguageFromCode(currentUICulture.Name);
		DefaultLanguage = (LanguageMenu.IsTranslationComplete(languageFromCode) ? languageFromCode : Language.English);
		Settings.Language = DefaultLanguage;
		if (!string.IsNullOrEmpty(currentUICulture.NumberFormat.NumberDecimalSeparator) && !string.IsNullOrEmpty(currentUICulture.NumberFormat.NumberGroupSeparator))
		{
			StringUtil._digits[19] = currentUICulture.NumberFormat.NumberDecimalSeparator[0];
			StringUtil._digits[20] = currentUICulture.NumberFormat.NumberGroupSeparator[0];
		}
		SteamInitialized = SteamAPI.Init();
		Debug.Log("SteamInitialized: " + SteamInitialized);
		if (SteamInitialized)
		{
			SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
			SteamClient.SetWarningMessageHook(SteamAPIWarningMessageHook);
			FloatingGamepadTextInputDismissedCallback = Callback<FloatingGamepadTextInputDismissed_t>.Create(OnFloatingGamepadTextInputDismissed);
			Language languageFromSteamCode = LanguageMenu.GetLanguageFromSteamCode(SteamApps.GetCurrentGameLanguage());
			DefaultLanguage = (LanguageMenu.IsTranslationComplete(languageFromSteamCode) ? languageFromSteamCode : Language.English);
			Settings.Language = DefaultLanguage;
		}
		LoadSettingsAndInit();
		SuperSample = PlayerPrefs.GetInt("SuperSample", SuperSample ? 1 : 0) != 0;
		TAA = PlayerPrefs.GetInt("TAA", TAA ? 1 : 0) != 0;
		Settings.HighQualityEffects = PlayerPrefs.GetInt("HighQualityEffects", Settings.HighQualityEffects ? 1 : 0) != 0;
		Settings.GrassDensity = PlayerPrefs.GetFloat("GrassDensity", Settings.GrassDensity);
		Settings.TreeQuality = PlayerPrefs.GetFloat("TreeQuality", Settings.TreeQuality);
		QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSyncCount", QualitySettings.vSyncCount);
		StoryManager.EvaluateSpeechOptionsOnThreadEnabled = PlayerPrefs.GetInt("MultithreadedSpeechOptions", 1) != 0;
		OnlineParty.NetworkLoggingEnabled = PlayerPrefs.GetInt("NetworkLoggingEnabled", 0) != 0;
		InputFunctionManager.JoystickDeadZone = PlayerPrefs.GetFloat("JoystickDeadZone", InputFunctionManager.JoystickDeadZone);
		InputFunctionManager.TriggerDeadZone = PlayerPrefs.GetFloat("TriggerDeadZone", InputFunctionManager.TriggerDeadZone);
		Character.VoiceSoundVolume = PlayerPrefs.GetFloat("VoiceSoundVolume", Character.VoiceSoundVolume);
		UnityLauncherObj = unityLauncherObj;
		UnityFaderPanelObj = UnityLauncherObj.transform.Find("FaderPanel").gameObject;
		UnityMenuMaskObj = UnityLauncherObj.transform.Find("MenuMask").gameObject;
		UnityMenuMaskObj.GetComponent<Mask>().enabled = true;
		UnityCursorObj = UnityLauncherObj.transform.Find("Cursor").gameObject;
		UnityCursorRectTransform = UnityCursorObj.GetComponent<RectTransform>();
		UnityCursorRawImage = UnityCursorObj.GetComponent<RawImage>();
		UnityCursorAnimator = UnityCursorObj.GetComponent<Animator>();
		UnityCursorForbidden = UnityLauncherObj.transform.Find("Cursor/Forbidden").GetComponent<RawImage>();
		UnityHudPanelObj = UnityLauncherObj.transform.Find("HudPanel").gameObject;
		UnityHudBehaviour = UnityHudPanelObj.GetComponent<HudBehaviour>();
		UnityUICameraObj = GameObject.Find("UICamera");
		UnityUICameraObj.AddComponent<AudioListener>();
		UnityPortraitCameraObj = GameObject.Find("PortraitCamera");
		UnityPortraitCameraObj.SetActive(value: false);
		UnityIconCameraObj = GameObject.Find("IconCamera");
		UnityIconCameraObj.SetActive(value: false);
		UnityCharacterCreationCameraObj = GameObject.Find("CharacterCreationCamera");
		UnityCharacterCreationCameraObj.SetActive(value: false);
		UnityScreenshotCameraObj = GameObject.Find("ScreenshotCamera");
		UnityScreenshotCanvasObj = GameObject.Find("ScreenshotCanvas");
		UnityScreenshotRenderTexture = new RenderTexture(SaveGameManager.ThumbnailWidth, SaveGameManager.ThumbnailHeight, 24);
		UnityScreenshotCameraObj.GetComponent<Camera>().targetTexture = UnityScreenshotRenderTexture;
		UnityScreenshotCameraObj.SetActive(value: false);
		UnityPortraitGeneratorCameraObj = GameObject.Find("PortraitGeneratorCamera");
		UnityPortraitGeneratorCanvasObj = GameObject.Find("PortraitGeneratorCanvas");
		UnityPortraitGeneratorCameraObj.SetActive(value: false);
		UnityPortraitGeneratorCanvasObj.SetActive(value: false);
		UnityReflectionCameraObj = GameObject.Find("ReflectionCamera");
		UnityRefractionCameraObj = GameObject.Find("RefractionCamera");
		UnityReflectionRenderTexture = new RenderTexture(256, 256, 16);
		UnityReflectionRenderTexture.isPowerOfTwo = true;
		UnityRefractionRenderTexture = new RenderTexture(256, 256, 16);
		UnityRefractionRenderTexture.isPowerOfTwo = true;
		UnityReflectionCameraObj.GetComponent<Camera>().targetTexture = UnityReflectionRenderTexture;
		UnityRefractionCameraObj.GetComponent<Camera>().targetTexture = UnityRefractionRenderTexture;
		UnityRainSound = GameObject.Find("RainSound").GetComponent<AudioSource>();
		UnityCricketsSound = GameObject.Find("CricketsSound").GetComponent<AudioSource>();
		UnityRiverSound = GameObject.Find("RiverSound").GetComponent<AudioSource>();
		UnityWaterfallSound = GameObject.Find("WaterfallSound").GetComponent<AudioSource>();
		UnityWindInLeavesSound = GameObject.Find("WindInLeavesSound").GetComponent<AudioSource>();
		UnityWindInGrassSound = GameObject.Find("WindInGrassSound").GetComponent<AudioSource>();
		UnityParryPromptSound = GameObject.Find("ParryPromptSound").GetComponent<AudioSource>();
		InitDialogs();
		OnScreenResized();
		GameCursor.CursorPointer = Resources.Load<Texture2D>("Textures/HUD/Cursor");
		BaseStory = new Story(StorySource.FromFolder("BaseStory"));
		CurrentStories.Add(BaseStory);
		Story item = new Story(StorySource.FromFolder("Common"));
		CurrentStories.Add(item);
		Character.InitLayerMasks();
		OnlineParty.Init();
		WorkshopManager.Init();
		SpecialEffectManager.Init();
		AnimManager.Init();
		SaveGameManager.Init();
		NotificationManager.Init();
		PortraitGallery.Init();
		Sun.Init();
		WeatherParticleSystem.Init();
		IconGenerator.Init();
		SetState(GameState.Loading);
	}

	private void LoadSettingsAndInit()
	{
		SaveGameManager.LoadSettings();
		SaveGameManager.EnumerateSavedCharacters();
		OnlineParty.LoadTerrainCacheLastUsedTimes();
		if (Settings.Language == Language.Invalid)
		{
			Settings.Language = DefaultLanguage;
		}
		OnLanguageChanged();
		UIEnglishTranslation = Translation.LoadFromFile(StreamingAssetsPath + "/UI/English.tsv", justTitleAndDescription: false);
		if (Settings.Language != Language.English)
		{
			UIForeignTranslation = Translation.LoadFromFile(StreamingAssetsPath + "/UI/" + Settings.Language.ToString() + ".tsv", justTitleAndDescription: false);
		}
		AchievementsManager.Init();
	}

	public void LoadContent()
	{
		BaseObjectManager.SetupPrototypeGameObjects();
		OnlineParty.Load();
		Hud.LoadContent();
		GameCursor.LoadContent();
		GameTerrain.LoadContent();
		FogOfWar.LoadContent();
		Equipment.LoadContent();
		SoundManager.LoadContent();
		BirdSongManager.LoadContent();
		SpecialEffectManager.LoadContent();
		Human.LoadHumanContent();
		Rabbit.LoadRabbitContent();
		Deer.LoadDeerContent();
		Chicken.LoadChickenContent();
		Character.LoadContent();
		Prop.LoadContent();
		AnimManager.LoadContent();
		BaseMenu.LoadContent();
		InfoScreen.LoadContent();
		GrassRenderer.LoadContentStatic();
		MainGrassRenderer.LoadContent();
		PipGrassRenderer.LoadContent();
		OutlineBehaviour.LoadContent();
		OutlineCameraBehaviour.LoadContent();
		WeatherParticleSystem.LoadContent();
		MusicManager.Init();
		LoadingThread = new Thread(LoadThreaded);
		LoadingThread.Start();
	}

	public void OnAllContentLoaded()
	{
		using (new StopWatchMarker("OnAllContentLoaded"))
		{
			AnimManager.OnAllContentLoaded();
			WeatherParticleSystem.OnAllContentLoaded();
			Human.OnAllContentLoaded();
			InfoScreen.OnAllContentLoaded();
			HudBehaviour.Instance.OnScreenResized();
		}
	}

	private void LoadThreaded()
	{
		Util.SetFloatingPointControl();
		Util.CheckFloatingPointControl();
		try
		{
			foreach (Story currentStory in CurrentStories)
			{
				currentStory.LoadStoryContent();
			}
			BuildPrototypeLookupLists();
		}
		catch (Exception ex)
		{
			Debug.Log("Error loading BaseStory on startup: " + ex.ToString());
			LoadingSessionInfo.ErrorMessage = "Error loading BaseStory on startup: " + ex.Message;
		}
		Profiler.EndThreadProfiling();
	}

	private void LoadFixedMap(string mapFileName)
	{
		using Stream stream = File.OpenRead(mapFileName);
		using CustomBinaryReader customBinaryReader = new CustomBinaryReader(stream);
		SetCurrentSessionReader(customBinaryReader);
		Session.Load(customBinaryReader, newGame: true, sandbox: false, LoadingSessionInfo.CharacterSettings, null);
		SetCurrentSessionReader(null);
	}

	private void LoadSessionThreaded()
	{
		Util.SetFloatingPointControl();
		Util.CheckFloatingPointControl();
		try
		{
			if (LoadingSessionInfo.LoadType == LoadSessionInfo.Type.LatestSavedGame)
			{
				SaveGame saveGame = null;
				int num = 0;
				string[] saveGameFolders = SaveGameManager.GetSaveGameFolders();
				foreach (string obj in saveGameFolders)
				{
					string path = obj + "/info.xml";
					string fileName = Path.GetFileName(obj);
					SaveGameType saveGameType = SaveGameType.Invalid;
					SaveGameManager.ParseSaveGameDirName(fileName, out saveGameType, out var _, out var gameUniqueId);
					if (LoadingSessionInfo.WantLoadGameUniqueId != 0 && gameUniqueId != LoadingSessionInfo.WantLoadGameUniqueId && num != 0 && num == LoadingSessionInfo.WantLoadGameUniqueId)
					{
						continue;
					}
					using Stream stream = SaveGameManager.OpenSaveGameFile(path);
					if (stream != null)
					{
						SaveGameData data = (SaveGameData)new XmlSerializer(typeof(SaveGameData)).Deserialize(stream);
						if (saveGame == null || data.Timestamp > saveGame.Data.Timestamp || (LoadingSessionInfo.WantLoadGameUniqueId != 0 && gameUniqueId == LoadingSessionInfo.WantLoadGameUniqueId && num != LoadingSessionInfo.WantLoadGameUniqueId))
						{
							saveGame = new SaveGame();
							saveGame.DirName = fileName;
							saveGame.Data = data;
							saveGame.Type = saveGameType;
							num = gameUniqueId;
						}
					}
				}
				lock (this)
				{
					LoadingSessionInfo.LoadType = LoadSessionInfo.Type.SavedGame;
					LoadingSessionInfo.SaveGame = saveGame;
				}
				if (saveGame == null)
				{
					throw new Exception("Most recent savegame not found!");
				}
				Session.StorySources = saveGame.GetStorySources();
				StartDownloadingStories();
			}
			while (DownloadingStories == DownloadingStoriesState.NeedsDownload)
			{
				Thread.Sleep(10);
			}
			if (Session.StorySources.Count == 0)
			{
				throw new Exception(Translate("MENU_InvalidStory"));
			}
			int num2 = -1;
			for (int j = 0; j < Session.StorySources.Count; j++)
			{
				if (!Session.StorySources[j].GetCachedIsMod())
				{
					num2 = j;
				}
			}
			if (num2 != -1)
			{
				List<StorySource> recursionCheck = new List<StorySource>();
				List<StorySource> list = new List<StorySource>();
				string error = string.Empty;
				StorySource storySource = Session.StorySources[num2];
				if (SelectStoryMenu.AddStoryDependenciesRecursive(storySource, null, list, recursionCheck, ref error, storySource))
				{
					bool flag = false;
					if (num2 + 1 == list.Count)
					{
						for (int k = 0; k <= num2; k++)
						{
							if (!Session.StorySources[k].Equals(list[k]))
							{
								flag = true;
								break;
							}
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						string storySourcesDebugString = StorySource.GetStorySourcesDebugString(Session.StorySources);
						Session.StorySources.RemoveRange(0, num2 + 1);
						Session.StorySources.InsertRange(0, list);
						string storySourcesDebugString2 = StorySource.GetStorySourcesDebugString(Session.StorySources);
						Debug.Log("Story dependencies have changed from [" + storySourcesDebugString + "] to [" + storySourcesDebugString2 + "]");
						StartDownloadingStories();
						while (DownloadingStories == DownloadingStoriesState.NeedsDownload)
						{
							Thread.Sleep(10);
						}
					}
				}
			}
			foreach (StorySource storySource2 in Session.StorySources)
			{
				if (!storySource2.IsValid())
				{
					string text = storySource2.GetTranslatedName();
					if (string.IsNullOrEmpty(text))
					{
						text = storySource2.WorkshopId.ToString();
					}
					throw new Exception(Translate("MENU_FailedToLoadStoryFromWorkshop").Replace("%1", text));
				}
				if (!Directory.Exists(storySource2.AbsolutePath))
				{
					throw new Exception("Story folder not found: " + storySource2.AbsolutePath);
				}
			}
			SetCurrentStory(Session.StorySources);
			while (!BaseResource.AreAllResourcesLoaded())
			{
				Thread.Sleep(10);
			}
			switch (LoadingSessionInfo.LoadType)
			{
			case LoadSessionInfo.Type.NewGame:
			{
				if (CurrentStory.Settings.ProcedurallyGenerated || LoadingSessionInfo.ForceProcedurallyGenerated)
				{
					LoadingSessionInfo.IsGenerating = true;
					Session.Load(null, newGame: true, sandbox: true, LoadingSessionInfo.CharacterSettings, null);
				}
				else if (File.Exists(CurrentStory.Path + "/Terrain.mapx"))
				{
					LoadFixedMap(CurrentStory.Path + "/Terrain.mapx");
				}
				else if (File.Exists(CurrentStory.Path + "/Terrain.map"))
				{
					LoadFixedMap(CurrentStory.Path + "/Terrain.map");
				}
				else
				{
					Session.Load(null, newGame: true, sandbox: false, LoadingSessionInfo.CharacterSettings, null);
				}
				GameTerrain instance = GameTerrain.Instance;
				if (!GameTerrain.SaveFixedToTerrainCache(out Session.Instance.TerrainHash) && !instance.SaveToTerrainCache(out Session.Instance.TerrainHash))
				{
					MD5Hash mD5Hash = Session.Instance.TerrainHash;
					throw new Exception("Unable to save terrain to cache " + mD5Hash.ToString());
				}
				break;
			}
			case LoadSessionInfo.Type.SavedGame:
			{
				if (GameTerrain.SaveFixedToTerrainCache(out var hash2))
				{
					Session.TerrainHash = hash2;
				}
				else
				{
					using Stream stream3 = SaveGameManager.OpenSaveGameFile(SaveGamePath + "/" + LoadingSessionInfo.SaveGame.DirName + "/terrain.map");
					if (stream3 != null)
					{
						byte[] array = new byte[stream3.Length];
						stream3.Read(array, 0, (int)stream3.Length);
						if (SaveGameManager.IsSaveGameCompressed())
						{
							byte[] array2 = Compression.Decompress(array, 0, array.Length);
							hash2 = new MD5Hash(array2, array2.Length);
						}
						else
						{
							hash2 = new MD5Hash(array, array.Length);
						}
						if (!OnlineParty.SaveToTerrainCache(hash2, array, array.Length, SaveGameManager.IsSaveGameCompressed()))
						{
							MD5Hash mD5Hash = hash2;
							throw new Exception("Unable to save terrain to cache " + mD5Hash.ToString());
						}
						Session.TerrainHash = hash2;
					}
				}
				using (Stream stream4 = SaveGameManager.OpenSaveGameFile(SaveGamePath + "/" + LoadingSessionInfo.SaveGame.DirName + "/savegame.sav"))
				{
					if (stream4 == null)
					{
						break;
					}
					byte[] array3 = new byte[stream4.Length];
					stream4.Read(array3, 0, (int)stream4.Length);
					if (Session.IsInMultiplayerGame())
					{
						lock (OnlineParty)
						{
							OnlineParty.WantSendLoadingSessionBytes = array3;
							OnlineParty.WantSendLoadingTerrainHash = Session.TerrainHash;
							OnlineParty.WantSendLoadingSession = true;
						}
					}
					if (SaveGameManager.IsSaveGameCompressed())
					{
						array3 = Compression.Decompress(array3, 0, array3.Length);
					}
					using CustomBinaryReaderFromMemory customBinaryReaderFromMemory2 = new CustomBinaryReaderFromMemory(array3, array3.Length);
					SetCurrentSessionReader(customBinaryReaderFromMemory2);
					Session.Load(customBinaryReaderFromMemory2, newGame: false, sandbox: false, null, null);
					SetCurrentSessionReader(null);
				}
				break;
			}
			case LoadSessionInfo.Type.NetworkSession:
			{
				using (CustomBinaryReaderFromMemory customBinaryReaderFromMemory = new CustomBinaryReaderFromMemory(LoadingSessionInfo.Bytes, LoadingSessionInfo.Len))
				{
					Util.SetFloatingPointControl();
					Util.CheckFloatingPointControl();
					bool num3 = Session.TerrainHash.IsNull();
					SetCurrentSessionReader(customBinaryReaderFromMemory);
					Session.Load(customBinaryReaderFromMemory, newGame: false, sandbox: false, null, null);
					SetCurrentSessionReader(null);
					if (num3)
					{
						if (!GameTerrain.SaveFixedToTerrainCache(out var hash))
						{
							MD5Hash mD5Hash = hash;
							throw new Exception("Unable to save terrain to cache " + mD5Hash.ToString());
						}
						if (Session.TerrainHash != hash)
						{
							MD5Hash mD5Hash = hash;
							string text2 = mD5Hash.ToString();
							mD5Hash = Session.TerrainHash;
							Debug.LogWarning("Fixed Terrain saved to cache but hash " + text2 + " doesn't match the one in the savegame: " + mD5Hash.ToString());
						}
					}
					Util.CheckFloatingPointControl();
				}
				break;
			}
			case LoadSessionInfo.Type.TextDump:
			{
				using (Stream stream2 = File.OpenRead(LoadingSessionInfo.LoadingPath))
				{
					using CustomBinaryReader customBinaryReader = new CustomBinaryReader(stream2);
					Util.SetFloatingPointControl();
					Util.CheckFloatingPointControl();
					customBinaryReader.IsDoingNetworkChecksum = true;
					customBinaryReader.AltTerrainFolder = Path.GetDirectoryName(LoadingSessionInfo.LoadingPath);
					string textDumpPath = LoadingSessionInfo.LoadingPath.Replace(".sav", ".txt");
					SetCurrentSessionReader(customBinaryReader);
					Session.BaseObjectManager.BaseObjectsMightNotBeSorted = true;
					Session.Load(customBinaryReader, newGame: false, sandbox: false, null, textDumpPath);
					SetCurrentSessionReader(null);
					Util.CheckFloatingPointControl();
				}
				break;
			}
			}
			LoadingSessionInfo.Success = true;
		}
		catch (Exception ex)
		{
			Debug.Log("Error loading game: " + ex.ToString());
			LoadingSessionInfo.ErrorMessage = "Error loading game: " + ex.Message;
		}
		Profiler.EndThreadProfiling();
		Util.CheckFloatingPointControl();
	}

	public void Unload()
	{
		Unloading = true;
		SetState(GameState.Finished);
		UpdateThreadPool.Unload();
		UpdateThreadPool = null;
		WeatherParticleSystem.Unload();
		Sun.Unload();
		PortraitGallery.Unload();
		NotificationManager.Unload();
		SaveGameManager.Unload();
		AnimManager.Unload();
		SpecialEffectManager.Unload();
		AchievementsManager.Unload();
		WorkshopManager.Unload();
		OnlineParty.Unload();
		FogOfWar.UnloadContent();
		IconGenerator.Unload();
		UnityScreenshotRenderTexture.Release();
		if (SteamInitialized)
		{
			SteamAPI.Shutdown();
		}
	}

	public void SetCurrentStory(List<StorySource> newStorySources)
	{
		int i;
		for (i = 0; i < newStorySources.Count && i < CurrentStories.Count; i++)
		{
			StorySource other = ((i < CurrentStories.Count) ? CurrentStories[i].StorySource : new StorySource());
			if (!((i < newStorySources.Count) ? newStorySources[i] : new StorySource()).Equals(other))
			{
				break;
			}
		}
		if (newStorySources.Count == CurrentStories.Count && i == CurrentStories.Count)
		{
			return;
		}
		for (int num = CurrentStories.Count - 1; num >= i; num--)
		{
			if (SteamInitialized && CurrentStories[num].Settings != null && CurrentStories[num].Settings.SteamWorkshopId != 0L && !EditorMenu.IsReservedStoryName(CurrentStories[num].StorySource.Folder))
			{
				SteamUGC.StopPlaytimeTracking(new PublishedFileId_t[1] { (PublishedFileId_t)CurrentStories[num].Settings.SteamWorkshopId }, 1u);
			}
			CurrentStories[num].UnloadStoryContent();
			CurrentStories.RemoveAt(num);
		}
		for (int j = i; j < newStorySources.Count; j++)
		{
			Story story = new Story(newStorySources[j]);
			CurrentStories.Add(story);
			story.LoadStoryContent();
			string text = story.Path + "/FixedTerrain.mapx";
			try
			{
				if (!File.Exists(text))
				{
					text = story.Path + "/FixedTerrain.map";
				}
				if (File.Exists(text))
				{
					using Stream stream = File.OpenRead(text);
					TerrainSize = (int)stream.Length;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to read " + text + ": " + ex.ToString());
			}
			if (SteamInitialized && story.Settings.SteamWorkshopId != 0L && !EditorMenu.IsReservedStoryName(story.StorySource.Folder))
			{
				SteamUGC.StartPlaytimeTracking(new PublishedFileId_t[1] { (PublishedFileId_t)story.Settings.SteamWorkshopId }, 1u);
			}
		}
		for (int k = 0; k < CurrentStories.Count; k++)
		{
			if (k == 0)
			{
				BaseStory = CurrentStories[k];
			}
			if (!CurrentStories[k].Settings.IsMod)
			{
				CurrentStory = CurrentStories[k];
			}
		}
		BuildPrototypeLookupLists();
	}

	public void ReloadCurrentStory(bool reloadFromDisk)
	{
		foreach (Story currentStory in CurrentStories)
		{
			currentStory.ReloadContent(reloadFromDisk);
		}
		BuildPrototypeLookupLists();
		if (Session == null)
		{
			return;
		}
		foreach (BaseObject baseObject in BaseObjectManager.Instance.BaseObjects)
		{
			baseObject.OnStoryReloaded();
		}
	}

	private void SetPrototypeLookupsFromStory(Story story)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in story.EquipmentPrototypes)
		{
			CurrentEquipmentPrototypes[equipmentPrototype.Key] = equipmentPrototype.Value;
			CurrentEquipmentPrototypesDeterministic[equipmentPrototype.Key] = equipmentPrototype.Value;
		}
		foreach (KeyValuePair<string, LiquidPrototype> liquidPrototype in story.LiquidPrototypes)
		{
			CurrentLiquidPrototypes[liquidPrototype.Key] = liquidPrototype.Value;
			CurrentLiquidPrototypesDeterministic[liquidPrototype.Key] = liquidPrototype.Value;
		}
		foreach (KeyValuePair<string, PropPrototype> propPrototype in story.PropPrototypes)
		{
			CurrentPropPrototypes[propPrototype.Value.NameHash] = propPrototype.Value;
			CurrentPropPrototypesDeterministic[propPrototype.Value.NameHash] = propPrototype.Value;
		}
		foreach (KeyValuePair<string, MemoryPrototype> memoryPrototype in story.MemoryPrototypes)
		{
			CurrentMemoryPrototypes[memoryPrototype.Key] = memoryPrototype.Value;
			CurrentMemoryPrototypesDeterministic[memoryPrototype.Key] = memoryPrototype.Value;
		}
		foreach (KeyValuePair<string, Recipe> recipe in story.Recipes)
		{
			CurrentRecipes[recipe.Key] = recipe.Value;
		}
		foreach (KeyValuePair<string, Script> script in story.Scripts)
		{
			script.Value.FixupAfterXmlLoad(story);
		}
	}

	public void BuildPrototypeLookupLists()
	{
		CurrentEquipmentPrototypes = new Dictionary<string, EquipmentPrototype>();
		CurrentLiquidPrototypes = new Dictionary<string, LiquidPrototype>();
		CurrentPropPrototypes = new Dictionary<int, PropPrototype>();
		CurrentMemoryPrototypes = new Dictionary<string, MemoryPrototype>();
		CurrentRecipes = new Dictionary<string, Recipe>();
		CurrentEquipmentPrototypesDeterministic = new SortedDictionary<string, EquipmentPrototype>();
		CurrentLiquidPrototypesDeterministic = new SortedDictionary<string, LiquidPrototype>();
		CurrentPropPrototypesDeterministic = new SortedDictionary<int, PropPrototype>();
		CurrentMemoryPrototypesDeterministic = new SortedDictionary<string, MemoryPrototype>();
		CurrentRecipesDeterministic = new SortedDictionary<string, Recipe>();
		CurrentRecipesSortedBySkill = new List<Recipe>();
		OneItemRecipesForBandages = new List<Recipe>();
		AllIngredientsForRecipesWhichCanBeRecurring = new List<EquipmentPrototype>();
		AllLiquidIngredientsForRecipesWhichCanBeRecurring = new List<LiquidPrototype>();
		foreach (Story currentStory in CurrentStories)
		{
			SetPrototypeLookupsFromStory(currentStory);
		}
		foreach (Story currentStory2 in CurrentStories)
		{
			foreach (KeyValuePair<string, Recipe> recipe in currentStory2.Recipes)
			{
				CurrentRecipesDeterministic[recipe.Key] = recipe.Value;
			}
		}
		EquipmentPrototype.CacheEquipmentPrototypeRefs();
		PropPrototype.CachePropPrototypeRefs();
		LiquidPrototype.CacheLiquidPrototypeRefs();
		MemoryPrototype.CacheMemoryPrototypeRefs();
		foreach (KeyValuePair<string, Recipe> item in CurrentRecipesDeterministic)
		{
			Recipe value = item.Value;
			value.CacheStuff();
			if (value.Deprecated)
			{
				continue;
			}
			CurrentRecipesSortedBySkill.Add(value);
			if (value.ProductPrototype != null && value.ProductPrototype.BandageLevel > -1 && value.Ingredients.Count == 1)
			{
				OneItemRecipesForBandages.Add(value);
			}
			if (value.ProductPropPrototype == null)
			{
				foreach (Ingredient ingredient in value.Ingredients)
				{
					if (ingredient.Prototypes != null)
					{
						foreach (EquipmentPrototype prototype in ingredient.Prototypes)
						{
							if (!AllIngredientsForRecipesWhichCanBeRecurring.Contains(prototype))
							{
								AllIngredientsForRecipesWhichCanBeRecurring.Add(prototype);
							}
						}
					}
					if (ingredient.LiquidTypes == null)
					{
						continue;
					}
					foreach (LiquidPrototype liquidType in ingredient.LiquidTypes)
					{
						if (!AllLiquidIngredientsForRecipesWhichCanBeRecurring.Contains(liquidType))
						{
							AllLiquidIngredientsForRecipesWhichCanBeRecurring.Add(liquidType);
						}
					}
				}
			}
			if (value.ProductPropPrototype != PropPrototype.Campfire || value.Deprecated)
			{
				continue;
			}
			foreach (Ingredient ingredient2 in value.Ingredients)
			{
				if (ingredient2.Prototypes == null)
				{
					continue;
				}
				foreach (EquipmentPrototype prototype2 in ingredient2.Prototypes)
				{
					if (!EquipmentPrototypesThatCanBeAddedToFire.Contains(prototype2))
					{
						EquipmentPrototypesThatCanBeAddedToFire.Add(prototype2);
					}
				}
			}
		}
		CurrentRecipesSortedBySkill.Sort(RecipeSorter);
		foreach (KeyValuePair<int, PropPrototype> item2 in CurrentPropPrototypesDeterministic)
		{
			item2.Value.CacheStuff();
		}
		PropSpawner.SetupPropCategories();
		PropPrototype.CachePlantableCropTypes();
		for (int i = 0; i < 172; i++)
		{
			if (SpeechesForSituation[i] == null)
			{
				SpeechesForSituation[i] = new List<Speech>();
			}
			StoryManager.GetSpeechesToEvaluate((SpeechSituation)i, SpeechesForSituation[i]);
		}
		LoadInvaderIcon(ref TraderSquadMapIcon, "Traders.png");
		LoadInvaderIcon(ref ExtortionSquadMapIcon, "ExtortionSquad.png");
	}

	private void LoadInvaderIcon(ref Resource<Texture2D> iconRes, string fileName)
	{
		iconRes = null;
		int num = CurrentStories.Count - 1;
		while (num >= 0 && iconRes == null)
		{
			iconRes = Resource<Texture2D>.CreateIfFileExists(CurrentStories[num].Path + "/InvaderIcons/" + fileName);
			num--;
		}
	}

	public void SetState(GameState state)
	{
		switch (State)
		{
		case GameState.Loading:
			if (LoadingThread != null)
			{
				LoadingThread.Join();
				LoadingThread = null;
			}
			OnAllContentLoaded();
			break;
		case GameState.LoadingSession:
			if (LoadingSessionThread != null)
			{
				while (!LoadingSessionThread.Join(5))
				{
					BaseResource.UpdateResources();
					Story.LoadAssetBundlesOnMainThreadIfNeeded();
				}
				LoadingSessionThread = null;
			}
			if (state != GameState.UnityInitSession)
			{
				Session.Unload();
				Session = null;
				TakeOutTheTrash();
			}
			break;
		case GameState.UnityInitSession:
			if (state != GameState.RunningSession)
			{
				Session.StopUnityInit();
				Session.Finish();
				Session.Unload();
				Session = null;
				TakeOutTheTrash();
			}
			break;
		case GameState.RunningSession:
			Session.Finish();
			Session.Unload();
			Session = null;
			WeatherParticleSystem.OnFinishedSession();
			NotificationManager.OnFinishedSession();
			IconGenerator.OnFinishedSession();
			if (HasAnyDialogsThatNeedSession())
			{
				ClearDialogStack();
			}
			if (OnlineParty.CurrentState == OnlineParty.State.LeavingLobby)
			{
				OnlineParty.ClearLobby();
			}
			TakeOutTheTrash();
			break;
		}
		State = state;
		switch (State)
		{
		case GameState.Loading:
			SetMenu(GetMenuBehaviourByPanelName("LoadingPanel"));
			LoadContent();
			break;
		case GameState.TitleMenu:
			if (Menu == null || Menu.IsFadeFinished() || Menu is LoadingMenu)
			{
				SetMenu(GetMenuBehaviourByPanelName("TitleMenuPanel"));
			}
			break;
		case GameState.LoadingSession:
		case GameState.ReceivingNetworkSession:
		case GameState.SendingNetworkSession:
			GameTerrain.CurGenerationStep = 0;
			if (Menu == null || Menu.IsFadeFinished() || Menu is GameOverMenu || Menu is GameCompleteMenu || Menu is DemoTimeoutMenu)
			{
				SetMenu(GetMenuBehaviourByPanelName("LoadingPanel"));
			}
			break;
		case GameState.UnityInitSession:
			Session.StartUnityInit();
			AchievementsManager.OnLoadGame();
			break;
		case GameState.RunningSession:
			Session.Start();
			NumInputFramesToAdd = OnlineParty.GetHowManyInputFramesToAddThisFrame(Session);
			NumTimesUpdateWasCalledBetweenFixedUpdates = 0;
			HandleInputFrame.ClearActions((NumInputFramesToAdd <= 0) ? (-1) : 0);
			PrevHandleInputFrame.ClearActions(-1);
			if (Menu is LoadingMenu)
			{
				SetMenu(null);
			}
			break;
		}
	}

	public void OnScreenResized()
	{
		ScreenWidth = Screen.width;
		ScreenHeight = Screen.height;
		float x = DefaultReferenceWidth;
		float y = DefaultReferenceHeight;
		float num = (float)ScreenWidth / (float)ScreenHeight;
		if (num > DefaultReferenceWidth / DefaultReferenceHeight)
		{
			x = DefaultReferenceHeight * num;
		}
		else if (num < DefaultReferenceWidth / DefaultReferenceHeight)
		{
			y = DefaultReferenceWidth / num;
		}
		Vector2 vector = new Vector2(x, y);
		Launcher.Instance.UnityCanvasScaler.referenceResolution = vector;
		HudBehaviour.Instance.Size = vector;
		HudBehaviour.Instance.Centre = vector * 0.5f;
		float num2 = vector.x / vector.y;
		WidthOnScreen = ((num <= num2) ? ScreenWidth : ((int)((float)ScreenHeight * num2)));
		HeightOnScreen = ((num >= num2) ? ScreenHeight : ((int)((float)ScreenWidth / num2)));
		ScreenScale = (float)WidthOnScreen / vector.x;
		LeftOnScreen = (ScreenWidth - WidthOnScreen) / 2;
		TopOnScreen = (ScreenHeight - HeightOnScreen) / 2;
		HudBehaviour.Instance.OnScreenResized();
	}

	public Rect CalcRectInScreenPixels(Rect rect)
	{
		Vector2 vector = CalcScreenPosFromReferenceSpacePos(rect.min);
		Vector2 vector2 = CalcScreenPosFromReferenceSpacePos(rect.max);
		return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
	}

	public Vector2 CalcScreenPosFromReferenceSpacePos(Vector2 pos)
	{
		return new Vector2((float)LeftOnScreen + (float)WidthOnScreen * pos.x / HudBehaviour.Instance.Size.x, (float)TopOnScreen + (float)HeightOnScreen * pos.y / HudBehaviour.Instance.Size.y);
	}

	private void UpdateDownloadingStories()
	{
		if (DownloadingStories != DownloadingStoriesState.NeedsDownload)
		{
			return;
		}
		if (Session == null)
		{
			DownloadingStories = DownloadingStoriesState.Error;
			return;
		}
		if (!IsOnline())
		{
			DownloadingStories = DownloadingStoriesState.Error;
			return;
		}
		bool flag = true;
		foreach (StorySource storySource in Session.StorySources)
		{
			if (storySource.IsValid())
			{
				continue;
			}
			if (!WorkshopManager.Instance.IsSubscribed(storySource.WorkshopId))
			{
				if (WorkshopManager.Instance.HadSubscribeError(storySource.WorkshopId))
				{
					DownloadingStories = DownloadingStoriesState.Error;
					return;
				}
				WorkshopManager.Instance.SubscribeItem(storySource.WorkshopId, canQueue: false);
				flag = false;
			}
			else if (!WorkshopManager.Instance.IsDownloaded(storySource.WorkshopId))
			{
				WorkshopManager.Instance.StartDownloadingItem(storySource.WorkshopId);
				flag = false;
			}
			else if (string.IsNullOrEmpty(storySource.AbsolutePath))
			{
				WorkshopManager.Instance.ApplyAbsolutePathToDownloadedItem(storySource);
			}
		}
		if (flag)
		{
			DownloadingStories = DownloadingStoriesState.Ready;
		}
	}

	public void FinishHandleInputOnThread()
	{
		using (new UnityProfileMarker(FinishHandleInputOnThreadStr))
		{
			HandleInputFinishedEvent.WaitOne();
			if (RequestedAutoSaveFromThread != SaveGameType.Invalid)
			{
				if (Session != null)
				{
					Session.AutoSave(RequestedAutoSaveFromThread, overwriteAllSlots: false);
				}
				RequestedAutoSaveFromThread = SaveGameType.Invalid;
			}
			foreach (BaseDialog queuedDialog in QueuedDialogs)
			{
				PushDialog(queuedDialog);
			}
			QueuedDialogs.Clear();
		}
	}

	private void MenuAndDialogHandleInput(InputFrame inputFrame)
	{
		if (DialogStack.Count > 0)
		{
			DialogStack[DialogStack.Count - 1].HandleInput(inputFrame);
		}
		else if (Menu != null)
		{
			Menu.HandleInput(inputFrame);
		}
	}

	public void HandleInputOnThread(BaseTaskData data)
	{
		HandleInputThread = Thread.CurrentThread;
		try
		{
			bool flag = !GUIHasControl;
			bool flag2 = Menu == null && flag;
			InputFrame inputFrame = ((HandleInputFrame.Frame != -1) ? HandleInputFrame : null);
			InputFrame.DisableActionMerging = NumTimesUpdateWasCalledBetweenFixedUpdates > 0;
			if (Session != null && Session.State == SessionState.Started && flag2)
			{
				using (new UnityProfileMarker(HandleInputStr))
				{
					Session.HandleInput(inputFrame);
				}
			}
			OnlineParty.PushToTalkButtonHeld = flag && OnlineParty.IsInMultiplayerGame() && Settings.VoiceChatEnabled && Settings.PushBtnToTalkEnabled && InputFunctionManager.IsPressed(InputFunction.VoiceChatPushToTalk);
			InputFrame.DisableActionMerging = false;
			NumTimesUpdateWasCalledBetweenFixedUpdates++;
		}
		catch (Exception ex)
		{
			Debug.Log("Error in HandleInputOnThread: " + ex.Message + ex.StackTrace);
			Launcher.Instance.SetExceptionToHandle("Error in HandleInputOnThread", ex.Message, ex.StackTrace);
		}
		HandleInputThread = null;
		HandleInputFinishedEvent.Set();
	}

	public void Update()
	{
		DeltaTime = Time.deltaTime;
		UnscaledDeltaTime = Time.unscaledDeltaTime;
		UnscaledTime = Time.unscaledTime;
		RealTimeSinceStartup = Time.realtimeSinceStartup;
		FrameCount = Time.frameCount;
		if (TimeCounter < 1f)
		{
			TimeCounter += UnscaledDeltaTime;
			FrameCounter++;
		}
		else
		{
			LastAvgFrameTime = TimeCounter / (float)FrameCounter;
			FrameCounter = 0;
			TimeCounter = 0f;
		}
		if (Session != null && Session.State == SessionState.Started)
		{
			HudBehaviour.Instance.UnityGrassCameraBehaviour.StartGrassTask(MainGrassRenderer);
		}
		if (SteamInitialized)
		{
			SteamAPI.RunCallbacks();
		}
		if (Screen.width != ScreenWidth || Screen.height != ScreenHeight)
		{
			OnScreenResized();
		}
		if (State >= GameState.TitleMenu)
		{
			using (new ProfileMarker(OnlineUpdateTimer))
			{
				OnlineParty.Update();
			}
			UpdateDownloadingStories();
			WorkshopManager.Update();
			AchievementsManager.Update();
		}
		UpdateThreadPool.Update();
		SaveGameManager.Update();
		if (Time.timeScale == 0f)
		{
			FixedUpdate();
		}
		if (IsMenuOpen() || IsDialogOpen() || InfoScreen.Instance.ActiveAndFullyTransitionedIn)
		{
			EventSystem current = EventSystem.current;
			if (current != null)
			{
				GameObject currentSelectedGameObject = current.currentSelectedGameObject;
				Selectable selectable = ((currentSelectedGameObject != null) ? currentSelectedGameObject.GetComponent<Selectable>() : null);
				if (selectable == null || !currentSelectedGameObject.activeInHierarchy || !selectable.IsInteractable())
				{
					Selectable selectable2 = null;
					if (selectable2 == null && current.firstSelectedGameObject != null)
					{
						Selectable component = current.firstSelectedGameObject.GetComponent<Selectable>();
						if (component != null && component.IsInteractable())
						{
							selectable2 = component;
						}
					}
					if (selectable2 == null)
					{
						Selectable[] allSelectablesArray = Selectable.allSelectablesArray;
						foreach (Selectable selectable3 in allSelectablesArray)
						{
							if (selectable3.IsInteractable() && selectable3.gameObject.activeInHierarchy && selectable3.navigation.mode != Navigation.Mode.None)
							{
								selectable2 = selectable3;
								break;
							}
						}
					}
					if (selectable2 != null)
					{
						SelectableBehaviour.InSelectCall = true;
						selectable2.Select();
						SelectableBehaviour.InSelectCall = false;
					}
				}
			}
		}
		using (new UnityProfileMarker(InputFunctionManagerUpdateStr))
		{
			InputFunctionManager.Update();
		}
		InputFrame inputFrame = ((HandleInputFrame.Frame != -1) ? HandleInputFrame : null);
		if (DialogStack.Count > 0)
		{
			DialogStack[DialogStack.Count - 1].PreHandleInput(inputFrame);
		}
		else if (Menu != null)
		{
			Menu.PreHandleInput(inputFrame);
		}
		if (Session != null && Session.State == SessionState.Started)
		{
			Session.PreHandleInput(inputFrame);
		}
		if (!GUIHasControl)
		{
			if (Session != null && Session.IsDebugMenuOpen() && Menu == null)
			{
				Session.DebugMenu.HandleInput(inputFrame);
			}
			MenuAndDialogHandleInput(inputFrame);
		}
		HandleInputFinishedEvent.Reset();
		Instance.UpdateThreadPool.AddTask(HandleInputTaskFunc, null, null, TaskPriority.High);
		if (Session != null && Session.State == SessionState.Started)
		{
			Session.Weather.StartWeatherUpdateTask();
		}
		if (DialogStack.Count > 0)
		{
			BaseDialog baseDialog = DialogStack[DialogStack.Count - 1];
			baseDialog.DialogUpdate();
			if (baseDialog.IsFinished())
			{
				PopDialog();
			}
		}
		if (Menu != null)
		{
			Menu.MenuUpdate();
			if (Menu.IsFadeFinished())
			{
				TitleMenu titleMenu = Menu as TitleMenu;
				PauseMenu pauseMenu = Menu as PauseMenu;
				GameOverMenu gameOverMenu = Menu as GameOverMenu;
				GameCompleteMenu gameCompleteMenu = Menu as GameCompleteMenu;
				DemoTimeoutMenu demoTimeoutMenu = Menu as DemoTimeoutMenu;
				LoadingMenu loadingMenu = Menu as LoadingMenu;
				CharacterCreationMenu characterCreationMenu = Menu as CharacterCreationMenu;
				CinematicMenu cinematicMenu = Menu as CinematicMenu;
				if (titleMenu != null && titleMenu.GetWantNewGame())
				{
					NewGame(titleMenu.GetCharacterCreationSettings());
				}
				else if (titleMenu != null && titleMenu.GetWantLatestSave())
				{
					LoadLatestSaveGame(0);
				}
				else if (titleMenu != null && titleMenu.GetWantLoadSaveGame() != null)
				{
					LoadGame(titleMenu.GetWantLoadSaveGame());
				}
				else if (pauseMenu != null && pauseMenu.GetWantLoadSaveGame() != null)
				{
					LoadGame(pauseMenu.GetWantLoadSaveGame());
				}
				else if (gameOverMenu != null)
				{
					if (gameOverMenu.GetWantLatestSave())
					{
						LoadLatestSaveGame(Session.DifficultySettings.SaveTokensRequired ? Session.GameUniqueId : 0);
					}
					else
					{
						SetState(GameState.TitleMenu);
					}
				}
				else if (gameCompleteMenu != null && gameCompleteMenu.WantQuit)
				{
					SetState(GameState.TitleMenu);
				}
				else if (demoTimeoutMenu != null && demoTimeoutMenu.WantQuit)
				{
					SetState(GameState.TitleMenu);
				}
				else if (titleMenu != null && titleMenu.GetWantRunEditor())
				{
					RunEditor();
				}
				else if (pauseMenu != null && pauseMenu.WantQuit)
				{
					SetState(GameState.TitleMenu);
				}
				else if (pauseMenu != null && pauseMenu.WantRespawn)
				{
					CharacterCreationMenu characterCreationMenu2 = (CharacterCreationMenu)GetMenuBehaviourByPanelName("CharacterCreationMenuPanel");
					characterCreationMenu2.JoiningNetworkGame = true;
					characterCreationMenu2.Respawning = true;
					SetMenu(characterCreationMenu2);
				}
				else if (loadingMenu != null && loadingMenu.Cancelled)
				{
					SetState(GameState.TitleMenu);
				}
				else if (characterCreationMenu != null && characterCreationMenu.WantReturnToTitleMenu)
				{
					SetState(GameState.TitleMenu);
				}
				else if (cinematicMenu != null)
				{
					if (!cinematicMenu.PopNextCinematicState())
					{
						gameCompleteMenu = (GameCompleteMenu)GetMenuBehaviourByPanelName("GameCompletePanel");
						gameCompleteMenu.ShowContinueButton = Session.ShowContinueButton;
						gameCompleteMenu.CompletionMessage = cinematicMenu.CompletionMessage;
						SetMenu(gameCompleteMenu);
					}
				}
				else if (State == GameState.LoadingSession || State == GameState.UnityInitSession || State == GameState.ReceivingNetworkSession || State == GameState.SendingNetworkSession)
				{
					SetMenu(GetMenuBehaviourByPanelName("LoadingPanel"));
				}
				else
				{
					if (Session == null || Session.State != SessionState.Started)
					{
						Debug.LogWarning("Unexpected Session state: " + ((Session != null) ? Session.State.ToString() : "null"));
					}
					SetMenu(null);
				}
			}
		}
		else if (Session != null && Session.IsInMultiplayerGameAsFollower() && Session.State >= SessionState.Loaded && Session.State <= SessionState.Started && Session.IncomingCharacter == null && Session.InputFrame > Session.InputFrameWhenWeLastSentCreateCharacter)
		{
			PlayerRecord localPlayerRecord = Session.GetLocalPlayerRecord();
			if (localPlayerRecord != null && !localPlayerRecord.HasCreatedCharacter)
			{
				CharacterCreationMenu characterCreationMenu3 = (CharacterCreationMenu)GetMenuBehaviourByPanelName("CharacterCreationMenuPanel");
				characterCreationMenu3.JoiningNetworkGame = true;
				characterCreationMenu3.Respawning = false;
				SetMenu(characterCreationMenu3);
			}
		}
		BaseResource.UpdateResources();
		Story.LoadAssetBundlesOnMainThreadIfNeeded();
		switch (State)
		{
		case GameState.Loading:
			if (LoadingThread.Join(0) && BaseResource.AreAllResourcesLoaded())
			{
				SetState(GameState.TitleMenu);
			}
			break;
		case GameState.LoadingSession:
		{
			if (!LoadingSessionThread.Join(0))
			{
				break;
			}
			if (LoadingSessionInfo.Success)
			{
				if (LoadingSessionInfo.LoadType == LoadSessionInfo.Type.TextDump)
				{
					SetState(GameState.TitleMenu);
				}
				else
				{
					SetState(GameState.UnityInitSession);
				}
				break;
			}
			LoadingMenu loadingMenu3 = Menu as LoadingMenu;
			if (loadingMenu3 != null)
			{
				loadingMenu3.SetFinished();
				if (!loadingMenu3.IsFadeFinished())
				{
					break;
				}
			}
			if (OnlineParty.IsInLobby())
			{
				OnlineParty.ClearLobby();
			}
			SetState(GameState.TitleMenu);
			if (!string.IsNullOrEmpty(LoadingSessionInfo.ErrorMessage))
			{
				ShowMessageBox(LoadingSessionInfo.ErrorMessage);
			}
			break;
		}
		case GameState.UnityInitSession:
		{
			if (Session.State != SessionState.UnityInited)
			{
				break;
			}
			LoadingMenu loadingMenu2 = Menu as LoadingMenu;
			if (loadingMenu2 != null)
			{
				loadingMenu2.SetFinished();
				if (!loadingMenu2.IsFadeFinished())
				{
					break;
				}
			}
			SetState(GameState.RunningSession);
			break;
		}
		case GameState.RunningSession:
		{
			if (Session.WantFinish == WantFinishState.None || Session.Transition != 0f || HudBehaviour.Instance.SkillsChangePopupTransition != 0f)
			{
				break;
			}
			WantFinishState wantFinish = Session.WantFinish;
			Session.WantFinish = WantFinishState.None;
			switch (wantFinish)
			{
			case WantFinishState.TitleMenu:
				SetState(GameState.TitleMenu);
				break;
			case WantFinishState.PauseMenu:
				SetMenu(GetMenuBehaviourByPanelName("PauseMenuPanel"));
				break;
			case WantFinishState.GameOverMenu:
				SetMenu(GetMenuBehaviourByPanelName("GameOverPanel"));
				break;
			case WantFinishState.GameCompleteMenu:
				if (Session.WantCinematicState != CinematicState.None)
				{
					CinematicMenu cinematicMenu2 = (CinematicMenu)GetMenuBehaviourByPanelName("CinematicPanel");
					cinematicMenu2.NextCinematicState = Session.WantCinematicState;
					cinematicMenu2.ExfilVehicle = Session.ExfilVehicle;
					Session.ExfilCharacters.CopyToList(cinematicMenu2.ExfilCharacters);
					if (Session.ExfilCharacters.Count > 0)
					{
						Character character5 = Session.ExfilCharacters[Session.ExfilCharacters.Count - 1];
						switch (Session.WantCinematicState)
						{
						case CinematicState.ExfilCharacter:
							cinematicMenu2.CompletionMessage = Translate("CINEMATIC_Refuge").Replace("%1", character5.GetDisplayNameString());
							break;
						case CinematicState.RipVehicle:
							cinematicMenu2.CompletionMessage = Translate("CINEMATIC_RIP").Replace("%1", character5.GetDisplayNameString());
							break;
						case CinematicState.OutbreakHelicopter:
							cinematicMenu2.CompletionMessage = Translate("CINEMATIC_Outbreak").Replace("%1", character5.GetDisplayNameString());
							break;
						}
						cinematicMenu2.CompletionMessage = StringUtil.ApplyFormulae(cinematicMenu2.CompletionMessage, character5, character5);
					}
					SetMenu(cinematicMenu2);
				}
				else
				{
					GameCompleteMenu gameCompleteMenu2 = (GameCompleteMenu)GetMenuBehaviourByPanelName("GameCompletePanel");
					gameCompleteMenu2.ShowContinueButton = Session.ShowContinueButton;
					gameCompleteMenu2.CompletionMessage = Translate("MENU_Congratulations");
					SetMenu(gameCompleteMenu2);
				}
				break;
			case WantFinishState.DemoTimeoutMenu:
				SetMenu(GetMenuBehaviourByPanelName("DemoTimeoutPanel"));
				break;
			case WantFinishState.LoadLatestSave:
				LoadLatestSaveGame(Session.DifficultySettings.SaveTokensRequired ? Session.GameUniqueId : 0);
				break;
			case WantFinishState.LoadNewMap:
			{
				List<StorySource> currentStorySources = GetCurrentStorySources();
				if (!CurrentStory.Settings.HitTheRoadStory.IsNull())
				{
					List<StorySource> recursionCheck = new List<StorySource>();
					List<StorySource> list = new List<StorySource>();
					string error = string.Empty;
					StorySource storySource = StorySource.FromStoryId(CurrentStory.Settings.HitTheRoadStory);
					if (SelectStoryMenu.AddStoryDependenciesRecursive(storySource, null, list, recursionCheck, ref error, storySource))
					{
						for (int j = 0; j < currentStorySources.Count; j++)
						{
							if (currentStorySources[j].Equals(CurrentStory.StorySource))
							{
								currentStorySources.RemoveRange(0, j + 1);
								break;
							}
						}
						currentStorySources.InsertRange(0, list);
					}
					else
					{
						if (string.IsNullOrEmpty(error))
						{
							break;
						}
						Debug.Log(error);
					}
				}
				CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
				Community playerCommunity = Session.CommunityManager.PlayerCommunity;
				Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
				if (localControlledCharacter == null || !(localControlledCharacter.InsideBuilding is EnterableVehicle enterableVehicle))
				{
					break;
				}
				Character character = playerCommunity.Leader;
				if (character == null || character.InsideBuilding != enterableVehicle)
				{
					character = localControlledCharacter;
				}
				Character[] inhabitants;
				if (IsCurrentStoryListEqualTo(AchievementsManager.SandboxMode, AchievementsManager.AllowAchievementsWithMods))
				{
					AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteSandbox_HitTheRoad);
					if (Session.DifficultySettings.DifficultyName == "Hard")
					{
						if (Session.LoneWolf)
						{
							AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteSandbox_Hard_LoneWolf);
						}
						bool flag = false;
						bool flag2 = false;
						inhabitants = enterableVehicle.Inhabitants;
						foreach (Character character2 in inhabitants)
						{
							if (character2 != null && character2 != character && character2.AliveAndNotZombie && !character2.IsPlayerAvatar())
							{
								flag |= Relationship.HasRomanticRelationship(character, character2);
								flag2 |= Relationship.GetRelationship(character, character2) == RelationshipType.FriendsWith;
							}
						}
						if (flag && flag2)
						{
							AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteSandbox_Hard_WithFriends);
						}
					}
				}
				if (IsCurrentStoryListEqualTo(AchievementsManager.StoryMode, AchievementsManager.AllowAchievementsWithMods))
				{
					AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteStory_HitTheRoad);
					if (Session.DifficultySettings.DifficultyName == "Harder" && BaseObjectManager.Instance.GetObjectByUniqueID(Character.EmmaOConnor) is Character character3 && (character3.InsideBuilding == enterableVehicle || (character3.AliveAndNotZombie && character3.IsDisappeared())))
					{
						AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteStory_Hard);
					}
				}
				GameTerrain.Instance.GetNearestHitTheRoadPoint(character.PosXZ, float.MaxValue, out var traits);
				CharacterCreationSettings characterCreationSettings = new CharacterCreationSettings();
				characterCreationSettings.HitTheRoadCount = Session.HitTheRoadCount + 1;
				characterCreationSettings.AchievementsEnabled = Session.AchievementsEnabled;
				characterCreationSettings.SetupFromCharacter(character);
				characterCreationSettings.GangName = playerCommunity.CommunityName;
				characterCreationSettings.RandomSeed = nonDeterministicRand.Next();
				characterCreationSettings.StartHourOfDay = nonDeterministicRand.RandomFloat() * 24f;
				characterCreationSettings.StartDayOfYear = Mathf.CeilToInt(Session.DayOfYear);
				characterCreationSettings.MapSize = GameTerrain.GetMapSizeFromSize(GameTerrain.Instance.Size);
				float travelTime = (characterCreationSettings.StartHourOfDay / 24f + (float)characterCreationSettings.StartDayOfYear - Session.DayOfYear) * Sun.DayLengthSecs;
				Dictionary<Character, CrewMemberSettings> dictionary = new Dictionary<Character, CrewMemberSettings>();
				dictionary[character] = characterCreationSettings;
				inhabitants = enterableVehicle.Inhabitants;
				foreach (Character character4 in inhabitants)
				{
					if (character4 != null && character4 != character)
					{
						CrewMemberSettings crewMemberSettings = new CrewMemberSettings();
						crewMemberSettings.SetupFromCharacter(character4);
						characterCreationSettings.Crew.Add(crewMemberSettings);
						dictionary[character4] = crewMemberSettings;
					}
				}
				foreach (KeyValuePair<Character, CrewMemberSettings> item in dictionary)
				{
					item.Value.SetupMemoriesAndRelationships(item.Key, dictionary, travelTime);
				}
				characterCreationSettings.VehicleProto = enterableVehicle.GetPropPrototype();
				characterCreationSettings.VehicleDamageFraction = enterableVehicle.GetDamageFraction();
				characterCreationSettings.VehicleMaterialVariation = enterableVehicle.GetMaterialVariation();
				characterCreationSettings.VehicleColorVariation = enterableVehicle.GetColorVariation();
				characterCreationSettings.VehicleColorVariation2 = enterableVehicle.GetColorVariation2();
				characterCreationSettings.VehicleColorVariation3 = enterableVehicle.GetColorVariation3();
				characterCreationSettings.VehicleColorVariation4 = enterableVehicle.GetColorVariation4();
				enterableVehicle.Inventory.CopyToList(characterCreationSettings.VehicleInventory, stripItemsThatCantTravel: true);
				playerCommunity.EquipmentPolicies.CopyToList(characterCreationSettings.CommunityEquipmentPolicies);
				foreach (Story currentStory in CurrentStories)
				{
					foreach (string hitTheRoadKeepVariable in currentStory.Settings.HitTheRoadKeepVariables)
					{
						float variable = StoryManager.Instance.GetVariable(hitTheRoadKeepVariable, null, null);
						if (variable != 0f)
						{
							characterCreationSettings.Variables.Add(new KeyValuePair<string, float>(hitTheRoadKeepVariable, variable));
						}
					}
				}
				characterCreationSettings.DifficultySettings = traits.GenerateDifficultySettings(Session.DifficultySettings);
				NewGame(characterCreationSettings, currentStorySources, forceProcedurallyGenerated: false, Session.GameUniqueId);
				break;
			}
			}
			break;
		}
		}
		IconGenerator.GenerateIcons();
		if (Session != null && Session.State == SessionState.Started)
		{
			Session.NonDeterministicUpdate();
			if (Settings.PiPBackgroundEnabled)
			{
				HudBehaviour.Instance.UnityPipGrassCameraBehaviour.StartGrassTask(PipGrassRenderer);
			}
		}
		else
		{
			FinishHandleInputOnThread();
		}
		SoundManager.PlayQueuedSounds();
		using (new ProfileMarker(ActionMenuUpdateTimer))
		{
			HudBehaviour.Instance.UpdateUnityActionMenu();
		}
		NotificationManager.Instance.Update();
		ButtonPromptBarBehaviour.Instance.UpdateButtonPrompts();
		MusicManager.Update();
		Sun.Update();
		UpdateUnityCursor();
	}

	public void FixedUpdate()
	{
		FixedFrameCount++;
		MinimapCameraBehaviour.MakeSureBuildingCharactersToRenderThreadIsFinished();
		if (Session != null && Session.State == SessionState.Started)
		{
			int num = Math.Max(1, Session.GetFramesPerFrame());
			if (FixedFrameCount % num != 0)
			{
				return;
			}
			if (NumInputFramesToAdd > 0)
			{
				if (NumTimesUpdateWasCalledBetweenFixedUpdates == 0)
				{
					for (int i = 0; i < PrevHandleInputFrame.Actions.Count; i++)
					{
						if (InputFrame.CanActionBeMerged(PrevHandleInputFrame.Actions[i].Type))
						{
							HandleInputFrame.AddAction(PrevHandleInputFrame.Actions[i]);
						}
					}
					for (int j = 0; j < PrevHandleInputFrame.Actions.Count; j++)
					{
						if (InputFrame.IsActionWhichYouMustLetGoOfToFireAgain(PrevHandleInputFrame.Actions[j].Type))
						{
							HandleInputFrame.AddAction(PrevHandleInputFrame.Actions[j]);
						}
					}
				}
				Session.PostHandleInput(HandleInputFrame);
				if (Session.IsInMultiplayerGame())
				{
					OnlineParty.SendInputFrame(Session, HandleInputFrame);
					if (NumInputFramesToAdd > 1)
					{
						mergeableActions.Clear();
						for (int k = 0; k < HandleInputFrame.Actions.Count; k++)
						{
							if (InputFrame.CanActionBeMerged(HandleInputFrame.Actions[k].Type))
							{
								mergeableActions.Add(HandleInputFrame.Actions[k]);
							}
						}
						for (int l = 1; l < NumInputFramesToAdd; l++)
						{
							HandleInputFrame.ClearActions(Session.GetInputFrameBeingSent());
							for (int m = 0; m < mergeableActions.Count; m++)
							{
								HandleInputFrame.AddAction(mergeableActions[m]);
							}
							OnlineParty.SendInputFrame(Session, HandleInputFrame);
						}
						mergeableActions.Clear();
					}
				}
			}
			Session.NonDeterministicFixedUpdate(HandleInputFrame);
			NumInputFramesToAdd = OnlineParty.GetHowManyInputFramesToAddThisFrame(Session);
			if (NumInputFramesToAdd > 0)
			{
				PrevHandleInputFrame.CopyActions(HandleInputFrame);
				HandleInputFrame.ClearActions(Session.GetInputFrameBeingSent());
			}
			else
			{
				PrevHandleInputFrame.CopyActions(HandleInputFrame);
				HandleInputFrame.ClearActions(-1);
			}
		}
		else
		{
			NumInputFramesToAdd = 0;
			PrevHandleInputFrame.CopyActions(HandleInputFrame);
			HandleInputFrame.ClearActions(-1);
		}
		NumTimesUpdateWasCalledBetweenFixedUpdates = 0;
	}

	public void LateUpdate()
	{
		float a = 1f;
		if (Menu != null)
		{
			a = BaseMenu.GetFadeBlackness();
		}
		else if (Session != null)
		{
			a = 1f - Session.Transition;
		}
		UnityFaderPanelObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, a);
		UnityMenuMaskObj.SetActive(Menu != null);
		if (Session != null && Session.State == SessionState.Started && InfoScreen.Instance.Transition == 0f)
		{
			MinimapCameraBehaviour.StartBuildingCharactersToRenderListOnThread();
		}
	}

	public void OnGUI()
	{
		if (OverrideGUISkin == null)
		{
			if (OriginalGUISkin == null)
			{
				OriginalGUISkin = GUI.skin;
			}
			GUISkin originalGUISkin = OriginalGUISkin;
			Font font = null;
			font = Settings.Language switch
			{
				Language.Japanese => Resources.Load<Font>("Fonts/NotoSansJP-Medium"), 
				Language.Korean => Resources.Load<Font>("Fonts/NotoSansKR-Medium"), 
				Language.SimplifiedChinese => Resources.Load<Font>("Fonts/NotoSansSC-Medium"), 
				Language.TraditionalChinese => Resources.Load<Font>("Fonts/NotoSansTC-Medium"), 
				Language.Thai => Resources.Load<Font>("Fonts/NotoSansThai-Black"), 
				Language.Persian => Resources.Load<Font>("Fonts/NotoSansArabic-Black"), 
				_ => originalGUISkin.font, 
			};
			OverrideGUISkin = ScriptableObject.CreateInstance<GUISkin>();
			OverrideGUISkin.font = font;
			OverrideGUISkin.box = originalGUISkin.box;
			OverrideGUISkin.box.font = font;
			OverrideGUISkin.button = originalGUISkin.button;
			OverrideGUISkin.button.font = font;
			OverrideGUISkin.label = originalGUISkin.label;
			OverrideGUISkin.label.font = font;
			OverrideGUISkin.toggle = originalGUISkin.toggle;
			OverrideGUISkin.toggle.font = font;
			OverrideGUISkin.textField = originalGUISkin.textField;
			OverrideGUISkin.textField.font = font;
			OverrideGUISkin.textArea = originalGUISkin.textArea;
			OverrideGUISkin.textArea.font = font;
			OverrideGUISkin.window = originalGUISkin.window;
			OverrideGUISkin.window.font = font;
			OverrideGUISkin.horizontalSlider = originalGUISkin.horizontalSlider;
			OverrideGUISkin.horizontalSliderThumb = originalGUISkin.horizontalSliderThumb;
			OverrideGUISkin.verticalSlider = originalGUISkin.verticalSlider;
			OverrideGUISkin.verticalSliderThumb = originalGUISkin.verticalSliderThumb;
			OverrideGUISkin.horizontalScrollbar = originalGUISkin.horizontalScrollbar;
			OverrideGUISkin.horizontalScrollbarThumb = originalGUISkin.horizontalScrollbarThumb;
			OverrideGUISkin.horizontalScrollbarLeftButton = originalGUISkin.horizontalScrollbarLeftButton;
			OverrideGUISkin.horizontalScrollbarRightButton = originalGUISkin.horizontalScrollbarRightButton;
			OverrideGUISkin.verticalScrollbar = originalGUISkin.verticalScrollbar;
			OverrideGUISkin.verticalScrollbarThumb = originalGUISkin.verticalScrollbarThumb;
			OverrideGUISkin.verticalScrollbarUpButton = originalGUISkin.verticalScrollbarUpButton;
			OverrideGUISkin.verticalScrollbarDownButton = originalGUISkin.verticalScrollbarDownButton;
		}
		GUI.skin = OverrideGUISkin;
		if (IsInLoadedSession())
		{
			Session.OnGUI();
		}
		if (GraphicsDebugMenu.ShowFPS)
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			FPSString.Length = 0;
			FPSString.Append(Avg);
			FPSString.AppendWithoutGarbage(LastAvgFrameTime * 1000f, 2);
			FPSString.Append(ms);
			FPSString.AppendWithoutGarbage(1f / LastAvgFrameTime, 2);
			FPSString.Append(fps);
			FPSString.Append(Frame);
			FPSString.AppendWithoutGarbage(unscaledDeltaTime * 1000f, 2);
			FPSString.Append(ms);
			FPSString.AppendWithoutGarbage(1f / unscaledDeltaTime, 2);
			FPSString.Append(fps);
			Session instance = Session.Instance;
			if (instance != null && instance.State == SessionState.Started && instance.IsInMultiplayerGame())
			{
				bool flag = true;
				foreach (PartyMember partyMember in OnlineParty.Instance.PartyMembers)
				{
					if (!partyMember.IsBannedOrIgnored())
					{
						if (flag)
						{
							FPSString.Append('\n');
						}
						else
						{
							FPSString.Append(',');
							FPSString.Append(' ');
						}
						flag = false;
						FPSString.Append(partyMember.GetPlayerName());
						FPSString.Append(':');
						FPSString.Append(' ');
						FPSString.AppendWithoutGarbage(partyMember.ReceivedInputFrame - instance.InputFrame, 0);
						FPSString.Append(' ');
						FPSString.Append('(');
						FPSString.AppendWithoutGarbage(partyMember.ReceivedInputFrame, 0);
						FPSString.Append(')');
					}
				}
				FPSString.Append(' ');
				FPSString.Append('|');
				FPSString.AppendWithoutGarbage(instance.InputFrame, 0);
				FPSString.Append(' ');
				FPSString.Append('|');
				FPSString.Append(' ');
				FPSString.AppendWithoutGarbage(OnlineParty.LastSentInputFrame, 0);
				FPSString.Append(' ');
				FPSString.Append('|');
				FPSString.Append(' ');
				FPSString.AppendWithoutGarbage(Session.FramesWithoutPrediction);
				FPSString.Append(' ');
				FPSString.Append('|');
				FPSString.Append(' ');
				FPSString.AppendWithoutGarbage(HandleInputFrame.Frame);
			}
			GUI.color = ((InfoScreen.Instance != null && InfoScreen.Instance.ActiveAndFullyTransitionedIn) ? Color.black : Color.white);
			GUI.Label(new Rect(30f, Screen.height - 60, 600f, 60f), FPSString.ToString());
			GUI.color = Color.white;
		}
		if (GraphicsDebugMenu.ShowVehicleUI && Hud.Instance != null)
		{
			Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
			if (localControlledCharacter != null && localControlledCharacter.InsideBuilding is EnterableVehicle enterableVehicle)
			{
				FPSString.Length = 0;
				FPSString.Append(Translate(HUD_VehicleGear));
				FPSString.Append(':');
				FPSString.Append(' ');
				if (enterableVehicle.Gear == enterableVehicle.Prototype.VehicleGearRatios.Length - 1)
				{
					FPSString.Append('R');
				}
				else if (enterableVehicle.Gear == 0)
				{
					FPSString.Append('N');
				}
				else
				{
					FPSString.AppendWithoutGarbage(enterableVehicle.Gear);
				}
				FPSString.Append(',');
				FPSString.Append(' ');
				FPSString.Append(Translate(HUD_VehicleRPM));
				FPSString.Append(':');
				FPSString.AppendWithoutGarbage(enterableVehicle.CurrentRPM, 1);
				FPSString.Append(' ');
				FPSString.Append('(');
				FPSString.AppendWithoutGarbage(enterableVehicle.WheelRPM, 1);
				FPSString.Append(')');
				FPSString.Append(',');
				FPSString.Append(' ');
				FPSString.Append(Translate(HUD_VehicleSpeed));
				FPSString.Append(':');
				FPSString.Append(' ');
				if (Settings.UseMetricWeights)
				{
					FPSString.AppendWithoutGarbage(enterableVehicle.GetSpeedInKmh(), 1);
					FPSString.Append(Translate(HUD_Kmh));
				}
				else
				{
					FPSString.AppendWithoutGarbage(enterableVehicle.GetSpeedInMph(), 1);
					FPSString.Append(Translate(HUD_Mph));
				}
				GUI.color = ((InfoScreen.Instance != null && InfoScreen.Instance.ActiveAndFullyTransitionedIn) ? Color.black : Color.white);
				GUI.Label(new Rect(30f, Screen.height - 300, 600f, 60f), FPSString.ToString());
				GUI.color = Color.white;
			}
		}
		GUIHasControl = GUIUtility.hotControl != 0;
	}

	public void OnDrawGizmos()
	{
		if (IsInLoadedSession())
		{
			Session.OnDrawGizmos();
		}
	}

	private void UpdateUnityCursor()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		CursorLockMode wantCursorLockMode = CursorLockMode.None;
		bool hovering = false;
		bool forbidden = false;
		Texture2D texture2D = GameCursor.CursorPointer;
		Color cursorCol = Color.white;
		float angle = 0f;
		Vector2 offset = Vector2.zero;
		bool flag = false;
		if (IsInLoadedSession() && !IsMenuOpen() && !IsDialogOpen() && !HudBehaviour.Instance.IsChatting())
		{
			if (NotificationManager.Instance.IsDisplayingNotification() && !InfoScreen.Instance.Active)
			{
				if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
				{
					wantCursorLockMode = CursorLockMode.Locked;
					switch (Session.Hud.Cursor.GetCursorState())
					{
					case CursorState.Select:
						texture2D = GameCursor.CursorSelect;
						break;
					case CursorState.SelectHover:
						hovering = true;
						texture2D = GameCursor.CursorSelect;
						break;
					case CursorState.DirectControl:
						texture2D = null;
						break;
					}
				}
			}
			else
			{
				texture2D = Session.Hud.Cursor.GetCursorTexture(out angle, out offset, out wantCursorLockMode, out hovering, out forbidden, out cursorCol);
				flag = Session.Hud.Cursor.IsPlacingBuilding;
			}
		}
		else if (MenuSelectableBehaviour.LinkyCursorRefCount > 0)
		{
			texture2D = GameCursor.CursorLink;
		}
		if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
		{
			if (IsMenuOpen() || IsDialogOpen() || HudBehaviour.Instance.IsChatting())
			{
				texture2D = null;
			}
			else if (InfoScreen.Instance.Active && !InfoScreen.Instance.IsShowingMap())
			{
				texture2D = null;
			}
		}
		if (!Application.isFocused)
		{
			wantCursorLockMode = CursorLockMode.None;
		}
		if (LastCursorLockMode != CursorLockMode.Locked && wantCursorLockMode == CursorLockMode.Locked)
		{
			MouseOriginalScreenPos = InputFunctionManager.GetMousePosition();
		}
		switch (wantCursorLockMode)
		{
		case CursorLockMode.Confined:
			CursorUtil.StartClipping(LeftOnScreen, TopOnScreen, instance.MainPanelWidthOnScreen, HeightOnScreen);
			break;
		case CursorLockMode.Locked:
			CursorUtil.StartClipping(LeftOnScreen, TopOnScreen, instance.MainPanelWidthOnScreen, HeightOnScreen);
			InputFunctionManager.SetMousePosition(new Vector2((float)LeftOnScreen + (float)instance.MainPanelWidthOnScreen * (flag ? CursorXFracWhenPlacingBuilding : 0.5f), (float)TopOnScreen + (float)HeightOnScreen * 0.5f));
			break;
		case CursorLockMode.None:
			CursorUtil.StopClipping();
			break;
		}
		if (LastCursorLockMode == CursorLockMode.Locked && wantCursorLockMode != CursorLockMode.Locked && (Hud.Instance == null || !Hud.Instance.Dragging))
		{
			InputFunctionManager.SetMousePosition(MouseOriginalScreenPos);
		}
		LastCursorLockMode = wantCursorLockMode;
		CursorUtil.SetCursorVisible(!Application.isFocused);
		UnityCursorObj.SetActive(CursorUtil.IsMouseInClientBounds() && texture2D != null);
		UnityCursorRectTransform.position = MathUtil.ToXY(InputFunctionManager.Instance.GetMousePosition() + new Vector2(offset.x * UnityCursorRectTransform.rect.width, offset.y * UnityCursorRectTransform.rect.height) * ScreenScale);
		UnityCursorRectTransform.eulerAngles = new Vector3(0f, 0f, angle * 57.29578f);
		UnityCursorRawImage.texture = texture2D;
		UnityCursorRawImage.color = cursorCol;
		UnityCursorForbidden.enabled = forbidden;
		if (UnityCursorAnimator.isInitialized)
		{
			UnityCursorAnimator.SetBool(AnimHash.Hovering, hovering);
		}
	}

	public BaseMenu GetMenuBehaviourByPanelName(string panelName)
	{
		return UnityLauncherObj.transform.Find("MenuMask/" + panelName).gameObject.GetComponent<BaseMenu>();
	}

	public void SetMenu(BaseMenu menu)
	{
		if (Menu != null)
		{
			Menu.OnDeactivate(popped: true);
		}
		Menu = menu;
		if (Menu != null)
		{
			Menu.OnActivate();
		}
		if (Menu != null && UnityHudBehaviour.IsActive())
		{
			UnityHudBehaviour.OnDeactivate();
			SoundManager.Instance.OnMenuOpen();
			UnityUICameraObj.AddComponent<AudioListener>();
			UnityUICameraObj.SetActive(value: true);
		}
		else if (Menu == null && !UnityHudBehaviour.IsActive())
		{
			UnityEngine.Object.Destroy(UnityUICameraObj.GetComponent<AudioListener>());
			UnityUICameraObj.SetActive(value: false);
			UnityHudBehaviour.OnActivate();
		}
	}

	public bool IsMenuOpen()
	{
		return Menu != null;
	}

	private BaseMenu GetMenu()
	{
		BaseMenu baseMenu = Menu;
		while (baseMenu != null && baseMenu.ChildMenu != null)
		{
			baseMenu = baseMenu.ChildMenu;
		}
		return null;
	}

	private BaseSelectionOwner GetCurrentSelectionOwner()
	{
		if (IsDialogOpen())
		{
			return GetDialog();
		}
		if (Menu != null)
		{
			return GetMenu();
		}
		if (InfoScreen.Instance.Active)
		{
			return InfoScreen.Instance.GetCurrentPage();
		}
		return null;
	}

	public ActionMenu GetActionMenu()
	{
		Hud instance = Hud.Instance;
		Session instance2 = Session.Instance;
		if (IsDialogOpen())
		{
			return GetDialog().GetActionMenu();
		}
		if (IsMenuOpen())
		{
			return Menu.GetActionMenu();
		}
		if (!InfoScreen.Instance.Active && NotificationManager.Instance.IsDisplayingNotification())
		{
			return null;
		}
		if (State == GameState.RunningSession && instance != null && instance2 != null && !instance2.IsDebugMenuOpen())
		{
			return instance.Cursor;
		}
		return null;
	}

	public BaseDialog GetDialogBehaviourByPanelName(string panelName)
	{
		return UnityLauncherObj.transform.Find(panelName).gameObject.GetComponent<BaseDialog>();
	}

	private void InitDialogs()
	{
		UnityMessageBox = (MessageBox)GetDialogBehaviourByPanelName("MessageBoxPanel");
		UnityConfirmationBox = (ConfirmationBox)GetDialogBehaviourByPanelName("ConfirmationBoxPanel");
		UnityHitTheRoadDialog = (HitTheRoadDialog)GetDialogBehaviourByPanelName("HitTheRoadBoxPanel");
		UnityYesNoCancelBox = (YesNoCancelBox)GetDialogBehaviourByPanelName("YesNoCancelBoxPanel");
		UnityInputBox = (InputBox)GetDialogBehaviourByPanelName("InputBoxPanel");
		UnityInputBoxMultiline = (InputBox)GetDialogBehaviourByPanelName("InputBoxPanelMultiline");
		UnityDifficultyDialog = (DifficultyDialog)GetDialogBehaviourByPanelName("DifficultyPanel");
		UnityAllowJoinDialog = (AllowJoinDialog)GetDialogBehaviourByPanelName("AllowJoinDialogBoxPanel");
		UnityEquipmentTransferAmountBox = (EquipmentTransferAmountBox)GetDialogBehaviourByPanelName("EquipmentTransferAmountBoxPanel");
		UnityCraftAmountBox = (CraftAmountBox)GetDialogBehaviourByPanelName("CraftingAmountBoxPanel");
		UnityCraftLimitBox = (CraftLimitBox)GetDialogBehaviourByPanelName("CraftLimitBoxPanel");
		UnityGatePolicyDialog = (GatePolicyDialog)GetDialogBehaviourByPanelName("GatePolicyPanel");
		UnityStoragePolicyDialog = (StoragePolicyDialog)GetDialogBehaviourByPanelName("StoragePolicyPanel");
		UnityDesignatedLiquidDialog = (DesignatedLiquidDialog)GetDialogBehaviourByPanelName("DesignatedLiquidPanel");
		UnityEquipmentPolicyDialog = (EquipmentPolicyDialog)GetDialogBehaviourByPanelName("EquipmentPolicyPanel");
	}

	public void ShowMessageBox(string message)
	{
		MessageBox unityMessageBox = UnityMessageBox;
		unityMessageBox.Message = message;
		Instance.PushDialog(unityMessageBox);
	}

	public void ShowConfirmationBox(string message, ConfirmationBox.ConfirmFunction onConfirm, bool needsSession = false)
	{
		ConfirmationBox unityConfirmationBox = UnityConfirmationBox;
		unityConfirmationBox.Message = message;
		unityConfirmationBox.OkButtonHash = BaseDialog.MENU_OK;
		unityConfirmationBox.OnConfirm = onConfirm;
		unityConfirmationBox.NeedSession = needsSession;
		Instance.PushDialog(unityConfirmationBox);
	}

	public void ShowConfirmationBox(string message, string okButtonTranslationTag, ConfirmationBox.ConfirmFunction onConfirm, bool needsSession = false)
	{
		ConfirmationBox unityConfirmationBox = UnityConfirmationBox;
		unityConfirmationBox.Message = message;
		unityConfirmationBox.OkButtonHash = StringUtil.JenkinsHash(okButtonTranslationTag);
		unityConfirmationBox.OnConfirm = onConfirm;
		unityConfirmationBox.NeedSession = needsSession;
		Instance.PushDialog(unityConfirmationBox);
	}

	public void ShowHitTheRoadBox(RoadDestinationTraits traits, HitTheRoadDialog.ConfirmFunction onConfirm)
	{
		HitTheRoadDialog unityHitTheRoadDialog = UnityHitTheRoadDialog;
		unityHitTheRoadDialog.Traits = traits;
		unityHitTheRoadDialog.OnConfirm = onConfirm;
		Instance.PushDialog(unityHitTheRoadDialog);
	}

	public void ShowYesNoCancelBox(string message, YesNoCancelBox.ConfirmFunction onYes, YesNoCancelBox.ConfirmFunction onNo, bool needsSession = false)
	{
		YesNoCancelBox unityYesNoCancelBox = UnityYesNoCancelBox;
		unityYesNoCancelBox.Message = message;
		unityYesNoCancelBox.OnYes = onYes;
		unityYesNoCancelBox.OnNo = onNo;
		unityYesNoCancelBox.NeedSession = needsSession;
		Instance.PushDialog(unityYesNoCancelBox);
	}

	public void ShowQuestFailedBox(string message)
	{
		ConfirmationBox unityConfirmationBox = UnityConfirmationBox;
		unityConfirmationBox.Message = message;
		Instance.PushDialog(unityConfirmationBox);
	}

	public void ShowInputBox(InputBox.AcceptFunction onAccept, string text, string initialText, bool multiline, bool readOnly, bool needsSession = false)
	{
		InputBox inputBox = (multiline ? UnityInputBoxMultiline : UnityInputBox);
		inputBox.Title = text;
		inputBox.InitialText = initialText;
		inputBox.OnAccept = onAccept;
		inputBox.ReadOnly = readOnly;
		inputBox.NeedSession = needsSession;
		Instance.PushDialog(inputBox);
	}

	public void ShowDifficultyDialogBox(DifficultyDialog.AcceptFunction onAccept, CharacterCreationSettings settings)
	{
		DifficultyDialog unityDifficultyDialog = UnityDifficultyDialog;
		unityDifficultyDialog.OnAccept = onAccept;
		unityDifficultyDialog.Settings = settings;
		Instance.PushDialog(unityDifficultyDialog);
	}

	public void ShowAllowJoinDialogBox(AllowJoinDialog.AcceptFunction onAccept)
	{
		AllowJoinDialog unityAllowJoinDialog = UnityAllowJoinDialog;
		unityAllowJoinDialog.OnAccept = onAccept;
		Instance.PushDialog(unityAllowJoinDialog);
	}

	public void ShowEquipmentTransferAmountBox(TileObject carrier, Equipment item, TileObject to, int minTransferrable, int maxTransferrable, EquipmentTransferAmountBox.Mode mode)
	{
		EquipmentTransferAmountBox unityEquipmentTransferAmountBox = UnityEquipmentTransferAmountBox;
		unityEquipmentTransferAmountBox.Carrier = carrier;
		unityEquipmentTransferAmountBox.Transferring = item;
		unityEquipmentTransferAmountBox.To = to;
		unityEquipmentTransferAmountBox.MinTransferrable = minTransferrable;
		unityEquipmentTransferAmountBox.MaxTransferrable = maxTransferrable;
		unityEquipmentTransferAmountBox.CurrentMode = mode;
		unityEquipmentTransferAmountBox.Amount = maxTransferrable;
		Instance.PushDialog(unityEquipmentTransferAmountBox);
	}

	public void ShowCraftAmountBox(Recipe recipe, Character crafter, TerrainCoord destTile, Equipment usingItem)
	{
		CraftAmountBox unityCraftAmountBox = UnityCraftAmountBox;
		unityCraftAmountBox.Recipe = recipe;
		unityCraftAmountBox.Crafter = crafter;
		unityCraftAmountBox.DestTile = destTile;
		unityCraftAmountBox.UsingItem = usingItem;
		Instance.PushDialog(unityCraftAmountBox);
	}

	public void ShowCraftLimitBox(Community community, EquipmentPrototype proto, LiquidPrototype liquid, int currentLimit)
	{
		CraftLimitBox unityCraftLimitBox = UnityCraftLimitBox;
		unityCraftLimitBox.Community = community;
		unityCraftLimitBox.Proto = proto;
		unityCraftLimitBox.Liquid = liquid;
		unityCraftLimitBox.RecurringLimit = currentLimit;
		Instance.PushDialog(unityCraftLimitBox);
	}

	public void ShowGatePolicyDialog(Gate gate)
	{
		GatePolicyDialog unityGatePolicyDialog = UnityGatePolicyDialog;
		unityGatePolicyDialog.Gate = gate;
		unityGatePolicyDialog.ApplyToAll = !gate.OverrideDefaultGatePolicy;
		Instance.PushDialog(unityGatePolicyDialog);
	}

	public void ShowStoragePolicyDialog(Prop prop)
	{
		StoragePolicyDialog unityStoragePolicyDialog = UnityStoragePolicyDialog;
		unityStoragePolicyDialog.SetProp(prop);
		Instance.PushDialog(unityStoragePolicyDialog);
	}

	public void ShowDesignateLiquidDialog(TileObject carrier, Equipment item)
	{
		DesignatedLiquidDialog unityDesignatedLiquidDialog = UnityDesignatedLiquidDialog;
		unityDesignatedLiquidDialog.Carrier = carrier;
		unityDesignatedLiquidDialog.Item = item;
		Instance.PushDialog(unityDesignatedLiquidDialog);
	}

	public void ShowEquipmentPolicyDialog(Character character, EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		EquipmentPolicyDialog unityEquipmentPolicyDialog = UnityEquipmentPolicyDialog;
		unityEquipmentPolicyDialog.Proto = proto;
		unityEquipmentPolicyDialog.Liquid = liquid;
		unityEquipmentPolicyDialog.InfectionType = infectionType;
		unityEquipmentPolicyDialog.Character = character;
		unityEquipmentPolicyDialog.CharacterPolicy = character.GetEquipmentPolicyMask(proto, liquid, infectionType, includeCommunityPolicy: false, out unityEquipmentPolicyDialog.CharacterSameAsCommunity);
		unityEquipmentPolicyDialog.CharacterTargetAmount = character.GetTargetAmountToCarry(proto, liquid, infectionType, includeCommunityPolicy: false);
		unityEquipmentPolicyDialog.CommunityPolicy = ((character.Community != null) ? character.Community.GetEquipmentPolicyMask(proto, liquid, infectionType) : 0);
		unityEquipmentPolicyDialog.CommunityTargetAmount = ((character.Community != null) ? character.Community.GetTargetAmountToCarry(proto, liquid, infectionType) : 0f);
		Instance.PushDialog(unityEquipmentPolicyDialog);
	}

	public void PushDialog(BaseDialog dialog)
	{
		if (!Util.AmIOnMainThread())
		{
			QueuedDialogs.Add(dialog);
			return;
		}
		if (DialogStack.Count > 0)
		{
			DialogStack[DialogStack.Count - 1].OnHidden();
		}
		DialogStack.Add(dialog);
		dialog.OnActivate();
		SoundManager.PlayMenuSound(SoundManager.DialogOpenSound);
	}

	public void PopDialog()
	{
		if (DialogStack.Count > 0)
		{
			DialogStack[DialogStack.Count - 1].OnDeactivate();
			DialogStack.RemoveAt(DialogStack.Count - 1);
		}
		if (DialogStack.Count > 0)
		{
			DialogStack[DialogStack.Count - 1].OnShown();
		}
	}

	public void ClearDialogStack()
	{
		while (IsDialogOpen())
		{
			PopDialog();
		}
	}

	public bool IsDialogOpen()
	{
		return DialogStack.Count > 0;
	}

	public BaseDialog GetDialog()
	{
		if (DialogStack.Count <= 0)
		{
			return null;
		}
		return DialogStack[DialogStack.Count - 1];
	}

	public bool HasAnyDialogsThatNeedSession()
	{
		foreach (BaseDialog item in DialogStack)
		{
			if (item.NeedsSession())
			{
				return true;
			}
		}
		return false;
	}

	public byte[] CreateScreenshot()
	{
		Texture2D texture2D = new Texture2D(UnityScreenshotRenderTexture.width, UnityScreenshotRenderTexture.height, TextureFormat.RGB24, mipChain: false);
		bool activeSelf = UnityHudBehaviour.gameObject.activeSelf;
		int siblingIndex = UnityHudBehaviour.UnityActionMenu.transform.GetSiblingIndex();
		UnityHudBehaviour.gameObject.SetActive(value: true);
		UnityHudBehaviour.gameObject.transform.SetParent(UnityScreenshotCanvasObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnitySpeechMenu.transform.SetParent(UnityScreenshotCanvasObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnityActionMenu.transform.SetParent(UnityScreenshotCanvasObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnityActionMenuLeftAligned.transform.SetParent(UnityScreenshotCanvasObj.transform, worldPositionStays: false);
		Color color = UnityHudBehaviour.UnityMainImage.color;
		UnityHudBehaviour.UnityMainImage.color = Color.white;
		UnityHudBehaviour.UnityNotification.SetActive(value: false);
		UnityHudBehaviour.UnityNewNotifications.SetActive(value: false);
		bool activeSelf2 = UnityHudBehaviour.UnityNotification.activeSelf;
		bool activeSelf3 = UnityHudBehaviour.UnityNewNotifications.activeSelf;
		UnityScreenshotCameraObj.SetActive(value: true);
		RenderTexture.active = UnityScreenshotRenderTexture;
		UnityScreenshotCameraObj.GetComponent<Camera>().Render();
		texture2D.ReadPixels(new Rect(0f, 0f, UnityScreenshotRenderTexture.width, UnityScreenshotRenderTexture.height), 0, 0);
		texture2D.Apply();
		UnityScreenshotCameraObj.SetActive(value: false);
		UnityHudBehaviour.gameObject.transform.SetParent(UnityLauncherObj.transform, worldPositionStays: false);
		UnityHudBehaviour.gameObject.transform.SetAsFirstSibling();
		UnityHudBehaviour.gameObject.SetActive(activeSelf);
		UnityHudBehaviour.UnityMainImage.color = color;
		UnityHudBehaviour.UnityNotification.SetActive(activeSelf2);
		UnityHudBehaviour.UnityNewNotifications.SetActive(activeSelf3);
		UnityHudBehaviour.UnityActionMenu.transform.SetParent(UnityLauncherObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnityActionMenu.transform.SetSiblingIndex(siblingIndex);
		UnityHudBehaviour.UnityActionMenuLeftAligned.transform.SetParent(UnityLauncherObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnityActionMenuLeftAligned.transform.SetSiblingIndex(siblingIndex + 1);
		UnityHudBehaviour.UnitySpeechMenu.transform.SetParent(UnityLauncherObj.transform, worldPositionStays: false);
		UnityHudBehaviour.UnitySpeechMenu.transform.SetSiblingIndex(siblingIndex + 2);
		RenderTexture.active = null;
		byte[] result = texture2D.EncodeToJPG();
		UnityEngine.Object.Destroy(texture2D);
		return result;
	}

	public GameState GetState()
	{
		return State;
	}

	public TimeSpan GetTime()
	{
		return TimeSpan.FromSeconds(UnscaledTime);
	}

	public void NewGame(CharacterCreationSettings characterSettings, List<StorySource> storySources, bool forceProcedurallyGenerated = false, int forceGameUniqueId = 0)
	{
		SoundManager.PlayMenuSound(SoundManager.StartSaveGameSound);
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: false, 0, storySources, default(MD5Hash));
		if (forceGameUniqueId != 0)
		{
			Session.GameUniqueId = forceGameUniqueId;
		}
		StartDownloadingStories();
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.NewGame;
		LoadingSessionInfo.CharacterSettings = characterSettings;
		LoadingSessionInfo.ForceProcedurallyGenerated = forceProcedurallyGenerated;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
	}

	public List<StorySource> GetCurrentStorySources()
	{
		List<StorySource> list = new List<StorySource>();
		foreach (Story currentStory in CurrentStories)
		{
			list.Add(currentStory.StorySource);
		}
		return list;
	}

	public void GetCurrentActiveMods(List<StorySource> storySources)
	{
		storySources.Clear();
		bool flag = false;
		foreach (Story currentStory in Instance.CurrentStories)
		{
			if (currentStory.Settings.IsMod)
			{
				if (flag)
				{
					storySources.Add(currentStory.StorySource);
				}
			}
			else
			{
				flag = true;
			}
		}
	}

	public bool IsInCurrentStorySources(StorySource storySource)
	{
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.StorySource.Equals(storySource))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInCurrentStories(StoryId storyId)
	{
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.StorySource.HasStoryId(storyId))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCurrentStoryListEqualTo(List<string> storyNames, bool ignoreMods = false)
	{
		if (ignoreMods)
		{
			if (CurrentStories.Count < storyNames.Count)
			{
				return false;
			}
		}
		else if (CurrentStories.Count != storyNames.Count)
		{
			return false;
		}
		int num = Math.Min(CurrentStories.Count, storyNames.Count);
		for (int i = 0; i < num; i++)
		{
			if (!CurrentStories[i].StorySource.Equals(StorySource.FromFolder(storyNames[i])))
			{
				return false;
			}
		}
		return true;
	}

	public MapSize GetMaxMapSizeForCurrentStories()
	{
		MapSize mapSize = MapSize.Tiny;
		foreach (Story currentStory in Instance.CurrentStories)
		{
			if (currentStory.Settings != null)
			{
				mapSize = (MapSize)Math.Max((int)mapSize, (int)currentStory.Settings.MapSize);
			}
		}
		return mapSize;
	}

	public void NewGame(CharacterCreationSettings characterSettings)
	{
		NewGame(characterSettings, GetCurrentStorySources());
	}

	public void RunEditor(CharacterCreationSettings characterSettings, List<StorySource> storySources, bool forceProcedurallyGenerated = false)
	{
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: true, 0, storySources, default(MD5Hash));
		StartDownloadingStories();
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.NewGame;
		LoadingSessionInfo.CharacterSettings = characterSettings;
		LoadingSessionInfo.ForceProcedurallyGenerated = forceProcedurallyGenerated;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
	}

	public void RunEditor()
	{
		RunEditor(null, GetCurrentStorySources());
	}

	public bool LoadGameFromPathForTextDump(string path)
	{
		List<StorySource> list = new List<StorySource>();
		MD5Hash terrainHash = default(MD5Hash);
		using (Stream stream = File.OpenRead(path))
		{
			using CustomBinaryReader customBinaryReader = new CustomBinaryReader(stream);
			customBinaryReader.Add(ref customBinaryReader.Version);
			if (customBinaryReader.Version < 240)
			{
				string value = "";
				ulong value2 = 0uL;
				customBinaryReader.AddAfter(ref value, 131);
				customBinaryReader.AddAfter(ref value2, 213);
				list.Add(StorySource.FromFolder("BaseStory"));
				list.Add(StorySource.FromWorkshopItemOrFolder(value2, value));
			}
			else
			{
				int value3 = 0;
				customBinaryReader.Add(ref value3);
				for (int i = 0; i < value3; i++)
				{
					string value4 = "";
					ulong value5 = 0uL;
					customBinaryReader.Add(ref value4);
					customBinaryReader.Add(ref value5);
					list.Add(StorySource.FromWorkshopItemOrFolder(value5, value4));
				}
			}
			if (customBinaryReader.Version >= 132)
			{
				terrainHash.Reflect(customBinaryReader);
			}
		}
		if (list.Count == 0)
		{
			ShowMessageBox(Translate("MENU_InvalidStory"));
			return false;
		}
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: false, 0, list, terrainHash);
		StartDownloadingStories();
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.TextDump;
		LoadingSessionInfo.LoadingPath = path;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
		return true;
	}

	public void LoadGame(SaveGame saveGame)
	{
		List<StorySource> storySources = saveGame.GetStorySources();
		if (storySources.Count == 0)
		{
			Debug.Log("No story specified");
			return;
		}
		SoundManager.PlayMenuSound(SoundManager.StartSaveGameSound);
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: false, 0, storySources, default(MD5Hash));
		StartDownloadingStories();
		if (OnlineParty.GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
		{
			foreach (PartyMember partyMember in OnlineParty.PartyMembers)
			{
				if (!partyMember.IsBannedOrIgnored())
				{
					Session.NetworkPlayerIDs.Add(partyMember.PlayerID);
					Session.NetworkPlayerNames.Add(partyMember.GetPlayerName());
				}
			}
		}
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.SavedGame;
		LoadingSessionInfo.SaveGame = saveGame;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
	}

	public void LoadLatestSaveGame(int wantLoadGameUniqueId)
	{
		SoundManager.PlayMenuSound(SoundManager.StartSaveGameSound);
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: false, 0, new List<StorySource>(), default(MD5Hash));
		if (OnlineParty.GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
		{
			foreach (PartyMember partyMember in OnlineParty.PartyMembers)
			{
				if (!partyMember.IsBannedOrIgnored())
				{
					Session.NetworkPlayerIDs.Add(partyMember.PlayerID);
					Session.NetworkPlayerNames.Add(partyMember.GetPlayerName());
				}
			}
		}
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.LatestSavedGame;
		LoadingSessionInfo.WantLoadGameUniqueId = wantLoadGameUniqueId;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
	}

	public void LoadNetworkGame(int sessionId, List<StorySource> storySources, byte[] bytes, int bytesLen, MD5Hash terrainHash, List<PlayerID> playerIDs, List<string> playerNames)
	{
		SoundManager.PlayMenuSound(SoundManager.StartSaveGameSound);
		SetState(GameState.LoadingSession);
		TakeOutTheTrash();
		Session = new Session(editor: false, sessionId, storySources, terrainHash);
		StartDownloadingStories();
		playerIDs.CopyToList(Session.NetworkPlayerIDs);
		playerNames.CopyToList(Session.NetworkPlayerNames);
		LoadingSessionInfo = default(LoadSessionInfo);
		LoadingSessionInfo.LoadType = LoadSessionInfo.Type.NetworkSession;
		LoadingSessionInfo.Bytes = bytes;
		LoadingSessionInfo.Len = bytesLen;
		LoadingSessionThread = new Thread(LoadSessionThreaded);
		LoadingSessionThread.Start();
	}

	private void StartDownloadingStories()
	{
		DownloadingStories = DownloadingStoriesState.Ready;
		WorkshopManager.Instance.ClearSubscribeErrors();
		foreach (StorySource storySource in Session.StorySources)
		{
			if (!storySource.IsValid())
			{
				DownloadingStories = DownloadingStoriesState.NeedsDownload;
			}
		}
	}

	public void StartReceivingNetworkSession(bool showLoadingScreen)
	{
		if (State != GameState.ReceivingNetworkSession)
		{
			SetState(GameState.ReceivingNetworkSession);
		}
		if (showLoadingScreen)
		{
			SetMenu(GetMenuBehaviourByPanelName("LoadingPanel"));
		}
	}

	public void QuitGame()
	{
		SetState(GameState.TitleMenu);
		TakeOutTheTrash();
	}

	public static void TakeOutTheTrash()
	{
		Resources.UnloadUnusedAssets();
		GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
		GC.Collect(int.MaxValue, GCCollectionMode.Forced, blocking: true, compacting: true);
		GC.Collect();
	}

	public void OnKicked(string msg)
	{
		QuitGame();
		ShowMessageBox(msg);
	}

	public bool IsInLoadedSession()
	{
		return State == GameState.RunningSession;
	}

	public bool IsDemo()
	{
		return false;
	}

	public bool IsMultiplayerEnabled()
	{
		return !IsDemo();
	}

	public bool IsWorkshopEnabled()
	{
		return true;
	}

	public bool WantShowAllowJoinDialog()
	{
		if (!IsMultiplayerEnabled())
		{
			return false;
		}
		if (Settings.ShowAllowJoinDialog)
		{
			return IsOnline();
		}
		return false;
	}

	public bool IsOnline()
	{
		if (SteamInitialized)
		{
			return SteamUser.BLoggedOn();
		}
		return false;
	}

	public bool IsSteamDeck()
	{
		if (SteamInitialized)
		{
			return SteamUtils.IsSteamRunningOnSteamDeck();
		}
		return false;
	}

	public string GetLocalPlayerName()
	{
		return string.Copy(SteamFriends.GetPersonaName());
	}

	public void SetAllowJoinMode(AllowJoinMode allowJoinMode)
	{
		Settings.AllowJoinMode = allowJoinMode;
		if (OnlineParty.CurrentState == OnlineParty.State.InLobbyAsLeader)
		{
			ELobbyType eLobbyType = ELobbyType.k_ELobbyTypePrivate;
			switch (allowJoinMode)
			{
			case AllowJoinMode.AllowAnyoneToJoin:
				eLobbyType = ELobbyType.k_ELobbyTypePublic;
				break;
			case AllowJoinMode.AllowFriendsToJoin:
				eLobbyType = ELobbyType.k_ELobbyTypeFriendsOnly;
				break;
			case AllowJoinMode.DontAllowAnyoneToJoin:
				eLobbyType = ELobbyType.k_ELobbyTypePrivate;
				break;
			}
			SteamMatchmaking.SetLobbyType(OnlineParty.CurrentLobby.LobbyID.SteamID, eLobbyType);
		}
	}

	public void SetMaxPartySize(int maxPartySize)
	{
		Settings.MaxPartySize = maxPartySize;
		if (OnlineParty.CurrentState == OnlineParty.State.InLobbyAsLeader)
		{
			SteamMatchmaking.SetLobbyMemberLimit(OnlineParty.CurrentLobby.LobbyID.SteamID, Settings.MaxPartySize);
		}
	}

	public void BanPlayer(PlayerID playerID)
	{
		if (Settings.BannedPlayers == null || !Settings.BannedPlayers.Contains(playerID))
		{
			if (Settings.BannedPlayers == null)
			{
				Settings.BannedPlayers = new List<PlayerID>();
			}
			Settings.BannedPlayers.Add(playerID);
		}
	}

	public void ResetBannedPlayers()
	{
		if (Settings.BannedPlayers != null)
		{
			Settings.BannedPlayers.Clear();
		}
	}

	public void OnPartyChanged()
	{
		if (Menu != null)
		{
			Menu.OnPartyChanged();
		}
	}

	public void OnLobbyListSearchFinished()
	{
		if (Menu != null)
		{
			Menu.OnLobbyListSearchFinished();
		}
	}

	public void OnJoinLobbyFailed(string errorMsg)
	{
		if (Menu != null)
		{
			Menu.OnJoinLobbyFailed(errorMsg);
		}
		Instance.ShowMessageBox(errorMsg);
	}

	public void OnWorkshopItemQueryFinished()
	{
		if (Menu != null)
		{
			Menu.OnWorkshopItemQueryFinished();
		}
	}

	public void OnSearchStoreFinished(bool mods, List<StorySource> results, int total)
	{
		if (Menu != null)
		{
			Menu.OnSearchStoreFinished(mods, results, total);
		}
	}

	public SettingsData CreateSettingsDataToSave()
	{
		SettingsData settingsData = Settings.Copy();
		settingsData.Version = 14;
		if (settingsData.BannedPlayers != null)
		{
			settingsData.BannedPlayerIDs = new List<ulong>();
			for (int i = 0; i < settingsData.BannedPlayers.Count; i++)
			{
				settingsData.BannedPlayerIDs.Add(settingsData.BannedPlayers[i].SteamID.m_SteamID);
			}
			settingsData.BannedPlayers.Clear();
		}
		return settingsData;
	}

	public void LoadSettingsData(SettingsData data)
	{
		Settings = data;
		Settings.BannedPlayers = new List<PlayerID>();
		if (Settings.BannedPlayerIDs != null)
		{
			for (int i = 0; i < Settings.BannedPlayerIDs.Count; i++)
			{
				Settings.BannedPlayers.Add(new PlayerID(new CSteamID(Settings.BannedPlayerIDs[i])));
			}
			Settings.BannedPlayerIDs.Clear();
		}
		if (Settings.OverriddenControls == null)
		{
			return;
		}
		foreach (MappedInput overriddenControl in Settings.OverriddenControls)
		{
			if (overriddenControl.InputFunction == InputFunction.Invalid)
			{
				continue;
			}
			if (!overriddenControl.CanPositiveButtonBeCleared())
			{
				if (overriddenControl.PositiveKey == KeyCode.None)
				{
					overriddenControl.PositiveKey = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].PositiveKey;
				}
				if (overriddenControl.PositiveXBoxButton == XBoxButton.None)
				{
					overriddenControl.PositiveXBoxButton = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].PositiveXBoxButton;
				}
				if (overriddenControl.PositivePS4Button == PS4Button.None)
				{
					overriddenControl.PositivePS4Button = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].PositivePS4Button;
				}
				if (overriddenControl.PositiveSteamDeckButton == SteamDeckButton.None)
				{
					overriddenControl.PositiveSteamDeckButton = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].PositiveSteamDeckButton;
				}
			}
			if (!overriddenControl.CanNegativeButtonBeCleared())
			{
				if (overriddenControl.NegativeKey == KeyCode.None)
				{
					overriddenControl.NegativeKey = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].NegativeKey;
				}
				if (overriddenControl.NegativeXBoxButton == XBoxButton.None)
				{
					overriddenControl.NegativeXBoxButton = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].NegativeXBoxButton;
				}
				if (overriddenControl.NegativePS4Button == PS4Button.None)
				{
					overriddenControl.NegativePS4Button = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].NegativePS4Button;
				}
				if (overriddenControl.NegativeSteamDeckButton == SteamDeckButton.None)
				{
					overriddenControl.NegativeSteamDeckButton = InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction].NegativeSteamDeckButton;
				}
			}
			InputFunctionManager.Instance.InputMappings[(int)overriddenControl.InputFunction] = overriddenControl;
		}
	}

	public void AutoSaveSettings()
	{
		SaveRequest saveRequest = new SaveRequest();
		saveRequest.SettingsData = CreateSettingsDataToSave();
		SaveGameManager.PushSaveRequest(saveRequest);
	}

	public static string Translate(int hash)
	{
		if (!Instance.TryTranslate(hash, out var result, englishOnly: false))
		{
			return "";
		}
		return result;
	}

	public static string TranslateUIOnly(int hash)
	{
		if (!Instance.TryTranslateUIOnly(hash, out var result, englishOnly: false))
		{
			return "";
		}
		return result;
	}

	public static string Translate(int hash, int femaleHash, GenderType gender, bool englishOnly = false)
	{
		if (gender == GenderType.Female && Instance.TryTranslate(femaleHash, out var result, englishOnly))
		{
			return result;
		}
		if (!Instance.TryTranslate(hash, out result, englishOnly))
		{
			return "";
		}
		return result;
	}

	public static string Translate(int hash, int femaleHash, int pluralHash, int femalePluralHash, GenderType gender, bool plural, bool englishOnly = false)
	{
		string result;
		if (plural)
		{
			if (gender == GenderType.Female && Instance.TryTranslate(femalePluralHash, out result, englishOnly))
			{
				return result;
			}
			if (Instance.TryTranslate(pluralHash, out result, englishOnly))
			{
				return result;
			}
		}
		if (gender == GenderType.Female && Instance.TryTranslate(femaleHash, out result, englishOnly))
		{
			return result;
		}
		if (!Instance.TryTranslate(hash, out result, englishOnly))
		{
			return "";
		}
		return result;
	}

	public static bool HasTranslationForHash(int hash, bool englishOnly)
	{
		string value;
		if (englishOnly || Instance.Settings.Language == Language.English)
		{
			if (Instance.UIEnglishTranslation != null && Instance.UIEnglishTranslation.Keys.TryGetValue(hash, out value))
			{
				return true;
			}
		}
		else if (Instance.UIForeignTranslation != null && Instance.UIForeignTranslation.Keys.TryGetValue(hash, out value))
		{
			return true;
		}
		return false;
	}

	public static string Translate(string key)
	{
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out var result, englishOnly: false))
		{
			return key;
		}
		return result;
	}

	public static string TranslateUIOnly(string key)
	{
		if (!Instance.TryTranslateUIOnly(StringUtil.JenkinsHash(key), out var result, englishOnly: false))
		{
			return key;
		}
		return result;
	}

	public static string Translate(string key, string femaleKey, GenderType gender, bool englishOnly = false)
	{
		if (gender == GenderType.Female && Instance.TryTranslate(StringUtil.JenkinsHash(femaleKey), out var result, englishOnly))
		{
			return result;
		}
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out result, englishOnly))
		{
			return "";
		}
		return result;
	}

	public static string Translate(string key, string femaleKey, string pluralKey, string femalePluralKey, GenderType gender, bool plural, bool englishOnly = false)
	{
		string result;
		if (plural)
		{
			if (gender == GenderType.Female && Instance.TryTranslate(StringUtil.JenkinsHash(femalePluralKey), out result, englishOnly))
			{
				return result;
			}
			if (Instance.TryTranslate(StringUtil.JenkinsHash(pluralKey), out result, englishOnly))
			{
				return result;
			}
		}
		if (gender == GenderType.Female && Instance.TryTranslate(StringUtil.JenkinsHash(femaleKey), out result, englishOnly))
		{
			return result;
		}
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out result, englishOnly))
		{
			return "";
		}
		return result;
	}

	public static string Translate(string key, GenderType gender, bool plural, bool englishOnly = false)
	{
		return Translate(key, key + "_Female", key + "_Plural", key + "_Female_Plural", gender, plural, englishOnly);
	}

	public static string TranslateName(string key)
	{
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out var result, englishOnly: false) || string.IsNullOrEmpty(result))
		{
			return key;
		}
		return result;
	}

	public static string TranslateSurname(string key, GenderType gender)
	{
		if (gender != GenderType.Female || !Instance.TryTranslate(StringUtil.JenkinsHash(key, 'F'), out var result, englishOnly: false) || string.IsNullOrEmpty(result))
		{
			return TranslateName(key);
		}
		return result;
	}

	public static string Translate(int hash, bool englishOnly)
	{
		if (!Instance.TryTranslate(hash, out var result, englishOnly))
		{
			return "";
		}
		return result;
	}

	public static string Translate(string key, bool englishOnly)
	{
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out var result, englishOnly))
		{
			return key;
		}
		return result;
	}

	public static bool WantCensoredString(bool englishOnly, StringStatus verified)
	{
		return false;
	}

	public static string TranslateName(string key, bool englishOnly, StringStatus verified)
	{
		if (!Instance.TryTranslate(StringUtil.JenkinsHash(key), out var result, englishOnly) || string.IsNullOrEmpty(result))
		{
			return key;
		}
		return result;
	}

	public static string TranslateSurname(string key, GenderType gender, bool englishOnly, StringStatus verified)
	{
		if (gender != GenderType.Female || !Instance.TryTranslate(StringUtil.JenkinsHash(key, 'F'), out var result, englishOnly) || string.IsNullOrEmpty(result))
		{
			return TranslateName(key, englishOnly, verified);
		}
		return result;
	}

	public static bool WantNamesReversed(string surnameUntranslated, bool englishOnly)
	{
		Language language = Instance.Settings.Language;
		if ((language == Language.TraditionalChinese || language == Language.SimplifiedChinese) && !englishOnly && !string.IsNullOrEmpty(surnameUntranslated) && surnameUntranslated[0] > 'ÿ')
		{
			return true;
		}
		return false;
	}

	public bool TryTranslate(int hash, out string result, bool englishOnly)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			if (CurrentStories[num].TryTranslate(hash, out result, englishOnly))
			{
				return true;
			}
		}
		if (!englishOnly && UIForeignTranslation != null && UIForeignTranslation.Keys.TryGetValue(hash, out result))
		{
			return true;
		}
		if (UIEnglishTranslation != null && UIEnglishTranslation.Keys.TryGetValue(hash, out result))
		{
			return true;
		}
		result = null;
		return false;
	}

	public bool TryTranslateUIOnly(int hash, out string result, bool englishOnly)
	{
		if (!englishOnly && UIForeignTranslation != null && UIForeignTranslation.Keys.TryGetValue(hash, out result))
		{
			return true;
		}
		if (UIEnglishTranslation != null && UIEnglishTranslation.Keys.TryGetValue(hash, out result))
		{
			return true;
		}
		result = null;
		return false;
	}

	public bool IsLanguageSupported(Language language)
	{
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.IsLanguageSupported(language))
			{
				return true;
			}
		}
		if (File.Exists(StreamingAssetsPath + "/UI/" + language.ToString() + ".tsv"))
		{
			return true;
		}
		return false;
	}

	private static string GetFallbackFontNameForLanguage(Language language)
	{
		switch (language)
		{
		case Language.Japanese:
			return "NotoSansJP-Medium SDF";
		case Language.Korean:
			return "NotoSansKR-Medium SDF";
		case Language.SimplifiedChinese:
			return "NotoSansSC-Medium SDF";
		case Language.TraditionalChinese:
			return "NotoSansTC-Medium SDF";
		case Language.Thai:
			return "NotoSansThai-Black SDF";
		case Language.Persian:
			return "NotoSansArabic-Black SDF";
		default:
			if (WantLuckiestGuyFont(language))
			{
				return "PoetsenOne-Regular SDF";
			}
			if (WantMitrFont(language))
			{
				return "Mitr-SemiBold";
			}
			return null;
		}
	}

	private static TMP_FontAsset GetFallbackFontForLanugage(string fontName)
	{
		foreach (TMP_FontAsset fallbackFontAsset in TMP_Settings.fallbackFontAssets)
		{
			if (fallbackFontAsset.name == fontName)
			{
				return fallbackFontAsset;
			}
		}
		return null;
	}

	public void OnLanguageChanged()
	{
		string fallbackFontNameForLanguage = GetFallbackFontNameForLanguage(Settings.Language);
		if (!string.IsNullOrEmpty(fallbackFontNameForLanguage))
		{
			TMP_FontAsset fallbackFontForLanugage = GetFallbackFontForLanugage(fallbackFontNameForLanguage);
			if (fallbackFontForLanugage != null)
			{
				TMP_Settings.fallbackFontAssets.Remove(fallbackFontForLanugage);
				TMP_Settings.fallbackFontAssets.Insert(0, fallbackFontForLanugage);
			}
		}
		SetFontEnabled(KomikaAxis, !WantLuckiestGuyFont(Settings.Language) && !WantMitrFont(Settings.Language));
		SetFontEnabled(Komika, !WantLuckiestGuyFont(Settings.Language) && !WantMitrFont(Settings.Language));
		SetFontEnabled(KomikaKaps, !WantLuckiestGuyFont(Settings.Language) && !WantMitrFont(Settings.Language) && !WantCyrillicFont(Settings.Language));
		if (WantLuckiestGuyFont(Settings.Language))
		{
			KomikaAxis.fallbackFontAssetTable.Clear();
			KomikaAxis.fallbackFontAssetTable.Add(LuckiestGuy);
			Komika.fallbackFontAssetTable.Clear();
			Komika.fallbackFontAssetTable.Add(PoetsenOne);
			KomikaKaps.fallbackFontAssetTable.Clear();
			KomikaKaps.fallbackFontAssetTable.Add(PoetsenOne);
			KomikaKaps.fallbackFontAssetTable.Add(Komika);
		}
		else if (WantMitrFont(Settings.Language))
		{
			KomikaAxis.fallbackFontAssetTable.Clear();
			KomikaAxis.fallbackFontAssetTable.Add(Mitr);
			Komika.fallbackFontAssetTable.Clear();
			Komika.fallbackFontAssetTable.Add(Mitr);
			KomikaKaps.fallbackFontAssetTable.Clear();
			KomikaKaps.fallbackFontAssetTable.Add(Mitr);
			KomikaKaps.fallbackFontAssetTable.Add(Komika);
		}
		else
		{
			KomikaAxis.fallbackFontAssetTable.Clear();
			Komika.fallbackFontAssetTable.Clear();
			KomikaKaps.fallbackFontAssetTable.Clear();
			KomikaKaps.fallbackFontAssetTable.Add(Komika);
		}
		KomikaAxis.ReadFontAssetDefinition();
		Komika.ReadFontAssetDefinition();
		KomikaKaps.ReadFontAssetDefinition();
		LuckiestGuy.ReadFontAssetDefinition();
		PoetsenOne.ReadFontAssetDefinition();
		Mitr.ReadFontAssetDefinition();
		if (OverrideGUISkin != null)
		{
			UnityEngine.Object.Destroy(OverrideGUISkin);
			OverrideGUISkin = null;
		}
		LastChangedLanguageFrame = Time.frameCount;
	}

	public static bool WantLuckiestGuyFont(Language language)
	{
		if (language != Language.Polish && language != Language.Lithuanian && language != Language.Turkish && language != Language.Czech)
		{
			return language == Language.Hungarian;
		}
		return true;
	}

	public static bool WantMitrFont(Language language)
	{
		return language == Language.Vietnamese;
	}

	public static bool WantCyrillicFont(Language language)
	{
		if (language != Language.Ukrainian)
		{
			return language == Language.Russian;
		}
		return true;
	}

	public static bool IsCJKLanguage(Language language)
	{
		if ((uint)(language - 11) <= 4u || language == Language.Persian)
		{
			return true;
		}
		return false;
	}

	private static void SetFontEnabled(TMP_FontAsset font, bool enabled)
	{
		if (enabled)
		{
			foreach (TMP_Character item in font.characterTable)
			{
				font.characterLookupTable.Remove(item.unicode);
				item.unicode &= 2147483647u;
				font.characterLookupTable[item.unicode] = item;
			}
			return;
		}
		foreach (TMP_Character item2 in font.characterTable)
		{
			font.characterLookupTable.Remove(item2.unicode);
			item2.unicode |= 2147483648u;
			font.characterLookupTable[item2.unicode] = item2;
		}
	}

	public void SetLanguage(Language language)
	{
		foreach (Story currentStory in CurrentStories)
		{
			currentStory.ForeignTranslation = null;
		}
		UIForeignTranslation = null;
		Settings.Language = language;
		OnLanguageChanged();
		if (Settings.Language != Language.English)
		{
			UIForeignTranslation = Translation.LoadFromFile(StreamingAssetsPath + "/UI/" + Settings.Language.ToString() + ".tsv", justTitleAndDescription: false);
			foreach (Story currentStory2 in CurrentStories)
			{
				currentStory2.ForeignTranslation = Translation.LoadFromFile(currentStory2.Path + "/" + Settings.Language.ToString() + ".tsv", justTitleAndDescription: false);
			}
		}
		if (Session != null)
		{
			Session.DebugMenu = new MainDebugMenu();
		}
	}

	public void SetSuperSample(bool v)
	{
		SuperSample = v;
		HudBehaviour.Instance.OnScreenResized();
		PlayerPrefs.SetInt("SuperSample", v ? 1 : 0);
	}

	public void SetTAA(bool v)
	{
		TAA = v;
		HudBehaviour.Instance.OnScreenResized();
		PlayerPrefs.SetInt("TAA", v ? 1 : 0);
	}

	public void SetVSyncCount(int v)
	{
		QualitySettings.vSyncCount = v;
		PlayerPrefs.SetInt("VSyncCount", v);
	}

	public void SetFOV(float v)
	{
		HudBehaviour.Instance.SetFOV(v);
		PlayerPrefs.SetFloat("FOV", v);
	}

	public void SetTreeQuality(float v)
	{
		Settings.TreeQuality = v;
		if (GameTerrain.Instance != null)
		{
			GameTerrain.Instance.SetTreeLODBias(v);
		}
		PlayerPrefs.SetFloat("TreeQuality", v);
	}

	public void SetGrassDensity(float v)
	{
		Settings.GrassDensity = v;
		Shader.SetGlobalFloat(ShaderHash._GrassDensityFactor, Settings.GrassDensity);
		PlayerPrefs.SetFloat("GrassDensity", v);
	}

	public void SetGrassRange(float v)
	{
		HudBehaviour.Instance.UnityGrassCameraBehaviour.GrassRangeMax = v;
		PlayerPrefs.SetFloat("GrassRange", v);
	}

	public Story FindStoryThatOwnsEquipmentPrototype(EquipmentPrototype proto)
	{
		foreach (Story currentStory in CurrentStories)
		{
			foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in currentStory.EquipmentPrototypes)
			{
				if (equipmentPrototype.Value == proto)
				{
					return currentStory;
				}
			}
		}
		return null;
	}

	public EquipmentPrototype FindEquipmentPrototypeByName(string name)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			EquipmentPrototype equipmentPrototype = CurrentStories[num].FindEquipmentPrototypeByName(name);
			if (equipmentPrototype != null)
			{
				return equipmentPrototype;
			}
		}
		return null;
	}

	public LiquidPrototype FindLiquidPrototypeByName(string name)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			LiquidPrototype liquidPrototype = CurrentStories[num].FindLiquidPrototypeByName(name);
			if (liquidPrototype != null)
			{
				return liquidPrototype;
			}
		}
		return null;
	}

	public PropPrototype FindPropPrototypeByName(string name)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			PropPrototype propPrototype = CurrentStories[num].FindPropPrototypeByName(name);
			if (propPrototype != null)
			{
				return propPrototype;
			}
		}
		return null;
	}

	public PropPrototype FindPropPrototypeByNameHash(int nameHash)
	{
		CurrentPropPrototypes.TryGetValue(nameHash, out var value);
		return value;
	}

	public MemoryPrototype FindMemoryPrototypeByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			MemoryPrototype memoryPrototype = CurrentStories[num].FindMemoryPrototypeByUniqueID(uniqueID);
			if (memoryPrototype != null)
			{
				return memoryPrototype;
			}
		}
		return null;
	}

	public Recipe FindRecipeByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Recipe recipe = CurrentStories[num].FindRecipeByUniqueID(uniqueID);
			if (recipe != null)
			{
				return recipe;
			}
		}
		return null;
	}

	public Recipe FindRecipeByProduct(PropPrototype productType, EquipmentPrototype productProto)
	{
		foreach (KeyValuePair<string, Recipe> item in CurrentRecipesDeterministic)
		{
			if (item.Value.ProductPropPrototype == productType && item.Value.ProductPrototype == productProto && !item.Value.Deprecated)
			{
				return item.Value;
			}
		}
		return null;
	}

	public Recipe FindRecipeByProductBaseObjectType(BaseObjectType type)
	{
		foreach (KeyValuePair<string, Recipe> item in CurrentRecipesDeterministic)
		{
			if (item.Value.ProductPropPrototype != null && item.Value.ProductPropPrototype.TypeName == type && !item.Value.Deprecated)
			{
				return item.Value;
			}
			if (item.Value.ProductPrototype != null && item.Value.ProductPrototype.TypeName == type && !item.Value.Deprecated)
			{
				return item.Value;
			}
		}
		return null;
	}

	public bool IsIngredientInAnyForRecipesWhichCanBeRecurring(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		if (proto != null)
		{
			return AllIngredientsForRecipesWhichCanBeRecurring.Contains(proto);
		}
		if (liquid != null)
		{
			return AllLiquidIngredientsForRecipesWhichCanBeRecurring.Contains(liquid);
		}
		return false;
	}

	public Trigger FindTriggerByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Trigger trigger = CurrentStories[num].FindTriggerByUniqueID(uniqueID);
			if (trigger != null)
			{
				return trigger;
			}
		}
		return null;
	}

	public ConditionBlock FindConditionBlockByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			ConditionBlock conditionBlock = CurrentStories[num].FindConditionBlockByUniqueID(uniqueID);
			if (conditionBlock != null)
			{
				return conditionBlock;
			}
		}
		return null;
	}

	public Speech FindSpeechByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Speech speech = CurrentStories[num].FindSpeechByUniqueID(uniqueID);
			if (speech != null)
			{
				return speech;
			}
		}
		return null;
	}

	public Quest FindQuestByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Quest quest = CurrentStories[num].FindQuestByUniqueID(uniqueID);
			if (quest != null)
			{
				return quest;
			}
		}
		return null;
	}

	public QuestGroup FindQuestGroupByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			QuestGroup questGroup = CurrentStories[num].FindQuestGroupByUniqueID(uniqueID);
			if (questGroup != null)
			{
				return questGroup;
			}
		}
		return null;
	}

	public Template FindTemplateByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Template template = CurrentStories[num].FindTemplateByUniqueID(uniqueID);
			if (template != null)
			{
				return template;
			}
		}
		return null;
	}

	public Invader FindInvaderByUniqueID(string uniqueID)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			Invader invader = CurrentStories[num].FindInvaderByUniqueID(uniqueID);
			if (invader != null)
			{
				return invader;
			}
		}
		return null;
	}

	public bool IsNameInAnyNamesList(string name)
	{
		for (int i = 0; i < 3; i++)
		{
			foreach (Story currentStory in CurrentStories)
			{
				if (currentStory.Names[i] != null && currentStory.Names[i].Names.Contains(name))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsNameInNamesList(string name, NameType nameType)
	{
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.Names[(int)nameType] != null && currentStory.Names[(int)nameType].Names.Contains(name))
			{
				return true;
			}
		}
		return false;
	}

	public string PickRandomName(NameType nameType, CustomRandom rand)
	{
		int num = 0;
		foreach (Story currentStory in CurrentStories)
		{
			NamesList namesList = currentStory.Names[(int)nameType];
			if (namesList != null)
			{
				num += namesList.Names.Count;
			}
		}
		int num2 = rand.Next(num);
		foreach (Story currentStory2 in CurrentStories)
		{
			NamesList namesList2 = currentStory2.Names[(int)nameType];
			if (namesList2 != null)
			{
				if (num2 < namesList2.Names.Count)
				{
					return namesList2.Names[num2];
				}
				num2 -= namesList2.Names.Count;
			}
		}
		return "Bob";
	}

	public List<string> GetAllPersonalities()
	{
		List<string> list = new List<string>();
		list.Add("");
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.PersonalitiesList == null)
			{
				continue;
			}
			foreach (PersonalityGroup personalityGroup in currentStory.PersonalitiesList.PersonalityGroups)
			{
				for (int i = 0; i < personalityGroup.Personalities.Count; i++)
				{
					if (!list.Contains(personalityGroup.Personalities[i].Personality))
					{
						list.Add(personalityGroup.Personalities[i].Personality);
					}
				}
			}
		}
		return list;
	}

	public List<PersonalityGroup> GetAllPersonalityGroups(string faction)
	{
		List<PersonalityGroup> list = new List<PersonalityGroup>();
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.PersonalitiesList == null)
			{
				continue;
			}
			foreach (PersonalityGroup personalityGroup in currentStory.PersonalitiesList.PersonalityGroups)
			{
				if (!personalityGroup.IsNormalFaction() && personalityGroup.Faction != faction)
				{
					continue;
				}
				int num = -1;
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Matches(personalityGroup))
					{
						num = i;
						break;
					}
				}
				if (num != -1)
				{
					if (!personalityGroup.IsNormalFaction() || list[num].IsNormalFaction())
					{
						list[num] = personalityGroup;
					}
				}
				else
				{
					list.Add(personalityGroup);
				}
			}
		}
		return list;
	}

	public PersonalityGroup FindPersonalityGroup(string personality, string faction)
	{
		PersonalityGroup personalityGroup = null;
		foreach (Story currentStory in CurrentStories)
		{
			if (currentStory.PersonalitiesList == null)
			{
				continue;
			}
			foreach (PersonalityGroup personalityGroup2 in currentStory.PersonalitiesList.PersonalityGroups)
			{
				if (!personalityGroup2.Contains(personality))
				{
					continue;
				}
				if (personalityGroup2.IsNormalFaction())
				{
					if (personalityGroup != null && !personalityGroup.IsNormalFaction())
					{
						continue;
					}
				}
				else if (personalityGroup2.Faction != faction)
				{
					continue;
				}
				personalityGroup = personalityGroup2;
			}
		}
		return personalityGroup;
	}

	public LootLocationDef FindLootLocation(string lootLocationName)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			if (CurrentStories[num].LootLocationsList != null)
			{
				LootLocationDef lootLocationDef = CurrentStories[num].LootLocationsList.FindLootLocation(lootLocationName);
				if (lootLocationDef != null)
				{
					return lootLocationDef;
				}
			}
		}
		return null;
	}

	public EquipmentPrototype PickRandomItem(string lootLocation, CustomRandom rand, bool includeClothing, int traderLevel)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			LootScarcity lootScarcityForLocation = item.Value.GetLootScarcityForLocation(lootLocation);
			if (lootScarcityForLocation != LootScarcity.None && (item.Value.TypeName != BaseObjectType.SavegameToken || Session.Instance.DifficultySettings.SaveTokensRequired) && (includeClothing || item.Value.ClothingType == ClothingType.Invalid || item.Value.ClothingType == ClothingType.Backpack) && (item.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item.Value) < item.Value.MaxSpawnable))
			{
				ScarcityBuckets[(int)lootScarcityForLocation].Add(item.Value);
			}
		}
		int[] totalProbabilityRatio = TotalProbabilityRatio;
		switch (traderLevel)
		{
		case 1:
			totalProbabilityRatio = TotalProbabilityRatioTraderLevel1;
			break;
		case 2:
			totalProbabilityRatio = TotalProbabilityRatioTraderLevel2;
			break;
		case 3:
			totalProbabilityRatio = TotalProbabilityRatioTraderLevel3;
			break;
		case 4:
			totalProbabilityRatio = TotalProbabilityRatioTraderLevel4;
			break;
		case 5:
			totalProbabilityRatio = TotalProbabilityRatioTraderLevel5;
			break;
		}
		return PickFromScarcityBuckets(totalProbabilityRatio, rand, mustPickSomething: false);
	}

	public EquipmentPrototype PickRandomClothing(string lootLocation, ClothingType clothingType, bool mustPickSomething, CustomRandom rand)
	{
		return PickRandomClothing(lootLocation, clothingType, mustPickSomething, null, rand);
	}

	public EquipmentPrototype PickRandomClothing(string lootLocation, ClothingType clothingType, bool mustPickSomething, Type type, CustomRandom rand)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			LootScarcity lootScarcityForLocation = item.Value.GetLootScarcityForLocation(lootLocation);
			if (lootScarcityForLocation != LootScarcity.None && item.Value.ClothingType == clothingType && (!(type != null) || type.IsInstanceOfType(BaseObjectManager.PrototypeGameObjects[(int)item.Value.TypeName])) && (item.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item.Value) < item.Value.MaxSpawnable))
			{
				ScarcityBuckets[(int)lootScarcityForLocation].Add(item.Value);
			}
		}
		return PickFromScarcityBuckets(TotalProbabilityRatio, rand, mustPickSomething);
	}

	public EquipmentPrototype PickLargeBackpack(CustomRandom rand)
	{
		float num = 0f;
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.ClothingType == ClothingType.Backpack)
			{
				num = Math.Max(num, item.Value.CarryWeight);
			}
		}
		foreach (KeyValuePair<string, EquipmentPrototype> item2 in CurrentEquipmentPrototypesDeterministic)
		{
			if (item2.Value.ClothingType == ClothingType.Backpack && item2.Value.CarryWeight >= num && (item2.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item2.Value) < item2.Value.MaxSpawnable))
			{
				ScarcityBuckets[(int)item2.Value.Scarcity].Add(item2.Value);
			}
		}
		return PickFromScarcityBuckets(TotalProbabilityRatio, rand, mustPickSomething: true);
	}

	public EquipmentPrototype PickRandomFood(CustomRandom rand, Character forCharacter)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.GetNutrition() > EquipmentContainer.MinNutritionToEat && !item.Value.ContainsHumanMeat && forCharacter.CalcTastiness(item.Value) >= EquipmentContainer.MinTastinessToEatUnlessReallyHungry && (item.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item.Value) < item.Value.MaxSpawnable))
			{
				ScarcityBuckets[(int)item.Value.Scarcity].Add(item.Value);
			}
		}
		return PickFromScarcityBuckets(TotalProbabilityRatio, rand, mustPickSomething: true);
	}

	public EquipmentPrototype PickRandomItemOfClass(string lootLocation, Type type, CustomRandom rand, bool mustPickSomething)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			LootScarcity lootScarcityForLocation = item.Value.GetLootScarcityForLocation(lootLocation);
			if (lootScarcityForLocation != LootScarcity.None && item.Value.TypeName != BaseObjectType.Invalid && type.IsInstanceOfType(BaseObjectManager.PrototypeGameObjects[(int)item.Value.TypeName]) && (item.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item.Value) < item.Value.MaxSpawnable))
			{
				ScarcityBuckets[(int)lootScarcityForLocation].Add(item.Value);
			}
		}
		return PickFromScarcityBuckets(TotalProbabilityRatio, rand, mustPickSomething);
	}

	public EquipmentPrototype PickAmmoForWeapon(string lootLocation, EquipmentPrototype weaponProto, CustomRandom rand, bool mustPickSomething)
	{
		foreach (EquipmentPrototype ammoPrototype in weaponProto.AmmoPrototypes)
		{
			LootScarcity lootScarcityForLocation = ammoPrototype.GetLootScarcityForLocation(lootLocation);
			if (lootScarcityForLocation != LootScarcity.None && (ammoPrototype.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(ammoPrototype) < ammoPrototype.MaxSpawnable))
			{
				ScarcityBuckets[(int)lootScarcityForLocation].Add(ammoPrototype);
			}
		}
		return PickFromScarcityBuckets(TotalProbabilityRatio, rand, mustPickSomething);
	}

	public EquipmentPrototype PickRandomItemOfClass(Type type, CustomRandom rand)
	{
		List<EquipmentPrototype> list = new List<EquipmentPrototype>();
		list.Clear();
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.TypeName != BaseObjectType.Invalid && type.IsInstanceOfType(BaseObjectManager.PrototypeGameObjects[(int)item.Value.TypeName]) && (item.Value.MaxSpawnable == 0 || Session.GetNumEquipmentSpawns(item.Value) < item.Value.MaxSpawnable))
			{
				list.Add(item.Value);
			}
		}
		if (list.Count <= 0)
		{
			return null;
		}
		return list[rand.Next(list.Count)];
	}

	public EquipmentPrototype PickFromScarcityBuckets(int[] totalProbabilityRatio, CustomRandom rand, bool mustPickSomething)
	{
		int num = rand.Next(totalProbabilityRatio[0]);
		LootScarcity lootScarcity = LootScarcity.Count;
		int num2 = 0;
		for (int num3 = 5; num3 >= 0; num3--)
		{
			lootScarcity = (LootScarcity)num3;
			num2 += ScarcityBuckets[num3].Count;
			if (num < totalProbabilityRatio[num3] && (!mustPickSomething || num2 != 0))
			{
				break;
			}
		}
		for (; lootScarcity < LootScarcity.Count && ScarcityBuckets[(int)lootScarcity].Count == 0; lootScarcity++)
		{
		}
		List<EquipmentPrototype> list = ((lootScarcity < LootScarcity.Count) ? ScarcityBuckets[(int)lootScarcity] : null);
		EquipmentPrototype result = ((list != null && list.Count > 0) ? list[rand.Next(list.Count)] : null);
		for (int i = 0; i < ScarcityBuckets.Length; i++)
		{
			ScarcityBuckets[i].Clear();
		}
		return result;
	}

	public EquipmentPrototype FindSeedPrototypeForPlantType(PropPrototype cropType)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.GetSeedForPlantType() == cropType)
			{
				return item.Value;
			}
		}
		return null;
	}

	public EquipmentPrototype FindAntigenPrototypeForInfectionType(InfectionType infectionType)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.AntigenType == infectionType)
			{
				return item.Value;
			}
		}
		return null;
	}

	public EquipmentPrototype FindDefaultContainerForLiquidType(LiquidPrototype liquid)
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.DefaultLiquidPrototype == liquid)
			{
				return item.Value;
			}
		}
		return null;
	}

	public Story GetCurrentlyEditingStory()
	{
		if (CurrentStories.Count <= 0)
		{
			return null;
		}
		return CurrentStories[CurrentStories.Count - 1];
	}

	public Story GetStoryThatOwnsPropPrototype(PropPrototype proto)
	{
		for (int num = CurrentStories.Count - 1; num >= 0; num--)
		{
			if (CurrentStories[num].PropPrototypes.ContainsKey(proto.Name))
			{
				return CurrentStories[num];
			}
		}
		return null;
	}

	private void SetCurrentSessionReader(CustomBinaryReader reader)
	{
		lock (this)
		{
			LoadingSessionInfo.CurrentSessionReader = reader;
			if (reader == null)
			{
				LoadingSessionInfo.SessionReaderFinished = true;
			}
			else
			{
				SessionSize = reader.TotalSize;
			}
		}
	}

	public void SetCurrentTerrainReader(CustomBinaryReader reader)
	{
		lock (this)
		{
			LoadingSessionInfo.CurrentTerrainReader = reader;
			if (reader == null)
			{
				LoadingSessionInfo.TerrainReaderFinished = true;
			}
		}
	}

	public void GetReaderProgress(out int progress, out int total, out SaveGameData? data, out bool generating)
	{
		lock (this)
		{
			switch (LoadingSessionInfo.LoadType)
			{
			case LoadSessionInfo.Type.SavedGame:
				if (LoadingSessionInfo.SaveGame != null)
				{
					data = LoadingSessionInfo.SaveGame.Data;
				}
				else
				{
					data = null;
				}
				break;
			case LoadSessionInfo.Type.NewGame:
				data = new SaveGameData
				{
					CommunitySize = 1,
					Day = 1,
					DayOfYear = ((LoadingSessionInfo.CharacterSettings != null) ? LoadingSessionInfo.CharacterSettings.StartDayOfYear : 0)
				};
				break;
			default:
				data = null;
				break;
			}
			if (LoadingSessionInfo.IsGenerating)
			{
				progress = GameTerrain.CurGenerationStep;
				total = GameTerrain.GenerationSteps.Length;
				generating = true;
			}
			else
			{
				progress = ((LoadingSessionInfo.CurrentSessionReader != null) ? LoadingSessionInfo.CurrentSessionReader.Progress : (LoadingSessionInfo.SessionReaderFinished ? SessionSize : 0)) + ((LoadingSessionInfo.CurrentTerrainReader != null) ? LoadingSessionInfo.CurrentTerrainReader.Progress : (LoadingSessionInfo.TerrainReaderFinished ? TerrainSize : 0));
				total = SessionSize + TerrainSize;
				generating = false;
			}
		}
	}
}
