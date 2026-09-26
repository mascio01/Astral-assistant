using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMA.PoseTools;

[Serializable]
public class UMABonePose : ScriptableObject
{
	[Serializable]
	public class PoseBone
	{
		public string bone;

		public int hash;

		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;
	}

	public PoseBone[] poses;

	public UMABonePose[] tweenPoses;

	public float[] tweenWeights;

	private void Reset()
	{
		poses = new PoseBone[0];
	}

	private void OnEnable()
	{
		if (poses == null)
		{
			poses = new PoseBone[0];
		}
		PoseBone[] array = poses;
		foreach (PoseBone poseBone in array)
		{
			if (poseBone.hash == 0)
			{
				poseBone.hash = Animator.StringToHash(poseBone.bone);
			}
		}
	}

	public int PoseCount()
	{
		if (poses != null)
		{
			return poses.Length;
		}
		return 0;
	}

	protected float ApplyPoseTweens(UMASkeleton umaSkeleton, float weight)
	{
		int num = tweenPoses.Length;
		if (tweenWeights.Length != num)
		{
			Debug.LogError("Tween pose / weight mismatch!");
			return weight;
		}
		if (weight <= tweenWeights[0])
		{
			weight /= tweenWeights[0];
			tweenPoses[0].ApplyPose(umaSkeleton, weight);
			return 0f;
		}
		if (weight >= tweenWeights[num - 1])
		{
			float num2 = 1f - tweenWeights[num - 1];
			float num3 = (1f - weight) / num2;
			tweenPoses[num - 1].ApplyPose(umaSkeleton, num3);
			return 1f - num3;
		}
		int i;
		for (i = 1; weight > tweenWeights[i]; i++)
		{
		}
		float num4 = tweenWeights[i - 1];
		float num5 = tweenWeights[i];
		float num6 = num5 - num4;
		num4 = (num5 - weight) / num6;
		tweenPoses[i - 1].ApplyPose(umaSkeleton, num4);
		num5 = 1f - num4;
		tweenPoses[i].ApplyPose(umaSkeleton, num5);
		return 0f;
	}

	public void ApplyPose(UMASkeleton umaSkeleton, float weight)
	{
		if (poses == null || umaSkeleton == null)
		{
			Debug.LogError("Missing poses or skeleton!");
			return;
		}
		if (tweenPoses != null && tweenPoses.Length != 0 && weight < 1f)
		{
			weight = ApplyPoseTweens(umaSkeleton, weight);
		}
		if (!(weight <= 0f))
		{
			PoseBone[] array = poses;
			foreach (PoseBone poseBone in array)
			{
				umaSkeleton.Morph(poseBone.hash, poseBone.position, poseBone.scale, poseBone.rotation, weight);
			}
		}
	}

	private static void RecurseTransformsInPrefab(Transform root, List<Transform> transforms)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			transforms.Add(child);
			RecurseTransformsInPrefab(child, transforms);
		}
	}

	public static Transform[] GetTransformsInPrefab(Transform prefab)
	{
		List<Transform> list = new List<Transform>();
		RecurseTransformsInPrefab(prefab, list);
		return list.ToArray();
	}
}
