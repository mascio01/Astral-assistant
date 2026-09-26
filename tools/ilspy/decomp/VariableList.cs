internal class VariableList : DebugMenu
{
	private string AddVariableName = "";

	private bool WantRepopulate;

	public VariableList()
		: base(GameImpl.Translate("DEBUG_Variables"))
	{
	}

	public override void ActivateImpl()
	{
		WantRepopulate = true;
		base.ActivateImpl();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (WantRepopulate)
		{
			Items.Clear();
			Items.Add(new DebugMenuString(GameImpl.Translate("DEBUG_NewVariableName"), () => AddVariableName, delegate(string v)
			{
				AddVariableName = v;
			}));
			Items.Add(new DebugMenuItemCustom(GameImpl.Translate("DEBUG_Add"), delegate
			{
				StoryManager.Instance.IncrementVariable(AddVariableName, null, null, 1f);
				WantRepopulate = true;
				Session.Instance.AchievementsEnabled = false;
			}));
			for (int num = 0; num < StoryManager.Instance.Variables.Count; num++)
			{
				int localIndex = num;
				Variable var = StoryManager.Instance.Variables[localIndex];
				BaseObject baseObject = BaseObjectManager.Instance.FindBaseObjectByID(var.SubjectID);
				BaseObject baseObject2 = BaseObjectManager.Instance.FindBaseObjectByID(var.ObjectID);
				DebugMenuString debugMenuString = new DebugMenuString(string.Concat(var.UniqueID + ", Subject: " + ((baseObject != null) ? (baseObject.GetDisplayNameString() + " (" + baseObject.Id + ")") : "null"), ", Object: ", (baseObject2 != null) ? (baseObject2.GetDisplayNameString() + " (" + baseObject2.Id + ")") : "null"), () => var.Value.ToString(), delegate(string v)
				{
					var.Value = StringUtil.ParseFloat(v);
					if (var.Value == 0f)
					{
						StoryManager.Instance.Variables.RemoveAt(localIndex);
					}
					else
					{
						StoryManager.Instance.Variables[localIndex] = var;
					}
					Session.Instance.AchievementsEnabled = false;
				});
				debugMenuString.NameWidth = 600f;
				Items.Add(debugMenuString);
			}
		}
		base.HandleInputImpl(inputFrame);
	}

	public void Edit(QuestInstance questInstance)
	{
		SetState(DebugPageState.ChildActive, new QuestEditor(questInstance));
	}
}
