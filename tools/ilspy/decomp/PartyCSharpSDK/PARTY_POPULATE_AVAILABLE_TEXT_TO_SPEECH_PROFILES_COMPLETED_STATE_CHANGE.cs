using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE localChatControl { get; }

	public object asyncIdentifier { get; }

	internal PARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_POPULATE_AVAILABLE_TEXT_TO_SPEECH_PROFILES_COMPLETED_STATE_CHANGE populateAvailableTextToSpeechProfilesCompleted = stateChange.populateAvailableTextToSpeechProfilesCompleted;
		result = populateAvailableTextToSpeechProfilesCompleted.result;
		errorDetail = populateAvailableTextToSpeechProfilesCompleted.errorDetail;
		localChatControl = new PARTY_CHAT_CONTROL_HANDLE(populateAvailableTextToSpeechProfilesCompleted.localChatControl);
		asyncIdentifier = null;
		if (populateAvailableTextToSpeechProfilesCompleted.asyncIdentifier != IntPtr.Zero)
		{
			GCHandle gCHandle = GCHandle.FromIntPtr(populateAvailableTextToSpeechProfilesCompleted.asyncIdentifier);
			asyncIdentifier = gCHandle.Target;
			gCHandle.Free();
		}
	}
}
