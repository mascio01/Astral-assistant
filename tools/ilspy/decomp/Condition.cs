using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public struct Condition : IScriptListItem<Condition>
{
	public static string[] TypeNames = StringUtil.GetEnumNames<ConditionType>();

	public static string[] SpecifierNames = StringUtil.GetEnumNames<Specifier>();

	public static string[] ModifierNames = StringUtil.GetEnumNames<SpecifierModifier>();

	[XmlAttribute]
	public ConditionType Type;

	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier Subject;

	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier Object;

	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier ThirdParty;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier SubjectModifier;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier ObjectModifier;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier ThirdPartyModifier;

	[XmlAttribute]
	public string SubjectID;

	[XmlAttribute]
	public string ObjectID;

	[XmlAttribute]
	public string ThirdPartyID;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float Data;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float Data2;

	[XmlAttribute]
	public string StringData;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool BoolData;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool Not;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float PriorityFactor;

	public List<ConditionBlockRef> ConditionBlocks;

	public Condition FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		Condition result = this;
		FixupSpecifier(ref result.Subject, ref result.SubjectModifier, ref result.SubjectID);
		FixupSpecifier(ref result.Object, ref result.ObjectModifier, ref result.ObjectID);
		FixupSpecifier(ref result.ThirdParty, ref result.ThirdPartyModifier, ref result.ThirdPartyID);
		if (result.ConditionBlocks != null)
		{
			for (int i = 0; i < result.ConditionBlocks.Count; i++)
			{
				result.ConditionBlocks[i] = result.ConditionBlocks[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (result.Type == ConditionType.ScriptedMoveHasSucceeded && !string.IsNullOrEmpty(result.ObjectID) && result.Object == Specifier.Invalid)
		{
			result.Object = Specifier.ID;
		}
		if (result.ConditionBlocks != null && result.ConditionBlocks.Count == 0)
		{
			result.ConditionBlocks = null;
		}
		return result;
	}

	public string GetTypeName()
	{
		return "Condition";
	}

	public void Construct()
	{
	}

	public static bool HasSubject(ConditionType conditionType, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (conditionType)
		{
		case ConditionType.Always:
		case ConditionType.And:
		case ConditionType.Or:
		case ConditionType.IsConditionBlockSatisfied:
		case ConditionType.ConditionBlockResult:
			hasSpecifier = false;
			return false;
		case ConditionType.GateIsOpen:
		case ConditionType.GateStateEquals:
			hasSpecifier = false;
			return true;
		case ConditionType.BuildingOfTypeIsOnMarker:
		case ConditionType.IsBuilt:
		case ConditionType.IsGapInWall:
			hasSpecifier = false;
			return true;
		case ConditionType.QuestIsActiveWithAnyParams:
		case ConditionType.QuestIsDiscoveredWithAnyParams:
		case ConditionType.QuestGroupIsActiveWithAnyParams:
		case ConditionType.WorldHasAnyUnownedVehicles:
		case ConditionType.WorldHasAnyUnownedBuildings:
		case ConditionType.IsShowingDialogueOptions:
		case ConditionType.PresenceIsAtLeast:
		case ConditionType.PresenceIsLessThan:
		case ConditionType.HasDeerOnMap:
		case ConditionType.HasDiscoveredDeer:
		case ConditionType.IsInvisibleStrainEnabled:
		case ConditionType.AreSaveTokensEnabled:
		case ConditionType.ShouldTradersHaveSaveTokens:
		case ConditionType.IsTemperatureLessThan:
		case ConditionType.IsTemperatureAtLeast:
		case ConditionType.IsSnowCoverLessThan:
		case ConditionType.IsSnowCoverAtLeast:
		case ConditionType.IsDay:
		case ConditionType.IsNight:
		case ConditionType.IsSnowing:
		case ConditionType.IsRaining:
		case ConditionType.HasSpokenOnce:
		case ConditionType.IsHitTheRoadCountAtLeast:
		case ConditionType.DayOfYearIsOnOrAfter:
		case ConditionType.DayOfYearIsBefore:
		case ConditionType.DaysSinceGameStartAtLeast:
		case ConditionType.WorldHasNItemsOfType:
		case ConditionType.IsLoneWolf:
		case ConditionType.SurvivorCampDensityIsAtLeast:
		case ConditionType.HasConqueredArea:
		case ConditionType.MemoryType:
		case ConditionType.MemoryApprovalBetween:
		case ConditionType.MemoryApprovalAtLeast:
		case ConditionType.MemoryApprovalLessThan:
		case ConditionType.MemoryRespectBetween:
		case ConditionType.MemoryRespectAtLeast:
		case ConditionType.MemoryRespectLessThan:
		case ConditionType.MemoryMoraleBetween:
		case ConditionType.MemoryMoraleAtLeast:
		case ConditionType.MemoryMoraleLessThan:
		case ConditionType.MemoryQuantityFactorBetween:
		case ConditionType.MemoryQuantityFactorAtLeast:
		case ConditionType.MemoryQuantityFactorLessThan:
		case ConditionType.MemoryIsRecent:
		case ConditionType.Presence:
		case ConditionType.HitTheRoadCount:
		case ConditionType.DaysSinceGameStart:
			hasSpecifier = false;
			return false;
		case ConditionType.QuestIsActive:
		case ConditionType.QuestIsDiscovered:
		case ConditionType.QuestIsDiscoveredGivenByWithSeeker:
		case ConditionType.QuestIsCompleted:
		case ConditionType.QuestIsFailed:
		case ConditionType.QuestGroupIsFailed:
		case ConditionType.QuestGroupIsActiveGivenBy:
		case ConditionType.QuestGroupIsActiveGivenByWithSeeker:
		case ConditionType.TimeSinceQuestStarted:
			title = "Quest Giver";
			hasSpecifier = true;
			return true;
		case ConditionType.CanAffordMedicalAttentionFrom:
		case ConditionType.CanAffordPrice:
		case ConditionType.CanAffordObject:
		case ConditionType.CanAffordItemOfType:
		case ConditionType.CanAffordToRefundItemOfType:
		case ConditionType.CanAffordWater:
		case ConditionType.CanAffordFood:
			title = "Buyer";
			hasSpecifier = true;
			return true;
		case ConditionType.HasAnyReplies:
		case ConditionType.HasSpokenSpeech:
		case ConditionType.HasSpokenSpeechRecently:
		case ConditionType.HasSpokenSpeechToAnyoneRecently:
		case ConditionType.HasSpokenSpeechForSituationRecently:
		case ConditionType.HasSpokenSpeechForSituationToAnyoneRecently:
			title = "Speaker";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = true;
			return true;
		}
	}

	public static bool HasObject(ConditionType conditionType, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (conditionType)
		{
		case ConditionType.IsInZone:
			hasSpecifier = false;
			return true;
		case ConditionType.Equals:
		case ConditionType.IsInSameSquad:
		case ConditionType.AreCommunityMembersNearby:
		case ConditionType.AreApparentlyAloneTogether:
		case ConditionType.IsMemberOf:
		case ConditionType.IsLeaderOf:
		case ConditionType.WasInitiallyMemberOf:
		case ConditionType.AreMembersOfSameCommunity:
		case ConditionType.IsAttractedTo:
		case ConditionType.AreSameGender:
		case ConditionType.HasFood:
		case ConditionType.HasDrink:
		case ConditionType.HasAlcoholicDrink:
		case ConditionType.HasAlcoholAmountTogether:
		case ConditionType.HasItem:
		case ConditionType.CanShareItemWith:
		case ConditionType.CanTreat:
		case ConditionType.CommunityOccupiesBuilding:
		case ConditionType.CommunityOwnsBuilding:
		case ConditionType.LikesGift:
		case ConditionType.DislikesGift:
		case ConditionType.KnowsName:
		case ConditionType.VariableEquals:
		case ConditionType.VariableIsGreaterThan:
		case ConditionType.VariableIsLessThan:
		case ConditionType.VariableIsGreaterThanOrEqual:
		case ConditionType.VariableIsLessThanOrEqual:
		case ConditionType.IsOlderThan:
		case ConditionType.AgeDifferenceIsLessThan:
		case ConditionType.IsInDatingAgeRange:
		case ConditionType.CanBandageInjury:
		case ConditionType.HasAntigenFor:
		case ConditionType.CommunityIntroductionHasStarted:
		case ConditionType.CommunityIsIntroduced:
		case ConditionType.CommunityIsAtWar:
		case ConditionType.CommunityHasCeasedFire:
		case ConditionType.CommunityIsDefeated:
		case ConditionType.CommunityIsAtWarOrDefeated:
		case ConditionType.CommunityHasAlliance:
		case ConditionType.CommunityHasAllianceOrDestroyed:
		case ConditionType.CommunityHasAnyMembersWhoWereInitiallyMembersOf:
		case ConditionType.CommunitySizeIsLargest:
		case ConditionType.CommunityIsTradingAtCamp:
		case ConditionType.SquadBehaviourEquals:
		case ConditionType.ScriptedMoveHasSucceeded:
		case ConditionType.WantJoinCommunity:
		case ConditionType.AnyoneWantsToJoinCommunity:
		case ConditionType.WantJoinCommunityWhenThreatened:
		case ConditionType.WantLeaveCommunity:
		case ConditionType.WantKickFromCommunity:
		case ConditionType.WantToDeclareWar:
		case ConditionType.DistanceToIsLessThan:
		case ConditionType.HasEnoughGoldForBoxerWager:
		case ConditionType.IsGatheringFrom:
		case ConditionType.HasRelationship:
		case ConditionType.HasRomanticRelationship:
		case ConditionType.HasGoodRelationship:
		case ConditionType.HasGoodRelationshipsInCommunity:
		case ConditionType.IsRelationshipKnownToPlayer:
		case ConditionType.CanFlirt:
		case ConditionType.Approves:
		case ConditionType.Respects:
		case ConditionType.ApprovalRespectAngleBetween:
		case ConditionType.ApprovalRespectAmountBetween:
		case ConditionType.ApprovalRespectAmountAtLeast:
		case ConditionType.ApprovalRespectAmountLessThan:
		case ConditionType.ApprovalRespectAbsoluteSum:
		case ConditionType.ApprovalSinceLastFight:
		case ConditionType.ApprovedBeforeThisMemory:
		case ConditionType.ApprovalSince:
		case ConditionType.RespectSince:
		case ConditionType.HasExceededLimitForMemoryType:
		case ConditionType.HasAnyMemoryOlderThan:
		case ConditionType.PartnerHasRelationship:
		case ConditionType.PartnerHasRelationshipKnownToPlayer:
		case ConditionType.IsWarmerThatCurrentClothing:
		case ConditionType.IsCarrying:
		case ConditionType.IsCarryingCommunityMember:
		case ConditionType.IsTimeToShakedownAgain:
		case ConditionType.IsFullyTracked:
		case ConditionType.IsCloseToBase:
		case ConditionType.IsInsidBasePerimeter:
		case ConditionType.TastinessIsAtLeast:
		case ConditionType.IsOutnumberedBy:
		case ConditionType.CanSeeNumberOfCommunityMembers:
		case ConditionType.VisibleCommunityMembersHaveGold:
		case ConditionType.VotedFor:
		case ConditionType.IsMostLikedCommunityMember:
		case ConditionType.Approval:
		case ConditionType.Respect:
		case ConditionType.ApprovalRespectSum:
		case ConditionType.Variable:
		case ConditionType.Distance:
			hasSpecifier = true;
			return true;
		case ConditionType.HasMemory:
		case ConditionType.HasMemoryQuantityBetween:
		case ConditionType.HasMemoryQuantityAtLeast:
		case ConditionType.HasMemoryQuantityLessThan:
		case ConditionType.HasRecentMemory:
		case ConditionType.HasRecentMemoryWhereObjectIsAnyoneInCommunity:
		case ConditionType.MemoryQuantityForCommunity:
			title = "Memory Actor";
			hasSpecifier = true;
			return true;
		case ConditionType.HasRecentMemoryOfAnyoneInCommunity:
			title = "Memory Actor Community";
			hasSpecifier = true;
			return true;
		case ConditionType.QuestIsActive:
		case ConditionType.QuestIsDiscovered:
		case ConditionType.QuestIsCompleted:
		case ConditionType.QuestIsFailed:
		case ConditionType.QuestGroupIsFailed:
		case ConditionType.TimeSinceQuestStarted:
			title = "Quest Object";
			hasSpecifier = true;
			return true;
		case ConditionType.IsInvaderWithSource:
			title = "Invader Source";
			hasSpecifier = true;
			return true;
		case ConditionType.CanAffordMedicalAttentionFrom:
		case ConditionType.CanAffordPrice:
		case ConditionType.CanAffordObject:
		case ConditionType.CanAffordItemOfType:
		case ConditionType.CanAffordToRefundItemOfType:
		case ConditionType.CanAffordWater:
		case ConditionType.CanAffordFood:
			title = "Seller";
			hasSpecifier = true;
			return true;
		case ConditionType.HasAnyReplies:
		case ConditionType.HasSpokenSpeech:
		case ConditionType.HasSpokenSpeechRecently:
		case ConditionType.HasSpokenSpeechForSituationRecently:
			title = "Listener";
			hasSpecifier = true;
			return true;
		case ConditionType.IsObjectInInventory:
			title = "Item";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasThirdParty(ConditionType conditionType, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (conditionType)
		{
		case ConditionType.HasAnyReplies:
			hasSpecifier = true;
			return true;
		case ConditionType.HasMemory:
		case ConditionType.HasMemoryQuantityBetween:
		case ConditionType.HasMemoryQuantityAtLeast:
		case ConditionType.HasMemoryQuantityLessThan:
		case ConditionType.HasRecentMemory:
		case ConditionType.HasRecentMemoryOfAnyoneInCommunity:
		case ConditionType.MemoryQuantityForCommunity:
			title = "Memory Object";
			hasSpecifier = true;
			return true;
		case ConditionType.HasRecentMemoryWhereObjectIsAnyoneInCommunity:
			title = "Memory Object Community";
			hasSpecifier = true;
			return true;
		case ConditionType.QuestIsActive:
		case ConditionType.QuestIsDiscovered:
		case ConditionType.QuestIsDiscoveredGivenByWithSeeker:
		case ConditionType.QuestIsCompleted:
		case ConditionType.QuestIsFailed:
		case ConditionType.QuestGroupIsFailed:
		case ConditionType.QuestGroupIsActiveGivenByWithSeeker:
		case ConditionType.TimeSinceQuestStarted:
			title = "Quest Seeker";
			hasSpecifier = true;
			return true;
		case ConditionType.CanShareItemWith:
			title = "Item";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasBoolData(ConditionType conditionType, out string title)
	{
		title = null;
		switch (conditionType)
		{
		case ConditionType.AreCommunityMembersNearby:
			title = "Ignore Those Indoors";
			return true;
		case ConditionType.HasFood:
		case ConditionType.HasDrink:
			title = "For Sharing";
			return true;
		case ConditionType.HasAlcoholicDrink:
		case ConditionType.HasAlcoholAmountTogether:
			title = "Ignore Policy";
			return true;
		case ConditionType.HasRelationshipWithAnyone:
		case ConditionType.HasRomanticRelationshipWithAnyone:
			title = "Living Only";
			return true;
		case ConditionType.CommunitySizeIsLargest:
		case ConditionType.CommunitySize:
			title = "Include Allies";
			return true;
		case ConditionType.HasAnythingEdible:
			title = "For Sale";
			return true;
		case ConditionType.WantJoinCommunity:
		case ConditionType.AnyoneWantsToJoinCommunity:
		case ConditionType.WantLeaveCommunity:
		case ConditionType.WantKickFromCommunity:
			title = "Check Relationships";
			return true;
		case ConditionType.HasGoodRelationshipsInCommunity:
			title = "Recurse";
			return true;
		case ConditionType.HasGun:
			title = "Loaded";
			return true;
		case ConditionType.CommunityHasVehicleToHitTheRoad:
			title = "Check Fuel";
			return true;
		case ConditionType.IsMostLikedCommunityMember:
			title = "Ignore Players";
			return true;
		case ConditionType.CanFlirt:
			title = "Ignore Community";
			return true;
		case ConditionType.CommunityHasAnyConsciousMembers:
			title = "Include Drunk";
			return true;
		case ConditionType.CommunityHasGates:
			title = "Must Enclose Something";
			return true;
		case ConditionType.CommunityHasAnyoneCraftingItem:
			title = "Must Have Zone";
			return true;
		default:
			return false;
		}
	}

	public static bool HasStringData(ConditionType conditionType, out string title, out List<string> options, out bool wantSort)
	{
		GameImpl instance = GameImpl.Instance;
		options = null;
		title = null;
		wantSort = true;
		switch (conditionType)
		{
		case ConditionType.HasID:
			title = "Unique ID:";
			return true;
		case ConditionType.HasNItemsOfType:
		case ConditionType.HasNGiftedItemsOfType:
		case ConditionType.HasTradedNItemsOfType:
		case ConditionType.LikesGiftType:
		case ConditionType.HasReadBook:
		case ConditionType.CanAffordItemOfType:
		case ConditionType.CanAffordToRefundItemOfType:
		case ConditionType.CommunityHasNItemsOfType:
		case ConditionType.CommunityHasNMembersWithGiftedItemsOfType:
		case ConditionType.CommunityHasAnyoneCraftingItem:
		case ConditionType.CommunityHasAnyGuardWithItem:
		case ConditionType.IsEquipped:
		case ConditionType.IsGatheringItemType:
		case ConditionType.IsEquipmentType:
		case ConditionType.WorldHasNItemsOfType:
		case ConditionType.ItemCountOfType:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in currentStory.EquipmentPrototypes)
				{
					EquipmentPrototype value = equipmentPrototype.Value;
					if ((conditionType != ConditionType.HasReadBook || value.TypeName == BaseObjectType.Book) && (conditionType != ConditionType.IsEquipped || value.CanBeEquipped) && !options.Contains(equipmentPrototype.Key))
					{
						options.Add(equipmentPrototype.Key);
					}
				}
			}
			title = "Item:";
			return true;
		case ConditionType.HasLiquidOfType:
		case ConditionType.LikesGiftLiquid:
		case ConditionType.IsLiquidType:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory2 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, LiquidPrototype> liquidPrototype in currentStory2.LiquidPrototypes)
				{
					if (!options.Contains(liquidPrototype.Key))
					{
						options.Add(liquidPrototype.Key);
					}
				}
			}
			title = "Liquid:";
			return true;
		case ConditionType.HasNItemsOfClothingType:
		case ConditionType.CommunityHasNItemsOfClothingType:
		case ConditionType.IsWearingSomething:
		case ConditionType.IsClothingType:
			options = new List<string>(Character.ClothingTypeNames);
			options.Insert(0, "Invalid");
			wantSort = false;
			title = "Clothing Type:";
			return true;
		case ConditionType.IsBuildingOfTypeUnlocked:
		case ConditionType.IsPropType:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory3 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, PropPrototype> propPrototype in currentStory3.PropPrototypes)
				{
					if (!options.Contains(propPrototype.Key))
					{
						options.Add(propPrototype.Key);
					}
				}
			}
			title = "Prop Type:";
			return true;
		case ConditionType.CommunityTypeEquals:
		case ConditionType.HasAnySurvivingEnemyCommunities:
		case ConditionType.HasAnySurvivingOtherCommunitiesOfType:
			options = new List<string>(Community.CommunityTypeNames);
			title = "Community Type:";
			return true;
		case ConditionType.SquadBehaviourEquals:
			options = new List<string>(Community.SquadBehaviourNames);
			title = "Behaviour:";
			return true;
		case ConditionType.SquadActionEquals:
			options = new List<string>(Community.SquadActionNames);
			title = "Action:";
			return true;
		case ConditionType.GateStateEquals:
			options = new List<string>(Gate.GateStateNames);
			title = "Gate State";
			return true;
		case ConditionType.HasExceededLimitForMemoryType:
		case ConditionType.HasMemory:
		case ConditionType.HasMemoryQuantityBetween:
		case ConditionType.HasMemoryQuantityAtLeast:
		case ConditionType.HasMemoryQuantityLessThan:
		case ConditionType.HasRecentMemory:
		case ConditionType.HasRecentMemoryOfAnyoneInCommunity:
		case ConditionType.HasRecentMemoryWhereObjectIsAnyoneInCommunity:
		case ConditionType.MemoryType:
		case ConditionType.MemoryQuantityForCommunity:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory4 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, MemoryPrototype> memoryPrototype in currentStory4.MemoryPrototypes)
				{
					if (!options.Contains(memoryPrototype.Key))
					{
						options.Add(memoryPrototype.Key);
					}
				}
			}
			title = "Memory:";
			return true;
		case ConditionType.HasPersonality:
		case ConditionType.HasUndiscoveredPersonality:
		case ConditionType.IsPersonalityKnownToPlayer:
			options = instance.GetAllPersonalities();
			title = "Personality:";
			return true;
		case ConditionType.HasRelationship:
		case ConditionType.HasRelationshipWithAnyone:
		case ConditionType.IsRelationshipKnownToPlayer:
		case ConditionType.PartnerHasRelationship:
		case ConditionType.PartnerHasRelationshipKnownToPlayer:
			options = new List<string>(Relationship.RelationshipTypeNames);
			title = "Relationship:";
			return true;
		case ConditionType.HasInjuryInBodyLocation:
			options = new List<string>(Injury.InjuryLocationNames);
			title = "Body Location:";
			return true;
		case ConditionType.HasInjuryOfType:
		case ConditionType.HasInfectedInjuryOfType:
			options = new List<string>(Injury.InjuryTypeNames);
			title = "Type:";
			return true;
		case ConditionType.PickStrain:
		case ConditionType.IsZombieWithStrain:
		case ConditionType.HasInfectedInjuryWithStrain:
		case ConditionType.MostRecentInfectedInjuryHadStrain:
		case ConditionType.CommunityHasNoAntigenKnown:
			options = new List<string>(Injury.InfectionTypeNames);
			title = "Strain";
			wantSort = false;
			return true;
		case ConditionType.HasInvisibleStrainType:
			options = new List<string>(Injury.InvisibleStrainTypeNames);
			title = "Type";
			wantSort = false;
			return true;
		case ConditionType.RankEquals:
			options = new List<string>(Character.RankNames);
			title = "Rank:";
			return true;
		case ConditionType.RoleEquals:
		case ConditionType.CommunityHasAnyoneWithRole:
			options = new List<string>(Character.RoleNames);
			title = "Role:";
			return true;
		case ConditionType.SkillEquals:
		case ConditionType.IsSkillKnownByPlayer:
		case ConditionType.HasSkill:
		case ConditionType.IsSkillBetween:
		case ConditionType.CommunityHasAnyoneWithSkill:
		case ConditionType.IsWearingAnythingWithSkillBonus:
		case ConditionType.Skill:
		case ConditionType.SkillCap:
			options = new List<string>(Skillset.SkillNames);
			options.Insert(0, "");
			title = "Skill:";
			return true;
		case ConditionType.BuildingOfTypeIsOnMarker:
		case ConditionType.CommunityHasBuildingOfType:
		case ConditionType.CommunityBuildingCountOfType:
			options = new List<string>(BaseObjectManager.BaseObjectNames);
			title = "Building Type:";
			return true;
		case ConditionType.CommunityHasPropWithPrototype:
		case ConditionType.CommunityHasVehicleToHitTheRoad:
			options = BaseMenu.GetPropOptions(typeof(Prop));
			title = "Prototype:";
			return true;
		case ConditionType.QuestIsActive:
		case ConditionType.QuestIsActiveGivenBy:
		case ConditionType.QuestIsActiveWithObject:
		case ConditionType.QuestIsActiveWithSeeker:
		case ConditionType.QuestIsActiveWithAnyParams:
		case ConditionType.QuestIsDiscovered:
		case ConditionType.QuestIsDiscoveredGivenBy:
		case ConditionType.QuestIsDiscoveredGivenByWithSeeker:
		case ConditionType.QuestIsDiscoveredWithObject:
		case ConditionType.QuestIsDiscoveredWithAnyParams:
		case ConditionType.QuestIsCompleted:
		case ConditionType.QuestIsFailed:
		case ConditionType.TimeSinceQuestStarted:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory5 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item in currentStory5.ScriptObjectsByUniqueID)
				{
					if (item.Value is Quest && !options.Contains(item.Key))
					{
						options.Add(item.Key);
					}
				}
			}
			title = "Quest:";
			return true;
		case ConditionType.QuestGroupIsFailed:
		case ConditionType.QuestGroupIsActiveWithAnyParams:
		case ConditionType.QuestGroupIsActiveGivenBy:
		case ConditionType.QuestGroupIsActiveGivenByWithSeeker:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory6 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item2 in currentStory6.ScriptObjectsByUniqueID)
				{
					if (item2.Value is QuestGroup && !options.Contains(item2.Key))
					{
						options.Add(item2.Key);
					}
				}
			}
			title = "Quest Group:";
			return true;
		case ConditionType.IsTimerRunning:
		case ConditionType.IsTimerAtLeast:
			title = "Name:";
			return true;
		case ConditionType.CauseOfDeath:
			options = new List<string>(Character.CauseOfDeathNames);
			title = "Cause Of Death:";
			return true;
		case ConditionType.VariableEquals:
		case ConditionType.VariableIsGreaterThan:
		case ConditionType.VariableIsLessThan:
		case ConditionType.VariableIsGreaterThanOrEqual:
		case ConditionType.VariableIsLessThanOrEqual:
		case ConditionType.Variable:
			title = "Variable:";
			return true;
		case ConditionType.IsCarryingCommunityMember:
		case ConditionType.IsCarryingCorpse:
		case ConditionType.CanSeeAnimal:
		case ConditionType.CanSeeDeadAnimal:
			options = BaseMenu.GetAnimalOptions(includeHuman: true);
			title = "Type:";
			return true;
		case ConditionType.IsAnimal:
		case ConditionType.CommunityHasAnimals:
			options = BaseMenu.GetAnimalOptions(includeHuman: false);
			title = "Type:";
			return true;
		case ConditionType.DeterministicRandom:
			title = "Salt:";
			return true;
		case ConditionType.CanOpenPlayerGatesEquals:
			title = "Can Open Gates:";
			options = new List<string>(Character.CanOpenGatesNames);
			return true;
		case ConditionType.IsAttractedToGender:
			title = "Gender:";
			options = new List<string>(CharacterAppearance.GenderTypeNames);
			return true;
		case ConditionType.IsInvader:
		case ConditionType.IsInvaderWithSource:
		case ConditionType.IsInvaderFinished:
			title = "Invader:";
			return true;
		case ConditionType.HasAnyReplies:
		case ConditionType.HasSpokenOnce:
		case ConditionType.HasSpokenSpeech:
		case ConditionType.HasSpokenSpeechRecently:
		case ConditionType.HasSpokenSpeechToAnyoneRecently:
			title = "Speech:";
			return true;
		case ConditionType.HasSpokenSpeechForSituationRecently:
		case ConditionType.HasSpokenSpeechForSituationToAnyoneRecently:
			title = "Situation:";
			options = new List<string>(Character.SpeechSituationNames);
			return true;
		default:
			return false;
		}
	}

	public static bool HasData(ConditionType conditionType, out string title, out bool hasFormula)
	{
		title = null;
		hasFormula = false;
		switch (conditionType)
		{
		case ConditionType.DeterministicRandom:
			title = "Mod:";
			return true;
		case ConditionType.HasNFoodItems:
		case ConditionType.HasNWater:
		case ConditionType.HasNItemsOfType:
		case ConditionType.HasNItemsOfClothingType:
		case ConditionType.HasNGiftedItemsOfType:
		case ConditionType.HasTradedNItemsOfType:
		case ConditionType.HasBackpack:
		case ConditionType.HasGiftableItems:
		case ConditionType.CommunityHasBuildingOfType:
		case ConditionType.CommunityHasPropWithPrototype:
		case ConditionType.CommunityHasNItemsOfType:
		case ConditionType.CommunityHasNItemsOfClothingType:
		case ConditionType.CommunityHasNMembersWithGiftedItemsOfType:
		case ConditionType.HasGoodRelationshipsInCommunity:
		case ConditionType.VotePlacingIsAtLeast:
		case ConditionType.IsHitTheRoadCountAtLeast:
		case ConditionType.WorldHasNItemsOfType:
			title = "At least:";
			return true;
		case ConditionType.HasGold:
		case ConditionType.HasSuppliesOfValue:
			hasFormula = true;
			title = "At least:";
			return true;
		case ConditionType.HasLiquidOfType:
			title = "At least (Fl Oz):";
			return true;
		case ConditionType.HasAlcoholAmountTogether:
			title = "At least:";
			return true;
		case ConditionType.VariableEquals:
		case ConditionType.VariableIsGreaterThan:
		case ConditionType.VariableIsLessThan:
		case ConditionType.VariableIsGreaterThanOrEqual:
		case ConditionType.VariableIsLessThanOrEqual:
			title = "Value:";
			return true;
		case ConditionType.IsAgeAtLeast:
			title = "Age:";
			return true;
		case ConditionType.CommunityHasCrops:
		case ConditionType.CommunityHasBuildings:
		case ConditionType.CommunityHasAccomodation:
		case ConditionType.CommunityHasUnusedAccomodation:
		case ConditionType.CommunityHasLitCampfires:
		case ConditionType.CommunityHasGates:
		case ConditionType.CommunityHasConsciousMembers:
		case ConditionType.CommunityHasAnimals:
		case ConditionType.CommunityHasDeadUnburied:
		case ConditionType.NumberOfCommunitiesIntroduced:
		case ConditionType.VisibleCommunityMembersHaveGold:
			title = "At least:";
			return true;
		case ConditionType.CommunitySizeIsAtLeast:
		case ConditionType.CanSeeNumberOfCommunityMembers:
			title = "Members:";
			return true;
		case ConditionType.AreCommunityMembersNearby:
		case ConditionType.DistanceToIsLessThan:
		case ConditionType.CanSeeAnimal:
		case ConditionType.CanSeeDeadAnimal:
		case ConditionType.IsInRangeOfDiscoveredDeerSpawnpoint:
			title = "Distance (m):";
			return true;
		case ConditionType.UnbandagedInjuryIsOlderThan:
		case ConditionType.InfectedInjuryIsOlderThan:
		case ConditionType.IsTimerAtLeast:
		case ConditionType.HasSpokenSpeechRecently:
		case ConditionType.HasSpokenSpeechToAnyoneRecently:
		case ConditionType.HasSpokenSpeechForSituationRecently:
		case ConditionType.HasSpokenSpeechForSituationToAnyoneRecently:
			title = "Seconds:";
			return true;
		case ConditionType.HasSkill:
		case ConditionType.IsSkillBetween:
		case ConditionType.CommunityHasAnyoneWithSkill:
			title = "At least (0-5):";
			return true;
		case ConditionType.SkillEquals:
			title = "Level (0-5):";
			return true;
		case ConditionType.TraderLevelEquals:
			title = "Level (0-5):";
			return true;
		case ConditionType.HasBloodLoss:
		case ConditionType.HasFatigue:
		case ConditionType.HasInfectionProgression:
		case ConditionType.HasDamageFraction:
		case ConditionType.HasFatness:
		case ConditionType.IsSnowing:
		case ConditionType.IsRaining:
			title = "At least (0-1):";
			return true;
		case ConditionType.HasHunger:
		case ConditionType.HasThirst:
		case ConditionType.HasSleepDeprivation:
		case ConditionType.PregnancyProgressionIsAtLeast:
		case ConditionType.HasAnyMemoryOlderThan:
		case ConditionType.TimeSinceDeathIsAtLeast:
		case ConditionType.DaysSinceGameStartAtLeast:
		case ConditionType.TimeSinceJoinedCommunityIsAtLeast:
			title = "At least (days):";
			return true;
		case ConditionType.HasRecentMemory:
		case ConditionType.HasRecentMemoryOfAnyoneInCommunity:
		case ConditionType.HasRecentMemoryWhereObjectIsAnyoneInCommunity:
			title = "Less than (days):";
			return true;
		case ConditionType.HasHypothermia:
			title = "Below (°C):";
			return true;
		case ConditionType.IsTemperatureLessThan:
			title = "Less Than (°C):";
			return true;
		case ConditionType.IsTemperatureAtLeast:
			title = "At Least (°C):";
			return true;
		case ConditionType.IsSnowCoverLessThan:
			title = "Less Than (0-1):";
			return true;
		case ConditionType.IsSnowCoverAtLeast:
			title = "At Least (0-1):";
			return true;
		case ConditionType.Approves:
		case ConditionType.ApprovalSinceLastFight:
		case ConditionType.ApprovedBeforeThisMemory:
			title = "At 0 Respect (%):";
			return true;
		case ConditionType.Respects:
			title = "At 0 Approval (%):";
			return true;
		case ConditionType.ApprovalRespectAmountBetween:
		case ConditionType.ApprovalRespectAmountAtLeast:
		case ConditionType.ApprovalRespectAbsoluteSum:
		case ConditionType.ApprovalSince:
		case ConditionType.RespectSince:
		case ConditionType.TastinessIsAtLeast:
		case ConditionType.CommunityNutritionLevelIsAtLeast:
		case ConditionType.SurvivorCampDensityIsAtLeast:
			title = "At least (%):";
			return true;
		case ConditionType.ApprovalRespectAngleBetween:
			title = "At least (°):";
			return true;
		case ConditionType.HasBloodAlcoholConcentration:
		case ConditionType.HasExcitement:
		case ConditionType.MoraleIsAtLeast:
		case ConditionType.PresenceIsAtLeast:
		case ConditionType.MemoryApprovalBetween:
		case ConditionType.MemoryApprovalAtLeast:
		case ConditionType.MemoryRespectBetween:
		case ConditionType.MemoryRespectAtLeast:
		case ConditionType.MemoryMoraleBetween:
		case ConditionType.MemoryMoraleAtLeast:
		case ConditionType.MemoryOtherApprovalAtLeast:
		case ConditionType.MemoryOtherApprovalLessThan:
			title = "At least (%):";
			return true;
		case ConditionType.MoraleIsLessThan:
		case ConditionType.PresenceIsLessThan:
		case ConditionType.ApprovalRespectAmountLessThan:
		case ConditionType.MemoryApprovalLessThan:
		case ConditionType.MemoryRespectLessThan:
		case ConditionType.MemoryMoraleLessThan:
			title = "Less than (%):";
			return true;
		case ConditionType.HasMemoryQuantityBetween:
		case ConditionType.HasMemoryQuantityAtLeast:
		case ConditionType.MemoryQuantityFactorBetween:
		case ConditionType.MemoryQuantityFactorAtLeast:
			title = "At least:";
			return true;
		case ConditionType.HasMemoryQuantityLessThan:
		case ConditionType.MemoryQuantityFactorLessThan:
			title = "Less than:";
			return true;
		case ConditionType.MemoryIsRecent:
			title = "Less than (days):";
			return true;
		case ConditionType.AgeDifferenceIsLessThan:
			title = "Less than (years):";
			return true;
		case ConditionType.TimeSinceLastSpoke:
		case ConditionType.TimeSinceQuestStarted:
			title = "At least (secs):";
			return true;
		case ConditionType.AreApparentlyAloneTogether:
		case ConditionType.IsCloseToBase:
		case ConditionType.AlliesNearby:
			title = "Distance:";
			return true;
		case ConditionType.CanAffordPrice:
			title = "Base Price:";
			hasFormula = true;
			return true;
		case ConditionType.DayOfYearIsOnOrAfter:
		case ConditionType.DayOfYearIsBefore:
			title = "Day:";
			return true;
		default:
			return false;
		}
	}

	public static bool HasData2(ConditionType conditionType, out string title)
	{
		title = null;
		switch (conditionType)
		{
		case ConditionType.DeterministicRandom:
			title = "Equals:";
			return true;
		case ConditionType.IsSkillBetween:
			title = "Less than (0-5):";
			return true;
		case ConditionType.Approves:
		case ConditionType.ApprovalSinceLastFight:
		case ConditionType.ApprovedBeforeThisMemory:
			title = "Respect Range (%):";
			return true;
		case ConditionType.Respects:
			title = "Approval Range (%):";
			return true;
		case ConditionType.ApprovalRespectAmountBetween:
			title = "Less than (%):";
			return true;
		case ConditionType.ApprovalRespectAngleBetween:
			title = "Less than (°):";
			return true;
		case ConditionType.MemoryApprovalBetween:
		case ConditionType.MemoryRespectBetween:
		case ConditionType.MemoryMoraleBetween:
			title = "Less than (%):";
			return true;
		case ConditionType.HasMemoryQuantityBetween:
		case ConditionType.MemoryQuantityFactorBetween:
			title = "Less than:";
			return true;
		case ConditionType.ApprovalSince:
		case ConditionType.RespectSince:
			title = "Since (days)";
			return true;
		default:
			return false;
		}
	}

	public static bool HasConditionBlocks(ConditionType conditionType)
	{
		switch (conditionType)
		{
		case ConditionType.HasGold:
		case ConditionType.HasSuppliesOfValue:
		case ConditionType.And:
		case ConditionType.Or:
		case ConditionType.CanAffordPrice:
			return true;
		default:
			return false;
		}
	}

	public bool IsConditionSatisfied(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		bool flag = IsConditionSatisfiedEx(actor, target, obj, param);
		if (Not)
		{
			flag = !flag;
		}
		return flag;
	}

	public static void FixupSpecifier(ref Specifier specifier, ref SpecifierModifier modifier, ref string id)
	{
		if (id != null && id.Length == 0)
		{
			id = null;
		}
		switch (specifier)
		{
		case Specifier.ActorCommunity:
			specifier = Specifier.Actor;
			modifier = SpecifierModifier.Community;
			break;
		case Specifier.TargetCommunity:
			specifier = Specifier.Target;
			modifier = SpecifierModifier.Community;
			break;
		case Specifier.ReferringToCommunity:
			specifier = Specifier.ReferringTo;
			modifier = SpecifierModifier.Community;
			break;
		case Specifier.PlayerCommunity:
			specifier = Specifier.Player;
			modifier = SpecifierModifier.Community;
			break;
		case Specifier.LeaderOfActor:
			specifier = Specifier.Actor;
			modifier = SpecifierModifier.Leader;
			break;
		case Specifier.LeaderOfTarget:
			specifier = Specifier.Target;
			modifier = SpecifierModifier.Leader;
			break;
		case Specifier.ReferringToLeader:
			specifier = Specifier.ReferringTo;
			modifier = SpecifierModifier.Leader;
			break;
		case Specifier.PartnerOfActor:
			specifier = Specifier.Actor;
			modifier = SpecifierModifier.Partner;
			break;
		case Specifier.PartnerOfTarget:
			specifier = Specifier.Target;
			modifier = SpecifierModifier.Partner;
			break;
		case Specifier.ReferringToPartner:
			specifier = Specifier.ReferringTo;
			modifier = SpecifierModifier.Partner;
			break;
		case Specifier.CarriedByActor:
			specifier = Specifier.Actor;
			modifier = SpecifierModifier.Carrying;
			break;
		case Specifier.CarriedByTarget:
			specifier = Specifier.Target;
			modifier = SpecifierModifier.Carrying;
			break;
		case Specifier.ReferringToCarried:
			specifier = Specifier.ReferringTo;
			modifier = SpecifierModifier.Carrying;
			break;
		case Specifier.CarrierOfActor:
			specifier = Specifier.Actor;
			modifier = SpecifierModifier.Carrier;
			break;
		case Specifier.CarrierOfTarget:
			specifier = Specifier.Target;
			modifier = SpecifierModifier.Carrier;
			break;
		case Specifier.ReferringToCarrier:
			specifier = Specifier.ReferringTo;
			modifier = SpecifierModifier.Carrier;
			break;
		}
	}

	public static T ResolveSpecifier<T>(Specifier specifier, SpecifierModifier modifier, string id, Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		BaseObject baseObject = null;
		switch (specifier)
		{
		case Specifier.Actor:
			baseObject = actor;
			break;
		case Specifier.Target:
			baseObject = target;
			break;
		case Specifier.ReferringTo:
			baseObject = obj;
			break;
		case Specifier.Player:
			baseObject = Session.Instance.CommunityManager.PlayerCommunity.Leader;
			break;
		case Specifier.ID:
			baseObject = BaseObjectManager.Instance.GetObjectByUniqueID(id);
			break;
		case Specifier.MemoryActor:
			baseObject = ((param.GetParamType() == ParamType.Memory) ? param.GetMemory().Actor : null);
			break;
		case Specifier.MemoryObject:
			baseObject = ((param.GetParamType() == ParamType.Memory) ? param.GetMemory().Object : null);
			break;
		case Specifier.MemoryThirdParty:
			baseObject = ((param.GetParamType() == ParamType.Memory) ? param.GetMemory().ThirdParty : null);
			break;
		case Specifier.ExtraParam:
			baseObject = ((param.GetParamType() == ParamType.BaseObject) ? param.GetBaseObject() : null);
			break;
		case Specifier.QuestGiver:
			baseObject = ((param.GetBaseObject() is QuestInstance questInstance4) ? questInstance4.QuestGiver : null);
			break;
		case Specifier.QuestSeeker:
			baseObject = ((param.GetBaseObject() is QuestInstance questInstance3) ? questInstance3.QuestSeeker : null);
			break;
		case Specifier.QuestObject:
			baseObject = ((param.GetBaseObject() is QuestInstance questInstance2) ? questInstance2.QuestObject : null);
			break;
		case Specifier.QuestParam:
			baseObject = ((param.GetBaseObject() is QuestInstance questInstance) ? questInstance.QuestParam.GetBaseObject() : null);
			break;
		}
		switch (modifier)
		{
		case SpecifierModifier.Community:
			if (baseObject != null)
			{
				baseObject = baseObject.GetCommunity();
			}
			break;
		case SpecifierModifier.Leader:
			baseObject = (baseObject?.GetCommunity())?.Leader;
			break;
		case SpecifierModifier.Partner:
		{
			Character character2 = baseObject?.GetAsCharacter();
			baseObject = ((character2 != null) ? Relationship.GetPartner(character2) : null);
			break;
		}
		case SpecifierModifier.LivingPartner:
		{
			Character character5 = baseObject?.GetAsCharacter();
			Character character6 = ((character5 != null) ? Relationship.GetPartner(character5) : null);
			baseObject = ((character6 != null && character6.AliveAndNotZombie) ? character6 : null);
			break;
		}
		case SpecifierModifier.Equipped:
			baseObject = (baseObject?.GetAsCharacter())?.EquippedItem;
			break;
		case SpecifierModifier.Owner:
			baseObject = ((baseObject is Equipment equipment) ? equipment.InventoryOwner : null);
			break;
		case SpecifierModifier.Carrying:
			baseObject = (baseObject?.GetAsCharacter())?.CarryingObject;
			break;
		case SpecifierModifier.Carrier:
			baseObject = (baseObject?.GetAsCharacter())?.CarriedBy;
			break;
		case SpecifierModifier.Occupying:
			baseObject = (baseObject?.GetAsCharacter())?.InsideBuilding;
			break;
		case SpecifierModifier.NearestMember:
		{
			Character character3 = baseObject?.GetAsCharacter();
			baseObject = ((character3 != null && character3.Community != null) ? character3.Community.GetNearestLivingNonZombieMember(character3.Tile, character3, BaseObjectType.Human, float.MaxValue) : null);
			break;
		}
		case SpecifierModifier.NearestFollower:
		{
			Character character = baseObject?.GetAsCharacter();
			baseObject = ((character != null && character.Community != null) ? character.GetNearestFollower() : null);
			break;
		}
		case SpecifierModifier.OldCommunity:
			baseObject = (baseObject?.GetAsCharacter())?.InitialCommunity;
			break;
		case SpecifierModifier.OldLeader:
		{
			Character character4 = baseObject?.GetAsCharacter();
			baseObject = ((character4 != null && character4.InitialCommunity != null && character4.InitialCommunity.Leader != null) ? character4.InitialCommunity.Leader : ((character4.Community != null) ? character4.Community.Leader : null));
			break;
		}
		case SpecifierModifier.InvaderSource:
		{
			Community community = baseObject?.GetCommunity();
			baseObject = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter((community != null) ? community : baseObject)?.SourceObject;
			break;
		}
		case SpecifierModifier.QuestGiver:
			baseObject = ((baseObject is QuestInstance questInstance8) ? questInstance8.QuestGiver : null);
			break;
		case SpecifierModifier.QuestSeeker:
			baseObject = ((baseObject is QuestInstance questInstance7) ? questInstance7.QuestSeeker : null);
			break;
		case SpecifierModifier.QuestObject:
			baseObject = ((baseObject is QuestInstance questInstance6) ? questInstance6.QuestObject : null);
			break;
		case SpecifierModifier.QuestParam:
			baseObject = ((baseObject is QuestInstance questInstance5) ? questInstance5.QuestParam.GetBaseObject() : null);
			break;
		}
		T val = baseObject as T;
		if (val == null && baseObject != null && typeof(T).IsA(typeof(Character)))
		{
			val = baseObject.GetAsCharacter() as T;
		}
		return val;
	}

	public static Community ResolveSpecifierCommunity(Specifier specifier, SpecifierModifier modifier, string id, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifier<BaseObject>(specifier, modifier, id, actor, target, obj, param)?.GetCommunity();
	}

	public static Equipment ResolveSpecifierEquipment(Specifier specifier, SpecifierModifier modifier, string id, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		BaseObject baseObject = ResolveSpecifier<BaseObject>(specifier, modifier, id, actor, target, obj, param);
		if (baseObject is Equipment result)
		{
			return result;
		}
		if (baseObject is Character character)
		{
			return character.EquippedItem;
		}
		return null;
	}

	public static EquipmentPrototype ResolveSpecifierEquipmentPrototype(Specifier specifier, SpecifierModifier modifier, string id, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		Equipment equipment = ResolveSpecifierEquipment(specifier, modifier, id, actor, target, obj, param);
		if (equipment != null)
		{
			return equipment.GetPrototype();
		}
		switch (specifier)
		{
		case Specifier.ExtraParam:
			return param.GetEquipmentPrototype();
		case Specifier.ReferringTo:
			_ = obj is QuestInstance;
			break;
		}
		return null;
	}

	public static LiquidPrototype ResolveSpecifierLiquidPrototype(Specifier specifier, SpecifierModifier modifier, string id, Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		Equipment equipment = ResolveSpecifierEquipment(specifier, modifier, id, actor, target, obj, param);
		if (equipment != null)
		{
			return equipment.GetLiquidContentsType();
		}
		if (specifier == Specifier.ExtraParam)
		{
			return param.GetLiquidPrototype();
		}
		return ResolveSpecifier<Prop>(specifier, modifier, id, actor, target, obj, param)?.GetLiquidType();
	}

	public T GetSubject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return ResolveSpecifier<T>(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public T GetObject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return ResolveSpecifier<T>(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public T GetThirdParty<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return ResolveSpecifier<T>(ThirdParty, ThirdPartyModifier, ThirdPartyID, actor, target, obj, param);
	}

	private Community GetSubjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierCommunity(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	private Community GetObjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierCommunity(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	private Community GetThirdPartyCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierCommunity(ThirdParty, ThirdPartyModifier, ThirdPartyID, actor, target, obj, param);
	}

	private Equipment GetSubjectEquipment(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierEquipment(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	private Equipment GetObjectEquipment(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierEquipment(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	private Equipment GetThirdPartyEquipment(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierEquipment(ThirdParty, ThirdPartyModifier, ThirdPartyID, actor, target, obj, param);
	}

	public EquipmentPrototype GetSubjectEquipmentPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierEquipmentPrototype(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public EquipmentPrototype GetObjectEquipmentPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierEquipmentPrototype(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public LiquidPrototype GetSubjectLiquidPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierLiquidPrototype(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public LiquidPrototype GetObjectLiquidPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return ResolveSpecifierLiquidPrototype(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	private bool IsConditionSatisfiedEx(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		switch (Type)
		{
		case ConditionType.Always:
			return true;
		case ConditionType.HasID:
		{
			BaseObject subject67 = GetSubject<BaseObject>(actor, target, obj, param);
			if (subject67 != null)
			{
				return subject67.GetUniqueID() == StringData;
			}
			return false;
		}
		case ConditionType.Equals:
		{
			BaseObject subject78 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject7 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject78 != null && baseObject7 != null)
			{
				return subject78 == baseObject7;
			}
			return false;
		}
		case ConditionType.DeterministicRandom:
		{
			BaseObject subject91 = GetSubject<BaseObject>(actor, target, obj, param);
			uint num25 = 0u;
			if (!string.IsNullOrEmpty(StringData))
			{
				num25 = (uint)StringUtil.JenkinsHash(StringData);
			}
			if (subject91 != null)
			{
				return MathUtil.WangHash((uint)subject91.Id ^ num25) % (uint)Data == (uint)Data2;
			}
			return false;
		}
		case ConditionType.PickStrain:
		{
			BaseObject subject217 = GetSubject<BaseObject>(actor, target, obj, param);
			if (subject217 != null)
			{
				InfectionType infectionType = Session.Instance.DifficultySettings.PickStrain(MathUtil.RandomFloat(subject217.Id));
				if (infectionType == InfectionType.None)
				{
					infectionType = (InfectionType)MathUtil.RandomInt(subject217.Id, 1, 5);
				}
				int num64 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
				return infectionType == (InfectionType)num64;
			}
			return false;
		}
		case ConditionType.IsAlive:
			return GetSubject<Character>(actor, target, obj, param)?.Alive ?? false;
		case ConditionType.IsAliveAndNotZombie:
			return GetSubject<Character>(actor, target, obj, param)?.AliveAndNotZombie ?? false;
		case ConditionType.IsLooter:
			return GetSubject<Character>(actor, target, obj, param)?.IsLooter() ?? GetSubjectCommunity(actor, target, obj, param)?.IsLooterCommunity() ?? false;
		case ConditionType.IsAmbient:
			return GetSubjectCommunity(actor, target, obj, param)?.IsAmbientCommunity() ?? true;
		case ConditionType.IsTemporary:
			return GetSubjectCommunity(actor, target, obj, param)?.IsTemporaryCommunity() ?? false;
		case ConditionType.IsAmbientOrTemporary:
		{
			Community subjectCommunity46 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity46 != null && !subjectCommunity46.IsAmbientCommunity())
			{
				return subjectCommunity46.IsTemporaryCommunity();
			}
			return true;
		}
		case ConditionType.IsAmbientOrUnknownTemporary:
		{
			Community subjectCommunity55 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity55 != null && !subjectCommunity55.IsAmbientCommunity())
			{
				if (subjectCommunity55.IsTemporaryCommunity())
				{
					return !subjectCommunity55.CommunityNameKnown;
				}
				return false;
			}
			return true;
		}
		case ConditionType.IsTraderCommunity:
		{
			Community subjectCommunity63 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity63 != null)
			{
				return subjectCommunity63.CommunityType == CommunityType.RovingTrader;
			}
			return false;
		}
		case ConditionType.IsRefugeeCommunity:
		{
			Community subjectCommunity60 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity60 != null)
			{
				return subjectCommunity60.CommunityType == CommunityType.RovingRefugee;
			}
			return false;
		}
		case ConditionType.IsNormalCommunity:
		{
			Community subjectCommunity7 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity7 != null)
			{
				return subjectCommunity7.CommunityType == CommunityType.Normal;
			}
			return false;
		}
		case ConditionType.IsAISettlement:
			return GetSubjectCommunity(actor, target, obj, param)?.IsAISettlement() ?? false;
		case ConditionType.CommunityTypeEquals:
		{
			Community subjectCommunity25 = GetSubjectCommunity(actor, target, obj, param);
			int num28 = Math.Max(0, Array.IndexOf(Community.CommunityTypeNames, StringData));
			if (subjectCommunity25 != null && num28 != -1)
			{
				return subjectCommunity25.CommunityType == (CommunityType)num28;
			}
			return false;
		}
		case ConditionType.SquadBehaviourEquals:
		{
			Character subject110 = GetSubject<Character>(actor, target, obj, param);
			int num36 = GetObjectCommunity(actor, target, obj, param)?.Id ?? 0;
			int num37 = Array.IndexOf(Community.SquadBehaviourNames, StringData);
			if (subject110 != null && subject110.Community != null && subject110.SquadId != 0)
			{
				Squad squad4 = subject110.Community.GetSquad(subject110.SquadId);
				if (squad4 != null && num37 != -1 && squad4.Behaviour == (SquadBehaviour)num37)
				{
					if (squad4.EnemyCommunityId != num36)
					{
						return num36 == 0;
					}
					return true;
				}
				return false;
			}
			return num37 <= 0;
		}
		case ConditionType.SquadActionEquals:
		{
			Character subject12 = GetSubject<Character>(actor, target, obj, param);
			int num2 = Array.IndexOf(Community.SquadActionNames, StringData);
			if (subject12 != null && subject12.Community != null && subject12.SquadId != 0)
			{
				Squad squad = subject12.Community.GetSquad(subject12.SquadId);
				if (squad != null && num2 != -1)
				{
					return squad.Action == (SquadAction)num2;
				}
				return false;
			}
			return num2 <= 0;
		}
		case ConditionType.IsSquadGoalAchieved:
		{
			Character subject155 = GetSubject<Character>(actor, target, obj, param);
			if (subject155 != null && subject155.Community != null && subject155.SquadId != 0)
			{
				return subject155.Community.GetSquad(subject155.SquadId)?.GoalAchieved ?? false;
			}
			return false;
		}
		case ConditionType.IsSquadLeader:
		{
			Character subject104 = GetSubject<Character>(actor, target, obj, param);
			if (subject104 != null && subject104.Community != null && subject104.SquadId != 0)
			{
				Squad squad3 = subject104.Community.GetSquad(subject104.SquadId);
				if (squad3 != null)
				{
					return squad3.GetLeader() == subject104;
				}
				return false;
			}
			return false;
		}
		case ConditionType.CommunityHasAnyHumanTraps:
			return GetSubjectCommunity(actor, target, obj, param)?.HasAnyHumanTraps() ?? false;
		case ConditionType.HasAnySurvivingEnemyCommunities:
		{
			Community subjectCommunity13 = GetSubjectCommunity(actor, target, obj, param);
			int num11 = Array.IndexOf(Community.CommunityTypeNames, StringData);
			if (subjectCommunity13 != null)
			{
				foreach (Community community4 in Session.Instance.CommunityManager.Communities)
				{
					if ((num11 == -1 || community4.CommunityType == (CommunityType)num11) && subjectCommunity13 != community4 && Session.Instance.CommunityManager.GetRelationship(subjectCommunity13, community4) == CommunityRelationshipType.Hostile && community4.HasAnyLivingNonZombieMembers())
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.HasAnySurvivingOtherCommunitiesOfType:
		{
			Community subjectCommunity5 = GetSubjectCommunity(actor, target, obj, param);
			int num4 = Array.IndexOf(Community.CommunityTypeNames, StringData);
			if (subjectCommunity5 != null)
			{
				foreach (Community community5 in Session.Instance.CommunityManager.Communities)
				{
					if ((num4 == -1 || community5.CommunityType == (CommunityType)num4) && subjectCommunity5 != community5 && community5.HasAnyLivingNonZombieMembers())
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.WorldHasAnyUnownedVehicles:
			foreach (Prop allProp in Session.Instance.PropManager.AllProps)
			{
				if (allProp.Community == null && allProp.GetBaseObjectType() == BaseObjectType.EnterableVehicle)
				{
					return true;
				}
			}
			return false;
		case ConditionType.WorldHasAnyUnownedBuildings:
			foreach (Town town in Session.Instance.CommunityManager.Towns)
			{
				if (town.IgnoreForQuests)
				{
					continue;
				}
				foreach (Prop building5 in town.Buildings)
				{
					if (building5 is Building { Community: null })
					{
						return true;
					}
				}
			}
			foreach (Prop allProp2 in Session.Instance.PropManager.AllProps)
			{
				if (allProp2.Town == null && allProp2.Community == null && allProp2 is Building)
				{
					return true;
				}
			}
			return false;
		case ConditionType.IsZombie:
		{
			Character subject162 = GetSubject<Character>(actor, target, obj, param);
			if (subject162 != null)
			{
				return subject162.Infection != InfectionType.None;
			}
			return GetSubjectCommunity(actor, target, obj, param)?.IsZombieCommunity() ?? false;
		}
		case ConditionType.IsZombieWithStrain:
		{
			int num44 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
			Character subject135 = GetSubject<Character>(actor, target, obj, param);
			if (subject135 != null)
			{
				return subject135.Infection == (InfectionType)num44;
			}
			Community subjectCommunity41 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity41 != null && subjectCommunity41.IsZombieCommunity() && subjectCommunity41.Leader != null)
			{
				return subjectCommunity41.Leader.Infection == (InfectionType)num44;
			}
			return false;
		}
		case ConditionType.IsSleeping:
			return GetSubject<Character>(actor, target, obj, param)?.IsSleeping() ?? false;
		case ConditionType.IsUnconscious:
		{
			Character subject86 = GetSubject<Character>(actor, target, obj, param);
			if (subject86 != null)
			{
				return subject86.Consciousness == Consciousness.Unconscious;
			}
			return false;
		}
		case ConditionType.IsBuried:
			return GetSubject<Character>(actor, target, obj, param)?.Disappeared ?? true;
		case ConditionType.IsDeleted:
		{
			BaseObject subject74 = GetSubject<BaseObject>(actor, target, obj, param);
			if (subject74 != null && !subject74.Deleted)
			{
				return subject74.IsDisappeared();
			}
			return true;
		}
		case ConditionType.IsPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.IsPlayerAvatar() ?? false;
		case ConditionType.IsControllableByPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.IsControllableByPlayer() ?? false;
		case ConditionType.IsControllableByOrFollowingPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.IsControllableByOrFollowingPlayer() ?? false;
		case ConditionType.IsDirectControlled:
			return GetSubject<Character>(actor, target, obj, param)?.DirectControlledMajorAIDisabled ?? false;
		case ConditionType.IsControlledCharacter:
		{
			Character subject170 = GetSubject<Character>(actor, target, obj, param);
			if (subject170 != null)
			{
				return Session.Instance.GetPlayerControllingCharacter(subject170) != null;
			}
			return false;
		}
		case ConditionType.IsControlledCharacterOrFollower:
		{
			Character subject148 = GetSubject<Character>(actor, target, obj, param);
			if (subject148 != null)
			{
				if (Session.Instance.GetPlayerControllingCharacter(subject148) == null)
				{
					return Session.Instance.GetPlayerControllingCharacter(subject148.SquadLeader) != null;
				}
				return true;
			}
			return false;
		}
		case ConditionType.IsInAGroup:
			return GetSubject<Character>(actor, target, obj, param)?.IsInAnySquad() ?? false;
		case ConditionType.IsInSameSquad:
		{
			Character subject117 = GetSubject<Character>(actor, target, obj, param);
			Character character28 = GetObject<Character>(actor, target, obj, param);
			if (subject117 != null && character28 != null)
			{
				return subject117.IsInSameSquad(character28);
			}
			return false;
		}
		case ConditionType.AreCommunityMembersNearby:
		{
			TileObject subject95 = GetSubject<TileObject>(actor, target, obj, param);
			Community objectCommunity9 = GetObjectCommunity(actor, target, obj, param);
			if (subject95 != null && objectCommunity9 != null)
			{
				return objectCommunity9.GetDistSqToNearestLivingNonZombieMember(subject95.GetCentreTile(), subject95 as Character, BoolData) < Data * Data;
			}
			return false;
		}
		case ConditionType.AreApparentlyAloneTogether:
		{
			Character subject83 = GetSubject<Character>(actor, target, obj, param);
			Character character15 = GetObject<Character>(actor, target, obj, param);
			if (subject83 != null && character15 != null)
			{
				for (int i = 0; i < 2; i++)
				{
					Character character16 = ((i == 0) ? subject83 : character15);
					if (character16 == null)
					{
						continue;
					}
					foreach (Target target4 in character16.Targets)
					{
						if (target4.Object is Human && target4.Object != ((i == 0) ? character15 : subject83) && !target4.GetFlag(TargetFlags.Dead) && MathUtil.ToXZ(target4.LastKnownPosition - character16.Pos).sqrMagnitude <= Data * Data)
						{
							return false;
						}
					}
				}
				return true;
			}
			return false;
		}
		case ConditionType.IsCloseToBase:
		{
			Community objectCommunity6 = GetObjectCommunity(actor, target, obj, param);
			if (objectCommunity6 != null)
			{
				Character subject52 = GetSubject<Character>(actor, target, obj, param);
				if (subject52 != null)
				{
					return objectCommunity6.IsCloseToBase(subject52.Tile, Data);
				}
				Community subjectCommunity12 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity12 != null)
				{
					foreach (Character member in subjectCommunity12.Members)
					{
						if (member.AliveAndNotZombie && member.GetBaseObjectType() == BaseObjectType.Human && objectCommunity6.IsCloseToBase(member.Tile, Data))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		case ConditionType.HasPlayerFoundBase:
		{
			Community subjectCommunity10 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity10 != null && subjectCommunity10.GetBaseCentre(out var centre, out var _))
			{
				return GameTerrain.Instance.FogOfWar.IsTileExploredNotIncludingMap(centre.x, centre.y);
			}
			return false;
		}
		case ConditionType.IsInsidBasePerimeter:
		{
			TileObject subject14 = GetSubject<TileObject>(actor, target, obj, param);
			Community objectCommunity = GetObjectCommunity(actor, target, obj, param);
			if (subject14 != null && objectCommunity != null)
			{
				return objectCommunity.IsTileInsidePerimeter(subject14.GetTile());
			}
			return false;
		}
		case ConditionType.IsMemberOf:
		{
			TileObject subject218 = GetSubject<TileObject>(actor, target, obj, param);
			Community objectCommunity23 = GetObjectCommunity(actor, target, obj, param);
			if (subject218 != null)
			{
				return subject218.GetCommunity() == objectCommunity23;
			}
			return false;
		}
		case ConditionType.IsLeaderOf:
		{
			Character subject180 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity21 = GetObjectCommunity(actor, target, obj, param);
			if (subject180 != null && subject180.GetCommunity() == objectCommunity21)
			{
				return subject180.Rank == Rank.Leader;
			}
			return false;
		}
		case ConditionType.WasInitiallyMemberOf:
		{
			Character subject178 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity20 = GetObjectCommunity(actor, target, obj, param);
			if (subject178 != null)
			{
				return subject178.InitialCommunity == objectCommunity20;
			}
			return false;
		}
		case ConditionType.CommunityHasAnyMembersWhoWereInitiallyMembersOf:
		{
			Community subjectCommunity56 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity19 = GetObjectCommunity(actor, target, obj, param);
			return subjectCommunity56?.HasAnyActiveMembersWhoWereInitiallyMembersOf(objectCommunity19) ?? false;
		}
		case ConditionType.IsInInitialCommunity:
		{
			Character subject149 = GetSubject<Character>(actor, target, obj, param);
			if (subject149 != null)
			{
				return subject149.GetCommunity() == subject149.InitialCommunity;
			}
			return false;
		}
		case ConditionType.AreMembersOfSameCommunity:
		{
			TileObject subject144 = GetSubject<TileObject>(actor, target, obj, param);
			TileObject tileObject4 = GetObject<TileObject>(actor, target, obj, param);
			if (subject144 != null && tileObject4 != null)
			{
				return subject144.GetCommunity() == tileObject4.GetCommunity();
			}
			return false;
		}
		case ConditionType.IsHuman:
			return GetSubject<Human>(actor, target, obj, param) != null;
		case ConditionType.IsAnimal:
		{
			Animal subject133 = GetSubject<Animal>(actor, target, obj, param);
			int num43 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (subject133 != null)
			{
				if (!string.IsNullOrEmpty(StringData))
				{
					return subject133.GetBaseObjectType() == (BaseObjectType)num43;
				}
				return true;
			}
			return false;
		}
		case ConditionType.IsMale:
		{
			Character subject122 = GetSubject<Character>(actor, target, obj, param);
			if (subject122 != null)
			{
				return subject122.Appearance.Gender == GenderType.Male;
			}
			return false;
		}
		case ConditionType.IsFemale:
		{
			Character subject123 = GetSubject<Character>(actor, target, obj, param);
			if (subject123 != null)
			{
				return subject123.Appearance.Gender == GenderType.Female;
			}
			return false;
		}
		case ConditionType.IsPlantableCrop:
			return GetSubject<PlantableCrop>(actor, target, obj, param) != null;
		case ConditionType.IsVehicle:
			return GetSubject<EnterableVehicle>(actor, target, obj, param) != null;
		case ConditionType.IsDriveableVehicle:
			return GetSubject<EnterableVehicle>(actor, target, obj, param)?.IsDriveable ?? false;
		case ConditionType.IsAttractedTo:
		{
			Character subject94 = GetSubject<Character>(actor, target, obj, param);
			Character character21 = GetObject<Character>(actor, target, obj, param);
			if (subject94 != null && character21 != null && subject94.IsAttractedTo(character21.GetGender()))
			{
				return !Relationship.IsFamily(subject94, character21);
			}
			return false;
		}
		case ConditionType.IsAttractedToGender:
		{
			Character subject76 = GetSubject<Character>(actor, target, obj, param);
			int num15 = Array.IndexOf(CharacterAppearance.GenderTypeNames, StringData);
			if (subject76 != null && num15 != -1)
			{
				return subject76.IsAttractedTo((GenderType)num15);
			}
			return false;
		}
		case ConditionType.AreSameGender:
		{
			Character subject57 = GetSubject<Character>(actor, target, obj, param);
			Character character11 = GetObject<Character>(actor, target, obj, param);
			if (subject57 != null && character11 != null)
			{
				return subject57.Appearance.Gender == character11.Appearance.Gender;
			}
			return false;
		}
		case ConditionType.HasNFoodItems:
		{
			Character subject43 = GetSubject<Character>(actor, target, obj, param);
			if (subject43 != null)
			{
				return (float)subject43.Inventory.CountFoodItems() >= Data;
			}
			Community subjectCommunity9 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity9 != null)
			{
				return (float)subjectCommunity9.CountFoodItems() >= Data;
			}
			return false;
		}
		case ConditionType.HasNWater:
		{
			Character subject16 = GetSubject<Character>(actor, target, obj, param);
			if (subject16 != null)
			{
				return subject16.Inventory.GetTotalWater() >= Data;
			}
			Community subjectCommunity = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity != null)
			{
				return subjectCommunity.GetTotalWater() >= Data;
			}
			return false;
		}
		case ConditionType.HasFood:
		{
			Character subject213 = GetSubject<Character>(actor, target, obj, param);
			Character character53 = GetObject<Character>(actor, target, obj, param);
			if (subject213 != null)
			{
				return subject213.Inventory.GetFood(subject213, (character53 != null) ? character53 : subject213, includeGifts: false, ignoreIfUsingForCrafting: true, BoolData) != null;
			}
			return false;
		}
		case ConditionType.HasAnythingEdible:
		{
			Character subject208 = GetSubject<Character>(actor, target, obj, param);
			if (subject208 != null)
			{
				if (BoolData)
				{
					return subject208.Inventory.HasAnythingToShowOnSellingFoodScreen(subject208);
				}
				return subject208.Inventory.GetTotalNutrition() > 0f;
			}
			return false;
		}
		case ConditionType.HasDrink:
		{
			Character subject164 = GetSubject<Character>(actor, target, obj, param);
			Character character38 = GetObject<Character>(actor, target, obj, param);
			if (subject164 != null)
			{
				return subject164.Inventory.GetBestWaterBottleToDrink(subject164, (character38 != null) ? character38 : subject164, BoolData) != null;
			}
			return false;
		}
		case ConditionType.HasAlcoholicDrink:
		{
			Character subject152 = GetSubject<Character>(actor, target, obj, param);
			Character character35 = GetObject<Character>(actor, target, obj, param);
			if (subject152 != null)
			{
				return subject152.Inventory.GetBestAlcoholToDrink(subject152, (character35 != null) ? character35 : subject152, BoolData) != null;
			}
			return false;
		}
		case ConditionType.HasAlcoholAmountTogether:
		{
			Character subject140 = GetSubject<Character>(actor, target, obj, param);
			Character character32 = GetObject<Character>(actor, target, obj, param);
			return (subject140?.Inventory.GetDrinkableAlcoholAmount(subject140, subject140, BoolData) ?? 0f) + (character32?.Inventory.GetDrinkableAlcoholAmount(character32, character32, BoolData) ?? 0f) >= Data;
		}
		case ConditionType.HasRangedWeapon:
		{
			Character subject136 = GetSubject<Character>(actor, target, obj, param);
			if (subject136 != null)
			{
				return subject136.Inventory.FindItemOfClass(typeof(RangedWeapon)) != null;
			}
			return false;
		}
		case ConditionType.HasAmmoWeapon:
		{
			Character subject131 = GetSubject<Character>(actor, target, obj, param);
			if (subject131 != null)
			{
				return subject131.Inventory.FindItemOfClass(typeof(AmmoWeapon)) != null;
			}
			return false;
		}
		case ConditionType.HasMeleeWeapon:
		{
			Character subject127 = GetSubject<Character>(actor, target, obj, param);
			if (subject127 != null)
			{
				return subject127.Inventory.FindItemOfClass(typeof(MeleeWeapon)) != null;
			}
			return false;
		}
		case ConditionType.HasGun:
		{
			Character subject108 = GetSubject<Character>(actor, target, obj, param);
			if (subject108 != null && subject108.Inventory.FindItemOfClass(typeof(Gun)) is Gun gun)
			{
				if (BoolData)
				{
					return gun.CurrentAmmo > 0;
				}
				return true;
			}
			return false;
		}
		case ConditionType.HasBow:
		{
			Character subject107 = GetSubject<Character>(actor, target, obj, param);
			if (subject107 != null)
			{
				return subject107.Inventory.FindItemOfClass(typeof(Bow)) != null;
			}
			return false;
		}
		case ConditionType.HasMolotovCocktail:
		{
			Character subject101 = GetSubject<Character>(actor, target, obj, param);
			if (subject101 != null)
			{
				return subject101.Inventory.FindItemOfClass(typeof(MolotovCocktail)) != null;
			}
			return false;
		}
		case ConditionType.EquippedRangedWeapon:
		{
			Character subject96 = GetSubject<Character>(actor, target, obj, param);
			if (subject96 != null)
			{
				return subject96.EquippedItem is RangedWeapon;
			}
			return false;
		}
		case ConditionType.EquippedMeleeWeapon:
		{
			Character subject87 = GetSubject<Character>(actor, target, obj, param);
			if (subject87 != null)
			{
				return subject87.EquippedItem is MeleeWeapon;
			}
			return false;
		}
		case ConditionType.EquippedGun:
		{
			Character subject88 = GetSubject<Character>(actor, target, obj, param);
			if (subject88 != null)
			{
				return subject88.EquippedItem is Gun;
			}
			return false;
		}
		case ConditionType.EquippedBow:
		{
			Character subject71 = GetSubject<Character>(actor, target, obj, param);
			if (subject71 != null)
			{
				return subject71.EquippedItem is Bow;
			}
			return false;
		}
		case ConditionType.EquippedMolotovCocktail:
		{
			Character subject68 = GetSubject<Character>(actor, target, obj, param);
			if (subject68 != null)
			{
				return subject68.EquippedItem is MolotovCocktail;
			}
			return false;
		}
		case ConditionType.HasItem:
		{
			Character subject48 = GetSubject<Character>(actor, target, obj, param);
			Community subjectCommunity11 = GetSubjectCommunity(actor, target, obj, param);
			Equipment objectEquipment4 = GetObjectEquipment(actor, target, obj, param);
			if (objectEquipment4 != null)
			{
				if (subject48 != null)
				{
					return subject48.InventoryContains(objectEquipment4);
				}
				if (subjectCommunity11 != null)
				{
					foreach (Character member2 in subjectCommunity11.Members)
					{
						if (member2.InventoryContains(objectEquipment4))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		case ConditionType.HasNItemsOfType:
		{
			EquipmentPrototype equipmentPrototype3 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype3 != null)
			{
				TileObject subject37 = GetSubject<TileObject>(actor, target, obj, param);
				if (subject37 != null && subject37.GetInventory() != null)
				{
					return (float)subject37.GetInventory().CountItemsOfType(equipmentPrototype3) >= Data;
				}
				Community subjectCommunity8 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity8 != null)
				{
					return (float)subjectCommunity8.CountInventoryItemsOfType(equipmentPrototype3) >= Data;
				}
			}
			return false;
		}
		case ConditionType.CommunityHasNItemsOfType:
		{
			EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype2 != null)
			{
				Community subjectCommunity2 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity2 != null)
				{
					return (float)subjectCommunity2.CountInventoryItemsOfType(equipmentPrototype2) >= Data;
				}
			}
			return false;
		}
		case ConditionType.WorldHasNItemsOfType:
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype != null)
			{
				return (float)Session.Instance.GetNumEquipmentSpawns(equipmentPrototype) >= Data;
			}
			return false;
		}
		case ConditionType.HasNItemsOfClothingType:
		{
			int num67 = Array.IndexOf(Character.ClothingTypeNames, StringData);
			if (num67 != -1)
			{
				Character subject223 = GetSubject<Character>(actor, target, obj, param);
				if (subject223 != null)
				{
					return (float)subject223.Inventory.CountItemsOfClothingType((ClothingType)num67) >= Data;
				}
				Community subjectCommunity65 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity65 != null)
				{
					return (float)subjectCommunity65.CountInventoryItemsOfClothingType((ClothingType)num67) >= Data;
				}
			}
			return false;
		}
		case ConditionType.CommunityHasNItemsOfClothingType:
		{
			int num59 = Array.IndexOf(Character.ClothingTypeNames, StringData);
			if (num59 != -1)
			{
				Community subjectCommunity64 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity64 != null)
				{
					return (float)subjectCommunity64.CountInventoryItemsOfClothingType((ClothingType)num59) >= Data;
				}
			}
			return false;
		}
		case ConditionType.CommunityHasNMembersWithGiftedItemsOfType:
		{
			Community subjectCommunity62 = GetSubjectCommunity(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype10 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subjectCommunity62 != null && equipmentPrototype10 != null)
			{
				return (float)subjectCommunity62.CountMembersWithGiftedItemsOfType(equipmentPrototype10) >= Data;
			}
			return false;
		}
		case ConditionType.HasNGiftedItemsOfType:
		{
			BaseObject subject159 = GetSubject<BaseObject>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype9 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype9 != null)
			{
				if (subject159 is TileObject tileObject7 && tileObject7.GetInventory() != null)
				{
					return (float)tileObject7.GetInventory().CountGiftedItemsOfType(equipmentPrototype9) >= Data;
				}
				if (subject159 is Community community3)
				{
					return (float)community3.CountGiftedItemsOfType(equipmentPrototype9) >= Data;
				}
			}
			return false;
		}
		case ConditionType.HasTradedNItemsOfType:
		{
			BaseObject subject146 = GetSubject<BaseObject>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype8 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype8 != null)
			{
				if (subject146 is TileObject tileObject5 && tileObject5.GetInventory() != null)
				{
					return (float)tileObject5.GetInventory().CountTradedItemsOfType(equipmentPrototype8) >= Data;
				}
				if (subject146 is Community community2)
				{
					return (float)community2.CountTradedInventoryItemsOfType(equipmentPrototype8) >= Data;
				}
			}
			return false;
		}
		case ConditionType.HasLiquidOfType:
		{
			LiquidPrototype liquidPrototype3 = GameImpl.Instance.FindLiquidPrototypeByName(StringData);
			if (liquidPrototype3 != null)
			{
				Character subject138 = GetSubject<Character>(actor, target, obj, param);
				if (subject138 != null)
				{
					return subject138.Inventory.GetAmountOfLiquidType(liquidPrototype3) >= Data;
				}
				Community subjectCommunity47 = GetSubjectCommunity(actor, target, obj, param);
				if (subjectCommunity47 != null)
				{
					return subjectCommunity47.GetTotalLiquid(liquidPrototype3) >= Data;
				}
			}
			return false;
		}
		case ConditionType.HasBackpack:
		{
			Character subject132 = GetSubject<Character>(actor, target, obj, param);
			if (subject132 != null)
			{
				return (float)subject132.Inventory.CountBackpacks() >= Data;
			}
			Community subjectCommunity40 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity40 != null)
			{
				return (float)subjectCommunity40.CountBackpacks() >= Data;
			}
			return false;
		}
		case ConditionType.HasGold:
		{
			TileObject subject116 = GetSubject<TileObject>(actor, target, obj, param);
			float num39 = Data;
			if (ConditionBlocks != null)
			{
				for (int m = 0; m < ConditionBlocks.Count; m++)
				{
					if (ConditionBlocks[m].GetConditionBlock() != null)
					{
						num39 += ConditionBlocks[m].GetConditionBlock().Evaluate(actor, target, obj, param);
					}
				}
			}
			if (subject116 != null && subject116.GetInventory() != null)
			{
				return (float)subject116.GetInventory().GetGoldAmount() >= num39;
			}
			Community subjectCommunity34 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity34 != null)
			{
				return (float)subjectCommunity34.GetGoldAmount() >= num39;
			}
			return false;
		}
		case ConditionType.HasGiftableItems:
		{
			Character subject103 = GetSubject<Character>(actor, target, obj, param);
			if (subject103 != null)
			{
				return (float)subject103.Inventory.GetNumGiftableItems() >= Data;
			}
			Community subjectCommunity29 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity29 != null)
			{
				return (float)subjectCommunity29.GetNumGiftableItems() >= Data;
			}
			return false;
		}
		case ConditionType.HasSuppliesOfValue:
		{
			TileObject subject92 = GetSubject<TileObject>(actor, target, obj, param);
			float num26 = Data;
			if (ConditionBlocks != null)
			{
				for (int k = 0; k < ConditionBlocks.Count; k++)
				{
					if (ConditionBlocks[k].GetConditionBlock() != null)
					{
						num26 += ConditionBlocks[k].GetConditionBlock().Evaluate(actor, target, obj, param);
					}
				}
			}
			if (subject92 != null && subject92.GetInventory() != null)
			{
				return subject92.GetInventory().CalcLootableSuppliesValue(subject92) >= num26;
			}
			Community subjectCommunity23 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity23 != null)
			{
				return subjectCommunity23.CalcLootableSuppliesValue() >= num26;
			}
			return false;
		}
		case ConditionType.CommunityOccupiesBuilding:
		{
			Community subjectCommunity22 = GetSubjectCommunity(actor, target, obj, param);
			Building building3 = GetObject<Building>(actor, target, obj, param);
			if (subjectCommunity22 != null && building3 != null)
			{
				return building3.GetOccupiedByCommunity() == subjectCommunity22;
			}
			return false;
		}
		case ConditionType.CommunityOwnsBuilding:
		{
			Community subjectCommunity17 = GetSubjectCommunity(actor, target, obj, param);
			Building building2 = GetObject<Building>(actor, target, obj, param);
			if (subjectCommunity17 != null && building2 != null)
			{
				return building2.Community == subjectCommunity17;
			}
			return false;
		}
		case ConditionType.IsMostLikedCommunityMember:
		{
			Character subject73 = GetSubject<Character>(actor, target, obj, param);
			Character character12 = GetObject<Character>(actor, target, obj, param);
			if (subject73 != null && character12 != null && character12.Community != null)
			{
				Character communityMemberMostLikedBy = character12.Community.GetCommunityMemberMostLikedBy(subject73, BoolData);
				return character12 == communityMemberMostLikedBy;
			}
			return false;
		}
		case ConditionType.LikesGiftType:
		{
			EquipmentPrototype equipmentPrototype4 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			Character subject69 = GetSubject<Character>(actor, target, obj, param);
			if (subject69 != null && equipmentPrototype4 != null)
			{
				return subject69.LikesGiftType(equipmentPrototype4, null);
			}
			return false;
		}
		case ConditionType.LikesGiftLiquid:
		{
			LiquidPrototype liquidPrototype = GameImpl.Instance.FindLiquidPrototypeByName(StringData);
			Character subject56 = GetSubject<Character>(actor, target, obj, param);
			if (subject56 != null && liquidPrototype != null)
			{
				return subject56.LikesGiftLiquid(liquidPrototype, null);
			}
			return false;
		}
		case ConditionType.LikesGift:
		{
			Character subject40 = GetSubject<Character>(actor, target, obj, param);
			Equipment objectEquipment3 = GetObjectEquipment(actor, target, obj, param);
			if (subject40 != null && objectEquipment3 != null)
			{
				return subject40.LikesGift(objectEquipment3, null);
			}
			return false;
		}
		case ConditionType.DislikesGift:
		{
			Character subject31 = GetSubject<Character>(actor, target, obj, param);
			Equipment objectEquipment2 = GetObjectEquipment(actor, target, obj, param);
			if (subject31 != null && objectEquipment2 != null)
			{
				return subject31.DislikesGift(objectEquipment2, null);
			}
			return false;
		}
		case ConditionType.KnowsName:
		{
			Character subject29 = GetSubject<Character>(actor, target, obj, param);
			Character character6 = GetObject<Character>(actor, target, obj, param);
			if (subject29 != null && character6 != null)
			{
				return subject29.KnowsName(character6);
			}
			return false;
		}
		case ConditionType.NameIsKnownByPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.NameKnown ?? false;
		case ConditionType.CommunityNameIsKnownByPlayer:
			return GetSubjectCommunity(actor, target, obj, param)?.CommunityNameKnown ?? false;
		case ConditionType.BackgroundIsKnownByPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.BackgroundKnown ?? false;
		case ConditionType.VariableEquals:
		{
			BaseObject subject20 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob3 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.GetVariable(StringData, subject20, ob3) == Data;
		}
		case ConditionType.VariableIsGreaterThan:
		{
			BaseObject subject8 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob2 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.GetVariable(StringData, subject8, ob2) > Data;
		}
		case ConditionType.VariableIsLessThan:
		{
			BaseObject subject222 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob9 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.GetVariable(StringData, subject222, ob9) < Data;
		}
		case ConditionType.VariableIsGreaterThanOrEqual:
		{
			BaseObject subject5 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.GetVariable(StringData, subject5, ob) >= Data;
		}
		case ConditionType.VariableIsLessThanOrEqual:
		{
			BaseObject subject224 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob10 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.GetVariable(StringData, subject224, ob10) <= Data;
		}
		case ConditionType.And:
		{
			bool flag2 = true;
			if (ConditionBlocks != null)
			{
				for (int num65 = 0; num65 < ConditionBlocks.Count; num65++)
				{
					ConditionBlock conditionBlock3 = ConditionBlocks[num65];
					if (conditionBlock3 != null)
					{
						flag2 &= conditionBlock3.Evaluate(actor, target, obj, param) != 0f;
					}
				}
			}
			return flag2;
		}
		case ConditionType.Or:
		{
			bool flag = false;
			if (ConditionBlocks != null)
			{
				for (int num58 = 0; num58 < ConditionBlocks.Count; num58++)
				{
					ConditionBlock conditionBlock2 = ConditionBlocks[num58];
					if (conditionBlock2 != null)
					{
						flag |= conditionBlock2.Evaluate(actor, target, obj, param) != 0f;
					}
				}
			}
			return flag;
		}
		case ConditionType.IsConditionBlockSatisfied:
			if (ConditionBlocks != null && ConditionBlocks.Count > 0)
			{
				ConditionBlock conditionBlock = ConditionBlocks[0];
				if (conditionBlock != null)
				{
					return conditionBlock.Evaluate(actor, target, obj, param) != 0f;
				}
			}
			return false;
		case ConditionType.QuestIsActive:
		{
			Character subject201 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty14 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob8 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestActive(StringData, subject201, thirdParty14, ob8);
		}
		case ConditionType.QuestIsActiveGivenBy:
		{
			Character subject200 = GetSubject<Character>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestActiveGivenBy(StringData, subject200);
		}
		case ConditionType.QuestIsActiveWithObject:
		{
			Character subject199 = GetSubject<Character>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestActiveWithObject(StringData, subject199);
		}
		case ConditionType.QuestIsActiveWithSeeker:
		{
			Character subject207 = GetSubject<Character>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestActiveWithSeeker(StringData, subject207);
		}
		case ConditionType.QuestIsActiveWithAnyParams:
			return StoryManager.Instance.IsQuestActiveWithAnyParams(StringData);
		case ConditionType.QuestIsDiscovered:
		{
			Character subject195 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty11 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob6 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestDiscovered(StringData, subject195, thirdParty11, ob6);
		}
		case ConditionType.QuestIsDiscoveredGivenBy:
		{
			Character subject190 = GetSubject<Character>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestDiscoveredGivenBy(StringData, subject190);
		}
		case ConditionType.QuestIsDiscoveredWithObject:
		{
			BaseObject subject193 = GetSubject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestDiscoveredWithObject(StringData, subject193);
		}
		case ConditionType.QuestIsDiscoveredGivenByWithSeeker:
		{
			Character subject191 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty9 = GetThirdParty<Character>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestDiscoveredGivenByWithSeeker(StringData, subject191, thirdParty9);
		}
		case ConditionType.QuestIsDiscoveredWithAnyParams:
			return StoryManager.Instance.IsQuestDiscoveredWithAnyParams(StringData);
		case ConditionType.QuestIsCompleted:
		{
			Character subject196 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty12 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob7 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestCompleted(StringData, subject196, thirdParty12, ob7);
		}
		case ConditionType.QuestIsFailed:
		{
			Character subject181 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty5 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob5 = GetObject<BaseObject>(actor, target, obj, param);
			return StoryManager.Instance.IsQuestFailed(StringData, subject181, thirdParty5, ob5);
		}
		case ConditionType.QuestGroupIsFailed:
		{
			Character subject182 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty6 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject baseObject9 = GetObject<BaseObject>(actor, target, obj, param);
			foreach (KeyValuePair<int, QuestInstance> questInstance in StoryManager.Instance.QuestInstances)
			{
				if (questInstance.Value.Quest.GroupID == StringData && questInstance.Value.QuestGiver == subject182 && questInstance.Value.QuestSeeker == thirdParty6 && questInstance.Value.QuestObject == baseObject9 && questInstance.Value.State == QuestInstance.EState.Failed)
				{
					return true;
				}
			}
			return false;
		}
		case ConditionType.QuestGroupIsActiveGivenBy:
		{
			Character subject186 = GetSubject<Character>(actor, target, obj, param);
			foreach (QuestInstance activeQuest in StoryManager.Instance.ActiveQuests)
			{
				if (activeQuest.Quest.GroupID == StringData && activeQuest.QuestGiver == subject186)
				{
					return true;
				}
			}
			return false;
		}
		case ConditionType.QuestGroupIsActiveGivenByWithSeeker:
		{
			Character subject175 = GetSubject<Character>(actor, target, obj, param);
			Character character41 = GetObject<Character>(actor, target, obj, param);
			foreach (QuestInstance activeQuest2 in StoryManager.Instance.ActiveQuests)
			{
				if (activeQuest2.Quest.GroupID == StringData && activeQuest2.QuestGiver == subject175 && activeQuest2.QuestSeeker == character41)
				{
					return true;
				}
			}
			return false;
		}
		case ConditionType.QuestGroupIsActiveWithAnyParams:
			foreach (KeyValuePair<int, QuestInstance> questInstance2 in StoryManager.Instance.QuestInstances)
			{
				if (questInstance2.Value.Quest.GroupID == StringData && questInstance2.Value.State == QuestInstance.EState.Active)
				{
					return true;
				}
			}
			return false;
		case ConditionType.TimeSinceQuestStarted:
		{
			Character subject172 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty2 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob4 = GetObject<BaseObject>(actor, target, obj, param);
			return (float)(Session.Instance.PlayTime - StoryManager.Instance.GetTimeSinceQuestStarted(StringData, subject172, thirdParty2, ob4)).TotalSeconds >= Data;
		}
		case ConditionType.TimeSinceLastSpoke:
		{
			Character subject168 = GetSubject<Character>(actor, target, obj, param);
			if (subject168 != null)
			{
				return (Session.Instance.PlayTime - subject168.SpeechStartTime).TotalSeconds >= (double)Data;
			}
			return false;
		}
		case ConditionType.IsAnyActiveQuestGivenBy:
		{
			Character subject166 = GetSubject<Character>(actor, target, obj, param);
			if (subject166 != null)
			{
				foreach (KeyValuePair<int, QuestInstance> questInstance3 in StoryManager.Instance.QuestInstances)
				{
					if (questInstance3.Value.QuestGiver == subject166 && questInstance3.Value.State == QuestInstance.EState.Active)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsAgeAtLeast:
		{
			Character subject160 = GetSubject<Character>(actor, target, obj, param);
			if (subject160 != null)
			{
				return subject160.Appearance.Age >= Data;
			}
			return false;
		}
		case ConditionType.IsOlderThan:
		{
			Character subject158 = GetSubject<Character>(actor, target, obj, param);
			Character character37 = GetObject<Character>(actor, target, obj, param);
			if (subject158 != null && character37 != null)
			{
				return subject158.Appearance.Age >= character37.Appearance.Age;
			}
			return false;
		}
		case ConditionType.AgeDifferenceIsLessThan:
		{
			Character subject151 = GetSubject<Character>(actor, target, obj, param);
			Character character34 = GetObject<Character>(actor, target, obj, param);
			if (subject151 != null && character34 != null)
			{
				return Math.Abs(subject151.Appearance.Age - character34.Appearance.Age) < Data;
			}
			return false;
		}
		case ConditionType.IsInDatingAgeRange:
		{
			Character subject143 = GetSubject<Character>(actor, target, obj, param);
			Character character33 = GetObject<Character>(actor, target, obj, param);
			if (subject143 != null && character33 != null)
			{
				return subject143.IsInDatingAgeRange(character33);
			}
			return false;
		}
		case ConditionType.IsCrouching:
			return GetSubject<Character>(actor, target, obj, param)?.IsCrouching() ?? false;
		case ConditionType.IsSitting:
			return GetSubject<Character>(actor, target, obj, param)?.IsSitting() ?? false;
		case ConditionType.IsOutdoors:
			return GetSubject<Character>(actor, target, obj, param)?.IsOutdoors() ?? false;
		case ConditionType.IsInvestigated:
			return GetSubject<TileObject>(actor, target, obj, param)?.IsInvestigated() ?? false;
		case ConditionType.IsPlayingDead:
			return GetSubject<Character>(actor, target, obj, param)?.IsPlayingDead() ?? false;
		case ConditionType.GateIsOpen:
			if (BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID) is Gate gate2)
			{
				return gate2.GateState == GateState.Open;
			}
			return true;
		case ConditionType.GateStateEquals:
		{
			Gate gate = BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID) as Gate;
			int num40 = Array.IndexOf(Gate.GateStateNames, StringData);
			if (gate != null && num40 != -1)
			{
				return gate.State == (GateState)num40;
			}
			return false;
		}
		case ConditionType.HasUnbandagedInjury:
			return GetSubject<Character>(actor, target, obj, param)?.HasUnbandagedInjury(0) ?? false;
		case ConditionType.CanBandageInjury:
		{
			Character subject111 = GetSubject<Character>(actor, target, obj, param);
			Character character26 = GetObject<Character>(actor, target, obj, param);
			if (subject111 != null && character26 != null)
			{
				return character26.HasUnbandagedInjury(Math.Min(subject111.GetSkillLevelWithEffects(SkillType.Medicine), Math.Max(0, subject111.Inventory.FindHighestBandageLevel())));
			}
			return false;
		}
		case ConditionType.CanAffordMedicalAttentionFrom:
		{
			Character subject113 = GetSubject<Character>(actor, target, obj, param);
			Character character27 = GetObject<Character>(actor, target, obj, param);
			if (subject113 != null && character27 != null)
			{
				return subject113.Inventory.GetGoldAmount() >= character27.GetPriceToBandage(subject113);
			}
			return false;
		}
		case ConditionType.HasInjuryInBodyLocation:
		{
			Character subject105 = GetSubject<Character>(actor, target, obj, param);
			int num32 = Array.IndexOf(Injury.InjuryLocationNames, StringData);
			if (subject105 != null && num32 >= 0)
			{
				return subject105.HasInjury((InjuryLocation)num32);
			}
			return false;
		}
		case ConditionType.HasInjuryOfType:
		{
			Character subject97 = GetSubject<Character>(actor, target, obj, param);
			int num29 = Array.IndexOf(Injury.InjuryTypeNames, StringData);
			if (subject97 != null && num29 >= 0)
			{
				return subject97.HasInjuryOfType((InjuryType)num29);
			}
			return false;
		}
		case ConditionType.HasInfectedInjury:
			return GetSubject<Character>(actor, target, obj, param)?.HasAnyInfectedInjuries() ?? false;
		case ConditionType.HasInfectedInjuryOfType:
		{
			Character subject89 = GetSubject<Character>(actor, target, obj, param);
			int num22 = Array.IndexOf(Injury.InjuryTypeNames, StringData);
			if (subject89 != null && num22 >= 0)
			{
				return subject89.HasInfectedInjuryOfType((InjuryType)num22);
			}
			return false;
		}
		case ConditionType.HasInfectedInjuryWithStrain:
		{
			Character subject99 = GetSubject<Character>(actor, target, obj, param);
			int num30 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
			if (subject99 != null && num30 >= 0)
			{
				return subject99.HasInjuryWithInfectionType((InfectionType)num30);
			}
			return false;
		}
		case ConditionType.MostRecentInfectedInjuryHadStrain:
		{
			Character subject82 = GetSubject<Character>(actor, target, obj, param);
			int num18 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
			if (subject82 != null && num18 >= 0)
			{
				return subject82.GetStrainFromMostRecentInfectedInjury() == (InfectionType)num18;
			}
			return false;
		}
		case ConditionType.CommunityHasNoAntigenKnown:
		{
			Community subjectCommunity18 = GetSubjectCommunity(actor, target, obj, param);
			int num19 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
			if (subjectCommunity18 != null && num19 >= 0)
			{
				return subjectCommunity18.IsNoAntigenKnown((InfectionType)num19);
			}
			return false;
		}
		case ConditionType.HasAntigenFor:
		{
			Character subject75 = GetSubject<Character>(actor, target, obj, param);
			Character character13 = GetObject<Character>(actor, target, obj, param);
			if (subject75 != null && character13 != null)
			{
				return subject75.Inventory.HasAntigenForInjuries(character13);
			}
			return false;
		}
		case ConditionType.HasInvisibleStrain:
		{
			Character subject66 = GetSubject<Character>(actor, target, obj, param);
			if (subject66 != null)
			{
				return subject66.InvisibleStrain != InvisibleStrainType.None;
			}
			return false;
		}
		case ConditionType.HasInvisibleStrainType:
		{
			Character subject61 = GetSubject<Character>(actor, target, obj, param);
			int num10 = Array.IndexOf(Injury.InvisibleStrainTypeNames, StringData);
			if (subject61 != null && num10 != -1)
			{
				return subject61.InvisibleStrain == (InvisibleStrainType)num10;
			}
			return false;
		}
		case ConditionType.HadInvisibleStrainFromStart:
			return GetSubject<Character>(actor, target, obj, param)?.HadInvisibleStrainFromStart ?? false;
		case ConditionType.CommunityHasInvisibleStrainExceptPlayer:
			return GetSubjectCommunity(actor, target, obj, param)?.HasInvisibleStrainExceptPlayer() ?? false;
		case ConditionType.CommunityHasAnyoneWithInvisibleStrain:
			return GetSubjectCommunity(actor, target, obj, param)?.HasAnyoneWithInvisibleStrain() ?? false;
		case ConditionType.WillActivateDoomStrain:
			return GetSubject<Character>(actor, target, obj, param)?.WillActivateDoomStrain() ?? false;
		case ConditionType.CommunityIsTradingAtCamp:
		{
			Community subjectCommunity3 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity3 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity3 != null && objectCommunity3 != null && subjectCommunity3.Squads.Count > 0)
			{
				Squad squad2 = subjectCommunity3.Squads[0];
				if (!squad2.MovedAlong && squad2.GetLeader() != null)
				{
					switch (squad2.Behaviour)
					{
					case SquadBehaviour.Trade:
						if (squad2.Action == SquadAction.Trade)
						{
							return objectCommunity3.GetDistSqToNearestBuilding(squad2.GoalTile) <= MathUtil.Squared(32f);
						}
						return false;
					case SquadBehaviour.Travel:
						if (squad2.Action == SquadAction.Wait)
						{
							return objectCommunity3.GetDistSqToNearestBuilding(squad2.GetLeader().Tile) <= MathUtil.Squared(32f);
						}
						return false;
					}
				}
			}
			return false;
		}
		case ConditionType.CommunityIsHidden:
			return GetSubjectCommunity(actor, target, obj, param)?.HiddenCommunity ?? false;
		case ConditionType.IsObjectInInventory:
		{
			Character subject17 = GetSubject<Character>(actor, target, obj, param);
			Equipment objectEquipment = GetObjectEquipment(actor, target, obj, param);
			if (subject17 != null && objectEquipment != null)
			{
				return subject17.Inventory.Contains(objectEquipment);
			}
			return false;
		}
		case ConditionType.CanShareItemWith:
		{
			Character subject3 = GetSubject<Character>(actor, target, obj, param);
			Character character = GetObject<Character>(actor, target, obj, param);
			Equipment thirdPartyEquipment = GetThirdPartyEquipment(actor, target, obj, param);
			if (subject3 != null && character != null && thirdPartyEquipment != null)
			{
				if (character.Community != subject3.Community)
				{
					return subject3.IsActionAllowedForItem(thirdPartyEquipment, EquipmentPolicyAction.CanShare);
				}
				return true;
			}
			return false;
		}
		case ConditionType.CanTreat:
		{
			Character subject221 = GetSubject<Character>(actor, target, obj, param);
			Character character55 = GetObject<Character>(actor, target, obj, param);
			Recipe recipe;
			if (subject221 != null && character55 != null)
			{
				return TreatGoal.PickCraftingPropAndRecipe(subject221, character55, out recipe) != null;
			}
			return false;
		}
		case ConditionType.CanAffordPrice:
		{
			Character subject215 = GetSubject<Character>(actor, target, obj, param);
			Character character54 = GetObject<Character>(actor, target, obj, param);
			if (subject215 != null && character54 != null)
			{
				float num60 = Data;
				if (ConditionBlocks != null)
				{
					for (int num61 = 0; num61 < ConditionBlocks.Count; num61++)
					{
						if (ConditionBlocks[num61].GetConditionBlock() != null)
						{
							num60 += ConditionBlocks[num61].GetConditionBlock().Evaluate(character54, subject215, obj, param);
						}
					}
				}
				return Math.Max(1, Mathf.CeilToInt(character54.GetPriceToSell(num60, subject215))) <= subject215.Inventory.GetGoldAmount();
			}
			return false;
		}
		case ConditionType.CanAffordObject:
		{
			Character subject206 = GetSubject<Character>(actor, target, obj, param);
			Character character51 = GetObject<Character>(actor, target, obj, param);
			Equipment equipment2 = obj as Equipment;
			if (subject206 != null && character51 != null && equipment2 != null)
			{
				if (equipment2 != null)
				{
					return Math.Max(1f, character51.GetPriceToSell(equipment2, subject206)) <= (float)subject206.Inventory.GetGoldAmount();
				}
				return false;
			}
			return false;
		}
		case ConditionType.CanAffordItemOfType:
		{
			Character subject210 = GetSubject<Character>(actor, target, obj, param);
			Character character52 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype12 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject210 != null && character52 != null && equipmentPrototype12 != null)
			{
				Equipment equipment3 = character52.Inventory.FindItemOfType(equipmentPrototype12);
				if (equipment3 != null)
				{
					return Math.Max(1f, character52.GetPriceToSell(equipment3, subject210)) <= (float)subject210.Inventory.GetGoldAmount();
				}
				return Math.Max(1f, character52.GetPriceToSell(equipmentPrototype12, subject210)) <= (float)subject210.Inventory.GetGoldAmount();
			}
			return false;
		}
		case ConditionType.CanAffordToRefundItemOfType:
		{
			Character subject197 = GetSubject<Character>(actor, target, obj, param);
			GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype11 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (equipmentPrototype11 != null)
			{
				return equipmentPrototype11.BasePrice * Character.BaseMarkup <= (float)subject197.Inventory.GetGoldAmount();
			}
			return false;
		}
		case ConditionType.CanAffordWater:
		{
			Character subject188 = GetSubject<Character>(actor, target, obj, param);
			Character character45 = GetObject<Character>(actor, target, obj, param);
			if (subject188 != null && character45 != null)
			{
				Equipment bestWaterBottleToDrink = character45.Inventory.GetBestWaterBottleToDrink(character45, subject188, forSharing: true);
				if (bestWaterBottleToDrink != null)
				{
					return Math.Max(1f, character45.GetPriceToSell(bestWaterBottleToDrink, subject188)) <= (float)subject188.Inventory.GetGoldAmount();
				}
				return false;
			}
			return false;
		}
		case ConditionType.CanAffordFood:
		{
			Character subject184 = GetSubject<Character>(actor, target, obj, param);
			Character character43 = GetObject<Character>(actor, target, obj, param);
			if (subject184 != null && character43 != null)
			{
				Equipment food = character43.Inventory.GetFood(character43, subject184, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: true);
				if (food != null)
				{
					return Math.Max(1f, character43.GetPriceToSell(food, subject184)) <= (float)subject184.Inventory.GetGoldAmount();
				}
				return false;
			}
			return false;
		}
		case ConditionType.BuildingOfTypeIsOnMarker:
		{
			Marker marker = BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID) as Marker;
			int num54 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (marker != null && num54 != -1)
			{
				List<TileObject> objectsInLookupSquare = GameTerrain.Instance.GetObjectsInLookupSquare(marker.Tile.x, marker.Tile.y);
				if (objectsInLookupSquare != null)
				{
					foreach (TileObject item in objectsInLookupSquare)
					{
						if (item.GetBaseObjectType() == (BaseObjectType)num54 && marker.Tile.IsWithinBounds(item.GetMinTile(), item.GetMaxTile()))
						{
							if (item.GetUnderConstructionInfo() != null)
							{
								return false;
							}
							return true;
						}
					}
				}
			}
			return false;
		}
		case ConditionType.CommunityHasBuildingOfType:
		{
			Community subjectCommunity61 = GetSubjectCommunity(actor, target, obj, param);
			int num53 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (subjectCommunity61 != null && num53 != -1)
			{
				int desiredAmount = Math.Max(0, (int)Data);
				return subjectCommunity61.HasCompletedBuildingsOfType((BaseObjectType)num53, desiredAmount);
			}
			return false;
		}
		case ConditionType.CommunityHasPropWithPrototype:
		{
			Community subjectCommunity59 = GetSubjectCommunity(actor, target, obj, param);
			PropPrototype propPrototype3 = GameImpl.Instance.FindPropPrototypeByName(StringData);
			if (subjectCommunity59 != null && propPrototype3 != null)
			{
				return subjectCommunity59.HasCompletedBuildingsOfProto(propPrototype3, (int)Data);
			}
			return false;
		}
		case ConditionType.CommunityHasVehicleToHitTheRoad:
		{
			Community subjectCommunity58 = GetSubjectCommunity(actor, target, obj, param);
			PropPrototype proto = GameImpl.Instance.FindPropPrototypeByName(StringData);
			return subjectCommunity58?.HasVehicleToHitTheRoad(BoolData, proto) ?? false;
		}
		case ConditionType.IsFullyTracked:
		{
			Character subject156 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject6 = GetObject<TileObject>(actor, target, obj, param);
			if (subject156 != null && tileObject6 != null)
			{
				return subject156.GetTarget(tileObject6)?.FullyTracked ?? false;
			}
			return false;
		}
		case ConditionType.CanSeeAnimal:
		{
			Character subject150 = GetSubject<Character>(actor, target, obj, param);
			int num52 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (num52 != -1 && subject150 != null)
			{
				foreach (Target target5 in subject150.Targets)
				{
					if (target5.Object != null && target5.Object.GetBaseObjectType() == (BaseObjectType)num52 && (Data == 0f || MathUtil.ToXZ(target5.LastKnownPosition - subject150.Pos).sqrMagnitude < Data * Data) && !target5.GetFlag(TargetFlags.Dead))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.CanSeeDeadAnimal:
		{
			Character subject147 = GetSubject<Character>(actor, target, obj, param);
			int num49 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (num49 != -1 && subject147 != null)
			{
				foreach (Target target6 in subject147.Targets)
				{
					if (target6.Object != null && target6.Object.GetBaseObjectType() == (BaseObjectType)num49 && (Data == 0f || MathUtil.ToXZ(target6.LastKnownPosition - subject147.Pos).sqrMagnitude < Data * Data) && target6.GetFlag(TargetFlags.Dead))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.CommunityHasCrops:
		{
			Community subjectCommunity51 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity51 == null)
			{
				return false;
			}
			return (float)Session.Instance.CropsManager.GetCommunityCropsCount(subjectCommunity51.Id) >= Data;
		}
		case ConditionType.CommunityHasAnimals:
		{
			Community subjectCommunity49 = GetSubjectCommunity(actor, target, obj, param);
			int num46 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (subjectCommunity49 == null || num46 == -1)
			{
				return false;
			}
			return (float)subjectCommunity49.GetLivingNonZombieMemberCountBySpecies((BaseObjectType)num46) >= Data;
		}
		case ConditionType.CommunityHasBuildings:
		{
			Community subjectCommunity48 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity48 == null)
			{
				return false;
			}
			return (float)subjectCommunity48.GetNumCompletedBuildings() >= Data;
		}
		case ConditionType.CommunityHasAccomodation:
		{
			Community subjectCommunity45 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity45 == null)
			{
				return false;
			}
			return (float)subjectCommunity45.GetAccommodation() >= Data;
		}
		case ConditionType.CommunityHasUnusedAccomodation:
		{
			Community subjectCommunity44 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity44 == null)
			{
				return false;
			}
			return (float)subjectCommunity44.GetAccommodation() >= (float)subjectCommunity44.GetLivingNonZombieMemberCount() + Data;
		}
		case ConditionType.CommunityHasLitCampfires:
		{
			Community subjectCommunity42 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity42 == null)
			{
				return false;
			}
			return (float)subjectCommunity42.GetNumLitCampfires() >= Data;
		}
		case ConditionType.CommunityHasGates:
		{
			Community subjectCommunity39 = GetSubjectCommunity(actor, target, obj, param);
			if (BoolData)
			{
				return subjectCommunity39?.HasGatesThatEncloseSomething((int)Data) ?? false;
			}
			if (subjectCommunity39 == null)
			{
				return false;
			}
			return (float)subjectCommunity39.GetNumCompletedBuildingsOfClass(typeof(Gate)) >= Data;
		}
		case ConditionType.NumberOfCommunitiesIntroduced:
		{
			Community subjectCommunity38 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity38 == null)
			{
				return false;
			}
			return (float)Session.Instance.CommunityManager.GetNumCommunitiesWithRelationship(subjectCommunity38, CommunityRelationshipType.Known) >= Data;
		}
		case ConditionType.CommunityIntroductionHasStarted:
		{
			Community subjectCommunity37 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity18 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity37 == null)
			{
				return true;
			}
			if (objectCommunity18 == null)
			{
				return true;
			}
			return Session.Instance.CommunityManager.GetRelationship(subjectCommunity37, objectCommunity18) >= CommunityRelationshipType.Introducing;
		}
		case ConditionType.CommunityIsIntroduced:
		{
			Community subjectCommunity35 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity16 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity35 == null)
			{
				return true;
			}
			if (objectCommunity16 == null)
			{
				return true;
			}
			return Session.Instance.CommunityManager.GetRelationship(subjectCommunity35, objectCommunity16) >= CommunityRelationshipType.Known;
		}
		case ConditionType.CommunityIsAtWar:
		{
			Community subjectCommunity36 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity17 = GetObjectCommunity(actor, target, obj, param);
			return Session.Instance.CommunityManager.GetRelationship(subjectCommunity36, objectCommunity17) == CommunityRelationshipType.Hostile;
		}
		case ConditionType.CommunityHasAlliance:
		{
			Community subjectCommunity33 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity15 = GetObjectCommunity(actor, target, obj, param);
			return Session.Instance.CommunityManager.GetRelationship(subjectCommunity33, objectCommunity15) == CommunityRelationshipType.Allied;
		}
		case ConditionType.CommunityHasAllianceOrDestroyed:
		{
			Community subjectCommunity32 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity13 = GetObjectCommunity(actor, target, obj, param);
			if (!subjectCommunity32.CachedAllies.Contains(objectCommunity13))
			{
				return !objectCommunity13.HasAnyActiveMembers();
			}
			return true;
		}
		case ConditionType.CommunityHasCeasedFire:
		{
			Community subjectCommunity31 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity12 = GetObjectCommunity(actor, target, obj, param);
			return Session.Instance.CommunityManager.GetRelationship(subjectCommunity31, objectCommunity12) == CommunityRelationshipType.Ceasefire;
		}
		case ConditionType.CommunityIsDefeated:
		{
			Community subjectCommunity30 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity11 = GetObjectCommunity(actor, target, obj, param);
			if (objectCommunity11 != null && objectCommunity11.HasAnyActiveMembers())
			{
				if (Session.Instance.CommunityManager.GetRelationship(subjectCommunity30, objectCommunity11, out var community1Surrendered) == CommunityRelationshipType.Ceasefire)
				{
					return !community1Surrendered;
				}
				return false;
			}
			return true;
		}
		case ConditionType.CommunityIsAtWarOrDefeated:
		{
			Community subjectCommunity28 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity10 = GetObjectCommunity(actor, target, obj, param);
			if (objectCommunity10 != null && objectCommunity10.HasAnyActiveMembers())
			{
				return Session.Instance.CommunityManager.GetRelationship(subjectCommunity28, objectCommunity10) >= CommunityRelationshipType.Ceasefire;
			}
			return true;
		}
		case ConditionType.CommunitySizeIsAtLeast:
		{
			Community subjectCommunity27 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity27 != null)
			{
				return (float)subjectCommunity27.GetActiveMemberCount() >= Data;
			}
			return false;
		}
		case ConditionType.CommunityHasConsciousMembers:
			return GetSubjectCommunity(actor, target, obj, param)?.HasConsciousMembers((int)Data) ?? false;
		case ConditionType.CommunityHasAnyActiveMembers:
			return GetSubjectCommunity(actor, target, obj, param)?.HasAnyActiveMembers() ?? false;
		case ConditionType.CommunityHasAnyConsciousMembers:
			return GetSubjectCommunity(actor, target, obj, param)?.IsAnyoneConscious(BoolData) ?? false;
		case ConditionType.CommunityHasTriggeredOnKilledEvents:
			return GetSubjectCommunity(actor, target, obj, param)?.HasTriggeredOnKilledEvents ?? false;
		case ConditionType.CommunitySizeIsLargest:
		{
			Community subjectCommunity21 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity7 = GetObjectCommunity(actor, target, obj, param);
			if (BoolData)
			{
				if (subjectCommunity21 != null && objectCommunity7 != null)
				{
					return subjectCommunity21.GetLivingNonZombieMemberCountIncludingAllies() > objectCommunity7.GetLivingNonZombieMemberCountIncludingAllies();
				}
				return false;
			}
			if (subjectCommunity21 != null && objectCommunity7 != null)
			{
				return subjectCommunity21.GetLivingNonZombieMemberCount() > objectCommunity7.GetLivingNonZombieMemberCount();
			}
			return false;
		}
		case ConditionType.CommunityHasDeadUnburied:
		{
			Community subjectCommunity20 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity20 != null)
			{
				return (float)subjectCommunity20.GetDeadUnburiedMemberCount() >= Data;
			}
			return false;
		}
		case ConditionType.CommunityIsUnhygienic:
			return GetSubjectCommunity(actor, target, obj, param)?.IsUnhygienic() ?? false;
		case ConditionType.CommunityLacksSpace:
			return GetSubjectCommunity(actor, target, obj, param)?.LacksSpace() ?? false;
		case ConditionType.ScriptedMoveHasSucceeded:
		{
			Character subject62 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject2 = GetObject<TileObject>(actor, target, obj, param);
			ScriptedGoal scriptedGoal = subject62?.GetScriptedGoal();
			if (!subject62.AliveAndNotZombie)
			{
				return false;
			}
			if (scriptedGoal != null)
			{
				if (tileObject2 != null && subject62.ScriptedGoalMarker != tileObject2)
				{
					return false;
				}
				return scriptedGoal.Success;
			}
			return false;
		}
		case ConditionType.Exists:
		{
			BaseObject subject54 = GetSubject<BaseObject>(actor, target, obj, param);
			if (subject54 != null)
			{
				return !subject54.Deleted;
			}
			if (GetSubjectEquipmentPrototype(actor, target, obj, param) != null)
			{
				return true;
			}
			if (GetSubjectLiquidPrototype(actor, target, obj, param) != null)
			{
				return true;
			}
			return false;
		}
		case ConditionType.RankEquals:
		{
			Character subject49 = GetSubject<Character>(actor, target, obj, param);
			int num8 = Array.IndexOf(Character.RankNames, StringData);
			if (subject49 != null && num8 != -1)
			{
				return subject49.GetRank() == (Rank)num8;
			}
			return false;
		}
		case ConditionType.RoleEquals:
		{
			Character subject39 = GetSubject<Character>(actor, target, obj, param);
			int num7 = Array.IndexOf(Character.RoleNames, StringData);
			if (subject39 != null && num7 != -1)
			{
				return subject39.HasRole((Role)num7);
			}
			return false;
		}
		case ConditionType.WantJoinCommunity:
		{
			Character subject34 = GetSubject<Character>(actor, target, obj, param);
			Character character9 = GetObject<Character>(actor, target, obj, param);
			if (subject34 != null && character9 != null)
			{
				return subject34.WantJoinCommunity(character9, threatened: false, BoolData);
			}
			return false;
		}
		case ConditionType.AnyoneWantsToJoinCommunity:
		{
			Community subjectCommunity4 = GetSubjectCommunity(actor, target, obj, param);
			Character character7 = GetObject<Character>(actor, target, obj, param);
			if (subjectCommunity4 != null && character7 != null)
			{
				foreach (Character member3 in subjectCommunity4.Members)
				{
					if (member3.AliveAndNotZombie && member3.GetBaseObjectType() == BaseObjectType.Human && member3.WantJoinCommunity(character7, threatened: false, BoolData))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.WantJoinCommunityWhenThreatened:
		{
			Character subject26 = GetSubject<Character>(actor, target, obj, param);
			Character character5 = GetObject<Character>(actor, target, obj, param);
			if (subject26 != null && character5 != null)
			{
				return subject26.WantJoinCommunity(character5, threatened: true, checkRelationships: false);
			}
			return false;
		}
		case ConditionType.WantLeaveCommunity:
		{
			Character subject19 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity2 = GetObjectCommunity(actor, target, obj, param);
			if (subject19 != null && objectCommunity2 != null)
			{
				return subject19.WantLeaveCommunity(objectCommunity2, BoolData);
			}
			return false;
		}
		case ConditionType.WantKickFromCommunity:
		{
			Character subject13 = GetSubject<Character>(actor, target, obj, param);
			Character character4 = GetObject<Character>(actor, target, obj, param);
			if (subject13 != null && subject13.Community != null && character4 != null)
			{
				return subject13.WantKickFromCommunity(character4, BoolData);
			}
			return false;
		}
		case ConditionType.WantToDeclareWar:
		{
			Character subject10 = GetSubject<Character>(actor, target, obj, param);
			Character character3 = GetObject<Character>(actor, target, obj, param);
			if (subject10 != null && character3 != null)
			{
				return subject10.WantToDeclareWar(character3);
			}
			return false;
		}
		case ConditionType.GetDontLeaveCommunity:
			return GetSubject<Character>(actor, target, obj, param)?.DontLeaveCommunity ?? false;
		case ConditionType.DistanceToIsLessThan:
		{
			TileObject subject4 = GetSubject<TileObject>(actor, target, obj, param);
			TileObject tileObject = GetObject<TileObject>(actor, target, obj, param);
			if (subject4 != null && tileObject != null)
			{
				return (subject4.PosXZ - tileObject.PosXZ).sqrMagnitude <= Data * Data;
			}
			return false;
		}
		case ConditionType.IsInZone:
		{
			Character subject220 = GetSubject<Character>(actor, target, obj, param);
			Zone zone2 = BaseObjectManager.Instance.GetObjectByUniqueID(ObjectID) as Zone;
			if (subject220 != null && zone2 != null)
			{
				return zone2.CharactersInZone.Contains(subject220);
			}
			return false;
		}
		case ConditionType.IsBuilt:
			if (BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID) is TileObject tileObject8)
			{
				return tileObject8.GetUnderConstructionInfo() != null;
			}
			return false;
		case ConditionType.IsGapInWall:
			if (BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID) is Zone zone)
			{
				for (int num62 = zone.MinTile.x; num62 <= zone.MaxTile.x; num62++)
				{
					for (int num63 = zone.MinTile.y; num63 <= zone.MaxTile.y; num63++)
					{
						TileObject fixedObjectOnTile = GameTerrain.Instance.GetFixedObjectOnTile(num62, num63);
						if (fixedObjectOnTile == null || (fixedObjectOnTile.GetUnderConstructionInfo() != null && !fixedObjectOnTile.GetUnderConstructionInfo().IsBuiltEnoughToBeAnObstacle()))
						{
							return true;
						}
					}
				}
			}
			return false;
		case ConditionType.InjuryIsRemarkedOn:
			return GetSubject<Character>(actor, target, obj, param)?.InjuryRemarkedOn ?? false;
		case ConditionType.InfectionIsRemarkedOn:
			return GetSubject<Character>(actor, target, obj, param)?.InfectionRemarkedOn ?? false;
		case ConditionType.UnbandagedInjuryIsOlderThan:
		{
			Character subject209 = GetSubject<Character>(actor, target, obj, param);
			if (subject209 != null)
			{
				return subject209.GetNewestUnbandagedInjuryAge() >= TimeSpan.FromSeconds(Data);
			}
			return false;
		}
		case ConditionType.InfectedInjuryIsOlderThan:
		{
			Character subject205 = GetSubject<Character>(actor, target, obj, param);
			if (subject205 != null)
			{
				return subject205.GetNewestInfectedInjuryAge() >= TimeSpan.FromSeconds(Data);
			}
			return false;
		}
		case ConditionType.IsShowingDialogueOptions:
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling && !playerRecord.FlyMode && !playerRecord.WantLockOnTarget && playerRecord.TargetObject is Character { AliveAndNotZombie: not false })
				{
					return true;
				}
			}
			return false;
		case ConditionType.HasAnyReplies:
		{
			Character subject204 = GetSubject<Character>(actor, target, obj, param);
			Character replier = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty15 = GetThirdParty<BaseObject>(actor, target, obj, param);
			Speech speech5 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			CustomRandom rand = new CustomRandom();
			MemoryParam param2 = param;
			BaseObject resultObj;
			bool multipleOptions;
			return StoryManager.Instance.GetReply(replier, subject204, thirdParty15, speech5, out resultObj, ref param2, out multipleOptions, rand) != null;
		}
		case ConditionType.HasSpokenOnce:
		{
			Speech speech4 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (speech4 != null)
			{
				return StoryManager.Instance.HasSpokenOnce(speech4);
			}
			return false;
		}
		case ConditionType.HasSpokenSpeech:
		{
			Character subject202 = GetSubject<Character>(actor, target, obj, param);
			Character character49 = GetObject<Character>(actor, target, obj, param);
			Speech speech3 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (subject202 != null && character49 != null && speech3 != null)
			{
				return subject202.GetSpokenSpeechIndex(speech3, character49) != -1;
			}
			return false;
		}
		case ConditionType.HasSpokenSpeechRecently:
		{
			Character subject192 = GetSubject<Character>(actor, target, obj, param);
			Character character46 = GetObject<Character>(actor, target, obj, param);
			Speech speech2 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (subject192 != null && character46 != null && speech2 != null)
			{
				return subject192.HasSpokenSpeechRecently(speech2, character46, Data);
			}
			return false;
		}
		case ConditionType.HasSpokenSpeechToAnyoneRecently:
		{
			Character subject187 = GetSubject<Character>(actor, target, obj, param);
			Speech speech = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (subject187 != null && speech != null)
			{
				return subject187.HasSpokenSpeechToAnyoneRecently(speech, Data);
			}
			return false;
		}
		case ConditionType.HasSpokenSpeechForSituationRecently:
		{
			Character subject183 = GetSubject<Character>(actor, target, obj, param);
			Character target3 = GetObject<Character>(actor, target, obj, param);
			int num56 = Array.IndexOf(Character.SpeechSituationNames, StringData);
			TimeSpan spokenTime2;
			if (subject183 != null && num56 != -1)
			{
				return subject183.HasSpokenSpeechForSituationRecently((SpeechSituation)num56, target3, TimeSpan.FromSeconds(Data), out spokenTime2);
			}
			return false;
		}
		case ConditionType.HasSpokenSpeechForSituationToAnyoneRecently:
		{
			Character subject179 = GetSubject<Character>(actor, target, obj, param);
			int num55 = Array.IndexOf(Character.SpeechSituationNames, StringData);
			TimeSpan spokenTime;
			if (subject179 != null && num55 != -1)
			{
				return subject179.HasSpokenSpeechForSituationToAnyoneRecently((SpeechSituation)num55, TimeSpan.FromSeconds(Data), out spokenTime);
			}
			return false;
		}
		case ConditionType.HasBeenPickpocketedRecently:
		{
			Character subject176 = GetSubject<Character>(actor, target, obj, param);
			if (subject176 != null)
			{
				return subject176.PickpocketDetection > 0f;
			}
			return false;
		}
		case ConditionType.IsInCombat:
		{
			Character subject173 = GetSubject<Character>(actor, target, obj, param);
			if (subject173 != null)
			{
				if (!subject173.InCombat && !subject173.AboutToBeInCombat)
				{
					return subject173.IsBeingBittenOrChoked();
				}
				return true;
			}
			return false;
		}
		case ConditionType.IsAnyMemberInCombat:
			return GetSubjectCommunity(actor, target, obj, param)?.IsAnyMemberInCombat() ?? false;
		case ConditionType.IsHighAlert:
			return GetSubject<Character>(actor, target, obj, param)?.IsHighAlert() ?? false;
		case ConditionType.IsLowAlert:
			return GetSubject<Character>(actor, target, obj, param)?.IsLowAlert() ?? false;
		case ConditionType.IsAnyoneInConversationWith:
		{
			Character subject163 = GetSubject<Character>(actor, target, obj, param);
			if (subject163 != null)
			{
				return subject163.IsAnyoneInConversationWithMe();
			}
			Community subjectCommunity57 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity57 != null)
			{
				foreach (Character member4 in subjectCommunity57.Members)
				{
					if (member4.AliveAndNotZombie && member4.IsAnyoneInConversationWithMe())
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsBoxer:
			return GetSubject<Character>(actor, target, obj, param)?.Boxer ?? false;
		case ConditionType.IsBoxerReadyToFight:
			return GetSubject<Character>(actor, target, obj, param)?.IsBoxerReadyToFight() ?? false;
		case ConditionType.IsKnownBoxer:
			return GetSubject<Character>(actor, target, obj, param)?.KnownBoxer ?? false;
		case ConditionType.HasEnoughGoldForBoxerWager:
		{
			Character subject153 = GetSubject<Character>(actor, target, obj, param);
			Character character36 = GetObject<Character>(actor, target, obj, param);
			if (subject153 != null && character36 != null)
			{
				return subject153.Inventory.GetGoldAmount() >= character36.GetBoxerWagerAmount();
			}
			return false;
		}
		case ConditionType.CommunityHasAnyBoxersReadyToFight:
			return GetSubjectCommunity(actor, target, obj, param)?.HasAnyBoxersReadyToFight() ?? false;
		case ConditionType.CommunityHasAnyoneWithRole:
		{
			Community subjectCommunity54 = GetSubjectCommunity(actor, target, obj, param);
			int num51 = Array.IndexOf(Character.RoleNames, StringData);
			if (num51 != -1 && subjectCommunity54 != null)
			{
				return subjectCommunity54.HasAnyoneWithRole((Role)num51);
			}
			return false;
		}
		case ConditionType.CommunityHasAnyGuardWithItem:
		{
			Community subjectCommunity52 = GetSubjectCommunity(actor, target, obj, param);
			EquipmentPrototype prototype = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subjectCommunity52 != null)
			{
				foreach (Character member5 in subjectCommunity52.Members)
				{
					if (member5.AliveAndNotZombie && member5.HasRole(Role.Guard) && member5.Inventory.FindItemOfType(prototype) != null)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.CommunityHasAnyoneCraftingItem:
		{
			Community subjectCommunity53 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity53 != null)
			{
				foreach (Character member6 in subjectCommunity53.Members)
				{
					if (!member6.AliveAndNotZombie)
					{
						continue;
					}
					for (int num50 = 0; num50 < member6.Roles.Count; num50++)
					{
						if (member6.Roles[num50].Role == Role.Crafter && member6.Roles[num50].Recipe != null && member6.Roles[num50].Recipe.ProductPrototype != null && member6.Roles[num50].Recipe.ProductPrototype.Name == StringData && (!BoolData || (member6.HasMovementZone() && (!(member6.Roles[num50].TargetLocation != TerrainCoord.Invalid) || member6.MovementZone.Contains(member6.Roles[num50].TargetLocation)))))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		case ConditionType.CommunityHasLeader:
		{
			Community subjectCommunity50 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity50 != null)
			{
				return subjectCommunity50.Leader != null;
			}
			return false;
		}
		case ConditionType.IsEquipped:
		{
			Character subject141 = GetSubject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype7 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject141 != null && subject141.EquippedItem != null && equipmentPrototype7 != null)
			{
				return subject141.EquippedItem.GetPrototype() == equipmentPrototype7;
			}
			return false;
		}
		case ConditionType.IsGatheringFrom:
		{
			Character subject139 = GetSubject<Character>(actor, target, obj, param);
			Prop prop = GetObject<Prop>(actor, target, obj, param);
			if (subject139 != null)
			{
				for (int num45 = 0; num45 < subject139.Roles.Count; num45++)
				{
					if (subject139.Roles[num45].Role == Role.Gatherer && GameTerrain.Instance.GetFixedObjectOnTile(subject139.Roles[num45].TargetLocation.x, subject139.Roles[num45].TargetLocation.y) as Prop == prop)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsGatheringItemType:
		{
			Character subject137 = GetSubject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype6 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject137 != null)
			{
				for (int n = 0; n < subject137.Roles.Count; n++)
				{
					if (subject137.Roles[n].Role == Role.Gatherer && subject137.Roles[n].ResourceType == equipmentPrototype6)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsAnyoneFarming:
		{
			Community subjectCommunity43 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity43 != null)
			{
				foreach (Character member7 in subjectCommunity43.Members)
				{
					if (member7.AliveAndNotZombie && member7.HasRole(Role.Farmer))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.HasPersonality:
			return GetSubject<Character>(actor, target, obj, param)?.HasPersonality(StringData) ?? false;
		case ConditionType.HasUndiscoveredPersonality:
		{
			Character subject134 = GetSubject<Character>(actor, target, obj, param);
			if (subject134 != null && subject134.HasPersonality(StringData))
			{
				return !subject134.IsPersonalityKnown(StringData);
			}
			return false;
		}
		case ConditionType.IsPersonalityKnownToPlayer:
			return GetSubject<Character>(actor, target, obj, param)?.IsPersonalityKnown(StringData) ?? false;
		case ConditionType.HasRelationship:
		{
			Character subject129 = GetSubject<Character>(actor, target, obj, param);
			Character character31 = GetObject<Character>(actor, target, obj, param);
			int num42 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (num42 == -1)
			{
				num42 = 0;
			}
			if (subject129 != null && character31 != null)
			{
				return Relationship.HasRelationship(subject129, (RelationshipType)num42, character31);
			}
			return false;
		}
		case ConditionType.HasRelationshipWithAnyone:
		{
			Character subject126 = GetSubject<Character>(actor, target, obj, param);
			int num41 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (subject126 != null && num41 != -1)
			{
				return Relationship.HasRelationshipWithAnyone(subject126, (RelationshipType)num41, BoolData);
			}
			return false;
		}
		case ConditionType.HasRomanticRelationship:
		{
			Character subject124 = GetSubject<Character>(actor, target, obj, param);
			Character character30 = GetObject<Character>(actor, target, obj, param);
			if (subject124 != null && character30 != null)
			{
				return Relationship.HasRomanticRelationship(subject124, character30);
			}
			return false;
		}
		case ConditionType.HasRomanticRelationshipWithAnyone:
		{
			Character subject121 = GetSubject<Character>(actor, target, obj, param);
			if (subject121 != null)
			{
				return Relationship.HasRomanticRelationshipWithAnyone(subject121, BoolData);
			}
			return false;
		}
		case ConditionType.HasGoodRelationship:
		{
			Character subject119 = GetSubject<Character>(actor, target, obj, param);
			Character character29 = GetObject<Character>(actor, target, obj, param);
			if (subject119 != null && character29 != null)
			{
				return subject119.HasGoodRelationship(character29);
			}
			return false;
		}
		case ConditionType.HasGoodRelationshipsInCommunity:
		{
			Character subject114 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity14 = GetObjectCommunity(actor, target, obj, param);
			if (subject114 != null && objectCommunity14 != null)
			{
				return (float)subject114.GetNumGoodRelationshipsInCommunity(BoolData, objectCommunity14) >= Data;
			}
			return false;
		}
		case ConditionType.PartnerHasRelationship:
		{
			Character subject106 = GetSubject<Character>(actor, target, obj, param);
			Character character24 = ((subject106 != null) ? Relationship.GetPartner(subject106) : null);
			Character character25 = GetObject<Character>(actor, target, obj, param);
			int num33 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (num33 == -1)
			{
				num33 = 0;
			}
			if (character24 != null && character25 != null)
			{
				return Relationship.HasRelationship(character24, (RelationshipType)num33, character25);
			}
			return false;
		}
		case ConditionType.PartnerHasRelationshipKnownToPlayer:
		{
			Character subject100 = GetSubject<Character>(actor, target, obj, param);
			Character character22 = ((subject100 != null) ? Relationship.GetPartner(subject100) : null);
			Character character23 = GetObject<Character>(actor, target, obj, param);
			int num31 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (character22 != null && character23 != null && num31 != -1 && Relationship.HasRelationship(character22, (RelationshipType)num31, character23))
			{
				return Relationship.IsKnownToPlayer(character22, character23);
			}
			return false;
		}
		case ConditionType.CanFlirt:
		{
			Character subject90 = GetSubject<Character>(actor, target, obj, param);
			Character character20 = GetObject<Character>(actor, target, obj, param);
			if (subject90 == null || character20 == null || subject90.Community == null || character20.Community == null)
			{
				return false;
			}
			if (!BoolData && subject90.Community != character20.Community)
			{
				return false;
			}
			if (!subject90.IsAttractedTo(character20.Appearance.Gender))
			{
				return false;
			}
			if (!character20.IsAttractedTo(subject90.Appearance.Gender))
			{
				return false;
			}
			if (!subject90.IsInDatingAgeRange(character20))
			{
				return false;
			}
			if (subject90.HasMemory(MemoryPrototype.Dumped, character20, subject90))
			{
				return false;
			}
			switch (Relationship.GetRelationship(subject90, character20))
			{
			case RelationshipType.InLoveWith:
				return true;
			default:
				return false;
			case RelationshipType.None:
			{
				int personality = subject90.GetPersonality(CachedPersonalityType.Loyal, CachedPersonalityType.Fickle);
				int num23 = 0;
				for (int j = 0; j < subject90.Relationships.Count; j++)
				{
					if (subject90.Relationships[j].RelationshipTarget == character20)
					{
						continue;
					}
					RelationshipType relationshipType = subject90.Relationships[j].RelationshipType;
					if ((uint)(relationshipType - 5) > 2u || !subject90.Relationships[j].RelationshipTarget.AliveAndNotZombie)
					{
						continue;
					}
					float num24 = (float)personality * 50f;
					switch (subject90.Relationships[j].RelationshipType)
					{
					case RelationshipType.MarriedTo:
						num24 -= 25f;
						break;
					case RelationshipType.InLoveWith:
						if (Relationship.GetRelationship(subject90.Relationships[j].RelationshipTarget, subject90) == RelationshipType.NotInLoveWith)
						{
							continue;
						}
						num24 += 25f;
						break;
					}
					subject90.CalcApprovalRating(subject90.Relationships[j].RelationshipTarget, out var approval7, out var respect7);
					if (Character.ApprovalExceeds(approval7, respect7, num24, 100f))
					{
						return false;
					}
					num23++;
				}
				if (num23 >= Relationship.GetMaxAffairCount(personality) && !character20.IsPlayerAvatar())
				{
					return false;
				}
				return true;
			}
			}
		}
		case ConditionType.IsRelationshipKnownToPlayer:
		{
			Character subject85 = GetSubject<Character>(actor, target, obj, param);
			Character character19 = GetObject<Character>(actor, target, obj, param);
			int num21 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (subject85 != null && character19 != null && num21 != -1)
			{
				return Relationship.IsKnownToPlayer(subject85, (RelationshipType)num21, character19);
			}
			return false;
		}
		case ConditionType.HasSkill:
		{
			Character subject81 = GetSubject<Character>(actor, target, obj, param);
			int num17 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject81 != null && num17 != -1)
			{
				return (float)subject81.GetSkillLevelWithEffects((SkillType)num17) >= Data;
			}
			return false;
		}
		case ConditionType.IsSkillBetween:
		{
			Character subject77 = GetSubject<Character>(actor, target, obj, param);
			int num16 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject77 != null && num16 != -1)
			{
				int skillLevelWithEffects = subject77.GetSkillLevelWithEffects((SkillType)num16);
				if ((float)skillLevelWithEffects >= Data)
				{
					return (float)skillLevelWithEffects < Data2;
				}
				return false;
			}
			return false;
		}
		case ConditionType.CommunityHasAnyoneWithSkill:
		{
			Community subjectCommunity16 = GetSubjectCommunity(actor, target, obj, param);
			int num14 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subjectCommunity16 != null && num14 != -1)
			{
				return subjectCommunity16.HasAnyoneWithSkill((SkillType)num14, (int)Data);
			}
			return false;
		}
		case ConditionType.SkillEquals:
		{
			Character subject70 = GetSubject<Character>(actor, target, obj, param);
			int num13 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject70 != null && num13 != -1)
			{
				return (float)subject70.GetSkillLevelWithEffects((SkillType)num13) == Data;
			}
			return false;
		}
		case ConditionType.IsSkillKnownByPlayer:
		{
			Character subject65 = GetSubject<Character>(actor, target, obj, param);
			int num12 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject65 != null && num12 != -1)
			{
				return subject65.Skillset.IsSkillKnown((SkillType)num12);
			}
			return false;
		}
		case ConditionType.TraderLevelEquals:
		{
			Character subject64 = GetSubject<Character>(actor, target, obj, param);
			if (subject64 != null)
			{
				return (float)subject64.TraderLevel == Data;
			}
			return false;
		}
		case ConditionType.HasBloodLoss:
		{
			Character subject63 = GetSubject<Character>(actor, target, obj, param);
			if (subject63 != null)
			{
				return subject63.GetBloodLoss() >= Data;
			}
			return false;
		}
		case ConditionType.HasDamageFraction:
		{
			TileObject subject58 = GetSubject<TileObject>(actor, target, obj, param);
			if (subject58 != null)
			{
				return subject58.GetDamageFraction() >= Data;
			}
			return false;
		}
		case ConditionType.HasFatness:
		{
			Human subject53 = GetSubject<Human>(actor, target, obj, param);
			if (subject53 != null)
			{
				return subject53.GetAppearance().Bones.Fatness >= Data;
			}
			return false;
		}
		case ConditionType.HasFatigue:
		{
			Character subject51 = GetSubject<Character>(actor, target, obj, param);
			if (subject51 != null)
			{
				return subject51.GetFatigue() >= Data;
			}
			return false;
		}
		case ConditionType.HasHunger:
		{
			Character subject47 = GetSubject<Character>(actor, target, obj, param);
			if (subject47 != null)
			{
				return subject47.GetHunger() >= Data * Sun.DayLengthSecs;
			}
			return false;
		}
		case ConditionType.HasThirst:
		{
			Character subject46 = GetSubject<Character>(actor, target, obj, param);
			if (subject46 != null)
			{
				return subject46.GetThirst() >= Data * Sun.DayLengthSecs;
			}
			return false;
		}
		case ConditionType.HasSleepDeprivation:
		{
			Character subject44 = GetSubject<Character>(actor, target, obj, param);
			if (subject44 != null)
			{
				return subject44.GetSleepDeprivation() >= Data * Sun.DayLengthSecs;
			}
			return false;
		}
		case ConditionType.HasHypothermia:
		{
			Character subject41 = GetSubject<Character>(actor, target, obj, param);
			if (subject41 != null)
			{
				return subject41.GetBodyTemperatureInCelsius() <= Data;
			}
			return false;
		}
		case ConditionType.HasInfectionProgression:
		{
			Character subject36 = GetSubject<Character>(actor, target, obj, param);
			if (subject36 != null)
			{
				return subject36.GetInfectionProgression() >= Data;
			}
			return false;
		}
		case ConditionType.HasBloodAlcoholConcentration:
		{
			Character subject33 = GetSubject<Character>(actor, target, obj, param);
			if (subject33 != null)
			{
				return subject33.GetBloodAlcoholConcentration() >= Data;
			}
			return false;
		}
		case ConditionType.HasExcitement:
		{
			Character subject32 = GetSubject<Character>(actor, target, obj, param);
			if (subject32 != null)
			{
				return subject32.Excitement * 100f >= Data;
			}
			return false;
		}
		case ConditionType.IsPregnant:
			return GetSubject<Human>(actor, target, obj, param)?.Pregnant ?? false;
		case ConditionType.PregnancyProgressionIsAtLeast:
		{
			Human subject27 = GetSubject<Human>(actor, target, obj, param);
			if (subject27 != null)
			{
				return subject27.PregnancyProgression * (float)Weather.DaysInAMonth * Human.PregnancyMonths >= Data;
			}
			return false;
		}
		case ConditionType.MoraleIsAtLeast:
		{
			Character subject25 = GetSubject<Character>(actor, target, obj, param);
			if (subject25 != null)
			{
				return subject25.CalcMorale() >= Data;
			}
			return false;
		}
		case ConditionType.MoraleIsLessThan:
		{
			Character subject22 = GetSubject<Character>(actor, target, obj, param);
			if (subject22 != null)
			{
				return subject22.CalcMorale() < Data;
			}
			return false;
		}
		case ConditionType.PresenceIsAtLeast:
			return Session.Instance.CommunityManager.GetCommunityAggro() >= Data;
		case ConditionType.PresenceIsLessThan:
			return Session.Instance.CommunityManager.GetCommunityAggro() < Data;
		case ConditionType.Approves:
		{
			BaseObject subject18 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject5 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject18 != null && baseObject5 != null)
			{
				subject18.CalcApprovalRatingForCharacterOrCommunity(baseObject5, out var approval6, out var respect6);
				return Character.ApprovalExceeds(approval6, respect6, Data, Data2);
			}
			return false;
		}
		case ConditionType.ApprovedBeforeThisMemory:
		{
			BaseObject subject15 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject4 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject15 != null && baseObject4 != null)
			{
				subject15.CalcApprovalRatingForCharacterOrCommunity(baseObject4, out var approval5, out var respect5, Target.Never, param.GetMemory().Time);
				return Character.ApprovalExceeds(approval5, respect5, Data, Data2);
			}
			return false;
		}
		case ConditionType.ApprovalSince:
		{
			BaseObject subject11 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject3 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject11 != null && baseObject3 != null)
			{
				TimeSpan afterTime2 = Session.Instance.PlayTime - TimeSpan.FromSeconds(Data2 * Sun.DayLengthSecs);
				subject11.CalcApprovalRatingForCharacterOrCommunity(baseObject3, out var approval4, out var _, afterTime2, TimeSpan.MaxValue);
				return approval4 >= Data;
			}
			return false;
		}
		case ConditionType.RespectSince:
		{
			BaseObject subject9 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject2 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject9 != null && baseObject2 != null)
			{
				TimeSpan afterTime = Session.Instance.PlayTime - TimeSpan.FromSeconds(Data2 * Sun.DayLengthSecs);
				subject9.CalcApprovalRatingForCharacterOrCommunity(baseObject2, out var _, out var respect3, afterTime, TimeSpan.MaxValue);
				return respect3 >= Data;
			}
			return false;
		}
		case ConditionType.ApprovalSinceLastFight:
		{
			Character subject7 = GetSubject<Character>(actor, target, obj, param);
			Character character2 = GetObject<Character>(actor, target, obj, param);
			if (subject7 != null && character2 != null)
			{
				subject7.CalcApprovalRatingSinceLastFight(character2, out var approval2, out var respect2);
				return Character.ApprovalExceeds(approval2, respect2, Data, Data2);
			}
			return false;
		}
		case ConditionType.Respects:
		{
			BaseObject subject = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject = GetObject<BaseObject>(actor, target, obj, param);
			if (subject != null && baseObject != null)
			{
				subject.CalcApprovalRatingForCharacterOrCommunity(baseObject, out var approval, out var respect);
				return Character.RespectExceeds(approval, respect, Data, Data2);
			}
			return false;
		}
		case ConditionType.ApprovalRespectAngleBetween:
		{
			BaseObject subject219 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject14 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject219 != null && baseObject14 != null)
			{
				subject219.CalcApprovalRatingForCharacterOrCommunity(baseObject14, out var approval12, out var respect12);
				float num66 = 57.29578f * MathUtil.ClampAngleBetween0AndTwoPi((float)Math.Atan2(approval12, respect12));
				if (num66 >= Data)
				{
					return num66 < Data2;
				}
				return false;
			}
			return false;
		}
		case ConditionType.ApprovalRespectAmountBetween:
		{
			BaseObject subject216 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject13 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject216 != null && baseObject13 != null)
			{
				subject216.CalcApprovalRatingForCharacterOrCommunity(baseObject13, out var approval11, out var respect11);
				float magnitude = new Vector2(approval11, respect11).magnitude;
				if (magnitude >= Data)
				{
					return magnitude < Data2;
				}
				return false;
			}
			return false;
		}
		case ConditionType.ApprovalRespectAmountAtLeast:
		{
			BaseObject subject214 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject12 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject214 != null && baseObject12 != null)
			{
				subject214.CalcApprovalRatingForCharacterOrCommunity(baseObject12, out var approval10, out var respect10);
				return new Vector2(approval10, respect10).magnitude >= Data;
			}
			return false;
		}
		case ConditionType.ApprovalRespectAmountLessThan:
		{
			BaseObject subject212 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject11 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject212 != null && baseObject11 != null)
			{
				subject212.CalcApprovalRatingForCharacterOrCommunity(baseObject11, out var approval9, out var respect9);
				return new Vector2(approval9, respect9).magnitude < Data;
			}
			return false;
		}
		case ConditionType.ApprovalRespectAbsoluteSum:
		{
			BaseObject subject211 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject10 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject211 != null && baseObject10 != null)
			{
				subject211.CalcApprovalRatingForCharacterOrCommunity(baseObject10, out var approval8, out var respect8);
				return approval8 + respect8 >= Data;
			}
			return false;
		}
		case ConditionType.HasExceededLimitForMemoryType:
		{
			Character subject203 = GetSubject<Character>(actor, target, obj, param);
			BaseObject obj2 = GetObject<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype9 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject203 != null && memoryPrototype9 != null && (memoryPrototype9.ApprovalLimit != 0f || memoryPrototype9.RespectLimit != 0f))
			{
				int num57 = actor.FindMemory(memoryPrototype9, subject203, obj2);
				if (num57 != -1)
				{
					if (memoryPrototype9.ApprovalLimit == 0f || Math.Abs(actor.Memories[num57].ApprovalContribution) > memoryPrototype9.ApprovalLimit)
					{
						if (memoryPrototype9.RespectLimit != 0f)
						{
							return Math.Abs(actor.Memories[num57].RespectContribution) > memoryPrototype9.RespectLimit;
						}
						return true;
					}
					return false;
				}
			}
			return false;
		}
		case ConditionType.HasMemory:
		{
			Character subject198 = GetSubject<Character>(actor, target, obj, param);
			Character character48 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty13 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype8 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject198 != null && character48 != null && memoryPrototype8 != null)
			{
				return subject198.HasMemory(memoryPrototype8, character48, thirdParty13);
			}
			return false;
		}
		case ConditionType.HasRecentMemory:
		{
			Character subject194 = GetSubject<Character>(actor, target, obj, param);
			Character character47 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty10 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype7 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject194 != null && character47 != null && memoryPrototype7 != null)
			{
				return subject194.HasMemoryAfter(memoryPrototype7, character47, thirdParty10, Session.Instance.PlayTime - TimeSpan.FromSeconds(Sun.DayLengthSecs * Data));
			}
			return false;
		}
		case ConditionType.HasRecentMemoryOfAnyoneInCommunity:
		{
			Character subject189 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity22 = GetObjectCommunity(actor, target, obj, param);
			BaseObject thirdParty8 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype6 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject189 != null && objectCommunity22 != null && memoryPrototype6 != null)
			{
				return subject189.HasMemoryOfAnyoneInCommunityAfter(memoryPrototype6, objectCommunity22, thirdParty8, Session.Instance.PlayTime - TimeSpan.FromSeconds(Sun.DayLengthSecs * Data));
			}
			return false;
		}
		case ConditionType.HasRecentMemoryWhereObjectIsAnyoneInCommunity:
		{
			Character subject185 = GetSubject<Character>(actor, target, obj, param);
			Character character44 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty7 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype5 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject185 != null && character44 != null && memoryPrototype5 != null)
			{
				return subject185.HasMemoryWhereObjectIsAnyoneInCommunityAfter(memoryPrototype5, character44, thirdParty7, Session.Instance.PlayTime - TimeSpan.FromSeconds(Sun.DayLengthSecs * Data));
			}
			return false;
		}
		case ConditionType.HasMemoryQuantityBetween:
		{
			Character subject177 = GetSubject<Character>(actor, target, obj, param);
			Character character42 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty4 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype4 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject177 != null && character42 != null && thirdParty4 != null && memoryPrototype4 != null)
			{
				float memoryQuantity = subject177.GetMemoryQuantity(memoryPrototype4, character42, thirdParty4);
				if (memoryQuantity >= Data)
				{
					return memoryQuantity < Data2;
				}
				return false;
			}
			return false;
		}
		case ConditionType.HasMemoryQuantityAtLeast:
		{
			Character subject174 = GetSubject<Character>(actor, target, obj, param);
			Character character40 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty3 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype3 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject174 != null && character40 != null && thirdParty3 != null && memoryPrototype3 != null)
			{
				return subject174.GetMemoryQuantity(memoryPrototype3, character40, thirdParty3) >= Data;
			}
			return false;
		}
		case ConditionType.HasMemoryQuantityLessThan:
		{
			Character subject171 = GetSubject<Character>(actor, target, obj, param);
			Character character39 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype2 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject171 != null && character39 != null && thirdParty != null && memoryPrototype2 != null)
			{
				return subject171.GetMemoryQuantity(memoryPrototype2, character39, thirdParty) < Data;
			}
			return false;
		}
		case ConditionType.HasAnyMemoryOlderThan:
		{
			Character subject169 = GetSubject<Character>(actor, target, obj, param);
			BaseObject baseObject8 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject169 != null && baseObject8 != null)
			{
				return Session.Instance.PlayTime - subject169.GetTimeOfOldestMemory(baseObject8) >= TimeSpan.FromSeconds(Sun.DayLengthSecs * Data);
			}
			return false;
		}
		case ConditionType.HasAnyFakeMemories:
			return GetSubject<Character>(actor, target, obj, param)?.HasAnyFakeMemories() ?? false;
		case ConditionType.HasPartner:
		{
			Character subject167 = GetSubject<Character>(actor, target, obj, param);
			if (subject167 != null)
			{
				return Relationship.GetPartner(subject167) != null;
			}
			return false;
		}
		case ConditionType.HasPartnerKnownToPlayer:
		{
			Character subject165 = GetSubject<Character>(actor, target, obj, param);
			if (subject165 != null)
			{
				return Relationship.IsPartnerKnownToPlayer(subject165);
			}
			return false;
		}
		case ConditionType.MemoryType:
		{
			MemoryPrototype memoryPrototype = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (param.GetParamType() == ParamType.Memory && memoryPrototype != null)
			{
				return param.GetMemory().Prototype == memoryPrototype;
			}
			return false;
		}
		case ConditionType.MemoryActorIsMe:
		{
			Character subject161 = GetSubject<Character>(actor, target, obj, param);
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().Actor == subject161;
			}
			return false;
		}
		case ConditionType.MemoryActorIsUnknown:
		{
			Character subject157 = GetSubject<Character>(actor, target, obj, param);
			if (param.GetParamType() == ParamType.Memory)
			{
				if (param.GetMemory().Actor != null)
				{
					if (!param.GetMemory().Actor.NameKnown && subject157 != null)
					{
						return subject157.IsControllableByPlayer();
					}
					return false;
				}
				return true;
			}
			return false;
		}
		case ConditionType.MemoryObjectIsMe:
		{
			Character subject154 = GetSubject<Character>(actor, target, obj, param);
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().Object == subject154;
			}
			return false;
		}
		case ConditionType.MemoryObjectIsACommunity:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().Object is Community;
			}
			return false;
		case ConditionType.MemoryApprovalBetween:
			if (param.GetParamType() == ParamType.Memory && param.GetMemory().ApprovalContribution >= Data)
			{
				return param.GetMemory().ApprovalContribution < Data2;
			}
			return false;
		case ConditionType.MemoryApprovalAtLeast:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().ApprovalContribution >= Data;
			}
			return false;
		case ConditionType.MemoryApprovalLessThan:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().ApprovalContribution < Data;
			}
			return false;
		case ConditionType.MemoryOtherApprovalAtLeast:
			if (param.GetParamType() == ParamType.Memory)
			{
				Character subject145 = GetSubject<Character>(actor, target, obj, param);
				if (subject145 != null)
				{
					Memory memory2 = param.GetMemory();
					int num48 = subject145.FindMemory(memory2.Prototype, memory2.Actor, memory2.Object);
					if (num48 != -1)
					{
						return subject145.Memories[num48].ApprovalContribution >= Data;
					}
					return false;
				}
			}
			return false;
		case ConditionType.MemoryOtherApprovalLessThan:
			if (param.GetParamType() == ParamType.Memory)
			{
				Character subject142 = GetSubject<Character>(actor, target, obj, param);
				if (subject142 != null)
				{
					Memory memory = param.GetMemory();
					int num47 = subject142.FindMemory(memory.Prototype, memory.Actor, memory.Object);
					if (num47 != -1)
					{
						return subject142.Memories[num47].ApprovalContribution < Data;
					}
					return false;
				}
			}
			return false;
		case ConditionType.MemoryRespectBetween:
			if (param.GetParamType() == ParamType.Memory && param.GetMemory().RespectContribution >= Data)
			{
				return param.GetMemory().RespectContribution < Data2;
			}
			return false;
		case ConditionType.MemoryRespectAtLeast:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().RespectContribution >= Data;
			}
			return false;
		case ConditionType.MemoryRespectLessThan:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().RespectContribution < Data;
			}
			return false;
		case ConditionType.MemoryMoraleBetween:
			if (param.GetParamType() == ParamType.Memory && param.GetMemory().MoraleContribution >= Data)
			{
				return param.GetMemory().MoraleContribution < Data2;
			}
			return false;
		case ConditionType.MemoryMoraleAtLeast:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().MoraleContribution >= Data;
			}
			return false;
		case ConditionType.MemoryMoraleLessThan:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().MoraleContribution < Data;
			}
			return false;
		case ConditionType.MemoryQuantityFactorBetween:
			if (param.GetParamType() == ParamType.Memory && param.GetMemory().QuantityFactor >= Data)
			{
				return param.GetMemory().QuantityFactor < Data2;
			}
			return false;
		case ConditionType.MemoryQuantityFactorAtLeast:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().QuantityFactor >= Data;
			}
			return false;
		case ConditionType.MemoryQuantityFactorLessThan:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().QuantityFactor < Data;
			}
			return false;
		case ConditionType.MemoryIsRecent:
			if (param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().Time >= Session.Instance.PlayTime - TimeSpan.FromSeconds(Data * Sun.DayLengthSecs);
			}
			return false;
		case ConditionType.MemoryIsBeforeJoinedCommunity:
		{
			Character subject130 = GetSubject<Character>(actor, target, obj, param);
			if (subject130 != null && param.GetParamType() == ParamType.Memory)
			{
				return param.GetMemory().Time < subject130.JoinedCommunityTime;
			}
			return false;
		}
		case ConditionType.IsEquipmentType:
		{
			Equipment subjectEquipment3 = GetSubjectEquipment(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype5 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subjectEquipment3 != null)
			{
				return subjectEquipment3.GetPrototype() == equipmentPrototype5;
			}
			ThrownProjectile subject128 = GetSubject<ThrownProjectile>(actor, target, obj, param);
			if (subject128 != null)
			{
				return subject128.Proto == equipmentPrototype5;
			}
			return false;
		}
		case ConditionType.IsLiquidType:
		{
			Equipment subjectEquipment2 = GetSubjectEquipment(actor, target, obj, param);
			LiquidPrototype liquidPrototype2 = GameImpl.Instance.FindLiquidPrototypeByName(StringData);
			if (subjectEquipment2 != null)
			{
				return subjectEquipment2.GetLiquidContentsType() == liquidPrototype2;
			}
			return false;
		}
		case ConditionType.IsPropType:
		{
			TileObject subject125 = GetSubject<TileObject>(actor, target, obj, param);
			PropPrototype propPrototype2 = GameImpl.Instance.FindPropPrototypeByName(StringData);
			if (subject125 != null)
			{
				return subject125.GetPropPrototype() == propPrototype2;
			}
			return false;
		}
		case ConditionType.IsAccomodationBuilding:
			return GetSubject<TileObject>(actor, target, obj, param)?.IsAccommodation() ?? false;
		case ConditionType.TastinessIsAtLeast:
		{
			Character subject120 = GetSubject<Character>(actor, target, obj, param);
			Equipment objectEquipment6 = GetObjectEquipment(actor, target, obj, param);
			if (subject120 != null && objectEquipment6 != null)
			{
				if (objectEquipment6.GetLiquidContentsType() != null)
				{
					return subject120.CalcTastiness(objectEquipment6.GetLiquidContentsType()) >= Data;
				}
				return subject120.CalcTastiness(objectEquipment6.GetPrototype()) >= Data;
			}
			if (param.IsInt())
			{
				return (float)param.GetInt() >= Data;
			}
			return false;
		}
		case ConditionType.IsInFisticuffs:
		{
			Character subject118 = GetSubject<Character>(actor, target, obj, param);
			if (subject118 != null)
			{
				return subject118.SparringPartner != null;
			}
			return false;
		}
		case ConditionType.IsWearingSomething:
		{
			Character subject115 = GetSubject<Character>(actor, target, obj, param);
			int num38 = Array.IndexOf(Character.ClothingTypeNames, StringData);
			if (subject115 != null && num38 != -1)
			{
				return subject115.Clothes[num38] != null;
			}
			return false;
		}
		case ConditionType.IsWearingHelmet:
		{
			Character subject112 = GetSubject<Character>(actor, target, obj, param);
			if (subject112 != null)
			{
				return subject112.Clothes[0] is Armor;
			}
			return false;
		}
		case ConditionType.IsWearingAnythingWithSkillBonus:
		{
			Character subject109 = GetSubject<Character>(actor, target, obj, param);
			int num35 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject109 != null && num35 != -1)
			{
				for (int l = 0; l < 9; l++)
				{
					if (subject109.Clothes[l] != null && subject109.Clothes[l].GetPrototype().SkillBonusType == (SkillType)num35 && subject109.Clothes[l].GetPrototype().SkillBonus > 0)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsClothingType:
		{
			Equipment subjectEquipment = GetSubjectEquipment(actor, target, obj, param);
			int num34 = Array.IndexOf(Character.ClothingTypeNames, StringData);
			if (subjectEquipment != null && num34 != -1)
			{
				return subjectEquipment.GetClothingType() == (ClothingType)num34;
			}
			return false;
		}
		case ConditionType.IsWarmerThatCurrentClothing:
		{
			Character subject102 = GetSubject<Character>(actor, target, obj, param);
			Equipment objectEquipment5 = GetObjectEquipment(actor, target, obj, param);
			if (subject102 != null && objectEquipment5 != null && objectEquipment5.GetClothingType() != ClothingType.Invalid)
			{
				Equipment equipment = subject102.Clothes[(int)objectEquipment5.GetClothingType()];
				return objectEquipment5.GetInsulation() > (equipment?.GetInsulation() ?? 0);
			}
			return false;
		}
		case ConditionType.IsCarrying:
		{
			Character subject98 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject3 = GetObject<TileObject>(actor, target, obj, param);
			if (subject98 != null && tileObject3 != null)
			{
				return subject98.CarryingObject == tileObject3;
			}
			Community subjectCommunity26 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity26 != null)
			{
				foreach (Character member8 in subjectCommunity26.Members)
				{
					if (member8.CarryingObject == tileObject3)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsCarryingCommunityMember:
		{
			Character subject93 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity8 = GetObjectCommunity(actor, target, obj, param);
			int num27 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (subject93 != null && objectCommunity8 != null)
			{
				if (subject93.CarryingObject != null && subject93.CarryingObject.GetCommunity() == objectCommunity8)
				{
					if (num27 != -1)
					{
						return subject93.CarryingObject.GetBaseObjectType() == (BaseObjectType)num27;
					}
					return true;
				}
				return false;
			}
			Community subjectCommunity24 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity24 != null)
			{
				foreach (Character member9 in subjectCommunity24.Members)
				{
					if (member9.CarryingObject != null && member9.CarryingObject.GetCommunity() == objectCommunity8 && (num27 == -1 || member9.CarryingObject.GetBaseObjectType() == (BaseObjectType)num27))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsCarryingCorpse:
		{
			Character subject84 = GetSubject<Character>(actor, target, obj, param);
			Character character17 = ((subject84 != null) ? (subject84.CarryingObject as Character) : null);
			int num20 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			if (character17 != null)
			{
				if (!character17.Alive)
				{
					if (num20 != -1)
					{
						return character17.GetBaseObjectType() == (BaseObjectType)num20;
					}
					return true;
				}
				return false;
			}
			Community subjectCommunity19 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity19 != null)
			{
				foreach (Character member10 in subjectCommunity19.Members)
				{
					if (member10.CarryingObject is Character { Alive: false } character18 && (num20 == -1 || character18.GetBaseObjectType() == (BaseObjectType)num20))
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsTimeToShakedownAgain:
		{
			Character subject80 = GetSubject<Character>(actor, target, obj, param);
			Character character14 = GetObject<Character>(actor, target, obj, param);
			if (subject80 != null && character14 != null && subject80.GetCommunity() != null)
			{
				return subject80.GetCommunity().IsTimeToShakedownAgain(subject80, character14);
			}
			return false;
		}
		case ConditionType.CauseOfDeath:
		{
			Character subject79 = GetSubject<Character>(actor, target, obj, param);
			if (subject79 != null)
			{
				return subject79.CauseOfDeath == CauseOfDeath.Zombie;
			}
			return false;
		}
		case ConditionType.IsTimerRunning:
			return GetSubject<Character>(actor, target, obj, param)?.IsTimerRunning(StringData) ?? false;
		case ConditionType.IsTimerAtLeast:
			return GetSubject<Character>(actor, target, obj, param)?.IsTimerAtLeast(StringData, Data) ?? false;
		case ConditionType.HasBeenWarnedAboutTraps:
		{
			Community subjectCommunity15 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity15 != null && subjectCommunity15.WarnedAboutTraps)
			{
				return subjectCommunity15.WarnedAboutTripwires;
			}
			return false;
		}
		case ConditionType.HasCustomName:
		{
			Prop subject72 = GetSubject<Prop>(actor, target, obj, param);
			if (subject72 != null)
			{
				return !string.IsNullOrEmpty(subject72.CustomName);
			}
			return false;
		}
		case ConditionType.IsInvisibleStrainEnabled:
			return Session.Instance.DifficultySettings.InvisibleStrainPercentage > 0f;
		case ConditionType.AreSaveTokensEnabled:
			return Session.Instance.DifficultySettings.SaveTokensRequired;
		case ConditionType.ShouldTradersHaveSaveTokens:
			return Session.Instance.DifficultySettings.TradersHaveSaveTokens;
		case ConditionType.IsHitTheRoadCountAtLeast:
			return (float)Session.Instance.HitTheRoadCount >= Data;
		case ConditionType.IsTemperatureLessThan:
			return Session.Instance.Weather.TemperatureInCelsius < Data;
		case ConditionType.IsTemperatureAtLeast:
			return Session.Instance.Weather.TemperatureInCelsius >= Data;
		case ConditionType.IsSnowCoverLessThan:
			return Session.Instance.Weather.SnowOnGroundAmount < Data;
		case ConditionType.IsSnowCoverAtLeast:
			return Session.Instance.Weather.SnowOnGroundAmount >= Data;
		case ConditionType.IsDay:
			return Sun.GetSunIntensity(Session.Instance.DaysSinceStart) > 0.5f;
		case ConditionType.IsNight:
			return Sun.GetSunIntensity(Session.Instance.DaysSinceStart) <= 0.5f;
		case ConditionType.IsSnowing:
			if (Session.Instance.Weather.GetPrecipitationAmount() > Data)
			{
				return Session.Instance.Weather.TemperatureInCelsius < 2f;
			}
			return false;
		case ConditionType.IsRaining:
			if (Session.Instance.Weather.GetPrecipitationAmount() > Data)
			{
				return Session.Instance.Weather.TemperatureInCelsius >= 2f;
			}
			return false;
		case ConditionType.IsBuildingOfTypeUnlocked:
		{
			Community subjectCommunity14 = GetSubjectCommunity(actor, target, obj, param);
			PropPrototype propPrototype = GameImpl.Instance.FindPropPrototypeByName(StringData);
			if (subjectCommunity14 != null && propPrototype != null)
			{
				foreach (Prop building6 in subjectCommunity14.Buildings)
				{
					if (building6.Prototype == propPrototype && building6 is Building { UnlockedToPlayer: not false })
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.HasCommunityGrantedMiningRightsToPlayer:
			return GetSubjectCommunity(actor, target, obj, param)?.GrantedMiningRightsToPlayer ?? false;
		case ConditionType.CanOpenPlayerGatesEquals:
		{
			Character subject60 = GetSubject<Character>(actor, target, obj, param);
			int num9 = Array.IndexOf(Character.CanOpenGatesNames, StringData);
			if (subject60 != null && num9 != -1)
			{
				return subject60.CanOpenPlayerGates == (CanOpenGates)num9;
			}
			return false;
		}
		case ConditionType.HasDeerOnMap:
			return Session.Instance.CommunityManager.DeerSpawnPoints.Count > 0;
		case ConditionType.HasDiscoveredDeer:
			return Session.Instance.CommunityManager.HasDiscoveredDeer();
		case ConditionType.IsInRangeOfDiscoveredDeerSpawnpoint:
		{
			Character subject59 = GetSubject<Character>(actor, target, obj, param);
			return Session.Instance.CommunityManager.IsInRangeOfDiscoveredDeerSpawnpoint(subject59.Tile, Data);
		}
		case ConditionType.IsInvaderWithSource:
		{
			BaseObject subject55 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject6 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject55 != null)
			{
				InvaderInstance invaderInstanceThatCreatedHunter2 = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(subject55);
				if (invaderInstanceThatCreatedHunter2 != null && invaderInstanceThatCreatedHunter2.SourceObject == baseObject6)
				{
					return invaderInstanceThatCreatedHunter2.Invader.UniqueID == StringData;
				}
				return false;
			}
			return false;
		}
		case ConditionType.IsInvader:
		{
			BaseObject subject50 = GetSubject<BaseObject>(actor, target, obj, param);
			if (subject50 != null)
			{
				InvaderInstance invaderInstanceThatCreatedHunter = StoryManager.Instance.GetInvaderInstanceThatCreatedHunter(subject50);
				if (invaderInstanceThatCreatedHunter != null)
				{
					return invaderInstanceThatCreatedHunter.Invader.UniqueID == StringData;
				}
				return false;
			}
			return false;
		}
		case ConditionType.IsInvaderFinished:
		{
			BaseObject subject45 = GetSubject<BaseObject>(actor, target, obj, param);
			Invader invader = GameImpl.Instance.FindInvaderByUniqueID(StringData);
			if (invader != null)
			{
				InvaderInstance invaderInstance = StoryManager.Instance.GetInvaderInstance(invader, subject45);
				if (invaderInstance != null)
				{
					if (invaderInstance.Active)
					{
						return false;
					}
					foreach (BaseObject createdObject in invaderInstance.CreatedObjects)
					{
						if (createdObject is Community community)
						{
							if (community.HasAnyActiveMembers())
							{
								return false;
							}
							continue;
						}
						return false;
					}
				}
				return true;
			}
			return false;
		}
		case ConditionType.IsOutnumberedBy:
		{
			Character subject42 = GetSubject<Character>(actor, target, obj, param);
			Character character10 = GetObject<Character>(actor, target, obj, param);
			if (subject42 != null && character10 != null)
			{
				return subject42.IsOutnumberedBy(character10.Community, orEqual: false, character10);
			}
			return false;
		}
		case ConditionType.CanSeeNumberOfCommunityMembers:
		{
			Character subject38 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity5 = GetObjectCommunity(actor, target, obj, param);
			if (subject38 != null && objectCommunity5 != null)
			{
				int num6 = 0;
				foreach (Target target7 in subject38.Targets)
				{
					if (target7.FullyTracked && target7.Object is Human human && human.Community == objectCommunity5)
					{
						num6++;
						if ((float)num6 >= Data)
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		case ConditionType.VisibleCommunityMembersHaveGold:
		{
			Character subject35 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity4 = GetObjectCommunity(actor, target, obj, param);
			if (subject35 != null && objectCommunity4 != null)
			{
				float nearbyDist = StoryEvent.NearbyDist;
				int num5 = 0;
				foreach (Prop building7 in objectCommunity4.Buildings)
				{
					if ((building7.PosXZ - subject35.PosXZ).magnitude <= nearbyDist)
					{
						num5 += building7.Inventory.GetGoldAmount();
						if ((float)num5 >= Data)
						{
							return true;
						}
					}
				}
				foreach (Character member11 in objectCommunity4.Members)
				{
					if (!member11.AliveAndNotZombie || member11 == subject35 || !((member11.PosXZ - subject35.PosXZ).magnitude <= nearbyDist))
					{
						continue;
					}
					if (member11.IsCrouching())
					{
						Target target2 = subject35.GetTarget(member11);
						if (target2 == null || !target2.FullyTracked)
						{
							continue;
						}
					}
					num5 += member11.Inventory.GetGoldAmount();
					if ((float)num5 >= Data)
					{
						return true;
					}
				}
			}
			return false;
		}
		case ConditionType.IsNemesis:
			return GetSubjectCommunity(actor, target, obj, param)?.Nemesis ?? false;
		case ConditionType.CommunityNutritionLevelIsAtLeast:
		{
			Community subjectCommunity6 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity6 != null)
			{
				return subjectCommunity6.CalcCommunityNutritionLevel() * 100f >= Data;
			}
			return false;
		}
		case ConditionType.VotedFor:
		{
			Character subject30 = GetSubject<Character>(actor, target, obj, param);
			Character character8 = GetObject<Character>(actor, target, obj, param);
			if (subject30 != null)
			{
				return subject30.VotedFor == character8;
			}
			return false;
		}
		case ConditionType.VotePlacingIsAtLeast:
		{
			Character subject28 = GetSubject<Character>(actor, target, obj, param);
			if (subject28 != null && subject28.Community != null)
			{
				int voteCountFor = subject28.Community.GetVoteCountFor(subject28);
				int num3 = 1;
				foreach (Character member12 in subject28.Community.Members)
				{
					if (member12.AliveAndNotZombie && member12.GetBaseObjectType() == BaseObjectType.Human && member12 != subject28 && subject28.Community.GetVoteCountFor(member12) > voteCountFor)
					{
						num3++;
					}
				}
				return (float)num3 <= Data;
			}
			return false;
		}
		case ConditionType.TimeSinceDeathIsAtLeast:
		{
			Character subject24 = GetSubject<Character>(actor, target, obj, param);
			if (subject24 == null)
			{
				return true;
			}
			if (!subject24.AliveAndNotZombie)
			{
				return Session.Instance.PlayTime - subject24.TimeOfDeath >= TimeSpan.FromSeconds(Data * Sun.DayLengthSecs);
			}
			return false;
		}
		case ConditionType.TimeSinceJoinedCommunityIsAtLeast:
		{
			Character subject23 = GetSubject<Character>(actor, target, obj, param);
			if (subject23 != null)
			{
				return Session.Instance.PlayTime - subject23.JoinedCommunityTime >= TimeSpan.FromSeconds(Data * Sun.DayLengthSecs);
			}
			return false;
		}
		case ConditionType.IsInDownTime:
		{
			Character subject21 = GetSubject<Character>(actor, target, obj, param);
			if (subject21 != null)
			{
				return subject21.DownTime > 0f;
			}
			return false;
		}
		case ConditionType.IsDoingSomethingTerriblyImportant:
			return GetSubject<Character>(actor, target, obj, param)?.IsDoingSomethingTerriblyImportant() ?? false;
		case ConditionType.WasRevivedInCaptivity:
			return GetSubject<Character>(actor, target, obj, param)?.WasRevivedInCaptivity(hysteresis: true) ?? false;
		case ConditionType.HasAnyUnpausedRoles:
			return GetSubject<Character>(actor, target, obj, param)?.HasAnyUnpausedRoles() ?? false;
		case ConditionType.HasAnyUrgentRoles:
			return GetSubject<Character>(actor, target, obj, param)?.HasAnyUrgentRoles() ?? false;
		case ConditionType.IsPerformingUrgentRole:
			return GetSubject<Character>(actor, target, obj, param)?.GetTopRunningRoleInfo(canShowPausedIfNoneAreUnpaused: false).Urgent ?? false;
		case ConditionType.IsDepressed:
			return GetSubject<Character>(actor, target, obj, param)?.IsTooDepressedToFollowOrders() ?? false;
		case ConditionType.HasTravelledFromAnotherMap:
			return GetSubject<Character>(actor, target, obj, param)?.HasTravelledFromAnotherMap ?? false;
		case ConditionType.IsTownVisited:
			return GetSubject<Town>(actor, target, obj, param)?.Visited ?? false;
		case ConditionType.DayOfYearIsOnOrAfter:
			return Session.Instance.DayOfYear >= Data;
		case ConditionType.DayOfYearIsBefore:
			return Session.Instance.DayOfYear < Data;
		case ConditionType.DaysSinceGameStartAtLeast:
			return Session.Instance.DaysSinceStart >= Data;
		case ConditionType.IsFEMA:
			return GetSubjectCommunity(actor, target, obj, param)?.IsFEMA ?? false;
		case ConditionType.IsFollowingAlly:
		{
			Character subject6 = GetSubject<Character>(actor, target, obj, param);
			if (subject6 != null && subject6.SquadLeader != null)
			{
				return subject6.IsAlly(subject6.SquadLeader);
			}
			return false;
		}
		case ConditionType.IsSimulatingSurvivalFactors:
		{
			Character subject2 = GetSubject<Character>(actor, target, obj, param);
			if (subject2 != null && !subject2.DontSimulateSurvivalFactorsUntilDiscovered)
			{
				return !subject2.DontSimulateSurvivalFactorsUntilJoinCommunity;
			}
			return false;
		}
		case ConditionType.IsLoneWolf:
			return Session.Instance.LoneWolf;
		case ConditionType.SurvivorCampDensityIsAtLeast:
			return Session.Instance.DifficultySettings.SurvivorCampDensity >= Data;
		case ConditionType.HasConqueredArea:
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			int num = 0;
			foreach (Community community6 in communityManager.Communities)
			{
				if (community6.CommunityType != CommunityType.Player && !community6.IsAnimalCommunity() && !community6.IsZombieCommunity())
				{
					if (community6.IsAISettlement())
					{
						num++;
					}
					if (community6.HasAnyActiveMembers())
					{
						return false;
					}
				}
			}
			return num > 0;
		}
		case ConditionType.Approval:
		case ConditionType.Respect:
		case ConditionType.ApprovalRespectSum:
		case ConditionType.Morale:
		case ConditionType.Presence:
		case ConditionType.Variable:
		case ConditionType.Skill:
		case ConditionType.SkillCap:
		case ConditionType.AlliesNearby:
		case ConditionType.CommunitySize:
		case ConditionType.Distance:
		case ConditionType.MemoryQuantityForCommunity:
		case ConditionType.InitialCommunitySize:
		case ConditionType.Gold:
		case ConditionType.ValueOfAllSupplies:
		case ConditionType.ItemCountOfType:
		case ConditionType.ConditionBlockResult:
		case ConditionType.CommunityBuildingCountOfType:
		case ConditionType.HitTheRoadCount:
		case ConditionType.PregnancyProgression:
		case ConditionType.DaysSinceGameStart:
		case ConditionType.CommunityActiveAllies:
			return true;
		default:
			return false;
		}
	}

	public float EvaluatePriority(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		float num = 1f;
		switch (Type)
		{
		case ConditionType.Approval:
		{
			BaseObject subject12 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject3 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject12 != null && baseObject3 != null)
			{
				subject12.CalcApprovalRatingForCharacterOrCommunity(baseObject3, out var approval3, out var _);
				num = approval3;
			}
			break;
		}
		case ConditionType.Respect:
		{
			BaseObject subject3 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject = GetObject<BaseObject>(actor, target, obj, param);
			if (subject3 != null && baseObject != null)
			{
				subject3.CalcApprovalRatingForCharacterOrCommunity(baseObject, out var _, out var respect);
				num = respect;
			}
			break;
		}
		case ConditionType.ApprovalRespectSum:
		{
			BaseObject subject6 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject baseObject2 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject6 != null && baseObject2 != null)
			{
				subject6.CalcApprovalRatingForCharacterOrCommunity(baseObject2, out var approval2, out var respect2);
				num = approval2 + respect2;
			}
			break;
		}
		case ConditionType.Morale:
		{
			Character subject13 = GetSubject<Character>(actor, target, obj, param);
			if (subject13 != null)
			{
				num = subject13.CalcMorale();
			}
			break;
		}
		case ConditionType.PregnancyProgression:
			num = GetSubject<Character>(actor, target, obj, param)?.GetPregnancyProgression() ?? 0f;
			break;
		case ConditionType.Presence:
			num = Session.Instance.CommunityManager.GetCommunityAggro();
			break;
		case ConditionType.HitTheRoadCount:
			num = Session.Instance.HitTheRoadCount;
			break;
		case ConditionType.Variable:
		{
			BaseObject subject9 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob = GetObject<BaseObject>(actor, target, obj, param);
			num = StoryManager.Instance.GetVariable(StringData, subject9, ob);
			break;
		}
		case ConditionType.Skill:
		{
			Character subject8 = GetSubject<Character>(actor, target, obj, param);
			int num2 = Array.IndexOf(Skillset.SkillNames, StringData);
			num = ((subject8 != null && num2 != -1) ? ((float)subject8.GetSkillLevelWithEffects((SkillType)num2)) : 0f);
			break;
		}
		case ConditionType.SkillCap:
		{
			Character subject10 = GetSubject<Character>(actor, target, obj, param);
			int num4 = Array.IndexOf(Skillset.SkillNames, StringData);
			num = ((subject10 != null && num4 != -1) ? ((float)subject10.Skillset.GetCap((SkillType)num4)) : 0f);
			break;
		}
		case ConditionType.AlliesNearby:
		{
			TileObject subject = GetSubject<TileObject>(actor, target, obj, param);
			Community community = subject?.GetCommunity();
			if (subject == null || community == null)
			{
				break;
			}
			num = community.GetConsciousNonZombieMemberCountInRange(subject.GetTile(), Data, subject);
			foreach (Community cachedAlly in community.CachedAllies)
			{
				num += (float)cachedAlly.GetConsciousNonZombieMemberCountInRange(subject.GetTile(), Data, subject);
			}
			break;
		}
		case ConditionType.CommunitySize:
		{
			Community subjectCommunity3 = GetSubjectCommunity(actor, target, obj, param);
			num = ((subjectCommunity3 != null) ? (BoolData ? subjectCommunity3.GetLivingNonZombieMemberCountIncludingAllies() : subjectCommunity3.GetLivingNonZombieMemberCount()) : 0);
			break;
		}
		case ConditionType.InitialCommunitySize:
			num = GetSubjectCommunity(actor, target, obj, param)?.InitialMemberCount ?? 0;
			break;
		case ConditionType.CommunityActiveAllies:
			num = GetSubjectCommunity(actor, target, obj, param)?.GetActiveAllyCount() ?? 0;
			break;
		case ConditionType.CommunityBuildingCountOfType:
		{
			Community subjectCommunity2 = GetSubjectCommunity(actor, target, obj, param);
			int num3 = Array.IndexOf(BaseObjectManager.BaseObjectNames, StringData);
			num = ((subjectCommunity2 == null || num3 == -1) ? 0f : ((float)subjectCommunity2.GetNumCompletedBuildingsOfType((BaseObjectType)num3)));
			break;
		}
		case ConditionType.Distance:
		{
			TileObject subject5 = GetSubject<TileObject>(actor, target, obj, param);
			TileObject tileObject = GetObject<TileObject>(actor, target, obj, param);
			num = ((subject5 != null && tileObject != null) ? (subject5.PosXZ - tileObject.PosXZ).magnitude : 0f);
			break;
		}
		case ConditionType.MemoryQuantityForCommunity:
		{
			Character subject2 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity = GetObjectCommunity(actor, target, obj, param);
			Community thirdPartyCommunity = GetThirdPartyCommunity(actor, target, obj, param);
			MemoryPrototype proto = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			num = subject2?.GetMemoryQuantityCountForCommunity(proto, objectCommunity.GetCommunity(), thirdPartyCommunity) ?? 0f;
			break;
		}
		case ConditionType.Gold:
		{
			TileObject subject11 = GetSubject<TileObject>(actor, target, obj, param);
			if (subject11 != null && subject11.GetInventory() != null)
			{
				num = subject11.GetInventory().GetGoldAmount();
				break;
			}
			Community subjectCommunity4 = GetSubjectCommunity(actor, target, obj, param);
			num = ((subjectCommunity4 == null) ? 0f : ((float)subjectCommunity4.GetGoldAmount()));
			break;
		}
		case ConditionType.ValueOfAllSupplies:
		{
			TileObject subject7 = GetSubject<TileObject>(actor, target, obj, param);
			num = ((subject7 != null && subject7.GetInventory() != null) ? subject7.GetInventory().CalcLootableSuppliesValue(subject7) : (GetSubjectCommunity(actor, target, obj, param)?.CalcLootableSuppliesValue() ?? 0f));
			break;
		}
		case ConditionType.ItemCountOfType:
		{
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			TileObject subject4 = GetSubject<TileObject>(actor, target, obj, param);
			if (subject4 != null && subject4.GetInventory() != null)
			{
				num = subject4.GetInventory().CountItemsOfType(equipmentPrototype);
				break;
			}
			Community subjectCommunity = GetSubjectCommunity(actor, target, obj, param);
			num = ((subjectCommunity == null) ? 0f : ((float)subjectCommunity.CountInventoryItemsOfType(equipmentPrototype)));
			break;
		}
		case ConditionType.ConditionBlockResult:
			if (ConditionBlocks != null && ConditionBlocks.Count > 0)
			{
				ConditionBlock conditionBlock = ConditionBlocks[0];
				if (conditionBlock != null)
				{
					num = conditionBlock.Evaluate(actor, target, obj, param);
				}
			}
			break;
		case ConditionType.DaysSinceGameStart:
			num = (float)Session.Instance.PlayTime.TotalSeconds / Sun.DayLengthSecs;
			break;
		default:
			num = 1f;
			break;
		}
		return num * PriorityFactor;
	}
}
