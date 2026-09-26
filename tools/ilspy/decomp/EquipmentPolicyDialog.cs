using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentPolicyDialog : BaseDialog
{
	public int CommunityPolicy;

	public float CommunityTargetAmount;

	public int CharacterPolicy;

	public float CharacterTargetAmount;

	public bool CharacterSameAsCommunity;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public InfectionType InfectionType;

	public Character Character;

	private RawImage UnityIcon;

	private RawImage UnityInfectionIcon;

	private TextMeshProUGUI UnityTitle;

	private TextMeshProUGUI UnityCommunityTitle;

	private TextMeshProUGUI UnityCharacterTitle;

	private TextMeshProUGUI UnityCommunityCanUseText;

	private TextMeshProUGUI UnityCommunityCanCraftWithText;

	private TextMeshProUGUI UnityCommunityCanPlantText;

	private TextMeshProUGUI UnityCommunityCanShareText;

	private TextMeshProUGUI UnityCommunityCanFeedToAnimalsText;

	private TextMeshProUGUI UnityCommunityCanStripText;

	private TextMeshProUGUI UnityCommunityAutoCollectText;

	private TextMeshProUGUI UnityCommunityAutoDepositText;

	private TextMeshProUGUI UnityCommunityTargetAmountText;

	private TextMeshProUGUI UnityCharacterCanUseText;

	private TextMeshProUGUI UnityCharacterCanCraftWithText;

	private TextMeshProUGUI UnityCharacterCanPlantText;

	private TextMeshProUGUI UnityCharacterCanShareText;

	private TextMeshProUGUI UnityCharacterCanFeedToAnimalsText;

	private TextMeshProUGUI UnityCharacterCanStripText;

	private TextMeshProUGUI UnityCharacterAutoCollectText;

	private TextMeshProUGUI UnityCharacterAutoDepositText;

	private TextMeshProUGUI UnityCharacterTargetAmountText;

	private Toggle UnityCommunityCanUseToggle;

	private Toggle UnityCommunityCanCraftWithToggle;

	private Toggle UnityCommunityCanPlantToggle;

	private Toggle UnityCommunityCanShareToggle;

	private Toggle UnityCommunityCanFeedToAnimalsToggle;

	private Toggle UnityCommunityCanStripToggle;

	private Toggle UnityCommunityAutoCollectToggle;

	private Toggle UnityCommunityAutoDepositToggle;

	private TMP_InputField UnityCommunityTargetAmountField;

	private Toggle UnityCharacterCanUseToggle;

	private Toggle UnityCharacterCanCraftWithToggle;

	private Toggle UnityCharacterCanPlantToggle;

	private Toggle UnityCharacterCanShareToggle;

	private Toggle UnityCharacterCanFeedToAnimalsToggle;

	private Toggle UnityCharacterCanStripToggle;

	private Toggle UnityCharacterAutoCollectToggle;

	private Toggle UnityCharacterAutoDepositToggle;

	private TMP_InputField UnityCharacterTargetAmountField;

	private Toggle UnityCharacterSameAsCommunityToggle;

	private TextMeshProUGUI UnityCharacterSameAsCommunityText;

	private float PressedTime;

	private bool SettingPolicy;

	public override void OnActivate()
	{
		base.OnActivate();
		UnityIcon = base.gameObject.FindChild("Header/Icon").GetComponent<RawImage>();
		UnityInfectionIcon = base.gameObject.FindChild("Header/Icon/Infection").GetComponent<RawImage>();
		UnityTitle = base.gameObject.FindChild("Header/Title").GetComponent<TextMeshProUGUI>();
		UnityCommunityTitle = base.gameObject.FindChild("Middle/CommunityPanel/Title").GetComponent<TextMeshProUGUI>();
		UnityCharacterTitle = base.gameObject.FindChild("Middle/CharacterPanel/Title").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanUseText = base.gameObject.FindChild("Middle/CommunityPanel/CanUseToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanCraftWithText = base.gameObject.FindChild("Middle/CommunityPanel/CanCraftWithToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanPlantText = base.gameObject.FindChild("Middle/CommunityPanel/CanPlantToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanShareText = base.gameObject.FindChild("Middle/CommunityPanel/CanShareToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanFeedToAnimalsText = base.gameObject.FindChild("Middle/CommunityPanel/CanFeedToAnimalsToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanStripText = base.gameObject.FindChild("Middle/CommunityPanel/CanStripToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityAutoCollectText = base.gameObject.FindChild("Middle/CommunityPanel/AutoCollectToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityAutoDepositText = base.gameObject.FindChild("Middle/CommunityPanel/AutoDepositToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityTargetAmountText = base.gameObject.FindChild("Middle/CommunityPanel/TargetAmountField/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanUseText = base.gameObject.FindChild("Middle/CharacterPanel/CanUseToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanCraftWithText = base.gameObject.FindChild("Middle/CharacterPanel/CanCraftWithToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanPlantText = base.gameObject.FindChild("Middle/CharacterPanel/CanPlantToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanShareText = base.gameObject.FindChild("Middle/CharacterPanel/CanShareToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanFeedToAnimalsText = base.gameObject.FindChild("Middle/CharacterPanel/CanFeedToAnimalsToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterCanStripText = base.gameObject.FindChild("Middle/CharacterPanel/CanStripToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterAutoCollectText = base.gameObject.FindChild("Middle/CharacterPanel/AutoCollectToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterAutoDepositText = base.gameObject.FindChild("Middle/CharacterPanel/AutoDepositToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityCharacterTargetAmountText = base.gameObject.FindChild("Middle/CharacterPanel/TargetAmountField/Label").GetComponent<TextMeshProUGUI>();
		UnityCommunityCanUseToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanUseToggle").GetComponent<Toggle>();
		UnityCommunityCanCraftWithToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanCraftWithToggle").GetComponent<Toggle>();
		UnityCommunityCanPlantToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanPlantToggle").GetComponent<Toggle>();
		UnityCommunityCanShareToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanShareToggle").GetComponent<Toggle>();
		UnityCommunityCanFeedToAnimalsToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanFeedToAnimalsToggle").GetComponent<Toggle>();
		UnityCommunityCanStripToggle = base.gameObject.FindChild("Middle/CommunityPanel/CanStripToggle").GetComponent<Toggle>();
		UnityCommunityAutoCollectToggle = base.gameObject.FindChild("Middle/CommunityPanel/AutoCollectToggle").GetComponent<Toggle>();
		UnityCommunityAutoDepositToggle = base.gameObject.FindChild("Middle/CommunityPanel/AutoDepositToggle").GetComponent<Toggle>();
		UnityCommunityTargetAmountField = base.gameObject.FindChild("Middle/CommunityPanel/TargetAmountField/InputField").GetComponent<TMP_InputField>();
		UnityCharacterCanUseToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanUseToggle").GetComponent<Toggle>();
		UnityCharacterCanCraftWithToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanCraftWithToggle").GetComponent<Toggle>();
		UnityCharacterCanPlantToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanPlantToggle").GetComponent<Toggle>();
		UnityCharacterCanShareToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanShareToggle").GetComponent<Toggle>();
		UnityCharacterCanFeedToAnimalsToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanFeedToAnimalsToggle").GetComponent<Toggle>();
		UnityCharacterCanStripToggle = base.gameObject.FindChild("Middle/CharacterPanel/CanStripToggle").GetComponent<Toggle>();
		UnityCharacterAutoCollectToggle = base.gameObject.FindChild("Middle/CharacterPanel/AutoCollectToggle").GetComponent<Toggle>();
		UnityCharacterAutoDepositToggle = base.gameObject.FindChild("Middle/CharacterPanel/AutoDepositToggle").GetComponent<Toggle>();
		UnityCharacterTargetAmountField = base.gameObject.FindChild("Middle/CharacterPanel/TargetAmountField/InputField").GetComponent<TMP_InputField>();
		UnityCharacterSameAsCommunityToggle = base.gameObject.FindChild("Middle/CharacterPanel/SameToggle").GetComponent<Toggle>();
		UnityCharacterSameAsCommunityText = base.gameObject.FindChild("Middle/CharacterPanel/SameToggle/Label").GetComponent<TextMeshProUGUI>();
		UnityIcon.texture = ((Liquid == null) ? ((Proto != null && Proto.Tex != null) ? Proto.Tex.GetAsset() : null) : ((Liquid.Tex != null) ? Liquid.Tex.GetAsset() : null));
		UnityIcon.color = ((Liquid != null) ? ((Color)Liquid.Col) : Color.white);
		UnityInfectionIcon.gameObject.SetActive(InfectionType != InfectionType.None);
		UnityInfectionIcon.color = GameTerrain.MinimapSettings.GetInfectionCol(InfectionType);
		UnityTitle.SetUnityText((Liquid != null) ? GameImpl.Translate(Liquid.NameHash) : GameImpl.Translate(Proto.NameHash));
		int num = Hud.Instance.SelectedCharacters.Count;
		if (Hud.Instance.SelectedCharacters.Contains(Character))
		{
			num--;
		}
		UnityCommunityTitle.SetUnityText(GameImpl.Translate("HUD_EntireCommunity"));
		UnityCharacterTitle.SetUnityText((num > 0) ? GameImpl.Translate((num == 1) ? "HUD_SetPolicyForAllSelectedSing" : "HUD_SetPolicyForAllSelected").Replace("%1", Character.GetDisplayNameString()).Replace("%2", num.ToString()) : (Character.GetDisplayNameString() + ":"));
		UnityCharacterSameAsCommunityText.SetUnityText(GameImpl.Translate("HUD_SameAsCommunity"));
		bool flag = ((Liquid != null) ? Liquid.Edible : (Proto.GetNutrition() > 0f));
		bool flag2 = Liquid != null && Liquid.Drinkable;
		bool flag3 = Proto != null && Proto.ContainsHumanMeat && InfectionType != InfectionType.None;
		bool flag4 = Proto != null && GameCursor.CanAddMaterialToFire(Proto);
		bool num2 = Proto != null && EquipmentPrototype.AllAmmoTypes.Contains(Proto);
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		Language language = GameImpl.Instance.Settings.Language;
		GenderType gender = ((Liquid != null) ? Liquid.GetGenderInLanguage(language) : ((Proto != null) ? Proto.GetGenderInLanguage(language) : GenderType.Count));
		bool plural = Liquid == null && Proto != null && Proto.UsePluralIndefiniteArticle;
		UnityCommunityCanUseText.SetUnityText(UnityCharacterCanUseText.SetUnityText(GameImpl.Translate((flag && !flag3) ? "HUD_CanEat" : (flag2 ? "HUD_CanDrink" : (flag4 ? "HUD_CanAddToFire" : "HUD_CanUse")), gender, plural)));
		UnityCommunityCanCraftWithText.SetUnityText(UnityCharacterCanCraftWithText.SetUnityText(GameImpl.Translate((flag && !flag3) ? "HUD_CanCookWith" : "HUD_CanCraftWith", gender, plural)));
		UnityCommunityCanPlantText.SetUnityText(UnityCharacterCanPlantText.SetUnityText(GameImpl.Translate("HUD_CanPlant", gender, plural)));
		UnityCommunityCanShareText.SetUnityText(UnityCharacterCanShareText.SetUnityText(GameImpl.Translate("HUD_CanShare", gender, plural)));
		UnityCommunityCanFeedToAnimalsText.SetUnityText(UnityCharacterCanFeedToAnimalsText.SetUnityText(GameImpl.Translate("HUD_CanFeedToAnimals", gender, plural)));
		UnityCommunityCanStripText.SetUnityText(UnityCharacterCanStripText.SetUnityText(GameImpl.Translate("HUD_CanStrip", gender, plural)));
		UnityCommunityAutoCollectText.SetUnityText(UnityCharacterAutoCollectText.SetUnityText(GameImpl.Translate("HUD_AutoCollect")));
		UnityCommunityAutoDepositText.SetUnityText(UnityCharacterAutoDepositText.SetUnityText(GameImpl.Translate("HUD_AutoDeposit")));
		string text = GameImpl.Translate("HUD_TargetAmount") + ((Liquid != null) ? (" (" + GameImpl.Translate(useMetricWeights ? EquipmentTotalBehaviour.HUD_Liter : EquipmentTotalBehaviour.HUD_FlOz) + ")") : "");
		UnityCharacterTargetAmountText.SetUnityText(text);
		if (num2)
		{
			text = text + " " + ((Proto.TypeName == BaseObjectType.Arrow) ? GameImpl.Translate("HUD_IfHasBow") : GameImpl.Translate("HUD_IfHasGun"));
		}
		UnityCommunityTargetAmountText.SetUnityText(text);
		TMP_InputField unityCommunityTargetAmountField = UnityCommunityTargetAmountField;
		TMP_InputField.ContentType contentType = (UnityCharacterTargetAmountField.contentType = ((Liquid != null) ? TMP_InputField.ContentType.DecimalNumber : TMP_InputField.ContentType.IntegerNumber));
		unityCommunityTargetAmountField.contentType = contentType;
		WantOkCancelButtonPromptsForController = false;
		Populate();
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (UnityEventSystem.currentSelectedGameObject == UnityCommunityTargetAmountField.gameObject)
		{
			int num = 0;
			num += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
			num += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
			num += instance.GetIntAxis(InputFunction.PlusMinus100, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus100) * 100;
			if (num != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
			{
				PressedTime = Time.realtimeSinceStartup;
				CommunityTargetAmount = Math.Max(CommunityTargetAmount + (float)num, 0f);
				Populate();
			}
		}
		if (UnityEventSystem.currentSelectedGameObject == UnityCharacterTargetAmountField.gameObject)
		{
			int num2 = 0;
			num2 += instance.GetIntAxis(InputFunction.PlusMinus1, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus1);
			num2 += instance.GetIntAxis(InputFunction.PlusMinus10, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus10) * 10;
			num2 += instance.GetIntAxis(InputFunction.PlusMinus100, capture: true, ButtonPromptBarBehaviour.PROMPT_PlusMinus100) * 100;
			if (num2 != 0 && Time.realtimeSinceStartup - PressedTime > 0.2f)
			{
				PressedTime = Time.realtimeSinceStartup;
				CharacterTargetAmount = Math.Max(CharacterTargetAmount + (float)num2, 0f);
				Populate();
			}
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (OKSelected)
		{
			inputFrame?.AddAction(InputAction.SetEquipmentPolicy(Character, CharacterPolicy, CharacterTargetAmount, CommunityPolicy, CommunityTargetAmount, CharacterSameAsCommunity, Proto, Liquid, InfectionType));
			OKSelected = false;
			Finished = true;
		}
	}

	private void SetPolicy(EquipmentPolicyAction action, bool allowed, bool forCommunity)
	{
		if (forCommunity)
		{
			if (allowed)
			{
				CommunityPolicy |= 1 << (int)action;
			}
			else
			{
				CommunityPolicy &= ~(1 << (int)action);
			}
		}
		else if (allowed)
		{
			CharacterPolicy |= 1 << (int)action;
		}
		else
		{
			CharacterPolicy &= ~(1 << (int)action);
		}
		Populate();
	}

	private void Populate()
	{
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		SettingPolicy = true;
		UnityCommunityCanUseToggle.isOn = (CommunityPolicy & 1) != 0;
		UnityCommunityCanCraftWithToggle.isOn = (CommunityPolicy & 2) != 0;
		UnityCommunityCanPlantToggle.isOn = (CommunityPolicy & 4) != 0;
		UnityCommunityCanShareToggle.isOn = (CommunityPolicy & 8) != 0;
		UnityCommunityCanFeedToAnimalsToggle.isOn = (CommunityPolicy & 0x20) != 0;
		UnityCommunityCanStripToggle.isOn = (CommunityPolicy & 0x80) != 0;
		UnityCommunityAutoCollectToggle.isOn = (CommunityPolicy & 0x10) != 0;
		UnityCommunityAutoDepositToggle.isOn = (CommunityPolicy & 0x40) != 0;
		UnityCommunityTargetAmountField.SetUnityText((CommunityTargetAmount * ((useMetricWeights && Liquid != null) ? 0.0295735f : 1f)).ToString((Liquid != null) ? AvailableAction.SensibleFloatFormat : ""));
		UnityCharacterCanUseToggle.isOn = (CharacterPolicy & 1) != 0;
		UnityCharacterCanCraftWithToggle.isOn = (CharacterPolicy & 2) != 0;
		UnityCharacterCanPlantToggle.isOn = (CharacterPolicy & 4) != 0;
		UnityCharacterCanShareToggle.isOn = (CharacterPolicy & 8) != 0;
		UnityCharacterCanFeedToAnimalsToggle.isOn = (CharacterPolicy & 0x20) != 0;
		UnityCharacterCanStripToggle.isOn = (CharacterPolicy & 0x80) != 0;
		UnityCharacterAutoCollectToggle.isOn = (CharacterPolicy & 0x10) != 0;
		UnityCharacterAutoDepositToggle.isOn = (CharacterPolicy & 0x40) != 0;
		UnityCharacterTargetAmountField.SetUnityText((CharacterTargetAmount * ((useMetricWeights && Liquid != null) ? 0.0295735f : 1f)).ToString((Liquid != null) ? AvailableAction.SensibleFloatFormat : ""));
		UnityCharacterCanUseToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterCanCraftWithToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterCanPlantToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterCanShareToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterCanFeedToAnimalsToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterCanStripToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterAutoCollectToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterAutoDepositToggle.interactable = !CharacterSameAsCommunity;
		UnityCharacterTargetAmountField.interactable = !CharacterSameAsCommunity;
		UnityCharacterSameAsCommunityToggle.isOn = CharacterSameAsCommunity;
		bool flag = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanUse, Proto, Liquid);
		UnityCommunityCanUseToggle.gameObject.SetActive(flag);
		UnityCharacterCanUseToggle.gameObject.SetActive(flag && !CharacterSameAsCommunity);
		bool flag2 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanCraftWith, Proto, Liquid);
		UnityCommunityCanCraftWithToggle.gameObject.SetActive(flag2);
		UnityCharacterCanCraftWithToggle.gameObject.SetActive(flag2 && !CharacterSameAsCommunity);
		bool flag3 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanPlant, Proto, Liquid);
		UnityCommunityCanPlantToggle.gameObject.SetActive(flag3);
		UnityCharacterCanPlantToggle.gameObject.SetActive(flag3 && !CharacterSameAsCommunity);
		bool flag4 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanShare, Proto, Liquid);
		UnityCommunityCanShareToggle.gameObject.SetActive(flag4);
		UnityCharacterCanShareToggle.gameObject.SetActive(flag4 && !CharacterSameAsCommunity);
		bool flag5 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanFeedToAnimals, Proto, Liquid);
		UnityCommunityCanFeedToAnimalsToggle.gameObject.SetActive(flag5);
		UnityCharacterCanFeedToAnimalsToggle.gameObject.SetActive(flag5 && !CharacterSameAsCommunity);
		bool flag6 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.CanStrip, Proto, Liquid);
		UnityCommunityCanStripToggle.gameObject.SetActive(flag6);
		UnityCharacterCanStripToggle.gameObject.SetActive(flag6 && !CharacterSameAsCommunity);
		bool flag7 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.AutoCollect, Proto, Liquid);
		UnityCommunityAutoCollectToggle.gameObject.SetActive(flag7);
		UnityCharacterAutoCollectToggle.gameObject.SetActive(flag7 && !CharacterSameAsCommunity);
		bool flag8 = EquipmentPolicy.IsActionPossibleForItem(EquipmentPolicyAction.AutoDeposit, Proto, Liquid);
		UnityCommunityAutoDepositToggle.gameObject.SetActive(flag8);
		UnityCharacterAutoDepositToggle.gameObject.SetActive(flag8 && !CharacterSameAsCommunity);
		UnityCommunityTargetAmountField.transform.parent.gameObject.SetActive(EquipmentPolicy.IsTargetAmountPossibleForItem(Proto, Liquid, CommunityPolicy));
		UnityCharacterTargetAmountField.transform.parent.gameObject.SetActive(EquipmentPolicy.IsTargetAmountPossibleForItem(Proto, Liquid, CharacterPolicy) && !CharacterSameAsCommunity);
		SettingPolicy = false;
	}

	public void OnCommunityCanUse(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanUse, on, forCommunity: true);
		}
	}

	public void OnCommunityCanCraftWith(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanCraftWith, on, forCommunity: true);
		}
	}

	public void OnCommunityCanPlant(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanPlant, on, forCommunity: true);
		}
	}

	public void OnCommunityCanShare(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanShare, on, forCommunity: true);
		}
	}

	public void OnCommunityCanFeedToAnimals(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanFeedToAnimals, on, forCommunity: true);
		}
	}

	public void OnCommunityCanStrip(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanStrip, on, forCommunity: true);
		}
	}

	public void OnCommunityAutoCollect(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.AutoCollect, on, forCommunity: true);
		}
	}

	public void OnCommunityAutoDeposit(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.AutoDeposit, on, forCommunity: true);
		}
	}

	public void OnCommunityTargetAmountTextChanged(string v)
	{
		if (!SettingPolicy)
		{
			CommunityTargetAmount = StringUtil.ParseFloat(v) * ((GameImpl.Instance.Settings.UseMetricWeights && Liquid != null) ? 33.814056f : 1f);
		}
	}

	public void OnCharacterCanUse(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanUse, on, forCommunity: false);
		}
	}

	public void OnCharacterCanCraftWith(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanCraftWith, on, forCommunity: false);
		}
	}

	public void OnCharacterCanPlant(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanPlant, on, forCommunity: false);
		}
	}

	public void OnCharacterCanShare(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanShare, on, forCommunity: false);
		}
	}

	public void OnCharacterCanFeedToAnimals(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanFeedToAnimals, on, forCommunity: false);
		}
	}

	public void OnCharacterCanStrip(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.CanStrip, on, forCommunity: false);
		}
	}

	public void OnCharacterAutoCollect(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.AutoCollect, on, forCommunity: false);
		}
	}

	public void OnCharacterAutoDeposit(bool on)
	{
		if (!SettingPolicy)
		{
			SetPolicy(EquipmentPolicyAction.AutoDeposit, on, forCommunity: false);
		}
	}

	public void OnCharacterTargetAmountTextChanged(string v)
	{
		if (!SettingPolicy)
		{
			CharacterTargetAmount = StringUtil.ParseFloat(v) * ((GameImpl.Instance.Settings.UseMetricWeights && Liquid != null) ? 33.814056f : 1f);
		}
	}

	public void OnCharacterSameAsCommunity(bool on)
	{
		if (!SettingPolicy)
		{
			CharacterSameAsCommunity = on;
			Populate();
		}
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
