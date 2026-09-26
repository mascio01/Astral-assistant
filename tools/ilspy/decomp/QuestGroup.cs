using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public class QuestGroup : BaseScriptObject
{
	[AlwaysVisible]
	[XmlAttribute]
	public string NativeTitle;

	[AlwaysVisible]
	[TranslatedTextField]
	[DefaultValue(null)]
	public string TranslatedTitle;

	[XmlIgnore]
	public int TitleHash;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool MergeInstances;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool MainQuest;

	public List<SpeechParam> Params;

	public string GetTitleKey()
	{
		return "QUESTGROUP_" + UniqueID;
	}

	public override void OnUniqueIDChanged()
	{
		TitleHash = StringUtil.JenkinsHash(GetTitleKey());
	}

	public override bool MatchText(string txt)
	{
		if (!base.MatchText(txt))
		{
			return NativeTitle.ToLower().Contains(txt);
		}
		return true;
	}

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		OnUniqueIDChanged();
		story.EnglishTranslation.Keys[TitleHash] = NativeTitle;
		if (Params != null)
		{
			for (int i = 0; i < Params.Count; i++)
			{
				Params[i] = Params[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Params != null && Params.Count == 0)
		{
			Params = null;
		}
	}

	public void SaveTranslatedText(Story story)
	{
		story.ForeignTranslation.Keys[TitleHash] = TranslatedTitle;
		TranslatedTitle = null;
	}

	public void LoadTranslatedText(Story story)
	{
		TranslatedTitle = ((story.ForeignTranslation != null) ? story.ForeignTranslation.Translate(TitleHash) : null);
	}

	public string GetTitleString(Character questGiver, Character questSeeker, BaseObject questObject, QuestInstance topQuest)
	{
		List<SpeechParamResult> paramResults = new List<SpeechParamResult>();
		List<SpeechEmoticon> emoticons = new List<SpeechEmoticon>();
		Speech.EvaluateSpeechParams(Params, questGiver, questSeeker, questObject, paramResults, default(MemoryParam), null, null, MathUtil.NonDeterministicRand, topQuest, UniqueID);
		Speech.BuildSpeechText(TitleHash, Params, questGiver, questSeeker, questObject, paramResults, out var speechText, emoticons, englishOnly: false, isQuest: true);
		return speechText;
	}
}
