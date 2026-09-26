public struct SaveGameSlot
{
	public SaveGameType SaveGameType;

	public int GameUniqueId;

	public SaveGameSlot(SaveGameType saveGameType, int gameUniqueId)
	{
		SaveGameType = saveGameType;
		if ((uint)(saveGameType - 5) <= 2u)
		{
			GameUniqueId = gameUniqueId;
		}
		else
		{
			GameUniqueId = 0;
		}
	}
}
