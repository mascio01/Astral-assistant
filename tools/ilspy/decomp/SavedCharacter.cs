using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SavedCharacter : IReflectable
{
	public string FirstName = "";

	public string Surname = "";

	private HumanAppearance Appearance = new HumanAppearance();

	public List<SavedEquipment> Inventory = new List<SavedEquipment>();

	public int[] Skills = new int[10];

	public List<string> Personality = new List<string>();

	public SavedCharacter()
	{
	}

	public SavedCharacter(Human character)
	{
		FirstName = character.FirstName;
		Surname = character.Surname;
		Appearance = character.GetAppearance();
		Personality = character.Personality;
		for (int i = 0; i < Skills.Length; i++)
		{
			Skills[i] = character.Skillset.GetLevel((SkillType)i);
		}
		for (int j = 0; j < character.Inventory.Count; j++)
		{
			Equipment item = character.Inventory.GetItem(j);
			SavedEquipment item2 = new SavedEquipment
			{
				Proto = item.GetPrototype(),
				Amount = item.GetAmount(),
				ColorVariation = item.ColorVariation,
				ColorVariation2 = item.ColorVariation2,
				ColorVariation3 = item.ColorVariation3,
				MaterialVariation = item.MaterialVariation
			};
			Inventory.Add(item2);
		}
	}

	public void ApplyToCharacter(Human character, bool includeClothes = false)
	{
		character.SetFirstName(FirstName);
		character.Surname = Surname;
		character.FirstNameVerified = StringStatus.Unverified;
		character.SurnameVerified = StringStatus.Unverified;
		character.Appearance = Appearance;
		character.Personality = Personality;
		character.CachePersonality();
		for (int i = 0; i < Skills.Length; i++)
		{
			character.Skillset.SetLevel(character, (SkillType)i, Skills[i]);
		}
		if (!includeClothes)
		{
			return;
		}
		for (int j = 0; j < Inventory.Count; j++)
		{
			SavedEquipment savedEquipment = Inventory[j];
			if (savedEquipment.Proto != null && savedEquipment.Proto.ClothingType != ClothingType.Invalid)
			{
				Equipment equipment = Equipment.Create(savedEquipment.Proto);
				equipment.ColorVariation = savedEquipment.ColorVariation;
				equipment.ColorVariation2 = savedEquipment.ColorVariation2;
				equipment.ColorVariation3 = savedEquipment.ColorVariation3;
				equipment.MaterialVariation = savedEquipment.MaterialVariation;
				character.Inventory.Add(character, equipment);
				equipment.Wear(character);
			}
		}
	}

	public void Reflect(Reflector reflector)
	{
		reflector.IsDoingSavedCharacter = true;
		reflector.Add(ref reflector.Version);
		if (reflector.Version > 629)
		{
			throw new Exception(GameImpl.Translate("MENU_SaveGameFromFuture").Replace("%1", reflector.Version.ToString()).Replace("%2", 629.ToString()));
		}
		reflector.Add(ref FirstName);
		reflector.Add(ref Surname);
		reflector.Add(Appearance);
		if (reflector.Version >= 188)
		{
			reflector.Add(ref Inventory);
			reflector.AddIntArray(ref Skills);
			reflector.AddStringList(ref Personality);
		}
	}

	public void SaveWithConfirmation(OnSavedFunction onSavedFunction)
	{
		string text = FirstName + " " + Surname;
		if (SaveGameManager.Instance.SavedCharacterNames.Contains(text))
		{
			GameImpl.Instance.ShowConfirmationBox(GameImpl.Translate("MENU_AreYouSureOverwriteFavourite").Replace("%1", text), delegate
			{
				SaveGameManager.Instance.SaveCharacter(this, onSavedFunction);
			});
		}
		else
		{
			SaveGameManager.Instance.SaveCharacter(this, onSavedFunction);
		}
	}

	public void Save(string path, OnSavedFunction onSavedFunction)
	{
		SaveGameManager.Instance.SaveCharacter(this, onSavedFunction);
	}

	public bool Load(string path)
	{
		try
		{
			using Stream stream = SaveGameManager.OpenSaveGameFile(path);
			if (stream != null)
			{
				using (CustomBinaryReader reflector = new CustomBinaryReader(stream))
				{
					Reflect(reflector);
				}
				return true;
			}
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			Debug.Log("Error loading " + path + ": " + ex.ToString());
		}
		return false;
	}
}
