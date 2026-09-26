using System;
using UnityEngine;

public class DeerSpawnPoint : AmbientEnemySpawnPoint
{
	public bool Discovered;

	public int HerdSize;

	private float LastMemberDiedTime;

	public static float MinTimeBetweenDeerSpawns = Sun.DayLengthSecs * 7f;

	public static float MinTimeBetweenHerdSpawns = Sun.DayLengthSecs * 7f * 3f;

	public int MaxHerdSize => MathUtil.RandomInt(Id * 567, 2, 5);

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return GameTerrain.MinimapSettings.NeutralCol;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.DeerSpawnPoint;
	}

	public override PrefabResource GetUnityModel()
	{
		return RabbitSpawnPoint.Model;
	}

	public override float GetMinTimeBetweenSpawns()
	{
		return MinTimeBetweenHerdSpawns;
	}

	public override void OnSpawn()
	{
		base.OnSpawn();
		HerdSize = MaxHerdSize;
	}

	public override Community SpawnEnemyGroup()
	{
		CustomRandom deterministicRand = Session.Instance.DeterministicRand;
		int num = Math.Max(1, HerdSize);
		Community community = null;
		int num2 = 0;
		while ((community == null || community.Members.Count < num) && num2 < 100)
		{
			num2++;
			int num3 = ((num2 < 50) ? 8 : 16);
			TerrainCoord tile = Tile + new TerrainCoord(deterministicRand.Next(-num3, num3), deterministicRand.Next(-num3, num3));
			if (!GameTerrain.Instance.IsImpassable(tile.x, tile.y, 2051, null, null))
			{
				if (community == null)
				{
					community = Community.Spawn(CommunityType.AmbientAnimal);
					community.SpawnPoint = this;
				}
				DeerAppearance appearance = new DeerAppearance((!deterministicRand.RandomChoice(0.5f)) ? GenderType.Female : GenderType.Male, DeerAppearance.PickRandomAge(deterministicRand));
				Deer deer = Deer.Spawn(tile, deterministicRand.RandomFloat() * (MathF.PI * 2f), appearance);
				deer.RandomizeSurvivalFactors(deterministicRand);
				community.AddMember(deer);
				deer.SetGoal(new AnimalGoal());
			}
		}
		if (community != null)
		{
			OnSpawned(community);
		}
		return community;
	}

	public override void OnSpawnedEnemyDied()
	{
		base.OnSpawnedEnemyDied();
		HerdSize = Math.Max(0, HerdSize - 1);
		LastMemberDiedTime = (float)Session.Instance.PlayTime.TotalSeconds;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref HerdSize, 275);
		if (reflector.IsDeserialising)
		{
			HerdSize = Math.Max(0, HerdSize);
			if (HerdSize > 0)
			{
				LastDiedTime = 0f;
			}
		}
		reflector.AddAfter(ref Discovered, 277);
		reflector.AddAfter(ref LastMemberDiedTime, 285);
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.CommunityManager.DeerSpawnPoints.Add(this);
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		Session.Instance.CommunityManager.DeerSpawnPoints.Remove(this);
		base.Delete();
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		if (HerdSize < MaxHerdSize)
		{
			float num = ((HerdSize == 0) ? MinTimeBetweenHerdSpawns : MinTimeBetweenDeerSpawns);
			if ((double)(LastMemberDiedTime + num) <= Session.Instance.PlayTime.TotalSeconds)
			{
				LastMemberDiedTime += num;
				if (!GameTerrain.Instance.IsTileEnclosed(Tile.x, Tile.y))
				{
					HerdSize++;
					LastDiedTime = 0f;
				}
			}
		}
		stillNeedUpdating = true;
	}
}
