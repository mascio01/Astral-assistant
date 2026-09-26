public class Equip : StateMachineGoal
{
	public Equipment Equipment;

	public bool IsSettingDesiredEquipment;

	public override GoalType GetGoalType()
	{
		return GoalType.Equip;
	}

	public Equip()
	{
	}

	public Equip(Equipment equipment)
	{
		Equipment = equipment;
	}

	public Equip(Equipment equipment, bool crouching)
	{
		Equipment = equipment;
		Crouching = crouching;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Equipment);
		reflector.AddAfter(ref IsSettingDesiredEquipment, 172);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (Equipment != null)
		{
			return character.Inventory.Contains(Equipment);
		}
		return true;
	}

	public override bool WantDisableSleep()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.DirectControlled = false;
		if (character.EquippedItem == Equipment)
		{
			Finished = true;
		}
		else
		{
			SetSubGoal(character, parent, new UnequipAnim(IsSettingDesiredEquipment));
		}
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (SubGoal is UnequipAnim)
		{
			return new EquipAnim(Equipment, IsSettingDesiredEquipment);
		}
		return null;
	}
}
