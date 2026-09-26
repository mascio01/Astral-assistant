using System;
using UnityEngine;

public class SatisfyThirst : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public TerrainCoord FilledFromTile = TerrainCoord.Invalid;

	public int FilledFromPathIndex = -1;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is DrinkGoal)
		{
			return null;
		}
		return GameCursor.CursorWaterBottle;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
		reflector.AddAfter(ref FilledFromTile, 386);
		reflector.AddAfter(ref FilledFromPathIndex, 386);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SatisfyThirst;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && !character.DirectControlled && (SubGoal is DrinkGoal || SubGoal is FillLiquidContainer))
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (!Active && character.IsCrouching() && character.DirectControlled)
		{
			return false;
		}
		if (!Active && character.CarryingObject != null && character.GetThirst() < Character.ThirstExtraCriticalTime)
		{
			return false;
		}
		if (character.Inventory.GetBestWaterBottleToDrink(character, character, forSharing: false) != null)
		{
			return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
		}
		if (character.InsideBuilding != null && character.InsideBuilding.Inventory.GetBestWaterBottleToDrink(character.InsideBuilding, character, forSharing: false) != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
		{
			return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
		}
		if (character.HasBeenPlayerControlledRecently(character.GetThirst() >= Character.ThirstCriticalTime, character.GetThirst() >= Character.ThirstExtraCriticalTime))
		{
			return false;
		}
		if (!Active && character.DirectControlledCrouching && character.GetThirst() < Character.ThirstExtraCriticalTime)
		{
			return false;
		}
		if (!Active)
		{
			if (Session.Instance.PlayTime - _lastSearchedTime < (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
			{
				return false;
			}
			if (_lastSearchFailed && character.IsSatisfyingNeeds())
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active && !Finished)
		{
			if (SubGoal is DrinkGoal || SubGoal is FillLiquidContainer || SubGoal is MoveToAndInteractGoal)
			{
				return GoalPriority.Survivor_SatisfyNeeds_Animation;
			}
			if (character.GetThirst() >= Character.ThirstCriticalTime)
			{
				return GoalPriority.Survivor_Thirst_Critical;
			}
			return GoalPriority.Survivor_SatisfyNeeds_Active;
		}
		CraftGoal craftGoal = character.GetCraftGoal();
		if (craftGoal != null && craftGoal.FollowingRecipe != null && craftGoal.FollowingRecipe.ProductLiquidPrototype == LiquidPrototype.Water && character.Inventory.GetBestWaterBottleToDrink(character, character, forSharing: false) == null)
		{
			return GoalPriority.Survivor_Thirst_Low;
		}
		if (character.GetThirst() < Character.ThirstyTime)
		{
			return GoalPriority.Impossible;
		}
		if (character.Inventory.GetBestWaterBottleToDrink(character, character, forSharing: false) != null)
		{
			return GoalPriority.Survivor_Thirst_Pocket;
		}
		if (character.InsideBuilding != null && character.InsideBuilding.Inventory.GetBestWaterBottleToDrink(character.InsideBuilding, character, forSharing: false) != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
		{
			return GoalPriority.Survivor_Thirst_Pocket;
		}
		if (character.GetThirst() >= Character.ThirstCriticalTime)
		{
			return GoalPriority.Survivor_Thirst_Critical;
		}
		if (character.ShouldRoleTakePriorityOverNeeds())
		{
			return GoalPriority.Impossible;
		}
		return GoalPriority.Survivor_Thirst;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		FilledFromTile = TerrainCoord.Invalid;
		FilledFromPathIndex = -1;
		Equipment bestWaterBottleToDrink = character.Inventory.GetBestWaterBottleToDrink(character, character, forSharing: false);
		if (bestWaterBottleToDrink != null)
		{
			SetCrouching(character, parent, character.DirectControlledCrouching);
			SetSubGoal(character, parent, new DrinkGoal(bestWaterBottleToDrink));
			return;
		}
		if (character.InsideBuilding != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
		{
			Equipment bestWaterBottleToDrink2 = character.InsideBuilding.Inventory.GetBestWaterBottleToDrink(character.InsideBuilding, character, forSharing: false);
			if (bestWaterBottleToDrink2 != null)
			{
				SetCrouching(character, parent, character.DirectControlledCrouching);
				SetSubGoal(character, parent, new MoveToAndInteractGoal(character, character.InsideBuilding, InteractionType.EatFromPot, bestWaterBottleToDrink2, MovementType.Walk));
				return;
			}
		}
		bool flag = character.GetThirst() >= Character.ThirstCriticalTime;
		if (flag && character.Community != null && (character.Community.CommunityType == CommunityType.HunterLooter || character.Community.CommunityType == CommunityType.HunterMercenary))
		{
			Squad squad = character.GetSquad();
			if (squad != null && squad.EverybodyFleeing && squad.Behaviour == SquadBehaviour.Travel)
			{
				flag = false;
			}
		}
		SetSubGoal(character, parent, new FindGoal(FindType.Drink, MovementType.Walk, flag));
		if (SubGoal.Finished && !((FindGoal)SubGoal).Success)
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		base.OnDeactivate(character, parent);
		SetCrouching(character, parent, crouching: false);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FindGoal { Success: not false, FoundItem: not null } findGoal)
		{
			FilledFromTile = findGoal.FilledFromTile;
			FilledFromPathIndex = findGoal.FilledFromPathIndex;
			return new DrinkGoal(findGoal.FoundItem);
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is DrinkGoal)
		{
			_lastSearchFailed = false;
			if (FilledFromTile != TerrainCoord.Invalid)
			{
				Goal fillExtraWaterContainerGoal = FindGoal.GetFillExtraWaterContainerGoal(character, FilledFromTile, FilledFromPathIndex, MovementType.Walk);
				if (fillExtraWaterContainerGoal != null)
				{
					return fillExtraWaterContainerGoal;
				}
			}
			return null;
		}
		if (SubGoal is FillLiquidContainer { Success: not false })
		{
			Goal fillExtraWaterContainerGoal2 = FindGoal.GetFillExtraWaterContainerGoal(character, FilledFromTile, FilledFromPathIndex, MovementType.Walk);
			if (fillExtraWaterContainerGoal2 != null)
			{
				return fillExtraWaterContainerGoal2;
			}
		}
		return base.GetNextSubGoal(character, parent);
	}
}
