using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class QuestPage : InfoPage
{
	private class SortGroupsByDiscoveredTimeDescending : IComparer<QuestGroupBehaviour>
	{
		int IComparer<QuestGroupBehaviour>.Compare(QuestGroupBehaviour a, QuestGroupBehaviour b)
		{
			if (!a.HasActiveQuests && b.HasActiveQuests)
			{
				return 1;
			}
			if (a.HasActiveQuests && !b.HasActiveQuests)
			{
				return -1;
			}
			if (!a.QuestGroup.MainQuest && b.QuestGroup.MainQuest)
			{
				return 1;
			}
			if (a.QuestGroup.MainQuest && !b.QuestGroup.MainQuest)
			{
				return -1;
			}
			if (a.DiscoveredTime < b.DiscoveredTime)
			{
				return 1;
			}
			if (a.DiscoveredTime > b.DiscoveredTime)
			{
				return -1;
			}
			return 0;
		}
	}

	private class SortQuestsByDiscoveredTimeDescending : IComparer<QuestBehaviour>
	{
		int IComparer<QuestBehaviour>.Compare(QuestBehaviour a, QuestBehaviour b)
		{
			if (a.QuestInstance.DiscoveredTime < b.QuestInstance.DiscoveredTime)
			{
				return 1;
			}
			if (a.QuestInstance.DiscoveredTime > b.QuestInstance.DiscoveredTime)
			{
				return -1;
			}
			if (a.QuestInstance.Id < b.QuestInstance.Id)
			{
				return 1;
			}
			if (a.QuestInstance.Id > b.QuestInstance.Id)
			{
				return -1;
			}
			return 0;
		}
	}

	public static QuestPage Instance;

	public GameObject UnityQuestsObj;

	public List<QuestBehaviour> UnityQuests = new List<QuestBehaviour>();

	public GameObject UnityQuestGroupsObj;

	public List<QuestGroupBehaviour> UnityQuestGroups = new List<QuestGroupBehaviour>();

	public GameObject UnityMainQuestsSectionTitle;

	public GameObject UnitySideQuestsSectionTitle;

	public GameObject UnityCompletedQuestsSectionTitle;

	public QuestGroupBehaviour CurrentGroup;

	private static SortGroupsByDiscoveredTimeDescending GroupSorter = new SortGroupsByDiscoveredTimeDescending();

	private static SortQuestsByDiscoveredTimeDescending QuestSorter = new SortQuestsByDiscoveredTimeDescending();

	private bool WantSelectGroupWithCurrentQuest = true;

	private static int INFOPAGE_Quests = StringUtil.JenkinsHash("INFOPAGE_Quests");

	public override void OnAwake()
	{
		Instance = this;
		UnityQuestsObj = base.gameObject.FindChild("Quests/Viewport/Content");
		UnityQuestGroupsObj = base.gameObject.FindChild("QuestGroups/Viewport/Content");
		UnityMainQuestsSectionTitle = base.gameObject.FindChild("QuestGroups/Viewport/Content/MainQuests");
		UnitySideQuestsSectionTitle = base.gameObject.FindChild("QuestGroups/Viewport/Content/SideQuests");
		UnityCompletedQuestsSectionTitle = base.gameObject.FindChild("QuestGroups/Viewport/Content/FinishedQuests");
	}

	public QuestGroupBehaviour FindQuestGroupBehaviour(string groupID, QuestInstance questInstance)
	{
		for (int i = 0; i < UnityQuestGroupsObj.transform.childCount; i++)
		{
			QuestGroupBehaviour component = UnityQuestGroupsObj.transform.GetChild(i).GetComponent<QuestGroupBehaviour>();
			if (component != null && component.MatchesQuestInstance(questInstance))
			{
				return component;
			}
		}
		return null;
	}

	public QuestBehaviour FindQuestBehaviour(QuestInstance questInstance)
	{
		for (int i = 0; i < UnityQuestsObj.transform.childCount; i++)
		{
			QuestBehaviour component = UnityQuestsObj.transform.GetChild(i).GetComponent<QuestBehaviour>();
			if (component != null && component.QuestInstance == questInstance)
			{
				return component;
			}
		}
		return null;
	}

	public QuestPage Initialize()
	{
		CurrentGroup = null;
		WantSelectGroupWithCurrentQuest = true;
		return this;
	}

	public override void Populate()
	{
		base.Populate();
		UnityQuests.Clear();
		UnityQuestGroups.Clear();
		for (int i = 0; i < UnityQuestGroupsObj.transform.childCount; i++)
		{
			QuestGroupBehaviour component = UnityQuestGroupsObj.transform.GetChild(i).GetComponent<QuestGroupBehaviour>();
			if (component != null)
			{
				component.DiscoveredTime = TimeSpan.Zero;
				component.TopActiveQuest = null;
				component.TopQuest = null;
				component.HasAnyFailedQuests = false;
				component.HasCurrentQuest = false;
			}
		}
		foreach (KeyValuePair<int, QuestInstance> questInstance in StoryManager.Instance.QuestInstances)
		{
			QuestInstance value = questInstance.Value;
			if (value.Quest == null)
			{
				continue;
			}
			QuestGroupBehaviour questGroupBehaviour = FindQuestGroupBehaviour(value.Quest.GroupID, value);
			if (questGroupBehaviour == null)
			{
				questGroupBehaviour = UnityEngine.Object.Instantiate(InfoScreen.QuestGroup.GetAsset(), UnityQuestGroupsObj.transform).GetComponent<QuestGroupBehaviour>();
				questGroupBehaviour.QuestGroup = GameImpl.Instance.FindQuestGroupByUniqueID(value.Quest.GroupID);
				questGroupBehaviour.QuestGiver = value.QuestGiver;
				questGroupBehaviour.QuestSeeker = value.QuestSeeker;
				questGroupBehaviour.QuestObject = value.QuestObject;
				questGroupBehaviour.DiscoveredTime = value.DiscoveredTime;
				questGroupBehaviour.TopActiveQuest = ((value.State == QuestInstance.EState.Active) ? value : null);
				questGroupBehaviour.TopQuest = value;
			}
			else
			{
				if (questGroupBehaviour.DiscoveredTime == TimeSpan.Zero || value.DiscoveredTime < questGroupBehaviour.DiscoveredTime)
				{
					questGroupBehaviour.DiscoveredTime = value.DiscoveredTime;
				}
				if (value.State == QuestInstance.EState.Active && (questGroupBehaviour.TopActiveQuest == null || questGroupBehaviour.TopActiveQuest.DiscoveredTime < value.DiscoveredTime))
				{
					questGroupBehaviour.TopActiveQuest = value;
				}
				if (questGroupBehaviour.TopQuest == null || questGroupBehaviour.TopQuest.DiscoveredTime < value.DiscoveredTime)
				{
					questGroupBehaviour.TopQuest = value;
				}
			}
			questGroupBehaviour.HasAnyFailedQuests |= value.State == QuestInstance.EState.Failed;
			questGroupBehaviour.HasCurrentQuest |= value == StoryManager.Instance.CurrentQuest;
			questGroupBehaviour.Populate();
			questGroupBehaviour.GetComponent<RawImage>().color = ((CurrentGroup == questGroupBehaviour) ? InfoScreen.QuestCol : InfoScreen.QuestGroupCol);
			if (!UnityQuestGroups.Contains(questGroupBehaviour))
			{
				UnityQuestGroups.Add(questGroupBehaviour);
			}
		}
		for (int num = UnityQuestGroupsObj.transform.childCount - 1; num >= 0; num--)
		{
			QuestGroupBehaviour component2 = UnityQuestGroupsObj.transform.GetChild(num).GetComponent<QuestGroupBehaviour>();
			if (component2 != null && !UnityQuestGroups.Contains(component2))
			{
				UnityEngine.Object.DestroyImmediate(component2.gameObject);
			}
		}
		UnityQuestGroups.Sort(GroupSorter);
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int j = 0; j < UnityQuestGroups.Count; j++)
		{
			if (j == 0 || UnityQuestGroups[j - 1].QuestGroup.MainQuest != UnityQuestGroups[j].QuestGroup.MainQuest || UnityQuestGroups[j - 1].HasActiveQuests != UnityQuestGroups[j].HasActiveQuests)
			{
				if (UnityQuestGroups[j].HasActiveQuests)
				{
					if (UnityQuestGroups[j].QuestGroup.MainQuest && !flag)
					{
						UnityMainQuestsSectionTitle.transform.SetSiblingIndex(num2);
						flag = true;
						num2++;
					}
					else if (!flag2)
					{
						UnitySideQuestsSectionTitle.transform.SetSiblingIndex(num2);
						flag2 = true;
						num2++;
					}
				}
				else if (!flag3)
				{
					UnityCompletedQuestsSectionTitle.transform.SetSiblingIndex(num2);
					flag3 = true;
					num2++;
				}
			}
			UnityQuestGroups[j].transform.SetSiblingIndex(num2);
			num2++;
		}
		UnityMainQuestsSectionTitle.SetActive(flag);
		UnitySideQuestsSectionTitle.SetActive(flag2);
		UnityCompletedQuestsSectionTitle.SetActive(flag3);
		if (WantSelectGroupWithCurrentQuest)
		{
			for (int k = 0; k < UnityQuestGroups.Count; k++)
			{
				if (UnityQuestGroups[k].HasCurrentQuest)
				{
					SetCurrentGroup(UnityQuestGroups[k]);
					break;
				}
			}
		}
		if (CurrentGroup != null)
		{
			foreach (KeyValuePair<int, QuestInstance> questInstance2 in StoryManager.Instance.QuestInstances)
			{
				QuestInstance value2 = questInstance2.Value;
				if (value2.Quest != null && CurrentGroup.QuestGroup != null && CurrentGroup.MatchesQuestInstance(value2))
				{
					QuestBehaviour questBehaviour = FindQuestBehaviour(value2);
					if (questBehaviour == null)
					{
						questBehaviour = UnityEngine.Object.Instantiate(InfoScreen.Quest.GetAsset(), UnityQuestsObj.transform).GetComponent<QuestBehaviour>();
						questBehaviour.QuestInstance = value2;
					}
					questBehaviour.Populate();
					UnityQuests.Add(questBehaviour);
				}
			}
		}
		for (int num3 = UnityQuestsObj.transform.childCount - 1; num3 >= 0; num3--)
		{
			QuestBehaviour component3 = UnityQuestsObj.transform.GetChild(num3).GetComponent<QuestBehaviour>();
			if (component3 != null && !UnityQuests.Contains(component3))
			{
				UnityEngine.Object.DestroyImmediate(component3.gameObject);
			}
		}
		UnityQuests.Sort(QuestSorter);
		for (int l = 0; l < UnityQuests.Count; l++)
		{
			UnityQuests[l].transform.SetSiblingIndex(l);
		}
	}

	public override void Update()
	{
		base.Update();
		SelectableBehaviour.SelectionMode curSelectionMode = SelectableBehaviour.CurSelectionMode;
		if (WantSelectGroupWithCurrentQuest)
		{
			if (CurrentGroup != null && curSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				CurrentGroup.GetComponent<Button>().Select();
			}
			WantSelectGroupWithCurrentQuest = false;
		}
		if (curSelectionMode == SelectableBehaviour.SelectionMode.Buttons && UnityEventSystem.currentSelectedGameObject != null)
		{
			QuestGroupBehaviour component = UnityEventSystem.currentSelectedGameObject.GetComponent<QuestGroupBehaviour>();
			if (component != null && component != CurrentGroup)
			{
				SetCurrentGroup(component);
			}
		}
		if (QuestBehaviour.Hovered != null)
		{
			if (QuestBehaviour.Hovered.CanMakeCurrentQuest())
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt((curSelectionMode == SelectableBehaviour.SelectionMode.Cursor) ? InputFunction.MainAction : InputFunction.MenuSelect, ButtonPromptBarBehaviour.PROMPT_SetActiveQuest);
			}
			else if (QuestBehaviour.Hovered.CanShowOnMap())
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt((curSelectionMode == SelectableBehaviour.SelectionMode.Cursor) ? InputFunction.MainAction : InputFunction.MenuSelect, ButtonPromptBarBehaviour.PROMPT_ViewOnMap);
			}
		}
		else if (QuestGroupBehaviour.Hovered != null)
		{
			if (QuestGroupBehaviour.Hovered.CanMakeCurrentQuestGroup())
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt((curSelectionMode == SelectableBehaviour.SelectionMode.Cursor) ? InputFunction.MainAction : InputFunction.MenuSelect, ButtonPromptBarBehaviour.PROMPT_SetActiveQuest);
			}
			else if (QuestGroupBehaviour.Hovered.CanShowOnMap())
			{
				ButtonPromptBarBehaviour.Instance.AddButtonPrompt((curSelectionMode == SelectableBehaviour.SelectionMode.Cursor) ? InputFunction.MainAction : InputFunction.MenuSelect, ButtonPromptBarBehaviour.PROMPT_ViewOnMap);
			}
		}
	}

	public void SetCurrentGroup(QuestGroupBehaviour questGroupBehaviour)
	{
		CurrentGroup = questGroupBehaviour;
		InfoScreen.Instance.WantRepopulate = true;
	}

	public override bool CanToggleActionMenuVisible()
	{
		return false;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Quests));
	}
}
