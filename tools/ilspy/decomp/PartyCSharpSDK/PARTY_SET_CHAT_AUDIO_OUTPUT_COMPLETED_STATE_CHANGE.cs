using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType { get; }

	public string audioDeviceSelectionContext { get; }

	public object asyncIdentifier { get; }

	internal PARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_SET_CHAT_AUDIO_OUTPUT_COMPLETED_STATE_CHANGE setChatAudioOutputCompleted = stateChange.setChatAudioOutputCompleted;
		result = setChatAudioOutputCompleted.result;
		errorDetail = setChatAudioOutputCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(setChatAudioOutputCompleted.localChatControl);
		audioDeviceSelectionType = setChatAudioOutputCompleted.audioDeviceSelectionType;
		audioDeviceSelectionContext = Converters.PtrToStringUTF8(setChatAudioOutputCompleted.audioDeviceSelectionContext);
		asyncIdentifier = null;
		if (setChatAudioOutputCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(setChatAudioOutputCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
