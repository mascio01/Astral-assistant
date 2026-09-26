using System;
using UnityEngine;

public class TerrainCollisionTracer : TerrainLineTracer
{
	public Vector3 Result;

	public TileObject HitObject;

	public int Options;

	public Character Requester;

	public TileObject Ignore;

	public TerrainCollisionTracer(GameTerrain terrain)
		: base(terrain)
	{
	}

	protected override void OnStartTrace(TerrainCoord tile)
	{
		HitObject = null;
	}

	protected override void OnFinishTrace(TerrainCoord tile, Ray ray, float length)
	{
		Result = ray.origin + ray.direction * length;
	}

	protected override bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		if (tile == Terrain.GetTileCoordForPos(ray.origin))
		{
			return false;
		}
		if (Terrain.IsImpassable(tile.x, tile.y, Options, Requester, Ignore) && DoesRayIntersectTile(ray, length, tile, out var best))
		{
			Result = ray.origin + ray.direction * best * length;
			HitObject = Terrain.GetFixedObjectOnTile(tile.x, tile.y);
			return true;
		}
		TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(tile.x, tile.y);
		if (fixedObjectOnTile is ITrap trap && trap.CanTriggerTrap(Requester) && !Requester.IsAwareOfTrap(fixedObjectOnTile) && DoesRayIntersectTile(ray, length, tile, out best))
		{
			Result = ray.origin + ray.direction * Math.Min(length, best * length + 0.1f);
			HitObject = Terrain.GetFixedObjectOnTile(tile.x, tile.y);
			return true;
		}
		return false;
	}

	private bool DoesRayIntersectTile(Ray ray, float length, TerrainCoord tile, out float best)
	{
		best = float.MaxValue;
		Vector2 a = MathUtil.ToXZ(ray.origin);
		Vector2 a2 = MathUtil.ToXZ(ray.origin + ray.direction * length);
		Terrain.GetTileRectXZ(tile.x, tile.y, out var minXZ, out var maxXZ);
		minXZ -= Requester.Radius * Vector2.one;
		maxXZ += Requester.Radius * Vector2.one;
		if (MathUtil.GetLineIntersection(a, a2, minXZ, new Vector2(maxXZ.x, minXZ.y), out var a3, out var b) && a3 >= 0f && a3 <= 1f && b >= 0f && b <= 1f)
		{
			best = Math.Min(best, a3);
		}
		if (MathUtil.GetLineIntersection(a, a2, new Vector2(maxXZ.x, minXZ.y), maxXZ, out a3, out b) && a3 >= 0f && a3 <= 1f && b >= 0f && b <= 1f)
		{
			best = Math.Min(best, a3);
		}
		if (MathUtil.GetLineIntersection(a, a2, maxXZ, new Vector2(minXZ.x, maxXZ.y), out a3, out b) && a3 >= 0f && a3 <= 1f && b >= 0f && b <= 1f)
		{
			best = Math.Min(best, a3);
		}
		if (MathUtil.GetLineIntersection(a, a2, new Vector2(minXZ.x, maxXZ.y), minXZ, out a3, out b) && a3 >= 0f && a3 <= 1f && b >= 0f && b <= 1f)
		{
			best = Math.Min(best, a3);
		}
		return best < float.MaxValue;
	}
}
