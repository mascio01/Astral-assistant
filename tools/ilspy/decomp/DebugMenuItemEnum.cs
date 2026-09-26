using System;
using UnityEngine;

public class DebugMenuItemEnum<T> : DebugMenuItem where T : Enum
{
	public delegate T GetterFunc();

	public delegate void SetterFunc(T val);

	private GetterFunc Getter;

	private SetterFunc Setter;

	private T CurVal;

	private T MaxVal;

	private string DisplayString;

	private const string Left = "<";

	private const string Right = ">";

	public DebugMenuItemEnum(string name, T maxVal, GetterFunc getter, SetterFunc setter)
		: base(name)
	{
		Getter = getter;
		Setter = setter;
		MaxVal = maxVal;
		BuildDisplayString();
	}

	public override void Update(Vector2 pos)
	{
		int num = Convert.ToInt32(Getter());
		int num2 = 0;
		int num3 = Convert.ToInt32(MaxVal);
		if (num != Convert.ToInt32(CurVal))
		{
			BuildDisplayString();
		}
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		if (GUIButton(new Rect(pos.x + 250f, pos.y, 30f, 30f), "<"))
		{
			Setter((T)Enum.ToObject(typeof(T), Math.Max(Convert.ToInt32(CurVal) - 1, num2)));
			BuildDisplayString();
		}
		int num4 = (int)GUIHorizontalSlider(new Rect(pos.x + 250f + 70f, pos.y, 200f, 30f), num, num2, num3);
		if (num4 != num)
		{
			Setter((T)Enum.ToObject(typeof(T), num4));
			BuildDisplayString();
		}
		if (GUIButton(new Rect(pos.x + 250f + 290f, pos.y, 30f, 30f), ">"))
		{
			Setter((T)Enum.ToObject(typeof(T), Math.Min(Convert.ToInt32(CurVal) + 1, num3)));
			BuildDisplayString();
		}
	}

	private void BuildDisplayString()
	{
		CurVal = Getter();
		DisplayString = Name + ": " + CurVal.ToString();
	}
}
