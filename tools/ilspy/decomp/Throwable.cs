using System;
using UnityEngine;

public class Throwable : RangedWeapon
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Throwable;
	}

	public virtual BaseObjectType GetProjectileType()
	{
		return BaseObjectType.ThrownProjectile;
	}

	public virtual float GetDamageRadius()
	{
		return Prototype.DamageRadius;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		bool flag = target is Character;
		float num = 1f;
		if (target != null)
		{
			bool flag2 = flag;
			if (this is MolotovCocktail && target.IsFlammable())
			{
				flag2 = true;
			}
			if (this is PipeBomb && !target.IsExplosionProof())
			{
				flag2 = true;
			}
			if (!flag2)
			{
				bestInfectedWith = InfectionType.None;
				return 0f;
			}
		}
		num = GetDamageIncludingEffects(character);
		num = ((!flag) ? (num * 2f) : (num * 0.1f));
		if (character.EquippedItem == this)
		{
			num *= 1.5f;
		}
		bestInfectedWith = InfectedWith;
		return num;
	}

	public override bool OnFired(Character character, Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, float throwAngle, float throwSpeed, bool assassinate, SecrecyMode secret, bool fromAI)
	{
		if (character.DirectControlled)
		{
			PlayerRecord playerControllingMe = character.GetPlayerControllingMe();
			if (playerControllingMe != null && playerControllingMe.IsLocal)
			{
				HintManager.Instance.Hints[0].MarkPerformed();
			}
		}
		TileObject tileObject = target?.Object;
		Character character2 = tileObject as Character;
		float rangeIncludingEffects = GetRangeIncludingEffects(character, tileObject);
		float num = GetDamageIncludingEffects(character);
		if (character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel && (GetFatiguePenaltyWhenAiming() != 0f || GetFatiguePenaltyWhenAttacking() != 0f) && !(this is MolotovCocktail) && !(this is PipeBomb))
		{
			num *= 0.5f;
		}
		character.ApplyFatiguePenalty(GetFatiguePenaltyWhenAttacking());
		Vector3 throwPosition = character.ThrowPosition;
		targetPos = character.GetTargetAimPos(target, targetBodyLocation, targetPos, deterministic: true, out var injuryLocation);
		if (character2 != null)
		{
			injuryLocation = character2.PickRandomInjuryLocation(character.PosXZ, targetBodyLocation);
		}
		targetPos = throwPosition + MathUtil.ToXZY(MathUtil.ToXZ(character.Forward) * Math.Min(rangeIncludingEffects, MathUtil.ToXZ(targetPos - throwPosition).magnitude), targetPos.y - throwPosition.y);
		float num2 = ((GetLiquidContentsType() != null && GetLiquidContentsType().Flammable) ? GetLiquidContentsAmount() : 0f) + Prototype.Fuel;
		if (character.EquippedItem == this)
		{
			character.EquippedItem = null;
		}
		if (character.CheckFrontmostPrediction(PredictedEventType.ThrowProjectile) || character.IsAuthoritative())
		{
			BaseThrownProjectile.Spawn(GetProjectileType(), Prototype, InfectedWith, character, tileObject, injuryLocation, throwPosition, targetPos, throwAngle, throwSpeed, rangeIncludingEffects, num2, num, GetDamageSkillType(), GetDamageRadius(), assassinate, secret, character.StealUnityWeapon(), fromAI);
		}
		if (character.IsAuthoritative())
		{
			if (!character.HasInfiniteAmmo(this))
			{
				IncrementAmount(-1);
				character.Inventory.CacheEncumbered(character);
			}
			if (GetAmount() == 0)
			{
				character.Inventory.Remove(character, this);
				character.DesiredEquippedItem = character.Inventory.FindItemOfType(Prototype);
				Delete();
			}
		}
		if (secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToSubject && character.IsAuthoritative())
		{
			if ((num == 0f && num2 == 0f && tileObject != null) || !character.IsEnemy(tileObject))
			{
				tileObject = null;
			}
			Session.Instance.AISoundManager.AddSound(new AISound((this is MolotovCocktail || this is PipeBomb) ? AISoundType.Attack : AISoundType.StealthAttack, character.Pos, GetAttackSoundRadius(), character.GetMaxSoundVisibilityRange(), character, character, tileObject, tileObject, Prototype));
		}
		return true;
	}
}
