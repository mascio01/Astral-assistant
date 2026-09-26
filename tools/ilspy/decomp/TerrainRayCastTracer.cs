using System.Collections.Generic;
using UnityEngine;

public class TerrainRayCastTracer : TerrainLineTracer
{
	private struct TestedObject
	{
		public BaseObject Obj;

		public float? HitDist;

		public Vector3 Normal;

		public TerrainCoord Tile;

		public Bone Bone;

		public Vector3 PosInBoneSpace;

		public float PlantCover;

		public TestedObject(BaseObject obj, float? hit_dist, Vector3 normal, TerrainCoord tile, Bone bone, Vector3 posInBoneSpace, float plantCover)
		{
			Obj = obj;
			HitDist = hit_dist;
			Normal = normal;
			Tile = tile;
			Bone = bone;
			PosInBoneSpace = posInBoneSpace;
			PlantCover = plantCover;
		}
	}

	private RaycastResult _result;

	public bool Predicted;

	public int Flags;

	public TileObject Ignore;

	public Character Source;

	public TileObject Target;

	public TerrainCoord StartTile;

	private List<AStarMoveableObstacle> AStarObstaclesCrossed = new List<AStarMoveableObstacle>();

	private List<TerrainCoord> LookupTilesCrossed = new List<TerrainCoord>();

	private List<TestedObject> TestedObjects = new List<TestedObject>();

	private static string OnFinishTraceStr = "OnFinishTrace";

	private static string OnCrossTileStr = "OnCrossTile";

	private static string GetResultStr = "GetResult";

	public RaycastResult Result => _result;

	public TerrainRayCastTracer(GameTerrain terrain)
		: base(terrain)
	{
	}

	public void CleanUp()
	{
		Ignore = null;
		Source = null;
		Target = null;
		AStarObstaclesCrossed.Clear();
		LookupTilesCrossed.Clear();
		TestedObjects.Clear();
		StartTile = TerrainCoord.Invalid;
	}

	protected override void OnStartTrace(TerrainCoord tile)
	{
		_result = default(RaycastResult);
		TestedObjects.Capacity = 50;
		TestedObjects.Clear();
		StartTile = tile;
	}

	protected override void OnFinishTrace(TerrainCoord tile, Ray ray, float length)
	{
		using (new UnityProfileMarker(OnFinishTraceStr))
		{
			_result = GetResult(ray, length, new TestedObject(null, length, -ray.direction, tile, Bone.Invalid, Vector3.zero, 0f));
		}
	}

	protected override bool OnCrossTile(TerrainCoord prevTile, TerrainCoord tile, Ray ray, float length)
	{
		using (new UnityProfileMarker(OnCrossTileStr))
		{
			if (Terrain.IsTileOutsideBounds(tile.x, tile.y))
			{
				Plane plane;
				if (tile.x > prevTile.x && tile.x >= Terrain.Size)
				{
					plane = new Plane(new Vector3(-1f, 0f, 0f), new Vector3((float)tile.x - Terrain.HalfSize, 0f, 0f));
				}
				else if (tile.x < prevTile.x && tile.x < 0)
				{
					plane = new Plane(new Vector3(1f, 0f, 0f), new Vector3((float)prevTile.x - Terrain.HalfSize, 0f, 0f));
				}
				else if (tile.y > prevTile.y && tile.y >= Terrain.Size)
				{
					plane = new Plane(new Vector3(0f, 0f, -1f), new Vector3(0f, 0f, (float)tile.y - Terrain.HalfSize));
				}
				else
				{
					if (tile.y >= prevTile.y || tile.y >= 0)
					{
						return false;
					}
					plane = new Plane(new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, (float)prevTile.y - Terrain.HalfSize));
				}
				if (!plane.Raycast(ray, out var enter))
				{
					_result = GetResult(ray, length, new TestedObject(Terrain, 0f, plane.normal, prevTile, Bone.Invalid, Vector3.zero, 0f));
					return true;
				}
				_result = GetResult(ray, length, new TestedObject(Terrain, enter, plane.normal, prevTile, Bone.Invalid, Vector3.zero, 0f));
				return true;
			}
			bool flag = (Flags & 0x4000) != 0;
			if (flag)
			{
				List<AStarMoveableObstacle> moveableObstacles = AStar.Tiles[tile.x, tile.y].MoveableObstacles;
				if (moveableObstacles != null)
				{
					foreach (AStarMoveableObstacle item2 in moveableObstacles)
					{
						if (!AStarObstaclesCrossed.Contains(item2))
						{
							AStarObstaclesCrossed.Add(item2);
						}
					}
				}
			}
			else if (Flags != 0)
			{
				TerrainCoord item = new TerrainCoord(tile.x / 8, tile.y / 8);
				if (!LookupTilesCrossed.Contains(item))
				{
					LookupTilesCrossed.Add(item);
				}
			}
			if ((Flags & 0x80000) == 0)
			{
				Vector3 normal;
				float? hit_dist = Terrain.RayCastAgainstTile(tile, ray, length, out normal, flag);
				if (hit_dist.HasValue)
				{
					_result = GetResult(ray, length, new TestedObject(Terrain, hit_dist, normal, tile, Bone.Invalid, Vector3.zero, 0f));
					return true;
				}
			}
			if (flag)
			{
				if ((Flags & 0x40000) != 0 && tile == StartTile)
				{
					return false;
				}
				BaseObjectType fixedObstacleType = (BaseObjectType)AStar.Tiles[tile.x, tile.y].FixedObstacleType;
				if (fixedObstacleType != BaseObjectType.Invalid)
				{
					PropPrototype propPrototype = ((AStar.Tiles[tile.x, tile.y].FixedObstaclePropProtoIndex != 0) ? GameTerrain.Instance.AStar.FixedPropPrototypes[AStar.Tiles[tile.x, tile.y].FixedObstaclePropProtoIndex - 1] : null);
					TileObject tileObject = ((propPrototype != null) ? propPrototype.ProtoInstance : BaseObjectManager.PrototypeGameObjects[(int)fixedObstacleType]) as SingleTileObject;
					if (tileObject != null)
					{
						byte fixedObstacleModelIndex = AStar.Tiles[tile.x, tile.y].FixedObstacleModelIndex;
						float? hit_dist2 = tileObject.RaycastThreadSafe(ray, length, null, tile, fixedObstacleModelIndex, (Flags & 0x1000) != 0);
						if (hit_dist2.HasValue)
						{
							_result = GetResult(ray, length, new TestedObject(Terrain, hit_dist2, -ray.direction, tile, Bone.Invalid, Vector3.zero, 0f));
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private RaycastResult GetResult(Ray ray, float length, TestedObject best)
	{
		TerrainCoord tile = best.Tile;
		TerrainRect terrainRect = new TerrainRect(StartTile, StartTile).Include(tile);
		using (new UnityProfileMarker(GetResultStr))
		{
			if ((Flags & 0x4000) != 0)
			{
				foreach (AStarMoveableObstacle item in AStarObstaclesCrossed)
				{
					if (item.Object != Ignore && item.Object != Source && item.Object != Target && !HasTestedObject(item.Object) && terrainRect.Overlaps(item.MinTile, item.MaxTile))
					{
						Bone bone = Bone.Invalid;
						Vector3 zero = Vector3.zero;
						Vector3 normal = -ray.direction;
						float? hit_dist = item.Object.RaycastThreadSafe(ray, length, item, item.Object.GetCentreTile(), 0, (Flags & 0x1000) != 0);
						TerrainCoord tile2 = (hit_dist.HasValue ? Terrain.GetTileCoordForPos(ray.origin + ray.direction * hit_dist.Value) : TerrainCoord.Invalid);
						TestedObjects.Add(new TestedObject(item.Object, hit_dist, normal, tile2, bone, zero, 0f));
					}
				}
			}
			else
			{
				for (int i = 0; i < LookupTilesCrossed.Count; i++)
				{
					TerrainCoord terrainCoord = LookupTilesCrossed[i];
					List<TileObject> objectsInLookupSquareUsingLookupCoords = Terrain.GetObjectsInLookupSquareUsingLookupCoords(terrainCoord.x, terrainCoord.y);
					if (objectsInLookupSquareUsingLookupCoords == null)
					{
						continue;
					}
					foreach (TileObject item2 in objectsInLookupSquareUsingLookupCoords)
					{
						TileObject tileObject = (Predicted ? item2.GetPredictedOrElseThis() : item2);
						if (tileObject != Ignore && tileObject != Source && !HasTestedObject(tileObject) && terrainRect.Overlaps(item2.GetMinTile(), item2.GetMaxTile()))
						{
							Bone bone2 = Bone.Invalid;
							Vector3 hitPosInBoneSpace = Vector3.zero;
							Vector3 normal2 = -ray.direction;
							float plantCover = 0f;
							float? hit_dist2 = tileObject.Raycast(ray, length, Flags, ref normal2, ref bone2, ref hitPosInBoneSpace, ref plantCover, Source, Target);
							TerrainCoord tile3 = (hit_dist2.HasValue ? Terrain.GetTileCoordForPos(ray.origin + ray.direction * hit_dist2.Value) : TerrainCoord.Invalid);
							TestedObjects.Add(new TestedObject(tileObject, hit_dist2, normal2, tile3, bone2, hitPosInBoneSpace, plantCover));
						}
					}
				}
				if (Predicted)
				{
					foreach (TileObject predictedObject in PredictedObjectManager.Instance.PredictedObjects)
					{
						if (predictedObject != Ignore && predictedObject != Source && !HasTestedObject(predictedObject) && terrainRect.Overlaps(predictedObject.GetMinTile(), predictedObject.GetMaxTile()))
						{
							Bone bone3 = Bone.Invalid;
							Vector3 hitPosInBoneSpace2 = Vector3.zero;
							Vector3 normal3 = -ray.direction;
							float plantCover2 = 0f;
							float? hit_dist3 = predictedObject.Raycast(ray, length, Flags, ref normal3, ref bone3, ref hitPosInBoneSpace2, ref plantCover2, Source, Target);
							TerrainCoord tile4 = (hit_dist3.HasValue ? Terrain.GetTileCoordForPos(ray.origin + ray.direction * hit_dist3.Value) : TerrainCoord.Invalid);
							TestedObjects.Add(new TestedObject(predictedObject, hit_dist3, normal3, tile4, bone3, hitPosInBoneSpace2, plantCover2));
						}
					}
				}
			}
			for (int j = 0; j < TestedObjects.Count; j++)
			{
				if (TestedObjects[j].HitDist.HasValue && TestedObjects[j].HitDist.Value < best.HitDist.Value && TestedObjects[j].PlantCover == 0f)
				{
					best = TestedObjects[j];
				}
			}
			float num = 0f;
			if ((Flags & 0x8000) != 0)
			{
				for (int k = 0; k < TestedObjects.Count; k++)
				{
					if (TestedObjects[k].HitDist.HasValue && TestedObjects[k].HitDist.Value < best.HitDist.Value)
					{
						num += TestedObjects[k].PlantCover;
					}
				}
			}
			if ((Flags & 0x204) == 516)
			{
				if (best.Obj is Building building)
				{
					foreach (TestedObject testedObject in TestedObjects)
					{
						if (testedObject.Obj is Character character && character.InsideBuilding == building && testedObject.HitDist.HasValue && (best.Obj == building || testedObject.HitDist.Value < best.HitDist.Value))
						{
							best = testedObject;
						}
					}
				}
				TileObject tileObject2 = best.Obj as TileObject;
				if (tileObject2?.GetUnderConstructionInfo() != null)
				{
					foreach (TestedObject testedObject2 in TestedObjects)
					{
						if (testedObject2.Obj is Character character2 && character2.IsInsideBuildingUnderConstruction(tileObject2) && testedObject2.HitDist.HasValue && (best.Obj == tileObject2 || testedObject2.HitDist.Value < best.HitDist.Value))
						{
							best = testedObject2;
						}
					}
				}
			}
			return new RaycastResult(ray, best.Obj, best.HitDist.Value, best.Normal, best.Tile, best.Bone, best.PosInBoneSpace, num);
		}
	}

	private bool HasTestedObject(BaseObject obj)
	{
		for (int i = 0; i < TestedObjects.Count; i++)
		{
			if (TestedObjects[i].Obj == obj)
			{
				return true;
			}
		}
		return false;
	}
}
