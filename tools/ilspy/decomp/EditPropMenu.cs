using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class EditPropMenu : BaseMenu
{
	private PropPrototype Selected;

	private string SelectedName;

	private bool HasChanged;

	public bool WantRepopulate;

	public static EditPropMenu Instance;

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		Instance = this;
		PaperTextureAmount = 0.25f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		WantRepopulate = true;
		Selected = null;
		if (GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes.Count > 0)
		{
			OnSelectProp(GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes.Values[0]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			SaveProp();
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			PopulatePropList();
			PopulatePropInfo();
			WantRepopulate = false;
		}
	}

	public void OnCreateNew()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterPropName"), "", multiline: false, readOnly: false);
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			CheckUniqueID(ref name);
			_ = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Props";
			PropPrototype propPrototype = new PropPrototype();
			propPrototype.Name = name;
			GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes[name] = propPrototype;
			OnSelectProp(propPrototype);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveProp();
		WantPop = true;
	}

	private void OnSelectProp(PropPrototype proto)
	{
		SaveProp();
		Selected = proto;
		SelectedName = proto?.Name;
		PopulatePropList();
		PopulatePropInfo();
	}

	private void SaveProp()
	{
		if (Selected == null || !HasChanged)
		{
			return;
		}
		string text = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Props";
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
			GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes.Remove(SelectedName);
			GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes[Selected.Name] = Selected;
			SelectedName = Selected.Name;
		}
		else
		{
			GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes[Selected.Name] = Selected;
		}
		HasChanged = false;
		GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
		GameImpl.Instance.BuildPrototypeLookupLists();
	}

	private void PopulatePropList()
	{
		GameObject gameObject = base.gameObject.transform.Find("PropList/Viewport/Content").gameObject;
		int num = 0;
		foreach (KeyValuePair<string, PropPrototype> propPrototype in GameImpl.Instance.GetCurrentlyEditingStory().PropPrototypes)
		{
			Transform transform = gameObject.transform.Find(propPrototype.Key);
			GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
			if (gameObject2 == null)
			{
				PropPrototype proto = propPrototype.Value;
				gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/IconButton"));
				gameObject2.name = propPrototype.Key;
				gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					OnSelectProp(proto);
				});
			}
			gameObject2.transform.Find("Text").gameObject.GetComponent<Text>().SetUnityText(propPrototype.Value.Name);
			gameObject2.transform.Find("Image").gameObject.GetComponent<RawImage>().texture = IconGenerator.Instance.GetIconForObjectType(propPrototype.Value, out var _, highlighted: false);
			gameObject2.transform.SetSiblingIndex(num);
			num++;
		}
		for (; num < gameObject.transform.childCount; num++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulatePropInfo()
	{
		GameObject gameObject = base.gameObject.FindChild("PropInfo/Viewport/Content");
		int index = 0;
		if (Selected != null)
		{
			Type type = BaseObjectManager.BaseObjectTypes[(int)Selected.TypeName];
			FieldInfo[] fields = typeof(PropPrototype).GetFields();
			foreach (FieldInfo field in fields)
			{
				FieldInfo localField = field;
				if (field.IsStatic || (field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0 && field.Name != "Name") || field.GetCustomAttribute<NotVisible>() != null)
				{
					continue;
				}
				OnlyVisibleForType customAttribute = field.GetCustomAttribute<OnlyVisibleForType>();
				if (customAttribute != null && !customAttribute.Matches(type))
				{
					continue;
				}
				NotVisibleForType customAttribute2 = field.GetCustomAttribute<NotVisibleForType>();
				if (customAttribute2 != null && customAttribute2.Matches(type))
				{
					continue;
				}
				OnlyVisibleForCategory customAttribute3 = field.GetCustomAttribute<OnlyVisibleForCategory>();
				if ((customAttribute3 != null && !customAttribute3.Matches(Selected.Category)) || (field.GetCustomAttribute<OnlyVisibleIfHasGarageDoor>() != null && !Selected.HasGarageDoor) || (field.GetCustomAttribute<OnlyVisibleIfHasConnectors>() != null && (Selected.Connectors == null || Selected.Connectors.Length == 0)) || (field.GetCustomAttribute<OnlyVisibleIfHasFrontOrRoadProp>() != null && string.IsNullOrEmpty(Selected.FrontPropType) && string.IsNullOrEmpty(Selected.RoadPropType)) || (field.GetCustomAttribute<OnlyVisibleIfHasFrontProp>() != null && string.IsNullOrEmpty(Selected.FrontPropType)) || (field.GetCustomAttribute<OnlyVisibleIfHasInventory>() != null && Selected.MaxInventoryWeight <= 0f) || (field.GetCustomAttribute<OnlyVisibleIfCanBeDestroyed>() != null && Selected.Flammability == Flammability.Invulnerable && !Selected.CanBeDemolished) || (field.GetCustomAttribute<OnlyVisibleIfCanBeRepaired>() != null && string.IsNullOrEmpty(Selected.RepairResourceType)) || (field.GetCustomAttribute<OnlyVisibleIfCanBeCaptured>() != null && string.IsNullOrEmpty(Selected.CaptureResourceType)) || (field.GetCustomAttribute<OnlyVisibleIfMultiplePrefabs>() != null && Selected.PrefabNames.Count <= 1) || (field.GetCustomAttribute<OnlyVisibleIfHasMaterialVariation>() != null && Selected.GetNumMaterialVariations() == 0) || (field.GetCustomAttribute<OnlyVisibleIfHasPosterVariation>() != null && Selected.GetNumPosterVariations() == 0) || (field.GetCustomAttribute<OnlyVisibleIfHasColorVariation>() != null && Selected.GetNumColorVariations() == 0) || (field.GetCustomAttribute<OnlyVisibleIfHasColorVariation2>() != null && Selected.GetNumColorVariations2() == 0) || (field.GetCustomAttribute<OnlyVisibleIfHasColorVariation3>() != null && Selected.GetNumColorVariations3() == 0) || (field.GetCustomAttribute<OnlyVisibleIfHasColorVariation4>() != null && Selected.GetNumColorVariations4() == 0))
				{
					continue;
				}
				if (field.FieldType == typeof(bool))
				{
					BaseMenu.AddCheckbox(gameObject, ref index, "EDITOR_" + field.Name, -1, (bool)field.GetValue(Selected), delegate(bool flag)
					{
						localField.SetValue(Selected, flag);
						HasChanged = true;
						WantRepopulate = true;
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
					if (field.Name == "GarageDoorAngle")
					{
						List<string> list = new List<string>();
						list.Add("0°");
						list.Add("90°");
						list.Add("180°");
						list.Add("270°");
						int v = Mathf.RoundToInt((float)field.GetValue(Selected) / (MathF.PI / 2f));
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_EntranceAngle", -1, null, v, list, delegate(int num14)
						{
							localField.SetValue(Selected, (float)num14 * (MathF.PI / 2f));
							HasChanged = true;
						});
						continue;
					}
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((float)field.GetValue(Selected)).ToString(), delegate(string s)
					{
						if (float.TryParse(s, out var result))
						{
							localField.SetValue(Selected, result);
							HasChanged = true;
							WantRepopulate = true;
							if (field.Name == "IconScale")
							{
								Selected.UnloadIcon();
							}
						}
					});
				}
				else if (field.FieldType == typeof(Vector2))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((Vector2)field.GetValue(Selected)).ToString("F3"), delegate(string str)
					{
						localField.SetValue(Selected, StringUtil.ParseVector2(str));
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(Vector3))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((Vector3)field.GetValue(Selected)).ToString("F3"), delegate(string str)
					{
						localField.SetValue(Selected, StringUtil.ParseVector3(str));
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(TerrainCoord))
				{
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, ((TerrainCoord)field.GetValue(Selected)/*cast due to .constrained prefix*/).ToString(), delegate(string str)
					{
						localField.SetValue(Selected, StringUtil.ParseTerrainCoord(str));
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(Color32))
				{
					BaseMenu.AddColorField(gameObject, ref index, "EDITOR_" + field.Name, -1, (Color32)field.GetValue(Selected), delegate(string hex)
					{
						localField.SetValue(Selected, StringUtil.HexToColor(hex));
						HasChanged = true;
						WantRepopulate = true;
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
					for (int num = 0; num < curArray.Length; num++)
					{
						int local_i = num;
						BaseMenu.AddColorField(gameObject, ref index, "EDITOR_" + field.Name, num, curArray[num], delegate(string hex)
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
				else if (field.FieldType == typeof(string))
				{
					if (field.Name == "RepairResourceType" || field.Name == "CaptureResourceType" || field.Name == "HarvestType" || field.Name == "HarvestSeedsType" || field.Name == "GrabbableEquipmentType")
					{
						List<string> equipmentOptions = BaseMenu.GetEquipmentOptions();
						AddStringDropDown(gameObject, ref index, Selected, field, -1, equipmentOptions);
						continue;
					}
					if (field.Name == "LiquidType")
					{
						List<string> liquidOptions = BaseMenu.GetLiquidOptions();
						AddStringDropDown(gameObject, ref index, Selected, field, -1, liquidOptions);
						continue;
					}
					if (field.Name == "FrontPropType" || field.Name == "RoadPropType" || field.Name == "FallbackPropType")
					{
						List<string> propOptions = BaseMenu.GetPropOptions();
						AddStringDropDown(gameObject, ref index, Selected, field, -1, propOptions);
						continue;
					}
					if (field.Name == "LootLocation")
					{
						List<string> lootLocationOptions = BaseMenu.GetLootLocationOptions();
						AddStringDropDown(gameObject, ref index, Selected, field, -1, lootLocationOptions);
						continue;
					}
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, (string)field.GetValue(Selected), delegate(string v7)
					{
						if (field.Name == "Name" && v7 != (string)field.GetValue(Selected))
						{
							if (string.IsNullOrEmpty(v7))
							{
								return;
							}
							CheckUniqueID(ref v7);
						}
						localField.SetValue(Selected, v7);
						HasChanged = true;
					});
				}
				else if (field.FieldType == typeof(List<GenderInLanguage>))
				{
					if (Selected.Gender == GenderType.Count)
					{
						continue;
					}
					List<string> list2 = new List<string>(StringUtil.GetEnumNames<Language>());
					List<string> list3 = new List<string>(StringUtil.GetEnumNames<GenderType>());
					list3.Add("Neuter");
					List<GenderInLanguage> list4 = (List<GenderInLanguage>)field.GetValue(Selected);
					for (int num2 = 0; num2 <= (list4?.Count ?? 0); num2++)
					{
						int localIndex = num2;
						if (list4 != null && num2 < list4.Count)
						{
							int v2 = Math.Max(0, list2.IndexOf(list4[num2].Language.ToString()));
							int v3 = Math.Max(0, list3.IndexOf(list4[num2].Gender.ToString()));
							BaseMenu.AddDoubleDropDown(gameObject, ref index, "EDITOR_" + field.Name, num2, (num2 > 0) ? "EDITOR_OR" : null, v2, list2, delegate(int num14)
							{
								Language language = (Language)(num14 - 1);
								if (list4 == null)
								{
									list4 = new List<GenderInLanguage>();
									field.SetValue(Selected, list4);
								}
								if (localIndex >= list4.Count)
								{
									list4.Add(new GenderInLanguage());
								}
								list4[localIndex].Language = language;
								if (language == Language.Invalid && localIndex >= 0 && localIndex < list4.Count)
								{
									list4.RemoveAt(localIndex);
								}
								HasChanged = true;
								WantRepopulate = true;
							}, v3, list3, delegate(int gender)
							{
								list4[localIndex].Gender = (GenderType)gender;
								HasChanged = true;
							});
							continue;
						}
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name + "_New", num2, (num2 > 0) ? "EDITOR_OR" : null, 0, list2, delegate(int num14)
						{
							Language language = (Language)(num14 - 1);
							if (list4 == null)
							{
								list4 = new List<GenderInLanguage>();
								field.SetValue(Selected, list4);
							}
							if (localIndex >= list4.Count)
							{
								list4.Add(new GenderInLanguage());
							}
							list4[localIndex].Language = language;
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<string>))
				{
					List<string> options;
					if (field.Name == "PrefabNames" || field.Name == "DeadPrefabNames" || field.Name == "OpenedPrefabNames" || field.Name == "Stage1PrefabNames" || field.Name == "Stage2PrefabNames")
					{
						options = BaseMenu.GetPrefabOptions();
					}
					else if (field.Name == "MaterialVariationNames" || field.Name == "PosterVariationNames")
					{
						options = BaseMenu.GetMaterialOptions();
					}
					else
					{
						options = new List<string>();
					}
					List<string> list5 = (List<string>)field.GetValue(Selected);
					for (int num3 = 0; num3 <= (list5?.Count ?? 0); num3++)
					{
						int localIndex2 = num3;
						int v4 = ((list5 != null && num3 < list5.Count) ? options.IndexOf(list5[num3].ToString()) : 0);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num3, null, v4, options, delegate(int index2)
						{
							string text2 = options[index2];
							if (list5 == null)
							{
								list5 = new List<string>();
								field.SetValue(Selected, list5);
							}
							if (localIndex2 >= list5.Count)
							{
								list5.Add(text2);
							}
							else
							{
								list5[localIndex2] = text2;
							}
							if (string.IsNullOrEmpty(text2) && localIndex2 >= 0 && localIndex2 < list5.Count)
							{
								list5.RemoveAt(localIndex2);
							}
							Selected.CacheStuff();
							Selected.UnloadIcon();
							WantRepopulate = true;
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<Vector3>) && field.Name == "ModelOffset")
				{
					List<Vector3> list6 = (List<Vector3>)field.GetValue(Selected);
					if (list6 == null)
					{
						list6 = new List<Vector3>();
						field.SetValue(Selected, list6);
					}
					if (list6.Count > Selected.PrefabNames.Count)
					{
						list6.RemoveRange(Selected.PrefabNames.Count, list6.Count - Selected.PrefabNames.Count);
					}
					else
					{
						while (list6.Count < Selected.PrefabNames.Count)
						{
							list6.Add((list6.Count > 0) ? list6[list6.Count - 1] : Vector3.zero);
						}
					}
					for (int num4 = 0; num4 < list6.Count; num4++)
					{
						int localIndex3 = num4;
						Vector3 vector = ((list6 != null && num4 < list6.Count) ? list6[num4] : Vector3.zero);
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, num4, vector.ToString("F3"), delegate(string str)
						{
							Vector3 value = StringUtil.ParseVector3(str);
							list6[localIndex3] = value;
							Selected.UnloadIcon();
							WantRepopulate = true;
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<Vector2>) && field.Name == "MinMaxScale")
				{
					List<Vector2> list7 = (List<Vector2>)field.GetValue(Selected);
					if (list7 == null)
					{
						list7 = new List<Vector2>();
						field.SetValue(Selected, list7);
					}
					if (list7.Count > Selected.PrefabNames.Count)
					{
						list7.RemoveRange(Selected.PrefabNames.Count, list7.Count - Selected.PrefabNames.Count);
					}
					else
					{
						while (list7.Count < Selected.PrefabNames.Count)
						{
							list7.Add((list7.Count > 0) ? list7[list7.Count - 1] : Vector2.zero);
						}
					}
					for (int num5 = 0; num5 < list7.Count; num5++)
					{
						int localIndex4 = num5;
						Vector2 vector2 = ((list7 != null && num5 < list7.Count) ? list7[num5] : Vector2.zero);
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, num5, vector2.ToString("F3"), delegate(string str)
						{
							Vector3 vector3 = StringUtil.ParseVector2(str);
							list7[localIndex4] = vector3;
							Selected.UnloadIcon();
							WantRepopulate = true;
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType.IsEnum)
				{
					Type enumType = field.FieldType;
					FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
					List<string> options2;
					if (field.Name == "TypeName")
					{
						options2 = new List<string>();
						options2.Add(BaseObjectType.Prop.ToString());
						options2.Add(BaseObjectType.SingleTileProp.ToString());
						options2.Add(BaseObjectType.TiltedProp.ToString());
						options2.Add(BaseObjectType.Building.ToString());
						options2.Add(BaseObjectType.TiltedBuilding.ToString());
						options2.Add(BaseObjectType.EnterableVehicle.ToString());
						options2.Add(BaseObjectType.Outhouse.ToString());
						options2.Add(BaseObjectType.Well.ToString());
						options2.Add(BaseObjectType.Campfire.ToString());
						options2.Add(BaseObjectType.WorkBench.ToString());
						options2.Add(BaseObjectType.Still.ToString());
						options2.Add(BaseObjectType.Kiln.ToString());
						options2.Add(BaseObjectType.Forge.ToString());
						options2.Add(BaseObjectType.Nitrary.ToString());
						options2.Add(BaseObjectType.Mine.ToString());
						options2.Add(BaseObjectType.Rock.ToString());
						options2.Add(BaseObjectType.Boulder.ToString());
						options2.Add(BaseObjectType.Bush.ToString());
						options2.Add(BaseObjectType.Flower.ToString());
						options2.Add(BaseObjectType.PlantableCrop.ToString());
						options2.Add(BaseObjectType.BaseFence.ToString());
						options2.Add(BaseObjectType.Gate.ToString());
						options2.Add(BaseObjectType.BridgeWall.ToString());
						options2.Add(BaseObjectType.PitTrap.ToString());
						options2.Add(BaseObjectType.RabbitTrap.ToString());
						options2.Add(BaseObjectType.Tripwire.ToString());
						options2.Add(BaseObjectType.TrapsSign.ToString());
						options2.Add(BaseObjectType.Grave.ToString());
						options2.Add(BaseObjectType.Trash.ToString());
						options2.Add(BaseObjectType.AnimalDrinkerProp.ToString());
						options2.Add(BaseObjectType.AnimalFeederProp.ToString());
						options2.Add(BaseObjectType.Snowman.ToString());
					}
					else if (field.Name == "EnterableBySpecies")
					{
						options2 = BaseMenu.GetAnimalOptions(includeHuman: true);
					}
					else if (field.Name == "Gender")
					{
						options2 = new List<string>();
						options2.Add("Male");
						options2.Add("Female");
						options2.Add("Neuter");
					}
					else
					{
						options2 = new List<string>();
						FieldInfo[] array = fields2;
						for (int num6 = 0; num6 < array.Length; num6++)
						{
							string text = array[num6].GetValue(null).ToString();
							if (!(text == "Count"))
							{
								options2.Add(text);
							}
						}
					}
					int v5 = options2.IndexOf(field.GetValue(Selected).ToString());
					BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, -1, null, v5, options2, delegate(int index2)
					{
						int num14 = (int)Enum.Parse(enumType, options2[index2]);
						localField.SetValue(Selected, num14);
						HasChanged = true;
						WantRepopulate = true;
						Selected.ProtoInstance = null;
					});
				}
				else if (field.FieldType == typeof(EntranceDef[]))
				{
					EntranceDef[] curArray2 = (EntranceDef[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray2 != null) ? curArray2.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							EntranceDef[] array2 = new EntranceDef[result];
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
					for (int num7 = 0; num7 < curArray2.Length; num7++)
					{
						int local_i2 = num7;
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_EntranceOffset", num7, curArray2[num7].EntranceOffset.ToString(), delegate(string str)
						{
							if (local_i2 < curArray2.Length)
							{
								curArray2[local_i2].EntranceOffset = StringUtil.ParseTerrainCoord(str);
							}
							HasChanged = true;
						});
						List<string> list8 = new List<string>();
						list8.Add("0°");
						list8.Add("45°");
						list8.Add("90°");
						list8.Add("135°");
						list8.Add("180°");
						list8.Add("225°");
						list8.Add("270°");
						list8.Add("315°");
						int v6 = Mathf.RoundToInt(curArray2[num7].EntranceAngle / (MathF.PI / 4f));
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_EntranceAngle", num7, null, v6, list8, delegate(int num14)
						{
							if (local_i2 < curArray2.Length)
							{
								curArray2[local_i2].EntranceAngle = (float)num14 * (MathF.PI / 4f);
							}
							HasChanged = true;
						});
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_EntranceName", num7, curArray2[num7].NativeName, delegate(string nativeName)
						{
							if (local_i2 < curArray2.Length)
							{
								curArray2[local_i2].NativeName = nativeName;
							}
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(InhabitantSlotDef[]))
				{
					InhabitantSlotDef[] curArray3 = (InhabitantSlotDef[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray3 != null) ? curArray3.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							InhabitantSlotDef[] array2 = new InhabitantSlotDef[result];
							if (curArray3 != null)
							{
								Array.Copy(curArray3, array2, Math.Min(curArray3.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray3 == null)
					{
						continue;
					}
					for (int num8 = 0; num8 < curArray3.Length; num8++)
					{
						int local_i3 = num8;
						BaseMenu.AddCheckbox(gameObject, ref index, "EDITOR_InhabitantExternal", num8, curArray3[num8].External, delegate(bool external)
						{
							if (local_i3 < curArray3.Length)
							{
								curArray3[local_i3].External = external;
							}
							HasChanged = true;
							WantRepopulate = true;
						});
						if (curArray3[num8].External)
						{
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_SlotName", num8, curArray3[num8].NativeName, delegate(string nativeName)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].NativeName = nativeName;
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_InhabitantPos", num8, curArray3[num8].Pos.ToString("F3"), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].Pos = StringUtil.ParseVector3(str);
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_PipFocusDist", num8, curArray3[num8].PipFocusDist.ToString("F3"), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].PipFocusDist = StringUtil.ParseFloat(str);
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_WeaponRangeModifier", num8, curArray3[num8].WeaponRangeModifier.ToString(), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].WeaponRangeModifier = StringUtil.ParseInt(str);
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_MinWeaponRange", num8, curArray3[num8].MinWeaponRange.ToString(), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].MinWeaponRange = StringUtil.ParseInt(str);
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_DefaultAngle", num8, curArray3[num8].DefaultAngle.ToString(), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].DefaultAngle = StringUtil.ParseFloat(str);
								}
								HasChanged = true;
							});
							BaseMenu.AddInputField(gameObject, ref index, "EDITOR_RangeAngle", num8, curArray3[num8].RangeAngle.ToString(), delegate(string str)
							{
								if (local_i3 < curArray3.Length)
								{
									curArray3[local_i3].RangeAngle = StringUtil.ParseFloat(str);
								}
								HasChanged = true;
							});
						}
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_CamDist", num8, curArray3[num8].CamDist.ToString("F3"), delegate(string str)
						{
							if (local_i3 < curArray3.Length)
							{
								curArray3[local_i3].CamDist = StringUtil.ParseFloat(str);
							}
							HasChanged = true;
						});
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_SightRangeModifier", num8, curArray3[num8].SightRangeModifier.ToString(), delegate(string str)
						{
							if (local_i3 < curArray3.Length)
							{
								curArray3[local_i3].SightRangeModifier = StringUtil.ParseInt(str);
							}
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(ChildPropDef[]))
				{
					ChildPropDef[] curArray4 = (ChildPropDef[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray4 != null) ? curArray4.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							ChildPropDef[] array2 = new ChildPropDef[result];
							if (curArray4 != null)
							{
								Array.Copy(curArray4, array2, Math.Min(curArray4.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray4 == null)
					{
						continue;
					}
					for (int num9 = 0; num9 < curArray4.Length; num9++)
					{
						int local_i4 = num9;
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_ChildOffset", num9, curArray4[num9].Offset.ToString(), delegate(string str)
						{
							if (local_i4 < curArray4.Length)
							{
								curArray4[local_i4].Offset = StringUtil.ParseTerrainCoord(str);
							}
							HasChanged = true;
						});
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_ChildExtentsMin", num9, curArray4[num9].ExtentsMin.ToString(), delegate(string str)
						{
							if (local_i4 < curArray4.Length)
							{
								curArray4[local_i4].ExtentsMin = StringUtil.ParseTerrainCoord(str);
							}
							HasChanged = true;
						});
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_ChildExtentsMax", num9, curArray4[num9].ExtentsMax.ToString(), delegate(string str)
						{
							if (local_i4 < curArray4.Length)
							{
								curArray4[local_i4].ExtentsMax = StringUtil.ParseTerrainCoord(str);
							}
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(Vector2[]))
				{
					Vector2[] curArray5 = (Vector2[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray5 != null) ? curArray5.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							Vector2[] array2 = new Vector2[result];
							if (curArray5 != null)
							{
								Array.Copy(curArray5, array2, Math.Min(curArray5.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray5 == null)
					{
						continue;
					}
					for (int num10 = 0; num10 < curArray5.Length; num10++)
					{
						int local_i5 = num10;
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, num10, curArray5[num10].ToString("F3"), delegate(string str)
						{
							if (local_i5 < curArray5.Length)
							{
								curArray5[local_i5] = StringUtil.ParseVector2(str);
							}
							HasChanged = true;
						});
					}
				}
				else if (field.FieldType == typeof(Vector3[]))
				{
					Vector3[] curArray6 = (Vector3[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray6 != null) ? curArray6.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							Vector3[] array2 = new Vector3[result];
							if (curArray6 != null)
							{
								Array.Copy(curArray6, array2, Math.Min(curArray6.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray6 == null)
					{
						continue;
					}
					for (int num11 = 0; num11 < curArray6.Length; num11++)
					{
						int local_i6 = num11;
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, num11, curArray6[num11].ToString("F3"), delegate(string str)
						{
							if (local_i6 < curArray6.Length)
							{
								curArray6[local_i6] = StringUtil.ParseVector3(str);
							}
							HasChanged = true;
						});
					}
				}
				else
				{
					if (!(field.FieldType == typeof(float[])))
					{
						continue;
					}
					float[] curArray7 = (float[])field.GetValue(Selected);
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name + "_Count", -1, ((curArray7 != null) ? curArray7.Length : 0).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							float[] array2 = new float[result];
							if (curArray7 != null)
							{
								Array.Copy(curArray7, array2, Math.Min(curArray7.Length, array2.Length));
							}
							localField.SetValue(Selected, array2);
							HasChanged = true;
							WantRepopulate = true;
						}
					});
					if (curArray7 == null)
					{
						continue;
					}
					for (int num12 = 0; num12 < curArray7.Length; num12++)
					{
						int local_i7 = num12;
						BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, num12, curArray7[num12].ToString("F3"), delegate(string str)
						{
							if (local_i7 < curArray7.Length)
							{
								curArray7[local_i7] = StringUtil.ParseFloat(str);
							}
							HasChanged = true;
						});
					}
				}
			}
		}
		for (int num13 = gameObject.transform.childCount - 1; num13 >= index; num13--)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num13).gameObject);
		}
	}

	private void AddStringDropDown(GameObject panel, ref int index, object obj, FieldInfo field, int arrayIndex, List<string> options)
	{
		int v = Math.Max(0, options.IndexOf((string)field.GetValue(obj)));
		BaseMenu.AddDropDown(panel, ref index, "EDITOR_" + field.Name, arrayIndex, null, v, options, delegate(int index2)
		{
			field.SetValue(obj, options[index2]);
			HasChanged = true;
			WantRepopulate = true;
		});
	}
}
