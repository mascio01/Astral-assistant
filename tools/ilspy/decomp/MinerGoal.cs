using System;
using UnityEngine;

public class MinerGoal : RoleGoal
{
	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private bool HasSaidNeedPickaxe;

	public EquipmentPrototype CurrentMiningResourceType;

	private static TimeSpan RememberFailedAttemptTime = TimeSpan.FromSeconds(60.0);

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.MinerGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorPickaxe;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active)
		{
			if (character.IsMining() || SubGoal is Wait)
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			int roleIndex = character.GetRoleIndex(new RoleInfo(Role.Miner, CurrentMiningResourceType));
			if (roleIndex != -1)
			{
				return (GoalPriority)(210 - roleIndex);
			}
		}
		return (GoalPriority)(210 - character.GetFirstRunningRoleIndex(Role.Miner));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if ((!Active || !(SubGoal is Wait)) && !character.HasRunningRole(Role.Miner))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Miner))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - _lastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastAttemptedTime);
		reflector.Add(ref MovementType);
		reflector.AddAfter(ref HasSaidNeedPickaxe, 179);
		reflector.AddAfter(ref CurrentMiningResourceType, 352);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role == Role.Miner && !character.Roles[i].Paused)
			{
				character.Inventory.FindItemOfType(character.Roles[i].ResourceType)?.SetGathered();
			}
		}
		Goal nextSubGoal = GetNextSubGoal(character, parent);
		if (nextSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, nextSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
	}

	private void OnFailed(Character character, EquipmentPrototype resourceType)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Miner, resourceType));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: false })
		{
			OnFailed(character, CurrentMiningResourceType);
			return null;
		}
		if (SubGoal is MoveToAndMine { Success: false } moveToAndMine && !moveToAndMine.IsTargetDeleted())
		{
			Building targetBuilding = moveToAndMine.GetTargetBuilding();
			if (targetBuilding == null || targetBuilding.CanEnterReason(character) != CursorActionDisabledReason.BuildingFull)
			{
				character.AddFailedFindAttempt(moveToAndMine.GetTargetObject(), moveToAndMine.EntranceIndex, 0, FindType.Prototype, CurrentMiningResourceType, null, null);
			}
			OnFailed(character, CurrentMiningResourceType);
			return null;
		}
		if (SubGoal is MoveToAndGrab { Success: false } moveToAndGrab && !moveToAndGrab.IsTargetDeleted())
		{
			TileObject targetObject = moveToAndGrab.GetTargetObject();
			if (targetObject == null)
			{
				character.AddFailedFindAttempt(targetObject, 0, 0, FindType.Prototype, CurrentMiningResourceType, null, null);
			}
			OnFailed(character, CurrentMiningResourceType);
			return null;
		}
		if (SubGoal is FindGoal { Success: false })
		{
			if (!HasSaidNeedPickaxe)
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.NeedPickaxe);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation);
				}
				character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				HasSaidNeedPickaxe = true;
			}
			OnFailed(character, CurrentMiningResourceType);
			return null;
		}
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role != Role.Miner || character.Roles[i].Paused || character.Roles[i].FailedRecently)
			{
				continue;
			}
			EquipmentPrototype resourceType = character.Roles[i].ResourceType;
			TerrainCoord targetLocation = character.Roles[i].TargetLocation;
			if (resourceType == null)
			{
				continue;
			}
			float maxInventoryWeight = character.GetMaxInventoryWeight();
			int num = Math.Max(1, Math.Min(10, Mathf.CeilToInt(maxInventoryWeight * 0.5f / resourceType.Weight)));
			bool controllableByPlayer = character.IsControllableByPlayer();
			int num2 = Math.Max(0, character.Inventory.CountItemsOfType(resourceType) - (int)character.GetTargetAmountToCarryIncludingAmmo(resourceType, null, InfectionType.None));
			bool flag = (character.Community == null || character.Community.CountInventoryItemsOfType(resourceType) < character.Community.GetCraftingLimit(resourceType)) && num2 < num;
			bool flag2 = num2 > 0;
			bool flag3 = character.HasInventorySpaceFor(resourceType.Weight) || (!controllableByPlayer && !flag2);
			bool mineIsFull = false;
			TileObject bestRock = null;
			if (flag3 && flag)
			{
				if (character.Inventory.GetPickaxe() == null)
				{
					CurrentMiningResourceType = resourceType;
					MovementType = ((!character.Roles[i].Urgent) ? MovementType.Walk : MovementType.Run);
					return new FindGoal(FindType.Pickaxe, MovementType, critical: false);
				}
				FindGoal.BuildListOfEscapedFromTargets(character);
				TerrainCoord startTile = ((targetLocation != TerrainCoord.Invalid) ? targetLocation : character.Tile);
				TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(startTile.x, startTile.y);
				if (fixedObjectOnTile != null && (fixedObjectOnTile.GetMiningResourceType() == resourceType || (fixedObjectOnTile is Mine && ((Mine)fixedObjectOnTile).CanEnter(character))) && !character.HasFailedFindAttempt(fixedObjectOnTile, 0, 0, FindType.Prototype, resourceType, null, null, FindGoal.RememberFailedAttemptTime, out var _, out var _) && !FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, fixedObjectOnTile.Pos))
				{
					bestRock = fixedObjectOnTile;
				}
				else
				{
					float bestDist = GameTerrain.Instance.Size * 2;
					foreach (Prop allProp in Session.Instance.PropManager.AllProps)
					{
						float num3 = CalcCost(character, allProp, controllableByPlayer, startTile, resourceType, ref mineIsFull);
						if (num3 < bestDist)
						{
							bestRock = allProp;
							bestDist = num3;
						}
					}
					instance.RockMapWho.GetNearestObject(startTile, Mathf.CeilToInt(bestDist), delegate(SingleTileObject rock)
					{
						if (GameTerrain.Instance.IsSlopeOrImpassableRaw(rock.GetTileX(), rock.GetTileY()))
						{
							return false;
						}
						float num4 = CalcCost(character, rock, controllableByPlayer, startTile, resourceType, ref mineIsFull);
						if (num4 < bestDist)
						{
							bestRock = rock;
							bestDist = num4;
							return true;
						}
						return false;
					});
				}
				FindGoal.ClearListOfEscapedFromTargets();
			}
			MovementType = ((!character.Roles[i].Urgent) ? MovementType.Walk : MovementType.Run);
			CurrentMiningResourceType = resourceType;
			character.SetRoleInProgress(i, inProgress: true);
			float idealWeightToFree = (flag2 ? float.MaxValue : (resourceType.Weight * (float)num));
			Equipment bestItem;
			int amountToDeposit;
			bool failed;
			Prop moveToAndDepositProp = GatherGoal.GetMoveToAndDepositProp(character, resourceType.Weight, idealWeightToFree, out bestItem, out amountToDeposit, out failed);
			if (bestRock != null && resourceType != null)
			{
				if (moveToAndDepositProp != null)
				{
					TerrainCoord tile = character.Tile;
					if (moveToAndDepositProp != null && moveToAndDepositProp.Tile.GetDistSquared(tile) < bestRock.GetCentreTile().GetDistSquared(tile))
					{
						return new MoveToAndDeposit(character, moveToAndDepositProp, bestItem, amountToDeposit, MovementType)
						{
							WasGathered = (bestItem.GetGatheredAmount() > 0)
						};
					}
				}
				if (bestRock.GetGrabbableEquipmentType() == resourceType)
				{
					return new MoveToAndGrab(character, bestRock, MovementType)
					{
						IsGathering = true
					};
				}
				HasSaidNeedPickaxe = false;
				return new MoveToAndMine(character, bestRock, resourceType.GetMineralType(), MovementType, FindGoal.CalcDontOpenOurGates(character))
				{
					IsGathering = true
				};
			}
			if (moveToAndDepositProp != null)
			{
				return new MoveToAndDeposit(character, moveToAndDepositProp, bestItem, amountToDeposit, MovementType)
				{
					WasGathered = (bestItem.GetGatheredAmount() > 0)
				};
			}
			if (!controllableByPlayer)
			{
				OnFailed(character, resourceType);
				return null;
			}
			if (!flag2 && !flag3)
			{
				MemoryParam param = new MemoryParam(resourceType);
				Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
				character.Speak(speechForSituation2, null, null, param);
				character.PauseRole(new RoleInfo(Role.Miner, resourceType));
				return new Wait(TimeSpan.FromSeconds(5.0));
			}
			if (flag2)
			{
				MemoryParam param2 = new MemoryParam(resourceType);
				Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.NoGatherDepotFound, param2);
				character.Speak(speechForSituation3, null, null, param2);
				character.PauseRole(new RoleInfo(Role.Miner, resourceType));
				return new Wait(TimeSpan.FromSeconds(5.0));
			}
			if (!flag)
			{
				OnFailed(character, resourceType);
				return null;
			}
			if (mineIsFull)
			{
				OnFailed(character, resourceType);
				return null;
			}
			Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.MiningComplete);
			character.Speak(speechForSituation4);
			character.CancelRole(new RoleInfo(Role.Miner, resourceType));
			return null;
		}
		_lastAttemptedTime = Session.Instance.PlayTime;
		return null;
	}

	private static float CalcCost(Character character, TileObject rock, bool controllableByPlayer, TerrainCoord startTile, EquipmentPrototype resourceType, ref bool mineIsFull)
	{
		if (rock.GetMiningResourceType() != resourceType && rock.GetGrabbableEquipmentType() != resourceType && rock.GetBaseObjectType() != BaseObjectType.Mine)
		{
			return float.MaxValue;
		}
		if (controllableByPlayer && !GameTerrain.Instance.FogOfWar.IsAnyTileInRectCornersExplored(rock.GetMinTile(), rock.GetMaxTile()))
		{
			return float.MaxValue;
		}
		if (character.HasMovementZone() && !character.MovementZone.Overlaps(rock.GetTileRect()))
		{
			return float.MaxValue;
		}
		Community communityThatOwnsThisArea = rock.GetCommunityThatOwnsThisArea();
		if (communityThatOwnsThisArea != null && communityThatOwnsThisArea != character.Community && communityThatOwnsThisArea.HasAnyLivingNonZombieMembers() && !character.Community.CachedAllies.Contains(communityThatOwnsThisArea))
		{
			return float.MaxValue;
		}
		if (rock.GetBaseObjectType() == BaseObjectType.Mine)
		{
			Mine mine = (Mine)rock;
			if (!mine.HasRichDeposits(resourceType.GetMineralType()))
			{
				return float.MaxValue;
			}
			if (!mine.CanEnter(character))
			{
				mineIsFull = true;
				return float.MaxValue;
			}
		}
		else if (!GameTerrain.Instance.IsAnySurroundingTileSpawnable(rock.GetTileRect()))
		{
			return float.MaxValue;
		}
		if (FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, rock.Pos))
		{
			return float.MaxValue;
		}
		float num = rock.GetCentreTile().GetDist(startTile);
		if (rock.GetGrabbableEquipmentType() != resourceType)
		{
			num += AnimationManager.Instance.Anims[141][0].Clip.length * (float)rock.GetMiningProgressNeededToExtract(resourceType.GetMineralType()) * Character.WalkSpeed;
		}
		if (character.HasFailedFindAttempt(rock, 0, 0, FindType.Prototype, resourceType, null, null, FindGoal.RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
		{
			num += (float)MathUtil.Squared(failCount) * FindGoal.CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
		}
		return num;
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
		HasSaidNeedPickaxe = false;
	}

	public void SetLastAttemptedTimeToNow()
	{
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Miner, CurrentMiningResourceType);
	}
}
