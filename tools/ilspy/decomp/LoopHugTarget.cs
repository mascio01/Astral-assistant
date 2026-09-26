using System;

public class LoopHugTarget : AnimationGoal
{
	public TimeSpan StartTime;

	public TimeSpan WaitTime;

	public bool Success;

	public LoopHugTarget()
	{
	}

	public LoopHugTarget(TimeSpan timeout)
		: base(ActionAnim.HugLoop)
	{
		WaitTime = timeout;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref Success);
	}

	public override GoalType GetGoalType()
	{
		return GoalType.LoopHugTarget;
	}

	public override bool WantStopAnimOnExit()
	{
		return false;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
		base.OnActivate(character, parent);
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		Character targetCharacter = GetTargetCharacter();
		character.SetPosition(HugTarget.GetPosForHuggingAnimation(character, targetCharacter));
		if (!targetCharacter.IsAwake || targetCharacter.GetCurrentActionPriority() > ActionPriority.Hugged)
		{
			Finished = true;
		}
		if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime >= WaitTime)
		{
			Finished = true;
			Success = true;
		}
	}
}
