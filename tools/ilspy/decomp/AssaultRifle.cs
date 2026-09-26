using UnityEngine;

public class AssaultRifle : LongGun
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.AssaultRifle;
	}

	public override AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return base.GetFireSound();
		}
		return SoundManager.AssaultRifleFireSound[MathUtil.NonDeterministicRand.Next(SoundManager.AssaultRifleFireSound.Length)];
	}

	public override AudioClip GetUnloadAmmoSound()
	{
		if (Prototype.ReloadReleaseSoundResources != null && Prototype.ReloadReleaseSoundResources.Count > 0)
		{
			return base.GetUnloadAmmoSound();
		}
		return SoundManager.AssaultRifleReleaseSound;
	}

	public override bool CanHoldTriggerToFire()
	{
		return true;
	}
}
