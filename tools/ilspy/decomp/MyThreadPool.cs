using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class MyThreadPool
{
	private List<QueuedTask> TaskQueue = new List<QueuedTask>();

	private List<QueuedTask> CompletedTaskQueue = new List<QueuedTask>();

	private AutoResetEvent ActionEvent = new AutoResetEvent(initialState: false);

	private Processor[] Processors;

	private bool _wantQuit;

	public MyThreadPool()
	{
		int num = Math.Max(3, Environment.ProcessorCount - 1);
		Processors = new Processor[num];
		for (int i = 0; i < Processors.Length; i++)
		{
			Processors[i] = new Processor(this);
		}
	}

	public void Unload()
	{
		lock (this)
		{
			_wantQuit = true;
			ActionEvent.Set();
		}
		Processor[] processors = Processors;
		for (int i = 0; i < processors.Length; i++)
		{
			processors[i].WaitForQuit();
		}
	}

	public int PrintRequestQueue(StringBuilder sb)
	{
		lock (this)
		{
			for (int i = 0; i < TaskQueue.Count; i++)
			{
				sb.Append(TaskQueue[i].Func.ToString() + ": " + TaskQueue[i].Priority);
				sb.Append('\n');
			}
			return TaskQueue.Count;
		}
	}

	public void AddTask(TaskFunc func, TaskCompletedFunc completedFunc, BaseTaskData data, TaskPriority priority)
	{
		lock (this)
		{
			TaskQueue.Add(new QueuedTask(func, completedFunc, data, priority));
			ActionEvent.Set();
		}
	}

	public void AddCompletedTask(QueuedTask task)
	{
		lock (this)
		{
			CompletedTaskQueue.Add(task);
		}
	}

	private QueuedTask? PopCompletedTask()
	{
		lock (this)
		{
			if (CompletedTaskQueue.Count > 0)
			{
				QueuedTask value = CompletedTaskQueue[0];
				CompletedTaskQueue.RemoveAt(0);
				return value;
			}
			return null;
		}
	}

	public void Update()
	{
		QueuedTask? queuedTask = PopCompletedTask();
		while (queuedTask.HasValue)
		{
			queuedTask.Value.CompletedFunc(queuedTask.Value.Data);
			queuedTask = PopCompletedTask();
		}
	}

	public void BlockWhileBusy()
	{
		while (IsBusy())
		{
			Thread.Sleep(1);
		}
	}

	public bool IsBusy()
	{
		for (int i = 0; i < Processors.Length; i++)
		{
			if (Processors[i].Working)
			{
				return true;
			}
		}
		lock (this)
		{
			return TaskQueue.Count > 0;
		}
	}

	public ThreadAction GetNextAction(out QueuedTask? task, out bool working)
	{
		working = false;
		ActionEvent.WaitOne();
		lock (this)
		{
			if (TaskQueue.Count > 0)
			{
				int index = 0;
				for (int i = 1; i < TaskQueue.Count; i++)
				{
					if (TaskQueue[i].Priority > TaskQueue[index].Priority)
					{
						index = i;
					}
				}
				task = TaskQueue[index];
				TaskQueue.RemoveAt(index);
				working = true;
				if (TaskQueue.Count > 0)
				{
					ActionEvent.Set();
				}
				return ThreadAction.PerformTask;
			}
			if (_wantQuit)
			{
				task = null;
				ActionEvent.Set();
				return ThreadAction.Quit;
			}
			task = null;
			return ThreadAction.None;
		}
	}
}
