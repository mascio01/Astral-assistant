using System.Collections.Generic;
using UnityEngine;

public class MusicGroup
{
	public MusicSituation MusicSituation;

	public List<string> Names = new List<string>();

	public Resource<AudioClip> Res;

	public float FadeInAtStartTime = 0.01f;

	public float FadeInFromPausedTime = 2f;

	public float FadeOutTime = 2f;

	public float MinTimeBetweenPlays;

	public float MaxTimePlayedWhileNotDesired;

	public float MinBuildUpTime;

	public float MaxBuildUpTime;

	public float MaxPausedTime = 20f;

	public float LastPlayedTime = -100000f;

	public float Volume = 1f;

	public void LoadMusicResource(string songName)
	{
		Res = new Resource<AudioClip>(MusicManager.MusicDir + songName);
	}

	public void UnloadMusicResource()
	{
		Res.UnloadResource();
		Res = null;
	}
}
