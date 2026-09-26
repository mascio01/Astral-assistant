using System.Collections.Generic;

public class TileContentsManager
{
	protected List<TileContents> _tileContents = new List<TileContents>();

	private int _nextFreeIndex = -1;

	public TileContents GetContents(int index)
	{
		return _tileContents[index];
	}

	public int CreateContents()
	{
		if (_nextFreeIndex == -1)
		{
			int count = _tileContents.Count;
			_tileContents.Add(new TileContents());
			return count;
		}
		int nextFreeIndex = _nextFreeIndex;
		_nextFreeIndex = _tileContents[nextFreeIndex].NextFreeIndex;
		return nextFreeIndex;
	}

	public void DeleteContents(int index)
	{
		_tileContents[index].NextFreeIndex = _nextFreeIndex;
		_nextFreeIndex = index;
	}

	public void ClearAllContents()
	{
		foreach (TileContents tileContent in _tileContents)
		{
			tileContent.Objects.Clear();
			tileContent.Objects = null;
		}
		_tileContents.Clear();
		_tileContents = null;
	}
}
