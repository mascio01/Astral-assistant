using System;
using System.Collections.Generic;
using UnityEngine;

public class DebugMenuItemOpenPage : DebugMenuItem
{
	private static List<DebugPage> PreviouslyOpenedPages = new List<DebugPage>();

	private Type PageType;

	public static void ClearOldMenus()
	{
		PreviouslyOpenedPages.Clear();
	}

	public DebugMenuItemOpenPage(string name, Type type)
		: base(name)
	{
		PageType = type;
	}

	public override void Update(Vector2 pos)
	{
		if (!GUIButton(new Rect(pos.x, pos.y, 250f, 30f), Name))
		{
			return;
		}
		DebugPage debugPage = null;
		foreach (DebugPage previouslyOpenedPage in PreviouslyOpenedPages)
		{
			if (previouslyOpenedPage.GetType() == PageType)
			{
				debugPage = previouslyOpenedPage;
			}
		}
		if (debugPage == null)
		{
			debugPage = (DebugPage)Activator.CreateInstance(PageType);
			PreviouslyOpenedPages.Add(debugPage);
		}
		ParentPage.OpenChildPage(debugPage);
	}
}
