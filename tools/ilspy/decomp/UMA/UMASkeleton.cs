using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMA;

[Serializable]
public class UMASkeleton
{
	[Serializable]
	public class BoneData
	{
		public int boneNameHash;

		public int parentBoneNameHash;

		public Transform boneTransform;

		public UMATransform umaTransform;

		public Quaternion rotation;

		public Vector3 position;

		public Vector3 scale;

		public int accessedFrame;
	}

	protected bool updating;

	protected int frame;

	private Dictionary<int, BoneData> boneHashDataLookup;

	public IEnumerable<int> BoneHashes => GetBoneHashes();

	public string[] BoneNames => GetBoneNames();

	public int rootBoneHash { get; protected set; }

	public virtual int boneCount => boneHashData.Count;

	public bool isUpdating => updating;

	private Dictionary<int, BoneData> boneHashData
	{
		get
		{
			if (boneHashDataLookup == null)
			{
				boneHashDataLookup = new Dictionary<int, BoneData>();
			}
			return boneHashDataLookup;
		}
		set
		{
			boneHashDataLookup = value;
		}
	}

	public UMASkeleton(Transform rootBone)
	{
		rootBoneHash = Animator.StringToHash(rootBone.name);
		boneHashData = new Dictionary<int, BoneData>();
		BeginSkeletonUpdate();
		AddBonesRecursive(rootBone);
		EndSkeletonUpdate();
	}

	protected UMASkeleton()
	{
	}

	public virtual void BeginSkeletonUpdate()
	{
		frame++;
		if (frame < 0)
		{
			frame = 0;
		}
		updating = true;
	}

	public virtual void EndSkeletonUpdate()
	{
		foreach (BoneData value in boneHashData.Values)
		{
			value.rotation = value.boneTransform.localRotation;
			value.position = value.boneTransform.localPosition;
			value.scale = value.boneTransform.localScale;
		}
		updating = false;
	}

	public virtual void SetAnimatedBone(int nameHash)
	{
	}

	public virtual void SetAnimatedBoneHierachy(int nameHash)
	{
	}

	public virtual void ClearAnimatedBoneHierachy(int nameHash, bool recursive)
	{
	}

	private void AddBonesRecursive(Transform transform)
	{
		int num = Animator.StringToHash(transform.name);
		int num2 = ((transform.parent != null) ? Animator.StringToHash(transform.parent.name) : 0);
		BoneData value = new BoneData
		{
			parentBoneNameHash = num2,
			boneNameHash = num,
			accessedFrame = frame,
			boneTransform = transform,
			umaTransform = new UMATransform(transform, num, num2)
		};
		if (!boneHashData.ContainsKey(num))
		{
			boneHashData.Add(num, value);
		}
		else
		{
			Debug.LogError("AddBonesRecursive: " + transform.name + " already exists in the dictionary!");
		}
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			AddBonesRecursive(child);
		}
	}

	protected virtual BoneData GetBone(int nameHash)
	{
		BoneData value = null;
		boneHashData.TryGetValue(nameHash, out value);
		return value;
	}

	public virtual bool HasBone(int nameHash)
	{
		return boneHashData.ContainsKey(nameHash);
	}

	public virtual bool TryGetBoneTransform(int nameHash, out Transform boneTransform, out bool transformDirty, out int parentBoneNameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			transformDirty = value.accessedFrame != frame;
			value.accessedFrame = frame;
			boneTransform = value.boneTransform;
			parentBoneNameHash = value.parentBoneNameHash;
			return true;
		}
		transformDirty = false;
		boneTransform = null;
		parentBoneNameHash = 0;
		return false;
	}

	public virtual Transform GetBoneTransform(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			return value.boneTransform;
		}
		return null;
	}

	public virtual GameObject GetBoneGameObject(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			return value.boneTransform.gameObject;
		}
		return null;
	}

	protected virtual IEnumerable<int> GetBoneHashes()
	{
		foreach (int key in boneHashData.Keys)
		{
			yield return key;
		}
	}

	private string[] GetBoneNames()
	{
		string[] array = new string[boneHashData.Count];
		int num = 0;
		foreach (KeyValuePair<int, BoneData> boneHashDatum in boneHashData)
		{
			array[num] = boneHashDatum.Value.boneTransform.gameObject.name;
			num++;
		}
		return array;
	}

	public virtual void Set(int nameHash, Vector3 position, Vector3 scale, Quaternion rotation)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = position;
			value.boneTransform.localRotation = rotation;
			value.boneTransform.localScale = scale;
			return;
		}
		throw new Exception("Bone not found.");
	}

	public virtual void SetPosition(int nameHash, Vector3 position)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = position;
		}
	}

	public virtual void SetPositionRelative(int nameHash, Vector3 delta)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = value.boneTransform.localPosition + delta;
		}
	}

	public virtual void SetScale(int nameHash, Vector3 scale)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localScale = scale;
		}
	}

	public virtual void SetScaleRelative(int nameHash, Vector3 scale)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			Vector3 localScale = scale;
			localScale.Scale(value.boneTransform.localScale);
			value.boneTransform.localScale = localScale;
		}
	}

	public virtual void SetRotation(int nameHash, Quaternion rotation)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localRotation = rotation;
		}
	}

	public virtual void SetRotationRelative(int nameHash, Quaternion rotation, float weight)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			Quaternion b = value.boneTransform.localRotation * rotation;
			value.boneTransform.localRotation = Quaternion.Slerp(value.boneTransform.localRotation, b, weight);
		}
	}

	public virtual void Lerp(int nameHash, Vector3 position, Vector3 scale, Quaternion rotation, float weight)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = Vector3.Lerp(value.boneTransform.localPosition, position, weight);
			value.boneTransform.localRotation = Quaternion.Slerp(value.boneTransform.localRotation, value.boneTransform.localRotation, weight);
			value.boneTransform.localScale = Vector3.Lerp(value.boneTransform.localScale, scale, weight);
		}
	}

	public virtual void Morph(int nameHash, Vector3 position, Vector3 scale, Quaternion rotation, float weight)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition += position * weight;
			Quaternion b = value.boneTransform.localRotation * rotation;
			value.boneTransform.localRotation = Quaternion.Slerp(value.boneTransform.localRotation, b, weight);
			Vector3 b2 = scale;
			b2.Scale(value.boneTransform.localScale);
			value.boneTransform.localScale = Vector3.Lerp(value.boneTransform.localScale, b2, weight);
		}
	}

	public virtual bool Reset(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value) && value.boneTransform != null)
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = value.umaTransform.position;
			value.boneTransform.localRotation = value.umaTransform.rotation;
			value.boneTransform.localScale = value.umaTransform.scale;
			return true;
		}
		return false;
	}

	public virtual void ResetAll()
	{
		foreach (BoneData value in boneHashData.Values)
		{
			if (value.boneTransform != null)
			{
				value.accessedFrame = frame;
				value.boneTransform.localPosition = value.umaTransform.position;
				value.boneTransform.localRotation = value.umaTransform.rotation;
				value.boneTransform.localScale = value.umaTransform.scale;
			}
		}
	}

	public virtual bool Restore(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value) && value.boneTransform != null)
		{
			value.accessedFrame = frame;
			value.boneTransform.localPosition = value.position;
			value.boneTransform.localRotation = value.rotation;
			value.boneTransform.localScale = value.scale;
			return true;
		}
		return false;
	}

	public virtual void RestoreAll()
	{
		foreach (BoneData value in boneHashData.Values)
		{
			if (value.boneTransform != null)
			{
				value.accessedFrame = frame;
				value.boneTransform.localPosition = value.position;
				value.boneTransform.localRotation = value.rotation;
				value.boneTransform.localScale = value.scale;
			}
		}
	}

	public virtual Vector3 GetPosition(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			return value.boneTransform.localPosition;
		}
		throw new Exception("Bone not found.");
	}

	public virtual Vector3 GetScale(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			return value.boneTransform.localScale;
		}
		throw new Exception("Bone not found.");
	}

	public virtual Quaternion GetRotation(int nameHash)
	{
		if (boneHashData.TryGetValue(nameHash, out var value))
		{
			value.accessedFrame = frame;
			return value.boneTransform.localRotation;
		}
		throw new Exception("Bone not found.");
	}

	public static int StringToHash(string name)
	{
		return Animator.StringToHash(name);
	}

	public virtual Transform[] HashesToTransforms(int[] boneNameHashes)
	{
		Transform[] array = new Transform[boneNameHashes.Length];
		for (int i = 0; i < boneNameHashes.Length; i++)
		{
			array[i] = boneHashData[boneNameHashes[i]].boneTransform;
		}
		return array;
	}

	public virtual Transform[] HashesToTransforms(List<int> boneNameHashes)
	{
		Transform[] array = new Transform[boneNameHashes.Count];
		for (int i = 0; i < boneNameHashes.Count; i++)
		{
			array[i] = boneHashData[boneNameHashes[i]].boneTransform;
		}
		return array;
	}

	public virtual void EnsureBoneHierarchy()
	{
		foreach (BoneData value in boneHashData.Values)
		{
			if (value.accessedFrame == -1)
			{
				if (boneHashData.ContainsKey(value.umaTransform.parent))
				{
					value.boneTransform.parent = boneHashData[value.umaTransform.parent].boneTransform;
					value.boneTransform.localPosition = value.umaTransform.position;
					value.boneTransform.localRotation = value.umaTransform.rotation;
					value.boneTransform.localScale = value.umaTransform.scale;
					value.accessedFrame = frame;
				}
				else
				{
					Debug.LogError("EnsureBoneHierarchy: " + value.umaTransform.name + " parent not found in dictionary!");
				}
			}
		}
	}

	public virtual Quaternion GetTPoseCorrectedRotation(int nameHash, Quaternion tPoseRotation)
	{
		return boneHashData[nameHash].boneTransform.localRotation;
	}
}
