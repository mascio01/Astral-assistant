using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;

public struct StoryEvent : IReflectable, IScriptListItem<StoryEvent>
{
	private struct ObjScore : IComparable<ObjScore>
	{
		public TileObject Obj;

		public float Score;

		public ObjScore(TileObject obj, float score)
		{
			Obj = obj;
			Score = score;
		}

		public int CompareTo(ObjScore other)
		{
			if (Score < other.Score)
			{
				return -1;
			}
			if (Score > other.Score)
			{
				return 1;
			}
			if (Obj.Id < other.Obj.Id)
			{
				return -1;
			}
			if (Obj.Id > other.Obj.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	private struct EquipmentScore : IComparable<EquipmentScore>
	{
		public Equipment Item;

		public float Score;

		public EquipmentScore(Equipment item, float score)
		{
			Item = item;
			Score = score;
		}

		public int CompareTo(EquipmentScore other)
		{
			if (Score < other.Score)
			{
				return -1;
			}
			if (Score > other.Score)
			{
				return 1;
			}
			if (Item.Id < other.Item.Id)
			{
				return -1;
			}
			if (Item.Id > other.Item.Id)
			{
				return 1;
			}
			return 0;
		}
	}

	public static string[] TypeNames = StringUtil.GetEnumNames<StoryEventType>();

	[XmlAttribute]
	public StoryEventType Type;

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
	public string StringData2;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool BoolData;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool BoolData2;

	[XmlAttribute]
	[DefaultValue(false)]
	public bool BoolData3;

	[XmlAttribute]
	[DefaultValue(SecrecyMode.Public)]
	public SecrecyMode Secrecy;

	[XmlAttribute]
	[DefaultValue(0f)]
	public float Delay;

	public List<ConditionBlockRef> Formulas;

	public List<SpeechRef> Speeches;

	public static float NearbyDist = 64f;

	public StoryEvent FixupAfterXmlLoad(Script script, Story story, Dictionary<string, string> newUniqueIDs)
	{
		StoryEvent result = this;
		Condition.FixupSpecifier(ref result.Subject, ref result.SubjectModifier, ref result.SubjectID);
		Condition.FixupSpecifier(ref result.Object, ref result.ObjectModifier, ref result.ObjectID);
		Condition.FixupSpecifier(ref result.ThirdParty, ref result.ThirdPartyModifier, ref result.ThirdPartyID);
		if (result.Formulas != null)
		{
			for (int i = 0; i < result.Formulas.Count; i++)
			{
				result.Formulas[i] = result.Formulas[i].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		if (result.Speeches != null)
		{
			for (int j = 0; j < result.Speeches.Count; j++)
			{
				result.Speeches[j] = result.Speeches[j].FixupAfterXmlLoad(script, story, newUniqueIDs);
			}
		}
		switch (result.Type)
		{
		case StoryEventType.EnableScriptedMove:
		case StoryEventType.DisableScriptedMove:
			if (!string.IsNullOrEmpty(result.ObjectID) && result.Object == Specifier.Invalid)
			{
				result.Object = Specifier.ID;
			}
			break;
		case StoryEventType.Speak:
			if (!string.IsNullOrEmpty(result.StringData))
			{
				result.Speeches.Add(SpeechRef.Create(result.StringData));
				result.StringData = string.Empty;
			}
			break;
		}
		if (result.Formulas != null && result.Formulas.Count == 0)
		{
			result.Formulas = null;
		}
		if (result.Speeches != null && result.Speeches.Count == 0)
		{
			result.Speeches = null;
		}
		return result;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Type);
		reflector.Add(ref Subject);
		reflector.Add(ref Object);
		reflector.AddAfter(ref ThirdParty, 124);
		reflector.AddAfter(ref SubjectModifier, 561);
		reflector.AddAfter(ref ObjectModifier, 561);
		reflector.AddAfter(ref ThirdPartyModifier, 561);
		reflector.Add(ref SubjectID);
		reflector.Add(ref ObjectID);
		reflector.AddAfter(ref ThirdPartyID, 124);
		reflector.Add(ref Data);
		reflector.AddAfter(ref Data2, 247);
		reflector.Add(ref StringData);
		reflector.AddAfter(ref StringData2, 561);
		reflector.Add(ref BoolData);
		reflector.Add(ref BoolData2);
		reflector.Add(ref BoolData3);
		reflector.Add(ref Secrecy);
		reflector.Add(ref Delay);
		reflector.AddAfter(ref Speeches, 561);
	}

	public string GetTypeName()
	{
		return "Event";
	}

	public void Construct()
	{
	}

	public static bool HasSubject(StoryEventType type, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (type)
		{
		case StoryEventType.None:
			hasSpecifier = false;
			return false;
		case StoryEventType.DeleteGameObject:
			hasSpecifier = false;
			return true;
		case StoryEventType.OneShotTrigger:
			hasSpecifier = false;
			return false;
		case StoryEventType.GameComplete:
		case StoryEventType.GameOver:
		case StoryEventType.SaveGame:
		case StoryEventType.SurpriseSaveGameOverwrite:
		case StoryEventType.SetFollowerCommandsEnabled:
		case StoryEventType.SetRoadEntranceEnabled:
		case StoryEventType.SetHitTheRoadEnabled:
		case StoryEventType.RememberSpokenOnce:
		case StoryEventType.TriggerInvaders:
		case StoryEventType.UnlockAchievement:
		case StoryEventType.GameCompleteOutbreak:
			hasSpecifier = false;
			return false;
		case StoryEventType.DiscoverQuest:
		case StoryEventType.CompleteQuest:
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		case StoryEventType.FailQuest:
		case StoryEventType.FailQuestIfItIsDiscovered:
		case StoryEventType.SetQuestParam:
		case StoryEventType.CompleteQuestGroup:
		case StoryEventType.FailQuestGroup:
			title = "Quest Giver:";
			hasSpecifier = true;
			return true;
		case StoryEventType.SellObject:
		case StoryEventType.SellItem:
		case StoryEventType.Sell:
			title = "Seller:";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = true;
			return true;
		}
	}

	public static bool HasObject(StoryEventType type, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (type)
		{
		case StoryEventType.DiscoverCharacterName:
		case StoryEventType.DiscoverCommunityName:
		case StoryEventType.DiscoverRelationship:
		case StoryEventType.GiveEquipment:
		case StoryEventType.GiveItemWithLiquid:
		case StoryEventType.GiveObject:
		case StoryEventType.SetVariable:
		case StoryEventType.IncrementVariable:
		case StoryEventType.OpenTradeScreen:
		case StoryEventType.OpenSwapSuppliesScreen:
		case StoryEventType.Speak:
		case StoryEventType.RememberSpokenSpeech:
		case StoryEventType.MemorableEvent:
		case StoryEventType.JoinCommunity:
		case StoryEventType.AddToSquad:
		case StoryEventType.StopFollowing:
		case StoryEventType.Follow:
		case StoryEventType.Introduce:
		case StoryEventType.IntroductionFinished:
		case StoryEventType.Unintroduce:
		case StoryEventType.DeclareWar:
		case StoryEventType.CeaseFire:
		case StoryEventType.RegisterAttack:
		case StoryEventType.SetCommunityInvasionTarget:
		case StoryEventType.SellFood:
		case StoryEventType.SellDrink:
		case StoryEventType.GiveFood:
		case StoryEventType.GiveDrink:
		case StoryEventType.ApplyBandage:
		case StoryEventType.SellBandage:
		case StoryEventType.GiveAntigen:
		case StoryEventType.SellAntigen:
		case StoryEventType.GiveAllGold:
		case StoryEventType.GiveAllSupplies:
		case StoryEventType.PretendShakedown:
		case StoryEventType.EnableScriptedMove:
		case StoryEventType.DisableScriptedMove:
		case StoryEventType.EnableTrigger:
		case StoryEventType.DisableTrigger:
		case StoryEventType.Hug:
		case StoryEventType.HugShoot:
		case StoryEventType.Attack:
		case StoryEventType.Assassinate:
		case StoryEventType.StartBoxingMatch:
		case StoryEventType.WinBoxingMatch:
		case StoryEventType.LoseBoxingMatch:
		case StoryEventType.GiveItemOfClothingType:
		case StoryEventType.DisableEmpathy:
		case StoryEventType.EnableEmpathy:
		case StoryEventType.SetRelationship:
		case StoryEventType.RefundItem:
		case StoryEventType.OneShotTriggerWithParams:
		case StoryEventType.DontInfectWithInvisibleStrain:
		case StoryEventType.FormAlliance:
		case StoryEventType.LeaveAlliance:
		case StoryEventType.RefusePlayerSurrender:
		case StoryEventType.TakeAllGold:
		case StoryEventType.TakeAllSupplies:
		case StoryEventType.VoteFor:
		case StoryEventType.PlayerSurrender:
		case StoryEventType.OccupyBase:
		case StoryEventType.AmbushMemberWithItem:
		case StoryEventType.PretendExtortion:
		case StoryEventType.CommunityFollow:
		case StoryEventType.GiveAllWeapons:
		case StoryEventType.RefreshTarget:
		case StoryEventType.Treat:
		case StoryEventType.Teleport:
		case StoryEventType.SetAutoCollectItem:
		case StoryEventType.RepairVehicleWithItem:
		case StoryEventType.DiscoverSkillIfPlayer:
		case StoryEventType.DiscoverPersonalityIfPlayer:
		case StoryEventType.DiscoverRelationshipIfPlayer:
		case StoryEventType.AssignBlameForPickpocketedItem:
			hasSpecifier = true;
			return true;
		case StoryEventType.SetHangOutLocation:
			hasSpecifier = false;
			return true;
		case StoryEventType.DeleteMemory:
			title = "Memory Subject:";
			hasSpecifier = true;
			return true;
		case StoryEventType.DiscoverQuest:
		case StoryEventType.CompleteQuest:
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		case StoryEventType.FailQuest:
		case StoryEventType.FailQuestIfItIsDiscovered:
		case StoryEventType.CompleteQuestGroup:
		case StoryEventType.FailQuestGroup:
			title = "Quest Object:";
			hasSpecifier = true;
			return true;
		case StoryEventType.SetQuestParam:
			title = "Quest Seeker:";
			hasSpecifier = true;
			return true;
		case StoryEventType.SellObject:
		case StoryEventType.SellItem:
		case StoryEventType.Sell:
			title = "Buyer:";
			hasSpecifier = true;
			return true;
		case StoryEventType.Explode:
			title = "Who to Blame:";
			hasSpecifier = true;
			return true;
		case StoryEventType.LeaveCommunity:
		case StoryEventType.RejoinInitialCommunity:
			title = "Kicked By:";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasThirdParty(StoryEventType type, out bool hasSpecifier, out string title)
	{
		title = null;
		switch (type)
		{
		case StoryEventType.Speak:
		case StoryEventType.MemorableEvent:
		case StoryEventType.EnableTrigger:
		case StoryEventType.DisableTrigger:
		case StoryEventType.Hug:
		case StoryEventType.HugShoot:
		case StoryEventType.OneShotTriggerWithParams:
		case StoryEventType.DiscoverRelationshipIfPlayer:
			hasSpecifier = true;
			return true;
		case StoryEventType.DeleteMemory:
			title = "Memory Object:";
			hasSpecifier = true;
			return true;
		case StoryEventType.DiscoverQuest:
		case StoryEventType.CompleteQuest:
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		case StoryEventType.FailQuest:
		case StoryEventType.FailQuestIfItIsDiscovered:
		case StoryEventType.CompleteQuestGroup:
		case StoryEventType.FailQuestGroup:
			title = "Quest Seeker:";
			hasSpecifier = true;
			return true;
		case StoryEventType.SetQuestParam:
			title = "Quest Param:";
			hasSpecifier = true;
			return true;
		case StoryEventType.Explode:
			title = "Intended Target:";
			hasSpecifier = true;
			return true;
		case StoryEventType.GiveObject:
			title = "Item:";
			hasSpecifier = true;
			return true;
		case StoryEventType.RepairVehicleWithItem:
			title = "Speak To:";
			hasSpecifier = true;
			return true;
		default:
			hasSpecifier = false;
			return false;
		}
	}

	public static bool HasData(StoryEventType type, out string title, out bool hasFormula)
	{
		title = null;
		hasFormula = false;
		switch (type)
		{
		case StoryEventType.SpawnEquipment:
		case StoryEventType.DeleteEquipment:
		case StoryEventType.GiveEquipment:
		case StoryEventType.SellItem:
		case StoryEventType.RefundItem:
		case StoryEventType.SetSkinnedAmount:
		case StoryEventType.SetInitialMemberCount:
		case StoryEventType.TriggerInvaders:
		case StoryEventType.RepairVehicleWithItem:
		case StoryEventType.SetCarryAmount:
			title = "Amount:";
			return true;
		case StoryEventType.SetVariable:
		case StoryEventType.IncrementVariable:
			title = "Value:";
			return true;
		case StoryEventType.MemorableEvent:
			title = "Quantity:";
			return true;
		case StoryEventType.SetCommunityInvasionTarget:
		case StoryEventType.Encourage:
		case StoryEventType.PretendExtortion:
		case StoryEventType.AddDownTime:
			title = "For Days:";
			return true;
		case StoryEventType.PretendShakedown:
			title = "Pretend Gold:";
			return true;
		case StoryEventType.SetTimer:
			title = "Time:";
			return true;
		case StoryEventType.SetRelationship:
			title = "Approval:";
			return true;
		case StoryEventType.Sell:
			title = "Base Price:";
			hasFormula = true;
			return true;
		case StoryEventType.GiveAllGold:
		case StoryEventType.GiveAllSupplies:
		case StoryEventType.TakeAllGold:
		case StoryEventType.TakeAllSupplies:
			title = "Worth:";
			hasFormula = true;
			return true;
		case StoryEventType.SetSkill:
		case StoryEventType.SetSkillCap:
			title = "Min:";
			return true;
		case StoryEventType.SetLimitBloodLoss:
			title = "Limit (0=none):";
			return true;
		case StoryEventType.Explode:
			title = "Damage";
			return true;
		case StoryEventType.AmbushMemberWithItem:
			title = "Teleport after (s)";
			return true;
		case StoryEventType.SetSurrenderThreshold:
			title = "Buddies Left";
			return true;
		case StoryEventType.DisableEmpathy:
			title = "Empathy Override";
			return true;
		default:
			return false;
		}
	}

	public static bool HasData2(StoryEventType type, out string title)
	{
		title = null;
		switch (type)
		{
		case StoryEventType.SetRelationship:
			title = "Respect:";
			return true;
		case StoryEventType.SetSkill:
		case StoryEventType.SetSkillCap:
			title = "Max:";
			return true;
		case StoryEventType.Explode:
			title = "Damage Radius:";
			return true;
		case StoryEventType.AmbushMemberWithItem:
			title = "Party Size:";
			return true;
		case StoryEventType.IncrementVariable:
			title = "Cap (0=infinite):";
			return true;
		default:
			return false;
		}
	}

	public static bool HasStringData(StoryEventType type, out string title, out List<string> options, out bool wantSort)
	{
		GameImpl instance = GameImpl.Instance;
		options = null;
		title = null;
		wantSort = true;
		switch (type)
		{
		case StoryEventType.SpawnEquipment:
		case StoryEventType.DeleteEquipment:
		case StoryEventType.GiveEquipment:
		case StoryEventType.GameComplete:
		case StoryEventType.SellItem:
		case StoryEventType.RefundItem:
		case StoryEventType.AmbushMemberWithItem:
		case StoryEventType.SetAutoCollect:
		case StoryEventType.SetCarryAmount:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory in instance.CurrentStories)
			{
				_ = currentStory;
				foreach (KeyValuePair<string, EquipmentPrototype> item in instance.CurrentEquipmentPrototypesDeterministic)
				{
					if (!options.Contains(item.Key))
					{
						options.Add(item.Key);
					}
				}
			}
			title = "Item:";
			return true;
		case StoryEventType.GiveItemWithLiquid:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory2 in instance.CurrentStories)
			{
				_ = currentStory2;
				foreach (KeyValuePair<string, LiquidPrototype> item2 in instance.CurrentLiquidPrototypesDeterministic)
				{
					if (!options.Contains(item2.Key))
					{
						options.Add(item2.Key);
					}
				}
			}
			title = "Liquid:";
			return true;
		case StoryEventType.GiveAntigen:
		case StoryEventType.SellAntigen:
			title = "Item:";
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory3 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, EquipmentPrototype> equipmentPrototype in currentStory3.EquipmentPrototypes)
				{
					if (equipmentPrototype.Value.AntigenType != InfectionType.None && !options.Contains(equipmentPrototype.Key))
					{
						options.Add(equipmentPrototype.Key);
					}
				}
			}
			return true;
		case StoryEventType.MemorableEvent:
		case StoryEventType.DeleteMemory:
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
		case StoryEventType.SetRank:
			options = new List<string>(Character.RankNames);
			title = "Rank";
			return true;
		case StoryEventType.DiscoverNamesOfCommunityMembersWithRole:
		case StoryEventType.SetRole:
		case StoryEventType.CommunityFollow:
			options = new List<string>(Character.RoleNames);
			options.Insert(0, "");
			title = "Role";
			return true;
		case StoryEventType.SetHelicopterAnimState:
			options = new List<string>(Helicopter.HelicopterAnimStateNames);
			title = "Anim State";
			return true;
		case StoryEventType.SetGateState:
			options = new List<string>(Gate.GateStateNames);
			title = "Gate State";
			return true;
		case StoryEventType.IncrementSkill:
		case StoryEventType.SetSkill:
		case StoryEventType.SetSkillCap:
		case StoryEventType.DiscoverSkill:
		case StoryEventType.DiscoverSkillIfPlayer:
			options = new List<string>(Skillset.SkillNames);
			options.Insert(0, "");
			title = "Skill";
			return true;
		case StoryEventType.SetPersonality:
		case StoryEventType.DiscoverPersonality:
		case StoryEventType.DiscoverPersonalityIfPlayer:
			options = instance.GetAllPersonalities();
			title = "Personality";
			return true;
		case StoryEventType.OpenSwapSuppliesScreen:
			options = new List<string>(TakeUIGoal.SwappingSuppliesModeNames);
			title = "Mode";
			return true;
		case StoryEventType.RememberSpokenSpeech:
		case StoryEventType.Hug:
		case StoryEventType.HugShoot:
		case StoryEventType.RememberSpokenOnce:
		case StoryEventType.Treat:
		case StoryEventType.RepairVehicleWithItem:
			title = "Speech:";
			return true;
		case StoryEventType.EnableTrigger:
		case StoryEventType.DisableTrigger:
		case StoryEventType.OneShotTrigger:
		case StoryEventType.FleeAndDisappear:
		case StoryEventType.OneShotTriggerWithParams:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory5 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item3 in currentStory5.ScriptObjectsByUniqueID)
				{
					if (item3.Value is Trigger && !options.Contains(item3.Key))
					{
						options.Add(item3.Key);
					}
				}
			}
			title = "Trigger:";
			return true;
		case StoryEventType.DiscoverCommunityHasNoAntigen:
			options = new List<string>(Injury.InfectionTypeNames);
			title = "Strain";
			return true;
		case StoryEventType.DiscoverQuest:
		case StoryEventType.CompleteQuest:
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		case StoryEventType.FailQuest:
		case StoryEventType.FailQuestIfItIsDiscovered:
		case StoryEventType.SetQuestParam:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory6 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item4 in currentStory6.ScriptObjectsByUniqueID)
				{
					if (item4.Value is Quest && !options.Contains(item4.Key))
					{
						options.Add(item4.Key);
					}
				}
			}
			title = "Quest:";
			return true;
		case StoryEventType.CompleteQuestGroup:
		case StoryEventType.FailQuestGroup:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory7 in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item5 in currentStory7.ScriptObjectsByUniqueID)
				{
					if (item5.Value is QuestGroup && !options.Contains(item5.Key))
					{
						options.Add(item5.Key);
					}
				}
			}
			title = "Quest Group:";
			return true;
		case StoryEventType.GiveItemOfClothingType:
			options = new List<string>(Character.ClothingTypeNames);
			options.Insert(0, "Invalid");
			wantSort = false;
			title = "Clothing Type:";
			return true;
		case StoryEventType.StartBoxingMatch:
			options = new List<string>(Character.SparringTypeNames);
			title = "Sparring Type:";
			return true;
		case StoryEventType.SetTimer:
			title = "Name:";
			return true;
		case StoryEventType.SetVariable:
		case StoryEventType.IncrementVariable:
			title = "Variable:";
			return true;
		case StoryEventType.SpawnTemplate:
			options = BaseMenu.GetTemplateOptions();
			title = "Template:";
			return true;
		case StoryEventType.SetRelationship:
			options = new List<string>(Relationship.RelationshipTypeNames);
			title = "Relationship:";
			return true;
		case StoryEventType.ActivateInvader:
		case StoryEventType.DeactivateInvader:
			options = BaseMenu.GetInvaderOptions();
			title = "Invader:";
			return true;
		case StoryEventType.SetBuildingOfTypeUnlocked:
			options = BaseMenu.GetPropOptions();
			title = "Building Type:";
			return true;
		case StoryEventType.GiveAllSupplies:
		case StoryEventType.TakeAllSupplies:
			options = new List<string>();
			options.Add(string.Empty);
			foreach (Type item6 in GetListOfBaseObjectClassesDerivedFrom(typeof(Equipment)))
			{
				options.Add(item6.Name);
			}
			title = "Type:";
			return true;
		case StoryEventType.EnableScriptedMove:
			options = new List<string>(Character.MovementTypeNames);
			title = "Movement Type:";
			return true;
		case StoryEventType.SetRoadEntranceEnabled:
		case StoryEventType.SetHitTheRoadEnabled:
			title = "Path Name:";
			return true;
		case StoryEventType.SetSurrenderMode:
			options = new List<string>(Character.SurrenderModeNames);
			title = "Mode:";
			return true;
		case StoryEventType.UnlockAchievement:
			options = new List<string>(AchievementsManager.AchievementNames);
			title = "Achievement";
			return true;
		default:
			return false;
		}
	}

	public static List<Type> GetListOfBaseObjectClassesDerivedFrom(Type root)
	{
		List<Type> list = new List<Type>();
		BaseObject[] prototypeGameObjects = BaseObjectManager.PrototypeGameObjects;
		foreach (BaseObject baseObject in prototypeGameObjects)
		{
			if (baseObject == null)
			{
				continue;
			}
			Type type = baseObject.GetType();
			if (type.IsA(root))
			{
				while (type != null && type != root && !list.Contains(type))
				{
					list.Add(type);
					type = type.BaseType;
				}
			}
		}
		return list;
	}

	public static Type FindBaseObjectClassByName(string name)
	{
		BaseObject[] prototypeGameObjects = BaseObjectManager.PrototypeGameObjects;
		foreach (BaseObject baseObject in prototypeGameObjects)
		{
			if (baseObject == null)
			{
				continue;
			}
			Type type = baseObject.GetType();
			while (type != null)
			{
				if (type.Name == name)
				{
					return type;
				}
				type = type.BaseType;
			}
		}
		return null;
	}

	public static bool HasStringData2(StoryEventType type, out string title, out List<string> options, out bool wantSort)
	{
		GameImpl instance = GameImpl.Instance;
		options = null;
		title = null;
		wantSort = true;
		switch (type)
		{
		case StoryEventType.SpawnEquipment:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory in instance.CurrentStories)
			{
				foreach (KeyValuePair<string, BaseScriptObject> item in currentStory.ScriptObjectsByUniqueID)
				{
					if (item.Value is Trigger && !options.Contains(item.Key))
					{
						options.Add(item.Key);
					}
				}
			}
			title = "Trigger:";
			return true;
		case StoryEventType.EnableScriptedMove:
			options = new List<string>(ScriptedGoal.ScriptedMoveImportanceNames);
			title = "Priority:";
			return true;
		case StoryEventType.RepairVehicleWithItem:
			options = new List<string>();
			options.Add("");
			foreach (Story currentStory2 in instance.CurrentStories)
			{
				_ = currentStory2;
				foreach (KeyValuePair<string, EquipmentPrototype> item2 in instance.CurrentEquipmentPrototypesDeterministic)
				{
					if (!options.Contains(item2.Key))
					{
						options.Add(item2.Key);
					}
				}
			}
			title = "Item:";
			return true;
		default:
			return false;
		}
	}

	public static bool HasBoolData(StoryEventType type, out string title, out string title2, out string title3)
	{
		title = null;
		title2 = null;
		title3 = null;
		switch (type)
		{
		case StoryEventType.EnableScriptedMove:
			title = "Disable when reached";
			title2 = "Sit";
			title3 = "Teleport if failed";
			return true;
		case StoryEventType.JoinCommunity:
			title = "Kicked";
			title2 = "Infected";
			title3 = "All Join";
			return true;
		case StoryEventType.LeaveCommunity:
		case StoryEventType.RejoinInitialCommunity:
			title = "Kicked";
			title2 = "Infected";
			title3 = "With Relations";
			return true;
		case StoryEventType.DiscoverQuest:
			title = "Can Rediscover";
			return true;
		case StoryEventType.CompleteQuest:
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		case StoryEventType.CompleteQuestGroup:
			title = "Skip Completion Events";
			return true;
		case StoryEventType.FailQuest:
		case StoryEventType.FailQuestIfItIsDiscovered:
		case StoryEventType.FailQuestGroup:
			title = "Skip Failure Events";
			return true;
		case StoryEventType.GiveAllGold:
		case StoryEventType.GiveAllSupplies:
		case StoryEventType.TakeAllGold:
		case StoryEventType.TakeAllSupplies:
			title = "Is Shakedown";
			title2 = "From Bystanders";
			title3 = "From Everyone";
			return true;
		case StoryEventType.GiveItemOfClothingType:
			title = "Prefer Wearing";
			return true;
		case StoryEventType.SetRelationship:
			title = "Allow Change";
			title2 = "No Notification";
			return true;
		case StoryEventType.Hug:
			title = "Subject Talks Afterwards";
			return true;
		case StoryEventType.GameComplete:
			title = "Show Continue Button";
			title2 = "Gift Not Needed";
			return true;
		case StoryEventType.DrinkAlcohol:
			title = "Ignore Policy";
			return true;
		case StoryEventType.GiveEquipment:
		case StoryEventType.SetGifted:
			title = "Is Gift";
			return true;
		case StoryEventType.SetBuildingOfTypeUnlocked:
			title = "Unlocked";
			return true;
		case StoryEventType.GrantMiningRightsToPlayer:
			title = "Granted";
			return true;
		case StoryEventType.SetGatePermission:
			title = "Granted";
			return true;
		case StoryEventType.SetConcealed:
			title = "Concealed";
			return true;
		case StoryEventType.SetHiddenOnMap:
			title = "Hidden";
			return true;
		case StoryEventType.StartSimulatingSurvivalFactors:
			title = "On Discovered";
			title2 = "On Join Player";
			return true;
		case StoryEventType.SetGraveDontDeteriorate:
			title = "Don't Deteriorate";
			return true;
		case StoryEventType.Drop:
			title = "Rescued By Player";
			return true;
		case StoryEventType.SetSkill:
			title = "Ignore Cap";
			return true;
		case StoryEventType.SetPersonality:
			title = "On";
			return true;
		case StoryEventType.SetPlayDead:
			title = "Play Dead";
			return true;
		case StoryEventType.DeclareWar:
			title = "No Notification";
			return true;
		case StoryEventType.CeaseFire:
			title = "No Notification";
			title2 = "Only If Alone";
			return true;
		case StoryEventType.SetDontBury:
			title = "Don't Bury";
			return true;
		case StoryEventType.SetDontLeaveCommunity:
		case StoryEventType.SetDontAbandonPlayer:
			title = "Don't Leave";
			return true;
		case StoryEventType.MemorableEvent:
			title = "Lie";
			return true;
		case StoryEventType.SetFollowerCommandsEnabled:
		case StoryEventType.SetRoadEntranceEnabled:
		case StoryEventType.SetHitTheRoadEnabled:
			title = "Enabled";
			return true;
		case StoryEventType.Attack:
			title = "Even if not enemy";
			return true;
		case StoryEventType.SellObject:
		case StoryEventType.SellItem:
			title = "Ignore Weight";
			title2 = "Spawn Gold";
			return true;
		case StoryEventType.GiveObject:
			title = "Ignore Weight";
			return true;
		case StoryEventType.Explode:
			title = "Don't Hurt Triggerer";
			return true;
		case StoryEventType.SetPlayerSurrenderDisabled:
			title = "Player Can't Surrender To Me";
			return true;
		case StoryEventType.SetIgnoreForQuests:
			title = "Ignore for Quests";
			return true;
		case StoryEventType.SetGod:
			title = "God";
			return true;
		case StoryEventType.SetInvulnerable:
			title = "Invulnerable";
			return true;
		case StoryEventType.SetBoxer:
			title = "Boxer";
			return true;
		default:
			return false;
		}
	}

	public static bool HasSecrecy(StoryEventType type)
	{
		switch (type)
		{
		case StoryEventType.MemorableEvent:
		case StoryEventType.JoinCommunity:
		case StoryEventType.LeaveCommunity:
		case StoryEventType.RejoinInitialCommunity:
		case StoryEventType.DeclareWar:
		case StoryEventType.Assassinate:
		case StoryEventType.Explode:
			return true;
		default:
			return false;
		}
	}

	public static bool HasSpeeches(StoryEventType eventType)
	{
		if (eventType == StoryEventType.Speak)
		{
			return true;
		}
		return false;
	}

	private T GetSubject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	private T GetObject<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	private T GetThirdParty<T>(Character actor, Character target, BaseObject obj, MemoryParam param) where T : BaseObject
	{
		return Condition.ResolveSpecifier<T>(ThirdParty, ThirdPartyModifier, ThirdPartyID, actor, target, obj, param);
	}

	private Community GetSubjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierCommunity(Subject, SubjectModifier, SubjectID, actor, target, obj, param);
	}

	private Community GetObjectCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierCommunity(Object, ObjectModifier, ObjectID, actor, target, obj, param);
	}

	private Community GetThirdPartyCommunity(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		return Condition.ResolveSpecifierCommunity(ThirdParty, ThirdPartyModifier, ThirdPartyID, actor, target, obj, param);
	}

	public void TriggerEvent(Character actor, Character target, BaseObject obj, MemoryParam param)
	{
		switch (Type)
		{
		case StoryEventType.DiscoverCharacterName:
		{
			Character subject88 = GetSubject<Character>(actor, target, obj, param);
			Character character50 = GetObject<Character>(actor, target, obj, param);
			if (character50 != null && subject88 != null && subject88.CanDiscoverStuffFromSpeech())
			{
				character50.NameKnown = true;
				if (character50.Community != null && !character50.Community.IsAmbientCommunity() && character50.Community.GetLivingNonZombieMemberCount() == 1)
				{
					character50.Community.CommunityNameKnown = true;
				}
				StoryManager.Instance.SetConditionsDirty();
			}
			break;
		}
		case StoryEventType.DiscoverCommunityName:
		{
			Character subject6 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity2 = GetObjectCommunity(actor, target, obj, param);
			if (objectCommunity2 != null && subject6 != null && subject6.CanDiscoverStuffFromSpeech())
			{
				objectCommunity2.CommunityNameKnown = true;
				StoryManager.Instance.SetConditionsDirty();
			}
			break;
		}
		case StoryEventType.DiscoverNamesOfCommunityMembersWithRole:
		{
			Community subjectCommunity = GetSubjectCommunity(actor, target, obj, param);
			int num = Array.IndexOf(Character.RoleNames, StringData);
			if (num == -1 || subjectCommunity == null)
			{
				break;
			}
			{
				foreach (Character member in subjectCommunity.Members)
				{
					if (member.HasRole((Role)num) && member.AliveAndNotZombie)
					{
						member.NameKnown = true;
						StoryManager.Instance.SetConditionsDirty();
					}
				}
				break;
			}
		}
		case StoryEventType.DiscoverCharacterBackground:
		{
			Character subject40 = GetSubject<Character>(actor, target, obj, param);
			if (subject40 != null)
			{
				subject40.BackgroundKnown = true;
				StoryManager.Instance.SetConditionsDirty();
			}
			break;
		}
		case StoryEventType.DiscoverRelationship:
		{
			Character subject53 = GetSubject<Character>(actor, target, obj, param);
			Character character36 = GetObject<Character>(actor, target, obj, param);
			if (subject53 != null && character36 != null)
			{
				Relationship.SetKnownToPlayer(subject53, character36);
			}
			break;
		}
		case StoryEventType.DiscoverRelationshipIfPlayer:
		{
			Character subject8 = GetSubject<Character>(actor, target, obj, param);
			Character character3 = GetObject<Character>(actor, target, obj, param);
			Character thirdParty2 = GetThirdParty<Character>(actor, target, obj, param);
			if (subject8 != null && character3 != null && thirdParty2 != null && subject8.CanDiscoverStuffFromSpeech())
			{
				Relationship.SetKnownToPlayer(character3, thirdParty2);
			}
			break;
		}
		case StoryEventType.DiscoverQuest:
		{
			Character subject96 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty16 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob6 = GetObject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.DiscoverQuest(StringData, subject96, thirdParty16, ob6, BoolData);
			break;
		}
		case StoryEventType.CompleteQuest:
		{
			Character subject98 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty17 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob7 = GetObject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.CompleteQuest(StringData, subject98, thirdParty17, ob7, BoolData);
			break;
		}
		case StoryEventType.CompleteQuestIfItIsDiscovered:
		{
			Character subject67 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty9 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob5 = GetObject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.CompleteQuestIfItIsDiscovered(StringData, subject67, thirdParty9, ob5, BoolData);
			break;
		}
		case StoryEventType.CompleteQuestGroup:
		{
			Character subject20 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty4 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject baseObject = GetObject<BaseObject>(actor, target, obj, param);
			List<QuestInstance> list = new List<QuestInstance>();
			foreach (KeyValuePair<int, QuestInstance> questInstance2 in StoryManager.Instance.QuestInstances)
			{
				if (questInstance2.Value.Quest.GroupID == StringData && questInstance2.Value.QuestGiver == subject20 && questInstance2.Value.QuestSeeker == thirdParty4 && questInstance2.Value.QuestObject == baseObject && questInstance2.Value.State == QuestInstance.EState.Active)
				{
					list.Add(questInstance2.Value);
				}
			}
			list.Sort((QuestInstance a, QuestInstance b) => a.Hash.CompareTo(b.Hash));
			{
				foreach (QuestInstance item2 in list)
				{
					item2.Complete(BoolData);
				}
				break;
			}
		}
		case StoryEventType.FailQuest:
		{
			Character subject37 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty5 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob3 = GetObject<BaseObject>(actor, target, obj, param);
			Quest quest2 = GameImpl.Instance.FindQuestByUniqueID(StringData);
			if (quest2 != null)
			{
				StoryManager.Instance.GetOrCreateQuestInstance(quest2, subject37, thirdParty5, ob3, canSkip: false).Fail(BoolData);
			}
			break;
		}
		case StoryEventType.FailQuestIfItIsDiscovered:
		{
			Character subject7 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject ob = GetObject<BaseObject>(actor, target, obj, param);
			Quest quest = GameImpl.Instance.FindQuestByUniqueID(StringData);
			if (quest != null)
			{
				StoryManager.Instance.GetQuestInstance(quest, subject7, thirdParty, ob)?.Fail(BoolData);
			}
			break;
		}
		case StoryEventType.FailQuestGroup:
		{
			Character subject70 = GetSubject<Character>(actor, target, obj, param);
			Character thirdParty10 = GetThirdParty<Character>(actor, target, obj, param);
			BaseObject baseObject2 = GetObject<BaseObject>(actor, target, obj, param);
			List<QuestInstance> list5 = new List<QuestInstance>();
			foreach (KeyValuePair<int, QuestInstance> questInstance3 in StoryManager.Instance.QuestInstances)
			{
				if (questInstance3.Value.Quest.GroupID == StringData && questInstance3.Value.QuestGiver == subject70 && questInstance3.Value.QuestSeeker == thirdParty10 && questInstance3.Value.QuestObject == baseObject2 && questInstance3.Value.State == QuestInstance.EState.Active)
				{
					list5.Add(questInstance3.Value);
				}
			}
			list5.Sort((QuestInstance a, QuestInstance b) => a.Hash.CompareTo(b.Hash));
			{
				foreach (QuestInstance item3 in list5)
				{
					item3.Fail(BoolData);
				}
				break;
			}
		}
		case StoryEventType.SetQuestParam:
		{
			Character subject54 = GetSubject<Character>(actor, target, obj, param);
			Character seeker = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty6 = GetThirdParty<BaseObject>(actor, target, obj, param);
			Quest quest3 = GameImpl.Instance.FindQuestByUniqueID(StringData);
			if (quest3 != null)
			{
				QuestInstance questInstance = StoryManager.Instance.FindQuestInstanceGivenByWithSeeker(quest3, subject54, seeker);
				if (questInstance != null)
				{
					questInstance.QuestParam = new MemoryParam(thirdParty6);
					StoryManager.Instance.RefreshQuestMarkers();
				}
			}
			break;
		}
		case StoryEventType.SpawnEquipment:
		{
			TileObject subject15 = GetSubject<TileObject>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype4 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject15 != null && equipmentPrototype4 != null && subject15.GetInventory() != null)
			{
				Equipment equipment = Equipment.Spawn(equipmentPrototype4);
				equipment.SetNewAmount(Math.Max(1, (int)Data));
				equipment.Init();
				NotificationManager.Instance.AddEquipmentNotification(null, subject15, equipment, equipment.GetAmount());
				equipment = subject15.GetInventory().Add(subject15, equipment);
				StoryManager.Instance.OneShotTrigger(StringData2, actor, target, equipment, default(MemoryParam));
			}
			break;
		}
		case StoryEventType.DeleteEquipment:
		{
			TileObject subject116 = GetSubject<TileObject>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype8 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject116 != null && equipmentPrototype8 != null && subject116.GetInventory() != null)
			{
				Equipment equipment13 = subject116.GetInventory().FindItemOfType(equipmentPrototype8);
				if (equipment13 != null)
				{
					int amount8 = Math.Min(equipment13.GetAmount(), (int)Data);
					subject116.GetInventory().UseItem(subject116, equipment13, amount8);
				}
			}
			break;
		}
		case StoryEventType.GiveEquipment:
		{
			Character character28 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype6 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (character28 == null || equipmentPrototype6 == null)
			{
				break;
			}
			Character subject41 = GetSubject<Character>(actor, target, obj, param);
			if (subject41 != null)
			{
				Equipment equipment5 = subject41.Inventory.FindItemOfType(equipmentPrototype6);
				if (equipment5 != null)
				{
					int amount = Math.Min(equipment5.GetAmount(), (int)Data);
					equipment5 = subject41.Inventory.Take(subject41, equipment5, amount);
					equipment5.Gifted = BoolData;
					NotificationManager.Instance.AddEquipmentNotification(subject41, character28, equipment5, amount);
					equipment5 = character28.Inventory.Add(character28, equipment5);
				}
				break;
			}
			Community subjectCommunity13 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity13 == null)
			{
				break;
			}
			int num8 = (int)Data;
			{
				foreach (Character member2 in subjectCommunity13.Members)
				{
					if (!member2.AliveAndNotZombie)
					{
						continue;
					}
					Equipment equipment6 = member2.Inventory.FindItemOfType(equipmentPrototype6);
					if (equipment6 != null)
					{
						int amount2 = Math.Min(equipment6.GetAmount(), num8);
						equipment6 = member2.Inventory.Take(member2, equipment6, amount2);
						equipment6.Gifted = BoolData;
						NotificationManager.Instance.AddEquipmentNotification(member2, character28, equipment6, amount2);
						num8 -= equipment6.GetAmount();
						equipment6 = character28.Inventory.Add(character28, equipment6);
						if (num8 <= 0)
						{
							break;
						}
					}
				}
				break;
			}
		}
		case StoryEventType.GiveItemWithLiquid:
		{
			Character subject19 = GetSubject<Character>(actor, target, obj, param);
			Character character11 = GetObject<Character>(actor, target, obj, param);
			LiquidPrototype liquidPrototype = GameImpl.Instance.FindLiquidPrototypeByName(StringData);
			if (subject19 != null && character11 != null && liquidPrototype != null)
			{
				Equipment equipment2 = subject19.Inventory.FindBestItemWithLiquid(liquidPrototype);
				if (equipment2 != null)
				{
					equipment2 = subject19.Inventory.Take(subject19, equipment2, 1);
					NotificationManager.Instance.AddEquipmentNotification(subject19, character11, equipment2, equipment2.GetAmount());
					equipment2 = character11.Inventory.Add(character11, equipment2);
				}
			}
			break;
		}
		case StoryEventType.GiveObject:
		{
			Character subject42 = GetSubject<Character>(actor, target, obj, param);
			Character character29 = GetObject<Character>(actor, target, obj, param);
			Equipment equipment7 = GetThirdParty<Equipment>(actor, target, obj, param);
			if (equipment7 == null)
			{
				equipment7 = obj as Equipment;
			}
			if (subject42 != null && character29 != null && equipment7 != null && subject42.Inventory.Contains(equipment7))
			{
				int num9 = equipment7.GetAmount();
				if (!BoolData)
				{
					num9 = Math.Min(num9, (int)(character29.GetAvailableInventorySpace() / equipment7.GetWeight()));
				}
				if (param.IsInt())
				{
					num9 = Math.Min(num9, param.GetInt());
				}
				int amount3 = Math.Max(1, num9);
				equipment7 = subject42.Inventory.Take(subject42, equipment7, amount3);
				NotificationManager.Instance.AddEquipmentNotification(subject42, character29, equipment7, equipment7.GetAmount());
				equipment7 = character29.Inventory.Add(character29, equipment7);
			}
			break;
		}
		case StoryEventType.Sell:
		{
			Character subject110 = GetSubject<Character>(actor, target, obj, param);
			Character character55 = GetObject<Character>(actor, target, obj, param);
			if (subject110 == null || character55 == null || !subject110.AliveAndNotZombie || !character55.AliveAndNotZombie)
			{
				break;
			}
			Equipment gold5 = character55.Inventory.GetGold();
			if (gold5 != null)
			{
				float num24 = Data;
				if (Formulas != null && Formulas.Count > 0 && Formulas[0].GetConditionBlock() != null)
				{
					num24 += Formulas[0].GetConditionBlock().Evaluate(subject110, character55, obj, param);
				}
				int amount7 = Math.Max(1, Mathf.CeilToInt(subject110.GetPriceToSell(num24, character55)));
				gold5 = character55.Inventory.Take(character55, gold5, amount7);
				amount7 = gold5.GetAmount();
				NotificationManager.Instance.AddEquipmentNotification(character55, subject110, gold5, amount7);
				gold5 = subject110.Inventory.Add(subject110, gold5);
				if (character55.IsControllableByPlayer() && MemoryPrototype.BoughtFrom != null)
				{
					float quantityFactor = Math.Max(0f, amount7);
					Memory.OnMemorableEvent(MemoryPrototype.BoughtFrom, character55, subject110, quantityFactor, secret: false);
				}
				if (subject110.IsControllableByPlayer() && MemoryPrototype.SoldTo != null)
				{
					float quantityFactor2 = Math.Max(0f, amount7);
					Memory.OnMemorableEvent(MemoryPrototype.SoldTo, subject110, character55, quantityFactor2, secret: false);
				}
			}
			break;
		}
		case StoryEventType.SellObject:
		{
			Character subject24 = GetSubject<Character>(actor, target, obj, param);
			Character character14 = GetObject<Character>(actor, target, obj, param);
			Equipment equipment3 = obj as Equipment;
			if (subject24 != null && character14 != null && equipment3 != null && subject24.AliveAndNotZombie && character14.AliveAndNotZombie)
			{
				int num3 = equipment3.GetAmount();
				if (!BoolData)
				{
					num3 = Math.Min(num3, Math.Max(1, (int)(character14.GetAvailableInventorySpace() / equipment3.GetWeight())));
				}
				float priceToSell = subject24.GetPriceToSell(equipment3, character14);
				if (BoolData2)
				{
					Equipment equipment4 = Equipment.Spawn(EquipmentPrototype.Gold, Mathf.CeilToInt(priceToSell));
					character14.Inventory.Add(character14, equipment4);
				}
				else
				{
					int goldAmount = character14.Inventory.GetGoldAmount();
					num3 = Math.Min(num3, (int)((float)goldAmount / priceToSell));
				}
				if (num3 > 0)
				{
					Session.Instance.EquipmentTrade(equipment3, subject24, character14, num3, null, fromTradingScreen: false);
				}
			}
			break;
		}
		case StoryEventType.SellItem:
		{
			Character subject81 = GetSubject<Character>(actor, target, obj, param);
			Character character48 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype prototype2 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			Equipment equipment10 = subject81.Inventory.FindItemOfType(prototype2);
			if (subject81 != null && character48 != null && equipment10 != null && subject81.AliveAndNotZombie && character48.AliveAndNotZombie)
			{
				int num19 = equipment10.GetAmount();
				if (!BoolData)
				{
					num19 = Math.Min(num19, Math.Max(1, (int)(character48.GetAvailableInventorySpace() / equipment10.GetWeight())));
				}
				float priceToSell2 = subject81.GetPriceToSell(equipment10, character48);
				if (BoolData2)
				{
					Equipment equipment11 = Equipment.Spawn(EquipmentPrototype.Gold, Mathf.CeilToInt(priceToSell2));
					character48.Inventory.Add(character48, equipment11);
				}
				else
				{
					int goldAmount2 = character48.Inventory.GetGoldAmount();
					num19 = Math.Min(num19, (int)((float)goldAmount2 / priceToSell2));
				}
				if (num19 > 0)
				{
					Session.Instance.EquipmentTrade(equipment10, subject81, character48, num19, null, fromTradingScreen: false);
				}
			}
			break;
		}
		case StoryEventType.RefundItem:
		{
			Character subject61 = GetSubject<Character>(actor, target, obj, param);
			Character character40 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype prototype = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			Equipment equipment9 = character40.Inventory.FindItemOfType(prototype);
			Equipment gold2 = subject61.Inventory.GetGold();
			if (gold2 != null && equipment9 != null)
			{
				float num14 = equipment9.GetBasePrice() * Character.BaseMarkup;
				int val = Math.Min(equipment9.GetAmount(), Math.Max(1, (int)(character40.GetAvailableInventorySpace() / equipment9.GetWeight())));
				val = Math.Min(val, Math.Max(1, (int)((float)gold2.GetAmount() / num14)));
				if (val > 0)
				{
					int amount4 = Math.Max(1, (int)(character40.IsControllableByPlayer() ? Math.Floor(num14 * (float)val) : Math.Ceiling(num14 * (float)val)));
					equipment9 = character40.Inventory.Take(character40, equipment9, val);
					gold2 = subject61.Inventory.Take(subject61, gold2, amount4);
					NotificationManager.Instance.AddEquipmentNotification(character40, subject61, equipment9, val);
					NotificationManager.Instance.AddEquipmentNotification(subject61, character40, gold2, gold2.GetAmount());
					character40.Inventory.Add(character40, gold2);
					subject61.Inventory.Add(subject61, equipment9);
				}
			}
			break;
		}
		case StoryEventType.DeleteGameObject:
			BaseObjectManager.Instance.GetObjectByUniqueID(SubjectID)?.Delete();
			break;
		case StoryEventType.SetVariable:
		{
			BaseObject subject43 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob4 = GetObject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.SetVariable(Variable.Create(StringData, subject43, ob4, Data));
			break;
		}
		case StoryEventType.IncrementVariable:
		{
			BaseObject subject30 = GetSubject<BaseObject>(actor, target, obj, param);
			BaseObject ob2 = GetObject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.IncrementVariable(StringData, subject30, ob2, Data, Data2);
			break;
		}
		case StoryEventType.OpenTradeScreen:
		{
			Character subject18 = GetSubject<Character>(actor, target, obj, param);
			Character character10 = GetObject<Character>(actor, target, obj, param);
			if (subject18 != null && character10 != null && subject18.AliveAndNotZombie && character10.AliveAndNotZombie)
			{
				PlayerRecord playerControllingCharacter = Session.Instance.GetPlayerControllingCharacter(character10);
				if (playerControllingCharacter != null)
				{
					TakeUIGoal.StartSwappingSupplies(playerControllingCharacter, character10, subject18, SwappingSuppliesMode.Trading);
				}
			}
			break;
		}
		case StoryEventType.OpenSwapSuppliesScreen:
		{
			Character subject109 = GetSubject<Character>(actor, target, obj, param);
			Character character54 = GetObject<Character>(actor, target, obj, param);
			int mode = Array.IndexOf(TakeUIGoal.SwappingSuppliesModeNames, StringData);
			if (subject109 != null && character54 != null && subject109.AliveAndNotZombie && character54.AliveAndNotZombie)
			{
				PlayerRecord playerControllingCharacter5 = Session.Instance.GetPlayerControllingCharacter(subject109);
				if (playerControllingCharacter5 != null)
				{
					TakeUIGoal.StartSwappingSupplies(playerControllingCharacter5, subject109, character54, (SwappingSuppliesMode)mode);
				}
			}
			break;
		}
		case StoryEventType.DramaticDeath:
		{
			Character subject104 = GetSubject<Character>(actor, target, obj, param);
			if (subject104 != null)
			{
				if (subject104.Alive)
				{
					subject104.OnDie(0f, Vector3.zero, Vector3.zero, Bone.Spine, Vector3.zero, CauseOfDeath.Other, null, SecrecyMode.Public);
					Memory.OnMemorableEvent(MemoryPrototype.Died, null, subject104, 1f, secret: false);
				}
				StoryManager.Instance.DramaticDeathCharacter = subject104;
			}
			break;
		}
		case StoryEventType.WakeUpZombie:
		{
			Character subject60 = GetSubject<Character>(actor, target, obj, param);
			if (subject60 != null && subject60.Alive && subject60.FindActiveGoal(GoalType.PlayDead, out var parent) is PlayDead playDead)
			{
				playDead.WakeUp(subject60, parent);
			}
			break;
		}
		case StoryEventType.ActivateInvisibleStrain:
		{
			Character subject56 = GetSubject<Character>(actor, target, obj, param);
			if (subject56 != null)
			{
				if (subject56.AliveAndNotZombie && subject56.InvisibleStrain != InvisibleStrainType.None)
				{
					subject56.ActivateInvisibleStrain();
				}
				break;
			}
			Community subjectCommunity15 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity15 == null)
			{
				break;
			}
			{
				foreach (Character member3 in subjectCommunity15.Members)
				{
					if (member3.AliveAndNotZombie && member3.InvisibleStrain != InvisibleStrainType.None)
					{
						member3.ActivateInvisibleStrain();
					}
				}
				break;
			}
		}
		case StoryEventType.Doom:
		{
			Session instance3 = Session.Instance;
			List<Community> list2 = new List<Community>();
			foreach (Community community9 in instance3.CommunityManager.Communities)
			{
				if (!community9.IsAnimalCommunity() && !community9.IsZombieCommunity())
				{
					list2.Add(community9);
				}
			}
			Character subject34 = GetSubject<Character>(actor, target, obj, param);
			Community community5 = Community.Spawn(CommunityType.Psycho);
			community5.SetUniqueID("Turned");
			community5.CommunityName.SetTranslatedString(Community.COMMUNITY_Turned);
			community5.PlayerSurrenderDisabled = true;
			bool flag3 = false;
			bool flag4 = false;
			foreach (Community item4 in list2)
			{
				Squad squad3 = null;
				List<ObjScore> list3 = new List<ObjScore>();
				foreach (Character member4 in item4.Members)
				{
					if (member4.WillActivateDoomStrain())
					{
						float sqrMagnitude = (member4.PosXZ - subject34.PosXZ).sqrMagnitude;
						list3.Add(new ObjScore(member4, sqrMagnitude));
					}
					else if (member4.AliveAndNotZombie && member4.GetBaseObjectType() == BaseObjectType.Human)
					{
						member4.EncouragementTimeout = MathUtil.Max(member4.EncouragementTimeout, Session.Instance.PlayTime + Sun.DayLength);
					}
				}
				list3.Sort();
				for (int num6 = 0; num6 < list3.Count; num6++)
				{
					Character character23 = (Character)list3[num6].Obj;
					if (character23.Community != null)
					{
						flag3 |= character23.Community.WarnedAboutTraps;
						flag4 |= character23.Community.WarnedAboutTripwires;
					}
					Memory.OnMemorableEvent(MemoryPrototype.HasInvisibleStrain, character23, null, 1f, SecrecyMode.OnlyKnownToSubjectCommunity);
					character23.InvisibleStrain = InvisibleStrainType.Subtle;
					character23.SetCommunity(community5);
					character23.SetPersonality(CachedPersonalityType.Nervous, val: false);
					character23.SetPersonality(CachedPersonalityType.Bold, val: true);
					if (MathUtil.RandomChoice((float)(character23.Id ^ instance3.GameUniqueId) + 1234f, 0.5f))
					{
						character23.ActivateInvisibleStrain();
					}
					character23.GetOrCreateTarget(subject34).ForceVisible(character23);
					if (character23.Zombie)
					{
						character23.SetLeaderCommand(new FeedOnLiving(character23, subject34), subject34, ObeyLeaderGoal.SourceType.Scripted);
						continue;
					}
					character23.SetLeaderCommand(new Attack(character23, subject34, dontOpenOurGates: false, StayInRangeParams.OfSquadLeader()), subject34, ObeyLeaderGoal.SourceType.Scripted);
					if (squad3 == null)
					{
						squad3 = community5.AddSquad(SquadBehaviour.Hunt, instance3.CommunityManager.PlayerCommunity.Id);
					}
					community5.AddToSquad(character23, squad3);
				}
				for (int num7 = 0; num7 < list3.Count; num7++)
				{
					((Character)list3[num7].Obj).DeleteAllMemoriesWithProto(MemoryPrototype.HasInvisibleStrain);
				}
				if (squad3 != null && !community5.StartPillaging(squad3, canAttackOutsideBase: true))
				{
					community5.RemoveSquad(squad3);
				}
			}
			community5.WarnAboutTraps(flag3, flag4);
			break;
		}
		case StoryEventType.Speak:
		{
			Character character4 = GetSubject<Character>(actor, target, obj, param);
			if (character4 == null)
			{
				character4 = GetSubjectCommunity(actor, target, obj, param)?.Leader;
			}
			Character character5 = GetObject<Character>(actor, target, obj, param);
			BaseObject resultObj = GetThirdParty<BaseObject>(actor, target, obj, param);
			if (character4 != null && character4 != character5)
			{
				MemoryParam param2 = default(MemoryParam);
				Speech speech = StoryManager.Instance.PickSpeech(character4, character5, resultObj, Speeches, out resultObj, ref param2, Session.Instance.DeterministicRand);
				if (speech != null && (character5 == null || character5.AliveAndNotZombie || speech.ListenerCanBeDead()))
				{
					character4.PushQueuedSpeech(speech, character5, resultObj, param2);
				}
			}
			break;
		}
		case StoryEventType.RememberSpokenSpeech:
		{
			Character subject122 = GetSubject<Character>(actor, target, obj, param);
			Character target4 = GetObject<Character>(actor, target, obj, param);
			Speech speech2 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (subject122 != null && speech2 != null)
			{
				subject122.RememberSpokenSpeech(speech2, target4, force: true);
			}
			break;
		}
		case StoryEventType.RememberSpokenOnce:
		{
			Speech speech3 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
			if (speech3 != null)
			{
				StoryManager.Instance.RememberSpokenOnce(speech3);
			}
			break;
		}
		case StoryEventType.MemorableEvent:
		{
			Character subject102 = GetSubject<Character>(actor, target, obj, param);
			BaseObject ob8 = GetObject<BaseObject>(actor, target, obj, param);
			BaseObject thirdParty18 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype2 = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (memoryPrototype2 != null)
			{
				Memory.OnMemorableEvent(memoryPrototype2, subject102, ob8, thirdParty18, (Data != 0f) ? Data : 1f, Secrecy, null, BoolData);
			}
			break;
		}
		case StoryEventType.DeleteMemory:
		{
			Character subject74 = GetSubject<Character>(actor, target, obj, param);
			Character character46 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty11 = GetThirdParty<BaseObject>(actor, target, obj, param);
			MemoryPrototype memoryPrototype = GameImpl.Instance.FindMemoryPrototypeByUniqueID(StringData);
			if (subject74 != null && character46 != null && memoryPrototype != null)
			{
				subject74.DeleteMemory(memoryPrototype, character46, thirdParty11);
			}
			break;
		}
		case StoryEventType.JoinCommunity:
		{
			Character subject45 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity8 = GetObjectCommunity(actor, target, obj, param);
			if (subject45 != null && objectCommunity8 != null && !subject45.IsPlayerAvatar())
			{
				Community community6 = subject45.Community;
				objectCommunity8.AddMemberWithNotifications(subject45, Secrecy, BoolData, BoolData2);
				if (community6 == null || !BoolData3)
				{
					break;
				}
				Character character30 = GetObject<Character>(actor, target, obj, param);
				for (int num10 = community6.Members.Count - 1; num10 >= 0; num10--)
				{
					if (community6.Members[num10].AliveAndNotZombie && !community6.Members[num10].WantLeaveCommunity(objectCommunity8, checkRelationships: false))
					{
						Character character31 = community6.Members[num10];
						objectCommunity8.AddMemberWithNotifications(character31, Secrecy, BoolData, BoolData2);
						if (character30 != null)
						{
							character31.Follow(character30);
						}
					}
				}
			}
			else
			{
				TileObject subject46 = GetSubject<TileObject>(actor, target, obj, param);
				if (subject46 != null && objectCommunity8 != null)
				{
					subject46.SetCommunity(objectCommunity8);
				}
			}
			break;
		}
		case StoryEventType.LeaveCommunity:
		case StoryEventType.RejoinInitialCommunity:
		{
			Character subject47 = GetSubject<Character>(actor, target, obj, param);
			Character kickedBy = GetObject<Character>(actor, target, obj, param);
			if (subject47 != null)
			{
				Community community7 = subject47.Community;
				subject47.LeaveCommunity(Type == StoryEventType.RejoinInitialCommunity, Secrecy, BoolData, BoolData2, kickedBy);
				if (community7 == null || subject47.Community == null || !BoolData3)
				{
					break;
				}
				for (int num11 = community7.Members.Count - 1; num11 >= 0; num11--)
				{
					Character character32 = community7.Members[num11];
					if (character32.AliveAndNotZombie && character32.HasGoodRelationship(subject47) && subject47.HasGoodRelationship(character32) && !character32.WantLeaveCommunity(subject47.Community, checkRelationships: false))
					{
						subject47.Community.AddMemberWithNotifications(character32, Secrecy, BoolData, BoolData2, kickedBy);
					}
				}
			}
			else
			{
				GetSubject<TileObject>(actor, target, obj, param)?.SetCommunity(null);
			}
			break;
		}
		case StoryEventType.AddToSquad:
		{
			Character subject27 = GetSubject<Character>(actor, target, obj, param);
			Character character17 = GetObject<Character>(actor, target, obj, param);
			if (subject27 != null && character17 != null && character17.Community != null)
			{
				Squad squad2 = character17.GetSquad();
				if (squad2 != null)
				{
					character17.Community.AddToSquad(subject27, squad2);
				}
			}
			break;
		}
		case StoryEventType.StopFollowing:
			GetSubject<Character>(actor, target, obj, param)?.LeaveSquad();
			break;
		case StoryEventType.Follow:
		{
			Character subject127 = GetSubject<Character>(actor, target, obj, param);
			Character character60 = GetObject<Character>(actor, target, obj, param);
			if (subject127 != null && subject127.AliveAndNotZombie && (character60 == null || character60.AliveAndNotZombie))
			{
				subject127.Follow(character60);
			}
			break;
		}
		case StoryEventType.CommunityFollow:
		{
			Community subjectCommunity30 = GetSubjectCommunity(actor, target, obj, param);
			Character character58 = GetObject<Character>(actor, target, obj, param);
			int num32 = Array.IndexOf(Character.RoleNames, StringData);
			if (subjectCommunity30 == null || (character58 != null && !character58.AliveAndNotZombie))
			{
				break;
			}
			{
				foreach (Character member5 in subjectCommunity30.Members)
				{
					if (member5.AliveAndNotZombie && member5.GetBaseObjectType() == BaseObjectType.Human && member5.SquadId == 0 && member5 != character58 && (num32 <= 0 || member5.HasRole((Role)num32)))
					{
						member5.Follow(character58);
					}
				}
				break;
			}
		}
		case StoryEventType.Introduce:
		{
			Community subjectCommunity25 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity11 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity25 != null && objectCommunity11 != null && Session.Instance.CommunityManager.GetRelationship(subjectCommunity25, objectCommunity11) < CommunityRelationshipType.Known)
			{
				Session.Instance.CommunityManager.SetRelationship(subjectCommunity25, objectCommunity11, CommunityRelationshipType.Known);
			}
			break;
		}
		case StoryEventType.IntroductionFinished:
		{
			Community subjectCommunity21 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity10 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity21 != null && objectCommunity10 != null && Session.Instance.CommunityManager.GetRelationship(subjectCommunity21, objectCommunity10) == CommunityRelationshipType.Introducing)
			{
				Session.Instance.CommunityManager.SetRelationship(subjectCommunity21, objectCommunity10, CommunityRelationshipType.Unknown);
			}
			break;
		}
		case StoryEventType.Unintroduce:
		{
			Community subjectCommunity19 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity9 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity19 != null && objectCommunity9 != null)
			{
				Session.Instance.CommunityManager.SetRelationship(subjectCommunity19, objectCommunity9, CommunityRelationshipType.Unknown);
			}
			break;
		}
		case StoryEventType.DeclareWar:
		{
			Character character12 = GetSubject<Character>(actor, target, obj, param);
			Character character13 = GetObject<Character>(actor, target, obj, param);
			Community community3 = GetSubjectCommunity(actor, target, obj, param);
			Community community4 = GetObjectCommunity(actor, target, obj, param);
			if (community3 == null || community4 == null)
			{
				break;
			}
			if (character12 == null && community3.Leader != null && community3.Leader.AliveAndNotZombie)
			{
				character12 = community3.Leader;
			}
			if (community3 == community4)
			{
				if (character13 == null)
				{
					Debug.LogWarning("Community trying to declare war on itself " + community3.GetDisplayNameString());
					break;
				}
				if (character13.Rank == Rank.Leader)
				{
					character12.LeaveCommunity(rejoinInitialCommunity: true, SecrecyMode.Public, kicked: true, kickedDueToInfection: false);
					community3 = character12.Community;
				}
				else
				{
					character13.LeaveCommunity(rejoinInitialCommunity: true, SecrecyMode.Public, kicked: true, kickedDueToInfection: false);
					community4 = character13.Community;
				}
			}
			Session.Instance.CommunityManager.SetRelationship(community3, community4, CommunityRelationshipType.Hostile, !BoolData);
			if (character12 != null)
			{
				Memory.OnMemorableEvent(MemoryPrototype.DeclaredWar, character12, (character13 != null) ? ((BaseObject)character13) : ((BaseObject)community4), 1f, Secrecy);
			}
			if (community3.CommunityType == CommunityType.Player || community4.CommunityType == CommunityType.Player)
			{
				HintManager.Instance.LastWarDeclaredTime = Session.Instance.PlayTime;
			}
			break;
		}
		case StoryEventType.ForgetPropertyDamage:
			GetSubjectCommunity(actor, target, obj, param)?.ForgetPropertyDamage();
			break;
		case StoryEventType.CeaseFire:
		{
			Community subjectCommunity6 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity3 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity6 == null || objectCommunity3 == null)
			{
				break;
			}
			Community community = subjectCommunity6;
			Community community2 = objectCommunity3;
			if (BoolData2 && community.GetActiveMemberCount() > 1)
			{
				break;
			}
			Session.Instance.CommunityManager.SetRelationship(subjectCommunity6, objectCommunity3, CommunityRelationshipType.Ceasefire, !BoolData);
			if (community2.CommunityType == CommunityType.Player)
			{
				foreach (Character character61 in Session.Instance.CharacterManager.Characters)
				{
					if (character61.CanFollowPlayer && character61.Community != null && character61.Community != community && character61.AliveAndNotZombie && Session.Instance.CommunityManager.GetRelationship(community, character61.Community) == CommunityRelationshipType.Hostile)
					{
						Session.Instance.CommunityManager.SetRelationship(community, character61.Community, CommunityRelationshipType.Ceasefire, !BoolData);
					}
				}
			}
			{
				foreach (Community cachedAlly in community2.CachedAllies)
				{
					if (cachedAlly.GetRelationship(community) == CommunityRelationshipType.Hostile)
					{
						Session.Instance.CommunityManager.SetRelationship(community, cachedAlly, CommunityRelationshipType.Ceasefire, !BoolData);
					}
				}
				break;
			}
		}
		case StoryEventType.FormAlliance:
		{
			Community subjectCommunity31 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity14 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity31 == null || objectCommunity14 == null)
			{
				break;
			}
			foreach (Community community10 in Session.Instance.CommunityManager.Communities)
			{
				if (community10.IsFEMA && subjectCommunity31.GetRelationship(community10) == CommunityRelationshipType.Hostile && objectCommunity14.GetRelationship(community10) != CommunityRelationshipType.Hostile)
				{
					Session.Instance.CommunityManager.SetRelationship(subjectCommunity31, community10, CommunityRelationshipType.Known);
				}
			}
			Session.Instance.CommunityManager.SetRelationship(subjectCommunity31, objectCommunity14, CommunityRelationshipType.Allied);
			break;
		}
		case StoryEventType.LeaveAlliance:
		{
			Character subject126 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity13 = GetObjectCommunity(actor, target, obj, param);
			if (subject126 != null && subject126.Community != null && objectCommunity13 != null)
			{
				Session.Instance.CommunityManager.SetRelationship(subject126.Community, objectCommunity13, CommunityRelationshipType.Known);
				Memory.OnMemorableEvent(MemoryPrototype.BrokeAlliance, subject126, objectCommunity13, 1f, secret: false);
			}
			break;
		}
		case StoryEventType.LeaveAllAlliances:
		{
			Character subject119 = GetSubject<Character>(actor, target, obj, param);
			if (subject119 != null && subject119.Community != null)
			{
				while (subject119.Community.CachedAllies.Count > 0)
				{
					Community community8 = subject119.Community.CachedAllies[0];
					Session.Instance.CommunityManager.SetRelationship(subject119.Community, community8, CommunityRelationshipType.Known);
					Memory.OnMemorableEvent(MemoryPrototype.BrokeAlliance, subject119, community8, 1f, secret: false);
				}
			}
			break;
		}
		case StoryEventType.RegisterAttack:
		{
			Character subject118 = GetSubject<Character>(actor, target, obj, param);
			Character character57 = GetObject<Character>(actor, target, obj, param);
			if (subject118 != null && character57 != null)
			{
				subject118.GetOrCreateTarget(character57, subject118.Position).OnAttackedMe(character57, subject118.Position, forceVisible: false);
			}
			break;
		}
		case StoryEventType.SetCommunityInvasionTarget:
		{
			Community subjectCommunity27 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity12 = GetObjectCommunity(actor, target, obj, param);
			subjectCommunity27?.SetInvasionTarget(objectCommunity12, Data, fromScript: true);
			break;
		}
		case StoryEventType.SellFood:
		{
			Character subject82 = GetSubject<Character>(actor, target, obj, param);
			Character character49 = GetObject<Character>(actor, target, obj, param);
			if (subject82 != null && character49 != null && subject82.AliveAndNotZombie && character49.AliveAndNotZombie)
			{
				Equipment gold4 = character49.Inventory.GetGold();
				Equipment food2 = subject82.Inventory.GetFood(subject82, character49, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: true);
				if (gold4 != null && food2 != null)
				{
					int amount6 = Math.Max(1, Mathf.CeilToInt(subject82.GetPriceToSell(food2, character49)));
					NotificationManager.Instance.AddEquipmentNotification(character49, subject82, gold4, amount6);
					NotificationManager.Instance.AddEquipmentNotification(subject82, character49, food2, 1);
					subject82.Inventory.Add(subject82, character49.Inventory.Take(character49, gold4, amount6));
					character49.Inventory.Add(character49, subject82.Inventory.Take(subject82, food2, 1));
				}
			}
			break;
		}
		case StoryEventType.SellDrink:
		{
			Character subject64 = GetSubject<Character>(actor, target, obj, param);
			Character character41 = GetObject<Character>(actor, target, obj, param);
			if (subject64 != null && character41 != null && subject64.AliveAndNotZombie && character41.AliveAndNotZombie)
			{
				Equipment gold3 = character41.Inventory.GetGold();
				Equipment bestWaterBottleToDrink2 = subject64.Inventory.GetBestWaterBottleToDrink(subject64, character41, forSharing: true);
				if (gold3 != null && bestWaterBottleToDrink2 != null)
				{
					int amount5 = Math.Max(1, Mathf.CeilToInt(subject64.GetPriceToSell(bestWaterBottleToDrink2, character41)));
					NotificationManager.Instance.AddEquipmentNotification(character41, subject64, gold3, amount5);
					NotificationManager.Instance.AddEquipmentNotification(subject64, character41, bestWaterBottleToDrink2, 1);
					subject64.Inventory.Add(subject64, character41.Inventory.Take(character41, gold3, amount5));
					character41.Inventory.Add(character41, subject64.Inventory.Take(subject64, bestWaterBottleToDrink2, 1));
				}
			}
			break;
		}
		case StoryEventType.GiveFood:
		{
			Character subject50 = GetSubject<Character>(actor, target, obj, param);
			Character character33 = GetObject<Character>(actor, target, obj, param);
			if (subject50 != null && character33 != null && subject50.AliveAndNotZombie && character33.AliveAndNotZombie)
			{
				Equipment food = subject50.Inventory.GetFood(subject50, character33, includeGifts: false, ignoreIfUsingForCrafting: true, forSharing: true);
				if (food != null)
				{
					NotificationManager.Instance.AddEquipmentNotification(subject50, character33, food, 1);
					character33.Inventory.Add(character33, subject50.Inventory.Take(subject50, food, 1));
				}
			}
			break;
		}
		case StoryEventType.GiveDrink:
		{
			Character subject39 = GetSubject<Character>(actor, target, obj, param);
			Character character27 = GetObject<Character>(actor, target, obj, param);
			if (subject39 != null && character27 != null && subject39.AliveAndNotZombie && character27.AliveAndNotZombie)
			{
				Equipment bestWaterBottleToDrink = subject39.Inventory.GetBestWaterBottleToDrink(subject39, character27, forSharing: true);
				if (bestWaterBottleToDrink != null)
				{
					NotificationManager.Instance.AddEquipmentNotification(subject39, character27, bestWaterBottleToDrink, 1);
					character27.Inventory.Add(character27, subject39.Inventory.Take(subject39, bestWaterBottleToDrink, 1));
				}
			}
			break;
		}
		case StoryEventType.ApplyBandage:
		{
			Character subject31 = GetSubject<Character>(actor, target, obj, param);
			Character character19 = GetObject<Character>(actor, target, obj, param);
			if (subject31 != null && character19 != null)
			{
				subject31.SetLeaderCommand(new MoveToAndInteractGoal(subject31, character19, InteractionType.ApplyBandage, 0, null, null, null), character19, ObeyLeaderGoal.SourceType.Scripted);
				subject31.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.SellBandage:
		{
			Character subject35 = GetSubject<Character>(actor, target, obj, param);
			Character character24 = GetObject<Character>(actor, target, obj, param);
			if (subject35 != null && character24 != null)
			{
				int priceToBandage = subject35.GetPriceToBandage(character24);
				subject35.SetLeaderCommand(new MoveToAndInteractGoal(subject35, character24, InteractionType.ApplyBandage, priceToBandage, null, null, null), character24, ObeyLeaderGoal.SourceType.Scripted);
				subject35.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.GiveAntigen:
		{
			Character subject14 = GetSubject<Character>(actor, target, obj, param);
			Character character7 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype3 = null;
			if (string.IsNullOrEmpty(StringData))
			{
				if (subject14 != null && character7 != null)
				{
					InfectionType worstInfectionTypeInProgression = character7.GetWorstInfectionTypeInProgression();
					if (worstInfectionTypeInProgression != InfectionType.None)
					{
						Equipment antigen = subject14.Inventory.GetAntigen(worstInfectionTypeInProgression);
						if (antigen != null)
						{
							equipmentPrototype3 = antigen.GetPrototype();
						}
					}
				}
			}
			else
			{
				equipmentPrototype3 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			}
			if (subject14 != null && character7 != null && equipmentPrototype3 != null)
			{
				subject14.SetLeaderCommand(new MoveToAndInteractGoal(subject14, character7, InteractionType.GiveAntigen, 0, null, equipmentPrototype3, null), character7, ObeyLeaderGoal.SourceType.Scripted);
				subject14.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.SellAntigen:
		{
			Character subject125 = GetSubject<Character>(actor, target, obj, param);
			Character character59 = GetObject<Character>(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype9 = null;
			if (string.IsNullOrEmpty(StringData))
			{
				if (subject125 != null && character59 != null)
				{
					InfectionType worstInfectionTypeInProgression2 = character59.GetWorstInfectionTypeInProgression();
					if (worstInfectionTypeInProgression2 != InfectionType.None)
					{
						Equipment antigen2 = subject125.Inventory.GetAntigen(worstInfectionTypeInProgression2);
						if (antigen2 != null)
						{
							equipmentPrototype9 = antigen2.GetPrototype();
						}
					}
				}
			}
			else
			{
				equipmentPrototype9 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			}
			if (subject125 != null && character59 != null && equipmentPrototype9 != null)
			{
				Equipment antigen3 = subject125.Inventory.GetAntigen(equipmentPrototype9.AntigenType);
				if (antigen3 != null)
				{
					int price = Math.Max(1, Mathf.CeilToInt(subject125.GetPriceToSell(antigen3, character59)));
					subject125.SetLeaderCommand(new MoveToAndInteractGoal(subject125, character59, InteractionType.GiveAntigen, price, null, equipmentPrototype9, null), character59, ObeyLeaderGoal.SourceType.Scripted);
					subject125.DirectControlled = false;
				}
			}
			break;
		}
		case StoryEventType.GiveItemOfClothingType:
		{
			Character subject114 = GetSubject<Character>(actor, target, obj, param);
			Character character56 = GetObject<Character>(actor, target, obj, param);
			int num26 = Array.IndexOf(Character.ClothingTypeNames, StringData);
			if (subject114 != null && character56 != null && num26 >= 0)
			{
				Equipment equipment12 = subject114.Inventory.GetCheapestClothingOfType((ClothingType)num26);
				if (BoolData && subject114.Clothes[num26] != null)
				{
					equipment12 = subject114.Clothes[num26];
				}
				NotificationManager.Instance.AddEquipmentNotification(subject114, character56, equipment12, 1);
				subject114.Inventory.Remove(subject114, equipment12);
				character56.Inventory.Add(character56, equipment12);
			}
			break;
		}
		case StoryEventType.GiveAllGold:
		case StoryEventType.TakeAllGold:
		{
			Character subject73 = GetSubject<Character>(actor, target, obj, param);
			Character character43 = GetObject<Character>(actor, target, obj, param);
			Character character44 = ((Type == StoryEventType.TakeAllGold) ? character43 : subject73);
			Character character45 = ((Type == StoryEventType.TakeAllGold) ? subject73 : character43);
			float num17 = Data;
			if (Formulas != null && Formulas.Count > 0 && Formulas[0].GetConditionBlock() != null)
			{
				num17 += Formulas[0].GetConditionBlock().Evaluate(character45, character44, obj, param);
			}
			if (num17 == 0f)
			{
				num17 = float.MaxValue;
			}
			if (character44 != null && character45 != null)
			{
				GiveAllGold(character44, character45, num17, BoolData, BoolData2, BoolData3);
			}
			break;
		}
		case StoryEventType.GiveAllSupplies:
		case StoryEventType.TakeAllSupplies:
		{
			Character subject32 = GetSubject<Character>(actor, target, obj, param);
			Character character20 = GetObject<Character>(actor, target, obj, param);
			Character character21 = ((Type == StoryEventType.TakeAllSupplies) ? character20 : subject32);
			Character character22 = ((Type == StoryEventType.TakeAllSupplies) ? subject32 : character20);
			float num5 = Data;
			if (Formulas != null && Formulas.Count > 0 && Formulas[0].GetConditionBlock() != null)
			{
				num5 += Formulas[0].GetConditionBlock().Evaluate(character22, character21, obj, param);
			}
			if (num5 == 0f)
			{
				num5 = float.MaxValue;
			}
			Type type = null;
			if (!string.IsNullOrEmpty(StringData))
			{
				type = FindBaseObjectClassByName(StringData);
			}
			if (character21 != null && character22 != null)
			{
				GiveAllSupplies(character21, character22, num5, BoolData, BoolData2, BoolData3, type);
			}
			break;
		}
		case StoryEventType.PretendShakedown:
		{
			Community subjectCommunity11 = GetSubjectCommunity(actor, target, obj, param);
			Character character25 = GetObject<Character>(actor, target, obj, param);
			if (subjectCommunity11 != null && character25 != null)
			{
				subjectCommunity11.OnShakedown(character25, Data);
			}
			break;
		}
		case StoryEventType.PretendExtortion:
		{
			Community subjectCommunity10 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity7 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity10 != null && objectCommunity7 != null)
			{
				subjectCommunity10.OnExtortion(objectCommunity7, Data);
			}
			break;
		}
		case StoryEventType.GiveAllWeapons:
		{
			Character subject26 = GetSubject<Character>(actor, target, obj, param);
			Character character16 = GetObject<Character>(actor, target, obj, param);
			if (subject26 == null || character16 == null)
			{
				break;
			}
			for (int num4 = subject26.Inventory.Contents.Count - 1; num4 >= 0; num4--)
			{
				Equipment item = subject26.Inventory.GetItem(num4);
				if (item is Weapon || item.GetPrototype().IsAmmo())
				{
					item = subject26.Inventory.Take(subject26, item, item.GetAmount());
					NotificationManager.Instance.AddEquipmentNotification(subject26, character16, item, item.GetAmount());
					character16.Inventory.Add(character16, item);
				}
			}
			break;
		}
		case StoryEventType.EnableScriptedMove:
		{
			Character subject21 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject4 = GetObject<TileObject>(actor, target, obj, param);
			MovementType movementType = (MovementType)Math.Max(1, Array.IndexOf(Character.MovementTypeNames, StringData));
			ScriptedMoveImportance importance = (ScriptedMoveImportance)Math.Max(0, Array.IndexOf(ScriptedGoal.ScriptedMoveImportanceNames, StringData2));
			if (subject21 != null && tileObject4 != null)
			{
				subject21.SetScriptedGoalMarker(tileObject4, movementType, BoolData, importance, BoolData3, BoolData2, avoidHostileBases: true);
			}
			break;
		}
		case StoryEventType.DisableScriptedMove:
		{
			Character subject23 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject5 = GetObject<TileObject>(actor, target, obj, param);
			if (subject23 != null && (tileObject5 == null || subject23.GetScriptedGoalMarker() == tileObject5))
			{
				subject23.SetScriptedGoalMarker(null, MovementType.Walk, disableWhenReachedMarker: false, ScriptedMoveImportance.Low, teleportIfMoveFailed: false);
			}
			break;
		}
		case StoryEventType.AmbushMemberWithItem:
		{
			Character subject13 = GetSubject<Character>(actor, target, obj, param);
			Community objectCommunity4 = GetObjectCommunity(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype2 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject13 != null && objectCommunity4 != null && subject13.Community != null && subject13.Community != objectCommunity4 && equipmentPrototype2 != null)
			{
				Character memberWithEquipmentType = objectCommunity4.GetMemberWithEquipmentType(equipmentPrototype2);
				if (memberWithEquipmentType != null)
				{
					subject13.Community.Ambush(subject13, memberWithEquipmentType, Data, (int)Data2, equipmentPrototype2);
				}
			}
			break;
		}
		case StoryEventType.SetAutoCollect:
		{
			Character subject4 = GetSubject<Character>(actor, target, obj, param);
			Community subjectCommunity3 = GetSubjectCommunity(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject4 != null && equipmentPrototype != null)
			{
				subject4.SetEquipmentPolicy(equipmentPrototype, null, InfectionType.None, 16, 0f, sameAsCommunity: false);
			}
			else
			{
				subjectCommunity3?.SetEquipmentPolicy(equipmentPrototype, null, InfectionType.None, 16, 0f);
			}
			break;
		}
		case StoryEventType.SetAutoCollectItem:
		{
			Character subject120 = GetSubject<Character>(actor, target, obj, param);
			Community subjectCommunity29 = GetSubjectCommunity(actor, target, obj, param);
			Equipment equipment14 = GetObject<Equipment>(actor, target, obj, param);
			if (equipment14 != null)
			{
				EquipmentPrototype proto2 = ((equipment14.GetLiquidContentsType() == null) ? equipment14.GetPrototype() : null);
				LiquidPrototype liquidContentsType = equipment14.GetLiquidContentsType();
				if (subject120 != null)
				{
					subject120.SetEquipmentPolicy(proto2, liquidContentsType, InfectionType.None, 16, 0f, sameAsCommunity: false);
				}
				else
				{
					subjectCommunity29?.SetEquipmentPolicy(proto2, liquidContentsType, InfectionType.None, 16, 0f);
				}
			}
			break;
		}
		case StoryEventType.SetCarryAmount:
		{
			Character subject113 = GetSubject<Character>(actor, target, obj, param);
			Community subjectCommunity28 = GetSubjectCommunity(actor, target, obj, param);
			EquipmentPrototype equipmentPrototype7 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
			if (subject113 != null && equipmentPrototype7 != null)
			{
				bool sameAsCommunity;
				int equipmentPolicyMask = subject113.GetEquipmentPolicyMask(equipmentPrototype7, null, InfectionType.None, includeCommunityPolicy: false, out sameAsCommunity);
				subject113.SetEquipmentPolicy(equipmentPrototype7, null, InfectionType.None, equipmentPolicyMask, Data, sameAsCommunity: false);
			}
			else if (subjectCommunity28 != null && equipmentPrototype7 != null)
			{
				int equipmentPolicyMask2 = subjectCommunity28.GetEquipmentPolicyMask(equipmentPrototype7, null, InfectionType.None);
				subjectCommunity28.SetEquipmentPolicy(equipmentPrototype7, null, InfectionType.None, equipmentPolicyMask2, Data);
			}
			break;
		}
		case StoryEventType.EnableCanFollowPlayer:
		{
			Character subject107 = GetSubject<Character>(actor, target, obj, param);
			if (subject107 != null)
			{
				subject107.CanFollowPlayer = true;
				break;
			}
			Community subjectCommunity26 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity26 == null)
			{
				break;
			}
			{
				foreach (Character member6 in subjectCommunity26.Members)
				{
					if (member6.AliveAndNotZombie)
					{
						member6.CanFollowPlayer = true;
					}
				}
				break;
			}
		}
		case StoryEventType.DisableCanFollowPlayer:
		{
			Character subject95 = GetSubject<Character>(actor, target, obj, param);
			if (subject95 != null)
			{
				subject95.CanFollowPlayer = false;
				if (subject95.SquadLeader != null && subject95.SquadLeader.IsControllableByPlayer())
				{
					subject95.Follow(null);
				}
				break;
			}
			Community subjectCommunity24 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity24 == null)
			{
				break;
			}
			{
				foreach (Character member7 in subjectCommunity24.Members)
				{
					member7.CanFollowPlayer = false;
					if (member7.SquadLeader != null && member7.SquadLeader.IsControllableByPlayer())
					{
						member7.Follow(null);
					}
				}
				break;
			}
		}
		case StoryEventType.EnableTrigger:
		{
			Character subject92 = GetSubject<Character>(actor, target, obj, param);
			BaseObject obj2 = GetObject<BaseObject>(actor, target, obj, param);
			Character thirdParty14 = GetThirdParty<Character>(actor, target, obj, param);
			StoryManager.Instance.EnableTrigger(StringData, subject92, thirdParty14, obj2);
			break;
		}
		case StoryEventType.DisableTrigger:
		{
			Character subject93 = GetSubject<Character>(actor, target, obj, param);
			BaseObject obj3 = GetObject<BaseObject>(actor, target, obj, param);
			Character thirdParty15 = GetThirdParty<Character>(actor, target, obj, param);
			StoryManager.Instance.DisableTrigger(StringData, subject93, thirdParty15, obj3);
			break;
		}
		case StoryEventType.OneShotTrigger:
			StoryManager.Instance.OneShotTrigger(StringData, actor, target, obj, param);
			break;
		case StoryEventType.OneShotTriggerWithParams:
		{
			Character subject91 = GetSubject<Character>(actor, target, obj, param);
			Character target3 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty13 = GetThirdParty<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.OneShotTrigger(StringData, subject91, target3, thirdParty13, param);
			break;
		}
		case StoryEventType.Hug:
		{
			Character subject77 = GetSubject<Character>(actor, target, obj, param);
			Character character47 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty12 = GetThirdParty<Character>(actor, target, obj, param);
			if (subject77 != null && character47 != null)
			{
				Speech speechOnFinish4 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
				subject77.SetLeaderCommand(new MoveToAndInteractGoal(subject77, character47, BoolData ? InteractionType.HugThenTalk : InteractionType.Hug, 0, speechOnFinish4, null, thirdParty12), character47, ObeyLeaderGoal.SourceType.Scripted);
				subject77.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.HugShoot:
		{
			Character subject65 = GetSubject<Character>(actor, target, obj, param);
			Character character42 = GetObject<Character>(actor, target, obj, param);
			BaseObject thirdParty8 = GetThirdParty<Character>(actor, target, obj, param);
			if (subject65 != null && character42 != null)
			{
				Speech speechOnFinish3 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
				if (!subject65.Inventory.HasAnyGuns(subject65, withAmmo: false))
				{
					subject65.Inventory.Add(subject65, Equipment.Spawn(EquipmentPrototype.Pistol));
				}
				subject65.SetLeaderCommand(new MoveToAndInteractGoal(subject65, character42, InteractionType.HugShoot, 0, speechOnFinish3, null, thirdParty8), character42, ObeyLeaderGoal.SourceType.Scripted);
				subject65.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.RepairVehicleWithItem:
		{
			Character subject58 = GetSubject<Character>(actor, target, obj, param);
			EnterableVehicle enterableVehicle = GetObject<EnterableVehicle>(actor, target, obj, param);
			BaseObject thirdParty7 = GetThirdParty<Character>(actor, target, obj, param);
			EquipmentPrototype proto = GameImpl.Instance.FindEquipmentPrototypeByName(StringData2);
			if (subject58 != null && enterableVehicle != null && subject58.GetLeaderCommand() == null)
			{
				Speech speechOnFinish2 = GameImpl.Instance.FindSpeechByUniqueID(StringData);
				subject58.SetLeaderCommand(new MoveToAndInteractGoal(subject58, enterableVehicle, InteractionType.RepairVehicleWithItem, 0, speechOnFinish2, proto, thirdParty7, (int)Data), enterableVehicle, ObeyLeaderGoal.SourceType.Scripted);
				subject58.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.StartEngine:
			GetSubject<EnterableVehicle>(actor, target, obj, param)?.StartMoving();
			break;
		case StoryEventType.Treat:
		{
			Character subject51 = GetSubject<Character>(actor, target, obj, param);
			Character character34 = GetObject<Character>(actor, target, obj, param);
			if (subject51 != null && character34 != null)
			{
				Speech speechOnFinish = GameImpl.Instance.FindSpeechByUniqueID(StringData);
				subject51.SetLeaderCommand(new TreatGoal(character34, speechOnFinish), character34, ObeyLeaderGoal.SourceType.Scripted);
				subject51.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.Attack:
		{
			Character subject17 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject3 = GetObject<TileObject>(actor, target, obj, param);
			Community subjectCommunity7 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity5 = GetObjectCommunity(actor, target, obj, param);
			if (tileObject3 == null && objectCommunity5 != null)
			{
				TerrainCoord tile = subject17?.Tile ?? ((subjectCommunity7 != null && subjectCommunity7.Leader != null) ? subjectCommunity7.Leader.Tile : TerrainCoord.Zero);
				tileObject3 = objectCommunity5.GetNearestLivingMember(tile, null, BaseObjectType.Human, float.MaxValue);
			}
			Character character9 = tileObject3 as Character;
			if (tileObject3 == null)
			{
				break;
			}
			Squad squad = null;
			if (subject17 != null && subject17.Alive)
			{
				squad = subject17.GetSquad();
				if (squad == null || character9 == null)
				{
					subject17.GetOrCreateTarget(tileObject3).ForceVisible(subject17);
					if (subject17.Zombie)
					{
						if (character9 != null)
						{
							subject17.SetLeaderCommand(new FeedOnLiving(subject17, character9), tileObject3, ObeyLeaderGoal.SourceType.Scripted);
						}
					}
					else
					{
						Attack attack = new Attack(subject17, tileObject3, dontOpenOurGates: false, StayInRangeParams.OfSquadLeader());
						attack.CanAttackPreferredTargetEvenIfNotEnemy = BoolData;
						subject17.SetLeaderCommand(attack, tileObject3, ObeyLeaderGoal.SourceType.Scripted);
					}
					subject17.DirectControlled = false;
				}
			}
			else if (subjectCommunity7 != null)
			{
				squad = ((subjectCommunity7.Squads.Count > 0) ? subjectCommunity7.Squads[0] : null);
				if (squad == null)
				{
					foreach (Character member8 in subjectCommunity7.Members)
					{
						if (!member8.Alive || member8.Zombie != subjectCommunity7.IsZombieCommunity())
						{
							continue;
						}
						member8.GetOrCreateTarget(tileObject3).ForceVisible(member8);
						if (member8.Zombie)
						{
							if (character9 != null)
							{
								member8.SetLeaderCommand(new FeedOnLiving(member8, character9), tileObject3, ObeyLeaderGoal.SourceType.Scripted);
							}
						}
						else
						{
							Attack attack2 = new Attack(member8, tileObject3, dontOpenOurGates: false, StayInRangeParams.OfSquadLeader());
							attack2.CanAttackPreferredTargetEvenIfNotEnemy = BoolData;
							member8.SetLeaderCommand(attack2, tileObject3, ObeyLeaderGoal.SourceType.Scripted);
						}
						member8.DirectControlled = false;
					}
				}
			}
			if (squad != null && character9 != null)
			{
				int threatId = subjectCommunity7.AddThreat(character9);
				subjectCommunity7.SetSquadAction(squad, SquadAction.AttackThreat, threatId, squad.GoalTile, null);
			}
			break;
		}
		case StoryEventType.Assassinate:
		{
			Character subject11 = GetSubject<Character>(actor, target, obj, param);
			Character character6 = GetObject<Character>(actor, target, obj, param);
			if (subject11 != null && character6 != null)
			{
				if (!subject11.Inventory.HasAnyGuns(subject11, withAmmo: false))
				{
					subject11.Inventory.Add(subject11, Equipment.Spawn(EquipmentPrototype.Pistol));
				}
				subject11.SetLeaderCommand(new RangedAttack(MovementType.Run, assassinate: true, Secrecy, dontOpenOurGates: false, default(StayInRangeParams)), character6, ObeyLeaderGoal.SourceType.Scripted);
				subject11.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.FleeAndDisappear:
		{
			Character subject9 = GetSubject<Character>(actor, target, obj, param);
			if (subject9 != null)
			{
				FleeAndDisappearGoal fleeAndDisappearGoal = new FleeAndDisappearGoal();
				fleeAndDisappearGoal.TriggerUniqueID = StringData;
				fleeAndDisappearGoal.TriggerActor = actor;
				fleeAndDisappearGoal.TriggerTarget = target;
				fleeAndDisappearGoal.TriggerObject = obj;
				fleeAndDisappearGoal.TriggerParam = param;
				subject9.SetLeaderCommand(fleeAndDisappearGoal, null, ObeyLeaderGoal.SourceType.Scripted);
				subject9.DirectControlled = false;
			}
			break;
		}
		case StoryEventType.Teleport:
		{
			Character subject2 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject = GetObject<TileObject>(actor, target, obj, param);
			if (subject2 != null && tileObject != null)
			{
				if (tileObject is Building building)
				{
					building.OnCharacterEnter(subject2, wasOrderedInsideBuilding: false);
				}
				if (tileObject is Marker marker)
				{
					subject2.SetPosition(marker.Pos);
				}
			}
			break;
		}
		case StoryEventType.SetRank:
		{
			Character subject128 = GetSubject<Character>(actor, target, obj, param);
			int num35 = Array.IndexOf(Character.RankNames, StringData);
			if (subject128 != null && num35 != -1)
			{
				subject128.SetRank((Rank)num35);
			}
			break;
		}
		case StoryEventType.SetRole:
		{
			Character subject124 = GetSubject<Character>(actor, target, obj, param);
			int num34 = Array.IndexOf(Character.RoleNames, StringData);
			if (subject124 != null && num34 != -1)
			{
				subject124.AddRole(new RoleInfo((Role)num34));
			}
			break;
		}
		case StoryEventType.IncrementSkill:
		{
			Character subject123 = GetSubject<Character>(actor, target, obj, param);
			int num33 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject123 != null && num33 != -1)
			{
				subject123.IncrementSkillLevel((SkillType)num33);
			}
			break;
		}
		case StoryEventType.SetSkill:
		{
			Character subject115 = GetSubject<Character>(actor, target, obj, param);
			int num27 = Array.IndexOf(Skillset.SkillNames, StringData);
			int num28 = Session.Instance.DeterministicRand.Next((int)Data, (int)Data2 + 1);
			if (subject115 != null && num27 != -1)
			{
				int cap2 = subject115.Skillset.GetCap((SkillType)num27);
				if (!BoolData)
				{
					num28 = Math.Min(num28, cap2);
				}
				else if (num28 > cap2)
				{
					subject115.Skillset.SetCap(subject115, (SkillType)num27, num28);
				}
				subject115.Skillset.SetLevel(subject115, (SkillType)num27, num28);
			}
			break;
		}
		case StoryEventType.SetSkillCap:
		{
			Character subject111 = GetSubject<Character>(actor, target, obj, param);
			int num25 = Array.IndexOf(Skillset.SkillNames, StringData);
			int cap = Session.Instance.DeterministicRand.Next((int)Data, (int)Data2 + 1);
			if (subject111 != null && num25 != -1)
			{
				subject111.Skillset.SetCap(subject111, (SkillType)num25, cap);
			}
			break;
		}
		case StoryEventType.SetPersonality:
		{
			Character subject105 = GetSubject<Character>(actor, target, obj, param);
			if (subject105 == null)
			{
				break;
			}
			subject105.SetPersonality(StringData, BoolData);
			if (!BoolData)
			{
				break;
			}
			PersonalityGroup personalityGroup = GameImpl.Instance.FindPersonalityGroup(StringData, PersonalityGroup.NormalFaction);
			if (personalityGroup == null)
			{
				break;
			}
			for (int num23 = 0; num23 < personalityGroup.Personalities.Count; num23++)
			{
				if (personalityGroup.Personalities[num23].Personality != StringData)
				{
					subject105.SetPersonality(personalityGroup.Personalities[num23].Personality, val: false);
				}
			}
			break;
		}
		case StoryEventType.DiscoverSkill:
		{
			Character subject103 = GetSubject<Character>(actor, target, obj, param);
			int num22 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject103 != null && num22 != -1)
			{
				subject103.SetSkillKnown((SkillType)num22);
			}
			break;
		}
		case StoryEventType.DiscoverSkillIfPlayer:
		{
			Character subject94 = GetSubject<Character>(actor, target, obj, param);
			Character character52 = GetObject<Character>(actor, target, obj, param);
			int num20 = Array.IndexOf(Skillset.SkillNames, StringData);
			if (subject94 != null && subject94.CanDiscoverStuffFromSpeech() && character52 != null && num20 != -1)
			{
				character52.SetSkillKnown((SkillType)num20);
			}
			break;
		}
		case StoryEventType.DiscoverPersonality:
			GetSubject<Character>(actor, target, obj, param)?.SetPersonalityKnown(StringData);
			break;
		case StoryEventType.DiscoverPersonalityIfPlayer:
		{
			Character subject90 = GetSubject<Character>(actor, target, obj, param);
			Character character51 = GetObject<Character>(actor, target, obj, param);
			if (subject90 != null && subject90.CanDiscoverStuffFromSpeech())
			{
				character51?.SetPersonalityKnown(StringData);
			}
			break;
		}
		case StoryEventType.SetInjuryRemarkedOn:
		{
			Character subject86 = GetSubject<Character>(actor, target, obj, param);
			if (subject86 != null)
			{
				subject86.InjuryRemarkedOn = true;
			}
			break;
		}
		case StoryEventType.SetInfectionRemarkedOn:
		{
			Character subject84 = GetSubject<Character>(actor, target, obj, param);
			if (subject84 != null)
			{
				subject84.InfectionRemarkedOn = true;
			}
			break;
		}
		case StoryEventType.DiscoverCommunityHasNoAntigen:
		{
			Community subjectCommunity20 = GetSubjectCommunity(actor, target, obj, param);
			int num18 = Array.IndexOf(Injury.InfectionTypeNames, StringData);
			if (subjectCommunity20 != null && num18 != -1)
			{
				subjectCommunity20.SetNoAntigenKnown((InfectionType)num18);
			}
			break;
		}
		case StoryEventType.SetHangOutLocation:
		{
			Character subject76 = GetSubject<Character>(actor, target, obj, param);
			TileObject tileObject6 = BaseObjectManager.Instance.GetObjectByUniqueID(ObjectID) as TileObject;
			if (subject76 != null && tileObject6 != null)
			{
				subject76.InitialHangOutLocation = tileObject6.GetCentreTile();
				subject76.SetHangoutLocation(tileObject6.GetCentreTile());
			}
			break;
		}
		case StoryEventType.Encourage:
		{
			Character subject71 = GetSubject<Character>(actor, target, obj, param);
			if (subject71 != null)
			{
				subject71.EncouragementTimeout = Session.Instance.PlayTime + TimeSpan.FromSeconds(Data * Sun.DayLengthSecs);
				if (subject71.IsSitting())
				{
					subject71.SetLeaderCommand(new StopSittingGoal(), null, ObeyLeaderGoal.SourceType.Scripted);
					subject71.DirectControlled = false;
				}
			}
			break;
		}
		case StoryEventType.AddDownTime:
			GetSubject<Character>(actor, target, obj, param)?.AddDownTime(Data * Sun.DayLengthSecs);
			break;
		case StoryEventType.StartSimulatingSurvivalFactors:
		{
			Character subject63 = GetSubject<Character>(actor, target, obj, param);
			if (subject63 != null)
			{
				subject63.DontSimulateSurvivalFactorsUntilDiscovered = BoolData;
				subject63.DontSimulateSurvivalFactorsUntilJoinCommunity = BoolData2;
				break;
			}
			Community subjectCommunity17 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity17 == null)
			{
				break;
			}
			{
				foreach (Character member9 in subjectCommunity17.Members)
				{
					member9.DontSimulateSurvivalFactorsUntilDiscovered = BoolData;
					member9.DontSimulateSurvivalFactorsUntilJoinCommunity = BoolData2;
				}
				break;
			}
		}
		case StoryEventType.StartBoxingMatch:
		{
			Character subject59 = GetSubject<Character>(actor, target, obj, param);
			Character character39 = GetObject<Character>(actor, target, obj, param);
			int num13 = Array.IndexOf(Character.SparringTypeNames, StringData);
			if (subject59 == null || character39 == null)
			{
				break;
			}
			switch (num13)
			{
			case 3:
			case 4:
			case 5:
				subject59.SetSparringPartner((SparringType)num13, character39, subject59);
				character39.SetSparringPartner((SparringType)num13, subject59, subject59);
				if (num13 == 3 && character39.IsPlayerAvatar())
				{
					PlayerRecord playerControllingCharacter3 = Session.Instance.GetPlayerControllingCharacter(character39);
					if (playerControllingCharacter3 != null && !playerControllingCharacter3.FlyMode)
					{
						character39.DirectControlled = true;
						if (!(character39.EquippedItem is MeleeWeapon) && !(character39.DesiredEquippedItem is MeleeWeapon))
						{
							character39.DesiredEquippedItem = character39.Inventory.GetBestWeapon(character39, subject59, wantRanged: false, bluntOnly: false);
						}
					}
				}
				else
				{
					if (num13 != 5)
					{
						break;
					}
					PlayerRecord playerControllingCharacter4 = Session.Instance.GetPlayerControllingCharacter(subject59);
					if (playerControllingCharacter4 != null && !playerControllingCharacter4.FlyMode)
					{
						subject59.DirectControlled = true;
						if (!(subject59.EquippedItem is MeleeWeapon) && !(subject59.DesiredEquippedItem is MeleeWeapon))
						{
							subject59.DesiredEquippedItem = subject59.Inventory.GetBestWeapon(subject59, character39, wantRanged: false, bluntOnly: false);
						}
					}
				}
				break;
			default:
			{
				Equipment gold = subject59.Inventory.GetGold();
				int boxerWagerAmount = character39.GetBoxerWagerAmount();
				if (gold != null && gold.GetAmount() >= boxerWagerAmount)
				{
					PlayerRecord playerControllingCharacter2 = Session.Instance.GetPlayerControllingCharacter(subject59);
					if (playerControllingCharacter2 != null && !playerControllingCharacter2.FlyMode)
					{
						subject59.DirectControlled = true;
						subject59.DesiredEquippedItem = null;
					}
					subject59.SetSparringPartner((SparringType)num13, character39, subject59);
					character39.SetSparringPartner((SparringType)num13, subject59, subject59);
					NotificationManager.Instance.AddEquipmentNotification(subject59, character39, gold, boxerWagerAmount);
					character39.Inventory.Add(character39, subject59.Inventory.Take(subject59, gold, boxerWagerAmount));
				}
				break;
			}
			case -1:
				break;
			}
			break;
		}
		case StoryEventType.WinBoxingMatch:
		{
			Character subject55 = GetSubject<Character>(actor, target, obj, param);
			Character character37 = GetObject<Character>(actor, target, obj, param);
			if (subject55 == null || character37 == null)
			{
				break;
			}
			SparringType sparringType2 = subject55.SparringType;
			Character sparringInstigator2 = subject55.SparringInstigator;
			subject55.SetSparringPartner(SparringType.None, null, null);
			character37.SetSparringPartner(SparringType.None, null, null);
			switch (sparringType2)
			{
			case SparringType.Feuding:
				if (sparringInstigator2 == subject55)
				{
					Memory.OnMemorableEvent(MemoryPrototype.BeatUp, subject55, character37, 1f, secret: false);
				}
				else
				{
					Memory.OnMemorableEvent(MemoryPrototype.FoughtOff, subject55, character37, 1f, secret: false);
				}
				break;
			case SparringType.Boxing:
			case SparringType.Fencing:
				Memory.OnMemorableEvent(MemoryPrototype.LostFisticuffs, character37, subject55, 1f, secret: false);
				break;
			}
			break;
		}
		case StoryEventType.LoseBoxingMatch:
		{
			Character subject52 = GetSubject<Character>(actor, target, obj, param);
			Character character35 = GetObject<Character>(actor, target, obj, param);
			if (subject52 == null || character35 == null || subject52.SparringPartner != character35)
			{
				break;
			}
			SparringType sparringType = subject52.SparringType;
			Character sparringInstigator = subject52.SparringInstigator;
			subject52.SetSparringPartner(SparringType.None, null, null);
			character35.SetSparringPartner(SparringType.None, null, null);
			switch (sparringType)
			{
			case SparringType.Feuding:
				if (sparringInstigator == character35)
				{
					Memory.OnMemorableEvent(MemoryPrototype.BeatUp, character35, subject52, 1f, secret: false);
				}
				else
				{
					Memory.OnMemorableEvent(MemoryPrototype.FoughtOff, character35, subject52, 1f, secret: false);
				}
				break;
			case SparringType.Boxing:
			case SparringType.Fencing:
			{
				Memory.OnMemorableEvent(MemoryPrototype.WonFisticuffs, character35, subject52, 1f, secret: false);
				Equipment equipment8 = subject52.Inventory.GetGold();
				int num12 = subject52.GetBoxerWagerAmount() * 2;
				if (equipment8 == null)
				{
					equipment8 = Equipment.Spawn(EquipmentPrototype.Gold, num12 + subject52.ReservedGoldAmount);
					subject52.Inventory.Add(subject52, equipment8);
				}
				else if (num12 + subject52.ReservedGoldAmount > equipment8.GetAmount())
				{
					equipment8.IncrementAmount(num12 + subject52.ReservedGoldAmount - equipment8.GetAmount());
				}
				NotificationManager.Instance.AddEquipmentNotification(subject52, character35, equipment8, num12);
				character35.Inventory.Add(character35, subject52.Inventory.Take(subject52, equipment8, num12));
				break;
			}
			}
			break;
		}
		case StoryEventType.SetKnownBoxer:
		{
			Character subject48 = GetSubject<Character>(actor, target, obj, param);
			if (subject48 != null && subject48.Boxer)
			{
				subject48.KnownBoxer = true;
			}
			break;
		}
		case StoryEventType.SetFirstCommunityBoxerReadyToFightKnown:
		{
			Community subjectCommunity14 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity14 != null)
			{
				Character firstBoxerReadyToFight = subjectCommunity14.GetFirstBoxerReadyToFight(Session.Instance.DeterministicRand);
				if (firstBoxerReadyToFight != null && firstBoxerReadyToFight.Boxer)
				{
					firstBoxerReadyToFight.KnownBoxer = true;
					firstBoxerReadyToFight.NameKnown = true;
				}
			}
			break;
		}
		case StoryEventType.EnsureCommunityHasBoxer:
		{
			Community subjectCommunity12 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity12 == null || subjectCommunity12.GetBoxerCount() != 0)
			{
				break;
			}
			List<Character> list4 = new List<Character>();
			foreach (Character member10 in subjectCommunity12.Members)
			{
				if (member10.AliveAndNotZombie && member10.GetBaseObjectType() == BaseObjectType.Human)
				{
					list4.Add(member10);
				}
			}
			if (list4.Count > 0)
			{
				int index = Session.Instance.DeterministicRand.Next(list4.Count);
				list4[index].Boxer = true;
			}
			break;
		}
		case StoryEventType.SetBoxer:
		{
			Character subject36 = GetSubject<Character>(actor, target, obj, param);
			if (subject36 != null)
			{
				subject36.Boxer = BoolData;
			}
			break;
		}
		case StoryEventType.AssignBlameForPickpocketedItem:
		{
			Character subject33 = GetSubject<Character>(actor, target, obj, param);
			Character suspect = GetObject<Character>(actor, target, obj, param);
			if (subject33 != null && subject33.PickpocketedItems != null)
			{
				switch (param.GetParamType())
				{
				case ParamType.EquipmentPrototype:
					subject33.AssignBlameForPickpocketedItem(GatheredItem.Create(param.GetEquipmentPrototype(), null, InfectionType.None, 1), suspect);
					break;
				case ParamType.LiquidPrototype:
					subject33.AssignBlameForPickpocketedItem(GatheredItem.Create(null, param.GetLiquidPrototype(), InfectionType.None, 1), suspect);
					break;
				}
			}
			break;
		}
		case StoryEventType.GameComplete:
		{
			if (Session.Instance.GameFinishedState == GameFinishedState.GameOver)
			{
				break;
			}
			Session instance2 = Session.Instance;
			Community playerCommunity = instance2.CommunityManager.PlayerCommunity;
			instance2.GameFinishedState = GameFinishedState.GameComplete;
			instance2.ShowContinueButton = BoolData;
			if (!string.IsNullOrEmpty(StringData))
			{
				EquipmentPrototype equipmentPrototype5 = GameImpl.Instance.FindEquipmentPrototypeByName(StringData);
				if (equipmentPrototype5 != null)
				{
					instance2.WantCinematicState = CinematicState.ExfilCharacter;
					instance2.ExfilCharacters.Clear();
					foreach (Character member11 in playerCommunity.Members)
					{
						if (BoolData2 ? (member11.Inventory.CountItemsOfType(equipmentPrototype5) > 0) : (member11.Inventory.CountGiftedItemsOfType(equipmentPrototype5) > 0))
						{
							instance2.ExfilCharacters.Add(member11);
						}
					}
					if (playerCommunity.Leader.AliveAndNotZombie)
					{
						instance2.ExfilCharacters.Remove(playerCommunity.Leader);
						instance2.ExfilCharacters.Add(playerCommunity.Leader);
					}
				}
			}
			if (!GameImpl.Instance.IsCurrentStoryListEqualTo(AchievementsManager.SandboxMode, AchievementsManager.AllowAchievementsWithMods) || !(instance2.DifficultySettings.DifficultyName == "Hard"))
			{
				break;
			}
			if (instance2.LoneWolf)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteSandbox_Hard_LoneWolf);
			}
			bool flag = false;
			bool flag2 = false;
			if (playerCommunity.Leader != null && playerCommunity.Leader.AliveAndNotZombie)
			{
				if (instance2.WantCinematicState == CinematicState.ExfilCharacter)
				{
					foreach (Character exfilCharacter in instance2.ExfilCharacters)
					{
						if (exfilCharacter.AliveAndNotZombie && !exfilCharacter.IsPlayerAvatar())
						{
							flag |= Relationship.HasRomanticRelationship(playerCommunity.Leader, exfilCharacter);
							flag2 |= Relationship.GetRelationship(playerCommunity.Leader, exfilCharacter) == RelationshipType.FriendsWith;
						}
					}
				}
				else
				{
					foreach (Character member12 in playerCommunity.Members)
					{
						if (member12.AliveAndNotZombie && !member12.IsPlayerAvatar())
						{
							flag |= Relationship.HasRomanticRelationship(playerCommunity.Leader, member12);
							flag2 |= Relationship.GetRelationship(playerCommunity.Leader, member12) == RelationshipType.FriendsWith;
						}
					}
				}
			}
			if (flag && flag2)
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteSandbox_Hard_WithFriends);
			}
			break;
		}
		case StoryEventType.GameCompleteRIP:
		{
			if (Session.Instance.GameFinishedState == GameFinishedState.GameOver)
			{
				break;
			}
			Session instance = Session.Instance;
			instance.GameFinishedState = GameFinishedState.GameComplete;
			instance.ShowContinueButton = false;
			EnterableVehicle subject5 = GetSubject<EnterableVehicle>(actor, target, obj, param);
			if (subject5 == null)
			{
				break;
			}
			instance.WantCinematicState = CinematicState.RipVehicle;
			instance.ExfilVehicle = subject5;
			instance.ExfilCharacters.Clear();
			Character[] inhabitants = subject5.Inhabitants;
			foreach (Character character in inhabitants)
			{
				if (character != null)
				{
					instance.ExfilCharacters.Add(character);
				}
			}
			if (instance.CommunityManager.PlayerCommunity.Leader != null && instance.CommunityManager.PlayerCommunity.Leader.AliveAndNotZombie)
			{
				instance.ExfilCharacters.Remove(instance.CommunityManager.PlayerCommunity.Leader);
				instance.ExfilCharacters.Add(instance.CommunityManager.PlayerCommunity.Leader);
			}
			if (instance.DifficultySettings.DifficultyName == "Harder" && BaseObjectManager.Instance.GetObjectByUniqueID(Character.EmmaOConnor) is Character { AliveAndNotZombie: not false } character2 && character2.IsDisappeared())
			{
				AchievementsManager.Instance.UnlockAchievement(Achievement.CompleteStory_Hard);
			}
			break;
		}
		case StoryEventType.GameCompleteOutbreak:
		{
			if (Session.Instance.GameFinishedState == GameFinishedState.GameOver)
			{
				break;
			}
			Session instance4 = Session.Instance;
			instance4.GameFinishedState = GameFinishedState.GameComplete;
			instance4.ShowContinueButton = false;
			instance4.WantCinematicState = CinematicState.OutbreakHelicopter;
			Character leader2 = instance4.CommunityManager.PlayerCommunity.Leader;
			instance4.ExfilCharacters.Clear();
			foreach (Character member13 in instance4.CommunityManager.PlayerCommunity.Members)
			{
				if (member13.AliveAndNotZombie && member13.GetBaseObjectType() == BaseObjectType.Human && (instance4.ExfilCharacters.Count < 4 || member13.IsPlayerAvatar() || Relationship.GetRelationship(member13, leader2) != RelationshipType.None))
				{
					instance4.ExfilCharacters.Add(member13);
				}
			}
			if (leader2 != null && leader2.AliveAndNotZombie)
			{
				instance4.ExfilCharacters.Remove(leader2);
				instance4.ExfilCharacters.Add(leader2);
			}
			break;
		}
		case StoryEventType.GameOver:
			Session.Instance.SetGameOver();
			break;
		case StoryEventType.SaveGame:
			if (Session.Instance.DifficultySettings.SaveTokensRequired)
			{
				Session.Instance.WantTokenSave = true;
			}
			else
			{
				Session.Instance.WantAutoSave = true;
			}
			break;
		case StoryEventType.SurpriseSaveGameOverwrite:
			if (Session.Instance.DifficultySettings.SaveTokensRequired)
			{
				Session.Instance.WantTokenSave = true;
				Session.Instance.WantSurpriseSaveGameOverwrite = true;
			}
			else
			{
				Session.Instance.WantAutoSave = true;
			}
			break;
		case StoryEventType.DisableEmpathy:
		{
			Character subject121 = GetSubject<Character>(actor, target, obj, param);
			BaseObject baseObject4 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject121 == null || baseObject4 == null)
			{
				break;
			}
			int num30 = -1;
			for (int num31 = 0; num31 < subject121.EmpathyOverrides.Count; num31++)
			{
				if (subject121.EmpathyOverrides[num31].OverrideObject == baseObject4)
				{
					num30 = num31;
				}
			}
			if (num30 == -1)
			{
				num30 = subject121.EmpathyOverrides.Count;
				subject121.EmpathyOverrides.Add(default(EmpathyOverride));
			}
			EmpathyOverride value = subject121.EmpathyOverrides[num30];
			value.OverrideObject = baseObject4;
			value.OverrideValue = Data;
			subject121.EmpathyOverrides[num30] = value;
			break;
		}
		case StoryEventType.EnableEmpathy:
		{
			Character subject117 = GetSubject<Character>(actor, target, obj, param);
			BaseObject baseObject3 = GetObject<BaseObject>(actor, target, obj, param);
			if (subject117 == null || baseObject3 == null)
			{
				break;
			}
			for (int num29 = 0; num29 < subject117.EmpathyOverrides.Count; num29++)
			{
				if (subject117.EmpathyOverrides[num29].OverrideObject == baseObject3)
				{
					subject117.EmpathyOverrides.RemoveAt(num29);
				}
			}
			break;
		}
		case StoryEventType.SetTimer:
			GetSubject<Character>(actor, target, obj, param)?.SetTimer(StringData, TimeSpan.FromSeconds(Data));
			break;
		case StoryEventType.WarnAboutTraps:
			GetSubjectCommunity(actor, target, obj, param)?.WarnAboutTraps(pitTraps: true, tripwires: true);
			break;
		case StoryEventType.TellTradersToMoveOn:
		{
			Character subject112 = GetSubject<Character>(actor, target, obj, param);
			if (subject112 != null && subject112.Community != null)
			{
				subject112.Community.TellTradersToMoveOn(subject112);
			}
			break;
		}
		case StoryEventType.ExfilHeliTeam:
			GetSubjectCommunity(actor, target, obj, param)?.ExfilHeliTeam();
			break;
		case StoryEventType.SpawnTemplate:
		{
			Template template = GameImpl.Instance.FindTemplateByUniqueID(StringData);
			if (template != null)
			{
				TerrainCoord tile2 = TerrainCoord.Invalid;
				BaseObject subject108 = GetSubject<BaseObject>(actor, target, obj, param);
				if (subject108 != null)
				{
					tile2 = subject108.GetTile();
				}
				List<CommunityManager.TileAndRadius> cachedPlayerCommunityTiles = null;
				template.SpawnFromTemplate(tile2, canBeEnclosed: true, null, null, null, actor, target, obj, param, 1, null, ref cachedPlayerCommunityTiles);
			}
			break;
		}
		case StoryEventType.MarkEulogyGiven:
		{
			Character subject106 = GetSubject<Character>(actor, target, obj, param);
			if (subject106 != null)
			{
				subject106.EulogyGiven = true;
			}
			break;
		}
		case StoryEventType.SetRelationship:
		{
			Character subject101 = GetSubject<Character>(actor, target, obj, param);
			Character character53 = GetObject<Character>(actor, target, obj, param);
			int num21 = Array.IndexOf(Relationship.RelationshipTypeNames, StringData);
			if (num21 == -1)
			{
				num21 = 0;
			}
			if (subject101 != null && character53 != null)
			{
				Relationship.SetRelationship(subject101, (RelationshipType)num21, character53, BoolData, generating: false, BoolData2);
				Relationship.SetRelationshipApprovalRespect(subject101, (RelationshipType)num21, character53, Data, Data2);
			}
			break;
		}
		case StoryEventType.ActivateInvader:
		{
			BaseObject subject100 = GetSubject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.ActivateInvader(StringData, subject100, out var _);
			break;
		}
		case StoryEventType.DeactivateInvader:
		{
			BaseObject subject99 = GetSubject<BaseObject>(actor, target, obj, param);
			StoryManager.Instance.DeactivateInvader(StringData, subject99);
			break;
		}
		case StoryEventType.DrinkAlcohol:
		{
			Character subject97 = GetSubject<Character>(actor, target, obj, param);
			if (subject97 != null)
			{
				Equipment bestAlcoholToDrink = subject97.Inventory.GetBestAlcoholToDrink(subject97, subject97, BoolData);
				if (bestAlcoholToDrink != null)
				{
					subject97.CommandDrink(null, bestAlcoholToDrink);
				}
			}
			break;
		}
		case StoryEventType.SetBuildingOfTypeUnlocked:
		{
			Community subjectCommunity23 = GetSubjectCommunity(actor, target, obj, param);
			PropPrototype propPrototype = GameImpl.Instance.FindPropPrototypeByName(StringData);
			if (subjectCommunity23 == null || propPrototype == null)
			{
				break;
			}
			{
				foreach (Prop building3 in subjectCommunity23.Buildings)
				{
					if (building3.GetPropPrototype() == propPrototype && building3 is Building building2)
					{
						building2.UnlockedToPlayer = BoolData;
					}
				}
				break;
			}
		}
		case StoryEventType.GrantMiningRightsToPlayer:
		{
			Community subjectCommunity22 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity22 != null)
			{
				subjectCommunity22.GrantedMiningRightsToPlayer = BoolData;
			}
			break;
		}
		case StoryEventType.SetGatePermission:
			GetSubjectCommunity(actor, target, obj, param)?.SetPlayerGatePermission(BoolData ? CanOpenGates.Yes : CanOpenGates.No);
			break;
		case StoryEventType.VisitGrave:
		{
			Character subject89 = GetSubject<Character>(actor, target, obj, param);
			if (subject89 != null && subject89.BuriedInGrave != null)
			{
				subject89.BuriedInGrave.VisitGrave();
			}
			break;
		}
		case StoryEventType.SetConcealed:
		{
			Equipment subject87 = GetSubject<Equipment>(actor, target, obj, param);
			if (subject87 != null)
			{
				subject87.Concealed = BoolData;
			}
			break;
		}
		case StoryEventType.UnconcealAllItems:
		{
			TileObject subject85 = GetSubject<TileObject>(actor, target, obj, param);
			if (subject85 == null)
			{
				break;
			}
			EquipmentContainer inventory = subject85.GetInventory();
			if (inventory == null)
			{
				break;
			}
			{
				foreach (Equipment content in inventory.Contents)
				{
					content.Concealed = false;
				}
				break;
			}
		}
		case StoryEventType.SetHiddenOnMap:
		{
			Character subject83 = GetSubject<Character>(actor, target, obj, param);
			if (subject83 != null)
			{
				subject83.HiddenOnMap = BoolData;
			}
			break;
		}
		case StoryEventType.SetPlayDead:
			GetSubject<Character>(actor, target, obj, param)?.SetPlayDead(BoolData);
			break;
		case StoryEventType.SetDontBury:
		{
			Character subject80 = GetSubject<Character>(actor, target, obj, param);
			if (subject80 != null)
			{
				subject80.DontBury = BoolData;
			}
			break;
		}
		case StoryEventType.SetDontLeaveCommunity:
		{
			Character subject79 = GetSubject<Character>(actor, target, obj, param);
			if (subject79 != null)
			{
				subject79.DontLeaveCommunity = BoolData;
			}
			break;
		}
		case StoryEventType.SetDontAbandonPlayer:
		{
			Character subject78 = GetSubject<Character>(actor, target, obj, param);
			if (subject78 != null)
			{
				subject78.DontAbandonPlayer = BoolData;
			}
			break;
		}
		case StoryEventType.SetPlayerSurrenderDisabled:
		{
			Community subjectCommunity18 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity18 != null)
			{
				subjectCommunity18.PlayerSurrenderDisabled = BoolData;
			}
			break;
		}
		case StoryEventType.SetGod:
		{
			Character subject75 = GetSubject<Character>(actor, target, obj, param);
			if (subject75 != null)
			{
				subject75.God = BoolData;
			}
			break;
		}
		case StoryEventType.SetInvulnerable:
			GetSubject<Prop>(actor, target, obj, param)?.SetForceInvulnerable(BoolData);
			break;
		case StoryEventType.HealAllInjuries:
		{
			Character subject72 = GetSubject<Character>(actor, target, obj, param);
			if (subject72 != null)
			{
				subject72.Injuries.Clear();
				subject72.BloodLoss = 0f;
			}
			break;
		}
		case StoryEventType.SetHelicopterAnimState:
		{
			Helicopter subject69 = GetSubject<Helicopter>(actor, target, obj, param);
			int num16 = Array.IndexOf(Helicopter.HelicopterAnimStateNames, StringData);
			if (subject69 != null && num16 != -1)
			{
				subject69.SetHelicopterAnimState((HelicopterAnimState)num16);
			}
			break;
		}
		case StoryEventType.SetGateState:
		{
			Gate subject68 = GetSubject<Gate>(actor, target, obj, param);
			int num15 = Array.IndexOf(Gate.GateStateNames, StringData);
			if (subject68 == null)
			{
				break;
			}
			switch (num15)
			{
			case 0:
				subject68.Open(null);
				break;
			case 1:
				if (subject68.State == GateState.Locked)
				{
					subject68.Unlock();
				}
				else if (subject68.State == GateState.Open)
				{
					subject68.Close(null);
				}
				break;
			case 2:
				subject68.Lock();
				break;
			}
			break;
		}
		case StoryEventType.SetGifted:
		{
			Equipment subject66 = GetSubject<Equipment>(actor, target, obj, param);
			if (subject66 != null)
			{
				subject66.Gifted = BoolData;
			}
			break;
		}
		case StoryEventType.DontInfectWithInvisibleStrain:
		{
			Character subject62 = GetSubject<Character>(actor, target, obj, param);
			Character dontInfectWithInvisibleStrain = GetObject<Character>(actor, target, obj, param);
			if (subject62 != null)
			{
				subject62.DontInfectWithInvisibleStrain = dontInfectWithInvisibleStrain;
				break;
			}
			Community subjectCommunity16 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity16 == null)
			{
				break;
			}
			{
				foreach (Character member14 in subjectCommunity16.Members)
				{
					member14.DontInfectWithInvisibleStrain = dontInfectWithInvisibleStrain;
				}
				break;
			}
		}
		case StoryEventType.RefusePlayerSurrender:
		{
			Character subject57 = GetSubject<Character>(actor, target, obj, param);
			Character character38 = GetObject<Character>(actor, target, obj, param);
			if (subject57 != null && subject57.Community != null && character38 != null && character38.IsControllableByPlayer())
			{
				subject57.Community.LastRefusedPlayerSurrender = Session.Instance.PlayTime;
				character38.StopActionAnim(ActionAnim.HandsUp);
				HintManager.Instance.LastWarDeclaredTime = Session.Instance.PlayTime;
			}
			break;
		}
		case StoryEventType.SetSquadGoalAchieved:
		{
			Character subject49 = GetSubject<Character>(actor, target, obj, param);
			if (subject49 != null && subject49.Community != null && subject49.SquadId != 0)
			{
				Squad squad4 = subject49.Community.GetSquad(subject49.SquadId);
				squad4.GoalAchieved = true;
				subject49.Community.UpdateSquads();
				if (squad4.Behaviour == SquadBehaviour.Extortion)
				{
					Community targetCommunity = BaseObjectManager.Instance.FindBaseObjectByID(squad4.EnemyCommunityId) as Community;
					subject49.Community.OnExtortion(targetCommunity);
				}
				if (squad4.Behaviour == SquadBehaviour.WarnOffAlliance)
				{
					subject49.Community.NemesisAllyWarningCount = Session.Instance.CommunityManager.PlayerCommunity.GetActiveAllyCount();
				}
				if (squad4.Behaviour == SquadBehaviour.WarnPopulation)
				{
					subject49.Community.NemesisPopulationWarningCount = Session.Instance.CommunityManager.PlayerCommunity.GetActiveMemberCount();
				}
			}
			break;
		}
		case StoryEventType.SetGraveDontDeteriorate:
		{
			Character subject44 = GetSubject<Character>(actor, target, obj, param);
			Grave grave = GetSubject<Grave>(actor, target, obj, param);
			if (subject44 != null)
			{
				grave = subject44.BuriedInGrave;
			}
			if (grave != null)
			{
				grave.DontDeteriorate = BoolData;
			}
			break;
		}
		case StoryEventType.Drop:
		{
			Character subject38 = GetSubject<Character>(actor, target, obj, param);
			if (subject38 != null)
			{
				Character character26 = subject38.CarryingObject as Character;
				if (BoolData && character26 != null && !character26.IsControllableByPlayer() && subject38.IsControllableByPlayer())
				{
					character26.LastRescuedByPlayer = Session.Instance.PlayTime;
				}
				subject38.DropAuthoritative();
			}
			break;
		}
		case StoryEventType.VoteFor:
		{
			Character subject29 = GetSubject<Character>(actor, target, obj, param);
			Character character18 = GetObject<Character>(actor, target, obj, param);
			if (subject29 == null)
			{
				break;
			}
			subject29.VotedFor = character18;
			if (subject29.Community != null && subject29.Community.HasElectionWinner(out var winner, out var runnerUp))
			{
				Character leader = subject29.Community.Leader;
				winner.SetRank(Rank.Leader);
				if (leader != null && leader.AvatarForPlayer.IsValid() && !winner.AvatarForPlayer.IsValid())
				{
					winner.AvatarForPlayer = leader.AvatarForPlayer;
					leader.AvatarForPlayer = default(PlayerID);
					winner.SetAllPersonalitiesKnown();
					Relationship.SetAllRelationshipsKnown(winner);
				}
				Character target2 = ((character18 == winner) ? null : character18);
				Speech speechForSituation = StoryManager.Instance.GetSpeechForSituation(winner, target2, runnerUp, SpeechSituation.ElectionVictory, new MemoryParam(leader));
				if (speechForSituation != null)
				{
					winner.PushQueuedSpeech(speechForSituation, target2, runnerUp, new MemoryParam(leader));
				}
			}
			break;
		}
		case StoryEventType.SetPreferredPlayerRescuer:
			Session.Instance.SetPreferredPlayerRescuer(GetSubjectCommunity(actor, target, obj, param));
			break;
		case StoryEventType.SetFollowerCommandsEnabled:
			Session.Instance.FollowerCommandsEnabled = BoolData;
			break;
		case StoryEventType.SetLimitBloodLoss:
		{
			Character subject28 = GetSubject<Character>(actor, target, obj, param);
			if (subject28 != null)
			{
				subject28.LimitBloodLoss = Data;
			}
			break;
		}
		case StoryEventType.PlayerSurrender:
		{
			Character subject25 = GetSubject<Character>(actor, target, obj, param);
			Character character15 = GetObject<Character>(actor, target, obj, param);
			if (subject25 != null && character15 != null && subject25.IsControllableByPlayer())
			{
				subject25.StartPlayerSurrendering(character15, null);
				subject25.DirectControlledMajorAIDisabled = true;
				subject25.DirectControlled = false;
				subject25.DirectControlledCrouching = false;
				subject25.DisableInputsUntilNextThink = true;
			}
			break;
		}
		case StoryEventType.CancelPlayerSurrender:
		{
			Character subject22 = GetSubject<Character>(actor, target, obj, param);
			if (subject22 != null && subject22.IsControllableByPlayer())
			{
				subject22.CancelSurrendering();
			}
			break;
		}
		case StoryEventType.OccupyBase:
		{
			Community subjectCommunity9 = GetSubjectCommunity(actor, target, obj, param);
			Community objectCommunity6 = GetObjectCommunity(actor, target, obj, param);
			if (subjectCommunity9 != null && objectCommunity6 != null)
			{
				subjectCommunity9.OccupyBase(objectCommunity6);
			}
			break;
		}
		case StoryEventType.SetInitialMemberCount:
		{
			Community subjectCommunity8 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity8 != null)
			{
				subjectCommunity8.InitialMemberCount = (int)Data;
			}
			break;
		}
		case StoryEventType.Explode:
		{
			TileObject subject16 = GetSubject<TileObject>(actor, target, obj, param);
			Character character8 = GetObject<Character>(actor, target, obj, param);
			Character thirdParty3 = GetThirdParty<Character>(actor, target, obj, param);
			if (subject16 != null)
			{
				PipeBombProjectile.Splosion(subject16.Pos + Vector3.up * subject16.Height * 0.5f, subject16, character8, BoolData ? character8 : null, thirdParty3, Data, SkillType.Invalid, InfectionType.None, Data2, predicted: false, frontmost: true, Secrecy, itsATrap: true, isFromAI: false);
			}
			break;
		}
		case StoryEventType.SetRoadEntranceEnabled:
		{
			int pathIndexByName2 = GameTerrain.Instance.GetPathIndexByName(StringData);
			if (pathIndexByName2 != -1)
			{
				GameTerrain.Instance.SetRoadEntranceEnabled(pathIndexByName2, BoolData);
			}
			break;
		}
		case StoryEventType.SetHitTheRoadEnabled:
		{
			int pathIndexByName = GameTerrain.Instance.GetPathIndexByName(StringData);
			if (pathIndexByName != -1)
			{
				GameTerrain.Instance.SetHitTheRoadEnabled(pathIndexByName, BoolData);
			}
			break;
		}
		case StoryEventType.SetSkinnedAmount:
		{
			Character subject12 = GetSubject<Character>(actor, target, obj, param);
			if (subject12 != null)
			{
				subject12.SkinnedAmount = Data;
				break;
			}
			Community subjectCommunity5 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity5 == null)
			{
				break;
			}
			{
				foreach (Character member15 in subjectCommunity5.Members)
				{
					member15.SkinnedAmount = Data;
				}
				break;
			}
		}
		case StoryEventType.SetSurrenderMode:
		{
			Character subject10 = GetSubject<Character>(actor, target, obj, param);
			int surrenderMode = Math.Max(0, Array.IndexOf(Character.SurrenderModeNames, StringData));
			if (subject10 != null)
			{
				subject10.SurrenderMode = (SurrenderMode)surrenderMode;
			}
			break;
		}
		case StoryEventType.SetSurrenderThreshold:
		{
			Community subjectCommunity4 = GetSubjectCommunity(actor, target, obj, param);
			if (subjectCommunity4 != null)
			{
				subjectCommunity4.SurrenderThreshold = (int)Data;
			}
			break;
		}
		case StoryEventType.RefreshTarget:
		{
			Character subject3 = GetSubject<Character>(actor, target, obj, param);
			Community subjectCommunity2 = GetSubjectCommunity(actor, target, obj, param);
			TileObject tileObject2 = GetObject<TileObject>(actor, target, obj, param);
			Community objectCommunity = GetObjectCommunity(actor, target, obj, param);
			if (subject3 != null)
			{
				if (tileObject2 != null)
				{
					subject3.RefreshTarget(tileObject2);
				}
				else
				{
					if (objectCommunity == null)
					{
						break;
					}
					{
						foreach (Character member16 in objectCommunity.Members)
						{
							subject3.RefreshTarget(member16);
						}
						break;
					}
				}
			}
			else
			{
				if (subjectCommunity2 == null)
				{
					break;
				}
				{
					foreach (Character member17 in subjectCommunity2.Members)
					{
						if (!member17.AliveAndNotZombie)
						{
							continue;
						}
						if (tileObject2 != null)
						{
							member17.RefreshTarget(tileObject2);
						}
						else
						{
							if (objectCommunity == null)
							{
								continue;
							}
							foreach (Character member18 in objectCommunity.Members)
							{
								member17.RefreshTarget(member18);
							}
						}
					}
					break;
				}
			}
			break;
		}
		case StoryEventType.TriggerInvaders:
			Session.Instance.CommunityManager.NumFreebieInvaders += (int)Data;
			break;
		case StoryEventType.UnlockAchievement:
		{
			int num2 = Array.IndexOf(AchievementsManager.AchievementNames, StringData);
			if (num2 != -1)
			{
				AchievementsManager.Instance.UnlockAchievement((Achievement)num2);
			}
			break;
		}
		case StoryEventType.SetIgnoreForQuests:
		{
			Town subject = GetSubject<Town>(actor, target, obj, param);
			if (subject != null)
			{
				subject.IgnoreForQuests = BoolData;
			}
			break;
		}
		}
	}

	private static void SortAndRemoveDuplicates(List<ObjScore> results)
	{
		results.Sort();
		for (int num = results.Count - 2; num >= 0; num--)
		{
			if (results[num].Obj == results[num + 1].Obj)
			{
				results.RemoveAt(num);
			}
		}
	}

	private static void GetNearbyTargetCommunityMembersAndBuildings(Character taker, Character giver, float range, List<ObjScore> results)
	{
		Community community = giver.Community;
		foreach (Prop building in community.Buildings)
		{
			float magnitude = (building.PosXZ - taker.PosXZ).magnitude;
			if (magnitude <= range)
			{
				results.Add(new ObjScore(building, magnitude));
			}
		}
		foreach (Character member in community.Members)
		{
			if (!member.AliveAndNotZombie || member == giver)
			{
				continue;
			}
			float magnitude2 = (member.PosXZ - taker.PosXZ).magnitude;
			if (!(magnitude2 <= range))
			{
				continue;
			}
			if (member.IsCrouching() && member != taker)
			{
				Target target = taker.GetTarget(member);
				if (target == null || !target.FullyTracked)
				{
					continue;
				}
			}
			results.Add(new ObjScore(member, magnitude2));
		}
		SortAndRemoveDuplicates(results);
	}

	private static void GetAllCommunityMembersAndBuildings(Character taker, Character giver, List<ObjScore> results)
	{
		Community community = giver.Community;
		if (community == null)
		{
			return;
		}
		foreach (Prop building in community.Buildings)
		{
			float magnitude = (building.PosXZ - taker.PosXZ).magnitude;
			results.Add(new ObjScore(building, magnitude));
		}
		foreach (Character member in community.Members)
		{
			if (member.AliveAndNotZombie && member != giver)
			{
				float magnitude2 = (member.PosXZ - taker.PosXZ).magnitude;
				results.Add(new ObjScore(member, magnitude2));
			}
		}
		SortAndRemoveDuplicates(results);
	}

	public static bool CanLootItem(TileObject source, Equipment item)
	{
		if (item.IsWorn(source) || source.CanTransferEquipmentAway(item) == CantTransferReason.WantToKeepMe)
		{
			return false;
		}
		if (item.GetLiquidCapacity() > 0f && (item.GetLiquidContentsType() == null || item.GetLiquidContentsType() == LiquidPrototype.Water))
		{
			return false;
		}
		if (source is Character character && !character.IsControllableByPlayer())
		{
			if (item is Weapon)
			{
				return false;
			}
			if (item.GetSeedForPlantType() != null)
			{
				return false;
			}
			if (item.GetPrototype() == EquipmentPrototype.Flint || item.GetPrototype() == EquipmentPrototype.Match)
			{
				return false;
			}
		}
		return true;
	}

	public static float GiveAllSupplies(Character giver, Character taker, float desiredValue, bool isShakedown, bool fromBystanders, bool fromEveryone, Type type)
	{
		List<ObjScore> list = new List<ObjScore>();
		list.Add(new ObjScore(giver, 0f));
		if (fromBystanders)
		{
			GetNearbyTargetCommunityMembersAndBuildings(taker, giver, NearbyDist, list);
		}
		if (fromEveryone)
		{
			GetAllCommunityMembersAndBuildings(taker, giver, list);
		}
		List<EquipmentScore> list2 = new List<EquipmentScore>();
		for (int i = 0; i < list.Count; i++)
		{
			TileObject obj = list[i].Obj;
			foreach (Equipment content in obj.GetInventory().Contents)
			{
				if ((type != null && !content.GetType().IsA(type)) || !CanLootItem(obj, content))
				{
					continue;
				}
				float num = content.GetBasePrice() + content.GetWeight() - (float)content.GetAmount() + list[i].Score;
				if (content.GetPrototype() == EquipmentPrototype.Gold)
				{
					num = ((!giver.IsControllableByPlayer()) ? (num - 1000f) : (num + (1000f + (float)content.GetAmount())));
				}
				if (!giver.IsActionAllowedForItem(content, EquipmentPolicyAction.CanShare))
				{
					if (!isShakedown)
					{
						continue;
					}
					num += 1000000f;
				}
				list2.Add(new EquipmentScore(content, num));
			}
		}
		list2.Sort();
		float num2 = 0f;
		for (int j = 0; j < list2.Count; j++)
		{
			if (!(num2 < desiredValue))
			{
				break;
			}
			Equipment item = list2[j].Item;
			TileObject inventoryOwner = item.InventoryOwner;
			TileObject tileObject = taker;
			if (!tileObject.HasInventorySpaceFor(item.GetWeight()))
			{
				tileObject = taker.Community.GetNearestMemberOrBuildingWithInventorySpaceFor(taker.Tile, taker, BaseObjectType.Human, float.MaxValue, item.GetWeight());
			}
			int num3 = Math.Min(item.GetAmount(), Math.Max(1, Mathf.CeilToInt((desiredValue - num2) / item.GetBasePrice())));
			if (tileObject != null)
			{
				num3 = Math.Max(1, Math.Min(num3, (int)(tileObject.GetAvailableInventorySpace() / item.GetWeight())));
			}
			if (num3 < item.GetAmount())
			{
				j--;
			}
			num2 += (float)num3 * item.GetBasePrice();
			NotificationManager.Instance.AddEquipmentNotification(inventoryOwner, tileObject, item, num3);
			Equipment equipment = inventoryOwner.GetInventory().Take(inventoryOwner, item, num3);
			if (tileObject != null)
			{
				equipment = tileObject.GetInventory().Add(tileObject, equipment);
				if (tileObject is Character character && !character.IsControllableByPlayer() && !character.IsAmbient() && character.GetGatherGoal() != null)
				{
					equipment.IncrementGatheredAmount(num3);
					character.AddRole(new RoleInfo(Role.Organizer));
				}
			}
			else
			{
				equipment.Delete();
			}
		}
		if (taker.Community != null && isShakedown)
		{
			taker.Community.OnShakedown(giver, num2);
		}
		return num2;
	}

	public static int GiveAllGold(Character giver, Character taker, float desiredAmount, bool isShakedown, bool fromBystanders, bool fromEveryone)
	{
		List<ObjScore> list = new List<ObjScore>();
		list.Add(new ObjScore(giver, 0f));
		if (fromBystanders)
		{
			GetNearbyTargetCommunityMembersAndBuildings(taker, giver, NearbyDist, list);
		}
		if (fromEveryone)
		{
			GetAllCommunityMembersAndBuildings(taker, giver, list);
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (!((float)num < desiredAmount))
			{
				break;
			}
			TileObject obj = list[i].Obj;
			Equipment gold = obj.GetInventory().GetGold();
			while (gold != null && (float)num < desiredAmount)
			{
				TileObject tileObject = taker;
				if (!tileObject.HasInventorySpaceFor(gold.GetWeight()))
				{
					tileObject = taker.Community.GetNearestMemberOrBuildingWithInventorySpaceFor(taker.Tile, taker, BaseObjectType.Human, float.MaxValue, gold.GetWeight());
				}
				int num2 = gold.GetAmount();
				if (tileObject != null)
				{
					num2 = Math.Min(num2, Math.Max(1, (int)(tileObject.GetAvailableInventorySpace() / gold.GetWeight())));
				}
				if (desiredAmount != float.MaxValue)
				{
					num2 = Math.Min(num2, Mathf.CeilToInt(desiredAmount) - num);
				}
				NotificationManager.Instance.AddEquipmentNotification(obj, tileObject, gold, num2);
				gold = obj.GetInventory().Take(obj, gold, num2);
				if (tileObject != null)
				{
					gold = tileObject.GetInventory().Add(tileObject, gold);
				}
				else
				{
					gold.Delete();
				}
				num += num2;
				gold = obj.GetInventory().GetGold();
			}
		}
		if (taker.Community != null && isShakedown)
		{
			taker.Community.OnShakedown(giver, num);
		}
		return num;
	}
}
