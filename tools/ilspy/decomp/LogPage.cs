using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogPage : InfoPage
{
	public static LogPage Instance;

	public TextMeshProUGUI UnityPerCharacterEventsText;

	public Toggle UnityPerCharacterEventsToggle;

	public Toggle UnityAllEventsToggle;

	public ScrollRect UnityLog;

	public GameObject UnityLogContents;

	public Character CurrentCharacter;

	public bool PerPerson;

	private int WantScrollToBottom = 2;

	private bool SettingToggles;

	private LogEvent PrevLogEvent;

	private List<LogEvent> EventsToShow = new List<LogEvent>();

	private static int DefaultNumEventsToShow = 10;

	private int NumEventsToShow = DefaultNumEventsToShow;

	private static int INFOPAGE_Log = StringUtil.JenkinsHash("INFOPAGE_Log");

	public override void OnAwake()
	{
		Instance = this;
		UnityPerCharacterEventsText = base.gameObject.FindChild("Toggles/PerCharacterToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityPerCharacterEventsToggle = base.gameObject.FindChild("Toggles/PerCharacterToggle").GetComponent<Toggle>();
		UnityAllEventsToggle = base.gameObject.FindChild("Toggles/AllToggle").GetComponent<Toggle>();
		UnityLog = base.gameObject.FindChild("Log").GetComponent<ScrollRect>();
		UnityLogContents = base.gameObject.FindChild("Log/Viewport/Content");
		UnityLogContents.DeleteAllChildren();
	}

	public LogPage Initialize(Character character)
	{
		CurrentCharacter = character;
		WantScrollToBottom = 2;
		if (CurrentCharacter == null)
		{
			PerPerson = false;
		}
		return this;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		EventsToShow.Clear();
	}

	public override void Populate()
	{
		base.Populate();
		float height = ((RectTransform)UnityLog.transform).rect.height;
		float height2 = ((RectTransform)UnityLog.content.transform).rect.height;
		if (UnityLog.content.anchoredPosition.y >= Mathf.Max(height2 - height, 0f) - 10f)
		{
			WantScrollToBottom = 2;
		}
		EventsToShow.Clear();
		foreach (LogEvent logEvent in Session.Instance.LogEvents)
		{
			if (!PerPerson || logEvent.IsAboutCharacter(CurrentCharacter))
			{
				EventsToShow.Add(logEvent);
			}
		}
		PrevLogEvent = ((EventsToShow.Count >= NumEventsToShow + 1) ? EventsToShow[EventsToShow.Count - NumEventsToShow - 1] : null);
		int i = 0;
		for (int j = Math.Max(0, EventsToShow.Count - NumEventsToShow); j < EventsToShow.Count; j++)
		{
			AddLogEntry(EventsToShow[j], ref i);
		}
		for (; i < UnityLogContents.transform.childCount; i++)
		{
			UnityEngine.Object.Destroy(UnityLogContents.transform.GetChild(i).gameObject);
		}
		PrevLogEvent = null;
		UnityEventSystem.firstSelectedGameObject = ((UnityLogContents.transform.childCount > 0) ? UnityLogContents.transform.GetChild(UnityLogContents.transform.childCount - 1).gameObject : null);
		UnityPerCharacterEventsToggle.gameObject.SetActive(CurrentCharacter != null);
		if (CurrentCharacter != null)
		{
			string str = GameImpl.Translate("HUD_PerCharacterEvents").Replace("%1", CurrentCharacter.GetDisplayNameString());
			str = StringUtil.ApplyFormulae(str, null, CurrentCharacter);
			UnityPerCharacterEventsText.SetUnityText(str);
		}
		SettingToggles = true;
		UnityPerCharacterEventsToggle.isOn = PerPerson;
		UnityAllEventsToggle.isOn = !PerPerson;
		SettingToggles = false;
	}

	public override void Update()
	{
		base.Update();
		if (WantScrollToBottom > 0)
		{
			float height = ((RectTransform)UnityLog.transform).rect.height;
			float height2 = ((RectTransform)UnityLog.content.transform).rect.height;
			UnityLog.content.anchoredPosition = new Vector2(UnityLog.content.anchoredPosition.x, Mathf.Max(height2 - height, 0f));
			WantScrollToBottom--;
		}
		if (UnityLog.content.anchoredPosition.y <= 10f && NumEventsToShow < EventsToShow.Count)
		{
			UnityLog.content.anchoredPosition = new Vector2(UnityLog.content.anchoredPosition.x, 20f);
			NumEventsToShow++;
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	private LogEventBehaviour FindLogEventBehaviour(LogEvent logEvent, int index)
	{
		for (int i = index; i < UnityLogContents.transform.childCount; i++)
		{
			LogEventBehaviour component = UnityLogContents.transform.GetChild(i).GetComponent<LogEventBehaviour>();
			if (component.LogEvent == logEvent)
			{
				return component;
			}
		}
		return null;
	}

	private void AddLogSeparator(int day, int dayOfYear, ref int index)
	{
		LogEventBehaviour logEventBehaviour = FindLogEventBehaviour(null, index);
		if (logEventBehaviour == null)
		{
			logEventBehaviour = UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogSeparator, UnityLogContents.transform).GetComponent<LogEventBehaviour>();
		}
		logEventBehaviour.InitializeSeparator(day, dayOfYear);
		logEventBehaviour.transform.SetSiblingIndex(index++);
	}

	private void AddLogEntry(LogEvent logEvent, ref int index)
	{
		if (PrevLogEvent == null || Session.CalcDayFromTime(logEvent.Time) != Session.CalcDayFromTime(PrevLogEvent.Time))
		{
			AddLogSeparator(Session.CalcDayFromTime(logEvent.Time), (int)Session.Instance.Weather.CalcDayOfYearFromTime(logEvent.Time), ref index);
		}
		LogEventBehaviour logEventBehaviour = FindLogEventBehaviour(logEvent, index);
		if (logEventBehaviour == null)
		{
			switch (logEvent.Type)
			{
			case LogEventType.Speech:
				logEventBehaviour = ((!logEvent.SpeakerWasDirectControlled) ? UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogEntrySpeechLeft, UnityLogContents.transform).GetComponent<LogEventBehaviour>() : UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogEntrySpeechRight, UnityLogContents.transform).GetComponent<LogEventBehaviour>());
				break;
			case LogEventType.DeclaredWar:
			case LogEventType.DeclaredPeace:
			case LogEventType.MadeAlliance:
			case LogEventType.BrokeAlliance:
				logEventBehaviour = UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogEntryText, UnityLogContents.transform).GetComponent<LogEventBehaviour>();
				break;
			case LogEventType.QuestDiscovered:
			case LogEventType.QuestCompleted:
			case LogEventType.QuestFailed:
			case LogEventType.NewRecipe:
				logEventBehaviour = UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogEntryQuest, UnityLogContents.transform).GetComponent<LogEventBehaviour>();
				break;
			default:
				logEventBehaviour = UnityEngine.Object.Instantiate((GameObject)InfoScreen.LogEntryIcon, UnityLogContents.transform).GetComponent<LogEventBehaviour>();
				break;
			}
			logEventBehaviour.Initialize(logEvent);
		}
		logEventBehaviour.transform.SetSiblingIndex(index++);
		PrevLogEvent = logEvent;
	}

	public void OnPerCharacterEventsClicked(bool v)
	{
		if (!SettingToggles)
		{
			PerPerson = true;
			NumEventsToShow = DefaultNumEventsToShow;
			WantScrollToBottom = 2;
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void OnAllEventsClicked(bool v)
	{
		if (!SettingToggles)
		{
			PerPerson = false;
			NumEventsToShow = DefaultNumEventsToShow;
			WantScrollToBottom = 2;
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Log));
	}

	public override bool CanToggleActionMenuVisible()
	{
		return false;
	}

	public override bool WantShowActionMenuOptionOnButtonPromptBar()
	{
		return false;
	}
}
