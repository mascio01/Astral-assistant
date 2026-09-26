using UnityEngine;

public class Pistol : HandGun
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Pistol;
	}

	public override AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return base.GetFireSound();
		}
		return SoundManager.PistolFireSound[MathUtil.NonDeterministicRand.Next(SoundManager.PistolFireSound.Length)];
	}

	public override AudioClip GetUnloadAmmoSound()
	{
		if (Prototype.ReloadReleaseSoundResources != null && Prototype.ReloadReleaseSoundResources.Count > 0)
		{
			return base.GetUnloadAmmoSound();
		}
		return SoundManager.PistolReleaseSound;
	}
}
