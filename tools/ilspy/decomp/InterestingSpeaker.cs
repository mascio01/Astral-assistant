internal struct InterestingSpeaker : IReflectable
{
	private const float NearbyDist = 64f;

	public Character Speaker;

	public Character Listener;

	public int FinishedCountdown;

	public Importance Importance;

	public static InterestingSpeaker Create(Character speaker, Character listener, int finishedCountdown, Importance importance)
	{
		return new InterestingSpeaker
		{
			Speaker = speaker,
			Listener = listener,
			FinishedCountdown = finishedCountdown,
			Importance = importance
		};
	}

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref Speaker);
		reflector.Add(ref Listener);
		reflector.Add(ref FinishedCountdown);
		reflector.Add(ref Importance);
	}

	public bool IsEitherKnown()
	{
		return IsEitherKnown(includeAllies: true);
	}

	public bool IsEitherKnown(bool includeAllies)
	{
		CommunityManager communityManager = Session.Instance.CommunityManager;
		if (Speaker != null && Speaker.Community != null)
		{
			if (Speaker.IsControllableByOrFollowingPlayer() || (includeAllies && communityManager.PlayerCommunity.CachedAllies.Contains(Speaker.Community)))
			{
				return true;
			}
			foreach (PlayerRecord playerRecord in Session.Instance.PlayerRecords)
			{
				if (playerRecord.PlayerMode == PlayerMode.Controlling && playerRecord.PlayerCharacter != null && (Speaker.PosXZ - playerRecord.PlayerCharacter.PosXZ).sqrMagnitude < 4096f)
				{
					return true;
				}
			}
		}
		if (Listener != null && Listener.Community != null)
		{
			if (Listener.IsControllableByOrFollowingPlayer() || (includeAllies && communityManager.PlayerCommunity.CachedAllies.Contains(Listener.Community)))
			{
				return true;
			}
			foreach (PlayerRecord playerRecord2 in Session.Instance.PlayerRecords)
			{
				if (playerRecord2.PlayerMode == PlayerMode.Controlling && playerRecord2.PlayerCharacter != null && (Listener.PosXZ - playerRecord2.PlayerCharacter.PosXZ).sqrMagnitude < 4096f)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEitherDirectControlled()
	{
		if (Speaker != null && Speaker.GetPlayerControllingMe() != null)
		{
			return true;
		}
		if (Listener != null && Listener.GetPlayerControllingMe() != null)
		{
			return true;
		}
		return false;
	}

	public bool IsEitherSparringPartnerDirectControlled()
	{
		if (Speaker != null && Speaker.SparringPartner != null && Speaker.SparringPartner.GetPlayerControllingMe() != null)
		{
			return true;
		}
		if (Listener != null && Listener.SparringPartner != null && Listener.SparringPartner.GetPlayerControllingMe() != null)
		{
			return true;
		}
		return false;
	}

	public bool IsEitherPlayerControlled()
	{
		if (Speaker != null && Speaker.IsControllableByOrFollowingPlayer())
		{
			return true;
		}
		if (Listener != null && Listener.IsControllableByOrFollowingPlayer())
		{
			return true;
		}
		return false;
	}

	public bool IsEitherSparring()
	{
		if (Speaker != null && Speaker.SparringPartner != null)
		{
			return true;
		}
		if (Listener != null && Listener.SparringPartner != null)
		{
			return true;
		}
		return false;
	}
}
