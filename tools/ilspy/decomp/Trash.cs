using UnityEngine;

public class Trash : SingleTileProp
{
	public const int DefaultPoolSize = 10;

	public static bool DebugShowTrash = true;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Trash;
	}

	public override Matrix4x4 GetCustomModelTransform()
	{
		Vector2 offset = MathUtil.RandomVec2(new Vector2(Id + 100000, Id + 200000)) - new Vector2(0.5f, 0.5f);
		return GetCustomModelTransformTiltedWithGroundSlope(offset);
	}

	public override bool WantUnityObjectVisible()
	{
		return DebugShowTrash;
	}

	public static bool GetDebugShowTrash()
	{
		return DebugShowTrash;
	}

	public static void SetDebugShowTrash(bool v)
	{
		DebugShowTrash = v;
		Session.Instance.WantFullRefreshOfActiveUnityObjectsInFocusArea = true;
	}
}
