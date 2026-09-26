using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VitalStatsPage : BaseCharacterCreationPage
{
	public static VitalStatsPage Instance;

	private GameObject UnityCommunityNameRandomizeButton;

	private static int Name = StringUtil.JenkinsHash("MENU_VitalStats");

	private List<Selectable> Selectables = new List<Selectable>();

	public VitalStatsPage Initialize()
	{
		return this;
	}

	public override void BuildDisplayName(StringBuilder sb)
	{
		sb.Append(GameImpl.Translate(Name));
	}

	public override void OnActivate()
	{
		base.OnActivate();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		for (int i = 0; i < 2; i++)
		{
			GenderType genderType = (GenderType)i;
			list.Add(new TMP_Dropdown.OptionData(GameImpl.Translate("MENU_" + genderType)));
		}
		base.gameObject.FindChild("MenuLayout/GenderField/Dropdown").GetComponent<TMP_Dropdown>().options = list;
		UnityCommunityNameRandomizeButton = base.gameObject.FindChild("RandomizeCommunityName");
	}

	public override void Populate()
	{
		base.Populate();
		Selectables.Clear();
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		List<TMP_Dropdown.OptionData> list = new List<TMP_Dropdown.OptionData>();
		list.Add(new TMP_Dropdown.OptionData(""));
		int value = 0;
		foreach (string savedCharacterName in SaveGameManager.Instance.SavedCharacterNames)
		{
			if (characterCreationMenu.LoadedFavouriteCharacterName == savedCharacterName)
			{
				value = list.Count;
			}
			list.Add(new TMP_Dropdown.OptionData(savedCharacterName));
		}
		base.gameObject.FindChild("MenuLayout1/FavouriteCharacterField/Dropdown").GetComponent<TMP_Dropdown>().options = list;
		base.gameObject.FindChild("MenuLayout1/FavouriteCharacterField/Dropdown").GetComponent<TMP_Dropdown>().value = value;
		base.gameObject.FindChild("MenuLayout1/FavouriteCharacterField/SaveFavouriteButton").GetComponent<Button>().interactable = string.IsNullOrEmpty(characterCreationMenu.LoadedFavouriteCharacterName);
		base.gameObject.FindChild("MenuLayout1/FavouriteCharacterField/DeleteFavouriteButton").GetComponent<Button>().interactable = !string.IsNullOrEmpty(characterCreationMenu.LoadedFavouriteCharacterName);
		base.gameObject.FindChild("MenuLayout/GenderField/Dropdown").GetComponent<TMP_Dropdown>().value = (int)characterCreationSettings.Appearance.Gender;
		base.gameObject.FindChild("MenuLayout/AgeField/InputField").GetComponent<TMP_InputField>().SetUnityText(((int)characterCreationSettings.Appearance.Age).ToString());
		base.gameObject.FindChild("MenuLayout/FirstNameField/InputField").GetComponent<TMP_InputField>().SetUnityText(GameImpl.TranslateName(characterCreationSettings.FirstName));
		base.gameObject.FindChild("MenuLayout/SurnameField/InputField").GetComponent<TMP_InputField>().SetUnityText(GameImpl.TranslateSurname(characterCreationSettings.Surname, characterCreationSettings.Appearance.Gender));
		base.gameObject.FindChild("MenuLayout/FacialHairTypeField").SetActive(characterCreationSettings.Appearance.Gender == GenderType.Male);
		base.gameObject.FindChild("MenuLayout/BreastSizeField").SetActive(characterCreationSettings.Appearance.Gender == GenderType.Female);
		Selectables.Add(base.gameObject.FindChild("MenuLayout/GenderField/Dropdown").GetComponent<TMP_Dropdown>());
		Selectables.Add(base.gameObject.FindChild("MenuLayout/FirstNameField/InputField").GetComponent<TMP_InputField>());
		Selectables.Add(base.gameObject.FindChild("MenuLayout/SurnameField/InputField").GetComponent<TMP_InputField>());
		Selectables.Add(base.gameObject.FindChild("MenuLayout/CommunityNameField/InputField").GetComponent<TMP_InputField>());
		Selectables.Add(SetSliderValue("MenuLayout/AgeField/Slider", characterCreationSettings.Appearance.Age, HumanAppearance.MinAge, HumanAppearance.MaxAge));
		Selectables.Add(SetSliderValue("MenuLayout/SkinColorField/Slider", characterCreationSettings.GetHumanAppearance().SkinColorIndex, 0f, HumanAppearance.SkinColorSpectrum.Length - 1));
		Selectables.Add(SetSliderValue("MenuLayout/HairColorField/Slider", characterCreationSettings.GetHumanAppearance().HairColorIndex, 0f, HumanAppearance.HairColorSpectrum.Length - 1));
		Selectables.Add(SetSliderValue("MenuLayout/EyeColorField/Slider", characterCreationSettings.GetHumanAppearance().EyeColorIndex, 0f, HumanAppearance.EyeColorSpectrum.Length - 1));
		Selectables.Add(SetSliderValue("MenuLayout/HairTypeField/Slider", (float)characterCreationSettings.GetHumanAppearance().HairType, (characterCreationSettings.Appearance.Gender == GenderType.Male) ? 1 : 15, (characterCreationSettings.Appearance.Gender == GenderType.Male) ? 14 : 25));
		Selectables.Add(SetSliderValue("MenuLayout/FacialHairTypeField/Slider", (float)characterCreationSettings.GetHumanAppearance().FacialHairType, 0f, 9f));
		Selectables.Add(SetSliderValue("MenuLayout/FrecklesColorField/Slider", (int)characterCreationSettings.GetHumanAppearance().FrecklesColor.a, 0f, 255f));
		Selectables.Add(SetSliderValue("MenuLayout/TallnessField/Slider", characterCreationSettings.GetHumanAppearance().Bones.Tallness, 0f, 1f));
		Selectables.Add(SetSliderValue("MenuLayout/FatnessField/Slider", characterCreationSettings.GetHumanAppearance().Bones.Fatness, 0f, 1f));
		Selectables.Add(SetSliderValue("MenuLayout/BreastSizeField/Slider", characterCreationSettings.GetHumanAppearance().Bones.BreastSize, 0f, 1f));
		StringBuilder sb = new StringBuilder();
		characterCreationSettings.GangName.BuildDisplayName(sb, englishOnly: false, characterCreationSettings.FirstName, StringStatus.Verified, characterCreationSettings.Surname, StringStatus.Verified, characterCreationSettings.Appearance.Gender, StringStatus.Verified);
		base.gameObject.FindChild("MenuLayout/CommunityNameField/InputField").GetComponent<TMP_InputField>().SetUnityText(sb);
		base.gameObject.FindChild("MenuLayout/CommunityNameField").SetActive(!characterCreationMenu.JoiningNetworkGame);
		BaseMenu.SetupNavigation(Selectables, topAndBottomAreAutomatic: true);
	}

	public override void Update()
	{
		base.Update();
		UnityCommunityNameRandomizeButton.SetActive(InputFunctionManager.Instance.CurrentInputType == InputType.MouseAndKeyboard);
	}

	public void OnSetGender(int val)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GenderType gender = GetCharacterCreationSettings().Appearance.Gender;
			if (val != (int)gender)
			{
				CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
				CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
				characterCreationSettings.Appearance.Gender = ((characterCreationSettings.Appearance.Gender == GenderType.Male) ? GenderType.Female : GenderType.Male);
				MathUtil.Swap(ref characterCreationMenu.GenderSwappedFaceType, ref characterCreationSettings.GetHumanAppearance().FaceType);
				MathUtil.Swap(ref characterCreationMenu.GenderSwappedHairType, ref characterCreationSettings.GetHumanAppearance().HairType);
				MathUtil.Swap(ref characterCreationMenu.GenderSwappedFacialHairType, ref characterCreationSettings.GetHumanAppearance().FacialHairType);
				MathUtil.Swap(ref characterCreationMenu.GenderSwappedUnderwearColor, ref characterCreationSettings.GetHumanAppearance().UnderwearColor);
				MathUtil.Swap(ref characterCreationMenu.GenderSwappedFirstName, ref characterCreationSettings.FirstName);
				characterCreationSettings.GangName.ClearCache();
				characterCreationMenu.SetChangedBones();
				Owner.WantRepopulate = true;
			}
		}
	}

	public void OnSetAgeText(TMP_InputField v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			if (int.TryParse(v.text, out var result))
			{
				result = MathUtil.Clamp(result, (int)HumanAppearance.MinAge, (int)HumanAppearance.MaxAge);
				GetCharacterCreationSettings().Appearance.Age = result;
				GetCharacterCreationMenu().SetChangedAppearance();
			}
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetAgeSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			float age = Mathf.Clamp(v.value, HumanAppearance.MinAge, HumanAppearance.MaxAge);
			GetCharacterCreationSettings().Appearance.Age = age;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFirstName(TMP_InputField v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().FirstName = v.text;
			GetCharacterCreationSettings().GangName.ClearCache();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetSurname(TMP_InputField v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().Surname = v.text;
			GetCharacterCreationSettings().GangName.ClearCache();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetCommunityName(TMP_InputField v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GangName.SetCustomString(v.text);
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetSkinColorSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().SkinColorIndex = v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetEyeColorSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().EyeColorIndex = v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetHairColorSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().HairColorIndex = v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetHairTypeSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().HairType = (HairType)v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFacialHairTypeSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().FacialHairType = (FacialHairType)v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFrecklesColorSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().FrecklesColor.a = (byte)v.value;
			GetCharacterCreationMenu().SetChangedAppearance();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetTallnessSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().Bones.Tallness = v.value;
			GetCharacterCreationMenu().SetChangedBones();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetFatnessSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().Bones.Fatness = v.value;
			GetCharacterCreationMenu().SetChangedBones();
			Owner.WantRepopulate = true;
		}
	}

	public void OnSetBreastSizeSlider(Slider v)
	{
		if (!CharacterCreationMenu.IsRepopulating)
		{
			GetCharacterCreationSettings().GetHumanAppearance().Bones.BreastSize = v.value;
			GetCharacterCreationMenu().SetChangedBones();
			Owner.WantRepopulate = true;
		}
	}

	public void OnRandomise()
	{
		GetCharacterCreationMenu().RandomiseAll();
		GetCharacterCreationMenu().SetChangedBones();
		Owner.WantRepopulate = true;
	}

	public void OnLoadFavouriteCharacter(int value)
	{
		string text = base.gameObject.FindChild("MenuLayout1/FavouriteCharacterField/Dropdown").GetComponent<TMP_Dropdown>().options[value].text;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		SavedCharacter savedCharacter = new SavedCharacter();
		if (!savedCharacter.Load(GameImpl.Instance.SaveGamePath + "/Characters/" + text + ".char"))
		{
			return;
		}
		Human previewGimp = GetCharacterCreationMenu().PreviewGimp;
		savedCharacter.ApplyToCharacter(previewGimp);
		GetCharacterCreationSettings().FirstName = previewGimp.FirstName;
		GetCharacterCreationSettings().Surname = previewGimp.Surname;
		GetCharacterCreationSettings().Appearance = previewGimp.GetAppearance();
		previewGimp.Personality.CopyToList(GetCharacterCreationSettings().Personality);
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		EquipmentPage.Instance.ResetEquipment(characterCreationMenu);
		ClothesPage.Instance.ResetClothes(characterCreationMenu);
		SkillsPage.Instance.ResetSkills(characterCreationMenu);
		for (int i = 0; i < savedCharacter.Inventory.Count; i++)
		{
			SavedEquipment savedEquipment = savedCharacter.Inventory[i];
			EquipmentPrototype proto = savedEquipment.Proto;
			if (proto == null)
			{
				continue;
			}
			int equipmentIndex = settings.GetEquipmentIndex(proto);
			if (equipmentIndex == -1)
			{
				continue;
			}
			StartingEquipment startingEquipment = settings.StartingEquipmentOptions[equipmentIndex];
			int num = Math.Max(1, savedEquipment.Amount / startingEquipment.GetAmount());
			for (int j = 0; j < num; j++)
			{
				int num2 = 0;
				num2 = ((proto.ClothingType == ClothingType.Invalid || proto.ClothingType == ClothingType.Backpack || proto.ClothingType == ClothingType.BodyArmor || proto.ClothingType == ClothingType.LegArmor) ? EquipmentPage.Instance.EquipmentPointsRemaining : ClothesPage.Instance.ClothesPointsRemaining);
				if (startingEquipment.Points <= num2)
				{
					Equipment equipment = characterCreationMenu.AddStartingEquipment(equipmentIndex, toggle: false);
					if (equipment != null)
					{
						equipment.ColorVariation = savedEquipment.ColorVariation;
						equipment.ColorVariation2 = savedEquipment.ColorVariation2;
						equipment.ColorVariation3 = savedEquipment.ColorVariation3;
						equipment.MaterialVariation = savedEquipment.MaterialVariation;
						Character.CancelGeneratingClothingIcon(equipment);
						equipment.ClothingIconIndex = -1;
						CharacterCreationMenu.Instance.SetChangedClothes();
						CharacterCreationMenu.Instance.TabsPanel.WantRepopulate = true;
					}
				}
			}
		}
		for (int k = 0; k < savedCharacter.Skills.Length; k++)
		{
			for (int l = 0; l < savedCharacter.Skills[k]; l++)
			{
				SkillsPage.Instance.IncrementSkill(k);
			}
		}
		GetCharacterCreationMenu().SetChangedAppearance();
		GetCharacterCreationMenu().SetChangedBones();
		GetCharacterCreationMenu().LoadedFavouriteCharacterName = text;
		Owner.WantRepopulate = true;
	}

	public void OnSaveFavouriteCharacter()
	{
		GetCharacterCreationMenu().PreviewGimp.SetFirstName(GetCharacterCreationSettings().FirstName);
		GetCharacterCreationMenu().PreviewGimp.Surname = GetCharacterCreationSettings().Surname;
		GetCharacterCreationMenu().PreviewGimp.SaveFavouriteCharacter(delegate(string name)
		{
			GetCharacterCreationMenu().LoadedFavouriteCharacterName = name;
			Owner.WantRepopulate = true;
		});
	}

	public void OnDeleteFavouriteCharacter()
	{
		SaveGameManager.Instance.DeleteCharacter(GetCharacterCreationMenu().LoadedFavouriteCharacterName, delegate
		{
			GetCharacterCreationMenu().LoadedFavouriteCharacterName = "";
			Owner.WantRepopulate = true;
		});
	}

	public void OnRandomizeCommunityName()
	{
		GetCharacterCreationSettings().GangName.Randomise(MathUtil.NonDeterministicRand, unique: false, null);
		Owner.WantRepopulate = true;
	}
}
