using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionMenu
{
	public GameObject FocusUnityObj;

	public List<AvailableAction> HeaderActions = new List<AvailableAction>();

	public List<AvailableAction> AvailableActions = new List<AvailableAction>();

	public int SelectedAction;

	public float DesiredWidth;

	protected float LastDPadTime;

	protected int DPadHeldDown;

	public CursorAction GetCursorAction()
	{
		if (AvailableActions.Count <= 0)
		{
			return CursorAction.None;
		}
		return AvailableActions[SelectedAction].ActionType;
	}

	public Speech GetCursorSpeech()
	{
		if (AvailableActions.Count <= 0)
		{
			return null;
		}
		return AvailableActions[SelectedAction].Speech;
	}

	public bool HasAnyVisibleActions()
	{
		if (InfoScreen.Instance.Active && InfoScreen.Instance.HideActionMenu)
		{
			return false;
		}
		if (HeaderActions.Count <= 0)
		{
			return AvailableActions.Count > 0;
		}
		return true;
	}

	public bool IsCursorActionEnabled()
	{
		if (AvailableActions.Count <= 0)
		{
			return false;
		}
		return AvailableActions[SelectedAction].Enabled == CursorActionDisabledReason.Enabled;
	}

	public bool WantSpeechBubbleMenu()
	{
		for (int i = 0; i < AvailableActions.Count; i++)
		{
			if (AvailableActions[i].Speech != null)
			{
				return true;
			}
		}
		return false;
	}

	public AvailableAction GetAvailableAction()
	{
		if (AvailableActions.Count <= 0)
		{
			return default(AvailableAction);
		}
		return AvailableActions[SelectedAction];
	}

	public int FindAvailableActionByType(CursorAction actionType)
	{
		for (int i = 0; i < AvailableActions.Count; i++)
		{
			if (AvailableActions[i].ActionType == actionType)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsTooltipMenu(out float offsetX, out bool leftAligned)
	{
		for (int i = 0; i < HeaderActions.Count; i++)
		{
			if (HeaderActions[i].ActionType == CursorAction.Tooltip)
			{
				offsetX = HeaderActions[i].FloatAmount;
				leftAligned = HeaderActions[i].Amount < 0;
				return true;
			}
		}
		offsetX = 0f;
		leftAligned = false;
		return false;
	}

	public bool ActionRequiresHold()
	{
		if (!IsCursorActionEnabled())
		{
			return false;
		}
		switch (GetCursorAction())
		{
		case CursorAction.LoadNewMap:
		case CursorAction.Slaughter:
		case CursorAction.Steal:
		case CursorAction.StealEmpty:
		case CursorAction.StealNotInvestigated:
		case CursorAction.StealAll:
		case CursorAction.PickUpSteal:
		case CursorAction.GrabSteal:
		case CursorAction.Demolish:
		case CursorAction.Abandon:
		case CursorAction.StealCrops:
		case CursorAction.ClearCrops:
		case CursorAction.ChopTreeStealing:
		case CursorAction.ChopLogStealing:
		case CursorAction.MineStealing:
		case CursorAction.StealFromPot:
			return true;
		case CursorAction.BludgeonUnconscious:
		case CursorAction.KillUnconscious:
			if (AvailableActions[SelectedAction].Object is Character character)
			{
				return !character.Zombie;
			}
			return false;
		case CursorAction.TalkTo:
		{
			Speech cursorSpeech = GetCursorSpeech();
			if (cursorSpeech != null)
			{
				if (cursorSpeech.Situation != SpeechSituation.Threat)
				{
					return cursorSpeech.Situation == SpeechSituation.KickFromCommunity;
				}
				return true;
			}
			return false;
		}
		case CursorAction.TakeAll:
		{
			TileObject tileObject = AvailableActions[SelectedAction].Target as TileObject;
			EquipmentContainer inventory = tileObject.GetInventory();
			int num = 0;
			foreach (Equipment content in inventory.Contents)
			{
				if (content.IsIncludedInTakeAll(tileObject))
				{
					num++;
					if (num >= 5)
					{
						return true;
					}
				}
			}
			return false;
		}
		default:
			return false;
		}
	}

	public bool HasAltAction()
	{
		CursorAction cursorAction = GetCursorAction();
		if ((uint)(cursorAction - 226) <= 4u)
		{
			return true;
		}
		return false;
	}

	public int GetFirstSelectableActionIndex()
	{
		for (int i = 0; i < AvailableActions.Count; i++)
		{
			if (AvailableActions[i].IsSelectable())
			{
				return i;
			}
		}
		return 0;
	}

	public void ClearActions()
	{
		FocusUnityObj = null;
		HeaderActions.Clear();
		AvailableActions.Clear();
	}

	public void OnFinishAddingActions()
	{
		SelectedAction = MathUtil.Clamp(SelectedAction, 0, AvailableActions.Count - 1);
	}

	public void HandleInputForMenuActions(InputFrame inputFrame)
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (AvailableActions.Count > 0)
		{
			AvailableAction availableAction = GetAvailableAction();
			SelectedAction = Math.Min(SelectedAction, AvailableActions.Count - 1);
			while (!AvailableActions[SelectedAction].IsSelectable() && SelectedAction < AvailableActions.Count - 1)
			{
				SelectedAction++;
			}
			if (AvailableActions[SelectedAction].ActionType != availableAction.ActionType)
			{
				for (int i = 0; i < AvailableActions.Count; i++)
				{
					if (AvailableActions[i].ActionType == availableAction.ActionType && AvailableActions[i].IsSelectable())
					{
						SelectedAction = i;
						break;
					}
				}
			}
		}
		else
		{
			SelectedAction = 0;
		}
		if (AvailableActions.Count <= 1)
		{
			return;
		}
		float num = 0f;
		float axis = instance.GetAxis(InputFunction.SelectAction);
		if (axis != 0f)
		{
			float num2 = (instance.IsMappedToMouseWheel(InputFunction.SelectAction) ? 0f : (((DPadHeldDown == 0) ? 0f : ((DPadHeldDown == 1) ? 10f : 4f)) / 60f));
			if (GameImpl.UnscaledTime - LastDPadTime >= num2)
			{
				num += axis;
				LastDPadTime = GameImpl.UnscaledTime;
				DPadHeldDown++;
			}
		}
		else
		{
			DPadHeldDown = 0;
		}
		if (num < 0f)
		{
			while (SelectedAction < AvailableActions.Count - 1)
			{
				SelectedAction++;
				if (GetAvailableAction().IsSelectable())
				{
					break;
				}
			}
		}
		else
		{
			if (!(num > 0f))
			{
				return;
			}
			while (SelectedAction > GetFirstSelectableActionIndex())
			{
				SelectedAction--;
				if (GetAvailableAction().IsSelectable())
				{
					break;
				}
			}
		}
	}

	public void AddHeaderActionsForEquipment(Character controlledCharacter, TileObject carrier, Equipment item, TileObject destObj, SwappingSuppliesMode swappingSuppliesMode)
	{
		Character character = carrier as Character;
		HeaderActions.Add(new AvailableAction(CursorAction.EquipmentName, character, destObj, item, CursorActionDisabledReason.Enabled));
		if ((item is WorldMap || item is GeologicalMap) && GameTerrain.Instance != null && GameTerrain.Instance.Size >= 1024)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.MapQuadrants, controlledCharacter, destObj, item, CursorActionDisabledReason.Enabled));
		}
		CursorAction actionType = ((swappingSuppliesMode == SwappingSuppliesMode.Trading) ? CursorAction.TradePriceAndWeight : ((swappingSuppliesMode == SwappingSuppliesMode.SellingFood && controlledCharacter == carrier) ? CursorAction.SalePriceAndWeight : ((swappingSuppliesMode == SwappingSuppliesMode.SellingFoodMarkedUp && controlledCharacter == carrier) ? CursorAction.MarkedUpSalePriceAndWeight : CursorAction.BasePriceAndWeight)));
		HeaderActions.Add(new AvailableAction(actionType, character, destObj, item, CursorActionDisabledReason.Enabled));
		if (item is AmmoWeapon)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.LoadedAmmo, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item.GetInsulation() != 0)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.Insulation, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item.GetSightRangeModifierWhenEquipped() != 0)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.SightBonus, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item.GetPrototype().SkillBonus != 0)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.SkillBonus, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item.GetPrototype().SkillOnConsumptionProgression != 0f || (item.GetLiquidContentsType() != null && item.GetLiquidContentsType().SkillOnConsumptionProgressionPerFlOz != 0f))
		{
			HeaderActions.Add(new AvailableAction(CursorAction.SkillOnConsumption, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item is Weapon && item.GetDamageIncludingEffects(character) > 0f)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.WeaponStats, (controlledCharacter != null) ? controlledCharacter : character, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if ((item.IsEdible() || item.IsDrinkable()) && (item.GetNutrition() != 0f || item.GetAlcoholContent() != 0f || (controlledCharacter?.CalcTastiness(item) ?? item.GetTastiness()) != 0f))
		{
			HeaderActions.Add(new AvailableAction(CursorAction.TastinessStat, controlledCharacter, destObj, item, CursorActionDisabledReason.Enabled));
		}
		if (item.GetPrototype().DescriptionHash != 0)
		{
			HeaderActions.Add(new AvailableAction(CursorAction.EquipmentDescription, character, destObj, item, CursorActionDisabledReason.Enabled));
		}
	}
}
