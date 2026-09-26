public class WatchTarget : Goal
{
	public WatchTarget()
	{
	}

	public WatchTarget(bool aiming)
	{
		Aiming = aiming;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.WatchTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.GoalTarget = Target;
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		if (Target.TimeSinceLastVisible.TotalSeconds > 1.0)
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}
}
