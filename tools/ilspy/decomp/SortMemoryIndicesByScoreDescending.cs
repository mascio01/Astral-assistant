using System.Collections.Generic;

public class SortMemoryIndicesByScoreDescending : IComparer<MemoryIndexScore>
{
	int IComparer<MemoryIndexScore>.Compare(MemoryIndexScore a, MemoryIndexScore b)
	{
		if (a.Score < b.Score)
		{
			return 1;
		}
		if (a.Score > b.Score)
		{
			return -1;
		}
		return 0;
	}
}
