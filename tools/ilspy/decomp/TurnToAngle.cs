public class TurnToAngle : StateMachineGoal
{
	private float Angle;

	public TurnToAngle()
	{
	}

	public TurnToAngle(float angle)
	{
		Angle = angle;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.TurnToAngle;
	}

	protected override bool FinishOnNullSubGoal()
	{
		return true;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Angle);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		if (character.IsSitting())
		{
			SetSubGoal(character, parent, new StopSittingGoal());
		}
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (SubGoal == null)
		{
			character.DesiredFacingAngle = Angle;
			if (MathUtil.AngleDiff(character.DesiredFacingAngle, character.FacingAngle) < 0.001f)
			{
				Finished = true;
			}
		}
	}
}
