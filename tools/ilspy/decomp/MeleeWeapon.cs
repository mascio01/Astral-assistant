using UnityEngine;

public class MeleeWeapon : Weapon
{
	private const int MaxDoses = 10;

	public int DosesRemaining;

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		if (reflector.Version < 545)
		{
			DosesRemaining = ((InfectedWith != InfectionType.None) ? 10 : 0);
		}
		else
		{
			reflector.Add(ref DosesRemaining);
		}
	}

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.MeleeWeapon;
	}

	public override SkillType GetRangeSkillType()
	{
		return SkillType.HandToHand;
	}

	public override SkillType GetDamageSkillType()
	{
		return SkillType.HandToHand;
	}

	public override float GetDamageIncludingEffects(Character character, EquipmentPrototype overrideAmmoType = null)
	{
		float num = base.GetDamageIncludingEffects(character, overrideAmmoType);
		if (character != null && character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel)
		{
			num *= 0.1f;
		}
		return num;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		if (target != null && !(target is Character))
		{
			bestInfectedWith = InfectionType.None;
			return 0f;
		}
		bestInfectedWith = InfectedWith;
		return GetBaseDamage();
	}

	public override void Infect(InfectionType infection)
	{
		base.Infect(infection);
		DosesRemaining = 10;
	}

	public override bool GetLiquidAmountBar(out float amount, out Color col)
	{
		if (InfectedWith != InfectionType.None)
		{
			amount = (float)DosesRemaining / 10f;
			col = GameTerrain.MinimapSettings.GetInfectionCol(InfectedWith);
			return true;
		}
		return base.GetLiquidAmountBar(out amount, out col);
	}

	public override bool CanBeCombinedWith(Equipment other)
	{
		if (InfectedWith != InfectionType.None)
		{
			return false;
		}
		return base.CanBeCombinedWith(other);
	}
}
