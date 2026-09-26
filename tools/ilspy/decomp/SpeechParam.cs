using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

public struct SpeechParam : IScriptListItem<SpeechParam>
{
	public static string[] TypeNames = StringUtil.GetEnumNames<SpeechParamType>();

	[XmlAttribute]
	public SpeechParamType Type;

	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier Subject;

	[XmlAttribute]
	[DefaultValue(Specifier.Invalid)]
	public Specifier Object;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier SubjectModifier;

	[XmlAttribute]
	[DefaultValue(SpecifierModifier.None)]
	public SpecifierModifier ObjectModifier;

	[XmlAttribute]
	public string SubjectID;

	[XmlAttribute]
	public string ObjectID;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float Data;

	[XmlAttribute]
	public string StringData;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool BoolData;

	public List<ConditionBlockRef> Formulas;

	public string GetTypeName()
	{
		return "Speech Param";
	}

	public void Construct()
	{
	}

	public T GetSubject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public T GetObject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public Community GetSubjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierCommunity(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public Community GetObjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierCommunity(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public Equipment GetSubjectEquipment(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierEquipment(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public Equipment GetObjectEquipment(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierEquipment(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public EquipmentPrototype GetSubjectEquipmentPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierEquipmentPrototype(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public EquipmentPrototype GetObjectEquipmentPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierEquipmentPrototype(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public LiquidPrototype GetSubjectLiquidPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierLiquidPrototype(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	public LiquidPrototype GetObjectLiquidPrototype(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierLiquidPrototype(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	public static bool HasSubject(SpeechParamType type, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (type)
		{
		case SpeechParamType.FirstName:
		case SpeechParamType.FullName:
		case SpeechParamType.GuyOrChick:
		case SpeechParamType.BastardOrBitch:
		case SpeechParamType.AssholeOrBitch:
		case SpeechParamType.ManOrWoman:
		case SpeechParamType.SirOrMadam:
		case SpeechParamType.HimOrHer:
		case SpeechParamType.HimOrHerOrYou:
		case SpeechParamType.HeOrShe:
		case SpeechParamType.HeOrSheOrYou:
		case SpeechParamType.HimOrHerself:
		case SpeechParamType.HimOrHerOrYourself:
		case SpeechParamType.HisOrHer:
		case SpeechParamType.HisOrHerOrYour:
		case SpeechParamType.FirstNamePossessive:
		case SpeechParamType.FirstNameActive:
		case SpeechParamType.FirstNameOrMeOrYou:
		case SpeechParamType.FullNamePossessive:
		case SpeechParamType.FullNameActive:
		case SpeechParamType.FullNameOrMeOrYou:
		case SpeechParamType.Partner:
		case SpeechParamType.GuyOrGirl:
		case SpeechParamType.BoyOrGirl:
		case SpeechParamType.BoyfriendOrGirlfriend:
		case SpeechParamType.HusbandOrWife:
		case SpeechParamType.BroOrSis:
		case SpeechParamType.BrotherOrSister:
		case SpeechParamType.MomOrDad:
		case SpeechParamType.MotherOrFather:
		case SpeechParamType.SonOrDaughter:
		case SpeechParamType.BoxerWagerAmount:
		case SpeechParamType.CommunityName:
		case SpeechParamType.CommunityLeaderNameOrYou:
		case SpeechParamType.CommunityLeaderName:
		case SpeechParamType.CommunityLeaderFirstName:
		case SpeechParamType.CommunityLeaderFullName:
		case SpeechParamType.CommunityPossessive:
		case SpeechParamType.CommunityIOrWe:
		case SpeechParamType.CommunityMeOrUs:
		case SpeechParamType.CommunityMyOrOur:
		case SpeechParamType.CommunityBoxerReadyToFight:
		case SpeechParamType.CommunityMemberWithRole:
		case SpeechParamType.CommunityNumMembersWithGiftedItem:
		case SpeechParamType.CommunitySize:
		case SpeechParamType.ItemName:
		case SpeechParamType.ItemLiquidContentsName:
		case SpeechParamType.AnItemOrSomeLiquid:
		case SpeechParamType.TownName:
		case SpeechParamType.Strain:
		case SpeechParamType.CommunityMiningResource:
		case SpeechParamType.SquadIOrWe:
		case SpeechParamType.SquadMeOrUs:
		case SpeechParamType.SquadMyOrOur:
		case SpeechParamType.NearestTownOrSettlementName:
		case SpeechParamType.NearestCommunityMemberName:
		case SpeechParamType.NearestCommunityMemberHeOrShe:
		case SpeechParamType.MostLikedCommunityMemberName:
		case SpeechParamType.MostLikedCommunityMemberHeOrShe:
		case SpeechParamType.VoteCount:
		case SpeechParamType.BuildingName:
		case SpeechParamType.RelationshipName:
		case SpeechParamType.NumGoodRelationshipsInCommunity:
		case SpeechParamType.DaysSinceMostRecentInfectedInjury:
		case SpeechParamType.MrOrMs:
		case SpeechParamType.Surname:
		case SpeechParamType.Age:
		case SpeechParamType.CommunityNameOrI:
			hasSpecifier = true;
			return true;
		case SpeechParamType.PriceToBandage:
		case SpeechParamType.PriceToSellItem:
		case SpeechParamType.PriceToSellFood:
		case SpeechParamType.PriceToSellWater:
		case SpeechParamType.PriceToSell:
			hasSpecifier = true;
			title = "Seller";
			return true;
		case SpeechParamType.PriceToBuy:
			hasSpecifier = true;
			title = "Buyer";
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasObject(SpeechParamType type, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (type)
		{
		case SpeechParamType.NearestCommunityMemberName:
		case SpeechParamType.NearestCommunityMemberHeOrShe:
		case SpeechParamType.MostLikedCommunityMemberName:
		case SpeechParamType.MostLikedCommunityMemberHeOrShe:
		case SpeechParamType.RelationshipName:
		case SpeechParamType.NumGoodRelationshipsInCommunity:
			hasSpecifier = true;
			return true;
		case SpeechParamType.PriceToBandage:
		case SpeechParamType.PriceToSellItem:
		case SpeechParamType.PriceToSellFood:
		case SpeechParamType.PriceToSellWater:
		case SpeechParamType.PriceToSell:
			hasSpecifier = true;
			title = "Buyer";
			return true;
		case SpeechParamType.PriceToBuy:
			hasSpecifier = true;
			title = "Seller";
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasData(SpeechParamType type, out string title, out bool hasFormula)
	{
		title = null;
		hasFormula = false;
		switch (type)
		{
		case SpeechParamType.PriceToSell:
		case SpeechParamType.PriceToBuy:
			title = "Base Price";
			hasFormula = true;
			return true;
		case SpeechParamType.Formula:
			title = "Base Amount";
			hasFormula = true;
			return true;
		default:
			return false;
		}
	}

	public static bool HasStringData(SpeechParamType type, out string title, out List<string> options, out bool wantSort)
	{
		GameImpl instance = GameImpl.Instance;
		options = null;
		title = null;
		wantSort = true;
		switch (type)
		{
		case SpeechParamType.PriceToSellItem:
		case SpeechParamType.CommunityNumMembersWithGiftedItem:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in currentStory.EquipmentPrototypes)
				{
					if (!options.Contains(equipmentPrototype.Key))
					{
						options.Add(equipmentPrototype.Key);
					}
				}
			}
			title = "Item:";
			return true;
		case SpeechParamType.CommunityMemberWithRole:
			options = new List<string>(Character.RoleNames);
			title = "Role:";
			return true;
		case SpeechParamType.NearestCommunityMemberName:
		case SpeechParamType.NearestCommunityMemberHeOrShe:
			options = BaseMenu.GetAnimalOptions(includeHuman: true);
			title = "Type:";
			return true;
		case SpeechParamType.DaysSinceMostRecentInfectedInjury:
			options = new List<string>(Injury.InfectionTypeNames);
			title = "Strain:";
			return true;
		default:
			return false;
		}
	}

	public static bool HasBoolData(SpeechParamType type, out string title)
	{
		title = null;
		switch (type)
		{
		case SpeechParamType.FirstName:
		case SpeechParamType.FullName:
		case SpeechParamType.FirstNamePossessive:
		case SpeechParamType.FirstNameActive:
		case SpeechParamType.FirstNameOrMeOrYou:
		case SpeechParamType.FullNamePossessive:
		case SpeechParamType.FullNameActive:
		case SpeechParamType.FullNameOrMeOrYou:
		case SpeechParamType.CommunityLeaderFirstName:
		case SpeechParamType.CommunityLeaderFullName:
		case SpeechParamType.Surname:
			title = "Name Known";
			return true;
		case SpeechParamType.MostLikedCommunityMemberName:
		case SpeechParamType.MostLikedCommunityMemberHeOrShe:
			title = "Ignore Player";
			return true;
		default:
			return false;
		}
	}

	public SpeechParam FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		SpeechParam result = this;
		Condition.FixupSpecifier(ref result.Subject, ref result.SubjectModifier, ref result.SubjectID);
		Condition.FixupSpecifier(ref result.Object, ref result.ObjectModifier, ref result.ObjectID);
		if (result.Formulas != null)
		{
			for (int i = 0; i < result.Formulas.Count; i++)
			{
				result.Formulas[i] = result.Formulas[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (result.Formulas != null && result.Formulas.Count == 0)
		{
			result.Formulas = null;
		}
		return result;
	}
}
