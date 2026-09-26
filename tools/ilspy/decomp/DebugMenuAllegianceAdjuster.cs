using System;
using System.Text;

public class DebugMenuAllegianceAdjuster : DebugMenuItemAdjuster
{
	private TileObject Obj;

	private bool AdjustInitialCommunity;

	public DebugMenuAllegianceAdjuster(string name, TileObject obj, bool initialCommunity = false)
		: base(name)
	{
		Obj = obj;
		AdjustInitialCommunity = initialCommunity;
		DecrementEvent = OnDecrement;
		IncrementEvent = OnIncrement;
		BuildValueStringEvent = BuildCommunityDisplayString;
		BuildDisplayString();
	}

	public void OnDecrement()
	{
		if (Obj != null)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			if (AdjustInitialCommunity)
			{
				int num = communityManager.Communities.IndexOf(((Character)Obj).InitialCommunity);
				((Character)Obj).InitialCommunity = ((num > 0) ? communityManager.Communities[num - 1] : null);
			}
			else
			{
				int num2 = communityManager.Communities.IndexOf(Obj.GetCommunity());
				Obj.SetCommunity((num2 > 0) ? communityManager.Communities[num2 - 1] : null);
			}
			BuildDisplayString();
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public void OnIncrement()
	{
		if (Obj != null)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			if (AdjustInitialCommunity)
			{
				int num = communityManager.Communities.IndexOf(((Character)Obj).GetCommunity());
				((Character)Obj).InitialCommunity = communityManager.Communities[Math.Min(communityManager.Communities.Count - 1, num + 1)];
			}
			else
			{
				int num2 = communityManager.Communities.IndexOf(Obj.GetCommunity());
				Obj.SetCommunity(communityManager.Communities[Math.Min(communityManager.Communities.Count - 1, num2 + 1)]);
			}
			BuildDisplayString();
			Session.Instance.AchievementsEnabled = false;
		}
	}

	public void BuildCommunityDisplayString(ref StringBuilder value)
	{
		value.Length = 0;
		value.Append(Name);
		value.Append(" < ");
		if (Obj != null)
		{
			(AdjustInitialCommunity ? ((Character)Obj).InitialCommunity : Obj.GetCommunity())?.BuildDisplayName(value, noStrangers: true, englishOnly: false);
		}
		value.Append(" >");
	}
}
