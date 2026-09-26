using UnityEngine;

public class FindImpassableObjectTracer : TerrainLineTracer
{
	public TileObject Ignore;

	public TileObject FoundObject;

	public FindImpassableObjectTracer(GameTerrain terrain)
		: base(terrain)
	{
	}

	protected override void OnStartTrace(TerrainCoord tile)
	{
		FoundObject = null;
	}

	protected override bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		if (Terrain.IsTileOutsideBounds(tile.x, tile.y))
		{
			return false;
		}
		TileObject fixedObjectOnTile = Terrain.GetFixedObjectOnTile(tile.x, tile.y);
		if (fixedObjectOnTile != null && fixedObjectOnTile != Ignore && fixedObjectOnTile.CoverType != CoverType.None)
		{
			FoundObject = fixedObjectOnTile;
			return true;
		}
		return false;
	}
}
