using System;
using System.Collections.Generic;
using UnityEngine;

public class RestockAmmoGoal : StateMachineGoal
{
	private TimeSpan _lastSearchedTime;

	private bool _lastSearchFailed;

	public static TimeSpan MinTimeBetweenSearches = TimeSpan.FromSeconds(30.0);

	public static TimeSpan MinTimeBetweenSuccessfulSearches = TimeSpan.FromSeconds(10.0);

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return IsRestockingAmmo(character) ? GameCursor.CursorAmmo : GameCursor.CursorTake;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _lastSearchedTime);
		reflector.Add(ref _lastSearchFailed);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.RestockAmmoGoal;
	}

	public override bool IsSatisfyingNeeds()
	{
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.IsControllableByPlayer())
		{
			return false;
		}
		if (character.HasBeenPlayerControlledRecently(critical: true, extraCritical: false))
		{
			return false;
		}
		if (!Active)
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
		if (character.Inventory.GetWeight(character) > character.GetMaxInventoryWeight() - 1f)
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
		if (character.HasInfiniteAmmo(null))
		{
			return GoalPriority.Impossible;
		}
		if (Active && character.IsCurrentActionAnimFromGoal)
		{
			return GoalPriority.Survivor_Animation;
		}
		List<EquipmentPolicy> allCarryAmountPolicies = character.GetAllCarryAmountPolicies();
		if (allCarryAmountPolicies != null)
		{
			for (int i = 0; i < allCarryAmountPolicies.Count; i++)
			{
				if (!allCarryAmountPolicies[i].HasTargetCarryAmount(character))
				{
					if (!_lastSearchFailed && character.Inventory.IsOutOfAmmo(character))
					{
						return GoalPriority.Survivor_RestockAmmo_Critical;
					}
					return GoalPriority.Survivor_RestockAmmo;
				}
			}
		}
		return GoalPriority.Impossible;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Session instance = Session.Instance;
		SetSubGoal(character, parent, new FindGoal(FindType.Ammo, MovementType.Walk, critical: false));
		if (SubGoal.Finished && !((FindGoal)SubGoal).Success)
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
		if (SubGoal is FindGoal { Success: not false })
		{
			foreach (Target target in character.Targets)
			{
				target.ClearInaccessible();
			}
			_lastSearchFailed = false;
			List<EquipmentPolicy> allCarryAmountPolicies = character.GetAllCarryAmountPolicies();
			if (allCarryAmountPolicies != null)
			{
				for (int i = 0; i < allCarryAmountPolicies.Count; i++)
				{
					if (!allCarryAmountPolicies[i].HasTargetCarryAmount(character))
					{
						return new FindGoal(FindType.Ammo, MovementType.Walk, critical: false);
					}
				}
			}
			return null;
		}
		return base.GetNextSubGoal(character, parent);
	}

	private bool IsRestockingAmmo(Character character)
	{
		if (SubGoal is FindGoal findGoal)
		{
			if (findGoal.SubGoal is MoveToAndTake moveToAndTake)
			{
				return moveToAndTake.GetTargetEquipment()?.GetPrototype().IsAmmo() ?? false;
			}
			if (findGoal.SubGoal is MoveToAndGrab moveToAndGrab)
			{
				TileObject targetObject = moveToAndGrab.GetTargetObject();
				if (targetObject != null && targetObject.GetGrabbableEquipmentType() != null)
				{
					return targetObject.GetGrabbableEquipmentType().IsAmmo();
				}
				return false;
			}
			if (findGoal.SubGoal is Conversation conversation)
			{
				if (conversation.OpeningSpeechObject is Equipment equipment)
				{
					return equipment.GetPrototype().IsAmmo();
				}
				return false;
			}
		}
		return false;
	}
}
