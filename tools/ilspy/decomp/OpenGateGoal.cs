public class OpenGateGoal : AnimationGoal
{
	public OpenGateGoal()
		: base(ActionAnim.OpenGate)
	{
	}

	public OpenGateGoal(Character character, Gate gate)
		: base(ActionAnim.OpenGate)
	{
		SetTarget(character, null, character.GetOrCreateTarget(gate));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.OpenGateGoal;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.OpenGate && character.IsAuthoritative() && GetTargetObject() is Gate gate)
		{
			gate.Open(character);
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
