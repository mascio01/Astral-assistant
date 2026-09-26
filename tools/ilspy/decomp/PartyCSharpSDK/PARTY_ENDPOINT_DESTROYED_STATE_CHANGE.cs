using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_ENDPOINT_DESTROYED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_ENDPOINT_HANDLE endpoint { get; }

	public PARTY_DESTROYED_REASON reason { get; }

	public uint errorDetail { get; }

	internal PARTY_ENDPOINT_DESTROYED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_ENDPOINT_DESTROYED_STATE_CHANGE endpointDestroyed = stateChange.endpointDestroyed;
		network = new PARTY_NETWORK_HANDLE(endpointDestroyed.network);
		endpoint = new PARTY_ENDPOINT_HANDLE(endpointDestroyed.endpoint);
		reason = endpointDestroyed.reason;
		errorDetail = endpointDestroyed.errorDetail;
	}
}
