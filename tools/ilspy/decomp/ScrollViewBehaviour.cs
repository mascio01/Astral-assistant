using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewBehaviour : MonoBehaviour
{
	private ScrollRect UnityScrollView;

	private void Awake()
	{
		UnityScrollView = GetComponent<ScrollRect>();
	}

	private static bool IsDescendantOf(Transform a, Transform b)
	{
		while (a != null)
		{
			if (a == b)
			{
				return true;
			}
			a = a.parent;
		}
		return false;
	}

	private void Update()
	{
		bool flag = InputFunctionManager.Instance.IsMouseAxisCaptured(MouseAxis.ScrollWheel);
		UnityScrollView.scrollSensitivity = (flag ? 0f : 100f);
		if (SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Buttons)
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (!(current != null))
		{
			return;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		if (currentSelectedGameObject != null && IsDescendantOf(currentSelectedGameObject.transform, UnityScrollView.content))
		{
			Canvas.ForceUpdateCanvases();
			float height = ((RectTransform)UnityScrollView.transform).rect.height;
			float height2 = ((RectTransform)UnityScrollView.content.transform).rect.height;
			Vector2 vector = UnityScrollView.content.transform.InverseTransformPoint(currentSelectedGameObject.transform.position);
			UnityScrollView.content.anchoredPosition = new Vector2(UnityScrollView.content.anchoredPosition.x, Mathf.Max(Mathf.Min((0f - height) * 0.5f - vector.y, height2 - height), 0f));
			Selectable component = currentSelectedGameObject.GetComponent<Selectable>();
			if (component != null)
			{
				SelectableBehaviour.StaticOnSelect(component, wantMoveCursorOnSelect: true);
			}
		}
	}
}
