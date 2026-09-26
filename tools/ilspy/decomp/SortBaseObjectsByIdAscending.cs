using System.Collections.Generic;

public class SortBaseObjectsByIdAscending : IComparer<BaseObject>
{
	public static SortBaseObjectsByIdAscending Instance = new SortBaseObjectsByIdAscending();

	int IComparer<BaseObject>.Compare(BaseObject a, BaseObject b)
	{
		if (a.Id > b.Id)
		{
			return 1;
		}
		if (a.Id < b.Id)
		{
			return -1;
		}
		return 0;
	}
}
