using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftLimitBox : BaseDialog
{
	private TextMeshProUGUI UnityMessage;

	private TextMeshProUGUI UnityFlOz;

	private TextMeshProUGUI UnityLiters;

	private RawImage UnityIcon;

	private TMP_InputField UnityInputField;

	private Toggle UnityInfiniteToggle;

	public Community Community;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public int RecurringLimit = 1;

	public float RecurringLiquidLimit;

	private float PressedTime;

	public override void OnActivate()
	{
		base.OnActivate();
		UnityMessage = base.gameObject.FindChild("MessageText").GetComponent<TextMeshProUGUI>();
		UnityFlOz = base.gameObject.FindChild("FlOz").GetComponent<TextMeshProUGUI>();
		UnityLiters = base.gameObject.FindChild("Liters").GetComponent<TextMeshProUGUI>();
		UnityIcon = base.gameObject.FindChild("EquipmentIcon").GetComponent<RawImage>();
		UnityInputField = base.gameObject.FindChild("InputField").GetComponent<TMP_InputField>();
		UnityInfiniteToggle = base.gameObject.FindChild("InfiniteToggle").GetComponent<Toggle>();
		if (Proto != null)
		{
			UnityMessage.SetUnityText(GameImpl.Translate(Proto.NameHash));
			UnityIcon.texture = (Texture2D)Proto.Tex;
			UnityIcon.color = Color.white;
		}
		else if (Liquid != null)
		{
			UnityMessage.SetUnityText(GameImpl.Translate(Liquid.NameHash));
			UnityIcon.texture = (Texture2D)Liquid.Tex;
			UnityIcon.color = Liquid.Col;
		}
		RecurringLimit = 1;
		UnityInfiniteToggle.isOn = false;
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		if (Proto != null && Community != null)
		{
			int craftingLimit = Community.GetCraftingLimit(Proto);
			if (craftingLimit == int.MaxValue)
			{
				UnityInfiniteToggle.isOn = true;
				RecurringLimit = Community.CountInventoryItemsOfType(Proto);
			}
			else
			{
				RecurringLimit = craftingLimit;
			}
		}
		if (Liquid != null && Community != null)
		{
			int craftingLimit2 = Community.GetCraftingLimit(Liquid);
			if (craftingLimit2 == int.MaxValue)
			{
				UnityInfiniteToggle.isOn = true;
				RecurringLiquidLimit = Community.GetTotalLiquid(Liquid) * (useMetricWeights ? 0.0295735f : 1f);
			}
			else
			{
				RecurringLiquidLimit = (float)craftingLimit2 * (useMetricWeights ? 0.0295735f : 1f);
			}
		}
		UnityFlOz.gameObject.SetActive(Liquid != null && !useMetricWeights);
		UnityLiters.gameObject.SetActive(Liquid != null && useMetricWeights);
		UnityInputField.onValueChanged.RemoveAllListeners();
		UnityInputField.SetUnityText((Liquid != null) ? RecurringLiquidLimit.ToString(AvailableAction.SensibleFloatFormat) : RecurringLimit.ToString());
		UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
	}

	public override void DialogUpdate()
	{
		base.DialogUpdate();
		bool isOn = UnityInfiniteToggle.isOn;
		UnityInputField.interactable = SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Cursor && !isOn;
		UnityInputField.GetComponent<RawImage>().color = (isOn ? Color.gray : Color.white);
	}

	public void OnInputValueChanged(string v)
	{
		try
		{
			if (Liquid != null)
			{
				RecurringLiquidLimit = StringUtil.ParseFloat(v);
			}
			else
			{
				RecurringLimit = StringUtil.ParseInt(v);
			}
		}
		catch (Exception)
		{
		}
	}

	public void OnInfiniteToggled()
	{
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (UnityInfiniteToggle.isOn)
		{
			return;
		}
		int num = 0;
		num += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
		num += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
		num += instance.GetIntAxis(InputFunction.PlusMinus100, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus100) * 100;
		if (num != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
		{
			PressedTime = Time.realtimeSinceStartup;
			if (Liquid != null)
			{
				RecurringLiquidLimit = Math.Max(RecurringLiquidLimit + (float)num, 0f);
			}
			else
			{
				RecurringLimit = Math.Max(RecurringLimit + num, 1);
			}
			UnityInputField.onValueChanged.RemoveAllListeners();
			UnityInputField.SetUnityText((Liquid != null) ? RecurringLiquidLimit.ToString(AvailableAction.SensibleFloatFormat) : RecurringLimit.ToString());
			UnityInputField.onValueChanged.AddListener(OnInputValueChanged);
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (!OKSelected)
		{
			return;
		}
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		int num = Mathf.RoundToInt(RecurringLiquidLimit / (useMetricWeights ? 0.0295735f : 1f));
		if (Session.Instance.Editor)
		{
			if (Proto != null)
			{
				Community.SetCraftingLimit(Proto, UnityInfiniteToggle.isOn ? int.MaxValue : RecurringLimit);
			}
			if (Liquid != null)
			{
				Community.SetCraftingLimit(Liquid, UnityInfiniteToggle.isOn ? int.MaxValue : num);
			}
			OKSelected = false;
			Finished = true;
		}
		else if (inputFrame != null)
		{
			if (Proto != null)
			{
				inputFrame.AddAction(InputAction.SetCraftItemLimit(Community, Proto, UnityInfiniteToggle.isOn ? int.MaxValue : RecurringLimit));
			}
			if (Liquid != null)
			{
				inputFrame.AddAction(InputAction.SetCraftLiquidLimit(Community, Liquid, UnityInfiniteToggle.isOn ? int.MaxValue : num));
			}
			OKSelected = false;
			Finished = true;
		}
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
