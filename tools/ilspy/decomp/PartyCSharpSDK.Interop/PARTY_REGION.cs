namespace PartyCSharpSDK.Interop;

internal struct PARTY_REGION
{
	private unsafe fixed byte regionName[20];

	internal readonly uint roundTripLatencyInMilliseconds;

	internal unsafe string GetRegionName()
	{
		fixed (byte* bytePointer = regionName)
		{
			return Converters.BytePointerToString(bytePointer, 20);
		}
	}

	internal unsafe PARTY_REGION(PartyCSharpSDK.PARTY_REGION publicObject)
	{
		fixed (byte* bytePointer = regionName)
		{
			Converters.StringToNullTerminatedUTF8FixedPointer(publicObject.RegionName, bytePointer, 20);
		}
		roundTripLatencyInMilliseconds = publicObject.RoundTripLatencyInMilliseconds;
	}
}
