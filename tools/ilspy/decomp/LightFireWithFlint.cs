public class LightFireWithFlint : AnimationGoal
{
	public LightFireWithFlint()
		: base(ActionAnim.LightFireWithFlint)
	{
		EnableFaceTarget = false;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LightFireWithFlint;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.EquipFlint:
			character.LeftHandEquippedItem = character.Inventory.FindItemOfType(EquipmentPrototype.Flint);
			break;
		case AnimationEventType.LightFire:
			if (parent is LightFireGoal lightFireGoal)
			{
				lightFireGoal.LightFire(character);
			}
			break;
		case AnimationEventType.UnequipFlint:
			character.LeftHandEquippedItem = null;
			break;
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.LeftHandEquippedItem = null;
		base.OnDeactivate(character, parent);
	}
}
