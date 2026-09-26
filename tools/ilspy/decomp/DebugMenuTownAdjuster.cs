using System;
using System.Text;

public class DebugMenuTownAdjuster : DebugMenuItemAdjuster
{
	private Prop Obj;

	public DebugMenuTownAdjuster(string name, Prop obj)
		: base(name)
	{
		Obj = obj;
		DecrementEvent = OnDecrement;
		IncrementEvent = OnIncrement;
		BuildValueStringEvent = BuildTownDisplayString;
		BuildDisplayString();
	}

	public void OnDecrement()
	{
		if (Obj != null)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			if (communityManager.Towns.Count != 0)
			{
				int num = communityManager.Towns.IndexOf(Obj.Town);
				Obj.Town = ((num > 0) ? communityManager.Towns[num - 1] : null);
				BuildDisplayString();
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	public void OnIncrement()
	{
		if (Obj != null)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			if (communityManager.Towns.Count != 0)
			{
				int num = communityManager.Towns.IndexOf(Obj.Town);
				Obj.Town = communityManager.Towns[Math.Min(communityManager.Towns.Count - 1, num + 1)];
				BuildDisplayString();
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	public void BuildTownDisplayString(ref StringBuilder value)
	{
		value.Length = 0;
		value.Append(Name);
		value.Append(" < ");
		if (Obj != null && Obj.Town != null)
		{
			Obj.Town.BuildDisplayName(value, noStrangers: true, englishOnly: false);
		}
		value.Append(" >");
	}
}
