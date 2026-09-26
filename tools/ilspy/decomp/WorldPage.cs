using System.Collections.Generic;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldPage : BaseCharacterCreationPage
{
	public static WorldPage Instance;

	public TMP_Dropdown UnityDifficultyDropdown;

	private static int Name = StringUtil.JenkinsHash("MENU_WorldPage");

	private List<Selectable> Selectables = new List<Selectable>();

	public static float MaxZombieRespawnDays = 4f;

	public static float MaxTravellerRespawnDays = 4f;

	public static float MaxSurvivorRepopulationDays = 4f;

	private Thread OpenFileDialogThread;

	private WorldGenSettings OpenedWorldGenSettings;

	public override void OnAwake()
	{
		base.OnAwake();
		UnityDifficultyDropdown = base.gameObject.FindChild("MenuLayout/DifficultyField/Dropdown").GetComponent<TMP_Dropdown>();
	}

	public WorldPage Initialize()
	{
		return this;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(Name));
	}

	public override void Populate()
	{
		base.Populate();
		GameImpl instance = GameImpl.Instance;
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		Populating = true;
		Selectables.Clear();
		TMP_Dropdown component = base.gameObject.FindChild("MenuLayout/MapSizeField/Dropdown").GetComponent<TMP_Dropdown>();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		MapSize maxMapSizeForCurrentStories = instance.GetMaxMapSizeForCurrentStories();
		for (int i = 0; i <= (int)maxMapSizeForCurrentStories; i++)
		{
			MapSize mapSize = (MapSize)i;
			list.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_" + mapSize)));
		}
		component.options = list;
		component.value = (int)characterCreationSettings.MapSize;
		List<TMP_Dropdown.OptionData> list2 = new List<TMP_Dropdown.OptionData>();
		int num = -1;
		for (int j = 0; j < settings.DifficultySettings.Count; j++)
		{
			DifficultySettings difficultySettings = settings.DifficultySettings[j];
			list2.Add(new TMP_Dropdown.OptionData(GameImpl.Translate(difficultySettings.GetNameKey())));
			if (difficultySettings.Matches(characterCreationSettings.DifficultySettings))
			{
				num = j;
			}
		}
		if (num == -1)
		{
			num = list2.Count;
			list2.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_CUSTOM")));
		}
		UnityDifficultyDropdown.transform.parent.gameObject.SetActive(settings.DifficultySettings.Count > 1 || (settings.ProcedurallyGenerated && settings.DifficultySettings.Count == 1));
		UnityDifficultyDropdown.options = list2;
		UnityDifficultyDropdown.value = num;
		base.gameObject.FindChild("MenuLayout/StartDayOfYearField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_StartDayOfYear").Replace("%1", LoadingMenu.GetSeasonString(characterCreationSettings.StartDayOfYear)));
		base.gameObject.FindChild("MenuLayout/StartHourOfDayField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_StartHourOfDay").Replace("%1", ((int)characterCreationSettings.StartHourOfDay).ToString()));
		SetupSlider("MenuLayout/StartDayOfYearField/Slider", characterCreationSettings.StartDayOfYear, 0f, Weather.DaysInAYear - 1);
		SetupSlider("MenuLayout/StartHourOfDayField/Slider", characterCreationSettings.StartHourOfDay, 0f, 23f);
		Selectables.Add(component);
		Selectables.Add(UnityDifficultyDropdown);
		base.gameObject.FindChild("Scroll View").SetActive(settings.ProcedurallyGenerated);
		base.gameObject.FindChild("Scroll View/Viewport/Content/DifficultyPanel/FriendlyFireSplashDamageField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_FriendlyFireSplashDamage").Replace("%1", ((int)characterCreationSettings.DifficultySettings.FriendlyFireSplashDamage).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/GreenStrainField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_GreenStrain").Replace("%1", ((int)characterCreationSettings.DifficultySettings.GreenStrainDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/BlueStrainField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_BlueStrain").Replace("%1", ((int)characterCreationSettings.DifficultySettings.BlueStrainDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/RedStrainField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RedStrain").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RedStrainDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/WhiteStrainField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_WhiteStrain").Replace("%1", ((int)characterCreationSettings.DifficultySettings.WhiteStrainDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/InvisibleStrainField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_InvisibleStrain").Replace("%1", ((int)characterCreationSettings.DifficultySettings.InvisibleStrainPercentage).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/InvisibleStrainIncreaseField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_InvisibleStrainIncrement").Replace("%1", ((int)characterCreationSettings.DifficultySettings.InvisibleStrainIncrementPerYear).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/ZombieCrippledField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_ZombieCrippled").Replace("%1", ((int)characterCreationSettings.DifficultySettings.ZombieCrippledPercentage).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/ZombiePanel/ZombieRespawnTimeField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(IsPluralDays(characterCreationSettings.DifficultySettings.ZombieRespawnDays) ? "MENU_ZombieRespawnDays" : "MENU_ZombieRespawnDay").Replace("%1", characterCreationSettings.DifficultySettings.ZombieRespawnDays.ToString("F1")));
		base.gameObject.FindChild("Scroll View/Viewport/Content/SurvivorPanel/SurvivorDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_SurvivorDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.SurvivorCampDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/SurvivorPanel/SurvivorTypeField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_SurvivorType").Replace("%1", ((int)characterCreationSettings.DifficultySettings.SurvivorCampLooterPercentage).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/SurvivorPanel/SurvivorRepopulateTimeField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate(IsPluralDays(characterCreationSettings.DifficultySettings.SurvivorRepopulationDays) ? "MENU_SurvivorRepopulationDays" : "MENU_SurvivorRepopulationDay").Replace("%1", characterCreationSettings.DifficultySettings.SurvivorRepopulationDays.ToString("F1")));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TravellerPanel/HordeDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_HordeDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.HordeDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TravellerPanel/RaiderDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RaiderDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RaiderDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TravellerPanel/RefugeeDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RefugeeDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RefugeeDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TravellerPanel/TraderDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_TraderDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.TraderDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TownPanel/RoadDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RoadDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RoadDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TownPanel/TownDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_TownDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.TownDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TownPanel/VehicleDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_VehicleDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.VehicleDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TownPanel/CountrysidePropDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_CountrysidePropDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.CountrysidePropDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/TownPanel/LootDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_LootDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.LootDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/RiverDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RiverDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RiverDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/RabbitDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_RabbitDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.RabbitDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/DeerDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_DeerDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.DeerDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/FlintDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_FlintDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.FlintDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/LeadOreDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_LeadOreDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.LeadOreDensity).ToString()));
		base.gameObject.FindChild("Scroll View/Viewport/Content/NaturePanel/IronOreDensityField/Text").GetComponent<TMP_Text>().SetUnityText(GameImpl.Translate("MENU_IronOreDensity").Replace("%1", ((int)characterCreationSettings.DifficultySettings.IronOreDensity).ToString()));
		Toggle component2 = base.gameObject.FindChild("Scroll View/Viewport/Content/DifficultyPanel/SaveTokensRequiredField/Toggle").GetComponent<Toggle>();
		component2.isOn = characterCreationSettings.DifficultySettings.SaveTokensRequired;
		Selectables.Add(component2);
		Toggle component3 = base.gameObject.FindChild("Scroll View/Viewport/Content/DifficultyPanel/TradersHaveSaveTokensField/Toggle").GetComponent<Toggle>();
		component3.isOn = characterCreationSettings.DifficultySettings.TradersHaveSaveTokens;
		Selectables.Add(component3);
		base.gameObject.FindChild("Scroll View/Viewport/Content/DifficultyPanel/TradersHaveSaveTokensField").SetActive(characterCreationSettings.DifficultySettings.SaveTokensRequired);
		SetupSlider("Scroll View/Viewport/Content/DifficultyPanel/FriendlyFireSplashDamageField/Slider", characterCreationSettings.DifficultySettings.FriendlyFireSplashDamage, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/GreenStrainField/Slider", characterCreationSettings.DifficultySettings.GreenStrainDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/BlueStrainField/Slider", characterCreationSettings.DifficultySettings.BlueStrainDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/RedStrainField/Slider", characterCreationSettings.DifficultySettings.RedStrainDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/WhiteStrainField/Slider", characterCreationSettings.DifficultySettings.WhiteStrainDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/InvisibleStrainField/Slider", characterCreationSettings.DifficultySettings.InvisibleStrainPercentage, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/InvisibleStrainIncreaseField/Slider", characterCreationSettings.DifficultySettings.InvisibleStrainIncrementPerYear, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/ZombieCrippledField/Slider", characterCreationSettings.DifficultySettings.ZombieCrippledPercentage, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/ZombiePanel/ZombieRespawnTimeField/Slider", characterCreationSettings.DifficultySettings.ZombieRespawnDays, 0f, MaxZombieRespawnDays);
		SetupSlider("Scroll View/Viewport/Content/SurvivorPanel/SurvivorDensityField/Slider", characterCreationSettings.DifficultySettings.SurvivorCampDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/SurvivorPanel/SurvivorTypeField/Slider", characterCreationSettings.DifficultySettings.SurvivorCampLooterPercentage, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/SurvivorPanel/SurvivorRepopulateTimeField/Slider", characterCreationSettings.DifficultySettings.SurvivorRepopulationDays, 0f, MaxSurvivorRepopulationDays);
		SetupSlider("Scroll View/Viewport/Content/TravellerPanel/HordeDensityField/Slider", characterCreationSettings.DifficultySettings.HordeDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/TravellerPanel/RaiderDensityField/Slider", characterCreationSettings.DifficultySettings.RaiderDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/TravellerPanel/RefugeeDensityField/Slider", characterCreationSettings.DifficultySettings.RefugeeDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/TravellerPanel/TraderDensityField/Slider", characterCreationSettings.DifficultySettings.TraderDensity, 0f, 100f);
		SetupSlider("Scroll View/Viewport/Content/TownPanel/RoadDensityField/Slider", characterCreationSettings.DifficultySettings.RoadDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/TownPanel/TownDensityField/Slider", characterCreationSettings.DifficultySettings.TownDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/TownPanel/VehicleDensityField/Slider", characterCreationSettings.DifficultySettings.VehicleDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/TownPanel/CountrysidePropDensityField/Slider", characterCreationSettings.DifficultySettings.CountrysidePropDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/TownPanel/LootDensityField/Slider", characterCreationSettings.DifficultySettings.LootDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/RiverDensityField/Slider", characterCreationSettings.DifficultySettings.RiverDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/RabbitDensityField/Slider", characterCreationSettings.DifficultySettings.RabbitDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/DeerDensityField/Slider", characterCreationSettings.DifficultySettings.DeerDensity, 0f, 200f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/FlintDensityField/Slider", characterCreationSettings.DifficultySettings.FlintDensity, 0f, 400f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/LeadOreDensityField/Slider", characterCreationSettings.DifficultySettings.LeadOreDensity, 0f, 400f);
		SetupSlider("Scroll View/Viewport/Content/NaturePanel/IronOreDensityField/Slider", characterCreationSettings.DifficultySettings.IronOreDensity, 0f, 400f);
		BaseMenu.SetupNavigation(Selectables, topAndBottomAreAutomatic: true);
		Populating = false;
	}

	private static bool IsPluralDays(float days)
	{
		if (GameImpl.Instance.Settings.Language == Language.BrazilianPortuguese)
		{
			return StringUtil.IsPlural(days, GameImpl.Instance.Settings.Language);
		}
		return Mathf.RoundToInt(days * 10f) != 10;
	}

	private void SetupSlider(string name, float val, float min, float max)
	{
		Slider item = SetSliderValue(name, val, min, max);
		Selectables.Add(item);
	}

	public void OnSetMapSize(int value)
	{
		if (!Populating)
		{
			MapSize mapSize = GetCharacterCreationSettings().MapSize;
			if (value != (int)mapSize)
			{
				GetCharacterCreationSettings().MapSize = (MapSize)value;
				Owner.WantRepopulate = true;
			}
		}
	}

	public void OnSetDifficulty(int value)
	{
		if (!Populating)
		{
			StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
			CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
			string text = ((value < settings.DifficultySettings.Count) ? settings.DifficultySettings[value].DifficultyName : "");
			DifficultySettings difficultySettings = settings.GetDifficultySettings(text);
			if (difficultySettings != null)
			{
				characterCreationSettings.DifficultySettings = difficultySettings.MakeCopy();
				Owner.WantRepopulate = true;
			}
		}
	}

	public void OnSetStartDayOfYearSlider(Slider v)
	{
		if (!Populating)
		{
			StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
			int num = settings.CalcClothesPoints(GetCharacterCreationSettings().StartDayOfYear);
			int num2 = settings.CalcClothesPoints((int)v.value);
			GetCharacterCreationSettings().StartDayOfYear = (int)v.value;
			if (num != num2)
			{
				ClothesPage.Instance.ResetClothes(GetCharacterCreationMenu());
				ClothesPage.Instance.ClothesPointsRemaining = num2;
				ClothesPage.Instance.RandomiseClothes(GetCharacterCreationMenu(), MathUtil.NonDeterministicRand);
			}
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetStartHourOfDaySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().StartHourOfDay = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSaveTokensToggled(bool on)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.SaveTokensRequired = on;
			Owner.WantRepopulate = true;
		}
	}

	public void OnTradersHaveSaveTokensToggled(bool on)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.TradersHaveSaveTokens = on;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFriendlyFireSplashDamageSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.FriendlyFireSplashDamage = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetZombieCrippledPercentageSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.ZombieCrippledPercentage = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetZombieRespawnDaysSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.ZombieRespawnDays = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetGreenStrainDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.GreenStrainDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetBlueStrainDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.BlueStrainDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRedStrainDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RedStrainDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetWhiteStrainDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.WhiteStrainDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetInvisibleStrainPercentageSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.InvisibleStrainPercentage = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetInvisibleStrainIncrementSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.InvisibleStrainIncrementPerYear = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetSurvivorCampDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.SurvivorCampDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetSurvivorCampLooterPercentageSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.SurvivorCampLooterPercentage = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetSurvivorRepopulateDaysSlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.SurvivorRepopulationDays = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetHordeDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.HordeDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRaiderDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RaiderDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRefugeeDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RefugeeDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetTraderDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.TraderDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRoadDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RoadDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetTownDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.TownDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetVehicleDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.VehicleDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetCountrysidePropDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.CountrysidePropDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetLootDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.LootDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRiverDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RiverDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetRabbitDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.RabbitDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetDeerDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.DeerDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFlintDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.FlintDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetLeadOreDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.LeadOreDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetIronOreDensitySlider(Slider v)
	{
		if (!Populating)
		{
			GetCharacterCreationSettings().DifficultySettings.IronOreDensity = v.value;
			Owner.WantRepopulate = true;
		}
	}

	public void OnRandomise()
	{
		GetCharacterCreationMenu();
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		characterCreationSettings.StartDayOfYear = nonDeterministicRand.Next(Weather.DaysInAYear);
		characterCreationSettings.StartHourOfDay = nonDeterministicRand.RandomFloat() * 24f;
		Owner.WantRepopulate = true;
	}

	public void OnSaveWorldGenSettings()
	{
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		UtilsDebugMenu.SaveWorldGenSettings(new WorldGenSettings
		{
			RandomSeed = characterCreationSettings.RandomSeed,
			MapSize = characterCreationSettings.MapSize,
			StartDayOfYear = characterCreationSettings.StartDayOfYear,
			StartHourOfDay = characterCreationSettings.StartHourOfDay,
			DifficultySettings = characterCreationSettings.DifficultySettings.MakeCopy()
		});
	}

	public void OnLoadWorldGenSettings()
	{
		if (OpenFileDialogThread == null)
		{
			GetCharacterCreationMenu().DisableInput = 2;
			OpenFileDialogThread = new Thread((ThreadStart)delegate
			{
				OpenedWorldGenSettings = UtilsDebugMenu.LoadWorldGenSettingsFromFile();
			});
			OpenFileDialogThread.Start();
		}
	}

	public override void Update()
	{
		base.Update();
		if (OpenFileDialogThread != null && OpenFileDialogThread.Join(0))
		{
			if (OpenedWorldGenSettings != null)
			{
				CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
				characterCreationSettings.RandomSeed = OpenedWorldGenSettings.RandomSeed;
				characterCreationSettings.MapSize = OpenedWorldGenSettings.MapSize;
				characterCreationSettings.StartDayOfYear = OpenedWorldGenSettings.StartDayOfYear;
				characterCreationSettings.StartHourOfDay = OpenedWorldGenSettings.StartHourOfDay;
				characterCreationSettings.DifficultySettings = OpenedWorldGenSettings.DifficultySettings.MakeCopy();
				Owner.WantRepopulate = true;
			}
			OpenFileDialogThread = null;
			OpenedWorldGenSettings = null;
		}
	}
}
