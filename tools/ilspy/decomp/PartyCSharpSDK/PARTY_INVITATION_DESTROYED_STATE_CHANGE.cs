using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_INVITATION_DESTROYED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_INVITATION_HANDLE invitation { get; }

	public PARTY_DESTROYED_REASON reason { get; }

	public uint errorDetail { get; }

	internal PARTY_INVITATION_DESTROYED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_INVITATION_DESTROYED_STATE_CHANGE invitationDestroyed = stateChange.invitationDestroyed;
		network = new PARTY_NETWORK_HANDLE(invitationDestroyed.network);
		invitation = new PARTY_INVITATION_HANDLE(invitationDestroyed.invitation);
		reason = invitationDestroyed.reason;
		errorDetail = invitationDestroyed.errorDetail;
	}
}
