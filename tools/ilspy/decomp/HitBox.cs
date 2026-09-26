using System;
using UnityEngine;

[Serializable]
public struct HitBox
{
	public Bone Bone;

	public InjuryLocation InjuryLocation;

	public Bounds Box;

	public HitBox(Bone bone, InjuryLocation injuryLocation, Bounds box)
	{
		Bone = bone;
		InjuryLocation = injuryLocation;
		Box = box;
	}
}
