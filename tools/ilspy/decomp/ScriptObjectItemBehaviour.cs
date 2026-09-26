using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScriptObjectItemBehaviour : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IComparable<ScriptObjectItemBehaviour>
{
	public ScriptObjectBehaviour Owner;

	public ScriptObjectBehaviour.CopyListItem CopyFunc;

	public ScriptObjectBehaviour.PasteListItem PasteFunc;

	public UnityAction DeleteFunc;

	public int ListIndex;

	public string TypeName;

	public string FieldName;

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
		{
			bool flag = InputFunctionManager.Instance.IsKeyPressed(KeyCode.LeftControl) || InputFunctionManager.Instance.IsKeyPressed(KeyCode.RightControl);
			if (!Owner.Dragging && !flag)
			{
				ScriptEditor.Instance.DeselectAllItems();
				ScriptEditor.Instance.DeselectAll();
			}
			if (flag && ScriptEditor.Instance.SelectedItems.Contains(this))
			{
				ScriptEditor.Instance.DeselectItem(this);
			}
			else
			{
				ScriptEditor.Instance.Select(Owner);
				ScriptEditor.Instance.SelectItem(this);
			}
			ScriptEditor.Instance.CloseContextMenu();
			break;
		}
		case PointerEventData.InputButton.Right:
			ScriptEditor.Instance.OpenContextMenu(this);
			break;
		}
	}

	public string GetTypeName()
	{
		return TypeName;
	}

	public int CompareTo(ScriptObjectItemBehaviour other)
	{
		if (ListIndex < other.ListIndex)
		{
			return -1;
		}
		if (ListIndex > other.ListIndex)
		{
			return 1;
		}
		return 0;
	}
}
