using UnityEngine;

public class DebugMenuItemToggle : DebugMenuItem
{
	private DebugMenuBoolGet Getter;

	private DebugMenuBoolSet Setter;

	private Texture2D Tex;

	private bool AffectsGameState;

	public DebugMenuItemToggle(string name, DebugMenuBoolGet getter, DebugMenuBoolSet setter, bool affectsGameState = false)
		: base(name)
	{
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
	}

	public DebugMenuItemToggle(string name, DebugMenuBoolGet getter, DebugMenuBoolSet setter, Texture2D tex, bool affectsGameState = false)
		: base(name)
	{
		Getter = getter;
		Setter = setter;
		Tex = tex;
		AffectsGameState = affectsGameState;
	}

	public override void Update(Vector2 pos)
	{
		bool flag = Getter();
		if (Tex != null)
		{
			GUI.DrawTexture(new Rect(pos.x, pos.y - 10f, 30f, 30f), Tex);
			pos.x += 40f;
		}
		bool flag2 = GUIToggle(new Rect(pos.x, pos.y, 250f, 30f), flag, Name);
		if (flag2 != flag)
		{
			Setter(flag2);
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}
