using System;
using UnityEngine;

public class RepairArmorGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.KevlarIcon;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.RepairArmorGoal;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.HasBeenPlayerControlledRecently(critical: false, extraCritical: false))
		{
			return false;
		}
		if (Active)
		{
			if (SubGoal is MoveToBenchAndRepairArmor moveToBenchAndRepairArmor && moveToBenchAndRepairArmor.IsAnimating())
			{
				return true;
			}
		}
		else
		{
			if (character.IsCrouching())
			{
				return false;
			}
			if (Session.Instance.PlayTime - _lastSearchedTime < (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
			{
				return false;
			}
			if (_lastSearchFailed && character.IsSatisfyingNeeds())
			{
				return false;
			}
		}
		return character.Inventory.HasAnyArmorThatNeedsRepairing();
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_RepairArmor;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		TerrainCoord tile = character.Tile;
		bool flag = character.IsControllableByPlayer();
		float num = (flag ? 120f : 200f);
		WorkBench workBench = null;
		foreach (Prop allProp in instance.PropManager.AllProps)
		{
			if (allProp.GetBaseObjectType() == BaseObjectType.WorkBench && allProp.CanRepairArmor() && allProp.GetUnderConstructionInfo() == null)
			{
				float num2 = allProp.Tile.GetDist(tile);
				if (allProp.Community != character.Community)
				{
					num2 += ((allProp.Community == null) ? 80f : 100f);
				}
				if ((!flag || instance2.FogOfWar.IsAnyTileInRectCornersExplored(allProp.MinTile, allProp.MaxTile)) && !(num2 >= num) && !character.WantToAvoidCommunity(allProp.GetCommunity()) && (!character.HasMovementZone() || character.MovementZone.Contains(((WorkBench)allProp).GetTileToStandOn(character))))
				{
					workBench = (WorkBench)allProp;
					num = num2;
				}
			}
		}
		if (workBench != null)
		{
			if (workBench.CurrentCrafter != null || character.Community == null || character.Community.IsAnyMemberRepairingArmorAtBench(workBench))
			{
				_lastSearchedTime = instance.PlayTime;
				_lastSearchFailed = false;
				Finished = true;
			}
			else
			{
				SetSubGoal(character, parent, new MoveToBenchAndRepairArmor(character, workBench, MovementType.Walk));
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
		if (SubGoal is MoveToBenchAndRepairArmor { Success: not false })
		{
			_lastSearchFailed = false;
		}
		return base.GetNextSubGoal(character, parent);
	}
}
