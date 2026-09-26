using System.Collections.Generic;
using UnityEngine;

public class BirdType
{
	public string Name;

	public List<Resource<AudioClip>> BirdCalls = new List<Resource<AudioClip>>();

	public int TimeOfDayMask;

	public int SeasonMask;

	public Distribution Distribution;

	public AnimationCurve TimeOfDayCurve;

	public AnimationCurve SeasonCurve;

	public float Volume;
}
