using UnityEngine;

public static class AudioSourceExtensions
{
	public static void RealisticRolloff(this AudioSource AS)
	{
		AnimationCurve animationCurve = new AnimationCurve(new Keyframe(AS.minDistance, 1f), new Keyframe(AS.minDistance + (AS.maxDistance - AS.minDistance) / 4f, 0.35f), new Keyframe(AS.maxDistance, 0f));
		AS.rolloffMode = AudioRolloffMode.Custom;
		animationCurve.SmoothTangents(1, 0.025f);
		AS.SetCustomCurve(AudioSourceCurveType.CustomRolloff, animationCurve);
		AS.dopplerLevel = 0f;
		AS.spread = 60f;
	}
}
