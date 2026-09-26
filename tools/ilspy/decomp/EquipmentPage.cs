using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EquipmentPage : BaseCharacterCreationPage
{
	public static EquipmentPage Instance;

	public int EquipmentPointsRemaining;

	private static int Name = StringUtil.JenkinsHash("MENU_EquipmentPage");

	public EquipmentPage Initialize()
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
		GameObject gameObject = base.gameObject.FindChild("EquipmentPanel/Viewport/Content");
		gameObject.DeleteAllChildren();
		for (int i = 0; i < characterCreationMenu.PreviewEquipment.Count; i++)
		{
			if (characterCreationMenu.PreviewEquipment[i] != null && (characterCreationMenu.PreviewEquipment[i].GetClothingType() == ClothingType.Invalid || characterCreationMenu.PreviewEquipment[i].GetClothingType() == ClothingType.Backpack || characterCreationMenu.PreviewEquipment[i].GetClothingType() == ClothingType.BodyArmor || characterCreationMenu.PreviewEquipment[i].GetClothingType() == ClothingType.LegArmor))
			{
				EquipmentOptionBehaviour component = Object.Instantiate(InfoScreen.EquipmentOption.GetAsset(), gameObject.transform, worldPositionStays: false).GetComponent<EquipmentOptionBehaviour>();
				component.Initialize(i);
				EquipmentOptions.Add(component);
			}
		}
	}

	public override void Populate()
	{
		base.Populate();
		PopulateLoadoutDropdown();
		base.gameObject.FindChild("StatsPanel/PointsRemaining").GetComponent<TextMeshProUGUI>().SetUnityText(GameImpl.Translate("MENU_EquipmentPoints").Replace("%1", EquipmentPointsRemaining.ToString()));
	}

	public void OnRandomise()
	{
		ResetEquipment(GetCharacterCreationMenu());
		RandomiseEquipment(GetCharacterCreationMenu(), MathUtil.NonDeterministicRand);
	}

	public void ResetEquipment(CharacterCreationMenu menu)
	{
		for (int i = 0; i < menu.PreviewEquipment.Count; i++)
		{
			Equipment equipment = menu.PreviewEquipment[i];
			if (equipment != null && (equipment.GetClothingType() == ClothingType.Invalid || equipment.GetClothingType() == ClothingType.BodyArmor || equipment.GetClothingType() == ClothingType.LegArmor || equipment.GetClothingType() == ClothingType.Backpack))
			{
				menu.RemoveStartingEquipment(i, toggle: true);
			}
		}
		menu.TabsPanel.WantRepopulate = true;
	}

	public void RandomiseEquipment(CharacterCreationMenu menu, CustomRandom rand)
	{
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		int num = 0;
		while (EquipmentPointsRemaining > 0 && num < 1000)
		{
			List<StartingEquipment> list = settings.StartingEquipmentOptions;
			if (num == 0 && menu.PreviewGimp.Inventory.FindItemOfClass(typeof(Weapon)) == null)
			{
				list = new List<StartingEquipment>();
				for (int i = 0; i < settings.StartingEquipmentOptions.Count; i++)
				{
					if (menu.PreviewEquipment[i] is Weapon)
					{
						list.Add(settings.StartingEquipmentOptions[i]);
					}
				}
			}
			if (list.Count > 0)
			{
				int index = rand.Next(list.Count);
				StartingEquipment startingEquipment = settings.StartingEquipmentOptions[index];
				Equipment equipment = menu.PreviewEquipment[index];
				if (equipment != null && (equipment.GetClothingType() == ClothingType.Invalid || equipment.GetClothingType() == ClothingType.Backpack || equipment.GetClothingType() == ClothingType.BodyArmor || equipment.GetClothingType() == ClothingType.LegArmor) && EquipmentPointsRemaining >= startingEquipment.Points && (num >= 500 || menu.PreviewGimp.Inventory.FindItemOfType(equipment.GetPrototype()) == null))
				{
					menu.AddStartingEquipment(index, toggle: false);
				}
			}
			num++;
		}
	}
}
