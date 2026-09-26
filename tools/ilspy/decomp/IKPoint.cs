using System;
using UnityEngine;

[Serializable]
public class IKPoint
{
	public Bone Bone;

	public float PosWeight;

	public float RotWeight;

	public Vector3 Pos;

	public Vector3 Rot;
}
