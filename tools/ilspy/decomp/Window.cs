using UnityEngine;

public struct Window
{
	public Vector3 Pos;

	public Quaternion Rot;

	public float Scale;

	public Window(Vector3 pos, Quaternion rot, float scale)
	{
		Pos = pos;
		Rot = rot;
		Scale = scale;
	}
}
