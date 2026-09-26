using UnityEngine;

public struct TerrainPathPoint : IReflectable
{
	public Vector3 Pos;

	public Vector2 DirXZ;

	public float Width;

	public float ControlPointIndex;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Pos);
		reflector.Add(ref DirXZ);
		reflector.AddAfter(ref Width, 224);
		reflector.AddAfter(ref ControlPointIndex, 451);
	}
}
