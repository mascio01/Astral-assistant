using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public object asyncIdentifier { get; }

	public PARTY_ENDPOINT_HANDLE localEndpoint { get; }

	internal PARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CREATE_ENDPOINT_COMPLETED_STATE_CHANGE createEndpointCompleted = stateChange.createEndpointCompleted;
		result = createEndpointCompleted.result;
		errorDetail = createEndpointCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(createEndpointCompleted.network);
		localUser = new PARTY_LOCAL_USER_HANDLE(createEndpointCompleted.localUser);
		asyncIdentifier = null;
		if (createEndpointCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(createEndpointCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		localEndpoint = new PARTY_ENDPOINT_HANDLE(createEndpointCompleted.localEndpoint);
	}
}
