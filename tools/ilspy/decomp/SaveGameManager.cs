using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Profiling;

public class SaveGameManager
{
	public delegate void OnSaveGameDeleted();

	public static string[] SaveGameDirName = new string[9] { "savegame", "autosave", "autosave_mp", "quicksave", "quicksave_mp", "token", "token_mp", "current", "current_mp" };

	public static int ThumbnailWidth = 1920;

	public static int ThumbnailHeight = 1080;

	public static SaveGameManager Instance;

	private Thread SaveThread;

	private AutoResetEvent SaveEvent = new AutoResetEvent(initialState: false);

	private List<SaveRequest> SaveRequestQueue = new List<SaveRequest>();

	public SaveRequest CurrentlySavingCharacter;

	public float SaveIconFakeTime = -1000f;

	public Dictionary<SaveGameSlot, int> NextSaveSlot = new Dictionary<SaveGameSlot, int>();

	public bool HasAnySaveGames;

	public bool FakeSaveGameFail;

	private bool Quitting;

	private bool ShowSavingIcon;

	public List<string> SavedCharacterNames = new List<string>();

	public SaveGameManager()
	{
		Instance = this;
	}

	public void Init()
	{
		SaveThread = new Thread(SaveThreadFunc);
		SaveThread.Start();
	}

	public void Unload()
	{
		Quitting = true;
		SaveEvent.Set();
		SaveThread.Join();
		Instance = null;
	}

	public void Update()
	{
		if (CurrentlySavingCharacter == null)
		{
			return;
		}
		switch (CurrentlySavingCharacter.CurrentState)
		{
		case SaveRequest.State.Finished:
		{
			string text = string.Empty;
			if (CurrentlySavingCharacter.SavedCharacter != null)
			{
				text = CurrentlySavingCharacter.SavedCharacter.FirstName + " " + CurrentlySavingCharacter.SavedCharacter.Surname;
				if (!SavedCharacterNames.Contains(text))
				{
					SavedCharacterNames.Add(text);
				}
				SavedCharacterNames.Sort();
			}
			else if (!string.IsNullOrEmpty(CurrentlySavingCharacter.CharacterNameToDelete))
			{
				text = CurrentlySavingCharacter.CharacterNameToDelete;
				SavedCharacterNames.Remove(text);
			}
			if (CurrentlySavingCharacter.OnSavedCharacterFunction != null)
			{
				CurrentlySavingCharacter.OnSavedCharacterFunction(text);
			}
			CurrentlySavingCharacter = null;
			break;
		}
		case SaveRequest.State.Failed:
			GameImpl.Instance.ShowMessageBox(CurrentlySavingCharacter.SaveResult.Message);
			CurrentlySavingCharacter = null;
			break;
		}
	}

	public void SaveCharacter(SavedCharacter savedCharacter, OnSavedFunction onSavedFunction)
	{
		if (CurrentlySavingCharacter == null)
		{
			SaveRequest saveRequest = new SaveRequest();
			saveRequest.SavedCharacter = savedCharacter;
			saveRequest.OnSavedCharacterFunction = onSavedFunction;
			CurrentlySavingCharacter = saveRequest;
			PushSaveRequest(saveRequest);
		}
	}

	public void DeleteCharacter(string name, OnSavedFunction onSavedFunction)
	{
		if (CurrentlySavingCharacter == null)
		{
			SaveRequest saveRequest = new SaveRequest();
			saveRequest.CharacterNameToDelete = name;
			saveRequest.OnSavedCharacterFunction = onSavedFunction;
			CurrentlySavingCharacter = saveRequest;
			PushSaveRequest(saveRequest);
		}
	}

	public void PushSaveRequest(SaveRequest autosave)
	{
		SaveIconFakeTime = Time.unscaledTime;
		lock (SaveRequestQueue)
		{
			SaveRequestQueue.Add(autosave);
			SaveEvent.Set();
			ShowSavingIcon = true;
		}
	}

	private SaveRequest PeekSaveRequest()
	{
		SaveRequest result = null;
		lock (SaveRequestQueue)
		{
			if (SaveRequestQueue.Count > 0)
			{
				result = SaveRequestQueue[0];
			}
		}
		return result;
	}

	private bool PopSaveRequest()
	{
		lock (SaveRequestQueue)
		{
			SaveRequestQueue.RemoveAt(0);
			if (SaveRequestQueue.Count == 0)
			{
				ShowSavingIcon = false;
			}
			return SaveRequestQueue.Count == 0;
		}
	}

	public bool WantSavingIcon()
	{
		if (!ShowSavingIcon)
		{
			return Time.unscaledTime - SaveIconFakeTime <= 0.5f;
		}
		return true;
	}

	public bool IsSaving()
	{
		lock (SaveRequestQueue)
		{
			return SaveRequestQueue.Count > 0;
		}
	}

	private void SaveThreadFunc()
	{
		Util.SetFloatingPointControl();
		Util.CheckFloatingPointControl();
		while (!Quitting)
		{
			SaveEvent.WaitOne();
			do
			{
				SaveRequest saveRequest = PeekSaveRequest();
				if (saveRequest == null)
				{
					break;
				}
				try
				{
					Util.SetFloatingPointControl();
					Util.CheckFloatingPointControl();
					Save(saveRequest);
					saveRequest.CurrentState = SaveRequest.State.Finished;
					Util.CheckFloatingPointControl();
				}
				catch (Exception ex)
				{
					Debug.LogWarning(ex.ToString());
					saveRequest.SaveResult = ex;
					saveRequest.CurrentState = SaveRequest.State.Failed;
				}
			}
			while (!PopSaveRequest());
		}
		Profiler.EndThreadProfiling();
	}

	public void DeleteCurrentSave(int gameUniqueId)
	{
		DeleteSaveGame(BuildSaveGameDirName(SaveGameType.Current, gameUniqueId), null);
	}

	public static void DeleteSaveGame(string dirName, OnSaveGameDeleted onSaveGameDeleted)
	{
		GameImpl instance = GameImpl.Instance;
		try
		{
			Directory.Delete(instance.SaveGamePath + "/" + dirName, recursive: true);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(GameImpl.Translate("MENU_DeleteSaveGameError") + " (" + dirName + "): " + ex.Message);
			return;
		}
		onSaveGameDeleted?.Invoke();
	}

	public void Save(SaveRequest request)
	{
		GameImpl instance = GameImpl.Instance;
		string empty = string.Empty;
		if (request.SaveGame != null)
		{
			empty = instance.SaveGamePath + "/" + request.SaveGame.DirName;
		}
		else if (request.SettingsData != null)
		{
			empty = instance.SaveGamePath;
		}
		else
		{
			if (request.SavedCharacter == null && string.IsNullOrEmpty(request.CharacterNameToDelete))
			{
				throw new Exception("Invalid request");
			}
			empty = instance.SaveGamePath + "/Characters";
		}
		if (request.SaveGame != null)
		{
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = false,
				Encoding = Encoding.UTF8
			};
			if (FakeSaveGameFail)
			{
				throw new Exception("Something went wrong");
			}
			string text = empty + "/info.xml";
			string text2 = empty + "/savegame.sav";
			string text3 = empty + "/terrain.map";
			string text4 = empty + "/thumbnail.jpg";
			if (Directory.Exists(empty))
			{
				Util.DeleteFile(text);
				Util.DeleteFile(text2);
				Util.DeleteFile(text3);
				Util.DeleteFile(text4);
			}
			else
			{
				Directory.CreateDirectory(empty);
			}
			using (Stream stream = File.Create(text2))
			{
				stream.Write(request.Writer._buffer, 0, request.Writer._index);
			}
			if (request.TerrainHash.IsValid())
			{
				File.Copy(OnlineParty.GetTerrainCacheFileName(request.TerrainHash), text3, overwrite: true);
			}
			using (Stream stream2 = File.Create(text4))
			{
				stream2.Write(request.ThumbnailBytes, 0, request.ThumbnailBytes.Length);
			}
			using (Stream output = File.Create(text))
			{
				using XmlWriter xmlWriter = XmlWriter.Create(output, settings);
				new XmlSerializer(typeof(SaveGameData)).Serialize(xmlWriter, request.SaveGame.Data);
			}
			HasAnySaveGames = true;
			return;
		}
		if (request.SettingsData != null)
		{
			string text5 = "settings.xml";
			string path = empty + "/" + text5;
			Directory.CreateDirectory(empty);
			using Stream stream3 = File.Create(path);
			new XmlSerializer(typeof(SettingsData)).Serialize(stream3, request.SettingsData);
			return;
		}
		if (request.SavedCharacter != null)
		{
			string text6 = request.SavedCharacter.FirstName + " " + request.SavedCharacter.Surname + ".char";
			string path2 = empty + "/" + text6;
			Directory.CreateDirectory(empty);
			using Stream stream4 = File.OpenWrite(path2);
			using CustomBinaryWriter reflector = new CustomBinaryWriter(stream4);
			request.SavedCharacter.Reflect(reflector);
			return;
		}
		if (!string.IsNullOrEmpty(request.CharacterNameToDelete))
		{
			string text7 = request.CharacterNameToDelete + ".char";
			FileDelete(empty + "/" + text7);
		}
	}

	public static string BuildSaveGameDirName(SaveGameType saveGameType, int gameUniqueId)
	{
		string text = SaveGameDirName[(int)saveGameType];
		if (saveGameType == SaveGameType.Token || saveGameType == SaveGameType.Token_Multiplayer)
		{
			text = text + "_" + gameUniqueId.ToString("x8") + "_";
		}
		if (saveGameType == SaveGameType.Current)
		{
			text = text + "_" + gameUniqueId.ToString("x8");
		}
		return text;
	}

	public static void ParseSaveGameDirName(string dirName, out SaveGameType saveGameType, out int slot, out int gameUniqueId)
	{
		saveGameType = SaveGameType.Invalid;
		slot = 0;
		gameUniqueId = 0;
		for (int i = 0; i < SaveGameDirName.Length; i++)
		{
			if (!dirName.StartsWith(SaveGameDirName[i]))
			{
				continue;
			}
			int num = SaveGameDirName[i].Length;
			if (i == 5 || i == 6 || i == 7)
			{
				num++;
				if (!int.TryParse(dirName.Substring(num, 8), NumberStyles.HexNumber, null, out gameUniqueId))
				{
					continue;
				}
				num += 8;
				if (i == 7)
				{
					saveGameType = (SaveGameType)i;
					break;
				}
				num++;
			}
			if (num <= dirName.Length && int.TryParse(dirName.Substring(num), out slot))
			{
				saveGameType = (SaveGameType)i;
				break;
			}
		}
	}

	public static bool IsSaveGameCompressed()
	{
		return false;
	}

	public static Stream OpenSaveGameFile(string path)
	{
		_ = GameImpl.Instance;
		Path.GetFileName(path);
		if (Directory.Exists(Path.GetDirectoryName(path)) && File.Exists(path))
		{
			return File.Open(path, FileMode.Open);
		}
		return null;
	}

	public static bool FileExists(string filePath, bool inSaveData = true)
	{
		return File.Exists(filePath);
	}

	public static bool DirectoryExists(string filePath)
	{
		return Directory.Exists(filePath);
	}

	public static byte[] FileReadAllBytes(string filePath, bool inSaveData = true)
	{
		if (!FileExists(filePath, inSaveData))
		{
			return null;
		}
		return File.ReadAllBytes(filePath);
	}

	public static Stream FileOpenRead(string fileName, bool inSaveData = true)
	{
		return File.OpenRead(fileName);
	}

	public static void FileWriteAllBytes(string filePath, byte[] data, int length)
	{
		if (length != data.Length)
		{
			byte[] array = new byte[length];
			Array.Copy(data, 0, array, 0, Math.Min(length, data.Length));
			data = array;
		}
		File.WriteAllBytes(filePath, data);
	}

	public static void DirectoryCreate(string dirName)
	{
		if (!Directory.Exists(dirName))
		{
			Directory.CreateDirectory(dirName);
		}
	}

	public static void FileDelete(string filePath)
	{
		if (File.Exists(filePath))
		{
			File.Delete(filePath);
		}
	}

	public static List<string> DirectoryGetFiles(string path, string extension)
	{
		return new List<string>(Directory.GetFiles(path, "*" + extension));
	}

	public static string[] GetSaveGameFolders()
	{
		GameImpl instance = GameImpl.Instance;
		if (Directory.Exists(instance.SaveGamePath))
		{
			return Directory.GetDirectories(instance.SaveGamePath);
		}
		return new string[0];
	}

	public static string[] GetSaveGameFiles(string dirName)
	{
		_ = GameImpl.Instance;
		if (Directory.Exists(dirName))
		{
			return Directory.GetFiles(dirName);
		}
		return null;
	}

	public bool LoadSettings()
	{
		bool result = false;
		try
		{
			GameImpl instance = GameImpl.Instance;
			using (Stream stream = OpenSaveGameFile(instance.SaveGamePath + "/settings.xml"))
			{
				if (stream != null)
				{
					SettingsData data = (SettingsData)new XmlSerializer(typeof(SettingsData)).Deserialize(stream);
					instance.LoadSettingsData(data);
					result = true;
				}
			}
			Dictionary<SaveGameSlot, DateTime> dictionary = new Dictionary<SaveGameSlot, DateTime>();
			Dictionary<SaveGameSlot, int> dictionary2 = new Dictionary<SaveGameSlot, int>();
			string[] saveGameFolders = GetSaveGameFolders();
			foreach (string obj in saveGameFolders)
			{
				string path = obj + "/info.xml";
				ParseSaveGameDirName(Path.GetFileName(obj), out var saveGameType, out var slot, out var gameUniqueId);
				if (saveGameType != SaveGameType.AutoSave && saveGameType != SaveGameType.AutoSave_Multiplayer && saveGameType != SaveGameType.QuickSave && saveGameType != SaveGameType.QuickSave_Multiplayer && saveGameType != SaveGameType.Token && saveGameType != SaveGameType.Token_Multiplayer && saveGameType != SaveGameType.Current)
				{
					continue;
				}
				try
				{
					using Stream stream2 = OpenSaveGameFile(path);
					if (stream2 != null)
					{
						SaveGameData saveGameData = (SaveGameData)new XmlSerializer(typeof(SaveGameData)).Deserialize(stream2);
						SaveGameSlot key = new SaveGameSlot(saveGameType, gameUniqueId);
						if (!dictionary.TryGetValue(key, out var value) || saveGameData.Timestamp > value)
						{
							dictionary[key] = saveGameData.Timestamp;
							dictionary2[key] = slot;
							HasAnySaveGames = true;
						}
					}
				}
				catch (Exception)
				{
				}
			}
			foreach (KeyValuePair<SaveGameSlot, DateTime> item in dictionary)
			{
				if (item.Key.SaveGameType != SaveGameType.Normal)
				{
					NextSaveSlot[item.Key] = ((dictionary2[item.Key] == 0) ? 1 : 0);
				}
			}
		}
		catch (Exception ex2)
		{
			Debug.LogError("Could not load settings.xml: " + ex2.Message);
		}
		return result;
	}

	public void EnumerateSavedCharacters()
	{
		try
		{
			string dirName = GameImpl.Instance.SaveGamePath + "/Characters";
			SavedCharacterNames.Clear();
			string[] saveGameFiles = GetSaveGameFiles(dirName);
			if (saveGameFiles != null)
			{
				string[] array = saveGameFiles;
				for (int i = 0; i < array.Length; i++)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(array[i]);
					if (!string.IsNullOrEmpty(fileNameWithoutExtension))
					{
						SavedCharacterNames.Add(fileNameWithoutExtension);
					}
				}
			}
			SavedCharacterNames.Sort();
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}
}
