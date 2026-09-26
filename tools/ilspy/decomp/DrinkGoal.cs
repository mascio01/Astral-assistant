public class DrinkGoal : StateMachineGoal
{
	public Equipment _drink;

	public LiquidPrototype Liquid;

	public Equipment PreviouslyEquipped;

	public bool Success;

	public DrinkGoal()
	{
	}

	public DrinkGoal(Equipment drink)
	{
		_drink = drink;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _drink);
		reflector.AddAfter(ref Liquid, 13);
		reflector.AddAfter(ref PreviouslyEquipped, 39);
		reflector.AddAfter(ref Success, 39);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.DrinkGoal;
	}

	public override bool IsUsingEquippedItem()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetCrouching(character, parent, Crouching || character.WasCrouching);
		Liquid = _drink.GetLiquidContentsType();
		if (character.EquippedItem != _drink)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(_drink));
		}
		else
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.Drink));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip equip && equip.Equipment == _drink && !Success)
		{
			return new AnimationGoal(ActionAnim.Drink);
		}
		if (SubGoal is AnimationGoal && character.EquippedItem == _drink && _drink != null && _drink.GetLiquidContentsAmount() > 0f && _drink.GetLiquidContentsAmount() > 0f && Liquid.AlcoholContent <= 0f && character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None) && ((Liquid.WaterContent < 0f && character.GetThirst() < Character.ThirstCriticalTime) || (Liquid.Drinkable && character.GetThirst() >= Character.ThirstyTime) || (Liquid.Edible && character.GetHunger() > Character.HungryTime)))
		{
			return new AnimationGoal(ActionAnim.Drink);
		}
		if (SubGoal is AnimationGoal && Liquid == LiquidPrototype.Wine && Success && _drink.GetLiquidContentsAmount() == 0f && character.Speak(StoryManager.Instance.GetSpeechForSituation(character, null, SpeechSituation.ConsumeWine)))
		{
			return new Idle();
		}
		if (SubGoal is AnimationGoal && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			return new Equip(PreviouslyEquipped);
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.StartDrink:
		{
			Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, _drink, SpeechSituation.Drinking);
			character.Speak(speechForSituation, null, _drink);
			return true;
		}
		case AnimationEventType.FinishDrink:
			if (_drink != null && character.InventoryContains(_drink))
			{
				if (_drink.GetLiquidCapacity() > 0f)
				{
					character.ConsumeLiquid(_drink, playSound: false, fromInfoScreen: false);
					Success = true;
				}
				else
				{
					Equipment equipment = character.Inventory.Take(character, _drink, 1);
					if (equipment != null)
					{
						NotificationManager.Instance.AddEquipmentNotification(character, character, _drink, 1);
						character.Eat(equipment, playSound: false, fromInfoScreen: false, null);
						equipment.Delete();
						Success = true;
					}
				}
			}
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}

	public override void OnSpeechFinished(Character character, Goal parent, Speech speech, Character listener, bool interrupted, Speech cont, BaseObject continueObject, MemoryParam continueParam)
	{
		if (speech.Situation == SpeechSituation.ConsumeWine)
		{
			if (PreviouslyEquipped != null && SubGoal is Idle && character.InventoryContains(PreviouslyEquipped))
			{
				SetSubGoal(character, parent, new Equip(PreviouslyEquipped));
			}
			else
			{
				Finished = true;
			}
		}
		base.OnSpeechFinished(character, parent, speech, listener, interrupted, cont, continueObject, continueParam);
	}

	public override bool IsSubstantiallyFinished(Character character)
	{
		return Success;
	}
}
