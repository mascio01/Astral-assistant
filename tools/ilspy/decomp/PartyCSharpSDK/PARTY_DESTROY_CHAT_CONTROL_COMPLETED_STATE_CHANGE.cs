using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_DEVICE_HANDLE localDevice { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public object asyncIdentifier { get; }

	internal PARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_DESTROY_CHAT_CONTROL_COMPLETED_STATE_CHANGE destroyChatControlCompleted = stateChange.destroyChatControlCompleted;
		result = destroyChatControlCompleted.result;
		errorDetail = destroyChatControlCompleted.errorDetail;
		localDevice = new PARTY_DEVICE_HANDLE(destroyChatControlCompleted.localDevice);
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(destroyChatControlCompleted.localChatControl);
		asyncIdentifier = null;
		if (destroyChatControlCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(destroyChatControlCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
