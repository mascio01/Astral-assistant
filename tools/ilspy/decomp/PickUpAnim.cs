public class PickUpAnim : AnimationGoal
{
	public PickUpAnim()
		: base(ActionAnim.PickUp)
	{
	}

	public PickUpAnim(Character character, TileObject prop)
		: base(ActionAnim.PickUp)
	{
		SetTarget(character, null, character.GetOrCreateTarget(prop));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.PickUpAnim;
	}

	public override bool WantFaceTarget(Character character)
	{
		return false;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.PickUp)
		{
			TileObject targetObject = GetTargetObject();
			if (targetObject != null)
			{
				character.PickUp(targetObject);
			}
			return true;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
