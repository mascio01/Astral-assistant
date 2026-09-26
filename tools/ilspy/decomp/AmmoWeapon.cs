using System;
using System.Collections.Generic;
using UnityEngine;

public class AmmoWeapon : RangedWeapon
{
	public int CurrentAmmo;

	public EquipmentPrototype CurrentAmmoType;

	private static float MaxInaccuracyAngle = 10f;

	private static float BulletHitForceAmount = 2000f;

	private static float Spread = 5f;

	private static List<Character> HitCharacters = new List<Character>();

	public override BaseObjectType GetBaseObjectType()
	{
		return BaseObjectType.AmmoWeapon;
	}

	public virtual int GetMaxAmmo()
	{
		return Prototype.MaxAmmo;
	}

	public virtual List<EquipmentPrototype> GetAmmoTypes()
	{
		return Prototype.AmmoPrototypes;
	}

	public virtual AudioClip GetFireSound()
	{
		if (Prototype.FireSoundResources != null && Prototype.FireSoundResources.Count > 0)
		{
			return Prototype.FireSoundResources[MathUtil.NonDeterministicRand.Next(Prototype.FireSoundResources.Count)];
		}
		return null;
	}

	public virtual AudioClip GetUnloadAmmoSound()
	{
		if (Prototype.ReloadReleaseSoundResources != null && Prototype.ReloadReleaseSoundResources.Count > 0)
		{
			return Prototype.ReloadReleaseSoundResources[MathUtil.NonDeterministicRand.Next(Prototype.ReloadReleaseSoundResources.Count)];
		}
		return null;
	}

	public virtual float GetRecoil()
	{
		return Prototype.Recoil;
	}

	public virtual float GetMaxInaccuracyAngle()
	{
		return MaxInaccuracyAngle * (MathF.PI / 180f);
	}

	public virtual float GetMinAimTime()
	{
		return Prototype.MinAimTime;
	}

	public virtual float GetMaxAimTime()
	{
		return Prototype.MaxAimTime;
	}

	public virtual float GetMinHeadAimTime()
	{
		if (Prototype.MinHeadAimTime != 0f)
		{
			return Prototype.MinHeadAimTime;
		}
		return Prototype.MinAimTime;
	}

	public virtual float GetMaxHeadAimTime()
	{
		if (Prototype.MaxHeadAimTime != 0f)
		{
			return Prototype.MaxHeadAimTime;
		}
		return Prototype.MaxAimTime;
	}

	public virtual float GetMinLegsAimTime()
	{
		if (Prototype.MinLegsAimTime != 0f)
		{
			return Prototype.MinLegsAimTime;
		}
		return Prototype.MinAimTime;
	}

	public virtual float GetMaxLegsAimTime()
	{
		if (Prototype.MaxLegsAimTime != 0f)
		{
			return Prototype.MaxLegsAimTime;
		}
		return Prototype.MaxAimTime;
	}

	public virtual SkillType GetReloadSpeedSkillType()
	{
		return SkillType.Invalid;
	}

	public float GetReloadSpeed(Character character)
	{
		int skillLevelWithEffects = character.GetSkillLevelWithEffects(GetReloadSpeedSkillType());
		return Mathf.Lerp(Prototype.MinReloadSpeed, Prototype.MaxReloadSpeed, (float)skillLevelWithEffects / 5f);
	}

	public virtual float GetAimPenaltyWhenMoving()
	{
		return Prototype.AimPenaltyWhenMoving;
	}

	public virtual float GetBulletHitForceAmount()
	{
		return BulletHitForceAmount;
	}

	public override bool CanFire()
	{
		return CurrentAmmo > 0;
	}

	public override int GetCurrentAmmo()
	{
		return CurrentAmmo;
	}

	public override EquipmentPrototype GetCurrentAmmoType()
	{
		return CurrentAmmoType;
	}

	public override float GetDamageIncludingEffects(Character character, EquipmentPrototype overrideAmmoType = null)
	{
		float num = base.GetDamageIncludingEffects(character, overrideAmmoType);
		if (overrideAmmoType != null)
		{
			num *= overrideAmmoType.DamageModifier;
		}
		else if (CurrentAmmoType != null)
		{
			num *= CurrentAmmoType.DamageModifier;
		}
		return num;
	}

	public override float GetScore(Character character, TileObject target, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		bestAmmoType = null;
		bestInfectedWith = InfectionType.None;
		if (target != null && !(target is Character) && (!target.IsSusceptibleToBulletHits() || !(this is Gun)))
		{
			return 0f;
		}
		Character character2 = target as Character;
		bool flag = character.HasInfiniteAmmo(this);
		float num = float.MinValue;
		List<EquipmentPrototype> ammoTypes = GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				InfectionType infectedWith = InfectedWith;
				if ((flag && (CurrentAmmoType == item || item == Prototype.GetDefaultAmmoPrototype())) || (CurrentAmmoType == item && CurrentAmmo > 0 && character.IsActionAllowedForItem(item, null, InfectedWith, EquipmentPolicyAction.CanUse)) || character.Inventory.HasAmmoOfType(item, out infectedWith, null, character))
				{
					float num2 = GetDamageIncludingEffects(character, item) * (float)GetNumPelletsPerShot() / Math.Max(0.1f, GetMinTimeBetweenFiring());
					if (item.ArmorPiercing > 0f)
					{
						num2 = ((character2 == null || !character2.IsWearingArmor()) ? (num2 * 0.99f) : (num2 * (1f + item.ArmorPiercing)));
					}
					if (character.EquippedItem == this && CurrentAmmoType == item)
					{
						num2 *= 1.01f;
					}
					if (num2 > num)
					{
						num = num2;
						bestAmmoType = item;
						bestInfectedWith = infectedWith;
					}
				}
			}
		}
		return num;
	}

	public override void OnSpawn()
	{
		CurrentAmmo = GetMaxAmmo();
		CurrentAmmoType = Prototype.GetDefaultAmmoPrototype();
		base.OnSpawn();
	}

	public override void Reflect(Reflector reflector)
	{
		base.Reflect(reflector);
		reflector.Add(ref CurrentAmmo);
		if (reflector.Version < 429)
		{
			CurrentAmmoType = ((Prototype != null) ? Prototype.GetDefaultAmmoPrototype() : null);
		}
		else
		{
			reflector.Add(ref CurrentAmmoType);
		}
	}

	public override int GetAmmoCountOfType(EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		if (CurrentAmmoType != ammoType || InfectedWith != infectedWith)
		{
			return 0;
		}
		return CurrentAmmo;
	}

	public override bool HasAmmoOfType(EquipmentPrototype ammoType, InfectionType infectedWith)
	{
		if (CurrentAmmoType == ammoType && InfectedWith == infectedWith)
		{
			return CurrentAmmo > 0;
		}
		return false;
	}

	public override bool HasAmmoOfType(EquipmentPrototype ammoType)
	{
		if (CurrentAmmoType == ammoType)
		{
			return CurrentAmmo > 0;
		}
		return false;
	}

	public override bool HasAmmoForWeapon(AmmoWeapon weapon, Character checkIfCharacterAllowedToUseIt = null)
	{
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				if (CurrentAmmoType == item && CurrentAmmo > 0 && (checkIfCharacterAllowedToUseIt == null || checkIfCharacterAllowedToUseIt.IsActionAllowedForItem(item, null, InfectedWith, EquipmentPolicyAction.CanUse)))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override int TakeBestAmmoForWeapon(Character checkIfCharacterIsAllowed, AmmoWeapon weapon, ref int requiredAmount, ref EquipmentPrototype takenAmmoType, ref InfectionType infectedWith, out bool deleteMe)
	{
		deleteMe = false;
		List<EquipmentPrototype> ammoTypes = weapon.GetAmmoTypes();
		if (ammoTypes != null)
		{
			foreach (EquipmentPrototype item in ammoTypes)
			{
				if (item == CurrentAmmoType && CurrentAmmo > 0 && (takenAmmoType == null || takenAmmoType == item) && (infectedWith == InfectionType.Count || InfectedWith == infectedWith) && (checkIfCharacterIsAllowed == null || checkIfCharacterIsAllowed.IsActionAllowedForItem(item, null, InfectedWith, EquipmentPolicyAction.CanUse)))
				{
					int num = Math.Min(CurrentAmmo, requiredAmount);
					requiredAmount -= num;
					infectedWith = InfectedWith;
					takenAmmoType = item;
					ConsumeAmmo(num);
					return num;
				}
			}
		}
		return 0;
	}

	public void ConsumeAmmo(int amount)
	{
		CurrentAmmo = Math.Max(0, CurrentAmmo - amount);
	}

	public Equipment Unload(TileObject carrier)
	{
		if (CurrentAmmo > 0 && CurrentAmmoType != null)
		{
			Equipment equipment = Equipment.Spawn(CurrentAmmoType, CurrentAmmo);
			equipment.InfectedWith = InfectedWith;
			ConsumeAmmo(CurrentAmmo);
			equipment = carrier.GetInventory().Add(carrier, equipment);
			StoryManager.Instance.RestoreEquipmentInQuestInstances(equipment);
			return equipment;
		}
		return null;
	}

	public override bool CanBeReloaded(Character character, bool checkIfAllowedToUseAmmo)
	{
		if (CurrentAmmo >= GetMaxAmmo())
		{
			return false;
		}
		if (CurrentAmmo > 0 && CurrentAmmoType != null && (!checkIfAllowedToUseAmmo || character.IsActionAllowedForItem(CurrentAmmoType, null, InfectedWith, EquipmentPolicyAction.CanUse)))
		{
			return character.Inventory.HasAmmoOfType(CurrentAmmoType, InfectedWith, this);
		}
		return character.Inventory.HasAmmoForWeapon(this, this, checkIfAllowedToUseAmmo ? character : null);
	}

	public override bool OnFired(Character character, Target target, TargettableBodyLocation targetBodyLocation, Vector3 targetPos, float throwAngle, float throwSpeed, bool assassinate, SecrecyMode secret, bool fromAI)
	{
		if (CurrentAmmo == 0)
		{
			return false;
		}
		if (character.DirectControlled)
		{
			PlayerRecord playerControllingMe = character.GetPlayerControllingMe();
			if (playerControllingMe != null && playerControllingMe.IsLocal)
			{
				HintManager.Instance.Hints[0].MarkPerformed();
			}
		}
		TileObject tileObject = target?.Object;
		float num = (assassinate ? 1f : character.GetAccuracy());
		float rangeIncludingEffects = GetRangeIncludingEffects(character, tileObject);
		float num2 = GetDamageIncludingEffects(character);
		int numPelletsPerShot = GetNumPelletsPerShot();
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		float num3 = (float)character.Id + (float)currentTime.TotalMilliseconds;
		bool flag = tileObject != null && character.SparringPartner == tileObject && character.SparringType == SparringType.FightToTheDeath;
		EquipmentPrototype currentAmmoType = CurrentAmmoType;
		if (character.IsAuthoritative())
		{
			ConsumeAmmo(1);
		}
		if (character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel && (GetFatiguePenaltyWhenAiming() != 0f || GetFatiguePenaltyWhenAttacking() != 0f))
		{
			num2 *= 0.5f;
			if (character.DirectControlled && character.IsAuthoritative() && GameImpl.Instance.Settings.HintsEnabled && Session.Instance.IsLocalPlayerControllingCharacter(character))
			{
				HudBehaviour.Instance.SetStatusBarMsg(GameImpl.Translate(Character.HINT_TooTiredToShoot));
			}
		}
		character.ApplyAimingPenalty(GetRecoil());
		character.ApplyFatiguePenalty(GetFatiguePenaltyWhenAttacking());
		InjuryLocation injuryLocation;
		Vector3 targetAimPos = character.GetTargetAimPos(target, targetBodyLocation, targetPos, deterministic: true, out injuryLocation);
		Vector3 muzzleWorldPos = GetMuzzleWorldPos(character, deterministic: false);
		Vector3 muzzleWorldPos2 = GetMuzzleWorldPos(character, deterministic: true);
		Vector3 normalized = (targetAimPos - muzzleWorldPos2).normalized;
		normalized = GetRandomFireDir(character, normalized, GetMaxInaccuracyAngle() * (1f - num), num3);
		if (character.CheckFrontmostPrediction(PredictedEventType.GunFireSound) && character.IsUnityObjectActive())
		{
			if (this is Gun)
			{
				GameObject unityBone = character.GetUnityBone(GetEquippedBone());
				SpecialEffectManager.Instance.SpawnMuzzleFlash(character, muzzleWorldPos, normalized, unityBone);
			}
			AudioClip fireSound = GetFireSound();
			if (fireSound != null)
			{
				character.PlaySoundOneShot(fireSound);
			}
		}
		HitCharacters.Clear();
		bool flag2 = this is Bow;
		Vector3 vector = Vector3.zero;
		bool flag3 = false;
		for (int i = 0; i < numPelletsPerShot; i++)
		{
			Vector3 vector2 = normalized;
			if (numPelletsPerShot > 1)
			{
				vector2 = GetRandomFireDir(character, normalized, Spread * (MathF.PI / 180f), num3 + 100f * (float)i);
			}
			int flags = 134697;
			RaycastResult raycastResult = GameTerrain.Instance.RayCast(new Ray(muzzleWorldPos2, vector2), rangeIncludingEffects, flags, character.InsideBuilding, character, tileObject, character.IsPredicted());
			if (assassinate && raycastResult.HitObject != tileObject && tileObject is Character)
			{
				Vector3 eyePosition = ((Character)tileObject).EyePosition;
				raycastResult = new RaycastResult(new Ray(muzzleWorldPos2, vector2), tileObject, (eyePosition - muzzleWorldPos2).magnitude, MathUtil.SafeNormalize(muzzleWorldPos2 - eyePosition, Vector3.up), tileObject.GetTile(), Bone.Head, Vector3.zero, 0f);
			}
			Vector3 hitPosition = raycastResult.GetHitPosition();
			Vector3 normal = raycastResult.Normal;
			Vector3 vector3 = raycastResult.GetHitPosition();
			Vector3 hitNormal = raycastResult.Normal;
			BulletHitEffect bulletHitEffect = BulletHitEffect.None;
			Bone bone = Bone.Invalid;
			GameObject hitBone = null;
			ArrowProp arrowProp = null;
			if (raycastResult.HitObject != null)
			{
				if (raycastResult.HitObject is Character character2)
				{
					if (!HitCharacters.Contains(character2))
					{
						HitCharacters.Add(character2);
					}
					if (!character.DirectControlled)
					{
						injuryLocation = character2.PickRandomInjuryLocation(character.PosXZ, targetBodyLocation);
					}
					Vector3 hitPosInBoneSpace;
					if (bone == Bone.Invalid)
					{
						character2.PickRandomHitPos(injuryLocation, (int)(num3 * 1000f) + 1000, muzzleWorldPos, out bone, out hitPosInBoneSpace);
						vector = hitPosInBoneSpace;
					}
					else
					{
						hitPosInBoneSpace = vector;
						character2.PickRandomNearbyHitPos(num3 * 1000f + (float)i * 100f, muzzleWorldPos, bone, ref hitPosInBoneSpace, Spread * (MathF.PI / 180f));
					}
					vector3 = (character2.IsUnityObjectActive() ? character2.GetUnityBoneTransform(bone).MultiplyPoint(hitPosInBoneSpace) : character2.GetBoundingBoxCentre());
					Vector3 normalized2 = (vector3 - muzzleWorldPos).normalized;
					hitNormal = -normalized2;
					float bulletHitRadius = Character.BulletHitRadius;
					bool absorbedByVest = false;
					if (character2.IsAuthoritative() == character.IsAuthoritative())
					{
						character2.OnProjectileHit(character, tileObject, GetInjuryType(), injuryLocation, InfectedWith, currentAmmoType, GetDamageSkillType(), num2, bulletHitRadius, vector3, normalized2 * GetBulletHitForceAmount(), bone, hitPosInBoneSpace, assassinate, secret, targetBodyLocation, out absorbedByVest);
					}
					bulletHitEffect = ((!absorbedByVest) ? BulletHitEffect.Blood : ((injuryLocation == InjuryLocation.Head && !flag2) ? BulletHitEffect.Ricochet : BulletHitEffect.Smoke));
					hitBone = character2.GetUnityBone(bone);
				}
				else
				{
					if (raycastResult.HitObject.IsSusceptibleToBulletHits() && character.IsAuthoritative() == raycastResult.HitObject.IsAuthoritative() && !flag2)
					{
						raycastResult.HitObject.OnProjectileHit(character, normalized, hitPosition, num2, character.IsPredicted());
					}
					bulletHitEffect = raycastResult.HitObject.GetBulletHitEffect(vector3);
				}
				flag3 |= target == null || raycastResult.HitObject == target.Object;
				if (secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToSubject && character.IsAuthoritative() && !assassinate)
				{
					AISoundType type = (flag2 ? AISoundType.Suspicious : AISoundType.Hit);
					AISound sound = new AISound(type, hitPosition, 8f, 8f, character, character, raycastResult.HitObject as TileObject, tileObject);
					sound.WasFightToTheDeath = flag && raycastResult.HitObject == tileObject;
					Session.Instance.AISoundManager.AddSound(sound);
				}
			}
			if (raycastResult.HitObject != null && !(raycastResult.HitObject is Character) && (bulletHitEffect == BulletHitEffect.Smoke || bulletHitEffect == BulletHitEffect.SnowPuff) && flag2)
			{
				if (MathUtil.RandomChoice(num3 + 9865.79f, 1f - currentAmmoType.ChanceOfArrowBreaking / 100f))
				{
					if (character.IsAuthoritative())
					{
						Quaternion rotation = Quaternion.Slerp(Quaternion.LookRotation(vector2), Quaternion.LookRotation(-normal), 0.5f);
						arrowProp = ArrowProp.Spawn(hitPosition, rotation, currentAmmoType);
						if (character.CheckFrontmostPrediction(PredictedEventType.GunFireSound))
						{
							arrowProp.NonDeterministicHit = false;
						}
						else
						{
							ArrowTracerBehaviour arrowTracerBehaviour = ArrowTracerBehaviour.FindArrowTracerBehaviour(character, currentTime);
							if (arrowTracerBehaviour != null)
							{
								arrowTracerBehaviour.Arrow = arrowProp;
								arrowProp.NonDeterministicHit = false;
							}
						}
					}
				}
				else
				{
					bulletHitEffect = BulletHitEffect.Ricochet;
				}
			}
			if (character.CheckFrontmostPrediction(PredictedEventType.SpawnTracer) && character.IsUnityObjectActive())
			{
				if (flag2)
				{
					SpecialEffectManager.Instance.SpawnArrowTracer(character, currentTime, muzzleWorldPos, vector3, hitNormal, bulletHitEffect, hitBone, arrowProp);
				}
				else
				{
					SpecialEffectManager.Instance.SpawnBulletTracer(muzzleWorldPos, vector3, hitNormal, bulletHitEffect, hitBone);
				}
			}
		}
		if (secret != SecrecyMode.OnlyKnownToSubjectCommunity && secret != SecrecyMode.OnlyKnownToObject && secret != SecrecyMode.OnlyKnownToSubject && character.IsAuthoritative())
		{
			AISoundType type2 = (assassinate ? AISoundType.Assassination : ((!flag2) ? AISoundType.Attack : AISoundType.StealthAttack));
			if (HitCharacters.Count > 0)
			{
				foreach (Character hitCharacter in HitCharacters)
				{
					AISound sound2 = new AISound(type2, character.Pos, GetAttackSoundRadius(), character.GetMaxSoundVisibilityRange(), character, character, hitCharacter, tileObject);
					sound2.WasFightToTheDeath = flag && hitCharacter == tileObject;
					Session.Instance.AISoundManager.AddSound(sound2);
				}
			}
			else
			{
				Session.Instance.AISoundManager.AddSound(new AISound(type2, character.Pos, GetAttackSoundRadius(), character.GetMaxSoundVisibilityRange(), character, character, tileObject, tileObject));
			}
		}
		HitCharacters.Clear();
		return flag3;
	}

	public virtual void OnReload(TileObject carrier, int amount, EquipmentPrototype ammoProto, InfectionType infectedWith)
	{
		if ((CurrentAmmoType != ammoProto || InfectedWith != infectedWith) && CurrentAmmo > 0)
		{
			Equipment equipment = Equipment.Spawn(CurrentAmmoType, CurrentAmmo);
			if (equipment != null)
			{
				equipment.InfectedWith = InfectedWith;
				carrier.GetInventory().Add(carrier, equipment);
				CurrentAmmo = 0;
			}
		}
		CurrentAmmo += amount;
		CurrentAmmoType = ammoProto;
		InfectedWith = infectedWith;
	}

	public static Vector3 GetRandomFireDir(Character character, Vector3 dir, float spread, float seed)
	{
		float perpAngle = MathUtil.RandomFloat(seed) * (MathF.PI * 2f);
		float spreadAngle = Mathf.Sqrt(MathUtil.RandomFloat(seed + 1000f)) * spread;
		return GetFireDir(character, dir, perpAngle, spreadAngle);
	}

	public static Vector3 GetFireDir(Character character, Vector3 dir, float perpAngle, float spreadAngle)
	{
		Vector3 rhs = ((Math.Abs(Vector3.Dot(dir, Vector3.up)) < 0.999f) ? Vector3.up : Vector3.right);
		Vector3 vector = Vector3.Normalize(Vector3.Cross(dir, rhs));
		vector = MathUtil.CreateFromAxisAngle(dir, perpAngle).MultiplyVector(vector);
		dir = MathUtil.CreateFromAxisAngle(vector, spreadAngle).MultiplyVector(dir);
		return dir;
	}
}
