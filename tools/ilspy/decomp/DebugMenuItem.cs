using UnityEngine;

public class DebugMenuItem
{
	public DebugMenu ParentPage;

	public string Name;

	public const float ButtonWidth = 250f;

	public DebugMenuItem(string name)
	{
		Name = name;
	}

	public virtual void Update(Vector2 pos)
	{
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDeactivate()
	{
	}

	public bool GUIButton(Rect position, string text)
	{
		return GUI.Button(position, text);
	}

	public bool GUIToggle(Rect position, bool val, string text)
	{
		val = GUI.Toggle(position, val, text);
		return val;
	}

	public float GUIHorizontalSlider(Rect position, float val, float minVal, float maxVal)
	{
		val = GUI.HorizontalSlider(position, val, minVal, maxVal);
		return val;
	}
}
