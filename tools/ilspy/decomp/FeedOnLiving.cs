using System;
using UnityEngine;

public class FeedOnLiving : StateMachineGoal
{
	public Character PreferredTarget;

	public TimeSpan CombatStartTime;

	public TimeSpan LastAttackTime = Target.Never;

	public static float MaxChargeDist = 6f;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("FeedOnLivingCalcBestTarget");

	public FeedOnLiving()
	{
	}

	public FeedOnLiving(Character character, Character preferredTarget)
	{
		if (preferredTarget != null)
		{
			PreferredTarget = preferredTarget;
			SetTarget(character, null, character.GetOrCreateTarget(preferredTarget));
		}
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref PreferredTarget);
		reflector.AddAfter(ref CombatStartTime, 45);
		reflector.AddAfter(ref LastAttackTime, 622);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FeedOnLiving;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Target != null && Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(60.0))
		{
			return GoalPriority.Zombie_FightAttacker;
		}
		if (Target != null && Target.FullyTracked && MathUtil.ToXZ(Target.LastKnownPosition - character.Pos).magnitude < (Active ? 16f : 8f))
		{
			return GoalPriority.Zombie_FeedOnLivingNearby;
		}
		return GoalPriority.Zombie_FeedOnLiving;
	}

	public override bool IsHighAlert(Character character)
	{
		return true;
	}

	public override void OnPreActivate(Character character, Goal parent)
	{
		base.OnPreActivate(character, parent);
		character.AboutToBeInCombat = true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		character.AboutToBeInCombat = false;
		character.InCombat = true;
		CombatStartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		base.OnActivate(character, parent);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		character.InCombat = false;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsAuthoritative())
		{
			targetCharacter.UnderAttackRefCount++;
		}
		if (CanCharge(character))
		{
			Target.ClearInaccessible();
			SetSubGoal(character, parent, new ChargeTarget());
			LastAttackTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			return;
		}
		SetSubGoal(character, parent, GetMoveToTargetOrFlankGoal(character));
		if (character.IsAuthoritative() && targetCharacter != null && !character.IsBeingChoked())
		{
			character.PlayVoiceSoundFromList(SoundManager.ZombieAttackSounds[(int)character.Appearance.Gender], VoiceSoundType.ZombieSnarl);
			Session.Instance.AISoundManager.AddSound(new AISound(AISoundType.Warning, character.Position, character.GetShoutVoiceRadius(), character.GetMaxSoundVisibilityRange(), character, targetCharacter, character, character));
		}
	}

	protected override void OnDeactivateTarget(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsAuthoritative())
		{
			targetCharacter.UnderAttackRefCount = Math.Max(0, targetCharacter.UnderAttackRefCount - 1);
		}
		base.OnDeactivateTarget(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (!Finished)
		{
			if (!(SubGoal is ChargeTarget) && CanCharge(character))
			{
				Target.ClearInaccessible();
				SetSubGoal(character, parent, new ChargeTarget());
				LastAttackTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			}
			if (SubGoal is MoveToTarget && !character.ShouldLimp() && HasPathToChargeTarget(character, 0))
			{
				SetSubGoal(character, parent, GetFlankGoal(character));
			}
			if (SubGoal is ZombieFrustrationGoal { TargetWasInaccessible: not false } && GameTerrain.Instance.LastChangedTime > Target.LastInaccessibleTime)
			{
				SetSubGoal(character, parent, new MoveToTarget(character.ShouldLimp() ? MovementType.Walk : MovementType.Run));
			}
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is ChargeTarget)
		{
			if (!targetCharacter.IsAwake)
			{
				return null;
			}
			LastAttackTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (SubGoal is MoveAsCloseAsPossibleToTarget)
		{
			Target.MarkInaccessible();
			return new ZombieFrustrationGoal(character)
			{
				TargetWasInaccessible = true
			};
		}
		if (SubGoal is MoveToTarget)
		{
			return new MoveAsCloseAsPossibleToTarget(character.ShouldLimp() ? MovementType.Walk : MovementType.Run);
		}
		return GetMoveToTargetOrFlankGoal(character);
	}

	public Goal GetMoveToTargetOrFlankGoal(Character character)
	{
		if (Target.Visible && !character.ShouldLimp() && HasPathToChargeTarget(character, 0))
		{
			return GetFlankGoal(character);
		}
		return new MoveToTarget(character.ShouldLimp() ? MovementType.Walk : MovementType.Run);
	}

	public Goal GetFlankGoal(Character character)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return new ZombieFrustrationGoal(character);
		}
		float desiredRange = Mathf.Lerp(targetCharacter.GetMeleeAttackRange(Character.GetMeleeWeaponAttackTypeFromStance(targetCharacter.GetCurrentTargetBodyLocation()), includeExtraDistForMovement: true) + 0.5f, MaxChargeDist - 1f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + 10000f));
		TimeSpan timeout = TimeSpan.FromSeconds(Mathf.Lerp(1f, 2f, MathUtil.RandomFloat((float)currentTime.TotalSeconds + 20000f)));
		return new FlankTarget(character, MovementType.Jog, desiredRange, timeout);
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

	private bool CanCharge(Character character)
	{
		if (character.IsRagdollOrProneOrRecovering() || character.IsGettingUp() || character.IsInDamageReactionAnim())
		{
			return false;
		}
		bool checkIfTargetIsAttacking = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - MathUtil.Max(CombatStartTime, LastAttackTime) < TimeSpan.FromSeconds(5.0);
		Character targetCharacter = GetTargetCharacter();
		if (!CanAttackTarget(character, targetCharacter, checkIfTargetIsAttacking))
		{
			return false;
		}
		return HasPathToChargeTarget(character, 2051);
	}

	public override TileObject GetCombatTarget(out TimeSpan combatStartTime)
	{
		combatStartTime = CombatStartTime;
		return GetTargetObject();
	}

	public static bool CanAttackTarget(Character character, Character targetCharacter, bool checkIfTargetIsAttacking = false)
	{
		if (targetCharacter == null || targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (targetCharacter.IsBeingBittenOrChoked())
		{
			return false;
		}
		if (targetCharacter.IsChokingSomeone())
		{
			return false;
		}
		if (targetCharacter.IsDodging() && (targetCharacter.PosXZ - character.PosXZ).sqrMagnitude <= MathUtil.Squared(ChargeTarget.JumpDist + 1f))
		{
			return false;
		}
		if (checkIfTargetIsAttacking && ((targetCharacter.EquippedItem is MeleeWeapon && targetCharacter.CurrentActionAnim == ActionAnim.Attack) || targetCharacter.CurrentActionAnim == ActionAnim.PunchLeft || targetCharacter.CurrentActionAnim == ActionAnim.PunchRight) && targetCharacter.IsFacing(character.PosXZ, MathF.PI / 2f))
		{
			return false;
		}
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (IsTargetMovingVehicle())
		{
			return true;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return false;
		}
		if (!Active && !targetCharacter.IsAwake)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (HasUserTarget)
			{
				return Target;
			}
			Target result = null;
			float num = 1E+38f;
			if (Active && SubGoal is ChargeTarget && !IsTargetDeleted() && Target.Object is Character { Zombie: false })
			{
				return Target;
			}
			Target target = (Active ? Target : null);
			foreach (Target target2 in character.Targets)
			{
				if (target2.Object == null || target2.Object.Deleted || target2.GetFlag(TargetFlags.Lost))
				{
					continue;
				}
				if (!(target2.Object is Character character3))
				{
					if (!(target2.Object is EnterableVehicle))
					{
						continue;
					}
				}
				else if (character3.Zombie || (character.Infection == InfectionType.Invisible && character3.InvisibleStrain != InvisibleStrainType.None) || !character3.IsAwake || character3.IsPlayingDead() || (!character3.IsOutdoors() && character3.InsideBuilding != null && (target2 != target || character.Tile.GetDistSquared(character3.InsideBuilding.GetNearestTileTo(character.Tile)) <= MathUtil.Squared(MaxChargeDist + 2f))))
				{
					continue;
				}
				if (target2 != target && target2.Object != PreferredTarget && target2.Camouflage > 0f)
				{
					continue;
				}
				float num2 = character.Get2DDistToTargetAimPos(target2, Vector3.zero) / Character.ZombieWalkSpeed;
				float num3 = (float)MathUtil.Min(target2.TimeSinceLastFullyVisible, target2.TimeSinceLastHeardAttack).TotalSeconds + num2;
				num3 *= Mathf.Clamp01((float)target2.TimeSinceLastAttackedMe.TotalSeconds / 10f);
				if (target2 == target)
				{
					num3 *= 0.75f;
				}
				if (target2.Object == PreferredTarget || !(num3 > 30f))
				{
					if (target2.IsInaccessible())
					{
						num3 *= 4f * Mathf.Max(0f, 10f - (float)target2.TimeSinceInaccessible.TotalSeconds);
					}
					if (target2.Object == PreferredTarget)
					{
						num3 *= 0.25f;
					}
					if (num3 < num)
					{
						result = target2;
						num = num3;
					}
				}
			}
			return result;
		}
	}

	public override MovementType GetMovementType()
	{
		return MovementType.Run;
	}
}
