using UnityEngine;

public class DebugMenuString : DebugMenuItem
{
	private BaseObject Obj;

	private DebugMenuStringGet Getter;

	private DebugMenuStringSet Setter;

	private string CurVal;

	private bool HasChanged;

	private bool AffectsGameState;

	public float NameWidth = 250f;

	public DebugMenuString(string name, DebugMenuStringGet getter, DebugMenuStringSet setter, bool affectsGameState = false)
		: base(name)
	{
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
	}

	public override void OnDeactivate()
	{
		HasChanged = false;
		base.OnDeactivate();
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, NameWidth, 30f), Name);
		string text = Getter();
		if (!HasChanged)
		{
			CurVal = text;
		}
		CurVal = GUI.TextField(new Rect(pos.x + NameWidth, pos.y, 200f, 30f), CurVal);
		if (CurVal != text)
		{
			HasChanged = true;
			Setter(CurVal);
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}
}
