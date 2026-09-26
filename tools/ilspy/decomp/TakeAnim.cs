using System;

public class TakeAnim : AnimationGoal
{
	private Equipment _targetEquipment;

	private Equipment PourIntoContainer;

	private float _desiredAmount;

	private bool _ignoreWeight;

	private bool _isGathering;

	public float AmountRetrieved;

	public TakeAnim()
		: base(ActionAnim.Scavenge)
	{
	}

	public TakeAnim(Equipment targetEquipment, float desiredAmount, bool ignoreWeight, Equipment pourIntoContainer, bool isGathering)
		: base(ActionAnim.Scavenge)
	{
		_targetEquipment = targetEquipment;
		PourIntoContainer = pourIntoContainer;
		_desiredAmount = desiredAmount;
		_ignoreWeight = ignoreWeight;
		_isGathering = isGathering;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _targetEquipment);
		reflector.AddAfter(ref PourIntoContainer, 72);
		if (reflector.Version < 72)
		{
			int value = 0;
			int value2 = 0;
			reflector.Add(ref value);
			reflector.Add(ref value2);
			_desiredAmount = value;
			AmountRetrieved = value2;
		}
		else
		{
			reflector.Add(ref _desiredAmount);
			reflector.Add(ref AmountRetrieved);
		}
		reflector.Add(ref _ignoreWeight);
		reflector.AddAfter(ref _isGathering, 4);
		if (reflector.Version < 353)
		{
			bool value3 = false;
			reflector.AddAfter(ref value3, 101);
		}
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TakeAnim;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Anim = GetTargetObject()?.GetTakeAnim() ?? ActionAnim.Scavenge;
		base.OnActivate(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (_targetEquipment != null && AmountRetrieved == 0f && (Target.Object.GetInventory() == null || !Target.Object.GetInventory().Contains(_targetEquipment)))
		{
			return false;
		}
		if (PourIntoContainer != null && AmountRetrieved == 0f && !character.GetInventory().Contains(PourIntoContainer))
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.Take)
		{
			TileObject targetObject = GetTargetObject();
			if (targetObject != null)
			{
				Take(character, targetObject, _targetEquipment, _desiredAmount, _ignoreWeight, PourIntoContainer, _isGathering, out AmountRetrieved);
				if (character.IsControllableByPlayer())
				{
					targetObject.MarkInvestigated(character);
				}
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public static void TakeAllItemsFromPot(Character character, TileObject targetObject, Equipment targetEquipment)
	{
		if (!(targetObject is Campfire) || (targetEquipment.GetPrototype() != EquipmentPrototype.Pot && targetEquipment.GetPrototype() != EquipmentPrototype.FryingPan))
		{
			return;
		}
		EquipmentContainer inventory = targetObject.GetInventory();
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			Equipment item = inventory.GetItem(num);
			if (item != targetEquipment)
			{
				item = inventory.Take(targetObject, item, item.GetAmount());
				if (item != null)
				{
					NotificationManager.Instance.AddEquipmentNotification(targetObject, character, item, item.GetAmount());
				}
				character.Inventory.Add(character, item);
			}
		}
	}

	public static void Take(Character character, TileObject targetObject, Equipment targetEquipment, float desiredAmount, bool ignoreWeight, Equipment pourIntoContainer, bool isGathering, out float amountRetrieved)
	{
		amountRetrieved = 0f;
		float num = 0f;
		if (targetEquipment == null || targetObject == null)
		{
			return;
		}
		character.RemoveFailedFindAttempt(targetObject);
		bool flag = !character.IsControllableByOrFollowingPlayer() && !targetObject.IsInvestigated() && targetObject.GetCommunity() == null && (targetEquipment.IsEdible() || targetEquipment.IsDrinkable() || targetEquipment.GetAntigenType() != InfectionType.None || targetEquipment.GetBandageLevel() != -1);
		if (pourIntoContainer != null)
		{
			LiquidPrototype liquidContentsType = targetEquipment.GetLiquidContentsType();
			pourIntoContainer.GetLiquidContentsType();
			if (liquidContentsType != null)
			{
				InfectionType infectedWith = targetEquipment.InfectedWith;
				float num2 = Math.Min(desiredAmount, pourIntoContainer.GetLiquidCapacity() - pourIntoContainer.GetLiquidContentsAmount());
				if (flag)
				{
					amountRetrieved = num2;
				}
				else
				{
					amountRetrieved = targetEquipment.DrainLiquid(num2);
				}
				num += amountRetrieved * liquidContentsType.BasePricePerFlOz;
				pourIntoContainer.FillLiquid(liquidContentsType, amountRetrieved, infectedWith);
			}
			if (isGathering && amountRetrieved > 0f)
			{
				pourIntoContainer.SetGathered();
			}
		}
		else
		{
			TakeAllItemsFromPot(character, targetObject, targetEquipment);
			int num3 = (int)Math.Min(desiredAmount, (character.GetMaxInventoryWeight() - character.Inventory.GetWeight(character)) / targetEquipment.GetWeight());
			if (ignoreWeight)
			{
				num3 = Math.Max(1, num3);
			}
			if (num3 <= 0)
			{
				return;
			}
			Equipment equipment;
			if (flag)
			{
				equipment = Equipment.Spawn(targetEquipment.GetPrototype(), num3);
				if (targetEquipment.GetLiquidContentsType() != null)
				{
					equipment.SetLiquid(targetEquipment.GetLiquidContentsType(), targetEquipment.GetLiquidContentsAmount());
				}
			}
			else
			{
				equipment = targetObject.GetInventory().Take(targetObject, targetEquipment, num3);
			}
			if (equipment != null)
			{
				amountRetrieved = equipment.GetAmount();
				NotificationManager.Instance.AddEquipmentNotification(targetObject, character, equipment, equipment.GetAmount());
				num += amountRetrieved * equipment.GetBasePrice();
				equipment = character.Inventory.Add(character, equipment);
			}
			if (isGathering && amountRetrieved > 0f)
			{
				equipment?.IncrementGatheredAmount((int)amountRetrieved);
			}
		}
		if (targetObject.GetCommunity() != null && targetObject.GetCommunity() != character.Community)
		{
			character.OnStoleSomething(targetObject.GetCommunity(), targetObject as Character, num);
		}
	}
}
