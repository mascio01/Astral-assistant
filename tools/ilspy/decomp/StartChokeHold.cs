public class StartChokeHold : AnimationGoal
{
	public StartChokeHold()
	{
	}

	public StartChokeHold(HoldType holdType)
		: base(holdType switch
		{
			HoldType.SlitThroat => ActionAnim.SlitThroatStart, 
			HoldType.Restrain => ActionAnim.RestrainStart, 
			_ => ActionAnim.ChokeHoldStart, 
		})
	{
	}

	public override GoalType GetGoalType()
	{
		return GoalType.StartChokeHold;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.ChokePower = 0.5f;
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (AnimState >= SpeechAnimState.Started)
		{
			character.SetPosition(LoopChokeHold.GetPosForChokeHoldAnimation(character, GetTargetCharacter(), PosWhenAnimStarted));
		}
	}
}
