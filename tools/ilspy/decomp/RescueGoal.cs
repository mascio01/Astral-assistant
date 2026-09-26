using System;
using UnityEngine;

public class RescueGoal : StateMachineGoal
{
	private TimeSpan LastAttemptTime = Target.Never;

	public static TimeSpan MinTimeBetweenAssumingPlayerIsRescuingSomeone = TimeSpan.FromSeconds(120.0);

	private static float MaxDistFromOurBaseToRescueCharacterFromAnotherCommunity = 32f;

	private static float DangerousToRescueMaxDistSqr = MathUtil.Squared(128f);

	public override GoalType GetGoalType()
	{
		return GoalType.RescueGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		CanIRescue(character, targetCharacter, out var lowPrio, out var mediumPrio);
		if (!lowPrio)
		{
			if (!mediumPrio)
			{
				return GoalPriority.Survivor_Rescue;
			}
			return GoalPriority.Survivor_Rescue_MediumPrio;
		}
		return GoalPriority.Survivor_Rescue_LowPrio;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter == null)
		{
			return false;
		}
		if (Session.Instance.PlayTime - LastAttemptTime < TimeSpan.FromSeconds(CanIAccostPlayerCarryingBody(character, targetCharacter) ? 10f : 60f))
		{
			return false;
		}
		if (!Active && character.HasUnbandagedInjury(0))
		{
			return false;
		}
		if (character.HasBeenPlayerControlledRecently(critical: true, extraCritical: true))
		{
			return false;
		}
		if (!Active && (character.DirectControlledCrouching || character.SquadLeader != null || character.CarryingObject != null) && character.IsControllableByPlayer())
		{
			return false;
		}
		if (character.CurrentActionAnim == ActionAnim.HandsUp)
		{
			return false;
		}
		if (!Active && (character.IsTooDepressedToFollowOrders() || targetCharacter.Community != character.Community))
		{
			if ((targetCharacter.PosXZ - character.PosXZ).sqrMagnitude >= MathUtil.Squared(32f))
			{
				return false;
			}
			if (character.HasAnyEnemiesNearby())
			{
				return false;
			}
		}
		if (Active)
		{
			if (character.CarryingObject == GetTargetObject())
			{
				return true;
			}
			if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal)
			{
				_ = moveToAndInteractGoal.SubGoal is InteractAnim;
				return true;
			}
		}
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref LastAttemptTime);
	}

	public bool CanIAccostPlayerCarryingBody(Character character, Character targetCharacter)
	{
		if (targetCharacter.CarriedBy != null && targetCharacter.CarriedBy.Community != character.Community && targetCharacter.CarriedBy.Community.CommunityType == CommunityType.Player && targetCharacter.GetBaseObjectType() == BaseObjectType.Human && Session.Instance.PlayTime - targetCharacter.LastRescuedByPlayer >= MinTimeBetweenAssumingPlayerIsRescuingSomeone && !character.IsControllableByPlayer() && !character.IsEnemy(targetCharacter.CarriedBy) && !IsSuspiciousOfPlayerCarryingBody(character, targetCharacter.CarriedBy, targetCharacter))
		{
			return character.GetTarget(targetCharacter.CarriedBy)?.FullyTracked ?? false;
		}
		return false;
	}

	public static bool IsSuspiciousOfPlayerCarryingBody(Character character, Character carrier, Character body)
	{
		if (!character.HasMemoryAfter(MemoryPrototype.ThreatenedSuccessfully, body, carrier, Session.Instance.PlayTime - MinTimeBetweenAssumingPlayerIsRescuingSomeone) && !character.HasMemoryWhereObjectIsAnyoneInCommunityAfter(MemoryPrototype.Killed, carrier, character.Community, Session.Instance.PlayTime - MinTimeBetweenAssumingPlayerIsRescuingSomeone) && !character.HasMemoryWhereObjectIsAnyoneInCommunityAfter(MemoryPrototype.KnockedOut, carrier, character.Community, Session.Instance.PlayTime - MinTimeBetweenAssumingPlayerIsRescuingSomeone))
		{
			return character.HasMemoryWhereObjectIsAnyoneInCommunityAfter(MemoryPrototype.ThreatenedSuccessfully, carrier, character.Community, Session.Instance.PlayTime - MinTimeBetweenAssumingPlayerIsRescuingSomeone);
		}
		return true;
	}

	public bool CanIRescue(Character character, Character targetCharacter, out bool lowPrio)
	{
		bool mediumPrio;
		return CanIRescue(character, targetCharacter, out lowPrio, out mediumPrio);
	}

	public bool CanIRescue(Character character, Character targetCharacter, out bool lowPrio, out bool mediumPrio)
	{
		lowPrio = false;
		mediumPrio = false;
		if (CanIAccostPlayerCarryingBody(character, targetCharacter))
		{
			return true;
		}
		if (!targetCharacter.AliveAndNotZombie)
		{
			return false;
		}
		if (targetCharacter.CarriedBy != null)
		{
			return false;
		}
		if (targetCharacter.InsideBuilding != null)
		{
			return false;
		}
		if (CanBringCharacterBackToOurBase(character, targetCharacter))
		{
			return true;
		}
		if (targetCharacter.HasUnbandagedInjury(0))
		{
			if (character.Inventory.FindItemWithHighestBandageLevel(0) == null)
			{
				mediumPrio = true;
			}
			return true;
		}
		InfectionType worstInfectionTypeInProgression = targetCharacter.GetWorstInfectionTypeInProgression();
		if (worstInfectionTypeInProgression != InfectionType.None && character.Inventory.FindAntigenForInfectionType(worstInfectionTypeInProgression) != null)
		{
			return true;
		}
		if (targetCharacter.BodyTemperatureInCelsius < Character.BodyTemperatureInCelsiusMildHypothermia && targetCharacter.IsOutdoors() && targetCharacter.Rank != Rank.Captive && targetCharacter.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) == null)
		{
			return true;
		}
		if (targetCharacter.GetThirst() >= Character.ThirstyTime || targetCharacter.GetHunger() >= Character.HungryTime)
		{
			lowPrio = targetCharacter.GetThirst() < Character.ThirstExtraCriticalTime && targetCharacter.GetHunger() < Character.HungerExtraCriticalTime;
			return true;
		}
		if (targetCharacter.HasUnbandagedInjury(1) && character.GetSkillLevelWithEffects(SkillType.Medicine) >= 1)
		{
			lowPrio = true;
			return true;
		}
		return false;
	}

	private bool AreThereEnemiesInsideOurBase(Character character)
	{
		foreach (Target target in character.Targets)
		{
			if (target.Object is Character character2 && character2.Community != character.Community && character2.IsAwake && character.Community != null && character.Community.IsTileInsidePerimeter(character2.Tile) && character.IsEnemy(character2))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanBringCharacterBackToOurBase(Character character, Character targetCharacter)
	{
		if (character.Community.IsAISettlement() && character.Community.BaseRect != TerrainRect.Invalid && (targetCharacter.Community == character.Community || character.Community.BaseRect.GetClosestDistSqTo(targetCharacter.Tile) < MaxDistFromOurBaseToRescueCharacterFromAnotherCommunity * MaxDistFromOurBaseToRescueCharacterFromAnotherCommunity) && (!character.Community.IsTileInsidePerimeter(targetCharacter.Tile, excludeWallAndGateTiles: true) || GetDistToNearestCampfireInOurBase(character, targetCharacter) > TileObject.MaxFireHeatRange) && !AreThereEnemiesInsideOurBase(character) && !IsAmbushing(character) && targetCharacter.Rank != Rank.Captive && character.Community.HasAnyBuildingOfType(BaseObjectType.Campfire))
		{
			return true;
		}
		return false;
	}

	private bool IsAmbushing(Character character)
	{
		Squad squad = character.GetSquad();
		if (squad != null && squad.Behaviour == SquadBehaviour.Ambush && squad.Action != SquadAction.GoToHighPrio)
		{
			return true;
		}
		return false;
	}

	public bool IsCarryingBodyHome(Character character)
	{
		if (!(SubGoal is MoveToAndPickUp))
		{
			return SubGoal is MoveWithinRange;
		}
		return true;
	}

	private bool DoIWantToRescue(Character character, Character targetCharacter)
	{
		if (targetCharacter.Community == character.Community)
		{
			return true;
		}
		if (character.IsControllableByPlayer())
		{
			return false;
		}
		if (character.IsEnemy(targetCharacter))
		{
			return false;
		}
		if (IsAmbushing(character))
		{
			return false;
		}
		if (!character.HasPersonality(CachedPersonalityType.Sociopath))
		{
			if (character.InitialCommunity == targetCharacter.Community || character.Community == targetCharacter.InitialCommunity)
			{
				return true;
			}
			if (character.InitialCommunity == targetCharacter.InitialCommunity && character.InitialCommunity != null)
			{
				return true;
			}
		}
		if (targetCharacter.CarriedBy != null)
		{
			return false;
		}
		if (!targetCharacter.Alive)
		{
			return false;
		}
		if (!character.HasPersonality(CachedPersonalityType.Compassionate))
		{
			return false;
		}
		if (Session.Instance.CommunityManager.PlayerCommunity.AreAnyMembersInCombat())
		{
			return false;
		}
		return true;
	}

	public override Target CalcBestTarget(Character character, Goal parent)
	{
		if (Active && !IsTargetDeleted())
		{
			if (character.CarryingObject == GetTargetObject())
			{
				return Target;
			}
			if (SubGoal is MoveToAndInteractGoal moveToAndInteractGoal && moveToAndInteractGoal.SubGoal is InteractAnim)
			{
				return Target;
			}
		}
		if (character.Rank == Rank.Captive || character.Community == null)
		{
			return null;
		}
		if (character.SparringType == SparringType.FightToTheDeath || character.SparringType == SparringType.Feuding)
		{
			return null;
		}
		Target result = null;
		float num = 1E+38f;
		foreach (Target target in character.Targets)
		{
			if (target.Object == null || target.Object.Deleted || !(target.Object is Character character2))
			{
				continue;
			}
			if (character2.Zombie && character2.Alive && character2.InCombat && target.FullyTracked && !target.GetFlag(TargetFlags.Inaccessible) && MathUtil.ToXZ(character.Pos - target.LastKnownPosition).magnitude < 16f)
			{
				return null;
			}
			if (character2.InsideBuilding != null && character2.InsideBuilding.IsMovingVehicle() && !Active && MathUtil.ToXZ(character.Pos - target.LastKnownPosition).magnitude < 64f && character.IsEnemy(character2))
			{
				return null;
			}
			if (((!target.GetFlag(TargetFlags.Unconscious) || !character2.AliveAndNotZombie) && !CanIAccostPlayerCarryingBody(character, character2)) || (character2.BeingRescuedBy != null && character2.BeingRescuedBy != character) || !DoIWantToRescue(character, character2) || (character.HasMovementZone() && !character.MovementZone.Contains(character2.Tile)) || (character2.SparringPartner != null && character2.SparringType == SparringType.FightToTheDeath) || !CanIRescue(character, character2, out var lowPrio))
			{
				continue;
			}
			if (Session.Instance.PlayTime - character2.DangerousToRescueMeTime < TimeSpan.FromSeconds(600.0) && character2.DangerousToRescueMeBecause != null && !character2.DangerousToRescueMeBecause.Deleted && character2.DangerousToRescueMeBecause.Community != null && character.Community.GetRelationship(character2.DangerousToRescueMeBecause.Community) == CommunityRelationshipType.Hostile)
			{
				bool flag = false;
				foreach (Character member in character2.DangerousToRescueMeBecause.Community.Members)
				{
					if (member.IsAwake && !member.Zombie && member.GetBaseObjectType() == BaseObjectType.Human && MathUtil.GetDistSqFromSegmentToPoint(character.PosXZ, character2.PosXZ, member.PosXZ, out var _) < DangerousToRescueMaxDistSqr)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			float num2 = character.Get2DDistToTargetLastKnownPos(target) / Character.WalkSpeed;
			if (target == Target && Active)
			{
				num2 *= 0f;
			}
			if (lowPrio)
			{
				num2 += 100f;
			}
			if (num2 < num)
			{
				result = target;
				num = num2;
			}
		}
		return result;
	}

	protected override void OnActivateTarget(Character character, Goal parent)
	{
		base.OnActivateTarget(character, parent);
		character.InCombat = true;
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null)
		{
			targetCharacter.BeingRescuedBy = character;
			if (CanIAccostPlayerCarryingBody(character, targetCharacter))
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter.CarriedBy, targetCharacter, SpeechSituation.DropTheBody);
				if (speechForSituation != null)
				{
					SetSubGoal(character, parent, new Conversation(character, targetCharacter.CarriedBy, targetCharacter, speechForSituation, controlledByPlayer: false));
					return;
				}
			}
			CanIRescue(character, targetCharacter, out var lowPrio);
			if (lowPrio)
			{
				character.InCombat = false;
			}
			else if (Session.Instance.PlayTime - LastAttemptTime >= TimeSpan.FromSeconds(60.0))
			{
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.Rescuing);
				if (speechForSituation2 != null)
				{
					character.Speak(speechForSituation2, targetCharacter);
				}
			}
			if (character.Community.IsAISettlement() && character.Community.BaseRect != TerrainRect.Invalid && !character.Community.IsTileInsidePerimeter(targetCharacter.Tile) && targetCharacter.Rank != Rank.Captive && !IsAmbushing(character) && character.HasAnyEnemiesNearby())
			{
				MoveToAndPickUp moveToAndPickUp = new MoveToAndPickUp(MovementType.Run);
				moveToAndPickUp.CanUnlockGatesFromInsideWithKey = true;
				SetSubGoal(character, parent, moveToAndPickUp);
				return;
			}
			Goal bandageOrInjectGoal = GetBandageOrInjectGoal(character, parent, targetCharacter);
			if (bandageOrInjectGoal != null)
			{
				SetSubGoal(character, parent, bandageOrInjectGoal);
				return;
			}
			if (CanBringCharacterBackToOurBase(character, targetCharacter))
			{
				MoveToAndPickUp moveToAndPickUp2 = new MoveToAndPickUp(MovementType.Run);
				moveToAndPickUp2.CanUnlockGatesFromInsideWithKey = true;
				SetSubGoal(character, parent, moveToAndPickUp2);
				return;
			}
			if (targetCharacter.BodyTemperatureInCelsius < Character.BodyTemperatureInCelsiusMildHypothermia && targetCharacter.IsOutdoors() && targetCharacter.Rank != Rank.Captive && targetCharacter.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) == null)
			{
				MoveToAndPickUp moveToAndPickUp3 = new MoveToAndPickUp(MovementType.Run);
				moveToAndPickUp3.CanUnlockGatesFromInsideWithKey = true;
				SetSubGoal(character, parent, moveToAndPickUp3);
				return;
			}
			if (targetCharacter.GetThirst() >= Character.ThirstyTime)
			{
				bool flag = targetCharacter.GetThirst() >= Character.ThirstExtraCriticalTime;
				Equipment bestWaterBottleToDrink = character.Inventory.GetBestWaterBottleToDrink(character, targetCharacter, forSharing: false);
				if (bestWaterBottleToDrink != null)
				{
					MoveToAndInteractGoal moveToAndInteractGoal = new MoveToAndInteractGoal(InteractionType.GiveWater, bestWaterBottleToDrink, (!flag) ? MovementType.Walk : MovementType.Run);
					moveToAndInteractGoal.CanUnlockGatesFromInsideWithKey = true;
					SetSubGoal(character, parent, moveToAndInteractGoal);
				}
				else
				{
					SetSubGoal(character, parent, new FindGoal(FindType.WaterForAnimals, (!flag) ? MovementType.Walk : MovementType.Run, flag));
				}
				return;
			}
			if (targetCharacter.GetHunger() >= Character.HungryTime)
			{
				bool flag2 = targetCharacter.GetHunger() >= Character.HungerExtraCriticalTime;
				Equipment food = character.Inventory.GetFood(character, targetCharacter, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false);
				if (food != null)
				{
					MoveToAndInteractGoal moveToAndInteractGoal2 = new MoveToAndInteractGoal(InteractionType.Feed, food, (!flag2) ? MovementType.Walk : MovementType.Run);
					moveToAndInteractGoal2.CanUnlockGatesFromInsideWithKey = true;
					SetSubGoal(character, parent, moveToAndInteractGoal2);
				}
				else
				{
					SetSubGoal(character, parent, new FindGoal(FindType.FoodForJourney, (!flag2) ? MovementType.Walk : MovementType.Run, flag2));
				}
				return;
			}
			if (targetCharacter.HasUnbandagedInjury(1) && character.GetSkillLevelWithEffects(SkillType.Medicine) >= 1)
			{
				if (character.Inventory.FindItemWithHighestBandageLevel(1) != null)
				{
					MoveToAndInteractGoal moveToAndInteractGoal3 = new MoveToAndInteractGoal(InteractionType.ApplyBandageAndSpeak, null, MovementType.Walk);
					moveToAndInteractGoal3.CanUnlockGatesFromInsideWithKey = true;
					SetSubGoal(character, parent, moveToAndInteractGoal3);
				}
				else
				{
					SetSubGoal(character, parent, new FindGoal(FindType.GoodBandage, MovementType.Walk, critical: false));
				}
				return;
			}
		}
		LastAttemptTime = Session.Instance.PlayTime;
		Finished = true;
	}

	private Goal GetBandageOrInjectGoal(Character character, Goal parent, Character targetCharacter)
	{
		if (targetCharacter.HasUnbandagedInjury(0))
		{
			if (character.Inventory.FindItemWithHighestBandageLevel(0) != null)
			{
				return new MoveToAndInteractGoal(InteractionType.ApplyBandageAndSpeak, null, MovementType.Run)
				{
					CanUnlockGatesFromInsideWithKey = true
				};
			}
			return new FindGoal(FindType.Bandage, MovementType.Run, critical: true);
		}
		InfectionType worstInfectionTypeInProgression = targetCharacter.GetWorstInfectionTypeInProgression();
		if (worstInfectionTypeInProgression != InfectionType.None)
		{
			Equipment equipment = character.Inventory.FindAntigenForInfectionType(worstInfectionTypeInProgression);
			if (equipment != null)
			{
				return new MoveToAndInteractGoal(InteractionType.GiveAntigen, equipment, MovementType.Run)
				{
					CanUnlockGatesFromInsideWithKey = true
				};
			}
			EquipmentPrototype equipmentPrototype = null;
			switch (worstInfectionTypeInProgression)
			{
			case InfectionType.Green:
				equipmentPrototype = EquipmentPrototype.Antigen_Green;
				break;
			case InfectionType.Blue:
				equipmentPrototype = EquipmentPrototype.Antigen_Blue;
				break;
			case InfectionType.Red:
				equipmentPrototype = EquipmentPrototype.Antigen_Red;
				break;
			}
			if (equipmentPrototype != null)
			{
				return new FindGoal(FindType.Prototype, equipmentPrototype, MovementType.Run, !character.IsControllableByPlayer());
			}
		}
		return null;
	}

	protected override void OnDeactivateTarget(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (targetCharacter != null && targetCharacter.BeingRescuedBy == character)
		{
			targetCharacter.BeingRescuedBy = null;
		}
		if (character.CarryingObject == GetTargetObject())
		{
			character.DropAuthoritative();
		}
		character.InCombat = false;
		base.OnDeactivateTarget(character, parent);
	}

	public float GetDistToNearestCampfireInOurBase(Character character, Character targetCharacter)
	{
		TerrainCoord tile = targetCharacter.Tile;
		float num = float.MaxValue;
		foreach (Prop building in character.Community.Buildings)
		{
			if (building.GetBaseObjectType() == BaseObjectType.Campfire && character.Community.IsTileInsidePerimeter(building.Tile))
			{
				float dist = building.Tile.GetDist(tile);
				if (dist < num)
				{
					num = dist;
				}
			}
		}
		return num;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		Character targetCharacter = GetTargetCharacter();
		if (SubGoal is MoveToAndPickUp { Success: not false })
		{
			bool flag = character.Community != null && character.Community.HasAnyBuildingOfType(BaseObjectType.Campfire);
			Vector2 posXZ = character.PosXZ;
			float num = float.MaxValue;
			float maxRange = TileObject.FireIdealWarmthRange;
			Prop prop = null;
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (allProp.GetBaseObjectType() != BaseObjectType.Campfire || (character.Community.IsAISettlement() && character.Community.BaseRect != TerrainRect.Invalid && flag && !character.Community.IsTileInsidePerimeter(allProp.Tile)))
				{
					continue;
				}
				float num2 = (allProp.PosXZ - posXZ).magnitude;
				if (allProp.Community != character.Community)
				{
					if (character.Community.CachedAllies.Contains(allProp.Community))
					{
						num2 += 100f;
					}
					else
					{
						if (character.IsEnemy(allProp))
						{
							continue;
						}
						num2 += 200f;
					}
				}
				if (allProp.IsBurning())
				{
					num2 -= 50f;
				}
				if (!(num2 < num))
				{
					continue;
				}
				float num3 = TileObject.FireIdealWarmthRange - 2f;
				if (character.Community.IsAISettlement() && character.Community.BaseRect != TerrainRect.Invalid && flag)
				{
					GameTerrain instance = GameTerrain.Instance;
					while (num3 > 1f && !character.Community.IsTileInsidePerimeter(instance.GetTileCoordForPosXZ(allProp.PosXZ + MathUtil.SafeNormalize(character.PosXZ - allProp.PosXZ, Vector2.zero) * num3), excludeWallAndGateTiles: true))
					{
						num3 -= 1f;
					}
				}
				if (!(num3 < 1.5f))
				{
					prop = allProp;
					num = num2;
					maxRange = num3;
				}
			}
			if (prop != null)
			{
				return new MoveWithinRange(MovementType.Run, prop.Tile, aiming: false, 0f, maxRange, dontOpenOurGates: false, default(StayInRangeParams))
				{
					AvoidHostileBases = true,
					CanUnlockGatesFromInsideWithKey = true
				};
			}
		}
		if (SubGoal is MoveWithinRange { Success: not false } moveWithinRange && targetCharacter != null)
		{
			character.DropAuthoritative();
			character.SetRecentActivity(RecentActivityType.CaredFor, targetCharacter);
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, targetCharacter, SpeechSituation.DroppedByFire);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation, targetCharacter);
			}
			if (targetCharacter.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusMildHypothermia && GameTerrain.Instance.GetFixedObjectOnTile(moveWithinRange.DestTile.x, moveWithinRange.DestTile.y) is Campfire campfire && !campfire.IsBurning())
			{
				return new LightFireGoal(character, null, campfire, MovementType.Run, canAddMaterialToFire: true);
			}
			return null;
		}
		if (SubGoal is FindGoal { Success: not false } findGoal && targetCharacter != null)
		{
			switch (findGoal.FindType)
			{
			case FindType.Bandage:
			case FindType.GoodBandage:
				return new MoveToAndInteractGoal(InteractionType.ApplyBandageAndSpeak, null, MovementType.Run)
				{
					CanUnlockGatesFromInsideWithKey = true
				};
			case FindType.Prototype:
			{
				InfectionType worstInfectionTypeInProgression = targetCharacter.GetWorstInfectionTypeInProgression();
				if (worstInfectionTypeInProgression != InfectionType.None)
				{
					Equipment equipment = character.Inventory.FindAntigenForInfectionType(worstInfectionTypeInProgression);
					if (equipment != null)
					{
						return new MoveToAndInteractGoal(InteractionType.GiveAntigen, equipment, MovementType.Run)
						{
							CanUnlockGatesFromInsideWithKey = true
						};
					}
				}
				break;
			}
			case FindType.WaterForAnimals:
			{
				Equipment bestWaterBottleToDrink = character.Inventory.GetBestWaterBottleToDrink(character, targetCharacter, forSharing: false);
				if (bestWaterBottleToDrink != null)
				{
					return new MoveToAndInteractGoal(InteractionType.GiveWater, bestWaterBottleToDrink, (!(targetCharacter.GetThirst() >= Character.ThirstExtraCriticalTime)) ? MovementType.Walk : MovementType.Run)
					{
						CanUnlockGatesFromInsideWithKey = true
					};
				}
				break;
			}
			case FindType.FoodForJourney:
			{
				Equipment food = character.Inventory.GetFood(character, targetCharacter, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false);
				if (food != null)
				{
					return new MoveToAndInteractGoal(InteractionType.Feed, food, (!(targetCharacter.GetHunger() >= Character.HungerExtraCriticalTime)) ? MovementType.Walk : MovementType.Run)
					{
						CanUnlockGatesFromInsideWithKey = true
					};
				}
				break;
			}
			}
		}
		if (SubGoal is MoveToAndInteractGoal { Success: not false })
		{
			character.SetRecentActivity(RecentActivityType.CaredFor, targetCharacter);
			return null;
		}
		LastAttemptTime = Session.Instance.PlayTime;
		return null;
	}

	public override void Update(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinRange)
		{
			Character targetCharacter = GetTargetCharacter();
			if (character.CarryingObject != targetCharacter)
			{
				Finished = true;
			}
			else if (!character.HasAnyEnemiesNearby())
			{
				Goal bandageOrInjectGoal = GetBandageOrInjectGoal(character, parent, targetCharacter);
				if (bandageOrInjectGoal != null)
				{
					SetSubGoal(character, parent, bandageOrInjectGoal);
				}
			}
		}
		base.Update(character, parent);
	}

	public override void OnDamaged(Character character, Goal parent, Character source, InjuryLocation injuryLocation, bool absorbedByVest)
	{
		if (source != null && !source.Zombie && (!absorbedByVest || source.DirectControlled))
		{
			LastAttemptTime = Session.Instance.PlayTime;
			Character targetCharacter = GetTargetCharacter();
			if (targetCharacter != null)
			{
				character.DangerousToRescueMeTime = (targetCharacter.DangerousToRescueMeTime = Session.Instance.PlayTime);
				character.DangerousToRescueMeBecause = (targetCharacter.DangerousToRescueMeBecause = source);
			}
		}
		base.OnDamaged(character, parent, source, injuryLocation, absorbedByVest);
	}
}
