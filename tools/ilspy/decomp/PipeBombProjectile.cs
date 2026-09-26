using System;
using System.Collections.Generic;
using UnityEngine;

public class PipeBombProjectile : PropProjectile
{
	public bool Exploded;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PipeBombProjectile;
	}

	public override bool PropWantDelete()
	{
		return Exploded;
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref Exploded, 302);
	}

	public static void Splosion(Vector3 pos, TileObject bomb, Character source, TileObject ignore, TileObject target, float hitDamage, SkillType damageSkillType, InfectionType infectionType, float damageRadius, bool predicted, bool frontmost, SecrecyMode secret, bool itsATrap, bool isFromAI)
	{
		if (frontmost)
		{
			SoundManager.PlaySound3DFromList(SoundManager.ExplosionSounds, pos);
			SpecialEffectManager.Instance.SpawnExplosionEffect(pos);
		}
		TileObject tileObject = null;
		float num = 0f;
		List<TileObject> list = new List<TileObject>();
		if (predicted)
		{
			PredictedObjectManager.Instance.GetPredictedObjectsInSphere(new BoundingSphere(pos, damageRadius), list);
		}
		else
		{
			GameTerrain.Instance.GetObjectsInSphere(new BoundingSphere(pos, damageRadius), list);
		}
		foreach (TileObject item in list)
		{
			if (item == ignore || item.IsExplosionProof())
			{
				continue;
			}
			float num2 = hitDamage;
			if (item == source || (source != null && item.IsFriendlyFire(source, target, itsATrap)))
			{
				if (isFromAI && !(item is Character))
				{
					continue;
				}
				num2 *= Session.Instance.DifficultySettings.FriendlyFireSplashDamage / 100f;
				if (num2 == 0f)
				{
					continue;
				}
			}
			float num3 = item.OnExplosionImpact(source, target, pos, num2, damageRadius, fromFoundations: false, damageSkillType, infectionType, itsATrap, bomb);
			if (num3 > num)
			{
				tileObject = item;
				num = num3;
			}
		}
		list.Clear();
		if (!predicted && secret != SecrecyMode.Private && secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToSubject)
		{
			TileObject tileObject2 = target;
			if (tileObject2 == null)
			{
				tileObject2 = tileObject;
			}
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Explode, pos, 32f, 32f, bomb, source, tileObject2, target));
		}
	}

	public override void ProjectileUpdate(TimeSpan dt)
	{
		base.ProjectileUpdate(dt);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(IsPredicted());
		if (Landed && !Exploded && currentTime - LandedTime >= TimeSpan.FromSeconds(1.0))
		{
			bool frontmost = _source != null && _source.CheckFrontmostPrediction(PredictedEventType.BottleHitSound) && !PredictedHasLanded;
			Splosion(Position + Vector3.up * MolotovCocktailProjectile.ExplosionHeightOffset, this, _source, null, _target, HitDamage, DamageSkillType, InfectedWith, DamageRadius, IsPredicted(), frontmost, Secret, itsATrap: false, IsFromAI);
			Exploded = true;
			if (IsAuthoritative() && Predicted != null)
			{
				Predicted.WantContinueAfterAuthoritativeHasBeenDeleted = true;
			}
		}
	}
}
