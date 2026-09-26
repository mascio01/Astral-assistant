using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK.Interop;

namespace PartyCSharpSDK;

public class PARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE : PARTY_STATE_CHANGE
{
	public PARTY_CHAT_CONTROL_HANDLE senderChatControl { get; }

	public PARTY_CHAT_CONTROL_HANDLE[] receiverChatControls { get; }

	public string languageCode { get; }

	public string chatText { get; }

	public byte[] data { get; }

	public PARTY_TRANSLATION[] translations { get; }

	public PARTY_CHAT_TEXT_RECEIVED_OPTIONS options { get; }

	public string originalChatText { get; }

	public uint errorDetail { get; }

	internal PARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE(PARTY_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyCSharpSDK.Interop.PARTY_CHAT_TEXT_RECEIVED_STATE_CHANGE chatTextReceived = stateChange.chatTextReceived;
		senderChatControl = new PARTY_CHAT_CONTROL_HANDLE(chatTextReceived.senderChatControl);
		receiverChatControls = Converters.PtrToClassArray(chatTextReceived.receiverChatControls, chatTextReceived.receiverChatControlCount, (PartyCSharpSDK.Interop.PARTY_CHAT_CONTROL_HANDLE x) => new PARTY_CHAT_CONTROL_HANDLE(x));
		languageCode = Converters.PtrToStringUTF8(chatTextReceived.languageCode);
		chatText = Converters.PtrToStringUTF8(chatTextReceived.chatText);
		data = new byte[chatTextReceived.dataSize];
		if (chatTextReceived.dataSize != 0)
		{
			Marshal.Copy(chatTextReceived.data, data, 0, (int)chatTextReceived.dataSize);
		}
		translations = Converters.PtrToClassArray(chatTextReceived.translations, chatTextReceived.translationCount, (PartyCSharpSDK.Interop.PARTY_TRANSLATION x) => new PARTY_TRANSLATION(x));
		options = chatTextReceived.options;
		originalChatText = Converters.PtrToStringUTF8(chatTextReceived.originalChatText);
		errorDetail = chatTextReceived.errorDetail;
	}
}
