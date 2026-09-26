public class MedicateSelfAnim : StateMachineGoal
{
	public Equipment _syringe;

	public Equipment PreviouslyEquipped;

	public bool Success;

	public MedicateSelfAnim()
	{
	}

	public MedicateSelfAnim(Equipment syringe)
	{
		_syringe = syringe;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref _syringe);
		reflector.AddAfter(ref PreviouslyEquipped, 39);
		reflector.AddAfter(ref Success, 39);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.MedicateSelfAnim;
	}

	public override bool IsUsingEquippedItem()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetCrouching(character, parent, Crouching || character.WasCrouching);
		if (character.EquippedItem != _syringe)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(_syringe));
		}
		else
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.AdministerInjectionToSelf));
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.EquippedItem = null;
		if (character.DirectControlled && PreviouslyEquipped != null && PreviouslyEquipped != character.EquippedItem && character.InventoryContains(PreviouslyEquipped))
		{
			character.DesiredEquippedItem = PreviouslyEquipped;
		}
		PreviouslyEquipped = null;
		base.OnDeactivate(character, parent);
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip equip && equip.Equipment == _syringe && !Success)
		{
			return new AnimationGoal(ActionAnim.AdministerInjectionToSelf);
		}
		if (SubGoal is AnimationGoal && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			return new Equip(PreviouslyEquipped);
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (SubGoal is AnimationGoal)
		{
			switch (animEvent.EventType)
			{
			case AnimationEventType.AdministerInjectionToSelf:
			{
				if (_syringe != null && _syringe.GetAntigenType() != InfectionType.None && character.HasInjuryWithInfectionType(_syringe.GetAntigenType()) && character.Inventory.UseItemOfType(character, character, _syringe.GetPrototype(), 1, null, out var infectionType) > 0)
				{
					character.InjectAntigen(character, _syringe.GetAntigenType(), infectionType, _syringe, Goal.WantPrediction(parent.GetGoalType()));
					Success = true;
				}
				return true;
			}
			case AnimationEventType.Unequip:
				character.EquippedItem = null;
				return true;
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
