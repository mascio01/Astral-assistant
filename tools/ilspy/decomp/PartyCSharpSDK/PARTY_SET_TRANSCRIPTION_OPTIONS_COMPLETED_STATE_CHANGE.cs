using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public PARTY_VOICE_CHAT_TRANSCRIPTION_OPTIONS options { get; }

	public object asyncIdentifier { get; }

	internal PARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_SET_TRANSCRIPTION_OPTIONS_COMPLETED_STATE_CHANGE setTranscriptionOptionsCompleted = stateChange.setTranscriptionOptionsCompleted;
		result = setTranscriptionOptionsCompleted.result;
		errorDetail = setTranscriptionOptionsCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(setTranscriptionOptionsCompleted.localChatControl);
		options = setTranscriptionOptionsCompleted.options;
		asyncIdentifier = null;
		if (setTranscriptionOptionsCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(setTranscriptionOptionsCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
