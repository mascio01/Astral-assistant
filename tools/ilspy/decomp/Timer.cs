using System;

public struct Timer : IReflectable
{
	public string Name;

	public TimeSpan StartTime;

	public TimeSpan FinishTime;

	public static Timer Create(string name, TimeSpan time)
	{
		Timer result = default(Timer);
		result.Name = name;
		result.StartTime = Session.Instance.PlayTime;
		result.FinishTime = result.StartTime + time;
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Name);
		reflector.Add(ref FinishTime);
		if (reflector.Version < 537)
		{
			StartTime = MathUtil.Min(FinishTime, Session.Instance.PlayTime);
		}
		else
		{
			reflector.Add(ref StartTime);
		}
	}
}
