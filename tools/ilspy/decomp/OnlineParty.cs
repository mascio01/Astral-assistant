using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Steamworks;
using UnityEngine;

public class OnlineParty
{
	public enum State
	{
		Idle,
		SearchingForLobbies,
		CreatingLobby,
		JoiningLobby,
		InLobbyAsLeader,
		InLobbyAsFollower,
		LeavingLobby
	}

	public static OnlineParty Instance;

	public static int PartySizeLimit = 4;

	public const int MaxChatMessageLength = 140;

	public static ushort Port = 42069;

	public State CurrentState;

	public Lobby CurrentLobby;

	public List<Lobby> Lobbies = new List<Lobby>();

	public List<PartyMember> PartyMembers = new List<PartyMember>();

	private TimeSpan LastFailedToCreateLobbyTime = TimeSpan.FromSeconds(-10000.0);

	private TimeSpan LastCheckedIfNeedToUpdateLobbyData = TimeSpan.FromSeconds(-10000.0);

	private int SentSessionId;

	public LobbyID RequestedLobbyIDToJoin;

	public P2PMsg Msg;

	private CustomBinaryReaderFromMemory MsgReader;

	private CustomBinaryWriterToMemory MsgWriter;

	private byte[] LongMsgBuffer = new byte[1048576];

	public byte[] ShortMsgBuffer = new byte[128];

	public bool IsVoiceChatting;

	public byte[] VoiceBufferCompressed = new byte[65536];

	public byte[] VoiceBufferDecompressed = new byte[65536];

	private int MyVoicePacketIndex;

	public static int VoiceSampleRate;

	public string Status;

	public NetworkStatusType StatusType;

	public int StatusId;

	public int StatusPercent;

	public float StatusTime;

	public static bool NetworkLoggingEnabled = false;

	private CallResult<LobbyCreated_t> LobbyCreatedCallResult;

	private CallResult<LobbyEnter_t> LobbyJoinedCallResult;

	private CallResult<LobbyMatchList_t> LobbyMatchListCallResult;

	private Callback<LobbyEnter_t> LobbyEnterCallback;

	private Callback<LobbyDataUpdate_t> LobbyDataUpdateCallback;

	private Callback<LobbyChatUpdate_t> LobbyChatUpdate;

	private Callback<LobbyKicked_t> LobbyKickedCallback;

	private Callback<GameLobbyJoinRequested_t> LobbyJoinRequestedCallback;

	private Callback<P2PSessionRequest_t> P2PSessionRequestCallback;

	private Callback<SteamNetworkingMessagesSessionRequest_t> SessionRequestCallback;

	private Callback<SteamNetworkingMessagesSessionFailed_t> SessionFailedCallback;

	private Callback<SteamNetConnectionStatusChangedCallback_t> SocketConnectionChangedCallback;

	public static bool SendAllChunksAtOnce = false;

	private IntPtr[] Messages = new IntPtr[1];

	public int LastSentInputFrame;

	public bool PushToTalkButtonHeld;

	public bool WantSendLoadingSession;

	public byte[] WantSendLoadingSessionBytes;

	public MD5Hash WantSendLoadingTerrainHash;

	public int SendingChunksSessionId;

	public List<P2PMsg> ChunkMessagesToSend = new List<P2PMsg>();

	private byte[] ReceivingSessionBytes;

	private int ReceivingSessionByteIndex;

	private int ReceivingSessionId;

	public int ReceivingChunkIndex;

	public int ReceivingChunkCount;

	private byte[] ReceivingTerrainBytes;

	private int ReceivingTerrainByteIndex;

	private MD5Hash ReceivingTerrainHash;

	public int ReceivingTerrainChunkIndex;

	public int ReceivingTerrainChunkCount;

	private byte[] ReceivingSnapshotBytes;

	private int ReceivingSnapshotByteIndex;

	public int ReceivingSnapshotChunkIndex;

	public int ReceivingSnapshotChunkCount;

	public int SessionToLoadId;

	public List<StorySource> SessionToLoadStorySources;

	public byte[] SessionToLoadBytes;

	public int SessionToLoadBytesLength;

	public MD5Hash SessionToLoadTerrainHash;

	public List<PlayerID> SessionToLoadPlayerIDs;

	public List<string> SessionToLoadPlayerNames;

	public bool SessionToLoadTerrainReceived;

	private Dictionary<MD5Hash, DateTime> TerrainCacheLastUsedTime = new Dictionary<MD5Hash, DateTime>();

	private const int MaxTerrainCacheSize = 10;

	public static int MaxChunkSize => GameImpl.Instance.Settings.NetworkProtocol switch
	{
		NetworkProtocol.SteamNetworkingMessages => 516096, 
		_ => 1040384, 
	};

	public static int GetPartySizeLimit()
	{
		return PartySizeLimit;
	}

	public void SetStatus(NetworkStatusType statusType, string status)
	{
		Status = status;
		StatusType = statusType;
		StatusId++;
		StatusPercent = 0;
		StatusTime = Time.unscaledTime;
	}

	public void ClearStatus()
	{
		SetStatus(NetworkStatusType.None, null);
	}

	public float GetCurrentStatusAge()
	{
		return Time.unscaledTime - StatusTime;
	}

	public string GetCurrentStatus()
	{
		switch (StatusType)
		{
		case NetworkStatusType.None:
			return null;
		case NetworkStatusType.WaitingForPartyMember:
		case NetworkStatusType.WaitingForPartyMemberAck:
			return Status;
		default:
			if (!(GetCurrentStatusAge() <= 10f))
			{
				return null;
			}
			return Status;
		}
	}

	public OnlineParty()
	{
		Instance = this;
	}

	public void Init()
	{
		Instance = this;
		if (GameImpl.Instance.SteamInitialized)
		{
			VoiceSampleRate = (int)SteamUser.GetVoiceOptimalSampleRate();
			LobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
			LobbyJoinedCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyJoined);
			LobbyMatchListCallResult = CallResult<LobbyMatchList_t>.Create(OnLobbyListRequested);
			LobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
			LobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
			LobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
			LobbyKickedCallback = Callback<LobbyKicked_t>.Create(OnLobbyKicked);
			LobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
			P2PSessionRequestCallback = Callback<P2PSessionRequest_t>.Create(OnP2PSessionRequest);
			SessionRequestCallback = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);
			SessionFailedCallback = Callback<SteamNetworkingMessagesSessionFailed_t>.Create(OnSessionFailed);
			SocketConnectionChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSocketConnectionChanged);
			InitNetworkProtocol();
			if (LobbyEnterCallback == null || LobbyDataUpdateCallback == null || LobbyChatUpdate == null || LobbyKickedCallback == null || LobbyJoinRequestedCallback == null || P2PSessionRequestCallback == null || SessionRequestCallback == null || SessionFailedCallback == null || SocketConnectionChangedCallback == null)
			{
				Debug.Log("I'm not expecting this to be null but I need to use these values somehow to avoid getting a warning that the variable is assigned but never used!");
			}
			MsgReader = new CustomBinaryReaderFromMemory(LongMsgBuffer, LongMsgBuffer.Length);
			MsgWriter = new CustomBinaryWriterToMemory(LongMsgBuffer);
			Msg = default(P2PMsg);
			Msg.InputFrame = new InputFrame();
		}
	}

	public void SetNetworkProtocol(NetworkProtocol networkProtocol)
	{
		GameImpl.Instance.Settings.NetworkProtocol = networkProtocol;
		GameImpl.Instance.AutoSaveSettings();
		InitNetworkProtocol();
	}

	public unsafe void InitNetworkProtocol()
	{
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworkingMessages:
			fixed (int* value = new int[1] { PartySizeLimit * 1024 * 1024 })
			{
				if (!SteamNetworkingUtils.SetConfigValue(ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_SendBufferSize, ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global, (IntPtr)0, ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32, new IntPtr(value)))
				{
					Debug.LogWarning("SetConfigValue k_ESteamNetworkingConfig_SendBufferSize failed");
				}
			}
			break;
		case NetworkProtocol.SteamNetworkingSocketsP2P:
		case NetworkProtocol.SteamNetworkingSocketsIP:
			SteamNetworkingUtils.InitRelayNetworkAccess();
			break;
		}
	}

	public void Load()
	{
	}

	public void Unload()
	{
		Instance = null;
	}

	private void SendHelloMessage(PartyMember partyMember)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.HelloNetwork,
			PlayerID = new PlayerID(SteamUser.GetSteamID())
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		partyMember.ConnectionEstablished = SendMessageTo(partyMember);
	}

	public bool CanJoinOrCreateLobby()
	{
		return true;
	}

	public bool IsVerifyingStrings()
	{
		return false;
	}

	public unsafe void Update()
	{
		GameImpl instance = GameImpl.Instance;
		if (!instance.SteamInitialized)
		{
			return;
		}
		if (RequestedLobbyIDToJoin.IsValid() && CanJoinOrCreateLobby())
		{
			Session instance2 = Session.Instance;
			if (instance2 != null && instance2.State == SessionState.Started && instance2.WantFinish == WantFinishState.None)
			{
				instance2.WantFinish = WantFinishState.TitleMenu;
			}
			if (instance.GetState() != GameState.RunningSession && instance.IsInLoadedSession())
			{
				instance.QuitGame();
			}
			if (IsInLobby())
			{
				LeaveLobby();
			}
			if (CurrentState == State.Idle && instance.GetState() == GameState.TitleMenu)
			{
				Lobby lobby = new Lobby();
				lobby.LobbyID = RequestedLobbyIDToJoin;
				JoinLobby(lobby);
				RequestedLobbyIDToJoin = default(LobbyID);
			}
		}
		if (instance.IsMultiplayerEnabled() && CurrentState == State.Idle && CanJoinOrCreateLobby() && instance.IsOnline() && instance.IsInLoadedSession() && instance.GetTime() - LastFailedToCreateLobbyTime >= TimeSpan.FromSeconds(60.0))
		{
			CreateLobby();
		}
		if (instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingMessages && IsInLobby())
		{
			foreach (PartyMember partyMember in PartyMembers)
			{
				if (!partyMember.ConnectionEstablished && !partyMember.IsLocal && !partyMember.IsBanned)
				{
					SendHelloMessage(partyMember);
				}
			}
		}
		if (CurrentState == State.InLobbyAsLeader && instance.GetTime() - LastCheckedIfNeedToUpdateLobbyData >= TimeSpan.FromSeconds(1.0))
		{
			CurrentLobby.UpdateLobbyDataIfNeeded(force: false);
			LastCheckedIfNeedToUpdateLobbyData = instance.GetTime();
		}
		if (CurrentState == State.LeavingLobby && !instance.IsInLoadedSession())
		{
			ClearLobby();
		}
		if (WantSendLoadingSession)
		{
			Session instance3 = Session.Instance;
			SendSession(instance3.SessionId, instance3.StorySources, WantSendLoadingSessionBytes, WantSendLoadingSessionBytes.Length, WantSendLoadingTerrainHash, SaveGameManager.IsSaveGameCompressed());
			lock (this)
			{
				WantSendLoadingSessionBytes = null;
				WantSendLoadingTerrainHash = default(MD5Hash);
				WantSendLoadingSession = false;
			}
		}
		else if (CurrentState == State.InLobbyAsLeader)
		{
			if (SendingChunksSessionId != 0)
			{
				foreach (PartyMember partyMember2 in PartyMembers)
				{
					if (partyMember2.IsLocal || partyMember2.IsBannedOrIgnored() || partyMember2.SentChunkIndex != -1)
					{
						continue;
					}
					if (SendAllChunksAtOnce)
					{
						for (int i = 0; i < ChunkMessagesToSend.Count; i++)
						{
							SendSessionChunk(partyMember2, i);
						}
					}
					else
					{
						SendSessionChunk(partyMember2, 0);
					}
				}
				if (IsSessionReadyToStart())
				{
					SendSessionReady();
				}
			}
			else if (instance.IsInLoadedSession())
			{
				Session instance4 = Session.Instance;
				GameTerrain instance5 = GameTerrain.Instance;
				if (!instance4.DoesNetworkPlayerIDsMatchOnlinePartyMembers())
				{
					instance4.AssignNewSessionId();
				}
				if (SentSessionId != instance4.SessionId && SessionToLoadId == 0)
				{
					if (instance4.NetworkPlayerIDs.Count > 1 || GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
					{
						GameImpl.TakeOutTheTrash();
						CustomBinaryWriterToMemory customBinaryWriterToMemory = new CustomBinaryWriterToMemory();
						instance4.Reflect(customBinaryWriterToMemory);
						MD5Hash mD5Hash = default(MD5Hash);
						if (!instance5.UseFixedTerrain)
						{
							mD5Hash = instance4.TerrainHash;
						}
						SendSession(instance4.SessionId, instance4.StorySources, customBinaryWriterToMemory._buffer, customBinaryWriterToMemory._index, mD5Hash, isCompressed: false);
						instance.SetState(GameState.SendingNetworkSession);
						SessionToLoadStorySources = instance4.StorySources;
						SessionToLoadBytes = customBinaryWriterToMemory._buffer;
						SessionToLoadBytesLength = customBinaryWriterToMemory._index;
						SessionToLoadTerrainHash = mD5Hash;
						SessionToLoadTerrainReceived = true;
					}
					else
					{
						SentSessionId = instance4.SessionId;
					}
				}
			}
		}
		bool flag = WantVoiceChat();
		if (!IsVoiceChatting && flag)
		{
			SteamUser.StartVoiceRecording();
			if (GameImpl.Instance.Settings.PushBtnToTalkEnabled)
			{
				SteamFriends.SetInGameVoiceSpeaking(SteamUser.GetSteamID(), bSpeaking: true);
			}
			IsVoiceChatting = true;
		}
		else if (IsVoiceChatting && !flag)
		{
			SteamUser.StopVoiceRecording();
			if (GameImpl.Instance.Settings.PushBtnToTalkEnabled)
			{
				SteamFriends.SetInGameVoiceSpeaking(SteamUser.GetSteamID(), bSpeaking: false);
			}
			IsVoiceChatting = false;
		}
		if (IsVoiceChatting && SteamUser.GetAvailableVoice(out var pcbCompressed) == EVoiceResult.k_EVoiceResultOK && pcbCompressed != 0)
		{
			if (VoiceBufferCompressed.Length < pcbCompressed)
			{
				VoiceBufferCompressed = new byte[pcbCompressed];
			}
			uint nBytesWritten = 0u;
			if (SteamUser.GetVoice(bWantCompressed: true, VoiceBufferCompressed, (uint)VoiceBufferCompressed.Length, out nBytesWritten) == EVoiceResult.k_EVoiceResultOK && nBytesWritten != 0)
			{
				P2PMsg p2PMsg = new P2PMsg
				{
					MsgType = P2PMsgType.VoiceRecording,
					PacketIndex = MyVoicePacketIndex++,
					Bytes = VoiceBufferCompressed,
					BytesSize = (int)nBytesWritten
				};
				MsgWriter.ResetIndex();
				p2PMsg.Reflect(MsgWriter);
				foreach (PartyMember partyMember3 in PartyMembers)
				{
					if (partyMember3.IsLocal || partyMember3.IsBannedOrIgnored())
					{
						continue;
					}
					switch (instance.Settings.NetworkProtocol)
					{
					case NetworkProtocol.SteamNetworking:
						SteamNetworking.SendP2PPacket(partyMember3.PlayerID.SteamID, MsgWriter._buffer, (uint)MsgWriter._index, EP2PSend.k_EP2PSendUnreliableNoDelay);
						break;
					case NetworkProtocol.SteamNetworkingMessages:
					{
						SteamNetworkingIdentity identityRemote = default(SteamNetworkingIdentity);
						identityRemote.SetSteamID(partyMember3.PlayerID.SteamID);
						fixed (byte* buffer2 = MsgWriter._buffer)
						{
							EResult eResult2 = SteamNetworkingMessages.SendMessageToUser(ref identityRemote, new IntPtr(buffer2), (uint)MsgWriter._index, 5, 0);
							if (eResult2 != EResult.k_EResultOK)
							{
								Debug.LogWarning("Voice SendMessageToUser error: " + eResult2);
							}
						}
						break;
					}
					case NetworkProtocol.SteamNetworkingSocketsP2P:
					case NetworkProtocol.SteamNetworkingSocketsIP:
						if (!(partyMember3.Connection != HSteamNetConnection.Invalid))
						{
							break;
						}
						fixed (byte* buffer = MsgWriter._buffer)
						{
							long pOutMessageNumber;
							EResult eResult = SteamNetworkingSockets.SendMessageToConnection(partyMember3.Connection, new IntPtr(buffer), (uint)MsgWriter._index, 5, out pOutMessageNumber);
							if (eResult != EResult.k_EResultOK)
							{
								Debug.LogWarning("Voice SendMessageToConnection error: " + eResult);
							}
						}
						break;
					}
				}
			}
		}
		bool flag2 = Session.Instance != null && Session.Instance.State == SessionState.None;
		PartyMember fromPartyMember;
		while (!flag2 && ReceiveMessageFrom(out fromPartyMember))
		{
			if (fromPartyMember != null && !fromPartyMember.IsBannedOrIgnored())
			{
				MsgReader.ResetIndex();
				bool flag3 = false;
				try
				{
					Msg.Reflect(MsgReader);
					flag3 = true;
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
				}
				if (flag3)
				{
					OnReceivedMessage(fromPartyMember, ref Msg);
				}
			}
		}
		if (SessionToLoadId != 0 && SessionToLoadTerrainReceived)
		{
			GameImpl.Instance.LoadNetworkGame(SessionToLoadId, SessionToLoadStorySources, SessionToLoadBytes, SessionToLoadBytesLength, SessionToLoadTerrainHash, SessionToLoadPlayerIDs, SessionToLoadPlayerNames);
			CleanUpSessionToLoad();
		}
	}

	public void SendSession(int sessionId, List<StorySource> storySources, byte[] sessionBytes, int sessionBytesLen, MD5Hash terrainHash, bool isCompressed)
	{
		byte[] array;
		int num;
		if (isCompressed)
		{
			array = sessionBytes;
			num = sessionBytes.Length;
		}
		else
		{
			using (new StopWatchMarker("Compress"))
			{
				array = Compression.Compress(sessionBytes, 0, sessionBytesLen);
				num = array.Length;
			}
		}
		if (NetworkLoggingEnabled)
		{
			Debug.Log("Compressed session from " + sessionBytesLen + " bytes to " + num + " bytes");
		}
		using (new StopWatchMarker("SendSession"))
		{
			ChunkMessagesToSend.Clear();
			P2PMsg item = new P2PMsg
			{
				MsgType = P2PMsgType.SyncGame,
				SessionId = sessionId,
				Stories = new List<StoryId>()
			};
			foreach (StorySource storySource in storySources)
			{
				StoryId item2 = new StoryId
				{
					Folder = storySource.Folder,
					WorkshopId = storySource.WorkshopId
				};
				item.Stories.Add(item2);
			}
			item.Version = GameImpl.ReleaseVersion;
			item.TerrainHash = terrainHash;
			int maxChunkSize = MaxChunkSize;
			int num2 = (num + maxChunkSize - 1) / maxChunkSize;
			for (int i = 0; i < num2; i++)
			{
				int num3 = i * maxChunkSize;
				byte[] array2 = new byte[Math.Min((i + 1) * maxChunkSize, num) - num3];
				Array.Copy(array, num3, array2, 0, array2.Length);
				item.ChunkIndex = i;
				item.ChunkCount = num2;
				item.TotalSize = num;
				item.BytesSize = array2.Length;
				item.Bytes = array2;
				ChunkMessagesToSend.Add(item);
			}
			foreach (PartyMember partyMember in PartyMembers)
			{
				partyMember.CleanUp();
				if (partyMember.IsBannedOrIgnored())
				{
					continue;
				}
				if (partyMember.IsLocal)
				{
					partyMember.SetAcknowledgedTerrainHash(sessionId, item.TerrainHash);
				}
				else if (SendAllChunksAtOnce)
				{
					for (int j = 0; j < num2; j++)
					{
						SendSessionChunk(partyMember, j);
					}
				}
				else
				{
					SendSessionChunk(partyMember, 0);
				}
			}
			SendingChunksSessionId = sessionId;
			SentSessionId = sessionId;
		}
	}

	public void SendSessionChunk(PartyMember partyMember, int chunkIndex)
	{
		MsgWriter.ResetIndex();
		P2PMsg p2PMsg = ChunkMessagesToSend[chunkIndex];
		p2PMsg.Reflect(MsgWriter);
		if (NetworkLoggingEnabled)
		{
			Debug.Log("Sending Chunk " + p2PMsg.SessionId + " (" + p2PMsg.ChunkIndex + "/" + p2PMsg.ChunkCount + ") to " + partyMember.GetPlayerName());
		}
		SendMessageTo(partyMember);
		partyMember.SentChunkIndex = p2PMsg.ChunkIndex;
	}

	public static bool IsTerrainCacheCompressed()
	{
		return false;
	}

	private void SendTerrain(int sessionId, MD5Hash terrainHash, PartyMember toPartyMember)
	{
		string terrainCacheFileName = GetTerrainCacheFileName(terrainHash);
		byte[] array;
		try
		{
			using (Stream stream = SaveGameManager.FileOpenRead(terrainCacheFileName))
			{
				array = new byte[stream.Length];
				stream.Read(array, 0, (int)stream.Length);
			}
			SetTerrainCacheLastUsedTime(terrainHash, DateTime.Now);
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to load cached terrain: " + terrainCacheFileName + ": " + ex.Message);
			LeaveLobby();
			ClearLobby();
			return;
		}
		byte[] array2;
		if (IsTerrainCacheCompressed())
		{
			array2 = array;
		}
		else
		{
			using (new StopWatchMarker("Compress Terrain"))
			{
				array2 = Compression.Compress(array, 0, array.Length);
			}
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Compressed terrain from " + array.Length + " bytes to " + array2.Length + " bytes");
			}
		}
		using (new StopWatchMarker("SendTerrain"))
		{
			toPartyMember.TerrainChunkMessagesToSend.Clear();
			P2PMsg item = new P2PMsg
			{
				MsgType = P2PMsgType.SendTerrain,
				SessionId = sessionId,
				TerrainHash = terrainHash
			};
			int maxChunkSize = MaxChunkSize;
			int num = (array2.Length + maxChunkSize - 1) / maxChunkSize;
			for (int i = 0; i < num; i++)
			{
				int num2 = i * maxChunkSize;
				byte[] array3 = new byte[Math.Min((i + 1) * maxChunkSize, array2.Length) - num2];
				Array.Copy(array2, num2, array3, 0, array3.Length);
				item.ChunkIndex = i;
				item.ChunkCount = num;
				item.TotalSize = array2.Length;
				item.BytesSize = array3.Length;
				item.Bytes = array3;
				toPartyMember.TerrainChunkMessagesToSend.Add(item);
			}
			if (SendAllChunksAtOnce)
			{
				for (int j = 0; j < num; j++)
				{
					SendTerrainChunk(toPartyMember, j);
				}
			}
			else
			{
				SendTerrainChunk(toPartyMember, 0);
			}
		}
	}

	public void SendTerrainChunk(PartyMember toPartyMember, int chunkIndex)
	{
		P2PMsg p2PMsg = toPartyMember.TerrainChunkMessagesToSend[chunkIndex];
		if (NetworkLoggingEnabled)
		{
			Debug.Log("Sending terrain " + p2PMsg.TerrainHash.ToString() + " for session " + p2PMsg.SessionId + " chunk " + p2PMsg.ChunkIndex + "/" + p2PMsg.ChunkCount + " to " + toPartyMember.GetPlayerName() + ": " + p2PMsg.BytesSize + "/" + p2PMsg.TotalSize + " bytes");
		}
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		SendMessageTo(toPartyMember);
		toPartyMember.SentTerrainChunkIndex = chunkIndex;
	}

	public void SendSessionReady()
	{
		List<PlayerID> list = new List<PlayerID>();
		List<string> list2 = new List<string>();
		if (GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
		{
			foreach (PartyMember partyMember in PartyMembers)
			{
				if (!partyMember.IsBannedOrIgnored())
				{
					list.Add(partyMember.PlayerID);
					list2.Add(partyMember.GetPlayerName());
				}
			}
			P2PMsg p2PMsg = new P2PMsg
			{
				MsgType = P2PMsgType.SessionReady,
				SessionId = SendingChunksSessionId,
				PlayerIDs = list,
				PlayerNames = list2
			};
			MsgWriter.ResetIndex();
			p2PMsg.Reflect(MsgWriter);
			for (int i = 0; i < list.Count; i++)
			{
				PartyMember partyMemberByID = GetPartyMemberByID(list[i]);
				if (partyMemberByID != null && !partyMemberByID.IsLocal)
				{
					if (NetworkLoggingEnabled)
					{
						Debug.Log("Sending SessionReady " + p2PMsg.SessionId + " to " + partyMemberByID.GetPlayerName());
					}
					SendMessageTo(partyMemberByID);
				}
			}
		}
		if (GameImpl.Instance.GetState() == GameState.SendingNetworkSession)
		{
			SessionToLoadPlayerIDs = list;
			SessionToLoadPlayerNames = list2;
			SessionToLoadId = SendingChunksSessionId;
		}
		SendingChunksSessionId = 0;
		ChunkMessagesToSend.Clear();
	}

	private bool SendMessageTo(PartyMember toPartyMember)
	{
		return toPartyMember.SendMessage(MsgWriter._buffer, MsgWriter._index);
	}

	private bool ReceiveMessageFrom(out PartyMember fromPartyMember)
	{
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworking:
		{
			if (SteamNetworking.IsP2PPacketAvailable(out var pcubMsgSize))
			{
				if (pcubMsgSize <= ShortMsgBuffer.Length)
				{
					MsgReader.SetBuffer(ShortMsgBuffer, ShortMsgBuffer.Length);
				}
				else
				{
					MsgReader.SetBuffer(LongMsgBuffer, LongMsgBuffer.Length);
				}
				if (SteamNetworking.ReadP2PPacket(MsgReader._buffer, pcubMsgSize, out var pcubMsgSize2, out var psteamIDRemote))
				{
					MsgReader._bufferLength = (int)pcubMsgSize2;
					fromPartyMember = GetPartyMemberByID(new PlayerID(psteamIDRemote));
					return true;
				}
			}
			break;
		}
		case NetworkProtocol.SteamNetworkingMessages:
			if (SteamNetworkingMessages.ReceiveMessagesOnChannel(0, Messages, Messages.Length) > 0)
			{
				SteamNetworkingMessage_t steamNetworkingMessage_t2 = SteamNetworkingMessage_t.FromIntPtr(Messages[0]);
				CSteamID steamID2 = steamNetworkingMessage_t2.m_identityPeer.GetSteamID();
				int cbSize2 = steamNetworkingMessage_t2.m_cbSize;
				Marshal.Copy(steamNetworkingMessage_t2.m_pData, LongMsgBuffer, 0, cbSize2);
				SteamNetworkingMessage_t.Release(Messages[0]);
				MsgReader.SetBuffer(LongMsgBuffer, LongMsgBuffer.Length);
				MsgReader._bufferLength = cbSize2;
				fromPartyMember = GetPartyMemberByID(new PlayerID(steamID2));
				return true;
			}
			break;
		case NetworkProtocol.SteamNetworkingSocketsP2P:
		case NetworkProtocol.SteamNetworkingSocketsIP:
			foreach (PartyMember partyMember in PartyMembers)
			{
				if (!partyMember.IsLocal && !partyMember.IsBannedOrIgnored() && SteamNetworkingSockets.ReceiveMessagesOnConnection(partyMember.Connection, Messages, Messages.Length) > 0)
				{
					SteamNetworkingMessage_t steamNetworkingMessage_t = SteamNetworkingMessage_t.FromIntPtr(Messages[0]);
					CSteamID steamID = steamNetworkingMessage_t.m_identityPeer.GetSteamID();
					int cbSize = steamNetworkingMessage_t.m_cbSize;
					Marshal.Copy(steamNetworkingMessage_t.m_pData, LongMsgBuffer, 0, cbSize);
					SteamNetworkingMessage_t.Release(Messages[0]);
					MsgReader.SetBuffer(LongMsgBuffer, LongMsgBuffer.Length);
					MsgReader._bufferLength = cbSize;
					fromPartyMember = GetPartyMemberByID(new PlayerID(steamID));
					return true;
				}
			}
			break;
		}
		fromPartyMember = null;
		return false;
	}

	public int GetHowManyInputFramesToAddThisFrame(Session session)
	{
		if (session.Editor)
		{
			return 0;
		}
		if (!session.IsInMultiplayerGame())
		{
			return 1;
		}
		PartyMember localPartyMember = GetLocalPartyMember();
		if (localPartyMember == null)
		{
			return 0;
		}
		int num = 0;
		int num2 = int.MaxValue;
		PartyMember partyMember = null;
		for (int i = 0; i < session.NetworkPlayerIDs.Count; i++)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(session.NetworkPlayerIDs[i]);
			if (partyMemberByID == null)
			{
				return 0;
			}
			if (partyMemberByID.AcknowledgedSessionId != session.SessionId)
			{
				int num3 = -1;
				int num4 = (IsInMultiplayerGameAsLeader() ? ChunkMessagesToSend.Count : ReceivingChunkCount);
				if (SendingChunksSessionId == session.SessionId && num4 > 0)
				{
					num3 = MathUtil.Clamp((int)(100f * ((float)(partyMemberByID.AcknowledgedChunkIndex + partyMemberByID.AcknowledgedTerrainChunkIndex) + 1f) / (float)(num4 + partyMemberByID.TerrainChunkCount)), 0, 100);
				}
				if (GetCurrentStatus() == null || (StatusType == NetworkStatusType.WaitingForPartyMemberAck && StatusPercent != num3))
				{
					string text = GameImpl.Translate("HUD_WaitingForPartyMemberAck").Replace("%1", partyMemberByID.GetPlayerName());
					if (num3 != -1)
					{
						text = text + " (" + num3 + "%)";
					}
					SetStatus(NetworkStatusType.WaitingForPartyMemberAck, text);
					StatusPercent = num3;
				}
				return 0;
			}
			int num5 = partyMemberByID.ReceivedInputFrame - session.InputFrame;
			num = Math.Max(num, num5);
			if (num5 < num2 && partyMemberByID != localPartyMember)
			{
				num2 = num5;
				partyMember = partyMemberByID;
			}
		}
		if (StatusType == NetworkStatusType.WaitingForPartyMemberAck)
		{
			ClearStatus();
		}
		int num6 = localPartyMember.ReceivedInputFrame - session.InputFrame;
		int num7 = 300;
		if (num6 >= num7)
		{
			if (!localPartyMember.FrameBufferWasFull)
			{
				if (partyMember != null)
				{
					SetStatus(NetworkStatusType.WaitingForPartyMember, GameImpl.Translate("HUD_WaitingForPartyMember").Replace("%1", partyMember.GetPlayerName()));
				}
				localPartyMember.FrameBufferWasFull = true;
			}
			return 0;
		}
		if (localPartyMember.FrameBufferWasFull)
		{
			if (num6 > 6)
			{
				return 0;
			}
			if (StatusType == NetworkStatusType.WaitingForPartyMember)
			{
				ClearStatus();
			}
			localPartyMember.FrameBufferWasFull = false;
		}
		if (num - num6 >= 2)
		{
			return Math.Min(num7, num - num6);
		}
		return 1;
	}

	public int GetHowManyInputFramesToProcessThisFrame(Session session)
	{
		PartyMember localPartyMember = GetLocalPartyMember();
		if (localPartyMember == null)
		{
			return 0;
		}
		int num = int.MaxValue;
		for (int i = 0; i < session.NetworkPlayerIDs.Count; i++)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(session.NetworkPlayerIDs[i]);
			int val = 0;
			if (partyMemberByID != null && partyMemberByID.AcknowledgedSessionId == session.SessionId)
			{
				val = partyMemberByID.ReceivedInputFrame - session.InputFrame;
			}
			num = Math.Min(num, val);
		}
		if (num >= 6)
		{
			localPartyMember.InputFrameProcessingMode = InputFrameProcessingMode.WantCatchUp;
		}
		else if (num <= 0)
		{
			localPartyMember.InputFrameProcessingMode = InputFrameProcessingMode.WantDropBack;
		}
		else
		{
			localPartyMember.InputFrameProcessingMode = InputFrameProcessingMode.Normal;
		}
		return localPartyMember.InputFrameProcessingMode switch
		{
			InputFrameProcessingMode.WantCatchUp => 6, 
			InputFrameProcessingMode.WantDropBack => 0, 
			_ => 1, 
		};
	}

	public void SendSyncGameAcknowledged(Session session)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.SyncGameAcknowledged,
			SessionId = session.SessionId,
			Version = GameImpl.ReleaseVersion
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		for (int i = 0; i < session.NetworkPlayerIDs.Count; i++)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(session.NetworkPlayerIDs[i]);
			if (partyMemberByID == null)
			{
				continue;
			}
			if (partyMemberByID.IsLocal)
			{
				partyMemberByID.SetAcknowledgedSession(session.SessionId);
			}
			else if (CurrentState == State.InLobbyAsFollower)
			{
				if (NetworkLoggingEnabled)
				{
					Debug.Log("Sending SyncGameAcknowledged " + p2PMsg.SessionId + " to " + partyMemberByID.GetPlayerName());
				}
				SendMessageTo(partyMemberByID);
			}
		}
	}

	public void SendInputFrame(Session session, InputFrame inputFrame)
	{
		LastSentInputFrame = inputFrame.Frame;
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.InputFrame,
			InputFrame = inputFrame
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		for (int i = 0; i < session.NetworkPlayerIDs.Count; i++)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(session.NetworkPlayerIDs[i]);
			if (partyMemberByID != null)
			{
				if (partyMemberByID.IsLocal)
				{
					partyMemberByID.AddInputFrame(inputFrame);
				}
				else
				{
					SendMessageTo(partyMemberByID);
				}
			}
		}
	}

	public void SendSnapshotHash(Session session, int inputFrame, MD5Hash hash)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.SnapshotHash,
			SessionId = session.SessionId,
			SnapshotInputFrame = inputFrame,
			SnapshotHash = hash
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		PartyMember leaderPartyMember = GetLeaderPartyMember();
		if (leaderPartyMember != null)
		{
			SendMessageTo(leaderPartyMember);
		}
		else
		{
			Debug.LogWarning("No party leader to send snapshot hash to!");
		}
	}

	public void SendSnapshotAcknowledged(Session session, int inputFrame, bool matched)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = (matched ? P2PMsgType.SnapshotMatched : P2PMsgType.SnapshotDidNotMatch),
			SessionId = session.SessionId,
			SnapshotInputFrame = inputFrame
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		for (int i = 0; i < session.NetworkPlayerIDs.Count; i++)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(session.NetworkPlayerIDs[i]);
			if (partyMemberByID != null && !partyMemberByID.IsLocal)
			{
				SendMessageTo(partyMemberByID);
			}
		}
	}

	public void SendSnapshotFull(Session session, SessionSnapshot snapshot, PartyMember toPartyMember)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.SnapshotFull,
			SnapshotInputFrame = snapshot.InputFrame,
			SessionId = session.SessionId
		};
		MD5Hash mD5Hash = new MD5Hash(snapshot.Writer._buffer, snapshot.Writer._index);
		if (NetworkLoggingEnabled)
		{
			MD5Hash mD5Hash2 = mD5Hash;
			string text = "SendSnapshotFull: " + mD5Hash2.ToString();
			for (int i = 0; i < snapshot.Hashes.Count; i++)
			{
				text = text + ", " + snapshot.Hashes[i].PlayerID.GetPlayerName() + ": " + snapshot.Hashes[i].Hash.ToString();
			}
			Debug.Log(text);
		}
		byte[] array = Compression.Compress(snapshot.Writer._buffer, 0, snapshot.Writer._index);
		int num = array.Length;
		int maxChunkSize = MaxChunkSize;
		int num2 = (num + maxChunkSize - 1) / maxChunkSize;
		for (int j = 0; j < num2; j++)
		{
			int num3 = j * maxChunkSize;
			byte[] array2 = new byte[Math.Min((j + 1) * maxChunkSize, num) - num3];
			Array.Copy(array, num3, array2, 0, array2.Length);
			p2PMsg.ChunkIndex = j;
			p2PMsg.ChunkCount = num2;
			p2PMsg.TotalSize = num;
			p2PMsg.BytesSize = array2.Length;
			p2PMsg.Bytes = array2;
			MsgWriter.ResetIndex();
			p2PMsg.Reflect(MsgWriter);
			SendMessageTo(toPartyMember);
		}
	}

	private void OnLobbyCreated(LobbyCreated_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_eResult == EResult.k_EResultOK && !bIOFailure)
		{
			CurrentState = State.InLobbyAsLeader;
			CurrentLobby = new Lobby();
			CurrentLobby.LobbyID = new LobbyID(new CSteamID(pCallback.m_ulSteamIDLobby));
			CurrentLobby.LeaderName = string.Copy(SteamFriends.GetPersonaName());
			PartyMembers.Clear();
			PartyMember partyMember = new PartyMember();
			partyMember.PlayerID = new PlayerID(SteamUser.GetSteamID());
			partyMember.IsLocal = true;
			partyMember.IsPartyLeader = true;
			PartyMembers.Add(partyMember);
			NetworkSessionMenu.Instance.WantRepopulate = true;
			CurrentLobby.UpdateLobbyDataIfNeeded(force: true);
			CurrentLobby.Listen();
		}
		else
		{
			ClearLobby();
			LastFailedToCreateLobbyTime = TimeSpan.FromSeconds(Time.unscaledTime);
		}
	}

	private void OnLobbyEnter(LobbyEnter_t pCallback)
	{
	}

	private void OnLobbyJoined(LobbyEnter_t pCallback, bool bIOFailure)
	{
		if (pCallback.m_EChatRoomEnterResponse == 1)
		{
			CSteamID cSteamID = new CSteamID(pCallback.m_ulSteamIDLobby);
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(cSteamID);
			if (numLobbyMembers <= 0)
			{
				CurrentState = State.LeavingLobby;
				SteamMatchmaking.LeaveLobby(cSteamID);
				GameImpl.Instance.OnJoinLobbyFailed(GameImpl.Translate("MENU_JoinFailed_HostLeft"));
				return;
			}
			Lobby lobby = new Lobby();
			lobby.LobbyID = new LobbyID(cSteamID);
			lobby.GetLobbyData();
			if (lobby.Version != GameImpl.ReleaseVersion)
			{
				CurrentState = State.LeavingLobby;
				SteamMatchmaking.LeaveLobby(cSteamID);
				GameImpl.Instance.OnJoinLobbyFailed(GameImpl.Translate("MENU_VersionMismatch").Replace("%1", GameImpl.ReleaseVersion.ToString()).Replace("%2", lobby.LeaderName)
					.Replace("%3", lobby.Version.ToString()));
				return;
			}
			if (lobby.NetworkProtocol != GameImpl.Instance.Settings.NetworkProtocol)
			{
				CurrentState = State.LeavingLobby;
				SteamMatchmaking.LeaveLobby(cSteamID);
				GameImpl.Instance.OnJoinLobbyFailed(GameImpl.Translate("MENU_NetworkProtocolMismatch"));
				return;
			}
			StorySource mismatchedStorySource = null;
			if (JoinGameMenu.HasStoryHashMismatch(lobby, ref mismatchedStorySource))
			{
				CurrentState = State.LeavingLobby;
				SteamMatchmaking.LeaveLobby(cSteamID);
				GameImpl.Instance.OnJoinLobbyFailed(GameImpl.Translate("MENU_StoryHashMismatch").Replace("%1", mismatchedStorySource.GetTranslatedName()));
				return;
			}
			CurrentState = State.InLobbyAsFollower;
			CurrentLobby = lobby;
			CurrentLobby.Listen();
			PartyMembers.Clear();
			for (int i = 0; i < numLobbyMembers; i++)
			{
				PartyMember partyMember = new PartyMember();
				partyMember.PlayerID = new PlayerID(SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobby.LobbyID.SteamID, i));
				partyMember.IsLocal = partyMember.PlayerID.IsLocal();
				partyMember.IsPartyLeader = partyMember.PlayerID.SteamID == SteamMatchmaking.GetLobbyOwner(CurrentLobby.LobbyID.SteamID);
				PartyMembers.Add(partyMember);
				if (!partyMember.IsLocal)
				{
					partyMember.Connect();
				}
			}
			ReceivingChunkIndex = 0;
			ReceivingChunkCount = 1;
			ReceivingTerrainChunkIndex = 0;
			ReceivingTerrainChunkCount = 1;
			GameImpl.Instance.StartReceivingNetworkSession(showLoadingScreen: true);
			NetworkSessionMenu.Instance.WantRepopulate = true;
		}
		else
		{
			CurrentState = State.Idle;
			string errorMsg = (EChatRoomEnterResponse)pCallback.m_EChatRoomEnterResponse switch
			{
				EChatRoomEnterResponse.k_EChatRoomEnterResponseDoesntExist => GameImpl.Translate("MENU_JoinFailed_DoesntExist"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseNotAllowed => GameImpl.Translate("MENU_JoinFailed_NotAllowed"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseFull => GameImpl.Translate("MENU_JoinFailed_Full"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseError => GameImpl.Translate("MENU_JoinFailed_Error"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseBanned => GameImpl.Translate("MENU_JoinFailed_Banned"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseLimited => GameImpl.Translate("MENU_JoinFailed_Limited"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseClanDisabled => GameImpl.Translate("MENU_JoinFailed_ClanDisabled"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseCommunityBan => GameImpl.Translate("MENU_JoinFailed_CommunityBan"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseMemberBlockedYou => GameImpl.Translate("MENU_JoinFailed_MemberBlockedYou"), 
				EChatRoomEnterResponse.k_EChatRoomEnterResponseYouBlockedMember => GameImpl.Translate("MENU_JoinFailed_YouBlockedMember"), 
				_ => "Unknown Error", 
			};
			GameImpl.Instance.OnJoinLobbyFailed(errorMsg);
		}
	}

	private Lobby FindLobbyInListBySteamID(CSteamID steamID)
	{
		foreach (Lobby lobby in Lobbies)
		{
			if (lobby.LobbyID.SteamID == steamID)
			{
				return lobby;
			}
		}
		return null;
	}

	private void OnLobbyDataUpdate(LobbyDataUpdate_t pCallback)
	{
		CSteamID cSteamID = new CSteamID(pCallback.m_ulSteamIDLobby);
		if (CurrentLobby != null && CurrentLobby.LobbyID.SteamID == cSteamID)
		{
			CurrentLobby.GetLobbyData();
		}
	}

	private void OnLobbyListRequested(LobbyMatchList_t pCallback, bool bIOFailure)
	{
		if (CurrentState != State.SearchingForLobbies)
		{
			return;
		}
		CurrentState = State.Idle;
		if (!bIOFailure)
		{
			for (int i = 0; i < (int)pCallback.m_nLobbiesMatching; i++)
			{
				CSteamID lobbyByIndex = SteamMatchmaking.GetLobbyByIndex(i);
				if (FindLobbyInListBySteamID(lobbyByIndex) == null)
				{
					Lobby lobby = new Lobby();
					lobby.LobbyID = new LobbyID(lobbyByIndex);
					Lobbies.Add(lobby);
				}
			}
			for (int num = Lobbies.Count - 1; num >= 0; num--)
			{
				if (!Lobbies[num].GetLobbyData() || Lobbies[num].Version != GameImpl.ReleaseVersion || Lobbies[num].NetworkProtocol != GameImpl.Instance.Settings.NetworkProtocol)
				{
					Lobbies.RemoveAt(num);
				}
			}
		}
		GameImpl.Instance.OnLobbyListSearchFinished();
	}

	private void OnLobbyChatUpdate(LobbyChatUpdate_t pCallback)
	{
		CSteamID cSteamID = new CSteamID(pCallback.m_ulSteamIDLobby);
		if (CurrentLobby == null || !(CurrentLobby.LobbyID.SteamID == cSteamID))
		{
			return;
		}
		if (!IsInLobby())
		{
			if (CurrentState != State.LeavingLobby)
			{
				Debug.LogWarning("OnLobbyChatUpdate called when we're not in a lobby: " + CurrentState);
			}
			return;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(CurrentLobby.LobbyID.SteamID);
		List<PlayerID> list = new List<PlayerID>();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			PlayerID playerID = new PlayerID(SteamMatchmaking.GetLobbyMemberByIndex(CurrentLobby.LobbyID.SteamID, i));
			list.Add(playerID);
			PartyMember partyMemberByID = GetPartyMemberByID(playerID);
			if (partyMemberByID == null)
			{
				partyMemberByID = new PartyMember();
				partyMemberByID.PlayerID = playerID;
				partyMemberByID.IsLocal = partyMemberByID.PlayerID.SteamID == SteamUser.GetSteamID();
				PartyMembers.Add(partyMemberByID);
				UpdatePartyLeader();
				partyMemberByID.Connect();
				OnPartyMemberJoined(partyMemberByID);
			}
		}
		for (int num = PartyMembers.Count - 1; num >= 0; num--)
		{
			PartyMember partyMember = PartyMembers[num];
			if (list.IndexOf(partyMember.PlayerID) == -1)
			{
				PartyMembers.RemoveAt(num);
				bool isPartyLeader = partyMember.IsPartyLeader;
				UpdatePartyLeader();
				if (OnPartyMemberLeft(partyMember, isPartyLeader))
				{
					break;
				}
			}
		}
	}

	private void OnLobbyKicked(LobbyKicked_t pCallback)
	{
		if (IsInLobby())
		{
			ClearLobby();
			GameImpl.Instance.QuitGame();
		}
	}

	private void OnLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
	{
		RequestedLobbyIDToJoin = new LobbyID(pCallback.m_steamIDLobby);
	}

	private void OnP2PSessionRequest(P2PSessionRequest_t pCallback)
	{
		if (GameImpl.Instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworking && GetPartyMemberByID(new PlayerID(pCallback.m_steamIDRemote)) != null)
		{
			SteamNetworking.AcceptP2PSessionWithUser(pCallback.m_steamIDRemote);
		}
	}

	private void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t pCallback)
	{
		if (GameImpl.Instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingMessages)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(new PlayerID(pCallback.m_identityRemote.GetSteamID()));
			if (partyMemberByID != null && !SteamNetworkingMessages.AcceptSessionWithUser(ref pCallback.m_identityRemote))
			{
				Debug.LogWarning("AcceptSessionWithUser failed with " + partyMemberByID.GetPlayerName());
			}
		}
	}

	private void OnSessionFailed(SteamNetworkingMessagesSessionFailed_t pCallback)
	{
		if (GameImpl.Instance.Settings.NetworkProtocol == NetworkProtocol.SteamNetworkingMessages)
		{
			PartyMember partyMemberByID = GetPartyMemberByID(new PlayerID(pCallback.m_info.m_identityRemote.GetSteamID()));
			Debug.LogWarning("Session Failed with " + ((partyMemberByID != null) ? partyMemberByID.GetPlayerName() : pCallback.m_info.m_identityRemote.ToString()) + ": " + pCallback.m_info.m_eState);
		}
	}

	private void OnSocketConnectionChanged(SteamNetConnectionStatusChangedCallback_t callback)
	{
		if (GameImpl.Instance.Settings.NetworkProtocol != NetworkProtocol.SteamNetworkingSocketsP2P && GameImpl.Instance.Settings.NetworkProtocol != NetworkProtocol.SteamNetworkingSocketsIP)
		{
			return;
		}
		switch (callback.m_info.m_eState)
		{
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
		{
			if (callback.m_eOldState != ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None)
			{
				break;
			}
			PartyMember partyMemberByID3 = GetPartyMemberByID(new PlayerID(callback.m_info.m_identityRemote.GetSteamID()));
			if (partyMemberByID3 != null)
			{
				EResult eResult = SteamNetworkingSockets.AcceptConnection(callback.m_hConn);
				if (eResult != EResult.k_EResultOK)
				{
					Debug.LogWarning("AcceptConnection failed with " + partyMemberByID3.GetPlayerName() + ": " + eResult);
				}
				partyMemberByID3.Connection = callback.m_hConn;
			}
			break;
		}
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
			if (callback.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
			{
				PartyMember partyMemberByID2 = GetPartyMemberByID(new PlayerID(callback.m_info.m_identityRemote.GetSteamID()));
				if (partyMemberByID2 != null)
				{
					partyMemberByID2.Connection = callback.m_hConn;
				}
			}
			break;
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
			if (callback.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting || callback.m_eOldState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
			{
				SteamNetworkingSockets.CloseConnection(callback.m_hConn, 0, callback.m_info.m_eState.ToString(), bEnableLinger: false);
				PartyMember partyMemberByID = GetPartyMemberByID(new PlayerID(callback.m_info.m_identityRemote.GetSteamID()));
				if (partyMemberByID != null)
				{
					partyMemberByID.Connection = default(HSteamNetConnection);
				}
			}
			break;
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
			break;
		}
	}

	private void CreateLobby()
	{
		GameImpl instance = GameImpl.Instance;
		CurrentState = State.CreatingLobby;
		SteamAPICall_t hAPICall = SteamMatchmaking.CreateLobby(instance.Settings.AllowJoinMode switch
		{
			AllowJoinMode.AllowAnyoneToJoin => ELobbyType.k_ELobbyTypePublic, 
			AllowJoinMode.AllowFriendsToJoin => ELobbyType.k_ELobbyTypeFriendsOnly, 
			_ => ELobbyType.k_ELobbyTypePrivate, 
		}, instance.Settings.MaxPartySize);
		LobbyCreatedCallResult.Set(hAPICall);
	}

	private void UpdatePartyLeader()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			partyMember.IsPartyLeader = partyMember.PlayerID.SteamID == SteamMatchmaking.GetLobbyOwner(CurrentLobby.LobbyID.SteamID);
			if (partyMember.IsLocal)
			{
				CurrentState = (partyMember.IsPartyLeader ? State.InLobbyAsLeader : State.InLobbyAsFollower);
			}
		}
	}

	private void OnPartyMemberJoined(PartyMember partyMember)
	{
		if (!partyMember.IsLocal)
		{
			if (CurrentState == State.InLobbyAsLeader)
			{
				if (GameImpl.Instance.Settings.AllowJoinMode != AllowJoinMode.AllowAnyoneToJoin && !SteamFriends.HasFriend(partyMember.PlayerID.SteamID, EFriendFlags.k_EFriendFlagImmediate))
				{
					KickFromGame(partyMember.PlayerID, kickedOnJoin: true, banned: false);
					return;
				}
				if (GameImpl.Instance.Settings.BannedPlayers != null)
				{
					foreach (PartyMember partyMember2 in PartyMembers)
					{
						if (GameImpl.Instance.Settings.BannedPlayers.Contains(partyMember2.PlayerID))
						{
							KickFromGame(partyMember2.PlayerID, kickedOnJoin: true, banned: true);
						}
					}
				}
			}
			if (!partyMember.IsBannedOrIgnored())
			{
				SetStatus(NetworkStatusType.PartyMemberJoined, StringUtil.ApplyFormulae(GameImpl.Translate("HUD_PartyMemberJoined").Replace("%1", partyMember.GetPlayerName())));
			}
		}
		NetworkSessionMenu.Instance.WantRepopulate = true;
	}

	private bool OnPartyMemberLeft(PartyMember partyMember, bool wasLeader)
	{
		if (!partyMember.IsLocal && !partyMember.IsKickedOnJoin)
		{
			SetStatus(NetworkStatusType.PartyMemberLeft, StringUtil.ApplyFormulae(GameImpl.Translate("HUD_PartyMemberLeft").Replace("%1", partyMember.GetPlayerName())));
		}
		partyMember.Disconnect();
		partyMember.CleanUp();
		NetworkSessionMenu.Instance.WantRepopulate = true;
		if (wasLeader && (CurrentState == State.JoiningLobby || GameImpl.Instance.GetState() == GameState.ReceivingNetworkSession))
		{
			LeaveLobby();
			ClearLobby();
			GameImpl.Instance.OnKicked(StringUtil.ApplyFormulae(GameImpl.Translate("HUD_PartyMemberLeft").Replace("%1", partyMember.GetPlayerName())));
			return true;
		}
		return false;
	}

	public void JoinLobby(Lobby lobby)
	{
		if (CurrentState != State.Idle)
		{
			Debug.LogWarning("Trying to join lobby when we are in state: " + CurrentState);
			return;
		}
		CurrentState = State.JoiningLobby;
		SteamAPICall_t hAPICall = SteamMatchmaking.JoinLobby(lobby.LobbyID.SteamID);
		LobbyJoinedCallResult.Set(hAPICall);
	}

	public void LeaveLobby()
	{
		if (IsInLobby())
		{
			SteamMatchmaking.LeaveLobby(CurrentLobby.LobbyID.SteamID);
			CurrentState = State.LeavingLobby;
		}
	}

	public void KickFromGame(PlayerID playerID, bool kickedOnJoin, bool banned)
	{
		P2PMsg p2PMsg = new P2PMsg
		{
			MsgType = P2PMsgType.KickFromGame,
			PlayerID = playerID,
			KickedOnJoin = kickedOnJoin,
			Banned = banned
		};
		MsgWriter.ResetIndex();
		p2PMsg.Reflect(MsgWriter);
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (!partyMember.IsLocal)
			{
				SendMessageTo(partyMember);
			}
			if (partyMember.PlayerID == playerID)
			{
				partyMember.IsBanned = true;
				partyMember.IsKickedOnJoin |= kickedOnJoin;
			}
		}
	}

	public bool WantVoiceChat()
	{
		if (!IsInLobby())
		{
			return false;
		}
		if (!GameImpl.Instance.Settings.VoiceChatEnabled)
		{
			return false;
		}
		if (GameImpl.Instance.Settings.PushBtnToTalkEnabled)
		{
			return PushToTalkButtonHeld;
		}
		return true;
	}

	public void SearchForLobbies()
	{
		if (CurrentState != State.Idle || !GameImpl.Instance.SteamInitialized)
		{
			return;
		}
		CurrentState = State.SearchingForLobbies;
		Lobbies.Clear();
		int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		for (int i = 0; i < friendCount; i++)
		{
			if (SteamFriends.GetFriendGamePlayed(SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate), out var pFriendGameInfo) && pFriendGameInfo.m_steamIDLobby.IsValid())
			{
				SteamMatchmaking.RequestLobbyData(pFriendGameInfo.m_steamIDLobby);
				Lobby lobby = new Lobby();
				lobby.LobbyID = new LobbyID(pFriendGameInfo.m_steamIDLobby);
				Lobbies.Add(lobby);
			}
		}
		SteamMatchmaking.AddRequestLobbyListStringFilter("Version", GameImpl.ReleaseVersionAsString, ELobbyComparison.k_ELobbyComparisonEqual);
		SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
		LobbyMatchListCallResult.Set(hAPICall);
	}

	public bool IsInLobby()
	{
		if (CurrentState != State.InLobbyAsLeader)
		{
			return CurrentState == State.InLobbyAsFollower;
		}
		return true;
	}

	public bool IsInMultiplayerGame()
	{
		return GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1;
	}

	public bool IsInMultiplayerGameAsLeader()
	{
		if (GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
		{
			return CurrentState == State.InLobbyAsLeader;
		}
		return false;
	}

	public bool IsInMultiplayerGameAsFollower()
	{
		if (GetNumPartyMembersExcludingBannedAndIgnoredPlayers() > 1)
		{
			return CurrentState == State.InLobbyAsFollower;
		}
		return false;
	}

	public PartyMember GetPartyMemberByID(PlayerID id)
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (partyMember.PlayerID == id)
			{
				return partyMember;
			}
		}
		return null;
	}

	public PartyMember GetLocalPartyMember()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (partyMember.IsLocal)
			{
				return partyMember;
			}
		}
		return null;
	}

	public PlayerID GetLocalPlayerID()
	{
		return GetLocalPartyMember()?.PlayerID ?? default(PlayerID);
	}

	public int GetNumPartyMembersExcludingBannedAndIgnoredPlayers()
	{
		int num = 0;
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (!partyMember.IsBannedOrIgnored())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumPartyMembersExcludingBannedPlayers()
	{
		int num = 0;
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (!partyMember.IsBanned)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsSessionReadyToStart()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (!partyMember.IsLocal && !partyMember.IsBannedOrIgnored())
			{
				if (partyMember.AcknowledgedChunkIndex < ChunkMessagesToSend.Count - 1)
				{
					return false;
				}
				if (partyMember.AcknowledgedTerrainForSessionId != SendingChunksSessionId)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsInputPausedDueToLocalFrameBufferFull()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (partyMember.IsLocal)
			{
				return partyMember.FrameBufferWasFull;
			}
		}
		return false;
	}

	public PartyMember GetLeaderPartyMember()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			if (partyMember.IsPartyLeader)
			{
				return partyMember;
			}
		}
		return null;
	}

	public void CleanUpSessionLoading()
	{
		foreach (PartyMember partyMember in PartyMembers)
		{
			partyMember.CleanUp();
		}
		lock (this)
		{
			WantSendLoadingSessionBytes = null;
			WantSendLoadingTerrainHash = default(MD5Hash);
			WantSendLoadingSession = false;
		}
		SendingChunksSessionId = 0;
		ChunkMessagesToSend.Clear();
		ReceivingSessionBytes = null;
		ReceivingSessionByteIndex = 0;
		ReceivingSessionId = 0;
		ReceivingChunkIndex = 0;
		ReceivingChunkCount = 1;
		ReceivingTerrainBytes = null;
		ReceivingTerrainByteIndex = 0;
		ReceivingTerrainChunkIndex = 0;
		ReceivingTerrainChunkCount = 1;
	}

	public void CleanUpSessionToLoad()
	{
		SessionToLoadId = 0;
		SessionToLoadStorySources = null;
		SessionToLoadBytes = null;
		SessionToLoadBytesLength = 0;
		SessionToLoadTerrainHash = default(MD5Hash);
		SessionToLoadPlayerIDs = null;
		SessionToLoadPlayerNames = null;
		SessionToLoadTerrainReceived = false;
	}

	public void ClearLobby()
	{
		if (CurrentLobby != null)
		{
			CurrentLobby.StopListening();
		}
		CleanUpSessionLoading();
		CleanUpSessionToLoad();
		foreach (PartyMember partyMember in PartyMembers)
		{
			partyMember.Disconnect();
			partyMember.CleanUp();
		}
		CurrentLobby = null;
		CurrentState = State.Idle;
		PartyMembers.Clear();
		ClearStatus();
		NetworkSessionMenu.Instance.WantRepopulate = true;
	}

	private void OnReceivedMessage(PartyMember fromPartyMember, ref P2PMsg msg)
	{
		Session instance = Session.Instance;
		switch (msg.MsgType)
		{
		case P2PMsgType.SyncGame:
		{
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving session " + msg.SessionId + " chunk " + msg.ChunkIndex + "/" + msg.ChunkCount + " from " + fromPartyMember.GetPlayerName() + ": " + msg.BytesSize + "/" + msg.TotalSize + " bytes");
			}
			if (msg.ChunkIndex == 0)
			{
				if (msg.Version != GameImpl.ReleaseVersion)
				{
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_VersionMismatch").Replace("%1", GameImpl.ReleaseVersion.ToString()).Replace("%2", fromPartyMember.GetPlayerName())
						.Replace("%3", msg.Version.ToString()));
					break;
				}
				ReceivingTerrainChunkIndex = 0;
				ReceivingTerrainChunkCount = 1;
				GameImpl.Instance.StartReceivingNetworkSession(showLoadingScreen: false);
				ReceivingSessionId = msg.SessionId;
				ReceivingSessionBytes = new byte[msg.TotalSize];
				ReceivingSessionByteIndex = 0;
				fromPartyMember.SetAcknowledgedTerrainHash(msg.SessionId, msg.TerrainHash);
				fromPartyMember.SetAcknowledgedSession(msg.SessionId);
				if (msg.TerrainHash.IsValid() && !IsTerrainCached(msg.TerrainHash))
				{
					P2PMsg p2PMsg = new P2PMsg
					{
						MsgType = P2PMsgType.RequestTerrain,
						SessionId = msg.SessionId,
						TerrainHash = msg.TerrainHash
					};
					MsgWriter.ResetIndex();
					p2PMsg.Reflect(MsgWriter);
					if (NetworkLoggingEnabled)
					{
						string[] obj3 = new string[6]
						{
							"Sending RequestTerrain for session ",
							msg.SessionId.ToString(),
							", hash ",
							null,
							null,
							null
						};
						MD5Hash terrainHash = msg.TerrainHash;
						obj3[3] = terrainHash.ToString();
						obj3[4] = " to ";
						obj3[5] = fromPartyMember.GetPlayerName();
						Debug.Log(string.Concat(obj3));
					}
					SendMessageTo(fromPartyMember);
				}
				else
				{
					P2PMsg p2PMsg2 = new P2PMsg
					{
						MsgType = P2PMsgType.TerrainAcknowledged,
						SessionId = msg.SessionId,
						TerrainHash = msg.TerrainHash
					};
					MsgWriter.ResetIndex();
					p2PMsg2.Reflect(MsgWriter);
					if (NetworkLoggingEnabled)
					{
						string[] obj4 = new string[5]
						{
							"Sending TerrainAcknowledged for session ",
							msg.SessionId.ToString(),
							", hash ",
							null,
							null
						};
						MD5Hash terrainHash = msg.TerrainHash;
						obj4[3] = terrainHash.ToString();
						obj4[4] = " to all";
						Debug.Log(string.Concat(obj4));
					}
					foreach (PartyMember partyMember in PartyMembers)
					{
						if (partyMember != null)
						{
							if (partyMember.IsLocal)
							{
								partyMember.SetAcknowledgedTerrainHash(msg.SessionId, msg.TerrainHash);
							}
							else
							{
								SendMessageTo(partyMember);
							}
						}
					}
					SessionToLoadTerrainReceived = true;
				}
			}
			else
			{
				if (ReceivingSessionId != msg.SessionId)
				{
					string msg4 = GameImpl.Translate("MENU_SyncError") + ": Receiving chunk from unexpected session: " + msg.SessionId + ", expected " + ReceivingSessionId;
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg4);
					break;
				}
				if (ReceivingChunkIndex != msg.ChunkIndex)
				{
					string msg5 = GameImpl.Translate("MENU_SyncError") + ": Receiving chunks out of order: " + msg.ChunkIndex + ", expected " + ReceivingChunkIndex;
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg5);
					break;
				}
				if (ReceivingSessionBytes == null)
				{
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_SyncError") + ": Receiving unexpected chunks");
					break;
				}
			}
			ReceivingChunkIndex = msg.ChunkIndex + 1;
			ReceivingChunkCount = msg.ChunkCount;
			int num2 = ReceivingSessionByteIndex + msg.BytesSize;
			if (num2 > ReceivingSessionBytes.Length)
			{
				string msg6 = GameImpl.Translate("MENU_SyncError") + ": Received more bytes than expected (" + num2 + "/" + ReceivingSessionBytes.Length + ")";
				LeaveLobby();
				ClearLobby();
				GameImpl.Instance.OnKicked(msg6);
				break;
			}
			Array.Copy(msg.Bytes, 0, ReceivingSessionBytes, ReceivingSessionByteIndex, msg.BytesSize);
			ReceivingSessionByteIndex += msg.BytesSize;
			P2PMsg p2PMsg3 = new P2PMsg
			{
				MsgType = P2PMsgType.ChunkAcknowledged,
				SessionId = msg.SessionId,
				ChunkIndex = msg.ChunkIndex,
				ChunkCount = msg.ChunkCount
			};
			MsgWriter.ResetIndex();
			p2PMsg3.Reflect(MsgWriter);
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Sending ChunkAcknowledged " + msg.SessionId + " (" + msg.ChunkIndex + "/" + msg.ChunkCount + ") to all");
			}
			foreach (PartyMember partyMember2 in PartyMembers)
			{
				if (partyMember2 != null && !partyMember2.IsLocal)
				{
					SendMessageTo(partyMember2);
				}
			}
			GameImpl.TakeOutTheTrash();
			if (msg.ChunkIndex != msg.ChunkCount - 1)
			{
				break;
			}
			using (new StopWatchMarker("Decompress"))
			{
				ReceivingSessionBytes = Compression.Decompress(ReceivingSessionBytes, 0, ReceivingSessionBytes.Length);
			}
			if (ReceivingSessionBytes != null)
			{
				List<StorySource> list = new List<StorySource>();
				for (int i = 0; i < msg.Stories.Count; i++)
				{
					list.Add(StorySource.FromWorkshopItemOrFolder(msg.Stories[i].WorkshopId, msg.Stories[i].Folder));
				}
				SessionToLoadStorySources = list;
				SessionToLoadBytes = ReceivingSessionBytes;
				SessionToLoadBytesLength = ReceivingSessionBytes.Length;
				SessionToLoadTerrainHash = msg.TerrainHash;
			}
			else
			{
				LeaveLobby();
				ClearLobby();
				GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_SyncError"));
			}
			break;
		}
		case P2PMsgType.SessionReady:
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving SessionReady " + msg.SessionId + " from " + fromPartyMember.GetPlayerName());
			}
			SessionToLoadId = msg.SessionId;
			SessionToLoadPlayerIDs = msg.PlayerIDs;
			SessionToLoadPlayerNames = msg.PlayerNames;
			break;
		case P2PMsgType.SyncGameAcknowledged:
			if (msg.Version != GameImpl.ReleaseVersion)
			{
				KickFromGame(fromPartyMember.PlayerID, kickedOnJoin: true, banned: false);
				break;
			}
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving SyncGameAcknowledged " + msg.SessionId + " from " + fromPartyMember.GetPlayerName());
			}
			fromPartyMember.SetAcknowledgedSession(msg.SessionId);
			break;
		case P2PMsgType.ChunkAcknowledged:
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving ChunkAcknowledged " + msg.SessionId + "(" + msg.ChunkIndex + "/" + msg.ChunkCount + ") from " + fromPartyMember.GetPlayerName());
			}
			if (msg.SessionId == SendingChunksSessionId)
			{
				fromPartyMember.AcknowledgedChunkIndex = msg.ChunkIndex;
				if (!SendAllChunksAtOnce && IsInMultiplayerGameAsLeader() && fromPartyMember.AcknowledgedChunkIndex + 1 < ChunkMessagesToSend.Count)
				{
					SendSessionChunk(fromPartyMember, fromPartyMember.AcknowledgedChunkIndex + 1);
				}
			}
			break;
		case P2PMsgType.InputFrame:
			if (instance != null && instance.State <= SessionState.Started && fromPartyMember.AcknowledgedSessionId == instance.SessionId)
			{
				int frame = fromPartyMember.ReceivedInputFrame + 1;
				msg.InputFrame.Frame = frame;
				fromPartyMember.AddInputFrame(msg.InputFrame);
			}
			break;
		case P2PMsgType.SnapshotHash:
			if (instance != null && instance.State == SessionState.Started && instance.SessionId == msg.SessionId)
			{
				Session.Instance.OnLeaderReceivedSnapshot(msg.SnapshotInputFrame, msg.SnapshotHash, fromPartyMember);
			}
			break;
		case P2PMsgType.SnapshotMatched:
			if (instance != null && instance.State == SessionState.Started && instance.SessionId == msg.SessionId)
			{
				Session.Instance.RemoveSnapshot(msg.SnapshotInputFrame);
			}
			break;
		case P2PMsgType.SnapshotDidNotMatch:
			if (instance != null && instance.State == SessionState.Started && instance.SessionId == msg.SessionId)
			{
				Session.Instance.OnSnapshotHashMismatch(msg.SnapshotInputFrame, fromPartyMember);
			}
			break;
		case P2PMsgType.SnapshotFull:
		{
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving snapshot chunk " + msg.ChunkIndex + "/" + msg.ChunkCount + " from " + fromPartyMember.GetPlayerName() + ": " + msg.BytesSize + "/" + msg.TotalSize + " bytes");
			}
			if (msg.ChunkIndex == 0)
			{
				ReceivingSnapshotBytes = new byte[msg.TotalSize];
				ReceivingSnapshotByteIndex = 0;
			}
			else
			{
				if (ReceivingSnapshotChunkIndex != msg.ChunkIndex)
				{
					string msg2 = GameImpl.Translate("MENU_SyncError") + ": Receiving snapshot chunks out of order: " + msg.ChunkIndex + ", expected " + ReceivingSnapshotChunkIndex;
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg2);
					break;
				}
				if (ReceivingSnapshotBytes == null)
				{
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_SyncError") + ": Receiving unexpected snapshot chunks");
					break;
				}
			}
			ReceivingSnapshotChunkIndex = msg.ChunkIndex + 1;
			ReceivingSnapshotChunkCount = msg.ChunkCount;
			int num = ReceivingSnapshotByteIndex + msg.BytesSize;
			if (num > ReceivingSnapshotBytes.Length)
			{
				string msg3 = GameImpl.Translate("MENU_SyncError") + ": Received more snapshot bytes than expected (" + num + "/" + ReceivingSnapshotBytes.Length + ")";
				LeaveLobby();
				ClearLobby();
				GameImpl.Instance.OnKicked(msg3);
				break;
			}
			Array.Copy(msg.Bytes, 0, ReceivingSnapshotBytes, ReceivingSnapshotByteIndex, msg.BytesSize);
			ReceivingSnapshotByteIndex += msg.BytesSize;
			if (msg.ChunkIndex == msg.ChunkCount - 1)
			{
				byte[] array = Compression.Decompress(ReceivingSnapshotBytes, 0, ReceivingSnapshotByteIndex);
				if (array == null)
				{
					array = new byte[0];
				}
				MD5Hash mD5Hash = new MD5Hash(array, array.Length);
				if (NetworkLoggingEnabled)
				{
					MD5Hash terrainHash = mD5Hash;
					Debug.Log("Received full snapshot: " + terrainHash.ToString() + " for frame " + msg.SnapshotInputFrame);
				}
				SaveSessionSnapshot(array, array.Length, msg.SnapshotInputFrame, fromPartyMember, Session.SessionOutOfSyncSnapshotFolder);
				ReceivingSnapshotBytes = null;
				ReceivingSnapshotByteIndex = 0;
			}
			break;
		}
		case P2PMsgType.VoiceRecording:
		{
			uint nBytesWritten = 0u;
			if (SteamUser.DecompressVoice(msg.Bytes, (uint)msg.BytesSize, VoiceBufferDecompressed, (uint)VoiceBufferDecompressed.Length, out nBytesWritten, (uint)VoiceSampleRate) == EVoiceResult.k_EVoiceResultBufferTooSmall)
			{
				VoiceBufferDecompressed = new byte[nBytesWritten];
				SteamUser.DecompressVoice(msg.Bytes, (uint)msg.BytesSize, VoiceBufferDecompressed, (uint)VoiceBufferDecompressed.Length, out nBytesWritten, (uint)VoiceSampleRate);
			}
			if (nBytesWritten != 0)
			{
				if (fromPartyMember.VoiceSpeaker == null)
				{
					fromPartyMember.StartVoice();
				}
				VoiceChatPacket voiceChatPacket = new VoiceChatPacket();
				voiceChatPacket.Data = VoiceBufferDecompressed;
				voiceChatPacket.Length = (int)nBytesWritten;
				voiceChatPacket.PacketId = msg.PacketIndex;
				fromPartyMember.OnNewSample(voiceChatPacket);
			}
			break;
		}
		case P2PMsgType.KickFromGame:
		{
			if (!fromPartyMember.IsPartyLeader)
			{
				break;
			}
			PartyMember partyMemberByID = GetPartyMemberByID(msg.PlayerID);
			if (partyMemberByID != null)
			{
				if (partyMemberByID.IsLocal)
				{
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg.Banned ? GameImpl.Translate("MENU_YouGotKicked") : GameImpl.Translate("MENU_PrivateGame"));
				}
				else
				{
					partyMemberByID.IsBanned = true;
					partyMemberByID.IsKickedOnJoin |= msg.KickedOnJoin;
				}
			}
			break;
		}
		case P2PMsgType.RequestTerrain:
			if (NetworkLoggingEnabled)
			{
				string[] obj5 = new string[6]
				{
					"Receiving RequestTerrain for session ",
					msg.SessionId.ToString(),
					", hash ",
					null,
					null,
					null
				};
				MD5Hash terrainHash = msg.TerrainHash;
				obj5[3] = terrainHash.ToString();
				obj5[4] = " from ";
				obj5[5] = fromPartyMember.GetPlayerName();
				Debug.Log(string.Concat(obj5));
			}
			SendTerrain(msg.SessionId, msg.TerrainHash, fromPartyMember);
			break;
		case P2PMsgType.SendTerrain:
		{
			if (NetworkLoggingEnabled)
			{
				string[] obj6 = new string[15]
				{
					"Receiving SendTerrain for session ",
					msg.SessionId.ToString(),
					", hash ",
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				MD5Hash terrainHash = msg.TerrainHash;
				obj6[3] = terrainHash.ToString();
				obj6[4] = " chunk ";
				obj6[5] = msg.ChunkIndex.ToString();
				obj6[6] = "/";
				obj6[7] = msg.ChunkCount.ToString();
				obj6[8] = " from ";
				obj6[9] = fromPartyMember.GetPlayerName();
				obj6[10] = ": ";
				obj6[11] = msg.BytesSize.ToString();
				obj6[12] = "/";
				obj6[13] = msg.TotalSize.ToString();
				obj6[14] = " bytes";
				Debug.Log(string.Concat(obj6));
			}
			if (msg.ChunkIndex == 0)
			{
				ReceivingTerrainHash = msg.TerrainHash;
				ReceivingTerrainBytes = new byte[msg.TotalSize];
				ReceivingTerrainByteIndex = 0;
			}
			else
			{
				if (ReceivingTerrainHash != msg.TerrainHash)
				{
					string[] obj7 = new string[5]
					{
						GameImpl.Translate("MENU_SyncError"),
						": Receiving chunk from unexpected terrain: ",
						null,
						null,
						null
					};
					MD5Hash terrainHash = msg.TerrainHash;
					obj7[2] = terrainHash.ToString();
					obj7[3] = ", expected ";
					terrainHash = ReceivingTerrainHash;
					obj7[4] = terrainHash.ToString();
					string msg7 = string.Concat(obj7);
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg7);
					break;
				}
				if (ReceivingTerrainChunkIndex != msg.ChunkIndex)
				{
					string msg8 = GameImpl.Translate("MENU_SyncError") + ": Receiving terrain chunks out of order: " + msg.ChunkIndex + ", expected " + ReceivingTerrainChunkIndex;
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(msg8);
					break;
				}
				if (ReceivingTerrainBytes == null)
				{
					LeaveLobby();
					ClearLobby();
					GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_SyncError") + ": Receiving unexpected terrain chunks");
					break;
				}
			}
			ReceivingTerrainChunkIndex = msg.ChunkIndex + 1;
			ReceivingTerrainChunkCount = msg.ChunkCount;
			int num3 = ReceivingTerrainByteIndex + msg.BytesSize;
			if (num3 > ReceivingTerrainBytes.Length)
			{
				string msg9 = GameImpl.Translate("MENU_SyncError") + ": Received more terrain bytes than expected (" + num3 + "/" + ReceivingTerrainBytes.Length + ")";
				LeaveLobby();
				ClearLobby();
				GameImpl.Instance.OnKicked(msg9);
				break;
			}
			Array.Copy(msg.Bytes, 0, ReceivingTerrainBytes, ReceivingTerrainByteIndex, msg.BytesSize);
			ReceivingTerrainByteIndex += msg.BytesSize;
			P2PMsg p2PMsg4 = new P2PMsg
			{
				MsgType = P2PMsgType.TerrainChunkAcknowledged,
				SessionId = msg.SessionId,
				TerrainHash = msg.TerrainHash,
				ChunkIndex = msg.ChunkIndex,
				ChunkCount = msg.ChunkCount
			};
			MsgWriter.ResetIndex();
			p2PMsg4.Reflect(MsgWriter);
			if (NetworkLoggingEnabled)
			{
				string[] obj8 = new string[9]
				{
					"Sending TerrainChunkAcknowledged for session ",
					msg.SessionId.ToString(),
					", hash ",
					null,
					null,
					null,
					null,
					null,
					null
				};
				MD5Hash terrainHash = msg.TerrainHash;
				obj8[3] = terrainHash.ToString();
				obj8[4] = " (";
				obj8[5] = msg.ChunkIndex.ToString();
				obj8[6] = "/";
				obj8[7] = msg.ChunkCount.ToString();
				obj8[8] = ") to all";
				Debug.Log(string.Concat(obj8));
			}
			foreach (PartyMember partyMember3 in PartyMembers)
			{
				if (partyMember3 != null && !partyMember3.IsLocal)
				{
					SendMessageTo(partyMember3);
				}
			}
			GameImpl.TakeOutTheTrash();
			if (msg.ChunkIndex != msg.ChunkCount - 1)
			{
				break;
			}
			bool isCompressed = true;
			if (!IsTerrainCacheCompressed())
			{
				using (new StopWatchMarker("Decompress"))
				{
					ReceivingTerrainBytes = Compression.Decompress(ReceivingTerrainBytes, 0, ReceivingTerrainBytes.Length);
					isCompressed = false;
				}
			}
			if (ReceivingTerrainBytes != null && SaveToTerrainCache(msg.TerrainHash, ReceivingTerrainBytes, ReceivingTerrainBytes.Length, isCompressed))
			{
				SessionToLoadTerrainReceived = true;
				GetLocalPartyMember().SetAcknowledgedTerrainHash(msg.SessionId, msg.TerrainHash);
			}
			else
			{
				LeaveLobby();
				ClearLobby();
				GameImpl.Instance.OnKicked(GameImpl.Translate("MENU_SyncError"));
			}
			break;
		}
		case P2PMsgType.TerrainChunkAcknowledged:
			if (NetworkLoggingEnabled)
			{
				string[] obj2 = new string[10]
				{
					"Receiving TerrainChunkAcknowledged for session ",
					msg.SessionId.ToString(),
					", hash ",
					null,
					null,
					null,
					null,
					null,
					null,
					null
				};
				MD5Hash terrainHash = msg.TerrainHash;
				obj2[3] = terrainHash.ToString();
				obj2[4] = "(";
				obj2[5] = msg.ChunkIndex.ToString();
				obj2[6] = "/";
				obj2[7] = msg.ChunkCount.ToString();
				obj2[8] = ") from ";
				obj2[9] = fromPartyMember.GetPlayerName();
				Debug.Log(string.Concat(obj2));
			}
			fromPartyMember.AcknowledgedTerrainChunkIndex = msg.ChunkIndex;
			fromPartyMember.TerrainChunkCount = msg.ChunkCount;
			if (fromPartyMember.AcknowledgedTerrainChunkIndex + 1 < fromPartyMember.TerrainChunkCount)
			{
				if (!SendAllChunksAtOnce && IsInMultiplayerGameAsLeader() && fromPartyMember.AcknowledgedTerrainChunkIndex + 1 < fromPartyMember.TerrainChunkMessagesToSend.Count)
				{
					SendTerrainChunk(fromPartyMember, fromPartyMember.AcknowledgedTerrainChunkIndex + 1);
				}
			}
			else
			{
				fromPartyMember.SetAcknowledgedTerrainHash(msg.SessionId, msg.TerrainHash);
			}
			break;
		case P2PMsgType.TerrainAcknowledged:
			if (NetworkLoggingEnabled)
			{
				string[] obj = new string[6]
				{
					"Receiving TerrainAcknowledged for session ",
					msg.SessionId.ToString(),
					", hash ",
					null,
					null,
					null
				};
				MD5Hash terrainHash = msg.TerrainHash;
				obj[3] = terrainHash.ToString();
				obj[4] = " from ";
				obj[5] = fromPartyMember.GetPlayerName();
				Debug.Log(string.Concat(obj));
			}
			fromPartyMember.SetAcknowledgedTerrainHash(msg.SessionId, msg.TerrainHash);
			break;
		case P2PMsgType.HelloNetwork:
			if (NetworkLoggingEnabled)
			{
				Debug.Log("Receiving HelloNetwork from " + fromPartyMember.GetPlayerName());
			}
			break;
		}
	}

	public static string GetTerrainCacheDir()
	{
		return GameImpl.Instance.SaveGamePath + "/TerrainCache";
	}

	public static string GetTerrainCacheFileName(MD5Hash hash)
	{
		return GetTerrainCacheDir() + "/" + hash.ToString() + ".map";
	}

	public static bool IsTerrainCached(MD5Hash hash)
	{
		try
		{
			return SaveGameManager.FileExists(GetTerrainCacheFileName(hash));
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
			return false;
		}
	}

	public void SetTerrainCacheLastUsedTime(MD5Hash hash, DateTime dateTime)
	{
		TerrainCacheLastUsedTime[hash] = dateTime;
		try
		{
			string filePath = GetTerrainCacheDir() + "/LastUsedTimes.txt";
			using MemoryStream memoryStream = new MemoryStream();
			using StreamWriter streamWriter = new StreamWriter(memoryStream);
			foreach (KeyValuePair<MD5Hash, DateTime> item in TerrainCacheLastUsedTime)
			{
				streamWriter.WriteLine(item.Key.ToString() + "\t" + item.Value);
			}
			SaveGameManager.FileWriteAllBytes(filePath, memoryStream.ToArray(), (int)memoryStream.Length);
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Error writing terrain cache last used time: " + ex.Message);
		}
	}

	public void LoadTerrainCacheLastUsedTimes()
	{
		try
		{
			string terrainCacheDir = GetTerrainCacheDir();
			string filePath = terrainCacheDir + "/LastUsedTimes.txt";
			if (SaveGameManager.FileExists(filePath))
			{
				using (MemoryStream stream = new MemoryStream(SaveGameManager.FileReadAllBytes(filePath)))
				{
					using StreamReader streamReader = new StreamReader(stream);
					while (!streamReader.EndOfStream)
					{
						string[] array = streamReader.ReadLine().Split('\t');
						if (array.Length >= 2)
						{
							MD5Hash key = new MD5Hash(array[0].Trim());
							DateTime value = StringUtil.ParseDateTime(array[1].Trim());
							if (key.IsValid())
							{
								TerrainCacheLastUsedTime[key] = value;
								if (TerrainCacheLastUsedTime.Count >= 10)
								{
									break;
								}
							}
						}
					}
					return;
				}
			}
			if (!SaveGameManager.DirectoryExists(terrainCacheDir))
			{
				return;
			}
			foreach (string item in new List<string>(SaveGameManager.DirectoryGetFiles(terrainCacheDir, ".map")))
			{
				MD5Hash key2 = new MD5Hash(Path.GetFileNameWithoutExtension(item));
				if (key2.IsValid())
				{
					TerrainCacheLastUsedTime[key2] = File.GetLastAccessTime(item);
					if (TerrainCacheLastUsedTime.Count >= 10)
					{
						break;
					}
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Error reading LastUsedTimes.txt: " + ex.Message);
		}
	}

	public bool SaveToTerrainCache(MD5Hash hash, byte[] bytes, int len, bool isCompressed)
	{
		if (IsTerrainCached(hash))
		{
			return true;
		}
		try
		{
			string terrainCacheDir = GetTerrainCacheDir();
			SaveGameManager.DirectoryCreate(terrainCacheDir);
			while (TerrainCacheLastUsedTime.Count > 9)
			{
				MD5Hash mD5Hash = default(MD5Hash);
				DateTime dateTime = DateTime.Now;
				foreach (KeyValuePair<MD5Hash, DateTime> item in TerrainCacheLastUsedTime)
				{
					if (mD5Hash.IsNull() || item.Value < dateTime)
					{
						mD5Hash = item.Key;
						dateTime = item.Value;
					}
				}
				if (!mD5Hash.IsValid())
				{
					break;
				}
				try
				{
					SaveGameManager.FileDelete(GetTerrainCacheFileName(mD5Hash));
				}
				catch (Exception ex)
				{
					Debug.LogWarning("Error deleting oldest terrain cache file: " + ex.Message);
				}
				TerrainCacheLastUsedTime.Remove(mD5Hash);
			}
			try
			{
				foreach (string item2 in SaveGameManager.DirectoryGetFiles(terrainCacheDir, ".map"))
				{
					MD5Hash key = new MD5Hash(Path.GetFileNameWithoutExtension(item2));
					if (!TerrainCacheLastUsedTime.ContainsKey(key))
					{
						SaveGameManager.FileDelete(item2);
					}
				}
			}
			catch (Exception ex2)
			{
				Debug.LogWarning("Error deleting old terrain cache file: " + ex2.Message);
			}
			string terrainCacheFileName = GetTerrainCacheFileName(hash);
			if (IsTerrainCacheCompressed() && !isCompressed)
			{
				byte[] array = Compression.Compress(bytes, 0, len);
				SaveGameManager.FileWriteAllBytes(terrainCacheFileName, array, array.Length);
			}
			else
			{
				SaveGameManager.FileWriteAllBytes(terrainCacheFileName, bytes, len);
			}
			SetTerrainCacheLastUsedTime(hash, DateTime.Now);
			return true;
		}
		catch (Exception ex3)
		{
			Debug.LogWarning(ex3.ToString());
		}
		return false;
	}

	public static void SaveSessionSnapshot(byte[] bytes, int size, int inputFrame, PartyMember fromPartyMember, string path)
	{
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			string path2 = path + "\\" + fromPartyMember.GetPlayerName() + " frame " + inputFrame + ".sav";
			if (File.Exists(path2))
			{
				File.Delete(path2);
			}
			using Stream stream = File.OpenWrite(path2);
			stream.Write(bytes, 0, size);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.ToString());
		}
	}
}
