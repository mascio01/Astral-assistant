using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public class Quest : BaseScriptObject
{
	[XmlAttribute]
	public string GroupID;

	[AlwaysVisible]
	[XmlAttribute]
	public string NativeDescription;

	[AlwaysVisible]
	[TranslatedTextField]
	[DefaultValue(null)]
	public string TranslatedDescription;

	[XmlIgnore]
	public int DescriptionHash;

	public List<SpeechParam> Params;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool SkipNotificationIfAlreadyCompleted;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool ShowNotificationEventInCombat;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool HighPriority;

	[XmlAttribute]
	[DefaultValue(QuestMarkerType.Normal)]
	public QuestMarkerType QuestMarkerType;

	public static string[] QestMarkerTypeNames = StringUtil.GetEnumNames<QuestMarkerType>();

	public List<QuestMarker> QuestMarkers;

	public List<Condition> QuestMarkerConditions;

	[DefaultValue(QuestMetric.None)]
	public QuestMetric Metric;

	[DefaultValue(100f)]
	public float MetricCompletedAmount = 100f;

	[DefaultValue(null)]
	public string MetricStringData;

	[DefaultValue(0f)]
	public float MetricLimit;

	[DefaultValue(null)]
	public string MetricStringData2;

	[DefaultValue(0f)]
	public float MetricLimit2;

	[DefaultValue(false)]
	public bool MetricIsCompletionCondition;

	public List<ConditionBlockRef> MetricFormulas;

	public List<Condition> StartConditions;

	public List<Condition> CompletionConditions;

	public List<Condition> FailureConditions;

	public List<StoryEvent> StartEvents;

	public List<StoryEvent> CompletionEvents;

	public List<StoryEvent> FailureEvents;

	public List<TriggerRef> Triggers;

	public override bool MatchText(string txt)
	{
		if (!base.MatchText(txt))
		{
			return NativeDescription.ToLower().Contains(txt);
		}
		return true;
	}

	public override void OnUniqueIDChanged()
	{
		DescriptionHash = StringUtil.JenkinsHash(GetDescriptionKey());
	}

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		OnUniqueIDChanged();
		story.EnglishTranslation.Keys[DescriptionHash] = NativeDescription;
		if (Params != null)
		{
			for (int i = 0; i < Params.Count; i++)
			{
				Params[i] = Params[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (QuestMarkers != null)
		{
			for (int j = 0; j < QuestMarkers.Count; j++)
			{
				QuestMarkers[j] = QuestMarkers[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (StartConditions != null)
		{
			for (int k = 0; k < StartConditions.Count; k++)
			{
				StartConditions[k] = StartConditions[k].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (CompletionConditions != null)
		{
			for (int l = 0; l < CompletionConditions.Count; l++)
			{
				CompletionConditions[l] = CompletionConditions[l].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (FailureConditions != null)
		{
			for (int m = 0; m < FailureConditions.Count; m++)
			{
				FailureConditions[m] = FailureConditions[m].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (QuestMarkerConditions != null)
		{
			for (int n = 0; n < QuestMarkerConditions.Count; n++)
			{
				QuestMarkerConditions[n] = QuestMarkerConditions[n].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (StartEvents != null)
		{
			for (int num = 0; num < StartEvents.Count; num++)
			{
				StartEvents[num] = StartEvents[num].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (CompletionEvents != null)
		{
			for (int num2 = 0; num2 < CompletionEvents.Count; num2++)
			{
				CompletionEvents[num2] = CompletionEvents[num2].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (FailureEvents != null)
		{
			for (int num3 = 0; num3 < FailureEvents.Count; num3++)
			{
				FailureEvents[num3] = FailureEvents[num3].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Triggers != null)
		{
			for (int num4 = 0; num4 < Triggers.Count; num4++)
			{
				Triggers[num4] = Triggers[num4].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (MetricFormulas != null)
		{
			for (int num5 = 0; num5 < MetricFormulas.Count; num5++)
			{
				MetricFormulas[num5] = MetricFormulas[num5].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Params != null && Params.Count == 0)
		{
			Params = null;
		}
		if (QuestMarkers != null && QuestMarkers.Count == 0)
		{
			QuestMarkers = null;
		}
		if (StartConditions != null && StartConditions.Count == 0)
		{
			StartConditions = null;
		}
		if (CompletionConditions != null && CompletionConditions.Count == 0)
		{
			CompletionConditions = null;
		}
		if (FailureConditions != null && FailureConditions.Count == 0)
		{
			FailureConditions = null;
		}
		if (QuestMarkerConditions != null && QuestMarkerConditions.Count == 0)
		{
			QuestMarkerConditions = null;
		}
		if (StartEvents != null && StartEvents.Count == 0)
		{
			StartEvents = null;
		}
		if (CompletionEvents != null && CompletionEvents.Count == 0)
		{
			CompletionEvents = null;
		}
		if (FailureEvents != null && FailureEvents.Count == 0)
		{
			FailureEvents = null;
		}
		if (Triggers != null && Triggers.Count == 0)
		{
			Triggers = null;
		}
		if (MetricFormulas != null && MetricFormulas.Count == 0)
		{
			MetricFormulas = null;
		}
	}

	public void SaveTranslatedText(Story story)
	{
		story.ForeignTranslation.Keys[DescriptionHash] = TranslatedDescription;
		TranslatedDescription = null;
	}

	public void LoadTranslatedText(Story story)
	{
		TranslatedDescription = ((story.ForeignTranslation != null) ? story.ForeignTranslation.Translate(DescriptionHash) : null);
	}

	public string GetDescriptionKey()
	{
		return "QUEST_" + UniqueID;
	}

	public int FindTrigger(Trigger trigger)
	{
		if (Triggers != null)
		{
			for (int i = 0; i < Triggers.Count; i++)
			{
				if (Triggers[i].GetTrigger() == trigger)
				{
					return i;
				}
			}
		}
		return -1;
	}
}
