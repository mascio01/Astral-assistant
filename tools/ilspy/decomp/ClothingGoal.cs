using System;
using System.Collections.Generic;
using UnityEngine;

public class ClothingGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public static float MinAverageTempDeltaToChangeClothes = 5f;

	public static float CriticalAverageTempDeltaToChangeClothes = 10f;

	public override GoalType GetGoalType()
	{
		return GoalType.ClothingGoal;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.CursorClothing;
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
		bool critical = false;
		if (!WantClothes(character, out critical))
		{
			return false;
		}
		if (character.HasBeenPlayerControlledRecently(critical, extraCritical: false))
		{
			return false;
		}
		if (!Active)
		{
			if (!critical || !(character.GetBodyTemperatureInCelsius() <= Character.BodyTemperatureInCelsiusModerateHypothermia))
			{
				if (character.DirectControlledCrouching)
				{
					return false;
				}
				if (character.CarryingObject != null)
				{
					return false;
				}
			}
			if (Session.Instance.PlayTime - _lastSearchedTime < (_lastSearchFailed ? MinTimeBetweenSearches : MinTimeBetweenSuccessfulSearches))
			{
				return false;
			}
			if (_lastSearchFailed && character.IsSatisfyingNeeds() && character.FindActiveGoal(GoalType.WarmGoal) == null)
			{
				return false;
			}
			if (!IsAllowedToChangeAnyClothes(character))
			{
				return false;
			}
		}
		return base.IsPossible(character, parent);
	}

	private bool IsAllowedToChangeAnyClothes(Character character)
	{
		for (int i = 0; i < 9; i++)
		{
			if (EquipmentPrototype.ClothingTypesThatHaveInsulation[i])
			{
				if (character.Clothes[i] == null)
				{
					return true;
				}
				if (character.IsActionAllowedForItem(character.Clothes[i], EquipmentPolicyAction.CanStrip))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool WantClothes(Character character, out bool critical)
	{
		float f = character.CalcAverageTemperatureInsulationDelta();
		if (Mathf.Abs(f) >= MinAverageTempDeltaToChangeClothes)
		{
			critical = Mathf.Abs(f) >= CriticalAverageTempDeltaToChangeClothes;
			return true;
		}
		if (character.Clothes[4] == null || character.Clothes[3] == null || character.Clothes[2] == null)
		{
			critical = true;
			return true;
		}
		critical = false;
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		bool critical = false;
		WantClothes(character, out critical);
		if (!Active)
		{
			if (!critical)
			{
				return GoalPriority.Survivor_Clothing;
			}
			return GoalPriority.Survivor_Clothing_Critical;
		}
		return GoalPriority.Survivor_Clothing_Active;
	}

	public static int ExchangeClothing(TileObject carrier, Character character, bool critical)
	{
		int num = 0;
		EquipmentContainer inventory = carrier.GetInventory();
		if (inventory == null)
		{
			return num;
		}
		List<Equipment> list = null;
		int num2 = inventory.Count + 10;
		for (int i = 0; i < num2; i++)
		{
			Equipment bestClothingForCharacter = inventory.GetBestClothingForCharacter(carrier, character, critical, list);
			if (bestClothingForCharacter == null)
			{
				return num;
			}
			for (int j = 0; j < character.Clothes.Length; j++)
			{
				Equipment equipment = character.Clothes[j];
				if (equipment != null && !bestClothingForCharacter.GetPrototype().IsClothingCompatible(equipment.GetPrototype()))
				{
					equipment.Strip(character);
					equipment = character.Inventory.Take(character, equipment, 1);
					if (equipment != null)
					{
						NotificationManager.Instance.AddEquipmentNotification(character, carrier, equipment, equipment.GetAmount());
					}
					equipment = inventory.Add(carrier, equipment);
					if (list == null)
					{
						list = new List<Equipment>();
					}
					list.Add(equipment);
				}
			}
			bestClothingForCharacter = inventory.Take(carrier, bestClothingForCharacter, 1);
			bestClothingForCharacter = character.Inventory.Add(character, bestClothingForCharacter);
			bestClothingForCharacter.Wear(character);
			num++;
		}
		Debug.LogError("Unexpected number of iterations in ExchangeClothing! " + num2);
		return num;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		if (character.Inventory.GetBestClothingForCharacter(character, character, critical: false, null) != null)
		{
			ExchangeClothing(character, character, critical: false);
			Finished = true;
			return;
		}
		bool critical = false;
		WantClothes(character, out critical);
		SetSubGoal(character, parent, new FindGoal(FindType.Clothing, (!critical) ? MovementType.Walk : MovementType.Run, critical));
		if (SubGoal.Finished && !((FindGoal)SubGoal).Success)
		{
			_lastSearchedTime = instance.PlayTime;
			_lastSearchFailed = true;
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is FindGoal { Success: not false } findGoal)
		{
			if (findGoal.FoundItem != null)
			{
				ExchangeClothing(character, character, critical: false);
			}
			_lastSearchFailed = false;
			return null;
		}
		_lastSearchedTime = Session.Instance.PlayTime;
		_lastSearchFailed = true;
		return base.GetNextSubGoal(character, parent);
	}
}
