using System;

[AttributeUsage(AttributeTargets.Field)]
public class OnlyVisibleForCommunityType : Attribute
{
	public CommunityType CommunityType1;

	public CommunityType CommunityType2;

	public OnlyVisibleForCommunityType(CommunityType communityType1, CommunityType communityType2)
	{
		CommunityType1 = communityType1;
		CommunityType2 = communityType2;
	}

	public bool Matches(BaseScriptObject obj)
	{
		if (obj is Template template)
		{
			if (template.CommunityType != CommunityType1)
			{
				return template.CommunityType == CommunityType2;
			}
			return true;
		}
		return false;
	}
}
