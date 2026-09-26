using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public object asyncIdentifier { get; }

	internal PARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_DESTROY_LOCAL_USER_COMPLETED_STATE_CHANGE destroyLocalUserCompleted = stateChange.destroyLocalUserCompleted;
		result = destroyLocalUserCompleted.result;
		errorDetail = destroyLocalUserCompleted.errorDetail;
		localUser = new PARTY_LOCAL_USER_HANDLE(destroyLocalUserCompleted.localUser);
		asyncIdentifier = null;
		if (destroyLocalUserCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(destroyLocalUserCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
