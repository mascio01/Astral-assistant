using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public class ConditionBlock : BaseScriptObject
{
	[XmlAttribute]
	[DefaultValue(ConditionBlockType.And)]
	public ConditionBlockType Type;

	public List<Condition> Conditions;

	public override void FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		if (Conditions != null)
		{
			for (int i = 0; i < Conditions.Count; i++)
			{
				Conditions[i] = Conditions[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (Conditions != null && Conditions.Count == 0)
		{
			Conditions = null;
		}
	}

	public float Evaluate(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		switch (Type)
		{
		case ConditionBlockType.And:
			if (!StoryManager.AreAllConditionsSatisfied(Conditions, actor, target, obj, param))
			{
				return 0f;
			}
			return 1f;
		case ConditionBlockType.Or:
			if (!StoryManager.AreAnyConditionsSatisfied(Conditions, actor, target, obj, param))
			{
				return 0f;
			}
			return 1f;
		case ConditionBlockType.Sum:
			return StoryManager.GetConditionsSum(Conditions, actor, target, obj, param);
		case ConditionBlockType.SumRoundedUp:
			return Mathf.Ceil(StoryManager.GetConditionsSum(Conditions, actor, target, obj, param));
		case ConditionBlockType.SumRoundedDown:
			return Mathf.Floor(StoryManager.GetConditionsSum(Conditions, actor, target, obj, param));
		case ConditionBlockType.Min:
			return StoryManager.GetConditionsMin(Conditions, actor, target, obj, param);
		case ConditionBlockType.Max:
			return StoryManager.GetConditionsMax(Conditions, actor, target, obj, param);
		case ConditionBlockType.Multiply:
			return StoryManager.GetConditionsMultiply(Conditions, actor, target, obj, param);
		default:
			return 0f;
		}
	}

	public static bool IsNumericFormula(ConditionBlockType conditionBlockType)
	{
		if ((uint)(conditionBlockType - 2) <= 5u)
		{
			return true;
		}
		return false;
	}
}
