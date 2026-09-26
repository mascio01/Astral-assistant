using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace UMA;

[Serializable]
public class UmaTPose : ScriptableObject
{
	[NonSerialized]
	public SkeletonBone[] boneInfo;

	[NonSerialized]
	public HumanBone[] humanInfo;

	[NonSerialized]
	public float armStretch;

	[NonSerialized]
	public float feetSpacing;

	[NonSerialized]
	public float legStretch;

	[NonSerialized]
	public float lowerArmTwist;

	[NonSerialized]
	public float lowerLegTwist;

	[NonSerialized]
	public float upperArmTwist;

	[NonSerialized]
	public float upperLegTwist;

	[NonSerialized]
	public bool extendedInfo;

	public byte[] serializedChunk;

	public void Serialize()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(boneInfo.Length);
		SkeletonBone[] array = boneInfo;
		foreach (SkeletonBone bone in array)
		{
			Serialize(binaryWriter, bone);
		}
		binaryWriter.Write(humanInfo.Length);
		HumanBone[] array2 = humanInfo;
		foreach (HumanBone value in array2)
		{
			Serialize(binaryWriter, value);
		}
		if (extendedInfo)
		{
			binaryWriter.Write(armStretch);
			binaryWriter.Write(feetSpacing);
			binaryWriter.Write(legStretch);
			binaryWriter.Write(lowerArmTwist);
			binaryWriter.Write(lowerLegTwist);
			binaryWriter.Write(upperArmTwist);
			binaryWriter.Write(upperLegTwist);
		}
		serializedChunk = memoryStream.ToArray();
	}

	public void DeSerialize()
	{
		if (boneInfo == null)
		{
			BinaryReader binaryReader = new BinaryReader(new MemoryStream(serializedChunk));
			int num = binaryReader.ReadInt32();
			boneInfo = new SkeletonBone[num];
			for (int i = 0; i < num; i++)
			{
				boneInfo[i] = DeSerializeSkeletonBone(binaryReader);
			}
			num = binaryReader.ReadInt32();
			humanInfo = new HumanBone[num];
			for (int j = 0; j < num; j++)
			{
				humanInfo[j] = DeSerializeHumanBone(binaryReader);
			}
			if (binaryReader.PeekChar() >= 0)
			{
				extendedInfo = true;
				armStretch = binaryReader.ReadSingle();
				feetSpacing = binaryReader.ReadSingle();
				legStretch = binaryReader.ReadSingle();
				lowerArmTwist = binaryReader.ReadSingle();
				lowerLegTwist = binaryReader.ReadSingle();
				upperArmTwist = binaryReader.ReadSingle();
				upperLegTwist = binaryReader.ReadSingle();
			}
		}
	}

	private SkeletonBone DeSerializeSkeletonBone(BinaryReader br)
	{
		SkeletonBone result = new SkeletonBone
		{
			name = br.ReadString(),
			position = DeserializeVector3(br),
			rotation = DeSerializeQuaternion(br),
			scale = DeserializeVector3(br)
		};
		br.ReadInt32();
		return result;
	}

	private Quaternion DeSerializeQuaternion(BinaryReader br)
	{
		return new Quaternion
		{
			x = br.ReadSingle(),
			y = br.ReadSingle(),
			z = br.ReadSingle(),
			w = br.ReadSingle()
		};
	}

	private HumanBone DeSerializeHumanBone(BinaryReader br)
	{
		return new HumanBone
		{
			boneName = br.ReadString(),
			humanName = br.ReadString(),
			limit = DeSerializeHumanLimit(br)
		};
	}

	private HumanLimit DeSerializeHumanLimit(BinaryReader br)
	{
		return new HumanLimit
		{
			axisLength = br.ReadSingle(),
			center = DeserializeVector3(br),
			max = DeserializeVector3(br),
			min = DeserializeVector3(br),
			useDefaultValues = br.ReadBoolean()
		};
	}

	private Vector3 DeserializeVector3(BinaryReader br)
	{
		return new Vector3
		{
			x = br.ReadSingle(),
			y = br.ReadSingle(),
			z = br.ReadSingle()
		};
	}

	private void Serialize(BinaryWriter bn, HumanBone value)
	{
		bn.Write(value.boneName);
		bn.Write(value.humanName);
		Serialize(bn, value.limit);
	}

	private void Serialize(BinaryWriter bn, HumanLimit value)
	{
		bn.Write(value.axisLength);
		Serialize(bn, value.center);
		Serialize(bn, value.max);
		Serialize(bn, value.min);
		bn.Write(value.useDefaultValues);
	}

	private void Serialize(BinaryWriter bn, SkeletonBone bone)
	{
		bn.Write(bone.name);
		Serialize(bn, bone.position);
		Serialize(bn, bone.rotation);
		Serialize(bn, bone.scale);
		bn.Write(1);
	}

	private void Serialize(BinaryWriter bn, Quaternion value)
	{
		bn.Write(value.x);
		bn.Write(value.y);
		bn.Write(value.z);
		bn.Write(value.w);
	}

	private void Serialize(BinaryWriter bn, Vector3 value)
	{
		bn.Write(value.x);
		bn.Write(value.y);
		bn.Write(value.z);
	}

	public void ReadFromHumanDescription(HumanDescription description)
	{
		humanInfo = description.human;
		boneInfo = description.skeleton;
		armStretch = description.armStretch;
		feetSpacing = description.feetSpacing;
		legStretch = description.legStretch;
		lowerArmTwist = description.lowerArmTwist;
		lowerLegTwist = description.lowerLegTwist;
		upperArmTwist = description.upperArmTwist;
		upperLegTwist = description.upperLegTwist;
		extendedInfo = true;
		Serialize();
		boneInfo = null;
		humanInfo = null;
	}

	public void ReadFromTransform(Animator rootAnimator)
	{
		List<SkeletonBone> list = new List<SkeletonBone>();
		AddRecursively(list, rootAnimator.transform);
		boneInfo = list.ToArray();
		List<HumanBone> list2 = new List<HumanBone>();
		ExtractHumanInfo(rootAnimator, list2);
		humanInfo = list2.ToArray();
		Serialize();
	}

	private void ExtractHumanInfo(Animator animator, List<HumanBone> humanInfoList)
	{
		for (int i = 0; i < HumanTrait.BoneCount; i++)
		{
			Transform boneTransform = animator.GetBoneTransform((HumanBodyBones)i);
			if (boneTransform != null)
			{
				humanInfoList.Add(new HumanBone
				{
					boneName = boneTransform.name,
					humanName = HumanTrait.BoneName[i],
					limit = new HumanLimit
					{
						useDefaultValues = true
					}
				});
			}
		}
	}

	private void AddRecursively(List<SkeletonBone> boneInfoList, Transform root)
	{
		boneInfoList.Add(new SkeletonBone
		{
			name = root.name,
			position = root.localPosition,
			rotation = root.localRotation,
			scale = root.localScale
		});
		for (int i = 0; i < root.childCount; i++)
		{
			AddRecursively(boneInfoList, root.GetChild(i));
		}
	}

	public void WriteToTransform(Animator rootAnimator)
	{
		DeSerialize();
		WriteRecursively(rootAnimator.transform);
	}

	private void WriteRecursively(Transform transform)
	{
		SkeletonBone[] array = boneInfo;
		for (int i = 0; i < array.Length; i++)
		{
			SkeletonBone skeletonBone = array[i];
			if (skeletonBone.name == transform.name)
			{
				transform.localPosition = skeletonBone.position;
				transform.localRotation = skeletonBone.rotation;
				transform.localScale = skeletonBone.scale;
				break;
			}
		}
		for (int j = 0; j < transform.childCount; j++)
		{
			WriteRecursively(transform.GetChild(j));
		}
	}

	public void Print()
	{
		DeSerialize();
		StringBuilder stringBuilder = new StringBuilder();
		HumanBone[] array = humanInfo;
		for (int i = 0; i < array.Length; i++)
		{
			HumanBone humanBone = array[i];
			stringBuilder.Append(humanBone.boneName + ": " + humanBone.humanName + "\n");
		}
		stringBuilder.Append("==========================\n");
		stringBuilder.Append("==========================\n");
		stringBuilder.Append("armStretch: " + armStretch + "\n");
		stringBuilder.Append("feetSpacing: " + feetSpacing + "\n");
		stringBuilder.Append("legStretch: " + legStretch + "\n");
		stringBuilder.Append("lowerArmTwist: " + lowerArmTwist + "\n");
		stringBuilder.Append("lowerLegTwist: " + lowerLegTwist + "\n");
		stringBuilder.Append("upperArmTwist: " + upperArmTwist + "\n");
		stringBuilder.Append("upperLegTwist: " + upperLegTwist + "\n");
		stringBuilder.Append("==========================\n");
		SkeletonBone[] array2 = boneInfo;
		for (int i = 0; i < array2.Length; i++)
		{
			SkeletonBone skeletonBone = array2[i];
			stringBuilder.Append("==========================\n");
			stringBuilder.Append(skeletonBone.name + "\n");
			Vector3 position = skeletonBone.position;
			stringBuilder.Append("pos: " + position.ToString() + "\n");
			Quaternion rotation = skeletonBone.rotation;
			stringBuilder.Append("rot: " + rotation.ToString() + "\n");
			position = skeletonBone.scale;
			stringBuilder.Append("scale: " + position.ToString() + "\n");
		}
		stringBuilder.Append("==========================\n");
		stringBuilder.Append("==========================\n");
		Debug.Log(stringBuilder.ToString());
	}
}
