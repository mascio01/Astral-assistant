public class EatGoal : StateMachineGoal
{
	public Equipment _food;

	public Equipment PreviouslyEquipped;

	public bool Success;

	public EatGoal()
	{
	}

	public EatGoal(Equipment food)
	{
		_food = food;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _food);
		reflector.AddAfter(ref PreviouslyEquipped, 39);
		reflector.AddAfter(ref Success, 39);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.EatGoal;
	}

	public override bool IsUsingEquippedItem()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetCrouching(character, parent, Crouching || character.WasCrouching);
		if (character.EquippedItem != _food)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(_food));
		}
		else
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.Eat));
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
		if (SubGoal is Equip equip && equip.Equipment == _food && !Success)
		{
			return new AnimationGoal(ActionAnim.Eat);
		}
		if (SubGoal is AnimationGoal && character.EquippedItem != null && character.EquippedItem == _food && character.GetHunger() > _food.GetNutrition() && _food.GetNutrition() > 0f && character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None))
		{
			return new AnimationGoal(ActionAnim.Eat);
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
		case AnimationEventType.StartEat:
			if (_food != null && character.InventoryContains(_food))
			{
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(character, null, _food, SpeechSituation.Eating);
				character.Speak(speechForSituation, null, _food);
			}
			break;
		case AnimationEventType.FinishEat:
			OnEat(character);
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public void OnEat(Character character)
	{
		if (_food == null || !character.InventoryContains(_food))
		{
			return;
		}
		if (_food.GetLiquidCapacity() > 0f)
		{
			character.ConsumeLiquid(_food, playSound: false, fromInfoScreen: false);
			Success = true;
			return;
		}
		Equipment equipment = character.Inventory.Take(character, _food, 1);
		if (equipment != null)
		{
			NotificationManager.Instance.AddEquipmentNotification(character, character, _food, 1);
			character.Eat(equipment, playSound: false, fromInfoScreen: false, null);
			equipment.Delete();
			Success = true;
		}
	}
}
