using System;

public class Wait : Goal
{
	private TimeSpan StartTime;

	public TimeSpan WaitTime;

	private bool DisableSleep;

	public Wait()
	{
	}

	public Wait(TimeSpan waitTime)
	{
		WaitTime = waitTime;
	}

	public Wait(TimeSpan waitTime, bool disableSleep)
	{
		WaitTime = waitTime;
		DisableSleep = disableSleep;
	}

	public override GoalType GetGoalType()
	{
		return GoalType.Wait;
	}

	public override bool WantDisableSleep()
	{
		return DisableSleep;
	}

	public override void Reflect(Reflector reflector, Character character)
	{
		base.Reflect(reflector, character);
		reflector.Add(ref StartTime);
		reflector.Add(ref WaitTime);
		reflector.AddAfter(ref DisableSleep, 391);
	}

	public override void OnActivate(Character character, Goal parent)
	{
		base.OnActivate(character, parent);
		StartTime = PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted());
	}

	public override void Update(Character character, Goal parent)
	{
		base.Update(character, parent);
		if (PredictedObjectManager.Instance.GetCurrentTime(character.IsPredicted()) - StartTime >= WaitTime)
		{
			Finished = true;
		}
	}
}
