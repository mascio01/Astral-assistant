using UnityEngine.EventSystems;

public class CustomInputBehaviour : BaseInput
{
	public static string MoveHoriz = "MoveHoriz";

	public static string MoveVert = "MoveVert";

	public static string Select = "Select";

	public static string Back = "Back";

	public static CustomInputBehaviour Instance;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	public override float GetAxisRaw(string axisName)
	{
		if (InputFunctionManager.Instance != null)
		{
			if (axisName == MoveHoriz)
			{
				float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MoveHoriz, capture: false);
				if (axis != 0f)
				{
					SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
				}
				return axis;
			}
			if (axisName == MoveVert)
			{
				float axis2 = InputFunctionManager.Instance.GetAxis(InputFunction.MoveVert, capture: false);
				if (axis2 != 0f)
				{
					SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
				}
				return axis2;
			}
		}
		return base.GetAxisRaw(axisName);
	}

	public override bool GetButtonDown(string buttonName)
	{
		if (InputFunctionManager.Instance != null)
		{
			if (buttonName == Select)
			{
				bool num = InputFunctionManager.Instance.IsJustPressed(InputFunction.MenuSelect, capture: false);
				if (num)
				{
					SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
				}
				return num;
			}
			if (buttonName == Back)
			{
				bool num2 = InputFunctionManager.Instance.IsJustPressed(InputFunction.Back, capture: false);
				if (num2)
				{
					SelectableBehaviour.CurSelectionMode = SelectableBehaviour.SelectionMode.Buttons;
				}
				return num2;
			}
		}
		return base.GetButtonDown(buttonName);
	}
}
