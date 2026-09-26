using System;
using System.Runtime.InteropServices;

namespace PartyCSharpSDK.Interop;

internal struct PARTY_DATA_BUFFER
{
	internal readonly IntPtr buffer;

	internal readonly uint bufferByteCount;

	internal PARTY_DATA_BUFFER(byte[] publicObject, DisposableCollection disposableCollection)
	{
		bufferByteCount = checked((uint)publicObject.Length);
		if (bufferByteCount != 0)
		{
			buffer = disposableCollection.Add(new DisposableBuffer(publicObject.Length)).IntPtr;
			Marshal.Copy(publicObject, 0, buffer, publicObject.Length);
		}
		else
		{
			buffer = IntPtr.Zero;
		}
	}

	internal PARTY_DATA_BUFFER(IntPtr bufferPtr, uint bufferSize)
	{
		buffer = bufferPtr;
		bufferByteCount = bufferSize;
	}
}
