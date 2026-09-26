using UnityEngine;

public class MappedInput
{
	public InputFunction InputFunction;

	public KeyCode PositiveKey;

	public KeyCode NegativeKey;

	public KeyCode AltPositiveKey;

	public KeyCode AltNegativeKey;

	public MouseAxis MouseAxis = MouseAxis.None;

	public XBoxButton PositiveXBoxButton = XBoxButton.None;

	public XBoxButton NegativeXBoxButton = XBoxButton.None;

	public XBoxAxis XBoxAxis = XBoxAxis.None;

	public PS4Button PositivePS4Button = PS4Button.None;

	public PS4Button NegativePS4Button = PS4Button.None;

	public PS4Axis PS4Axis = PS4Axis.None;

	public SteamDeckButton PositiveSteamDeckButton = SteamDeckButton.None;

	public SteamDeckButton NegativeSteamDeckButton = SteamDeckButton.None;

	public SteamDeckAxis SteamDeckAxis = SteamDeckAxis.None;

	public bool AxisInverted;

	public bool DefaultOverridden;

	public MappedInput Copy()
	{
		return new MappedInput
		{
			InputFunction = InputFunction,
			PositiveKey = PositiveKey,
			NegativeKey = NegativeKey,
			AltPositiveKey = AltPositiveKey,
			AltNegativeKey = AltNegativeKey,
			MouseAxis = MouseAxis,
			PositiveXBoxButton = PositiveXBoxButton,
			NegativeXBoxButton = NegativeXBoxButton,
			XBoxAxis = XBoxAxis,
			PositivePS4Button = PositivePS4Button,
			NegativePS4Button = NegativePS4Button,
			PS4Axis = PS4Axis,
			PositiveSteamDeckButton = PositiveSteamDeckButton,
			NegativeSteamDeckButton = NegativeSteamDeckButton,
			SteamDeckAxis = SteamDeckAxis,
			AxisInverted = AxisInverted,
			DefaultOverridden = DefaultOverridden
		};
	}

	public bool IsClear(InputRecordingType inputRecordingType)
	{
		return inputRecordingType switch
		{
			InputRecordingType.PositiveKey => PositiveKey == KeyCode.None, 
			InputRecordingType.NegativeKey => NegativeKey == KeyCode.None, 
			InputRecordingType.AltPositiveKey => AltPositiveKey == KeyCode.None, 
			InputRecordingType.AltNegativeKey => AltNegativeKey == KeyCode.None, 
			InputRecordingType.PositiveXBoxButton => PositiveXBoxButton == XBoxButton.None, 
			InputRecordingType.NegativeXBoxButton => NegativeXBoxButton == XBoxButton.None, 
			InputRecordingType.PositivePS4Button => PositivePS4Button == PS4Button.None, 
			InputRecordingType.NegativePS4Button => NegativePS4Button == PS4Button.None, 
			InputRecordingType.PositiveSteamDeckButton => PositiveSteamDeckButton == SteamDeckButton.None, 
			InputRecordingType.NegativeSteamDeckButton => NegativeSteamDeckButton == SteamDeckButton.None, 
			_ => true, 
		};
	}

	public void Clear(InputRecordingType inputRecordingType)
	{
		switch (inputRecordingType)
		{
		case InputRecordingType.PositiveKey:
			PositiveKey = KeyCode.None;
			break;
		case InputRecordingType.NegativeKey:
			NegativeKey = KeyCode.None;
			break;
		case InputRecordingType.AltPositiveKey:
			AltPositiveKey = KeyCode.None;
			break;
		case InputRecordingType.AltNegativeKey:
			AltNegativeKey = KeyCode.None;
			break;
		case InputRecordingType.PositiveXBoxButton:
			PositiveXBoxButton = XBoxButton.None;
			break;
		case InputRecordingType.NegativeXBoxButton:
			NegativeXBoxButton = XBoxButton.None;
			break;
		case InputRecordingType.PositivePS4Button:
			PositivePS4Button = PS4Button.None;
			break;
		case InputRecordingType.NegativePS4Button:
			NegativePS4Button = PS4Button.None;
			break;
		case InputRecordingType.PositiveSteamDeckButton:
			PositiveSteamDeckButton = SteamDeckButton.None;
			break;
		case InputRecordingType.NegativeSteamDeckButton:
			NegativeSteamDeckButton = SteamDeckButton.None;
			break;
		}
	}

	public bool CanPositiveButtonBeCleared()
	{
		InputFunction inputFunction = InputFunction;
		if (inputFunction == InputFunction.MenuSelect || (uint)(inputFunction - 5) <= 2u)
		{
			return false;
		}
		return true;
	}

	public bool CanNegativeButtonBeCleared()
	{
		InputFunction inputFunction = InputFunction;
		if ((uint)(inputFunction - 5) <= 1u)
		{
			return false;
		}
		return true;
	}

	public bool CanBeCleared(InputRecordingType inputRecordingType)
	{
		switch (inputRecordingType)
		{
		case InputRecordingType.PositiveKey:
		case InputRecordingType.PositiveXBoxButton:
		case InputRecordingType.PositivePS4Button:
		case InputRecordingType.PositiveSteamDeckButton:
		case InputRecordingType.PositiveNintendoSwitchButton:
			return CanPositiveButtonBeCleared();
		case InputRecordingType.NegativeKey:
		case InputRecordingType.NegativeXBoxButton:
		case InputRecordingType.NegativePS4Button:
		case InputRecordingType.NegativeSteamDeckButton:
		case InputRecordingType.NegativeNintendoSwitchButton:
			return CanNegativeButtonBeCleared();
		default:
			return true;
		}
	}

	public void Set(InputRecordingType inputRecordingType)
	{
	}
}
