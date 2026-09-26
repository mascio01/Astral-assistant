using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BaseTabbableMenu : MonoBehaviour
{
	public GameObject UnityTabBar;

	public GameObject UnityJoiner;

	public TextMeshProUGUI UnityTabLeftPrompt;

	public TextMeshProUGUI UnityTabRightPrompt;

	public bool WantRepopulate;

	public Equipment DesiredEquipmentToSelect;

	public TileObject DesiredEquipmentCarrierToSelect;

	protected List<BaseTabPage> Pages = new List<BaseTabPage>();

	protected int CurrentPage = -1;

	protected int DesiredPage = -1;

	public const string UnityTabTextObjName = "Background/Text";

	public const string UnityTabBackgroundObjName = "Background";

	private bool ShowTabPrompts = true;

	public virtual void OnAwake()
	{
	}

	public void Awake()
	{
		OnAwake();
		CacheUnityPtrs();
	}

	private void CacheUnityPtrs()
	{
		if (UnityTabBar == null)
		{
			UnityTabBar = base.transform.Find("TabBar").gameObject;
			UnityJoiner = base.transform.Find("Joiner").gameObject;
			UnityTabLeftPrompt = base.transform.Find("TabLeftPrompt").GetComponent<TextMeshProUGUI>();
			UnityTabRightPrompt = base.transform.Find("TabRightPrompt").GetComponent<TextMeshProUGUI>();
		}
	}

	public void Start()
	{
		if (CurrentPage == -1)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Update()
	{
		if (DesiredPage != -1)
		{
			if (DesiredPage < Pages.Count)
			{
				SetPage(DesiredPage);
			}
			DesiredPage = -1;
		}
		CacheUnityPtrs();
		if (CurrentPage == -1)
		{
			UnityJoiner.SetActive(value: false);
			return;
		}
		UnityJoiner.SetActive(value: true);
		RectTransform rectTransform = (RectTransform)UnityTabBar.transform;
		RectTransform rectTransform2 = (RectTransform)Pages[CurrentPage].UnityTab.transform;
		RectTransform obj = (RectTransform)UnityJoiner.transform;
		obj.anchoredPosition = MathUtil.ToXY(rectTransform.localPosition + rectTransform2.localPosition) - new Vector2(1f, rectTransform2.rect.height * 0.5f);
		obj.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform2.rect.width - 2f);
		UnityTabLeftPrompt.gameObject.SetActive(ShowTabPrompts);
		UnityTabRightPrompt.gameObject.SetActive(ShowTabPrompts);
		RectTransform obj2 = (RectTransform)UnityTabLeftPrompt.transform;
		RectTransform rectTransform3 = (RectTransform)UnityTabRightPrompt.transform;
		obj2.anchoredPosition = new Vector2(rectTransform.rect.xMin - 16f, rectTransform.anchoredPosition.y + 8f);
		rectTransform3.anchoredPosition = new Vector2(rectTransform.rect.xMax + 16f, rectTransform.anchoredPosition.y + 8f);
		StringUtil.SetUnityTextButtonPrompt(UnityTabLeftPrompt, InputFunction.TabLeft);
		StringUtil.SetUnityTextButtonPrompt(UnityTabRightPrompt, InputFunction.TabRight);
	}

	public void CheckWantRepopulate()
	{
		if (WantRepopulate && CurrentPage != -1)
		{
			Pages[CurrentPage].Populate();
		}
		if (DesiredEquipmentToSelect != null)
		{
			InfoPage infoPage = GetCurrentPage() as InfoPage;
			if (infoPage != null && SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons)
			{
				infoPage.TrySelectEquipment(DesiredEquipmentToSelect, DesiredEquipmentCarrierToSelect);
			}
			if (!WantRepopulate)
			{
				DesiredEquipmentToSelect = null;
				DesiredEquipmentCarrierToSelect = null;
			}
		}
		WantRepopulate = false;
	}

	public virtual void OnActivate(int page)
	{
		base.gameObject.SetActive(value: true);
		SetPage(page);
	}

	public virtual void OnDeactivate()
	{
		SetPage(-1);
		base.gameObject.SetActive(value: false);
		TabBehaviour.CurrentHovered = null;
	}

	public BaseTabPage GetCurrentPage()
	{
		if (CurrentPage == -1)
		{
			return null;
		}
		return Pages[CurrentPage];
	}

	public int GetPageCount()
	{
		return Pages.Count;
	}

	public int FindPageIndexByType(Type type)
	{
		for (int i = 0; i < Pages.Count; i++)
		{
			if (Pages[i].GetType() == type)
			{
				return i;
			}
		}
		return -1;
	}

	private void NextPage()
	{
		if (Pages.Count > 1)
		{
			SoundManager.PlayMenuSound(SoundManager.TabSound, 0.5f);
			SetPage((CurrentPage + 1) % Pages.Count);
		}
	}

	private void PrevPage()
	{
		if (Pages.Count > 1)
		{
			SoundManager.PlayMenuSound(SoundManager.TabSound, 0.5f);
			SetPage((CurrentPage + Pages.Count - 1) % Pages.Count);
		}
	}

	public virtual void SetPage(int page)
	{
		if (Util.AmIOnMainThread())
		{
			if (CurrentPage != -1)
			{
				Pages[CurrentPage].OnDeactivate();
			}
			CurrentPage = page;
			if (CurrentPage != -1)
			{
				Pages[CurrentPage].OnActivate();
			}
		}
		else
		{
			DesiredPage = page;
		}
	}

	public void SetPage(BaseTabPage page)
	{
		int num = Pages.IndexOf(page);
		if (num != -1)
		{
			SetPage(num);
		}
	}

	public void ClearTabs()
	{
		CacheUnityPtrs();
		Pages.Clear();
		UnityTabBar.DeleteAllChildren();
	}

	public void AddTab(BaseTabPage infoPage)
	{
		InsertTab(Pages.Count, infoPage);
	}

	public void InsertTab(int i, BaseTabPage infoPage)
	{
		infoPage.Owner = this;
		Pages.Insert(i, infoPage);
		GameObject obj = UnityEngine.Object.Instantiate(InfoScreen.Tab.GetAsset());
		obj.transform.SetParent(UnityTabBar.transform, worldPositionStays: false);
		obj.transform.SetSiblingIndex(i);
		TabBehaviour component = obj.GetComponent<TabBehaviour>();
		component.Init(this, infoPage);
		StringBuilder sb = new StringBuilder();
		infoPage.BuildDisplayName(sb);
		component.transform.Find("Background/Text").GetComponent<TextMeshProUGUI>().SetUnityText(sb);
		infoPage.UnityTab = component;
	}

	public void RemoveTab(int i)
	{
		UnityEngine.Object.DestroyImmediate(Pages[i].UnityTab.gameObject);
		Pages.RemoveAt(i);
	}

	public void ReInitTab(InfoPage infoPage)
	{
		int num = Pages.IndexOf(infoPage);
		if (num != -1)
		{
			RemoveTab(num);
		}
		InsertTab(num, infoPage);
	}

	public virtual void HandleInput(InputFrame inputFrame)
	{
		ShowTabPrompts = false;
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (!GameImpl.Instance.IsDialogOpen() && Pages.Count > 1 && !instance.IsCaptured(InputFunction.TabLeft) && !instance.IsCaptured(InputFunction.TabRight))
		{
			ShowTabPrompts = true;
			if (instance.IsJustPressed(InputFunction.TabLeft))
			{
				PrevPage();
			}
			else if (instance.IsJustPressed(InputFunction.TabRight))
			{
				NextPage();
			}
		}
	}
}
