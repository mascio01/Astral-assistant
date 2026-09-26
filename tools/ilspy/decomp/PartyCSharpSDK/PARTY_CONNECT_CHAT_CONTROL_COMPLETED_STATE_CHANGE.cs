using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_NETWORK_HANDLE network { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public object asyncIdentifier { get; }

	internal PARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CONNECT_CHAT_CONTROL_COMPLETED_STATE_CHANGE connectChatControlCompleted = stateChange.connectChatControlCompleted;
		result = connectChatControlCompleted.result;
		errorDetail = connectChatControlCompleted.errorDetail;
		network = new PARTY_NETWORK_HANDLE(connectChatControlCompleted.network);
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(connectChatControlCompleted.localChatControl);
		asyncIdentifier = null;
		if (connectChatControlCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(connectChatControlCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
