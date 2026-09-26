using UnityEngine;

public struct QuadraticBezier2D : IReflectable
{
	public Vector2 p0;

	public Vector2 p1;

	public Vector2 p2;

	public Vector2 EvaluatePos(float t)
	{
		return MathUtil.Squared(1f - t) * p0 + 2f * (1f - t) * t * p1 + MathUtil.Squared(t) * p2;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref p0);
		reflector.Add(ref p1);
		reflector.Add(ref p2);
	}
}
