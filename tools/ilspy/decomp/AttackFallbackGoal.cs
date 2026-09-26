using System;

public class AttackFallbackGoal : StateMachineGoal
{
	public bool DontOpenOurGates = true;

	public bool IsInCover;

	public bool IsInCrouchingCover;

	public bool RanAway;

	public TimeSpan TakeCoverFailedTime;

	public StayInRangeParams StayInRangeParams = StayInRangeParams.OfSquadLeader();

	public AttackFallbackGoal()
	{
	}

	public AttackFallbackGoal(bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		DontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.AttackFallbackGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (SubGoal is FindGoal)
		{
			return GoalPriority.Attack_Fallback_FindingSomething;
		}
		return GoalPriority.Attack_Fallback;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref DontOpenOurGates);
		reflector.AddAfter(ref IsInCover, 263);
		reflector.AddAfter(ref IsInCrouchingCover, 263);
		reflector.AddAfter(ref RanAway, 263);
		reflector.AddAfter(ref TakeCoverFailedTime, 263);
		StayInRangeParams.Reflect(reflector);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		RanAway = false;
		SetSubGoal(character, parent, GetNextSubGoal(character, parent));
	}

	public bool ShouldTakeCover(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (character.InsideBuilding != null)
		{
			if (!character.IsOutdoors())
			{
				return false;
			}
			if (!Target.GetFlag(TargetFlags.HasLoadedRangedWeapon))
			{
				return false;
			}
			if (character.Inventory.GetBestWeapon(character, Target.Object, wantRanged: true, bluntOnly: false) != null)
			{
				return false;
			}
		}
		if (TakeCoverFailedTime.Ticks != 0L && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - TakeCoverFailedTime < TimeSpan.FromSeconds(3.0))
		{
			return false;
		}
		if (Target.TimeSinceLastVisible >= RangedAttack.MaxTimeSinceTargetVisibleForTakeCover)
		{
			return false;
		}
		Character targetCharacter = GetTargetCharacter();
		if (character.IsTargetSurrendering(targetCharacter))
		{
			return false;
		}
		if (targetCharacter != null && targetCharacter.EquippedItem is RangedWeapon)
		{
			return (character.PosXZ - targetCharacter.PosXZ).magnitude < targetCharacter.EquippedItem.GetRangeIncludingEffects(targetCharacter, character);
		}
		return false;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		FindGoal findGoal = SubGoal as FindGoal;
		if (findGoal != null && findGoal.Success && findGoal.FindType == FindType.Bandage)
		{
			Equipment equipment = character.Inventory.FindItemWithHighestBandageLevel();
			if (equipment != null && character.HasUnbandagedInjury(equipment.GetBandageLevel()))
			{
				return new BandageSelfAnim(equipment);
			}
		}
		if (findGoal != null && findGoal.Success && findGoal.FindType == FindType.Ammo)
		{
			foreach (Target target in character.Targets)
			{
				target.ClearInaccessible();
			}
			return null;
		}
		if (SubGoal is MoveWithinRangeOfTarget moveWithinRangeOfTarget)
		{
			if (moveWithinRangeOfTarget.Success)
			{
				RanAway = true;
			}
			else
			{
				TakeCoverFailedTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			}
		}
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is TakeCoverGoal { Success: false } && targetCharacter != null && targetCharacter.EquippedItem is RangedWeapon)
		{
			float rangeIncludingEffects = targetCharacter.EquippedItem.GetRangeIncludingEffects(targetCharacter, character);
			return new MoveWithinRangeOfTarget(MovementType.Run, aiming: false, rangeIncludingEffects + 4f, rangeIncludingEffects + 32f, DontOpenOurGates);
		}
		IsInCover = TakeCoverGoal.CheckCover(character, Target, out IsInCrouchingCover);
		if (!IsInCover && ShouldTakeCover(character, parent))
		{
			return new TakeCoverGoal(DontOpenOurGates, StayInRangeParams);
		}
		bool flag = findGoal != null && !findGoal.Success;
		if (!flag)
		{
			Goal bandageOrMedicateGoal = RangedAttack.GetBandageOrMedicateGoal(character, Target);
			if (bandageOrMedicateGoal != null)
			{
				return bandageOrMedicateGoal;
			}
		}
		if (Target != null && Target.Camouflage >= 1f)
		{
			if (RanAway || Target.GetFlag(TargetFlags.EscapedFromTarget))
			{
				Target.SetFlag((TargetFlags)516, on: true);
				character.SetRecentActivity(RecentActivityType.Escape, GetTargetCharacter());
			}
			else
			{
				Target.SetFlag(TargetFlags.Lost, on: true);
				character.SetRecentActivity(RecentActivityType.HighAlert, GetTargetCharacter());
			}
			return null;
		}
		if (!(SubGoal is MoveWithinRangeOfSquadLeader) && !(SubGoal is MoveToAndEnterBuilding))
		{
			Goal moveWithinRangeOfSquadLeaderGoal = Attack.GetMoveWithinRangeOfSquadLeaderGoal(character, DontOpenOurGates, StayInRangeParams);
			if (moveWithinRangeOfSquadLeaderGoal != null)
			{
				return moveWithinRangeOfSquadLeaderGoal;
			}
		}
		if (!flag && character.IsAuthoritative() && !character.HasInfiniteAmmo(null) && character.Inventory.IsOutOfAmmo(character) && !WouldBeMoreVulnerableIfExitingBuilding(character))
		{
			return new FindGoal(FindType.Ammo, MovementType.Run, critical: true)
			{
				MaxDistToTravel = 64f,
				OnlyInEnclosedAreas = Attack.IsInsideEnclosedBase(character)
			};
		}
		if (IsTargetDeleted())
		{
			return new Wait(TimeSpan.FromSeconds(2.0));
		}
		if (targetCharacter != null && Target.GetFlag(TargetFlags.OutsideBase) && !GameTerrain.Instance.IsTileEnclosedOrBuiltOn(targetCharacter.Tile) && GameTerrain.Instance.IsTileEnclosedOrBuiltOn(character.Tile) && !WouldBeMoreVulnerableIfExitingBuilding(character))
		{
			Target.MarkInaccessible();
		}
		return new WaitAndFaceTarget(TimeSpan.FromSeconds(2.0), aiming: false, IsInCrouchingCover);
	}

	public static bool WouldBeMoreVulnerableIfExitingBuilding(Character character)
	{
		if (character.InsideBuilding != null)
		{
			bool flag = Attack.IsInsideEnclosedBase(character);
			bool flag2 = character.IsOutdoors();
			bool flag3 = character.InsideBuilding.IsFlammable();
			bool flag4 = character.InsideBuilding.IsExplosionProof();
			bool flag5 = false;
			{
				foreach (Target target in character.Targets)
				{
					if (!(target.Object is Character character2) || !character.IsEnemy(character2))
					{
						continue;
					}
					if (target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && !character2.Zombie)
					{
						if (flag2)
						{
							return false;
						}
						if (flag3 && character2.HasMolotovCocktail())
						{
							return false;
						}
						if (!flag4 && character2.HasExplosives())
						{
							return false;
						}
					}
					TerrainCoord tile = character2.Tile;
					flag5 |= flag == GameTerrain.Instance.IsTileEnclosed(tile.x, tile.y);
				}
				return flag5;
			}
		}
		return false;
	}
}
