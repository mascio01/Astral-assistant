using System.Collections.Generic;
using UnityEngine;

public class AStarTileContentsManager
{
	public Dictionary<int, AStarMoveableObstacle> AllMoveableObstacles = new Dictionary<int, AStarMoveableObstacle>();

	private List<AStarTileContents> _tileContents = new List<AStarTileContents>();

	private int _nextFreeIndex = -1;

	public AStarTileContents GetContents(ushort index)
	{
		return _tileContents[index];
	}

	public ushort CreateContents()
	{
		if (_nextFreeIndex == -1)
		{
			ushort result = (ushort)_tileContents.Count;
			_tileContents.Add(new AStarTileContents());
			if (_tileContents.Count >= 65535)
			{
				Debug.LogError("Too many objects in world!");
			}
			return result;
		}
		ushort num = (ushort)_nextFreeIndex;
		_nextFreeIndex = _tileContents[num].NextFreeIndex;
		return num;
	}

	public void DeleteContents(ushort index)
	{
		_tileContents[index].NextFreeIndex = _nextFreeIndex;
		_nextFreeIndex = index;
	}

	public void ClearAllContents()
	{
		foreach (AStarTileContents tileContent in _tileContents)
		{
			tileContent.MoveableObstacles.Clear();
			tileContent.MoveableObstacles = null;
		}
		_tileContents.Clear();
		_tileContents = null;
	}
}
