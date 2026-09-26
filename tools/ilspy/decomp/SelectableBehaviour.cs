using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler
{
	public enum SelectionMode
	{
		Cursor,
		Buttons
	}

	public static SelectionMode CurSelectionMode;

	public static Selectable CurrentCursorHovered;

	public static bool InSelectCall;

	public Selectable UnitySelectable;

	public bool WantMoveCursorOnSelect = true;

	public void Awake()
	{
		UnitySelectable = GetComponent<Selectable>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		StaticOnPointerEnter(UnitySelectable);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StaticOnPointerExit(UnitySelectable);
	}

	public void OnSelect(BaseEventData eventData)
	{
		StaticOnSelect(UnitySelectable, WantMoveCursorOnSelect);
	}

	public static void StaticOnPointerEnter(Selectable selectable)
	{
		if (selectable.interactable)
		{
			SoundManager.PlayMenuSound(SoundManager.HoverSound);
			CurrentCursorHovered = selectable;
			InSelectCall = true;
			selectable.Select();
			InSelectCall = false;
		}
	}

	public static void StaticOnPointerExit(Selectable selectable)
	{
		CurrentCursorHovered = null;
	}

	public static void StaticOnSelect(Selectable selectable, bool wantMoveCursorOnSelect)
	{
		if (!InSelectCall && wantMoveCursorOnSelect && CurSelectionMode == SelectionMode.Buttons)
		{
			MoveCursorToButton(selectable);
		}
	}

	public static void MoveCursorToButton(Selectable selectable)
	{
		RectTransform child = (RectTransform)selectable.transform;
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(HudBehaviour.Instance.HudPanelRectTransform, child);
		InputFunctionManager.Instance.SetCursorPos(bounds.center);
	}
}
