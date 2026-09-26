using System.Diagnostics;
using System.Text;
using UnityEngine;

internal class OneShotTimer
{
	private Stopwatch Stopwatch = new Stopwatch();

	private StringBuilder OutputString = new StringBuilder(100);

	private string Name;

	public OneShotTimer(string name)
	{
		Name = name;
	}

	public void Start()
	{
		OutputString.Length = 0;
		Stopwatch.Reset();
		Stopwatch.Start();
	}

	public void Stop()
	{
		Stopwatch.Stop();
		OutputString.Append(Name);
		OutputString.AppendWithoutGarbage((double)(Stopwatch.ElapsedTicks * 1000) / (double)Stopwatch.Frequency, 5);
		OutputString.Append(GameProfiler.Ms);
		UnityEngine.Debug.Log(OutputString);
	}
}
