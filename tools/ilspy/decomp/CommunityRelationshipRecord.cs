public struct CommunityRelationshipRecord : IReflectable
{
	public int OtherCommunityId;

	public CommunityRelationshipType RelationshipType;

	public bool Initiator;

	public CommunityRelationshipRecord(int id, CommunityRelationshipType relationshipType, bool initiator)
	{
		OtherCommunityId = id;
		RelationshipType = relationshipType;
		Initiator = initiator;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref OtherCommunityId);
		reflector.Add(ref RelationshipType);
		reflector.Add(ref Initiator);
	}

	public Community GetOtherCommunity()
	{
		return BaseObjectManager.Instance.FindBaseObjectByID(OtherCommunityId) as Community;
	}
}
