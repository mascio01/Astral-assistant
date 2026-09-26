using UnityEngine;

public class DebugMenuItemCustom : DebugMenuItem
{
	private DebugMenuItemSelected SelectedEvent;

	private Texture2D Tex;

	private Color Tint = Color.white;

	private string ExtraText;

	private bool AffectsGameState;

	public DebugMenuItemCustom(string name, DebugMenuItemSelected selectedEvent, bool affectsGameState = false)
		: base(name)
	{
		SelectedEvent = selectedEvent;
		AffectsGameState = affectsGameState;
	}

	public DebugMenuItemCustom(string name, DebugMenuItemSelected selectedEvent, string extraText, bool affectsGameState = false)
		: base(name)
	{
		SelectedEvent = selectedEvent;
		ExtraText = extraText;
		AffectsGameState = affectsGameState;
	}

	public DebugMenuItemCustom(string name, DebugMenuItemSelected selectedEvent, Texture2D tex, Color tint, bool affectsGameState = false)
		: base(name)
	{
		SelectedEvent = selectedEvent;
		Tex = tex;
		Tint = tint;
		AffectsGameState = affectsGameState;
	}

	public override void Update(Vector2 pos)
	{
		if (GUIButton(new Rect(pos.x, pos.y, 250f, 30f), Name))
		{
			SelectedEvent();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
		pos.x += 210f;
		if (Tex != null)
		{
			GUI.color = Tint;
			GUI.DrawTexture(new Rect(pos.x, pos.y - 5f, 40f, 40f), Tex);
			GUI.color = Color.white;
			pos.x += 50f;
		}
		if (!string.IsNullOrEmpty(ExtraText))
		{
			GUI.Label(new Rect(pos.x, pos.y, 1000f, 30f), ExtraText);
		}
	}
}
