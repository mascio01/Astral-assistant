using System;

public struct ZoneTrigger : IReflectable
{
	public string TriggerName;

	public Trigger Trigger;

	public TimeSpan LastTriggeredTime;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref TriggerName);
		reflector.Add(ref LastTriggeredTime);
	}

	public bool CanTrigger()
	{
		if (Trigger == null)
		{
			return false;
		}
		if (Trigger.OnceOnly && LastTriggeredTime != TimeSpan.Zero)
		{
			return false;
		}
		if ((Session.Instance.PlayTime - LastTriggeredTime).TotalSeconds < (double)Trigger.TriggerRepeatTime)
		{
			return false;
		}
		return true;
	}
}
