using UnityEngine;

public class CommunityStatNotification
{
	public CommunityStatType CommunityStatType;

	public Community Community;

	public bool Finished;

	public float DisplayedTime;

	public float Transition;

	public float StartAmount;

	public CommunityStatNotification(CommunityStatType communityStatType, Community community, float startAmount)
	{
		CommunityStatType = communityStatType;
		Community = community;
		StartAmount = startAmount;
	}

	public bool IsFinished()
	{
		if (Finished)
		{
			return Transition == 0f;
		}
		return false;
	}

	public void UpdateCommunityStatNotification()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		RectTransform rectTransform = (RectTransform)instance.UnityCommunityStatNotification.transform;
		CommunityStatBehaviour unityCommunityStatNotificationStat = instance.UnityCommunityStatNotificationStat;
		if (DisplayedTime == 0f)
		{
			if (StoryManager.Instance.GetMostInterestingSpeaker() != null || HudBehaviour.Instance.IsShowingPipSpeechBubble() || Notification.IsInCombat() || NotificationManager.Instance.IsDisplayingNotification() || GameImpl.Instance.IsDialogOpen())
			{
				return;
			}
			SoundManager.PlayMenuSoundFromList(SoundManager.NotificationSounds);
			unityCommunityStatNotificationStat.Initialize(CommunityStatType, Community);
		}
		float displayedTime = DisplayedTime;
		DisplayedTime += Time.unscaledDeltaTime;
		if (DisplayedTime >= 8f)
		{
			Finished = true;
		}
		Transition = Mathf.Clamp(Transition + Time.unscaledDeltaTime * (Finished ? (-1f) : 1f) * 4f, 0f, 1f);
		instance.UnityCommunityStatNotification.gameObject.SetActive(Transition > 0f);
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, Mathf.Lerp(128f, -64f, Mathf.Clamp01(Transition - InfoScreen.Instance.Transition)));
		float num = StartAmount;
		if (DisplayedTime >= 1f)
		{
			if (displayedTime < 1f)
			{
				SoundManager.PlayMenuSound(SoundManager.SuccessSound, 0.25f);
			}
			num = Mathf.Lerp(StartAmount, unityCommunityStatNotificationStat.Current, Mathf.Clamp01(DisplayedTime - 1f));
		}
		unityCommunityStatNotificationStat.UnityProgressBar.SetValue(num / unityCommunityStatNotificationStat.Total);
	}
}
