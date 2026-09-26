using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildGoal : RoleGoal
{
	public TileObject CurrentBuilding;

	private TimeSpan _lastAttemptedTime = TimeSpan.FromDays(-365.0);

	private TerrainCoord _entryPoint = TerrainCoord.Invalid;

	private bool _entering;

	private bool _exiting;

	private bool _tooHeavy;

	private bool HasSaidTooHeavy;

	private bool HasSaidInsufficientResources;

	private bool HasSaidNeedAxe;

	private bool HasSaidNeedPickaxe;

	private bool HasSaidNoGatherDepot;

	private bool HasSaidNeedToolbox;

	public bool WasTriggeredFromDirectControl;

	private static float[] ResourceCount;

	private static TimeSpan RememberFailedAttemptTime = TimeSpan.FromSeconds(600.0);

	private static float CostPerFailure = 100f;

	private static float CostOfRecentFailure = 100f;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.BuildGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is BuildAnim)
		{
			return null;
		}
		return GameCursor.CursorBuild;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		if (reflector.Version < 382)
		{
			List<TileObject> list = new List<TileObject>();
			reflector.AddGameObjectRefList(ref list);
			CurrentBuilding = ((list.Count > 0) ? list[0] : null);
		}
		else
		{
			reflector.Add(ref CurrentBuilding);
		}
		reflector.Add(ref MovementType);
		reflector.Add(ref _entryPoint);
		reflector.Add(ref _entering);
		reflector.Add(ref _exiting);
		reflector.Add(ref _tooHeavy);
		reflector.Add(ref HasSaidInsufficientResources);
		reflector.AddAfter(ref HasSaidTooHeavy, 390);
		reflector.AddAfter(ref HasSaidNeedAxe, 390);
		reflector.AddAfter(ref HasSaidNeedPickaxe, 390);
		reflector.AddAfter(ref HasSaidNoGatherDepot, 390);
		reflector.AddAfter(ref HasSaidNeedToolbox, 390);
		reflector.AddAfter(ref WasTriggeredFromDirectControl, 44);
		reflector.Add(ref _lastAttemptedTime);
		if (reflector.Version < 321)
		{
			float value = 0f;
			reflector.Add(ref value);
		}
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.Builder), inProgress: true);
		Goal startBuildingGoal = GetStartBuildingGoal(character, parent);
		if (startBuildingGoal == null)
		{
			OnFailed(character, null);
			Finished = true;
		}
		else
		{
			SetSubGoal(character, parent, startBuildingGoal);
		}
	}

	public bool SetCurrentBuilding(Character character, Goal parent, TileObject building)
	{
		if (CurrentBuilding != building)
		{
			if (building != null && !DoesBuildingNeedMoreWorkers(building, character))
			{
				float num = float.MinValue;
				Character character2 = null;
				foreach (Character member in character.Community.Members)
				{
					if (member != character)
					{
						float sqrMagnitude = MathUtil.ToXZ(member.Pos - building.Pos).sqrMagnitude;
						if (!(sqrMagnitude <= num) && IsMemberWorkingOnBuilding(building, member))
						{
							num = sqrMagnitude;
							character2 = member;
						}
					}
				}
				character2?.GetBuildGoal().SetCurrentBuilding(character2, character2.GetGoal(), null);
			}
			if (Active)
			{
				OnDeactivate(character, parent);
				SetCurrentBuildingInternal(character, building);
				if (building == null || building.GetUnderConstructionInfo() == null)
				{
					return false;
				}
				OnActivate(character, parent);
			}
			else
			{
				SetCurrentBuildingInternal(character, building);
			}
		}
		WasTriggeredFromDirectControl = true;
		ClearHasSaidFlags();
		return true;
	}

	private void SetCurrentBuildingInternal(Character character, TileObject building)
	{
		if (CurrentBuilding == building)
		{
			return;
		}
		if (CurrentBuilding != null)
		{
			Recipe recipe = GameImpl.Instance.FindRecipeByProduct(CurrentBuilding.GetPropPrototype(), null);
			if (recipe != null)
			{
				foreach (Ingredient ingredient in recipe.Ingredients)
				{
					character.Inventory.FindIngredient(character, character, ingredient, recipe, null, character, character)?.SetGathered();
				}
			}
		}
		CurrentBuilding = building;
		if (CurrentBuilding == null)
		{
			return;
		}
		UnderConstructionInfo underConstructionInfo = CurrentBuilding.GetUnderConstructionInfo();
		if (underConstructionInfo == null)
		{
			return;
		}
		foreach (Ingredient ingredient2 in underConstructionInfo.Recipe.Ingredients)
		{
			if (!underConstructionInfo.HasUsedEnoughOfIngredient(ingredient2))
			{
				character.Inventory.FindIngredient(character, character, ingredient2, underConstructionInfo.Recipe, null, character, character)?.ClearGathered();
			}
		}
	}

	public Recipe GetFollowingRecipe()
	{
		if (CurrentBuilding == null || CurrentBuilding.GetUnderConstructionInfo() == null)
		{
			return null;
		}
		return CurrentBuilding.GetUnderConstructionInfo().Recipe;
	}

	public static int GetMaxWorkersOnBuilding(TileObject obj)
	{
		TerrainRect terrainRect = new TerrainRect(obj.GetMinTile(), obj.GetMaxTile());
		int num = 1 + Math.Max(0, terrainRect.TilesArea - 1) / 4;
		if (terrainRect.LongestEdge >= 3)
		{
			num = Math.Max(num, 2);
		}
		return num;
	}

	public bool IsMemberWorkingOnBuilding(TileObject building, Character member)
	{
		BuildGoal buildGoal = member.GetBuildGoal();
		if (buildGoal != null && buildGoal.CurrentBuilding == building && member.IsAwake)
		{
			if (member.GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: false).Role != Role.Builder)
			{
				return false;
			}
			if (member.IsTooDepressedToFollowOrders())
			{
				return false;
			}
			if (member.GetGoal() is SurvivorGoal { SubGoal: not null } survivorGoal)
			{
				GoalType goalType = survivorGoal.SubGoal.GetGoalType();
				if (goalType == GoalType.SatisfyNeedForSleep || goalType == GoalType.WarmGoal)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool DoesBuildingNeedMoreWorkers(TileObject building, Character character)
	{
		UnderConstructionInfo underConstructionInfo = building.GetUnderConstructionInfo();
		Recipe recipe = underConstructionInfo.Recipe;
		if (ResourceCount == null || ResourceCount.Length < recipe.Ingredients.Count)
		{
			ResourceCount = new float[recipe.Ingredients.Count];
		}
		for (int i = 0; i < recipe.Ingredients.Count; i++)
		{
			ResourceCount[i] = underConstructionInfo.GetAmountUsedOfIngredient(recipe.Ingredients[i]);
		}
		int maxWorkersOnBuilding = GetMaxWorkersOnBuilding(building);
		int num = 0;
		foreach (Character member in character.Community.Members)
		{
			if (member != character && IsMemberWorkingOnBuilding(building, member))
			{
				num++;
				if (num >= maxWorkersOnBuilding)
				{
					return false;
				}
				bool flag = true;
				for (int j = 0; j < recipe.Ingredients.Count; j++)
				{
					float num2 = (float)recipe.Ingredients[j].Amount + recipe.Ingredients[j].LiquidAmount;
					recipe.Ingredients[j].HasEnoughOfIngredient(member, member, recipe, null, member, out var amountNeeded);
					float num3 = num2 - amountNeeded;
					ResourceCount[j] += num3;
					flag &= ResourceCount[j] >= num2;
				}
				if (flag)
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool HasToolRequiredForBuilding(Character character, TileObject obj)
	{
		return GameImpl.Instance.FindRecipeByProduct(obj.GetPropPrototype(), null)?.HasToolTypeForRecipe(character) ?? false;
	}

	public bool PickBuildingToStartOn(Character character)
	{
		if (CurrentBuilding != null && CurrentBuilding.GetUnderConstructionInfo() != null && !CurrentBuilding.IsDestroyed() && !CurrentBuilding.Deleted && HasToolRequiredForBuilding(character, CurrentBuilding) && (!character.HasMovementZone() || character.MovementZone.Overlaps(CurrentBuilding.GetTileRect())) && DoesBuildingNeedMoreWorkers(CurrentBuilding, character))
		{
			return true;
		}
		float num = float.MaxValue;
		TileObject building = null;
		foreach (TileObject underConstructionBuilding in character.Community.UnderConstructionBuildings)
		{
			UnderConstructionInfo underConstructionInfo = underConstructionBuilding.GetUnderConstructionInfo();
			if (underConstructionInfo.Recipe.SkillLevel <= character.GetSkillLevelWithEffects(underConstructionInfo.Recipe.SkillType) && (!character.HasMovementZone() || character.MovementZone.Overlaps(underConstructionBuilding.GetTileRect())) && DoesBuildingNeedMoreWorkers(underConstructionBuilding, character))
			{
				float num2 = underConstructionBuilding.GetNearestTileTo(character.Tile).GetDist(character.Tile);
				if (!HasToolRequiredForBuilding(character, underConstructionBuilding))
				{
					num2 += 128f;
				}
				if (character.HasFailedFindAttempt(underConstructionBuilding, 0, 0, FindType.Build, null, null, null, RememberFailedAttemptTime, out var timeSinceAttempt, out var failCount))
				{
					num2 += (float)failCount * CostPerFailure + Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFailedAttemptTime.TotalSeconds) * CostOfRecentFailure;
				}
				if (num2 < num)
				{
					building = underConstructionBuilding;
					num = num2;
				}
			}
		}
		SetCurrentBuildingInternal(character, building);
		return CurrentBuilding != null;
	}

	public Goal GetStartBuildingGoal(Character character, Goal parent)
	{
		if (!PickBuildingToStartOn(character))
		{
			return null;
		}
		character.DirectControlledCrouching = false;
		Recipe followingRecipe = GetFollowingRecipe();
		if (followingRecipe != null && !followingRecipe.HasToolTypeForRecipe(character))
		{
			followingRecipe.GetRecipeToolWeight(out var minWeight, out var maxWeight);
			bool failed;
			Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, minWeight, maxWeight, MovementType, out failed);
			if (moveToAndDepositGoal != null)
			{
				return moveToAndDepositGoal;
			}
			FindGoal findGoalForRecipeTool = followingRecipe.GetFindGoalForRecipeTool(MovementType);
			if (findGoalForRecipeTool != null)
			{
				return findGoalForRecipeTool;
			}
		}
		if (DoesBuilderHaveNeededResources(character))
		{
			ClearHasSaidFlags();
			return new MoveWithinBounds(MovementType, CurrentBuilding.GetMinTile() - new TerrainCoord(1, 1), CurrentBuilding.GetMaxTile() + new TerrainCoord(1, 1))
			{
				_dontOpenOurGates = CalcDontOpenOurGates(character, parent)
			};
		}
		return GetFindGoal(character);
	}

	private bool CalcDontOpenOurGates(Character character, Goal parent)
	{
		if (parent is BuryGoal)
		{
			return false;
		}
		return FindGoal.CalcDontOpenOurGates(character);
	}

	public void ResetLastAttemptedTime()
	{
		_lastAttemptedTime = TimeSpan.FromDays(-365.0);
		ClearHasSaidFlags();
	}

	public void SetLastAttemptedTimeToNow()
	{
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool WasSuccessful()
	{
		if (_lastAttemptedTime <= TimeSpan.Zero)
		{
			return CurrentBuilding == null;
		}
		return false;
	}

	private void OnFailed(Character character, TileObject building)
	{
		if (building != null)
		{
			character.AddFailedFindAttempt(building, 0, 0, FindType.Build, null, null, null);
			CurrentBuilding = null;
		}
		character.SetRoleFailedRecently(new RoleInfo(Role.Builder));
		_lastAttemptedTime = Session.Instance.PlayTime;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (parent is PrioritiserGoal && !character.HasRunningRole(Role.Builder))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.Builder, WasTriggeredFromDirectControl, isSpeechGoal: false))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - _lastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active)
		{
			if (SubGoal is BuildAnim)
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (character.IsChoppingWood())
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (character.IsMining())
			{
				return GoalPriority.Survivor_Role_Animation;
			}
			if (SubGoal is FindGoal { HasStartedChoppingOrMining: not false })
			{
				return GoalPriority.Survivor_Role_Animation;
			}
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.Builder));
	}

	public void CompleteBuildingIfNotYetCompleted(Character character, TileObject building)
	{
		if (building.GetUnderConstructionInfo() == null)
		{
			SetCurrentBuildingInternal(character, null);
		}
		else
		{
			CompleteBuilding(character, building);
		}
	}

	public void CompleteBuilding(Character character, TileObject building)
	{
		if (building.GetUnderConstructionInfo() == null)
		{
			SetCurrentBuildingInternal(character, null);
			return;
		}
		building.SetUnderConstructionInfo(null);
		Session.Instance.ClearTrashForCompletedBuilding(character, building);
		if (building is Gate gate)
		{
			gate.RecalcFenceNeighbours();
		}
		if (building is BaseFence baseFence)
		{
			baseFence.UpdateModelAndAngle(inited: true);
			baseFence.RecalcNeighbours();
		}
		building.GetCommunity()?.OnCompletedBuildingAddedToCommunity(character, building, null);
		if (character.Tile.IsWithinBounds(building.GetMinTile(), building.GetMaxTile()))
		{
			character.SetPosition(GameTerrain.Instance.GetTileCentrePos(FindSuitableExitTile(character, building)));
			character.SetFade(1f);
			character.StartFade(0f, 1f / 60f);
		}
		StoryManager.Instance.TriggerEnabledTriggersOfType(TriggerType.BuiltSomething, character, building);
		SetCurrentBuildingInternal(character, null);
		foreach (Character member in character.Community.Members)
		{
			if (member != character)
			{
				BuildGoal buildGoal = member.GetBuildGoal();
				if (buildGoal != null && buildGoal.CurrentBuilding == building)
				{
					buildGoal.SetCurrentBuilding(member, member.GetGoal(), null);
				}
			}
		}
	}

	public void OnBuildingGotDeleted(Character character, Goal parent, TileObject obj)
	{
		bool num = obj == CurrentBuilding;
		if (num)
		{
			SetCurrentBuildingInternal(character, null);
		}
		if (!num || !Active)
		{
			return;
		}
		if (SubGoal is BuildAnim && character.Tile.IsWithinBounds(obj.GetMinTile(), obj.GetMaxTile()))
		{
			character.SetPosition(GameTerrain.Instance.GetTileCentrePos(FindSuitableExitTile(character, obj)));
			character.SetFade(1f);
			character.StartFade(0f, 1f / 60f);
		}
		if (character.Community.UnderConstructionBuildings.Count == 0)
		{
			if (parent is SurvivorGoal)
			{
				character.SetRoleInProgress(new RoleInfo(Role.Builder), inProgress: false);
				character.OnRoleSucceeded();
			}
			Finished = true;
		}
		else
		{
			SetSubGoal(character, parent, GetStartBuildingGoal(character, parent));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (CurrentBuilding != null && IsBuildingCompleted(CurrentBuilding))
		{
			CompleteBuildingIfNotYetCompleted(character, CurrentBuilding);
			PickBuildingToStartOn(character);
			character.SetRoleInProgress(new RoleInfo(Role.Builder), inProgress: false);
			character.OnRoleSucceeded();
		}
		if (character.Community.UnderConstructionBuildings.Count == 0 && parent is SurvivorGoal && !character.IsControllableByPlayer())
		{
			character.CancelRole(new RoleInfo(Role.Builder), fromBuildGoal: true);
			character.OnRoleSucceeded();
		}
		if (CurrentBuilding != null && character.Tile.IsWithinBounds(CurrentBuilding.GetMinTile(), CurrentBuilding.GetMaxTile()) && (CurrentBuilding.GetUnderConstructionInfo() == null || CurrentBuilding.GetUnderConstructionInfo().IsBuiltEnoughToBeAnObstacle()))
		{
			character.SetPosition(GameTerrain.Instance.GetTileCentrePos(FindSuitableExitTile(character, CurrentBuilding)));
			character.SetFade(1f);
			character.StartFade(0f, 1f / 60f);
		}
		else
		{
			character.SetFade(0f);
		}
		WasTriggeredFromDirectControl = false;
		base.OnDeactivate(character, parent);
	}

	private TerrainCoord FindSuitableExitTile(Character character, TileObject building)
	{
		GameTerrain instance = GameTerrain.Instance;
		if (_entryPoint != TerrainCoord.Invalid && _entryPoint != TerrainCoord.Zero && !instance.IsImpassable(_entryPoint.x, _entryPoint.y, 2051, character, null))
		{
			return _entryPoint;
		}
		return FindSuitableExitTileStatic(character, building);
	}

	public static TerrainCoord FindSuitableExitTileStatic(Character character, TileObject building)
	{
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord minTile = building.GetMinTile();
		TerrainCoord maxTile = building.GetMaxTile();
		int num = 1;
		do
		{
			for (int i = minTile.x - num; i <= maxTile.x + num; i++)
			{
				if (!instance.IsImpassable(i, minTile.y - num, 2051, character, null))
				{
					return new TerrainCoord(i, minTile.y - num);
				}
				if (!instance.IsImpassable(i, maxTile.y + num, 2051, character, null))
				{
					return new TerrainCoord(i, maxTile.y + num);
				}
			}
			for (int j = minTile.y - num + 1; j <= maxTile.y + num - 1; j++)
			{
				if (!instance.IsImpassable(minTile.x - num, j, 2051, character, null))
				{
					return new TerrainCoord(minTile.x - num, j);
				}
				if (!instance.IsImpassable(maxTile.x + num, j, 2051, character, null))
				{
					return new TerrainCoord(maxTile.x + num, j);
				}
			}
			num++;
		}
		while (num < instance.Size);
		return character.Tile;
	}

	public override void Update(Character character, Goal parent)
	{
		if (CurrentBuilding == null)
		{
			Finished = true;
			return;
		}
		if (_entering && character.IsFadeComplete())
		{
			_entering = false;
			character.StartFade(0f, 1f / 60f);
			if (CurrentBuilding.GetUnderConstructionInfo() == null)
			{
				Finished = true;
				return;
			}
			bool flag = CurrentBuilding.GetUnderConstructionInfo().Recipe.RecipeType == RecipeType.Shovel;
			Equipment equipment = (flag ? ((MeleeWeapon)character.Inventory.GetShovel()) : ((MeleeWeapon)character.Inventory.GetToolbox()));
			if (equipment == null)
			{
				Finished = true;
				return;
			}
			character.SetPosition(BuildAnim.CalcPositionForBuilder(character, CurrentBuilding));
			character.SetFacingAngle((float)Session.Instance.DeterministicRand.Next(4) * (MathF.PI / 2f));
			character.EquippedItem = equipment;
			character.DesiredEquippedItem = equipment;
			ActionAnim anim = (flag ? ActionAnim.DigLoop : ActionAnim.Build);
			SetSubGoal(character, parent, new BuildAnim(CurrentBuilding, anim, MovementType == MovementType.Run));
		}
		if (_exiting && character.IsFadeComplete() && character.GetFadeTarget() == 1f)
		{
			character.SetPosition(GameTerrain.Instance.GetTileCentrePos(FindSuitableExitTile(character, CurrentBuilding)));
			character.StartFade(0f, 1f / 60f);
			SetSubGoal(character, parent, new Idle());
		}
		if (_exiting && character.IsFadeComplete() && character.GetFadeTarget() == 0f)
		{
			_exiting = false;
			Goal goal = CheckBuildingCompleted(character, parent);
			if (goal != null)
			{
				SetSubGoal(character, parent, goal);
			}
			else
			{
				Finished = true;
			}
		}
		base.Update(character, parent);
	}

	private Goal CheckBuildingCompleted(Character character, Goal parent)
	{
		character.OnRoleSucceeded();
		if (IsBuildingCompleted(CurrentBuilding))
		{
			CompleteBuildingIfNotYetCompleted(character, CurrentBuilding);
			Goal startBuildingGoal = GetStartBuildingGoal(character, parent);
			if (startBuildingGoal != null)
			{
				return startBuildingGoal;
			}
			character.SetRoleInProgress(new RoleInfo(Role.Builder), inProgress: false);
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.ConstructionComplete);
			if (speechForSituation != null)
			{
				character.Speak(speechForSituation);
			}
			character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
			if (parent is SurvivorGoal && !character.IsControllableByPlayer())
			{
				character.CancelRole(new RoleInfo(Role.Builder), fromBuildGoal: true);
			}
			return null;
		}
		return GetFindGoal(character);
	}

	private Goal GetFindGoal(Character character)
	{
		Recipe followingRecipe = GetFollowingRecipe();
		if (followingRecipe == null)
		{
			return null;
		}
		CurrentBuilding.GetUnderConstructionInfo().GetNeededIngredientsWeight(out var minRequiredWeight, out var desiredWeight, character.GetMaxInventoryWeight());
		bool failed;
		Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, minRequiredWeight, desiredWeight, MovementType, out failed);
		if (moveToAndDepositGoal != null)
		{
			return moveToAndDepositGoal;
		}
		int desiredAmount = Math.Max(1, character.Community.GetNumUnderConstructionBuildingsOfType(CurrentBuilding.GetPropPrototype()));
		return new FindGoal(FindType.BuildingResources, followingRecipe, CurrentBuilding, desiredAmount, MovementType, critical: false)
		{
			CanTravelFar = character.IsControllableByPlayer()
		};
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveWithinBounds moveWithinBounds)
		{
			if (moveWithinBounds.Success)
			{
				TerrainCoord nearestTileTo = CurrentBuilding.GetNearestTileTo(character.Tile);
				if (GameTerrain.Instance.IsUpgradingFence(CurrentBuilding) != null && character.Tile.IsWithinBounds(nearestTileTo - new TerrainCoord(1, 1), nearestTileTo + new TerrainCoord(1, 1)))
				{
					return new TurnTo(nearestTileTo);
				}
				character.StartFade(1f, 1f / 60f);
				_entering = true;
				_entryPoint = character.Tile;
				return new Idle();
			}
			if (CurrentBuilding != null && !CurrentBuilding.Deleted && !moveWithinBounds._dontOpenOurGates)
			{
				return new MoveAsCloseAsPossibleToTarget(character, CurrentBuilding, MovementType, 4f)
				{
					_dontOpenOurGates = CalcDontOpenOurGates(character, parent)
				};
			}
		}
		if (SubGoal is MoveAsCloseAsPossibleToTarget { Success: not false })
		{
			TerrainCoord nearestTileTo2 = CurrentBuilding.GetNearestTileTo(character.Tile);
			if (GameTerrain.Instance.IsUpgradingFence(CurrentBuilding) != null && character.Tile.IsWithinBounds(nearestTileTo2 - new TerrainCoord(1, 1), nearestTileTo2 + new TerrainCoord(1, 1)))
			{
				return new TurnTo(nearestTileTo2);
			}
			character.StartFade(1f, 1f / 60f);
			_entering = true;
			_entryPoint = character.Tile;
			return new Idle();
		}
		if (SubGoal is MoveToAndDeposit moveToAndDeposit)
		{
			if (moveToAndDeposit.SuccessfullyReachedDestination)
			{
				return GetFindGoal(character);
			}
			if (!HasSaidNoGatherDepot)
			{
				MemoryParam param = new MemoryParam(moveToAndDeposit.StuffToDeposit);
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.NoGatherDepotFound, param);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation, null, null, param);
				}
				character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
				HasSaidNoGatherDepot = true;
			}
			OnFailed(character, null);
			return null;
		}
		if (SubGoal is TurnTo)
		{
			TileObject tileObject = GameTerrain.Instance.IsUpgradingFence(CurrentBuilding);
			if (tileObject != null)
			{
				return new AnimationGoal((tileObject.GetPropPrototype().RepairAnim == RepairAnimType.Hammering) ? ActionAnim.RepairStart : ActionAnim.CraftStart, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			}
		}
		if (SubGoal is AnimationGoal animationGoal)
		{
			switch (animationGoal.GetAnim())
			{
			case ActionAnim.RepairStart:
				return new BuildAnim(CurrentBuilding, ActionAnim.Repair, MovementType == MovementType.Run);
			case ActionAnim.CraftStart:
				return new BuildAnim(CurrentBuilding, ActionAnim.CraftLoop, MovementType == MovementType.Run);
			case ActionAnim.Repair:
				return new AnimationGoal(ActionAnim.RepairFinish, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			case ActionAnim.CraftLoop:
				return new AnimationGoal(ActionAnim.CraftEnd, (MovementType == MovementType.Run) ? RoleGoal.UrgentWorkSpeed : 1f);
			case ActionAnim.RepairFinish:
			case ActionAnim.CraftEnd:
				return CheckBuildingCompleted(character, parent);
			}
		}
		if (SubGoal is BuildAnim)
		{
			character.StartFade(1f, 1f / 60f);
			_exiting = true;
			return new Idle();
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.FindType == FindType.BuildingResources)
			{
				if (DoesBuilderHaveNeededResources(character))
				{
					ClearHasSaidFlags();
					return new MoveWithinBounds(MovementType, CurrentBuilding.GetMinTile() - new TerrainCoord(1, 1), CurrentBuilding.GetMaxTile() + new TerrainCoord(1, 1))
					{
						_dontOpenOurGates = CalcDontOpenOurGates(character, parent)
					};
				}
				if (findGoal.Result == FindResult.TooHeavy)
				{
					bool failed;
					Goal moveToAndDepositGoal = GatherGoal.GetMoveToAndDepositGoal(character, float.MaxValue, float.MaxValue, MovementType, out failed);
					if (moveToAndDepositGoal != null)
					{
						return moveToAndDepositGoal;
					}
					if (!HasSaidTooHeavy)
					{
						MemoryParam param2 = ((findGoal.FoundItem != null) ? new MemoryParam(findGoal.FoundItem) : new MemoryParam(findGoal.FoundResource));
						Speech speechForSituation2 = StoryManager.Instance.GetSpeechForSituation(character, null, null, SpeechSituation.GatherTooHeavy, param2);
						if (speechForSituation2 != null)
						{
							character.Speak(speechForSituation2, null, null, param2);
						}
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
						HasSaidTooHeavy = true;
					}
				}
				else if (findGoal.Result == FindResult.NeedAxe)
				{
					if (!HasSaidNeedAxe)
					{
						Speech speechForSituation3 = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.NeedAxe);
						if (speechForSituation3 != null)
						{
							character.Speak(speechForSituation3);
						}
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
						HasSaidNeedAxe = true;
					}
				}
				else if (findGoal.Result == FindResult.NeedPickaxe)
				{
					if (!HasSaidNeedPickaxe)
					{
						Speech speechForSituation4 = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.NeedPickaxe);
						if (speechForSituation4 != null)
						{
							character.Speak(speechForSituation4);
						}
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
						HasSaidNeedPickaxe = true;
					}
				}
				else if (findGoal.Result != FindResult.Success && findGoal.Result != FindResult.NotAccessible && !HasSaidInsufficientResources)
				{
					MemoryParam param3 = default(MemoryParam);
					if (CurrentBuilding != null && (WasTriggeredFromDirectControl || !CalcDontOpenOurGates(character, parent)))
					{
						UnderConstructionInfo underConstructionInfo = CurrentBuilding.GetUnderConstructionInfo();
						if (underConstructionInfo != null)
						{
							foreach (Ingredient ingredient in underConstructionInfo.Recipe.Ingredients)
							{
								if (!underConstructionInfo.HasUsedEnoughOfIngredient(ingredient))
								{
									if (ingredient.Prototypes != null && ingredient.Prototypes.Count > 0)
									{
										param3 = new MemoryParam(ingredient.Prototypes[0]);
									}
									else if (ingredient.LiquidTypes != null && ingredient.LiquidTypes.Count > 0)
									{
										param3 = new MemoryParam(ingredient.LiquidTypes[0]);
									}
								}
							}
						}
						Speech speechForSituation5 = StoryManager.Instance.GetSpeechForSituation(character, null, CurrentBuilding, SpeechSituation.InsufficientResources, param3);
						if (speechForSituation5 != null)
						{
							character.Speak(speechForSituation5, null, CurrentBuilding, param3);
						}
						character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
						HasSaidInsufficientResources = true;
					}
				}
			}
			else
			{
				if (findGoal.Success)
				{
					return GetStartBuildingGoal(character, parent);
				}
				if (character.IsControllableByPlayer() && !HasSaidNeedToolbox)
				{
					Speech speechForSituation6 = StoryManager.Instance.GetSpeechForSituation(character, null, (findGoal.FindType == FindType.Toolbox) ? SpeechSituation.NeedToolbox : SpeechSituation.NeedShovel);
					character.Speak(speechForSituation6);
					character.SetRecentActivity(RecentActivityType.SpeakToSelf, null);
					HasSaidNeedToolbox = true;
				}
			}
		}
		OnFailed(character, CurrentBuilding);
		return null;
	}

	private void ClearHasSaidFlags()
	{
		HasSaidInsufficientResources = false;
		HasSaidTooHeavy = false;
		HasSaidNeedAxe = false;
		HasSaidNeedPickaxe = false;
		HasSaidNoGatherDepot = false;
		HasSaidNeedToolbox = false;
	}

	public bool IsInsideBuilding(Character character, out TileObject building)
	{
		building = CurrentBuilding;
		if (building == null)
		{
			return false;
		}
		if (!(SubGoal is BuildAnim))
		{
			if (_exiting)
			{
				return character.GetFadeTarget() == 1f;
			}
			return false;
		}
		return true;
	}

	public bool IsBuildingCompleted(TileObject building)
	{
		return building.GetUnderConstructionInfo()?.IsCompleted() ?? true;
	}

	public bool DoesBuilderHaveNeededResources(Character character)
	{
		UnderConstructionInfo underConstructionInfo = CurrentBuilding.GetUnderConstructionInfo();
		if (underConstructionInfo == null || underConstructionInfo.Recipe == null)
		{
			return false;
		}
		for (int i = 0; i < underConstructionInfo.Recipe.Ingredients.Count; i++)
		{
			Ingredient ingredient = underConstructionInfo.Recipe.Ingredients[i];
			if (!underConstructionInfo.HasUsedEnoughOfIngredient(ingredient) && character.Inventory.FindIngredient(character, character, ingredient, underConstructionInfo.Recipe, null, character, null) != null)
			{
				return true;
			}
		}
		return underConstructionInfo.Recipe.Ingredients.Count == 0;
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.Builder);
	}

	public override void SetMovementType(Character character, MovementType movementType)
	{
		base.SetMovementType(character, movementType);
		if (SubGoal is BuildAnim buildAnim)
		{
			buildAnim.Urgent = movementType == MovementType.Run;
		}
	}
}
