using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTracerBehaviour : MonoBehaviour
{
	public Character FiredBy;

	public TimeSpan FiredTime;

	public Vector3 StartPos;

	public Vector3 HitPos;

	public Vector3 HitNormal;

	public BulletHitEffect HitEffect;

	public GameObject HitBone;

	public ArrowProp Arrow;

	public float PhysicsTimeout;

	private LineRenderer UnityLineRenderer;

	private MeshRenderer UnityMeshRenderer;

	private float Dist;

	private float DistTravelled;

	private bool SpawnedHitEffect;

	private const float Speed = 100f;

	private const float TrailLength = 4f;

	private static float RicochetSpeed = 12f;

	public static List<ArrowTracerBehaviour> AllArrowTracers = new List<ArrowTracerBehaviour>();

	private static float ArrowLength = 1f;

	public static ArrowTracerBehaviour FindArrowTracerBehaviour(Character firedBy, TimeSpan firedTime)
	{
		foreach (ArrowTracerBehaviour allArrowTracer in AllArrowTracers)
		{
			if (allArrowTracer.FiredBy == firedBy || allArrowTracer.FiredTime == firedTime)
			{
				return allArrowTracer;
			}
		}
		return null;
	}

	private void Start()
	{
		Dist = (HitPos - StartPos).magnitude;
		UnityLineRenderer = GetComponent<LineRenderer>();
		UnityMeshRenderer = GetComponent<MeshRenderer>();
		AllArrowTracers.Add(this);
	}

	private void OnDestroy()
	{
		AllArrowTracers.Remove(this);
	}

	private void Update()
	{
		DistTravelled += 100f * Time.deltaTime;
		float num = DistTravelled - 4f;
		if (PhysicsTimeout > 0f)
		{
			PhysicsTimeout -= Time.deltaTime;
		}
		else
		{
			base.transform.position = Vector3.Lerp(StartPos, HitPos, Mathf.Clamp(DistTravelled - ArrowLength, 0f, Dist - ArrowLength) / Dist);
			base.transform.rotation = Quaternion.LookRotation(MathUtil.SafeNormalize(HitPos - StartPos, base.transform.localToWorldMatrix.Forward()));
		}
		if (num < Dist)
		{
			UnityLineRenderer.SetPosition(0, StartPos + (HitPos - StartPos) * Mathf.Clamp(num, 0f, Dist - ArrowLength) / Dist);
			UnityLineRenderer.SetPosition(1, StartPos + (HitPos - StartPos) * Mathf.Clamp(DistTravelled - ArrowLength, 0f, Dist - ArrowLength) / Dist);
		}
		if (DistTravelled >= Dist && !SpawnedHitEffect)
		{
			switch (HitEffect)
			{
			case BulletHitEffect.Blood:
				UnityMeshRenderer.enabled = false;
				SoundManager.PlaySound3DFromList(SoundManager.BulletHitSounds, HitPos);
				SpecialEffectManager.Instance.SpawnBloodEffect(HitPos, HitNormal, HitBone, small: false);
				SpecialEffectManager.Instance.SpawnBloodEffect(HitPos, -HitNormal, HitBone, small: false);
				break;
			case BulletHitEffect.Ricochet:
			{
				SoundManager.PlaySound3DFromList(SoundManager.ArrowHitRockSounds, HitPos);
				PhysicsTimeout = 2f;
				Rigidbody component = GetComponent<Rigidbody>();
				component.isKinematic = false;
				component.collisionDetectionMode = CollisionDetectionMode.Continuous;
				component.linearVelocity = RicochetSpeed * MathUtil.SafeNormalize(HitPos - StartPos, base.transform.localToWorldMatrix.Forward());
				GetComponent<CapsuleCollider>().enabled = true;
				break;
			}
			case BulletHitEffect.Smoke:
			case BulletHitEffect.SnowPuff:
				UnityMeshRenderer.enabled = false;
				SoundManager.PlaySound3DFromList(SoundManager.ArrowHitSounds, HitPos);
				if (Arrow != null)
				{
					Arrow.OnUnityNonDeterministicHit();
				}
				break;
			}
			AllArrowTracers.Remove(this);
			SpawnedHitEffect = true;
		}
		if (num >= Dist)
		{
			UnityLineRenderer.enabled = false;
			if (PhysicsTimeout <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
