using System.Collections.Generic;

public struct P2PMsg : IReflectable
{
	public P2PMsgType MsgType;

	public int SessionId;

	public List<StoryId> Stories;

	public int BytesSize;

	public byte[] Bytes;

	public int ChunkIndex;

	public int ChunkCount;

	public int TotalSize;

	public InputFrame InputFrame;

	public List<PlayerID> PlayerIDs;

	public List<string> PlayerNames;

	public int SnapshotInputFrame;

	public MD5Hash SnapshotHash;

	public PlayerID PlayerID;

	public bool KickedOnJoin;

	public bool Banned;

	public int Version;

	public int PacketIndex;

	public bool IsFixedTerrain;

	public MD5Hash TerrainHash;

	public void Reflect(Reflector reflector)
	{
		reflector.Add(ref MsgType);
		switch (MsgType)
		{
		case P2PMsgType.SyncGame:
			reflector.Add(ref SessionId);
			reflector.Add(ref Version);
			CustomBinaryReaderFromMemory.IsSerialisingStoryIds = true;
			reflector.Add(ref Stories);
			CustomBinaryReaderFromMemory.IsSerialisingStoryIds = false;
			reflector.Add(ref Bytes, ref BytesSize);
			reflector.Add(ref ChunkIndex);
			reflector.Add(ref ChunkCount);
			reflector.Add(ref TotalSize);
			TerrainHash.Reflect(reflector);
			break;
		case P2PMsgType.SyncGameAcknowledged:
			reflector.Add(ref SessionId);
			reflector.Add(ref Version);
			break;
		case P2PMsgType.ChunkAcknowledged:
			reflector.Add(ref SessionId);
			reflector.Add(ref ChunkIndex);
			reflector.Add(ref ChunkCount);
			break;
		case P2PMsgType.InputFrame:
			reflector.Add(InputFrame);
			break;
		case P2PMsgType.SnapshotHash:
			reflector.Add(ref SessionId);
			reflector.Add(ref SnapshotInputFrame);
			SnapshotHash.Reflect(reflector);
			break;
		case P2PMsgType.SnapshotMatched:
		case P2PMsgType.SnapshotDidNotMatch:
			reflector.Add(ref SessionId);
			reflector.Add(ref SnapshotInputFrame);
			break;
		case P2PMsgType.SnapshotFull:
			reflector.Add(ref SessionId);
			reflector.Add(ref SnapshotInputFrame);
			reflector.Add(ref Bytes, ref BytesSize);
			reflector.Add(ref ChunkIndex);
			reflector.Add(ref ChunkCount);
			reflector.Add(ref TotalSize);
			break;
		case P2PMsgType.VoiceRecording:
			reflector.Add(ref PacketIndex);
			if (reflector.IsDeserialising)
			{
				Bytes = OnlineParty.Instance.VoiceBufferCompressed;
			}
			reflector.AddByteArray(ref Bytes, ref BytesSize);
			break;
		case P2PMsgType.KickFromGame:
			reflector.Add(ref PlayerID);
			reflector.Add(ref KickedOnJoin);
			reflector.Add(ref Banned);
			break;
		case P2PMsgType.RequestTerrain:
			reflector.Add(ref SessionId);
			TerrainHash.Reflect(reflector);
			break;
		case P2PMsgType.SendTerrain:
			reflector.Add(ref SessionId);
			TerrainHash.Reflect(reflector);
			reflector.Add(ref Bytes, ref BytesSize);
			reflector.Add(ref ChunkIndex);
			reflector.Add(ref ChunkCount);
			reflector.Add(ref TotalSize);
			break;
		case P2PMsgType.TerrainChunkAcknowledged:
			reflector.Add(ref SessionId);
			TerrainHash.Reflect(reflector);
			reflector.Add(ref ChunkIndex);
			reflector.Add(ref ChunkCount);
			break;
		case P2PMsgType.TerrainAcknowledged:
			reflector.Add(ref SessionId);
			TerrainHash.Reflect(reflector);
			break;
		case P2PMsgType.SessionReady:
			reflector.Add(ref SessionId);
			reflector.AddPlayerIDList(ref PlayerIDs);
			reflector.AddStringList(ref PlayerNames);
			break;
		case P2PMsgType.HelloNetwork:
			reflector.Add(ref PlayerID);
			break;
		}
	}
}
