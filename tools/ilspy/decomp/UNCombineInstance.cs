using UnityEngine;

public struct UNCombineInstance
{
	public Matrix4x4 transform;

	public Mesh mesh;

	public Vector2 densityOffset;

	public int density;

	public UNCombineInstance(Matrix4x4 transform, Mesh mesh, int density, CustomRandom rand)
	{
		this.transform = transform;
		this.mesh = mesh;
		this.density = density;
		densityOffset.x = Mathf.Lerp(-1f, 1f, rand.RandomFloat());
		densityOffset.y = Mathf.Lerp(-1f, 1f, rand.RandomFloat());
	}
}
