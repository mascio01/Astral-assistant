using System;

public abstract class AmbientEnemySpawnPoint : SpawnPoint
{
	public float LastDiedTime;

	public Community SpawnedEnemyGroup;

	public static float MinTimeBetweenSpawns = Sun.DayLengthSecs;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref LastDiedTime);
		reflector.Add(ref SpawnedEnemyGroup);
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.CommunityManager.AmbientEnemySpawnPoints.Add(this);
	}

	public override void Delete()
	{
		Session.Instance.CommunityManager.AmbientEnemySpawnPoints.Remove(this);
		base.Delete();
	}

	public virtual float GetMinTimeBetweenSpawns()
	{
		return MinTimeBetweenSpawns;
	}

	public bool CanSpawn(bool respawnsAllowed, TimeSpan sessionTime)
	{
		if (SpawnedEnemyGroup != null)
		{
			return false;
		}
		if (LastDiedTime > 0f)
		{
			if (!respawnsAllowed)
			{
				return false;
			}
			if ((float)sessionTime.TotalSeconds - LastDiedTime < GetMinTimeBetweenSpawns())
			{
				return false;
			}
		}
		return CanSpawnOnTile();
	}

	public bool CanSpawnOnTile()
	{
		if (GameTerrain.Instance.IsTileEnclosed(Tile.x, Tile.y) && LastDiedTime > 0f)
		{
			return false;
		}
		return GameTerrain.Instance.IsTileSpawnable(Tile.x, Tile.y);
	}

	public bool HasSpawnedSomeoneWhoDied()
	{
		return LastDiedTime > 0f;
	}

	public void OnSpawned(Community group)
	{
		if (group != null)
		{
			if (LastDiedTime > 0f)
			{
				Session.Instance.CommunityManager.TakeFromRespawnBuildup();
			}
			SpawnedEnemyGroup = group;
		}
	}

	public virtual void OnSpawnedEnemyDied()
	{
	}

	public virtual void OnSpawnedEnemyGroupDied()
	{
		LastDiedTime = (float)Session.Instance.PlayTime.TotalSeconds;
	}

	public void OnSpawnedEnemyDeleted()
	{
		if (SpawnedEnemyGroup.HasAnyActiveMembers() && LastDiedTime > 0f)
		{
			Session.Instance.CommunityManager.GiveBackToRespawnBuildup();
		}
		SpawnedEnemyGroup.SpawnPoint = null;
		SpawnedEnemyGroup = null;
	}

	public abstract Community SpawnEnemyGroup();
}
