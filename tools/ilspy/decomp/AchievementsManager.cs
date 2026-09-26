using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class AchievementsManager
{
	public static AchievementsManager Instance;

	public static string[] AchievementNames = StringUtil.GetEnumNames<Achievement>();

	public static string[] AchievementStatNames = new string[33];

	private Callback<UserStatsReceived_t> UserStatsReceivedCallback;

	private Callback<UserStatsStored_t> UserStatsStoredCallback;

	private Callback<UserAchievementStored_t> UserAchievementStoredCallback;

	public bool StatsInitialized;

	public bool WantStoreStats;

	public static List<string> SandboxMode = new List<string> { "BaseStory", "Common", "Sandbox" };

	public static List<string> StoryMode = new List<string> { "BaseStory", "Common", "MainStory" };

	public static bool AllowAchievementsWithMods = true;

	public void Init()
	{
		Instance = this;
		for (int i = 0; i < AchievementStatNames.Length; i++)
		{
			if (GetStatLimit((Achievement)i) > 0)
			{
				AchievementStatNames[i] = "Stat_" + AchievementNames[i];
			}
		}
		if (GameImpl.Instance.SteamInitialized)
		{
			UserStatsReceivedCallback = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
			UserStatsStoredCallback = Callback<UserStatsStored_t>.Create(OnUserStatsStored);
			UserAchievementStoredCallback = Callback<UserAchievementStored_t>.Create(OnUserAchievementStored);
			if (!SteamUserStats.RequestCurrentStats())
			{
				Debug.LogWarning("RequestCurrentStats failed!");
			}
		}
	}

	public void Unload()
	{
		Instance = null;
	}

	public void Update()
	{
	}

	public void OnLoadGame()
	{
		if (StatsInitialized && WantStoreStats && !SteamUserStats.StoreStats())
		{
			Debug.LogWarning("StoreStats failed!");
		}
	}

	public void OnUserStatsReceived(UserStatsReceived_t callback)
	{
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			StatsInitialized = true;
		}
		else
		{
			Debug.LogWarning("OnUserStatsReceived error: " + callback.m_eResult);
		}
	}

	public void OnUserStatsStored(UserStatsStored_t callback)
	{
		if (callback.m_eResult != EResult.k_EResultOK)
		{
			WantStoreStats = true;
		}
	}

	public void OnUserAchievementStored(UserAchievementStored_t callback)
	{
	}

	public void UnlockAchievement(Achievement achievement)
	{
		if (!IsAchievementAllowed(achievement) || !StatsInitialized)
		{
			return;
		}
		if (!SteamUserStats.GetAchievement(AchievementNames[(int)achievement], out var pbAchieved))
		{
			Debug.LogWarning("GetAchievement failed! " + achievement);
		}
		else if (!pbAchieved)
		{
			if (!SteamUserStats.SetAchievement(AchievementNames[(int)achievement]))
			{
				Debug.LogWarning("SetAchievement failed! " + achievement);
			}
			else if (!SteamUserStats.StoreStats())
			{
				Debug.LogWarning("StoreStats failed! " + achievement);
			}
		}
	}

	public void IncrementAchievementStat(Achievement achievement, int amount)
	{
		if (!IsAchievementAllowed(achievement))
		{
			return;
		}
		int statLimit = GetStatLimit(achievement);
		if (statLimit == 0 || !StatsInitialized)
		{
			return;
		}
		int pData = 0;
		bool pbAchieved;
		if (!SteamUserStats.GetStat(AchievementStatNames[(int)achievement], out pData))
		{
			Debug.LogWarning("GetStat failed! " + achievement);
		}
		else if (!SteamUserStats.GetAchievement(AchievementNames[(int)achievement], out pbAchieved))
		{
			Debug.LogWarning("GetAchievement failed! " + achievement);
		}
		else
		{
			if (pData >= statLimit && pbAchieved)
			{
				return;
			}
			pData += amount;
			if (!SteamUserStats.SetStat(AchievementStatNames[(int)achievement], pData))
			{
				Debug.LogWarning("SetStat failed! " + achievement);
			}
			else if (pData >= statLimit)
			{
				if (!SteamUserStats.SetAchievement(AchievementNames[(int)achievement]))
				{
					Debug.LogWarning("SetAchievement failed! " + achievement);
				}
				else if (!SteamUserStats.StoreStats())
				{
					Debug.LogWarning("StoreStats failed! " + achievement);
				}
			}
		}
	}

	public void ResetAchievements()
	{
		if (StatsInitialized && !SteamUserStats.ResetAllStats(bAchievementsToo: true))
		{
			Debug.LogWarning("ResetAllStats failed!");
		}
	}

	public int GetStatLimit(Achievement achievement)
	{
		return achievement switch
		{
			Achievement.InfectWithWeapon => 10, 
			Achievement.VehicleKills => 10, 
			Achievement.Craft_Cookies => 30, 
			Achievement.Craft_ArmorPiercingAmmo => 100, 
			Achievement.Story_DumpTruckKills => 20, 
			_ => 0, 
		};
	}

	public bool IsAchievementAllowed(Achievement achievement)
	{
		if (!Session.Instance.AchievementsEnabled)
		{
			return false;
		}
		GameImpl instance = GameImpl.Instance;
		switch (achievement)
		{
		case Achievement.CompleteSandbox_Evac:
		case Achievement.CompleteSandbox_Conquest:
		case Achievement.CompleteSandbox_HitTheRoad:
		case Achievement.CompleteSandbox_Hard_WithFriends:
		case Achievement.CompleteSandbox_Hard_LoneWolf:
			return instance.IsCurrentStoryListEqualTo(SandboxMode, AllowAchievementsWithMods);
		case Achievement.Story_RitzCreekWar:
		case Achievement.Story_FindRitzvillePasses:
		case Achievement.Story_FindCabinPeople:
		case Achievement.Story_FindBrainScanner:
		case Achievement.Story_DumpTruckKills:
		case Achievement.Story_RecruitJoeWheeler:
		case Achievement.Story_EvacEmma:
		case Achievement.CompleteStory_Evac:
		case Achievement.CompleteStory_SuicideMission:
		case Achievement.CompleteStory_HitTheRoad:
		case Achievement.CompleteStory_Hard:
			return instance.IsCurrentStoryListEqualTo(StoryMode, AllowAchievementsWithMods);
		default:
			if (!AllowAchievementsWithMods && !instance.IsCurrentStoryListEqualTo(SandboxMode))
			{
				return instance.IsCurrentStoryListEqualTo(StoryMode);
			}
			return true;
		}
	}
}
