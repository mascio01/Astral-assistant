using System;
using System.Collections.Generic;
using UnityEngine;

public class CropPatch : IReflectable
{
	public int CommunityId;

	public PropPrototype CropType;

	public TerrainCoord MinTile;

	public TerrainCoord MaxTile;

	public List<TerrainCoord> Tiles = new List<TerrainCoord>();

	public List<PlantableCrop> CropsInPatch = new List<PlantableCrop>();

	public TerrainRect Rect => new TerrainRect(MinTile, MaxTile);

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref CommunityId);
		if (reflector.Version < 218)
		{
			BaseObjectType value = BaseObjectType.Invalid;
			reflector.Add(ref value);
			CropType = GameImpl.Instance.FindPropPrototypeByName(value.ToString());
		}
		else
		{
			reflector.Add(ref CropType);
			if (CropType == null)
			{
				Debug.LogWarning("Null crop type in patch for community " + CommunityId);
			}
		}
		reflector.Add(ref MinTile);
		reflector.Add(ref MaxTile);
		reflector.Add(ref Tiles);
	}

	public Community GetCommunity()
	{
		if (CommunityId == 0)
		{
			return null;
		}
		return BaseObjectManager.Instance.FindBaseObjectByID(CommunityId) as Community;
	}

	public void SetCommunity(Community community)
	{
		CommunityId = community.Id;
		for (int i = 0; i < Tiles.Count; i++)
		{
			GameTerrain.Instance.GetPlant(Tiles[i].x, Tiles[i].y)?.SetCommunity(community);
		}
	}

	public void RecalcBounds()
	{
		TerrainCoord terrainCoord = new TerrainCoord(int.MaxValue, int.MaxValue);
		TerrainCoord terrainCoord2 = new TerrainCoord(-2147483647, -2147483647);
		for (int i = 0; i < CropsInPatch.Count; i++)
		{
			terrainCoord = TerrainCoord.Min(terrainCoord, CropsInPatch[i].Tile);
			terrainCoord2 = TerrainCoord.Max(terrainCoord2, CropsInPatch[i].Tile);
		}
		for (int j = 0; j < Tiles.Count; j++)
		{
			terrainCoord = TerrainCoord.Min(terrainCoord, Tiles[j]);
			terrainCoord2 = TerrainCoord.Max(terrainCoord2, Tiles[j]);
		}
		if (GameTerrain.Instance.IsTileRectWithinBounds(terrainCoord, terrainCoord2))
		{
			MinTile = terrainCoord;
			MaxTile = terrainCoord2;
		}
		else
		{
			Debug.LogError("Empty crop patch?");
		}
	}

	public bool IsWithinRangeOf(TerrainCoord tile, float range)
	{
		if (Rect.GetClosestDistSqTo(tile) > range * range)
		{
			return false;
		}
		for (int i = 0; i < Tiles.Count; i++)
		{
			if (Tiles[i].GetDistSquared(tile) > range * range)
			{
				return false;
			}
		}
		return true;
	}

	public int GetMaxTileX()
	{
		int num = -2147483647;
		for (int i = 0; i < Tiles.Count; i++)
		{
			num = Math.Max(num, Tiles[i].x);
		}
		return num;
	}

	public int GetMaxTileY()
	{
		int num = -2147483647;
		for (int i = 0; i < Tiles.Count; i++)
		{
			num = Math.Max(num, Tiles[i].y);
		}
		return num;
	}
}
