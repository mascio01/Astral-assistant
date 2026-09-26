using UnityEngine;
using UnityEngine.UI;

public class EquipmentIconBehaviour : MonoBehaviour
{
	public static Color32 SpecialIconCol = new Color32(0, byte.MaxValue, 216, byte.MaxValue);

	public RawImage UnityIcon;

	public RawImage UnityLiquidAmountBarBG;

	public RawImage UnityLiquidAmountBar;

	public RawImage UnityLiquidIcon;

	public RawImage UnityInfectedWithIcon;

	public RawImage UnityGatheredIcon;

	public RawImage UnityGiftIcon;

	public RawImage UnitySpecialIcon;

	public Equipment Item;

	public InfectionType InfectedWith;

	public Recipe Recipe;

	public void Awake()
	{
		UnityIcon = base.gameObject.GetComponent<RawImage>();
		UnityLiquidAmountBarBG = base.transform.Find("LiquidAmountBarBG").gameObject.GetComponent<RawImage>();
		UnityLiquidAmountBar = base.transform.Find("LiquidAmountBarBG/LiquidAmountBar").gameObject.GetComponent<RawImage>();
		UnityLiquidIcon = base.transform.Find("LiquidIcon").gameObject.GetComponent<RawImage>();
		UnityInfectedWithIcon = base.transform.Find("IconsPanel/InfectedWithIcon").gameObject.GetComponent<RawImage>();
		UnityGatheredIcon = base.transform.Find("IconsPanel/GatheredIcon").gameObject.GetComponent<RawImage>();
		UnityGiftIcon = base.transform.Find("IconsPanel/GiftIcon").gameObject.GetComponent<RawImage>();
		UnitySpecialIcon = base.transform.Find("IconsPanel/SpecialIcon").gameObject.GetComponent<RawImage>();
	}

	public void Initialize(Equipment item, InfectionType infectedWith)
	{
		Item = item;
		InfectedWith = infectedWith;
		Recipe = null;
	}

	public void Initialize(Recipe recipe)
	{
		Recipe = recipe;
		InfectedWith = InfectionType.None;
		Item = null;
	}

	public void Update()
	{
		if (Item != null)
		{
			UnityIcon.texture = Item.GetIcon(out var mat, out var col, highlighted: false);
			UnityIcon.material = mat;
			UnityIcon.color = col;
			LiquidPrototype liquidPrototype = Item.GetLiquidContentsType();
			if (liquidPrototype == null)
			{
				liquidPrototype = Item.DesignatedLiquid;
			}
			UnityLiquidAmountBarBG.gameObject.SetActive(Item.GetLiquidAmountBar(out var amount, out var col2));
			if (UnityLiquidAmountBarBG.gameObject.activeSelf)
			{
				UnityLiquidAmountBar.rectTransform.sizeDelta = new Vector2(100f * amount, UnityLiquidAmountBar.rectTransform.sizeDelta.y);
				UnityLiquidAmountBar.color = col2;
			}
			UnityLiquidIcon.gameObject.SetActive(liquidPrototype != null);
			if (UnityLiquidIcon.gameObject.activeSelf)
			{
				UnityLiquidIcon.texture = ((liquidPrototype.Tex != null && liquidPrototype.Tex.GetAsset() != null) ? liquidPrototype.Tex.GetAsset() : null);
				Color color = liquidPrototype.Col;
				color.a = ((Item.GetLiquidContentsAmount() > 0f) ? 1f : 0.5f);
				UnityLiquidIcon.color = color;
			}
			UnityInfectedWithIcon.gameObject.SetActive(InfectedWith != InfectionType.None);
			if (UnityInfectedWithIcon.gameObject.activeSelf)
			{
				UnityInfectedWithIcon.color = GameTerrain.MinimapSettings.GetInfectionCol(InfectedWith);
			}
			UnityGatheredIcon.gameObject.SetActive(Item.GetGatheredAmount() > 0);
			UnityGiftIcon.gameObject.SetActive(Item.Gifted);
			UnitySpecialIcon.gameObject.SetActive(Item.GetPrototype().Special);
		}
		else if (Recipe != null)
		{
			SetUnityIconFromRecipeProduct(UnityIcon, Recipe);
			UnityLiquidAmountBarBG.gameObject.SetActive(value: false);
			UnityLiquidIcon.gameObject.SetActive(value: false);
			UnityInfectedWithIcon.gameObject.SetActive(value: false);
			UnitySpecialIcon.gameObject.SetActive(value: false);
			UnityGatheredIcon.gameObject.SetActive(value: false);
			UnityGiftIcon.gameObject.SetActive(value: false);
		}
	}

	public static void SetUnityIconFromRecipeProduct(RawImage unityIcon, Recipe recipe)
	{
		if (recipe.ProductPrototype != null && recipe.ProductPrototype.Tex != null && recipe.ProductPrototype.Tex.GetAsset() != null)
		{
			unityIcon.texture = (Texture2D)recipe.ProductPrototype.Tex;
			unityIcon.color = Color.white;
			unityIcon.material = Hud.OutlineBlackMat;
		}
		else if (recipe.ProductLiquidPrototype != null && recipe.ProductLiquidPrototype.Tex != null && recipe.ProductLiquidPrototype.Tex.GetAsset() != null)
		{
			unityIcon.texture = (Texture2D)recipe.ProductLiquidPrototype.Tex;
			unityIcon.color = recipe.ProductLiquidPrototype.Col;
			unityIcon.material = Hud.OutlineBlackMat;
		}
		else if (recipe.ProductPropPrototype != null)
		{
			unityIcon.texture = IconGenerator.Instance.GetIconForObjectType(recipe.ProductPropPrototype, out var mat, highlighted: false);
			unityIcon.color = Color.white;
			unityIcon.material = mat;
		}
	}
}
