using System;
using System.Runtime.InteropServices;
using PartyCSharpSDK;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_TOKEN_AND_SIGNATURE_REQUESTED_STATE_CHANGE : PARTY_XBL_STATE_CHANGE
{
	public uint correlationId { get; }

	public string method { get; }

	public string url { get; }

	public PARTY_XBL_HTTP_HEADER[] headers { get; }

	public byte[] body { get; }

	public bool forceRefresh { get; }

	public bool allUsers { get; }

	public PARTY_XBL_CHAT_USER_HANDLE localChatUser { get; }

	internal PARTY_XBL_TOKEN_AND_SIGNATURE_REQUESTED_STATE_CHANGE(PARTY_XBL_STATE_CHANGE_UNION stateChange, IntPtr StateChangeId)
		: base(stateChange.stateChange.stateChangeType, StateChangeId)
	{
		PartyXBLCSharpSDK.Interop.PARTY_XBL_TOKEN_AND_SIGNATURE_REQUESTED_STATE_CHANGE tokenAndSignatureRequested = stateChange.tokenAndSignatureRequested;
		correlationId = tokenAndSignatureRequested.correlationId;
		method = Converters.PtrToStringUTF8(tokenAndSignatureRequested.method);
		url = Converters.PtrToStringUTF8(tokenAndSignatureRequested.url);
		headers = Converters.PtrToClassArray(tokenAndSignatureRequested.headers, tokenAndSignatureRequested.headerCount, (PartyXBLCSharpSDK.Interop.PARTY_XBL_HTTP_HEADER x) => new PARTY_XBL_HTTP_HEADER(x));
		body = new byte[tokenAndSignatureRequested.bodySize];
		if (tokenAndSignatureRequested.bodySize != 0)
		{
			Marshal.Copy(tokenAndSignatureRequested.body, body, 0, (int)tokenAndSignatureRequested.bodySize);
		}
		forceRefresh = Convert.ToBoolean(tokenAndSignatureRequested.forceRefresh);
		allUsers = Convert.ToBoolean(tokenAndSignatureRequested.allUsers);
		localChatUser = new PARTY_XBL_CHAT_USER_HANDLE(tokenAndSignatureRequested.localChatUser);
	}
}
