using UnityEngine;

public struct CubicBezier2D : IReflectable
{
	public Vector2 p0;

	public Vector2 p1;

	public Vector2 p2;

	public Vector2 p3;

	public Vector2 EvaluatePos(float t)
	{
		return MathUtil.Cubed(1f - t) * p0 + 3f * MathUtil.Squared(1f - t) * t * p1 + 3f * (1f - t) * MathUtil.Squared(t) * p2 + MathUtil.Cubed(t) * p3;
	}

	public Vector2 EvaluateDir(float t)
	{
		return MathUtil.SafeNormalize(3f * MathUtil.Squared(1f - t) * (p1 - p0) + 6f * (1f - t) * t * (p2 - p1) + 3f * MathUtil.Squared(t) * (p3 - p2), Vector2.zero);
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref p0);
		reflector.Add(ref p1);
		reflector.Add(ref p2);
		reflector.Add(ref p3);
	}
}
