using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

public class PartyMember
{
	public const int MaxInputFrames = 300;

	public const int CatchUpWhenFramesBehind = 6;

	public const int AddFewerInputsWhenSomeoneIsStrugglingToProcessFrames = 120;

	public PlayerID PlayerID;

	private string CachedPlayerName = "";

	public bool IsLocal;

	public bool IsPartyLeader;

	public bool IsBanned;

	public bool IsKickedOnJoin;

	public InputFrame[] InputFrames = new InputFrame[3000];

	public int ReceivedInputFrame;

	public int AcknowledgedSessionId;

	public int SentChunkIndex = -1;

	public int AcknowledgedChunkIndex = -1;

	public int SentTerrainChunkIndex = -1;

	public int AcknowledgedTerrainChunkIndex = -1;

	public int TerrainChunkCount;

	public List<P2PMsg> TerrainChunkMessagesToSend = new List<P2PMsg>();

	public MD5Hash AcknowledgedTerrainHash;

	public int AcknowledgedTerrainForSessionId;

	public bool FrameBufferWasFull;

	public InputFrameProcessingMode InputFrameProcessingMode;

	public bool ConnectionEstablished;

	public HSteamNetConnection Connection;

	public string PublicIP;

	public AudioSource VoiceSpeaker;

	public SortedList<int, VoiceChatPacket> PacketsToPlay = new SortedList<int, VoiceChatPacket>();

	public VoiceChatPacket CurrentPacket;

	public int VoiceDataPosition;

	public int CurrentPacketSampleIndex;

	public PartyMember()
	{
		for (int i = 0; i < InputFrames.Length; i++)
		{
			InputFrames[i] = new InputFrame();
		}
	}

	public void Connect()
	{
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworkingSocketsP2P:
			if (PlayerID.SteamID.m_SteamID < SteamUser.GetSteamID().m_SteamID)
			{
				SteamNetworkingConfigValue_t[] pOptions2 = new SteamNetworkingConfigValue_t[0];
				SteamNetworkingIdentity identityRemote = default(SteamNetworkingIdentity);
				identityRemote.SetSteamID(PlayerID.SteamID);
				Connection = SteamNetworkingSockets.ConnectP2P(ref identityRemote, 0, 0, pOptions2);
			}
			break;
		case NetworkProtocol.SteamNetworkingSocketsIP:
			if (PlayerID.SteamID.m_SteamID < SteamUser.GetSteamID().m_SteamID)
			{
				SteamNetworkingIPAddr pOutAddr;
				EResult remoteFakeIPForConnection = SteamNetworkingSockets.GetRemoteFakeIPForConnection(Connection, out pOutAddr);
				if (remoteFakeIPForConnection != EResult.k_EResultOK)
				{
					Debug.LogWarning("GetRemoteFakeIPForConnection error: " + remoteFakeIPForConnection.ToString() + " for " + GetPlayerName());
					break;
				}
				SteamNetworkingConfigValue_t[] pOptions = new SteamNetworkingConfigValue_t[0];
				Connection = SteamNetworkingSockets.ConnectByIPAddress(ref pOutAddr, 0, pOptions);
			}
			break;
		}
	}

	public void Disconnect()
	{
		if (!IsLocal && PlayerID.IsValid())
		{
			switch (GameImpl.Instance.Settings.NetworkProtocol)
			{
			case NetworkProtocol.SteamNetworking:
				SteamNetworking.CloseP2PSessionWithUser(PlayerID.SteamID);
				break;
			case NetworkProtocol.SteamNetworkingMessages:
			{
				SteamNetworkingIdentity identityRemote = default(SteamNetworkingIdentity);
				identityRemote.SetSteamID(PlayerID.SteamID);
				SteamNetworkingMessages.CloseSessionWithUser(ref identityRemote);
				break;
			}
			case NetworkProtocol.SteamNetworkingSocketsP2P:
			case NetworkProtocol.SteamNetworkingSocketsIP:
				SteamNetworkingSockets.CloseConnection(Connection, 0, "Disconnecting", bEnableLinger: false);
				Connection = default(HSteamNetConnection);
				break;
			}
		}
		if (VoiceSpeaker != null)
		{
			UnityEngine.Object.Destroy(VoiceSpeaker.clip);
			UnityEngine.Object.Destroy(VoiceSpeaker.gameObject);
			VoiceSpeaker = null;
		}
	}

	public bool IsBannedOrIgnored()
	{
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworkingMessages:
			if (!IsLocal && !ConnectionEstablished)
			{
				return true;
			}
			break;
		case NetworkProtocol.SteamNetworkingSocketsP2P:
		case NetworkProtocol.SteamNetworkingSocketsIP:
			if (!IsLocal && Connection == HSteamNetConnection.Invalid)
			{
				return true;
			}
			break;
		}
		return IsBanned;
	}

	public unsafe bool SendMessage(byte[] buf, int len)
	{
		if (len <= OnlineParty.Instance.ShortMsgBuffer.Length)
		{
			Array.Copy(buf, OnlineParty.Instance.ShortMsgBuffer, len);
			buf = OnlineParty.Instance.ShortMsgBuffer;
		}
		switch (GameImpl.Instance.Settings.NetworkProtocol)
		{
		case NetworkProtocol.SteamNetworking:
			SteamNetworking.SendP2PPacket(PlayerID.SteamID, buf, (uint)len, EP2PSend.k_EP2PSendReliable);
			break;
		case NetworkProtocol.SteamNetworkingMessages:
		{
			SteamNetworkingIdentity identityRemote = default(SteamNetworkingIdentity);
			identityRemote.SetSteamID(PlayerID.SteamID);
			fixed (byte* value2 = buf)
			{
				EResult eResult2 = SteamNetworkingMessages.SendMessageToUser(ref identityRemote, new IntPtr(value2), (uint)len, 8, 0);
				if (eResult2 != EResult.k_EResultOK)
				{
					Debug.LogWarning("SendMessageToUser failed: " + eResult2.ToString() + " (sending to " + GetPlayerName() + ")");
					return false;
				}
			}
			break;
		}
		case NetworkProtocol.SteamNetworkingSocketsP2P:
		case NetworkProtocol.SteamNetworkingSocketsIP:
			if (Connection != HSteamNetConnection.Invalid)
			{
				fixed (byte* value = buf)
				{
					long pOutMessageNumber;
					EResult eResult = SteamNetworkingSockets.SendMessageToConnection(Connection, new IntPtr(value), (uint)len, 8, out pOutMessageNumber);
					if (eResult != EResult.k_EResultOK)
					{
						Debug.LogWarning("SendMessageToConnection failed: " + eResult.ToString() + " (sending to " + GetPlayerName() + ")");
						return false;
					}
				}
				break;
			}
			Debug.LogWarning("Trying to send message to player without a connection: " + GetPlayerName());
			return false;
		}
		return true;
	}

	public string GetPlayerName()
	{
		if (CachedPlayerName.Length == 0)
		{
			CachedPlayerName = PlayerID.GetPlayerName();
		}
		return CachedPlayerName;
	}

	public void SetAcknowledgedSession(int sessionId)
	{
		AcknowledgedSessionId = sessionId;
		ReceivedInputFrame = -1;
		FrameBufferWasFull = false;
	}

	public void SetAcknowledgedTerrainHash(int sessionId, MD5Hash hash)
	{
		AcknowledgedTerrainForSessionId = sessionId;
		AcknowledgedTerrainHash = hash;
	}

	public void AddInputFrame(InputFrame inputFrame)
	{
		ReceivedInputFrame++;
		int num = ReceivedInputFrame % InputFrames.Length;
		InputFrames[num].CopyActions(inputFrame);
	}

	public InputFrame GetInputFrame(int inputFrame)
	{
		return InputFrames[inputFrame % InputFrames.Length];
	}

	public void CleanUp()
	{
		SentChunkIndex = -1;
		AcknowledgedChunkIndex = -1;
		SentTerrainChunkIndex = -1;
		AcknowledgedTerrainChunkIndex = -1;
		TerrainChunkCount = 0;
		TerrainChunkMessagesToSend.Clear();
	}

	public void StartVoice()
	{
		int lengthSamples = OnlineParty.VoiceSampleRate * 10;
		GameObject gameObject = new GameObject();
		gameObject.name = GetPlayerName() + " Voice";
		VoiceSpeaker = gameObject.AddComponent<AudioSource>();
		VoiceSpeaker.loop = true;
		VoiceSpeaker.clip = AudioClip.Create(GetPlayerName() + " Voice Clip", lengthSamples, 1, OnlineParty.VoiceSampleRate, stream: true, OnAudioRead, OnAudioSetPosition);
		VoiceSpeaker.spatialBlend = 0f;
		VoiceSpeaker.Play();
	}

	private void OnAudioRead(float[] data)
	{
		if (CurrentPacket == null)
		{
			CurrentPacket = NextPacket();
			CurrentPacketSampleIndex = 0;
			if (CurrentPacket != null)
			{
				PacketsToPlay.Remove(CurrentPacket.PacketId);
			}
		}
		for (int i = 0; i < data.Length; i++)
		{
			float num = 0f;
			if (CurrentPacket != null)
			{
				num = CurrentPacket.DecodedData[CurrentPacketSampleIndex];
				CurrentPacketSampleIndex++;
				if (CurrentPacketSampleIndex >= CurrentPacket.DecodedData.Length)
				{
					CurrentPacket = NextPacket();
					CurrentPacketSampleIndex = 0;
					if (CurrentPacket != null)
					{
						PacketsToPlay.Remove(CurrentPacket.PacketId);
					}
				}
			}
			data[i] = num;
			VoiceDataPosition++;
		}
	}

	private void OnAudioSetPosition(int newPosition)
	{
		VoiceDataPosition = newPosition;
	}

	private VoiceChatPacket NextPacket()
	{
		if (PacketsToPlay.Count > 0)
		{
			VoiceChatPacket voiceChatPacket = PacketsToPlay.Values[0];
			if (voiceChatPacket != null)
			{
				return voiceChatPacket;
			}
		}
		return null;
	}

	public void OnNewSample(VoiceChatPacket newPacket)
	{
		if (PacketsToPlay.ContainsKey(newPacket.PacketId))
		{
			Debug.LogWarning("already have packet " + newPacket.PacketId + ". abort");
			return;
		}
		newPacket.Decode();
		PacketsToPlay.Add(newPacket.PacketId, newPacket);
	}
}
