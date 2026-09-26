using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class PrefabSettings
{
	public List<EquippedModelProperties> EquippedModels = new List<EquippedModelProperties>();

	[XmlIgnore]
	public string FileName;

	public EquippedModelProperties FindProperties(string prefabPath)
	{
		foreach (EquippedModelProperties equippedModel in EquippedModels)
		{
			if (equippedModel.PrefabPath == prefabPath)
			{
				return equippedModel;
			}
		}
		return null;
	}

	public void Reload()
	{
		PrefabSettings prefabSettings = LoadFromFile(FileName);
		if (prefabSettings == null)
		{
			return;
		}
		foreach (EquippedModelProperties equippedModel in prefabSettings.EquippedModels)
		{
			EquippedModelProperties equippedModelProperties = FindProperties(equippedModel.PrefabPath);
			if (equippedModelProperties != null)
			{
				equippedModelProperties.CopyFrom(equippedModel);
			}
			else
			{
				EquippedModels.Add(equippedModel);
			}
		}
	}

	private static void FindOrCreatePrefabResource(string path, ref PrefabResource prefab)
	{
		if (!string.IsNullOrEmpty(path) && prefab == null)
		{
			prefab = Resource<GameObject>.FindResourceByPath(path) as PrefabResource;
			if (prefab == null)
			{
				prefab = new PrefabResource(path);
			}
		}
	}

	public static PrefabSettings LoadFromFile(string fileName)
	{
		PrefabSettings prefabSettings = null;
		try
		{
			if (File.Exists(fileName))
			{
				using StreamReader textReader = new StreamReader(fileName);
				prefabSettings = (PrefabSettings)new XmlSerializer(typeof(PrefabSettings)).Deserialize(textReader);
				prefabSettings.FileName = fileName;
				foreach (EquippedModelProperties equippedModel in prefabSettings.EquippedModels)
				{
					equippedModel.Owner = prefabSettings;
					FindOrCreatePrefabResource(equippedModel.PrefabPath, ref equippedModel.Prefab);
					FindOrCreatePrefabResource(equippedModel.LoadedAmmoPrefabPath, ref equippedModel.LoadedAmmoPrefab);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return prefabSettings;
	}

	public static bool SaveToFile(string fileName, PrefabSettings prefabSettings)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				new XmlSerializer(typeof(PrefabSettings)).Serialize(stream, prefabSettings);
			}
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
