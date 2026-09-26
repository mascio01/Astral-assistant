using System;
using System.Reflection;
using UnityEngine;

public class DebugMenuItemToggleField : DebugMenuItem
{
	private bool CurVal;

	private string DisplayString;

	private FieldInfo FieldInfo;

	private object Obj;

	private bool AffectsGameState;

	public DebugMenuItemToggleField(string name, Type type, string fieldName, bool affectsGameState = false)
		: base(name)
	{
		FieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (FieldInfo == null)
		{
			Debug.LogError("Field not found: " + fieldName);
		}
		CurVal = (bool)FieldInfo.GetValue(Obj);
		AffectsGameState = affectsGameState;
	}

	public DebugMenuItemToggleField(string name, object obj, string fieldName, bool affectsGameState = false)
		: base(name)
	{
		Obj = obj;
		FieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (FieldInfo == null)
		{
			Debug.LogError("Field not found: " + fieldName);
		}
		CurVal = (bool)FieldInfo.GetValue(Obj);
		AffectsGameState = affectsGameState;
	}

	public override void Update(Vector2 pos)
	{
		CurVal = (bool)FieldInfo.GetValue(Obj);
		bool flag = GUIToggle(new Rect(pos.x, pos.y, 250f, 30f), CurVal, Name);
		if (flag != CurVal)
		{
			FieldInfo.SetValue(Obj, flag);
			CurVal = flag;
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}
