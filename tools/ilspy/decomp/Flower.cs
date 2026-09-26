using System.Collections.Generic;
using UnityEngine;

public class Flower : SingleTileProp
{
	private struct UnityFlowerObj
	{
		public PrefabResource Res;

		public GameObject UnityObj;
	}

	private const string UnityName = "Flower";

	private List<UnityFlowerObj> Flowers;

	public const int MaxFlowerCount = 8;

	public static bool DebugShowFlowers = true;

	public int FlowerCount => MathUtil.RandomInt(Id, 1, 8);

	public virtual PrefabResource[] GetUnityModels()
	{
		if (Prototype == null)
		{
			return null;
		}
		return Prototype.GetPrefabsAsArray();
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Flower;
	}

	public override void UnityActivate()
	{
		if (IsUnityObjectActive())
		{
			return;
		}
		int flowerCount = FlowerCount;
		Flowers = new List<UnityFlowerObj>(flowerCount);
		PrefabResource[] unityModels = GetUnityModels();
		if (unityModels.Length == 0)
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		for (int i = 0; i < flowerCount; i++)
		{
			int num = Id % 100 * 100 + i * 10;
			int num2 = MathUtil.RandomInt(num + 1, unityModels.Length);
			Vector2 v = (instance?.GetTileCentreXZ(Tile) ?? Vector2.zero) + MathUtil.RandomVec2(new Vector2(num + 2, num + 3)) - new Vector2(0.5f, 0.5f);
			PrefabResource prefabResource = unityModels[num2];
			GameObject gameObject = prefabResource.InstantiatePrefab(instance?.UnityTerrainObj.transform);
			Vector3 eulerAngles = prefabResource.GetAsset().transform.eulerAngles;
			gameObject.transform.position = instance?.ClampPosToSurface(MathUtil.ToX0Y(v)) ?? Vector3.zero;
			gameObject.transform.rotation = Quaternion.Euler(eulerAngles.x, MathUtil.RandomFloat(num + 4) * 360f, eulerAngles.z);
			float a = ((Prototype != null) ? Prototype.MinMaxScale[num2].x : 1f);
			float b = ((Prototype != null) ? Prototype.MinMaxScale[num2].y : 1f);
			gameObject.transform.localScale = Vector3.one * Mathf.Lerp(a, b, MathUtil.RandomFloat(num + 5));
			gameObject.SetActive(value: true);
			if (i == 0)
			{
				UnityObj = gameObject;
			}
			Flowers.Add(new UnityFlowerObj
			{
				UnityObj = gameObject,
				Res = prefabResource
			});
		}
	}

	public override void UnityDeactivate()
	{
		if ((bool)UnityBurningEffect)
		{
			Object.DestroyImmediate(UnityBurningEffect);
			UnityBurningEffect = null;
		}
		if (Flowers != null)
		{
			for (int num = Flowers.Count - 1; num >= 0; num--)
			{
				Flowers[num].UnityObj.SetActive(value: false);
				Flowers[num].Res.DeletePrefab(Flowers[num].UnityObj);
			}
			Flowers.Clear();
			Flowers = null;
			UnityObj = null;
		}
	}

	public override bool HasUnityObjectPool()
	{
		return true;
	}

	public override void OnTerrainHeightChanged()
	{
		if (Flowers != null)
		{
			for (int i = 0; i < Flowers.Count; i++)
			{
				Flowers[i].UnityObj.transform.position = GameTerrain.Instance.ClampPosToSurface(Flowers[i].UnityObj.transform.position);
			}
		}
	}

	public override Bounds GetBoundingBox()
	{
		return MathUtil.CreateBoundsCentreExtents(Pos + new Vector3(0f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.5f));
	}

	public override float GetPlantCoverForCamouflage()
	{
		return (float)FlowerCount / 8f * ((Prototype != null) ? Prototype.PlantCover : 0f);
	}

	public override bool WantUnityObjectVisible()
	{
		return DebugShowFlowers;
	}

	public static bool GetDebugShowFlowers()
	{
		return DebugShowFlowers;
	}

	public static void SetDebugShowFlowers(bool v)
	{
		DebugShowFlowers = v;
		Session.Instance.WantFullRefreshOfActiveUnityObjectsInFocusArea = true;
	}
}
