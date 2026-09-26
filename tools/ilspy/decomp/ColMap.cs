using System.Collections.Generic;
using UnityEngine;

public class ColMap
{
	private int CurIdx;

	private List<Color32[]> Cols = new List<Color32[]>();

	public ColMap()
	{
		for (int i = 0; i < 5; i++)
		{
			Cols.Add(null);
		}
	}

	public void InitSize(int size)
	{
		CurIdx = (int)GameTerrain.GetMapSizeFromSize(size);
		if (Cols[CurIdx] == null)
		{
			Cols[CurIdx] = new Color32[size * size];
		}
	}

	public Color32[] GetColMap()
	{
		return Cols[CurIdx];
	}
}
