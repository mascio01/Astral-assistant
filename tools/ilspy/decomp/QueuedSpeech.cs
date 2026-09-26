using System;

public struct QueuedSpeech : IReflectable
{
	public Speech Speech;

	public Character Target;

	public BaseObject SpeechObject;

	public MemoryParam Param;

	public TimeSpan LastFailedAttemptTime;

	public static QueuedSpeech Create(Speech speech, Character target, BaseObject speechObject, MemoryParam param, TimeSpan lastFailedAttemptTime)
	{
		return new QueuedSpeech
		{
			Speech = speech,
			Target = target,
			SpeechObject = speechObject,
			Param = param,
			LastFailedAttemptTime = lastFailedAttemptTime
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Speech);
		reflector.Add(ref Target);
		reflector.AddAfter(ref SpeechObject, 24);
		if (reflector.Version >= 245)
		{
			Param.Reflect(reflector);
		}
		reflector.Add(ref LastFailedAttemptTime);
	}
}
