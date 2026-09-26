using System.Text;
using UnityEngine;

public class DebugMenuItemProfiler : DebugMenuItem
{
	private static StringBuilder sb = new StringBuilder(100);

	public GameProfiler Profiler;

	public int LoggedFrame;

	public DebugMenuItemProfiler(GameProfiler profiler, int loggedFrame)
		: base(profiler.Name)
	{
		Profiler = profiler;
		LoggedFrame = loggedFrame;
	}

	public override void Update(Vector2 pos)
	{
		sb.Length = 0;
		Profiler.Print(sb, LoggedFrame);
		GUI.Label(new Rect(pos.x, pos.y, 200f, 30f), Name);
		GUI.Label(new Rect(pos.x + 200f, pos.y, 1000f, 30f), sb.ToString());
	}
}
