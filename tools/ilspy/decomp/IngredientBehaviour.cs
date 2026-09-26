using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientBehaviour : Selectable
{
	public Character Crafter;

	public Equipment Item;

	public EquipmentPrototype Proto;

	public LiquidPrototype Liquid;

	public PropPrototype PropProto;

	public bool Enabled;

	public InfectionType InfectedWith;

	public TextMeshProUGUI UnityText;

	public RawImage UnityIcon;

	public RawImage UnityRestrictedIcon;

	public RawImage UnityInfectedWithIcon;

	private Material UnityIconMat;

	private float HoveredTransition;

	private bool IsProduct;

	private bool Dirty;

	protected override void Awake()
	{
		base.Awake();
		UnityText = base.transform.Find("Text").gameObject.GetComponent<TextMeshProUGUI>();
		UnityIcon = base.transform.Find("Panel/Icon").gameObject.GetComponent<RawImage>();
		UnityRestrictedIcon = base.transform.Find("Panel/Icon/RestrictedIcon").gameObject.GetComponent<RawImage>();
		UnityInfectedWithIcon = base.transform.Find("Panel/Icon/InfectedWithIcon").gameObject.GetComponent<RawImage>();
	}

	public void Initialize(Character crafter, Equipment item, bool enabled)
	{
		Crafter = crafter;
		Proto = item.GetPrototype();
		Item = item;
		Enabled = enabled;
		InfectedWith = item.InfectedWith;
		Dirty = true;
	}

	public void Initialize(Character crafter, EquipmentPrototype proto, LiquidPrototype liquid, bool enabled, InfectionType infectedWith, bool isProduct)
	{
		Crafter = crafter;
		Proto = proto;
		Liquid = liquid;
		Enabled = enabled;
		InfectedWith = infectedWith;
		IsProduct = isProduct;
		Dirty = true;
	}

	public void Initialize(Character crafter, PropPrototype propProto, bool enabled, bool isProduct)
	{
		Crafter = crafter;
		PropProto = propProto;
		Enabled = enabled;
		IsProduct = isProduct;
		Dirty = true;
	}

	public void Populate()
	{
		if (UnityIconMat == null)
		{
			UnityIcon.material = new Material(UnityIcon.material);
			UnityIconMat = UnityIcon.materialForRendering;
		}
		if (Item != null)
		{
			UnityText.SetUnityText(Item.GetDisplayNameString());
			UnityIcon.texture = Item.GetIcon(out var _, out var col, highlighted: false);
			UnityIcon.color = col;
			UnityIcon.SetMaterialDirty();
			UnityIconMat = UnityIcon.materialForRendering;
		}
		else if (Proto != null)
		{
			UnityText.SetUnityText(GameImpl.Translate(Proto.NameHash));
			UnityIcon.texture = ((Proto.Tex != null && Proto.Tex.GetAsset() != null) ? Proto.Tex.GetAsset() : null);
			UnityIcon.SetMaterialDirty();
			UnityIconMat = UnityIcon.materialForRendering;
		}
		else if (Liquid != null)
		{
			UnityText.SetUnityText(GameImpl.Translate(Liquid.NameHash));
			UnityIcon.texture = ((Liquid.Tex != null && Liquid.Tex.GetAsset() != null) ? Liquid.Tex.GetAsset() : null);
			UnityIcon.SetMaterialDirty();
			UnityIconMat = UnityIcon.materialForRendering;
		}
		else if (PropProto != null)
		{
			UnityIcon.texture = IconGenerator.Instance.GetIconForObjectType(PropProto, out var mat2, highlighted: false);
			UnityIcon.material = mat2;
			UnityText.SetUnityText(GameImpl.Translate(PropProto.NameHash));
		}
		UnityInfectedWithIcon.gameObject.SetActive(InfectedWith != InfectionType.None);
		UnityInfectedWithIcon.color = GameTerrain.MinimapSettings.GetInfectionCol(InfectedWith);
	}

	public void Update()
	{
		if (CraftAmountBox.Instance != null)
		{
			base.interactable = CraftAmountBox.Instance.Recurring && !IsProduct;
		}
		if (Dirty)
		{
			Populate();
			Dirty = false;
		}
		if (Proto != null || Item != null || PropProto != null)
		{
			UnityText.color = Color.black * (Enabled ? 1f : 0.5f);
			UnityIcon.color = Color.white * (Enabled ? 1f : 0.5f);
		}
		else if (Liquid != null)
		{
			UnityText.color = Color.black * (Enabled ? 1f : 0.5f);
			UnityIcon.color = (Color)Liquid.Col * (Enabled ? 1f : 0.5f);
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
			UnityIconMat.color = (Enabled ? Color.white : Color.gray);
		}
		if (UnityRestrictedIcon != null && Crafter != null && (Proto != null || Liquid != null))
		{
			UnityRestrictedIcon.gameObject.SetActive(CraftAmountBox.Instance.Recurring && !IsProduct && !Crafter.IsActionAllowedForItem(Proto, Liquid, InfectedWith, EquipmentPolicyAction.CanCraftWith));
		}
	}
}
