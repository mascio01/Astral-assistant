using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoragePolicyDialog : BaseDialog
{
	public Prop Prop;

	private RawImage UnityIcon;

	private TextMeshProUGUI UnityTitle;

	public GameObject UnityEquipmentTotalsPanel;

	public GridLayoutGroup UnityEquipmentTotalsContents;

	public List<EquipmentTotalBehaviour> EquipmentTotalBehaviours = new List<EquipmentTotalBehaviour>();

	private GameObject[] UnityEquipmentSortByButton = new GameObject[6];

	private GameObject[] UnityEquipmentSortOrderButton = new GameObject[2];

	private bool WantRepopulate;

	public ActionMenu ActionMenu = new ActionMenu();

	private List<bool> WasOnScreen = new List<bool>();

	private static string EquipmentStr = "Equipment";

	private int WantUpdateEquipmnentOnScreen;

	public override ActionMenu GetActionMenu()
	{
		return ActionMenu;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnityIcon = base.gameObject.FindChild("Icon").GetComponent<RawImage>();
		UnityTitle = base.gameObject.FindChild("Title").GetComponent<TextMeshProUGUI>();
		UnityEquipmentTotalsPanel = base.gameObject.FindChild("EquipmentTotalsPanel");
		UnityEquipmentTotalsContents = base.gameObject.FindChild("EquipmentTotalsPanel/Viewport/Content").GetComponent<GridLayoutGroup>();
		for (int i = 0; i < UnityEquipmentSortByButton.Length; i++)
		{
			GameObject[] unityEquipmentSortByButton = UnityEquipmentSortByButton;
			int num = i;
			Transform obj = base.transform;
			SortBy sortBy = (SortBy)i;
			unityEquipmentSortByButton[num] = obj.Find("EquipmentTitlePanel/SortBy" + sortBy.ToString() + "Button").gameObject;
		}
		for (int j = 0; j < UnityEquipmentSortOrderButton.Length; j++)
		{
			GameObject[] unityEquipmentSortOrderButton = UnityEquipmentSortOrderButton;
			int num2 = j;
			Transform obj2 = base.transform;
			SortOrder sortOrder = (SortOrder)j;
			unityEquipmentSortOrderButton[num2] = obj2.Find("EquipmentTitlePanel/SortOrder" + sortOrder.ToString() + "Button").gameObject;
		}
		WantOkCancelButtonPromptsForController = false;
		UpdateTitleAndIcon();
		Populate();
	}

	public void SetProp(Prop prop)
	{
		Prop = prop;
		WantRepopulate = true;
	}

	private void Populate()
	{
		WasOnScreen.Clear();
		foreach (EquipmentTotalBehaviour equipmentTotalBehaviour2 in EquipmentTotalBehaviours)
		{
			WasOnScreen.Add(equipmentTotalBehaviour2.OnScreen);
		}
		List<EquipmentTotalBehaviour> list = new List<EquipmentTotalBehaviour>();
		list.Capacity = GameImpl.Instance.CurrentEquipmentPrototypesDeterministic.Count + GameImpl.Instance.CurrentLiquidPrototypesDeterministic.Count;
		CommunityPage.AddAllEquipmentTotals(Prop.Community);
		if (Session.Instance.InventorySortBy == SortBy.Type)
		{
			CommunityPage.Temp2.Sort(CommunityPage.EquipmentSorterByCategory);
		}
		else
		{
			CommunityPage.Temp2.Sort(CommunityPage.EquipmentSorterByScore);
		}
		if (Session.Instance.InventorySortOrder == SortOrder.Descending)
		{
			CommunityPage.Temp2.Reverse();
		}
		for (int i = 0; i < CommunityPage.Temp2.Count; i++)
		{
			EquipmentPrototype proto = CommunityPage.Temp2[i].Proto;
			LiquidPrototype liquid = CommunityPage.Temp2[i].Liquid;
			EquipmentTotalBehaviour equipmentTotalBehaviour = FindEquipmentTotalBehaviour(proto, liquid);
			if (equipmentTotalBehaviour == null)
			{
				equipmentTotalBehaviour = UnityEngine.Object.Instantiate(InfoScreen.EquipmentTotal.GetAsset(), UnityEquipmentTotalsContents.transform, worldPositionStays: false).GetComponent<EquipmentTotalBehaviour>();
				equipmentTotalBehaviour.Initialize(proto, liquid, checkOnScreen: true);
			}
			equipmentTotalBehaviour.gameObject.transform.SetSiblingIndex(list.Count);
			equipmentTotalBehaviour.SetAmount(CommunityPage.Temp2[i].Amount);
			if (i < WasOnScreen.Count)
			{
				equipmentTotalBehaviour.SetOnScreen(WasOnScreen[i]);
			}
			list.Add(equipmentTotalBehaviour);
		}
		for (int num = UnityEquipmentTotalsContents.transform.childCount - 1; num >= list.Count; num--)
		{
			UnityEngine.Object.Destroy(UnityEquipmentTotalsContents.transform.GetChild(num).gameObject);
		}
		list.CopyToList(EquipmentTotalBehaviours);
		foreach (EquipmentTotalBehaviour equipmentTotalBehaviour3 in EquipmentTotalBehaviours)
		{
			equipmentTotalBehaviour3.SettingStoragePolicyForProp = Prop;
		}
		WantUpdateEquipmnentOnScreen = 2;
		for (int j = 0; j < 6; j++)
		{
			UnityEquipmentSortByButton[j].SetActive(j == (int)Session.Instance.InventorySortBy);
		}
		for (int k = 0; k < 2; k++)
		{
			UnityEquipmentSortOrderButton[k].SetActive(k == (int)Session.Instance.InventorySortOrder);
		}
		CommunityPage.Temp2.Clear();
	}

	public void UpdateEquipmnentOnScreen()
	{
		using (new UnityProfileMarker(EquipmentStr))
		{
			RectTransform obj = (RectTransform)UnityEquipmentTotalsContents.transform;
			RectTransform rectTransform = (RectTransform)UnityEquipmentTotalsPanel.transform;
			float y = obj.localPosition.y;
			float num = y + rectTransform.rect.height;
			for (int i = 0; i < EquipmentTotalBehaviours.Count; i++)
			{
				RectTransform rectTransform2 = (RectTransform)EquipmentTotalBehaviours[i].transform;
				float num2 = 0f - rectTransform2.localPosition.y + rectTransform2.rect.height;
				float num3 = 0f - rectTransform2.localPosition.y;
				EquipmentTotalBehaviours[i].SetOnScreen(num2 >= y && num3 <= num);
			}
		}
	}

	private EquipmentTotalBehaviour FindEquipmentTotalBehaviour(EquipmentPrototype proto, LiquidPrototype liquid)
	{
		foreach (EquipmentTotalBehaviour equipmentTotalBehaviour in EquipmentTotalBehaviours)
		{
			if (equipmentTotalBehaviour.Proto == proto && equipmentTotalBehaviour.Liquid == liquid)
			{
				return equipmentTotalBehaviour;
			}
		}
		return null;
	}

	private bool CommunityHasMultiplePropsThatCanHaveStoragePolicy()
	{
		int num = 0;
		foreach (Prop building in Prop.Community.Buildings)
		{
			if (building.CanSetStoragePolicy())
			{
				num++;
				if (num > 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateTitleAndIcon()
	{
		BaseDialog.sb.Length = 0;
		Prop.BuildDisplayName(BaseDialog.sb, noStrangers: false, englishOnly: false);
		if (CommunityHasMultiplePropsThatCanHaveStoragePolicy())
		{
			BaseDialog.sb.Append(' ');
			BaseDialog.sb.AppendButtonPromptString(InputFunction.Control);
		}
		UnityTitle.SetUnityTextIfDifferent(BaseDialog.sb);
		UnityIcon.texture = Prop.GetIcon(out var _, out var col, highlighted: false);
		UnityIcon.color = ((UnityIcon.texture != null) ? col : MathUtil.TransparentBlackCol);
	}

	public override void DialogUpdate()
	{
		base.DialogUpdate();
		UpdateTitleAndIcon();
		if (WantRepopulate)
		{
			Populate();
			WantRepopulate = false;
		}
		if (WantUpdateEquipmnentOnScreen > 0)
		{
			WantUpdateEquipmnentOnScreen--;
			if (WantUpdateEquipmnentOnScreen == 0)
			{
				UpdateEquipmnentOnScreen();
			}
		}
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		ActionMenu.ClearActions();
		EventSystem current = EventSystem.current;
		if (current != null && current.currentSelectedGameObject != null && (SelectableBehaviour.CurSelectionMode != SelectableBehaviour.SelectionMode.Cursor || SelectableBehaviour.CurrentCursorHovered != null))
		{
			EquipmentTotalBehaviour component = current.currentSelectedGameObject.GetComponent<EquipmentTotalBehaviour>();
			if (component != null)
			{
				GameCursor.AddEquipmentTotalActions(component, Prop.Community, null, Prop, null, ActionMenu.HeaderActions, ActionMenu.AvailableActions);
			}
		}
		ActionMenu.OnFinishAddingActions();
		ActionMenu.HandleInputForMenuActions(inputFrame);
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		if (ActionMenu.GetCursorAction() != CursorAction.None && instance2.IsJustPressed(InputFunction.MainAction) && inputFrame != null)
		{
			AvailableAction availableAction = ActionMenu.GetAvailableAction();
			switch (availableAction.ActionType)
			{
			case CursorAction.StoreHere:
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				inputFrame.AddAction(InputAction.SetStoragePolicy(Prop, availableAction.Proto, availableAction.Liquid, on: true));
				break;
			case CursorAction.DontStoreHere:
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				inputFrame.AddAction(InputAction.SetStoragePolicy(Prop, availableAction.Proto, availableAction.Liquid, on: false));
				break;
			case CursorAction.LocateProp:
				if (availableAction.Enabled == CursorActionDisabledReason.Enabled)
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					SetProp((Prop)availableAction.Object);
				}
				else
				{
					SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
				}
				break;
			}
		}
		if (CommunityHasMultiplePropsThatCanHaveStoragePolicy())
		{
			float axis = instance2.GetAxis(InputFunction.Control);
			if (axis != 0f)
			{
				instance2.Capture(InputFunction.Control, untilReleased: true);
				List<Prop> list = new List<Prop>();
				foreach (Prop building in Prop.Community.Buildings)
				{
					if (building.CanSetStoragePolicy())
					{
						list.Add(building);
					}
				}
				int num = Math.Max(list.IndexOf(Prop), 0);
				if (list.Count > 0)
				{
					Prop prop = list[(num + list.Count + (int)Mathf.Sign(axis)) % list.Count];
					SetProp(prop);
				}
			}
		}
		CursorAction cursorAction = (CursorAction)((instance.InventorySortBy == SortBy.Time) ? ((SortBy)10) : (3 + instance.InventorySortBy));
		if (instance2.IsJustPressed(InputFunction.InventorySortBy, capture: true, AvailableAction.Caption[(int)cursorAction]))
		{
			InventoryBehaviour.OnSortByPressedStatic();
			WantRepopulate = true;
		}
		if (instance2.IsJustPressed(InputFunction.InventorySortOrder, capture: true, AvailableAction.Caption[(int)(1 + instance.InventorySortOrder)]))
		{
			InventoryBehaviour.OnSortOrderPressedStatic();
			WantRepopulate = true;
		}
		if (OKSelected)
		{
			OKSelected = false;
			Finished = true;
		}
	}

	public void OnEquipmentSortByPressed()
	{
		InventoryBehaviour.OnSortByPressedStatic();
		WantRepopulate = true;
	}

	public void OnEquipmentSortOrderPressed()
	{
		InventoryBehaviour.OnSortOrderPressedStatic();
		WantRepopulate = true;
	}

	public void OnEquipmentTotalsScrollRectChanged(Vector2 scrollPos)
	{
		UpdateEquipmnentOnScreen();
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
