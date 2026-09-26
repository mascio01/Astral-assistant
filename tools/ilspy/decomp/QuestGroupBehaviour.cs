using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestGroupBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private RawImage UnityIcon;

	private TextMeshProUGUI UnityText;

	public QuestGroup QuestGroup;

	public Character QuestGiver;

	public Character QuestSeeker;

	public BaseObject QuestObject;

	public TimeSpan DiscoveredTime;

	public QuestInstance TopActiveQuest;

	public QuestInstance TopQuest;

	public bool HasCurrentQuest;

	public bool HasAnyFailedQuests;

	public static QuestGroupBehaviour Hovered;

	public bool HasActiveQuests => TopActiveQuest != null;

	public bool CanMakeCurrentQuestGroup()
	{
		if (HasActiveQuests)
		{
			return StoryManager.Instance.CurrentQuest != TopActiveQuest;
		}
		return false;
	}

	public bool CanShowOnMap()
	{
		if (HasActiveQuests && StoryManager.Instance.CurrentQuest == TopActiveQuest)
		{
			return StoryManager.Instance.QuestDestinations.Count > 0;
		}
		return false;
	}

	public void OnClicked()
	{
		bool flag = false;
		if (CanMakeCurrentQuestGroup())
		{
			StoryManager.Instance.CurrentQuest = TopActiveQuest;
			flag = true;
		}
		if (QuestPage.Instance.CurrentGroup != this)
		{
			QuestPage.Instance.SetCurrentGroup(this);
			flag = true;
		}
		if (!flag && CanShowOnMap())
		{
			InfoScreen.Instance.SetPage(MapPage.Instance);
			MapPage.Instance.GoToQuestDestination();
			flag = true;
		}
		if (flag)
		{
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
		}
	}

	public void Populate()
	{
		if (UnityIcon == null)
		{
			UnityIcon = base.gameObject.FindChild("Icon").GetComponent<RawImage>();
		}
		if (UnityText == null)
		{
			UnityText = base.gameObject.FindChild("Text").GetComponent<TextMeshProUGUI>();
		}
		QuestInstance.EState eState = ((!HasActiveQuests) ? ((!HasAnyFailedQuests) ? QuestInstance.EState.Completed : QuestInstance.EState.Failed) : QuestInstance.EState.Active);
		string str = ((QuestGroup != null) ? QuestGroup.GetTitleString(QuestGiver, QuestSeeker, QuestObject, TopQuest) : string.Empty);
		UnityText.SetUnityText(str);
		Texture2D texture2D = null;
		if (QuestGroup != null && StoryManager.Instance.CurrentQuest != null && MatchesQuestInstance(StoryManager.Instance.CurrentQuest))
		{
			texture2D = GameCursor.CurrentQuestIcon.GetAsset();
		}
		else
		{
			switch (eState)
			{
			case QuestInstance.EState.Completed:
				texture2D = GameCursor.CompletedQuestIcon.GetAsset();
				break;
			case QuestInstance.EState.Failed:
				texture2D = GameCursor.FailedQuestIcon.GetAsset();
				break;
			}
		}
		UnityIcon.gameObject.SetActive(texture2D != null);
		UnityIcon.texture = texture2D;
	}

	public bool MatchesQuestInstance(QuestInstance questInstance)
	{
		if (QuestGroup != null && QuestGroup.UniqueID == questInstance.Quest.GroupID)
		{
			if (QuestGiver != questInstance.QuestGiver || QuestSeeker != questInstance.QuestSeeker || QuestObject != questInstance.QuestObject)
			{
				return QuestGroup.MergeInstances;
			}
			return true;
		}
		return false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Hovered = this;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Hovered = null;
	}
}
