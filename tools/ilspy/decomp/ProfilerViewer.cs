public class ProfilerViewer : DebugMenu
{
	private GameProfilerFolder Folder;

	private int LoggedFrame;

	public ProfilerViewer(DebugPage parent, GameProfilerFolder profilerFolder, int loggedFrame)
		: base(profilerFolder.Name)
	{
		Folder = profilerFolder;
		LoggedFrame = loggedFrame;
		foreach (GameProfilerFolder subFolder in Folder.SubFolders)
		{
			GameProfilerFolder localSubFolder = subFolder;
			Items.Add(new DebugMenuItemCustom(localSubFolder.Name, delegate
			{
				OpenChildPage(new ProfilerViewer(this, localSubFolder, LoggedFrame));
			}));
		}
		foreach (GameProfiler gameProfiler in Folder.GameProfilers)
		{
			Items.Add(new DebugMenuItemProfiler(gameProfiler, LoggedFrame));
		}
	}
}
