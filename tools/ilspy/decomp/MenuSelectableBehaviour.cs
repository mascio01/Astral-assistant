using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuSelectableBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public bool WantLinkyCursor;

	public static int LinkyCursorRefCount;

	private Selectable UnitySelectable;

	public void Awake()
	{
		UnitySelectable = GetComponent<Selectable>();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		SoundManager.PlayMenuSound(SoundManager.HoverSound);
		if (InputFunctionManager.Instance.CurrentInputType == InputType.MouseAndKeyboard || eventData.delta.sqrMagnitude != 0f)
		{
			UnitySelectable.Select();
		}
		if (WantLinkyCursor)
		{
			LinkyCursorRefCount++;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (WantLinkyCursor)
		{
			LinkyCursorRefCount = Math.Max(0, LinkyCursorRefCount - 1);
		}
	}
}
