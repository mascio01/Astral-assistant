using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TakePage : InfoPage
{
	public static TakePage Instance;

	public Character Taker;

	public TileObject TakeFrom;

	public SwappingSuppliesMode Mode;

	public float PickpocketItemDetection;

	private InventoryBehaviour UnityTakerInventory;

	private InventoryBehaviour UnityTakeFromInventory;

	private ProgressBarBehaviour UnityPickpocketDetectionBar;

	private Button UnityTakeAllButton;

	private Button UnityGiveAllButton;

	private bool WantTakeAll;

	private bool WantGiveAll;

	private bool First;

	private static int INFOPAGE_Transfer = StringUtil.JenkinsHash("INFOPAGE_Transfer");

	public static float MaxPickpocketDetection = 100f;

	public static float PickpocketBaseWibble = 10f;

	public static float PickpocketItemWibbleFactor = 2f;

	public static float PickpicketWibbleSpeed = 1f;

	public override void OnAwake()
	{
		Instance = this;
		UnityTakerInventory = base.gameObject.transform.Find("TakerInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityTakeFromInventory = base.gameObject.transform.Find("TakeFromInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityPickpocketDetectionBar = base.gameObject.FindChild("PickpocketDetection").GetComponent<ProgressBarBehaviour>();
		UnityTakeAllButton = base.gameObject.FindChild("TakeAllButton").GetComponent<Button>();
		UnityGiveAllButton = base.gameObject.FindChild("GiveAllButton").GetComponent<Button>();
	}

	public TakePage Initialize(TileObject takeFrom, Character taker, SwappingSuppliesMode mode)
	{
		Taker = taker;
		TakeFrom = takeFrom;
		Mode = mode;
		UnityTakerInventory.Initialize(taker, taker.GetDisplayNameString(noStrangers: false, englishOnly: false));
		UnityTakeFromInventory.Initialize(takeFrom, takeFrom.GetDisplayNameString(noStrangers: false, englishOnly: false));
		UnityTakeFromInventory.Interactable = mode != SwappingSuppliesMode.GivingFood && mode != SwappingSuppliesMode.SellingFood && mode != SwappingSuppliesMode.SellingFoodMarkedUp;
		UnityTakeAllButton.gameObject.SetActive(mode != SwappingSuppliesMode.Pickpocketing);
		UnityGiveAllButton.gameObject.SetActive(mode != SwappingSuppliesMode.Pickpocketing);
		UnityPickpocketDetectionBar.gameObject.SetActive(mode == SwappingSuppliesMode.Pickpocketing);
		First = true;
		return this;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (Mode == SwappingSuppliesMode.Pickpocketing)
		{
			InfoScreen.Instance.HideActionMenu = true;
			PickpocketItemDetection = 0f;
		}
	}

	public override void Populate()
	{
		UnityTakerInventory.Populate(Mode, this);
		UnityTakeFromInventory.Populate(Mode, this);
		UnityTakeAllButton.interactable = HasAnyItemsForTransferAll(UnityTakeFromInventory) && UnityTakeFromInventory.Interactable;
		UnityGiveAllButton.interactable = HasAnyItemsForTransferAll(UnityTakerInventory);
		if (First)
		{
			UnityEventSystem.firstSelectedGameObject = ((UnityTakeFromInventory.GetFirstItemExcludingWorn() != null) ? UnityTakeFromInventory.GetFirstItemExcludingWorn().gameObject : ((UnityTakeFromInventory.UnityItems.Count > 0) ? UnityTakeFromInventory.UnityItems[0].gameObject : ((UnityTakerInventory.UnityItems.Count > 0) ? UnityTakerInventory.UnityItems[0].gameObject : null)));
			First = false;
		}
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
		sb.Append(GameImpl.Translate(INFOPAGE_Transfer));
	}

	public void OnTakeAll()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantTakeAll = true;
		WantGiveAll = false;
		WantTransferAllInputFrame = 0;
	}

	public void OnGiveAll()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		WantGiveAll = true;
		WantTakeAll = false;
		WantTransferAllInputFrame = 0;
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		base.HandleInput(inputFrame);
		if (WantTakeAll && TransferAll(UnityTakeFromInventory, UnityTakerInventory, trading: false, inputFrame))
		{
			WantTakeAll = false;
		}
		if (WantGiveAll && TransferAll(UnityTakerInventory, UnityTakeFromInventory, trading: false, inputFrame))
		{
			WantGiveAll = false;
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

	public override void Update()
	{
		base.Update();
		if (Mode == SwappingSuppliesMode.Pickpocketing)
		{
			PickpocketItemDetection = 0f;
			Character character = TakeFrom as Character;
			float num = character?.PickpocketDetection ?? 0f;
			EquipmentBehaviour currentHovered = EquipmentBehaviour.CurrentHovered;
			if (currentHovered != null)
			{
				float pickpocketNoticeability = currentHovered.Item.GetPickpocketNoticeability(Taker);
				PickpocketItemDetection += pickpocketNoticeability;
				PickpocketItemDetection += (Mathf.PerlinNoise1D((GameImpl.RealTimeSinceStartup + 3600f) * PickpicketWibbleSpeed) - 0.5f) * pickpocketNoticeability * PickpocketItemWibbleFactor;
				PickpocketItemDetection += (Mathf.PerlinNoise1D(GameImpl.RealTimeSinceStartup * PickpicketWibbleSpeed) - 0.5f) * PickpocketBaseWibble;
				num += PickpocketItemDetection;
			}
			float num2 = MaxPickpocketDetection * 0.5f;
			Color color = ((num < num2) ? Color.Lerp(Color.green, Color.yellow, Mathf.Clamp01(num / num2)) : Color.Lerp(Color.yellow, Color.red, Mathf.Clamp01((num - num2) / num2)));
			UnityPickpocketDetectionBar.SetCol(color, Color.Lerp(color, Color.white, 0.5f));
			UnityPickpocketDetectionBar.SetFlashing(num >= MaxPickpocketDetection);
			UnityPickpocketDetectionBar.SetValue(num / MaxPickpocketDetection);
			if ((character == null || character.PickpocketDetection >= MaxPickpocketDetection) && InfoScreen.Instance.Active && !InfoScreen.Instance.WantClose)
			{
				InfoScreen.Instance.CloseInfoScreen();
			}
		}
	}
}
