using System;

internal struct OneShotTimerMarker : IDisposable
{
	private OneShotTimer Timer;

	public OneShotTimerMarker(OneShotTimer timer)
	{
		Timer = timer;
		Timer.Start();
	}

	public void Dispose()
	{
		Timer.Stop();
	}
}
