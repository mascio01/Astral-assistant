using System.Collections.Generic;

public struct AStarTile
{
	public CompressedTerrainCoord CameFrom;

	public int SetMask;

	public int OpenSetIndex;

	public int ImpassableMask;

	public int FixedObstacleCommunityId;

	public ushort MoveableContentsIndex;

	public byte FixedObstacleType;

	public byte FixedObstaclePropProtoIndex;

	public byte FixedObstacleModelIndex;

	public List<AStarMoveableObstacle> MoveableObstacles
	{
		get
		{
			if (MoveableContentsIndex == 0)
			{
				return null;
			}
			return GameTerrain.Instance.AStar.AStarTileContentsManager.GetContents((ushort)(MoveableContentsIndex - 1)).MoveableObstacles;
		}
	}

	public void AddMoveableObstacle(AStarMoveableObstacle obstacle)
	{
		AStarTileContentsManager aStarTileContentsManager = GameTerrain.Instance.AStar.AStarTileContentsManager;
		if (MoveableContentsIndex == 0)
		{
			MoveableContentsIndex = (ushort)(aStarTileContentsManager.CreateContents() + 1);
		}
		aStarTileContentsManager.GetContents((ushort)(MoveableContentsIndex - 1)).MoveableObstacles.Add(obstacle);
	}

	public void RemoveMoveableObstacle(AStarMoveableObstacle obstacle)
	{
		AStarTileContentsManager aStarTileContentsManager = GameTerrain.Instance.AStar.AStarTileContentsManager;
		AStarTileContents contents = aStarTileContentsManager.GetContents((ushort)(MoveableContentsIndex - 1));
		contents.MoveableObstacles.Remove(obstacle);
		if (contents.MoveableObstacles.Count == 0)
		{
			aStarTileContentsManager.DeleteContents((ushort)(MoveableContentsIndex - 1));
			MoveableContentsIndex = 0;
		}
	}
}
