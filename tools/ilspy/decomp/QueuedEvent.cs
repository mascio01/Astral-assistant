using System;

public struct QueuedEvent : IReflectable
{
	public StoryEvent Event;

	public Character Actor;

	public Character Target;

	public BaseObject Obj;

	public MemoryParam Param;

	public TimeSpan QueuedTime;

	public static QueuedEvent Create(StoryEvent storyEvent, Character actor, Character target, BaseObject obj, MemoryParam param, TimeSpan time)
	{
		return new QueuedEvent
		{
			Event = storyEvent,
			Actor = actor,
			Target = target,
			Obj = obj,
			Param = param,
			QueuedTime = time
		};
	}

	public void Reflect(Reflector reflector)
	{
		Event.Reflect(reflector);
		reflector.Add(ref Actor);
		reflector.Add(ref Target);
		reflector.Add(ref Obj);
		if (reflector.Version >= 246)
		{
			Param.Reflect(reflector);
		}
		reflector.Add(ref QueuedTime);
	}
}
