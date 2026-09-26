using System.Collections.Generic;

internal class TileObjectsSorter : IComparer<TileObject>
{
	public static TileObjectsSorter Instance = new TileObjectsSorter();

	private static int RemoveDuplicatesRunId;

	int IComparer<TileObject>.Compare(TileObject a, TileObject b)
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

	public static void RemoveDuplicates(List<TileObject> list, bool wantSort)
	{
		if (!wantSort && Util.AmIOnMainThread())
		{
			RemoveDuplicatesRunId++;
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				TileObject tileObject = list[i];
				if (tileObject.FoundInList != RemoveDuplicatesRunId)
				{
					tileObject.FoundInList = RemoveDuplicatesRunId;
					list[num] = tileObject;
					num++;
				}
			}
			list.RemoveRange(num, list.Count - num);
			return;
		}
		list.Sort(Instance);
		TileObject tileObject2 = null;
		int num2 = 0;
		for (int j = 0; j < list.Count; j++)
		{
			TileObject tileObject3 = list[j];
			if (tileObject3 != tileObject2)
			{
				tileObject2 = tileObject3;
				list[num2] = tileObject3;
				num2++;
			}
		}
		list.RemoveRange(num2, list.Count - num2);
	}
}
