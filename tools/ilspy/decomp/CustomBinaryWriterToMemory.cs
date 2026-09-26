using System;

public class CustomBinaryWriterToMemory : CustomBinaryWriter
{
	public CustomBinaryWriterToMemory()
	{
		_buffer = new byte[1048576];
	}

	public CustomBinaryWriterToMemory(int byteCount)
	{
		_buffer = new byte[byteCount];
	}

	public CustomBinaryWriterToMemory(byte[] buffer)
	{
		_buffer = buffer;
	}

	public void ResetIndex()
	{
		_index = 0;
	}

	public override void Flush()
	{
		Array.Resize(ref _buffer, _buffer.Length + 1048576);
	}

	public override void Add(ref byte[] bytes, ref int size)
	{
		Write(size);
		Write(bytes, size);
	}
}
