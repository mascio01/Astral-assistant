using System;
using System.Text;
using UnityEngine;

public class InfoPage : BaseTabPage
{
	protected int WantTransferAllInputFrame;

	public virtual bool HasInventory()
	{
		return false;
	}

	public virtual bool IsShowingInventoryFor(TileObject obj)
	{
		return false;
	}

	public virtual bool IsShowingBrainScanFor(Character character)
	{
		return false;
	}

	public virtual bool IsShowingTradeScreenFor(TileObject obj)
	{
		return false;
	}

	public virtual TileObject GetOther(TileObject obj)
	{
		return null;
	}

	public virtual EquipmentBehaviour FindUnityItem(Equipment item, TileObject carrier)
	{
		return null;
	}

	public bool TrySelectEquipment(Equipment item, TileObject carrier)
	{
		EquipmentBehaviour equipmentBehaviour = FindUnityItem(item, carrier);
		if (equipmentBehaviour != null && UnityEventSystem != null && UnityEventSystem.currentSelectedGameObject != null)
		{
			UnityEventSystem.SetSelectedGameObject(equipmentBehaviour.gameObject);
			SelectableBehaviour.StaticOnSelect(equipmentBehaviour, wantMoveCursorOnSelect: true);
			return true;
		}
		return false;
	}

	public bool CanShowEquipmentPolicy(Character controlledCharacter, TileObject carrier, Equipment item)
	{
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (carrier is CraftingProp craftingProp && craftingProp.IsCrafting())
		{
			return false;
		}
		if (controlledCharacter != null && controlledCharacter.Community != null)
		{
			return controlledCharacter.Community.CanUseRoleCommands();
		}
		return false;
	}

	public bool CanDeleteEquipment(TileObject carrier, Equipment item)
	{
		if (Session.Instance.Editor || InfoScreen.AllowViewInfoOnAnyone)
		{
			return true;
		}
		if (this is GatherPage)
		{
			return false;
		}
		if (this is TradePage)
		{
			return false;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (item != null && item.CanBeDestroyed())
		{
			return carrier.CanTransferEquipmentAway(item) == CantTransferReason.CanTransfer;
		}
		return false;
	}

	public bool CanPourAwayLiquid(TileObject carrier, Equipment item)
	{
		if (Session.Instance.Editor || InfoScreen.AllowViewInfoOnAnyone)
		{
			return true;
		}
		if (this is GatherPage)
		{
			return false;
		}
		if (this is TradePage)
		{
			return false;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (item.Gifted)
		{
			return false;
		}
		if (item != null)
		{
			return item.GetLiquidContentsAmount() > 0f;
		}
		return false;
	}

	public virtual bool CanToggleActionMenuVisible()
	{
		return true;
	}

	public virtual bool WantShowActionMenuOptionOnButtonPromptBar()
	{
		return true;
	}

	public virtual CantTransferReason CanDropEquipment(TileObject carrier, Equipment item, TileObject target)
	{
		if (item == null || target == null)
		{
			return CantTransferReason.NotOnCorrectPage;
		}
		CantTransferReason result = carrier.CanTransferEquipmentAway(item);
		if (target is Character character && character.IsAmbient() && !item.CanBeDestroyed())
		{
			return CantTransferReason.CantTransferToAmbientCharacter;
		}
		if (item.GetMaxTransferrableTo(target) == 0)
		{
			return CantTransferReason.TooHeavy;
		}
		return result;
	}

	public virtual CantTransferReason CanStartTransferEquipment(TileObject carrier, Equipment item, out TileObject to, out CursorAction cursorAction)
	{
		to = null;
		cursorAction = CursorAction.None;
		if (item == null)
		{
			return CantTransferReason.NotOnCorrectPage;
		}
		TradePage tradePage = this as TradePage;
		CantTransferReason result = carrier.CanTransferEquipmentAway(item, tradePage != null);
		CharacterPage characterPage = this as CharacterPage;
		if (characterPage != null)
		{
			if (characterPage.CurrentCharacter.InsideBuilding == null || characterPage.CurrentCharacter != carrier)
			{
				return CantTransferReason.NotOnCorrectPage;
			}
			to = characterPage.CurrentCharacter.InsideBuilding;
			cursorAction = CursorAction.Store;
			if (item.GetMaxTransferrableTo(to) == 0)
			{
				return CantTransferReason.TooHeavy;
			}
			return result;
		}
		BuildingPage buildingPage = this as BuildingPage;
		if (buildingPage != null && buildingPage.GetSelectedInhabitant() != null)
		{
			if (carrier == buildingPage.CurrentProp)
			{
				to = buildingPage.GetSelectedInhabitant();
				cursorAction = CursorAction.Take;
			}
			else
			{
				if (carrier != buildingPage.GetSelectedInhabitant())
				{
					return CantTransferReason.NotOnCorrectPage;
				}
				to = buildingPage.CurrentProp;
				cursorAction = CursorAction.Store;
			}
			if (to is Character character && character.IsAmbient() && !item.CanBeDestroyed())
			{
				return CantTransferReason.CantTransferToAmbientCharacter;
			}
			if (to == null)
			{
				return CantTransferReason.NotOnCorrectPage;
			}
			if (item.GetMaxTransferrableTo(to) == 0)
			{
				return CantTransferReason.TooHeavy;
			}
			return result;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null)
		{
			if (carrier == takePage.Taker)
			{
				if (takePage.TakeFrom is CraftingProp || takePage.TakeFrom is RabbitTrap)
				{
					return CantTransferReason.NotOnCorrectPage;
				}
				to = takePage.TakeFrom;
				cursorAction = ((to is Character) ? CursorAction.Give : CursorAction.Store);
			}
			else
			{
				if (carrier != takePage.TakeFrom)
				{
					return CantTransferReason.NotOnCorrectPage;
				}
				to = takePage.Taker;
				cursorAction = CursorAction.Take;
			}
			if (to is Character character2 && character2.IsAmbient() && !item.CanBeDestroyed())
			{
				return CantTransferReason.CantTransferToAmbientCharacter;
			}
			if (item.GetMaxTransferrableTo(to) == 0)
			{
				return CantTransferReason.TooHeavy;
			}
			return result;
		}
		if (tradePage != null)
		{
			if (carrier == tradePage.Taker)
			{
				to = tradePage.TakeFrom;
				cursorAction = CursorAction.Sell;
			}
			else
			{
				if (carrier != tradePage.TakeFrom)
				{
					return CantTransferReason.NotOnCorrectPage;
				}
				to = tradePage.Taker;
				cursorAction = CursorAction.Buy;
			}
			if (to is Character character3 && character3.IsAmbient() && !item.CanBeDestroyed())
			{
				return CantTransferReason.CantTransferToAmbientCharacter;
			}
			return result;
		}
		return CantTransferReason.NotOnCorrectPage;
	}

	public bool CanEquipOrWear(TileObject carrier, Equipment item)
	{
		if (this is TradePage || this is GatherPage)
		{
			return false;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (item != null)
		{
			if (Session.Instance.Editor || InfoScreen.AllowViewInfoOnAnyone)
			{
				return true;
			}
			if (carrier is Character character)
			{
				return character.Community == Session.Instance.CommunityManager.PlayerCommunity;
			}
			return false;
		}
		return false;
	}

	public bool CanUse(TileObject carrier, Equipment item)
	{
		if (this is TradePage || this is GatherPage)
		{
			return false;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (item != null)
		{
			if (Session.Instance.Editor || InfoScreen.AllowViewInfoOnAnyone)
			{
				return true;
			}
			if (carrier is Character character)
			{
				if (character.Community != Session.Instance.CommunityManager.PlayerCommunity)
				{
					return !character.IsAwake;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	public bool CanUnloadGun(TileObject carrier, Equipment item)
	{
		if (this is GatherPage)
		{
			return false;
		}
		TakePage takePage = this as TakePage;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			return false;
		}
		if (item != null && (!(carrier is Character character) || character.GetCommunity() == Session.Instance.CommunityManager.PlayerCommunity || !character.IsAwake || Session.Instance.Editor || InfoScreen.AllowViewInfoOnAnyone))
		{
			return item is AmmoWeapon;
		}
		return false;
	}

	public bool CanLightFuse(Character controlledCharacter, TileObject carrier, Equipment item)
	{
		if (item is PipeBomb { Fuse: 0f })
		{
			if (this is GatherPage || this is TradePage)
			{
				return false;
			}
			if (controlledCharacter == null || controlledCharacter == carrier || controlledCharacter.Community == carrier.GetCommunity())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool CanGather(TileObject carrier, Equipment item)
	{
		if (this is GatherPage && item != null)
		{
			return true;
		}
		return false;
	}

	public bool OnTransferEquipment(TileObject carrier, TileObject to, Equipment item, bool trading, bool transferAll, InputFrame inputFrame)
	{
		int minTransferrable = 1;
		int maxTransferrable = item.GetAmount();
		if (trading)
		{
			TradePage.IsTradable(item, carrier as Character, to as Character, out minTransferrable, out maxTransferrable);
		}
		maxTransferrable = Math.Min(maxTransferrable, item.GetMaxTransferrableTo(to));
		TakePage takePage = this as TakePage;
		bool flag = false;
		float detection = 0f;
		if (takePage != null && takePage.Mode == SwappingSuppliesMode.Pickpocketing)
		{
			maxTransferrable = 1;
			flag = true;
			detection = takePage.PickpocketItemDetection;
		}
		if (maxTransferrable <= 0)
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			StringBuilder stringBuilder = new StringBuilder(200);
			stringBuilder.Append((to is Character) ? GameImpl.Translate("HUD_TooHeavyForCharacter") : GameImpl.Translate("HUD_TooHeavyForBuilding"));
			stringBuilder.Replace("%1", to.GetDisplayNameString());
			StringUtil.ApplyFormulae(stringBuilder, null, to);
			GameImpl.Instance.ShowMessageBox(stringBuilder.ToString());
		}
		else if (minTransferrable > item.GetAmount())
		{
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_TooFewToSell"));
		}
		else
		{
			if (maxTransferrable <= 1 || transferAll)
			{
				if (inputFrame == null)
				{
					SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
					return false;
				}
				SoundManager.PlayMenuSound(SoundManager.SelectSound);
				if (trading)
				{
					inputFrame.AddAction(InputAction.EquipmentTrade(item, carrier, to, maxTransferrable));
				}
				else if (flag)
				{
					inputFrame.AddAction(InputAction.EquipmentPickpocket(item, carrier, to, maxTransferrable, detection));
				}
				else
				{
					inputFrame.AddAction(InputAction.EquipmentTransfer(item, carrier, to, maxTransferrable));
				}
				return true;
			}
			SoundManager.PlayMenuSound(SoundManager.SelectSound);
			GameImpl.Instance.ShowEquipmentTransferAmountBox(carrier, item, to, minTransferrable, maxTransferrable, trading ? EquipmentTransferAmountBox.Mode.Trading : EquipmentTransferAmountBox.Mode.Transferring);
		}
		return false;
	}

	public static void ShowCantTransferReason(CantTransferReason reason, TileObject carrier, Equipment item, TileObject to)
	{
		switch (reason)
		{
		case CantTransferReason.SourceTooHeavyWithoutMe:
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			Character character = carrier as Character;
			bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
			StringBuilder stringBuilder5 = new StringBuilder(50);
			stringBuilder5.AppendWithoutGarbage(character.GetMaxInventoryWeightIgnoringBackpack(item) * (useMetricWeights ? 0.45359236f : 1f), 2);
			StringBuilder stringBuilder6 = new StringBuilder(50);
			stringBuilder6.AppendWithoutGarbage(character.Inventory.GetWeight(character) * (useMetricWeights ? 0.45359236f : 1f), 2);
			StringBuilder stringBuilder7 = new StringBuilder(200);
			stringBuilder7.Append(GameImpl.Translate(useMetricWeights ? "HUD_CantTransferBackpackKg" : "HUD_CantTransferBackpack"));
			stringBuilder7.Replace("%1", character.GetDisplayNameString());
			stringBuilder7.Replace("%2", stringBuilder5.ToString());
			stringBuilder7.Replace("%3", (character.Appearance.Gender == GenderType.Female) ? GameImpl.Translate("SPEECH_She") : GameImpl.Translate("SPEECH_He"));
			stringBuilder7.Replace("%4", stringBuilder6.ToString());
			StringUtil.ApplyFormulae(stringBuilder7, null, character, null, character, null);
			GameImpl.Instance.ShowMessageBox(stringBuilder7.ToString());
			break;
		}
		case CantTransferReason.WantToKeepMe:
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			StringBuilder stringBuilder4 = new StringBuilder(200);
			stringBuilder4.Append(GameImpl.Translate("HUD_WantToKeep"));
			stringBuilder4.Replace("%1", carrier.GetDisplayNameString());
			StringUtil.ApplyFormulae(stringBuilder4, null, carrier);
			GameImpl.Instance.ShowMessageBox(stringBuilder4.ToString());
			break;
		}
		case CantTransferReason.NotUsableYet:
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_NotUsableYet"));
			break;
		case CantTransferReason.TooHeavy:
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			StringBuilder stringBuilder3 = new StringBuilder(200);
			stringBuilder3.Append((to is Character) ? GameImpl.Translate("HUD_TooHeavyForCharacter") : GameImpl.Translate("HUD_TooHeavyForBuilding"));
			stringBuilder3.Replace("%1", to.GetDisplayNameString());
			StringUtil.ApplyFormulae(stringBuilder3, null, to);
			GameImpl.Instance.ShowMessageBox(stringBuilder3.ToString());
			break;
		}
		case CantTransferReason.QuantityTooSmallToTrade:
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_QuantityTooSmallToTrade"));
			break;
		case CantTransferReason.CantAfford:
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			StringBuilder stringBuilder2 = new StringBuilder(200);
			stringBuilder2.Append(GameImpl.Translate("HUD_CantAfford"));
			stringBuilder2.Replace("%1", to.GetDisplayNameString());
			StringUtil.ApplyFormulae(stringBuilder2, null, to);
			GameImpl.Instance.ShowMessageBox(stringBuilder2.ToString());
			break;
		}
		case CantTransferReason.CantTransferToAmbientCharacter:
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_CantTransferToAmbientCharacter"));
			break;
		case CantTransferReason.CookingPot:
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("HUD_CantTransferCookingPot"));
			break;
		case CantTransferReason.LiquidProductReceptacle:
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
			StringBuilder stringBuilder = new StringBuilder(200);
			stringBuilder.Append(GameImpl.Translate("HUD_CantTransferReceptacle"));
			stringBuilder.Replace("%1", item.GetDisplayNameString());
			StringUtil.ApplyFormulae(stringBuilder, null, item.GetPrototype(), item.GetAmount());
			GameImpl.Instance.ShowMessageBox(stringBuilder.ToString());
			break;
		}
		}
	}

	protected bool HasAnyItemsForTransferAll(InventoryBehaviour inventory)
	{
		foreach (EquipmentBehaviour unityItem in inventory.UnityItems)
		{
			if (unityItem.Item.IsIncludedInTakeAll(inventory.Carrier))
			{
				return true;
			}
		}
		return false;
	}

	protected bool TransferAll(InventoryBehaviour fromInventory, InventoryBehaviour toInventory, bool trading, InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		if (fromInventory.UnityItems.Count > 0)
		{
			if (inputFrame == null || instance.InputFrame <= WantTransferAllInputFrame)
			{
				return false;
			}
			TileObject carrier = fromInventory.Carrier;
			bool flag = false;
			CantTransferReason cantTransferReason = CantTransferReason.CanTransfer;
			Equipment equipment = null;
			foreach (EquipmentBehaviour unityItem in fromInventory.UnityItems)
			{
				if (!unityItem.Item.IsIncludedInTakeAll(carrier))
				{
					continue;
				}
				TileObject to;
				CursorAction cursorAction;
				CantTransferReason cantTransferReason2 = CanStartTransferEquipment(carrier, unityItem.Item, out to, out cursorAction);
				if (cantTransferReason2 == CantTransferReason.CanTransfer)
				{
					int minTransferrable = 1;
					int maxTransferrable = unityItem.Item.GetAmount();
					if (trading)
					{
						TradePage.IsTradable(unityItem.Item, carrier as Character, to as Character, out minTransferrable, out maxTransferrable);
					}
					maxTransferrable = Math.Min(maxTransferrable, unityItem.Item.GetMaxTransferrableTo(to));
					if (maxTransferrable > 0)
					{
						if (trading)
						{
							inputFrame.AddAction(InputAction.EquipmentTrade(unityItem.Item, carrier, to, maxTransferrable));
						}
						else
						{
							inputFrame.AddAction(InputAction.EquipmentTransfer(unityItem.Item, carrier, to, maxTransferrable));
						}
						flag = true;
						WantTransferAllInputFrame = inputFrame.Frame;
						break;
					}
					Debug.Log("TransferAll: maxTransferrable was negative or 0: " + maxTransferrable);
				}
				else if (cantTransferReason == CantTransferReason.CanTransfer || (cantTransferReason2 == CantTransferReason.TooHeavy && cantTransferReason != CantTransferReason.TooHeavy))
				{
					cantTransferReason = cantTransferReason2;
					equipment = unityItem.Item;
				}
			}
			if (!flag)
			{
				if (equipment != null)
				{
					ShowCantTransferReason(cantTransferReason, carrier, equipment, toInventory.Carrier);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		Session instance = Session.Instance;
		InputFunctionManager instance2 = InputFunctionManager.Instance;
		if (!GameImpl.Instance.IsDialogOpen() && (this is CharacterPage || this is BuildingPage || this is TakePage || this is TradePage || this is GatherPage))
		{
			TradePage tradePage = this as TradePage;
			bool flag = tradePage == null || tradePage.PendingTrades.Count == 0;
			if (instance2.IsJustPressed(InputFunction.InventorySortBy, capture: true, flag ? AvailableAction.Caption[(int)(3 + instance.InventorySortBy)] : 0))
			{
				InventoryBehaviour.OnSortByPressedStatic();
			}
			if (instance2.IsJustPressed(InputFunction.InventorySortOrder, capture: true, flag ? AvailableAction.Caption[(int)(1 + instance.InventorySortOrder)] : 0))
			{
				InventoryBehaviour.OnSortOrderPressedStatic();
			}
		}
		base.HandleInput(inputFrame);
	}
}
