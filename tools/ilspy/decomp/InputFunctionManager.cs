using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.UI;

public class InputFunctionManager
{
	public static InputFunctionManager Instance;

	public static string[] Name = StringUtil.GetEnumNames<InputFunction>("Invalid");

	public static string[] KeyCodeName = new string[330];

	public static string[] MouseAxisName = new string[5];

	public static string[] XBoxButtonName = new string[16];

	public static string[] XBoxAxisName = new string[4];

	public static string[] PS4ButtonName = new string[17];

	public static string[] PS4AxisName = new string[4];

	public static string[] SteamDeckButtonName = new string[16];

	public static string[] NintendoSwitchButtonName = new string[16];

	public KeyCode[] ValidKeyCodes;

	public InputType CurrentInputType;

	public int CurrentInputTypeFrame;

	public float LastInputTime;

	public bool ControllerConnected;

	public bool LostController;

	public static bool IgnoreGamepads;

	public MappedInput[] InputMappings = new MappedInput[78];

	public HandledInput[] HandledKey = new HandledInput[330];

	public HandledInput[] HandledMouseAxis = new HandledInput[5];

	public HandledInput[] HandledXBoxButton = new HandledInput[16];

	public HandledInput[] HandledXBoxAxis = new HandledInput[4];

	public HandledInput[] HandledPS4Button = new HandledInput[17];

	public HandledInput[] HandledPS4Axis = new HandledInput[4];

	public HandledInput[] HandledSteamDeckButton = new HandledInput[16];

	public HandledInput[] HandledSteamDeckAxis = new HandledInput[4];

	public HandledInput[] HandledNintendoSwitchButton = new HandledInput[16];

	public HandledInput[] HandledNintendoSwitchAxis = new HandledInput[4];

	public static float DefaultJoystickDeadZone = 0.1f;

	public static float DefaultTriggerDeadZone = 0.1f;

	public static float JoystickDeadZone = DefaultJoystickDeadZone;

	public static float TriggerDeadZone = DefaultTriggerDeadZone;

	public static float MouseWheelSensitivity = 1f;

	private const string MouseXAxis = "Mouse X Axis";

	private const string MouseYAxis = "Mouse Y Axis";

	private static float XBoxDPadDeadZone = 0.5f;

	private static float PS4DPadDeadZone = 0.5f;

	private static float SteamDeckDPadDeadZone = 0.5f;

	private Vector2 MousePosition;

	private Vector2[] MouseHistory = new Vector2[5];

	private int MouseHistoryIter;

	private Vector2 CursorPos;

	private static float MouseLookNonacceleratedScale = 0.1f;

	private static float MouseLookAcceleratedScale = 0.01f;

	public static float MouseLookSensitivity => GameImpl.Instance.Settings.MouseLookSensitivity;

	public static float MouseTargetSensitivity => GameImpl.Instance.Settings.MouseTargetSensitivity;

	public static float MouseBodyLocationSensitivity => GameImpl.Instance.Settings.MouseBodyLocationSensitivity;

	public static bool MouseLookAcceleration => GameImpl.Instance.Settings.MouseLookAcceleration;

	public static bool IsAxis(InputFunction inputFunction)
	{
		switch (inputFunction)
		{
		case InputFunction.MoveHoriz:
		case InputFunction.MoveVert:
		case InputFunction.RotateCamera:
		case InputFunction.PitchCamera:
		case InputFunction.SwitchTarget:
		case InputFunction.SwitchTargetBodyLocation:
		case InputFunction.CommandModeRotateCamera:
		case InputFunction.PlaySpeed:
		case InputFunction.SelectAction:
		case InputFunction.Control:
		case InputFunction.ControlFollower:
		case InputFunction.MapZoom:
		case InputFunction.RotateBuilding:
		case InputFunction.PlusMinus1:
		case InputFunction.PlusMinus10:
		case InputFunction.PlusMinus100:
		case InputFunction.MapSelectMineral:
		case InputFunction.MapSelectMarkerType:
		case InputFunction.MenuSwitchInventory:
			return true;
		default:
			return false;
		}
	}

	public InputFunctionManager()
	{
		Instance = this;
		KeyCode[] array = (from x in typeof(KeyCode).GetFields(BindingFlags.Static | BindingFlags.Public)
			select (KeyCode)x.GetValue(null)).ToArray();
		int num;
		for (num = 0; num < array.Length && array[num] < KeyCode.JoystickButton0; num++)
		{
		}
		ValidKeyCodes = new KeyCode[num];
		Array.Copy(array, ValidKeyCodes, num);
		for (int num2 = 0; num2 < ValidKeyCodes.Length; num2++)
		{
			KeyCodeName[(int)ValidKeyCodes[num2]] = ValidKeyCodes[num2].ToString();
		}
		for (int num3 = 0; num3 < MouseAxisName.Length; num3++)
		{
			string[] mouseAxisName = MouseAxisName;
			int num4 = num3;
			MouseAxis mouseAxis = (MouseAxis)num3;
			mouseAxisName[num4] = mouseAxis.ToString();
		}
		for (int num5 = 0; num5 < XBoxButtonName.Length; num5++)
		{
			string[] xBoxButtonName = XBoxButtonName;
			int num6 = num5;
			XBoxButton xBoxButton = (XBoxButton)num5;
			xBoxButtonName[num6] = xBoxButton.ToString();
		}
		for (int num7 = 0; num7 < XBoxAxisName.Length; num7++)
		{
			string[] xBoxAxisName = XBoxAxisName;
			int num8 = num7;
			XBoxAxis xBoxAxis = (XBoxAxis)num7;
			xBoxAxisName[num8] = xBoxAxis.ToString();
		}
		for (int num9 = 0; num9 < PS4ButtonName.Length; num9++)
		{
			string[] pS4ButtonName = PS4ButtonName;
			int num10 = num9;
			PS4Button pS4Button = (PS4Button)num9;
			pS4ButtonName[num10] = pS4Button.ToString();
		}
		for (int num11 = 0; num11 < PS4AxisName.Length; num11++)
		{
			string[] pS4AxisName = PS4AxisName;
			int num12 = num11;
			PS4Axis pS4Axis = (PS4Axis)num11;
			pS4AxisName[num12] = pS4Axis.ToString();
		}
		for (int num13 = 0; num13 < SteamDeckButtonName.Length; num13++)
		{
			string[] steamDeckButtonName = SteamDeckButtonName;
			int num14 = num13;
			SteamDeckButton steamDeckButton = (SteamDeckButton)num13;
			steamDeckButtonName[num14] = steamDeckButton.ToString();
		}
		MousePosition = new Vector2(Screen.width, Screen.height) * 0.5f;
		SetupDefaultMappings();
	}

	public void SetupDefaultMappings()
	{
		for (int i = 0; i < InputMappings.Length; i++)
		{
			InputMappings[i] = new MappedInput();
			InputMappings[i].InputFunction = (InputFunction)i;
		}
		InputMappings[0].PositiveKey = KeyCode.Space;
		InputMappings[0].AltPositiveKey = KeyCode.Return;
		InputMappings[1].PositiveKey = KeyCode.Mouse0;
		InputMappings[1].AltPositiveKey = KeyCode.Return;
		InputMappings[2].PositiveKey = KeyCode.Mouse1;
		InputMappings[2].AltPositiveKey = KeyCode.Keypad0;
		InputMappings[3].PositiveKey = KeyCode.Mouse1;
		InputMappings[4].PositiveKey = KeyCode.Mouse0;
		InputMappings[6].PositiveKey = KeyCode.W;
		InputMappings[6].NegativeKey = KeyCode.S;
		InputMappings[6].AltPositiveKey = KeyCode.UpArrow;
		InputMappings[6].AltNegativeKey = KeyCode.DownArrow;
		InputMappings[5].PositiveKey = KeyCode.D;
		InputMappings[5].NegativeKey = KeyCode.A;
		InputMappings[5].AltPositiveKey = KeyCode.RightArrow;
		InputMappings[5].AltNegativeKey = KeyCode.LeftArrow;
		InputMappings[21].PositiveKey = KeyCode.LeftShift;
		InputMappings[21].AltPositiveKey = KeyCode.RightShift;
		InputMappings[20].PositiveKey = KeyCode.Space;
		InputMappings[7].PositiveKey = KeyCode.Escape;
		InputMappings[8].PositiveKey = KeyCode.Escape;
		InputMappings[9].PositiveKey = KeyCode.X;
		InputMappings[9].NegativeKey = KeyCode.Z;
		InputMappings[9].AltPositiveKey = KeyCode.Keypad9;
		InputMappings[9].AltNegativeKey = KeyCode.Keypad7;
		InputMappings[9].MouseAxis = MouseAxis.SmoothedHoriz;
		InputMappings[15].PositiveKey = KeyCode.X;
		InputMappings[15].NegativeKey = KeyCode.Z;
		InputMappings[15].AltPositiveKey = KeyCode.Keypad9;
		InputMappings[15].AltNegativeKey = KeyCode.Keypad7;
		InputMappings[10].MouseAxis = MouseAxis.SmoothedVert;
		InputMappings[11].MouseAxis = MouseAxis.Horiz;
		InputMappings[11].PositiveKey = KeyCode.X;
		InputMappings[11].NegativeKey = KeyCode.Z;
		InputMappings[12].MouseAxis = MouseAxis.Vert;
		InputMappings[13].PositiveKey = KeyCode.LeftControl;
		InputMappings[13].AltPositiveKey = KeyCode.End;
		InputMappings[14].PositiveKey = KeyCode.LeftShift;
		InputMappings[14].AltPositiveKey = KeyCode.RightShift;
		InputMappings[16].PositiveKey = KeyCode.F8;
		InputMappings[17].PositiveKey = KeyCode.LeftShift;
		InputMappings[17].AltPositiveKey = KeyCode.RightShift;
		InputMappings[22].PositiveKey = KeyCode.G;
		InputMappings[22].NegativeKey = KeyCode.F;
		InputMappings[22].AltPositiveKey = KeyCode.Keypad6;
		InputMappings[22].AltNegativeKey = KeyCode.Keypad4;
		InputMappings[23].MouseAxis = MouseAxis.ScrollWheel;
		InputMappings[23].PositiveKey = KeyCode.Keypad8;
		InputMappings[23].NegativeKey = KeyCode.Keypad2;
		InputMappings[24].PositiveKey = KeyCode.Mouse2;
		InputMappings[54].PositiveKey = KeyCode.Mouse2;
		InputMappings[55].PositiveKey = KeyCode.E;
		InputMappings[56].PositiveKey = KeyCode.Q;
		InputMappings[57].PositiveKey = KeyCode.LeftShift;
		InputMappings[57].AltPositiveKey = KeyCode.RightShift;
		InputMappings[25].PositiveKey = KeyCode.E;
		InputMappings[25].NegativeKey = KeyCode.Q;
		InputMappings[25].AltPositiveKey = KeyCode.PageDown;
		InputMappings[25].AltNegativeKey = KeyCode.Delete;
		InputMappings[26].PositiveKey = KeyCode.Y;
		InputMappings[26].NegativeKey = KeyCode.T;
		InputMappings[26].AltPositiveKey = KeyCode.PageUp;
		InputMappings[26].AltNegativeKey = KeyCode.Insert;
		InputMappings[27].PositiveKey = KeyCode.R;
		InputMappings[27].AltPositiveKey = KeyCode.RightControl;
		InputMappings[28].PositiveKey = KeyCode.H;
		InputMappings[30].PositiveKey = KeyCode.B;
		InputMappings[30].AltPositiveKey = KeyCode.N;
		InputMappings[31].PositiveKey = KeyCode.B;
		InputMappings[31].AltPositiveKey = KeyCode.N;
		InputMappings[29].PositiveKey = KeyCode.Space;
		InputMappings[29].AltPositiveKey = KeyCode.Return;
		InputMappings[32].PositiveKey = KeyCode.F1;
		InputMappings[33].PositiveKey = KeyCode.F2;
		InputMappings[34].PositiveKey = KeyCode.F3;
		InputMappings[35].PositiveKey = KeyCode.F4;
		InputMappings[36].PositiveKey = KeyCode.F5;
		InputMappings[37].PositiveKey = KeyCode.Space;
		InputMappings[37].AltPositiveKey = KeyCode.Return;
		InputMappings[38].PositiveKey = KeyCode.C;
		InputMappings[38].AltPositiveKey = KeyCode.Keypad0;
		InputMappings[39].PositiveKey = KeyCode.Equals;
		InputMappings[39].NegativeKey = KeyCode.Minus;
		InputMappings[39].AltPositiveKey = KeyCode.KeypadPlus;
		InputMappings[39].AltNegativeKey = KeyCode.KeypadMinus;
		InputMappings[40].PositiveKey = KeyCode.M;
		InputMappings[60].PositiveKey = KeyCode.P;
		InputMappings[61].PositiveKey = KeyCode.RightBracket;
		InputMappings[61].NegativeKey = KeyCode.LeftBracket;
		InputMappings[62].PositiveKey = KeyCode.X;
		InputMappings[62].NegativeKey = KeyCode.Z;
		InputMappings[63].PositiveKey = KeyCode.X;
		InputMappings[63].NegativeKey = KeyCode.Z;
		InputMappings[41].PositiveKey = KeyCode.Mouse1;
		InputMappings[41].AltPositiveKey = KeyCode.Delete;
		InputMappings[45].PositiveKey = KeyCode.Tab;
		InputMappings[45].AltPositiveKey = KeyCode.Home;
		InputMappings[46].PositiveKey = KeyCode.Comma;
		InputMappings[46].NegativeKey = KeyCode.Period;
		InputMappings[47].PositiveKey = KeyCode.F9;
		InputMappings[48].PositiveKey = KeyCode.F11;
		InputMappings[42].PositiveKey = KeyCode.X;
		InputMappings[42].NegativeKey = KeyCode.Z;
		InputMappings[42].AltPositiveKey = KeyCode.Keypad9;
		InputMappings[42].AltNegativeKey = KeyCode.Keypad7;
		InputMappings[43].PositiveKey = KeyCode.Comma;
		InputMappings[44].PositiveKey = KeyCode.Period;
		InputMappings[49].PositiveKey = KeyCode.X;
		InputMappings[49].NegativeKey = KeyCode.Z;
		InputMappings[50].PositiveKey = KeyCode.RightBracket;
		InputMappings[50].NegativeKey = KeyCode.LeftBracket;
		InputMappings[58].PositiveKey = KeyCode.LeftAlt;
		InputMappings[58].AltPositiveKey = KeyCode.RightAlt;
		InputMappings[59].PositiveKey = KeyCode.K;
		InputMappings[64].PositiveKey = KeyCode.Alpha1;
		InputMappings[65].PositiveKey = KeyCode.Alpha2;
		InputMappings[66].PositiveKey = KeyCode.Alpha3;
		InputMappings[67].PositiveKey = KeyCode.Alpha4;
		InputMappings[68].PositiveKey = KeyCode.Alpha5;
		InputMappings[69].PositiveKey = KeyCode.Alpha6;
		InputMappings[70].PositiveKey = KeyCode.Alpha7;
		InputMappings[71].PositiveKey = KeyCode.Alpha8;
		InputMappings[72].PositiveKey = KeyCode.Alpha9;
		InputMappings[73].PositiveKey = KeyCode.Alpha0;
		InputMappings[77].PositiveKey = KeyCode.LeftShift;
		InputMappings[77].AltPositiveKey = KeyCode.RightShift;
		InputMappings[0].PositiveXBoxButton = XBoxButton.A;
		InputMappings[1].PositiveXBoxButton = XBoxButton.A;
		InputMappings[2].PositiveXBoxButton = XBoxButton.X;
		InputMappings[3].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[4].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[6].XBoxAxis = XBoxAxis.LeftStickVert;
		InputMappings[5].XBoxAxis = XBoxAxis.LeftStickHoriz;
		InputMappings[18].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[19].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[20].PositiveXBoxButton = XBoxButton.B;
		InputMappings[21].PositiveXBoxButton = XBoxButton.X;
		InputMappings[7].PositiveXBoxButton = XBoxButton.B;
		InputMappings[8].PositiveXBoxButton = XBoxButton.Start;
		InputMappings[58].PositiveXBoxButton = XBoxButton.Start;
		InputMappings[9].XBoxAxis = XBoxAxis.RightStickHoriz;
		InputMappings[10].XBoxAxis = XBoxAxis.RightStickVert;
		InputMappings[11].XBoxAxis = XBoxAxis.RightStickHoriz;
		InputMappings[12].XBoxAxis = XBoxAxis.RightStickVert;
		InputMappings[13].PositiveXBoxButton = XBoxButton.Y;
		InputMappings[14].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[15].XBoxAxis = XBoxAxis.RightStickHoriz;
		InputMappings[17].PositiveXBoxButton = XBoxButton.LeftStickClick;
		InputMappings[22].PositiveXBoxButton = XBoxButton.DPadRight;
		InputMappings[22].NegativeXBoxButton = XBoxButton.DPadLeft;
		InputMappings[23].PositiveXBoxButton = XBoxButton.DPadUp;
		InputMappings[23].NegativeXBoxButton = XBoxButton.DPadDown;
		InputMappings[25].PositiveXBoxButton = XBoxButton.RB;
		InputMappings[25].NegativeXBoxButton = XBoxButton.LB;
		InputMappings[27].PositiveXBoxButton = XBoxButton.X;
		InputMappings[28].PositiveXBoxButton = XBoxButton.X;
		InputMappings[32].PositiveXBoxButton = XBoxButton.Back;
		InputMappings[37].PositiveXBoxButton = XBoxButton.A;
		InputMappings[29].PositiveXBoxButton = XBoxButton.X;
		InputMappings[30].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[31].PositiveXBoxButton = XBoxButton.RightStickClick;
		InputMappings[38].PositiveXBoxButton = XBoxButton.B;
		InputMappings[39].XBoxAxis = XBoxAxis.RightStickVert;
		InputMappings[61].PositiveXBoxButton = XBoxButton.RightStickClick;
		InputMappings[62].PositiveXBoxButton = XBoxButton.LeftStickClick;
		InputMappings[63].XBoxAxis = XBoxAxis.RightStickHoriz;
		InputMappings[41].PositiveXBoxButton = XBoxButton.X;
		InputMappings[45].PositiveXBoxButton = XBoxButton.RightStickClick;
		InputMappings[46].PositiveXBoxButton = XBoxButton.DPadRight;
		InputMappings[46].NegativeXBoxButton = XBoxButton.DPadLeft;
		InputMappings[42].XBoxAxis = XBoxAxis.RightStickHoriz;
		InputMappings[42].PositiveXBoxButton = XBoxButton.DPadRight;
		InputMappings[42].NegativeXBoxButton = XBoxButton.DPadLeft;
		InputMappings[43].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[44].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[49].PositiveXBoxButton = XBoxButton.DPadRight;
		InputMappings[49].NegativeXBoxButton = XBoxButton.DPadLeft;
		InputMappings[50].PositiveXBoxButton = XBoxButton.RB;
		InputMappings[50].NegativeXBoxButton = XBoxButton.LB;
		InputMappings[51].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[51].NegativeXBoxButton = XBoxButton.LT;
		InputMappings[52].PositiveXBoxButton = XBoxButton.LeftStickClick;
		InputMappings[53].PositiveXBoxButton = XBoxButton.RightStickClick;
		InputMappings[54].PositiveXBoxButton = XBoxButton.Y;
		InputMappings[56].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[55].PositiveXBoxButton = XBoxButton.RT;
		InputMappings[57].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[77].PositiveXBoxButton = XBoxButton.LT;
		InputMappings[0].PositivePS4Button = PS4Button.Cross;
		InputMappings[1].PositivePS4Button = PS4Button.Cross;
		InputMappings[2].PositivePS4Button = PS4Button.Square;
		InputMappings[3].PositivePS4Button = PS4Button.L2;
		InputMappings[4].PositivePS4Button = PS4Button.R2;
		InputMappings[6].PS4Axis = PS4Axis.LeftStickVert;
		InputMappings[5].PS4Axis = PS4Axis.LeftStickHoriz;
		InputMappings[18].PositivePS4Button = PS4Button.R2;
		InputMappings[19].PositivePS4Button = PS4Button.L2;
		InputMappings[20].PositivePS4Button = PS4Button.Circle;
		InputMappings[21].PositivePS4Button = PS4Button.Square;
		InputMappings[7].PositivePS4Button = PS4Button.Circle;
		InputMappings[8].PositivePS4Button = PS4Button.Options;
		InputMappings[58].PositivePS4Button = PS4Button.Options;
		InputMappings[9].PS4Axis = PS4Axis.RightStickHoriz;
		InputMappings[10].PS4Axis = PS4Axis.RightStickVert;
		InputMappings[11].PS4Axis = PS4Axis.RightStickHoriz;
		InputMappings[12].PS4Axis = PS4Axis.RightStickVert;
		InputMappings[13].PositivePS4Button = PS4Button.Triangle;
		InputMappings[14].PositivePS4Button = PS4Button.L2;
		InputMappings[15].PS4Axis = PS4Axis.RightStickHoriz;
		InputMappings[17].PositivePS4Button = PS4Button.LeftStickClick;
		InputMappings[22].PositivePS4Button = PS4Button.DPadRight;
		InputMappings[22].NegativePS4Button = PS4Button.DPadLeft;
		InputMappings[23].PositivePS4Button = PS4Button.DPadUp;
		InputMappings[23].NegativePS4Button = PS4Button.DPadDown;
		InputMappings[24].PositivePS4Button = PS4Button.Share;
		InputMappings[25].PositivePS4Button = PS4Button.R1;
		InputMappings[25].NegativePS4Button = PS4Button.L1;
		InputMappings[27].PositivePS4Button = PS4Button.Square;
		InputMappings[28].PositivePS4Button = PS4Button.Square;
		InputMappings[32].PositivePS4Button = PS4Button.Touchpad;
		InputMappings[37].PositivePS4Button = PS4Button.Cross;
		InputMappings[29].PositivePS4Button = PS4Button.Square;
		InputMappings[30].PositivePS4Button = PS4Button.R2;
		InputMappings[31].PositivePS4Button = PS4Button.RightStickClick;
		InputMappings[38].PositivePS4Button = PS4Button.Circle;
		InputMappings[39].PS4Axis = PS4Axis.RightStickVert;
		InputMappings[61].PositivePS4Button = PS4Button.RightStickClick;
		InputMappings[62].PositivePS4Button = PS4Button.LeftStickClick;
		InputMappings[63].PS4Axis = PS4Axis.RightStickHoriz;
		InputMappings[41].PositivePS4Button = PS4Button.Square;
		InputMappings[45].PositivePS4Button = PS4Button.RightStickClick;
		InputMappings[46].PositivePS4Button = PS4Button.DPadRight;
		InputMappings[46].NegativePS4Button = PS4Button.DPadLeft;
		InputMappings[42].PS4Axis = PS4Axis.RightStickHoriz;
		InputMappings[42].PositivePS4Button = PS4Button.DPadRight;
		InputMappings[42].NegativePS4Button = PS4Button.DPadLeft;
		InputMappings[43].PositivePS4Button = PS4Button.L2;
		InputMappings[44].PositivePS4Button = PS4Button.R2;
		InputMappings[49].PositivePS4Button = PS4Button.DPadRight;
		InputMappings[49].NegativePS4Button = PS4Button.DPadLeft;
		InputMappings[50].PositivePS4Button = PS4Button.R1;
		InputMappings[50].NegativePS4Button = PS4Button.L1;
		InputMappings[51].PositivePS4Button = PS4Button.R2;
		InputMappings[51].NegativePS4Button = PS4Button.L2;
		InputMappings[52].PositivePS4Button = PS4Button.LeftStickClick;
		InputMappings[53].PositivePS4Button = PS4Button.RightStickClick;
		InputMappings[54].PositivePS4Button = PS4Button.Triangle;
		InputMappings[56].PositivePS4Button = PS4Button.L2;
		InputMappings[55].PositivePS4Button = PS4Button.R2;
		InputMappings[57].PositivePS4Button = PS4Button.L2;
		InputMappings[77].PositivePS4Button = PS4Button.L2;
		InputMappings[0].PositiveSteamDeckButton = SteamDeckButton.A;
		InputMappings[1].PositiveSteamDeckButton = SteamDeckButton.A;
		InputMappings[2].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[3].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[4].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[6].SteamDeckAxis = SteamDeckAxis.LeftStickVert;
		InputMappings[5].SteamDeckAxis = SteamDeckAxis.LeftStickHoriz;
		InputMappings[18].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[19].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[20].PositiveSteamDeckButton = SteamDeckButton.B;
		InputMappings[21].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[7].PositiveSteamDeckButton = SteamDeckButton.B;
		InputMappings[8].PositiveSteamDeckButton = SteamDeckButton.Start;
		InputMappings[58].PositiveSteamDeckButton = SteamDeckButton.Start;
		InputMappings[9].SteamDeckAxis = SteamDeckAxis.RightStickHoriz;
		InputMappings[10].SteamDeckAxis = SteamDeckAxis.RightStickVert;
		InputMappings[11].SteamDeckAxis = SteamDeckAxis.RightStickHoriz;
		InputMappings[12].SteamDeckAxis = SteamDeckAxis.RightStickVert;
		InputMappings[13].PositiveSteamDeckButton = SteamDeckButton.Y;
		InputMappings[14].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[15].SteamDeckAxis = SteamDeckAxis.RightStickHoriz;
		InputMappings[17].PositiveSteamDeckButton = SteamDeckButton.LeftStickClick;
		InputMappings[22].PositiveSteamDeckButton = SteamDeckButton.DPadRight;
		InputMappings[22].NegativeSteamDeckButton = SteamDeckButton.DPadLeft;
		InputMappings[23].PositiveSteamDeckButton = SteamDeckButton.DPadUp;
		InputMappings[23].NegativeSteamDeckButton = SteamDeckButton.DPadDown;
		InputMappings[25].PositiveSteamDeckButton = SteamDeckButton.R1;
		InputMappings[25].NegativeSteamDeckButton = SteamDeckButton.L1;
		InputMappings[27].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[28].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[32].PositiveSteamDeckButton = SteamDeckButton.Select;
		InputMappings[37].PositiveSteamDeckButton = SteamDeckButton.A;
		InputMappings[29].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[30].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[31].PositiveSteamDeckButton = SteamDeckButton.RightStickClick;
		InputMappings[38].PositiveSteamDeckButton = SteamDeckButton.B;
		InputMappings[39].SteamDeckAxis = SteamDeckAxis.RightStickVert;
		InputMappings[61].PositiveSteamDeckButton = SteamDeckButton.RightStickClick;
		InputMappings[62].PositiveSteamDeckButton = SteamDeckButton.LeftStickClick;
		InputMappings[63].SteamDeckAxis = SteamDeckAxis.RightStickHoriz;
		InputMappings[41].PositiveSteamDeckButton = SteamDeckButton.X;
		InputMappings[45].PositiveSteamDeckButton = SteamDeckButton.RightStickClick;
		InputMappings[46].PositiveSteamDeckButton = SteamDeckButton.DPadRight;
		InputMappings[46].NegativeSteamDeckButton = SteamDeckButton.DPadLeft;
		InputMappings[42].SteamDeckAxis = SteamDeckAxis.RightStickHoriz;
		InputMappings[42].PositiveSteamDeckButton = SteamDeckButton.DPadRight;
		InputMappings[42].NegativeSteamDeckButton = SteamDeckButton.DPadLeft;
		InputMappings[43].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[44].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[49].PositiveSteamDeckButton = SteamDeckButton.DPadRight;
		InputMappings[49].NegativeSteamDeckButton = SteamDeckButton.DPadLeft;
		InputMappings[50].PositiveSteamDeckButton = SteamDeckButton.R1;
		InputMappings[50].NegativeSteamDeckButton = SteamDeckButton.L1;
		InputMappings[51].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[51].NegativeSteamDeckButton = SteamDeckButton.L2;
		InputMappings[52].PositiveSteamDeckButton = SteamDeckButton.LeftStickClick;
		InputMappings[53].PositiveSteamDeckButton = SteamDeckButton.RightStickClick;
		InputMappings[54].PositiveSteamDeckButton = SteamDeckButton.Y;
		InputMappings[56].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[55].PositiveSteamDeckButton = SteamDeckButton.R2;
		InputMappings[57].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[77].PositiveSteamDeckButton = SteamDeckButton.L2;
		InputMappings[10].AxisInverted = true;
	}

	private static float ApplyJoystickDeadZone(float v)
	{
		return Mathf.Clamp01((Mathf.Abs(v) - JoystickDeadZone) / (1f - JoystickDeadZone)) * Mathf.Sign(v);
	}

	private static Vector2 ApplyJoystickDeadZone(Vector2 v)
	{
		float magnitude = v.magnitude;
		if (magnitude < 0.0001f)
		{
			return Vector2.zero;
		}
		return Mathf.Clamp01((magnitude - JoystickDeadZone) / (1f - JoystickDeadZone)) * (v / magnitude);
	}

	private static float ApplyTriggerDeadZone(float v)
	{
		return Mathf.Clamp01((Mathf.Abs(v) - TriggerDeadZone) / (1f - TriggerDeadZone)) * Mathf.Sign(v);
	}

	public void Update()
	{
		if (Input.mousePresent)
		{
			MousePosition = MathUtil.ToXY(Input.mousePosition);
		}
		RectTransformUtility.ScreenPointToLocalPointInRectangle(HudBehaviour.Instance.HudPanelRectTransform, MousePosition, null, out var localPoint);
		CursorPos = localPoint + HudBehaviour.Instance.Centre;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		for (int i = 0; i < ValidKeyCodes.Length; i++)
		{
			KeyCode keyCode = ValidKeyCodes[i];
			float num = (Input.GetKey(keyCode) ? 1f : 0f);
			flag |= HandledKey[(int)keyCode].Update(num, num);
		}
		for (int j = 0; j < HandledMouseAxis.Length; j++)
		{
			float num2 = 0f;
			switch ((MouseAxis)j)
			{
			case MouseAxis.Horiz:
				num2 = Input.GetAxisRaw("Mouse X Axis");
				MouseHistory[MouseHistoryIter].x = num2;
				break;
			case MouseAxis.Vert:
				num2 = Input.GetAxisRaw("Mouse Y Axis");
				MouseHistory[MouseHistoryIter].y = num2;
				break;
			case MouseAxis.ScrollWheel:
				num2 = Mathf.Clamp(Input.mouseScrollDelta.y * MouseWheelSensitivity, -1f, 1f);
				break;
			case MouseAxis.SmoothedHoriz:
			{
				num2 = 0f;
				for (int l = 0; l < MouseHistory.Length; l++)
				{
					num2 += MouseHistory[l].x;
				}
				num2 /= (float)MouseHistory.Length;
				break;
			}
			case MouseAxis.SmoothedVert:
			{
				num2 = 0f;
				for (int k = 0; k < MouseHistory.Length; k++)
				{
					num2 += MouseHistory[k].y;
				}
				num2 /= (float)MouseHistory.Length;
				break;
			}
			}
			flag |= HandledMouseAxis[j].Update(num2, num2);
		}
		MouseHistoryIter = (MouseHistoryIter + 1) % MouseHistory.Length;
		bool flag5 = false;
		bool flag6 = false;
		DualShockGamepad dualShockGamepad = null;
		Gamepad gamepad = null;
		if (!IgnoreGamepads)
		{
			dualShockGamepad = DualShockGamepad.current;
			flag5 = dualShockGamepad != null;
			if (!(Gamepad.current is DualShockGamepad))
			{
				gamepad = Gamepad.current;
			}
			if (gamepad == null)
			{
				foreach (Gamepad item in Gamepad.all)
				{
					if (!(item is DualShockGamepad) && (gamepad == null || item.lastUpdateTime > gamepad.lastUpdateTime))
					{
						gamepad = item;
					}
				}
			}
			flag6 = gamepad != null;
		}
		for (int m = 0; m < HandledPS4Button.Length; m++)
		{
			float num3 = 0f;
			if (dualShockGamepad != null && Application.isFocused)
			{
				switch ((PS4Button)m)
				{
				case PS4Button.Cross:
					num3 = dualShockGamepad.crossButton.ReadValue();
					break;
				case PS4Button.Circle:
					num3 = dualShockGamepad.circleButton.ReadValue();
					break;
				case PS4Button.Square:
					num3 = dualShockGamepad.squareButton.ReadValue();
					break;
				case PS4Button.Triangle:
					num3 = dualShockGamepad.triangleButton.ReadValue();
					break;
				case PS4Button.L1:
					num3 = dualShockGamepad.leftShoulder.ReadValue();
					break;
				case PS4Button.R1:
					num3 = dualShockGamepad.rightShoulder.ReadValue();
					break;
				case PS4Button.L2:
					num3 = ApplyTriggerDeadZone(dualShockGamepad.leftTrigger.ReadValue());
					break;
				case PS4Button.R2:
					num3 = ApplyTriggerDeadZone(dualShockGamepad.rightTrigger.ReadValue());
					break;
				case PS4Button.DPadRight:
				{
					Vector2 vector2 = dualShockGamepad.dpad.ReadValue();
					num3 = Mathf.Max(0f, (Math.Abs(vector2.x) > Math.Abs(vector2.y) && Math.Abs(vector2.x) >= PS4DPadDeadZone) ? vector2.x : 0f);
					break;
				}
				case PS4Button.DPadLeft:
				{
					Vector2 vector4 = dualShockGamepad.dpad.ReadValue();
					num3 = Mathf.Max(0f, (Math.Abs(vector4.x) > Math.Abs(vector4.y) && Math.Abs(vector4.x) >= PS4DPadDeadZone) ? (0f - vector4.x) : 0f);
					break;
				}
				case PS4Button.DPadUp:
				{
					Vector2 vector3 = dualShockGamepad.dpad.ReadValue();
					num3 = Mathf.Max(0f, (Math.Abs(vector3.y) > Math.Abs(vector3.x) && Math.Abs(vector3.y) >= PS4DPadDeadZone) ? vector3.y : 0f);
					break;
				}
				case PS4Button.DPadDown:
				{
					Vector2 vector = dualShockGamepad.dpad.ReadValue();
					num3 = Mathf.Max(0f, (Math.Abs(vector.y) > Math.Abs(vector.x) && Math.Abs(vector.y) >= PS4DPadDeadZone) ? (0f - vector.y) : 0f);
					break;
				}
				case PS4Button.Share:
					num3 = dualShockGamepad.shareButton.ReadValue();
					break;
				case PS4Button.Options:
					num3 = dualShockGamepad.optionsButton.ReadValue();
					break;
				case PS4Button.Touchpad:
					num3 = dualShockGamepad.touchpadButton.ReadValue();
					break;
				case PS4Button.LeftStickClick:
					num3 = dualShockGamepad.leftStickButton.ReadValue();
					break;
				case PS4Button.RightStickClick:
					num3 = dualShockGamepad.rightStickButton.ReadValue();
					break;
				}
			}
			flag3 |= HandledPS4Button[m].Update(num3, num3);
		}
		for (int n = 0; n < HandledPS4Axis.Length; n++)
		{
			float num4 = 0f;
			if (dualShockGamepad != null)
			{
				switch ((PS4Axis)n)
				{
				case PS4Axis.LeftStickHoriz:
					num4 = dualShockGamepad.leftStick.x.ReadValue();
					break;
				case PS4Axis.LeftStickVert:
					num4 = dualShockGamepad.leftStick.y.ReadValue();
					break;
				case PS4Axis.RightStickHoriz:
					num4 = dualShockGamepad.rightStick.x.ReadValue();
					break;
				case PS4Axis.RightStickVert:
					num4 = dualShockGamepad.rightStick.y.ReadValue();
					break;
				}
			}
			flag3 |= HandledPS4Axis[n].Update(ApplyJoystickDeadZone(num4), num4);
		}
		for (int num5 = 0; num5 < HandledXBoxButton.Length; num5++)
		{
			float num6 = 0f;
			if (gamepad != null)
			{
				switch ((XBoxButton)num5)
				{
				case XBoxButton.A:
					num6 = gamepad.aButton.ReadValue();
					break;
				case XBoxButton.B:
					num6 = gamepad.bButton.ReadValue();
					break;
				case XBoxButton.X:
					num6 = gamepad.xButton.ReadValue();
					break;
				case XBoxButton.Y:
					num6 = gamepad.yButton.ReadValue();
					break;
				case XBoxButton.LB:
					num6 = gamepad.leftShoulder.ReadValue();
					break;
				case XBoxButton.RB:
					num6 = gamepad.rightShoulder.ReadValue();
					break;
				case XBoxButton.LT:
					num6 = ApplyTriggerDeadZone(gamepad.leftTrigger.ReadValue());
					break;
				case XBoxButton.RT:
					num6 = ApplyTriggerDeadZone(gamepad.rightTrigger.ReadValue());
					break;
				case XBoxButton.DPadRight:
				{
					Vector2 vector6 = gamepad.dpad.ReadValue();
					num6 = Mathf.Max(0f, (Math.Abs(vector6.x) > Math.Abs(vector6.y) && Math.Abs(vector6.x) >= XBoxDPadDeadZone) ? vector6.x : 0f);
					break;
				}
				case XBoxButton.DPadLeft:
				{
					Vector2 vector8 = gamepad.dpad.ReadValue();
					num6 = Mathf.Max(0f, (Math.Abs(vector8.x) > Math.Abs(vector8.y) && Math.Abs(vector8.x) >= XBoxDPadDeadZone) ? (0f - vector8.x) : 0f);
					break;
				}
				case XBoxButton.DPadUp:
				{
					Vector2 vector7 = gamepad.dpad.ReadValue();
					num6 = Mathf.Max(0f, (Math.Abs(vector7.y) > Math.Abs(vector7.x) && Math.Abs(vector7.y) >= XBoxDPadDeadZone) ? vector7.y : 0f);
					break;
				}
				case XBoxButton.DPadDown:
				{
					Vector2 vector5 = gamepad.dpad.ReadValue();
					num6 = Mathf.Max(0f, (Math.Abs(vector5.y) > Math.Abs(vector5.x) && Math.Abs(vector5.y) >= XBoxDPadDeadZone) ? (0f - vector5.y) : 0f);
					break;
				}
				case XBoxButton.Back:
					num6 = gamepad.selectButton.ReadValue();
					break;
				case XBoxButton.Start:
					num6 = gamepad.startButton.ReadValue();
					break;
				case XBoxButton.LeftStickClick:
					num6 = gamepad.leftStickButton.ReadValue();
					break;
				case XBoxButton.RightStickClick:
					num6 = gamepad.rightStickButton.ReadValue();
					break;
				}
			}
			flag2 |= HandledXBoxButton[num5].Update(num6, num6);
		}
		for (int num7 = 0; num7 < HandledXBoxAxis.Length; num7++)
		{
			float num8 = 0f;
			if (gamepad != null)
			{
				switch ((XBoxAxis)num7)
				{
				case XBoxAxis.LeftStickHoriz:
					num8 = gamepad.leftStick.x.ReadValue();
					break;
				case XBoxAxis.LeftStickVert:
					num8 = gamepad.leftStick.y.ReadValue();
					break;
				case XBoxAxis.RightStickHoriz:
					num8 = gamepad.rightStick.x.ReadValue();
					break;
				case XBoxAxis.RightStickVert:
					num8 = gamepad.rightStick.y.ReadValue();
					break;
				}
			}
			flag2 |= HandledXBoxAxis[num7].Update(ApplyJoystickDeadZone(num8), num8);
		}
		if (GameImpl.Instance.IsSteamDeck() && !IgnoreGamepads)
		{
			flag4 = true;
			for (int num9 = 0; num9 < HandledSteamDeckButton.Length; num9++)
			{
				float num10 = 0f;
				if (gamepad != null)
				{
					switch ((SteamDeckButton)num9)
					{
					case SteamDeckButton.A:
						num10 = gamepad.aButton.ReadValue();
						break;
					case SteamDeckButton.B:
						num10 = gamepad.bButton.ReadValue();
						break;
					case SteamDeckButton.X:
						num10 = gamepad.xButton.ReadValue();
						break;
					case SteamDeckButton.Y:
						num10 = gamepad.yButton.ReadValue();
						break;
					case SteamDeckButton.L1:
						num10 = gamepad.leftShoulder.ReadValue();
						break;
					case SteamDeckButton.R1:
						num10 = gamepad.rightShoulder.ReadValue();
						break;
					case SteamDeckButton.L2:
						num10 = ApplyTriggerDeadZone(gamepad.leftTrigger.ReadValue());
						break;
					case SteamDeckButton.R2:
						num10 = ApplyTriggerDeadZone(gamepad.rightTrigger.ReadValue());
						break;
					case SteamDeckButton.DPadRight:
					{
						Vector2 vector10 = gamepad.dpad.ReadValue();
						num10 = Mathf.Max(0f, (Math.Abs(vector10.x) > Math.Abs(vector10.y) && Math.Abs(vector10.x) >= SteamDeckDPadDeadZone) ? vector10.x : 0f);
						break;
					}
					case SteamDeckButton.DPadLeft:
					{
						Vector2 vector12 = gamepad.dpad.ReadValue();
						num10 = Mathf.Max(0f, (Math.Abs(vector12.x) > Math.Abs(vector12.y) && Math.Abs(vector12.x) >= SteamDeckDPadDeadZone) ? (0f - vector12.x) : 0f);
						break;
					}
					case SteamDeckButton.DPadUp:
					{
						Vector2 vector11 = gamepad.dpad.ReadValue();
						num10 = Mathf.Max(0f, (Math.Abs(vector11.y) > Math.Abs(vector11.x) && Math.Abs(vector11.y) >= SteamDeckDPadDeadZone) ? vector11.y : 0f);
						break;
					}
					case SteamDeckButton.DPadDown:
					{
						Vector2 vector9 = gamepad.dpad.ReadValue();
						num10 = Mathf.Max(0f, (Math.Abs(vector9.y) > Math.Abs(vector9.x) && Math.Abs(vector9.y) >= SteamDeckDPadDeadZone) ? (0f - vector9.y) : 0f);
						break;
					}
					case SteamDeckButton.Select:
						num10 = gamepad.selectButton.ReadValue();
						break;
					case SteamDeckButton.Start:
						num10 = gamepad.startButton.ReadValue();
						break;
					case SteamDeckButton.LeftStickClick:
						num10 = gamepad.leftStickButton.ReadValue();
						break;
					case SteamDeckButton.RightStickClick:
						num10 = gamepad.rightStickButton.ReadValue();
						break;
					}
				}
				HandledSteamDeckButton[num9].Update(num10, num10);
			}
			for (int num11 = 0; num11 < HandledSteamDeckAxis.Length; num11++)
			{
				float num12 = 0f;
				if (gamepad != null)
				{
					switch ((SteamDeckAxis)num11)
					{
					case SteamDeckAxis.LeftStickHoriz:
						num12 = gamepad.leftStick.x.ReadValue();
						break;
					case SteamDeckAxis.LeftStickVert:
						num12 = gamepad.leftStick.y.ReadValue();
						break;
					case SteamDeckAxis.RightStickHoriz:
						num12 = gamepad.rightStick.x.ReadValue();
						break;
					case SteamDeckAxis.RightStickVert:
						num12 = gamepad.rightStick.y.ReadValue();
						break;
					}
				}
				HandledSteamDeckAxis[num11].Update(ApplyJoystickDeadZone(num12), num12);
			}
		}
		bool flag7 = false;
		InputType currentInputType = CurrentInputType;
		if (flag)
		{
			CurrentInputType = InputType.MouseAndKeyboard;
			flag7 = true;
		}
		if (flag2)
		{
			CurrentInputType = InputType.XBox;
			flag7 = true;
		}
		if (flag3)
		{
			CurrentInputType = InputType.PS4;
			flag7 = true;
		}
		if (flag4)
		{
			CurrentInputType = InputType.SteamDeck;
			flag7 = true;
		}
		if (CurrentInputType != currentInputType)
		{
			CurrentInputTypeFrame = Time.frameCount;
		}
		if (flag7)
		{
			LastInputTime = GameImpl.UnscaledTime;
		}
		LostController = false;
		switch (CurrentInputType)
		{
		case InputType.XBox:
			if (ControllerConnected && !flag6)
			{
				LostController = true;
			}
			ControllerConnected = flag6;
			break;
		case InputType.SteamDeck:
			if (ControllerConnected && !flag6)
			{
				LostController = true;
			}
			ControllerConnected = flag6;
			break;
		case InputType.PS4:
			if (ControllerConnected && !flag5)
			{
				LostController = true;
			}
			ControllerConnected = flag5;
			break;
		}
		if (HandledMouseAxis[0].Value != HandledMouseAxis[0].LastValue && HandledMouseAxis[1].Value != HandledMouseAxis[1].LastValue && SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor)
		{
			SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Cursor;
		}
		if (GameImpl.IsShowingOSK || GameImpl.FinishedShowingOSKCountdown > 0)
		{
			for (PS4Button pS4Button = PS4Button.Cross; pS4Button < PS4Button.Count; pS4Button++)
			{
				CapturePS4Button(pS4Button, Captured.UntilReleased);
			}
			for (PS4Axis pS4Axis = PS4Axis.LeftStickHoriz; pS4Axis < PS4Axis.Count; pS4Axis++)
			{
				CapturePS4Axis(pS4Axis, Captured.UntilReleased);
			}
			for (XBoxButton xBoxButton = XBoxButton.A; xBoxButton < XBoxButton.Count; xBoxButton++)
			{
				CaptureXBoxButton(xBoxButton, Captured.UntilReleased);
			}
			for (XBoxAxis xBoxAxis = XBoxAxis.LeftStickHoriz; xBoxAxis < XBoxAxis.Count; xBoxAxis++)
			{
				CaptureXBoxAxis(xBoxAxis, Captured.UntilReleased);
			}
			for (SteamDeckButton steamDeckButton = SteamDeckButton.A; steamDeckButton < SteamDeckButton.Count; steamDeckButton++)
			{
				CaptureSteamDeckButton(steamDeckButton, Captured.UntilReleased);
			}
			for (SteamDeckAxis steamDeckAxis = SteamDeckAxis.LeftStickHoriz; steamDeckAxis < SteamDeckAxis.Count; steamDeckAxis++)
			{
				CaptureSteamDeckAxis(steamDeckAxis, Captured.UntilReleased);
			}
			GameImpl.FinishedShowingOSKCountdown--;
		}
	}

	private bool ShouldIgnoreCapture(InputFunction capturedBy, InputFunction ignore)
	{
		if (capturedBy != InputFunction.Invalid)
		{
			return capturedBy == ignore;
		}
		return false;
	}

	public void CaptureKey(KeyCode key, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (key != KeyCode.None)
		{
			HandledKey[(int)key].Capture(captureType, captureBy);
		}
	}

	public bool IsKeyPressed(KeyCode key, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (key == KeyCode.None)
		{
			return false;
		}
		return HandledKey[(int)key].IsPressed(capture, captureBy);
	}

	public bool IsKeyJustPressed(KeyCode key, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (key == KeyCode.None)
		{
			return false;
		}
		return HandledKey[(int)key].IsJustPressed(capture, captureBy);
	}

	public bool IsKeyJustReleased(KeyCode key)
	{
		if (key == KeyCode.None)
		{
			return false;
		}
		return HandledKey[(int)key].IsJustReleased();
	}

	public bool IsKeyCaptured(KeyCode key, InputFunction ignore = InputFunction.Invalid)
	{
		if (key != KeyCode.None && HandledKey[(int)key].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledKey[(int)key].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureMouseAxis(MouseAxis mouseAxis, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (mouseAxis != MouseAxis.None)
		{
			HandledMouseAxis[(int)mouseAxis].Capture(captureType, captureBy);
		}
	}

	public float GetMouseAxis(MouseAxis mouseAxis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (mouseAxis == MouseAxis.None)
		{
			return 0f;
		}
		return HandledMouseAxis[(int)mouseAxis].GetAxis(capture, captureBy);
	}

	public bool IsMouseAxisCaptured(MouseAxis mouseAxis, InputFunction ignore = InputFunction.Invalid)
	{
		if (mouseAxis != MouseAxis.None && HandledMouseAxis[(int)mouseAxis].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledMouseAxis[(int)mouseAxis].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureXBoxButton(XBoxButton button, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button != XBoxButton.None)
		{
			HandledXBoxButton[(int)button].Capture(captureType, captureBy);
		}
	}

	public bool IsXBoxButtonPressed(XBoxButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == XBoxButton.None)
		{
			return false;
		}
		return HandledXBoxButton[(int)button].IsPressed(capture, captureBy);
	}

	public bool IsXBoxButtonJustPressed(XBoxButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == XBoxButton.None)
		{
			return false;
		}
		return HandledXBoxButton[(int)button].IsJustPressed(capture, captureBy);
	}

	public bool IsXBoxButtonJustReleased(XBoxButton button)
	{
		if (button == XBoxButton.None)
		{
			return false;
		}
		return HandledXBoxButton[(int)button].IsJustReleased();
	}

	public float GetXBoxButtonPressedAmount(XBoxButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == XBoxButton.None)
		{
			return 0f;
		}
		return HandledXBoxButton[(int)button].GetAxis(capture, captureBy);
	}

	public bool IsXBoxButtonCaptured(XBoxButton button, InputFunction ignore = InputFunction.Invalid)
	{
		if (button != XBoxButton.None && HandledXBoxButton[(int)button].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledXBoxButton[(int)button].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureXBoxAxis(XBoxAxis axis, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis != XBoxAxis.None)
		{
			HandledXBoxAxis[(int)axis].Capture(captureType, captureBy);
		}
	}

	public float GetXBoxAxis(XBoxAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == XBoxAxis.None)
		{
			return 0f;
		}
		return HandledXBoxAxis[(int)axis].GetAxis(capture, captureBy);
	}

	public float GetXBoxAxisRaw(XBoxAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == XBoxAxis.None)
		{
			return 0f;
		}
		return HandledXBoxAxis[(int)axis].GetAxisRaw(capture, captureBy);
	}

	public bool IsXBoxAxisCaptured(XBoxAxis axis, InputFunction ignore = InputFunction.Invalid)
	{
		if (axis != XBoxAxis.None && HandledXBoxAxis[(int)axis].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledXBoxAxis[(int)axis].CapturedBy, ignore);
		}
		return false;
	}

	public void CapturePS4Button(PS4Button button, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button != PS4Button.None)
		{
			HandledPS4Button[(int)button].Capture(captureType, captureBy);
		}
	}

	public bool IsPS4ButtonPressed(PS4Button button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == PS4Button.None)
		{
			return false;
		}
		return HandledPS4Button[(int)button].IsPressed(capture, captureBy);
	}

	public bool IsPS4ButtonJustPressed(PS4Button button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == PS4Button.None)
		{
			return false;
		}
		return HandledPS4Button[(int)button].IsJustPressed(capture, captureBy);
	}

	public bool IsPS4ButtonJustReleased(PS4Button button)
	{
		if (button == PS4Button.None)
		{
			return false;
		}
		return HandledPS4Button[(int)button].IsJustReleased();
	}

	public float GetPS4ButtonPressedAmount(PS4Button button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == PS4Button.None)
		{
			return 0f;
		}
		return HandledPS4Button[(int)button].GetAxis(capture, captureBy);
	}

	public bool IsPS4ButtonCaptured(PS4Button button, InputFunction ignore = InputFunction.Invalid)
	{
		if (button != PS4Button.None && HandledPS4Button[(int)button].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledPS4Button[(int)button].CapturedBy, ignore);
		}
		return false;
	}

	public void CapturePS4Axis(PS4Axis axis, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis != PS4Axis.None)
		{
			HandledPS4Axis[(int)axis].Capture(captureType, captureBy);
		}
	}

	public float GetPS4Axis(PS4Axis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == PS4Axis.None)
		{
			return 0f;
		}
		return HandledPS4Axis[(int)axis].GetAxis(capture, captureBy);
	}

	public float GetPS4AxisRaw(PS4Axis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == PS4Axis.None)
		{
			return 0f;
		}
		return HandledPS4Axis[(int)axis].GetAxisRaw(capture, captureBy);
	}

	public bool IsPS4AxisCaptured(PS4Axis axis, InputFunction ignore = InputFunction.Invalid)
	{
		if (axis != PS4Axis.None && HandledPS4Axis[(int)axis].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledPS4Axis[(int)axis].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureSteamDeckButton(SteamDeckButton button, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button != SteamDeckButton.None)
		{
			HandledSteamDeckButton[(int)button].Capture(captureType, captureBy);
		}
	}

	public bool IsSteamDeckButtonPressed(SteamDeckButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == SteamDeckButton.None)
		{
			return false;
		}
		return HandledSteamDeckButton[(int)button].IsPressed(capture, captureBy);
	}

	public bool IsSteamDeckButtonJustPressed(SteamDeckButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == SteamDeckButton.None)
		{
			return false;
		}
		return HandledSteamDeckButton[(int)button].IsJustPressed(capture, captureBy);
	}

	public bool IsSteamDeckButtonJustReleased(SteamDeckButton button)
	{
		if (button == SteamDeckButton.None)
		{
			return false;
		}
		return HandledSteamDeckButton[(int)button].IsJustReleased();
	}

	public float GetSteamDeckButtonPressedAmount(SteamDeckButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == SteamDeckButton.None)
		{
			return 0f;
		}
		return HandledSteamDeckButton[(int)button].GetAxis(capture, captureBy);
	}

	public bool IsSteamDeckButtonCaptured(SteamDeckButton button, InputFunction ignore = InputFunction.Invalid)
	{
		if (button != SteamDeckButton.None && HandledSteamDeckButton[(int)button].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledSteamDeckButton[(int)button].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureSteamDeckAxis(SteamDeckAxis axis, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis != SteamDeckAxis.None)
		{
			HandledSteamDeckAxis[(int)axis].Capture(captureType, captureBy);
		}
	}

	public float GetSteamDeckAxis(SteamDeckAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == SteamDeckAxis.None)
		{
			return 0f;
		}
		return HandledSteamDeckAxis[(int)axis].GetAxis(capture, captureBy);
	}

	public float GetSteamDeckAxisRaw(SteamDeckAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == SteamDeckAxis.None)
		{
			return 0f;
		}
		return HandledSteamDeckAxis[(int)axis].GetAxisRaw(capture, captureBy);
	}

	public bool IsSteamDeckAxisCaptured(SteamDeckAxis axis, InputFunction ignore = InputFunction.Invalid)
	{
		if (axis != SteamDeckAxis.None && HandledSteamDeckAxis[(int)axis].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledSteamDeckAxis[(int)axis].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureNintendoSwitchButton(NintendoSwitchButton button, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button != NintendoSwitchButton.None)
		{
			HandledNintendoSwitchButton[(int)button].Capture(captureType, captureBy);
		}
	}

	public bool IsNintendoSwitchButtonPressed(NintendoSwitchButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == NintendoSwitchButton.None)
		{
			return false;
		}
		return HandledNintendoSwitchButton[(int)button].IsPressed(capture, captureBy);
	}

	public bool IsNintendoSwitchButtonJustPressed(NintendoSwitchButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == NintendoSwitchButton.None)
		{
			return false;
		}
		return HandledNintendoSwitchButton[(int)button].IsJustPressed(capture, captureBy);
	}

	public bool IsNintendoSwitchButtonJustReleased(NintendoSwitchButton button)
	{
		if (button == NintendoSwitchButton.None)
		{
			return false;
		}
		return HandledNintendoSwitchButton[(int)button].IsJustReleased();
	}

	public float GetNintendoSwitchButtonPressedAmount(NintendoSwitchButton button, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (button == NintendoSwitchButton.None)
		{
			return 0f;
		}
		return HandledNintendoSwitchButton[(int)button].GetAxis(capture, captureBy);
	}

	public bool IsNintendoSwitchButtonCaptured(NintendoSwitchButton button, InputFunction ignore = InputFunction.Invalid)
	{
		if (button != NintendoSwitchButton.None && HandledNintendoSwitchButton[(int)button].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledNintendoSwitchButton[(int)button].CapturedBy, ignore);
		}
		return false;
	}

	public void CaptureNintendoSwitchAxis(NintendoSwitchAxis axis, Captured captureType, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis != NintendoSwitchAxis.None)
		{
			HandledNintendoSwitchAxis[(int)axis].Capture(captureType, captureBy);
		}
	}

	public float GetNintendoSwitchAxis(NintendoSwitchAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == NintendoSwitchAxis.None)
		{
			return 0f;
		}
		return HandledNintendoSwitchAxis[(int)axis].GetAxis(capture, captureBy);
	}

	public float GetNintendoSwitchAxisRaw(NintendoSwitchAxis axis, bool capture = true, InputFunction captureBy = InputFunction.Invalid)
	{
		if (axis == NintendoSwitchAxis.None)
		{
			return 0f;
		}
		return HandledNintendoSwitchAxis[(int)axis].GetAxisRaw(capture, captureBy);
	}

	public bool IsNintendoSwitchAxisCaptured(NintendoSwitchAxis axis, InputFunction ignore = InputFunction.Invalid)
	{
		if (axis != NintendoSwitchAxis.None && HandledNintendoSwitchAxis[(int)axis].Captured != Captured.None)
		{
			return !ShouldIgnoreCapture(HandledNintendoSwitchAxis[(int)axis].CapturedBy, ignore);
		}
		return false;
	}

	public bool ShouldShowButtonPrompt(InputFunction inputFunction)
	{
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		if (CurrentInputType == InputType.SteamDeck)
		{
			if (IsAxis(inputFunction))
			{
				if (mappedInput.SteamDeckAxis != SteamDeckAxis.None)
				{
					return !IsSteamDeckAxisCaptured(mappedInput.SteamDeckAxis, inputFunction);
				}
				if (mappedInput.NegativeSteamDeckButton != SteamDeckButton.None || mappedInput.PositiveSteamDeckButton != SteamDeckButton.None)
				{
					if (!IsSteamDeckButtonCaptured(mappedInput.PositiveSteamDeckButton, inputFunction))
					{
						return !IsSteamDeckButtonCaptured(mappedInput.NegativeSteamDeckButton, inputFunction);
					}
					return false;
				}
			}
			else if (mappedInput.PositiveSteamDeckButton != SteamDeckButton.None)
			{
				return !IsSteamDeckButtonCaptured(mappedInput.PositiveSteamDeckButton, inputFunction);
			}
		}
		if (CurrentInputType == InputType.XBox)
		{
			if (IsAxis(inputFunction))
			{
				if (mappedInput.XBoxAxis != XBoxAxis.None)
				{
					return !IsXBoxAxisCaptured(mappedInput.XBoxAxis, inputFunction);
				}
				if (mappedInput.NegativeXBoxButton != XBoxButton.None || mappedInput.PositiveXBoxButton != XBoxButton.None)
				{
					if (!IsXBoxButtonCaptured(mappedInput.PositiveXBoxButton, inputFunction))
					{
						return !IsXBoxButtonCaptured(mappedInput.NegativeXBoxButton, inputFunction);
					}
					return false;
				}
			}
			else if (mappedInput.PositiveXBoxButton != XBoxButton.None)
			{
				return !IsXBoxButtonCaptured(mappedInput.PositiveXBoxButton, inputFunction);
			}
		}
		if (CurrentInputType == InputType.PS4)
		{
			if (IsAxis(inputFunction))
			{
				if (mappedInput.PS4Axis != PS4Axis.None)
				{
					return !IsPS4AxisCaptured(mappedInput.PS4Axis, inputFunction);
				}
				if (mappedInput.NegativePS4Button != PS4Button.None || mappedInput.PositivePS4Button != PS4Button.None)
				{
					if (!IsPS4ButtonCaptured(mappedInput.PositivePS4Button, inputFunction))
					{
						return !IsPS4ButtonCaptured(mappedInput.NegativePS4Button, inputFunction);
					}
					return false;
				}
			}
			else if (mappedInput.PositivePS4Button != PS4Button.None)
			{
				return !IsPS4ButtonCaptured(mappedInput.PositivePS4Button, inputFunction);
			}
		}
		if (IsAxis(inputFunction))
		{
			if (mappedInput.MouseAxis != MouseAxis.None)
			{
				return !IsMouseAxisCaptured(mappedInput.MouseAxis, inputFunction);
			}
			if (mappedInput.NegativeKey != KeyCode.None || mappedInput.PositiveKey != KeyCode.None)
			{
				if (!IsKeyCaptured(mappedInput.PositiveKey, inputFunction))
				{
					return !IsKeyCaptured(mappedInput.NegativeKey, inputFunction);
				}
				return false;
			}
			if (mappedInput.AltNegativeKey != KeyCode.None || mappedInput.AltPositiveKey != KeyCode.None)
			{
				if (!IsKeyCaptured(mappedInput.AltPositiveKey, inputFunction))
				{
					return !IsKeyCaptured(mappedInput.NegativeKey, inputFunction);
				}
				return false;
			}
		}
		else
		{
			if (mappedInput.PositiveKey != KeyCode.None)
			{
				return !IsKeyCaptured(mappedInput.PositiveKey, inputFunction);
			}
			if (mappedInput.AltPositiveKey != KeyCode.None)
			{
				return !IsKeyCaptured(mappedInput.AltPositiveKey, inputFunction);
			}
		}
		return false;
	}

	public static bool CanHaveNegative(InputFunction inputFunction)
	{
		switch (inputFunction)
		{
		case InputFunction.MoveHoriz:
		case InputFunction.MoveVert:
		case InputFunction.RotateCamera:
		case InputFunction.PitchCamera:
		case InputFunction.SwitchTarget:
		case InputFunction.SwitchTargetBodyLocation:
		case InputFunction.CommandModeRotateCamera:
		case InputFunction.PlaySpeed:
		case InputFunction.SelectAction:
		case InputFunction.Control:
		case InputFunction.ControlFollower:
		case InputFunction.MapZoom:
		case InputFunction.CharacterPreviewRotate:
		case InputFunction.RotateBuilding:
		case InputFunction.PlusMinus1:
		case InputFunction.PlusMinus10:
		case InputFunction.PlusMinus100:
		case InputFunction.MapSelectMineral:
		case InputFunction.MapSelectMarkerType:
		case InputFunction.MenuSwitchInventory:
			return true;
		default:
			return false;
		}
	}

	public bool IsPressed(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		return (byte)(0u | (IsKeyPressed(InputMappings[(int)inputFunction].PositiveKey, capture, inputFunction) ? 1u : 0u) | (IsKeyPressed(InputMappings[(int)inputFunction].AltPositiveKey, capture, inputFunction) ? 1u : 0u) | (IsXBoxButtonPressed(InputMappings[(int)inputFunction].PositiveXBoxButton, capture, inputFunction) ? 1u : 0u) | (IsPS4ButtonPressed(InputMappings[(int)inputFunction].PositivePS4Button, capture, inputFunction) ? 1u : 0u) | (IsSteamDeckButtonPressed(InputMappings[(int)inputFunction].PositiveSteamDeckButton, capture, inputFunction) ? 1u : 0u)) != 0;
	}

	public bool IsKeyPressed(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		return (byte)(0u | (IsKeyPressed(InputMappings[(int)inputFunction].PositiveKey, capture, inputFunction) ? 1u : 0u) | (IsKeyPressed(InputMappings[(int)inputFunction].AltPositiveKey, capture, inputFunction) ? 1u : 0u)) != 0;
	}

	public bool IsJustPressed(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		return (byte)(0u | (IsKeyJustPressed(InputMappings[(int)inputFunction].PositiveKey, capture, inputFunction) ? 1u : 0u) | (IsKeyJustPressed(InputMappings[(int)inputFunction].AltPositiveKey, capture, inputFunction) ? 1u : 0u) | (IsXBoxButtonJustPressed(InputMappings[(int)inputFunction].PositiveXBoxButton, capture, inputFunction) ? 1u : 0u) | (IsPS4ButtonJustPressed(InputMappings[(int)inputFunction].PositivePS4Button, capture, inputFunction) ? 1u : 0u) | (IsSteamDeckButtonJustPressed(InputMappings[(int)inputFunction].PositiveSteamDeckButton, capture, inputFunction) ? 1u : 0u)) != 0;
	}

	public bool IsJustReleased(InputFunction inputFunction)
	{
		return (byte)(0u | (IsKeyJustReleased(InputMappings[(int)inputFunction].PositiveKey) ? 1u : 0u) | (IsKeyJustReleased(InputMappings[(int)inputFunction].AltPositiveKey) ? 1u : 0u) | (IsXBoxButtonJustReleased(InputMappings[(int)inputFunction].PositiveXBoxButton) ? 1u : 0u) | (IsPS4ButtonJustReleased(InputMappings[(int)inputFunction].PositivePS4Button) ? 1u : 0u) | (IsSteamDeckButtonJustReleased(InputMappings[(int)inputFunction].PositiveSteamDeckButton) ? 1u : 0u)) != 0;
	}

	public bool IsCaptured(InputFunction inputFunction)
	{
		bool flag = false;
		switch (CurrentInputType)
		{
		case InputType.MouseAndKeyboard:
			flag |= IsKeyCaptured(InputMappings[(int)inputFunction].PositiveKey);
			flag |= IsMouseAxisCaptured(InputMappings[(int)inputFunction].MouseAxis);
			break;
		case InputType.XBox:
			flag |= IsXBoxButtonCaptured(InputMappings[(int)inputFunction].PositiveXBoxButton);
			flag |= IsXBoxAxisCaptured(InputMappings[(int)inputFunction].XBoxAxis);
			break;
		case InputType.PS4:
			flag |= IsPS4ButtonCaptured(InputMappings[(int)inputFunction].PositivePS4Button);
			flag |= IsPS4AxisCaptured(InputMappings[(int)inputFunction].PS4Axis);
			break;
		case InputType.SteamDeck:
			flag |= IsSteamDeckButtonCaptured(InputMappings[(int)inputFunction].PositiveSteamDeckButton);
			flag |= IsSteamDeckAxisCaptured(InputMappings[(int)inputFunction].SteamDeckAxis);
			break;
		}
		return flag;
	}

	public Vector2 GetVector2(InputFunction axis1, InputFunction axis2)
	{
		switch (CurrentInputType)
		{
		case InputType.XBox:
			if (InputMappings[(int)axis1].XBoxAxis != XBoxAxis.None && InputMappings[(int)axis2].XBoxAxis != XBoxAxis.None)
			{
				return ApplyJoystickDeadZone(new Vector2(GetXBoxAxisRaw(InputMappings[(int)axis1].XBoxAxis), GetXBoxAxisRaw(InputMappings[(int)axis2].XBoxAxis)));
			}
			break;
		case InputType.PS4:
			if (InputMappings[(int)axis1].PS4Axis != PS4Axis.None && InputMappings[(int)axis2].PS4Axis != PS4Axis.None)
			{
				return ApplyJoystickDeadZone(new Vector2(GetPS4AxisRaw(InputMappings[(int)axis1].PS4Axis), GetPS4AxisRaw(InputMappings[(int)axis2].PS4Axis)));
			}
			break;
		case InputType.SteamDeck:
			if (InputMappings[(int)axis1].SteamDeckAxis != SteamDeckAxis.None && InputMappings[(int)axis2].SteamDeckAxis != SteamDeckAxis.None)
			{
				return ApplyJoystickDeadZone(new Vector2(GetSteamDeckAxisRaw(InputMappings[(int)axis1].SteamDeckAxis), GetSteamDeckAxisRaw(InputMappings[(int)axis2].SteamDeckAxis)));
			}
			break;
		}
		return new Vector2(GetAxis(axis1), GetAxis(axis2));
	}

	public float GetAxis(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		float num = 0f;
		num += (IsKeyPressed(mappedInput.PositiveKey, capture, inputFunction) ? 1f : 0f);
		num -= (IsKeyPressed(mappedInput.NegativeKey, capture, inputFunction) ? 1f : 0f);
		num += (IsKeyPressed(mappedInput.AltPositiveKey, capture, inputFunction) ? 1f : 0f);
		num -= (IsKeyPressed(mappedInput.AltNegativeKey, capture, inputFunction) ? 1f : 0f);
		num += GetXBoxButtonPressedAmount(mappedInput.PositiveXBoxButton, capture, inputFunction);
		num -= GetXBoxButtonPressedAmount(mappedInput.NegativeXBoxButton, capture, inputFunction);
		num += GetXBoxAxis(mappedInput.XBoxAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
		num += GetPS4ButtonPressedAmount(mappedInput.PositivePS4Button, capture, inputFunction);
		num -= GetPS4ButtonPressedAmount(mappedInput.NegativePS4Button, capture, inputFunction);
		num += GetPS4Axis(mappedInput.PS4Axis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
		num += GetSteamDeckButtonPressedAmount(mappedInput.PositiveSteamDeckButton, capture, inputFunction);
		num -= GetSteamDeckButtonPressedAmount(mappedInput.NegativeSteamDeckButton, capture, inputFunction);
		num += GetSteamDeckAxis(mappedInput.SteamDeckAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
		float num2 = GetMouseAxis(mappedInput.MouseAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
		if (inputFunction == InputFunction.RotateCamera || inputFunction == InputFunction.PitchCamera)
		{
			if (MouseLookAcceleration)
			{
				num2 *= 1f / 60f / GameImpl.UnscaledDeltaTime;
				num2 = MathUtil.SquaredButKeepSign(num2);
				num2 *= GameImpl.UnscaledDeltaTime / (1f / 60f);
				num2 *= MouseLookAcceleratedScale * MouseLookSensitivity;
			}
			else
			{
				num2 *= MouseLookNonacceleratedScale * MouseLookSensitivity;
			}
			num *= GameImpl.UnscaledDeltaTime;
			num2 *= 1f / 60f;
		}
		return num + num2;
	}

	public float GetButtonAxis(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		return 0f + (IsKeyPressed(mappedInput.PositiveKey, capture, inputFunction) ? 1f : 0f) - (IsKeyPressed(mappedInput.NegativeKey, capture, inputFunction) ? 1f : 0f) + (IsKeyPressed(mappedInput.AltPositiveKey, capture, inputFunction) ? 1f : 0f) - (IsKeyPressed(mappedInput.AltNegativeKey, capture, inputFunction) ? 1f : 0f) + GetXBoxButtonPressedAmount(mappedInput.PositiveXBoxButton, capture, inputFunction) - GetXBoxButtonPressedAmount(mappedInput.NegativeXBoxButton, capture, inputFunction) + GetPS4ButtonPressedAmount(mappedInput.PositivePS4Button, capture, inputFunction) - GetPS4ButtonPressedAmount(mappedInput.NegativePS4Button, capture, inputFunction) + GetSteamDeckButtonPressedAmount(mappedInput.PositiveSteamDeckButton, capture, inputFunction) - GetSteamDeckButtonPressedAmount(mappedInput.NegativeSteamDeckButton, capture, inputFunction);
	}

	public float GetMouseAxis(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		float num = GetMouseAxis(mappedInput.MouseAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
		return 0f + num;
	}

	public float GetGamepadAxis(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		return 0f + GetXBoxAxis(mappedInput.XBoxAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f) + GetPS4Axis(mappedInput.PS4Axis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f) + GetSteamDeckAxis(mappedInput.SteamDeckAxis, capture, inputFunction) * (mappedInput.AxisInverted ? (-1f) : 1f);
	}

	public int GetIntAxis(InputFunction inputFunction, bool capture = true, int buttonPromptHash = 0)
	{
		float axis = GetAxis(inputFunction, capture, buttonPromptHash);
		axis = ((Math.Abs(axis) > 0f) ? Mathf.Sign(axis) : 0f);
		return (int)axis;
	}

	public void Capture(InputFunction inputFunction, bool untilReleased)
	{
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		Captured captureType = ((!untilReleased) ? Captured.ThisFrame : Captured.UntilReleased);
		CaptureKey(mappedInput.PositiveKey, captureType, inputFunction);
		CaptureKey(mappedInput.NegativeKey, captureType, inputFunction);
		CaptureKey(mappedInput.AltPositiveKey, captureType, inputFunction);
		CaptureKey(mappedInput.AltNegativeKey, captureType, inputFunction);
		CaptureMouseAxis(mappedInput.MouseAxis, captureType, inputFunction);
		CaptureXBoxButton(mappedInput.PositiveXBoxButton, captureType, inputFunction);
		CaptureXBoxButton(mappedInput.NegativeXBoxButton, captureType, inputFunction);
		CaptureXBoxAxis(mappedInput.XBoxAxis, captureType, inputFunction);
		CapturePS4Button(mappedInput.PositivePS4Button, captureType, inputFunction);
		CapturePS4Button(mappedInput.NegativePS4Button, captureType, inputFunction);
		CapturePS4Axis(mappedInput.PS4Axis, captureType, inputFunction);
		CaptureSteamDeckButton(mappedInput.PositiveSteamDeckButton, captureType, inputFunction);
		CaptureSteamDeckButton(mappedInput.NegativeSteamDeckButton, captureType, inputFunction);
		CaptureSteamDeckAxis(mappedInput.SteamDeckAxis, captureType, inputFunction);
	}

	public float GetJustPressedAxis(InputFunction inputFunction, int buttonPromptHash = 0)
	{
		if (buttonPromptHash != 0 && ShouldShowButtonPrompt(inputFunction))
		{
			ButtonPromptBarBehaviour.Instance.AddButtonPrompt(inputFunction, buttonPromptHash);
		}
		float axis = GetAxis(inputFunction);
		if (axis != 0f)
		{
			Capture(inputFunction, untilReleased: true);
		}
		return axis;
	}

	public bool IsMapped(InputFunction inputFunction)
	{
		MappedInput mappedInput = InputMappings[(int)inputFunction];
		switch (CurrentInputType)
		{
		case InputType.SteamDeck:
			if (mappedInput.PositiveSteamDeckButton == SteamDeckButton.None && mappedInput.NegativeSteamDeckButton == SteamDeckButton.None)
			{
				return mappedInput.SteamDeckAxis != SteamDeckAxis.None;
			}
			return true;
		case InputType.XBox:
			if (mappedInput.PositiveXBoxButton == XBoxButton.None && mappedInput.NegativeXBoxButton == XBoxButton.None)
			{
				return mappedInput.XBoxAxis != XBoxAxis.None;
			}
			return true;
		case InputType.PS4:
			if (mappedInput.PositivePS4Button == PS4Button.None && mappedInput.NegativePS4Button == PS4Button.None)
			{
				return mappedInput.PS4Axis != PS4Axis.None;
			}
			return true;
		case InputType.MouseAndKeyboard:
			if (mappedInput.PositiveKey == KeyCode.None && mappedInput.NegativeKey == KeyCode.None && mappedInput.AltPositiveKey == KeyCode.None && mappedInput.AltNegativeKey == KeyCode.None)
			{
				return mappedInput.MouseAxis != MouseAxis.None;
			}
			return true;
		default:
			return false;
		}
	}

	public bool IsMappedToSameInput(InputFunction a, InputFunction b)
	{
		return CurrentInputType switch
		{
			InputType.MouseAndKeyboard => InputMappings[(int)a].PositiveKey == InputMappings[(int)b].PositiveKey, 
			InputType.XBox => InputMappings[(int)a].PositiveXBoxButton == InputMappings[(int)b].PositiveXBoxButton, 
			InputType.PS4 => InputMappings[(int)a].PositivePS4Button == InputMappings[(int)b].PositivePS4Button, 
			InputType.SteamDeck => InputMappings[(int)a].PositiveSteamDeckButton == InputMappings[(int)b].PositiveSteamDeckButton, 
			_ => false, 
		};
	}

	public bool IsMappedToMouseWheel(InputFunction inputFunction)
	{
		if (CurrentInputType == InputType.MouseAndKeyboard)
		{
			return InputMappings[(int)inputFunction].MouseAxis == MouseAxis.ScrollWheel;
		}
		return false;
	}

	public bool IsMappedToMouseMovement(InputFunction inputFunction)
	{
		if (CurrentInputType == InputType.MouseAndKeyboard)
		{
			if (InputMappings[(int)inputFunction].MouseAxis != MouseAxis.Horiz && InputMappings[(int)inputFunction].MouseAxis != MouseAxis.Vert && InputMappings[(int)inputFunction].MouseAxis != MouseAxis.SmoothedHoriz)
			{
				return InputMappings[(int)inputFunction].MouseAxis == MouseAxis.SmoothedVert;
			}
			return true;
		}
		return false;
	}

	public Vector2 GetCursorPos()
	{
		return CursorPos;
	}

	public void SetCursorPos(Vector2 pos)
	{
		GameImpl instance = GameImpl.Instance;
		HudBehaviour instance2 = HudBehaviour.Instance;
		SetMousePosition(new Vector2((float)instance.LeftOnScreen + (instance2.Centre.x + pos.x) * instance.ScreenScale, (float)instance.TopOnScreen + (instance2.Centre.y + pos.y) * instance.ScreenScale));
	}

	public void SetMousePosition(Vector2 pos)
	{
		if (Mouse.current != null)
		{
			Mouse.current.WarpCursorPosition(pos);
		}
		MousePosition = pos;
	}

	public Vector2 GetMousePosition()
	{
		return MousePosition;
	}

	public bool IsMouseOverPanel(RectTransform rectTransform)
	{
		return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, MousePosition, null);
	}

	public static bool IsUIInputFieldActive()
	{
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && current.currentSelectedGameObject.GetComponent<InputField>() != null)
		{
			return true;
		}
		return false;
	}
}
