using System.Text;

public class GangName : IReflectable
{
	public GangNameType Type;

	public string CustomString;

	public string TranslatedStringKey;

	public int GangHash;

	public int AdjHash;

	public int NounHash;

	public int PluralNounHash;

	public string CachedEnglishString;

	public string CachedTranslatedString;

	public Language CachedLanguage;

	private static int GANGNAME_LeadersGang = StringUtil.JenkinsHash("GANGNAME_LeadersGang");

	private static int GANGNAME_AdjPluralNoun = StringUtil.JenkinsHash("GANGNAME_AdjPluralNoun");

	private static int GANGNAME_TheAdjNounGang = StringUtil.JenkinsHash("GANGNAME_TheAdjNounGang");

	private StringBuilder sb2 = new StringBuilder(50);

	public bool IsNull()
	{
		if (Type == GangNameType.CustomString)
		{
			return string.IsNullOrEmpty(CustomString);
		}
		return false;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref CustomString);
		reflector.Add(ref TranslatedStringKey);
		reflector.Add(ref GangHash);
		reflector.Add(ref AdjHash);
		reflector.Add(ref NounHash);
		reflector.Add(ref PluralNounHash);
	}

	public void BuildDisplayName(StringBuilder sb, bool englishOnly, string leaderFirstName, StringStatus leaderFirstNameVerified, string leaderSurname, StringStatus leaderSurnameVerified, GenderType leaderGender, StringStatus customNameVerified)
	{
		if (englishOnly)
		{
			if (!string.IsNullOrEmpty(CachedEnglishString))
			{
				sb.Append(CachedEnglishString);
				return;
			}
		}
		else if (!string.IsNullOrEmpty(CachedTranslatedString) && CachedLanguage == GameImpl.Instance.Settings.Language)
		{
			sb.Append(CachedTranslatedString);
			return;
		}
		sb2.Length = 0;
		switch (Type)
		{
		case GangNameType.CustomString:
			sb2.Append(CustomString);
			break;
		case GangNameType.TranslatedString:
			sb2.Append(GameImpl.Translate(TranslatedStringKey, englishOnly));
			break;
		case GangNameType.LeadersFirstNameGang:
		{
			string str7 = GameImpl.Translate(GangHash, englishOnly);
			string text2 = GameImpl.TranslateName(leaderFirstName, englishOnly, leaderFirstNameVerified);
			DummyBaseObject dummyBaseObject2 = new DummyBaseObject(leaderGender, plural: false);
			DummyBaseObject dummyGenderObject7 = TownName.GetDummyGenderObject(ref str7, plural: false);
			dummyBaseObject2.Name = text2;
			sb2.Append(GameImpl.Translate(GANGNAME_LeadersGang, englishOnly));
			sb2.Replace("%1", text2);
			sb2.Replace("%2", str7);
			StringUtil.ApplyFormulae(sb2, null, dummyBaseObject2, dummyGenderObject7, englishOnly);
			break;
		}
		case GangNameType.LeadersSurnameGang:
		{
			string str6 = GameImpl.Translate(GangHash, englishOnly);
			string text = GameImpl.TranslateSurname(leaderSurname, leaderGender, englishOnly, leaderSurnameVerified);
			DummyBaseObject dummyBaseObject = new DummyBaseObject(leaderGender, plural: false);
			DummyBaseObject dummyGenderObject6 = TownName.GetDummyGenderObject(ref str6, plural: false);
			dummyBaseObject.Name = text;
			sb2.Append(GameImpl.Translate(GANGNAME_LeadersGang, englishOnly));
			sb2.Replace("%1", text);
			sb2.Replace("%2", str6);
			StringUtil.ApplyFormulae(sb2, null, dummyBaseObject, dummyGenderObject6, englishOnly);
			break;
		}
		case GangNameType.AdjPluralNoun:
		{
			string str4 = GameImpl.Translate(AdjHash, englishOnly);
			string str5 = GameImpl.Translate(PluralNounHash, englishOnly);
			DummyBaseObject dummyGenderObject4 = TownName.GetDummyGenderObject(ref str4, plural: false);
			DummyBaseObject dummyGenderObject5 = TownName.GetDummyGenderObject(ref str5, plural: true);
			sb2.Append(GameImpl.Translate(GANGNAME_AdjPluralNoun, englishOnly));
			sb2.Replace("%1", str4);
			sb2.Replace("%2", str5);
			StringUtil.ApplyFormulae(sb2, null, dummyGenderObject4, dummyGenderObject5, englishOnly);
			break;
		}
		case GangNameType.TheAdjNounGang:
		{
			string str = GameImpl.Translate(AdjHash, englishOnly);
			string str2 = GameImpl.Translate(NounHash, englishOnly);
			string str3 = GameImpl.Translate(GangHash, englishOnly);
			DummyBaseObject dummyGenderObject = TownName.GetDummyGenderObject(ref str, plural: false);
			DummyBaseObject dummyGenderObject2 = TownName.GetDummyGenderObject(ref str2, plural: false);
			DummyBaseObject dummyGenderObject3 = TownName.GetDummyGenderObject(ref str3, plural: false);
			sb2.Append(GameImpl.Translate(GANGNAME_TheAdjNounGang, englishOnly));
			sb2.Replace("%1", str);
			sb2.Replace("%2", str2);
			sb2.Replace("%3", str3);
			StringUtil.ApplyFormulae(sb2, null, dummyGenderObject, dummyGenderObject2, dummyGenderObject3, englishOnly);
			break;
		}
		}
		if (englishOnly)
		{
			CachedEnglishString = sb2.ToString();
		}
		else
		{
			CachedTranslatedString = sb2.ToString();
			CachedLanguage = GameImpl.Instance.Settings.Language;
		}
		sb.Append(sb2);
	}

	public void ClearCache()
	{
		CachedEnglishString = null;
		CachedTranslatedString = null;
	}

	public void Randomise(CustomRandom rand, bool unique, Community community)
	{
		ClearCache();
		do
		{
			Type = (GangNameType)rand.Next(2, 6);
			GangHash = rand.RandomTranslatedStringHash("GANGNAME_Gang");
			AdjHash = rand.RandomTranslatedStringHash("GANGNAME_Adj");
			NounHash = rand.RandomTranslatedStringHash("GANGNAME_Noun");
			PluralNounHash = rand.RandomTranslatedStringHash("GANGNAME_PluralNoun");
		}
		while (unique && Session.Instance.CommunityManager.FindCommunityWithSameGangName(community) != null);
	}

	public bool IsEqual(GangName other)
	{
		if (Type != other.Type)
		{
			return false;
		}
		switch (Type)
		{
		case GangNameType.CustomString:
			return CustomString == other.CustomString;
		case GangNameType.TranslatedString:
			return CustomString == other.TranslatedStringKey;
		default:
			if (Type == other.Type && GangHash == other.GangHash && AdjHash == other.AdjHash && NounHash == other.NounHash)
			{
				return PluralNounHash == other.PluralNounHash;
			}
			return false;
		}
	}

	public void SetCustomString(string v)
	{
		ClearCache();
		if (Type != GangNameType.TranslatedString)
		{
			Type = GangNameType.CustomString;
		}
		CustomString = v;
	}

	public void SetTranslatedString(string v)
	{
		ClearCache();
		Type = GangNameType.TranslatedString;
		TranslatedStringKey = v;
	}
}
