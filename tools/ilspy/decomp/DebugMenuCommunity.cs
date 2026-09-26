using System;
using System.Text;

public class DebugMenuCommunity : DebugMenuItemAdjuster
{
	public static Community Community;

	public DebugMenuCommunity(string name)
		: base(name)
	{
		DecrementEvent = OnDecrement;
		IncrementEvent = OnIncrement;
		BuildValueStringEvent = BuildCommunityDisplayString;
		BuildDisplayString();
	}

	public void OnDecrement()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		int num = communityManager.Communities.IndexOf(Community);
		Community = ((num > 0) ? communityManager.Communities[num - 1] : null);
		BuildDisplayString();
	}

	public void OnIncrement()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		int num = communityManager.Communities.IndexOf(Community);
		Community = communityManager.Communities[Math.Min(communityManager.Communities.Count - 1, num + 1)];
		BuildDisplayString();
	}

	public void BuildCommunityDisplayString(ref StringBuilder value)
	{
		value.Length = 0;
		value.Append(Name);
		value.Append(" < ");
		if (Community != null)
		{
			Community.BuildDisplayName(value, noStrangers: true, englishOnly: false);
		}
		value.Append(" >");
	}
}
