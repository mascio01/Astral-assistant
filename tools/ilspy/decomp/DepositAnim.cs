public class DepositAnim : AnimationGoal
{
	public DepositAnim()
		: base(ActionAnim.Scavenge)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.DepositAnim;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		Anim = GetTargetObject()?.GetTakeAnim() ?? ActionAnim.Scavenge;
		base.OnActivate(character, parent);
	}
}
