using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public PARTY_INVITATION_HANDLE invitation { get; }

	public object asyncIdentifier { get; }

	internal PARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_REVOKE_INVITATION_COMPLETED_STATE_CHANGE revokeInvitationCompleted = stateChange.revokeInvitationCompleted;
		result = revokeInvitationCompleted.result;
		errorDetail = revokeInvitationCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(revokeInvitationCompleted.network);
		localUser = new PARTY_LOCAL_USER_HANDLE(revokeInvitationCompleted.localUser);
		invitation = new PARTY_INVITATION_HANDLE(revokeInvitationCompleted.invitation);
		asyncIdentifier = null;
		if (revokeInvitationCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(revokeInvitationCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
