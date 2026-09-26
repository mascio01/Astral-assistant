using System;

namespace PartyCSharpSDK.Interop;

internal struct PARTY_MUTABLE_DATA_BUFFER
{
	internal readonly IntPtr buffer;

	internal readonly uint bufferByteCount;

	internal PARTY_MUTABLE_DATA_BUFFER(PartyCSharpSDK.PARTY_MUTABLE_DATA_BUFFER publicObject)
	{
		buffer = publicObject.Buffer;
		bufferByteCount = publicObject.BufferByteCount;
	}
}
