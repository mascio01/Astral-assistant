using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_REMOTE_DEVICE_DESTROYED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_DEVICE_HANDLE device { get; }

	internal PARTY_REMOTE_DEVICE_DESTROYED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_REMOTE_DEVICE_DESTROYED_STATE_CHANGE remoteDeviceDestroyed = stateChange.remoteDeviceDestroyed;
		device = new PARTY_DEVICE_HANDLE(remoteDeviceDestroyed.device);
	}
}
