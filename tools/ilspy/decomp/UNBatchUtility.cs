using System.Collections.Generic;
using UnityEngine;

public static class UNBatchUtility
{
	public static void CombineMeshes(List<UNCombineInstance> instances, Mesh combinedMesh, Mesh mesh)
	{
		if (instances.Count == 0)
		{
			combinedMesh.Clear();
			return;
		}
		int count = instances.Count;
		Vector3[] vertices = new Vector3[count * mesh.vertices.Length];
		Color32[] array = new Color32[count * mesh.colors32.Length];
		Vector3[] normals = new Vector3[count * mesh.normals.Length];
		Vector4[] tangents = new Vector4[count * mesh.tangents.Length];
		Vector2[] array2 = new Vector2[count * mesh.uv.Length];
		Vector2[] array3 = new Vector2[count * mesh.vertices.Length];
		Vector2[] array4 = new Vector2[count * mesh.vertices.Length];
		Vector2[] array5 = new Vector2[count * mesh.vertices.Length];
		int[] array6 = new int[count * mesh.triangles.Length];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		for (int i = 0; i < instances.Count; i++)
		{
			MergeMesh(instances[i], i, vertices, array, normals, tangents, array2, array3, array4, array5, array6, mesh, num, num2, num3, num4, num5, num6, num7);
			num += mesh.vertices.Length;
			num2 += mesh.colors.Length;
			num3 += mesh.normals.Length;
			num4 += mesh.tangents.Length;
			num5 += mesh.uv.Length;
			num6 += mesh.vertices.Length;
			num7 += mesh.triangles.Length;
		}
		combinedMesh.Clear();
		combinedMesh.vertices = vertices;
		combinedMesh.colors32 = array;
		combinedMesh.normals = normals;
		combinedMesh.tangents = tangents;
		combinedMesh.uv = array2;
		combinedMesh.uv2 = array3;
		combinedMesh.uv3 = array4;
		combinedMesh.uv4 = array5;
		combinedMesh.SetTriangles(array6, 0);
		combinedMesh.RecalculateBounds();
		combinedMesh.bounds = new Bounds(combinedMesh.bounds.center, new Vector3(10000f, 10000f, 10000f));
	}

	private static void MergeMesh(UNCombineInstance batchInstance, int id, Vector3[] vertices, Color32[] colors, Vector3[] normals, Vector4[] tangents, Vector2[] uv1s, Vector2[] uv2s, Vector2[] uv3s, Vector2[] uv4s, int[] subMeshes, Mesh mesh, int verticesOffset, int colorsOffset, int normalsOffset, int tangentsOffset, int uv1sOffset, int uv2sOffset, int subMeshesOffset)
	{
		Vector3 vector = batchInstance.transform.MultiplyPoint3x4(Vector3.zero);
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			vertices[verticesOffset + i] = vector;
		}
		for (int j = 0; j < mesh.colors32.Length; j++)
		{
			colors[colorsOffset + j] = mesh.colors32[j];
		}
		for (int k = 0; k < mesh.normals.Length; k++)
		{
			normals[normalsOffset + k] = mesh.normals[k];
		}
		for (int l = 0; l < mesh.tangents.Length; l++)
		{
			tangents[tangentsOffset + l] = mesh.tangents[l];
		}
		for (int m = 0; m < mesh.uv.Length; m++)
		{
			uv1s[uv1sOffset + m] = mesh.uv[m];
		}
		for (int n = 0; n < mesh.vertices.Length; n++)
		{
			uv2s[uv2sOffset + n] = new Vector2(mesh.vertices[n].x, mesh.vertices[n].y);
			uv3s[uv2sOffset + n] = batchInstance.densityOffset;
			uv4s[uv2sOffset + n] = new Vector2(mesh.vertices[n].z, batchInstance.density);
		}
		for (int num = 0; num < mesh.triangles.Length; num++)
		{
			subMeshes[subMeshesOffset + num] = mesh.triangles[num] + verticesOffset;
		}
	}
}
