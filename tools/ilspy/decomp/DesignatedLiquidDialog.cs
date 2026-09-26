using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DesignatedLiquidDialog : BaseDialog
{
	public TileObject Carrier;

	public Equipment Item;

	private RawImage UnityIcon;

	private TextMeshProUGUI UnityTitle;

	public GridLayoutGroup UnityEquipmentTotalsContents;

	public List<EquipmentTotalBehaviour> EquipmentTotalBehaviours = new List<EquipmentTotalBehaviour>();

	private bool WantRepopulate;

	public ActionMenu ActionMenu = new ActionMenu();

	public override ActionMenu GetActionMenu()
	{
		return ActionMenu;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnityIcon = base.gameObject.FindChild("Icon").GetComponent<RawImage>();
		UnityTitle = base.gameObject.FindChild("Title").GetComponent<TextMeshProUGUI>();
		UnityEquipmentTotalsContents = base.gameObject.FindChild("EquipmentTotalsPanel/Viewport/Content").GetComponent<GridLayoutGroup>();
		string str = GameImpl.Translate("HUD_SetDesignatedLiquid").Replace("%1", Item.GetDisplayNameString());
		str = StringUtil.ApplyFormulae(str, null, Item);
		UnityTitle.SetUnityText(str);
		WantOkCancelButtonPromptsForController = false;
		Populate();
	}

	private void Populate()
	{
		CommunityPage.Temp2.Clear();
		Community community = Carrier.GetCommunity();
		List<EquipmentTotalBehaviour> list = new List<EquipmentTotalBehaviour>();
		list.Capacity = GameImpl.Instance.CurrentEquipmentPrototypesDeterministic.Count + GameImpl.Instance.CurrentLiquidPrototypesDeterministic.Count;
		foreach (KeyValuePair<string, LiquidPrototype> item in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
		{
			LiquidPrototype value = item.Value;
			bool flag = value.Discovered;
			if (!flag)
			{
				foreach (KeyValuePair<string, Recipe> item2 in GameImpl.Instance.CurrentRecipesDeterministic)
				{
					if (item2.Value.ProductLiquidPrototype == value && !item2.Value.Deprecated && item2.Value.IsDiscovered())
					{
						flag = true;
						break;
					}
				}
			}
			if (flag && (!Item.IsBottle() || value.CanPourIntoBottles))
			{
				float totalLiquid = community.GetTotalLiquid(value);
				CommunityPage.Temp2.Add(new CommunityPage.EquipmentScore(null, value, totalLiquid, totalLiquid));
			}
		}
		CommunityPage.Temp2.Sort(CommunityPage.EquipmentSorterByScore);
		for (int i = 0; i < CommunityPage.Temp2.Count; i++)
		{
			EquipmentPrototype proto = CommunityPage.Temp2[i].Proto;
			LiquidPrototype liquid = CommunityPage.Temp2[i].Liquid;
			EquipmentTotalBehaviour equipmentTotalBehaviour = FindEquipmentTotalBehaviour(proto, liquid);
			if (equipmentTotalBehaviour == null)
			{
				equipmentTotalBehaviour = Object.Instantiate(InfoScreen.EquipmentTotal.GetAsset(), UnityEquipmentTotalsContents.transform, worldPositionStays: false).GetComponent<EquipmentTotalBehaviour>();
				equipmentTotalBehaviour.Initialize(proto, liquid);
			}
			equipmentTotalBehaviour.gameObject.transform.SetSiblingIndex(list.Count);
			equipmentTotalBehaviour.SetAmount(CommunityPage.Temp2[i].Amount);
			list.Add(equipmentTotalBehaviour);
		}
		for (int num = UnityEquipmentTotalsContents.transform.childCount - 1; num >= list.Count; num--)
		{
			Object.Destroy(UnityEquipmentTotalsContents.transform.GetChild(num).gameObject);
		}
		list.CopyToList(EquipmentTotalBehaviours);
		foreach (EquipmentTotalBehaviour equipmentTotalBehaviour2 in EquipmentTotalBehaviours)
		{
			equipmentTotalBehaviour2.DesignatingLiquidForItem = Item;
		}
		CommunityPage.Temp2.Clear();
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

	public override void DialogUpdate()
	{
		base.DialogUpdate();
		UnityIcon.texture = Item.GetIcon(out var _, out var col, highlighted: false);
		UnityIcon.color = col;
		if (WantRepopulate)
		{
			Populate();
			WantRepopulate = false;
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
				GameCursor.AddEquipmentTotalActions(component, Carrier.GetCommunity(), null, null, Item, ActionMenu.HeaderActions, ActionMenu.AvailableActions);
			}
		}
		ActionMenu.OnFinishAddingActions();
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (ActionMenu.GetCursorAction() != CursorAction.None && instance.IsJustPressed(InputFunction.MainAction))
		{
			AvailableAction availableAction = ActionMenu.GetAvailableAction();
			switch (availableAction.ActionType)
			{
			case CursorAction.SelectLiquid:
				inputFrame.AddAction(InputAction.SetDesignatedLiquid(Item, availableAction.Liquid));
				break;
			case CursorAction.DeselectLiquid:
				inputFrame.AddAction(InputAction.SetDesignatedLiquid(Item, null));
				break;
			}
		}
		if (OKSelected)
		{
			OKSelected = false;
			Finished = true;
		}
	}

	public override bool NeedsSession()
	{
		return true;
	}
}
