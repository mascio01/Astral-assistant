using System.Collections.Generic;

public class InputsRecord : IReflectable
{
	public struct Frame : IReflectable
	{
		public List<InputAction> Actions;

		public Frame(List<InputAction> actions)
		{
			Actions = new List<InputAction>();
			actions.CopyToList(Actions);
		}

		public void Reflect(Reflector reflector)
		{
			reflector.Add(ref Actions);
		}
	}

	public struct Player : IReflectable
	{
		public PlayerID PlayerID;

		public List<Frame> Frames;

		public Player(PlayerID playerID)
		{
			PlayerID = playerID;
			Frames = new List<Frame>();
		}

		public void Reflect(Reflector reflector)
		{
			reflector.Add(ref PlayerID);
			reflector.Add(ref Frames);
		}
	}

	public byte[] InitialGameState;

	public int InitialGameStateLength;

	public List<Player> Players = new List<Player>();

	public InputsRecord()
	{
	}

	public InputsRecord(byte[] initialGameState, int len)
	{
		InitialGameState = initialGameState;
		InitialGameStateLength = len;
	}

	public void Reflect(Reflector reflector)
	{
		reflector.AddByteArray(ref InitialGameState, ref InitialGameStateLength);
		reflector.Add(ref Players);
	}
}
