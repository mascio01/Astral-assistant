using System.Text;

public class GatherPage : InfoPage
{
	public static GatherPage Instance;

	public Prop GatherFrom;

	public Character Gatherer;

	public SwappingSuppliesMode Mode;

	public InventoryBehaviour UnityTakerInventory;

	public InventoryBehaviour UnityTakeFromInventory;

	private bool First;

	private static int INFOPAGE_Gather = StringUtil.JenkinsHash("INFOPAGE_Gather");

	public override void OnAwake()
	{
		Instance = this;
		UnityTakerInventory = base.gameObject.transform.Find("TakerInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityTakeFromInventory = base.gameObject.transform.Find("TakeFromInventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
	}

	public GatherPage Initialize(Prop gatherFrom, Character gatherer, SwappingSuppliesMode mode)
	{
		GatherFrom = gatherFrom;
		Gatherer = gatherer;
		Mode = mode;
		UnityTakerInventory.Initialize(gatherer, gatherer.GetDisplayNameString(noStrangers: false, englishOnly: false));
		UnityTakerInventory.Interactable = false;
		UnityTakeFromInventory.Initialize(gatherFrom, gatherFrom.GetDisplayNameString(noStrangers: false, englishOnly: false));
		First = true;
		return this;
	}

	public override void Populate()
	{
		UnityTakerInventory.Populate(Mode, this);
		UnityTakeFromInventory.Populate(Mode, this);
		if (First)
		{
			UnityEventSystem.firstSelectedGameObject = ((UnityTakeFromInventory.GetFirstItemExcludingWorn() != null) ? UnityTakeFromInventory.GetFirstItemExcludingWorn().gameObject : ((UnityTakeFromInventory.UnityItems.Count > 0) ? UnityTakeFromInventory.UnityItems[0].gameObject : null));
			First = false;
		}
	}

	public override EquipmentBehaviour FindUnityItem(Equipment item, TileObject carrier)
	{
		if (carrier == null || carrier == Gatherer)
		{
			EquipmentBehaviour equipmentBehaviour = UnityTakerInventory.FindUnityItem(item);
			if (equipmentBehaviour != null)
			{
				return equipmentBehaviour;
			}
		}
		if (carrier == null || carrier == GatherFrom)
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
		if (Gatherer != obj)
		{
			return GatherFrom == obj;
		}
		return true;
	}

	public override TileObject GetOther(TileObject obj)
	{
		if (obj != Gatherer)
		{
			return Gatherer;
		}
		return GatherFrom;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(INFOPAGE_Gather));
	}
}
