using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradePage : InfoPage
{
	public static TradePage Instance;

	public Character TakeFrom;

	public Character Taker;

	public SwappingSuppliesMode Mode;

	public List<PendingTrade> PendingTrades = new List<PendingTrade>();

	private InventoryBehaviour UnityTakerInventory;

	private InventoryBehaviour UnityTakeFromInventory;

	private RawImage UnityTakerGold;

	private RawImage UnityTakeFromGold;

	private TextMeshProUGUI UnityTakerGoldAmount;

	private TextMeshProUGUI UnityTakeFromGoldAmount;

	private GameObject UnityBasket;

	private GameObject UnityArrowBuy;

	private GameObject UnityArrowSell;

	private GameObject UnityBasketButtons;

	private List<BasketItemBehaviour> UnityBuyingItems = new List<BasketItemBehaviour>();

	private List<BasketItemBehaviour> UnitySellingItems = new List<BasketItemBehaviour>();

	private bool First;

	private static string Ellipsis = "...";

	private static int INFOPAGE_Trade = StringUtil.JenkinsHash("INFOPAGE_Trade");

	private bool ResetClicked;

	private bool ConfirmClicked;

	public override void OnAwake()
	{
		Instance = this;
		UnityTakerInventory = base.gameObject.transform.Find("TakerInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityTakeFromInventory = base.gameObject.transform.Find("TakeFromInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityTakerGold = base.gameObject.FindChild("TakerGold").GetComponent<RawImage>();
		UnityTakeFromGold = base.gameObject.FindChild("TakeFromGold").GetComponent<RawImage>();
		UnityTakerGoldAmount = base.gameObject.FindChild("TakerGoldAmount").GetComponent<TextMeshProUGUI>();
		UnityTakeFromGoldAmount = base.gameObject.FindChild("TakeFromGoldAmount").GetComponent<TextMeshProUGUI>();
		UnityBasket = base.gameObject.FindChild("Basket");
		UnityBasket.DeleteAllChildren();
	}

	public TradePage Initialize(Character takeFrom, Character taker, SwappingSuppliesMode mode)
	{
		TakeFrom = takeFrom;
		Taker = taker;
		Mode = mode;
		PendingTrades.Clear();
		UnityTakerInventory.Initialize(taker, taker.GetDisplayNameString(noStrangers: false, englishOnly: false));
		UnityTakeFromInventory.Initialize(takeFrom, takeFrom.GetDisplayNameString(noStrangers: false, englishOnly: false));
		UnityTakerInventory.TradingWith = TakeFrom;
		UnityTakeFromInventory.TradingWith = Taker;
		if (EquipmentPrototype.Gold != null && EquipmentPrototype.Gold.Tex != null)
		{
			UnityTakerGold.texture = (Texture2D)EquipmentPrototype.Gold.Tex;
			UnityTakeFromGold.texture = (Texture2D)EquipmentPrototype.Gold.Tex;
		}
		First = true;
		return this;
	}

	public override void OnDeactivate()
	{
		PendingTrades.Clear();
		base.OnDeactivate();
	}

	public override void Populate()
	{
		UnityTakerInventory.Populate(Mode, this);
		UnityTakeFromInventory.Populate(Mode, this);
		bool invalidTrade;
		int num = CalcTotalGoldPlayerPays(out invalidTrade);
		int num2 = Taker.Inventory.GetGoldAmount() - num;
		int num3 = TakeFrom.Inventory.GetGoldAmount() + num;
		UnityTakerGoldAmount.SetUnityText(num2.ToString());
		UnityTakerGoldAmount.color = ((num2 < 0) ? Color.red : Color.black);
		UnityTakeFromGoldAmount.SetUnityText(num3.ToString());
		UnityTakeFromGoldAmount.color = ((num3 < 0) ? Color.red : Color.black);
		int num4 = CountPendingTradesOfType(selling: false);
		int num5 = CountPendingTradesOfType(selling: true);
		int i = 0;
		int num6 = 0;
		if (num < 0 || num4 > 0)
		{
			if (UnityArrowBuy == null)
			{
				UnityArrowBuy = UnityEngine.Object.Instantiate((UnityEngine.Object)(GameObject)InfoScreen.ArrowBuy, UnityBasket.transform) as GameObject;
			}
			UnityArrowBuy.transform.SetSiblingIndex(i++);
			int columnIndex = 0;
			if (num < 0 && EquipmentPrototype.Gold != null && EquipmentPrototype.Gold.Tex != null)
			{
				AddBasketItem(-1, EquipmentPrototype.Gold.Tex, Hud.OutlineBlackMat, Color.white, -num, null, UnityBuyingItems, ref columnIndex, ref i);
			}
			for (int j = 0; j < PendingTrades.Count; j++)
			{
				if (!PendingTrades[j].Selling)
				{
					if (PendingTrades.Count > 10 && num6 >= Math.Max(5, 10 - num5))
					{
						AddBasketItem(-1, null, null, Color.white, -1, null, UnityBuyingItems, ref columnIndex, ref i);
						break;
					}
					Material mat;
					Color col;
					Texture2D icon = PendingTrades[j].Item.GetIcon(out mat, out col, highlighted: false);
					AddBasketItem(j, icon, mat, col, PendingTrades[j].Amount, PendingTrades[j].Item.GetLiquidContentsType(), UnityBuyingItems, ref columnIndex, ref i);
					num6++;
				}
			}
		}
		if (num > 0 || num5 > 0)
		{
			if (UnityArrowSell == null)
			{
				UnityArrowSell = UnityEngine.Object.Instantiate((UnityEngine.Object)(GameObject)InfoScreen.ArrowSell, UnityBasket.transform) as GameObject;
			}
			UnityArrowSell.transform.SetSiblingIndex(i++);
			int columnIndex2 = 0;
			if (num > 0 && EquipmentPrototype.Gold != null && EquipmentPrototype.Gold.Tex != null)
			{
				AddBasketItem(-1, EquipmentPrototype.Gold.Tex, Hud.OutlineBlackMat, Color.white, num, null, UnitySellingItems, ref columnIndex2, ref i);
			}
			for (int k = 0; k < PendingTrades.Count; k++)
			{
				if (PendingTrades[k].Selling)
				{
					if (PendingTrades.Count > 10 && num6 >= 10)
					{
						AddBasketItem(-1, null, null, Color.white, -1, null, UnitySellingItems, ref columnIndex2, ref i);
						break;
					}
					Material mat2;
					Color col2;
					Texture2D icon2 = PendingTrades[k].Item.GetIcon(out mat2, out col2, highlighted: false);
					AddBasketItem(k, icon2, mat2, col2, PendingTrades[k].Amount, PendingTrades[k].Item.GetLiquidContentsType(), UnitySellingItems, ref columnIndex2, ref i);
					num6++;
				}
			}
		}
		if (PendingTrades.Count > 0)
		{
			if (UnityBasketButtons == null)
			{
				UnityBasketButtons = UnityEngine.Object.Instantiate((UnityEngine.Object)(GameObject)InfoScreen.BasketButtons, UnityBasket.transform) as GameObject;
				UnityBasketButtons.transform.GetChild(0).GetComponent<CloseIconBehaviour>().OnClicked.RemoveAllListeners();
				UnityBasketButtons.transform.GetChild(0).GetComponent<CloseIconBehaviour>().OnClicked.AddListener(OnResetClicked);
				UnityBasketButtons.transform.GetChild(1).GetComponent<CloseIconBehaviour>().OnClicked.RemoveAllListeners();
				UnityBasketButtons.transform.GetChild(1).GetComponent<CloseIconBehaviour>().OnClicked.AddListener(OnConfirmClicked);
			}
			UnityBasketButtons.transform.SetSiblingIndex(i++);
		}
		for (; i < UnityBasket.transform.childCount; i++)
		{
			GameObject gameObject = UnityBasket.transform.GetChild(i).gameObject;
			if (UnityArrowSell == gameObject)
			{
				UnityArrowSell = null;
			}
			else if (UnityArrowBuy == gameObject)
			{
				UnityArrowBuy = null;
			}
			else
			{
				BasketItemBehaviour component = gameObject.GetComponent<BasketItemBehaviour>();
				UnitySellingItems.Remove(component);
				UnityBuyingItems.Remove(component);
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		if (First)
		{
			UnityEventSystem.firstSelectedGameObject = ((UnityTakeFromInventory.UnityItems.Count > 0) ? UnityTakeFromInventory.UnityItems[0].gameObject : ((UnityTakerInventory.UnityItems.Count > 0) ? UnityTakerInventory.UnityItems[0].gameObject : null));
			First = false;
		}
	}

	private void AddBasketItem(int pendingTradeIndex, Texture2D tex, Material mat, Color col, int amount, LiquidPrototype liquid, List<BasketItemBehaviour> column, ref int columnIndex, ref int index)
	{
		BasketItemBehaviour basketItemBehaviour;
		if (columnIndex < column.Count)
		{
			basketItemBehaviour = column[columnIndex];
		}
		else
		{
			basketItemBehaviour = UnityEngine.Object.Instantiate((GameObject)InfoScreen.BasketItem, UnityBasket.transform).GetComponent<BasketItemBehaviour>();
			column.Add(basketItemBehaviour);
		}
		columnIndex++;
		basketItemBehaviour.Initialize(tex, liquid, (amount < 0) ? Ellipsis : amount.ToString(), pendingTradeIndex);
		basketItemBehaviour.transform.SetSiblingIndex(index++);
	}

	public override EquipmentBehaviour FindUnityItem(Equipment item, TileObject carrier)
	{
		if (carrier == null || carrier == Taker)
		{
			EquipmentBehaviour equipmentBehaviour = UnityTakerInventory.FindUnityItem(item);
			if (equipmentBehaviour != null)
			{
				return equipmentBehaviour;
			}
		}
		if (carrier == null || carrier == TakeFrom)
		{
			EquipmentBehaviour equipmentBehaviour2 = UnityTakeFromInventory.FindUnityItem(item);
			if (equipmentBehaviour2 != null)
			{
				return equipmentBehaviour2;
			}
		}
		return null;
	}

	public override bool HasInventory()
	{
		return true;
	}

	public override bool IsShowingInventoryFor(TileObject obj)
	{
		if (Taker != obj)
		{
			return TakeFrom == obj;
		}
		return true;
	}

	public override TileObject GetOther(TileObject obj)
	{
		if (obj != Taker)
		{
			return Taker;
		}
		return TakeFrom;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Trade));
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (PendingTrades.Count > 0)
		{
			InputFunctionManager instance = InputFunctionManager.Instance;
			if (instance.IsJustPressed(InputFunction.TradeReset, capture: true, ButtonPromptBarBehaviour.PROMPT_TradeReset) || ResetClicked)
			{
				SoundManager.PlayMenuSound(SoundManager.CancelSound);
				CancelTrade(inputFrame);
				ResetClicked = false;
			}
			if (instance.IsJustPressed(InputFunction.TradeConfirm, capture: true, ButtonPromptBarBehaviour.PROMPT_TradeConfirm) || ConfirmClicked)
			{
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				ConfirmTrade(inputFrame);
				ConfirmClicked = false;
			}
		}
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (UnityTakerInventory.UnityItems.Count > 0 && UnityTakeFromInventory.UnityItems.Count > 0 && !GameImpl.Instance.IsDialogOpen())
		{
			float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MenuSwitchInventory);
			if (axis != 0f)
			{
				InventoryBehaviour inventoryBehaviour = ((axis < 0f) ? UnityTakerInventory : UnityTakeFromInventory);
				EquipmentBehaviour equipmentBehaviour = ((inventoryBehaviour.LastSelectedItem != null) ? inventoryBehaviour.LastSelectedItem : inventoryBehaviour.UnityItems[0]);
				UnityEventSystem.SetSelectedGameObject(equipmentBehaviour.gameObject);
				SelectableBehaviour.MoveCursorToButton(equipmentBehaviour);
			}
		}
	}

	public void OnResetClicked()
	{
		ResetClicked = true;
	}

	public void OnConfirmClicked()
	{
		ConfirmClicked = true;
	}

	public bool ConfirmTrade(InputFrame inputFrame, bool closeOnTrade = false)
	{
		int numberOfItemsInPendingTrades = GetNumberOfItemsInPendingTrades();
		bool invalidTrade;
		int num = CalcTotalGoldPlayerPays(out invalidTrade);
		if (num > Taker.Inventory.GetGoldAmount())
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			string str = GameImpl.Translate((numberOfItemsInPendingTrades > 1) ? "HUD_CantAffordPlural" : "HUD_CantAfford").Replace("%1", Taker.GetDisplayNameString());
			str = StringUtil.ApplyFormulae(str, null, Taker);
			GameImpl.Instance.ShowMessageBox(str);
			return false;
		}
		if (UnityTakerInventory.Weight > UnityTakerInventory.MaxWeight && UnityTakerInventory.Weight / UnityTakerInventory.MaxWeight > Taker.Inventory.GetWeight(Taker) / Taker.GetMaxInventoryWeight())
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			string str2 = GameImpl.Translate((numberOfItemsInPendingTrades > 1) ? "HUD_TooHeavyForCharacterPlural" : "HUD_TooHeavyForCharacter").Replace("%1", Taker.GetDisplayNameString());
			str2 = StringUtil.ApplyFormulae(str2, null, Taker);
			GameImpl.Instance.ShowMessageBox(str2);
			return false;
		}
		if (UnityTakeFromInventory.Weight > UnityTakeFromInventory.MaxWeight && UnityTakeFromInventory.Weight / UnityTakeFromInventory.MaxWeight > TakeFrom.Inventory.GetWeight(TakeFrom) / TakeFrom.GetMaxInventoryWeight())
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			string str3 = GameImpl.Translate((numberOfItemsInPendingTrades > 1) ? "HUD_TooHeavyForCharacterPlural" : "HUD_TooHeavyForCharacter").Replace("%1", TakeFrom.GetDisplayNameString());
			str3 = StringUtil.ApplyFormulae(str3, null, TakeFrom);
			GameImpl.Instance.ShowMessageBox(str3);
			return false;
		}
		if (-num > TakeFrom.Inventory.GetGoldAmount() || invalidTrade)
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			string str4;
			if (invalidTrade)
			{
				str4 = GameImpl.Translate((numberOfItemsInPendingTrades > 1) ? "HUD_QuantityTooSmallToTradePlural" : "HUD_QuantityTooSmallToTrade");
				str4 = StringUtil.ApplyFormulae(str4);
			}
			else
			{
				str4 = GameImpl.Translate((numberOfItemsInPendingTrades > 1) ? "HUD_CantAffordPlural" : "HUD_CantAfford").Replace("%1", TakeFrom.GetDisplayNameString());
				str4 = StringUtil.ApplyFormulae(str4, null, TakeFrom);
			}
			GameImpl.Instance.ShowConfirmationBox(str4, "MENU_Continue", delegate(InputFrame inputFrame2)
			{
				inputFrame2.AddAction(InputAction.ConfirmTrade(PendingTrades, Taker, TakeFrom, evenIfNotEnoughGold: true));
				SoundManager.PlayMenuSoundFromList(SoundManager.CoinSounds, 4f);
				if (closeOnTrade)
				{
					InfoScreen.Instance.CloseInfoScreen();
				}
			}, needsSession: true);
			return false;
		}
		if (inputFrame != null)
		{
			inputFrame.AddAction(InputAction.ConfirmTrade(PendingTrades, Taker, TakeFrom, evenIfNotEnoughGold: false));
			SoundManager.PlayMenuSoundFromList(SoundManager.CoinSounds, 4f);
			return true;
		}
		return false;
	}

	public bool CancelTrade(InputFrame inputFrame)
	{
		PendingTrades.Clear();
		InfoScreen.Instance.WantRepopulate = true;
		if (EquipmentBehaviour.CurrentHovered != null)
		{
			InfoScreen.Instance.DesiredEquipmentToSelect = EquipmentBehaviour.CurrentHovered.Item;
		}
		return true;
	}

	public void AddPendingTrade(Equipment item, bool sell, int amount)
	{
		bool flag = false;
		for (int i = 0; i < PendingTrades.Count; i++)
		{
			if (PendingTrades[i].Item != item && !PendingTrades[i].Item.CanBeCombinedWith(item))
			{
				continue;
			}
			if (PendingTrades[i].Selling == sell)
			{
				PendingTrade value = PendingTrades[i];
				value.Amount += amount;
				PendingTrades[i] = value;
			}
			else
			{
				PendingTrade value2 = PendingTrades[i];
				value2.Amount -= amount;
				if (value2.Amount == 0)
				{
					PendingTrades.RemoveAt(i);
				}
				else if (value2.Amount < 0)
				{
					value2.Item = item;
					value2.ItemId = item.Id;
					value2.Amount = -value2.Amount;
					value2.Selling = !value2.Selling;
					PendingTrades[i] = value2;
				}
				else
				{
					PendingTrades[i] = value2;
				}
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			PendingTrade item2 = new PendingTrade
			{
				Item = item,
				ItemId = item.Id,
				Amount = amount,
				Selling = sell
			};
			PendingTrades.Add(item2);
		}
		InfoScreen instance = InfoScreen.Instance;
		instance.WantRepopulate = true;
		EquipmentBehaviour equipmentBehaviour = FindUnityItem(item, sell ? Taker : TakeFrom);
		if (!(equipmentBehaviour != null) || !(equipmentBehaviour.ParentInventoryBehaviour != null) || !(UnityEventSystem != null))
		{
			return;
		}
		if (GetAmountAfterPendingTrade(item, equipmentBehaviour.ParentInventoryBehaviour.Carrier) <= 0)
		{
			instance.DesiredEquipmentToSelect = equipmentBehaviour.ParentInventoryBehaviour.GetNextEquipmentToSelect(equipmentBehaviour);
			instance.DesiredEquipmentCarrierToSelect = equipmentBehaviour.ParentInventoryBehaviour.Carrier;
			if (instance.DesiredEquipmentToSelect == null)
			{
				instance.DesiredEquipmentToSelect = item;
				instance.DesiredEquipmentCarrierToSelect = null;
			}
		}
		else
		{
			instance.DesiredEquipmentToSelect = item;
			instance.DesiredEquipmentCarrierToSelect = equipmentBehaviour.ParentInventoryBehaviour.Carrier;
		}
	}

	public void RemovePendingTrade(int index)
	{
		if (index < PendingTrades.Count)
		{
			PendingTrades.RemoveAt(index);
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public void SubtractPendingTrade(int index, int amount)
	{
		if (index < PendingTrades.Count)
		{
			PendingTrade value = PendingTrades[index];
			value.Amount -= amount;
			if (value.Amount <= 0)
			{
				PendingTrades.RemoveAt(index);
			}
			else
			{
				PendingTrades[index] = value;
			}
			InfoScreen.Instance.WantRepopulate = true;
		}
	}

	public int GetAmountAfterPendingTrade(Equipment item, TileObject carrier)
	{
		int pendingTradeIndex;
		return GetAmountAfterPendingTrade(item, carrier, out pendingTradeIndex);
	}

	public int GetAmountAfterPendingTrade(Equipment item, TileObject carrier, out int pendingTradeIndex)
	{
		for (int i = 0; i < PendingTrades.Count; i++)
		{
			TileObject tileObject = (PendingTrades[i].Selling ? Taker : TakeFrom);
			if (PendingTrades[i].Item == item && carrier == tileObject)
			{
				pendingTradeIndex = i;
				return item.GetAmount() - PendingTrades[i].Amount;
			}
			if (PendingTrades[i].Item == item && carrier != tileObject)
			{
				pendingTradeIndex = i;
				return PendingTrades[i].Amount;
			}
			if (PendingTrades[i].Item.CanBeCombinedWith(item) && carrier != tileObject)
			{
				pendingTradeIndex = i;
				return item.GetAmount() + PendingTrades[i].Amount;
			}
		}
		pendingTradeIndex = -1;
		if (item.InventoryOwner != carrier)
		{
			return 0;
		}
		return item.GetAmount();
	}

	public int CalcTotalGoldPlayerPays(out bool invalidTrade)
	{
		return CalcTotalGoldPlayerPays(PendingTrades, Taker, TakeFrom, out invalidTrade);
	}

	public static int CalcTotalGoldPlayerPays(List<PendingTrade> pendingTrades, Character taker, Character takeFrom, out bool invalidTrade)
	{
		float num = 0f;
		bool flag = false;
		for (int i = 0; i < pendingTrades.Count; i++)
		{
			if (pendingTrades[i].Selling)
			{
				float inventoryItemPriceAdjustedForTrade = taker.GetInventoryItemPriceAdjustedForTrade(pendingTrades[i].Item, takeFrom, fromTradingScreen: true);
				num -= inventoryItemPriceAdjustedForTrade * (float)pendingTrades[i].Amount;
			}
			else
			{
				float inventoryItemPriceAdjustedForTrade2 = takeFrom.GetInventoryItemPriceAdjustedForTrade(pendingTrades[i].Item, taker, fromTradingScreen: true);
				num += inventoryItemPriceAdjustedForTrade2 * (float)pendingTrades[i].Amount;
				flag = true;
			}
		}
		if (num > 0f)
		{
			invalidTrade = false;
			return Mathf.CeilToInt(num);
		}
		if (num < 0f)
		{
			int num2 = (int)num;
			invalidTrade = num2 == 0 && !flag;
			return num2;
		}
		invalidTrade = false;
		return 0;
	}

	public int CountPendingTradesOfType(bool selling)
	{
		int num = 0;
		for (int i = 0; i < PendingTrades.Count; i++)
		{
			if (PendingTrades[i].Selling == selling)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumberOfItemsInPendingTrades()
	{
		int num = 0;
		for (int i = 0; i < PendingTrades.Count; i++)
		{
			num += PendingTrades[i].Amount;
		}
		return num;
	}

	public static CantTransferReason IsTradable(Equipment item, Character from, Character to, out int minTransferrable, out int maxTransferrable)
	{
		float num = from.GetInventoryItemPriceAdjustedForTrade(item, to, fromTradingScreen: true);
		if (num == 0f)
		{
			num = 1f;
		}
		minTransferrable = 1;
		if (from.IsControllableByPlayer())
		{
			minTransferrable = Math.Max(minTransferrable, (int)Math.Ceiling(1f / num));
		}
		maxTransferrable = item.GetAmount();
		if (to.IsControllableByPlayer())
		{
			maxTransferrable = Math.Min(maxTransferrable, item.GetMaxTransferrableTo(to));
			if (maxTransferrable <= 0)
			{
				return CantTransferReason.TooHeavy;
			}
			if ((int)(from.IsControllableByPlayer() ? Math.Floor(num * (float)maxTransferrable) : Math.Ceiling(num * (float)maxTransferrable)) <= 0)
			{
				return CantTransferReason.TooHeavy;
			}
		}
		int num2 = Math.Max(0, to.Inventory.GetGoldAmount() - to.GetReservedGoldAmount());
		maxTransferrable = Math.Min(maxTransferrable, from.IsControllableByPlayer() ? ((int)Math.Floor(((float)num2 + 0.999f) / num)) : ((int)Math.Floor((float)num2 / num)));
		if (maxTransferrable <= 0)
		{
			return CantTransferReason.CantAfford;
		}
		if ((int)(from.IsControllableByPlayer() ? Math.Floor(num * (float)maxTransferrable) : Math.Ceiling(num * (float)maxTransferrable)) <= 0)
		{
			return CantTransferReason.CantAfford;
		}
		if (item.GetAmount() < minTransferrable)
		{
			return CantTransferReason.QuantityTooSmallToTrade;
		}
		if (to.IsAmbient() && !item.CanBeDestroyed())
		{
			return CantTransferReason.CantTransferToAmbientCharacter;
		}
		return CantTransferReason.CanTransfer;
	}
}
