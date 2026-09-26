using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class Lobby
{
	public LobbyID LobbyID;

	public string LeaderName;

	public string Greeting;

	public List<StorySource> StorySources = new List<StorySource>();

	public List<int> StoryHashes = new List<int>();

	public int StoryIndex;

	public int NumLobbyMembers;

	public int MaxLobbyMembers;

	public int Day;

	public int DayOfYear;

	public int CommunitySize;

	public int Version;

	public NetworkProtocol NetworkProtocol;

	public string PublicIP;

	public HSteamListenSocket ListenSocket;

	private float LastUpdatedLobbyData = -10000f;

	public void Listen()
	{
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworkingSocketsP2P:
		{
			SteamNetworkingConfigValue_t[] pOptions2 = new SteamNetworkingConfigValue_t[0];
			ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, pOptions2);
			break;
		}
		case NetworkProtocol.SteamNetworkingSocketsIP:
		{
			SteamNetworkingIPAddr localAddress = default(SteamNetworkingIPAddr);
			localAddress.Clear();
			localAddress.m_port = OnlineParty.Port;
			SteamNetworkingConfigValue_t[] pOptions = new SteamNetworkingConfigValue_t[0];
			ListenSocket = SteamNetworkingSockets.CreateListenSocketIP(ref localAddress, 0, pOptions);
			break;
		}
		}
	}

	public void StopListening()
	{
		NetworkProtocol networkProtocol = GameImpl.Instance.Settings.NetworkProtocol;
		if ((uint)(networkProtocol - 2) <= 1u)
		{
			SteamNetworkingSockets.CloseListenSocket(ListenSocket);
			ListenSocket = default(HSteamListenSocket);
		}
	}

	public bool GetLobbyData()
	{
		try
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Leader");
			string lobbyData2 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Members");
			string lobbyData3 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "MaxMembers");
			string lobbyData4 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Community");
			string lobbyData5 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Day");
			string lobbyData6 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "DayOfYear");
			string lobbyData7 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Version");
			string lobbyData8 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "NetworkProtocol");
			string lobbyData9 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Greeting");
			string lobbyData10 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "StoryIndex");
			string lobbyData11 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "Stories");
			string publicIP = string.Empty;
			if (GameImpl.Instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingSocketsIP)
			{
				publicIP = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "PublicIP");
			}
			int num = int.Parse(lobbyData11);
			StorySources.Clear();
			StoryHashes.Clear();
			for (int i = 0; i < num; i++)
			{
				string lobbyData12 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "StoryFolder" + i);
				string lobbyData13 = SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "StoryWorkshopId" + i);
				int item = int.Parse(SteamMatchmaking.GetLobbyData(LobbyID.SteamID, "StoryHash" + i));
				ulong id = ulong.Parse(lobbyData13);
				StorySources.Add(StorySource.FromWorkshopItemOrFolder(id, lobbyData12));
				StoryHashes.Add(item);
			}
			LeaderName = lobbyData;
			StoryIndex = StringUtil.ParseInt(lobbyData10);
			NumLobbyMembers = StringUtil.ParseInt(lobbyData2);
			MaxLobbyMembers = StringUtil.ParseInt(lobbyData3);
			CommunitySize = StringUtil.ParseInt(lobbyData4);
			Day = StringUtil.ParseInt(lobbyData5);
			DayOfYear = StringUtil.ParseInt(lobbyData6);
			NetworkProtocol = (NetworkProtocol)StringUtil.ParseInt(lobbyData8);
			Version = StringUtil.ParseInt(lobbyData7);
			Greeting = lobbyData9;
			PublicIP = publicIP;
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public int GetMaxLobbyMembers()
	{
		return SteamMatchmaking.GetLobbyMemberLimit(LobbyID.SteamID);
	}

	private bool DoStoriesMatch(List<Story> stories, List<StorySource> storySources, List<int> storyHashes)
	{
		if (stories.Count != storySources.Count)
		{
			return false;
		}
		for (int i = 0; i < stories.Count; i++)
		{
			if (!stories[i].StorySource.Equals(storySources[i]))
			{
				return false;
			}
			if (stories[i].StoryHash != storyHashes[i])
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateLobbyDataIfNeeded(bool force)
	{
		GameImpl instance = GameImpl.Instance;
		OnlineParty instance2 = OnlineParty.Instance;
		string localPlayerName = instance.GetLocalPlayerName();
		string text = ((instance.Settings.Greeting != null) ? instance.Settings.Greeting : string.Empty);
		int numPartyMembersExcludingBannedAndIgnoredPlayers = instance2.GetNumPartyMembersExcludingBannedAndIgnoredPlayers();
		int maxPartySize = instance.Settings.MaxPartySize;
		int num = CommunitySize;
		int day = Day;
		int num2 = DayOfYear;
		NetworkProtocol networkProtocol = instance.Settings.NetworkProtocol;
		if (instance.IsInLoadedSession())
		{
			Session instance3 = Session.Instance;
			num = instance3.CommunityManager.PlayerCommunity.GetLivingNonZombieMemberCount();
			day = instance3.Day;
			num2 = (int)instance3.DayOfYear;
		}
		else if (instance.GetState() == GameState.LoadingSession || instance.GetState() == GameState.UnityInitSession)
		{
			SaveGame saveGame = instance.LoadingSessionInfo.SaveGame;
			if (saveGame != null)
			{
				num = saveGame.Data.CommunitySize;
				day = saveGame.Data.Day;
				num2 = saveGame.Data.DayOfYear;
			}
		}
		string text2 = string.Empty;
		if (instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingSocketsIP)
		{
			text2 = SteamGameServer.GetPublicIP().ToString();
		}
		if (!force && !(Time.unscaledTime - LastUpdatedLobbyData >= 60f) && !(localPlayerName != LeaderName) && !(text != Greeting) && !(text2 != PublicIP) && networkProtocol == NetworkProtocol && DoStoriesMatch(instance.CurrentStories, StorySources, StoryHashes) && numPartyMembersExcludingBannedAndIgnoredPlayers == NumLobbyMembers && maxPartySize == MaxLobbyMembers && num == CommunitySize && day == Day && num2 == DayOfYear)
		{
			return;
		}
		LeaderName = string.Copy(localPlayerName);
		NumLobbyMembers = numPartyMembersExcludingBannedAndIgnoredPlayers;
		MaxLobbyMembers = maxPartySize;
		CommunitySize = num;
		Day = day;
		DayOfYear = num2;
		Version = GameImpl.ReleaseVersion;
		Greeting = text;
		PublicIP = text2;
		NetworkProtocol = networkProtocol;
		StorySources.Clear();
		StoryHashes.Clear();
		StoryIndex = -1;
		for (int i = 0; i < instance.CurrentStories.Count; i++)
		{
			Story story = instance.CurrentStories[i];
			StorySources.Add(story.StorySource);
			StoryHashes.Add(story.StoryHash);
			if (story.Settings != null && !story.Settings.IsMod)
			{
				StoryIndex = i;
			}
		}
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Leader", LeaderName);
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Members", NumLobbyMembers.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "MaxMembers", MaxLobbyMembers.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Community", CommunitySize.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Day", Day.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "DayOfYear", DayOfYear.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Version", Version.ToString());
		CSteamID steamID = LobbyID.SteamID;
		int networkProtocol2 = (int)NetworkProtocol;
		SteamMatchmaking.SetLobbyData(steamID, "NetworkProtocol", networkProtocol2.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Greeting", Greeting);
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "StoryIndex", StoryIndex.ToString());
		SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "Stories", StorySources.Count.ToString());
		for (int j = 0; j < StorySources.Count; j++)
		{
			SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "StoryFolder" + j, StorySources[j].Folder);
			SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "StoryWorkshopId" + j, StorySources[j].WorkshopId.ToString());
			SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "StoryHash" + j, StoryHashes[j].ToString());
		}
		if (instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingSocketsIP)
		{
			SteamMatchmaking.SetLobbyData(LobbyID.SteamID, "PublicIP", PublicIP.ToString());
		}
		LastUpdatedLobbyData = Time.unscaledTime;
	}
}
