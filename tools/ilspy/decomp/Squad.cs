using System;
using System.Collections.Generic;

public class Squad : IReflectable
{
	public int Id;

	public Community SquadOwner;

	public List<Character> Members = new List<Character>();

	public SquadBehaviour Behaviour;

	public SquadAction Action;

	public bool ActionFinished;

	public int InitialSquadMemberCount;

	public TerrainCoord DestTile;

	public TerrainCoord GoalTile;

	public TimeSpan SquadCreatedTime;

	public TimeSpan ActionStartedTime;

	public TimeSpan LastEncounterTime;

	public int ThreatId;

	public int EnemyCommunityId;

	public int PillageObjectId;

	public Character GoalCharacter;

	public bool AvoidHostileBases;

	public bool GoalAchieved;

	public bool EverybodyFleeing;

	public bool Teleported;

	public bool MovedAlong;

	public TimeSpan StopForWarmthFailedTime = Target.Never;

	public TimeSpan TeleportAfterTime;

	public EquipmentPrototype AmbushForEquipmentType;

	public Character GetLeader()
	{
		return Members[0];
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Id);
		if (reflector.IsDeserialising)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			communityManager.NextFreeSquadId = Math.Max(communityManager.NextFreeSquadId, Id + 1);
		}
		reflector.AddGameObjectRefList(ref Members);
		reflector.Add(ref Behaviour);
		reflector.Add(ref Action);
		reflector.Add(ref ActionFinished);
		if (reflector.Version < 356)
		{
			InitialSquadMemberCount = Members.Count;
		}
		else
		{
			reflector.Add(ref InitialSquadMemberCount);
		}
		reflector.Add(ref DestTile);
		reflector.AddAfter(ref GoalTile, 21);
		reflector.AddAfter(ref ActionStartedTime, 21);
		reflector.AddAfter(ref LastEncounterTime, 21);
		if (reflector.Version >= 585)
		{
			reflector.Add(ref SquadCreatedTime);
		}
		else
		{
			SquadCreatedTime = ActionStartedTime;
		}
		reflector.Add(ref ThreatId);
		reflector.Add(ref EnemyCommunityId);
		reflector.AddAfter(ref PillageObjectId, 591);
		reflector.AddAfter(ref GoalCharacter, 79);
		reflector.AddAfter(ref AvoidHostileBases, 339);
		reflector.AddAfter(ref GoalAchieved, 337);
		reflector.AddAfter(ref EverybodyFleeing, 99);
		reflector.AddAfter(ref Teleported, 475);
		reflector.AddAfter(ref MovedAlong, 597);
		reflector.AddAfter(ref StopForWarmthFailedTime, 377);
		reflector.AddAfter(ref TeleportAfterTime, 499);
		reflector.AddAfter(ref AmbushForEquipmentType, 500);
		if (reflector.Version < 416 && Behaviour == SquadBehaviour.Travel && EverybodyFleeing)
		{
			AvoidHostileBases = true;
		}
	}

	public bool HasAnyEquipmentOfType(EquipmentPrototype proto)
	{
		foreach (Character member in Members)
		{
			if (member.Inventory.FindItemOfType(proto) != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyEquipmentOfClass(Type type)
	{
		foreach (Character member in Members)
		{
			if (member.Inventory.FindItemOfClass(type) != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyMolotovCocktails()
	{
		foreach (Character member in Members)
		{
			if (member.HasMolotovCocktail())
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyExplosives()
	{
		foreach (Character member in Members)
		{
			if (member.HasExplosives())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEveryoneInSquadIdleOrSatisfyingNeeds()
	{
		foreach (Character member in Members)
		{
			if (!member.IsBored() && !member.IsSatisfyingNeeds())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsEveryoneInSquadBored()
	{
		foreach (Character member in Members)
		{
			if (!member.IsBored())
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAnyoneInSquadVisibleToPlayer()
	{
		foreach (Character member in Members)
		{
			if (Session.Instance.CommunityManager.PlayerCommunity.IsCharacterVisibleToAnyMember(member, out var _))
			{
				return true;
			}
			if (Session.Instance.IsVisibleDeterministic(member.PosXZ))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEveryoneInSquadInsideBuilding(Building building)
	{
		foreach (Character member in Members)
		{
			if (member.InsideBuilding != building)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAnyoneInSquadInCombatOrAlertOrConversation()
	{
		foreach (Character member in Members)
		{
			if (member.InCombat)
			{
				return true;
			}
			if (member.IsLowAlert())
			{
				return true;
			}
			Goal goal = member.FindActiveGoal(GoalType.Conversation);
			if (goal != null)
			{
				if (MovedAlong)
				{
					Character targetCharacter = goal.GetTargetCharacter();
					if (targetCharacter != null && (targetCharacter.Community == member.Community || !targetCharacter.Alive))
					{
						continue;
					}
				}
				if (member != GetLeader() || member.GetLeaderCommand() == null)
				{
					return true;
				}
			}
			if (member.RecentActivityType == RecentActivityType.TalkTo && !MovedAlong && member.FindActiveGoal(GoalType.AftermathGoal) != null)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAnyoneInSquadInConversation(out TerrainCoord tile)
	{
		foreach (Character member in Members)
		{
			if (member.FindActiveGoal(GoalType.Conversation) != null && (member != GetLeader() || member.GetLeaderCommand() == null))
			{
				tile = member.Tile;
				return true;
			}
			if (member.RecentActivityType == RecentActivityType.TalkTo && member.FindActiveGoal(GoalType.AftermathGoal) != null)
			{
				tile = member.Tile;
				return true;
			}
		}
		tile = TerrainCoord.Invalid;
		return false;
	}

	public bool HasEveryoneInSquadFinishedLeaderCommand()
	{
		bool result = true;
		foreach (Character member in Members)
		{
			if (!member.IsLeaderCommandFinished())
			{
				result = false;
			}
		}
		return result;
	}

	public bool WasLeaderCommandSuccessfulForEveryoneInSquad()
	{
		bool result = true;
		foreach (Character member in Members)
		{
			if (!member.WasLeaderCommandSuccessful())
			{
				result = false;
			}
		}
		return result;
	}

	public bool WasLeaderCommandSuccessfulForAnyoneInSquad()
	{
		bool result = false;
		foreach (Character member in Members)
		{
			if (member.WasLeaderCommandSuccessful())
			{
				result = true;
			}
		}
		return result;
	}

	public bool CanForceOpenPlayerGates()
	{
		SquadBehaviour behaviour = Behaviour;
		if ((uint)(behaviour - 8) <= 4u)
		{
			return EnemyCommunityId == Session.Instance.CommunityManager.PlayerCommunity.Id;
		}
		return false;
	}

	public float GetLowestBodyTemperature()
	{
		float num = float.MaxValue;
		foreach (Character member in Members)
		{
			num = Math.Min(num, member.GetBodyTemperatureInCelsius());
		}
		return num;
	}

	public bool WantMapIcon()
	{
		switch (Behaviour)
		{
		case SquadBehaviour.Trade:
		{
			CommunityType communityType = SquadOwner.CommunityType;
			if ((uint)(communityType - 9) <= 1u)
			{
				return !Session.Instance.CommunityManager.Hunters.Contains(SquadOwner);
			}
			return false;
		}
		case SquadBehaviour.Extortion:
		case SquadBehaviour.Beg:
		case SquadBehaviour.WarnOffAlliance:
		case SquadBehaviour.WarnPopulation:
		case SquadBehaviour.RequestAlliance:
		{
			SquadAction action = Action;
			if (action == SquadAction.AttackThreat || (uint)(action - 13) <= 1u)
			{
				return false;
			}
			return true;
		}
		default:
			return false;
		}
	}
}
