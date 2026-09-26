using System;
using System.Collections.Generic;

public class CrewMemberSettings : IComparable<CrewMemberSettings>
{
	public string FirstName = "";

	public string Surname = "";

	public PlayerID AvatarForPlayer;

	public CharacterAppearance Appearance = new HumanAppearance();

	public List<Equipment> Inventory = new List<Equipment>();

	public int[] Skills = new int[10];

	public int[] SkillLimits = new int[10];

	public float[] SkillProgression = new float[10];

	public List<string> Personality = new List<string>();

	public uint PersonalityKnown = uint.MaxValue;

	public List<CrewMemory> Memories = new List<CrewMemory>();

	public List<CrewRelationship> Relationships = new List<CrewRelationship>();

	public List<EquipmentPolicy> CharacterEquipmentPolicies = new List<EquipmentPolicy>();

	public InvisibleStrainType InvisibleStrain;

	public int OriginalId;

	private int SortIndex;

	private static int NextFreeSortIndex = 1;

	public HumanAppearance GetHumanAppearance()
	{
		return (HumanAppearance)Appearance;
	}

	public CrewMemberSettings()
	{
		SortIndex = NextFreeSortIndex++;
	}

	public int CompareTo(CrewMemberSettings other)
	{
		if (SortIndex < other.SortIndex)
		{
			return -1;
		}
		if (SortIndex > other.SortIndex)
		{
			return 1;
		}
		return 0;
	}

	public void SetupFromCharacter(Character character)
	{
		FirstName = character.FirstName;
		Surname = character.Surname;
		AvatarForPlayer = character.AvatarForPlayer;
		Appearance = character.Appearance;
		character.Personality.CopyToList(Personality);
		PersonalityKnown = character.PersonalityKnown;
		InvisibleStrain = character.InvisibleStrain;
		OriginalId = ((character.OriginalId != 0) ? character.OriginalId : character.Id);
		character.Inventory.CopyToList(Inventory, stripItemsThatCantTravel: true);
		character.EquipmentPolicies.CopyToList(CharacterEquipmentPolicies);
		SkillsChangePopup skillsChangePopup = Session.Instance.FindSkillsChangePopupForCharacter(character);
		for (int i = 0; i < 10; i++)
		{
			Skills[i] = character.Skillset.GetLevel((SkillType)i);
			SkillLimits[i] = character.Skillset.GetCap((SkillType)i);
			SkillProgression[i] = character.Skillset.GetProgression((SkillType)i);
			if (skillsChangePopup != null)
			{
				int num = skillsChangePopup.FindSkillChange((SkillType)i);
				if (num != -1)
				{
					SkillLimits[i] = skillsChangePopup.SkillChanges[num].NewCap;
				}
			}
		}
	}

	public void SetupMemoriesAndRelationships(Character character, Dictionary<Character, CrewMemberSettings> crewMembers, float travelTime)
	{
		for (int i = 0; i < character.Relationships.Count; i++)
		{
			if (crewMembers.TryGetValue(character.Relationships[i].RelationshipTarget, out var value))
			{
				CrewRelationship item = new CrewRelationship
				{
					RelationshipTarget = value,
					RelationshipType = character.Relationships[i].RelationshipType,
					ApprovalContribution = character.Relationships[i].ApprovalContribution,
					RespectContribution = character.Relationships[i].RespectContribution,
					KnownToPlayer = character.Relationships[i].KnownToPlayer
				};
				Relationships.Add(item);
			}
		}
		for (int j = 0; j < character.Memories.Count; j++)
		{
			Memory memory = character.Memories[j];
			CrewMemberSettings value2 = null;
			CrewMemberSettings value3 = null;
			CrewMemberSettings value4 = null;
			TimeSpan timeSpan = Session.Instance.PlayTime - memory.Time + TimeSpan.FromSeconds(travelTime);
			if (memory.Actor != null && !crewMembers.TryGetValue(memory.Actor, out value2))
			{
				int num = -1;
				for (int k = 0; k < Memories.Count; k++)
				{
					if (Memories[k].Prototype == MemoryPrototype.Past)
					{
						num = k;
						break;
					}
				}
				if (num == -1)
				{
					CrewMemory item2 = default(CrewMemory);
					item2.Prototype = MemoryPrototype.Past;
					item2.LastQuantity = (item2.QuantityFactor = 1f);
					item2.TimeAgo = timeSpan;
					num = Memories.Count;
					Memories.Add(item2);
				}
				CrewMemory value5 = Memories[num];
				value5.MoraleContribution += memory.MoraleContribution;
				value5.TimeAgo = MathUtil.Min(timeSpan, value5.TimeAgo);
				Memories[num] = value5;
			}
			else if ((memory.Object is Character && !crewMembers.TryGetValue(memory.Object as Character, out value3)) || (memory.ThirdParty is Character && !crewMembers.TryGetValue(memory.ThirdParty as Character, out value4)) || memory.Object is Community)
			{
				MemoryPrototype memoryPrototype = ((memory.Actor == null) ? MemoryPrototype.Past : ((memory.Actor == character) ? MemoryPrototype.Regrets : MemoryPrototype.History));
				int num2 = -1;
				for (int l = 0; l < Memories.Count; l++)
				{
					if (Memories[l].Prototype == memoryPrototype && Memories[l].Actor == value2)
					{
						num2 = l;
						break;
					}
				}
				if (num2 == -1)
				{
					CrewMemory item3 = default(CrewMemory);
					item3.Prototype = memoryPrototype;
					item3.Actor = value2;
					item3.LastQuantity = (item3.QuantityFactor = 1f);
					item3.TimeAgo = timeSpan;
					num2 = Memories.Count;
					Memories.Add(item3);
				}
				CrewMemory value6 = Memories[num2];
				value6.ApprovalContribution += memory.ApprovalContribution;
				value6.RespectContribution += memory.RespectContribution;
				value6.MoraleContribution += memory.MoraleContribution;
				value6.TimeAgo = MathUtil.Min(timeSpan, value6.TimeAgo);
				Memories[num2] = value6;
			}
			else
			{
				CrewMemory item4 = new CrewMemory
				{
					Prototype = memory.Prototype,
					Actor = value2,
					Object = value3,
					ThirdParty = value4,
					ApprovalContribution = memory.ApprovalContribution,
					RespectContribution = memory.RespectContribution,
					MoraleContribution = memory.MoraleContribution,
					QuantityFactor = memory.QuantityFactor,
					LastQuantity = memory.LastQuantity,
					FakeNews = memory.FakeNews,
					TimeAgo = timeSpan
				};
				Memories.Add(item4);
			}
		}
	}

	public void ApplyMemoriesAndRelationships(Character character, SortedDictionary<CrewMemberSettings, Character> crewMembers)
	{
		for (int i = 0; i < Relationships.Count; i++)
		{
			if (crewMembers.TryGetValue(Relationships[i].RelationshipTarget, out var value))
			{
				Relationship item = new Relationship
				{
					RelationshipType = Relationships[i].RelationshipType,
					RelationshipTarget = value,
					ApprovalContribution = Relationships[i].ApprovalContribution,
					RespectContribution = Relationships[i].RespectContribution,
					KnownToPlayer = Relationships[i].KnownToPlayer
				};
				character.Relationships.Add(item);
			}
		}
		for (int j = 0; j < Memories.Count; j++)
		{
			CrewMemory crewMemory = Memories[j];
			Memory item2 = new Memory
			{
				Prototype = crewMemory.Prototype
			};
			if (crewMemory.Actor != null && crewMembers.TryGetValue(crewMemory.Actor, out var value2))
			{
				item2.Actor = value2;
			}
			if (crewMemory.Object != null && crewMembers.TryGetValue(crewMemory.Object, out var value3))
			{
				item2.Object = value3;
			}
			if (crewMemory.ThirdParty != null && crewMembers.TryGetValue(crewMemory.ThirdParty, out var value4))
			{
				item2.ThirdParty = value4;
			}
			item2.ApprovalContribution = crewMemory.ApprovalContribution;
			item2.RespectContribution = crewMemory.RespectContribution;
			item2.MoraleContribution = crewMemory.MoraleContribution;
			item2.QuantityFactor = crewMemory.QuantityFactor;
			item2.LastQuantity = crewMemory.LastQuantity;
			item2.FakeNews = crewMemory.FakeNews;
			item2.Time = -crewMemory.TimeAgo;
			item2.CalcPriority(character);
			character.Memories.Add(item2);
		}
		character.SortMemories();
	}

	public static void CopyValidEquipmentPolicies(List<EquipmentPolicy> from, List<EquipmentPolicy> to)
	{
		for (int i = 0; i < from.Count; i++)
		{
			EquipmentPolicy item = from[i];
			item.Proto = ((item.Proto != null) ? GameImpl.Instance.FindEquipmentPrototypeByName(item.Proto.Name) : null);
			item.Liquid = ((item.Liquid != null) ? GameImpl.Instance.FindLiquidPrototypeByName(item.Liquid.Name) : null);
			if (item.Proto != null || item.Liquid != null)
			{
				to.Add(item);
			}
		}
	}
}
