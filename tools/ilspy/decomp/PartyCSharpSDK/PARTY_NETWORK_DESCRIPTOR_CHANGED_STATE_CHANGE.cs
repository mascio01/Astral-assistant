using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	internal PARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_NETWORK_DESCRIPTOR_CHANGED_STATE_CHANGE networkDescriptorChanged = stateChange.networkDescriptorChanged;
		network = new PARTY_NETWORK_HANDLE(networkDescriptorChanged.network);
	}
}
