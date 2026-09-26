public class MakeSureWeHaveReloadedGoal : StateMachineGoal
{
	public override GoalType GetGoalType()
	{
		return GoalType.MakeSureWeHaveReloadedGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.DirectControlledMajorAIDisabled)
		{
			return false;
		}
		if (Active)
		{
			return true;
		}
		if (character.CarryingObject != null)
		{
			return false;
		}
		if ((character.EquippedItem != character.DesiredEquippedItem || (character.EquippedItem != null && character.EquippedItem.CanBeReloaded(character, checkIfAllowedToUseAmmo: true))) && character.CurrentActionAnim == ActionAnim.None)
		{
			return true;
		}
		return false;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Survivor_MakeSureWeHaveReloaded;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Crouching = character.DirectControlledCrouching;
		base.OnActivate(character, parent);
		if (character.EquippedItem != character.DesiredEquippedItem)
		{
			Equip equip = new Equip(character.DesiredEquippedItem);
			equip.IsSettingDesiredEquipment = true;
			SetSubGoal(character, parent, equip);
		}
		else
		{
			SetSubGoal(character, parent, new ReloadAnim(aiming: false));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is Equip && character.EquippedItem != null && character.EquippedItem.CanBeReloaded(character, checkIfAllowedToUseAmmo: true))
		{
			return new ReloadAnim(aiming: false);
		}
		return base.GetNextSubGoal(character, parent);
	}
}
