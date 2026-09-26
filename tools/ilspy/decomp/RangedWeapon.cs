using System;
using UnityEngine;

public class RangedWeapon : Weapon
{
	public virtual bool CanFire()
	{
		return true;
	}

	public virtual bool CanHoldTriggerToFire()
	{
		return false;
	}

	public virtual bool OnFired(Character character, Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, float throwAngle, float throwSpeed, bool assassinate, SecrecyMode secret, bool fromAI)
	{
		return false;
	}

	public override float GetRangeIncludingEffects(Character character, TileObject targetObj, out float accurateRange, bool includingBuildingEffects, out float accurateRangeAtLowestSkill)
	{
		float num = base.GetRangeIncludingEffects(character, targetObj, out accurateRange, includingBuildingEffects, out accurateRangeAtLowestSkill);
		if (character != null && character.InsideBuilding != null && includingBuildingEffects)
		{
			accurateRange += character.InsideBuilding.GetInhabitantSlotDef(character).WeaponRangeModifier;
			accurateRange = Math.Max(accurateRange, character.InsideBuilding.GetInhabitantSlotDef(character).MinWeaponRange);
			accurateRangeAtLowestSkill += character.InsideBuilding.GetInhabitantSlotDef(character).WeaponRangeModifier;
			accurateRangeAtLowestSkill = Math.Max(accurateRangeAtLowestSkill, character.InsideBuilding.GetInhabitantSlotDef(character).MinWeaponRange);
			num += (float)character.InsideBuilding.GetInhabitantSlotDef(character).WeaponRangeModifier;
			num = Math.Max(num, character.InsideBuilding.GetInhabitantSlotDef(character).MinWeaponRange);
		}
		if (targetObj != null)
		{
			float weaponRangeFactorWhenAimedAtMe = targetObj.GetWeaponRangeFactorWhenAimedAtMe();
			num *= weaponRangeFactorWhenAimedAtMe;
			accurateRange *= weaponRangeFactorWhenAimedAtMe;
			accurateRangeAtLowestSkill *= weaponRangeFactorWhenAimedAtMe;
		}
		return num;
	}
}
