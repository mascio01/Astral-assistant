using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LanguageMenu : BaseMenu
{
	private struct LanguageName : IComparable<LanguageName>
	{
		public Language Language;

		public string Name;

		public int CompareTo(LanguageName other)
		{
			if (GameImpl.IsCJKLanguage(Language) && !GameImpl.IsCJKLanguage(other.Language))
			{
				return -1;
			}
			if (!GameImpl.IsCJKLanguage(Language) && GameImpl.IsCJKLanguage(other.Language))
			{
				return 1;
			}
			return Name.CompareTo(other.Name);
		}
	}

	private GameObject UnityLanguageList;

	private bool WantRepopulate;

	private bool WantSaveSettings;

	private List<KeyValuePair<Language, GameObject>> LanguageButtons = new List<KeyValuePair<Language, GameObject>>();

	public ActionMenu ActionMenu = new ActionMenu();

	private static int MENU_WorkInProgress = StringUtil.JenkinsHash("MENU_WorkInProgress");

	public static float LanuguageSelectMenuXOffset = -180f;

	public override ActionMenu GetActionMenu()
	{
		if (!(ChildMenu != null))
		{
			return ActionMenu;
		}
		return ChildMenu.GetActionMenu();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnityLanguageList = base.gameObject.FindChild("LanguageList/Viewport/Content");
		List<LanguageName> list = new List<LanguageName>();
		for (int i = 0; i < 25; i++)
		{
			LanguageName item = new LanguageName
			{
				Language = (Language)i
			};
			Language language = (Language)i;
			item.Name = GameImpl.Translate("MENU_" + language);
			list.Add(item);
		}
		list.Sort();
		for (int j = 0; j < list.Count; j++)
		{
			Language language2 = list[j].Language;
			if (GameImpl.Instance.IsLanguageSupported(language2))
			{
				GameObject value = AddMenuButton(UnityLanguageList, LanguageButtons.Count, list[j].Name, delegate
				{
					GameImpl.Instance.SetLanguage(language2);
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					WantSaveSettings = true;
					WantRepopulate = true;
				});
				LanguageButtons.Add(new KeyValuePair<Language, GameObject>(language2, value));
			}
		}
		WantRepopulate = true;
	}

	public void Populate()
	{
		WantRepopulate = false;
		for (int i = 0; i < LanguageButtons.Count; i++)
		{
			Language key = LanguageButtons[i].Key;
			TextMeshProUGUI component = LanguageButtons[i].Value.FindChild("Text").GetComponent<TextMeshProUGUI>();
			component.SetUnityText(GameImpl.Translate("MENU_" + key));
			if (GameImpl.WantLuckiestGuyFont(key))
			{
				component.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/luckiestguy SDF");
			}
		}
		UpdateWIPText();
	}

	public override void OnDeactivate(bool popped)
	{
		if (WantSaveSettings)
		{
			GameImpl.Instance.AutoSaveSettings();
			WantSaveSettings = false;
		}
		for (int i = 0; i < LanguageButtons.Count; i++)
		{
			UnityEngine.Object.Destroy(LanguageButtons[i].Value);
		}
		LanguageButtons.Clear();
		base.OnDeactivate(popped);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			Populate();
		}
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null)
		{
			for (int i = 0; i < LanguageButtons.Count; i++)
			{
				if (!IsTranslationComplete(LanguageButtons[i].Key) && LanguageButtons[i].Value == current.currentSelectedGameObject)
				{
					ActionMenu.FocusUnityObj = LanguageButtons[i].Value;
					ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_WorkInProgress), LanuguageSelectMenuXOffset, leftAligned: false));
					break;
				}
			}
		}
		ActionMenu.OnFinishAddingActions();
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}

	public static bool IsTranslationComplete(Language language)
	{
		switch (language)
		{
		case Language.English:
		case Language.Hungarian:
		case Language.Spanish:
		case Language.Russian:
		case Language.German:
		case Language.French:
		case Language.BrazilianPortuguese:
		case Language.Turkish:
		case Language.Italian:
		case Language.SimplifiedChinese:
		case Language.Japanese:
		case Language.Korean:
		case Language.Indonesian:
		case Language.Lithuanian:
		case Language.Ukrainian:
		case Language.LatinAmericanSpanish:
		case Language.Czech:
		case Language.Vietnamese:
			return true;
		default:
			return false;
		}
	}

	public static Language GetLanguageFromSteamCode(string steamLanguage)
	{
		return steamLanguage switch
		{
			"english" => Language.English, 
			"schinese" => Language.SimplifiedChinese, 
			"tchinese" => Language.TraditionalChinese, 
			"latam" => Language.LatinAmericanSpanish, 
			"spanish" => Language.Spanish, 
			"french" => Language.French, 
			"hungarian" => Language.Hungarian, 
			"russian" => Language.Russian, 
			"german" => Language.German, 
			"italian" => Language.Italian, 
			"turkish" => Language.Turkish, 
			"brazilian" => Language.BrazilianPortuguese, 
			"portuguese" => Language.Portuguese, 
			"polish" => Language.Polish, 
			"danish" => Language.Danish, 
			"dutch" => Language.Dutch, 
			"indonesian" => Language.Indonesian, 
			"japanese" => Language.Japanese, 
			"koreana" => Language.Korean, 
			"thai" => Language.Thai, 
			"ukrainian" => Language.Ukrainian, 
			"czech" => Language.Czech, 
			"vietnamese" => Language.Vietnamese, 
			_ => Language.English, 
		};
	}

	public static Language GetLanguageFromCode(string locale)
	{
		switch (locale)
		{
		case "en-US":
		case "en-GB":
			return Language.English;
		case "ja-JP":
			return Language.Japanese;
		case "ko-KR":
			return Language.Korean;
		case "zh-CN":
			return Language.SimplifiedChinese;
		case "zh-TW":
			return Language.TraditionalChinese;
		case "pt-BR":
			return Language.BrazilianPortuguese;
		case "it-IT":
			return Language.Italian;
		case "hu-HU":
			return Language.Hungarian;
		case "tr-TR":
			return Language.Turkish;
		case "fr-FR":
			return Language.French;
		case "de-DE":
			return Language.German;
		case "es-MX":
			return Language.LatinAmericanSpanish;
		case "ru-RU":
			return Language.Russian;
		case "cs-CZ":
			return Language.Czech;
		case "pl-PL":
			return Language.Polish;
		case "es-ES":
			return Language.Spanish;
		case "nl-NL":
			return Language.Dutch;
		case "lt-LT":
			return Language.Lithuanian;
		case "vi-VN":
			return Language.Vietnamese;
		default:
			return Language.English;
		}
	}

	public static string GetCodeFromLanguage(Language language)
	{
		return language switch
		{
			Language.English => "en-US", 
			Language.Japanese => "ja-JP", 
			Language.Korean => "ko-KR", 
			Language.SimplifiedChinese => "zh-CN", 
			Language.TraditionalChinese => "zh-TW", 
			Language.BrazilianPortuguese => "pt-BR", 
			Language.Italian => "it-IT", 
			Language.Hungarian => "hu-HU", 
			Language.Turkish => "tr-TR", 
			Language.French => "fr-FR", 
			Language.German => "de-DE", 
			Language.LatinAmericanSpanish => "es-MX", 
			Language.Russian => "ru-RU", 
			Language.Czech => "cs-CZ", 
			Language.Polish => "pl-PL", 
			Language.Spanish => "es-ES", 
			Language.Dutch => "nl-NL", 
			Language.Lithuanian => "lt-LT", 
			Language.Vietnamese => "vi-VN", 
			_ => "en-US", 
		};
	}
}
