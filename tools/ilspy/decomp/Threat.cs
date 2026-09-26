using System;
using System.Collections.Generic;

public class Threat : IReflectable
{
	public int Id;

	public List<Character> ThreatMembers = new List<Character>();

	public TimeSpan LastEncounteredTime;

	public TerrainCoord BoundsMin;

	public TerrainCoord BoundsMax;

	public Threat(Character character)
	{
		Session instance = Session.Instance;
		Id = instance.CommunityManager.NextFreeThreatId++;
		ThreatMembers.Add(character);
		LastEncounteredTime = instance.PlayTime;
		BoundsMin = (BoundsMax = character.Tile);
	}

	public Threat()
	{
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Id);
		if (reflector.IsDeserialising)
		{
			CommunityManager communityManager = Session.Instance.CommunityManager;
			communityManager.NextFreeThreatId = Math.Max(communityManager.NextFreeThreatId, Id + 1);
		}
		reflector.AddGameObjectRefList(ref ThreatMembers);
		reflector.Add(ref LastEncounteredTime);
		reflector.Add(ref BoundsMin);
		reflector.Add(ref BoundsMax);
	}

	public bool IsThreatInaccessibleTo(Character squadLeader)
	{
		foreach (Character threatMember in ThreatMembers)
		{
			Target target = squadLeader.GetTarget(threatMember);
			if (target == null || !target.GetFlag(TargetFlags.Inaccessible))
			{
				return false;
			}
		}
		return true;
	}
}
