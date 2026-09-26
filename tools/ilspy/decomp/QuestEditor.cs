internal class QuestEditor : DebugMenu
{
	private QuestInstance QuestInstance;

	private bool WantRepopulate;

	public QuestEditor(QuestInstance questInstance)
		: base(questInstance.GetDisplayNameString())
	{
		QuestInstance = questInstance;
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
		Items.Add(new DebugMenuBaseObjectID("Quest Giver", QuestInstance.QuestGiver, delegate(BaseObject obj)
		{
			QuestInstance.QuestGiver = obj as Character;
			StoryManager.Instance.RebuildQuestInstances();
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuBaseObjectID("Quest Seeker", QuestInstance.QuestSeeker, delegate(BaseObject obj)
		{
			QuestInstance.QuestSeeker = obj as Character;
			StoryManager.Instance.RebuildQuestInstances();
			Session.Instance.AchievementsEnabled = false;
		}));
		Items.Add(new DebugMenuBaseObjectID("Quest Object", QuestInstance.QuestObject, delegate(BaseObject obj)
		{
			QuestInstance.SetQuestObject(obj);
			StoryManager.Instance.RebuildQuestInstances();
			Session.Instance.AchievementsEnabled = false;
		}));
	}
}
