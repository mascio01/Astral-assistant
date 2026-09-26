using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_REMOTE_DEVICE_CREATED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_DEVICE_HANDLE device { get; }

	internal PARTY_REMOTE_DEVICE_CREATED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_REMOTE_DEVICE_CREATED_STATE_CHANGE remoteDeviceCreated = stateChange.remoteDeviceCreated;
		device = new PARTY_DEVICE_HANDLE(remoteDeviceCreated.device);
	}
}
