using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

public class Script
{
	public enum ReferenceType
	{
		Other,
		EnabledTriggers,
		QuestTriggers,
		Price
	}

	[XmlIgnore]
	public string UniqueID = "";

	public int NextFreeSpeechID = 1;

	public int NextFreeTriggerID = 1;

	public int NextFreeConditionBlockID = 1;

	public int NextFreeQuestID = 1;

	public int NextFreeQuestGroupID = 1;

	public int NextFreeTemplateID = 1;

	public int NextFreeInvaderID = 1;

	public Vector2 EditorScrollPos = Vector2.zero;

	public float EditorZoomScale = 1f;

	public List<Speech> Speeches = new List<Speech>();

	public List<Trigger> Triggers = new List<Trigger>();

	public List<ConditionBlock> ConditionBlocks = new List<ConditionBlock>();

	public List<Quest> Quests = new List<Quest>();

	public List<QuestGroup> QuestGroups = new List<QuestGroup>();

	public List<Template> Templates = new List<Template>();

	public List<Invader> Invaders = new List<Invader>();

	public void FixupAfterXmlLoad(Story story)
	{
		foreach (Speech speech in Speeches)
		{
			speech.FixupAfterXmlLoad(this, story, null);
		}
		foreach (Trigger trigger in Triggers)
		{
			trigger.FixupAfterXmlLoad(this, story, null);
		}
		foreach (ConditionBlock conditionBlock in ConditionBlocks)
		{
			conditionBlock.FixupAfterXmlLoad(this, story, null);
		}
		foreach (Quest quest in Quests)
		{
			quest.FixupAfterXmlLoad(this, story, null);
		}
		foreach (QuestGroup questGroup in QuestGroups)
		{
			questGroup.FixupAfterXmlLoad(this, story, null);
		}
		foreach (Template template in Templates)
		{
			template.FixupAfterXmlLoad(this, story, null);
		}
		foreach (Invader invader in Invaders)
		{
			invader.FixupAfterXmlLoad(this, story, null);
		}
	}

	public static Script LoadFromFile(string fileName, Translation englishTranslation, Story story)
	{
		try
		{
			if (File.Exists(fileName))
			{
				using (StreamReader textReader = new StreamReader(fileName))
				{
					Script script = (Script)new XmlSerializer(typeof(Script)).Deserialize(textReader);
					script.UniqueID = Path.GetFileNameWithoutExtension(fileName);
					foreach (Speech speech in script.Speeches)
					{
						story.RegisterScriptObject(speech);
					}
					foreach (Trigger trigger in script.Triggers)
					{
						story.RegisterScriptObject(trigger);
					}
					foreach (ConditionBlock conditionBlock in script.ConditionBlocks)
					{
						story.RegisterScriptObject(conditionBlock);
					}
					foreach (Quest quest in script.Quests)
					{
						story.RegisterScriptObject(quest);
					}
					foreach (QuestGroup questGroup in script.QuestGroups)
					{
						story.RegisterScriptObject(questGroup);
					}
					foreach (Template template in script.Templates)
					{
						story.RegisterScriptObject(template);
					}
					foreach (Invader invader in script.Invaders)
					{
						story.RegisterScriptObject(invader);
					}
					return script;
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
			GameImpl instance = GameImpl.Instance;
			Story currentlyEditingStory = instance.GetCurrentlyEditingStory();
			if (instance.Settings.Language != Language.English)
			{
				foreach (Speech speech in Speeches)
				{
					speech.SaveTranslatedText(currentlyEditingStory);
				}
				foreach (Quest quest in Quests)
				{
					quest.SaveTranslatedText(currentlyEditingStory);
				}
				foreach (QuestGroup questGroup in QuestGroups)
				{
					questGroup.SaveTranslatedText(currentlyEditingStory);
				}
				foreach (Template template in Templates)
				{
					template.SaveTranslatedText(currentlyEditingStory);
				}
				foreach (Invader invader in Invaders)
				{
					invader.SaveTranslatedText(currentlyEditingStory);
				}
			}
			Directory.CreateDirectory(Path.GetDirectoryName(fileName));
			using (FileStream stream = File.Create(fileName))
			{
				new XmlSerializer(typeof(Script)).Serialize(stream, this);
			}
			currentlyEditingStory.BuildTSVFile();
			CopyFileBackToUnityFolder(fileName);
			if (instance.Settings.Language != Language.English)
			{
				currentlyEditingStory.BuildTranslatedTSVFile(instance.Settings.Language);
				foreach (Speech speech2 in Speeches)
				{
					speech2.LoadTranslatedText(currentlyEditingStory);
				}
				foreach (Quest quest2 in Quests)
				{
					quest2.LoadTranslatedText(currentlyEditingStory);
				}
				foreach (QuestGroup questGroup2 in QuestGroups)
				{
					questGroup2.LoadTranslatedText(currentlyEditingStory);
				}
				foreach (Template template2 in Templates)
				{
					template2.LoadTranslatedText(currentlyEditingStory);
				}
				foreach (Invader invader2 in Invaders)
				{
					invader2.LoadTranslatedText(currentlyEditingStory);
				}
			}
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("Failed to save '" + fileName + "': " + ex.ToString());
			GameImpl.Instance.ShowMessageBox(GameImpl.Translate("MENU_FileSaveFailed").Replace("%1", ex.Message));
			return false;
		}
	}

	public static void CopyFileBackToUnityFolder(string fileName)
	{
		Debug.Log("Saved: " + fileName);
		string text = "C:/Survivalist2/Build/Survivalist Invisible Strain_Data/StreamingAssets";
		string text2 = "C:/Survivalist2/Assets/StreamingAssets";
		if (fileName.StartsWith(text) && Directory.Exists(text2))
		{
			string text3 = text2 + fileName.Substring(text.Length);
			Debug.Log("Copying back to: " + text3);
			Directory.CreateDirectory(Path.GetDirectoryName(text3));
			File.Copy(fileName, text3, overwrite: true);
		}
	}

	public void AddScriptObject(BaseScriptObject obj)
	{
		if (obj is Speech)
		{
			Speeches.Add(obj as Speech);
		}
		else if (obj is Trigger)
		{
			Triggers.Add(obj as Trigger);
		}
		else if (obj is ConditionBlock)
		{
			ConditionBlocks.Add(obj as ConditionBlock);
		}
		else if (obj is Quest)
		{
			Quests.Add(obj as Quest);
		}
		else if (obj is QuestGroup)
		{
			QuestGroups.Add(obj as QuestGroup);
		}
		else if (obj is Template)
		{
			Templates.Add(obj as Template);
		}
		else if (obj is Invader)
		{
			Invaders.Add(obj as Invader);
		}
	}

	public BaseScriptObject FindFirstReferenceTo(BaseScriptObject obj, out ReferenceType referenceType)
	{
		foreach (Speech speech in Speeches)
		{
			if (HasReferenceTo(speech, obj, out referenceType))
			{
				return speech;
			}
		}
		foreach (Trigger trigger in Triggers)
		{
			if (HasReferenceTo(trigger, obj, out referenceType))
			{
				return trigger;
			}
		}
		foreach (ConditionBlock conditionBlock in ConditionBlocks)
		{
			if (HasReferenceTo(conditionBlock, obj, out referenceType))
			{
				return conditionBlock;
			}
		}
		foreach (Quest quest in Quests)
		{
			if (HasReferenceTo(quest, obj, out referenceType))
			{
				return quest;
			}
		}
		foreach (QuestGroup questGroup in QuestGroups)
		{
			if (HasReferenceTo(questGroup, obj, out referenceType))
			{
				return questGroup;
			}
		}
		foreach (Template template in Templates)
		{
			if (HasReferenceTo(template, obj, out referenceType))
			{
				return template;
			}
		}
		foreach (Invader invader in Invaders)
		{
			if (HasReferenceTo(invader, obj, out referenceType))
			{
				return invader;
			}
		}
		referenceType = ReferenceType.Other;
		return null;
	}

	private bool HasReferenceTo(BaseScriptObject obj1, BaseScriptObject obj2, out ReferenceType referenceType)
	{
		FieldInfo[] fields = obj1.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			FieldInfo fieldInfo2 = fieldInfo;
			if (fieldInfo.FieldType == typeof(List<SpeechRef>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<SpeechRef> list))
				{
					continue;
				}
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].GetSpeech() == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
				}
			}
			else if (fieldInfo.FieldType == typeof(List<TriggerRef>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<TriggerRef> list2))
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k].GetTrigger() == obj2)
					{
						referenceType = ReferenceType.QuestTriggers;
						return true;
					}
				}
			}
			else if (fieldInfo.FieldType == typeof(List<Condition>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<Condition> list3))
				{
					continue;
				}
				for (int l = 0; l < list3.Count; l++)
				{
					if (Condition.HasConditionBlocks(list3[l].Type))
					{
						if (list3[l].ConditionBlocks == null)
						{
							continue;
						}
						for (int m = 0; m < list3[l].ConditionBlocks.Count; m++)
						{
							if (list3[l].ConditionBlocks[m].GetConditionBlock() == obj2)
							{
								referenceType = ((list3[l].Type == ConditionType.CanAffordPrice) ? ReferenceType.Price : ReferenceType.Other);
								return true;
							}
						}
					}
					else if ((list3[l].Type == ConditionType.IsConditionBlockSatisfied || list3[l].Type == ConditionType.ConditionBlockResult) && list3[l].ConditionBlocks != null && list3[l].ConditionBlocks != null && list3[l].ConditionBlocks.Count > 0 && list3[l].ConditionBlocks[0].GetConditionBlock() == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
				}
			}
			else if (fieldInfo.FieldType == typeof(List<SpeechParam>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<SpeechParam> list4))
				{
					continue;
				}
				for (int n = 0; n < list4.Count; n++)
				{
					if (list4[n].Formulas != null && list4[n].Formulas.Count > 0 && list4[n].Formulas[0].GetConditionBlock() != null && list4[n].Formulas[0].GetConditionBlock() == obj2)
					{
						referenceType = ((list4[n].Type == SpeechParamType.PriceToSell) ? ReferenceType.Price : ReferenceType.Other);
						return true;
					}
				}
			}
			else if (fieldInfo.FieldType == typeof(List<StoryEvent>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<StoryEvent> list5))
				{
					continue;
				}
				for (int num = 0; num < list5.Count; num++)
				{
					if (StoryEvent.HasSpeeches(list5[num].Type) && list5[num].Speeches != null)
					{
						for (int num2 = 0; num2 < list5[num].Speeches.Count; num2++)
						{
							if (list5[num].Speeches[num2].GetSpeech() == obj2)
							{
								referenceType = ((list5[num].Type == StoryEventType.Sell || list5[num].Type == StoryEventType.TakeAllSupplies) ? ReferenceType.Price : ReferenceType.Other);
								return true;
							}
						}
					}
					string text = null;
					ReferenceType referenceType2 = ReferenceType.Other;
					if (list5[num].Type == StoryEventType.OneShotTrigger || list5[num].Type == StoryEventType.SpawnTemplate || list5[num].Type == StoryEventType.FleeAndDisappear)
					{
						text = list5[num].StringData;
						referenceType2 = ReferenceType.Other;
					}
					else if (list5[num].Type == StoryEventType.DisableTrigger || list5[num].Type == StoryEventType.EnableTrigger)
					{
						text = list5[num].StringData;
						referenceType2 = ReferenceType.EnabledTriggers;
					}
					else if (list5[num].Type == StoryEventType.SpawnEquipment)
					{
						text = list5[num].StringData2;
						referenceType2 = ReferenceType.Other;
					}
					if (text != null && GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(text) == obj2)
					{
						referenceType = referenceType2;
						return true;
					}
					if (list5[num].Formulas != null && list5[num].Formulas.Count > 0 && list5[num].Formulas[0].GetConditionBlock() != null && list5[num].Formulas[0].GetConditionBlock() == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
				}
			}
			else if (fieldInfo.FieldType == typeof(List<TemplateChild>))
			{
				if (!(fieldInfo2.GetValue(obj1) is List<TemplateChild> list6))
				{
					continue;
				}
				for (int num3 = 0; num3 < list6.Count; num3++)
				{
					if (!string.IsNullOrEmpty(list6[num3].UniqueID) && GameImpl.Instance.GetCurrentlyEditingStory().FindTemplateByUniqueID(list6[num3].UniqueID) == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
					if (list6[num3].MinFormula.GetConditionBlock() != null && list6[num3].MinFormula.GetConditionBlock() == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
					if (list6[num3].MaxFormula.GetConditionBlock() != null && list6[num3].MaxFormula.GetConditionBlock() == obj2)
					{
						referenceType = ReferenceType.Other;
						return true;
					}
				}
			}
			else if (fieldInfo.Name == "TemplateID")
			{
				string text2 = fieldInfo2.GetValue(obj1) as string;
				if (!string.IsNullOrEmpty(text2) && GameImpl.Instance.GetCurrentlyEditingStory().FindTemplateByUniqueID(text2) == obj2)
				{
					referenceType = ReferenceType.Other;
					return true;
				}
			}
		}
		referenceType = ReferenceType.Other;
		return false;
	}
}
