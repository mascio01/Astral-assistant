using System;
using UnityEngine;

public class ChickenLayEggGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static TimeSpan RememberFindFailedTime = TimeSpan.FromSeconds(300.0);

	public override GoalType GetGoalType()
	{
		return GoalType.ChickenLayEggGoal;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Animal_LayEgg;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.EggIcon;
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
		if (character.Community == null)
		{
			return false;
		}
		if (!Active)
		{
			if (character.GetDontSimulateSurvivalFactorsUntilDiscovered() || character.GetDontSimulateSurvivalFactorsUntilJoinCommunity())
			{
				return false;
			}
			if (!((Chicken)character).IsReadyToLayEgg())
			{
				return false;
			}
			float num = Session.Instance.HourOfDay + MathUtil.RandomFloat(character.Id * 3995) - 0.5f;
			if (num < 7f || num > 14f)
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
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
		Building building = null;
		int entranceIndex = -1;
		float num = float.MaxValue;
		foreach (Prop building3 in character.Community.Buildings)
		{
			if (building3.Prototype == null || building3.Prototype.EnterableBySpecies != BaseObjectType.Chicken)
			{
				continue;
			}
			Building building2 = building3 as Building;
			if (!building2.CanEnter(character) || !building2.HasAnyInternalSlots() || (character.HasMovementZone() && !character.MovementZone.Overlaps(building2.GetTileRect())))
			{
				continue;
			}
			for (int i = 0; i < building2.GetEntranceDefs().Length; i++)
			{
				float sqrMagnitude = MathUtil.ToXZ(building2.GetEntrancePos(i) - character.Pos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					building = building2;
					entranceIndex = i;
					num = sqrMagnitude;
				}
			}
		}
		if (building != null)
		{
			SetSubGoal(character, parent, new MoveToAndEnterBuilding(character, building, entranceIndex, MovementType.Walk));
			return;
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		Finished = true;
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		if (SubGoal is MoveToAndEnterBuilding moveToAndEnterBuilding)
		{
			if (moveToAndEnterBuilding.Success)
			{
				_lastSearchFailed = false;
				return new Wait(TimeSpan.FromSeconds(Mathf.Lerp(30f, 60f, Session.Instance.DeterministicRand.RandomFloat())));
			}
			character.AddFailedFindAttempt(moveToAndEnterBuilding.GetTargetObject(), moveToAndEnterBuilding.EntranceIndex, 0, FindType.Warmth, null, null, null);
		}
		if (SubGoal is Wait && character.InsideBuilding != null)
		{
			Chicken chicken = (Chicken)character;
			Equipment equipment = Equipment.Spawn(chicken.IsFertilized() ? EquipmentPrototype.FertilizedEgg : EquipmentPrototype.Egg);
			character.InsideBuilding.Inventory.Add(character.InsideBuilding, equipment);
			chicken.EggProduction = 0f;
			if (equipment is FertilizedEgg fertilizedEgg)
			{
				chicken.BroodyStartTime = Session.Instance.PlayTime;
				fertilizedEgg.Mother = chicken;
			}
		}
		return base.GetNextSubGoal(character, parent);
	}
}
