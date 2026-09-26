using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputFieldBehaviour : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private TMP_InputField UnityInputField;

	private bool HasReleasedStickSinceFocusGained;

	private bool WasFocused;

	private bool WantVirtualKeyboard;

	private void Awake()
	{
		UnityInputField = GetComponent<TMP_InputField>();
	}

	public static bool IsLetterKey(KeyCode key)
	{
		switch (key)
		{
		case KeyCode.None:
		case KeyCode.Keypad2:
		case KeyCode.Keypad8:
		case KeyCode.UpArrow:
		case KeyCode.DownArrow:
		case KeyCode.PageUp:
		case KeyCode.PageDown:
			return false;
		default:
			return key < KeyCode.WheelUp;
		}
	}

	private void CaptureLetterKeys(InputFunction inputFunction)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (IsLetterKey(instance.InputMappings[(int)inputFunction].PositiveKey))
		{
			instance.CaptureKey(instance.InputMappings[(int)inputFunction].PositiveKey, Captured.UntilReleased);
		}
		if (IsLetterKey(instance.InputMappings[(int)inputFunction].NegativeKey))
		{
			instance.CaptureKey(instance.InputMappings[(int)inputFunction].NegativeKey, Captured.UntilReleased);
		}
		if (IsLetterKey(instance.InputMappings[(int)inputFunction].AltPositiveKey))
		{
			instance.CaptureKey(instance.InputMappings[(int)inputFunction].AltPositiveKey, Captured.UntilReleased);
		}
		if (IsLetterKey(instance.InputMappings[(int)inputFunction].AltNegativeKey))
		{
			instance.CaptureKey(instance.InputMappings[(int)inputFunction].AltNegativeKey, Captured.UntilReleased);
		}
	}

	private void Update()
	{
		if (UnityInputField.isFocused)
		{
			InputFunctionManager instance = InputFunctionManager.Instance;
			CaptureLetterKeys(InputFunction.MoveVert);
			CaptureLetterKeys(InputFunction.MoveHoriz);
			CaptureLetterKeys(InputFunction.MenuSelect);
			float axis = instance.GetAxis(InputFunction.MoveVert, capture: false);
			if (axis > 0f)
			{
				if (HasReleasedStickSinceFocusGained)
				{
					instance.Capture(InputFunction.MoveVert, untilReleased: true);
					Selectable selectable = UnityInputField.FindSelectableOnUp();
					if (selectable != null)
					{
						selectable.Select();
						SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
					}
				}
			}
			else if (axis < 0f)
			{
				if (HasReleasedStickSinceFocusGained)
				{
					instance.Capture(InputFunction.MoveVert, untilReleased: true);
					Selectable selectable2 = UnityInputField.FindSelectableOnDown();
					if (selectable2 != null)
					{
						selectable2.Select();
						SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
					}
				}
			}
			else
			{
				HasReleasedStickSinceFocusGained = true;
			}
			if (GameImpl.Instance.IsSteamDeck())
			{
				if (instance.IsJustPressed(InputFunction.MenuSelect, capture: false, ButtonPromptBarBehaviour.PROMPT_ShowKeyboard))
				{
					WantVirtualKeyboard = true;
				}
				if (!WasFocused && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor)
				{
					WantVirtualKeyboard = true;
				}
				if (WantVirtualKeyboard && !instance.IsPressed(InputFunction.MenuSelect))
				{
					Rect rect = GameImpl.Instance.CalcRectInScreenPixels(HudBehaviour.Instance.GetUIObjectRectOnScreen(base.gameObject));
					GameImpl.IsShowingOSK = SteamUtils.ShowFloatingGamepadTextInput(EFloatingGamepadTextInputMode.k_EFloatingGamepadTextInputModeModeSingleLine, (int)rect.x, GameImpl.Instance.ScreenHeight - ((int)rect.y + (int)rect.height), (int)rect.width, (int)rect.height);
					WantVirtualKeyboard = false;
				}
			}
			WasFocused = true;
		}
		else
		{
			HasReleasedStickSinceFocusGained = false;
			WasFocused = false;
			WantVirtualKeyboard = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && GameImpl.Instance.IsSteamDeck())
		{
			WantVirtualKeyboard = true;
		}
	}
}
