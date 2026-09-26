using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_KICK_DEVICE_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_DEVICE_HANDLE kickedDevice { get; }

	public object asyncIdentifier { get; }

	internal PARTY_KICK_DEVICE_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_KICK_DEVICE_COMPLETED_STATE_CHANGE kickDeviceCompleted = stateChange.kickDeviceCompleted;
		result = kickDeviceCompleted.result;
		errorDetail = kickDeviceCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(kickDeviceCompleted.network);
		kickedDevice = new PARTY_DEVICE_HANDLE(kickDeviceCompleted.kickedDevice);
		asyncIdentifier = null;
		if (kickDeviceCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(kickDeviceCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
