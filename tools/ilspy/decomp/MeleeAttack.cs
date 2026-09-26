using System;
using UnityEngine;

public class MeleeAttack : StateMachineGoal
{
	private MeleeWeapon CurrentWeapon;

	private MovementType _movementTypeWhenNotInDanger = MovementType.Jog;

	private TimeSpan _evasiveActionTime;

	private TimeSpan _strikeTime;

	private TimeSpan _attackDefencesFailedTime;

	private TimeSpan StayInRangeFailedTime = Target.Never;

	public bool _dontOpenOurGates = true;

	public StayInRangeParams StayInRangeParams = StayInRangeParams.OfSquadLeader();

	private TargettableBodyLocation CurrentBlockingStance;

	private TimeSpan ChangeBlockingStanceCountdown;

	private static float PreferredRange = 8f;

	private static float OutOfDangerRange = 10f;

	private static float EvasiveFatigueLevel = 0.5f;

	private static float EvasiveMinRange = 4f;

	private static float EvasiveMaxRange = 12f;

	public static float MaxAimingDist = 16f;

	public static float MaxChargeDist = 32f;

	public MeleeAttack()
	{
	}

	public MeleeAttack(MovementType movementType, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref CurrentWeapon);
		reflector.Add(ref _evasiveActionTime);
		reflector.Add(ref _strikeTime);
		reflector.AddAfter(ref _attackDefencesFailedTime, 303);
		reflector.AddAfter(ref StayInRangeFailedTime, 419);
		reflector.Add(ref _dontOpenOurGates);
		StayInRangeParams.Reflect(reflector);
		reflector.Add(ref CurrentBlockingStance);
		if (reflector.Version < 520)
		{
			TargettableBodyLocation value = TargettableBodyLocation.Torso;
			reflector.Add(ref value);
		}
		reflector.Add(ref ChangeBlockingStanceCountdown);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MeleeAttack;
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
		if (targetCharacter == null)
		{
			return false;
		}
		if (targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (targetCharacter == character.SparringPartner && character.SparringType == SparringType.SnowballFight)
		{
			return false;
		}
		if (character.Rank == Rank.Captive && targetCharacter.IsControllableByPlayer() && Target.LastAttackedMeTime == Target.Never)
		{
			return false;
		}
		if (parent is Attack attack)
		{
			if (!attack.WantAttackTarget(character, melee: true))
			{
				return false;
			}
			if (Target.IsInaccessible() && Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && Target.TimeSinceLastAttackedMe <= TimeSpan.FromSeconds(60.0) && attack.PreferredTarget != Target.Object)
			{
				return false;
			}
			if ((Target.GetFlag(TargetFlags.OutsideBase) || Target.IsInaccessible()) && attack._dontOpenOurGates && (!character.IsControllableByPlayer() || attack.PreferredTarget != Target.Object))
			{
				GameTerrain instance = GameTerrain.Instance;
				if (instance.IsTileEnclosedOrBuiltOn(character.Tile) && !instance.IsTileEnclosedOrBuiltOn(targetCharacter.Tile) && (character.SquadLeader == null || instance.IsTileEnclosedOrBuiltOn(character.SquadLeader.Tile)))
				{
					return false;
				}
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && character.IsCurrentActionAnimFromGoal && !character.IsActionAnimFinished())
		{
			return GoalPriority.Attack_Animation;
		}
		Character targetCharacter = GetTargetCharacter();
		if (character.IsGuarding() || !(MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).sqrMagnitude <= PreferredRange * PreferredRange) || Target.GetFlag(TargetFlags.Inaccessible) || targetCharacter == null || Attack.IsInsideEnclosedBase(character) != Attack.IsInsideEnclosedBase(targetCharacter) || GetBestMeleeWeapon(character, parent) == null)
		{
			if (!Active || Target.IsInaccessible())
			{
				return GoalPriority.Attack_MeleeAttack;
			}
			return GoalPriority.Attack_MeleeAttack_Active;
		}
		return GoalPriority.Attack_MeleeAttack_Preferred;
	}

	public void SetMovementTypeWhenNotInDanger(Character character, MovementType movementType)
	{
		_movementTypeWhenNotInDanger = movementType;
		if (Active && IsOutOfDangerRange(character) && SubGoal is MoveTo moveTo)
		{
			moveTo.SetMovementType(character, movementType);
		}
	}

	private bool IsOutOfDangerRange(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			return (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude > OutOfDangerRange * OutOfDangerRange;
		}
		return false;
	}

	private bool IsInRangeOfTarget(Character character, AttackType desiredAttackType)
	{
		Character targetCharacter = GetTargetCharacter();
		float meleeAttackRange = character.GetMeleeAttackRange(desiredAttackType, includeExtraDistForMovement: false);
		if (targetCharacter != null)
		{
			return (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= meleeAttackRange * meleeAttackRange;
		}
		return false;
	}

	private bool IsInEvasiveMinRange(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			return (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude < EvasiveMinRange * EvasiveMinRange;
		}
		return false;
	}

	private bool IsFacingTarget(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return false;
		}
		return Vector2.Dot(targetCharacter.PosXZ - character.PosXZ, MathUtil.ToXZ(character.Forward)) >= 0f;
	}

	private bool CouldParryIncomingAttack(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return false;
		}
		if (targetCharacter.CurrentActionAnim == ActionAnim.Attack && character.EquippedItem is MeleeWeapon && targetCharacter.EquippedItem is MeleeWeapon && targetCharacter.IsFacing(character.PosXZ, MathF.PI / 2f))
		{
			return !targetCharacter.IsActionAnimInterruptible();
		}
		return false;
	}

	private AttackType PickAttackType(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.IsRagdollOrProneOrRecovering())
		{
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			if (character.EquippedItem != null && MathUtil.RandomChoice((float)currentTime.TotalSeconds + (float)character.Id * 57f + 3457f, 0.5f))
			{
				return Character.GetMeleeWeaponAttackTypeFromStance(character.GetCurrentTargetBodyLocation());
			}
			return AttackType.Kick;
		}
		if (targetCharacter != null && targetCharacter.IsZombieJumping())
		{
			if (character.EquippedItem != null)
			{
				return AttackType.AttackJumpingZombie;
			}
			return AttackType.Punch;
		}
		if (targetCharacter != null && targetCharacter.CurrentActionAnim == ActionAnim.ZombieBiteStart)
		{
			if (character.EquippedItem != null)
			{
				return AttackType.Snap;
			}
			return AttackType.Punch;
		}
		if (character.EquippedItem != null)
		{
			return Character.GetMeleeWeaponAttackTypeFromStance(character.GetCurrentTargetBodyLocation());
		}
		return AttackType.Invalid;
	}

	private MeleeWeapon GetBestMeleeWeapon(Character character, Goal parent)
	{
		if (character.SparringPartner == Target.Object && character.SparringType == SparringType.Boxing)
		{
			return null;
		}
		return character.Inventory.GetBestWeapon(character, (Target != null) ? Target.Object : null, wantRanged: false, character.SparringType == SparringType.Fencing) as MeleeWeapon;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.EquippedItem == CurrentWeapon)
		{
			character.DesiredEquippedItem = character.EquippedItem;
		}
		Character targetCharacter = GetTargetCharacter();
		if (character.IsAuthoritative() && character.SparringPartner == null && FleeGoal.IsOutOfRangedAmmo(character) && !Target.GetFlag(TargetFlags.Inaccessible) && targetCharacter != null && !character.IsTargetSurrendering(targetCharacter))
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.OutOfAmmoMelee);
			character.Speak(speechForSituation);
		}
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		CurrentWeapon = GetBestMeleeWeapon(character, parent);
		PickNewBlockingStance(character);
		SetSubGoal(character, parent, PickGoal(character, parent));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return null;
		}
		if (SubGoal is MeleeAttackAnim)
		{
			PickNewBlockingStance(character);
		}
		if (SubGoal is FindGoal { Success: not false, FindType: FindType.Bandage })
		{
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel();
			if (equipment != null && character.HasUnbandagedInjury(equipment.GetBandageLevel()))
			{
				return new BandageSelfAnim(equipment);
			}
		}
		if (SubGoal is AttackDefences { Success: false })
		{
			_attackDefencesFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (SubGoal is MoveTo { Success: false } moveTo)
		{
			Goal moveWithinRangeOfSquadLeaderGoal = Attack.GetMoveWithinRangeOfSquadLeaderGoal(character, _dontOpenOurGates, StayInRangeParams);
			if (moveWithinRangeOfSquadLeaderGoal != null)
			{
				return moveWithinRangeOfSquadLeaderGoal;
			}
			if (Target.Camouflage < 1f)
			{
				if (!(moveTo is MoveWithinRangeOfTarget moveWithinRangeOfTarget) || (character.Pos - Target.Object.Pos).magnitude > moveWithinRangeOfTarget.MaxRange)
				{
					if (character.SparringPartner != Target.Object && (_attackDefencesFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _attackDefencesFailedTime >= TimeSpan.FromSeconds(4.0)) && (character.HasMolotovCocktail() || character.HasExplosives()))
					{
						AttackType desiredAttackType = PickAttackType(character);
						return GetAttackDefencesGoal(character, desiredAttackType);
					}
					Target.MarkInaccessible();
				}
				bool aiming = character.WasAiming || character.EquippedItem is MeleeWeapon;
				return new WaitAndFaceTarget(TimeSpan.FromSeconds((character.SquadLeader != null) ? 1f : 2f), aiming);
			}
			Target.SetFlag(TargetFlags.Lost, on: true);
			character.SetRecentActivity(RecentActivityType.HighAlert, GetTargetCharacter());
			return null;
		}
		if (SubGoal is MoveDirectlyToTarget { Success: not false })
		{
			Character targetCharacter = GetTargetCharacter();
			AttackType attackType = PickAttackType(character);
			if (character.CanMeleeAttack(targetCharacter, attackType) && !TargetHasSurrenderred(character) && !TargetIsBeingRestrained(character, targetCharacter))
			{
				return new MeleeAttackAnim(attackType);
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
			StayInRangeFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		return PickGoal(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
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
		AttackType attackType = PickAttackType(character);
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is WaitAndFaceTarget || (SubGoal is FlankTarget && character.GetFatigueMinusAdrenaline() < Character.ExhaustedFatigueLevel && IsInEvasiveMinRange(character)))
		{
			if (character.CanMeleeAttack(targetCharacter, attackType))
			{
				if (!TargetHasSurrenderred(character) && !TargetIsBeingRestrained(character, targetCharacter) && !CouldParryIncomingAttack(character))
				{
					SetSubGoal(character, parent, new MeleeAttackAnim(attackType));
				}
			}
			else if (character.SparringPartner == Target.Object && IsInEvasiveMinRange(character) && Target.TimeSinceInaccessible >= TimeSpan.FromSeconds(2.0) && (character.SparringType != SparringType.Feuding || character.GetFatigueMinusAdrenaline() >= EvasiveFatigueLevel))
			{
				SetSubGoal(character, parent, new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams));
			}
		}
		if (ChangeBlockingStanceCountdown > TimeSpan.Zero)
		{
			TimeSpan timeSpan = currentTime - character.LastThinkTime;
			ChangeBlockingStanceCountdown -= timeSpan;
			if (ChangeBlockingStanceCountdown <= TimeSpan.Zero)
			{
				ChangeBlockingStanceCountdown = TimeSpan.Zero;
			}
		}
		if (ChangeBlockingStanceCountdown <= TimeSpan.Zero && (SubGoal is WaitAndFaceTarget || SubGoal is FlankTarget))
		{
			PickNewBlockingStance(character);
		}
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget && !TargetHasSurrenderred(character) && !TargetIsBeingRestrained(character, targetCharacter))
		{
			if (moveWithinRangeOfTarget.MinRange == EvasiveMinRange && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _evasiveActionTime >= TimeSpan.FromSeconds(0.5) && character.GetFatigueMinusAdrenaline() < Character.ExhaustedFatigueLevel && character.CanMeleeAttack(targetCharacter, attackType) && !CouldParryIncomingAttack(character))
			{
				SetSubGoal(character, parent, new MeleeAttackAnim(attackType));
			}
			else if (CanCharge(character, attackType))
			{
				SetSubGoal(character, parent, new MoveDirectlyToTarget(character, MovementType.Run, Character.HumanRadius * 2f + 0.1f));
			}
			else if (moveWithinRangeOfTarget.MinRange == 0f && targetCharacter != null && character.SparringPartner == targetCharacter && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _strikeTime >= TimeSpan.FromSeconds(3.0) && !character.CanMeleeAttack(targetCharacter, attackType))
			{
				SetSubGoal(character, parent, new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams));
			}
		}
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		if (source != null && source == Target.Object && !source.Zombie && injuryLocation == InjuryLocation.Head && CurrentBlockingStance == TargettableBodyLocation.Legs && (injuryLocation == InjuryLocation.LeftLeg || injuryLocation == InjuryLocation.RightLeg) && CurrentBlockingStance == TargettableBodyLocation.Head)
		{
			SetBlockingStance(character, TargettableBodyLocation.Torso);
		}
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
	}

	private void SetBlockingStance(Character character, TargettableBodyLocation stance)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		float t = character.GetSkillLevelWithEffects(SkillType.HandToHand);
		float a = Mathf.Lerp(2f, 1f, t);
		float b = Mathf.Lerp(10f, 2f, t);
		CurrentBlockingStance = stance;
		ChangeBlockingStanceCountdown = TimeSpan.FromSeconds(Mathf.Lerp(a, b, MathUtil.RandomFloat((float)currentTime.TotalSeconds)));
	}

	private void PickNewBlockingStance(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		TargettableBodyLocation targettableBodyLocation = (TargettableBodyLocation)MathUtil.RandomInt(((int)currentTime.Ticks ^ (int)(currentTime.Ticks >> 32)) * 2 + character.Id * 1000 + 234, 3);
		if (targetCharacter != null)
		{
			if (targetCharacter.Zombie && !targetCharacter.ShouldLimp())
			{
				float t = (float)character.GetSkillLevelWithEffects(SkillType.HandToHand) / 5f;
				if (MathUtil.RandomChoice((float)(currentTime.Ticks + character.Id * 12) + 9877f, Mathf.Lerp(0.25f, 0.75f, t)) && !targetCharacter.IsArmoredOnBodyLocation(TargettableBodyLocation.Legs))
				{
					targettableBodyLocation = TargettableBodyLocation.Legs;
				}
			}
			else if (targetCharacter.IsRagdollOrProneOrRecovering())
			{
				targettableBodyLocation = (TargettableBodyLocation)MathUtil.RandomInt(((int)currentTime.Ticks ^ (int)(currentTime.Ticks >> 32)) * 3 + character.Id * 2000 + 567, 2);
			}
		}
		if (character.CanTargetBodyLocation(targettableBodyLocation, targetCharacter))
		{
			SetBlockingStance(character, targettableBodyLocation);
		}
	}

	private bool TargetHasSurrenderred(Character character)
	{
		Character targetCharacter = GetTargetCharacter();
		return character.IsTargetSurrendering(targetCharacter);
	}

	public static bool TargetIsBeingRestrained(Character character, Character targetCharacter)
	{
		if (targetCharacter != null && targetCharacter.SparringPartner == character)
		{
			if (targetCharacter.SparringType == SparringType.Feuding && targetCharacter.InteractionObject != null)
			{
				return true;
			}
			if (targetCharacter.Speaking != null && targetCharacter.Speaking.Importance == Importance.EndFisticuffs)
			{
				return true;
			}
		}
		return false;
	}

	public Goal PickGoal(Character character, Goal parent)
	{
		AttackType attackType = PickAttackType(character);
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		Character targetCharacter = GetTargetCharacter();
		if (character.SquadLeader != null)
		{
			Building buildingSquadLeaderIsInOrWantsToBeIn = Attack.GetBuildingSquadLeaderIsInOrWantsToBeIn(character);
			if (buildingSquadLeaderIsInOrWantsToBeIn != null && character.InsideBuilding == buildingSquadLeaderIsInOrWantsToBeIn)
			{
				return new Wait(TimeSpan.FromSeconds(2.0));
			}
		}
		if (targetCharacter == null)
		{
			return new Idle();
		}
		if (!character.IsOutdoors())
		{
			return new LeaveBuilding();
		}
		if (IsOutOfDangerRange(character))
		{
			if (Target.GetFlag(TargetFlags.Inaccessible))
			{
				return GetMoveWithinRangeAndSightOfTargetGoal(character, attackType);
			}
			_strikeTime = currentTime;
			bool flag = character.WasAiming || (Target.Visible && MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).sqrMagnitude < MaxAimingDist * MaxAimingDist && character.EquippedItem == CurrentWeapon);
			return new MoveWithinRangeOfTarget(flag ? MovementType.Jog : _movementTypeWhenNotInDanger, flag, 0f, OutOfDangerRange * 0.9f, _dontOpenOurGates, StayInRangeParams);
		}
		if (character.EquippedItem != CurrentWeapon)
		{
			if (CurrentWeapon != null && !character.Inventory.Contains(CurrentWeapon))
			{
				return null;
			}
			return new Equip(CurrentWeapon);
		}
		if (TargetHasSurrenderred(character))
		{
			Goal bandageOrMedicateGoal = RangedAttack.GetBandageOrMedicateGoal(character, Target);
			if (bandageOrMedicateGoal != null)
			{
				return bandageOrMedicateGoal;
			}
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(1.0), aiming: true);
		}
		if (character.SparringPartner == Target.Object && character.SparringType != SparringType.Feuding && SubGoal is Equip && IsInEvasiveMinRange(character))
		{
			_evasiveActionTime = currentTime;
			return new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams);
		}
		if (character.SparringPartner == Target.Object && SubGoal is MeleeAttackAnim && !targetCharacter.IsRagdollOrProneOrRecovering())
		{
			_evasiveActionTime = currentTime;
			return new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams);
		}
		if (character.SparringPartner == Target.Object && SubGoal is MoveWithinRangeOfTarget && (SubGoal as MoveWithinRangeOfTarget).MinRange == EvasiveMinRange)
		{
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.5f + MathUtil.RandomFloat((float)character.Id + (float)currentTime.TotalSeconds) * 1.5f), aiming: true);
		}
		if (Target.IsInaccessible() && character.SparringPartner != Target.Object && (_attackDefencesFailedTime.Ticks == 0L || PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - _attackDefencesFailedTime >= TimeSpan.FromSeconds(4.0)) && (character.HasMolotovCocktail() || character.HasExplosives()))
		{
			return GetAttackDefencesGoal(character, attackType);
		}
		if (!IsInRangeOfTarget(character, attackType))
		{
			if (!Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && !Target.GetFlag(TargetFlags.Inaccessible) && character.GetFatigueMinusAdrenaline() >= EvasiveFatigueLevel && (!targetCharacter.IsRagdollOrProneOrRecovering() || character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel))
			{
				if (IsInEvasiveMinRange(character))
				{
					_evasiveActionTime = currentTime;
					return new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams);
				}
				if (character.InsideBuilding == null && MathUtil.RandomChoice((float)currentTime.TotalSeconds, 0.5f))
				{
					return new FlankTarget(character, MovementType.Jog, Mathf.Lerp(EvasiveMinRange, EvasiveMaxRange, 0.5f), TimeSpan.FromSeconds(Mathf.Lerp(2f, 4f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + 10000f))));
				}
				return new WaitAndFaceTarget(TimeSpan.FromSeconds(1.0), aiming: true);
			}
			_strikeTime = currentTime;
			if (CanCharge(character, attackType))
			{
				return new MoveDirectlyToTarget(character, MovementType.Run, Character.HumanRadius * 2f + 0.1f);
			}
			return GetMoveWithinRangeAndSightOfTargetGoal(character, attackType);
		}
		if (!Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && character.GetFatigueMinusAdrenaline() >= Character.ExhaustedFatigueLevel)
		{
			_evasiveActionTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			return new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, EvasiveMinRange, EvasiveMaxRange, _dontOpenOurGates, StayInRangeParams);
		}
		if (!IsFacingTarget(character))
		{
			return new TurnToTarget();
		}
		if (CouldParryIncomingAttack(character))
		{
			return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.5), aiming: true);
		}
		if (!TargetHasSurrenderred(character) && !TargetIsBeingRestrained(character, targetCharacter))
		{
			if (character.CanMeleeAttack(targetCharacter, attackType))
			{
				return new MeleeAttackAnim(attackType);
			}
			bool flag2 = Attack.IsInsideEnclosedBase(character) != Attack.IsInsideEnclosedBase(targetCharacter);
			if (!flag2 && !Target.IsInaccessible() && character.InsideBuilding == null && MathUtil.RandomChoice((float)currentTime.TotalSeconds, 0.5f))
			{
				float desiredRange = Mathf.Max(character.GetMeleeAttackRange(attackType, includeExtraDistForMovement: false) - 0.5f, character.Radius + targetCharacter.Radius);
				return new FlankTarget(character, MovementType.Jog, desiredRange, TimeSpan.FromSeconds(Mathf.Lerp(1f, 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + 10000f))));
			}
			if (flag2 && SubGoal is WaitAndFaceTarget)
			{
				return new MoveWithinRangeOfTarget(MovementType.Jog, aiming: true, 0f, 1.1f, _dontOpenOurGates, StayInRangeParams);
			}
		}
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(0.5f + MathUtil.RandomFloat((float)character.Id + (float)currentTime.TotalSeconds) * 0.5f), aiming: true);
	}

	private Goal GetMoveWithinRangeAndSightOfTargetGoal(Character character, AttackType desiredAttackType)
	{
		float meleeAttackRange = character.GetMeleeAttackRange(desiredAttackType, includeExtraDistForMovement: false);
		bool flag = Target.Visible && MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).sqrMagnitude < MaxAimingDist * MaxAimingDist && character.EquippedItem == CurrentWeapon;
		return new MoveWithinRangeAndSightOfTarget(flag ? MovementType.Jog : MovementType.Run, flag, 0f, meleeAttackRange * 0.9f, _dontOpenOurGates, StayInRangeParams, 0f);
	}

	private Goal GetAttackDefencesGoal(Character character, AttackType desiredAttackType)
	{
		float meleeAttackRange = character.GetMeleeAttackRange(desiredAttackType, includeExtraDistForMovement: false);
		return new AttackDefences(0f, Math.Max(meleeAttackRange * 0.9f, 1.5f), _movementTypeWhenNotInDanger, StayInRangeParams);
	}

	private bool HasPathToChargeTarget(Character character, int options)
	{
		if (character.InteractionObject != null)
		{
			return false;
		}
		Vector2 vector = Target.Object.PosXZ - character.PosXZ;
		float magnitude = vector.magnitude;
		if (magnitude <= MaxChargeDist)
		{
			Ray ray = new Ray(character.Position, MathUtil.ToX0Y(vector / magnitude));
			return GameTerrain.Instance.IsPassable(ray, magnitude, options, character, Target.Object, character.Tile, character.IsPredicted());
		}
		return false;
	}

	private bool CanCharge(Character character, AttackType desiredAttackType)
	{
		if (character.IsRagdollOrProneOrRecovering() || character.IsGettingUp() || character.IsInDamageReactionAnim())
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (targetCharacter.IsBeingBittenOrChoked())
		{
			return false;
		}
		if (targetCharacter.IsDodging())
		{
			return false;
		}
		if (targetCharacter.IsFleeing())
		{
			return false;
		}
		if (!(character.EquippedItem is MeleeWeapon))
		{
			return false;
		}
		StayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		if (resultDist < float.MaxValue)
		{
			float meleeAttackRange = character.GetMeleeAttackRange(desiredAttackType, includeExtraDistForMovement: false);
			if (resultTile.GetDistSquared(targetCharacter.Tile) >= MathUtil.Squared(resultDist + meleeAttackRange + 2f))
			{
				return false;
			}
		}
		if (targetCharacter.EquippedItem is RangedWeapon)
		{
			return HasPathToChargeTarget(character, 2051);
		}
		return false;
	}

	public override TargettableBodyLocation GetCurrentTargetBodyLocation(Character character)
	{
		return CurrentBlockingStance;
	}
}
