using System;
using System.Collections.Generic;
using UnityEngine;

public struct Memory : IReflectable
{
	public static string[] SecrecyModeNames = StringUtil.GetEnumNames<SecrecyMode>();

	public MemoryPrototype Prototype;

	public Character Actor;

	public BaseObject Object;

	public BaseObject ThirdParty;

	public float MoraleContribution;

	public float ApprovalContribution;

	public float RespectContribution;

	public float QuantityFactor;

	public float LastQuantity;

	public TimeSpan Time;

	public float Priority;

	public bool FakeNews;

	public bool Actioned;

	private static List<Character> _nearbyObjects = new List<Character>();

	private static GameProfiler MemorableEventTimer = new GameProfiler("Update.OnMemorableEvent");

	public static Memory Create(MemoryPrototype proto, Character actor, BaseObject obj, BaseObject thirdParty, float quantityFactor, bool fakeNews)
	{
		Memory result = default(Memory);
		result.Prototype = proto;
		result.Actor = actor;
		result.Object = obj;
		result.ThirdParty = thirdParty;
		result.MoraleContribution = 0f;
		result.ApprovalContribution = 0f;
		result.RespectContribution = 0f;
		result.Time = Session.Instance.PlayTime;
		result.Priority = 0f;
		result.LastQuantity = (result.QuantityFactor = quantityFactor);
		result.FakeNews = fakeNews;
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Prototype);
		if (Prototype == null && reflector.IsDeserialising && reflector is CustomBinaryReader customBinaryReader)
		{
			Debug.LogWarning("Missing memory type! " + customBinaryReader.LastReadName);
		}
		reflector.Add(ref Actor);
		reflector.Add(ref Object);
		reflector.AddAfter(ref ThirdParty, 327);
		reflector.Add(ref ApprovalContribution);
		reflector.Add(ref RespectContribution);
		reflector.AddAfter(ref MoraleContribution, 417);
		reflector.Add(ref Time);
		reflector.Add(ref Priority);
		reflector.Add(ref QuantityFactor);
		reflector.AddAfter(ref LastQuantity, 144);
		reflector.AddAfter(ref FakeNews, 146);
		reflector.AddAfter(ref Actioned, 232);
		if (!reflector.IsDeserialising)
		{
			return;
		}
		if (Prototype != null)
		{
			if (Prototype.QuantityLimit != 0f)
			{
				QuantityFactor = Math.Min(QuantityFactor, Prototype.QuantityLimit);
				LastQuantity = Math.Min(LastQuantity, Prototype.QuantityLimit);
			}
			if (Prototype.ApprovalLimit != 0f)
			{
				ApprovalContribution = Mathf.Sign(ApprovalContribution) * Math.Min(Math.Abs(ApprovalContribution), Prototype.ApprovalLimit);
			}
			if (Prototype.RespectLimit != 0f)
			{
				RespectContribution = Mathf.Sign(RespectContribution) * Math.Min(Math.Abs(RespectContribution), Prototype.RespectLimit);
			}
		}
		MathUtil.FixNaNorInfinity(ref ApprovalContribution);
		MathUtil.FixNaNorInfinity(ref RespectContribution);
		MathUtil.FixNaNorInfinity(ref MoraleContribution);
		MathUtil.FixNaNorInfinity(ref QuantityFactor);
	}

	public static bool operator ==(Memory a, Memory b)
	{
		if (a.Prototype == b.Prototype && a.Actor == b.Actor && a.Object == b.Object && a.ThirdParty == b.ThirdParty && a.ApprovalContribution == b.ApprovalContribution && a.RespectContribution == b.RespectContribution && a.MoraleContribution == b.MoraleContribution && a.Time == b.Time && a.Priority == b.Priority && a.QuantityFactor == b.QuantityFactor && a.LastQuantity == b.LastQuantity && a.FakeNews == b.FakeNews)
		{
			return a.Actioned == b.Actioned;
		}
		return false;
	}

	public static bool operator !=(Memory a, Memory b)
	{
		return !(a == b);
	}

	public bool Equals(Memory other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Memory)
		{
			return this == (Memory)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((Prototype != null) ? Prototype.GetHashCode() : 0) ^ ((Actor != null) ? Actor.GetHashCode() : 0) ^ ((Object != null) ? Object.GetHashCode() : 0) ^ ((ThirdParty != null) ? ThirdParty.GetHashCode() : 0);
	}

	public bool ShouldMerge(Memory other)
	{
		if (other.Object != Object)
		{
			return false;
		}
		if (Prototype != MemoryPrototype.Killed && Prototype.RuleSet != MemoryRuleSet.Alliance && other.Actor != Actor)
		{
			return false;
		}
		if (other.ThirdParty != ThirdParty)
		{
			return false;
		}
		if (other.Prototype != Prototype)
		{
			if (Prototype.Replace != null)
			{
				return Prototype.Replace.Contains(other.Prototype.UniqueID);
			}
			return false;
		}
		return true;
	}

	public void Merge(Character character, Memory other)
	{
		if (Prototype == other.Prototype)
		{
			float quantityFactor = QuantityFactor;
			QuantityFactor += other.QuantityFactor;
			if (Prototype.QuantityLimit != 0f && quantityFactor > 1E-05f)
			{
				QuantityFactor = Math.Min(QuantityFactor, Prototype.QuantityLimit);
				float num = (QuantityFactor - other.QuantityFactor) / quantityFactor;
				ApprovalContribution *= num;
				RespectContribution *= num;
				MoraleContribution *= num;
			}
			ApprovalContribution += other.ApprovalContribution;
			RespectContribution += other.RespectContribution;
			MoraleContribution += other.MoraleContribution;
			if (Prototype.ApprovalLimit != 0f)
			{
				ApprovalContribution = Mathf.Sign(ApprovalContribution) * Math.Min(Math.Abs(ApprovalContribution), Prototype.ApprovalLimit);
				float val = CalcMoraleContribution(character, Prototype.ApprovalLimit);
				MoraleContribution = Mathf.Sign(MoraleContribution) * Math.Min(Math.Abs(MoraleContribution), val);
			}
			if (Prototype.RespectLimit != 0f)
			{
				RespectContribution = Mathf.Sign(RespectContribution) * Math.Min(Math.Abs(RespectContribution), Prototype.RespectLimit);
			}
		}
	}

	public bool IsTrivial()
	{
		if ((Prototype == MemoryPrototype.LeadershipCausedHunger || Prototype == MemoryPrototype.LeadershipCausedThirst || Prototype == MemoryPrototype.LeadershipCausedHypothermia || Prototype == MemoryPrototype.LeadershipCausedLowMorale || Prototype == MemoryPrototype.LeadershipCausedSleepDeprivation || Prototype == MemoryPrototype.LeadershipCausedLackOfSpace || Prototype == MemoryPrototype.LeadershipCausedOverwork || Prototype == MemoryPrototype.Chatted) && Math.Abs(ApprovalContribution) + Math.Abs(RespectContribution) < 1f)
		{
			return true;
		}
		return false;
	}

	public bool CanBeForgotten(Character character)
	{
		switch (Prototype.CanForget)
		{
		case CanForget.No:
			return false;
		case CanForget.OnlyIfNotInvolvingMe:
			if (Actor != character && Object != character && Object != character.Community && ThirdParty != character)
			{
				return ThirdParty != character.Community;
			}
			return false;
		case CanForget.UnlessInvolvingMeAndPlayer:
			if (Actor == character || Object == character || ThirdParty == character)
			{
				if (Actor != null && Actor.IsPlayerAvatar())
				{
					return false;
				}
				if (Object != null && Object.IsPlayerAvatar())
				{
					return false;
				}
				if (ThirdParty != null && ThirdParty.IsPlayerAvatar())
				{
					return false;
				}
			}
			return true;
		case CanForget.IfNotInvolvingMeOrOld:
			if (Session.Instance.PlayTime - Time >= Sun.DayLength * 7.0)
			{
				return true;
			}
			if (Actor != character && Object != character && Object != character.Community && ThirdParty != character)
			{
				return ThirdParty != character.Community;
			}
			return false;
		default:
			return true;
		}
	}

	public int GetBystanderEmpathy(Character character, Character objectCharacter, int minLimit, int maxLimit)
	{
		if (objectCharacter == character)
		{
			return 0;
		}
		int num = 1 + character.GetPersonality(CachedPersonalityType.Compassionate, CachedPersonalityType.Detached);
		RelationshipType relationship = Relationship.GetRelationship(character, objectCharacter);
		if ((relationship != RelationshipType.None && relationship != RelationshipType.NotInLoveWith) || Relationship.IsFamily(character, objectCharacter))
		{
			num += 3 + character.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		}
		else if (objectCharacter.Community == character.Community && objectCharacter.Community != null)
		{
			if (character.Rank == Rank.Captive)
			{
				num -= 2;
			}
			else
			{
				num += 2 + character.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
				if (character.Community.IsLooterCommunity())
				{
					num--;
				}
			}
		}
		else if (objectCharacter.Community == character.InitialCommunity && objectCharacter.Community != null)
		{
			num += 1 + character.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
		}
		else
		{
			if (!objectCharacter.IsLooter())
			{
				num += character.GetPersonality(CachedPersonalityType.Moral, CachedPersonalityType.Immoral);
			}
			if (character.HasPersonality(CachedPersonalityType.Sociopath))
			{
				num -= 2;
			}
			if (objectCharacter.Community != null && objectCharacter.Community.IsLooterCommunity())
			{
				num = Math.Min(0, num);
			}
			if (character.IsLooter())
			{
				num = Math.Min(0, num);
			}
			if (character.Community != null && objectCharacter.Community != null && Session.Instance.CommunityManager.GetRelationship(character.Community, objectCharacter.Community) == CommunityRelationshipType.Hostile)
			{
				num = 0;
			}
		}
		return Math.Max(minLimit, Math.Min(num, maxLimit));
	}

	public float GetEmpathy(Character character, Character objectCharacter, float approvalForObject)
	{
		for (int i = 0; i < character.EmpathyOverrides.Count; i++)
		{
			if (character.EmpathyOverrides[i].OverrideObject == objectCharacter || character.EmpathyOverrides[i].OverrideObject == objectCharacter.Community)
			{
				return character.EmpathyOverrides[i].OverrideValue;
			}
		}
		float num = ((objectCharacter.GetBaseObjectType() == BaseObjectType.Human) ? 10f : 2f);
		if (Prototype.RuleSet == MemoryRuleSet.Restrain)
		{
			num = 0f;
		}
		float num2 = Mathf.Clamp((float)GetBystanderEmpathy(character, objectCharacter, -10, 10) * num + approvalForObject, -100f, 100f);
		if (Prototype.RuleSet == MemoryRuleSet.SelfInterest && character == Actor)
		{
			num2 = Math.Min(num2, -50f);
		}
		if (objectCharacter.Community != null && objectCharacter.Community.CommunityType == CommunityType.Psycho)
		{
			num2 = Math.Min(num2, 0f);
		}
		return num2;
	}

	public float GetBystanderRespect(Character character, Character objectCharacter, float respectForObject)
	{
		float num = 0f;
		if (objectCharacter.GetBaseObjectType() == BaseObjectType.Human)
		{
			int num2 = 0;
			for (int i = 0; i < 10; i++)
			{
				int level = objectCharacter.Skillset.GetLevel((SkillType)i);
				num2 += level - 2;
			}
			if (objectCharacter.Community != null && objectCharacter.Community.Leader == objectCharacter)
			{
				num += (float)objectCharacter.Community.GetLivingNonZombieMemberCount();
			}
		}
		return Mathf.Clamp(num + respectForObject, -100f, 100f);
	}

	public void CalcApprovalRespectContribution(Character character)
	{
		Character character2 = Object as Character;
		Community community = Object as Community;
		float num = Prototype.GoodnessBadness;
		float num2 = 0f;
		if (Prototype.RuleSet == MemoryRuleSet.Cannibal)
		{
			if (character.HasPersonality(CachedPersonalityType.Immoral))
			{
				num *= -0.1f;
			}
			else if (character.HasPersonality(CachedPersonalityType.Amoral))
			{
				num *= 0f;
			}
			else if (character.HasPersonality(CachedPersonalityType.Hypocritical) && Actor == Object)
			{
				num *= 0f;
			}
		}
		if (Prototype.RuleSet == MemoryRuleSet.MoralStand && (character.HasPersonality(CachedPersonalityType.Moral) || (character.HasPersonality(CachedPersonalityType.Hypocritical) && Object != character)))
		{
			num *= -1f;
		}
		if (character2 != null)
		{
			character.CalcApprovalRating(character2, out var approval, out var respect);
			float empathy = GetEmpathy(character, character2, approval);
			float bystanderRespect = GetBystanderRespect(character, character2, respect);
			if (Prototype.RuleSet == MemoryRuleSet.Restrain && approval > 0f && num < 0f && ThirdParty is Character)
			{
				character.CalcApprovalRating((Character)ThirdParty, out var approval2, out var _);
				if (approval2 > 0f)
				{
					num = 0f - num;
				}
			}
			ApprovalContribution = empathy / 100f * (num / 100f) * 100f;
			num2 = empathy / 100f * (Prototype.MoraleBoost / 100f) * 100f;
			if (Prototype.RuleSet == MemoryRuleSet.Envy && character2 != character && Actor != character)
			{
				float num3 = 0f;
				float num4 = QuantityFactor * num;
				for (int i = 0; i < character.Memories.Count; i++)
				{
					if (character.Memories[i].Prototype.RuleSet == MemoryRuleSet.Envy && character.Memories[i].Actor == Actor)
					{
						if (character.Memories[i].Object == character)
						{
							num3 += character.Memories[i].QuantityFactor * character.Memories[i].Prototype.GoodnessBadness;
						}
						if (character.Memories[i].Object == character2)
						{
							num4 += character.Memories[i].QuantityFactor * character.Memories[i].Prototype.GoodnessBadness;
						}
					}
				}
				float num5 = Math.Max(0f, num4 - num3);
				float num6 = 0.05f * ((float)character.GetPersonality(CachedPersonalityType.Jealous, CachedPersonalityType.Unpossessive) + 1f);
				float num7 = Mathf.Clamp(num5 * num6, 0f, QuantityFactor * num * 0.5f);
				ApprovalContribution = Math.Max(Math.Min(0f - num7, ApprovalContribution), ApprovalContribution - num7);
			}
			float num8 = ((character2 == character) ? 1f : ((character2.GetBaseObjectType() == BaseObjectType.Human) ? 0.5f : 0.1f));
			if (Prototype.DifficultyPatheticness >= 0f)
			{
				RespectContribution = (num8 + num8 * (bystanderRespect / 100f)) * (Prototype.DifficultyPatheticness / 100f) * 100f;
			}
			else
			{
				RespectContribution = (num8 - num8 * (bystanderRespect / 100f)) * (Prototype.DifficultyPatheticness / 100f) * 100f;
			}
			if (Prototype.RuleSet == MemoryRuleSet.Romantic && character2 != character && Actor != character)
			{
				if (Relationship.HasRelationship(character, RelationshipType.InLoveWith, character2) || Relationship.HasRelationship(character, RelationshipType.MarriedTo, character2) || Relationship.HasRelationship(character, RelationshipType.SleepingWith, character2) || character.HasMemory(MemoryPrototype.FlirtedSuccessfullyWith, character, character2) || character.HasMemoryAfter(MemoryPrototype.FlirtedUnsuccessfullyWith, character, character2, Session.Instance.PlayTime - Sun.DayLength))
				{
					if (ApprovalContribution > 0f)
					{
						float num9 = (character.HasPersonality(CachedPersonalityType.Jealous) ? 2f : (character.HasPersonality(CachedPersonalityType.Unpossessive) ? 0.5f : 1f));
						ApprovalContribution *= 0f - num9;
					}
				}
				else if (Relationship.HasRelationship(character, RelationshipType.InLoveWith, Actor) || Relationship.HasRelationship(character, RelationshipType.MarriedTo, Actor) || Relationship.HasRelationship(character, RelationshipType.SleepingWith, Actor) || character.HasMemory(MemoryPrototype.FlirtedSuccessfullyWith, character, Actor) || character.HasMemoryAfter(MemoryPrototype.FlirtedUnsuccessfullyWith, character, Actor, Session.Instance.PlayTime - Sun.DayLength))
				{
					if (ApprovalContribution > 0f)
					{
						float num10 = (character.HasPersonality(CachedPersonalityType.Jealous) ? 2f : (character.HasPersonality(CachedPersonalityType.Unpossessive) ? 0.5f : 1f));
						ApprovalContribution *= 0f - num10;
					}
				}
				else
				{
					ApprovalContribution *= 0.1f;
				}
			}
			else if (Prototype.RuleSet == MemoryRuleSet.Rejected && character2 != character && Actor != character)
			{
				if (Relationship.HasRelationship(character, RelationshipType.InLoveWith, character2) || Relationship.HasRelationship(character, RelationshipType.MarriedTo, character2) || Relationship.HasRelationship(character, RelationshipType.SleepingWith, character2) || character.HasMemory(MemoryPrototype.FlirtedSuccessfullyWith, character, character2) || character.HasMemoryAfter(MemoryPrototype.FlirtedUnsuccessfullyWith, character, character2, Session.Instance.PlayTime - Sun.DayLength))
				{
					if (ApprovalContribution < 0f)
					{
						float num11 = (character.HasPersonality(CachedPersonalityType.Jealous) ? 2f : (character.HasPersonality(CachedPersonalityType.Unpossessive) ? 0.5f : 1f));
						ApprovalContribution *= 0f - num11;
					}
				}
				else if (Relationship.HasRelationship(character, RelationshipType.InLoveWith, Actor) || Relationship.HasRelationship(character, RelationshipType.MarriedTo, Actor) || Relationship.HasRelationship(character, RelationshipType.SleepingWith, Actor) || character.HasMemory(MemoryPrototype.FlirtedSuccessfullyWith, character, Actor) || character.HasMemoryAfter(MemoryPrototype.FlirtedUnsuccessfullyWith, character, Actor, Session.Instance.PlayTime - Sun.DayLength))
				{
					if (ApprovalContribution < 0f)
					{
						float num12 = (character.HasPersonality(CachedPersonalityType.Jealous) ? 2f : (character.HasPersonality(CachedPersonalityType.Unpossessive) ? 0.5f : 1f));
						ApprovalContribution *= 0f - num12;
					}
				}
				else
				{
					ApprovalContribution *= 0.1f;
				}
			}
			else if (Prototype.RuleSet == MemoryRuleSet.Personal && character2 != character)
			{
				ApprovalContribution *= 0.25f;
			}
			else if (Prototype.RuleSet == MemoryRuleSet.DeclareWar && character2.Community == character.Community && character.Community != null && Actor != null && Actor.Community != character.Community)
			{
				ApprovalContribution = Math.Min(ApprovalContribution, 0f);
			}
			else if (Prototype.RuleSet == MemoryRuleSet.AlwaysApproveOfMyself && Actor == character)
			{
				ApprovalContribution = Math.Max(0f, ApprovalContribution);
			}
		}
		else if (community != null)
		{
			float num13 = 0f;
			if (community == character.Community)
			{
				num13 = ((character.Rank != Rank.Captive) ? 100f : (-100f));
			}
			else if (character.Community != null && character.Community.HasOverlappingAllianceWith(community))
			{
				num13 = 50f;
			}
			else if (!community.IsLooterCommunity() || community == character.InitialCommunity)
			{
				num13 += (float)character.GetPersonality(CachedPersonalityType.Moral, CachedPersonalityType.Immoral) * 25f;
				num13 += (float)character.GetPersonality(CachedPersonalityType.Compassionate, CachedPersonalityType.Sociopath) * 25f;
			}
			else if (community.IsLooterCommunity())
			{
				num13 = ((community.GetRelationship(character.Community) != CommunityRelationshipType.Hostile) ? (-25f - (float)character.GetPersonality(CachedPersonalityType.Aggressive, CachedPersonalityType.Passive) * 25f) : (-50f - (float)character.GetPersonality(CachedPersonalityType.Aggressive, CachedPersonalityType.Passive) * 25f));
			}
			if (character.IsLooter())
			{
				num13 -= 25f;
			}
			if (Prototype.RuleSet == MemoryRuleSet.Alliance && character.Community != null && Actor.Community != null && !character.Community.HasOverlappingAllianceWith(community) && !character.Community.HasOverlappingAllianceWith(Actor.Community))
			{
				if (character.Community.Nemesis || character.Community.GetRelationship(community) == CommunityRelationshipType.Hostile)
				{
					num13 += -100f;
				}
				else if (character.Community.IsLooterCommunity() != community.IsLooterCommunity())
				{
					num13 += -50f;
				}
			}
			character.CalcApprovalRating(community, out var approval3, out var _);
			num13 += approval3;
			num13 = Mathf.Clamp(num13, -100f, 100f);
			ApprovalContribution = num13 / 100f * (num / 100f) * 100f;
			num2 = num13 / 100f * (Prototype.MoraleBoost / 100f) * 100f;
			RespectContribution = Prototype.DifficultyPatheticness;
		}
		else
		{
			ApprovalContribution = num;
			num2 = Prototype.MoraleBoost;
			RespectContribution = Prototype.DifficultyPatheticness;
		}
		ApprovalContribution *= QuantityFactor;
		RespectContribution *= QuantityFactor;
		num2 *= QuantityFactor;
		MoraleContribution = CalcMemoryMoraleContribution(character) + num2;
		if (Prototype.RuleSet == MemoryRuleSet.Defeated && community != null && community.IsAlwaysHostileToPlayerCommunity())
		{
			ApprovalContribution = Math.Min(0f, ApprovalContribution);
		}
		if (character.Rank == Rank.Captive && Actor != character && Actor != null && Actor.Community == character.Community)
		{
			ApprovalContribution = Math.Min(0f, ApprovalContribution);
		}
		if (Prototype.RuleSet == MemoryRuleSet.OnlyNegative)
		{
			ApprovalContribution = Math.Min(0f, ApprovalContribution);
			RespectContribution = Math.Min(0f, RespectContribution);
			MoraleContribution = Math.Min(0f, MoraleContribution);
		}
		if (Prototype.RuleSet == MemoryRuleSet.Helped && RespectContribution < 0f && Actor != null)
		{
			if ((character2 != null && Actor.IsInMyCommunityOrAlly(character2)) || (community != null && Actor.IsMyCommunityOrAlly(community)))
			{
				RespectContribution *= 0.25f;
			}
			else if ((character2 != null && character2.Community != null && character2.Community.IsLooterCommunity()) || (community != null && community.IsLooterCommunity()))
			{
				RespectContribution *= 1.5f;
			}
		}
	}

	public float CalcMemoryMoraleContribution(Character character)
	{
		float num = CalcMoraleContribution(character, ApprovalContribution);
		bool num2 = character.HasPersonality(CachedPersonalityType.Cynical);
		bool flag = character.HasPersonality(CachedPersonalityType.Idealistic);
		if (num2)
		{
			num *= ((num <= 0f) ? 1.5f : 0.66667f);
		}
		if (flag)
		{
			num *= ((num >= 0f) ? 1.5f : 0.66667f);
		}
		return num * Prototype.MoraleMultiplier;
	}

	public static float CalcMoraleContribution(Character character, float approvalContribution)
	{
		bool num = character.HasPersonality(CachedPersonalityType.Unflappable);
		bool flag = character.HasPersonality(CachedPersonalityType.Emotional);
		bool flag2 = character.HasPersonality(CachedPersonalityType.Bipolar);
		float num2 = approvalContribution * 0.25f;
		if (num)
		{
			num2 *= 0.5f;
		}
		if (flag)
		{
			num2 *= 2f;
		}
		if (flag2)
		{
			num2 *= 8f;
		}
		return num2;
	}

	public void CalcPriority(Character character)
	{
		float num = Math.Abs(ApprovalContribution) + Math.Abs(RespectContribution);
		num += Math.Abs(MoraleContribution);
		if (Object == character)
		{
			num *= 2f;
		}
		else if (Object is TileObject && (Object as TileObject).GetCommunity() == character.Community && character.Community != null)
		{
			num *= 1.75f;
		}
		else if (Actor == character)
		{
			num *= 1.5f;
		}
		else if (Actor != null && Actor.GetCommunity() == character.Community && character.Community != null)
		{
			num *= 1.25f;
		}
		float num2 = (float)(Session.Instance.PlayTime - Time).TotalSeconds / Sun.DayLengthSecs;
		Priority = num - (float)Math.Tanh(num2 * 2f / 28f) * (10f + num * 0.5f);
	}

	private static bool CanSeeMemorableEvent(MemoryPrototype proto, Character nearbyCharacter, Character sub, BaseObject ob)
	{
		if (proto.ActorMustBeLeader)
		{
			return true;
		}
		if (nearbyCharacter == sub)
		{
			return true;
		}
		if (!nearbyCharacter.AliveAndNotZombie)
		{
			return false;
		}
		if (!nearbyCharacter.IsAwake)
		{
			return false;
		}
		if (sub != null)
		{
			if (!sub.IsOutdoors())
			{
				return true;
			}
		}
		else if (ob is Character character && !character.IsOutdoors())
		{
			return true;
		}
		TileObject tileObject = ((sub != null) ? sub : (ob as TileObject));
		return ((tileObject != null && tileObject != nearbyCharacter) ? nearbyCharacter.GetTarget(tileObject) : null)?.FullyTracked ?? false;
	}

	private static bool CanAnyoneInCommunitySeeMemorableEvent(MemoryPrototype proto, Community community, Character sub, BaseObject ob, List<Character> nearbyObjects)
	{
		if (community.CommunityType == CommunityType.Player)
		{
			return true;
		}
		foreach (Character member in community.Members)
		{
			if (nearbyObjects.Contains(member))
			{
				return true;
			}
			if (CanSeeMemorableEvent(proto, member, sub, ob))
			{
				return true;
			}
		}
		return false;
	}

	public static void OnMemorableEvent(MemoryPrototype proto, Character sub, BaseObject ob, float quantityFactor, bool secret)
	{
		OnMemorableEvent(proto, sub, ob, null, quantityFactor, secret ? SecrecyMode.OnlyKnownToObject : SecrecyMode.Public, null, fakeNews: false);
	}

	public static void OnMemorableEvent(MemoryPrototype proto, Character sub, BaseObject ob, float quantityFactor, bool secret, Character witness)
	{
		OnMemorableEvent(proto, sub, ob, null, quantityFactor, secret ? SecrecyMode.OnlyKnownToObject : SecrecyMode.Public, witness, fakeNews: false);
	}

	public static void OnMemorableEvent(MemoryPrototype proto, Character sub, BaseObject ob, float quantityFactor, SecrecyMode secrecyMode)
	{
		OnMemorableEvent(proto, sub, ob, null, quantityFactor, secrecyMode, null, fakeNews: false);
	}

	public static void OnMemorableEvent(MemoryPrototype proto, Character sub, BaseObject ob, BaseObject thirdParty, float quantityFactor, SecrecyMode secrecyMode, Character witness)
	{
		OnMemorableEvent(proto, sub, ob, thirdParty, quantityFactor, secrecyMode, witness, fakeNews: false);
	}

	public static void OnMemorableEvent(MemoryPrototype proto, Character sub, BaseObject ob, BaseObject thirdParty, float quantityFactor, SecrecyMode secrecyMode, Character witness, bool fakeNews, TileObject propertyThatWasDamaged = null, Community ignoredByCommunity = null)
	{
		if (proto == null || secrecyMode == SecrecyMode.Private)
		{
			return;
		}
		using (new ProfileMarker(MemorableEventTimer))
		{
			CharacterManager.DeterministicTimeSpentThinking += CharacterManager.ThinkTimeUnit;
			Character character = ob as Character;
			if (character != null)
			{
				if (proto == MemoryPrototype.FlirtedSuccessfullyWith)
				{
					RelationshipType relationship = Relationship.GetRelationship(sub, (Character)ob);
					RelationshipType relationship2 = Relationship.GetRelationship((Character)ob, sub);
					if (relationship == RelationshipType.None || relationship == RelationshipType.NotInLoveWith || relationship2 == RelationshipType.NotInLoveWith)
					{
						Relationship.TrimUnreciprocatedLoveRelationships(sub, Session.Instance.DeterministicRand);
						Relationship.SetRelationship(sub, RelationshipType.InLoveWith, character, allowChange: true, generating: false);
						Relationship.SetRelationship(character, RelationshipType.InLoveWith, sub, allowChange: true, generating: false);
					}
				}
				if (proto == MemoryPrototype.FlirtedUnsuccessfullyWith)
				{
					RelationshipType relationship3 = Relationship.GetRelationship(sub, (Character)ob);
					RelationshipType relationship4 = Relationship.GetRelationship((Character)ob, sub);
					if (relationship3 == RelationshipType.None || relationship3 == RelationshipType.NotInLoveWith || relationship4 == RelationshipType.InLoveWith)
					{
						Relationship.TrimUnreciprocatedLoveRelationships(sub, Session.Instance.DeterministicRand);
						Relationship.SetRelationship(sub, RelationshipType.InLoveWith, character, allowChange: true, generating: false);
						Relationship.SetRelationship(character, RelationshipType.NotInLoveWith, sub, allowChange: true, generating: false);
					}
				}
			}
			_nearbyObjects.Clear();
			TerrainCoord terrainCoord = TerrainCoord.Invalid;
			if (sub != null)
			{
				terrainCoord = sub.Tile;
			}
			else if (ob is TileObject)
			{
				terrainCoord = ((TileObject)ob).GetCentreTile();
			}
			if (terrainCoord != TerrainCoord.Invalid)
			{
				GameTerrain.Instance.CharacterMapWho.GetObjectsInRect(terrainCoord - new TerrainCoord(32, 32), terrainCoord + new TerrainCoord(32, 32), _nearbyObjects);
			}
			for (int num = _nearbyObjects.Count - 1; num >= 0; num--)
			{
				Character nearbyCharacter = _nearbyObjects[num];
				if (!CanSeeMemorableEvent(proto, nearbyCharacter, sub, ob))
				{
					_nearbyObjects.RemoveAt(num);
				}
			}
			if (character != null && character.AliveAndNotZombie && proto.WitnessedByObject && !_nearbyObjects.Contains(character))
			{
				_nearbyObjects.Add(character);
			}
			if (witness != null && witness.AliveAndNotZombie && !_nearbyObjects.Contains(witness))
			{
				_nearbyObjects.Add(witness);
			}
			OnMemorableEvent(_nearbyObjects, proto, sub, ob, thirdParty, quantityFactor, secrecyMode, fakeNews, propertyThatWasDamaged, ignoredByCommunity);
			MemoryPrototype memoryPrototype = null;
			if (!string.IsNullOrEmpty(proto.ReciprocalMemoryID) && character != null)
			{
				memoryPrototype = GameImpl.Instance.FindMemoryPrototypeByUniqueID(proto.ReciprocalMemoryID);
				if (memoryPrototype != null)
				{
					OnMemorableEvent(_nearbyObjects, memoryPrototype, character, sub, thirdParty, quantityFactor, secrecyMode, fakeNews, propertyThatWasDamaged, ignoredByCommunity);
				}
			}
			_nearbyObjects.Clear();
		}
	}

	private static void OnMemorableEvent(List<Character> nearbyObjects, MemoryPrototype proto, Character sub, BaseObject ob, BaseObject thirdParty, float quantityFactor, SecrecyMode secrecyMode, bool fakeNews, TileObject propertyThatWasDamaged = null, Community ignoredByCommunity = null)
	{
		Community community = sub?.GetCommunity();
		Community community2 = ob?.GetCommunity();
		if (proto.SpreadToAllSubjectCommunity && community != null)
		{
			foreach (Character member in sub.Community.Members)
			{
				if (member.AliveAndNotZombie && !nearbyObjects.Contains(member))
				{
					nearbyObjects.Add(member);
				}
			}
		}
		if (proto.SpreadToAllObjectCommunity && community2 != null && CanAnyoneInCommunitySeeMemorableEvent(proto, community2, sub, ob, nearbyObjects))
		{
			foreach (Character member2 in community2.Members)
			{
				if (member2.AliveAndNotZombie && !nearbyObjects.Contains(member2))
				{
					nearbyObjects.Add(member2);
				}
			}
		}
		if (proto.SpreadToAllCommunities)
		{
			foreach (Character character in Session.Instance.CharacterManager.Characters)
			{
				if (character.AliveAndNotZombie && !nearbyObjects.Contains(character))
				{
					nearbyObjects.Add(character);
				}
			}
		}
		foreach (Character nearbyObject in nearbyObjects)
		{
			if ((nearbyObject == sub && !proto.RememberedBySubject) || (ignoredByCommunity != null && ignoredByCommunity == nearbyObject.Community))
			{
				continue;
			}
			switch (secrecyMode)
			{
			case SecrecyMode.OnlyKnownToSubject:
				if (nearbyObject != sub)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToObject:
				if (nearbyObject != ob)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToSubjectAndObject:
				if (nearbyObject != sub && nearbyObject != ob)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToSubjectCommunity:
				if (nearbyObject.Community == null || nearbyObject.Community != community)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToObjectCommunity:
				if (nearbyObject.Community == null || nearbyObject.Community != community2)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToSubjectCommunityAndObject:
				if (nearbyObject != ob && (nearbyObject.Community == null || nearbyObject.Community != community))
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByObject:
				if (nearbyObject.Community == null || nearbyObject.Community != community || nearbyObject == ob)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByThirdParty:
				if (nearbyObject.Community == null || nearbyObject.Community != community || nearbyObject == thirdParty)
				{
					continue;
				}
				break;
			case SecrecyMode.OnlyKnownToObjectCommunityIgnoredByThirdParty:
				if (nearbyObject.Community == null || nearbyObject.Community != community2 || nearbyObject == thirdParty)
				{
					continue;
				}
				break;
			case SecrecyMode.IgnoredBySubjectCommunity:
				if (nearbyObject.Community != null && nearbyObject.Community == community)
				{
					continue;
				}
				break;
			case SecrecyMode.IgnoredByObjectCommunity:
				if (nearbyObject.Community != null && nearbyObject.Community == community2)
				{
					continue;
				}
				break;
			case SecrecyMode.IgnoredByObject:
				if (nearbyObject == ob)
				{
					continue;
				}
				break;
			case SecrecyMode.IgnoredByThirdParty:
				if (nearbyObject == thirdParty)
				{
					continue;
				}
				break;
			}
			nearbyObject.AddMemory(proto, sub, ob, thirdParty, quantityFactor, fakeNews, null, forceAdd: false);
		}
	}

	public static bool IsKnownToSubjectCommunity(SecrecyMode secrecyMode)
	{
		switch (secrecyMode)
		{
		case SecrecyMode.Public:
		case SecrecyMode.OnlyKnownToSubjectCommunity:
		case SecrecyMode.OnlyKnownToSubjectCommunityAndObject:
		case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByObject:
		case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByThirdParty:
		case SecrecyMode.IgnoredByObjectCommunity:
		case SecrecyMode.IgnoredByObject:
		case SecrecyMode.IgnoredByThirdParty:
			return true;
		case SecrecyMode.OnlyKnownToSubject:
		case SecrecyMode.OnlyKnownToObject:
		case SecrecyMode.OnlyKnownToSubjectAndObject:
		case SecrecyMode.OnlyKnownToObjectCommunity:
		case SecrecyMode.OnlyKnownToObjectCommunityIgnoredByThirdParty:
		case SecrecyMode.IgnoredBySubjectCommunity:
		case SecrecyMode.Private:
			return false;
		default:
			return false;
		}
	}

	public static bool IsKnownToObjectCommunity(SecrecyMode secrecyMode)
	{
		switch (secrecyMode)
		{
		case SecrecyMode.Public:
		case SecrecyMode.OnlyKnownToObjectCommunity:
		case SecrecyMode.OnlyKnownToObjectCommunityIgnoredByThirdParty:
		case SecrecyMode.IgnoredBySubjectCommunity:
		case SecrecyMode.IgnoredByThirdParty:
			return true;
		case SecrecyMode.OnlyKnownToSubject:
		case SecrecyMode.OnlyKnownToObject:
		case SecrecyMode.OnlyKnownToSubjectAndObject:
		case SecrecyMode.OnlyKnownToSubjectCommunity:
		case SecrecyMode.OnlyKnownToSubjectCommunityAndObject:
		case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByObject:
		case SecrecyMode.OnlyKnownToSubjectCommunityIgnoredByThirdParty:
		case SecrecyMode.IgnoredByObjectCommunity:
		case SecrecyMode.IgnoredByObject:
		case SecrecyMode.Private:
			return false;
		default:
			return false;
		}
	}
}
