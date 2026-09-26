using UnityEngine;

public class DebugMenuColorSpectrumAdjuster : DebugMenuItem
{
	private float CurVal;

	private Color32[] Colors;

	private DebugMenuFloatGet Getter;

	private DebugMenuFloatSet Setter;

	private string DisplayString;

	public DebugMenuColorSpectrumAdjuster(string name, Color32[] colors, DebugMenuFloatGet getter, DebugMenuFloatSet setter)
		: base(name)
	{
		Colors = colors;
		Getter = getter;
		Setter = setter;
		DisplayString = name + ":";
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		for (int i = 0; i < Colors.Length; i++)
		{
			GUI.color = Colors[i];
			GUI.DrawTexture(new Rect(pos.x + 220f + 30f * (float)i, pos.y - 5f, 30f, 30f), (Texture2D)GameCursor.WhiteTex);
		}
		GUI.color = Color.white;
		float num = Getter();
		float num2 = GUIHorizontalSlider(new Rect(pos.x + 220f + 15f, pos.y, 30f * (float)(Colors.Length - 1), 30f), num, 0f, Colors.Length - 1);
		if (num2 != num)
		{
			Setter(num2);
		}
	}
}
