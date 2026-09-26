using System;
using UnityEngine;

public class PipeBomb : Throwable
{
	public float Fuse;

	public Character FuseLighter;

	public static float FuseTime = 2f;

	public override void Init()
	{
		base.Init();
		if (Fuse > 0f)
		{
			Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.EveryFrame);
		}
	}

	public override void Delete()
	{
		if (Fuse != 0f)
		{
			Session.Instance.PropManager.RemoveFromObjectsThatNeedUpdating(this, PropManager.Bucket.EveryFrame);
		}
		base.Delete();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.AddAfter(ref Fuse, 628);
		reflector.AddAfter(ref FuseLighter, 628);
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.PipeBomb;
	}

	public override BaseObjectType GetProjectileType()
	{
		return BaseObjectType.PipeBombProjectile;
	}

	public override SkillType GetRangeSkillType()
	{
		return SkillType.Strength;
	}

	public override SkillType GetDamageSkillType()
	{
		return SkillType.Construction;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		if (target is Character { InsideBuilding: null })
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		if (target == null)
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		if (character.HasInfiniteAmmo(this) && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - character.LastUsedPipeBombTime < TimeSpan.FromSeconds(20.0))
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		return base.GetScore(character, target, out bestAmmoType, out bestInfectedWith);
	}

	public override bool OnFired(Character character, Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, float throwAngle, float throwSpeed, bool assassinate, SecrecyMode secret, bool fromAI)
	{
		character.LastUsedPipeBombTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		return base.OnFired(character, target, targetBodyLocation, targetPos, throwAngle, throwSpeed, assassinate, secret, fromAI);
	}

	public void LightFuse(Character lighter)
	{
		Fuse = FuseTime;
		FuseLighter = lighter;
		Session.Instance.PropManager.AddToObjectsThatNeedUpdating(this, PropManager.Bucket.EveryFrame);
	}

	public override void PropUpdate(TimeSpan dt, ref bool stillNeedUpdating)
	{
		if (Fuse > 0f)
		{
			Fuse -= (float)dt.TotalSeconds;
			if (Fuse <= 0f)
			{
				Fuse = -1f;
				if (InventoryOwner != null)
				{
					PipeBombProjectile.Splosion(InventoryOwner.GetBoundingBoxCentre(), InventoryOwner, FuseLighter, null, InventoryOwner, GetDamageIncludingEffects(FuseLighter), SkillType.Stealth, InfectedWith, GetDamageRadius(), predicted: false, frontmost: true, SecrecyMode.Public, itsATrap: false, isFromAI: false);
				}
			}
			else
			{
				stillNeedUpdating = true;
			}
		}
		base.PropUpdate(dt, ref stillNeedUpdating);
	}

	public override bool PropWantDelete()
	{
		if (Fuse < 0f)
		{
			return true;
		}
		return base.PropWantDelete();
	}

	public override bool GetLiquidAmountBar(out float amount, out Color col)
	{
		if (Fuse > 0f)
		{
			amount = Fuse / FuseTime;
			col = Color.red;
			return true;
		}
		return base.GetLiquidAmountBar(out amount, out col);
	}
}
