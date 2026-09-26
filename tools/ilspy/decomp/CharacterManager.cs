using System;
using System.Collections.Generic;
using System.Diagnostics;

public class CharacterManager
{
	public List<Character> Characters = new List<Character>();

	public List<Character> DisappearedCharacters = new List<Character>();

	public Dictionary<string, int> UsedFirstNames = new Dictionary<string, int>();

	public CollisionManager CollisionManager = new CollisionManager();

	public List<Character>[] ThinkBuckets = new List<Character>[3];

	public int[] ThinkBucketCursor = new int[3];

	private Stopwatch _timer = new Stopwatch();

	private TimeSpan[] MaxTimeForBucket = new TimeSpan[3];

	public static TimeSpan DeterministicTimeSpentThinking;

	public static TimeSpan ThinkTimeUnit = MathUtil.FromMilliseconds(0.5f);

	public static GameProfiler[] ThinkBucketTimer;

	private static ThinkPriority[] VisibleFirstOrder = new ThinkPriority[3]
	{
		ThinkPriority.Visible,
		ThinkPriority.InCombat,
		ThinkPriority.Idle
	};

	private static ThinkPriority[] CombatFirstOrder = new ThinkPriority[3]
	{
		ThinkPriority.InCombat,
		ThinkPriority.Idle,
		ThinkPriority.Visible
	};

	private static ThinkPriority[] IdleFirstOrder = new ThinkPriority[3]
	{
		ThinkPriority.Idle,
		ThinkPriority.InCombat,
		ThinkPriority.Visible
	};

	private static int Skip = 30;

	private static string CharacterUpdateStr = "CharacterUpdate";

	public CharacterManager()
	{
		for (int i = 0; i < ThinkBuckets.Length; i++)
		{
			ThinkBuckets[i] = new List<Character>();
		}
		if (ThinkBucketTimer == null)
		{
			ThinkBucketTimer = new GameProfiler[3];
			for (int j = 0; j < ThinkBucketTimer.Length; j++)
			{
				GameProfiler[] thinkBucketTimer = ThinkBucketTimer;
				int num = j;
				ThinkPriority thinkPriority = (ThinkPriority)j;
				thinkBucketTimer[num] = new GameProfiler("Update.ThinkBucket." + thinkPriority);
			}
		}
		MaxTimeForBucket[2] = TimeSpan.FromMilliseconds(1.0);
		MaxTimeForBucket[1] = TimeSpan.FromMilliseconds(1.5);
		MaxTimeForBucket[0] = TimeSpan.FromMilliseconds(2.0);
	}

	public void Think()
	{
		Session instance = Session.Instance;
		ThinkPriority[] visibleFirstOrder = VisibleFirstOrder;
		bool flag = instance.IsInMultiplayerGame() || GameplayDebugMenu.ForceDeterministicUpdate;
		float num = 1f / (float)Math.Max(1, instance.GetFramesPerFrame());
		_timer.Start();
		DeterministicTimeSpentThinking = TimeSpan.Zero;
		for (int i = 0; i < visibleFirstOrder.Length; i++)
		{
			int num2 = (int)visibleFirstOrder[i];
			if (ThinkBuckets[num2].Count == 0)
			{
				continue;
			}
			TimeSpan timeSpan = TimeSpan.FromMilliseconds(MaxTimeForBucket[num2].TotalMilliseconds * (double)num);
			ThinkBucketCursor[num2] %= ThinkBuckets[num2].Count;
			int num3 = ThinkBucketCursor[num2];
			do
			{
				DeterministicTimeSpentThinking += ThinkTimeUnit;
				int num4 = 1;
				using (new ProfileMarker(ThinkBucketTimer[num2]))
				{
					Character character = ThinkBuckets[num2][ThinkBucketCursor[num2]];
					character.Think();
					if (character.PropWantDelete())
					{
						character.Delete();
						num4 = 0;
					}
				}
				if (ThinkBuckets[num2].Count == 0)
				{
					break;
				}
				ThinkBucketCursor[num2] = (ThinkBucketCursor[num2] + num4) % ThinkBuckets[num2].Count;
			}
			while (ThinkBucketCursor[num2] != num3 && (flag ? DeterministicTimeSpentThinking : _timer.Elapsed) < timeSpan);
		}
		_timer.Reset();
	}

	public void UpdateVisible()
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		float num = 1024f;
		foreach (PlayerRecord playerRecord in instance.PlayerRecords)
		{
			if (playerRecord.PlayerMode == PlayerMode.Dormant || playerRecord.PlayerMode == PlayerMode.CreatingCharacter)
			{
				playerRecord.DeterministicVisibleCharacters.Clear();
				continue;
			}
			TerrainCoord tileCoordForPosXZ = instance2.GetTileCoordForPosXZ(playerRecord.SyncedCamFocusPosXZ);
			instance2.CharacterMapWho.GetObjectsInRect(tileCoordForPosXZ - new TerrainCoord(32, 32), tileCoordForPosXZ + new TerrainCoord(32, 32), playerRecord.DeterministicVisibleCharacters);
			foreach (Character deterministicVisibleCharacter in playerRecord.DeterministicVisibleCharacters)
			{
				if (!(deterministicVisibleCharacter.Tile.GetDistSquared(tileCoordForPosXZ) > num))
				{
					deterministicVisibleCharacter.MarkVisibleDeterministic();
				}
			}
			if (playerRecord.PlayerCharacter != null && playerRecord.PlayerMode == PlayerMode.Controlling)
			{
				playerRecord.PlayerCharacter.MarkVisibleDeterministic();
			}
			if (playerRecord.SyncedPipObj is Character character)
			{
				character.MarkVisibleDeterministic();
			}
		}
	}

	public void Update()
	{
		Session instance = Session.Instance;
		TimeSpan playTime = instance.PlayTime;
		int num = instance.PlaySpeedIndependantUpdateFrame % Skip;
		int num2 = 0;
		while (num2 < Characters.Count)
		{
			Character character = Characters[num2];
			if (num2 % Skip != num && !character.IsVisibleForUpdate())
			{
				num2++;
				continue;
			}
			using (new UnityProfileMarker(CharacterUpdateStr))
			{
				character.Update(playTime);
				CollisionManager.UpdatedThisFrame.Add(character);
			}
			num2++;
		}
	}

	public void OnNewGame()
	{
		foreach (Character character in Characters)
		{
			character.OnNewGame();
		}
	}

	public void OnLoad(int version)
	{
		for (int num = Characters.Count - 1; num >= 0; num--)
		{
			Characters[num].OnLoad(version);
		}
		for (int num2 = DisappearedCharacters.Count - 1; num2 >= 0; num2--)
		{
			Character character = DisappearedCharacters[num2];
			character.OnLoad(version);
			if (character.KeptAroundForReferences && !CommunityManager.IsAnyoneImportantReferencingObject(character))
			{
				Session.Instance.DeterministicRand.Locked = false;
				character.Delete();
				Session.Instance.DeterministicRand.Locked = true;
			}
		}
		GameImpl.Instance.UpdateThreadPool.BlockWhileBusy();
	}

	public void OnFirstNameAdded(string firstName)
	{
		if (!string.IsNullOrEmpty(firstName))
		{
			int value = 0;
			UsedFirstNames.TryGetValue(firstName, out value);
			UsedFirstNames[firstName] = value + 1;
		}
	}

	public void OnFirstNameRemoved(string firstName)
	{
		if (!string.IsNullOrEmpty(firstName))
		{
			int value = 0;
			UsedFirstNames.TryGetValue(firstName, out value);
			if (value == 1)
			{
				UsedFirstNames.Remove(firstName);
			}
			else
			{
				UsedFirstNames[firstName] = value - 1;
			}
		}
	}

	public void OnCharacterCreated(Character character)
	{
		if (character.Disappeared)
		{
			DisappearedCharacters.Add(character);
		}
		else
		{
			Characters.Add(character);
		}
		OnFirstNameAdded(character.FirstName);
	}

	public void OnCharacterDisappeared(Character disappearedCharacter)
	{
		Characters.Remove(disappearedCharacter);
		DisappearedCharacters.Add(disappearedCharacter);
	}

	public void OnCharacterDeleted(Character deletedCharacter)
	{
		OnFirstNameRemoved(deletedCharacter.FirstName);
		if (deletedCharacter.Disappeared)
		{
			DisappearedCharacters.Remove(deletedCharacter);
		}
		else
		{
			Characters.Remove(deletedCharacter);
		}
		if (deletedCharacter.Community != null && (deletedCharacter.Community.CommunityType == CommunityType.AmbientAnimal || deletedCharacter.Community.CommunityType == CommunityType.AmbientZombie))
		{
			return;
		}
		foreach (Character character in Characters)
		{
			character.RemoveDeletedObjFromMemories(deletedCharacter);
		}
		foreach (Character disappearedCharacter in DisappearedCharacters)
		{
			disappearedCharacter.RemoveDeletedObjFromMemories(deletedCharacter);
		}
	}

	public void OnCommunityDeleted(Community deletedCommunity)
	{
		foreach (Character character in Characters)
		{
			character.OnCommunityDeleted(deletedCommunity);
		}
		foreach (Character disappearedCharacter in DisappearedCharacters)
		{
			disappearedCharacter.OnCommunityDeleted(deletedCommunity);
		}
	}

	public Character GetClosestCharacterTo(TerrainCoord tile)
	{
		Character result = null;
		float num = float.MaxValue;
		foreach (Character character in Characters)
		{
			float distSquared = character.Tile.GetDistSquared(tile);
			if (distSquared < num)
			{
				result = character;
				num = distSquared;
			}
		}
		return result;
	}

	public Character GetAvatarForPlayer(PlayerID playerID)
	{
		foreach (Character character in Characters)
		{
			if (character.AvatarForPlayer == playerID)
			{
				return character;
			}
		}
		return null;
	}

	public int GetCharacterCountBySpecies(BaseObjectType species)
	{
		int num = 0;
		foreach (Character character in Characters)
		{
			if (character.GetBaseObjectType() == species)
			{
				num++;
			}
		}
		return num;
	}

	public bool DoesAnyCharacterHaveFirstName(string firstName)
	{
		return UsedFirstNames.ContainsKey(firstName);
	}
}
