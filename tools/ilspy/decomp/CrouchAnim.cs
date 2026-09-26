public class CrouchAnim : FaceTarget
{
	public CrouchAnim()
	{
		Crouching = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.CrouchAnim;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (character.CrouchingTransition >= 1f)
		{
			Finished = true;
		}
	}
}
