using System;

public struct FailedToBuryRecord : IReflectable
{
	public Character Corpse;

	public TimeSpan FailedTime;

	public int Attempts;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Corpse);
		reflector.Add(ref FailedTime);
		reflector.Add(ref Attempts);
	}
}
