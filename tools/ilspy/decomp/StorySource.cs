using System;
using System.Collections.Generic;
using System.IO;
using Steamworks;

public class StorySource
{
	public enum CachedBool
	{
		Uncached,
		True,
		False,
		FileNotFound
	}

	public string AbsolutePath;

	public string Folder;

	public ulong WorkshopId;

	public string CachedTitle;

	public string CachedDescription;

	public string CachedAuthor;

	public string CachedDateString;

	public float CachedScore = -1f;

	public int CachedNumVotes;

	public List<StorySource> CachedDependencies;

	public CachedBool CachedIsMod;

	public Language CachedLanguage = Language.Invalid;

	private static string Crlf = "\r\n";

	private static string Newline = "\n";

	public static StorySource FromFolder(string folder)
	{
		return new StorySource
		{
			AbsolutePath = GameImpl.Instance.StreamingAssetsPath + "/" + folder,
			Folder = folder,
			WorkshopId = 0uL
		};
	}

	public static StorySource FromDownloadedWorkshopItem(ulong id, string absolutePath)
	{
		StorySource obj = new StorySource
		{
			AbsolutePath = absolutePath
		};
		obj.Folder = Path.GetFileName(obj.AbsolutePath);
		obj.WorkshopId = id;
		return obj;
	}

	public static StorySource FromQueriedWorkshopItem(ulong id)
	{
		return new StorySource
		{
			AbsolutePath = string.Empty,
			Folder = string.Empty,
			WorkshopId = id
		};
	}

	public static StorySource FromWorkshopItemOrFolder(ulong id, string folder)
	{
		if (id != 0L)
		{
			return WorkshopManager.Instance.GetWorkshopItem(id);
		}
		return FromFolder(folder);
	}

	public static StorySource FromStoryId(StoryId storyId)
	{
		return FromWorkshopItemOrFolder(storyId.WorkshopId, storyId.Folder);
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(AbsolutePath);
	}

	public override string ToString()
	{
		if (WorkshopId != 0L)
		{
			return WorkshopId + ((!string.IsNullOrEmpty(CachedTitle)) ? (" (" + CachedTitle + ")") : string.Empty);
		}
		return Folder;
	}

	public bool HasStoryId(StoryId storyId)
	{
		if (WorkshopId == storyId.WorkshopId)
		{
			return Folder == storyId.Folder;
		}
		return false;
	}

	public bool Equals(StorySource other)
	{
		if (AbsolutePath == other.AbsolutePath)
		{
			return WorkshopId == other.WorkshopId;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StorySource))
		{
			return false;
		}
		return this == (StorySource)obj;
	}

	public override int GetHashCode()
	{
		return AbsolutePath.GetHashCode() ^ WorkshopId.GetHashCode();
	}

	private static bool IsFormattingTag(string text)
	{
		if (text.Length > 0 && text[0] == '/')
		{
			text = text.Substring(1);
		}
		switch (text)
		{
		case "b":
		case "u":
		case "h1":
		case "h2":
		case "h3":
			return true;
		default:
			return false;
		}
	}

	private static string StripFormattingTags(string text)
	{
		int num = text.IndexOf('[');
		while (num != -1)
		{
			int num2 = text.IndexOf(']', num);
			if (num2 != -1)
			{
				if (IsFormattingTag(text.Substring(num + 1, num2 - num - 1)))
				{
					text = text.Remove(num, num2 - num + 1);
					num = text.IndexOf('[', num);
				}
				else
				{
					num = text.IndexOf('[', num2 + 1);
				}
			}
			else
			{
				num = text.IndexOf('[', num + 1);
			}
		}
		return text;
	}

	public void LoadStoryTitleAndDescriptionIfNeeded(ref bool requesting)
	{
		if (CachedLanguage == GameImpl.Instance.Settings.Language)
		{
			return;
		}
		CachedTitle = string.Empty;
		CachedDescription = string.Empty;
		GameImpl instance = GameImpl.Instance;
		int key = StringUtil.JenkinsHash(StorySettings.NameKey);
		int key2 = StringUtil.JenkinsHash(StorySettings.DescriptionKey);
		if (string.IsNullOrEmpty(AbsolutePath))
		{
			if (WorkshopId != 0L)
			{
				if (!WorkshopManager.Instance.GetWorkshopItemDetails(new PublishedFileId_t(WorkshopId), out var details))
				{
					requesting = true;
					return;
				}
				CachedTitle = details.m_rgchTitle;
				CachedDescription = details.m_rgchDescription;
			}
		}
		else
		{
			Translation translation = Translation.LoadFromFile(AbsolutePath + "/" + instance.Settings.Language.ToString() + ".tsv", justTitleAndDescription: true);
			if (translation != null)
			{
				translation.Keys.TryGetValue(key, out CachedTitle);
				translation.Keys.TryGetValue(key2, out CachedDescription);
			}
			if (string.IsNullOrEmpty(CachedTitle) && string.IsNullOrEmpty(CachedDescription) && instance.Settings.Language != Language.English)
			{
				translation = Translation.LoadFromFile(AbsolutePath + "/English.tsv", justTitleAndDescription: true);
				if (translation != null)
				{
					translation.Keys.TryGetValue(key, out CachedTitle);
					translation.Keys.TryGetValue(key2, out CachedDescription);
				}
			}
		}
		if (string.IsNullOrEmpty(CachedTitle))
		{
			CachedTitle = Folder;
		}
		if (!string.IsNullOrEmpty(CachedDescription))
		{
			CachedDescription = CachedDescription.Replace(Crlf, Newline);
			if (CachedDescription.Length > 256)
			{
				CachedDescription = CachedDescription.Substring(0, 256);
				CachedDescription += "...";
			}
			int num = -1;
			int num2 = -1;
			do
			{
				num++;
				num2++;
				if (num >= 3)
				{
					CachedDescription = CachedDescription.Substring(0, num2);
					break;
				}
				num2 = CachedDescription.IndexOf('\n', num2);
			}
			while (num2 != -1 && num2 < CachedDescription.Length - 1);
		}
		CachedLanguage = GameImpl.Instance.Settings.Language;
	}

	public string GetTranslatedName()
	{
		bool requesting = false;
		LoadStoryTitleAndDescriptionIfNeeded(ref requesting);
		return CachedTitle;
	}

	public string GetTranslatedName(ref bool requesting)
	{
		LoadStoryTitleAndDescriptionIfNeeded(ref requesting);
		return CachedTitle;
	}

	public string GetTranslatedDescription()
	{
		bool requesting = false;
		LoadStoryTitleAndDescriptionIfNeeded(ref requesting);
		return CachedDescription;
	}

	public string GetAuthor()
	{
		if (string.IsNullOrEmpty(CachedAuthor) && WorkshopId != 0L && WorkshopManager.Instance.GetWorkshopItemDetails(new PublishedFileId_t(WorkshopId), out var details) && !SteamFriends.RequestUserInformation(new CSteamID(details.m_ulSteamIDOwner), bRequireNameOnly: true))
		{
			CachedAuthor = SteamFriends.GetFriendPersonaName(new CSteamID(details.m_ulSteamIDOwner));
		}
		return CachedAuthor;
	}

	public float GetScore(out int numVotes)
	{
		if (CachedScore < 0f && WorkshopId != 0L && WorkshopManager.Instance.GetWorkshopItemDetails(new PublishedFileId_t(WorkshopId), out var details))
		{
			CachedScore = details.m_flScore;
			CachedNumVotes = (int)(details.m_unVotesUp + details.m_unVotesDown);
		}
		numVotes = CachedNumVotes;
		return CachedScore;
	}

	public string GetDateString()
	{
		if (string.IsNullOrEmpty(CachedDateString) && WorkshopId != 0L && WorkshopManager.Instance.GetWorkshopItemDetails(new PublishedFileId_t(WorkshopId), out var details))
		{
			CachedDateString = MathUtil.UnixTimeStampToDateTime(details.m_rtimeUpdated).ToShortDateString();
		}
		return CachedDateString;
	}

	public List<StorySource> GetDependencies()
	{
		if (CachedDependencies == null)
		{
			StorySettings storySettings = StorySettings.LoadFromFile(AbsolutePath + "/Settings.xml");
			CachedDependencies = new List<StorySource>();
			for (int i = 0; i < storySettings.Dependencies.Count; i++)
			{
				StorySource item = FromWorkshopItemOrFolder(storySettings.Dependencies[i].WorkshopId, storySettings.Dependencies[i].Folder);
				CachedDependencies.Add(item);
			}
		}
		return CachedDependencies;
	}

	public bool GetCachedIsMod()
	{
		if (CachedIsMod == CachedBool.Uncached)
		{
			if (string.IsNullOrEmpty(AbsolutePath) && WorkshopId != 0L && WorkshopManager.Instance.GetWorkshopItemDetails(new PublishedFileId_t(WorkshopId), out var details))
			{
				CachedIsMod = (details.m_rgchTags.Contains(WorkshopManager.IsMod) ? CachedBool.True : CachedBool.False);
				return CachedIsMod == CachedBool.True;
			}
			if (!string.IsNullOrEmpty(AbsolutePath))
			{
				StorySettings storySettings = StorySettings.LoadFromFile(AbsolutePath + "/Settings.xml");
				if (storySettings != null)
				{
					CachedIsMod = (storySettings.IsMod ? CachedBool.True : CachedBool.False);
				}
				else
				{
					CachedIsMod = CachedBool.FileNotFound;
				}
			}
		}
		return CachedIsMod == CachedBool.True;
	}

	public static string GetStorySourcesDebugString(List<StorySource> sources)
	{
		string text = string.Empty;
		for (int i = 0; i < sources.Count; i++)
		{
			if (i > 0)
			{
				text += ",";
			}
			text += sources[i].GetTranslatedName();
		}
		return text;
	}

	public bool ContainsSearchText(string searchText)
	{
		if (!string.IsNullOrEmpty(searchText))
		{
			string text = GetTranslatedName();
			string text2 = GetTranslatedDescription();
			string text3 = GetAuthor();
			if (text == null)
			{
				text = string.Empty;
			}
			if (text2 == null)
			{
				text2 = string.Empty;
			}
			if (text3 == null)
			{
				text3 = string.Empty;
			}
			if (!text.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) && !text2.Contains(searchText, StringComparison.CurrentCultureIgnoreCase) && !text3.Contains(searchText, StringComparison.CurrentCultureIgnoreCase))
			{
				return false;
			}
		}
		return true;
	}
}
