using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Community : BaseObject
{
	private struct CharacterScore : IComparable<CharacterScore>
	{
		public Character Character;

		public float Score;

		public int CompareTo(CharacterScore other)
		{
			if (Score < other.Score)
			{
				return -1;
			}
			if (Score > other.Score)
			{
				return 1;
			}
			if (Character.Id < other.Character.Id)
			{
				return -1;
			}
			if (Character.Id > other.Character.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	public class SortByStrengthSkillAscending : IComparer<Character>
	{
		int IComparer<Character>.Compare(Character a, Character b)
		{
			if (a.Skillset.Strength < b.Skillset.Strength)
			{
				return -1;
			}
			if (a.Skillset.Strength > b.Skillset.Strength)
			{
				return 1;
			}
			if (a.Id < b.Id)
			{
				return -1;
			}
			if (a.Id > b.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	public static string[] CommunityTypeNames = StringUtil.GetEnumNames<CommunityType>();

	public static string[] SquadBehaviourNames = StringUtil.GetEnumNames<SquadBehaviour>();

	public static string[] SquadActionNames = StringUtil.GetEnumNames<SquadAction>();

	public CommunityType CommunityType;

	public Character Leader;

	public GangName CommunityName = new GangName();

	public string LastLeaderName;

	public StringStatus CommunityNameVerified;

	public bool CommunityNameKnown;

	public bool DiscoverNameIfAtWar;

	public bool CommunityStatusShown = true;

	public List<InfectionType> NoAntigenKnown = new List<InfectionType>();

	public AmbientEnemySpawnPoint SpawnPoint;

	public List<Character> Members = new List<Character>();

	public List<Prop> Buildings = new List<Prop>();

	public List<PitTrap> PitTraps = new List<PitTrap>();

	public List<TileObject> UnderConstructionBuildings = new List<TileObject>();

	public List<TileObject> NeedsRepair = new List<TileObject>();

	public List<Community> CachedAllies = new List<Community>();

	public List<GatheredItem> StolenGoodsRecords = new List<GatheredItem>();

	public List<ConstructionRecord> ConstructionRecords = new List<ConstructionRecord>();

	public List<PropertyDamageRecord> PropertyDamageRecords = new List<PropertyDamageRecord>();

	public List<CommunityMemberDeathNotification> PendingDeathNotifications = new List<CommunityMemberDeathNotification>();

	public List<Character> GivingBirth = new List<Character>();

	public List<CommunityRelationshipRecord> CommunityRelationships = new List<CommunityRelationshipRecord>();

	public int NextFreePropertyDamageRecordId = 1;

	public TerrainRect BaseRect = TerrainRect.Invalid;

	public TerrainRect PrisonRect = TerrainRect.Invalid;

	public List<TerrainCoord> Perimeter;

	public int InitialMemberCount;

	public int InitialChickenCount;

	public int SurrenderThreshold;

	public bool IsFEMA;

	public bool HiddenCommunity;

	public bool Nemesis;

	public int NemesisAllyWarningCount;

	public int NemesisPopulationWarningCount;

	public bool KeptAroundForReferences;

	public bool WarnedAboutTraps;

	public bool WarnedAboutTripwires;

	public bool GrantedMiningRightsToPlayer;

	public bool HasTriggeredOnKilledEvents;

	public bool PlayerSurrenderDisabled;

	public TimeSpan LastWarnedAboutTime = Target.Never;

	public TimeSpan LastRefusedPlayerSurrender = Target.Never;

	public CanOpenGates CanOpenPlayerGates;

	public EquipmentPrototype KeyProtoForAllNewSpawns;

	public int ExtortionPresenceThreshold = 8;

	public bool ExtortAISettlements = true;

	public bool BuildingsCantBeCaptured;

	public string LootLocation = string.Empty;

	public List<Squad> Squads = new List<Squad>();

	public List<Threat> Threats = new List<Threat>();

	public List<CraftingLimit> CraftingLimits = new List<CraftingLimit>();

	public List<FailedToBuryRecord> FailedToBury = new List<FailedToBuryRecord>();

	public List<EquipmentPolicy> EquipmentPolicies = new List<EquipmentPolicy>();

	public GatePolicy DefaultGatePolicy;

	public Community InvasionTarget;

	public TimeSpan InvasionTimeoutTime;

	public TimeSpan LastInvasionTime = Target.Never;

	public bool InvasionTargetWasSetFromScript;

	public TimeSpan LastAIUpdateTime;

	public TimeSpan NextPlayerShakedownTime;

	public TimeSpan NextExtortionTime;

	public TimeSpan LastExtortedFromTime = Target.Never;

	public TimeSpan SpawnedTime;

	public static int COMMUNITY_Looters = StringUtil.JenkinsHash("COMMUNITY_Looters");

	public static int COMMUNITY_Raiders = StringUtil.JenkinsHash("COMMUNITY_Raiders");

	public static int COMMUNITY_Traders = StringUtil.JenkinsHash("COMMUNITY_Traders");

	public static int COMMUNITY_Refugees = StringUtil.JenkinsHash("COMMUNITY_Refugees");

	public static int COMMUNITY_Zombies = StringUtil.JenkinsHash("COMMUNITY_Zombies");

	public static int COMMUNITY_Wildlife = StringUtil.JenkinsHash("COMMUNITY_Wildlife");

	public static int COMMUNITY_Settlers = StringUtil.JenkinsHash("COMMUNITY_Settlers");

	public static int COMMUNITY_Turned_Female = StringUtil.JenkinsHash("COMMUNITY_Turned_Female");

	public static string COMMUNITY_Turned = "COMMUNITY_Turned";

	public static string UnknownLeaderName = "Bob";

	public string UniqueID;

	public static string PilotsUnion = "PilotsUnion";

	private static GameProfiler GetRelationshipTimer = new GameProfiler("Update.GetRelationship");

	public static int HUD_NewMember = StringUtil.JenkinsHash("HUD_NewMember");

	public static int HUD_LostMember = StringUtil.JenkinsHash("HUD_LostMember");

	public static int HUD_MemberDied = StringUtil.JenkinsHash("HUD_MemberDied");

	public static int HUD_ViewCommunity = StringUtil.JenkinsHash("HUD_ViewCommunity");

	public static float LeadershipDefeatedQuantityFactor = 0.1f;

	public bool CachedAllDeadOrUnconscious;

	public bool CachedSurrendering;

	private static List<TileObject> TempPlayerObjectsInArea = new List<TileObject>();

	public int ThreatOverlap = 20;

	private static TimeSpan ForgetThreatTime = TimeSpan.FromSeconds(30.0);

	private static float MaxReactToThreatDist = 40f;

	private static List<TileObject> _nearbyObjects = new List<TileObject>();

	public bool DeleteMe;

	private static List<Community> ShakedownCommunities = new List<Community>();

	private static List<Character> AvailableEnforcers = new List<Character>();

	private static List<Character> AvailableGuards = new List<Character>();

	private static List<Character> AvailableOther = new List<Character>();

	private List<CharacterScore> TempCharacterScores = new List<CharacterScore>();

	private static SortByStrengthSkillAscending BoxerSorter = new SortByStrengthSkillAscending();

	private static List<Character> _boxers = new List<Character>();

	private static List<TileObject> _potentialTargets = new List<TileObject>();

	private static TimeSpan TimeBetweenOpinionOfLeaderUpdates = TimeSpan.FromSeconds(10.0);

	private static TimeSpan TimeBetweenAIUpdates = TimeSpan.FromSeconds(2.0);

	private int[] DesiredNumRoles = new int[18];

	private List<Character>[] CurrentMembersInRoles = new List<Character>[18];

	private static List<Character> Temp = new List<Character>();

	private static List<Character> NearbyCharacters = new List<Character>();

	private static List<Character> NearbyCharacters2 = new List<Character>();

	public float CachedHarvestedNutritionAmount = -1f;

	public float CachedPlantedNutritionAmount = -1f;

	public static float RecommendedPlantedNutritionDays = 6f;

	public static float DaysOfFrost = (float)(Weather.DaysInAMonth * 3) + (0f - Weather.AverageDaytimeWinterTemperatureInCelsius - Weather.DiurnalTemperatureVariationInCelsius * 0.5f) / (Weather.AverageDaytimeSummerTemperatureInCelsius - Weather.AverageDaytimeWinterTemperatureInCelsius) * (float)(Weather.DaysInAMonth * 6);

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Community;
	}

	public override Community GetCommunity()
	{
		return this;
	}

	public override TerrainCoord GetTile()
	{
		if (IsAISettlement())
		{
			return BaseRect.Centre;
		}
		if (Leader != null)
		{
			return Leader.GetTile();
		}
		if (Members.Count > 0)
		{
			return Members[0].GetTile();
		}
		return base.GetTile();
	}

	public static Community Spawn(CommunityType type)
	{
		Community obj = new Community
		{
			CommunityType = type,
			CommunityNameKnown = (type == CommunityType.Player)
		};
		obj.LastAIUpdateTime = (obj.SpawnedTime = Session.Instance.PlayTime);
		obj.OnSpawn();
		return obj;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		if (CommunityType == CommunityType.HunterMercenary)
		{
			Session.Instance.CommunityManager.SetRelationship(this, Session.Instance.CommunityManager.PlayerCommunity, CommunityRelationshipType.Hostile, showWarNotifications: false);
		}
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if ((CommunityNameKnown || noStrangers) && !CommunityName.IsNull())
		{
			Character character = ((Leader != null) ? Leader : ((Members.Count > 0 && string.IsNullOrEmpty(LastLeaderName)) ? Members[0] : null));
			Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
			if (character != null)
			{
				CommunityName.BuildDisplayName(sb, englishOnly, character.FirstName, character.FirstNameVerified, character.Surname, character.SurnameVerified, character.GetGender(language), CommunityNameVerified);
				return;
			}
			string text = (string.IsNullOrEmpty(LastLeaderName) ? UnknownLeaderName : LastLeaderName);
			CommunityName.BuildDisplayName(sb, englishOnly, text, StringStatus.Verified, text, StringStatus.Verified, GenderType.Count, CommunityNameVerified);
			return;
		}
		switch (CommunityType)
		{
		case CommunityType.AmbientLooter:
			sb.Append(GameImpl.Translate(COMMUNITY_Looters, englishOnly));
			break;
		case CommunityType.HunterLooter:
			sb.Append(GameImpl.Translate(COMMUNITY_Raiders, englishOnly));
			break;
		case CommunityType.RovingTrader:
			sb.Append(GameImpl.Translate(COMMUNITY_Traders, englishOnly));
			break;
		case CommunityType.RovingRefugee:
			sb.Append(GameImpl.Translate(COMMUNITY_Refugees, englishOnly));
			break;
		case CommunityType.AmbientZombie:
		case CommunityType.HunterZombie:
		case CommunityType.TemporaryZombie:
			sb.Append(GameImpl.Translate(COMMUNITY_Zombies, englishOnly));
			break;
		case CommunityType.AmbientAnimal:
			sb.Append(GameImpl.Translate(COMMUNITY_Wildlife, englishOnly));
			break;
		default:
			sb.Append(GameImpl.Translate(IsLooterCommunity() ? COMMUNITY_Looters : COMMUNITY_Settlers, englishOnly));
			break;
		}
	}

	public override string GetUniqueID()
	{
		return UniqueID;
	}

	public override void SetUniqueID(string id)
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		UniqueID = id;
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override string GetLootLocation()
	{
		return LootLocation;
	}

	public override void SetLootLocation(string lootLocation)
	{
		LootLocation = lootLocation;
	}

	public override void Init()
	{
		base.Init();
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
		Session.Instance.CommunityManager.Communities.Add(this);
		if (CommunityType == CommunityType.Player)
		{
			Session.Instance.CommunityManager.PlayerCommunity = this;
		}
		foreach (Squad squad in Squads)
		{
			squad.SquadOwner = this;
		}
	}

	public override void Delete()
	{
		StoryManager.Instance.RemoveFromInvaderInstance(this);
		Session.Instance.CommunityManager.OnCommunityDeleted(this);
		Session.Instance.CharacterManager.OnCommunityDeleted(this);
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref UniqueID, 15);
		reflector.Add(ref CommunityType);
		reflector.Add(CommunityName);
		reflector.AddAfter(ref LastLeaderName, 614);
		if (!reflector.IsDoingNetworkChecksum)
		{
			reflector.AddAfter(ref CommunityNameVerified, 595);
		}
		reflector.AddAfter(ref CommunityNameKnown, 33);
		reflector.AddAfter(ref DiscoverNameIfAtWar, 619);
		reflector.AddAfter(ref CommunityStatusShown, 458);
		if (reflector.IsDeserialising && CommunityType == CommunityType.Player)
		{
			CommunityNameKnown = true;
		}
		if (reflector.Version >= 33)
		{
			reflector.AddInfectionTypeList(ref NoAntigenKnown);
		}
		reflector.AddAfter(ref InitialMemberCount, 115);
		if (reflector.Version < 115)
		{
			InitialMemberCount = Members.Count;
		}
		else if (reflector.Version < 568 && CommunityType == CommunityType.RovingTrader && InitialMemberCount == 0)
		{
			InitialMemberCount = 3;
		}
		reflector.AddAfter(ref InitialChickenCount, 311);
		reflector.AddAfter(ref SurrenderThreshold, 563);
		reflector.AddAfter(ref LootLocation, 570);
		reflector.AddAfter(ref IsFEMA, 493);
		reflector.AddAfter(ref HiddenCommunity, 495);
		reflector.AddAfter(ref Nemesis, 335);
		reflector.AddAfter(ref NemesisAllyWarningCount, 342);
		reflector.AddAfter(ref NemesisPopulationWarningCount, 344);
		reflector.Add(ref Leader);
		reflector.Add(ref SpawnPoint);
		reflector.AddAfter(ref DefaultGatePolicy, 128);
		reflector.Add(ref InvasionTarget);
		reflector.AddAfter(ref InvasionTimeoutTime, 336);
		reflector.Add(ref LastInvasionTime);
		reflector.AddAfter(ref InvasionTargetWasSetFromScript, 343);
		reflector.Add(ref LastAIUpdateTime);
		if (reflector.Version < 329)
		{
			InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(this);
			if (invaderInstanceThatCreatedHunter != null)
			{
				SpawnedTime = invaderInstanceThatCreatedHunter.LastSpawnedTime;
			}
		}
		else
		{
			reflector.Add(ref SpawnedTime);
		}
		reflector.AddAfter(ref KeptAroundForReferences, 130);
		reflector.AddAfter(ref WarnedAboutTraps, 167);
		reflector.AddAfter(ref WarnedAboutTripwires, 476);
		if (reflector.Version < 476)
		{
			WarnedAboutTripwires = WarnedAboutTraps;
		}
		reflector.AddAfter(ref GrantedMiningRightsToPlayer, 272);
		reflector.AddAfter(ref HasTriggeredOnKilledEvents, 262);
		reflector.AddAfter(ref PlayerSurrenderDisabled, 449);
		if (reflector.Version < 449 && CommunityType == CommunityType.HunterLooter)
		{
			PlayerSurrenderDisabled = true;
		}
		reflector.AddAfter(ref LastWarnedAboutTime, 267);
		reflector.AddAfter(ref LastRefusedPlayerSurrender, 334);
		if (reflector.Version < 107)
		{
			TimeSpan value = TimeSpan.Zero;
			reflector.AddAfter(ref value, 12);
		}
		reflector.AddAfter(ref NextPlayerShakedownTime, 93);
		reflector.AddAfter(ref NextExtortionTime, 349);
		reflector.AddAfter(ref LastExtortedFromTime, 370);
		if (reflector.Version >= 49)
		{
			reflector.Add(ref Threats);
			reflector.Add(ref Squads);
		}
		if (reflector.Version >= 79)
		{
			reflector.Add(ref FailedToBury);
		}
		if (reflector.Version >= 206)
		{
			reflector.Add(ref EquipmentPolicies);
		}
		reflector.AddGameObjectRefList(ref Members);
		reflector.AddGameObjectRefList(ref Buildings);
		if (reflector.Version < 98 && (CommunityType == CommunityType.RovingRefugee || CommunityType == CommunityType.RovingTrader))
		{
			foreach (Squad squad in Squads)
			{
				if (squad.Behaviour == SquadBehaviour.Hunt)
				{
					squad.Behaviour = ((CommunityType == CommunityType.RovingRefugee) ? SquadBehaviour.Travel : SquadBehaviour.Trade);
					ControlPoint? controlPoint = CommunityManager.PickRandomMapEntrance(MathUtil.NonDeterministicRand);
					if (controlPoint.HasValue)
					{
						TerrainCoord tileCoordForPosXZ = GameTerrain.Instance.GetTileCoordForPosXZ(controlPoint.Value.Pos);
						tileCoordForPosXZ = GameTerrain.Instance.ClampTileWithinBounds(tileCoordForPosXZ);
						squad.GoalTile = tileCoordForPosXZ;
					}
				}
			}
		}
		if (reflector.Version >= 106)
		{
			reflector.Add(ref CraftingLimits);
		}
		if (reflector.Version >= 340)
		{
			reflector.Add(ref StolenGoodsRecords);
		}
		if (reflector.Version >= 185)
		{
			reflector.Add(ref ConstructionRecords);
			if (reflector.Version < 384)
			{
				for (int num = ConstructionRecords.Count - 1; num >= 0; num--)
				{
					if (ConstructionRecords[num].Proto.ProtoInstance is PlantableCrop)
					{
						ConstructionRecords.RemoveAt(num);
					}
				}
			}
		}
		if (reflector.Version >= 234)
		{
			reflector.Add(ref NextFreePropertyDamageRecordId);
			reflector.Add(ref PropertyDamageRecords);
		}
		if (reflector.Version >= 403)
		{
			reflector.Add(ref PendingDeathNotifications);
		}
		TerrainRect rect;
		if (reflector.Version >= 186)
		{
			BaseRect.Reflect(reflector);
		}
		else if (GetBaseRect(out rect))
		{
			BaseRect = rect;
		}
		if (reflector.Version >= 559)
		{
			PrisonRect.Reflect(reflector);
		}
		reflector.AddAfter(ref Perimeter, 421);
		reflector.AddAfter(ref CanOpenPlayerGates, 364);
		reflector.AddAfter(ref KeyProtoForAllNewSpawns, 550);
		reflector.AddAfter(ref ExtortionPresenceThreshold, 460);
		reflector.AddAfter(ref ExtortAISettlements, 492);
		reflector.AddAfter(ref BuildingsCantBeCaptured, 543);
		if (reflector.Version < 543)
		{
			BuildingsCantBeCaptured = UniqueID == PilotsUnion;
		}
		if (reflector.Version >= 508)
		{
			reflector.AddGameObjectRefList(ref GivingBirth);
		}
		reflector.AddAfter(ref CommunityRelationships, 569);
	}

	public void AddMember(Character character, SecrecyMode secret = SecrecyMode.Public)
	{
		if (character.Community != null)
		{
			if (character.Community == this)
			{
				return;
			}
			character.Community.RemoveMember(character, secret);
		}
		KeptAroundForReferences = false;
		Members.Add(character);
		character.Community = this;
		UpdateCachedAStarInfo();
		CachedHarvestedNutritionAmount = -1f;
		StoryManager.Instance.SetConditionsDirty();
		if (CommunityType != CommunityType.Player)
		{
			return;
		}
		character.LastSkillAtrophyTime = Session.Instance.PlayTime;
		foreach (Equipment content in character.Inventory.Contents)
		{
			content.MarkDiscovered();
		}
		Session.Instance.UpdateHighestSkillLevel(character);
		GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
		NotificationManager.Instance.CheckCommunityStats = true;
		if (InfoScreen.Instance.IsShowingCommunity())
		{
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void RemoveMember(Character character, SecrecyMode secret = SecrecyMode.Public)
	{
		RemoveMember(character, cantDeleteMeHere: false, secret);
	}

	public void RemoveMember(Character character, bool cantDeleteMeHere, SecrecyMode secret = SecrecyMode.Public)
	{
		if (character.Community != this)
		{
			return;
		}
		bool flag = HasAnyActiveMembers();
		if (character.SquadId != 0)
		{
			RemoveFromSquad(character);
		}
		if (character.SquadLeader != null)
		{
			character.Follow(null);
		}
		while (character.Followers != null && character.Followers.Count > 0)
		{
			character.Followers[0].Follow(null);
		}
		if (CommunityType == CommunityType.Player)
		{
			if (character.DirectControlled)
			{
				character.OnStopDirectControl();
			}
			character.DirectControlledCrouching = false;
			PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(character);
			if (playerControllingCharacter != null && playerControllingCharacter.IsLocal)
			{
				Session.Instance.WantSwitchControlledCharacter = GetNearestLivingNonZombieMember(character.Tile, character, BaseObjectType.Human, float.MaxValue);
			}
		}
		foreach (Target target in character.Targets)
		{
			if (target.LastAttackedMeTime == Target.Never)
			{
				target.LastAttackedUsTime = Target.Never;
			}
		}
		character.SetRank(Rank.None);
		character.CancelAllRoles();
		character.ClearEquipmentPolicies();
		character.ClearFailedFindAttempts();
		character.SetMovementZone(TerrainRect.Invalid);
		character.SetHangoutLocation(TerrainCoord.Invalid);
		character.Inventory.ClearGatheredItems();
		Members.Remove(character);
		character.Community = null;
		UpdateCachedAStarInfo();
		CachedHarvestedNutritionAmount = -1f;
		StoryManager.Instance.SetConditionsDirty();
		if (CommunityType == CommunityType.Player)
		{
			character.Feed.ClearFeed();
			GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
			NotificationManager.Instance.CheckCommunityStats = true;
			if (InfoScreen.Instance.IsShowingCommunity())
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
			if (secret == SecrecyMode.Private)
			{
				NotificationManager.Instance.SuppressNextAggroNotification = true;
			}
		}
		if (IsTemporaryCommunity() && Members.Count == 0 && Buildings.Count == 0)
		{
			if (CommunityManager.IsAnyoneImportantReferencingObject(this))
			{
				KeptAroundForReferences = true;
			}
			else if (cantDeleteMeHere)
			{
				DeleteMe = true;
			}
			else
			{
				Delete();
			}
		}
		if (flag && !HasAnyActiveMembers())
		{
			OnCommunityHasNoActiveMembers(secret);
		}
	}

	public void AddMemberWithNotifications(Character character)
	{
		AddMemberWithNotifications(character, SecrecyMode.Public, kicked: false, kickedDueToInfection: false, null);
	}

	public void AddMemberWithNotifications(Character character, SecrecyMode secret, bool kicked, bool kickedDueToInfection)
	{
		AddMemberWithNotifications(character, secret, kicked, kickedDueToInfection, null);
	}

	public void AddMemberWithNotifications(Character character, SecrecyMode secret, bool kicked, bool kickedDueToInfection, Character kickedBy)
	{
		Community community = character.Community;
		if (community == this)
		{
			return;
		}
		if (community != null && community.CommunityType == CommunityType.Player)
		{
			if (secret != SecrecyMode.Private)
			{
				string text = LogEventBehaviour.BuildCommunityMemberLeftMsg(character);
				NotificationManager.Instance.AddCharacterNotification(character, HUD_LostMember, text, HUD_ViewCommunity, typeof(CommunityPage));
				LogEvent logEvent = new LogEvent(LogEventType.CommunityMemberLeft);
				logEvent.Character = character;
				Session.Instance.AddLogEvent(logEvent);
			}
			foreach (Equipment content in character.Inventory.Contents)
			{
				content.ForceSetTradedAmount(0);
			}
		}
		if (character.GetBaseObjectType() == BaseObjectType.Human)
		{
			if (kickedBy == null && community != null)
			{
				kickedBy = community.Leader;
			}
			if (kicked && kickedBy != null)
			{
				if (kickedDueToInfection)
				{
					Memory.OnMemorableEvent(MemoryPrototype.KickedPossibleInvisibleStrain, kickedBy, character, 1f, secret);
				}
				else
				{
					Memory.OnMemorableEvent(MemoryPrototype.KickedFromCommunity, kickedBy, character, 1f, secret);
				}
			}
			else
			{
				Memory.OnMemorableEvent(MemoryPrototype.LeftCommunity, character, community, 1f, (secret == SecrecyMode.Public) ? SecrecyMode.OnlyKnownToObjectCommunity : secret);
			}
			foreach (Character member in Members)
			{
				character.DeleteMemory(MemoryPrototype.LeftCommunity, member, community);
				character.DeleteMemory(MemoryPrototype.PoachedFrom, member, community);
			}
		}
		character.SetRank(Rank.None);
		character.JoinedCommunityTime = Session.Instance.PlayTime;
		AddMember(character, secret);
		if (CommunityType == CommunityType.Player)
		{
			for (int num = character.Inventory.Count - 1; num >= 0; num--)
			{
				Equipment item = character.Inventory.GetItem(num);
				if (item.GetTradedAmount() > 0 && (character.EquippedItem != item || item.GetTradedAmount() < item.GetAmount()) && !character.IsWearing(item))
				{
					character.Inventory.Take(character, item, item.GetTradedAmount()).Delete();
				}
			}
			character.NameKnown = true;
			character.Investigated = true;
			character.DontSimulateSurvivalFactorsUntilJoinCommunity = false;
			character.SetAllSkillsKnown();
			character.CanFollowPlayer = false;
			character.ReservedGoldAmount = 0;
			string text2 = LogEventBehaviour.BuildCommunityMemberJoinedMsg(character);
			NotificationManager.Instance.AddCharacterNotification(character, HUD_NewMember, text2, HUD_ViewCommunity, typeof(CommunityPage));
			LogEvent logEvent2 = new LogEvent(LogEventType.CommunityMemberJoined);
			logEvent2.Character = character;
			Session.Instance.AddLogEvent(logEvent2);
			if (community != null && character.GetBaseObjectType() == BaseObjectType.Human)
			{
				switch (community.CommunityType)
				{
				case CommunityType.RovingRefugee:
					AchievementsManager.Instance.UnlockAchievement(Achievement.Recruit_Refugee);
					break;
				case CommunityType.HunterLooter:
					AchievementsManager.Instance.UnlockAchievement(Achievement.Recruit_Looter);
					break;
				case CommunityType.Normal:
				case CommunityType.Looter:
					AchievementsManager.Instance.UnlockAchievement(Achievement.Recruit_Settler);
					break;
				}
			}
			if (character is Chicken)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.AdoptAChicken);
			}
			if (GetLivingNonZombieMemberCount() >= 20)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.BuildLargeCommunity);
			}
			if (character.UniqueID == Character.JoeWheeler)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.Story_RecruitJoeWheeler);
			}
			if (!IsLoneWolfCommunity())
			{
				Session.Instance.LoneWolf = false;
			}
		}
		else if (this != character.InitialCommunity)
		{
			character.InitialCommunity = this;
			character.InitialHangOutLocation = TerrainCoord.Invalid;
		}
		if ((CommunityType == CommunityType.Temporary || CommunityType == CommunityType.TemporaryLooter) && Squads.Count > 0 && character.AliveAndNotZombie)
		{
			AddToSquad(character, Squads[0]);
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.JoinedCommunity, character, this);
	}

	public void RemoveMemberWithNotifications(Character character)
	{
		RemoveMemberWithNotifications(character, SecrecyMode.Public, kicked: false);
	}

	public void RemoveMemberWithNotifications(Character character, SecrecyMode secret, bool kicked)
	{
		if (character.Community != null && character.Community.CommunityType == CommunityType.Player && character.AliveAndNotZombie)
		{
			string text = LogEventBehaviour.BuildCommunityMemberLeftMsg(character);
			NotificationManager.Instance.AddCharacterNotification(character, HUD_LostMember, text, HUD_ViewCommunity, typeof(CommunityPage));
			LogEvent logEvent = new LogEvent(LogEventType.CommunityMemberLeft);
			logEvent.Character = character;
			Session.Instance.AddLogEvent(logEvent);
		}
		if (kicked)
		{
			Memory.OnMemorableEvent(MemoryPrototype.KickedFromCommunity, Leader, character, 1f, secret);
		}
		else
		{
			Memory.OnMemorableEvent(MemoryPrototype.LeftCommunity, character, this, 1f, (secret == SecrecyMode.Public) ? SecrecyMode.OnlyKnownToObjectCommunity : secret);
		}
		character.SetRank(Rank.None);
		RemoveMember(character);
	}

	public void AddBuilding(Prop building)
	{
		if (building.Community != null)
		{
			if (building.Community == this)
			{
				return;
			}
			building.Community.RemoveBuilding(building);
		}
		Buildings.Add(building);
		building.Community = this;
		if (building.UnderConstructionInfo != null && !UnderConstructionBuildings.Contains(building))
		{
			UnderConstructionBuildings.Add(building);
		}
		if (building.CanBeRepaired())
		{
			NeedsRepair.Add(building);
		}
		CachedHarvestedNutritionAmount = -1f;
		StoryManager.Instance.SetConditionsDirty();
		if (CommunityType != CommunityType.Player)
		{
			return;
		}
		if (building.Prototype != null && building.UnderConstructionInfo == null)
		{
			building.Prototype.MarkDiscovered();
		}
		foreach (Equipment content in building.Inventory.Contents)
		{
			content.MarkDiscovered();
		}
		NotificationManager.Instance.CheckCommunityStats = true;
		NotificationManager.Instance.ShowRecipeDiscoveredNotifications();
	}

	public void RemoveBuilding(Prop building)
	{
		if (building.Community == this)
		{
			if (building.UnderConstructionInfo != null)
			{
				UnderConstructionBuildings.Remove(building);
			}
			if (building.CanBeRepaired())
			{
				NeedsRepair.Remove(building);
			}
			Buildings.Remove(building);
			building.Community = null;
			if (CommunityType == CommunityType.Player)
			{
				building.CaptureAmount = 0f;
			}
			CachedHarvestedNutritionAmount = -1f;
			StoryManager.Instance.SetConditionsDirty();
			if (CommunityType == CommunityType.Player)
			{
				NotificationManager.Instance.CheckCommunityStats = true;
			}
			if (IsTemporaryCommunity() && Members.Count == 0 && Buildings.Count == 0)
			{
				Delete();
			}
		}
	}

	public void AddConstructionRecord(PropPrototype proto, TerrainCoord tile, Prop.OrientationType orientation)
	{
		if (IsAISettlement() && !(proto.ProtoInstance is Grave) && !(proto.ProtoInstance is ITrap) && !(proto.ProtoInstance is PlantableCrop))
		{
			ConstructionRecord item = new ConstructionRecord
			{
				Proto = proto,
				Tile = tile,
				Orientation = orientation
			};
			ConstructionRecords.Add(item);
		}
	}

	public void OnPropertyDamaged(TerrainCoord tile, float radius, TileObject obj, Character source)
	{
		if (obj.CanBeRepaired() && !NeedsRepair.Contains(obj))
		{
			NeedsRepair.Add(obj);
		}
		for (int num = PropertyDamageRecords.Count - 1; num >= 0; num--)
		{
			if (PropertyDamageRecords[num].Region.GetClosestDistSqTo(tile) < MathUtil.Squared(radius + 10f) && PropertyDamageRecords[num].LastDamagedTime >= Session.Instance.PlayTime - TimeSpan.FromSeconds(60.0))
			{
				if (!(radius > 0f) || (!PropertyDamageRecords[num].HasAssignedBlame && !PropertyDamageRecords[num].Investigated))
				{
					PropertyDamageRecord value = PropertyDamageRecords[num];
					value.LastDamagedTime = Session.Instance.PlayTime;
					value.MaxDamagedHeight = Math.Max(value.MaxDamagedHeight, obj.Height);
					PropertyDamageRecords[num] = value;
					return;
				}
			}
			else if (PropertyDamageRecords[num].LastDamagedTime < Session.Instance.PlayTime - TimeSpan.FromSeconds(600.0))
			{
				PropertyDamageRecords.RemoveAt(num);
			}
		}
		PropertyDamageRecord item = new PropertyDamageRecord
		{
			PropertyDamageRecordId = NextFreePropertyDamageRecordId++,
			LastDamagedTime = Session.Instance.PlayTime,
			MaxDamagedHeight = obj.Height,
			Region = new TerrainRect(tile - new TerrainCoord(Mathf.CeilToInt(radius), Mathf.CeilToInt(radius)), tile + new TerrainCoord(Mathf.CeilToInt(radius), Mathf.CeilToInt(radius)))
		};
		PropertyDamageRecords.Add(item);
		Debug.Log("New property damage record for " + obj.GetDisplayNameString() + " (source: " + ((source != null) ? source.GetDisplayNameString() : "unknown") + ")");
	}

	public TerrainRect GetPropertyDamageRegion(int id)
	{
		for (int i = 0; i < PropertyDamageRecords.Count; i++)
		{
			if (PropertyDamageRecords[i].PropertyDamageRecordId == id)
			{
				return PropertyDamageRecords[i].Region;
			}
		}
		return TerrainRect.Invalid;
	}

	public TimeSpan GetTimeSinceLastPropertyDamaged(int id)
	{
		for (int i = 0; i < PropertyDamageRecords.Count; i++)
		{
			if (PropertyDamageRecords[i].PropertyDamageRecordId == id)
			{
				return Session.Instance.PlayTime - PropertyDamageRecords[i].LastDamagedTime;
			}
		}
		return TimeSpan.MaxValue;
	}

	public bool IsPropertyDamageInvestigationFinished(int id)
	{
		for (int i = 0; i < PropertyDamageRecords.Count; i++)
		{
			if (PropertyDamageRecords[i].PropertyDamageRecordId == id)
			{
				if (!PropertyDamageRecords[i].HasAssignedBlame)
				{
					return PropertyDamageRecords[i].Investigated;
				}
				return true;
			}
		}
		return true;
	}

	public void SetPropertyDamageInvestigated(int id)
	{
		for (int i = 0; i < PropertyDamageRecords.Count; i++)
		{
			if (PropertyDamageRecords[i].PropertyDamageRecordId == id)
			{
				PropertyDamageRecord value = PropertyDamageRecords[i];
				value.Investigated = true;
				PropertyDamageRecords[i] = value;
				break;
			}
		}
	}

	public void AddStolenGoodsRecord(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, int amount)
	{
		for (int i = 0; i < StolenGoodsRecords.Count; i++)
		{
			if (StolenGoodsRecords[i].Type == proto && StolenGoodsRecords[i].Liquid == liquid)
			{
				StolenGoodsRecords[i] = GatheredItem.Create(proto, liquid, infectionType, StolenGoodsRecords[i].Amount + amount);
				return;
			}
		}
		StolenGoodsRecords.Add(GatheredItem.Create(proto, liquid, infectionType, amount));
	}

	public void RemoveStolenGoodsRecord(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, int amount)
	{
		for (int i = 0; i < StolenGoodsRecords.Count; i++)
		{
			if (StolenGoodsRecords[i].Type == proto && StolenGoodsRecords[i].Liquid == liquid)
			{
				if (StolenGoodsRecords[i].Amount > amount)
				{
					StolenGoodsRecords[i] = GatheredItem.Create(proto, liquid, infectionType, StolenGoodsRecords[i].Amount - amount);
				}
				else
				{
					StolenGoodsRecords.RemoveAt(i);
				}
				break;
			}
		}
	}

	public void UpdateLeader()
	{
		CommunityName.ClearCache();
		Leader = null;
		foreach (Character member in Members)
		{
			if (member.GetRank() == Rank.Leader)
			{
				Leader = member;
				LastLeaderName = Leader.FirstName;
				break;
			}
		}
	}

	public bool IsSurrendering()
	{
		if (Leader != null && Leader.ConsciousAndNotZombie)
		{
			return Leader.CurrentActionAnim == ActionAnim.HandsUp;
		}
		foreach (Character member in Members)
		{
			if (member.ConsciousAndNotZombie)
			{
				return member.CurrentActionAnim == ActionAnim.HandsUp;
			}
		}
		return false;
	}

	public bool HasSurrenderedTo(Community otherCommunity)
	{
		bool community1Surrendered;
		return GetRelationship(otherCommunity, out community1Surrendered) == CommunityRelationshipType.Ceasefire && community1Surrendered;
	}

	public CommunityRelationshipType GetRelationship(Community community2)
	{
		bool community1Surrendered;
		return GetRelationship(community2, out community1Surrendered);
	}

	public CommunityRelationshipType GetRelationship(Community community2, out bool community1Surrendered)
	{
		using (new ProfileMarker(GetRelationshipTimer))
		{
			community1Surrendered = false;
			if (community2 == null)
			{
				return CommunityRelationshipType.Unknown;
			}
			if (this == community2)
			{
				return CommunityRelationshipType.Known;
			}
			if (IsZombieCommunity() && community2.IsZombieCommunity())
			{
				return CommunityRelationshipType.Unknown;
			}
			if (IsAnimalCommunity() || community2.IsAnimalCommunity())
			{
				return CommunityRelationshipType.Unknown;
			}
			if (IsAlwaysHostileCommunity() || community2.IsAlwaysHostileCommunity())
			{
				if (CommunityType == CommunityType.Player || community2.CommunityType == CommunityType.Player)
				{
					for (int i = 0; i < CommunityRelationships.Count; i++)
					{
						if (CommunityRelationships[i].OtherCommunityId == community2.Id)
						{
							community1Surrendered = CommunityRelationships[i].Initiator && CommunityRelationships[i].RelationshipType == CommunityRelationshipType.Ceasefire;
							return CommunityRelationships[i].RelationshipType;
						}
					}
				}
				return CommunityRelationshipType.Hostile;
			}
			for (int j = 0; j < CommunityRelationships.Count; j++)
			{
				if (CommunityRelationships[j].OtherCommunityId == community2.Id)
				{
					community1Surrendered = CommunityRelationships[j].Initiator && CommunityRelationships[j].RelationshipType == CommunityRelationshipType.Ceasefire;
					return CommunityRelationships[j].RelationshipType;
				}
			}
			return CommunityRelationshipType.Unknown;
		}
	}

	public CommunityRelationshipType SetCommunityRelationship(Community otherCommunity, CommunityRelationshipType relationshipType, bool initiator)
	{
		for (int i = 0; i < CommunityRelationships.Count; i++)
		{
			if (CommunityRelationships[i].OtherCommunityId == otherCommunity.Id)
			{
				CommunityRelationshipRecord value = CommunityRelationships[i];
				CommunityRelationshipType relationshipType2 = value.RelationshipType;
				value.RelationshipType = relationshipType;
				value.Initiator = initiator;
				CommunityRelationships[i] = value;
				return relationshipType2;
			}
		}
		CommunityRelationships.Add(new CommunityRelationshipRecord(otherCommunity.Id, relationshipType, initiator));
		return CommunityRelationshipType.Unknown;
	}

	public void ClearCommunityRelationship(Community otherCommunity)
	{
		for (int i = 0; i < CommunityRelationships.Count; i++)
		{
			if (CommunityRelationships[i].OtherCommunityId == otherCommunity.Id)
			{
				CommunityRelationships.RemoveAt(i);
				break;
			}
		}
	}

	public bool HasOverlappingAllianceWith(Community otherCommunity)
	{
		if (CachedAllies.Contains(otherCommunity))
		{
			return true;
		}
		foreach (Community cachedAlly in otherCommunity.CachedAllies)
		{
			if (CachedAllies.Contains(cachedAlly))
			{
				return true;
			}
		}
		return false;
	}

	public void OnNewGame()
	{
		InitialMemberCount = GetLivingNonZombieMemberCount();
		InitialChickenCount = GetLivingNonZombieMemberCountBySpecies(BaseObjectType.Chicken);
		if (IsAISettlement() && BaseRect.VerticesArea == 0 && GetBaseRect(out var rect))
		{
			BaseRect = rect;
		}
		if (CommunityType == CommunityType.Looter)
		{
			NextExtortionTime = TimeSpan.FromSeconds(Mathf.Lerp(0f, Sun.DayLengthSecs * 2f, MathUtil.RandomFloat(Id)));
		}
		if (IsAISettlement())
		{
			LastExtortedFromTime = TimeSpan.FromSeconds(Mathf.Lerp((0f - Sun.DayLengthSecs) * 7f, 0f, MathUtil.RandomFloat(Id + 1000)));
		}
		FixupTraders();
	}

	public void FixupTraders()
	{
		if ((CommunityType != CommunityType.RovingTrader && CommunityType != CommunityType.RovingRefugee) || Squads.Count != 0 || !HasAnyLivingNonZombieMembers())
		{
			return;
		}
		Squad squad = AddSquad(SquadBehaviour.Trade, 0);
		if (Leader != null && Leader.AliveAndNotZombie && Leader.GetBaseObjectType() == BaseObjectType.Human)
		{
			AddToSquad(Leader, squad);
		}
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member != Leader && !member.Disappeared)
			{
				AddToSquad(member, squad);
			}
		}
		CustomRandom rand = new CustomRandom(Id);
		GoToNextTradeDestination(squad, rand, mustMove: false, canOccupyBases: false);
	}

	public void OnExtortion(Community targetCommunity)
	{
		OnExtortion(targetCommunity, Mathf.Lerp(1f, 3f, Session.Instance.DeterministicRand.RandomFloat()));
	}

	public void OnExtortion(Community targetCommunity, float days)
	{
		if (CommunityType == CommunityType.Looter)
		{
			NextExtortionTime = MathUtil.Max(NextExtortionTime, Session.Instance.PlayTime) + TimeSpan.FromSeconds(days * Sun.DayLengthSecs);
		}
	}

	public void OnShakedown(Character victim, float gold)
	{
		if (CommunityType == CommunityType.Looter && victim.IsControllableByPlayer())
		{
			NextPlayerShakedownTime = MathUtil.Max(NextPlayerShakedownTime, Session.Instance.PlayTime) + TimeSpan.FromSeconds(gold * 60f);
		}
	}

	public bool IsTimeToShakedownAgain(Character actor, Character victim)
	{
		if (CommunityType == CommunityType.Looter && victim.IsControllableByPlayer() && NextPlayerShakedownTime != TimeSpan.Zero && Session.Instance.PlayTime >= NextPlayerShakedownTime && !GrantedMiningRightsToPlayer && Session.Instance.CommunityManager.GetRelationship(this, victim.Community) == CommunityRelationshipType.Known)
		{
			foreach (Character member in Members)
			{
				if (member.AliveAndNotZombie)
				{
					if (actor != member && member.FindActiveGoal(GoalType.Conversation) != null)
					{
						return false;
					}
					if (member.InCombat)
					{
						return false;
					}
					if (member.SparringPartner != null)
					{
						return false;
					}
				}
			}
			if (StoryManager.Instance.HasQueuedShakedownEvents())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void OnMemberKnockedUnconscious(Character member)
	{
		Squad squad = member.GetSquad();
		if (squad != null)
		{
			SquadBehaviour behaviour = squad.Behaviour;
			if ((behaviour == SquadBehaviour.Funeral || (uint)(behaviour - 8) <= 4u) && !IsHunterCommunity() && !IsTemporaryCommunity())
			{
				RemoveFromSquad(member);
			}
		}
		foreach (Character member2 in Members)
		{
			if (member2 != member && member2.AliveAndNotZombie)
			{
				member2.GetTarget(member)?.OnTargetedCommunityMemberDiedOrKnockedUnconscious(member2);
			}
		}
		UpdateCachedAStarInfo();
	}

	public void OnMemberRevived(Character member)
	{
		UpdateCachedAStarInfo();
	}

	public void OnMemberDied(Character member, Character killer, SecrecyMode secret, bool justActivatedInvisibleStrain = false)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		Squad squad = member.GetSquad();
		if (squad != null && squad.Behaviour == SquadBehaviour.Funeral)
		{
			AddFailedToBury(member);
		}
		RemoveFromSquad(member);
		foreach (Character member2 in Members)
		{
			if (member2 != member && member2.AliveAndNotZombie)
			{
				member2.GetTarget(member)?.OnTargetedCommunityMemberDiedOrKnockedUnconscious(member2);
			}
		}
		if (member == Leader)
		{
			foreach (Character member3 in Members)
			{
				member3.VotedFor = null;
			}
		}
		if (!member.Zombie || justActivatedInvisibleStrain)
		{
			if (CommunityType == CommunityType.Player)
			{
				GameTerrain.Instance.FogOfWar.UpdateHasWorldMap();
				if (InfoScreen.Instance.IsShowingCommunity())
				{
					InfoScreen.Instance.WantRepopulate = true;
				}
				if (secret == SecrecyMode.Private || secret == SecrecyMode.IgnoredByObjectCommunity)
				{
					PendingDeathNotifications.Add(new CommunityMemberDeathNotification
					{
						Killer = killer,
						Victim = member
					});
				}
				else
				{
					OnPlayerCommunityMemberDeathNotification(member, killer);
				}
				if (member == Leader && GetLivingNonZombieMemberCount() <= 1)
				{
					foreach (Character member4 in Members)
					{
						if (member4.AliveAndNotZombie && member4.GetBaseObjectType() == BaseObjectType.Human && member4.GetUniqueID() != Character.KellySalas && member4.GetUniqueID() != Character.EmmaOConnor)
						{
							member4.SetRank(Rank.Leader);
							break;
						}
					}
				}
				if (GetLivingNonZombieMemberCount() == 0)
				{
					StoryEvent storyEvent = new StoryEvent
					{
						Type = StoryEventType.GameOver,
						Delay = 5f
					};
					StoryManager.Instance.QueuedEvents.Add(QueuedEvent.Create(storyEvent, null, null, null, default(MemoryParam), Session.Instance.PlayTime));
				}
			}
			else if (communityManager.GetRelationship(communityManager.PlayerCommunity, this) == CommunityRelationshipType.Allied)
			{
				foreach (Character member5 in Members)
				{
					if (member5.AliveAndNotZombie)
					{
						member5.AddMemory(MemoryPrototype.LeadershipCausedDeath, communityManager.PlayerCommunity.Leader, member, 1f);
					}
				}
			}
		}
		if (member.Zombie == IsZombieCommunity() || justActivatedInvisibleStrain)
		{
			if (IsAISettlement() && !InvasionTargetWasSetFromScript && member.Killer != null && member.Killer.GetBaseObjectType() == BaseObjectType.Human && member.Killer.Community != null && !member.Killer.Community.IsAmbientCommunity() && (member.Killer.Community.CommunityType == CommunityType.Player || Session.Instance.CommunityManager.PlayerCommunity.CachedAllies.Contains(member.Killer.Community)) && !member.Killer.Zombie && communityManager.GetRelationship(this, member.Killer.Community) == CommunityRelationshipType.Hostile)
			{
				Squad squad2 = member.GetSquad();
				if (squad2 == null || squad2.Behaviour != SquadBehaviour.Hunt)
				{
					SetInvasionTarget(member.Killer.Community, 7f, fromScript: false);
				}
			}
			if (SpawnPoint != null)
			{
				SpawnPoint.OnSpawnedEnemyDied();
			}
			if (member.GetBaseObjectType() != BaseObjectType.Human == IsAnimalCommunity() && !HasAnyActiveMembers())
			{
				if (SpawnPoint != null)
				{
					SpawnPoint.OnSpawnedEnemyGroupDied();
				}
				OnCommunityHasNoActiveMembers();
			}
		}
		CachedHarvestedNutritionAmount = -1f;
		UpdateCachedAStarInfo();
	}

	public void ProcessPendingDeathNotificationsForMember(Character observer, Character member)
	{
		for (int i = 0; i < PendingDeathNotifications.Count; i++)
		{
			if (PendingDeathNotifications[i].Victim == member && PendingDeathNotifications[i].Killer != observer && MathUtil.ToXZ(observer.Pos - member.Pos).sqrMagnitude < (float)(InvisibleStrainSpreadGoal.MinObserverRange * InvisibleStrainSpreadGoal.MinObserverRange))
			{
				OnPlayerCommunityMemberDeathNotification(PendingDeathNotifications[i].Victim, PendingDeathNotifications[i].Killer);
				PendingDeathNotifications.RemoveAt(i);
				break;
			}
		}
	}

	public void OnPlayerCommunityMemberDeathNotification(Character member, Character killer)
	{
		if (Leader != null && Leader.AliveAndNotZombie && member.IsDeathSignificant(member.Killer))
		{
			foreach (Character member2 in Members)
			{
				if (member2.AliveAndNotZombie && member2 != Leader && killer != Leader)
				{
					member2.AddMemory(MemoryPrototype.LeadershipCausedDeath, Leader, member, 1f);
				}
			}
		}
		string text = LogEventBehaviour.BuildCommunityMemberDiedMsg(member);
		NotificationManager.Instance.AddCharacterNotification(member, HUD_MemberDied, text, HUD_ViewCommunity, typeof(CommunityPage));
		LogEvent logEvent = new LogEvent(LogEventType.CommunityMemberDied);
		logEvent.Character = member;
		Session.Instance.AddLogEvent(logEvent);
	}

	public void OnCommunityHasNoActiveMembers(SecrecyMode secret = SecrecyMode.Public)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		if (!IsZombieCommunity() && !IsAnimalCommunity() && CommunityType != CommunityType.Player)
		{
			if ((CommunityType == CommunityType.Normal || CommunityType == CommunityType.Looter) && CommunityNameKnown && secret != SecrecyMode.Private && !HiddenCommunity)
			{
				string text = GameImpl.Translate("HUD_CommunityWipedOut");
				text = text.Replace("%1", GetDisplayNameString(noStrangers: false, englishOnly: false));
				HudBehaviour.Instance.SetStatusBarMsg(text);
			}
			bool flag = false;
			if (secret != SecrecyMode.Private && communityManager.GetRelationship(this, communityManager.PlayerCommunity) == CommunityRelationshipType.Hostile)
			{
				flag = !IsAmbientCommunity() || HasAnyoneInCommunityBeenInjuredBy(communityManager.PlayerCommunity);
			}
			if (flag)
			{
				Memory.OnMemorableEvent(MemoryPrototype.LeadershipDefeated, communityManager.PlayerCommunity.Leader, this, (float)InitialMemberCount * LeadershipDefeatedQuantityFactor, SecrecyMode.OnlyKnownToSubjectCommunity);
			}
			while (UnderConstructionBuildings.Count > 0)
			{
				UnderConstructionBuildings[0].Delete();
			}
		}
		if (CommunityType == CommunityType.Looter || (CommunityType == CommunityType.Normal && IsFEMA))
		{
			StoryManager.Instance.DeactivateInvader(CommunityManager.Invaders_HiredMercs, this);
		}
		InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(this);
		if (invaderInstanceThatCreatedHunter != null && !HasTriggeredOnKilledEvents)
		{
			HasTriggeredOnKilledEvents = true;
			StoryManager.QueueEvents(invaderInstanceThatCreatedHunter.Invader.OnKilledEvents, null, null, this, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
		}
	}

	public void RemoveUnfinishedGraves()
	{
		for (int num = Buildings.Count - 1; num >= 0; num--)
		{
			Prop prop = Buildings[num];
			if (prop.GetBaseObjectType() == BaseObjectType.Grave && prop.GetUnderConstructionInfo() != null)
			{
				prop.Delete();
			}
		}
	}

	public bool HasAnyoneInCommunityBeenInjuredBy(Community community)
	{
		foreach (Character member in Members)
		{
			for (int i = 0; i < member.Memories.Count; i++)
			{
				if ((member.Memories[i].Prototype == MemoryPrototype.Killed || member.Memories[i].Prototype == MemoryPrototype.Hurt) && member.Memories[i].Actor != null && member.Memories[i].Actor.Community == community && !member.Memories[i].Actor.Zombie && member.Memories[i].Object != null && member.Memories[i].Object.GetCommunity() == this)
				{
					return true;
				}
			}
			if (member.Killer != null && member.Killer.Community == community && member.CauseOfDeath != CauseOfDeath.Zombie)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateCachedAStarInfo()
	{
		bool flag = !IsAnyoneConscious(includeDrunk: true);
		bool flag2 = IsSurrendering();
		if (flag != CachedAllDeadOrUnconscious)
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.SetCommunityAllDeadOrUnconscious(Id, flag));
		}
		if (flag2 != CachedSurrendering)
		{
			GameTerrain.Instance.AStar.AddChange(AStarChange.SetCommunitySurrendering(Id, flag2));
		}
		CachedAllDeadOrUnconscious = flag;
		CachedSurrendering = flag2;
	}

	public void SetCraftingLimit(EquipmentPrototype proto, int limit)
	{
		for (int i = 0; i < CraftingLimits.Count; i++)
		{
			if (CraftingLimits[i].Proto == proto)
			{
				if (limit == int.MaxValue)
				{
					CraftingLimits.RemoveAt(i);
					return;
				}
				CraftingLimit value = CraftingLimits[i];
				value.Limit = limit;
				CraftingLimits[i] = value;
				return;
			}
		}
		if (limit != int.MaxValue)
		{
			CraftingLimit item = new CraftingLimit
			{
				Proto = proto,
				Limit = limit
			};
			CraftingLimits.Add(item);
		}
	}

	public void SetCraftingLimit(LiquidPrototype liquid, int limit)
	{
		for (int i = 0; i < CraftingLimits.Count; i++)
		{
			if (CraftingLimits[i].Liquid == liquid)
			{
				if (limit == int.MaxValue)
				{
					CraftingLimits.RemoveAt(i);
					return;
				}
				CraftingLimit value = CraftingLimits[i];
				value.Limit = limit;
				CraftingLimits[i] = value;
				return;
			}
		}
		if (limit != int.MaxValue)
		{
			CraftingLimit item = new CraftingLimit
			{
				Liquid = liquid,
				Limit = limit
			};
			CraftingLimits.Add(item);
		}
	}

	public int GetCraftingLimit(EquipmentPrototype proto)
	{
		for (int i = 0; i < CraftingLimits.Count; i++)
		{
			if (CraftingLimits[i].Proto == proto)
			{
				return CraftingLimits[i].Limit;
			}
		}
		return int.MaxValue;
	}

	public int GetCraftingLimit(LiquidPrototype liquid)
	{
		for (int i = 0; i < CraftingLimits.Count; i++)
		{
			if (CraftingLimits[i].Liquid == liquid)
			{
				return CraftingLimits[i].Limit;
			}
		}
		return int.MaxValue;
	}

	public bool HasReachedCraftingLimitForItem(Equipment item)
	{
		if (item.GetLiquidContentsType() != null)
		{
			int craftingLimit = GetCraftingLimit(item.GetLiquidContentsType());
			if (craftingLimit != int.MaxValue && GetTotalLiquid(item.GetLiquidContentsType()) >= (float)craftingLimit)
			{
				return true;
			}
		}
		else
		{
			int craftingLimit2 = GetCraftingLimit(item.GetPrototype());
			if (craftingLimit2 != int.MaxValue && CountInventoryItemsOfType(item.GetPrototype()) >= craftingLimit2)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasReachedCraftingLimitForProduct(Recipe recipe)
	{
		if (recipe.ProductPrototype != null)
		{
			int craftingLimit = GetCraftingLimit(recipe.ProductPrototype);
			if (craftingLimit != int.MaxValue && CountInventoryItemsOfType(recipe.ProductPrototype) >= craftingLimit)
			{
				return true;
			}
		}
		if (recipe.ProductLiquidPrototype != null)
		{
			int craftingLimit2 = GetCraftingLimit(recipe.ProductLiquidPrototype);
			if (craftingLimit2 != int.MaxValue && GetTotalLiquid(recipe.ProductLiquidPrototype) >= (float)craftingLimit2)
			{
				return true;
			}
		}
		return false;
	}

	public void SetGatePolicy(Gate targetGate, GatePolicy gatePolicy)
	{
		DefaultGatePolicy = gatePolicy;
		foreach (Prop building in Buildings)
		{
			if (building is Gate gate && (gate == targetGate || !gate.OverrideDefaultGatePolicy))
			{
				gate.SetGatePolicy(gatePolicy, overrideDefault: false);
			}
		}
		foreach (Character member in Members)
		{
			member.ClearFailedFindAttempts();
		}
	}

	public void SetNoAntigenKnown(InfectionType infectionType)
	{
		if (!NoAntigenKnown.Contains(infectionType))
		{
			NoAntigenKnown.Add(infectionType);
		}
	}

	public void ClearNoAntigenKnown(InfectionType infectionType)
	{
		NoAntigenKnown.Remove(infectionType);
	}

	public bool IsNoAntigenKnown(InfectionType infectionType)
	{
		return NoAntigenKnown.Contains(infectionType);
	}

	public bool IsNeutralCommunity()
	{
		CommunityType communityType = CommunityType;
		if (communityType == CommunityType.Normal || (uint)(communityType - 9) <= 2u)
		{
			return true;
		}
		return false;
	}

	public bool IsAmbientCommunity()
	{
		CommunityType communityType = CommunityType;
		if ((uint)(communityType - 4) <= 6u || communityType == CommunityType.HunterMercenary)
		{
			return true;
		}
		return false;
	}

	public bool IsLooterCommunity()
	{
		switch (CommunityType)
		{
		case CommunityType.Looter:
		case CommunityType.Psycho:
		case CommunityType.AmbientLooter:
		case CommunityType.HunterLooter:
		case CommunityType.TemporaryLooter:
		case CommunityType.HunterMercenary:
			return true;
		default:
			return false;
		}
	}

	public bool IsAlwaysHostileCommunity()
	{
		switch (CommunityType)
		{
		case CommunityType.Psycho:
		case CommunityType.AmbientZombie:
		case CommunityType.AmbientLooter:
		case CommunityType.HunterZombie:
		case CommunityType.HunterLooter:
		case CommunityType.TemporaryZombie:
			return true;
		default:
			return false;
		}
	}

	public bool IsAlwaysHostileToPlayerCommunity()
	{
		if (!IsAlwaysHostileCommunity())
		{
			return CommunityType == CommunityType.HunterMercenary;
		}
		return true;
	}

	public bool IsZombieCommunity()
	{
		CommunityType communityType = CommunityType;
		if (communityType == CommunityType.AmbientZombie || communityType == CommunityType.HunterZombie || communityType == CommunityType.TemporaryZombie)
		{
			return true;
		}
		return false;
	}

	public bool IsHunterCommunity()
	{
		CommunityType communityType = CommunityType;
		if ((uint)(communityType - 7) <= 3u || communityType == CommunityType.HunterMercenary)
		{
			return true;
		}
		return false;
	}

	public bool IsTemporaryCommunity()
	{
		CommunityType communityType = CommunityType;
		if ((uint)(communityType - 11) <= 2u)
		{
			return true;
		}
		return false;
	}

	public bool IsAnimalCommunity()
	{
		if (CommunityType == CommunityType.AmbientAnimal)
		{
			return true;
		}
		return false;
	}

	public bool IsAISettlement()
	{
		CommunityType communityType = CommunityType;
		if ((uint)(communityType - 1) <= 1u)
		{
			return true;
		}
		return false;
	}

	public HunterIconType GetHunterIconType()
	{
		return CommunityType switch
		{
			CommunityType.HunterZombie => HunterIconType.HunterZombies, 
			CommunityType.HunterLooter => HunterIconType.HunterLooters, 
			CommunityType.RovingTrader => HunterIconType.RovingTrader, 
			CommunityType.RovingRefugee => HunterIconType.RovingRefugee, 
			_ => HunterIconType.Invalid, 
		};
	}

	public Character GetNextLivingMember(Character character, bool followersOnly)
	{
		CommunityPage.BuildSortedListOfCommunityMembers(this);
		List<CommunityPage.CharacterScore> temp = CommunityPage.Temp;
		if (temp.Count == 1)
		{
			return temp[0].Character;
		}
		int num = 0;
		for (int i = 0; i < temp.Count; i++)
		{
			if (temp[i].Character == character)
			{
				num = i;
				break;
			}
		}
		for (int num2 = (num + 1) % temp.Count; num2 != num; num2 = (num2 + 1) % temp.Count)
		{
			if ((!followersOnly || temp[num2].Character.IsInSameSquad(character)) && temp[num2].Character.AliveAndNotZombie)
			{
				character = temp[num2].Character;
				break;
			}
		}
		CommunityPage.Temp.Clear();
		return character;
	}

	public Character GetPrevLivingMember(Character character, bool followersOnly)
	{
		CommunityPage.BuildSortedListOfCommunityMembers(this);
		List<CommunityPage.CharacterScore> temp = CommunityPage.Temp;
		if (temp.Count == 1)
		{
			return temp[0].Character;
		}
		int num = 0;
		for (int i = 0; i < temp.Count; i++)
		{
			if (temp[i].Character == character)
			{
				num = i;
				break;
			}
		}
		for (int num2 = (num + temp.Count - 1) % temp.Count; num2 != num; num2 = (num2 + temp.Count - 1) % temp.Count)
		{
			if ((!followersOnly || temp[num2].Character.IsInSameSquad(character)) && temp[num2].Character.AliveAndNotZombie)
			{
				character = temp[num2].Character;
				break;
			}
		}
		CommunityPage.Temp.Clear();
		return character;
	}

	public bool IsAnyMemberMovingToEnterBuilding(Building building)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].FindActiveGoal(GoalType.MoveToAndEnterBuilding) is MoveToAndEnterBuilding moveToAndEnterBuilding && moveToAndEnterBuilding.GetTargetBuilding() == building)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberRepairingBuilding(TileObject prop)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie)
			{
				RepairGoal repairGoal = Members[i].GetRepairGoal();
				if (repairGoal != null && repairGoal.CurrentBuildingToRepair == prop)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyMemberLightingFire(Campfire campfire)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].FindActiveGoal(GoalType.LightFireGoal) is LightFireGoal lightFireGoal && lightFireGoal.GetTargetObject() == campfire)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberHealing(Character character)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (!Members[i].AliveAndNotZombie || Members[i] == character)
			{
				continue;
			}
			if (Members[i].FindActiveGoal(GoalType.MedicGoal) is MedicGoal medicGoal && medicGoal.GetTargetObject() == character)
			{
				return true;
			}
			if (Members[i].FindActiveGoal(GoalType.MoveToAndInteractGoal) is MoveToAndInteractGoal moveToAndInteractGoal && moveToAndInteractGoal.GetTargetObject() == character)
			{
				InteractionType interactionType = moveToAndInteractGoal.GetInteractionType();
				if (interactionType == InteractionType.ApplyBandage || interactionType == InteractionType.ApplyBandageAndSpeak)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyMemberAddingMaterialToFire(Campfire campfire)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].FindActiveGoal(GoalType.MoveToAndAddMaterialToFire) is MoveToAndAddMaterialToFire moveToAndAddMaterialToFire && moveToAndAddMaterialToFire.Tile == campfire.Tile)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberFindingFrom(FindType findType, TileObject obj, TileObject ignore = null)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i] != ignore && Members[i].FindActiveGoal(GoalType.FindGoal) is FindGoal findGoal && findGoal.FindType == findType && findGoal.FoundItem != null && findGoal.FoundItem.InventoryOwner == obj)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreTooManyMembersFindingFrom(EquipmentPrototype proto, TileObject obj, TileObject ignore = null)
	{
		int num = obj.GetInventory().CountItemsOfType(proto);
		float num2 = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member != ignore && member.FindActiveGoal(GoalType.FindGoal) is FindGoal findGoal && findGoal.IsLookingInObject(obj) && findGoal.IsLookingForType(member, proto, out var amountNeeded))
			{
				num2 += amountNeeded;
				if (num2 >= (float)num)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyMemberRepairingArmorAtBench(WorkBench workBench)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].FindActiveGoal(GoalType.MoveToBenchAndRepairArmor) is MoveToBenchAndRepairArmor moveToBenchAndRepairArmor && moveToBenchAndRepairArmor.GetTargetObject() == workBench)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberCookingWithFire(Campfire campfire)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie)
			{
				if (Members[i].HasRoleWithTargetLocation(Role.Cook, campfire.Tile))
				{
					return true;
				}
				if (Members[i].HasRoleWithTargetLocation(Role.Crafter, campfire.Tile))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyMemberCraftingWithProp(CraftingProp craftingProp, Character ignore)
	{
		TerrainCoord centreTile = craftingProp.GetCentreTile();
		foreach (Character member in Members)
		{
			if (member != ignore && member.GetBaseObjectType() == BaseObjectType.Human && member.AliveAndNotZombie && member.FindActiveGoal(GoalType.CraftGoal) is CraftGoal craftGoal && craftGoal.DestTile == centreTile)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberResettingTrap(ITrap trap)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].FindActiveGoal(GoalType.MoveToAndResetTrap) is MoveToAndResetTrap moveToAndResetTrap && moveToAndResetTrap.GetTargetObject() == trap)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberCollectingSomethingFrom(TileObject obj, Character ignore)
	{
		foreach (Character member in Members)
		{
			if (member != ignore && member.AliveAndNotZombie && member.IsCollectingSomethingFrom(obj))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberPaused()
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.HasAnyPausedRoles())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberPausedAndNotTakingABreak()
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.DownTime == 0f && member.HasAnyPausedRoles())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberInCombat()
	{
		foreach (Character member in Members)
		{
			if (member.InCombat && member.AliveAndNotZombie && member.IsOutdoors() && !member.IsFleeing())
			{
				if (!(member.FindActiveGoal(GoalType.Attack) is Attack { Target: not null } attack) || !attack.Target.GetFlag(TargetFlags.Inaccessible))
				{
					return true;
				}
			}
			else if (member.DirectControlled && member.UnderAttackRefCount > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberRagdolled()
	{
		foreach (Character member in Members)
		{
			if (member.IsRagdoll())
			{
				return true;
			}
		}
		return false;
	}

	public bool DoesAnyOtherMemberWantToUseGuardPostInsteadOfMe(Character character, Character targetCharacter)
	{
		foreach (Character member in Members)
		{
			if (character.AliveAndNotZombie && character.GetBaseObjectType() == BaseObjectType.Human && !character.IsGuarding() && character.Inventory.GetBestWeapon(character, targetCharacter, wantRanged: true, bluntOnly: false) != null && (character.PosXZ - member.PosXZ).magnitude <= 128f)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCharacterVisibleToAnyMember(Character character, out Character closestMember, bool notIncludingCaptives = false)
	{
		closestMember = null;
		float num = float.MaxValue;
		foreach (Character member in Members)
		{
			if (!member.AliveAndNotZombie || !member.IsAwake || member.GetBaseObjectType() != BaseObjectType.Human || (notIncludingCaptives && member.GetRank() == Rank.Captive))
			{
				continue;
			}
			if (character == member)
			{
				return true;
			}
			Target target = member.GetTarget(character);
			if (target != null && target.FullyTracked)
			{
				float sqrMagnitude = (member.PosXZ - character.PosXZ).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					closestMember = member;
					num = sqrMagnitude;
				}
			}
		}
		return closestMember != null;
	}

	public bool AreAnyMembersInMainView()
	{
		foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
		{
			if (playerRecord.PlayerMode != PlayerMode.Dormant && playerRecord.PlayerMode != PlayerMode.CreatingCharacter)
			{
				TerrainCoord tileCoordForPosXZ = GameTerrain.Instance.GetTileCoordForPosXZ(playerRecord.SyncedCamFocusPosXZ);
				if (Math.Min(GetDistSqToNearestMember(tileCoordForPosXZ), GetDistSqToNearestBuilding(tileCoordForPosXZ)) < MathUtil.Squared(40f))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AreAnyMembersInCombat()
	{
		bool flag = IsZombieCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if ((Members[i].UnderAttackRefCount > 0 || Members[i].InCombat) && Members[i].Zombie == flag)
			{
				return true;
			}
		}
		return false;
	}

	public bool AreAllLivingNonZombieMembersInsideBuilding(Building building)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && !Members[i].Disappeared && Members[i].InsideBuilding != building)
			{
				return false;
			}
		}
		return true;
	}

	public TimeSpan GetTimeWhenLastMemberDied()
	{
		TimeSpan result = TimeSpan.Zero;
		for (int i = 0; i < Members.Count; i++)
		{
			result = TimeSpan.FromTicks(Math.Max(result.Ticks, Members[i].TimeOfDeath.Ticks));
		}
		return result;
	}

	public int GetNumTallStructures()
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null)
			{
				if (building.IsTall())
				{
					num++;
				}
				if ((building.GetBaseObjectType() == BaseObjectType.Campfire || building.GetBaseObjectType() == BaseObjectType.Kiln) && building.IsBurning())
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool CanUseRoleCommands()
	{
		if (!Session.Instance.FollowerCommandsEnabled)
		{
			return false;
		}
		if (Members.Count <= 1 && GetAccommodation() <= 0)
		{
			return !GameImpl.Instance.Settings.HintsEnabled;
		}
		return true;
	}

	public override bool IsPlural()
	{
		return GetLivingNonZombieMemberCount() > 1;
	}

	public override bool IsMany()
	{
		return GetLivingNonZombieMemberCount() >= 5;
	}

	public override bool IsZero()
	{
		return GetLivingNonZombieMemberCount() == 0;
	}

	public override GenderType GetGender(Language language = Language.Count)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = IsZombieCommunity();
		bool flag5 = IsAnimalCommunity();
		foreach (Character member in Members)
		{
			if ((flag5 || member.GetBaseObjectType() == BaseObjectType.Human) && !member.Disappeared)
			{
				bool flag6 = (flag4 ? member.Alive : member.AliveAndNotZombie);
				bool flag7 = member.GetGender(language) == GenderType.Male;
				flag = flag || flag6;
				if (flag6)
				{
					flag2 = flag2 || flag7;
				}
				else
				{
					flag3 = flag3 || flag7;
				}
			}
		}
		if (!flag)
		{
			if (!flag3)
			{
				return GenderType.Female;
			}
			return GenderType.Male;
		}
		if (!flag2)
		{
			return GenderType.Female;
		}
		return GenderType.Male;
	}

	public bool HasAnyLivingNonZombieMembers()
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.Disappeared)
			{
				return true;
			}
		}
		return false;
	}

	public int GetLivingNonZombieMemberCount()
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].GetBaseObjectType() == BaseObjectType.Human && !Members[i].Disappeared)
			{
				num++;
			}
		}
		return num;
	}

	public int GetConsciousNonZombieMemberCount()
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].ConsciousAndNotZombie && Members[i].GetBaseObjectType() == BaseObjectType.Human && !Members[i].Disappeared)
			{
				num++;
			}
		}
		return num;
	}

	public int GetLivingNonZombieMemberCountIncludingAllies()
	{
		int num = GetLivingNonZombieMemberCount();
		foreach (Community cachedAlly in CachedAllies)
		{
			num += cachedAlly.GetLivingNonZombieMemberCount();
		}
		return num;
	}

	public int GetActiveAllyCount()
	{
		int num = 0;
		foreach (Community cachedAlly in CachedAllies)
		{
			if (cachedAlly.HasAnyActiveMembers())
			{
				num++;
			}
		}
		return num;
	}

	public bool HasAnyLivingNonZombieMembersOfSpecies(BaseObjectType species)
	{
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].GetBaseObjectType() == species && !Members[i].Disappeared)
			{
				return true;
			}
		}
		return false;
	}

	public int GetLivingNonZombieMemberCountBySpecies(BaseObjectType species)
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].GetBaseObjectType() == species && !Members[i].Disappeared)
			{
				num++;
			}
		}
		return num;
	}

	public int GetChickenCountIncludingEggs()
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i] is Chicken chicken)
			{
				num++;
				if (chicken.IsFertilized())
				{
					num++;
				}
			}
		}
		return num + CountInventoryItemsOfType(EquipmentPrototype.FertilizedEgg);
	}

	public int GetLivingNonZombieMemberCountBySpeciesGender(BaseObjectType species, GenderType gender)
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].AliveAndNotZombie && Members[i].GetBaseObjectType() == species && Members[i].GetGender() == gender && !Members[i].Disappeared)
			{
				num++;
			}
		}
		return num;
	}

	public int GetLivingNonZombieMemberCountNotIncludingKidnappedMembers()
	{
		return GetLivingNonZombieMemberCount();
	}

	public int GetDeadUnburiedMemberCount()
	{
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if (!Members[i].AliveAndNotZombie && Members[i].GetBaseObjectType() == BaseObjectType.Human && !Members[i].Disappeared)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsLoneWolfCommunity()
	{
		foreach (Character member in Members)
		{
			if (member.GetBaseObjectType() == BaseObjectType.Human && (!member.AliveAndNotZombie || !member.IsPlayerAvatar()))
			{
				return false;
			}
		}
		return true;
	}

	public bool HasAnyActiveMembers()
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if ((flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && (flag ? Members[i].Alive : Members[i].AliveAndNotZombie))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyActiveMembersWhoWereInitiallyMembersOf(Community community)
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if (Members[i].InitialCommunity == community && (flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && (flag ? Members[i].Alive : Members[i].AliveAndNotZombie))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyConsciousMembers()
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if ((flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && (flag ? Members[i].IsConscious : Members[i].ConsciousAndNotZombie))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyConsciousMembersInRect(TerrainRect rect)
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if ((flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && rect.Contains(Members[i].Tile) && (flag ? Members[i].IsConscious : Members[i].ConsciousAndNotZombie))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasConsciousMembers(int desiredCount)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.ConsciousAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.Disappeared)
			{
				num++;
				if (num >= desiredCount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyoneConscious(bool includeDrunk)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.Disappeared)
			{
				if (member.Consciousness < Consciousness.Unconscious)
				{
					return true;
				}
				if (!includeDrunk && (member.BloodAlcoholConcentration >= Character.BACStupor || member.SedativeEffect > 0f) && member.BloodLoss < 1f && member.BodyTemperatureInCelsius > Character.BodyTemperatureInCelsiusWakeUp)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyoneIntroducing(Community community)
	{
		foreach (Character member in Members)
		{
			if (member.FindActiveGoal(GoalType.Conversation) is Conversation { OpeningSpeech: not null } conversation && (conversation.OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.Introduction || conversation.OpeningSpeech.SpecialBehaviour == SpecialSpeechBehaviour.IntroductionShakedown))
			{
				Character targetCharacter = conversation.GetTargetCharacter();
				if (targetCharacter != null && targetCharacter.Community == community)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyoneHealable()
	{
		foreach (Character member in Members)
		{
			if (member.GetBaseObjectType() == BaseObjectType.Human && member.IsHealable())
			{
				return true;
			}
		}
		return false;
	}

	public int GetActiveMemberCount()
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		int num = 0;
		for (int i = 0; i < Members.Count; i++)
		{
			if ((flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && (flag ? Members[i].Alive : Members[i].AliveAndNotZombie))
			{
				num++;
			}
		}
		return num;
	}

	public Character GetFirstActiveMember()
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		for (int i = 0; i < Members.Count; i++)
		{
			if ((flag2 || Members[i].GetBaseObjectType() == BaseObjectType.Human) && !Members[i].Disappeared && (flag ? Members[i].Alive : Members[i].AliveAndNotZombie))
			{
				return Members[i];
			}
		}
		return null;
	}

	public bool HasRecentlyDeadMembers(int howMany, TimeSpan after)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (!member.AliveAndNotZombie && member.TimeOfDeath >= after)
			{
				num++;
				if (num >= howMany)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsCloseToBase(TerrainCoord tile, float maxDist)
	{
		float num = maxDist * maxDist;
		if (BaseRect != TerrainRect.Invalid)
		{
			return BaseRect.GetClosestDistSqTo(tile) <= num;
		}
		foreach (Prop building in Buildings)
		{
			if (!(building is ITrap) && !(building is Campfire) && building.GetBaseObjectType() != BaseObjectType.Grave && (!(building is EnterableVehicle enterableVehicle) || (!enterableVehicle.IsDriveable && !enterableVehicle.HasEverMoved)) && tile.GetDistSquared(building.Tile) <= num)
			{
				return true;
			}
		}
		return false;
	}

	public float GetDistSqToNearestBuilding(TerrainCoord tile)
	{
		float num = float.MaxValue;
		foreach (Prop building in Buildings)
		{
			if (!(building is ITrap) && !(building is Campfire) && building.GetBaseObjectType() != BaseObjectType.Grave && !building.IsDriveableVehicle())
			{
				num = Math.Min(num, tile.GetDistSquared(building.Tile));
			}
		}
		return num;
	}

	public float GetDistSqToNearestMember(TerrainCoord tile)
	{
		float num = float.MaxValue;
		foreach (Character member in Members)
		{
			num = Math.Min(num, tile.GetDistSquared(member.Tile));
		}
		return num;
	}

	public float GetDistSqToNearestLivingNonZombieMember(TerrainCoord tile, Character ignore, bool ignoreThoseIndoors)
	{
		float num = float.MaxValue;
		foreach (Character member in Members)
		{
			if (ignore != member && member.AliveAndNotZombie && member.IsAwake && member.GetBaseObjectType() == BaseObjectType.Human && (!ignoreThoseIndoors || member.IsOutdoors()))
			{
				num = Math.Min(num, tile.GetDistSquared(member.Tile));
			}
		}
		return num;
	}

	public List<TileObject> GetPlayerObjectsInBaseRect()
	{
		Community playerCommunity = Session.Instance.CommunityManager.PlayerCommunity;
		TempPlayerObjectsInArea.Clear();
		foreach (Prop building in playerCommunity.Buildings)
		{
			if (BaseRect.Contains(building.GetTileRect()))
			{
				TempPlayerObjectsInArea.Add(building);
			}
		}
		return TempPlayerObjectsInArea;
	}

	public bool IsTileInsidePerimeter(TerrainCoord tile)
	{
		return IsTileInsidePerimeter(tile, excludeWallAndGateTiles: false);
	}

	public bool IsTileInsidePerimeter(TerrainCoord tile, bool excludeWallAndGateTiles)
	{
		if (BaseRect.VerticesArea > 0)
		{
			if (!BaseRect.Expand(excludeWallAndGateTiles ? (-1) : 0).Contains(tile))
			{
				return false;
			}
			if (Perimeter != null && Perimeter.Count > 0)
			{
				if (!MathUtil.IsTileInPolygon(tile, Perimeter))
				{
					return false;
				}
				if (excludeWallAndGateTiles && MathUtil.IsTileOnPerimeterEdge(tile, Perimeter))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public TerrainCoord PickRandomTileInsidePerimeter(CustomRandom rand)
	{
		if (Perimeter != null && Perimeter.Count > 0)
		{
			for (int i = 0; i < 100; i++)
			{
				TerrainCoord terrainCoord = rand.RandomTile(BaseRect.min, BaseRect.max);
				if (MathUtil.IsTileInPolygon(terrainCoord, Perimeter))
				{
					return terrainCoord;
				}
			}
		}
		if (BaseRect.VerticesArea > 0)
		{
			return rand.RandomTile(BaseRect.min + new TerrainCoord(1, 1), BaseRect.max - new TerrainCoord(1, 1));
		}
		GetBaseCentre(out var centre, out var radius);
		int num = Mathf.FloorToInt(radius);
		return centre + rand.RandomTile(new TerrainCoord(-num, -num), new TerrainCoord(num, num));
	}

	public bool IsTargetingAnyoneFromCommunity(Community community)
	{
		foreach (Character member in Members)
		{
			foreach (Target target in member.Targets)
			{
				if (target.Object.GetCommunityId() == community.Id)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsWithinRangeOfAnyLivingNonZombieMember(TerrainCoord tile, float range)
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		foreach (Character member in Members)
		{
			if ((flag2 || member.GetBaseObjectType() == BaseObjectType.Human) && (flag ? member.Alive : member.AliveAndNotZombie) && tile.GetDistSquared(member.Tile) < range * range)
			{
				return true;
			}
		}
		return false;
	}

	public int GetConsciousNonZombieMemberCountInRange(TerrainCoord tile, float range, TileObject ignore)
	{
		bool flag = IsZombieCommunity();
		bool flag2 = IsAnimalCommunity();
		int num = 0;
		foreach (Character member in Members)
		{
			if ((flag2 || member.GetBaseObjectType() == BaseObjectType.Human) && (flag ? member.Alive : member.AliveAndNotZombie) && member != ignore && member.Consciousness != Consciousness.Unconscious && tile.GetDistSquared(member.Tile) < range * range && !member.IsFleeing())
			{
				num++;
			}
		}
		return num;
	}

	public Prop GetNearestBuildingOfType(TerrainCoord tile, Type type, float closestDistSq, Character checkMovementZone = null, Character checkMovementZone2 = null)
	{
		Prop result = null;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null && building.GetType().IsA(type))
			{
				float distSquared = tile.GetDistSquared(building.Tile);
				if (distSquared < closestDistSq && (checkMovementZone == null || !checkMovementZone.HasMovementZone() || checkMovementZone.MovementZone.Overlaps(building.GetTileRect())) && (checkMovementZone2 == null || !checkMovementZone2.HasMovementZone() || checkMovementZone2.MovementZone.Overlaps(building.GetTileRect())))
				{
					closestDistSq = distSquared;
					result = building;
				}
			}
		}
		return result;
	}

	public Building GetNearestGuardPost(TerrainCoord tile, float closestDistSq)
	{
		Prop prop = null;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null && building.IsGuardPost())
			{
				float distSquared = tile.GetDistSquared(building.Tile);
				if (distSquared < closestDistSq)
				{
					closestDistSq = distSquared;
					prop = building;
				}
			}
		}
		return prop as Building;
	}

	public Character GetCommunityMemberMostLikedBy(Character sub, bool ignorePlayers)
	{
		float num = float.MinValue;
		Character result = null;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member != sub && (!ignorePlayers || !member.IsPlayerAvatar()))
			{
				sub.CalcApprovalRating(member, out var approval, out var _);
				float num2 = approval;
				if (num2 > num)
				{
					num = num2;
					result = member;
				}
			}
		}
		return result;
	}

	public bool HasAnyBuildingOfType(BaseObjectType baseObjectType)
	{
		foreach (Prop building in Buildings)
		{
			if (building.GetBaseObjectType() == baseObjectType)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumUnderConstructionBuildingsOfType(PropPrototype proto)
	{
		int num = 0;
		foreach (TileObject underConstructionBuilding in UnderConstructionBuildings)
		{
			if (underConstructionBuilding.GetPropPrototype() == proto)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumWorkersOnBuilding(TileObject obj, Character ignore)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member != ignore)
			{
				BuildGoal buildGoal = member.GetBuildGoal();
				if (buildGoal != null && buildGoal.CurrentBuilding == obj)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool GetBaseCentre(out TerrainCoord centre, out float radius)
	{
		centre = new TerrainCoord(0, 0);
		TerrainCoord a = new TerrainCoord(int.MaxValue, int.MaxValue);
		TerrainCoord terrainCoord = new TerrainCoord(-2147483647, -2147483647);
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (!(building is ITrap) && building.GetBaseObjectType() != BaseObjectType.Grave)
			{
				a = TerrainCoord.Min(a, building.GetMinTile());
				terrainCoord = TerrainCoord.Max(terrainCoord, building.GetMaxTile());
				centre += building.Tile;
				num++;
			}
		}
		foreach (TileObject underConstructionBuilding in UnderConstructionBuildings)
		{
			if (!(underConstructionBuilding is ITrap) && underConstructionBuilding.GetBaseObjectType() != BaseObjectType.Grave)
			{
				a = TerrainCoord.Min(a, underConstructionBuilding.GetMinTile());
				terrainCoord = TerrainCoord.Max(terrainCoord, underConstructionBuilding.GetMaxTile());
				centre += underConstructionBuilding.GetCentreTile();
				num++;
			}
		}
		if (num == 0)
		{
			for (int i = 0; i < ConstructionRecords.Count; i++)
			{
				a = TerrainCoord.Min(a, ConstructionRecords[i].Tile);
				terrainCoord = TerrainCoord.Max(terrainCoord, ConstructionRecords[i].Tile);
				centre += ConstructionRecords[i].Tile;
				num++;
			}
		}
		if (num == 0)
		{
			foreach (Character member in Members)
			{
				TerrainCoord tile = member.Tile;
				a = TerrainCoord.Min(a, tile);
				terrainCoord = TerrainCoord.Max(terrainCoord, tile);
				centre += tile;
				num++;
			}
		}
		if (num > 0)
		{
			centre /= num;
			radius = (a.GetDist(terrainCoord) + 1f) * 0.5f;
		}
		else
		{
			centre = TerrainCoord.Invalid;
			radius = 0f;
		}
		return num > 0;
	}

	public bool GetBaseRect(out TerrainRect rect)
	{
		TerrainCoord terrainCoord = new TerrainCoord(int.MaxValue, int.MaxValue);
		TerrainCoord terrainCoord2 = new TerrainCoord(-2147483647, -2147483647);
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (!(building is ITrap) && building.GetBaseObjectType() != BaseObjectType.Grave)
			{
				terrainCoord = TerrainCoord.Min(terrainCoord, building.GetMinTile());
				terrainCoord2 = TerrainCoord.Max(terrainCoord2, building.GetMaxTile());
				num++;
			}
		}
		foreach (TileObject underConstructionBuilding in UnderConstructionBuildings)
		{
			if (!(underConstructionBuilding is ITrap) && underConstructionBuilding.GetBaseObjectType() != BaseObjectType.Grave)
			{
				terrainCoord = TerrainCoord.Min(terrainCoord, underConstructionBuilding.GetMinTile());
				terrainCoord2 = TerrainCoord.Max(terrainCoord2, underConstructionBuilding.GetMaxTile());
				num++;
			}
		}
		if (num == 0)
		{
			for (int i = 0; i < ConstructionRecords.Count; i++)
			{
				terrainCoord = TerrainCoord.Min(terrainCoord, ConstructionRecords[i].Tile);
				terrainCoord2 = TerrainCoord.Max(terrainCoord2, ConstructionRecords[i].Tile);
				num++;
			}
		}
		rect = new TerrainRect(terrainCoord, terrainCoord2);
		return num > 0;
	}

	public Character GetNearestLivingNonZombieMember(TerrainCoord tile, Character ignore, BaseObjectType species, float closestDistSq)
	{
		Character result = null;
		foreach (Character member in Members)
		{
			if (ignore != member && member.AliveAndNotZombie && !member.Disappeared && (member.GetBaseObjectType() == species || species == BaseObjectType.Invalid))
			{
				float distSquared = tile.GetDistSquared(member.Tile);
				if (distSquared < closestDistSq)
				{
					result = member;
					closestDistSq = distSquared;
				}
			}
		}
		return result;
	}

	public Character GetNearestLivingMember(TerrainCoord tile, Character ignore, BaseObjectType species, float closestDistSq)
	{
		Character result = null;
		foreach (Character member in Members)
		{
			if (ignore != member && member.Alive && !member.Disappeared && (member.GetBaseObjectType() == species || species == BaseObjectType.Invalid))
			{
				float distSquared = tile.GetDistSquared(member.Tile);
				if (distSquared < closestDistSq)
				{
					result = member;
					closestDistSq = distSquared;
				}
			}
		}
		return result;
	}

	public Character GetNearestAwakeNonZombieMember(TerrainCoord tile, Character ignore, BaseObjectType species, float closestDistSq)
	{
		Character result = null;
		foreach (Character member in Members)
		{
			if (ignore != member && member.AwakeAndNotZombie && !member.Disappeared && (member.GetBaseObjectType() == species || species == BaseObjectType.Invalid))
			{
				float distSquared = tile.GetDistSquared(member.Tile);
				if (distSquared < closestDistSq)
				{
					result = member;
					closestDistSq = distSquared;
				}
			}
		}
		return result;
	}

	public TileObject GetNearestMemberOrBuildingWithInventorySpaceFor(TerrainCoord tile, TileObject ignore, BaseObjectType species, float closestDistSq, float weight)
	{
		TileObject result = null;
		foreach (Character member in Members)
		{
			if (ignore != member && member.AliveAndNotZombie && !member.Disappeared && (member.GetBaseObjectType() == species || species == BaseObjectType.Invalid) && member.HasInventorySpaceFor(weight))
			{
				float distSquared = tile.GetDistSquared(member.Tile);
				if (distSquared < closestDistSq)
				{
					result = member;
					closestDistSq = distSquared;
				}
			}
		}
		foreach (Prop building in Buildings)
		{
			if (ignore != building && building.HasInventorySpaceFor(weight))
			{
				float distSquared2 = tile.GetDistSquared(building.Tile);
				if (distSquared2 < closestDistSq)
				{
					result = building;
					closestDistSq = distSquared2;
				}
			}
		}
		return result;
	}

	public Prop GetNearestAccomodationBuildingWithInventorySpaceFor(TerrainCoord tile, float closestDistSq, float weight)
	{
		Prop result = null;
		foreach (Prop building in Buildings)
		{
			if (building.IsAccommodation() && building.HasInventorySpaceFor(weight))
			{
				float distSquared = tile.GetDistSquared(building.Tile);
				if (distSquared < closestDistSq)
				{
					result = building;
					closestDistSq = distSquared;
				}
			}
		}
		return result;
	}

	public Character GetNearestMemberWhoCanNarrate(TerrainCoord tile, float closestDistSq)
	{
		Character result = null;
		foreach (Character member in Members)
		{
			if (member.CanNarrate())
			{
				float distSquared = tile.GetDistSquared(member.Tile);
				if (distSquared < closestDistSq)
				{
					result = member;
					closestDistSq = distSquared;
				}
			}
		}
		return result;
	}

	public bool CanPlayerSurrenderToMe(Character player, Character member)
	{
		if (IsZombieCommunity() || IsAnimalCommunity())
		{
			return false;
		}
		if (PlayerSurrenderDisabled)
		{
			return false;
		}
		if (Session.Instance.PlayTime - LastRefusedPlayerSurrender < Character.MinTimeBetweenPlayerSurrender)
		{
			return false;
		}
		if (!player.IsEnemy(member))
		{
			return false;
		}
		if (member.IsSurrendering())
		{
			return false;
		}
		if (member.Rank == Rank.Captive)
		{
			return false;
		}
		return true;
	}

	public Threat FindThreatById(int id)
	{
		for (int i = 0; i < Threats.Count; i++)
		{
			if (Threats[i].Id == id)
			{
				return Threats[i];
			}
		}
		return null;
	}

	public Threat FindThreatByCharacter(Character threatCharacter)
	{
		for (int i = 0; i < Threats.Count; i++)
		{
			if (Threats[i].ThreatMembers.Contains(threatCharacter))
			{
				return Threats[i];
			}
		}
		return null;
	}

	public Threat FindThreatByLocation(TerrainCoord tile, float maxDist)
	{
		Threat result = null;
		float num = maxDist * maxDist;
		for (int i = 0; i < Threats.Count; i++)
		{
			float closestDistSqTo = new TerrainRect(Threats[i].BoundsMin, Threats[i].BoundsMax).GetClosestDistSqTo(tile);
			if (closestDistSqTo < num)
			{
				result = Threats[i];
				num = closestDistSqTo;
			}
		}
		return result;
	}

	public bool CanBeThreat(Character threatCharacter)
	{
		if (threatCharacter.Consciousness >= Consciousness.Unconscious)
		{
			return false;
		}
		if (threatCharacter.GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (threatCharacter.IsSurrendering() && !IsZombieCommunity())
		{
			return false;
		}
		return true;
	}

	public void OnEncounteredThreat(Character member, Character threatCharacter, Target target)
	{
		CommunityType communityType = CommunityType;
		if (communityType == CommunityType.Player || communityType == CommunityType.AmbientZombie || communityType == CommunityType.AmbientAnimal || !CanBeThreat(threatCharacter) || Attack.ShouldForgetInaccessibleTarget(member, target))
		{
			return;
		}
		TerrainCoord tile = threatCharacter.Tile;
		Threat threat = FindThreatByCharacter(threatCharacter);
		if (threat == null)
		{
			if ((member.IsGuarding() && !threatCharacter.DirectControlled && !threatCharacter.InCombat) || member.CanFollowPlayer)
			{
				return;
			}
			foreach (Threat threat2 in Threats)
			{
				if (tile.IsWithinBounds(threat2.BoundsMin, threat2.BoundsMax))
				{
					threat = threat2;
					break;
				}
			}
			if (threat == null)
			{
				threat = new Threat(threatCharacter);
				Threats.Add(threat);
			}
			else
			{
				threat.ThreatMembers.Add(threatCharacter);
			}
		}
		if (CommunityType == CommunityType.Player)
		{
			foreach (Character member2 in Members)
			{
				Target orCreateTarget = member2.GetOrCreateTarget(threatCharacter);
				if (orCreateTarget != null)
				{
					target.ShareInfoWith(orCreateTarget);
				}
			}
		}
		else
		{
			foreach (Squad squad in Squads)
			{
				if (squad.ThreatId != threat.Id)
				{
					continue;
				}
				foreach (Character member3 in squad.Members)
				{
					if (member3 != member)
					{
						Target orCreateTarget2 = member3.GetOrCreateTarget(threatCharacter, target.LastKnownPosition);
						if (orCreateTarget2 != null)
						{
							target.ShareInfoWith(orCreateTarget2);
						}
					}
				}
			}
		}
		threat.LastEncounteredTime = Session.Instance.PlayTime;
		threat.BoundsMin = TerrainCoord.Min(threat.BoundsMin, tile - new TerrainCoord(ThreatOverlap, ThreatOverlap));
		threat.BoundsMax = TerrainCoord.Max(threat.BoundsMax, tile + new TerrainCoord(ThreatOverlap, ThreatOverlap));
	}

	public int AddThreat(Character threatCharacter)
	{
		TerrainCoord tile = threatCharacter.Tile;
		Threat threat = FindThreatByCharacter(threatCharacter);
		if (threat == null)
		{
			foreach (Threat threat2 in Threats)
			{
				if (tile.IsWithinBounds(threat2.BoundsMin, threat2.BoundsMax))
				{
					threat = threat2;
					break;
				}
			}
			if (threat == null)
			{
				threat = new Threat(threatCharacter);
				Threats.Add(threat);
			}
			else
			{
				threat.ThreatMembers.Add(threatCharacter);
			}
		}
		return threat.Id;
	}

	private Squad FindSquadById(int id)
	{
		foreach (Squad squad in Squads)
		{
			if (squad.Id == id)
			{
				return squad;
			}
		}
		return null;
	}

	private Squad FindSquadDefendingAgainstThreat(Threat threat)
	{
		foreach (Squad squad in Squads)
		{
			if (squad.Behaviour == SquadBehaviour.Defend && squad.Action == SquadAction.AttackThreat && squad.ThreatId == threat.Id)
			{
				return squad;
			}
		}
		return null;
	}

	private Squad FindHunterSquad()
	{
		foreach (Squad squad in Squads)
		{
			if (squad.Behaviour == SquadBehaviour.Hunt)
			{
				return squad;
			}
		}
		return null;
	}

	private void OnStopSquadAction(Squad squad, Character member)
	{
		if (squad.Action == SquadAction.Wait && member == squad.GetLeader())
		{
			if (IsAISettlement())
			{
				member.TryToEnsureHangoutLocationIsSet();
			}
			else
			{
				member.SetHangoutLocation(TerrainCoord.Invalid);
			}
		}
		member.ClearLeaderCommand();
	}

	public void RemoveFromSquad(Character member)
	{
		if (member.SquadId == 0)
		{
			return;
		}
		Squad squad = FindSquadById(member.SquadId);
		if (squad == null)
		{
			return;
		}
		if (squad.Members.Count == 0)
		{
			string text = member.GetDisplayNameString() + " removed from squad " + member.SquadId + " with 0 members, all squads: ";
			foreach (Squad squad2 in Squads)
			{
				text = text + "(" + squad2.Id + ": " + squad2.Members.Count + " members) ";
			}
			Debug.LogError(text);
		}
		Character leader = squad.GetLeader();
		OnStopSquadAction(squad, member);
		member.SquadId = 0;
		squad.Members.Remove(member);
		Character character = ((squad.Members.Count > 0) ? squad.GetLeader() : null);
		if (member == leader)
		{
			if (character != null)
			{
				character.Follow(null);
				for (int i = 1; i < squad.Members.Count; i++)
				{
					squad.Members[i].Follow(character);
				}
				AssignSquadActionToLeader(squad);
			}
		}
		else
		{
			member.Follow(null);
		}
		if (squad.Members.Count == 0)
		{
			RemoveSquad(squad);
		}
	}

	public void AddToSquad(Character member, Squad squad)
	{
		if (!(IsZombieCommunity() ? member.Alive : member.AliveAndNotZombie))
		{
			Debug.LogError("Non-living member added to " + squad.Behaviour.ToString() + " squad: " + member.GetDisplayNameString());
			return;
		}
		RemoveFromSquad(member);
		member.SquadId = squad.Id;
		squad.Members.Add(member);
		squad.InitialSquadMemberCount++;
		if (squad.Members.Count > 1)
		{
			member.Follow(squad.GetLeader());
		}
		else
		{
			AssignSquadActionToLeader(squad);
		}
		member.SanitiseSquad();
	}

	public void AssignSquadActionToLeader(Squad squad)
	{
		if (squad.Members.Count == 0)
		{
			return;
		}
		Character leader = squad.GetLeader();
		switch (squad.Action)
		{
		case SquadAction.AttackThreat:
		{
			Threat threat = FindThreatById(squad.ThreatId);
			if (threat == null)
			{
				break;
			}
			Character character = null;
			float num = float.MaxValue;
			foreach (Character threatMember in threat.ThreatMembers)
			{
				float sqrMagnitude = (threatMember.PosXZ - leader.PosXZ).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					Target target = leader.GetTarget(threatMember);
					if (target != null && target.FullyTracked)
					{
						character = threatMember;
						num = sqrMagnitude;
					}
				}
			}
			if (character != null)
			{
				Goal goal = null;
				if (leader.Zombie)
				{
					goal = new FeedOnLiving(leader, character);
				}
				else
				{
					StayInRangeParams stayInRangeParams = StayInRangeParams.OfSquadLeaderOrBase(leader);
					goal = new Attack(leader, character, dontOpenOurGates: true, stayInRangeParams);
				}
				leader.SetLeaderCommand(goal, character, ObeyLeaderGoal.SourceType.SquadLeader);
			}
			break;
		}
		case SquadAction.Pillage:
		{
			bool flag = squad.HasAnyMolotovCocktails();
			bool flag2 = squad.HasAnyExplosives();
			TimeSpan waitTime = TimeSpan.FromSeconds(Mathf.Lerp(2f, 10f, Session.Instance.DeterministicRand.RandomFloat()));
			MoveAsCloseAsPossibleAndWait moveAsCloseAsPossibleAndWait2 = new MoveAsCloseAsPossibleAndWait(IsHunterCommunity() ? MovementType.Walk : MovementType.Jog, squad.DestTile, waitTime, 64f);
			moveAsCloseAsPossibleAndWait2.IgnoreFlammableDefences = flag || flag2;
			moveAsCloseAsPossibleAndWait2.IgnoreExplodableDefences = flag2;
			moveAsCloseAsPossibleAndWait2.AvoidHostileBases = !GameTerrain.Instance.ComplexPathfinding;
			moveAsCloseAsPossibleAndWait2.DontAvoidCommunityId = squad.EnemyCommunityId;
			leader.SetLeaderCommand(moveAsCloseAsPossibleAndWait2, null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		}
		case SquadAction.GoTo:
		{
			MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo = new MoveAsCloseAsPossibleTo(MovementType.Walk, squad.DestTile);
			moveAsCloseAsPossibleTo.AvoidHostileBases = squad.AvoidHostileBases;
			if (squad.Behaviour != SquadBehaviour.Ambush)
			{
				leader.SetHangoutLocation(TerrainCoord.Invalid);
			}
			leader.SetLeaderCommand(moveAsCloseAsPossibleTo, null, ObeyLeaderGoal.SourceType.SquadLeader_LowPrio);
			break;
		}
		case SquadAction.GoToHighPrio:
		{
			MoveAsCloseAsPossibleTo moveAsCloseAsPossibleTo2 = new MoveAsCloseAsPossibleTo(MovementType.Walk, squad.DestTile);
			moveAsCloseAsPossibleTo2.AvoidHostileBases = squad.AvoidHostileBases;
			leader.SetLeaderCommand(moveAsCloseAsPossibleTo2, null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		}
		case SquadAction.GoToAmbush:
		{
			MoveAsCloseAsPossibleToTarget moveAsCloseAsPossibleToTarget = new MoveAsCloseAsPossibleToTarget(leader, squad.GoalCharacter, (leader.Tile.GetDist(squad.GoalCharacter.Tile) < 64f) ? MovementType.Run : MovementType.Jog);
			moveAsCloseAsPossibleToTarget.AvoidHostileBases = squad.AvoidHostileBases;
			moveAsCloseAsPossibleToTarget.FinishWhenInRange = 8f;
			leader.SetLeaderCommand(moveAsCloseAsPossibleToTarget, null, ObeyLeaderGoal.SourceType.SquadLeader);
			squad.Teleported = false;
			break;
		}
		case SquadAction.SearchForItem:
		{
			FindGoal findGoal = new FindGoal(FindType.Prototype, squad.AmbushForEquipmentType, MovementType.Walk, critical: true);
			findGoal.DesiredAmountOfRecipe = 1000;
			leader.SetLeaderCommand(findGoal, null, ObeyLeaderGoal.SourceType.Squad_HighPrio);
			break;
		}
		case SquadAction.TalkToAmbush:
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(leader, squad.GoalCharacter, SpeechSituation.Ambush);
			if (speechForSituation != null)
			{
				if (GetRelationship(squad.GoalCharacter.Community) == CommunityRelationshipType.Hostile)
				{
					leader.Speak(speechForSituation, squad.GoalCharacter);
					leader.SetLeaderCommand(new Attack(leader, squad.GoalCharacter, dontOpenOurGates: false, default(StayInRangeParams)), null, ObeyLeaderGoal.SourceType.Scripted);
					squad.GoalAchieved = true;
				}
				else
				{
					leader.SetLeaderCommand(new Conversation(leader, squad.GoalCharacter, null, speechForSituation, controlledByPlayer: false), null, ObeyLeaderGoal.SourceType.Scripted);
				}
			}
			else
			{
				squad.GoalAchieved = true;
				squad.ActionFinished = true;
			}
			break;
		}
		case SquadAction.Wait:
			leader.ClearLeaderCommand();
			if (squad.GoalTile != TerrainCoord.Invalid && squad.GoalTile != TerrainCoord.Zero)
			{
				leader.SetHangoutLocation(squad.GoalTile);
			}
			else
			{
				leader.SetHangoutLocation(leader.Tile);
			}
			break;
		case SquadAction.Trade:
			leader.ClearLeaderCommand();
			leader.SetHangoutLocation(squad.GoalTile);
			break;
		case SquadAction.Bury:
			leader.SetLeaderCommand(new BuryGoal(squad.GoalCharacter), null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		case SquadAction.Enter:
		{
			Building building = GameTerrain.Instance.GetBuilding(squad.DestTile.x, squad.DestTile.y);
			if (building != null)
			{
				MoveToAndEnterBuilding moveToAndEnterBuilding = new MoveToAndEnterBuilding(leader, building, MovementType.Walk);
				moveToAndEnterBuilding.StayThere = true;
				leader.SetLeaderCommand(moveToAndEnterBuilding, null, ObeyLeaderGoal.SourceType.SquadLeader);
			}
			else
			{
				squad.ActionFinished = true;
			}
			break;
		}
		case SquadAction.FlyAway:
			leader.SetLeaderCommand(new Idle(), null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		case SquadAction.FindTarget:
			leader.SetLeaderCommand(new MoveAsCloseAsPossibleTo(MovementType.Walk, squad.DestTile), null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		case SquadAction.Retreat:
			if (IsAISettlement())
			{
				if (GetBaseCentre(out var centre, out var _))
				{
					leader.SetLeaderCommand(new MoveAsCloseAsPossibleTo(MovementType.Run, centre), null, ObeyLeaderGoal.SourceType.SquadLeader);
				}
				else
				{
					leader.SetLeaderCommand(new FleeFromAllEnemies(MovementType.Run, 4f, FleeGoal.FleeDist, dontOpenOurGates: false, avoidHostileBases: true), null, ObeyLeaderGoal.SourceType.SquadLeader);
				}
			}
			break;
		case SquadAction.StockUpFood:
		{
			foreach (Character member in squad.Members)
			{
				member.SetLeaderCommand(new FindGoal(FindType.FoodForJourney, MovementType.Walk, critical: false), null, ObeyLeaderGoal.SourceType.Squad_HighPrio);
			}
			break;
		}
		case SquadAction.StockUpWater:
		{
			foreach (Character member2 in squad.Members)
			{
				member2.SetLeaderCommand(new FindGoal((member2.GetThirst() >= Character.ThirstyTime) ? FindType.Drink : FindType.WaterForJourney, MovementType.Walk, critical: false), null, ObeyLeaderGoal.SourceType.Squad_HighPrio);
			}
			break;
		}
		case SquadAction.StopForWarmth:
			leader.SetLeaderCommand(new WarmGoal(), null, ObeyLeaderGoal.SourceType.Squad_HighPrio);
			break;
		case SquadAction.HangAround:
		{
			MoveAsCloseAsPossibleAndWait moveAsCloseAsPossibleAndWait = new MoveAsCloseAsPossibleAndWait(MovementType.Run, squad.DestTile, TimeSpan.FromSeconds(30.0), 8f);
			moveAsCloseAsPossibleAndWait.WaitTimeIfFailed = TimeSpan.FromSeconds(4.0);
			leader.SetLeaderCommand(moveAsCloseAsPossibleAndWait, null, ObeyLeaderGoal.SourceType.SquadLeader);
			break;
		}
		case SquadAction.ExitMap:
			break;
		}
	}

	public void SetSquadAction(Squad squad, SquadAction action, int threatId, TerrainCoord destTile, Character goalCharacter, bool isInvader = false)
	{
		if (squad.Action != SquadAction.None)
		{
			foreach (Character member in squad.Members)
			{
				OnStopSquadAction(squad, member);
			}
		}
		squad.Action = action;
		squad.ActionFinished = false;
		squad.ThreatId = threatId;
		squad.DestTile = destTile;
		squad.GoalCharacter = goalCharacter;
		squad.LastEncounterTime = Session.Instance.PlayTime;
		squad.ActionStartedTime = Session.Instance.PlayTime;
		squad.PillageObjectId = 0;
		AssignSquadActionToLeader(squad);
		if (!isInvader && squad.WantMapIcon())
		{
			Session.Instance.CommunityManager.AddSquadWithMapIcons(squad);
		}
		else
		{
			Session.Instance.CommunityManager.SquadsWithMapIcons.Remove(squad);
		}
	}

	public void RemoveSquad(Squad squad)
	{
		for (int num = squad.Members.Count - 1; num >= 0; num--)
		{
			RemoveFromSquad(squad.Members[num]);
		}
		if (squad.Behaviour == SquadBehaviour.Hunt && InvasionTarget != null && squad.EnemyCommunityId == InvasionTarget.Id)
		{
			LastInvasionTime = Session.Instance.PlayTime;
		}
		Squads.Remove(squad);
		Session.Instance.CommunityManager.SquadsWithMapIcons.Remove(squad);
	}

	public Squad AddSquad(SquadBehaviour behaviour, int enemyCommunityId)
	{
		Squad squad = new Squad();
		squad.Id = Session.Instance.CommunityManager.NextFreeSquadId++;
		squad.Behaviour = behaviour;
		squad.EnemyCommunityId = enemyCommunityId;
		squad.SquadOwner = this;
		squad.SquadCreatedTime = Session.Instance.PlayTime;
		Squads.Add(squad);
		return squad;
	}

	public Squad GetSquad(int squadId)
	{
		if (squadId == 0)
		{
			return null;
		}
		foreach (Squad squad in Squads)
		{
			if (squad.Id == squadId)
			{
				return squad;
			}
		}
		return null;
	}

	public int GetNumberOfSquadMembersAssignedToThreat(int threatId)
	{
		int num = 0;
		foreach (Squad squad in Squads)
		{
			if (squad.ThreatId == threatId)
			{
				num += squad.Members.Count;
			}
		}
		return num;
	}

	private Threat AreAnySquadMembersThreatened(Squad squad)
	{
		foreach (Threat threat in Threats)
		{
			if (Session.Instance.PlayTime - threat.LastEncounteredTime >= Target.LoseTargetTime)
			{
				continue;
			}
			foreach (Character member in squad.Members)
			{
				if (member.Tile.IsWithinBounds(threat.BoundsMin, threat.BoundsMax))
				{
					return threat;
				}
			}
		}
		return null;
	}

	private bool WantToRetreat(Squad squad)
	{
		int num = squad.InitialSquadMemberCount - squad.Members.Count;
		foreach (Character member in squad.Members)
		{
			if (member.IsFleeing() || !member.IsConscious)
			{
				num++;
			}
			else if (member.FindActiveGoal(GoalType.RescueGoal) is RescueGoal rescueGoal && rescueGoal.IsCarryingBodyHome(member))
			{
				num++;
			}
		}
		return num >= Math.Max(1, squad.InitialSquadMemberCount / 2);
	}

	private void UpdateThreats()
	{
		Session instance = Session.Instance;
		CommunityManager communityManager = instance.CommunityManager;
		for (int num = Threats.Count - 1; num >= 0; num--)
		{
			Threat threat = Threats[num];
			for (int num2 = threat.ThreatMembers.Count - 1; num2 >= 0; num2--)
			{
				if (threat.ThreatMembers[num2] == null || !CanBeThreat(threat.ThreatMembers[num2]) || (threat.ThreatMembers[num2].Community != null && communityManager.GetRelationship(this, threat.ThreatMembers[num2].Community) != CommunityRelationshipType.Hostile))
				{
					threat.ThreatMembers.RemoveAt(num2);
				}
			}
			for (int num3 = Threats.Count - 1; num3 > num; num3--)
			{
				Threat threat2 = Threats[num3];
				if (TerrainCoord.Overlaps(threat.BoundsMin, threat.BoundsMax, threat2.BoundsMin, threat2.BoundsMax))
				{
					foreach (Character threatMember in threat2.ThreatMembers)
					{
						threat.ThreatMembers.Add(threatMember);
					}
					foreach (Squad squad3 in Squads)
					{
						if (squad3.ThreatId == threat2.Id)
						{
							squad3.ThreatId = threat.Id;
						}
					}
					threat.LastEncounteredTime = TimeSpan.FromTicks(Math.Max(threat.LastEncounteredTime.Ticks, threat2.LastEncounteredTime.Ticks));
					threat2.ThreatMembers.Clear();
					Threats.RemoveAt(num3);
				}
			}
			int num4 = GetNumberOfSquadMembersAssignedToThreat(threat.Id);
			TimeSpan timeSpan = instance.PlayTime - threat.LastEncounteredTime;
			if ((timeSpan >= ForgetThreatTime || threat.ThreatMembers.Count == 0) && num4 == 0)
			{
				Threats.RemoveAt(num);
			}
			else if ((CommunityType == CommunityType.Normal || CommunityType == CommunityType.Looter) && timeSpan < Target.LoseTargetTime && threat.ThreatMembers.Count > 0)
			{
				TerrainCoord other = (threat.BoundsMin + threat.BoundsMax) / 2;
				if (num4 < threat.ThreatMembers.Count + 3)
				{
					Squad squad = null;
					while (squad == null || squad.Members.Count < 5)
					{
						Character character = null;
						float num5 = float.MaxValue;
						foreach (Character member in Members)
						{
							if (member.Alive && member.Consciousness != Consciousness.Unconscious && member.GetBaseObjectType() == BaseObjectType.Human && member.Zombie == IsZombieCommunity() && member.SquadId == 0 && member.Rank != Rank.Captive && !member.IsGuarding() && !member.CanFollowPlayer && member.SquadLeader == null && !member.IsFleeing())
							{
								TerrainCoord tile = member.Tile;
								float distSquared = tile.GetDistSquared(other);
								if (distSquared < num5 && (distSquared <= MaxReactToThreatDist * MaxReactToThreatDist || tile.IsWithinBounds(threat.BoundsMin, threat.BoundsMax)))
								{
									character = member;
									num5 = distSquared;
								}
							}
						}
						if (character == null)
						{
							break;
						}
						if (squad == null)
						{
							squad = AddSquad(SquadBehaviour.Defend, 0);
							SetSquadAction(squad, SquadAction.AttackThreat, threat.Id, TerrainCoord.Invalid, null);
						}
						AddToSquad(character, squad);
						num4++;
					}
				}
			}
		}
		if (!ShouldAbandonAISettlement())
		{
			return;
		}
		bool flag = false;
		Community community = null;
		foreach (Community community2 in communityManager.Communities)
		{
			if (community2.IsTemporaryCommunity() && community2.Members.Count > 0 && community2.Members[0].InitialCommunity == this)
			{
				community = community2;
				flag = true;
			}
		}
		if (community == null)
		{
			community = Spawn(IsLooterCommunity() ? CommunityType.TemporaryLooter : CommunityType.Temporary);
		}
		Squad squad2 = null;
		squad2 = ((community.Squads.Count <= 0 || community.Squads[0].Behaviour != SquadBehaviour.Trade) ? community.AddSquad(SquadBehaviour.Trade, 0) : community.Squads[0]);
		if (Leader != null && Leader.ConsciousAndNotZombie && Leader.GetBaseObjectType() == BaseObjectType.Human)
		{
			Character leader = Leader;
			community.AddMember(leader);
			community.AddToSquad(leader, squad2);
			leader.SetRank(Rank.Leader);
		}
		for (int num6 = Members.Count - 1; num6 >= 0; num6--)
		{
			Character character2 = Members[num6];
			if (character2.ConsciousAndNotZombie && character2.GetBaseObjectType() == BaseObjectType.Human)
			{
				community.AddMember(character2);
				community.AddToSquad(character2, squad2);
			}
		}
		if (community.CommunityName.Type == GangNameType.CustomString)
		{
			community.CommunityName.Randomise(instance.DeterministicRand, unique: true, community);
			community.CommunityNameKnown |= community.HasAnyMembersNamesKnownToPlayer();
		}
		if (!flag)
		{
			CommunityRelationshipType relationship = GetRelationship(communityManager.PlayerCommunity);
			if (relationship >= CommunityRelationshipType.Known)
			{
				communityManager.SetRelationship(community, communityManager.PlayerCommunity, relationship, showWarNotifications: false);
			}
		}
		community.GoToNextTradeDestination(squad2, instance.DeterministicRand, mustMove: false, canOccupyBases: false);
	}

	private bool ShouldAbandonAISettlement()
	{
		if (Threats.Count > 0 && IsAISettlement() && !IsFEMA)
		{
			if (Leader != null && Leader.ConsciousAndNotZombie)
			{
				return false;
			}
			bool result = false;
			{
				foreach (Character member in Members)
				{
					if (member.ConsciousAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
					{
						result = true;
						if (member.DontLeaveCommunity)
						{
							return false;
						}
						if (IsTileInsidePerimeter(member.Tile))
						{
							return false;
						}
						if (!member.IsTooDepressedToFollowOrders() && !member.IsFleeing())
						{
							return false;
						}
					}
				}
				return result;
			}
		}
		return false;
	}

	public bool HasAnyMembersNamesKnownToPlayer()
	{
		foreach (Character member in Members)
		{
			if (member.NameKnown)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyMemberInsidePerimeter()
	{
		if (!IsAISettlement())
		{
			return true;
		}
		foreach (Character member in Members)
		{
			if (member.ConsciousAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && IsTileInsidePerimeter(member.Tile))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsAttackTargetInaccessible(Character character)
	{
		if (character.FindActiveGoal(GoalType.Attack) is Attack attack && !attack.IsTargetDeleted() && attack.Target.Object == attack.PreferredTarget && attack.Target.IsInaccessible())
		{
			return true;
		}
		return false;
	}

	private bool CanPillage(TileObject obj)
	{
		if (obj is Campfire)
		{
			return false;
		}
		if (obj is ITrap)
		{
			return false;
		}
		if (obj is Grave)
		{
			return false;
		}
		if (obj.IsLooterProp())
		{
			return false;
		}
		if (obj is Character character && (IsZombieCommunity() ? (!character.AliveAndNotZombie) : (!character.ConsciousAndNotZombie)))
		{
			return false;
		}
		return true;
	}

	public void UpdateSquads()
	{
		for (int num = Squads.Count - 1; num >= 0; num = Math.Min(num - 1, Squads.Count - 1))
		{
			Squad squad = Squads[num];
			Character leader = squad.GetLeader();
			switch (squad.Action)
			{
			case SquadAction.Pillage:
				if (leader.IsLeaderCommandFinished() || IsAttackTargetInaccessible(leader))
				{
					squad.ActionFinished = true;
					if (CommunityType != CommunityType.Psycho)
					{
						squad.GoalAchieved = true;
					}
				}
				else
				{
					if (leader.Zombie || leader.InCombat)
					{
						break;
					}
					TerrainCoord tile2 = leader.Tile;
					int num2 = 8;
					GameTerrain.Instance.GetObjectsInRect(tile2 - new TerrainCoord(num2, num2), tile2 + new TerrainCoord(num2, num2), _nearbyObjects);
					bool flag = squad.HasAnyEquipmentOfClass(typeof(RPG)) || squad.HasAnyEquipmentOfClass(typeof(PipeBomb));
					bool flag2 = squad.HasAnyEquipmentOfClass(typeof(MolotovCocktail));
					for (int num3 = _nearbyObjects.Count - 1; num3 >= 0; num3--)
					{
						TileObject tileObject = _nearbyObjects[num3];
						Community community = tileObject.GetCommunity();
						int num4 = ((squad.Behaviour == SquadBehaviour.Occupy) ? Session.Instance.CommunityManager.PlayerCommunity.Id : squad.EnemyCommunityId);
						if (community == null || (community.Id != num4 && GetRelationship(community) != CommunityRelationshipType.Hostile) || tileObject.IsDestroyed() || !CanPillage(tileObject) || !TerrainCoord.Overlaps(tileObject.GetMinTile(), tileObject.GetMaxTile(), tile2 - new TerrainCoord(num2, num2), tile2 + new TerrainCoord(num2, num2)))
						{
							_nearbyObjects.RemoveAt(num3);
						}
						else if ((!flag2 || !tileObject.IsFlammable()) && (!flag || tileObject.IsExplosionProof()))
						{
							_nearbyObjects.RemoveAt(num3);
						}
					}
					if (_nearbyObjects.Count > 0)
					{
						TileObject tileObject2 = _nearbyObjects[Session.Instance.DeterministicRand.Next(_nearbyObjects.Count)];
						leader.SetLeaderCommand(new Attack(leader, tileObject2, dontOpenOurGates: false, StayInRangeParams.OfSquadLeader()), tileObject2, ObeyLeaderGoal.SourceType.SquadLeader);
						squad.PillageObjectId = tileObject2.Id;
					}
					_nearbyObjects.Clear();
				}
				break;
			case SquadAction.AttackThreat:
			{
				Threat threat = FindThreatById(squad.ThreatId);
				if (threat == null || threat.ThreatMembers.Count == 0)
				{
					squad.ActionFinished = true;
					break;
				}
				TimeSpan timeSpan = Session.Instance.PlayTime - threat.LastEncounteredTime;
				if (leader.InCombat)
				{
					break;
				}
				if (timeSpan < Target.LoseTargetTime)
				{
					if (!threat.IsThreatInaccessibleTo(leader) || leader.FindActiveGoal(GoalType.Conversation) == null)
					{
						AssignSquadActionToLeader(squad);
					}
				}
				else
				{
					squad.ActionFinished = true;
				}
				break;
			}
			case SquadAction.GoTo:
			case SquadAction.Retreat:
			case SquadAction.GoToHighPrio:
				if (leader.IsLeaderCommandFinished())
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.TalkToAmbush:
				if (leader.IsLeaderCommandFinished() && leader.QueuedSpeeches.Count == 0)
				{
					if (squad.GoalAchieved || squad.GoalCharacter == null || squad.GoalCharacter.Deleted || !squad.GoalCharacter.AliveAndNotZombie)
					{
						squad.ActionFinished = true;
					}
					else
					{
						AssignSquadActionToLeader(squad);
					}
				}
				break;
			case SquadAction.GoToAmbush:
				if (!squad.Teleported && !squad.IsAnyoneInSquadVisibleToPlayer() && Session.Instance.PlayTime - squad.ActionStartedTime >= squad.TeleportAfterTime)
				{
					CustomRandom deterministicRand = Session.Instance.DeterministicRand;
					TerrainCoord tile3 = squad.GoalCharacter.Tile;
					foreach (Character member in squad.Members)
					{
						TerrainCoord terrainCoord = ((member == leader) ? tile3 : leader.Tile);
						TerrainCoord tile4 = terrainCoord;
						int num5 = 0;
						while (GameTerrain.Instance.IsImpassable(tile4.x, tile4.y, 2051, member, null) || !GameTerrain.Instance.IsTileSpawnable(tile4.x, tile4.y) || Session.Instance.IsVisibleDeterministic(GameTerrain.Instance.GetTileCentreXZ(tile4)))
						{
							num5++;
							tile4 = deterministicRand.RandomTile(terrainCoord - new TerrainCoord(num5, num5), terrainCoord + new TerrainCoord(num5, num5));
							if (num5 > 64)
							{
								break;
							}
						}
						if (member.InsideBuilding != null)
						{
							member.InsideBuilding.OnCharacterLeave(member, 0, fromBuildingDestroyed: false, fromRagdolled: false);
						}
						Vector2 tileCentreXZ = GameTerrain.Instance.GetTileCentreXZ(tile4);
						member.SetPosition(tileCentreXZ.x, tileCentreXZ.y);
						member.SetFollowMePos(Vector2.zero);
						member.HaltMovement();
					}
					squad.Teleported = true;
					squad.ActionFinished = true;
				}
				if (leader.IsLeaderCommandFinished())
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.SearchForItem:
				if (leader.IsLeaderCommandFinished())
				{
					squad.GoalAchieved = FindInventoryItemOfType(squad.AmbushForEquipmentType, includeDead: false) != null;
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.Wait:
				if (!squad.IsEveryoneInSquadIdleOrSatisfyingNeeds())
				{
					squad.LastEncounterTime = Session.Instance.PlayTime;
				}
				if (Session.Instance.PlayTime - squad.LastEncounterTime >= TimeSpan.FromSeconds(20.0))
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.Trade:
				if (Session.Instance.PlayTime - squad.ActionStartedTime >= Sun.DayLength && squad.IsEveryoneInSquadBored())
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.Bury:
				if (leader.IsLeaderCommandFinished())
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.Enter:
			{
				if (leader.IsLeaderCommandFinished())
				{
					squad.ActionFinished = true;
				}
				Building building = GameTerrain.Instance.GetBuilding(squad.DestTile.x, squad.DestTile.y);
				if (building == null || squad.IsEveryoneInSquadInsideBuilding(building))
				{
					squad.ActionFinished = true;
				}
				break;
			}
			case SquadAction.FlyAway:
				if (GameTerrain.Instance.GetBuilding(squad.DestTile.x, squad.DestTile.y) is Helicopter helicopter)
				{
					switch (helicopter.CurrentHelicopterAnimState)
					{
					case HelicopterAnimState.Idle:
						helicopter.SetHelicopterAnimState(HelicopterAnimState.RotorSpinning);
						break;
					case HelicopterAnimState.RotorSpinning:
						if (Session.Instance.PlayTime - squad.ActionStartedTime >= TimeSpan.FromSeconds(10.0))
						{
							helicopter.SetHelicopterAnimState(HelicopterAnimState.TakingOff);
						}
						break;
					case HelicopterAnimState.TakingOff:
						if (helicopter.IsTakeOffFinished())
						{
							squad.ActionFinished = true;
						}
						break;
					}
				}
				else if (!Session.Instance.IsVisibleDeterministic(leader.PosXZ))
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.FindTarget:
				if (squad.GoalAchieved)
				{
					if (!(leader.GetLeaderCommandEvenIfItIsInactive() is RangedAttack))
					{
						squad.ActionFinished = true;
					}
				}
				else if (leader.IsLeaderCommandFinished())
				{
					if (squad.GoalCharacter != null && squad.GoalCharacter.AliveAndNotZombie && squad.GoalCharacter.Consciousness < Consciousness.Unconscious)
					{
						squad.GoalCharacter.SetRecentActivity(RecentActivityType.Doorbell, squad.GetLeader());
					}
					if (!StartFindTarget(squad))
					{
						squad.ActionFinished = true;
					}
				}
				break;
			case SquadAction.StockUpFood:
				if (squad.HasEveryoneInSquadFinishedLeaderCommand() && (!squad.WasLeaderCommandSuccessfulForAnyoneInSquad() || !StartStockUpFood(squad)))
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.StockUpWater:
				if (squad.HasEveryoneInSquadFinishedLeaderCommand() && (!squad.WasLeaderCommandSuccessfulForEveryoneInSquad() || !StartStockUpWater(squad)))
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.StopForWarmth:
				if (squad.GetLowestBodyTemperature() > Character.BodyTemperatureInCelsiusShivering)
				{
					squad.ActionFinished = true;
				}
				else if (leader.IsLeaderCommandFinished())
				{
					squad.ActionFinished = true;
					squad.StopForWarmthFailedTime = Session.Instance.PlayTime;
				}
				else if (squad.GoalAchieved && !(leader.GetLeaderCommandEvenIfItIsInactive() is RangedAttack))
				{
					squad.ActionFinished = true;
				}
				break;
			case SquadAction.HangAround:
			{
				if (!squad.IsAnyoneInSquadInConversation(out var _))
				{
					squad.ActionFinished = true;
				}
				else if (leader.IsLeaderCommandFinished())
				{
					AssignSquadActionToLeader(squad);
				}
				break;
			}
			}
			switch (squad.Behaviour)
			{
			case SquadBehaviour.Defend:
				if (squad.Action == SquadAction.AttackThreat)
				{
					if (leader.IsFleeing())
					{
						squad.ActionFinished = true;
					}
					if (squad.ActionFinished)
					{
						RemoveSquad(squad);
					}
				}
				break;
			case SquadBehaviour.Hunt:
				switch (squad.Action)
				{
				case SquadAction.AttackThreat:
					if (CommunityType != CommunityType.Psycho)
					{
						if (WantToRetreat(squad))
						{
							squad.EverybodyFleeing = true;
							SetSquadAction(squad, SquadAction.Retreat, squad.ThreatId, TerrainCoord.Invalid, null);
							if (!InvasionTargetWasSetFromScript)
							{
								SetInvasionTarget(null, 0f, fromScript: false);
							}
							break;
						}
						if (!squad.GoalAchieved)
						{
							Threat threat4 = FindThreatById(squad.ThreatId);
							if (threat4 != null)
							{
								foreach (Character threatMember in threat4.ThreatMembers)
								{
									if (threatMember.GetCommunityId() == squad.EnemyCommunityId)
									{
										squad.GoalAchieved = true;
										break;
									}
								}
							}
						}
					}
					if (squad.ActionFinished && !StartPillaging(squad, !squad.GoalAchieved))
					{
						StartJourneyToExitMap(squad);
					}
					break;
				case SquadAction.Retreat:
					if (squad.ActionFinished)
					{
						StartJourneyToExitMap(squad, scripted: false, avoidHostileBases: true);
					}
					break;
				case SquadAction.Pillage:
				{
					Threat threat5 = AreAnySquadMembersThreatened(squad);
					if (threat5 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat5.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished && (squad.PillageObjectId == 0 || (BaseObjectManager.Instance.FindBaseObjectByID(squad.PillageObjectId) is TileObject tileObject3 && !tileObject3.IsDestroyedNotIncludingDeadCrops()) || !StartPillaging(squad, !squad.GoalAchieved)))
					{
						if (IsHunterCommunity() || CommunityType == CommunityType.Psycho)
						{
							SetSquadAction(squad, SquadAction.Wait, 0, TerrainCoord.Invalid, null);
						}
						else
						{
							RemoveSquad(squad);
						}
					}
					break;
				}
				case SquadAction.Wait:
				{
					Threat threat3 = AreAnySquadMembersThreatened(squad);
					if (threat3 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat3.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished && (squad.GoalAchieved || !StartPillaging(squad, canAttackOutsideBase: false)))
					{
						StartJourneyToExitMap(squad);
					}
					break;
				}
				}
				break;
			case SquadBehaviour.Travel:
			case SquadBehaviour.Trade:
				switch (squad.Action)
				{
				case SquadAction.GoTo:
				case SquadAction.GoToHighPrio:
					if (squad.IsAnyoneInSquadInCombatOrAlertOrConversation())
					{
						squad.MovedAlong = false;
						SetSquadAction(squad, SquadAction.Wait, 0, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished)
					{
						squad.MovedAlong = false;
						if (squad.Behaviour == SquadBehaviour.Trade)
						{
							SetSquadAction(squad, SquadAction.Trade, 0, squad.GoalTile, null);
						}
						else
						{
							SetSquadAction(squad, SquadAction.ExitMap, 0, TerrainCoord.Invalid, null);
						}
					}
					break;
				case SquadAction.Wait:
					if (squad.ActionFinished)
					{
						SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null);
					}
					break;
				case SquadAction.Trade:
					if (squad.ActionFinished)
					{
						GoToNextTradeDestination(squad, Session.Instance.DeterministicRand, mustMove: false, IsTemporaryCommunity());
					}
					break;
				case SquadAction.AttackThreat:
				case SquadAction.Pillage:
					if (squad.ActionFinished)
					{
						squad.MovedAlong = false;
						SetSquadAction(squad, SquadAction.Wait, 0, TerrainCoord.Invalid, null);
					}
					break;
				case SquadAction.ExitMap:
					if (!IsHunterCommunity())
					{
						DeleteMe = true;
					}
					break;
				}
				break;
			case SquadBehaviour.Funeral:
				switch (squad.Action)
				{
				case SquadAction.AttackThreat:
					if (squad.ActionFinished)
					{
						SetSquadAction(squad, SquadAction.Bury, 0, squad.GoalTile, squad.GoalCharacter);
					}
					break;
				case SquadAction.Bury:
					if (WantToRetreat(squad))
					{
						if (squad.GoalCharacter != null && !IsTileInsidePerimeter(squad.GoalCharacter.Tile))
						{
							AddFailedToBury(squad.GoalCharacter);
							RemoveUnfinishedGraves();
						}
						squad.ActionFinished = true;
					}
					if (squad.ActionFinished)
					{
						if (CommunityType == CommunityType.RovingTrader || IsTemporaryCommunity())
						{
							squad.Behaviour = SquadBehaviour.Trade;
							GoToNextTradeDestination(squad, Session.Instance.DeterministicRand, mustMove: false, canOccupyBases: false);
						}
						else
						{
							StartJourneyToExitMap(squad);
						}
					}
					break;
				}
				break;
			case SquadBehaviour.Occupy:
			{
				Session instance = Session.Instance;
				CommunityManager communityManager = instance.CommunityManager;
				Community playerCommunity = communityManager.PlayerCommunity;
				CommunityRelationshipType relationship = communityManager.GetRelationship(this, playerCommunity);
				if (relationship == CommunityRelationshipType.Ceasefire && IsAmbientCommunity())
				{
					StartJourneyToExitMap(squad, scripted: false, avoidHostileBases: true);
					break;
				}
				if (squad.Action != SquadAction.AttackThreat)
				{
					Threat threat6 = AreAnySquadMembersThreatened(squad);
					if (threat6 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat6.Id, TerrainCoord.Invalid, null);
					}
				}
				switch (squad.Action)
				{
				case SquadAction.GoTo:
				case SquadAction.GoToHighPrio:
				{
					if (!squad.ActionFinished)
					{
						break;
					}
					Community community2 = BaseObjectManager.Instance.FindBaseObjectByID(squad.EnemyCommunityId) as Community;
					if (community2 != null && !community2.HasAnyLivingNonZombieMembers() && community2.CanBeOccupied())
					{
						List<TileObject> playerObjectsInBaseRect = community2.GetPlayerObjectsInBaseRect();
						Threat threat7 = AreAnySquadMembersThreatened(squad);
						if (threat7 != null)
						{
							SetSquadAction(squad, SquadAction.AttackThreat, threat7.Id, TerrainCoord.Invalid, null);
						}
						else if (playerObjectsInBaseRect.Count > 0)
						{
							if (relationship == CommunityRelationshipType.Hostile)
							{
								TileObject tileObject4 = playerObjectsInBaseRect[instance.DeterministicRand.Next(playerObjectsInBaseRect.Count)];
								SetSquadAction(squad, SquadAction.Pillage, 0, tileObject4.GetCentreTile(), null);
							}
							else
							{
								StartJourneyToExitMap(squad, scripted: false, avoidHostileBases: true);
							}
						}
						else
						{
							RemoveSquad(squad);
							OccupyBase(community2);
							playerObjectsInBaseRect.Clear();
						}
					}
					else if (IsTemporaryCommunity() && community2 != null && community2.HasAnyLivingNonZombieMembers() && Members.Count > 0 && Members[0].InitialCommunity == community2)
					{
						for (int num6 = Members.Count - 1; num6 >= 0; num6--)
						{
							Character character = Members[num6];
							RemoveMember(character, cantDeleteMeHere: true);
							community2.AddMember(character);
						}
					}
					else
					{
						StartJourneyToExitMap(squad);
					}
					break;
				}
				case SquadAction.AttackThreat:
				case SquadAction.Pillage:
					if (IsTemporaryCommunity() && WantToRetreat(squad))
					{
						squad.Behaviour = SquadBehaviour.Trade;
						GoToNextTradeDestination(squad, Session.Instance.DeterministicRand, mustMove: false, canOccupyBases: false);
					}
					else if (squad.ActionFinished)
					{
						SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null);
					}
					break;
				}
				break;
			}
			case SquadBehaviour.Exfil:
				switch (squad.Action)
				{
				case SquadAction.Enter:
					if (squad.ActionFinished)
					{
						SetSquadAction(squad, SquadAction.FlyAway, 0, squad.DestTile, null);
					}
					break;
				case SquadAction.FlyAway:
				{
					Building building2 = GameTerrain.Instance.GetBuilding(squad.DestTile.x, squad.DestTile.y);
					if (building2 == null)
					{
						StartJourneyToExitMap(squad, scripted: true, avoidHostileBases: true);
					}
					else if (!squad.IsEveryoneInSquadInsideBuilding(building2))
					{
						SetSquadAction(squad, SquadAction.Enter, 0, squad.DestTile, null);
						break;
					}
					if (squad.ActionFinished)
					{
						DeleteMe = true;
					}
					break;
				}
				}
				break;
			case SquadBehaviour.Extortion:
			case SquadBehaviour.Beg:
			case SquadBehaviour.WarnOffAlliance:
			case SquadBehaviour.WarnPopulation:
			case SquadBehaviour.RequestAlliance:
				switch (squad.Action)
				{
				case SquadAction.StockUpFood:
				{
					Threat threat11 = AreAnySquadMembersThreatened(squad);
					if (threat11 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat11.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished && !StartStockUpWater(squad) && !StartFindTarget(squad))
					{
						RemoveSquad(squad);
					}
					break;
				}
				case SquadAction.StockUpWater:
				{
					Threat threat9 = AreAnySquadMembersThreatened(squad);
					if (threat9 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat9.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished && !StartFindTarget(squad))
					{
						RemoveSquad(squad);
					}
					break;
				}
				case SquadAction.StopForWarmth:
				{
					Threat threat10 = AreAnySquadMembersThreatened(squad);
					if (threat10 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat10.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished)
					{
						if (squad.GoalAchieved)
						{
							GoHome(squad);
						}
						else if (!StartFindTarget(squad))
						{
							RemoveSquad(squad);
						}
					}
					break;
				}
				case SquadAction.FindTarget:
				{
					Threat threat13 = AreAnySquadMembersThreatened(squad);
					if (threat13 != null)
					{
						Character leader2 = squad.GetLeader();
						Attack attack = ((leader2 != null) ? (leader2.GetLeaderCommand() as Attack) : null);
						if (attack == null || attack.PreferredTarget == null || attack.PreferredTarget.IsDestroyed())
						{
							SetSquadAction(squad, SquadAction.AttackThreat, threat13.Id, TerrainCoord.Invalid, null);
						}
					}
					else if (squad.ActionFinished)
					{
						GoHome(squad);
					}
					else if (Session.Instance.PlayTime - squad.StopForWarmthFailedTime > TimeSpan.FromSeconds(600.0) && squad.GetLowestBodyTemperature() < Character.BodyTemperatureInCelsiusMildHypothermia)
					{
						SetSquadAction(squad, SquadAction.StopForWarmth, 0, TerrainCoord.Invalid, null);
					}
					break;
				}
				case SquadAction.AttackThreat:
					if (WantToRetreat(squad))
					{
						squad.EverybodyFleeing = true;
						SetSquadAction(squad, SquadAction.Retreat, squad.ThreatId, TerrainCoord.Invalid, null);
						if (!InvasionTargetWasSetFromScript)
						{
							SetInvasionTarget(null, 0f, fromScript: false);
						}
					}
					else
					{
						if (!squad.ActionFinished)
						{
							break;
						}
						if (squad.EverybodyFleeing || squad.GoalAchieved)
						{
							GoHome(squad);
						}
						else if (BaseRect.VerticesArea > 0 && BaseRect.Expand(32).Contains(squad.GetLeader().Tile))
						{
							if (!StartStockUpFood(squad) && !StartStockUpWater(squad) && !StartFindTarget(squad))
							{
								RemoveSquad(squad);
							}
						}
						else if (!StartFindTarget(squad))
						{
							GoHome(squad);
						}
					}
					break;
				case SquadAction.Retreat:
					if (squad.ActionFinished)
					{
						GoHome(squad);
					}
					break;
				case SquadAction.GoTo:
				case SquadAction.GoToHighPrio:
				{
					Threat threat12 = AreAnySquadMembersThreatened(squad);
					TerrainCoord tile5;
					if (threat12 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat12.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.IsAnyoneInSquadInConversation(out tile5) && squad.GetLowestBodyTemperature() > Character.BodyTemperatureInCelsiusModerateHypothermia)
					{
						SetSquadAction(squad, SquadAction.HangAround, 0, tile5, null);
					}
					else if (squad.ActionFinished)
					{
						RemoveSquad(squad);
					}
					break;
				}
				case SquadAction.HangAround:
				{
					Threat threat8 = AreAnySquadMembersThreatened(squad);
					if (threat8 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat8.Id, TerrainCoord.Invalid, null);
					}
					else if (squad.ActionFinished || squad.GetLowestBodyTemperature() <= Character.BodyTemperatureInCelsiusModerateHypothermia)
					{
						GoHome(squad);
					}
					break;
				}
				}
				break;
			case SquadBehaviour.Ambush:
				if (squad.Action != SquadAction.AttackThreat && (squad.Action != SquadAction.GoTo || !squad.EverybodyFleeing))
				{
					Threat threat2 = AreAnySquadMembersThreatened(squad);
					if (threat2 != null)
					{
						SetSquadAction(squad, SquadAction.AttackThreat, threat2.Id, TerrainCoord.Invalid, squad.GoalCharacter);
					}
				}
				switch (squad.Action)
				{
				case SquadAction.GoToAmbush:
					if (squad.ActionFinished)
					{
						SetSquadAction(squad, SquadAction.TalkToAmbush, 0, TerrainCoord.Invalid, squad.GoalCharacter);
					}
					break;
				case SquadAction.TalkToAmbush:
					if (squad.ActionFinished)
					{
						if (squad.GoalAchieved)
						{
							GoHome(squad);
						}
						else if (!SearchForAmbushItem(squad))
						{
							GoHome(squad);
						}
					}
					break;
				case SquadAction.SearchForItem:
					if (squad.ActionFinished)
					{
						GoHome(squad);
					}
					break;
				case SquadAction.GoTo:
				case SquadAction.GoToHighPrio:
					if (squad.ActionFinished)
					{
						RemoveSquad(squad);
					}
					break;
				case SquadAction.AttackThreat:
					if (WantToRetreat(squad))
					{
						squad.EverybodyFleeing = true;
						squad.ActionFinished = true;
					}
					if (squad.ActionFinished && (squad.EverybodyFleeing || FindInventoryItemOfType(squad.AmbushForEquipmentType, includeDead: false) != null || !SearchForAmbushItem(squad)))
					{
						if (squad.GoalAchieved || squad.EverybodyFleeing || squad.GoalCharacter == null || squad.GoalCharacter.Deleted || !squad.GoalCharacter.AliveAndNotZombie)
						{
							GoHome(squad);
						}
						else
						{
							SetSquadAction(squad, SquadAction.GoToAmbush, 0, TerrainCoord.Invalid, squad.GoalCharacter);
						}
					}
					break;
				}
				break;
			}
		}
	}

	private bool SearchForAmbushItem(Squad squad)
	{
		if (FindInventoryItemOfType(squad.AmbushForEquipmentType, includeDead: true) != null)
		{
			SetSquadAction(squad, SquadAction.SearchForItem, 0, TerrainCoord.Invalid, squad.GoalCharacter);
			return true;
		}
		for (int i = 0; i < CommunityRelationships.Count; i++)
		{
			if (CommunityRelationships[i].RelationshipType != CommunityRelationshipType.Hostile)
			{
				continue;
			}
			Community otherCommunity = CommunityRelationships[i].GetOtherCommunity();
			if (otherCommunity == null)
			{
				continue;
			}
			TileObject tileObject = otherCommunity.FindInventoryItemOfType(squad.AmbushForEquipmentType, includeDead: true);
			if (tileObject != null)
			{
				if (!(tileObject is Character { IsConscious: not false }))
				{
					SetSquadAction(squad, SquadAction.SearchForItem, 0, TerrainCoord.Invalid, squad.GoalCharacter);
					return true;
				}
				SetInvasionTarget(otherCommunity, 21f, fromScript: true);
				GoHome(squad);
				return true;
			}
		}
		if (squad.GoalCharacter != null && squad.GoalCharacter.Community != null && squad.GoalCharacter.Community.FindInventoryItemOfType(squad.AmbushForEquipmentType, includeDead: true) is Character { IsConscious: false })
		{
			SetSquadAction(squad, SquadAction.SearchForItem, 0, TerrainCoord.Invalid, squad.GoalCharacter);
			return true;
		}
		return false;
	}

	public static bool CanBeOccupied(TileObject prop)
	{
		if (prop is Grave)
		{
			return false;
		}
		if (prop is Character)
		{
			return false;
		}
		return true;
	}

	public bool CanBeOccupiedBy(Community occupier)
	{
		if (!IsAISettlement())
		{
			return false;
		}
		if (HasAnyActiveMembers())
		{
			return false;
		}
		if (BaseRect.VerticesArea <= 0)
		{
			return false;
		}
		if (Session.Instance.CommunityManager.FindCommunityTryingToOccupyBase(this) != null)
		{
			return false;
		}
		if (Nemesis && !occupier.IsLooterCommunity())
		{
			return false;
		}
		return CanBeOccupied();
	}

	public bool CanBeOccupied()
	{
		if (BuildingsCantBeCaptured)
		{
			return false;
		}
		foreach (Prop building in Buildings)
		{
			if (CanBeOccupied(building))
			{
				return true;
			}
		}
		foreach (TileObject underConstructionBuilding in UnderConstructionBuildings)
		{
			if (CanBeOccupied(underConstructionBuilding))
			{
				return true;
			}
		}
		return ConstructionRecords.Count > 0;
	}

	public void OccupyBase(Community targetCommunity)
	{
		Session instance = Session.Instance;
		CommunityManager communityManager = instance.CommunityManager;
		instance.CommunityManager.Hunters.Remove(this);
		CommunityType = ((!IsLooterCommunity()) ? CommunityType.Normal : CommunityType.Looter);
		if ((CommunityName.Type != GangNameType.CustomString || string.IsNullOrEmpty(CommunityName.CustomString)) && (CommunityName.Type != GangNameType.TranslatedString || string.IsNullOrEmpty(CommunityName.TranslatedStringKey)))
		{
			CommunityName.Randomise(instance.DeterministicRand, unique: true, this);
		}
		InitialMemberCount = targetCommunity.InitialMemberCount;
		InitialChickenCount = targetCommunity.InitialChickenCount;
		BaseRect = targetCommunity.BaseRect;
		Perimeter = targetCommunity.Perimeter;
		ConstructionRecords = targetCommunity.ConstructionRecords;
		targetCommunity.ConstructionRecords = new List<ConstructionRecord>();
		StoryManager.Instance.RemoveFromInvaderInstance(this);
		for (int num = targetCommunity.UnderConstructionBuildings.Count - 1; num >= 0; num--)
		{
			TileObject tileObject = targetCommunity.UnderConstructionBuildings[num];
			if (CanBeOccupied(tileObject))
			{
				tileObject.SetCommunity(this);
			}
		}
		for (int num2 = targetCommunity.Buildings.Count - 1; num2 >= 0; num2--)
		{
			Prop prop = targetCommunity.Buildings[num2];
			if (CanBeOccupied(prop))
			{
				prop.SetCommunity(this);
				if (prop is Gate { GateState: GateState.Open } gate)
				{
					Session.Instance.PropManager.AddToObjectsThatNeedUpdating(gate);
				}
			}
		}
		List<PropPrototype> list = new List<PropPrototype>();
		List<float> list2 = new List<float>();
		List<int> list3 = new List<int>();
		foreach (CropPatch patch in instance.CropsManager.Patches)
		{
			if (patch.CommunityId != targetCommunity.Id && (patch.CommunityId != communityManager.PlayerCommunity.Id || !patch.Rect.Overlaps(BaseRect)))
			{
				continue;
			}
			patch.CommunityId = Id;
			int num3 = list.IndexOf(patch.CropType);
			if (num3 == -1)
			{
				num3 = list.Count;
				list.Add(patch.CropType);
				list2.Add(0f);
				list3.Add(0);
			}
			list3[num3] += patch.Tiles.Count;
			foreach (PlantableCrop item in patch.CropsInPatch)
			{
				if (item.CommunityId == targetCommunity.Id || item.CommunityId == communityManager.PlayerCommunity.Id)
				{
					item.SetCommunity(this);
					list2[num3] += item.GetPredictedSeedYield();
				}
			}
		}
		List<TileObject> list4 = new List<TileObject>();
		GameTerrain.Instance.GetObjectsInRect(BaseRect.min, BaseRect.max, list4);
		foreach (TileObject item2 in list4)
		{
			if (item2.GetCommunityId() == targetCommunity.Id && CanBeOccupied(item2))
			{
				item2.SetCommunity(this);
			}
		}
		for (int num4 = targetCommunity.Members.Count - 1; num4 >= 0; num4--)
		{
			Character character = targetCommunity.Members[num4];
			if (character.Alive && character is Animal)
			{
				character.SetCommunity(this);
			}
		}
		foreach (Character member in Members)
		{
			member.TryToEnsureHangoutLocationIsSet(forceOverride: true);
			member.DontSimulateSurvivalFactorsUntilDiscovered = false;
			member.DontSimulateSurvivalFactorsUntilJoinCommunity = false;
			if (member.FindActiveGoal(GoalType.BoredGoal) is BoredGoal boredGoal)
			{
				boredGoal.Finished = true;
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindSeedPrototypeForPlantType(list[i]);
			if (equipmentPrototype != null)
			{
				float num5 = list2[i] + (float)CountInventoryItemsOfType(equipmentPrototype);
				int num6 = list3[i] - (int)num5;
				if (num6 > 0)
				{
					int num7 = Members[instance.DeterministicRand.Next(Members.Count)].SpawnEquipmentIfSpaceIsAvailable(equipmentPrototype, num6, fillLiquidContainers: false);
					list2[i] += num7;
				}
			}
		}
		foreach (Community community in communityManager.Communities)
		{
			foreach (Squad squad in community.Squads)
			{
				if (squad.Behaviour == SquadBehaviour.Occupy && squad.EnemyCommunityId == targetCommunity.Id)
				{
					squad.EnemyCommunityId = Id;
				}
			}
		}
		targetCommunity.ConstructionRecords.Clear();
		int num8 = GetNumCompletedBuildingsOfClass(typeof(Campfire));
		for (int j = 0; j < ConstructionRecords.Count; j++)
		{
			if (ConstructionRecords[j].Proto.TypeName == BaseObjectType.Campfire)
			{
				num8++;
			}
		}
		SetCraftingLimit(EquipmentPrototype.Wood, num8 * GameTerrain.WoodLimitPerCampfire);
	}

	private void UpdateInvasionTarget()
	{
		if (Threats.Count != 0 || Squads.Count != 0 || Leader == null || Leader.Consciousness >= Consciousness.Unconscious || AreAnyMembersInCombat())
		{
			return;
		}
		Session instance = Session.Instance;
		if (InvasionTarget != null && (instance.PlayTime > InvasionTimeoutTime || !InvasionTarget.HasAnyActiveMembers() || !InvasionTarget.IsAnyoneConscious(includeDrunk: false) || GetRelationship(InvasionTarget) != CommunityRelationshipType.Hostile))
		{
			SetInvasionTarget(null, 0f, fromScript: false);
		}
		Community community = InvasionTarget;
		SquadBehaviour squadBehaviour = SquadBehaviour.Hunt;
		if (!CachedAllies.Contains(instance.CommunityManager.PlayerCommunity))
		{
			foreach (Community cachedAlly in CachedAllies)
			{
				if (cachedAlly.InvasionTarget != null)
				{
					community = cachedAlly.InvasionTarget;
				}
			}
		}
		if (community == null && Nemesis && NemesisAllyWarningCount < instance.CommunityManager.PlayerCommunity.GetActiveAllyCount() && !HasSurrenderedTo(instance.CommunityManager.PlayerCommunity))
		{
			community = instance.CommunityManager.PlayerCommunity;
			squadBehaviour = SquadBehaviour.WarnOffAlliance;
		}
		if (community == null && Nemesis && NemesisPopulationWarningCount + 10 < instance.CommunityManager.PlayerCommunity.GetActiveMemberCount() && !HasSurrenderedTo(instance.CommunityManager.PlayerCommunity))
		{
			community = instance.CommunityManager.PlayerCommunity;
			squadBehaviour = SquadBehaviour.WarnPopulation;
		}
		if (community == null && Nemesis && GetActiveAllyCount() < instance.CommunityManager.PlayerCommunity.GetActiveAllyCount())
		{
			ShakedownCommunities.Clear();
			foreach (Community community2 in instance.CommunityManager.Communities)
			{
				if (community2 != this && community2.IsAISettlement() && community2.Leader != null && community2.Leader.AliveAndNotZombie && community2.HasAnyActiveMembers() && community2.IsAnyoneConscious(includeDrunk: true))
				{
					CommunityRelationshipType relationship = GetRelationship(community2);
					if (relationship != CommunityRelationshipType.Hostile && relationship != CommunityRelationshipType.Allied && !community2.CachedAllies.Contains(instance.CommunityManager.PlayerCommunity))
					{
						ShakedownCommunities.Add(community2);
					}
				}
			}
			if (ShakedownCommunities.Count > 0)
			{
				community = ShakedownCommunities[instance.DeterministicRand.Next(ShakedownCommunities.Count)];
				squadBehaviour = SquadBehaviour.RequestAlliance;
			}
			ShakedownCommunities.Clear();
		}
		if (community == null && CommunityType == CommunityType.Looter && instance.PlayTime >= NextExtortionTime)
		{
			ShakedownCommunities.Clear();
			foreach (Community community3 in instance.CommunityManager.Communities)
			{
				if (community3 == this)
				{
					continue;
				}
				if (community3.IsAISettlement())
				{
					if (community3.InitialMemberCount >= InitialMemberCount)
					{
						continue;
					}
				}
				else if (community3.CommunityType != CommunityType.Player)
				{
					continue;
				}
				TimeSpan timeSpan = instance.PlayTime - community3.LastExtortedFromTime;
				if (community3.CommunityType == CommunityType.Player)
				{
					float communityAggro = Session.Instance.CommunityManager.GetCommunityAggro();
					if (communityAggro < (float)ExtortionPresenceThreshold || (communityAggro < (float)(ExtortionPresenceThreshold * 2) && timeSpan < TimeSpan.FromSeconds(5f * Sun.DayLengthSecs)) || (communityAggro < (float)(ExtortionPresenceThreshold * 3) && timeSpan < TimeSpan.FromSeconds(3f * Sun.DayLengthSecs)) || (communityAggro < (float)(ExtortionPresenceThreshold * 4) && timeSpan < TimeSpan.FromSeconds(2f * Sun.DayLengthSecs)) || timeSpan < Sun.DayLength)
					{
						continue;
					}
				}
				else if (!ExtortAISettlements || timeSpan < TimeSpan.FromSeconds(7f * Sun.DayLengthSecs))
				{
					continue;
				}
				if (community3.IsFEMA || community3.HiddenCommunity || community3.GetAccommodation() == 0 || community3.GetLivingNonZombieMemberCount() < 2 || !community3.IsAnyoneConscious(includeDrunk: true))
				{
					continue;
				}
				bool community1Surrendered;
				CommunityRelationshipType relationship2 = GetRelationship(community3, out community1Surrendered);
				if (!(relationship2 == CommunityRelationshipType.Hostile || relationship2 == CommunityRelationshipType.Allied || community1Surrendered) && !HasOverlappingAllianceWith(community3))
				{
					if (community3.CommunityType == CommunityType.Player && timeSpan >= TimeSpan.FromSeconds(7f * Sun.DayLengthSecs))
					{
						community = community3;
						squadBehaviour = SquadBehaviour.Extortion;
						break;
					}
					ShakedownCommunities.Add(community3);
				}
			}
			if (ShakedownCommunities.Count > 0 && community == null)
			{
				ShakedownCommunities.Sort((Community a, Community b) => (!(a.LastExtortedFromTime < b.LastExtortedFromTime)) ? ((a.LastExtortedFromTime > b.LastExtortedFromTime) ? 1 : ((a.Id >= b.Id) ? ((a.Id > b.Id) ? 1 : 0) : (-1))) : (-1));
				community = ShakedownCommunities[instance.DeterministicRand.Next(Math.Max(1, ShakedownCommunities.Count / 2))];
				squadBehaviour = SquadBehaviour.Extortion;
			}
			ShakedownCommunities.Clear();
		}
		if (community == null && instance.CommunityManager.PlayerCommunity.Buildings.Count > 0 && IsAISettlement() && GetLivingNonZombieMemberCount() >= 2 && CalcCommunityNutritionLevel() < 0.5f)
		{
			CommunityRelationshipType relationship3 = GetRelationship(instance.CommunityManager.PlayerCommunity);
			if (relationship3 != CommunityRelationshipType.Hostile && relationship3 >= CommunityRelationshipType.Known)
			{
				ShakedownCommunities.Add(instance.CommunityManager.PlayerCommunity);
				squadBehaviour = SquadBehaviour.Beg;
			}
		}
		if (community == null)
		{
			return;
		}
		bool flag = InvasionTarget != null && LastInvasionTime.Ticks <= 0 && squadBehaviour == SquadBehaviour.Hunt;
		AvailableEnforcers.Clear();
		AvailableGuards.Clear();
		AvailableOther.Clear();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Character member2 in Members)
		{
			if (!member2.AliveAndNotZombie || member2.GetBaseObjectType() != BaseObjectType.Human || member2.CanFollowPlayer)
			{
				continue;
			}
			bool flag2 = false;
			bool flag3 = false;
			if (member2.GetRank() != Rank.Leader)
			{
				if (member2.HasRole(Role.Enforcer))
				{
					flag2 = true;
				}
				else if (member2.HasRole(Role.Guard))
				{
					flag3 = true;
				}
			}
			if (flag2)
			{
				num++;
			}
			else if (flag3)
			{
				num2++;
			}
			else
			{
				num3++;
			}
			if (member2.SquadId == 0 && member2.SquadLeader == null && (flag || (!(member2.GetThirst() >= Character.ThirstCriticalTime) && !(member2.GetHunger() >= Character.HungerCriticalTime) && !(member2.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusMildHypothermia) && !(member2.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime) && !(member2.GetBloodLoss() > 0f) && (member2.GetRank() != Rank.Leader || squadBehaviour == SquadBehaviour.Beg))))
			{
				if (flag2)
				{
					AvailableEnforcers.Add(member2);
				}
				else if (flag3)
				{
					AvailableGuards.Add(member2);
				}
				else
				{
					AvailableOther.Add(member2);
				}
			}
		}
		int activeMemberCount = community.GetActiveMemberCount();
		int num4 = Math.Min(MathUtil.Clamp(flag ? activeMemberCount : (activeMemberCount / 2), 4, 12), num + num2 + num3);
		bool flag4 = LastInvasionTime.Ticks == 0L || !(instance.PlayTime - LastInvasionTime <= Sun.DayLength);
		if (!flag)
		{
			flag4 &= AvailableEnforcers.Count >= Math.Min(num4, num);
			flag4 &= AvailableEnforcers.Count + AvailableGuards.Count >= Math.Min(num4, num + num2);
			flag4 &= AvailableEnforcers.Count + AvailableGuards.Count + AvailableOther.Count >= num4;
		}
		if (squadBehaviour == SquadBehaviour.Beg)
		{
			flag4 &= AvailableEnforcers.Contains(Leader) || AvailableGuards.Contains(Leader) || AvailableOther.Contains(Leader);
		}
		if (flag4)
		{
			int num5 = 1;
			int num6 = num4;
			int num7 = 0;
			for (int num8 = 0; num8 < num5; num8++)
			{
				Squad squad = AddSquad(squadBehaviour, community.Id);
				for (int num9 = 0; num9 < num6; num9++)
				{
					if (num7 >= num4)
					{
						break;
					}
					if (squadBehaviour == SquadBehaviour.Beg && num7 == 0)
					{
						AvailableEnforcers.Remove(Leader);
						AvailableGuards.Remove(Leader);
						AvailableOther.Remove(Leader);
						AddToSquad(Leader, squad);
						num7++;
						continue;
					}
					List<Character> list = ((AvailableEnforcers.Count > 0) ? AvailableEnforcers : ((AvailableGuards.Count > 0) ? AvailableGuards : AvailableOther));
					if (list.Count == 0)
					{
						break;
					}
					int index = instance.DeterministicRand.Next(list.Count);
					Character member = list[index];
					list.RemoveAt(index);
					AddToSquad(member, squad);
					num7++;
				}
				if (squad.Members.Count > 0 && squadBehaviour == SquadBehaviour.Extortion)
				{
					community.LastExtortedFromTime = instance.PlayTime;
				}
				bool flag5 = false;
				if (squad.Members.Count > 0)
				{
					flag5 = ((squadBehaviour != SquadBehaviour.Hunt) ? (StartStockUpFood(squad) || StartStockUpWater(squad) || StartFindTarget(squad)) : StartPillaging(squad, canAttackOutsideBase: true));
				}
				if (!flag5)
				{
					RemoveSquad(squad);
				}
			}
		}
		AvailableEnforcers.Clear();
		AvailableGuards.Clear();
		AvailableOther.Clear();
	}

	public void UpdateGuardDuty()
	{
		if (CommunityType != CommunityType.Normal && CommunityType != CommunityType.Looter)
		{
			return;
		}
		TempCharacterScores.Clear();
		foreach (Character member in Members)
		{
			bool guardDuty = member.GuardDuty;
			member.GuardDuty = false;
			if (member.HasRole(Role.Guard) && !member.IsInAnySquad() && !(member.SleepDeprivation > Character.SleepDeprivationCriticalTime) && !(member.BodyTemperatureInCelsius <= Character.BodyTemperatureInCelsiusMildHypothermia) && !(member.InfectionProgression > 0f) && !member.HasRole(Role.Organizer))
			{
				float val = 0f;
				val = Math.Max(val, Mathf.Clamp01(member.SleepDeprivation / Character.SleepDeprivationCriticalTime));
				val = Math.Max(val, Mathf.Clamp01((Character.BodyTemperatureInCelsiusNormal - member.BodyTemperatureInCelsius) / (Character.BodyTemperatureInCelsiusNormal - Character.BodyTemperatureInCelsiusMildHypothermia)));
				if (guardDuty)
				{
					float num = 0.5f * Sun.DayLengthSecs / Character.SleepDeprivationCriticalTime;
					val = Math.Max(0f, val - num);
				}
				CharacterScore item = new CharacterScore
				{
					Character = member,
					Score = val
				};
				TempCharacterScores.Add(item);
			}
		}
		int num2 = 0;
		foreach (Prop building2 in Buildings)
		{
			if (!building2.IsGuardPost())
			{
				continue;
			}
			Building building = building2 as Building;
			for (int i = 0; i < building.GetInhabitantSlotDefs().Length; i++)
			{
				if (building.GetInhabitantSlotDefs()[i].External && (building.Inhabitants[i] == null || building.Inhabitants[i].HasRole(Role.Guard)))
				{
					num2++;
				}
			}
		}
		TempCharacterScores.Sort();
		int val2 = Math.Min(TempCharacterScores.Count * 3 / 4, num2);
		for (int j = 0; j < Math.Min(val2, TempCharacterScores.Count); j++)
		{
			TempCharacterScores[j].Character.GuardDuty = true;
		}
		TempCharacterScores.Clear();
	}

	public int GetVoteCountFor(Character character)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.VotedFor == character)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasElectionWinner(out Character winner, out Character runnerUp)
	{
		winner = null;
		runnerUp = null;
		int num = 0;
		int num2 = 0;
		TempCharacterScores.Clear();
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
			{
				CharacterScore item = new CharacterScore
				{
					Character = member,
					Score = GetVoteCountFor(member)
				};
				if (item.Score > 0f)
				{
					TempCharacterScores.Add(item);
				}
				num++;
				if (member.VotedFor != null && member.VotedFor.AliveAndNotZombie)
				{
					num2++;
				}
			}
		}
		TempCharacterScores.Sort();
		int num3 = 0;
		int num4 = 0;
		if (TempCharacterScores.Count > 1)
		{
			runnerUp = TempCharacterScores[TempCharacterScores.Count - 2].Character;
			num4 = (int)TempCharacterScores[TempCharacterScores.Count - 2].Score;
		}
		if (TempCharacterScores.Count > 0)
		{
			winner = TempCharacterScores[TempCharacterScores.Count - 1].Character;
			num3 = (int)TempCharacterScores[TempCharacterScores.Count - 1].Score;
		}
		int num5 = num - num2;
		if (num3 > 0)
		{
			return num3 - num4 > num5;
		}
		return false;
	}

	public void UpdateApproachingInvaders()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		if (GetLivingNonZombieMemberCount() <= 1 || !GameTerrain.Instance.FogOfWar.HasRadio)
		{
			return;
		}
		foreach (Community hunter in communityManager.Hunters)
		{
			TimeSpan timeSpan = (hunter.IsAlwaysHostileToPlayerCommunity() ? TimeSpan.FromMinutes(5.0) : TimeSpan.FromSeconds(Sun.DayLengthSecs * 3f));
			if (!(Session.Instance.PlayTime - hunter.LastWarnedAboutTime >= timeSpan) || hunter.Squads.Count <= 0)
			{
				continue;
			}
			CommunityRelationshipType relationship = communityManager.GetRelationship(this, hunter);
			if ((relationship != CommunityRelationshipType.Hostile || !hunter.IsAlwaysHostileToPlayerCommunity()) && relationship != CommunityRelationshipType.Unknown)
			{
				continue;
			}
			Character leader = hunter.Squads[0].GetLeader();
			if (leader == null)
			{
				continue;
			}
			Character nearestMemberWhoCanNarrate = GetNearestMemberWhoCanNarrate(leader.Tile, MathUtil.Squared(64f));
			if (nearestMemberWhoCanNarrate == null || nearestMemberWhoCanNarrate.InitialCommunity == hunter)
			{
				continue;
			}
			InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(hunter);
			if (invaderInstanceThatCreatedHunter != null)
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(nearestMemberWhoCanNarrate, null, leader, SpeechSituation.InvadersApproaching, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
				if (speechForSituation != null)
				{
					nearestMemberWhoCanNarrate.Speak(speechForSituation, null, leader, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
				}
				hunter.LastWarnedAboutTime = Session.Instance.PlayTime;
			}
		}
	}

	public bool HasAnyBoxersReadyToFight()
	{
		foreach (Character member in Members)
		{
			if (member.IsBoxerReadyToFight())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyoneWithSkill(SkillType skillType, int level)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetSkillLevelWithEffects(skillType) >= level)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyoneWithRole(Role role)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.HasRole(role))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyoneWithInvisibleStrain()
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.InvisibleStrain != InvisibleStrainType.None)
			{
				return true;
			}
		}
		return false;
	}

	public int CountMembersWithRole(Role role)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.HasRole(role))
			{
				num++;
			}
		}
		return num;
	}

	public int CountMembersInRangeOf(TerrainCoord tile, float range)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.Tile.GetDistSquared(tile) <= range * range)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasAnyHumanTraps()
	{
		if (PitTraps.Count > 0)
		{
			return true;
		}
		foreach (Prop building in Buildings)
		{
			if (building.GetBaseObjectType() == BaseObjectType.Tripwire)
			{
				return true;
			}
		}
		return false;
	}

	public Prop GetNearestCraftingPropForRecipe(Recipe recipe, Character character, float bestDistSq, Character checkMovementZone = null, Character checkMovementZone2 = null)
	{
		Prop result = null;
		foreach (Prop building in Buildings)
		{
			if (recipe.IsCraftingPropForRecipe(building))
			{
				float distSquared = building.GetTile().GetDistSquared(character.Tile);
				if (distSquared < bestDistSq && (checkMovementZone == null || !checkMovementZone.HasMovementZone() || checkMovementZone.MovementZone.Overlaps(building.GetTileRect())) && (checkMovementZone2 == null || !checkMovementZone2.HasMovementZone() || checkMovementZone2.MovementZone.Overlaps(building.GetTileRect())) && !IsAnyMemberCraftingWithProp((CraftingProp)building, character))
				{
					bestDistSq = distSquared;
					result = building;
				}
			}
		}
		return result;
	}

	public bool HasMemoryOfHelpingAgainstZombies(Community community, TimeSpan time)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.HasMemoryOfHelpingAgainstZombies(community, time))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasInvisibleStrainExceptPlayer()
	{
		int num = 0;
		int num2 = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
			{
				if (member.InvisibleStrain == InvisibleStrainType.None)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		return num > num2;
	}

	public Character GetFirstBoxerReadyToFight(CustomRandom rand)
	{
		List<Character> list = (Util.AmIOnMainThread() ? _boxers : new List<Character>());
		list.Clear();
		foreach (Character member in Members)
		{
			if (member.IsBoxerReadyToFight())
			{
				list.Add(member);
			}
		}
		list.InsertionSort(BoxerSorter);
		foreach (Character item in list)
		{
			if (!item.KnownBoxer)
			{
				list.Clear();
				return item;
			}
		}
		Character result = ((list.Count > 0) ? list[rand.Next(list.Count)] : null);
		list.Clear();
		list = null;
		return result;
	}

	public int GetBoxerCount()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.Boxer && member.AliveAndNotZombie)
			{
				num++;
			}
		}
		return num;
	}

	public Character GetMemberWithRole(Role role, CustomRandom rand)
	{
		List<Character> list = (Util.AmIOnMainThread() ? _boxers : new List<Character>());
		list.Clear();
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.HasRole(role))
			{
				list.Add(member);
			}
		}
		Character result = ((list.Count > 0) ? list[rand.Next(list.Count)] : null);
		list.Clear();
		list = null;
		return result;
	}

	public Character GetMemberWithEquipmentType(EquipmentPrototype proto)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.Inventory.FindItemOfType(proto) != null)
			{
				return member;
			}
		}
		return null;
	}

	public void SetPlayerGatePermission(CanOpenGates canOpenGates)
	{
		CanOpenPlayerGates = canOpenGates;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				member.ClearFailedFindAttempts();
			}
		}
	}

	public void ForgetPropertyDamage()
	{
		foreach (Character member in Members)
		{
			member.ForgetPropertyDamage();
		}
		PropertyDamageRecords.Clear();
	}

	public void OnCeaseFire(Community otherCommunity)
	{
		foreach (Threat threat in Threats)
		{
			for (int num = threat.ThreatMembers.Count - 1; num >= 0; num--)
			{
				if (threat.ThreatMembers[num].Community == otherCommunity)
				{
					threat.ThreatMembers.RemoveAt(num);
				}
			}
		}
		for (int num2 = Squads.Count - 1; num2 >= 0; num2--)
		{
			Squad squad = Squads[num2];
			if (squad.EnemyCommunityId == otherCommunity.Id)
			{
				if (CommunityType == CommunityType.AmbientLooter || CommunityType == CommunityType.HunterLooter || CommunityType == CommunityType.HunterMercenary)
				{
					StartJourneyToExitMap(squad, scripted: false, avoidHostileBases: true);
				}
				else
				{
					RemoveSquad(squad);
				}
			}
		}
		foreach (Character member in Members)
		{
			if (!member.ConsciousAndNotZombie)
			{
				member.WasAttackedBeforeLastCeaseFire = true;
			}
			if (member.FindActiveGoal(GoalType.Attack) is Attack attack && !attack.IsTargetDeleted() && attack.Target.Object.GetCommunity() == otherCommunity)
			{
				attack.Finished = true;
			}
			foreach (Target target in member.Targets)
			{
				if (target.Object.GetCommunity() == otherCommunity)
				{
					target.LastAttackedMeTime = Target.Never;
					target.LastAttackedUsTime = Target.Never;
					target.LastHeardAttackTime = Target.Never;
				}
				if (target.GetFlag((TargetFlags)80) || target.GetFlag((TargetFlags)48))
				{
					target.SetFlag(TargetFlags.HaveInvestigatedBody, on: true);
					target.SetFlag(TargetFlags.HaveAssignedBlameForAttack, on: true);
				}
			}
		}
		PropertyDamageRecords.Clear();
		if (InvasionTarget == otherCommunity)
		{
			SetInvasionTarget(null, 0f, fromScript: false);
		}
	}

	public bool StartPillaging(Squad squad, bool canAttackOutsideBase, bool isInvader = false)
	{
		TileObject tileObject = FindRandomEnemyBuildingToAttack(squad, canAttackOutsideBase);
		if (tileObject != null)
		{
			SetSquadAction(squad, SquadAction.Pillage, 0, tileObject.GetCentreTile(), null, isInvader);
			return true;
		}
		return false;
	}

	private Character GetBestCommunityMemberToTalkToForDiplomacy(Squad squad)
	{
		if (!(BaseObjectManager.Instance.FindBaseObjectByID(squad.EnemyCommunityId) is Community community))
		{
			return null;
		}
		Character result = null;
		float num = float.MaxValue;
		if (squad.Behaviour == SquadBehaviour.RequestAlliance)
		{
			if (community.Leader != null && community.Leader.AliveAndNotZombie && community.Leader.Consciousness < Consciousness.Unconscious)
			{
				result = community.Leader;
			}
		}
		else
		{
			foreach (Character member in community.Members)
			{
				if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.Consciousness < Consciousness.Unconscious)
				{
					float distSquared = member.Tile.GetDistSquared(squad.GetLeader().Tile);
					if (distSquared < num && community.IsCloseToBase(member.Tile, 32f))
					{
						result = member;
						num = distSquared;
					}
				}
			}
		}
		return result;
	}

	public bool StartFindTarget(Squad squad)
	{
		Character bestCommunityMemberToTalkToForDiplomacy = GetBestCommunityMemberToTalkToForDiplomacy(squad);
		if (bestCommunityMemberToTalkToForDiplomacy == null)
		{
			return false;
		}
		SetSquadAction(squad, SquadAction.FindTarget, 0, bestCommunityMemberToTalkToForDiplomacy.Tile, bestCommunityMemberToTalkToForDiplomacy);
		return true;
	}

	public bool StartStockUpFood(Squad squad)
	{
		Character bestCommunityMemberToTalkToForDiplomacy = GetBestCommunityMemberToTalkToForDiplomacy(squad);
		if (bestCommunityMemberToTalkToForDiplomacy == null)
		{
			return false;
		}
		float dist = bestCommunityMemberToTalkToForDiplomacy.Tile.GetDist(squad.GetLeader().Tile);
		float num = (float)squad.Members.Count * (2f * (dist / Character.WalkSpeed) + Sun.DayLengthSecs);
		float num2 = 0f;
		foreach (Character member in squad.Members)
		{
			num2 += member.Inventory.GetTotalNutritionThatImAllowedToEat(member);
		}
		if (num2 < num)
		{
			SetSquadAction(squad, SquadAction.StockUpFood, 0, TerrainCoord.Invalid, null);
			return true;
		}
		return false;
	}

	public bool StartStockUpWater(Squad squad)
	{
		bool flag = false;
		foreach (Character member in squad.Members)
		{
			flag |= member.GetThirst() >= Character.ThirstyTime;
			Equipment bestWaterBottle = member.Inventory.GetBestWaterBottle();
			flag |= bestWaterBottle == null || bestWaterBottle.GetLiquidContentsAmount() < bestWaterBottle.GetLiquidCapacity();
		}
		if (flag)
		{
			SetSquadAction(squad, SquadAction.StockUpWater, 0, TerrainCoord.Invalid, null);
			return true;
		}
		return false;
	}

	public void StartJourneyToExitMap(Squad squad)
	{
		StartJourneyToExitMap(squad, scripted: false, avoidHostileBases: false);
	}

	public void StartJourneyToExitMap(Squad squad, bool scripted, bool avoidHostileBases, bool highPrio = false)
	{
		if (!scripted && !IsHunterCommunity())
		{
			RemoveSquad(squad);
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		squad.Behaviour = SquadBehaviour.Travel;
		squad.AvoidHostileBases = avoidHostileBases;
		ControlPoint? controlPoint = CommunityManager.PickRandomMapEntrance(Session.Instance.DeterministicRand);
		if (controlPoint.HasValue)
		{
			TerrainCoord tileCoordForPosXZ = instance.GetTileCoordForPosXZ(controlPoint.Value.Pos);
			tileCoordForPosXZ = instance.ClampTileWithinBounds(tileCoordForPosXZ);
			squad.GoalTile = tileCoordForPosXZ;
		}
		else
		{
			squad.GoalTile = Session.Instance.DeterministicRand.RandomTile(new TerrainCoord(16, 16), new TerrainCoord(instance.Size - 16, instance.Size - 16));
		}
		SetSquadAction(squad, highPrio ? SquadAction.GoToHighPrio : SquadAction.GoTo, 0, squad.GoalTile, null);
	}

	public void SetInvasionTarget(Community newInvasionTarget, float days, bool fromScript)
	{
		if (InvasionTarget != null)
		{
			Squad squad = FindHunterSquad();
			if (squad != null && !squad.EverybodyFleeing)
			{
				RemoveSquad(squad);
			}
		}
		if (newInvasionTarget != InvasionTarget)
		{
			LastInvasionTime = Target.Never;
		}
		InvasionTarget = newInvasionTarget;
		InvasionTimeoutTime = ((newInvasionTarget != null) ? (Session.Instance.PlayTime + TimeSpan.FromSeconds(Sun.DayLengthSecs * days)) : Target.Never);
		InvasionTargetWasSetFromScript = fromScript;
	}

	public void GoHome(Squad squad)
	{
		if (GetBaseCentre(out var centre, out var _))
		{
			SetSquadAction(squad, (squad.Behaviour == SquadBehaviour.Ambush) ? SquadAction.GoTo : SquadAction.GoToHighPrio, 0, centre, null);
		}
		else
		{
			RemoveSquad(squad);
		}
	}

	public bool GoToNextTradeDestination(Squad squad, CustomRandom rand, bool mustMove, bool canOccupyBases, bool isInvader = false)
	{
		GameTerrain instance = GameTerrain.Instance;
		Session instance2 = Session.Instance;
		List<Community> list = new List<Community>();
		List<Community> list2 = new List<Community>();
		foreach (Community community3 in instance2.CommunityManager.Communities)
		{
			if ((community3.IsAISettlement() || community3.CommunityType == CommunityType.Player) && !community3.HiddenCommunity && community3.GetAccommodation() > 0 && community3.HasAnyActiveMembers() && GetRelationship(community3) != CommunityRelationshipType.Hostile)
			{
				if (mustMove && (community3.CommunityType == CommunityType.Player || (squad.Members.Count > 0 && community3.GetDistSqToNearestBuilding(squad.Members[0].Tile) <= (float)MathUtil.Squared(32))))
				{
					continue;
				}
				list.Add(community3);
			}
			if (canOccupyBases)
			{
				if (community3.CanBeOccupiedBy(this))
				{
					list2.Add(community3);
				}
				else if (IsTemporaryCommunity() && Members.Count > 0 && community3 == Members[0].InitialCommunity && community3.BaseRect.VerticesArea > 0 && community3.IsAISettlement() && community3.HasAnyActiveMembers())
				{
					list2.Add(community3);
				}
			}
		}
		if (list2.Count > 0)
		{
			Community community = list2[rand.Next(list2.Count)];
			squad.Behaviour = SquadBehaviour.Occupy;
			squad.EnemyCommunityId = community.Id;
			squad.GoalTile = rand.RandomTile(community.BaseRect.min, community.BaseRect.max);
			SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null, isInvader);
			return true;
		}
		if (list.Count > 0)
		{
			Community community2 = list[rand.Next() % list.Count];
			List<Building> list3 = new List<Building>();
			foreach (Prop building2 in community2.Buildings)
			{
				if (building2.IsAccommodation())
				{
					list3.Add((Building)building2);
				}
			}
			TerrainCoord centre;
			TerrainCoord terrainCoord;
			if (list3.Count > 0)
			{
				Building building = list3[rand.Next() % list3.Count];
				centre = instance.GetTileCoordForPos(building.GetEntrancePos(0));
				Vector2 dirFromAngle = MathUtil.GetDirFromAngle(building.GetEntranceAngle(0));
				terrainCoord = new TerrainCoord((dirFromAngle.x > 0f) ? 1 : ((dirFromAngle.x < 0f) ? (-1) : 0), (dirFromAngle.y > 0f) ? 1 : ((dirFromAngle.y < 0f) ? (-1) : 0));
			}
			else
			{
				if (!community2.GetBaseCentre(out centre, out var _))
				{
					squad.GoalTile = Session.Instance.DeterministicRand.RandomTile(new TerrainCoord(16, 16), new TerrainCoord(instance.Size - 16, instance.Size - 16));
					SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null, isInvader);
					return false;
				}
				terrainCoord = Prop.GetDirFromOrientationType(rand.RandomOrientationType());
			}
			int i;
			for (i = 1; i < 16 && !instance.IsImpassable(centre.x + terrainCoord.x * i, centre.y + terrainCoord.y * i, 0, squad.GetLeader(), null); i++)
			{
			}
			centre += terrainCoord * (i / 2);
			if (CanOpenPlayerGates == CanOpenGates.No)
			{
				int num = 1;
				TerrainCoord terrainCoord2 = centre;
				while (instance.IsTileOutsideBounds(centre.x, centre.y) || instance.IsTileEnclosed(centre.x, centre.y) || instance.IsImpassable(centre.x, centre.y, 0, squad.GetLeader(), null))
				{
					num++;
					centre = rand.RandomTileOnEdge(terrainCoord2 - new TerrainCoord(num, num), terrainCoord2 + new TerrainCoord(num, num));
					if (num >= instance.Size)
					{
						SetSquadAction(squad, SquadAction.Wait, 0, TerrainCoord.Invalid, null, isInvader);
						return false;
					}
				}
			}
			squad.GoalTile = instance.ClampTileWithinBounds(centre);
			SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null, isInvader);
			return true;
		}
		squad.GoalTile = Session.Instance.DeterministicRand.RandomTile(new TerrainCoord(16, 16), new TerrainCoord(instance.Size - 16, instance.Size - 16));
		SetSquadAction(squad, SquadAction.GoTo, 0, squad.GoalTile, null, isInvader);
		return false;
	}

	public TileObject FindRandomEnemyBuildingToAttack(Squad squad, bool canAttackOutsideBase)
	{
		if (!(BaseObjectManager.Instance.FindBaseObjectByID(squad.EnemyCommunityId) is Community community))
		{
			return null;
		}
		if (CommunityType == CommunityType.Psycho)
		{
			Character leader = squad.GetLeader();
			if (leader != null)
			{
				Character character = null;
				float num = float.MaxValue;
				foreach (Character member in community.Members)
				{
					if (CanPillage(member))
					{
						float sqrMagnitude = (member.PosXZ - leader.PosXZ).sqrMagnitude;
						if (sqrMagnitude < num)
						{
							character = member;
							num = sqrMagnitude;
						}
					}
				}
				if (character != null)
				{
					return character;
				}
			}
		}
		bool flag = squad.HasAnyEquipmentOfClass(typeof(RPG)) || squad.HasAnyEquipmentOfClass(typeof(PipeBomb));
		bool flag2 = squad.HasAnyEquipmentOfClass(typeof(MolotovCocktail));
		foreach (Prop building in community.Buildings)
		{
			if (((flag && !building.IsExplosionProof()) || (flag2 && building.IsFlammable())) && CanPillage(building))
			{
				_potentialTargets.Add(building);
			}
		}
		if (_potentialTargets.Count == 0)
		{
			foreach (Character member2 in community.Members)
			{
				if (CanPillage(member2) && member2.InsideBuilding != null && member2.InsideBuilding.Community == community)
				{
					_potentialTargets.Add(member2);
				}
			}
		}
		if (_potentialTargets.Count == 0)
		{
			foreach (Character member3 in community.Members)
			{
				if (CanPillage(member3) && community.GetDistSqToNearestBuilding(member3.Tile) <= 225f)
				{
					_potentialTargets.Add(member3);
				}
			}
		}
		if (_potentialTargets.Count == 0 && canAttackOutsideBase)
		{
			foreach (Character member4 in community.Members)
			{
				if (CanPillage(member4))
				{
					_potentialTargets.Add(member4);
				}
			}
		}
		TileObject result = ((_potentialTargets.Count > 0) ? _potentialTargets[Session.Instance.DeterministicRand.Next(_potentialTargets.Count)] : null);
		_potentialTargets.Clear();
		return result;
	}

	public void Update()
	{
		switch (CommunityType)
		{
		case CommunityType.Player:
			UpdatePlayer();
			break;
		default:
			UpdateAI();
			break;
		case CommunityType.AmbientZombie:
		case CommunityType.AmbientAnimal:
			break;
		}
	}

	private void UpdatePlayer()
	{
		Session instance = Session.Instance;
		TimeSpan playTime = Session.Instance.PlayTime;
		TimeSpan timeSpan = playTime - LastAIUpdateTime;
		if (!IsAnyoneConscious(includeDrunk: false) && GetLivingNonZombieMemberCount() > 0 && !IsAnyMemberRagdolled() && instance.CountdownToTimeJump == 0)
		{
			if (IsAnyoneHealable())
			{
				instance.CountdownToTimeJump = 60;
			}
			else
			{
				StoryEvent storyEvent = new StoryEvent
				{
					Type = StoryEventType.GameOver,
					Delay = 5f
				};
				StoryManager.Instance.QueuedEvents.Add(QueuedEvent.Create(storyEvent, null, null, null, default(MemoryParam), Session.Instance.PlayTime));
			}
		}
		if (!(timeSpan < TimeBetweenAIUpdates))
		{
			LastAIUpdateTime = playTime;
			UpdateGuardDuty();
			UpdateApproachingInvaders();
		}
	}

	private void UpdateAI()
	{
		TimeSpan playTime = Session.Instance.PlayTime;
		if (!(playTime - LastAIUpdateTime < TimeBetweenAIUpdates))
		{
			LastAIUpdateTime = playTime;
			UpdateThreats();
			UpdateSquads();
			UpdateInvasionTarget();
			UpdateGuardDuty();
			UpdateFunerals();
			UpdateRoles(null);
			UpdateKicking();
			UpdateEncouraging();
			if (IsTemporaryCommunity() && !KeptAroundForReferences && !HasAnyActiveMembers() && playTime - GetTimeWhenLastMemberDied() >= TimeSpan.FromSeconds(600.0) && !AreAnyMembersInMainView() && !HasAnyNamedZombifiedMembers() && !IsReferencedByAnyActiveQuest())
			{
				DeleteMe = true;
			}
		}
	}

	private bool HasAnyNamedZombifiedMembers()
	{
		foreach (Character member in Members)
		{
			if (member.NameKnown && member.Zombie && member.Alive)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsReferencedByAnyActiveQuest()
	{
		foreach (QuestInstance activeQuest in StoryManager.Instance.ActiveQuests)
		{
			if (activeQuest.HasReferenceTo(this))
			{
				return true;
			}
			foreach (Character member in Members)
			{
				if (activeQuest.HasReferenceTo(member))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CalcDesiredNumberOfMembersAssignedToRoles(int[] results, bool canAddTraders)
	{
		_ = GameTerrain.Instance;
		int num = 0;
		int num2 = 0;
		bool flag = false;
		foreach (Prop building in Buildings)
		{
			if (building.IsGuardPost())
			{
				num2++;
			}
			if (building is Campfire && IsTileInsidePerimeter(building.Tile))
			{
				num++;
			}
		}
		foreach (TileObject underConstructionBuilding in UnderConstructionBuildings)
		{
			if (!(underConstructionBuilding is Grave))
			{
				flag = true;
			}
		}
		for (int i = 0; i < ConstructionRecords.Count; i++)
		{
			if (ConstructionRecords[i].CanRebuildHere(this))
			{
				flag = true;
				break;
			}
		}
		Array.Clear(results, 0, results.Length);
		int livingNonZombieMemberCount = GetLivingNonZombieMemberCount();
		int livingNonZombieMemberCountBySpecies = GetLivingNonZombieMemberCountBySpecies(BaseObjectType.Chicken);
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.HasRole(Role.NoRole))
			{
				if (results[8] < Math.Max(1, num / 2) && livingNonZombieMemberCount > 1)
				{
					results[8]++;
				}
				else if (results[1] < 1 + livingNonZombieMemberCount / 8)
				{
					results[1]++;
				}
				else if (results[4] == 0 && flag)
				{
					results[4]++;
				}
				else if (results[5] == 0 && results[4] == 0 && NeedsRepair.Count > 0)
				{
					results[5]++;
				}
				else if (results[9] < num)
				{
					results[9]++;
				}
				else if (results[3] == 0)
				{
					results[3]++;
				}
				else if (results[14] < 1 && livingNonZombieMemberCountBySpecies > 0)
				{
					results[14]++;
				}
				else if (results[10] < 1 && (canAddTraders || CountMembersWithRole(Role.Trader) > 0))
				{
					results[10]++;
				}
				else if (results[3] < num2 * 2)
				{
					results[3]++;
				}
				else if (CommunityType == CommunityType.Looter && results[15] < livingNonZombieMemberCount / 2)
				{
					results[15]++;
				}
			}
		}
	}

	private bool HasAssignedBlameOrInvestigatedDeath(Character member)
	{
		foreach (Character member2 in Members)
		{
			if (member2 != member && member2.AliveAndNotZombie)
			{
				Target target = member2.GetTarget(member);
				if (target != null && target.HasAnyFlag((TargetFlags)384))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool CanUpdateRoles()
	{
		if (HiddenCommunity)
		{
			return false;
		}
		foreach (Character member in Members)
		{
			if (member.InCombat || member.IsLowAlert())
			{
				return false;
			}
			if (!member.Alive && !member.Zombie && !member.Disappeared && Session.Instance.PlayTime - member.TimeOfDeath <= TimeSpan.FromSeconds(120.0) && !HasAssignedBlameOrInvestigatedDeath(member))
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateRoles(Character justSpawnedNewMember)
	{
		if (CommunityType != CommunityType.Normal && CommunityType != CommunityType.Looter)
		{
			return;
		}
		if (GetCraftingLimit(EquipmentPrototype.Wood) == int.MaxValue)
		{
			int numCompletedBuildingsOfType = GetNumCompletedBuildingsOfType(BaseObjectType.Campfire);
			SetCraftingLimit(EquipmentPrototype.Wood, numCompletedBuildingsOfType * GameTerrain.WoodLimitPerCampfire);
		}
		if (justSpawnedNewMember == null && !CanUpdateRoles())
		{
			return;
		}
		CalcDesiredNumberOfMembersAssignedToRoles(DesiredNumRoles, justSpawnedNewMember != null);
		for (int i = 0; i < CurrentMembersInRoles.Length; i++)
		{
			if (CurrentMembersInRoles[i] == null)
			{
				CurrentMembersInRoles[i] = new List<Character>();
			}
			CurrentMembersInRoles[i].Clear();
		}
		foreach (Character member in Members)
		{
			if (!member.AliveAndNotZombie || member.GetBaseObjectType() != BaseObjectType.Human || member.Rank == Rank.Captive)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < member.Roles.Count; j++)
			{
				Role role = member.Roles[j].Role;
				if (role == Role.Crafter && member.Roles[j].Recipe != null && member.Roles[j].Recipe.ProductPropPrototype != null)
				{
					role = Role.Builder;
				}
				CurrentMembersInRoles[(int)role].Add(member);
				flag = true;
			}
			if (!flag)
			{
				CurrentMembersInRoles[0].Add(member);
			}
		}
		for (int k = 1; k < CurrentMembersInRoles.Length; k++)
		{
			Role role2 = (Role)k;
			if (role2 == Role.Trader || role2 == Role.Organizer || role2 == Role.NoRole)
			{
				continue;
			}
			while (CurrentMembersInRoles[k].Count > DesiredNumRoles[k])
			{
				int index = CurrentMembersInRoles[k].Count - 1;
				Character character = CurrentMembersInRoles[k][index];
				character.CancelRole(new RoleInfo(role2));
				CurrentMembersInRoles[k].RemoveAt(index);
				if (character.Roles.Count == 0)
				{
					CurrentMembersInRoles[0].Add(character);
				}
			}
		}
		int numGuardPosts = GetNumGuardPosts();
		for (int l = 1; l < CurrentMembersInRoles.Length; l++)
		{
			Role role3 = (Role)l;
			if (role3 == Role.Trader)
			{
				if (justSpawnedNewMember == null || justSpawnedNewMember.Roles.Count != 0)
				{
					continue;
				}
				CurrentMembersInRoles[0].Remove(justSpawnedNewMember);
				CurrentMembersInRoles[0].Add(justSpawnedNewMember);
			}
			while (CurrentMembersInRoles[l].Count < DesiredNumRoles[l] && CurrentMembersInRoles[0].Count > 0)
			{
				List<Character> list = CurrentMembersInRoles[0];
				if (justSpawnedNewMember == null)
				{
					switch (role3)
					{
					case Role.Farmer:
						list.Sort(Skillset.SortCharactersByFarmingSkillAscending);
						break;
					case Role.Cook:
						list.Sort(Skillset.SortCharactersByCookingSkillAscending);
						break;
					case Role.Builder:
						list.Sort(Skillset.SortCharactersByBuildingSkillAscending);
						break;
					case Role.Lumberjack:
						if (list.Count > 1 && list[list.Count - 1].Rank == Rank.Leader)
						{
							Character value = list[list.Count - 1];
							list[list.Count - 1] = list[list.Count - 2];
							list[list.Count - 2] = value;
						}
						break;
					}
				}
				int index2 = list.Count - 1;
				Character character2 = list[index2];
				RoleInfo roleInfo = new RoleInfo(role3);
				switch (role3)
				{
				case Role.Farmer:
					if (justSpawnedNewMember == null)
					{
						character2.Skillset.EnsureMinLevel(character2, SkillType.Farming, Skillset.MinAIFarmerSkill);
					}
					break;
				case Role.Cook:
					if (justSpawnedNewMember == null)
					{
						character2.Skillset.EnsureMinLevel(character2, SkillType.Cooking, Skillset.MinAICookingSkill);
					}
					break;
				}
				CurrentMembersInRoles[0].RemoveAt(index2);
				if (role3 != Role.Guard || character2.Rank != Rank.Leader || CurrentMembersInRoles[l].Count + CurrentMembersInRoles[0].Count < numGuardPosts)
				{
					CurrentMembersInRoles[l].Add(character2);
					character2.AddRole(roleInfo);
				}
			}
			if (role3 == Role.Builder && DesiredNumRoles[l] > 0 && CurrentMembersInRoles[l].Count == 0)
			{
				Character character3 = null;
				int num = -1;
				foreach (Character member2 in Members)
				{
					if (member2.AliveAndNotZombie && member2.GetBaseObjectType() == BaseObjectType.Human && !member2.HasRole(Role.Trader) && !member2.HasRole(Role.Organizer) && !member2.HasRole(Role.NoRole))
					{
						int level = member2.Skillset.GetLevel(SkillType.Construction);
						if (level > num)
						{
							character3 = member2;
							num = level;
						}
					}
				}
				if (character3 != null)
				{
					while (character3.Roles.Count > 0)
					{
						Role role4 = character3.Roles[0].Role;
						character3.CancelRole(character3.Roles[0]);
						CurrentMembersInRoles[(int)role4].Remove(character3);
					}
					CurrentMembersInRoles[4].Add(character3);
					character3.AddRole(new RoleInfo(Role.Builder));
				}
			}
			if (role3 != Role.Builder || CurrentMembersInRoles[l].Count <= 0)
			{
				continue;
			}
			Character character4 = CurrentMembersInRoles[l][0];
			BuildGoal buildGoal = character4.GetBuildGoal();
			CraftGoal craftGoal = character4.GetCraftGoal();
			if (buildGoal == null || craftGoal == null || character4.Consciousness >= Consciousness.Unconscious || character4.SquadId != 0 || character4.IsTooDepressedToFollowOrders() || buildGoal.CurrentBuilding != null || craftGoal.FollowingRecipe != null)
			{
				continue;
			}
			int num2 = FindConstructionRecordToStartBuilding(character4);
			if (num2 != -1)
			{
				PropPrototype typeToBuild = ConstructionRecords[num2].GetTypeToBuild();
				Recipe recipe = GameImpl.Instance.FindRecipeByProduct(typeToBuild, null);
				character4.Skillset.EnsureMinLevel(character4, recipe.SkillType, Math.Min(recipe.SkillLevel, 5));
				if (recipe.RecipeType == RecipeType.Normal)
				{
					craftGoal.StartRecipe(character4, recipe, null, 1, ConstructionRecords[num2].Tile, ConstructionRecords[num2].Orientation, wasTriggeredFromDirectControl: false, urgent: false);
				}
				else
				{
					TileObject tileObject = Session.Instance.PlaceBuildingDuringGameplay(character4, recipe, ConstructionRecords[num2].Tile, ConstructionRecords[num2].Orientation);
					if (tileObject != null)
					{
						buildGoal.SetCurrentBuilding(character4, character4.GetGoal(), tileObject);
					}
				}
				ConstructionRecords.RemoveAt(num2);
			}
			else
			{
				if (UnderConstructionBuildings.Count <= 0 || UnderConstructionBuildings[0].GetUnderConstructionInfo() == null || UnderConstructionBuildings[0].GetUnderConstructionInfo().Recipe == null)
				{
					break;
				}
				Recipe recipe2 = UnderConstructionBuildings[0].GetUnderConstructionInfo().Recipe;
				character4.Skillset.EnsureMinLevel(character4, recipe2.SkillType, Math.Min(recipe2.SkillLevel, 5));
			}
		}
		for (int m = 0; m < CurrentMembersInRoles.Length; m++)
		{
			CurrentMembersInRoles[m].Clear();
		}
	}

	private int FindConstructionRecordToStartBuilding(Character member)
	{
		_ = GameTerrain.Instance;
		int result = -1;
		float num = 0f;
		for (int i = 0; i < ConstructionRecords.Count; i++)
		{
			if (!ConstructionRecords[i].CanRebuildHere(this))
			{
				continue;
			}
			PropPrototype typeToBuild = ConstructionRecords[i].GetTypeToBuild();
			if (GameImpl.Instance.FindRecipeByProduct(typeToBuild, null) != null)
			{
				Prop prop = ((typeToBuild != null) ? (typeToBuild.ProtoInstance as Prop) : null);
				float num2 = ((prop != null) ? 100f : 10f);
				if (prop != null && prop.IsAccommodation())
				{
					num2 += 10f;
				}
				if (num2 > num)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	public void UpdateEncouraging()
	{
		if (Leader == null || !Leader.AliveAndNotZombie || Leader.GetLeaderCommand() != null || Leader.DontSimulateSurvivalFactorsUntilDiscovered || Leader.DontSimulateSurvivalFactorsUntilJoinCommunity || (!Leader.IsBored() && Leader.FindActiveGoal(GoalType.GuardGoal) == null))
		{
			return;
		}
		Temp.Clear();
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.Consciousness < Consciousness.Unconscious && member.Rank != Rank.Captive && member.IsTooDepressedToFollowOrders())
			{
				Temp.Add(member);
			}
		}
		if (Temp.Count > 0)
		{
			Character target = Temp[Session.Instance.DeterministicRand.Next() % Temp.Count];
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(Leader, target, SpeechSituation.Encourage);
			if (speechForSituation != null)
			{
				Leader.SetLeaderCommand(new Conversation(Leader, target, null, speechForSituation, controlledByPlayer: false), target, ObeyLeaderGoal.SourceType.SquadLeader);
			}
		}
	}

	public bool IsCommunityBusy()
	{
		if (Threats.Count > 0)
		{
			return true;
		}
		foreach (Squad squad in Squads)
		{
			if (squad.Behaviour == SquadBehaviour.Funeral || squad.Behaviour == SquadBehaviour.Hunt || squad.Behaviour == SquadBehaviour.Defend || squad.Action == SquadAction.Bury)
			{
				return true;
			}
		}
		return false;
	}

	private bool CanStartBurying(Character character)
	{
		if (character.Alive)
		{
			return false;
		}
		if (character.Disappeared)
		{
			return false;
		}
		if (character.EulogyGiven)
		{
			return false;
		}
		if (character.DontBury)
		{
			return false;
		}
		if (character.IsAmbient())
		{
			return false;
		}
		if (character.IsZombifying())
		{
			return false;
		}
		if (character.CarriedBy != null)
		{
			return false;
		}
		if (character.GetBaseObjectType() != BaseObjectType.Human)
		{
			return false;
		}
		if (HasFailedToBury(character))
		{
			return false;
		}
		if (Session.Instance.PlayTime - character.TimeOfDeath < Sun.DayLength && !IsCharacterVisibleToAnyMember(character, out var _, notIncludingCaptives: true))
		{
			return false;
		}
		TerrainCoord tile = character.Tile;
		if (!IsTileInsidePerimeter(tile))
		{
			GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(tile - new TerrainCoord(64, 64), tile + new TerrainCoord(64, 64), NearbyCharacters2);
			foreach (Character item in NearbyCharacters2)
			{
				if (item.Consciousness < Consciousness.Unconscious && item.GetBaseObjectType() == BaseObjectType.Human && item.Infection != InfectionType.Green && item.Infection != InfectionType.Blue && GetRelationship(item.Community) == CommunityRelationshipType.Hostile)
				{
					return false;
				}
			}
			NearbyCharacters2.Clear();
		}
		return true;
	}

	public void UpdateFunerals()
	{
		if (IsCommunityBusy() || IsZombieCommunity() || IsAnimalCommunity())
		{
			return;
		}
		Temp.Clear();
		foreach (Character member in Members)
		{
			if (CanStartBurying(member))
			{
				Temp.Add(member);
			}
			if (member.AliveAndNotZombie && (member.InCombat || member.IsLowAlert() || member.UnderAttackRefCount > 0))
			{
				return;
			}
		}
		if (Temp.Count == 0 && BaseRect != TerrainRect.Invalid)
		{
			GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(BaseRect.min - new TerrainCoord(32, 32), BaseRect.max + new TerrainCoord(32, 32), NearbyCharacters);
			foreach (Character nearbyCharacter in NearbyCharacters)
			{
				if (CanStartBurying(nearbyCharacter))
				{
					Temp.Add(nearbyCharacter);
				}
			}
			NearbyCharacters.Clear();
		}
		if (Temp.Count > 0)
		{
			Character character = Temp[Session.Instance.DeterministicRand.Next(Temp.Count)];
			TempCharacterScores.Clear();
			foreach (Character member2 in Members)
			{
				if (!member2.AliveAndNotZombie || member2.GetBaseObjectType() != BaseObjectType.Human || member2.SquadId != 0 || member2.CanFollowPlayer || member2.SquadLeader != null || member2.IsFleeing() || member2.Consciousness >= Consciousness.Unconscious)
				{
					continue;
				}
				if (member2.GetHunger() >= Character.HungerCriticalTime || member2.GetThirst() >= Character.ThirstCriticalTime || member2.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime || member2.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusMildHypothermia || member2.DontSimulateSurvivalFactorsUntilJoinCommunity || member2.DontSimulateSurvivalFactorsUntilDiscovered || member2.HasUnbandagedInjury(0) || member2.GetInfectionProgression() >= Character.InfectionProgressionCriticalLevel || member2.HasInjuryWithInfectionType(InfectionType.Red))
				{
					if (IsHunterCommunity() || IsTemporaryCommunity())
					{
						return;
					}
					continue;
				}
				float num = (member2.PosXZ - character.PosXZ).magnitude;
				if (member2.Inventory.GetShovel() != null)
				{
					num -= 50f;
				}
				member2.CalcApprovalRating(character, out var approval, out var _);
				num -= approval;
				CharacterScore item = new CharacterScore
				{
					Character = member2,
					Score = num
				};
				TempCharacterScores.Add(item);
			}
			if (TempCharacterScores.Count > 0)
			{
				TempCharacterScores.Sort();
				Squad squad = (((IsHunterCommunity() || IsTemporaryCommunity()) && Squads.Count > 0) ? Squads[0] : AddSquad(SquadBehaviour.Funeral, 0));
				for (int i = 0; i < Math.Min(TempCharacterScores.Count, 4); i++)
				{
					AddToSquad(TempCharacterScores[i].Character, squad);
				}
				SetSquadAction(squad, SquadAction.Bury, 0, squad.GoalTile, character);
				TempCharacterScores.Clear();
			}
		}
		Temp.Clear();
	}

	public bool HasFailedToBury(Character corpse)
	{
		for (int i = 0; i < FailedToBury.Count; i++)
		{
			if (FailedToBury[i].Corpse == corpse)
			{
				return Session.Instance.PlayTime - FailedToBury[i].FailedTime <= TimeSpan.FromSeconds(Math.Min((float)MathUtil.Squared(FailedToBury[i].Attempts) * 60f, Sun.DayLengthSecs));
			}
		}
		return false;
	}

	public void AddFailedToBury(Character corpse)
	{
		if (corpse == null || corpse.Deleted)
		{
			return;
		}
		FailedToBuryRecord failedToBuryRecord = new FailedToBuryRecord
		{
			Corpse = corpse,
			FailedTime = Session.Instance.PlayTime,
			Attempts = 1
		};
		for (int i = 0; i < FailedToBury.Count; i++)
		{
			if (FailedToBury[i].Corpse == corpse)
			{
				failedToBuryRecord.Attempts += FailedToBury[i].Attempts;
				FailedToBury[i] = failedToBuryRecord;
				return;
			}
		}
		FailedToBury.Add(failedToBuryRecord);
	}

	public void RemoveFailedToBury(Character corpse)
	{
		for (int i = 0; i < FailedToBury.Count; i++)
		{
			if (FailedToBury[i].Corpse == corpse)
			{
				FailedToBury.RemoveAt(i);
				break;
			}
		}
	}

	public bool IsActionAllowedForItem(Equipment item, EquipmentPolicyAction action)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(item))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
		}
		return (EquipmentPolicy.GetDefaultActionsMask(item.GetPrototype(), item.GetLiquidContentsType()) & (1 << (int)action)) != 0;
	}

	public bool IsActionAllowedForItemIgnoringInfection(EquipmentPrototype proto, LiquidPrototype liquid, EquipmentPolicyAction action)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].MatchesIgnoringInfection(proto, liquid))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
		}
		return (EquipmentPolicy.GetDefaultActionsMask(proto, liquid) & (1 << (int)action)) != 0;
	}

	public bool IsActionAllowedForItem(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, EquipmentPolicyAction action)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				return EquipmentPolicies[i].GetPolicy(action);
			}
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
		return false;
	}

	public float GetTargetAmountToCarry(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				return EquipmentPolicies[i].TargetAmount;
			}
		}
		if (proto == null)
		{
			return 0f;
		}
		return proto.DefaultCarryAmount;
	}

	public int GetEquipmentPolicyMask(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				return EquipmentPolicies[i].GetPolicyMask();
			}
		}
		return EquipmentPolicy.GetDefaultActionsMask(proto, liquid);
	}

	public void SetEquipmentPolicy(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType, int policyMask, float targetAmount)
	{
		for (int i = 0; i < EquipmentPolicies.Count; i++)
		{
			if (EquipmentPolicies[i].Matches(proto, liquid, infectionType))
			{
				EquipmentPolicy value = EquipmentPolicies[i];
				value.SetPolicyMask(policyMask, targetAmount);
				if (value.AreAllActionsSetToDefault())
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
		EquipmentPolicy item = new EquipmentPolicy(proto, liquid, infectionType);
		item.SetPolicyMask(policyMask, targetAmount);
		if (!item.AreAllActionsSetToDefault())
		{
			EquipmentPolicies.Add(item);
			OnEquipmentPolicyChanged();
		}
	}

	public void OnEquipmentPolicyChanged()
	{
		foreach (Character member in Members)
		{
			member.OnEquipmentPolicyChanged();
		}
	}

	public void UpdateKicking()
	{
		if (Leader == null || !Leader.AliveAndNotZombie || Leader.GetLeaderCommand() != null || Leader.DontSimulateSurvivalFactorsUntilDiscovered || Leader.DontSimulateSurvivalFactorsUntilJoinCommunity || Leader.FindActiveGoal(GoalType.Conversation) != null)
		{
			return;
		}
		foreach (Character member in Members)
		{
			if (member != Leader && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && !member.InCombat && member.SparringPartner == null && member.IsAwake && Leader.WantKickFromCommunity(member, checkRelationships: true) && !member.IsLowAlert() && member.FindActiveGoal(GoalType.Conversation) == null)
			{
				BaseObject obj = null;
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(Leader, member, ref obj, SpeechSituation.KickFromCommunity, default(MemoryParam));
				if (speechForSituation != null)
				{
					Leader.SetLeaderCommand(new Conversation(Leader, member, obj, speechForSituation, controlledByPlayer: false), member, ObeyLeaderGoal.SourceType.SpeakTo);
				}
				else
				{
					Debug.LogError("No KickFromCommunity speech found! " + Leader.GetDisplayNameString() + " -> " + member.GetDisplayNameString());
				}
				break;
			}
		}
	}

	public float GetHarvestedNutritionAmount()
	{
		if (CachedHarvestedNutritionAmount != -1f)
		{
			return CachedHarvestedNutritionAmount;
		}
		float num = 0f;
		foreach (Prop building in Buildings)
		{
			for (int i = 0; i < building.Inventory.Count; i++)
			{
				Equipment item = building.Inventory.GetItem(i);
				num += item.GetNutrition() * (float)item.GetAmount();
			}
		}
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				for (int j = 0; j < member.Inventory.Count; j++)
				{
					Equipment item2 = member.Inventory.GetItem(j);
					num += item2.GetNutrition() * (float)item2.GetAmount();
				}
			}
		}
		CachedHarvestedNutritionAmount = num;
		return num;
	}

	public float GetPlantedNutritionAmount()
	{
		if (CachedPlantedNutritionAmount != -1f)
		{
			return CachedPlantedNutritionAmount;
		}
		CachedPlantedNutritionAmount = Session.Instance.CropsManager.GetPlantedNutritionAmount(Id);
		return CachedPlantedNutritionAmount;
	}

	public float GetAverageMemberSkillForRole(SkillType skillType, Role role)
	{
		float num = 0f;
		int num2 = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && member.HasRole(role))
			{
				num += (float)member.GetSkillLevelWithEffects(skillType);
				num2++;
			}
		}
		if (num2 <= 0)
		{
			return 0f;
		}
		return num / (float)num2;
	}

	public float GetCropPatchNutritionAmount()
	{
		float averageMemberSkillForRole = GetAverageMemberSkillForRole(SkillType.Farming, Role.Farmer);
		return Session.Instance.CropsManager.GetCropPatchNutritionAmount(Id, averageMemberSkillForRole);
	}

	public float GetMemberCountWhenConsideringFoodNeeds()
	{
		if (!IsAISettlement() || Session.Instance.Editor)
		{
			return (float)GetLivingNonZombieMemberCount() + (float)GetLivingNonZombieMemberCountBySpecies(BaseObjectType.Chicken) / Chicken.NutritionFactor;
		}
		return (float)InitialMemberCount + (float)InitialChickenCount / Chicken.NutritionFactor;
	}

	public float GetNutritionAmountNeededToReplaceCurrentStocks()
	{
		return GetMemberCountWhenConsideringFoodNeeds() * RecommendedPlantedNutritionDays * Sun.DayLengthSecs;
	}

	public float GetReservedNutritionAmount()
	{
		float num = 0f;
		foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
		{
			if (item.Value.GetSeedForPlantType() != null && item.Value.GetNutrition() > 0f)
			{
				int val = CountInventoryItemsOfType(item.Value);
				int reservedCropAmount = GetReservedCropAmount(item.Value.GetSeedForPlantType());
				num += (float)Math.Min(val, reservedCropAmount) * item.Value.GetNutrition();
			}
		}
		return num;
	}

	public int GetReservedCropAmount(PropPrototype cropType)
	{
		int num = 0;
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId == Id && patch.CropType == cropType)
			{
				num += patch.Tiles.Count - patch.CropsInPatch.Count;
			}
		}
		return num;
	}

	public float CalcCommunityNutritionLevel(float subtractNutrition = 0f)
	{
		float num = GetHarvestedNutritionAmount() - subtractNutrition;
		float plantedNutritionAmount = GetPlantedNutritionAmount();
		float num2 = Weather.CalcNutritionNeededToStoreForWinterPerPerson(Session.Instance.DayOfYear, rampUpOverPlantingSeason: true) * GetMemberCountWhenConsideringFoodNeeds();
		return (num + plantedNutritionAmount) / num2;
	}

	public int GetCropTileCount(PropPrototype cropType)
	{
		int num = 0;
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId == Id && patch.CropType == cropType)
			{
				num += patch.Tiles.Count;
			}
		}
		return num;
	}

	public bool IsAllowedToEatCrops(PropPrototype cropType)
	{
		if (CommunityType == CommunityType.Player)
		{
			return true;
		}
		int reservedCropAmount = GetReservedCropAmount(cropType);
		if (reservedCropAmount <= 0)
		{
			return true;
		}
		if (!HasAnyoneWithRole(Role.Farmer))
		{
			return true;
		}
		EquipmentPrototype equipmentPrototype = cropType.HarvestSeedsPrototype;
		if (equipmentPrototype == null)
		{
			equipmentPrototype = cropType.HarvestPrototype;
		}
		return CountInventoryItemsOfType(equipmentPrototype) > reservedCropAmount;
	}

	public int GetNumLitCampfires()
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building is Campfire campfire && campfire.IsBurning())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumCompletedBuildings()
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumCompletedBuildingsOfType(BaseObjectType buildingType)
	{
		int num = 0;
		if (buildingType == BaseObjectType.PitTrap)
		{
			foreach (PitTrap pitTrap in PitTraps)
			{
				if (pitTrap.GetUnderConstructionInfo() == null)
				{
					num++;
				}
			}
		}
		else
		{
			foreach (Prop building in Buildings)
			{
				if (building.GetUnderConstructionInfo() == null && building.GetBaseObjectType() == buildingType)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool HasCompletedBuildingsOfType(BaseObjectType buildingType, int desiredAmount)
	{
		int num = 0;
		if (buildingType == BaseObjectType.PitTrap)
		{
			foreach (PitTrap pitTrap in PitTraps)
			{
				if (pitTrap.GetUnderConstructionInfo() == null)
				{
					num++;
					if (num >= desiredAmount)
					{
						return true;
					}
				}
			}
		}
		else
		{
			foreach (Prop building in Buildings)
			{
				if (building.GetUnderConstructionInfo() == null && building.GetBaseObjectType() == buildingType)
				{
					num++;
					if (num >= desiredAmount)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasCompletedBuildingsOfProto(PropPrototype proto, int desiredAmount)
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null && building.GetPropPrototype() == proto)
			{
				num++;
				if (num >= desiredAmount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetNumCompletedBuildingsOfClass(Type type)
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null && building.GetType().IsA(type))
			{
				num++;
			}
		}
		return num;
	}

	public bool HasGatesThatEncloseSomething(int desiredCount)
	{
		int num = 0;
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() != null || !(building is Gate gate))
			{
				continue;
			}
			TerrainCoord tileInsideGate = gate.GetTileInsideGate();
			if (GameTerrain.Instance.IsTileEnclosed(tileInsideGate.x, tileInsideGate.y))
			{
				num++;
				if (num >= desiredCount)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasVehicleToHitTheRoad(bool checkFuel, PropPrototype proto)
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		foreach (Prop building in Buildings)
		{
			if (building is EnterableVehicle enterableVehicle && enterableVehicle.GetUnderConstructionInfo() == null && !enterableVehicle.IsDestroyed() && enterableVehicle.IsDriveable && (proto == null || enterableVehicle.GetPropPrototype() == proto) && enterableVehicle.GetPropPrototype().VehicleCanHitTheRoad && !(enterableVehicle.GetDamageFraction() > settings.HitTheRoadMaxDamagePercent / 100f) && (!checkFuel || !(enterableVehicle.GetLiquidAmount() < enterableVehicle.GetLiquidCapacity() * (settings.HitTheRoadMinFuelPercent / 100f))))
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumOuthouses()
	{
		return GetNumCompletedBuildingsOfClass(typeof(Outhouse));
	}

	public int GetNumGuardPosts()
	{
		int num = 0;
		foreach (Prop building2 in Buildings)
		{
			if (building2 is Building building && building.GetUnderConstructionInfo() == null && building.IsGuardPost())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumChickenCoops()
	{
		int num = 0;
		foreach (Prop building2 in Buildings)
		{
			if (building2 is Building building && building.GetUnderConstructionInfo() == null && building.Prototype.EnterableBySpecies == BaseObjectType.Chicken)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetDesiredNumOuthouses(int population)
	{
		return Math.Max(0, population - 1) / 4;
	}

	public bool IsUnhygienic()
	{
		int current;
		int desired;
		return IsUnhygienic(out current, out desired);
	}

	public bool IsUnhygienic(out int current, out int desired)
	{
		current = GetNumOuthouses();
		desired = GetDesiredNumOuthouses(GetLivingNonZombieMemberCount());
		return current < desired;
	}

	public int GetAccommodation()
	{
		int num = 0;
		foreach (Prop building2 in Buildings)
		{
			if (building2.GetUnderConstructionInfo() == null && building2.IsAccommodation() && building2 is Building building)
			{
				num += building.GetInhabitantSlotDefs().Length;
			}
		}
		return num;
	}

	public bool HasAnyBuildingsWithInternalSlots()
	{
		foreach (Prop building in Buildings)
		{
			if (building.GetUnderConstructionInfo() == null && !building.IsGuardPost() && building is Building)
			{
				return true;
			}
		}
		return false;
	}

	public bool LacksSpace()
	{
		int current;
		int desired;
		return LacksSpace(out current, out desired);
	}

	public bool LacksSpace(out int current, out int desired)
	{
		if (IsAmbientCommunity())
		{
			current = 0;
			desired = 0;
			return false;
		}
		current = GetAccommodation();
		desired = GetLivingNonZombieMemberCount();
		return current < desired;
	}

	public int GetChickenAccommodation()
	{
		int num = 0;
		foreach (Prop building2 in Buildings)
		{
			if (building2 is Building building && building.GetUnderConstructionInfo() == null && building.Prototype.EnterableBySpecies == BaseObjectType.Chicken)
			{
				num += building.GetInhabitantSlotDefs().Length;
			}
		}
		return num;
	}

	private float GetTechScoreForBuilding(TileObject building)
	{
		if (building is SingleTileObject)
		{
			return 0f;
		}
		float result = 0f;
		PropPrototype propPrototype = building.GetPropPrototype();
		if (propPrototype != null)
		{
			Recipe recipe = GameImpl.Instance.FindRecipeByProduct(propPrototype, null);
			if (recipe != null)
			{
				if (!recipe.AreAllIngredientsReturnedInFull())
				{
					result = (float)recipe.SkillLevel / 5f;
				}
			}
			else if (propPrototype.CaptureResourceProto != null)
			{
				result = Mathf.Clamp01(propPrototype.CaptureResourceNeeded * propPrototype.CaptureResourceProto.BasePrice / 10f);
			}
		}
		return result;
	}

	public void OnCompletedBuildingAddedToCommunity(Character addedBy, TileObject building, Community oldCommunity)
	{
		if (CommunityType != CommunityType.Player)
		{
			return;
		}
		float num = 0f;
		PropPrototype propPrototype = building.GetPropPrototype();
		if (propPrototype != null)
		{
			num = GetTechScoreForBuilding(building);
			int num2 = 0;
			foreach (Prop building2 in Buildings)
			{
				if (building2 != building)
				{
					PropPrototype propPrototype2 = building2.GetPropPrototype();
					GameImpl.Instance.FindRecipeByProduct(propPrototype, null);
					if (propPrototype2 == propPrototype || (building2.IsAccommodation() && building.IsAccommodation() && GetTechScoreForBuilding(building2) >= num) || (building2.IsGuardPost() && building.IsGuardPost() && GetTechScoreForBuilding(building2) >= num))
					{
						num2++;
					}
				}
			}
			num *= Mathf.Clamp01(1f - (float)num2 * 0.25f);
		}
		foreach (Character member in Members)
		{
			member.UpdateMemories(force: true);
			if (Leader != null && Leader.AliveAndNotZombie && num > 0f)
			{
				member.AddMemory(MemoryPrototype.LeadershipBuiltTech, Leader, this, num);
			}
		}
		if (building is Gate || building is BaseFence)
		{
			List<TileObject> list = new List<TileObject>();
			list.Add(building);
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
						if (fixedObjectOnTile is BaseFence)
						{
							Community community = fixedObjectOnTile.GetCommunity();
							if (community != this && GameCursor.CanTakeOverCommunityBuildings(addedBy, community))
							{
								fixedObjectOnTile.SetCommunity(this);
								list.Add(fixedObjectOnTile);
							}
						}
					}
				}
			}
		}
		if (building is PlantableCrop && oldCommunity != null)
		{
			Session.Instance.CropsManager.GetPatch(building.GetTile(), oldCommunity.Id)?.SetCommunity(this);
		}
	}

	public int CountFoodItems()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountFoodItems();
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountFoodItems();
		}
		return num;
	}

	public float GetTotalWater()
	{
		float num = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.GetTotalWater();
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.GetTotalWater();
		}
		return num;
	}

	public float GetTotalLiquid(LiquidPrototype liquid)
	{
		float num = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.GetTotalLiquid(liquid);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.GetTotalLiquid(liquid);
		}
		return num;
	}

	public float GetTotalLiquid(LiquidPrototype liquid, InfectionType infectionType)
	{
		float num = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.GetTotalLiquid(liquid, infectionType);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.GetTotalLiquid(liquid, infectionType);
		}
		return num;
	}

	public float GetTotalLiquidWithinMovementZone(Character character, LiquidPrototype liquid)
	{
		float num = 0f;
		if (liquid == LiquidPrototype.Snow && Session.Instance.Weather.SnowOnGroundAmount >= Weather.ScoopableSnowOnGroundAmount)
		{
			return float.MaxValue;
		}
		if (liquid == LiquidPrototype.Water)
		{
			TerrainCoord nearestTileOnPath = GameTerrain.Instance.GetNearestTileOnPath(character.PosXZ, river: true, float.MaxValue);
			if (nearestTileOnPath != TerrainCoord.Invalid && (!character.HasMovementZone() || character.MovementZone.Contains(nearestTileOnPath)))
			{
				return float.MaxValue;
			}
		}
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && (!character.HasMovementZone() || character.MovementZone.Contains(member.Tile)))
			{
				num += member.Inventory.GetTotalLiquid(liquid);
			}
		}
		foreach (Prop building in Buildings)
		{
			if ((!character.HasMovementZone() || character.MovementZone.Overlaps(building.GetTileRect())) && building.GetBaseObjectType() != BaseObjectType.AnimalFeederProp)
			{
				if (liquid == building.GetLiquidType() && building.CanBeEmptied())
				{
					num += building.GetLiquidAmount();
				}
				if (liquid == LiquidPrototype.Water && building is Well)
				{
					return float.MaxValue;
				}
				num += building.Inventory.GetTotalLiquid(liquid);
			}
		}
		return num;
	}

	public int GetGoldAmount()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.GetGoldAmount();
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.GetGoldAmount();
		}
		return num;
	}

	public int GetNumGiftableItems()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.GetNumGiftableItems();
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.GetNumGiftableItems();
		}
		return num;
	}

	public float CalcLootableSuppliesValue()
	{
		float num = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CalcLootableSuppliesValue(member);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CalcLootableSuppliesValue(building);
		}
		return num;
	}

	public int CountBackpacks()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountBackpacks();
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountBackpacks();
		}
		return num;
	}

	public int FindHighestBandageLevel(bool includeDead)
	{
		int num = -1;
		foreach (Character member in Members)
		{
			if (includeDead || member.AliveAndNotZombie)
			{
				num = Math.Max(num, member.Inventory.FindHighestBandageLevel());
				if (num >= 5)
				{
					return num;
				}
			}
		}
		foreach (Prop building in Buildings)
		{
			num = Math.Max(num, building.Inventory.FindHighestBandageLevel());
			if (num >= 5)
			{
				return num;
			}
		}
		return num;
	}

	public TileObject FindInventoryItemOfType(EquipmentPrototype proto, bool includeDead)
	{
		foreach (Character member in Members)
		{
			if ((member.AliveAndNotZombie || includeDead) && member.Inventory.FindItemOfType(proto) != null)
			{
				return member;
			}
		}
		foreach (Prop building in Buildings)
		{
			if (building.Inventory.FindItemOfType(proto) != null)
			{
				return building;
			}
		}
		return null;
	}

	public int CountInventoryItemsOfType(EquipmentPrototype proto)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountItemsOfType(proto);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountItemsOfType(proto);
		}
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId != Id || patch.CropsInPatch.Count <= 0)
			{
				continue;
			}
			if (patch.CropsInPatch[0].GetHarvestPrototype() == proto)
			{
				foreach (PlantableCrop item in patch.CropsInPatch)
				{
					if (item.IsRipe())
					{
						num += item.HarvestableAmount;
					}
				}
			}
			else
			{
				if (patch.CropsInPatch[0].GetHarvestSeedsPrototype() != proto)
				{
					continue;
				}
				foreach (PlantableCrop item2 in patch.CropsInPatch)
				{
					if (item2.IsRipe())
					{
						num += item2.HarvestableSeedsAmount;
					}
				}
			}
		}
		return num;
	}

	public int CountInventoryItemsOfType(EquipmentPrototype proto, InfectionType infectionType)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountItemsOfType(proto, infectionType);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountItemsOfType(proto, infectionType);
		}
		if (infectionType == InfectionType.None)
		{
			foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
			{
				if (patch.CommunityId != Id || patch.CropsInPatch.Count <= 0)
				{
					continue;
				}
				if (patch.CropsInPatch[0].GetHarvestPrototype() == proto)
				{
					foreach (PlantableCrop item in patch.CropsInPatch)
					{
						if (item.IsRipe())
						{
							num += item.HarvestableAmount;
						}
					}
				}
				else
				{
					if (patch.CropsInPatch[0].GetHarvestSeedsPrototype() != proto)
					{
						continue;
					}
					foreach (PlantableCrop item2 in patch.CropsInPatch)
					{
						if (item2.IsRipe())
						{
							num += item2.HarvestableSeedsAmount;
						}
					}
				}
			}
		}
		return num;
	}

	public int CountMembersWithGiftedItemsOfType(EquipmentPrototype proto)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.Inventory.CountGiftedItemsOfType(proto) > 0 && (member != Leader || CommunityType != CommunityType.Player))
			{
				num++;
			}
		}
		return num;
	}

	public int CountGiftedItemsOfType(EquipmentPrototype proto)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountGiftedItemsOfType(proto);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountGiftedItemsOfType(proto);
		}
		return num;
	}

	public int CountTradedInventoryItemsOfType(EquipmentPrototype proto)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountTradedItemsOfType(proto);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountTradedItemsOfType(proto);
		}
		return num;
	}

	public int CountInventoryItemsOfClass(Type type, bool includeBuildings)
	{
		return CountInventoryItemsOfClass(type, includeBuildings, includeUnconscious: true);
	}

	public int CountInventoryItemsOfClass(Type type, bool includeBuildings, bool includeUnconscious)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && (member.IsAwake || includeUnconscious))
			{
				num += member.Inventory.CountItemsOfClass(type);
			}
		}
		if (includeBuildings)
		{
			foreach (Prop building in Buildings)
			{
				num += building.Inventory.CountItemsOfClass(type);
			}
		}
		return num;
	}

	public Equipment FindInventoryItemOfClass(Type type, bool includeBuildings = true, bool includeDead = false)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie || includeDead)
			{
				Equipment equipment = member.Inventory.FindItemOfClass(type);
				if (equipment != null)
				{
					return equipment;
				}
			}
		}
		foreach (Prop building in Buildings)
		{
			Equipment equipment2 = building.Inventory.FindItemOfClass(type);
			if (equipment2 != null)
			{
				return equipment2;
			}
		}
		return null;
	}

	public bool HasUsableInventoryItemsOfTypeWithinMovementZone(Character character, EquipmentPrototype proto, int needed)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if ((member.Alive && (member.Zombie || character == member)) || (character.HasMovementZone() && !character.MovementZone.Contains(member.Tile)))
			{
				continue;
			}
			for (int i = 0; i < member.Inventory.Count; i++)
			{
				Equipment item = member.Inventory.GetItem(i);
				if (item.GetPrototype() == proto && item.InfectedWith == InfectionType.None && !member.IsDoingSomethingTerriblyImportant() && FindGoal.CanTransferFromCharacter(member, item, FindType.CraftingResources, out var amountToTransfer))
				{
					num += amountToTransfer;
					if (num >= needed)
					{
						return true;
					}
				}
			}
		}
		foreach (Prop building in Buildings)
		{
			if ((!character.HasMovementZone() || character.MovementZone.Overlaps(building.GetTileRect())) && (building.GetBaseObjectType() != BaseObjectType.Campfire || (proto != EquipmentPrototype.Pot && proto != EquipmentPrototype.FryingPan)) && building.GetBaseObjectType() != BaseObjectType.AnimalFeederProp)
			{
				num += building.Inventory.CountItemsOfType(proto, InfectionType.None);
				if (num >= needed)
				{
					return true;
				}
			}
		}
		foreach (CropPatch patch in Session.Instance.CropsManager.Patches)
		{
			if (patch.CommunityId != Id || (character.HasMovementZone() && !character.MovementZone.Overlaps(patch.Rect)) || patch.CropsInPatch.Count <= 0)
			{
				continue;
			}
			if (patch.CropsInPatch[0].GetHarvestPrototype() == proto)
			{
				foreach (PlantableCrop item2 in patch.CropsInPatch)
				{
					if (item2.IsRipe() && (!character.HasMovementZone() || character.MovementZone.Contains(item2.Tile)))
					{
						num += item2.HarvestableAmount;
						if (num >= needed)
						{
							return true;
						}
					}
				}
			}
			else
			{
				if (patch.CropsInPatch[0].GetHarvestSeedsPrototype() != proto)
				{
					continue;
				}
				foreach (PlantableCrop item3 in patch.CropsInPatch)
				{
					if (item3.IsRipe() && (!character.HasMovementZone() || character.MovementZone.Contains(item3.Tile)))
					{
						num += item3.HarvestableSeedsAmount;
						if (num >= needed)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool HasUsableInventoryItemsOfClassWithinMovementZone(Character character, Type type, int needed)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if ((member.Alive && member.Zombie) || (character.HasMovementZone() && !character.MovementZone.Contains(member.Tile)) || member.IsDoingSomethingTerriblyImportant())
			{
				continue;
			}
			Equipment equipment = member.Inventory.FindItemOfClass(type);
			if (equipment != null && FindGoal.CanTransferFromCharacter(member, equipment, FindType.CraftingResources, out var amountToTransfer))
			{
				num += amountToTransfer;
				if (num >= needed)
				{
					return true;
				}
			}
		}
		foreach (Prop building in Buildings)
		{
			if (!character.HasMovementZone() || character.MovementZone.Overlaps(building.GetTileRect()))
			{
				num += building.Inventory.CountItemsOfClass(type);
				if (num >= needed)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int CountInventoryItemsOfClothingType(ClothingType clothingType)
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num += member.Inventory.CountItemsOfClothingType(clothingType);
			}
		}
		foreach (Prop building in Buildings)
		{
			num += building.Inventory.CountItemsOfClothingType(clothingType);
		}
		return num;
	}

	public bool HasAnyInventoryItemsOfClass(Type type)
	{
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.Inventory.FindItemOfClass(type) != null)
			{
				return true;
			}
		}
		foreach (Prop building in Buildings)
		{
			if (building.Inventory.FindItemOfClass(type) != null)
			{
				return true;
			}
		}
		return false;
	}

	public override void CalcApprovalRatingForCharacterOrCommunity(BaseObject obj, out float approval, out float respect, TimeSpan afterTime, TimeSpan beforeTime)
	{
		if (obj is Character character)
		{
			CalcAverageApprovalRating(character, out approval, out respect, afterTime, beforeTime);
		}
		else if (obj is Community community)
		{
			CalcAverageApprovalRating(community, out approval, out respect, afterTime, beforeTime);
		}
		else
		{
			base.CalcApprovalRatingForCharacterOrCommunity(obj, out approval, out respect, afterTime, beforeTime);
		}
	}

	public void CalcAverageApprovalRating(Character character, out float approval, out float respect)
	{
		CalcAverageApprovalRating(character, out approval, out respect, Target.Never, TimeSpan.MaxValue);
	}

	public void CalcAverageApprovalRating(Character character, out float avgApproval, out float avgRespect, TimeSpan afterTime, TimeSpan beforeTime)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
			{
				member.CalcApprovalRating(character, out var approval, out var respect, afterTime, beforeTime);
				num += approval;
				num2 += respect;
				num3 += 1f;
			}
		}
		if (num3 > 0f)
		{
			avgApproval = num / num3;
			avgRespect = num2 / num3;
		}
		else
		{
			avgApproval = 0f;
			avgRespect = 0f;
		}
	}

	public void CalcAverageApprovalRating(Community community, out float approval, out float respect)
	{
		CalcAverageApprovalRating(community, out approval, out respect, Target.Never, TimeSpan.MaxValue);
	}

	public void CalcAverageApprovalRating(Community community, out float avgApproval, out float avgRespect, TimeSpan afterTime, TimeSpan beforeTime)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
			{
				member.CalcApprovalRating(community, out var approval, out var respect, afterTime, beforeTime);
				num += approval;
				num2 += respect;
				num3 += 1f;
			}
		}
		if (num3 > 0f)
		{
			avgApproval = num / num3;
			avgRespect = num2 / num3;
		}
		else
		{
			avgApproval = 0f;
			avgRespect = 0f;
		}
	}

	public int GetWorldMapQuadrants()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num |= member.Inventory.GetWorldMapQuadrants();
			}
		}
		foreach (Prop building in Buildings)
		{
			num |= building.Inventory.GetWorldMapQuadrants();
		}
		return num;
	}

	public int GetGeologicalMapQuadrants()
	{
		int num = 0;
		foreach (Character member in Members)
		{
			if (member.AliveAndNotZombie)
			{
				num |= member.Inventory.GetGeologicalMapQuadrants();
			}
		}
		foreach (Prop building in Buildings)
		{
			num |= building.Inventory.GetGeologicalMapQuadrants();
		}
		return num;
	}

	public void RandomizeRelationships(CustomRandom rand)
	{
		int livingNonZombieMemberCount = GetLivingNonZombieMemberCount();
		int desiredRelationships = rand.Next(livingNonZombieMemberCount / 2, livingNonZombieMemberCount);
		RandomizeRelationships(rand, desiredRelationships);
	}

	public void RandomizeRelationships(CustomRandom rand, int desiredRelationships)
	{
		if (Members.Count <= 1)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		while (num < desiredRelationships && num2 < 1000)
		{
			num2++;
			Character character = Members[rand.Next(Members.Count)];
			Character character2 = Members[rand.Next(Members.Count)];
			if (character == character2 || character.GetBaseObjectType() != BaseObjectType.Human || character2.GetBaseObjectType() != BaseObjectType.Human)
			{
				continue;
			}
			RelationshipType relationshipType = (RelationshipType)rand.Next(1, 8);
			float num3 = character2.Appearance.Age - character.Appearance.Age;
			if (num3 >= Relationship.MinParentAge)
			{
				relationshipType = RelationshipType.ChildOf;
			}
			else if (num3 <= 0f - Relationship.MinParentAge)
			{
				relationshipType = RelationshipType.ParentOf;
			}
			if ((((uint)(relationshipType - 1) > 2u && relationshipType != RelationshipType.MarriedTo) || (string.IsNullOrEmpty(character.GetUniqueID()) && string.IsNullOrEmpty(character2.GetUniqueID()))) && Relationship.SetRelationship(character, relationshipType, character2, allowChange: false, generating: true))
			{
				if (relationshipType == RelationshipType.InLoveWith && rand.RandomChoice(Relationship.ProbabilityOfLoveBeingReciprocated))
				{
					Relationship.SetRelationship(character2, RelationshipType.InLoveWith, character, allowChange: true, generating: true);
				}
				Relationship.RandomizeRelationshipApprovalRespect(character, character2, rand);
				Relationship.RandomizeRelationshipApprovalRespect(character2, character, rand);
				num++;
			}
		}
	}

	public void WarnAboutTraps(bool pitTraps, bool tripwires)
	{
		if (pitTraps && !WarnedAboutTraps)
		{
			WarnedAboutTraps = true;
			GameTerrain.Instance.AStar.AddChange(AStarChange.WarnAboutTraps(Id));
		}
		if (tripwires && !WarnedAboutTripwires)
		{
			WarnedAboutTripwires = true;
			GameTerrain.Instance.AStar.AddChange(AStarChange.WarnAboutTripwires(Id));
		}
	}

	public void TellTradersToMoveOn(Character character)
	{
		if (Squads.Count <= 0)
		{
			return;
		}
		Squad squad = Squads[0];
		squad.MovedAlong = true;
		foreach (Character member in squad.Members)
		{
			if (member != squad.GetLeader())
			{
				member.ClearFollowCommand();
			}
		}
		if (squad.Behaviour == SquadBehaviour.Trade)
		{
			if (!GoToNextTradeDestination(squad, Session.Instance.DeterministicRand, mustMove: true, canOccupyBases: false))
			{
				StartJourneyToExitMap(squad, scripted: true, avoidHostileBases: true, highPrio: true);
			}
		}
		else if (squad.Behaviour == SquadBehaviour.Travel)
		{
			StartJourneyToExitMap(squad, scripted: true, avoidHostileBases: true, highPrio: true);
		}
	}

	public void ExfilHeliTeam()
	{
		Character firstActiveMember = GetFirstActiveMember();
		if (firstActiveMember == null)
		{
			return;
		}
		TerrainCoord tile = ((Leader != null && Leader.AliveAndNotZombie) ? Leader.Tile : firstActiveMember.Tile);
		Building building = GetNearestBuildingOfType(tile, typeof(Helicopter), float.MaxValue) as Building;
		if (building == null)
		{
			building = GetNearestBuildingOfType(tile, typeof(Building), float.MaxValue) as Building;
		}
		if (building != null)
		{
			Squad squad = AddSquad(SquadBehaviour.Exfil, 0);
			foreach (Character member in Members)
			{
				if (member.AliveAndNotZombie)
				{
					AddToSquad(member, squad);
				}
			}
			if (squad.Members.Count == 0)
			{
				RemoveSquad(squad);
			}
			else
			{
				SetSquadAction(squad, SquadAction.Enter, 0, building.Tile, null);
			}
			return;
		}
		Squad squad2 = AddSquad(SquadBehaviour.Travel, 0);
		foreach (Character member2 in Members)
		{
			if (member2.AliveAndNotZombie)
			{
				AddToSquad(member2, squad2);
			}
		}
		if (squad2.Members.Count == 0)
		{
			RemoveSquad(squad2);
		}
		else
		{
			StartJourneyToExitMap(squad2, scripted: true, avoidHostileBases: true);
		}
	}

	public void Ambush(Character leadCharacter, Character targetCharacter, float teleportAfterTime, int partySize, EquipmentPrototype proto)
	{
		if (!leadCharacter.AliveAndNotZombie)
		{
			return;
		}
		if (GetRelationship(targetCharacter.Community) == CommunityRelationshipType.Hostile)
		{
			Session.Instance.CommunityManager.SetRelationship(this, targetCharacter.Community, CommunityRelationshipType.Known, showWarNotifications: false);
		}
		Squad squad = AddSquad(SquadBehaviour.Ambush, targetCharacter.GetCommunityId());
		squad.TeleportAfterTime = TimeSpan.FromSeconds(teleportAfterTime);
		squad.AmbushForEquipmentType = proto;
		AddToSquad(leadCharacter, squad);
		if (partySize == 0)
		{
			partySize = 5;
		}
		int num = Math.Max(targetCharacter.Community.CountMembersInRangeOf(targetCharacter.Tile, 64f) + 2, partySize);
		while (squad.Members.Count < num)
		{
			Character character = null;
			float num2 = float.MaxValue;
			foreach (Character member in Members)
			{
				if (!member.AliveAndNotZombie || member.GetBaseObjectType() != BaseObjectType.Human || member.SquadId == squad.Id)
				{
					continue;
				}
				float num3 = member.Tile.GetDist(leadCharacter.Tile);
				if (member.HasRole(Role.Enforcer))
				{
					num3 -= 200f;
				}
				else if (member.HasRole(Role.Guard))
				{
					num3 -= 100f;
				}
				if (Session.Instance.CommunityManager.PlayerCommunity.IsCharacterVisibleToAnyMember(member, out var _))
				{
					num3 += 10000f;
					if (Session.Instance.IsVisibleDeterministic(member.PosXZ))
					{
						num3 += 1000000f;
					}
				}
				if (num3 < num2)
				{
					character = member;
					num2 = num3;
				}
			}
			if (character == null)
			{
				break;
			}
			AddToSquad(character, squad);
		}
		SetSquadAction(squad, SquadAction.GoToAmbush, 0, TerrainCoord.Invalid, targetCharacter);
	}
}
