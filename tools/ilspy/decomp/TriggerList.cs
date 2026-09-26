using System.Collections.Generic;

internal class TriggerList : DebugMenu
{
	public TriggerList()
		: base(GameImpl.Translate("DEBUG_Triggers"))
	{
	}

	public override void ActivateImpl()
	{
		Items.Clear();
		for (int i = 0; i < StoryManager.Instance.EnabledTriggers.Length; i++)
		{
			List<EnabledTrigger> list = StoryManager.Instance.EnabledTriggers[i];
			for (int j = 0; j < list.Count; j++)
			{
				EnabledTrigger enabledTrigger = list[j];
				string text = "Actor: " + ((enabledTrigger.Actor != null) ? enabledTrigger.Actor.GetDisplayNameString() : "null");
				text = text + ", Object: " + ((enabledTrigger.Object != null) ? enabledTrigger.Object.GetDisplayNameString() : "null");
				Items.Add(new DebugMenuItemCustom(enabledTrigger.Trigger.UniqueID, delegate
				{
					Edit(enabledTrigger);
				}, text));
			}
		}
		base.ActivateImpl();
	}

	public void Edit(EnabledTrigger enabledTrigger)
	{
		SetState(DebugPageState.ChildActive, new TriggerEditor(enabledTrigger));
	}
}
