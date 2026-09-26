using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScriptObjectBehaviour : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
	public delegate void PopulateListItem(int i, GameObject listItemObj);

	public delegate void CopyListItem(XmlWriter writer, List<ScriptObjectItemBehaviour> items);

	public delegate bool PasteListItem(XmlReader reader);

	public delegate void DeleteListItem();

	public static SpecifierOption[] DefaultSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Actor", Specifier.Actor),
		new SpecifierOption("Target", Specifier.Target),
		new SpecifierOption("ReferringTo", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("ExtraParam", Specifier.ExtraParam)
	};

	public static SpecifierOption[] SpeechSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Speaker", Specifier.Actor),
		new SpecifierOption("Listener", Specifier.Target),
		new SpecifierOption("ReferringTo", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("ExtraParam", Specifier.ExtraParam)
	};

	public static SpecifierOption[] MemorySpeechSpecifierOptions = new SpecifierOption[9]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Speaker", Specifier.Actor),
		new SpecifierOption("Listener", Specifier.Target),
		new SpecifierOption("ReferringTo", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("MemoryActor", Specifier.MemoryActor),
		new SpecifierOption("MemoryObject", Specifier.MemoryObject),
		new SpecifierOption("MemoryThirdParty", Specifier.MemoryThirdParty)
	};

	public static SpecifierOption[] QuestSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("QuestGiver", Specifier.Actor),
		new SpecifierOption("QuestSeeker", Specifier.Target),
		new SpecifierOption("QuestObject", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("QuestParam", Specifier.ExtraParam)
	};

	public static SpecifierOption[] QuestMarkerSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("QuestGiver", Specifier.Actor),
		new SpecifierOption("QuestSeeker", Specifier.Target),
		new SpecifierOption("QuestObject", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("Marker", Specifier.ExtraParam)
	};

	public static SpecifierOption[] TriggerSpecifierOptions = new SpecifierOption[10]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Triggerer", Specifier.Actor),
		new SpecifierOption("Triggeree", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("QuestGiver", Specifier.QuestGiver),
		new SpecifierOption("QuestSeeker", Specifier.QuestSeeker),
		new SpecifierOption("QuestObject", Specifier.QuestObject),
		new SpecifierOption("QuestParam", Specifier.QuestParam),
		new SpecifierOption("Quest", Specifier.ExtraParam)
	};

	public static SpecifierOption[] EnabledTriggerSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Subject", Specifier.Actor),
		new SpecifierOption("Triggerer", Specifier.Target),
		new SpecifierOption("Object", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("Triggeree", Specifier.ExtraParam)
	};

	public static SpecifierOption[] PriceSpecifierOptions = new SpecifierOption[7]
	{
		new SpecifierOption("Invalid", Specifier.Invalid),
		new SpecifierOption("Seller", Specifier.Actor),
		new SpecifierOption("Buyer", Specifier.Target),
		new SpecifierOption("ReferringTo", Specifier.ReferringTo),
		new SpecifierOption("ID", Specifier.ID),
		new SpecifierOption("Player", Specifier.Player),
		new SpecifierOption("ExtraParam", Specifier.ExtraParam)
	};

	public static ScriptObjectBehaviour Expanded;

	public BaseScriptObject ScriptObject;

	private Vector2 DragGrabPoint;

	private bool WantUpdateFields;

	private bool WantSetConnectionsDirty;

	public bool Dragging;

	public int TargetedBySpeech;

	public int TargetedByQuest;

	public static Color32 ConditionBlockCol = new Color32(156, 221, 156, byte.MaxValue);

	public static Color32 TriggerCol = new Color32(250, 145, 114, byte.MaxValue);

	public static Color32 TemplateCol = new Color32(173, 114, 250, byte.MaxValue);

	public static Color32 InvaderCol = new Color32(250, 114, 163, byte.MaxValue);

	public static Color32 QuestCol = new Color32(54, 246, byte.MaxValue, byte.MaxValue);

	public static Color ItemCol = Color.white;

	public static Color ItemAltCol = new Color(0.75f, 0.75f, 0.75f, 1f);

	public static Color SelectedItemCol = new Color(1f, 0.75f, 0.75f, 1f);

	public static Color SelectedItemAltCol = new Color(1f, 0.5f, 0.5f, 1f);

	public void OnBeginDrag(PointerEventData eventData)
	{
		ScriptEditor.Instance.CloseContextMenu();
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)base.transform, InputFunctionManager.Instance.GetMousePosition(), null, out DragGrabPoint);
		Dragging = true;
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)ScriptEditor.Instance.UnityContentPane.transform, InputFunctionManager.Instance.GetMousePosition(), null, out var localPoint);
		Vector2 editorPos = ScriptObject.EditorPos;
		Vector2 vector = localPoint - DragGrabPoint;
		base.transform.localPosition = vector;
		ScriptObject.EditorPos = MathUtil.ToXY(vector);
		foreach (ScriptObjectBehaviour selectedObject in ScriptEditor.Instance.SelectedObjects)
		{
			if (!(selectedObject == this))
			{
				selectedObject.transform.localPosition += MathUtil.ToXY0(vector - editorPos);
				selectedObject.ScriptObject.EditorPos = MathUtil.ToXY(selectedObject.transform.localPosition);
			}
		}
		ScriptEditor.Instance.ConnectionsGraphic.SetVerticesDirty();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		Dragging = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		switch (eventData.button)
		{
		case PointerEventData.InputButton.Left:
		{
			bool flag = InputFunctionManager.Instance.IsKeyPressed(KeyCode.LeftControl) || InputFunctionManager.Instance.IsKeyPressed(KeyCode.RightControl);
			if (!Dragging && !flag)
			{
				ScriptEditor.Instance.DeselectAllItems();
				ScriptEditor.Instance.DeselectAll();
			}
			if (flag && ScriptEditor.Instance.SelectedObjects.Contains(this))
			{
				ScriptEditor.Instance.Deselect(this);
			}
			else
			{
				ScriptEditor.Instance.Select(this);
			}
			ScriptEditor.Instance.CloseContextMenu();
			break;
		}
		case PointerEventData.InputButton.Right:
			ScriptEditor.Instance.OpenContextMenu(null);
			break;
		}
	}

	public void Update()
	{
		if (WantUpdateFields)
		{
			UpdateFields();
		}
		if (WantSetConnectionsDirty)
		{
			ScriptEditor.Instance.ConnectionsGraphic.SetVerticesDirty();
		}
	}

	public void UpdateFields()
	{
		List<GameObject> list = new List<GameObject>();
		int index = 0;
		Color col = new Color32(159, 156, byte.MaxValue, byte.MaxValue);
		if (ScriptObject is Speech)
		{
			col = new Color32(byte.MaxValue, 215, 0, byte.MaxValue);
		}
		else if (ScriptObject is ConditionBlock)
		{
			col = ConditionBlockCol;
		}
		else if (ScriptObject is Trigger)
		{
			col = TriggerCol;
		}
		else if (ScriptObject is Quest)
		{
			col = QuestCol;
		}
		else if (ScriptObject is QuestGroup)
		{
			col = new Color32(49, 53, byte.MaxValue, byte.MaxValue);
		}
		else if (ScriptObject is Invader)
		{
			col = InvaderCol;
		}
		else if (ScriptObject is Template)
		{
			col = TemplateCol;
		}
		list.Add(AddHeaderField(base.gameObject, ref index, "EDITOR_" + ScriptObject.GetType().Name, ScriptObject.UniqueID, col));
		FieldInfo[] fields = ScriptObject.GetType().GetFields();
		foreach (FieldInfo field in fields)
		{
			FieldInfo localField = field;
			if (field.IsStatic || field.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: true).Length != 0)
			{
				continue;
			}
			if (Expanded != this && field.GetCustomAttribute<AlwaysVisible>() == null)
			{
				DefaultValueAttribute customAttribute = field.GetCustomAttribute<DefaultValueAttribute>();
				if (customAttribute != null)
				{
					object value = field.GetValue(ScriptObject);
					if (value != null && value.Equals(customAttribute.Value))
					{
						continue;
					}
				}
			}
			if ((field.GetCustomAttribute<TranslatedTextField>() != null && GameImpl.Instance.Settings.Language == Language.English) || field.Name == "x" || field.Name == "y" || field.Name == "UniqueID" || field.Name == "RunAtStart")
			{
				continue;
			}
			OnlyVisibleForTemplateType customAttribute2 = field.GetCustomAttribute<OnlyVisibleForTemplateType>();
			if (customAttribute2 != null && !customAttribute2.Matches(ScriptObject))
			{
				continue;
			}
			NotVisibleForZombie customAttribute3 = field.GetCustomAttribute<NotVisibleForZombie>();
			if (customAttribute3 != null && !customAttribute3.Matches(ScriptObject))
			{
				continue;
			}
			OnlyVisibleForZombie customAttribute4 = field.GetCustomAttribute<OnlyVisibleForZombie>();
			if (customAttribute4 != null && !customAttribute4.Matches(ScriptObject))
			{
				continue;
			}
			NotVisibleForTemplateType customAttribute5 = field.GetCustomAttribute<NotVisibleForTemplateType>();
			if (customAttribute5 != null && !customAttribute5.Matches(ScriptObject))
			{
				continue;
			}
			OnlyVisibleForSpawnLocation customAttribute6 = field.GetCustomAttribute<OnlyVisibleForSpawnLocation>();
			if (customAttribute6 != null && !customAttribute6.Matches(ScriptObject))
			{
				continue;
			}
			NotVisibleForSpawnLocation customAttribute7 = field.GetCustomAttribute<NotVisibleForSpawnLocation>();
			if (customAttribute7 != null && !customAttribute7.Matches(ScriptObject))
			{
				continue;
			}
			OnlyVisibleForRole customAttribute8 = field.GetCustomAttribute<OnlyVisibleForRole>();
			if (customAttribute8 != null && !customAttribute8.Matches(ScriptObject))
			{
				continue;
			}
			OnlyVisibleForCommunityType customAttribute9 = field.GetCustomAttribute<OnlyVisibleForCommunityType>();
			if (customAttribute9 != null && !customAttribute9.Matches(ScriptObject))
			{
				continue;
			}
			if (field.GetCustomAttribute<OnlyVisibleForClothing>() != null)
			{
				if (!(ScriptObject is Template { Type: TemplateType.Equipment } template))
				{
					continue;
				}
				EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(template.EquipmentPrototypeName);
				if (equipmentPrototype == null || equipmentPrototype.ClothingType == ClothingType.Invalid)
				{
					continue;
				}
			}
			OnlyVisibleForEquipmentWithVariation customAttribute10 = field.GetCustomAttribute<OnlyVisibleForEquipmentWithVariation>();
			if (customAttribute10 != null && !customAttribute10.Matches(ScriptObject))
			{
				continue;
			}
			Quest quest = null;
			Speech speech = null;
			string text = "EDITOR_" + field.Name;
			if (field.Name == "PerRelationship")
			{
				if (ScriptObject is Speech { PerObjectType: not SpeechPerObjectType.PerRelationship })
				{
					continue;
				}
			}
			else if (field.Name == "IncludeListener")
			{
				if (ScriptObject is Speech { PerObjectType: not SpeechPerObjectType.PerRelationship, PerObjectType: not SpeechPerObjectType.PerRomanticRelationship, PerObjectType: not SpeechPerObjectType.PerCommunityMember, PerObjectType: not SpeechPerObjectType.PerCommunityMemberSample, PerObjectType: not SpeechPerObjectType.PerListenerCommunityMember, PerObjectType: not SpeechPerObjectType.PerListenerCommunityMemberSample, PerObjectType: not SpeechPerObjectType.PerEnemyCommunityMemberSample, PerObjectType: not SpeechPerObjectType.PerLooterCommunityMemberSample, PerObjectType: not SpeechPerObjectType.PerReferringToCommunityMember })
				{
					continue;
				}
			}
			else if (field.Name == "PerStringData")
			{
				if (ScriptObject is Speech { PerObjectType: not SpeechPerObjectType.PerUniqueID, PerObjectType: not SpeechPerObjectType.PerActiveQuest, PerObjectType: not SpeechPerObjectType.PerActiveQuestGiver, PerObjectType: not SpeechPerObjectType.PerActiveQuestSeeker, PerObjectType: not SpeechPerObjectType.PerActiveQuestObject })
				{
					continue;
				}
			}
			else if (field.Name == "PerMemory")
			{
				if (ScriptObject is Speech { PerObjectType: not SpeechPerObjectType.PerMemory })
				{
					continue;
				}
			}
			else if (field.Name == "MetricCompletedAmount")
			{
				quest = ScriptObject as Quest;
				if (quest != null && quest.Metric == QuestMetric.None)
				{
					continue;
				}
			}
			else if (field.Name == "MetricLimit")
			{
				quest = ScriptObject as Quest;
				if (quest != null)
				{
					switch (quest.Metric)
					{
					case QuestMetric.Formula:
						text = "Target Amount";
						break;
					case QuestMetric.TimeSinceStarted:
						text = "Time (s)";
						break;
					case QuestMetric.RelationshipProgress:
						text = "Quantity";
						break;
					case QuestMetric.Drunkenness:
						text = "Blood Alcohol";
						break;
					default:
						continue;
					}
				}
			}
			else if (field.Name == "MetricLimit2")
			{
				quest = ScriptObject as Quest;
				if (quest != null && quest.Metric != QuestMetric.RelationshipProgress)
				{
					continue;
				}
				text = "Quantity2";
			}
			else if (field.Name == "MetricStringData")
			{
				quest = ScriptObject as Quest;
				if (quest != null && quest.Metric != QuestMetric.RelationshipProgress && quest.Metric != QuestMetric.InvaderTimeout)
				{
					continue;
				}
				text = ((quest.Metric == QuestMetric.InvaderTimeout) ? "Invader" : "Memory");
			}
			else if (field.Name == "MetricStringData2")
			{
				quest = ScriptObject as Quest;
				if (quest != null && quest.Metric != QuestMetric.RelationshipProgress)
				{
					continue;
				}
				text = "Memory2";
			}
			else if (field.Name == "MetricFormula")
			{
				quest = ScriptObject as Quest;
				if (quest != null && quest.Metric != QuestMetric.Formula)
				{
					continue;
				}
				text = "Formula";
			}
			BaseScriptObject baseScriptObject = ScriptObject;
			Script.ReferenceType referenceType = Script.ReferenceType.Other;
			List<BaseScriptObject> list2 = new List<BaseScriptObject>();
			while (!(baseScriptObject is Quest) && !(baseScriptObject is QuestGroup) && !(baseScriptObject is Speech) && referenceType == Script.ReferenceType.Other)
			{
				list2.Add(baseScriptObject);
				BaseScriptObject baseScriptObject2 = ScriptEditor.Instance.EditingScript.FindFirstReferenceTo(baseScriptObject, out referenceType);
				if (baseScriptObject2 == null || list2.Contains(baseScriptObject2))
				{
					break;
				}
				baseScriptObject = baseScriptObject2;
			}
			SpecifierOption[] specifierOptions = DefaultSpecifierOptions;
			switch (referenceType)
			{
			case Script.ReferenceType.EnabledTriggers:
				specifierOptions = EnabledTriggerSpecifierOptions;
				break;
			case Script.ReferenceType.QuestTriggers:
				specifierOptions = TriggerSpecifierOptions;
				break;
			case Script.ReferenceType.Price:
				specifierOptions = PriceSpecifierOptions;
				break;
			default:
				if (baseScriptObject is Speech)
				{
					SpeechSituation situation = ((Speech)baseScriptObject).Situation;
					SpeechPerObjectType perObjectType = ((Speech)baseScriptObject).PerObjectType;
					specifierOptions = ((situation == SpeechSituation.Memory || situation == SpeechSituation.MemoryOfYou || situation == SpeechSituation.MemoryDisbelief || situation == SpeechSituation.MemoryResponse || perObjectType == SpeechPerObjectType.PerMemory) ? MemorySpeechSpecifierOptions : SpeechSpecifierOptions);
				}
				else if (baseScriptObject is Quest || baseScriptObject is QuestGroup)
				{
					if (field.Name == "QuestMarkerConditions")
					{
						specifierOptions = QuestMarkerSpecifierOptions;
					}
					else
					{
						specifierOptions = QuestSpecifierOptions;
					}
				}
				break;
			}
			string[] modifierNames = Condition.ModifierNames;
			modifierNames[0] = string.Empty;
			if (field.FieldType == typeof(bool))
			{
				bool v = (bool)field.GetValue(ScriptObject);
				list.Add(BaseMenu.AddCheckbox(base.gameObject, ref index, text, -1, v, delegate(bool flag)
				{
					localField.SetValue(ScriptObject, flag);
					OnFieldChanged();
				}));
			}
			else if (field.FieldType == typeof(int))
			{
				int num = (int)field.GetValue(ScriptObject);
				list.Add(BaseMenu.AddInputField(base.gameObject, ref index, text, -1, num.ToString(), delegate(string s)
				{
					if (int.TryParse(s, out var result))
					{
						localField.SetValue(ScriptObject, result);
						OnFieldChanged();
					}
				}));
			}
			else if (field.FieldType == typeof(float))
			{
				float num2 = (float)field.GetValue(ScriptObject);
				list.Add(BaseMenu.AddInputField(base.gameObject, ref index, text, -1, num2.ToString(), delegate(string s)
				{
					if (float.TryParse(s, out var result))
					{
						localField.SetValue(ScriptObject, result);
						OnFieldChanged();
					}
				}));
			}
			else if (field.FieldType == typeof(string))
			{
				if (field.Name == "PerMemory" || (field.Name == "MetricStringData" && quest != null && quest.Metric == QuestMetric.RelationshipProgress) || (field.Name == "MetricStringData2" && quest != null && quest.Metric == QuestMetric.RelationshipProgress))
				{
					string text2 = field.GetValue(ScriptObject) as string;
					if (Expanded != this && string.IsNullOrEmpty(text2))
					{
						continue;
					}
					List<string> options = new List<string>();
					options.Add("");
					foreach (KeyValuePair<string, MemoryPrototype> item in GameImpl.Instance.CurrentMemoryPrototypesDeterministic)
					{
						options.Add(item.Key);
					}
					int v2 = ((text2 != null) ? options.IndexOf(text2) : (-1));
					list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v2, options, delegate(int index2)
					{
						localField.SetValue(ScriptObject, options[index2]);
						OnFieldChanged();
					}));
				}
				else if (field.Name == "EquipmentPrototypeName")
				{
					string text3 = field.GetValue(ScriptObject) as string;
					if (!(Expanded != this) || !string.IsNullOrEmpty(text3))
					{
						List<string> options2 = BaseMenu.GetEquipmentOptions();
						int v3 = ((text3 != null) ? options2.IndexOf(text3) : (-1));
						list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v3, options2, delegate(int index2)
						{
							localField.SetValue(ScriptObject, options2[index2]);
							OnFieldChanged();
						}));
					}
				}
				else if (field.Name == "PropPrototypeName")
				{
					string text4 = field.GetValue(ScriptObject) as string;
					if (!(Expanded != this) || !string.IsNullOrEmpty(text4))
					{
						List<string> options3 = BaseMenu.GetPropOptions();
						int v4 = ((text4 != null) ? options3.IndexOf(text4) : (-1));
						list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v4, options3, delegate(int index2)
						{
							localField.SetValue(ScriptObject, options3[index2]);
							OnFieldChanged();
						}));
					}
				}
				else if (field.Name == "LootLocation")
				{
					string text5 = field.GetValue(ScriptObject) as string;
					if (!(Expanded != this) || !string.IsNullOrEmpty(text5))
					{
						List<string> options4 = BaseMenu.GetLootLocationOptions();
						int v5 = ((text5 != null) ? options4.IndexOf(text5) : (-1));
						list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v5, options4, delegate(int index2)
						{
							localField.SetValue(ScriptObject, options4[index2]);
							OnFieldChanged();
						}));
					}
				}
				else if (field.Name == "PersonalityFaction")
				{
					string text6 = field.GetValue(ScriptObject) as string;
					if (!(Expanded != this) || !string.IsNullOrEmpty(text6))
					{
						List<string> options5 = BaseMenu.GetPersonalityFactionOptions();
						int v6 = ((text6 != null) ? options5.IndexOf(text6) : (-1));
						list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v6, options5, delegate(int index2)
						{
							localField.SetValue(ScriptObject, options5[index2]);
							OnFieldChanged();
						}));
					}
				}
				else
				{
					string v7 = (string)field.GetValue(ScriptObject);
					list.Add(BaseMenu.AddInputField(base.gameObject, ref index, text, -1, v7, delegate(string text8)
					{
						localField.SetValue(ScriptObject, text8.Trim());
						OnFieldChanged();
					}));
				}
			}
			else if (field.FieldType.IsEnum)
			{
				Type enumType = field.FieldType;
				FieldInfo[] fields2 = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
				List<string> options6 = new List<string>();
				FieldInfo[] array = fields2;
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					string text7 = array[num3].GetValue(null).ToString();
					if (!(text7 == "Count"))
					{
						options6.Add(text7);
					}
				}
				int v8 = options6.IndexOf(field.GetValue(ScriptObject).ToString());
				list.Add(BaseMenu.AddDropDown(base.gameObject, ref index, text, -1, null, v8, options6, delegate(int index2)
				{
					int num4 = (int)Enum.Parse(enumType, options6[index2]);
					localField.SetValue(ScriptObject, num4);
					OnFieldChanged();
				}));
			}
			else if (field.FieldType == typeof(List<QuestMarker>))
			{
				List<QuestMarker> list3 = localField.GetValue(ScriptObject) as List<QuestMarker>;
				if (Expanded != this && (list3 == null || list3.Count == 0))
				{
					continue;
				}
				list.Add(AddList<QuestMarker>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemQuestTarget", "Subject/Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateSpecifierDropdown(listItemObj, "Subject/Panel/Dropdown", specifierOptions, list3[index2].Specifier, delegate(Specifier specifier)
					{
						QuestMarker value2 = list3[index2];
						value2.Specifier = specifier;
						list3[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Subject/Panel/Dropdown2", modifierNames, sorted: false, (int)list3[index2].Modifier, delegate(int modifier)
					{
						QuestMarker value2 = list3[index2];
						value2.Modifier = (SpecifierModifier)modifier;
						list3[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Subject/Panel/InputField", list3[index2].UniqueID, delegate(string uniqueID)
					{
						QuestMarker value2 = list3[index2];
						value2.UniqueID = uniqueID;
						list3[index2] = value2;
						OnFieldChanged();
					});
					listItemObj.FindChild("Subject/Panel/Dropdown2").SetActive(list3[index2].Specifier != Specifier.ID);
					listItemObj.FindChild("Subject/Panel/InputField").SetActive(list3[index2].Specifier == Specifier.ID);
				}));
			}
			else if (field.FieldType == typeof(List<TemplateChild>))
			{
				List<TemplateChild> list4 = localField.GetValue(ScriptObject) as List<TemplateChild>;
				if (Expanded != this && (list4 == null || list4.Count == 0))
				{
					continue;
				}
				list.Add(AddList<TemplateChild>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemTemplateChild", "Subject/Buttons", delegate(int index2, GameObject listItemObj)
				{
					List<string> options7 = BaseMenu.GetTemplateOptions();
					PopulateDropdown(listItemObj, "Subject/Panel/Dropdown", options7.ToArray(), sorted: false, Math.Max(0, options7.IndexOf(list4[index2].UniqueID)), delegate(int index3)
					{
						TemplateChild value2 = list4[index2];
						value2.UniqueID = options7[index3];
						list4[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data/InputField1", list4[index2].Min.ToString(), delegate(string str)
					{
						TemplateChild value2 = list4[index2];
						value2.Min = StringUtil.ParseInt(str);
						list4[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data/InputField2", list4[index2].Max.ToString(), delegate(string str)
					{
						TemplateChild value2 = list4[index2];
						value2.Max = StringUtil.ParseInt(str);
						list4[index2] = value2;
						OnFieldChanged();
					});
					listItemObj.FindChild("MinFormula").SetActive(Expanded == this || !string.IsNullOrEmpty(list4[index2].MinFormula.UniqueID));
					listItemObj.FindChild("MaxFormula").SetActive(Expanded == this || !string.IsNullOrEmpty(list4[index2].MaxFormula.UniqueID));
					PopulateInputField(listItemObj, "MinFormula/InputField", list4[index2].MinFormula.UniqueID, delegate(string uniqueID)
					{
						TemplateChild value2 = list4[index2];
						value2.MinFormula = ConditionBlockRef.Create(uniqueID);
						list4[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "MaxFormula/InputField", list4[index2].MaxFormula.UniqueID, delegate(string uniqueID)
					{
						TemplateChild value2 = list4[index2];
						value2.MaxFormula = ConditionBlockRef.Create(uniqueID);
						list4[index2] = value2;
						OnFieldChanged();
					});
				}));
			}
			else if (field.FieldType == typeof(List<SpeechParam>))
			{
				List<SpeechParam> list5 = localField.GetValue(ScriptObject) as List<SpeechParam>;
				if (Expanded != this && (list5 == null || list5.Count == 0))
				{
					continue;
				}
				list.Add(AddList<SpeechParam>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemSpeechParam", "Type/Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateDropdown(listItemObj, "Type/Dropdown", SpeechParam.TypeNames, sorted: true, (int)list5[index2].Type, delegate(int type)
					{
						SpeechParam value2 = list5[index2];
						value2.Type = (SpeechParamType)type;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Subject/Dropdown", specifierOptions, list5[index2].Subject, delegate(Specifier subject)
					{
						SpeechParam value2 = list5[index2];
						value2.Subject = subject;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Object/Dropdown", specifierOptions, list5[index2].Object, delegate(Specifier specifier)
					{
						SpeechParam value2 = list5[index2];
						value2.Object = specifier;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Subject/Dropdown2", modifierNames, sorted: false, (int)list5[index2].SubjectModifier, delegate(int subjectModifier)
					{
						SpeechParam value2 = list5[index2];
						value2.SubjectModifier = (SpecifierModifier)subjectModifier;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Object/Dropdown2", modifierNames, sorted: false, (int)list5[index2].ObjectModifier, delegate(int objectModifier)
					{
						SpeechParam value2 = list5[index2];
						value2.ObjectModifier = (SpecifierModifier)objectModifier;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Subject/InputField", list5[index2].SubjectID, delegate(string subjectID)
					{
						SpeechParam value2 = list5[index2];
						value2.SubjectID = subjectID;
						list5[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Object/InputField", list5[index2].ObjectID, delegate(string objectID)
					{
						SpeechParam value2 = list5[index2];
						value2.ObjectID = objectID;
						list5[index2] = value2;
						OnFieldChanged();
					});
					string title = null;
					bool hasFormula;
					bool flag = SpeechParam.HasData(list5[index2].Type, out title, out hasFormula);
					listItemObj.FindChild("Data").SetActive(flag);
					if (flag)
					{
						PopulateText(listItemObj, "Data/Title", title);
						PopulateInputField(listItemObj, "Data/InputField", list5[index2].Data.ToString(), delegate(string s)
						{
							SpeechParam value2 = list5[index2];
							float.TryParse(s, out value2.Data);
							list5[index2] = value2;
							OnFieldChanged();
						});
					}
					listItemObj.FindChild("Formula").SetActive(hasFormula);
					if (hasFormula)
					{
						PopulateInputField(listItemObj, "Formula/InputField", (list5[index2].Formulas != null && list5[index2].Formulas.Count > 0) ? list5[index2].Formulas[0].UniqueID : string.Empty, delegate(string uniqueID)
						{
							SpeechParam value2 = list5[index2];
							value2.Formulas = new List<ConditionBlockRef>();
							value2.Formulas.Add(default(ConditionBlockRef));
							value2.Formulas[0] = ConditionBlockRef.Create(uniqueID);
							list5[index2] = value2;
							OnFieldChanged();
						});
					}
					List<string> strDataOptions = null;
					string title2 = null;
					string title3 = null;
					string title4 = null;
					bool wantSort;
					bool flag2 = SpeechParam.HasStringData(list5[index2].Type, out title2, out strDataOptions, out wantSort);
					listItemObj.FindChild("StringData").SetActive(flag2 && strDataOptions == null);
					listItemObj.FindChild("StringDataDropdown").SetActive(flag2 && strDataOptions != null && strDataOptions.Count > 0);
					if (flag2 && strDataOptions == null)
					{
						PopulateText(listItemObj, "StringData/Text", title2);
						PopulateInputField(listItemObj, "StringData/InputField", list5[index2].StringData, delegate(string stringData)
						{
							SpeechParam value2 = list5[index2];
							value2.StringData = stringData;
							list5[index2] = value2;
							OnFieldChanged();
						});
					}
					else if (flag2 && strDataOptions != null && strDataOptions.Count > 0)
					{
						int val = Math.Max(0, strDataOptions.IndexOf(list5[index2].StringData));
						PopulateText(listItemObj, "StringDataDropdown/Text", title2);
						PopulateDropdown(listItemObj, "StringDataDropdown/Dropdown", strDataOptions.ToArray(), wantSort, val, delegate(int index3)
						{
							SpeechParam value2 = list5[index2];
							value2.StringData = strDataOptions[index3];
							list5[index2] = value2;
							OnFieldChanged();
						});
					}
					string title5 = null;
					bool flag3 = SpeechParam.HasBoolData(list5[index2].Type, out title5);
					listItemObj.FindChild("BoolData").SetActive(flag3);
					if (flag3)
					{
						listItemObj.FindChild("BoolData/Checkbox1").SetActive(title5 != null);
						PopulateText(listItemObj, "BoolData/Checkbox1/Label", title5);
						PopulateCheckboxField(listItemObj, "BoolData/Checkbox1", list5[index2].BoolData, delegate(bool boolData)
						{
							SpeechParam value2 = list5[index2];
							value2.BoolData = boolData;
							list5[index2] = value2;
							OnFieldChanged();
						});
					}
					listItemObj.FindChild("Subject").SetActive(SpeechParam.HasSubject(list5[index2].Type, out var hasSpecifier, out title3));
					listItemObj.FindChild("Subject/Dropdown").SetActive(hasSpecifier);
					listItemObj.FindChild("Subject/Dropdown2").SetActive(list5[index2].Subject != Specifier.ID && hasSpecifier);
					listItemObj.FindChild("Subject/InputField").SetActive(list5[index2].Subject == Specifier.ID || !hasSpecifier);
					listItemObj.FindChild("Object").SetActive(SpeechParam.HasObject(list5[index2].Type, out var hasSpecifier2, out title4));
					listItemObj.FindChild("Object/Dropdown").SetActive(hasSpecifier2);
					listItemObj.FindChild("Object/Dropdown2").SetActive(list5[index2].Object != Specifier.ID && hasSpecifier2);
					listItemObj.FindChild("Object/InputField").SetActive(list5[index2].Object == Specifier.ID || !hasSpecifier2);
					PopulateText(listItemObj, "Subject/Text", (title3 != null) ? title3 : "Subject:");
					PopulateText(listItemObj, "Object/Text", (title4 != null) ? title4 : "Object:");
				}));
			}
			else if (field.FieldType == typeof(List<SpeechRef>))
			{
				List<SpeechRef> list6 = localField.GetValue(ScriptObject) as List<SpeechRef>;
				if (Expanded != this && (list6 == null || list6.Count == 0))
				{
					continue;
				}
				list.Add(AddList<SpeechRef>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemString", "Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateInputField(listItemObj, "InputField", list6[index2].GetUniqueID(), delegate(string uniqueID)
					{
						if (field.Name == "ExtraReplyTo")
						{
							GameImpl.Instance.GetCurrentlyEditingStory().UnregisterExtraReply(ScriptObject);
						}
						list6[index2] = SpeechRef.Create(uniqueID);
						OnFieldChanged();
						if (field.Name == "ExtraReplyTo")
						{
							GameImpl.Instance.GetCurrentlyEditingStory().RegisterExtraReply(ScriptObject);
						}
					});
				}));
			}
			else if (field.FieldType == typeof(List<TriggerRef>))
			{
				List<TriggerRef> list7 = localField.GetValue(ScriptObject) as List<TriggerRef>;
				if (Expanded != this && (list7 == null || list7.Count == 0))
				{
					continue;
				}
				list.Add(AddList<TriggerRef>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemString", "Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateInputField(listItemObj, "InputField", list7[index2].GetUniqueID(), delegate(string uniqueID)
					{
						list7[index2] = TriggerRef.Create(uniqueID);
						OnFieldChanged();
					});
				}));
			}
			else if (field.FieldType == typeof(List<ConditionBlockRef>))
			{
				List<ConditionBlockRef> list8 = localField.GetValue(ScriptObject) as List<ConditionBlockRef>;
				if (Expanded != this && (list8 == null || list8.Count == 0))
				{
					continue;
				}
				list.Add(AddList<ConditionBlockRef>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemString", "Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateInputField(listItemObj, "InputField", list8[index2].GetUniqueID(), delegate(string uniqueID)
					{
						list8[index2] = ConditionBlockRef.Create(uniqueID);
						OnFieldChanged();
					});
				}));
			}
			else if (field.FieldType == typeof(List<StoryEvent>))
			{
				List<StoryEvent> list9 = localField.GetValue(ScriptObject) as List<StoryEvent>;
				if (Expanded != this && (list9 == null || list9.Count == 0))
				{
					continue;
				}
				list.Add(AddList<StoryEvent>(base.gameObject, ref index, field, null, "Prefabs/UI/ListItemEvent", "Type/Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateDropdown(listItemObj, "Type/Dropdown", StoryEvent.TypeNames, sorted: true, (int)list9[index2].Type, delegate(int type)
					{
						StoryEvent value2 = list9[index2];
						value2.Type = (StoryEventType)type;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Type/Delay", list9[index2].Delay.ToString(), delegate(string s)
					{
						StoryEvent value2 = list9[index2];
						float.TryParse(s, out value2.Delay);
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Subject/Dropdown", specifierOptions, list9[index2].Subject, delegate(Specifier subject)
					{
						StoryEvent value2 = list9[index2];
						value2.Subject = subject;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Object/Dropdown", specifierOptions, list9[index2].Object, delegate(Specifier specifier)
					{
						StoryEvent value2 = list9[index2];
						value2.Object = specifier;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "ThirdParty/Dropdown", specifierOptions, list9[index2].ThirdParty, delegate(Specifier thirdParty)
					{
						StoryEvent value2 = list9[index2];
						value2.ThirdParty = thirdParty;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Subject/Dropdown2", modifierNames, sorted: false, (int)list9[index2].SubjectModifier, delegate(int subjectModifier)
					{
						StoryEvent value2 = list9[index2];
						value2.SubjectModifier = (SpecifierModifier)subjectModifier;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Object/Dropdown2", modifierNames, sorted: false, (int)list9[index2].ObjectModifier, delegate(int objectModifier)
					{
						StoryEvent value2 = list9[index2];
						value2.ObjectModifier = (SpecifierModifier)objectModifier;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "ThirdParty/Dropdown2", modifierNames, sorted: false, (int)list9[index2].ThirdPartyModifier, delegate(int thirdPartyModifier)
					{
						StoryEvent value2 = list9[index2];
						value2.ThirdPartyModifier = (SpecifierModifier)thirdPartyModifier;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Subject/InputField", list9[index2].SubjectID, delegate(string subjectID)
					{
						StoryEvent value2 = list9[index2];
						value2.SubjectID = subjectID;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Object/InputField", list9[index2].ObjectID, delegate(string objectID)
					{
						StoryEvent value2 = list9[index2];
						value2.ObjectID = objectID;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "ThirdParty/InputField", list9[index2].ThirdPartyID, delegate(string thirdPartyID)
					{
						StoryEvent value2 = list9[index2];
						value2.ThirdPartyID = thirdPartyID;
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data/InputField", list9[index2].Data.ToString(), delegate(string s)
					{
						StoryEvent value2 = list9[index2];
						float.TryParse(s, out value2.Data);
						list9[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data2/InputField", list9[index2].Data2.ToString(), delegate(string s)
					{
						StoryEvent value2 = list9[index2];
						float.TryParse(s, out value2.Data2);
						list9[index2] = value2;
						OnFieldChanged();
					});
					List<string> strDataOptions = null;
					List<string> strDataOptions2 = null;
					string title = null;
					string title2 = null;
					bool wantSort;
					bool flag = StoryEvent.HasStringData(list9[index2].Type, out title, out strDataOptions, out wantSort);
					listItemObj.FindChild("StringData").SetActive(flag && strDataOptions == null);
					listItemObj.FindChild("StringDataDropdown").SetActive(flag && strDataOptions != null && strDataOptions.Count > 0);
					if (flag && strDataOptions == null)
					{
						PopulateText(listItemObj, "StringData/Text", title);
						PopulateInputField(listItemObj, "StringData/InputField", list9[index2].StringData, delegate(string stringData)
						{
							StoryEvent value2 = list9[index2];
							value2.StringData = stringData;
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					else if (flag && strDataOptions != null && strDataOptions.Count > 0)
					{
						int val = Math.Max(0, strDataOptions.IndexOf(list9[index2].StringData));
						PopulateText(listItemObj, "StringDataDropdown/Text", title);
						PopulateDropdown(listItemObj, "StringDataDropdown/Dropdown", strDataOptions.ToArray(), sorted: true, val, delegate(int index4)
						{
							StoryEvent value2 = list9[index2];
							value2.StringData = strDataOptions[index4];
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					bool wantSort2;
					bool flag2 = StoryEvent.HasStringData2(list9[index2].Type, out title2, out strDataOptions2, out wantSort2);
					listItemObj.FindChild("StringData2").SetActive(flag2 && strDataOptions2 == null);
					listItemObj.FindChild("StringData2Dropdown").SetActive(flag2 && strDataOptions2 != null && strDataOptions2.Count > 0);
					if (flag2 && strDataOptions2 == null)
					{
						PopulateText(listItemObj, "StringData2/Text", title2);
						PopulateInputField(listItemObj, "StringData2/InputField", list9[index2].StringData2, delegate(string stringData)
						{
							StoryEvent value2 = list9[index2];
							value2.StringData2 = stringData;
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					else if (flag2 && strDataOptions2 != null && strDataOptions2.Count > 0)
					{
						int val2 = Math.Max(0, strDataOptions2.IndexOf(list9[index2].StringData2));
						PopulateText(listItemObj, "StringData2Dropdown/Text", title2);
						PopulateDropdown(listItemObj, "StringData2Dropdown/Dropdown", strDataOptions2.ToArray(), sorted: true, val2, delegate(int index4)
						{
							StoryEvent value2 = list9[index2];
							value2.StringData2 = strDataOptions2[index4];
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					string title3 = null;
					string title4 = null;
					string title5 = null;
					string title6 = null;
					string title7 = null;
					bool hasFormula;
					bool active = StoryEvent.HasData(list9[index2].Type, out title3, out hasFormula);
					bool active2 = StoryEvent.HasData2(list9[index2].Type, out title4);
					bool flag3 = StoryEvent.HasBoolData(list9[index2].Type, out title5, out title6, out title7);
					listItemObj.FindChild("Data").SetActive(active);
					listItemObj.FindChild("Data2").SetActive(active2);
					PopulateText(listItemObj, "Data/Text", title3);
					PopulateText(listItemObj, "Data2/Text", title4);
					listItemObj.FindChild("BoolData").SetActive(flag3);
					if (flag3)
					{
						listItemObj.FindChild("BoolData/Checkbox1").SetActive(title5 != null);
						listItemObj.FindChild("BoolData/Checkbox2").SetActive(title6 != null);
						listItemObj.FindChild("BoolData/Checkbox3").SetActive(title7 != null);
						PopulateText(listItemObj, "BoolData/Checkbox1/Label", title5);
						PopulateText(listItemObj, "BoolData/Checkbox2/Label", title6);
						PopulateText(listItemObj, "BoolData/Checkbox3/Label", title7);
						PopulateCheckboxField(listItemObj, "BoolData/Checkbox1", list9[index2].BoolData, delegate(bool boolData)
						{
							StoryEvent value2 = list9[index2];
							value2.BoolData = boolData;
							list9[index2] = value2;
							OnFieldChanged();
						});
						PopulateCheckboxField(listItemObj, "BoolData/Checkbox2", list9[index2].BoolData2, delegate(bool boolData)
						{
							StoryEvent value2 = list9[index2];
							value2.BoolData2 = boolData;
							list9[index2] = value2;
							OnFieldChanged();
						});
						PopulateCheckboxField(listItemObj, "BoolData/Checkbox3", list9[index2].BoolData3, delegate(bool boolData)
						{
							StoryEvent value2 = list9[index2];
							value2.BoolData3 = boolData;
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					listItemObj.FindChild("Formula").SetActive(hasFormula);
					if (hasFormula)
					{
						PopulateInputField(listItemObj, "Formula/InputField", (list9[index2].Formulas != null && list9[index2].Formulas.Count > 0) ? list9[index2].Formulas[0].UniqueID : string.Empty, delegate(string uniqueID)
						{
							StoryEvent value2 = list9[index2];
							value2.Formulas = new List<ConditionBlockRef>();
							value2.Formulas.Add(default(ConditionBlockRef));
							value2.Formulas[0] = ConditionBlockRef.Create(uniqueID);
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					bool flag4 = StoryEvent.HasSecrecy(list9[index2].Type);
					listItemObj.FindChild("SecrecyDropdown").SetActive(flag4);
					if (flag4)
					{
						PopulateText(listItemObj, "SecrecyDropdown/Text", "Secrecy:");
						PopulateDropdown(listItemObj, "SecrecyDropdown/Dropdown", Memory.SecrecyModeNames, sorted: true, (int)list9[index2].Secrecy, delegate(int secrecy)
						{
							StoryEvent value2 = list9[index2];
							value2.Secrecy = (SecrecyMode)secrecy;
							list9[index2] = value2;
							OnFieldChanged();
						});
					}
					string title8 = null;
					string title9 = null;
					string title10 = null;
					listItemObj.FindChild("Subject").SetActive(StoryEvent.HasSubject(list9[index2].Type, out var hasSpecifier, out title8));
					listItemObj.FindChild("Subject/Dropdown").SetActive(hasSpecifier);
					listItemObj.FindChild("Subject/Dropdown2").SetActive(list9[index2].Subject != Specifier.ID && hasSpecifier);
					listItemObj.FindChild("Subject/InputField").SetActive(list9[index2].Subject == Specifier.ID || !hasSpecifier);
					listItemObj.FindChild("Object").SetActive(StoryEvent.HasObject(list9[index2].Type, out var hasSpecifier2, out title9));
					listItemObj.FindChild("Object/Dropdown").SetActive(hasSpecifier2);
					listItemObj.FindChild("Object/Dropdown2").SetActive(list9[index2].Object != Specifier.ID && hasSpecifier2);
					listItemObj.FindChild("Object/InputField").SetActive(list9[index2].Object == Specifier.ID || !hasSpecifier2);
					listItemObj.FindChild("ThirdParty").SetActive(StoryEvent.HasThirdParty(list9[index2].Type, out var hasSpecifier3, out title10));
					listItemObj.FindChild("ThirdParty/Dropdown").SetActive(hasSpecifier3);
					listItemObj.FindChild("ThirdParty/Dropdown2").SetActive(list9[index2].ThirdParty != Specifier.ID && hasSpecifier3);
					listItemObj.FindChild("ThirdParty/InputField").SetActive(list9[index2].ThirdParty == Specifier.ID || !hasSpecifier3);
					PopulateText(listItemObj, "Subject/Text", (title8 != null) ? title8 : "Subject:");
					PopulateText(listItemObj, "Object/Text", (title9 != null) ? title9 : "Object:");
					PopulateText(listItemObj, "ThirdParty/Text", (title10 != null) ? title10 : "Third Party:");
					GameObject gameObject = listItemObj.FindChild("EDITOR_Speeches");
					if (gameObject != null)
					{
						gameObject.SetActive(StoryEvent.HasSpeeches(list9[index2].Type));
					}
					if (StoryEvent.HasSpeeches(list9[index2].Type))
					{
						List<SpeechRef> speeches = list9[index2].Speeches;
						int index3 = listItemObj.transform.childCount;
						AddList(listItemObj, ref index3, "Speeches", speeches, "Prefabs/UI/ListItemString", "Buttons", delegate(int j, GameObject speechRefObj)
						{
							PopulateInputField(speechRefObj, "InputField", speeches[j].GetUniqueID(), delegate(string uniqueID)
							{
								speeches[j] = SpeechRef.Create(uniqueID);
								OnFieldChanged();
							});
						}, delegate
						{
							if (list9[index2].Speeches == null)
							{
								StoryEvent value2 = list9[index2];
								value2.Speeches = new List<SpeechRef>();
								list9[index2] = value2;
							}
							list9[index2].Speeches.Add(default(SpeechRef));
							OnFieldChanged();
						});
					}
				}));
			}
			else
			{
				if (!(field.FieldType == typeof(List<Condition>)))
				{
					continue;
				}
				List<Condition> list10 = localField.GetValue(ScriptObject) as List<Condition>;
				if (Expanded != this && (list10 == null || list10.Count == 0))
				{
					continue;
				}
				ConditionBlock conditionBlock = ScriptObject as ConditionBlock;
				string fieldOverrideName = GetFieldOverrideName();
				list.Add(AddList<Condition>(base.gameObject, ref index, field, fieldOverrideName, "Prefabs/UI/ListItemCondition", "Type/Buttons", delegate(int index2, GameObject listItemObj)
				{
					PopulateCheckboxField(listItemObj, "Type/Not", list10[index2].Not, delegate(bool not)
					{
						Condition value2 = list10[index2];
						value2.Not = not;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Type/Dropdown", Condition.TypeNames, sorted: true, (int)list10[index2].Type, delegate(int type)
					{
						Condition value2 = list10[index2];
						value2.Type = (ConditionType)type;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Subject/Dropdown", specifierOptions, list10[index2].Subject, delegate(Specifier subject)
					{
						Condition value2 = list10[index2];
						value2.Subject = subject;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "Object/Dropdown", specifierOptions, list10[index2].Object, delegate(Specifier specifier)
					{
						Condition value2 = list10[index2];
						value2.Object = specifier;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateSpecifierDropdown(listItemObj, "ThirdParty/Dropdown", specifierOptions, list10[index2].ThirdParty, delegate(Specifier thirdParty)
					{
						Condition value2 = list10[index2];
						value2.ThirdParty = thirdParty;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Subject/Dropdown2", modifierNames, sorted: false, (int)list10[index2].SubjectModifier, delegate(int subjectModifier)
					{
						Condition value2 = list10[index2];
						value2.SubjectModifier = (SpecifierModifier)subjectModifier;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "Object/Dropdown2", modifierNames, sorted: false, (int)list10[index2].ObjectModifier, delegate(int objectModifier)
					{
						Condition value2 = list10[index2];
						value2.ObjectModifier = (SpecifierModifier)objectModifier;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateDropdown(listItemObj, "ThirdParty/Dropdown2", modifierNames, sorted: false, (int)list10[index2].ThirdPartyModifier, delegate(int thirdPartyModifier)
					{
						Condition value2 = list10[index2];
						value2.ThirdPartyModifier = (SpecifierModifier)thirdPartyModifier;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Subject/InputField", list10[index2].SubjectID, delegate(string subjectID)
					{
						Condition value2 = list10[index2];
						value2.SubjectID = subjectID;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Object/InputField", list10[index2].ObjectID, delegate(string objectID)
					{
						Condition value2 = list10[index2];
						value2.ObjectID = objectID;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "ThirdParty/InputField", list10[index2].ThirdPartyID, delegate(string thirdPartyID)
					{
						Condition value2 = list10[index2];
						value2.ThirdPartyID = thirdPartyID;
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data/InputField1", list10[index2].Data.ToString(), delegate(string s)
					{
						Condition value2 = list10[index2];
						float.TryParse(s, out value2.Data);
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Data/InputField2", list10[index2].Data2.ToString(), delegate(string s)
					{
						Condition value2 = list10[index2];
						float.TryParse(s, out value2.Data2);
						list10[index2] = value2;
						OnFieldChanged();
					});
					PopulateInputField(listItemObj, "Priority/InputField", list10[index2].PriorityFactor.ToString(), delegate(string s)
					{
						Condition value2 = list10[index2];
						float.TryParse(s, out value2.PriorityFactor);
						list10[index2] = value2;
						OnFieldChanged();
					});
					listItemObj.FindChild("Subject").SetActive(Condition.HasSubject(list10[index2].Type, out var hasSpecifier, out var title));
					listItemObj.FindChild("Subject/Dropdown").SetActive(hasSpecifier);
					listItemObj.FindChild("Subject/Dropdown2").SetActive(list10[index2].Subject != Specifier.ID && hasSpecifier);
					listItemObj.FindChild("Subject/InputField").SetActive(list10[index2].Subject == Specifier.ID || !hasSpecifier);
					listItemObj.FindChild("Object").SetActive(Condition.HasObject(list10[index2].Type, out var hasSpecifier2, out var title2));
					listItemObj.FindChild("Object/Dropdown").SetActive(hasSpecifier2);
					listItemObj.FindChild("Object/Dropdown2").SetActive(list10[index2].Object != Specifier.ID && hasSpecifier2);
					listItemObj.FindChild("Object/InputField").SetActive(list10[index2].Object == Specifier.ID || !hasSpecifier2);
					listItemObj.FindChild("ThirdParty").SetActive(Condition.HasThirdParty(list10[index2].Type, out var hasSpecifier3, out var title3));
					listItemObj.FindChild("ThirdParty/Dropdown").SetActive(hasSpecifier3);
					listItemObj.FindChild("ThirdParty/Dropdown2").SetActive(list10[index2].ThirdParty != Specifier.ID && hasSpecifier3);
					listItemObj.FindChild("ThirdParty/InputField").SetActive(list10[index2].ThirdParty == Specifier.ID || !hasSpecifier3);
					listItemObj.FindChild("Data").SetActive(Condition.HasData(list10[index2].Type, out var title4, out var hasFormula));
					listItemObj.FindChild("Data/InputField2").SetActive(Condition.HasData2(list10[index2].Type, out var title5));
					PopulateText(listItemObj, "Subject/Text", (title != null) ? title : "Subject:");
					PopulateText(listItemObj, "Object/Text", (title2 != null) ? title2 : "Object:");
					PopulateText(listItemObj, "ThirdParty/Text", (title3 != null) ? title3 : "Third Party:");
					PopulateText(listItemObj, "Data/Title1", title4);
					PopulateText(listItemObj, "Data/Title2", title5);
					listItemObj.FindChild("Priority").SetActive(field.Name == "PriorityTerms" || (conditionBlock != null && ConditionBlock.IsNumericFormula(conditionBlock.Type)));
					if (conditionBlock != null && ConditionBlock.IsNumericFormula(conditionBlock.Type))
					{
						PopulateText(listItemObj, "Priority/Text", "Multiplied By");
					}
					List<string> strDataOptions = null;
					string title6 = null;
					bool wantSort;
					bool flag = Condition.HasStringData(list10[index2].Type, out title6, out strDataOptions, out wantSort);
					listItemObj.FindChild("StringData").SetActive((flag && strDataOptions == null) || list10[index2].Type == ConditionType.IsConditionBlockSatisfied || list10[index2].Type == ConditionType.ConditionBlockResult || hasFormula);
					listItemObj.FindChild("StringDataDropdown").SetActive(flag && strDataOptions != null && strDataOptions.Count > 0);
					if (flag && strDataOptions == null)
					{
						PopulateText(listItemObj, "StringData/Text", title6);
						PopulateInputField(listItemObj, "StringData/InputField", list10[index2].StringData, delegate(string stringData)
						{
							Condition value2 = list10[index2];
							value2.StringData = stringData;
							list10[index2] = value2;
							OnFieldChanged();
						});
					}
					else if (flag && strDataOptions != null && strDataOptions.Count > 0)
					{
						int val = Math.Max(0, strDataOptions.IndexOf(list10[index2].StringData));
						PopulateText(listItemObj, "StringDataDropdown/Text", title6);
						PopulateDropdown(listItemObj, "StringDataDropdown/Dropdown", strDataOptions.ToArray(), wantSort, val, delegate(int index4)
						{
							Condition value2 = list10[index2];
							value2.StringData = strDataOptions[index4];
							list10[index2] = value2;
							OnFieldChanged();
						});
					}
					string title7;
					bool flag2 = Condition.HasBoolData(list10[index2].Type, out title7);
					listItemObj.FindChild("BoolData").SetActive(flag2);
					if (flag2)
					{
						listItemObj.FindChild("BoolData/Checkbox1").SetActive(title7 != null);
						PopulateText(listItemObj, "BoolData/Checkbox1/Label", title7);
						PopulateCheckboxField(listItemObj, "BoolData/Checkbox1", list10[index2].BoolData, delegate(bool boolData)
						{
							Condition value2 = list10[index2];
							value2.BoolData = boolData;
							list10[index2] = value2;
							OnFieldChanged();
						});
					}
					GameObject gameObject = listItemObj.FindChild("EDITOR_ConditionBlocks");
					if (gameObject != null)
					{
						gameObject.SetActive(Condition.HasConditionBlocks(list10[index2].Type));
					}
					if (Condition.HasConditionBlocks(list10[index2].Type))
					{
						List<ConditionBlockRef> conditionBlocks = list10[index2].ConditionBlocks;
						int index3 = listItemObj.transform.childCount;
						AddList(listItemObj, ref index3, "ConditionBlocks", conditionBlocks, "Prefabs/UI/ListItemString", "Buttons", delegate(int j, GameObject conditionBlockRefObj)
						{
							PopulateInputField(conditionBlockRefObj, "InputField", conditionBlocks[j].GetUniqueID(), delegate(string uniqueID)
							{
								conditionBlocks[j] = ConditionBlockRef.Create(uniqueID);
								OnFieldChanged();
							});
						}, delegate
						{
							if (list10[index2].ConditionBlocks == null)
							{
								Condition value2 = list10[index2];
								value2.ConditionBlocks = new List<ConditionBlockRef>();
								list10[index2] = value2;
							}
							list10[index2].ConditionBlocks.Add(default(ConditionBlockRef));
							OnFieldChanged();
						});
					}
					if (list10[index2].Type == ConditionType.IsConditionBlockSatisfied || list10[index2].Type == ConditionType.ConditionBlockResult || hasFormula)
					{
						List<ConditionBlockRef> conditionBlocks2 = list10[index2].ConditionBlocks;
						PopulateText(listItemObj, "StringData/Text", hasFormula ? "+ Formula Block:" : "Condition Block:");
						PopulateInputField(listItemObj, "StringData/InputField", (conditionBlocks2 != null && conditionBlocks2.Count > 0) ? conditionBlocks2[0].UniqueID : "", delegate(string uniqueID)
						{
							if (list10[index2].ConditionBlocks == null)
							{
								Condition value2 = list10[index2];
								value2.ConditionBlocks = new List<ConditionBlockRef>();
								list10[index2] = value2;
							}
							if (list10[index2].ConditionBlocks.Count == 0)
							{
								list10[index2].ConditionBlocks.Add(default(ConditionBlockRef));
							}
							else if (list10[index2].ConditionBlocks.Count > 1)
							{
								list10[index2].ConditionBlocks.RemoveRange(1, list10[index2].ConditionBlocks.Count - 1);
							}
							list10[index2].ConditionBlocks[0] = ConditionBlockRef.Create(uniqueID);
							OnFieldChanged();
						});
					}
				}));
			}
		}
		if (Expanded != this)
		{
			list.Add(BaseMenu.AddButtonField(base.gameObject, ref index, "EDITOR_Expand", -1, delegate
			{
				if (Expanded != null)
				{
					Expanded.WantUpdateFields = true;
				}
				Expanded = this;
				base.gameObject.transform.SetAsLastSibling();
				WantUpdateFields = true;
				WantSetConnectionsDirty = true;
			}));
		}
		else
		{
			list.Add(BaseMenu.AddButtonField(base.gameObject, ref index, "EDITOR_Collapse", -1, delegate
			{
				Expanded = null;
				WantUpdateFields = true;
				WantSetConnectionsDirty = true;
			}));
		}
		DeleteInvalidFields(base.gameObject, list);
		WantUpdateFields = false;
	}

	public void OnFieldChanged()
	{
		WantUpdateFields = true;
		ScriptEditor.Instance.OnFieldChanged();
	}

	public Dropdown PopulateDropdown(GameObject listItemObj, string dropDownName, string[] options, bool sorted, int val, UnityAction<int> call)
	{
		Dropdown component = listItemObj.transform.Find(dropDownName).GetComponent<Dropdown>();
		component.ClearOptions();
		List<string> optionsList = new List<string>(options);
		if (sorted)
		{
			optionsList.Sort();
		}
		component.AddOptions(optionsList);
		component.onValueChanged.RemoveAllListeners();
		component.value = optionsList.IndexOf(options[val]);
		component.onValueChanged.AddListener(delegate(int vv)
		{
			int num = Array.IndexOf(options, optionsList[vv]);
			if (num != -1)
			{
				call(num);
			}
		});
		return component;
	}

	public Dropdown PopulateSpecifierDropdown(GameObject listItemObj, string dropDownName, SpecifierOption[] options, Specifier val, UnityAction<Specifier> call)
	{
		int value = 0;
		List<string> list = new List<string>();
		for (int i = 0; i < options.Length; i++)
		{
			if (options[i].Val == val)
			{
				value = i;
			}
			list.Add(options[i].Name);
		}
		Dropdown component = listItemObj.transform.Find(dropDownName).GetComponent<Dropdown>();
		component.ClearOptions();
		component.AddOptions(list);
		component.onValueChanged.RemoveAllListeners();
		component.value = value;
		component.onValueChanged.AddListener(delegate(int vv)
		{
			Specifier val2 = options[vv].Val;
			call(val2);
		});
		return component;
	}

	public InputField PopulateInputField(GameObject listItemObj, string inputFieldName, string val, UnityAction<string> call)
	{
		InputField inputField = listItemObj.transform.Find(inputFieldName).GetComponent<InputField>();
		inputField.onValueChanged.RemoveAllListeners();
		inputField.onEndEdit.RemoveAllListeners();
		inputField.SetUnityText(val);
		inputField.onEndEdit.AddListener(call);
		inputField.onValueChanged.AddListener(delegate
		{
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)inputField.transform);
		});
		return inputField;
	}

	public Toggle PopulateCheckboxField(GameObject listItemObj, string toggleName, bool val, UnityAction<bool> call)
	{
		Toggle component = listItemObj.transform.Find(toggleName).GetComponent<Toggle>();
		component.onValueChanged.RemoveAllListeners();
		component.isOn = val;
		component.onValueChanged.AddListener(call);
		return component;
	}

	public Text PopulateText(GameObject listItemObj, string textObjName, string text)
	{
		Text component = listItemObj.transform.Find(textObjName).GetComponent<Text>();
		component.SetUnityText(text);
		return component;
	}

	public GameObject AddList<T>(GameObject panel, ref int index, FieldInfo field, string overrideName, string listItemPrefabName, string buttonsFrameName, PopulateListItem populateFunc) where T : IScriptListItem<T>, new()
	{
		List<T> list = field.GetValue(ScriptObject) as List<T>;
		return AddList(panel, ref index, (overrideName != null) ? overrideName : field.Name, list, listItemPrefabName, buttonsFrameName, populateFunc, delegate
		{
			if (list == null)
			{
				list = new List<T>();
				field.SetValue(ScriptObject, list);
			}
			T item = new T();
			item.Construct();
			list.Add(item);
			OnFieldChanged();
		});
	}

	public GameObject AddList<T>(GameObject panel, ref int index, string fieldName, List<T> list, string listItemPrefabName, string buttonsFrameName, PopulateListItem populateFunc, UnityAction call) where T : IScriptListItem<T>, new()
	{
		string key = "EDITOR_" + fieldName;
		GameObject gameObject = panel.FindChild(key);
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/ListField"), panel.transform, worldPositionStays: false);
			gameObject.name = key;
			GameObject obj = gameObject.transform.Find("ListHeader/Text").gameObject;
			obj.GetComponent<Text>().SetUnityText(GameImpl.Translate(key) + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject obj2 = gameObject.transform.Find("ListHeader/Buttons/AddButton").gameObject;
		obj2.GetComponent<Button>().onClick.RemoveAllListeners();
		obj2.GetComponent<Button>().onClick.AddListener(call);
		List<GameObject> list2 = new List<GameObject>();
		GameObject gameObject2 = gameObject.FindChild("ListHeader");
		gameObject2.transform.SetSiblingIndex(0);
		list2.Add(gameObject2);
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int localIndex = i;
				GameObject gameObject3 = gameObject.FindChild("ListItem" + i);
				if (gameObject3 == null)
				{
					gameObject3 = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(listItemPrefabName), gameObject.transform, worldPositionStays: false);
					gameObject3.name = "ListItem" + i;
				}
				gameObject3.transform.SetSiblingIndex(i + 1);
				list2.Add(gameObject3);
				ScriptObjectItemBehaviour itemBehaviour = gameObject3.GetComponent<ScriptObjectItemBehaviour>();
				UnityAction unityAction = delegate
				{
					if (fieldName == "ExtraReplyTo")
					{
						GameImpl.Instance.GetCurrentlyEditingStory().UnregisterExtraReply(ScriptObject);
					}
					ScriptEditor.Instance.DeselectItem(itemBehaviour);
					list.RemoveAt(localIndex);
					OnFieldChanged();
					if (fieldName == "ExtraReplyTo")
					{
						GameImpl.Instance.GetCurrentlyEditingStory().UnregisterExtraReply(ScriptObject);
					}
				};
				if (itemBehaviour != null)
				{
					itemBehaviour.Owner = this;
					itemBehaviour.ListIndex = i;
					itemBehaviour.FieldName = fieldName;
					itemBehaviour.TypeName = list[i].GetTypeName();
					itemBehaviour.CopyFunc = delegate(XmlWriter writer, List<ScriptObjectItemBehaviour> selectedItems)
					{
						ScriptListItems<T> scriptListItems = new ScriptListItems<T>
						{
							Items = new List<T>()
						};
						foreach (ScriptObjectItemBehaviour selectedItem in selectedItems)
						{
							scriptListItems.Items.Add(list[selectedItem.ListIndex]);
						}
						XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces(new XmlQualifiedName[1] { XmlQualifiedName.Empty });
						new XmlSerializer(typeof(ScriptListItems<T>)).Serialize(writer, scriptListItems, namespaces);
					};
					itemBehaviour.PasteFunc = delegate(XmlReader reader)
					{
						try
						{
							object obj6 = new XmlSerializer(typeof(ScriptListItems<T>)).Deserialize(reader);
							if (obj6 != null)
							{
								Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
								Script editingScript = ScriptEditor.Instance.EditingScript;
								ScriptListItems<T> scriptListItems = (ScriptListItems<T>)obj6;
								for (int j = 0; j < scriptListItems.Items.Count; j++)
								{
									T item = scriptListItems.Items[j].FixupAfterXmlLoad(editingScript, currentlyEditingStory, null);
									list.Insert(localIndex + j, item);
								}
								OnFieldChanged();
								return true;
							}
							return false;
						}
						catch (Exception ex)
						{
							Debug.LogWarning(ex.Message);
							return false;
						}
					};
					itemBehaviour.DeleteFunc = unityAction;
				}
				RawImage component = gameObject3.GetComponent<RawImage>();
				if (component != null)
				{
					if (itemBehaviour != null && ScriptEditor.Instance.SelectedItems.Contains(itemBehaviour))
					{
						component.color = ((i % 2 == 0) ? SelectedItemCol : SelectedItemAltCol);
					}
					else
					{
						component.color = ((i % 2 == 0) ? ItemCol : ItemAltCol);
					}
				}
				populateFunc(i, gameObject3);
				GameObject obj3 = gameObject3.transform.Find(buttonsFrameName + "/DownButton").gameObject;
				obj3.SetActive(i < list.Count - 1);
				obj3.GetComponent<Button>().onClick.RemoveAllListeners();
				obj3.GetComponent<Button>().onClick.AddListener(delegate
				{
					T value = list[localIndex + 1];
					list[localIndex + 1] = list[localIndex];
					list[localIndex] = value;
					OnFieldChanged();
				});
				GameObject obj4 = gameObject3.transform.Find(buttonsFrameName + "/UpButton").gameObject;
				obj4.SetActive(i > 0);
				obj4.GetComponent<Button>().onClick.RemoveAllListeners();
				obj4.GetComponent<Button>().onClick.AddListener(delegate
				{
					T value = list[localIndex - 1];
					list[localIndex - 1] = list[localIndex];
					list[localIndex] = value;
					OnFieldChanged();
				});
				GameObject obj5 = gameObject3.transform.Find(buttonsFrameName + "/DeleteButton").gameObject;
				obj5.GetComponent<Button>().onClick.RemoveAllListeners();
				obj5.GetComponent<Button>().onClick.AddListener(unityAction);
			}
		}
		DeleteInvalidFields(gameObject, list2);
		return gameObject;
	}

	public GameObject GetListField(FieldInfo field, int i)
	{
		string fieldOverrideName = GetFieldOverrideName();
		string text = ((fieldOverrideName != null) ? fieldOverrideName : field.Name);
		return base.gameObject.FindChild("EDITOR_" + text + "/ListItem" + i);
	}

	public string GetFieldOverrideName()
	{
		string result = null;
		if (ScriptObject is ConditionBlock conditionBlock)
		{
			switch (conditionBlock.Type)
			{
			case ConditionBlockType.Or:
				result = "ConditionsAny";
				break;
			case ConditionBlockType.Sum:
				result = "ConditionsSum";
				break;
			case ConditionBlockType.SumRoundedUp:
				result = "SumRoundedUp";
				break;
			case ConditionBlockType.SumRoundedDown:
				result = "SumRoundedDown";
				break;
			case ConditionBlockType.Min:
				result = "ConditionsMin";
				break;
			case ConditionBlockType.Max:
				result = "ConditionsMax";
				break;
			case ConditionBlockType.Multiply:
				result = "ConditionsMultiply";
				break;
			}
		}
		return result;
	}

	private void DeleteInvalidFields(GameObject panel, List<GameObject> validObjects)
	{
		for (int i = 0; i < panel.transform.childCount; i++)
		{
			GameObject gameObject = panel.transform.GetChild(i).gameObject;
			if (!validObjects.Contains(gameObject))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public GameObject AddHeaderField(GameObject panel, ref int index, string name, string v, Color col)
	{
		GameObject gameObject = panel.FindChild(name);
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/UI/HeaderField"), panel.transform, worldPositionStays: false);
			gameObject.name = name;
			gameObject.GetComponent<RawImage>().color = col;
			GameObject obj = gameObject.FindChild("Text");
			obj.GetComponent<Text>().SetUnityText(GameImpl.Translate(name) + ":");
			obj.GetComponent<TranslateBehaviour>().DontTranslate();
		}
		gameObject.transform.SetSiblingIndex(index++);
		GameObject inputFieldObj = gameObject.FindChild("InputField");
		inputFieldObj.GetComponent<InputField>().onValueChanged.RemoveAllListeners();
		inputFieldObj.GetComponent<InputField>().onEndEdit.RemoveAllListeners();
		inputFieldObj.GetComponent<InputField>().SetUnityText(v);
		inputFieldObj.GetComponent<InputField>().onEndEdit.AddListener(delegate(string vv)
		{
			Story currentlyEditingStory = GameImpl.Instance.GetCurrentlyEditingStory();
			BaseScriptObject baseScriptObject = currentlyEditingStory.FindScriptObjectByUniqueID(vv);
			if (baseScriptObject != null && baseScriptObject != ScriptObject)
			{
				inputFieldObj.GetComponent<InputField>().SetUnityText(ScriptObject.UniqueID);
				GameImpl.Instance.ShowMessageBox(GameImpl.Translate("EDITOR_DuplicateID").Replace("%1", vv));
			}
			else
			{
				currentlyEditingStory.UnregisterScriptObject(ScriptObject);
				ScriptObject.UniqueID = vv;
				ScriptObject.OnUniqueIDChanged();
				OnFieldChanged();
				for (int i = 0; i < ScriptEditor.Instance.UnityContentPane.transform.childCount; i++)
				{
					ScriptObjectBehaviour component = ScriptEditor.Instance.UnityContentPane.transform.GetChild(i).gameObject.GetComponent<ScriptObjectBehaviour>();
					if (component != null)
					{
						component.WantUpdateFields = true;
					}
				}
				currentlyEditingStory.RegisterScriptObject(ScriptObject);
			}
		});
		inputFieldObj.GetComponent<InputField>().onValueChanged.AddListener(delegate
		{
			LayoutRebuilder.MarkLayoutForRebuild((RectTransform)inputFieldObj.transform);
		});
		GameObject obj2 = gameObject.transform.Find("Buttons/DeleteButton").gameObject;
		obj2.GetComponent<Button>().onClick.RemoveAllListeners();
		obj2.GetComponent<Button>().onClick.AddListener(delegate
		{
			ScriptEditor.Instance.DeleteScriptObject(this);
		});
		return gameObject;
	}

	public Rect GetRect()
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		return new Rect(base.transform.localPosition.x, base.transform.localPosition.y, rectTransform.rect.width, rectTransform.rect.height);
	}
}
