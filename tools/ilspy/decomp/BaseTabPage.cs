using System.Text;
using UnityEngine;

public class BaseTabPage : BaseSelectionOwner
{
	public BaseTabbableMenu Owner;

	public TabBehaviour UnityTab;

	public override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
		OnAwake();
	}

	public override void Update()
	{
		UnityCanvasGroup.interactable = !GameImpl.Instance.IsDialogOpen();
		if (UnityEventSystem != null)
		{
			UnityEventSystem.gameObject.SetActive(UnityCanvasGroup.interactable);
		}
		base.Update();
	}

	public virtual void OnAwake()
	{
	}

	public virtual void OnActivate()
	{
		base.gameObject.SetActive(value: true);
		((RectTransform)base.gameObject.transform).anchoredPosition3D = Vector3.zero;
		Populate();
	}

	public virtual void OnDeactivate()
	{
		DeactivateEventSystem(popped: true);
	}

	public virtual void Populate()
	{
	}

	public virtual void PreHandleInput(InputFrame inputFrame)
	{
	}

	public virtual void HandleInput(InputFrame inputFrame)
	{
	}

	public virtual void OnTabClicked()
	{
		Owner.SetPage(this);
	}

	public virtual void BuildDisplayName(StringBuilder sb)
	{
	}
}
