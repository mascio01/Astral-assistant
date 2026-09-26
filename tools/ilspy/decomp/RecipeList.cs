using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class RecipeList
{
	public List<Recipe> Recipes = new List<Recipe>();

	public static SortedList<string, Recipe> LoadFromFile(string fileName, Translation englishTranslation)
	{
		SortedList<string, Recipe> sortedList = new SortedList<string, Recipe>();
		try
		{
			if (File.Exists(fileName))
			{
				using StreamReader textReader = new StreamReader(fileName);
				RecipeList obj = (RecipeList)new XmlSerializer(typeof(RecipeList)).Deserialize(textReader);
				int num = 0;
				foreach (Recipe recipe in obj.Recipes)
				{
					if (string.IsNullOrEmpty(recipe.UniqueID))
					{
						num++;
						recipe.UniqueID = num.ToString();
					}
					if (!string.IsNullOrEmpty(recipe.NativeName))
					{
						recipe.NameHash = StringUtil.JenkinsHash(recipe.GetNameKey());
						englishTranslation.Keys[recipe.NameHash] = recipe.NativeName;
					}
					if (recipe.ExtraOutputs != null && recipe.ExtraOutputs.Count == 0)
					{
						recipe.ExtraOutputs = null;
					}
					sortedList[recipe.UniqueID] = recipe;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return sortedList;
	}

	public static bool SaveToFile(string fileName, SortedList<string, Recipe> recipes)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				RecipeList recipeList = new RecipeList();
				foreach (KeyValuePair<string, Recipe> recipe in recipes)
				{
					Recipe value = recipe.Value;
					recipeList.Recipes.Add(value);
				}
				new XmlSerializer(typeof(RecipeList)).Serialize(stream, recipeList);
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
