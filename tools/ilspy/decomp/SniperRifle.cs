using UnityEngine;

public class SniperRifle : LongGun
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.SniperRifle;
	}

	public override AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return base.GetFireSound();
		}
		return SoundManager.SniperRifleFireSound[MathUtil.NonDeterministicRand.Next(SoundManager.SniperRifleFireSound.Length)];
	}

	public override AudioClip GetUnloadAmmoSound()
	{
		if (Prototype.ReloadReleaseSoundResources != null && Prototype.ReloadReleaseSoundResources.Count > 0)
		{
			return base.GetUnloadAmmoSound();
		}
		return SoundManager.SniperRifleReleaseSound;
	}
}
