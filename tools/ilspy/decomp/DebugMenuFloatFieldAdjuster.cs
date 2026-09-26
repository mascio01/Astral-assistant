using System;
using System.Reflection;
using UnityEngine;

public class DebugMenuFloatFieldAdjuster : DebugMenuItem
{
	private float MinVal;

	private float MaxVal;

	private float CurVal;

	private string DisplayString;

	private FieldInfo FieldInfo;

	private object Obj;

	private bool AffectsGameState;

	public DebugMenuFloatFieldAdjuster(string name, float min, float max, Type type, string fieldName, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxVal = max;
		FieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (FieldInfo == null)
		{
			Debug.LogError("Field not found: " + fieldName);
		}
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuFloatFieldAdjuster(string name, float min, float max, object obj, string fieldName, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxVal = max;
		Obj = obj;
		FieldInfo = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (FieldInfo == null)
		{
			Debug.LogError("Field not found: " + fieldName);
		}
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		if ((float)FieldInfo.GetValue(Obj) != CurVal)
		{
			BuildDisplayString();
		}
		float num = GUIHorizontalSlider(new Rect(pos.x + 250f + 20f, pos.y, 200f, 30f), CurVal, MinVal, MaxVal);
		if (num != CurVal)
		{
			FieldInfo.SetValue(Obj, num);
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	private void BuildDisplayString()
	{
		CurVal = (float)FieldInfo.GetValue(Obj);
		DisplayString = Name + ": " + CurVal.ToString("N2");
	}
}
