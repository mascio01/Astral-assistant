using System.Text;

public class TownName : IReflectable
{
	public TownNameType Type;

	public string CustomString;

	public string TranslatedStringKey;

	public int AdjHash;

	public int NounHash;

	public string CachedEnglishString;

	public string CachedTranslatedString;

	public Language CachedLanguage;

	private static string Male = "[M]";

	private static string Female = "[F]";

	private static string Neuter = "[N]";

	private static string Plural = "[P]";

	private static string Swap = "[Swap]";

	private static int TOWNNAME_AdjNoun = StringUtil.JenkinsHash("TOWNNAME_AdjNoun");

	private StringBuilder sb2 = new StringBuilder(50);

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref CustomString);
		reflector.Add(ref TranslatedStringKey);
		reflector.Add(ref AdjHash);
		reflector.Add(ref NounHash);
	}

	public static DummyBaseObject GetDummyGenderObject(ref string str, bool plural)
	{
		if (str.Contains(Plural))
		{
			plural = true;
			str = str.Replace(Plural, string.Empty).Trim();
		}
		DummyBaseObject dummyBaseObject = null;
		if (str.Contains(Male))
		{
			dummyBaseObject = new DummyBaseObject(GenderType.Male, plural);
			str = str.Replace(Male, string.Empty).Trim();
		}
		else if (str.Contains(Female))
		{
			dummyBaseObject = new DummyBaseObject(GenderType.Female, plural);
			str = str.Replace(Female, string.Empty).Trim();
		}
		else if (str.Contains(Neuter))
		{
			dummyBaseObject = new DummyBaseObject(GenderType.Count, plural);
			str = str.Replace(Neuter, string.Empty).Trim();
		}
		if (str.Contains(Swap))
		{
			str = str.Replace(Swap, string.Empty).Trim();
			if (dummyBaseObject == null)
			{
				dummyBaseObject = new DummyBaseObject(GenderType.Count, plural);
			}
			dummyBaseObject.Swap = true;
		}
		return dummyBaseObject;
	}

	public void BuildDisplayName(StringBuilder sb, bool englishOnly)
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
		case TownNameType.CustomString:
			sb2.Append(CustomString);
			break;
		case TownNameType.TranslatedString:
			sb2.Append(GameImpl.Translate(TranslatedStringKey, englishOnly));
			break;
		case TownNameType.AdjNoun:
		{
			string str = GameImpl.Translate(AdjHash, englishOnly);
			string str2 = GameImpl.Translate(NounHash, englishOnly);
			DummyBaseObject dummyGenderObject = GetDummyGenderObject(ref str, plural: false);
			DummyBaseObject dummyGenderObject2 = GetDummyGenderObject(ref str2, plural: false);
			sb2.Append(GameImpl.Translate(TOWNNAME_AdjNoun, englishOnly));
			if (dummyGenderObject != null && dummyGenderObject.Swap)
			{
				sb2.Replace("%2", str);
				sb2.Replace("%1", str2);
			}
			else
			{
				sb2.Replace("%1", str);
				sb2.Replace("%2", str2);
			}
			StringUtil.ApplyFormulae(sb2, null, dummyGenderObject, dummyGenderObject2);
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

	public void Randomise(CustomRandom rand, bool unique, Town town)
	{
		ClearCache();
		do
		{
			Type = TownNameType.AdjNoun;
			AdjHash = rand.RandomTranslatedStringHash("TOWNNAME_Adj");
			NounHash = rand.RandomTranslatedStringHash("TOWNNAME_Noun");
		}
		while (unique && Session.Instance.CommunityManager.FindTownWithSameName(town) != null);
	}

	public bool IsEqual(TownName other)
	{
		if (Type != other.Type)
		{
			return false;
		}
		switch (Type)
		{
		case TownNameType.CustomString:
			return CustomString == other.CustomString;
		case TownNameType.TranslatedString:
			return TranslatedStringKey == other.TranslatedStringKey;
		default:
			if (AdjHash == other.AdjHash)
			{
				return NounHash == other.NounHash;
			}
			return false;
		}
	}

	public void SetCustomString(string v)
	{
		ClearCache();
		if (Type != TownNameType.TranslatedString)
		{
			Type = TownNameType.CustomString;
		}
		CustomString = v;
	}

	public void SetTranslatedString(string v)
	{
		ClearCache();
		Type = TownNameType.TranslatedString;
		TranslatedStringKey = v;
	}

	public bool IsEmpty()
	{
		switch (Type)
		{
		case TownNameType.CustomString:
			return string.IsNullOrEmpty(CustomString);
		case TownNameType.TranslatedString:
			return string.IsNullOrEmpty(TranslatedStringKey);
		default:
			if (AdjHash == 0)
			{
				return NounHash == 0;
			}
			return false;
		}
	}
}
