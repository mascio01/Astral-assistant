using UnityEngine;

public class DebugMenuItemText : DebugMenuItem
{
	public string Text;

	public DebugMenuItemText(string name)
		: base(name)
	{
	}

	public DebugMenuItemText(string name, string text)
		: base(name)
	{
		Text = text;
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), Name);
		GUI.Label(new Rect(pos.x + 250f, pos.y, 1000f, 30f), Text);
	}
}
