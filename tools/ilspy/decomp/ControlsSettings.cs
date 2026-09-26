using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : BaseMenu
{
	private bool WantRepopulate;

	private bool WantSaveSettings;

	private bool WantSavePlayerPrefs;

	private InputType CurInputType;

	private InputRecordingType CurRecordingType;

	private InputFunction CurRecordingFunction = InputFunction.Invalid;

	private GameObject UnityCurRecordingButtonObj;

	private GameObject UnityContents;

	private GameObject UnityHeading;

	private GameObject UnityHeadingXBox;

	private GameObject UnityMouseSettingsPanel;

	private GameObject UnityGamepadSettingsPanel;

	private Button UnityResetButton;

	private Button UnityBackButton;

	private Slider UnityDeadZoneSlider;

	private Slider UnityTriggerDeadZoneSlider;

	private Slider UnityMouseLookSlider;

	private Slider UnityMouseTargetXSlider;

	private Slider UnityMouseTargetYSlider;

	public TextMeshProUGUI UnityTabLeftPrompt;

	public TextMeshProUGUI UnityTabRightPrompt;

	private TMP_Dropdown UnityInputTypeDropdown;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		UnityContents = base.gameObject.FindChild("MenuLayout/Scroll View/Viewport/Content");
		UnityHeading = base.gameObject.FindChild("MenuLayout/Headings");
		UnityHeadingXBox = base.gameObject.FindChild("MenuLayout/HeadingsXBox");
		UnityInputTypeDropdown = base.gameObject.FindChild("MenuLayout/InputTypeDropdown").GetComponent<TMP_Dropdown>();
		UnityMouseSettingsPanel = base.gameObject.FindChild("MenuLayout/Panel");
		UnityGamepadSettingsPanel = base.gameObject.FindChild("MenuLayout/GamepadPanel");
		UnityResetButton = base.gameObject.FindChild("MenuLayout/ResetButton").GetComponent<Button>();
		UnityBackButton = base.gameObject.FindChild("MenuLayout/BackButton").GetComponent<Button>();
		UnityDeadZoneSlider = base.gameObject.FindChild("MenuLayout/GamepadPanel/DeadZoneField/Slider").GetComponent<Slider>();
		UnityTriggerDeadZoneSlider = base.gameObject.FindChild("MenuLayout/GamepadPanel/TriggerDeadZoneField/Slider").GetComponent<Slider>();
		UnityMouseLookSlider = base.gameObject.FindChild("MenuLayout/Panel/MouseLookSensitivityField/Slider").GetComponent<Slider>();
		UnityMouseTargetXSlider = base.gameObject.FindChild("MenuLayout/Panel/MouseTargetXSensitivityField/Slider").GetComponent<Slider>();
		UnityMouseTargetYSlider = base.gameObject.FindChild("MenuLayout/Panel/MouseTargetYSensitivityField/Slider").GetComponent<Slider>();
		UnityTabLeftPrompt = base.transform.Find("MenuLayout/TabLeftPrompt").GetComponent<TextMeshProUGUI>();
		UnityTabRightPrompt = base.transform.Find("MenuLayout/TabRightPrompt").GetComponent<TextMeshProUGUI>();
		PaperTextureAmount = 0.25f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		CurInputType = InputFunctionManager.Instance.CurrentInputType;
		Populate();
	}

	public override void OnDeactivate(bool popped)
	{
		StopRecording();
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

	public void SetupMappingButton(GameObject unityRow, string name, string button, InputRecordingType recordingType, InputFunction inputFunction, bool enabled)
	{
		TextMeshProUGUI component = unityRow.FindChild(name + "/Text").GetComponent<TextMeshProUGUI>();
		component.SetUnityText(button);
		component.enabled = enabled;
		GameObject unityButtonObj = unityRow.FindChild(name);
		InputMappingButtonBehaviour component2 = unityButtonObj.GetComponent<InputMappingButtonBehaviour>();
		component2.InputFunction = inputFunction;
		component2.InputRecordingType = recordingType;
		Button component3 = unityButtonObj.GetComponent<Button>();
		component3.enabled = enabled;
		component3.onClick.RemoveAllListeners();
		component3.onClick.AddListener(delegate
		{
			StopRecording();
			StartRecording(recordingType, inputFunction, unityButtonObj);
		});
		unityButtonObj.GetComponent<Outline>().enabled = enabled;
	}

	public void Populate()
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		List<string> list = new List<string>();
		for (int i = 0; i < 5; i++)
		{
			InputType inputType = (InputType)i;
			list.Add(GameImpl.Translate("MENU_" + inputType));
		}
		UnityInputTypeDropdown.onValueChanged.RemoveAllListeners();
		UnityInputTypeDropdown.ClearOptions();
		UnityInputTypeDropdown.AddOptions(list);
		UnityInputTypeDropdown.value = (int)CurInputType;
		UnityInputTypeDropdown.onValueChanged.AddListener(delegate(int v)
		{
			CurInputType = (InputType)v;
			WantRepopulate = true;
		});
		UnityHeading.SetActive(CurInputType == InputType.MouseAndKeyboard);
		UnityHeadingXBox.SetActive(CurInputType != InputType.MouseAndKeyboard);
		List<GameObject> list2 = new List<GameObject>();
		for (int num = 0; num < 78; num++)
		{
			InputFunction inputFunction = (InputFunction)num;
			MappedInput mappedInput = instance.InputMappings[num];
			string text = CurInputType.ToString() + "_" + inputFunction;
			GameObject gameObject = UnityContents.FindChild(text);
			if (gameObject == null)
			{
				gameObject = Object.Instantiate((CurInputType == InputType.MouseAndKeyboard) ? BaseMenu.ControlsRow.GetAsset() : BaseMenu.ControlsRowXBox.GetAsset(), UnityContents.transform, worldPositionStays: false);
				gameObject.name = text;
			}
			gameObject.transform.SetSiblingIndex(num);
			list2.Add(gameObject);
			gameObject.FindChild("Command").GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate("INPUT_" + inputFunction));
			bool flag = InputFunctionManager.CanHaveNegative(inputFunction);
			bool active = false;
			switch (CurInputType)
			{
			case InputType.MouseAndKeyboard:
			{
				SetupMappingButton(gameObject, "Pos1", (mappedInput.PositiveKey != KeyCode.None) ? mappedInput.PositiveKey.ToString() : " ", InputRecordingType.PositiveKey, inputFunction, enabled: true);
				SetupMappingButton(gameObject, "Pos2", (mappedInput.AltPositiveKey != KeyCode.None) ? mappedInput.AltPositiveKey.ToString() : " ", InputRecordingType.AltPositiveKey, inputFunction, enabled: true);
				SetupMappingButton(gameObject, "Neg1", (mappedInput.NegativeKey != KeyCode.None) ? mappedInput.NegativeKey.ToString() : " ", InputRecordingType.NegativeKey, inputFunction, flag);
				SetupMappingButton(gameObject, "Neg2", (mappedInput.AltNegativeKey != KeyCode.None) ? mappedInput.AltNegativeKey.ToString() : " ", InputRecordingType.AltNegativeKey, inputFunction, flag);
				List<string> list4 = new List<string>();
				list4.Add("");
				for (int num3 = 0; num3 < 5; num3++)
				{
					MouseAxis mouseAxis = (MouseAxis)num3;
					list4.Add(GameImpl.Translate("INPUT_" + mouseAxis));
				}
				TMP_Dropdown component2 = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
				component2.onValueChanged.RemoveAllListeners();
				component2.ClearOptions();
				component2.AddOptions(list4);
				component2.value = (int)(mappedInput.MouseAxis + 1);
				component2.onValueChanged.AddListener(delegate(int v)
				{
					mappedInput.MouseAxis = (MouseAxis)(v - 1);
					CommitChanges(mappedInput);
				});
				active = mappedInput.MouseAxis != MouseAxis.None;
				break;
			}
			case InputType.XBox:
			{
				SetupMappingButton(gameObject, "Pos", (mappedInput.PositiveXBoxButton != XBoxButton.None) ? StringUtil.GetXBoxButtonSpriteString(mappedInput.PositiveXBoxButton) : " ", InputRecordingType.PositiveXBoxButton, inputFunction, enabled: true);
				SetupMappingButton(gameObject, "Neg", (mappedInput.NegativeXBoxButton != XBoxButton.None) ? StringUtil.GetXBoxButtonSpriteString(mappedInput.NegativeXBoxButton) : " ", InputRecordingType.NegativeXBoxButton, inputFunction, flag);
				List<string> list6 = new List<string>();
				if (mappedInput.CanBeCleared(InputRecordingType.PositiveXBoxButton) || mappedInput.CanBeCleared(InputRecordingType.NegativeXBoxButton))
				{
					list6.Add("");
				}
				for (int num5 = 0; num5 < 4; num5++)
				{
					XBoxAxis xBoxAxis = (XBoxAxis)num5;
					list6.Add(GameImpl.Translate("INPUT_" + xBoxAxis));
				}
				TMP_Dropdown component4 = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
				component4.onValueChanged.RemoveAllListeners();
				component4.ClearOptions();
				component4.AddOptions(list6);
				component4.value = (int)(mappedInput.XBoxAxis + 1);
				component4.onValueChanged.AddListener(delegate(int v)
				{
					mappedInput.XBoxAxis = (XBoxAxis)(v - 1);
					CommitChanges(mappedInput);
				});
				active = mappedInput.XBoxAxis != XBoxAxis.None;
				break;
			}
			case InputType.PS4:
			{
				SetupMappingButton(gameObject, "Pos", (mappedInput.PositivePS4Button != PS4Button.None) ? StringUtil.GetPS4ButtonSpriteString(mappedInput.PositivePS4Button) : " ", InputRecordingType.PositivePS4Button, inputFunction, enabled: true);
				SetupMappingButton(gameObject, "Neg", (mappedInput.NegativePS4Button != PS4Button.None) ? StringUtil.GetPS4ButtonSpriteString(mappedInput.NegativePS4Button) : " ", InputRecordingType.NegativePS4Button, inputFunction, flag);
				List<string> list5 = new List<string>();
				if (mappedInput.CanBeCleared(InputRecordingType.PositivePS4Button) || mappedInput.CanBeCleared(InputRecordingType.NegativePS4Button))
				{
					list5.Add("");
				}
				for (int num4 = 0; num4 < 4; num4++)
				{
					PS4Axis pS4Axis = (PS4Axis)num4;
					list5.Add(GameImpl.Translate("INPUT_" + pS4Axis));
				}
				TMP_Dropdown component3 = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
				component3.onValueChanged.RemoveAllListeners();
				component3.ClearOptions();
				component3.AddOptions(list5);
				component3.value = (int)(mappedInput.PS4Axis + 1);
				component3.onValueChanged.AddListener(delegate(int v)
				{
					mappedInput.PS4Axis = (PS4Axis)(v - 1);
					CommitChanges(mappedInput);
				});
				active = mappedInput.PS4Axis != PS4Axis.None;
				break;
			}
			case InputType.SteamDeck:
			{
				SetupMappingButton(gameObject, "Pos", (mappedInput.PositiveSteamDeckButton != SteamDeckButton.None) ? StringUtil.GetSteamDeckButtonSpriteString(mappedInput.PositiveSteamDeckButton) : " ", InputRecordingType.PositiveSteamDeckButton, inputFunction, enabled: true);
				SetupMappingButton(gameObject, "Neg", (mappedInput.NegativeSteamDeckButton != SteamDeckButton.None) ? StringUtil.GetSteamDeckButtonSpriteString(mappedInput.NegativeSteamDeckButton) : " ", InputRecordingType.NegativeSteamDeckButton, inputFunction, flag);
				List<string> list3 = new List<string>();
				if (mappedInput.CanBeCleared(InputRecordingType.PositiveSteamDeckButton) || mappedInput.CanBeCleared(InputRecordingType.NegativeSteamDeckButton))
				{
					list3.Add("");
				}
				for (int num2 = 0; num2 < 4; num2++)
				{
					SteamDeckAxis steamDeckAxis = (SteamDeckAxis)num2;
					list3.Add(GameImpl.Translate("INPUT_" + steamDeckAxis));
				}
				TMP_Dropdown component = gameObject.FindChild("Dropdown").GetComponent<TMP_Dropdown>();
				component.onValueChanged.RemoveAllListeners();
				component.ClearOptions();
				component.AddOptions(list3);
				component.value = (int)(mappedInput.SteamDeckAxis + 1);
				component.onValueChanged.AddListener(delegate(int v)
				{
					mappedInput.SteamDeckAxis = (SteamDeckAxis)(v - 1);
					CommitChanges(mappedInput);
				});
				active = mappedInput.SteamDeckAxis != SteamDeckAxis.None;
				break;
			}
			}
			Toggle component5 = gameObject.FindChild("Toggle").GetComponent<Toggle>();
			component5.onValueChanged.RemoveAllListeners();
			component5.gameObject.SetActive(active);
			component5.isOn = mappedInput.AxisInverted;
			component5.onValueChanged.AddListener(delegate(bool v)
			{
				mappedInput.AxisInverted = v;
				CommitChanges(mappedInput);
			});
		}
		for (int num6 = 0; num6 < UnityContents.transform.childCount; num6++)
		{
			GameObject gameObject2 = UnityContents.transform.GetChild(num6).gameObject;
			if (!list2.Contains(gameObject2))
			{
				Object.Destroy(gameObject2);
			}
		}
		UnityMouseSettingsPanel.SetActive(UnityInputTypeDropdown.value == 0);
		UnityGamepadSettingsPanel.SetActive(UnityInputTypeDropdown.value != 0);
		Navigation navigation = UnityResetButton.navigation;
		navigation.selectOnUp = ((UnityInputTypeDropdown.value == 0) ? UnityMouseLookSlider : UnityDeadZoneSlider);
		UnityResetButton.navigation = navigation;
		Navigation navigation2 = UnityBackButton.navigation;
		navigation2.selectOnUp = ((UnityInputTypeDropdown.value == 0) ? UnityMouseTargetXSlider : UnityTriggerDeadZoneSlider);
		UnityBackButton.navigation = navigation2;
		SetSliderValue("MenuLayout/Panel/MouseLookSensitivityField/Slider", InputFunctionManager.MouseLookSensitivity, 0.1f, 2f);
		SetSliderValue("MenuLayout/Panel/MouseTargetXSensitivityField/Slider", 0f - InputFunctionManager.MouseTargetSensitivity, -100f, -2f);
		SetSliderValue("MenuLayout/Panel/MouseTargetYSensitivityField/Slider", 0f - InputFunctionManager.MouseBodyLocationSensitivity, -100f, -2f);
		SetSliderValue("MenuLayout/GamepadPanel/DeadZoneField/Slider", InputFunctionManager.JoystickDeadZone, 0f, 0.9f);
		SetSliderValue("MenuLayout/GamepadPanel/TriggerDeadZoneField/Slider", InputFunctionManager.TriggerDeadZone, 0f, 0.9f);
		base.gameObject.FindChild("MenuLayout/Panel/MouseLookAccelerationField").GetComponent<Toggle>().isOn = InputFunctionManager.MouseLookAcceleration;
		WantRepopulate = false;
	}

	public void StartRecording(InputRecordingType recordingType, InputFunction inputFunction, GameObject unityButtonObj)
	{
		CurRecordingType = recordingType;
		CurRecordingFunction = inputFunction;
		UnityCanvasGroup.interactable = false;
		UnityCurRecordingButtonObj = unityButtonObj;
		UnityCurRecordingButtonObj.GetComponent<Outline>().effectColor = Color.red;
	}

	public void StopRecording()
	{
		if (CurRecordingType != InputRecordingType.None)
		{
			UnityCurRecordingButtonObj.GetComponent<Outline>().effectColor = Color.black;
			CurRecordingType = InputRecordingType.None;
			CurRecordingFunction = InputFunction.Invalid;
			UnityCurRecordingButtonObj = null;
			UnityCanvasGroup.interactable = true;
		}
	}

	public void CommitChangeAndStopRecording()
	{
		if (CurRecordingFunction != InputFunction.Invalid)
		{
			MappedInput mappedInput = InputFunctionManager.Instance.InputMappings[(int)CurRecordingFunction];
			CommitChanges(mappedInput);
			StopRecording();
		}
	}

	public void CommitChanges(MappedInput mappedInput)
	{
		GameImpl instance = GameImpl.Instance;
		if (instance.Settings.OverriddenControls == null)
		{
			instance.Settings.OverriddenControls = new List<MappedInput>();
		}
		instance.Settings.OverriddenControls.Add(mappedInput);
		instance.AutoSaveSettings();
		WantRepopulate = true;
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (CurRecordingType == InputRecordingType.None && WantRepopulate)
		{
			Populate();
		}
		UnityTabLeftPrompt.gameObject.SetActive(CurRecordingType == InputRecordingType.None);
		UnityTabRightPrompt.gameObject.SetActive(CurRecordingType == InputRecordingType.None);
		StringUtil.SetUnityTextButtonPrompt(UnityTabLeftPrompt, InputFunction.TabLeft);
		StringUtil.SetUnityTextButtonPrompt(UnityTabRightPrompt, InputFunction.TabRight);
	}

	public override void PreHandleInputImpl(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (CurRecordingType == InputRecordingType.None)
		{
			GameObject currentSelectedGameObject = UnityEventSystem.currentSelectedGameObject;
			if (currentSelectedGameObject != null)
			{
				InputMappingButtonBehaviour component = currentSelectedGameObject.GetComponent<InputMappingButtonBehaviour>();
				if (component != null)
				{
					MappedInput mappedInput = instance.InputMappings[(int)component.InputFunction];
					if (mappedInput.CanBeCleared(component.InputRecordingType) && !mappedInput.IsClear(component.InputRecordingType) && instance.IsJustPressed(InputFunction.Clear, capture: true, ButtonPromptBarBehaviour.PROMPT_Clear))
					{
						mappedInput.Clear(component.InputRecordingType);
						CommitChanges(mappedInput);
					}
				}
			}
			if (instance.IsJustPressed(InputFunction.TabLeft) && UnityInputTypeDropdown.value > 0)
			{
				UnityInputTypeDropdown.value--;
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
			if (instance.IsJustPressed(InputFunction.TabRight) && UnityInputTypeDropdown.value < UnityInputTypeDropdown.options.Count - 1)
			{
				UnityInputTypeDropdown.value++;
				SoundManager.PlayMenuSound(SoundManager.TabSound);
			}
			if (instance.IsJustPressed(InputFunction.Back))
			{
				SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
				if (instance.CurrentInputType != InputType.MouseAndKeyboard && currentSelectedGameObject != null && currentSelectedGameObject.transform.IsChildOf(UnityContents.transform))
				{
					UnityEventSystem.SetSelectedGameObject(UnityBackButton.gameObject);
				}
				else
				{
					WantPop = true;
				}
			}
			return;
		}
		MappedInput mappedInput2 = instance.InputMappings[(int)CurRecordingFunction];
		switch (CurRecordingType)
		{
		case InputRecordingType.PositiveKey:
		case InputRecordingType.NegativeKey:
		case InputRecordingType.AltPositiveKey:
		case InputRecordingType.AltNegativeKey:
		{
			for (int l = 0; l < instance.ValidKeyCodes.Length; l++)
			{
				KeyCode keyCode = instance.ValidKeyCodes[l];
				if (instance.IsKeyJustPressed(keyCode))
				{
					switch (CurRecordingType)
					{
					case InputRecordingType.PositiveKey:
						mappedInput2.PositiveKey = keyCode;
						break;
					case InputRecordingType.NegativeKey:
						mappedInput2.NegativeKey = keyCode;
						break;
					case InputRecordingType.AltPositiveKey:
						mappedInput2.AltPositiveKey = keyCode;
						break;
					case InputRecordingType.AltNegativeKey:
						mappedInput2.AltNegativeKey = keyCode;
						break;
					}
					CommitChangeAndStopRecording();
					break;
				}
			}
			break;
		}
		case InputRecordingType.PositiveXBoxButton:
		case InputRecordingType.NegativeXBoxButton:
		{
			for (int j = 0; j < 16; j++)
			{
				XBoxButton xBoxButton = (XBoxButton)j;
				if (instance.IsXBoxButtonJustPressed(xBoxButton))
				{
					switch (CurRecordingType)
					{
					case InputRecordingType.PositiveXBoxButton:
						mappedInput2.PositiveXBoxButton = xBoxButton;
						break;
					case InputRecordingType.NegativeXBoxButton:
						mappedInput2.NegativeXBoxButton = xBoxButton;
						break;
					}
					CommitChangeAndStopRecording();
					break;
				}
			}
			break;
		}
		case InputRecordingType.PositivePS4Button:
		case InputRecordingType.NegativePS4Button:
		{
			for (int k = 0; k < 17; k++)
			{
				PS4Button pS4Button = (PS4Button)k;
				if (instance.IsPS4ButtonJustPressed(pS4Button))
				{
					switch (CurRecordingType)
					{
					case InputRecordingType.PositivePS4Button:
						mappedInput2.PositivePS4Button = pS4Button;
						break;
					case InputRecordingType.NegativePS4Button:
						mappedInput2.NegativePS4Button = pS4Button;
						break;
					}
					CommitChangeAndStopRecording();
					break;
				}
			}
			break;
		}
		case InputRecordingType.PositiveSteamDeckButton:
		case InputRecordingType.NegativeSteamDeckButton:
		{
			for (int i = 0; i < 16; i++)
			{
				SteamDeckButton steamDeckButton = (SteamDeckButton)i;
				if (instance.IsSteamDeckButtonJustPressed(steamDeckButton))
				{
					switch (CurRecordingType)
					{
					case InputRecordingType.PositiveSteamDeckButton:
						mappedInput2.PositiveSteamDeckButton = steamDeckButton;
						break;
					case InputRecordingType.NegativeSteamDeckButton:
						mappedInput2.NegativeSteamDeckButton = steamDeckButton;
						break;
					}
					CommitChangeAndStopRecording();
					break;
				}
			}
			break;
		}
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		WantPop = true;
	}

	public void OnResetToDefaults()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		InputFunctionManager.Instance.SetupDefaultMappings();
		WantRepopulate = true;
		GameImpl instance = GameImpl.Instance;
		if (instance.Settings.OverriddenControls != null)
		{
			instance.Settings.OverriddenControls.Clear();
		}
		instance.Settings.MouseLookSensitivity = Hud.DefaultMouseLookSensitivity;
		instance.Settings.MouseTargetSensitivity = Hud.DefaultMouseTargetThreshold;
		instance.Settings.MouseBodyLocationSensitivity = Hud.DefaultMouseBodyLocationThreshold;
		instance.Settings.MouseLookAcceleration = true;
		instance.AutoSaveSettings();
		WantSaveSettings = false;
		OnSetDeadZone(InputFunctionManager.DefaultJoystickDeadZone);
		OnSetTriggerDeadZone(InputFunctionManager.DefaultTriggerDeadZone);
		PlayerPrefs.Save();
		WantSavePlayerPrefs = false;
	}

	public void OnSetMouseLookSensitivity(float v)
	{
		GameImpl.Instance.Settings.MouseLookSensitivity = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetMouseTargetXSensitivity(float v)
	{
		GameImpl.Instance.Settings.MouseTargetSensitivity = 0f - v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetMouseTargetYSensitivity(float v)
	{
		GameImpl.Instance.Settings.MouseBodyLocationSensitivity = 0f - v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetMouseLookAcceleration(bool v)
	{
		GameImpl.Instance.Settings.MouseLookAcceleration = v;
		WantSaveSettings = true;
		WantRepopulate = true;
	}

	public void OnSetDeadZone(float v)
	{
		InputFunctionManager.JoystickDeadZone = v;
		PlayerPrefs.SetFloat("JoystickDeadZone", v);
		WantSavePlayerPrefs = true;
	}

	public void OnSetTriggerDeadZone(float v)
	{
		InputFunctionManager.TriggerDeadZone = v;
		PlayerPrefs.SetFloat("TriggerDeadZone", v);
		WantSavePlayerPrefs = true;
	}
}
