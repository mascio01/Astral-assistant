using System.Collections.Generic;
using UnityEngine;

public class ImpactedBehaviour : MonoBehaviour
{
	private TileObject AuthoritativeSource;

	private PrefabResource PrefabToReturn;

	private float Timer;

	private float OldMossAmount;

	private float OldMossOnSidesAmount;

	private static float AccelerationLimit = 1000f;

	private List<BoxCollider> BoxColliders;

	private List<Vector3> OldCentres;

	private List<Vector3> OldSizes;

	public void Init(TileObject authoritativeSource, PrefabResource prefab, Vector3 impactForce, Vector3 contactPoint, bool isFence = false)
	{
		PropPrototype propPrototype = authoritativeSource.GetPropPrototype();
		Prop.UnitySetMoss(base.gameObject, 0f, 0f, ref OldMossAmount, ref OldMossOnSidesAmount);
		base.gameObject.layer = Character.GibLayer;
		AuthoritativeSource = authoritativeSource;
		PrefabToReturn = prefab;
		Timer = 0f;
		float magnitude = impactForce.magnitude;
		if (magnitude > 0f && propPrototype.Mass > 0f)
		{
			float b = magnitude / propPrototype.Mass;
			b = Mathf.Min(AccelerationLimit, b);
			impactForce *= propPrototype.Mass * b / magnitude;
		}
		if (isFence)
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(base.gameObject.transform.position);
			float tileMaxHeight = GameTerrain.Instance.GetTileMaxHeight(tileCoordForPos.x, tileCoordForPos.y);
			BoxColliders = new List<BoxCollider>();
			OldCentres = new List<Vector3>();
			OldSizes = new List<Vector3>();
			base.gameObject.GetComponentsInChildren(BoxColliders);
			foreach (BoxCollider boxCollider in BoxColliders)
			{
				OldCentres.Add(boxCollider.center);
				OldSizes.Add(boxCollider.size);
				float num = tileMaxHeight - boxCollider.bounds.min.y;
				if (num > 0f)
				{
					boxCollider.center += new Vector3(0f, 0f, num * 0.5f);
					boxCollider.size += new Vector3(0f, 0f, 0f - num);
				}
			}
		}
		Rigidbody rigidbody = base.gameObject.GetComponentInChildren<Rigidbody>();
		if (rigidbody == null)
		{
			rigidbody = base.gameObject.AddComponent<Rigidbody>();
		}
		rigidbody.isKinematic = false;
		rigidbody.mass = propPrototype.Mass;
		rigidbody.AddForceAtPosition(impactForce, contactPoint);
	}

	private void Update()
	{
		Timer += Time.deltaTime;
		if (!(Timer >= 60f))
		{
			return;
		}
		if (PrefabToReturn != null && PrefabToReturn.Pool != null)
		{
			Rigidbody componentInChildren = base.gameObject.GetComponentInChildren<Rigidbody>();
			if (componentInChildren != null)
			{
				componentInChildren.isKinematic = true;
			}
			Object.Destroy(this);
			float oldMossAmount = 0f;
			float oldMossOnSidesAmount = 0f;
			Prop.UnitySetMoss(base.gameObject, OldMossAmount, OldMossOnSidesAmount, ref oldMossAmount, ref oldMossOnSidesAmount);
			base.gameObject.layer = Character.DefaultLayer;
			if (BoxColliders != null)
			{
				for (int i = 0; i < BoxColliders.Count; i++)
				{
					BoxColliders[i].center = OldCentres[i];
					BoxColliders[i].size = OldSizes[i];
				}
			}
			PrefabToReturn.DeletePrefab(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		if (AuthoritativeSource != null && !AuthoritativeSource.IsDestroyed() && AuthoritativeSource.GetUnityObject() == null)
		{
			AuthoritativeSource.UnityInit();
		}
	}
}
