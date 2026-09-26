using UnityEngine;

public class Collidable
{
	public CollidableType CollidableType;

	public string Name;

	public Matrix4x4 WorldFromLocalTrans;

	public Bounds Box;

	public BoundingSphere Sphere;

	public int[] Triangles;

	public Vector3[] Vertices;
}
