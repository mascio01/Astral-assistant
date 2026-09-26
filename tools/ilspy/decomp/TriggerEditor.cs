internal class TriggerEditor : DebugMenu
{
	private EnabledTrigger EnabledTrigger;

	private bool WantRepopulate;

	public TriggerEditor(EnabledTrigger enabledTrigger)
		: base(enabledTrigger.Trigger.UniqueID)
	{
		EnabledTrigger = enabledTrigger;
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
		Items.Add(new DebugMenuBaseObjectID("Actor", EnabledTrigger.Actor, delegate(BaseObject obj)
		{
			EnabledTrigger.Actor = obj as Character;
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuBaseObjectID("Object", EnabledTrigger.Object, delegate(BaseObject obj)
		{
			EnabledTrigger.Object = obj;
			Session.Instance.AchievementsEnabled = false;
		}));
	}
}
