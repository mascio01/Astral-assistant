using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_ENDPOINT_CREATED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_ENDPOINT_HANDLE endpoint { get; }

	internal PARTY_ENDPOINT_CREATED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_ENDPOINT_CREATED_STATE_CHANGE endpointCreated = stateChange.endpointCreated;
		network = new PARTY_NETWORK_HANDLE(endpointCreated.network);
		endpoint = new PARTY_ENDPOINT_HANDLE(endpointCreated.endpoint);
	}
}
