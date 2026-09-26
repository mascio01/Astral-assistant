using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class EditEquipmentMenu : BaseMenu
{
	private EquipmentPrototype Selected;

	private string SelectedName;

	private bool HasChanged;

	private bool WantRepopulate;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0.25f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		WantRepopulate = true;
		Selected = null;
		if (GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes.Count > 0)
		{
			OnSelectEquipment(GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes.Values[0]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			SaveEquipment();
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			PopulateEquipmentList();
			PopulateEquipmentInfo();
			WantRepopulate = false;
		}
	}

	public void OnCreateNew()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterEquipmentName"), "", multiline: false, readOnly: false);
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			CheckUniqueID(ref name);
			string text = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Equipment";
			EquipmentPrototype equipmentPrototype = new EquipmentPrototype();
			equipmentPrototype.Name = name;
			equipmentPrototype.Tex = Resource<Texture2D>.CreateIfFileExists(text + "/" + equipmentPrototype.IconPath);
			GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes[name] = equipmentPrototype;
			OnSelectEquipment(equipmentPrototype);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveEquipment();
		WantPop = true;
	}

	private void OnSelectEquipment(EquipmentPrototype proto)
	{
		SaveEquipment();
		Selected = proto;
		SelectedName = proto?.Name;
		PopulateEquipmentList();
		PopulateEquipmentInfo();
	}

	private void SaveEquipment()
	{
		if (Selected == null || !HasChanged)
		{
			return;
		}
		string text = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Equipment";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		if (!Selected.SaveToFile(text + "/" + Selected.Name + ".xml"))
		{
			Selected.Name = SelectedName;
		}
		if (Selected.Name != SelectedName)
		{
			if (File.Exists(text + "/" + SelectedName + ".xml"))
			{
				File.Delete(text + "/" + SelectedName + ".xml");
			}
			GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes.Remove(SelectedName);
			GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes[Selected.Name] = Selected;
			SelectedName = Selected.Name;
		}
		else
		{
			GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes[Selected.Name] = Selected;
		}
		Selected.Tex = Resource<Texture2D>.CreateIfFileExists(text + "/" + Selected.IconPath);
		HasChanged = false;
		GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
		GameImpl.Instance.BuildPrototypeLookupLists();
	}

	private void PopulateEquipmentList()
	{
		GameObject gameObject = base.gameObject.transform.Find("EquipmentList/Viewport/Content").gameObject;
		int num = 0;
		foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in GameImpl.Instance.GetCurrentlyEditingStory().EquipmentPrototypes)
		{
			Transform transform = gameObject.transform.Find(equipmentPrototype.Key);
			GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
			if (gameObject2 == null)
			{
				EquipmentPrototype proto = equipmentPrototype.Value;
				gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/IconButton"));
				gameObject2.name = equipmentPrototype.Key;
				gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					OnSelectEquipment(proto);
				});
			}
			gameObject2.transform.Find("Text").gameObject.GetComponent<Text>().SetUnityText(equipmentPrototype.Value.Name);
			gameObject2.transform.Find("Image").gameObject.GetComponent<RawImage>().texture = ((equipmentPrototype.Value.Tex != null && (bool)equipmentPrototype.Value.Tex.GetAsset()) ? equipmentPrototype.Value.Tex.GetAsset() : null);
			gameObject2.transform.SetSiblingIndex(num);
			num++;
		}
		for (; num < gameObject.transform.childCount; num++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulateEquipmentInfo()
	{
		GameObject gameObject = base.gameObject.FindChild("EquipmentInfo/Viewport/Content");
		int index = 0;
		if (Selected != null)
		{
			Type type = BaseObjectManager.BaseObjectTypes[(int)Selected.TypeName];
			FieldInfo[] fields = typeof(EquipmentPrototype).GetFields();
			foreach (FieldInfo field in fields)
			{
				FieldInfo localField = field;
				if (field.IsStatic || (field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0 && field.Name != "Name"))
				{
					continue;
				}
				OnlyVisibleForType customAttribute = field.GetCustomAttribute<OnlyVisibleForType>();
				if (customAttribute != null && !customAttribute.Matches(type))
				{
					continue;
				}
				NotVisibleForType customAttribute2 = field.GetCustomAttribute<NotVisibleForType>();
				if ((customAttribute2 != null && customAttribute2.Matches(type)) || (field.GetCustomAttribute<OnlyVisibleForClothing>() != null && Selected.ClothingType == ClothingType.Invalid) || (field.GetCustomAttribute<OnlyVisibleForLiquidContainer>() != null && Selected.LiquidCapacity == 0f) || (field.GetCustomAttribute<OnlyVisibleForSkillBook>() != null && Selected.SkillBonusType == SkillType.Invalid) || (field.GetCustomAttribute<OnlyVisibleForGifts>() != null && !Selected.CanBeGift()) || (field.GetCustomAttribute<OnlyVisibleForFood>() != null && Selected.GetNutrition() == 0f && Selected.Water == 0f) || (field.GetCustomAttribute<OnlyVisibleForFoodOrLiquidContainerOrGlassBottle>() != null && Selected.GetNutrition() == 0f && Selected.Water == 0f && Selected.LiquidCapacity == 0f && Selected.TypeName != BaseObjectType.GlassBottle) || (field.GetCustomAttribute<OnlyVisibleForAntigen>() != null && Selected.AntigenType != InfectionType.White) || field.Name == "AmmoType")
				{
					continue;
				}
				if (field.FieldType == typeof(bool))
				{
					BaseMenu.AddCheckbox(gameObject, ref index, "EDITOR_" + field.Name, -1, (bool)field.GetValue(Selected), delegate(bool flag)
					{
						localField.SetValue(Selected, flag);
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(int))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((int)field.GetValue(Selected)).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							localField.SetValue(Selected, result);
							HasChanged = true;
						}
					});
				}
				else if (field.FieldType == typeof(float))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((float)field.GetValue(Selected)).ToString(), delegate(string s)
					{
						if (float.TryParse(s, out var result))
						{
							localField.SetValue(Selected, result);
							HasChanged = true;
						}
					});
				}
				else if (field.FieldType == typeof(string))
				{
					if (field.Name == "EquippedModelName")
					{
						List<string> list = new List<string>();
						for (int num = GameImpl.Instance.CurrentStories.Count - 1; num >= 0; num--)
						{
							foreach (PrefabResource prefab in GameImpl.Instance.CurrentStories[num].Prefabs)
							{
								if (!list.Contains(prefab.GetPath()))
								{
									list.Add(prefab.GetPath());
								}
							}
						}
						int num2 = 0;
						for (int num3 = GameImpl.Instance.CurrentStories.Count - 1; num3 >= 0; num3--)
						{
							PrefabSettings prefabSettings = GameImpl.Instance.CurrentStories[num3].PrefabSettings;
							if (prefabSettings != null)
							{
								foreach (EquippedModelProperties equippedModel in prefabSettings.EquippedModels)
								{
									list.Insert(num2++, equippedModel.PrefabPath.ToString());
								}
							}
						}
						list.Insert(0, "");
						AddStringDropDown(gameObject, ref index, field, list);
						continue;
					}
					if (field.Name == "MaleMeshName" || field.Name == "FemaleMeshName")
					{
						List<string> list2 = new List<string>();
						list2.Add("");
						for (int num4 = GameImpl.Instance.CurrentStories.Count - 1; num4 >= 0; num4--)
						{
							foreach (PrefabResource prefab2 in GameImpl.Instance.CurrentStories[num4].Prefabs)
							{
								if (prefab2.IsSkinned() && !list2.Contains(prefab2.GetPath()))
								{
									list2.Add(prefab2.GetPath());
								}
							}
						}
						foreach (KeyValuePair<string, GameObject> item in Human.ClothingMeshByName)
						{
							if (!list2.Contains(item.Key))
							{
								list2.Add(item.Key);
							}
						}
						AddStringDropDown(gameObject, ref index, field, list2);
						continue;
					}
					if (field.Name == "DefaultLiquidType")
					{
						List<string> list3 = new List<string>();
						list3.Add("");
						foreach (KeyValuePair<string, LiquidPrototype> item2 in GameImpl.Instance.CurrentLiquidPrototypesDeterministic)
						{
							list3.Add(item2.Key);
						}
						AddStringDropDown(gameObject, ref index, field, list3);
						continue;
					}
					if (field.Name == "HitEffectName")
					{
						List<string> list4 = new List<string>();
						list4.Add("");
						for (int num5 = GameImpl.Instance.CurrentStories.Count - 1; num5 >= 0; num5--)
						{
							foreach (PrefabResource prefab3 in GameImpl.Instance.CurrentStories[num5].Prefabs)
							{
								if (!prefab3.IsSkinned() && !list4.Contains(prefab3.GetPath()))
								{
									list4.Add(prefab3.GetPath());
								}
							}
						}
						PrefabResource[] effectPrefabs = SpecialEffectManager.Instance.EffectPrefabs;
						foreach (PrefabResource prefabResource in effectPrefabs)
						{
							list4.Add(prefabResource.GetPath());
						}
						AddStringDropDown(gameObject, ref index, field, list4);
						continue;
					}
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, (string)field.GetValue(Selected), delegate(string v9)
					{
						if (field.Name == "Name" && v9 != (string)field.GetValue(Selected))
						{
							if (string.IsNullOrEmpty(v9))
							{
								return;
							}
							CheckUniqueID(ref v9);
						}
						localField.SetValue(Selected, v9);
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(Vector3))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((Vector3)field.GetValue(Selected)).ToString("F4"), delegate(string str)
					{
						localField.SetValue(Selected, StringUtil.ParseVector3(str));
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(Color32[]))
				{
					Color32[] curArray = (Color32[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray != null) ? curArray.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							Color32[] array2 = new Color32[result];
							if (curArray != null)
							{
								Array.Copy(curArray, array2, Math.Min(curArray.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray == null)
					{
						continue;
					}
					for (int num7 = 0; num7 < curArray.Length; num7++)
					{
						int local_i = num7;
						BaseMenu.AddColorField(gameObject, ref index, "EDITOR_" + field.Name, num7, curArray[num7], delegate(string hex)
						{
							if (local_i < curArray.Length)
							{
								curArray[local_i] = StringUtil.HexToColor(hex);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.FieldType == typeof(string[]) && field.Name == "MaterialVariations")
				{
					string[] curArray2 = (string[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray2 != null) ? curArray2.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							string[] array2 = new string[result];
							if (curArray2 != null)
							{
								Array.Copy(curArray2, array2, Math.Min(curArray2.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray2 == null)
					{
						continue;
					}
					List<string> options = new List<string>();
					options.Add("");
					foreach (KeyValuePair<string, Material> item3 in Human.ClothingMaterialsByName)
					{
						options.Add(item3.Key);
					}
					for (int num8 = 0; num8 < curArray2.Length; num8++)
					{
						int local_i2 = num8;
						int v = options.IndexOf(curArray2[num8]);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num8, null, v, options, delegate(int index2)
						{
							if (local_i2 < curArray2.Length)
							{
								curArray2[local_i2] = options[index2];
							}
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<LootableFrom>))
				{
					List<string> options2 = BaseMenu.GetLootLocationOptions();
					List<string> list5 = new List<string>(StringUtil.GetEnumNames<LootScarcity>());
					list5[0] = "Default Scarcity";
					List<LootableFrom> list6 = (List<LootableFrom>)field.GetValue(Selected);
					for (int num9 = 0; num9 <= (list6?.Count ?? 0); num9++)
					{
						int localIndex = num9;
						if (list6 != null && num9 < list6.Count)
						{
							int v2 = Math.Max(0, options2.IndexOf(list6[num9].Name.ToString()));
							int v3 = Math.Max(0, list5.IndexOf(list6[num9].OverrideScarcity.ToString()));
							BaseMenu.AddDoubleDropDown(gameObject, ref index, "EDITOR_" + field.Name, num9, (num9 > 0) ? "EDITOR_OR" : null, v2, options2, delegate(int index2)
							{
								string value = options2[index2];
								if (list6 == null)
								{
									list6 = new List<LootableFrom>();
									field.SetValue(Selected, list6);
								}
								if (localIndex >= list6.Count)
								{
									list6.Add(new LootableFrom());
								}
								list6[localIndex].Name = value;
								if (string.IsNullOrEmpty(value) && localIndex >= 0 && localIndex < list6.Count)
								{
									list6.RemoveAt(localIndex);
								}
								HasChanged = true;
								WantRepopulate = true;
							}, v3, list5, delegate(int overrideScarcity)
							{
								list6[localIndex].OverrideScarcity = (LootScarcity)overrideScarcity;
								HasChanged = true;
							});
							continue;
						}
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name + "_New", num9, (num9 > 0) ? "EDITOR_OR" : null, 0, options2, delegate(int index2)
						{
							string text5 = options2[index2];
							if (list6 == null)
							{
								list6 = new List<LootableFrom>();
								field.SetValue(Selected, list6);
							}
							if (localIndex >= list6.Count)
							{
								list6.Add(new LootableFrom());
							}
							list6[localIndex].Name = text5;
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<GenderInLanguage>))
				{
					List<string> list7 = new List<string>(StringUtil.GetEnumNames<Language>());
					List<string> list8 = new List<string>(StringUtil.GetEnumNames<GenderType>());
					list8.Add("Neuter");
					List<GenderInLanguage> list9 = (List<GenderInLanguage>)field.GetValue(Selected);
					for (int num10 = 0; num10 <= (list9?.Count ?? 0); num10++)
					{
						int localIndex2 = num10;
						if (list9 != null && num10 < list9.Count)
						{
							int v4 = Math.Max(0, list7.IndexOf(list9[num10].Language.ToString()));
							int v5 = Math.Clamp((int)list9[num10].Gender, 0, 2);
							BaseMenu.AddDoubleDropDown(gameObject, ref index, "EDITOR_" + field.Name, num10, (num10 > 0) ? "EDITOR_AND" : null, v4, list7, delegate(int num15)
							{
								Language language = (Language)(num15 - 1);
								if (list9 == null)
								{
									list9 = new List<GenderInLanguage>();
									field.SetValue(Selected, list9);
								}
								if (localIndex2 >= list9.Count)
								{
									list9.Add(new GenderInLanguage());
								}
								list9[localIndex2].Language = language;
								if (language == Language.Invalid && localIndex2 >= 0 && localIndex2 < list9.Count)
								{
									list9.RemoveAt(localIndex2);
								}
								HasChanged = true;
								WantRepopulate = true;
							}, v5, list8, delegate(int gender)
							{
								list9[localIndex2].Gender = (GenderType)gender;
								HasChanged = true;
							});
							continue;
						}
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name + "_New", num10, (num10 > 0) ? "EDITOR_AND" : null, 0, list7, delegate(int num15)
						{
							Language language = (Language)(num15 - 1);
							if (list9 == null)
							{
								list9 = new List<GenderInLanguage>();
								field.SetValue(Selected, list9);
							}
							if (localIndex2 >= list9.Count)
							{
								list9.Add(new GenderInLanguage());
							}
							list9[localIndex2].Language = language;
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<string>))
				{
					string text = ((field.Name == "GiftFor") ? "EDITOR_AND" : "EDITOR_OR");
					List<string> options3;
					if (field.Name == "GiftFor" || field.Name == "BadGiftFor")
					{
						options3 = GameImpl.Instance.GetAllPersonalities();
						options3.Insert(1, Character.Anyone);
					}
					else if (field.Name == "AmmoTypes" || field.Name == "ExtraAmmoTypeForWeapons")
					{
						options3 = BaseMenu.GetEquipmentOptions();
					}
					else
					{
						options3 = BaseMenu.GetSoundOptions();
					}
					List<string> list10 = (List<string>)field.GetValue(Selected);
					for (int num11 = 0; num11 <= (list10?.Count ?? 0); num11++)
					{
						int localIndex3 = num11;
						int v6 = ((list10 != null && num11 < list10.Count) ? options3.IndexOf(list10[num11].ToString()) : 0);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num11, (num11 > 0) ? text : null, v6, options3, delegate(int index2)
						{
							string text5 = options3[index2];
							if (list10 == null)
							{
								list10 = new List<string>();
								field.SetValue(Selected, list10);
							}
							if (localIndex3 >= list10.Count)
							{
								list10.Add(text5);
							}
							else
							{
								list10[localIndex3] = text5;
							}
							if (string.IsNullOrEmpty(text5) && localIndex3 >= 0 && localIndex3 < list10.Count)
							{
								list10.RemoveAt(localIndex3);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.Name == "FoodForAnimal")
				{
					string text2 = "EDITOR_AND";
					List<string> options4 = BaseMenu.GetAnimalOptions(includeHuman: false);
					List<BaseObjectType> list11 = (List<BaseObjectType>)field.GetValue(Selected);
					for (int num12 = 0; num12 <= (list11?.Count ?? 0); num12++)
					{
						int localIndex4 = num12;
						int v7 = ((list11 != null && num12 < list11.Count) ? options4.IndexOf(list11[num12].ToString()) : 0);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num12, (num12 > 0) ? text2 : null, v7, options4, delegate(int index2)
						{
							BaseObjectType baseObjectType = (BaseObjectType)Math.Max(0, Array.IndexOf(BaseObjectManager.BaseObjectNames, options4[index2]));
							if (list11 == null)
							{
								list11 = new List<BaseObjectType>();
								field.SetValue(Selected, list11);
							}
							if (localIndex4 >= list11.Count)
							{
								list11.Add(baseObjectType);
							}
							else
							{
								list11[localIndex4] = baseObjectType;
							}
							if (baseObjectType == BaseObjectType.Invalid && localIndex4 >= 0 && localIndex4 < list11.Count)
							{
								list11.RemoveAt(localIndex4);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else
				{
					if (!field.FieldType.IsEnum)
					{
						continue;
					}
					Type enumType = field.FieldType;
					FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
					List<string> options5 = new List<string>();
					if (field.Name == "TypeName")
					{
						for (int num13 = 0; num13 < 244; num13++)
						{
							if (BaseObjectManager.PrototypeGameObjects[num13] is Equipment)
							{
								options5.Add(BaseObjectManager.BaseObjectNames[num13]);
							}
						}
					}
					else if (field.Name == "Gender")
					{
						options5.Add("Male");
						options5.Add("Female");
						options5.Add("Neuter");
					}
					else
					{
						FieldInfo[] array = fields2;
						for (int num6 = 0; num6 < array.Length; num6++)
						{
							string text3 = array[num6].GetValue(null).ToString();
							if (!(text3 == "Count"))
							{
								options5.Add(text3);
							}
						}
					}
					string text4 = field.GetValue(Selected).ToString();
					if (text4 == "Count" && field.Name == "Gender")
					{
						text4 = "Neuter";
					}
					int v8 = options5.IndexOf(text4);
					BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, -1, null, v8, options5, delegate(int index2)
					{
						int num15 = (int)Enum.Parse(enumType, options5[index2]);
						localField.SetValue(Selected, num15);
						HasChanged = true;
						WantRepopulate = true;
					});
				}
			}
		}
		for (int num14 = gameObject.transform.childCount - 1; num14 >= index; num14--)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num14).gameObject);
		}
	}

	private void AddStringDropDown(GameObject panel, ref int index, FieldInfo field, List<string> options)
	{
		int v = options.IndexOf((string)field.GetValue(Selected));
		BaseMenu.AddDropDown(panel, ref index, "EDITOR_" + field.Name, -1, null, v, options, delegate(int index2)
		{
			field.SetValue(Selected, options[index2]);
			HasChanged = true;
		});
	}
}
