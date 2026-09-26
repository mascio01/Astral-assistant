using System;
using UnityEngine;

public struct AggroTile
{
	public TimeSpan AggroTime;

	public float GetSongProbability()
	{
		if (AggroTime == TimeSpan.Zero)
		{
			return 1f;
		}
		return Mathf.Clamp01(((float)(Session.Instance.PlayTime - AggroTime).TotalSeconds - BirdSongManager.TimeToStartSingingAfterAggro) / (BirdSongManager.TimeToFullSingingAfterAggro - BirdSongManager.TimeToStartSingingAfterAggro));
	}
}
