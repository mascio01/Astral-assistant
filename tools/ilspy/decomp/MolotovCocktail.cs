using System;
using UnityEngine;

public class MolotovCocktail : Throwable
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MolotovCocktail;
	}

	public override BaseObjectType GetProjectileType()
	{
		return BaseObjectType.MolotovCocktailProjectile;
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
		if (character.IsAmbient() && target is Character)
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		if (target == null)
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		if (character.HasInfiniteAmmo(this) && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - character.LastUsedMolotovTime < TimeSpan.FromSeconds(10.0))
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		return base.GetScore(character, target, out bestAmmoType, out bestInfectedWith);
	}

	public override bool OnFired(Character character, Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, float throwAngle, float throwSpeed, bool assassinate, SecrecyMode secret, bool fromAI)
	{
		character.LastUsedMolotovTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		return base.OnFired(character, target, targetBodyLocation, targetPos, throwAngle, throwSpeed, assassinate, secret, fromAI);
	}
}
