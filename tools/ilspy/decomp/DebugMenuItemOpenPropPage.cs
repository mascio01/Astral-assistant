using UnityEngine;

public class DebugMenuItemOpenPropPage : DebugMenuItem
{
	private PropSpawner.Category Cat;

	public DebugMenuItemOpenPropPage(PropSpawner.Category cat)
		: base(cat.Name)
	{
		Cat = cat;
	}

	public override void Update(Vector2 pos)
	{
		if (GUIButton(new Rect(pos.x, pos.y, 250f, 30f), Name))
		{
			PropSpawner page = new PropSpawner(Cat);
			ParentPage.OpenChildPage(page);
		}
	}
}
