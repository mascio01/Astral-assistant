using UnityEngine;

public class CharacterBehaviour : MonoBehaviour
{
	public Human Owner;

	private void SendEvent(string s)
	{
	}

	private void OnAnimatorIK(int layerIndex)
	{
		Owner.GetPredictedOrElseThisHuman().UnityUpdateIK();
	}

	private void BowPullSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.BowPullSound);
	}

	private void PistolRelease()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadReleaseSoundResources != null && equipmentPrototype.ReloadReleaseSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadReleaseSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadReleaseSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.PistolReleaseSound);
			}
		}
	}

	private void PistolInsert()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadInsertSoundResources != null && equipmentPrototype.ReloadInsertSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadInsertSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadInsertSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.PistolInsertSound);
			}
		}
	}

	private void PistolSlide()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadSlideSoundResources != null && equipmentPrototype.ReloadSlideSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadSlideSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadSlideSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.PistolSlideSound);
			}
		}
	}

	private void ShotgunPump()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadSlideSoundResources != null && equipmentPrototype.ReloadSlideSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadSlideSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadSlideSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.ShotgunPumpSound);
			}
		}
	}

	private void AssaultRifleRelease()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadReleaseSoundResources != null && equipmentPrototype.ReloadReleaseSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadReleaseSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadReleaseSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.AssaultRifleReleaseSound);
			}
		}
	}

	private void AssaultRifleInsert()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadInsertSoundResources != null && equipmentPrototype.ReloadInsertSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadInsertSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadInsertSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.AssaultRifleInsertSound);
			}
		}
	}

	private void AssaultRifleSlide()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadSlideSoundResources != null && equipmentPrototype.ReloadSlideSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadSlideSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadSlideSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.AssaultRifleSlideSound);
			}
		}
	}

	private void RPGLoad()
	{
	}

	private void SniperRifleRelease()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadReleaseSoundResources != null && equipmentPrototype.ReloadReleaseSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadReleaseSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadReleaseSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.SniperRifleReleaseSound);
			}
		}
	}

	private void SniperRifleInsert()
	{
		if (Owner.GetPredictedOrElseThisHuman().CurrentActionAnim == ActionAnim.Reload)
		{
			EquipmentPrototype equipmentPrototype = ((Owner != null && Owner.EquippedItem != null) ? Owner.EquippedItem.GetPrototype() : null);
			if (equipmentPrototype != null && equipmentPrototype.ReloadInsertSoundResources != null && equipmentPrototype.ReloadInsertSoundResources.Count > 0)
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(equipmentPrototype.ReloadInsertSoundResources[MathUtil.NonDeterministicRand.Next() % equipmentPrototype.ReloadInsertSoundResources.Count]);
			}
			else
			{
				Owner.GetPredictedOrElseThis().PlaySoundOneShot(SoundManager.SniperRifleInsertSound);
			}
		}
	}

	private void ChopWoodSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.ChopWoodSounds);
	}

	private void HammerSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.HammerSounds, 0.5f);
	}

	private void DigSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.DigSounds);
	}

	private void DigThrowSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.DigThrowSounds);
	}

	private void ForgeSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.ForgeSounds);
	}

	private void DrinkSound()
	{
		Owner?.GetPredictedOrElseThisCharacter().PlaySoundUsingFootstepAudioSourceFromList(SoundManager.DrinkSounds, 1f);
	}

	private void EatSound()
	{
		Owner?.GetPredictedOrElseThisCharacter().PlaySoundUsingFootstepAudioSourceFromList(SoundManager.EatSounds, 1f);
	}

	private void StrikeMatch()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.MatchSounds);
	}

	private void StrikeFlint()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.FlintSounds);
	}

	private void BreakRockSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.BreakRockSounds);
	}

	private void SkinningSound()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.SkinningSounds);
	}

	private void StartApplyBandageToSelf()
	{
		Owner.GetPredictedOrElseThis().PlaySoundOneShotFromList(SoundManager.BandageSounds);
	}

	private void WalkingFootstepSound(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			Owner.GetPredictedOrElseThisHuman().PlayFoostepSound(attacking: false);
		}
	}

	private void RunningFootstepSound(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			Owner.GetPredictedOrElseThisHuman().PlayFoostepSound(attacking: false);
		}
	}

	private void CrouchWalkingFootstepSound(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			Owner.GetPredictedOrElseThisHuman().PlayFoostepSound(attacking: false);
		}
	}

	private void AttackFootstepSound(AnimationEvent animationEvent)
	{
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			Owner.GetPredictedOrElseThisHuman().PlayFoostepSound(attacking: true);
		}
	}
}
