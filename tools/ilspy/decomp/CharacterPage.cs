using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPage : InfoPage
{
	public static CharacterPage Instance;

	public Character CurrentCharacter;

	private InventoryBehaviour UnityInventory;

	private ThermometerBehaviour UnityThermometer;

	private InsulationDisplayBehaviour UnityInsulationDisplay;

	private Button UnityBuildingButton;

	private RawImage UnityBuildingIcon;

	private TextMeshProUGUI UnityBuildingText;

	private GameObject UnityInfoPanel;

	private GameObject UnityRolesPanel;

	private GameObject UnityRolesContents;

	private GameObject UnitySkillsPanelBG;

	private GameObject[] UnitySkillsPanel = new GameObject[2];

	private SkillDisplayBehaviour[] SkillDisplays = new SkillDisplayBehaviour[10];

	private List<SurvivalFactorDisplayBehaviour> SurvivalFactorDisplays = new List<SurvivalFactorDisplayBehaviour>();

	private List<RoleDisplayBehaviour> RoleDisplays = new List<RoleDisplayBehaviour>();

	private static int HUD_Thirst = StringUtil.JenkinsHash("HUD_Thirst");

	private static int HUD_Hunger = StringUtil.JenkinsHash("HUD_Hunger");

	private static int HUD_SleepDeprivation = StringUtil.JenkinsHash("HUD_SleepDeprivation");

	private static int HUD_LungCancer = StringUtil.JenkinsHash("HUD_LungCancer");

	private static int HUD_Intoxication = StringUtil.JenkinsHash("HUD_Intoxication");

	private static int HUD_Pregnancy = StringUtil.JenkinsHash("HUD_Pregnancy");

	private static int HUD_DownTime = StringUtil.JenkinsHash("HUD_DownTime");

	private static float TotalVerticalSpacingOnRolesColumn = 56f;

	private Selectable LastSelectedRoleButton;

	public override EquipmentBehaviour FindUnityItem(Equipment item, TileObject carrier)
	{
		if (carrier == null || carrier == CurrentCharacter)
		{
			EquipmentBehaviour equipmentBehaviour = UnityInventory.FindUnityItem(item);
			if (equipmentBehaviour != null)
			{
				return equipmentBehaviour;
			}
		}
		return null;
	}

	public override void OnAwake()
	{
		Instance = this;
		UnityBuildingButton = base.gameObject.transform.Find("BuildingButton").gameObject.GetComponent<Button>();
		UnityBuildingIcon = base.gameObject.transform.Find("BuildingButton/Icon").gameObject.GetComponent<RawImage>();
		UnityBuildingText = base.gameObject.transform.Find("BuildingButton/Text").gameObject.GetComponent<TextMeshProUGUI>();
		UnityInventory = base.gameObject.transform.Find("InventoryPanel").gameObject.GetComponent<InventoryBehaviour>();
		UnityThermometer = base.gameObject.transform.Find("Thermometer").gameObject.GetComponent<ThermometerBehaviour>();
		UnityInsulationDisplay = base.gameObject.transform.Find("InventoryPanel/InsulationPanel").gameObject.GetComponent<InsulationDisplayBehaviour>();
		UnityInfoPanel = base.gameObject.transform.Find("InfoPanel").gameObject;
		UnityRolesPanel = base.gameObject.transform.Find("RolesPanel").gameObject;
		UnityRolesContents = base.gameObject.transform.Find("RolesPanel/Viewport/Content").gameObject;
		UnitySkillsPanelBG = base.gameObject.transform.Find("SkillsPanel").gameObject;
		for (int i = 0; i < UnitySkillsPanel.Length; i++)
		{
			UnitySkillsPanel[i] = base.gameObject.transform.Find("SkillsPanel/SkillsPanel" + (i + 1)).gameObject;
		}
	}

	public CharacterPage Initialize(Character character)
	{
		CurrentCharacter = character;
		UnityInventory.Initialize(character, GameImpl.Translate(InventoryBehaviour.HUD_Inventory));
		for (int i = 0; i < UnitySkillsPanel.Length; i++)
		{
			UnitySkillsPanel[i].DeleteAllChildren();
		}
		for (int j = 0; j < 10; j++)
		{
			GameObject gameObject = Object.Instantiate(InfoScreen.SkillDisplay.GetAsset(), UnitySkillsPanel[j / 5].transform);
			SkillDisplays[j] = gameObject.GetComponent<SkillDisplayBehaviour>();
			SkillDisplays[j].Initialize(character, (SkillType)j);
		}
		UnityInfoPanel.DeleteAllChildren();
		SurvivalFactorDisplays.Clear();
		AddSurvivalFactorDisplay(HUD_Thirst, Character.ThirstCriticalTime, Character.ThirstDieTime, Sun.DayLengthSecs, wantPowerNapButton: false);
		AddSurvivalFactorDisplay(HUD_Hunger, Character.HungerCriticalTime, Character.HungerDieTime, Sun.DayLengthSecs, wantPowerNapButton: false);
		AddSurvivalFactorDisplay(HUD_SleepDeprivation, Character.SleepDeprivationCriticalTime, Character.SleepDeprivationDieTime, Sun.DayLengthSecs, wantPowerNapButton: true);
		AddSurvivalFactorDisplay(HUD_Intoxication, Character.BACConfusion, Character.BACDeath, 0.05f, wantPowerNapButton: false);
		AddSurvivalFactorDisplay(HUD_Pregnancy, Human.PregnancyMonths - 1f, Human.PregnancyMonths, 1f, wantPowerNapButton: false);
		AddSurvivalFactorDisplay(HUD_DownTime, Sun.DayLengthSecs, Sun.DayLengthSecs, Sun.DayLengthSecs, wantPowerNapButton: false);
		PopulateRoleDisplay();
		UnityThermometer.Populate();
		Update();
		return this;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (SelectableBehaviour.CurSelectionMode == SelectableBehaviour.SelectionMode.Buttons && CurrentCharacter.InsideBuilding != null)
		{
			SelectableBehaviour.MoveCursorToButton(UnityBuildingButton);
		}
	}

	private void AddSurvivalFactorDisplay(int nameHash, float criticalTime, float deathTime, float notches, bool wantPowerNapButton)
	{
		SurvivalFactorDisplayBehaviour component = Object.Instantiate(InfoScreen.SurvivalFactorDisplay.GetAsset(), UnityInfoPanel.transform).GetComponent<SurvivalFactorDisplayBehaviour>();
		component.Initialize(nameHash, criticalTime, deathTime, notches, wantPowerNapButton);
		SurvivalFactorDisplays.Add(component);
	}

	private void PopulateRoleDisplay()
	{
		for (int num = UnityRolesContents.transform.childCount - 1; num >= RoleDisplays.Count; num--)
		{
			Object.DestroyImmediate(UnityRolesContents.transform.GetChild(num).gameObject);
		}
		for (int i = RoleDisplays.Count; i < CurrentCharacter.Roles.Count; i++)
		{
			RoleDisplayBehaviour component = Object.Instantiate(InfoScreen.RoleDisplay.GetAsset(), UnityRolesContents.transform).GetComponent<RoleDisplayBehaviour>();
			component.Initialize(i, this);
			RoleDisplays.Add(component);
		}
		for (int num2 = RoleDisplays.Count - 1; num2 >= CurrentCharacter.Roles.Count; num2--)
		{
			Object.DestroyImmediate(UnityRolesContents.transform.GetChild(num2).gameObject);
			RoleDisplays.RemoveAt(num2);
		}
		foreach (RoleDisplayBehaviour roleDisplay in RoleDisplays)
		{
			roleDisplay.Populate(CurrentCharacter);
		}
		UnityRolesPanel.SetActive(CurrentCharacter.Roles.Count > 0);
	}

	public void UpdateRolesSize()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		RectTransform rectTransform2 = (RectTransform)UnitySkillsPanelBG.transform;
		RectTransform rectTransform3 = (RectTransform)UnityInfoPanel.transform;
		((RectTransform)UnityRolesPanel.transform).sizeDelta = new Vector2(654f, rectTransform.rect.height - rectTransform2.rect.height - rectTransform3.rect.height - TotalVerticalSpacingOnRolesColumn);
	}

	public override void Populate()
	{
		UnityInventory.Populate(SwappingSuppliesMode.None, this);
		Update();
		if (UnityEventSystem != null)
		{
			UnityEventSystem.firstSelectedGameObject = ((CurrentCharacter.InsideBuilding != null) ? UnityBuildingButton.gameObject : null);
		}
		UnityBuildingButton.gameObject.SetActive(CurrentCharacter.InsideBuilding != null);
		UnityBuildingText.SetUnityText((CurrentCharacter.InsideBuilding != null) ? CurrentCharacter.InsideBuilding.GetDisplayNameString() : "");
		PopulateRoleDisplay();
	}

	public override void Update()
	{
		base.Update();
		float height = ((RectTransform)base.transform).rect.height;
		float num = ((CurrentCharacter.InsideBuilding != null) ? 128f : 48f);
		RectTransform obj = (RectTransform)UnityInventory.transform;
		obj.sizeDelta = new Vector2(obj.sizeDelta.x, height - num - 12f - 12f);
		Material mat = null;
		Color col = Color.white;
		UnityBuildingIcon.texture = ((CurrentCharacter.InsideBuilding != null) ? CurrentCharacter.InsideBuilding.GetIcon(out mat, out col, highlighted: false) : null);
		UnityBuildingIcon.material = mat;
		UnityBuildingIcon.color = col;
		int desiredInsulation = Character.CalcRequiredInsulationForTemperature(Session.Instance.Weather.GetTemperatureInCelsius());
		int count;
		GenderType clothingCountAndGender = CurrentCharacter.GetClothingCountAndGender(GameImpl.Instance.Settings.Language, out count);
		UnityThermometer.SetTemperature(CurrentCharacter.GetBodyTemperatureIncludingFeverInCelsius());
		UnityInsulationDisplay.SetInsulation(CurrentCharacter.GetTotalClothingInsulationIncludingDampness(), CurrentCharacter.GetTotalClothingInsulation(), desiredInsulation, clothingCountAndGender, count);
		foreach (SurvivalFactorDisplayBehaviour survivalFactorDisplay in SurvivalFactorDisplays)
		{
			if (survivalFactorDisplay.NameHash == HUD_Thirst)
			{
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetThirst());
			}
			if (survivalFactorDisplay.NameHash == HUD_Hunger)
			{
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetHunger());
			}
			if (survivalFactorDisplay.NameHash == HUD_SleepDeprivation)
			{
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetSleepDeprivation());
			}
			if (survivalFactorDisplay.NameHash == HUD_Intoxication)
			{
				survivalFactorDisplay.gameObject.SetActive(CurrentCharacter.GetBloodAlcoholConcentration() > 0f);
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetBloodAlcoholConcentration());
			}
			if (survivalFactorDisplay.NameHash == HUD_Pregnancy)
			{
				survivalFactorDisplay.gameObject.SetActive(CurrentCharacter.GetPregnancyProgression() > 0f);
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetPregnancyProgression() * Human.PregnancyMonths);
			}
			if (survivalFactorDisplay.NameHash == HUD_LungCancer)
			{
				survivalFactorDisplay.gameObject.SetActive(CurrentCharacter.GetLungCancer() > 0f);
				survivalFactorDisplay.SetAmount(CurrentCharacter.GetThirst());
			}
			if (survivalFactorDisplay.NameHash == HUD_DownTime)
			{
				survivalFactorDisplay.gameObject.SetActive(CurrentCharacter.DownTime > 0f);
				survivalFactorDisplay.SetAmount(CurrentCharacter.DownTime);
			}
		}
		UpdateRolesSize();
		if (!(UnityEventSystem.currentSelectedGameObject != null))
		{
			return;
		}
		Selectable component = UnityEventSystem.currentSelectedGameObject.GetComponent<Selectable>();
		if (!(component != null))
		{
			return;
		}
		GameObject gameObject = UnityEventSystem.currentSelectedGameObject;
		while (gameObject != null && gameObject != base.gameObject)
		{
			if (gameObject.GetComponent<RoleDisplayBehaviour>() != null)
			{
				LastSelectedRoleButton = component;
				break;
			}
			gameObject = ((gameObject.transform.parent != null) ? gameObject.transform.parent.gameObject : null);
		}
	}

	public override bool HasInventory()
	{
		return true;
	}

	public override bool IsShowingInventoryFor(TileObject obj)
	{
		return CurrentCharacter == obj;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		CurrentCharacter.BuildDisplayName(sb, noStrangers: true, englishOnly: false);
	}

	public void OnBuildingButtonClicked()
	{
		if (CurrentCharacter.InsideBuilding != null)
		{
			InfoScreen.Instance.OnDeactivate();
			InfoScreen.Instance.Activate(CurrentCharacter.InsideBuilding, null);
		}
	}

	public override void HandleInput(InputFrame inputFrame)
	{
		foreach (SurvivalFactorDisplayBehaviour survivalFactorDisplay in SurvivalFactorDisplays)
		{
			survivalFactorDisplay.HandleInput(inputFrame);
		}
		foreach (RoleDisplayBehaviour roleDisplay in RoleDisplays)
		{
			roleDisplay.HandleInput(inputFrame);
		}
		base.HandleInput(inputFrame);
	}

	public override void PreHandleInput(InputFrame inputFrame)
	{
		base.PreHandleInput(inputFrame);
		if (UnityInventory.UnityItems.Count > 0 && RoleDisplays.Count > 0 && !GameImpl.Instance.IsDialogOpen())
		{
			float axis = InputFunctionManager.Instance.GetAxis(InputFunction.MenuSwitchInventory);
			if (axis < 0f)
			{
				EquipmentBehaviour equipmentBehaviour = ((UnityInventory.LastSelectedItem != null) ? UnityInventory.LastSelectedItem : UnityInventory.UnityItems[0]);
				UnityEventSystem.SetSelectedGameObject(equipmentBehaviour.gameObject);
				SelectableBehaviour.MoveCursorToButton(equipmentBehaviour);
			}
			else if (axis > 0f)
			{
				Selectable selectable = ((LastSelectedRoleButton != null) ? LastSelectedRoleButton : RoleDisplays[0].UnityCancelButton);
				UnityEventSystem.SetSelectedGameObject(selectable.gameObject);
				SelectableBehaviour.MoveCursorToButton(selectable);
			}
		}
	}
}
