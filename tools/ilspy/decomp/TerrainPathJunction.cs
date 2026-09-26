using UnityEngine;

public class TerrainPathJunction : IReflectable
{
	public int PathIndex0;

	public int PathIndex1;

	public float PointIndex0;

	public float PointIndex1;

	public Vector3 Pos;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref PathIndex0);
		reflector.Add(ref PathIndex1);
		reflector.Add(ref PointIndex0);
		reflector.Add(ref PointIndex1);
		reflector.Add(ref Pos);
	}
}
