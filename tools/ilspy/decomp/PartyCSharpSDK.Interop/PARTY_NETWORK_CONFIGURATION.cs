namespace PartyCSharpSDK.Interop;

internal struct PARTY_NETWORK_CONFIGURATION
{
	internal readonly uint maxUserCount;

	internal readonly uint maxDeviceCount;

	internal readonly uint maxUsersPerDeviceCount;

	internal readonly uint maxDevicesPerUserCount;

	internal readonly uint maxEndpointsPerDeviceCount;

	internal readonly PARTY_DIRECT_PEER_CONNECTIVITY_OPTIONS directPeerConnectivityOptions;

	internal PARTY_NETWORK_CONFIGURATION(PartyCSharpSDK.PARTY_NETWORK_CONFIGURATION publicObject)
	{
		maxUserCount = publicObject.MaxUserCount;
		maxDeviceCount = publicObject.MaxDeviceCount;
		maxUsersPerDeviceCount = publicObject.MaxUsersPerDeviceCount;
		maxDevicesPerUserCount = publicObject.MaxDevicesPerUserCount;
		maxEndpointsPerDeviceCount = publicObject.MaxEndpointsPerDeviceCount;
		directPeerConnectivityOptions = publicObject.DirectPeerConnectivityOptions;
	}
}
