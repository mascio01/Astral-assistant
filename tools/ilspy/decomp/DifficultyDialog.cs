using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultyDialog : BaseDialog
{
	public delegate void AcceptFunction();

	public AcceptFunction OnAccept;

	public CharacterCreationSettings Settings;

	private GameObject UnityPanel;

	public ActionMenu ActionMenu = new ActionMenu();

	private bool Populating;

	public static int MENU_Normal_Description = StringUtil.JenkinsHash("MENU_Normal_Description");

	public static int MENU_SlightlyEasier_Description = StringUtil.JenkinsHash("MENU_SlightlyEasier_Description");

	public static int MENU_InvisibleStrain_Description = StringUtil.JenkinsHash("MENU_InvisibleStrain_Description");

	public static int MENU_SaveTokens_Description = StringUtil.JenkinsHash("MENU_SaveTokens_Description");

	public static float DifficultyTooltipXOffset = 100f;

	public override ActionMenu GetActionMenu()
	{
		return ActionMenu;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnityPanel = base.gameObject.FindChild("Panel");
		UnityPanel.DeleteAllChildrenImmediately();
		foreach (DifficultySettings difficultySetting in GameImpl.Instance.CurrentStory.Settings.DifficultySettings)
		{
			DifficultySettings localDifficultySettings = difficultySetting;
			GameObject obj = Object.Instantiate((GameObject)BaseMenu.DifficultyToggle, UnityPanel.transform);
			TextMeshProUGUI component = obj.FindChild("Label").GetComponent<TextMeshProUGUI>();
			Toggle component2 = obj.GetComponent<Toggle>();
			component.SetUnityText(GameImpl.Translate(difficultySetting.GetNameKey()));
			component2.onValueChanged.RemoveAllListeners();
			component2.onValueChanged.AddListener(delegate
			{
				if (!Populating)
				{
					Settings.DifficultySettings = localDifficultySettings.MakeCopy();
					Populate();
				}
			});
		}
		WantOkCancelButtonPromptsForController = false;
		Populate();
	}

	private void Populate()
	{
		Populating = true;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		for (int i = 0; i < settings.DifficultySettings.Count; i++)
		{
			UnityPanel.transform.GetChild(i).gameObject.GetComponent<Toggle>().isOn = Settings.DifficultySettings.Matches(settings.DifficultySettings[i]);
		}
		Populating = false;
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (OKSelected)
		{
			OKSelected = false;
			Finished = true;
			OnAccept();
		}
	}

	public override void DialogUpdate()
	{
		base.DialogUpdate();
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null)
		{
			StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
			for (int i = 0; i < settings.DifficultySettings.Count; i++)
			{
				GameObject gameObject = UnityPanel.transform.GetChild(i).gameObject;
				if (gameObject == current.currentSelectedGameObject)
				{
					ActionMenu.FocusUnityObj = gameObject;
					ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(settings.DifficultySettings[i].GetDescriptionKey()), DifficultyTooltipXOffset, leftAligned: true));
				}
			}
		}
		ActionMenu.OnFinishAddingActions();
	}
}
