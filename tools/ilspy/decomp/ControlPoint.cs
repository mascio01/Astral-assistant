using UnityEngine;

public struct ControlPoint : IReflectable
{
	public Vector2 Pos;

	public Vector2 Dir;

	public float Width;

	public float RiverDepth;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Pos);
		reflector.Add(ref Dir);
		reflector.AddAfter(ref Width, 224);
		reflector.AddAfter(ref RiverDepth, 224);
	}
}
