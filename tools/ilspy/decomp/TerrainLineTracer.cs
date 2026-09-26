using System;
using UnityEngine;

public class TerrainLineTracer
{
	protected GameTerrain Terrain;

	public TerrainLineTracer(GameTerrain terrain)
	{
		Terrain = terrain;
	}

	public void TraceLine(Ray ray, float length)
	{
		Vector3 origin = ray.origin;
		Vector3 vector = ray.origin + ray.direction * length;
		Vector3 vector2 = vector - origin;
		TerrainCoord tileCoordForPos = Terrain.GetTileCoordForPos(origin);
		TerrainCoord tileCoordForPos2 = Terrain.GetTileCoordForPos(vector);
		TerrainCoord terrainCoord = tileCoordForPos;
		OnStartTrace(terrainCoord);
		if (OnCrossTile(tileCoordForPos, terrainCoord, ray, length))
		{
			return;
		}
		int num = ((vector.x > origin.x) ? 1 : (-1));
		int num2 = ((vector.z > origin.z) ? 1 : (-1));
		if (tileCoordForPos != tileCoordForPos2)
		{
			if (Math.Abs(vector2.z) <= Math.Abs(vector2.x))
			{
				float num3 = (vector.z - origin.z) / (vector.x - origin.x);
				float num4 = origin.x / (origin.x - vector.x);
				float num5 = origin.z + (vector.z - origin.z) * num4;
				while (true)
				{
					float num6 = ((num == 1) ? ((float)(terrainCoord.x + 1)) : ((float)terrainCoord.x)) - Terrain.HalfSize;
					int num7 = (int)(Terrain.HalfSize + (num3 * num6 + num5));
					if (num7 != terrainCoord.y && num7 == terrainCoord.y + num2)
					{
						TerrainCoord prevTile = terrainCoord;
						terrainCoord.y += num2;
						if (OnCrossTile(prevTile, terrainCoord, ray, length))
						{
							return;
						}
					}
					if (num * terrainCoord.x >= num * tileCoordForPos2.x)
					{
						break;
					}
					TerrainCoord prevTile2 = terrainCoord;
					terrainCoord.x += num;
					if (OnCrossTile(prevTile2, terrainCoord, ray, length))
					{
						return;
					}
				}
			}
			else
			{
				float num8 = (vector.x - origin.x) / (vector.z - origin.z);
				float num9 = origin.z / (origin.z - vector.z);
				float num10 = origin.x + (vector.x - origin.x) * num9;
				while (true)
				{
					float num11 = ((num2 == 1) ? ((float)(terrainCoord.y + 1)) : ((float)terrainCoord.y)) - Terrain.HalfSize;
					int num12 = (int)(Terrain.HalfSize + (num8 * num11 + num10));
					if (num12 != terrainCoord.x && num12 == terrainCoord.x + num)
					{
						TerrainCoord prevTile3 = terrainCoord;
						terrainCoord.x += num;
						if (OnCrossTile(prevTile3, terrainCoord, ray, length))
						{
							return;
						}
					}
					if (num2 * terrainCoord.y >= num2 * tileCoordForPos2.y)
					{
						break;
					}
					TerrainCoord prevTile4 = terrainCoord;
					terrainCoord.y += num2;
					if (OnCrossTile(prevTile4, terrainCoord, ray, length))
					{
						return;
					}
				}
			}
		}
		OnFinishTrace(terrainCoord, ray, length);
	}

	protected virtual void OnStartTrace(TerrainCoord tile)
	{
	}

	protected virtual void OnFinishTrace(TerrainCoord tile, Ray ray, float length)
	{
	}

	protected virtual bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		return false;
	}
}
