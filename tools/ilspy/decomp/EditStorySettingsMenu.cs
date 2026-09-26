using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class EditStorySettingsMenu : BaseMenu
{
	public enum Tab
	{
		Settings,
		Equipment,
		Loadout,
		Difficulty
	}

	private TabBehaviour UnitySettingsTab;

	private TabBehaviour UnityEquipmentTab;

	private TabBehaviour UnityLoadoutTab;

	private TabBehaviour UnityDifficultyTab;

	private GameObject UnityContents;

	public bool WantRepopulate;

	public bool HasChanged;

	public bool WantDeleteAllChildren;

	public Tab CurrentTab;

	public int CurrentLoadout = -1;

	public int CurrentDifficulty = -1;

	public void SetTab(Tab tab)
	{
		CurrentTab = tab;
		UnitySettingsTab.Selected = tab == Tab.Settings;
		UnityEquipmentTab.Selected = tab == Tab.Equipment;
		UnityLoadoutTab.Selected = tab == Tab.Loadout;
		UnityDifficultyTab.Selected = tab == Tab.Difficulty;
		UnityContents.DeleteAllChildren();
		WantRepopulate = true;
	}

	public override void AwakeImpl()
	{
		base.AwakeImpl();
		PaperTextureAmount = 0.25f;
	}

	public override void OnActivate()
	{
		base.OnActivate();
		UnitySettingsTab = base.gameObject.FindChild("Panel/TabSettings").GetComponent<TabBehaviour>();
		UnityEquipmentTab = base.gameObject.FindChild("Panel/TabEquipment").GetComponent<TabBehaviour>();
		UnityLoadoutTab = base.gameObject.FindChild("Panel/TabLoadout").GetComponent<TabBehaviour>();
		UnityDifficultyTab = base.gameObject.FindChild("Panel/TabDifficulty").GetComponent<TabBehaviour>();
		UnityContents = base.gameObject.FindChild("Fields/Viewport/Content");
		SetTab(Tab.Settings);
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantDeleteAllChildren)
		{
			UnityContents.DeleteAllChildrenImmediately();
			WantDeleteAllChildren = false;
		}
		if (WantRepopulate)
		{
			PopulateFields();
			WantRepopulate = false;
		}
	}

	private void SaveStorySettings()
	{
		if (HasChanged)
		{
			Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
			currentlyEditingStory.Settings.SaveToFile(currentlyEditingStory.Path + "/Settings.xml");
			HasChanged = false;
			GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
			GameImpl.Instance.BuildPrototypeLookupLists();
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (CurrentTab != Tab.Settings && UnitySettingsTab.Selected)
		{
			SetTab(Tab.Settings);
		}
		if (CurrentTab != Tab.Equipment && UnityEquipmentTab.Selected)
		{
			SetTab(Tab.Equipment);
		}
		if (CurrentTab != Tab.Loadout && UnityLoadoutTab.Selected)
		{
			SetTab(Tab.Loadout);
		}
		if (CurrentTab != Tab.Difficulty && UnityDifficultyTab.Selected)
		{
			SetTab(Tab.Difficulty);
		}
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SaveStorySettings();
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			WantPop = true;
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveStorySettings();
		GameImpl.Instance.ReloadCurrentStory(reloadFromDisk: false);
		WantPop = true;
	}

	public void PopulateFields()
	{
		Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
		StorySettings storySettings = currentlyEditingStory.Settings;
		UnityEquipmentTab.gameObject.SetActive(!storySettings.IsMod);
		UnityLoadoutTab.gameObject.SetActive(!storySettings.IsMod);
		UnityDifficultyTab.gameObject.SetActive(!storySettings.IsMod);
		GameObject unityContents = UnityContents;
		int index = 0;
		switch (CurrentTab)
		{
		case Tab.Settings:
		{
			FieldInfo[] fields = typeof(StorySettings).GetFields();
			foreach (FieldInfo field3 in fields)
			{
				FieldInfo localField3 = field3;
				if (field3.IsStatic || field3.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0)
				{
					continue;
				}
				IsModAttribute customAttribute = field3.GetCustomAttribute<IsModAttribute>();
				if (customAttribute != null && customAttribute.IsMod != storySettings.IsMod)
				{
					continue;
				}
				ProcedurallyGeneratedAttribute customAttribute2 = field3.GetCustomAttribute<ProcedurallyGeneratedAttribute>();
				if (customAttribute2 != null && customAttribute2.ProcedurallyGenerated != storySettings.ProcedurallyGenerated)
				{
					continue;
				}
				if (field3.FieldType == typeof(bool))
				{
					BaseMenu.AddCheckbox(unityContents, ref index, "EDITOR_" + field3.Name, -1, (bool)field3.GetValue(storySettings), delegate(bool flag)
					{
						localField3.SetValue(storySettings, flag);
						HasChanged = true;
						WantRepopulate = true;
					});
				}
				else if (field3.FieldType == typeof(int))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field3.Name, -1, ((int)field3.GetValue(storySettings)).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							localField3.SetValue(storySettings, result);
							HasChanged = true;
						}
					});
				}
				else if (field3.FieldType == typeof(float))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field3.Name, -1, ((float)field3.GetValue(storySettings)).ToString(), delegate(string s)
					{
						if (float.TryParse(s, out var result))
						{
							localField3.SetValue(storySettings, result);
							HasChanged = true;
						}
					});
				}
				else if (field3.FieldType == typeof(string))
				{
					if (field3.Name == "DefaultDifficulty")
					{
						if (storySettings.DifficultySettings.Count <= 1)
						{
							continue;
						}
						List<string> list3 = new List<string>();
						foreach (DifficultySettings difficultySetting in storySettings.DifficultySettings)
						{
							list3.Add(difficultySetting.DifficultyName);
						}
						AddStringDropDown(unityContents, ref index, storySettings, field3, -1, list3);
					}
					else
					{
						BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field3.Name, -1, (string)field3.GetValue(storySettings), delegate(string value)
						{
							localField3.SetValue(storySettings, value);
							HasChanged = true;
						});
					}
				}
				else if (field3.FieldType == typeof(List<string>))
				{
					List<string> list4 = (List<string>)field3.GetValue(storySettings);
					for (int num9 = 0; num9 <= (list4?.Count ?? 0); num9++)
					{
						int localIndex3 = num9;
						string v3 = ((list4 != null && num9 < list4.Count) ? list4[num9] : string.Empty);
						BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field3.Name, num9, showArrayIndex: true, v3, delegate(string text2)
						{
							if (list4 == null)
							{
								list4 = new List<string>();
								field3.SetValue(storySettings, list4);
							}
							if (localIndex3 >= list4.Count)
							{
								list4.Add(text2);
							}
							else
							{
								list4[localIndex3] = text2;
							}
							if (text2 == string.Empty && localIndex3 >= 0 && localIndex3 < list4.Count)
							{
								list4.RemoveAt(localIndex3);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field3.FieldType.IsEnum)
				{
					Type enumType = field3.FieldType;
					FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
					List<string> options3 = new List<string>();
					FieldInfo[] array = fields2;
					for (int num10 = 0; num10 < array.Length; num10++)
					{
						string text = array[num10].GetValue(null).ToString();
						if (text == "Count")
						{
							break;
						}
						options3.Add(text);
					}
					int v4 = options3.IndexOf(field3.GetValue(storySettings).ToString());
					BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_" + field3.Name, -1, null, v4, options3, delegate(int index2)
					{
						int num15 = (int)Enum.Parse(enumType, options3[index2]);
						localField3.SetValue(storySettings, num15);
						HasChanged = true;
					});
				}
				else if (field3.FieldType == typeof(StoryId))
				{
					List<StorySource> storySources = new List<StorySource>();
					List<string> options4 = new List<string>();
					GetStoryDropdownOptions(out storySources, out options4);
					StoryId storyId = (StoryId)field3.GetValue(storySettings);
					int v5 = 0;
					for (int num11 = 0; num11 < storySources.Count; num11++)
					{
						if (storyId.Folder == storySources[num11].Folder && storyId.WorkshopId == storySources[num11].WorkshopId)
						{
							v5 = num11;
						}
					}
					BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_" + field3.Name, -1, null, v5, options4, delegate(int index2)
					{
						StoryId storyId2 = new StoryId
						{
							Folder = storySources[index2].Folder,
							WorkshopId = storySources[index2].WorkshopId
						};
						localField3.SetValue(storySettings, storyId2);
						HasChanged = true;
						WantRepopulate = true;
					});
				}
				else
				{
					if (!(field3.FieldType == typeof(List<StoryId>)))
					{
						continue;
					}
					List<StorySource> storySources2 = new List<StorySource>();
					List<string> options5 = new List<string>();
					GetStoryDropdownOptions(out storySources2, out options5);
					List<StoryId> list5 = (List<StoryId>)field3.GetValue(storySettings);
					for (int num12 = 0; num12 <= (list5?.Count ?? 0); num12++)
					{
						int localIndex4 = num12;
						int v6 = 0;
						if (list5 != null && num12 < list5.Count)
						{
							for (int num13 = 0; num13 < storySources2.Count; num13++)
							{
								if (list5[num12].Folder == storySources2[num13].Folder && list5[num12].WorkshopId == storySources2[num13].WorkshopId)
								{
									v6 = num13;
								}
							}
						}
						BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_" + field3.Name, num12, (num12 > 0) ? "EDITOR_AND" : null, v6, options5, delegate(int index2)
						{
							StoryId storyId2 = new StoryId
							{
								Folder = storySources2[index2].Folder,
								WorkshopId = storySources2[index2].WorkshopId
							};
							if (list5 == null)
							{
								list5 = new List<StoryId>();
								field3.SetValue(storySettings, list5);
							}
							if (localIndex4 >= list5.Count)
							{
								list5.Add(storyId2);
							}
							else
							{
								list5[localIndex4] = storyId2;
							}
							if (string.IsNullOrEmpty(storyId2.Folder) && storyId2.WorkshopId == 0L && localIndex4 >= 0 && localIndex4 < list5.Count)
							{
								list5.RemoveAt(localIndex4);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
			}
			break;
		}
		case Tab.Equipment:
		{
			List<string> options2 = new List<string>();
			options2.Add("");
			foreach (KeyValuePair<string, EquipmentPrototype> item in GameImpl.Instance.CurrentEquipmentPrototypesDeterministic)
			{
				options2.Add(item.Key);
			}
			for (int num8 = 0; num8 <= storySettings.StartingEquipmentOptions.Count; num8++)
			{
				StartingEquipment startingEquipment2 = ((num8 < storySettings.StartingEquipmentOptions.Count) ? storySettings.StartingEquipmentOptions[num8] : null);
				int localIndex2 = num8;
				int v2 = ((startingEquipment2 != null) ? options2.IndexOf(startingEquipment2.Name.ToString()) : 0);
				BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_StartingEquipmentName", num8, null, v2, options2, delegate(int index2)
				{
					StartingEquipment startingEquipment3 = ((startingEquipment2 != null) ? startingEquipment2 : new StartingEquipment());
					startingEquipment3.Name = options2[index2];
					if (localIndex2 >= storySettings.StartingEquipmentOptions.Count)
					{
						storySettings.StartingEquipmentOptions.Add(startingEquipment3);
					}
					else
					{
						storySettings.StartingEquipmentOptions[localIndex2] = startingEquipment3;
					}
					if (string.IsNullOrEmpty(startingEquipment3.Name) && localIndex2 >= 0 && localIndex2 < storySettings.StartingEquipmentOptions.Count)
					{
						storySettings.StartingEquipmentOptions.RemoveAt(localIndex2);
					}
					HasChanged = true;
					WantRepopulate = true;
				});
				if (startingEquipment2 == null)
				{
					continue;
				}
				BaseMenu.AddInputField(unityContents, ref index, "EDITOR_Amount", num8, startingEquipment2.Amount.ToString(), delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						startingEquipment2.Amount = result;
						HasChanged = true;
					}
				});
				BaseMenu.AddInputField(unityContents, ref index, "EDITOR_Points", num8, startingEquipment2.Points.ToString(), delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						startingEquipment2.Points = result;
						HasChanged = true;
					}
				});
			}
			break;
		}
		case Tab.Loadout:
		{
			if (storySettings.Loadouts.Count > 0)
			{
				CurrentLoadout = MathUtil.Clamp(CurrentLoadout, 0, storySettings.Loadouts.Count - 1);
				List<string> list2 = new List<string>();
				foreach (Loadout loadout2 in storySettings.Loadouts)
				{
					list2.Add(loadout2.LoadoutName);
				}
				BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_SelectLoadout", -1, null, CurrentLoadout, list2, delegate(int currentLoadout)
				{
					CurrentLoadout = currentLoadout;
					WantDeleteAllChildren = true;
					WantRepopulate = true;
				});
				BaseMenu.AddButtonField(unityContents, ref index, "EDITOR_DeleteLoadout", -1, delegate
				{
					storySettings.Loadouts.RemoveAt(CurrentLoadout);
					CurrentLoadout--;
					WantDeleteAllChildren = true;
					HasChanged = true;
					WantRepopulate = true;
				});
			}
			else
			{
				CurrentLoadout = -1;
			}
			BaseMenu.AddButtonField(unityContents, ref index, "EDITOR_NewLoadout", -1, delegate
			{
				CurrentLoadout = storySettings.Loadouts.Count;
				storySettings.Loadouts.Add(new Loadout());
				WantDeleteAllChildren = true;
				HasChanged = true;
				WantRepopulate = true;
			});
			if (CurrentLoadout < 0 || CurrentLoadout >= storySettings.Loadouts.Count)
			{
				break;
			}
			Loadout loadout = storySettings.Loadouts[CurrentLoadout];
			FieldInfo[] fields = typeof(Loadout).GetFields();
			foreach (FieldInfo field2 in fields)
			{
				FieldInfo localField2 = field2;
				if (!field2.IsStatic && field2.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length == 0 && field2.FieldType == typeof(string))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field2.Name, -1, (string)field2.GetValue(loadout), delegate(string value)
					{
						localField2.SetValue(loadout, value);
						HasChanged = true;
						WantRepopulate = field2.Name == "LoadoutName";
					});
				}
			}
			int num2 = 0;
			foreach (LoadoutEquipment item2 in loadout.Equipment)
			{
				StartingEquipment startingEquipment = storySettings.FindStartingEquipment(item2.Name);
				if (startingEquipment != null)
				{
					num2 += startingEquipment.Points * item2.Amount;
				}
			}
			BaseMenu.AddTitleField(unityContents, ref index, GameImpl.Translate("EDITOR_LoadoutEquipmentTitle").Replace("%1", num2.ToString()).Replace("%2", storySettings.EquipmentPoints.ToString()), -1, null);
			List<string> options = new List<string>();
			options.Add("");
			foreach (StartingEquipment startingEquipmentOption in storySettings.StartingEquipmentOptions)
			{
				EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(startingEquipmentOption.Name);
				if (equipmentPrototype != null && (equipmentPrototype.ClothingType == ClothingType.Invalid || equipmentPrototype.ClothingType == ClothingType.Backpack || equipmentPrototype.ClothingType == ClothingType.BodyArmor || equipmentPrototype.ClothingType == ClothingType.LegArmor))
				{
					options.Add(startingEquipmentOption.Name);
				}
			}
			for (int num3 = 0; num3 <= loadout.Equipment.Count; num3++)
			{
				LoadoutEquipment loadoutEquipment = ((num3 < loadout.Equipment.Count) ? loadout.Equipment[num3] : null);
				int localIndex = num3;
				int v = ((loadoutEquipment != null) ? options.IndexOf(loadoutEquipment.Name) : 0);
				BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_StartingEquipmentName", num3, null, v, options, delegate(int index2)
				{
					LoadoutEquipment loadoutEquipment2 = ((loadoutEquipment != null) ? loadoutEquipment : new LoadoutEquipment());
					loadoutEquipment2.Name = options[index2];
					if (localIndex >= loadout.Equipment.Count)
					{
						loadout.Equipment.Add(loadoutEquipment2);
					}
					else
					{
						loadout.Equipment[localIndex] = loadoutEquipment2;
					}
					if (string.IsNullOrEmpty(loadoutEquipment2.Name) && localIndex >= 0 && localIndex < loadout.Equipment.Count)
					{
						loadout.Equipment.RemoveAt(localIndex);
					}
					HasChanged = true;
					WantRepopulate = true;
				});
				if (loadoutEquipment == null)
				{
					continue;
				}
				BaseMenu.AddInputField(unityContents, ref index, "EDITOR_Amount", num3, loadoutEquipment.Amount.ToString(), delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						loadoutEquipment.Amount = result;
						HasChanged = true;
						WantRepopulate = true;
					}
				});
			}
			int num4 = 0;
			for (int num5 = 0; num5 < loadout.SkillPoints.Length; num5++)
			{
				for (int num6 = 0; num6 < loadout.SkillPoints[num5]; num6++)
				{
					num4 += SkillsPage.PointsPerLevel[Math.Min(num6, SkillsPage.PointsPerLevel.Length - 1)];
				}
			}
			BaseMenu.AddTitleField(unityContents, ref index, GameImpl.Translate("EDITOR_LoadoutSkillsTitle").Replace("%1", num4.ToString()).Replace("%2", storySettings.SkillPoints.ToString()), -1, null);
			for (int num7 = 0; num7 < 10; num7++)
			{
				SkillType skillType = (SkillType)num7;
				BaseMenu.AddInputField(unityContents, ref index, "HUD_" + skillType, -1, (num7 < loadout.SkillPoints.Length) ? loadout.SkillPoints[num7].ToString() : string.Empty, delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						if ((int)skillType >= loadout.SkillPoints.Length)
						{
							List<int> list6 = new List<int>(loadout.SkillPoints);
							while (list6.Count < 10)
							{
								list6.Add(0);
							}
							loadout.SkillPoints = list6.ToArray();
						}
						loadout.SkillPoints[(int)skillType] = Math.Min(result, 5);
						HasChanged = true;
						WantRepopulate = true;
					}
				});
			}
			break;
		}
		case Tab.Difficulty:
		{
			if (storySettings.DifficultySettings.Count > 0)
			{
				CurrentDifficulty = MathUtil.Clamp(CurrentDifficulty, 0, storySettings.DifficultySettings.Count - 1);
				List<string> list = new List<string>();
				foreach (DifficultySettings difficultySetting2 in storySettings.DifficultySettings)
				{
					list.Add(difficultySetting2.DifficultyName);
				}
				BaseMenu.AddDropDown(unityContents, ref index, "EDITOR_SelectDifficulty", -1, null, CurrentDifficulty, list, delegate(int currentDifficulty)
				{
					CurrentDifficulty = currentDifficulty;
					WantDeleteAllChildren = true;
					WantRepopulate = true;
				});
				BaseMenu.AddButtonField(unityContents, ref index, "EDITOR_DeleteDifficulty", -1, delegate
				{
					storySettings.DifficultySettings.RemoveAt(CurrentDifficulty);
					CurrentDifficulty--;
					WantDeleteAllChildren = true;
					HasChanged = true;
					WantRepopulate = true;
				});
			}
			else
			{
				CurrentDifficulty = -1;
			}
			BaseMenu.AddButtonField(unityContents, ref index, "EDITOR_NewDifficulty", -1, delegate
			{
				CurrentDifficulty = storySettings.DifficultySettings.Count;
				storySettings.DifficultySettings.Add(new DifficultySettings());
				WantDeleteAllChildren = true;
				HasChanged = true;
				WantRepopulate = true;
			});
			if (CurrentDifficulty < 0 || CurrentDifficulty >= storySettings.DifficultySettings.Count)
			{
				break;
			}
			DifficultySettings difficultySettings = storySettings.DifficultySettings[CurrentDifficulty];
			FieldInfo[] fields = typeof(DifficultySettings).GetFields();
			foreach (FieldInfo field in fields)
			{
				FieldInfo localField = field;
				if (field.IsStatic || field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0)
				{
					continue;
				}
				if (field.FieldType == typeof(bool))
				{
					BaseMenu.AddCheckbox(unityContents, ref index, "EDITOR_" + field.Name, -1, (bool)field.GetValue(difficultySettings), delegate(bool flag)
					{
						localField.SetValue(difficultySettings, flag);
						HasChanged = true;
						WantRepopulate = true;
					});
				}
				else if (field.FieldType == typeof(int))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field.Name, -1, ((int)field.GetValue(difficultySettings)).ToString(), delegate(string s)
					{
						if (int.TryParse(s, out var result))
						{
							localField.SetValue(difficultySettings, result);
							HasChanged = true;
						}
					});
				}
				else if (field.FieldType == typeof(float))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field.Name, -1, ((float)field.GetValue(difficultySettings)).ToString(), delegate(string s)
					{
						if (float.TryParse(s, out var result))
						{
							localField.SetValue(difficultySettings, result);
							HasChanged = true;
						}
					});
				}
				else if (field.FieldType == typeof(string))
				{
					BaseMenu.AddInputField(unityContents, ref index, "EDITOR_" + field.Name, -1, (string)field.GetValue(difficultySettings), delegate(string value)
					{
						localField.SetValue(difficultySettings, value);
						HasChanged = true;
						WantRepopulate = field.Name == "DifficultyName";
					});
				}
			}
			break;
		}
		}
		for (int num14 = unityContents.transform.childCount - 1; num14 >= index; num14--)
		{
			UnityEngine.Object.Destroy(unityContents.transform.GetChild(num14).gameObject);
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

	private void GetStoryDropdownOptions(out List<StorySource> storySources, out List<string> options)
	{
		storySources = new List<StorySource>();
		storySources.Add(new StorySource());
		SelectStoryMenu.GetListOfLocalStoryFolders(storySources, skipBaseStory: false);
		SelectStoryMenu.GetListOfDownloadedStoryFolders(storySources, mods: true, stories: true, string.Empty);
		options = new List<string>();
		foreach (StorySource storySource in storySources)
		{
			options.Add(storySource.GetTranslatedName() + ((storySource.WorkshopId != 0L) ? (" (Workshop Id: " + storySource.WorkshopId + ")") : ((!string.IsNullOrEmpty(storySource.Folder)) ? " (Local)" : string.Empty)));
		}
	}

	public override void OnWorkshopItemQueryFinished()
	{
		base.OnWorkshopItemQueryFinished();
		WantRepopulate = true;
	}
}
