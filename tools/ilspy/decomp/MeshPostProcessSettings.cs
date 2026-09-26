using System.Collections.Generic;
using UnityEngine;

public class MeshPostProcessSettings : MonoBehaviour
{
	public MeshExtraInfo ExtraInfo;

	[SerializeField]
	private List<MeshConstructionProgressData> UV2ForMesh;

	public void ResetUV2ForMesh()
	{
		UV2ForMesh = new List<MeshConstructionProgressData>();
		UV2ForMesh.Clear();
	}

	public int GetMeshDataIndex(string meshName)
	{
		for (int i = 0; i < UV2ForMesh.Count; i++)
		{
			if (UV2ForMesh[i].MeshName == meshName)
			{
				return i;
			}
		}
		return -1;
	}

	public Vector2[] GetUV2ForMesh(string meshName)
	{
		int meshDataIndex = GetMeshDataIndex(meshName);
		if (meshDataIndex != -1)
		{
			return UV2ForMesh[meshDataIndex].UV2;
		}
		return null;
	}

	public void SetUV2ForMesh(string meshName, Vector2[] data)
	{
		MeshConstructionProgressData meshConstructionProgressData = new MeshConstructionProgressData
		{
			MeshName = meshName,
			UV2 = data
		};
		int meshDataIndex = GetMeshDataIndex(meshName);
		if (meshDataIndex != -1)
		{
			UV2ForMesh[meshDataIndex] = meshConstructionProgressData;
		}
		else
		{
			UV2ForMesh.Add(meshConstructionProgressData);
		}
	}
}
