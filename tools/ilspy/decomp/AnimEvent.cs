using System;

public class AnimEvent : IComparable<AnimEvent>
{
	public AnimationEventType EventType;

	public float NormalizedTime;

	public TimeSpan Time;

	public AnimEvent(AnimationEventType eventType, float normalizedTime)
	{
		EventType = eventType;
		NormalizedTime = normalizedTime;
	}

	public int CompareTo(AnimEvent other)
	{
		if (Time < other.Time)
		{
			return -1;
		}
		if (Time > other.Time)
		{
			return 1;
		}
		return 0;
	}
}
