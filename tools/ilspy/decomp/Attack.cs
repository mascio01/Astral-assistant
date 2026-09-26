using System;
using UnityEngine;

public class Attack : PrioritiserGoal
{
	public TileObject PreferredTarget;

	public bool CanAttackPreferredTargetEvenIfNotEnemy;

	public bool _dontOpenOurGates = true;

	public TimeSpan ChaseTimer;

	public TimeSpan CombatStartTime;

	public TimeSpan FallbackTimer;

	public StayInRangeParams StayInRangeParams = StayInRangeParams.OfSquadLeader();

	public static float FollowerBuildingHysteresisDist = 4f;

	public static float FollowerHysteresisDist = 2f;

	public static float MaxDistFromLeader = 16f;

	public static TimeSpan StopChasingTime = TimeSpan.FromSeconds(2.0);

	private MovementType _movementTypeWhenNotInDanger = MovementType.Run;

	private static GameProfiler CalcBestTargetTimer = new GameProfiler("AttackCalcBestTarget");

	public Attack()
	{
	}

	public Attack(Character character, TileObject preferredTarget, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		_dontOpenOurGates = dontOpenOurGates;
		StayInRangeParams = stayInRangeParams;
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
		reflector.AddAfter(ref CanAttackPreferredTargetEvenIfNotEnemy, 490);
		reflector.Add(ref _movementTypeWhenNotInDanger);
		reflector.Add(ref _dontOpenOurGates);
		reflector.AddAfter(ref ChaseTimer, 35);
		reflector.AddAfter(ref CombatStartTime, 45);
		reflector.AddAfter(ref FallbackTimer, 114);
		StayInRangeParams.Reflect(reflector);
		if (reflector.IsDeserialising && reflector.Version < 326)
		{
			AddSubGoal(new RetreatToBaseGoal());
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.Attack;
	}

	public override TileObject GetCombatTarget(out TimeSpan combatStartTime)
	{
		combatStartTime = CombatStartTime;
		return GetTargetObject();
	}

	public override bool IsHighAlert(Character character)
	{
		return true;
	}

	public override bool WantSquadToStayInRange(Character character)
	{
		if (SubGoal is FleeGoal)
		{
			return false;
		}
		if (SubGoal is AttackFallbackGoal && !character.IsControllableByPlayer())
		{
			return false;
		}
		return true;
	}

	public override AIOverridesControlReason AIOverridesControl(Character character, Goal parent)
	{
		if ((character.SparringType == SparringType.Feuding || character.SparringType == SparringType.FightToTheDeath) && !character.IsPlayerAvatar() && Target != null && Target.Object == character.SparringPartner)
		{
			return AIOverridesControlReason.Feuding;
		}
		return base.AIOverridesControl(character, parent);
	}

	public static Building GetBuildingSquadLeaderIsInOrWantsToBeIn(Character character)
	{
		Building building = null;
		if (character.SquadLeader != null)
		{
			building = character.SquadLeader.InsideBuilding;
			if (building == null && character.SquadLeader.FindActiveGoal(GoalType.MoveToAndEnterBuilding) is MoveToAndEnterBuilding moveToAndEnterBuilding)
			{
				building = moveToAndEnterBuilding.GetTargetBuilding();
				if (building != character.InsideBuilding && building.Inhabitants.Length - building.GetInhabitantCount() <= 1)
				{
					building = null;
				}
			}
		}
		return building;
	}

	public static Goal GetMoveWithinRangeOfSquadLeaderGoal(Character character, bool dontOpenOurGates, StayInRangeParams stayInRangeParams)
	{
		stayInRangeParams.CalcStayInRangeDistAndTile(character, out var resultDist, out var resultTile);
		if (resultDist < float.MaxValue)
		{
			Building buildingSquadLeaderIsInOrWantsToBeIn = GetBuildingSquadLeaderIsInOrWantsToBeIn(character);
			if (stayInRangeParams.StayInRangeOf == StayInRangeOf.SquadLeader && character.SquadLeader != null && buildingSquadLeaderIsInOrWantsToBeIn != null)
			{
				if (character.InsideBuilding == buildingSquadLeaderIsInOrWantsToBeIn)
				{
					return null;
				}
				if (FollowGoal.FollowSquadLeaderToAdjoiningBuildingIfPossible(character))
				{
					return null;
				}
				if (buildingSquadLeaderIsInOrWantsToBeIn.CanEnter(character))
				{
					return new MoveToAndEnterBuilding(character, buildingSquadLeaderIsInOrWantsToBeIn, MovementType.Run, dontOpenOurGates);
				}
			}
			float dist = character.Tile.GetDist(resultTile);
			if (stayInRangeParams.StayInRangeOf == StayInRangeOf.SquadLeader && character.SquadLeader != null && buildingSquadLeaderIsInOrWantsToBeIn == null && character.InsideBuilding != null && (!character.IsGuarding() || !(dist <= resultDist + FollowerBuildingHysteresisDist)))
			{
				return new LeaveBuilding(character.InsideBuilding.GetClosestEntranceTo(character.SquadLeader.Tile));
			}
			if (dist > resultDist + FollowerHysteresisDist)
			{
				if (stayInRangeParams.StayInRangeOf == StayInRangeOf.SquadLeader)
				{
					if (dist > MaxDistFromLeader || (buildingSquadLeaderIsInOrWantsToBeIn != null && character.InsideBuilding == null))
					{
						return new MoveWithinRangeOfSquadLeader(MovementType.Run, aiming: false, resultDist, dontOpenOurGates);
					}
					bool aiming = character.WasAiming || character.EquippedItem is Weapon;
					return new MoveWithinRangeOfSquadLeader(MovementType.Jog, aiming, resultDist, dontOpenOurGates);
				}
				if (dist > MaxDistFromLeader)
				{
					return new MoveWithinRange(MovementType.Run, resultTile, aiming: false, 0f, resultDist, dontOpenOurGates, default(StayInRangeParams));
				}
				bool aiming2 = character.WasAiming || character.EquippedItem is Weapon;
				return new MoveWithinRange(MovementType.Jog, resultTile, aiming2, 0f, resultDist, dontOpenOurGates, default(StayInRangeParams));
			}
		}
		return null;
	}

	public override void OnPreActivate(Character character, Goal parent)
	{
		base.OnPreActivate(character, parent);
		character.AboutToBeInCombat = true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (PreferredTarget == null)
		{
			_dontOpenOurGates = !WantOpenGates(character);
		}
		if (parent is SurvivorGoal)
		{
			StayInRangeParams = StayInRangeParams.OfSquadLeaderOrBase(character);
		}
		UpdateCrouching(character, parent);
		AddSubGoal(new AttackFallbackGoal(_dontOpenOurGates, StayInRangeParams));
		AddSubGoal(new MeleeAttack(_movementTypeWhenNotInDanger, _dontOpenOurGates, StayInRangeParams));
		AddSubGoal(new RangedAttack(_movementTypeWhenNotInDanger, assassinate: false, SecrecyMode.Public, _dontOpenOurGates, StayInRangeParams));
		AddSubGoal(new RetreatToBaseGoal());
		AddSubGoal(new FleeGoal(_dontOpenOurGates && character.IsControllableByOrFollowingPlayer()));
		AddSubGoal(new SurrenderGoal());
		character.AboutToBeInCombat = false;
		character.InCombat = true;
		CombatStartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		base.OnActivate(character, parent);
		bool hidingInBuilding = false;
		if (PreferredTarget == null && character.IsAuthoritative() && character.IsControllableByPlayer() && Session.Instance.IsVisibleDeterministic(character.PosXZ) && targetCharacter != null && !character.IsTargetSurrendering(targetCharacter) && !Target.GetFlag(TargetFlags.Inaccessible) && targetCharacter.GetBaseObjectType() == BaseObjectType.Human && character.SparringPartner != targetCharacter && (WantAttackTarget(character, character.Inventory.GetBestWeapon(character, targetCharacter, wantRanged: true, bluntOnly: false) == null, out hidingInBuilding) || hidingInBuilding))
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, hidingInBuilding ? SpeechSituation.HidingInBuilding : SpeechSituation.UnderAttack);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter);
			}
		}
	}

	private bool WantOpenGates(Character character)
	{
		if (character.SparringPartner != null && character.SparringPartner == GetTargetCharacter() && (character.SparringType == SparringType.Feuding || character.SparringType == SparringType.FightToTheDeath || character.IsControllableByOrFollowingPlayer()))
		{
			return true;
		}
		if (character.IsControllableByOrFollowingPlayer())
		{
			if (character.SquadLeader != null && character.SquadLeader.FindActiveGoal(GoalType.Attack) is Attack { _dontOpenOurGates: false })
			{
				return true;
			}
			if (character.HaveIOrAnyoneInMySquadBeenOrderedToAttackSomeone())
			{
				return true;
			}
			return false;
		}
		if (Target != null && Target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && Target.TimeSinceLastAttackedUs < TimeSpan.FromSeconds(10.0) && character.Community != null)
		{
			return character.Community.CountInventoryItemsOfClass(typeof(RangedWeapon), includeBuildings: false) == 0;
		}
		return false;
	}

	public bool WantAttackTarget(Character character, bool melee)
	{
		bool hidingInBuilding;
		return WantAttackTarget(character, melee, out hidingInBuilding);
	}

	public bool WantAttackTarget(Character character, bool melee, out bool hidingInBuilding)
	{
		Character targetCharacter = GetTargetCharacter();
		hidingInBuilding = false;
		if (ChaseTimer >= StopChasingTime)
		{
			return false;
		}
		if ((melee ? (character.InsideBuilding != null) : (!character.IsOutdoors())) && targetCharacter != null && (targetCharacter.Zombie || character.IsControllableByOrFollowingPlayer()) && PreferredTarget != targetCharacter && GetLeaderTarget(character) != Target)
		{
			hidingInBuilding = true;
			return false;
		}
		return true;
	}

	private void UpdateCrouching(Character character, Goal parent)
	{
		if (character.SquadLeader != null && !character.IsOutdoors())
		{
			SetCrouching(character, parent, FollowGoal.WantCrouching(character));
			character.DirectControlledCrouching = Crouching;
		}
		else if (character.DirectControlledCrouching && character.InsideBuilding != null)
		{
			SetCrouching(character, parent, crouching: true);
		}
		else
		{
			SetCrouching(character, parent, crouching: false);
			character.DirectControlledCrouching = false;
		}
	}

	public override void Update(Character character, Goal parent)
	{
		UpdateCrouching(character, parent);
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		if (_dontOpenOurGates && WantOpenGates(character))
		{
			_dontOpenOurGates = false;
			foreach (Target target in character.Targets)
			{
				target.ClearInaccessible();
			}
			foreach (Goal subGoal in SubGoals)
			{
				if (subGoal is MeleeAttack meleeAttack)
				{
					meleeAttack._dontOpenOurGates = false;
				}
				if (subGoal is RangedAttack rangedAttack)
				{
					rangedAttack._dontOpenOurGates = false;
				}
				if (subGoal is AttackFallbackGoal attackFallbackGoal)
				{
					attackFallbackGoal.DontOpenOurGates = false;
				}
				if (subGoal is FleeGoal fleeGoal)
				{
					fleeGoal.DontOpenOurGates = false;
				}
			}
		}
		if (PreferredTarget != null && !(parent is PrioritiserGoal) && Target != null)
		{
			if (SubGoal is FaceTarget && (!Target.Object.IsSusceptibleToBulletHits() || !character.HasGun()) && (!Target.Object.IsFlammable() || !character.HasMolotovCocktail()) && (Target.Object.IsExplosionProof() || !character.HasExplosives()))
			{
				Target.MarkInaccessible();
				Finished = true;
			}
			if (ShouldForgetInaccessibleTarget(character, Target))
			{
				Finished = true;
			}
		}
		if (Target == null || Target.GetFlag(TargetFlags.Lost) || Target.Object == null || Target.Object.IsDestroyed())
		{
			Finished = true;
		}
		TimeSpan timeSpan = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - character.LastThinkTime;
		if (targetCharacter != null && targetCharacter != PreferredTarget && targetCharacter.IsFleeing())
		{
			ChaseTimer += timeSpan;
			if (ChaseTimer >= StopChasingTime && Target.TimeSinceLastDetected >= StopChasingTime && Target.Camouflage >= 0.5f)
			{
				Target.SetFlag(TargetFlags.Lost, on: true);
				character.SetRecentActivity(RecentActivityType.Chase, targetCharacter);
				Finished = true;
			}
		}
		else
		{
			ChaseTimer = TimeSpan.Zero;
		}
		if (SubGoal is AttackFallbackGoal)
		{
			if (GetTargetCharacter() != null)
			{
				FallbackTimer += timeSpan;
			}
			else
			{
				FallbackTimer = TimeSpan.Zero;
			}
		}
		else if (SubGoal is MeleeAttack || SubGoal is RangedAttack)
		{
			FallbackTimer = TimeSpan.Zero;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		character.InCombat = false;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		ChaseTimer = TimeSpan.Zero;
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsAuthoritative())
		{
			targetCharacter.UnderAttackRefCount++;
		}
	}

	protected override void OnDeactivateTarget(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && character.IsAuthoritative())
		{
			targetCharacter.UnderAttackRefCount = Mathf.Max(0, targetCharacter.UnderAttackRefCount - 1);
		}
		base.OnDeactivateTarget(character, parent);
	}

	public void SetMovementTypeWhenNotInDanger(Character character, MovementType movementType)
	{
		_movementTypeWhenNotInDanger = movementType;
		foreach (Goal subGoal in SubGoals)
		{
			RangedAttack rangedAttack = subGoal as RangedAttack;
			MeleeAttack meleeAttack = subGoal as MeleeAttack;
			rangedAttack?.SetMovementTypeWhenNotInDanger(character, movementType);
			meleeAttack?.SetMovementTypeWhenNotInDanger(character, movementType);
		}
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
	}

	public override MovementType GetMovementType()
	{
		return MovementType.Run;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && character.IsCurrentActionAnimFromGoal && !character.IsActionAnimFinished())
		{
			return true;
		}
		if (IsTargetDeleted())
		{
			return false;
		}
		if (Target.Object.GetCommunity() == character.Community && character.SparringPartner != Target.Object)
		{
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter == null || !targetCharacter.Zombie)
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (character.SparringType == SparringType.Feuding && character.SparringPartner == targetCharacter && targetCharacter != null && targetCharacter.Consciousness >= Consciousness.Unconscious)
		{
			return GoalPriority.Survivor_Attack_EnemySurrendering;
		}
		if (Active && character.IsCurrentActionAnimFromGoal && !character.IsActionAnimFinished())
		{
			return GoalPriority.Survivor_Animation;
		}
		if (character.SparringPartner == targetCharacter && character.SparringPartner != null && !character.IsPlayerAvatar() && (character.SparringType == SparringType.Feuding || character.SparringType == SparringType.FightToTheDeath))
		{
			if (!Active)
			{
				return GoalPriority.Survivor_Attack;
			}
			return GoalPriority.Survivor_Attack_Active;
		}
		if (PreferredTarget == null && character.DirectControlledMajorAIDisabled)
		{
			return GoalPriority.Impossible;
		}
		if (character.IsTargetSurrendering(targetCharacter))
		{
			return GoalPriority.Survivor_Attack_EnemySurrendering;
		}
		if (targetCharacter == null && !IsTargetMovingVehicle())
		{
			return GoalPriority.Survivor_Attack_PassiveTarget;
		}
		if (!Active)
		{
			return GoalPriority.Survivor_Attack;
		}
		return GoalPriority.Survivor_Attack_Active;
	}

	public Target GetLeaderTarget(Character character)
	{
		Target target = null;
		if (character.SquadLeader != null && character.SquadLeader.InCombat && character.SquadLeader.IsOutdoors() && character.SquadLeader.FindActiveGoal(GoalType.Attack) is Attack { Finished: false } attack && !attack.IsTargetDeleted() && attack.Target.TimeSinceLastVisible <= Target.LoseTargetTime && attack.Target.Object != character && attack.Target.Object != character.SquadLeader.SparringPartner)
		{
			target = character.GetOrCreateTarget(attack.Target.Object);
			attack.Target.ShareInfoWith(target);
		}
		return target;
	}

	public static bool IsInsideEnclosedBase(Character character)
	{
		TerrainCoord terrainCoord = ((character.InsideBuilding != null) ? character.GetBuildingExitPos(0) : character.Tile);
		return GameTerrain.Instance.IsTileEnclosed(terrainCoord.x, terrainCoord.y);
	}

	public static bool ShouldForgetInaccessibleTarget(Character character, Target target)
	{
		if (target.GetFlag(TargetFlags.Inaccessible))
		{
			if (character.SparringPartner == target.Object)
			{
				return false;
			}
			if (character.IsGuarding() && character.Inventory.GetBestWeapon(character, target.Object, wantRanged: true, bluntOnly: false) != null)
			{
				return false;
			}
			if (!target.GetFlag(TargetFlags.HasLoadedRangedWeapon) && target.TimeSinceLastAttackedUs >= TimeSpan.FromSeconds(20.0))
			{
				bool flag = IsInsideEnclosedBase(character);
				if (flag && character.HasCurrentRoleOutsideBase() && character.GetSleepDeprivation() < Character.SleepDeprivationCriticalTime && character.GetHunger() < Character.HungerCriticalTime && character.GetThirst() < Character.ThirstCriticalTime && character.GetBodyTemperatureInCelsius() > Character.BodyTemperatureInCelsiusModerateHypothermia)
				{
					return false;
				}
				if (flag != GameTerrain.Instance.IsTileEnclosed(target.Object.GetTileX(), target.Object.GetTileY()))
				{
					return true;
				}
				if (!(target.Object is Character character2) || !character2.IsControllableByPlayer())
				{
					Vector3 vector = target.Object.Pos - character.Pos;
					float magnitude = vector.magnitude;
					if (GameTerrain.Instance.IsPassable(new Ray(character.Pos, vector / magnitude), magnitude, 2051, character, target.Object, character.Tile, character.IsPredicted()))
					{
						return false;
					}
					return true;
				}
			}
		}
		return false;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		using (new ProfileMarker(CalcBestTargetTimer))
		{
			if (Active && SubGoal is SurrenderGoal)
			{
				return Target;
			}
			Target leaderTarget = GetLeaderTarget(character);
			Target target = null;
			float num = 1E+38f;
			TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
			foreach (Target target2 in character.Targets)
			{
				if (target2.Object == null || target2.Object.Deleted || target2.GetFlag(TargetFlags.Lost) || target2.Object.IsDestroyed())
				{
					continue;
				}
				TileObject authoritativeOrElseThis = target2.Object.GetAuthoritativeOrElseThis();
				Character character2 = authoritativeOrElseThis as Character;
				if ((character2 == null && authoritativeOrElseThis != PreferredTarget && target2 != leaderTarget && (target2 != Target || !Active)) || (authoritativeOrElseThis != character.SparringPartner && !character.IsAttackableEnemy(authoritativeOrElseThis) && (authoritativeOrElseThis != PreferredTarget || !CanAttackPreferredTargetEvenIfNotEnemy)))
				{
					continue;
				}
				if (authoritativeOrElseThis != character.SparringPartner && authoritativeOrElseThis != PreferredTarget && target2 != leaderTarget)
				{
					if (target2.GetFlag(TargetFlags.Unconscious))
					{
						continue;
					}
					if (character2 != null)
					{
						if (!Active && target2.Camouflage > 0f)
						{
							continue;
						}
						if ((!Active || character.DirectControlledCrouching) && target2.TimeSinceLastAttackedUs >= TimeSpan.FromSeconds(character2.IsControllableByOrFollowingPlayer() ? AlertGoal.HiddenAttackerAlertTime : 10f))
						{
							TimeSpan combatStartTime;
							Character character3 = character2.GetCombatTarget(out combatStartTime) as Character;
							bool flag = false;
							if (character3 != null && currentTime - combatStartTime >= TimeSpan.FromSeconds(0.5))
							{
								if (character3 == character)
								{
									flag = true;
								}
								else if (character.Community != null && character3.Community == character.Community)
								{
									flag = true;
								}
								else if (character3.SquadLeader != null && character3.SquadLeader.Community == character.Community && character.Community != null)
								{
									flag = true;
								}
							}
							if (!character.IsGuarding())
							{
								if (character.IsControllableByOrFollowingPlayer())
								{
									if (character.SquadLeader == null || !(currentTime - character.SquadLeader.LastAttackedSomeoneTime < TimeSpan.FromSeconds(10.0)))
									{
										if (character.DirectControlledCrouching && character.IsOutdoors())
										{
											if (!flag)
											{
												continue;
											}
										}
										else if (!flag)
										{
											float num2 = character2.GetSightRange();
											if ((character2.PosXZ - character.PosXZ).sqrMagnitude > num2 * num2)
											{
												continue;
											}
										}
									}
								}
								else if (character2.Zombie && !flag)
								{
									float num3 = character2.GetSightRange();
									if ((character2.PosXZ - character.PosXZ).sqrMagnitude > num3 * num3)
									{
										continue;
									}
								}
							}
							else if (!flag)
							{
								Weapon bestWeapon = character.Inventory.GetBestWeapon(character, authoritativeOrElseThis, wantRanged: true, bluntOnly: false);
								if (bestWeapon == null)
								{
									continue;
								}
								float rangeIncludingEffects = bestWeapon.GetRangeIncludingEffects(character, authoritativeOrElseThis);
								if ((character2.PosXZ - character.PosXZ).sqrMagnitude > rangeIncludingEffects * rangeIncludingEffects)
								{
									continue;
								}
							}
						}
					}
				}
				else if (character.ShouldIgnoreInvisibleStrainEnemy(authoritativeOrElseThis) || (target2.GetFlag(TargetFlags.Unconscious) && !character.IsControllableByPlayer() && (character.SparringType != SparringType.FightToTheDeath || character.SparringPartner != authoritativeOrElseThis || character.SparringInstigator != character)))
				{
					continue;
				}
				float num4 = character.Get2DDistToTargetAimPos(target2, Vector3.zero) / Character.WalkSpeed;
				float num5 = (float)MathUtil.Min(target2.TimeSinceLastFullyVisible, target2.TimeSinceLastHeardAttack).TotalSeconds + num4;
				if (character2 != null && character2.GetBaseObjectType() != BaseObjectType.Human)
				{
					if (character2.CarriedBy != null || (!character.IsLooter() && !character.HasPersonality(CachedPersonalityType.Sociopath)))
					{
						continue;
					}
					num5 += 1000f;
				}
				num5 *= Mathf.Clamp01((float)target2.TimeSinceLastAttackedMe.TotalSeconds / 10f);
				if (target2 == Target && Active)
				{
					num5 *= 0.75f;
				}
				if (Active ? target2.IsInaccessible() : target2.GetFlag(TargetFlags.Inaccessible))
				{
					if (ShouldForgetInaccessibleTarget(character, target2))
					{
						continue;
					}
					num5 *= 4f * Mathf.Max(0f, 10f - (float)target2.TimeSinceInaccessible.TotalSeconds);
				}
				if (character2 != null)
				{
					if (character2.IsZombieAttacking() && character2.InteractionObject == character)
					{
						num5 *= 0.25f;
					}
					if (character.IsTargetSurrendering(character2))
					{
						num5 *= 10f;
					}
					if (!character2.IsOutdoors() && !(character2.InsideBuilding is EnterableVehicle))
					{
						num5 *= 10f;
					}
					if (character2 == character.SparringPartner && character.SparringType != SparringType.FightToTheDeath)
					{
						num5 *= 2f;
					}
				}
				if (((character2 != null && character2.IsGuarding()) || character2 == null) && character.Inventory.GetBestWeapon(character, authoritativeOrElseThis, wantRanged: true, bluntOnly: false) == null)
				{
					num5 *= 10f;
				}
				Building building = authoritativeOrElseThis as Building;
				if (authoritativeOrElseThis != PreferredTarget && ((building != null && target2 != leaderTarget && building.GetInhabitantCount() == 0) || !(authoritativeOrElseThis is Character)))
				{
					num5 *= 100f;
				}
				if (authoritativeOrElseThis == PreferredTarget)
				{
					num5 *= 0.25f;
				}
				if (target2 == leaderTarget)
				{
					num5 *= 0.25f;
				}
				if (num5 < num)
				{
					target = target2;
					num = num5;
				}
			}
			if (target == null && PreferredTarget != null && character.IsAttackableEnemy(PreferredTarget) && !character.ShouldIgnoreInvisibleStrainEnemy(PreferredTarget))
			{
				Target orCreateTarget = character.GetOrCreateTarget(PreferredTarget);
				if (!orCreateTarget.GetFlag(TargetFlags.Unconscious) || character.IsControllableByPlayer())
				{
					target = orCreateTarget;
				}
			}
			if (target != null && target.Object is Character character4 && !character4.IsOutdoors() && character4.InsideBuilding != null && (target.GetFlag(TargetFlags.InMovingVehicle) || (character4.InsideBuilding.IsFlammable() && character.HasMolotovCocktail()) || (!character4.InsideBuilding.IsExplosionProof() && character.HasExplosives()) || (character4.InsideBuilding.IsSusceptibleToBulletHits() && character.HasGun())))
			{
				target = character.GetOrCreateTarget(character4.InsideBuilding);
			}
			return target;
		}
	}
}
