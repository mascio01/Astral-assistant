using System;
using UnityEngine;

public class TerrainMaxHeightTracer : TerrainLineTracer
{
	private float _maxHeight;

	private bool _inBounds;

	public float Result => _maxHeight;

	public bool InBounds => _inBounds;

	public TerrainMaxHeightTracer(GameTerrain terrain)
		: base(terrain)
	{
	}

	protected override void OnStartTrace(TerrainCoord tile)
	{
		_maxHeight = float.NegativeInfinity;
		_inBounds = false;
	}

	protected override bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		if (Terrain.IsTileOutsideBounds(tile.x, tile.y))
		{
			return false;
		}
		_maxHeight = Math.Max(_maxHeight, Terrain.GetTileMaxHeight(tile.x, tile.y));
		_inBounds = true;
		return false;
	}
}
