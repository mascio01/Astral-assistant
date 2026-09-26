using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class StorySettings
{
	public ulong SteamWorkshopId;

	public string NativeName;

	public string NativeDescription;

	public bool IsMod;

	[IsMod(false)]
	public bool ProcedurallyGenerated;

	[IsMod(false)]
	public MapSize MapSize = MapSize.Large;

	[IsMod(false)]
	[ProcedurallyGenerated(false)]
	public int StartDayOfYear;

	[IsMod(false)]
	[ProcedurallyGenerated(false)]
	public int StartHourOfDay;

	[IsMod(false)]
	[ProcedurallyGenerated(false)]
	public int ClothesPoints = 6;

	[IsMod(false)]
	[ProcedurallyGenerated(true)]
	public int SummerClothesPoints = 4;

	[IsMod(false)]
	[ProcedurallyGenerated(true)]
	public int WinterClothesPoints = 8;

	[IsMod(false)]
	public int EquipmentPoints = 3;

	[IsMod(false)]
	public int SkillPoints = 6;

	public List<StartingEquipment> StartingEquipmentOptions = new List<StartingEquipment>();

	public List<Loadout> Loadouts = new List<Loadout>();

	public List<DifficultySettings> DifficultySettings = new List<DifficultySettings>();

	public List<StoryId> Dependencies = new List<StoryId>();

	[IsMod(false)]
	public StoryId HitTheRoadStory;

	[IsMod(false)]
	public List<string> HitTheRoadKeepVariables = new List<string>();

	[IsMod(false)]
	public float HitTheRoadMinFuelPercent = 75f;

	[IsMod(false)]
	public float HitTheRoadMaxDamagePercent = 50f;

	[IsMod(false)]
	public string DefaultDifficulty;

	public static string NameKey = "STORY_Title";

	public static string DescriptionKey = "STORY_Description";

	public static StorySettings LoadFromFile(string fileName)
	{
		StorySettings storySettings = null;
		try
		{
			if (File.Exists(fileName))
			{
				using StreamReader textReader = new StreamReader(fileName);
				storySettings = (StorySettings)new XmlSerializer(typeof(StorySettings)).Deserialize(textReader);
				foreach (StartingEquipment startingEquipmentOption in storySettings.StartingEquipmentOptions)
				{
					startingEquipmentOption.Amount = Math.Max(startingEquipmentOption.Amount, 1);
					startingEquipmentOption.Points = Math.Max(startingEquipmentOption.Points, 1);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to load '" + fileName + "': " + ex.ToString());
		}
		if (storySettings == null)
		{
			storySettings = new StorySettings();
		}
		if (storySettings.Dependencies.Count == 0 && !storySettings.IsMod && !fileName.Contains("/BaseStory/Settings.xml") && !fileName.Contains("/UI/Settings.xml"))
		{
			StoryId item = new StoryId
			{
				Folder = "BaseStory"
			};
			storySettings.Dependencies.Add(item);
		}
		return storySettings;
	}

	public bool SaveToFile(string fileName)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				new XmlSerializer(typeof(StorySettings)).Serialize(stream, this);
			}
			GameImpl.Instance.GetCurrentlyEditingStory().BuildTSVFile();
			Script.CopyFileBackToUnityFolder(fileName);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save '" + fileName + "': " + ex.ToString());
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_FileSaveFailed").Replace("%1", ex.Message));
			return false;
		}
	}

	public int CalcClothesPoints(float startDayOfYear)
	{
		if (ProcedurallyGenerated)
		{
			float t = Weather.CalcWinterinessFromDayOfYear(startDayOfYear);
			return (int)(Mathf.Lerp(SummerClothesPoints, WinterClothesPoints, t) + 0.5f);
		}
		return ClothesPoints;
	}

	public int GetEquipmentIndex(EquipmentPrototype proto)
	{
		for (int i = 0; i < StartingEquipmentOptions.Count; i++)
		{
			if (StartingEquipmentOptions[i].Name == proto.Name)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetEquipmentIndex(string name)
	{
		for (int i = 0; i < StartingEquipmentOptions.Count; i++)
		{
			if (StartingEquipmentOptions[i].Name == name)
			{
				return i;
			}
		}
		return -1;
	}

	public StartingEquipment FindStartingEquipment(string name)
	{
		for (int i = 0; i < StartingEquipmentOptions.Count; i++)
		{
			if (StartingEquipmentOptions[i].Name == name)
			{
				return StartingEquipmentOptions[i];
			}
		}
		return null;
	}

	public DifficultySettings GetDifficultySettings(string name)
	{
		foreach (DifficultySettings difficultySetting in DifficultySettings)
		{
			if (difficultySetting.DifficultyName == name)
			{
				return difficultySetting;
			}
		}
		return null;
	}

	public DifficultySettings FindMatchingDifficultySettings(DifficultySettings other)
	{
		foreach (DifficultySettings difficultySetting in DifficultySettings)
		{
			if (difficultySetting.Matches(other))
			{
				return difficultySetting;
			}
		}
		return null;
	}
}
