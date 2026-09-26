internal class InvaderList : DebugMenu
{
	public InvaderList()
		: base(GameImpl.Translate("DEBUG_Invaders"))
	{
	}

	public override void ActivateImpl()
	{
		Items.Clear();
		foreach (InvaderInstance invaderInstance in StoryManager.Instance.ActiveInvaders)
		{
			string text = invaderInstance.CreatedObjects.Count + " objects";
			text = text + ", Active: " + invaderInstance.Active;
			text = text + ", Source: " + ((invaderInstance.SourceObject != null) ? invaderInstance.SourceObject.GetDisplayNameString() : "null");
			text = text + "(" + invaderInstance.GetDisplayNameString() + ")";
			Items.Add(new DebugMenuItemCustom(invaderInstance.Invader.UniqueID, delegate
			{
				Edit(invaderInstance);
			}, text));
		}
		base.ActivateImpl();
	}

	public void Edit(InvaderInstance invaderInstance)
	{
		SetState(DebugPageState.ChildActive, new InvaderEditor(invaderInstance));
	}
}
