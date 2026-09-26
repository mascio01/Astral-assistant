using System.Collections.Generic;
using UnityEngine;

public class MusicManager
{
	public MusicGroup[] MusicGroupForSituation = new MusicGroup[13];

	public List<MusicPlayer> AllPlayingMusic = new List<MusicPlayer>();

	public MusicPlayer CurrentMusic;

	private static List<Character> NearbyCharacters = new List<Character>();

	public static List<RadioProp> NearbyRadios = new List<RadioProp>();

	public static string MusicDir = "Sounds/Music/";

	private static string MusicObjectName = "Music";

	private static float BaseManagementDist = 16f;

	private static float EnemiesNearbyHysteresisDist = 8f;

	private static int EnemiesNearbyCheckDist = 32;

	private static float MusicBaseVolume = 0.5f;

	public void Init()
	{
		MusicGroup musicGroup = AddGroup(MusicSituation.TitleMenu);
		musicGroup.Names.Add("1. SII main theme");
		musicGroup.Names.Add("14. SII this is not the end");
		musicGroup.MinBuildUpTime = 1f;
		musicGroup.MaxBuildUpTime = 1f;
		musicGroup.MinTimeBetweenPlays = 10f;
		musicGroup.Volume = 0.75f;
		MusicGroup musicGroup2 = AddGroup(MusicSituation.BaseManagement);
		musicGroup2.Names.Add("6. SII sweeter than roses");
		musicGroup2.Names.Add("16. Sll PlayerCamp");
		musicGroup2.Names.Add("kalm surv");
		musicGroup2.MinTimeBetweenPlays = 60f;
		musicGroup2.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup2.MinBuildUpTime = 5f;
		musicGroup2.MaxBuildUpTime = 30f;
		MusicGroup musicGroup3 = AddGroup(MusicSituation.Exploring);
		musicGroup3.Names.Add("8. SII alone in the wild take 1");
		musicGroup3.Names.Add("11. SII we need more stuff");
		musicGroup3.Names.Add("27. Sll The End V2");
		musicGroup3.MinTimeBetweenPlays = 60f;
		musicGroup3.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup3.MinBuildUpTime = 20f;
		musicGroup3.MaxBuildUpTime = 60f;
		MusicGroup musicGroup4 = AddGroup(MusicSituation.ExploringTown);
		musicGroup4.Names.Add("2. SII dead town take 1");
		musicGroup4.Names.Add("9. SII ruined church");
		musicGroup4.Names.Add("20. Sll Explo");
		musicGroup4.MinTimeBetweenPlays = 30f;
		musicGroup4.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup4.MinBuildUpTime = 5f;
		musicGroup4.MaxBuildUpTime = 20f;
		MusicGroup musicGroup5 = AddGroup(MusicSituation.ExploringNight);
		musicGroup5.Names.Add("7. SII dark exploration");
		musicGroup5.Names.Add("21. Sll Ambience");
		musicGroup5.MinTimeBetweenPlays = 120f;
		musicGroup5.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup5.MinBuildUpTime = 30f;
		musicGroup5.MaxBuildUpTime = 90f;
		MusicGroup musicGroup6 = AddGroup(MusicSituation.NeutralCamp);
		musicGroup6.Names.Add("18. Sll NeutralCamp");
		musicGroup6.Names.Add("28. SII Camp");
		musicGroup6.MinTimeBetweenPlays = 60f;
		musicGroup6.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup6.MaxBuildUpTime = 10f;
		MusicGroup musicGroup7 = AddGroup(MusicSituation.LooterCamp);
		musicGroup7.Names.Add("17. Sll LooterCamp");
		musicGroup7.MinTimeBetweenPlays = 60f;
		musicGroup7.MaxTimePlayedWhileNotDesired = 20f;
		musicGroup7.MaxBuildUpTime = 10f;
		MusicGroup musicGroup8 = AddGroup(MusicSituation.Stealth);
		musicGroup8.Names.Add("5. SII stress");
		musicGroup8.Names.Add("12. SII danger is near");
		musicGroup8.Names.Add("13. SII be quiet");
		musicGroup8.MinTimeBetweenPlays = 10f;
		musicGroup8.MaxTimePlayedWhileNotDesired = 5f;
		MusicGroup musicGroup9 = AddGroup(MusicSituation.Combat);
		musicGroup9.Names.Add("15. SII pretty dead");
		musicGroup9.Names.Add("10. SII gathering stuff for my camp");
		musicGroup9.Names.Add("19. Sll action (V5)");
		musicGroup9.Names.Add("22. Sll Fight");
		musicGroup9.Names.Add("Bob_MusiqueAction");
		musicGroup9.MinTimeBetweenPlays = 10f;
		musicGroup9.MaxTimePlayedWhileNotDesired = 0f;
		AddGroup(MusicSituation.GameOver).Names.Add("24. Sll Jingle death");
		AddGroup(MusicSituation.GameComplete).Names.Add("26. SII The End");
		AddGroup(MusicSituation.QuestComplete).Names.Add("25 Sll Jingle mission accomplish");
	}

	public MusicGroup AddGroup(MusicSituation musicSituation)
	{
		MusicGroupForSituation[(int)musicSituation] = new MusicGroup();
		MusicGroupForSituation[(int)musicSituation].MusicSituation = musicSituation;
		return MusicGroupForSituation[(int)musicSituation];
	}

	public static bool AreAnyEnemiesNearby(Character controllerCharacter, float hysteresisDist)
	{
		return AreAnyEnemiesNearby(controllerCharacter, hysteresisDist, checkAccessible: false);
	}

	public static bool AreAnyEnemiesNearby(Character controllerCharacter, float hysteresisDist, bool checkAccessible)
	{
		GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(controllerCharacter.Tile - new TerrainCoord(EnemiesNearbyCheckDist, EnemiesNearbyCheckDist), controllerCharacter.Tile + new TerrainCoord(EnemiesNearbyCheckDist, EnemiesNearbyCheckDist), NearbyCharacters);
		foreach (Character nearbyCharacter in NearbyCharacters)
		{
			if (!nearbyCharacter.IsAwake || !controllerCharacter.IsEnemy(nearbyCharacter, includeJustActivatedInvisibleStrain: true) || !(controllerCharacter.Tile.GetDist(nearbyCharacter.Tile) <= (float)nearbyCharacter.GetSightRange() + hysteresisDist))
			{
				continue;
			}
			if (checkAccessible)
			{
				Target target = controllerCharacter.GetTarget(nearbyCharacter);
				if (target != null && target.GetFlag(TargetFlags.Inaccessible))
				{
					continue;
				}
			}
			return true;
		}
		NearbyCharacters.Clear();
		return false;
	}

	public static bool AreWeUnderAttack(Character controlledCharacter)
	{
		Character groupLeader = controlledCharacter.GetGroupLeader();
		if (groupLeader.UnderAttackRefCount > 0 || groupLeader.InCombat)
		{
			return true;
		}
		if (groupLeader.Followers != null)
		{
			foreach (Character follower in groupLeader.Followers)
			{
				if (follower.UnderAttackRefCount > 0 || follower.InCombat)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsRadioNearby(Character controlledCharacter)
	{
		int num = (int)RadioProp.Range;
		GameTerrain.Instance.RadioMapWho.GetObjectsInRect(controlledCharacter.Tile - new TerrainCoord(num, num), controlledCharacter.Tile + new TerrainCoord(num, num), NearbyRadios);
		foreach (RadioProp nearbyRadio in NearbyRadios)
		{
			if (nearbyRadio.Tile.GetDist(controlledCharacter.Tile) < RadioProp.Range && nearbyRadio.IsStillPlaying())
			{
				return true;
			}
		}
		return false;
	}

	public MusicSituation CalcDesiredMusicSituation(MusicSituation currentMusicSituation, out bool radioNearby)
	{
		GameImpl instance = GameImpl.Instance;
		MusicSituation result = MusicSituation.None;
		radioNearby = false;
		switch (instance.GetState())
		{
		case GameState.TitleMenu:
			result = MusicSituation.TitleMenu;
			break;
		case GameState.LoadingSession:
			result = currentMusicSituation;
			break;
		case GameState.RunningSession:
		{
			Session instance2 = Session.Instance;
			Hud instance3 = Hud.Instance;
			if (instance2.GameFinishedState == GameFinishedState.GameOver)
			{
				result = MusicSituation.GameOver;
				break;
			}
			if (instance2.GameFinishedState == GameFinishedState.GameComplete)
			{
				result = MusicSituation.GameComplete;
				break;
			}
			Character localControlledCharacter = instance3.LocalControlledCharacter;
			if (localControlledCharacter == null || !localControlledCharacter.AliveAndNotZombie)
			{
				break;
			}
			if (AreWeUnderAttack(localControlledCharacter))
			{
				result = MusicSituation.Combat;
				break;
			}
			if (IsRadioNearby(localControlledCharacter))
			{
				result = MusicSituation.None;
				radioNearby = true;
				break;
			}
			if (localControlledCharacter.IsCrouching() && AreAnyEnemiesNearby(localControlledCharacter, (currentMusicSituation == MusicSituation.Stealth) ? EnemiesNearbyHysteresisDist : 0f))
			{
				result = MusicSituation.Stealth;
				break;
			}
			TerrainCoord tile = localControlledCharacter.Tile;
			if (localControlledCharacter.Community.GetDistSqToNearestBuilding(tile) <= BaseManagementDist * BaseManagementDist)
			{
				result = MusicSituation.BaseManagement;
				break;
			}
			int ownerCommunityIdForTile = GameTerrain.Instance.GetOwnerCommunityIdForTile(tile.x, tile.y);
			if (BaseObjectManager.Instance.FindBaseObjectByID(ownerCommunityIdForTile) is Community community)
			{
				if (community.CommunityType == CommunityType.Looter)
				{
					result = MusicSituation.LooterCamp;
					break;
				}
				if (community.CommunityType == CommunityType.Normal)
				{
					result = MusicSituation.NeutralCamp;
					break;
				}
			}
			result = ((instance2.CommunityManager.GetVisitedTownForTile(tile) == null) ? ((Sun.GetSunIntensity(instance2.DaysSinceStart) != 0f) ? MusicSituation.Exploring : MusicSituation.ExploringNight) : MusicSituation.ExploringTown);
			break;
		}
		}
		return result;
	}

	public void Update()
	{
		_ = GameImpl.Instance;
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		MusicSituation musicSituation = ((CurrentMusic != null) ? CurrentMusic.MusicSituation : MusicSituation.None);
		bool radioNearby;
		MusicSituation musicSituation2 = CalcDesiredMusicSituation(musicSituation, out radioNearby);
		if (MusicGroupForSituation[(int)musicSituation2] != null && Time.unscaledTime - MusicGroupForSituation[(int)musicSituation2].LastPlayedTime < MusicGroupForSituation[(int)musicSituation2].MinTimeBetweenPlays && FindMusicPlayerForSituation(musicSituation2) == null)
		{
			musicSituation2 = MusicSituation.None;
		}
		if (musicSituation2 == MusicSituation.None && !radioNearby && CurrentMusic != null && CurrentMusic.TimePlayedWhileNotDesired <= CurrentMusic.MusicGroup.MaxTimePlayedWhileNotDesired)
		{
			musicSituation2 = CurrentMusic.MusicSituation;
			CurrentMusic.TimePlayedWhileNotDesired += Time.unscaledDeltaTime;
		}
		if (musicSituation != musicSituation2)
		{
			if (musicSituation2 == MusicSituation.None)
			{
				CurrentMusic = null;
			}
			else
			{
				MusicPlayer musicPlayer = FindMusicPlayerForSituation(musicSituation2);
				if (musicPlayer != null)
				{
					CurrentMusic = musicPlayer;
				}
				else
				{
					MusicGroup musicGroup = MusicGroupForSituation[(int)musicSituation2];
					int index = nonDeterministicRand.Next() % musicGroup.Names.Count;
					CurrentMusic = new MusicPlayer();
					CurrentMusic.SongName = musicGroup.Names[index];
					CurrentMusic.MusicSituation = musicSituation2;
					CurrentMusic.MusicGroup = musicGroup;
					CurrentMusic.BuildUpTime = Mathf.Lerp(CurrentMusic.MusicGroup.MinBuildUpTime, CurrentMusic.MusicGroup.MaxBuildUpTime, nonDeterministicRand.RandomFloat());
					AllPlayingMusic.Add(CurrentMusic);
				}
			}
		}
		for (int i = 0; i < AllPlayingMusic.Count; i++)
		{
			MusicPlayer musicPlayer2 = AllPlayingMusic[i];
			bool flag = CurrentMusic == musicPlayer2;
			MusicGroup musicGroup2 = MusicGroupForSituation[(int)musicPlayer2.MusicSituation];
			musicGroup2.LastPlayedTime = Time.unscaledTime;
			if (flag && musicPlayer2.UnityAudioSource == null && musicPlayer2.Time >= musicPlayer2.BuildUpTime && SoundManager.MusicSoundVolume > 0f)
			{
				if (musicGroup2.Res == null)
				{
					musicGroup2.LoadMusicResource(musicPlayer2.SongName);
				}
				else if (musicGroup2.Res.IsFinishedLoading())
				{
					AudioSource audioSource = new GameObject(MusicObjectName).AddComponent<AudioSource>();
					audioSource.clip = musicGroup2.Res;
					audioSource.volume = 0f;
					audioSource.spatialBlend = 0f;
					audioSource.RealisticRolloff();
					audioSource.Play();
					musicPlayer2.UnityAudioSource = audioSource;
				}
			}
			bool flag2 = false;
			if (musicPlayer2.UnityAudioSource != null)
			{
				if (!musicPlayer2.UnityAudioSource.isPlaying && !musicPlayer2.IsPaused && !musicPlayer2.IsFinished)
				{
					musicPlayer2.IsFinished = true;
				}
				flag2 = !musicPlayer2.IsFinished;
			}
			musicPlayer2.Transition = Mathf.Clamp01(musicPlayer2.Transition + Time.unscaledDeltaTime / ((!(flag && flag2)) ? (0f - musicGroup2.FadeOutTime) : (musicPlayer2.WasEverPaused ? musicGroup2.FadeInFromPausedTime : musicGroup2.FadeInAtStartTime)));
			musicPlayer2.Time += Time.unscaledDeltaTime;
			if (musicPlayer2.UnityAudioSource != null)
			{
				if (!musicPlayer2.UnityAudioSource.isPlaying && musicPlayer2.Transition > 0f && flag && flag2)
				{
					musicPlayer2.UnityAudioSource.Play();
					musicPlayer2.IsPaused = false;
				}
				musicPlayer2.UnityAudioSource.volume = musicPlayer2.Transition * SoundManager.MusicSoundVolume * MusicBaseVolume * musicGroup2.Volume;
			}
			if ((flag && !musicPlayer2.IsFinished) || musicPlayer2.Transition != 0f || (musicGroup2.Res != null && !musicGroup2.Res.IsFinishedLoading()))
			{
				continue;
			}
			if (flag2 && musicPlayer2.TimePaused < musicGroup2.MaxPausedTime)
			{
				if (musicPlayer2.UnityAudioSource.isPlaying)
				{
					musicPlayer2.UnityAudioSource.Pause();
					musicPlayer2.IsPaused = true;
					musicPlayer2.WasEverPaused = true;
				}
				else
				{
					musicPlayer2.TimePaused += Time.unscaledDeltaTime;
				}
				continue;
			}
			if (musicPlayer2.UnityAudioSource != null)
			{
				musicPlayer2.UnityAudioSource.Stop();
				Object.Destroy(musicPlayer2.UnityAudioSource.gameObject);
			}
			if (musicGroup2.Res != null)
			{
				musicGroup2.UnloadMusicResource();
			}
			AllPlayingMusic.RemoveAt(i);
			i--;
		}
	}

	private MusicPlayer FindMusicPlayerForSituation(MusicSituation musicSituation)
	{
		foreach (MusicPlayer item in AllPlayingMusic)
		{
			if (item.MusicSituation == musicSituation)
			{
				return item;
			}
		}
		return null;
	}
}
