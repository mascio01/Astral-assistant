using System;
using System.Collections.Generic;
using UnityEngine;

public class MolotovCocktailProjectile : BaseThrownProjectile
{
	private static float PuddleScatterRadius = 2f;

	private static float PuddleMinRadius = 0.5f;

	private static float PuddleMaxRadius = 1f;

	private static float PuddleMinTimeout = 8f;

	private static float PuddleMaxTimeout = 12f;

	public static float ExplosionHeightOffset = 0.5f;

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MolotovCocktailProjectile;
	}

	public override void Land(TileObject hitObject, Vector3 dir, Vector3 hitNormal)
	{
		base.Land(hitObject, dir, hitNormal);
		if (_source != null && _source.CheckFrontmostPrediction(PredictedEventType.Explode) && !PredictedHasLanded)
		{
			SoundManager.PlaySound3DFromList(SoundManager.GlassHitSounds, Position);
			int num = MathUtil.NonDeterministicRand.Next(8, 10);
			for (int i = 0; i < num; i++)
			{
				Vector3 pos = Position + MathUtil.NonDeterministicRand.RandomVec3() * PuddleScatterRadius;
				pos.y = GameTerrain.Instance.GetTileHeightAtPos(pos.x, pos.z);
				float radius = Mathf.Lerp(PuddleMinRadius, PuddleMaxRadius, MathUtil.NonDeterministicRand.RandomFloat());
				float timeout = Mathf.Lerp(PuddleMinTimeout, PuddleMaxTimeout, MathUtil.NonDeterministicRand.RandomFloat());
				SpecialEffectManager.Instance.SpawnBurningPoolEffect(pos, timeout, radius);
			}
		}
		TileObject tileObject = null;
		float num2 = 0f;
		List<TileObject> list = new List<TileObject>();
		if (IsPredicted())
		{
			PredictedObjectManager.Instance.GetPredictedObjectsInSphere(new BoundingSphere(Position, DamageRadius), list);
		}
		else
		{
			GameTerrain.Instance.GetObjectsInSphere(new BoundingSphere(Position, DamageRadius), list);
		}
		float fuel = Fuel / (float)Math.Max(4, list.Count);
		foreach (TileObject item in list)
		{
			if (!item.IsFlammable())
			{
				continue;
			}
			float hitDamage = HitDamage;
			if (item == _source || (_source != null && item.IsFriendlyFire(_source, _target, itsATrap: false)))
			{
				if (IsFromAI && !(item is Character))
				{
					continue;
				}
				hitDamage *= Session.Instance.DifficultySettings.FriendlyFireSplashDamage / 100f;
				if (hitDamage == 0f)
				{
					continue;
				}
			}
			float num3 = item.OnBurned(_source, _target, Position + Vector3.up * ExplosionHeightOffset, HitDamage, DamageRadius, fuel, DamageSkillType, InfectedWith, Assassinate, Secret);
			if (num3 > num2)
			{
				tileObject = item;
				num2 = num3;
			}
		}
		list.Clear();
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
			TileObject tileObject2 = _target;
			if (tileObject2 == null)
			{
				tileObject2 = tileObject;
			}
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Explode, Position, 16f, 32f, this, _source, tileObject2, _target));
		}
	}
}
