using System.Diagnostics;
using System.Text;

public class GameProfiler
{
	public string Name;

	private double AverageTimePerCall;

	private double AverageTimePerFrame;

	private double AverageCallsPerFrame;

	private double ElapsedTime;

	private uint Calls;

	public const int NumLoggedFrames = 120;

	public double[] LoggedTime = new double[120];

	public uint[] LoggedCalls = new uint[120];

	private uint CallsThisFrame;

	private Stopwatch Stopwatch = new Stopwatch();

	public static string Comma = ", ";

	public static string Ms = "ms";

	public static string Newline = "\n";

	private StringBuilder Tmp = new StringBuilder(50);

	public GameProfiler(string name)
	{
		GameProfilerFolder gameProfilerFolder = GameProfilerFolder.Root;
		while (true)
		{
			int num = name.IndexOf('.');
			if (num == -1)
			{
				break;
			}
			gameProfilerFolder = gameProfilerFolder.GetOrCreateSubFolder(name.Substring(0, num));
			name = name.Substring(num + 1);
		}
		gameProfilerFolder.GameProfilers.Add(this);
		Name = name;
	}

	public void Start()
	{
		Stopwatch.Start();
	}

	public void Stop()
	{
		Stopwatch.Stop();
		CallsThisFrame++;
	}

	public void Log(int sessionFrame, bool wantLog)
	{
		double totalMilliseconds = Stopwatch.Elapsed.TotalMilliseconds;
		if (wantLog)
		{
			int num = sessionFrame % 120;
			LoggedTime[num] = totalMilliseconds;
			LoggedCalls[num] = CallsThisFrame;
		}
		ElapsedTime += totalMilliseconds;
		Calls += CallsThisFrame;
		Stopwatch.Reset();
		CallsThisFrame = 0u;
	}

	public void Reset(int frames)
	{
		AverageTimePerCall = ((Calls == 0) ? 0.0 : (ElapsedTime / (double)Calls));
		AverageTimePerFrame = ((frames == 0) ? 0.0 : (ElapsedTime / (double)frames));
		AverageCallsPerFrame = (double)Calls / (double)frames;
		ElapsedTime = 0.0;
		Calls = 0u;
	}

	public void Print(StringBuilder sb, int loggedFrame)
	{
		if (loggedFrame == -1)
		{
			Tmp.Length = 0;
			Tmp.AppendWithoutGarbage(AverageTimePerCall, 4);
			sb.AppendWithoutGarbage(Tmp);
			sb.Append(Ms);
			sb.Append(Comma);
			Tmp.Length = 0;
			Tmp.AppendWithoutGarbage(AverageTimePerFrame, 4);
			sb.AppendWithoutGarbage(Tmp);
			sb.Append(Ms);
			sb.Append(Comma);
			Tmp.Length = 0;
			Tmp.AppendWithoutGarbage(AverageCallsPerFrame, 2);
			sb.AppendWithoutGarbage(Tmp);
		}
		else
		{
			Tmp.Length = 0;
			Tmp.AppendWithoutGarbage(LoggedTime[loggedFrame], 4);
			sb.AppendWithoutGarbage(Tmp);
			sb.Append(Ms);
			sb.Append(Comma);
			Tmp.Length = 0;
			Tmp.AppendWithoutGarbage(LoggedCalls[loggedFrame]);
			sb.AppendWithoutGarbage(Tmp);
		}
		sb.Append(Newline);
	}
}
