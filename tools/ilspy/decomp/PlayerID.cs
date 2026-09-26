using System;
using Steamworks;

public struct PlayerID : IEquatable<PlayerID>
{
	public CSteamID SteamID;

	public PlayerID(CSteamID steamID)
	{
		SteamID = steamID;
	}

	public bool IsLocal()
	{
		return SteamID == SteamUser.GetSteamID();
	}

	public bool IsValid()
	{
		return SteamID.m_SteamID != 0;
	}

	public bool IsNull()
	{
		return SteamID.m_SteamID == 0;
	}

	public bool Equals(PlayerID other)
	{
		return SteamID == other.SteamID;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlayerID)
		{
			return this == (PlayerID)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return SteamID.GetHashCode();
	}

	public static bool operator ==(PlayerID a, PlayerID b)
	{
		return a.SteamID == b.SteamID;
	}

	public static bool operator !=(PlayerID a, PlayerID b)
	{
		return a.SteamID != b.SteamID;
	}

	public string GetPlayerName()
	{
		return string.Copy(SteamFriends.GetFriendPersonaName(SteamID));
	}
}
