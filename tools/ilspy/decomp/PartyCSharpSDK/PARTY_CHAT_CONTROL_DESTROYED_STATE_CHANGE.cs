using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_CHAT_CONTROL_HANDLE chatControl { get; }

	public PARTY_DESTROYED_REASON reason { get; }

	public uint errorDetail { get; }

	internal PARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_DESTROYED_STATE_CHANGE chatControlDestroyed = stateChange.chatControlDestroyed;
		chatControl = new PARTY_CHAT_CONTROL_HANDLE(chatControlDestroyed.chatControl);
		reason = chatControlDestroyed.reason;
		errorDetail = chatControlDestroyed.errorDetail;
	}
}
