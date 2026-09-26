using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentTotalBehaviour : Selectable
{
	public static EquipmentTotalBehaviour CurrentHovered;

	public TextMeshProUGUI UnityAmountText;

	public RawImage UnityIcon;

	public RawImage UnityBackground;

	public RawImage UnityAmountBackground;

	public RawImage UnitySpecialIcon;

	private Material UnityIconMat;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public Prop SettingStoragePolicyForProp;

	public Equipment DesignatingLiquidForItem;

	public float Amount;

	public bool Dirty;

	public float HoveredTransition;

	public static int HUD_FlOz = StringUtil.JenkinsHash("HUD_FlOz");

	public static int HUD_Liter = StringUtil.JenkinsHash("HUD_Liter");

	public static Color Yellow = new Color32(byte.MaxValue, 185, 0, byte.MaxValue);

	public bool OnScreen;

	protected override void Awake()
	{
		base.Awake();
		UnityAmountText = base.transform.Find("AmountBG/AmountText").gameObject.GetComponent<TextMeshProUGUI>();
		UnityIcon = base.transform.Find("Background/Icon").gameObject.GetComponent<RawImage>();
		if (Application.isPlaying)
		{
			UnityIcon.material = new Material(UnityIcon.material);
			UnityIconMat = UnityIcon.materialForRendering;
		}
		UnityBackground = base.transform.Find("Background").gameObject.GetComponent<RawImage>();
		UnityAmountBackground = base.transform.Find("AmountBG").gameObject.GetComponent<RawImage>();
		UnitySpecialIcon = base.transform.Find("Background/IconsPanel/SpecialIcon").gameObject.GetComponent<RawImage>();
		Dirty = true;
		if (OnScreen)
		{
			UnityBackground.gameObject.SetActive(value: true);
			UnityAmountBackground.gameObject.SetActive(value: true);
		}
	}

	public void Initialize(EquipmentPrototype proto, LiquidPrototype liquid, bool checkOnScreen = false)
	{
		Proto = proto;
		Liquid = liquid;
		Dirty = true;
		if (checkOnScreen)
		{
			base.enabled = false;
			return;
		}
		UnityBackground.gameObject.SetActive(value: true);
		UnityAmountBackground.gameObject.SetActive(value: true);
		OnScreen = true;
	}

	public void SetAmount(float amount)
	{
		Amount = amount;
		Dirty = true;
	}

	public void Populate()
	{
		if (Proto != null)
		{
			UnityAmountText.SetUnityText(((int)Amount).ToString());
			UnityAmountText.fontSize = 28f;
			if (Proto.Tex != null && Proto.Tex.GetAsset() != null)
			{
				UnityIcon.texture = (Texture2D)Proto.Tex;
				UnityIcon.SetMaterialDirty();
				UnityIconMat = UnityIcon.materialForRendering;
			}
			UnitySpecialIcon.gameObject.SetActive(Proto.Special);
		}
		else if (Liquid != null)
		{
			bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
			StringUtil.SetUnityText(str: (Amount * (useMetricWeights ? 0.0295735f : 1f)).ToString(AvailableAction.SensibleFloatFormat) + GameImpl.Translate(useMetricWeights ? HUD_Liter : HUD_FlOz), unityText: UnityAmountText);
			UnityAmountText.fontSize = ((UnityAmountText.text.Length > 8) ? 16f : ((UnityAmountText.text.Length > 7) ? 20f : ((UnityAmountText.text.Length > 6) ? 24f : 28f)));
			if (Liquid.Tex != null && Liquid.Tex.GetAsset() != null)
			{
				UnityIcon.texture = (Texture2D)Liquid.Tex;
				UnityIcon.color = Liquid.Col;
				UnityIcon.SetMaterialDirty();
				UnityIconMat = UnityIcon.materialForRendering;
			}
			UnitySpecialIcon.gameObject.SetActive(value: false);
		}
	}

	public void SetOnScreen(bool onScreen)
	{
		if (OnScreen != onScreen)
		{
			OnScreen = onScreen;
			base.enabled = onScreen;
			if (UnityBackground != null)
			{
				UnityBackground.gameObject.SetActive(OnScreen);
				UnityAmountBackground.gameObject.SetActive(OnScreen);
			}
		}
	}

	public void Update()
	{
		if (Dirty)
		{
			Populate();
			Dirty = false;
		}
		bool flag = Session.Instance != null && Session.Instance.IsDebugMenuOpen();
		HoveredTransition = Mathf.Clamp01(HoveredTransition + (((base.currentSelectionState == SelectionState.Highlighted || base.currentSelectionState == SelectionState.Selected || base.currentSelectionState == SelectionState.Pressed) && !flag) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f);
		if (UnityIcon != null)
		{
			UnityIcon.transform.localScale = Vector3.one * (1f + Mathf.SmoothStep(0f, 1f, HoveredTransition) * 0.25f);
		}
		if (UnityIconMat != null)
		{
			UnityIconMat.SetColor(ShaderHash._OutlineColor, Color.Lerp(Color.black, Color.red, HoveredTransition));
			UnityIconMat.color = (base.interactable ? Color.white : Color.gray);
		}
		UnityAmountBackground.color = ((Amount > 0f) ? Yellow : Color.gray);
		if (DesignatingLiquidForItem != null)
		{
			base.interactable = DesignatingLiquidForItem.GetLiquidContentsType() == null || DesignatingLiquidForItem.GetLiquidContentsType() == Liquid;
			Color color = UnityAmountBackground.color;
			color.a = (base.interactable ? 1f : 0.5f);
			UnityAmountBackground.color = color;
			bool flag2 = DesignatingLiquidForItem.DesignatedLiquid == Liquid;
			UnityBackground.material = (flag2 ? InfoScreen.HalftoneEquipped : InfoScreen.HalftoneDisabled);
			UnityBackground.color = (base.interactable ? Color.white : new Color(1f, 1f, 1f, 0.5f));
		}
		else if (SettingStoragePolicyForProp != null)
		{
			bool flag3 = SettingStoragePolicyForProp.WantStoreHere(Proto, Liquid);
			UnityBackground.material = (flag3 ? InfoScreen.HalftoneEquipped : InfoScreen.HalftoneDisabled);
		}
		else
		{
			UnityBackground.material = null;
		}
	}
}
