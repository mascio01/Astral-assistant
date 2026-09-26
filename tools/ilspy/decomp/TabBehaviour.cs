using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public HalftoneBehaviour UnityBackground;

	public TextMeshProUGUI UnityText;

	private Material Mat;

	private float HoveredTransition;

	private BaseTabbableMenu TabbableMenu;

	private BaseTabPage Page;

	public bool Selected;

	public Color SelectedCol = Color.white;

	public bool WantJoin;

	public bool Interactable = true;

	public static TabBehaviour CurrentHovered;

	public void Init(BaseTabbableMenu menu, BaseTabPage page)
	{
		TabbableMenu = menu;
		Page = page;
	}

	private void Awake()
	{
		UnityBackground = base.transform.Find("Background").GetComponent<HalftoneBehaviour>();
		UnityText = base.transform.Find("Background/Text").GetComponent<TextMeshProUGUI>();
		Mat = null;
	}

	public void OnDestroy()
	{
		if (Mat != null)
		{
			Object.Destroy(Mat);
		}
	}

	public void Update()
	{
		bool flag = Session.Instance != null && Session.Instance.IsDebugMenuOpen();
		HoveredTransition = Mathf.Clamp01(HoveredTransition + ((CurrentHovered == this && !flag && Interactable) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f);
		Color color = InfoScreen.HalftoneTab.GetAsset().color;
		Color color2 = InfoScreen.HalftoneTab.GetAsset().GetColor(ShaderHash._DotColor);
		Color color3 = InfoScreen.HalftoneEquipped.GetAsset().color;
		Color color4 = InfoScreen.HalftoneEquipped.GetAsset().GetColor(ShaderHash._DotColor);
		if (Mat == null)
		{
			UnityBackground.material = new Material(InfoScreen.HalftoneTab);
			Mat = UnityBackground.materialForRendering;
		}
		if (TabbableMenu != null)
		{
			Selected = TabbableMenu.GetCurrentPage() == Page;
		}
		Mat.color = (Selected ? SelectedCol : (Interactable ? Color.Lerp(color, color3, HoveredTransition) : Color.gray));
		Mat.SetColor(ShaderHash._DotColor, Selected ? SelectedCol : (Interactable ? Color.Lerp(color2, color4, HoveredTransition) : Color.gray));
		UnityText.color = (Interactable ? Color.white : Color.gray);
		if (WantJoin)
		{
			UnityBackground.rectTransform.offsetMin = new Vector2(0f, Selected ? (-2f) : 0f);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SoundManager.PlayMenuSound(SoundManager.HoverSound);
		CurrentHovered = this;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (CurrentHovered == this)
		{
			CurrentHovered = null;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (Interactable)
		{
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
			if (Page != null)
			{
				Page.OnTabClicked();
			}
			else
			{
				Selected = true;
			}
		}
	}
}
