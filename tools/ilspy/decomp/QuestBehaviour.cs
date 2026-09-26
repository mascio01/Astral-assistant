using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuestBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private TextMeshProUGUI UnityText;

	private RawImage UnityIcon;

	private ProgressBarBehaviour UnityMetric;

	private RawImage UnityMarker;

	private RawImage UnityArrow;

	public QuestInstance QuestInstance;

	public static QuestBehaviour Hovered;

	public bool CanMakeCurrentQuest()
	{
		if (QuestInstance.State == QuestInstance.EState.Active)
		{
			return StoryManager.Instance.CurrentQuest != QuestInstance;
		}
		return false;
	}

	public bool CanShowOnMap()
	{
		if (QuestInstance.State == QuestInstance.EState.Active && StoryManager.Instance.CurrentQuest == QuestInstance)
		{
			return StoryManager.Instance.QuestDestinations.Count > 0;
		}
		return false;
	}

	public void OnClicked()
	{
		if (CanMakeCurrentQuest())
		{
			if (QuestPage.Instance.FindQuestGroupBehaviour(QuestInstance.Quest.GroupID, QuestInstance) != null)
			{
				StoryManager.Instance.CurrentQuest = QuestInstance;
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
			}
		}
		else if (CanShowOnMap())
		{
			InfoScreen.Instance.SetPage(MapPage.Instance);
			MapPage.Instance.GoToQuestDestination();
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
		}
	}

	public void Populate()
	{
		if (UnityIcon == null)
		{
			UnityIcon = base.gameObject.FindChild("Panel/Icon").GetComponent<RawImage>();
		}
		if (UnityText == null)
		{
			UnityText = base.gameObject.FindChild("Panel/Text").GetComponent<TextMeshProUGUI>();
		}
		if (UnityMetric == null)
		{
			UnityMetric = base.gameObject.FindChild("Metric").GetComponent<ProgressBarBehaviour>();
		}
		if (UnityMarker == null)
		{
			UnityMarker = base.gameObject.FindChild("Metric/Marker").GetComponent<RawImage>();
		}
		if (UnityArrow == null)
		{
			UnityArrow = base.gameObject.FindChild("Metric/Marker/Arrow").GetComponent<RawImage>();
		}
		UnityText.SetUnityText(QuestInstance.GetDisplayNameString());
		Texture2D texture2D = null;
		if (QuestInstance == StoryManager.Instance.CurrentQuest)
		{
			texture2D = GameCursor.CurrentQuestIcon;
		}
		else if (QuestInstance.State == QuestInstance.EState.Completed)
		{
			texture2D = GameCursor.CompletedQuestIcon;
		}
		else if (QuestInstance.State == QuestInstance.EState.Failed)
		{
			texture2D = GameCursor.FailedQuestIcon;
		}
		UnityIcon.gameObject.SetActive(texture2D != null);
		UnityIcon.texture = texture2D;
		UnityMetric.gameObject.SetActive(QuestInstance.Quest.Metric != QuestMetric.None && QuestInstance.State == QuestInstance.EState.Active);
	}

	public void Update()
	{
		if (QuestInstance != null && QuestInstance.Quest != null && QuestInstance.Quest.Metric != QuestMetric.None && UnityMarker != null)
		{
			float num = QuestInstance.CalcQuestMetric(addQuestMarkers: false);
			UnityMetric.SetValue(num / 100f);
			UnityMarker.rectTransform.anchoredPosition = new Vector2(((RectTransform)UnityMetric.transform).rect.width * QuestInstance.Quest.MetricCompletedAmount / 100f, 0f);
			bool flag = num >= QuestInstance.Quest.MetricCompletedAmount;
			if (QuestInstance.Quest.Metric == QuestMetric.ReduceQuestObjectMorale)
			{
				flag = !flag;
			}
			UnityArrow.color = (flag ? Color.green : Color.red);
		}
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
