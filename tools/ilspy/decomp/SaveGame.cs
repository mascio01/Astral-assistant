using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class SaveGame
{
	public SaveGameType Type;

	public string DirName;

	public int GameUniqueId;

	public SaveGameData Data;

	public string JPGFilePath;

	public Resource<Texture2D> Thumbnail;

	public string BuildNameString(ref bool requesting)
	{
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		string text = ((Type == SaveGameType.AutoSave) ? (GameImpl.Translate("MENU_AutoSave") + " ") : ((Type == SaveGameType.AutoSave_Multiplayer) ? (GameImpl.Translate("MENU_AutoSave_Multiplayer") + " ") : ((Type == SaveGameType.QuickSave) ? (GameImpl.Translate("MENU_QuickSave") + " ") : ((Type == SaveGameType.QuickSave_Multiplayer) ? (GameImpl.Translate("MENU_QuickSave_Multiplayer") + " ") : ((Type == SaveGameType.Current) ? (GameImpl.Translate("MENU_CurrentSave") + " ") : ""))))) + Data.Timestamp.ToString("dd MMM yyyy, HH:mm:ss", invariantCulture);
		if (Data.Stories != null && Data.Stories.Count > 0)
		{
			bool flag = true;
			for (int i = 0; i < Data.Stories.Count; i++)
			{
				StorySource storySource = StorySource.FromWorkshopItemOrFolder(Data.Stories[i].WorkshopId, Data.Stories[i].Folder);
				if (storySource.Folder != "BaseStory" && storySource.Folder != "Common")
				{
					string translatedName = storySource.GetTranslatedName(ref requesting);
					if (!string.IsNullOrEmpty(translatedName))
					{
						text = text + (flag ? " - " : ", ") + translatedName;
						flag = false;
					}
				}
			}
		}
		else if (Data.StoryWorkshopId != 0L || !string.IsNullOrEmpty(Data.StoryFolder))
		{
			StorySource storySource2 = StorySource.FromWorkshopItemOrFolder(Data.StoryWorkshopId, Data.StoryFolder);
			if (storySource2 != null)
			{
				text = text + " - " + storySource2.GetTranslatedName(ref requesting);
			}
		}
		return text;
	}

	public List<StorySource> GetStorySources()
	{
		List<StorySource> list = new List<StorySource>();
		if (Data.Stories != null && Data.Stories.Count > 0)
		{
			for (int i = 0; i < Data.Stories.Count; i++)
			{
				list.Add(StorySource.FromWorkshopItemOrFolder(Data.Stories[i].WorkshopId, Data.Stories[i].Folder));
			}
		}
		else
		{
			list.Add(StorySource.FromFolder("BaseStory"));
			list.Add(StorySource.FromWorkshopItemOrFolder(Data.StoryWorkshopId, Data.StoryFolder));
		}
		return list;
	}

	public bool IsTokenSave()
	{
		if (Type != SaveGameType.Token)
		{
			return Type == SaveGameType.Token_Multiplayer;
		}
		return true;
	}
}
