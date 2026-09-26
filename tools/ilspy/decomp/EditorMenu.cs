using System.Diagnostics;
using System.IO;

public class EditorMenu : BaseMenu
{
	private bool WantRunEditor;

	public override bool GetWantRunEditor()
	{
		return WantRunEditor;
	}

	public static bool IsReservedStoryName(string v)
	{
		switch (v)
		{
		default:
			return v == "Common";
		case "BaseStory":
		case "UI":
		case "Sandbox":
		case "MainStory":
		case "IntroStory":
		case "TutorialStory":
			return true;
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		GameImpl instance = GameImpl.Instance;
		Story currentlyEditingStory = instance.GetCurrentlyEditingStory();
		SetMenuCaption(currentlyEditingStory.StorySource.GetTranslatedName());
		SetMenuButtonActive("MenuLayout/EditMapButton", !currentlyEditingStory.Settings.IsMod && !currentlyEditingStory.Settings.ProcedurallyGenerated);
		if (instance.SteamInitialized && !IsReservedStoryName(currentlyEditingStory.StorySource.Folder) && currentlyEditingStory.StorySource.WorkshopId == 0L)
		{
			SetMenuButtonActive("MenuLayout/UploadToWorkshopButton", active: true);
		}
		else
		{
			SetMenuButtonActive("MenuLayout/UploadToWorkshopButton", active: false);
		}
		SetMenuButtonActive("MenuLayout/BuildTranslationFileButton", active: false);
		SetMenuButtonActive("MenuLayout/PortraitGeneratorButton", active: false);
	}

	public override void OnDeactivate(bool popped)
	{
		base.OnDeactivate(popped);
		WantRunEditor = false;
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			GameImpl.Instance.ReloadCurrentStory(reloadFromDisk: false);
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
	}

	public void OnEditMap()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantRunEditor = true;
		WantFadeOut = true;
	}

	public void OnEditStorySettings()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditStorySettingsPanel"));
	}

	public void OnEditEquipment()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditEquipmentPanel"));
	}

	public void OnEditLiquid()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditLiquidPanel"));
	}

	public void OnEditScript()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("SelectScriptPanel"));
	}

	public void OnEditMemory()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditMemoryPanel"));
	}

	public void OnEditRecipe()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditRecipePanel"));
	}

	public void OnEditProps()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("EditPropsPanel"));
	}

	public void OnBuildTranslationFile()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		GameImpl.Instance.GetCurrentlyEditingStory().BuildTSVFile(wantDialog: true);
	}

	public void OnPortraitGenerator()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("PortraitGeneratorPanel"));
	}

	public void OnOpenFolder()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		Process.Start("explorer.exe", currentlyEditingStory.Path.Replace('/', '\\'));
	}

	public void OnUploadToWorkshop()
	{
		SoundManager.PlayMenuSound(SoundManager.ForwardPageSound);
		Story story = GameImpl.Instance.GetCurrentlyEditingStory();
		if (WorkshopManager.Instance.CurrentState != WorkshopManager.State.Idle)
		{
			return;
		}
		if (!File.Exists(story.Path + "/CoverIcon.jpg"))
		{
			GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_SetCoverImage"), delegate
			{
				Process.Start("explorer.exe", story.Path.Replace('/', '\\'));
			});
			return;
		}
		if (story.Settings.SteamWorkshopId == 0L)
		{
			WorkshopManager.Instance.CreateWorkshopItem(story);
		}
		else
		{
			WorkshopManager.Instance.UpdateWorkshopItem(story);
		}
		OpenChildMenu(GameImpl.Instance.GetMenuBehaviourByPanelName("LoadingPanel"));
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		GameImpl.Instance.ReloadCurrentStory(reloadFromDisk: false);
		WantPop = true;
	}
}
