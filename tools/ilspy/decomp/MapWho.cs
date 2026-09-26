using System;
using System.Collections.Generic;
using UnityEngine;

public class MapWho<T> where T : TileObject
{
	public delegate bool FilterFunc(T obj);

	public int SquareSize;

	public static List<T>[,] Squares;

	public MapWho(int squareSize)
	{
		SquareSize = squareSize;
		if (Squares == null || Squares.GetLength(0) != GameTerrain.MaxSize / SquareSize)
		{
			Squares = new List<T>[GameTerrain.MaxSize / SquareSize, GameTerrain.MaxSize / SquareSize];
		}
	}

	public void InitSize(int terrainSize)
	{
	}

	public void Unload()
	{
		if (Squares != null)
		{
			Array.Clear(Squares, 0, Squares.Length);
		}
	}

	private bool IsSquareWithinBounds(TerrainCoord square)
	{
		int num = GameTerrain.Instance.Size / SquareSize;
		if (square.x >= 0 && square.y >= 0 && square.x < num)
		{
			return square.y < num;
		}
		return false;
	}

	public void AddToMapWho(T obj, TerrainCoord tile)
	{
		TerrainCoord square = tile / SquareSize;
		if (IsSquareWithinBounds(square))
		{
			if (Squares[square.x, square.y] == null)
			{
				Squares[square.x, square.y] = new List<T>();
			}
			Squares[square.x, square.y].Add(obj);
		}
	}

	public void RemoveFromMapWho(T obj, TerrainCoord tile)
	{
		TerrainCoord terrainCoord = tile / SquareSize;
		if (IsSquareWithinBounds(terrainCoord))
		{
			int num = ((Squares[terrainCoord.x, terrainCoord.y] != null) ? Squares[terrainCoord.x, terrainCoord.y].IndexOf(obj) : (-1));
			if (num != -1)
			{
				Squares[terrainCoord.x, terrainCoord.y].RemoveAt(num);
			}
			else
			{
				string displayNameString = obj.GetDisplayNameString();
				TerrainCoord terrainCoord2 = terrainCoord;
				Debug.LogWarning("Couldn't find " + displayNameString + " when trying to remove from MapWho at " + terrainCoord2.ToString());
			}
			if (Squares[terrainCoord.x, terrainCoord.y].Count == 0)
			{
				Squares[terrainCoord.x, terrainCoord.y] = null;
			}
		}
	}

	public void OnMoved(T obj, TerrainCoord oldTile, TerrainCoord newTile)
	{
		TerrainCoord terrainCoord = oldTile / SquareSize;
		TerrainCoord terrainCoord2 = newTile / SquareSize;
		if (!(terrainCoord != terrainCoord2))
		{
			return;
		}
		bool num = IsSquareWithinBounds(terrainCoord);
		bool flag = IsSquareWithinBounds(terrainCoord2);
		if (flag && Squares[terrainCoord2.x, terrainCoord2.y] == null)
		{
			Squares[terrainCoord2.x, terrainCoord2.y] = new List<T>();
		}
		if (num)
		{
			int num2 = ((Squares[terrainCoord.x, terrainCoord.y] != null) ? Squares[terrainCoord.x, terrainCoord.y].IndexOf(obj) : (-1));
			if (num2 != -1)
			{
				Squares[terrainCoord.x, terrainCoord.y].RemoveAt(num2);
			}
			else
			{
				string displayNameString = obj.GetDisplayNameString();
				TerrainCoord terrainCoord3 = terrainCoord;
				Debug.LogWarning("Couldn't find " + displayNameString + " when trying to remove from MapWho at " + terrainCoord3.ToString());
			}
		}
		if (flag)
		{
			Squares[terrainCoord2.x, terrainCoord2.y].Add(obj);
		}
		if (num && Squares[terrainCoord.x, terrainCoord.y].Count == 0)
		{
			Squares[terrainCoord.x, terrainCoord.y] = null;
		}
	}

	public void GetObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, List<T> objectsInRect)
	{
		objectsInRect.Clear();
		GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / SquareSize; i <= maxTile.x / SquareSize; i++)
		{
			for (int j = minTile.y / SquareSize; j <= maxTile.y / SquareSize; j++)
			{
				List<T> list = Squares[i, j];
				if (list != null)
				{
					for (int k = 0; k < list.Count; k++)
					{
						objectsInRect.Add(list[k]);
					}
				}
			}
		}
	}

	public bool AreAnyObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, T ignore)
	{
		GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / SquareSize; i <= maxTile.x / SquareSize; i++)
		{
			for (int j = minTile.y / SquareSize; j <= maxTile.y / SquareSize; j++)
			{
				List<T> list = Squares[i, j];
				if (list == null)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] != ignore && list[k].GetCentreTile().IsWithinBounds(minTile, maxTile))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void GetObjectsInRect(Vector2 minXZ, Vector2 maxXZ, List<T> objectsInRect)
	{
		objectsInRect.Clear();
		TerrainCoord tl = GameTerrain.Instance.GetTileCoordForPosXZ(minXZ);
		TerrainCoord br = GameTerrain.Instance.GetTileCoordForPosXZ(maxXZ);
		GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref tl, ref br);
		for (int i = tl.x / SquareSize; i <= br.x / SquareSize; i++)
		{
			for (int j = tl.y / SquareSize; j <= br.y / SquareSize; j++)
			{
				List<T> list = Squares[i, j];
				if (list == null)
				{
					continue;
				}
				for (int k = 0; k < list.Count; k++)
				{
					Vector2 posXZ = list[k].PosXZ;
					if (posXZ.x >= minXZ.x && posXZ.x <= maxXZ.x && posXZ.y >= minXZ.y && posXZ.y <= maxXZ.y)
					{
						objectsInRect.Add(list[k]);
					}
				}
			}
		}
	}

	public void GetObjectsInRect(TerrainCoord minTile, TerrainCoord maxTile, List<TileObject> objectsInRect)
	{
		objectsInRect.Clear();
		GameTerrain.Instance.ClampMinMaxTileWithinBounds(ref minTile, ref maxTile);
		for (int i = minTile.x / SquareSize; i <= maxTile.x / SquareSize; i++)
		{
			for (int j = minTile.y / SquareSize; j <= maxTile.y / SquareSize; j++)
			{
				List<T> list = Squares[i, j];
				if (list != null)
				{
					for (int k = 0; k < list.Count; k++)
					{
						objectsInRect.Add(list[k]);
					}
				}
			}
		}
	}

	public T GetNearestObject(TerrainCoord tile, int maxDist, FilterFunc filterFunc)
	{
		TerrainCoord terrainCoord = tile / SquareSize;
		int i = 0;
		for (int num = Math.Min(maxDist, GameTerrain.Instance.Size - 1) / SquareSize; i <= num; i++)
		{
			if (i == 0)
			{
				T val = CheckSquare(terrainCoord.x, terrainCoord.y, filterFunc);
				if (val != null)
				{
					return val;
				}
				continue;
			}
			for (int j = terrainCoord.x - i; j <= terrainCoord.x + i; j++)
			{
				T val2 = CheckSquare(j, terrainCoord.y - i, filterFunc);
				if (val2 != null)
				{
					return val2;
				}
			}
			for (int k = terrainCoord.x - i; k <= terrainCoord.x + i; k++)
			{
				T val3 = CheckSquare(k, terrainCoord.y + i, filterFunc);
				if (val3 != null)
				{
					return val3;
				}
			}
			for (int l = terrainCoord.y - i + 1; l <= terrainCoord.y + i - 1; l++)
			{
				T val4 = CheckSquare(terrainCoord.x - i, l, filterFunc);
				if (val4 != null)
				{
					return val4;
				}
			}
			for (int m = terrainCoord.y - i + 1; m <= terrainCoord.y + i - 1; m++)
			{
				T val5 = CheckSquare(terrainCoord.x + i, m, filterFunc);
				if (val5 != null)
				{
					return val5;
				}
			}
		}
		return null;
	}

	public T CheckSquare(int x, int y, FilterFunc filterFunc)
	{
		if (IsSquareWithinBounds(new TerrainCoord(x, y)))
		{
			List<T> list = Squares[x, y];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (filterFunc == null || filterFunc(list[i]))
					{
						return list[i];
					}
				}
			}
		}
		return null;
	}

	public T GetNearestObjectMoreAccurate(Vector2 posXZ, float maxDist)
	{
		TerrainCoord tileCoordForPosXZ = GameTerrain.Instance.GetTileCoordForPosXZ(posXZ);
		TerrainCoord terrainCoord = tileCoordForPosXZ / SquareSize;
		TerrainCoord terrainCoord2 = new TerrainCoord((tileCoordForPosXZ.x < terrainCoord.x * SquareSize + SquareSize / 2) ? (terrainCoord.x - 1) : terrainCoord.x, (tileCoordForPosXZ.y < terrainCoord.y * SquareSize + SquareSize / 2) ? (terrainCoord.y - 1) : terrainCoord.y);
		TerrainCoord terrainCoord3 = terrainCoord2 + new TerrainCoord(1, 1);
		int i = 0;
		for (int num = Math.Min(Mathf.CeilToInt(maxDist), GameTerrain.Instance.Size - 1) / SquareSize; i < num; i++)
		{
			T best = null;
			float bestDistSq = float.MaxValue;
			for (int j = terrainCoord2.x; j <= terrainCoord3.x; j++)
			{
				CheckSquareMoreAccurate(posXZ, new TerrainCoord(j, terrainCoord2.y), ref best, ref bestDistSq);
			}
			for (int k = terrainCoord2.x; k <= terrainCoord3.x; k++)
			{
				CheckSquareMoreAccurate(posXZ, new TerrainCoord(k, terrainCoord3.y), ref best, ref bestDistSq);
			}
			for (int l = terrainCoord2.y + 1; l <= terrainCoord3.y - 1; l++)
			{
				CheckSquareMoreAccurate(posXZ, new TerrainCoord(terrainCoord2.x, l), ref best, ref bestDistSq);
			}
			for (int m = terrainCoord2.y + 1; m <= terrainCoord3.y - 1; m++)
			{
				CheckSquareMoreAccurate(posXZ, new TerrainCoord(terrainCoord3.x, m), ref best, ref bestDistSq);
			}
			if (best != null)
			{
				return best;
			}
			terrainCoord2 -= new TerrainCoord(1, 1);
			terrainCoord3 += new TerrainCoord(1, 1);
		}
		return null;
	}

	private void CheckSquareMoreAccurate(Vector2 posXZ, TerrainCoord square, ref T best, ref float bestDistSq)
	{
		if (!IsSquareWithinBounds(square))
		{
			return;
		}
		List<T> list = Squares[square.x, square.y];
		if (list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			float sqrMagnitude = (list[i].PosXZ - posXZ).sqrMagnitude;
			if (sqrMagnitude < bestDistSq)
			{
				bestDistSq = sqrMagnitude;
				best = list[i];
			}
		}
	}
}
