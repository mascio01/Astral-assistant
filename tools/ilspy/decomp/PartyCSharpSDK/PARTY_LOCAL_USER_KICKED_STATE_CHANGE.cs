using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_LOCAL_USER_KICKED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	internal PARTY_LOCAL_USER_KICKED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_LOCAL_USER_KICKED_STATE_CHANGE localUserKicked = stateChange.localUserKicked;
		network = new PARTY_NETWORK_HANDLE(localUserKicked.network);
		localUser = new PARTY_LOCAL_USER_HANDLE(localUserKicked.localUser);
	}
}
