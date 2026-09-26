using System;

public class DrinkFromRiver : AnimationGoal
{
	public DrinkFromRiver()
		: base(ActionAnim.DrinkFromRiver)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.DrinkFromRiver;
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		switch (animEvent.EventType)
		{
		case AnimationEventType.StartDrink:
			return true;
		case AnimationEventType.FinishDrink:
			character.ConsumeLiquid(LiquidPrototype.Water, Math.Min(character.GetThirstInFlOz(), 50f), playSound: false, fromInfoScreen: false, InfectionType.None);
			return true;
		default:
			return base.OnAnimationEvent(character, parent, animEvent);
		}
	}
}
