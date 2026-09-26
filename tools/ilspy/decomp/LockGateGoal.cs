public class LockGateGoal : AnimationGoal
{
	public LockGateGoal()
		: base(ActionAnim.LockGate)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LockGateGoal;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.LockGate && character.IsAuthoritative() && GetTargetObject() is Gate gate)
		{
			gate.Lock();
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
