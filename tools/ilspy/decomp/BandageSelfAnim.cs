public class BandageSelfAnim : StateMachineGoal
{
	public Equipment Bandage;

	public Equipment PreviouslyEquipped;

	public bool Success;

	public BandageSelfAnim()
	{
	}

	public BandageSelfAnim(Equipment bandage)
	{
		Bandage = bandage;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.BandageSelfAnim;
	}

	public override bool IsUsingEquippedItem()
	{
		return true;
	}

	public override bool CanShowDialogOptions(Character character)
	{
		return false;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.AddAfter(ref Bandage, 8);
		reflector.AddAfter(ref PreviouslyEquipped, 39);
		reflector.AddAfter(ref Success, 39);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetCrouching(character, parent, Crouching || character.WasCrouching);
		if (character.EquippedItem != Bandage)
		{
			PreviouslyEquipped = character.EquippedItem;
			SetSubGoal(character, parent, new Equip(Bandage));
		}
		else
		{
			SetSubGoal(character, parent, new AnimationGoal(ActionAnim.ApplyBandageToSelf));
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
		if (SubGoal is Equip equip && equip.Equipment == Bandage && !Success)
		{
			return new AnimationGoal(ActionAnim.ApplyBandageToSelf);
		}
		int skillLevel = (character.HasRunningRole(Role.Medic) ? character.GetSkillLevelWithEffects(SkillType.Medicine) : 0);
		if (SubGoal is AnimationGoal && character.EquippedItem != null && character.EquippedItem == Bandage && character.HasUnbandagedInjury(skillLevel) && character.CanBreakOutOfDirectControlForMinorAI(Active, Role.None))
		{
			return new AnimationGoal(ActionAnim.ApplyBandageToSelf);
		}
		if (SubGoal is AnimationGoal && PreviouslyEquipped != null && character.InventoryContains(PreviouslyEquipped))
		{
			return new Equip(PreviouslyEquipped);
		}
		return base.GetNextSubGoal(character, parent);
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishApplyBandageToSelf)
		{
			if (character.HasInfiniteBandages(Bandage, character) || character.Inventory.UseItem(character, Bandage, 1) > 0)
			{
				character.ApplyBandage(character, Bandage, playSound: false, Goal.WantPrediction(parent.GetGoalType()));
				Success = true;
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
