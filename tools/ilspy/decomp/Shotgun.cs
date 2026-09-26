using UnityEngine;

public class Shotgun : LongGun
{
	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.Shotgun;
	}

	public override AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return base.GetFireSound();
		}
		return SoundManager.ShotgunFireSound[MathUtil.NonDeterministicRand.Next(SoundManager.ShotgunFireSound.Length)];
	}

	public override AudioClip GetUnloadAmmoSound()
	{
		if (Prototype.ReloadReleaseSoundResources != null && Prototype.ReloadReleaseSoundResources.Count > 0)
		{
			return base.GetUnloadAmmoSound();
		}
		return SoundManager.ShotgunPumpSound;
	}
}
