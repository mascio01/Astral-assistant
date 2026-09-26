using System;
using System.Collections.Generic;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_ENDPOINT_HANDLE senderEndpoint { get; }

	public List<PARTY_ENDPOINT_HANDLE> receiverEndpoints { get; }

	public PARTY_MESSAGE_RECEIVED_OPTIONS options { get; }

	public uint messageSize { get; }

	public IntPtr messageBuffer { get; }

	internal PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_ENDPOINT_MESSAGE_RECEIVED_STATE_CHANGE endpointMessageReceived = stateChange.endpointMessageReceived;
		useObjectPool = true;
		network = SDK.objectPool.Retrieve<PARTY_NETWORK_HANDLE>(endpointMessageReceived.network);
		senderEndpoint = SDK.objectPool.Retrieve<PARTY_ENDPOINT_HANDLE>(endpointMessageReceived.senderEndpoint);
		receiverEndpoints = Converters.PtrToClassListFromPool<PARTY_ENDPOINT_HANDLE, PartyCSharpSDK.Interop.PARTY_ENDPOINT_HANDLE>(endpointMessageReceived.receiverEndpoints, endpointMessageReceived.receiverEndpointCount, SDK.objectPool);
		options = endpointMessageReceived.options;
		messageSize = endpointMessageReceived.messageSize;
		messageBuffer = endpointMessageReceived.messageBuffer;
	}

	internal override void Cleanup()
	{
		SDK.objectPool.Return(network);
		SDK.objectPool.Return(senderEndpoint);
		foreach (PARTY_ENDPOINT_HANDLE receiverEndpoint in receiverEndpoints)
		{
			SDK.objectPool.Return(receiverEndpoint);
		}
		receiverEndpoints.Clear();
		SDK.objectPool.Return(receiverEndpoints);
		base.Cleanup();
	}
}
