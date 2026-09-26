public class TurnToTarget : StateMachineGoal
{
	private float Tolerance;

	public TurnToTarget()
	{
		Tolerance = 0.001f;
	}

	public TurnToTarget(float tolerance)
	{
		Tolerance = tolerance;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TurnToTarget;
	}

	protected override bool FinishOnNullSubGoal()
	{
		return true;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.IsSitting())
		{
			SetSubGoal(character, parent, new StopSittingGoal());
		}
		else
		{
			character.GoalTarget = Target;
		}
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Tolerance);
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		if (IsTargetDeleted())
		{
			return false;
		}
		return base.IsPossible(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal == null)
		{
			character.GoalTarget = Target;
			if (MathUtil.AngleDiff(character.DesiredFacingAngle, character.FacingAngle) < Tolerance)
			{
				Finished = true;
			}
		}
	}
}
