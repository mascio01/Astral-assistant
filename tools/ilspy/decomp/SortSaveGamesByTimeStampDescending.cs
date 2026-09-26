using System.Collections.Generic;

public class SortSaveGamesByTimeStampDescending : IComparer<SaveGame>
{
	public static SortSaveGamesByTimeStampDescending Instance = new SortSaveGamesByTimeStampDescending();

	int IComparer<SaveGame>.Compare(SaveGame a, SaveGame b)
	{
		if (a.Data.Timestamp < b.Data.Timestamp)
		{
			return 1;
		}
		if (a.Data.Timestamp > b.Data.Timestamp)
		{
			return -1;
		}
		return 0;
	}
}
