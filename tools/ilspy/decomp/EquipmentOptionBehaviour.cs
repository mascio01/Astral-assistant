using UnityEngine;
using UnityEngine.UI;

public class EquipmentOptionBehaviour : MonoBehaviour
{
	private enum VariationType
	{
		None,
		ColorVariation,
		ColorVariation2,
		ColorVariation3,
		MaterialVariation
	}

	private EquipmentBehaviour UnityEquipmentIcon;

	private Slider[] UnitySlider = new Slider[2];

	private GameObject UnityMinusButton;

	private GameObject UnityPlusButton;

	private VariationType[] SliderVariationType = new VariationType[2];

	private int StartingEquipmentIndex;

	private void Awake()
	{
		UnityEquipmentIcon = Object.Instantiate(InfoScreen.EquipmentIcon.GetAsset(), base.transform, worldPositionStays: false).GetComponent<EquipmentBehaviour>();
		UnityEquipmentIcon.transform.SetAsFirstSibling();
		((RectTransform)UnityEquipmentIcon.transform).anchoredPosition = Vector2.zero;
		UnitySlider[0] = base.gameObject.FindChild("Slider1").GetComponent<Slider>();
		UnitySlider[1] = base.gameObject.FindChild("Slider2").GetComponent<Slider>();
		UnityMinusButton = base.gameObject.FindChild("MinusButton");
		UnityPlusButton = base.gameObject.FindChild("PlusButton");
	}

	public void Initialize(int index)
	{
		CharacterCreationMenu instance = CharacterCreationMenu.Instance;
		StartingEquipmentIndex = index;
		Equipment equipment = instance.PreviewEquipment[index];
		_ = GameImpl.Instance.CurrentStory.Settings.StartingEquipmentOptions[index];
		UnityEquipmentIcon.Initialize(instance.PreviewGimp, equipment);
		UnityEquipmentIcon.Populate();
		EquipmentPrototype prototype = equipment.GetPrototype();
		int num = 0;
		if (num < SliderVariationType.Length && prototype.ColorVariations != null && prototype.ColorVariations.Length > 1)
		{
			SliderVariationType[num++] = VariationType.ColorVariation;
		}
		if (num < SliderVariationType.Length && prototype.ColorVariations2 != null && prototype.ColorVariations2.Length > 1)
		{
			SliderVariationType[num++] = VariationType.ColorVariation2;
		}
		if (num < SliderVariationType.Length && prototype.ColorVariations3 != null && prototype.ColorVariations3.Length > 1)
		{
			SliderVariationType[num++] = VariationType.ColorVariation3;
		}
		if (num < SliderVariationType.Length && prototype.MaterialVariations != null && prototype.MaterialVariations.Length > 1)
		{
			SliderVariationType[num++] = VariationType.MaterialVariation;
		}
		for (int i = 0; i < UnitySlider.Length; i++)
		{
			UnitySlider[i].gameObject.SetActive(SliderVariationType[i] != VariationType.None);
			UnitySlider[i].minValue = 0f;
			UnitySlider[i].maxValue = GetNumVariations(i) - 1;
			UnitySlider[i].value = GetSliderValue(i);
		}
		UnityMinusButton.SetActive(equipment.CanBeCombined() && !(equipment is MeleeWeapon));
		UnityPlusButton.SetActive(equipment.CanBeCombined() && !(equipment is MeleeWeapon));
	}

	public void Populate()
	{
		UnityEquipmentIcon.Populate();
	}

	private int GetNumVariations(int i)
	{
		EquipmentPrototype prototype = UnityEquipmentIcon.Item.GetPrototype();
		return SliderVariationType[i] switch
		{
			VariationType.ColorVariation => prototype.ColorVariations.Length, 
			VariationType.ColorVariation2 => prototype.ColorVariations2.Length, 
			VariationType.ColorVariation3 => prototype.ColorVariations3.Length, 
			VariationType.MaterialVariation => prototype.MaterialVariations.Length, 
			_ => 0, 
		};
	}

	private int GetSliderValue(int i)
	{
		return SliderVariationType[i] switch
		{
			VariationType.ColorVariation => UnityEquipmentIcon.Item.ColorVariation, 
			VariationType.ColorVariation2 => UnityEquipmentIcon.Item.ColorVariation2, 
			VariationType.ColorVariation3 => UnityEquipmentIcon.Item.ColorVariation3, 
			VariationType.MaterialVariation => UnityEquipmentIcon.Item.MaterialVariation, 
			_ => 0, 
		};
	}

	public void OnSliderValueChanged(int i)
	{
		int sliderValue = GetSliderValue(i);
		int num = (int)(UnitySlider[i].value + 0.5f);
		if (sliderValue != num)
		{
			switch (SliderVariationType[i])
			{
			case VariationType.ColorVariation:
				UnityEquipmentIcon.Item.ColorVariation = num;
				break;
			case VariationType.ColorVariation2:
				UnityEquipmentIcon.Item.ColorVariation2 = num;
				break;
			case VariationType.ColorVariation3:
				UnityEquipmentIcon.Item.ColorVariation3 = num;
				break;
			case VariationType.MaterialVariation:
				UnityEquipmentIcon.Item.MaterialVariation = num;
				break;
			}
			Character.CancelGeneratingClothingIcon(UnityEquipmentIcon.Item);
			UnityEquipmentIcon.Item.ClothingIconIndex = -1;
			CharacterCreationMenu.Instance.SetChangedClothes();
			CharacterCreationMenu.Instance.TabsPanel.WantRepopulate = true;
		}
	}

	public bool HasEnoughPoints()
	{
		CharacterCreationMenu instance = CharacterCreationMenu.Instance;
		StorySettings settings = GameImpl.Instance.CurrentStory.Settings;
		int num = settings.StartingEquipmentOptions[StartingEquipmentIndex].Points;
		ClothingType clothingType = UnityEquipmentIcon.Item.GetClothingType();
		if (clothingType == ClothingType.Invalid || clothingType == ClothingType.Backpack || clothingType == ClothingType.BodyArmor || clothingType == ClothingType.LegArmor)
		{
			return EquipmentPage.Instance.EquipmentPointsRemaining >= num;
		}
		if (instance.PreviewGimp.Clothes[(int)clothingType] != null)
		{
			int index = instance.PreviewEquipment.IndexOf(instance.PreviewGimp.Clothes[(int)clothingType]);
			num -= settings.StartingEquipmentOptions[index].Points;
		}
		return ClothesPage.Instance.ClothesPointsRemaining >= num;
	}

	public void OnToggle()
	{
		CharacterCreationMenu instance = CharacterCreationMenu.Instance;
		if (instance.PreviewGimp.InventoryContains(instance.PreviewEquipment[StartingEquipmentIndex]))
		{
			instance.RemoveStartingEquipment(StartingEquipmentIndex, toggle: true);
		}
		else if (HasEnoughPoints())
		{
			instance.AddStartingEquipment(StartingEquipmentIndex, toggle: true);
		}
		else
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
		}
	}

	public void OnIncrement()
	{
		if (HasEnoughPoints())
		{
			CharacterCreationMenu.Instance.AddStartingEquipment(StartingEquipmentIndex, toggle: false);
		}
		else
		{
			SoundManager.PlayMenuSound(SoundManager.DenySelectSound);
		}
	}

	public void OnDecrement()
	{
		CharacterCreationMenu.Instance.RemoveStartingEquipment(StartingEquipmentIndex, toggle: false);
	}
}
