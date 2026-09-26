using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public object asyncIdentifier { get; }

	internal PARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_REMOVE_LOCAL_USER_COMPLETED_STATE_CHANGE removeLocalUserCompleted = stateChange.removeLocalUserCompleted;
		result = removeLocalUserCompleted.result;
		errorDetail = removeLocalUserCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(removeLocalUserCompleted.network);
		localUser = new PARTY_LOCAL_USER_HANDLE(removeLocalUserCompleted.localUser);
		asyncIdentifier = null;
		if (removeLocalUserCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(removeLocalUserCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
