public struct PlayerHash
{
	public PlayerID PlayerID;

	public MD5Hash Hash;

	public PlayerHash(PlayerID playerID, MD5Hash hash)
	{
		PlayerID = playerID;
		Hash = hash;
	}
}
