using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_DESCRIPTOR networkDescriptor { get; }

	public object asyncIdentifier { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	internal PARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CONNECT_TO_NETWORK_COMPLETED_STATE_CHANGE connectToNetworkCompleted = stateChange.connectToNetworkCompleted;
		result = connectToNetworkCompleted.result;
		errorDetail = connectToNetworkCompleted.errorDetail;
		networkDescriptor = new PARTY_NETWORK_DESCRIPTOR(connectToNetworkCompleted.networkDescriptor);
		asyncIdentifier = null;
		if (connectToNetworkCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(connectToNetworkCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		network = new PARTY_NETWORK_HANDLE(connectToNetworkCompleted.network);
	}
}
