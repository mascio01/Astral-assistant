using System.Collections.Generic;

public class ByteMap
{
	private int CurIdx;

	private List<byte[]> Cols = new List<byte[]>();

	public ByteMap()
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
			Cols[CurIdx] = new byte[size * size];
		}
	}

	public byte[] GetColMap()
	{
		return Cols[CurIdx];
	}
}
