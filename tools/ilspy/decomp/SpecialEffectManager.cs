using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialEffectManager
{
	public static SpecialEffectManager Instance;

	public PrefabResource MuzzleFlash;

	public PrefabResource BulletTracer;

	public PrefabResource ArrowTracer;

	public PrefabResource BulletSmoke;

	public PrefabResource BloodSplatter;

	public PrefabResource SlidingDust;

	public PrefabResource SlidingDebris;

	public PrefabResource BurningPool;

	public PrefabResource BurningEffect;

	public PrefabResource DemolitionSmokeEffect;

	public PrefabResource DemolitionSnowEffect;

	public PrefabResource ExplosionEffect;

	public PrefabResource SnowballHit;

	public PrefabResource[] EffectPrefabs;

	public Resource<GameObject>[] VolumetricBloodSplatter;

	public Resource<GameObject>[] SmallVolumetricBloodSplatter;

	private static List<MeshRenderer> MeshRenderers = new List<MeshRenderer>();

	private static float BurningParticleSpawnRatePerCubicMetre = 10f;

	private static float SmokeParticleSpawnRatePerCubicMetre = 20f;

	public void Init()
	{
		Instance = this;
	}

	public void LoadContent()
	{
		EffectPrefabs = new PrefabResource[13]
		{
			MuzzleFlash = new PrefabResource("Prefabs/SpecialEffects/MuzzleFlashEffect"),
			BulletTracer = new PrefabResource("Prefabs/SpecialEffects/BulletTracer"),
			ArrowTracer = new PrefabResource("Prefabs/SpecialEffects/ArrowTracer"),
			BulletSmoke = new PrefabResource("Prefabs/SpecialEffects/BulletSmoke"),
			BloodSplatter = new PrefabResource("Prefabs/SpecialEffects/BloodSplatter"),
			SlidingDust = new PrefabResource("Prefabs/SpecialEffects/SlidingDust"),
			SlidingDebris = new PrefabResource("Prefabs/SpecialEffects/SlidingDebris"),
			BurningPool = new PrefabResource("Prefabs/SpecialEffects/BurningPool"),
			BurningEffect = new PrefabResource("Prefabs/SpecialEffects/BurningEffect"),
			DemolitionSmokeEffect = new PrefabResource("Prefabs/SpecialEffects/DemolitionSmokeEffect"),
			DemolitionSnowEffect = new PrefabResource("Prefabs/SpecialEffects/DemolitionSnowEffect"),
			ExplosionEffect = new PrefabResource("Prefabs/SpecialEffects/BigExplosionEffect"),
			SnowballHit = new PrefabResource("Prefabs/SpecialEffects/SnowballHit")
		};
		VolumetricBloodSplatter = new Resource<GameObject>[14]
		{
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood1"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood2"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood2_Left"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood2_Right"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood3"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood7"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood8"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood9"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood10"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood11"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood12"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood13"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood14"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood15")
		};
		SmallVolumetricBloodSplatter = new Resource<GameObject>[3]
		{
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood4"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood5"),
			new Resource<GameObject>("Prefabs/SpecialEffects/VolumetricBloodFX/Blood6")
		};
	}

	public void Unload()
	{
		Instance = null;
	}

	public void SpawnMuzzleFlash(Character owner, Vector3 pos, Vector3 dir, GameObject bone)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(MuzzleFlash.GetAsset(), pos, Quaternion.LookRotation(dir) * Quaternion.Euler(0f, -90f, 0f), (bone != null) ? bone.transform : GameTerrain.Instance.UnityTerrainObj.transform);
		ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
		component.Play();
		gameObject.GetComponent<ParticleSystemAutoDestroyBehaviour>().OwnerCharacter = owner;
		owner.Unity.MuzzleFlash = component;
	}

	public void SpawnBulletTracer(Vector3 startPos, Vector3 hitPos, Vector3 hitNormal, BulletHitEffect hitEffect, GameObject hitBone)
	{
		BulletTracerBehaviour component = UnityEngine.Object.Instantiate(BulletTracer.GetAsset(), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<BulletTracerBehaviour>();
		component.StartPos = startPos;
		component.HitPos = hitPos;
		component.HitNormal = hitNormal;
		component.HitEffect = hitEffect;
		component.HitBone = hitBone;
	}

	public void SpawnArrowTracer(Character firedBy, TimeSpan firedTime, Vector3 startPos, Vector3 hitPos, Vector3 hitNormal, BulletHitEffect hitEffect, GameObject hitBone, ArrowProp arrow)
	{
		ArrowTracerBehaviour component = UnityEngine.Object.Instantiate(ArrowTracer.GetAsset(), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ArrowTracerBehaviour>();
		component.FiredBy = firedBy;
		component.FiredTime = firedTime;
		component.StartPos = startPos;
		component.HitPos = hitPos;
		component.HitNormal = hitNormal;
		component.HitEffect = hitEffect;
		component.HitBone = hitBone;
		component.Arrow = arrow;
	}

	public void SpawnSmokeEffect(Vector3 hitPos, Vector3 hitNormal)
	{
		UnityEngine.Object.Instantiate(BulletSmoke.GetAsset(), hitPos, Quaternion.LookRotation(hitNormal, Vector3.up), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ParticleSystem>().Play();
	}

	public void SpawnSnowPuffEffect(Vector3 hitPos, Vector3 hitNormal)
	{
		UnityEngine.Object.Instantiate(SnowballHit.GetAsset(), hitPos, Quaternion.LookRotation(hitNormal, Vector3.up), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ParticleSystem>().Play();
	}

	public void SpawnBloodEffect(Vector3 hitPos, Vector3 hitNormal, GameObject bone, bool small)
	{
		float num = Mathf.Atan2(hitNormal.x, hitNormal.z) * 57.29578f + 180f;
		Resource<GameObject>[] array = (small ? SmallVolumetricBloodSplatter : VolumetricBloodSplatter);
		BFX_BloodSettings component = UnityEngine.Object.Instantiate(array[MathUtil.NonDeterministicRand.Next(array.Length)].GetAsset(), hitPos, Quaternion.Euler(0f, num + 90f, 0f), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<BFX_BloodSettings>();
		component.AnimationSpeed = 2f;
		component.GroundHeight = GameTerrain.Instance.GetTileHeightAtPos(hitPos.x, hitPos.z);
		component.LightIntensityMultiplier = Sun.GetSunIntensity(Session.Instance.DaysSinceStart);
	}

	public void SpawnBurningPoolEffect(Vector3 pos, float timeout, float radius)
	{
		ParticleSystem component = UnityEngine.Object.Instantiate(BurningPool.GetAsset(), pos, Quaternion.identity, GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ParticleSystem>();
		ParticleSystem.MainModule main = component.main;
		ParticleSystem.ShapeModule shape = component.shape;
		main.duration = timeout;
		shape.radius = radius;
		component.Play();
	}

	public void SpawnExplosionEffect(Vector3 pos)
	{
		UnityEngine.Object.Instantiate(ExplosionEffect.GetAsset(), pos, Quaternion.identity, GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ParticleSystem>().Play();
	}

	public void SpawnGenericHitEffect(string prefabPath, Vector3 hitPos, Vector3 hitNormal)
	{
		Resource<GameObject> resource = Resource<GameObject>.FindResourceByPath(prefabPath);
		if (resource != null && resource.GetAsset() != null)
		{
			if (resource == BloodSplatter)
			{
				SpawnBloodEffect(hitPos, -hitNormal, null, small: false);
			}
			else
			{
				UnityEngine.Object.Instantiate(resource.GetAsset(), hitPos, Quaternion.LookRotation(hitNormal, Vector3.up), GameTerrain.Instance.UnityTerrainObj.transform).GetComponent<ParticleSystem>().Play();
			}
		}
	}

	private void GetAllMeshRenderers(GameObject prop)
	{
		MeshRenderer component = prop.GetComponent<MeshRenderer>();
		if (component != null)
		{
			MeshRenderers.Add(component);
		}
		for (int i = 0; i < prop.transform.childCount; i++)
		{
			GameObject gameObject = prop.transform.GetChild(i).gameObject;
			GetAllMeshRenderers(gameObject);
		}
	}

	public GameObject SpawnBurningEffect(GameObject prop)
	{
		GetAllMeshRenderers(prop);
		GameObject gameObject = new GameObject("Burning Effect");
		gameObject.transform.parent = GameTerrain.Instance.UnityTerrainObj.transform;
		gameObject.AddComponent<AutoDestroyWhenEmptyBehaviour>();
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = SoundManager.BurningSounds[MathUtil.NonDeterministicRand.Next(SoundManager.BurningSounds.Count)];
		audioSource.loop = true;
		audioSource.volume = SoundManager.WorldSoundVolume;
		audioSource.spatialBlend = 1f;
		audioSource.minDistance = SoundManager.AudioRolloffMinDist;
		audioSource.maxDistance = SoundManager.AudioRolloffMaxDist;
		audioSource.RealisticRolloff();
		audioSource.Play();
		foreach (MeshRenderer meshRenderer in MeshRenderers)
		{
			Vector3 size = meshRenderer.bounds.size;
			float num = size.x * size.y * size.z;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(BurningEffect.GetAsset(), gameObject.transform, worldPositionStays: false);
			gameObject2.transform.localPosition = Vector3.zero;
			ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shape = component.shape;
			shape.shapeType = ParticleSystemShapeType.MeshRenderer;
			shape.meshRenderer = meshRenderer;
			ParticleSystem.EmissionModule emission = component.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(num * BurningParticleSpawnRatePerCubicMetre);
			emission.rateOverDistance = new ParticleSystem.MinMaxCurve(num * BurningParticleSpawnRatePerCubicMetre);
			component.Play();
		}
		MeshRenderers.Clear();
		return gameObject;
	}

	public GameObject SpawnDemolitionSmokeEffect(GameObject prop, bool madeOfSnow)
	{
		GetAllMeshRenderers(prop);
		GameObject gameObject = new GameObject("Demolition Smoke Effect");
		gameObject.transform.parent = GameTerrain.Instance.UnityTerrainObj.transform;
		gameObject.AddComponent<AutoDestroyWhenEmptyBehaviour>();
		foreach (MeshRenderer meshRenderer in MeshRenderers)
		{
			Vector3 size = meshRenderer.bounds.size;
			float num = size.x * size.y * size.z;
			GameObject gameObject2 = UnityEngine.Object.Instantiate(madeOfSnow ? ((GameObject)DemolitionSnowEffect) : DemolitionSmokeEffect.GetAsset(), gameObject.transform, worldPositionStays: false);
			gameObject2.transform.localPosition = Vector3.zero;
			ParticleSystem component = gameObject2.GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shape = component.shape;
			shape.shapeType = ParticleSystemShapeType.MeshRenderer;
			shape.meshRenderer = meshRenderer;
			ParticleSystem.EmissionModule emission = component.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(num * SmokeParticleSpawnRatePerCubicMetre);
			emission.rateOverDistance = new ParticleSystem.MinMaxCurve(num * SmokeParticleSpawnRatePerCubicMetre);
			component.Play();
		}
		MeshRenderers.Clear();
		return gameObject;
	}

	public static void StopParticleEffect(GameObject obj)
	{
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
			if (component != null)
			{
				ParticleSystem.ShapeModule shape = component.shape;
				shape.meshRenderer = null;
				component.Stop();
			}
			StopParticleEffect(gameObject);
		}
	}
}
