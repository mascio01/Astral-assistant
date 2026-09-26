using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public string languageCode { get; }

	public object asyncIdentifier { get; }

	internal PARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_SET_LANGUAGE_COMPLETED_STATE_CHANGE setLanguageCompleted = stateChange.setLanguageCompleted;
		result = setLanguageCompleted.result;
		errorDetail = setLanguageCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(setLanguageCompleted.localChatControl);
		languageCode = Converters.PtrToStringUTF8(setLanguageCompleted.languageCode);
		asyncIdentifier = null;
		if (setLanguageCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(setLanguageCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
