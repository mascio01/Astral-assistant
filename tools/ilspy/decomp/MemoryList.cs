using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class MemoryList
{
	public List<MemoryPrototype> Memories = new List<MemoryPrototype>();

	public static SortedList<string, MemoryPrototype> LoadFromFile(string fileName, Translation englishTranslation)
	{
		SortedList<string, MemoryPrototype> sortedList = new SortedList<string, MemoryPrototype>();
		try
		{
			if (File.Exists(fileName))
			{
				using StreamReader textReader = new StreamReader(fileName);
				MemoryList obj = (MemoryList)new XmlSerializer(typeof(MemoryList)).Deserialize(textReader);
				int num = 0;
				foreach (MemoryPrototype memory in obj.Memories)
				{
					if (!memory.CanBeForgotten)
					{
						memory.CanBeForgotten = true;
						memory.CanForget = CanForget.No;
					}
					if (string.IsNullOrEmpty(memory.UniqueID))
					{
						num++;
						memory.UniqueID = num.ToString();
					}
					memory.GoodnessBadness = Mathf.Clamp(memory.GoodnessBadness, -100f, 100f);
					memory.DifficultyPatheticness = Mathf.Clamp(memory.DifficultyPatheticness, -100f, 100f);
					memory.DescriptionHash = StringUtil.JenkinsHash(memory.GetDescriptionKey());
					if (memory.Replace.Count == 0)
					{
						memory.Replace = null;
					}
					englishTranslation.Keys[memory.DescriptionHash] = memory.NativeDescription;
					sortedList[memory.UniqueID] = memory;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return sortedList;
	}

	public static bool SaveToFile(string fileName, SortedList<string, MemoryPrototype> protos)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				MemoryList memoryList = new MemoryList();
				foreach (KeyValuePair<string, MemoryPrototype> proto in protos)
				{
					MemoryPrototype value = proto.Value;
					memoryList.Memories.Add(value);
				}
				new XmlSerializer(typeof(MemoryList)).Serialize(stream, memoryList);
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
}
