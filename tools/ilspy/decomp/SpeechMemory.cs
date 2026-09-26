using System;

public struct SpeechMemory : IReflectable
{
	public Speech Speech;

	public TimeSpan SpokenTime;

	public int TargetId;

	public static SpeechMemory Create(Speech speech, Character target)
	{
		return new SpeechMemory
		{
			Speech = speech,
			SpokenTime = Session.Instance.PlayTime,
			TargetId = (target?.Id ?? 0)
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Speech);
		reflector.Add(ref SpokenTime);
		reflector.Add(ref TargetId);
	}
}
