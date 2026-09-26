using System;
using System.Collections.Generic;
using UnityEngine;

namespace UMA;

[Serializable]
public class UMATransform
{
	public class UMATransformComparer : IComparer<UMATransform>
	{
		public int Compare(UMATransform x, UMATransform y)
		{
			if (x.hash >= y.hash)
			{
				if (x.hash <= y.hash)
				{
					return 0;
				}
				return 1;
			}
			return -1;
		}
	}

	public Vector3 position;

	public Quaternion rotation;

	public Vector3 scale;

	public string name;

	public int hash;

	public int parent;

	public static UMATransformComparer TransformComparer = new UMATransformComparer();

	public UMATransform()
	{
	}

	public UMATransform(Transform transform, int nameHash, int parentHash)
	{
		hash = nameHash;
		parent = parentHash;
		position = transform.localPosition;
		rotation = transform.localRotation;
		scale = transform.localScale;
		name = transform.name;
	}

	public UMATransform Duplicate()
	{
		return new UMATransform
		{
			hash = hash,
			name = name,
			parent = parent,
			position = position,
			rotation = rotation,
			scale = scale
		};
	}

	public void Assign(UMATransform other)
	{
		hash = other.hash;
		name = other.name;
		parent = other.parent;
		position = other.position;
		rotation = other.rotation;
		scale = other.scale;
	}
}
