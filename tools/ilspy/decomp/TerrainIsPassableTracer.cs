using UnityEngine;

public class TerrainIsPassableTracer : TerrainLineTracer
{
	private bool _hitObstacle;

	public bool Predicted;

	public int Options;

	public Character Requester;

	public TileObject Ignore;

	public TerrainCoord IgnoreTile;

	public bool Passable => !_hitObstacle;

	public TerrainIsPassableTracer(GameTerrain terrain)
		: base(terrain)
	{
	}

	protected override void OnStartTrace(TerrainCoord tile)
	{
		_hitObstacle = false;
	}

	protected override bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		if (tile == IgnoreTile)
		{
			return false;
		}
		int num = Options;
		if (Predicted && (Options & 3) != 0)
		{
			foreach (TileObject predictedObject in PredictedObjectManager.Instance.PredictedObjects)
			{
				if (predictedObject != Requester && predictedObject != Ignore && tile.IsWithinBounds(predictedObject.GetMinTile(), predictedObject.GetMaxTile()) && predictedObject.IsImpassable(Requester, Options, tile))
				{
					_hitObstacle = true;
					return true;
				}
			}
			num &= -4;
		}
		if (Terrain.IsImpassable(tile.x, tile.y, num, Requester, Ignore))
		{
			_hitObstacle = true;
			return true;
		}
		return false;
	}
}
