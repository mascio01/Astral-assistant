using System;
using UnityEngine;

public class AnimalKeepWarm : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static TimeSpan RememberFindFailedTime = TimeSpan.FromSeconds(300.0);

	private static float CostOfFailure = 10f;

	public static GameProfiler _Timer = new GameProfiler("Update.AnimalKeepWarmSearch");

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalKeepWarm;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		if (SubGoal is Idle)
		{
			return null;
		}
		if (SubGoal is MoveToAndEnterBuilding)
		{
			return GameCursor.CursorShelter;
		}
		return GameCursor.CursorCampfire;
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

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
		{
			return false;
		}
		if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusNormal && character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) != null)
		{
			return true;
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
		if (Active && character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusMildHypothermia)
		{
			return GoalPriority.Animal_SatisfyNeeds_Critical_Active;
		}
		if (character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			return GoalPriority.Animal_KeepWarm_Critical;
		}
		if (Active && character.GetBodyTemperatureInCelsius() < Character.BodyTemperatureInCelsiusNormal)
		{
			return GoalPriority.Animal_SatisfyNeeds_Active;
		}
		if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusShivering)
		{
			return GoalPriority.Animal_KeepWarm;
		}
		return GoalPriority.Impossible;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsControllableByPlayer();
		float num = (flag ? 80f : 200f);
		TileObject tileObject = null;
		int entranceIndex = -1;
		bool flag2 = false;
		if (character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia)
		{
			num = float.MaxValue;
			flag2 = true;
		}
		using (new ProfileMarker(_Timer))
		{
			foreach (Prop allProp in instance.PropManager.AllProps)
			{
				if (allProp is Building building)
				{
					if (!building.CanEnter(character) || !building.HasAnyInternalSlots() || (character.HasMovementZone() && !character.MovementZone.Overlaps(building.GetTileRect())) || (flag && !instance2.FogOfWar.IsAnyTileInRectCornersExplored(building.MinTile, building.MaxTile)))
					{
						continue;
					}
					for (int i = 0; i < building.GetEntranceDefs().Length; i++)
					{
						float num2 = MathUtil.ToXZ(building.GetEntrancePos(i) - character.Pos).magnitude;
						if (building.Community != character.Community)
						{
							if (!flag2)
							{
								continue;
							}
							num2 += 250f;
						}
						else if (building.GetPropPrototype().EnterableBySpecies == character.GetBaseObjectType())
						{
							num2 -= 250f;
						}
						if (num2 >= num)
						{
							continue;
						}
						if (character.HasFailedFindAttempt(building, 0, 0, FindType.Warmth, null, null, null, RememberFindFailedTime, out var timeSinceAttempt, out var failCount))
						{
							num2 += (float)MathUtil.Squared(failCount) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt.TotalSeconds / (float)RememberFindFailedTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
							if (num2 >= num)
							{
								continue;
							}
						}
						tileObject = building;
						entranceIndex = i;
						num = num2;
					}
				}
				if (!(allProp is Campfire campfire))
				{
					continue;
				}
				float num3 = campfire.Tile.GetDist(tile);
				if (campfire.State != CampfireState.Burning || num3 >= num)
				{
					continue;
				}
				if (campfire.Community != character.Community)
				{
					if (character.WantToAvoidCommunity(campfire.GetCommunity()))
					{
						continue;
					}
					num3 += 50f;
				}
				if ((character.HasMovementZone() && !character.MovementZone.Contains(campfire.Tile)) || (flag && !instance2.FogOfWar.IsAnyTileInRectCornersExplored(campfire.MinTile, campfire.MaxTile)))
				{
					continue;
				}
				if (character.HasFailedFindAttempt(campfire, 0, 0, FindType.Warmth, null, null, null, RememberFindFailedTime, out var timeSinceAttempt2, out var failCount2))
				{
					num3 += (float)MathUtil.Squared(failCount2) * CostOfFailure + MathUtil.Squared(Mathf.Clamp01(1f - (float)timeSinceAttempt2.TotalSeconds / (float)RememberFindFailedTime.TotalSeconds)) * (float)GameTerrain.Instance.Size;
					if (num3 >= num)
					{
						continue;
					}
				}
				tileObject = campfire;
				entranceIndex = -1;
				num = num3;
			}
		}
		if (tileObject != null)
		{
			if (tileObject is Campfire campfire2)
			{
				SetSubGoal(character, parent, new SitAroundFireGoal(character, campfire2, MovementType.Walk));
			}
			if (tileObject is Building targetBuilding)
			{
				SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, targetBuilding, entranceIndex, MovementType.Walk));
			}
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
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is SitAroundFireGoal sitAroundFireGoal)
		{
			if (sitAroundFireGoal.Success)
			{
				_lastSearchFailed = false;
				return new Idle();
			}
			character.AddFailedFindAttempt(sitAroundFireGoal.GetTargetObject(), 0, 0, FindType.Warmth, null, null, null);
		}
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				_lastSearchFailed = false;
				return new Idle();
			}
			character.AddFailedFindAttempt(moveToAndEnterBuilding.GetTargetObject(), moveToAndEnterBuilding.EntranceIndex, 0, FindType.Warmth, null, null, null);
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal is Idle && character.IsOutdoors() && character.GetNearestBurningCampfire(TileObject.FireIdealWarmthRange) == null)
		{
			Finished = true;
		}
	}
}
