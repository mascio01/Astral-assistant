using System.Collections.Generic;

public class SessionSnapshot
{
	public int InputFrame;

	public List<PlayerHash> Hashes;

	public CustomBinaryWriterToMemory Writer;

	public SessionSnapshot(int inputFrame)
	{
		InputFrame = inputFrame;
		Hashes = new List<PlayerHash>();
	}
}
