using System;
using UnityEngine;

public class AnimalFeederGoal : RoleGoal
{
	private TimeSpan LastAttemptedTime = TimeSpan.FromDays(-365.0);

	private Prop PropToFill;

	public static TimeSpan MinTimeBetweenAttempts = TimeSpan.FromSeconds(10.0);

	public override GoalType GetGoalType()
	{
		return GoalType.AnimalFeederGoal;
	}

	public override Texture2D GetOverheadActionIcon(Character character)
	{
		return GameCursor.ChickenIcon;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		if (Active)
		{
			WaterPlant waterPlant = SubGoal as WaterPlant;
			MoveToAndDeposit moveToAndDeposit = SubGoal as MoveToAndDeposit;
			if ((waterPlant != null && waterPlant.SubGoal is AnimationGoal) || (moveToAndDeposit != null && moveToAndDeposit.SubGoal is AnimationGoal))
			{
				return GoalPriority.Survivor_Role_Animation;
			}
		}
		return (GoalPriority)(210 - character.GetFirstRoleIndex(Role.AnimalFeeder));
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (!character.HasRunningRole(Role.AnimalFeeder))
		{
			return false;
		}
		if (character.Community == null)
		{
			return false;
		}
		if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.AnimalFeeder))
		{
			return false;
		}
		if (!Active && Session.Instance.PlayTime - LastAttemptedTime < MinTimeBetweenAttempts)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref LastAttemptedTime);
		reflector.Add(ref MovementType);
		reflector.Add(ref PropToFill);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.SetRoleInProgress(new RoleInfo(Role.AnimalFeeder), inProgress: true);
		Goal nextSubGoal = GetNextSubGoal(character, parent);
		if (nextSubGoal != null)
		{
			character.DirectControlledCrouching = false;
			SetSubGoal(character, parent, nextSubGoal);
		}
		else
		{
			Finished = true;
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is WaterPlant waterPlant)
		{
			if (waterPlant.Success)
			{
				character.SetRoleInProgress(new RoleInfo(Role.AnimalFeeder), inProgress: false);
				character.OnRoleSucceeded();
			}
			else
			{
				OnFailed(character);
			}
			return null;
		}
		if (SubGoal is MoveToAndDeposit moveToAndDeposit)
		{
			if (moveToAndDeposit.SuccessfullyReachedDestination)
			{
				character.SetRoleInProgress(new RoleInfo(Role.AnimalFeeder), inProgress: false);
				character.OnRoleSucceeded();
			}
			else
			{
				OnFailed(character);
			}
			return null;
		}
		if (SubGoal is FindGoal findGoal)
		{
			if (!findGoal.Success || PropToFill == null || PropToFill.Deleted)
			{
				OnFailed(character);
				return null;
			}
			if (findGoal.FindType == FindType.WaterForAnimals)
			{
				return new WaterPlant(character, PropToFill, findGoal.FoundItem, MovementType);
			}
			if (findGoal.FindType == FindType.FoodForAnimals && findGoal.FoundItem != null && character.InventoryContains(findGoal.FoundItem))
			{
				int num = Math.Min(findGoal.FoundItem.GetAmount(), PropToFill.GetAmountOfEquipmentThatCanBeStored(findGoal.FoundItem));
				if (num > 0)
				{
					return new MoveToAndDeposit(character, PropToFill, findGoal.FoundItem, num, MovementType);
				}
				OnFailed(character);
				return null;
			}
		}
		TerrainCoord tile = character.Tile;
		Prop prop = null;
		float num2 = float.MaxValue;
		if (character.Community != null)
		{
			foreach (Prop building in character.Community.Buildings)
			{
				AnimalFeederProp animalFeederProp = building as AnimalFeederProp;
				AnimalDrinkerProp animalDrinkerProp = building as AnimalDrinkerProp;
				if (animalFeederProp != null)
				{
					if (animalFeederProp.Inventory.GetFoodForAnimal(BaseObjectType.Chicken, character) != null)
					{
						continue;
					}
				}
				else if (animalDrinkerProp == null || animalDrinkerProp.GetLiquidAmount() >= animalDrinkerProp.GetPropPrototype().LiquidCapacity * 0.5f)
				{
					continue;
				}
				float dist = building.Tile.GetDist(tile);
				if (dist < num2)
				{
					num2 = dist;
					prop = building;
				}
			}
		}
		if (prop != null)
		{
			AnimalFeederProp obj = prop as AnimalFeederProp;
			AnimalDrinkerProp animalDrinkerProp2 = prop as AnimalDrinkerProp;
			PropToFill = prop;
			if (obj != null)
			{
				return new FindGoal(FindType.FoodForAnimals, MovementType, critical: false);
			}
			if (animalDrinkerProp2 != null)
			{
				return new FindGoal(FindType.WaterForAnimals, MovementType, critical: false);
			}
			return base.GetNextSubGoal(character, parent);
		}
		OnFailed(character);
		return null;
	}

	private void OnFailed(Character character)
	{
		character.SetRoleFailedRecently(new RoleInfo(Role.AnimalFeeder));
		LastAttemptedTime = Session.Instance.PlayTime;
	}

	public void ResetLastAttemptedTime()
	{
		LastAttemptedTime = TimeSpan.FromDays(-365.0);
	}

	public override bool IsDoingSomethingTerriblyImportant()
	{
		return true;
	}

	public override RoleInfo GetRoleInfoBeingPerformed(Character character)
	{
		return new RoleInfo(Role.AnimalFeeder);
	}
}
