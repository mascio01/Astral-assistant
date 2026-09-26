using System;
using System.Runtime.InteropServices;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_CREATE_LOCAL_CHAT_USER_COMPLETED_STATE_CHANGE : PARTY_XBL_STATE_CHANGE
{
	public PARTY_XBL_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public object asyncIdentifier { get; }

	public PARTY_XBL_CHAT_USER_HANDLE localChatUser { get; }

	internal PARTY_XBL_CREATE_LOCAL_CHAT_USER_COMPLETED_STATE_CHANGE(PARTY_XBL_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyXBLCSharpSDK.Interop.PARTY_XBL_CREATE_LOCAL_CHAT_USER_COMPLETED_STATE_CHANGE createLocalChatUserCompleted = stateChange.createLocalChatUserCompleted;
		result = createLocalChatUserCompleted.result;
		errorDetail = createLocalChatUserCompleted.errorDetail;
		asyncIdentifier = null;
		if (createLocalChatUserCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(createLocalChatUserCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		localChatUser = new PARTY_XBL_CHAT_USER_HANDLE(createLocalChatUserCompleted.localChatUser);
	}
}
