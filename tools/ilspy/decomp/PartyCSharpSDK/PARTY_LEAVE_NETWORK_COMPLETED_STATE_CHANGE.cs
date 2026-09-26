using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public object asyncIdentifier { get; }

	internal PARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_LEAVE_NETWORK_COMPLETED_STATE_CHANGE leaveNetworkCompleted = stateChange.leaveNetworkCompleted;
		result = leaveNetworkCompleted.result;
		errorDetail = leaveNetworkCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(leaveNetworkCompleted.network);
		asyncIdentifier = null;
		if (leaveNetworkCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(leaveNetworkCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
