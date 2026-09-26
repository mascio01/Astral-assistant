using UnityEngine;

public class CheckTranslationMenu : DebugMenu
{
	private bool Fixup;

	public CheckTranslationMenu()
		: base("Check Translation")
	{
		for (int i = 0; i < 25; i++)
		{
			Language language = (Language)i;
			if (language != Language.English && GameImpl.Instance.IsLanguageSupported(language))
			{
				Items.Add(new DebugMenuItemCustom(language.ToString(), delegate
				{
					CheckTranslation(language, Fixup);
				}));
			}
		}
	}

	public void CheckTranslation(Language language, bool fixup)
	{
		GameImpl instance = GameImpl.Instance;
		Debug.LogError(Launcher.Log + "**************************************************************************");
		int num = Story.CheckTranslation(instance.StreamingAssetsPath + "/UI", language, fixup);
		foreach (Story currentStory in instance.CurrentStories)
		{
			num += Story.CheckTranslation(currentStory.Path, language, fixup);
		}
		if (num == 0)
		{
			Debug.LogError(Launcher.Log + "No errors found");
		}
		else
		{
			Debug.LogError(Launcher.Log + num + " errors found");
		}
		Debug.LogError(Launcher.Log + "**************************************************************************");
	}
}
