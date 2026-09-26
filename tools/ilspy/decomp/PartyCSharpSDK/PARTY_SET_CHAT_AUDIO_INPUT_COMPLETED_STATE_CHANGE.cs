using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public PARTY_AUDIO_DEVICE_SELECTION_TYPE audioDeviceSelectionType { get; }

	public string audioDeviceSelectionContext { get; }

	public object asyncIdentifier { get; }

	internal PARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_SET_CHAT_AUDIO_INPUT_COMPLETED_STATE_CHANGE setChatAudioInputCompleted = stateChange.setChatAudioInputCompleted;
		result = setChatAudioInputCompleted.result;
		errorDetail = setChatAudioInputCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(setChatAudioInputCompleted.localChatControl);
		audioDeviceSelectionType = setChatAudioInputCompleted.audioDeviceSelectionType;
		audioDeviceSelectionContext = Converters.PtrToStringUTF8(setChatAudioInputCompleted.audioDeviceSelectionContext);
		asyncIdentifier = null;
		if (setChatAudioInputCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(setChatAudioInputCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
