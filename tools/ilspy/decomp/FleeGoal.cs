using System;

public class FleeGoal : StateMachineGoal
{
	public bool DontOpenOurGates;

	private bool FleeFailed;

	private TimeSpan FleeFailedTime;

	public static float FleeDist = 60f;

	public FleeGoal()
	{
	}

	public FleeGoal(bool dontOpenOurGates)
	{
		DontOpenOurGates = dontOpenOurGates;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref DontOpenOurGates, 86);
		reflector.Add(ref FleeFailed);
		reflector.AddAfter(ref FleeFailedTime, 114);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.FleeGoal;
	}

	public override bool IsFleeing()
	{
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (FleeFailed && currentTime - FleeFailedTime < TimeSpan.FromSeconds(60.0))
		{
			return false;
		}
		if (IsTargetMovingVehicle() && (!Target.Object.IsSusceptibleToBulletHits() || !character.HasGun()) && (!Target.Object.IsFlammable() || !character.HasMolotovCocktail()) && (Target.Object.IsExplosionProof() || !character.HasExplosives()))
		{
			return true;
		}
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null || targetCharacter.Deleted || Target.GetFlag(TargetFlags.Lost))
		{
			return false;
		}
		if (targetCharacter == character.SparringPartner)
		{
			return false;
		}
		if (targetCharacter.Zombie)
		{
			return false;
		}
		if (character.IsTargetSurrendering(targetCharacter))
		{
			return false;
		}
		if (character.IsTooDepressedToFollowOrders() && !character.IsFEMA() && (Active || character.IsControllableByPlayer() || character.IsOutnumberedBy(targetCharacter.Community, orEqual: true, targetCharacter)))
		{
			return true;
		}
		bool flag = character.IsControllableByOrFollowingPlayer();
		if (flag && character.HaveIOrAnyoneInMySquadBeenOrderedToAttackSomeone())
		{
			return false;
		}
		if (!Active && targetCharacter.IsFleeing())
		{
			return false;
		}
		if (Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && IsOutOfRangedAmmo(character))
		{
			if (Target.GetFlag(TargetFlags.Inaccessible) && !character.IsInEnclosedArea() && targetCharacter.IsInEnclosedArea())
			{
				return true;
			}
			if (Target.LastAttackedMeTime > Target.Never && MathUtil.ToXZ(character.Pos - Target.LastKnownPosition).sqrMagnitude >= MathUtil.Squared(Active ? 8f : 12f) && targetCharacter.UnderAttackRefCount <= 1)
			{
				return true;
			}
			if (flag && (character.IsInEnclosedArea() == targetCharacter.IsInEnclosedArea() || (Active && targetCharacter.IsInEnclosedArea())) && character.IsOutnumberedBy(targetCharacter.Community, orEqual: true, targetCharacter))
			{
				return true;
			}
			if (Target.LastAttackedMeTime > Target.Never && character.Inventory.GetBestWeapon(character, targetCharacter, wantRanged: false, bluntOnly: false) == null)
			{
				return true;
			}
		}
		if (Target.GetFlag(TargetFlags.Inaccessible) && Target.TimeSinceLastAttackedUs >= TimeSpan.FromSeconds(60.0) && !targetCharacter.IsControllableByPlayer() && !character.IsControllableByPlayer() && (Active || !character.IsInEnclosedArea()))
		{
			return true;
		}
		if (!character.IsControllableByPlayer() && !character.IsGuarding() && parent is Attack attack && attack.FallbackTimer >= TimeSpan.FromSeconds(60.0))
		{
			return true;
		}
		if (character.HasPersonality(CachedPersonalityType.Nervous))
		{
			if (character.GetBloodLoss() >= 0.25f)
			{
				if (!Active && !character.IsControllableByPlayer())
				{
					return character.IsOutnumberedBy(targetCharacter.Community, orEqual: true, targetCharacter);
				}
				return true;
			}
			return false;
		}
		if (character.HasPersonality(CachedPersonalityType.Bold))
		{
			return false;
		}
		if (character.GetBloodLoss() >= 0.75f)
		{
			if (!Active && !character.IsControllableByPlayer())
			{
				return character.IsOutnumberedBy(targetCharacter.Community, orEqual: true, targetCharacter);
			}
			return true;
		}
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Attack_Flee;
	}

	public static bool IsOutOfRangedAmmo(Character character)
	{
		for (int i = 0; i < character.Inventory.Count; i++)
		{
			Equipment item = character.Inventory.GetItem(i);
			if (item is AmmoWeapon ammoWeapon)
			{
				if (character.HasInfiniteAmmo(ammoWeapon))
				{
					return false;
				}
				if (ammoWeapon.CurrentAmmo > 0 && character.IsActionAllowedForItem(ammoWeapon.CurrentAmmoType, null, ammoWeapon.InfectedWith, EquipmentPolicyAction.CanUse))
				{
					return false;
				}
				if (character.Inventory.HasAmmoForWeapon(ammoWeapon, null, character))
				{
					return false;
				}
			}
			if (item is MolotovCocktail && character.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
			{
				return false;
			}
			if (item is PipeBomb && character.IsActionAllowedForItem(item, EquipmentPolicyAction.CanUse))
			{
				return false;
			}
		}
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (!character.IsAuthoritative())
		{
			return;
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.Fled, character, GetTargetObject());
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && !character.IsTargetSurrendering(targetCharacter) && !Target.GetFlag(TargetFlags.Inaccessible))
		{
			if (targetCharacter.EquippedItem != null && IsOutOfRangedAmmo(character))
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.OutOfAmmoFleeing);
				character.Speak(speechForSituation, targetCharacter);
			}
			else
			{
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Fleeing);
				character.Speak(speechForSituation2, targetCharacter);
			}
		}
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		if (HasEscaped(character))
		{
			SetSubGoal(character, parent, new WaitAndFaceTarget(TimeSpan.FromSeconds(2.0)));
		}
		else
		{
			SetSubGoal(character, parent, new FleeFromAllEnemies(MovementType.Run, 4f, FleeDist, DontOpenOurGates, avoidHostileBases: true));
		}
	}

	private bool HasEscaped(Character character)
	{
		if (GetTargetObject() == null || MathUtil.ToXZ(character.Pos - Target.LastKnownPosition).sqrMagnitude >= FleeDist * FleeDist || !character.IsOutdoors())
		{
			return true;
		}
		if (!FleeFromAllEnemies.HasAnyThreats(character))
		{
			return true;
		}
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FleeFromAllEnemies fleeFromAllEnemies)
		{
			if (fleeFromAllEnemies.Success)
			{
				return new WaitAndFaceTarget(TimeSpan.FromSeconds(2.0));
			}
			FleeFailed = true;
			FleeFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		}
		if (SubGoal is WaitAndFaceTarget)
		{
			if (!(Target.Camouflage >= 1f))
			{
				if (HasEscaped(character))
				{
					return new WaitAndFaceTarget(TimeSpan.FromSeconds(2.0));
				}
				return new FleeFromAllEnemies(MovementType.Run, 4f, FleeDist, DontOpenOurGates, avoidHostileBases: true);
			}
			Target.SetFlag((TargetFlags)516, on: true);
			character.SetRecentActivity(RecentActivityType.Escape, GetTargetCharacter());
		}
		return base.GetNextSubGoal(character, parent);
	}
}
