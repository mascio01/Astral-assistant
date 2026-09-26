public class ZombieStumble : AnimationGoal
{
	public ZombieStumble()
		: base(ActionAnim.ZombieBiteStumble)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.ZombieStumble;
	}
}
