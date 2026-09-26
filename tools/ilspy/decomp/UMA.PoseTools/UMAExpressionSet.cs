using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMA.PoseTools;

[Serializable]
public class UMAExpressionSet : ScriptableObject
{
	[Serializable]
	public class PosePair
	{
		public UMABonePose primary;

		public UMABonePose inverse;
	}

	public PosePair[] posePairs = new PosePair[36];

	[NonSerialized]
	private int[] boneHashes;

	private void ValidateBoneHashes()
	{
		if (boneHashes != null)
		{
			return;
		}
		List<int> list = new List<int>();
		PosePair[] array = posePairs;
		foreach (PosePair posePair in array)
		{
			UMABonePose.PoseBone[] poses;
			if (posePair.primary != null)
			{
				poses = posePair.primary.poses;
				foreach (UMABonePose.PoseBone poseBone in poses)
				{
					if (!list.Contains(poseBone.hash))
					{
						list.Add(poseBone.hash);
					}
				}
			}
			if (!(posePair.inverse != null))
			{
				continue;
			}
			poses = posePair.inverse.poses;
			foreach (UMABonePose.PoseBone poseBone2 in poses)
			{
				if (!list.Contains(poseBone2.hash))
				{
					list.Add(poseBone2.hash);
				}
			}
		}
		boneHashes = list.ToArray();
	}

	public void RestoreBones(UMASkeleton umaSkeleton, bool logErrors = false)
	{
		if (umaSkeleton == null)
		{
			return;
		}
		ValidateBoneHashes();
		int[] array = boneHashes;
		foreach (int num in array)
		{
			if (umaSkeleton.Restore(num) || !logErrors)
			{
				continue;
			}
			string text = "";
			PosePair[] array2 = posePairs;
			foreach (PosePair posePair in array2)
			{
				UMABonePose.PoseBone[] poses;
				if (posePair.primary != null)
				{
					poses = posePair.primary.poses;
					foreach (UMABonePose.PoseBone poseBone in poses)
					{
						if (poseBone.hash == num)
						{
							text = poseBone.bone;
						}
					}
				}
				if (!(posePair.inverse != null))
				{
					continue;
				}
				poses = posePair.inverse.poses;
				foreach (UMABonePose.PoseBone poseBone2 in poses)
				{
					if (poseBone2.hash == num)
					{
						text = poseBone2.bone;
					}
				}
			}
			Debug.LogWarning("Couldn't reset bone! " + text);
		}
	}

	public int[] GetAnimatedBoneHashes()
	{
		ValidateBoneHashes();
		return boneHashes;
	}

	public Transform[] GetAnimatedBones(UMASkeleton umaSkeleton)
	{
		if (umaSkeleton == null)
		{
			return null;
		}
		ValidateBoneHashes();
		Transform[] array = new Transform[boneHashes.Length];
		for (int i = 0; i < boneHashes.Length; i++)
		{
			array[i] = umaSkeleton.GetBoneGameObject(boneHashes[i]).transform;
		}
		return array;
	}
}
