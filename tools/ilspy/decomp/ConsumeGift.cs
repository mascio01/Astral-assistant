internal class ConsumeGift : StateMachineGoal
{
	public override GoalType GetGoalType()
	{
		return GoalType.ConsumeGift;
	}

	private bool CanConsumeGift(Character character)
	{
		if (character.IsControllableByPlayer() && character.Community.AreAnyMembersInCombat())
		{
			return false;
		}
		return character.InTerrain;
	}

	private bool CanEatGift(Character character, Equipment item)
	{
		if (item.GetNutrition() <= character.GetHunger())
		{
			return true;
		}
		if (item.GetLiquidCapacity() > 0f && character.GetHunger() >= Character.HungryTime)
		{
			return true;
		}
		return false;
	}

	private bool CanDrinkGift(Character character, Equipment item)
	{
		LiquidPrototype liquidContentsType = item.GetLiquidContentsType();
		if (liquidContentsType != null)
		{
			if (liquidContentsType.WaterContent < 0f && character.GetThirst() > Character.ThirstCriticalTime)
			{
				return false;
			}
			if (liquidContentsType.WaterContent > 0f && character.GetThirst() < Character.ThirstyTime)
			{
				return false;
			}
			if (liquidContentsType.AlcoholContent > 0f)
			{
				if (character.DirectControlled)
				{
					return false;
				}
				if (!character.WantDrinkAlcohol())
				{
					return false;
				}
			}
		}
		else
		{
			float water = item.GetPrototype().GetWater();
			if (water < 0f && character.GetThirst() > Character.ThirstCriticalTime)
			{
				return false;
			}
			if (water > 0f && water < character.GetThirst())
			{
				return false;
			}
		}
		return true;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.HasBeenPlayerControlledRecently(critical: false, extraCritical: false))
		{
			return false;
		}
		if (!Active)
		{
			if (!character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None))
			{
				return false;
			}
			if (!CanConsumeGift(character))
			{
				return false;
			}
			for (int i = 0; i < character.Inventory.Count; i++)
			{
				Equipment item = character.Inventory.GetItem(i);
				if (!item.Gifted)
				{
					continue;
				}
				if (item.IsEdible())
				{
					if (!CanEatGift(character, item))
					{
						continue;
					}
				}
				else if (item.IsDrinkable())
				{
					if (!CanDrinkGift(character, item))
					{
						continue;
					}
				}
				else if (!(item is Book))
				{
					continue;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_ConsumeGift;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		for (int i = 0; i < character.Inventory.Count; i++)
		{
			Equipment item = character.Inventory.GetItem(i);
			if (!item.Gifted)
			{
				continue;
			}
			if (item.IsDrinkable() && CanDrinkGift(character, item))
			{
				if (character.DislikesGift(item, null))
				{
					SetSubGoal(character, parent, new PourAway(item));
				}
				else
				{
					SetSubGoal(character, parent, new DrinkGoal(item));
				}
				break;
			}
			if (character.DislikesGift(item, null))
			{
				character.Inventory.Take(character, item, item.GetAmount()).Delete();
				Finished = true;
				break;
			}
			if (item.IsEdible() && CanEatGift(character, item))
			{
				SetSubGoal(character, parent, new EatGoal(item));
				break;
			}
			if (item is Book)
			{
				SetSubGoal(character, parent, new ReadGoal(item));
				break;
			}
		}
		if (SubGoal == null)
		{
			Finished = true;
		}
	}
}
