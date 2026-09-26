using System.Text;
using UnityEngine;

public class DebugMenuItemAdjuster : DebugMenuItem
{
	protected DebugMenuItemDecrement DecrementEvent;

	protected DebugMenuItemIncrement IncrementEvent;

	protected DebugMenuItemBuildValueString BuildValueStringEvent;

	private StringBuilder DisplayStringBuilder = new StringBuilder(50);

	private string DisplayString;

	private bool AffectsGameState;

	private const string Left = "<";

	private const string Right = ">";

	public DebugMenuItemAdjuster(string name, bool affectsGameState = false)
		: base(name)
	{
		AffectsGameState = affectsGameState;
	}

	public DebugMenuItemAdjuster(string name, DebugMenuItemDecrement decrementEvent, DebugMenuItemIncrement incrementEvent, DebugMenuItemBuildValueString buildValueStringEvent, bool affectsGameState = false)
		: base(name)
	{
		DecrementEvent = decrementEvent;
		IncrementEvent = incrementEvent;
		BuildValueStringEvent = buildValueStringEvent;
		AffectsGameState = affectsGameState;
		BuildDisplayString();
	}

	public override void Update(Vector2 pos)
	{
		GUI.Label(new Rect(pos.x, pos.y, 250f, 30f), DisplayString);
		if (GUIButton(new Rect(pos.x + 250f, pos.y, 30f, 30f), "<"))
		{
			DecrementEvent();
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
		if (GUIButton(new Rect(pos.x + 250f + 30f, pos.y, 30f, 30f), ">"))
		{
			IncrementEvent();
			BuildDisplayString();
			if (AffectsGameState)
			{
				Session.Instance.AchievementsEnabled = false;
			}
		}
	}

	public void BuildDisplayString()
	{
		DisplayStringBuilder.Length = 0;
		DisplayStringBuilder.Append(Name);
		DisplayStringBuilder.Append(": ");
		if (BuildValueStringEvent != null)
		{
			BuildValueStringEvent(ref DisplayStringBuilder);
		}
		DisplayString = DisplayStringBuilder.ToString();
	}
}
