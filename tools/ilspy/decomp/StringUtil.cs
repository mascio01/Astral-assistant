using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class StringUtil
{
	public enum Func
	{
		Invalid = -1,
		MaleCommunity,
		FemaleCommunity,
		Male,
		Female,
		Neuter,
		Plural,
		Uncountable,
		SpeakerCommunity,
		ListenerCommunity,
		ThirdPersonCommunity,
		Speaker,
		Listener,
		ThirdPerson,
		Alive,
		Zero,
		Many,
		Someone,
		Stranger,
		Community,
		Liquid,
		PossessiveFullMascAnim,
		PossessiveFullMascInanim,
		PossessiveFullFem,
		PossessiveFullNeut,
		PossessiveFullPluralFem,
		PossessiveFullPluralNeut,
		PossessiveFullPluralMascAnim,
		PossessiveFullPluralMascInanim,
		PossessiveFull,
		PossessiveMascAnim,
		PossessiveMascInanim,
		PossessiveFem,
		PossessiveNeut,
		PossessivePluralFem,
		PossessivePluralNeut,
		PossessivePluralMascAnim,
		PossessivePluralMascInanim,
		Possessive,
		LocativeFull,
		VocativeFull,
		AccusativeFull,
		DativeFull,
		InstrumentalFull,
		CausalFull,
		SublativeFull,
		DelativeFull,
		AblativeFull,
		Locative,
		Vocative,
		Accusative,
		Dative,
		Instrumental,
		Causal,
		Sublative,
		Delative,
		Ablative,
		Count
	}

	private enum OpType
	{
		None,
		And,
		Or
	}

	private struct Scope
	{
		public bool Not;

		public bool Result;

		public OpType Op;
	}

	public static string Vowels = "aAáÁаАяЯeEéÉеЕэЭєЄёЁiIіІíÍїЇиИoOóöőÓÖŐоОuUúüűÚÜŰуУюЮыЫ";

	public static string HungarianBackVowels = "aAáÁаoOóÓоОuUúÚ";

	public static string HungarianFrontVowels = "eEéÉеЕöőÖŐüűÜŰ";

	public static string HungarianRoundedFrontVowels = "öőÖŐüűÜŰ";

	public static string HungarianUnroundedFrontVowels = "eEéÉеЕ";

	public static string HungarianIntermediateVowels = "iIíÍ";

	public static string RussianConsonants = "БбВвГгДдЖжЗзЙйКкЛлМмНнПпРрСсТтФфХхЦцЧчШшЩщ";

	public static string UkrainianConsonants = "БбВвГгҐґДдЖжЗзЙйКкЛлМмНнПпРрСсТтФфХхЦцЧчШшЩщ";

	private static FastStringBuilder ReversedText = new FastStringBuilder(1000);

	private static char[] lolz = new char[1];

	public static char[] _digits = new char[21]
	{
		'9', '8', '7', '6', '5', '4', '3', '2', '1', '0',
		'1', '2', '3', '4', '5', '6', '7', '8', '9', '.',
		','
	};

	private const string X2 = "X2";

	private static string TrueStr = "true";

	private static string YesStr = "yes";

	public const char EmoticonReplacementChar = '\u0001';

	public static List<SpeechParamResult> FakeParamResults = new List<SpeechParamResult>();

	private static StringBuilder _stringBuilder = new StringBuilder(200);

	public static string[] FuncName = new string[56]
	{
		"maleCommunity", "femaleCommunity", "male", "female", "neuter", "plural", "uncountable", "speakerCommunity", "listenerCommunity", "thirdpersonCommunity",
		"speaker", "listener", "thirdperson", "alive", "zero", "many", "someone", "stranger", "community", "liquid",
		"possessiveFullMascAnim", "possessiveFullMascInanim", "possessiveFullFem", "possessiveFullNeut", "possessiveFullPluralFem", "possessiveFullPluralNeut", "possessiveFullPluralMascAnim", "possessiveFullPluralMascInanim", "possessiveFull", "possessiveMascAnim",
		"possessiveMascInanim", "possessiveFem", "possessiveNeut", "possessivePluralFem", "possessivePluralNeut", "possessivePluralMascAnim", "possessivePluralMascInanim", "possessive", "locativeFull", "vocativeFull",
		"accusativeFull", "dativeFull", "instrumentalFull", "causalFull", "sublativeFull", "delativeFull", "ablativeFull", "locative", "vocative", "accusative",
		"dative", "instrumental", "causal", "sublative", "delative", "ablative"
	};

	private static StringBuilder _btnPromptStr = new StringBuilder(200);

	private const string spriteBegin = "<sprite=\"";

	private const string spriteMid = "\" name=\"";

	private const string spriteEnd = "\">";

	private const string strKeys = "Keys";

	private const string strMouseButtons = "MouseButtons";

	private const string strXBoxButtons = "XBoxButtons";

	private const string XBoxAxes = "XBoxAxes";

	private const string strPS4Buttons = "PS4Buttons";

	private const string strPS4TouchPad = "PS4TouchPad";

	private const string PS4Axes = "PS4Axes";

	private const string strSteamDeckButtons = "SteamDeckButtons";

	private const string strNintendoSwitchButtons = "SwitchButtons";

	private static string Speaker = "speaker";

	private static string Listener = "listener";

	private static string ReferringTo = "referringTo";

	private static string SpeakerCommunity = "speakerCommunity";

	private static string ListenerCommunity = "listenerCommunity";

	private static string ReferringToCommunity = "referringToCommunity";

	private static string SpeakerLeader = "speakerLeader";

	private static string ListenerLeader = "listenerLeader";

	private static string ReferringToLeader = "referringToLeader";

	private static string SpeakerCarrying = "speakerCarrying";

	private static string ListenerCarrying = "listenerCarrying";

	private static string ReferringToCarrying = "referringToCarrying";

	private static List<Scope> ScopeStackMain = new List<Scope>();

	private static List<Scope> ScopeStackThread = new List<Scope>();

	private static string CzechInstrumentalMascSuffix = "em";

	private static string CzechInstrumentalFemSuffix = "ou";

	private static string CzechLocativeSuffix = "ovi";

	private static string CzechVocativeMascSuffix = "e";

	private static string CzechVocativeFemSuffix = "o";

	private static string CzechAccusativeMascSuffix = "a";

	private static string CzechAccusativeFemSuffix = "u";

	private static string CzechDativeMascSuffix = "ovi";

	private static string CzechDativeFemSuffix = "e";

	private static string _val = "val";

	private static string _al = "al";

	private static string _sszal = "sszal";

	private static string _vel = "vel";

	private static string _el = "el";

	private static string _sszel = "sszel";

	private static string UkrainianInstrumentalSuffix_Masc1 = "ом";

	private static string UkrainianInstrumentalSuffix_Masc2 = "ем";

	private static string UkrainianInstrumentalSuffix_Masc3 = "єм";

	private static string UkrainianInstrumentalSuffix_Fem1 = "ою";

	private static string UkrainianInstrumentalSuffix_Fem2 = "ею";

	private static string CzechInstrumental_Me = "mnou";

	private static string CzechInstrumental_You = "tebou";

	private static string CzechInstrumental_Us = "námi";

	private static string CzechInstrumental_YouPl = "vámi";

	private static string HungarianInstrumental_Me = "velem";

	private static string HungarianInstrumental_You = "veled";

	private static string HungarianInstrumental_Us = "velünk";

	private static string HungarianInstrumental_YouPl = "veletek";

	private static string RussianInstrumental_Me = "мной";

	private static string RussianInstrumental_You = "тобой";

	private static string RussianInstrumental_Us = "нами";

	private static string RussianInstrumental_YouPl = "вами";

	private static string UkrainianInstrumental_Me = "мною";

	private static string UkrainianInstrumental_You = "тобою";

	private static string UkrainianInstrumental_Us = "нами";

	private static string UkrainianInstrumental_YouPl = "вами";

	private static string UkrainianLocativeSuffix_Object_Masc = "і";

	private static string UkrainianLocativeSuffix_Living_Masc = "ові";

	private static string CzechAccusative_Me = "mě";

	private static string CzechAccusative_You = "tebe";

	private static string CzechAccusative_Us = "nás";

	private static string CzechAccusative_YouPl = "vás";

	private static string GermanAccusative_Me = "mich";

	private static string GermanAccusative_You = "dich";

	private static string GermanAccusative_Us = "uns";

	private static string GermanAccusative_YouPl = "euch";

	private static string HungarianAccusativeSuffix_t = "t";

	private static string HungarianAccusativeSuffix_ot = "ot";

	private static string HungarianAccusativeSuffix_et = "et";

	private static string HungarianAccusativeSuffix_öt = "öt";

	private static string HungarianAccusative_Me = "engem";

	private static string HungarianAccusative_You = "téged";

	private static string HungarianAccusative_Us = "minket";

	private static string HungarianAccusative_YouPl = "titeket";

	private static string RussianAccusative_Me = "меня";

	private static string RussianAccusative_You = "тебя";

	private static string RussianAccusative_Us = "нас";

	private static string RussianAccusative_YouPl = "вас";

	private static string UkrainianAccusative_Me = "мене";

	private static string UkrainianAccusative_You = "тебе";

	private static string UkrainianAccusative_Us = "нас";

	private static string UkrainianAccusative_YouPl = "вас";

	private static string CzechDative_Me = "mi";

	private static string CzechDative_You = "ti";

	private static string CzechDative_Us = "nám";

	private static string CzechDative_YouPl = "vám";

	private static string GermanDative_Me = "mir";

	private static string GermanDative_You = "dir";

	private static string GermanDative_Us = "uns";

	private static string GermanDative_YouPl = "euch";

	private static string HungarianDativeSuffix_nak = "nak";

	private static string HungarianDativeSuffix_nek = "nek";

	private static string HungarianDative_Me = "nekem";

	private static string HungarianDative_You = "neked";

	private static string HungarianDative_Us = "nekünk";

	private static string HungarianDative_YouPl = "nektek";

	private static string RussianDative_Me = "мне";

	private static string RussianDative_You = "тебе";

	private static string RussianDative_Us = "нам";

	private static string RussianDative_YouPl = "вам";

	private static string UkrainianDative_Me = "мені";

	private static string UkrainianDative_You = "тобі";

	private static string UkrainianDative_Us = "нам";

	private static string UkrainianDative_YouPl = "вам";

	private static string CzechPossessiveSuffix_MascSub_MascOb = "ův";

	private static string CzechPossessiveSuffix_MascSub_FemOb = "ova";

	private static string CzechPossessiveSuffix_MascSub_NeutOb = "ovo";

	private static string CzechPossessiveSuffix_FemSub_MascOb = "in";

	private static string CzechPossessiveSuffix_FemSub_FemOb = "ina";

	private static string CzechPossessiveSuffix_FemSub_NeutOb = "ino";

	private static string CzechGenitiveSuffix_Masc = "a";

	private static string CzechGenitiveSuffix_Masc_y = "ho";

	private static string CzechGenitiveSuffix_Fem = "ové";

	private static string CzechGenitiveSuffix_Fem_o = "vé";

	private static StringBuilder SB = new StringBuilder(50);

	private static StringBuilder SB2 = new StringBuilder(50);

	private static string _Possessive = "_Pos";

	private static string _Dative = "_Dat";

	private static string F = "F";

	private static string CzechMyMasc = "můj";

	private static string CzechMyFem = "má";

	private static string CzechMyNeut = "moje";

	private static string CzechYourMasc = "tvůj";

	private static string CzechYourFem = "tvoje";

	private static string CzechYourNeut = "tvoje";

	private static string CzechOurMasc = "náš";

	private static string CzechOurFem = "naše";

	private static string CzechOurNeut = "naše";

	private static string CzechYourPlMasc = "váš";

	private static string CzechYourPlFem = "vaše";

	private static string CzechYourPlNeut = "vaše";

	private static string CzechIts = "jeho";

	private static string RussianMyMasc = "мой";

	private static string RussianMyFem = "моя";

	private static string RussianMyNeut = "моё";

	private static string RussianYourMasc = "твой";

	private static string RussianYourFem = "твоя";

	private static string RussianYourNeut = "твоё";

	private static string RussianOurMasc = "наш";

	private static string RussianOurFem = "наша";

	private static string RussianOurNeut = "наше";

	private static string RussianYourPlMasc = "ваш";

	private static string RussianYourPlFem = "ваша";

	private static string RussianYourPlNeut = "ваше";

	private static string UkrainianMyMasc = "мій";

	private static string UkrainianMyFem = "моя";

	private static string UkrainianMyNeut = "моє";

	private static string UkrainianYourMasc = "твій";

	private static string UkrainianYourFem = "твоя";

	private static string UkrainianYourNeut = "твоє";

	private static string UkrainianOurMasc = "наш";

	private static string UkrainianOurFem = "наша";

	private static string UkrainianOurNeut = "наше";

	private static string UkrainianYourPlMasc = "ваш";

	private static string UkrainianYourPlFem = "ваша";

	private static string UkrainianYourPlNeut = "ваше";

	private static string UkrainianIts = "його";

	private static string GermanMyMasc = "mein";

	private static string GermanMyFem = "meine";

	private static string GermanMyNeut = "mein";

	private static string GermanYourMasc = "dein";

	private static string GermanYourFem = "deine";

	private static string GermanYourNeut = "dein";

	private static string GermanOurMasc = "unser";

	private static string GermanOurFem = "unsere";

	private static string GermanOurNeut = "unser";

	private static string GermanYourPlMasc = "euer";

	private static string GermanYourPlFem = "eure";

	private static string GermanYourPlNeut = "euer";

	private static string GermanItsMasc = "sein";

	private static string GermanItsFem = "seine";

	private static string GermanItsNeut = "sein";

	private static string HungarianCausalSuffix_ert = "ért";

	private static string HungarianCausal_Me = "értem";

	private static string HungarianCausal_You = "érted";

	private static string HungarianCausal_Us = "értünk";

	private static string HungarianCausal_YouPl = "értetek";

	private static string HungarianSublativeSuffix_ra = "ra";

	private static string HungarianSublativeSuffix_re = "re";

	private static string HungarianSublative_Me = "rám";

	private static string HungarianSublative_You = "rád";

	private static string HungarianSublative_Us = "ránk";

	private static string HungarianSublative_YouPl = "rátok";

	private static string HungarianSublative_Someone = "valakire";

	private static string HungarianDelativeSuffix_ról = "ról";

	private static string HungarianDelativeSuffix_ről = "ről";

	private static string HungarianDelative_Me = "rólam";

	private static string HungarianDelative_You = "rólad";

	private static string HungarianDelative_Us = "rólunk";

	private static string HungarianDelative_YouPl = "rólatok";

	private static string HungarianDelative_Someone = "valakivel";

	private static string HungarianAblativeSuffix_tól = "tól";

	private static string HungarianAblativeSuffix_től = "től";

	private static string HungarianAblative_Me = "tőlem";

	private static string HungarianAblative_You = "tőled";

	private static string HungarianAblative_Us = "tólunk";

	private static string HungarianAblative_YouPl = "mindnyájatoktól";

	private static string HungarianAblative_Someone = "valakitől";

	private static StringBuilder sb = new StringBuilder();

	private const long OneKb = 1024L;

	private const long OneMb = 1048576L;

	private const long OneGb = 1073741824L;

	public static bool Contains(this string str, char c)
	{
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == c)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AreContentsIdentical(this string lhs, string rhs)
	{
		if (lhs.Length != rhs.Length)
		{
			return false;
		}
		for (int i = 0; i < lhs.Length; i++)
		{
			if (lhs[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreContentsIdentical(this StringBuilder stringBuilder, string rhs)
	{
		if (stringBuilder.Length != rhs.Length)
		{
			return false;
		}
		for (int i = 0; i < rhs.Length; i++)
		{
			if (stringBuilder[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreContentsIdentical(this FastStringBuilder stringBuilder, string rhs)
	{
		if (stringBuilder.Length != rhs.Length)
		{
			return false;
		}
		for (int i = 0; i < rhs.Length; i++)
		{
			if (stringBuilder.Get(i) != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AreContentsIdentical(this StringBuilder stringBuilder, StringBuilder rhs)
	{
		if (stringBuilder.Length != rhs.Length)
		{
			return false;
		}
		for (int i = 0; i < rhs.Length; i++)
		{
			if (stringBuilder[i] != rhs[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsArabic(char character)
	{
		if (character >= '\u0600' && character <= 'ۿ')
		{
			return true;
		}
		if (character >= 'ݐ' && character <= 'ݿ')
		{
			return true;
		}
		if (character >= 'ﭐ' && character <= 'ﰿ')
		{
			return true;
		}
		if (character >= 'ﹰ' && character <= 'ﻼ')
		{
			return true;
		}
		return false;
	}

	public static bool IsPunctuation(char c)
	{
		switch (c)
		{
		case '\t':
		case '\n':
		case '\r':
		case ' ':
		case '!':
		case '"':
		case '\'':
		case '(':
		case ')':
		case '+':
		case ',':
		case '-':
		case '.':
		case ':':
		case '=':
		case '?':
		case '؟':
			return true;
		default:
			return false;
		}
	}

	public static bool IsRTLLanguage(Language language)
	{
		return language == Language.Persian;
	}

	public static bool IsStringBuilderRTLInput(StringBuilder input)
	{
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			switch (c)
			{
			case '<':
				flag = true;
				continue;
			case '>':
				flag = false;
				continue;
			}
			if (!flag && char.IsLetter(c))
			{
				return TextUtils.IsRTLCharacter(c);
			}
		}
		return false;
	}

	public static FastStringBuilder ReverseString(string str)
	{
		ReversedText.Clear();
		RTLSupport.FixRTL(str, ReversedText);
		return ReversedText;
	}

	public static FastStringBuilder ReverseStringBuilder(StringBuilder sb)
	{
		return ReverseString(sb.ToString());
	}

	private static bool WantReverseString(string str)
	{
		if (IsRTLLanguage(GameImpl.Instance.Settings.Language) && str != null)
		{
			return TextUtils.IsRTLInput(str);
		}
		return false;
	}

	private static bool WantReverseStringBuilder(StringBuilder sb)
	{
		if (IsRTLLanguage(GameImpl.Instance.Settings.Language))
		{
			return IsStringBuilderRTLInput(sb);
		}
		return false;
	}

	public static string SetUnityText(this TMP_Text unityText, string str)
	{
		if (WantReverseString(str))
		{
			unityText.text = ReverseString(str).ToString();
		}
		else
		{
			unityText.text = str;
		}
		return unityText.text;
	}

	public static string SetUnityText(this TMP_Text unityText, StringBuilder sb)
	{
		if (WantReverseStringBuilder(sb))
		{
			unityText.text = ReverseStringBuilder(sb).ToString();
		}
		else
		{
			unityText.text = sb.ToString();
		}
		return unityText.text;
	}

	public static string SetUnityText(this Text unityText, string str)
	{
		if (WantReverseString(str))
		{
			unityText.text = ReverseString(str).ToString();
		}
		else
		{
			unityText.text = str;
		}
		return unityText.text;
	}

	public static string SetUnityText(this Text unityText, StringBuilder sb)
	{
		if (WantReverseStringBuilder(sb))
		{
			unityText.text = ReverseStringBuilder(sb).ToString();
		}
		else
		{
			unityText.text = sb.ToString();
		}
		return unityText.text;
	}

	public static string SetUnityText(this InputField unityInputField, string str)
	{
		if (WantReverseString(str))
		{
			unityInputField.text = ReverseString(str).ToString();
		}
		else
		{
			unityInputField.text = str;
		}
		return unityInputField.text;
	}

	public static string SetUnityText(this InputField unityInputField, StringBuilder sb)
	{
		if (WantReverseStringBuilder(sb))
		{
			unityInputField.text = ReverseStringBuilder(sb).ToString();
		}
		else
		{
			unityInputField.text = sb.ToString();
		}
		return unityInputField.text;
	}

	public static string SetUnityText(this TMP_InputField unityInputField, string str)
	{
		if (WantReverseString(str))
		{
			unityInputField.text = ReverseString(str).ToString();
		}
		else
		{
			unityInputField.text = str;
		}
		return unityInputField.text;
	}

	public static string SetUnityText(this TMP_InputField unityInputField, StringBuilder sb)
	{
		if (WantReverseStringBuilder(sb))
		{
			unityInputField.text = ReverseStringBuilder(sb).ToString();
		}
		else
		{
			unityInputField.text = sb.ToString();
		}
		return unityInputField.text;
	}

	public static void SetUnityTextIfDifferent(this TMP_Text unityText, string str)
	{
		if (WantReverseString(str))
		{
			FastStringBuilder fastStringBuilder = ReverseString(str);
			if (!fastStringBuilder.AreContentsIdentical(unityText.text))
			{
				unityText.text = fastStringBuilder.ToString();
			}
		}
		else if (str != unityText.text)
		{
			unityText.text = str;
		}
	}

	public static void SetUnityTextIfDifferent(this TMP_Text unityText, StringBuilder sb)
	{
		if (WantReverseStringBuilder(sb))
		{
			FastStringBuilder fastStringBuilder = ReverseStringBuilder(sb);
			if (!fastStringBuilder.AreContentsIdentical(unityText.text))
			{
				unityText.text = fastStringBuilder.ToString();
			}
		}
		else if (!sb.AreContentsIdentical(unityText.text))
		{
			unityText.text = sb.ToString();
		}
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, StringBuilder rhs)
	{
		for (int i = 0; i < rhs.Length; i++)
		{
			stringBuilder.Append(rhs[i]);
		}
		return stringBuilder;
	}

	public static StringBuilder InsertWithoutGarbage(this StringBuilder stringBuilder, int pos, StringBuilder rhs)
	{
		for (int i = 0; i < rhs.Length; i++)
		{
			lolz[0] = rhs[i];
			stringBuilder.Insert(pos + i, lolz);
		}
		return stringBuilder;
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, uint number)
	{
		int length = stringBuilder.Length;
		int num = 0;
		do
		{
			if (num > 0 && num % 3 == 0)
			{
				stringBuilder.Insert(length, _digits, 20, 1);
			}
			stringBuilder.Insert(length, _digits, (int)number % 10 + 9, 1);
			number /= 10;
			num++;
		}
		while (number != 0);
		return stringBuilder;
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, int number)
	{
		return stringBuilder.AppendWithoutGarbage(number, comma: true);
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, int number, bool comma)
	{
		if (number < 0)
		{
			number = -number;
			stringBuilder.Append('-');
		}
		int length = stringBuilder.Length;
		int num = 0;
		do
		{
			if (comma && num > 0 && num % 3 == 0)
			{
				stringBuilder.Insert(length, _digits, 20, 1);
			}
			stringBuilder.Insert(length, _digits, number % 10 + 9, 1);
			number /= 10;
			num++;
		}
		while (number != 0);
		return stringBuilder;
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, float originalNumber, int dps)
	{
		return stringBuilder.AppendWithoutGarbage(originalNumber, dps, comma: true);
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, float originalNumber, int dps, bool comma)
	{
		float num = originalNumber;
		for (int i = 0; i < dps; i++)
		{
			num *= 10f;
		}
		num += 0.5f * (float)Math.Sign(num);
		int num2 = (int)num;
		if (num2 < 0)
		{
			num2 = -num2;
			stringBuilder.Append('-');
		}
		if (originalNumber == 0f)
		{
			stringBuilder.Append('0');
		}
		int length = stringBuilder.Length;
		int num3 = 0;
		bool flag = true;
		do
		{
			if (comma && num3 > dps && (num3 - dps) % 3 == 0)
			{
				stringBuilder.Insert(length, _digits, 20, 1);
			}
			if (flag && (num3 >= dps || num2 % 10 != 0))
			{
				flag = false;
			}
			if (!flag)
			{
				stringBuilder.Insert(length, _digits, num2 % 10 + 9, 1);
			}
			num2 /= 10;
			num3++;
			if (num3 == dps && !flag)
			{
				stringBuilder.Insert(length, _digits, 19, 1);
				if (num2 == 0)
				{
					stringBuilder.Insert(length, '0');
				}
			}
		}
		while (num2 != 0 || num3 < dps);
		return stringBuilder;
	}

	public static StringBuilder AppendWithoutGarbage(this StringBuilder stringBuilder, double originalNumber, int dps)
	{
		double num = originalNumber;
		for (int i = 0; i < dps; i++)
		{
			num *= 10.0;
		}
		num += 0.5 * (double)Math.Sign(num);
		long num2 = (long)num;
		if (num2 < 0)
		{
			num2 = -num2;
			stringBuilder.Append('-');
		}
		if (originalNumber == 0.0)
		{
			stringBuilder.Append('0');
		}
		int length = stringBuilder.Length;
		int num3 = 0;
		bool flag = true;
		do
		{
			if (num3 > dps && (num3 - dps) % 3 == 0)
			{
				stringBuilder.Insert(length, _digits, 20, 1);
			}
			if (flag && (num3 >= dps || num2 % 10 != 0L))
			{
				flag = false;
			}
			if (!flag)
			{
				stringBuilder.Insert(length, _digits, (int)(num2 % 10) + 9, 1);
			}
			num2 /= 10;
			num3++;
			if (num3 == dps && !flag)
			{
				stringBuilder.Insert(length, _digits, 19, 1);
				if (num2 == 0L)
				{
					stringBuilder.Insert(length, '0');
				}
			}
		}
		while (num2 != 0L || num3 < dps);
		return stringBuilder;
	}

	public static string[] GetEnumNames<T>()
	{
		return GetEnumNames<T>(null);
	}

	public static string[] GetEnumNames<T>(string ignoreMe)
	{
		List<T> list = (from x in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public)
			select (T)x.GetValue(null)).ToList();
		List<string> list2 = new List<string>();
		foreach (T item in list)
		{
			string text = item.ToString();
			if (!(text == "Count") && !(text == ignoreMe))
			{
				list2.Add(text);
			}
		}
		return list2.ToArray();
	}

	public static Type[] GetEnumTypes<T>()
	{
		List<T> list = (from x in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public)
			select (T)x.GetValue(null)).ToList();
		List<Type> list2 = new List<Type>();
		foreach (T item in list)
		{
			Type type = Type.GetType(item.ToString());
			if (type == null && item.ToString() != "Invalid" && item.ToString() != "Count" && item.ToString() != "Dog" && item.ToString() != "Cat" && item.ToString() != "MassGrave" && item.ToString() != "CompostBin" && item.ToString() != "Recycler" && item.ToString() != "MarketStall" && item.ToString() != "Generator" && item.ToString() != "LightSource" && item.ToString() != "Beehive" && !item.ToString().StartsWith("Bear") && !item.ToString().StartsWith("Wolf") && !item.ToString().StartsWith("Fox") && !item.ToString().StartsWith("Rat") && !item.ToString().StartsWith("Horse") && !item.ToString().Contains("DEPRECATED"))
			{
				Debug.LogError("Could not find type for " + item.ToString());
			}
			list2.Add(type);
		}
		return list2.ToArray();
	}

	public static int JenkinsHash(string key)
	{
		int length = key.Length;
		int i;
		int num = (i = 0);
		for (; i < length; i++)
		{
			num += key[i];
			num += num << 10;
			num ^= num >> 6;
		}
		num += num << 3;
		num ^= num >> 11;
		return num + (num << 15);
	}

	public static int JenkinsHash(StringBuilder key)
	{
		int length = key.Length;
		int i;
		int num = (i = 0);
		for (; i < length; i++)
		{
			num += key[i];
			num += num << 10;
			num ^= num >> 6;
		}
		num += num << 3;
		num ^= num >> 11;
		return num + (num << 15);
	}

	public static int JenkinsHash(string key, char extraChar)
	{
		int length = key.Length;
		int i;
		int num = (i = 0);
		for (; i < length; i++)
		{
			num += key[i];
			num += num << 10;
			num ^= num >> 6;
		}
		num += extraChar;
		num += num << 10;
		num ^= num >> 6;
		num += num << 3;
		num ^= num >> 11;
		return num + (num << 15);
	}

	public static int JenkinsHash(byte[] key)
	{
		int num = key.Length;
		int i;
		int num2 = (i = 0);
		for (; i < num; i++)
		{
			num2 += key[i];
			num2 += num2 << 10;
			num2 ^= num2 >> 6;
		}
		num2 += num2 << 3;
		num2 ^= num2 >> 11;
		return num2 + (num2 << 15);
	}

	public static string ColorToHex(Color32 color)
	{
		return color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
	}

	public static Color32 HexToColor(string hex)
	{
		hex = hex.Replace("#", "");
		hex = hex.Trim();
		byte r = ((hex.Length >= 2) ? ParseHexNumber(hex.Substring(0, 2), 0) : byte.MaxValue);
		byte g = ((hex.Length >= 4) ? ParseHexNumber(hex.Substring(2, 2), 0) : byte.MaxValue);
		byte b = ((hex.Length >= 6) ? ParseHexNumber(hex.Substring(4, 2), 0) : byte.MaxValue);
		byte a = ((hex.Length >= 8) ? ParseHexNumber(hex.Substring(6, 2), 0) : byte.MaxValue);
		return new Color32(r, g, b, a);
	}

	public static byte ParseHexNumber(string str, byte defaultValue = 0)
	{
		if (byte.TryParse(str, NumberStyles.HexNumber, null, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static bool ParseBool(string str)
	{
		if (!(str == TrueStr) && !(str == YesStr))
		{
			return ParseFloat(str) != 0f;
		}
		return true;
	}

	public static int ParseInt(string str)
	{
		return ParseInt(str, 0);
	}

	public static int ParseInt(string str, int defaultValue)
	{
		if (int.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static uint ParseUInt(string str)
	{
		return ParseUInt(str, 0u);
	}

	public static uint ParseUInt(string str, uint defaultValue)
	{
		if (uint.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static long ParseLong(string str)
	{
		return ParseLong(str, 0L);
	}

	public static long ParseLong(string str, long defaultValue)
	{
		if (long.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static ulong ParseULong(string str)
	{
		return ParseULong(str, 0uL);
	}

	public static ulong ParseULong(string str, ulong defaultValue)
	{
		if (ulong.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static float ParseFloat(string str)
	{
		return ParseFloat(str, 0f);
	}

	public static float ParseFloat(string str, float defaultValue)
	{
		if (float.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static DateTime ParseDateTime(string str)
	{
		return ParseDateTime(str, DateTime.Now);
	}

	public static DateTime ParseDateTime(string str, DateTime defaultValue)
	{
		if (DateTime.TryParse(str, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public static Vector2 ParseVector2(string str)
	{
		return ParseVector2(str, Vector2.zero);
	}

	public static Vector2 ParseVector2(string str, Vector2 defaultValue)
	{
		try
		{
			string[] array = str.Replace("(", "").Replace(")", "").Split(',');
			float x = float.Parse(array[0], CultureInfo.InvariantCulture);
			float y = float.Parse(array[1], CultureInfo.InvariantCulture);
			return new Vector2(x, y);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	public static Vector3 ParseVector3(string str)
	{
		return ParseVector3(str, Vector3.zero);
	}

	public static Vector3 ParseVector3(string str, Vector3 defaultVal)
	{
		try
		{
			string[] array = str.Replace("(", "").Replace(")", "").Split(',');
			float x = float.Parse(array[0], CultureInfo.InvariantCulture);
			float y = float.Parse(array[1], CultureInfo.InvariantCulture);
			float z = float.Parse(array[2], CultureInfo.InvariantCulture);
			return new Vector3(x, y, z);
		}
		catch (Exception)
		{
			return defaultVal;
		}
	}

	public static TerrainCoord ParseTerrainCoord(string str)
	{
		return ParseTerrainCoord(str, TerrainCoord.Zero);
	}

	public static TerrainCoord ParseTerrainCoord(string str, TerrainCoord defaultValue)
	{
		try
		{
			string[] array = str.Replace("(", "").Replace(")", "").Split(',');
			int x = int.Parse(array[0], CultureInfo.InvariantCulture);
			int y = int.Parse(array[1], CultureInfo.InvariantCulture);
			return new TerrainCoord(x, y);
		}
		catch (Exception)
		{
			return defaultValue;
		}
	}

	public static void ApplyLowercaseBeforeInsertingArticle(StringBuilder stringBuilder, int pos, Language language)
	{
		if (pos < _stringBuilder.Length && language != Language.German && (pos + 1 >= stringBuilder.Length || stringBuilder[pos + 1] != '.'))
		{
			_stringBuilder[pos] = char.ToLower(_stringBuilder[pos]);
		}
	}

	public static void ApplyCapitalisationIfAtBeginningOfASentence(StringBuilder stringBuilder, int pos, int end, bool isProperNoun)
	{
		ApplyCapitalisationIfAtBeginningOfASentence(stringBuilder, pos, end, isProperNoun, Language.English);
	}

	public static void ApplyCapitalisationIfAtBeginningOfASentence(StringBuilder stringBuilder, int pos, int end, bool isProperNoun, Language language)
	{
		bool flag = false;
		int num = pos - 1;
		while (!flag)
		{
			if (num < 0)
			{
				flag = true;
				continue;
			}
			switch (stringBuilder[num])
			{
			case '!':
			case '.':
			case '?':
				flag = true;
				continue;
			case '\u0001':
			case ' ':
				num--;
				continue;
			}
			break;
		}
		if (flag)
		{
			if (pos >= 0 && pos < stringBuilder.Length)
			{
				stringBuilder[pos] = char.ToUpper(stringBuilder[pos]);
			}
		}
		else
		{
			if (isProperNoun || language == Language.German || pos < 0 || pos >= stringBuilder.Length)
			{
				return;
			}
			end = Math.Min(end, stringBuilder.Length);
			while (pos < end)
			{
				if (pos + 1 >= end || stringBuilder[pos + 1] != '.')
				{
					stringBuilder[pos] = char.ToLower(stringBuilder[pos]);
				}
				pos++;
			}
		}
	}

	public static void ApplyLanguageSpecificGrammarRulesToPreviousWords(StringBuilder stringBuilder, int pos, Language language)
	{
		switch (language)
		{
		case Language.English:
			if (pos >= 2 && pos < stringBuilder.Length && Vowels.Contains(stringBuilder[pos]) && stringBuilder[pos - 1] == ' ' && (stringBuilder[pos - 2] == 'a' || stringBuilder[pos - 2] == 'A') && (pos - 3 < 0 || stringBuilder[pos - 3] == ' '))
			{
				stringBuilder.Insert(pos - 1, 'n');
			}
			break;
		case Language.French:
			if (pos >= 3 && pos < stringBuilder.Length && Vowels.Contains(stringBuilder[pos]) && (stringBuilder[pos - 3] == 'd' || stringBuilder[pos - 3] == 'D') && stringBuilder[pos - 2] == 'e' && stringBuilder[pos - 1] == ' ' && (pos - 4 < 0 || stringBuilder[pos - 4] == ' '))
			{
				stringBuilder[pos - 2] = '\'';
				stringBuilder.Remove(pos - 1, 1);
			}
			if (pos >= 4 && pos < stringBuilder.Length && Vowels.Contains(stringBuilder[pos]) && (stringBuilder[pos - 4] == 'q' || stringBuilder[pos - 4] == 'Q') && stringBuilder[pos - 3] == 'u' && stringBuilder[pos - 2] == 'e' && stringBuilder[pos - 1] == ' ' && (pos - 5 < 0 || stringBuilder[pos - 5] == ' '))
			{
				stringBuilder[pos - 2] = '\'';
				stringBuilder.Remove(pos - 1, 1);
			}
			break;
		}
	}

	public static bool ContainsFormulaWithAnyParam(string str)
	{
		for (int i = 0; i < str.Length - 2; i++)
		{
			if (str[i] == '(' && str[i + 1] >= '1' && str[i + 1] <= '9' && str[i + 2] >= ')')
			{
				return true;
			}
		}
		return false;
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, BaseObject speaker, BaseObject listener, BaseObject referringTo, List<SpeechParamResult> paramResults, bool englishOnly)
	{
		ApplyFormulae(stringBuilder, speaker, listener, referringTo, paramResults, englishOnly, isQuest: false);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, BaseObject speaker, BaseObject listener, BaseObject referringTo, List<SpeechParamResult> paramResults, bool englishOnly, bool isQuest)
	{
		for (int i = 0; i < stringBuilder.Length; i++)
		{
			if (stringBuilder[i] != '[')
			{
				continue;
			}
			int pos = i + 1;
			int j;
			for (j = pos; j < stringBuilder.Length && stringBuilder[j] != ']'; j++)
			{
			}
			if (!ParseAndSubstituteButtonPrompt(stringBuilder, i, j))
			{
				string text = ParseFormula(stringBuilder, ref pos, j, acceptStrings: false, paramResults, speaker, listener, referringTo, englishOnly, isQuest);
				if (text != null)
				{
					stringBuilder.Remove(i, Math.Min(stringBuilder.Length, j + 1) - i);
					stringBuilder.Insert(i, text);
					i--;
				}
			}
		}
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		ApplyFormulae(stringBuilder, null, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, BaseObject param0, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, EquipmentPrototype proto0, float num0 = 0f, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(proto0, num0));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, LiquidPrototype liquid0, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(liquid0));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, float num0, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(num0));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulaeWithHashes(StringBuilder stringBuilder, Character speaker, BaseObject param0, int hash0, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0, hash0));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, BaseObject param0, BaseObject param1, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0));
		FakeParamResults.Add(SpeechParamResult.Create(param1));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, BaseObject param0, float num1, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0));
		FakeParamResults.Add(SpeechParamResult.Create(num1));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, float num0, float num1, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(num0));
		FakeParamResults.Add(SpeechParamResult.Create(num1));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulaeWithHashes(StringBuilder stringBuilder, Character speaker, BaseObject param0, int hash0, BaseObject param1, int hash1, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0, hash0));
		FakeParamResults.Add(SpeechParamResult.Create(param1, hash1));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, BaseObject param0, BaseObject param1, BaseObject param2, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0));
		FakeParamResults.Add(SpeechParamResult.Create(param1));
		FakeParamResults.Add(SpeechParamResult.Create(param2));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static void ApplyFormulae(StringBuilder stringBuilder, Character speaker, BaseObject param0, BaseObject param1, BaseObject param2, BaseObject param3, bool englishOnly = false)
	{
		FakeParamResults.Clear();
		FakeParamResults.Add(SpeechParamResult.Create(param0));
		FakeParamResults.Add(SpeechParamResult.Create(param1));
		FakeParamResults.Add(SpeechParamResult.Create(param2));
		FakeParamResults.Add(SpeechParamResult.Create(param3));
		ApplyFormulae(stringBuilder, speaker, null, null, FakeParamResults, englishOnly);
	}

	public static string ApplyFormulae(string str, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, BaseObject param0, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, param0, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, float num0, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, num0, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulaeWithHashes(string str, Character speaker, BaseObject param0, int hash0, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulaeWithHashes(_stringBuilder, speaker, param0, hash0, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, BaseObject param0, BaseObject param1, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, param0, param1, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulaeWithHashes(string str, Character speaker, BaseObject param0, int hash0, BaseObject param1, int hash1, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulaeWithHashes(_stringBuilder, speaker, param0, hash0, param1, hash1, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, BaseObject param0, BaseObject param1, BaseObject param2, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, param0, param1, param2, englishOnly);
		return _stringBuilder.ToString();
	}

	public static string ApplyFormulae(string str, Character speaker, BaseObject param0, BaseObject param1, BaseObject param2, BaseObject param3, bool englishOnly = false)
	{
		_stringBuilder.Length = 0;
		_stringBuilder.Append(str);
		ApplyFormulae(_stringBuilder, speaker, param0, param1, param2, param3, englishOnly);
		return _stringBuilder.ToString();
	}

	public static StringBuilder AppendSpriteString(this StringBuilder stringBuilder, string sprite, string name)
	{
		stringBuilder.Append("<sprite=\"");
		stringBuilder.Append(sprite);
		stringBuilder.Append("\" name=\"");
		stringBuilder.Append(name);
		stringBuilder.Append("\">");
		return stringBuilder;
	}

	public static StringBuilder AppendSpriteStringForKey(this StringBuilder stringBuilder, KeyCode key)
	{
		if (key >= KeyCode.Mouse0)
		{
			if (key > KeyCode.Mouse4)
			{
				key = KeyCode.Mouse4;
			}
			stringBuilder.AppendSpriteString("MouseButtons", InputFunctionManager.KeyCodeName[(int)key]);
		}
		else if (key != KeyCode.None)
		{
			stringBuilder.AppendSpriteString("Keys", InputFunctionManager.KeyCodeName[(int)key]);
		}
		return stringBuilder;
	}

	public static string GetXBoxButtonSpriteString(XBoxButton xBoxButton)
	{
		return new StringBuilder(50).AppendSpriteString("XBoxButtons", InputFunctionManager.XBoxButtonName[(int)xBoxButton]).ToString();
	}

	public static string GetPS4ButtonSpriteString(PS4Button ps4Button)
	{
		return new StringBuilder(50).AppendSpriteString((ps4Button == PS4Button.Touchpad) ? "PS4TouchPad" : "PS4Buttons", InputFunctionManager.PS4ButtonName[(int)ps4Button]).ToString();
	}

	public static string GetSteamDeckButtonSpriteString(SteamDeckButton steamDeckButton)
	{
		return new StringBuilder(50).AppendSpriteString("SteamDeckButtons", InputFunctionManager.SteamDeckButtonName[(int)steamDeckButton]).ToString();
	}

	public static string GetNintendoSwitchButtonSpriteString(NintendoSwitchButton steamDeckButton)
	{
		return new StringBuilder(50).AppendSpriteString("SwitchButtons", InputFunctionManager.NintendoSwitchButtonName[(int)steamDeckButton]).ToString();
	}

	public static string GetButtonPromptString(InputFunction inputFunction)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendButtonPromptString(inputFunction);
		return stringBuilder.ToString();
	}

	public static StringBuilder AppendButtonPromptString(this StringBuilder stringBuilder, InputFunction inputFunction)
	{
		MappedInput mappedInput = InputFunctionManager.Instance.InputMappings[(int)inputFunction];
		int length = stringBuilder.Length;
		if (InputFunctionManager.Instance.CurrentInputType == InputType.XBox)
		{
			if (InputFunctionManager.IsAxis(inputFunction))
			{
				if (mappedInput.XBoxAxis != XBoxAxis.None)
				{
					stringBuilder.AppendSpriteString("XBoxAxes", InputFunctionManager.XBoxAxisName[(int)mappedInput.XBoxAxis]);
				}
				else if (mappedInput.NegativeXBoxButton != XBoxButton.None || mappedInput.PositiveXBoxButton != XBoxButton.None)
				{
					if (mappedInput.NegativeXBoxButton != XBoxButton.None)
					{
						stringBuilder.AppendSpriteString("XBoxButtons", InputFunctionManager.XBoxButtonName[(int)mappedInput.NegativeXBoxButton]);
					}
					if (mappedInput.PositiveXBoxButton != XBoxButton.None)
					{
						stringBuilder.AppendSpriteString("XBoxButtons", InputFunctionManager.XBoxButtonName[(int)mappedInput.PositiveXBoxButton]);
					}
				}
			}
			else if (mappedInput.PositiveXBoxButton != XBoxButton.None)
			{
				stringBuilder.AppendSpriteString("XBoxButtons", InputFunctionManager.XBoxButtonName[(int)mappedInput.PositiveXBoxButton]);
			}
		}
		if (InputFunctionManager.Instance.CurrentInputType == InputType.PS4)
		{
			if (InputFunctionManager.IsAxis(inputFunction))
			{
				if (mappedInput.PS4Axis != PS4Axis.None)
				{
					stringBuilder.AppendSpriteString("PS4Axes", InputFunctionManager.PS4AxisName[(int)mappedInput.PS4Axis]);
				}
				else if (mappedInput.NegativePS4Button != PS4Button.None || mappedInput.PositivePS4Button != PS4Button.None)
				{
					if (mappedInput.NegativePS4Button != PS4Button.None)
					{
						stringBuilder.AppendSpriteString((mappedInput.NegativePS4Button == PS4Button.Touchpad) ? "PS4TouchPad" : "PS4Buttons", InputFunctionManager.PS4ButtonName[(int)mappedInput.NegativePS4Button]);
					}
					if (mappedInput.PositivePS4Button != PS4Button.None)
					{
						stringBuilder.AppendSpriteString((mappedInput.PositivePS4Button == PS4Button.Touchpad) ? "PS4TouchPad" : "PS4Buttons", InputFunctionManager.PS4ButtonName[(int)mappedInput.PositivePS4Button]);
					}
				}
			}
			else if (mappedInput.PositivePS4Button != PS4Button.None)
			{
				stringBuilder.AppendSpriteString((mappedInput.PositivePS4Button == PS4Button.Touchpad) ? "PS4TouchPad" : "PS4Buttons", InputFunctionManager.PS4ButtonName[(int)mappedInput.PositivePS4Button]);
			}
		}
		if (InputFunctionManager.Instance.CurrentInputType == InputType.SteamDeck)
		{
			if (InputFunctionManager.IsAxis(inputFunction))
			{
				if (mappedInput.SteamDeckAxis != SteamDeckAxis.None)
				{
					stringBuilder.AppendSpriteString("PS4Axes", InputFunctionManager.PS4AxisName[(int)mappedInput.SteamDeckAxis]);
				}
				else if (mappedInput.NegativeSteamDeckButton != SteamDeckButton.None || mappedInput.PositiveSteamDeckButton != SteamDeckButton.None)
				{
					if (mappedInput.NegativeSteamDeckButton != SteamDeckButton.None)
					{
						stringBuilder.AppendSpriteString("SteamDeckButtons", InputFunctionManager.SteamDeckButtonName[(int)mappedInput.NegativeSteamDeckButton]);
					}
					if (mappedInput.PositiveSteamDeckButton != SteamDeckButton.None)
					{
						stringBuilder.AppendSpriteString("SteamDeckButtons", InputFunctionManager.SteamDeckButtonName[(int)mappedInput.PositiveSteamDeckButton]);
					}
				}
			}
			else if (mappedInput.PositiveSteamDeckButton != SteamDeckButton.None)
			{
				stringBuilder.AppendSpriteString("SteamDeckButtons", InputFunctionManager.SteamDeckButtonName[(int)mappedInput.PositiveSteamDeckButton]);
			}
		}
		if (stringBuilder.Length == length)
		{
			if (InputFunctionManager.Instance.CurrentInputType != InputType.MouseAndKeyboard && (uint)(inputFunction - 33) <= 3u && InputFunctionManager.Instance.IsMapped(InputFunction.Inventory))
			{
				return stringBuilder.AppendButtonPromptString(InputFunction.Inventory);
			}
			if (InputFunctionManager.IsAxis(inputFunction))
			{
				if (mappedInput.MouseAxis != MouseAxis.None)
				{
					stringBuilder.AppendSpriteString("MouseButtons", InputFunctionManager.MouseAxisName[(int)mappedInput.MouseAxis]);
				}
				else if (mappedInput.NegativeKey != KeyCode.None || mappedInput.PositiveKey != KeyCode.None)
				{
					stringBuilder.AppendSpriteStringForKey(mappedInput.NegativeKey);
					stringBuilder.AppendSpriteStringForKey(mappedInput.PositiveKey);
				}
				else if (mappedInput.AltNegativeKey != KeyCode.None || mappedInput.AltPositiveKey != KeyCode.None)
				{
					stringBuilder.AppendSpriteStringForKey(mappedInput.AltNegativeKey);
					stringBuilder.AppendSpriteStringForKey(mappedInput.AltPositiveKey);
				}
			}
			else if (mappedInput.PositiveKey != KeyCode.None)
			{
				stringBuilder.AppendSpriteStringForKey(mappedInput.PositiveKey);
			}
			else if (mappedInput.AltPositiveKey != KeyCode.None)
			{
				stringBuilder.AppendSpriteStringForKey(mappedInput.AltPositiveKey);
			}
		}
		return stringBuilder;
	}

	private static bool ParseAndSubstituteButtonPrompt(StringBuilder stringBuilder, int start, int end)
	{
		int pos = start + 1;
		SkipWhitespace(stringBuilder, ref pos, end);
		for (int i = 0; i < 78; i++)
		{
			if (IsSubStringEqual(stringBuilder, pos, end, InputFunctionManager.Name[i]))
			{
				InputFunction inputFunction = (InputFunction)i;
				_btnPromptStr.Length = 0;
				_btnPromptStr.AppendButtonPromptString(inputFunction);
				stringBuilder.Remove(start, end + 1 - start);
				stringBuilder.InsertWithoutGarbage(start, _btnPromptStr);
				return true;
			}
		}
		return false;
	}

	private static void ApplyOpToScope(List<Scope> scopeStack, int i, bool val)
	{
		Scope value = scopeStack[i];
		switch (value.Op)
		{
		case OpType.None:
			value.Result = val;
			break;
		case OpType.And:
			value.Result &= val;
			break;
		case OpType.Or:
			value.Result |= val;
			break;
		}
		scopeStack[i] = value;
	}

	public static bool IsPlural(float val, Language language)
	{
		if (language == Language.BrazilianPortuguese)
		{
			return Mathf.Abs(val) >= 2f;
		}
		if (Mathf.Abs(val) != 1f)
		{
			return val != 0f;
		}
		return false;
	}

	private static string ParseFormula(StringBuilder stringBuilder, ref int pos, int end, bool acceptStrings, List<SpeechParamResult> paramResults, BaseObject speaker, BaseObject listener, BaseObject referringTo, bool englishOnly)
	{
		return ParseFormula(stringBuilder, ref pos, end, acceptStrings, paramResults, speaker, listener, referringTo, englishOnly, isQuest: false);
	}

	private static string ParseFormula(StringBuilder stringBuilder, ref int pos, int end, bool acceptStrings, List<SpeechParamResult> paramResults, BaseObject speaker, BaseObject listener, BaseObject referringTo, bool englishOnly, bool isQuest)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		List<Scope> list = (Util.AmIOnMainThread() ? ScopeStackMain : ScopeStackThread);
		list.Clear();
		list.Add(default(Scope));
		while (true)
		{
			int num = pos;
			SkipWhitespace(stringBuilder, ref pos, end);
			bool flag = false;
			while (pos < end)
			{
				if (stringBuilder[pos] == '(')
				{
					pos++;
					list.Add(new Scope
					{
						Not = flag
					});
					flag = false;
				}
				else
				{
					if (stringBuilder[pos] != '!')
					{
						break;
					}
					pos++;
					flag = true;
				}
				SkipWhitespace(stringBuilder, ref pos, end);
			}
			Func func = Func.Invalid;
			for (int i = 0; i < 56; i++)
			{
				if (IsSubStringEqual(stringBuilder, pos, FuncName[i]))
				{
					func = (Func)i;
					pos += FuncName[i].Length;
					break;
				}
			}
			if (func == Func.Invalid)
			{
				if (acceptStrings)
				{
					while (pos < end && stringBuilder[pos] != ':')
					{
						pos++;
					}
					return stringBuilder.ToString(num, pos - num);
				}
				return null;
			}
			SkipWhitespace(stringBuilder, ref pos, end);
			if (pos >= end || stringBuilder[pos] != '(')
			{
				return null;
			}
			pos++;
			SkipWhitespace(stringBuilder, ref pos, end);
			BaseObject baseObject = null;
			EquipmentPrototype equipmentPrototype = null;
			LiquidPrototype liquidPrototype = null;
			PropPrototype propPrototype = null;
			float num2 = 0f;
			int num3 = 0;
			if (pos >= end)
			{
				return null;
			}
			if (stringBuilder[pos] >= '1' && stringBuilder[pos] <= '9')
			{
				int num4 = stringBuilder[pos] - 49;
				if (num4 < paramResults.Count)
				{
					baseObject = paramResults[num4].Obj;
					equipmentPrototype = paramResults[num4].EquipmentProto;
					liquidPrototype = paramResults[num4].Liquid;
					propPrototype = paramResults[num4].PropProto;
					num2 = paramResults[num4].NumericVal;
					num3 = paramResults[num4].Hash;
				}
				pos++;
			}
			else if (IsSubStringEqual(stringBuilder, pos, Speaker))
			{
				if (IsSubStringEqual(stringBuilder, pos, SpeakerCommunity))
				{
					baseObject = speaker?.GetCommunity();
					pos += SpeakerCommunity.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, SpeakerLeader))
				{
					baseObject = (speaker?.GetCommunity())?.Leader;
					pos += SpeakerLeader.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, SpeakerCarrying))
				{
					baseObject = (speaker?.GetAsCharacter())?.CarryingObject;
					pos += SpeakerCarrying.Length;
				}
				else
				{
					baseObject = speaker;
					pos += Speaker.Length;
				}
			}
			else if (IsSubStringEqual(stringBuilder, pos, Listener))
			{
				if (IsSubStringEqual(stringBuilder, pos, ListenerCommunity))
				{
					baseObject = listener?.GetCommunity();
					pos += ListenerCommunity.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, ListenerLeader))
				{
					baseObject = (listener?.GetCommunity())?.Leader;
					pos += ListenerLeader.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, ListenerCarrying))
				{
					baseObject = ((speaker != null) ? listener.GetAsCharacter() : null)?.CarryingObject;
					pos += ListenerCarrying.Length;
				}
				else
				{
					baseObject = listener;
					pos += Listener.Length;
				}
			}
			else
			{
				if (!IsSubStringEqual(stringBuilder, pos, ReferringTo))
				{
					return null;
				}
				if (IsSubStringEqual(stringBuilder, pos, ReferringToCommunity))
				{
					baseObject = referringTo?.GetCommunity();
					pos += ReferringToCommunity.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, ReferringToLeader))
				{
					baseObject = (referringTo?.GetCommunity())?.Leader;
					pos += ReferringToLeader.Length;
				}
				else if (IsSubStringEqual(stringBuilder, pos, ReferringToCarrying))
				{
					baseObject = ((speaker != null) ? referringTo.GetAsCharacter() : null)?.CarryingObject;
					pos += ReferringToCarrying.Length;
				}
				else
				{
					baseObject = referringTo;
					pos += ReferringTo.Length;
				}
			}
			SkipWhitespace(stringBuilder, ref pos, end);
			if (pos >= end || stringBuilder[pos] != ')')
			{
				return null;
			}
			pos++;
			SkipWhitespace(stringBuilder, ref pos, end);
			BaseObject speaker2 = (isQuest ? null : speaker);
			BaseObject listener2 = (isQuest ? null : listener);
			bool flag2 = false;
			switch (func)
			{
			case Func.MaleCommunity:
				flag2 = baseObject != null && baseObject.GetCommunity() != null && baseObject.GetCommunity().GetGender(language) == GenderType.Male;
				break;
			case Func.FemaleCommunity:
				flag2 = baseObject != null && baseObject.GetCommunity() != null && baseObject.GetCommunity().GetGender(language) == GenderType.Female;
				break;
			case Func.Male:
				flag2 = ((baseObject != null) ? (baseObject.GetGender(language) == GenderType.Male) : ((liquidPrototype != null) ? (liquidPrototype.GetGenderInLanguage(language) == GenderType.Male) : ((equipmentPrototype != null) ? (equipmentPrototype.GetGenderInLanguage(language) == GenderType.Male) : (propPrototype != null && propPrototype.GetGenderInLanguage(language) == GenderType.Male))));
				break;
			case Func.Female:
				flag2 = ((baseObject != null) ? (baseObject.GetGender(language) == GenderType.Female) : ((liquidPrototype != null) ? (liquidPrototype.GetGenderInLanguage(language) == GenderType.Female) : ((equipmentPrototype != null) ? (equipmentPrototype.GetGenderInLanguage(language) == GenderType.Female) : (propPrototype != null && propPrototype.GetGenderInLanguage(language) == GenderType.Female))));
				break;
			case Func.Neuter:
				flag2 = ((baseObject != null) ? (baseObject.GetGender(language) == GenderType.Count) : ((liquidPrototype != null) ? (liquidPrototype.GetGenderInLanguage(language) == GenderType.Count) : ((equipmentPrototype != null) ? (equipmentPrototype.GetGenderInLanguage(language) == GenderType.Count) : (propPrototype != null && propPrototype.GetGenderInLanguage(language) == GenderType.Count))));
				break;
			case Func.Plural:
				flag2 = baseObject?.IsPlural() ?? equipmentPrototype?.UsePluralIndefiniteArticle ?? IsPlural(num2, language);
				break;
			case Func.Uncountable:
				flag2 = baseObject?.IsUncountable() ?? equipmentPrototype?.Uncountable ?? false;
				break;
			case Func.Many:
				flag2 = baseObject?.IsMany() ?? equipmentPrototype?.UsePluralIndefiniteArticle ?? (num2 >= 5f);
				break;
			case Func.Zero:
				flag2 = baseObject?.IsZero() ?? (num2 == 0f);
				break;
			case Func.Someone:
				flag2 = num3 == Speech.SPEECH_Someone;
				break;
			case Func.Stranger:
				flag2 = num3 == Character.NAME_Stranger;
				break;
			case Func.Community:
				flag2 = baseObject is Community;
				break;
			case Func.Speaker:
				flag2 = baseObject != null && baseObject == speaker;
				break;
			case Func.Listener:
				flag2 = baseObject != null && baseObject == listener;
				break;
			case Func.ThirdPerson:
				flag2 = baseObject is Character && baseObject != speaker && baseObject != listener;
				break;
			case Func.SpeakerCommunity:
				flag2 = baseObject != null && speaker != null && baseObject.GetCommunity() == speaker.GetCommunity();
				break;
			case Func.ListenerCommunity:
				flag2 = baseObject != null && listener != null && baseObject.GetCommunity() == listener.GetCommunity();
				break;
			case Func.ThirdPersonCommunity:
				flag2 = baseObject is Community && (speaker == null || baseObject != speaker.GetCommunity()) && (listener == null || baseObject != listener.GetCommunity());
				break;
			case Func.Alive:
				flag2 = baseObject is Character && ((Character)baseObject).AliveAndNotZombie;
				break;
			case Func.Liquid:
				flag2 = liquidPrototype != null || (baseObject is Equipment && ((Equipment)baseObject).GetLiquidContentsType() != null);
				break;
			case Func.Possessive:
				return Possessive(englishOnly, baseObject, speaker2, listener2);
			case Func.PossessiveMascAnim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: false, objectIsAnimate: true);
			case Func.PossessiveMascInanim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male);
			case Func.PossessiveFem:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Female);
			case Func.PossessiveNeut:
				return Possessive(englishOnly, baseObject, speaker2, listener2);
			case Func.PossessivePluralFem:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Female, objectIsPlural: true);
			case Func.PossessivePluralNeut:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Count, objectIsPlural: true);
			case Func.PossessivePluralMascAnim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: true, objectIsAnimate: true);
			case Func.PossessivePluralMascInanim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: true);
			case Func.PossessiveFull:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Count, objectIsPlural: false, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullMascAnim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: false, objectIsAnimate: true);
			case Func.PossessiveFullMascInanim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: false, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullFem:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Female, objectIsPlural: false, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullNeut:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Count, objectIsPlural: false, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullPluralFem:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Female, objectIsPlural: true, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullPluralNeut:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Count, objectIsPlural: true, objectIsAnimate: false, fullName: true);
			case Func.PossessiveFullPluralMascAnim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: true, objectIsAnimate: true, fullName: true);
			case Func.PossessiveFullPluralMascInanim:
				return Possessive(englishOnly, baseObject, speaker2, listener2, GenderType.Male, objectIsPlural: true, objectIsAnimate: false, fullName: true);
			case Func.Locative:
				return Locative(baseObject, speaker2, listener2, englishOnly);
			case Func.Vocative:
				return Vocative(baseObject, speaker2, listener2, englishOnly);
			case Func.Accusative:
				return Accusative(baseObject, speaker2, listener2, englishOnly);
			case Func.Dative:
				return Dative(baseObject, speaker2, listener2, englishOnly);
			case Func.Instrumental:
				return Instrumental(baseObject, speaker2, listener2, englishOnly);
			case Func.Causal:
				return Causal(baseObject, speaker2, listener2, englishOnly);
			case Func.Sublative:
				return Sublative(baseObject, speaker2, listener2, englishOnly);
			case Func.Delative:
				return Delative(baseObject, speaker2, listener2, englishOnly);
			case Func.Ablative:
				return Ablative(baseObject, speaker2, listener2, englishOnly);
			case Func.LocativeFull:
				return Locative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.VocativeFull:
				return Vocative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.AccusativeFull:
				return Accusative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.DativeFull:
				return Dative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.InstrumentalFull:
				return Instrumental(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.CausalFull:
				return Causal(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.SublativeFull:
				return Sublative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.DelativeFull:
				return Delative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			case Func.AblativeFull:
				return Ablative(baseObject, speaker2, listener2, englishOnly, fullName: true);
			}
			if (flag)
			{
				flag2 = !flag2;
			}
			int num5 = list.Count - 1;
			ApplyOpToScope(list, num5, flag2);
			while (pos < end && stringBuilder[pos] == ')')
			{
				pos++;
				if (list.Count <= 1)
				{
					return null;
				}
				ApplyOpToScope(list, list.Count - 2, list[num5].Not ^ list[num5].Result);
				list.RemoveAt(num5);
				num5--;
				SkipWhitespace(stringBuilder, ref pos, end);
			}
			if (pos >= end)
			{
				break;
			}
			if (stringBuilder[pos] == '&')
			{
				pos++;
				Scope value = list[num5];
				value.Op = OpType.And;
				list[num5] = value;
				continue;
			}
			if (stringBuilder[pos] != '|')
			{
				break;
			}
			pos++;
			Scope value2 = list[num5];
			value2.Op = OpType.Or;
			list[num5] = value2;
		}
		if (list.Count != 1)
		{
			return null;
		}
		bool result = list[0].Result;
		if (pos >= end || stringBuilder[pos] != '?')
		{
			return null;
		}
		pos++;
		string text = ParseFormula(stringBuilder, ref pos, end, acceptStrings: true, paramResults, speaker, listener, referringTo, englishOnly);
		if (text == null)
		{
			return null;
		}
		if (pos >= end)
		{
			if (!result)
			{
				return string.Empty;
			}
			return text;
		}
		if (pos >= end || stringBuilder[pos] != ':')
		{
			return null;
		}
		pos++;
		string text2 = ParseFormula(stringBuilder, ref pos, end, acceptStrings: true, paramResults, speaker, listener, referringTo, englishOnly);
		if (text2 == null)
		{
			return null;
		}
		if (!result)
		{
			return text2;
		}
		return text;
	}

	private static string Instrumental(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				return language switch
				{
					Language.Czech => CzechInstrumental_Me, 
					Language.Hungarian => HungarianInstrumental_Me, 
					Language.Russian => RussianInstrumental_Me, 
					Language.Ukrainian => UkrainianInstrumental_Me, 
					_ => GameImpl.Translate(Speech.SPEECH_Me, englishOnly), 
				};
			}
			if (listener == obj)
			{
				return language switch
				{
					Language.Czech => CzechInstrumental_You, 
					Language.Hungarian => HungarianInstrumental_You, 
					Language.Russian => RussianInstrumental_You, 
					Language.Ukrainian => UkrainianInstrumental_You, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			if (!KnowsName(speaker, listener, obj))
			{
				return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, obj.GetGender(language), englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			switch (language)
			{
			case Language.Hungarian:
				if (text.Length > 0)
				{
					char c2 = text[text.Length - 1];
					text = ((!IsHungarianBackVowelWord(text)) ? ((!Vowels.Contains(c2)) ? ((c2 != 'z' || text.Length <= 1 || text[text.Length - 2] != 's') ? (text + c2 + _el) : (text.Substring(0, text.Length - 2) + _sszel)) : (text + _vel)) : ((!Vowels.Contains(c2)) ? ((c2 != 'z' || text.Length <= 1 || text[text.Length - 2] != 's') ? (text + c2 + _al) : (text.Substring(0, text.Length - 2) + _sszal)) : (text + _val)));
				}
				break;
			case Language.Czech:
				if (IsNameThatSupportsCzechSuffixes(text))
				{
					string text2 = string.Empty;
					switch (obj.GetGender())
					{
					case GenderType.Male:
						text2 = CzechInstrumentalMascSuffix;
						break;
					case GenderType.Female:
						text2 = CzechInstrumentalFemSuffix;
						break;
					}
					char value = text[text.Length - 1];
					text = ((!Vowels.Contains(value)) ? (text + text2) : (text.Substring(0, text.Length - 1) + text2));
				}
				break;
			case Language.Ukrainian:
			{
				if (!IsNameThatSupportsUkrainianSuffixes(text))
				{
					break;
				}
				char c = text[text.Length - 1];
				switch (obj.GetGender())
				{
				case GenderType.Male:
					switch (c)
					{
					case 'ь':
						text = text.Substring(0, text.Length - 1) + UkrainianInstrumentalSuffix_Masc2;
						break;
					case 'й':
						text = text.Substring(0, text.Length - 1) + UkrainianInstrumentalSuffix_Masc3;
						break;
					default:
						if (UkrainianConsonants.Contains(c))
						{
							text += UkrainianInstrumentalSuffix_Masc1;
						}
						break;
					}
					break;
				case GenderType.Female:
					switch (c)
					{
					case 'а':
						text = text.Substring(0, text.Length - 1) + UkrainianInstrumentalSuffix_Fem1;
						break;
					case 'я':
						text = text.Substring(0, text.Length - 1) + UkrainianInstrumentalSuffix_Fem2;
						break;
					}
					break;
				}
				break;
			}
			}
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified), character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechInstrumental_Us, 
					Language.Hungarian => HungarianInstrumental_Us, 
					Language.Russian => RussianInstrumental_Us, 
					Language.Ukrainian => UkrainianInstrumental_Us, 
					_ => GameImpl.Translate(Speech.SPEECH_Us, englishOnly), 
				};
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechInstrumental_YouPl, 
					Language.Hungarian => HungarianInstrumental_YouPl, 
					Language.Russian => RussianInstrumental_YouPl, 
					Language.Ukrainian => UkrainianInstrumental_YouPl, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		return text;
	}

	private static string Locative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		Character character = obj as Character;
		string text;
		if (character != null)
		{
			if (speaker == obj)
			{
				return GameImpl.Translate(Speech.SPEECH_Me, englishOnly);
			}
			if (listener == obj)
			{
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			if (!KnowsName(speaker, listener, character))
			{
				return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, character.GetGender(), englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified), character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else
		{
			if (obj == null)
			{
				return GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		switch (language)
		{
		case Language.Czech:
			if (IsNameThatSupportsCzechSuffixes(text))
			{
				char value = text[text.Length - 1];
				text = ((!Vowels.Contains(value)) ? (text + CzechLocativeSuffix) : (text.Substring(0, text.Length - 1) + CzechLocativeSuffix));
			}
			break;
		case Language.Ukrainian:
		{
			if (!IsNameThatSupportsUkrainianSuffixes(text))
			{
				break;
			}
			char c = text[text.Length - 1];
			switch (obj.GetGender(language))
			{
			case GenderType.Male:
				if (UkrainianConsonants.Contains(c))
				{
					text += ((character != null) ? UkrainianLocativeSuffix_Living_Masc : UkrainianLocativeSuffix_Object_Masc);
				}
				break;
			case GenderType.Female:
				switch (c)
				{
				case 'а':
					text = text.Substring(0, text.Length - 1) + "і";
					break;
				case 'я':
					text = text.Substring(0, text.Length - 1) + "і";
					break;
				}
				break;
			}
			break;
		}
		}
		return text;
	}

	private static string Vocative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		if (!KnowsName(speaker, listener, obj))
		{
			return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, obj.GetGender(language), englishOnly);
		}
		string text;
		if (obj is Character character)
		{
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			switch (language)
			{
			case Language.Czech:
				if (IsNameThatSupportsCzechSuffixes(text))
				{
					string text2 = string.Empty;
					switch (obj.GetGender())
					{
					case GenderType.Male:
						text2 = CzechVocativeMascSuffix;
						break;
					case GenderType.Female:
						text2 = CzechVocativeFemSuffix;
						break;
					}
					char value = text[text.Length - 1];
					text = ((!Vowels.Contains(value)) ? (text + text2) : (text.Substring(0, text.Length - 1) + text2));
				}
				break;
			case Language.Ukrainian:
			{
				if (!IsNameThatSupportsUkrainianSuffixes(text))
				{
					break;
				}
				char c = text[text.Length - 1];
				switch (obj.GetGender(language))
				{
				case GenderType.Male:
					switch (c)
					{
					case 'а':
						text = text.Substring(0, text.Length - 1) + "о";
						break;
					case 'ь':
						text = text.Substring(0, text.Length - 1) + "ю";
						break;
					case 'й':
						text = text.Substring(0, text.Length - 1) + "ю";
						break;
					case 'я':
						text = text.Substring(0, text.Length - 1) + "ю";
						break;
					case 'o':
						text = text.Substring(0, text.Length - 1) + "е";
						break;
					case 'х':
						text = text.Substring(0, text.Length - 1) + "у";
						break;
					case 'г':
						text = text.Substring(0, text.Length - 1) + "у";
						break;
					default:
						if (UkrainianConsonants.Contains(c))
						{
							text = text.Substring(0, text.Length - 1) + "е";
						}
						break;
					}
					break;
				case GenderType.Female:
					switch (c)
					{
					case 'а':
						text = text.Substring(0, text.Length - 1) + "о";
						break;
					case 'я':
						text = ((text.Length < 2 || !Vowels.Contains(text[text.Length - 2])) ? (text.Substring(0, text.Length - 1) + "ю") : (text.Substring(0, text.Length - 1) + "є"));
						break;
					}
					break;
				}
				break;
			}
			}
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else
		{
			text = ((obj == null) ? GameImpl.Translate(Speech.SPEECH_Someone, englishOnly) : obj.GetDisplayNameString(noStrangers: true, englishOnly));
		}
		return text;
	}

	private static string Accusative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				return language switch
				{
					Language.Czech => CzechAccusative_Me, 
					Language.German => GermanAccusative_Me, 
					Language.Hungarian => HungarianAccusative_Me, 
					Language.Russian => RussianAccusative_Me, 
					Language.Ukrainian => UkrainianAccusative_Me, 
					_ => GameImpl.Translate(Speech.SPEECH_Me, englishOnly), 
				};
			}
			if (listener == obj)
			{
				return language switch
				{
					Language.Czech => CzechAccusative_You, 
					Language.German => GermanAccusative_You, 
					Language.Hungarian => HungarianAccusative_You, 
					Language.Russian => RussianAccusative_You, 
					Language.Ukrainian => UkrainianAccusative_You, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			if (!KnowsName(speaker, listener, character))
			{
				return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, character.GetGender(), englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (language == Language.Czech && IsNameThatSupportsCzechSuffixes(text))
			{
				string text2 = string.Empty;
				switch (obj.GetGender())
				{
				case GenderType.Male:
					text2 = CzechAccusativeMascSuffix;
					break;
				case GenderType.Female:
					text2 = CzechAccusativeFemSuffix;
					break;
				}
				char value = text[text.Length - 1];
				text = ((!Vowels.Contains(value)) ? (text + text2) : (text.Substring(0, text.Length - 1) + text2));
			}
			else if (language == Language.Ukrainian && IsNameThatSupportsUkrainianSuffixes(text))
			{
				char c = text[text.Length - 1];
				switch (obj.GetGender())
				{
				case GenderType.Male:
					switch (c)
					{
					case 'ь':
						text = text.Substring(0, text.Length - 1) + "я";
						break;
					case 'й':
						text = text.Substring(0, text.Length - 1) + "я";
						break;
					case 'я':
						text = text.Substring(0, text.Length - 1) + "ю";
						break;
					case 'o':
						text = text.Substring(0, text.Length - 1) + "а";
						break;
					default:
						if (UkrainianConsonants.Contains(c))
						{
							text += "а";
						}
						break;
					}
					break;
				case GenderType.Female:
					switch (c)
					{
					case 'а':
						text = text.Substring(0, text.Length - 1) + "у";
						break;
					case 'я':
						text = text.Substring(0, text.Length - 1) + "ю";
						break;
					}
					break;
				}
			}
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechAccusative_Us, 
					Language.German => GermanAccusative_Us, 
					Language.Hungarian => HungarianAccusative_Us, 
					Language.Russian => RussianAccusative_Us, 
					Language.Ukrainian => UkrainianAccusative_Us, 
					_ => GameImpl.Translate(Speech.SPEECH_Us, englishOnly), 
				};
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechAccusative_YouPl, 
					Language.German => GermanAccusative_YouPl, 
					Language.Hungarian => HungarianAccusative_YouPl, 
					Language.Russian => RussianAccusative_YouPl, 
					Language.Ukrainian => UkrainianAccusative_YouPl, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian && text.Length > 1)
		{
			char c2 = text[text.Length - 1];
			_ = text[text.Length - 2];
			text = ((Vowels.Contains(c2) || c2 == 's' || c2 == 'l' || c2 == 'r' || c2 == 'n' || c2 == 'j' || c2 == 'y') ? (text + HungarianAccusativeSuffix_t) : (IsHungarianBackVowelWord(text) ? (text + HungarianAccusativeSuffix_ot) : (IsHungarianUnroundedFrontVowelWord(text) ? (text + HungarianAccusativeSuffix_et) : ((!IsHungarianRoundedFrontVowelWord(text)) ? (text + HungarianAccusativeSuffix_t) : (text + HungarianAccusativeSuffix_öt)))));
		}
		return text;
	}

	public static bool KnowsName(BaseObject speaker, BaseObject listener, BaseObject sub)
	{
		if (sub == null)
		{
			return true;
		}
		if (sub == speaker)
		{
			return true;
		}
		if (sub == listener)
		{
			return true;
		}
		if (speaker != null && sub == speaker.GetCommunity())
		{
			return true;
		}
		if (listener != null && sub == listener.GetCommunity())
		{
			return true;
		}
		if (!(sub is Character character))
		{
			return true;
		}
		if (character.NameKnown)
		{
			return true;
		}
		if (!(speaker is Character character2))
		{
			return true;
		}
		if (character2.KnowsName(character))
		{
			return true;
		}
		return false;
	}

	private static string Dative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string result2;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				return language switch
				{
					Language.Czech => CzechDative_Me, 
					Language.German => GermanDative_Me, 
					Language.Hungarian => HungarianDative_Me, 
					Language.Russian => RussianDative_Me, 
					Language.Ukrainian => UkrainianDative_Me, 
					_ => GameImpl.Translate(Speech.SPEECH_Me, englishOnly), 
				};
			}
			if (listener == obj)
			{
				return language switch
				{
					Language.Czech => CzechDative_You, 
					Language.German => GermanDative_You, 
					Language.Hungarian => HungarianDative_You, 
					Language.Russian => RussianDative_You, 
					Language.Ukrainian => UkrainianDative_You, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			string result;
			if (!KnowsName(speaker, listener, character))
			{
				if (character.GetGender() == GenderType.Female && GameImpl.Instance.TryTranslate(Speech.SPEECH_Someone_Female_Dat, out result, englishOnly))
				{
					return result;
				}
				if (GameImpl.Instance.TryTranslate(Speech.SPEECH_Someone_Dat, out result, englishOnly))
				{
					return result;
				}
				return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, character.GetGender(), englishOnly);
			}
			result2 = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (GameImpl.Instance.TryTranslate(JenkinsHash(character.FirstName + _Dative), out result, englishOnly))
			{
				result2 = result;
			}
			else if (language == Language.Czech && IsNameThatSupportsCzechSuffixes(result2))
			{
				string text = string.Empty;
				switch (obj.GetGender())
				{
				case GenderType.Male:
					text = CzechDativeMascSuffix;
					break;
				case GenderType.Female:
					text = CzechDativeFemSuffix;
					break;
				}
				char value = result2[result2.Length - 1];
				result2 = ((!Vowels.Contains(value)) ? (result2 + text) : (result2.Substring(0, result2.Length - 1) + text));
			}
			else if (language == Language.Ukrainian && IsNameThatSupportsUkrainianSuffixes(result2))
			{
				char c = result2[result2.Length - 1];
				switch (obj.GetGender())
				{
				case GenderType.Male:
					if (c == 'й')
					{
						result2 = result2.Substring(0, result2.Length - 1) + "ю";
					}
					else if (UkrainianConsonants.Contains(c))
					{
						result2 += "у";
					}
					break;
				case GenderType.Female:
					switch (c)
					{
					case 'а':
						result2 = result2.Substring(0, result2.Length - 1) + "і";
						break;
					case 'я':
						result2 = result2.Substring(0, result2.Length - 1) + "ї";
						break;
					}
					break;
				}
			}
			else if (language == Language.Russian && result2.Length >= 2)
			{
				char c2 = result2[result2.Length - 1];
				char c3 = result2[result2.Length - 2];
				switch (obj.GetGender())
				{
				case GenderType.Male:
					if (c3 == 'и' && c2 == 'я')
					{
						result2 = result2.Substring(0, result2.Length - 1) + "и";
						break;
					}
					switch (c2)
					{
					case 'а':
					case 'я':
						result2 = result2.Substring(0, result2.Length - 1) + "е";
						break;
					case 'й':
					case 'ь':
						result2 = result2.Substring(0, result2.Length - 1) + "ю";
						break;
					default:
						if (RussianConsonants.Contains(c2))
						{
							result2 += "у";
						}
						break;
					}
					break;
				case GenderType.Female:
					if (c3 == 'и' && c2 == 'я')
					{
						result2 = result2.Substring(0, result2.Length - 1) + "и";
					}
					else if (c2 == 'а' || c2 == 'я')
					{
						result2 = result2.Substring(0, result2.Length - 1) + "е";
					}
					break;
				}
			}
			if (fullName)
			{
				string text2 = GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
				if (character.GetGender() == GenderType.Female && GameImpl.Instance.TryTranslate(JenkinsHash(character.Surname + F + _Dative), out result, englishOnly))
				{
					text2 = result;
				}
				else if (GameImpl.Instance.TryTranslate(JenkinsHash(character.Surname + _Dative), out result, englishOnly))
				{
					text2 = result;
				}
				else if (language == Language.Russian)
				{
					char c4 = text2[text2.Length - 1];
					char c5 = text2[text2.Length - 2];
					if ((c5 == 'и' && c4 == 'а') || (c5 == 'и' && c4 == 'я'))
					{
						text2 = text2.Substring(0, text2.Length - 1) + "и";
					}
					else
					{
						switch (c4)
						{
						case 'а':
						case 'я':
							text2 = text2.Substring(0, text2.Length - 1) + "е";
							break;
						case 'й':
						case 'ь':
							text2 = text2.Substring(0, text2.Length - 1) + "ю";
							break;
						default:
							if (RussianConsonants.Contains(c4))
							{
								text2 += "у";
							}
							break;
						}
					}
				}
				result2 = result2 + " " + text2;
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechDative_Us, 
					Language.German => GermanDative_Us, 
					Language.Hungarian => HungarianDative_Us, 
					Language.Russian => RussianDative_Us, 
					Language.Ukrainian => UkrainianDative_Us, 
					_ => GameImpl.Translate(Speech.SPEECH_Us, englishOnly), 
				};
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				return language switch
				{
					Language.Czech => CzechDative_YouPl, 
					Language.German => GermanDative_YouPl, 
					Language.Hungarian => HungarianDative_YouPl, 
					Language.Russian => RussianDative_YouPl, 
					Language.Ukrainian => UkrainianDative_YouPl, 
					_ => GameImpl.Translate(Speech.SPEECH_You, englishOnly), 
				};
			}
			result2 = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else if (!GameImpl.Instance.TryTranslate(Speech.SPEECH_Someone_Dat, out result2, englishOnly))
		{
			result2 = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian)
		{
			if (IsHungarianBackVowelWord(result2))
			{
				result2 += HungarianDativeSuffix_nak;
			}
			else if (IsHungarianFrontVowelWord(result2))
			{
				result2 += HungarianDativeSuffix_nek;
			}
		}
		return result2;
	}

	public static void ApplyCzechPossessiveSuffixToName(StringBuilder stringBuilder, int end, GenderType subjectGender, GenderType objectGender = GenderType.Count, bool objectIsPlural = false, bool objectIsAnimate = false, bool genitive = false, bool surname = false)
	{
		if (genitive)
		{
			char c = ((end > 0) ? stringBuilder[end - 1] : ' ');
			switch (subjectGender)
			{
			case GenderType.Male:
				if (c == 'y' || c == 'i' || c == 'e')
				{
					stringBuilder.Insert(end, CzechGenitiveSuffix_Masc_y);
				}
				else if (Vowels.Contains(c))
				{
					stringBuilder[end - 1] = 'a';
				}
				else
				{
					stringBuilder.Insert(end, CzechGenitiveSuffix_Masc);
				}
				break;
			case GenderType.Female:
				if (c == 'a')
				{
					stringBuilder[end - 1] = 'y';
				}
				else if (surname)
				{
					if (c == 'o')
					{
						stringBuilder.Insert(end, CzechGenitiveSuffix_Fem_o);
					}
					else
					{
						stringBuilder.Insert(end, CzechGenitiveSuffix_Fem);
					}
				}
				break;
			}
			return;
		}
		if (end > 0 && Vowels.Contains(stringBuilder[end - 1]))
		{
			stringBuilder.Remove(end - 1, 1);
			end--;
		}
		switch (subjectGender)
		{
		case GenderType.Male:
			switch (objectGender)
			{
			case GenderType.Male:
				stringBuilder.Insert(end, CzechPossessiveSuffix_MascSub_MascOb);
				break;
			case GenderType.Female:
				stringBuilder.Insert(end, CzechPossessiveSuffix_MascSub_FemOb);
				break;
			case GenderType.Count:
				stringBuilder.Insert(end, CzechPossessiveSuffix_MascSub_NeutOb);
				break;
			}
			break;
		case GenderType.Female:
			switch (objectGender)
			{
			case GenderType.Male:
				stringBuilder.Insert(end, CzechPossessiveSuffix_FemSub_MascOb);
				break;
			case GenderType.Female:
				stringBuilder.Insert(end, CzechPossessiveSuffix_FemSub_FemOb);
				break;
			case GenderType.Count:
				stringBuilder.Insert(end, CzechPossessiveSuffix_FemSub_NeutOb);
				break;
			}
			break;
		}
	}

	public static void ApplyUkrainianPossessiveSuffixToName(StringBuilder stringBuilder, int end, GenderType gender)
	{
		ApplyUkrainianPossessiveSuffixToName(stringBuilder, 0, end, gender, surname: false, null);
	}

	public static void ApplyUkrainianPossessiveSuffixToName(StringBuilder stringBuilder, int start, int end, GenderType gender, bool surname, string untranslatedName)
	{
		if (stringBuilder.Length == 0)
		{
			return;
		}
		if (untranslatedName != null)
		{
			string result;
			if (gender == GenderType.Female && surname)
			{
				SB2.Length = 0;
				SB2.Append(untranslatedName);
				SB2.Append(F);
				SB2.Append(_Possessive);
				if (GameImpl.Instance.TryTranslate(JenkinsHash(SB2), out result, englishOnly: false))
				{
					stringBuilder.Remove(start, end - start);
					stringBuilder.Insert(start, result);
					return;
				}
			}
			SB2.Length = 0;
			SB2.Append(untranslatedName);
			SB2.Append(_Possessive);
			if (GameImpl.Instance.TryTranslate(JenkinsHash(SB2), out result, englishOnly: false))
			{
				stringBuilder.Remove(start, end - start);
				stringBuilder.Insert(start, result);
				return;
			}
		}
		int index = end - 1;
		char c = stringBuilder[index];
		switch (gender)
		{
		case GenderType.Male:
			switch (c)
			{
			case 'й':
				stringBuilder[index] = 'я';
				break;
			case 'ь':
				stringBuilder[index] = 'я';
				break;
			default:
				if (UkrainianConsonants.Contains(c))
				{
					stringBuilder.Insert(end, 'а');
				}
				break;
			}
			break;
		case GenderType.Female:
			switch (c)
			{
			case 'а':
				stringBuilder[index] = 'и';
				break;
			case 'я':
				stringBuilder[index] = 'і';
				break;
			default:
				if (UkrainianConsonants.Contains(c))
				{
					stringBuilder.Insert(end, 'і');
				}
				break;
			}
			break;
		}
	}

	public static void ApplyRussianPossessiveSuffixToName(StringBuilder stringBuilder, int end, GenderType gender)
	{
		ApplyRussianPossessiveSuffixToName(stringBuilder, 0, end, gender, surname: false, null);
	}

	public static void ApplyRussianPossessiveSuffixToName(StringBuilder stringBuilder, int start, int end, GenderType gender, bool surname, string untranslatedName)
	{
		if (stringBuilder.Length == 0 || end <= 1)
		{
			return;
		}
		if (untranslatedName != null)
		{
			string result;
			if (gender == GenderType.Female && surname)
			{
				SB2.Length = 0;
				SB2.Append(untranslatedName);
				SB2.Append(F);
				SB2.Append(_Possessive);
				if (GameImpl.Instance.TryTranslate(JenkinsHash(SB2), out result, englishOnly: false))
				{
					stringBuilder.Remove(start, end - start);
					stringBuilder.Insert(start, result);
					return;
				}
			}
			SB2.Length = 0;
			SB2.Append(untranslatedName);
			SB2.Append(_Possessive);
			if (GameImpl.Instance.TryTranslate(JenkinsHash(SB2), out result, englishOnly: false))
			{
				stringBuilder.Remove(start, end - start);
				stringBuilder.Insert(start, result);
				return;
			}
		}
		int num = end - 1;
		char c = stringBuilder[num];
		char c2 = stringBuilder[num - 1];
		if (surname)
		{
			switch (c)
			{
			case 'й':
				stringBuilder[num] = 'ю';
				return;
			case 'ь':
				stringBuilder[num] = 'ю';
				return;
			case 'я':
				stringBuilder[num] = 'е';
				return;
			case 'а':
				stringBuilder[num] = 'е';
				return;
			}
			if (c2 == 'и' && c == 'я')
			{
				stringBuilder[num] = 'и';
			}
			else if (c2 == 'и' && c == 'а')
			{
				stringBuilder[num] = 'и';
			}
			else if (RussianConsonants.Contains(c))
			{
				stringBuilder.Insert(end, 'у');
			}
			return;
		}
		switch (gender)
		{
		case GenderType.Male:
			switch (c)
			{
			case 'й':
				stringBuilder[num] = 'я';
				break;
			case 'ь':
				stringBuilder[num] = 'я';
				break;
			case 'я':
				stringBuilder[num] = 'и';
				break;
			case 'а':
				if (c2 == 'г' || c2 == 'к' || c2 == 'х' || c2 == 'ж' || c2 == 'ш')
				{
					stringBuilder[num] = 'и';
				}
				else
				{
					stringBuilder[num] = 'ы';
				}
				break;
			default:
				if (RussianConsonants.Contains(c))
				{
					stringBuilder.Insert(end, 'а');
				}
				break;
			}
			break;
		case GenderType.Female:
			switch (c)
			{
			case 'а':
				if (c2 == 'г' || c2 == 'к' || c2 == 'х' || c2 == 'ж' || c2 == 'ш')
				{
					stringBuilder[num] = 'и';
				}
				else
				{
					stringBuilder[num] = 'ы';
				}
				break;
			case 'я':
				stringBuilder[num] = 'и';
				break;
			}
			break;
		case GenderType.Count:
			switch (c)
			{
			case 'о':
				stringBuilder[num] = 'а';
				break;
			case 'е':
				stringBuilder[num] = 'я';
				break;
			}
			break;
		}
	}

	public static string Possessive(bool englishOnly, BaseObject sub, BaseObject speaker, BaseObject listener, GenderType objectGender = GenderType.Count, bool objectIsPlural = false, bool objectIsAnimate = false, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		if (!KnowsName(speaker, listener, sub))
		{
			if (sub.GetGender(language) == GenderType.Female && GameImpl.Instance.TryTranslate(Speech.SPEECH_Someone_Female_Pos, out var result, englishOnly))
			{
				return result;
			}
			if (GameImpl.Instance.TryTranslate(Speech.SPEECH_Someone_Pos, out result, englishOnly))
			{
				return result;
			}
			return GameImpl.Translate(Speech.SPEECH_Someone, Speech.SPEECH_Someone_Female, sub.GetGender(language), englishOnly);
		}
		switch (language)
		{
		case Language.Czech:
			if (sub != null)
			{
				if (sub == speaker)
				{
					return objectGender switch
					{
						GenderType.Male => CzechMyMasc, 
						GenderType.Female => CzechMyFem, 
						_ => CzechMyNeut, 
					};
				}
				if (sub == listener)
				{
					return objectGender switch
					{
						GenderType.Male => CzechYourMasc, 
						GenderType.Female => CzechYourFem, 
						_ => CzechYourNeut, 
					};
				}
				if (speaker != null && sub == speaker.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => CzechOurMasc, 
						GenderType.Female => CzechOurFem, 
						_ => CzechOurNeut, 
					};
				}
				if (listener != null && sub == listener.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => CzechYourPlMasc, 
						GenderType.Female => CzechYourPlFem, 
						_ => CzechYourPlNeut, 
					};
				}
				SB.Length = 0;
				if (sub is Character character6)
				{
					SB.Append(GameImpl.TranslateName(character6.FirstName, englishOnly, character6.FirstNameVerified));
					ApplyCzechPossessiveSuffixToName(SB, SB.Length, character6.GetGender(language), objectGender, objectIsPlural, objectIsAnimate, fullName);
					if (fullName)
					{
						SB.Append(' ');
						SB.Append(GameImpl.TranslateSurname(character6.Surname, character6.GetGender(language), englishOnly, character6.SurnameVerified));
						ApplyCzechPossessiveSuffixToName(SB, SB.Length, character6.GetGender(language), objectGender, objectIsPlural, objectIsAnimate, genitive: true, surname: true);
					}
				}
				else
				{
					sub.BuildDisplayName(SB, noStrangers: true, englishOnly);
					ApplyCzechPossessiveSuffixToName(SB, SB.Length, sub.GetGender(language), objectGender, objectIsPlural, objectIsAnimate, genitive: true);
				}
				return SB.ToString();
			}
			return CzechIts;
		case Language.Russian:
			if (sub != null)
			{
				if (sub == speaker)
				{
					return objectGender switch
					{
						GenderType.Male => RussianMyMasc, 
						GenderType.Female => RussianMyFem, 
						_ => RussianMyNeut, 
					};
				}
				if (sub == listener)
				{
					return objectGender switch
					{
						GenderType.Male => RussianYourMasc, 
						GenderType.Female => RussianYourFem, 
						_ => RussianYourNeut, 
					};
				}
				if (speaker != null && sub == speaker.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => RussianOurMasc, 
						GenderType.Female => RussianOurFem, 
						_ => RussianOurNeut, 
					};
				}
				if (listener != null && sub == listener.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => RussianYourPlMasc, 
						GenderType.Female => RussianYourPlFem, 
						_ => RussianYourPlNeut, 
					};
				}
				SB.Length = 0;
				if (sub is Character character3)
				{
					SB.Append(GameImpl.TranslateName(character3.FirstName, englishOnly, character3.FirstNameVerified));
					ApplyRussianPossessiveSuffixToName(SB, 0, SB.Length, sub.GetGender(language), surname: false, character3.FirstName);
					if (fullName)
					{
						SB.Append(' ');
						int length = SB.Length;
						SB.Append(GameImpl.TranslateSurname(character3.Surname, character3.GetGender(), englishOnly, character3.SurnameVerified));
						ApplyRussianPossessiveSuffixToName(SB, length, SB.Length, sub.GetGender(language), surname: true, character3.Surname);
					}
				}
				else
				{
					sub.BuildDisplayName(SB, noStrangers: true, englishOnly);
					ApplyRussianPossessiveSuffixToName(SB, 0, SB.Length, sub.GetGender(language), surname: false, null);
				}
				return SB.ToString();
			}
			return GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		case Language.Ukrainian:
			if (sub != null)
			{
				if (sub == speaker)
				{
					return objectGender switch
					{
						GenderType.Male => UkrainianMyMasc, 
						GenderType.Female => UkrainianMyFem, 
						_ => UkrainianMyNeut, 
					};
				}
				if (sub == listener)
				{
					return objectGender switch
					{
						GenderType.Male => UkrainianYourMasc, 
						GenderType.Female => UkrainianYourFem, 
						_ => UkrainianYourNeut, 
					};
				}
				if (speaker != null && sub == speaker.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => UkrainianOurMasc, 
						GenderType.Female => UkrainianOurFem, 
						_ => UkrainianOurNeut, 
					};
				}
				if (listener != null && sub == listener.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => UkrainianYourPlMasc, 
						GenderType.Female => UkrainianYourPlFem, 
						_ => UkrainianYourPlNeut, 
					};
				}
				SB.Length = 0;
				if (sub is Character character5)
				{
					SB.Append(GameImpl.TranslateName(character5.FirstName, englishOnly, character5.FirstNameVerified));
					ApplyUkrainianPossessiveSuffixToName(SB, 0, SB.Length, sub.GetGender(language), surname: false, character5.FirstName);
					if (fullName)
					{
						SB.Append(' ');
						SB.Append(GameImpl.TranslateSurname(character5.Surname, character5.GetGender(), englishOnly, character5.SurnameVerified));
						ApplyUkrainianPossessiveSuffixToName(SB, 0, SB.Length, sub.GetGender(language), surname: true, character5.Surname);
					}
				}
				else
				{
					sub.BuildDisplayName(SB, noStrangers: true, englishOnly);
					ApplyUkrainianPossessiveSuffixToName(SB, 0, SB.Length, sub.GetGender(language), surname: false, null);
				}
				return SB.ToString();
			}
			return UkrainianIts;
		case Language.German:
			if (sub != null)
			{
				if (sub == speaker)
				{
					return objectGender switch
					{
						GenderType.Male => GermanMyMasc, 
						GenderType.Female => GermanMyFem, 
						_ => GermanMyNeut, 
					};
				}
				if (sub == listener)
				{
					return objectGender switch
					{
						GenderType.Male => GermanYourMasc, 
						GenderType.Female => GermanYourFem, 
						_ => GermanYourNeut, 
					};
				}
				if (speaker != null && sub == speaker.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => GermanOurMasc, 
						GenderType.Female => GermanOurFem, 
						_ => GermanOurNeut, 
					};
				}
				if (listener != null && sub == listener.GetCommunity())
				{
					return objectGender switch
					{
						GenderType.Male => GermanYourPlMasc, 
						GenderType.Female => GermanYourPlFem, 
						_ => GermanYourPlNeut, 
					};
				}
				SB.Length = 0;
				if (sub is Character character4)
				{
					SB.Append(GameImpl.TranslateName(character4.FirstName, englishOnly, character4.FirstNameVerified));
					if (fullName)
					{
						SB.Append(' ');
						SB.Append(GameImpl.TranslateSurname(character4.Surname, character4.GetGender(), englishOnly, character4.SurnameVerified));
					}
				}
				else
				{
					sub.BuildDisplayName(SB, noStrangers: true, englishOnly);
				}
				SB.Append(GameImpl.Translate(Speech.SPEECH_Possessive, englishOnly));
				return SB.ToString();
			}
			return objectGender switch
			{
				GenderType.Male => GermanItsMasc, 
				GenderType.Female => GermanItsFem, 
				_ => GermanItsNeut, 
			};
		default:
		{
			Character character = speaker as Character;
			Character character2 = sub as Character;
			SB.Length = 0;
			if (character2 != null && (character2.NameKnown || character == null || character.KnowsName(character2) || character2 == listener))
			{
				if (sub == speaker)
				{
					return GameImpl.Translate(Speech.SPEECH_My, englishOnly);
				}
				if (sub == listener)
				{
					return GameImpl.Translate(Speech.SPEECH_Your, englishOnly);
				}
				SB.Append(GameImpl.TranslateName(character2.FirstName, englishOnly, character2.FirstNameVerified));
				if (fullName)
				{
					SB.Append(' ');
					SB.Append(GameImpl.TranslateSurname(character2.Surname, character2.GetGender(), englishOnly, character2.SurnameVerified));
				}
			}
			else if (sub is Community community)
			{
				if (speaker != null && community == speaker.GetCommunity())
				{
					return GameImpl.Translate(Speech.SPEECH_Our, englishOnly);
				}
				if (listener != null && community == listener.GetCommunity())
				{
					return GameImpl.Translate(Speech.SPEECH_Your, englishOnly);
				}
				community.BuildDisplayName(SB, noStrangers: true, englishOnly);
			}
			else if (sub is DummyBaseObject dummyBaseObject)
			{
				SB.Append(dummyBaseObject.Name);
			}
			else
			{
				SB.Append(GameImpl.Translate(Speech.SPEECH_Someone, englishOnly));
			}
			if (language == Language.English || language == Language.German)
			{
				SB.Append(GameImpl.Translate(Speech.SPEECH_Possessive, englishOnly));
			}
			return SB.ToString();
		}
		}
	}

	private static string Causal(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianCausal_Me;
				}
				return GameImpl.Translate(Speech.SPEECH_Me, englishOnly);
			}
			if (listener == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianCausal_You;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianCausal_Us;
				}
				return GameImpl.Translate(Speech.SPEECH_Us, englishOnly);
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianCausal_YouPl;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian)
		{
			text += HungarianCausalSuffix_ert;
		}
		return text;
	}

	private static string Sublative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianSublative_Me;
				}
				return GameImpl.Translate(Speech.SPEECH_Me, englishOnly);
			}
			if (listener == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianSublative_You;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianSublative_Us;
				}
				return GameImpl.Translate(Speech.SPEECH_Us, englishOnly);
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianSublative_YouPl;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			if (language == Language.Hungarian)
			{
				return HungarianSublative_Someone;
			}
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian)
		{
			if (IsHungarianBackVowelWord(text))
			{
				text += HungarianSublativeSuffix_ra;
			}
			else if (IsHungarianFrontVowelWord(text))
			{
				text += HungarianSublativeSuffix_re;
			}
		}
		return text;
	}

	private static string Delative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianDelative_Me;
				}
				return GameImpl.Translate(Speech.SPEECH_Me, englishOnly);
			}
			if (listener == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianDelative_You;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianDelative_Us;
				}
				return GameImpl.Translate(Speech.SPEECH_Us, englishOnly);
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianDelative_YouPl;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			if (language == Language.Hungarian)
			{
				return HungarianDelative_Someone;
			}
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian)
		{
			if (IsHungarianBackVowelWord(text))
			{
				text += HungarianDelativeSuffix_ról;
			}
			else if (IsHungarianFrontVowelWord(text))
			{
				text += HungarianDelativeSuffix_ről;
			}
		}
		return text;
	}

	private static string Ablative(BaseObject obj, BaseObject speaker, BaseObject listener, bool englishOnly, bool fullName = false)
	{
		Language language = ((!englishOnly) ? GameImpl.Instance.Settings.Language : Language.English);
		string text;
		if (obj is Character character)
		{
			if (speaker == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianAblative_Me;
				}
				return GameImpl.Translate(Speech.SPEECH_Me, englishOnly);
			}
			if (listener == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianAblative_You;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = GameImpl.TranslateName(character.FirstName, englishOnly, character.FirstNameVerified);
			if (fullName)
			{
				text = text + " " + GameImpl.TranslateSurname(character.Surname, character.GetGender(), englishOnly, character.SurnameVerified);
			}
		}
		else if (obj != null)
		{
			if (speaker != null && speaker.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianAblative_Us;
				}
				return GameImpl.Translate(Speech.SPEECH_Us, englishOnly);
			}
			if (listener != null && listener.GetCommunity() == obj)
			{
				if (language == Language.Hungarian)
				{
					return HungarianAblative_YouPl;
				}
				return GameImpl.Translate(Speech.SPEECH_You, englishOnly);
			}
			text = obj.GetDisplayNameString(noStrangers: true, englishOnly);
		}
		else
		{
			if (language == Language.Hungarian)
			{
				return HungarianAblative_Someone;
			}
			text = GameImpl.Translate(Speech.SPEECH_Someone, englishOnly);
		}
		if (language == Language.Hungarian)
		{
			if (IsHungarianBackVowelWord(text))
			{
				text += HungarianAblativeSuffix_tól;
			}
			else if (IsHungarianFrontVowelWord(text))
			{
				text += HungarianAblativeSuffix_től;
			}
		}
		return text;
	}

	private static bool IsHungarianBackVowelWord(string s)
	{
		bool result = false;
		for (int num = s.Length - 1; num >= 0; num--)
		{
			if (HungarianBackVowels.Contains(s[num]))
			{
				result = true;
				break;
			}
			if (HungarianFrontVowels.Contains(s[num]))
			{
				result = false;
				break;
			}
			if (HungarianIntermediateVowels.Contains(s[num]))
			{
				result = true;
			}
		}
		return result;
	}

	private static bool IsHungarianFrontVowelWord(string s)
	{
		for (int num = s.Length - 1; num >= 0; num--)
		{
			if (HungarianFrontVowels.Contains(s[num]))
			{
				return true;
			}
			if (HungarianBackVowels.Contains(s[num]))
			{
				return false;
			}
		}
		return false;
	}

	private static bool IsHungarianRoundedFrontVowelWord(string s)
	{
		for (int num = s.Length - 1; num >= 0; num--)
		{
			if (HungarianRoundedFrontVowels.Contains(s[num]))
			{
				return true;
			}
			if (HungarianBackVowels.Contains(s[num]) || HungarianUnroundedFrontVowels.Contains(s[num]))
			{
				return false;
			}
		}
		return false;
	}

	private static bool IsHungarianUnroundedFrontVowelWord(string s)
	{
		for (int num = s.Length - 1; num >= 0; num--)
		{
			if (HungarianUnroundedFrontVowels.Contains(s[num]))
			{
				return true;
			}
			if (HungarianBackVowels.Contains(s[num]) || HungarianRoundedFrontVowels.Contains(s[num]))
			{
				return false;
			}
		}
		return false;
	}

	private static bool IsNameThatSupportsCzechSuffixes(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		if (name[name.Length - 1] == 'x')
		{
			return false;
		}
		return true;
	}

	private static bool IsNameThatSupportsUkrainianSuffixes(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return false;
		}
		foreach (char c in name)
		{
			if (c < 'Ѐ' || c > 'ӿ')
			{
				return false;
			}
		}
		return true;
	}

	private static void SkipWhitespace(StringBuilder stringBuilder, ref int pos, int end)
	{
		while (pos < end && stringBuilder[pos] == ' ')
		{
			pos++;
		}
	}

	public static bool IsSubStringEqual(StringBuilder stringBuilder, int pos, string other)
	{
		int length = other.Length;
		if (stringBuilder.Length - pos < length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			if (stringBuilder[pos + i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsSubStringEqual(StringBuilder stringBuilder, int pos, int end, string other)
	{
		int length = other.Length;
		if (stringBuilder.Length - pos < length)
		{
			return false;
		}
		if (end - pos != length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			if (stringBuilder[pos + i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	public static void SetUnityTextButtonPrompt(TextMeshProUGUI unityText, InputFunction inputFunction)
	{
		sb.Length = 0;
		sb.AppendButtonPromptString(inputFunction);
		unityText.SetUnityTextIfDifferent(sb);
	}

	public static string ToBytesString(this long bytes, int decimalPlaces = 1)
	{
		if (bytes < 0)
		{
			return "-" + (-bytes).ToBytesString(decimalPlaces);
		}
		double num = Math.Round((double)bytes / 1073741824.0, decimalPlaces);
		double num2 = Math.Round((double)bytes / 1048576.0, decimalPlaces);
		double num3 = Math.Round((double)bytes / 1024.0, decimalPlaces);
		if (!(num > 1.0))
		{
			if (!(num2 > 1.0))
			{
				if (!(num3 > 1.0))
				{
					return bytes + " " + GameImpl.Translate("MENU_Bytes");
				}
				return num3 + GameImpl.Translate("MENU_Kilobytes");
			}
			return num2 + GameImpl.Translate("MENU_Megabytes");
		}
		return num + GameImpl.Translate("MENU_Gigabytes");
	}
}
