using System.Collections.Generic;

public struct MemoryIndexScore
{
	public int Index;

	public float Score;

	public static List<MemoryIndexScore> MemoryIndices = new List<MemoryIndexScore>();

	public static SortMemoryIndicesByScoreDescending Sorter = new SortMemoryIndicesByScoreDescending();
}
