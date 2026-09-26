using System;
using System.Collections.Generic;

public struct SaveGameData
{
	public string StoryFolder;

	public ulong StoryWorkshopId;

	public List<StoryId> Stories;

	public DateTime Timestamp;

	public long GameTimeTicks;

	public int CommunitySize;

	public string CommunityName;

	public string CharacterName;

	public int Day;

	public int DayOfYear;

	public List<string> PlayerNames;
}
