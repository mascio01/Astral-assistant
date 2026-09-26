using System.Collections.Generic;
using UnityEngine;

public class PrefabResource : Resource<GameObject>
{
	public UnityObjectPool Pool;

	public Bounds IdentityBounds;

	public Bounds Bounds;

	public Matrix4x4 LocalToWorldMatrix;

	public Vector3 LocalScale;

	public List<Window> Windows = new List<Window>();

	public List<Collidable> Collidables = new List<Collidable>();

	public List<Quaternion> InitialWheelRot;

	private const string WindowStr = "BoardedWindow";

	private const float WindowScaleFactor = 6.6666665f;

	public float Height => Bounds.max.y - Bounds.min.y;

	public PrefabResource(string path)
		: base(path, includeInList: true)
	{
	}

	public PrefabResource(string path, int createPoolWithCount)
		: base(path, includeInList: true)
	{
		Pool = new UnityObjectPool(this, createPoolWithCount);
	}

	public PrefabResource(string path, AssetBundle assetBundle)
		: base(path, assetBundle, includeInList: true)
	{
	}

	public PrefabResource(string path, AssetBundle assetBundle, int createPoolWithCount)
		: base(path, assetBundle, includeInList: true)
	{
		Pool = new UnityObjectPool(this, createPoolWithCount);
	}

	protected override void FinishLoading()
	{
		base.FinishLoading();
		GameObject asset = GetAsset();
		if (!(asset != null))
		{
			return;
		}
		LocalToWorldMatrix = asset.transform.localToWorldMatrix;
		LocalScale = asset.transform.localScale;
		IdentityBounds = Prop.CalcObjectBounds(asset, Matrix4x4.identity, physics: true, render: true);
		Bounds = Prop.CalcObjectBounds(asset, LocalToWorldMatrix, physics: true, render: true);
		CacheCollidables(asset, Matrix4x4.identity, 0);
		ExtractWindows(asset);
		MeshPostProcessSettings component = asset.GetComponent<MeshPostProcessSettings>();
		if (component != null && component.ExtraInfo == MeshExtraInfo.ConstructionProgressInfo)
		{
			MeshFilter[] componentsInChildren = asset.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Mesh sharedMesh = componentsInChildren[i].sharedMesh;
				if (sharedMesh == null)
				{
					continue;
				}
				Vector2[] uV2ForMesh = component.GetUV2ForMesh(sharedMesh.name);
				if (uV2ForMesh != null)
				{
					if (sharedMesh.vertexCount != uV2ForMesh.Length)
					{
						Debug.LogWarning("Incorrect number of uv2s for mesh " + sharedMesh?.ToString() + " (expected " + sharedMesh.vertexCount + " but got " + uV2ForMesh.Length + ")");
					}
					else
					{
						sharedMesh.uv2 = uV2ForMesh;
					}
				}
			}
		}
		if (FromAssetBundle != null)
		{
			ReplaceAssetBundleShaders(asset);
		}
		if (Pool != null)
		{
			for (int j = 0; j < Pool.PrepopulateCount; j++)
			{
				Pool.AddToPool(this);
			}
		}
	}

	public GameObject InstantiatePrefab(Transform parent)
	{
		if (Pool != null)
		{
			return Pool.InstantiateFromPool(this, parent);
		}
		return Object.Instantiate(GetAsset(), parent, worldPositionStays: true);
	}

	public void DeletePrefab(GameObject obj)
	{
		if (Pool != null)
		{
			Pool.ReturnToPool(obj);
		}
		else
		{
			Object.Destroy(obj);
		}
	}

	private void CacheCollidables(GameObject obj, Matrix4x4 rootTransform, int recursionDepth)
	{
		Matrix4x4 matrix4x = ((recursionDepth != 0) ? (rootTransform * Matrix4x4.TRS(obj.transform.localPosition, obj.transform.localRotation, obj.transform.localScale)) : rootTransform);
		MeshCollider component = obj.GetComponent<MeshCollider>();
		if (component != null)
		{
			Collidable collidable = new Collidable();
			collidable.Name = obj.name;
			collidable.CollidableType = CollidableType.Mesh;
			collidable.Triangles = component.sharedMesh.triangles;
			collidable.Vertices = component.sharedMesh.vertices;
			collidable.WorldFromLocalTrans = matrix4x;
			Collidables.Add(collidable);
		}
		BoxCollider[] components = obj.GetComponents<BoxCollider>();
		if (components != null)
		{
			BoxCollider[] array = components;
			foreach (BoxCollider boxCollider in array)
			{
				Collidable collidable2 = new Collidable();
				collidable2.Name = obj.name;
				collidable2.CollidableType = CollidableType.Box;
				collidable2.Box = new Bounds(boxCollider.center, boxCollider.size);
				collidable2.WorldFromLocalTrans = matrix4x;
				Collidables.Add(collidable2);
			}
		}
		SphereCollider[] components2 = obj.GetComponents<SphereCollider>();
		if (components != null)
		{
			SphereCollider[] array2 = components2;
			foreach (SphereCollider sphereCollider in array2)
			{
				Collidable collidable3 = new Collidable();
				collidable3.Name = obj.name;
				collidable3.CollidableType = CollidableType.Sphere;
				collidable3.Sphere = new BoundingSphere(sphereCollider.center, sphereCollider.radius);
				collidable3.WorldFromLocalTrans = matrix4x;
				Collidables.Add(collidable3);
			}
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			GameObject gameObject = obj.transform.GetChild(j).gameObject;
			CacheCollidables(gameObject, matrix4x, recursionDepth + 1);
		}
	}

	private void ExtractWindows(GameObject obj)
	{
		Windows.Clear();
		if (!(obj != null))
		{
			return;
		}
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			if (gameObject.name.StartsWith("BoardedWindow"))
			{
				Windows.Add(new Window(gameObject.transform.localPosition, gameObject.transform.localRotation, gameObject.transform.localScale.x * 6.6666665f));
				gameObject.SetActive(value: false);
			}
		}
	}

	public bool IsSkinned()
	{
		GameObject asset = GetAsset();
		if (asset != null)
		{
			for (int i = 0; i < asset.transform.childCount; i++)
			{
				if (asset.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void CacheWheelRotations()
	{
		if (InitialWheelRot != null || !(GetAsset() != null))
		{
			return;
		}
		InitialWheelRot = new List<Quaternion>();
		GameObject asset = GetAsset();
		int num = 0;
		for (int i = -1; i <= 1; i += 2)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				string str = "Wheel_" + ((i == -1) ? "F" : "R") + "_" + ((j == -1) ? "R" : "L");
				GameObject gameObject = asset.FindChildWithNameContaining(str);
				InitialWheelRot.Add((gameObject != null) ? gameObject.transform.rotation : Quaternion.identity);
				num++;
			}
		}
	}

	private void ReplaceAssetBundleShaders(GameObject obj)
	{
		MeshRenderer component = obj.GetComponent<MeshRenderer>();
		if (component != null)
		{
			for (int i = 0; i < component.sharedMaterials.Length; i++)
			{
				Material material = component.sharedMaterials[i];
				if (material != null && material.shader != null)
				{
					Shader shader = Shader.Find(material.shader.name);
					if (shader != null && material.shader != shader)
					{
						material.shader = shader;
					}
				}
			}
		}
		for (int j = 0; j < obj.transform.childCount; j++)
		{
			ReplaceAssetBundleShaders(obj.transform.GetChild(j).gameObject);
		}
	}
}
