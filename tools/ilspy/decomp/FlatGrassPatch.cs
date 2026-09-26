using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FlatGrassPatch
{
	public int Density;

	public Resource<Mesh> Mesh;

	public Resource<Material> Material;

	public List<Matrix4x4> BatchMatrices = new List<Matrix4x4>();

	public List<Vector4> BatchVectors = new List<Vector4>();

	public int DrawCount;

	public const int MAX_INSTANCING_AMOUNT = 1023;

	public static bool EnableInstancing = true;

	public static bool EnableShadows = true;

	public static string DrawBatchStr = "DrawBatch";

	public FlatGrassPatch(int density, Resource<Mesh> mesh, Resource<Material> material)
	{
		Density = density;
		Mesh = mesh;
		Material = material;
		BatchMatrices.Capacity = 100;
	}

	public void AddToBatch(Vector4 pos)
	{
		if (BatchMatrices.Count < 1023)
		{
			BatchMatrices.Add(Matrix4x4.identity);
			BatchVectors.Add(pos);
		}
	}

	public void DrawBatch(MaterialPropertyBlock properties, int layer, Camera cam)
	{
		if (BatchMatrices.Count <= 0)
		{
			return;
		}
		using (new UnityProfileMarker(DrawBatchStr))
		{
			DrawCount += BatchMatrices.Count;
			ShadowCastingMode castShadows = (EnableShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off);
			if (EnableInstancing)
			{
				properties.SetVectorArray(ShaderHash._WorldPosition, BatchVectors);
				Graphics.DrawMeshInstanced(Mesh, 0, Material, BatchMatrices, properties, castShadows, receiveShadows: true, layer, cam);
			}
			else
			{
				for (int i = 0; i < BatchMatrices.Count; i++)
				{
					properties.SetVector(ShaderHash._WorldPosition, BatchVectors[i]);
					Graphics.DrawMesh(Mesh, BatchMatrices[i], Material, layer, cam, 0, properties, castShadows, receiveShadows: true);
				}
			}
			BatchMatrices.Clear();
			BatchVectors.Clear();
		}
	}
}
