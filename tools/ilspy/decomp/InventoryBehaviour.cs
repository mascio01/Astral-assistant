using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryBehaviour : MonoBehaviour
{
	public struct EquipmentScore
	{
		public Equipment Item;

		public float Score;

		public string Name;

		public int AmountAfterPendingTrades;

		public bool Transferred;

		public int VisibleAmountWithoutTrades;

		public int Order;

		public EquipmentScore(Equipment equipment, int amountAfterPendingTrades, bool transferred, int visibleAmount, int order)
		{
			Item = equipment;
			AmountAfterPendingTrades = amountAfterPendingTrades;
			Transferred = transferred;
			VisibleAmountWithoutTrades = visibleAmount;
			Order = order;
			Name = GameImpl.Translate(equipment.GetPrototype().NameHash);
			switch (Session.Instance.InventorySortBy)
			{
			case SortBy.Weight:
				Score = equipment.GetWeight();
				break;
			case SortBy.WeightTotal:
				Score = equipment.GetWeight() * (float)(Transferred ? AmountAfterPendingTrades : (VisibleAmountWithoutTrades + (AmountAfterPendingTrades - Item.GetAmount())));
				break;
			case SortBy.Value:
				Score = equipment.GetBasePrice();
				break;
			case SortBy.ValueTotal:
				Score = equipment.GetBasePrice() * (float)(Transferred ? AmountAfterPendingTrades : (VisibleAmountWithoutTrades + (AmountAfterPendingTrades - Item.GetAmount())));
				break;
			default:
				Score = 0f;
				break;
			}
		}
	}

	public class SortEquipmentByScore : IComparer<EquipmentScore>
	{
		int IComparer<EquipmentScore>.Compare(EquipmentScore a, EquipmentScore b)
		{
			if (a.Score < b.Score)
			{
				return -1;
			}
			if (a.Score > b.Score)
			{
				return 1;
			}
			return string.CompareOrdinal(a.Name, b.Name);
		}
	}

	public class SortEquipmentByCategory : IComparer<EquipmentScore>
	{
		int IComparer<EquipmentScore>.Compare(EquipmentScore a, EquipmentScore b)
		{
			int num = string.CompareOrdinal(a.Item.GetCategory(), b.Item.GetCategory());
			if (num == 0)
			{
				num = string.CompareOrdinal(a.Name, b.Name);
				if (num == 0)
				{
					if (a.Item.GetLiquidContentsAmount() < b.Item.GetLiquidContentsAmount())
					{
						return -1;
					}
					if (a.Item.GetLiquidContentsAmount() > b.Item.GetLiquidContentsAmount())
					{
						return 1;
					}
					if (a.Item.GetCurrentAmmo() < b.Item.GetCurrentAmmo())
					{
						return -1;
					}
					if (a.Item.GetCurrentAmmo() > b.Item.GetCurrentAmmo())
					{
						return 1;
					}
					if (a.Item.GetArmorProtection() < b.Item.GetArmorProtection())
					{
						return -1;
					}
					if (a.Item.GetArmorProtection() > b.Item.GetArmorProtection())
					{
						return 1;
					}
					if (a.Order < b.Order)
					{
						return -1;
					}
					if (a.Order > b.Order)
					{
						return 1;
					}
				}
			}
			return num;
		}
	}

	public static SortEquipmentByScore EquipmentSorterByScore = new SortEquipmentByScore();

	public static SortEquipmentByCategory EquipmentSorterByCategory = new SortEquipmentByCategory();

	public static List<EquipmentScore> Temp = new List<EquipmentScore>();

	public TileObject Carrier;

	public Character TradingWith;

	public List<EquipmentBehaviour> UnityItems = new List<EquipmentBehaviour>();

	public EquipmentBehaviour LastSelectedItem;

	public bool Interactable = true;

	public float Weight;

	public float MaxWeight;

	private GameObject UnityContentObj;

	private RawImage UnityBackgroundImage;

	private TextMeshProUGUI UnityTitle;

	private TextMeshProUGUI UnityWeight;

	private ProgressBarBehaviour UnityProgressBar;

	private GameObject[] UnitySortByButton = new GameObject[6];

	private GameObject[] UnitySortOrderButton = new GameObject[2];

	public static int HUD_lbs = StringUtil.JenkinsHash("HUD_lbs");

	public static int HUD_kg = StringUtil.JenkinsHash("HUD_kg");

	public static int HUD_Inventory = StringUtil.JenkinsHash("HUD_Inventory");

	private bool HasItemsRemaining;

	private const int CellSize = 128;

	private void Awake()
	{
		UnityContentObj = base.transform.Find("Inventory/Viewport/Content").gameObject;
		UnityBackgroundImage = base.transform.Find("Inventory").GetComponent<RawImage>();
		UnityTitle = base.transform.Find("Title").GetComponent<TextMeshProUGUI>();
		UnityWeight = base.transform.Find("Weight").GetComponent<TextMeshProUGUI>();
		UnityProgressBar = base.transform.Find("WeightBar").GetComponent<ProgressBarBehaviour>();
		for (int i = 0; i < UnitySortByButton.Length; i++)
		{
			GameObject[] unitySortByButton = UnitySortByButton;
			int num = i;
			Transform obj = base.transform;
			SortBy sortBy = (SortBy)i;
			unitySortByButton[num] = obj.Find("SortBy" + sortBy.ToString() + "Button").gameObject;
		}
		for (int j = 0; j < UnitySortOrderButton.Length; j++)
		{
			GameObject[] unitySortOrderButton = UnitySortOrderButton;
			int num2 = j;
			Transform obj2 = base.transform;
			SortOrder sortOrder = (SortOrder)j;
			unitySortOrderButton[num2] = obj2.Find("SortOrder" + sortOrder.ToString() + "Button").gameObject;
		}
	}

	public void Initialize(TileObject carrier, string title)
	{
		if (UnityContentObj == null)
		{
			UnityContentObj = base.transform.Find("Inventory/Viewport/Content").gameObject;
		}
		if (UnityBackgroundImage == null)
		{
			UnityBackgroundImage = base.transform.Find("Inventory").GetComponent<RawImage>();
		}
		if (UnityTitle == null)
		{
			UnityTitle = base.transform.Find("Title").GetComponent<TextMeshProUGUI>();
		}
		if (UnityWeight == null)
		{
			UnityWeight = base.transform.Find("Weight").GetComponent<TextMeshProUGUI>();
		}
		if (UnityProgressBar == null)
		{
			UnityProgressBar = base.transform.Find("WeightBar").GetComponent<ProgressBarBehaviour>();
		}
		Carrier = carrier;
		UnityItems.Clear();
		LastSelectedItem = null;
		UnityContentObj.DeleteAllChildren();
		((RectTransform)UnityContentObj.transform).anchoredPosition = Vector2.zero;
		UnityTitle.SetUnityText(title);
	}

	public static void SortInventory(List<EquipmentScore> equipmentScores)
	{
		if (Session.Instance.InventorySortBy == SortBy.Type)
		{
			equipmentScores.Sort(EquipmentSorterByCategory);
		}
		else if (Session.Instance.InventorySortBy != SortBy.Time)
		{
			equipmentScores.Sort(EquipmentSorterByScore);
		}
		if (Session.Instance.InventorySortOrder == SortOrder.Descending)
		{
			equipmentScores.Reverse();
		}
	}

	public static bool CanShowItemOnSellingFoodScreen(Character carrierCharacter, Equipment equipment)
	{
		if (equipment.GetNutrition() <= 0f)
		{
			return false;
		}
		if (equipment.GetBasePrice() <= 0f)
		{
			return false;
		}
		if (equipment.WasGifted())
		{
			return false;
		}
		if (!carrierCharacter.CanSellItemToAICommunity(equipment))
		{
			return false;
		}
		return true;
	}

	private int ShouldEquipmentBeVisible(SwappingSuppliesMode mode, TradePage tradePage, Character carrierCharacter, Equipment equipment)
	{
		if (equipment.Concealed)
		{
			if (carrierCharacter == null)
			{
				return 0;
			}
			if (mode == SwappingSuppliesMode.Trading || (uint)(mode - 6) <= 1u)
			{
				return 0;
			}
		}
		if ((mode == SwappingSuppliesMode.GivingFood || mode == SwappingSuppliesMode.SellingFood || mode == SwappingSuppliesMode.SellingFoodMarkedUp) && !CanShowItemOnSellingFoodScreen(carrierCharacter, equipment))
		{
			return 0;
		}
		if (mode == SwappingSuppliesMode.Pickpocketing && carrierCharacter != null && carrierCharacter.IsWearing(equipment))
		{
			return 0;
		}
		if (tradePage != null)
		{
			if (equipment.GetBasePrice() == 0f)
			{
				return 0;
			}
			if (equipment.GetPrototype() == EquipmentPrototype.Gold)
			{
				return 0;
			}
			if (equipment.WasGifted())
			{
				return 0;
			}
			if (!carrierCharacter.IsControllableByPlayer())
			{
				return carrierCharacter.CanSellItemToPlayerCommunity(equipment);
			}
			if (!carrierCharacter.CanSellItemToAICommunity(equipment))
			{
				return 0;
			}
		}
		return equipment.GetAmount();
	}

	public void Populate(SwappingSuppliesMode mode, InfoPage page)
	{
		HasItemsRemaining = false;
		int num = Mathf.FloorToInt(((RectTransform)base.transform).rect.width / 128f);
		RectTransform rectTransform = (RectTransform)UnityContentObj.transform;
		Temp.Clear();
		TradePage tradePage = page as TradePage;
		Character character = Carrier as Character;
		EquipmentContainer inventory = Carrier.GetInventory();
		Weight = 0f;
		bool[] array = null;
		if (tradePage != null)
		{
			array = new bool[tradePage.PendingTrades.Count];
			if (EquipmentPrototype.Gold != null)
			{
				bool invalidTrade;
				int num2 = tradePage.CalcTotalGoldPlayerPays(out invalidTrade);
				Weight += EquipmentPrototype.Gold.Weight * (float)((tradePage.Taker == Carrier) ? (-num2) : num2);
			}
		}
		for (int i = 0; i < inventory.Count; i++)
		{
			Equipment item = inventory.GetItem(i);
			int visibleAmount = ShouldEquipmentBeVisible(mode, tradePage, character, item);
			int num3 = item.GetAmount();
			if (tradePage != null)
			{
				num3 = tradePage.GetAmountAfterPendingTrade(item, Carrier, out var pendingTradeIndex);
				if (pendingTradeIndex != -1)
				{
					array[pendingTradeIndex] = true;
				}
				if (num3 <= 0)
				{
					continue;
				}
			}
			if (!item.IsWornOrRemovedForSparring(Carrier))
			{
				Weight += (float)num3 * item.GetWeight();
			}
			Temp.Add(new EquipmentScore(item, num3, transferred: false, visibleAmount, i));
		}
		if (tradePage != null)
		{
			for (int j = 0; j < tradePage.PendingTrades.Count; j++)
			{
				if (!array[j])
				{
					Equipment item2 = tradePage.PendingTrades[j].Item;
					int amount = tradePage.PendingTrades[j].Amount;
					if ((tradePage.PendingTrades[j].Selling ? tradePage.TakeFrom : tradePage.Taker) == Carrier)
					{
						Temp.Add(new EquipmentScore(item2, amount, transferred: true, amount, inventory.Count + j));
						Weight += (float)amount * item2.GetWeight();
					}
				}
			}
		}
		SortInventory(Temp);
		List<EquipmentBehaviour> list = new List<EquipmentBehaviour>();
		list.Capacity = Temp.Count;
		for (int k = 0; k < Temp.Count; k++)
		{
			Equipment item3 = Temp[k].Item;
			if (Temp[k].Transferred || Temp[k].AmountAfterPendingTrades > item3.GetAmount() - Temp[k].VisibleAmountWithoutTrades)
			{
				EquipmentBehaviour equipmentBehaviour = FindUnityItem(item3);
				if (equipmentBehaviour == null)
				{
					GameObject obj = Object.Instantiate(InfoScreen.EquipmentIcon.GetAsset());
					obj.transform.SetParent(UnityContentObj.transform, worldPositionStays: false);
					equipmentBehaviour = obj.GetComponent<EquipmentBehaviour>();
					equipmentBehaviour.Initialize(Carrier, item3);
					equipmentBehaviour.ParentInventoryBehaviour = this;
				}
				equipmentBehaviour.AmountAfterPendingTrades = Temp[k].AmountAfterPendingTrades;
				equipmentBehaviour.VisibleAmountWithoutTrades = Temp[k].VisibleAmountWithoutTrades;
				equipmentBehaviour.Transferred = Temp[k].Transferred;
				equipmentBehaviour.gameObject.transform.SetSiblingIndex(list.Count);
				equipmentBehaviour.Populate();
				equipmentBehaviour.interactable = Interactable;
				list.Add(equipmentBehaviour);
				if ((float)(Mathf.CeilToInt((float)list.Count / (float)num) * 128) > rectTransform.anchoredPosition.y + HudBehaviour.Instance.HudPanelRectTransform.rect.height)
				{
					HasItemsRemaining = true;
					break;
				}
			}
		}
		for (int num4 = UnityContentObj.transform.childCount - 1; num4 >= list.Count; num4--)
		{
			GameObject gameObject = UnityContentObj.transform.GetChild(num4).gameObject;
			if (LastSelectedItem != null && LastSelectedItem.gameObject == gameObject)
			{
				LastSelectedItem = null;
			}
			Object.Destroy(gameObject);
		}
		list.CopyToList(UnityItems);
		bool flashing;
		if (tradePage != null)
		{
			MaxWeight = character.GetMaxInventoryWeightAfterPendingTrades(Temp);
			flashing = Weight > MaxWeight;
		}
		else
		{
			MaxWeight = Carrier.GetMaxInventoryWeight();
			flashing = character != null && character.Alive && character.Encumbered;
		}
		Temp.Clear();
		bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
		StringBuilder stringBuilder = new StringBuilder(50);
		stringBuilder.AppendWithoutGarbage(Weight * (useMetricWeights ? 0.45359236f : 1f), 2);
		stringBuilder.Append('/');
		stringBuilder.AppendWithoutGarbage(MaxWeight * (useMetricWeights ? 0.45359236f : 1f), 2, comma: false);
		stringBuilder.Append(' ');
		stringBuilder.Append(GameImpl.Translate(useMetricWeights ? HUD_kg : HUD_lbs));
		UnityWeight.SetUnityText(stringBuilder);
		UnityProgressBar.SetValue(Weight / MaxWeight);
		UnityProgressBar.SetFlashing(flashing);
		UnityBackgroundImage.color = (Interactable ? Color.white : Color.gray);
		for (int l = 0; l < 6; l++)
		{
			if (UnitySortByButton[l] != null)
			{
				UnitySortByButton[l].SetActive(l == (int)Session.Instance.InventorySortBy);
			}
		}
		for (int m = 0; m < 2; m++)
		{
			if (UnitySortOrderButton[m] != null)
			{
				UnitySortOrderButton[m].SetActive(m == (int)Session.Instance.InventorySortOrder);
			}
		}
	}

	public void Update()
	{
		if (EquipmentBehaviour.CurrentHovered != null && EquipmentBehaviour.CurrentHovered.ParentInventoryBehaviour == this)
		{
			LastSelectedItem = EquipmentBehaviour.CurrentHovered;
		}
		if (HasItemsRemaining)
		{
			RectTransform rectTransform = (RectTransform)UnityContentObj.transform;
			if (rectTransform.anchoredPosition.y + HudBehaviour.Instance.HudPanelRectTransform.rect.height >= rectTransform.rect.height)
			{
				InfoScreen.Instance.WantRepopulate = true;
			}
		}
	}

	public EquipmentBehaviour GetFirstItemExcludingWorn()
	{
		foreach (EquipmentBehaviour unityItem in UnityItems)
		{
			if (unityItem.Item.IsIncludedInTakeAll(Carrier))
			{
				return unityItem;
			}
		}
		return null;
	}

	public EquipmentBehaviour FindUnityItem(Equipment item)
	{
		foreach (EquipmentBehaviour unityItem in UnityItems)
		{
			if (unityItem.Item == item)
			{
				return unityItem;
			}
		}
		return null;
	}

	public Equipment GetNextEquipmentToSelect(EquipmentBehaviour current)
	{
		for (int i = 0; i < UnityItems.Count; i++)
		{
			if (!(UnityItems[i] == current))
			{
				continue;
			}
			if (i < UnityItems.Count - 1)
			{
				EquipmentBehaviour equipmentBehaviour = UnityItems[i + 1];
				if (!(equipmentBehaviour != null))
				{
					return null;
				}
				return equipmentBehaviour.Item;
			}
			if (i > 0)
			{
				EquipmentBehaviour equipmentBehaviour2 = UnityItems[i - 1];
				if (!(equipmentBehaviour2 != null))
				{
					return null;
				}
				return equipmentBehaviour2.Item;
			}
			return null;
		}
		return null;
	}

	public void OnSortOrderPressed()
	{
		OnSortOrderPressedStatic();
	}

	public void OnSortByPressed()
	{
		OnSortByPressedStatic();
	}

	public void OnSetSortByPressed(int i)
	{
		OnSetSortByPressedStatic(i);
	}

	public static void OnSortOrderPressedStatic()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.InventorySortOrder = (SortOrder)((int)(Session.Instance.InventorySortOrder + 1 + 2) % 2);
		InfoScreen.Instance.WantRepopulate = true;
	}

	public static void OnSortByPressedStatic()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.InventorySortBy = (SortBy)((int)(Session.Instance.InventorySortBy + 1 + 6) % 6);
		InfoScreen.Instance.WantRepopulate = true;
	}

	public static void OnSetSortByPressedStatic(int i)
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		Session.Instance.InventorySortBy = (SortBy)i;
		InfoScreen.Instance.WantRepopulate = true;
	}
}
