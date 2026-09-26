using System;
using UnityEngine;

public class SatisfyHunger : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static GameProfiler _Timer = new GameProfiler("Update.EatGoalSearch");

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is EatGoal)
		{
			return null;
		}
		return GameCursor.CursorEat;
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
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SatisfyHunger;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Active && !character.DirectControlled && SubGoal is EatGoal)
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
		if (!Active && character.CarryingObject != null && character.GetHunger() < Character.HungerExtraCriticalTime)
		{
			return false;
		}
		if (character.Inventory.GetFood(character, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null)
		{
			return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
		}
		if (character.InsideBuilding != null && character.InsideBuilding.Inventory.GetFood(character.InsideBuilding, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
		{
			return character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None);
		}
		if (character.HasBeenPlayerControlledRecently(character.GetHunger() >= Character.HungerCriticalTime, character.GetHunger() >= Character.HungerExtraCriticalTime))
		{
			return false;
		}
		if (!Active && character.DirectControlledCrouching && character.GetHunger() < Character.HungerExtraCriticalTime)
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
			if (SubGoal is EatGoal || SubGoal is MoveToAndInteractGoal)
			{
				return GoalPriority.Survivor_SatisfyNeeds_Animation;
			}
			if (character.GetHunger() >= Character.HungerCriticalTime)
			{
				if (character.Inventory.GetFood(character, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null)
				{
					return GoalPriority.Survivor_Hunger_Pocket;
				}
				if (character.InsideBuilding != null && character.InsideBuilding.Inventory.GetFood(character.InsideBuilding, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
				{
					return GoalPriority.Survivor_Hunger_Pocket;
				}
				return GoalPriority.Survivor_Hunger_Critical;
			}
			return GoalPriority.Survivor_SatisfyNeeds_Active;
		}
		if (character.GetHunger() < Character.HungryTime)
		{
			return GoalPriority.Impossible;
		}
		if (character.GetHunger() >= Character.HungerCriticalTime && character.Inventory.GetFood(character, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null)
		{
			return GoalPriority.Survivor_Hunger_Pocket;
		}
		if (character.GetHunger() >= Character.HungerCriticalTime && character.InsideBuilding != null && character.InsideBuilding.Inventory.GetFood(character.InsideBuilding, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
		{
			return GoalPriority.Survivor_Hunger_Pocket;
		}
		if (character.GetHunger() >= Character.HungerCriticalTime)
		{
			return GoalPriority.Survivor_Hunger_Critical;
		}
		if (character.ShouldRoleTakePriorityOverNeeds() && character.Inventory.GetFood(character, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false) == null)
		{
			return GoalPriority.Impossible;
		}
		return GoalPriority.Survivor_Hunger;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		if (character.GetHunger() >= Character.HungerCriticalTime || character.IsCrouching() || character.WasCrouching || character.IsAmbient() || character.HasBeenPlayerControlledRecently(character.GetHunger() >= Character.HungerCriticalTime, character.GetHunger() >= Character.HungerExtraCriticalTime) || character.ShouldRoleTakePriorityOverNeeds())
		{
			Equipment food = character.Inventory.GetFood(character, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false);
			if (food != null)
			{
				SetCrouching(character, parent, character.DirectControlledCrouching);
				SetSubGoal(character, parent, new EatGoal(food));
				return;
			}
			if (character.InsideBuilding != null && !GameCursor.IsStealingToTakeFrom(character, character.InsideBuilding))
			{
				Equipment food2 = character.InsideBuilding.Inventory.GetFood(character.InsideBuilding, character, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: false);
				if (food2 != null)
				{
					SetCrouching(character, parent, character.DirectControlledCrouching);
					SetSubGoal(character, parent, new MoveToAndInteractGoal(character, character.InsideBuilding, InteractionType.EatFromPot, food2, MovementType.Walk));
					return;
				}
			}
		}
		bool flag = character.GetHunger() >= Character.HungerCriticalTime;
		if (flag && character.Community != null && (character.Community.CommunityType == CommunityType.HunterLooter || character.Community.CommunityType == CommunityType.HunterMercenary))
		{
			Squad squad = character.GetSquad();
			if (squad != null && squad.EverybodyFleeing && squad.Behaviour == SquadBehaviour.Travel)
			{
				flag = false;
			}
		}
		SetSubGoal(character, parent, new FindGoal(FindType.Food, MovementType.Walk, flag));
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
		if (SubGoal is FindGoal { Success: not false } findGoal)
		{
			if (findGoal.FoundItem != null)
			{
				return new EatGoal(findGoal.FoundItem);
			}
			_lastSearchFailed = false;
			return null;
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is EatGoal)
		{
			_lastSearchFailed = false;
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}
}
