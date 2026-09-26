using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class EditLiquidMenu : BaseMenu
{
	private LiquidPrototype Selected;

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
		if (GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes.Count > 0)
		{
			OnSelectLiquid(GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes.Values[0]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			SaveLiquid();
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			PopulateLiquidList();
			PopulateLiquidInfo();
			WantRepopulate = false;
		}
	}

	public void OnCreateNew()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterLiquidName"), "", multiline: false, readOnly: false);
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string name)
	{
		if (!string.IsNullOrEmpty(name))
		{
			CheckUniqueID(ref name);
			string text = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Liquid";
			LiquidPrototype liquidPrototype = new LiquidPrototype();
			liquidPrototype.Name = name;
			liquidPrototype.Tex = Resource<Texture2D>.CreateIfFileExists(text + "/" + liquidPrototype.IconPath);
			GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes[name] = liquidPrototype;
			OnSelectLiquid(liquidPrototype);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveLiquid();
		WantPop = true;
	}

	private void OnSelectLiquid(LiquidPrototype proto)
	{
		SaveLiquid();
		Selected = proto;
		SelectedName = proto?.Name;
		PopulateLiquidList();
		PopulateLiquidInfo();
	}

	private void SaveLiquid()
	{
		if (Selected == null || !HasChanged)
		{
			return;
		}
		string text = GameImpl.Instance.GetCurrentlyEditingStory().Path + "/Liquid";
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
			GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes.Remove(SelectedName);
			GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes[Selected.Name] = Selected;
			GameImpl.Instance.BuildPrototypeLookupLists();
			SelectedName = Selected.Name;
		}
		else
		{
			GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes[Selected.Name] = Selected;
		}
		Selected.Tex = Resource<Texture2D>.CreateIfFileExists(text + "/" + Selected.IconPath);
		HasChanged = false;
		GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
		GameImpl.Instance.BuildPrototypeLookupLists();
	}

	private void PopulateLiquidList()
	{
		GameObject gameObject = base.gameObject.transform.Find("LiquidList/Viewport/Content").gameObject;
		int num = 0;
		foreach (KeyValuePair<string, LiquidPrototype> liquidPrototype in GameImpl.Instance.GetCurrentlyEditingStory().LiquidPrototypes)
		{
			Transform transform = gameObject.transform.Find(liquidPrototype.Key);
			GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
			if (gameObject2 == null)
			{
				LiquidPrototype proto = liquidPrototype.Value;
				gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/IconButton"));
				gameObject2.name = liquidPrototype.Key;
				gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					OnSelectLiquid(proto);
				});
			}
			gameObject2.transform.Find("Text").gameObject.GetComponent<Text>().SetUnityText(liquidPrototype.Value.Name);
			GameObject obj = gameObject2.transform.Find("Image").gameObject;
			obj.GetComponent<RawImage>().texture = ((liquidPrototype.Value.Tex != null && (bool)liquidPrototype.Value.Tex.GetAsset()) ? liquidPrototype.Value.Tex.GetAsset() : null);
			obj.GetComponent<RawImage>().color = liquidPrototype.Value.Col;
			gameObject2.transform.SetSiblingIndex(num);
			num++;
		}
		for (; num < gameObject.transform.childCount; num++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulateLiquidInfo()
	{
		GameObject gameObject = base.gameObject.FindChild("LiquidInfo/Viewport/Content");
		int index = 0;
		if (Selected != null)
		{
			FieldInfo[] fields = typeof(LiquidPrototype).GetFields();
			foreach (FieldInfo field in fields)
			{
				FieldInfo localField = field;
				if (field.IsStatic || (field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0 && field.Name != "Name"))
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
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, (string)field.GetValue(Selected), delegate(string v5)
					{
						if (field.Name == "Name" && v5 != (string)field.GetValue(Selected))
						{
							if (string.IsNullOrEmpty(v5))
							{
								return;
							}
							CheckUniqueID(ref v5);
						}
						localField.SetValue(Selected, v5);
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
				else if (field.FieldType == typeof(List<GenderInLanguage>))
				{
					List<string> list = new List<string>(StringUtil.GetEnumNames<Language>());
					List<string> list2 = new List<string>(StringUtil.GetEnumNames<GenderType>());
					list2.Add("Neuter");
					List<GenderInLanguage> list3 = (List<GenderInLanguage>)field.GetValue(Selected);
					for (int num = 0; num <= (list3?.Count ?? 0); num++)
					{
						int localIndex = num;
						if (list3 != null && num < list3.Count)
						{
							int v = Math.Max(0, list.IndexOf(list3[num].Language.ToString()));
							int v2 = Math.Max(0, list2.IndexOf(list3[num].Gender.ToString()));
							BaseMenu.AddDoubleDropDown(gameObject, ref index, "EDITOR_" + field.Name, num, (num > 0) ? "EDITOR_AND" : null, v, list, delegate(int num5)
							{
								Language language = (Language)(num5 - 1);
								if (list3 == null)
								{
									list3 = new List<GenderInLanguage>();
									field.SetValue(Selected, list3);
								}
								if (localIndex >= list3.Count)
								{
									list3.Add(new GenderInLanguage());
								}
								list3[localIndex].Language = language;
								if (language == Language.Invalid && localIndex >= 0 && localIndex < list3.Count)
								{
									list3.RemoveAt(localIndex);
								}
								HasChanged = true;
								WantRepopulate = true;
							}, v2, list2, delegate(int gender)
							{
								list3[localIndex].Gender = (GenderType)gender;
								HasChanged = true;
							});
							continue;
						}
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name + "_New", num, (num > 0) ? "EDITOR_AND" : null, 0, list, delegate(int num5)
						{
							Language language = (Language)(num5 - 1);
							if (list3 == null)
							{
								list3 = new List<GenderInLanguage>();
								field.SetValue(Selected, list3);
							}
							if (localIndex >= list3.Count)
							{
								list3.Add(new GenderInLanguage());
							}
							list3[localIndex].Language = language;
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
				else if (field.FieldType == typeof(List<string>))
				{
					string text = ((field.Name == "GiftFor") ? "EDITOR_AND" : "EDITOR_OR");
					List<string> options = GameImpl.Instance.GetAllPersonalities();
					List<string> list4 = (List<string>)field.GetValue(Selected);
					for (int num2 = 0; num2 <= (list4?.Count ?? 0); num2++)
					{
						int localIndex2 = num2;
						int v3 = ((list4 != null && num2 < list4.Count) ? options.IndexOf(list4[num2].ToString()) : 0);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num2, (num2 > 0) ? text : null, v3, options, delegate(int index2)
						{
							string text3 = options[index2];
							if (list4 == null)
							{
								list4 = new List<string>();
								field.SetValue(Selected, list4);
							}
							if (localIndex2 >= list4.Count)
							{
								list4.Add(text3);
							}
							else
							{
								list4[localIndex2] = text3;
							}
							if (string.IsNullOrEmpty(text3) && localIndex2 >= 0 && localIndex2 < list4.Count)
							{
								list4.RemoveAt(localIndex2);
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
					List<string> options2 = new List<string>();
					if (field.Name == "Gender")
					{
						options2.Add("Male");
						options2.Add("Female");
						options2.Add("Neuter");
					}
					else
					{
						FieldInfo[] array = fields2;
						foreach (FieldInfo fieldInfo in array)
						{
							options2.Add(fieldInfo.GetValue(null).ToString());
						}
					}
					string text2 = field.GetValue(Selected).ToString();
					if (text2 == "Count" && field.Name == "Gender")
					{
						text2 = "Neuter";
					}
					int v4 = options2.IndexOf(text2);
					BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, -1, null, v4, options2, delegate(int index2)
					{
						int num5 = (int)Enum.Parse(enumType, options2[index2]);
						localField.SetValue(Selected, num5);
						HasChanged = true;
					});
				}
			}
		}
		for (int num4 = gameObject.transform.childCount - 1; num4 >= index; num4--)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num4).gameObject);
		}
	}
}
