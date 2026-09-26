using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

public class UtilsDebugMenu : DebugMenu
{
	private class SpeechGraphNode
	{
		public Speech Speech;

		public List<SpeechGraphNode> ReplyingTo = new List<SpeechGraphNode>();

		public List<SpeechGraphNode> Replies = new List<SpeechGraphNode>();

		public List<SpeechGraphNode> ContinuingFrom = new List<SpeechGraphNode>();

		public List<SpeechGraphNode> Continues = new List<SpeechGraphNode>();

		public bool Printed;
	}

	public UtilsDebugMenu()
		: base(GameImpl.Translate("DEBUG_Utils"))
	{
		if (!Session.Instance.IsInMultiplayerGame())
		{
			Items.Add(new DebugMenuItemCustom("Reload Story", ReloadStory));
		}
		if (!GameTerrain.Instance.UseFixedTerrain && Session.Instance.RandomSeed != 0)
		{
			Items.Add(new DebugMenuItemCustom("Save World Gen Settings", SaveWorldGenSettings));
		}
		Items.Add(new DebugMenuItemCustom("Generate World from Settings", LoadWorldGenSettings));
		Items.Add(new DebugMenuItemOpenPage("Check Translation", typeof(CheckTranslationMenu)));
		Items.Add(new DebugMenuItemCustom("Print Full Speech Translation", PrintFullTranslation));
		Items.Add(new DebugMenuItemCustom("Print Speech Graph", PrintSpeechGraph));
		Items.Add(new DebugMenuItemCustom("Profiler", OpenProfilerViewer));
		Items.Add(new DebugMenuItemCustom("Crash!", Crash));
		Items.Add(new DebugMenuItemToggle("Network Logging Enabled", () => OnlineParty.NetworkLoggingEnabled, delegate(bool v)
		{
			OnlineParty.NetworkLoggingEnabled = v;
			PlayerPrefs.SetInt("NetworkLoggingEnabled", v ? 1 : 0);
		}));
		Items.Add(new DebugMenuItemToggle("Fake Save Game Fail", () => SaveGameManager.Instance.FakeSaveGameFail, delegate(bool v)
		{
			SaveGameManager.Instance.FakeSaveGameFail = v;
		}));
	}

	public void PrintFullTranslation()
	{
		GameImpl instance = GameImpl.Instance;
		try
		{
			string text = GameImpl.Instance.SaveGamePath + "/Translation Debug";
			string text2 = text + "/Full Speech Translation - " + GameImpl.Instance.Settings.Language.ToString() + ".txt";
			Directory.CreateDirectory(text);
			using (StreamWriter stream = File.CreateText(text2))
			{
				foreach (Story currentStory in instance.CurrentStories)
				{
					currentStory.PrintFullTranslation(stream);
				}
			}
			Application.OpenURL(text2);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public void PrintSpeechGraph()
	{
		GameImpl instance = GameImpl.Instance;
		Dictionary<string, SpeechGraphNode> dictionary = new Dictionary<string, SpeechGraphNode>();
		foreach (Story currentStory in instance.CurrentStories)
		{
			foreach (KeyValuePair<string, Script> script in currentStory.Scripts)
			{
				foreach (Speech speech4 in script.Value.Speeches)
				{
					SpeechGraphNode speechGraphNode = new SpeechGraphNode();
					speechGraphNode.Speech = speech4;
					dictionary[speech4.UniqueID] = speechGraphNode;
				}
			}
		}
		foreach (KeyValuePair<string, SpeechGraphNode> item in dictionary)
		{
			SpeechGraphNode value = item.Value;
			if (value.Speech.Replies != null)
			{
				for (int i = 0; i < value.Speech.Replies.Count; i++)
				{
					Speech speech = value.Speech.Replies[i];
					if (speech != null && dictionary.TryGetValue(speech.UniqueID, out var value2))
					{
						value.Replies.AddUnique(value2);
						value2.ReplyingTo.AddUnique(value);
					}
				}
			}
			if (value.Speech.Continues != null)
			{
				for (int j = 0; j < value.Speech.Continues.Count; j++)
				{
					Speech speech2 = value.Speech.Continues[j];
					if (speech2 != null && dictionary.TryGetValue(speech2.UniqueID, out var value3))
					{
						value.Continues.AddUnique(value3);
						value3.ContinuingFrom.AddUnique(value);
					}
				}
			}
			if (value.Speech.ExtraReplyTo == null)
			{
				continue;
			}
			for (int k = 0; k < value.Speech.ExtraReplyTo.Count; k++)
			{
				Speech speech3 = value.Speech.ExtraReplyTo[k];
				if (speech3 != null && dictionary.TryGetValue(speech3.UniqueID, out var value4) && !value.Replies.Contains(value4))
				{
					value.ReplyingTo.AddUnique(value4);
					value4.Replies.AddUnique(value);
				}
			}
		}
		Story.CreateDummyData(out var fakeCommunities, out var fakeMaleNPCs, out var fakeFemaleNPCs);
		List<Character> list = new List<Character>();
		list.AddRange(fakeMaleNPCs);
		list.AddRange(fakeFemaleNPCs);
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		try
		{
			string text = GameImpl.Instance.SaveGamePath + "/Translation Debug";
			string text2 = text + "/Speech Graph - " + GameImpl.Instance.Settings.Language.ToString() + ".txt";
			Directory.CreateDirectory(text);
			using (StreamWriter streamWriter = File.CreateText(text2))
			{
				foreach (KeyValuePair<string, SpeechGraphNode> item2 in dictionary)
				{
					SpeechGraphNode value5 = item2.Value;
					if (value5.ReplyingTo.Count <= 0 && value5.ContinuingFrom.Count <= 0)
					{
						List<Character> list2 = new List<Character>();
						list.CopyToList(list2);
						Character[] array = new Character[3];
						for (int l = 0; l < 3; l++)
						{
							int index = nonDeterministicRand.Next(list2.Count);
							array[l] = list2[index];
							list2.RemoveAt(index);
						}
						PrintSpeechGraphNode(streamWriter, value5, 0, isInitiator: true, array, fakeCommunities);
						streamWriter.WriteLine("===============================================");
						streamWriter.WriteLine("");
					}
				}
			}
			Application.OpenURL(text2);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	private void PrintSpeechGraphNode(StreamWriter stream, SpeechGraphNode node, int indents, bool isInitiator, Character[] fakeNPCs, Community[] fakeCommunities)
	{
		if (node.Printed)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < indents; i++)
		{
			stringBuilder.Append('>');
		}
		stringBuilder.Append(' ');
		stringBuilder.Append('[');
		stringBuilder.Append(node.Speech.GetTextKey());
		stringBuilder.Append(']');
		stringBuilder.Append(' ');
		string speechTextWithDummyData = Story.GetSpeechTextWithDummyData(node.Speech, (!isInitiator) ? 5 : 0, fakeNPCs, fakeCommunities);
		stringBuilder.Append(speechTextWithDummyData);
		stream.WriteLine(stringBuilder);
		stream.WriteLine("");
		node.Printed = true;
		foreach (SpeechGraphNode @continue in node.Continues)
		{
			PrintSpeechGraphNode(stream, @continue, indents, isInitiator, fakeNPCs, fakeCommunities);
		}
		foreach (SpeechGraphNode reply in node.Replies)
		{
			PrintSpeechGraphNode(stream, reply, indents + 1, !isInitiator, fakeNPCs, fakeCommunities);
		}
	}

	public void SaveSteamAchievementTranslations()
	{
		GameImpl instance = GameImpl.Instance;
		try
		{
			Language language = instance.Settings.Language;
			string text = language.ToString();
			switch (language)
			{
			case Language.SimplifiedChinese:
				text = "schinese";
				break;
			case Language.Korean:
				text = "koreana";
				break;
			case Language.LatinAmericanSpanish:
				text = "latam";
				break;
			}
			string text2 = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/Achievement Translations";
			string text3 = text2 + "/" + language.ToString() + ".vdf";
			Directory.CreateDirectory(text2);
			using (StreamWriter streamWriter = File.CreateText(text3))
			{
				streamWriter.WriteLine("\"lang\"");
				streamWriter.WriteLine("{");
				streamWriter.WriteLine("\t\"Language\"\t\"" + text.ToString().ToLower() + "\"");
				streamWriter.WriteLine("\t\"Tokens\"");
				streamWriter.WriteLine("\t{");
				for (int i = 0; i < 33; i++)
				{
					Achievement achievement = (Achievement)i;
					string text4 = "";
					switch (achievement)
					{
					case Achievement.Recruit_Refugee:
						text4 = "1_0";
						break;
					case Achievement.Recruit_Looter:
						text4 = "1_2";
						break;
					case Achievement.Recruit_Settler:
						text4 = "1_3";
						break;
					case Achievement.GiftAllBooksInSet:
						text4 = "1_4";
						break;
					case Achievement.AdoptAChicken:
						text4 = "1_5";
						break;
					case Achievement.BuildLargeCommunity:
						text4 = "1_6";
						break;
					case Achievement.FormAlliance:
						text4 = "1_7";
						break;
					case Achievement.MakeAFriend:
						text4 = "1_8";
						break;
					case Achievement.StartRelationship:
						text4 = "1_9";
						break;
					case Achievement.PartnersFightOverYou:
						text4 = "1_10";
						break;
					case Achievement.InfectWithFood:
						text4 = "1_11";
						break;
					case Achievement.InfectWithWeapon:
						text4 = "1_12";
						break;
					case Achievement.VehicleKills:
						text4 = "1_14";
						break;
					case Achievement.FindInvisibleStrain:
						text4 = "1_15";
						break;
					case Achievement.Craft_Cookies:
						text4 = "1_16";
						break;
					case Achievement.Craft_Vodka:
						text4 = "1_17";
						break;
					case Achievement.Craft_ArmorPiercingAmmo:
						text4 = "7_3";
						break;
					case Achievement.Story_RitzCreekWar:
						text4 = "1_19";
						break;
					case Achievement.Story_FindRitzvillePasses:
						text4 = "1_20";
						break;
					case Achievement.Story_FindCabinPeople:
						text4 = "1_21";
						break;
					case Achievement.Story_FindBrainScanner:
						text4 = "1_22";
						break;
					case Achievement.Story_DumpTruckKills:
						text4 = "1_23";
						break;
					case Achievement.Story_RecruitJoeWheeler:
						text4 = "1_24";
						break;
					case Achievement.Story_EvacEmma:
						text4 = "1_25";
						break;
					case Achievement.CompleteSandbox_Evac:
						text4 = "1_26";
						break;
					case Achievement.CompleteSandbox_Conquest:
						text4 = "1_27";
						break;
					case Achievement.CompleteSandbox_HitTheRoad:
						text4 = "1_28";
						break;
					case Achievement.CompleteSandbox_Hard_WithFriends:
						text4 = "1_29";
						break;
					case Achievement.CompleteSandbox_Hard_LoneWolf:
						text4 = "1_30";
						break;
					case Achievement.CompleteStory_Evac:
						text4 = "1_31";
						break;
					case Achievement.CompleteStory_SuicideMission:
						text4 = "7_0";
						break;
					case Achievement.CompleteStory_HitTheRoad:
						text4 = "7_1";
						break;
					case Achievement.CompleteStory_Hard:
						text4 = "7_2";
						break;
					}
					streamWriter.WriteLine("\t\t\"NEW_ACHIEVEMENT_" + text4 + "_NAME\"\t\"" + SanitizeString(GameImpl.Translate("ACHIEVEMENT_NAME_" + achievement)) + "\"\t");
					streamWriter.WriteLine("\t\t\"NEW_ACHIEVEMENT_" + text4 + "_DESC\"\t\"" + SanitizeString(GameImpl.Translate("ACHIEVEMENT_DESC_" + achievement)) + "\"\t");
				}
				streamWriter.WriteLine("\t}");
				streamWriter.WriteLine("}");
			}
			Application.OpenURL(text3);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	public void SaveXBoxAchievementTranslations()
	{
		GameImpl instance = GameImpl.Instance;
		try
		{
			string text = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
			Dictionary<Language, Translation> dictionary = new Dictionary<Language, Translation>();
			Dictionary<Language, Translation> dictionary2 = new Dictionary<Language, Translation>();
			for (Language language = Language.English; language < Language.Count; language++)
			{
				if (LanguageMenu.IsTranslationComplete(language))
				{
					Translation value = Translation.LoadFromFile(instance.StreamingAssetsPath + "/UI/" + language.ToString() + ".tsv", justTitleAndDescription: false);
					dictionary[language] = value;
					Translation translation = Translation.LoadFromFile(text + "/Short Translations/" + language.ToString() + ".tsv", justTitleAndDescription: false);
					if (translation != null)
					{
						dictionary2[language] = translation;
					}
				}
			}
			string text2 = text + "/Achievement Translations";
			string text3 = text2 + "/localization.xml";
			Directory.CreateDirectory(text2);
			using (StreamWriter streamWriter = File.CreateText(text3))
			{
				streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				streamWriter.WriteLine("<Localization xmlns=\"http://config.mgt.xboxlive.com/schema/localization/1\">");
				streamWriter.WriteLine("\t<DevDisplayLocale locale=\"en-US\"/>");
				for (int i = 0; i < 33; i++)
				{
					Achievement achievement = (Achievement)i;
					string text4 = "";
					switch (achievement)
					{
					case Achievement.Recruit_Refugee:
						text4 = "54beabab-e4f2-466f-87d8-9e0e0072e9de";
						break;
					case Achievement.Recruit_Looter:
						text4 = "852c436a-f687-4e49-85de-e66e13bfdd29";
						break;
					case Achievement.Recruit_Settler:
						text4 = "c6776e29-cc14-4158-a0bb-1f84b22ee72a";
						break;
					case Achievement.GiftAllBooksInSet:
						text4 = "e2ba5951-d168-4814-8c5a-d5f7c3d9a00a";
						break;
					case Achievement.AdoptAChicken:
						text4 = "ffeb4115-4482-4979-8c38-b87ae8975887";
						break;
					case Achievement.BuildLargeCommunity:
						text4 = "cd6b8412-c03c-4c55-a786-e6616b790bfd";
						break;
					case Achievement.FormAlliance:
						text4 = "bd57a318-7361-4d8d-a26e-37ea16f18d1b";
						break;
					case Achievement.MakeAFriend:
						text4 = "1a412b8c-d269-42b3-acdb-b36937331236";
						break;
					case Achievement.StartRelationship:
						text4 = "7744fe47-f923-40ee-9df5-d9a9e7152e00";
						break;
					case Achievement.PartnersFightOverYou:
						text4 = "9bce8253-663d-4bb3-9ee4-06ebb855ccf5";
						break;
					case Achievement.InfectWithFood:
						text4 = "2f3e0829-e25b-499b-a9f5-d5db0b4bc4eb";
						break;
					case Achievement.InfectWithWeapon:
						text4 = "24016978-1aaa-4d0c-a895-a731597c8f74";
						break;
					case Achievement.VehicleKills:
						text4 = "f42ec4f1-4e87-4570-928b-3258576fe320";
						break;
					case Achievement.FindInvisibleStrain:
						text4 = "8a38cb88-5f8a-48a0-a0e4-930c47cc8729";
						break;
					case Achievement.Craft_Cookies:
						text4 = "8ece5f0c-80fb-4275-926c-b23b93d4f6a2";
						break;
					case Achievement.Craft_Vodka:
						text4 = "96d0b3a8-4c39-4f42-9cf5-c6aa21a14abf";
						break;
					case Achievement.Craft_ArmorPiercingAmmo:
						text4 = "2c90ddee-21c3-4682-87bb-1a0ef3012dbe";
						break;
					case Achievement.Story_RitzCreekWar:
						text4 = "61285b4a-36d6-43b2-97e9-37e8ddf75c76";
						break;
					case Achievement.Story_FindRitzvillePasses:
						text4 = "99f5d5ef-380e-4c2e-ac04-9272fe08d62b";
						break;
					case Achievement.Story_FindCabinPeople:
						text4 = "b59f473d-52db-4009-afbd-b41b110f131b";
						break;
					case Achievement.Story_FindBrainScanner:
						text4 = "2302ad7e-1559-4e1d-a5c6-e06ac7532252";
						break;
					case Achievement.Story_DumpTruckKills:
						text4 = "b709be9e-5032-47a6-ad3c-1b17a29d7ba1";
						break;
					case Achievement.Story_RecruitJoeWheeler:
						text4 = "bfcdbf1d-b6c1-49ee-8a84-b7d0a81ea3d2";
						break;
					case Achievement.Story_EvacEmma:
						text4 = "09e6ab04-51ab-4881-9f85-05ea3c0e818f";
						break;
					case Achievement.CompleteSandbox_Evac:
						text4 = "31e4788f-77aa-489c-89d9-316bbf8f86df";
						break;
					case Achievement.CompleteSandbox_Conquest:
						text4 = "92d08a9e-b608-4639-9876-aca16b5a0049";
						break;
					case Achievement.CompleteSandbox_HitTheRoad:
						text4 = "5bda576d-6754-4ee0-aff1-09595e055dbb";
						break;
					case Achievement.CompleteSandbox_Hard_WithFriends:
						text4 = "6550d5a1-cf09-469f-90f3-a799a202f9c8";
						break;
					case Achievement.CompleteSandbox_Hard_LoneWolf:
						text4 = "f7becb9d-94d5-4f07-a4ef-dfc3ccf2af7e";
						break;
					case Achievement.CompleteStory_Evac:
						text4 = "9eec9c5a-6644-4203-a050-72232cec5ff4";
						break;
					case Achievement.CompleteStory_SuicideMission:
						text4 = "ddb980f6-2926-494b-8160-4965e84e47ae";
						break;
					case Achievement.CompleteStory_HitTheRoad:
						text4 = "4b8e5bc2-c2f0-4092-9a24-ca2a68d8326d";
						break;
					case Achievement.CompleteStory_Hard:
						text4 = "9fef60ee-1b41-4e90-99e4-24f663575b3a";
						break;
					}
					streamWriter.WriteLine("\t<LocalizedString id=\"AchievementNameId_" + text4 + "\">");
					for (Language language2 = Language.English; language2 < Language.Count; language2++)
					{
						string achievementString = GetAchievementString(achievement, language2, dictionary, dictionary2, "ACHIEVEMENT_NAME_", 44);
						if (!string.IsNullOrEmpty(achievementString))
						{
							string codeFromLanguage = LanguageMenu.GetCodeFromLanguage(language2);
							streamWriter.WriteLine("\t\t<Value locale=\"" + codeFromLanguage + "\">" + achievementString + "</Value>");
						}
					}
					streamWriter.WriteLine("\t</LocalizedString>");
					streamWriter.WriteLine("\t<LocalizedString id=\"UnlockedDescriptionId_" + text4 + "\">");
					for (Language language3 = Language.English; language3 < Language.Count; language3++)
					{
						string achievementString2 = GetAchievementString(achievement, language3, dictionary, dictionary2, "ACHIEVEMENT_DESC_", 100);
						if (!string.IsNullOrEmpty(achievementString2))
						{
							string codeFromLanguage2 = LanguageMenu.GetCodeFromLanguage(language3);
							streamWriter.WriteLine("\t\t<Value locale=\"" + codeFromLanguage2 + "\">" + achievementString2 + "</Value>");
						}
					}
					streamWriter.WriteLine("\t</LocalizedString>");
					streamWriter.WriteLine("\t<LocalizedString id=\"LockedDescriptionId_" + text4 + "\">");
					for (Language language4 = Language.English; language4 < Language.Count; language4++)
					{
						string achievementString3 = GetAchievementString(achievement, language4, dictionary, dictionary2, "ACHIEVEMENT_DESC_", 100);
						if (!string.IsNullOrEmpty(achievementString3))
						{
							string codeFromLanguage3 = LanguageMenu.GetCodeFromLanguage(language4);
							streamWriter.WriteLine("\t\t<Value locale=\"" + codeFromLanguage3 + "\">" + achievementString3 + "</Value>");
						}
					}
					streamWriter.WriteLine("\t</LocalizedString>");
				}
				streamWriter.Write("</Localization>");
			}
			Application.OpenURL(text3);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}

	private string SanitizeString(string str)
	{
		if (str == null)
		{
			return string.Empty;
		}
		return str.Replace("\"", "\\\"");
	}

	private string GetAchievementString(Achievement achievement, Language language, Dictionary<Language, Translation> translations, Dictionary<Language, Translation> shortTranslations, string key, int limit)
	{
		if (translations.TryGetValue(language, out var value))
		{
			string text = SanitizeString(value.Translate(key + achievement));
			if (text.Length > limit && shortTranslations.TryGetValue(language, out var value2))
			{
				string text2 = SanitizeString(value2.Translate(key + achievement.ToString() + "_" + limit + "Chars"));
				if (string.IsNullOrEmpty(text2))
				{
					Debug.LogWarning("Too long (" + text.Length + "/" + limit + ") " + key + achievement.ToString() + " in " + language.ToString() + ": " + text);
				}
				else
				{
					text = text2;
					if (text.Length > limit)
					{
						Debug.LogWarning("Short string is still too long (" + text.Length + "/" + limit + ") " + key + achievement.ToString() + " in " + language.ToString() + ": " + text);
					}
				}
			}
			return text;
		}
		return string.Empty;
	}

	private void OpenProfilerViewer()
	{
		OpenChildPage(new ProfilerViewer(this, GameProfilerFolder.Root, -1));
	}

	private void ReloadStory()
	{
		GameImpl.Instance.ReloadCurrentStory(reloadFromDisk: true);
	}

	private void SaveWorldGenSettings()
	{
		Session instance = Session.Instance;
		GameTerrain instance2 = GameTerrain.Instance;
		_ = instance.CommunityManager.PlayerCommunity.Leader;
		SaveWorldGenSettings(new WorldGenSettings
		{
			RandomSeed = instance.RandomSeed,
			StartDayOfYear = instance.Weather.StartDayOfYear,
			StartHourOfDay = instance.Weather.StartHourOfDay,
			MapSize = GameTerrain.GetMapSizeFromSize(instance2.Size),
			DifficultySettings = instance.DifficultySettings.MakeCopy()
		});
	}

	public static void SaveWorldGenSettings(WorldGenSettings worldGenSettings)
	{
		string text = worldGenSettings.RandomSeed.ToString();
		string text2 = string.Concat(GameImpl.Instance.SaveGamePath + "\\WorldGen", "\\", text, ".xml");
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text2));
			using (FileStream stream = File.Create(text2))
			{
				new XmlSerializer(typeof(WorldGenSettings)).Serialize(stream, worldGenSettings);
			}
			GameImpl.Instance.ShowMessageBox("Saved to " + text2);
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			Debug.Log("Error saving " + text2 + ": " + ex.ToString());
		}
	}

	private void LoadWorldGenSettings()
	{
		WorldGenSettings worldGenSettings = LoadWorldGenSettingsFromFile();
		if (worldGenSettings != null)
		{
			CharacterCreationSettings characterCreationSettings = new CharacterCreationSettings();
			characterCreationSettings.RandomSeed = worldGenSettings.RandomSeed;
			characterCreationSettings.StartDayOfYear = worldGenSettings.StartDayOfYear;
			characterCreationSettings.StartHourOfDay = worldGenSettings.StartHourOfDay;
			characterCreationSettings.MapSize = worldGenSettings.MapSize;
			characterCreationSettings.DifficultySettings = worldGenSettings.DifficultySettings.MakeCopy();
			characterCreationSettings.Appearance = null;
			if (Session.Instance.Editor)
			{
				GameImpl.Instance.RunEditor(characterCreationSettings, GameImpl.Instance.GetCurrentStorySources(), forceProcedurallyGenerated: true);
			}
			else
			{
				GameImpl.Instance.NewGame(characterCreationSettings, GameImpl.Instance.GetCurrentStorySources(), forceProcedurallyGenerated: true);
			}
		}
	}

	public static WorldGenSettings LoadWorldGenSettingsFromFile()
	{
		OpenFileName openFileName = new OpenFileName();
		openFileName.structSize = Marshal.SizeOf(openFileName);
		openFileName.filter = "xml files (*.xml)\0*.xml\0All files (*.*)\0*.*\0\0";
		openFileName.file = new string(new char[256]);
		openFileName.maxFile = openFileName.file.Length;
		openFileName.fileTitle = new string(new char[64]);
		openFileName.maxFileTitle = openFileName.fileTitle.Length;
		openFileName.initialDir = GameImpl.Instance.SaveGamePath + "\\WorldGen";
		openFileName.title = "Open World Gen Settings";
		openFileName.defExt = "GEN";
		openFileName.flags = 530440;
		if (DllTest.GetOpenFileName(openFileName))
		{
			try
			{
				using StreamReader textReader = new StreamReader(openFileName.file);
				return (WorldGenSettings)new XmlSerializer(typeof(WorldGenSettings)).Deserialize(textReader);
			}
			catch (Exception ex)
			{
				GameImpl.Instance.ShowMessageBox(ex.Message);
				Debug.Log("Error loading " + openFileName.file + ": " + ex.ToString());
			}
		}
		return null;
	}

	private void Crash()
	{
		((List<int>)null).Add(1);
	}

	private void Foo()
	{
		Debug.LogError("Infinity or NaN floating point numbers appear when calculating the transform matrix for a Collider.  Scene hierarch path blah blah blah");
	}

	private void Bar()
	{
		Debug.developerConsoleVisible = false;
	}
}
