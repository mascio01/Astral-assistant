using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnconsciousBehaviour : MonoBehaviour
{
	private class Star
	{
		public RawImage UnityStar;

		public float Transition;

		public float Scale;

		public float Dist;

		public float Angle;

		public float Height;

		public float Speed;
	}

	private int WantNumStars;

	private List<Star> Stars = new List<Star>();

	private Character Owner;

	private static Color[] Colors = new Color[3]
	{
		new Color(1f, 0.25f, 0.25f),
		new Color(0.25f, 0.38f, 1f),
		new Color(1f, 0.75f, 0.25f)
	};

	public static float MinHeight = 0.2f;

	public static float MaxHeight = 0.3f;

	public static float MinDist = 0.2f;

	public static float MaxDist = 0.3f;

	public static float MinSpeed = 0.5f;

	public static float MaxSpeed = 1f;

	public static float MinScale = 0.05f;

	public static float MaxScale = 0.1f;

	public void SetWantNumStars(Character character, int n)
	{
		Owner = character;
		WantNumStars = n;
	}

	public bool WantToBeDeleted()
	{
		if (Stars.Count == 0)
		{
			return WantNumStars == 0;
		}
		return false;
	}

	public void Update()
	{
		while (Stars.Count < WantNumStars)
		{
			RawImage component = UnityEngine.Object.Instantiate(Character.UnconsciousStar.GetAsset(), base.transform, worldPositionStays: false).GetComponent<RawImage>();
			component.color = Colors[MathUtil.NonDeterministicRand.Next() % Colors.Length];
			Owner.UnityUpdateCanvasList();
			Star star = new Star();
			star.UnityStar = component;
			star.Angle = MathUtil.NonDeterministicRand.RandomFloat() * (MathF.PI * 2f);
			star.Height = MathUtil.NonDeterministicRand.RandomFloat();
			star.Scale = MathUtil.NonDeterministicRand.RandomFloat();
			star.Dist = MathUtil.NonDeterministicRand.RandomFloat();
			star.Speed = MathUtil.NonDeterministicRand.RandomFloat();
			star.Transition = 0.01f;
			Stars.Add(star);
		}
		Transform transform = HudBehaviour.Instance.UnityGameCamera.transform;
		for (int num = Stars.Count - 1; num >= 0; num--)
		{
			Star star2 = Stars[num];
			star2.Transition = Mathf.Clamp01(star2.Transition + ((num < WantNumStars) ? 1f : (-1f)) * Time.deltaTime);
			if (star2.Transition > 0f)
			{
				star2.UnityStar.transform.position = base.transform.position + new Vector3(0f, Mathf.Lerp(MinHeight, MaxHeight, star2.Height), 0f) + MathUtil.ToX0Y(MathUtil.GetDirFromAngle(star2.Angle + Time.time * (MathF.PI * 2f) * Mathf.Lerp(MinSpeed, MaxSpeed, star2.Speed))) * Mathf.Lerp(MinDist, MaxDist, star2.Dist);
				star2.UnityStar.transform.rotation = transform.rotation;
				star2.UnityStar.transform.localScale = Vector3.one * Mathf.Lerp(MinScale, MaxScale, star2.Scale) * star2.Transition;
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(Stars[num].UnityStar.gameObject);
				Owner.UnityUpdateCanvasList();
				Stars.RemoveAt(num);
			}
		}
	}
}
