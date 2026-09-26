using System;
using System.Collections.Generic;
using UnityEngine;

public class RabbitTrap : TiltedProp, ITrap
{
	public bool IsOpen = true;

	public int FreeResets;

	public TimeSpan LastCheckTime;

	public static int MaxFreeResets = 3;

	private static PrefabResource[] UnityRabbitTrap = new PrefabResource[2]
	{
		new PrefabResource("Prefabs\\Props\\Trap\\RabbitTrap", 10),
		new PrefabResource("Prefabs\\Props\\Trap\\RabbitTrap_Closed", 10)
	};

	public List<TileObject> NearbyRabbitSpawnPoints;

	private static List<RabbitSpawnPoint> Candidates = new List<RabbitSpawnPoint>();

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.RabbitTrap;
	}

	public override PrefabResource GetUnityModel()
	{
		int num = ((!IsOpen) ? 1 : 0);
		if (Prototype == null || num >= Prototype.Prefabs.Count)
		{
			return UnityRabbitTrap[num];
		}
		return Prototype.Prefabs[num];
	}

	public override bool SupportsVariation()
	{
		return false;
	}

	public override bool CanSetStoragePolicy()
	{
		return false;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref IsOpen, 161);
		reflector.AddAfter(ref FreeResets, 252);
		reflector.AddAfter(ref LastCheckTime, 165);
	}

	public override void Init()
	{
		base.Init();
		GameTerrain.Instance.FoodMapWho.AddToMapWho(this, Tile);
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
	}

	public override void Delete()
	{
		Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.Rare);
		GameTerrain.Instance.FoodMapWho.RemoveFromMapWho(this, Tile);
		base.Delete();
	}

	public override void SetTile(TerrainCoord tile)
	{
		if (InTerrain)
		{
			GameTerrain.Instance.FoodMapWho.OnMoved(this, Tile, tile);
		}
		base.SetTile(tile);
	}

	public TerrainCoord GetEntranceTile()
	{
		return Tile + Prop.GetDirFromOrientationType(Orientation);
	}

	public void TriggerTrap(Character character)
	{
		Equipment equipment = Equipment.Spawn(EquipmentPrototype.DeadRabbit);
		Inventory.Add(this, equipment);
		SetOpen(open: false);
	}

	public void ResetTrap(Character character, bool isGathering)
	{
		if (FreeResets >= MaxFreeResets)
		{
			FreeResets = 0;
		}
		else
		{
			FreeResets++;
		}
		while (Inventory.Count > 0)
		{
			Equipment item = Inventory.GetItem(0);
			int amount = item.GetAmount();
			NotificationManager.Instance.AddEquipmentNotification(this, character, item, amount);
			item = Inventory.Take(this, item, amount);
			item = character.Inventory.Add(character, item);
			if (isGathering)
			{
				item.IncrementGatheredAmount(amount);
			}
		}
		SetOpen(open: true);
	}

	public void SetOpen(bool open)
	{
		bool num = IsUnityObjectActive();
		if (num)
		{
			UnityDeactivate();
		}
		UnityDelete();
		IsOpen = open;
		UnityInit();
		if (num)
		{
			UnityActivate();
		}
	}

	public bool CanTriggerTrap(Character character)
	{
		return false;
	}

	public bool CanResetTrap()
	{
		if (UnderConstructionInfo == null)
		{
			return !IsOpen;
		}
		return false;
	}

	public EquipmentPrototype GetEquipmentNeededForReset()
	{
		if (FreeResets < MaxFreeResets)
		{
			return null;
		}
		return EquipmentPrototype.Carrot;
	}

	public override void PropUpdateRare(ref bool stillNeedUpdating)
	{
		base.PropUpdateRare(ref stillNeedUpdating);
		if (!Session.Instance.IsVisibleDeterministic(GameTerrain.Instance.GetTileCentreXZ(Tile)) && Session.Instance.PlayTime - LastCheckTime >= TimeSpan.FromSeconds(Mathf.Lerp(Sun.DayLengthSecs, Sun.DayLengthSecs * 3f, MathUtil.RandomFloat((float)LastCheckTime.TotalSeconds + (float)Id))))
		{
			if (IsOpen)
			{
				if (NearbyRabbitSpawnPoints == null)
				{
					NearbyRabbitSpawnPoints = new List<TileObject>();
					GameTerrain.Instance.GetObjectsOfBaseTypeInRect(Tile - new TerrainCoord(48, 48), Tile + new TerrainCoord(48, 48), NearbyRabbitSpawnPoints, BaseObjectType.RabbitSpawnPoint);
				}
				Candidates.Clear();
				foreach (TileObject nearbyRabbitSpawnPoint in NearbyRabbitSpawnPoints)
				{
					RabbitSpawnPoint rabbitSpawnPoint = nearbyRabbitSpawnPoint as RabbitSpawnPoint;
					if (rabbitSpawnPoint.CanSpawnOnTile() && (!(rabbitSpawnPoint.LastDiedTime > 0f) || !((float)Session.Instance.PlayTime.TotalSeconds - rabbitSpawnPoint.LastDiedTime < AmbientEnemySpawnPoint.MinTimeBetweenSpawns)))
					{
						Candidates.Add(rabbitSpawnPoint);
					}
				}
				if (Candidates.Count > 0)
				{
					int index = Session.Instance.DeterministicRand.Next(Candidates.Count);
					Session.Instance.CommunityManager.PretendDied.Add(Candidates[index]);
					TriggerTrap(null);
				}
				Candidates.Clear();
			}
			LastCheckTime = Session.Instance.PlayTime;
		}
		stillNeedUpdating = true;
	}

	public override bool IsImpassable(Character requester, int options, TerrainCoord tile)
	{
		if ((options & 0x80) != 0)
		{
			return true;
		}
		return base.IsImpassable(requester, options, tile);
	}

	public override bool IsTargetable()
	{
		return true;
	}

	public override bool WantDebrisOnDemolition()
	{
		return false;
	}
}
