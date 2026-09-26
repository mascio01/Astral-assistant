using System;
using UnityEngine;

public class DebugMenuIntAdjuster : DebugMenuItem
{
	private int MinVal;

	private int MaxVal;

	private int CurVal;

	private DebugMenuIntGet Getter;

	private DebugMenuIntSet Setter;

	private DebugMenuIntGet MinGetter;

	private DebugMenuIntGet MaxGetter;

	private string DisplayString;

	private bool AffectsGameState;

	private const string Left = "<";

	private const string Right = ">";

	public DebugMenuIntAdjuster(string name, int min, int max, DebugMenuIntGet getter, DebugMenuIntSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxVal = max;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuIntAdjuster(string name, int min, DebugMenuIntGet maxGetter, DebugMenuIntGet getter, DebugMenuIntSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxGetter = maxGetter;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuIntAdjuster(string name, DebugMenuIntGet minGetter, int max, DebugMenuIntGet getter, DebugMenuIntSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinGetter = minGetter;
		MaxVal = max;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuIntAdjuster(string name, DebugMenuIntGet minGetter, DebugMenuIntGet maxGetter, DebugMenuIntGet getter, DebugMenuIntSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinGetter = minGetter;
		MaxGetter = maxGetter;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public override void Update(Vector2 pos)
	{
		int num = Getter();
		int num2 = ((MinGetter != null) ? MinGetter() : MinVal);
		int num3 = ((MaxGetter != null) ? MaxGetter() : MaxVal);
		if (num != CurVal)
		{
			BuildDisplayString();
		}
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		if (GUIButton(new Rect(pos.x + 250f + 20f, pos.y, 30f, 30f), "<"))
		{
			Setter(Math.Max(Getter() - 1, num2));
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
		int num4 = (int)GUIHorizontalSlider(new Rect(pos.x + 250f + 70f, pos.y, 200f, 30f), num, num2, num3);
		if (num4 != num)
		{
			Setter(num4);
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
		if (GUIButton(new Rect(pos.x + 250f + 290f, pos.y, 30f, 30f), ">"))
		{
			Setter(Math.Min(Getter() + 1, num3));
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	private void BuildDisplayString()
	{
		CurVal = Getter();
		DisplayString = Name + ": " + CurVal;
	}
}
