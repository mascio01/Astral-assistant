public class PourAway : StateMachineGoal
{
	public Equipment Item;

	public override GoalType GetGoalType()
	{
		return GoalType.PourAway;
	}

	public PourAway()
	{
	}

	public PourAway(Equipment item)
	{
		Item = item;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Item);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		SetSubGoal(character, parent, new Equip(Item));
	}

	protected override Goal GetNextSubGoal(Character character, Goal parent)
	{
		if (character.EquippedItem == Item && character.EquippedItem.GetLiquidContentsAmount() > 0f)
		{
			return new WaterPlantAnim();
		}
		return base.GetNextSubGoal(character, parent);
	}
}
