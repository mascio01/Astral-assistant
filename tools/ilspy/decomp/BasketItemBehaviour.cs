using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasketItemBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public static BasketItemBehaviour Hovered;

	public static GameObject HoveredGameObject;

	private RawImage UnityIcon;

	private RawImage UnityLiquidIcon;

	private TextMeshProUGUI UnityText;

	private Texture2D Tex;

	private LiquidPrototype Liquid;

	private string Str;

	private int PendingTradeIndex;

	private void Awake()
	{
		UnityIcon = base.transform.GetChild(0).GetComponent<RawImage>();
		UnityLiquidIcon = base.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
		UnityText = base.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (PendingTradeIndex != -1)
		{
			HoveredGameObject = base.gameObject;
			Hovered = this;
		}
		Populate();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HoveredGameObject = null;
		Hovered = null;
		Populate();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (PendingTradeIndex == -1)
		{
			return;
		}
		TradePage tradePage = InfoScreen.Instance.GetCurrentPage() as TradePage;
		if (tradePage != null)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				tradePage.RemovePendingTrade(PendingTradeIndex);
			}
			else if (eventData.button == PointerEventData.InputButton.Right)
			{
				tradePage.SubtractPendingTrade(PendingTradeIndex, 1);
			}
		}
	}

	public void Initialize(Texture2D tex, LiquidPrototype liquid, string str, int index)
	{
		Tex = tex;
		Liquid = liquid;
		Str = str;
		PendingTradeIndex = index;
		if (Hovered == this && PendingTradeIndex == -1)
		{
			Hovered = null;
		}
		Populate();
	}

	public void Populate()
	{
		UnityIcon.gameObject.SetActive(Tex != null);
		UnityIcon.texture = Tex;
		UnityIcon.material = ((Hovered == this) ? Hud.OutlineRedMat : Hud.OutlineBlackMat);
		UnityLiquidIcon.gameObject.SetActive(Liquid != null && Liquid.Tex != null);
		if (Liquid != null && Liquid.Tex != null)
		{
			UnityLiquidIcon.texture = (Texture2D)Liquid.Tex;
			UnityLiquidIcon.color = Liquid.Col;
		}
		UnityText.SetUnityText(Str);
		UnityText.color = ((Hovered == this) ? Color.red : Color.black);
	}
}
