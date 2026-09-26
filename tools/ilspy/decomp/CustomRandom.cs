using System;
using UnityEngine;

public class CustomRandom
{
	public uint Seed;

	public bool Locked;

	public static string RandTranslatedStringNumFormat = "D2";

	public CustomRandom()
	{
		Seed = (uint)DateTime.Now.Ticks;
	}

	public CustomRandom(int seed)
	{
		Seed = (uint)seed;
	}

	public int Next()
	{
		uint seed = Seed;
		seed *= 1103515245;
		seed += 12345;
		uint num = seed / 65536 % 2048;
		seed *= 1103515245;
		seed += 12345;
		uint num2 = (num << 10) ^ (seed / 65536 % 1024);
		seed *= 1103515245;
		seed += 12345;
		uint result = (num2 << 10) ^ (seed / 65536 % 1024);
		Seed = seed;
		return (int)result;
	}

	public int Next(int maxValue)
	{
		return Next() % Math.Max(maxValue, 1);
	}

	public int Next(int minValue, int maxValue)
	{
		return minValue + Next() % Math.Max(maxValue - minValue, 1);
	}

	public double NextDouble()
	{
		return (double)Next() * 1E-06 % 1.0;
	}

	public int RandomTranslatedStringHash(string stem, int count)
	{
		return StringUtil.JenkinsHash(stem + (Next(count) + 1).ToString(RandTranslatedStringNumFormat));
	}

	public int RandomTranslatedStringHash(string stem)
	{
		int i;
		string result;
		for (i = 0; i < 100 && GameImpl.Instance.TryTranslate(StringUtil.JenkinsHash(stem + (i + 1).ToString(RandTranslatedStringNumFormat)), out result, englishOnly: true); i++)
		{
		}
		return RandomTranslatedStringHash(stem, i);
	}

	public bool RandomChoice(float probability)
	{
		return NextDouble() <= (double)probability;
	}

	public float RandomFloat()
	{
		return (float)NextDouble();
	}

	public TerrainCoord RandomTile(TerrainCoord minTile, TerrainCoord maxTile)
	{
		return new TerrainCoord(Next(minTile.x, maxTile.x + 1), Next(minTile.y, maxTile.y + 1));
	}

	public TerrainCoord RandomTileOnOutsideEdge(TerrainCoord minTile, TerrainCoord maxTile, int range)
	{
		minTile -= new TerrainCoord(range, range);
		maxTile += new TerrainCoord(range, range);
		int num = maxTile.x + 1 - minTile.x;
		int num2 = maxTile.y + 1 - minTile.y;
		int num3 = Next(num * range * 2 + (num2 - range * 2) * 2);
		if (num3 < num * range)
		{
			return new TerrainCoord(Next(minTile.x, maxTile.x + 1), minTile.y - Next(range));
		}
		if (num3 < num * range * 2)
		{
			return new TerrainCoord(Next(minTile.x, maxTile.x + 1), maxTile.y + Next(range));
		}
		if (num3 < num * range * 2 + num2 - range * 2)
		{
			return new TerrainCoord(minTile.x - Next(range), Next(minTile.y + range, maxTile.y + 1 - range));
		}
		return new TerrainCoord(maxTile.x + Next(range), Next(minTile.y + range, maxTile.y + 1 - range));
	}

	public TerrainCoord RandomTileOnEdge(TerrainCoord minTile, TerrainCoord maxTile)
	{
		int num = maxTile.x + 1 - minTile.x;
		int num2 = maxTile.y + 1 - minTile.y;
		int num3 = Next(num * 2 + (num2 - 2) * 2);
		if (num3 < num)
		{
			return new TerrainCoord(Next(minTile.x, maxTile.x + 1), minTile.y);
		}
		if (num3 < num * 2)
		{
			return new TerrainCoord(Next(minTile.x, maxTile.x + 1), maxTile.y);
		}
		if (num3 < num * 2 + num2 - 2)
		{
			return new TerrainCoord(minTile.x, Next(minTile.y + 1, maxTile.y));
		}
		return new TerrainCoord(maxTile.x, Next(minTile.y + 1, maxTile.y));
	}

	public Vector2 RandomVec2()
	{
		return new Vector2((float)NextDouble() * 2f - 1f, (float)NextDouble() * 2f - 1f);
	}

	public Vector3 RandomVec3()
	{
		return new Vector3((float)NextDouble() * 2f - 1f, (float)NextDouble() * 2f - 1f, (float)NextDouble() * 2f - 1f);
	}

	public Prop.OrientationType RandomOrientationType()
	{
		return (Prop.OrientationType)Next(4);
	}

	public float GaussianRandom(float mean, float dev)
	{
		float f = RandomFloat();
		float num = RandomFloat();
		float num2 = Mathf.Sqrt(-2f * Mathf.Log(f)) * Mathf.Sin(MathF.PI * 2f * num);
		return mean + dev * num2;
	}
}
