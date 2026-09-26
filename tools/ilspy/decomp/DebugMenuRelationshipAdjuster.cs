using System;
using UnityEngine;

public class DebugMenuRelationshipAdjuster : DebugMenuItem
{
	private Community Community;

	private int Index;

	private string OtherCommunityDisplayString;

	private string RelationshipTypeDisplayString;

	private const string Left = "<";

	private const string Right = ">";

	private static int MENU_Delete = StringUtil.JenkinsHash("MENU_Delete");

	public DebugMenuRelationshipAdjuster(string name, Community community, int index)
		: base(name)
	{
		Community = community;
		Index = index;
		BuildDisplayString();
	}

	public override void Update(Vector2 pos)
	{
		CommunityRelationshipRecord communityRelationshipRecord = ((Index >= Community.CommunityRelationships.Count) ? default(CommunityRelationshipRecord) : Community.CommunityRelationships[Index]);
		GUI.Label(new Rect(pos.x, pos.y, 300f, 30f), OtherCommunityDisplayString);
		if (GUIButton(new Rect(pos.x + 300f, pos.y, 30f, 30f), "<"))
		{
			OnDecrementCommunity();
		}
		if (GUIButton(new Rect(pos.x + 330f, pos.y, 30f, 30f), ">"))
		{
			OnIncrementCommunity();
		}
		GUI.Label(new Rect(pos.x + 400f, pos.y, 100f, 30f), RelationshipTypeDisplayString);
		if (GUIButton(new Rect(pos.x + 520f, pos.y, 30f, 30f), "<"))
		{
			OnSetRelationshipType((CommunityRelationshipType)Math.Max((int)(communityRelationshipRecord.RelationshipType - 1), 0));
		}
		int num = (int)GUIHorizontalSlider(new Rect(pos.x + 570f, pos.y, 200f, 30f), (float)communityRelationshipRecord.RelationshipType, 0f, 5f);
		if (num != (int)communityRelationshipRecord.RelationshipType)
		{
			OnSetRelationshipType((CommunityRelationshipType)num);
		}
		if (GUIButton(new Rect(pos.x + 790f, pos.y, 30f, 30f), ">"))
		{
			OnSetRelationshipType((CommunityRelationshipType)Math.Min((int)(communityRelationshipRecord.RelationshipType + 1), 5));
		}
		if (Index < Community.CommunityRelationships.Count && GUIButton(new Rect(pos.x + 830f, pos.y, 100f, 30f), GameImpl.Translate(MENU_Delete)))
		{
			OnDeleteRelationship();
		}
	}

	private void BuildDisplayString()
	{
		Community community = ((Index < Community.CommunityRelationships.Count) ? Community.CommunityRelationships[Index].GetOtherCommunity() : null);
		OtherCommunityDisplayString = Name.Replace("%1", (community != null) ? community.GetDisplayNameString() : string.Empty);
		RelationshipTypeDisplayString = ((Index < Community.CommunityRelationships.Count) ? Community.CommunityRelationships[Index].RelationshipType.ToString() : string.Empty);
	}

	public void OnDecrementCommunity()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		CommunityRelationshipRecord communityRelationshipRecord;
		if (Index < Community.CommunityRelationships.Count)
		{
			communityRelationshipRecord = Community.CommunityRelationships[Index];
		}
		else
		{
			communityRelationshipRecord = default(CommunityRelationshipRecord);
			Community.CommunityRelationships.Add(communityRelationshipRecord);
		}
		Community otherCommunity = communityRelationshipRecord.GetOtherCommunity();
		otherCommunity?.ClearCommunityRelationship(Community);
		int num = communityManager.Communities.IndexOf(otherCommunity);
		Community community = ((num > 0) ? communityManager.Communities[num - 1] : null);
		communityRelationshipRecord.OtherCommunityId = community?.Id ?? 0;
		communityRelationshipRecord.Initiator = true;
		Community.CommunityRelationships[Index] = communityRelationshipRecord;
		community?.SetCommunityRelationship(Community, communityRelationshipRecord.RelationshipType, initiator: false);
		communityManager.CacheAllies();
		BuildDisplayString();
		Session.Instance.AchievementsEnabled = false;
	}

	public void OnIncrementCommunity()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		CommunityRelationshipRecord communityRelationshipRecord;
		if (Index < Community.CommunityRelationships.Count)
		{
			communityRelationshipRecord = Community.CommunityRelationships[Index];
		}
		else
		{
			communityRelationshipRecord = default(CommunityRelationshipRecord);
			Community.CommunityRelationships.Add(communityRelationshipRecord);
		}
		Community otherCommunity = communityRelationshipRecord.GetOtherCommunity();
		otherCommunity?.ClearCommunityRelationship(Community);
		int num = communityManager.Communities.IndexOf(otherCommunity);
		Community community = communityManager.Communities[Math.Min(communityManager.Communities.Count - 1, num + 1)];
		if (community == Community)
		{
			community = communityManager.Communities[Math.Min(communityManager.Communities.Count - 1, num + 2)];
			if (community == Community)
			{
				community = otherCommunity;
			}
		}
		communityRelationshipRecord.OtherCommunityId = community?.Id ?? 0;
		communityRelationshipRecord.Initiator = true;
		if (Index >= Community.CommunityRelationships.Count || Index < 0)
		{
			Debug.LogError("Index out of range: " + Index);
		}
		Community.CommunityRelationships[Index] = communityRelationshipRecord;
		community?.SetCommunityRelationship(Community, communityRelationshipRecord.RelationshipType, initiator: false);
		communityManager.CacheAllies();
		BuildDisplayString();
		Session.Instance.AchievementsEnabled = false;
	}

	private void OnSetRelationshipType(CommunityRelationshipType relationshipType)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		CommunityRelationshipRecord communityRelationshipRecord;
		if (Index < Community.CommunityRelationships.Count)
		{
			communityRelationshipRecord = Community.CommunityRelationships[Index];
		}
		else
		{
			communityRelationshipRecord = default(CommunityRelationshipRecord);
			Community.CommunityRelationships.Add(communityRelationshipRecord);
		}
		communityRelationshipRecord.RelationshipType = relationshipType;
		Community.CommunityRelationships[Index] = communityRelationshipRecord;
		communityManager.CacheAllies();
		BuildDisplayString();
		Session.Instance.AchievementsEnabled = false;
	}

	private void OnDeleteRelationship()
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		if (Index < Community.CommunityRelationships.Count)
		{
			Community.CommunityRelationships[Index].GetOtherCommunity()?.ClearCommunityRelationship(Community);
			Community.CommunityRelationships.RemoveAt(Index);
			communityManager.CacheAllies();
			Session.Instance.AchievementsEnabled = false;
		}
	}
}
