internal class InvaderEditor : DebugMenu
{
	private InvaderInstance InvaderInstance;

	private bool WantRepopulate;

	public InvaderEditor(InvaderInstance questInstance)
		: base(questInstance.GetDisplayNameString())
	{
		InvaderInstance = questInstance;
		WantRepopulate = true;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (WantRepopulate)
		{
			WantRepopulate = false;
			Populate();
		}
		base.HandleInputImpl(inputFrame);
	}

	private void Populate()
	{
		Items.Add(new DebugMenuBaseObjectID("Source", InvaderInstance.SourceObject, delegate(BaseObject obj)
		{
			InvaderInstance.SourceObject = obj;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuItemCustom("Deactivate", delegate
		{
			StoryManager.Instance.DeactivateInvader(InvaderInstance);
			ClosePage();
		}, affectsGameState: true));
	}
}
