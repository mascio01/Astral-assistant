using System;
using UnityEngine;

public class DepressedGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.DepressedGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.DepressedIcon;
	}

	public override bool IsBored(Character character)
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (Active)
		{
			if (character.DirectControlled)
			{
				if (!character.IsTooDepressedToFollowOrders())
				{
					return false;
				}
				if (!character.IsLeaderConsciousAndNotZombie() && !character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None))
				{
					return false;
				}
			}
			Squad squad = character.GetSquad();
			if (squad != null && squad.Action == SquadAction.GoTo)
			{
				return false;
			}
			return true;
		}
		if (character.IsTooDepressedToFollowOrders() && (character.IsLeaderConsciousAndNotZombie() || character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None)))
		{
			TerrainCoord tile = character.Tile;
			if (!GameTerrain.Instance.IsTileRiver(tile.x, tile.y))
			{
				Squad squad2 = character.GetSquad();
				if (squad2 != null && squad2.Action == SquadAction.GoTo)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_Depressed;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DirectControlledCrouching = false;
		SetSubGoal(character, parent, GetNextSubGoal(character, parent));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is StopSittingGoal)
		{
			return null;
		}
		if (!character.IsTooDepressedToFollowOrders())
		{
			if (character.IsSitting())
			{
				return new StopSittingGoal();
			}
			return null;
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.Success)
			{
				return new DrinkGoal(findGoal.FoundItem);
			}
			_lastSearchedTime = Session.Instance.PlayTime;
			_lastSearchFailed = true;
		}
		TimeSpan currentTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		if (character.WantDrinkAlcohol())
		{
			Equipment bestAlcoholToDrink = character.Inventory.GetBestAlcoholToDrink(character, character, ignorePolicy: false);
			if (bestAlcoholToDrink != null)
			{
				return new DrinkGoal(bestAlcoholToDrink);
			}
			if (currentTime - _lastSearchedTime >= (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
			{
				return new FindGoal(FindType.Alcohol, MovementType.Walk, critical: false);
			}
		}
		TerrainCoord tile = character.Tile;
		if (GameTerrain.Instance.IsTileRiver(tile.x, tile.y))
		{
			return null;
		}
		if (!character.IsSitting())
		{
			return new SitGoal();
		}
		return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(10f, 20f, MathUtil.RandomFloat((float)currentTime.TotalMilliseconds + (float)character.Id * 2343f))));
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is Wait && !character.IsTooDepressedToFollowOrders())
		{
			SetSubGoal(character, parent, new StopSittingGoal());
		}
	}
}
