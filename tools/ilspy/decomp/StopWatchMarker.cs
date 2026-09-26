using System;
using System.Diagnostics;
using UnityEngine;

internal struct StopWatchMarker : IDisposable
{
	private static bool AllLogTimes;

	private string Name;

	private Stopwatch Stopwatch;

	private bool LogTimes;

	public StopWatchMarker(string name, bool logTimes = false)
	{
		Name = name;
		LogTimes = logTimes;
		Stopwatch = new Stopwatch();
		Stopwatch.Start();
	}

	public void Dispose()
	{
		Stopwatch.Stop();
		if (AllLogTimes || LogTimes)
		{
			UnityEngine.Debug.Log(Name + ": " + Stopwatch.Elapsed.TotalMilliseconds + "ms");
		}
	}
}
