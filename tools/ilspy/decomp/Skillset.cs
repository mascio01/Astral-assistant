using System;
using System.Collections.Generic;
using UnityEngine;

public struct Skillset : IReflectable
{
	public class SortCharactersBySkillAscending : IComparer<Character>
	{
		private SkillType SkillType;

		public SortCharactersBySkillAscending(SkillType skillType)
		{
			SkillType = skillType;
		}

		int IComparer<Character>.Compare(Character a, Character b)
		{
			int level = a.Skillset.GetLevel(SkillType);
			int level2 = b.Skillset.GetLevel(SkillType);
			if (level > level2)
			{
				return 1;
			}
			if (level < level2)
			{
				return -1;
			}
			if (a.Id > b.Id)
			{
				return 1;
			}
			if (a.Id < b.Id)
			{
				return -1;
			}
			return 0;
		}
	}

	public static string[] SkillNames = StringUtil.GetEnumNames<SkillType>("Invalid");

	public const int Cap = 5;

	public const float Capf = 5f;

	public static float[] ProgressionToLevel = new float[7] { 0f, 50f, 150f, 500f, 1500f, 5000f, 10000f };

	public static int HUD_Strength = StringUtil.JenkinsHash("HUD_Strength");

	public static int HUD_HandToHand = StringUtil.JenkinsHash("HUD_HandToHand");

	public static int HUD_Archery = StringUtil.JenkinsHash("HUD_Archery");

	public static int HUD_Firearms = StringUtil.JenkinsHash("HUD_Firearms");

	public static int HUD_Stealth = StringUtil.JenkinsHash("HUD_Stealth");

	public static int HUD_Construction = StringUtil.JenkinsHash("HUD_Construction");

	public static int HUD_Farming = StringUtil.JenkinsHash("HUD_Farming");

	public static int HUD_Medicine = StringUtil.JenkinsHash("HUD_Medicine");

	public static int HUD_Cooking = StringUtil.JenkinsHash("HUD_Cooking");

	public static int HUD_Constitution = StringUtil.JenkinsHash("HUD_Constitution");

	public int Strength;

	public int HandToHand;

	public int Archery;

	public int Firearms;

	public int Stealth;

	public int Construction;

	public int Farming;

	public int Medicine;

	public int Cooking;

	public int Constitution;

	public float StrengthProgression;

	public float HandToHandProgression;

	public float ArcheryProgression;

	public float FirearmsProgression;

	public float StealthProgression;

	public float ConstructionProgression;

	public float FarmingProgression;

	public float MedicineProgression;

	public float CookingProgression;

	public float ConstitutionProgression;

	public int StrengthCap;

	public int HandToHandCap;

	public int ArcheryCap;

	public int FirearmsCap;

	public int StealthCap;

	public int ConstructionCap;

	public int FarmingCap;

	public int MedicineCap;

	public int CookingCap;

	public int ConstitutionCap;

	public short SkillKnown;

	public static int MinAICookingSkill = 2;

	public static int MinAIFarmerSkill = 3;

	public static int MinAIEnforcerCombatSkill = 2;

	public static int HUD_LevelUp = StringUtil.JenkinsHash("HUD_LevelUp");

	public static int HUD_LevelDown = StringUtil.JenkinsHash("HUD_LevelDown");

	public static int HUD_ReachedLevel = StringUtil.JenkinsHash("HUD_ReachedLevel");

	public static int HUD_ReducedLevel = StringUtil.JenkinsHash("HUD_ReducedLevel");

	public static SortCharactersBySkillAscending SortCharactersByFarmingSkillAscending = new SortCharactersBySkillAscending(SkillType.Farming);

	public static SortCharactersBySkillAscending SortCharactersByCookingSkillAscending = new SortCharactersBySkillAscending(SkillType.Cooking);

	public static SortCharactersBySkillAscending SortCharactersByBuildingSkillAscending = new SortCharactersBySkillAscending(SkillType.Construction);

	public static int GetSkillNameHash(SkillType type)
	{
		return type switch
		{
			SkillType.Strength => HUD_Strength, 
			SkillType.HandToHand => HUD_HandToHand, 
			SkillType.Archery => HUD_Archery, 
			SkillType.Firearms => HUD_Firearms, 
			SkillType.Stealth => HUD_Stealth, 
			SkillType.Construction => HUD_Construction, 
			SkillType.Farming => HUD_Farming, 
			SkillType.Medicine => HUD_Medicine, 
			SkillType.Cooking => HUD_Cooking, 
			SkillType.Constitution => HUD_Constitution, 
			_ => 0, 
		};
	}

	public int GetLevel(SkillType type)
	{
		return type switch
		{
			SkillType.Strength => Strength, 
			SkillType.HandToHand => HandToHand, 
			SkillType.Archery => Archery, 
			SkillType.Firearms => Firearms, 
			SkillType.Stealth => Stealth, 
			SkillType.Construction => Construction, 
			SkillType.Farming => Farming, 
			SkillType.Medicine => Medicine, 
			SkillType.Cooking => Cooking, 
			SkillType.Constitution => Constitution, 
			_ => 0, 
		};
	}

	public void SetLevel(Character character, SkillType type, int level)
	{
		switch (type)
		{
		case SkillType.Strength:
			Strength = level;
			StrengthProgression = ProgressionToLevel[level];
			character.LinkMuscleToStrengthSkill();
			break;
		case SkillType.HandToHand:
			HandToHand = level;
			HandToHandProgression = ProgressionToLevel[level];
			break;
		case SkillType.Archery:
			Archery = level;
			ArcheryProgression = ProgressionToLevel[level];
			break;
		case SkillType.Firearms:
			Firearms = level;
			FirearmsProgression = ProgressionToLevel[level];
			break;
		case SkillType.Stealth:
			Stealth = level;
			StealthProgression = ProgressionToLevel[level];
			break;
		case SkillType.Construction:
			Construction = level;
			ConstructionProgression = ProgressionToLevel[level];
			break;
		case SkillType.Farming:
			Farming = level;
			FarmingProgression = ProgressionToLevel[level];
			break;
		case SkillType.Medicine:
			Medicine = level;
			MedicineProgression = ProgressionToLevel[level];
			break;
		case SkillType.Cooking:
			Cooking = level;
			CookingProgression = ProgressionToLevel[level];
			break;
		case SkillType.Constitution:
			Constitution = level;
			ConstitutionProgression = ProgressionToLevel[level];
			break;
		}
		character.ClearCachedSkillLevelWithEffects(type);
		if (Session.Instance != null)
		{
			Session.Instance.UpdateHighestSkillLevel(character);
		}
	}

	private static bool IsCombatSkill(SkillType skillType)
	{
		if ((uint)skillType <= 3u || skillType == SkillType.Constitution)
		{
			return true;
		}
		return false;
	}

	public void Randomize(Character character, CustomRandom rand)
	{
		for (int i = 0; i < 10; i++)
		{
			SkillType skillType = (SkillType)i;
			int num = 5;
			int num2 = 0;
			int num3 = 5;
			if (skillType == SkillType.Strength)
			{
				if (character.Appearance.Gender == GenderType.Female)
				{
					num--;
				}
				if (character.Appearance.Age >= 65f)
				{
					num--;
				}
			}
			for (int j = 0; j < character.Roles.Count; j++)
			{
				switch (character.Roles[j].Role)
				{
				case Role.Cook:
					if (skillType == SkillType.Cooking)
					{
						num2 = MinAICookingSkill;
					}
					break;
				case Role.Farmer:
					if (skillType == SkillType.Farming)
					{
						num2 = MinAIFarmerSkill;
					}
					break;
				case Role.Guard:
				case Role.Enforcer:
					if (IsCombatSkill(skillType))
					{
						num2 = MinAIEnforcerCombatSkill;
					}
					break;
				}
			}
			if (character.Community != null && character.Community.Nemesis && IsCombatSkill(skillType))
			{
				num2++;
			}
			if (character.Rank == Rank.Leader && IsCombatSkill(skillType))
			{
				num2++;
			}
			if (skillType == SkillType.Constitution)
			{
				if (character.Community != null)
				{
					CommunityType communityType = character.Community.CommunityType;
					if ((uint)(communityType - 4) <= 4u || (uint)(communityType - 10) <= 4u)
					{
						num3 = 0;
					}
				}
				if (character.Appearance.Age >= 65f)
				{
					num--;
				}
			}
			num2 = Math.Min(num2, num3);
			int minValue = Math.Max(1, num2);
			SetCap(character, skillType, rand.Next(minValue, num + 1));
			SetLevel(character, skillType, rand.Next(num2, Math.Min(num3, GetCap(skillType)) + 1));
		}
		if (character.HasRole(Role.Farmer) && Farming == 0)
		{
			Debug.LogError("wtf?");
		}
	}

	public void Reset(Character character)
	{
		for (int i = 0; i < 10; i++)
		{
			SkillType type = (SkillType)i;
			SetProgression(type, 0f);
			SetLevel(character, type, 0);
			SetCap(character, type, 0);
		}
	}

	public void EnsureMinLevel(Character character, SkillType skillType, int minLevel)
	{
		SetCap(character, skillType, Math.Max(minLevel, GetCap(skillType)));
		SetLevel(character, skillType, Math.Max(minLevel, GetLevel(skillType)));
	}

	public float GetProgression(SkillType type)
	{
		return type switch
		{
			SkillType.Strength => StrengthProgression, 
			SkillType.HandToHand => HandToHandProgression, 
			SkillType.Archery => ArcheryProgression, 
			SkillType.Firearms => FirearmsProgression, 
			SkillType.Stealth => StealthProgression, 
			SkillType.Construction => ConstructionProgression, 
			SkillType.Farming => FarmingProgression, 
			SkillType.Medicine => MedicineProgression, 
			SkillType.Cooking => CookingProgression, 
			SkillType.Constitution => ConstitutionProgression, 
			_ => 0f, 
		};
	}

	public int GetCap(SkillType type)
	{
		return type switch
		{
			SkillType.Strength => StrengthCap, 
			SkillType.HandToHand => HandToHandCap, 
			SkillType.Archery => ArcheryCap, 
			SkillType.Firearms => FirearmsCap, 
			SkillType.Stealth => StealthCap, 
			SkillType.Construction => ConstructionCap, 
			SkillType.Farming => FarmingCap, 
			SkillType.Medicine => MedicineCap, 
			SkillType.Cooking => CookingCap, 
			SkillType.Constitution => ConstitutionCap, 
			_ => 0, 
		};
	}

	public void SetCap(Character character, SkillType type, int cap)
	{
		switch (type)
		{
		case SkillType.Strength:
			StrengthCap = cap;
			break;
		case SkillType.HandToHand:
			HandToHandCap = cap;
			break;
		case SkillType.Archery:
			ArcheryCap = cap;
			break;
		case SkillType.Firearms:
			FirearmsCap = cap;
			break;
		case SkillType.Stealth:
			StealthCap = cap;
			break;
		case SkillType.Construction:
			ConstructionCap = cap;
			break;
		case SkillType.Farming:
			FarmingCap = cap;
			break;
		case SkillType.Medicine:
			MedicineCap = cap;
			break;
		case SkillType.Cooking:
			CookingCap = cap;
			break;
		case SkillType.Constitution:
			ConstitutionCap = cap;
			break;
		}
		SetLevel(character, type, Math.Min(GetLevel(type), GetCap(type)));
	}

	public bool IsSkillKnown(SkillType skillType)
	{
		return (SkillKnown & (short)(1 << (int)skillType)) != 0;
	}

	public bool HasAnyUnknownSkills()
	{
		for (int i = 0; i < 10; i++)
		{
			if (!IsSkillKnown((SkillType)i))
			{
				return true;
			}
		}
		return false;
	}

	public void ClearSkillKnown(SkillType skillType)
	{
		SkillKnown &= (short)(~(1 << (int)skillType));
	}

	public void SetSkillKnown(SkillType skillType)
	{
		SkillKnown |= (short)(1 << (int)skillType);
	}

	public void SetAllSkillsKnown()
	{
		SkillKnown = short.MaxValue;
	}

	public bool IsAtMaxCaps()
	{
		for (int i = 0; i < 10; i++)
		{
			if (GetCap((SkillType)i) < 5)
			{
				return false;
			}
		}
		return true;
	}

	public void AddProgress(Character character, SkillType type, float progress)
	{
		if (character.GetBaseObjectType() != BaseObjectType.Human || character.Zombie)
		{
			return;
		}
		switch (type)
		{
		case SkillType.Strength:
			StrengthProgression += progress;
			break;
		case SkillType.HandToHand:
			HandToHandProgression += progress;
			break;
		case SkillType.Archery:
			ArcheryProgression += progress;
			break;
		case SkillType.Firearms:
			FirearmsProgression += progress;
			break;
		case SkillType.Stealth:
			StealthProgression += progress;
			break;
		case SkillType.Construction:
			ConstructionProgression += progress;
			break;
		case SkillType.Farming:
			FarmingProgression += progress;
			break;
		case SkillType.Medicine:
			MedicineProgression += progress;
			break;
		case SkillType.Cooking:
			CookingProgression += progress;
			break;
		case SkillType.Constitution:
			ConstitutionProgression += progress;
			break;
		}
		int level = GetLevel(type);
		int i;
		for (i = level; i < GetCap(type) && GetProgression(type) >= ProgressionToLevel[i + 1]; i++)
		{
		}
		while (i > 0 && GetProgression(type) < ProgressionToLevel[i])
		{
			i--;
		}
		if (level == i)
		{
			return;
		}
		switch (type)
		{
		case SkillType.Strength:
			Strength = i;
			character.LinkMuscleToStrengthSkill();
			break;
		case SkillType.HandToHand:
			HandToHand = i;
			break;
		case SkillType.Archery:
			Archery = i;
			break;
		case SkillType.Firearms:
			Firearms = i;
			break;
		case SkillType.Stealth:
			Stealth = i;
			break;
		case SkillType.Construction:
			Construction = i;
			break;
		case SkillType.Farming:
			Farming = i;
			break;
		case SkillType.Medicine:
			Medicine = i;
			break;
		case SkillType.Cooking:
			Cooking = i;
			break;
		case SkillType.Constitution:
			Constitution = i;
			break;
		}
		if (character.IsAuthoritative())
		{
			character.ClearCachedSkillLevelWithEffects(type);
			if (character.Community == Session.Instance.CommunityManager.PlayerCommunity)
			{
				int level2 = GetLevel(type);
				string statusBarMsg = ((level > i) ? LogEventBehaviour.BuildCommunityMemberLevelledDownMsg(character, type, level2) : LogEventBehaviour.BuildCommunityMemberLevelledUpMsg(character, type, level2));
				HudBehaviour.Instance.SetStatusBarMsg(statusBarMsg);
				LogEvent logEvent = new LogEvent((level > i) ? LogEventType.CommunityMemberLevelledDown : LogEventType.CommunityMemberLevelledUp);
				logEvent.Character = character;
				logEvent.SkillLevel = level2;
				logEvent.SkillType = type;
				Session.Instance.AddLogEvent(logEvent);
			}
			StoryManager.Instance.SetConditionsDirty();
			Session.Instance.UpdateHighestSkillLevel(character);
		}
	}

	public void SetProgression(SkillType type, float progress)
	{
		switch (type)
		{
		case SkillType.Strength:
			StrengthProgression = progress;
			break;
		case SkillType.HandToHand:
			HandToHandProgression = progress;
			break;
		case SkillType.Archery:
			ArcheryProgression = progress;
			break;
		case SkillType.Firearms:
			FirearmsProgression = progress;
			break;
		case SkillType.Stealth:
			StealthProgression = progress;
			break;
		case SkillType.Construction:
			ConstructionProgression = progress;
			break;
		case SkillType.Farming:
			FarmingProgression = progress;
			break;
		case SkillType.Medicine:
			MedicineProgression = progress;
			break;
		case SkillType.Cooking:
			CookingProgression = progress;
			break;
		case SkillType.Constitution:
			ConstitutionProgression = progress;
			break;
		}
	}

	public void Reflect(Reflector reflector)
	{
		reflector.OnEnterObject();
		reflector.Add(ref Strength);
		reflector.Add(ref HandToHand);
		reflector.Add(ref Archery);
		reflector.Add(ref Firearms);
		reflector.Add(ref Stealth);
		reflector.Add(ref Construction);
		reflector.Add(ref Farming);
		reflector.Add(ref Medicine);
		reflector.AddAfter(ref Cooking, 20);
		reflector.AddAfter(ref Constitution, 346);
		reflector.Add(ref StrengthProgression);
		reflector.Add(ref HandToHandProgression);
		reflector.Add(ref ArcheryProgression);
		reflector.Add(ref FirearmsProgression);
		reflector.Add(ref StealthProgression);
		reflector.Add(ref ConstructionProgression);
		reflector.Add(ref FarmingProgression);
		reflector.Add(ref MedicineProgression);
		reflector.AddAfter(ref CookingProgression, 20);
		reflector.AddAfter(ref ConstitutionProgression, 346);
		reflector.Add(ref StrengthCap);
		reflector.Add(ref HandToHandCap);
		reflector.Add(ref ArcheryCap);
		reflector.Add(ref FirearmsCap);
		reflector.Add(ref StealthCap);
		reflector.Add(ref ConstructionCap);
		reflector.Add(ref FarmingCap);
		reflector.Add(ref MedicineCap);
		reflector.AddAfter(ref CookingCap, 20);
		reflector.AddAfter(ref ConstitutionCap, 346);
		reflector.AddAfter(ref SkillKnown, 398);
		if (!Session.Instance.Editor && reflector.IsDeserialising)
		{
			MakeSureProgressionMatchesLevel();
		}
		reflector.OnExitObject();
	}

	public void MakeSureProgressionMatchesLevel()
	{
		StrengthProgression = Math.Max(StrengthProgression, ProgressionToLevel[Strength]);
		HandToHandProgression = Math.Max(HandToHandProgression, ProgressionToLevel[HandToHand]);
		FirearmsProgression = Math.Max(FirearmsProgression, ProgressionToLevel[Firearms]);
		ArcheryProgression = Math.Max(ArcheryProgression, ProgressionToLevel[Archery]);
		StealthProgression = Math.Max(StealthProgression, ProgressionToLevel[Stealth]);
		ConstructionProgression = Math.Max(ConstructionProgression, ProgressionToLevel[Construction]);
		FarmingProgression = Math.Max(FarmingProgression, ProgressionToLevel[Farming]);
		MedicineProgression = Math.Max(MedicineProgression, ProgressionToLevel[Medicine]);
		CookingProgression = Math.Max(CookingProgression, ProgressionToLevel[Cooking]);
		ConstitutionProgression = Math.Max(ConstitutionProgression, ProgressionToLevel[Constitution]);
	}
}
