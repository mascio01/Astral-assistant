public struct QueuedTask
{
	public TaskFunc Func;

	public TaskCompletedFunc CompletedFunc;

	public BaseTaskData Data;

	public TaskPriority Priority;

	public QueuedTask(TaskFunc func, TaskCompletedFunc completedFunc, BaseTaskData data, TaskPriority priority)
	{
		Func = func;
		CompletedFunc = completedFunc;
		Data = data;
		Priority = priority;
	}
}
