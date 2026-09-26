using System;

internal struct ProfileMarker : IDisposable
{
	private GameProfiler GameProfiler;

	public ProfileMarker(GameProfiler gameProfiler)
	{
		GameProfiler = gameProfiler;
		GameProfiler.Start();
	}

	public void Dispose()
	{
		GameProfiler.Stop();
	}
}
