using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class Speech : BaseScriptObject
{
	[AlwaysVisible]
	[XmlAttribute]
	public string NativeText;

	[AlwaysVisible]
	[TranslatedTextField]
	[DefaultValue(null)]
	public string TranslatedText;

	[XmlIgnore]
	public int TextHash;

	[XmlAttribute]
	[DefaultValue(SpeechSituation.None)]
	public SpeechSituation Situation;

	[XmlAttribute]
	[DefaultValue(Importance.Normal)]
	public Importance Importance = Importance.Normal;

	[XmlAttribute]
	[DefaultValue(ActionAnim.None)]
	public ActionAnim Anim;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float RepeatTime;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float MaxRange;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool MustFinish;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool CanTalkOutOfRange;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool Run;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool StandUp;

	[XmlAttribute]
	[DefaultValue(SpecialSpeechBehaviour.None)]
	public SpecialSpeechBehaviour SpecialBehaviour;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool Once;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool SuccessfulResponse;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool Lie;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool AIOverridesControl;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool AIOverridesListenerControl;

	[XmlAttribute]
	[DefaultValue(SpeechPerObjectType.None)]
	public SpeechPerObjectType PerObjectType;

	[XmlAttribute]
	[DefaultValue(RelationshipType.None)]
	public RelationshipType PerRelationship;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool IncludeListener;

	[XmlAttribute]
	[DefaultValue(null)]
	public string PerMemory;

	[XmlAttribute]
	[DefaultValue(null)]
	public string PerStringData;

	public List<SpeechParam> Params;

	public List<Condition> PriorityTerms;

	public List<SpeechRef> Replies;

	public List<SpeechRef> Continues;

	public List<SpeechRef> NoReply;

	public List<Condition> Conditions;

	public List<Condition> CancelConditions;

	public List<StoryEvent> Events;

	public List<StoryEvent> UninterruptibleEvents;

	public List<SpeechRef> ExtraReplyTo;

	public static int SPEECH_Man = StringUtil.JenkinsHash("SPEECH_Man");

	public static int SPEECH_Woman = StringUtil.JenkinsHash("SPEECH_Woman");

	public static int SPEECH_Boy = StringUtil.JenkinsHash("SPEECH_Boy");

	public static int SPEECH_Guy = StringUtil.JenkinsHash("SPEECH_Guy");

	public static int SPEECH_Girl = StringUtil.JenkinsHash("SPEECH_Girl");

	public static int SPEECH_Chick = StringUtil.JenkinsHash("SPEECH_Chick");

	public static int SPEECH_Bastard = StringUtil.JenkinsHash("SPEECH_Bastard");

	public static int SPEECH_Bitch = StringUtil.JenkinsHash("SPEECH_Bitch");

	public static int SPEECH_Asshole = StringUtil.JenkinsHash("SPEECH_Asshole");

	public static int SPEECH_Sir = StringUtil.JenkinsHash("SPEECH_Sir");

	public static int SPEECH_Madam = StringUtil.JenkinsHash("SPEECH_Madam");

	public static int SPEECH_Me = StringUtil.JenkinsHash("SPEECH_Me");

	public static int SPEECH_I = StringUtil.JenkinsHash("SPEECH_I");

	public static int SPEECH_We = StringUtil.JenkinsHash("SPEECH_We");

	public static int SPEECH_Us = StringUtil.JenkinsHash("SPEECH_Us");

	public static int SPEECH_Myself = StringUtil.JenkinsHash("SPEECH_Myself");

	public static int SPEECH_Myself_Female = StringUtil.JenkinsHash("SPEECH_Myself_Female");

	public static int SPEECH_Yourself = StringUtil.JenkinsHash("SPEECH_Yourself");

	public static int SPEECH_Yourself_Female = StringUtil.JenkinsHash("SPEECH_Yourself_Female");

	public static int SPEECH_Themselves = StringUtil.JenkinsHash("SPEECH_Themselves");

	public static int SPEECH_Himself = StringUtil.JenkinsHash("SPEECH_Himself");

	public static int SPEECH_Herself = StringUtil.JenkinsHash("SPEECH_Herself");

	public static int SPEECH_Them = StringUtil.JenkinsHash("SPEECH_Them");

	public static int SPEECH_Him = StringUtil.JenkinsHash("SPEECH_Him");

	public static int SPEECH_Their = StringUtil.JenkinsHash("SPEECH_Their");

	public static int SPEECH_His = StringUtil.JenkinsHash("SPEECH_His");

	public static int SPEECH_Her = StringUtil.JenkinsHash("SPEECH_Her");

	public static int SPEECH_Her_Possessive = StringUtil.JenkinsHash("SPEECH_Her_Possessive");

	public static int SPEECH_They = StringUtil.JenkinsHash("SPEECH_They");

	public static int SPEECH_He = StringUtil.JenkinsHash("SPEECH_He");

	public static int SPEECH_She = StringUtil.JenkinsHash("SPEECH_She");

	public static int SPEECH_My = StringUtil.JenkinsHash("SPEECH_My");

	public static int SPEECH_Your = StringUtil.JenkinsHash("SPEECH_Your");

	public static int SPEECH_Our = StringUtil.JenkinsHash("SPEECH_Our");

	public static int SPEECH_Possessive = StringUtil.JenkinsHash("SPEECH_Possessive");

	public static int SPEECH_A = StringUtil.JenkinsHash("SPEECH_A");

	public static int SPEECH_An = StringUtil.JenkinsHash("SPEECH_An");

	public static int SPEECH_IndefiniteArticleFeminine = StringUtil.JenkinsHash("SPEECH_IndefiniteArticleFeminine");

	public static int SPEECH_Some = StringUtil.JenkinsHash("SPEECH_Some");

	public static int SPEECH_Some_PluralMasculine = StringUtil.JenkinsHash("SPEECH_Some_PluralMasculine");

	public static int SPEECH_Some_PluralFeminine = StringUtil.JenkinsHash("SPEECH_Some_PluralFeminine");

	public static int SPEECH_Bro = StringUtil.JenkinsHash("SPEECH_Bro");

	public static int SPEECH_Sis = StringUtil.JenkinsHash("SPEECH_Sis");

	public static int SPEECH_Brother = StringUtil.JenkinsHash("SPEECH_Brother");

	public static int SPEECH_Sister = StringUtil.JenkinsHash("SPEECH_Sister");

	public static int SPEECH_Sibling = StringUtil.JenkinsHash("SPEECH_Sibling");

	public static int SPEECH_Friend_Male = StringUtil.JenkinsHash("SPEECH_Friend_Male");

	public static int SPEECH_Friend_Female = StringUtil.JenkinsHash("SPEECH_Friend_Female");

	public static int SPEECH_Ex_Male = StringUtil.JenkinsHash("SPEECH_Ex_Male");

	public static int SPEECH_Ex_Female = StringUtil.JenkinsHash("SPEECH_Ex_Female");

	public static int SPEECH_Crush_Male = StringUtil.JenkinsHash("SPEECH_Crush_Male");

	public static int SPEECH_Crush_Female = StringUtil.JenkinsHash("SPEECH_Crush_Female");

	public static int SPEECH_Stalker_Male = StringUtil.JenkinsHash("SPEECH_Stalker_Male");

	public static int SPEECH_Stalker_Female = StringUtil.JenkinsHash("SPEECH_Stalker_Female");

	public static int SPEECH_InLoveWith = StringUtil.JenkinsHash("SPEECH_InLoveWith");

	public static int SPEECH_InLoveWith_Female = StringUtil.JenkinsHash("SPEECH_InLoveWith_Female");

	public static int SPEECH_NotInLoveWith = StringUtil.JenkinsHash("SPEECH_NotInLoveWith");

	public static int SPEECH_NotInLoveWith_Female = StringUtil.JenkinsHash("SPEECH_NotInLoveWith_Female");

	public static int SPEECH_You = StringUtil.JenkinsHash("SPEECH_You");

	public static int SPEECH_Someone = StringUtil.JenkinsHash("SPEECH_Someone");

	public static int SPEECH_Someone_Female = StringUtil.JenkinsHash("SPEECH_Someone_Female");

	public static int SPEECH_Someone_Dat = StringUtil.JenkinsHash("SPEECH_Someone_Dat");

	public static int SPEECH_Someone_Female_Dat = StringUtil.JenkinsHash("SPEECH_Someone_Female_Dat");

	public static int SPEECH_Someone_Pos = StringUtil.JenkinsHash("SPEECH_Someone_Pos");

	public static int SPEECH_Someone_Female_Pos = StringUtil.JenkinsHash("SPEECH_Someone_Female_Pos");

	public static int SPEECH_SomePeople = StringUtil.JenkinsHash("SPEECH_SomePeople");

	public static int SPEECH_Something = StringUtil.JenkinsHash("SPEECH_Something");

	public static int SPEECH_Somewhere = StringUtil.JenkinsHash("SPEECH_Somewhere");

	public static int SPEECH_Person = StringUtil.JenkinsHash("SPEECH_Person");

	public static int SPEECH_Husband = StringUtil.JenkinsHash("SPEECH_Husband");

	public static int SPEECH_Wife = StringUtil.JenkinsHash("SPEECH_Wife");

	public static int SPEECH_Boyfriend = StringUtil.JenkinsHash("SPEECH_Boyfriend");

	public static int SPEECH_Girlfriend = StringUtil.JenkinsHash("SPEECH_Girlfriend");

	public static int SPEECH_Partner = StringUtil.JenkinsHash("SPEECH_Partner");

	public static int SPEECH_Spouse = StringUtil.JenkinsHash("SPEECH_Spouse");

	public static int SPEECH_Mom = StringUtil.JenkinsHash("SPEECH_Mom");

	public static int SPEECH_Dad = StringUtil.JenkinsHash("SPEECH_Dad");

	public static int SPEECH_Mother = StringUtil.JenkinsHash("SPEECH_Mother");

	public static int SPEECH_Father = StringUtil.JenkinsHash("SPEECH_Father");

	public static int SPEECH_Parent = StringUtil.JenkinsHash("SPEECH_Parent");

	public static int SPEECH_Son = StringUtil.JenkinsHash("SPEECH_Son");

	public static int SPEECH_Daughter = StringUtil.JenkinsHash("SPEECH_Daughter");

	public static int SPEECH_Kid = StringUtil.JenkinsHash("SPEECH_Kid");

	public static int SPEECH_Mr = StringUtil.JenkinsHash("SPEECH_Mr");

	public static int SPEECH_Ms = StringUtil.JenkinsHash("SPEECH_Ms");

	private static StringBuilder _stringBuilder = new StringBuilder(200);

	private static List<FacialExpression> TempEmoticons = new List<FacialExpression>();

	public string GetTextKey()
	{
		return "SPEECH_" + UniqueID;
	}

	public override void OnUniqueIDChanged()
	{
		TextHash = StringUtil.JenkinsHash(GetTextKey());
	}

	public override bool MatchText(string txt)
	{
		if (!base.MatchText(txt))
		{
			if (NativeText != null)
			{
				return NativeText.ToLower().Contains(txt);
			}
			return false;
		}
		return true;
	}

	public bool ListenerCanBeDead()
	{
		SpeechSituation situation = Situation;
		if (situation == SpeechSituation.Eulogy || situation == SpeechSituation.VisitGrave || situation == SpeechSituation.PayRespects)
		{
			return true;
		}
		return SpecialBehaviour == SpecialSpeechBehaviour.ListenerCanBeDead;
	}

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		OnUniqueIDChanged();
		story.EnglishTranslation.Keys[TextHash] = NativeText;
		if (Params != null)
		{
			for (int i = 0; i < Params.Count; i++)
			{
				Params[i] = Params[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (PriorityTerms != null)
		{
			for (int j = 0; j < PriorityTerms.Count; j++)
			{
				PriorityTerms[j] = PriorityTerms[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Conditions != null)
		{
			for (int k = 0; k < Conditions.Count; k++)
			{
				Conditions[k] = Conditions[k].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (CancelConditions != null)
		{
			for (int l = 0; l < CancelConditions.Count; l++)
			{
				CancelConditions[l] = CancelConditions[l].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Events != null)
		{
			for (int m = 0; m < Events.Count; m++)
			{
				Events[m] = Events[m].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (UninterruptibleEvents != null)
		{
			for (int n = 0; n < UninterruptibleEvents.Count; n++)
			{
				UninterruptibleEvents[n] = UninterruptibleEvents[n].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Replies != null)
		{
			for (int num = 0; num < Replies.Count; num++)
			{
				Replies[num] = Replies[num].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Continues != null)
		{
			for (int num2 = 0; num2 < Continues.Count; num2++)
			{
				Continues[num2] = Continues[num2].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (NoReply != null)
		{
			for (int num3 = 0; num3 < NoReply.Count; num3++)
			{
				NoReply[num3] = NoReply[num3].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (ExtraReplyTo != null)
		{
			for (int num4 = 0; num4 < ExtraReplyTo.Count; num4++)
			{
				ExtraReplyTo[num4] = ExtraReplyTo[num4].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Params != null && Params.Count == 0)
		{
			Params = null;
		}
		if (PriorityTerms != null && PriorityTerms.Count == 0)
		{
			PriorityTerms = null;
		}
		if (Replies != null && Replies.Count == 0)
		{
			Replies = null;
		}
		if (Continues != null && Continues.Count == 0)
		{
			Continues = null;
		}
		if (NoReply != null && NoReply.Count == 0)
		{
			NoReply = null;
		}
		if (Conditions != null && Conditions.Count == 0)
		{
			Conditions = null;
		}
		if (CancelConditions != null && CancelConditions.Count == 0)
		{
			CancelConditions = null;
		}
		if (Events != null && Events.Count == 0)
		{
			Events = null;
		}
		if (UninterruptibleEvents != null && UninterruptibleEvents.Count == 0)
		{
			UninterruptibleEvents = null;
		}
		if (ExtraReplyTo != null && ExtraReplyTo.Count == 0)
		{
			ExtraReplyTo = null;
		}
	}

	public void SaveTranslatedText(Story story)
	{
		story.ForeignTranslation.Keys[TextHash] = TranslatedText;
		TranslatedText = null;
	}

	public void LoadTranslatedText(Story story)
	{
		TranslatedText = ((story.ForeignTranslation != null) ? story.ForeignTranslation.Translate(TextHash) : null);
	}

	private static int FindParamPos(StringBuilder stringBuilder, int paramIndex)
	{
		for (int i = 0; i < stringBuilder.Length - 1; i++)
		{
			if (stringBuilder[i] == '%' && stringBuilder[i + 1] == 49 + paramIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public static void EvaluateSpeechParams(List<SpeechParam> Params, Character speaker, Character listener, BaseObject obj, List<SpeechParamResult> paramResults, MemoryParam param, string chatSpeech, string chatSpeaker, CustomRandom rand, object caller, string uniqueID)
	{
		bool flag = caller != null;
		paramResults.Clear();
		if (Params == null)
		{
			return;
		}
		for (int i = 0; i < Params.Count; i++)
		{
			SpeechParamResult item = default(SpeechParamResult);
			switch (Params[i].Type)
			{
			case SpeechParamType.FirstName:
			{
				Character subject31 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject31 != null && (subject31.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(subject31)))
				{
					item.Str1 = subject31.FirstName;
					item.Obj = subject31;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
					break;
				}
				Community subject32 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
				if (subject32 != null)
				{
					item.Obj = subject32;
					break;
				}
				item.Hash = SPEECH_Someone;
				item.Obj = subject31;
				break;
			}
			case SpeechParamType.FullName:
			{
				Character subject16 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject16 != null && (subject16.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(subject16)))
				{
					item.Str1 = subject16.FirstName;
					item.Str2 = subject16.Surname;
					item.Obj = subject16;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
					break;
				}
				Community subject17 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
				if (subject17 != null)
				{
					item.Obj = subject17;
					break;
				}
				item.Hash = SPEECH_Someone;
				item.Obj = subject16;
				break;
			}
			case SpeechParamType.Surname:
			{
				Character subject54 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject54 != null && (subject54.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(subject54)))
				{
					item.Str1 = subject54.Surname;
					item.Obj = subject54;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
					break;
				}
				Community subject55 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
				if (subject55 != null)
				{
					item.Obj = subject55;
					break;
				}
				item.Hash = SPEECH_Someone;
				item.Obj = subject54;
				break;
			}
			case SpeechParamType.GuyOrChick:
			{
				Character subject52 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject52 != null)
				{
					item.Hash = ((subject52.Appearance.Gender == GenderType.Male) ? SPEECH_Guy : SPEECH_Chick);
					item.Obj = subject52;
				}
				else
				{
					item.Hash = SPEECH_Person;
				}
				break;
			}
			case SpeechParamType.BastardOrBitch:
			{
				Character subject12 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject12 != null)
				{
					item.Hash = ((subject12.Appearance.Gender == GenderType.Male) ? SPEECH_Bastard : SPEECH_Bitch);
					item.Obj = subject12;
				}
				else
				{
					item.Hash = SPEECH_Bastard;
				}
				break;
			}
			case SpeechParamType.AssholeOrBitch:
			{
				Character subject27 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject27 != null)
				{
					item.Hash = ((subject27.Appearance.Gender == GenderType.Male) ? SPEECH_Asshole : SPEECH_Bitch);
					item.Obj = subject27;
				}
				else
				{
					item.Hash = SPEECH_Asshole;
				}
				item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				break;
			}
			case SpeechParamType.ManOrWoman:
			{
				Character subject26 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject26 != null)
				{
					item.Hash = ((subject26.Appearance.Gender == GenderType.Male) ? SPEECH_Man : SPEECH_Woman);
					item.Obj = subject26;
				}
				else
				{
					item.Hash = SPEECH_Person;
				}
				break;
			}
			case SpeechParamType.SirOrMadam:
			{
				Character subject29 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject29 != null)
				{
					item.Hash = ((subject29.Appearance.Gender == GenderType.Male) ? SPEECH_Sir : SPEECH_Madam);
					item.Obj = subject29;
				}
				else
				{
					item.Hash = SPEECH_Sir;
				}
				break;
			}
			case SpeechParamType.MrOrMs:
			{
				Character subject4 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject4 != null)
				{
					item.Hash = ((subject4.Appearance.Gender == GenderType.Male) ? SPEECH_Mr : SPEECH_Ms);
					item.Obj = subject4;
				}
				else
				{
					item.Hash = SPEECH_Mr;
				}
				break;
			}
			case SpeechParamType.HimOrHer:
			{
				Character subject42 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject42 != null)
				{
					item.Hash = ((subject42 == speaker && !flag) ? SPEECH_Me : ((subject42.Appearance.Gender == GenderType.Male) ? SPEECH_Him : SPEECH_Her));
					item.Obj = subject42;
				}
				else
				{
					item.Hash = SPEECH_Them;
				}
				break;
			}
			case SpeechParamType.HimOrHerOrYou:
			{
				Character subject21 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject21 != null)
				{
					item.Hash = ((subject21 == speaker && !flag) ? SPEECH_Me : ((subject21 == listener && !flag) ? SPEECH_You : ((subject21.Appearance.Gender == GenderType.Male) ? SPEECH_Him : SPEECH_Her)));
					item.Obj = subject21;
				}
				else
				{
					item.Hash = SPEECH_Them;
				}
				break;
			}
			case SpeechParamType.HeOrShe:
			{
				Character subject9 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject9 != null)
				{
					item.Hash = ((subject9 == speaker && !flag) ? SPEECH_I : ((subject9.Appearance.Gender == GenderType.Male) ? SPEECH_He : SPEECH_She));
					item.Obj = subject9;
				}
				else
				{
					item.Hash = SPEECH_They;
				}
				break;
			}
			case SpeechParamType.HeOrSheOrYou:
			{
				Character subject7 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject7 != null)
				{
					item.Hash = ((subject7 == speaker && !flag) ? SPEECH_I : ((subject7 == listener && !flag) ? SPEECH_You : ((subject7.Appearance.Gender == GenderType.Male) ? SPEECH_He : SPEECH_She)));
					item.Obj = subject7;
				}
				else
				{
					item.Hash = SPEECH_They;
				}
				break;
			}
			case SpeechParamType.HimOrHerself:
			{
				Character subject47 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject47 != null)
				{
					item.Hash = ((subject47 == speaker && !flag) ? SPEECH_Myself : ((subject47.Appearance.Gender == GenderType.Male) ? SPEECH_Himself : SPEECH_Herself));
					item.Obj = subject47;
				}
				else
				{
					item.Hash = SPEECH_Themselves;
				}
				break;
			}
			case SpeechParamType.HimOrHerOrYourself:
			{
				Character subject49 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject49 != null)
				{
					item.Hash = ((subject49 == speaker && !flag) ? SPEECH_Myself : ((subject49 == listener && !flag) ? SPEECH_Yourself : ((subject49.Appearance.Gender == GenderType.Male) ? SPEECH_Himself : SPEECH_Herself)));
					item.Obj = subject49;
				}
				else
				{
					item.Hash = SPEECH_Themselves;
				}
				break;
			}
			case SpeechParamType.HisOrHer:
			{
				Character subject53 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject53 != null)
				{
					item.Hash = ((subject53 == speaker && !flag) ? SPEECH_My : ((subject53.Appearance.Gender == GenderType.Male) ? SPEECH_His : SPEECH_Her_Possessive));
					item.Obj = subject53;
				}
				else
				{
					item.Hash = SPEECH_Their;
				}
				break;
			}
			case SpeechParamType.HisOrHerOrYour:
			{
				Character subject35 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject35 != null)
				{
					item.Hash = ((subject35 == speaker && !flag) ? SPEECH_My : ((subject35 == listener && !flag) ? SPEECH_Your : ((subject35.Appearance.Gender == GenderType.Male) ? SPEECH_His : SPEECH_Her_Possessive)));
					item.Obj = subject35;
				}
				else
				{
					item.Hash = SPEECH_Their;
				}
				break;
			}
			case SpeechParamType.FirstNamePossessive:
			{
				Character character5 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character5 != null && (character5.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character5) || character5 == listener))
				{
					if (character5 == speaker && !flag)
					{
						item.Hash = SPEECH_My;
						break;
					}
					if (character5 == listener && !flag)
					{
						item.Hash = SPEECH_Your;
						break;
					}
					item.Str1 = character5.FirstName;
					item.Modifier = SpeechParamModifier.Possessive;
					break;
				}
				Community subject25 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
				if (subject25 != null)
				{
					if (speaker != null && subject25 == speaker.Community && !flag)
					{
						item.Hash = SPEECH_Our;
						break;
					}
					if (listener != null && subject25 == listener.Community && !flag)
					{
						item.Hash = SPEECH_Your;
						break;
					}
					item.Obj = subject25;
					item.Modifier = SpeechParamModifier.Possessive;
				}
				else
				{
					item.Hash = SPEECH_Someone;
					item.Modifier = SpeechParamModifier.Possessive;
					item.Obj = character5;
				}
				break;
			}
			case SpeechParamType.FirstNameActive:
			{
				Character character13 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character13 != null && (character13.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character13) || character13 == listener))
				{
					if (character13 == speaker && !flag)
					{
						item.Hash = SPEECH_I;
						break;
					}
					if (character13 == listener && !flag)
					{
						item.Hash = SPEECH_You;
						break;
					}
					item.Str1 = character13.FirstName;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					Community subject56 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
					if (subject56 != null)
					{
						item.Obj = subject56;
						break;
					}
					item.Hash = SPEECH_Someone;
					item.Obj = character13;
				}
				break;
			}
			case SpeechParamType.FirstNameOrMeOrYou:
			{
				Character character10 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character10 != null && (character10.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character10) || character10 == listener))
				{
					if (character10 == speaker && !flag)
					{
						item.Hash = SPEECH_Me;
						break;
					}
					if (character10 == listener && !flag)
					{
						item.Hash = SPEECH_You;
						break;
					}
					item.Str1 = character10.FirstName;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					Community subject41 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
					if (subject41 != null)
					{
						item.Obj = subject41;
						break;
					}
					item.Hash = SPEECH_Someone;
					item.Obj = character10;
				}
				break;
			}
			case SpeechParamType.FullNamePossessive:
			{
				Character character9 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character9 != null && (character9.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character9) || character9 == listener))
				{
					if (character9 == speaker && !flag)
					{
						item.Hash = SPEECH_My;
						break;
					}
					if (character9 == listener && !flag)
					{
						item.Hash = SPEECH_Your;
						break;
					}
					item.Str1 = character9.FirstName;
					item.Str2 = character9.Surname;
					item.Modifier = SpeechParamModifier.Possessive;
					break;
				}
				Community subject39 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
				if (subject39 != null)
				{
					if (speaker != null && subject39 == speaker.Community && !flag)
					{
						item.Hash = SPEECH_Our;
						break;
					}
					if (listener != null && subject39 == listener.Community && !flag)
					{
						item.Hash = SPEECH_Your;
						break;
					}
					item.Obj = subject39;
					item.Modifier = SpeechParamModifier.Possessive;
				}
				else
				{
					item.Hash = SPEECH_Someone;
					item.Modifier = SpeechParamModifier.Possessive;
					item.Obj = character9;
				}
				break;
			}
			case SpeechParamType.FullNameActive:
			{
				Character character3 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character3 != null && (character3.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character3) || character3 == listener))
				{
					if (character3 == speaker && !flag)
					{
						item.Hash = SPEECH_I;
						break;
					}
					if (character3 == listener && !flag)
					{
						item.Hash = SPEECH_You;
						break;
					}
					item.Str1 = character3.FirstName;
					item.Str2 = character3.Surname;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					Community subject20 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
					if (subject20 != null)
					{
						item.Obj = subject20;
						break;
					}
					item.Hash = SPEECH_Someone;
					item.Obj = character3;
				}
				break;
			}
			case SpeechParamType.FullNameOrMeOrYou:
			{
				Character character11 = (Character)(item.Obj = Params[i].GetSubject<Character>(speaker, listener, obj, param));
				if (character11 != null && (character11.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(character11) || character11 == listener))
				{
					if (character11 == speaker && !flag)
					{
						item.Hash = SPEECH_Me;
						break;
					}
					if (character11 == listener && !flag)
					{
						item.Hash = SPEECH_You;
						break;
					}
					item.Str1 = character11.FirstName;
					item.Str2 = character11.Surname;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					Community subject44 = Params[i].GetSubject<Community>(speaker, listener, obj, param);
					if (subject44 != null)
					{
						item.Obj = subject44;
						break;
					}
					item.Hash = SPEECH_Someone;
					item.Obj = character11;
				}
				break;
			}
			case SpeechParamType.Partner:
			{
				Character subject57 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character14 = ((subject57 != null) ? Relationship.GetPartner(subject57) : null);
				if (character14 != null)
				{
					RelationshipType relationship2 = Relationship.GetRelationship(subject57, character14);
					item.Hash = ((relationship2 != RelationshipType.MarriedTo) ? ((character14.Appearance.Gender == GenderType.Male) ? SPEECH_Boyfriend : SPEECH_Girlfriend) : ((character14.Appearance.Gender == GenderType.Male) ? SPEECH_Husband : SPEECH_Wife));
					item.Obj = character14;
				}
				else
				{
					item.Hash = SPEECH_Partner;
				}
				break;
			}
			case SpeechParamType.GuyOrGirl:
			{
				Character subject40 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject40 != null)
				{
					item.Hash = ((subject40.Appearance.Gender == GenderType.Male) ? SPEECH_Guy : SPEECH_Girl);
					item.Obj = subject40;
				}
				else
				{
					item.Hash = SPEECH_Kid;
				}
				break;
			}
			case SpeechParamType.BoyOrGirl:
			{
				Character subject19 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject19 != null)
				{
					item.Hash = ((subject19.Appearance.Gender == GenderType.Male) ? SPEECH_Boy : SPEECH_Girl);
					item.Obj = subject19;
				}
				else
				{
					item.Hash = SPEECH_Person;
				}
				break;
			}
			case SpeechParamType.BroOrSis:
			{
				Character subject8 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject8 != null)
				{
					item.Hash = ((subject8.Appearance.Gender == GenderType.Male) ? SPEECH_Bro : SPEECH_Sis);
					item.Obj = subject8;
				}
				else
				{
					item.Hash = SPEECH_Sibling;
				}
				break;
			}
			case SpeechParamType.BrotherOrSister:
			{
				Character subject5 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject5 != null)
				{
					item.Hash = ((subject5.Appearance.Gender == GenderType.Male) ? SPEECH_Brother : SPEECH_Sister);
					item.Obj = subject5;
				}
				else
				{
					item.Hash = SPEECH_Sibling;
				}
				break;
			}
			case SpeechParamType.BoyfriendOrGirlfriend:
			{
				Character subject43 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject43 != null)
				{
					item.Hash = ((subject43.Appearance.Gender == GenderType.Male) ? SPEECH_Boyfriend : SPEECH_Girlfriend);
					item.Obj = subject43;
				}
				else
				{
					item.Hash = SPEECH_Partner;
				}
				break;
			}
			case SpeechParamType.HusbandOrWife:
			{
				Character subject = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject != null)
				{
					item.Hash = ((subject.Appearance.Gender == GenderType.Male) ? SPEECH_Husband : SPEECH_Wife);
					item.Obj = subject;
				}
				else
				{
					item.Hash = SPEECH_Spouse;
				}
				break;
			}
			case SpeechParamType.MomOrDad:
			{
				Character subject23 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject23 != null)
				{
					item.Hash = ((subject23.Appearance.Gender == GenderType.Male) ? SPEECH_Dad : SPEECH_Mom);
					item.Obj = subject23;
				}
				else
				{
					item.Hash = SPEECH_Parent;
				}
				break;
			}
			case SpeechParamType.MotherOrFather:
			{
				Character subject24 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject24 != null)
				{
					item.Hash = ((subject24.Appearance.Gender == GenderType.Male) ? SPEECH_Father : SPEECH_Mother);
					item.Obj = subject24;
				}
				else
				{
					item.Hash = SPEECH_Parent;
				}
				break;
			}
			case SpeechParamType.SonOrDaughter:
			{
				Character subject13 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject13 != null)
				{
					item.Hash = ((subject13.Appearance.Gender == GenderType.Male) ? SPEECH_Son : SPEECH_Daughter);
					item.Obj = subject13;
				}
				else
				{
					item.Hash = SPEECH_Kid;
				}
				break;
			}
			case SpeechParamType.RelationshipName:
			{
				Character subject3 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject3 != null && character != null)
				{
					RelationshipType relationship = Relationship.GetRelationship(subject3, character);
					item.Hash = Relationship.GetRelationshipNameHash(relationship, character.Appearance.Gender);
					item.Obj = character;
				}
				else
				{
					item.Hash = SPEECH_Kid;
				}
				break;
			}
			case SpeechParamType.NumGoodRelationshipsInCommunity:
			{
				Character subject50 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Community objectCommunity4 = Params[i].GetObjectCommunity(speaker, listener, obj, param);
				int num4 = 0;
				if (subject50 != null && objectCommunity4 != null)
				{
					num4 = subject50.GetNumGoodRelationshipsInCommunity(recurse: true, objectCommunity4);
				}
				item.NumericVal = num4;
				break;
			}
			case SpeechParamType.PriceToBandage:
			{
				Character subject45 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character12 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject45 != null)
				{
					item.NumericVal = subject45.GetPriceToBandage(character12);
				}
				else
				{
					item.NumericVal = EquipmentPrototype.Bandage.BasePrice;
				}
				break;
			}
			case SpeechParamType.PriceToSellItem:
			{
				Character subject38 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character8 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.FindEquipmentPrototypeByName(Params[i].StringData);
				if (subject38 != null && character8 != null && equipmentPrototype2 != null)
				{
					Equipment equipment2 = subject38.Inventory.FindItemOfType(equipmentPrototype2);
					if (equipment2 != null)
					{
						item.NumericVal = Math.Max(1, Mathf.CeilToInt(subject38.GetPriceToSell(equipment2, character8)));
					}
					else
					{
						item.NumericVal = Math.Max(1, Mathf.CeilToInt(subject38.GetPriceToSell(equipmentPrototype2, character8)));
					}
				}
				else if (equipmentPrototype2 != null)
				{
					item.NumericVal = equipmentPrototype2.BasePrice;
				}
				break;
			}
			case SpeechParamType.PriceToSellFood:
			{
				Character subject28 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character6 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject28 != null && character6 != null)
				{
					Equipment food = subject28.Inventory.GetFood(subject28, character6, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: true);
					if (food != null)
					{
						item.NumericVal = Math.Max(1, Mathf.CeilToInt(subject28.GetPriceToSell(food, character6)));
					}
				}
				break;
			}
			case SpeechParamType.PriceToSellWater:
			{
				Character subject33 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character7 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject33 != null && character7 != null)
				{
					Equipment bestWaterBottleToDrink = subject33.Inventory.GetBestWaterBottleToDrink(subject33, character7, forSharing: true);
					if (bestWaterBottleToDrink != null)
					{
						item.NumericVal = Math.Max(1, Mathf.CeilToInt(subject33.GetPriceToSell(bestWaterBottleToDrink, character7)));
					}
				}
				break;
			}
			case SpeechParamType.PriceToSell:
			{
				Character subject22 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character4 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject22 != null && character4 != null)
				{
					float num2 = Params[i].Data;
					if (Params[i].Formulas != null && Params[i].Formulas.Count > 0 && Params[i].Formulas[0].GetConditionBlock() != null)
					{
						num2 += Params[i].Formulas[0].GetConditionBlock().Evaluate(subject22, character4, obj, param);
					}
					item.NumericVal = Math.Max(1, Mathf.CeilToInt(subject22.GetPriceToSell(num2, character4)));
				}
				break;
			}
			case SpeechParamType.PriceToBuy:
			{
				Character subject10 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Character character2 = Params[i].GetObject<Character>(speaker, listener, obj, param);
				if (subject10 != null && character2 != null)
				{
					float num = Params[i].Data;
					if (Params[i].Formulas != null && Params[i].Formulas.Count > 0 && Params[i].Formulas[0].GetConditionBlock() != null)
					{
						num += Params[i].Formulas[0].GetConditionBlock().Evaluate(character2, subject10, obj, param);
					}
					item.NumericVal = Math.Max(1, Mathf.CeilToInt(character2.GetPriceToSell(num, subject10)));
				}
				break;
			}
			case SpeechParamType.Formula:
			{
				float num5 = Params[i].Data;
				if (Params[i].Formulas != null && Params[i].Formulas.Count > 0 && Params[i].Formulas[0].GetConditionBlock() != null)
				{
					num5 += Params[i].Formulas[0].GetConditionBlock().Evaluate(speaker, listener, obj, param);
				}
				item.NumericVal = num5;
				break;
			}
			case SpeechParamType.BoxerWagerAmount:
			{
				Character subject15 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject15 != null)
				{
					item.NumericVal = subject15.GetBoxerWagerAmount();
				}
				break;
			}
			case SpeechParamType.Age:
			{
				Character subject51 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject51 != null)
				{
					item.NumericVal = Mathf.Floor(subject51.Appearance.Age);
				}
				break;
			}
			case SpeechParamType.CommunityName:
			{
				Community subjectCommunity11 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity11 != null)
				{
					item.Obj = subjectCommunity11;
				}
				else
				{
					item.Hash = SPEECH_SomePeople;
				}
				break;
			}
			case SpeechParamType.CommunityNameOrI:
			{
				Community subjectCommunity14 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity14 != null && subjectCommunity14.GetLivingNonZombieMemberCount() > 1)
				{
					if (subjectCommunity14.CommunityName.IsNull())
					{
						item.Hash = SPEECH_We;
					}
					else
					{
						item.Obj = subjectCommunity14;
					}
				}
				else
				{
					item.Hash = SPEECH_I;
				}
				break;
			}
			case SpeechParamType.TownName:
			{
				BaseObject subject37 = Params[i].GetSubject<BaseObject>(speaker, listener, obj, param);
				if (subject37 is Town)
				{
					item.Obj = subject37;
				}
				else if (subject37 is Prop)
				{
					item.Obj = ((Prop)subject37).Town;
					if (item.Obj == null)
					{
						item.Obj = subject37;
					}
				}
				else
				{
					item.Hash = SPEECH_Somewhere;
				}
				break;
			}
			case SpeechParamType.BuildingName:
			{
				BaseObject subject34 = Params[i].GetSubject<BaseObject>(speaker, listener, obj, param);
				if (subject34 != null)
				{
					item.Obj = subject34;
				}
				else
				{
					item.Hash = SPEECH_Something;
				}
				break;
			}
			case SpeechParamType.NearestTownOrSettlementName:
			{
				BaseObject baseObject = Params[i].GetSubject<BaseObject>(speaker, listener, obj, param);
				if (baseObject is Equipment equipment)
				{
					baseObject = equipment.InventoryOwner;
				}
				if (baseObject is Town || baseObject is Community)
				{
					item.Obj = baseObject;
				}
				else if (baseObject is TileObject)
				{
					BaseObject closestTownOrSettlement = Session.Instance.CommunityManager.GetClosestTownOrSettlement(((TileObject)baseObject).GetTile());
					if (closestTownOrSettlement != null)
					{
						item.Obj = closestTownOrSettlement;
					}
					else
					{
						item.Hash = SPEECH_Somewhere;
					}
				}
				else
				{
					item.Hash = SPEECH_Somewhere;
				}
				break;
			}
			case SpeechParamType.CommunityLeaderNameOrYou:
			{
				Community subjectCommunity3 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity3 != null && subjectCommunity3.Leader != null)
				{
					if (subjectCommunity3.Leader == listener)
					{
						item.Hash = SPEECH_You;
						item.Obj = subjectCommunity3.Leader;
					}
					else
					{
						item.Obj = subjectCommunity3.Leader;
					}
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityLeaderName:
			{
				Community subjectCommunity = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity != null && subjectCommunity.Leader != null)
				{
					item.Obj = subjectCommunity.Leader;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityLeaderFirstName:
			{
				Community subjectCommunity15 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity15 != null && subjectCommunity15.Leader != null && (subjectCommunity15.Leader.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(subjectCommunity15.Leader)))
				{
					item.Str1 = subjectCommunity15.Leader.FirstName;
					item.Obj = subjectCommunity15.Leader;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityLeaderFullName:
			{
				Community subjectCommunity12 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity12 != null && subjectCommunity12.Leader != null && (subjectCommunity12.Leader.NameKnown || Params[i].BoolData || speaker == null || speaker.KnowsName(subjectCommunity12.Leader)))
				{
					item.Str1 = subjectCommunity12.Leader.FirstName;
					item.Str2 = subjectCommunity12.Leader.Surname;
					item.Obj = subjectCommunity12.Leader;
					item.Modifier = SpeechParamModifier.ModifyPreviousWord;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityPossessive:
			{
				Community subjectCommunity10 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity10 != null && speaker != null && subjectCommunity10 == speaker.Community && !flag)
				{
					if (subjectCommunity10.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_Our;
						item.Obj = subjectCommunity10;
					}
					else
					{
						item.Hash = SPEECH_My;
						item.Obj = speaker;
					}
				}
				else if (subjectCommunity10 != null && listener != null && subjectCommunity10 == listener.Community && !flag)
				{
					if (subjectCommunity10.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_Your;
						item.Obj = subjectCommunity10;
					}
					else
					{
						item.Hash = SPEECH_Your;
						item.Obj = listener;
					}
				}
				else if (subjectCommunity10 != null)
				{
					item.Obj = subjectCommunity10;
					item.Modifier = SpeechParamModifier.Possessive;
				}
				else
				{
					item.Hash = SPEECH_Their;
				}
				break;
			}
			case SpeechParamType.CommunityIOrWe:
			{
				Community subjectCommunity2 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity2 != null && speaker != null && subjectCommunity2 == speaker.Community && !flag)
				{
					if (subjectCommunity2.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_We;
						item.Obj = subjectCommunity2;
					}
					else
					{
						item.Hash = SPEECH_I;
						item.Obj = speaker;
					}
				}
				else if (subjectCommunity2 != null && listener != null && subjectCommunity2 == listener.Community && !flag)
				{
					if (subjectCommunity2.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_You;
						item.Obj = subjectCommunity2;
					}
					else
					{
						item.Hash = SPEECH_You;
						item.Obj = listener;
					}
				}
				else if (subjectCommunity2 != null)
				{
					item.Obj = subjectCommunity2;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityMeOrUs:
			{
				Community subjectCommunity13 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity13 != null && speaker != null && subjectCommunity13 == speaker.Community && !flag)
				{
					if (subjectCommunity13.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_Us;
						item.Obj = subjectCommunity13;
					}
					else
					{
						item.Hash = SPEECH_Me;
						item.Obj = speaker;
					}
				}
				else if (subjectCommunity13 != null && listener != null && subjectCommunity13 == listener.Community && !flag)
				{
					if (subjectCommunity13.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_You;
						item.Obj = subjectCommunity13;
					}
					else
					{
						item.Hash = SPEECH_You;
						item.Obj = listener;
					}
				}
				else if (subjectCommunity13 != null)
				{
					item.Obj = subjectCommunity13;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityMyOrOur:
			{
				Community subjectCommunity8 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity8 != null && speaker != null && subjectCommunity8 == speaker.Community && !flag)
				{
					if (subjectCommunity8.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_Our;
						item.Obj = subjectCommunity8;
					}
					else
					{
						item.Hash = SPEECH_My;
						item.Obj = speaker;
					}
				}
				else if (subjectCommunity8 != null && listener != null && subjectCommunity8 == listener.Community && !flag)
				{
					if (subjectCommunity8.GetLivingNonZombieMemberCount() > 1)
					{
						item.Hash = SPEECH_Your;
						item.Obj = subjectCommunity8;
					}
					else
					{
						item.Hash = SPEECH_Your;
						item.Obj = listener;
					}
				}
				else if (subjectCommunity8 != null)
				{
					item.Obj = subjectCommunity8;
				}
				else
				{
					item.Hash = SPEECH_Their;
				}
				break;
			}
			case SpeechParamType.SquadIOrWe:
			{
				Character subject30 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject30 != null && speaker != null && subject30 == speaker && !flag)
				{
					Squad squad3 = subject30.GetSquad();
					if (squad3 != null && squad3.Members.Count > 1)
					{
						item.Hash = SPEECH_We;
						item.Obj = subject30;
					}
					else
					{
						item.Hash = SPEECH_I;
						item.Obj = speaker;
					}
				}
				else if (subject30 != null && subject30 == listener && !flag)
				{
					Squad squad4 = subject30.GetSquad();
					if (squad4 != null && squad4.Members.Count > 1)
					{
						item.Hash = SPEECH_You;
						item.Obj = subject30;
					}
					else
					{
						item.Hash = SPEECH_You;
						item.Obj = speaker;
					}
				}
				else if (subject30 != null)
				{
					item.Obj = subject30;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.SquadMeOrUs:
			{
				Character subject14 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject14 != null && speaker != null && subject14 == speaker && !flag)
				{
					Squad squad = subject14.GetSquad();
					if (squad != null && squad.Members.Count > 1)
					{
						item.Hash = SPEECH_Us;
						item.Obj = subject14;
					}
					else
					{
						item.Hash = SPEECH_Me;
						item.Obj = speaker;
					}
				}
				else if (subject14 != null && subject14 == listener && !flag)
				{
					Squad squad2 = subject14.GetSquad();
					if (squad2 != null && squad2.Members.Count > 1)
					{
						item.Hash = SPEECH_You;
						item.Obj = subject14;
					}
					else
					{
						item.Hash = SPEECH_You;
						item.Obj = speaker;
					}
				}
				else if (subject14 != null)
				{
					item.Obj = subject14;
				}
				else
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.SquadMyOrOur:
			{
				Character subject48 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject48 != null && speaker != null && subject48 == speaker && !flag)
				{
					Squad squad5 = subject48.GetSquad();
					if (squad5 != null && squad5.Members.Count > 1)
					{
						item.Hash = SPEECH_Our;
						item.Obj = subject48;
					}
					else
					{
						item.Hash = SPEECH_My;
						item.Obj = speaker;
					}
				}
				else if (subject48 != null && subject48 == listener && !flag)
				{
					Squad squad6 = subject48.GetSquad();
					if (squad6 != null && squad6.Members.Count > 1)
					{
						item.Hash = SPEECH_Your;
						item.Obj = subject48;
					}
					else
					{
						item.Hash = SPEECH_Your;
						item.Obj = speaker;
					}
				}
				else if (subject48 != null)
				{
					item.Obj = subject48;
				}
				else
				{
					item.Hash = SPEECH_Their;
				}
				break;
			}
			case SpeechParamType.CommunityBoxerReadyToFight:
			{
				Community subjectCommunity9 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity9 != null && rand != null)
				{
					item.Obj = subjectCommunity9.GetFirstBoxerReadyToFight(rand);
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityMemberWithRole:
			{
				Community subjectCommunity7 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				int num3 = Array.IndexOf(Character.RoleNames, Params[i].StringData);
				if (num3 != -1 && subjectCommunity7 != null)
				{
					item.Obj = subjectCommunity7.GetMemberWithRole((Role)num3, rand);
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.CommunityNumMembersWithGiftedItem:
			{
				Community subjectCommunity6 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(Params[i].StringData);
				if (equipmentPrototype != null && subjectCommunity6 != null)
				{
					item.NumericVal = subjectCommunity6.CountMembersWithGiftedItemsOfType(equipmentPrototype);
				}
				else
				{
					item.NumericVal = 0f;
				}
				break;
			}
			case SpeechParamType.CommunitySize:
			{
				Community subjectCommunity5 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity5 != null)
				{
					item.NumericVal = subjectCommunity5.GetLivingNonZombieMemberCount();
				}
				else
				{
					item.NumericVal = 0f;
				}
				break;
			}
			case SpeechParamType.CommunityMiningResource:
			{
				Community subjectCommunity4 = Params[i].GetSubjectCommunity(speaker, listener, obj, param);
				if (subjectCommunity4 == null)
				{
					break;
				}
				foreach (Prop building in subjectCommunity4.Buildings)
				{
					if (!(building is Mine mine))
					{
						continue;
					}
					for (int j = 1; j < 4; j++)
					{
						if (mine.HasRichDeposits((MineralType)j) && EquipmentPrototype.MiningResources[j] != null)
						{
							item.Hash = EquipmentPrototype.MiningResources[j].NameHash;
							break;
						}
					}
				}
				if (item.Hash != 0)
				{
					break;
				}
				foreach (Prop building2 in subjectCommunity4.Buildings)
				{
					if (building2 is Mine mine2 && mine2.HasRichDeposits(MineralType.Stone) && EquipmentPrototype.MiningResources[0] != null)
					{
						item.Hash = EquipmentPrototype.MiningResources[0].NameHash;
						break;
					}
				}
				break;
			}
			case SpeechParamType.NearestCommunityMemberName:
			{
				Character subject18 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Community objectCommunity = Params[i].GetObjectCommunity(speaker, listener, obj, param);
				BaseObjectType species = (BaseObjectType)Math.Max(0, Array.IndexOf(BaseObjectManager.BaseObjectNames, Params[i].StringData));
				if (subject18 != null && objectCommunity != null)
				{
					Character nearestLivingNonZombieMember = objectCommunity.GetNearestLivingNonZombieMember(subject18.Tile, null, species, float.MaxValue);
					if (nearestLivingNonZombieMember != null && (nearestLivingNonZombieMember.NameKnown || speaker == null || speaker.KnowsName(nearestLivingNonZombieMember) || nearestLivingNonZombieMember == listener))
					{
						if (nearestLivingNonZombieMember == speaker && !flag)
						{
							item.Hash = SPEECH_Me;
						}
						else if (nearestLivingNonZombieMember == listener && !flag)
						{
							item.Hash = SPEECH_You;
						}
						else
						{
							item.Str1 = nearestLivingNonZombieMember.FirstName;
							item.Str2 = nearestLivingNonZombieMember.Surname;
							item.Modifier = SpeechParamModifier.ModifyPreviousWord;
						}
						item.Obj = nearestLivingNonZombieMember;
					}
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.NearestCommunityMemberHeOrShe:
			{
				Character subject58 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Community objectCommunity5 = Params[i].GetObjectCommunity(speaker, listener, obj, param);
				BaseObjectType species2 = (BaseObjectType)Math.Max(0, Array.IndexOf(BaseObjectManager.BaseObjectNames, Params[i].StringData));
				if (subject58 != null && objectCommunity5 != null)
				{
					Character nearestLivingNonZombieMember2 = objectCommunity5.GetNearestLivingNonZombieMember(subject58.Tile, null, species2, float.MaxValue);
					if (nearestLivingNonZombieMember2 != null)
					{
						item.Hash = ((nearestLivingNonZombieMember2 == speaker && !flag) ? SPEECH_I : ((nearestLivingNonZombieMember2 == listener && !flag) ? SPEECH_You : ((nearestLivingNonZombieMember2.Appearance.Gender == GenderType.Male) ? SPEECH_He : SPEECH_She)));
						item.Obj = nearestLivingNonZombieMember2;
					}
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_They;
				}
				break;
			}
			case SpeechParamType.MostLikedCommunityMemberName:
			{
				Character subject46 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Community objectCommunity3 = Params[i].GetObjectCommunity(speaker, listener, obj, param);
				if (subject46 != null && objectCommunity3 != null)
				{
					Character communityMemberMostLikedBy2 = objectCommunity3.GetCommunityMemberMostLikedBy(subject46, Params[i].BoolData);
					if (communityMemberMostLikedBy2 != null && (communityMemberMostLikedBy2.NameKnown || speaker == null || speaker.KnowsName(communityMemberMostLikedBy2) || communityMemberMostLikedBy2 == listener))
					{
						if (communityMemberMostLikedBy2 == speaker && !flag)
						{
							item.Hash = SPEECH_Me;
						}
						else if (communityMemberMostLikedBy2 == listener && !flag)
						{
							item.Hash = SPEECH_You;
						}
						else
						{
							item.Str1 = communityMemberMostLikedBy2.FirstName;
							item.Str2 = communityMemberMostLikedBy2.Surname;
							item.Modifier = SpeechParamModifier.ModifyPreviousWord;
						}
						item.Obj = communityMemberMostLikedBy2;
					}
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_Someone;
				}
				break;
			}
			case SpeechParamType.MostLikedCommunityMemberHeOrShe:
			{
				Character subject36 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				Community objectCommunity2 = Params[i].GetObjectCommunity(speaker, listener, obj, param);
				if (subject36 != null && objectCommunity2 != null)
				{
					Character communityMemberMostLikedBy = objectCommunity2.GetCommunityMemberMostLikedBy(subject36, Params[i].BoolData);
					if (communityMemberMostLikedBy != null)
					{
						item.Hash = ((communityMemberMostLikedBy == speaker && !flag) ? SPEECH_I : ((communityMemberMostLikedBy == listener && !flag) ? SPEECH_You : ((communityMemberMostLikedBy.Appearance.Gender == GenderType.Male) ? SPEECH_He : SPEECH_She)));
						item.Obj = communityMemberMostLikedBy;
					}
				}
				if (item.Obj == null)
				{
					item.Hash = SPEECH_They;
				}
				break;
			}
			case SpeechParamType.ItemName:
			{
				EquipmentPrototype subjectEquipmentPrototype3 = Params[i].GetSubjectEquipmentPrototype(speaker, listener, obj, param);
				if (subjectEquipmentPrototype3 != null)
				{
					item.Hash = subjectEquipmentPrototype3.NameHash;
					item.Obj = Params[i].GetSubjectEquipment(speaker, listener, obj, param);
					item.EquipmentProto = subjectEquipmentPrototype3;
				}
				else if (caller is QuestInstance { QuestObjectEquipmentType: not null } questInstance && Params[i].Subject == Specifier.ReferringTo)
				{
					item.Hash = questInstance.QuestObjectEquipmentType.NameHash;
					item.EquipmentProto = questInstance.QuestObjectEquipmentType;
				}
				else
				{
					item.Hash = SPEECH_Something;
				}
				break;
			}
			case SpeechParamType.ItemLiquidContentsName:
			{
				EquipmentPrototype subjectEquipmentPrototype2 = Params[i].GetSubjectEquipmentPrototype(speaker, listener, obj, param);
				LiquidPrototype subjectLiquidPrototype2 = Params[i].GetSubjectLiquidPrototype(speaker, listener, obj, param);
				if (subjectLiquidPrototype2 != null)
				{
					item.Hash = subjectLiquidPrototype2.NameHash;
					item.Obj = Params[i].GetSubjectEquipment(speaker, listener, obj, param);
					item.Liquid = subjectLiquidPrototype2;
				}
				else if (subjectEquipmentPrototype2 != null)
				{
					item.Hash = subjectEquipmentPrototype2.NameHash;
					item.Obj = Params[i].GetSubjectEquipment(speaker, listener, obj, param);
					item.EquipmentProto = subjectEquipmentPrototype2;
				}
				else
				{
					item.Hash = SPEECH_Something;
				}
				break;
			}
			case SpeechParamType.AnItemOrSomeLiquid:
			{
				EquipmentPrototype subjectEquipmentPrototype = Params[i].GetSubjectEquipmentPrototype(speaker, listener, obj, param);
				LiquidPrototype subjectLiquidPrototype = Params[i].GetSubjectLiquidPrototype(speaker, listener, obj, param);
				if (subjectLiquidPrototype != null)
				{
					item.Hash = subjectLiquidPrototype.NameHash;
					item.Modifier = SpeechParamModifier.IndefiniteArticleUncountable;
					item.Obj = Params[i].GetSubjectEquipment(speaker, listener, obj, param);
					item.Liquid = subjectLiquidPrototype;
				}
				else if (subjectEquipmentPrototype != null)
				{
					item.Hash = subjectEquipmentPrototype.NameHash;
					item.Modifier = (subjectEquipmentPrototype.Uncountable ? SpeechParamModifier.IndefiniteArticleUncountable : (subjectEquipmentPrototype.UsePluralIndefiniteArticle ? SpeechParamModifier.IndefiniteArticlePlural : SpeechParamModifier.IndefiniteArticle));
					item.Obj = Params[i].GetSubjectEquipment(speaker, listener, obj, param);
					item.EquipmentProto = subjectEquipmentPrototype;
				}
				else
				{
					item.Hash = SPEECH_Something;
				}
				break;
			}
			case SpeechParamType.WineAdjUpper:
				if (rand != null)
				{
					item.Hash = rand.RandomTranslatedStringHash("WINE_ADJ");
				}
				break;
			case SpeechParamType.WineAdjLower:
				if (rand != null)
				{
					item.Hash = rand.RandomTranslatedStringHash("WINE_ADJ");
				}
				break;
			case SpeechParamType.WineNoun:
				if (rand != null)
				{
					item.Str1 = GameImpl.Translate(rand.RandomTranslatedStringHash("WINE_NOUN"));
					item.Obj = TownName.GetDummyGenderObject(ref item.Str1, plural: false);
				}
				break;
			case SpeechParamType.WineWith:
				if (rand != null)
				{
					item.Hash = rand.RandomTranslatedStringHash("WINE_WITH");
				}
				break;
			case SpeechParamType.WineDescPl:
				if (rand != null)
				{
					item.Hash = rand.RandomTranslatedStringHash("WINE_DESC_PL");
				}
				break;
			case SpeechParamType.WineDescSing:
				if (rand != null)
				{
					item.Hash = rand.RandomTranslatedStringHash("WINE_DESC_SG");
				}
				break;
			case SpeechParamType.ChatSpeech:
				if (chatSpeech != null)
				{
					item.Str1 = chatSpeech;
				}
				break;
			case SpeechParamType.ChatSpeaker:
				if (chatSpeaker != null)
				{
					item.Str1 = chatSpeaker;
				}
				break;
			case SpeechParamType.Strain:
			{
				Character subject11 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				if (subject11 != null)
				{
					switch (subject11.Infection)
					{
					case InfectionType.Green:
						item.Hash = Character.HUD_GreenStrain;
						break;
					case InfectionType.Blue:
						item.Hash = Character.HUD_BlueStrain;
						break;
					case InfectionType.Red:
						item.Hash = Character.HUD_RedStrain;
						break;
					case InfectionType.White:
						item.Hash = Character.HUD_WhiteStrain;
						break;
					}
				}
				break;
			}
			case SpeechParamType.VoteCount:
			{
				Character subject6 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				item.NumericVal = ((subject6 != null && subject6.Community != null) ? subject6.Community.GetVoteCountFor(subject6) : 0);
				break;
			}
			case SpeechParamType.HitTheRoadCount:
				item.NumericVal = Session.Instance.HitTheRoadCount;
				break;
			case SpeechParamType.DaysSinceMostRecentInfectedInjury:
			{
				Character subject2 = Params[i].GetSubject<Character>(speaker, listener, obj, param);
				item.NumericVal = ((subject2 != null) ? Mathf.FloorToInt((float)(Session.Instance.PlayTime - subject2.GetTimeOfMostRecentInfectedInjury()).TotalSeconds / Sun.DayLengthSecs) : 0);
				break;
			}
			case SpeechParamType.MemoryDaysAgo:
				if (param.GetParamType() == ParamType.Memory)
				{
					item.NumericVal = Mathf.FloorToInt((float)(Session.Instance.PlayTime - param.GetMemory().Time).TotalSeconds / Sun.DayLengthSecs);
				}
				break;
			}
			paramResults.Add(item);
		}
	}

	public static void BuildSpeechText(int textHash, List<SpeechParam> Params, BaseObject speaker, BaseObject listener, BaseObject referringTo, List<SpeechParamResult> paramResults, out string speechText, List<SpeechEmoticon> emoticons, bool englishOnly)
	{
		BuildSpeechText(textHash, Params, speaker, listener, referringTo, paramResults, out speechText, emoticons, englishOnly, isQuest: false);
	}

	public static void BuildSpeechText(int textHash, List<SpeechParam> Params, BaseObject speaker, BaseObject listener, BaseObject referringTo, List<SpeechParamResult> paramResults, out string speechText, List<SpeechEmoticon> emoticons, bool englishOnly, bool isQuest)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(GameImpl.Translate(textHash, englishOnly));
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		TempEmoticons.Clear();
		for (int i = 0; i < _stringBuilder.Length - 1; i++)
		{
			switch (_stringBuilder[i])
			{
			case '/':
			case '8':
			case ':':
			case '{':
			case '}':
			{
				for (int j = 0; j < SpeechEmoticon.Emoticons.Length; j++)
				{
					string text = SpeechEmoticon.Emoticons[j];
					if (StringUtil.IsSubStringEqual(_stringBuilder, i, text))
					{
						TempEmoticons.Add((FacialExpression)j);
						_stringBuilder.Remove(i, text.Length);
						if (i < _stringBuilder.Length && _stringBuilder[i] == ' ')
						{
							_stringBuilder.Remove(i, 1);
						}
						_stringBuilder.Insert(i, '\u0001');
						break;
					}
				}
				break;
			}
			}
		}
		StringUtil.ApplyFormulae(_stringBuilder, speaker, listener, referringTo, paramResults, englishOnly, isQuest);
		if (paramResults != null)
		{
			for (int k = 0; k < paramResults.Count; k++)
			{
				int num = FindParamPos(_stringBuilder, k);
				while (num != -1 && k < Params.Count)
				{
					_stringBuilder.Remove(num, 2);
					switch (Params[k].Type)
					{
					case SpeechParamType.WineAdjLower:
					{
						string text8 = GameImpl.Translate(paramResults[k].Hash, englishOnly);
						_stringBuilder.Insert(num, text8);
						if (language != Language.TraditionalChinese && language != Language.SimplifiedChinese && language != Language.Indonesian && language != Language.Russian)
						{
							_stringBuilder.Insert(num, GameImpl.Translate(StringUtil.Vowels.Contains(text8[0]) ? SPEECH_An : SPEECH_A, englishOnly) + " ");
						}
						break;
					}
					case SpeechParamType.PriceToBandage:
					case SpeechParamType.PriceToSellItem:
					case SpeechParamType.PriceToSellFood:
					case SpeechParamType.PriceToSellWater:
					case SpeechParamType.PriceToSell:
					case SpeechParamType.PriceToBuy:
					case SpeechParamType.Formula:
					case SpeechParamType.BoxerWagerAmount:
					case SpeechParamType.CommunityNumMembersWithGiftedItem:
					case SpeechParamType.CommunitySize:
					case SpeechParamType.VoteCount:
					case SpeechParamType.NumGoodRelationshipsInCommunity:
					case SpeechParamType.DaysSinceMostRecentInfectedInjury:
					case SpeechParamType.MemoryDaysAgo:
					case SpeechParamType.HitTheRoadCount:
					case SpeechParamType.Age:
						_stringBuilder.Insert(num, paramResults[k].NumericVal.ToString());
						break;
					default:
					{
						if (paramResults[k].Modifier == SpeechParamModifier.Possessive && (language == Language.English || language == Language.German))
						{
							_stringBuilder.Insert(num, GameImpl.Translate(SPEECH_Possessive, englishOnly));
						}
						bool isProperNoun = false;
						switch (Params[k].Type)
						{
						case SpeechParamType.FirstName:
						case SpeechParamType.FullName:
						case SpeechParamType.HeOrShe:
						case SpeechParamType.HeOrSheOrYou:
						case SpeechParamType.FirstNamePossessive:
						case SpeechParamType.FirstNameActive:
						case SpeechParamType.FirstNameOrMeOrYou:
						case SpeechParamType.FullNamePossessive:
						case SpeechParamType.FullNameActive:
						case SpeechParamType.FullNameOrMeOrYou:
						case SpeechParamType.CommunityName:
						case SpeechParamType.CommunityLeaderNameOrYou:
						case SpeechParamType.CommunityLeaderName:
						case SpeechParamType.CommunityLeaderFirstName:
						case SpeechParamType.CommunityLeaderFullName:
						case SpeechParamType.CommunityPossessive:
						case SpeechParamType.CommunityIOrWe:
						case SpeechParamType.CommunityMeOrUs:
						case SpeechParamType.CommunityBoxerReadyToFight:
						case SpeechParamType.CommunityMemberWithRole:
						case SpeechParamType.ChatSpeaker:
						case SpeechParamType.TownName:
						case SpeechParamType.SquadIOrWe:
						case SpeechParamType.NearestTownOrSettlementName:
						case SpeechParamType.NearestCommunityMemberName:
						case SpeechParamType.NearestCommunityMemberHeOrShe:
						case SpeechParamType.MostLikedCommunityMemberName:
						case SpeechParamType.MostLikedCommunityMemberHeOrShe:
						case SpeechParamType.MrOrMs:
						case SpeechParamType.Surname:
						case SpeechParamType.CommunityNameOrI:
							isProperNoun = true;
							break;
						case SpeechParamType.ItemName:
						case SpeechParamType.ItemLiquidContentsName:
						case SpeechParamType.AnItemOrSomeLiquid:
							isProperNoun = paramResults[k].EquipmentProto != null && paramResults[k].EquipmentProto.IsProperNoun;
							break;
						}
						int num2 = -1;
						int num3 = -1;
						int num4 = num;
						if (paramResults[k].Hash != 0)
						{
							int num5 = paramResults[k].Hash;
							if (num5 == SPEECH_Myself && paramResults[k].GetGender(language) == GenderType.Female && GameImpl.HasTranslationForHash(SPEECH_Myself_Female, englishOnly))
							{
								num5 = SPEECH_Myself_Female;
							}
							else if (num5 == SPEECH_Yourself && paramResults[k].GetGender(language) == GenderType.Female && GameImpl.HasTranslationForHash(SPEECH_Yourself_Female, englishOnly))
							{
								num5 = SPEECH_Yourself_Female;
							}
							else if (num5 == SPEECH_Someone && paramResults[k].GetGender(language) == GenderType.Female && GameImpl.HasTranslationForHash(SPEECH_Someone_Female, englishOnly))
							{
								num5 = SPEECH_Someone_Female;
							}
							if (paramResults[k].Modifier == SpeechParamModifier.Possessive)
							{
								if (num5 == SPEECH_Someone && GameImpl.HasTranslationForHash(SPEECH_Someone_Pos, englishOnly))
								{
									num5 = SPEECH_Someone_Pos;
								}
								else if (num5 == SPEECH_Someone_Female && GameImpl.HasTranslationForHash(SPEECH_Someone_Female_Pos, englishOnly))
								{
									num5 = SPEECH_Someone_Female_Pos;
								}
							}
							string text2 = GameImpl.Translate(num5, englishOnly);
							if (string.IsNullOrEmpty(text2) && paramResults[k].Hash == SPEECH_Her_Possessive && !englishOnly)
							{
								text2 = GameImpl.Translate(SPEECH_Her, englishOnly);
							}
							_stringBuilder.Insert(num, text2);
							num4 = num + text2.Length;
							StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num4, isProperNoun, language);
						}
						else if (!string.IsNullOrEmpty(paramResults[k].Str1))
						{
							Character character = paramResults[k].Obj as Character;
							string text3 = GameImpl.TranslateName(paramResults[k].Str1, englishOnly, character?.FirstNameVerified ?? StringStatus.Verified);
							_stringBuilder.Insert(num, text3);
							num4 = num + text3.Length;
							if (!string.IsNullOrEmpty(paramResults[k].Str2))
							{
								string text4 = GameImpl.TranslateSurname(paramResults[k].Str2, paramResults[k].GetGender(language), englishOnly, character?.SurnameVerified ?? StringStatus.Verified);
								if (GameImpl.WantNamesReversed(paramResults[k].Str2, englishOnly))
								{
									_stringBuilder.Insert(num, text4);
									num4 += text4.Length;
								}
								else
								{
									num2 = num4;
									_stringBuilder.Insert(num4, ' ');
									num4++;
									num3 = num4;
									_stringBuilder.Insert(num4, text4);
									num4 += text4.Length;
								}
							}
							StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num4, isProperNoun, language);
						}
						else if (paramResults[k].Obj != null)
						{
							string displayNameString = paramResults[k].Obj.GetDisplayNameString(noStrangers: true, englishOnly);
							_stringBuilder.Insert(num, displayNameString);
							num4 = num + displayNameString.Length;
							StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num4, isProperNoun, language);
						}
						if (paramResults[k].Modifier == SpeechParamModifier.IndefiniteArticle)
						{
							if (language != Language.Russian)
							{
								GenderType gender = paramResults[k].GetGender(language);
								StringUtil.ApplyLowercaseBeforeInsertingArticle(_stringBuilder, num, language);
								string text5 = GameImpl.Translate(StringUtil.Vowels.Contains(_stringBuilder[num]) ? SPEECH_An : SPEECH_A, SPEECH_IndefiniteArticleFeminine, gender, englishOnly) + " ";
								_stringBuilder.Insert(num, text5);
								StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num + text5.Length, isProperNoun, language);
							}
						}
						else if (paramResults[k].Modifier == SpeechParamModifier.IndefiniteArticlePlural)
						{
							if (language != Language.Russian)
							{
								int hash = ((paramResults[k].GetGender(language) == GenderType.Female) ? SPEECH_Some_PluralFeminine : SPEECH_Some_PluralMasculine);
								if (!GameImpl.HasTranslationForHash(hash, englishOnly))
								{
									hash = SPEECH_Some;
								}
								StringUtil.ApplyLowercaseBeforeInsertingArticle(_stringBuilder, num, language);
								string text6 = GameImpl.Translate(hash, englishOnly) + " ";
								_stringBuilder.Insert(num, text6);
								StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num + text6.Length, isProperNoun, language);
							}
						}
						else if (paramResults[k].Modifier == SpeechParamModifier.IndefiniteArticleUncountable)
						{
							if (language != Language.Russian)
							{
								StringUtil.ApplyLowercaseBeforeInsertingArticle(_stringBuilder, num, language);
								string text7 = GameImpl.Translate(SPEECH_Some, englishOnly) + " ";
								_stringBuilder.Insert(num, text7);
								StringUtil.ApplyCapitalisationIfAtBeginningOfASentence(_stringBuilder, num, num + text7.Length, isProperNoun, language);
							}
						}
						else if (paramResults[k].Modifier == SpeechParamModifier.ModifyPreviousWord || (paramResults[k].Modifier == SpeechParamModifier.Possessive && language == Language.French))
						{
							StringUtil.ApplyLanguageSpecificGrammarRulesToPreviousWords(_stringBuilder, num, language);
						}
						else
						{
							if (paramResults[k].Modifier != SpeechParamModifier.Possessive || paramResults[k].Hash != 0)
							{
								break;
							}
							if (num2 != -1 && num3 != -1)
							{
								switch (language)
								{
								case Language.Czech:
									StringUtil.ApplyCzechPossessiveSuffixToName(_stringBuilder, num4, paramResults[k].GetGender(language), GenderType.Count, objectIsPlural: false, objectIsAnimate: false, genitive: true);
									break;
								case Language.Ukrainian:
									StringUtil.ApplyUkrainianPossessiveSuffixToName(_stringBuilder, num3, num4, paramResults[k].GetGender(language), surname: false, paramResults[k].Str1);
									break;
								}
								switch (language)
								{
								case Language.Czech:
									StringUtil.ApplyCzechPossessiveSuffixToName(_stringBuilder, num2, paramResults[k].GetGender(language), GenderType.Count, objectIsPlural: false, objectIsAnimate: false, genitive: true, surname: true);
									break;
								case Language.Ukrainian:
									StringUtil.ApplyUkrainianPossessiveSuffixToName(_stringBuilder, num, num2, paramResults[k].GetGender(language), surname: true, paramResults[k].Str2);
									break;
								}
							}
							else
							{
								switch (language)
								{
								case Language.Czech:
									StringUtil.ApplyCzechPossessiveSuffixToName(_stringBuilder, num4, paramResults[k].GetGender(language));
									break;
								case Language.Ukrainian:
									StringUtil.ApplyUkrainianPossessiveSuffixToName(_stringBuilder, num, num4, paramResults[k].GetGender(language), surname: false, paramResults[k].Str1);
									break;
								}
							}
						}
						break;
					}
					}
					num = FindParamPos(_stringBuilder, k);
				}
			}
		}
		if (TempEmoticons.Count > 0)
		{
			int num6 = 0;
			int num7 = 0;
			while (num7 < _stringBuilder.Length)
			{
				if (_stringBuilder[num7] == '\u0001')
				{
					_stringBuilder.Remove(num7, 1);
					emoticons?.Add(SpeechEmoticon.Create(TempEmoticons[num6], num7));
					num6++;
					if (num6 >= TempEmoticons.Count)
					{
						break;
					}
				}
				else
				{
					num7++;
				}
			}
		}
		speechText = _stringBuilder.ToString();
	}
}
