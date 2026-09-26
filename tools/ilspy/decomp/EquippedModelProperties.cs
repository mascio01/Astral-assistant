using System;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class EquippedModelProperties
{
	[XmlIgnore]
	public PrefabSettings Owner;

	[NonSerialized]
	[XmlIgnore]
	public PrefabResource Prefab;

	[NonSerialized]
	[XmlIgnore]
	public PrefabResource LoadedAmmoPrefab;

	public string PrefabPath;

	public string LoadedAmmoPrefabPath;

	public EquippedAnim EquippedAnim;

	public Bone EquippedBone = Bone.RightHand;

	public Vector3 LocalPos;

	public Vector3 LocalRotation;

	public float LocalScale = 1f;

	public Vector3 MuzzleLocalPos;

	public Bone LoadedAmmoBone;

	public Vector3 LoadedAmmoLocalPos;

	public Vector3 LoadedAmmoLocalRotation;

	public float LoadedAmmoLocalScale = 1f;

	public IKPoint LeftHandIKPoint = new IKPoint();

	public IKPoint RightHandIKPoint = new IKPoint();

	public IKPoint CrouchingRightHandIKPoint = new IKPoint();

	public IKPoint RunningRightHandIKPoint = new IKPoint();

	public IKPoint CrouchIdleRightHandIKPoint = new IKPoint();

	public void CopyFrom(EquippedModelProperties other)
	{
		FieldInfo[] fields = typeof(EquippedModelProperties).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}
}
