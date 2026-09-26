public class SitGoal : AnimationGoal
{
	public SitGoal()
		: base(ActionAnim.SitByFire)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.SitGoal;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.StartSitting)
		{
			character.SetSitting(GetTargetObject()?.GetCentreTile() ?? TerrainCoord.Invalid);
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
