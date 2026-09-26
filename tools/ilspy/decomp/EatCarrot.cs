public class EatCarrot : AnimationGoal
{
	public EatCarrot()
	{
	}

	public EatCarrot(ActionAnim anim)
		: base(anim)
	{
	}

	public EatCarrot(Character character, TileObject targetObj, ActionAnim anim)
		: base(character, targetObj, anim)
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.EatCarrot;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		Tame(character);
	}

	private void Tame(Character character)
	{
		foreach (Target target in character.Targets)
		{
			if (target.Object is Character character2 && target.FullyTracked && (character2.PosXZ - character.PosXZ).magnitude <= RabbitFleeGoal.IgnoreThreatIfCloseToCarrotDist)
			{
				target.SetFlag(TargetFlags.TamedBy, on: true);
			}
		}
	}

	public override bool OnAnimationEvent(Character character, Goal parent, AnimEvent animEvent)
	{
		if (animEvent.EventType == AnimationEventType.FinishEat && !IsTargetDeleted() && !(Target.Object is Human))
		{
			float num = 0.01f;
			if (GetTargetObject() is FoodProp { ThrownBy: not null } foodProp && foodProp.GetGrabbableEquipmentType() != null && foodProp.GetGrabbableEquipmentType().ContainsHumanMeat)
			{
				Memory.OnMemorableEvent(MemoryPrototype.FedHumanMeatTo, foodProp.ThrownBy, character, num, secret: false);
			}
			character.SetHunger(character.GetHunger() - Sun.DayLengthSecs * num);
			Target.Object.Consume(character, num, InfectionType.None);
			if (Target.Object.GetConsumedAmount() >= 1f && Target.Object.DeleteWhenSkinned())
			{
				Target.Object.DeleteOrDisappear(fromGoal: false);
			}
		}
		return base.OnAnimationEvent(character, parent, animEvent);
	}
}
