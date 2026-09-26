using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

public class MemoryPrototype
{
	public static MemoryPrototype Died;

	public static MemoryPrototype Killed;

	public static MemoryPrototype KilledLooter;

	public static MemoryPrototype KilledZombie;

	public static MemoryPrototype KilledAccidentally;

	public static MemoryPrototype KilledPossibleInvisibleStrain;

	public static MemoryPrototype Hurt;

	public static MemoryPrototype HurtAccidentally;

	public static MemoryPrototype HurtPossibleInvisibleStrain;

	public static MemoryPrototype KnockedOut;

	public static MemoryPrototype FailedAttack;

	public static MemoryPrototype DestroyedProperty;

	public static MemoryPrototype MurderSuspect;

	public static MemoryPrototype AttackSuspect;

	public static MemoryPrototype ArsonSuspect;

	public static MemoryPrototype TheftSuspect;

	public static MemoryPrototype StoleFrom;

	public static MemoryPrototype DeclaredWar;

	public static MemoryPrototype SavedLife;

	public static MemoryPrototype BandagedForGold;

	public static MemoryPrototype BandagedForFree;

	public static MemoryPrototype GaveThoughtfulGift;

	public static MemoryPrototype GaveInappropriateGift;

	public static MemoryPrototype GaveGift;

	public static MemoryPrototype LeadershipCausedHunger;

	public static MemoryPrototype LeadershipCausedThirst;

	public static MemoryPrototype LeadershipCausedSleepDeprivation;

	public static MemoryPrototype LeadershipCausedHypothermia;

	public static MemoryPrototype LeadershipCausedLowMorale;

	public static MemoryPrototype LeadershipCausedLackOfHygiene;

	public static MemoryPrototype LeadershipCausedLackOfSpace;

	public static MemoryPrototype LeadershipCausedOverwork;

	public static MemoryPrototype LeadershipCausedDeath;

	public static MemoryPrototype LeadershipDefeated;

	public static MemoryPrototype LeadershipBuiltTech;

	public static MemoryPrototype KickedFromCommunity;

	public static MemoryPrototype KickedPossibleInvisibleStrain;

	public static MemoryPrototype LeftCommunity;

	public static MemoryPrototype PoachedFrom;

	public static MemoryPrototype LostFisticuffs;

	public static MemoryPrototype WonFisticuffs;

	public static MemoryPrototype LostSnowballFight;

	public static MemoryPrototype WonSnowballFight;

	public static MemoryPrototype BeatUp;

	public static MemoryPrototype FoughtOff;

	public static MemoryPrototype Coup;

	public static MemoryPrototype RestrainedSuccessfully;

	public static MemoryPrototype RestrainedUnsuccessfully;

	public static MemoryPrototype BoughtFrom;

	public static MemoryPrototype SoldTo;

	public static MemoryPrototype Begged;

	public static MemoryPrototype AteTastyFood;

	public static MemoryPrototype AteDisgustingFood;

	public static MemoryPrototype DrankTastyDrink;

	public static MemoryPrototype DrankDisgustingDrink;

	public static MemoryPrototype DrankTastyAlcohol;

	public static MemoryPrototype DrankDisgustingAlcohol;

	public static MemoryPrototype Chatted;

	public static MemoryPrototype HasInvisibleStrain;

	public static MemoryPrototype ContractedInvisibleStrain;

	public static MemoryPrototype AccusedOfInvisibleStrain;

	public static MemoryPrototype AccusedOfInvisibleStrainCorrectly;

	public static MemoryPrototype CheatedOn;

	public static MemoryPrototype DeclaredLove;

	public static MemoryPrototype Rejected;

	public static MemoryPrototype Cannibal;

	public static MemoryPrototype UsedHumanIngredients;

	public static MemoryPrototype FedHumanMeatTo;

	public static MemoryPrototype DesecratedCorpse;

	public static MemoryPrototype Helped;

	public static MemoryPrototype RippedOff;

	public static MemoryPrototype BrokeAlliance;

	public static MemoryPrototype LookedAfter;

	public static MemoryPrototype ThreatenedSuccessfully;

	public static MemoryPrototype Surrendered;

	public static MemoryPrototype FlirtedSuccessfullyWith;

	public static MemoryPrototype FlirtedUnsuccessfullyWith;

	public static MemoryPrototype Dumped;

	public static MemoryPrototype History;

	public static MemoryPrototype Regrets;

	public static MemoryPrototype Past;

	public static MemoryPrototype Miscarriage;

	public static MemoryPrototype DiedInChildbirth;

	public static MemoryPrototype Rescued;

	public static MemoryPrototype NeededRescue;

	public string UniqueID = "";

	public string NativeDescription = "";

	[XmlIgnore]
	public int DescriptionHash;

	[NotVisible]
	[DefaultValue(true)]
	public bool CanBeForgotten = true;

	[DefaultValue(CanForget.Yes)]
	public CanForget CanForget;

	[DefaultValue(false)]
	public bool SpreadToAllSubjectCommunity;

	[DefaultValue(false)]
	public bool SpreadToAllObjectCommunity;

	[DefaultValue(false)]
	public bool SpreadToAllCommunities;

	[DefaultValue(false)]
	public bool RememberedBySubject;

	[DefaultValue(true)]
	public bool WitnessedByObject = true;

	[DefaultValue(MemoryRuleSet.Normal)]
	public MemoryRuleSet RuleSet;

	public float GoodnessBadness;

	public float DifficultyPatheticness;

	[DefaultValue(0f)]
	public float MoraleBoost;

	[DefaultValue(1f)]
	public float MoraleMultiplier = 1f;

	[DefaultValue(0f)]
	public float QuantityLimit;

	[DefaultValue(0f)]
	public float ApprovalLimit;

	[DefaultValue(0f)]
	public float RespectLimit;

	[DefaultValue(1f)]
	public float FadeSpeed = 1f;

	[DefaultValue("")]
	public string ReciprocalMemoryID = "";

	public List<string> Replace;

	[DefaultValue(true)]
	public bool CanBeARumour = true;

	[DefaultValue(MemoryObjectType.Character)]
	public MemoryObjectType ObjectType = MemoryObjectType.Character;

	[DefaultValue(false)]
	public bool ActorMustBeSelf;

	[DefaultValue(false)]
	public bool ActorMustBeLeader;

	[DefaultValue(false)]
	public bool ObjectMustBeDead;

	[DefaultValue(false)]
	public bool MustBeACouple;

	[DefaultValue(false)]
	public bool MustBeAttractedTo;

	public static void CacheMemoryPrototypeRefs()
	{
		GameImpl instance = GameImpl.Instance;
		Died = instance.FindMemoryPrototypeByUniqueID("Died");
		Killed = instance.FindMemoryPrototypeByUniqueID("Killed");
		KilledLooter = instance.FindMemoryPrototypeByUniqueID("KilledLooter");
		KilledZombie = instance.FindMemoryPrototypeByUniqueID("KilledZombie");
		KilledAccidentally = instance.FindMemoryPrototypeByUniqueID("KilledAccidentally");
		KilledPossibleInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("KilledPossibleInvisibleStrain");
		Hurt = instance.FindMemoryPrototypeByUniqueID("Hurt");
		HurtAccidentally = instance.FindMemoryPrototypeByUniqueID("HurtAccidentally");
		HurtPossibleInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("HurtPossibleInvisibleStrain");
		KnockedOut = instance.FindMemoryPrototypeByUniqueID("KnockedOut");
		FailedAttack = instance.FindMemoryPrototypeByUniqueID("FailedAttack");
		DestroyedProperty = instance.FindMemoryPrototypeByUniqueID("DestroyedProperty");
		MurderSuspect = instance.FindMemoryPrototypeByUniqueID("MurderSuspect");
		AttackSuspect = instance.FindMemoryPrototypeByUniqueID("AttackSuspect");
		ArsonSuspect = instance.FindMemoryPrototypeByUniqueID("ArsonSuspect");
		TheftSuspect = instance.FindMemoryPrototypeByUniqueID("TheftSuspect");
		StoleFrom = instance.FindMemoryPrototypeByUniqueID("StoleFrom");
		DeclaredWar = instance.FindMemoryPrototypeByUniqueID("DeclaredWar");
		SavedLife = instance.FindMemoryPrototypeByUniqueID("SavedLife");
		BandagedForGold = instance.FindMemoryPrototypeByUniqueID("BandagedForGold");
		BandagedForFree = instance.FindMemoryPrototypeByUniqueID("BandagedForFree");
		GaveThoughtfulGift = instance.FindMemoryPrototypeByUniqueID("GaveThoughtfulGift");
		GaveInappropriateGift = instance.FindMemoryPrototypeByUniqueID("GaveInappropriateGift");
		GaveGift = instance.FindMemoryPrototypeByUniqueID("GaveGift");
		LeadershipCausedHunger = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedHunger");
		LeadershipCausedThirst = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedThirst");
		LeadershipCausedSleepDeprivation = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedSleepDeprivation");
		LeadershipCausedHypothermia = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedHypothermia");
		LeadershipCausedLowMorale = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedLowMorale");
		LeadershipCausedLackOfHygiene = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedLackOfHygiene");
		LeadershipCausedLackOfSpace = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedLackOfSpace");
		LeadershipCausedOverwork = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedOverwork");
		LeadershipCausedDeath = instance.FindMemoryPrototypeByUniqueID("LeadershipCausedDeath");
		LeadershipDefeated = instance.FindMemoryPrototypeByUniqueID("LeadershipDefeated");
		LeadershipBuiltTech = instance.FindMemoryPrototypeByUniqueID("LeadershipBuiltTech");
		KickedFromCommunity = instance.FindMemoryPrototypeByUniqueID("KickedFromCommunity");
		KickedPossibleInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("KickedPossibleInvisibleStrain");
		LeftCommunity = instance.FindMemoryPrototypeByUniqueID("LeftCommunity");
		PoachedFrom = instance.FindMemoryPrototypeByUniqueID("PoachedFrom");
		LostFisticuffs = instance.FindMemoryPrototypeByUniqueID("LostFisticuffs");
		WonFisticuffs = instance.FindMemoryPrototypeByUniqueID("WonFisticuffs");
		LostSnowballFight = instance.FindMemoryPrototypeByUniqueID("LostSnowballFight");
		WonSnowballFight = instance.FindMemoryPrototypeByUniqueID("WonSnowballFight");
		BeatUp = instance.FindMemoryPrototypeByUniqueID("BeatUp");
		FoughtOff = instance.FindMemoryPrototypeByUniqueID("FoughtOff");
		Coup = instance.FindMemoryPrototypeByUniqueID("Coup");
		RestrainedSuccessfully = instance.FindMemoryPrototypeByUniqueID("RestrainedSuccessfully");
		RestrainedUnsuccessfully = instance.FindMemoryPrototypeByUniqueID("RestrainedUnsuccessfully");
		BoughtFrom = instance.FindMemoryPrototypeByUniqueID("BoughtFrom");
		SoldTo = instance.FindMemoryPrototypeByUniqueID("SoldTo");
		Begged = instance.FindMemoryPrototypeByUniqueID("Begged");
		AteTastyFood = instance.FindMemoryPrototypeByUniqueID("AteTastyFood");
		AteDisgustingFood = instance.FindMemoryPrototypeByUniqueID("AteDisgustingFood");
		DrankTastyDrink = instance.FindMemoryPrototypeByUniqueID("DrankTastyDrink");
		DrankDisgustingDrink = instance.FindMemoryPrototypeByUniqueID("DrankDisgustingDrink");
		DrankTastyAlcohol = instance.FindMemoryPrototypeByUniqueID("DrankTastyAlcohol");
		DrankDisgustingAlcohol = instance.FindMemoryPrototypeByUniqueID("DrankDisgustingAlcohol");
		Chatted = instance.FindMemoryPrototypeByUniqueID("Chatted");
		HasInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("HasInvisibleStrain");
		ContractedInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("ContractedInvisibleStrain");
		AccusedOfInvisibleStrain = instance.FindMemoryPrototypeByUniqueID("AccusedOfInvisibleStrain");
		AccusedOfInvisibleStrainCorrectly = instance.FindMemoryPrototypeByUniqueID("AccusedOfInvisibleStrainCorrectly");
		CheatedOn = instance.FindMemoryPrototypeByUniqueID("CheatedOn");
		DeclaredLove = instance.FindMemoryPrototypeByUniqueID("DeclaredLove");
		Rejected = instance.FindMemoryPrototypeByUniqueID("Rejected");
		Cannibal = instance.FindMemoryPrototypeByUniqueID("Cannibal");
		UsedHumanIngredients = instance.FindMemoryPrototypeByUniqueID("UsedHumanIngredients");
		FedHumanMeatTo = instance.FindMemoryPrototypeByUniqueID("FedHumanMeatTo");
		DesecratedCorpse = instance.FindMemoryPrototypeByUniqueID("DesecratedCorpse");
		Helped = instance.FindMemoryPrototypeByUniqueID("Helped");
		RippedOff = instance.FindMemoryPrototypeByUniqueID("RippedOff");
		BrokeAlliance = instance.FindMemoryPrototypeByUniqueID("BrokeAlliance");
		LookedAfter = instance.FindMemoryPrototypeByUniqueID("LookedAfter");
		ThreatenedSuccessfully = instance.FindMemoryPrototypeByUniqueID("ThreatenedSuccessfully");
		Surrendered = instance.FindMemoryPrototypeByUniqueID("Surrendered");
		FlirtedSuccessfullyWith = instance.FindMemoryPrototypeByUniqueID("FlirtedSuccessfullyWith");
		FlirtedUnsuccessfullyWith = instance.FindMemoryPrototypeByUniqueID("FlirtedUnsuccessfullyWith");
		Dumped = instance.FindMemoryPrototypeByUniqueID("Dumped");
		History = instance.FindMemoryPrototypeByUniqueID("History");
		Regrets = instance.FindMemoryPrototypeByUniqueID("Regrets");
		Past = instance.FindMemoryPrototypeByUniqueID("Past");
		Miscarriage = instance.FindMemoryPrototypeByUniqueID("Miscarriage");
		DiedInChildbirth = instance.FindMemoryPrototypeByUniqueID("DiedInChildbirth");
		Rescued = instance.FindMemoryPrototypeByUniqueID("Rescued");
		NeededRescue = instance.FindMemoryPrototypeByUniqueID("NeededRescue");
	}

	public string GetDescriptionKey()
	{
		return "MEMORY_" + UniqueID;
	}

	public void CopyFrom(MemoryPrototype other)
	{
		FieldInfo[] fields = typeof(MemoryPrototype).GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(other));
			}
		}
	}
}
