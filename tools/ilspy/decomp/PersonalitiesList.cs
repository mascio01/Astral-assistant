using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PersonalitiesList
{
	public static string[] CachedType = new string[28]
	{
		"Nervous", "Bold", "Compassionate", "Detached", "Loyal", "Fickle", "Sociopath", "Idealistic", "Cynical", "Moral",
		"Immoral", "Amoral", "Hypocritical", "Unflappable", "Emotional", "Bipolar", "Alcoholic", "Aggressive", "Passive", "Passive-Aggressive",
		"Homosexual", "Bisexual", "Skeptical", "Credulous", "Likes Bland Food", "Likes Spicy Food", "Jealous", "Unpossessive"
	};

	public List<PersonalityGroup> PersonalityGroups = new List<PersonalityGroup>();

	private static string FactionPrefix = "Faction:";

	public static PersonalitiesList LoadFromFile(string tsvFileName)
	{
		if (File.Exists(tsvFileName))
		{
			try
			{
				using StreamReader streamReader = new StreamReader(tsvFileName);
				PersonalitiesList personalitiesList = new PersonalitiesList();
				string faction = PersonalityGroup.NormalFaction;
				while (!streamReader.EndOfStream)
				{
					string[] array = streamReader.ReadLine().Split('\t');
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = array[i].Trim();
					}
					if (array.Length != 0 && array[0].StartsWith(FactionPrefix))
					{
						faction = array[0].Substring(FactionPrefix.Length);
						continue;
					}
					PersonalityGroup personalityGroup = new PersonalityGroup();
					personalityGroup.Faction = faction;
					float num = 0f;
					int num2 = 0;
					string[] array2 = array;
					foreach (string text in array2)
					{
						if (text.Length > 0)
						{
							PersonalityProbability item = default(PersonalityProbability);
							int num3 = text.IndexOf(':');
							item.Personality = ((num3 != -1) ? text.Substring(0, num3) : text).Trim();
							if (num3 != -1 && float.TryParse(text.Substring(num3 + 1).Replace("%", ""), out item.Probability))
							{
								num += item.Probability;
							}
							else
							{
								num2++;
							}
							personalityGroup.Personalities.Add(item);
						}
					}
					if (num2 > 0)
					{
						for (int k = 0; k < personalityGroup.Personalities.Count; k++)
						{
							PersonalityProbability value = personalityGroup.Personalities[k];
							if (value.Probability == 0f)
							{
								value.Probability = (100f - num) / (float)(num2 + 1);
								personalityGroup.Personalities[k] = value;
							}
						}
					}
					if (personalityGroup.Personalities.Count > 0)
					{
						personalitiesList.PersonalityGroups.Add(personalityGroup);
					}
				}
				return personalitiesList;
			}
			catch (Exception ex)
			{
				Debug.Log("PersonalitiesList.LoadFromFile error: " + ex.Message);
			}
		}
		return null;
	}
}
