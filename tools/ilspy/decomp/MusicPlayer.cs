using UnityEngine;

public class MusicPlayer
{
	public string SongName;

	public MusicSituation MusicSituation;

	public MusicGroup MusicGroup;

	public float Transition;

	public float BuildUpTime;

	public float Time;

	public float TimePlayedWhileNotDesired;

	public float TimePaused;

	public bool IsFinished;

	public bool IsPaused;

	public bool WasEverPaused;

	public AudioSource UnityAudioSource;
}
