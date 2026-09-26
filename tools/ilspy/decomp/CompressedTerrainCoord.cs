using UnityEngine;

public struct CompressedTerrainCoord
{
	public static CompressedTerrainCoord Invalid = new CompressedTerrainCoord(-1, -1);

	public short x;

	public short y;

	public CompressedTerrainCoord(short _x, short _y)
	{
		x = _x;
		y = _y;
	}

	public static bool operator ==(CompressedTerrainCoord a, CompressedTerrainCoord b)
	{
		return ((ushort)a.x | ((ushort)a.y << 16)) == ((ushort)b.x | ((ushort)b.y << 16));
	}

	public static bool operator !=(CompressedTerrainCoord a, CompressedTerrainCoord b)
	{
		return ((ushort)a.x | ((ushort)a.y << 16)) != ((ushort)b.x | ((ushort)b.y << 16));
	}

	public static bool operator ==(CompressedTerrainCoord a, TerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(CompressedTerrainCoord a, TerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public static bool operator ==(TerrainCoord a, CompressedTerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(TerrainCoord a, CompressedTerrainCoord b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return (ushort)x | ((ushort)y << 16);
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CompressedTerrainCoord))
		{
			return false;
		}
		return this == (CompressedTerrainCoord)obj;
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

	public float GetDistTo(CompressedTerrainCoord other)
	{
		return new Vector2(x - other.x, y - other.y).magnitude;
	}

	public static CompressedTerrainCoord operator +(CompressedTerrainCoord a, CompressedTerrainCoord b)
	{
		return new CompressedTerrainCoord((short)(a.x + b.x), (short)(a.y + b.y));
	}

	public static CompressedTerrainCoord operator -(CompressedTerrainCoord a, CompressedTerrainCoord b)
	{
		return new CompressedTerrainCoord((short)(a.x - b.x), (short)(a.y - b.y));
	}

	public TerrainCoord ToUncompressed()
	{
		return new TerrainCoord(x, y);
	}
}
