using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class CloseIconBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public Image UnityImage;

	public RawImage UnityRawImage;

	public Outline UnityOutine;

	private bool IsPointerOver;

	private bool IsSelected;

	public Color OutlineCol = new Color(0f, 0f, 0f, 1f);

	public Color OutlineHoveredCol = new Color(1f, 0.5f, 0.5f, 1f);

	public Color OutlineSelectedCol = new Color(1f, 0f, 0f, 1f);

	public Sprite Idle;

	public Sprite Highlighted;

	public Sprite Pressed;

	public UnityEvent OnClicked;

	public CursorAction Tooltip;

	public static CloseIconBehaviour Hovered;

	public static GameObject HoveredGameObject;

	private void Awake()
	{
		UnityImage = GetComponent<Image>();
		UnityRawImage = GetComponent<RawImage>();
		UnityOutine = GetComponent<Outline>();
	}

	public void Update()
	{
		if (!Input.mousePresent)
		{
			if (Hovered != this && InputFunctionManager.Instance.IsMouseOverPanel((RectTransform)base.transform))
			{
				OnPointerEnter(null);
			}
			else if (Hovered == this && !InputFunctionManager.Instance.IsMouseOverPanel((RectTransform)base.transform))
			{
				OnPointerExit(null);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (UnityImage != null)
		{
			UnityImage.overrideSprite = (IsSelected ? Pressed : Highlighted);
		}
		if (UnityOutine != null)
		{
			UnityOutine.effectColor = OutlineHoveredCol;
		}
		IsPointerOver = true;
		Hovered = this;
		HoveredGameObject = base.gameObject;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (UnityImage != null)
		{
			UnityImage.overrideSprite = (IsSelected ? Pressed : Idle);
		}
		if (UnityOutine != null)
		{
			UnityOutine.effectColor = (IsSelected ? OutlineSelectedCol : OutlineCol);
		}
		IsPointerOver = false;
		HoveredGameObject = null;
		Hovered = null;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && UnityImage != null)
		{
			UnityImage.overrideSprite = Pressed;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			if (UnityImage != null)
			{
				UnityImage.overrideSprite = (IsSelected ? Pressed : (IsPointerOver ? Highlighted : Idle));
			}
			if (OnClicked != null && IsPointerOver)
			{
				OnClicked.Invoke();
			}
		}
	}

	public void SetSelected(bool on)
	{
		IsSelected = on;
		if (UnityOutine != null)
		{
			UnityOutine.effectColor = (IsPointerOver ? OutlineHoveredCol : (IsSelected ? OutlineSelectedCol : OutlineCol));
		}
		if (UnityImage != null)
		{
			UnityImage.overrideSprite = (IsSelected ? Pressed : (IsPointerOver ? Highlighted : Idle));
		}
	}
}
