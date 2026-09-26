using System.Collections.Generic;

internal class QuestList : DebugMenu
{
	public QuestList()
		: base(GameImpl.Translate("DEBUG_Quests"))
	{
	}

	public override void ActivateImpl()
	{
		Items.Clear();
		foreach (KeyValuePair<int, QuestInstance> questInstance2 in StoryManager.Instance.QuestInstances)
		{
			QuestInstance questInstance = questInstance2.Value;
			string text = questInstance.State.ToString();
			text = text + ", Giver: " + ((questInstance.QuestGiver != null) ? questInstance.QuestGiver.GetDisplayNameString() : "null");
			text = text + ", Seeker: " + ((questInstance.QuestSeeker != null) ? questInstance.QuestSeeker.GetDisplayNameString() : "null");
			text = text + ", Object: " + ((questInstance.QuestObject != null) ? questInstance.QuestObject.GetDisplayNameString() : "null");
			text = text + " (" + questInstance.GetDisplayNameString() + ")";
			Items.Add(new DebugMenuItemCustom(questInstance.Quest.UniqueID, delegate
			{
				Edit(questInstance);
			}, text));
		}
		base.ActivateImpl();
	}

	public void Edit(QuestInstance questInstance)
	{
		SetState(DebugPageState.ChildActive, new QuestEditor(questInstance));
	}
}
