using System;
using UnityEngine;

[Serializable]
public class SavedRootMotion : ScriptableObject
{
	[SerializeField]
	public AnimationCurve Curve;

	[SerializeField]
	public AnimationCurve CurveX;

	[SerializeField]
	public AnimationCurve CurveY;

	public float GetDist()
	{
		if (Curve.length < 2)
		{
			return 0f;
		}
		return Curve.keys[Curve.length - 1].value - Curve.keys[0].value;
	}
}
