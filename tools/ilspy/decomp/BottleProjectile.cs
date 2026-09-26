using UnityEngine;

public class BottleProjectile : BaseThrownProjectile
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.BottleProjectile;
	}

	public override void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		base.Land(hitObject, dir, hitNormal);
		if (_source != null && _source.CheckFrontmostPrediction(PredictedEventType.BottleHitSound) && !PredictedHasLanded)
		{
			if (Proto != null && Proto.ProjectileHitSoundResources != null && Proto.ProjectileHitSoundResources.Count > 0)
			{
				SoundManager.PlaySound3D(Proto.ProjectileHitSoundResources[MathUtil.NonDeterministicRand.Next() % Proto.ProjectileHitSoundResources.Count], Position);
			}
			else
			{
				SoundManager.PlaySound3DFromList(SoundManager.GlassHitSounds, Position);
			}
			if (Proto != null && !string.IsNullOrEmpty(Proto.HitEffectName))
			{
				SpecialEffectManager.Instance.SpawnGenericHitEffect(Proto.HitEffectName, Position, -dir);
			}
		}
		OnHitObjectNonExplosive(hitObject, dir, HitDamage, hitNormal);
		if (!IsAuthoritative())
		{
			return;
		}
		if (Predicted != null)
		{
			Predicted.WantContinueAfterAuthoritativeHasBeenDeleted = true;
		}
		if (Secret != SecrecyMode.OnlyKnownToSubjectCommunity && Secret != SecrecyMode.OnlyKnownToObject && Secret != SecrecyMode.OnlyKnownToSubject)
		{
			TileObject tileObject = _target;
			if (tileObject == null)
			{
				tileObject = hitObject;
			}
			Session.Instance.AISoundManager.AddSound(new AISound((HitDamage > 0f) ? AISoundType.Suspicious : AISoundType.Interesting, Position, 16f, 16f, this, _source, tileObject, _target));
		}
	}
}
