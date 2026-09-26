using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityObjectPool
{
	public GameObject Owner;

	public List<GameObject> FreeGameObjects = new List<GameObject>();

	public string Name;

	public int PrepopulateCount;

	public static List<UnityObjectPool> AllPools = new List<UnityObjectPool>();

	public UnityObjectPool(PrefabResource resource, int prepopulateCount)
	{
		PrepopulateCount = prepopulateCount;
		Name = resource.GetPath();
		int num = Math.Max(Name.LastIndexOf('\\'), Name.LastIndexOf('/'));
		if (num != -1)
		{
			Name = Name.Substring(num);
		}
		Owner = new GameObject(Name + " Pool");
		Owner.SetActive(value: false);
		AllPools.Add(this);
	}

	public void AddToPool(PrefabResource resource)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(resource.GetAsset(), Owner.transform, worldPositionStays: true);
		gameObject.SetActive(value: false);
		gameObject.name = Name;
		FreeGameObjects.Add(gameObject);
	}

	public GameObject InstantiateFromPool(PrefabResource resource, Transform parent)
	{
		if (FreeGameObjects.Count == 0)
		{
			AddToPool(resource);
		}
		GameObject gameObject = FreeGameObjects[FreeGameObjects.Count - 1];
		FreeGameObjects.RemoveAt(FreeGameObjects.Count - 1);
		gameObject.transform.parent = parent;
		return gameObject;
	}

	public void ReturnToPool(GameObject obj)
	{
		obj.SetActive(value: false);
		FreeGameObjects.Add(obj);
		obj.transform.parent = Owner.transform;
	}

	public void RevertToPrepopulatedAmount()
	{
		while (FreeGameObjects.Count > PrepopulateCount)
		{
			int index = FreeGameObjects.Count - 1;
			UnityEngine.Object.Destroy(FreeGameObjects[index]);
			FreeGameObjects.RemoveAt(index);
		}
	}

	public static void RevertAllPoolsToPrepopulatedAmount()
	{
		foreach (UnityObjectPool allPool in AllPools)
		{
			allPool.RevertToPrepopulatedAmount();
		}
	}
}
