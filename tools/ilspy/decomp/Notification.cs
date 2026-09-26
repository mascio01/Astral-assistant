using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Notification
{
	public bool Finished;

	public float DisplayedTime;

	public float Transition;

	public bool Maximised;

	public bool AskToReload;

	public int TitleHash;

	public string Message;

	public int PromptHash;

	public Type InfoPageType;

	public Character Character;

	public QuestInstance QuestInstance;

	public bool TriggeredAutosave;

	private static List<Character> _nearbyCharacters = new List<Character>();

	private static int MaximisableCountdown = 0;

	private static StringBuilder sb = new StringBuilder();

	private static int HUD_CloseNotification = StringUtil.JenkinsHash("HUD_CloseNotification");

	public static float TransitionSpeed = 4f;

	public Notification(int titleHash, string text)
	{
		TitleHash = titleHash;
		Message = text;
	}

	public bool IsFinished()
	{
		if (Finished)
		{
			return Transition == 0f;
		}
		return false;
	}

	public virtual void OnNotificationAdded(Notification notification)
	{
		if (QuestInstance != null && TitleHash != NotificationManager.HUD_QuestFailed && notification.QuestInstance != null && notification.QuestInstance.Quest == QuestInstance.Quest)
		{
			Finished = true;
		}
	}

	public virtual bool WantWaitUntilCombatFinished()
	{
		if (QuestInstance == null || !QuestInstance.Quest.ShowNotificationEventInCombat)
		{
			return true;
		}
		return false;
	}

	public bool WantMinimised(out bool becauseOfCombat)
	{
		becauseOfCombat = false;
		if (!Maximised)
		{
			if (StoryManager.Instance.GetMostInterestingSpeaker() != null || HudBehaviour.Instance.IsShowingPipSpeechBubble() || Hud.Instance.Cursor.LastSpeechToReplyTo != null || SaveGameManager.Instance.IsSaving() || OnlineParty.Instance.IsVerifyingStrings() || NotificationManager.Instance.IsDisplayingCommunityStat() || Session.Instance.Transition < 1f)
			{
				MaximisableCountdown = 2;
				return true;
			}
			if (MaximisableCountdown > 0)
			{
				MaximisableCountdown--;
				return true;
			}
		}
		if (WantWaitUntilCombatFinished() && IsInCombat())
		{
			becauseOfCombat = true;
			return true;
		}
		return false;
	}

	public static bool IsInCombat()
	{
		Character localControlledCharacter = Hud.Instance.LocalControlledCharacter;
		if (localControlledCharacter != null)
		{
			if (localControlledCharacter.SparringPartner != null || localControlledCharacter.IsBeingBitten())
			{
				return true;
			}
			int num = Character.BaseSightRange * 2;
			GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(localControlledCharacter.Tile - new TerrainCoord(num, num), localControlledCharacter.Tile + new TerrainCoord(num, num), _nearbyCharacters);
			foreach (Character nearbyCharacter in _nearbyCharacters)
			{
				if (!(nearbyCharacter.Tile.GetDistSquared(localControlledCharacter.Tile) > (float)(num * num)))
				{
					if (nearbyCharacter.InCombat && nearbyCharacter.CurrentActionAnim != ActionAnim.HandsUp)
					{
						_nearbyCharacters.Clear();
						return true;
					}
					if (nearbyCharacter.CurrentAnimState == AnimState.Ragdoll && nearbyCharacter.CarriedBy == null)
					{
						_nearbyCharacters.Clear();
						return true;
					}
				}
			}
			_nearbyCharacters.Clear();
		}
		return false;
	}

	public static InputFunction GetInputFunctionForPageType(Type pageType)
	{
		if (pageType == typeof(CharacterPage))
		{
			return InputFunction.Inventory;
		}
		if (pageType == typeof(BuildingPage))
		{
			return InputFunction.Inventory;
		}
		if (pageType == typeof(CommunityPage))
		{
			return InputFunction.Community;
		}
		if (pageType == typeof(QuestPage))
		{
			return InputFunction.Quests;
		}
		if (pageType == typeof(MapPage))
		{
			return InputFunction.Map;
		}
		if (pageType == typeof(LogPage))
		{
			return InputFunction.Log;
		}
		return InputFunction.Invalid;
	}

	private InputFunction GetInputFunctionToOpenPage()
	{
		InputFunctionManager instance = InputFunctionManager.Instance;
		InputFunction inputFunction = GetInputFunctionForPageType(InfoPageType);
		if (inputFunction != InputFunction.Invalid && !instance.IsMapped(inputFunction))
		{
			inputFunction = InputFunction.Inventory;
		}
		return inputFunction;
	}

	public void HandleInput(InputFrame inputFrame)
	{
		if (Transition == 0f || !Maximised)
		{
			return;
		}
		InputFunctionManager instance = InputFunctionManager.Instance;
		if (AskToReload)
		{
			return;
		}
		if (InfoPageType != null && Hud.Instance.LocalControlledCharacter != null)
		{
			InputFunction inputFunctionToOpenPage = GetInputFunctionToOpenPage();
			if (inputFunctionToOpenPage != InputFunction.Invalid && instance.IsJustPressed(inputFunctionToOpenPage))
			{
				Hud.Instance.WantInfoScreenType = ((InfoPageType == typeof(QuestPage)) ? Hud.OpenInfoScreenType.QuestPage : ((InfoPageType == typeof(CommunityPage)) ? Hud.OpenInfoScreenType.CommunityPage : Hud.OpenInfoScreenType.Inventory));
				Hud.Instance.OpenInfoScreenFor = Hud.Instance.LocalControlledCharacter;
				Finished = true;
				Transition = 0f;
			}
		}
		if (instance.IsJustPressed(InputFunction.Back))
		{
			Finished = true;
		}
	}

	public virtual void UpdateNotification()
	{
		HudBehaviour instance = HudBehaviour.Instance;
		RectTransform rectTransform = (RectTransform)instance.UnityNotification.transform;
		if (Transition == 0f && !Finished && Maximised)
		{
			instance.UnityNotificationTitle.SetUnityText(GameImpl.Translate(TitleHash));
			QuestGroup questGroup = ((QuestInstance != null) ? GameImpl.Instance.FindQuestGroupByUniqueID(QuestInstance.Quest.GroupID) : null);
			instance.UnityNotificationQuestGroupName.gameObject.SetActive(questGroup != null);
			if (instance.UnityNotificationQuestGroupName.gameObject.activeSelf)
			{
				instance.UnityNotificationQuestGroupName.SetUnityText(questGroup.GetTitleString(QuestInstance.QuestGiver, QuestInstance.QuestSeeker, QuestInstance.QuestObject, QuestInstance) + ":");
			}
		}
		instance.UnityNotificationMessage.SetUnityTextIfDifferent(Message);
		if (Transition >= 1f && !Finished && Maximised && TitleHash == NotificationManager.HUD_NewQuest && !Session.Instance.DifficultySettings.SaveTokensRequired && !TriggeredAutosave)
		{
			Session.Instance.AutoSave(SaveGameType.AutoSave, overwriteAllSlots: false);
			TriggeredAutosave = true;
		}
		InputFunction inputFunctionToOpenPage = GetInputFunctionToOpenPage();
		sb.Length = 0;
		if (inputFunctionToOpenPage != InputFunction.Invalid && PromptHash != 0)
		{
			sb.AppendButtonPromptString(inputFunctionToOpenPage);
			sb.Append(GameImpl.Translate(PromptHash));
			sb.Append(' ');
		}
		sb.AppendButtonPromptString(InputFunction.Back);
		sb.Append(GameImpl.Translate(HUD_CloseNotification));
		instance.UnityNotificationPrompt.SetUnityTextIfDifferent(sb);
		Texture2D texture2D = null;
		Material mat = null;
		Color col = Color.white;
		bool flag = true;
		if (Character != null)
		{
			texture2D = Character.GetIcon(out mat, out col, highlighted: false);
		}
		else if (TitleHash == NotificationManager.HUD_NewQuest)
		{
			texture2D = GameCursor.CurrentQuestIcon.GetAsset();
		}
		else
		{
			flag = false;
		}
		instance.UnityNotificationIcon.gameObject.SetActive(flag);
		if (flag)
		{
			instance.UnityNotificationIcon.texture = texture2D;
			instance.UnityNotificationIcon.material = mat;
			instance.UnityNotificationIcon.color = ((texture2D != null) ? col : new Color(0f, 0f, 0f, 0f));
		}
		Transition = Mathf.Clamp(Transition + Time.unscaledDeltaTime * ((Finished || !Maximised) ? (-1f) : 1f) * TransitionSpeed, 0f, 1f);
		instance.UnityNotification.SetActive(Transition > 0f);
		instance.UnityNotificationEventSystem.gameObject.SetActive(Transition > 0f && !InfoScreen.Instance.Active && !GameImpl.Instance.IsDialogOpen());
		float t = Mathf.Clamp01(Transition - InfoScreen.Instance.Transition);
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, Mathf.Lerp(rectTransform.rect.height + 8f, -8f, t));
		rectTransform.offsetMax = new Vector2((Hud.Instance.Pip.FocusObject != null) ? (-420f) : (-8f), rectTransform.offsetMax.y);
	}
}
