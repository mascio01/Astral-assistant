using PartyCSharpSDK;
using PartyXBLCSharpSDK.Interop;

namespace PartyXBLCSharpSDK;

public class PARTY_XBL_CHAT_USER_HANDLE
{
	internal PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE InteropHandle { get; set; }

	internal PARTY_XBL_CHAT_USER_HANDLE(PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE interopHandle)
	{
		InteropHandle = interopHandle;
	}

	internal static uint WrapAndReturnError(uint error, PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE interopHandle, out PARTY_XBL_CHAT_USER_HANDLE handle)
	{
		if (PartyError.SUCCEEDED(error))
		{
			handle = new PARTY_XBL_CHAT_USER_HANDLE(interopHandle);
		}
		else
		{
			handle = null;
		}
		return error;
	}

	internal void ClearInteropHandle()
	{
		InteropHandle = default(PartyXBLCSharpSDK.Interop.PARTY_XBL_CHAT_USER_HANDLE);
	}
}
