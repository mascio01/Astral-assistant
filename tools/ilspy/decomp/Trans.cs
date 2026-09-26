using UnityEngine;

public struct Trans : IReflectable
{
	public Vector3 Pos;

	public float Scale;

	public Quaternion Orientation;

	public static Trans identity = new Trans(Vector3.zero, 1f, Quaternion.identity);

	public Trans(Vector3 pos, float scale, Quaternion orientation)
	{
		Pos = pos;
		Scale = scale;
		Orientation = orientation;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Pos);
		reflector.Add(ref Scale);
		reflector.Add(ref Orientation);
	}
}
