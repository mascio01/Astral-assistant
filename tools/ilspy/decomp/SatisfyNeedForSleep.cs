using System;
using UnityEngine;

public class SatisfyNeedForSleep : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static TimeSpan RememberFindFailedTime = TimeSpan.FromSeconds(300.0);

	public static GameProfiler _Timer = new GameProfiler("Update.SleepGoalSearch");

	private static float CostOfFailure = 10f;

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

	public override GoalType GetGoalType()
	{
		return GoalType.SatisfyNeedForSleep;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (character.CanSleep())
		{
			return true;
		}
		if (character.HasBeenPlayerControlledRecently(character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime, character.GetSleepDeprivation() >= Character.SleepDeprivationExtraCriticalTime))
		{
			return false;
		}
		if (!Active && character.DirectControlledCrouching && character.GetSleepDeprivation() < Character.SleepDeprivationExtraCriticalTime)
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
			if (character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime || (character.GetSleepDeprivation() > Character.SleepyTime && character.IsSleeping()))
			{
				return GoalPriority.Survivor_SatisfyNeeds_Critical_Active;
			}
			if (character.GetSleepDeprivation() > Character.SleepyTime || character.IsSleeping())
			{
				if (character.GetSleepDeprivation() <= 0f)
				{
					return GoalPriority.Survivor_SleepWhenNotTired;
				}
				return GoalPriority.Survivor_SatisfyNeeds_Active;
			}
			return GoalPriority.Impossible;
		}
		if (character.GetSleepDeprivation() < Character.SleepyTime)
		{
			return GoalPriority.Impossible;
		}
		if (character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime)
		{
			return GoalPriority.Survivor_Sleep_Critical;
		}
		if (character.ShouldRoleTakePriorityOverNeeds())
		{
			return GoalPriority.Impossible;
		}
		if (character.Community != null && character.Community.HasAnyBuildingsWithInternalSlots())
		{
			return GoalPriority.Survivor_Sleep;
		}
		return GoalPriority.Impossible;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.CanSleep())
		{
			MakeBuildingHangoutLocation(character, 0);
			SetSubGoal(character, parent, new Idle());
			return;
		}
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsControllableByPlayer();
		bool flag2 = false;
		float num = (flag ? 80f : 200f);
		if (!flag && character.Community != null && character.Community.IsAISettlement() && character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime)
		{
			character.Community.GetBaseCentre(out var centre, out var radius);
			num = Math.Max(num, centre.GetDist(character.Tile) + radius);
		}
		Building building = null;
		if (character.GetSleepDeprivation() >= Character.SleepDeprivationExtraCriticalTime)
		{
			num = float.MaxValue;
			flag2 = true;
		}
		else if (character.GetSleepDeprivation() >= Character.SleepDeprivationCriticalTime && character.Community != null && !character.Community.IsAmbientCommunity() && !character.Community.HasAnyBuildingsWithInternalSlots())
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
						num2 += 50f;
						break;
					case BaseObjectType.Mine:
						num2 += 60f;
						break;
					case BaseObjectType.EnterableVehicle:
						num2 += 50f;
						break;
					}
				}
				if (!building2.HasAnyInternalSlots() || (flag && !instance2.FogOfWar.IsAnyTileInRectCornersExplored(building2.MinTile, building2.MaxTile)) || num2 >= num)
				{
					continue;
				}
				if (character.HasFailedFindAttempt(building2, 0, 0, FindType.Sleep, null, null, null, RememberFindFailedTime, out var timeSinceAttempt, out var failCount))
				{
					num2 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFindFailedTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
					if (num2 >= num)
					{
						continue;
					}
				}
				building = building2;
				num = num2;
			}
		}
		if (building != null)
		{
			if (character.IsControllableByPlayer() && StoryManager.Instance.GetMostInterestingSpeaker() == null)
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.GoingToBed);
				if (speechForSituation != null)
				{
					character.Speak(speechForSituation);
				}
			}
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building));
		}
		else
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			_lastSearchedTime = Session.Instance.PlayTime;
			if (moveToAndEnterBuilding.Success)
			{
				MakeBuildingHangoutLocation(character, moveToAndEnterBuilding.EntranceIndex);
				_lastSearchFailed = false;
				return new Idle();
			}
			character.AddFailedFindAttempt(moveToAndEnterBuilding.GetTargetObject(), 0, 0, FindType.Sleep, null, null, null);
			_lastSearchFailed = true;
		}
		return base.GetNextSubGoal(character, parent);
	}

	private void MakeBuildingHangoutLocation(Character character, int entranceIndex)
	{
		if (!character.IsControllableByPlayer() || character.InsideBuilding == null || character.InsideBuilding.Community != character.Community)
		{
			return;
		}
		GameTerrain instance = GameTerrain.Instance;
		TerrainCoord entranceTile = character.InsideBuilding.GetEntranceTile(Math.Max(entranceIndex, 0));
		bool flag = instance.IsTileEnclosed(entranceTile.x, entranceTile.y);
		for (int i = 0; i < 100; i++)
		{
			TerrainCoord hangoutLocation = Session.Instance.DeterministicRand.RandomTileOnOutsideEdge(character.InsideBuilding.GetMinTile(), character.InsideBuilding.GetMaxTile(), 8);
			if (!instance.IsImpassable(hangoutLocation.x, hangoutLocation.y, 3, character, null) && !instance.IsTileRiver(hangoutLocation.x, hangoutLocation.y) && (!flag || instance.IsTileEnclosed(hangoutLocation.x, hangoutLocation.y)))
			{
				character.SetHangoutLocation(hangoutLocation);
				break;
			}
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is Idle && !character.CanSleep())
		{
			Finished = true;
		}
	}
}
