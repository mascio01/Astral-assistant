using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public class Invader : BaseScriptObject
{
	[AlwaysVisible]
	[XmlAttribute]
	public string NativeDescription;

	[AlwaysVisible]
	[TranslatedTextField]
	[DefaultValue(null)]
	public string TranslatedDescription;

	[XmlIgnore]
	public int DescriptionHash;

	[XmlAttribute]
	public string IconPath;

	[XmlIgnore]
	public Resource<Texture2D> IconResource;

	[XmlAttribute]
	public string TemplateID;

	[XmlAttribute]
	[DefaultValue(100f)]
	public float Density = 100f;

	[XmlAttribute]
	[DefaultValue(DifficultySetting.None)]
	public DifficultySetting DifficultySetting;

	[XmlAttribute]
	[DefaultValue(-1)]
	public int MaxActiveOnLargeMap = -1;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float MinCooldownDays;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float MaxCooldownDays;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float MinTimeoutDays;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float MaxTimeoutDays;

	public List<SpeechParam> Params;

	public List<Condition> StartConditions;

	public List<StoryEvent> OnSpawnedEvents;

	public List<StoryEvent> OnKilledEvents;

	public List<StoryEvent> OnDeletedEvents;

	public static int HUD_DayRemaining = StringUtil.JenkinsHash("HUD_DayRemaining");

	public static int HUD_DaysRemaining = StringUtil.JenkinsHash("HUD_DaysRemaining");

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
		if (StartConditions != null)
		{
			for (int j = 0; j < StartConditions.Count; j++)
			{
				StartConditions[j] = StartConditions[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (OnSpawnedEvents != null)
		{
			for (int k = 0; k < OnSpawnedEvents.Count; k++)
			{
				OnSpawnedEvents[k] = OnSpawnedEvents[k].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (OnKilledEvents != null)
		{
			for (int l = 0; l < OnKilledEvents.Count; l++)
			{
				OnKilledEvents[l] = OnKilledEvents[l].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (OnDeletedEvents != null)
		{
			for (int m = 0; m < OnDeletedEvents.Count; m++)
			{
				OnDeletedEvents[m] = OnDeletedEvents[m].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Params != null && Params.Count == 0)
		{
			Params = null;
		}
		if (StartConditions != null && StartConditions.Count == 0)
		{
			StartConditions = null;
		}
		if (OnSpawnedEvents != null && OnSpawnedEvents.Count == 0)
		{
			OnSpawnedEvents = null;
		}
		if (OnKilledEvents != null && OnKilledEvents.Count == 0)
		{
			OnKilledEvents = null;
		}
		if (OnDeletedEvents != null && OnDeletedEvents.Count == 0)
		{
			OnDeletedEvents = null;
		}
		if (!string.IsNullOrEmpty(IconPath))
		{
			GameImpl instance = GameImpl.Instance;
			int num = instance.CurrentStories.Count - 1;
			while (num >= 0 && IconResource == null)
			{
				IconResource = Resource<Texture2D>.CreateIfFileExists(instance.CurrentStories[num].Path + "/InvaderIcons/" + IconPath);
				num--;
			}
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

	public string GetDescriptionString(BaseObject sourceObject, InvaderInstance invaderInstance)
	{
		List<SpeechParamResult> paramResults = new List<SpeechParamResult>();
		List<SpeechEmoticon> emoticons = new List<SpeechEmoticon>();
		Speech.EvaluateSpeechParams(Params, null, null, sourceObject, paramResults, default(MemoryParam), null, null, MathUtil.NonDeterministicRand, invaderInstance, UniqueID);
		Speech.BuildSpeechText(DescriptionHash, Params, null, null, sourceObject, paramResults, out var speechText, emoticons, englishOnly: false, isQuest: true);
		if (MaxTimeoutDays > 0f)
		{
			int num = Mathf.CeilToInt(Math.Max(1f, (float)(invaderInstance.CalcTimeout() + invaderInstance.TriggeredTime - Session.Instance.PlayTime).TotalSeconds / Sun.DayLengthSecs));
			return speechText + " " + GameImpl.Translate((num == 1) ? HUD_DayRemaining : HUD_DaysRemaining).Replace("%1", num.ToString());
		}
		return speechText;
	}

	public override bool MatchText(string txt)
	{
		if (!base.MatchText(txt))
		{
			return NativeDescription.ToLower().Contains(txt);
		}
		return true;
	}

	public float CalcDensity()
	{
		float difficultySetting = Session.Instance.DifficultySettings.GetDifficultySetting(DifficultySetting);
		return Density / 100f * difficultySetting * 100f;
	}

	public int CalcMaxActive()
	{
		GameTerrain instance = GameTerrain.Instance;
		DifficultySettings difficultySettings = Session.Instance.DifficultySettings;
		return Mathf.CeilToInt((float)Math.Max(1, Mathf.CeilToInt((float)MaxActiveOnLargeMap * ((float)(instance.Size * instance.Size) / 1048576f))) * difficultySettings.GetDifficultySetting(DifficultySetting));
	}

	public string GetDescriptionKey()
	{
		return "INVADER_" + UniqueID;
	}
}
