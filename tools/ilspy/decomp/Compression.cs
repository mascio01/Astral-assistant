using System;
using System.IO;

public static class Compression
{
	public static void CopyTo(this Stream src, Stream destination)
	{
		byte[] array = new byte[Math.Max(src.Length, destination.Length)];
		int count;
		while ((count = src.Read(array, 0, array.Length)) != 0)
		{
			destination.Write(array, 0, count);
		}
	}

	public static byte[] Compress(byte[] bytes, int offset, int length)
	{
		if (offset != 0 || length != bytes.Length)
		{
			byte[] array = new byte[length];
			Buffer.BlockCopy(bytes, offset, array, 0, length);
			bytes = array;
		}
		return CLZF2.Compress(bytes);
	}

	public static byte[] Decompress(byte[] bytes, int offset, int length)
	{
		if (offset != 0 || length != bytes.Length)
		{
			byte[] array = new byte[length];
			Buffer.BlockCopy(bytes, offset, array, 0, length);
			bytes = array;
		}
		return CLZF2.Decompress(bytes);
	}
}
