using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BaseSelectionOwner : MonoBehaviour
{
	public EventSystem UnityEventSystem;

	public CanvasGroup UnityCanvasGroup;

	public Selectable LastSelected;

	public int LastInteractableFrame = -1000;

	public virtual void Awake()
	{
		GameObject gameObject = base.gameObject.FindChild("EventSystem");
		UnityEventSystem = ((gameObject != null) ? gameObject.GetComponent<EventSystem>() : null);
		UnityCanvasGroup = base.gameObject.GetComponent<CanvasGroup>();
	}

	public virtual void Update()
	{
		if (!UnityEventSystem || !(UnityEventSystem.currentInputModule != null))
		{
			return;
		}
		UnityEventSystem.currentInputModule.inputOverride = CustomInputBehaviour.Instance;
		if (!UnityCanvasGroup.interactable)
		{
			return;
		}
		if (LastInteractableFrame < Time.frameCount - 1)
		{
			if (LastSelected != null)
			{
				LastSelected.Select();
				LastSelected = null;
			}
		}
		else if (UnityEventSystem.currentSelectedGameObject != null)
		{
			LastSelected = UnityEventSystem.currentSelectedGameObject.GetComponent<Selectable>();
		}
		LastInteractableFrame = Time.frameCount;
	}

	public void DeactivateEventSystem(bool popped)
	{
		if ((bool)UnityEventSystem)
		{
			UnityEventSystem.gameObject.SetActive(value: false);
		}
		base.gameObject.SetActive(value: false);
		if (popped)
		{
			LastSelected = null;
		}
	}
}
