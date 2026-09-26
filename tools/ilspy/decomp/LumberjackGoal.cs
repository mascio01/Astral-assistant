using System;
using UnityEngine;

public class LumberjackGoal : RoleGoal
{
	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private bool HasSaidNeedAxe;

	private static TimeSpan RememberFailedAttemptTime = Sun.DayLength;

	private static float CostOfFailure = 10f;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.LumberjackGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorAxe;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && character.IsChoppingWood())
		{
			return GoalPriority.Survivor_Role_Animation;
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Lumberjack));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.Lumberjack))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Lumberjack))
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
		reflector.AddAfter(ref HasSaidNeedAxe, 179);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		for (int i = 0; i < character.Roles.Count; i++)
		{
			if (character.Roles[i].Role == Role.Lumberjack && !character.Roles[i].Paused && EquipmentPrototype.Wood != null)
			{
				character.Inventory.FindItemOfType(EquipmentPrototype.Wood)?.SetGathered();
				break;
			}
		}
		character.SetRoleInProgress(new RoleInfo(Role.Lumberjack), inProgress: true);
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

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (SubGoal is MoveToAndDeposit { SuccessfullyReachedDestination: false })
		{
			OnFailed(character);
			return null;
		}
		if (SubGoal is MoveToAndChop { Success: false } moveToAndChop && !moveToAndChop.IsTargetDeleted())
		{
			character.AddFailedFindAttempt(moveToAndChop.GetTargetObject(), 0, 0, FindType.Prototype, EquipmentPrototype.Wood, null, null);
			OnFailed(character);
			return null;
		}
		if (SubGoal is FindGoal { Success: false })
		{
			if (!HasSaidNeedAxe)
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.NeedAxe);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation);
				}
				character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				HasSaidNeedAxe = true;
			}
			OnFailed(character);
			return null;
		}
		bool controllableByPlayer = character.IsControllableByPlayer();
		int num = Math.Max(0, character.Inventory.CountItemsOfType(EquipmentPrototype.Wood) - (int)character.GetTargetAmountToCarryIncludingAmmo(EquipmentPrototype.Wood, null, InfectionType.None));
		bool flag = character.Community == null || character.Community.CountInventoryItemsOfType(EquipmentPrototype.Wood) < character.Community.GetCraftingLimit(EquipmentPrototype.Wood);
		bool flag2 = num > 0;
		bool flag3 = character.HasInventorySpaceFor(EquipmentPrototype.Wood.Weight) || (!controllableByPlayer && !flag2);
		TileObject bestTree = null;
		if (flag3 && flag)
		{
			if (character.Inventory.GetAxe() == null)
			{
				return new FindGoal(FindType.Axe, MovementType, critical: false);
			}
			FindGoal.BuildListOfEscapedFromTargets(character);
			TerrainCoord roleTargetLocation = character.GetRoleTargetLocation(Role.Lumberjack);
			TerrainCoord startTile = ((roleTargetLocation != TerrainCoord.Invalid) ? roleTargetLocation : character.Tile);
			TileObject fixedObjectOnTile = instance.GetFixedObjectOnTile(startTile.x, startTile.y);
			if (IsChoppableTree(fixedObjectOnTile) && CheckTree(character, fixedObjectOnTile, controllableByPlayer) && !character.HasFailedFindAttempt(fixedObjectOnTile, 0, 0, FindType.Prototype, EquipmentPrototype.Wood, null, null, RememberFailedAttemptTime, out var timeSinceFailedAttempt, out var failCount) && !FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, fixedObjectOnTile.Pos))
			{
				bestTree = fixedObjectOnTile;
			}
			else
			{
				float bestDist = GameTerrain.Instance.Size * 2;
				foreach (Prop allProp in Session.Instance.PropManager.AllProps)
				{
					if (allProp.GetBaseObjectType() == BaseObjectType.FallenTreeProp && IsChoppableTree(allProp) && !FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, allProp.Pos))
					{
						float num2 = allProp.GetCentreTile().GetDist(startTile);
						if (character.HasFailedFindAttempt(allProp, 0, 0, FindType.Prototype, EquipmentPrototype.Wood, null, null, RememberFailedAttemptTime, out timeSinceFailedAttempt, out failCount))
						{
							num2 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceFailedAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
						}
						if (num2 < bestDist && CheckTree(character, allProp, controllableByPlayer))
						{
							bestTree = allProp;
							bestDist = num2;
						}
					}
				}
				instance.TreeMapWho.GetNearestObject(startTile, Mathf.CeilToInt(bestDist), delegate(TreeProp tree)
				{
					if (GameTerrain.Instance.IsSlopeOrImpassableRaw(tree.GetTileX(), tree.GetTileY()))
					{
						return false;
					}
					if (FindGoal.IsNearSomeoneWeJustRanAwayFrom(character, tree.Pos))
					{
						return false;
					}
					float num4 = tree.GetCentreTile().GetDist(startTile);
					if (character.HasFailedFindAttempt(tree, 0, 0, FindType.Prototype, EquipmentPrototype.Wood, null, null, RememberFailedAttemptTime, out timeSinceFailedAttempt, out failCount))
					{
						num4 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceFailedAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
					}
					if (num4 < bestDist && CheckTree(character, tree, controllableByPlayer))
					{
						bestTree = tree;
						bestDist = num4;
						return true;
					}
					return false;
				});
			}
			FindGoal.ClearListOfEscapedFromTargets();
		}
		float maxInventoryWeight = character.GetMaxInventoryWeight();
		int num3 = Math.Max(1, Mathf.CeilToInt(maxInventoryWeight * 0.5f / EquipmentPrototype.Wood.Weight));
		float idealWeightToFree = (flag2 ? float.MaxValue : (EquipmentPrototype.Wood.Weight * (float)num3));
		Equipment bestItem;
		int amountToDeposit;
		bool failed;
		Prop moveToAndDepositProp = GatherGoal.GetMoveToAndDepositProp(character, EquipmentPrototype.Wood.Weight, idealWeightToFree, out bestItem, out amountToDeposit, out failed);
		if (bestTree != null)
		{
			if (moveToAndDepositProp != null)
			{
				TerrainCoord tile = character.Tile;
				if (moveToAndDepositProp != null && moveToAndDepositProp.Tile.GetDistSquared(tile) < bestTree.GetCentreTile().GetDistSquared(tile))
				{
					return new MoveToAndDeposit(character, moveToAndDepositProp, bestItem, amountToDeposit, MovementType)
					{
						WasGathered = (bestItem.GetGatheredAmount() > 0)
					};
				}
			}
			HasSaidNeedAxe = false;
			return new MoveToAndChop(character, bestTree, MovementType, FindGoal.CalcDontOpenOurGates(character))
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
			OnFailed(character);
			return null;
		}
		if (!flag2 && !flag3)
		{
			MemoryParam param = new MemoryParam(EquipmentPrototype.Wood);
			Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param);
			character.Speak(speechForSituation2, null, null, param);
			character.PauseRole(new RoleInfo(Role.Lumberjack));
			return null;
		}
		if (flag2)
		{
			MemoryParam param2 = new MemoryParam(EquipmentPrototype.Wood);
			Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.NoGatherDepotFound, param2);
			character.Speak(speechForSituation3, null, null, param2);
			character.PauseRole(new RoleInfo(Role.Lumberjack));
			return null;
		}
		if (!flag)
		{
			OnFailed(character);
			return null;
		}
		Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.LumberjackComplete);
		character.Speak(speechForSituation4);
		character.PauseRole(new RoleInfo(Role.Lumberjack));
		return null;
	}

	public static bool CheckTree(Character character, TileObject tree, bool controllableByPlayer)
	{
		TerrainCoord centreTile = tree.GetCentreTile();
		if (controllableByPlayer && !GameTerrain.Instance.FogOfWar.IsTileExplored(centreTile.x, centreTile.y))
		{
			return false;
		}
		if (character.HasMovementZone() && !character.MovementZone.Overlaps(tree.GetTileRect()))
		{
			return false;
		}
		if (GameTerrain.Instance.IsSlopeOrImpassableRaw(centreTile.x, centreTile.y))
		{
			return false;
		}
		Community communityThatOwnsThisArea = tree.GetCommunityThatOwnsThisArea();
		if (communityThatOwnsThisArea != null && communityThatOwnsThisArea != character.Community && communityThatOwnsThisArea.HasAnyLivingNonZombieMembers() && !character.Community.CachedAllies.Contains(communityThatOwnsThisArea) && centreTile != character.GetRoleTargetLocation(Role.Lumberjack))
		{
			return false;
		}
		if (tree.GetBaseObjectType() == BaseObjectType.TreeProp && ((TreeProp)tree).Growth < 1f)
		{
			return false;
		}
		if (!GameTerrain.Instance.IsAnySurroundingTileSpawnable(tree.GetTileRect()))
		{
			return false;
		}
		return true;
	}

	public static bool IsChoppableTree(TileObject obj)
	{
		if (obj is FallenTreeProp fallenTreeProp)
		{
			return fallenTreeProp.WoodRemaining > 0;
		}
		return obj is TreeProp;
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
		HasSaidNeedAxe = false;
	}

	public void SetLastAttemptedTimeToNow()
	{
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.Lumberjack));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Lumberjack);
	}
}
