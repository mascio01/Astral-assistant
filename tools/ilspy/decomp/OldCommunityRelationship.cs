public struct OldCommunityRelationship : IReflectable
{
	public int Community1Id;

	public int Community2Id;

	public CommunityRelationshipType RelationshipType;

	public OldCommunityRelationship(int id1, int id2, CommunityRelationshipType relationshipType)
	{
		Community1Id = id1;
		Community2Id = id2;
		RelationshipType = relationshipType;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Community1Id);
		reflector.Add(ref Community2Id);
		reflector.Add(ref RelationshipType);
	}

	public Community GetCommunity1()
	{
		return BaseObjectManager.Instance.FindBaseObjectByID(Community1Id) as Community;
	}

	public Community GetCommunity2()
	{
		return BaseObjectManager.Instance.FindBaseObjectByID(Community2Id) as Community;
	}

	public bool Involves(int id)
	{
		if (Community1Id != id)
		{
			return Community2Id == id;
		}
		return true;
	}

	public bool IsBetween(int id1, int id2)
	{
		if (Community1Id != id1 || Community2Id != id2)
		{
			if (Community2Id == id1)
			{
				return Community1Id == id2;
			}
			return false;
		}
		return true;
	}

	public Community GetOther(Community community)
	{
		if (Community1Id == community.Id)
		{
			return BaseObjectManager.Instance.FindBaseObjectByID(Community2Id) as Community;
		}
		if (Community2Id == community.Id)
		{
			return BaseObjectManager.Instance.FindBaseObjectByID(Community1Id) as Community;
		}
		return null;
	}

	public void SetOther(Community myCommunity, Community otherCommunity)
	{
		if (Community1Id == (myCommunity?.Id ?? 0))
		{
			Community2Id = otherCommunity?.Id ?? 0;
		}
		if (Community2Id == (myCommunity?.Id ?? 0))
		{
			Community1Id = otherCommunity?.Id ?? 0;
		}
	}
}
