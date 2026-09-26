using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_DESTROYED_REASON reason { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_CHAT_CONTROL_HANDLE chatControl { get; }

	internal PARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_LEFT_NETWORK_STATE_CHANGE chatControlLeftNetwork = stateChange.chatControlLeftNetwork;
		reason = chatControlLeftNetwork.reason;
		errorDetail = chatControlLeftNetwork.errorDetail;
		network = new PARTY_NETWORK_HANDLE(chatControlLeftNetwork.network);
		chatControl = new PARTY_CHAT_CONTROL_HANDLE(chatControlLeftNetwork.chatControl);
	}
}
