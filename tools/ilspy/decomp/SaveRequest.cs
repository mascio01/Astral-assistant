using System;

public class SaveRequest
{
	public enum State
	{
		Running,
		Finished,
		Failed
	}

	public SaveGame SaveGame;

	public SettingsData SettingsData;

	public SavedCharacter SavedCharacter;

	public OnSavedFunction OnSavedCharacterFunction;

	public string CharacterNameToDelete;

	public CustomBinaryWriter Writer;

	public byte[] ThumbnailBytes;

	public MD5Hash TerrainHash;

	public int TokenSaveCameFromCarrierId;

	public EquipmentPrototype TokenProto;

	public bool FromSuspend;

	public volatile State CurrentState;

	public Exception SaveResult;

	public string GetErrorMessage()
	{
		if (SaveResult != null)
		{
			return SaveResult.Message;
		}
		return string.Empty;
	}
}
