using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

public class Processor
{
	private MyThreadPool _pool;

	private Thread _thread;

	public bool Working;

	private static string TaskStr = "Task";

	public Processor(MyThreadPool pool)
	{
		_pool = pool;
		_thread = new Thread(ThreadFunc);
		_thread.IsBackground = true;
		_thread.Start();
	}

	public void ThreadFunc()
	{
		try
		{
			Util.SetFloatingPointControl();
			Util.CheckFloatingPointControl();
			while (true)
			{
				QueuedTask? task;
				ThreadAction nextAction = _pool.GetNextAction(out task, out Working);
				Util.SetFloatingPointControl();
				Util.CheckFloatingPointControl();
				switch (nextAction)
				{
				case ThreadAction.Quit:
					return;
				case ThreadAction.PerformTask:
					using (new UnityProfileMarker(TaskStr))
					{
						task.Value.Func(task.Value.Data);
						if (task.Value.CompletedFunc != null)
						{
							_pool.AddCompletedTask(task.Value);
						}
					}
					break;
				}
				Util.CheckFloatingPointControl();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message + ex.StackTrace);
		}
		Profiler.EndThreadProfiling();
	}

	public void WaitForQuit()
	{
		_thread.Join();
	}
}
