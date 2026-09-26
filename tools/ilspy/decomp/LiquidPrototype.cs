using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class LiquidPrototype
{
	public static LiquidPrototype Snow;

	public static LiquidPrototype Water;

	public static LiquidPrototype Wine;

	public static LiquidPrototype Urine;

	public static LiquidPrototype Vodka;

	[XmlIgnore]
	public string Name = "";

	[DefaultValue("")]
	public string NativeName = "";

	[DefaultValue("")]
	public string NativeDescription = "";

	[DefaultValue("")]
	public string Category = "";

	[XmlIgnore]
	public int NameHash;

	[XmlIgnore]
	public int DescriptionHash;

	[XmlIgnore]
	public Resource<Texture2D> Tex;

	public Color32 Col;

	[DefaultValue(GenderType.Count)]
	public GenderType Gender = GenderType.Count;

	[DefaultValue(null)]
	public List<GenderInLanguage> GenderInLanguages;

	[DefaultValue(0f)]
	public float BasePricePerFlOz;

	[DefaultValue(0f)]
	public float NutritionPerFlOz;

	[DefaultValue(0f)]
	public float CaffeinePerFlOz;

	[DefaultValue(0f)]
	public float WaterContent;

	[DefaultValue(0f)]
	public float AlcoholContent;

	[DefaultValue(0f)]
	public float Tastiness;

	[DefaultValue(false)]
	public bool Spicy;

	[DefaultValue(false)]
	public bool Flammable;

	[DefaultValue(false)]
	public bool CanPourIntoBottles;

	[DefaultValue(null)]
	public List<string> GiftFor;

	[DefaultValue(null)]
	public List<string> BadGiftFor;

	[DefaultValue(0f)]
	public float GiftAmount;

	[DefaultValue(SkillType.Invalid)]
	public SkillType SkillOnConsumptionType = SkillType.Invalid;

	[DefaultValue(0)]
	public float SkillOnConsumptionProgressionPerFlOz;

	[XmlIgnore]
	public bool Discovered;

	[XmlIgnore]
	public float Counter;

	public string IconPath => "Icons/" + Name + ".png";

	public bool DrinkableOrEdible
	{
		get
		{
			if (WaterContent == 0f && NutritionPerFlOz == 0f)
			{
				return CaffeinePerFlOz != 0f;
			}
			return true;
		}
	}

	public bool Edible
	{
		get
		{
			if (DrinkableOrEdible)
			{
				return Math.Abs(WaterContent / 100f * Sun.DayLengthSecs / Character.WaterNeededPerDayInFlOz) < Math.Abs(GetNutritionPerFlOz());
			}
			return false;
		}
	}

	public bool Drinkable
	{
		get
		{
			if (DrinkableOrEdible)
			{
				return Math.Abs(WaterContent / 100f * Sun.DayLengthSecs / Character.WaterNeededPerDayInFlOz) >= Math.Abs(GetNutritionPerFlOz());
			}
			return false;
		}
	}

	public static void CacheLiquidPrototypeRefs()
	{
		GameImpl instance = GameImpl.Instance;
		Snow = instance.FindLiquidPrototypeByName("Snow");
		Water = instance.FindLiquidPrototypeByName("Water");
		Wine = instance.FindLiquidPrototypeByName("Wine");
		Urine = instance.FindLiquidPrototypeByName("Urine");
		Vodka = instance.FindLiquidPrototypeByName("Vodka");
	}

	public float GetNutritionPerFlOz()
	{
		return NutritionPerFlOz * Sun.DayLengthSecs;
	}

	public float GetCaffeinePerFlOz()
	{
		return CaffeinePerFlOz * Sun.DayLengthSecs;
	}

	public bool CanBeGift()
	{
		if (GiftFor != null)
		{
			return GiftFor.Count > 0;
		}
		return false;
	}

	public string GetNameKey()
	{
		return "LIQUID_" + Name;
	}

	public string GetDescriptionKey()
	{
		return "DESC_" + Name;
	}

	public static LiquidPrototype LoadFromFile(string fileName, Translation englishTranslation)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		if (string.IsNullOrEmpty(fileNameWithoutExtension))
		{
			return null;
		}
		try
		{
			if (File.Exists(fileName))
			{
				using (StreamReader textReader = new StreamReader(fileName))
				{
					LiquidPrototype liquidPrototype = (LiquidPrototype)new XmlSerializer(typeof(LiquidPrototype)).Deserialize(textReader);
					liquidPrototype.Name = fileNameWithoutExtension;
					liquidPrototype.Tex = Resource<Texture2D>.CreateIfFileExists(Path.GetDirectoryName(fileName) + "/" + liquidPrototype.IconPath);
					liquidPrototype.NameHash = StringUtil.JenkinsHash(liquidPrototype.GetNameKey());
					liquidPrototype.DescriptionHash = StringUtil.JenkinsHash(liquidPrototype.GetDescriptionKey());
					englishTranslation.Keys[liquidPrototype.NameHash] = liquidPrototype.NativeName;
					englishTranslation.Keys[liquidPrototype.DescriptionHash] = liquidPrototype.NativeDescription;
					if (liquidPrototype.GiftFor != null && liquidPrototype.GiftFor.Count == 0)
					{
						liquidPrototype.GiftFor = null;
					}
					if (liquidPrototype.BadGiftFor != null && liquidPrototype.BadGiftFor.Count == 0)
					{
						liquidPrototype.BadGiftFor = null;
					}
					if (liquidPrototype.GenderInLanguages != null && liquidPrototype.GenderInLanguages.Count == 0)
					{
						liquidPrototype.GenderInLanguages = null;
					}
					return liquidPrototype;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to load '" + fileName + "': " + ex.ToString());
		}
		return null;
	}

	public bool SaveToFile(string fileName)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				XmlAttributeOverrides xmlAttributeOverrides = new XmlAttributeOverrides();
				LiquidPrototype obj = new LiquidPrototype();
				FieldInfo[] fields = GetType().GetFields();
				foreach (FieldInfo fieldInfo in fields)
				{
					if (!fieldInfo.IsStatic)
					{
						object value = fieldInfo.GetValue(this);
						object value2 = fieldInfo.GetValue(obj);
						if (value != null && value.Equals(value2))
						{
							XmlAttributes xmlAttributes = new XmlAttributes();
							xmlAttributes.XmlIgnore = true;
							xmlAttributeOverrides.Add(typeof(LiquidPrototype), fieldInfo.Name, xmlAttributes);
						}
					}
				}
				new XmlSerializer(typeof(LiquidPrototype), xmlAttributeOverrides).Serialize(stream, this);
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

	public void CopyFrom(LiquidPrototype other)
	{
		FieldInfo[] fields = typeof(LiquidPrototype).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}

	public void MarkDiscovered()
	{
		if (!Discovered)
		{
			Discovered = true;
			if (Session.Instance.State == SessionState.Started)
			{
				NotificationManager.Instance.ShowRecipeDiscoveredNotifications();
			}
		}
	}

	public GenderType GetGenderInLanguage(Language language)
	{
		if (GenderInLanguages != null)
		{
			for (int i = 0; i < GenderInLanguages.Count; i++)
			{
				if (GenderInLanguages[i].Language == language)
				{
					return GenderInLanguages[i].Gender;
				}
			}
		}
		return Gender;
	}

	public void SetGenderInLanguage(Language language, GenderType gender)
	{
		if (GenderInLanguages == null)
		{
			GenderInLanguages = new List<GenderInLanguage>();
		}
		for (int i = 0; i < GenderInLanguages.Count; i++)
		{
			if (GenderInLanguages[i].Language == language)
			{
				GenderInLanguage genderInLanguage = GenderInLanguages[i];
				genderInLanguage.Gender = gender;
				GenderInLanguages[i] = genderInLanguage;
				return;
			}
		}
		GenderInLanguage genderInLanguage2 = new GenderInLanguage();
		genderInLanguage2.Language = language;
		genderInLanguage2.Gender = gender;
		GenderInLanguages.Add(genderInLanguage2);
	}
}
