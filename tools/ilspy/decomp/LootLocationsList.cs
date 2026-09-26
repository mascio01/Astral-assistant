using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LootLocationsList
{
	public List<LootLocationDef> LootLocations = new List<LootLocationDef>();

	public static LootLocationsList LoadFromFile(string tsvFileName)
	{
		if (File.Exists(tsvFileName))
		{
			try
			{
				using StreamReader streamReader = new StreamReader(tsvFileName);
				streamReader.ReadLine();
				LootLocationsList lootLocationsList = new LootLocationsList();
				while (!streamReader.EndOfStream)
				{
					LootLocationDef lootLocationDef = new LootLocationDef();
					string[] array = streamReader.ReadLine().Split('\t');
					for (int i = 0; i < array.Length; i++)
					{
						string text = array[i].Trim();
						switch (i)
						{
						case 0:
							lootLocationDef.Name = text;
							break;
						case 1:
							lootLocationDef.ClothingProbability = StringUtil.ParseFloat(text);
							break;
						case 2:
							lootLocationDef.EmptyLiquidContainers = StringUtil.ParseBool(text);
							break;
						}
					}
					lootLocationsList.LootLocations.Add(lootLocationDef);
				}
				return lootLocationsList;
			}
			catch (Exception ex)
			{
				Debug.Log("LootLocationsList.LoadFromFile error: " + ex.Message);
			}
		}
		return null;
	}

	public LootLocationDef FindLootLocation(string name)
	{
		for (int i = 0; i < LootLocations.Count; i++)
		{
			if (LootLocations[i].Name == name)
			{
				return LootLocations[i];
			}
		}
		return null;
	}
}
