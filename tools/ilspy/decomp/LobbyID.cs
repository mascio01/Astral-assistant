using Steamworks;

public struct LobbyID
{
	public CSteamID SteamID;

	public LobbyID(CSteamID steamID)
	{
		SteamID = steamID;
	}

	public bool IsValid()
	{
		return SteamID.m_SteamID != 0;
	}

	public bool Equals(LobbyID other)
	{
		return SteamID == other.SteamID;
	}

	public override bool Equals(object obj)
	{
		if (obj is LobbyID)
		{
			return this == (LobbyID)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return SteamID.GetHashCode();
	}

	public static bool operator ==(LobbyID a, LobbyID b)
	{
		return a.SteamID == b.SteamID;
	}

	public static bool operator !=(LobbyID a, LobbyID b)
	{
		return a.SteamID != b.SteamID;
	}
}
