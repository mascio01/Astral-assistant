using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class ScriptConnectionsGraphic : Graphic
{
	private static string NoReplyStr = "NoReply";

	private static float NoReplyGrey = 0.75f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();
		ScriptEditor instance = ScriptEditor.Instance;
		if (instance == null)
		{
			return;
		}
		for (int i = 0; i < instance.UnityContentPane.transform.childCount; i++)
		{
			GameObject obj = instance.UnityContentPane.transform.GetChild(i).gameObject;
			_ = (RectTransform)obj.transform;
			ScriptObjectBehaviour component = obj.GetComponent<ScriptObjectBehaviour>();
			if (!(component != null))
			{
				continue;
			}
			FieldInfo[] fields = component.ScriptObject.GetType().GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				FieldInfo fieldInfo2 = fieldInfo;
				if (fieldInfo.FieldType == typeof(List<SpeechRef>))
				{
					if (fieldInfo2.GetValue(component.ScriptObject) is List<SpeechRef> list)
					{
						for (int k = 0; k < list.Count; k++)
						{
							Speech speech = list[k].GetSpeech();
							GameObject listField = component.GetListField(fieldInfo, k);
							AddConnection(instance, listField, speech, vh, (fieldInfo.Name == NoReplyStr) ? new Color(NoReplyGrey, NoReplyGrey, NoReplyGrey) : Color.black);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<TriggerRef>))
				{
					if (fieldInfo2.GetValue(component.ScriptObject) is List<TriggerRef> list2)
					{
						for (int l = 0; l < list2.Count; l++)
						{
							Trigger trigger = list2[l].GetTrigger();
							GameObject listField2 = component.GetListField(fieldInfo, l);
							AddConnection(instance, listField2, trigger, vh, ScriptObjectBehaviour.TriggerCol);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<ConditionBlockRef>))
				{
					if (fieldInfo2.GetValue(component.ScriptObject) is List<ConditionBlockRef> list3)
					{
						for (int m = 0; m < list3.Count; m++)
						{
							ConditionBlock conditionBlock = list3[m].GetConditionBlock();
							GameObject listField3 = component.GetListField(fieldInfo, m);
							AddConnection(instance, listField3, conditionBlock, vh, ScriptObjectBehaviour.ConditionBlockCol);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<Condition>))
				{
					if (!(fieldInfo2.GetValue(component.ScriptObject) is List<Condition> list4))
					{
						continue;
					}
					for (int n = 0; n < list4.Count; n++)
					{
						string title;
						bool hasFormula;
						if (Condition.HasConditionBlocks(list4[n].Type))
						{
							if (list4[n].ConditionBlocks == null)
							{
								continue;
							}
							GameObject listField4 = component.GetListField(fieldInfo, n);
							if (listField4 != null)
							{
								for (int num = 0; num < list4[n].ConditionBlocks.Count; num++)
								{
									ConditionBlock conditionBlock2 = list4[n].ConditionBlocks[num].GetConditionBlock();
									GameObject startGameObj = listField4.FindChild("EDITOR_ConditionBlocks/ListItem" + num);
									AddConnection(instance, startGameObj, conditionBlock2, vh, ScriptObjectBehaviour.ConditionBlockCol);
								}
							}
						}
						else if ((list4[n].Type == ConditionType.IsConditionBlockSatisfied || list4[n].Type == ConditionType.ConditionBlockResult || (Condition.HasData(list4[n].Type, out title, out hasFormula) && hasFormula)) && list4[n].ConditionBlocks != null)
						{
							GameObject listField5 = component.GetListField(fieldInfo, n);
							ConditionBlock target = ((list4[n].ConditionBlocks != null && list4[n].ConditionBlocks.Count > 0) ? list4[n].ConditionBlocks[0].GetConditionBlock() : null);
							AddConnection(instance, listField5, target, vh, ScriptObjectBehaviour.ConditionBlockCol);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<SpeechParam>))
				{
					if (!(fieldInfo2.GetValue(component.ScriptObject) is List<SpeechParam> list5))
					{
						continue;
					}
					for (int num2 = 0; num2 < list5.Count; num2++)
					{
						if (list5[num2].Formulas != null)
						{
							GameObject listField6 = component.GetListField(fieldInfo, num2);
							for (int num3 = 0; num3 < Math.Min(1, list5[num2].Formulas.Count); num3++)
							{
								ConditionBlock conditionBlock3 = list5[num2].Formulas[num3].GetConditionBlock();
								GameObject startGameObj2 = listField6.FindChild("Formula");
								AddConnection(instance, startGameObj2, conditionBlock3, vh, ScriptObjectBehaviour.ConditionBlockCol);
							}
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<StoryEvent>))
				{
					if (!(fieldInfo2.GetValue(component.ScriptObject) is List<StoryEvent> list6))
					{
						continue;
					}
					for (int num4 = 0; num4 < list6.Count; num4++)
					{
						if (StoryEvent.HasSpeeches(list6[num4].Type))
						{
							if (list6[num4].Speeches != null)
							{
								GameObject listField7 = component.GetListField(fieldInfo, num4);
								if (listField7 != null)
								{
									for (int num5 = 0; num5 < list6[num4].Speeches.Count; num5++)
									{
										Speech speech2 = list6[num4].Speeches[num5].GetSpeech();
										GameObject startGameObj3 = listField7.FindChild("EDITOR_Speeches/ListItem" + num5);
										AddConnection(instance, startGameObj3, speech2, vh, Color.black);
									}
								}
							}
						}
						else
						{
							Color col = Color.black;
							string text = null;
							switch (list6[num4].Type)
							{
							case StoryEventType.EnableTrigger:
							case StoryEventType.DisableTrigger:
							case StoryEventType.OneShotTrigger:
							case StoryEventType.FleeAndDisappear:
							case StoryEventType.OneShotTriggerWithParams:
								text = list6[num4].StringData;
								col = ScriptObjectBehaviour.TriggerCol;
								break;
							case StoryEventType.SpawnTemplate:
								text = list6[num4].StringData;
								col = ScriptObjectBehaviour.TemplateCol;
								break;
							case StoryEventType.ActivateInvader:
								text = list6[num4].StringData;
								col = ScriptObjectBehaviour.InvaderCol;
								break;
							case StoryEventType.DiscoverQuest:
								text = list6[num4].StringData;
								col = ScriptObjectBehaviour.QuestCol;
								break;
							case StoryEventType.CompleteQuest:
							case StoryEventType.CompleteQuestIfItIsDiscovered:
								col = Color.green;
								text = list6[num4].StringData;
								break;
							case StoryEventType.FailQuest:
							case StoryEventType.FailQuestIfItIsDiscovered:
								col = Color.red;
								text = list6[num4].StringData;
								break;
							case StoryEventType.SpawnEquipment:
								text = list6[num4].StringData2;
								col = ScriptObjectBehaviour.TriggerCol;
								break;
							}
							if (text != null)
							{
								BaseScriptObject target2 = GameImpl.Instance.GetCurrentlyEditingStory().FindScriptObjectByUniqueID(text);
								GameObject listField8 = component.GetListField(fieldInfo, num4);
								AddConnection(instance, listField8, target2, vh, col);
							}
						}
						if (list6[num4].Formulas != null && list6[num4].Formulas.Count > 0 && list6[num4].Formulas[0].GetConditionBlock() != null)
						{
							GameObject startGameObj4 = component.GetListField(fieldInfo, num4).FindChild("Formula");
							AddConnection(instance, startGameObj4, list6[num4].Formulas[0].GetConditionBlock(), vh, ScriptObjectBehaviour.ConditionBlockCol);
						}
					}
				}
				else if (fieldInfo.FieldType == typeof(List<TemplateChild>))
				{
					if (!(fieldInfo2.GetValue(component.ScriptObject) is List<TemplateChild> list7))
					{
						continue;
					}
					for (int num6 = 0; num6 < list7.Count; num6++)
					{
						if (!string.IsNullOrEmpty(list7[num6].UniqueID))
						{
							Template target3 = GameImpl.Instance.GetCurrentlyEditingStory().FindTemplateByUniqueID(list7[num6].UniqueID);
							GameObject listField9 = component.GetListField(fieldInfo, num6);
							AddConnection(instance, listField9, target3, vh, ScriptObjectBehaviour.TemplateCol);
						}
						if (list7[num6].MinFormula.GetConditionBlock() != null)
						{
							GameObject startGameObj5 = component.GetListField(fieldInfo, num6).FindChild("MinFormula");
							AddConnection(instance, startGameObj5, list7[num6].MinFormula.GetConditionBlock(), vh, ScriptObjectBehaviour.ConditionBlockCol);
						}
						if (list7[num6].MaxFormula.GetConditionBlock() != null)
						{
							GameObject startGameObj6 = component.GetListField(fieldInfo, num6).FindChild("MaxFormula");
							AddConnection(instance, startGameObj6, list7[num6].MaxFormula.GetConditionBlock(), vh, ScriptObjectBehaviour.ConditionBlockCol);
						}
					}
				}
				else
				{
					if (!(fieldInfo.Name == "TemplateID"))
					{
						continue;
					}
					string text2 = fieldInfo2.GetValue(component.ScriptObject) as string;
					if (!string.IsNullOrEmpty(text2))
					{
						Template template = GameImpl.Instance.GetCurrentlyEditingStory().FindTemplateByUniqueID(text2);
						if (template != null)
						{
							GameObject startGameObj7 = component.gameObject.FindChild("EDITOR_TemplateID");
							AddConnection(instance, startGameObj7, template, vh, ScriptObjectBehaviour.TemplateCol);
						}
					}
				}
			}
		}
	}

	private void AddConnection(ScriptEditor scriptEditor, GameObject startGameObj, BaseScriptObject target, VertexHelper vh, Color col)
	{
		ScriptObjectBehaviour scriptObjectBehaviour = scriptEditor.GetScriptObjectBehaviour(target);
		if (!(startGameObj != null) || !(scriptObjectBehaviour != null))
		{
			return;
		}
		RectTransform rectTransform = (RectTransform)startGameObj.transform;
		RectTransform rectTransform2 = (RectTransform)scriptObjectBehaviour.transform;
		Vector2 vector = MathUtil.ToXY(base.transform.worldToLocalMatrix.MultiplyPoint(rectTransform.TransformPoint(0f, 0f, 0f)));
		bool flag = MathUtil.ToXY(base.transform.worldToLocalMatrix.MultiplyPoint(rectTransform2.TransformPoint(rectTransform2.sizeDelta.x * 0.5f, 0f, 0f))).x < vector.x;
		Vector2 vector2 = MathUtil.ToXY(base.transform.worldToLocalMatrix.MultiplyPoint(rectTransform.TransformPoint((flag ? (-1f) : 1f) * rectTransform.sizeDelta.x * 0.5f, 0f, 0f)));
		Vector2 vector3 = MathUtil.ToXY(base.transform.worldToLocalMatrix.MultiplyPoint(rectTransform2.TransformPoint(flag ? rectTransform2.sizeDelta.x : 0f, (0f - rectTransform2.sizeDelta.y) * 0.5f, 0f)));
		float num = (vector3 - vector2).magnitude * 0.25f;
		CubicBezier2D cubicBezier2D = default(CubicBezier2D);
		cubicBezier2D.p0 = vector2;
		cubicBezier2D.p1 = vector2 + new Vector2(flag ? (-1f) : 1f, 0f) * num;
		cubicBezier2D.p2 = vector3 + new Vector2(flag ? 1f : (-1f), 0f) * num;
		cubicBezier2D.p3 = vector3;
		UIVertex simpleVert = UIVertex.simpleVert;
		for (int i = 0; i <= 15; i++)
		{
			float t = (float)i / 15f;
			Vector2 vector4 = cubicBezier2D.EvaluatePos(t);
			Vector2 vector5 = MathUtil.RightNormal(cubicBezier2D.EvaluateDir(t));
			simpleVert.position = vector4 - vector5 * 2f;
			simpleVert.color = col;
			vh.AddVert(simpleVert);
			simpleVert.position = vector4 + vector5 * 2f;
			simpleVert.color = col;
			vh.AddVert(simpleVert);
			if (i > 0)
			{
				int num2 = vh.currentVertCount - 4;
				vh.AddTriangle(num2, num2 + 1, num2 + 2);
				vh.AddTriangle(num2 + 2, num2 + 3, num2 + 1);
			}
		}
	}
}
