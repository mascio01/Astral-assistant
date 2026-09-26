internal class CommunityList : DebugMenu
{
	public CommunityList()
		: base(GameImpl.Translate("DEBUG_CommunityList"))
	{
	}

	public override void ActivateImpl()
	{
		Items.Clear();
		foreach (Community community in Session.Instance.CommunityManager.Communities)
		{
			Community localCommunity = community;
			Items.Add(new DebugMenuItemCustom(CommunityEditor.GetCommunityNameAsString(community), delegate
			{
				Edit(localCommunity);
			}));
		}
		Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_New"), New, affectsGameState: true));
		base.ActivateImpl();
	}

	public void Edit(Community community)
	{
		SetState(DebugPageState.ChildActive, new CommunityEditor(community));
	}

	public void New()
	{
		Community community = Community.Spawn(CommunityType.Normal);
		community.CommunityName.Randomise(MathUtil.NonDeterministicRand, unique: true, community);
		SetState(DebugPageState.ChildActive, new CommunityEditor(community));
	}
}
