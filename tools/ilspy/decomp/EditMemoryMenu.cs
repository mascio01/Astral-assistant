using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class EditMemoryMenu : BaseMenu
{
	private MemoryPrototype Selected;

	private string SelectedUniqueID;

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
		if (GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes.Count > 0)
		{
			OnSelectMemory(GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes.Values[0]);
		}
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		if (InputFunctionManager.Instance.IsJustPressed(InputFunction.Back))
		{
			SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
			SaveMemory();
			WantPop = true;
		}
	}

	public override void UpdateImpl()
	{
		base.UpdateImpl();
		if (WantRepopulate)
		{
			PopulateMemoryList();
			PopulateMemoryInfo();
			WantRepopulate = false;
		}
	}

	public void OnCreateNew()
	{
		SoundManager.PlayMenuSound(SoundManager.SelectSound);
		GameImpl.Instance.ShowInputBox(OnAcceptCreateNew, GameImpl.Translate("EDITOR_EnterMemoryUniqueID"), "", multiline: false, readOnly: false);
	}

	private void CheckUniqueID(ref string v)
	{
		int i;
		for (i = 0; GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes.ContainsKey(v + ((i == 0) ? "" : i.ToString())); i++)
		{
		}
		v += ((i == 0) ? "" : i.ToString());
	}

	public void OnAcceptCreateNew(InputFrame inputFrame, string uniqueID)
	{
		if (!string.IsNullOrEmpty(uniqueID))
		{
			CheckUniqueID(ref uniqueID);
			MemoryPrototype memoryPrototype = new MemoryPrototype();
			memoryPrototype.UniqueID = uniqueID;
			GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes[uniqueID] = memoryPrototype;
			OnSelectMemory(memoryPrototype);
		}
	}

	public void OnBack()
	{
		SoundManager.PlayMenuSound(SoundManager.BackwardPageSound);
		SaveMemory();
		WantPop = true;
	}

	private void OnSelectMemory(MemoryPrototype proto)
	{
		SaveMemory();
		Selected = proto;
		SelectedUniqueID = proto?.UniqueID;
		PopulateMemoryList();
		PopulateMemoryInfo();
	}

	private void SaveMemory()
	{
		if (Selected != null && HasChanged)
		{
			Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
			if (!MemoryList.SaveToFile(currentlyEditingStory.Path + "/Memories.xml", currentlyEditingStory.MemoryPrototypes))
			{
				Selected.UniqueID = SelectedUniqueID;
			}
			if (Selected.UniqueID != SelectedUniqueID)
			{
				GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes.Remove(SelectedUniqueID);
				GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes[Selected.UniqueID] = Selected;
				SelectedUniqueID = Selected.UniqueID;
			}
			else
			{
				GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes[Selected.UniqueID] = Selected;
			}
			HasChanged = false;
			GameImpl.Instance.GetCurrentlyEditingStory().OnLoadFinished();
			GameImpl.Instance.BuildPrototypeLookupLists();
		}
	}

	private void PopulateMemoryList()
	{
		GameObject gameObject = base.gameObject.transform.Find("MemoryList/Viewport/Content").gameObject;
		int num = 0;
		foreach (KeyValuePair<string, MemoryPrototype> memoryPrototype in GameImpl.Instance.GetCurrentlyEditingStory().MemoryPrototypes)
		{
			Transform transform = gameObject.transform.Find(memoryPrototype.Key);
			GameObject gameObject2 = ((transform != null) ? transform.gameObject : null);
			if (gameObject2 == null)
			{
				MemoryPrototype proto = memoryPrototype.Value;
				gameObject2 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/ListButton"));
				gameObject2.name = memoryPrototype.Key;
				gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
				gameObject2.GetComponent<Button>().onClick.AddListener(delegate
				{
					SoundManager.PlayMenuSound(SoundManager.SelectSound);
					OnSelectMemory(proto);
				});
			}
			gameObject2.transform.Find("Text").gameObject.GetComponent<Text>().SetUnityText(memoryPrototype.Value.UniqueID);
			gameObject2.transform.SetSiblingIndex(num);
			num++;
		}
		for (; num < gameObject.transform.childCount; num++)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num).gameObject);
		}
	}

	private void PopulateMemoryInfo()
	{
		GameObject gameObject = base.gameObject.FindChild("MemoryInfo/Viewport/Content");
		int index = 0;
		if (Selected != null)
		{
			FieldInfo[] fields = typeof(MemoryPrototype).GetFields();
			foreach (FieldInfo field in fields)
			{
				FieldInfo localField = field;
				if (field.IsStatic || field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0 || field.GetCustomAttribute<NotVisible>() != null)
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
					BaseMenu.AddInputField(gameObject, ref index, "EDITOR_" + field.Name, -1, (string)field.GetValue(Selected), delegate(string v3)
					{
						if (field.Name == "UniqueID" && v3 != (string)field.GetValue(Selected))
						{
							if (string.IsNullOrEmpty(v3))
							{
								return;
							}
							CheckUniqueID(ref v3);
						}
						localField.SetValue(Selected, v3);
						HasChanged = true;
					});
				}
				else if (field.FieldType.IsEnum)
				{
					Type enumType = field.FieldType;
					FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
					List<string> options = new List<string>();
					FieldInfo[] array = fields2;
					foreach (FieldInfo fieldInfo in array)
					{
						options.Add(fieldInfo.GetValue(null).ToString());
					}
					int v = options.IndexOf(field.GetValue(Selected).ToString());
					BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, -1, null, v, options, delegate(int index2)
					{
						int num4 = (int)Enum.Parse(enumType, options[index2]);
						localField.SetValue(Selected, num4);
						HasChanged = true;
					});
				}
				else
				{
					if (!(field.FieldType == typeof(List<string>)))
					{
						continue;
					}
					List<string> options2 = new List<string>();
					options2.Add("");
					foreach (KeyValuePair<string, MemoryPrototype> item in GameImpl.Instance.CurrentMemoryPrototypesDeterministic)
					{
						options2.Add(item.Key);
					}
					List<string> list = (List<string>)field.GetValue(Selected);
					for (int num2 = 0; num2 <= (list?.Count ?? 0); num2++)
					{
						int localIndex = num2;
						int v2 = ((list != null && num2 < list.Count) ? options2.IndexOf(list[num2].ToString()) : 0);
						BaseMenu.AddDropDown(gameObject, ref index, "EDITOR_" + field.Name, num2, null, v2, options2, delegate(int index2)
						{
							string text = options2[index2];
							if (list == null)
							{
								list = new List<string>();
								field.SetValue(Selected, list);
							}
							if (localIndex >= list.Count)
							{
								list.Add(text);
							}
							else
							{
								list[localIndex] = text;
							}
							if (string.IsNullOrEmpty(text) && localIndex >= 0 && localIndex < list.Count)
							{
								list.RemoveAt(localIndex);
							}
							HasChanged = true;
							WantRepopulate = true;
						});
					}
				}
			}
		}
		for (int num3 = gameObject.transform.childCount - 1; num3 >= index; num3--)
		{
			UnityEngine.Object.Destroy(gameObject.transform.GetChild(num3).gameObject);
		}
	}
}
