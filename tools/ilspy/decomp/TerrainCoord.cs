using System;
using UnityEngine;

public struct TerrainCoord : IReflectable, IEquatable<TerrainCoord>
{
	public static TerrainCoord Invalid = new TerrainCoord(-1, -1);

	public static TerrainCoord Zero = new TerrainCoord(0, 0);

	public int x;

	public int y;

	public TerrainCoord(int _x, int _y)
	{
		x = _x;
		y = _y;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref x);
		reflector.Add(ref y);
	}

	public Vector2 AsVector2()
	{
		return new Vector2(x, y);
	}

	public static bool operator ==(TerrainCoord a, TerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(TerrainCoord a, TerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public bool Equals(TerrainCoord other)
	{
		if (x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TerrainCoord)
		{
			return this == (TerrainCoord)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x + (y << 16);
	}

	public bool IsAdjacent(TerrainCoord other)
	{
		if (other.x != x || other.y < y - 1 || other.y > y + 1)
		{
			if (other.y == y && other.x >= x - 1)
			{
				return other.x <= x + 1;
			}
			return false;
		}
		return true;
	}

	public bool IsWithinBounds(TerrainCoord min, TerrainCoord max)
	{
		if (x >= min.x && y >= min.y && x <= max.x)
		{
			return y <= max.y;
		}
		return false;
	}

	public float GetDistSquared(TerrainCoord other)
	{
		return new Vector2(x - other.x, y - other.y).sqrMagnitude;
	}

	public float GetDist(TerrainCoord other)
	{
		return new Vector2(x - other.x, y - other.y).magnitude;
	}

	public int GetDistManhattan(TerrainCoord other)
	{
		return Math.Abs(other.x - x) + Math.Abs(other.y - y);
	}

	public TerrainCoord GetDirTo(TerrainCoord dest)
	{
		return new TerrainCoord(MathUtil.Clamp(dest.x - x, -1, 1), MathUtil.Clamp(dest.y - y, -1, 1));
	}

	public TerrainCoord GetAdjacentTileInDir(Vector2 dirXZ)
	{
		float num = MathUtil.GetAngleFromDir(dirXZ, 0f) * 57.29578f;
		TerrainCoord terrainCoord = (((double)num < -157.5) ? new TerrainCoord(0, -1) : (((double)num < -112.5) ? new TerrainCoord(-1, -1) : (((double)num < -67.5) ? new TerrainCoord(-1, 0) : (((double)num < -22.5) ? new TerrainCoord(-1, 1) : (((double)num < 22.5) ? new TerrainCoord(0, 1) : (((double)num < 67.5) ? new TerrainCoord(1, 1) : (((double)num < 112.5) ? new TerrainCoord(1, 0) : ((!((double)num < 157.5)) ? new TerrainCoord(0, -1) : new TerrainCoord(1, -1)))))))));
		return this + terrainCoord;
	}

	public static TerrainCoord Min(TerrainCoord a, TerrainCoord b)
	{
		return new TerrainCoord(Math.Min(a.x, b.x), Math.Min(a.y, b.y));
	}

	public static TerrainCoord Max(TerrainCoord a, TerrainCoord b)
	{
		return new TerrainCoord(Math.Max(a.x, b.x), Math.Max(a.y, b.y));
	}

	public static TerrainCoord operator +(TerrainCoord a, TerrainCoord b)
	{
		return new TerrainCoord(a.x + b.x, a.y + b.y);
	}

	public static TerrainCoord operator -(TerrainCoord a, TerrainCoord b)
	{
		return new TerrainCoord(a.x - b.x, a.y - b.y);
	}

	public static TerrainCoord operator -(TerrainCoord a)
	{
		return new TerrainCoord(-a.x, -a.y);
	}

	public static TerrainCoord operator *(TerrainCoord v, int a)
	{
		return new TerrainCoord(v.x * a, v.y * a);
	}

	public static TerrainCoord operator /(TerrainCoord v, int a)
	{
		return new TerrainCoord(v.x / a, v.y / a);
	}

	public static bool Overlaps(TerrainCoord min0, TerrainCoord max0, TerrainCoord min1, TerrainCoord max1)
	{
		if (max0.x >= min1.x && max0.y >= min1.y && min0.x <= max1.x)
		{
			return min0.y <= max1.y;
		}
		return false;
	}

	public static bool ClampRectWithinBounds(ref TerrainCoord tl, ref TerrainCoord br, TerrainCoord boundsTL, TerrainCoord boundsBR)
	{
		tl.x = Math.Max(tl.x, boundsTL.x);
		tl.y = Math.Max(tl.y, boundsTL.y);
		br.x = Math.Min(br.x, boundsBR.x);
		br.y = Math.Min(br.y, boundsBR.y);
		if (br.x >= tl.x)
		{
			return br.y >= tl.y;
		}
		return false;
	}

	public CompressedTerrainCoord ToCompressed()
	{
		return new CompressedTerrainCoord((short)x, (short)y);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public override string ToString()
	{
		return x + ", " + y;
	}

	public void Normalize()
	{
		if (Math.Abs(x) >= Math.Abs(y))
		{
			x = Math.Sign(x);
			y = 0;
		}
		else
		{
			x = 0;
			y = Math.Sign(y);
		}
	}
}
