using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CHAT_CONTROL_CREATED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_CHAT_CONTROL_HANDLE chatControl { get; }

	internal PARTY_CHAT_CONTROL_CREATED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_CREATED_STATE_CHANGE chatControlCreated = stateChange.chatControlCreated;
		chatControl = new PARTY_CHAT_CONTROL_HANDLE(chatControlCreated.chatControl);
	}
}
