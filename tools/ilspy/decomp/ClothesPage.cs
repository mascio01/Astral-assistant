using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClothesPage : BaseCharacterCreationPage
{
	public static ClothesPage Instance;

	public int ClothesPointsRemaining;

	private static int Name = StringUtil.JenkinsHash("MENU_ClothesPage");

	public ClothesPage Initialize()
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
		CharacterCreationMenu characterCreationMenu = GetCharacterCreationMenu();
		GameObject gameObject = base.gameObject.FindChild("ClothesPanel/Viewport/Content");
		gameObject.DeleteAllChildren();
		for (int i = 0; i < characterCreationMenu.PreviewEquipment.Count; i++)
		{
			if (characterCreationMenu.PreviewEquipment[i] != null && characterCreationMenu.PreviewEquipment[i].GetClothingType() != ClothingType.Invalid && characterCreationMenu.PreviewEquipment[i].GetClothingType() != ClothingType.Backpack && characterCreationMenu.PreviewEquipment[i].GetClothingType() != ClothingType.BodyArmor && characterCreationMenu.PreviewEquipment[i].GetClothingType() != ClothingType.LegArmor)
			{
				EquipmentOptionBehaviour component = UnityEngine.Object.Instantiate(InfoScreen.EquipmentOption.GetAsset(), gameObject.transform, worldPositionStays: false).GetComponent<EquipmentOptionBehaviour>();
				component.Initialize(i);
				EquipmentOptions.Add(component);
			}
		}
	}

	public override void Populate()
	{
		base.Populate();
		CharacterCreationSettings characterCreationSettings = GetCharacterCreationSettings();
		base.gameObject.FindChild("ClothesPanel").SetActive(GameImpl.Instance.CurrentStory.Settings.CalcClothesPoints(characterCreationSettings.StartDayOfYear) > 0);
		string text = GameImpl.Translate("MENU_ClothesPoints");
		text = text.Replace("%1", ClothesPointsRemaining.ToString());
		text = text.Replace("%2", LoadingMenu.GetSeasonString(characterCreationSettings.StartDayOfYear));
		base.gameObject.FindChild("StatsPanel/PointsRemaining").GetComponent<TextMeshProUGUI>().SetUnityText(text);
		Color32[] array = ((characterCreationSettings.Appearance.Gender == GenderType.Male) ? HumanAppearance.MaleUnderwearColors : HumanAppearance.FemaleUnderwearColors);
		SetSliderValue("MenuLayout1/UnderwearColorField/Slider", Math.Max(Array.IndexOf(array, characterCreationSettings.GetHumanAppearance().UnderwearColor), 0), 0f, array.Length - 1);
		SetSliderValue("MenuLayout1/HairBandColorField/Slider", Math.Max(Array.IndexOf(HumanAppearance.HairBandColors, characterCreationSettings.GetHumanAppearance().HairBandColor), 0), 0f, HumanAppearance.HairBandColors.Length - 1);
	}

	public void OnSetUnderwearColorSlider(Slider v)
	{
		Color32[] array = ((GetCharacterCreationSettings().Appearance.Gender == GenderType.Male) ? HumanAppearance.MaleUnderwearColors : HumanAppearance.FemaleUnderwearColors);
		GetCharacterCreationSettings().GetHumanAppearance().UnderwearColor = array[MathUtil.Clamp((int)(v.value + 0.5f), 0, array.Length - 1)];
		GetCharacterCreationMenu().SetChangedAppearance();
		Owner.WantRepopulate = true;
	}

	public void OnSetHairBandColorSlider(Slider v)
	{
		GetCharacterCreationSettings().GetHumanAppearance().HairBandColor = HumanAppearance.HairBandColors[MathUtil.Clamp((int)(v.value + 0.5f), 0, HumanAppearance.HairBandColors.Length - 1)];
		GetCharacterCreationMenu().SetChangedAppearance();
		Owner.WantRepopulate = true;
	}

	public void OnRandomise()
	{
		ResetClothes(GetCharacterCreationMenu());
		RandomiseClothes(GetCharacterCreationMenu(), MathUtil.NonDeterministicRand);
	}

	public void ResetClothes(CharacterCreationMenu menu)
	{
		for (int i = 0; i < menu.PreviewEquipment.Count; i++)
		{
			Equipment equipment = menu.PreviewEquipment[i];
			if (equipment != null && equipment.GetClothingType() != ClothingType.Invalid && equipment.GetClothingType() != ClothingType.BodyArmor && equipment.GetClothingType() != ClothingType.LegArmor && equipment.GetClothingType() != ClothingType.Backpack)
			{
				menu.RemoveStartingEquipment(i, toggle: true);
			}
		}
		menu.TabsPanel.WantRepopulate = true;
	}

	public void RandomiseClothes(CharacterCreationMenu menu, CustomRandom rand)
	{
		for (int i = 0; i < menu.PreviewEquipment.Count; i++)
		{
			Equipment equipment = menu.PreviewEquipment[i];
			if (equipment != null && equipment.GetClothingType() != ClothingType.Invalid && equipment.GetClothingType() != ClothingType.BodyArmor && equipment.GetClothingType() != ClothingType.LegArmor && equipment.GetClothingType() != ClothingType.Backpack)
			{
				equipment.RandomiseVariations(rand);
				equipment.ClothingIconIndex = -1;
			}
		}
		int num = Character.CalcRequiredInsulationForTemperature(Weather.CalcAverageTemperatureInCelsiusFromDayOfYear(menu.Settings.StartDayOfYear));
		AddRandomClothingOfType(menu, ClothingType.Bottom, rand, useFewestPoints: true, mustIncreaseInsulation: false);
		AddRandomClothingOfType(menu, ClothingType.Top, rand, useFewestPoints: true, mustIncreaseInsulation: false);
		AddRandomClothingOfType(menu, ClothingType.Shoes, rand, useFewestPoints: true, mustIncreaseInsulation: false);
		for (int j = 0; j < 100 && (0u | (AddRandomClothingOfType(menu, ClothingType.Bottom, rand, useFewestPoints: false, j > 0) ? 1u : 0u) | (AddRandomClothingOfType(menu, ClothingType.Top, rand, useFewestPoints: false, j > 0) ? 1u : 0u) | (AddRandomClothingOfType(menu, ClothingType.Shoes, rand, useFewestPoints: false, j > 0) ? 1u : 0u) | (AddRandomClothingOfType(menu, ClothingType.Hat, rand, useFewestPoints: false, j > 0) ? 1u : 0u)) != 0; j++)
		{
			if (menu.PreviewGimp.GetTotalClothingInsulation() >= num)
			{
				break;
			}
		}
		if (rand.RandomChoice(0.25f))
		{
			AddRandomClothingOfType(menu, ClothingType.Glasses, rand, useFewestPoints: false, mustIncreaseInsulation: false);
		}
	}

	public bool AddRandomClothingOfType(CharacterCreationMenu menu, ClothingType clothingType, CustomRandom rand, bool useFewestPoints, bool mustIncreaseInsulation)
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		Equipment equipment = menu.PreviewGimp.Clothes[(int)clothingType];
		int num = -1;
		int num2 = 0;
		if (equipment != null)
		{
			if (mustIncreaseInsulation)
			{
				num = equipment.GetInsulation();
			}
			int index = menu.PreviewEquipment.IndexOf(equipment);
			num2 = settings.StartingEquipmentOptions[index].Points;
		}
		int num3 = int.MaxValue;
		if (useFewestPoints)
		{
			for (int i = 0; i < menu.PreviewEquipment.Count; i++)
			{
				if (menu.PreviewEquipment[i] != null && menu.PreviewEquipment[i].GetClothingType() == clothingType && menu.PreviewEquipment[i].GetInsulation() > num && settings.StartingEquipmentOptions[i].Points < num3)
				{
					num3 = settings.StartingEquipmentOptions[i].Points;
				}
			}
		}
		List<int> list = new List<int>();
		for (int j = 0; j < menu.PreviewEquipment.Count; j++)
		{
			if (menu.PreviewEquipment[j] != null && menu.PreviewEquipment[j].GetClothingType() == clothingType && menu.PreviewEquipment[j].GetInsulation() > num && settings.StartingEquipmentOptions[j].Points <= num2 + ClothesPointsRemaining && settings.StartingEquipmentOptions[j].Points <= num3)
			{
				list.Add(j);
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		int index2 = list[rand.Next(list.Count)];
		_ = settings.StartingEquipmentOptions[index2];
		if (menu.PreviewEquipment[index2] != null && !menu.PreviewGimp.InventoryContains(menu.PreviewEquipment[index2]))
		{
			menu.AddStartingEquipment(index2, toggle: true);
			return true;
		}
		return false;
	}
}
