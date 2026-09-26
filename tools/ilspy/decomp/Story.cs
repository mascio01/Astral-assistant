using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using UnityEngine;

public class Story
{
	public StorySource StorySource;

	public int StoryHash;

	public Translation EnglishTranslation;

	public Translation ForeignTranslation;

	public StorySettings Settings;

	public NamesList[] Names = new NamesList[3];

	public PersonalitiesList PersonalitiesList;

	public LootLocationsList LootLocationsList;

	public SortedList<string, EquipmentPrototype> EquipmentPrototypes = new SortedList<string, EquipmentPrototype>();

	public SortedList<string, PropPrototype> PropPrototypes = new SortedList<string, PropPrototype>();

	public SortedList<string, LiquidPrototype> LiquidPrototypes = new SortedList<string, LiquidPrototype>();

	public SortedList<string, MemoryPrototype> MemoryPrototypes = new SortedList<string, MemoryPrototype>();

	public SortedList<string, Recipe> Recipes = new SortedList<string, Recipe>();

	public SortedList<string, Script> Scripts = new SortedList<string, Script>();

	public Dictionary<string, BaseScriptObject> ScriptObjectsByUniqueID = new Dictionary<string, BaseScriptObject>();

	public List<Speech>[] SpeechesBySituation = new List<Speech>[172];

	public Dictionary<string, List<Speech>> ExtraRepliesTo = new Dictionary<string, List<Speech>>();

	public List<Quest> QuestsWithStartConditions = new List<Quest>();

	public List<Invader> InvadersWithStartConditions = new List<Invader>();

	public List<Assembly> Assemblies = new List<Assembly>();

	public List<AssetBundle> AssetBundles = new List<AssetBundle>();

	public List<PrefabResource> Prefabs = new List<PrefabResource>();

	public List<Resource<Material>> Materials = new List<Resource<Material>>();

	public List<Resource<AudioClip>> Sounds = new List<Resource<AudioClip>>();

	public PrefabSettings PrefabSettings;

	private static string[] AssetBundlesToLoad = null;

	private static int CurAssetBundleIdx = -1;

	private static AssetBundleCreateRequest CurAssetBundleCreateRequest;

	private static Story WantAssetBundlesLoaded;

	private static Story WantAssetBundlesUnloaded;

	public static int LoadingProgress;

	public static int TotalFilesToLoad;

	private static string[] EmptyList = new string[0];

	private bool ChangedMaxAge;

	public string Path => StorySource.AbsolutePath;

	public Story(StorySource storySource)
	{
		StorySource = storySource;
	}

	public static int CalcStoryHash(string absolutePath)
	{
		if (Directory.Exists(absolutePath))
		{
			StringBuilder stringBuilder = new StringBuilder();
			string[] files = Directory.GetFiles(absolutePath, "*", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				if ((!text.Contains(".xml") && !text.Contains(".tsv") && !text.Contains(".map") && !text.Contains(".mapx") && !text.Contains(".dll") && (!text.Contains("AssetBundles") || ShouldIgnoreAssetBundleFile(text))) || text.Contains(".meta"))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < 25; j++)
				{
					Language language = (Language)j;
					if (language != Language.English && text.Contains(language.ToString() + ".tsv"))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					using Stream stream = File.OpenRead(text);
					byte[] array = new byte[stream.Length];
					stream.Read(array, 0, (int)stream.Length);
					MD5Hash mD5Hash = new MD5Hash(array, array.Length);
					stringBuilder.Append(mD5Hash);
				}
			}
			return StringUtil.JenkinsHash(stringBuilder.ToString());
		}
		return 0;
	}

	private bool LoadAssetBundles(bool threaded)
	{
		if (AssetBundlesToLoad == null)
		{
			CheckVersionFile();
			LoadDLLs();
			if (Directory.Exists(Path + "/AssetBundles"))
			{
				AssetBundlesToLoad = Directory.GetFiles(Path + "/AssetBundles");
				CurAssetBundleIdx = 0;
				if (threaded)
				{
					TotalFilesToLoad += AssetBundlesToLoad.Length;
					Launcher.Instance.WantSuppressExceptionHandler = true;
				}
				else
				{
					string[] assetBundlesToLoad = AssetBundlesToLoad;
					foreach (string text in assetBundlesToLoad)
					{
						if (!ShouldIgnoreAssetBundleFile(text))
						{
							Debug.Log("Loading asset bundle: " + text);
							AssetBundle assetBundle = null;
							try
							{
								Launcher.Instance.WantSuppressExceptionHandler = true;
								assetBundle = AssetBundle.LoadFromFile(text);
								Launcher.Instance.WantSuppressExceptionHandler = false;
							}
							catch (Exception ex)
							{
								Debug.LogError("Error loading Asset Bundle " + text + ": " + ex.Message);
								continue;
							}
							OnLoadedAssetBundle(assetBundle);
						}
					}
					AssetBundlesToLoad = null;
					CurAssetBundleIdx = -1;
				}
			}
		}
		else if (CurAssetBundleCreateRequest != null)
		{
			if (CurAssetBundleCreateRequest.isDone)
			{
				Debug.Log("Finished loading asset bundle: " + AssetBundlesToLoad[CurAssetBundleIdx]);
				OnLoadedAssetBundle(CurAssetBundleCreateRequest.assetBundle);
				CurAssetBundleCreateRequest = null;
				CurAssetBundleIdx++;
				LoadingProgress++;
			}
		}
		else if (CurAssetBundleIdx < AssetBundlesToLoad.Length)
		{
			while (CurAssetBundleIdx < AssetBundlesToLoad.Length && ShouldIgnoreAssetBundleFile(AssetBundlesToLoad[CurAssetBundleIdx]))
			{
				Debug.Log("Ignoring file in asset bundles folder: " + AssetBundlesToLoad[CurAssetBundleIdx]);
				CurAssetBundleIdx++;
				LoadingProgress++;
			}
			if (CurAssetBundleIdx < AssetBundlesToLoad.Length)
			{
				string text2 = AssetBundlesToLoad[CurAssetBundleIdx];
				Debug.Log("Loading Asset Bundle: " + text2);
				try
				{
					CurAssetBundleCreateRequest = AssetBundle.LoadFromFileAsync(text2);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Error loading Asset Bundle " + text2 + ": " + ex2.Message);
				}
			}
		}
		else
		{
			AssetBundlesToLoad = null;
			CurAssetBundleIdx = -1;
			Launcher.Instance.WantSuppressExceptionHandler = false;
		}
		return AssetBundlesToLoad == null;
	}

	private static bool ShouldIgnoreAssetBundleFile(string file)
	{
		return System.IO.Path.GetFileName(file).Contains('.');
	}

	private void OnLoadedAssetBundle(AssetBundle assetBundle)
	{
		if (!(assetBundle != null))
		{
			return;
		}
		string[] allAssetNames = assetBundle.GetAllAssetNames();
		foreach (string text in allAssetNames)
		{
			if (text.EndsWith(".prefab"))
			{
				Prefabs.Add(new PrefabResource(text, assetBundle));
			}
			else if (text.EndsWith(".mat"))
			{
				Materials.Add(new Resource<Material>(text, assetBundle, includeInList: true));
			}
			else if (text.EndsWith(".wav"))
			{
				Sounds.Add(new Resource<AudioClip>(text, assetBundle, includeInList: true));
			}
		}
		AssetBundles.Add(assetBundle);
	}

	private void UnloadAssetBundles()
	{
		foreach (Resource<Material> material in Materials)
		{
			material.UnloadResource();
		}
		Materials.Clear();
		foreach (Resource<AudioClip> sound in Sounds)
		{
			sound.UnloadResource();
		}
		Sounds.Clear();
		foreach (PrefabResource prefab in Prefabs)
		{
			prefab.UnloadResource();
		}
		Prefabs.Clear();
		foreach (AssetBundle assetBundle in AssetBundles)
		{
			assetBundle.Unload(unloadAllLoadedObjects: true);
		}
		AssetBundles.Clear();
		UnloadDLLs();
	}

	public static void LoadAssetBundlesOnMainThreadIfNeeded()
	{
		if (WantAssetBundlesLoaded != null && WantAssetBundlesLoaded.LoadAssetBundles(threaded: true))
		{
			WantAssetBundlesLoaded = null;
		}
		if (WantAssetBundlesUnloaded != null)
		{
			WantAssetBundlesUnloaded.UnloadAssetBundles();
			WantAssetBundlesUnloaded = null;
		}
	}

	public void LoadStoryContent()
	{
		GameImpl instance = GameImpl.Instance;
		string path = Path;
		if (!Directory.Exists(path))
		{
			Settings = new StorySettings();
			return;
		}
		string[] array = (Directory.Exists(path + "/Liquid") ? Directory.GetFiles(path + "/Liquid", "*.xml") : EmptyList);
		string[] array2 = (Directory.Exists(path + "/Equipment") ? Directory.GetFiles(path + "/Equipment", "*.xml") : EmptyList);
		string[] array3 = (Directory.Exists(path + "/Props") ? Directory.GetFiles(path + "/Props", "*.xml") : EmptyList);
		string[] array4 = (Directory.Exists(path + "/Scripts") ? Directory.GetFiles(path + "/Scripts", "*.xml") : EmptyList);
		TotalFilesToLoad = 8 + array.Length + array2.Length + array3.Length + array4.Length;
		if (Util.AmIOnMainThread())
		{
			LoadAssetBundles(threaded: false);
		}
		else
		{
			WantAssetBundlesLoaded = this;
			while (WantAssetBundlesLoaded == this && !GameImpl.Unloading)
			{
				Thread.Sleep(5);
			}
		}
		LoadingProgress++;
		Settings = StorySettings.LoadFromFile(path + "/Settings.xml");
		LoadingProgress++;
		EnglishTranslation = Translation.LoadFromFile(path + "/English.tsv", justTitleAndDescription: false);
		if (EnglishTranslation == null)
		{
			EnglishTranslation = new Translation();
		}
		if (instance.Settings.Language != Language.English)
		{
			ForeignTranslation = Translation.LoadFromFile(path + "/" + instance.Settings.Language.ToString() + ".tsv", justTitleAndDescription: false);
		}
		LoadingProgress++;
		Names[0] = NamesList.LoadFromFile(path + "/MaleNames.tsv");
		Names[1] = NamesList.LoadFromFile(path + "/FemaleNames.tsv");
		Names[2] = NamesList.LoadFromFile(path + "/Surnames.tsv");
		LoadingProgress++;
		PersonalitiesList = PersonalitiesList.LoadFromFile(path + "/Personalities.tsv");
		LootLocationsList = LootLocationsList.LoadFromFile(path + "/LootLocations.tsv");
		LoadingProgress++;
		PrefabSettings = PrefabSettings.LoadFromFile(path + "/PrefabSettings.xml");
		LoadingProgress++;
		if (array != null)
		{
			string[] array5 = array;
			for (int i = 0; i < array5.Length; i++)
			{
				LiquidPrototype liquidPrototype = LiquidPrototype.LoadFromFile(array5[i], EnglishTranslation);
				if (liquidPrototype != null)
				{
					LiquidPrototypes[liquidPrototype.Name] = liquidPrototype;
				}
				LoadingProgress++;
			}
		}
		if (array2 != null)
		{
			string[] array5 = array2;
			for (int i = 0; i < array5.Length; i++)
			{
				EquipmentPrototype equipmentPrototype = EquipmentPrototype.LoadFromFile(array5[i], EnglishTranslation);
				if (equipmentPrototype != null)
				{
					EquipmentPrototypes[equipmentPrototype.Name] = equipmentPrototype;
				}
				LoadingProgress++;
			}
		}
		if (array3 != null)
		{
			string[] array5 = array3;
			for (int i = 0; i < array5.Length; i++)
			{
				PropPrototype propPrototype = PropPrototype.LoadFromFile(array5[i], EnglishTranslation);
				if (propPrototype != null)
				{
					PropPrototypes[propPrototype.Name] = propPrototype;
				}
				LoadingProgress++;
			}
		}
		Recipes = RecipeList.LoadFromFile(path + "/Recipes.xml", EnglishTranslation);
		LoadingProgress++;
		MemoryPrototypes = MemoryList.LoadFromFile(path + "/Memories.xml", EnglishTranslation);
		LoadingProgress++;
		if (array4 != null)
		{
			string[] array5 = array4;
			for (int i = 0; i < array5.Length; i++)
			{
				Script script = Script.LoadFromFile(array5[i], EnglishTranslation, this);
				if (script != null)
				{
					Scripts[script.UniqueID] = script;
				}
				LoadingProgress++;
			}
			foreach (KeyValuePair<string, Script> script2 in Scripts)
			{
				script2.Value.FixupAfterXmlLoad(this);
			}
		}
		OnLoadFinished();
	}

	public void UnloadStoryContent()
	{
		if (Util.AmIOnMainThread())
		{
			UnloadAssetBundles();
		}
		else
		{
			WantAssetBundlesUnloaded = this;
			while (WantAssetBundlesUnloaded == this)
			{
				Thread.Sleep(5);
			}
		}
		LoadingProgress = 0;
		TotalFilesToLoad = 0;
	}

	public void ReloadContent(bool reloadFromDisk)
	{
		GameImpl instance = GameImpl.Instance;
		if (reloadFromDisk)
		{
			if (PrefabSettings != null)
			{
				PrefabSettings.Reload();
			}
			foreach (KeyValuePair<string, LiquidPrototype> liquidPrototype2 in LiquidPrototypes)
			{
				LiquidPrototype liquidPrototype = LiquidPrototype.LoadFromFile(Path + "/Liquid/" + liquidPrototype2.Key + ".xml", EnglishTranslation);
				if (liquidPrototype != null)
				{
					bool discovered = liquidPrototype.Discovered;
					liquidPrototype2.Value.CopyFrom(liquidPrototype);
					liquidPrototype.Discovered = discovered;
				}
			}
			foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype2 in EquipmentPrototypes)
			{
				EquipmentPrototype equipmentPrototype = EquipmentPrototype.LoadFromFile(Path + "/Equipment/" + equipmentPrototype2.Key + ".xml", EnglishTranslation);
				if (equipmentPrototype != null)
				{
					bool discovered2 = equipmentPrototype.Discovered;
					equipmentPrototype2.Value.CopyFrom(equipmentPrototype);
					equipmentPrototype.Discovered = discovered2;
				}
			}
			foreach (KeyValuePair<string, PropPrototype> propPrototype2 in PropPrototypes)
			{
				PropPrototype propPrototype = PropPrototype.LoadFromFile(Path + "/Props/" + propPrototype2.Key + ".xml", EnglishTranslation);
				if (propPrototype != null)
				{
					bool discovered3 = propPrototype.Discovered;
					propPrototype2.Value.CopyFrom(propPrototype);
					propPrototype.Discovered = discovered3;
				}
			}
			foreach (KeyValuePair<string, Recipe> item in RecipeList.LoadFromFile(Path + "/Recipes.xml", EnglishTranslation))
			{
				if (Recipes.TryGetValue(item.Key, out var value))
				{
					bool shownDiscoveredNotification = value.ShownDiscoveredNotification;
					value.CopyFrom(item.Value);
					value.ShownDiscoveredNotification = shownDiscoveredNotification;
				}
				else
				{
					Recipes[item.Key] = item.Value;
				}
			}
			foreach (KeyValuePair<string, MemoryPrototype> item2 in MemoryList.LoadFromFile(Path + "/Memories.xml", EnglishTranslation))
			{
				if (MemoryPrototypes.TryGetValue(item2.Key, out var value2))
				{
					value2.CopyFrom(item2.Value);
				}
				else
				{
					MemoryPrototypes[item2.Key] = item2.Value;
				}
			}
		}
		EnglishTranslation = Translation.LoadFromFile(Path + "/English.tsv", justTitleAndDescription: false);
		if (EnglishTranslation == null)
		{
			EnglishTranslation = new Translation();
		}
		if (instance.Settings.Language != Language.English)
		{
			ForeignTranslation = Translation.LoadFromFile(Path + "/" + instance.Settings.Language.ToString() + ".tsv", justTitleAndDescription: false);
		}
		OnLoadFinished();
	}

	public bool TryTranslate(int hash, out string result, bool englishOnly)
	{
		if (!englishOnly && ForeignTranslation != null && ForeignTranslation.Keys.TryGetValue(hash, out result) && !string.IsNullOrEmpty(result))
		{
			return true;
		}
		if (EnglishTranslation != null && EnglishTranslation.Keys.TryGetValue(hash, out result) && !string.IsNullOrEmpty(result))
		{
			return true;
		}
		result = null;
		return false;
	}

	public EquipmentPrototype FindEquipmentPrototypeByName(string name)
	{
		if (!EquipmentPrototypes.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public LiquidPrototype FindLiquidPrototypeByName(string name)
	{
		if (!LiquidPrototypes.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public PropPrototype FindPropPrototypeByName(string name)
	{
		if (!PropPrototypes.TryGetValue(name, out var value))
		{
			return null;
		}
		return value;
	}

	public MemoryPrototype FindMemoryPrototypeByUniqueID(string uniqueID)
	{
		if (!MemoryPrototypes.TryGetValue(uniqueID, out var value))
		{
			return null;
		}
		return value;
	}

	public Recipe FindRecipeByUniqueID(string uniqueID)
	{
		if (!Recipes.TryGetValue(uniqueID, out var value))
		{
			return null;
		}
		return value;
	}

	public void RegisterScriptObject(BaseScriptObject obj)
	{
		if (!string.IsNullOrEmpty(obj.UniqueID))
		{
			if (ScriptObjectsByUniqueID.ContainsKey(obj.UniqueID))
			{
				Debug.LogError("Duplicate unique id! " + obj.UniqueID);
				return;
			}
			ScriptObjectsByUniqueID[obj.UniqueID] = obj;
			RegisterExtraReply(obj);
		}
	}

	public void UnregisterScriptObject(BaseScriptObject obj)
	{
		UnregisterExtraReply(obj);
		if (!string.IsNullOrEmpty(obj.UniqueID))
		{
			ScriptObjectsByUniqueID.Remove(obj.UniqueID);
		}
	}

	public void RegisterExtraReply(BaseScriptObject obj)
	{
		if (!(obj is Speech { ExtraReplyTo: not null } speech))
		{
			return;
		}
		for (int i = 0; i < speech.ExtraReplyTo.Count; i++)
		{
			string uniqueID = speech.ExtraReplyTo[i].UniqueID;
			if (!string.IsNullOrEmpty(uniqueID))
			{
				if (!ExtraRepliesTo.ContainsKey(uniqueID))
				{
					ExtraRepliesTo[uniqueID] = new List<Speech>();
				}
				ExtraRepliesTo[uniqueID].Add(speech);
			}
		}
	}

	public void UnregisterExtraReply(BaseScriptObject obj)
	{
		if (!(obj is Speech { ExtraReplyTo: not null } speech))
		{
			return;
		}
		for (int i = 0; i < speech.ExtraReplyTo.Count; i++)
		{
			string uniqueID = speech.ExtraReplyTo[i].UniqueID;
			if (!string.IsNullOrEmpty(uniqueID) && ExtraRepliesTo.ContainsKey(uniqueID))
			{
				ExtraRepliesTo[uniqueID].Remove(speech);
				if (ExtraRepliesTo[uniqueID].Count == 0)
				{
					ExtraRepliesTo.Remove(uniqueID);
				}
			}
		}
	}

	public BaseScriptObject FindScriptObjectByUniqueID(string uniqueID)
	{
		if (string.IsNullOrEmpty(uniqueID))
		{
			return null;
		}
		BaseScriptObject value = null;
		ScriptObjectsByUniqueID.TryGetValue(uniqueID, out value);
		return value;
	}

	public Speech FindSpeechByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as Speech;
	}

	public Trigger FindTriggerByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as Trigger;
	}

	public ConditionBlock FindConditionBlockByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as ConditionBlock;
	}

	public Quest FindQuestByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as Quest;
	}

	public QuestGroup FindQuestGroupByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as QuestGroup;
	}

	public Template FindTemplateByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as Template;
	}

	public Invader FindInvaderByUniqueID(string uniqueID)
	{
		return FindScriptObjectByUniqueID(uniqueID) as Invader;
	}

	public void OnLoadFinished()
	{
		for (int i = 0; i < SpeechesBySituation.Length; i++)
		{
			SpeechesBySituation[i] = new List<Speech>();
		}
		QuestsWithStartConditions.Clear();
		InvadersWithStartConditions.Clear();
		foreach (KeyValuePair<string, Script> script in Scripts)
		{
			foreach (Speech speech in script.Value.Speeches)
			{
				if (speech.Situation != SpeechSituation.None)
				{
					SpeechesBySituation[(int)speech.Situation].Add(speech);
				}
			}
			foreach (Quest quest in script.Value.Quests)
			{
				if (quest.StartConditions != null && quest.StartConditions.Count > 0)
				{
					QuestsWithStartConditions.Add(quest);
				}
			}
			foreach (Invader invader in script.Value.Invaders)
			{
				if (invader.StartConditions != null && invader.StartConditions.Count > 0)
				{
					InvadersWithStartConditions.Add(invader);
				}
			}
		}
		StoryHash = CalcStoryHash(Path);
	}

	public bool IsLanguageSupported(Language language)
	{
		return File.Exists(Path + "/" + language.ToString() + ".tsv");
	}

	public void BuildTranslatedTSVFile(Language language)
	{
		string text = Path + "/" + language.ToString() + ".tsv";
		string path = Path + "/English.tsv";
		try
		{
			using StreamWriter stream = File.CreateText(text);
			PrintTSVLine(stream, "Tag", "Translation", "English");
			using StreamReader streamReader = new StreamReader(path);
			int num = 0;
			while (!streamReader.EndOfStream)
			{
				string[] array = streamReader.ReadLine().Split('\t');
				if (array.Length >= 2)
				{
					string text2 = array[0].Trim();
					string native = array[1];
					if (num == 0 && text2 == Translation.Tag)
					{
						continue;
					}
					string translated = ForeignTranslation.Translate(text2);
					PrintTSVLine(stream, text2, translated, native);
				}
				num++;
			}
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			Debug.Log("Error saving " + text + ": " + ex.ToString());
		}
	}

	public void BuildTSVFile()
	{
		BuildTSVFile(wantDialog: false);
	}

	public void BuildTSVFile(bool wantDialog)
	{
		string text = Path + "/English.tsv";
		string path = Path + "/TerrainText.tsv";
		try
		{
			using (StreamWriter streamWriter = File.CreateText(text))
			{
				PrintTSVLine(streamWriter, "Tag", "Translation");
				PrintTSVLine(streamWriter, StorySettings.NameKey, Settings.NativeName);
				PrintTSVLine(streamWriter, StorySettings.DescriptionKey, Settings.NativeDescription);
				foreach (DifficultySettings difficultySetting in Settings.DifficultySettings)
				{
					PrintTSVLine(streamWriter, difficultySetting.GetNameKey(), difficultySetting.NativeName);
					PrintTSVLine(streamWriter, difficultySetting.GetDescriptionKey(), difficultySetting.NativeDescription);
				}
				foreach (Loadout loadout in Settings.Loadouts)
				{
					PrintTSVLine(streamWriter, loadout.GetNameKey(), loadout.NativeName);
					PrintTSVLine(streamWriter, loadout.GetDescriptionKey(), loadout.NativeDescription);
				}
				streamWriter.WriteLine();
				bool flag = false;
				foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in EquipmentPrototypes)
				{
					PrintTSVLine(streamWriter, equipmentPrototype.Value.GetNameKey(), equipmentPrototype.Value.NativeName);
					PrintTSVLine(streamWriter, equipmentPrototype.Value.GetDescriptionKey(), equipmentPrototype.Value.NativeDescription);
					flag = true;
				}
				if (flag)
				{
					streamWriter.WriteLine();
					flag = false;
				}
				foreach (KeyValuePair<string, LiquidPrototype> liquidPrototype in LiquidPrototypes)
				{
					PrintTSVLine(streamWriter, liquidPrototype.Value.GetNameKey(), liquidPrototype.Value.NativeName);
					PrintTSVLine(streamWriter, liquidPrototype.Value.GetDescriptionKey(), liquidPrototype.Value.NativeDescription);
					flag = true;
				}
				if (flag)
				{
					streamWriter.WriteLine();
					flag = false;
				}
				List<string> list = new List<string>();
				List<string> list2 = new List<string>();
				foreach (KeyValuePair<string, PropPrototype> propPrototype in PropPrototypes)
				{
					PrintTSVLine(streamWriter, propPrototype.Value.GetNameKey(), propPrototype.Value.NativeName);
					if (propPrototype.Value.Entrances != null)
					{
						for (int i = 0; i < propPrototype.Value.Entrances.Length; i++)
						{
							if (!string.IsNullOrEmpty(propPrototype.Value.Entrances[i].NativeName) && !list.Contains(propPrototype.Value.Entrances[i].NativeName))
							{
								list.Add(propPrototype.Value.Entrances[i].NativeName);
							}
						}
					}
					if (propPrototype.Value.Inhabitants != null)
					{
						for (int j = 0; j < propPrototype.Value.Inhabitants.Length; j++)
						{
							if (!string.IsNullOrEmpty(propPrototype.Value.Inhabitants[j].NativeName) && !list2.Contains(propPrototype.Value.Inhabitants[j].NativeName))
							{
								list2.Add(propPrototype.Value.Inhabitants[j].NativeName);
							}
						}
					}
					flag = true;
				}
				foreach (string item in list)
				{
					PrintTSVLine(streamWriter, "ENTRANCE_" + item.Replace(" ", ""), item);
					flag = true;
				}
				foreach (string item2 in list2)
				{
					PrintTSVLine(streamWriter, "SLOT_" + item2.Replace(" ", ""), item2);
					flag = true;
				}
				if (flag)
				{
					streamWriter.WriteLine();
					flag = false;
				}
				foreach (KeyValuePair<string, MemoryPrototype> memoryPrototype in MemoryPrototypes)
				{
					PrintTSVLine(streamWriter, memoryPrototype.Value.GetDescriptionKey(), memoryPrototype.Value.NativeDescription);
					flag = true;
				}
				if (flag)
				{
					streamWriter.WriteLine();
					flag = false;
				}
				foreach (KeyValuePair<string, Recipe> recipe in Recipes)
				{
					PrintTSVLine(streamWriter, recipe.Value.GetNameKey(), recipe.Value.NativeName);
					flag = true;
				}
				if (flag)
				{
					streamWriter.WriteLine();
					flag = false;
				}
				foreach (KeyValuePair<string, Script> script in Scripts)
				{
					foreach (QuestGroup questGroup in script.Value.QuestGroups)
					{
						PrintTSVLine(streamWriter, questGroup.GetTitleKey(), questGroup.NativeTitle);
						flag = true;
					}
					foreach (Quest quest in script.Value.Quests)
					{
						PrintTSVLine(streamWriter, quest.GetDescriptionKey(), quest.NativeDescription);
						flag = true;
					}
					if (flag)
					{
						streamWriter.WriteLine();
						flag = false;
					}
					foreach (Invader invader in script.Value.Invaders)
					{
						PrintTSVLine(streamWriter, invader.GetDescriptionKey(), invader.NativeDescription);
						flag = true;
					}
					foreach (Template template in script.Value.Templates)
					{
						PrintTSVLine(streamWriter, template.GetCommunityNameKey(), template.NativeCommunityName);
						flag = true;
					}
					if (flag)
					{
						streamWriter.WriteLine();
						flag = false;
					}
					foreach (Speech speech in script.Value.Speeches)
					{
						PrintTSVLine(streamWriter, speech.GetTextKey(), speech.NativeText);
						flag = true;
					}
					if (flag)
					{
						streamWriter.WriteLine();
						flag = false;
					}
				}
				if (File.Exists(path))
				{
					using StreamReader streamReader = new StreamReader(path);
					while (!streamReader.EndOfStream)
					{
						string text2 = streamReader.ReadLine();
						if (!text2.StartsWith(Translation.Tag + "\t"))
						{
							streamWriter.WriteLine(text2);
						}
					}
				}
				for (int k = 0; k < 3; k++)
				{
					if (Names[k] == null)
					{
						continue;
					}
					foreach (string name in Names[k].Names)
					{
						PrintTSVLine(streamWriter, name, "");
						if (k == 2)
						{
							PrintTSVLine(streamWriter, name + "F", "");
						}
						flag = true;
					}
					if (flag)
					{
						streamWriter.WriteLine();
						flag = false;
					}
				}
			}
			if (wantDialog)
			{
				GameImpl.Instance.ShowMessageBox("Saved " + text);
			}
			Debug.Log("Saved " + text);
			StorySource.CachedLanguage = Language.Invalid;
			Script.CopyFileBackToUnityFolder(text);
		}
		catch (Exception ex)
		{
			GameImpl.Instance.ShowMessageBox(ex.Message);
			Debug.Log("Error saving " + text + ": " + ex.ToString());
		}
		SaveVersionFile();
	}

	private static void SanitiseStringForTSVFile(ref string str)
	{
		if (str == null)
		{
			str = "";
		}
		str = str.Replace('\t', ' ');
		str = str.Replace('\n', ' ');
		str = str.Replace('"', '\'');
	}

	private static void PrintTSVLine(StreamWriter stream, string key, string native)
	{
		SanitiseStringForTSVFile(ref native);
		stream.WriteLine(key + "\t" + native);
	}

	private static void PrintTSVLine(StreamWriter stream, string key, string translated, string native)
	{
		SanitiseStringForTSVFile(ref translated);
		SanitiseStringForTSVFile(ref native);
		stream.WriteLine(key + "\t" + translated + "\t" + native);
	}

	public static int CheckTranslation(string path, Language language, bool fixup)
	{
		int num = 0;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
		List<string[]> list = new List<string[]>();
		string path2 = path + "/" + language.ToString() + ".tsv";
		string path3 = path + "/English.tsv";
		if (File.Exists(path2))
		{
			using StreamReader streamReader = new StreamReader(path2);
			int num2 = 0;
			while (!streamReader.EndOfStream)
			{
				string[] array = streamReader.ReadLine().Split('\t');
				if (fixup)
				{
					list.Add(array);
				}
				if (array.Length >= 2)
				{
					string text = array[0].Trim();
					string value = array[1];
					string value2 = ((array.Length >= 3) ? array[2] : "");
					if (!string.IsNullOrEmpty(value))
					{
						if (num2 == 0 && text == Translation.Tag)
						{
							continue;
						}
						dictionary[text] = value;
						dictionary2[text] = value2;
					}
				}
				num2++;
			}
		}
		if (File.Exists(path3))
		{
			using StreamReader streamReader2 = new StreamReader(path3);
			int num3 = 0;
			while (!streamReader2.EndOfStream)
			{
				string[] array2 = streamReader2.ReadLine().Split('\t');
				if (array2.Length >= 2)
				{
					string text2 = array2[0].Trim();
					string value3 = array2[1];
					if (!string.IsNullOrEmpty(value3))
					{
						if (num3 == 0 && text2 == Translation.Tag)
						{
							continue;
						}
						dictionary3[text2] = value3;
					}
				}
				num3++;
			}
		}
		foreach (KeyValuePair<string, string> item in dictionary3)
		{
			string key = item.Key;
			string value4 = item.Value;
			if (key == "SPEECH_The")
			{
				continue;
			}
			if (!dictionary.TryGetValue(key, out var value5))
			{
				if (!key.StartsWith("EDITOR_") && !key.StartsWith("DEBUG_"))
				{
					Debug.LogError(Launcher.Log + "Missing Line: " + key);
					num++;
				}
				continue;
			}
			if (!dictionary2.TryGetValue(key, out var value6) || value6 != value4)
			{
				Debug.LogError(Launcher.Log + "English translation has changed for " + key + " from '" + value6 + "' to '" + value4 + "'");
				num++;
				continue;
			}
			if (value5 == null || value5.Length == 0)
			{
				Debug.LogError(Launcher.Log + "Empty Line: " + key);
				num++;
				continue;
			}
			if (value4.Length > 0 && value4[0] == ' ' && (value5.Length == 0 || value5[0] != ' '))
			{
				Debug.LogError(Launcher.Log + "Missing space at beginning: " + key);
				num++;
			}
			else if (value4.Length > 0 && value4[value4.Length - 1] == ' ' && (value5.Length == 0 || value5[value5.Length - 1] != ' '))
			{
				Debug.LogError(Launcher.Log + "Missing space at end: " + key);
				num++;
			}
			else if (value4.Length > 0 && value4[0] == '\'' && value4[value4.Length - 1] == '\'' && value5.Length > 0 && value5[0] != '\'' && value5[value5.Length - 1] == '\'')
			{
				Debug.LogError(Launcher.Log + "Missing single quote at beginning: " + key);
				num++;
			}
			for (int i = 0; i < value5.Length; i++)
			{
				if (value5[i] == '%')
				{
					for (int j = i + 1; j < value5.Length; j++)
					{
						if (value5[j] >= '1' && value5[j] <= '9')
						{
							if (j != i + 1)
							{
								Debug.LogError(Launcher.Log + "Space between % and " + value5[j] + ": " + key);
								num++;
							}
							break;
						}
						if (value5[j] != ' ')
						{
							break;
						}
					}
				}
				if (value5[i] != '[')
				{
					continue;
				}
				for (int k = 47; k < 56; k++)
				{
					if (value5.IndexOf(StringUtil.FuncName[k], i + 1) != i + 1 || value5.Length <= i + 1 + StringUtil.FuncName[k].Length + 1 || value5[i + 1 + StringUtil.FuncName[k].Length] != '(')
					{
						continue;
					}
					int num4 = value5[i + 1 + StringUtil.FuncName[k].Length + 1] - 48;
					if (key.StartsWith("MEMORY_"))
					{
						Debug.LogError(Launcher.Log + "Should use " + StringUtil.FuncName[k] + "Full for " + key + " param " + num4);
						num++;
					}
					else
					{
						if (!key.StartsWith("SPEECH_"))
						{
							continue;
						}
						Speech speech = GameImpl.Instance.FindSpeechByUniqueID(key.Replace("SPEECH_", ""));
						if (speech != null && num4 <= speech.Params.Count)
						{
							SpeechParamType type = speech.Params[num4 - 1].Type;
							if (type == SpeechParamType.FullName || (uint)(type - 19) <= 2u)
							{
								Debug.LogError(Launcher.Log + "Should use " + StringUtil.FuncName[k] + "Full for " + key + " param " + num4);
								num++;
							}
						}
					}
				}
				if (!key.StartsWith("QUEST"))
				{
					continue;
				}
				int num5 = value5.IndexOf(']', i);
				if (num5 != -1)
				{
					int num6 = num5 - i;
					if (value5.IndexOf("speaker", i, num6) != -1)
					{
						Debug.LogError(Launcher.Log + "Quest uses speaker for " + key + ": " + value5.Substring(i, num6 + 1));
						num++;
					}
					if (value5.IndexOf("listener", i, num6) != -1)
					{
						Debug.LogError(Launcher.Log + "Quest uses listener for " + key + ": " + value5.Substring(i, num6 + 1));
						num++;
					}
					if (value5.IndexOf("thirdperson", i, num6) != -1)
					{
						Debug.LogError(Launcher.Log + "Quest uses thirdperson for " + key + ": " + value5.Substring(i, num6 + 1));
						num++;
					}
					if (value5.IndexOf("referringTo", i, num6) != -1)
					{
						Debug.LogError(Launcher.Log + "Quest uses referringTo for " + key + ": " + value5.Substring(i, num6 + 1));
						num++;
					}
				}
			}
			for (int l = 0; l < value4.Length; l++)
			{
				switch (value4[l])
				{
				case '/':
				case '8':
				case ':':
				case '{':
				case '}':
				{
					for (int m = 0; m < SpeechEmoticon.Emoticons.Length; m++)
					{
						string text3 = SpeechEmoticon.Emoticons[m];
						if (value4.Length < l + text3.Length || !(value4.Substring(l, text3.Length) == text3))
						{
							continue;
						}
						if (value5.IndexOf(text3) == -1)
						{
							Debug.LogError(Launcher.Log + "Emoticon not found: " + key + " " + text3);
							num++;
							if (fixup && l == 0)
							{
								value5 = (dictionary[key] = text3 + " " + value5);
							}
						}
						l += text3.Length - 1;
						break;
					}
					break;
				}
				case '[':
				{
					for (int n = 0; n < InputFunctionManager.Name.Length; n++)
					{
						string text5 = "[" + InputFunctionManager.Name[n] + "]";
						if (value4.Length < l + text5.Length || !(value4.Substring(l, text5.Length) == text5))
						{
							continue;
						}
						if (value5.IndexOf(text5) == -1)
						{
							Debug.LogError(Launcher.Log + "Button icon not found: " + key + " " + text5);
							num++;
							if (fixup)
							{
								int num7 = value5.IndexOf('[');
								int num8 = value5.IndexOf(']');
								if (num7 != -1 && num8 != -1 && value5.LastIndexOf('[') == num7 && value5.LastIndexOf(']') == num8)
								{
									dictionary[key] = value5.Substring(0, num7) + text5 + value5.Substring(num8 + 1);
								}
							}
						}
						l += text5.Length - 1;
						break;
					}
					break;
				}
				case '%':
					if (l + 1 < value4.Length && value4[l + 1] >= '1' && value4[l + 1] <= '9')
					{
						char c = value4[l + 1];
						if (value5.IndexOf("%" + c) == -1 && value5.IndexOf(c + "%") != -1)
						{
							Debug.LogError(Launcher.Log + "Parameter may have been swapped around: " + key + " " + c + "% for %" + c);
							num++;
						}
					}
					break;
				}
			}
			if (key != "HUD_Lie" && key != "HUD_RolePaused" && !key.StartsWith("HUD_Set") && !key.StartsWith("HUD_Cancel") && !key.StartsWith("HUD_Pause") && !key.StartsWith("HUD_Resume"))
			{
				string text6 = value5;
				for (int num9 = 0; num9 < SpeechEmoticon.Emoticons.Length; num9++)
				{
					string oldValue = SpeechEmoticon.Emoticons[num9];
					text6 = text6.Replace(oldValue, string.Empty);
				}
				for (int num10 = 0; num10 < InputFunctionManager.Name.Length; num10++)
				{
					string oldValue2 = "[" + InputFunctionManager.Name[num10] + "]";
					text6 = text6.Replace(oldValue2, string.Empty);
				}
				text6 = text6.Replace("[M]", string.Empty);
				text6 = text6.Replace("[F]", string.Empty);
				text6 = text6.Replace("[N]", string.Empty);
				text6 = text6.Replace("[P]", string.Empty);
				text6 = text6.Replace("[Swap]", string.Empty);
				text6 = StringUtil.ApplyFormulae(text6);
				if (text6.IndexOf('[') != -1 || text6.IndexOf(']') != -1)
				{
					Debug.LogError(Launcher.Log + "Possible invalid formula in: " + key + " '" + text6 + "'");
					num++;
				}
			}
		}
		if (fixup)
		{
			try
			{
				using StreamWriter streamWriter = File.CreateText(path2);
				foreach (string[] item2 in list)
				{
					for (int num11 = 0; num11 < item2.Length; num11++)
					{
						if (num11 > 0)
						{
							streamWriter.Write('\t');
						}
						if (num11 == 1)
						{
							string key2 = item2[0].Trim();
							dictionary.TryGetValue(key2, out item2[num11]);
						}
						streamWriter.Write(item2[num11]);
					}
					streamWriter.WriteLine();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex.Message);
			}
		}
		return num;
	}

	private static Character SetupFakeNPC(Community fakeCommunity, GenderType gender, string firstName, string surname)
	{
		Human human = new Human();
		human.Community = (human.InitialCommunity = fakeCommunity);
		if (fakeCommunity.Leader == null)
		{
			fakeCommunity.Leader = human;
			fakeCommunity.LastLeaderName = fakeCommunity.Leader.FirstName;
		}
		fakeCommunity.Members.Add(human);
		human.Appearance = new HumanAppearance();
		human.Appearance.Gender = gender;
		human.SetFirstName(firstName);
		human.Surname = surname;
		human.NameKnown = true;
		return human;
	}

	public static void CreateDummyData(out Community[] fakeCommunities, out Character[] fakeMaleNPCs, out Character[] fakeFemaleNPCs)
	{
		GameImpl instance = GameImpl.Instance;
		CustomRandom nonDeterministicRand = MathUtil.NonDeterministicRand;
		Community community = new Community();
		Community community2 = new Community();
		Community community3 = new Community();
		community.CommunityName.Randomise(nonDeterministicRand, unique: false, community);
		community2.CommunityName.Randomise(nonDeterministicRand, unique: false, community2);
		community3.CommunityName.Randomise(nonDeterministicRand, unique: false, community3);
		fakeCommunities = new Community[3] { community, community2, community3 };
		fakeMaleNPCs = new Character[3]
		{
			SetupFakeNPC(community, GenderType.Male, instance.PickRandomName(NameType.MaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand)),
			SetupFakeNPC(community3, GenderType.Male, instance.PickRandomName(NameType.MaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand)),
			SetupFakeNPC(community3, GenderType.Male, instance.PickRandomName(NameType.MaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand))
		};
		fakeFemaleNPCs = new Character[3]
		{
			SetupFakeNPC(community2, GenderType.Female, instance.PickRandomName(NameType.FemaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand)),
			SetupFakeNPC(community3, GenderType.Female, instance.PickRandomName(NameType.FemaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand)),
			SetupFakeNPC(community3, GenderType.Female, instance.PickRandomName(NameType.FemaleFirstName, nonDeterministicRand), instance.PickRandomName(NameType.Surname, nonDeterministicRand))
		};
		Relationship.SetRelationship(fakeMaleNPCs[0], RelationshipType.InLoveWith, fakeFemaleNPCs[0], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeMaleNPCs[1], RelationshipType.SleepingWith, fakeFemaleNPCs[1], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeMaleNPCs[2], RelationshipType.MarriedTo, fakeFemaleNPCs[2], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeMaleNPCs[2], RelationshipType.SiblingOf, fakeMaleNPCs[1], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeMaleNPCs[2], RelationshipType.ParentOf, fakeMaleNPCs[0], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeMaleNPCs[2], RelationshipType.ParentOf, fakeFemaleNPCs[0], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeFemaleNPCs[2], RelationshipType.ParentOf, fakeMaleNPCs[0], allowChange: true, generating: true, silent: true);
		Relationship.SetRelationship(fakeFemaleNPCs[2], RelationshipType.ParentOf, fakeFemaleNPCs[0], allowChange: true, generating: true, silent: true);
	}

	public static string GetSpeechTextWithDummyData(Speech speech, int i, Character[] fakeNPCs, Community[] fakeCommunities)
	{
		return GetSpeechTextWithDummyData(speech.UniqueID, speech.TextHash, speech.Params, i, fakeNPCs, fakeCommunities, null);
	}

	public static string GetSpeechTextWithDummyData(string uniqueID, int textHash, List<SpeechParam> speechParams, int i, Character[] fakeNPCs, Community[] fakeCommunities, QuestInstance questInstance)
	{
		Character character;
		Character character2;
		BaseObject baseObject;
		switch (i % 10)
		{
		default:
			character = fakeNPCs[0];
			character2 = fakeNPCs[1];
			baseObject = fakeNPCs[2];
			break;
		case 1:
			character = fakeNPCs[0];
			character2 = fakeNPCs[0];
			baseObject = fakeNPCs[0];
			break;
		case 2:
			character = fakeNPCs[0];
			character2 = fakeNPCs[1];
			baseObject = fakeNPCs[1];
			break;
		case 3:
			character = fakeNPCs[0];
			character2 = fakeNPCs[1];
			baseObject = fakeNPCs[0];
			break;
		case 4:
			character = fakeNPCs[0];
			character2 = fakeNPCs[0];
			baseObject = fakeNPCs[1];
			break;
		case 5:
			character = fakeNPCs[1];
			character2 = fakeNPCs[0];
			baseObject = fakeNPCs[2];
			break;
		case 6:
			character = fakeNPCs[1];
			character2 = fakeNPCs[1];
			baseObject = fakeNPCs[1];
			break;
		case 7:
			character = fakeNPCs[1];
			character2 = fakeNPCs[0];
			baseObject = fakeNPCs[0];
			break;
		case 8:
			character = fakeNPCs[1];
			character2 = fakeNPCs[0];
			baseObject = fakeNPCs[1];
			break;
		case 9:
			character = fakeNPCs[1];
			character2 = fakeNPCs[1];
			baseObject = fakeNPCs[0];
			break;
		}
		Memory memory = new Memory
		{
			Prototype = MemoryPrototype.Chatted
		};
		if (i < 10)
		{
			memory.Actor = fakeNPCs[0];
			memory.Object = fakeNPCs[1];
		}
		else
		{
			memory.Actor = fakeNPCs[1];
			memory.Object = fakeNPCs[0];
		}
		if (speechParams != null)
		{
			for (int j = 0; j < speechParams.Count; j++)
			{
				switch (speechParams[j].Type)
				{
				case SpeechParamType.CommunityName:
				case SpeechParamType.CommunityLeaderNameOrYou:
				case SpeechParamType.CommunityLeaderName:
				case SpeechParamType.CommunityLeaderFirstName:
				case SpeechParamType.CommunityLeaderFullName:
				case SpeechParamType.CommunityPossessive:
				case SpeechParamType.CommunityIOrWe:
				case SpeechParamType.CommunityMeOrUs:
				case SpeechParamType.CommunityMyOrOur:
				case SpeechParamType.CommunityBoxerReadyToFight:
				case SpeechParamType.CommunityMemberWithRole:
				case SpeechParamType.CommunityNumMembersWithGiftedItem:
				case SpeechParamType.CommunitySize:
				case SpeechParamType.CommunityNameOrI:
					if ((speechParams[j].Subject == Specifier.ReferringTo && speechParams[j].SubjectModifier == SpecifierModifier.None) || (speechParams[j].Subject == Specifier.MemoryObject && speechParams[j].SubjectModifier == SpecifierModifier.None))
					{
						baseObject = ((i % 2 == 0) ? fakeCommunities[0] : fakeCommunities[1]);
					}
					break;
				case SpeechParamType.RelationshipName:
				{
					Character subject = speechParams[j].GetSubject<Character>(character, character2, baseObject, new MemoryParam(memory));
					Character to = speechParams[j].GetObject<Character>(character, character2, baseObject, new MemoryParam(memory));
					if (Relationship.GetRelationship(subject, to) == RelationshipType.None)
					{
						Relationship.SetRelationship(subject, RelationshipType.FriendsWith, to, allowChange: false, generating: false, silent: true);
					}
					break;
				}
				}
			}
		}
		List<SpeechParamResult> paramResults = new List<SpeechParamResult>();
		List<SpeechEmoticon> emoticons = new List<SpeechEmoticon>();
		Speech.EvaluateSpeechParams(speechParams, character, character2, baseObject, paramResults, new MemoryParam(memory), string.Empty, string.Empty, MathUtil.NonDeterministicRand, questInstance, uniqueID);
		Speech.BuildSpeechText(textHash, speechParams, character, character2, baseObject, paramResults, out var speechText, emoticons, englishOnly: false, questInstance != null);
		return speechText;
	}

	public void PrintFullTranslation(StreamWriter stream)
	{
		CreateDummyData(out var fakeCommunities, out var fakeMaleNPCs, out var fakeFemaleNPCs);
		QuestInstance questInstance = new QuestInstance();
		questInstance.QuestObjectEquipmentType = EquipmentPrototype.FryingPan;
		foreach (KeyValuePair<string, Script> script in Scripts)
		{
			foreach (Speech speech in script.Value.Speeches)
			{
				PrintFullTranslationForScriptObject(stream, speech.UniqueID, speech.TextHash, speech.GetTextKey(), speech.NativeText, speech.Params, fakeCommunities, fakeMaleNPCs, fakeFemaleNPCs, null, speech.Situation == SpeechSituation.Memory);
			}
			foreach (Quest quest in script.Value.Quests)
			{
				PrintFullTranslationForScriptObject(stream, quest.UniqueID, quest.DescriptionHash, quest.GetDescriptionKey(), quest.NativeDescription, quest.Params, fakeCommunities, fakeMaleNPCs, fakeFemaleNPCs, questInstance, wantSwapActorAndObject: false);
			}
			foreach (QuestGroup questGroup in script.Value.QuestGroups)
			{
				PrintFullTranslationForScriptObject(stream, questGroup.UniqueID, questGroup.TitleHash, questGroup.GetTitleKey(), questGroup.NativeTitle, questGroup.Params, fakeCommunities, fakeMaleNPCs, fakeFemaleNPCs, questInstance, wantSwapActorAndObject: false);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, MemoryPrototype> memoryPrototype in MemoryPrototypes)
		{
			MemoryPrototype value = memoryPrototype.Value;
			stream.WriteLine("================================== " + value.GetDescriptionKey() + " ====================================");
			stream.WriteLine(value.NativeDescription);
			List<string> list = new List<string>();
			list.Add(GameImpl.Translate(value.DescriptionHash));
			for (int i = 0; i < 2; i++)
			{
				Character[] array = ((i == 0) ? fakeMaleNPCs : fakeFemaleNPCs);
				for (int j = 0; j < 6; j++)
				{
					bool noStrangers = true;
					Character character;
					Character character2;
					BaseObject obj;
					BaseObject obj2;
					switch (j)
					{
					case 0:
						character = array[0];
						character2 = array[0];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[1]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[1] : null)));
						obj2 = array[2];
						break;
					case 1:
						character = array[0];
						character2 = array[1];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[2]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[2] : null)));
						obj2 = array[0];
						break;
					case 2:
						character = array[0];
						character2 = array[2];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[0]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[0] : null)));
						obj2 = array[1];
						break;
					case 3:
						character = array[1];
						character2 = array[0];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[1]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[1] : null)));
						obj2 = array[2];
						break;
					case 4:
						character = array[1];
						character2 = array[1];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[2]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[2] : null)));
						obj2 = array[0];
						break;
					case 5:
						character = array[1];
						character2 = array[2];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[0]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[0] : null)));
						obj2 = array[1];
						break;
					case 6:
						character = array[2];
						character2 = array[0];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[1]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[1] : null)));
						obj2 = array[2];
						break;
					case 7:
						character = array[2];
						character2 = array[1];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[2]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[2] : null)));
						obj2 = array[0];
						break;
					case 8:
						character = array[2];
						character2 = array[2];
						obj = ((value.ObjectType == MemoryObjectType.Character) ? ((BaseObject)array[0]) : ((BaseObject)((value.ObjectType == MemoryObjectType.Community) ? fakeCommunities[0] : null)));
						obj2 = array[1];
						break;
					default:
						continue;
					}
					if (!value.ActorMustBeSelf || character2 == character)
					{
						BrainScanPage.FakeParamResults.Clear();
						BrainScanPage.AddStringParameter(character, 1, character2, noStrangers);
						BrainScanPage.AddStringParameter(character, 2, obj, noStrangers);
						BrainScanPage.AddStringParameter(character, 3, obj2, noStrangers);
						stringBuilder.Length = 0;
						stringBuilder.Append(GameImpl.Translate(value.DescriptionHash));
						StringUtil.ApplyFormulae(stringBuilder, character, null, null, BrainScanPage.FakeParamResults, englishOnly: false);
						for (int k = 0; k < BrainScanPage.FakeParamResults.Count; k++)
						{
							BrainScanPage.ApplyStringParameter(stringBuilder, k + 1, BrainScanPage.FakeParamResults[k].Str1);
						}
						string item = stringBuilder.ToString();
						if (!list.Contains(item))
						{
							list.Add(item);
						}
					}
				}
			}
			foreach (string item2 in list)
			{
				stream.WriteLine(item2);
			}
		}
	}

	private void PrintFullTranslationForScriptObject(StreamWriter stream, string uniqueID, int textHash, string title, string nativeText, List<SpeechParam> speechParams, Community[] fakeCommunities, Character[] fakeMaleNPCs, Character[] fakeFemaleNPCs, QuestInstance questInstance, bool wantSwapActorAndObject)
	{
		stream.WriteLine("================================== " + title + " ====================================");
		string text = "";
		if (speechParams != null)
		{
			for (int i = 0; i < speechParams.Count; i++)
			{
				text += ((i == 0) ? " (" : ", ");
				text += speechParams[i].Type;
			}
			if (text.Length > 0)
			{
				text += ")";
			}
		}
		stream.WriteLine(nativeText + text);
		List<string> list = new List<string>();
		list.Add(GameImpl.Translate(textHash));
		for (int j = 0; j < 2; j++)
		{
			Character[] fakeNPCs = ((j == 0) ? fakeMaleNPCs : fakeFemaleNPCs);
			for (int k = 0; k < (wantSwapActorAndObject ? 20 : 10); k++)
			{
				string speechTextWithDummyData = GetSpeechTextWithDummyData(uniqueID, textHash, speechParams, k, fakeNPCs, fakeCommunities, questInstance);
				if (!list.Contains(speechTextWithDummyData))
				{
					list.Add(speechTextWithDummyData);
				}
			}
		}
		foreach (string item in list)
		{
			stream.WriteLine(item);
		}
	}

	public void SaveVersionFile()
	{
		string text = Path + "/Version.txt";
		try
		{
			using StreamWriter streamWriter = File.CreateText(text);
			streamWriter.WriteLine(GameImpl.ReleaseVersionAsString);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to save '" + text + "': " + ex.ToString());
		}
	}

	public void CheckVersionFile()
	{
		string text = Path + "/Version.txt";
		try
		{
			if (!File.Exists(text))
			{
				return;
			}
			using StreamReader streamReader = File.OpenText(text);
			int num = StringUtil.ParseInt(streamReader.ReadLine());
			if (num > GameImpl.ReleaseVersion)
			{
				string text2 = GameImpl.Translate("MENU_StoryFromFuture");
				text2 = text2.Replace("%1", StorySource.GetTranslatedName());
				text2 = text2.Replace("%2", num.ToString());
				text2 = text2.Replace("%3", GameImpl.ReleaseVersionAsString);
				GameImpl.Instance.ShowMessageBox(text2);
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to load '" + text + "': " + ex.ToString());
		}
	}

	public void LoadDLLs()
	{
		try
		{
			if (!Directory.Exists(Path + "/DLLs"))
			{
				return;
			}
			string[] files = Directory.GetFiles(Path + "/DLLs", "*.dll");
			foreach (string text in files)
			{
				Debug.Log("Loading DLL: " + text);
				try
				{
					float maxAge = HumanAppearance.MaxAge;
					Assembly assembly = Assembly.LoadFrom(text);
					Type type = assembly.GetType("Main");
					if (type != null)
					{
						MethodInfo method = type.GetMethod("Load", BindingFlags.Static | BindingFlags.Public);
						if (method != null)
						{
							method.Invoke(null, null);
							Assemblies.Add(assembly);
						}
					}
					ChangedMaxAge |= HumanAppearance.MaxAge != maxAge;
				}
				catch (Exception ex)
				{
					Debug.LogError("Error loading DLL " + text + ": " + ex.ToString());
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Error loading DLLs from " + Path + "/DLLs: " + ex2.ToString());
		}
	}

	public void UnloadDLLs()
	{
		try
		{
			foreach (Assembly assembly in Assemblies)
			{
				Type type = assembly.GetType("Main");
				if (type != null)
				{
					MethodInfo method = type.GetMethod("Unload", BindingFlags.Static | BindingFlags.Public);
					if (method != null)
					{
						method.Invoke(null, null);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Error unloading DLLs from " + Path + "/DLLs: " + ex.Message);
		}
		if (ChangedMaxAge)
		{
			HumanAppearance.MaxAge = 80f;
		}
		Assemblies.Clear();
	}
}
