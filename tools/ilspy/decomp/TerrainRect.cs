using System;
using System.Collections.Generic;
using UnityEngine;

public struct TerrainRect : IReflectable, IEquatable<TerrainRect>
{
	public static TerrainRect Invalid = new TerrainRect(TerrainCoord.Invalid, TerrainCoord.Invalid);

	public TerrainCoord min;

	public TerrainCoord max;

	public TerrainCoord Centre => min + (max - min) / 2;

	public int VerticesArea => (max.x - min.x) * (max.y - min.y);

	public int TilesArea => (max.x + 1 - min.x) * (max.y + 1 - min.y);

	public int LongestEdge => Math.Max(max.x + 1 - min.x, max.y + 1 - min.y);

	public int TilesSizeX => max.x + 1 - min.x;

	public int TilesSizeY => max.y + 1 - min.y;

	public TerrainRect(TerrainCoord _min, TerrainCoord _max)
	{
		min = _min;
		max = _max;
	}

	public TerrainRect(List<TerrainCoord> perimeter)
	{
		if (perimeter != null && perimeter.Count > 0)
		{
			min = (max = perimeter[0]);
			for (int i = 1; i < perimeter.Count; i++)
			{
				min = TerrainCoord.Min(min, perimeter[i]);
				max = TerrainCoord.Max(max, perimeter[i]);
			}
		}
		else
		{
			min = (max = TerrainCoord.Invalid);
		}
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref min);
		reflector.Add(ref max);
	}

	public static bool operator ==(TerrainRect a, TerrainRect b)
	{
		if (a.min == b.min)
		{
			return a.max == b.max;
		}
		return false;
	}

	public static bool operator !=(TerrainRect a, TerrainRect b)
	{
		if (!(a.min != b.min))
		{
			return a.max != b.max;
		}
		return true;
	}

	public static TerrainRect operator /(TerrainRect v, int a)
	{
		return new TerrainRect(v.min / a, v.max / a);
	}

	public bool Equals(TerrainRect other)
	{
		if (min == other.min)
		{
			return max == other.max;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TerrainRect)
		{
			return this == (TerrainRect)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return min.GetHashCode() ^ max.GetHashCode();
	}

	public override string ToString()
	{
		return "(" + min.ToString() + ") (" + max.ToString() + ")";
	}

	public bool Contains(TerrainCoord tile)
	{
		if (tile.x >= min.x && tile.y >= min.y && tile.x <= max.x)
		{
			return tile.y <= max.y;
		}
		return false;
	}

	public bool Contains(TerrainRect rect)
	{
		if (rect.min.x >= min.x && rect.min.y >= min.y && rect.max.x <= max.x)
		{
			return rect.max.y <= max.y;
		}
		return false;
	}

	public bool Overlaps(TerrainRect rect)
	{
		if (max.x >= rect.min.x && max.y >= rect.min.y && min.x <= rect.max.x)
		{
			return min.y <= rect.max.y;
		}
		return false;
	}

	public bool Overlaps(TerrainCoord minTile, TerrainCoord maxTile)
	{
		if (max.x >= minTile.x && max.y >= minTile.y && min.x <= maxTile.x)
		{
			return min.y <= maxTile.y;
		}
		return false;
	}

	public bool Intersect(TerrainRect other, out TerrainRect result)
	{
		if (Overlaps(other))
		{
			result = new TerrainRect(TerrainCoord.Max(min, other.min), TerrainCoord.Min(max, other.max));
			return true;
		}
		result = default(TerrainRect);
		return false;
	}

	public TerrainRect Expand(int amount)
	{
		return new TerrainRect(min - new TerrainCoord(amount, amount), max + new TerrainCoord(amount, amount));
	}

	public TerrainRect Expand(TerrainCoord extend)
	{
		return new TerrainRect(min - extend, max + extend);
	}

	public TerrainRect Include(TerrainCoord tile)
	{
		return new TerrainRect(TerrainCoord.Min(min, tile), TerrainCoord.Max(max, tile));
	}

	public TerrainRect Include(TerrainRect rect)
	{
		return new TerrainRect(TerrainCoord.Min(min, rect.min), TerrainCoord.Max(max, rect.max));
	}

	public TerrainCoord ClampTileWithinBounds(TerrainCoord tile)
	{
		tile.x = Math.Max(tile.x, min.x);
		tile.y = Math.Max(tile.y, min.y);
		tile.x = Math.Min(tile.x, max.x);
		tile.y = Math.Min(tile.y, max.y);
		return tile;
	}

	public float GetClosestDistSqTo(TerrainCoord other)
	{
		if (other.x > max.x)
		{
			if (other.y > max.y)
			{
				return max.GetDistSquared(other);
			}
			if (other.y < min.y)
			{
				return new TerrainCoord(max.x, min.y).GetDistSquared(other);
			}
			return MathUtil.Squared(other.x - max.x);
		}
		if (other.x < min.x)
		{
			if (other.y > max.y)
			{
				return new TerrainCoord(min.x, max.y).GetDistSquared(other);
			}
			if (other.y < min.y)
			{
				return min.GetDistSquared(other);
			}
			return MathUtil.Squared(other.x - min.x);
		}
		if (other.y > max.y)
		{
			return MathUtil.Squared(other.y - max.y);
		}
		if (other.y < min.y)
		{
			return MathUtil.Squared(other.y - min.y);
		}
		return 0f;
	}

	public TerrainCoord GetNearestCoordWithinRectTo(TerrainCoord other)
	{
		int x = Math.Max(min.x, Math.Min(max.x, other.x));
		int y = Math.Max(min.y, Math.Min(max.y, other.y));
		return new TerrainCoord(x, y);
	}

	public float GetClosestDistSqToRect(TerrainRect other)
	{
		if (Overlaps(other))
		{
			return 0f;
		}
		return GetClosestDistSqTo(other.GetNearestCoordWithinRectTo((min + max) / 2));
	}

	public static void TestTile(TerrainCoord startTile, TerrainCoord tile, int options, Character requester, TileObject ignore, ref TerrainCoord bestTile, ref float bestDistSq, bool mustBeSpawnable = false, bool checkPath = false)
	{
		float distSquared = tile.GetDistSquared(startTile);
		if (!(distSquared < bestDistSq) && (distSquared != bestDistSq || requester == null || !(requester.Tile.GetDistSquared(tile) < requester.Tile.GetDistSquared(bestTile))))
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		if (instance.IsImpassable(tile.x, tile.y, options, requester, ignore) || (mustBeSpawnable && !instance.IsTileSpawnable(tile.x, tile.y)))
		{
			return;
		}
		if (checkPath && requester != null)
		{
			Vector2 tileCentreXZ = instance.GetTileCentreXZ(tile);
			Vector3 vector = MathUtil.ToX0Y(tileCentreXZ - requester.PosXZ);
			float magnitude = vector.magnitude;
			if (magnitude > 0.01f)
			{
				Ray ray = new Ray(requester.Pos, vector / magnitude);
				int options2 = 26112;
				if ((MathUtil.ToXZ(instance.TraceCollisions(ray, vector.magnitude, options2, requester, null, out var _)) - tileCentreXZ).sqrMagnitude > 0.01f)
				{
					return;
				}
			}
		}
		bestDistSq = distSquared;
		bestTile = tile;
	}

	public TerrainCoord GetNearestPassableTileTo(TerrainCoord tile, int options, Character requester, TileObject ignore, bool mustBeSpawnable = false, bool checkPath = false)
	{
		TerrainCoord terrainCoord = min;
		TerrainCoord terrainCoord2 = max;
		TerrainCoord terrainCoord3 = new TerrainCoord(Math.Max(terrainCoord.x - 1, Math.Min(terrainCoord2.x + 1, tile.x)), Math.Max(terrainCoord.y - 1, Math.Min(terrainCoord2.y + 1, tile.y)));
		GameTerrain instance = GameTerrain.Instance;
		if (instance.IsImpassable(terrainCoord3.x, terrainCoord3.y, options, requester, ignore) || (mustBeSpawnable && (!instance.IsTileSpawnable(terrainCoord3.x, terrainCoord3.y) || instance.IsTileIllegal(terrainCoord3.x, terrainCoord3.y))))
		{
			int num = 1;
			TerrainCoord bestTile = TerrainCoord.Invalid;
			do
			{
				if (checkPath && num >= 10)
				{
					checkPath = false;
					num = 1;
				}
				TerrainCoord terrainCoord4 = terrainCoord - new TerrainCoord(num, num);
				TerrainCoord terrainCoord5 = terrainCoord2 + new TerrainCoord(num, num);
				float bestDistSq = float.MaxValue;
				for (int i = terrainCoord4.x; i <= terrainCoord5.x; i++)
				{
					TestTile(terrainCoord3, new TerrainCoord(i, terrainCoord4.y), options, requester, ignore, ref bestTile, ref bestDistSq, mustBeSpawnable, checkPath);
				}
				for (int j = terrainCoord4.x; j <= terrainCoord5.x; j++)
				{
					TestTile(terrainCoord3, new TerrainCoord(j, terrainCoord5.y), options, requester, ignore, ref bestTile, ref bestDistSq, mustBeSpawnable, checkPath);
				}
				for (int k = terrainCoord4.y + 1; k <= terrainCoord5.y - 1; k++)
				{
					TestTile(terrainCoord3, new TerrainCoord(terrainCoord4.x, k), options, requester, ignore, ref bestTile, ref bestDistSq, mustBeSpawnable, checkPath);
				}
				for (int l = terrainCoord4.y + 1; l <= terrainCoord5.y - 1; l++)
				{
					TestTile(terrainCoord3, new TerrainCoord(terrainCoord5.x, l), options, requester, ignore, ref bestTile, ref bestDistSq, mustBeSpawnable, checkPath);
				}
				num++;
			}
			while (bestTile == TerrainCoord.Invalid);
			return bestTile;
		}
		return terrainCoord3;
	}

	public TerrainCoord GetEdgeTileNearestTo(TerrainCoord tile)
	{
		if (Contains(tile))
		{
			Vector2 vector = Vector2.Lerp(new Vector2(min.x, min.y), new Vector2(max.x, max.y), 0.5f);
			if (Math.Abs((float)tile.x - vector.x) >= Math.Abs((float)tile.y - vector.y))
			{
				if ((float)tile.x >= vector.x)
				{
					return new TerrainCoord(max.x, tile.y);
				}
				return new TerrainCoord(min.x, tile.y);
			}
			if ((float)tile.y >= vector.y)
			{
				return new TerrainCoord(tile.x, max.y);
			}
			return new TerrainCoord(tile.x, min.y);
		}
		return GetNearestCoordWithinRectTo(tile);
	}
}
