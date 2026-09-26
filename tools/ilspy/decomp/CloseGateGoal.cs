public class CloseGateGoal : AnimationGoal
{
	public CloseGateGoal()
		: base(ActionAnim.CloseGate)
	{
	}

	public CloseGateGoal(Character character, Gate gate)
		: base(ActionAnim.CloseGate)
	{
		SetTarget(character, null, character.GetOrCreateTarget(gate));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.CloseGateGoal;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.CloseGate && character.IsAuthoritative() && GetTargetObject() is Gate gate)
		{
			gate.CloseOrLock();
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
