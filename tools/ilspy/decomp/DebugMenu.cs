using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenu : DebugPage
{
	protected List<DebugMenuItem> Items = new List<DebugMenuItem>();

	public Vector2 ScrollPosition = Vector2.zero;

	public Vector2 ScrollViewPos = Vector2.zero;

	protected DebugMenu(string name)
		: base(name)
	{
	}

	public override void OnGUIImpl()
	{
		base.OnGUIImpl();
		float num = Math.Min((float)Screen.width - 20f, 1800f);
		int num2 = MathUtil.Clamp(Mathf.FloorToInt(((float)Screen.height - 40f - 20f) / 40f), 5, 20);
		GUI.Box(new Rect(10f, 10f, num + 40f, (float)Math.Min(Items.Count, num2) * 40f + 40f), TitleString);
		Vector2 pos = new Vector2(30f, 40f);
		bool flag = Items.Count > num2;
		if (flag)
		{
			ScrollPosition = GUIBeginScrollView(new Rect(pos.x, pos.y, num, (float)num2 * 40f), ScrollPosition, new Rect(0f, 0f, num, (float)Items.Count * 40f), alwaysShowHorizontal: false, alwaysShowVertical: true);
			pos = Vector2.zero;
		}
		for (int i = 0; i < Items.Count; i++)
		{
			Items[i].ParentPage = this;
			Items[i].Update(pos);
			pos.y += 40f;
		}
		if (flag)
		{
			GUI.EndScrollView();
		}
	}

	public Vector2 GUIBeginScrollView(Rect position, Vector2 scrollPosition, Rect viewRect, bool alwaysShowHorizontal, bool alwaysShowVertical)
	{
		ScrollViewPos = position.min;
		return GUI.BeginScrollView(position, scrollPosition, viewRect, alwaysShowHorizontal, alwaysShowVertical);
	}

	public override void DeactivateImpl()
	{
		foreach (DebugMenuItem item in Items)
		{
			item.OnDeactivate();
		}
		base.DeactivateImpl();
	}
}
