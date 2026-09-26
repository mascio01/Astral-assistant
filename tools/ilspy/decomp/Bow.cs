using UnityEngine;

public class Bow : AmmoWeapon
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Bow;
	}

	public override AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return base.GetFireSound();
		}
		return SoundManager.BowReleaseSound[MathUtil.NonDeterministicRand.Next(SoundManager.BowReleaseSound.Length)];
	}

	public override bool ShowLoadedAmmo()
	{
		return CurrentAmmo > 0;
	}

	public override SkillType GetRangeSkillType()
	{
		return SkillType.Archery;
	}

	public override SkillType GetDamageSkillType()
	{
		return SkillType.Archery;
	}

	public override SkillType GetReloadSpeedSkillType()
	{
		return SkillType.Archery;
	}

	public override bool AllowLockOn(Character character)
	{
		if (CurrentAmmo <= 0)
		{
			return character.Inventory.HasAmmoForWeapon(this);
		}
		return true;
	}
}
