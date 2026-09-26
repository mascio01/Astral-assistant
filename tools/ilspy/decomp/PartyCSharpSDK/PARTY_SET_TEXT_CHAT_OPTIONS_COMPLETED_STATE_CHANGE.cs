using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public PARTY_TEXT_CHAT_OPTIONS options { get; }

	public object asyncIdentifier { get; }

	internal PARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_SET_TEXT_CHAT_OPTIONS_COMPLETED_STATE_CHANGE setTextChatOptionsCompleted = stateChange.setTextChatOptionsCompleted;
		result = setTextChatOptionsCompleted.result;
		errorDetail = setTextChatOptionsCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(setTextChatOptionsCompleted.localChatControl);
		options = setTextChatOptionsCompleted.options;
		asyncIdentifier = null;
		if (setTextChatOptionsCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(setTextChatOptionsCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
