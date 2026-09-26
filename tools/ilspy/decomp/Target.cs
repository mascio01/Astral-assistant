using System;
using UnityEngine;

public class Target : IReflectable
{
	public TileObject Object;

	public int RefCount;

	public bool Predicted;

	public int SanityCheckRefCount;

	public static TimeSpan Never = -TimeSpan.FromDays(1000.0);

	public TimeSpan LastUpdatedTime = -TimeSpan.FromDays(100.0);

	public TimeSpan LastVisibleTime = Never;

	public TimeSpan LastFullyTrackedTime = Never;

	public TimeSpan LastLineOfSightIgnoringPlantCoverTime = Never;

	public TimeSpan LastHeardTime = Never;

	public TimeSpan LastHeardAttackTime = Never;

	public TimeSpan LastSmelledTime = Never;

	public TimeSpan LastAttackedMeTime = Never;

	public TimeSpan LastAttackedUsTime = Never;

	public TimeSpan LastInaccessibleTime = Never;

	public Vector3 LastKnownPosition;

	public float LastVisibility;

	public float Camouflage = 1f;

	public float Movement;

	private TimeSpan LoseTargetTimer = TimeSpan.Zero;

	private int _lastKnownFlags;

	public int InvestigateCorpseCount;

	private static TimeSpan DistractionTime = TimeSpan.FromSeconds(10.0);

	public static TimeSpan LoseTargetTime = TimeSpan.FromSeconds(8.0);

	public static float MaxLoseCamoSpeed = 2f;

	public static float MaxGainCamoSpeed = 1f;

	public static float SkillFactorInCover = 0.8f;

	public static float SkillFactorInOpen = 0.2f;

	public static int StolenAnimalOutsideBaseRange = 32;

	public static float AlertCamouflageThreshold = 0.5f;

	public bool Visible => LastVisibleTime >= LastUpdatedTime;

	public bool FullyTracked => LastFullyTrackedTime >= LastUpdatedTime;

	public bool HasLineOfSightIgnoringPlantCover => LastLineOfSightIgnoringPlantCoverTime >= LastUpdatedTime;

	public bool CanSmellTarget => LastSmelledTime >= LastUpdatedTime;

	public TimeSpan TimeSinceLastVisible => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastVisibleTime;

	public TimeSpan TimeSinceLastFullyVisible => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastFullyTrackedTime;

	public TimeSpan TimeSinceLastLineOfSightIgnoringPlantCover => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastLineOfSightIgnoringPlantCoverTime;

	public TimeSpan TimeSinceLastHeard => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastHeardTime;

	public TimeSpan TimeSinceLastHeardAttack => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastHeardAttackTime;

	public TimeSpan TimeSinceLastSmelled => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastSmelledTime;

	public TimeSpan TimeSinceLastAttackedMe => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastAttackedMeTime;

	public TimeSpan TimeSinceLastAttackedUs => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastAttackedUsTime;

	public TimeSpan TimeSinceLastDetected => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - MathUtil.Max(MathUtil.Max(MathUtil.Max(LastVisibleTime, LastHeardTime), LastSmelledTime), LastAttackedUsTime);

	public TimeSpan TimeSinceInaccessible => PredictedObjectManager.Instance.GetCurrentTime(Predicted) - LastInaccessibleTime;

	public Target()
	{
	}

	public Target(TileObject obj)
	{
		Object = obj;
		if (!(obj is Character))
		{
			Camouflage = 0f;
		}
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref LastUpdatedTime);
		reflector.Add(ref LastVisibleTime);
		reflector.Add(ref LastFullyTrackedTime);
		reflector.Add(ref LastLineOfSightIgnoringPlantCoverTime);
		reflector.Add(ref LastHeardTime);
		reflector.Add(ref LastHeardAttackTime);
		reflector.Add(ref LastSmelledTime);
		reflector.Add(ref LastAttackedMeTime);
		reflector.Add(ref LastAttackedUsTime);
		reflector.Add(ref LastInaccessibleTime);
		reflector.Add(ref LastKnownPosition);
		reflector.Add(ref LastVisibility);
		reflector.Add(ref Camouflage);
		reflector.AddAfter(ref Movement, 258);
		reflector.Add(ref LoseTargetTimer);
		reflector.Add(ref _lastKnownFlags);
		reflector.Add(ref InvestigateCorpseCount);
	}

	public void ShareInfoWith(Target other)
	{
		if (other.TimeSinceLastDetected > TimeSinceLastDetected && !other.FullyTracked)
		{
			other.LastKnownPosition = LastKnownPosition;
		}
		other.LastUpdatedTime = MathUtil.Max(other.LastUpdatedTime, LastUpdatedTime);
		other.LastVisibleTime = MathUtil.Max(other.LastVisibleTime, LastVisibleTime);
		other.LastFullyTrackedTime = MathUtil.Max(other.LastFullyTrackedTime, LastFullyTrackedTime);
		other.LastLineOfSightIgnoringPlantCoverTime = MathUtil.Max(other.LastLineOfSightIgnoringPlantCoverTime, LastFullyTrackedTime);
		other.LastHeardTime = MathUtil.Max(other.LastHeardTime, LastHeardTime);
		other.LastHeardAttackTime = MathUtil.Max(other.LastHeardAttackTime, LastHeardAttackTime);
		other.LastSmelledTime = MathUtil.Max(other.LastSmelledTime, LastSmelledTime);
		other.LastAttackedUsTime = MathUtil.Max(other.LastAttackedUsTime, LastAttackedUsTime);
		other.LastVisibility = Math.Max(other.LastVisibility, LastVisibility);
		other.Camouflage = Math.Min(other.Camouflage, Camouflage);
		other.Movement = Math.Min(other.Movement, Movement);
		other.SetFlag(TargetFlags.KnownAssailant, other.GetFlag(TargetFlags.KnownAssailant) || GetFlag(TargetFlags.KnownAssailant));
		other.SetFlag(TargetFlags.DeadOrZombie, other.GetFlag(TargetFlags.DeadOrZombie) || GetFlag(TargetFlags.DeadOrZombie));
	}

	public void ForceVisible(Character character)
	{
		MarkVisible(1f, hasLineOfSightIgnoringPlantCover: true);
		Camouflage = 0f;
		LoseTargetTimer = LoseTargetTime;
		LastFullyTrackedTime = (LastUpdatedTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted));
		if (LastAttackedUsTime != Never)
		{
			SetFlag(TargetFlags.KnownAssailant, on: true);
		}
		UpdateLastKnownInfo(character);
	}

	public void OnSound(Character character, AISound sound, bool seen, bool heard)
	{
		if (sound.Type == AISoundType.Radio && !character.Zombie)
		{
			return;
		}
		if (sound.Type == AISoundType.Knock)
		{
			SetFlag(TargetFlags.KnockedOnGate, on: true);
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
		if (AISound.SoundComesFromSource(sound.Type) || sound.Type == AISoundType.Suspicious || sound.Type == AISoundType.Interesting || sound.Target != character)
		{
			if (sound.Speaker != character)
			{
				LastHeardTime = currentTime;
			}
			if (AISound.SoundComesFromSource(sound.Type) || TimeSinceLastFullyVisible >= DistractionTime)
			{
				UpdateLastKnownInfo(character, sound.Pos);
			}
		}
		if (sound.Type == AISoundType.Knock || (heard && sound.Type == AISoundType.Attack) || (seen && AISound.SoundComesFromSource(sound.Type) && Camouflage < 1f) || (seen && AISound.SoundComesFromSource(sound.Type) && HasLineOfSightIgnoringPlantCover))
		{
			ForceVisible(character);
		}
		if (AISound.IsAttackSound(sound.Source, character, sound.IntendedTarget, sound.Type, sound.EquipmentProto) && sound.Source != null && (sound.Source.SparringPartner == null || sound.Source.SparringPartner != sound.Target || sound.Type != AISoundType.Pain))
		{
			SetFlag(TargetFlags.HasLoadedRangedWeapon, sound.Source.HasEquippedRangedWeaponAndAmmo(GetFlag(TargetFlags.HasLoadedRangedWeapon)));
			LastHeardAttackTime = currentTime;
			Character character2 = sound.Target as Character;
			if (sound.Target != null && sound.Target.GetCommunity() == character.GetCommunity() && (character2?.Zombie ?? false) == character.Zombie && !character.IsEmpathyDisabled(sound.Target))
			{
				bool flag = true;
				if ((sound.Type == AISoundType.Hit || sound.Type == AISoundType.Pain || sound.Type == AISoundType.Explode) && sound.IntendedTarget != sound.Target)
				{
					if (sound.Source.GetCommunity() == character.GetCommunity() && character.GetCommunity() != null)
					{
						flag = false;
					}
					if (!sound.Source.IsControllableByPlayer() && !character.IsEnemy(Object))
					{
						flag = false;
					}
					if (character.Community != null && Object != null)
					{
						if (character.Community.CachedAllies.Contains(sound.Source.Community))
						{
							flag = false;
						}
						if (character.Community.IsFEMA && sound.Source.Community != null && sound.Source.Community.IsFEMA)
						{
							flag = false;
						}
					}
				}
				if (sound.Type == AISoundType.Hit && !(sound.Target is Character) && (!(sound.IntendedTarget is Character) || !character.IsInMyCommunityOrAlly(sound.IntendedTarget)))
				{
					flag = false;
				}
				if (flag)
				{
					if (character.Community != null && character.Community.CachedAllies.Contains(sound.Source.Community) && Object != null)
					{
						Debug.Log("Ally attacked us? " + character.GetDisplayNameString() + ", " + sound.Source.GetDisplayNameString() + ", " + Object.GetDisplayNameString() + ((sound.IntendedTarget != null) ? (", " + sound.IntendedTarget.GetDisplayNameString()) : "") + ", " + sound.Type);
					}
					LastAttackedUsTime = currentTime;
					if (FullyTracked)
					{
						SetFlag(TargetFlags.KnownAssailant, on: true);
					}
				}
			}
		}
		if (sound.Type != AISoundType.Warning)
		{
			SetFlag(TargetFlags.Indoors, sound.Source != null && !sound.Source.IsOutdoors() && sound.Type != AISoundType.Radio);
		}
	}

	public void OnHeardFootsteps(Character character)
	{
		LastHeardTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
		SetFlag(TargetFlags.Indoors, on: false);
		UpdateLastKnownInfo(character);
	}

	public void OnHeardAboutBody(Character character)
	{
		LastHeardTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
		ForceVisible(character);
	}

	public void OnSmelled(Character character)
	{
		if (!(Camouflage > 0f) || !(TimeSinceLastHeard < DistractionTime))
		{
			LastSmelledTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
			SetFlag(TargetFlags.Indoors, on: false);
			UpdateLastKnownInfo(character);
		}
	}

	public void OnAttackedMe(Character character, Vector3 pos, bool forceVisible)
	{
		if (character.Community != null && Object != null && character.Community.CachedAllies.Contains(Object.GetCommunity()))
		{
			Debug.Log("Ally attacked us? " + character.GetDisplayNameString() + ", " + Object.GetDisplayNameString());
		}
		LastAttackedUsTime = (LastAttackedMeTime = (LastHeardAttackTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted)));
		if (FullyTracked)
		{
			SetFlag(TargetFlags.KnownAssailant, on: true);
		}
		SetFlag(TargetFlags.Indoors, on: false);
		UpdateLastKnownInfo(character, pos);
		if (forceVisible && (Camouflage < 1f || HasLineOfSightIgnoringPlantCover))
		{
			ForceVisible(character);
		}
	}

	public void OnGrabbedMe(Character character)
	{
		LastAttackedUsTime = (LastAttackedMeTime = (LastHeardAttackTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted)));
		if (FullyTracked)
		{
			SetFlag(TargetFlags.KnownAssailant, on: true);
		}
		SetFlag(TargetFlags.Indoors, on: false);
		UpdateLastKnownInfo(character);
		ForceVisible(character);
	}

	public void TakeBlameForAttacks(TimeSpan time)
	{
		LastHeardAttackTime = (LastAttackedUsTime = time);
		SetFlag(TargetFlags.KnownAssailant, on: true);
	}

	public void ForgetUnprovenAttacks()
	{
		if (LastAttackedUsTime != Never && !GetFlag(TargetFlags.KnownAssailant))
		{
			LastHeardAttackTime = (LastAttackedUsTime = (LastAttackedMeTime = Never));
			SetFlag(TargetFlags.KnownAssailant, on: false);
		}
	}

	public void OnTargetedCommunityMemberDiedOrKnockedUnconscious(Character character)
	{
		Camouflage = 1f;
		Movement = 0f;
		LoseTargetTimer = TimeSpan.Zero;
		LastVisibleTime = Never;
		LastFullyTrackedTime = Never;
		LastLineOfSightIgnoringPlantCoverTime = Never;
		LastHeardTime = Never;
		LastHeardAttackTime = Never;
		LastSmelledTime = Never;
		LastAttackedMeTime = Never;
		LastAttackedUsTime = Never;
		LastInaccessibleTime = Never;
	}

	public void MarkVisible(float visibility, bool hasLineOfSightIgnoringPlantCover)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
		if (visibility > 0f)
		{
			LastVisibility = visibility;
			LastVisibleTime = currentTime;
		}
		if (hasLineOfSightIgnoringPlantCover)
		{
			LastLineOfSightIgnoringPlantCoverTime = currentTime;
		}
	}

	public void MarkInaccessible()
	{
		SetFlag(TargetFlags.Inaccessible, on: true);
		LastInaccessibleTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
	}

	public void ClearInaccessible()
	{
		SetFlag(TargetFlags.Inaccessible, on: false);
	}

	public bool IsInaccessible()
	{
		if (GetFlag(TargetFlags.Inaccessible))
		{
			return LastInaccessibleTime > GameTerrain.Instance.LastChangedTime;
		}
		return false;
	}

	public void ForgetAboutMe()
	{
		SetFlag(TargetFlags.Unconscious, on: true);
		Camouflage = 1f;
		LoseTargetTimer = TimeSpan.Zero;
	}

	public void Update(Character character)
	{
		Character character2 = Object as Character;
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(Predicted);
		TimeSpan timeSpan = ((LastUpdatedTime >= TimeSpan.Zero) ? MathUtil.Min(currentTime - LastUpdatedTime, TimeSpan.FromSeconds(0.10000000149011612)) : TimeSpan.Zero);
		LastUpdatedTime = currentTime;
		LoseTargetTimer = MathUtil.Max(LoseTargetTimer - timeSpan, TimeSpan.Zero);
		if (!Visible)
		{
			LastVisibility = 0f;
		}
		if (LoseTargetTimer > TimeSpan.Zero)
		{
			Camouflage = 0f;
		}
		else if (character2 != null && character2.IsHiding())
		{
			float num = (float)character2.GetSkillLevelWithEffects(SkillType.Stealth) / 5f;
			float num2 = (character2.IsOutdoors() ? MathUtil.Squared(Mathf.Clamp01(MathUtil.ToXZ(character2.Position - character.Position).magnitude / (float)character.GetSightRange())) : 1f);
			float num3 = 0f;
			if (LastVisibility < 0.5f)
			{
				float num4 = Mathf.Clamp01((0.5f - LastVisibility) * 2f);
				float num5 = Mathf.Lerp(SkillFactorInOpen, SkillFactorInCover, num4);
				num3 = MaxGainCamoSpeed * num2 * (1f - num5 + num * num5) * num4;
			}
			else
			{
				float num6 = Mathf.Clamp01((LastVisibility - 0.5f) * 2f);
				float num7 = Mathf.Lerp(SkillFactorInCover, SkillFactorInOpen, num6);
				num3 = (0f - MaxLoseCamoSpeed) * (1f - num2) * (1f - num * num7) * num6;
			}
			Camouflage = Mathf.Clamp01(Camouflage + num3 * (float)timeSpan.TotalSeconds);
		}
		else if (LastVisibility > 0f || character2 == null)
		{
			Camouflage = 0f;
		}
		else
		{
			float num8 = (character2.IsOutdoors() ? Mathf.Clamp01(MathUtil.ToXZ(character2.Position - character.Position).magnitude / (float)character.GetSightRange()) : 1f);
			Camouflage = Mathf.Clamp01(Camouflage + MaxGainCamoSpeed * (1f - SkillFactorInCover) * num8 * (float)timeSpan.TotalSeconds);
		}
		if (Camouflage < 1f)
		{
			if (Camouflage >= 0.5f && TimeSinceLastHeard < DistractionTime)
			{
				UpdateLastKnownInfo(character, LastKnownPosition);
			}
			else
			{
				UpdateLastKnownInfo(character);
			}
		}
		if (Camouflage == 0f)
		{
			if (Visible)
			{
				LoseTargetTimer = LoseTargetTime;
			}
			LastFullyTrackedTime = LastUpdatedTime;
			if (LastAttackedUsTime != Never)
			{
				SetFlag(TargetFlags.KnownAssailant, on: true);
			}
			SetFlag(TargetFlags.HasLoadedRangedWeapon, character2?.HasEquippedRangedWeaponAndAmmo(GetFlag(TargetFlags.HasLoadedRangedWeapon)) ?? false);
			SetFlag(TargetFlags.Indoors, character2 != null && !character2.IsOutdoors());
		}
		bool flag = FullyTracked || (Visible && Camouflage < AlertCamouflageThreshold);
		if (flag && character2 != null && character2.GetOldVelocity().sqrMagnitude > 0f)
		{
			Movement = Math.Min(1f, Movement + (float)timeSpan.TotalSeconds * 2f);
		}
		else
		{
			Movement = Math.Max(0f, Movement - (float)timeSpan.TotalSeconds);
		}
		if (character.GetBaseObjectType() == BaseObjectType.Human || character2 == null || character2.GetBaseObjectType() != BaseObjectType.Human)
		{
			return;
		}
		if (flag || TimeSinceLastSmelled < TimeSpan.FromSeconds(1.0))
		{
			SetFlag(TargetFlags.HasCarrot, (character2.EquippedItem != null && character.LikesFood(character2.EquippedItem.GetPrototype())) || (character2.DesiredEquippedItem != null && character.LikesFood(character2.DesiredEquippedItem.GetPrototype())));
		}
		if (GetFlag(TargetFlags.AlarmedBy))
		{
			if (TimeSinceLastDetected >= TimeSpan.FromSeconds(8.0) && TimeSinceLastHeardAttack >= TimeSpan.FromSeconds(30.0))
			{
				SetFlag(TargetFlags.AlarmedBy, on: false);
			}
		}
		else if (!GetFlag(TargetFlags.Dead))
		{
			if (flag && (!GetFlag(TargetFlags.Crouching) || (GetFlag(TargetFlags.Aiming) && !GetFlag(TargetFlags.HasCarrot)) || (Movement >= 1f && character2 != null && character2.DirectControlled)))
			{
				SetFlag(TargetFlags.AlarmedBy, on: true);
				SetFlag(TargetFlags.TamedBy, on: false);
			}
			else if (TimeSinceLastHeardAttack < TimeSpan.FromSeconds(10.0))
			{
				SetFlag(TargetFlags.AlarmedBy, on: true);
				SetFlag(TargetFlags.TamedBy, on: false);
			}
		}
		if (GetFlag(TargetFlags.TamedBy) && TimeSinceLastDetected >= TimeSpan.FromSeconds(8.0))
		{
			SetFlag(TargetFlags.TamedBy, on: false);
		}
	}

	public void UpdateLastKnownInfo(Character character)
	{
		UpdateLastKnownInfo(character, (Object != null) ? Object.GetBoundingBoxBottom() : LastKnownPosition);
	}

	public void UpdateLastKnownInfo(Character character, Vector3 pos)
	{
		Character character2 = Object as Character;
		if (character2 != null && character2.Disappeared)
		{
			LastKnownPosition = pos;
			SetFlag(TargetFlags.Lost, on: true);
			return;
		}
		if (Object != null)
		{
			LastKnownPosition = pos;
		}
		_ = _lastKnownFlags;
		bool flag = ((character2 != null) ? (!character2.Alive) : (Object == null || Object.IsDestroyedNotIncludingDeadCrops()));
		EnterableVehicle enterableVehicle = ((character2 != null) ? (character2.InsideBuilding as EnterableVehicle) : null);
		SetFlag(TargetFlags.Crouching, character2?.IsCrouching() ?? false);
		SetFlag(TargetFlags.Aiming, character2?.IsAiming() ?? false);
		SetFlag(TargetFlags.Dead, flag);
		SetFlag(TargetFlags.DeadOrZombie, (character2 != null) ? (!character2.AliveAndNotZombie) : (Object == null || Object.IsDestroyedNotIncludingDeadCrops()));
		SetFlag(TargetFlags.KnockedOut, character2 != null && character2.Consciousness == Consciousness.Unconscious && character2.SedativeEffect > 0f);
		SetFlag(TargetFlags.Unconscious, character2 != null && character2.Consciousness == Consciousness.Unconscious);
		SetFlag(TargetFlags.InMovingVehicle, enterableVehicle?.IsMoving ?? false);
		SetFlag(TargetFlags.Friend, Object.GetCommunity() == character.Community && character.Community != null);
		SetFlag((TargetFlags)516, on: false);
		SetFlag(TargetFlags.CarryingFriend, IsCarryingStolenFriend(character, character2));
		if (GetFlag(TargetFlags.OutsideBase))
		{
			TerrainCoord tileCoordForPos = GameTerrain.Instance.GetTileCoordForPos(LastKnownPosition);
			if (GameTerrain.Instance.IsTileEnclosed(tileCoordForPos.x, tileCoordForPos.y) || (LastKnownPosition - character.Pos).magnitude <= 1.5f)
			{
				SetFlag(TargetFlags.OutsideBase, on: false);
			}
		}
		if (character2 != null && character2.AliveAndNotZombie && character2.Consciousness != Consciousness.Unconscious)
		{
			SetFlag((TargetFlags)384, on: false);
		}
		if (flag && character.Community != null && character2 != null)
		{
			character.Community.ProcessPendingDeathNotificationsForMember(character, character2);
		}
	}

	private bool IsCarryingStolenFriend(Character character, Character targetCharacter)
	{
		if (targetCharacter == null)
		{
			return false;
		}
		if (targetCharacter.CarryingObject == null)
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (targetCharacter.CarryingObject.GetCommunity() == character.Community)
		{
			return true;
		}
		if (targetCharacter.CarryingObject is Animal { Stolen: not false } animal && animal.InitialCommunity == character.Community && character.Community.BaseRect != TerrainRect.Invalid && character.Community.BaseRect.Expand(StolenAnimalOutsideBaseRange).Contains(targetCharacter.Tile))
		{
			return true;
		}
		return false;
	}

	public void SetFlag(TargetFlags flag, bool on)
	{
		if (on)
		{
			_lastKnownFlags |= (int)flag;
		}
		else
		{
			_lastKnownFlags &= (int)(~flag);
		}
	}

	public bool GetFlag(TargetFlags flag)
	{
		return ((uint)_lastKnownFlags & (uint)flag) == (uint)flag;
	}

	public bool HasAnyFlag(TargetFlags flags)
	{
		return ((uint)_lastKnownFlags & (uint)flags) != 0;
	}

	public Texture2D GetAlertIcon()
	{
		Texture2D result = ((Camouflage < AlertCamouflageThreshold) ? GameCursor.EyeIcon : (CanSmellTarget ? GameCursor.NoseIcon : GameCursor.AlertIcon));
		if (TimeSinceLastHeard <= TimeSpan.FromSeconds(2.0))
		{
			result = GameCursor.EarIcon;
		}
		return result;
	}

	public bool AllowDeletion()
	{
		if (Object == null || Object.Deleted)
		{
			return true;
		}
		if (TimeSinceLastDetected.TotalSeconds <= (double)((LastAttackedUsTime == Never) ? 10f : AlertGoal.HiddenAttackerAlertTime))
		{
			return false;
		}
		if (HasLineOfSightIgnoringPlantCover)
		{
			return false;
		}
		Character character = Object as Character;
		if (character != null && Camouflage < 1f)
		{
			return false;
		}
		if (HasAnyFlag((TargetFlags)1920) && character != null && !character.Disappeared)
		{
			return false;
		}
		return true;
	}
}
