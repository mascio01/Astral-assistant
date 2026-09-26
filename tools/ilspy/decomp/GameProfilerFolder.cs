using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GameProfilerFolder
{
	public static GameProfilerFolder Root = new GameProfilerFolder("GameProfilers");

	public string Name;

	public List<GameProfilerFolder> SubFolders;

	public List<GameProfiler> GameProfilers;

	private static int Frames = 0;

	private static float LastResetTime = 0f;

	public GameProfilerFolder(string name)
	{
		Name = name;
		SubFolders = new List<GameProfilerFolder>();
		GameProfilers = new List<GameProfiler>();
	}

	public GameProfilerFolder GetSubFolder(string name)
	{
		foreach (GameProfilerFolder subFolder in SubFolders)
		{
			if (subFolder.Name == name)
			{
				return subFolder;
			}
		}
		return null;
	}

	public GameProfilerFolder GetOrCreateSubFolder(string name)
	{
		GameProfilerFolder gameProfilerFolder = GetSubFolder(name);
		if (gameProfilerFolder == null)
		{
			gameProfilerFolder = new GameProfilerFolder(name);
			SubFolders.Add(gameProfilerFolder);
		}
		return gameProfilerFolder;
	}

	public void PrintGameProfilers(StringBuilder sb, int loggedFrame)
	{
		sb.Length = 0;
		foreach (GameProfiler gameProfiler in GameProfilers)
		{
			gameProfiler.Print(sb, loggedFrame);
		}
	}

	public static void UpdateAll(int sessionFrame, bool wantLog)
	{
		Frames++;
		Root.LogAll(sessionFrame, wantLog);
		if (Time.unscaledTime - LastResetTime >= 1f)
		{
			Root.ResetAll();
			Frames = 0;
			LastResetTime = Time.unscaledTime;
		}
	}

	private void LogAll(int sessionFrame, bool wantLog)
	{
		foreach (GameProfilerFolder subFolder in SubFolders)
		{
			subFolder.LogAll(sessionFrame, wantLog);
		}
		foreach (GameProfiler gameProfiler in GameProfilers)
		{
			gameProfiler.Log(sessionFrame, wantLog);
		}
	}

	private void ResetAll()
	{
		foreach (GameProfilerFolder subFolder in SubFolders)
		{
			subFolder.ResetAll();
		}
		foreach (GameProfiler gameProfiler in GameProfilers)
		{
			gameProfiler.Reset(Frames);
		}
	}
}
