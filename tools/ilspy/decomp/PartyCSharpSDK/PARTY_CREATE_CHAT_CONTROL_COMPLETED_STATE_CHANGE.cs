using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_DEVICE_HANDLE localDevice { get; }

	public PARTY_LOCAL_USER_HANDLE localUser { get; }

	public string languageCode { get; }

	public object asyncIdentifier { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	internal PARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CREATE_CHAT_CONTROL_COMPLETED_STATE_CHANGE createChatControlCompleted = stateChange.createChatControlCompleted;
		result = createChatControlCompleted.result;
		errorDetail = createChatControlCompleted.errorDetail;
		localDevice = new PARTY_DEVICE_HANDLE(createChatControlCompleted.localDevice);
		localUser = new PARTY_LOCAL_USER_HANDLE(createChatControlCompleted.localUser);
		languageCode = Converters.PtrToStringUTF8(createChatControlCompleted.languageCode);
		asyncIdentifier = null;
		if (createChatControlCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(createChatControlCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(createChatControlCompleted.localChatControl);
	}
}
