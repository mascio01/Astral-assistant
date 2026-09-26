using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Town : SpawnPoint
{
	public string UniqueID;

	public List<Prop> Buildings = new List<Prop>();

	public float Radius;

	public int RoadIndex = -1;

	public float RoadPointIndex;

	public bool Visited;

	public bool IgnoreForQuests;

	public InfectionType Infection = InfectionType.Green;

	public string InvaderName;

	public TownName TownName = new TownName();

	private static PrefabResource Model = new PrefabResource("Prefabs/SpawnPoints/TownSpawnPoint", 5);

	private static string TownStr = "Town";

	public static Color32 TownCol = new Color32(90, 87, 174, byte.MaxValue);

	public override Color32 MapColor
	{
		get
		{
			if (!Session.Instance.Editor && !SpawnPoint.DebugShowSpawnPoints)
			{
				return MathUtil.TransparentBlack;
			}
			return TownCol;
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Town;
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

	public override PrefabResource GetUnityModel()
	{
		return Model;
	}

	public override void BuildDisplayName(StringBuilder sb, bool noStrangers, bool englishOnly)
	{
		if (TownName.Type == TownNameType.CustomString && string.IsNullOrEmpty(TownName.CustomString))
		{
			sb.Append(TownStr);
		}
		else
		{
			TownName.BuildDisplayName(sb, englishOnly);
		}
	}

	public static Town Spawn(TerrainCoord tile)
	{
		Town town = new Town();
		town.Tile = tile;
		town.OnSpawn();
		return town;
	}

	public override void Init()
	{
		base.Init();
		Session.Instance.CommunityManager.Towns.Add(this);
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.RegisterUniqueID(UniqueID, this);
		}
	}

	public override void Delete()
	{
		if (!string.IsNullOrEmpty(UniqueID))
		{
			BaseObjectManager.Instance.UnregisterUniqueID(UniqueID, this);
		}
		Session.Instance.CommunityManager.Towns.Remove(this);
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref UniqueID, 474);
		reflector.AddGameObjectRefList(ref Buildings);
		reflector.Add(ref Radius);
		reflector.Add(ref RoadIndex);
		reflector.Add(ref RoadPointIndex);
		reflector.Add(ref Visited);
		reflector.AddAfter(ref IgnoreForQuests, 580);
		reflector.Add(ref Infection);
		reflector.AddAfter(ref InvaderName, 481);
		if (reflector.Version >= 126)
		{
			TownName.Reflect(reflector);
		}
	}

	private void CalcRadius()
	{
		Vector2 posXZ = PosXZ;
		float num = 0f;
		foreach (Prop building in Buildings)
		{
			Bounds boundingBox = building.GetBoundingBox();
			float val = (MathUtil.ToXZ(boundingBox.center) - posXZ).magnitude + boundingBox.extents.magnitude;
			num = Math.Max(num, val);
		}
		Radius = num;
	}

	public TerrainRect GetTownRect()
	{
		int num = Mathf.CeilToInt(Radius);
		return new TerrainRect(Tile - new TerrainCoord(num, num), Tile + new TerrainCoord(num, num));
	}

	public bool IsInRadius(TerrainCoord tile)
	{
		return tile.GetDistSquared(Tile) <= Radius * Radius;
	}

	public void AddBuilding(Prop building)
	{
		if (building.Town != null)
		{
			if (building.Town == this)
			{
				return;
			}
			building.Town.RemoveBuilding(building);
		}
		Buildings.Add(building);
		building.Town = this;
		if (!Session.Instance.Editor)
		{
			CalcRadius();
		}
		StoryManager.Instance.SetConditionsDirty();
	}

	public void RemoveBuilding(Prop building)
	{
		if (building.Town == this)
		{
			Buildings.Remove(building);
			building.Town = null;
			if (!Session.Instance.Editor)
			{
				CalcRadius();
			}
			StoryManager.Instance.SetConditionsDirty();
		}
	}
}
