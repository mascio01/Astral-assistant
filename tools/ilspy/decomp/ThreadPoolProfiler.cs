using System;
using System.Text;
using UnityEngine;

public class ThreadPoolProfiler : DebugMenu
{
	private StringBuilder _sb = new StringBuilder(2000);

	public ThreadPoolProfiler()
		: base("Thread Pool Profiler")
	{
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		_sb.Length = 0;
		int num = GameImpl.Instance.UpdateThreadPool.PrintRequestQueue(_sb);
		GUI.Box(new Rect(10f, 10f, 1640f, (float)Math.Min(num, 20) * 40f + 40f), TitleString);
		Vector2 vector = new Vector2(30f, 40f);
		bool num2 = Items.Count > 20;
		if (num2)
		{
			ScrollPosition = GUIBeginScrollView(new Rect(vector.x, vector.y, 1600f, 800f), ScrollPosition, new Rect(0f, 0f, 1600f, (float)num * 40f), alwaysShowHorizontal: false, alwaysShowVertical: true);
			vector = Vector2.zero;
		}
		GUI.Label(new Rect(30f, 30f, 1600f, (float)num * 40f + 40f), _sb.ToString());
		if (num2)
		{
			GUI.EndScrollView();
		}
	}
}
