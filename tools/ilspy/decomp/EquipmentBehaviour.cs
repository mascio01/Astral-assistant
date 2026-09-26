using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentBehaviour : Selectable, IPointerClickHandler, IEventSystemHandler, ISubmitHandler
{
	public static EquipmentBehaviour CurrentHovered;

	public TextMeshProUGUI UnityAmountText;

	public RawImage UnityAmountBG;

	public RawImage UnityIcon;

	public RawImage UnityBackground;

	public RawImage UnityLiquidAmountBarBG;

	public RawImage UnityLiquidAmountBar;

	public RawImage UnityLiquidIcon;

	public RawImage UnityUndiscoveredIcon;

	public RawImage UnityInfectedWithIcon;

	public RawImage UnityGatheredIcon;

	public RawImage UnityGiftIcon;

	public RawImage UnitySpecialIcon;

	private Material UnityIconMat;

	public float HoveredTransition;

	public TileObject Carrier;

	public Equipment Item;

	public int AmountAfterPendingTrades;

	public int VisibleAmountWithoutTrades;

	public bool Transferred;

	public EquipmentOptionBehaviour ParentEquipmentOptionBehaviour;

	public InventoryBehaviour ParentInventoryBehaviour;

	protected override void Awake()
	{
		base.Awake();
		UnityAmountText = base.transform.Find("AmountBG/AmountText").gameObject.GetComponent<TextMeshProUGUI>();
		UnityAmountBG = base.transform.Find("AmountBG").gameObject.GetComponent<RawImage>();
		UnityIcon = base.transform.Find("Background/Icon").gameObject.GetComponent<RawImage>();
		if (Application.isPlaying)
		{
			UnityIcon.material = new Material(UnityIcon.material);
			UnityIconMat = UnityIcon.materialForRendering;
		}
		UnityBackground = base.transform.Find("Background").gameObject.GetComponent<RawImage>();
		UnityLiquidAmountBarBG = base.transform.Find("LiquidAmountBarBG").gameObject.GetComponent<RawImage>();
		UnityLiquidAmountBar = base.transform.Find("LiquidAmountBarBG/LiquidAmountBar").gameObject.GetComponent<RawImage>();
		UnityLiquidIcon = base.transform.Find("LiquidIcon").gameObject.GetComponent<RawImage>();
		UnityUndiscoveredIcon = base.transform.Find("Background/UndiscoveredIcon").gameObject.GetComponent<RawImage>();
		UnityInfectedWithIcon = base.transform.Find("IconsPanel/InfectedWithIcon").gameObject.GetComponent<RawImage>();
		UnityGatheredIcon = base.transform.Find("IconsPanel/GatheredIcon").gameObject.GetComponent<RawImage>();
		UnityGiftIcon = base.transform.Find("IconsPanel/GiftIcon").gameObject.GetComponent<RawImage>();
		UnitySpecialIcon = base.transform.Find("IconsPanel/SpecialIcon").gameObject.GetComponent<RawImage>();
	}

	protected override void OnDestroy()
	{
		if (Application.isPlaying && UnityIcon != null)
		{
			Object.Destroy(UnityIcon.material);
		}
		base.OnDestroy();
	}

	public void Initialize(TileObject carrier, Equipment item)
	{
		Carrier = carrier;
		Item = item;
		ParentEquipmentOptionBehaviour = base.transform.parent.GetComponent<EquipmentOptionBehaviour>();
		Update();
	}

	public void Populate()
	{
		if (ParentEquipmentOptionBehaviour != null)
		{
			AmountAfterPendingTrades = (VisibleAmountWithoutTrades = Item.GetAmount());
		}
		int num = (Transferred ? AmountAfterPendingTrades : (AmountAfterPendingTrades - Item.GetAmount()));
		int num2 = (Transferred ? AmountAfterPendingTrades : (VisibleAmountWithoutTrades + num));
		UnityAmountText.transform.parent.gameObject.SetActive(num2 > 1 || Transferred || (Item.CanBeCombined() && !(Item is MeleeWeapon)));
		UnityAmountText.SetUnityText(num2.ToString());
		UnityAmountBG.color = ((num > 0) ? GameTerrain.MinimapSettings.IncrementCol : ((num < 0) ? GameTerrain.MinimapSettings.DecrementCol : GameTerrain.MinimapSettings.AmountCol));
		UnityIcon.texture = null;
		UnityGatheredIcon.gameObject.SetActive(Item.GetGatheredAmount() > 0);
		UnityGiftIcon.gameObject.SetActive(Item.Gifted);
		UnitySpecialIcon.gameObject.SetActive(Item.GetPrototype().Special);
		Update();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (ParentEquipmentOptionBehaviour != null)
		{
			ParentEquipmentOptionBehaviour.OnToggle();
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		if (ParentEquipmentOptionBehaviour != null)
		{
			ParentEquipmentOptionBehaviour.OnToggle();
		}
	}

	private bool ShowEquipped()
	{
		if (ParentEquipmentOptionBehaviour != null)
		{
			return CharacterCreationMenu.Instance.PreviewGimp.InventoryContains(Item);
		}
		return Item.IsEquippedOrWorn(Carrier);
	}

	public void Update()
	{
		if (Item == null || UnityIcon == null || !Application.isPlaying)
		{
			return;
		}
		bool flag = Session.Instance != null && Session.Instance.IsDebugMenuOpen();
		HoveredTransition = Mathf.Clamp01(HoveredTransition + (((base.currentSelectionState == SelectionState.Highlighted || base.currentSelectionState == SelectionState.Selected || base.currentSelectionState == SelectionState.Pressed) && !flag) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f);
		Vector2 uIObjectCentreOnScreen = HudBehaviour.Instance.GetUIObjectCentreOnScreen(UnityIcon.gameObject);
		if (uIObjectCentreOnScreen.y >= -128f && uIObjectCentreOnScreen.y <= 1208f)
		{
			Material mat;
			Color col;
			Texture2D icon = Item.GetIcon(out mat, out col, highlighted: false);
			if (UnityIcon.texture != icon)
			{
				UnityIcon.texture = icon;
				UnityIcon.color = col;
				UnityIcon.SetMaterialDirty();
				UnityIconMat = UnityIcon.materialForRendering;
			}
		}
		UnityIcon.transform.localScale = Vector3.one * (1f + Mathf.SmoothStep(0f, 1f, HoveredTransition) * 0.25f);
		if (UnityIconMat != null)
		{
			UnityIconMat.SetColor(ShaderHash._OutlineColor, Color.Lerp(Color.black, Color.red, HoveredTransition));
			UnityIconMat.color = (base.interactable ? Color.white : Color.gray);
		}
		if (ShowEquipped())
		{
			UnityBackground.material = (base.interactable ? InfoScreen.HalftoneEquipped : InfoScreen.HalftoneDisabled);
		}
		else
		{
			UnityBackground.material = null;
			UnityBackground.color = (base.interactable ? Color.white : Color.gray);
		}
		LiquidPrototype liquidPrototype = ((Item.GetLiquidCapacity() > 0f) ? Item.GetLiquidContentsType() : null);
		UnityLiquidAmountBarBG.gameObject.SetActive(Item.GetLiquidAmountBar(out var amount, out var col2));
		if (UnityLiquidAmountBarBG.gameObject.activeSelf)
		{
			UnityLiquidAmountBar.rectTransform.sizeDelta = new Vector2(100f * amount, UnityLiquidAmountBar.rectTransform.sizeDelta.y);
			UnityLiquidAmountBar.color = col2;
		}
		if (liquidPrototype == null)
		{
			liquidPrototype = Item.DesignatedLiquid;
		}
		Armor armor = Item as Armor;
		UnityLiquidIcon.gameObject.SetActive(liquidPrototype != null || armor != null);
		if (UnityLiquidIcon.gameObject.activeSelf)
		{
			if (liquidPrototype != null)
			{
				Color color = liquidPrototype.Col;
				color.a = ((Item.GetLiquidContentsAmount() > 0f) ? 1f : 0.5f);
				UnityLiquidIcon.texture = ((liquidPrototype.Tex != null && liquidPrototype.Tex.GetAsset() != null) ? liquidPrototype.Tex.GetAsset() : null);
				UnityLiquidIcon.color = color;
				UnityLiquidIcon.rectTransform.sizeDelta = new Vector2(48f, 48f);
			}
			else if (armor != null)
			{
				UnityLiquidIcon.texture = ((armor.GetClothingType() == ClothingType.Hat) ? GameCursor.HelmetIcon.GetAsset() : ((armor.GetClothingType() == ClothingType.LegArmor) ? GameCursor.LegArmorIcon.GetAsset() : GameCursor.KevlarIcon.GetAsset()));
				UnityLiquidIcon.color = armor.GetArmorIconCol();
				UnityLiquidIcon.rectTransform.sizeDelta = new Vector2(32f, 32f);
			}
		}
		UnityInfectedWithIcon.gameObject.SetActive(Item.InfectedWith != InfectionType.None);
		if (UnityInfectedWithIcon.gameObject.activeSelf)
		{
			UnityInfectedWithIcon.color = GameTerrain.MinimapSettings.GetInfectionCol(Item.InfectedWith);
		}
		UnityUndiscoveredIcon.gameObject.SetActive((!Item.GetPrototype().Discovered || (Item.GetLiquidContentsType() != null && !Item.GetLiquidContentsType().Discovered)) && ParentEquipmentOptionBehaviour == null && Session.Instance != null && !Session.Instance.Editor);
	}
}
