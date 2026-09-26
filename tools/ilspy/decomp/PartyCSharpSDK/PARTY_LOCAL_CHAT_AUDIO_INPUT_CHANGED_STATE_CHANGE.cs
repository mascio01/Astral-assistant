using System;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_LOCAL_CHAT_AUDIO_INPUT_CHANGED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public PARTY_AUDIO_INPUT_STATE state { get; }

	public uint errorDetail { get; }

	internal PARTY_LOCAL_CHAT_AUDIO_INPUT_CHANGED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_LOCAL_CHAT_AUDIO_INPUT_CHANGED_STATE_CHANGE localChatAudioInputChanged = stateChange.localChatAudioInputChanged;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(localChatAudioInputChanged.localChatControl);
		state = localChatAudioInputChanged.state;
		errorDetail = localChatAudioInputChanged.errorDetail;
	}
}
