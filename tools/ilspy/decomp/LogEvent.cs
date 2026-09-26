using System;
using System.Collections.Generic;

public class LogEvent : IReflectable
{
	public LogEventType Type;

	public Character Character;

	public Character Listener;

	public BaseObject ReferringTo;

	public Speech Speech;

	public List<SpeechParamResult> SpeakingParamResults;

	public bool SpeakerWasDirectControlled;

	public SkillType SkillType;

	public int SkillLevel;

	public QuestInstance Quest;

	public PlayerID PlayerID;

	public string PlayerName;

	public Community Community;

	public TimeSpan Time;

	public Recipe Recipe;

	public int FeedRefCount;

	private static int GlobalFeedLimit = 100;

	private static int CharacterFeedLimit = 20;

	public LogEvent()
	{
	}

	public LogEvent(LogEventType type)
	{
		Type = type;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.AddAfter(ref Time, 414);
		switch (Type)
		{
		case LogEventType.Speech:
			reflector.Add(ref Character);
			reflector.AddAfter(ref Listener, 413);
			reflector.AddAfter(ref ReferringTo, 413);
			reflector.Add(ref Speech);
			reflector.Add(ref SpeakingParamResults);
			reflector.Add(ref SpeakerWasDirectControlled);
			break;
		case LogEventType.CommunityMemberJoined:
		case LogEventType.CommunityMemberLeft:
		case LogEventType.CommunityMemberDied:
			reflector.Add(ref Character);
			break;
		case LogEventType.CommunityMemberLevelledUp:
		case LogEventType.CommunityMemberLevelledDown:
			reflector.Add(ref Character);
			reflector.Add(ref SkillType);
			reflector.Add(ref SkillLevel);
			break;
		case LogEventType.QuestDiscovered:
		case LogEventType.QuestCompleted:
		case LogEventType.QuestFailed:
			reflector.Add(ref Quest);
			reflector.AddAfter(ref Character, 413);
			reflector.AddAfter(ref Listener, 413);
			reflector.AddAfter(ref ReferringTo, 413);
			reflector.AddAfter(ref SpeakingParamResults, 413);
			break;
		case LogEventType.PlayerJoined:
		case LogEventType.PlayerLeft:
			reflector.Add(ref PlayerID);
			reflector.Add(ref PlayerName);
			break;
		case LogEventType.DeclaredWar:
		case LogEventType.DeclaredPeace:
		case LogEventType.MadeAlliance:
		case LogEventType.BrokeAlliance:
			reflector.Add(ref Community);
			break;
		case LogEventType.NewRecipe:
			reflector.Add(ref Recipe);
			break;
		case LogEventType.MadeFriends:
		case LogEventType.StartedDating:
		case LogEventType.BrokeUp:
			reflector.AddAfter(ref Character, 553);
			reflector.AddAfter(ref Listener, 553);
			break;
		}
	}

	public bool IsAboutCharacter(Character character)
	{
		if (character == null)
		{
			return false;
		}
		switch (Type)
		{
		case LogEventType.Speech:
		case LogEventType.QuestDiscovered:
		case LogEventType.QuestCompleted:
		case LogEventType.QuestFailed:
			if (Character != character && Listener != character)
			{
				return ReferringTo == character;
			}
			return true;
		case LogEventType.CommunityMemberJoined:
		case LogEventType.CommunityMemberLeft:
		case LogEventType.CommunityMemberDied:
		case LogEventType.CommunityMemberLevelledUp:
		case LogEventType.CommunityMemberLevelledDown:
			return Character == character;
		case LogEventType.PlayerJoined:
		case LogEventType.PlayerLeft:
			return character.AvatarForPlayer == PlayerID;
		default:
			return false;
		}
	}

	public void AddToFeeds(bool canDeleteEvents)
	{
		Session instance = Session.Instance;
		instance.GlobalEventFeed.AddToFeed(this, GlobalFeedLimit, canDeleteEvents);
		switch (Type)
		{
		case LogEventType.Speech:
		case LogEventType.QuestDiscovered:
		case LogEventType.QuestCompleted:
		case LogEventType.QuestFailed:
			if (Character != null && Character.IsControllableByPlayer())
			{
				Character.Feed.AddToFeed(this, CharacterFeedLimit, canDeleteEvents);
			}
			if (Listener != null && Listener.IsControllableByPlayer() && Listener != Character)
			{
				Listener.Feed.AddToFeed(this, CharacterFeedLimit, canDeleteEvents);
			}
			if (ReferringTo is Character character && character.IsControllableByPlayer() && character != Listener && character != Character)
			{
				character.Feed.AddToFeed(this, CharacterFeedLimit, canDeleteEvents);
			}
			break;
		case LogEventType.CommunityMemberJoined:
		case LogEventType.CommunityMemberLeft:
		case LogEventType.CommunityMemberDied:
		case LogEventType.CommunityMemberLevelledUp:
		case LogEventType.CommunityMemberLevelledDown:
			if (Character != null && Character.IsControllableByPlayer())
			{
				Character.Feed.AddToFeed(this, CharacterFeedLimit, canDeleteEvents);
			}
			break;
		case LogEventType.PlayerJoined:
		case LogEventType.PlayerLeft:
		{
			Character avatarForPlayer = instance.CharacterManager.GetAvatarForPlayer(PlayerID);
			if (avatarForPlayer != null && avatarForPlayer.IsControllableByPlayer())
			{
				avatarForPlayer.Feed.AddToFeed(this, CharacterFeedLimit, canDeleteEvents);
			}
			break;
		}
		case LogEventType.DeclaredWar:
		case LogEventType.DeclaredPeace:
		case LogEventType.MadeAlliance:
		case LogEventType.BrokeAlliance:
			break;
		}
	}
}
