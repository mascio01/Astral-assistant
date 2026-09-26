using System;
using System.Collections.Generic;
using UnityEngine;

public struct Relationship : IReflectable
{
	public static string[] RelationshipTypeNames = StringUtil.GetEnumNames<RelationshipType>();

	public RelationshipType RelationshipType;

	public Character RelationshipTarget;

	public float ApprovalContribution;

	public float RespectContribution;

	public bool KnownToPlayer;

	public static float MinParentAge = 18f;

	public static float MaxSiblingAgeDiff = 10f;

	public static float MaxFamilySkinColorDiff = 2f;

	public static float ProbabilityOfLoveBeingReciprocated = 0.5f;

	private static List<Character> Mark = new List<Character>();

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref RelationshipType);
		reflector.Add(ref RelationshipTarget);
		reflector.Add(ref ApprovalContribution);
		reflector.Add(ref RespectContribution);
		reflector.Add(ref KnownToPlayer);
	}

	public override int GetHashCode()
	{
		return (int)(RelationshipType + (RelationshipTarget.Id << 16));
	}

	public override bool Equals(object obj)
	{
		if (obj is Relationship)
		{
			return this == (Relationship)obj;
		}
		return false;
	}

	public bool Equals(Relationship other)
	{
		if (RelationshipType == other.RelationshipType)
		{
			return RelationshipTarget == other.RelationshipTarget;
		}
		return false;
	}

	public static bool operator ==(Relationship a, Relationship b)
	{
		if (a.RelationshipType == b.RelationshipType)
		{
			return a.RelationshipTarget == b.RelationshipTarget;
		}
		return false;
	}

	public static bool operator !=(Relationship a, Relationship b)
	{
		if (a.RelationshipType == b.RelationshipType)
		{
			return a.RelationshipTarget != b.RelationshipTarget;
		}
		return true;
	}

	public static bool HasRelationship(Character from, RelationshipType relationshipType, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				return from.Relationships[i].RelationshipType == relationshipType;
			}
		}
		return relationshipType == RelationshipType.None;
	}

	public static bool HasRelationshipWithAnyone(Character from, RelationshipType relationshipType, bool livingOnly)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipType == relationshipType && (!livingOnly || (from.Relationships[i].RelationshipTarget != null && from.Relationships[i].RelationshipTarget.AliveAndNotZombie)))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsRomanticRelationshipType(RelationshipType relationshipType)
	{
		if ((uint)(relationshipType - 5) <= 2u || relationshipType == RelationshipType.Ex)
		{
			return true;
		}
		return false;
	}

	public static bool HasRomanticRelationship(Character from, Character to)
	{
		RelationshipType relationship = GetRelationship(from, to);
		if ((uint)(relationship - 5) <= 1u)
		{
			return true;
		}
		return false;
	}

	public static bool HasRomanticRelationshipWithAnyone(Character from, bool livingOnly)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			RelationshipType relationshipType = from.Relationships[i].RelationshipType;
			if ((uint)(relationshipType - 5) <= 1u && (!livingOnly || (from.Relationships[i].RelationshipTarget != null && from.Relationships[i].RelationshipTarget.AliveAndNotZombie)))
			{
				return true;
			}
		}
		return false;
	}

	public static RelationshipType GetRelationship(Character from, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				return from.Relationships[i].RelationshipType;
			}
		}
		return RelationshipType.None;
	}

	public static int GetRelationshipNameHash(RelationshipType relationshipType, GenderType gender)
	{
		switch (relationshipType)
		{
		case RelationshipType.ParentOf:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Daughter;
			}
			return Speech.SPEECH_Son;
		case RelationshipType.ChildOf:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Mom;
			}
			return Speech.SPEECH_Dad;
		case RelationshipType.SiblingOf:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Sister;
			}
			return Speech.SPEECH_Brother;
		case RelationshipType.MarriedTo:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Wife;
			}
			return Speech.SPEECH_Husband;
		case RelationshipType.SleepingWith:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Girlfriend;
			}
			return Speech.SPEECH_Boyfriend;
		case RelationshipType.FriendsWith:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Friend_Female;
			}
			return Speech.SPEECH_Friend_Male;
		case RelationshipType.Ex:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Ex_Female;
			}
			return Speech.SPEECH_Ex_Male;
		case RelationshipType.InLoveWith:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Crush_Female;
			}
			return Speech.SPEECH_Crush_Male;
		case RelationshipType.NotInLoveWith:
			if (gender != GenderType.Male)
			{
				return Speech.SPEECH_Stalker_Female;
			}
			return Speech.SPEECH_Stalker_Male;
		default:
			return 0;
		}
	}

	public static bool HasAnyRelationship(Character from, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				return true;
			}
		}
		return false;
	}

	public static int FindRelationshipIndex(Character from, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				return i;
			}
		}
		return -1;
	}

	private static bool CanBeFamily(Character from, RelationshipType relationshipType, Character to, bool force)
	{
		for (int i = 0; i < to.Relationships.Count; i++)
		{
			RelationshipType relationshipType2 = to.Relationships[i].RelationshipType;
			if ((uint)(relationshipType2 - 4) <= 5u && (relationshipType != RelationshipType.ChildOf || to.Relationships[i].RelationshipType != RelationshipType.MarriedTo || !HasRelationship(from, RelationshipType.ChildOf, to.Relationships[i].RelationshipTarget)) && IsFamily(from, to.Relationships[i].RelationshipTarget))
			{
				return false;
			}
		}
		for (int j = 0; j < from.Relationships.Count; j++)
		{
			RelationshipType relationshipType2 = from.Relationships[j].RelationshipType;
			if ((uint)(relationshipType2 - 4) <= 5u && (relationshipType != RelationshipType.ParentOf || from.Relationships[j].RelationshipType != RelationshipType.MarriedTo || !HasRelationship(to, RelationshipType.ChildOf, from.Relationships[j].RelationshipTarget)) && IsFamily(to, from.Relationships[j].RelationshipTarget))
			{
				return false;
			}
		}
		if (force)
		{
			return true;
		}
		HumanAppearance humanAppearance = to.Appearance as HumanAppearance;
		HumanAppearance humanAppearance2 = from.Appearance as HumanAppearance;
		if (humanAppearance != null && humanAppearance2 != null)
		{
			return Math.Abs(humanAppearance.SkinColorIndex - humanAppearance2.SkinColorIndex) <= MaxFamilySkinColorDiff;
		}
		return false;
	}

	public static bool CanHaveRelationship(Character from, RelationshipType relationshipType, Character to, bool allowChange, bool force)
	{
		if (from == null || to == null)
		{
			return false;
		}
		if (from.GetBaseObjectType() != to.GetBaseObjectType())
		{
			return false;
		}
		RelationshipType relationship = GetRelationship(from, to);
		if (relationship == relationshipType)
		{
			return true;
		}
		if (!allowChange && relationship != RelationshipType.None)
		{
			return false;
		}
		switch (relationshipType)
		{
		case RelationshipType.ChildOf:
		{
			if (!force && to.Appearance.Age - MinParentAge < from.Appearance.Age)
			{
				return false;
			}
			if (!CanBeFamily(from, relationshipType, to, force))
			{
				return false;
			}
			if (relationship != RelationshipType.None)
			{
				return false;
			}
			for (int j = 0; j < from.Relationships.Count; j++)
			{
				if (from.Relationships[j].RelationshipType == RelationshipType.ChildOf && from.Relationships[j].RelationshipTarget != to && from.Relationships[j].RelationshipTarget.GetGender() == to.GetGender())
				{
					return false;
				}
			}
			if (IsDescendantOf(to, from))
			{
				return false;
			}
			break;
		}
		case RelationshipType.ParentOf:
		{
			if (!force && from.Appearance.Age - MinParentAge < to.Appearance.Age)
			{
				return false;
			}
			if (!CanBeFamily(from, relationshipType, to, force))
			{
				return false;
			}
			if (relationship != RelationshipType.None)
			{
				return false;
			}
			for (int i = 0; i < to.Relationships.Count; i++)
			{
				if (to.Relationships[i].RelationshipType == RelationshipType.ChildOf && to.Relationships[i].RelationshipTarget != from && to.Relationships[i].RelationshipTarget.GetGender() == from.GetGender())
				{
					return false;
				}
			}
			if (IsDescendantOf(from, to))
			{
				return false;
			}
			break;
		}
		case RelationshipType.SiblingOf:
			if (!force && Math.Abs(from.Appearance.Age - to.Appearance.Age) > MaxSiblingAgeDiff)
			{
				return false;
			}
			if (relationship != RelationshipType.None)
			{
				return false;
			}
			if (!CanBeFamily(from, relationshipType, to, force))
			{
				return false;
			}
			if (IsDescendantOf(from, to) || IsDescendantOf(to, from))
			{
				return false;
			}
			break;
		case RelationshipType.FriendsWith:
			if (relationship == RelationshipType.ChildOf || relationship == RelationshipType.ParentOf || relationship == RelationshipType.SiblingOf)
			{
				return false;
			}
			if (IsFamily(from, to))
			{
				return false;
			}
			break;
		case RelationshipType.SleepingWith:
		case RelationshipType.MarriedTo:
		case RelationshipType.Ex:
		{
			if (relationship == RelationshipType.ChildOf || relationship == RelationshipType.ParentOf || relationship == RelationshipType.SiblingOf)
			{
				return false;
			}
			if (!from.IsAttractedTo(to.GetGender()) || !to.IsAttractedTo(from.GetGender()))
			{
				return false;
			}
			Character partner = GetPartner(from);
			if (partner != null && partner != to && partner.AliveAndNotZombie)
			{
				return false;
			}
			Character partner2 = GetPartner(to);
			if (partner2 != null && partner2 != from && partner2.AliveAndNotZombie)
			{
				return false;
			}
			if (IsFamily(from, to))
			{
				return false;
			}
			break;
		}
		case RelationshipType.InLoveWith:
		case RelationshipType.NotInLoveWith:
			if (relationship == RelationshipType.ChildOf || relationship == RelationshipType.ParentOf || relationship == RelationshipType.SiblingOf)
			{
				return false;
			}
			if (!from.IsAttractedTo(to.GetGender()) || !to.IsAttractedTo(from.GetGender()))
			{
				return false;
			}
			if (IsFamily(from, to))
			{
				return false;
			}
			break;
		}
		return true;
	}

	public static void SetRelationshipApprovalRespect(Character from, RelationshipType relationshipType, Character to, float approval, float respect)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to && from.Relationships[i].RelationshipType == relationshipType)
			{
				Relationship value = from.Relationships[i];
				value.ApprovalContribution = approval;
				value.RespectContribution = respect;
				from.Relationships[i] = value;
				from.ClearOpinionCacheForObject(to);
			}
		}
	}

	public static void RandomizeRelationshipApprovalRespect(Character from, Character to, CustomRandom rand)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				Relationship value = from.Relationships[i];
				switch (value.RelationshipType)
				{
				case RelationshipType.ParentOf:
				case RelationshipType.ChildOf:
				case RelationshipType.SiblingOf:
					value.ApprovalContribution = Mathf.Lerp(-40f, 80f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(-40f, 80f, rand.RandomFloat());
					break;
				case RelationshipType.FriendsWith:
					value.ApprovalContribution = Mathf.Lerp(20f, 60f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(-20f, 40f, rand.RandomFloat());
					break;
				case RelationshipType.SleepingWith:
				case RelationshipType.MarriedTo:
					value.ApprovalContribution = Mathf.Lerp(20f, 100f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(-40f, 80f, rand.RandomFloat());
					break;
				case RelationshipType.Ex:
					value.ApprovalContribution = Mathf.Lerp(-100f, 60f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(-100f, 100f, rand.RandomFloat());
					break;
				case RelationshipType.InLoveWith:
					value.ApprovalContribution = Mathf.Lerp(50f, 100f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(0f, 80f, rand.RandomFloat());
					break;
				case RelationshipType.NotInLoveWith:
					value.ApprovalContribution = Mathf.Lerp(-50f, 50f, rand.RandomFloat());
					value.RespectContribution = Mathf.Lerp(-50f, 50f, rand.RandomFloat());
					break;
				}
				from.Relationships[i] = value;
			}
		}
	}

	public static bool SetRelationship(Character from, RelationshipType relationshipType, Character to, bool allowChange, bool generating, bool silent = false)
	{
		return SetRelationship(from, relationshipType, to, allowChange, generating, force: false, 0, silent);
	}

	private static bool SetRelationship(Character from, RelationshipType relationshipType, Character to, bool allowChange, bool generating, bool force, int recursion, bool silent = false)
	{
		if (HasRelationship(from, relationshipType, to))
		{
			return true;
		}
		if (relationshipType == RelationshipType.NotInLoveWith && GetRelationship(to, from) != RelationshipType.InLoveWith)
		{
			relationshipType = RelationshipType.None;
		}
		if (!CanHaveRelationship(from, relationshipType, to, allowChange, force))
		{
			return false;
		}
		switch (relationshipType)
		{
		case RelationshipType.ChildOf:
			if (!CanHaveRelationship(to, RelationshipType.ParentOf, from, allowChange, force))
			{
				return false;
			}
			break;
		case RelationshipType.ParentOf:
			if (!CanHaveRelationship(to, RelationshipType.ChildOf, from, allowChange, force))
			{
				return false;
			}
			break;
		case RelationshipType.None:
		case RelationshipType.SiblingOf:
		case RelationshipType.FriendsWith:
		case RelationshipType.SleepingWith:
		case RelationshipType.MarriedTo:
		case RelationshipType.Ex:
			if (!CanHaveRelationship(to, relationshipType, from, allowChange, force))
			{
				return false;
			}
			break;
		case RelationshipType.InLoveWith:
			if (GetRelationship(to, from) != RelationshipType.InLoveWith && !CanHaveRelationship(to, RelationshipType.NotInLoveWith, from, allowChange, force))
			{
				return false;
			}
			break;
		}
		bool flag = false;
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget != to)
			{
				continue;
			}
			if (relationshipType == RelationshipType.None)
			{
				from.Relationships.RemoveAt(i);
			}
			else
			{
				Relationship value = from.Relationships[i];
				value.RelationshipType = relationshipType;
				if (from.IsControllableByPlayer() || to.IsControllableByPlayer())
				{
					value.KnownToPlayer = true;
				}
				from.Relationships[i] = value;
			}
			flag = true;
			break;
		}
		if (!flag && relationshipType != RelationshipType.None)
		{
			Relationship item = new Relationship
			{
				RelationshipType = relationshipType,
				RelationshipTarget = to
			};
			if (from.IsControllableByPlayer() || to.IsControllableByPlayer())
			{
				item.KnownToPlayer = true;
			}
			from.Relationships.Add(item);
		}
		if ((uint)(relationshipType - 1) <= 2u || relationshipType == RelationshipType.MarriedTo)
		{
			to.Surname = from.Surname;
			to.SurnameVerified = from.SurnameVerified;
		}
		from.ClearOpinionCacheForObject(to);
		if ((from.IsControllableByPlayer() || to.IsControllableByPlayer()) && recursion == 0)
		{
			if (!silent)
			{
				LogEventType logEventType = LogEventType.Invalid;
				string text = null;
				switch (relationshipType)
				{
				case RelationshipType.FriendsWith:
					logEventType = LogEventType.MadeFriends;
					text = LogEventBehaviour.BuildMadeFriendsMsg(from, to);
					break;
				case RelationshipType.SleepingWith:
					logEventType = LogEventType.StartedDating;
					text = LogEventBehaviour.BuildStartedDatingMsg(from, to);
					break;
				case RelationshipType.Ex:
					logEventType = LogEventType.BrokeUp;
					text = LogEventBehaviour.BuildBrokeUpMsg(from, to);
					break;
				}
				if (text != null)
				{
					HudBehaviour.Instance.SetStatusBarMsg(text);
				}
				if (logEventType != LogEventType.Invalid)
				{
					LogEvent logEvent = new LogEvent(logEventType);
					logEvent.Character = from;
					logEvent.Listener = to;
					Session.Instance.AddLogEvent(logEvent);
				}
			}
			if ((from.Rank == Rank.Leader && from.IsControllableByPlayer()) || (to.Rank == Rank.Leader && to.IsControllableByPlayer()))
			{
				switch (relationshipType)
				{
				case RelationshipType.FriendsWith:
					AchievementsManager.Instance.UnlockAchievement(Achievement.MakeAFriend);
					break;
				case RelationshipType.SleepingWith:
					AchievementsManager.Instance.UnlockAchievement(Achievement.StartRelationship);
					break;
				}
			}
		}
		bool flag2 = false;
		switch (relationshipType)
		{
		case RelationshipType.ChildOf:
			flag2 = SetRelationship(to, RelationshipType.ParentOf, from, allowChange, generating, force, recursion + 1, silent);
			break;
		case RelationshipType.ParentOf:
		{
			flag2 = SetRelationship(to, RelationshipType.ChildOf, from, allowChange, generating, force, recursion + 1, silent);
			for (int l = 0; l < from.Relationships.Count; l++)
			{
				if (from.Relationships[l].RelationshipType == RelationshipType.ParentOf && from.Relationships[l].RelationshipTarget != to && !SetRelationship(to, RelationshipType.SiblingOf, from.Relationships[l].RelationshipTarget, allowChange, generating: true, generating, recursion + 1, silent))
				{
					Debug.Log("Unable to set siblings for parent " + from.GetDisplayNameString() + ": " + to.GetDisplayNameString() + " and " + from.Relationships[l].RelationshipTarget.GetDisplayNameString());
				}
			}
			break;
		}
		case RelationshipType.SiblingOf:
		{
			flag2 = SetRelationship(to, RelationshipType.SiblingOf, from, allowChange, generating, force, recursion + 1, silent);
			for (int j = 0; j < from.Relationships.Count; j++)
			{
				if (from.Relationships[j].RelationshipType == RelationshipType.ChildOf && from.Relationships[j].RelationshipTarget != to && !SetRelationship(to, RelationshipType.ChildOf, from.Relationships[j].RelationshipTarget, allowChange, generating: true, generating, recursion + 1, silent))
				{
					Debug.Log("Unable to set parent for sibling of " + from.GetDisplayNameString() + ": " + to.GetDisplayNameString() + " and " + from.Relationships[j].RelationshipTarget.GetDisplayNameString());
				}
			}
			break;
		}
		case RelationshipType.None:
		case RelationshipType.FriendsWith:
		case RelationshipType.SleepingWith:
		case RelationshipType.Ex:
			flag2 = SetRelationship(to, relationshipType, from, allowChange, generating, force, recursion + 1, silent);
			break;
		case RelationshipType.MarriedTo:
		{
			flag2 = SetRelationship(to, relationshipType, from, allowChange, generating, force, recursion + 1, silent);
			if (!generating)
			{
				break;
			}
			for (int k = 0; k < from.Relationships.Count; k++)
			{
				if (from.Relationships[k].RelationshipType == RelationshipType.ParentOf && from.Relationships[k].RelationshipTarget != to)
				{
					SetRelationship(to, RelationshipType.ParentOf, from.Relationships[k].RelationshipTarget, allowChange, generating, force, recursion + 1, silent);
				}
			}
			break;
		}
		case RelationshipType.InLoveWith:
			flag2 = GetRelationship(to, from) == RelationshipType.InLoveWith || SetRelationship(to, RelationshipType.NotInLoveWith, from, allowChange: false, generating, force, recursion + 1, silent);
			break;
		default:
			flag2 = true;
			break;
		}
		if (!flag2)
		{
			Debug.Log("Unable to set reciprocal relationship for " + from.GetDisplayNameString() + " " + relationshipType.ToString() + " " + to.GetDisplayNameString());
			SetRelationship(from, relationshipType, to, allowChange, generating, force, recursion, silent);
		}
		return true;
	}

	public static int GetMaxAffairCount(int loyalty)
	{
		return 2 - loyalty;
	}

	public static void TrimUnreciprocatedLoveRelationships(Character character, CustomRandom rand)
	{
		int maxAffairCount = GetMaxAffairCount(character.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle));
		List<Character> list = new List<Character>();
		for (int i = 0; i < character.Relationships.Count; i++)
		{
			if (character.Relationships[i].RelationshipType == RelationshipType.InLoveWith && GetRelationship(character.Relationships[i].RelationshipTarget, character) == RelationshipType.NotInLoveWith)
			{
				list.Add(character.Relationships[i].RelationshipTarget);
			}
		}
		while (list.Count >= maxAffairCount)
		{
			int index = rand.Next(list.Count);
			Character to = list[index];
			SetRelationship(character, RelationshipType.None, to, allowChange: true, generating: false);
			list.RemoveAt(index);
		}
	}

	public static RelationshipType GetOppositeRelationshipType(RelationshipType relationshipType)
	{
		return relationshipType switch
		{
			RelationshipType.None => RelationshipType.None, 
			RelationshipType.ParentOf => RelationshipType.ChildOf, 
			RelationshipType.ChildOf => RelationshipType.ParentOf, 
			RelationshipType.SiblingOf => RelationshipType.SiblingOf, 
			RelationshipType.FriendsWith => RelationshipType.FriendsWith, 
			RelationshipType.SleepingWith => RelationshipType.SleepingWith, 
			RelationshipType.MarriedTo => RelationshipType.MarriedTo, 
			RelationshipType.InLoveWith => RelationshipType.NotInLoveWith, 
			RelationshipType.NotInLoveWith => RelationshipType.InLoveWith, 
			RelationshipType.Ex => RelationshipType.Ex, 
			_ => RelationshipType.None, 
		};
	}

	public static bool IsDescendantOf(Character a, Character b)
	{
		for (int i = 0; i < a.Relationships.Count; i++)
		{
			if (a.Relationships[i].RelationshipType == RelationshipType.ChildOf)
			{
				if (a.Relationships[i].RelationshipTarget == b)
				{
					return true;
				}
				if (IsDescendantOf(a.Relationships[i].RelationshipTarget, b))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsFamily(Character a, Character b)
	{
		bool result = IsFamily(a, b, Mark);
		Mark.Clear();
		return result;
	}

	private static bool IsFamily(Character a, Character b, List<Character> Mark)
	{
		Mark.Add(a);
		for (int i = 0; i < a.Relationships.Count; i++)
		{
			if (Mark.Contains(a.Relationships[i].RelationshipTarget))
			{
				continue;
			}
			RelationshipType relationshipType = a.Relationships[i].RelationshipType;
			if ((uint)(relationshipType - 1) <= 2u)
			{
				if (a.Relationships[i].RelationshipTarget == b)
				{
					return true;
				}
				if (IsFamily(a.Relationships[i].RelationshipTarget, b, Mark))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Character GetPartner(Character from)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			RelationshipType relationshipType = from.Relationships[i].RelationshipType;
			if ((uint)(relationshipType - 5) <= 1u)
			{
				return from.Relationships[i].RelationshipTarget;
			}
		}
		return null;
	}

	public static bool IsPartnerKnownToPlayer(Character from)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			RelationshipType relationshipType = from.Relationships[i].RelationshipType;
			if ((uint)(relationshipType - 5) <= 1u)
			{
				return from.Relationships[i].KnownToPlayer;
			}
		}
		return false;
	}

	public static void SetKnownToPlayer(Character from, Character to)
	{
		bool flag = false;
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				Relationship value = from.Relationships[i];
				value.KnownToPlayer = true;
				from.Relationships[i] = value;
				flag = value.RelationshipType != RelationshipType.InLoveWith && value.RelationshipType != RelationshipType.NotInLoveWith;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < to.Relationships.Count; j++)
		{
			if (to.Relationships[j].RelationshipTarget == from)
			{
				Relationship value2 = to.Relationships[j];
				value2.KnownToPlayer = true;
				to.Relationships[j] = value2;
			}
		}
	}

	public static bool IsKnownToPlayer(Character from, RelationshipType relationshipType, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipType == relationshipType && from.Relationships[i].RelationshipTarget == to)
			{
				return from.Relationships[i].KnownToPlayer;
			}
		}
		return false;
	}

	public static bool IsKnownToPlayer(Character from, Character to)
	{
		for (int i = 0; i < from.Relationships.Count; i++)
		{
			if (from.Relationships[i].RelationshipTarget == to)
			{
				return from.Relationships[i].KnownToPlayer;
			}
		}
		return false;
	}

	public static bool HasAnyUnknownRelationships(Character character)
	{
		for (int i = 0; i < character.Relationships.Count; i++)
		{
			if (!character.Relationships[i].KnownToPlayer)
			{
				return true;
			}
		}
		return false;
	}

	public static void SetAllRelationshipsKnown(Character character)
	{
		for (int i = 0; i < character.Relationships.Count; i++)
		{
			Relationship value = character.Relationships[i];
			value.KnownToPlayer = true;
			character.Relationships[i] = value;
		}
	}
}
