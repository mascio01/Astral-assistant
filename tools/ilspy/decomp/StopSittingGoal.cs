public class StopSittingGoal : AnimationGoal
{
	public StopSittingGoal()
		: base(ActionAnim.StopSittingByFire)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.StopSittingGoal;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.StopSitting)
		{
			character.ClearSitting();
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (AnimState == SpeechAnimState.NotStarted && !character.Sitting)
		{
			Finished = true;
		}
	}
}
