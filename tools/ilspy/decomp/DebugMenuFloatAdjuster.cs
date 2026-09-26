using UnityEngine;

public class DebugMenuFloatAdjuster : DebugMenuItem
{
	private float MinVal;

	private float MaxVal;

	private float CurVal;

	private DebugMenuFloatGet Getter;

	private DebugMenuFloatSet Setter;

	private DebugMenuFloatGet MinGetter;

	private DebugMenuFloatGet MaxGetter;

	private string DisplayString;

	private bool AffectsGameState;

	public DebugMenuFloatAdjuster(string name, float min, float max, DebugMenuFloatGet getter, DebugMenuFloatSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxVal = max;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuFloatAdjuster(string name, float min, DebugMenuFloatGet maxGetter, DebugMenuFloatGet getter, DebugMenuFloatSet setter, bool affectsGameState = false)
		: base(name)
	{
		MinVal = min;
		MaxGetter = maxGetter;
		Getter = getter;
		Setter = setter;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public DebugMenuFloatAdjuster(string name, DebugMenuFloatGet minGetter, DebugMenuFloatGet maxGetter, DebugMenuFloatGet getter, DebugMenuFloatSet setter, bool affectsGameState = false)
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
		float num = Getter();
		if (num != CurVal)
		{
			BuildDisplayString();
		}
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		float num2 = GUIHorizontalSlider(new Rect(pos.x + 250f + 20f, pos.y, 200f, 30f), num, (MinGetter != null) ? MinGetter() : MinVal, (MaxGetter != null) ? MaxGetter() : MaxVal);
		if (num2 != num)
		{
			Setter(num2);
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
		DisplayString = Name + ": " + CurVal.ToString("N2");
	}
}
