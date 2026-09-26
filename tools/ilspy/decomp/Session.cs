using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

public class Session
{
	public struct SpawnedEquipmentCount : IReflectable, IComparable<SpawnedEquipmentCount>
	{
		public string ProtoName;

		public int Count;

		public void Reflect(Reflector reflector)
		{
			reflector.Add(ref ProtoName);
			reflector.Add(ref Count);
		}

		public int CompareTo(SpawnedEquipmentCount other)
		{
			return string.CompareOrdinal(ProtoName, other.ProtoName);
		}
	}

	public class SnapshotTaskData : BaseTaskData
	{
		public CustomBinaryWriterToMemory Writer;

		public int InputFrame;

		public MD5Hash Hash;
	}

	private struct FolderTimestamp : IComparable<FolderTimestamp>
	{
		public string Name;

		public DateTime Timestamp;

		public int CompareTo(FolderTimestamp other)
		{
			return Timestamp.CompareTo(other.Timestamp);
		}
	}

	private class SortPlayerRecordsByLastUsedTimeDescending : IComparer<PlayerRecord>
	{
		int IComparer<PlayerRecord>.Compare(PlayerRecord a, PlayerRecord b)
		{
			if (a.LastUsedTime > b.LastUsedTime)
			{
				return -1;
			}
			if (a.LastUsedTime < b.LastUsedTime)
			{
				return 1;
			}
			return 0;
		}
	}

	public static TimeSpan DemoTimeout = Sun.DayLength;

	public static TimeSpan DemoTimeoutWarning = DemoTimeout - TimeSpan.FromMinutes(3.0);

	public static Session Instance;

	public List<StorySource> StorySources = new List<StorySource>();

	public bool Editor;

	public bool DebugPaused;

	public bool DebugOneStep;

	public SessionState State;

	public WantFinishState WantFinish;

	public GameFinishedState GameFinishedState;

	public bool ShowContinueButton;

	public CinematicState WantCinematicState;

	public TileObject ExfilVehicle;

	public List<Character> ExfilCharacters = new List<Character>();

	public float Transition;

	public int SessionId;

	public int InputFrame = -1;

	public CustomRandom DeterministicRand = new CustomRandom(42);

	public TimeSpan PlayTime;

	public int HitTheRoadCount;

	public int Frame;

	public int PlaySpeedIndependantUpdateFrame;

	public int PlaySpeedAndPauseIndependantUpdateFrame;

	public PlaySpeed PlaySpeed = PlaySpeed.Normal;

	public bool CanFastForwardEvenInCombat;

	public bool WasPausedAutomatically;

	private int InputFrameWhenWeLastSetPlaySpeed;

	public int GameUniqueId;

	public int RandomSeed;

	public int FirstObjectIdSpawnedDuringPlay;

	public DifficultySettings DifficultySettings = new DifficultySettings();

	public TimeSpan LastDayStartTime;

	public bool WantAutoSave;

	public bool WantTokenSave;

	public bool WantSurpriseSaveGameOverwrite;

	public bool PlaySoundAfterTokenSave;

	public int TokenSaveCameFromCarrierId;

	public EquipmentPrototype TokenSaveProto;

	public bool FollowerCommandsEnabled = true;

	public bool LoneWolf = true;

	public bool AchievementsEnabled = true;

	public TimeSpan LastAutoSavedTime;

	public TimeSpan LastSavedTime;

	public int CountdownToTimeJump;

	public Community PreferredPlayerRescuer;

	public TerrainCoord PreferredPlayerRescuerTile;

	public List<SkillsChangePopup> SkillsChangePopups = new List<SkillsChangePopup>();

	public bool WantLoadNewMapAfterSkillsChange;

	public List<PlayerRecord> PlayerRecords = new List<PlayerRecord>();

	public List<PlayerID> NetworkPlayerIDs = new List<PlayerID>();

	public List<string> NetworkPlayerNames = new List<string>();

	public Character WantSwitchControlledCharacter;

	public Hud Hud = new Hud();

	public GameCamera GameCamera = new GameCamera();

	public Weather Weather = new Weather();

	public BaseObjectManager BaseObjectManager = new BaseObjectManager();

	public CharacterManager CharacterManager = new CharacterManager();

	public CommunityManager CommunityManager = new CommunityManager();

	public PropManager PropManager = new PropManager();

	public StoryManager StoryManager = new StoryManager();

	public AISoundManager AISoundManager = new AISoundManager();

	public PredictedObjectManager PredictedObjectManager = new PredictedObjectManager();

	public CropsManager CropsManager = new CropsManager();

	public HintManager HintManager = new HintManager();

	public DebugMenu DebugMenu;

	private List<SessionSnapshot> SessionSnapshots = new List<SessionSnapshot>();

	private int SessionOutOfSyncFrame = -1;

	public static string SessionOutOfSyncSnapshotFolder;

	private InputsRecord RecordingInputs;

	private InputsRecord PlayingInputs;

	public static InputsRecord WantPlayingInputs;

	public SortBy InventorySortBy;

	public SortOrder InventorySortOrder = SortOrder.Descending;

	public SortCharactersBy CharactersSortBy;

	public SortOrder CharactersSortOrder = SortOrder.Descending;

	private SortBy SentInventorySortBy;

	private SortOrder SentInventorySortOrder;

	private SortCharactersBy SentCharactersSortBy;

	private SortOrder SentCharactersSortOrder;

	public List<TileObject> MovableUnityObjectsVisibleInMainView = new List<TileObject>();

	public List<TileObject> UnityObjectsVisibleInPip = new List<TileObject>();

	public List<TileObject> ActiveMovingUnityObjects = new List<TileObject>();

	public List<TileObject> StaticUnityObjectsThatNeedUpdate = new List<TileObject>();

	public TimeSpan LastBushRustleTime = Target.Never;

	public int[] HighestSkillLevel = new int[10];

	public bool VerifiedStrings;

	public List<SpawnedEquipmentCount> EquipmentSpawns = new List<SpawnedEquipmentCount>();

	public int NextFreeInjuryId = 1;

	private static int NextFreeSessionId = 1;

	private TaskFunc CalcSnapshotHashOnThreadFunc;

	public MD5Hash TerrainHash;

	private static string FEMA = "FEMA";

	private static string FEMASquad = "FEMASquad";

	private static string Guardians = "Guardians";

	private static string RitzCreekGang = "RitzCreekGang";

	private static string CabinPeople = "CabinPeople";

	private static string CarterMoreno = "CarterMoreno";

	private static string MartinSteele = "MartinSteele";

	private static string MysteriousStranger = "MysteriousStranger";

	public CharacterCreationSettings IncomingCharacter;

	public int InputFrameWhenWeLastSentCreateCharacter = -1;

	public string IncomingChatText;

	private float TitleMenuPressedTime;

	private static GameProfiler AISoundManagerUpdateTimer = new GameProfiler("Update.AISoundManager");

	private static GameProfiler CharacterUpdateVisibleTimer = new GameProfiler("Update.VisibleCharacters");

	private static GameProfiler CommunityUpdateTimer = new GameProfiler("Update.CommunityUpdate");

	private static GameProfiler CharacterUpdateTimer = new GameProfiler("Update.CharacterManager");

	private static GameProfiler CharacterThinkTimer = new GameProfiler("Update.CharacterThink");

	private static GameProfiler CollisionDetectionTimer = new GameProfiler("Update.CollisionDetection");

	private static GameProfiler CropsUpdateTimer = new GameProfiler("Update.CropsUpdate");

	private static GameProfiler PropsUpdateTimer = new GameProfiler("Update.PropsUpdate");

	private static GameProfiler StoryUpdateTimer = new GameProfiler("Update.StoryUpdate");

	private static GameProfiler BirdSongUpdateTimer = new GameProfiler("Update.BirdSong");

	private static GameProfiler FogOfWarUpdateTimer = new GameProfiler("Update.FogOfWar");

	private static GameProfiler PredictionTimer = new GameProfiler("Update.Prediction");

	private static GameProfiler NonDeterministicFixedUpdateTimer = new GameProfiler("Update.NonDeterministicFixedUpdate");

	private static GameProfiler NonDeterministicUpdateTimer = new GameProfiler("Update.NonDeterministicUpdate");

	private static GameProfiler UpdateVisibleObjectsTimer = new GameProfiler("Update.UpdateVisibleObjects");

	private static GameProfiler DeterministicUpdateTimer = new GameProfiler("Update.DeterministicUpdate");

	private static GameProfiler TakeSnapshotTimer = new GameProfiler("Update.TakeSnapshot");

	public static int UnityActivationRangeMainView = 32;

	public static int UnityActivationRangePipView = 16;

	private static List<TileObject> tmp = new List<TileObject>();

	private static List<TileObject> tmp2 = new List<TileObject>();

	public static int FramesWithoutPrediction = 0;

	public static int MaxFramesWithoutPrediction = 10;

	public bool WantSlowerTransitionIn;

	private TerrainCoord LastFocusTile = new TerrainCoord(-100000, -100000);

	public bool WantFullRefreshOfActiveUnityObjectsInFocusArea;

	private static GameProfiler PreProcessInputFrameTimer = new GameProfiler("Update.PreProcessInputFrameTimer");

	private static GameProfiler PostProcessInputFrameTimer = new GameProfiler("Update.PostProcessInputFrameTimer");

	public static float FoodMarkup = 2f;

	public List<LogEvent> LogEvents = new List<LogEvent>();

	public LogEventFeed GlobalEventFeed;

	public const int DeterministicMaxVisibleDist = 32;

	public const int DeterministicPipMaxVisibleDist = 8;

	private IEnumerator UnityInitCoroutine;

	private int UnityInitProgress;

	private static StringBuilder GoalString = new StringBuilder(100);

	private static int FastSnapshotFrequency = 600;

	private static int FullSnapshotFrequency = 3600;

	public static bool OutOfSyncDebugEnabled = true;

	public static bool RecordInputsEnabled = false;

	public static bool ForceFullSnapshotStatic;

	public bool ForceFullSnapshot;

	private List<SaveRequest> LatestAutosaveRequests = new List<SaveRequest>();

	private static SortPlayerRecordsByLastUsedTimeDescending PlayerRecordSorter = new SortPlayerRecordsByLastUsedTimeDescending();

	public float DaysSinceStart => Weather.CalcDaysSinceStartFromTime(PlayTime);

	public float DayOfYear => (DaysSinceStart + (float)Weather.StartDayOfYear) % (float)Weather.DaysInAYear;

	public float HourOfDay => CalcHourOfDayFromDaysSinceStart(DaysSinceStart);

	public int Day => CalcDayFromTime(PlayTime);

	public static float CalcHourOfDayFromDaysSinceStart(float daysSinceStart)
	{
		return (daysSinceStart - Mathf.Floor(daysSinceStart)) * 24f;
	}

	public void AssignNewSessionId()
	{
		SessionId = NextFreeSessionId++;
	}

	public Session(bool editor, int sessionId, List<StorySource> storySources, MD5Hash terrainHash)
	{
		Instance = this;
		DeterministicRand.Locked = true;
		Editor = editor;
		if (sessionId == 0)
		{
			AssignNewSessionId();
		}
		else
		{
			SessionId = sessionId;
		}
		StorySources = storySources;
		TerrainHash = terrainHash;
		ForceFullSnapshot = ForceFullSnapshotStatic;
		CalcSnapshotHashOnThreadFunc = CalcSnapshotHashOnThread;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref reflector.Version);
		if (reflector.Version > 629)
		{
			throw new Exception(GameImpl.Translate("MENU_SaveGameFromFuture").Replace("%1", reflector.Version.ToString()).Replace("%2", 629.ToString()));
		}
		if (reflector.Version < 240)
		{
			string value = string.Empty;
			ulong value2 = 0uL;
			reflector.AddAfter(ref value, 131);
			reflector.AddAfter(ref value2, 213);
			if (reflector.IsDeserialising && value != GameImpl.Instance.CurrentStory.StorySource.Folder)
			{
				UnityEngine.Debug.LogWarning("Session was saved with a different story folder '" + value + "', you are using '" + GameImpl.Instance.CurrentStory.StorySource.Folder + "'");
			}
		}
		else
		{
			int value3 = GameImpl.Instance.CurrentStories.Count;
			reflector.Add(ref value3);
			for (int i = 0; i < value3; i++)
			{
				string text = ((i < GameImpl.Instance.CurrentStories.Count) ? GameImpl.Instance.CurrentStories[i].StorySource.Folder : string.Empty);
				ulong num = ((i < GameImpl.Instance.CurrentStories.Count) ? GameImpl.Instance.CurrentStories[i].StorySource.WorkshopId : 0);
				string value4 = text;
				ulong value5 = num;
				reflector.Add(ref value4);
				reflector.Add(ref value5);
				if (reflector.IsDeserialising && value4 != text)
				{
					UnityEngine.Debug.LogWarning("Session was saved with a different story folder '" + value4 + "', you are using '" + text + "' (" + i + ")");
				}
			}
		}
		if (reflector.Version >= 132)
		{
			MD5Hash terrainHash = TerrainHash;
			terrainHash.Reflect(reflector);
			if (reflector.IsDeserialising)
			{
				if (TerrainHash.IsValid())
				{
					if (TerrainHash != terrainHash)
					{
						string[] obj = new string[5] { "Loading terrain with hash (", null, null, null, null };
						MD5Hash terrainHash2 = TerrainHash;
						obj[1] = terrainHash2.ToString();
						obj[2] = ") that doesn't match savegame (";
						terrainHash2 = terrainHash;
						obj[3] = terrainHash2.ToString();
						obj[4] = ")";
						UnityEngine.Debug.LogWarning(string.Concat(obj));
					}
				}
				else
				{
					TerrainHash = terrainHash;
				}
			}
		}
		reflector.AddAfter(ref reflector.IsDoingNetworkChecksum, 140);
		reflector.AddAfter(ref reflector.IsFastPath, 140);
		reflector.Add(ref PlayTime);
		reflector.Add(ref Frame);
		reflector.Add(ref PlaySpeed);
		reflector.AddAfter(ref GameFinishedState, 135);
		reflector.AddAfter(ref ShowContinueButton, 249);
		reflector.AddAfter(ref HitTheRoadCount, 478);
		if (reflector.Version < 551)
		{
			LastDayStartTime = PlayTime;
		}
		else
		{
			reflector.Add(ref LastDayStartTime);
		}
		if (reflector.Version >= 509)
		{
			reflector.Add(ref WantCinematicState);
			reflector.Add(ref ExfilVehicle);
		}
		if (reflector.Version >= 250)
		{
			reflector.AddGameObjectRefList(ref ExfilCharacters);
		}
		reflector.Add(ref CanFastForwardEvenInCombat);
		reflector.AddAfter(ref WasPausedAutomatically, 14);
		reflector.Add(ref DeterministicRand.Seed);
		reflector.Add(ref PlaySpeedIndependantUpdateFrame);
		reflector.AddAfter(ref PlaySpeedAndPauseIndependantUpdateFrame, 488);
		reflector.Add(ref NextFreeInjuryId);
		reflector.AddAfter(ref CountdownToTimeJump, 348);
		if (reflector.Version < 528)
		{
			reflector.AddAfter(ref PreferredPlayerRescuer, 454);
		}
		if (reflector.Version < 603)
		{
			reflector.AddAfter(ref SkillsChangePopups, 601);
			reflector.AddAfter(ref WantLoadNewMapAfterSkillsChange, 602);
		}
		reflector.AddAfter(ref FollowerCommandsEnabled, 457);
		reflector.AddAfter(ref RandomSeed, 229);
		reflector.AddAfter(ref GameUniqueId, 6);
		reflector.AddAfter(ref FirstObjectIdSpawnedDuringPlay, 427);
		reflector.AddAfter(ref ForceFullSnapshot, 191);
		if (reflector.Version < 227)
		{
			bool value6 = true;
			bool value7 = true;
			DifficultyMode value8 = DifficultyMode.Normal;
			reflector.AddAfter(ref value6, 6);
			reflector.AddAfter(ref value7, 201);
			reflector.AddAfter(ref value8, 202);
			DifficultySettings.DifficultyName = value8.ToString();
			DifficultySettings.SaveTokensRequired = value6;
			DifficultySettings.ZombieCrippledPercentage = ((value8 == DifficultyMode.SlightlyEasier) ? 50 : 0);
			DifficultySettings.InvisibleStrainPercentage = (value7 ? 5f : 0f);
		}
		else
		{
			reflector.Add(DifficultySettings);
		}
		if (reflector.Version >= 88 && !reflector.IsFastPath)
		{
			reflector.Add(CropsManager);
		}
		reflector.Add(BaseObjectManager);
		reflector.Add(StoryManager);
		reflector.Add(CommunityManager);
		reflector.Add(AISoundManager);
		reflector.Add(Weather);
		reflector.AddAfter(ref SkillsChangePopups, 603);
		reflector.AddAfter(ref WantLoadNewMapAfterSkillsChange, 603);
		if (reflector.Version >= 265)
		{
			reflector.Add(PropManager);
		}
		if (reflector.Version >= 84 && reflector.Version < 88)
		{
			reflector.Add(CropsManager);
		}
		if (reflector.Version >= 186)
		{
			reflector.Add(HintManager);
		}
		if (reflector.Version >= 223)
		{
			reflector.Add(ref EquipmentSpawns);
		}
		reflector.AddAfter(ref PreferredPlayerRescuer, 528);
		reflector.AddAfter(ref PreferredPlayerRescuerTile, 606);
		if (reflector.Version < 553)
		{
			LoneWolf = CommunityManager.PlayerCommunity.IsLoneWolfCommunity();
		}
		else
		{
			reflector.Add(ref LoneWolf);
		}
		reflector.AddAfter(ref AchievementsEnabled, 553);
		MapPage.Instance.Reflect(reflector);
		reflector.Add(ref PlayerRecords);
		_ = Editor;
		reflector.Add(ref LogEvents);
		if (!reflector.IsDoingNetworkChecksum)
		{
			if (reflector.Version >= 431)
			{
				if (reflector.IsSerialising)
				{
					if (Editor)
					{
						int value9 = 0;
						int value10 = 0;
						int value11 = 0;
						reflector.Add(ref value9);
						reflector.Add(ref value10);
						reflector.Add(ref value11);
					}
					else
					{
						int value12 = GetNumDiscoveredEquipmentTypes();
						reflector.Add(ref value12);
						foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
						{
							if (item.Value.Discovered)
							{
								reflector.Add(ref item.Value.Name);
							}
						}
						int value13 = GetNumDiscoveredLiquidTypes();
						reflector.Add(ref value13);
						foreach (KeyValuePair<string, LiquidPrototype> item2 in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
						{
							if (item2.Value.Discovered)
							{
								reflector.Add(ref item2.Value.Name);
							}
						}
						int value14 = GetNumDiscoveredPropTypes();
						reflector.Add(ref value14);
						foreach (KeyValuePair<int, PropPrototype> item3 in GameImpl.Instance.CurrentPropPrototypesDeterministic)
						{
							if (item3.Value.Discovered)
							{
								reflector.Add(ref item3.Value.Name);
							}
						}
					}
				}
				else
				{
					int value15 = 0;
					reflector.Add(ref value15);
					for (int j = 0; j < value15; j++)
					{
						string value16 = null;
						reflector.Add(ref value16);
						if (reflector.Version >= 437)
						{
							GameImpl.Instance.FindEquipmentPrototypeByName(value16)?.MarkDiscovered();
						}
					}
					if (reflector.Version < 437)
					{
						int value17 = 0;
						reflector.Add(ref value17);
						for (int k = 0; k < value17; k++)
						{
							string value18 = null;
							reflector.Add(ref value18);
						}
					}
					else
					{
						int value19 = 0;
						reflector.Add(ref value19);
						for (int l = 0; l < value19; l++)
						{
							string value20 = null;
							reflector.Add(ref value20);
							GameImpl.Instance.FindLiquidPrototypeByName(value20)?.MarkDiscovered();
						}
						int value21 = 0;
						reflector.Add(ref value21);
						for (int m = 0; m < value21; m++)
						{
							string value22 = null;
							reflector.Add(ref value22);
							GameImpl.Instance.FindPropPrototypeByName(value22)?.MarkDiscovered();
						}
					}
				}
			}
			if (reflector.Version < 438 && !Editor)
			{
				Community playerCommunity = CommunityManager.PlayerCommunity;
				foreach (Character member in playerCommunity.Members)
				{
					UpdateHighestSkillLevel(member);
					foreach (Equipment content in member.Inventory.Contents)
					{
						content.MarkDiscovered();
					}
				}
				foreach (Prop building2 in playerCommunity.Buildings)
				{
					if (building2.UnderConstructionInfo != null)
					{
						continue;
					}
					building2.GetPropPrototype().MarkDiscovered();
					foreach (Equipment content2 in building2.Inventory.Contents)
					{
						content2.MarkDiscovered();
					}
				}
			}
		}
		if (reflector.Version >= 438)
		{
			reflector.AddIntArray(ref HighestSkillLevel);
		}
		if (reflector.IsDeserialising)
		{
			if (reflector.Version < 413 || Editor)
			{
				LogEvents.Clear();
			}
			foreach (LogEvent logEvent in LogEvents)
			{
				logEvent.AddToFeeds(canDeleteEvents: false);
			}
			for (int num2 = LogEvents.Count - 1; num2 >= 0; num2--)
			{
				if (LogEvents[num2].FeedRefCount <= 0)
				{
					LogEvents.RemoveAt(num2);
				}
			}
		}
		if (reflector.Version < 111 && reflector.IsDeserialising)
		{
			foreach (Prop allProp in PropManager.AllProps)
			{
				if (!(allProp is Campfire campfire))
				{
					continue;
				}
				while (true)
				{
					TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(campfire.Tile.x, campfire.Tile.y);
					if (fixedObjectOnTile == null || fixedObjectOnTile == campfire || !fixedObjectOnTile.CanBeClearedForBuilding(null, null))
					{
						break;
					}
					fixedObjectOnTile.Delete();
				}
			}
		}
		if (reflector.Version < 512)
		{
			foreach (Community community2 in CommunityManager.Communities)
			{
				if (!(community2.GetUniqueID() == FEMASquad) || community2.CommunityType != CommunityType.Normal)
				{
					continue;
				}
				foreach (Character member2 in community2.Members)
				{
					if (member2.SquadLeader != null && member2.SquadLeader.GetUniqueID() == CarterMoreno && member2.GetSquad() == null)
					{
						member2.Follow(null);
					}
				}
			}
		}
		if (reflector.Version < 513 && GameTerrain.Instance.UseFixedTerrain && BaseObjectManager.Instance.GetObjectByUniqueID("CabinInTheWoods") is Building building)
		{
			StoryManager.Instance.EnableTrigger("Meat_Trigger_CabinInvestigated", null, null, null);
			building.Investigated = false;
		}
		if (reflector.Version < 514 && GameTerrain.Instance.UseFixedTerrain)
		{
			foreach (Community community3 in CommunityManager.Communities)
			{
				if (!(community3.GetUniqueID() == CabinPeople))
				{
					continue;
				}
				foreach (Character member3 in community3.Members)
				{
					if (member3.HasPersonality(CachedPersonalityType.Hypocritical))
					{
						member3.SetPersonality(CachedPersonalityType.Hypocritical, val: false);
						member3.SetPersonality(CachedPersonalityType.Amoral, val: true);
					}
					else if (member3.HasPersonality(CachedPersonalityType.Moral))
					{
						member3.SetPersonality(CachedPersonalityType.Moral, val: false);
						member3.SetPersonality(CachedPersonalityType.Amoral, val: true);
					}
					if (member3.InvisibleStrain == InvisibleStrainType.None)
					{
						member3.InvisibleStrain = InvisibleStrainType.Subtle;
						member3.HadInvisibleStrainFromStart = true;
					}
					if (member3.GetUniqueID() == MartinSteele && member3.GetRank() != Rank.Leader && member3.AliveAndNotZombie)
					{
						member3.SetRank(Rank.Leader);
					}
				}
			}
		}
		if (reflector.Version < 619 && GameTerrain.Instance.UseFixedTerrain)
		{
			foreach (Community community4 in CommunityManager.Communities)
			{
				if (!(community4.GetUniqueID() == CabinPeople) && !(community4.GetUniqueID() == MysteriousStranger))
				{
					continue;
				}
				community4.DiscoverNameIfAtWar = true;
				foreach (Character member4 in community4.Members)
				{
					member4.AlwaysActivateInvisibleStrain = true;
				}
			}
		}
		if (reflector.Version < 515 && GameTerrain.Instance.UseFixedTerrain)
		{
			foreach (Community community5 in CommunityManager.Communities)
			{
				if (!(community5.GetUniqueID() == FEMA) && !(community5.GetUniqueID() == Guardians) && !(community5.GetUniqueID() == RitzCreekGang))
				{
					continue;
				}
				foreach (Character member5 in community5.Members)
				{
					if (member5.HasPersonality(CachedPersonalityType.Bipolar))
					{
						member5.SetPersonality(CachedPersonalityType.Bipolar, val: false);
						member5.SetPersonality(CachedPersonalityType.Emotional, val: true);
					}
				}
			}
		}
		if (reflector.Version < 516 && GameTerrain.Instance.UseFixedTerrain)
		{
			foreach (Community community6 in CommunityManager.Communities)
			{
				community6.FixupTraders();
			}
		}
		if (reflector.Version < 609 && GameTerrain.Instance.UseFixedTerrain && BaseObjectManager.Instance.FindBaseObjectByID(1154) is Still { Community: null } still && BaseObjectManager.Instance.GetObjectByUniqueID(Character.TentCity) is Community community)
		{
			still.SetCommunity(community);
		}
	}

	public void UpdateHighestSkillLevel(Character member)
	{
		if (member.IsControllableByPlayer())
		{
			member.GetSkillEffects(Character.SkillEffects);
			bool flag = false;
			for (int i = 0; i < 10; i++)
			{
				int num = Math.Max(HighestSkillLevel[i], member.CalcSkillLevelFromEffects((SkillType)i, Character.SkillEffects));
				flag |= num > HighestSkillLevel[i];
				HighestSkillLevel[i] = num;
			}
			if (flag && State == SessionState.Started)
			{
				NotificationManager.Instance.ShowRecipeDiscoveredNotifications();
			}
		}
	}

	public void ClearDiscoveredFlags()
	{
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			item.Value.Discovered = false;
		}
		foreach (KeyValuePair<string, LiquidPrototype> item2 in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			item2.Value.Discovered = false;
		}
		foreach (KeyValuePair<int, PropPrototype> item3 in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			item3.Value.Discovered = false;
		}
	}

	public int GetNumDiscoveredEquipmentTypes()
	{
		int num = 0;
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.Discovered)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumDiscoveredLiquidTypes()
	{
		int num = 0;
		foreach (KeyValuePair<string, LiquidPrototype> item in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			if (item.Value.Discovered)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumDiscoveredPropTypes()
	{
		int num = 0;
		foreach (KeyValuePair<int, PropPrototype> item in GameImpl.Instance.CurrentPropPrototypesDeterministic)
		{
			if (item.Value.Discovered)
			{
				num++;
			}
		}
		return num;
	}

	public void PreHandleInput(InputFrame inputFrame)
	{
		GameCursor.CachedIsAnyoneOnScreenInCombat = GameCursor.IsAnyoneOnScreenInCombat();
		InfoScreen.Instance.PreHandleInput(inputFrame);
		if (!Editor)
		{
			return;
		}
		InputFunctionManager instance = InputFunctionManager.Instance;
		float axis = instance.GetAxis(InputFunction.Control);
		if (axis == 0f)
		{
			return;
		}
		instance.Capture(InputFunction.Control, untilReleased: true);
		Character character = Hud.EditorSelectedObject as Character;
		Community community = character?.GetCommunity();
		if (character == null || community == null)
		{
			return;
		}
		Hud.EditorSelectedObject = ((axis > 0f) ? community.GetNextLivingMember(character, followersOnly: false) : community.GetPrevLivingMember(character, followersOnly: false));
		SoundManager.PlayMenuSound(SoundManager.TabSound);
		if (InfoScreen.Instance.Active)
		{
			if (InfoScreen.Instance.Active)
			{
				InfoScreen.Instance.OnDeactivate();
			}
			InfoScreen.Instance.Activate(Hud.EditorSelectedObject, null);
		}
		else
		{
			HudBehaviour.Instance.ShowCharacterSwitchBar();
		}
	}

	public void HandleInput(InputFrame inputFrame)
	{
		GameImpl instance = GameImpl.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		PlayerRecord localPlayerRecord = GetLocalPlayerRecord();
		if (inputFrame != null && IncomingCharacter != null)
		{
			inputFrame.AddAction(InputAction.CreateCharacter(IncomingCharacter));
			IncomingCharacter = null;
			InputFrameWhenWeLastSentCreateCharacter = inputFrame.Frame;
		}
		while (LatestAutosaveRequests.Count > 0 && LatestAutosaveRequests[0].CurrentState != SaveRequest.State.Running && (inputFrame != null || !LatestAutosaveRequests[0].SaveGame.IsTokenSave()))
		{
			if (LatestAutosaveRequests[0].CurrentState == SaveRequest.State.Failed)
			{
				HudBehaviour.Instance.SetStatusBarMsg(LatestAutosaveRequests[0].GetErrorMessage());
				if (LatestAutosaveRequests[0].SaveGame.IsTokenSave() && LatestAutosaveRequests[0].TokenSaveCameFromCarrierId != 0 && LatestAutosaveRequests[0] != null && BaseObjectManager.Instance.FindBaseObjectByID(LatestAutosaveRequests[0].TokenSaveCameFromCarrierId) is TileObject obj)
				{
					inputFrame.AddAction(InputAction.EquipmentSpawn(obj, LatestAutosaveRequests[0].TokenProto));
				}
			}
			LatestAutosaveRequests.RemoveAt(0);
		}
		if (IsInMultiplayerGame())
		{
			if (inputFrame != null && !string.IsNullOrEmpty(IncomingChatText))
			{
				inputFrame.AddAction(InputAction.Chat(IncomingChatText));
				IncomingChatText = null;
				instance2.Capture(InputFunction.MenuSelect, untilReleased: true);
			}
			if (HudBehaviour.Instance.IsChatting())
			{
				if (instance2.IsJustPressed(InputFunction.Back))
				{
					HudBehaviour.Instance.CurrentChatState = ChatState.WantClosed;
				}
				return;
			}
			if (string.IsNullOrEmpty(IncomingChatText) && instance2.IsJustPressed(InputFunction.Chat))
			{
				HudBehaviour.Instance.CurrentChatState = ChatState.WantOpen;
			}
		}
		if (instance.LastCursorLockMode != CursorLockMode.Locked || GameCamera.FlyCam)
		{
			instance2.CaptureMouseAxis(MouseAxis.Vert, Captured.ThisFrame);
			instance2.CaptureMouseAxis(MouseAxis.Horiz, Captured.ThisFrame);
			instance2.CaptureMouseAxis(MouseAxis.SmoothedVert, Captured.ThisFrame);
			instance2.CaptureMouseAxis(MouseAxis.SmoothedHoriz, Captured.ThisFrame);
		}
		if (instance2.IsJustPressed(InputFunction.DebugMenu))
		{
			bool flag = true;
			if (DebugMenu.State == DebugPageState.Inactive && flag)
			{
				DebugMenu.SetState(DebugPageState.Active, null);
			}
			else
			{
				DebugMenu.SetState(DebugPageState.Inactive, null);
			}
		}
		Hud.HandleInput(inputFrame);
		if (instance.IsDialogOpen())
		{
			return;
		}
		bool flag2 = false;
		if (IsInMultiplayerGame() && instance.Settings.VoiceChatEnabled && instance.Settings.PushBtnToTalkEnabled)
		{
			if (TitleMenuPressedTime < 0.5f)
			{
				if (instance2.IsPressed(InputFunction.TitleMenu))
				{
					TitleMenuPressedTime += GameImpl.UnscaledDeltaTime;
				}
				else if (TitleMenuPressedTime > 0f)
				{
					TitleMenuPressedTime = 0f;
					flag2 = true;
				}
			}
			else if (!instance2.IsPressed(InputFunction.TitleMenu, capture: false))
			{
				TitleMenuPressedTime = 0f;
			}
		}
		else
		{
			flag2 = instance2.IsJustPressed(InputFunction.TitleMenu);
		}
		if (flag2 && WantFinish == WantFinishState.None && GameFinishedState != GameFinishedState.GameOver && GameFinishedState != GameFinishedState.GameComplete)
		{
			WantFinish = WantFinishState.PauseMenu;
			if (DifficultySettings.SaveTokensRequired)
			{
				instance.RequestedAutoSaveFromThread = SaveGameType.Current;
			}
		}
		if (!Editor && IsPartyLeader() && instance2.IsJustPressed(InputFunction.QuickSave))
		{
			if (DifficultySettings.SaveTokensRequired)
			{
				inputFrame?.AddAction(new InputAction(InputActionType.QuickSaveUsingToken));
			}
			else
			{
				instance.RequestedAutoSaveFromThread = SaveGameType.QuickSave;
			}
		}
		if ((!IsInMultiplayerGame() || IsInMultiplayerGameAsLeader()) && !DifficultySettings.SaveTokensRequired && SaveGameManager.Instance.HasAnySaveGames && instance2.IsJustPressed(InputFunction.QuickLoad))
		{
			WantFinish = WantFinishState.LoadLatestSave;
		}
		if (inputFrame != null && !InfoScreen.Instance.Active && !NotificationManager.Instance.IsDisplayingNotification() && localPlayerRecord != null && localPlayerRecord.PlayerMode == PlayerMode.Controlling && GameCamera.FlyCam == GameCamera.SentFlyCam)
		{
			if (!GameCamera.FlyCam && !IsPaused())
			{
				localPlayerRecord.PlayerCharacter.HandleInput(localPlayerRecord, inputFrame);
			}
			localPlayerRecord.PlayerCharacter.HandleInputEvenWhilePaused(localPlayerRecord, inputFrame);
		}
		if (inputFrame != null)
		{
			if (!InfoScreen.Instance.Active && Hud.Instance.Pip.CanUseRightStickToGoToPipObject && instance2.IsJustPressed(InputFunction.ControlPipCharacter))
			{
				TileObject focusObject = Hud.Instance.Pip.FocusObject;
				Character character = focusObject as Character;
				if (character != null && !character.IsControllableByPlayer() && character.GetCurrentTarget() is Character character2 && character2.IsControllableByPlayer())
				{
					character = character2;
				}
				if (character != null && character.IsControllableByPlayer())
				{
					bool snap = character != Hud.Instance.CursorTargetObject;
					SoundManager.PlayMenuSound(SoundManager.TabSound);
					Hud.SetLocalControlledCharacter(character, inputFrame);
					GameCamera.SetFlyCamMode(on: false, snap, inputFrame);
				}
				else
				{
					GameCamera.SetFlyCamMode(on: true, snap: true, inputFrame);
				}
				GameCamera.TeleportToObject(focusObject);
			}
			float axis = instance2.GetAxis(InputFunction.Control);
			float axis2 = instance2.GetAxis(InputFunction.ControlFollower);
			if (axis != 0f)
			{
				instance2.Capture(InputFunction.Control, untilReleased: true);
			}
			if (axis2 != 0f)
			{
				instance2.Capture(InputFunction.ControlFollower, untilReleased: true);
			}
			if (axis > 0f && CanSwitchControlledCharacter())
			{
				Hud.SetLocalControlledCharacter(CommunityManager.PlayerCommunity.GetNextLivingMember(localPlayerRecord.PlayerCharacter, followersOnly: false), inputFrame);
				HintManager.Hints[8].MarkPerformed();
				HudBehaviour.Instance.ShowCharacterSwitchBar();
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
			else if (axis < 0f && CanSwitchControlledCharacter())
			{
				Hud.SetLocalControlledCharacter(CommunityManager.PlayerCommunity.GetPrevLivingMember(localPlayerRecord.PlayerCharacter, followersOnly: false), inputFrame);
				HintManager.Hints[8].MarkPerformed();
				HudBehaviour.Instance.ShowCharacterSwitchBar();
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
			else if (axis2 > 0f && CanSwitchControlledCharacter())
			{
				Hud.SetLocalControlledCharacter(CommunityManager.PlayerCommunity.GetNextLivingMember(localPlayerRecord.PlayerCharacter, followersOnly: true), inputFrame);
				HintManager.Hints[9].MarkPerformed();
				HudBehaviour.Instance.ShowCharacterSwitchBar();
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
			else if (axis2 < 0f && CanSwitchControlledCharacter())
			{
				Hud.SetLocalControlledCharacter(CommunityManager.PlayerCommunity.GetPrevLivingMember(localPlayerRecord.PlayerCharacter, followersOnly: true), inputFrame);
				HintManager.Hints[9].MarkPerformed();
				HudBehaviour.Instance.ShowCharacterSwitchBar();
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
		}
		if (!InfoScreen.Instance.Active)
		{
			GameCamera.HandleInput(inputFrame);
		}
		if (inputFrame == null)
		{
			return;
		}
		PlaySpeed slowSpeed = GetSlowSpeed();
		if (CanPressPauseOrFastForward())
		{
			int buttonPromptHash = ((PlaySpeed != PlaySpeed.Normal && !InfoScreen.Instance.Active) ? ButtonPromptBarBehaviour.PROMPT_PlaySpeed : 0);
			float justPressedAxis = instance2.GetJustPressedAxis(InputFunction.PlaySpeed, buttonPromptHash);
			if (FollowerCommandsEnabled)
			{
				if (justPressedAxis < 0f)
				{
					if (PlaySpeed == PlaySpeed.Paused && IsAnyoneInPlayerCommunityInCombat())
					{
						inputFrame.AddAction(InputAction.SetCanFastForwardEvenInCombat(val: true));
					}
					inputFrame.AddAction(new InputAction(InputActionType.DecreasePlaySpeed));
					InputFrameWhenWeLastSetPlaySpeed = inputFrame.Frame;
				}
				if (justPressedAxis > 0f)
				{
					if (PlaySpeed >= slowSpeed && PlaySpeed < PlaySpeed.FastForwardx4 && IsAnyoneInPlayerCommunityInCombat())
					{
						inputFrame.AddAction(InputAction.SetCanFastForwardEvenInCombat(val: true));
					}
					inputFrame.AddAction(new InputAction(InputActionType.IncreasePlaySpeed));
					InputFrameWhenWeLastSetPlaySpeed = inputFrame.Frame;
				}
				if (justPressedAxis != 0f)
				{
					HintManager.Instance.Hints[13].MarkPerformed();
				}
			}
			else if (justPressedAxis != 0f)
			{
				HudBehaviour.Instance.SetStatusBarMsg(StringUtil.ApplyFormulae(GameImpl.Translate("HINT_FastForwardDisabled")));
			}
		}
		if (InputFrame >= InputFrameWhenWeLastSetPlaySpeed)
		{
			if (instance2.LostController && PlaySpeed > PlaySpeed.Paused && !IsInMultiplayerGame())
			{
				inputFrame.AddAction(InputAction.SetPlaySpeed(PlaySpeed.Paused));
				InputFrameWhenWeLastSetPlaySpeed = inputFrame.Frame;
			}
			else if (PlaySpeed > slowSpeed && !CanFastForwardEvenInCombat && IsAnyoneInPlayerCommunityInCombat())
			{
				inputFrame.AddAction(InputAction.SetPlaySpeed(slowSpeed));
				InputFrameWhenWeLastSetPlaySpeed = inputFrame.Frame;
			}
		}
		if (WasPausedAutomatically && PlaySpeed == PlaySpeed.Paused && !IsAnyoneInInfoScreen() && (instance2.ControllerConnected || instance2.CurrentInputType == InputType.MouseAndKeyboard || IsInMultiplayerGame()))
		{
			inputFrame.AddAction(InputAction.SetPlaySpeed(PlaySpeed.Normal));
			InputFrameWhenWeLastSetPlaySpeed = inputFrame.Frame;
		}
	}

	private PlaySpeed GetSlowSpeed()
	{
		if (IsInMultiplayerGame() || !IsAnyoneInInfoScreen())
		{
			return PlaySpeed.Normal;
		}
		return PlaySpeed.Paused;
	}

	public void PostHandleInput(InputFrame inputFrame)
	{
		if (WantSwitchControlledCharacter != null && WantSwitchControlledCharacter.IsControllableByPlayer())
		{
			Hud.SetLocalControlledCharacter(WantSwitchControlledCharacter, inputFrame);
			WantSwitchControlledCharacter = null;
		}
		StoryManager.PostHandleInput(inputFrame);
		Hud.PostHandleInput(inputFrame);
		GameCamera.PostHandleInput(inputFrame);
		HintManager.PostHandleInput(inputFrame);
		foreach (TileObject activeMovingUnityObject in ActiveMovingUnityObjects)
		{
			activeMovingUnityObject.GetAuthoritativeOrElseThis()?.UnitySendPhysicsStateToClients(inputFrame);
		}
		if (SentInventorySortBy != InventorySortBy)
		{
			inputFrame.AddAction(InputAction.SetSyncedInventorySortBy(InventorySortBy));
			SentInventorySortBy = InventorySortBy;
		}
		if (SentInventorySortOrder != InventorySortOrder)
		{
			inputFrame.AddAction(InputAction.SetSyncedInventorySortOrder(InventorySortOrder));
			SentInventorySortOrder = InventorySortOrder;
		}
		if (SentCharactersSortBy != CharactersSortBy)
		{
			inputFrame.AddAction(InputAction.SetSyncedCharactersSortBy(CharactersSortBy));
			SentCharactersSortBy = CharactersSortBy;
		}
		if (SentCharactersSortOrder != CharactersSortOrder)
		{
			inputFrame.AddAction(InputAction.SetSyncedCharactersSortOrder(CharactersSortOrder));
			SentCharactersSortOrder = CharactersSortOrder;
		}
		if (Instance.IsInMultiplayerGameAsLeader() && ForceFullSnapshot != ForceFullSnapshotStatic)
		{
			inputFrame.AddAction(InputAction.SetForceFullSnapshot(ForceFullSnapshotStatic));
		}
	}

	private bool CanPressPauseOrFastForward()
	{
		if (!IsInMultiplayerGame() && NotificationManager.Instance.IsDisplayingNotification())
		{
			return false;
		}
		return true;
	}

	public bool IsAnyoneInPlayerCommunityInCombat()
	{
		if (GameCursor.CachedIsAnyoneOnScreenInCombat)
		{
			return true;
		}
		foreach (Character member in CommunityManager.PlayerCommunity.Members)
		{
			if (member.AliveAndNotZombie && member.InCombat)
			{
				return true;
			}
		}
		if (!IsInMultiplayerGame())
		{
			foreach (PlayerRecord playerRecord in PlayerRecords)
			{
				if (playerRecord.SyncedSwappingSuppliesMode == SwappingSuppliesMode.Pickpocketing && playerRecord.PlayerMode == PlayerMode.Controlling)
				{
					return true;
				}
			}
		}
		Character mostInterestingSpeaker = StoryManager.Instance.GetMostInterestingSpeaker();
		if (mostInterestingSpeaker != null && mostInterestingSpeaker.Speaking != null && mostInterestingSpeaker.Speaking.Importance >= Importance.Encouragement)
		{
			return true;
		}
		if (StoryManager.Instance.DramaticDeathCharacter != null)
		{
			return true;
		}
		return false;
	}

	public void QuickSaveUsingToken(PlayerRecord playerRecord)
	{
		Equipment equipment = null;
		TileObject tileObject = null;
		foreach (Character member in CommunityManager.PlayerCommunity.Members)
		{
			if (member.AliveAndNotZombie)
			{
				equipment = member.Inventory.FindItemOfClass(typeof(SavegameToken));
				if (equipment != null)
				{
					tileObject = member;
					break;
				}
			}
		}
		if (equipment == null)
		{
			foreach (Prop building in CommunityManager.PlayerCommunity.Buildings)
			{
				equipment = building.Inventory.FindItemOfClass(typeof(SavegameToken));
				if (equipment != null)
				{
					tileObject = building;
					break;
				}
			}
		}
		if (equipment != null)
		{
			tileObject.GetInventory().UseItem(tileObject, equipment, 1);
			AutoSave(SaveGameType.Token, overwriteAllSlots: false, fromSuspend: false, tileObject.Id, equipment.GetPrototype());
			SoundManager.PlayMenuSound(SoundManager.HallelujahSound, SoundManager.HallelujahVolume);
			if (playerRecord != null && playerRecord.IsLocal)
			{
				HintManager.Instance.Hints[11].MarkPerformed();
			}
			int num = CommunityManager.PlayerCommunity.CountInventoryItemsOfClass(typeof(SavegameToken), includeBuildings: true);
			string str = GameImpl.Translate("HINT_SaveTokensRemaining").Replace("%1", num.ToString());
			HudBehaviour.Instance.SetStatusBarMsg(StringUtil.ApplyFormulae(str, null, num));
		}
		else
		{
			HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate("HINT_NoSaveTokens"));
		}
	}

	public void SpawnCharacter(PlayerRecord playerRecord, CharacterCreationSettings characterSettings)
	{
		CustomRandom deterministicRand = DeterministicRand;
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord tile = new TerrainCoord(instance.Size, instance.Size) / 2;
		float facingAngle = 0f;
		if (StoryManager.PlayerStartPoints.Count > 0)
		{
			PlayerStartPoint playerStartPoint = StoryManager.PlayerStartPoints[deterministicRand.Next(StoryManager.PlayerStartPoints.Count)];
			tile = playerStartPoint.Tile;
			facingAngle = playerStartPoint.Angle;
		}
		int num = 0;
		while (instance.IsImpassable(tile.x, tile.y, 199, null, null) && num < 1000)
		{
			tile += deterministicRand.RandomTile(new TerrainCoord(-1, -1), new TerrainCoord(1, 1));
			num++;
		}
		playerRecord.HasCreatedCharacter = true;
		Character avatarForPlayer = Instance.CharacterManager.GetAvatarForPlayer(playerRecord.PlayerID);
		if (avatarForPlayer != null)
		{
			avatarForPlayer.AvatarForPlayer = default(PlayerID);
		}
		Character character = Human.Spawn(tile, facingAngle, (HumanAppearance)characterSettings.Appearance, InfectionType.None);
		character.NameKnown = true;
		character.Investigated = true;
		character.AvatarForPlayer = playerRecord.PlayerID;
		SetupCharacterFromCharacterSettings(character, characterSettings, newSpawn: true);
		character.SetAllPersonalitiesKnown();
		CommunityManager.PlayerCommunity.AddMemberWithNotifications(character);
		character.OnNewGame();
		if (playerRecord.IsLocal)
		{
			Hud.SetLocalControlledCharacter(character, null);
		}
		SetControlledCharacter(playerRecord, character);
	}

	private void SetupCharacterFromCharacterSettings(Character player, CrewMemberSettings characterSettings, bool newSpawn)
	{
		player.SetFirstName(characterSettings.FirstName);
		player.Surname = characterSettings.Surname;
		player.FirstNameVerified = StringStatus.Unverified;
		player.SurnameVerified = StringStatus.Unverified;
		VerifiedStrings = false;
		characterSettings.Personality.CopyToList(player.Personality);
		player.PersonalityKnown = characterSettings.PersonalityKnown;
		player.InvisibleStrain = characterSettings.InvisibleStrain;
		player.OriginalId = characterSettings.OriginalId;
		player.CachePersonality();
		CrewMemberSettings.CopyValidEquipmentPolicies(characterSettings.CharacterEquipmentPolicies, player.EquipmentPolicies);
		for (int i = 0; i < 10; i++)
		{
			player.Skillset.SetCap(player, (SkillType)i, newSpawn ? 5 : characterSettings.SkillLimits[i]);
			player.Skillset.SetLevel(player, (SkillType)i, characterSettings.Skills[i]);
			if (newSpawn)
			{
				if (i == 9 || i == 0)
				{
					player.Skillset.AddProgress(player, (SkillType)i, (Skillset.ProgressionToLevel[characterSettings.Skills[i] + 1] - Skillset.ProgressionToLevel[characterSettings.Skills[i]]) * 0.25f);
				}
			}
			else
			{
				player.Skillset.SetProgression((SkillType)i, characterSettings.SkillProgression[i]);
			}
		}
		for (int j = 0; j < characterSettings.Inventory.Count; j++)
		{
			Equipment equipment = characterSettings.Inventory[j];
			equipment.OnSpawn();
			player.Inventory.Add(player, equipment);
			if (equipment.GetClothingType() != ClothingType.Invalid && equipment.GetClothingType() != ClothingType.Backpack)
			{
				equipment.Wear(player);
			}
		}
		player.Inventory.GetBestBackpack(player.Skillset.Strength)?.Wear(player);
		if (!DifficultySettings.SaveTokensRequired)
		{
			ReplaceSaveTokensWithGold(player);
		}
	}

	private void DeterministicUpdate()
	{
		using (new ProfileMarker(DeterministicUpdateTimer))
		{
			int framesPerFrame = GetFramesPerFrame();
			bool flag = framesPerFrame == 0;
			TimeSpan timeSpan = TimeSpan.Zero;
			if (!flag)
			{
				timeSpan = MathUtil.FromSeconds(1f / 60f * (float)framesPerFrame);
				Frame += framesPerFrame;
				PlaySpeedIndependantUpdateFrame++;
				PlayTime += timeSpan;
				if (GameImpl.Instance.IsDemo() && PlayTime >= DemoTimeout)
				{
					if (Instance.Transition >= 1f)
					{
						AutoSave(SaveGameType.QuickSave, overwriteAllSlots: false);
					}
					WantFinish = WantFinishState.DemoTimeoutMenu;
				}
				while (PlayTime - LastDayStartTime >= Sun.DayLength)
				{
					LastDayStartTime += Sun.DayLength;
					DifficultySettings.InvisibleStrainPercentage += DifficultySettings.InvisibleStrainIncrementPerYear / (float)Weather.DaysInAYear;
				}
			}
			PlaySpeedAndPauseIndependantUpdateFrame++;
			using (new ProfileMarker(CharacterUpdateVisibleTimer))
			{
				CharacterManager.UpdateVisible();
			}
			if (!flag)
			{
				Weather.DeterministicUpdate(timeSpan);
				using (new ProfileMarker(CommunityUpdateTimer))
				{
					CommunityManager.Update(timeSpan);
				}
				using (new ProfileMarker(CharacterUpdateTimer))
				{
					CharacterManager.Update();
				}
				using (new ProfileMarker(CharacterThinkTimer))
				{
					CharacterManager.Think();
				}
				for (int i = 0; i < framesPerFrame; i++)
				{
					using (new ProfileMarker(CollisionDetectionTimer))
					{
						CharacterManager.CollisionManager.ProcessMovers(10, predicted: false);
					}
				}
				using (new ProfileMarker(CropsUpdateTimer))
				{
					CropsManager.Update();
				}
				using (new ProfileMarker(PropsUpdateTimer))
				{
					PropManager.Update(timeSpan);
				}
				using (new ProfileMarker(StoryUpdateTimer))
				{
					StoryManager.Update();
				}
				using (new ProfileMarker(BirdSongUpdateTimer))
				{
					GameTerrain.Instance.BirdSongManager.Update(flag);
				}
				using (new ProfileMarker(FogOfWarUpdateTimer))
				{
					GameTerrain.Instance.FogOfWar.DeterministicUpdate();
				}
				if (ShouldAutoSave())
				{
					if (DifficultySettings.SaveTokensRequired)
					{
						Instance.AutoSave(SaveGameType.Current, overwriteAllSlots: false);
					}
					else
					{
						WantAutoSave = true;
					}
				}
				if (WantAutoSave)
				{
					Instance.AutoSave(SaveGameType.AutoSave, overwriteAllSlots: false);
					WantAutoSave = false;
				}
				if (CountdownToTimeJump > 0)
				{
					CountdownToTimeJump = Math.Max(0, CountdownToTimeJump - framesPerFrame);
					if (CountdownToTimeJump == 0 && !CommunityManager.PlayerCommunity.IsAnyoneConscious(includeDrunk: false) && !RescuePlayer())
					{
						SetGameOver();
						WantFinish = WantFinishState.GameOverMenu;
					}
				}
				DebugOneStep = false;
			}
			else if (PlaySpeed == PlaySpeed.Paused)
			{
				using (new ProfileMarker(CharacterThinkTimer))
				{
					CharacterManager.Think();
				}
				using (new ProfileMarker(FogOfWarUpdateTimer))
				{
					GameTerrain.Instance.FogOfWar.DeterministicUpdate();
				}
			}
			if (SkillsChangePopups.Count > 0)
			{
				bool flag2 = true;
				foreach (SkillsChangePopup skillsChangePopup in SkillsChangePopups)
				{
					if (skillsChangePopup.Displayed < (float)(skillsChangePopup.SkillChanges.Count + 1))
					{
						float displayed = skillsChangePopup.Displayed;
						skillsChangePopup.Displayed = Math.Min(skillsChangePopup.Displayed + 1f / 60f / SkillsChangeBehaviour.StarTime, skillsChangePopup.SkillChanges.Count + 1);
						if ((int)displayed != (int)skillsChangePopup.Displayed && (int)skillsChangePopup.Displayed < skillsChangePopup.SkillChanges.Count + 1)
						{
							SoundManager.PlayMenuSoundFromList(SoundManager.NotificationSounds);
						}
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					if (WantLoadNewMapAfterSkillsChange)
					{
						if (IsPartyLeader())
						{
							WantFinish = WantFinishState.LoadNewMap;
						}
					}
					else
					{
						SkillsChangePopups.Clear();
					}
				}
			}
			TakeSnapshotIfNecessary();
			GameProfilerFolder.UpdateAll(Frame, !IsPaused());
		}
	}

	public SkillsChangePopup FindSkillsChangePopupForCharacter(Character character)
	{
		foreach (SkillsChangePopup skillsChangePopup in SkillsChangePopups)
		{
			if (skillsChangePopup.Character == character)
			{
				return skillsChangePopup;
			}
		}
		return null;
	}

	private bool ShouldAutoSave()
	{
		if (PlayTime < LastSavedTime + TimeSpan.FromMinutes(GameImpl.Instance.Settings.MinutesBetweenAutosaves))
		{
			return false;
		}
		if (PlayTime < LastSavedTime + TimeSpan.FromMinutes(GameImpl.Instance.Settings.MinutesBetweenAutosaves * 2f))
		{
			if (GameImpl.UnscaledTime - InputFunctionManager.Instance.LastInputTime < 1f)
			{
				return false;
			}
			if (PlaySpeed > PlaySpeed.Normal)
			{
				return false;
			}
			if (GameCursor.CachedIsAnyoneOnScreenInCombat)
			{
				return false;
			}
			if (Hud.Pip.FocusObjectIsInteresting)
			{
				return false;
			}
		}
		return true;
	}

	public void SetGameOver()
	{
		if (DifficultySettings.SaveTokensRequired)
		{
			SaveGameManager.Instance.DeleteCurrentSave(GameUniqueId);
		}
		GameFinishedState = GameFinishedState.GameOver;
	}

	public void NonDeterministicFixedUpdate(InputFrame handleInputFrame)
	{
		using (new ProfileMarker(NonDeterministicFixedUpdateTimer))
		{
			if (IsInMultiplayerGame())
			{
				int howManyInputFramesToProcessThisFrame = OnlineParty.Instance.GetHowManyInputFramesToProcessThisFrame(this);
				for (int i = 0; i < howManyInputFramesToProcessThisFrame; i++)
				{
					DeterministicRand.Locked = false;
					PreProcessInputFrame();
					for (int j = 0; j < NetworkPlayerIDs.Count; j++)
					{
						PartyMember partyMemberByID = OnlineParty.Instance.GetPartyMemberByID(NetworkPlayerIDs[j]);
						InputFrame inputFrame = partyMemberByID.GetInputFrame(InputFrame);
						if (PlayingInputs != null && inputFrame.Frame < PlayingInputs.Players[j].Frames.Count)
						{
							PlayingInputs.Players[j].Frames[inputFrame.Frame].Actions.CopyToList(inputFrame.Actions);
						}
						ProcessInputFrame(partyMemberByID, inputFrame);
						if (RecordingInputs != null)
						{
							RecordingInputs.Players[j].Frames.Add(new InputsRecord.Frame(inputFrame.Actions));
						}
					}
					PostProcessInputFrame();
					DeterministicUpdate();
					DeterministicRand.Locked = true;
				}
				using (new ProfileMarker(PredictionTimer))
				{
					if (handleInputFrame.Frame != -1)
					{
						if (howManyInputFramesToProcessThisFrame > 0)
						{
							PredictedObjectManager.CopyCurrentSessionState();
						}
						PredictedObjectManager.PredictFrames(handleInputFrame.Frame);
					}
				}
			}
			else if (!Editor && handleInputFrame.Frame != -1)
			{
				DeterministicRand.Locked = false;
				PreProcessInputFrame();
				if (PlayingInputs != null && handleInputFrame.Frame < PlayingInputs.Players[0].Frames.Count)
				{
					PlayingInputs.Players[0].Frames[handleInputFrame.Frame].Actions.CopyToList(handleInputFrame.Actions);
				}
				ProcessInputFrame(null, handleInputFrame);
				if (RecordingInputs != null)
				{
					RecordingInputs.Players[0].Frames.Add(new InputsRecord.Frame(handleInputFrame.Actions));
				}
				PostProcessInputFrame();
				DeterministicUpdate();
				DeterministicRand.Locked = true;
			}
			GameCamera.Update(1f / 60f);
		}
	}

	public bool IsInFocusArea(TerrainRect rect)
	{
		return rect.Overlaps(new TerrainRect(LastFocusTile - new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView), LastFocusTile + new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView)));
	}

	public void NonDeterministicUpdate()
	{
		using (new ProfileMarker(NonDeterministicUpdateTimer))
		{
			GameTerrain.Instance.FogOfWar.UpdateTexture();
			GameTerrain.Instance.UpdateMinimapTexIfNeeded();
			GameTerrain.Instance.UpdateHeightMapTexIfNeeded();
			bool flag = WantFinish == WantFinishState.None && !GameImpl.Instance.IsMenuOpen() && CountdownToTimeJump == 0 && SkillsChangePopups.Count == 0 && HudBehaviour.Instance.SkillsChangePopupTransition == 0f;
			Transition = Mathf.Clamp(Transition + (flag ? 1f : (-1f)) * Time.unscaledDeltaTime / (WantSlowerTransitionIn ? 1f : BaseMenu.FadeTime), 0f, 1f);
			if (WantSlowerTransitionIn && SkillsChangePopups.Count == 0 && HudBehaviour.Instance.SkillsChangePopupTransition == 0f && (Transition >= 1f || !flag))
			{
				WantSlowerTransitionIn = false;
			}
			if (IsInMultiplayerGame() && !PredictedObjectManager.HasPredictedAnyFramesSinceLastNonDeterministicUpdate)
			{
				FramesWithoutPrediction++;
			}
			else
			{
				FramesWithoutPrediction = 0;
			}
			if (IsPaused() || FramesWithoutPrediction >= MaxFramesWithoutPrediction)
			{
				Time.timeScale = 0f;
			}
			else
			{
				switch (PlaySpeed)
				{
				case PlaySpeed.Normal:
					Time.timeScale = 1f;
					break;
				case PlaySpeed.FastForwardx2:
					Time.timeScale = 2f;
					break;
				case PlaySpeed.FastForwardx4:
					Time.timeScale = 4f;
					break;
				}
			}
			using (new ProfileMarker(UpdateVisibleObjectsTimer))
			{
				GameTerrain instance = GameTerrain.Instance;
				TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(GameCamera.Focus);
				TerrainCoord terrainCoord = LastFocusTile / 8;
				TerrainCoord terrainCoord2 = tileCoordForPos / 8;
				TerrainRect terrainRect = new TerrainRect(LastFocusTile - new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView), LastFocusTile + new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView));
				TerrainRect terrainRect2 = new TerrainRect(tileCoordForPos - new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView), tileCoordForPos + new TerrainCoord(UnityActivationRangeMainView, UnityActivationRangeMainView));
				bool flag2 = instance.ClampMinMaxTileWithinBounds(ref terrainRect.min, ref terrainRect.max);
				bool flag3 = instance.ClampMinMaxTileWithinBounds(ref terrainRect2.min, ref terrainRect2.max);
				if (terrainCoord2 != terrainCoord)
				{
					TerrainRect terrainRect3 = terrainRect / 8;
					TerrainRect terrainRect4 = terrainRect2 / 8;
					if (flag2)
					{
						for (int i = terrainRect3.min.x; i <= terrainRect3.max.x; i++)
						{
							for (int j = terrainRect3.min.y; j <= terrainRect3.max.y; j++)
							{
								if (terrainRect4.Contains(new TerrainCoord(i, j)))
								{
									continue;
								}
								List<TileObject> objectsInLookupSquareUsingLookupCoords = instance.GetObjectsInLookupSquareUsingLookupCoords(i, j);
								if (objectsInLookupSquareUsingLookupCoords == null)
								{
									continue;
								}
								foreach (TileObject item in objectsInLookupSquareUsingLookupCoords)
								{
									if (item.IsUnityObjectActive() && item.IsUnityObjectStatic() && !item.IsUnityObjectAlwaysActive() && !item.GetTileRect().Overlaps(terrainRect2))
									{
										item.UnityDeactivate();
									}
								}
							}
						}
					}
					for (int k = terrainRect4.min.x; k <= terrainRect4.max.x; k++)
					{
						for (int l = terrainRect4.min.y; l <= terrainRect4.max.y; l++)
						{
							if (terrainRect3.Contains(new TerrainCoord(k, l)))
							{
								continue;
							}
							List<TileObject> objectsInLookupSquareUsingLookupCoords2 = instance.GetObjectsInLookupSquareUsingLookupCoords(k, l);
							if (objectsInLookupSquareUsingLookupCoords2 == null)
							{
								continue;
							}
							foreach (TileObject item2 in objectsInLookupSquareUsingLookupCoords2)
							{
								if (!item2.IsUnityObjectActive() && !item2.IsBeingPredicted() && item2.IsUnityObjectStatic() && item2.CanUnityObjectBeActivated())
								{
									item2.UnityActivate();
								}
							}
						}
					}
				}
				if (WantFullRefreshOfActiveUnityObjectsInFocusArea)
				{
					tmp.Clear();
					instance.GetUnityObjectsInRectApprox(terrainRect2.min, terrainRect2.max, tmp);
					foreach (TileObject item3 in tmp)
					{
						if (item3.IsUnityObjectActive())
						{
							if (item3.IsUnityObjectStatic() && !item3.CanUnityObjectBeActivated())
							{
								item3.UnityDeactivate();
							}
						}
						else if (item3.IsUnityObjectStatic() && item3.CanUnityObjectBeActivated() && !item3.IsBeingPredicted())
						{
							item3.UnityActivate();
						}
					}
					tmp.Clear();
					WantFullRefreshOfActiveUnityObjectsInFocusArea = false;
				}
				PipCameraBehaviour.BuildListOfRenderedObjects(tmp);
				foreach (TileObject item4 in tmp)
				{
					item4.MarkWithinUnityActivationRange();
					if (!item4.IsUnityObjectActive() && !item4.IsBeingPredicted())
					{
						item4.UnityActivate();
					}
				}
				if (flag3)
				{
					instance.CharacterMapWho.GetObjectsInRect(terrainRect2.min, terrainRect2.max, tmp2);
					foreach (EnterableVehicle movingVehicle in Instance.PropManager.MovingVehicles)
					{
						if (movingVehicle.GetTileRect().Overlaps(terrainRect2))
						{
							tmp2.Add(movingVehicle);
						}
					}
					foreach (Projectile projectile in Instance.PropManager.Projectiles)
					{
						if (projectile.GetTileRect().Overlaps(terrainRect2))
						{
							tmp2.Add(projectile);
						}
					}
					foreach (TileObject item5 in tmp2)
					{
						if (item5.CanUnityObjectBeActivated())
						{
							item5.MarkWithinUnityActivationRange();
							if (!item5.IsUnityObjectActive() && !item5.IsBeingPredicted())
							{
								item5.UnityActivate();
							}
						}
					}
				}
				foreach (TileObject item6 in UnityObjectsVisibleInPip)
				{
					if (item6.IsUnityObjectActive() && !item6.WantUnityObjectToStayActive() && (!item6.GetTileRect().Overlaps(terrainRect2) || !item6.CanUnityObjectBeActivated()))
					{
						item6.UnityDeactivate();
					}
				}
				MathUtil.Swap(ref tmp, ref UnityObjectsVisibleInPip);
				MathUtil.Swap(ref tmp2, ref MovableUnityObjectsVisibleInMainView);
				tmp.Clear();
				tmp2.Clear();
				for (int m = 0; m < ActiveMovingUnityObjects.Count; m++)
				{
					TileObject tileObject = ActiveMovingUnityObjects[m];
					if (tileObject.WantUnityObjectToStayActive())
					{
						tileObject.UnityUpdate();
						continue;
					}
					_ = ActiveMovingUnityObjects.Count;
					tileObject.UnityDeactivate();
					m--;
				}
				foreach (TileObject item7 in StaticUnityObjectsThatNeedUpdate)
				{
					item7.UnityUpdate();
				}
				LastFocusTile = tileCoordForPos;
			}
			PredictedObjectManager.NonDeterministicUpdatePredictedObjects();
			GameImpl.Instance.FinishHandleInputOnThread();
			Hud.Update();
			HintManager.Update();
			GameImpl.Instance.Sun.SetupLightForTime(DaysSinceStart, useMiddayReflectionTex: false);
			Weather.NonDeterministicUpdate(Time.deltaTime);
			if (Transition == 1f)
			{
				switch (GameFinishedState)
				{
				case GameFinishedState.GameOver:
					WantFinish = WantFinishState.GameOverMenu;
					break;
				case GameFinishedState.GameComplete:
					WantFinish = WantFinishState.GameCompleteMenu;
					break;
				}
			}
			GameCamera.SkipZoomDistRaycast = false;
		}
	}

	public bool IsPaused()
	{
		return IsPaused(includeTransition: true);
	}

	public bool IsPaused(bool includeTransition)
	{
		return GetFramesPerFrame(includeTransition) == 0;
	}

	public int GetFramesPerFrame()
	{
		return GetFramesPerFrame(includeTransition: true);
	}

	public int GetFramesPerFrame(bool includeTransition)
	{
		if (!IsInMultiplayerGame())
		{
			if (Editor || GameImpl.Instance.IsMenuOpen() || NotificationManager.Instance.IsDisplayingNotification())
			{
				return 0;
			}
			if (includeTransition && Transition != 1f && CountdownToTimeJump == 0 && !WantSlowerTransitionIn)
			{
				return 0;
			}
		}
		if (DebugPaused && !DebugOneStep)
		{
			return 0;
		}
		if (SkillsChangePopups.Count > 0)
		{
			return 0;
		}
		return PlaySpeed switch
		{
			PlaySpeed.Paused => 0, 
			PlaySpeed.Normal => 1, 
			PlaySpeed.FastForwardx2 => 2, 
			PlaySpeed.FastForwardx4 => 4, 
			_ => 0, 
		};
	}

	public bool IsDebugMenuOpen()
	{
		if (DebugMenu != null)
		{
			return DebugMenu.State != DebugPageState.Inactive;
		}
		return false;
	}

	public static int CalcDayFromTime(TimeSpan time)
	{
		return (int)(time.Ticks / Sun.DayLength.Ticks) + 1;
	}

	public bool IsInMultiplayerGame()
	{
		return NetworkPlayerIDs.Count > 1;
	}

	public bool IsPartyLeader()
	{
		return GetLocalPlayerRecord()?.IsPartyLeader ?? false;
	}

	public bool IsInMultiplayerGameAsLeader()
	{
		if (NetworkPlayerIDs.Count > 1)
		{
			return GetLocalPlayerRecord().IsPartyLeader;
		}
		return false;
	}

	public bool IsInMultiplayerGameAsFollower()
	{
		if (NetworkPlayerIDs.Count > 1)
		{
			return !GetLocalPlayerRecord().IsPartyLeader;
		}
		return false;
	}

	public bool IsInMultiplayerGameAsCommunityLeader()
	{
		if (NetworkPlayerIDs.Count > 1)
		{
			PlayerRecord localPlayerRecord = GetLocalPlayerRecord();
			Character avatarForPlayer = CharacterManager.GetAvatarForPlayer(localPlayerRecord.PlayerID);
			if (avatarForPlayer != null)
			{
				return avatarForPlayer.Rank == Rank.Leader;
			}
			return false;
		}
		return false;
	}

	public bool IsInMultiplayerGameAsCommunityFollower()
	{
		if (NetworkPlayerIDs.Count > 1)
		{
			PlayerRecord localPlayerRecord = GetLocalPlayerRecord();
			Character avatarForPlayer = CharacterManager.GetAvatarForPlayer(localPlayerRecord.PlayerID);
			if (avatarForPlayer != null)
			{
				return avatarForPlayer.Rank != Rank.Leader;
			}
			return true;
		}
		return false;
	}

	public int GetInputFrameBeingSent()
	{
		if (!IsInMultiplayerGame())
		{
			return InputFrame + 1;
		}
		return OnlineParty.Instance.GetLocalPartyMember().ReceivedInputFrame + 1;
	}

	public void PreProcessInputFrame()
	{
		using (new ProfileMarker(PreProcessInputFrameTimer))
		{
			InputFrame++;
			foreach (PlayerRecord playerRecord in PlayerRecords)
			{
				if (playerRecord.PlayerMode != PlayerMode.Dormant)
				{
					playerRecord.LastUsedTime = PlayTime;
				}
				if (playerRecord.PlayerMode != PlayerMode.Dormant && playerRecord.PlayerMode != PlayerMode.CreatingCharacter && !playerRecord.FlyMode)
				{
					playerRecord.SyncedCamFocusPosXZ = playerRecord.PlayerCharacter.PosXZ;
				}
			}
			if (IsPaused())
			{
				return;
			}
			foreach (PlayerRecord playerRecord2 in PlayerRecords)
			{
				if (playerRecord2.PlayerMode == PlayerMode.Controlling)
				{
					playerRecord2.PlayerCharacter.UpdatePreProcessInputFrame(playerRecord2);
				}
			}
		}
	}

	public void PostProcessInputFrame()
	{
		using (new ProfileMarker(PostProcessInputFrameTimer))
		{
			if (IsPaused())
			{
				return;
			}
			foreach (PlayerRecord playerRecord in PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling)
				{
					playerRecord.PlayerCharacter.UpdatePostProcessInputFrame(playerRecord);
				}
			}
		}
	}

	public void ProcessInputFrame(PartyMember partyMember, InputFrame inputFrame)
	{
		PlayerRecord playerRecord = ((partyMember != null) ? GetPlayerRecord(partyMember.PlayerID) : GetLocalPlayerRecord());
		if (!inputFrame.HasActionsWhichYouMustLetGoOfToFireAgain())
		{
			playerRecord.MustLetGoToFireAgain = false;
		}
		for (int i = 0; i < inputFrame.Actions.Count; i++)
		{
			ProcessInputAction(playerRecord, inputFrame.Actions[i]);
		}
	}

	private void ProcessInputAction(PlayerRecord playerRecord, InputAction action)
	{
		switch (action.Type)
		{
		case InputActionType.Move:
			if (playerRecord.IsPlayerMoveable())
			{
				playerRecord.PlayerCharacter.OnProcessMoveAction(playerRecord, action.Dir);
			}
			break;
		case InputActionType.EnableSprint:
		case InputActionType.EnableSprintAutomatic:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessSprintAction(enable: true, action.Type == InputActionType.EnableSprintAutomatic);
			}
			break;
		case InputActionType.DisableSprint:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessSprintAction(enable: false, automatic: false);
			}
			break;
		case InputActionType.Fire:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessFireAction(playerRecord);
			}
			break;
		case InputActionType.Kick:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessKickAction(playerRecord);
			}
			break;
		case InputActionType.Vault:
			if (action.GetObject(Instance) is TileObject prop3 && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandVaultWaistHighWall(playerRecord, prop3, action.IsDoubleClick);
			}
			break;
		case InputActionType.PickUp:
		{
			TileObject tileObject12 = action.GetObject(Instance) as TileObject;
			if (tileObject12 != null && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandPickUp(playerRecord, tileObject12, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter) && tileObject12 != null)
					{
						GoTo(playerRecord, selectedCharacter, tileObject12.GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.Drop:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessDropAction();
			}
			break;
		case InputActionType.Grab:
		{
			TileObject tileObject = action.GetObject(Instance) as TileObject;
			if (tileObject != null && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandGrab(playerRecord, tileObject, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter2 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter2) && tileObject != null)
					{
						GoTo(playerRecord, selectedCharacter2, tileObject.GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.Bury:
		{
			Character character12 = action.GetObject(Instance) as Character;
			Prop prop4 = action.GetTo(Instance) as Prop;
			if (prop4 != null && character12 != null && playerRecord.IsPlayerControllable() && playerRecord.PlayerCharacter.CarryingObject == character12)
			{
				playerRecord.PlayerCharacter.CommandBury(playerRecord, character12, prop4, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter3 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter3) && prop4 != null)
					{
						GoTo(playerRecord, selectedCharacter3, prop4.GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.Eulogy:
		{
			Grave grave2 = action.GetObject(Instance) as Grave;
			if (grave2 != null && grave2.Corpse != null && !grave2.Corpse.EulogyGiven && playerRecord.IsPlayerControllable())
			{
				BaseObject obj = null;
				Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(playerRecord.PlayerCharacter, grave2.Corpse, ref obj, SpeechSituation.Eulogy, default(MemoryParam));
				if (speechForSituation3 != null)
				{
					playerRecord.PlayerCharacter.CommandSpeakTo(playerRecord, grave2.Corpse, obj, default(MemoryParam), speechForSituation3, null, null, default(MemoryParam), action.IsDoubleClick);
				}
			}
			{
				foreach (Character selectedCharacter4 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter4) && grave2 != null)
					{
						GoTo(playerRecord, selectedCharacter4, grave2.GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.VisitGrave:
		{
			Grave grave = action.GetObject(Instance) as Grave;
			if (grave != null && grave.Corpse != null && playerRecord.IsPlayerControllable() && Instance.PlayTime - grave.LastVisitedTime >= Grave.MinTimeBetweenVisits)
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(playerRecord.PlayerCharacter, grave.Corpse, null, SpeechSituation.VisitGrave);
				if (speechForSituation != null)
				{
					playerRecord.PlayerCharacter.CommandSpeakTo(playerRecord, grave.Corpse, null, default(MemoryParam), speechForSituation, null, null, default(MemoryParam), action.IsDoubleClick);
				}
			}
			{
				foreach (Character selectedCharacter5 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter5) && grave != null)
					{
						GoTo(playerRecord, selectedCharacter5, grave.GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.Parry:
		{
			Character attacker = action.GetObject(this) as Character;
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessParryAction(playerRecord, attacker, action.ActionAnim);
			}
			break;
		}
		case InputActionType.Escape:
		case InputActionType.EscapeHeld:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessEscapeAction(playerRecord, action.Type == InputActionType.EscapeHeld, action.IntAmount);
			}
			break;
		case InputActionType.Choke:
		case InputActionType.ChokeHeld:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessChokeAction(playerRecord, action.Type == InputActionType.ChokeHeld, action.IntAmount);
			}
			break;
		case InputActionType.Reload:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessReloadAction();
			}
			break;
		case InputActionType.Surrender:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessSurrenderAction(playerRecord);
			}
			break;
		case InputActionType.Crouch:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessCrouchAction();
			}
			break;
		case InputActionType.SetControlledCharacter:
			if (action.GetObject(this) is Character character13 && character13.IsControllableByPlayer())
			{
				SetControlledCharacter(playerRecord, character13);
			}
			break;
		case InputActionType.SetDesiredWeapon:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnProcessSetDesiredWeaponAction(playerRecord, action.GetObject(this) as Equipment, action.Prototype, action.InfectionType);
			}
			break;
		case InputActionType.SetTargetObject:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.SetTargetObject(action.GetObject(this) as TileObject, action.TargettableBodyLocation);
			}
			break;
		case InputActionType.SetTargetPos:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.TargetPos = action.HalfPos.ToVector3();
			}
			break;
		case InputActionType.SetThrowAngle:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.ThrowAngle = action.Angle;
				playerRecord.ThrowSpeed = action.FloatAmount;
			}
			break;
		case InputActionType.SetWantLockOnTarget:
			playerRecord.WantLockOnTarget = true;
			break;
		case InputActionType.ClearWantLockOnTarget:
			playerRecord.WantLockOnTarget = false;
			break;
		case InputActionType.SetTargetBodyLocationToAimFor:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.SetTargetBodyLocationToAimFor(action.TargettableBodyLocation);
			}
			break;
		case InputActionType.EnterFlyMode:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.OnStopDirectControl();
				playerRecord.WantLockOnTarget = false;
				playerRecord.TargetBodyLocationToAimFor = TargettableBodyLocation.Torso;
				playerRecord.TargetObject = null;
			}
			playerRecord.FlyMode = true;
			break;
		case InputActionType.LeaveFlyMode:
			playerRecord.FlyMode = false;
			break;
		case InputActionType.SetSyncedCamPosXZ:
			playerRecord.SyncedCamFocusPosXZ = action.PosXZ;
			break;
		case InputActionType.SetSyncedCamAngle:
			playerRecord.SyncedCamAngle = action.Angle;
			break;
		case InputActionType.SetSyncedCamZoom:
			playerRecord.SyncedFlyModeZoomDist = action.FloatAmount;
			break;
		case InputActionType.SetSyncedPipObj:
			playerRecord.SyncedPipObj = action.GetObject(this) as TileObject;
			break;
		case InputActionType.SetSyncedIsNavigatingMenus:
			playerRecord.SyncedIsNavigatingMenus = action.Value;
			break;
		case InputActionType.SetSyncedInInfoScreen:
			playerRecord.SyncedIsInInfoScreen = action.Value;
			if (!playerRecord.SyncedIsInInfoScreen)
			{
				playerRecord.JustSetRoleToUrgent = false;
			}
			if (!IsAnyoneInInfoScreen())
			{
				CanFastForwardEvenInCombat = false;
			}
			break;
		case InputActionType.IncreasePlaySpeed:
			PlaySpeed = ((PlaySpeed == PlaySpeed.FastForwardx4) ? PlaySpeed.Normal : (PlaySpeed + 1));
			WasPausedAutomatically = false;
			if (PlaySpeed <= GetSlowSpeed())
			{
				CanFastForwardEvenInCombat = false;
			}
			break;
		case InputActionType.DecreasePlaySpeed:
			PlaySpeed = ((PlaySpeed == PlaySpeed.Paused) ? PlaySpeed.Normal : (PlaySpeed - 1));
			WasPausedAutomatically = false;
			if (PlaySpeed <= GetSlowSpeed())
			{
				CanFastForwardEvenInCombat = false;
			}
			break;
		case InputActionType.SetPlaySpeed:
			PlaySpeed = action.PlaySpeed;
			WasPausedAutomatically = PlaySpeed == PlaySpeed.Paused;
			if (PlaySpeed <= GetSlowSpeed())
			{
				CanFastForwardEvenInCombat = false;
			}
			break;
		case InputActionType.SetCanFastForwardEvenInCombat:
			CanFastForwardEvenInCombat = action.Value;
			break;
		case InputActionType.GoTo:
			if (playerRecord.IsPlayerControllable())
			{
				GoTo(playerRecord, playerRecord.PlayerCharacter, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter6 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter6))
					{
						GoTo(playerRecord, selectedCharacter6, action.Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.GoToAndEnterBuilding:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Building)
			{
				GoToAndEnterBuilding(playerRecord, playerRecord.PlayerCharacter, action.GetObject(this) as Building, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter7 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter7))
					{
						GoToAndEnterBuilding(playerRecord, selectedCharacter7, action.GetObject(this) as Building, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.ChangeBuildingSlot:
			if (playerRecord.IsPlayerControllable())
			{
				ChangeBuildingSlot(playerRecord, playerRecord.PlayerCharacter, action.IntAmount);
			}
			break;
		case InputActionType.ExitBuilding:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandLeaveBuilding(playerRecord, action.IntAmount, action.IsDoubleClick);
			}
			break;
		case InputActionType.EnterBuilding:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Building)
			{
				EnterBuilding(playerRecord, playerRecord.PlayerCharacter, action.GetObject(this) as Building, action.IsDoubleClick);
			}
			break;
		case InputActionType.FollowMe:
			if (playerRecord.IsPlayerControllableIgnoringAIOverridesControl() && action.GetObject(this) is TileObject)
			{
				FollowMe(playerRecord.PlayerCharacter, action.GetObject(this) as TileObject);
			}
			break;
		case InputActionType.StopFollowingMe:
			if (playerRecord.IsPlayerControllableIgnoringAIOverridesControl() && action.GetObject(this) is TileObject)
			{
				StopFollowingMe(playerRecord.PlayerCharacter, action.GetObject(this) as TileObject);
			}
			break;
		case InputActionType.GroupFollowMe:
			if (!playerRecord.IsPlayerControllableIgnoringAIOverridesControl())
			{
				break;
			}
			{
				foreach (Character selectedCharacter8 in playerRecord.SelectedCharacters)
				{
					if (selectedCharacter8 != playerRecord.PlayerCharacter && selectedCharacter8.CanFollowPlayerIncludeAllies())
					{
						FollowMe(playerRecord.PlayerCharacter, selectedCharacter8);
					}
				}
				break;
			}
		case InputActionType.GroupStopFollowingMe:
			if (!playerRecord.IsPlayerControllableIgnoringAIOverridesControl())
			{
				break;
			}
			{
				foreach (Character selectedCharacter9 in playerRecord.SelectedCharacters)
				{
					if (selectedCharacter9 != playerRecord.PlayerCharacter && selectedCharacter9.IsInSameSquad(playerRecord.PlayerCharacter))
					{
						StopFollowingMe(playerRecord.PlayerCharacter, selectedCharacter9);
					}
				}
				break;
			}
		case InputActionType.Attack:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandAttack(playerRecord, action.GetObject(this) as TileObject, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter10 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter10))
					{
						selectedCharacter10.CommandAttack(playerRecord, action.GetObject(this) as TileObject, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.RagdollMove:
			if (action.GetObject(this) is Character character6)
			{
				character6.OnRagdollMove(action.PosXZ, action.Angle);
			}
			break;
		case InputActionType.RagdollStopMoving:
			if (action.GetObject(this) is Character character5)
			{
				character5.OnRagdollStopMoving(action.PosXZ, action.Value, action.Angle);
			}
			break;
		case InputActionType.RigidBodyMove:
			if (action.GetObject(this) is TileObject tileObject6)
			{
				tileObject6.OnRigidBodyMove(action.Pos, action.Rot, 0f);
			}
			break;
		case InputActionType.VehicleMove:
			if (action.GetObject(this) is TileObject tileObject4)
			{
				tileObject4.OnRigidBodyMove(action.Pos, action.Rot, action.FloatAmount);
			}
			break;
		case InputActionType.RigidBodyStopMoving:
			if (action.GetObject(this) is TileObject tileObject2)
			{
				tileObject2.OnRigidBodyStopMoving(action.Pos, action.Rot);
			}
			break;
		case InputActionType.VehicleCollision:
		{
			EnterableVehicle enterableVehicle4 = action.GetFrom(this) as EnterableVehicle;
			BaseObject to = action.GetTo(this);
			if (enterableVehicle4 != null && to != null)
			{
				if (enterableVehicle4.IsSusceptibleToVehicleCollisions(juggernaut: false))
				{
					enterableVehicle4.OnVehicleCollision(to, action.RelativeVelocity, action.Pos, predicted: false);
				}
				if (to.IsSusceptibleToVehicleCollisions(enterableVehicle4.IsJuggernaut()))
				{
					to.OnVehicleCollision(enterableVehicle4, -action.RelativeVelocity, action.Pos, predicted: false);
				}
			}
			break;
		}
		case InputActionType.Horn:
			if (playerRecord.IsPlayerControllable() && playerRecord.PlayerCharacter.InsideBuilding is EnterableVehicle { IsDriveable: not false } enterableVehicle3)
			{
				enterableVehicle3.OnHorn();
			}
			break;
		case InputActionType.HandBrake:
			if (playerRecord.IsPlayerControllable() && playerRecord.PlayerCharacter.InsideBuilding is EnterableVehicle { IsDriveable: not false } enterableVehicle2)
			{
				enterableVehicle2.OnHandBrake();
			}
			break;
		case InputActionType.ScavengingFinished:
			playerRecord.SyncedSwappingSupplies = null;
			playerRecord.SyncedSwappingSuppliesWith = null;
			playerRecord.SyncedSwappingSuppliesMode = SwappingSuppliesMode.None;
			break;
		case InputActionType.MarkInvestigated:
			if (action.GetObject(this) is TileObject)
			{
				(action.GetObject(this) as TileObject).MarkInvestigated(action.GetFrom(this) as Character);
			}
			break;
		case InputActionType.SetSyncedInventorySortBy:
			playerRecord.SyncedInventorySortBy = action.SortBy;
			break;
		case InputActionType.SetSyncedInventorySortOrder:
			playerRecord.SyncedInventorySortOrder = action.SortOrder;
			break;
		case InputActionType.SetSyncedCharactersSortBy:
			playerRecord.SyncedCharactersSortBy = action.SortCharactersBy;
			break;
		case InputActionType.SetSyncedCharactersSortOrder:
			playerRecord.SyncedCharactersSortOrder = action.SortOrder;
			break;
		case InputActionType.EquipmentTransfer:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is TileObject && action.GetTo(this) is TileObject)
			{
				EquipmentTransfer(action.GetObject(this) as Equipment, action.GetFrom(this) as TileObject, action.GetTo(this) as TileObject, action.IntAmount, playerRecord);
			}
			break;
		case InputActionType.EquipmentPickpocket:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is TileObject && action.GetTo(this) is TileObject)
			{
				EquipmentTransfer(action.GetObject(this) as Equipment, action.GetFrom(this) as TileObject, action.GetTo(this) as TileObject, action.IntAmount, action.FloatAmount, playerRecord);
			}
			break;
		case InputActionType.EquipmentTrade:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is Character && action.GetTo(this) is Character)
			{
				EquipmentTrade(action.GetObject(this) as Equipment, action.GetFrom(this) as Character, action.GetTo(this) as Character, action.IntAmount, playerRecord, fromTradingScreen: true);
			}
			break;
		case InputActionType.ConfirmTrade:
			if (action.GetFrom(this) is Character && action.GetTo(this) is Character)
			{
				ConfirmTrade(action.PendingTrades, action.GetFrom(this) as Character, action.GetTo(this) as Character, action.Value, playerRecord);
			}
			break;
		case InputActionType.EquipmentDestroy:
		{
			Equipment equipment6 = action.GetObject(this) as Equipment;
			TileObject tileObject10 = action.GetFrom(this) as TileObject;
			Character character11 = action.GetTo(this) as Character;
			if (equipment6 != null && tileObject10 != null)
			{
				float value = equipment6.GetBasePrice() * (float)equipment6.GetAmount();
				EquipmentDestroy(equipment6, tileObject10, playerRecord.IsLocal, action.IntAmount);
				if (character11 != null && tileObject10 != null)
				{
					character11.OnStoleSomething(tileObject10.GetCommunity(), tileObject10 as Character, value);
				}
			}
			break;
		}
		case InputActionType.EquipmentPourAway:
		{
			Equipment equipment5 = action.GetObject(this) as Equipment;
			TileObject tileObject9 = action.GetFrom(this) as TileObject;
			Character character9 = action.GetTo(this) as Character;
			if (equipment5 != null && tileObject9 != null)
			{
				float liquidContentsPrice2 = equipment5.GetLiquidContentsPrice();
				EquipmentPourAway(equipment5, tileObject9);
				if (character9 != null && tileObject9 != null)
				{
					character9.OnStoleSomething(tileObject9.GetCommunity(), tileObject9 as Character, liquidContentsPrice2);
				}
			}
			break;
		}
		case InputActionType.EquipmentPourInto:
		{
			Character character8 = action.GetObject(this) as Character;
			Equipment equipment3 = action.GetFrom(this) as Equipment;
			Equipment equipment4 = action.GetTo(this) as Equipment;
			if (equipment3 != null && equipment4 != null)
			{
				float liquidContentsPrice = equipment3.GetLiquidContentsPrice();
				EquipmentPourInto(equipment3, equipment4);
				if (character8 != null && equipment3.InventoryOwner != null)
				{
					character8.OnStoleSomething(equipment3.InventoryOwner.GetCommunity(), equipment3.InventoryOwner as Character, liquidContentsPrice);
				}
			}
			break;
		}
		case InputActionType.EquipmentUse:
		{
			Equipment equipment2 = action.GetObject(this) as Equipment;
			TileObject tileObject8 = (TileObject)action.GetFrom(this);
			Character character7 = action.GetTo(this) as Character;
			if (equipment2 != null && character7 != null && tileObject8 != null && tileObject8.InventoryContains(action.GetObject(this) as Equipment))
			{
				float basePrice = equipment2.GetBasePrice();
				equipment2.OnUsedFromInfoScreen(playerRecord, character7, tileObject8);
				character7.OnStoleSomething(tileObject8.GetCommunity(), tileObject8 as Character, basePrice);
			}
			break;
		}
		case InputActionType.EquipmentEquip:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is Character && ((Character)action.GetFrom(this)).InventoryContains(action.GetObject(this) as Equipment))
			{
				(action.GetObject(this) as Equipment).Equip(action.GetFrom(this) as Character);
			}
			break;
		case InputActionType.EquipmentUnequip:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is Character && ((Character)action.GetFrom(this)).InventoryContains(action.GetObject(this) as Equipment))
			{
				(action.GetObject(this) as Equipment).Unequip(action.GetFrom(this) as Character);
			}
			break;
		case InputActionType.EquipmentWear:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is Character && ((Character)action.GetFrom(this)).InventoryContains(action.GetObject(this) as Equipment))
			{
				(action.GetObject(this) as Equipment).Wear(action.GetFrom(this) as Character);
			}
			break;
		case InputActionType.EquipmentStrip:
			if (action.GetObject(this) is Equipment && action.GetFrom(this) is Character && ((Character)action.GetFrom(this)).InventoryContains(action.GetObject(this) as Equipment))
			{
				(action.GetObject(this) as Equipment).Strip(action.GetFrom(this) as Character);
			}
			break;
		case InputActionType.EquipmentLoadAmmo:
			if (action.GetObject(this) is AmmoWeapon && action.GetFrom(this) is TileObject)
			{
				EquipmentLoadAmmo(action.GetObject(this) as AmmoWeapon, action.GetFrom(this) as TileObject, action.Prototype, playerRecord.IsLocal);
			}
			break;
		case InputActionType.EquipmentUnloadAmmo:
			if (action.GetObject(this) is AmmoWeapon && action.GetFrom(this) is TileObject)
			{
				EquipmentUnloadAmmo(action.GetObject(this) as AmmoWeapon, action.GetFrom(this) as TileObject, playerRecord.IsLocal);
			}
			break;
		case InputActionType.EquipmentLightFuse:
			if (action.GetObject(this) is PipeBomb && action.GetFrom(this) is TileObject && playerRecord.IsPlayerControllable())
			{
				((PipeBomb)action.GetObject(this)).LightFuse(playerRecord.PlayerCharacter);
			}
			break;
		case InputActionType.EquipmentSpawn:
			if (action.GetObject(this) is TileObject tileObject11 && tileObject11.GetInventory() != null)
			{
				Equipment equipment8 = Equipment.Spawn(action.Prototype);
				if (equipment8 != null)
				{
					tileObject11.GetInventory().Add(tileObject11, equipment8);
				}
			}
			break;
		case InputActionType.Craft:
			if (action.GetFrom(this) is Character character14)
			{
				Equipment equipment7 = action.GetObject(this) as Equipment;
				if (action.ObjectId == 0 || (equipment7 != null && character14.Inventory.Contains(equipment7)))
				{
					character14.CommandCraft(action.Recipe, equipment7, action.Value, action.IntAmount, action.Tile, action.OrientationType, action.IsDoubleClick);
				}
			}
			break;
		case InputActionType.ResumeCrafting:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is CraftingProp)
			{
				playerRecord.PlayerCharacter.CommandResumeCrafting(action.GetObject(this) as CraftingProp, action.IsDoubleClick);
			}
			break;
		case InputActionType.Take:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandTake(playerRecord, action.GetObject(this) as TileObject, action.IsDoubleClick, playerRecord.PlayerID);
			}
			break;
		case InputActionType.TakeAll:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandTakeAll(playerRecord, action.GetObject(this) as TileObject, action.IsDoubleClick, playerRecord.PlayerID);
			}
			break;
		case InputActionType.Open:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Gate)
			{
				(action.GetObject(this) as Gate).Open(playerRecord.PlayerCharacter);
			}
			break;
		case InputActionType.Close:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Gate)
			{
				(action.GetObject(this) as Gate).Close(playerRecord.PlayerCharacter);
			}
			break;
		case InputActionType.Knock:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Gate)
			{
				playerRecord.PlayerCharacter.CommandInteract(playerRecord, action.GetObject(this) as Gate, InteractionType.Knock, null, action.IsDoubleClick);
			}
			break;
		case InputActionType.Unlock:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Gate)
			{
				playerRecord.PlayerCharacter.CommandInteract(playerRecord, action.GetObject(this) as Gate, InteractionType.Unlock, null, action.IsDoubleClick);
			}
			break;
		case InputActionType.Demolish:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				Demolish(playerRecord.PlayerCharacter, action.GetObject(this) as TileObject, playSound: true, createDebris: true);
			}
			break;
		case InputActionType.Abandon:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				Abandon(playerRecord.PlayerCharacter, action.GetObject(this) as TileObject);
			}
			break;
		case InputActionType.VehiclePark:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is EnterableVehicle)
			{
				((EnterableVehicle)action.GetObject(this)).Park(Instance.DeterministicRand);
			}
			break;
		case InputActionType.Farm:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.Farm((action.GetObject(this) as TileObject).GetCentreTile(), resetFarmingGoal: true);
			}
			{
				foreach (Character selectedCharacter11 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter11))
					{
						selectedCharacter11.Farm((action.GetObject(this) as TileObject).GetCentreTile(), resetFarmingGoal: true);
					}
				}
				break;
			}
		case InputActionType.Gather:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Prop)
			{
				playerRecord.PlayerCharacter.Gather(action.GetObject(this) as Prop, action.Prototype);
			}
			{
				foreach (Character selectedCharacter12 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter12))
					{
						selectedCharacter12.Gather(action.GetObject(this) as Prop, action.Prototype);
					}
				}
				break;
			}
		case InputActionType.Guard:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Building)
			{
				playerRecord.PlayerCharacter.Guard(action.GetObject(this) as Building);
			}
			{
				foreach (Character selectedCharacter13 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter13))
					{
						selectedCharacter13.Guard(action.GetObject(this) as Building);
					}
				}
				break;
			}
		case InputActionType.Repair:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandRepair(action.GetObject(this) as TileObject);
			}
			break;
		case InputActionType.TakeOver:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandCapture(action.GetObject(this) as TileObject);
			}
			break;
		case InputActionType.Lumberjack:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.Lumberjack(action.GetObject(this) as TileObject);
			}
			{
				foreach (Character selectedCharacter14 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter14))
					{
						selectedCharacter14.Lumberjack(action.GetObject(this) as TileObject);
					}
				}
				break;
			}
		case InputActionType.SetMiner:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.SetMiner(action.GetObject(this) as TileObject, action.MineralType);
			}
			{
				foreach (Character selectedCharacter15 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter15))
					{
						selectedCharacter15.SetMiner(action.GetObject(this) as TileObject, action.MineralType);
					}
				}
				break;
			}
		case InputActionType.SetTrapper:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.SetTrapper(action.GetObject(this) as TileObject);
			}
			{
				foreach (Character selectedCharacter16 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter16))
					{
						selectedCharacter16.SetTrapper(action.GetObject(this) as TileObject);
					}
				}
				break;
			}
		case InputActionType.SetAnimalFeeder:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.SetAnimalFeeder();
			}
			{
				foreach (Character selectedCharacter17 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter17))
					{
						selectedCharacter17.SetAnimalFeeder();
					}
				}
				break;
			}
		case InputActionType.SetOrganizer:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.SetOrganizer();
			}
			{
				foreach (Character selectedCharacter18 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter18))
					{
						selectedCharacter18.SetOrganizer();
					}
				}
				break;
			}
		case InputActionType.SetMedic:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.SetMedic();
			}
			{
				foreach (Character selectedCharacter19 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter19))
					{
						selectedCharacter19.SetMedic();
					}
				}
				break;
			}
		case InputActionType.Cook:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Campfire)
			{
				playerRecord.PlayerCharacter.Cook(action.GetObject(this) as Campfire);
			}
			{
				foreach (Character selectedCharacter20 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter20))
					{
						selectedCharacter20.Cook(action.GetObject(this) as Campfire);
					}
				}
				break;
			}
		case InputActionType.ChopTree:
		{
			TileObject tileObject7 = action.GetObject(Instance) as TileObject;
			if (tileObject7 != null && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandChop(playerRecord, tileObject7, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter21 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter21))
					{
						selectedCharacter21.CommandChop(playerRecord, tileObject7, action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.ChopLog:
		{
			TileObject tileObject5 = action.GetObject(Instance) as TileObject;
			if (tileObject5 != null && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandChop(playerRecord, tileObject5, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter22 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter22))
					{
						selectedCharacter22.CommandChop(playerRecord, tileObject5, action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.Mine:
		{
			TileObject tileObject3 = action.GetObject(Instance) as TileObject;
			if (tileObject3 != null && playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandMine(playerRecord, tileObject3, action.MineralType, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter23 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter23))
					{
						selectedCharacter23.CommandMine(playerRecord, tileObject3, action.MineralType, action.IsDoubleClick);
					}
				}
				break;
			}
		}
		case InputActionType.CancelRole:
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).CancelRole(new RoleInfo(action.Role, action.Tile, action.Prototype, action.Recipe));
			}
			break;
		case InputActionType.PauseRole:
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).PauseRole(new RoleInfo(action.Role, action.Tile, action.Prototype, action.Recipe));
			}
			break;
		case InputActionType.ResumeRole:
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).ResumeRole(new RoleInfo(action.Role, action.Tile, action.Prototype, action.Recipe));
			}
			break;
		case InputActionType.ChangeRolePriority:
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).ChangeRolePriority(new RoleInfo(action.Role, action.Tile, action.Prototype, action.Recipe), action.IntAmount);
			}
			break;
		case InputActionType.SetRoleUrgent:
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).SetRoleUrgent(new RoleInfo(action.Role, action.Tile, action.Prototype, action.Recipe), action.Value, playerRecord);
			}
			break;
		case InputActionType.ResumeAll:
			if (action.ObjectId == 0)
			{
				foreach (Character member in CommunityManager.PlayerCommunity.Members)
				{
					if (member.AliveAndNotZombie)
					{
						member.ResumeAllRoles();
					}
				}
				break;
			}
			if (action.GetObject(this) is Character)
			{
				((Character)action.GetObject(this)).ResumeAllRoles();
			}
			break;
		case InputActionType.BuildHere:
			if (playerRecord.IsPlayerControllable() && action.Recipe != null)
			{
				BuildHere(playerRecord.PlayerCharacter, action.Recipe, action.Tile, action.OrientationType, action.IsDoubleClick);
			}
			break;
		case InputActionType.ResumeBuilding:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandBuild(action.GetObject(this) as TileObject, action.IsDoubleClick, justPlaced: false);
			}
			break;
		case InputActionType.StopBuilding:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.DirectControlled = true;
				playerRecord.PlayerCharacter.SetBuildGoalLastAttemptedTimeToNow();
				playerRecord.PlayerCharacter.CancelOneOffCrafting();
			}
			break;
		case InputActionType.AddMaterialToFire:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandAddMaterialToFire(playerRecord, action.GetObject(this) as Equipment, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter24 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter24))
					{
						selectedCharacter24.CommandAddMaterialToFire(playerRecord, action.GetObject(this) as Equipment, action.Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.PourInto:
		case InputActionType.PourOnto:
			if (playerRecord.IsPlayerControllable() && action.GetTo(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandWaterPlant(playerRecord, action.GetObject(this) as Equipment, action.GetTo(this) as TileObject, action.Type == InputActionType.PourOnto, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter25 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter25))
					{
						selectedCharacter25.CommandWaterPlant(playerRecord, action.GetObject(this) as Equipment, action.GetTo(this) as TileObject, action.Type == InputActionType.PourOnto, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.Harvest:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandHarvestCrops(playerRecord, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter26 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter26))
					{
						selectedCharacter26.CommandHarvestCrops(playerRecord, action.Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.Eat:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandEat(playerRecord, action.GetObject(this) as Equipment);
			}
			break;
		case InputActionType.Drink:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandDrink(playerRecord, action.GetObject(this) as Equipment);
			}
			break;
		case InputActionType.Use:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandUse(playerRecord, action.GetObject(this) as Equipment);
			}
			break;
		case InputActionType.DrinkFromRiver:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandDrinkFromRiver(playerRecord, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter27 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter27))
					{
						selectedCharacter27.CommandDrinkFromRiver(playerRecord, action.Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.Fill:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Equipment)
			{
				playerRecord.PlayerCharacter.CommandFillLiquidContainer(playerRecord, action.GetObject(this) as Equipment, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter28 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter28))
					{
						Equipment bestLiquidContainerToFill = selectedCharacter28.Inventory.GetBestLiquidContainerToFill(GameTerrain.Instance.IsTileRiver(action.Tile.x, action.Tile.y) ? LiquidPrototype.Water : LiquidPrototype.Snow);
						if (bestLiquidContainerToFill != null)
						{
							selectedCharacter28.CommandFillLiquidContainer(playerRecord, bestLiquidContainerToFill, action.Tile, action.IsDoubleClick);
						}
					}
				}
				break;
			}
		case InputActionType.ScoopSnow:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandScoopSnow(playerRecord, action.Tile, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter29 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter29))
					{
						selectedCharacter29.CommandScoopSnow(playerRecord, action.Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.Plant:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Equipment)
			{
				playerRecord.PlayerCharacter.CommandPlantCrops(playerRecord, action.GetObject(this) as Equipment, action.Tile, action.IsDoubleClick);
			}
			break;
		case InputActionType.SpeakTo:
			if (playerRecord.IsPlayerControllableIgnoringAIOverridesControl() && action.GetTo(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandSpeakTo(playerRecord, action.GetTo(this) as Character, action.GetObject(this), action.SpeechParam, action.Speech, action.ReplyTo, action.GetReplyToReferringTo(this), action.ReplyToParam, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter30 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter30) && action.GetTo(this) is Character)
					{
						GoTo(playerRecord, selectedCharacter30, ((Character)action.GetTo(this)).Tile, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.GiveGift:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Equipment && action.GetTo(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandGiveGift(playerRecord, action.GetTo(this) as Character, action.GetObject(this) as Equipment, action.IsDoubleClick);
			}
			break;
		case InputActionType.Interact:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Equipment && action.GetTo(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandInteract(playerRecord, action.GetTo(this) as TileObject, action.InteractionType, action.GetObject(this) as Equipment, action.IsDoubleClick);
			}
			break;
		case InputActionType.TalkToInhabitant:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Building && action.GetTo(this) is Character)
			{
				TalkToInhabitant(playerRecord.PlayerCharacter, action.GetObject(this) as Building, action.GetTo(this) as Character);
			}
			break;
		case InputActionType.Chat:
			if (playerRecord.IsPlayerControllable())
			{
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(playerRecord.PlayerCharacter, null, SpeechSituation.Chat);
				playerRecord.PlayerCharacter.Speak(speechForSituation2, null, null, default(MemoryParam), action.ChatSpeech, playerRecord.PlayerName);
			}
			break;
		case InputActionType.SkipConversation:
			if (playerRecord.IsPlayerControllableIgnoringAIOverridesControl() && action.GetObject(this) is Character)
			{
				SkipConversation(playerRecord.PlayerCharacter, action.GetObject(this) as Character);
			}
			break;
		case InputActionType.LightFire:
			if (playerRecord.IsPlayerControllable() && action.GetTo(this) is TileObject)
			{
				playerRecord.PlayerCharacter.CommandLightFire(playerRecord, action.GetObject(this) as Equipment, action.GetTo(this) as TileObject, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter31 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter31) && action.GetTo(this) is TileObject)
					{
						GoTo(playerRecord, selectedCharacter31, ((TileObject)action.GetTo(this)).GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.SitByFire:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.CommandSitByFire(playerRecord, action.GetObject(this) as Campfire, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter32 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter32))
					{
						selectedCharacter32.CommandSitByFire(playerRecord, action.GetObject(this) as Campfire, action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.RepairArmor:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is WorkBench)
			{
				playerRecord.PlayerCharacter.CommandRepairArmor(playerRecord, action.GetObject(this) as WorkBench, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter33 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter33) && action.GetObject(this) is WorkBench)
					{
						GoTo(playerRecord, selectedCharacter33, ((WorkBench)action.GetObject(this)).GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.Skin:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandSkin(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter34 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter34) && action.GetObject(this) is Character)
					{
						GoTo(playerRecord, selectedCharacter34, ((Character)action.GetObject(this)).GetTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.SetRecentActivityTalkToMe:
			if (playerRecord.IsPlayerControllable())
			{
				if (action.GetObject(this) is Character character10)
				{
					character10.SetRecentActivity(RecentActivityType.TalkTo, playerRecord.PlayerCharacter);
				}
				else
				{
					UnityEngine.Debug.Log("SetRecentActivityTalkToMe received but character not found: " + action.ObjectId);
				}
			}
			else
			{
				UnityEngine.Debug.Log("SetRecentActivityTalkToMe received but player record not controllable: " + action.ObjectId);
			}
			break;
		case InputActionType.ChokeHold:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandChokeHold(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick, HoldType.ChokeHold);
			}
			break;
		case InputActionType.SlitThroat:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandChokeHold(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick, HoldType.SlitThroat);
			}
			break;
		case InputActionType.Restrain:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandChokeHold(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick, HoldType.Restrain);
			}
			break;
		case InputActionType.BludgeonUnconscious:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandAttackUnconscious(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick, kill: false);
			}
			break;
		case InputActionType.KillUnconscious:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is Character)
			{
				playerRecord.PlayerCharacter.CommandAttackUnconscious(playerRecord, action.GetObject(this) as Character, action.IsDoubleClick, kill: true);
			}
			break;
		case InputActionType.SetCropsPatch:
			CropsManager.SetPlantableCropType(CommunityManager.PlayerCommunity.Id, action.Tile, action.PropPrototype);
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.Farm(action.Tile, resetFarmingGoal: false);
			}
			break;
		case InputActionType.SetCraftItemLimit:
			if (action.Prototype != null && action.GetObject(this) is Community)
			{
				((Community)action.GetObject(this)).SetCraftingLimit(action.Prototype, action.IntAmount);
			}
			break;
		case InputActionType.SetCraftLiquidLimit:
			if (action.Liquid != null && action.GetObject(this) is Community)
			{
				((Community)action.GetObject(this)).SetCraftingLimit(action.Liquid, action.IntAmount);
			}
			break;
		case InputActionType.SetCurrentQuest:
			playerRecord.SyncedCurrentQuest = action.GetObject(this) as QuestInstance;
			break;
		case InputActionType.SetMapMarkerTile:
		{
			for (int m = 0; m < playerRecord.MapMarkerLocations.Count; m++)
			{
				if (playerRecord.MapMarkerLocations[m].Type == action.MapMarkerType)
				{
					playerRecord.MapMarkerLocations.RemoveAt(m);
					break;
				}
			}
			playerRecord.MapMarkerLocations.Add(new MapMarkerLocation(action.MapMarkerType, action.Tile));
			break;
		}
		case InputActionType.ClearMapMarkerTile:
		{
			for (int l = 0; l < playerRecord.MapMarkerLocations.Count; l++)
			{
				if (playerRecord.MapMarkerLocations[l].Type == action.MapMarkerType && playerRecord.MapMarkerLocations[l].Tile == action.Tile)
				{
					playerRecord.MapMarkerLocations.RemoveAt(l);
				}
			}
			break;
		}
		case InputActionType.SetGeologicalMap:
			playerRecord.SyncedGeologicalMap = action.MineralType;
			break;
		case InputActionType.SetMapCamPos:
			playerRecord.SyncedMapCamPosition = action.Pos;
			break;
		case InputActionType.SetHint:
			playerRecord.SyncedHint[(int)action.HintType] = action.Hint;
			break;
		case InputActionType.SetDirectControlled:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.DirectControlled = true;
			}
			break;
		case InputActionType.AddSelectedCharacter:
			if (action.GetObject(this) is Character && !playerRecord.SelectedCharacters.Contains(action.GetObject(this) as Character))
			{
				playerRecord.SelectedCharacters.Add(action.GetObject(this) as Character);
			}
			break;
		case InputActionType.RemoveSelectedCharacter:
			if (action.GetObject(this) is Character)
			{
				playerRecord.SelectedCharacters.Remove(action.GetObject(this) as Character);
			}
			break;
		case InputActionType.ClearSelectedCharacters:
			playerRecord.SelectedCharacters.Clear();
			break;
		case InputActionType.AddFollowersToSelection:
		case InputActionType.SelectFollowers:
			if (!playerRecord.IsPlayerControllable() || playerRecord.PlayerCharacter.Followers == null)
			{
				break;
			}
			if (action.Type == InputActionType.SelectFollowers)
			{
				playerRecord.SelectedCharacters.Clear();
			}
			{
				foreach (Character follower in playerRecord.PlayerCharacter.Followers)
				{
					if (!playerRecord.SelectedCharacters.Contains(follower))
					{
						playerRecord.SelectedCharacters.Add(follower);
					}
				}
				break;
			}
		case InputActionType.SelectEveryone:
		{
			foreach (Character member2 in Instance.CommunityManager.PlayerCommunity.Members)
			{
				if (member2.AliveAndNotZombie && member2.GetBaseObjectType() == BaseObjectType.Human && !playerRecord.SelectedCharacters.Contains(member2))
				{
					playerRecord.SelectedCharacters.Add(member2);
				}
			}
			break;
		}
		case InputActionType.SetGatePolicy:
			if (action.GetObject(this) is Gate)
			{
				((Gate)action.GetObject(this)).SetGatePolicy(action.GatePolicy, overrideDefault: true);
			}
			break;
		case InputActionType.SetBlockAnimals:
			if (action.GetObject(this) is Gate)
			{
				((Gate)action.GetObject(this)).SetBlockAnimals(action.Value);
			}
			break;
		case InputActionType.SetAllGatesPolicy:
			if (action.GetObject(this) is Gate)
			{
				CommunityManager.PlayerCommunity.SetGatePolicy((Gate)action.GetObject(this), action.GatePolicy);
			}
			break;
		case InputActionType.SetPropName:
		{
			Prop prop2 = action.GetObject(this) as Prop;
			Animal animal = action.GetObject(this) as Animal;
			if (prop2 != null)
			{
				prop2.CustomName = action.ChatSpeech;
				prop2.PropNameVerified = StringStatus.Unverified;
				VerifiedStrings = false;
			}
			if (animal != null)
			{
				animal.SetFirstName(action.ChatSpeech);
				animal.NameKnown = true;
				animal.FirstNameVerified = StringStatus.Unverified;
				VerifiedStrings = false;
			}
			break;
		}
		case InputActionType.SetStoragePolicy:
			if (action.GetObject(this) is Prop prop)
			{
				prop.SetStoragePolicy(action.Prototype, action.Liquid, action.Value);
			}
			break;
		case InputActionType.SetDesignatedLiquid:
			if (action.GetObject(this) is Equipment equipment)
			{
				equipment.SetDesignatedLiquid(action.Liquid);
			}
			break;
		case InputActionType.QuickSaveUsingToken:
			QuickSaveUsingToken(playerRecord);
			break;
		case InputActionType.CreateCharacter:
			SpawnCharacter(playerRecord, action.CharacterCreationSettings);
			break;
		case InputActionType.ContinueGame:
			if (GameFinishedState == GameFinishedState.GameComplete)
			{
				GameFinishedState = GameFinishedState.Continuing;
			}
			break;
		case InputActionType.ResetTrap:
			if (playerRecord.IsPlayerControllable() && action.GetObject(this) is ITrap)
			{
				playerRecord.PlayerCharacter.CommandResetTrap(playerRecord, action.GetObject(this) as TileObject, action.IsDoubleClick);
			}
			{
				foreach (Character selectedCharacter35 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterControllable(selectedCharacter35) && action.GetObject(this) is ITrap)
					{
						GoTo(playerRecord, selectedCharacter35, ((TileObject)action.GetObject(this)).GetCentreTile(), action.IsDoubleClick);
					}
				}
				break;
			}
		case InputActionType.SetForceFullSnapshot:
			ForceFullSnapshot = action.Value;
			break;
		case InputActionType.SetEquipmentPolicy:
		{
			CommunityManager.PlayerCommunity.SetEquipmentPolicy(action.Prototype, action.Liquid, action.InfectionType, action.CommunityPolicy, action.CommunityTargetAmount);
			Character character4 = action.GetObject(this) as Character;
			if (character4 != null && character4.IsControllableByPlayer())
			{
				character4.SetEquipmentPolicy(action.Prototype, action.Liquid, action.InfectionType, action.CharacterPolicy, action.CharacterTargetAmount, action.Value);
			}
			{
				foreach (Character selectedCharacter36 in playerRecord.SelectedCharacters)
				{
					if (selectedCharacter36 != character4 && selectedCharacter36.IsControllableByPlayer())
					{
						selectedCharacter36.SetEquipmentPolicy(action.Prototype, action.Liquid, action.InfectionType, action.CharacterPolicy, action.CharacterTargetAmount, action.Value);
					}
				}
				break;
			}
		}
		case InputActionType.SetMovementZone:
			Hud.Instance.SentMovementZone = false;
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.SetMovementZone(action.Rect);
			}
			foreach (Character selectedCharacter37 in playerRecord.SelectedCharacters)
			{
				if (playerRecord.IsSelectedCharacterCommandable(selectedCharacter37))
				{
					selectedCharacter37.SetMovementZone(action.Rect);
				}
			}
			HintManager.Instance.Hints[21].MarkPerformed();
			break;
		case InputActionType.PauseMovementZone:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.PauseMovementZone();
			}
			{
				foreach (Character selectedCharacter38 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterCommandable(selectedCharacter38))
					{
						selectedCharacter38.PauseMovementZone();
					}
				}
				break;
			}
		case InputActionType.ResumeMovementZone:
			if (playerRecord.IsPlayerControllable())
			{
				playerRecord.PlayerCharacter.ResumeMovementZone();
			}
			{
				foreach (Character selectedCharacter39 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterCommandable(selectedCharacter39))
					{
						selectedCharacter39.ResumeMovementZone();
					}
				}
				break;
			}
		case InputActionType.CopyMovementZone:
			if (!playerRecord.IsPlayerControllable())
			{
				break;
			}
			{
				foreach (Character selectedCharacter40 in playerRecord.SelectedCharacters)
				{
					if (playerRecord.IsSelectedCharacterCommandable(selectedCharacter40))
					{
						selectedCharacter40.SetMovementZone(playerRecord.PlayerCharacter.MovementZone, playerRecord.PlayerCharacter.MovementZonePaused);
					}
				}
				break;
			}
		case InputActionType.SetShortcutGroup:
		{
			if (!(action.GetObject(this) is Character controlledCharacter))
			{
				break;
			}
			List<Character> list2 = new List<Character>();
			if (action.CharacterIds != null)
			{
				for (int k = 0; k < action.CharacterIds.Count; k++)
				{
					if (BaseObjectManager.FindBaseObjectByID(action.CharacterIds[k]) is Character item)
					{
						list2.Add(item);
					}
				}
			}
			playerRecord.SetShortcutGroup(action.IntAmount, controlledCharacter, list2);
			break;
		}
		case InputActionType.BrainScan:
			if (action.GetObject(this) is Character character3)
			{
				character3.BrainScanned = true;
				character3.SetAllPersonalitiesKnown();
				character3.SetAllSkillsKnown();
				Relationship.SetAllRelationshipsKnown(character3);
			}
			break;
		case InputActionType.PowerNap:
			if (action.GetObject(this) is Character character2)
			{
				character2.PowerNap();
				playerRecord.LastPowerNapTime = PlayTime;
			}
			break;
		case InputActionType.LoadNewMap:
		{
			if (!(action.GetObject(this) is EnterableVehicle enterableVehicle))
			{
				break;
			}
			GameTerrain.Instance.GetNearestHitTheRoadPoint(enterableVehicle.PosXZ, float.MaxValue, out var traits);
			Character[] inhabitants = enterableVehicle.Inhabitants;
			foreach (Character character in inhabitants)
			{
				if (character == null || !character.AliveAndNotZombie || character.GetBaseObjectType() != BaseObjectType.Human || character.Skillset.IsAtMaxCaps())
				{
					continue;
				}
				SkillsChangePopup skillsChangePopup = new SkillsChangePopup();
				skillsChangePopup.Character = character;
				List<SkillType> list = new List<SkillType>();
				for (SkillType skillType = SkillType.Strength; skillType < SkillType.Count; skillType++)
				{
					if (character.Skillset.GetCap(skillType) < 5)
					{
						list.Add(skillType);
					}
				}
				for (int j = 0; j < traits.SkillCapBonus; j++)
				{
					if (list.Count <= 0)
					{
						break;
					}
					int index = DeterministicRand.Next(list.Count);
					SkillType skillType2 = list[index];
					int num = skillsChangePopup.FindSkillChange(skillType2);
					SkillChange skillChange;
					if (num != -1)
					{
						skillChange = skillsChangePopup.SkillChanges[num];
						skillChange.NewCap++;
						skillsChangePopup.SkillChanges[num] = skillChange;
					}
					else
					{
						skillChange = default(SkillChange);
						skillChange.Skill = skillType2;
						skillChange.OldLevel = (skillChange.NewLevel = character.Skillset.GetLevel(skillType2));
						skillChange.OldCap = character.Skillset.GetCap(skillType2);
						skillChange.NewCap = skillChange.OldCap + 1;
						skillsChangePopup.SkillChanges.Add(skillChange);
					}
					if (skillChange.NewCap >= 5)
					{
						list.RemoveAt(index);
					}
				}
				if (character.Consciousness == Consciousness.Sleeping)
				{
					character.SetConsciousness(Consciousness.Conscious);
				}
				SkillsChangePopups.Add(skillsChangePopup);
				WantLoadNewMapAfterSkillsChange = true;
			}
			if (SkillsChangePopups.Count == 0)
			{
				WantFinish = WantFinishState.LoadNewMap;
			}
			break;
		}
		case InputActionType.SetGatherDepot_DEPRECATED:
			break;
		}
	}

	private void GoTo(PlayerRecord playerRecord, Character controlledCharacter, TerrainCoord tile, bool isDoubleClick)
	{
		controlledCharacter.PauseAllRoles();
		controlledCharacter.SetHangoutLocation(tile);
		if (controlledCharacter.InsideBuilding != null)
		{
			controlledCharacter.CommandLeaveBuildingAndGoTo(playerRecord, tile, isDoubleClick);
		}
		else
		{
			controlledCharacter.CommandMoveTo(playerRecord, tile, isDoubleClick);
		}
	}

	private void GoToAndEnterBuilding(PlayerRecord playerRecord, Character controlledCharacter, Building building, bool isDoubleClick)
	{
		controlledCharacter.PauseAllRoles();
		controlledCharacter.SetHangoutLocation(building.Tile);
		controlledCharacter.CommandEnterBuilding(playerRecord, building, isDoubleClick);
	}

	private void EnterBuilding(PlayerRecord playerRecord, Character controlledCharacter, Building building, bool isDoubleClick)
	{
		controlledCharacter.SetHangoutLocation(building.Tile);
		controlledCharacter.CommandEnterBuilding(playerRecord, building, isDoubleClick);
	}

	private void ChangeBuildingSlot(PlayerRecord playerRecord, Character controlledCharacter, int slotIndex)
	{
		if (controlledCharacter.InsideBuilding != null)
		{
			controlledCharacter.InsideBuilding.OnCharacterSlotChange(controlledCharacter, slotIndex);
			controlledCharacter.DirectControlled = true;
			controlledCharacter.MarkPlayerControlled();
			controlledCharacter.CancelOneOffCrafting();
		}
	}

	private void TalkToInhabitant(Character controlledCharacter, Building targetBuilding, Character talkTo)
	{
		int closestEntranceTo = targetBuilding.GetClosestEntranceTo(controlledCharacter.Tile);
		for (int i = 0; i < targetBuilding.Inhabitants.Length; i++)
		{
			Character character = targetBuilding.Inhabitants[i];
			if (character == talkTo)
			{
				targetBuilding.OnCharacterLeave(character, closestEntranceTo, fromBuildingDestroyed: false, fromRagdolled: false);
				character.SetRecentActivity(RecentActivityType.Doorbell, controlledCharacter);
			}
		}
	}

	private void SkipConversation(Character controlledCharacter, Character target)
	{
		if ((!(target.FindActiveGoal(GoalType.Conversation) is Conversation conversation) || !conversation.Skip(target)) && target.IsSpeechSkippable())
		{
			target.SkipSpeech(onlySkipLipsMoving: false);
		}
	}

	private void FollowMe(Character controlledCharacter, TileObject target)
	{
		controlledCharacter.MakeMeGroupLeader();
		controlledCharacter.CancelOneOffCrafting();
		if (target is Character character)
		{
			character.FollowPlayer(controlledCharacter);
		}
		if (!(target is Building { Inhabitants: var inhabitants }))
		{
			return;
		}
		foreach (Character character2 in inhabitants)
		{
			if (character2 != null && character2 != controlledCharacter && (character2.IsControllableByPlayer() || character2.CanFollowPlayer || controlledCharacter.Community.CachedAllies.Contains(character2.Community)))
			{
				character2.FollowPlayer(controlledCharacter);
			}
		}
	}

	private void StopFollowingMe(Character controlledCharacter, TileObject target)
	{
		controlledCharacter.MakeMeGroupLeader();
		if (target is Character character)
		{
			character.Follow(null);
		}
		if (!(target is Building { Inhabitants: var inhabitants }))
		{
			return;
		}
		foreach (Character character2 in inhabitants)
		{
			if (character2 != null && character2.SquadLeader == controlledCharacter)
			{
				character2.Follow(null);
			}
		}
	}

	public void ClearAreaForBuilding(Community builderCommunity, TileObject building)
	{
		CropsManager.ClearAllCropPatchesFromRect(building.GetMinTile(), building.GetMaxTile());
		ClearTrashForBuilding(building.GetMinTile(), building.GetMaxTile(), builderCommunity, building);
	}

	public void ClearGrassForBuilding(TileObject building)
	{
		if (building.Id >= FirstObjectIdSpawnedDuringPlay)
		{
			PropPrototype propPrototype = building.GetPropPrototype();
			if (propPrototype != null && propPrototype.WantClearGrass && !propPrototype.WantFlattenTerrain)
			{
				int num = ((building is Campfire) ? 1 : 0);
				GameTerrain.Instance.ClearAllGrassInRect(building.GetMinTile() - new TerrainCoord(num, num), building.GetMaxTile() + new TerrainCoord(num, num));
			}
		}
	}

	public void ClearTrashForBuilding(TerrainCoord minTile, TerrainCoord maxTile, Community builderCommunity, TileObject newBuilding)
	{
		List<TileObject> list = new List<TileObject>();
		GameTerrain.Instance.GetObjectsInRect(minTile, maxTile, list);
		foreach (TileObject item in list)
		{
			if (item.CanBeClearedForBuilding(builderCommunity, newBuilding) && ((!(item is BaseFence) && !(item is Gate)) || item.GetUnderConstructionInfo() != null))
			{
				item.Delete();
			}
		}
		list.Clear();
	}

	public void ClearTrashForCompletedBuilding(Character builder, TileObject building)
	{
		List<TileObject> list = new List<TileObject>();
		GameTerrain.Instance.GetObjectsInRect(building.GetMinTile(), building.GetMaxTile(), list);
		foreach (TileObject item in list)
		{
			if (item != building && item.CanBeClearedForBuilding(builder.Community, building))
			{
				Demolish(builder, item, playSound: false, createDebris: false);
			}
		}
		list.Clear();
	}

	public TileObject PlaceBuildingDuringGameplay(Character controlledCharacter, Recipe recipe, TerrainCoord tile, Prop.OrientationType orientation, bool checkCanBuildHere = false)
	{
		TileObject tileObject = TileObject.CreateProp(recipe.ProductPropPrototype);
		if (tileObject != null)
		{
			Prop prop = tileObject as Prop;
			if (prop != null)
			{
				prop.SetTile(tile);
				prop.SetOrientationType(orientation);
				prop.Investigated = true;
			}
			if (recipe.ProductPropPrototype.PrefabIndexWhenBuiltByPlayer >= 0 && recipe.ProductPropPrototype.PrefabIndexWhenBuiltByPlayer < recipe.ProductPropPrototype.Prefabs.Count)
			{
				prop.SetVariation(recipe.ProductPropPrototype.PrefabIndexWhenBuiltByPlayer);
			}
			if (prop is EnterableVehicle enterableVehicle)
			{
				enterableVehicle.SetDriveable(driveable: true);
			}
			if (tileObject is SingleTileProp singleTileProp)
			{
				singleTileProp.SetTileGhost(tile);
			}
			if (checkCanBuildHere && GameCursor.CanBuildHere(controlledCharacter, tileObject, checkOtherCharacters: false, checkCropPatches: false, controlledCharacter.Community) != CursorActionDisabledReason.Enabled)
			{
				return null;
			}
			ClearAreaForBuilding(controlledCharacter.Community, tileObject);
			tileObject.SetUnderConstructionInfo(new UnderConstructionInfo(recipe));
			tileObject.SetCommunity(controlledCharacter.Community);
			tileObject.OnSpawn();
			GameTerrain.Instance.BuildMinimap(tileObject.GetMinTile(), tileObject.GetMaxTile());
			if (controlledCharacter.Tile.IsWithinBounds(tileObject.GetMinTile(), tileObject.GetMaxTile()))
			{
				GameTerrain instance = GameTerrain.Instance;
				controlledCharacter.SetPosition(instance.GetTileCentrePos(BuildGoal.FindSuitableExitTileStatic(controlledCharacter, tileObject)));
			}
		}
		return tileObject;
	}

	private void BuildHere(Character controlledCharacter, Recipe recipe, TerrainCoord tile, Prop.OrientationType orientation, bool isDoubleClick)
	{
		TileObject tileObject = PlaceBuildingDuringGameplay(controlledCharacter, recipe, tile, orientation, checkCanBuildHere: true);
		if (tileObject != null)
		{
			controlledCharacter.CommandBuild(tileObject, isDoubleClick, justPlaced: true);
		}
	}

	private void Abandon(Character abandoner, TileObject obj)
	{
		SoundManager.PlaySound3DFromList(SoundManager.DemolishSounds, obj.Pos);
		obj.AbandonBuilding();
		if ((obj is Gate || obj is BaseFence) && abandoner.Community != null)
		{
			List<TileObject> list = new List<TileObject>();
			list.Add(obj);
			while (list.Count > 0)
			{
				TileObject tileObject = list[0];
				list.RemoveAt(0);
				TerrainRect terrainRect = tileObject.GetTileRect().Expand(1);
				for (int i = terrainRect.min.x; i <= terrainRect.max.x; i++)
				{
					for (int j = terrainRect.min.y; j <= terrainRect.max.y; j++)
					{
						TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(i, j);
						if (fixedObjectOnTile is BaseFence && fixedObjectOnTile.GetCommunity() == abandoner.Community)
						{
							fixedObjectOnTile.AbandonBuilding();
							list.Add(fixedObjectOnTile);
						}
					}
				}
			}
		}
		GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
	}

	private void Demolish(Character demolisher, TileObject obj, bool playSound, bool createDebris)
	{
		PropPrototype propPrototype = obj.GetPropPrototype();
		if (playSound && propPrototype != null && propPrototype.WantSoundOnDemolition)
		{
			SoundManager.PlaySound3DFromList(SoundManager.DemolishSounds, obj.Pos);
		}
		Recipe recipe = GameImpl.Instance.FindRecipeByProduct(propPrototype, null);
		if (recipe != null)
		{
			for (int i = 0; i < recipe.Ingredients.Count; i++)
			{
				Ingredient ingredient = recipe.Ingredients[i];
				if (ingredient.Prototypes == null || ingredient.Prototypes.Count <= 0)
				{
					continue;
				}
				int num = ingredient.Amount;
				if (obj.GetUnderConstructionInfo() != null)
				{
					num = (int)obj.GetUnderConstructionInfo().GetAmountUsedOfIngredient(ingredient);
				}
				int num2 = 0;
				if (ingredient.ReturnType == IngredientReturnType.Half)
				{
					num2 = num / 2;
					if (num % 2 == 1)
					{
						if (demolisher.HalfDemolishLoot)
						{
							num2++;
							demolisher.HalfDemolishLoot = false;
						}
						else
						{
							demolisher.HalfDemolishLoot = true;
						}
					}
				}
				else if (ingredient.ReturnType == IngredientReturnType.Full)
				{
					num2 = num;
				}
				else if (ingredient.ReturnType == IngredientReturnType.LostIfTrapSprung)
				{
					if (!(obj is ITrap trap) || !trap.CanResetTrap())
					{
						num2 = num;
					}
				}
				else if (ingredient.ReturnType == IngredientReturnType.LostIfUsed && !(obj is Campfire { State: not CampfireState.Fresh }))
				{
					num2 = num;
				}
				if (num2 <= 0)
				{
					continue;
				}
				Equipment equipment = Equipment.Spawn(ingredient.Prototypes[0], num2);
				if (!equipment.CanBeCombined() && obj is Prop prop)
				{
					if (prop.Prototype.GetNumMaterialVariations() > 0)
					{
						equipment.MaterialVariation = ((prop.MaterialVariation != -1) ? prop.MaterialVariation : prop.GetMaterialVariation());
					}
					if (prop.Prototype.GetNumColorVariations() > 0)
					{
						equipment.ColorVariation = ((prop.ColorVariation != -1) ? prop.ColorVariation : prop.GetColorVariation());
					}
					if (prop.Prototype.GetNumColorVariations2() > 0)
					{
						equipment.ColorVariation2 = ((prop.ColorVariation2 != -1) ? prop.ColorVariation2 : prop.GetColorVariation2());
					}
					if (prop.Prototype.GetNumColorVariations3() > 0)
					{
						equipment.ColorVariation3 = ((prop.ColorVariation3 != -1) ? prop.ColorVariation3 : prop.GetColorVariation3());
					}
				}
				NotificationManager.Instance.AddEquipmentNotification(obj, demolisher, equipment, equipment.GetAmount());
				equipment = demolisher.Inventory.Add(demolisher, equipment);
				demolisher.Skillset.AddProgress(demolisher, SkillType.Construction, num2);
				equipment.IncrementGatheredAmount(num2);
			}
		}
		else if (propPrototype != null && propPrototype.RepairResourceProto != null)
		{
			int num3 = Mathf.FloorToInt(propPrototype.RepairResourceNeeded / 2f);
			if ((int)propPrototype.RepairResourceNeeded % 2 == 1)
			{
				if (demolisher.HalfDemolishLoot)
				{
					num3++;
					demolisher.HalfDemolishLoot = false;
				}
				else
				{
					demolisher.HalfDemolishLoot = true;
				}
			}
			if (num3 > 0)
			{
				Equipment equipment2 = Equipment.Spawn(propPrototype.RepairResourceProto, num3);
				NotificationManager.Instance.AddEquipmentNotification(obj, demolisher, equipment2, equipment2.GetAmount());
				equipment2 = demolisher.Inventory.Add(demolisher, equipment2);
				demolisher.Skillset.AddProgress(demolisher, SkillType.Construction, num3);
				equipment2.IncrementGatheredAmount(num3);
			}
		}
		if (createDebris && (obj.GetUnderConstructionInfo() == null || obj.GetUnderConstructionInfo().IngredientsUsed.Count > 0) && obj.WantDebrisOnDemolition())
		{
			GameTerrain.Instance.SetDebris(obj.GetMinTile(), obj.GetMaxTile());
		}
		obj.Delete();
	}

	public void EquipmentLoadAmmo(AmmoWeapon gun, TileObject from, EquipmentPrototype ammoType, bool triggeredByLocalPlayer)
	{
		if (!from.InventoryContains(gun))
		{
			return;
		}
		int num = ((gun.CurrentAmmoType == ammoType) ? (gun.GetMaxAmmo() - gun.CurrentAmmo) : gun.GetMaxAmmo());
		Equipment equipment = from.GetInventory().FindItemOfType(ammoType);
		if (equipment != null && num > 0)
		{
			equipment = from.GetInventory().Take(from, equipment, num);
			gun.OnReload(from, equipment.GetAmount(), ammoType, equipment.InfectedWith);
			equipment.Delete();
			if (!Instance.Editor && gun.GetPrototype().ReloadInsertSoundResources != null && gun.GetPrototype().ReloadInsertSoundResources.Count > 0)
			{
				from.PlaySoundOneShot(gun.GetPrototype().ReloadInsertSoundResources[MathUtil.NonDeterministicRand.Next() % gun.GetPrototype().ReloadInsertSoundResources.Count]);
			}
		}
	}

	public void EquipmentUnloadAmmo(AmmoWeapon gun, TileObject from, bool triggeredByLocalPlayer)
	{
		if (!from.InventoryContains(gun) || gun.CurrentAmmo <= 0 || gun.CurrentAmmoType == null)
		{
			return;
		}
		Equipment desiredEquipmentToSelect = gun.Unload(from);
		if (!Instance.Editor)
		{
			AudioClip unloadAmmoSound = gun.GetUnloadAmmoSound();
			if (unloadAmmoSound != null)
			{
				SoundManager.PlaySound3D(unloadAmmoSound, from.Pos);
			}
		}
		if (triggeredByLocalPlayer)
		{
			InfoScreen.Instance.DesiredEquipmentToSelect = desiredEquipmentToSelect;
		}
		InfoScreen.Instance.WantRepopulate = true;
	}

	public void EquipmentDestroy(Equipment equipment, TileObject from, bool triggeredByLocalPlayer, int amount)
	{
		if (from.InventoryContains(equipment))
		{
			equipment.TransferFrom(from, amount, triggeredByLocalPlayer).Delete();
		}
	}

	public void EquipmentPourAway(Equipment equipment, TileObject from)
	{
		if (from.InventoryContains(equipment))
		{
			equipment.DrainLiquid(equipment.GetLiquidContentsAmount());
		}
	}

	public void EquipmentPourInto(Equipment from, Equipment to)
	{
		float maxAmount = Math.Min(from.GetLiquidContentsAmount(), to.GetLiquidCapacity() - to.GetLiquidContentsAmount());
		InfectionType infectedWith = from.InfectedWith;
		to.FillLiquid(from.GetLiquidContentsType(), from.DrainLiquid(maxAmount), infectedWith);
	}

	private void EquipmentTransfer(Equipment equipment, TileObject from, TileObject to, int amount, PlayerRecord playerRecord)
	{
		EquipmentTransfer(equipment, from, to, amount, 0f, playerRecord);
	}

	private void EquipmentTransfer(Equipment equipment, TileObject from, TileObject to, int amount, float pickpocketDetection, PlayerRecord playerRecord)
	{
		bool flag = playerRecord?.IsLocal ?? false;
		if (!from.InventoryContains(equipment))
		{
			return;
		}
		Character character = to as Character;
		Character character2 = from as Character;
		bool flag2 = false;
		int num = 0;
		if (playerRecord != null && character2 != null && character != null && EquipmentPrototype.Gold != null)
		{
			switch (playerRecord.SyncedSwappingSuppliesMode)
			{
			case SwappingSuppliesMode.SellingFood:
				num = Mathf.FloorToInt(character.GetPriceToBuy(equipment, character2) * (float)amount);
				if (num == 0)
				{
					if (flag)
					{
						GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_QuantityTooSmallToTrade"));
					}
					return;
				}
				flag2 = true;
				break;
			case SwappingSuppliesMode.SellingFoodMarkedUp:
				num = Mathf.FloorToInt(character.GetPriceToBuy(equipment, character2) * FoodMarkup * (float)amount);
				if (num == 0)
				{
					if (flag)
					{
						GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_QuantityTooSmallToTrade"));
					}
					return;
				}
				flag2 = true;
				break;
			case SwappingSuppliesMode.GivingFood:
				flag2 = true;
				break;
			}
		}
		Equipment equipment2 = equipment.TransferFrom(from, amount, flag);
		float num2 = ((equipment2 != null) ? (equipment2.GetBasePrice() * (float)equipment2.GetAmount()) : 0f);
		int amountMadeTraded = 0;
		float num3 = num2;
		if (flag2)
		{
			equipment2.SetTraded(out amountMadeTraded);
			num3 = equipment2.GetBasePrice() * (float)amountMadeTraded;
		}
		equipment2 = to.GetInventory().Add(to, equipment2, from);
		Character character3 = ((character != null && character.ConsciousAndNotZombie) ? character : playerRecord.PlayerCharacter);
		bool flag3 = character2 != null && character2.Consciousness == Consciousness.Conscious;
		if (playerRecord != null && playerRecord.SyncedSwappingSuppliesMode == SwappingSuppliesMode.Pickpocketing && playerRecord.PlayerCharacter == character)
		{
			flag3 = false;
		}
		if (character3 != null && !flag3 && from.GetCommunity() != null && from.GetCommunity() != character3.GetCommunity())
		{
			character3.OnStoleSomething(from.GetCommunity(), character2, num2);
		}
		if (playerRecord == null)
		{
			return;
		}
		playerRecord.HasScavengedAnyEquipment = true;
		if (character2 == null || character == null)
		{
			return;
		}
		switch (playerRecord.SyncedSwappingSuppliesMode)
		{
		case SwappingSuppliesMode.GivingFood:
		case SwappingSuppliesMode.SellingFood:
		case SwappingSuppliesMode.SellingFoodMarkedUp:
			equipment2.IncrementGatheredAmount(amount);
			character.AddRole(new RoleInfo(Role.Organizer));
			break;
		case SwappingSuppliesMode.Pickpocketing:
		{
			Character character4 = ((playerRecord.PlayerCharacter == character) ? character2 : character);
			Character pickpocket = ((playerRecord.PlayerCharacter == character) ? character : character2);
			character4.OnPickpocketedFrom(pickpocket, equipment2, character4 == character2, pickpocketDetection);
			break;
		}
		}
		switch (playerRecord.SyncedSwappingSuppliesMode)
		{
		case SwappingSuppliesMode.GivingFood:
			Memory.OnMemorableEvent(MemoryPrototype.Helped, character2, character, num3 * 0.04f, secret: false);
			if (character.Community.CommunityType == CommunityType.Looter)
			{
				float gold = character2.GetPriceToSell(equipment2, character) * 10f * (float)amount;
				character.Community.OnShakedown(character2, gold);
			}
			break;
		case SwappingSuppliesMode.SellingFood:
			if (EquipmentPrototype.Gold != null)
			{
				Equipment equipment4 = Equipment.Spawn(EquipmentPrototype.Gold, num);
				NotificationManager.Instance.AddEquipmentNotification(null, character2, equipment4, equipment4.GetAmount());
				equipment4 = character2.Inventory.Add(character2, equipment4);
				Memory.OnMemorableEvent(MemoryPrototype.SoldTo, character2, character, num3 * 2f, secret: false);
				if (character.Community.CommunityType == CommunityType.Looter)
				{
					character.Community.OnShakedown(character2, num);
				}
			}
			break;
		case SwappingSuppliesMode.SellingFoodMarkedUp:
			if (EquipmentPrototype.Gold != null)
			{
				Equipment equipment3 = Equipment.Spawn(EquipmentPrototype.Gold, num);
				NotificationManager.Instance.AddEquipmentNotification(null, character2, equipment3, equipment3.GetAmount());
				equipment3 = character2.Inventory.Add(character2, equipment3);
				Memory.OnMemorableEvent(MemoryPrototype.RippedOff, character2, character, num3, secret: false);
			}
			break;
		}
	}

	public void EquipmentTrade(Equipment equipment, Character from, Character to, int amount, PlayerRecord playerRecord, bool fromTradingScreen)
	{
		bool triggeredByLocalPlayer = playerRecord?.IsLocal ?? false;
		if (!from.InventoryContains(equipment))
		{
			return;
		}
		float inventoryItemPriceAdjustedForTrade = from.GetInventoryItemPriceAdjustedForTrade(equipment, to, fromTradingScreen);
		int num = Math.Max(1, (int)((fromTradingScreen && from.IsControllableByPlayer()) ? Math.Floor(inventoryItemPriceAdjustedForTrade * (float)amount) : Math.Ceiling(inventoryItemPriceAdjustedForTrade * (float)amount)));
		Equipment gold = to.Inventory.GetGold();
		if (gold != null && gold.GetAmount() >= num)
		{
			Equipment equipment2 = equipment.TransferFrom(from, amount, triggeredByLocalPlayer);
			int amountMadeTraded = 0;
			if (to.IsControllableByPlayer() || from.IsControllableByPlayer())
			{
				equipment2.SetTraded(out amountMadeTraded);
			}
			equipment2 = to.GetInventory().Add(to, equipment2, from);
			if (to.IsControllableByPlayer() || from.IsControllableByPlayer())
			{
				gold.SetTraded(out var _);
			}
			from.Inventory.Add(from, to.Inventory.Take(to, gold, num), to);
			float basePrice = equipment.GetBasePrice();
			if (to.IsControllableByPlayer() && MemoryPrototype.BoughtFrom != null)
			{
				float quantityFactor = Math.Max(0f, basePrice * (float)amountMadeTraded);
				Memory.OnMemorableEvent(MemoryPrototype.BoughtFrom, to, from, quantityFactor, secret: false);
			}
			if (from.IsControllableByPlayer() && MemoryPrototype.SoldTo != null)
			{
				float quantityFactor2 = Math.Max(0f, basePrice * (float)amountMadeTraded);
				Memory.OnMemorableEvent(MemoryPrototype.SoldTo, from, to, quantityFactor2, secret: false);
			}
			to.SetupRestockTime();
			from.SetupRestockTime();
			if (playerRecord != null)
			{
				playerRecord.HasScavengedAnyEquipment = true;
			}
			if (!InfoScreen.Instance.Active)
			{
				NotificationManager.Instance.AddEquipmentNotification(to, from, gold, (int)inventoryItemPriceAdjustedForTrade);
				NotificationManager.Instance.AddEquipmentNotification(from, to, equipment, 1);
			}
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Sold, to, equipment2);
		}
	}

	public void ConfirmTrade(List<PendingTrade> pendingTradesIn, Character taker, Character takeFrom, bool evenIfNotEnoughGold, PlayerRecord playerRecord)
	{
		List<PendingTrade> list = new List<PendingTrade>();
		for (int i = 0; i < pendingTradesIn.Count; i++)
		{
			PendingTrade item = pendingTradesIn[i];
			item.Item = BaseObjectManager.FindBaseObjectByID(item.ItemId) as Equipment;
			if (item.Item != null && (item.Selling ? taker.InventoryContains(item.Item) : takeFrom.InventoryContains(item.Item)))
			{
				list.Add(item);
			}
		}
		bool invalidTrade;
		int num = TradePage.CalcTotalGoldPlayerPays(list, taker, takeFrom, out invalidTrade);
		if (num > 0)
		{
			if (num > taker.Inventory.GetGoldAmount())
			{
				return;
			}
		}
		else if (num < 0 && !evenIfNotEnoughGold && -num > takeFrom.Inventory.GetGoldAmount())
		{
			return;
		}
		int num2 = 0;
		if (num != 0)
		{
			Character character = ((num < 0) ? taker : takeFrom);
			Character character2 = ((num < 0) ? takeFrom : taker);
			Equipment gold = character2.Inventory.GetGold();
			int num3 = 0;
			if (gold != null)
			{
				if (character2.IsControllableByPlayer() || character.IsControllableByPlayer())
				{
					gold.SetTraded(out var _);
				}
				Equipment equipment = character2.Inventory.Take(character2, gold, Math.Abs(num));
				if (equipment != null)
				{
					num3 = equipment.GetAmount();
					if (!InfoScreen.Instance.Active)
					{
						NotificationManager.Instance.AddEquipmentNotification(character2, character, equipment, equipment.GetAmount());
					}
					character.Inventory.Add(character, equipment, character2);
				}
			}
			if (num < 0 && num3 < -num)
			{
				num2 = -num - num3;
			}
		}
		float num4 = 0f;
		float num5 = 0f;
		for (int j = 0; j < list.Count; j++)
		{
			Equipment item2 = list[j].Item;
			Character character3 = (list[j].Selling ? taker : takeFrom);
			Character character4 = (list[j].Selling ? takeFrom : taker);
			Equipment equipment2 = item2.TransferFrom(character3, list[j].Amount, triggeredByLocalPlayer: false);
			int amountMadeTraded2 = 0;
			if (character4.IsControllableByPlayer() || character3.IsControllableByPlayer())
			{
				equipment2.SetTraded(out amountMadeTraded2);
			}
			equipment2 = character4.GetInventory().Add(character4, equipment2, character3);
			float basePrice = item2.GetBasePrice();
			if (character4.IsControllableByPlayer())
			{
				float num6 = Math.Max(0f, basePrice * (float)amountMadeTraded2);
				num4 += num6;
			}
			if (character3.IsControllableByPlayer())
			{
				float num7 = Math.Max(0f, basePrice * (float)amountMadeTraded2);
				num5 += num7;
			}
			if (!InfoScreen.Instance.Active)
			{
				NotificationManager.Instance.AddEquipmentNotification(character3, character4, item2, list[j].Amount);
			}
			StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Sold, character4, equipment2);
		}
		if (num4 > 0f && MemoryPrototype.BoughtFrom != null)
		{
			Memory.OnMemorableEvent(MemoryPrototype.BoughtFrom, taker, takeFrom, num4, secret: false);
		}
		if (num5 > 0f && MemoryPrototype.SoldTo != null)
		{
			if (num2 > 0)
			{
				num5 *= Mathf.Clamp01(1f - (float)num2 / (0f - (float)num));
			}
			Memory.OnMemorableEvent(MemoryPrototype.SoldTo, taker, takeFrom, num5, secret: false);
		}
		if (num2 > 0)
		{
			Memory.OnMemorableEvent(MemoryPrototype.Helped, taker, takeFrom, (float)num2 * 0.02f, secret: false);
		}
		taker.SetupRestockTime();
		takeFrom.SetupRestockTime();
		if (playerRecord != null)
		{
			playerRecord.HasScavengedAnyEquipment = true;
		}
		TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
		if (tradePage != null && tradePage.Taker == taker && tradePage.TakeFrom == takeFrom)
		{
			tradePage.PendingTrades.Clear();
		}
	}

	public bool DoesNetworkPlayerIDsMatchOnlinePartyMembers()
	{
		if (!IsInMultiplayerGame() && !OnlineParty.Instance.IsInMultiplayerGame())
		{
			return true;
		}
		if (NetworkPlayerIDs.Count != OnlineParty.Instance.GetNumPartyMembersExcludingBannedAndIgnoredPlayers())
		{
			return false;
		}
		foreach (PartyMember partyMember in OnlineParty.Instance.PartyMembers)
		{
			if (!partyMember.IsBannedOrIgnored() && !NetworkPlayerIDs.Contains(partyMember.PlayerID))
			{
				return false;
			}
		}
		return true;
	}

	public void AddLogEvent(LogEvent logEvent)
	{
		if (!Editor)
		{
			logEvent.Time = PlayTime;
			LogEvents.Add(logEvent);
			logEvent.AddToFeeds(canDeleteEvents: true);
			if (InfoScreen.Instance.IsShowingLog())
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
		}
	}

	public void OnSpawnedEquipment(EquipmentPrototype proto, int amount)
	{
		SpawnedEquipmentCount item = new SpawnedEquipmentCount
		{
			ProtoName = proto.Name
		};
		int num = EquipmentSpawns.BinarySearch(item);
		if (num >= 0)
		{
			SpawnedEquipmentCount value = EquipmentSpawns[num];
			value.Count += amount;
			EquipmentSpawns[num] = value;
		}
		else
		{
			SpawnedEquipmentCount item2 = new SpawnedEquipmentCount
			{
				ProtoName = proto.Name,
				Count = amount
			};
			EquipmentSpawns.Insert(~num, item2);
		}
	}

	public int GetNumEquipmentSpawns(EquipmentPrototype proto)
	{
		SpawnedEquipmentCount item = new SpawnedEquipmentCount
		{
			ProtoName = proto.Name
		};
		int num = EquipmentSpawns.BinarySearch(item);
		if (num >= 0)
		{
			return EquipmentSpawns[num].Count;
		}
		return 0;
	}

	public void SetPreferredPlayerRescuer(Community community)
	{
		PreferredPlayerRescuer = community;
		community?.GetBaseCentre(out PreferredPlayerRescuerTile, out var _);
	}

	public bool RescuePlayer()
	{
		Character character = CommunityManager.PlayerCommunity.Leader;
		if (character == null || !character.AliveAndNotZombie)
		{
			character = CommunityManager.PlayerCommunity.GetFirstActiveMember();
			if (character == null)
			{
				return false;
			}
		}
		Community community = null;
		float num = float.MaxValue;
		foreach (Community community3 in CommunityManager.Communities)
		{
			if (community3 == PreferredPlayerRescuer)
			{
				community = community3;
				num = float.MinValue;
				break;
			}
			if (!community3.HasAnyConsciousMembers() || community3.CommunityType == CommunityType.Player || community3.IsZombieCommunity() || community3.IsAnimalCommunity() || community3.IsAlwaysHostileToPlayerCommunity() || community3.HiddenCommunity)
			{
				continue;
			}
			CommunityRelationshipType relationship = community3.GetRelationship(CommunityManager.PlayerCommunity);
			if (relationship == CommunityRelationshipType.Hostile)
			{
				continue;
			}
			bool flag = false;
			foreach (Character member in community3.Members)
			{
				if (character.WantToAvoidCommunity(member.GetCommunityThatOwnsThisArea()) || member.WantToAvoidCommunity(member.GetCommunityThatOwnsThisArea()))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				float num2 = Mathf.Sqrt(community3.GetDistSqToNearestMember(character.Tile));
				community3.CalcApprovalRatingForCharacterOrCommunity(character, out var approval, out var _);
				num2 -= approval * 5f;
				if (relationship == CommunityRelationshipType.Known)
				{
					num2 -= 100f;
				}
				if (relationship == CommunityRelationshipType.Ceasefire)
				{
					num2 += 50f;
				}
				num2 += DeterministicRand.RandomFloat() * 200f;
				if (community3.GetNumLitCampfires() > 0)
				{
					num2 -= 500f;
				}
				else if (community3.Buildings.Count > 0)
				{
					num2 -= 200f;
				}
				if (num2 < num)
				{
					num = num2;
					community = community3;
				}
			}
		}
		if (community == null)
		{
			return false;
		}
		CommunityRelationshipType relationship2 = community.GetRelationship(CommunityManager.PlayerCommunity);
		if (relationship2 < CommunityRelationshipType.Known || relationship2 == CommunityRelationshipType.Hostile)
		{
			CommunityManager.SetRelationship(CommunityManager.PlayerCommunity, community, CommunityRelationshipType.Known);
			if (community.IsAmbientCommunity())
			{
				community.CommunityNameKnown = true;
			}
		}
		bool flag2 = false;
		Character character2 = null;
		Character character3 = null;
		Character character4 = null;
		for (int i = 0; i < CommunityManager.PlayerCommunity.Members.Count; i++)
		{
			Character character5 = CommunityManager.PlayerCommunity.Members[i];
			if (!character5.AliveAndNotZombie || character5.GetBaseObjectType() != BaseObjectType.Human)
			{
				continue;
			}
			TerrainCoord terrainCoord = community.PickRandomTileInsidePerimeter(DeterministicRand);
			List<Campfire> list = new List<Campfire>();
			foreach (Prop building in community.Buildings)
			{
				if (!(building is Campfire campfire))
				{
					continue;
				}
				if (campfire.IsBurning())
				{
					if (list.Count > 0 && !list[0].IsBurning())
					{
						list.Clear();
					}
				}
				else if (list.Count > 0 && list[0].IsBurning())
				{
					continue;
				}
				list.Add(campfire);
			}
			if (list.Count > 0)
			{
				Campfire campfire2 = list[DeterministicRand.Next(list.Count)];
				int num3 = 0;
				while (num3 < 100)
				{
					terrainCoord = campfire2.Tile + DeterministicRand.RandomTile(new TerrainCoord(-6, -6), new TerrainCoord(6, 6));
					num3++;
					if (community.IsTileInsidePerimeter(terrainCoord) && !GameTerrain.Instance.IsImpassable(terrainCoord.x, terrainCoord.y, 199, character5, null) && GameTerrain.Instance.IsTileSpawnable(terrainCoord.x, terrainCoord.y))
					{
						break;
					}
				}
			}
			Character character6 = community.GetNearestLivingNonZombieMember(terrainCoord, null, BaseObjectType.Human, float.MaxValue);
			if (character6 == null)
			{
				if (character5.InitialCommunity == community && CommunityManager.PlayerCommunity.GetLivingNonZombieMemberCount() > 1)
				{
					community.AddMemberWithNotifications(character5);
					i--;
					flag2 = true;
				}
				character6 = character5;
			}
			if (character5.InsideBuilding != null)
			{
				character5.InsideBuilding.OnCharacterLeave(character5, 0, fromBuildingDestroyed: false, fromRagdolled: true);
			}
			if (community == PreferredPlayerRescuer)
			{
				int num4 = 0;
				while (num4 < 100)
				{
					terrainCoord = PreferredPlayerRescuerTile + DeterministicRand.RandomTile(new TerrainCoord(-10, -10), new TerrainCoord(10, 10));
					num4++;
					if (!GameTerrain.Instance.IsImpassable(terrainCoord.x, terrainCoord.y, 199, character5, null) && GameTerrain.Instance.IsTileSpawnable(terrainCoord.x, terrainCoord.y))
					{
						break;
					}
				}
			}
			character5.SetTile(terrainCoord);
			character5.SetHangoutLocation(terrainCoord);
			character5.SetFollowMePos(Vector2.zero);
			character5.SetFacingAngle(DeterministicRand.RandomFloat() * (MathF.PI * 2f));
			while (character5.HasUnbandagedInjury(0))
			{
				character5.ApplyBandage(character6, playSound: false, 5, InfectionType.None, callingCodeSupportsPrediction: false);
			}
			for (int j = 1; j < 6; j++)
			{
				InfectionType infectionType = (InfectionType)j;
				if (character5.HasInjuryWithInfectionType(infectionType) && GameImpl.Instance.FindAntigenPrototypeForInfectionType(infectionType) != null)
				{
					character5.InjectAntigen(character6, infectionType, InfectionType.None, null, callingCodeSupportsPrediction: false);
				}
			}
			character5.SetBloodLoss(0f);
			character5.SetFatigue(0f);
			character5.SetBodyTemperatureInCelsius(Character.BodyTemperatureInCelsiusNormal);
			character5.SedativeEffect = 0f;
			character5.BloodAlcoholConcentration = 0f;
			character5.SetConsciousness(Consciousness.Conscious);
			if (character5.HasMovementZone() && !character5.MovementZone.Contains(terrainCoord))
			{
				character5.PauseMovementZone();
			}
			int constitution = character5.Skillset.Constitution;
			float constitutionProgression = character5.Skillset.ConstitutionProgression;
			float[] progressionToLevel = Skillset.ProgressionToLevel;
			float num5 = (constitutionProgression - progressionToLevel[constitution]) / (progressionToLevel[constitution + 1] - progressionToLevel[constitution]);
			float progress = ((constitution > 0) ? (progressionToLevel[constitution - 1] + num5 * (progressionToLevel[constitution] - progressionToLevel[constitution - 1])) : 0f) - constitutionProgression;
			character5.Skillset.AddProgress(character5, SkillType.Constitution, progress);
			SkillsChangePopup skillsChangePopup = new SkillsChangePopup();
			skillsChangePopup.Character = character5;
			SkillChange item = default(SkillChange);
			item.Skill = SkillType.Constitution;
			item.OldLevel = constitution;
			item.NewLevel = character5.Skillset.Constitution;
			item.OldCap = (item.NewCap = character5.Skillset.ConstitutionCap);
			skillsChangePopup.SkillChanges.Add(item);
			SkillsChangePopups.Add(skillsChangePopup);
			if (!character5.IsInPlayerCommunity())
			{
				character4 = character5;
				continue;
			}
			if (character3 == null)
			{
				character3 = character5;
				character4 = character6;
			}
			PlayerRecord playerControllingMe = character5.GetPlayerControllingMe();
			if (playerControllingMe != null && playerControllingMe.IsPartyLeader)
			{
				character3 = character5;
				character4 = character6;
			}
			if (character2 == null)
			{
				character2 = character5;
			}
			if (playerControllingMe != null && playerControllingMe.IsLocal)
			{
				character2 = character5;
			}
		}
		if (character2 != null)
		{
			if (GameCamera.FlyCam)
			{
				GameCamera.TeleportToObject(character2);
			}
			else
			{
				GameCamera.WantSnapZoom = true;
			}
			if (character2 != Hud.Instance.LocalControlledCharacter)
			{
				WantSwitchControlledCharacter = character2;
			}
		}
		int num6 = (int)(60f * Sun.DayLengthSecs * Mathf.Lerp(0.5f, 1f, DeterministicRand.RandomFloat()));
		TimeSpan timeSpan = MathUtil.FromSeconds(1f / 60f * (float)num6);
		Frame += num6;
		PlayTime += timeSpan;
		PlaySpeed = PlaySpeed.Normal;
		foreach (Character character7 in CharacterManager.Characters)
		{
			character7.OnSpawnOrTimeJump();
		}
		CommunityManager.UpdateAmbientEnemies(TimeSpan.Zero, forceFinishTask: true);
		for (int num7 = CommunityManager.Communities.Count - 1; num7 >= 0; num7--)
		{
			Community community2 = CommunityManager.Communities[num7];
			if (community2 != null && community2 != community && (community2.GetRelationship(CommunityManager.PlayerCommunity) == CommunityRelationshipType.Hostile || community2.GetRelationship(community) == CommunityRelationshipType.Hostile) && (community2.IsTargetingAnyoneFromCommunity(CommunityManager.PlayerCommunity) || community2.IsTargetingAnyoneFromCommunity(community)))
			{
				if (CommunityManager.Hunters.Contains(community2))
				{
					CommunityManager.DeleteHunters(community2);
				}
				else if (community2.SpawnPoint != null)
				{
					CommunityManager.DeleteAmbientEnemy(community2);
				}
				else
				{
					if (community2.IsAISettlement() && community2.BaseRect != TerrainRect.Invalid)
					{
						foreach (Character member2 in community2.Members)
						{
							if (!community2.IsTileInsidePerimeter(member2.Tile))
							{
								TerrainCoord tile = community2.PickRandomTileInsidePerimeter(DeterministicRand);
								int num8 = 0;
								while (GameTerrain.Instance.IsImpassable(tile.x, tile.y, 199, member2, null) && num8 < 100)
								{
									tile = community2.PickRandomTileInsidePerimeter(DeterministicRand);
									num8++;
								}
								member2.SetTile(tile);
							}
						}
					}
					foreach (Character member3 in community2.Members)
					{
						foreach (Target target in member3.Targets)
						{
							if (target.Object != null && target.Object.GetCommunityId() == CommunityManager.PlayerCommunity.Id)
							{
								target.ForgetAboutMe();
							}
						}
					}
				}
			}
		}
		if (character4 != null && character3 != null)
		{
			if (character4.Consciousness == Consciousness.Unconscious)
			{
				while (character4.HasUnbandagedInjury(0))
				{
					character4.ApplyBandage(character4, playSound: false, 5, InfectionType.None, callingCodeSupportsPrediction: false);
				}
				character4.SetBloodLoss(0f);
				character4.SetFatigue(0f);
				character4.SetBodyTemperatureInCelsius(Character.BodyTemperatureInCelsiusNormal);
				character4.SedativeEffect = 0f;
				character4.BloodAlcoholConcentration = 0f;
				character4.SetConsciousness(Consciousness.Conscious);
			}
			if (character4.Tile.GetDist(character3.Tile) >= 20f)
			{
				TerrainCoord tile2 = character3.Tile;
				int num9 = 0;
				while (num9 < 100)
				{
					tile2 = character3.Tile + DeterministicRand.RandomTile(new TerrainCoord(-10, -10), new TerrainCoord(10, 10));
					num9++;
					if (!GameTerrain.Instance.IsImpassable(tile2.x, tile2.y, 199, character4, null) && GameTerrain.Instance.IsTileSpawnable(tile2.x, tile2.y))
					{
						break;
					}
				}
				character4.SetTile(tile2);
			}
		}
		SpeechSituation sit = SpeechSituation.Rescued;
		if (community.IsLooterCommunity() && character4 != null && character4 != character3 && !flag2)
		{
			float num10 = StoryEvent.GiveAllGold(character3, character4, 20f, isShakedown: true, fromBystanders: true, fromEveryone: false);
			if (num10 == 0f)
			{
				num10 = StoryEvent.GiveAllSupplies(character3, character4, 10f, isShakedown: true, fromBystanders: true, fromEveryone: false, null);
			}
			if (num10 > 0f)
			{
				sit = SpeechSituation.RescuedLooted;
			}
			community.OnShakedown(character3, 5f);
			StoryManager.Instance.SetVariable(Variable.Create("RescuedLoot", community, CommunityManager.PlayerCommunity, num10));
		}
		if (community.IsHunterCommunity() && community.Squads.Count > 0 && (community.Squads[0].Behaviour == SquadBehaviour.Travel || community.Squads[0].Behaviour == SquadBehaviour.Trade))
		{
			community.SetSquadAction(community.Squads[0], SquadAction.Wait, 0, TerrainCoord.Invalid, null);
		}
		if (character4 != null && character4 != character3)
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character4, character3, sit);
			if (speechForSituation != null)
			{
				float num11 = Mathf.Lerp(4f, 1f, Mathf.Clamp01((character4.PosXZ - character3.PosXZ).magnitude * 0.1f));
				StoryEvent storyEvent = default(StoryEvent);
				storyEvent.Type = StoryEventType.Speak;
				storyEvent.Subject = Specifier.Actor;
				storyEvent.Object = Specifier.Target;
				storyEvent.Speeches = new List<SpeechRef>();
				storyEvent.Speeches.Add(SpeechRef.Create(speechForSituation));
				StoryManager.Instance.QueuedEvents.Add(QueuedEvent.Create(storyEvent, character4, character3, null, default(MemoryParam), Instance.PlayTime + TimeSpan.FromSeconds(num11)));
			}
		}
		GameTerrain.Instance.FogOfWar.DeterministicUpdate();
		HintManager.Instance.HideAllHints();
		HudBehaviour.Instance.ClearStatusBarMsg();
		WantSlowerTransitionIn = true;
		return true;
	}

	public void OnCharacterDeleted(Character deletedCharacter)
	{
		for (int num = Instance.LogEvents.Count - 1; num >= 0; num--)
		{
			if (Instance.LogEvents[num].Character == deletedCharacter)
			{
				Instance.LogEvents.RemoveAt(num);
				if (InfoScreen.Instance.IsShowingLog())
				{
					InfoScreen.Instance.WantRepopulate = true;
				}
			}
		}
		foreach (PlayerRecord playerRecord in Instance.PlayerRecords)
		{
			if (playerRecord.PlayerCharacter != deletedCharacter)
			{
				continue;
			}
			playerRecord.PlayerCharacter = Instance.CommunityManager.PlayerCommunity.Leader;
			foreach (ShortcutGroup shortcutGroup in playerRecord.ShortcutGroups)
			{
				if (shortcutGroup.ControlledCharacter == deletedCharacter)
				{
					playerRecord.ShortcutGroups.Remove(shortcutGroup);
				}
				else
				{
					shortcutGroup.SelectedCharacters.Remove(deletedCharacter);
				}
			}
		}
	}

	public bool CanSwitchControlledCharacter()
	{
		if (Editor)
		{
			return false;
		}
		int livingNonZombieMemberCount = CommunityManager.PlayerCommunity.GetLivingNonZombieMemberCount();
		if (livingNonZombieMemberCount <= 1)
		{
			if (Hud.LocalControlledCharacter != null && !Hud.LocalControlledCharacter.AliveAndNotZombie)
			{
				return livingNonZombieMemberCount > 0;
			}
			return false;
		}
		return true;
	}

	public void SetControlledCharacter(PlayerRecord playerRecord, Character toCharacter)
	{
		if (playerRecord.PlayerCharacter == toCharacter)
		{
			return;
		}
		if (playerRecord.PlayerCharacter != null && playerRecord.PlayerMode == PlayerMode.Controlling)
		{
			playerRecord.PlayerCharacter.OnStopDirectControl();
			foreach (PlayerRecord playerRecord2 in PlayerRecords)
			{
				if (playerRecord2 != playerRecord && playerRecord2.PlayerCharacter == playerRecord.PlayerCharacter && playerRecord2.PlayerMode == PlayerMode.Observing)
				{
					playerRecord2.PlayerMode = PlayerMode.Controlling;
					break;
				}
			}
		}
		bool flag = GetPlayerControllingCharacter(toCharacter) != null;
		playerRecord.PlayerMode = ((!flag) ? PlayerMode.Controlling : PlayerMode.Observing);
		playerRecord.PlayerCharacter = toCharacter;
		playerRecord.SyncedSwappingSupplies = null;
		playerRecord.SyncedSwappingSuppliesWith = null;
		playerRecord.SyncedSwappingSuppliesMode = SwappingSuppliesMode.None;
		playerRecord.TargetObject = null;
		playerRecord.TargetBodyLocationToAimFor = TargettableBodyLocation.Torso;
		playerRecord.WantLockOnTarget = false;
		if (!FollowerCommandsEnabled && toCharacter.IsControllableByPlayer())
		{
			foreach (Character member in CommunityManager.PlayerCommunity.Members)
			{
				if (member != playerRecord.PlayerCharacter && member != toCharacter && member.SquadLeader == null && GetPlayerControllingCharacter(member) == null)
				{
					member.Follow(toCharacter);
				}
			}
		}
		if (toCharacter == Hud.LocalControlledCharacter)
		{
			Hud.ShowPlayerName();
		}
	}

	public PlayerRecord GetPlayerControllingCharacter(Character character)
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerCharacter == character && playerRecord.PlayerMode == PlayerMode.Controlling)
			{
				return playerRecord;
			}
		}
		return null;
	}

	public bool IsPlayerControllingCharacterInFlyMode(Character character)
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerCharacter == character && playerRecord.PlayerMode == PlayerMode.Controlling)
			{
				return playerRecord.FlyMode;
			}
		}
		return false;
	}

	public bool IsLocalPlayerControllingCharacter(Character character)
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerCharacter == character && playerRecord.PlayerMode == PlayerMode.Controlling)
			{
				return playerRecord.IsLocal;
			}
		}
		return false;
	}

	public PlayerRecord GetPlayerRecord(PlayerID playerID)
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerID == playerID)
			{
				return playerRecord;
			}
		}
		return null;
	}

	public PlayerRecord GetLocalPlayerRecord()
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.IsLocal)
			{
				return playerRecord;
			}
		}
		return null;
	}

	public PlayerRecord GetPartyLeaderRecord()
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.IsPartyLeader)
			{
				return playerRecord;
			}
		}
		return null;
	}

	public PlayerRecord GetMostRecentlyUsedPlayerRecord()
	{
		PlayerRecord playerRecord = null;
		foreach (PlayerRecord playerRecord2 in PlayerRecords)
		{
			if (playerRecord == null || playerRecord2.LastUsedTime > playerRecord.LastUsedTime)
			{
				playerRecord = playerRecord2;
			}
		}
		return playerRecord;
	}

	public bool HasEverBeenInMultiplayer()
	{
		int num = 0;
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.HasCreatedCharacter && playerRecord.PlayerID.IsValid())
			{
				num++;
				if (num >= 2)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsPlayerSurrendering()
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerMode == PlayerMode.Controlling && playerRecord.PlayerCharacter != null && playerRecord.PlayerCharacter.CurrentActionAnim == ActionAnim.HandsUp)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSwappingSupplies(Character character)
	{
		foreach (PlayerRecord playerRecord in Instance.PlayerRecords)
		{
			if (playerRecord.SyncedSwappingSupplies == character)
			{
				return true;
			}
			if (playerRecord.SyncedSwappingSuppliesWith == character)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInInfoScreen(Character character)
	{
		return GetPlayerControllingCharacter(character)?.SyncedIsInInfoScreen ?? false;
	}

	public bool IsAnyoneInInfoScreen()
	{
		foreach (PlayerRecord playerRecord in Instance.PlayerRecords)
		{
			if (playerRecord.SyncedIsInInfoScreen)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEveryoneInInfoScreen()
	{
		foreach (PlayerRecord playerRecord in Instance.PlayerRecords)
		{
			if (playerRecord.PlayerMode == PlayerMode.Controlling && !playerRecord.SyncedIsInInfoScreen)
			{
				return false;
			}
		}
		return true;
	}

	public Character GetLocalPlayerCharacter()
	{
		return GetLocalPlayerRecord()?.PlayerCharacter;
	}

	public TileObject GetLocalPlayerCharacterOrBuildingTheyAreIn()
	{
		return GetLocalPlayerRecord()?.GetPlayerCharacterOrBuildingTheyAreIn();
	}

	public Character GetPartyLeaderCharacter()
	{
		return GetPartyLeaderRecord()?.PlayerCharacter;
	}

	public bool IsVisibleDeterministic(Vector2 pos)
	{
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerMode != PlayerMode.Dormant && playerRecord.PlayerMode != PlayerMode.CreatingCharacter)
			{
				if ((playerRecord.SyncedCamFocusPosXZ - pos).sqrMagnitude <= 1024f)
				{
					return true;
				}
				if (playerRecord.SyncedPipObj != null && (pos - playerRecord.SyncedPipObj.PosXZ).sqrMagnitude <= 64f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsWithinRangeOfMainCameraDeterministic(Vector2 pos, float range)
	{
		float num = range * range;
		foreach (PlayerRecord playerRecord in PlayerRecords)
		{
			if (playerRecord.PlayerMode != PlayerMode.Dormant && playerRecord.PlayerMode != PlayerMode.CreatingCharacter && (playerRecord.SyncedCamFocusPosXZ - pos).sqrMagnitude <= num)
			{
				return true;
			}
		}
		return false;
	}

	public void SetupPlayerRecords()
	{
		if (Editor)
		{
			return;
		}
		PlayerRecord playerRecord = null;
		if (NetworkPlayerIDs.Count == 0)
		{
			PlayerRecord playerRecord2 = GetPlayerRecord(default(PlayerID));
			if (playerRecord2 == null)
			{
				playerRecord2 = new PlayerRecord();
				playerRecord2.PlayerCharacter = CommunityManager.PlayerCommunity.Leader;
				PlayerRecords.Add(playerRecord2);
			}
			PlayerRecord mostRecentlyUsedPlayerRecord = GetMostRecentlyUsedPlayerRecord();
			if (mostRecentlyUsedPlayerRecord != playerRecord2)
			{
				playerRecord2.CopyPlayerRecordSettings(mostRecentlyUsedPlayerRecord);
				playerRecord = mostRecentlyUsedPlayerRecord;
			}
			playerRecord2.PlayerMode = PlayerMode.Controlling;
			playerRecord2.IsLocal = true;
			playerRecord2.IsPartyLeader = true;
			playerRecord2.HasCreatedCharacter = true;
		}
		foreach (PlayerRecord playerRecord5 in PlayerRecords)
		{
			if ((NetworkPlayerIDs.Count != 0 || !playerRecord5.PlayerID.IsNull()) && !NetworkPlayerIDs.Contains(playerRecord5.PlayerID) && playerRecord5.PlayerMode != PlayerMode.Dormant)
			{
				playerRecord5.PlayerMode = PlayerMode.Dormant;
				playerRecord5.IsLocal = false;
				playerRecord5.IsPartyLeader = false;
				if (playerRecord5.PlayerName != null && playerRecord5.PlayerName.Length != 0 && playerRecord5 != playerRecord)
				{
					LogEvent logEvent = new LogEvent(LogEventType.PlayerLeft);
					logEvent.PlayerID = playerRecord5.PlayerID;
					logEvent.PlayerName = playerRecord5.PlayerName;
					AddLogEvent(logEvent);
				}
			}
		}
		for (int i = 0; i < NetworkPlayerIDs.Count; i++)
		{
			PlayerRecord playerRecord3 = GetPlayerRecord(NetworkPlayerIDs[i]);
			PlayerRecord playerRecord4 = null;
			bool flag = false;
			if (playerRecord3 == null)
			{
				playerRecord3 = new PlayerRecord();
				playerRecord3.PlayerCharacter = CommunityManager.PlayerCommunity.Leader;
				foreach (Character member in CommunityManager.PlayerCommunity.Members)
				{
					if (member.AvatarForPlayer.IsValid() && member.AvatarForPlayer == NetworkPlayerIDs[i])
					{
						if (member.AliveAndNotZombie)
						{
							playerRecord3.PlayerCharacter = member;
						}
						playerRecord3.HasCreatedCharacter = true;
						break;
					}
				}
				PlayerRecord mostRecentlyUsedPlayerRecord2 = GetMostRecentlyUsedPlayerRecord();
				if (mostRecentlyUsedPlayerRecord2 != null)
				{
					playerRecord3.SyncedCurrentQuest = mostRecentlyUsedPlayerRecord2.SyncedCurrentQuest;
					playerRecord3.SyncedMapCamPosition = mostRecentlyUsedPlayerRecord2.SyncedMapCamPosition;
					mostRecentlyUsedPlayerRecord2.MapMarkerLocations.CopyToList(playerRecord3.MapMarkerLocations);
				}
				PlayerRecords.Add(playerRecord3);
				flag = true;
			}
			playerRecord3.PlayerID = NetworkPlayerIDs[i];
			playerRecord3.PlayerName = NetworkPlayerNames[i];
			if (i == 0)
			{
				PlayerRecord mostRecentlyUsedPlayerRecord3 = GetMostRecentlyUsedPlayerRecord();
				if (mostRecentlyUsedPlayerRecord3 != null && mostRecentlyUsedPlayerRecord3.PlayerID.IsNull())
				{
					playerRecord3.CopyPlayerRecordSettings(mostRecentlyUsedPlayerRecord3);
					playerRecord4 = mostRecentlyUsedPlayerRecord3;
				}
			}
			if ((flag || playerRecord3.PlayerMode == PlayerMode.Dormant) && playerRecord4 == null)
			{
				LogEvent logEvent2 = new LogEvent(LogEventType.PlayerJoined);
				logEvent2.PlayerID = playerRecord3.PlayerID;
				logEvent2.PlayerName = playerRecord3.PlayerName;
				AddLogEvent(logEvent2);
			}
			playerRecord3.IsLocal = playerRecord3.PlayerID.IsLocal();
			playerRecord3.IsPartyLeader = i == 0;
			if (playerRecord3.IsPartyLeader)
			{
				playerRecord3.HasCreatedCharacter = true;
			}
			if (!playerRecord3.HasCreatedCharacter)
			{
				playerRecord3.PlayerMode = PlayerMode.CreatingCharacter;
			}
			else if (playerRecord3.PlayerMode != PlayerMode.Controlling)
			{
				playerRecord3.PlayerMode = ((GetPlayerControllingCharacter(playerRecord3.PlayerCharacter) == null) ? PlayerMode.Controlling : PlayerMode.Observing);
			}
		}
		foreach (Character member2 in CommunityManager.PlayerCommunity.Members)
		{
			if (member2.DirectControlled && GetPlayerControllingCharacter(member2) == null)
			{
				member2.OnStopDirectControl();
			}
		}
		foreach (PlayerRecord playerRecord6 in PlayerRecords)
		{
			if (playerRecord6.PlayerMode != PlayerMode.Dormant)
			{
				playerRecord6.LastUsedTime = PlayTime;
			}
		}
		PlayerRecord partyLeaderRecord = GetPartyLeaderRecord();
		if (partyLeaderRecord != null && partyLeaderRecord.PlayerID.IsValid() && CommunityManager.PlayerCommunity.Leader != null && CommunityManager.PlayerCommunity.Leader.AvatarForPlayer != partyLeaderRecord.PlayerID && CharacterManager.GetAvatarForPlayer(partyLeaderRecord.PlayerID) == null)
		{
			CommunityManager.PlayerCommunity.Leader.AvatarForPlayer = partyLeaderRecord.PlayerID;
		}
		PredictedObjectManager.SetupPredictedPlayerRecords();
	}

	public void Load(CustomBinaryReader reader, bool newGame, bool sandbox, CharacterCreationSettings characterSettings, string textDumpPath)
	{
		if (WantPlayingInputs != null && Math.Max(NetworkPlayerIDs.Count, 1) == WantPlayingInputs.Players.Count)
		{
			PlayingInputs = WantPlayingInputs;
			reader = new CustomBinaryReaderFromMemory(PlayingInputs.InitialGameState, PlayingInputs.InitialGameStateLength);
		}
		if (!(characterSettings != null && characterSettings.VehicleProto != null && characterSettings.HitTheRoadCount > 0 && newGame && sandbox) || Editor)
		{
			ClearDiscoveredFlags();
		}
		CustomRandom customRandom = MathUtil.NonDeterministicRand;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		int sizeFromMapSize = GameTerrain.GetSizeFromMapSize(GameImpl.Instance.GetMaxMapSizeForCurrentStories());
		if (GameTerrain.MaxSize < sizeFromMapSize)
		{
			GameTerrain.MaxSize = sizeFromMapSize;
			GameTerrain.InitTerrainArrays();
		}
		if (reader != null)
		{
			Reflect(reader);
			if (Editor)
			{
				Weather.StartDayOfYear = settings.StartDayOfYear;
				Weather.StartHourOfDay = settings.StartHourOfDay;
				Weather.SetInitialTemperatureInCelsius(customRandom);
			}
			if (textDumpPath != null)
			{
				using (TextDumper reflector = new TextDumper(textDumpPath))
				{
					Reflect(reflector);
				}
				DebugPaused = true;
			}
			if (newGame && characterSettings != null)
			{
				DifficultySettings = characterSettings.DifficultySettings.MakeCopy();
			}
		}
		else
		{
			MapSize mapSize;
			if (sandbox && characterSettings != null)
			{
				if (characterSettings.RandomSeed != 0)
				{
					customRandom = new CustomRandom(characterSettings.RandomSeed);
				}
				RandomSeed = characterSettings.RandomSeed;
				Weather.StartDayOfYear = characterSettings.StartDayOfYear;
				Weather.StartHourOfDay = characterSettings.StartHourOfDay;
				mapSize = characterSettings.MapSize;
			}
			else
			{
				Weather.StartDayOfYear = settings.StartDayOfYear;
				Weather.StartHourOfDay = settings.StartHourOfDay;
				mapSize = settings.MapSize;
			}
			if (characterSettings != null)
			{
				DifficultySettings = characterSettings.DifficultySettings.MakeCopy();
			}
			else if (settings.DifficultySettings.Count > 0)
			{
				DifficultySettings = settings.DifficultySettings[0].MakeCopy();
			}
			GameTerrain gameTerrain = GameTerrain.Spawn(GameTerrain.GetSizeFromMapSize(mapSize));
			Weather.SetInitialTemperatureInCelsius(customRandom);
			if (sandbox)
			{
				gameTerrain.Generate(customRandom);
			}
			else
			{
				gameTerrain.InitBlank();
			}
		}
		Community community = CommunityManager.PlayerCommunity;
		if (community == null)
		{
			community = Community.Spawn(CommunityType.Player);
			community.UniqueID = "PlayerCommunity";
			community.CommunityName.SetCustomString("Player Community");
		}
		if (newGame && !Editor)
		{
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord tile = new TerrainCoord(instance.Size, instance.Size) / 2;
			float num = 0f;
			if (StoryManager.PlayerStartPoints.Count > 0)
			{
				PlayerStartPoint playerStartPoint = StoryManager.PlayerStartPoints[customRandom.Next(StoryManager.PlayerStartPoints.Count)];
				tile = playerStartPoint.Tile;
				num = playerStartPoint.Angle;
			}
			if (characterSettings != null && characterSettings.Appearance != null)
			{
				HitTheRoadCount = characterSettings.HitTheRoadCount;
				foreach (KeyValuePair<string, float> variable in characterSettings.Variables)
				{
					StoryManager.SetVariable(Variable.Create(variable.Key, null, null, variable.Value));
				}
				EnterableVehicle enterableVehicle = null;
				if (characterSettings.VehicleProto != null)
				{
					enterableVehicle = TileObject.SpawnProp(characterSettings.VehicleProto, tile, Prop.GetClosestOrientationTypeToAngle(num + MathF.PI)) as EnterableVehicle;
					if (enterableVehicle != null)
					{
						float damageFraction = Mathf.Max(characterSettings.VehicleDamageFraction, Mathf.Lerp(characterSettings.VehicleDamageFraction, 0.9f, customRandom.RandomFloat()));
						enterableVehicle.SetDamageFraction(damageFraction);
						enterableVehicle.MaterialVariation = characterSettings.VehicleMaterialVariation;
						enterableVehicle.ColorVariation = characterSettings.VehicleColorVariation;
						enterableVehicle.ColorVariation2 = characterSettings.VehicleColorVariation2;
						enterableVehicle.ColorVariation3 = characterSettings.VehicleColorVariation3;
						enterableVehicle.ColorVariation4 = characterSettings.VehicleColorVariation4;
						enterableVehicle.SetCommunity(community);
						enterableVehicle.SetDriveable(driveable: true);
						enterableVehicle.Park(customRandom);
						for (int i = 0; i < characterSettings.VehicleInventory.Count; i++)
						{
							Equipment equipment = characterSettings.VehicleInventory[i];
							equipment.OnSpawn();
							enterableVehicle.Inventory.Add(enterableVehicle, equipment);
						}
					}
				}
				SortedDictionary<CrewMemberSettings, Character> sortedDictionary = new SortedDictionary<CrewMemberSettings, Character>();
				Character character = Human.Spawn(tile, num, (HumanAppearance)characterSettings.Appearance, InfectionType.None);
				character.AvatarForPlayer = characterSettings.AvatarForPlayer;
				character.NameKnown = true;
				character.Investigated = true;
				character.SetAllSkillsKnown();
				character.SetCommunity(community);
				character.SetRank(Rank.Leader);
				SetupCharacterFromCharacterSettings(character, characterSettings, HitTheRoadCount == 0);
				sortedDictionary[characterSettings] = character;
				enterableVehicle?.OnCharacterEnter(character, wasOrderedInsideBuilding: true);
				community.CommunityName = characterSettings.GangName;
				CrewMemberSettings.CopyValidEquipmentPolicies(characterSettings.CommunityEquipmentPolicies, community.EquipmentPolicies);
				foreach (CrewMemberSettings item in characterSettings.Crew)
				{
					Character character2 = null;
					if (item.Appearance is HumanAppearance)
					{
						character2 = Human.Spawn(tile, num, item.GetHumanAppearance(), InfectionType.None);
					}
					else if (item.Appearance is ChickenAppearance)
					{
						character2 = Chicken.Spawn(tile, num, (ChickenAppearance)item.Appearance);
					}
					else if (item.Appearance is DeerAppearance)
					{
						character2 = Deer.Spawn(tile, num, (DeerAppearance)item.Appearance);
					}
					else if (item.Appearance is RabbitAppearance)
					{
						character2 = Rabbit.Spawn(tile, num, (RabbitAppearance)item.Appearance);
					}
					if (character2 != null)
					{
						character2.AvatarForPlayer = item.AvatarForPlayer;
						character2.NameKnown = true;
						character2.Investigated = true;
						character2.SetAllSkillsKnown();
						character2.SetCommunity(community);
						SetupCharacterFromCharacterSettings(character2, item, HitTheRoadCount == 0);
						sortedDictionary[item] = character2;
						character2.Follow(character);
						enterableVehicle?.OnCharacterEnter(character2, wasOrderedInsideBuilding: true);
					}
				}
				foreach (KeyValuePair<CrewMemberSettings, Character> item2 in sortedDictionary)
				{
					item2.Key.ApplyMemoriesAndRelationships(item2.Value, sortedDictionary);
					item2.Value.HasTravelledFromAnotherMap = HitTheRoadCount > 0;
				}
			}
			else
			{
				HumanAppearance humanAppearance = new HumanAppearance((!customRandom.RandomChoice(0.5f)) ? GenderType.Female : GenderType.Male, HumanAppearance.PickRandomAge(customRandom));
				humanAppearance.Randomize(InfectionType.None, customRandom);
				Character character3 = Human.Spawn(tile, num, humanAppearance, InfectionType.None);
				character3.RandomizeName(customRandom);
				character3.NameKnown = true;
				character3.Investigated = true;
				character3.SetAllSkillsKnown();
				character3.SetAllPersonalitiesKnown();
				character3.RandomizeClothing(customRandom, seasonallyAppropriate: true);
				character3.SetCommunity(community);
				character3.SetRank(Rank.Leader);
				character3.Skillset.Randomize(character3, customRandom);
				community.CommunityName.Randomise(customRandom, unique: true, community);
			}
		}
		if (Editor)
		{
			GameTerrain.Instance.UseFixedTerrain = true;
		}
		if (RecordInputsEnabled && PlayingInputs == null)
		{
			CustomBinaryWriterToMemory customBinaryWriterToMemory = new CustomBinaryWriterToMemory();
			Reflect(customBinaryWriterToMemory);
			RecordingInputs = new InputsRecord(customBinaryWriterToMemory._buffer, customBinaryWriterToMemory._index);
			if (NetworkPlayerIDs.Count > 0)
			{
				for (int j = 0; j < NetworkPlayerIDs.Count; j++)
				{
					RecordingInputs.Players.Add(new InputsRecord.Player(NetworkPlayerIDs[j]));
				}
			}
			else
			{
				RecordingInputs.Players.Add(new InputsRecord.Player(default(PlayerID)));
			}
		}
		SetupPlayerRecords();
		if (!Editor)
		{
			StoryManager.FixupOnLoad(this, reader);
			HintManager.FixupOnLoad(this, reader);
			CropsManager.OnLoad();
			PlayerRecord localPlayerRecord = GetLocalPlayerRecord();
			InventorySortBy = localPlayerRecord.SyncedInventorySortBy;
			InventorySortOrder = localPlayerRecord.SyncedInventorySortOrder;
			CharactersSortBy = localPlayerRecord.SyncedCharactersSortBy;
			CharactersSortOrder = localPlayerRecord.SyncedCharactersSortOrder;
			SentInventorySortBy = InventorySortBy;
			SentInventorySortOrder = InventorySortOrder;
			SentCharactersSortBy = CharactersSortBy;
			SentCharactersSortOrder = CharactersSortOrder;
			if (newGame)
			{
				GameUniqueId = MathUtil.NonDeterministicRand.Next();
				CharacterManager.OnNewGame();
				CommunityManager.OnNewGame();
				StoryManager.OnNewGame();
				StoryManager.RefreshQuestMarkers();
				if (!DifficultySettings.SaveTokensRequired && GameTerrain.Instance.UseFixedTerrain)
				{
					ReplaceAllSaveTokensWithGold();
				}
				foreach (PlantableCrop allCrop in CropsManager.AllCrops)
				{
					if (allCrop.CommunityId != 0)
					{
						CropsManager.SetPlantableCropType(allCrop.CommunityId, allCrop.Tile, allCrop.Prototype);
					}
				}
				GameTerrain.Instance.SetupRoadSkillCapBonus();
				FirstObjectIdSpawnedDuringPlay = BaseObjectManager.Instance.GetNextFreeId();
				LoneWolf = CommunityManager.PlayerCommunity.IsLoneWolfCommunity();
				AchievementsEnabled = true;
				if (characterSettings != null && characterSettings.VehicleProto != null && characterSettings.HitTheRoadCount > 0)
				{
					AchievementsEnabled = characterSettings.AchievementsEnabled;
				}
			}
			else
			{
				CharacterManager.OnLoad(reader?.Version ?? 629);
				CommunityManager.OnLoad(reader?.Version ?? 629);
				StoryManager.OnLoad(reader?.Version ?? 629);
				if (reader.Version < 529 && StoryManager.IsQuestDiscovered("Charlie_Quest_ClaimSanctuary", null, null, null))
				{
					UnlockGate("GuardianGate1");
					UnlockGate("GuardianGate2");
					UnlockGate("BrownGate1");
					UnlockGate("BrownGate2");
				}
				if (reader.Version < 604 && GameTerrain.Instance.UseFixedTerrain && !StoryManager.IsQuestCompleted("Plan_Quest_Defeat", null, null, null) && !StoryManager.IsQuestFailed("Plan_Quest_Defeat", null, null, null) && BaseObjectManager.Instance.GetObjectByUniqueID(CarterMoreno) is Character character4)
				{
					character4.DontLeaveCommunity = true;
				}
				if (reader.Version < 607 && GameTerrain.Instance.UseFixedTerrain && BaseObjectManager.GetObjectByUniqueID("MorganHarding") is Character { ScriptedGoalMarker: not null } character5 && character5.ScriptedGoalMarker.GetUniqueID() == "WindfarmGangMarker")
				{
					StoryManager.EnableTrigger("Scout_Trigger_ReturnedHome", character5, null, null);
				}
				if (reader.Version < 623 && GameTerrain.Instance.UseFixedTerrain && BaseObjectManager.GetObjectByUniqueID("ChristinaRiley") is Character { Community: not null } character6 && character6.Community.GetUniqueID() == "RitzCreekGang")
				{
					character6.DontLeaveCommunity = true;
				}
				if (reader.Version < 629 && GameTerrain.Instance.UseFixedTerrain)
				{
					if (BaseObjectManager.FindBaseObjectByID(47053) is Building building)
					{
						building.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(47058) is Building building2)
					{
						building2.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(47054) is Building building3)
					{
						building3.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(47059) is Building building4)
					{
						building4.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(123733) is Building building5)
					{
						building5.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(81942) is Building building6)
					{
						building6.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(113255) is Building building7)
					{
						building7.IgnoreForQuests = true;
					}
					if (BaseObjectManager.FindBaseObjectByID(324) is Building building8)
					{
						building8.IgnoreForQuests = true;
					}
				}
			}
			if (newGame)
			{
				foreach (PlayerRecord playerRecord in PlayerRecords)
				{
					if (playerRecord.PlayerCharacter != null)
					{
						playerRecord.SyncedCamAngle = MathUtil.WrapAngle(playerRecord.PlayerCharacter.FacingAngle + MathF.PI);
					}
				}
			}
		}
		foreach (Recipe item3 in GameImpl.Instance.CurrentRecipesSortedBySkill)
		{
			item3.ShownDiscoveredNotification = item3.IsDiscovered();
		}
		if (GameTerrain.Instance != null)
		{
			GameTerrain.Instance.InitOnThread();
		}
		LastAutoSavedTime = (LastSavedTime = PlayTime);
		State = SessionState.Loaded;
	}

	private void ReplaceAllSaveTokensWithGold()
	{
		foreach (Prop allProp in PropManager.AllProps)
		{
			ReplaceSaveTokensWithGold(allProp);
		}
		foreach (Character character in CharacterManager.Characters)
		{
			ReplaceSaveTokensWithGold(character);
		}
	}

	private void ReplaceSaveTokensWithGold(TileObject carrier)
	{
		EquipmentContainer inventory = carrier.GetInventory();
		if (inventory == null)
		{
			return;
		}
		Equipment equipment = inventory.FindItemOfClass(typeof(SavegameToken));
		if (equipment != null)
		{
			int num = Mathf.RoundToInt(equipment.GetBasePrice() * (float)equipment.GetAmount());
			inventory.Remove(carrier, equipment);
			equipment.Delete();
			if (num > 0 && EquipmentPrototype.Gold != null)
			{
				inventory.Add(carrier, Equipment.Spawn(EquipmentPrototype.Gold, num));
			}
		}
	}

	private void UnlockGate(string name)
	{
		if (BaseObjectManager.Instance.GetObjectByUniqueID(name) is Gate { GateState: GateState.Locked } gate)
		{
			gate.Unlock();
		}
	}

	private IEnumerator UnityInitEverything()
	{
		using (new StopWatchMarker("Unity Init Everything"))
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			for (UnityInitProgress = 0; UnityInitProgress < BaseObjectManager.BaseObjects.Count; UnityInitProgress++)
			{
				BaseObjectManager.BaseObjects[UnityInitProgress].UnityInit();
				if (stopwatch.Elapsed >= TimeSpan.FromSeconds(0.01666666753590107))
				{
					stopwatch.Stop();
					yield return null;
					stopwatch.Reset();
					stopwatch.Start();
				}
			}
		}
		State = SessionState.UnityInited;
	}

	public void GetUnityInitProgress(out int loaded, out int total)
	{
		loaded = UnityInitProgress;
		total = BaseObjectManager.BaseObjects.Count;
	}

	public void StartUnityInit()
	{
		GameCamera.OnStart();
		GameImpl.Instance.Sun.SetupLightForTime(DaysSinceStart, useMiddayReflectionTex: false);
		GameImpl.Instance.Sun.SetupSkyShaderGlobals();
		GameImpl.Instance.Sun.StartRenderingMainReflectionProbe();
		UnityInitCoroutine = UnityInitEverything();
		Launcher.Instance.StartCoroutine(UnityInitCoroutine);
	}

	public void StopUnityInit()
	{
		Launcher.Instance.StopCoroutine(UnityInitCoroutine);
		UnityInitCoroutine = null;
		HudBehaviour.Instance.UnityGameCameraBehaviour.FogOfWarBehaviour.ConstantsDirty = true;
		HudBehaviour.Instance.UnityPipCameraBehaviour.FogOfWarBehaviour.ConstantsDirty = true;
	}

	public void Start()
	{
		UnityInitCoroutine = null;
		GameTerrain.Instance.AStar.OnStartPlaying();
		Hud.OnStart();
		GameImpl.Instance.Sun.SetupLightForTime(DaysSinceStart, useMiddayReflectionTex: false);
		Weather.OnStart();
		Weather.NonDeterministicUpdate(0f);
		NotificationManager.Instance.InitCommunityStats = true;
		if (IsInMultiplayerGame())
		{
			OnlineParty.Instance.SendSyncGameAcknowledged(this);
		}
		if (OnlineParty.Instance.StatusType == NetworkStatusType.OutOfSync || OnlineParty.Instance.StatusType == NetworkStatusType.WaitingForPartyMember)
		{
			OnlineParty.Instance.ClearStatus();
		}
		BirdSongDebugMenu.Log.Clear();
		DebugMenu = new MainDebugMenu();
		DebugMenuCommunity.Community = null;
		if (InfoScreen.AllowViewInfoOnAnyone || Character.AlwaysActivateInvisibleStrainWhenNearDeath)
		{
			AchievementsEnabled = false;
		}
		State = SessionState.Started;
		GameCamera.Update(1f / 60f);
		HudBehaviour.Instance.UnityGrassCameraBehaviour.StartGrassTask(GameImpl.Instance.MainGrassRenderer);
	}

	public void Finish()
	{
		PredictedObjectManager.OnFinish();
		foreach (BaseObject baseObject in BaseObjectManager.BaseObjects)
		{
			baseObject.UnityDelete();
		}
		UnityObjectPool.RevertAllPoolsToPrepopulatedAmount();
		HudBehaviour.Instance.KillSkillsChangePopupPanel();
		GameCamera.OnFinish();
		State = SessionState.Finished;
	}

	public void Unload()
	{
		if (RecordingInputs != null)
		{
			string text = GameImpl.Instance.SaveGamePath + "/OutOfSync";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			using Stream stream = File.OpenWrite(text + "\\InputsRecord.rec");
			using CustomBinaryWriter reflector = new CustomBinaryWriter(stream);
			RecordingInputs.Reflect(reflector);
		}
		State = SessionState.Unloaded;
		HintManager.Unload();
		CommunityManager.Unload();
		if (GameTerrain.Instance != null)
		{
			GameTerrain.Instance.Unload();
		}
		Hud.Unload();
		HudBehaviour.Instance.ClearStatusBarMsg();
		BaseObjectManager.Unload();
		PredictedObjectManager.Unload();
		StoryManager.Unload();
		DebugMenuItemOpenPage.ClearOldMenus();
		InfoScreen.Instance.CloseInfoScreen(playSound: false);
		MovableUnityObjectsVisibleInMainView.Clear();
		UnityObjectsVisibleInPip.Clear();
		ActiveMovingUnityObjects.Clear();
		Instance = null;
	}

	public void OnDrawGizmos()
	{
		if (TerrainEditor.ShowPaths)
		{
			foreach (TerrainPath path in GameTerrain.Instance.Paths)
			{
				path.DrawPathGizmos();
			}
		}
		DebugMenu.OnDrawGizmos();
	}

	public void OnGUI()
	{
		if (GameplayDebugMenu.ShowGoals && !GameImpl.Instance.IsMenuOpen())
		{
			GameImpl instance = GameImpl.Instance;
			HudBehaviour instance2 = HudBehaviour.Instance;
			foreach (Character character in CharacterManager.Characters)
			{
				if (character.GetGoal() == null)
				{
					continue;
				}
				Vector3 vector = instance2.UnityGameCamera.WorldToScreenPoint(character.Pos) / (GameImpl.SuperSample ? 2f : 1f);
				if (vector.x >= 0f && vector.y >= 0f && vector.x < (float)instance2.MainPanelWidthOnScreen && vector.y < (float)instance.HeightOnScreen)
				{
					GoalString.Length = 0;
					character.GetGoal().BuildDebugString(character, null, GoalString);
					for (int i = 0; i < character.Roles.Count; i++)
					{
						Role role = character.Roles[i].Role;
						GoalString.Append('\n');
						GoalString.Append(Character.RoleNames[(int)role] + ((role == Role.Guard && character.GuardDuty) ? " GuardDuty" : string.Empty));
					}
					if (character.Rank != Rank.None)
					{
						GoalString.Append('\n');
						GoalString.Append(Character.RankNames[(int)character.Rank]);
					}
					GoalString.Append('\n');
					GoalString.Append(character.CurrentAnimState.ToString());
					if (character.AnimWrapper != null)
					{
						GoalString.Append('\n');
						GoalString.Append(character.AnimWrapper.BaseStateName);
						GoalString.Append('\n');
						GoalString.Append(character.AnimWrapper.ClipName);
					}
					GUI.Label(new Rect((float)instance.LeftOnScreen + vector.x, (float)(instance.TopOnScreen + instance.HeightOnScreen) - vector.y, 600f, 200f), GoalString.ToString());
				}
			}
		}
		DebugMenu.OnGUI();
	}

	public void OnPostRender()
	{
		Hud.OnPostRender();
		if (TerrainEditor.ShowImpassable)
		{
			GameTerrain instance = GameTerrain.Instance;
			TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(GameCamera.Focus);
			Vector3 vector = new Vector3(0f, 0.1f, 0f);
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			for (int i = tileCoordForPos.x - 20; i < tileCoordForPos.x + 20; i++)
			{
				for (int j = tileCoordForPos.y - 20; j < tileCoordForPos.y + 20; j++)
				{
					if (!instance.IsTileOutsideBounds(i, j))
					{
						Color col = MathUtil.TransparentBlack;
						if (instance.IsImpassableRaw(i, j))
						{
							col = Color.red;
						}
						else if (instance.IsSlope(i, j))
						{
							col = Color.yellow;
						}
						else if (instance.IsTileRiver(i, j))
						{
							col = Color.blue;
						}
						if (col.a > 0f)
						{
							DebugGraphics.DrawLine(instance.GetVertexPos(i, j) + vector, instance.GetVertexPos(i + 1, j) + vector, col);
							DebugGraphics.DrawLine(instance.GetVertexPos(i, j) + vector, instance.GetVertexPos(i, j + 1) + vector, col);
							DebugGraphics.DrawLine(instance.GetVertexPos(i, j + 1) + vector, instance.GetVertexPos(i + 1, j + 1) + vector, col);
							DebugGraphics.DrawLine(instance.GetVertexPos(i + 1, j) + vector, instance.GetVertexPos(i + 1, j + 1) + vector, col);
						}
					}
				}
			}
			DebugGraphics.EndDrawLines();
		}
		if (TerrainEditor.ShowOwnership)
		{
			GameTerrain instance2 = GameTerrain.Instance;
			TerrainCoord tileCoordForPos2 = instance2.GetTileCoordForPos(GameCamera.Focus);
			Vector3 vector2 = new Vector3(0f, 0.1f, 0f);
			DebugGraphics.StartDrawLines(Matrix4x4.identity);
			for (int k = tileCoordForPos2.x - 20; k < tileCoordForPos2.x + 20; k++)
			{
				for (int l = tileCoordForPos2.y - 20; l < tileCoordForPos2.y + 20; l++)
				{
					if (!instance2.IsTileOutsideBounds(k, l))
					{
						int ownerCommunityIdForTile = instance2.GetOwnerCommunityIdForTile(k, l);
						Color col2 = MathUtil.TransparentBlack;
						Community community = BaseObjectManager.FindBaseObjectByID(ownerCommunityIdForTile) as Community;
						if (community != null && community.CommunityType == CommunityType.Player)
						{
							col2 = Color.green;
						}
						else if (community != null && community.CommunityType == CommunityType.Looter)
						{
							col2 = Color.red;
						}
						else if (community != null)
						{
							col2 = Color.yellow;
						}
						if (TerrainEditor.ShowOwnershipForCommunityId != 0 && TerrainEditor.ShowOwnershipForCommunityId != ownerCommunityIdForTile)
						{
							col2 *= 0.5f;
						}
						if (col2.a > 0f)
						{
							DebugGraphics.DrawLine(instance2.GetVertexPos(k, l) + vector2, instance2.GetVertexPos(k + 1, l) + vector2, col2);
							DebugGraphics.DrawLine(instance2.GetVertexPos(k, l) + vector2, instance2.GetVertexPos(k, l + 1) + vector2, col2);
							DebugGraphics.DrawLine(instance2.GetVertexPos(k, l + 1) + vector2, instance2.GetVertexPos(k + 1, l + 1) + vector2, col2);
							DebugGraphics.DrawLine(instance2.GetVertexPos(k + 1, l) + vector2, instance2.GetVertexPos(k + 1, l + 1) + vector2, col2);
						}
					}
				}
			}
			DebugGraphics.EndDrawLines();
		}
		if (TerrainEditor.ShowPaths || PathEditor.Current != null)
		{
			foreach (TerrainPath path in GameTerrain.Instance.Paths)
			{
				path.DrawPath();
			}
		}
		if (GameplayDebugMenu.DrawConstructionRecords)
		{
			GameTerrain instance3 = GameTerrain.Instance;
			Vector3 vector3 = new Vector3(0f, 0.1f, 0f);
			Color red = Color.red;
			foreach (Community community2 in CommunityManager.Communities)
			{
				if (community2.ConstructionRecords.Count > 0)
				{
					DebugGraphics.StartDrawLines(Matrix4x4.identity);
					for (int m = 0; m < community2.ConstructionRecords.Count; m++)
					{
						community2.ConstructionRecords[m].Proto.CalcMinMaxTile(community2.ConstructionRecords[m].Tile, community2.ConstructionRecords[m].Orientation, out var minTile, out var maxTile);
						DebugGraphics.DrawLine(instance3.GetVertexPos(minTile.x, minTile.y) + vector3, instance3.GetVertexPos(maxTile.x + 1, minTile.y) + vector3, red);
						DebugGraphics.DrawLine(instance3.GetVertexPos(minTile.x, minTile.y) + vector3, instance3.GetVertexPos(minTile.x, maxTile.y + 1) + vector3, red);
						DebugGraphics.DrawLine(instance3.GetVertexPos(minTile.x, maxTile.y + 1) + vector3, instance3.GetVertexPos(maxTile.x + 1, maxTile.y + 1) + vector3, red);
						DebugGraphics.DrawLine(instance3.GetVertexPos(maxTile.x + 1, minTile.y) + vector3, instance3.GetVertexPos(maxTile.x + 1, maxTile.y + 1) + vector3, red);
					}
					DebugGraphics.EndDrawLines();
				}
			}
		}
		if (Editor || Character.DrawLastKnownTargetPositions || Character.DrawBoundingBoxes || Character.DrawHitBoxes || Character.DrawInjuries || Character.DrawBones || PropEditor.ShowEntrances || PropEditor.ShowWheelPositions || PropEditor.ShowPitTrapJoiners || SpawnPoint.DebugShowSpawnPoints)
		{
			GameTerrain instance4 = GameTerrain.Instance;
			TerrainCoord tileCoordForPos3 = instance4.GetTileCoordForPos(GameCamera.Focus);
			instance4.GetObjectsInRect(tileCoordForPos3 - new TerrainCoord(24, 24), tileCoordForPos3 + new TerrainCoord(24, 24), tmp);
			foreach (TileObject item in tmp)
			{
				item.GetPredictedOrElseThis().OnPostRender();
			}
			tmp.Clear();
		}
		DebugGraphics.DrawPersistentLines();
		DebugMenu.OnPostRender();
	}

	public void TakeSnapshotIfNecessary()
	{
		int num = (ForceFullSnapshot ? 60 : FastSnapshotFrequency);
		if (IsInMultiplayerGame() && SessionOutOfSyncFrame == -1 && InputFrame % num == 0)
		{
			using (new ProfileMarker(TakeSnapshotTimer))
			{
				CustomBinaryWriterToMemory customBinaryWriterToMemory = new CustomBinaryWriterToMemory();
				customBinaryWriterToMemory.IsDoingNetworkChecksum = true;
				customBinaryWriterToMemory.IsFastPath = InputFrame % FullSnapshotFrequency != 0 && GameTerrain.Instance.Size > 512 && !ForceFullSnapshot;
				Reflect(customBinaryWriterToMemory);
				SnapshotTaskData snapshotTaskData = new SnapshotTaskData();
				snapshotTaskData.Writer = customBinaryWriterToMemory;
				snapshotTaskData.InputFrame = InputFrame;
				GameImpl.Instance.UpdateThreadPool.AddTask(CalcSnapshotHashOnThreadFunc, OnSnapshotHashCalculated, snapshotTaskData, TaskPriority.Low);
			}
		}
	}

	private void CalcSnapshotHashOnThread(BaseTaskData data)
	{
		SnapshotTaskData snapshotTaskData = (SnapshotTaskData)data;
		snapshotTaskData.Hash = new MD5Hash(snapshotTaskData.Writer._buffer, snapshotTaskData.Writer._index);
	}

	private void OnSnapshotHashCalculated(BaseTaskData data)
	{
		SnapshotTaskData snapshotTaskData = (SnapshotTaskData)data;
		if (State != SessionState.Finished)
		{
			SessionSnapshot sessionSnapshot = FindSnapshot(snapshotTaskData.InputFrame);
			if (sessionSnapshot == null)
			{
				sessionSnapshot = new SessionSnapshot(snapshotTaskData.InputFrame);
				SessionSnapshots.Add(sessionSnapshot);
			}
			sessionSnapshot.Hashes.Add(new PlayerHash(OnlineParty.Instance.GetLocalPlayerID(), snapshotTaskData.Hash));
			if (OutOfSyncDebugEnabled)
			{
				sessionSnapshot.Writer = snapshotTaskData.Writer;
			}
			if (OnlineParty.Instance.IsInMultiplayerGameAsLeader())
			{
				LeaderCheckSnapshot(sessionSnapshot);
			}
			else
			{
				OnlineParty.Instance.SendSnapshotHash(this, snapshotTaskData.InputFrame, snapshotTaskData.Hash);
			}
		}
	}

	public SessionSnapshot FindSnapshot(int inputFrame)
	{
		for (int i = 0; i < SessionSnapshots.Count; i++)
		{
			if (SessionSnapshots[i].InputFrame == inputFrame)
			{
				return SessionSnapshots[i];
			}
		}
		return null;
	}

	public void RemoveSnapshot(int inputFrame)
	{
		for (int i = 0; i < SessionSnapshots.Count; i++)
		{
			if (SessionSnapshots[i].InputFrame == inputFrame)
			{
				SessionSnapshots.RemoveAt(i);
			}
		}
	}

	public void OnLeaderReceivedSnapshot(int inputFrame, MD5Hash hash, PartyMember fromPartyMember)
	{
		if (SessionOutOfSyncFrame == -1)
		{
			SessionSnapshot sessionSnapshot = FindSnapshot(inputFrame);
			if (sessionSnapshot != null)
			{
				sessionSnapshot.Hashes.Add(new PlayerHash(fromPartyMember.PlayerID, hash));
				LeaderCheckSnapshot(sessionSnapshot);
			}
			else
			{
				sessionSnapshot = new SessionSnapshot(inputFrame);
				sessionSnapshot.Hashes.Add(new PlayerHash(fromPartyMember.PlayerID, hash));
				SessionSnapshots.Add(sessionSnapshot);
			}
		}
	}

	public void LeaderCheckSnapshot(SessionSnapshot snapshot)
	{
		if (snapshot.Hashes.Count < NetworkPlayerIDs.Count)
		{
			return;
		}
		bool flag = true;
		for (int i = 1; i < snapshot.Hashes.Count; i++)
		{
			if (snapshot.Hashes[i].Hash != snapshot.Hashes[i - 1].Hash)
			{
				flag = false;
				break;
			}
		}
		OnlineParty.Instance.SendSnapshotAcknowledged(this, snapshot.InputFrame, flag);
		if (flag)
		{
			RemoveSnapshot(snapshot.InputFrame);
			return;
		}
		string text = "Snapshot hash mismatch: ";
		for (int j = 0; j < snapshot.Hashes.Count; j++)
		{
			if (j > 0)
			{
				text += ", ";
			}
			text = text + snapshot.Hashes[j].PlayerID.GetPlayerName() + ": " + snapshot.Hashes[j].Hash.ToString();
		}
		text = text + " for frame " + snapshot.InputFrame;
		UnityEngine.Debug.Log(text);
		if (snapshot.Writer != null)
		{
			SessionOutOfSyncSnapshotFolder = GameImpl.Instance.SaveGamePath + "/OutOfSync/" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "/";
			MD5Hash mD5Hash = new MD5Hash(snapshot.Writer._buffer, snapshot.Writer._index);
			MD5Hash mD5Hash2 = mD5Hash;
			UnityEngine.Debug.Log("Saving full snapshot: " + mD5Hash2.ToString() + " for frame " + snapshot.InputFrame);
			OnlineParty.SaveSessionSnapshot(snapshot.Writer._buffer, snapshot.Writer._index, snapshot.InputFrame, OnlineParty.Instance.GetLeaderPartyMember(), SessionOutOfSyncSnapshotFolder);
			File.Copy(OnlineParty.GetTerrainCacheFileName(TerrainHash), SessionOutOfSyncSnapshotFolder + "/" + TerrainHash.ToString() + ".map");
		}
		SessionOutOfSyncFrame = snapshot.InputFrame;
		SessionSnapshots.Clear();
		try
		{
			string[] directories = Directory.GetDirectories(GameImpl.Instance.SaveGamePath.Replace('/', '\\') + "\\OutOfSync");
			if (directories.Length > 10)
			{
				List<FolderTimestamp> list = new List<FolderTimestamp>();
				string[] array = directories;
				foreach (string text2 in array)
				{
					list.Add(new FolderTimestamp
					{
						Timestamp = Directory.GetCreationTime(text2),
						Name = text2
					});
				}
				list.Sort();
				while (list.Count > 10)
				{
					Directory.Delete(list[0].Name, recursive: true);
					list.RemoveAt(0);
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning(ex.Message);
		}
		OnlineParty.Instance.SetStatus(NetworkStatusType.OutOfSync, GameImpl.Translate("HUD_OutOfSync"));
		AssignNewSessionId();
	}

	public void OnSnapshotHashMismatch(int inputFrame, PartyMember fromPartyMember)
	{
		if (SessionOutOfSyncFrame == -1)
		{
			SessionSnapshot sessionSnapshot = FindSnapshot(inputFrame);
			if (sessionSnapshot != null && sessionSnapshot.Writer != null)
			{
				OnlineParty.Instance.SendSnapshotFull(this, sessionSnapshot, fromPartyMember);
			}
			SessionOutOfSyncFrame = inputFrame;
			SessionSnapshots.Clear();
			OnlineParty.Instance.SetStatus(NetworkStatusType.OutOfSync, GameImpl.Translate("HUD_OutOfSync"));
		}
	}

	public bool AutoSave(SaveGameType saveGameType, bool overwriteAllSlots, bool fromSuspend = false, int tokenSaveCameFromCarrierId = 0, EquipmentPrototype tokenProto = null)
	{
		GameImpl instance = GameImpl.Instance;
		SaveGameManager instance2 = SaveGameManager.Instance;
		if (!IsPartyLeader())
		{
			return false;
		}
		if (Editor)
		{
			return false;
		}
		if (SkillsChangePopups.Count > 0)
		{
			return false;
		}
		if (GameFinishedState == GameFinishedState.GameOver)
		{
			return false;
		}
		if (saveGameType != SaveGameType.Current && !CommunityManager.PlayerCommunity.IsAnyoneHealable())
		{
			return false;
		}
		if (saveGameType != SaveGameType.QuickSave && saveGameType != SaveGameType.QuickSave_Multiplayer)
		{
			if (saveGameType != SaveGameType.Current)
			{
				if (CommunityManager.PlayerCommunity.Leader != null && CommunityManager.PlayerCommunity.Leader.HasInjuryWithInfectionType(InfectionType.Red))
				{
					return false;
				}
				Character localPlayerCharacter = GetLocalPlayerCharacter();
				if (localPlayerCharacter != null)
				{
					if (localPlayerCharacter.HasInjuryWithInfectionType(InfectionType.Red))
					{
						return false;
					}
					if (localPlayerCharacter.HasInjuryWithInfectionType(InfectionType.White))
					{
						return false;
					}
				}
			}
			if (IsInMultiplayerGameAsFollower())
			{
				return false;
			}
		}
		if (instance2.IsSaving())
		{
			return true;
		}
		if (IsInMultiplayerGame())
		{
			if (saveGameType == SaveGameType.QuickSave)
			{
				saveGameType = SaveGameType.QuickSave_Multiplayer;
			}
			if (saveGameType == SaveGameType.AutoSave)
			{
				saveGameType = SaveGameType.AutoSave_Multiplayer;
			}
			if (saveGameType == SaveGameType.Token)
			{
				saveGameType = SaveGameType.Token_Multiplayer;
			}
		}
		try
		{
			SaveGameData data;
			CustomBinaryWriter writer = SerialiseGame(out data);
			byte[] thumbnailBytes = instance.CreateScreenshot();
			for (int i = 0; i < ((!overwriteAllSlots) ? 1 : 2); i++)
			{
				SaveRequest saveRequest = new SaveRequest();
				saveRequest.SaveGame = new SaveGame();
				saveRequest.SaveGame.Type = saveGameType;
				saveRequest.TokenSaveCameFromCarrierId = tokenSaveCameFromCarrierId;
				saveRequest.TokenProto = tokenProto;
				string text = SaveGameManager.BuildSaveGameDirName(saveRequest.SaveGame.Type, GameUniqueId);
				SaveGameSlot key = new SaveGameSlot(saveGameType, GameUniqueId);
				if (saveGameType != SaveGameType.Current)
				{
					int value = 0;
					instance2.NextSaveSlot.TryGetValue(key, out value);
					text += value;
					instance2.NextSaveSlot[key] = (value + 1) % 2;
				}
				saveRequest.SaveGame.DirName = text;
				saveRequest.SaveGame.Data = data;
				saveRequest.Writer = writer;
				saveRequest.ThumbnailBytes = thumbnailBytes;
				saveRequest.TerrainHash = TerrainHash;
				saveRequest.FromSuspend = fromSuspend;
				if (fromSuspend)
				{
					instance2.Save(saveRequest);
				}
				else
				{
					instance2.PushSaveRequest(saveRequest);
				}
				LatestAutosaveRequests.Add(saveRequest);
			}
			if (saveGameType != SaveGameType.Current)
			{
				LastAutoSavedTime = PlayTime;
			}
			LastSavedTime = PlayTime;
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogWarning(ex.ToString());
		}
		return false;
	}

	public CustomBinaryWriterToMemory SerialiseGame(out SaveGameData data)
	{
		CustomBinaryWriterToMemory customBinaryWriterToMemory = new CustomBinaryWriterToMemory();
		Reflect(customBinaryWriterToMemory);
		List<string> list = new List<string>();
		List<PlayerRecord> list2 = new List<PlayerRecord>();
		PlayerRecords.CopyToList(list2);
		list2.Sort(PlayerRecordSorter);
		foreach (PlayerRecord item in list2)
		{
			if (item.PlayerName != null && item.PlayerName.Length > 0)
			{
				list.Add(item.PlayerName);
			}
		}
		List<StoryId> list3 = new List<StoryId>();
		foreach (StorySource storySource in StorySources)
		{
			list3.Add(new StoryId
			{
				Folder = storySource.Folder,
				WorkshopId = storySource.WorkshopId
			});
		}
		data = default(SaveGameData);
		data.Stories = list3;
		data.Timestamp = DateTime.Now;
		data.GameTimeTicks = PlayTime.Ticks;
		data.CommunitySize = CommunityManager.PlayerCommunity.GetLivingNonZombieMemberCount();
		data.CommunityName = CommunityManager.PlayerCommunity.GetDisplayNameString();
		data.CharacterName = ((CommunityManager.PlayerCommunity.Leader != null) ? CommunityManager.PlayerCommunity.Leader.GetDisplayNameString() : string.Empty);
		data.Day = Day;
		data.DayOfYear = (int)DayOfYear;
		data.PlayerNames = list;
		return customBinaryWriterToMemory;
	}
}
