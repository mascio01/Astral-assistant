using UnityEngine;

public abstract class SpawnPoint : SingleTileProp
{
	public static bool DebugShowSpawnPoints;

	public override string Category => "SpawnPoints";

	public override bool IsTargetable()
	{
		if (!Session.Instance.Editor)
		{
			return DebugShowSpawnPoints;
		}
		return true;
	}

	public override float? Raycast(Ray ray, float length, int flags, ref Vector3 normal, ref Bone bone, ref Vector3 hitPosInBoneSpace, ref float plantCover, Character source, TileObject target)
	{
		if ((flags & 0x80) == 0)
		{
			return null;
		}
		if (GetBoundingBox().IntersectRay(ray, out var distance) && distance < length)
		{
			return distance;
		}
		return null;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		return MathUtil.CreateTranslation(Pos + new Vector3(0f, 0.5f, 0f));
	}

	public static bool GetDebugShowSpawnPoints()
	{
		return DebugShowSpawnPoints;
	}

	public static void SetDebugShowSpawnPoints(bool v)
	{
		DebugShowSpawnPoints = v;
		GameTerrain.Instance.BuildEntireMinimap();
	}

	public override bool WantUnityObjectVisible()
	{
		if (!Session.Instance.Editor)
		{
			return DebugShowSpawnPoints;
		}
		return true;
	}
}
