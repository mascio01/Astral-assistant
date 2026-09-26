public class UnconsciousGoal : Goal
{
	public override GoalType GetGoalType()
	{
		return GoalType.UnconsciousGoal;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (character.Consciousness != Consciousness.Unconscious)
		{
			return character.CarriedBy != null;
		}
		return true;
	}

	public override GoalPriority CalcPriority(Character character, Goal parent)
	{
		return GoalPriority.Unconscious;
	}

	public override bool IsBored(Character character)
	{
		return true;
	}
}
