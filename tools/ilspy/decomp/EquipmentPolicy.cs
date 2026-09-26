public struct EquipmentPolicy : IReflectable
{
	public bool CanUse;

	public bool CanCraftWith;

	public bool CanPlant;

	public bool CanShare;

	public bool CanFeedToAnimals;

	public bool CanStrip;

	public bool AutoCollect;

	public bool AutoDeposit;

	public float TargetAmount;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public InfectionType InfectedWith;

	public static int NoRestrictionsPolicyMask = 175;

	public EquipmentPolicy(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		Proto = proto;
		Liquid = liquid;
		InfectedWith = infectionType;
		CanUse = true;
		CanCraftWith = true;
		CanPlant = true;
		CanShare = true;
		CanFeedToAnimals = true;
		CanStrip = true;
		AutoCollect = false;
		AutoDeposit = false;
		TargetAmount = ((proto != null) ? ((float)proto.DefaultCarryAmount) : 0f);
	}

	public void Reflect(Reflector reflector)
	{
		if (reflector.Version < 241)
		{
			EquipmentPolicyType_DEPRECATED value = EquipmentPolicyType_DEPRECATED.Invalid;
			reflector.Add(ref value);
			CanUse = value != EquipmentPolicyType_DEPRECATED.DontUse;
		}
		else
		{
			reflector.Add(ref CanUse);
			reflector.Add(ref CanCraftWith);
			reflector.Add(ref CanPlant);
			reflector.Add(ref CanShare);
			reflector.AddAfter(ref CanFeedToAnimals, 380);
			reflector.AddAfter(ref AutoCollect, 299);
			reflector.AddAfter(ref AutoDeposit, 372);
			reflector.AddAfter(ref CanStrip, 415);
			reflector.AddAfter(ref TargetAmount, 442);
		}
		reflector.Add(ref Proto);
		reflector.Add(ref Liquid);
		reflector.AddAfter(ref InfectedWith, 452);
		if (reflector.Version < 518 && Proto != null && TargetAmount == 0f)
		{
			TargetAmount = Proto.DefaultCarryAmount;
		}
	}

	public void SetPolicy(EquipmentPolicyAction action, bool on)
	{
		switch (action)
		{
		case EquipmentPolicyAction.CanUse:
			CanUse = on;
			break;
		case EquipmentPolicyAction.CanCraftWith:
			CanCraftWith = on;
			break;
		case EquipmentPolicyAction.CanPlant:
			CanPlant = on;
			break;
		case EquipmentPolicyAction.CanShare:
			CanShare = on;
			break;
		case EquipmentPolicyAction.CanFeedToAnimals:
			CanFeedToAnimals = on;
			break;
		case EquipmentPolicyAction.CanStrip:
			CanStrip = on;
			break;
		case EquipmentPolicyAction.AutoCollect:
			AutoCollect = on;
			break;
		case EquipmentPolicyAction.AutoDeposit:
			AutoDeposit = on;
			break;
		}
	}

	public bool GetPolicy(EquipmentPolicyAction action)
	{
		return action switch
		{
			EquipmentPolicyAction.CanUse => CanUse, 
			EquipmentPolicyAction.CanCraftWith => CanCraftWith, 
			EquipmentPolicyAction.CanPlant => CanPlant, 
			EquipmentPolicyAction.CanShare => CanShare, 
			EquipmentPolicyAction.CanFeedToAnimals => CanFeedToAnimals, 
			EquipmentPolicyAction.CanStrip => CanStrip, 
			EquipmentPolicyAction.AutoCollect => AutoCollect, 
			EquipmentPolicyAction.AutoDeposit => AutoDeposit, 
			_ => false, 
		};
	}

	public void SetPolicyMask(int policyMask, float targetAmount)
	{
		for (int i = 0; i < 8; i++)
		{
			SetPolicy((EquipmentPolicyAction)i, (policyMask & (1 << i)) != 0);
		}
		TargetAmount = targetAmount;
	}

	public int GetPolicyMask()
	{
		int num = 0;
		for (int i = 0; i < 8; i++)
		{
			if (GetPolicy((EquipmentPolicyAction)i))
			{
				num |= 1 << i;
			}
		}
		return num;
	}

	public static int GetDefaultActionsMask(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		if (proto != null && proto.ContainsHumanMeat)
		{
			return 0;
		}
		return NoRestrictionsPolicyMask;
	}

	public bool AreAllActionsSetToDefault()
	{
		if (GetPolicyMask() == GetDefaultActionsMask(Proto, Liquid))
		{
			return TargetAmount == ((Proto != null) ? ((float)Proto.DefaultCarryAmount) : 0f);
		}
		return false;
	}

	public static bool IsActionPossibleForItem(EquipmentPolicyAction action, EquipmentPrototype proto, LiquidPrototype liquid)
	{
		switch (action)
		{
		case EquipmentPolicyAction.CanUse:
			if (liquid?.Edible ?? (proto.GetNutrition() > 0f))
			{
				return true;
			}
			if (liquid != null && liquid.Drinkable)
			{
				return true;
			}
			if (proto != null)
			{
				if (EquipmentPrototype.AllAmmoTypes.Contains(proto))
				{
					return true;
				}
				if (BaseObjectManager.PrototypeGameObjects[(int)proto.TypeName] is MolotovCocktail || BaseObjectManager.PrototypeGameObjects[(int)proto.TypeName] is PipeBomb)
				{
					return true;
				}
				if (GameCursor.CanAddMaterialToFire(proto))
				{
					return true;
				}
			}
			return false;
		case EquipmentPolicyAction.CanCraftWith:
			return GameImpl.Instance.IsIngredientInAnyForRecipesWhichCanBeRecurring(proto, liquid);
		case EquipmentPolicyAction.CanPlant:
			if (proto != null)
			{
				return proto.GetSeedForPlantType() != null;
			}
			return false;
		case EquipmentPolicyAction.CanShare:
			if ((proto == null || !(proto.GetNutrition() > 0f)) && (liquid == null || !liquid.DrinkableOrEdible) && (proto == null || !(proto.LiquidCapacity > 0f)))
			{
				if (proto != null)
				{
					return proto.SeedForPlantTypeProto != null;
				}
				return false;
			}
			return true;
		case EquipmentPolicyAction.CanFeedToAnimals:
			if (proto != null)
			{
				return proto.GetNutrition() > 0f;
			}
			return false;
		case EquipmentPolicyAction.CanStrip:
			if (proto != null && proto.ClothingType != ClothingType.Invalid)
			{
				return proto.ClothingType != ClothingType.Backpack;
			}
			return false;
		case EquipmentPolicyAction.AutoCollect:
		case EquipmentPolicyAction.AutoDeposit:
			return true;
		default:
			return false;
		}
	}

	public static bool IsTargetAmountPossibleForItem(EquipmentPrototype proto, LiquidPrototype liquid, int policyMask)
	{
		return true;
	}

	public bool Matches(Equipment item)
	{
		return Matches(item.GetPrototype(), item.GetLiquidContentsType(), item.InfectedWith);
	}

	public bool Matches(EquipmentPrototype proto, LiquidPrototype liquid, InfectionType infectionType)
	{
		if (InfectedWith != infectionType)
		{
			return false;
		}
		if (Liquid != null || liquid != null)
		{
			return Liquid == liquid;
		}
		return Proto == proto;
	}

	public bool MatchesIgnoringInfection(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		if (Liquid != null || liquid != null)
		{
			return Liquid == liquid;
		}
		return Proto == proto;
	}

	public bool HasTargetCarryAmount(Character character)
	{
		if (TargetAmount > 0f)
		{
			if (Proto != null)
			{
				return (float)character.Inventory.CountItemsOfType(Proto, InfectedWith) >= TargetAmount;
			}
			if (Liquid != null)
			{
				return character.Inventory.GetAmountOfLiquidType(Liquid, InfectedWith) >= TargetAmount;
			}
		}
		return true;
	}
}
