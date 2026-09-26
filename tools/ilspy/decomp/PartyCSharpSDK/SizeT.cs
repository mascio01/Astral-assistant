using System;

namespace PartyCSharpSDK;

public struct SizeT
{
	private readonly UIntPtr value;

	public bool IsZero => value == UIntPtr.Zero;

	public SizeT(int length)
	{
		value = new UIntPtr(Convert.ToUInt64(length));
	}

	public SizeT(uint length)
	{
		value = new UIntPtr(Convert.ToUInt64(length));
	}

	public uint ToUInt32()
	{
		return Convert.ToUInt32(value.ToUInt64());
	}

	public int ToInt32()
	{
		return Convert.ToInt32(value.ToUInt64());
	}
}
