using System;
using System.Collections.Generic;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_STATE_CHANGE_RESULT result { get; }

	public uint errorDetail { get; }

	public PARTY_CHAT_CONTROL_HANDLE senderChatControl { get; }

	public List<PARTY_CHAT_CONTROL_HANDLE> receiverChatControls { get; }

	public PARTY_AUDIO_SOURCE_TYPE sourceType { get; }

	public string languageCode { get; }

	public string transcription { get; }

	public PARTY_VOICE_CHAT_TRANSCRIPTION_PHRASE_TYPE type { get; }

	public List<PARTY_TRANSLATION> translations { get; }

	internal PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_VOICE_CHAT_TRANSCRIPTION_RECEIVED_STATE_CHANGE voiceChatTranscriptionReceived = stateChange.voiceChatTranscriptionReceived;
		useObjectPool = true;
		result = voiceChatTranscriptionReceived.result;
		errorDetail = voiceChatTranscriptionReceived.errorDetail;
		senderChatControl = SDK.objectPool.Retrieve<PARTY_CHAT_CONTROL_HANDLE>(voiceChatTranscriptionReceived.senderChatControl);
		receiverChatControls = Converters.PtrToClassListFromPool<PARTY_CHAT_CONTROL_HANDLE, PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE>(voiceChatTranscriptionReceived.receiverChatControls, voiceChatTranscriptionReceived.receiverChatControlCount, SDK.objectPool);
		sourceType = voiceChatTranscriptionReceived.sourceType;
		languageCode = Converters.PtrToStringUTF8(voiceChatTranscriptionReceived.languageCode);
		transcription = Converters.PtrToStringUTF8(voiceChatTranscriptionReceived.transcription);
		type = voiceChatTranscriptionReceived.type;
		translations = Converters.PtrToClassListFromPool<PARTY_TRANSLATION, PartyCSharpSDK.Interop.PARTY_TRANSLATION>(voiceChatTranscriptionReceived.translations, voiceChatTranscriptionReceived.translationCount, SDK.objectPool);
	}

	internal override void Cleanup()
	{
		SDK.objectPool.Return(senderChatControl);
		foreach (PARTY_CHAT_CONTROL_HANDLE receiverChatControl in receiverChatControls)
		{
			SDK.objectPool.Return(receiverChatControl);
		}
		foreach (PARTY_TRANSLATION translation in translations)
		{
			SDK.objectPool.Return(translation);
		}
		receiverChatControls.Clear();
		SDK.objectPool.Return(receiverChatControls);
		translations.Clear();
		SDK.objectPool.Return(translations);
		base.Cleanup();
	}
}
