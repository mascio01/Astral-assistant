using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacterCreationPage : BaseTabPage
{
	public RawImage PreviewImage;

	public TextMeshProUGUI UnityWeight;

	public ProgressBarBehaviour UnityProgressBar;

	public InsulationDisplayBehaviour UnityInsulationDisplay;

	public TMP_Dropdown UnityLoadoutDropdown;

	public List<EquipmentOptionBehaviour> EquipmentOptions = new List<EquipmentOptionBehaviour>();

	public bool Populating;

	public override void OnAwake()
	{
		GameObject gameObject = base.gameObject.FindChild("CharacterView");
		PreviewImage = ((gameObject != null) ? gameObject.GetComponent<RawImage>() : null);
		GameObject gameObject2 = base.gameObject.FindChild("StatsPanel/Weight");
		UnityWeight = ((gameObject2 != null) ? gameObject2.GetComponent<TextMeshProUGUI>() : null);
		GameObject gameObject3 = base.gameObject.FindChild("StatsPanel/WeightBar");
		UnityProgressBar = ((gameObject3 != null) ? gameObject3.GetComponent<ProgressBarBehaviour>() : null);
		GameObject gameObject4 = base.gameObject.FindChild("StatsPanel/InsulationPanel");
		UnityInsulationDisplay = ((gameObject4 != null) ? gameObject4.gameObject.GetComponent<InsulationDisplayBehaviour>() : null);
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		EquipmentOptions.Clear();
	}

	public override void Populate()
	{
		base.Populate();
		foreach (EquipmentOptionBehaviour equipmentOption in EquipmentOptions)
		{
			equipmentOption.Populate();
		}
		if (UnityWeight != null && UnityProgressBar != null && UnityInsulationDisplay != null)
		{
			Character previewGimp = GetCharacterCreationMenu().PreviewGimp;
			float weight = previewGimp.Inventory.GetWeight(previewGimp);
			float maxInventoryWeight = previewGimp.GetMaxInventoryWeight();
			bool useMetricWeights = GameImpl.Instance.Settings.UseMetricWeights;
			StringBuilder stringBuilder = new StringBuilder(50);
			stringBuilder.AppendWithoutGarbage(weight * (useMetricWeights ? 0.45359236f : 1f), 2);
			stringBuilder.Append('/');
			stringBuilder.AppendWithoutGarbage(maxInventoryWeight * (useMetricWeights ? 0.45359236f : 1f), 2);
			stringBuilder.Append(' ');
			stringBuilder.Append(GameImpl.Translate(useMetricWeights ? HudBehaviour.HUD_kg : HudBehaviour.HUD_lbs));
			UnityWeight.SetUnityText(stringBuilder);
			UnityProgressBar.SetValue(weight / maxInventoryWeight);
			int desiredInsulation = Character.CalcRequiredInsulationForTemperature(Weather.CalcAverageTemperatureInCelsiusFromDayOfYear(GetCharacterCreationSettings().StartDayOfYear));
			int count;
			GenderType clothingCountAndGender = previewGimp.GetClothingCountAndGender(GameImpl.Instance.Settings.Language, out count);
			UnityInsulationDisplay.SetInsulation(previewGimp.GetTotalClothingInsulationIncludingDampness(), previewGimp.GetTotalClothingInsulation(), desiredInsulation, clothingCountAndGender, count);
		}
	}

	public override void Update()
	{
		base.Update();
		if (PreviewImage != null)
		{
			GetCharacterCreationMenu().SetupPreviewImage(PreviewImage);
		}
	}

	public CharacterCreationTabsPanel GetCharacterCreationTabsPanel()
	{
		return (CharacterCreationTabsPanel)Owner;
	}

	public CharacterCreationMenu GetCharacterCreationMenu()
	{
		return GetCharacterCreationTabsPanel().Owner;
	}

	public CharacterCreationSettings GetCharacterCreationSettings()
	{
		return GetCharacterCreationMenu().Settings;
	}

	public Slider SetSliderValue(string name, float val, float min, float max)
	{
		Slider component = base.gameObject.FindChild(name).GetComponent<Slider>();
		component.minValue = min;
		component.maxValue = max;
		component.value = val;
		return component;
	}

	public void PopulateLoadoutDropdown()
	{
		Populating = true;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		int num = settings.Loadouts.Count;
		for (int i = 0; i < settings.Loadouts.Count; i++)
		{
			Loadout loadout = settings.Loadouts[i];
			bool flag = true;
			foreach (LoadoutEquipment item in loadout.Equipment)
			{
				StartingEquipment startingEquipment = GameImpl.Instance.CurrentStory.Settings.FindStartingEquipment(item.Name);
				if (startingEquipment == null)
				{
					flag = false;
					break;
				}
				if (characterCreationSettings.GetAmountOfEquipmentByName(item.Name) < startingEquipment.GetAmount() * item.Amount)
				{
					flag = false;
					break;
				}
			}
			for (int j = 0; j < loadout.SkillPoints.Length; j++)
			{
				if (characterCreationSettings.Skills[j] < loadout.SkillPoints[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				num = i;
				break;
			}
		}
		foreach (Loadout loadout2 in settings.Loadouts)
		{
			list.Add(new TMP_Dropdown.OptionData(GameImpl.Translate(loadout2.GetNameKey())));
		}
		if (num == settings.Loadouts.Count)
		{
			list.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_CUSTOM")));
		}
		if (UnityLoadoutDropdown == null)
		{
			UnityLoadoutDropdown = base.gameObject.FindChild("MenuLayout/LoadoutField/Dropdown").GetComponent<TMP_Dropdown>();
		}
		UnityLoadoutDropdown.transform.parent.gameObject.SetActive(GameImpl.Instance.CurrentStory.Settings.Loadouts.Count > 0);
		UnityLoadoutDropdown.GetComponent<TMP_Dropdown>().options = list;
		UnityLoadoutDropdown.GetComponent<TMP_Dropdown>().value = num;
		Populating = false;
	}

	public void OnSetLoadout(int value)
	{
		if (!Populating)
		{
			CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
			StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
			Loadout loadout = ((value < settings.Loadouts.Count) ? settings.Loadouts[value] : null);
			if (loadout != null)
			{
				characterCreationMenu.SetLoadout(loadout);
			}
		}
	}
}
