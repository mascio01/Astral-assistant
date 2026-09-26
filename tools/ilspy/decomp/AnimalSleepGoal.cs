using System;
using UnityEngine;

public class AnimalSleepGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static GameProfiler _Timer = new GameProfiler("Update.AnimalSleepGoalSearch");

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalSleepGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (WasStashedInBuildingRecently(character))
		{
			return GoalPriority.Animal_SleepInVehicle;
		}
		return GoalPriority.Animal_Sleep;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorSleep;
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

	private bool WasStashedInBuildingRecently(Character character)
	{
		if (character.InsideBuilding != null)
		{
			if (character.InsideBuilding.IsMovingVehicle())
			{
				return true;
			}
			if (character.WasOrderedInsideBuilding && PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - character.LastEnteredOrExitedBuildingTime < TimeSpan.FromSeconds(20.0))
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (WasStashedInBuildingRecently(character))
		{
			return true;
		}
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (character.CanSleep())
		{
			return true;
		}
		if (character.GetSleepDeprivation() < Character.SleepDeprivationCriticalTime && Sun.GetSunIntensity(Session.Instance.DaysSinceStart) > 0.5f)
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
		else if (SubGoal is Idle)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.CanSleep())
		{
			SetSubGoal(character, parent, new Idle());
			return;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsOnPlayersTeam();
		bool flag2 = false;
		float num = 200f;
		Building building = null;
		if (character.GetSleepDeprivation() >= Character.SleepDeprivationDieTime - Sun.DayLengthSecs)
		{
			num = float.MaxValue;
			flag2 = true;
		}
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (!(allProp is Building building2) || !building2.CanEnter(character) || building2.IsBurning())
				{
					continue;
				}
				float num2 = building2.Tile.GetDist(tile);
				if (building2.Prototype.EnterableBySpecies != character.GetBaseObjectType())
				{
					num2 += 1000f;
				}
				if (building2.Community != character.Community)
				{
					if (!flag2)
					{
						continue;
					}
					num2 += 250f;
				}
				else
				{
					switch (allProp.GetBaseObjectType())
					{
					case BaseObjectType.Outhouse:
						num2 += 30f;
						break;
					case BaseObjectType.Mine:
						num2 += 50f;
						break;
					}
				}
				if (!(num2 >= num) && building2.HasAnyInternalSlots() && (!character.HasMovementZone() || character.MovementZone.Overlaps(building2.GetTileRect())) && (!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(building2.MinTile, building2.MaxTile)))
				{
					building = building2;
					num = num2;
				}
			}
		}
		if (building != null)
		{
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building));
			return;
		}
		_lastSearchedTime = instance.PlayTime;
		_lastSearchFailed = true;
		Finished = true;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is MoveToAndEnterBuilding { Success: not false })
		{
			_lastSearchFailed = false;
			return new Idle();
		}
		return base.GetNextSubGoal(character, parent);
	}
}
