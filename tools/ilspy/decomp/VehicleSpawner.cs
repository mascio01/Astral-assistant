using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : DebugMenu
{
	public VehicleSpawner()
		: base(GameImpl.Translate("DEBUG_VehicleSpawner"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		InputFunctionManager instance3 = InputFunctionManager.Instance;
		if (!instance3.IsPressed(InputFunction.MainAction))
		{
			return;
		}
		RaycastResult raycastResult = instance.GameCamera.RayCastFromPointOnScreen(instance3.GetCursorPos(), 0);
		if (raycastResult.HitObject != instance2)
		{
			return;
		}
		Vector2 vector = MathUtil.ToXZ(raycastResult.GetHitPosition());
		TerrainPath nearestPath;
		Vector2 bestDirXZ;
		float pathIndex;
		Vector3? nearestPointOnPath = instance2.GetNearestPointOnPath(vector, river: false, 16f, out nearestPath, out bestDirXZ, out pathIndex);
		if (!nearestPointOnPath.HasValue)
		{
			return;
		}
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		TerrainCoord tileCoordForPosXZ = instance2.GetTileCoordForPosXZ(vector);
		Prop.OrientationType closestOrientationTypeToAngle = Prop.GetClosestOrientationTypeToAngle(MathUtil.GetAngleFromNormalizedDir(((Vector2.Dot(MathUtil.RightNormal(bestDirXZ), MathUtil.ToXZ(raycastResult.GetHitPosition() - nearestPointOnPath.Value)) < 0f) ? 1f : (-1f)) * bestDirXZ));
		List<PropPrototype> typesWithCategory = PropPrototype.GetTypesWithCategory("Vehicles");
		List<PropPrototype> typesWithCategory2 = PropPrototype.GetTypesWithCategory("Town/Props/Trash");
		PropPrototype propPrototype = PropPrototype.PickRandom(nonDeterministicRand, typesWithCategory);
		if (propPrototype == null)
		{
			return;
		}
		TileObject tileObject = TileObject.CreateProp(propPrototype);
		tileObject.SetTileGhost(tileCoordForPosXZ);
		tileObject.SetOrientationType(closestOrientationTypeToAngle);
		TileObject tileObject2 = instance2.SpawnTiltedPropIfNotBlocked(tileObject, checkSlope: false);
		if (tileObject2 != null)
		{
			GameTerrain.GenerateLoot(tileObject2, nonDeterministicRand);
			int num = nonDeterministicRand.Next(3);
			for (int i = 0; i < num; i++)
			{
				TerrainCoord tile = nonDeterministicRand.RandomTileOnOutsideEdge(tileObject2.GetMinTile(), tileObject2.GetMaxTile(), 1);
				instance2.GeneratePropCluster(tile, nonDeterministicRand, typesWithCategory2, nonDeterministicRand.Next(1, 8), notOnRoads: false, 0, null);
			}
		}
		Session.Instance.AchievementsEnabled = false;
	}
}
