using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SnoreBehaviour : MonoBehaviour
{
	private struct Snore
	{
		public TextMeshProUGUI UnityText;

		public float Fade;

		public float Angle;
	}

	public static int HUD_Snore = StringUtil.JenkinsHash("HUD_Snore");

	public bool Snoring;

	private List<Snore> Snores = new List<Snore>();

	private float LastSnoreTime;

	private static float TimeBetweenSnores = 0.25f;

	private static float MinAngle = -1f;

	private static float MaxAngle = 0f;

	private static float Dist = 50f;

	private static float RotSpeed = 1f;

	private static Vector2 Offset = new Vector2(-0.25f, 0.25f);

	private void Update()
	{
		if (Snoring)
		{
			float unscaledTime = Time.unscaledTime;
			if (unscaledTime - LastSnoreTime >= TimeBetweenSnores)
			{
				LastSnoreTime = unscaledTime;
				Snore item = new Snore
				{
					UnityText = Object.Instantiate(Hud.Snore.GetAsset(), base.transform, worldPositionStays: false).GetComponent<TextMeshProUGUI>(),
					Fade = 0f,
					Angle = Mathf.Lerp(MinAngle, MaxAngle, MathUtil.NonDeterministicRand.RandomFloat())
				};
				Snores.Add(item);
			}
			RectTransform rectTransform = (RectTransform)base.transform;
			for (int i = 0; i < Snores.Count; i++)
			{
				Snore value = Snores[i];
				RectTransform obj = (RectTransform)value.UnityText.transform;
				obj.anchoredPosition = new Vector2((0f - rectTransform.rect.width) * 0.5f + rectTransform.rect.height * 0.5f + Offset.x * rectTransform.rect.height, Offset.y * rectTransform.rect.height) + MathUtil.GetDirFromAngle(value.Angle) * Dist * value.Fade;
				obj.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, value.Fade);
				value.UnityText.color = new Color(1f, 1f, 1f, 1f - value.Fade);
				value.Fade += Time.unscaledDeltaTime;
				value.Angle += Time.unscaledDeltaTime * RotSpeed;
				if (value.Fade >= 1f)
				{
					Object.Destroy(value.UnityText.gameObject);
					Snores.RemoveAt(i);
					i--;
				}
				else
				{
					Snores[i] = value;
				}
			}
		}
		else
		{
			for (int j = 0; j < Snores.Count; j++)
			{
				Object.Destroy(Snores[j].UnityText.gameObject);
			}
			Snores.Clear();
		}
	}
}
