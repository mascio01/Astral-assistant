using System;

public struct PredictedEvent
{
	public TimeSpan Time;

	public int ActionCounter;

	public PredictedEventType EventType;

	public int ExtraData;

	public PredictedEvent(TimeSpan time, int actionCounter, PredictedEventType type, int extraData)
	{
		Time = time;
		ActionCounter = actionCounter;
		EventType = type;
		ExtraData = extraData;
	}
}
