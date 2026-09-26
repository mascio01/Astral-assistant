using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GraphicsSettings : BaseMenu
{
	public static float MinTreeQuality = 0.25f;

	public static float MaxTreeQuality = 2f;

	public static float MinFOV = 30f;

	public static float MaxFOV = 60f;

	private bool WantRepopulate;

	private bool WantSaveSettings;

	private bool WantSavePlayerPrefs;

	private List<Resolution> ValidResolutions = new List<Resolution>();

	private TMP_Dropdown UnityQualityDropdown;

	private Slider UnityGrassDensitySlider;

	private Slider UnityGrassRangeSlider;

	private Slider UnityTreeQualitySlider;

	private Toggle UnitySuperSampleToggle;

	private Toggle UnityTAAToggle;

	private Toggle UnityHighQualityEffectsToggle;

	private Toggle UnitySidebarLayoutToggle;

	private Toggle UnityPiPBackgroundToggle;

	private Toggle UnityHintsToggle;

	public ActionMenu ActionMenu = new ActionMenu();

	private List<Selectable> Selectables = new List<Selectable>();

	private static int MENU_SidebarLayoutHelp = StringUtil.JenkinsHash("MENU_SidebarLayoutHelp");

	private static int MENU_PiPBackgroundHelp = StringUtil.JenkinsHash("MENU_PiPBackgroundHelp");

	private static int MENU_HintsEnabledHelp = StringUtil.JenkinsHash("MENU_HintsEnabledHelp");

	private static int MENU_SuperSampleHelp = StringUtil.JenkinsHash("MENU_SuperSampleHelp");

	private static int MENU_TAAHelp = StringUtil.JenkinsHash("MENU_TAAHelp");

	private static int MENU_HighQualityEffectsHelp = StringUtil.JenkinsHash("MENU_HighQualityEffectsHelp");

	private static int MENU_GrassQualityHelp = StringUtil.JenkinsHash("MENU_GrassQualityHelp");

	private static int MENU_GrassRangeHelp = StringUtil.JenkinsHash("MENU_GrassRangeHelp");

	private static int MENU_TreeQualityHelp = StringUtil.JenkinsHash("MENU_TreeQualityHelp");

	private static int MENU_QualityHelp = StringUtil.JenkinsHash("MENU_QualityHelp");

	private static float TooltipMenuXOffset = 260f;

	private int SettingResolution;

	private bool UpdatingDropdown;

	public override ActionMenu GetActionMenu()
	{
		if (!(ChildMenu != null))
		{
			return ActionMenu;
		}
		return ChildMenu.GetActionMenu();
	}

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		UnityQualityDropdown = base.gameObject.FindChild("MenuLayout/QualityField/Dropdown").GetComponent<TMP_Dropdown>();
		UnityGrassDensitySlider = base.gameObject.FindChild("MenuLayout/GrassDensityField/Slider").GetComponent<Slider>();
		UnityGrassRangeSlider = base.gameObject.FindChild("MenuLayout/GrassRangeField/Slider").GetComponent<Slider>();
		UnityTreeQualitySlider = base.gameObject.FindChild("MenuLayout/TreeQualityField/Slider").GetComponent<Slider>();
		UnitySuperSampleToggle = base.gameObject.FindChild("MenuLayout/SuperSampleField/Toggle").GetComponent<Toggle>();
		UnityTAAToggle = base.gameObject.FindChild("MenuLayout/TAAField/Toggle").GetComponent<Toggle>();
		UnityHighQualityEffectsToggle = base.gameObject.FindChild("MenuLayout/HighQualityEffectsField/Toggle").GetComponent<Toggle>();
		UnitySidebarLayoutToggle = base.gameObject.FindChild("MenuLayout/SidebarLayoutField/Toggle").GetComponent<Toggle>();
		UnityPiPBackgroundToggle = base.gameObject.FindChild("MenuLayout/PipBackgroundField/Toggle").GetComponent<Toggle>();
		UnityHintsToggle = base.gameObject.FindChild("MenuLayout/HintsField/Toggle").GetComponent<Toggle>();
	}

	public override void OnActivate()
	{
		base.OnActivate();
		ValidResolutions.Clear();
		for (int i = 0; i < Screen.resolutions.Length; i++)
		{
			Resolution item = Screen.resolutions[i];
			if (!(item.refreshRateRatio.value >= 59.0))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < ValidResolutions.Count; j++)
			{
				if (ValidResolutions[j].width == item.width && ValidResolutions[j].height == item.height)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				ValidResolutions.Add(item);
			}
		}
		Populate();
	}

	public override void OnDeactivate(bool popped)
	{
		if (WantSaveSettings)
		{
			GameImpl.Instance.AutoSaveSettings();
			WantSaveSettings = false;
		}
		if (WantSavePlayerPrefs)
		{
			PlayerPrefs.Save();
			WantSavePlayerPrefs = false;
		}
		base.OnDeactivate(popped);
	}

	public void Populate()
	{
		WantRepopulate = false;
		Selectables.Clear();
		UpdatingDropdown = true;
		TMP_Dropdown component = base.gameObject.FindChild("MenuLayout/ResolutionField/Dropdown").GetComponent<TMP_Dropdown>();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int num = (int)Math.Round(Screen.currentResolution.refreshRateRatio.value);
		int value = -1;
		for (int i = 0; i < ValidResolutions.Count; i++)
		{
			Resolution resolution = ValidResolutions[i];
			list.Add(new TMP_Dropdown.OptionData(resolution.width + " x " + resolution.height + " (" + num + " " + GameImpl.Translate("HUD_Hz") + ")"));
			if (resolution.width == Screen.width && resolution.height == Screen.height)
			{
				value = i;
			}
		}
		component.options = list;
		component.value = value;
		Selectables.Add(component);
		Toggle component2 = base.gameObject.FindChild("MenuLayout/FullScreenField/Toggle").GetComponent<Toggle>();
		component2.isOn = Screen.fullScreen;
		Selectables.Add(component2);
		TMP_Dropdown component3 = base.gameObject.FindChild("MenuLayout/QualityField/Dropdown").GetComponent<TMP_Dropdown>();
		List<TMP_Dropdown.OptionData> list2 = new List<TMP_Dropdown.OptionData>();
		list2.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_QualityLow")));
		list2.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_QualityMedium")));
		list2.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_QualityHigh")));
		component3.options = list2;
		component3.value = QualitySettings.GetQualityLevel();
		Selectables.Add(component3);
		TMP_Dropdown component4 = base.gameObject.FindChild("MenuLayout/VSyncField/Dropdown").GetComponent<TMP_Dropdown>();
		List<TMP_Dropdown.OptionData> list3 = new List<TMP_Dropdown.OptionData>();
		list3.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_NoVSync")));
		list3.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_EveryVBlank")));
		list3.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_EverySecondVBlank")));
		component4.options = list3;
		component4.value = Math.Min(QualitySettings.vSyncCount, list3.Count - 1);
		Selectables.Add(component4);
		Selectables.Add(SetSliderValue("MenuLayout/FOVField/Slider", HudBehaviour.Instance.GameCameraFOV, MinFOV, MaxFOV));
		Selectables.Add(SetSliderValue("MenuLayout/TreeQualityField/Slider", GameImpl.Instance.Settings.TreeQuality, MinTreeQuality, MaxTreeQuality));
		Selectables.Add(SetSliderValue("MenuLayout/GrassDensityField/Slider", GameImpl.Instance.Settings.GrassDensity, 0f, 4f));
		Selectables.Add(SetSliderValue("MenuLayout/GrassRangeField/Slider", HudBehaviour.Instance.UnityGrassCameraBehaviour.GrassRangeMax, 0f, 48f));
		Selectables.Add(SetCheckboxValue("MenuLayout/HighQualityEffectsField/Toggle", GameImpl.Instance.Settings.HighQualityEffects));
		Selectables.Add(SetCheckboxValue("MenuLayout/TAAField/Toggle", GameImpl.TAA));
		Selectables.Add(SetCheckboxValue("MenuLayout/SuperSampleField/Toggle", GameImpl.SuperSample));
		Selectables.Add(SetCheckboxValue("MenuLayout/SidebarLayoutField/Toggle", GameImpl.Instance.Settings.SidebarLayoutEnabled));
		Selectables.Add(SetCheckboxValue("MenuLayout/PipBackgroundField/Toggle", GameImpl.Instance.Settings.PiPBackgroundEnabled));
		Selectables.Add(SetCheckboxValue("MenuLayout/HintsField/Toggle", GameImpl.Instance.Settings.HintsEnabled));
		Selectables.Add(SetCheckboxValue("MenuLayout/UseMetricWeightsField/Toggle", GameImpl.Instance.Settings.UseMetricWeights));
		Selectables.Add(SetCheckboxValue("MenuLayout/UseCelsiusField/Toggle", GameImpl.Instance.Settings.UseCelsius));
		Selectables.Add(SetCheckboxValue("MenuLayout/MinimapRotationField/Toggle", GameImpl.Instance.Settings.MinimapRotationEnabled));
		Selectables.Add(base.gameObject.FindChild("MenuLayout/BackButton").GetComponent<Button>());
		BaseMenu.SetupNavigation(Selectables, topAndBottomAreAutomatic: false);
		UpdatingDropdown = false;
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (SettingResolution > 0)
		{
			SettingResolution--;
			if (SettingResolution == 0)
			{
				WantRepopulate = true;
				GameTerrain.UnityOnQualityLevelChanged();
			}
		}
		if (WantRepopulate)
		{
			Populate();
		}
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null)
		{
			if (current.currentSelectedGameObject == UnitySuperSampleToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnitySuperSampleToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_SuperSampleHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityTAAToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityTAAToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_TAAHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityHighQualityEffectsToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityHighQualityEffectsToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_HighQualityEffectsHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnitySidebarLayoutToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnitySidebarLayoutToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_SidebarLayoutHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityPiPBackgroundToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityPiPBackgroundToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_PiPBackgroundHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityHintsToggle.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityHintsToggle.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_HintsEnabledHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityGrassDensitySlider.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityGrassDensitySlider.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_GrassQualityHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityGrassRangeSlider.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityGrassRangeSlider.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_GrassRangeHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityTreeQualitySlider.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityTreeQualitySlider.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_TreeQualityHelp), TooltipMenuXOffset, leftAligned: true));
			}
			if (current.currentSelectedGameObject == UnityQualityDropdown.gameObject)
			{
				ActionMenu.FocusUnityObj = UnityQualityDropdown.gameObject;
				ActionMenu.HeaderActions.Add(new AvailableAction(CursorAction.Tooltip, GameImpl.Translate(MENU_QualityHelp), TooltipMenuXOffset, leftAligned: true));
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

	public void OnSetResolution(int i)
	{
		if (!UpdatingDropdown && SettingResolution <= 0)
		{
			Resolution resolution = ValidResolutions[i];
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode, resolution.refreshRateRatio);
			PlayerPrefs.SetInt("Screenmanager Resolution Width", resolution.width);
			PlayerPrefs.SetInt("Screenmanager Resolution Height", resolution.height);
			WantSavePlayerPrefs = true;
			SettingResolution = 10;
		}
	}

	public void OnSetFullScreen(bool v)
	{
		if (!UpdatingDropdown && SettingResolution <= 0)
		{
			Screen.fullScreen = v;
			SettingResolution = 10;
		}
	}

	public void OnSetQualityLevel(int i)
	{
		if (!UpdatingDropdown && SettingResolution <= 0)
		{
			QualitySettings.SetQualityLevel(i);
			QualitySettings.vSyncCount = PlayerPrefs.GetInt("VSyncCount", QualitySettings.vSyncCount);
			SettingResolution = 10;
		}
	}

	public void OnSetVSyncCount(int i)
	{
		if (!UpdatingDropdown && SettingResolution <= 0)
		{
			GameImpl.Instance.SetVSyncCount(i);
			SettingResolution = 10;
		}
	}

	public void OnSetFOV(float v)
	{
		if (!UpdatingDropdown && SettingResolution <= 0)
		{
			GameImpl.Instance.SetFOV(v);
			WantSavePlayerPrefs = true;
		}
	}

	public void OnSetTreeQuality(float v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.SetTreeQuality(v);
			WantSavePlayerPrefs = true;
			WantRepopulate = true;
		}
	}

	public void OnSetGrassDensity(float v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.SetGrassDensity(v);
			WantSavePlayerPrefs = true;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetGrassRange(float v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.SetGrassRange(v);
			WantSavePlayerPrefs = true;
			WantRepopulate = true;
		}
	}

	public void OnSetTAAEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.SetTAA(v);
			WantSavePlayerPrefs = true;
			WantRepopulate = true;
		}
	}

	public void OnSetSuperSampleEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.SetSuperSample(v);
			WantSavePlayerPrefs = true;
			WantRepopulate = true;
		}
	}

	public void OnSetHighQualityEffectsEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.HighQualityEffects = v;
			PlayerPrefs.SetInt("HighQualityEffects", v ? 1 : 0);
			WantSavePlayerPrefs = true;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetSidebarLayoutEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.SidebarLayoutEnabled = v;
			HudBehaviour.Instance.OnScreenResized();
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetPiPBackgroundEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.PiPBackgroundEnabled = v;
			HudBehaviour.Instance.OnScreenResized();
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetHintsEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.HintsEnabled = v;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetUseMetricWeights(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.UseMetricWeights = v;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnSetUseCelsius(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.UseCelsius = v;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}

	public void OnMinimapRotationEnabled(bool v)
	{
		if (!UpdatingDropdown)
		{
			GameImpl.Instance.Settings.MinimapRotationEnabled = v;
			WantSaveSettings = true;
			WantRepopulate = true;
		}
	}
}
