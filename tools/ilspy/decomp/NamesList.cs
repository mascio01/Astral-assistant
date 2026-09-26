using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NamesList
{
	public List<string> Names = new List<string>();

	public static NamesList LoadFromFile(string tsvFileName)
	{
		if (File.Exists(tsvFileName))
		{
			try
			{
				using StreamReader streamReader = new StreamReader(tsvFileName);
				NamesList namesList = new NamesList();
				while (!streamReader.EndOfStream)
				{
					string text = streamReader.ReadLine();
					namesList.Names.Add(text.Trim());
				}
				return namesList;
			}
			catch (Exception ex)
			{
				Debug.Log("NamesList.LoadFromFile error: " + ex.Message);
			}
		}
		return null;
	}
}
