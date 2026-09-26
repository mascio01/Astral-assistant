using System;
using System.Diagnostics;

internal struct StopWatchScope : IDisposable
{
	private Stopwatch Stopwatch;

	public StopWatchScope(Stopwatch stopWatch)
	{
		Stopwatch = stopWatch;
		Stopwatch.Start();
	}

	public void Dispose()
	{
		Stopwatch.Stop();
	}
}
