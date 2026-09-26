using System.Collections.Generic;

public class BirdSongDebugMenu : DebugMenu
{
	public static List<BirdCallLog> Log = new List<BirdCallLog>();

	public BirdSongDebugMenu()
		: base(GameImpl.Translate("DEBUG_BirdSongDebug"))
	{
	}

	public override void HandleInputImpl(InputFrame inputFrame)
	{
		base.HandleInputImpl(inputFrame);
		Items.Clear();
		for (int num = Log.Count - 1; num >= 0; num--)
		{
			Items.Add(new DebugMenuItemText(Log[num].Name + Log[num].Index, " countdown: " + Log[num].Count + " time: " + Log[num].Time));
		}
	}

	public static void LogBirdCall(string name, int index, int count)
	{
		while (Log.Count > 20)
		{
			Log.RemoveAt(0);
		}
		BirdCallLog item = new BirdCallLog
		{
			Name = name,
			Index = index,
			Count = count,
			Time = Session.Instance.PlayTime
		};
		Log.Add(item);
	}
}
