using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogEventBehaviour : MonoBehaviour
{
	public LogEvent LogEvent;

	private static string TextStr = "Text";

	private static string IconStr = "Icon";

	private static string SpeechNameStr = "Background/Name";

	private static string SpeechTextStr = "Background/Speech";

	private static string QuestTitleStr = "Panel/Title";

	private static string QuestTextStr = "Panel/Text";

	public static int HUD_JoinedYou = StringUtil.JenkinsHash("HUD_JoinedYou");

	public static int HUD_LeftYou = StringUtil.JenkinsHash("HUD_LeftYou");

	public static int HUD_Died = StringUtil.JenkinsHash("HUD_Died");

	public static int HUD_YouAreAtWar = StringUtil.JenkinsHash("HUD_YouAreAtWar");

	public static int HUD_YouAreAtPeace = StringUtil.JenkinsHash("HUD_YouAreAtPeace");

	public static int HUD_YouAreAllies = StringUtil.JenkinsHash("HUD_YouAreAllies");

	public static int HUD_YouAreNotAllies = StringUtil.JenkinsHash("HUD_YouAreNotAllies");

	public static int HUD_PartyMemberJoined = StringUtil.JenkinsHash("HUD_PartyMemberJoined");

	public static int HUD_PartyMemberLeft = StringUtil.JenkinsHash("HUD_PartyMemberLeft");

	public static int HUD_NowFriends = StringUtil.JenkinsHash("HUD_NowFriends");

	public static int HUD_NowInRelationship = StringUtil.JenkinsHash("HUD_NowInRelationship");

	public static int HUD_BrokenUp = StringUtil.JenkinsHash("HUD_BrokenUp");

	public void InitializeSeparator(int day, int dayOfYear)
	{
		base.gameObject.FindChild(TextStr).GetComponent<TextMeshProUGUI>().SetUnityText(LoadingMenu.BuildSessionDescriptionString(day, dayOfYear));
	}

	public static void CalcTimeOfDay(TimeSpan playTime, out int hour, out int minute)
	{
		float num = Session.CalcHourOfDayFromDaysSinceStart(Session.Instance.Weather.CalcDaysSinceStartFromTime(playTime));
		hour = (int)num;
		minute = (int)((num - (float)hour) * 60f);
	}

	public void Initialize(LogEvent logEvent)
	{
		LogEvent = logEvent;
		TextMeshProUGUI unityText = null;
		TextMeshProUGUI textMeshProUGUI = null;
		switch (LogEvent.Type)
		{
		case LogEventType.Speech:
		{
			unityText = base.gameObject.FindChild(SpeechNameStr).GetComponent<TextMeshProUGUI>();
			textMeshProUGUI = base.gameObject.FindChild(SpeechTextStr).GetComponent<TextMeshProUGUI>();
			CalcTimeOfDay(LogEvent.Time, out var hour, out var minute);
			unityText.SetUnityText(LogEvent.Character.GetDisplayNameString() + " - " + hour + ":" + minute.ToString("00"));
			break;
		}
		case LogEventType.QuestDiscovered:
		case LogEventType.QuestCompleted:
		case LogEventType.QuestFailed:
		{
			unityText = base.gameObject.FindChild(QuestTitleStr).GetComponent<TextMeshProUGUI>();
			textMeshProUGUI = base.gameObject.FindChild(QuestTextStr).GetComponent<TextMeshProUGUI>();
			Speech.BuildSpeechText(LogEvent.Quest.Quest.DescriptionHash, LogEvent.Quest.Quest.Params, LogEvent.Character, LogEvent.Listener, LogEvent.ReferringTo, LogEvent.SpeakingParamResults, out var speechText, null, englishOnly: false, isQuest: true);
			textMeshProUGUI.SetUnityText(speechText);
			break;
		}
		case LogEventType.NewRecipe:
			unityText = base.gameObject.FindChild(QuestTitleStr).GetComponent<TextMeshProUGUI>();
			textMeshProUGUI = base.gameObject.FindChild(QuestTextStr).GetComponent<TextMeshProUGUI>();
			break;
		default:
			textMeshProUGUI = base.gameObject.FindChild(TextStr).GetComponent<TextMeshProUGUI>();
			break;
		}
		switch (LogEvent.Type)
		{
		case LogEventType.Speech:
			if (LogEvent != null && LogEvent.Speech != null)
			{
				Speech.BuildSpeechText(LogEvent.Speech.TextHash, LogEvent.Speech.Params, LogEvent.Character, LogEvent.Listener, LogEvent.ReferringTo, LogEvent.SpeakingParamResults, out var speechText2, null, englishOnly: false, isQuest: false);
				textMeshProUGUI.SetUnityText(speechText2);
			}
			break;
		case LogEventType.CommunityMemberJoined:
			textMeshProUGUI.SetUnityText(BuildCommunityMemberJoinedMsg(LogEvent.Character));
			break;
		case LogEventType.CommunityMemberLeft:
			textMeshProUGUI.SetUnityText(BuildCommunityMemberLeftMsg(LogEvent.Character));
			break;
		case LogEventType.CommunityMemberLevelledUp:
			textMeshProUGUI.SetUnityText(BuildCommunityMemberLevelledUpMsg(LogEvent.Character, LogEvent.SkillType, LogEvent.SkillLevel));
			break;
		case LogEventType.CommunityMemberLevelledDown:
			textMeshProUGUI.SetUnityText(BuildCommunityMemberLevelledDownMsg(LogEvent.Character, LogEvent.SkillType, LogEvent.SkillLevel));
			break;
		case LogEventType.CommunityMemberDied:
			textMeshProUGUI.SetUnityText(BuildCommunityMemberDiedMsg(LogEvent.Character));
			break;
		case LogEventType.DeclaredWar:
			textMeshProUGUI.SetUnityText(BuildDeclaredWarMsg(LogEvent.Community));
			break;
		case LogEventType.DeclaredPeace:
			textMeshProUGUI.SetUnityText(BuildDeclaredPeaceMsg(LogEvent.Community));
			break;
		case LogEventType.MadeAlliance:
			textMeshProUGUI.SetUnityText(BuildMadeAllianceMsg(LogEvent.Community));
			break;
		case LogEventType.BrokeAlliance:
			textMeshProUGUI.SetUnityText(BuildBrokeAllianceMsg(LogEvent.Community));
			break;
		case LogEventType.PlayerJoined:
			textMeshProUGUI.SetUnityText(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_PartyMemberJoined).Replace("%1", LogEvent.PlayerName)));
			break;
		case LogEventType.PlayerLeft:
			textMeshProUGUI.SetUnityText(StringUtil.ApplyFormulae(GameImpl.Translate(HUD_PartyMemberLeft).Replace("%1", LogEvent.PlayerName)));
			break;
		case LogEventType.QuestDiscovered:
			unityText.SetUnityText(GameImpl.Translate(NotificationManager.HUD_NewQuest));
			break;
		case LogEventType.QuestFailed:
			unityText.SetUnityText(GameImpl.Translate(NotificationManager.HUD_QuestFailed));
			break;
		case LogEventType.QuestCompleted:
			unityText.SetUnityText(GameImpl.Translate(NotificationManager.HUD_QuestCompleted));
			break;
		case LogEventType.NewRecipe:
			unityText.SetUnityText(EquipmentNotification.GetNewRecipeTitle(LogEvent.Recipe));
			textMeshProUGUI.SetUnityText(EquipmentNotification.GetNewRecipeText(LogEvent.Recipe));
			break;
		case LogEventType.MadeFriends:
			textMeshProUGUI.SetUnityText(BuildMadeFriendsMsg(LogEvent.Character, LogEvent.Listener));
			break;
		case LogEventType.StartedDating:
			textMeshProUGUI.SetUnityText(BuildStartedDatingMsg(LogEvent.Character, LogEvent.Listener));
			break;
		case LogEventType.BrokeUp:
			textMeshProUGUI.SetUnityText(BuildBrokeUpMsg(LogEvent.Character, LogEvent.Listener));
			break;
		}
	}

	public void Update()
	{
		if (LogEvent == null)
		{
			return;
		}
		Vector2 uIObjectCentreOnScreen = HudBehaviour.Instance.GetUIObjectCentreOnScreen(base.gameObject);
		if (!(uIObjectCentreOnScreen.y >= -128f) || !(uIObjectCentreOnScreen.y <= 1208f))
		{
			return;
		}
		switch (LogEvent.Type)
		{
		case LogEventType.Speech:
		case LogEventType.CommunityMemberJoined:
		case LogEventType.CommunityMemberLeft:
		case LogEventType.CommunityMemberDied:
		case LogEventType.CommunityMemberLevelledUp:
		case LogEventType.CommunityMemberLevelledDown:
		{
			RawImage component2 = base.gameObject.FindChild(IconStr).GetComponent<RawImage>();
			if (LogEvent.Character != null)
			{
				Material mat2 = null;
				component2.texture = LogEvent.Character.GetIcon(out mat2, out var col2, highlighted: false);
				component2.color = col2;
			}
			else
			{
				component2.gameObject.SetActive(value: false);
			}
			break;
		}
		case LogEventType.PlayerJoined:
		case LogEventType.PlayerLeft:
		{
			RawImage component = base.gameObject.FindChild(IconStr).GetComponent<RawImage>();
			Character avatarForPlayer = Session.Instance.CharacterManager.GetAvatarForPlayer(LogEvent.PlayerID);
			if (avatarForPlayer != null)
			{
				Material mat = null;
				Color col = Color.white;
				component.texture = avatarForPlayer?.GetIcon(out mat, out col, highlighted: false);
				component.color = col;
			}
			else
			{
				component.gameObject.SetActive(value: false);
			}
			break;
		}
		case LogEventType.QuestDiscovered:
			base.gameObject.FindChild(IconStr).GetComponent<RawImage>().texture = (Texture2D)GameCursor.CurrentQuestIcon;
			break;
		case LogEventType.QuestFailed:
			base.gameObject.FindChild(IconStr).GetComponent<RawImage>().texture = (Texture2D)GameCursor.FailedQuestIcon;
			break;
		case LogEventType.QuestCompleted:
			base.gameObject.FindChild(IconStr).GetComponent<RawImage>().texture = (Texture2D)GameCursor.CompletedQuestIcon;
			break;
		case LogEventType.NewRecipe:
			EquipmentIconBehaviour.SetUnityIconFromRecipeProduct(base.gameObject.FindChild(IconStr).GetComponent<RawImage>(), LogEvent.Recipe);
			break;
		case LogEventType.DeclaredWar:
		case LogEventType.DeclaredPeace:
		case LogEventType.MadeAlliance:
		case LogEventType.BrokeAlliance:
			break;
		}
	}

	public static string BuildCommunityMemberJoinedMsg(Character character)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		if (character != null)
		{
			character.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		}
		else
		{
			stringBuilder.Append(GameImpl.Translate(Speech.SPEECH_Someone));
		}
		return StringUtil.ApplyFormulaeWithHashes(GameImpl.Translate(HUD_JoinedYou).Replace("%1", stringBuilder.ToString()), null, character, (character == null) ? Speech.SPEECH_Someone : 0);
	}

	public static string BuildCommunityMemberLeftMsg(Character character)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		if (character != null)
		{
			character.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		}
		else
		{
			stringBuilder.Append(GameImpl.Translate(Speech.SPEECH_Someone));
		}
		return StringUtil.ApplyFormulaeWithHashes(GameImpl.Translate(HUD_LeftYou).Replace("%1", stringBuilder.ToString()), null, character, (character == null) ? Speech.SPEECH_Someone : 0);
	}

	public static string BuildCommunityMemberLevelledUpMsg(Character character, SkillType skillType, int level)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		if (character != null)
		{
			character.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		}
		else
		{
			stringBuilder.Append(GameImpl.Translate(Speech.SPEECH_Someone));
		}
		StringBuilder stringBuilder2 = new StringBuilder(GameImpl.Translate(Skillset.HUD_ReachedLevel)).Replace("%1", stringBuilder.ToString()).Replace("%2", level.ToString()).Replace("%3", GameImpl.Translate(Skillset.GetSkillNameHash(skillType)));
		StringUtil.ApplyFormulae(stringBuilder2, null, character, level);
		return stringBuilder2.ToString();
	}

	public static string BuildCommunityMemberLevelledDownMsg(Character character, SkillType skillType, int level)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		if (character != null)
		{
			character.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		}
		else
		{
			stringBuilder.Append(GameImpl.Translate(Speech.SPEECH_Someone));
		}
		StringBuilder stringBuilder2 = new StringBuilder(GameImpl.Translate(Skillset.HUD_ReducedLevel)).Replace("%1", stringBuilder.ToString()).Replace("%2", level.ToString()).Replace("%3", GameImpl.Translate(Skillset.GetSkillNameHash(skillType)));
		StringUtil.ApplyFormulae(stringBuilder2, null, character, level);
		return stringBuilder2.ToString();
	}

	public static string BuildCommunityMemberDiedMsg(Character member)
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		if (member != null)
		{
			member.BuildDisplayName(stringBuilder, noStrangers: true, englishOnly: false);
		}
		else
		{
			stringBuilder.Append(GameImpl.Translate(Speech.SPEECH_Someone));
		}
		StringBuilder stringBuilder2 = new StringBuilder(GameImpl.Translate(HUD_Died)).Replace("%1", stringBuilder.ToString());
		StringUtil.ApplyFormulae(stringBuilder2, null, member);
		return stringBuilder2.ToString();
	}

	public static string BuildDeclaredWarMsg(Community community)
	{
		StringBuilder stringBuilder = new StringBuilder(GameImpl.Translate(HUD_YouAreAtWar)).Replace("%1", (community != null) ? community.GetDisplayNameString(noStrangers: false, englishOnly: false) : GameImpl.Translate(Speech.SPEECH_Someone));
		StringUtil.ApplyFormulae(stringBuilder, null, community);
		return stringBuilder.ToString();
	}

	public static string BuildDeclaredPeaceMsg(Community community)
	{
		StringBuilder stringBuilder = new StringBuilder(GameImpl.Translate(HUD_YouAreAtPeace)).Replace("%1", (community != null) ? community.GetDisplayNameString(noStrangers: false, englishOnly: false) : GameImpl.Translate(Speech.SPEECH_Someone));
		StringUtil.ApplyFormulae(stringBuilder, null, community);
		return stringBuilder.ToString();
	}

	public static string BuildMadeAllianceMsg(Community community)
	{
		StringBuilder stringBuilder = new StringBuilder(GameImpl.Translate(HUD_YouAreAllies)).Replace("%1", (community != null) ? community.GetDisplayNameString(noStrangers: false, englishOnly: false) : GameImpl.Translate(Speech.SPEECH_Someone));
		StringUtil.ApplyFormulae(stringBuilder, null, community);
		return stringBuilder.ToString();
	}

	public static string BuildBrokeAllianceMsg(Community community)
	{
		StringBuilder stringBuilder = new StringBuilder(GameImpl.Translate(HUD_YouAreNotAllies)).Replace("%1", (community != null) ? community.GetDisplayNameString(noStrangers: false, englishOnly: false) : GameImpl.Translate(Speech.SPEECH_Someone));
		StringUtil.ApplyFormulae(stringBuilder, null, community);
		return stringBuilder.ToString();
	}

	public static string BuildMadeFriendsMsg(Character from, Character to)
	{
		return StringUtil.ApplyFormulaeWithHashes(GameImpl.Translate(HUD_NowFriends).Replace("%1", (from != null) ? from.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)).Replace("%2", (to != null) ? to.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)), null, from, (from == null) ? Speech.SPEECH_Someone : 0, to, (to == null) ? Speech.SPEECH_Someone : 0);
	}

	public static string BuildStartedDatingMsg(Character from, Character to)
	{
		return StringUtil.ApplyFormulaeWithHashes(GameImpl.Translate(HUD_NowInRelationship).Replace("%1", (from != null) ? from.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)).Replace("%2", (to != null) ? to.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)), null, from, (from == null) ? Speech.SPEECH_Someone : 0, to, (to == null) ? Speech.SPEECH_Someone : 0);
	}

	public static string BuildBrokeUpMsg(Character from, Character to)
	{
		return StringUtil.ApplyFormulaeWithHashes(GameImpl.Translate(HUD_BrokenUp).Replace("%1", (from != null) ? from.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)).Replace("%2", (to != null) ? to.GetDisplayNameString() : GameImpl.Translate(Speech.SPEECH_Someone)), null, from, (from == null) ? Speech.SPEECH_Someone : 0, to, (to == null) ? Speech.SPEECH_Someone : 0);
	}
}
