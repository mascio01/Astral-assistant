using System;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_LOCAL_CHAT_USER_DESTROYED_STATE_CHANGE : PARTY_XBL_STATE_CHANGE
{
	public PARTY_XBL_CHAT_USER_HANDLE localChatUser { get; }

	public PARTY_XBL_LOCAL_CHAT_USER_DESTROYED_REASON reason { get; }

	public uint errorDetail { get; }

	internal PARTY_XBL_LOCAL_CHAT_USER_DESTROYED_STATE_CHANGE(PARTY_XBL_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyXBLCSharpSDK.Interop.PARTY_XBL_LOCAL_CHAT_USER_DESTROYED_STATE_CHANGE localChatUserDestroyed = stateChange.localChatUserDestroyed;
		localChatUser = new PARTY_XBL_CHAT_USER_HANDLE(localChatUserDestroyed.localChatUser);
		reason = localChatUserDestroyed.reason;
		errorDetail = localChatUserDestroyed.errorDetail;
	}
}
