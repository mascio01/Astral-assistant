using System;
using System.Runtime.InteropServices;

namespace PartyCSharpSDK.Interop;

internal struct PARTY_NETWORK_DESCRIPTOR
{
	private unsafe fixed byte networkIdentifier[37];

	private unsafe fixed byte regionName[20];

	private unsafe fixed byte opaqueConnectionInformation[301];

	internal unsafe string GetNetworkIdentifier()
	{
		fixed (byte* bytePointer = networkIdentifier)
		{
			return Converters.BytePointerToString(bytePointer, 37);
		}
	}

	internal unsafe string GetRegionName()
	{
		fixed (byte* bytePointer = regionName)
		{
			return Converters.BytePointerToString(bytePointer, 20);
		}
	}

	internal unsafe byte[] GetOpaqueConnectionInformation()
	{
		fixed (byte* ptr = opaqueConnectionInformation)
		{
			byte[] array = new byte[301];
			Marshal.Copy((IntPtr)ptr, array, 0, 301);
			return array;
		}
	}

	internal unsafe PARTY_NETWORK_DESCRIPTOR(PartyCSharpSDK.PARTY_NETWORK_DESCRIPTOR publicObject)
	{
		fixed (byte* bytePointer = networkIdentifier)
		{
			Converters.StringToNullTerminatedUTF8FixedPointer(publicObject.NetworkIdentifier, bytePointer, 37);
		}
		fixed (byte* bytePointer2 = regionName)
		{
			Converters.StringToNullTerminatedUTF8FixedPointer(publicObject.RegionName, bytePointer2, 20);
		}
		fixed (byte* ptr = opaqueConnectionInformation)
		{
			Marshal.Copy(publicObject.OpaqueConnectionInformation, 0, (IntPtr)ptr, 301);
		}
	}
}
