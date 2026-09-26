using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CommunityManager : IReflectable
{
	public struct TileAndRadius
	{
		public TerrainCoord Tile;

		public float Radius;

		public TileAndRadius(TerrainCoord tile, float radius)
		{
			Tile = tile;
			Radius = radius;
		}
	}

	public enum Status
	{
		None,
		InProgress,
		Success,
		Fail
	}

	public List<Community> Communities = new List<Community>();

	public Community PlayerCommunity;

	public List<AmbientEnemySpawnPoint> AmbientEnemySpawnPoints = new List<AmbientEnemySpawnPoint>();

	public List<DeerSpawnPoint> DeerSpawnPoints = new List<DeerSpawnPoint>();

	public List<Town> Towns = new List<Town>();

	public List<Squad> SquadsWithMapIcons = new List<Squad>();

	private TimeSpan LastAmbientEnemiesUpdate;

	public List<AmbientEnemySpawnPoint> PretendDied = new List<AmbientEnemySpawnPoint>();

	public int NextFreeThreatId = 1;

	public int NextFreeSquadId = 1;

	private int CurUpdateIndex;

	private static GameProfiler CommunityReflectTimer = new GameProfiler("CommunityReflect");

	private static List<Community> RepopulateCandidates = new List<Community>();

	private static List<Community> RepopulateChickenCandidates = new List<Community>();

	private static List<Prop> RepopulateCandidateBuildings = new List<Prop>();

	private static List<Character> RepopulateCandidateMembersForRelationship = new List<Character>();

	private static RelationshipType[] RepopulateRelationshipTypes = new RelationshipType[4]
	{
		RelationshipType.None,
		RelationshipType.InLoveWith,
		RelationshipType.FriendsWith,
		RelationshipType.SleepingWith
	};

	private float AmbientRespawnBuildup;

	private static int MaxAmbientEnemyGroups = 16;

	private TaskFunc _threadedAmbientEnemiesTaskFunc;

	private ManualResetEvent _threadedAmbientEnemiesTaskFinishedEvent = new ManualResetEvent(initialState: true);

	public List<TileAndRadius> CachedPlayerCommunityTiles = new List<TileAndRadius>();

	private bool RespawnsAllowed;

	private TimeSpan SessionTime;

	private int InputFrameWhenTaskStarted;

	public int VisitedTownCount;

	private AmbientEnemySpawnPoint BestSpawnPoint;

	private Community WorstEnemyGroup;

	private bool WorstEnemyGroupAllMembersDead;

	private float WorstEnemyGroupDistSqr;

	private int AmbientEnemyGroupCount;

	private bool WantRebuildMinimap;

	private bool _success;

	private Status _status;

	private static string AmbientEnemySpawnPointsStr = "AmbientEnemySpawnPoints";

	public static string Invaders_Town_GreenStrain = "Invaders_Town_GreenStrain";

	public static string Invaders_Town_BlueStrain = "Invaders_Town_BlueStrain";

	public static string Invaders_Town_RedStrain = "Invaders_Town_RedStrain";

	public static string Invaders_Town_WhiteStrain = "Invaders_Town_WhiteStrain";

	public static string Invaders_HiredMercs = "Invaders_HiredMercs";

	private static TimeSpan AmbientEnemiesUpdateFrequency = TimeSpan.FromSeconds(1.0);

	private static List<ControlPoint> RoadCandidates = new List<ControlPoint>();

	public static float MaxAggro = 100f;

	public int NumFreebieInvaders;

	public List<BaseObject> Hunters = new List<BaseObject>();

	public TimeSpan RepopulateCountdown;

	public CommunityManager()
	{
		_threadedAmbientEnemiesTaskFunc = ThreadedUpdateAmbientEnemiesTaskFunc;
	}

	public void Unload()
	{
	}

	public void AddSquadWithMapIcons(Squad squad)
	{
		if (!SquadsWithMapIcons.Contains(squad))
		{
			SquadsWithMapIcons.Add(squad);
		}
	}

	public void CacheAllies()
	{
		foreach (Community community in Communities)
		{
			community.CachedAllies.Clear();
			for (int i = 0; i < community.CommunityRelationships.Count; i++)
			{
				if (community.CommunityRelationships[i].RelationshipType == CommunityRelationshipType.Allied)
				{
					Community otherCommunity = community.CommunityRelationships[i].GetOtherCommunity();
					if (otherCommunity != null)
					{
						community.CachedAllies.Add(otherCommunity);
					}
				}
			}
		}
	}

	public void Reflect(Reflector reflector)
	{
		using (new ProfileMarker(CommunityReflectTimer))
		{
			if (reflector.Version < 569)
			{
				List<OldCommunityRelationship> list = new List<OldCommunityRelationship>();
				reflector.Add(ref list);
				for (int i = 0; i < list.Count; i++)
				{
					Community community = list[i].GetCommunity1();
					Community community2 = list[i].GetCommunity2();
					if (community != null && community2 != null)
					{
						CommunityRelationshipType relationshipType = list[i].RelationshipType;
						community.CommunityRelationships.Add(new CommunityRelationshipRecord(community2.Id, relationshipType, initiator: true));
						community2.CommunityRelationships.Add(new CommunityRelationshipRecord(community.Id, relationshipType, initiator: false));
					}
				}
			}
			if (reflector.IsDeserialising)
			{
				CacheAllies();
			}
			reflector.Add(ref AmbientRespawnBuildup);
			reflector.AddAfter(ref RepopulateCountdown, 115);
			if (reflector.Version < 253)
			{
				float value = 0f;
				reflector.Add(ref value);
			}
			reflector.AddAfter(ref NumFreebieInvaders, 254);
			reflector.AddGameObjectRefList(ref Hunters);
			reflector.Add(ref LastAmbientEnemiesUpdate);
			reflector.Add(ref CurUpdateIndex);
			if (reflector.Version >= 289)
			{
				reflector.AddGameObjectRefList(ref PretendDied);
			}
			if (reflector.Version < 96)
			{
				foreach (Community community3 in Communities)
				{
					if (community3.CommunityType == CommunityType.Looter && GetRelationship(community3, PlayerCommunity) == CommunityRelationshipType.Known)
					{
						community3.NextPlayerShakedownTime = Session.Instance.PlayTime;
					}
				}
			}
			if (reflector.Version < 566)
			{
				foreach (Community community4 in Communities)
				{
					for (int num = community4.FailedToBury.Count - 1; num >= 0; num--)
					{
						if (community4.FailedToBury[num].Corpse == null || community4.FailedToBury[num].Corpse.Disappeared)
						{
							community4.FailedToBury.RemoveAt(num);
						}
					}
				}
			}
			if (reflector.Version >= 584 && reflector.Version < 586)
			{
				List<Squad> list2 = new List<Squad>();
				reflector.Add(ref list2);
			}
		}
	}

	public void OnCommunityDeleted(Community deletedCommunity)
	{
		deletedCommunity.WarnedAboutTraps = false;
		deletedCommunity.WarnedAboutTripwires = false;
		Communities.Remove(deletedCommunity);
		for (int i = 0; i < deletedCommunity.CommunityRelationships.Count; i++)
		{
			deletedCommunity.CommunityRelationships[i].GetOtherCommunity()?.ClearCommunityRelationship(deletedCommunity);
		}
		foreach (Community cachedAlly in deletedCommunity.CachedAllies)
		{
			cachedAlly.CachedAllies.Remove(deletedCommunity);
		}
		deletedCommunity.CachedAllies.Clear();
		if (Session.Instance.Editor || deletedCommunity.IsAISettlement())
		{
			Session.Instance.CropsManager.DeletePatchesOwnedByCommunity(deletedCommunity.Id);
		}
	}

	public CommunityRelationshipType GetRelationship(Community community1, Community community2)
	{
		bool community1Surrendered;
		return GetRelationship(community1, community2, out community1Surrendered);
	}

	public CommunityRelationshipType GetRelationship(Community community1, Community community2, out bool community1Surrendered)
	{
		if (community1 != null)
		{
			return community1.GetRelationship(community2, out community1Surrendered);
		}
		community1Surrendered = false;
		return CommunityRelationshipType.Unknown;
	}

	public static void ApplyRelationship(List<OldCommunityRelationship> relationships, OldCommunityRelationship relationship, out CommunityRelationshipType oldRelationshipType)
	{
		oldRelationshipType = CommunityRelationshipType.Unknown;
		bool flag = false;
		for (int i = 0; i < relationships.Count; i++)
		{
			if (relationships[i].IsBetween(relationship.Community1Id, relationship.Community2Id))
			{
				oldRelationshipType = relationships[i].RelationshipType;
				relationships[i] = relationship;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			relationships.Add(relationship);
		}
	}

	public void SetRelationship(Community community1, Community community2, CommunityRelationshipType relationshipType)
	{
		SetRelationship(community1, community2, relationshipType, showWarNotifications: true);
	}

	public void SetRelationship(Community community1, Community community2, CommunityRelationshipType relationshipType, bool showWarNotifications)
	{
		if (community1 == community2 || ((community1.IsAlwaysHostileCommunity() || community2.IsAlwaysHostileCommunity()) && ((relationshipType != CommunityRelationshipType.Ceasefire && relationshipType != CommunityRelationshipType.Hostile) || (community1.CommunityType != CommunityType.Player && community2.CommunityType != CommunityType.Player))) || community1.IsAnimalCommunity() || community2.IsAnimalCommunity())
		{
			return;
		}
		CommunityRelationshipType communityRelationshipType = community1.SetCommunityRelationship(community2, relationshipType, initiator: true);
		community2.SetCommunityRelationship(community1, relationshipType, initiator: false);
		if (communityRelationshipType == CommunityRelationshipType.Unknown && (community1.IsAlwaysHostileCommunity() || community2.IsAlwaysHostileCommunity()))
		{
			communityRelationshipType = CommunityRelationshipType.Hostile;
		}
		if (communityRelationshipType != CommunityRelationshipType.Hostile && relationshipType == CommunityRelationshipType.Hostile)
		{
			Debug.Log("War declared between " + community1.GetDisplayNameString() + " and " + community2.GetDisplayNameString());
		}
		StoryManager.Instance.SetConditionsDirty();
		OldCommunityRelationship communityRelationship = new OldCommunityRelationship(community1.Id, community2.Id, relationshipType);
		GameTerrain.Instance.AStar.AddChange(AStarChange.SetCommunityRelationship(communityRelationship));
		if (communityRelationshipType != CommunityRelationshipType.Allied && relationshipType == CommunityRelationshipType.Allied)
		{
			community1.CachedAllies.Add(community2);
			community2.CachedAllies.Add(community1);
			List<Community> listOfCommunitiesWithRelationship = GetListOfCommunitiesWithRelationship(community1, CommunityRelationshipType.Hostile);
			List<Community> listOfCommunitiesWithRelationship2 = GetListOfCommunitiesWithRelationship(community2, CommunityRelationshipType.Hostile);
			foreach (Community item in listOfCommunitiesWithRelationship)
			{
				if (item != community2 && GetRelationship(community2, item) != CommunityRelationshipType.Hostile)
				{
					SetRelationship(community2, item, CommunityRelationshipType.Hostile);
				}
			}
			foreach (Community item2 in listOfCommunitiesWithRelationship2)
			{
				if (item2 != community1 && GetRelationship(community1, item2) != CommunityRelationshipType.Hostile)
				{
					SetRelationship(community1, item2, CommunityRelationshipType.Hostile);
				}
			}
		}
		else if (communityRelationshipType == CommunityRelationshipType.Allied && relationshipType != CommunityRelationshipType.Allied)
		{
			community1.CachedAllies.Remove(community2);
			community2.CachedAllies.Remove(community1);
			foreach (Character member in community1.Members)
			{
				if (member.SquadLeader != null && member.SquadLeader.Community == community2)
				{
					member.Follow(null);
				}
			}
			foreach (Character member2 in community2.Members)
			{
				if (member2.SquadLeader != null && member2.SquadLeader.Community == community1)
				{
					member2.Follow(null);
				}
			}
			if (community1.CommunityType == CommunityType.Player || community2.CommunityType == CommunityType.Player)
			{
				Community community3 = ((community1.CommunityType == CommunityType.Player) ? community1 : community2);
				Community community4 = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
				if (community4.IsAISettlement() && community4.BaseRect != TerrainRect.Invalid)
				{
					int num = 16;
					List<TileObject> list = new List<TileObject>();
					GameTerrain.Instance.GetObjectsInRect(community4.BaseRect.min - new TerrainCoord(num, num), community4.BaseRect.max + new TerrainCoord(num, num), list);
					foreach (TileObject item3 in list)
					{
						if ((item3 is Prop || item3 is SingleTileProp) && item3.GetCommunityId() == community3.Id && item3.GetCommunityThatOwnsThisArea() == community4)
						{
							item3.SetCommunity(community4);
						}
					}
				}
			}
		}
		if ((community1.CommunityType == CommunityType.Player && community2.HasAnyActiveMembers()) || (community2.CommunityType == CommunityType.Player && community1.HasAnyActiveMembers()))
		{
			if (showWarNotifications)
			{
				if (communityRelationshipType != CommunityRelationshipType.Hostile && relationshipType == CommunityRelationshipType.Hostile)
				{
					if (community1.DiscoverNameIfAtWar)
					{
						community1.CommunityNameKnown = true;
					}
					if (community2.DiscoverNameIfAtWar)
					{
						community2.CommunityNameKnown = true;
					}
					string statusBarMsg = LogEventBehaviour.BuildDeclaredWarMsg((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					HudBehaviour.Instance.SetStatusBarMsg(statusBarMsg);
					LogEvent logEvent = new LogEvent(LogEventType.DeclaredWar);
					logEvent.Community = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					Session.Instance.AddLogEvent(logEvent);
				}
				else if (communityRelationshipType != CommunityRelationshipType.Allied && relationshipType == CommunityRelationshipType.Allied)
				{
					string statusBarMsg2 = LogEventBehaviour.BuildMadeAllianceMsg((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					HudBehaviour.Instance.SetStatusBarMsg(statusBarMsg2);
					LogEvent logEvent2 = new LogEvent(LogEventType.MadeAlliance);
					logEvent2.Community = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					Session.Instance.AddLogEvent(logEvent2);
				}
				else if (communityRelationshipType == CommunityRelationshipType.Hostile && relationshipType != CommunityRelationshipType.Hostile)
				{
					string statusBarMsg3 = LogEventBehaviour.BuildDeclaredPeaceMsg((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					HudBehaviour.Instance.SetStatusBarMsg(statusBarMsg3);
					LogEvent logEvent3 = new LogEvent(LogEventType.DeclaredPeace);
					logEvent3.Community = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					Session.Instance.AddLogEvent(logEvent3);
				}
				else if (communityRelationshipType == CommunityRelationshipType.Allied && relationshipType != CommunityRelationshipType.Allied)
				{
					string statusBarMsg4 = LogEventBehaviour.BuildBrokeAllianceMsg((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					HudBehaviour.Instance.SetStatusBarMsg(statusBarMsg4);
					LogEvent logEvent4 = new LogEvent(LogEventType.BrokeAlliance);
					logEvent4.Community = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
					Session.Instance.AddLogEvent(logEvent4);
				}
			}
			Community community5 = ((community1.CommunityType == CommunityType.Player) ? community2 : community1);
			Community community6 = ((community1.CommunityType == CommunityType.Player) ? community1 : community2);
			if (community5.CommunityType == CommunityType.Looter || (community5.CommunityType == CommunityType.Normal && community5.IsFEMA))
			{
				if (communityRelationshipType != CommunityRelationshipType.Hostile && relationshipType == CommunityRelationshipType.Hostile)
				{
					StoryManager.Instance.ActivateInvader(Invaders_HiredMercs, community5, out var _);
					InvaderInstance invaderInstanceByUniqueID = StoryManager.Instance.GetInvaderInstanceByUniqueID(Invaders_HiredMercs, community5);
					if (invaderInstanceByUniqueID != null)
					{
						foreach (BaseObject createdObject in invaderInstanceByUniqueID.CreatedObjects)
						{
							if (createdObject is Community community7)
							{
								Session.Instance.CommunityManager.SetRelationship(community6, community7, CommunityRelationshipType.Hostile, showWarNotifications: false);
							}
						}
					}
				}
				else if (communityRelationshipType == CommunityRelationshipType.Hostile && relationshipType != CommunityRelationshipType.Hostile)
				{
					InvaderInstance invaderInstanceByUniqueID2 = StoryManager.Instance.GetInvaderInstanceByUniqueID(Invaders_HiredMercs, community5);
					if (invaderInstanceByUniqueID2 != null)
					{
						foreach (BaseObject createdObject2 in invaderInstanceByUniqueID2.CreatedObjects)
						{
							if (createdObject2 is Community community8)
							{
								Session.Instance.CommunityManager.SetRelationship(community6, community8, CommunityRelationshipType.Ceasefire, showWarNotifications: false);
							}
						}
					}
					StoryManager.Instance.DeactivateInvader(Invaders_HiredMercs, community5);
				}
			}
			if (communityRelationshipType != CommunityRelationshipType.Allied && relationshipType == CommunityRelationshipType.Allied)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.FormAlliance);
			}
		}
		if (communityRelationshipType == CommunityRelationshipType.Hostile && relationshipType != CommunityRelationshipType.Hostile)
		{
			community1.OnCeaseFire(community2);
			community2.OnCeaseFire(community1);
		}
		if (relationshipType != CommunityRelationshipType.Hostile && relationshipType != CommunityRelationshipType.Ceasefire)
		{
			return;
		}
		for (int i = 0; i < community1.CachedAllies.Count; i++)
		{
			Community community9 = community1.CachedAllies[i];
			if (GetRelationship(community9, community2) != relationshipType)
			{
				SetRelationship(community9, community2, relationshipType);
			}
		}
		for (int j = 0; j < community2.CachedAllies.Count; j++)
		{
			Community community10 = community2.CachedAllies[j];
			if (GetRelationship(community10, community1) != relationshipType)
			{
				SetRelationship(community10, community1, relationshipType);
			}
		}
	}

	public int GetNumCommunitiesWithRelationship(Community community, CommunityRelationshipType relationshipType)
	{
		int num = 0;
		for (int i = 0; i < community.CommunityRelationships.Count; i++)
		{
			if (community.CommunityRelationships[i].RelationshipType == relationshipType)
			{
				num++;
			}
		}
		return num;
	}

	public List<Community> GetListOfCommunitiesWithRelationship(Community community, CommunityRelationshipType relationshipType)
	{
		List<Community> list = new List<Community>();
		for (int i = 0; i < community.CommunityRelationships.Count; i++)
		{
			if (community.CommunityRelationships[i].RelationshipType == relationshipType)
			{
				list.Add(community.CommunityRelationships[i].GetOtherCommunity());
			}
		}
		return list;
	}

	public void OnNewGame()
	{
		RepopulateCountdown = TimeSpan.FromSeconds(Session.Instance.DifficultySettings.SurvivorRepopulationDays * Sun.DayLengthSecs);
		foreach (Community community in Communities)
		{
			community.OnNewGame();
		}
	}

	public void OnLoad(int version)
	{
		for (int num = Communities.Count - 1; num >= 0; num--)
		{
			Community community = Communities[num];
			if (community.KeptAroundForReferences && !IsAnyoneImportantReferencingObject(community))
			{
				community.Delete();
			}
			else
			{
				if (version < 614 && community.Leader != null)
				{
					community.LastLeaderName = community.Leader.FirstName;
				}
				if (version < 312 && (community.IsAISettlement() || community.CommunityType == CommunityType.Player))
				{
					PropPrototype[] array = new PropPrototype[4]
					{
						PropPrototype.Maize,
						PropPrototype.PumpkinPlant,
						PropPrototype.CucumberPlant,
						PropPrototype.PepperPlant
					};
					foreach (PropPrototype propPrototype in array)
					{
						if (propPrototype == null || propPrototype.HarvestSeedsPrototype == null || propPrototype.HarvestPrototype == null)
						{
							continue;
						}
						int num2 = community.GetCropTileCount(propPrototype) * 2;
						if (num2 <= 0)
						{
							continue;
						}
						float weight = propPrototype.HarvestSeedsPrototype.Weight * (float)num2;
						Prop prop = null;
						float num3 = 0f;
						foreach (Prop building in community.Buildings)
						{
							float num4 = building.GetMaxInventoryWeight() + (float)building.Inventory.CountItemsOfType(propPrototype.HarvestPrototype) * 10f;
							if (num4 > num3 && building.HasInventorySpaceFor(weight))
							{
								prop = building;
								num3 = num4;
							}
						}
						if (prop != null)
						{
							Equipment equipment = Equipment.Spawn(propPrototype.HarvestSeedsPrototype, num2);
							prop.Inventory.Add(prop, equipment);
						}
					}
				}
				foreach (Character member in community.Members)
				{
					if (member.Disappeared && member.SquadId != 0)
					{
						community.RemoveFromSquad(member);
					}
				}
				foreach (Squad squad in community.Squads)
				{
					if (squad.WantMapIcon())
					{
						Session.Instance.CommunityManager.AddSquadWithMapIcons(squad);
					}
				}
			}
		}
		using (new StopWatchMarker("BuildCommunityAreaOwner"))
		{
			GameTerrain instance = GameTerrain.Instance;
			instance.BuildCommunityAreaOwner(new TerrainRect(new TerrainCoord(0, 0), new TerrainCoord(instance.Size, instance.Size)));
		}
	}

	public void OnCharacterDeletedOrDisappeared(Character deletedCharacter)
	{
		foreach (Community community in Communities)
		{
			foreach (Threat threat in community.Threats)
			{
				threat.ThreatMembers.Remove(deletedCharacter);
			}
			community.RemoveFailedToBury(deletedCharacter);
		}
	}

	public void Update(TimeSpan dt)
	{
		if (Communities.Count > 0)
		{
			CurUpdateIndex = (CurUpdateIndex + 1) % Communities.Count;
			Communities[CurUpdateIndex].Update();
			if (Communities[CurUpdateIndex].DeleteMe)
			{
				if (RemoveCommunity(Communities[CurUpdateIndex]))
				{
					CurUpdateIndex = ((Communities.Count > 0) ? ((CurUpdateIndex + Communities.Count - 1) % Communities.Count) : 0);
				}
				else
				{
					Communities[CurUpdateIndex].DeleteMe = false;
				}
			}
		}
		UpdateAmbientEnemies(dt, forceFinishTask: false);
		UpdateRepopulation(dt);
	}

	public void UpdateRepopulation(TimeSpan dt)
	{
		RepopulateCountdown -= dt;
		if (!(RepopulateCountdown < TimeSpan.Zero))
		{
			return;
		}
		RepopulateCandidates.Clear();
		RepopulateChickenCandidates.Clear();
		foreach (Community community3 in Communities)
		{
			if (community3.CommunityType != CommunityType.Normal && community3.CommunityType != CommunityType.Looter && (community3.CommunityType != CommunityType.RovingTrader || community3.Leader == null || !community3.Leader.AliveAndNotZombie || !community3.Leader.HasRole(Role.Trader)))
			{
				continue;
			}
			int livingNonZombieMemberCount = community3.GetLivingNonZombieMemberCount();
			if (livingNonZombieMemberCount <= 0)
			{
				continue;
			}
			int num = ((community3.CommunityType != CommunityType.RovingTrader) ? (Math.Min(community3.InitialMemberCount, community3.GetAccommodation()) - livingNonZombieMemberCount) : (community3.InitialMemberCount - livingNonZombieMemberCount));
			if (num > 0)
			{
				RepopulateCandidates.Add(community3);
			}
			if (community3.InitialChickenCount > 0)
			{
				int chickenCountIncludingEggs = community3.GetChickenCountIncludingEggs();
				if (Math.Min(community3.InitialChickenCount, community3.GetChickenAccommodation()) - chickenCountIncludingEggs > 0)
				{
					RepopulateChickenCandidates.Add(community3);
				}
			}
		}
		bool flag = false;
		int num2 = RepopulateCandidates.Count + RepopulateChickenCandidates.Count;
		if (num2 > 0)
		{
			flag = true;
			CustomRandom deterministicRand = Session.Instance.DeterministicRand;
			if (deterministicRand.RandomChoice((float)RepopulateChickenCandidates.Count / (float)num2))
			{
				int index = deterministicRand.Next(RepopulateChickenCandidates.Count);
				Community community = RepopulateChickenCandidates[index];
				for (int i = 0; i < 100; i++)
				{
					RepopulateCandidateBuildings.Clear();
					foreach (Prop building in community.Buildings)
					{
						if (building.Prototype.EnterableBySpecies == BaseObjectType.Chicken)
						{
							RepopulateCandidateBuildings.Add(building);
						}
					}
					if (RepopulateCandidateBuildings.Count > 0)
					{
						int index2 = deterministicRand.Next(RepopulateCandidateBuildings.Count);
						Prop prop = RepopulateCandidateBuildings[index2];
						TerrainCoord tile = deterministicRand.RandomTileOnOutsideEdge(prop.GetMinTile(), prop.GetMaxTile(), 2);
						if (!Session.Instance.IsWithinRangeOfMainCameraDeterministic(GameTerrain.Instance.GetTileCentreXZ(tile), 64f) && !GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, null, null))
						{
							GameTerrain.GenerateChicken(community, tile, deterministicRand).SetGoal(new AnimalGoal());
							flag = false;
							break;
						}
					}
				}
			}
			else
			{
				int index3 = deterministicRand.Next(RepopulateCandidates.Count);
				Community community2 = RepopulateCandidates[index3];
				for (int j = 0; j < 100; j++)
				{
					TerrainCoord terrainCoord = TerrainCoord.Invalid;
					RepopulateCandidateBuildings.Clear();
					if (community2.CommunityType == CommunityType.RovingTrader)
					{
						for (int k = 0; k < 100; k++)
						{
							TerrainCoord terrainCoord2 = community2.Leader.Tile + deterministicRand.RandomTile(new TerrainCoord(-8, -8), new TerrainCoord(8, 8));
							if (!Session.Instance.IsWithinRangeOfMainCameraDeterministic(GameTerrain.Instance.GetTileCentreXZ(terrainCoord2), 64f) && !GameTerrain.Instance.IsImpassable(terrainCoord2.x, terrainCoord2.y, 2051, null, null))
							{
								terrainCoord = terrainCoord2;
								break;
							}
						}
					}
					else
					{
						foreach (Prop building2 in community2.Buildings)
						{
							if (building2.IsAccommodation())
							{
								RepopulateCandidateBuildings.Add(building2);
							}
						}
						if (RepopulateCandidateBuildings.Count > 0)
						{
							for (int l = 0; l < 100; l++)
							{
								int index4 = deterministicRand.Next(RepopulateCandidateBuildings.Count);
								Prop prop2 = RepopulateCandidateBuildings[index4];
								TerrainCoord terrainCoord3 = deterministicRand.RandomTileOnOutsideEdge(prop2.GetMinTile(), prop2.GetMaxTile(), 8);
								if (!community2.PrisonRect.Contains(terrainCoord3) && !Session.Instance.IsWithinRangeOfMainCameraDeterministic(GameTerrain.Instance.GetTileCentreXZ(terrainCoord3), 64f) && !GameTerrain.Instance.IsImpassable(terrainCoord3.x, terrainCoord3.y, 2051, null, null))
								{
									terrainCoord = terrainCoord3;
									break;
								}
							}
						}
					}
					if (!(terrainCoord != TerrainCoord.Invalid))
					{
						continue;
					}
					Character character = GameTerrain.GenerateCharacter(community2, terrainCoord, deterministicRand);
					character.SetGoal(new SurvivorGoal());
					if (community2.CommunityType == CommunityType.RovingTrader && community2.Squads.Count > 0)
					{
						Squad squad = community2.Squads[0];
						community2.AddToSquad(character, squad);
					}
					community2.UpdateRoles(character);
					if (community2.KeyProtoForAllNewSpawns != null)
					{
						character.Inventory.Add(character, Equipment.Spawn(community2.KeyProtoForAllNewSpawns));
					}
					GameTerrain.GenerateCharacterEquipment(character, community2.IsLooterCommunity() ? PersonalityGroup.LooterFaction : PersonalityGroup.NormalFaction, deterministicRand);
					character.InitialBandageCount = character.Inventory.CountItemsOfType(EquipmentPrototype.Bandage);
					character.RandomizeInvisibleStrain(TemplateHasInvisibleStrain.Default, deterministicRand);
					int num3 = deterministicRand.Next(RepopulateRelationshipTypes.Length);
					RelationshipType relationshipType = RepopulateRelationshipTypes[num3];
					if (relationshipType != RelationshipType.None)
					{
						RepopulateCandidateMembersForRelationship.Clear();
						foreach (Character member in community2.Members)
						{
							if (member != character && member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human)
							{
								RepopulateCandidateMembersForRelationship.Add(member);
							}
						}
						if (RepopulateCandidateMembersForRelationship.Count > 0)
						{
							int index5 = deterministicRand.Next(RepopulateCandidateMembersForRelationship.Count);
							Character character2 = RepopulateCandidateMembersForRelationship[index5];
							if (Relationship.SetRelationship(character, relationshipType, character2, allowChange: false, generating: false))
							{
								if (relationshipType == RelationshipType.InLoveWith && deterministicRand.RandomChoice(Relationship.ProbabilityOfLoveBeingReciprocated))
								{
									Relationship.SetRelationship(character2, RelationshipType.InLoveWith, character, allowChange: true, generating: false);
								}
								Relationship.RandomizeRelationshipApprovalRespect(character, character2, deterministicRand);
								Relationship.RandomizeRelationshipApprovalRespect(character2, character, deterministicRand);
							}
						}
						RepopulateCandidateMembersForRelationship.Clear();
					}
					flag = false;
					break;
				}
			}
		}
		RepopulateCountdown += (flag ? TimeSpan.FromSeconds(10.0) : TimeSpan.FromSeconds(Session.Instance.DifficultySettings.SurvivorRepopulationDays * Sun.DayLengthSecs));
		RepopulateCandidates.Clear();
		RepopulateChickenCandidates.Clear();
		RepopulateCandidateBuildings.Clear();
		RepopulateCandidateMembersForRelationship.Clear();
	}

	public Town GetTownForTile(TerrainCoord tile, float extra)
	{
		Town result = null;
		float num = float.MaxValue;
		foreach (Town town in Towns)
		{
			float distSquared = town.Tile.GetDistSquared(tile);
			if (distSquared < num && distSquared < MathUtil.Squared(town.Radius + extra))
			{
				result = town;
				num = distSquared;
			}
		}
		return result;
	}

	public Town GetVisitedTownForTile(TerrainCoord tile)
	{
		Town result = null;
		float num = float.MaxValue;
		foreach (Town town in Towns)
		{
			if (town.Visited)
			{
				float distSquared = town.Tile.GetDistSquared(tile);
				if (distSquared < num && distSquared < town.Radius * town.Radius)
				{
					result = town;
					num = distSquared;
				}
			}
		}
		return result;
	}

	public Town GetClosestTown(TerrainCoord tile)
	{
		Town result = null;
		float num = float.MaxValue;
		foreach (Town town in Towns)
		{
			float distSquared = town.Tile.GetDistSquared(tile);
			if (distSquared < num)
			{
				result = town;
				num = distSquared;
			}
		}
		return result;
	}

	public BaseObject GetClosestTownOrSettlement(TerrainCoord tile)
	{
		BaseObject result = null;
		float num = float.MaxValue;
		foreach (Town town in Towns)
		{
			float distSquared = town.Tile.GetDistSquared(tile);
			if (distSquared < num)
			{
				result = town;
				num = distSquared;
			}
		}
		foreach (Community community in Communities)
		{
			if (community.IsAISettlement())
			{
				float closestDistSqTo = community.BaseRect.GetClosestDistSqTo(tile);
				if (closestDistSqTo < num)
				{
					result = community;
					num = closestDistSqTo;
				}
			}
		}
		return result;
	}

	public float GetClosestTownDistSq(TerrainCoord tile)
	{
		float num = float.MaxValue;
		foreach (Town town in Towns)
		{
			num = Math.Min(num, town.Tile.GetDistSquared(tile));
		}
		return num;
	}

	public float GetClosestDeerDistSq(TerrainCoord tile)
	{
		float num = float.MaxValue;
		foreach (DeerSpawnPoint deerSpawnPoint in DeerSpawnPoints)
		{
			num = Math.Min(num, deerSpawnPoint.Tile.GetDistSquared(tile));
		}
		return num;
	}

	public bool HasDiscoveredDeer()
	{
		foreach (DeerSpawnPoint deerSpawnPoint in DeerSpawnPoints)
		{
			if (deerSpawnPoint.Discovered)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInRangeOfDiscoveredDeerSpawnpoint(TerrainCoord tile, float range)
	{
		foreach (DeerSpawnPoint deerSpawnPoint in DeerSpawnPoints)
		{
			if (deerSpawnPoint.Discovered && deerSpawnPoint.HerdSize > 0 && deerSpawnPoint.Tile.GetDistSquared(tile) <= range * range && deerSpawnPoint.CanSpawnOnTile())
			{
				return true;
			}
		}
		return false;
	}

	public void TakeFromRespawnBuildup()
	{
		AmbientRespawnBuildup -= 1f;
	}

	public void GiveBackToRespawnBuildup()
	{
		AmbientRespawnBuildup += 1f;
	}

	public Status GetAmbientEnemiesUpdateStatus()
	{
		return _status;
	}

	public void CachePlayerCommunityTiles(List<TileAndRadius> cachedPlayerCommunityTiles)
	{
		cachedPlayerCommunityTiles.Clear();
		foreach (Character member in PlayerCommunity.Members)
		{
			if (member.AliveAndNotZombie)
			{
				cachedPlayerCommunityTiles.Add(new TileAndRadius(member.Tile, (float)member.GetSightRange() + 1f));
			}
		}
	}

	public float GetMinDistSqrCachedPlayerCommunityTiles(List<TileAndRadius> cachedPlayerCommunityTiles, TerrainCoord tile, float sightRangePadding, ref bool inSightRange)
	{
		float num = float.MaxValue;
		for (int i = 0; i < cachedPlayerCommunityTiles.Count; i++)
		{
			float distSquared = tile.GetDistSquared(cachedPlayerCommunityTiles[i].Tile);
			float num2 = cachedPlayerCommunityTiles[i].Radius + sightRangePadding;
			inSightRange |= distSquared <= num2 * num2;
			num = Math.Min(distSquared, num);
		}
		return num;
	}

	private void ThreadedUpdateAmbientEnemiesTaskFunc(BaseTaskData data)
	{
		_success = ThreadedUpdateAmbientEnemies();
		_threadedAmbientEnemiesTaskFinishedEvent.Set();
	}

	private bool ThreadedUpdateAmbientEnemies()
	{
		if (GameTerrain.Instance.CalcEnclosedAreas())
		{
			WantRebuildMinimap |= TerrainEditor.ShowImpassable;
		}
		BestSpawnPoint = null;
		float num = float.MaxValue;
		for (int i = 0; i < AmbientEnemySpawnPoints.Count; i++)
		{
			using (new UnityProfileMarker(AmbientEnemySpawnPointsStr))
			{
				AmbientEnemySpawnPoint ambientEnemySpawnPoint = AmbientEnemySpawnPoints[i];
				if (ambientEnemySpawnPoint.CanSpawn(RespawnsAllowed, SessionTime))
				{
					bool inSightRange = false;
					float minDistSqrCachedPlayerCommunityTiles = GetMinDistSqrCachedPlayerCommunityTiles(CachedPlayerCommunityTiles, ambientEnemySpawnPoint.Tile, 1f, ref inSightRange);
					if (!inSightRange && minDistSqrCachedPlayerCommunityTiles < num)
					{
						BestSpawnPoint = ambientEnemySpawnPoint;
						num = minDistSqrCachedPlayerCommunityTiles;
					}
				}
			}
		}
		if (BestSpawnPoint == null)
		{
			return false;
		}
		if (AmbientEnemyGroupCount >= MaxAmbientEnemyGroups)
		{
			if (WorstEnemyGroup == null)
			{
				return false;
			}
			float num2 = (float)Math.Sqrt(num);
			if ((float)Math.Sqrt(WorstEnemyGroupDistSqr) <= num2 + 10f)
			{
				return false;
			}
		}
		else
		{
			WorstEnemyGroup = null;
		}
		return true;
	}

	private void KickOffThreadedAmbientEnemiesTask()
	{
		SessionTime = Session.Instance.PlayTime;
		InputFrameWhenTaskStarted = Session.Instance.InputFrame;
		CachePlayerCommunityTiles(CachedPlayerCommunityTiles);
		RespawnsAllowed = AmbientRespawnBuildup >= 1f;
		foreach (AmbientEnemySpawnPoint item in PretendDied)
		{
			item.LastDiedTime = (float)Session.Instance.PlayTime.TotalSeconds;
		}
		PretendDied.Clear();
		WorstEnemyGroup = null;
		WorstEnemyGroupAllMembersDead = false;
		WorstEnemyGroupDistSqr = float.MinValue;
		AmbientEnemyGroupCount = 0;
		for (int i = 0; i < Communities.Count; i++)
		{
			Community community = Communities[i];
			if (community.SpawnPoint == null)
			{
				continue;
			}
			AmbientEnemyGroupCount++;
			bool flag = false;
			bool flag2 = true;
			bool flag3 = true;
			bool flag4 = true;
			float num = 0f;
			float num2 = float.MaxValue;
			foreach (Character member in community.Members)
			{
				bool flag5 = !member.Alive;
				flag2 = flag2 && flag5;
				flag4 &= member.CarriedBy == null;
				if (flag5)
				{
					num = Math.Max(num, (float)member.TimeOfDeath.TotalSeconds);
				}
				bool inSightRange = false;
				num2 = Math.Min(num2, GetMinDistSqrCachedPlayerCommunityTiles(CachedPlayerCommunityTiles, member.Tile, 1f, ref inSightRange));
				flag = flag || inSightRange;
				if (inSightRange)
				{
					flag3 = flag3 && flag5;
				}
			}
			if (!flag4)
			{
				continue;
			}
			bool flag6 = false;
			if (flag3 && num > 0f)
			{
				float num3 = (float)SessionTime.TotalSeconds - num;
				if (num3 >= 600f)
				{
					num2 += 1048576f + num3 * num3;
					flag6 = true;
				}
			}
			if ((!flag || flag6) && num2 > WorstEnemyGroupDistSqr)
			{
				WorstEnemyGroup = community;
				WorstEnemyGroupDistSqr = num2;
				WorstEnemyGroupAllMembersDead = flag2;
			}
		}
		RespawnsAllowed |= WorstEnemyGroup != null && WorstEnemyGroup.SpawnPoint.HasSpawnedSomeoneWhoDied() && !WorstEnemyGroupAllMembersDead;
		foreach (Town town in Towns)
		{
			bool inSightRange2 = false;
			bool num4 = GetMinDistSqrCachedPlayerCommunityTiles(CachedPlayerCommunityTiles, town.Tile, 0f, ref inSightRange2) <= town.Radius * town.Radius;
			if (num4 && !town.Visited)
			{
				town.Visited = true;
				town.IgnoreForQuests = false;
				HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate("HINT_TownDiscovered").Replace("%1", town.GetDisplayNameString()));
			}
			if (num4)
			{
				bool newlyActivated = false;
				switch (town.Infection)
				{
				case InfectionType.Green:
					StoryManager.Instance.ActivateInvader(Invaders_Town_GreenStrain, town, out newlyActivated);
					break;
				case InfectionType.Blue:
					StoryManager.Instance.ActivateInvader(Invaders_Town_BlueStrain, town, out newlyActivated);
					break;
				case InfectionType.Red:
					StoryManager.Instance.ActivateInvader(Invaders_Town_RedStrain, town, out newlyActivated);
					break;
				case InfectionType.White:
					StoryManager.Instance.ActivateInvader(Invaders_Town_WhiteStrain, town, out newlyActivated);
					break;
				}
				if (!string.IsNullOrEmpty(town.InvaderName))
				{
					StoryManager.Instance.ActivateInvader(town.InvaderName, town, out newlyActivated);
				}
				if (newlyActivated)
				{
					HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate("HINT_TownEntered").Replace("%1", town.GetDisplayNameString()));
				}
			}
		}
		_status = Status.InProgress;
		_threadedAmbientEnemiesTaskFinishedEvent.Reset();
		GameTerrain.Instance.CopyTilesForThread();
		GameImpl.Instance.UpdateThreadPool.AddTask(_threadedAmbientEnemiesTaskFunc, null, null, TaskPriority.Low);
	}

	public void WaitForThreadedAmbientEnemiesTask(int millisecondsTimeout)
	{
		if (_status == Status.InProgress && _threadedAmbientEnemiesTaskFinishedEvent.WaitOne(millisecondsTimeout))
		{
			_status = (_success ? Status.Success : Status.Fail);
		}
	}

	public int GetNumLivingHunterGroups()
	{
		int num = 0;
		foreach (BaseObject hunter in Hunters)
		{
			if (hunter is Community community)
			{
				if (!community.HasAnyActiveMembers())
				{
					continue;
				}
				if (community.CommunityType != CommunityType.RovingTrader)
				{
					bool flag = false;
					foreach (Squad squad in community.Squads)
					{
						if (squad.Behaviour == SquadBehaviour.Trade && squad.Members.Count > 0)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						continue;
					}
				}
				num++;
			}
			else if (hunter is TileObject tileObject)
			{
				if (!tileObject.IsDestroyed() && !tileObject.Deleted)
				{
					num++;
				}
			}
			else if (hunter != null && !hunter.Deleted)
			{
				num++;
			}
		}
		return num;
	}

	public int GetMaxLivingHunterGroups()
	{
		return Mathf.CeilToInt((float)(GameTerrain.Instance.Size / 128) * (Session.Instance.DifficultySettings.GetTotalHunterDensity() / 100f));
	}

	public bool CanSpawnHunters()
	{
		if (GetNumLivingHunterGroups() < GetMaxLivingHunterGroups())
		{
			return true;
		}
		return false;
	}

	public bool CanDeleteHunters(BaseObject hunters)
	{
		if (hunters is Community community)
		{
			if (community.Members.Count == 0)
			{
				return true;
			}
			if (!community.HasAnyActiveMembers())
			{
				if (Session.Instance.PlayTime - community.GetTimeWhenLastMemberDied() >= TimeSpan.FromSeconds(600.0))
				{
					return !community.AreAnyMembersInMainView();
				}
			}
			else if ((community.CommunityType == CommunityType.HunterZombie || community.CommunityType == CommunityType.HunterLooter || community.CommunityType == CommunityType.HunterMercenary || community.CommunityType == CommunityType.RovingRefugee) && Session.Instance.PlayTime - community.SpawnedTime >= TimeSpan.FromSeconds(Sun.DayLengthSecs * 7f) && !community.IsAnyMemberInCombat() && !community.AreAnyMembersInMainView())
			{
				return true;
			}
			if (community.Squads.Count > 0 && community.Squads[0].Action == SquadAction.ExitMap)
			{
				return !community.AreAnyMembersInMainView();
			}
		}
		if (hunters is TileObject tileObject)
		{
			return tileObject.IsDestroyed();
		}
		return hunters.Deleted;
	}

	public static bool IsAnyoneImportantReferencingObject(BaseObject obj)
	{
		foreach (Character character in Session.Instance.CharacterManager.Characters)
		{
			if (character.Community == obj)
			{
				return true;
			}
			if (character.InitialCommunity == obj)
			{
				return true;
			}
			if (!character.IsAmbient() && character.AliveAndNotZombie && character.HasMemoryOfObject(obj))
			{
				return true;
			}
		}
		foreach (KeyValuePair<int, QuestInstance> questInstance in StoryManager.Instance.QuestInstances)
		{
			if (questInstance.Value.HasReferenceTo(obj))
			{
				return true;
			}
		}
		for (int i = 0; i < StoryManager.Instance.EnabledTriggers.Length; i++)
		{
			List<EnabledTrigger> list = StoryManager.Instance.EnabledTriggers[i];
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Actor == obj)
				{
					return true;
				}
				if (list[j].Object == obj)
				{
					return true;
				}
			}
		}
		foreach (InvaderInstance activeInvader in StoryManager.Instance.ActiveInvaders)
		{
			if (activeInvader.SourceObject == obj)
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveCommunity(Community community)
	{
		bool flag = false;
		CommunityType communityType = community.CommunityType;
		if (communityType == CommunityType.AmbientZombie || communityType == CommunityType.AmbientAnimal || communityType == CommunityType.TemporaryZombie)
		{
			flag = true;
		}
		while (community.Buildings.Count > 0)
		{
			community.Buildings[0].Delete();
		}
		for (int num = community.Members.Count - 1; num >= 0; num--)
		{
			Character character = community.Members[num];
			if (!flag && IsAnyoneImportantReferencingObject(character))
			{
				character.Disappear(fromGoal: false);
				character.KeptAroundForReferences = true;
			}
			else
			{
				int count = community.Members.Count;
				character.Delete();
				if (community.Members.Count >= count)
				{
					Debug.LogWarning(community.GetDisplayNameString() + " (" + community.CommunityType.ToString() + ") members didn't decrease after deleting " + character.GetDisplayNameString() + " (was " + count + ", now: " + community.Members.Count + ")");
				}
			}
		}
		if (!flag && IsAnyoneImportantReferencingObject(community))
		{
			community.KeptAroundForReferences = true;
			return false;
		}
		community.Delete();
		return true;
	}

	public static ControlPoint? PickRandomMapEntrance(CustomRandom rand)
	{
		RoadCandidates.Clear();
		GameTerrain instance = GameTerrain.Instance;
		foreach (TerrainPath path in instance.Paths)
		{
			if (!path.IsRiver && path.ControlPoints.Count >= 3 && !instance.DisabledEntrancePathIndices.Contains(path.Index))
			{
				TerrainCoord terrainCoord = instance.ClampTileWithinBounds(instance.GetTileCoordForPosXZ(path.ControlPoints[0].Pos));
				if (path.EntrancePointIndex >= 0 && path.EntrancePointIndex < path.ControlPoints.Count && instance.IsTileOnEdge(terrainCoord.x, terrainCoord.y))
				{
					RoadCandidates.Add(path.ControlPoints[path.EntrancePointIndex]);
				}
				TerrainCoord terrainCoord2 = instance.ClampTileWithinBounds(instance.GetTileCoordForPosXZ(path.ControlPoints[path.ControlPoints.Count - 1].Pos));
				if (path.ExitPointIndex >= 0 && path.ExitPointIndex < path.ControlPoints.Count && instance.IsTileOnEdge(terrainCoord2.x, terrainCoord2.y))
				{
					ControlPoint item = path.ControlPoints[path.ExitPointIndex];
					item.Dir = -item.Dir;
					RoadCandidates.Add(item);
				}
			}
		}
		if (RoadCandidates.Count > 1)
		{
			int index = rand.Next() % RoadCandidates.Count;
			return RoadCandidates[index];
		}
		return null;
	}

	public void DeleteHunters(BaseObject hunters)
	{
		InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(hunters);
		if (invaderInstanceThatCreatedHunter != null)
		{
			StoryManager.QueueEvents(invaderInstanceThatCreatedHunter.Invader.OnDeletedEvents, null, null, hunters, new MemoryParam(invaderInstanceThatCreatedHunter.SourceObject));
			StoryManager.Instance.TriggerQueuedEvents();
		}
		StoryManager.Instance.RemoveFromInvaderInstance(hunters);
		_ = Hunters.Count;
		if (hunters is Community community)
		{
			if (!community.IsZombieCommunity() && !community.IsAnimalCommunity() && community.HasAnyLivingNonZombieMembers() && community.HasAnyoneInCommunityBeenInjuredBy(PlayerCommunity))
			{
				Memory.OnMemorableEvent(MemoryPrototype.LeadershipDefeated, PlayerCommunity.Leader, community, (float)community.InitialMemberCount * Community.LeadershipDefeatedQuantityFactor, SecrecyMode.OnlyKnownToSubjectCommunity);
			}
			RemoveCommunity(community);
		}
		else
		{
			hunters.Delete();
		}
		Hunters.Remove(hunters);
	}

	public void DeleteAmbientEnemy(Community community)
	{
		community.SpawnPoint.OnSpawnedEnemyDeleted();
		RemoveCommunity(community);
	}

	public void UpdateAmbientEnemies(TimeSpan dt, bool forceFinishTask)
	{
		Session instance = Session.Instance;
		AmbientRespawnBuildup += (float)dt.TotalSeconds / (instance.DifficultySettings.ZombieRespawnDays * Sun.DayLengthSecs);
		if (_status == Status.None && !forceFinishTask && instance.PlayTime - LastAmbientEnemiesUpdate > AmbientEnemiesUpdateFrequency && instance.CountdownToTimeJump == 0)
		{
			KickOffThreadedAmbientEnemiesTask();
		}
		if (_status == Status.None)
		{
			return;
		}
		if (forceFinishTask)
		{
			WaitForThreadedAmbientEnemiesTask(-1);
		}
		else if (instance.IsInMultiplayerGame())
		{
			if (instance.InputFrame < InputFrameWhenTaskStarted + 60)
			{
				return;
			}
			WaitForThreadedAmbientEnemiesTask(-1);
		}
		else
		{
			WaitForThreadedAmbientEnemiesTask(0);
		}
		if (_status == Status.InProgress)
		{
			return;
		}
		GameTerrain.Instance.CopyTilesFromThread();
		LastAmbientEnemiesUpdate = Session.Instance.PlayTime;
		for (int num = Hunters.Count - 1; num >= 0; num--)
		{
			BaseObject hunters = Hunters[num];
			if (CanDeleteHunters(hunters))
			{
				DeleteHunters(hunters);
			}
		}
		if (CanSpawnHunters())
		{
			_ = GameTerrain.Instance;
			CustomRandom deterministicRand = instance.DeterministicRand;
			InvaderInstance invaderInstance = StoryManager.Instance.PickInvader(deterministicRand, NumFreebieInvaders > 0);
			if (invaderInstance != null)
			{
				NumFreebieInvaders = Math.Max(0, NumFreebieInvaders - 1);
				BaseObject baseObject = invaderInstance.SpawnInvader();
				if (baseObject != null)
				{
					Hunters.Add(baseObject);
				}
			}
		}
		bool num2 = _status == Status.Success;
		_status = Status.None;
		if (num2)
		{
			if (WorstEnemyGroup != null)
			{
				DeleteAmbientEnemy(WorstEnemyGroup);
				WorstEnemyGroup = null;
			}
			if (AmbientRespawnBuildup >= 1f || !BestSpawnPoint.HasSpawnedSomeoneWhoDied())
			{
				BestSpawnPoint.SpawnEnemyGroup();
			}
		}
		if (WantRebuildMinimap)
		{
			GameTerrain.Instance.BuildEntireMinimap();
			WantRebuildMinimap = false;
		}
	}

	public float GetCommunityAggro()
	{
		return Mathf.Clamp((float)(PlayerCommunity.GetLivingNonZombieMemberCount() - 1) + (float)PlayerCommunity.GetNumTallStructures(), 0f, MaxAggro);
	}

	public int GetVisitedTownCount()
	{
		int num = 0;
		foreach (Town town in Towns)
		{
			if (town.Visited)
			{
				num++;
			}
		}
		return num;
	}

	private float GetDistSqrToNearestPlayerCommunityMember(Vector2 posXZ)
	{
		float num = float.MaxValue;
		List<Character> members = PlayerCommunity.Members;
		for (int i = 0; i < members.Count; i++)
		{
			Character character = members[i];
			if (character.AliveAndNotZombie)
			{
				num = Math.Min(num, (character.PosXZ - posXZ).sqrMagnitude);
			}
		}
		return num;
	}

	public bool IsWithinRangeOfHunters(TerrainCoord tile, float range)
	{
		foreach (BaseObject hunter in Hunters)
		{
			if (hunter is Community community)
			{
				if (community.IsWithinRangeOfAnyLivingNonZombieMember(tile, range))
				{
					return true;
				}
			}
			else if (hunter is TileObject tileObject && tileObject.GetCentreTile().GetDist(tile) <= range)
			{
				return true;
			}
		}
		return false;
	}

	public Community FindCommunityWithSameGangName(Community community)
	{
		foreach (Community community2 in Communities)
		{
			if (community2 != community && community2.CommunityName.IsEqual(community.CommunityName))
			{
				return community2;
			}
		}
		return null;
	}

	public Community FindCommunityTryingToOccupyBase(Community community)
	{
		foreach (Community community2 in Communities)
		{
			if (community2 == community)
			{
				continue;
			}
			foreach (Squad squad in community2.Squads)
			{
				if (squad.Behaviour == SquadBehaviour.Occupy && squad.EnemyCommunityId == community.Id)
				{
					return community2;
				}
			}
		}
		return null;
	}

	public Town FindTownWithSameName(Town town)
	{
		foreach (Town town2 in Towns)
		{
			if (town2 != town && town2.TownName.IsEqual(town.TownName))
			{
				return town2;
			}
		}
		return null;
	}
}
