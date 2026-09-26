using System;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : StateMachineGoal
{
	public RangedWeapon CurrentWeapon;

	public EquipmentPrototype CurrentAmmoType;

	public InfectionType CurrentAmmoInfectedWith;

	private TargettableBodyLocation CurrentAimingStance;

	private TimeSpan ChangeAimingStanceCountdown;

	private TimeSpan _moveFailedTime;

	private TimeSpan _moveToBuildingFailedTime;

	private TimeSpan _takeCoverFailedTime;

	private TimeSpan _attackDefencesFailedTime;

	private TimeSpan StayInRangeFailedTime = Target.Never;

	public int UnsuccessfulShotCount;

	private int UnsuccessfulShotLimit;

	private bool LastMoveFailed;

	private bool IsInCover;

	private bool IsInCrouchingCover;

	private bool Assassinate;

	private bool WantCheckCurrentWeapon;

	private SecrecyMode _secret;

	public bool _dontOpenOurGates = true;

	public StayInRangeParams StayInRangeParams = StayInRangeParams.OfSquadLeader();

	private static float PreferredRange = 16f;

	public static float WatchTowerOffsetHack = 0.5f;

	private static bool DrawCanAttackRaycasts = false;

	private static List<Character> TempCharacters = new List<Character>();

	private const float MinZombieRangeInner = 2f;

	private const float MinZombieRangeOuter = 4f;

	public const float MinTimeBetweenFailedMoveRequests = 3f;

	public const float MinTimeBetweenFailedMoveToBuildingRequests = 10f;

	public const float MinTimeBetweenFailedTakeCoverRequests = 3f;

	public const float MinTimeBetweenFailedAttackDefences = 4f;

	public const float Timeout = 3f;

	public static TimeSpan MaxTimeSinceTargetVisibleForTakeCover = TimeSpan.FromSeconds(10.0);

	public static TimeSpan MaxTimeSinceTargetAttackedUsForTakeCover = TimeSpan.FromSeconds(30.0);

	private static float BuildingAccurateRangeLerp = 0.25f;

	private static List<Building> _buildingsToShootFrom = new List<Building>();

	private static List<int> _buildingsToShootFromSlotIndex = new List<int>();

	private MovementType _movementTypeWhenNotInDanger = MovementType.Jog;

	public RangedAttack()
	{
	}

	public RangedAttack(MovementType movementTypeWhenNotInDanger, bool assassinate, SecrecyMode secret, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		_movementTypeWhenNotInDanger = movementTypeWhenNotInDanger;
		Assassinate = assassinate;
		_secret = secret;
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public RangedAttack(Character character, TileObject targetObj, MovementType movementTypeWhenNotInDanger, StayInRangeParams stayInRangeParams)
	{
		_movementTypeWhenNotInDanger = movementTypeWhenNotInDanger;
		StayInRangeParams = stayInRangeParams;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref CurrentWeapon);
		reflector.AddAfter(ref CurrentAmmoType, 432);
		reflector.AddAfter(ref CurrentAmmoInfectedWith, 450);
		reflector.AddAfter(ref CurrentAimingStance, 520);
		reflector.AddAfter(ref ChangeAimingStanceCountdown, 520);
		reflector.Add(ref _movementTypeWhenNotInDanger);
		reflector.Add(ref _moveFailedTime);
		reflector.Add(ref _moveToBuildingFailedTime);
		reflector.Add(ref _takeCoverFailedTime);
		reflector.Add(ref _attackDefencesFailedTime);
		reflector.AddAfter(ref StayInRangeFailedTime, 420);
		reflector.AddAfter(ref UnsuccessfulShotCount, 61);
		reflector.AddAfter(ref LastMoveFailed, 171);
		reflector.AddAfter(ref IsInCover, 60);
		reflector.AddAfter(ref IsInCrouchingCover, 60);
		reflector.Add(ref Assassinate);
		reflector.AddAfter(ref WantCheckCurrentWeapon, 434);
		reflector.Add(ref _secret);
		reflector.Add(ref _dontOpenOurGates);
		StayInRangeParams.Reflect(reflector);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.RangedAttack;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if (!Assassinate || character.IsBeingBitten())
		{
			return AIOverridesControlReason.None;
		}
		return AIOverridesControlReason.Scripted;
	}

	public override TargettableBodyLocation GetCurrentTargetBodyLocation(Character character)
	{
		return CurrentAimingStance;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && character.IsCurrentActionAnimFromGoal && !character.IsActionAnimFinished())
		{
			return true;
		}
		if (IsTargetDeleted() || Target.GetFlag(TargetFlags.Lost))
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !targetCharacter.IsOutdoors() && !Assassinate)
		{
			return false;
		}
		if (targetCharacter != null && targetCharacter == character.SparringPartner && character.SparringType != SparringType.SnowballFight)
		{
			return false;
		}
		if (character.Rank == Rank.Captive && targetCharacter != null && targetCharacter.IsControllableByPlayer() && Target.LastAttackedMeTime == Target.Never)
		{
			return false;
		}
		if (GetBestRangedWeapon(character, parent, out var _, out var _) == null && !IsInSnowballFight(character))
		{
			return false;
		}
		if (parent is Attack attack && !attack.WantAttackTarget(character, melee: false))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && character.IsCurrentActionAnimFromGoal && !character.IsActionAnimFinished())
		{
			return GoalPriority.Attack_Animation;
		}
		bool flag = MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).sqrMagnitude >= PreferredRange * PreferredRange;
		TerrainCoord tile = character.Tile;
		if (GameTerrain.Instance.IsTileEnclosed(tile.x, tile.y) != GameTerrain.Instance.IsTileEnclosed(Target.Object.GetTileX(), Target.Object.GetTileY()))
		{
			flag = true;
		}
		if (Target.Object is Character)
		{
			EquipmentPrototype bestAmmoType;
			InfectionType bestInfectedWith;
			RangedWeapon bestRangedWeapon = GetBestRangedWeapon(character, parent, out bestAmmoType, out bestInfectedWith);
			if (bestRangedWeapon is Throwable)
			{
				flag = false;
			}
			if (bestRangedWeapon is Gun)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return GoalPriority.Attack_RangedAttack;
		}
		return GoalPriority.Attack_RangedAttack_Preferred;
	}

	private bool CanAttack(Character character)
	{
		if (Assassinate)
		{
			return true;
		}
		return CanAttack(character, Target, SubGoal != null && SubGoal.Crouching && !character.IsGuarding(), LastMoveFailed);
	}

	public static bool CanAttack(Character character, Target target, bool isCrouching, bool canDestroyNearbyBuildings)
	{
		if (character.EquippedItem == null)
		{
			return false;
		}
		if (!(character.EquippedItem is RangedWeapon rangedWeapon))
		{
			return false;
		}
		if (character.EquippedItem is AmmoWeapon { CurrentAmmo: 0 })
		{
			return false;
		}
		if (target == null)
		{
			return false;
		}
		if (target.Camouflage >= 1f)
		{
			return false;
		}
		if (!target.HasLineOfSightIgnoringPlantCover)
		{
			return false;
		}
		float accurateRange;
		float num = rangedWeapon.GetRangeIncludingEffects(character, target.Object, out accurateRange);
		if (character.InsideBuilding != null && !character.HasInfiniteAmmo(rangedWeapon))
		{
			num = Mathf.Lerp(accurateRange, num, BuildingAccurateRangeLerp);
		}
		if (character.Get2DDistSqToTargetAimPos(target, Vector3.zero) > num * num)
		{
			return false;
		}
		Character character2 = target.Object as Character;
		Vector3 targetAimPos = character.GetTargetAimPos(target, TargettableBodyLocation.Torso, Vector3.zero, deterministic: true);
		Vector3 vector = character.Position + new Vector3(0f, isCrouching ? character.Appearance.CrouchingGunHeight : character.Appearance.GunHeight, 0f);
		float magnitude = (targetAimPos - vector).magnitude;
		Vector3 direction = ((magnitude > 0.001f) ? ((targetAimPos - vector) / magnitude) : character.Forward);
		if (character.InsideBuilding != null && !character.InsideBuilding.GetInhabitantSlotDef(character).IsTargetInAngleRange(character.InsideBuilding, MathUtil.ToXZ(targetAimPos)))
		{
			return false;
		}
		if (character.InsideBuilding != null && character.InsideBuilding.IsGuardPost())
		{
			vector += character.Forward * WatchTowerOffsetHack;
		}
		if (character.IsAuthoritative())
		{
			CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		}
		int num2 = 40;
		if (character.EquippedItem is Throwable || character.EquippedItem is RPG)
		{
			num2 |= 0x1000;
		}
		if (character.EquippedItem is Throwable)
		{
			return true;
		}
		RaycastResult raycastResult = GameTerrain.Instance.RayCast(new Ray(vector, direction), magnitude, num2, character.InsideBuilding, character, target.Object, predicted: false);
		if (DrawCanAttackRaycasts)
		{
			DebugGraphics.AddPersistentLine(vector, raycastResult.GetHitPosition(), Color.yellow);
		}
		if (raycastResult.HitObject == target.Object || raycastResult.HitObject == null || (character2 != null && raycastResult.HitObject == character2.InsideBuilding))
		{
			return true;
		}
		TileObject tileObject = raycastResult.HitObject as TileObject;
		if (canDestroyNearbyBuildings && tileObject != null && tileObject.GetCommunityId() != character.GetCommunityId() && !tileObject.IsFriendlyFire(character, target.Object, itsATrap: false) && ((character.EquippedItem is MolotovCocktail && tileObject.IsFlammable()) || (character.EquippedItem is PipeBomb && !tileObject.IsExplosionProof())))
		{
			return true;
		}
		return false;
	}

	public static bool IsAttackBlockedByAllies(Character character, Target target, bool assassinate)
	{
		if (assassinate)
		{
			return false;
		}
		float num = ((character.EquippedItem != null) ? character.EquippedItem.GetPrototype().DamageRadius : 0f);
		if (num > 0f && character.EquippedItem is Throwable && Session.Instance.DifficultySettings.FriendlyFireSplashDamage > 0f)
		{
			num += 2f;
			GameTerrain instance = GameTerrain.Instance;
			Vector3 targetAimPos = character.GetTargetAimPos(target, TargettableBodyLocation.Torso, Vector3.zero, deterministic: true);
			TerrainCoord tileCoordForPos = instance.GetTileCoordForPos(targetAimPos);
			int num2 = Mathf.CeilToInt(num);
			instance.CharacterMapWho.GetObjectsInRect(tileCoordForPos - new TerrainCoord(num2, num2), tileCoordForPos + new TerrainCoord(num2, num2), TempCharacters);
			foreach (Character tempCharacter in TempCharacters)
			{
				if (!tempCharacter.AliveAndNotZombie || !((tempCharacter.Pos - targetAimPos).magnitude < num) || (tempCharacter.CurrentAnimState == AnimState.Animation && tempCharacter.GetCurrentActionPriority() <= ActionPriority.Dodge && !tempCharacter.DirectControlled))
				{
					continue;
				}
				if (character.IsLooter())
				{
					if (tempCharacter.Community == character.Community)
					{
						return true;
					}
				}
				else if (!character.IsEnemy(tempCharacter))
				{
					return true;
				}
			}
		}
		return false;
	}

	private RangedWeapon GetBestRangedWeapon(Character character, Goal parent, out EquipmentPrototype bestAmmoType, out InfectionType bestInfectedWith)
	{
		if (IsInSnowballFight(character))
		{
			bestAmmoType = null;
			bestInfectedWith = InfectionType.None;
			return character.Inventory.FindItemOfType(EquipmentPrototype.Snowball) as RangedWeapon;
		}
		if (Assassinate)
		{
			if (character.EquippedItem is Gun gun && (character.HasInfiniteAmmo(gun) || gun.CurrentAmmo > 0 || character.Inventory.HasAmmoForWeapon(gun)))
			{
				bestAmmoType = gun.CurrentAmmoType;
				bestInfectedWith = gun.InfectedWith;
				return gun;
			}
			foreach (Equipment content in character.Inventory.Contents)
			{
				if (content is Gun gun2 && (character.HasInfiniteAmmo(gun2) || gun2.CurrentAmmo > 0))
				{
					bestAmmoType = gun2.CurrentAmmoType;
					bestInfectedWith = gun2.InfectedWith;
					return gun2;
				}
			}
			foreach (Equipment content2 in character.Inventory.Contents)
			{
				if (content2 is Gun gun3 && character.Inventory.HasAmmoForWeapon(gun3))
				{
					bestAmmoType = gun3.CurrentAmmoType;
					bestInfectedWith = gun3.InfectedWith;
					return gun3;
				}
			}
		}
		return character.Inventory.GetBestWeapon(character, (Target != null) ? Target.Object : null, wantRanged: true, bluntOnly: false, out bestAmmoType, out bestInfectedWith) as RangedWeapon;
	}

	private float GetMinRangeInner()
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || !targetCharacter.Zombie || !targetCharacter.ShouldLimp())
		{
			return 0f;
		}
		return 2f;
	}

	private float GetMinRangeOuter()
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || !targetCharacter.Zombie || !targetCharacter.ShouldLimp())
		{
			return 0f;
		}
		return 4f;
	}

	private float GetMaxRange(Character character)
	{
		if (Assassinate)
		{
			return 10000f;
		}
		float accurateRange;
		float rangeIncludingEffects = CurrentWeapon.GetRangeIncludingEffects(character, GetTargetObject(), out accurateRange);
		float num = 20f + (float)character.GetPersonality(CachedPersonalityType.Bold, CachedPersonalityType.Nervous) * 10f;
		return Mathf.Lerp(t: Mathf.Clamp01((float)Target.TimeSinceLastAttackedMe.TotalSeconds / num) * 0.5f, a: rangeIncludingEffects, b: accurateRange);
	}

	public bool ShouldTakeCover(Character character, Goal parent)
	{
		if (character.InsideBuilding != null)
		{
			return false;
		}
		if (Assassinate)
		{
			return false;
		}
		if (_takeCoverFailedTime.Ticks != 0L && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _takeCoverFailedTime < TimeSpan.FromSeconds(3.0))
		{
			return false;
		}
		if (Target.TimeSinceLastVisible >= MaxTimeSinceTargetVisibleForTakeCover)
		{
			return false;
		}
		if (Target.TimeSinceLastAttackedUs >= MaxTimeSinceTargetAttackedUsForTakeCover)
		{
			return false;
		}
		if (TargetHasSurrenderred(character))
		{
			return false;
		}
		if (parent is AttackDefences)
		{
			return false;
		}
		return Target.GetFlag(TargetFlags.HasLoadedRangedWeapon);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		WantCheckCurrentWeapon = false;
		CurrentWeapon = GetBestRangedWeapon(character, parent, out CurrentAmmoType, out CurrentAmmoInfectedWith);
		AmmoWeapon ammoWeapon = CurrentWeapon as AmmoWeapon;
		base.OnActivate(character, parent);
		Character targetCharacter = GetTargetCharacter();
		IsInCover = TakeCoverGoal.CheckCover(character, Target, out IsInCrouchingCover);
		PickUnsuccessfulShotLimit(character);
		if (!character.IsOutdoors())
		{
			SetSubGoal(character, parent, new LeaveBuilding());
		}
		else if (character.EquippedItem != CurrentWeapon)
		{
			SetSubGoal(character, parent, new Equip(CurrentWeapon, IsInCrouchingCover));
		}
		else
		{
			character.DesiredEquippedItem = character.EquippedItem;
			int slotIndex;
			if (ShouldTakeCover(character, parent))
			{
				SetSubGoal(character, parent, new TakeCoverGoal(_dontOpenOurGates, StayInRangeParams));
			}
			else if (CurrentWeapon != null && CurrentAmmoType != null && CurrentWeapon.GetCurrentAmmoType() != CurrentAmmoType && character.Inventory.HasAmmoOfType(CurrentAmmoType, CurrentAmmoInfectedWith))
			{
				SetSubGoal(character, parent, new ReloadAnim(aiming: true, IsInCrouchingCover, CurrentAmmoType, CurrentAmmoInfectedWith));
			}
			else if (ammoWeapon != null && ammoWeapon.CurrentAmmo == 0)
			{
				if (!character.HasInfiniteAmmo(ammoWeapon) && (CurrentAmmoType != null || !character.Inventory.HasAmmoForWeapon(ammoWeapon, ammoWeapon)) && (CurrentAmmoType == null || !character.Inventory.HasAmmoOfType(CurrentAmmoType, CurrentAmmoInfectedWith)))
				{
					Finished = true;
					return;
				}
				if (character.IsAuthoritative())
				{
					ammoWeapon.CurrentAmmoType = CurrentAmmoType;
					ammoWeapon.InfectedWith = CurrentAmmoInfectedWith;
				}
				SetSubGoal(character, parent, new ReloadAnim(aiming: true, IsInCrouchingCover));
			}
			else if (character.EquippedItem == null && IsInSnowballFight(character))
			{
				Goal scoopSnowballGoal = GetScoopSnowballGoal(character);
				if (scoopSnowballGoal != null)
				{
					SetSubGoal(character, parent, scoopSnowballGoal);
				}
				else
				{
					SetSubGoal(character, parent, GetWaitAndFaceGoal(character));
				}
			}
			else if (CanAttack(character))
			{
				if (TargetHasSurrenderred(character) || IsAttackBlockedByAllies(character, Target, Assassinate))
				{
					SetSubGoal(character, parent, GetWaitAndFaceGoal(character));
				}
				else
				{
					SetSubGoal(character, parent, new AimAndAttack(character.WasCrouching && !character.IsGuarding(), Assassinate, _secret));
				}
			}
			else if (CanSwitchSlotInBuilding(character, out slotIndex))
			{
				character.InsideBuilding.OnCharacterSlotChange(character, slotIndex);
				SetSubGoal(character, parent, new WaitAndFaceTarget(TimeSpan.FromSeconds(0.5), aiming: true, crouching: false));
			}
			else if (CanMoveWithinRange(character))
			{
				Building building = FindBuildingToShootFrom(character, fallback: false);
				if (building != null)
				{
					SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, MovementType.Run, _dontOpenOurGates));
				}
				PickUnsuccessfulShotLimit(character);
				IsInCover = (IsInCrouchingCover = false);
				SetSubGoal(character, parent, GetMoveWithinRangeGoal(character, _movementTypeWhenNotInDanger, aiming: false, LastMoveFailed ? 0.5f : 0f));
			}
			else
			{
				SetSubGoal(character, parent, GetWaitAndFaceGoal(character));
			}
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (character.IsAuthoritative() && character.Speaking == null && !Assassinate && targetCharacter != null && !character.IsTargetSurrendering(targetCharacter) && MathUtil.RandomChoice((float)currentTime.TotalSeconds, 0.25f + (float)character.GetPersonality(CachedPersonalityType.Emotional, CachedPersonalityType.Unflappable) * 0.25f))
		{
			character.Speak(StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Attacking));
		}
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		PickNewAimingStance(character);
	}

	protected override void OnDeactivateTarget(Character character, Goal parent)
	{
		IsInCover = (IsInCrouchingCover = false);
		base.OnDeactivateTarget(character, parent);
	}

	public override void OnEquipmentPolicyChanged(Character character, Goal parent)
	{
		WantCheckCurrentWeapon = true;
	}

	private void PickUnsuccessfulShotLimit(Character character)
	{
		int num = ((CurrentWeapon is AmmoWeapon ammoWeapon) ? ammoWeapon.GetMaxAmmo() : 8);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		UnsuccessfulShotLimit = Math.Max(1, MathUtil.RandomInt(character.Id * 1000 + (int)currentTime.Ticks, num / 2, num * 2));
		UnsuccessfulShotCount = 0;
	}

	private bool IsInSnowballFight(Character character)
	{
		if (character.SparringType == SparringType.SnowballFight && character.SparringPartner != null)
		{
			return GetTargetObject() == character.SparringPartner;
		}
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return null;
		}
		if (SubGoal is AimAndAttack && CurrentAimingStance == TargettableBodyLocation.Legs)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null && targetCharacter.Zombie && targetCharacter.ShouldLimp())
			{
				PickNewAimingStance(character);
			}
		}
		if (SubGoal is Equip)
		{
			PickNewAimingStance(character);
		}
		if (SubGoal is FindGoal { Success: not false, FindType: FindType.Bandage })
		{
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel();
			if (equipment != null && character.HasUnbandagedInjury(equipment.GetBandageLevel()))
			{
				return new BandageSelfAnim(equipment);
			}
		}
		if (SubGoal is MoveWithinRange { Success: false })
		{
			StayInRangeFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (SubGoal is MoveWithinRangeOfSquadLeader { Success: false })
		{
			StayInRangeFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (SubGoal is MoveToAndEnterBuilding { Success: false })
		{
			_moveToBuildingFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			StayInRangeFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		bool flag = false;
		MoveWithinRangeOfTarget moveWithinRangeOfTarget = SubGoal as MoveWithinRangeOfTarget;
		if (moveWithinRangeOfTarget != null)
		{
			if (moveWithinRangeOfTarget.Success)
			{
				LastMoveFailed = false;
				if (!TargetHasSurrenderred(character) && CanAttack(character) && !IsAttackBlockedByAllies(character, Target, Assassinate))
				{
					return new AimAndAttack(character.WasCrouching && !character.IsGuarding(), Assassinate, _secret);
				}
			}
			else
			{
				_moveFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
				flag = !LastMoveFailed;
				LastMoveFailed = true;
			}
		}
		if (SubGoal is TakeCoverAndReload takeCoverAndReload)
		{
			PickNewAimingStance(character);
			if (!takeCoverAndReload.Success)
			{
				IsInCover = takeCoverAndReload.Success;
				IsInCrouchingCover = takeCoverAndReload.Crouching;
				_takeCoverFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			}
		}
		TakeCoverGoal takeCoverGoal = SubGoal as TakeCoverGoal;
		if (takeCoverGoal != null)
		{
			PickNewAimingStance(character);
			if (!takeCoverGoal.Success)
			{
				IsInCover = takeCoverGoal.Success;
				IsInCrouchingCover = takeCoverGoal.Crouching;
				_takeCoverFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			}
		}
		if (SubGoal is AttackDefences { Success: false })
		{
			_attackDefencesFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (WantCheckCurrentWeapon || IsInSnowballFight(character) || CurrentWeapon == null || CurrentWeapon is MolotovCocktail || !(Target.Object is Character))
		{
			WantCheckCurrentWeapon = false;
			CurrentWeapon = GetBestRangedWeapon(character, parent, out CurrentAmmoType, out CurrentAmmoInfectedWith);
			if (CurrentWeapon == null && !IsInSnowballFight(character))
			{
				return null;
			}
		}
		if (character.EquippedItem != CurrentWeapon)
		{
			if (!character.Inventory.Contains(CurrentWeapon))
			{
				return null;
			}
			return new Equip(CurrentWeapon);
		}
		if (CurrentWeapon != null && CurrentAmmoType != null && CurrentWeapon.GetCurrentAmmoType() != CurrentAmmoType && character.Inventory.HasAmmoOfType(CurrentAmmoType, CurrentAmmoInfectedWith))
		{
			return new ReloadAnim(aiming: true, IsInCrouchingCover, CurrentAmmoType, CurrentAmmoInfectedWith);
		}
		IsInCover = TakeCoverGoal.CheckCover(character, Target, out IsInCrouchingCover);
		if (!IsInCover && ShouldTakeCover(character, parent))
		{
			return new TakeCoverGoal(_dontOpenOurGates, StayInRangeParams);
		}
		AmmoWeapon ammoWeapon = CurrentWeapon as AmmoWeapon;
		if (ammoWeapon != null && ammoWeapon.CurrentAmmo == 0)
		{
			if (character.HasInfiniteAmmo(ammoWeapon) || (CurrentAmmoType == null && character.Inventory.HasAmmoForWeapon(ammoWeapon, ammoWeapon)) || (CurrentAmmoType != null && character.Inventory.HasAmmoOfType(CurrentAmmoType, CurrentAmmoInfectedWith)))
			{
				if (character.IsAuthoritative())
				{
					ammoWeapon.CurrentAmmoType = CurrentAmmoType;
					ammoWeapon.InfectedWith = CurrentAmmoInfectedWith;
				}
				if (!IsInCover && ShouldTakeCover(character, parent))
				{
					return new TakeCoverAndReload(_dontOpenOurGates, StayInRangeParams);
				}
				return new ReloadAnim(aiming: true, IsInCrouchingCover);
			}
			WantCheckCurrentWeapon = false;
			CurrentWeapon = GetBestRangedWeapon(character, parent, out CurrentAmmoType, out CurrentAmmoInfectedWith);
			if (CurrentWeapon != null)
			{
				return new Equip(CurrentWeapon);
			}
			return null;
		}
		if (character.EquippedItem == null && IsInSnowballFight(character))
		{
			Goal scoopSnowballGoal = GetScoopSnowballGoal(character);
			if (scoopSnowballGoal != null)
			{
				return scoopSnowballGoal;
			}
			return GetWaitAndFaceGoal(character);
		}
		if (CanAttack(character))
		{
			if (character.InsideBuilding == null && (UnsuccessfulShotCount >= UnsuccessfulShotLimit || character.Get2DDistSqToTargetAimPos(Target, Vector3.zero) > MathUtil.Squared(GetMaxRange(character))) && (_moveFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _moveFailedTime > TimeSpan.FromSeconds(3.0)))
			{
				PickUnsuccessfulShotLimit(character);
				IsInCover = (IsInCrouchingCover = false);
				return GetMoveWithinRangeGoal(character, MovementType.Jog, aiming: true, 0.5f);
			}
			if (TargetHasSurrenderred(character) || IsAttackBlockedByAllies(character, Target, Assassinate))
			{
				return GetWaitAndFaceGoal(character);
			}
			return new AimAndAttack(character.WasCrouching && !character.IsGuarding(), Assassinate, _secret);
		}
		if (flag)
		{
			LastMoveFailed = true;
			return GetMoveWithinRangeGoal(character, MovementType.Jog, aiming: true, 0.5f);
		}
		if (ammoWeapon != null && ammoWeapon.CurrentAmmo < ammoWeapon.GetMaxAmmo() && (character.HasInfiniteAmmo(ammoWeapon) || (CurrentAmmoType == null && character.Inventory.HasAmmoForWeapon(ammoWeapon, ammoWeapon)) || (CurrentAmmoType != null && character.Inventory.HasAmmoOfType(CurrentAmmoType, CurrentAmmoInfectedWith, ammoWeapon))))
		{
			if (IsInCover)
			{
				return new ReloadAnim(aiming: true, IsInCrouchingCover, CurrentAmmoType, CurrentAmmoInfectedWith);
			}
			if (ShouldTakeCover(character, parent))
			{
				return new TakeCoverAndReload(_dontOpenOurGates, StayInRangeParams, CurrentAmmoType, CurrentAmmoInfectedWith);
			}
		}
		if (SubGoal is WaitAndFaceTarget)
		{
			if (TargetHasSurrenderred(character))
			{
				return GetWaitAndFaceGoal(character);
			}
			if (Target.Camouflage >= 1f)
			{
				Target.SetFlag(TargetFlags.Lost, on: true);
				character.SetRecentActivity(RecentActivityType.HighAlert, GetTargetCharacter());
				return null;
			}
		}
		if (takeCoverGoal != null && takeCoverGoal.Success)
		{
			return GetWaitAndFaceGoal(character);
		}
		if (CanSwitchSlotInBuilding(character, out var slotIndex))
		{
			character.InsideBuilding.OnCharacterSlotChange(character, slotIndex);
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.5), aiming: true, crouching: false);
		}
		if (moveWithinRangeOfTarget == null && CanMoveWithinRange(character))
		{
			Building building = FindBuildingToShootFrom(character, fallback: false);
			if (building != null)
			{
				IsInCover = (IsInCrouchingCover = false);
				return new MoveToAndEnterBuilding(character, building, MovementType.Run, _dontOpenOurGates);
			}
			if (_moveFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _moveFailedTime > TimeSpan.FromSeconds(3.0))
			{
				PickUnsuccessfulShotLimit(character);
				IsInCover = (IsInCrouchingCover = false);
				bool flag2 = IsInDanger(character);
				MovementType movementType = (flag2 ? MovementType.Jog : _movementTypeWhenNotInDanger);
				return GetMoveWithinRangeGoal(character, movementType, flag2, LastMoveFailed ? 0.5f : 0f);
			}
		}
		if (character.InsideBuilding == null && !(parent is AttackDefences))
		{
			Building building2 = FindBuildingToShootFrom(character, fallback: true);
			if (building2 != null)
			{
				IsInCover = (IsInCrouchingCover = false);
				return new MoveToAndEnterBuilding(character, building2, MovementType.Run, _dontOpenOurGates);
			}
		}
		if ((_attackDefencesFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _attackDefencesFailedTime >= TimeSpan.FromSeconds(4.0)) && !(parent is AttackDefences) && !IsInSnowballFight(character))
		{
			bool num = character.HasMolotovCocktail();
			bool flag3 = character.HasExplosives();
			if (num || flag3)
			{
				IsInCover = (IsInCrouchingCover = false);
				return new AttackDefences(GetMinRangeInner(), GetMaxRange(character), _movementTypeWhenNotInDanger, StayInRangeParams);
			}
		}
		Target.MarkInaccessible();
		return GetWaitAndFaceGoal(character);
	}

	public static Goal GetBandageOrMedicateGoal(Character character, Target target)
	{
		bool flag = false;
		if (character.HasUnbandagedInjury(0))
		{
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel();
			if (equipment == null && character.InsideBuilding != null)
			{
				equipment = character.InsideBuilding.Inventory.FindItemWithHighestBandageLevel();
				if (equipment != null)
				{
					equipment = character.InsideBuilding.Inventory.Take(character.InsideBuilding, equipment, 1);
					equipment = character.Inventory.Add(character, equipment);
				}
			}
			if (equipment != null)
			{
				return new BandageSelfAnim(equipment);
			}
			flag = true;
		}
		InfectionType worstInfectionTypeInProgression = character.GetWorstInfectionTypeInProgression();
		if (worstInfectionTypeInProgression != InfectionType.None)
		{
			Equipment equipment2 = character.Inventory.FindAntigenForInfectionType(worstInfectionTypeInProgression);
			if (equipment2 == null && character.InsideBuilding != null)
			{
				equipment2 = character.InsideBuilding.Inventory.FindAntigenForInfectionType(worstInfectionTypeInProgression);
				if (equipment2 != null)
				{
					equipment2 = character.InsideBuilding.Inventory.Take(character.InsideBuilding, equipment2, 1);
					equipment2 = character.Inventory.Add(character, equipment2);
				}
			}
			if (equipment2 != null)
			{
				return new MedicateSelfAnim(equipment2);
			}
		}
		if (flag && target != null && character.IsAuthoritative() && (target.IsInaccessible() || character.IsTargetSurrendering(target.Object as Character)))
		{
			return new FindGoal(FindType.Bandage, MovementType.Run, critical: true);
		}
		return null;
	}

	public Goal GetWaitAndFaceGoal(Character character)
	{
		Goal bandageOrMedicateGoal = GetBandageOrMedicateGoal(character, Target);
		if (bandageOrMedicateGoal != null)
		{
			return bandageOrMedicateGoal;
		}
		Goal moveWithinRangeOfSquadLeaderGoal = Attack.GetMoveWithinRangeOfSquadLeaderGoal(character, _dontOpenOurGates, StayInRangeParams);
		if (moveWithinRangeOfSquadLeaderGoal != null)
		{
			return moveWithinRangeOfSquadLeaderGoal;
		}
		bool crouching = (IsInCrouchingCover || character.IsGuarding()) && !TargetHasSurrenderred(character);
		bool aiming = CurrentWeapon == null || CurrentWeapon.GetFatiguePenaltyWhenAiming() == 0f;
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(3.0), aiming, crouching);
	}

	public Goal GetScoopSnowballGoal(Character character)
	{
		GameTerrain instance = GameTerrain.Instance;
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		TerrainCoord tile = character.Tile;
		for (int i = 4; i < 12; i++)
		{
			TerrainCoord terrainCoord = tile + MathUtil.RandomTile((int)currentTime.Ticks + character.Id, new TerrainCoord(-i, -i), new TerrainCoord(i, i));
			if (!instance.RecentSnowScoops.Contains(terrainCoord) && !instance.IsTileOutsideBounds(terrainCoord.x, terrainCoord.y) && instance.IsTileEnclosed(terrainCoord) == instance.IsTileEnclosed(tile) && !instance.IsImpassable(terrainCoord.x, terrainCoord.y, 3, character, null))
			{
				return new MoveToAndScoopSnow(character, terrainCoord, MovementType.Run);
			}
		}
		return null;
	}

	public Goal GetMoveWithinRangeGoal(Character character, MovementType movementType, bool aiming, float minDistFromStart)
	{
		float minRangeOuter = GetMinRangeOuter();
		if (aiming && CurrentWeapon != null && CurrentWeapon.GetFatiguePenaltyWhenAiming() > 0f && MathUtil.ToXZ(character.Pos - Target.LastKnownPosition).sqrMagnitude > minRangeOuter * minRangeOuter)
		{
			aiming = false;
		}
		if (character.EquippedItem is Throwable)
		{
			return new MoveWithinRangeOfTarget(movementType, aiming, minRangeOuter, GetMaxRange(character), _dontOpenOurGates, StayInRangeParams, minDistFromStart);
		}
		return new MoveWithinRangeAndSightOfTarget(movementType, aiming, minRangeOuter, GetMaxRange(character), _dontOpenOurGates, StayInRangeParams, minDistFromStart);
	}

	public bool CanMoveWithinRange(Character character)
	{
		if (CurrentWeapon == null)
		{
			return false;
		}
		if (character.InsideBuilding == null)
		{
			return true;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return true;
		}
		if (character.OrderedToAttack && !CanShootFromBuilding(character, character.InsideBuilding, fallback: false, CalcWeaponRange(character, CurrentWeapon, targetCharacter), targetCharacter, out var _, out var _))
		{
			return true;
		}
		return false;
	}

	public bool CanSwitchSlotInBuilding(Character character, out int slotIndex)
	{
		if (character.InsideBuilding != null && character.IsAuthoritative() && CurrentWeapon != null)
		{
			TileObject targetObject = GetTargetObject();
			float weaponRange = CalcWeaponRange(character, CurrentWeapon, targetObject);
			float bestDistSqr;
			return CanShootFromBuilding(character, character.InsideBuilding, fallback: false, weaponRange, targetObject, out bestDistSqr, out slotIndex);
		}
		slotIndex = -1;
		return false;
	}

	public static float CalcWeaponRange(Character character, Equipment weapon, TileObject targetObj)
	{
		float accurateRange;
		float accurateRangeAtLowestSkill;
		float num = weapon.GetRangeIncludingEffects(character, targetObj, out accurateRange, includingBuildingEffects: false, out accurateRangeAtLowestSkill);
		if (!character.HasInfiniteAmmo(weapon))
		{
			num = Mathf.Lerp(accurateRange, num, BuildingAccurateRangeLerp);
		}
		return num;
	}

	public static bool CanKickInhabitantOutOfGuardPost(Character inhabitant)
	{
		if (inhabitant.AliveAndNotZombie && inhabitant.GetBaseObjectType() == BaseObjectType.Human)
		{
			return inhabitant.Inventory.IsOutOfAmmo(inhabitant);
		}
		return true;
	}

	public static bool CanShootFromBuilding(Character character, Building building, bool fallback, float weaponRange, TileObject targetObj, out float bestDistSqr, out int slotIndex)
	{
		bestDistSqr = float.MaxValue;
		slotIndex = -1;
		for (int i = 0; i < building.GetInhabitantSlotDefs().Length; i++)
		{
			if (building.GetInhabitantSlotDefs()[i].External && building.GetInhabitantSlotDefs()[i].IsTargetInAngleRange(building, targetObj.PosXZ) && (building.Inhabitants[i] == null || building.Inhabitants[i] == character || CanKickInhabitantOutOfGuardPost(building.Inhabitants[i])))
			{
				float num = Math.Max(building.GetInhabitantSlotDefs()[i].MinWeaponRange, weaponRange + (float)(fallback ? building.GetInhabitantSlotDefs()[i].SightRangeModifier : building.GetInhabitantSlotDefs()[i].WeaponRangeModifier));
				float distSquared = building.Tile.GetDistSquared(targetObj.GetTile());
				if (distSquared <= num * num && distSquared < bestDistSqr)
				{
					bestDistSqr = distSquared;
					slotIndex = i;
				}
			}
		}
		if (slotIndex != -1)
		{
			if (building == character.InsideBuilding)
			{
				return true;
			}
			if (building.CouldEnterIfNotFull(character))
			{
				return !character.Community.IsAnyMemberMovingToEnterBuilding(building);
			}
			return false;
		}
		return false;
	}

	public static void FindBuildingToShootFromInCommunity(Character character, Community community, bool fallback, float weaponRange, TileObject targetObj, TerrainCoord stayInRangeOfTile, float stayInRangeDist, ref float bestDistSqr, ref Building bestBuilding)
	{
		foreach (Prop building2 in community.Buildings)
		{
			if (!building2.IsGuardPost())
			{
				continue;
			}
			Building building = building2 as Building;
			if (building != character.InsideBuilding && CanShootFromBuilding(character, building, fallback, weaponRange, targetObj, out var bestDistSqr2, out var slotIndex) && !(building.Tile.GetDist(stayInRangeOfTile) > stayInRangeDist) && (!character.HasMovementZone() || character.MovementZone.Overlaps(building.GetTileRect())))
			{
				if (bestDistSqr2 < bestDistSqr)
				{
					bestDistSqr = bestDistSqr2;
					bestBuilding = building;
				}
				_buildingsToShootFrom.Add(building);
				_buildingsToShootFromSlotIndex.Add(slotIndex);
			}
		}
	}

	private Building FindBuildingToShootFrom(Character character, bool fallback)
	{
		return FindBuildingToShootFrom(character, GetTargetCharacter(), CurrentWeapon, fallback, _moveToBuildingFailedTime, StayInRangeParams);
	}

	public static Building FindBuildingToShootFrom(Character character, Character targetCharacter, Equipment weapon, bool fallback, TimeSpan moveToBuildingFailedTime, StayInRangeParams stayInRangeParams)
	{
		if (character.Community == null)
		{
			return null;
		}
		if (targetCharacter == null)
		{
			return null;
		}
		if (weapon == null)
		{
			return null;
		}
		if (character.SparringPartner == targetCharacter)
		{
			return null;
		}
		if (moveToBuildingFailedTime.Ticks != 0L && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - moveToBuildingFailedTime < TimeSpan.FromSeconds(10.0))
		{
			return null;
		}
		_buildingsToShootFrom.Clear();
		_buildingsToShootFromSlotIndex.Clear();
		float weaponRange = CalcWeaponRange(character, weapon, targetCharacter);
		stayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		float bestDistSqr = float.MaxValue;
		Building bestBuilding = null;
		FindBuildingToShootFromInCommunity(character, character.Community, fallback, weaponRange, targetCharacter, resultTile, resultDist, ref bestDistSqr, ref bestBuilding);
		foreach (Community cachedAlly in character.Community.CachedAllies)
		{
			FindBuildingToShootFromInCommunity(character, cachedAlly, fallback, weaponRange, targetCharacter, resultTile, resultDist, ref bestDistSqr, ref bestBuilding);
		}
		if (_buildingsToShootFrom.Count == 0)
		{
			return null;
		}
		if (moveToBuildingFailedTime.Ticks == 0L && bestBuilding != null)
		{
			_buildingsToShootFrom.Clear();
			_buildingsToShootFromSlotIndex.Clear();
			return bestBuilding;
		}
		int index = MathUtil.RandomInt((int)PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()).Ticks, _buildingsToShootFrom.Count);
		Building building = _buildingsToShootFrom[index];
		int num = _buildingsToShootFromSlotIndex[index];
		if (building != null && building.Inhabitants[num] != null && character.IsAuthoritative())
		{
			building.OnCharacterLeave(building.Inhabitants[num], 0, fromBuildingDestroyed: false, fromRagdolled: false);
		}
		_buildingsToShootFrom.Clear();
		_buildingsToShootFromSlotIndex.Clear();
		return building;
	}

	private bool IsInDanger(Character character)
	{
		float num = character.Get2DDistSqToTargetAimPos(Target, Vector3.zero);
		float maxRange = GetMaxRange(character);
		return num < maxRange * maxRange;
	}

	private bool TargetHasSurrenderred(Character character)
	{
		if (Assassinate)
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		return character.IsTargetSurrendering(targetCharacter);
	}

	public override void Update(Character character, Goal parent)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (currentTime - StayInRangeFailedTime >= TimeSpan.FromSeconds(5.0) && (!character.IsCurrentActionAnimFromGoal || character.IsActionAnimFinished()))
		{
			StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
			Building buildingSquadLeaderIsInOrWantsToBeIn = Attack.GetBuildingSquadLeaderIsInOrWantsToBeIn(character);
			if (((resultDist < float.MaxValue && character.Tile.GetDist(resultTile) > Math.Max(resultDist + Attack.FollowerHysteresisDist, Attack.MaxDistFromLeader)) || (StayInRangeParams.StayInRangeOf == StayInRangeOf.SquadLeader && character.SquadLeader != null && buildingSquadLeaderIsInOrWantsToBeIn != null && character.InsideBuilding == null)) && (!(SubGoal is MoveWithinRange) || ((MoveWithinRange)SubGoal).MovementType == MovementType.Jog) && (!(SubGoal is MoveWithinRangeOfSquadLeader) || ((MoveWithinRangeOfSquadLeader)SubGoal).MovementType == MovementType.Jog) && (!(SubGoal is MoveToAndEnterBuilding) || (character.SquadLeader != null && ((MoveToAndEnterBuilding)SubGoal).GetTargetBuilding() != buildingSquadLeaderIsInOrWantsToBeIn)))
			{
				Goal moveWithinRangeOfSquadLeaderGoal = Attack.GetMoveWithinRangeOfSquadLeaderGoal(character, _dontOpenOurGates, StayInRangeParams);
				if (moveWithinRangeOfSquadLeaderGoal != null)
				{
					SetSubGoal(character, parent, moveWithinRangeOfSquadLeaderGoal);
				}
			}
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !Finished && !Assassinate)
		{
			character.SetRecentActivity(RecentActivityType.Combat, targetCharacter);
		}
		if (!(SubGoal is MoveWithinRangeAndSightOfTarget) && character.InsideBuilding == null && CurrentWeapon != null)
		{
			float minRangeInner = GetMinRangeInner();
			if (minRangeInner > 0f && Target != null && (Target.LastKnownPosition - character.Position).sqrMagnitude < minRangeInner * minRangeInner)
			{
				Building building = FindBuildingToShootFrom(character, fallback: false);
				if (building != null)
				{
					SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, MovementType.Run, _dontOpenOurGates));
				}
				if (_moveFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _moveFailedTime > TimeSpan.FromSeconds(3.0))
				{
					PickUnsuccessfulShotLimit(character);
					IsInCover = (IsInCrouchingCover = false);
					SetSubGoal(character, parent, GetMoveWithinRangeGoal(character, MovementType.Jog, aiming: true, LastMoveFailed ? 0.5f : 0f));
				}
			}
		}
		if (SubGoal is WaitAndFaceTarget && !TargetHasSurrenderred(character) && CanAttack(character) && !IsAttackBlockedByAllies(character, Target, Assassinate))
		{
			SetSubGoal(character, parent, new AimAndAttack(character.WasCrouching && !character.IsGuarding(), Assassinate, _secret));
		}
		if (targetCharacter != null && Target.GetFlag(TargetFlags.OutsideBase) && SubGoal is MoveTo && !character.IsFinishedRoute() && !GameTerrain.Instance.IsTileEnclosedOrBuiltOn(character.GetRouteDestination()) && GameTerrain.Instance.IsTileEnclosedOrBuiltOn(character.Tile))
		{
			Target.MarkInaccessible();
			SetSubGoal(character, parent, GetWaitAndFaceGoal(character));
		}
		if (ChangeAimingStanceCountdown > TimeSpan.Zero)
		{
			TimeSpan timeSpan = currentTime - character.LastThinkTime;
			ChangeAimingStanceCountdown -= timeSpan;
			if (ChangeAimingStanceCountdown <= TimeSpan.Zero)
			{
				ChangeAimingStanceCountdown = TimeSpan.Zero;
			}
		}
		if (ChangeAimingStanceCountdown <= TimeSpan.Zero && (SubGoal is WaitAndFaceTarget || SubGoal is FlankTarget))
		{
			PickNewAimingStance(character);
		}
		if (Target != null && Target.Object != null && Target.Object.IsDestroyed() && (!character.IsFiring() || character.IsActionAnimAfterLastEventTime()))
		{
			Finished = true;
		}
		base.Update(character, parent);
	}

	private void PickNewAimingStance(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (CurrentWeapon == null)
		{
			return;
		}
		TargettableBodyLocation currentAimingStance = TargettableBodyLocation.Torso;
		if (targetCharacter == null)
		{
			currentAimingStance = TargettableBodyLocation.Torso;
		}
		else
		{
			float t = (float)character.GetSkillLevelWithEffects(CurrentWeapon.GetDamageSkillType()) / 5f;
			if (character.CanTargetBodyLocation(TargettableBodyLocation.Head, targetCharacter) && MathUtil.RandomChoice((float)(currentTime.Ticks + character.Id * 12) + 9877f, Mathf.Lerp(0.25f, 0.75f, t)) && !targetCharacter.IsArmoredOnBodyLocation(TargettableBodyLocation.Head))
			{
				currentAimingStance = TargettableBodyLocation.Head;
			}
			if (targetCharacter.Zombie)
			{
				if (!targetCharacter.ShouldLimp())
				{
					if (character.CanTargetBodyLocation(TargettableBodyLocation.Legs, targetCharacter) && MathUtil.RandomChoice((float)(currentTime.Ticks + character.Id * 12) + 9877f, Mathf.Lerp(0.25f, 0.75f, t)) && !targetCharacter.IsArmoredOnBodyLocation(TargettableBodyLocation.Legs))
					{
						currentAimingStance = TargettableBodyLocation.Legs;
					}
				}
				else if (targetCharacter.IsRagdollOrProneOrRecovering() && character.CanTargetBodyLocation(TargettableBodyLocation.Head, targetCharacter))
				{
					currentAimingStance = (TargettableBodyLocation)MathUtil.RandomInt((int)currentTime.Ticks, 2);
				}
			}
		}
		CurrentAimingStance = currentAimingStance;
		ChangeAimingStanceCountdown = TimeSpan.FromSeconds(Mathf.Lerp(2f, 10f, MathUtil.RandomFloat((float)currentTime.TotalSeconds)));
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Throw)
		{
			CurrentWeapon = null;
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		if (source == Target.Object)
		{
			IsInCover = false;
			IsInCrouchingCover = false;
			if ((SubGoal is AimAndAttack || SubGoal is MoveWithinRangeAndSightOfTarget || SubGoal is WaitAndFaceTarget) && ShouldTakeCover(character, parent))
			{
				SetSubGoal(character, parent, new TakeCoverGoal(_dontOpenOurGates, StayInRangeParams));
			}
		}
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
	}

	public void SetMovementTypeWhenNotInDanger(Character character, MovementType movementType)
	{
		_movementTypeWhenNotInDanger = movementType;
		if (Active && !IsInDanger(character) && SubGoal is MoveTo moveTo)
		{
			moveTo.SetMovementType(character, movementType);
		}
	}
}
