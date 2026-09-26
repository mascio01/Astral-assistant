public class LightFireWithMatch : AnimationGoal
{
	public LightFireWithMatch()
		: base(ActionAnim.LightFireWithMatch)
	{
		EnableFaceTarget = false;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LightFireWithMatch;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.LightFire)
		{
			if (parent is LightFireGoal lightFireGoal)
			{
				lightFireGoal.LightFire(character);
			}
			character.Inventory.UseItemOfType(character, character, EquipmentPrototype.Match, 1, null, out var _);
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
