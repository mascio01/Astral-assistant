using UnityEngine;

public class BulletTracerBehaviour : MonoBehaviour
{
	public Vector3 StartPos;

	public Vector3 HitPos;

	public Vector3 HitNormal;

	public BulletHitEffect HitEffect;

	public GameObject HitBone;

	private LineRenderer UnityLineRenderer;

	private float Dist;

	private float DistTravelled;

	private bool SpawnedHitEffect;

	private const float Speed = 100f;

	private const float TrailLength = 4f;

	private void Start()
	{
		Dist = (HitPos - StartPos).magnitude;
		UnityLineRenderer = GetComponent<LineRenderer>();
	}

	private void Update()
	{
		DistTravelled += 100f * Time.deltaTime;
		float num = DistTravelled - 4f;
		if (DistTravelled >= Dist && !SpawnedHitEffect)
		{
			switch (HitEffect)
			{
			case BulletHitEffect.Blood:
				SoundManager.PlaySound3DFromList(SoundManager.BulletHitSounds, HitPos);
				SpecialEffectManager.Instance.SpawnBloodEffect(HitPos, HitNormal, HitBone, small: false);
				SpecialEffectManager.Instance.SpawnBloodEffect(HitPos, -HitNormal, HitBone, small: false);
				break;
			case BulletHitEffect.Ricochet:
				SoundManager.PlaySound3DFromList(SoundManager.RicochetSounds, HitPos);
				SpecialEffectManager.Instance.SpawnBulletTracer(HitPos, HitPos + (HitNormal + MathUtil.NonDeterministicRand.RandomVec3()).normalized * 2f, Vector3.zero, BulletHitEffect.None, null);
				break;
			case BulletHitEffect.Smoke:
				SpecialEffectManager.Instance.SpawnSmokeEffect(HitPos, HitNormal);
				break;
			case BulletHitEffect.SnowPuff:
				SpecialEffectManager.Instance.SpawnSnowPuffEffect(HitPos, HitNormal);
				break;
			}
			SpawnedHitEffect = true;
		}
		if (num >= Dist)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		UnityLineRenderer.SetPosition(0, StartPos + (HitPos - StartPos) * Mathf.Clamp(num, 0f, Dist) / Dist);
		UnityLineRenderer.SetPosition(1, StartPos + (HitPos - StartPos) * Mathf.Clamp(DistTravelled, 0f, Dist) / Dist);
	}
}
