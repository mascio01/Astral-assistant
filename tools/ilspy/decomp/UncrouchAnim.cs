public class UncrouchAnim : FaceTarget
{
	public override GoalType GetGoalType()
	{
		return GoalType.UncrouchAnim;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.CrouchingTransition <= 0f)
		{
			Finished = true;
		}
		else if (character.IsCrouching())
		{
			Finished = true;
		}
	}
}
