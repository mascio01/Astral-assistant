using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentTransferAmountBox : BaseDialog
{
	public enum Mode
	{
		Transferring,
		Trading,
		Destroying
	}

	public static int HUD_Transferring = StringUtil.JenkinsHash("HUD_Transferring");

	public static int HUD_Destroying = StringUtil.JenkinsHash("HUD_Destroying");

	private TextMeshProUGUI UnityMessage;

	private RawImage UnityIcon;

	private TMP_InputField UnityInputField;

	private Slider UnitySlider;

	public TileObject Carrier;

	public TileObject To;

	public Equipment Transferring;

	public int Amount;

	public int MaxTransferrable;

	public int MinTransferrable = 1;

	public Mode CurrentMode;

	private float PressedTime;

	public override void OnActivate()
	{
		base.OnActivate();
		UnityMessage = base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>();
		UnityIcon = base.gameObject.FindChild("EquipmentIcon").GetComponent<RawImage>();
		UnityInputField = base.gameObject.FindChild("InputField").GetComponent<TMP_InputField>();
		UnitySlider = base.gameObject.FindChild("Slider").GetComponent<Slider>();
		if (CurrentMode == Mode.Destroying)
		{
			UnityMessage.SetUnityText(GameImpl.Translate(HUD_Destroying) + " " + Transferring.GetDisplayNameString());
		}
		else
		{
			UnityMessage.SetUnityText(GameImpl.Translate(HUD_Transferring) + " " + Transferring.GetDisplayNameString());
		}
		UnityIcon.texture = (Texture2D)Transferring.GetPrototype().Tex;
		UnitySlider.minValue = MinTransferrable;
		UnitySlider.maxValue = MaxTransferrable;
		UnitySlider.onValueChanged.RemoveAllListeners();
		UnitySlider.value = Amount;
		UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
		UnityInputField.onValueChanged.RemoveAllListeners();
		UnityInputField.SetUnityText(Amount.ToString());
		UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
		UnityInputField.onEndEdit.AddListener(OnInputValueSubmitted);
	}

	public void OnSliderValueChanged(float v)
	{
		Amount = (int)v;
		UnityInputField.onValueChanged.RemoveAllListeners();
		UnityInputField.SetUnityText(Amount.ToString());
		UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
	}

	public void OnInputValueSubmitted(string v)
	{
		bool flag = true;
		try
		{
			Amount = int.Parse(v);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (Amount < MinTransferrable || Amount > MaxTransferrable)
		{
			Amount = MathUtil.Clamp(Amount, MinTransferrable, MaxTransferrable);
			flag = false;
		}
		if (!flag)
		{
			UnityInputField.onValueChanged.RemoveAllListeners();
			UnityInputField.SetUnityText(Amount.ToString());
			UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
		}
		UnitySlider.onValueChanged.RemoveAllListeners();
		UnitySlider.value = Amount;
		UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
	}

	public void OnInputValueChanged(string v)
	{
		bool flag = true;
		try
		{
			Amount = int.Parse(v);
		}
		catch (Exception)
		{
			flag = false;
		}
		if (Amount < MinTransferrable || Amount > MaxTransferrable)
		{
			Amount = MathUtil.Clamp(Amount, MinTransferrable, MaxTransferrable);
			flag = true;
		}
		if (flag)
		{
			UnitySlider.onValueChanged.RemoveAllListeners();
			UnitySlider.value = Amount;
			UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
		}
	}

	public override void DialogUpdate()
	{
		UnityInputField.interactable = SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		UnitySlider.interactable = SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor;
		base.DialogUpdate();
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		InputFunctionManager instance = InputFunctionManager.Instance;
		int num = 0;
		num += instance.GetIntAxis(InputFunction.MoveHoriz);
		num += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
		num += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
		num += instance.GetIntAxis(InputFunction.PlusMinus100, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus100) * 100;
		if (num != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
		{
			PressedTime = Time.realtimeSinceStartup;
			Amount = MathUtil.Clamp(Amount + num, MinTransferrable, MaxTransferrable);
			UnityInputField.onValueChanged.RemoveAllListeners();
			UnityInputField.SetUnityText(Amount.ToString());
			UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
			UnitySlider.onValueChanged.RemoveAllListeners();
			UnitySlider.value = Amount;
			UnitySlider.onValueChanged.AddListener(OnSliderValueChanged);
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (!OKSelected)
		{
			return;
		}
		if (inputFrame != null && Amount > 0)
		{
			switch (CurrentMode)
			{
			case Mode.Trading:
			{
				TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
				if (tradePage != null)
				{
					tradePage.AddPendingTrade(Transferring, Carrier == tradePage.Taker, Amount);
				}
				break;
			}
			case Mode.Transferring:
				inputFrame.AddAction(InputAction.EquipmentTransfer(Transferring, Carrier, To, Amount));
				break;
			case Mode.Destroying:
				inputFrame.AddAction(InputAction.EquipmentDestroy(Transferring, Carrier, To as Character, Amount));
				break;
			}
		}
		OKSelected = false;
		Finished = true;
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
