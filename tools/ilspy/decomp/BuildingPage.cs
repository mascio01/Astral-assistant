using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BuildingPage : InfoPage
{
	public static BuildingPage Instance;

	public Prop CurrentProp;

	private InventoryBehaviour UnityBuildingInventory;

	private InventoryBehaviour UnityCharacterInventory;

	private GameObject UnityCharacterPanel;

	private List<CharacterIconBehaviour> UnityInhabitants = new List<CharacterIconBehaviour>();

	public override EquipmentBehaviour FindUnityItem(Equipment item, TileObject carrier)
	{
		if (carrier == null || carrier == CurrentProp)
		{
			EquipmentBehaviour equipmentBehaviour = UnityBuildingInventory.FindUnityItem(item);
			if (equipmentBehaviour != null)
			{
				return equipmentBehaviour;
			}
		}
		if (carrier == null || carrier == UnityCharacterInventory.Carrier)
		{
			EquipmentBehaviour equipmentBehaviour2 = UnityCharacterInventory.FindUnityItem(item);
			if (equipmentBehaviour2 != null)
			{
				return equipmentBehaviour2;
			}
		}
		return null;
	}

	public override void OnAwake()
	{
		Instance = this;
		UnityBuildingInventory = base.gameObject.FindChild("InventoryPanel").GetComponent<InventoryBehaviour>();
		UnityCharacterInventory = base.gameObject.FindChild("CharacterInventoryPanel").GetComponent<InventoryBehaviour>();
		UnityCharacterPanel = base.gameObject.FindChild("CharactersPanel");
	}

	public BuildingPage Initialize(Prop prop)
	{
		CurrentProp = prop;
		UnityBuildingInventory.Initialize(prop, GameImpl.Translate(InventoryBehaviour.HUD_Inventory));
		Hud instance = Hud.Instance;
		bool flag = prop is Building building && instance.BuildingSelectedInhabitant != null && instance.BuildingSelectedInhabitant.InsideBuilding == building;
		UnityCharacterInventory.gameObject.SetActive(flag);
		if (flag)
		{
			UnityCharacterInventory.Initialize(instance.BuildingSelectedInhabitant, instance.BuildingSelectedInhabitant.GetDisplayNameString(noStrangers: false, englishOnly: false));
		}
		UnityInhabitants.Clear();
		UnityCharacterPanel.DeleteAllChildren();
		return this;
	}

	public override void Populate()
	{
		Hud instance = Hud.Instance;
		Building building = CurrentProp as Building;
		bool active = building != null && building.GetInhabitantCount() > 0;
		bool flag = building != null && instance.BuildingSelectedInhabitant != null && instance.BuildingSelectedInhabitant.InsideBuilding == building;
		UnityCharacterPanel.gameObject.SetActive(active);
		UnityCharacterInventory.gameObject.SetActive(flag);
		UnityBuildingInventory.Populate(SwappingSuppliesMode.None, this);
		if (flag)
		{
			if (instance.BuildingSelectedInhabitant != UnityCharacterInventory.Carrier)
			{
				UnityCharacterInventory.Initialize(instance.BuildingSelectedInhabitant, instance.BuildingSelectedInhabitant.GetDisplayNameString(noStrangers: false, englishOnly: false));
			}
			UnityCharacterInventory.Populate(SwappingSuppliesMode.None, this);
		}
		if (building == null)
		{
			return;
		}
		for (int i = 0; i < building.Inhabitants.Length; i++)
		{
			CharacterIconBehaviour characterIconBehaviour;
			if (i < UnityInhabitants.Count)
			{
				characterIconBehaviour = UnityInhabitants[i];
			}
			else
			{
				characterIconBehaviour = Object.Instantiate(InfoScreen.CharacterIcon.GetAsset(), UnityCharacterPanel.transform, worldPositionStays: false).GetComponent<CharacterIconBehaviour>();
				UnityInhabitants.Add(characterIconBehaviour);
			}
			Character character = building.Inhabitants[i];
			characterIconBehaviour.gameObject.SetActive(character != null);
			if (character != null)
			{
				characterIconBehaviour.Initialize(building.Inhabitants[i]);
			}
		}
		while (UnityInhabitants.Count > building.Inhabitants.Length)
		{
			Object.Destroy(UnityInhabitants[building.Inhabitants.Length].gameObject);
			UnityInhabitants.RemoveAt(building.Inhabitants.Length);
		}
	}

	public override void Update()
	{
		base.Update();
		int num = (((CurrentProp is Building) ? ((Building)CurrentProp).GetInhabitantCount() : 0) - 1) / 4 + 1;
		float num2 = 32f + 132f * (float)num + 16f * (float)(num - 1);
		float height = ((RectTransform)base.transform).rect.height;
		RectTransform obj = (RectTransform)UnityCharacterPanel.transform;
		obj.sizeDelta = new Vector2(obj.sizeDelta.x, num2);
		RectTransform obj2 = (RectTransform)UnityCharacterInventory.transform;
		obj2.sizeDelta = new Vector2(obj2.sizeDelta.x, height - (num2 + 8f + 8f) - 12f - 12f);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (UnityBuildingInventory.UnityItems.Count > 0 && UnityCharacterInventory.UnityItems.Count > 0 && !GameImpl.Instance.IsDialogOpen())
		{
			float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MenuSwitchInventory);
			if (axis != 0f)
			{
				InventoryBehaviour inventoryBehaviour = ((axis < 0f) ? UnityBuildingInventory : UnityCharacterInventory);
				EquipmentBehaviour equipmentBehaviour = ((inventoryBehaviour.LastSelectedItem != null) ? inventoryBehaviour.LastSelectedItem : inventoryBehaviour.UnityItems[0]);
				UnityEventSystem.SetSelectedGameObject(equipmentBehaviour.gameObject);
				SelectableBehaviour.MoveCursorToButton(equipmentBehaviour);
			}
		}
	}

	public override bool HasInventory()
	{
		return true;
	}

	public override bool IsShowingInventoryFor(TileObject obj)
	{
		if (CurrentProp != obj)
		{
			return GetSelectedInhabitant() == obj;
		}
		return true;
	}

	public override TileObject GetOther(TileObject obj)
	{
		if (obj != CurrentProp)
		{
			return CurrentProp;
		}
		return GetSelectedInhabitant();
	}

	public Character GetSelectedInhabitant()
	{
		Hud instance = Hud.Instance;
		if (instance == null || instance.BuildingSelectedInhabitant == null || instance.BuildingSelectedInhabitant.InsideBuilding != CurrentProp)
		{
			return null;
		}
		return instance.BuildingSelectedInhabitant;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		CurrentProp.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
	}
}
