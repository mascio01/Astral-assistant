using System;
using TMPro;
using UnityEngine;

public class SpeechBubbleBehaviour : MonoBehaviour
{
	public float Transition;

	public int LastFrame;

	public TimeSpan SpeechStartTime;

	public Speech Speaking;

	private string Text;

	private string TextEnglish;

	private TextMeshProUGUI TextMesh;

	public Vector3 InitialScale;

	public float Scale = 1f;

	public bool IsPipBubble;

	public Vector2 InitialAnchoredPosition;

	private static float PipBubbleScreenTopBorder = 8f;

	private void Awake()
	{
		TextMesh = base.transform.Find("Background/Speech").GetComponent<TextMeshProUGUI>();
		InitialScale = base.transform.localScale;
		InitialAnchoredPosition = ((RectTransform)base.transform).anchoredPosition;
	}

	public bool IsFinished()
	{
		return Transition == 0f;
	}

	public void Update()
	{
		Session instance = Session.Instance;
		Transition = Mathf.Clamp(Transition + ((instance != null && LastFrame >= Time.frameCount - 1 && SpeechStartTime < instance.PlayTime) ? 1f : (-1f)) * Time.unscaledDeltaTime * 4f, 0f, 1f);
		float num = ((instance != null) ? Mathf.Clamp((float)((instance.PlayTime - SpeechStartTime).TotalSeconds / (double)StoryManager.GetSpeechLipsMoveTime(TextEnglish)), 0f, 1f) : 1f);
		if (instance != null && instance.PlaySpeed == PlaySpeed.Paused)
		{
			num = 1f;
		}
		TextMesh.maxVisibleCharacters = (int)(num * (float)Text.Length);
	}

	public void LateUpdate()
	{
		base.transform.localScale = new Vector3(Transition * InitialScale.x, Transition * InitialScale.y, InitialScale.z) * Scale;
		if (IsPipBubble)
		{
			HudBehaviour instance = HudBehaviour.Instance;
			RectTransform obj = (RectTransform)base.transform;
			float height = obj.rect.height;
			float height2 = ((RectTransform)instance.UnityStatusBar.transform).rect.height;
			float num = Math.Min(InitialAnchoredPosition.y, GameImpl.DefaultReferenceHeight - height2 * instance.StatusBarTransition - height - PipBubbleScreenTopBorder);
			obj.anchoredPosition = new Vector3(y: num + Math.Max(0f, instance.Size.y - GameImpl.DefaultReferenceHeight), x: obj.anchoredPosition.x);
		}
	}

	public void SetSpeaking(Speech speech, TimeSpan startTime, string speechText, string speechTextEnglish, bool forceUpdate)
	{
		if (Speaking != speech || SpeechStartTime != startTime || forceUpdate)
		{
			Speaking = speech;
			SpeechStartTime = startTime;
			Text = speechText;
			TextEnglish = speechTextEnglish;
			TextMesh.SetUnityText(Text);
		}
		LastFrame = Time.frameCount;
	}
}
