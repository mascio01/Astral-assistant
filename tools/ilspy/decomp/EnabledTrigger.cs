using System;

public struct EnabledTrigger : IReflectable
{
	public Trigger Trigger;

	public BaseObject QuestInstance;

	public Character Actor;

	public Character Target;

	public BaseObject Object;

	public TimeSpan LastTriggeredTime;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Trigger);
		reflector.AddAfter(ref QuestInstance, 290);
		reflector.Add(ref Actor);
		reflector.AddAfter(ref Target, 470);
		reflector.Add(ref Object);
		reflector.AddAfter(ref LastTriggeredTime, 292);
	}

	public bool CanTrigger()
	{
		if (LastTriggeredTime != TimeSpan.Zero)
		{
			if (Trigger.OnceOnly)
			{
				return false;
			}
			if ((Session.Instance.PlayTime - LastTriggeredTime).TotalSeconds < (double)Trigger.TriggerRepeatTime)
			{
				return false;
			}
		}
		return true;
	}
}
