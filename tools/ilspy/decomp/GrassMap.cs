using System;
using UnityEngine;

public class GrassMap
{
	private int Size;

	public Texture2D Tex;

	private static ColMap TileCol = new ColMap();

	private bool Dirty;

	public static uint[,] MaxDensityForPatch;

	public GrassMap(GameTerrain terrain)
	{
	}

	public void Init()
	{
	}

	public void InitSize(int size)
	{
		Size = size;
		TileCol.InitSize(size);
	}

	public void Unload()
	{
		Array.Clear(TileCol.GetColMap(), 0, TileCol.GetColMap().Length);
		Array.Clear(MaxDensityForPatch, 0, MaxDensityForPatch.Length);
	}

	public void UnityInit()
	{
		Tex = new Texture2D(Size, Size, TextureFormat.ARGB32, mipChain: false);
		Tex.wrapMode = TextureWrapMode.Clamp;
		Tex.filterMode = FilterMode.Point;
		ApplyGrassChangesIfNeeded(new TerrainCoord(0, 0), new TerrainCoord(Size - 1, Size - 1));
	}

	public void UnityDelete()
	{
		UnityEngine.Object.Destroy(Tex);
		Tex = null;
	}

	public void Reflect(Reflector reflector, int size)
	{
		Color32[] colMap = TileCol.GetColMap();
		for (int i = 0; i < size * size; i++)
		{
			reflector.Add(ref colMap[i]);
		}
		if (reflector.IsDeserialising)
		{
			Dirty = true;
		}
	}

	public void ApplyGrassChangesIfNeeded(TerrainCoord tl, TerrainCoord br)
	{
		if (!Dirty)
		{
			return;
		}
		GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref tl, ref br);
		Color32[] colMap = TileCol.GetColMap();
		uint[] array = new uint[8];
		for (int i = tl.x / 8; i <= br.x / 8; i++)
		{
			for (int j = tl.y / 8; j <= br.y / 8; j++)
			{
				Array.Clear(array, 0, array.Length);
				for (int k = 0; k < 8; k++)
				{
					for (int l = 0; l < 8; l++)
					{
						int num = i * 8 + k + (j * 8 + l) * Size;
						uint num2 = MathUtil.Color32ToUInt(colMap[num]);
						for (int m = 0; m < 8; m++)
						{
							int num3 = m * 4;
							uint val = (num2 >> num3) & 0xF;
							array[m] = Math.Max(array[m], val);
						}
					}
				}
				uint num4 = 0u;
				for (int n = 0; n < 8; n++)
				{
					int num5 = n * 4;
					num4 |= array[n] << num5;
				}
				MaxDensityForPatch[i, j] = num4;
			}
		}
		Tex.SetPixels32(colMap);
		Tex.Apply();
		Dirty = false;
	}

	public void SetGrassAmount(int x, int y, GrassType grassType, int amount)
	{
		if (x >= 0 && y >= 0 && x < Size && y < Size)
		{
			amount = Math.Min(amount, GrassRenderer.GetMaxDensityForGrassType(grassType));
			int num = x + y * Size;
			Color32[] colMap = TileCol.GetColMap();
			uint num2 = MathUtil.Color32ToUInt(colMap[num]);
			int num3 = (int)grassType * 4;
			uint num4 = num2;
			num4 &= (uint)(~(15 << num3));
			num4 |= (uint)(amount << num3);
			colMap[num] = MathUtil.UIntToColor32(num4);
			Dirty |= num4 != num2;
		}
	}

	public int GetGrassAmount(int x, int y, GrassType grassType)
	{
		if (x < 0 || y < 0 || x >= Size || y >= Size)
		{
			return 0;
		}
		int num = x + y * Size;
		uint num2 = MathUtil.Color32ToUInt(TileCol.GetColMap()[num]);
		int num3 = (int)grassType * 4;
		return (int)((num2 >> num3) & 0xF);
	}

	public int GetMaxDensityForPatch(int patchX, int patchY, GrassType grassType)
	{
		if (patchX < 0 || patchY < 0 || patchX >= Size / 8 || patchY >= Size / 8)
		{
			return 0;
		}
		int num = (int)grassType * 4;
		return (int)((MaxDensityForPatch[patchX, patchY] >> num) & 0xF);
	}
}
