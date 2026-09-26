using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager
{
	public static NotificationManager Instance;

	private List<Notification> Notifications = new List<Notification>();

	private List<EquipmentNotification> EquipmentNotifications = new List<EquipmentNotification>();

	private List<CommunityStatNotification> CommunityStatNotifications = new List<CommunityStatNotification>();

	public bool CheckCommunityStats;

	public bool InitCommunityStats;

	public bool SuppressNextAggroNotification;

	private int NewNotificationsShown;

	private float NewNotificationsScale;

	public float DesiredScreenDarkenAmount;

	private float[] LastCommunityStat = new float[5];

	public static int HUD_NewQuest = StringUtil.JenkinsHash("HUD_NewQuest");

	public static int HUD_QuestFailed = StringUtil.JenkinsHash("HUD_QuestFailed");

	public static int HUD_QuestCompleted = StringUtil.JenkinsHash("HUD_QuestCompleted");

	public static int HUD_ViewQuests = StringUtil.JenkinsHash("HUD_ViewQuests");

	public NotificationManager()
	{
		Instance = this;
	}

	public void Init()
	{
	}

	public void Unload()
	{
		Instance = null;
	}

	public void OnFinishedSession()
	{
		Notifications.Clear();
		EquipmentNotifications.Clear();
		CommunityStatNotifications.Clear();
		HudBehaviour.Instance.UnityNotification.gameObject.SetActive(value: false);
		HudBehaviour.Instance.UnityNewNotifications.gameObject.SetActive(value: false);
		HudBehaviour.Instance.UnityEquipmentNotification.gameObject.SetActive(value: false);
		HudBehaviour.Instance.UnityCommunityStatNotification.gameObject.SetActive(value: false);
		NewNotificationsScale = 0f;
		DesiredScreenDarkenAmount = 0f;
	}

	public void CloseCurrentNotification()
	{
		if (Notifications.Count > 0)
		{
			Notifications[0].Finished = true;
		}
	}

	public void AddNotification(Notification notification)
	{
		SoundManager.PlayMenuSoundFromList(SoundManager.NotificationSounds);
		foreach (Notification notification2 in Notifications)
		{
			notification2.OnNotificationAdded(notification);
		}
		bool becauseOfCombat = false;
		if (Notifications.Count == 0 && !notification.WantMinimised(out becauseOfCombat))
		{
			notification.Maximised = true;
		}
		Notifications.Add(notification);
	}

	public bool IsDisplayingNotification()
	{
		if (Notifications.Count > 0)
		{
			return Notifications[0].Maximised;
		}
		return false;
	}

	public bool IsDisplayingCommunityStat()
	{
		if (CommunityStatNotifications.Count > 0)
		{
			return CommunityStatNotifications[0].DisplayedTime > 0f;
		}
		return false;
	}

	public bool HasAnyNotificationsDisplayedOrQueued()
	{
		if (Notifications.Count <= 0)
		{
			return CommunityStatNotifications.Count > 0;
		}
		return true;
	}

	public void HandleInput(InputFrame inputFrame)
	{
		if (Notifications.Count > 0)
		{
			Notifications[0].HandleInput(inputFrame);
		}
	}

	public void Update()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		bool flag = false;
		if (Notifications.Count > 0)
		{
			bool becauseOfCombat = false;
			bool flag2 = Notifications[0].WantMinimised(out becauseOfCombat);
			if (becauseOfCombat && !Notifications[0].Maximised && Notifications[0].Transition == 0f)
			{
				for (int i = 1; i < Notifications.Count; i++)
				{
					Notification notification = Notifications[i];
					if (!notification.WantWaitUntilCombatFinished() && !notification.WantMinimised(out becauseOfCombat))
					{
						Notifications.RemoveAt(i);
						Notifications.Insert(0, notification);
						flag2 = false;
						break;
					}
				}
			}
			Notifications[0].Maximised = !flag2;
			flag = Notifications[0].Maximised;
			if (Notifications[0].Finished && !flag2 && Notifications.Count > 1)
			{
				flag = true;
			}
			Notifications[0].UpdateNotification();
			for (int num = Notifications.Count - 1; num >= 0; num--)
			{
				if (Notifications[num].IsFinished())
				{
					Notifications.RemoveAt(num);
				}
			}
		}
		DesiredScreenDarkenAmount = Mathf.Clamp01(DesiredScreenDarkenAmount + (flag ? 1f : (-1f)) * Notification.TransitionSpeed * Time.unscaledDeltaTime);
		int num2 = ((Notifications.Count > 0 && Notifications[0].Maximised) ? (Notifications.Count - 1) : Notifications.Count);
		NewNotificationsScale = Mathf.Clamp01(NewNotificationsScale + ((num2 > 0) ? 1f : (-1f)) * 8f * Time.unscaledDeltaTime);
		instance.UnityNewNotifications.SetActive(NewNotificationsScale > 0f && InfoScreen.Instance.Transition == 0f);
		instance.UnityNewNotifications.transform.localScale = Vector3.one * NewNotificationsScale;
		if (NewNotificationsShown != num2)
		{
			instance.UnityNewNotificationsAmount.SetUnityText(num2.ToString());
			NewNotificationsShown = num2;
		}
		if (instance.UnityNotification.activeSelf)
		{
			instance.UnityNotificationCloseButton.rectTransform.anchoredPosition = new Vector2(instance.UnityNewNotifications.gameObject.activeSelf ? 64f : 0f, instance.UnityNotificationCloseButton.rectTransform.anchoredPosition.y);
		}
		if (EquipmentNotifications.Count > 0)
		{
			EquipmentNotifications[0].UpdateEquipmentNotification();
			if (EquipmentNotifications[0].IsFinished())
			{
				EquipmentNotifications.RemoveAt(0);
			}
		}
		if ((CheckCommunityStats || InitCommunityStats) && !IsDisplayingCommunityStat() && Session.Instance != null && Session.Instance.State == SessionState.Started)
		{
			for (int j = 0; j < 5; j++)
			{
				CommunityStatType communityStatType = (CommunityStatType)j;
				if ((uint)(communityStatType - 2) > 1u)
				{
					instance.UnityCommunityStatNotificationStat.Initialize((CommunityStatType)j, Session.Instance.CommunityManager.PlayerCommunity);
					if (!InitCommunityStats && instance.UnityCommunityStatNotificationStat.Current != LastCommunityStat[j] && (!SuppressNextAggroNotification || j != 4) && (!(Session.Instance.DifficultySettings.GetTotalHunterDensity() <= 0f) || j != 4))
					{
						CommunityStatNotifications.Add(new CommunityStatNotification((CommunityStatType)j, Session.Instance.CommunityManager.PlayerCommunity, LastCommunityStat[j]));
					}
					LastCommunityStat[j] = instance.UnityCommunityStatNotificationStat.Current;
				}
			}
			CheckCommunityStats = false;
			InitCommunityStats = false;
			SuppressNextAggroNotification = false;
		}
		if (CommunityStatNotifications.Count > 0)
		{
			CommunityStatNotifications[0].UpdateCommunityStatNotification();
			if (CommunityStatNotifications[0].IsFinished())
			{
				CommunityStatNotifications.RemoveAt(0);
			}
		}
	}

	public void AddEquipmentNotification(TileObject giver, TileObject taker, Equipment item, int amount)
	{
		Community playerCommunity = Session.Instance.CommunityManager.PlayerCommunity;
		Community community = giver?.GetCommunity();
		Community community2 = taker?.GetCommunity();
		if (community != playerCommunity && community2 != playerCommunity)
		{
			return;
		}
		if ((giver == null || giver is CraftingProp) && community2 == playerCommunity)
		{
			Character playerCharacter = Session.Instance.GetLocalPlayerRecord().PlayerCharacter;
			if (taker != playerCharacter)
			{
				return;
			}
		}
		else if (community == playerCommunity && taker == null)
		{
			Character playerCharacter2 = Session.Instance.GetLocalPlayerRecord().PlayerCharacter;
			if (giver != playerCharacter2)
			{
				return;
			}
			amount = -amount;
		}
		else if (community == playerCommunity && community2 != playerCommunity)
		{
			amount = -amount;
		}
		else if (community == playerCommunity && community2 == playerCommunity)
		{
			Character playerCharacter3 = Session.Instance.GetLocalPlayerRecord().PlayerCharacter;
			if (giver == playerCharacter3)
			{
				amount = -amount;
			}
			else if (taker != playerCharacter3)
			{
				return;
			}
		}
		for (int i = 0; i < EquipmentNotifications.Count; i++)
		{
			EquipmentNotification equipmentNotification = EquipmentNotifications[i];
			if (!equipmentNotification.IsFinished() && equipmentNotification.NotificationType == EquipmentNotification.Type.Item && equipmentNotification.Item.GetPrototype() == item.GetPrototype() && equipmentNotification.InfectedWith == item.InfectedWith && equipmentNotification.Item.GetLiquidContentsType() == item.GetLiquidContentsType())
			{
				equipmentNotification.SetAmount(equipmentNotification.Amount + amount);
				if (i == 0)
				{
					HudBehaviour.Instance.UnityEquipmentNotificationAmount.SetUnityText(((equipmentNotification.Amount >= 0) ? "+" : "-") + Mathf.Abs(equipmentNotification.Amount));
				}
				return;
			}
		}
		int index = EquipmentNotifications.Count;
		int num = EquipmentNotifications.Count - 1;
		while (num >= 0 && EquipmentNotifications[num].Item == null && !EquipmentNotifications[num].IsStarted())
		{
			index = num;
			num--;
		}
		EquipmentNotifications.Insert(index, new EquipmentNotification(item, item.InfectedWith, amount));
	}

	public void ShowRecipeDiscoveredNotifications()
	{
		foreach (Recipe item in GameImpl.Instance.CurrentRecipesSortedBySkill)
		{
			if (!item.ShownDiscoveredNotification && item.IsDiscovered())
			{
				item.ShownDiscoveredNotification = true;
				EquipmentNotifications.Add(new EquipmentNotification(item));
				LogEvent logEvent = new LogEvent();
				logEvent.Type = LogEventType.NewRecipe;
				logEvent.Recipe = item;
				Session.Instance.AddLogEvent(logEvent);
			}
		}
	}

	public void AddCharacterNotification(Character character, int titleHash, string text, int promptHash, Type infoPageType)
	{
		Notification notification = new Notification(titleHash, text);
		notification.Character = character;
		notification.PromptHash = promptHash;
		notification.InfoPageType = infoPageType;
		AddNotification(notification);
	}

	public void AddNewQuestNotification(QuestInstance questInstance)
	{
		Notification notification = new Notification(HUD_NewQuest, questInstance.GetDisplayNameString());
		notification.PromptHash = HUD_ViewQuests;
		notification.QuestInstance = questInstance;
		notification.InfoPageType = typeof(QuestPage);
		AddNotification(notification);
	}

	public void AddQuestFailedNotification(QuestInstance questInstance)
	{
		Notification notification = new Notification(HUD_QuestFailed, questInstance.GetDisplayNameString());
		notification.QuestInstance = questInstance;
		AddNotification(notification);
	}

	public void OnStringsVerified()
	{
		foreach (Notification notification in Notifications)
		{
			if (notification.TitleHash == Community.HUD_NewMember)
			{
				notification.Message = LogEventBehaviour.BuildCommunityMemberJoinedMsg(notification.Character);
			}
			else if (notification.TitleHash == Community.HUD_LostMember)
			{
				notification.Message = LogEventBehaviour.BuildCommunityMemberLeftMsg(notification.Character);
			}
			else if (notification.TitleHash == Community.HUD_MemberDied)
			{
				notification.Message = LogEventBehaviour.BuildCommunityMemberDiedMsg(notification.Character);
			}
		}
	}
}
