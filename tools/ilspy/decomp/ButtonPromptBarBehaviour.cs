using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPromptBarBehaviour : MonoBehaviour
{
	public struct ButtonPrompt
	{
		public InputFunction InputFunction;

		public float HoldTime;

		public string Text;
	}

	public static ButtonPromptBarBehaviour Instance;

	private List<ButtonPrompt> ButtonPrompts = new List<ButtonPrompt>();

	private RawImage UnityFill;

	private static StringBuilder sb = new StringBuilder();

	public static int PROMPT_Sprint = StringUtil.JenkinsHash("PROMPT_Sprint");

	public static int PROMPT_Reload = StringUtil.JenkinsHash("PROMPT_Reload");

	public static int PROMPT_Surrender = StringUtil.JenkinsHash("PROMPT_Surrender");

	public static int PROMPT_Vault = StringUtil.JenkinsHash("PROMPT_Vault");

	public static int PROMPT_Skip = StringUtil.JenkinsHash("PROMPT_Skip");

	public static int PROMPT_BrainScan = StringUtil.JenkinsHash("PROMPT_BrainScan");

	public static int PROMPT_PlaySpeed = StringUtil.JenkinsHash("PROMPT_PlaySpeed");

	public static int PROMPT_RotateBuilding = StringUtil.JenkinsHash("PROMPT_RotateBuilding");

	public static int PROMPT_RotateCamera = StringUtil.JenkinsHash("PROMPT_RotateCamera");

	public static int PROMPT_Zoom = StringUtil.JenkinsHash("PROMPT_Zoom");

	public static int PROMPT_ExitCommandMode = StringUtil.JenkinsHash("PROMPT_ExitCommandMode");

	public static int PROMPT_PlusMinus1 = StringUtil.JenkinsHash("PROMPT_PlusMinus1");

	public static int PROMPT_PlusMinus10 = StringUtil.JenkinsHash("PROMPT_PlusMinus10");

	public static int PROMPT_PlusMinus100 = StringUtil.JenkinsHash("PROMPT_PlusMinus100");

	public static int PROMPT_Clear = StringUtil.JenkinsHash("PROMPT_Clear");

	public static int PROMPT_GoToMapLocation = StringUtil.JenkinsHash("PROMPT_GoToMapLocation");

	public static int PROMPT_SetMapMarker = StringUtil.JenkinsHash("PROMPT_SetMapMarker");

	public static int PROMPT_MapSelectMineral = StringUtil.JenkinsHash("PROMPT_MapSelectMineral");

	public static int PROMPT_ClearMapMarker = StringUtil.JenkinsHash("PROMPT_ClearMapMarker");

	public static int PROMPT_SetCropsPatchHere = StringUtil.JenkinsHash("PROMPT_SetCropsPatchHere");

	public static int PROMPT_Finished = StringUtil.JenkinsHash("PROMPT_Finished");

	public static int PROMPT_Cancel = StringUtil.JenkinsHash("PROMPT_Cancel");

	public static int PROMPT_Inventory = StringUtil.JenkinsHash("PROMPT_Inventory");

	public static int PROMPT_DeselectAll = StringUtil.JenkinsHash("PROMPT_DeselectAll");

	public static int PROMPT_FindPlayer = StringUtil.JenkinsHash("PROMPT_FindPlayer");

	public static int PROMPT_ShowEquipmentSelector = StringUtil.JenkinsHash("PROMPT_ShowEquipmentSelector");

	public static int PROMPT_ShowInventoryActionMenu = StringUtil.JenkinsHash("PROMPT_ShowInventoryActionMenu");

	public static int PROMPT_HideInventoryActionMenu = StringUtil.JenkinsHash("PROMPT_HideInventoryActionMenu");

	public static int PROMPT_TradeReset = StringUtil.JenkinsHash("PROMPT_TradeReset");

	public static int PROMPT_TradeConfirm = StringUtil.JenkinsHash("PROMPT_TradeConfirm");

	public static int PROMPT_ShowKeyboard = StringUtil.JenkinsHash("PROMPT_ShowKeyboard");

	public static int PROMPT_Horn = StringUtil.JenkinsHash("PROMPT_Horn");

	public static int PROMPT_HandBrake = StringUtil.JenkinsHash("PROMPT_HandBrake");

	public static int PROMPT_Actions = StringUtil.JenkinsHash("PROMPT_Actions");

	public static int PROMPT_TransferAll = StringUtil.JenkinsHash("PROMPT_TransferAll");

	public static int PROMPT_SwitchInventory = StringUtil.JenkinsHash("PROMPT_SwitchInventory");

	public static int PROMPT_SetActiveQuest = StringUtil.JenkinsHash("PROMPT_SetActiveQuest");

	public static int PROMPT_ViewOnMap = StringUtil.JenkinsHash("PROMPT_ViewOnMap");

	public void Awake()
	{
		Instance = this;
		UnityFill = base.transform.GetChild(0).GetComponent<RawImage>();
		for (int num = base.gameObject.transform.childCount - 1; num >= 1; num--)
		{
			Object.Destroy(base.gameObject.transform.GetChild(num).gameObject);
		}
	}

	public void UpdateButtonPrompts()
	{
		base.gameObject.SetActive(ButtonPrompts.Count > 0);
		if (ButtonPrompts.Count > 0)
		{
			GameImpl instance = GameImpl.Instance;
			HudBehaviour instance2 = HudBehaviour.Instance;
			RectTransform rectTransform = (RectTransform)base.transform;
			rectTransform.anchoredPosition = new Vector2((instance.IsMenuOpen() || instance.IsDialogOpen()) ? 0f : ((GameImpl.Instance.Settings.SidebarLayoutEnabled || InfoScreen.Instance.Active) ? ((0f - instance2.SidebarRectTransform.rect.width) * 0.5f) : ((0f - (instance2.SidebarRectTransform.rect.width - HudBehaviour.GetMinimapSize())) * 0.5f)), InfoScreen.Instance.Active ? 48f : 64f);
			float num = 0f;
			for (int i = 0; i < ButtonPrompts.Count; i++)
			{
				if (ButtonPrompts[i].HoldTime > 0f)
				{
					num = ButtonPrompts[i].HoldTime;
					break;
				}
			}
			int num2 = 1;
			for (int j = 0; j < ButtonPrompts.Count; j++)
			{
				if (num > 0f && ButtonPrompts[j].HoldTime == 0f)
				{
					continue;
				}
				sb.Length = 0;
				sb.AppendButtonPromptString(ButtonPrompts[j].InputFunction);
				if (sb.Length > 0)
				{
					sb.Append(ButtonPrompts[j].Text);
					if (num2 >= base.transform.childCount)
					{
						Object.Instantiate(Hud.ButtonPrompt.GetAsset(), base.transform, worldPositionStays: false);
					}
					base.transform.GetChild(num2).GetComponent<TextMeshProUGUI>().SetUnityTextIfDifferent(sb);
					num2++;
				}
			}
			while (num2 < base.transform.childCount)
			{
				Object.Destroy(base.transform.GetChild(num2++).gameObject);
			}
			UnityFill.gameObject.SetActive(num > 0f);
			if (num > 0f)
			{
				UnityFill.rectTransform.sizeDelta = new Vector2(num * rectTransform.sizeDelta.x, UnityFill.rectTransform.sizeDelta.y);
			}
		}
		ButtonPrompts.Clear();
	}

	public void AddButtonPrompt(InputFunction inputFunction, int hash)
	{
		ButtonPrompt item = new ButtonPrompt
		{
			InputFunction = inputFunction,
			Text = GameImpl.Translate(hash)
		};
		ButtonPrompts.Add(item);
	}

	public void AddButtonPrompt(InputFunction inputFunction, int hash, float holdTime)
	{
		ButtonPrompt item = new ButtonPrompt
		{
			InputFunction = inputFunction,
			HoldTime = holdTime,
			Text = GameImpl.Translate(hash)
		};
		ButtonPrompts.Add(item);
	}

	public void AddButtonPrompt(InputFunction inputFunction, string str)
	{
		ButtonPrompt item = new ButtonPrompt
		{
			InputFunction = inputFunction,
			Text = str
		};
		ButtonPrompts.Add(item);
	}
}
