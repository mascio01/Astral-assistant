using System;

public struct ExitBuildingAttempt : IReflectable
{
	public int BuildingId;

	public int ExitIndex;

	public int FailCount;

	public TimeSpan FailTime;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref BuildingId);
		reflector.Add(ref ExitIndex);
		reflector.Add(ref FailCount);
		reflector.Add(ref FailTime);
	}

	public bool Matches(Building building, int exitIndex)
	{
		if (BuildingId == building.Id)
		{
			return ExitIndex == exitIndex;
		}
		return false;
	}
}
