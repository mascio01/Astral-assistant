public class WaitForTargetToInteractWithMe : FaceTarget
{
	public override GoalType GetGoalType()
	{
		return GoalType.WaitForTargetToInteractWithMe;
	}

	public override bool IsPossible(Character character, Goal parent)
	{
		return GetTargetCharacter().FindActiveGoal(GoalType.MoveToAndInteractGoal) != null;
	}
}
