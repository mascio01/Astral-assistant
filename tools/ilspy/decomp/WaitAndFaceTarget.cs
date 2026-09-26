using System;

public class WaitAndFaceTarget : Goal
{
	public TimeSpan StartTime;

	public TimeSpan WaitTime;

	public WaitAndFaceTarget()
	{
	}

	public WaitAndFaceTarget(TimeSpan waitTime)
	{
		WaitTime = waitTime;
	}

	public WaitAndFaceTarget(TimeSpan waitTime, bool aiming)
	{
		WaitTime = waitTime;
		Aiming = aiming;
	}

	public WaitAndFaceTarget(TimeSpan waitTime, bool aiming, bool crouching)
	{
		WaitTime = waitTime;
		Aiming = aiming;
		Crouching = crouching;
	}

	public WaitAndFaceTarget(Character character, TileObject targetObj, TimeSpan waitTime)
	{
		WaitTime = waitTime;
		SetTarget(character, null, character.GetOrCreateTarget(targetObj));
		HasUserTarget = true;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.WaitAndFaceTarget;
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		character.GoalTarget = Target;
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
	}

	public override void OnDeactivate(Character character, Goal parent)
	{
		character.GoalTarget = null;
		base.OnDeactivate(character, parent);
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartTime);
		reflector.Add(ref WaitTime);
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
		if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime >= WaitTime)
		{
			OnTimeout();
		}
	}

	public virtual void OnTimeout()
	{
		Finished = true;
	}
}
